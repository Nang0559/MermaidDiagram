namespace MermaidDiagram.Core.Models
{
    /// <summary>
    /// Kiểu hiển thị đường nối của Mermaid edge.
    ///
    /// Đây là model trung lập với UI framework.
    /// DevExpress/WinForms sẽ tự map kiểu này
    /// sang kiểu đường nối tương ứng.
    /// </summary>
    public enum MermaidEdgeStyle
    {
        /// <summary>
        /// Đường liền thông thường.
        /// </summary>
        Solid = 0,

        /// <summary>
        /// Đường nét đứt.
        /// </summary>
        Dashed = 1,

        /// <summary>
        /// Đường liền nét dày.
        /// </summary>
        Thick = 2
    }
}