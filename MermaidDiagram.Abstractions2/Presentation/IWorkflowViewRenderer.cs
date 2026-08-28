
using MermaidDiagram.Abstractions.Views;


namespace MermaidDiagram.Abstractions.Presentation
{
    /// <summary>
    /// Renderer chịu trách nhiệm trình bày một Workflow View
    /// trên UI framework cụ thể.
    ///
    /// Đây là abstraction boundary giữa Workflow engine
    /// và UI technology.
    ///
    /// Implementation có thể là:
    ///
    ///     DevExpressWorkflowViewRenderer
    ///     MauiWorkflowViewRenderer
    ///     BlazorWorkflowViewRenderer
    ///
    /// Abstraction này không biết:
    ///     - Form
    ///     - UserControl
    ///     - Page
    ///     - Component
    ///     - DialogControl
    /// </summary>
    public interface IWorkflowViewRenderer
    {
        /// <summary>
        /// Hiển thị Workflow View.
        /// </summary>
        WorkflowViewResult Show(
            ViewRenderContext context);

        /// <summary>
        /// Đóng Workflow View hiện tại.
        /// </summary>
        void Close();

        /// <summary>
        /// Kiểm tra renderer hiện đang có View được hiển thị hay không.
        /// </summary>
        bool IsVisible
        {
            get;
        }
    }
}

