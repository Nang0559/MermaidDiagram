using MermaidDiagram.Abstractions.Views;
using MermaidDiagram.Abstractions.Workflow.Runtime;


namespace MermaidDiagram.Abstractions.Workflow.Views
{
    /// <summary>
    /// Contract chung cho một màn hình nghiệp vụ
    /// được mở từ một Workflow Node.
    ///
    /// Workflow framework không cần biết đây là:
    ///     - Form
    ///     - UserControl
    ///     - XtraForm
    ///     - MAUI ContentPage
    ///     - Blazor Component
    ///     - hoặc một loại UI khác.
    /// </summary>
    public interface IWorkflowView
    {
        /// <summary>
        /// Key định danh loại View.
        ///
        /// Ví dụ:
        ///     "QCDinhHuong"
        ///     "ReworkProcess"
        ///     "QCXacNhanCuoi"
        ///     "NhapPhieuTraHang"
        /// </summary>
        string ViewKey { get; }

        /// <summary>
        /// Tên hiển thị của View.
        /// </summary>
        string ViewTitle { get; }

        /// <summary>
        /// Context nghiệp vụ hiện tại của View.
        /// </summary>
        WorkflowContext Context { get; }

        /// <summary>
        /// Khởi tạo View bằng context nghiệp vụ.
        /// </summary>
        void Initialize(
            WorkflowContext context);

        /// <summary>
        /// Thực hiện lifecycle chính của View.
        ///
        /// View tự xử lý UI và trả về kết quả nghiệp vụ.
        /// </summary>
        WorkflowViewResult Execute();

        /// <summary>
        /// Được gọi khi View chuẩn bị đóng.
        ///
        /// Implementation dùng để giải phóng
        /// trạng thái UI hoặc resource riêng của View.
        /// </summary>
        void Close();
    }
}





