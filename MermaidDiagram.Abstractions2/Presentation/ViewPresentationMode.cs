
namespace MermaidDiagram.Abstractions.Presentation
{
    /// <summary>
    /// Cách một Workflow View được trình bày
    /// trên UI.
    ///
    /// Đây chỉ là abstraction.
    /// Platform cụ thể sẽ quyết định cách hiện thực.
    /// </summary>
    public enum ViewPresentationMode
    {
        /// <summary>
        /// Không xác định.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Mở View như một màn hình thông thường.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Mở View dạng dialog/modal.
        /// </summary>
        Dialog = 2,

        /// <summary>
        /// Mở View như một màn hình chi tiết
        /// được phóng lớn từ Workflow Node.
        ///
        /// Ví dụ:
        ///     Click node "Nhập phiếu trả hàng"
        ///         ↓
        ///     Node zoom/focus
        ///         ↓
        ///     mở màn hình thao tác.
        /// </summary>
        Detail = 3,

        /// <summary>
        /// Mở View dạng overlay/panel trên
        /// workflow hiện tại.
        /// </summary>
        Overlay = 4
    }
}

