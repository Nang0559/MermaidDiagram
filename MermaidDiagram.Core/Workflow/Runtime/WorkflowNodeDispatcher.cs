using MermaidDiagram.Abstractions.Views;
using MermaidDiagram.Abstractions.Workflow.Actions;
using MermaidDiagram.Abstractions.Workflow.Definitions;
using MermaidDiagram.Abstractions.Workflow.Runtime;
using MermaidDiagram.Abstractions.Workflow.Views;
using MermaidDiagram.Core.Workflow.Registry;

namespace MermaidDiagram.Core.Workflow.Runtime
{
    /// <summary>
    /// Điều phối một Workflow Node.
    ///
    /// Dispatcher chỉ chịu trách nhiệm:
    ///
    ///     NodeKey
    ///       ↓
    ///     Registry
    ///       ↓
    ///     WorkflowNodeDefinition
    ///       ↓
    ///     ViewFactory / ActionFactory
    ///       ↓
    ///     WorkflowDispatchResult
    ///
    /// Không chứa business logic.
    /// Không quyết định business transition.
    /// </summary>
    public sealed class WorkflowNodeDispatcher
    {
        // ============================================================
        // DEPENDENCIES
        // ============================================================

        private readonly WorkflowNodeRegistry _nodeRegistry;

        private readonly IWorkflowViewFactory _viewFactory;

        private readonly IWorkflowActionFactory _actionFactory;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public WorkflowNodeDispatcher(
            WorkflowNodeRegistry nodeRegistry,
            IWorkflowViewFactory viewFactory,
            IWorkflowActionFactory actionFactory)
        {
            _nodeRegistry =
                nodeRegistry
                ?? throw new ArgumentNullException(
                    nameof(nodeRegistry));

            _viewFactory =
                viewFactory
                ?? throw new ArgumentNullException(
                    nameof(viewFactory));

            _actionFactory =
                actionFactory
                ?? throw new ArgumentNullException(
                    nameof(actionFactory));
        }

        // ============================================================
        // DISPATCH
        // ============================================================

        public WorkflowDispatchResult Dispatch(
            string nodeKey,
            WorkflowContext context)
        {
            if (string.IsNullOrWhiteSpace(nodeKey))
            {
                throw new ArgumentException(
                    "NodeKey không được rỗng.",
                    nameof(nodeKey));
            }

            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            // ========================================================
            // 1. RESOLVE DEFINITION
            // ========================================================

            WorkflowNodeDefinition definition =
                _nodeRegistry.Get(nodeKey);

            // ========================================================
            // 2. VALIDATE
            // ========================================================

            definition.Validate();

            // ========================================================
            // 3. UPDATE CONTEXT
            // ========================================================

            context.PreviousNodeKey =
                context.CurrentNodeKey;

            context.CurrentNodeKey =
                definition.NodeKey;

            // ========================================================
            // 4. VIEW
            // ========================================================

            WorkflowViewResult? viewResult =
                null;

            if (definition.HasView)
            {
                viewResult =
                    ExecuteView(
                        definition,
                        context);

                if (viewResult.IsCancelled)
                {
                    return WorkflowDispatchResult.Cancelled(
                        viewResult.Message,
                        definition.NodeKey);
                }

                if (viewResult.IsFailed)
                {
                    return WorkflowDispatchResult.Failed(
                        viewResult.Message
                        ?? "Workflow View thực thi thất bại.",
                        definition.NodeKey);
                }
            }

            // ========================================================
            // 5. ACTION
            // ========================================================

            WorkflowActionResult? actionResult =
                null;

            if (definition.HasAction)
            {
                actionResult =
                    ExecuteAction(
                        definition,
                        context);

                if (actionResult.IsCancelled)
                {
                    return WorkflowDispatchResult.Cancelled(
                        actionResult.Message,
                        definition.NodeKey);
                }

                if (actionResult.IsFailed)
                {
                    return WorkflowDispatchResult.Failed(
                        actionResult.Message
                        ?? "Workflow Action thực thi thất bại.",
                        definition.NodeKey);
                }
            }

            // ========================================================
            // 6. RESOLVE NEXT NODE
            // ========================================================

            string? nextNodeKey =
                ResolveNextNode(
                    definition,
                    viewResult,
                    actionResult);

            // ========================================================
            // 7. SUCCESS
            // ========================================================

            return WorkflowDispatchResult.Success(
                definition.NodeKey,
                nextNodeKey);
        }

        // ============================================================
        // EXECUTE VIEW
        // ============================================================

        private WorkflowViewResult ExecuteView(
            WorkflowNodeDefinition definition,
            WorkflowContext context)
        {
            // --------------------------------------------------------
            // ViewKey
            // --------------------------------------------------------

            string viewKey =
                definition.ViewKey
                ?? throw new InvalidOperationException(
                    $"Node '{definition.NodeKey}' " +
                    "có View nhưng ViewKey đang rỗng.");

            // --------------------------------------------------------
            // ViewTitle
            // --------------------------------------------------------

            string viewTitle =
                !string.IsNullOrWhiteSpace(
                    definition.DisplayName)
                    ? definition.DisplayName!
                    : viewKey;

            // --------------------------------------------------------
            // ActionKey
            // --------------------------------------------------------
            //
            // WorkflowViewDefinition hiện tại yêu cầu ActionKey
            // non-null.
            //
            // Vì vậy nếu ViewDefinition vẫn giữ contract đó,
            // node có View bắt buộc phải có ActionKey.
            //

            string actionKey =
                definition.ActionKey
                ?? throw new InvalidOperationException(
                    $"Node '{definition.NodeKey}' có View " +
                    "nhưng chưa cấu hình ActionKey. " +
                    "WorkflowViewDefinition yêu cầu ActionKey.");

            // --------------------------------------------------------
            // VIEW DEFINITION
            // --------------------------------------------------------

            WorkflowViewDefinition viewDefinition =
                new WorkflowViewDefinition(
                    viewKey,
                    viewTitle,
                    definition.NodeKey,
                    actionKey);

            // --------------------------------------------------------
            // CREATE VIEW
            // --------------------------------------------------------

            IWorkflowView view =
                _viewFactory.Create(
                    viewDefinition,
                    context);

            if (view == null)
            {
                throw new InvalidOperationException(
                    $"WorkflowViewFactory không tạo được " +
                    $"View '{viewKey}'.");
            }

            try
            {
                return view.Execute();
            }
            finally
            {
                view.Close();
            }
        }

        // ============================================================
        // EXECUTE ACTION
        // ============================================================

        private WorkflowActionResult ExecuteAction(
            WorkflowNodeDefinition definition,
            WorkflowContext context)
        {
            string actionKey =
                definition.ActionKey
                ?? throw new InvalidOperationException(
                    $"Node '{definition.NodeKey}' " +
                    "có Action nhưng ActionKey đang rỗng.");

            IWorkflowAction action =
                _actionFactory.Create(
                    actionKey);

            if (action == null)
            {
                throw new InvalidOperationException(
                    $"WorkflowActionFactory không tạo được " +
                    $"Action '{actionKey}'.");
            }

            return action.Execute(
                context);
        }

        // ============================================================
        // RESOLVE NEXT NODE
        // ============================================================

        private string? ResolveNextNode(
     WorkflowNodeDefinition definition,
     WorkflowViewResult? viewResult,
     WorkflowActionResult? actionResult)
        {
            if (viewResult != null &&
                !string.IsNullOrWhiteSpace(
                    viewResult.NextAction))
            {
                string nextAction =
                    viewResult.NextAction
                    ?? throw new InvalidOperationException(
                        "ViewResult.NextAction không được null.");

                return ResolveExplicitNextNode(
                    definition,
                    nextAction);
            }

            if (actionResult != null &&
                !string.IsNullOrWhiteSpace(
                    actionResult.NextAction))
            {
                string nextAction =
                    actionResult.NextAction
                    ?? throw new InvalidOperationException(
                        "ActionResult.NextAction không được null.");

                return ResolveExplicitNextNode(
                    definition,
                    nextAction);
            }

            if (definition.NextNodeKeys.Count == 1)
            {
                return definition.NextNodeKeys[0];
            }

            return null;
        }

        // ============================================================
        // RESOLVE EXPLICIT NEXT NODE
        // ============================================================

        private string? ResolveExplicitNextNode(
            WorkflowNodeDefinition definition,
            string nextAction)
        {
            for (int i = 0;
                 i < definition.NextNodeKeys.Count;
                 i++)
            {
                string nextNodeKey =
                    definition.NextNodeKeys[i];

                if (string.Equals(
                        nextNodeKey,
                        nextAction,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return nextNodeKey;
                }
            }

            return null;
        }
    }
}