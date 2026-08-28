
namespace MermaidDiagram.Abstractions.Navigation
{
    /// <summary>
    /// Contract điều phối navigation trong Workflow.
    ///
    /// Không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    ///     - UI framework cụ thể.
    /// </summary>
    public interface IWorkflowNavigator
    {
        /// <summary>
        /// Thực hiện navigation theo request.
        /// </summary>
        WorkflowNavigationResult Navigate(
            WorkflowNavigationRequest request);
    }
}

