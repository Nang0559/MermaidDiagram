namespace MermaidDiagram.Core.Models
{
    /// <summary>
    /// Style hiển thị của Mermaid node.
    ///
    /// Đây là model trung lập với UI framework.
    /// DevExpress/WinForms sẽ tự map style này sang
    /// DiagramShape tương ứng.
    /// </summary>
    public sealed class MermaidNodeStyle
    {
        

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidNodeStyle()
        {
            Fill =
                string.Empty;

            Stroke =
                string.Empty;

            TextColor =
                string.Empty;

            CssClass =
                string.Empty;

            StrokeWidth =
                null;
        }

        // ============================================================
        // COLORS
        // ============================================================

        public string Fill
        {
            get;
            set;
        }

        public string Stroke
        {
            get;
            set;
        }

        public string TextColor
        {
            get;
            set;
        }

        // ============================================================
        // CSS / CLASS
        // ============================================================

        public string CssClass
        {
            get;
            set;
        }

        // ============================================================
        // BORDER
        // ============================================================

        public double? StrokeWidth
        {
            get;
            set;
        }
    }
}