using MermaidDiagram.Abstractions.Workflow.Runtime;

namespace MermaidDiagram.Abstractions.Workflow.Actions
{
    /// <summary>
    /// Contract chung cho một Workflow Action.
    ///
    /// Action có:
    ///     - Identity kỹ thuật
    ///     - Thông tin trình diễn
    ///     - Context Workflow khi thực thi
    ///
    /// Không phụ thuộc UI framework.
    /// </summary>
    public interface IWorkflowAction
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// Key định danh kỹ thuật của Action.
        ///
        /// Ví dụ:
        ///     "TaoPhieuXuLyBatThuong"
        ///     "QCDinhHuong"
        ///     "XuatKhoRework"
        /// </summary>
        string ActionKey
        {
            get;
        }

        // ============================================================
        // PRESENTATION
        // ============================================================

        /// <summary>
        /// Tên hiển thị của Action.
        ///
        /// Ví dụ:
        ///     "Tạo phiếu xử lý bất thường"
        ///     "QC định hướng"
        ///     "Xuất kho Rework"
        /// </summary>
        string ActionTitle
        {
            get;
        }

        /// <summary>
        /// Mô tả ngắn cho Action.
        ///
        /// Dùng cho:
        ///     - Tooltip
        ///     - Command description
        ///     - Button description
        ///     - Action panel
        ///     - Accessibility
        /// </summary>
        string ActionDescription
        {
            get;
        }

        // ============================================================
        // EXECUTION
        // ============================================================

        /// <summary>
        /// Thực thi Action trong Workflow Context.
        /// </summary>
        WorkflowActionResult Execute(
            WorkflowContext context);
    }
}