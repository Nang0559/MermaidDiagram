namespace MermaidDiagram.Core.Graph
{
    /// <summary>
    /// Kết quả tính layer của MermaidGraph.
    ///
    /// Không thuộc MermaidGraph topology.
    ///
    /// Chỉ chứa:
    ///     - Node -> Layer
    ///     - Layer -> Nodes
    /// </summary>
    public sealed class MermaidGraphLayer
    {
        // ============================================================
        // NODE -> LAYER
        // ============================================================

        /// <summary>
        /// Mapping:
        ///
        ///     NodeId -> Layer
        ///
        /// Ví dụ:
        ///
        ///     A = 0
        ///     B = 1
        ///     C = 2
        /// </summary>
        public Dictionary<string, int> Layer
        {
            get;
        } =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

        // ============================================================
        // LAYER -> NODES
        // ============================================================

        /// <summary>
        /// Mapping:
        ///
        ///     Layer -> NodeIds
        ///
        /// Ví dụ:
        ///
        ///     0 -> A
        ///     1 -> B, C
        ///     2 -> D
        /// </summary>
        public Dictionary<int, List<string>> Layers
        {
            get;
        } =
            new Dictionary<int, List<string>>();

        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Xóa toàn bộ kết quả layer.
        /// </summary>
        public void Clear()
        {
            Layer.Clear();
            Layers.Clear();
        }
    }
}