namespace MermaidDiagram.Core.Models
{
    /// <summary>
    /// Một cạnh có hướng trong Mermaid graph.
    ///
    /// Chỉ chứa dữ liệu được parse từ Mermaid.
    /// Không chứa workflow/business logic.
    /// </summary>
    public sealed class MermaidEdge
    {
        // ============================================================
        // ENDPOINTS
        // ============================================================

        /// <summary>
        /// ID của node nguồn.
        /// </summary>
        public string Source
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// ID của node đích.
        /// </summary>
        public string Target
        {
            get;
            set;
        } = string.Empty;

        // ============================================================
        // DISPLAY
        // ============================================================

        /// <summary>
        /// Nhãn hiển thị trên cạnh.
        ///
        /// Ví dụ:
        ///     CanRework
        ///     ChiGiaoBu
        ///     TuChoiGiaoBu
        ///
        /// Có thể rỗng.
        /// </summary>
        public string Label
        {
            get;
            set;
        } = string.Empty;

        // ============================================================
        // STYLE
        // ============================================================

        /// <summary>
        /// Kiểu hiển thị của đường nối sau khi parse từ Mermaid.
        /// </summary>
        public MermaidEdgeStyle Style
        {
            get;
            set;
        } = MermaidEdgeStyle.Solid;

        // ============================================================
        // RAW MERMAID
        // ============================================================

        /// <summary>
        /// Operator Mermaid nguyên bản.
        ///
        /// Ví dụ:
        ///     -->
        ///     -.-> 
        ///     ==>
        ///     ---
        ///
        /// Chỉ dùng để bảo toàn syntax nguồn.
        /// Không dùng làm workflow/business rule.
        /// </summary>
        public string Operator
        {
            get;
            set;
        } = string.Empty;
    }
}