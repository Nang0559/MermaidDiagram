namespace MermaidDiagram.Core.Models
{
    /// <summary>
    /// Một node được parse từ Mermaid.
    ///
    /// Chỉ chứa dữ liệu mô tả node.
    /// Không chứa workflow/business logic.
    /// </summary>
    public sealed class MermaidNode
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// ID duy nhất của node trong Mermaid document.
        /// </summary>
        public string Id
        {
            get;
            set;
        } = string.Empty;

        // ============================================================
        // DISPLAY
        // ============================================================

        /// <summary>
        /// Nội dung hiển thị bên trong node.
        /// </summary>
        public string Text
        {
            get;
            set;
        } = string.Empty;

        // ============================================================
        // SHAPE
        // ============================================================

        /// <summary>
        /// Hình dạng của node trong Mermaid.
        /// </summary>
        public MermaidNodeShape Shape
        {
            get;
            set;
        } = MermaidNodeShape.Rectangle;

        // ============================================================
        // STYLE
        // ============================================================

        /// <summary>
        /// Style được parse hoặc suy ra từ Mermaid.
        /// </summary>
        public MermaidNodeStyle Style
        {
            get;
            set;
        } = new MermaidNodeStyle();


        // ============================================================
        // GROUP
        // ============================================================

        /// <summary>
        /// ID của subgraph chứa node.
        ///
        /// Null/rỗng nếu node không thuộc subgraph nào.
        /// </summary>
        public string? SubgraphId
        {
            get;
            set;
        }
    }
}