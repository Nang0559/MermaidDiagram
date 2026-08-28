using System;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace MermaidDiagram.DevExpress.ViewRendering
{
    /// <summary>
    /// Factory tạo các DevExpress / WinForms control
    /// phục vụ Workflow View Rendering.
    ///
    /// Trách nhiệm:
    ///     - tạo control;
    ///     - cấu hình thuộc tính cơ bản;
    ///     - cung cấp control cho các renderer khác.
    ///
    /// Không chịu trách nhiệm:
    ///     - render WorkflowView;
    ///     - render field;
    ///     - render action;
    ///     - render validation;
    ///     - render collection;
    ///     - navigation;
    ///     - workflow dispatch;
    ///     - business logic.
    /// </summary>
    public sealed class DevExpressControlFactory
    {
        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public DevExpressControlFactory()
        {
        }

        // ============================================================
        // PANEL
        // ============================================================

        /// <summary>
        /// Tạo panel chứa nội dung Workflow View.
        ///
        /// Không sử dụng ScrollableControlContainer.
        /// Scroll được thực hiện bằng AutoScroll của PanelControl.
        /// </summary>
        public PanelControl CreateViewPanel()
        {
            PanelControl panel =
                new PanelControl();

            panel.Dock =
                DockStyle.Fill;

            panel.AutoScroll =
                true;

            return panel;
        }

        // ============================================================
        // FIELD PANEL
        // ============================================================

        /// <summary>
        /// Tạo container cho một WorkflowField.
        /// </summary>
        public PanelControl CreateFieldPanel()
        {
            PanelControl panel =
                new PanelControl();

            panel.Dock =
                DockStyle.Top;

            panel.AutoSize =
                true;

            panel.Padding =
                new Padding(
                    8,
                    6,
                    8,
                    6);

            return panel;
        }

        // ============================================================
        // LABEL
        // ============================================================

        /// <summary>
        /// Tạo label hiển thị caption.
        /// </summary>
        public LabelControl CreateLabel(
            string text)
        {
            LabelControl label =
                new LabelControl();

            label.Text =
                text ?? string.Empty;

            label.AutoSizeMode =
                LabelAutoSizeMode.None;

            label.Dock =
                DockStyle.Top;

            label.Height =
                24;

            return label;
        }

        // ============================================================
        // TEXT EDIT
        // ============================================================

        /// <summary>
        /// Tạo TextEdit cho field text.
        /// </summary>
        public TextEdit CreateTextEdit()
        {
            TextEdit editor =
                new TextEdit();

            editor.Dock =
                DockStyle.Top;

            return editor;
        }

        // ============================================================
        // MEMO EDIT
        // ============================================================

        /// <summary>
        /// Tạo MemoEdit cho field multiline.
        /// </summary>
        public MemoEdit CreateMemoEdit()
        {
            MemoEdit editor =
                new MemoEdit();

            editor.Dock =
                DockStyle.Top;

            editor.Properties.ScrollBars =
                ScrollBars.Vertical;

            editor.Properties.AcceptsReturn =
                true;

            editor.Height =
                80;

            return editor;
        }

        // ============================================================
        // BUTTON
        // ============================================================

        /// <summary>
        /// Tạo SimpleButton cho WorkflowAction.
        /// </summary>
        public SimpleButton CreateButton(
            string text)
        {
            SimpleButton button =
                new SimpleButton();

            button.Text =
                text ?? string.Empty;

            button.AutoSize =
                true;

            button.MinimumSize =
                new Size(
                    90,
                    32);

            return button;
        }

        // ============================================================
        // ACTION PANEL
        // ============================================================

        /// <summary>
        /// Tạo panel chứa các WorkflowAction.
        /// </summary>
        public PanelControl CreateActionPanel()
        {
            PanelControl panel =
                new PanelControl();

            panel.Dock =
                DockStyle.Top;

            panel.AutoSize =
                true;

            panel.Padding =
                new Padding(
                    8);

            return panel;
        }

        // ============================================================
        // COLLECTION PANEL
        // ============================================================

        /// <summary>
        /// Tạo panel chứa WorkflowCollection.
        /// </summary>
        public PanelControl CreateCollectionPanel()
        {
            PanelControl panel =
                new PanelControl();

            panel.Dock =
                DockStyle.Top;

            panel.AutoSize =
                true;

            panel.Padding =
                new Padding(
                    4);

            return panel;
        }

        // ============================================================
        // VALIDATION PANEL
        // ============================================================

        /// <summary>
        /// Tạo panel hiển thị validation messages.
        /// </summary>
        public PanelControl CreateValidationPanel()
        {
            PanelControl panel =
                new PanelControl();

            panel.Dock =
                DockStyle.Top;

            panel.AutoSize =
                true;

            panel.Padding =
                new Padding(
                    8,
                    4,
                    8,
                    4);

            panel.Visible =
                false;

            return panel;
        }

        // ============================================================
        // FORM CONTENT
        // ============================================================

        /// <summary>
        /// Tạo container chính cho nội dung Workflow Form.
        ///
        /// Đây là PanelControl thông thường,
        /// không phải ScrollableControlContainer.
        /// </summary>
        public PanelControl CreateFormContent()
        {
            PanelControl panel =
                new PanelControl();

            panel.Dock =
                DockStyle.Fill;

            panel.AutoScroll =
                true;

            return panel;
        }

        // ============================================================
        // SEPARATOR
        // ============================================================

        /// <summary>
        /// Tạo khoảng phân cách giữa các khu vực.
        /// </summary>
        public Control CreateSeparator(
            int height = 8)
        {
            Panel separator =
                new Panel();

            separator.Dock =
                DockStyle.Top;

            separator.Height =
                Math.Max(
                    0,
                    height);

            return separator;
        }
    }
}