
namespace MermaidDiagram.Abstractions.Workflow.Views
{
    /// <summary>
    /// Contract điều khiển viewport của Mermaid Diagram.
    ///
    /// Viewport chịu trách nhiệm:
    ///     - Fit toàn bộ sơ đồ vào vùng hiển thị
    ///     - Center sơ đồ
    ///     - Reset trạng thái viewport
    ///     - Zoom In / Zoom Out
    ///
    /// Interface này không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - WPF
    ///     - MAUI
    ///     - Blazor
    ///
    /// Implementation cụ thể nằm ở tầng UI tương ứng.
    /// </summary>
    public interface IMermaidDiagramViewport
    {
        /// <summary>
        /// Đưa toàn bộ nội dung sơ đồ vào vùng hiển thị.
        /// </summary>
        void FitToContent();

        /// <summary>
        /// Đưa nội dung sơ đồ về giữa viewport.
        /// </summary>
        void CenterContent();

        /// <summary>
        /// Đưa viewport về trạng thái mặc định.
        /// </summary>
        void Reset();

        /// <summary>
        /// Phóng to sơ đồ một bước.
        /// </summary>
        void ZoomIn();

        /// <summary>
        /// Thu nhỏ sơ đồ một bước.
        /// </summary>
        void ZoomOut();
    }
}

