namespace MermaidDiagram.Abstractions.Workflow.Definitions
{
    /// <summary>
    /// Loại node trong Workflow.
    ///
    /// Đây là metadata mô tả vai trò của node,
    /// không phụ thuộc UI framework.
    /// </summary>
    public enum WorkflowNodeKind
    {
        /// <summary>
        /// Node chỉ mang tính thông tin.
        ///
        /// Không yêu cầu người dùng thực hiện
        /// nghiệp vụ trực tiếp.
        /// </summary>
        Information = 0,

        /// <summary>
        /// Node đại diện cho một View nghiệp vụ.
        ///
        /// UI framework có thể trình diễn node này
        /// bằng Form, Page, UserControl, Dialog...
        /// </summary>
        View = 1,

        /// <summary>
        /// Node đại diện cho một Workflow Action.
        /// </summary>
        Action = 2,

        /// <summary>
        /// Node có tương tác với người dùng nhưng
        /// không nhất thiết phải là một View hoặc Action.
        /// </summary>
        Interactive = 3,

        /// <summary>
        /// Node yêu cầu quyết định để xác định
        /// nhánh tiếp theo của Workflow.
        /// </summary>
        Decision = 4,

        /// <summary>
        /// Node được Workflow tự động xử lý,
        /// không yêu cầu tương tác trực tiếp.
        /// </summary>
        Automatic = 5,

        /// <summary>
        /// Node kết thúc Workflow hoặc một nhánh Workflow.
        /// </summary>
        Terminal = 6
    }
}