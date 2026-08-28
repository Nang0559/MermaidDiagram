namespace MermaidDiagram.Core.Graph
{
    /// <summary>
    /// Graph trung gian được xây dựng từ MermaidDocument.
    ///
    /// MermaidGraph chỉ quản lý topology của graph:
    ///
    ///     - Node
    ///     - Directed Edge
    ///     - Adjacency
    ///     - Reverse Adjacency
    ///
    /// Không chứa:
    ///
    ///     - Root analysis
    ///     - Layer
    ///     - Main path
    ///     - Layout
    ///     - Spacing
    ///     - Collision
    ///     - Viewport
    ///     - Workflow business logic
    /// </summary>
    public sealed class MermaidGraph
    {
        // ============================================================
        // NODES
        // ============================================================

        /// <summary>
        /// Tập tất cả node key trong graph.
        /// </summary>
        public HashSet<string> NodeKeys
        {
            get;
        }

        // ============================================================
        // ADJACENCY
        // ============================================================

        /// <summary>
        /// Directed adjacency.
        ///
        /// A -> B
        ///
        /// Adjacency[A] = B
        /// </summary>
        public Dictionary<string, List<string>> Adjacency
        {
            get;
        }

        // ============================================================
        // REVERSE ADJACENCY
        // ============================================================

        /// <summary>
        /// Reverse directed adjacency.
        ///
        /// A -> B
        ///
        /// ReverseAdjacency[B] = A
        /// </summary>
        public Dictionary<string, List<string>> ReverseAdjacency
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidGraph()
        {
            NodeKeys =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            Adjacency =
                new Dictionary<string, List<string>>(
                    StringComparer.OrdinalIgnoreCase);

            ReverseAdjacency =
                new Dictionary<string, List<string>>(
                    StringComparer.OrdinalIgnoreCase);
        }

        // ============================================================
        // ADD NODE
        // ============================================================

        /// <summary>
        /// Thêm node vào graph.
        ///
        /// Nếu node đã tồn tại thì không làm gì.
        /// </summary>
        public void AddNode(
            string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            key =
                key.Trim();

            if (!NodeKeys.Add(key))
            {
                return;
            }

            Adjacency[key] =
                new List<string>();

            ReverseAdjacency[key] =
                new List<string>();
        }

        // ============================================================
        // ADD EDGE
        // ============================================================

        /// <summary>
        /// Thêm directed edge:
        ///
        ///     from -> to
        ///
        /// Nếu node chưa tồn tại thì tự động tạo node.
        /// Duplicate edge sẽ bị bỏ qua.
        /// </summary>
        public void AddEdge(
            string from,
            string to)
        {
            if (string.IsNullOrWhiteSpace(from) ||
                string.IsNullOrWhiteSpace(to))
            {
                return;
            }

            from =
                from.Trim();

            to =
                to.Trim();

            AddNode(from);
            AddNode(to);

            List<string> children =
                Adjacency[from];

            if (!ContainsIgnoreCase(
                    children,
                    to))
            {
                children.Add(to);
            }

            List<string> parents =
                ReverseAdjacency[to];

            if (!ContainsIgnoreCase(
                    parents,
                    from))
            {
                parents.Add(from);
            }
        }

        // ============================================================
        // CONTAINS NODE
        // ============================================================

        public bool ContainsNode(
            string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            return NodeKeys.Contains(
                key.Trim());
        }

        // ============================================================
        // CONTAINS EDGE
        // ============================================================

        public bool ContainsEdge(
            string from,
            string to)
        {
            if (string.IsNullOrWhiteSpace(from) ||
                string.IsNullOrWhiteSpace(to))
            {
                return false;
            }

            if (!Adjacency.TryGetValue(
                    from.Trim(),
                    out List<string>? children))
            {
                return false;
            }

            return ContainsIgnoreCase(
                children,
                to.Trim());
        }

        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Xóa toàn bộ graph topology.
        /// </summary>
        public void Clear()
        {
            NodeKeys.Clear();
            Adjacency.Clear();
            ReverseAdjacency.Clear();
        }

        // ============================================================
        // INTERNAL HELPER
        // ============================================================

        private static bool ContainsIgnoreCase(
            List<string> values,
            string value)
        {
            for (int i = 0;
                 i < values.Count;
                 i++)
            {
                if (string.Equals(
                        values[i],
                        value,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}