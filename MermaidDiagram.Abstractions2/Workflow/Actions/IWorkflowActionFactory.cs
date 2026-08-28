namespace MermaidDiagram.Abstractions.Workflow.Actions
{
    /// <summary>
    /// Factory tạo Workflow Action từ ActionKey.
    ///
    /// Không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    ///     - Repository
    ///     - Service nghiệp vụ cụ thể
    /// </summary>
    public interface IWorkflowActionFactory
    {
        /// <summary>
        /// Kiểm tra Action có được đăng ký hay không.
        /// </summary>
        bool Contains(
            string actionKey);

        /// <summary>
        /// Tạo Action theo ActionKey.
        /// </summary>
        IWorkflowAction Create(
            string actionKey);
    }
}