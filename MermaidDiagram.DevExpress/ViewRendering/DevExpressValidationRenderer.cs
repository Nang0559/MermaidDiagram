

using DevExpress.XtraEditors;

using MermaidDiagram.Abstractions.Workflow.Views.Schema;

namespace MermaidDiagram.DevExpress.ViewRendering
{
    /// <summary>
    /// Render Workflow validation thành DevExpress controls.
    ///
    /// Trách nhiệm:
    ///     - tạo validation panel;
    ///     - hiển thị validation messages;
    ///     - ẩn panel khi không có validation message;
    ///     - quản lý presentation của validation.
    ///
    /// Không chịu trách nhiệm:
    ///     - thực hiện validation;
    ///     - xác định field có hợp lệ hay không;
    ///     - chạy WorkflowValidationRule;
    ///     - workflow dispatch;
    ///     - business logic;
    ///     - navigation;
    ///     - field rendering;
    ///     - action rendering.
    /// </summary>
    public sealed class DevExpressValidationRenderer
    {
        // ============================================================
        // FACTORY
        // ============================================================

        private readonly DevExpressControlFactory _controlFactory;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public DevExpressValidationRenderer(
            DevExpressControlFactory controlFactory)
        {
            _controlFactory =
                controlFactory
                ?? throw new ArgumentNullException(
                    nameof(controlFactory));
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Tạo validation panel chưa có message.
        ///
        /// Panel mặc định Visible = false theo
        /// DevExpressControlFactory.
        /// </summary>
        public PanelControl CreatePanel()
        {
            return _controlFactory.CreateValidationPanel();
        }

        // ============================================================
        // RENDER MESSAGES
        // ============================================================

        /// <summary>
        /// Render danh sách validation message vào panel.
        ///
        /// Không có message:
        ///     → panel được ẩn.
        ///
        /// Có message:
        ///     → panel được hiển thị.
        /// </summary>
        public void Render(
            PanelControl panel,
            IEnumerable<string> messages)
        {
            if (panel == null)
            {
                throw new ArgumentNullException(
                    nameof(panel));
            }

            if (messages == null)
            {
                throw new ArgumentNullException(
                    nameof(messages));
            }

            panel.Controls.Clear();

            List<string> validMessages =
                messages
                    .Where(
                        message =>
                            !string.IsNullOrWhiteSpace(
                                message))
                    .Select(
                        message =>
                            message.Trim())
                    .ToList();

            if (validMessages.Count == 0)
            {
                panel.Visible =
                    false;

                return;
            }

            // --------------------------------------------------------
            // MESSAGES
            // --------------------------------------------------------

            for (int i = 0;
                 i < validMessages.Count;
                 i++)
            {
                LabelControl label =
                    _controlFactory.CreateLabel(
                        validMessages[i]);

                panel.Controls.Add(
                    label);
            }

            // --------------------------------------------------------
            // VISIBILITY
            // --------------------------------------------------------

            panel.Visible =
                true;
        }

        // ============================================================
        // SINGLE MESSAGE
        // ============================================================

        /// <summary>
        /// Render một validation message.
        ///
        /// Null hoặc empty message:
        ///     → panel được ẩn.
        /// </summary>
        public void Render(
            PanelControl panel,
            string? message)
        {
            if (panel == null)
            {
                throw new ArgumentNullException(
                    nameof(panel));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                Clear(panel);

                return;
            }

            Render(
                panel,
                new[]
                {
                    message
                });
        }

        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Xóa toàn bộ validation messages.
        /// </summary>
        public void Clear(
            PanelControl panel)
        {
            if (panel == null)
            {
                throw new ArgumentNullException(
                    nameof(panel));
            }

            panel.Controls.Clear();

            panel.Visible =
                false;
        }
    }
}

