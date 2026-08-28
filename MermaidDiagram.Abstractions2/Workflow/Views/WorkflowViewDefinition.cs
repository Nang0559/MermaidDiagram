
using System;

namespace MermaidDiagram.Abstractions.Workflow.Views
{
    /// <summary>
    /// Định nghĩa một Workflow View.
    ///
    /// Definition chỉ mô tả View cần mở như thế nào.
    /// Không chứa implementation của UI framework.
    /// </summary>
    public sealed class WorkflowViewDefinition
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// Key định danh duy nhất của View.
        ///
        /// Ví dụ:
        ///     "NhapPhieuTraHang"
        ///     "QCDinhHuong"
        ///     "ReworkProcess"
        /// </summary>
        public string ViewKey
        {
            get;
        }

        /// <summary>
        /// Tên hiển thị của View.
        /// </summary>
        public string ViewTitle
        {
            get;
        }

        // ============================================================
        // WORKFLOW
        // ============================================================

        /// <summary>
        /// Node nghiệp vụ tạo ra View.
        /// </summary>
        public string NodeKey
        {
            get;
        }

        /// <summary>
        /// Action nghiệp vụ dẫn tới View.
        /// </summary>
        public string ActionKey
        {
            get;
        }

        // ============================================================
        // PRESENTATION
        // ============================================================

        /// <summary>
        /// Chế độ hiển thị của View.
        /// </summary>
        public WorkflowViewMode ViewMode
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public WorkflowViewDefinition(
            string viewKey,
            string viewTitle,
            string nodeKey,
            string actionKey,
            WorkflowViewMode viewMode =
                WorkflowViewMode.Default)
        {
            if (string.IsNullOrWhiteSpace(viewKey))
            {
                throw new ArgumentException(
                    "ViewKey không được rỗng.",
                    nameof(viewKey));
            }

            if (string.IsNullOrWhiteSpace(viewTitle))
            {
                throw new ArgumentException(
                    "ViewTitle không được rỗng.",
                    nameof(viewTitle));
            }

            if (string.IsNullOrWhiteSpace(nodeKey))
            {
                throw new ArgumentException(
                    "NodeKey không được rỗng.",
                    nameof(nodeKey));
            }

            if (string.IsNullOrWhiteSpace(actionKey))
            {
                throw new ArgumentException(
                    "ActionKey không được rỗng.",
                    nameof(actionKey));
            }

            ViewKey =
                viewKey.Trim();

            ViewTitle =
                viewTitle.Trim();

            NodeKey =
                nodeKey.Trim();

            ActionKey =
                actionKey.Trim();

            ViewMode =
                viewMode;
        }

        // ============================================================
        // DISPLAY
        // ============================================================

        public override string ToString()
        {
            return ViewKey;
        }
    }
}

