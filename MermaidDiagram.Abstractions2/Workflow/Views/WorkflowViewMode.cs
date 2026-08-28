namespace MermaidDiagram.Abstractions.Workflow.Views
{
    /// <summary>
    /// Chế độ hiển thị của một Workflow View.
    ///
    /// Đây chỉ là contract ở tầng Abstractions.
    /// UI framework cụ thể sẽ quyết định cách hiện thực:
    ///
    ///     Dialog
    ///     Window
    ///     Embedded
    ///     Overlay
    ///     FullScreen
    ///     ...
    /// </summary>
    public enum WorkflowViewMode
    {
        /// <summary>
        /// Chế độ mặc định.
        /// UI framework tự quyết định cách hiển thị phù hợp.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Hiển thị dạng cửa sổ / dialog độc lập.
        /// </summary>
        Dialog = 10,

        /// <summary>
        /// Hiển thị trực tiếp bên trong vùng nội dung
        /// của ứng dụng hiện tại.
        /// </summary>
        Embedded = 20,

        /// <summary>
        /// Hiển thị dạng overlay trên workflow hiện tại.
        /// </summary>
        Overlay = 30,

        /// <summary>
        /// Hiển thị toàn màn hình.
        /// </summary>
        FullScreen = 40
    }
}