
using MermaidDiagram.Abstractions.Workflow.Runtime;
using MermaidDiagram.Abstractions.Workflow.Views;

namespace MermaidDiagram.Core.Workflow.Views
{
    /// <summary>
    /// Factory tạo Workflow View từ WorkflowViewDefinition.
    ///
    /// Chịu trách nhiệm:
    ///     - Đăng ký View Factory.
    ///     - Resolve View theo ViewKey.
    ///     - Tạo View với WorkflowContext.
    ///     - Validate View identity.
    ///     - Initialize View.
    ///
    /// Không biết:
    ///     - Form cụ thể.
    ///     - DevExpress.
    ///     - WinForms.
    ///     - Business Service.
    ///     - Business logic.
    /// </summary>
    public sealed class WorkflowViewFactory
        : IWorkflowViewFactory
    {
        // ============================================================
        // FACTORIES
        // ============================================================

        private readonly Dictionary<
            string,
            Func<WorkflowContext, IWorkflowView>>
            _factories =
                new Dictionary<
                    string,
                    Func<WorkflowContext, IWorkflowView>>(
                        StringComparer.OrdinalIgnoreCase);

        // ============================================================
        // REGISTER
        // ============================================================

        /// <summary>
        /// Đăng ký một loại Workflow View.
        ///
        /// Mỗi ViewKey chỉ được đăng ký một lần.
        /// </summary>
        public void Register(
            string viewKey,
            Func<WorkflowContext, IWorkflowView> factory)
        {
            ValidateViewKey(viewKey);

            if (factory == null)
            {
                throw new ArgumentNullException(
                    nameof(factory));
            }

            if (_factories.ContainsKey(viewKey))
            {
                throw new InvalidOperationException(
                    $"Workflow View '{viewKey}' đã được đăng ký.");
            }

            _factories.Add(
                viewKey,
                factory);
        }

        // ============================================================
        // REGISTER OR REPLACE
        // ============================================================

        /// <summary>
        /// Đăng ký mới hoặc thay thế Workflow View Factory.
        /// </summary>
        public void RegisterOrReplace(
            string viewKey,
            Func<WorkflowContext, IWorkflowView> factory)
        {
            ValidateViewKey(viewKey);

            if (factory == null)
            {
                throw new ArgumentNullException(
                    nameof(factory));
            }

            _factories[viewKey] =
                factory;
        }

        // ============================================================
        // CREATE
        // ============================================================

        /// <summary>
        /// Tạo Workflow View từ WorkflowViewDefinition.
        ///
        /// Lifecycle:
        ///
        ///     WorkflowViewDefinition
        ///              ↓
        ///           ViewKey
        ///              ↓
        ///        Resolve Factory
        ///              ↓
        ///          Create View
        ///              ↓
        ///       Validate Identity
        ///              ↓
        ///       Initialize(Context)
        ///              ↓
        ///          Return View
        /// </summary>
        public IWorkflowView Create(
            WorkflowViewDefinition definition,
            WorkflowContext context)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            ValidateViewKey(
                definition.ViewKey);

            // ========================================================
            // RESOLVE FACTORY
            // ========================================================

            if (!_factories.TryGetValue(
                    definition.ViewKey,
                    out Func<
                        WorkflowContext,
                        IWorkflowView> factory))
            {
                throw new KeyNotFoundException(
                    $"Không tìm thấy Workflow View " +
                    $"'{definition.ViewKey}'.");
            }

            // ========================================================
            // CREATE
            // ========================================================

            IWorkflowView view =
                factory(context);

            if (view == null)
            {
                throw new InvalidOperationException(
                    $"Factory Workflow View " +
                    $"'{definition.ViewKey}' " +
                    "đã trả về null.");
            }

            // ========================================================
            // VALIDATE IDENTITY
            // ========================================================

            if (string.IsNullOrWhiteSpace(
                    view.ViewKey))
            {
                throw new InvalidOperationException(
                    $"Workflow View Factory " +
                    $"'{definition.ViewKey}' " +
                    "tạo ra View không có ViewKey.");
            }

            if (!string.Equals(
                    view.ViewKey,
                    definition.ViewKey,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Workflow View Factory " +
                    $"'{definition.ViewKey}' " +
                    $"tạo ra View có ViewKey=" +
                    $"'{view.ViewKey}'.");
            }

            // ========================================================
            // INITIALIZE
            // ========================================================

            view.Initialize(
                context);

            return view;
        }

        // ============================================================
        // CONTAINS
        // ============================================================

        /// <summary>
        /// Kiểm tra ViewKey đã được đăng ký hay chưa.
        /// </summary>
        public bool Contains(
            string viewKey)
        {
            return
                !string.IsNullOrWhiteSpace(viewKey)
                &&
                _factories.ContainsKey(viewKey);
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private static void ValidateViewKey(
            string viewKey)
        {
            if (string.IsNullOrWhiteSpace(viewKey))
            {
                throw new ArgumentException(
                    "ViewKey không được rỗng.",
                    nameof(viewKey));
            }
        }
    }
}

