using System;
using MermaidDiagram.Abstractions.Views;
using MermaidDiagram.Abstractions.Workflow.Runtime;
using MermaidDiagram.Abstractions.Workflow.Views;

namespace MermaidDiagram.Core.Workflow.Views
{
    /// <summary>
    /// Base implementation cho tất cả Workflow View.
    ///
    /// Các Form nghiệp vụ chỉ cần kế thừa class này
    /// và override những phần thực sự cần thiết.
    ///
    /// WorkflowViewBase không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - UI framework cụ thể
    ///     - Business Service cụ thể
    ///
    /// Nó chỉ quản lý lifecycle của Workflow View.
    /// </summary>
    public abstract class WorkflowViewBase
        : IWorkflowView
    {
        // ============================================================
        // CONTEXT
        // ============================================================

        private WorkflowContext? _workflowContext;

        /// <summary>
        /// Context nghiệp vụ hiện tại.
        ///
        /// Chỉ có giá trị sau khi Initialize() được gọi.
        /// </summary>
        protected WorkflowContext WorkflowContext
        {
            get
            {
                if (_workflowContext == null)
                {
                    throw new InvalidOperationException(
                        $"Workflow View '{ViewKey}' " +
                        "chưa được Initialize().");
                }

                return _workflowContext;
            }
        }

        /// <summary>
        /// Context nghiệp vụ hiện tại.
        /// </summary>
        public WorkflowContext Context
        {
            get
            {
                return WorkflowContext;
            }
        }

        // ============================================================
        // IDENTIFICATION
        // ============================================================

        /// <summary>
        /// Key định danh View.
        ///
        /// Ví dụ:
        ///     "TaoPhieuTraHang"
        ///     "QCDinhHuong"
        ///     "ReworkProcess"
        ///     "QCXacNhanCuoi"
        /// </summary>
        public abstract string ViewKey
        {
            get;
        }

        /// <summary>
        /// Tên hiển thị của View.
        ///
        /// Mặc định sử dụng ViewKey.
        /// View nghiệp vụ có thể override.
        /// </summary>
        public virtual string ViewTitle
        {
            get
            {
                return ViewKey;
            }
        }

        // ============================================================
        // INITIALIZE
        // ============================================================

        /// <summary>
        /// Khởi tạo View bằng WorkflowContext.
        ///
        /// Lifecycle:
        ///
        ///     Create
        ///       ↓
        ///     Initialize
        ///       ↓
        ///     Execute
        ///       ↓
        ///     Close
        /// </summary>
        public virtual void Initialize(
            WorkflowContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            _workflowContext = context;

            OnInitialize(
                context);
        }

        // ============================================================
        // INITIALIZE HOOK
        // ============================================================

        /// <summary>
        /// Hook cho View nghiệp vụ thực hiện
        /// khởi tạo dữ liệu/UI.
        /// </summary>
        protected virtual void OnInitialize(
            WorkflowContext context)
        {
        }

        // ============================================================
        // EXECUTE
        // ============================================================

        /// <summary>
        /// Thực hiện View.
        ///
        /// Base implementation trả về Success.
        ///
        /// Form nghiệp vụ thường override method này
        /// để trả về kết quả thực tế.
        /// </summary>
        public virtual WorkflowViewResult Execute()
        {
            EnsureInitialized();

            return WorkflowViewResult.Success();
        }

        // ============================================================
        // CLOSE
        // ============================================================

        /// <summary>
        /// Đóng View và giải phóng lifecycle state.
        /// </summary>
        public virtual void Close()
        {
            if (_workflowContext == null)
            {
                return;
            }

            try
            {
                OnClose();
            }
            finally
            {
                _workflowContext = null;
            }
        }

        // ============================================================
        // CLOSE HOOK
        // ============================================================

        /// <summary>
        /// Hook cho View nghiệp vụ xử lý trước khi đóng.
        /// </summary>
        protected virtual void OnClose()
        {
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Đảm bảo View đã được Initialize.
        /// </summary>
        protected void EnsureInitialized()
        {
            if (_workflowContext == null)
            {
                throw new InvalidOperationException(
                    $"Workflow View '{ViewKey}' " +
                    "chưa được Initialize().");
            }
        }

        /// <summary>
        /// Cho phép kiểm tra View đã được Initialize hay chưa
        /// mà không phát sinh exception.
        /// </summary>
        protected bool IsInitialized
        {
            get
            {
                return _workflowContext != null;
            }
        }
    }
}