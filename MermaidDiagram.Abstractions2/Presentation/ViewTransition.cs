
namespace MermaidDiagram.Abstractions.Presentation
{
    /// <summary>
    /// Mô tả cách chuyển từ Workflow hiện tại
    /// sang một Workflow View.
    ///
    /// Đây là abstraction contract.
    ///
    /// Không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    ///     - animation framework cụ thể.
    /// </summary>
    public enum ViewTransition
    {
        /// <summary>
        /// Không có hiệu ứng đặc biệt.
        /// </summary>
        None = 0,

        /// <summary>
        /// Chuyển sang View bằng hiệu ứng fade.
        /// </summary>
        Fade = 1,

        /// <summary>
        /// View xuất hiện từ bên phải.
        /// </summary>
        SlideFromRight = 2,

        /// <summary>
        /// View xuất hiện từ bên trái.
        /// </summary>
        SlideFromLeft = 3,

        /// <summary>
        /// View xuất hiện từ phía dưới.
        /// </summary>
        SlideFromBottom = 4,

        /// <summary>
        /// View được mở rộng từ vị trí của Workflow Node.
        ///
        /// Đây là transition quan trọng cho Workflow UI:
        ///
        ///     Node
        ///       ↓ click
        ///     focus node
        ///       ↓
        ///     expand
        ///       ↓
        ///     View
        /// </summary>
        ExpandFromNode = 5,

        /// <summary>
        /// View được thu nhỏ/quay trở lại Workflow Node.
        /// </summary>
        CollapseToNode = 6
    }
}

