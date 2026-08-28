
using MermaidDiagram.Abstractions.Navigation;
using MermaidDiagram.Abstractions.Workflow.Views;

namespace MermaidDiagram.Abstractions.Presentation
{
    /// <summary>
    /// Context được truyền vào Workflow View Renderer
    /// khi một View được trình bày.
    ///
    /// Chứa thông tin về:
    ///     - View cần mở
    ///     - cách trình bày
    ///     - node đã kích hoạt View
    ///     - trạng thái navigation
    ///
    /// Không chứa:
    ///     - DevExpress Control
    ///     - WinForms Form
    ///     - MAUI Page
    ///     - Blazor Component
    ///     - UI implementation cụ thể.
    /// </summary>
    public sealed class ViewRenderContext
    {
        // ============================================================
        // PRESENTATION
        // ============================================================

        /// <summary>
        /// Cách View được trình bày.
        /// </summary>
        public ViewPresentationMode PresentationMode
        {
            get;
        }

        // ============================================================
        // VIEW
        // ============================================================

        /// <summary>
        /// Key của View cần trình bày.
        /// </summary>
        public string ViewKey
        {
            get;
        }

        // ============================================================
        // WORKFLOW NODE
        // ============================================================

        /// <summary>
        /// Mermaid node đã kích hoạt việc mở View.
        /// </summary>
        public string? NodeKey
        {
            get;
        }

        /// <summary>
        /// Workflow node tương ứng.
        /// </summary>
        public string? WorkflowNodeKey
        {
            get;
        }

        // ============================================================
        // NAVIGATION
        // ============================================================

        /// <summary>
        /// Request navigation ban đầu.
        ///
        /// Có thể null nếu View được mở trực tiếp
        /// mà không thông qua Workflow Navigator.
        /// </summary>
        public WorkflowNavigationRequest? NavigationRequest
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public ViewRenderContext(
            string viewKey,
            ViewPresentationMode presentationMode,
            string? nodeKey = null,
            string? workflowNodeKey = null,
            WorkflowNavigationRequest? navigationRequest = null)
        {
            if (string.IsNullOrWhiteSpace(viewKey))
            {
                throw new System.ArgumentException(
                    "ViewKey không được rỗng.",
                    nameof(viewKey));
            }

            ViewKey =
                viewKey;

            PresentationMode =
                presentationMode;

            NodeKey =
                nodeKey;

            WorkflowNodeKey =
                workflowNodeKey;

            NavigationRequest =
                navigationRequest;
        }
    }
}

