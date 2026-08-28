namespace MermaidDiagram.Core.Graph
{
    /// <summary>
    /// Phân tích cấu trúc MermaidGraph.
    ///
    /// Chịu trách nhiệm:
    ///     - Root nodes
    ///     - Terminal nodes
    ///     - Node / edge count
    ///     - Path checking
    ///     - Cycle detection
    ///     - Unreachable nodes
    ///
    /// Không chịu trách nhiệm:
    ///     - Layer
    ///     - Tính X/Y
    ///     - Layout
    ///     - Spacing
    ///     - Collision
    ///     - Viewport
    ///     - Workflow business logic
    /// </summary>
    public sealed class GraphAnalyzer
    {
        // ============================================================
        // ROOT NODES
        // ============================================================

        /// <summary>
        /// Tìm các node không có node cha.
        /// </summary>
        public HashSet<string> FindRootKeys(
            MermaidGraph graph)
        {
            ValidateGraph(graph);

            var result =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (string key in graph.NodeKeys)
            {
                if (!graph.ReverseAdjacency.TryGetValue(
                        key,
                        out List<string>? parents))
                {
                    result.Add(key);

                    continue;
                }

                if (parents == null ||
                    parents.Count == 0)
                {
                    result.Add(key);
                }
            }

            return result;
        }

        // ============================================================
        // TERMINAL NODES
        // ============================================================

        /// <summary>
        /// Tìm các node không có node con.
        /// </summary>
        public HashSet<string> FindTerminalKeys(
            MermaidGraph graph)
        {
            ValidateGraph(graph);

            var result =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (string key in graph.NodeKeys)
            {
                if (!graph.Adjacency.TryGetValue(
                        key,
                        out List<string>? children))
                {
                    result.Add(key);

                    continue;
                }

                if (children == null ||
                    children.Count == 0)
                {
                    result.Add(key);
                }
            }

            return result;
        }

        // ============================================================
        // NODE COUNT
        // ============================================================

        /// <summary>
        /// Số lượng node trong graph.
        /// </summary>
        public int CountNodes(
            MermaidGraph graph)
        {
            ValidateGraph(graph);

            return graph.NodeKeys.Count;
        }

        // ============================================================
        // EDGE COUNT
        // ============================================================

        /// <summary>
        /// Số lượng directed edge trong graph.
        /// </summary>
        public int CountEdges(
            MermaidGraph graph)
        {
            ValidateGraph(graph);

            int count = 0;

            foreach (List<string>? children
                     in graph.Adjacency.Values)
            {
                if (children == null)
                {
                    continue;
                }

                count +=
                    children.Count;
            }

            return count;
        }

        // ============================================================
        // HAS PATH
        // ============================================================

        /// <summary>
        /// Kiểm tra có đường đi từ source đến target hay không.
        ///
        /// Sử dụng BFS.
        /// Có visited để tránh infinite loop khi graph chứa cycle.
        /// </summary>
        public bool HasPath(
            MermaidGraph graph,
            string source,
            string target)
        {
            ValidateGraph(graph);

            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            source =
                source.Trim();

            target =
                target.Trim();

            if (!ContainsNode(
                    graph,
                    source) ||
                !ContainsNode(
                    graph,
                    target))
            {
                return false;
            }

            if (string.Equals(
                    source,
                    target,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var visited =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            var queue =
                new Queue<string>();

            visited.Add(source);

            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                string current =
                    queue.Dequeue();

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

                foreach (string next in children)
                {
                    if (string.IsNullOrWhiteSpace(next))
                    {
                        continue;
                    }

                    if (string.Equals(
                            next,
                            target,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (visited.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            return false;
        }

        // ============================================================
        // HAS CYCLE
        // ============================================================

        /// <summary>
        /// Kiểm tra graph có cycle hay không.
        ///
        /// Sử dụng DFS với 3 trạng thái:
        ///
        ///     0 = chưa duyệt
        ///     1 = đang duyệt
        ///     2 = đã hoàn tất
        ///
        /// Nếu gặp node đang ở trạng thái 1,
        /// graph có cycle.
        /// </summary>
        public bool HasCycle(
            MermaidGraph graph)
        {
            ValidateGraph(graph);

            var state =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (string key in graph.NodeKeys)
            {
                state[key] = 0;
            }

            foreach (string key in graph.NodeKeys)
            {
                if (state[key] != 0)
                {
                    continue;
                }

                if (HasCycleFrom(
                        graph,
                        key,
                        state))
                {
                    return true;
                }
            }

            return false;
        }

        // ============================================================
        // CYCLE DFS
        // ============================================================

        private bool HasCycleFrom(
            MermaidGraph graph,
            string current,
            Dictionary<string, int> state)
        {
            state[current] = 1;

            if (graph.Adjacency.TryGetValue(
                    current,
                    out List<string>? children) &&
                children != null)
            {
                foreach (string next in children)
                {
                    if (string.IsNullOrWhiteSpace(next))
                    {
                        continue;
                    }

                    if (!state.TryGetValue(
                            next,
                            out int nextState))
                    {
                        continue;
                    }

                    // ------------------------------------------------
                    // Back edge => cycle
                    // ------------------------------------------------

                    if (nextState == 1)
                    {
                        return true;
                    }

                    // ------------------------------------------------
                    // Chưa duyệt => DFS tiếp
                    // ------------------------------------------------

                    if (nextState == 0 &&
                        HasCycleFrom(
                            graph,
                            next,
                            state))
                    {
                        return true;
                    }
                }
            }

            state[current] = 2;

            return false;
        }

        // ============================================================
        // UNREACHABLE NODES
        // ============================================================

        /// <summary>
        /// Tìm các node không reachable từ bất kỳ root nào.
        ///
        /// Hữu ích để phát hiện:
        ///     - graph rời
        ///     - node mồ côi
        ///     - graph bất thường
        ///     - cycle không có root
        /// </summary>
        public HashSet<string> FindUnreachableNodes(
            MermaidGraph graph)
        {
            ValidateGraph(graph);

            HashSet<string> roots =
                FindRootKeys(graph);

            var reachable =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            var queue =
                new Queue<string>();

            foreach (string root in roots)
            {
                if (reachable.Add(root))
                {
                    queue.Enqueue(root);
                }
            }

            while (queue.Count > 0)
            {
                string current =
                    queue.Dequeue();

                if (!graph.Adjacency.TryGetValue(
                        current,
                        out List<string>? children))
                {
                    continue;
                }

                if (children == null)
                {
                    continue;
                }

                foreach (string child in children)
                {
                    if (string.IsNullOrWhiteSpace(child))
                    {
                        continue;
                    }

                    if (reachable.Add(child))
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            var result =
                new HashSet<string>(
                    graph.NodeKeys,
                    StringComparer.OrdinalIgnoreCase);

            result.ExceptWith(
                reachable);

            return result;
        }

        // ============================================================
        // CONTAINS NODE
        // ============================================================

        private static bool ContainsNode(
            MermaidGraph graph,
            string key)
        {
            return graph.NodeKeys.Contains(key);
        }

        // ============================================================
        // VALIDATE
        // ============================================================

        private static void ValidateGraph(
            MermaidGraph graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(
                    nameof(graph));
            }
        }
    }
}