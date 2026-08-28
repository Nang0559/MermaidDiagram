namespace MermaidDiagram.Core.Graph
{
    /// <summary>
    /// Tìm Main Path của Mermaid graph.
    ///
    /// Main Path là đường đi dài nhất từ một root
    /// đến một terminal node.
    ///
    /// Class này chỉ chịu trách nhiệm:
    ///     - Tìm Main Path
    ///     - Tạo MainPathIndex
    ///
    /// Không xử lý:
    ///     - Parse Mermaid
    ///     - Root analysis
    ///     - Layer calculation
    ///     - Layout
    ///     - DevExpress
    ///     - WinForms
    /// </summary>
    public sealed class MainPathCalculator
    {
        // ============================================================
        // FIND MAIN PATH
        // ============================================================

        /// <summary>
        /// Tìm đường đi dài nhất từ các root.
        ///
        /// MermaidGraph:
        ///     topology
        ///
        /// MermaidGraphLayer:
        ///     layer information
        ///
        /// rootKeys:
        ///     các node bắt đầu.
        /// </summary>
        public List<string> Find(
            MermaidGraph graph,
            MermaidGraphLayer graphLayer,
            HashSet<string> rootKeys)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(
                    nameof(graph));
            }

            if (graphLayer == null)
            {
                throw new ArgumentNullException(
                    nameof(graphLayer));
            }

            if (rootKeys == null)
            {
                throw new ArgumentNullException(
                    nameof(rootKeys));
            }

            if (graph.NodeKeys.Count == 0)
            {
                return new List<string>();
            }

            if (graphLayer.Layer.Count == 0)
            {
                throw new InvalidOperationException(
                    "Graph layers have not been calculated.");
            }

            if (rootKeys.Count == 0)
            {
                return FindFromLowestLayer(
                    graph,
                    graphLayer);
            }

            var bestPaths =
                new Dictionary<string, List<string>>(
                    StringComparer.OrdinalIgnoreCase);

            // ========================================================
            // 1. KHỞI TẠO PATH TỪ ROOT
            // ========================================================

            foreach (string root
                     in rootKeys
                         .OrderBy(
                             x => x,
                             StringComparer.OrdinalIgnoreCase))
            {
                if (!graph.NodeKeys.Contains(root))
                {
                    continue;
                }

                bestPaths[root] =
                    new List<string>
                    {
                        root
                    };
            }

            // ========================================================
            // 2. DUYỆT THEO LAYER
            // ========================================================

            IEnumerable<string> orderedNodes =
                graphLayer.Layer
                    .OrderBy(
                        x => x.Value)
                    .ThenBy(
                        x => x.Key,
                        StringComparer.OrdinalIgnoreCase)
                    .Select(
                        x => x.Key);

            foreach (string key
                     in orderedNodes)
            {
                if (!bestPaths.TryGetValue(
                        key,
                        out List<string>? currentPath))
                {
                    continue;
                }

                if (!graph.Adjacency.TryGetValue(
                        key,
                        out List<string>? nextNodes))
                {
                    continue;
                }

                if (nextNodes == null ||
                    nextNodes.Count == 0)
                {
                    continue;
                }

                foreach (string next
                         in nextNodes
                             .Where(
                                 x => !string.IsNullOrWhiteSpace(x))
                             .OrderBy(
                                 x => x,
                                 StringComparer.OrdinalIgnoreCase))
                {
                    // ------------------------------------------------
                    // Không cho phép cycle trong MainPath.
                    // ------------------------------------------------

                    if (currentPath.Contains(
                            next,
                            StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var candidate =
                        new List<string>(
                            currentPath);

                    candidate.Add(
                        next);

                    // ------------------------------------------------
                    // Chưa có path.
                    // ------------------------------------------------

                    if (!bestPaths.TryGetValue(
                            next,
                            out List<string>? existingPath))
                    {
                        bestPaths[next] =
                            candidate;

                        continue;
                    }

                    // ------------------------------------------------
                    // Candidate dài hơn.
                    // ------------------------------------------------

                    if (candidate.Count >
                        existingPath.Count)
                    {
                        bestPaths[next] =
                            candidate;

                        continue;
                    }

                    // ------------------------------------------------
                    // Cùng độ dài -> deterministic tie-break.
                    // ------------------------------------------------

                    if (candidate.Count ==
                        existingPath.Count &&
                        IsBetterPath(
                            candidate,
                            existingPath))
                    {
                        bestPaths[next] =
                            candidate;
                    }
                }
            }

            // ========================================================
            // 3. CHỌN PATH DÀI NHẤT
            // ========================================================

            if (bestPaths.Count == 0)
            {
                return new List<string>();
            }

            KeyValuePair<string, List<string>> best =
                bestPaths
                    .OrderByDescending(
                        x => x.Value.Count)
                    .ThenBy(
                        x => x.Key,
                        StringComparer.OrdinalIgnoreCase)
                    .First();

            return new List<string>(
                best.Value);
        }

        // ============================================================
        // FALLBACK
        // ============================================================

        /// <summary>
        /// Fallback khi graph không có root.
        ///
        /// Chọn node ở layer thấp nhất làm điểm bắt đầu
        /// và đi theo nhánh sâu nhất.
        ///
        /// Đây chủ yếu là cơ chế an toàn cho graph bất thường.
        /// </summary>
        private List<string> FindFromLowestLayer(
            MermaidGraph graph,
            MermaidGraphLayer graphLayer)
        {
            if (graphLayer.Layer.Count == 0)
            {
                return new List<string>();
            }

            string start =
                graphLayer.Layer
                    .OrderBy(
                        x => x.Value)
                    .ThenBy(
                        x => x.Key,
                        StringComparer.OrdinalIgnoreCase)
                    .Select(
                        x => x.Key)
                    .First();

            var path =
                new List<string>
                {
                    start
                };

            var visited =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            visited.Add(
                start);

            string current =
                start;

            while (graph.Adjacency.TryGetValue(
                       current,
                       out List<string>? nextNodes))
            {
                if (nextNodes == null ||
                    nextNodes.Count == 0)
                {
                    break;
                }

                string? next =
                    nextNodes
                        .Where(
                            x => !string.IsNullOrWhiteSpace(x))
                        .Where(
                            x => !visited.Contains(x))
                        .OrderByDescending(
                            x => GetLayer(
                                graphLayer,
                                x))
                        .ThenBy(
                            x => x,
                            StringComparer.OrdinalIgnoreCase)
                        .FirstOrDefault();

                if (string.IsNullOrEmpty(next))
                {
                    break;
                }

                path.Add(
                    next);

                visited.Add(
                    next);

                current =
                    next;
            }

            return path;
        }

        // ============================================================
        // TIE BREAK
        // ============================================================

        /// <summary>
        /// So sánh hai path có cùng độ dài.
        ///
        /// Ưu tiên path có node key nhỏ hơn theo
        /// thứ tự alphabetic.
        ///
        /// Mục đích:
        ///     deterministic result.
        /// </summary>
        private static bool IsBetterPath(
            List<string> candidate,
            List<string> existing)
        {
            int count =
                Math.Min(
                    candidate.Count,
                    existing.Count);

            for (int i = 0;
                 i < count;
                 i++)
            {
                int comparison =
                    string.Compare(
                        candidate[i],
                        existing[i],
                        StringComparison.OrdinalIgnoreCase);

                if (comparison < 0)
                {
                    return true;
                }

                if (comparison > 0)
                {
                    return false;
                }
            }

            return candidate.Count >
                   existing.Count;
        }

        // ============================================================
        // LAYER
        // ============================================================

        private static int GetLayer(
            MermaidGraphLayer graphLayer,
            string key)
        {
            if (graphLayer.Layer.TryGetValue(
                    key,
                    out int layer))
            {
                return layer;
            }

            return 0;
        }

        // ============================================================
        // BUILD INDEX
        // ============================================================

        /// <summary>
        /// Tạo index cho MainPath.
        ///
        /// Ví dụ:
        ///
        ///     A -> B -> C -> D
        ///
        /// Kết quả:
        ///
        ///     A = 0
        ///     B = 1
        ///     C = 2
        ///     D = 3
        /// </summary>
        public Dictionary<string, int> BuildIndex(
            List<string> mainPath)
        {
            var result =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            if (mainPath == null ||
                mainPath.Count == 0)
            {
                return result;
            }

            for (int i = 0;
                 i < mainPath.Count;
                 i++)
            {
                string key =
                    mainPath[i];

                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                result[key] =
                    i;
            }

            return result;
        }
    }
}