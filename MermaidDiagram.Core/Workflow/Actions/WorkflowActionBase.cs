using MermaidDiagram.Abstractions.Workflow.Actions;
using MermaidDiagram.Abstractions.Workflow.Runtime;

namespace MermaidDiagram.Core.Workflow.Actions
{
    /// <summary>
    /// Base implementation dùng chung cho tất cả Workflow Action.
    ///
    /// Chịu trách nhiệm:
    ///     - Validate WorkflowContext.
    ///     - Chuẩn hóa execution.
    ///     - Exception handling.
    ///     - Cung cấp execution hooks.
    ///
    /// Không chứa business logic cụ thể.
    ///
    /// Business logic phải được triển khai trong ExecuteCore().
    /// </summary>
    public abstract class WorkflowActionBase
        : IWorkflowAction
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// Key định danh kỹ thuật của Action.
        /// </summary>
        public abstract string ActionKey
        {
            get;
        }

        // ============================================================
        // PRESENTATION
        // ============================================================

        /// <summary>
        /// Tên hiển thị của Action.
        ///
        /// Mặc định sử dụng ActionKey.
        /// </summary>
        public virtual string ActionTitle
        {
            get
            {
                return ActionKey;
            }
        }

        /// <summary>
        /// Mô tả ngắn của Action.
        ///
        /// Mặc định sử dụng ActionTitle.
        /// </summary>
        public virtual string ActionDescription
        {
            get
            {
                return ActionTitle;
            }
        }

        // ============================================================
        // EXECUTE
        // ============================================================

        /// <summary>
        /// Entry point chuẩn để thực thi Action.
        ///
        /// Implementation không override phương thức này.
        /// Chỉ triển khai ExecuteCore().
        /// </summary>
        public WorkflowActionResult Execute(
            WorkflowContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            ValidateContext(
                context);

            try
            {
                OnBeforeExecute(
                    context);

                WorkflowActionResult result =
                    ExecuteCore(
                        context);

                if (result == null)
                {
                    return WorkflowActionResult.Failed(
                        $"Action '{ActionKey}' không trả về kết quả.");
                }

                OnAfterExecute(
                    context,
                    result);

                return result;
            }
            catch (Exception ex)
            {
                return OnException(
                    context,
                    ex);
            }
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Validate context chung trước khi Action chạy.
        ///
        /// Không giả định Action phải có View.
        /// </summary>
        protected virtual void ValidateContext(
            WorkflowContext context)
        {
            if (string.IsNullOrWhiteSpace(
                    context.WorkflowKey))
            {
                throw new InvalidOperationException(
                    $"WorkflowContext của Action '{ActionKey}' " +
                    "không có WorkflowKey.");
            }
        }

        // ============================================================
        // CORE
        // ============================================================

        /// <summary>
        /// Thực thi business logic thực tế.
        /// </summary>
        protected abstract WorkflowActionResult ExecuteCore(
            WorkflowContext context);

        // ============================================================
        // HOOKS
        // ============================================================

        /// <summary>
        /// Hook trước ExecuteCore().
        /// </summary>
        protected virtual void OnBeforeExecute(
            WorkflowContext context)
        {
        }

        /// <summary>
        /// Hook sau ExecuteCore().
        /// </summary>
        protected virtual void OnAfterExecute(
            WorkflowContext context,
            WorkflowActionResult result)
        {
        }

        /// <summary>
        /// Xử lý exception trong quá trình execution.
        ///
        /// Mặc định chuyển exception thành Failed result.
        /// </summary>
        protected virtual WorkflowActionResult OnException(
            WorkflowContext context,
            Exception exception)
        {
            return WorkflowActionResult.Failed(
                $"Action '{ActionKey}' lỗi: {exception.Message}");
        }
    }
}