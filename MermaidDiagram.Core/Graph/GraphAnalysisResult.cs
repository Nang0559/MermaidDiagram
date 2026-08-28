namespace MermaidDiagram.Core.Graph
{
    /// <summary>
    /// Kết quả phân tích MermaidGraph.
    ///
    /// Đây là model trung gian giữa Graph layer
    /// và Layout layer.
    ///
    /// Chứa:
    ///     - Graph topology
    ///     - Root
    ///     - Layer
    ///     - Main path
    ///
    /// Không chứa:
    ///     - X/Y
    ///     - Width/Height
    ///     - Spacing
    ///     - Collision
    ///     - Viewport
    ///     - DevExpress
    /// </summary>
    public sealed class GraphAnalysisResult
    {
        // ============================================================
        // GRAPH
        // ============================================================

        /// <summary>
        /// Graph topology đã được xây dựng.
        /// </summary>
        public MermaidGraph Graph
        {
            get;
        }

        // ============================================================
        // ROOT
        // ============================================================

        /// <summary>
        /// Các node không có predecessor.
        /// </summary>
        public HashSet<string> RootKeys
        {
            get;
        }

        // ============================================================
        // LAYER
        // ============================================================

        /// <summary>
        /// Mapping:
        ///
        ///     NodeId -> Layer
        /// </summary>
        public Dictionary<string, int> Layer
        {
            get;
        }

        /// <summary>
        /// Mapping:
        ///
        ///     Layer -> NodeIds
        /// </summary>
        public Dictionary<int, List<string>> Layers
        {
            get;
        }

        // ============================================================
        // MAIN PATH
        // ============================================================

        /// <summary>
        /// Main path của graph.
        ///
        /// Thứ tự từ node đầu đến node cuối.
        /// </summary>
        public List<string> MainPath
        {
            get;
        }

        /// <summary>
        /// Mapping:
        ///
        ///     NodeId -> vị trí trên MainPath.
        /// </summary>
        public Dictionary<string, int> MainPathIndex
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public GraphAnalysisResult(
            MermaidGraph graph,
            HashSet<string> rootKeys,
            Dictionary<string, int> layer,
            List<string> mainPath,
            Dictionary<string, int> mainPathIndex)
        {
            Graph =
                graph
                ?? throw new ArgumentNullException(
                    nameof(graph));

            RootKeys =
                rootKeys
                ?? throw new ArgumentNullException(
                    nameof(rootKeys));

            Layer =
                layer
                ?? throw new ArgumentNullException(
                    nameof(layer));

            MainPath =
                mainPath
                ?? throw new ArgumentNullException(
                    nameof(mainPath));

            MainPathIndex =
                mainPathIndex
                ?? throw new ArgumentNullException(
                    nameof(mainPathIndex));

            Layers =
                BuildLayers(
                    Layer);
        }

        // ============================================================
        // BUILD LAYERS
        // ============================================================

        /// <summary>
        /// Xây dựng Layer -> NodeIds từ NodeId -> Layer.
        ///
        /// Đây là projection của Layer,
        /// không phải dữ liệu độc lập.
        /// </summary>
        private static Dictionary<int, List<string>> BuildLayers(
            Dictionary<string, int> layer)
        {
            var result =
                new Dictionary<int, List<string>>();

            foreach (KeyValuePair<string, int> pair
                     in layer)
            {
                if (!result.TryGetValue(
                        pair.Value,
                        out List<string>? nodes))
                {
                    nodes =
                        new List<string>();

                    result[pair.Value] =
                        nodes;
                }

                nodes.Add(
                    pair.Key);
            }

            // --------------------------------------------------------
            // Deterministic ordering
            // --------------------------------------------------------

            foreach (List<string> nodes
                     in result.Values)
            {
                nodes.Sort(
                    StringComparer.OrdinalIgnoreCase);
            }

            return result;
        }
    }
}