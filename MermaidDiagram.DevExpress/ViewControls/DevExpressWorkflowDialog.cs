
using DevExpress.XtraEditors;
using MermaidDiagram.Core.Models;
using MermaidDiagram.DevExpress.Controls;
using MermaidDiagram.DevExpress.Pipeline;

namespace MermaidDiagram.DevExpress.ViewControls
{
    /// <summary>
    /// Dialog DevExpress dùng để hiển thị Mermaid workflow.
    ///
    /// Trách nhiệm:
    ///     - Quản lý lifecycle của MermaidDiagramControl.
    ///     - Tạo UI dialog.
    ///     - Cung cấp API Load Mermaid source.
    ///     - Cung cấp API Load MermaidDocument.
    ///     - Clear diagram.
    ///
    /// Không chịu trách nhiệm:
    ///     - Parse Mermaid.
    ///     - Graph analysis.
    ///     - Layout.
    ///     - Mapping.
    ///     - Rendering.
    ///     - Viewport algorithm.
    ///     - Interaction algorithm.
    ///     - Workflow / business logic.
    ///
    /// Architecture:
    ///
    /// DevExpressWorkflowDialog
    ///          ↓
    /// MermaidDiagramControl
    ///          ↓
    /// IMermaidDiagramPipeline
    ///          ↓
    /// Core + Rendering
    /// </summary>
    public sealed class DevExpressWorkflowDialog
        : XtraForm
    {
        // ============================================================
        // CONTROLS
        // ============================================================

        private readonly MermaidDiagramControl _diagramControl;

        // ============================================================
        // TOOLBAR
        // ============================================================

        private readonly PanelControl _toolbar;

        private readonly SimpleButton _btnClear;

        private readonly SimpleButton _btnRefresh;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        /// <summary>
        /// Tạo dialog bằng pipeline đã được application cung cấp.
        ///
        /// Đây là constructor chính.
        /// </summary>
        public DevExpressWorkflowDialog(
            IMermaidDiagramPipeline pipeline)
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException(
                    nameof(pipeline));
            }

            _diagramControl =
                new MermaidDiagramControl(
                    pipeline);

            _toolbar =
                new PanelControl();

            _btnClear =
                CreateButton(
                    "Clear");

            _btnRefresh =
                CreateButton(
                    "Refresh");

            InitializeComponent();
        }

        /// <summary>
        /// Constructor tiện dụng dùng composition root mặc định.
        ///
        /// Dùng cho demo / test / application đơn giản.
        /// </summary>
        public DevExpressWorkflowDialog()
            : this(
                MermaidDiagramPipelineFactory.Create())
        {
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// MermaidDiagramControl mà dialog đang quản lý.
        ///
        /// Application có thể dùng facade này để:
        ///     - Load;
        ///     - Clear;
        ///     - Refresh;
        ///     - cấu hình interaction;
        ///     - subscribe event.
        /// </summary>
        public MermaidDiagramControl DiagramControl
        {
            get
            {
                return _diagramControl;
            }
        }

        // ============================================================
        // LOAD SOURCE
        // ============================================================

        /// <summary>
        /// Load Mermaid source vào diagram.
        /// </summary>
        public void Load(
            string source)
        {
            _diagramControl.Load(
                source);
        }

        // ============================================================
        // LOAD DOCUMENT
        // ============================================================

        /// <summary>
        /// Load MermaidDocument đã được parse.
        /// </summary>
        public void Load(
            MermaidDocument document)
        {
            _diagramControl.Load(
                document);
        }

        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Xóa diagram hiện tại.
        /// </summary>
        public void Clear()
        {
            _diagramControl.Clear();
        }

        // ============================================================
        // REFRESH
        // ============================================================

        /// <summary>
        /// Render lại MermaidDocument hiện tại.
        /// </summary>
        public void RefreshDiagram()
        {
            _diagramControl.RefreshDiagram();
        }

        // ============================================================
        // INITIALIZE
        // ============================================================

        private void InitializeComponent()
        {
            SuspendLayout();

            // ========================================================
            // FORM
            // ========================================================

            Name =
                "DevExpressWorkflowDialog";

            Text =
                "Mermaid Workflow";

            StartPosition =
                FormStartPosition.CenterParent;

            WindowState =
                FormWindowState.Maximized;

            MinimumSize =
                new Size(
                    900,
                    600);

            // ========================================================
            // TOOLBAR
            // ========================================================

            _toolbar.Dock =
                DockStyle.Top;

            _toolbar.Height =
                46;

            // ========================================================
            // BUTTONS
            // ========================================================

            _btnClear.Left =
                8;

            _btnRefresh.Left =
                _btnClear.Right + 6;

            _toolbar.Controls.Add(
                _btnClear);

            _toolbar.Controls.Add(
                _btnRefresh);

            // ========================================================
            // EVENTS
            // ========================================================

            _btnClear.Click +=
                OnClearClick;

            _btnRefresh.Click +=
                OnRefreshClick;

            // ========================================================
            // DIAGRAM
            // ========================================================

            _diagramControl.Dock =
                DockStyle.Fill;

            // ========================================================
            // FORM
            // ========================================================

            Controls.Add(
                _diagramControl);

            Controls.Add(
                _toolbar);

            ResumeLayout(
                false);
        }

        // ============================================================
        // BUTTON FACTORY
        // ============================================================

        private static SimpleButton CreateButton(
            string text)
        {
            SimpleButton button =
                new SimpleButton();

            button.Text =
                text;

            button.Width =
                80;

            button.Height =
                30;

            button.Top =
                8;

            return button;
        }

        // ============================================================
        // EVENTS
        // ============================================================

        private void OnClearClick(
            object? sender,
            EventArgs e)
        {
            Clear();
        }

        private void OnRefreshClick(
            object? sender,
            EventArgs e)
        {
            RefreshDiagram();
        }

        // ============================================================
        // DISPOSE
        // ============================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _diagramControl.Dispose();
            }

            base.Dispose(
                disposing);
        }
    }
}

