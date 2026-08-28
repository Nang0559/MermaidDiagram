
using System;

namespace MermaidDiagram.Abstractions.Workflow.Actions
{
    /// <summary>
    /// Metadata mô tả một Workflow Action.
    ///
    /// WorkflowActionDefinition chỉ chứa cấu hình tĩnh.
    ///
    /// Không chứa:
    ///     - Business logic
    ///     - UI logic
    ///     - Service implementation
    ///     - Repository
    ///     - Workflow runtime
    /// </summary>
    public sealed class WorkflowActionDefinition
    {
        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public WorkflowActionDefinition(
            string actionKey)
        {
            if (string.IsNullOrWhiteSpace(actionKey))
            {
                throw new ArgumentException(
                    "ActionKey không được rỗng.",
                    nameof(actionKey));
            }

            ActionKey = actionKey;
        }

        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// Key định danh duy nhất của Action.
        ///
        /// Phải thống nhất với IWorkflowAction.ActionKey.
        ///
        /// Ví dụ:
        ///     "TaoPhieuXuLyBatThuong"
        ///     "QCDinhHuong"
        ///     "XuatKhoRework"
        ///     "QCXacNhanCuoi"
        ///     "NhapLaiHangNG"
        /// </summary>
        public string ActionKey
        {
            get;
        }

        /// <summary>
        /// Tên hiển thị của Action.
        ///
        /// Ví dụ:
        ///     "Tạo phiếu xử lý bất thường"
        ///     "QC định hướng"
        ///     "Xuất kho Rework"
        /// </summary>
        public string? ActionTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Mô tả mục đích của Action.
        /// </summary>
        public string? Description
        {
            get;
            set;
        }

        // ============================================================
        // EXECUTION
        // ============================================================

        /// <summary>
        /// Cách Action được thực thi.
        ///
        /// Interactive:
        ///     Cần tương tác với người dùng.
        ///
        /// Automatic:
        ///     Workflow có thể tự động thực thi.
        ///
        /// Background:
        ///     Thực thi nền, không gắn trực tiếp với UI.
        /// </summary>
        public WorkflowActionMode Mode
        {
            get;
            set;
        } = WorkflowActionMode.Interactive;

        // ============================================================
        // USER CONTEXT
        // ============================================================

        /// <summary>
        /// Action có yêu cầu thông tin người dùng
        /// trong WorkflowContext hay không.
        ///
        /// Ví dụ:
        ///     Người tạo
        ///     Người xác nhận
        ///     Người thực hiện
        /// </summary>
        public bool RequiresUser
        {
            get;
            set;
        } = true;

        // ============================================================
        // RETRY
        // ============================================================

        /// <summary>
        /// Cho phép Workflow thực hiện lại Action
        /// khi Action thất bại hay không.
        /// </summary>
        public bool AllowRetry
        {
            get;
            set;
        }
    }
}

