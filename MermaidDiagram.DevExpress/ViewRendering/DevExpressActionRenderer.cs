

using DevExpress.XtraEditors;

using MermaidDiagram.Abstractions.Workflow.Actions;

namespace MermaidDiagram.DevExpress.ViewRendering
{
    /// <summary>
    /// Render WorkflowActionDefinition thành DevExpress control.
    ///
    /// Trách nhiệm:
    ///     - tạo control cho WorkflowAction;
    ///     - áp dụng ActionTitle;
    ///     - áp dụng trạng thái Enabled nếu definition cung cấp;
    ///     - lưu identity của action;
    ///     - đưa action definition vào Tag;
    ///     - chỉ render action Interactive thành UI control.
    ///
    /// Không chịu trách nhiệm:
    ///     - workflow dispatch;
    ///     - business logic;
    ///     - navigation;
    ///     - validation;
    ///     - field rendering;
    ///     - collection rendering;
    ///     - workflow execution.
    ///
    /// WorkflowActionMode có ý nghĩa:
    ///
    ///     Interactive
    ///         → có UI control để người dùng tương tác.
    ///
    ///     Automatic
    ///         → workflow tự thực hiện,
    ///           không render thành button.
    ///
    ///     Background
    ///         → workflow/background infrastructure thực hiện,
    ///           không render thành button.
    /// </summary>
    public sealed class DevExpressActionRenderer
    {
        // ============================================================
        // FACTORY
        // ============================================================

        private readonly DevExpressControlFactory _controlFactory;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public DevExpressActionRenderer(
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
        /// Render một WorkflowActionDefinition.
        ///
        /// Chỉ Interactive Action mới tạo UI control.
        ///
        /// Automatic / Background trả về null vì
        /// không phải UI action.
        /// </summary>
        public SimpleButton? Render(
            WorkflowActionDefinition action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(
                    nameof(action));
            }

            // --------------------------------------------------------
            // ACTION MODE
            // --------------------------------------------------------

            if (action.Mode !=
                WorkflowActionMode.Interactive)
            {
                return null;
            }

            // --------------------------------------------------------
            // CREATE BUTTON
            // --------------------------------------------------------

            SimpleButton button =
                _controlFactory.CreateButton(
                    GetTitle(action));

            // --------------------------------------------------------
            // IDENTITY
            // --------------------------------------------------------

            button.Tag =
                action;

            // --------------------------------------------------------
            // STATE
            // --------------------------------------------------------

            ConfigureState(
                button,
                action);

            return button;
        }

        /// <summary>
        /// Render Interactive Action và thêm vào ActionPanel.
        ///
        /// Automatic / Background không tạo UI.
        ///</summary>
        public SimpleButton? Render(
            WorkflowActionDefinition action,
            PanelControl actionPanel)
        {
            if (action == null)
            {
                throw new ArgumentNullException(
                    nameof(action));
            }

            if (actionPanel == null)
            {
                throw new ArgumentNullException(
                    nameof(actionPanel));
            }

            SimpleButton? button =
                Render(action);

            if (button == null)
            {
                return null;
            }

            actionPanel.Controls.Add(
                button);

            return button;
        }

        // ============================================================
        // TITLE
        // ============================================================

        private static string GetTitle(
            WorkflowActionDefinition action)
        {
            return action.ActionTitle;
        }

        // ============================================================
        // STATE
        // ============================================================

        private static void ConfigureState(
            SimpleButton button,
            WorkflowActionDefinition action)
        {
            button.Enabled =
                true;
        }
    }
}

