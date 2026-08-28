
namespace MermaidDiagram.Abstractions.Interaction
{
    /// <summary>
    /// Cấu hình hành vi tương tác với Mermaid Diagram.
    ///
    /// Đây là abstraction dùng chung cho:
    ///     - DevExpress
    ///     - WinForms
    ///     - WPF
    ///     - MAUI
    ///     - Blazor
    ///
    /// Không chứa cấu hình phụ thuộc UI framework cụ thể.
    /// </summary>
    public sealed class MermaidInteractionOptions
    {
        // ============================================================
        // ENABLE / DISABLE
        // ============================================================

        /// <summary>
        /// Cho phép người dùng click vào node.
        /// </summary>
        public bool EnableNodeClick
        {
            get;
            set;
        }

        /// <summary>
        /// Cho phép người dùng hover lên node.
        /// </summary>
        public bool EnableNodeHover
        {
            get;
            set;
        }

        // ============================================================
        // CLICK BEHAVIOR
        // ============================================================

        /// <summary>
        /// Cho phép click node thực hiện navigation
        /// tới Workflow View tương ứng.
        /// </summary>
        public bool EnableNodeNavigation
        {
            get;
            set;
        }

        /// <summary>
        /// Cho phép giữ node hiện tại ở trạng thái selected
        /// sau khi click.
        /// </summary>
        public bool KeepSelectedNode
        {
            get;
            set;
        }

        // ============================================================
        // HOVER BEHAVIOR
        // ============================================================

        /// <summary>
        /// Cho phép áp dụng hiệu ứng hover lên node.
        ///
        /// Cách hiển thị hiệu ứng do UI implementation quyết định.
        /// </summary>
        public bool EnableHoverEffect
        {
            get;
            set;
        }

        // ============================================================
        // DEFAULT
        // ============================================================

        /// <summary>
        /// Tạo cấu hình interaction mặc định.
        /// </summary>
        public MermaidInteractionOptions()
        {
            EnableNodeClick =
                true;

            EnableNodeHover =
                true;

            EnableNodeNavigation =
                true;

            KeepSelectedNode =
                true;

            EnableHoverEffect =
                true;
        }

        // ============================================================
        // FACTORY
        // ============================================================

        /// <summary>
        /// Tạo cấu hình interaction mặc định.
        /// </summary>
        public static MermaidInteractionOptions Default()
        {
            return new MermaidInteractionOptions();
        }
    }
}

