using DevExpress.XtraEditors;
using MermaidDiagram.Abstractions.Workflow.Views;
using MermaidDiagram.Abstractions.Workflow.Views.Schema;
using System;
using System.Windows.Forms;

namespace MermaidDiagram.DevExpress.ViewRendering
{
    /// <summary>
    /// Render WorkflowFieldDefinition thành DevExpress / WinForms control.
    ///
    /// Trách nhiệm:
    ///     - đọc WorkflowFieldDefinition;
    ///     - kiểm tra Visible;
    ///     - tạo FieldPanel;
    ///     - tạo FieldTitle;
    ///     - chọn editor theo WorkflowFieldKind;
    ///     - áp dụng ReadOnly;
    ///     - áp dụng Placeholder khi editor hỗ trợ;
    ///     - lưu FieldKey vào Tag.
    ///
    /// Không chịu trách nhiệm:
    ///     - tạo WorkflowView;
    ///     - render Action;
    ///     - render Collection;
    ///     - render Validation;
    ///     - workflow dispatch;
    ///     - navigation;
    ///     - business logic.
    ///
    /// Kiến trúc:
    ///
    /// WorkflowFieldDefinition
    ///          ↓
    /// DevExpressFieldRenderer
    ///          ↓
    /// DevExpressControlFactory
    ///          ↓
    /// PanelControl
    ///     ├── LabelControl
    ///     └── Editor
    /// </summary>
    public sealed class DevExpressFieldRenderer
    {
        // ============================================================
        // FACTORY
        // ============================================================

        private readonly DevExpressControlFactory _controlFactory;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public DevExpressFieldRenderer(
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
        /// Render một WorkflowFieldDefinition.
        ///
        /// Field Hidden hoặc Visible=false sẽ không được render.
        /// </summary>
        public PanelControl? Render(
            WorkflowFieldDefinition field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(
                    nameof(field));
            }

            // --------------------------------------------------------
            // VISIBILITY
            // --------------------------------------------------------

            if (!field.Visible ||
                field.IsHidden)
            {
                return null;
            }

            // --------------------------------------------------------
            // FIELD PANEL
            // --------------------------------------------------------

            PanelControl panel =
                _controlFactory.CreateFieldPanel();

            // --------------------------------------------------------
            // LABEL
            // --------------------------------------------------------

            LabelControl label =
                _controlFactory.CreateLabel(
                    field.FieldTitle);

            panel.Controls.Add(
                label);

            // --------------------------------------------------------
            // EDITOR
            // --------------------------------------------------------

            Control editor =
                CreateEditor(field);

            ConfigureEditor(
                editor,
                field);

            panel.Controls.Add(
                editor);

            // --------------------------------------------------------
            // IDENTITY
            // --------------------------------------------------------

            panel.Tag =
                field.FieldKey;

            editor.Tag =
                field.FieldKey;

            // --------------------------------------------------------
            // ORDER
            // --------------------------------------------------------

            // Label nằm phía trên editor.
            //
            // Vì cả hai đều Dock.Top,
            // label phải được đưa về index 0.
            panel.Controls.SetChildIndex(
                label,
                0);

            return panel;
        }

        // ============================================================
        // EDITOR FACTORY
        // ============================================================

        private Control CreateEditor(
            WorkflowFieldDefinition field)
        {
            switch (field.Kind)
            {
                // ----------------------------------------------------
                // TEXT
                // ----------------------------------------------------

                case WorkflowFieldKind.Text:
                    return _controlFactory.CreateTextEdit();

                // ----------------------------------------------------
                // MULTILINE TEXT
                // ----------------------------------------------------

                case WorkflowFieldKind.MultilineText:
                    return _controlFactory.CreateMemoEdit();

                // ----------------------------------------------------
                // NUMBER
                // ----------------------------------------------------

                case WorkflowFieldKind.Number:
                    return CreateNumberEdit();

                // ----------------------------------------------------
                // DECIMAL
                // ----------------------------------------------------

                case WorkflowFieldKind.Decimal:
                    return CreateDecimalEdit();

                // ----------------------------------------------------
                // DATE
                // ----------------------------------------------------

                case WorkflowFieldKind.Date:
                    return CreateDateEdit();

                // ----------------------------------------------------
                // DATETIME
                // ----------------------------------------------------

                case WorkflowFieldKind.DateTime:
                    return CreateDateEdit();

                // ----------------------------------------------------
                // BOOLEAN
                // ----------------------------------------------------

                case WorkflowFieldKind.Boolean:
                    return CreateCheckEdit();

                // ----------------------------------------------------
                // SELECT
                // ----------------------------------------------------

                case WorkflowFieldKind.Select:
                    return CreateSelectEdit();

                // ----------------------------------------------------
                // COLLECTION
                // ----------------------------------------------------

                case WorkflowFieldKind.Collection:
                    throw new NotSupportedException(
                        $"WorkflowField '{field.FieldKey}' " +
                        "là Collection và phải được render bởi " +
                        "DevExpressCollectionRenderer.");

                // ----------------------------------------------------
                // HIDDEN
                // ----------------------------------------------------

                case WorkflowFieldKind.Hidden:
                    throw new NotSupportedException(
                        $"WorkflowField '{field.FieldKey}' " +
                        "là Hidden và không được render thành editor.");

                // ----------------------------------------------------
                // UNKNOWN
                // ----------------------------------------------------

                default:
                    throw new NotSupportedException(
                        $"WorkflowField '{field.FieldKey}' " +
                        $"có WorkflowFieldKind không được hỗ trợ: " +
                        $"{field.Kind}.");
            }
        }

        // ============================================================
        // NUMBER
        // ============================================================

        private static SpinEdit CreateNumberEdit()
        {
            SpinEdit editor =
                new SpinEdit();

            editor.Dock =
                DockStyle.Top;

            editor.Properties.IsFloatValue =
                false;

            return editor;
        }

        // ============================================================
        // DECIMAL
        // ============================================================

        private static SpinEdit CreateDecimalEdit()
        {
            SpinEdit editor =
                new SpinEdit();

            editor.Dock =
                DockStyle.Top;

            editor.Properties.IsFloatValue =
                true;

            return editor;
        }

        // ============================================================
        // DATE / DATETIME
        // ============================================================

        private static DateEdit CreateDateEdit()
        {
            DateEdit editor =
                new DateEdit();

            editor.Dock =
                DockStyle.Top;

            return editor;
        }

        // ============================================================
        // BOOLEAN
        // ============================================================

        private static CheckEdit CreateCheckEdit()
        {
            CheckEdit editor =
                new CheckEdit();

            editor.Dock =
                DockStyle.Top;

            return editor;
        }

        // ============================================================
        // SELECT
        // ============================================================

        private static ComboBoxEdit CreateSelectEdit()
        {
            ComboBoxEdit editor =
                new ComboBoxEdit();

            editor.Dock =
                DockStyle.Top;

            return editor;
        }

        // ============================================================
        // EDITOR CONFIGURATION
        // ============================================================

        private static void ConfigureEditor(
            Control editor,
            WorkflowFieldDefinition field)
        {
            if (editor == null)
            {
                throw new ArgumentNullException(
                    nameof(editor));
            }

            if (field == null)
            {
                throw new ArgumentNullException(
                    nameof(field));
            }

            // --------------------------------------------------------
            // READ ONLY
            // --------------------------------------------------------

            if (editor is BaseEdit baseEdit)
            {
                baseEdit.Properties.ReadOnly =
                    field.ReadOnly;
            }

            // --------------------------------------------------------
            // PLACEHOLDER
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    field.Placeholder))
            {
                ApplyPlaceholder(
                    editor,
                    field.Placeholder);
            }
        }

        // ============================================================
        // PLACEHOLDER
        // ============================================================

        private static void ApplyPlaceholder(
     Control editor,
     string placeholder)
        {
            if (string.IsNullOrWhiteSpace(placeholder))
            {
                return;
            }

            // ============================================================
            // TEXT
            // ============================================================

            if (editor is TextEdit textEdit)
            {
                textEdit.Properties.NullValuePrompt =
                    placeholder;

                textEdit.Properties.ShowNullValuePrompt =
                    ShowNullValuePromptOptions.Default;

                return;
            }

            // ============================================================
            // MEMO
            // ============================================================

            if (editor is MemoEdit memoEdit)
            {
                memoEdit.Properties.NullValuePrompt =
                    placeholder;

                memoEdit.Properties.ShowNullValuePrompt =
                    ShowNullValuePromptOptions.Default;

                return;
            }

            // ============================================================
            // SELECT
            // ============================================================

            if (editor is ComboBoxEdit comboBoxEdit)
            {
                comboBoxEdit.Properties.NullValuePrompt =
                    placeholder;

                comboBoxEdit.Properties.ShowNullValuePrompt =
                    ShowNullValuePromptOptions.Default;

                return;
            }
        }
    }
}