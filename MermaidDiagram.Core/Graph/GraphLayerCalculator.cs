namespace MermaidDiagram.Core.Graph
{
    /// <summary>
    /// Tính layer cho MermaidGraph.
    ///
    /// Quy tắc:
    ///
    ///     Root = Layer 0
    ///
    ///     Node có parent:
    ///         Layer = max(parent layer) + 1
    ///
    /// Kết quả được trả về dưới dạng MermaidGraphLayer.
    ///
    /// Class này không sửa MermaidGraph.
    ///
    /// Không xử lý:
    ///     - X/Y
    ///     - Width/Height
    ///     - Spacing
    ///     - Collision
    ///     - Viewport
    ///     - DevExpress
    ///     - WinForms
    /// </summary>
    public sealed class GraphLayerCalculator
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Tính layer cho MermaidGraph.
        ///
        /// Graph phải là DAG.
        /// Nếu graph chứa cycle thì không thể tính
        /// layer theo quy tắc max(parent layer) + 1.
        /// </summary>
        public MermaidGraphLayer Calculate(
            MermaidGraph graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(
                    nameof(graph));
            }

            MermaidGraphLayer result =
                new MermaidGraphLayer();

            if (graph.NodeKeys.Count == 0)
            {
                return result;
            }

            // --------------------------------------------------------
            // Cycle không thể xử lý bằng DAG layering.
            // --------------------------------------------------------

            GraphAnalyzer analyzer =
                new GraphAnalyzer();

            if (analyzer.HasCycle(graph))
            {
                throw new InvalidOperationException(
                    "Không thể tính layer cho graph " +
                    "có cycle.");
            }

            // --------------------------------------------------------
            // Tìm root.
            // --------------------------------------------------------

            HashSet<string> roots =
                analyzer.FindRootKeys(graph);

            // --------------------------------------------------------
            // Graph không có root nhưng không có cycle
            // là trường hợp bất thường.
            // --------------------------------------------------------

            if (roots.Count == 0)
            {
                throw new InvalidOperationException(
                    "Graph không có root node.");
            }

            // --------------------------------------------------------
            // Topological layering.
            // --------------------------------------------------------

            CalculateLayers(
                graph,
                roots,
                result);

            // --------------------------------------------------------
            // Kiểm tra mọi node đều đã được layer.
            // --------------------------------------------------------

            ValidateAllNodesLayered(
                graph,
                result);

            // --------------------------------------------------------
            // Build:
            //
            //     Layer -> Nodes
            // --------------------------------------------------------

            BuildLayers(
                result);

            return result;
        }

        // ============================================================
        // CALCULATE
        // ============================================================

        /// <summary>
        /// Tính layer bằng topological traversal.
        ///
        /// Node chỉ được xử lý khi toàn bộ predecessor
        /// đã được xử lý.
        /// </summary>
        private static void CalculateLayers(
            MermaidGraph graph,
            HashSet<string> roots,
            MermaidGraphLayer result)
        {
            var remainingParents =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (string nodeId
                     in graph.NodeKeys)
            {
                if (!graph.ReverseAdjacency.TryGetValue(
                        nodeId,
                        out List<string>? parents))
                {
                    remainingParents[nodeId] = 0;
                    continue;
                }

                remainingParents[nodeId] =
                    parents?.Count ?? 0;
            }

            var queue =
                new Queue<string>();

            // --------------------------------------------------------
            // Root = layer 0
            // --------------------------------------------------------

            foreach (string root
                     in roots)
            {
                result.Layer[root] = 0;
                queue.Enqueue(root);
            }

            // --------------------------------------------------------
            // Topological traversal
            // --------------------------------------------------------

            while (queue.Count > 0)
            {
                string current =
                    queue.Dequeue();

                int currentLayer =
                    result.Layer[current];

                if (!graph.Adjacency.TryGetValue(
                        current,
                        out List<string>? children))
                {
                    continue;
                }

                if (children == null ||
                    children.Count == 0)
                {
                    continue;
                }

                foreach (string child
                         in children)
                {
                    if (string.IsNullOrWhiteSpace(child))
                    {
                        continue;
                    }

                    // ------------------------------------------------
                    // Layer của child phụ thuộc parent sâu nhất.
                    // ------------------------------------------------

                    int candidateLayer =
                        currentLayer + 1;

                    if (!result.Layer.TryGetValue(
                            child,
                            out int existingLayer))
                    {
                        result.Layer[child] =
                            candidateLayer;
                    }
                    else if (candidateLayer > existingLayer)
                    {
                        result.Layer[child] =
                            candidateLayer;
                    }

                    // ------------------------------------------------
                    // Một parent đã được xử lý.
                    // ------------------------------------------------

                    if (remainingParents.TryGetValue(
                            child,
                            out int count))
                    {
                        count--;

                        remainingParents[child] =
                            count;

                        // ------------------------------------------------
                        // Chỉ enqueue khi toàn bộ parent đã xử lý.
                        // ------------------------------------------------

                        if (count == 0)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Bảo đảm mọi node trong graph đều có layer.
        /// </summary>
        private static void ValidateAllNodesLayered(
            MermaidGraph graph,
            MermaidGraphLayer result)
        {
            foreach (string nodeId
                     in graph.NodeKeys)
            {
                if (!result.Layer.ContainsKey(
                        nodeId))
                {
                    throw new InvalidOperationException(
                        $"Không thể tính layer cho node " +
                        $"'{nodeId}'.");
                }
            }
        }

        // ============================================================
        // BUILD LAYERS
        // ============================================================

        /// <summary>
        /// Chuyển:
        ///
        ///     Node -> Layer
        ///
        /// thành:
        ///
        ///     Layer -> Nodes
        /// </summary>
        private static void BuildLayers(
            MermaidGraphLayer result)
        {
            result.Layers.Clear();

            foreach (KeyValuePair<string, int> pair
                     in result.Layer)
            {
                if (!result.Layers.TryGetValue(
                        pair.Value,
                        out List<string>? nodes))
                {
                    nodes =
                        new List<string>();

                    result.Layers[pair.Value] =
                        nodes;
                }

                nodes.Add(
                    pair.Key);
            }

            // --------------------------------------------------------
            // Deterministic ordering.
            // --------------------------------------------------------

            foreach (List<string> nodes
                     in result.Layers.Values)
            {
                nodes.Sort(
                    StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}