namespace MermaidDiagram.Abstractions.Workflow.Actions
{
    /// <summary>
    /// Chế độ thực thi của Workflow Action.
    ///
    /// Enum này mô tả cách Action được thực thi,
    /// không mô tả cách UI trình diễn Action.
    /// </summary>
    public enum WorkflowActionMode
    {
        /// <summary>
        /// Action cần người dùng tương tác.
        ///
        /// UI framework có thể quyết định cách
        /// trình diễn:
        ///     - Form
        ///     - Dialog
        ///     - UserControl
        ///     - Drawer
        ///     - BottomSheet
        ///     - Page
        /// </summary>
        Interactive = 0,

        /// <summary>
        /// Action được Workflow tự động thực hiện.
        ///
        /// Không yêu cầu người dùng tương tác.
        /// </summary>
        Automatic = 1,

        /// <summary>
        /// Action được thực hiện ở chế độ nền.
        ///
        /// Không yêu cầu người dùng tương tác trực tiếp
        /// và không nên phụ thuộc vào UI.
        /// </summary>
        Background = 2
    }
}