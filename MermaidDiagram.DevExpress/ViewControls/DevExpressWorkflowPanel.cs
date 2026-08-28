
using DevExpress.XtraEditors;
using MermaidDiagram.Abstractions.Interaction;
using MermaidDiagram.Core.Models;
using MermaidDiagram.DevExpress.Controls;
using MermaidDiagram.DevExpress.Pipeline;
using MermaidDiagram.DevExpress.Services;

namespace MermaidDiagram.DevExpress.ViewControls
{
    /// <summary>
    /// Panel DevExpress dùng để hiển thị và điều khiển
    /// Mermaid workflow trong phạm vi một màn hình / region.
    ///
    /// Architecture:
    ///
    ///     DevExpressWorkflowPanel
    ///              │
    ///              ├── MermaidDiagramService
    ///              │        ↓
    ///              │   IMermaidDiagramPipeline
    ///              │
    ///              └── MermaidDiagramControl
    ///
    /// Trách nhiệm:
    ///     - Quản lý UI layout của workflow panel.
    ///     - Chứa MermaidDiagramControl.
    ///     - Điều phối các thao tác UI cấp panel.
    ///     - Cung cấp API Load / Clear / Refresh.
    ///     - Forward các interaction event từ diagram.
    ///
    /// Không chịu trách nhiệm:
    ///     - Parse Mermaid.
    ///     - Graph analysis.
    ///     - Layout calculation.
    ///     - Mapping.
    ///     - Rendering implementation.
    ///     - Tính geometry.
    ///     - Viewport algorithm.
    ///     - Workflow / business logic.
    ///
    /// MermaidDiagramControl vẫn là public facade
    /// của MermaidDiagram.DevExpress.
    /// </summary>
    public sealed class DevExpressWorkflowPanel
        : XtraUserControl
    {
        // ============================================================
        // SERVICE
        // ============================================================

        private readonly MermaidDiagramService _diagramService;

        // ============================================================
        // DIAGRAM
        // ============================================================

        private readonly MermaidDiagramControl _diagramControl;

        // ============================================================
        // TOOLBAR
        // ============================================================

        private readonly PanelControl _toolbar;

        private readonly SimpleButton _btnRefresh;

        private readonly SimpleButton _btnClear;

        // ============================================================
        // EVENTS
        // ============================================================

        /// <summary>
        /// Forward event click node từ MermaidDiagramControl.
        /// </summary>
        public event EventHandler<MermaidNodeClickEventArgs>?
            NodeClicked;

        /// <summary>
        /// Forward event hover node từ MermaidDiagramControl.
        /// </summary>
        public event EventHandler<MermaidNodeHoverEventArgs>?
            NodeHovered;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        /// <summary>
        /// Constructor chính.
        ///
        /// Application cung cấp MermaidDiagramService.
        /// </summary>
        public DevExpressWorkflowPanel(
            MermaidDiagramService diagramService)
        {
            _diagramService =
                diagramService
                ?? throw new ArgumentNullException(
                    nameof(diagramService));

            _diagramControl =
                CreateDiagramControl();

            _toolbar =
                new PanelControl();

            _btnRefresh =
                CreateButton(
                    "Refresh");

            _btnClear =
                CreateButton(
                    "Clear");

            InitializeComponent();

            SubscribeEvents();
        }

        // ============================================================
        // CONSTRUCTOR - DEFAULT
        // ============================================================

        /// <summary>
        /// Constructor tiện dụng cho demo / application đơn giản.
        ///
        /// Composition:
        ///
        /// MermaidDiagramPipelineFactory
        ///          ↓
        /// MermaidDiagramService
        ///          ↓
        /// MermaidDiagramControl
        /// </summary>
        public DevExpressWorkflowPanel()
            : this(
                new MermaidDiagramService(
                    MermaidDiagramPipelineFactory.Create()))
        {
        }

        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        /// <summary>
        /// MermaidDiagramControl bên trong panel.
        ///
        /// Dùng khi application cần cấu hình trực tiếp
        /// các option thuộc diagram control.
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
        /// Parse và render Mermaid source.
        /// </summary>
        public void Load(
            string source)
        {
            _diagramService.Load(
                _diagramControl,
                source);
        }

        // ============================================================
        // LOAD DOCUMENT
        // ============================================================

        /// <summary>
        /// Render MermaidDocument đã được parse.
        /// </summary>
        public void Load(
            MermaidDocument document)
        {
            _diagramService.Load(
                _diagramControl,
                document);
        }

        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Xóa workflow hiện tại.
        /// </summary>
        public void Clear()
        {
            _diagramService.Clear(
                _diagramControl);
        }

        // ============================================================
        // REFRESH
        // ============================================================

        /// <summary>
        /// Render lại MermaidDocument hiện tại.
        ///
        /// Không parse lại source.
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
            // CONTROL
            // ========================================================

            Name =
                "DevExpressWorkflowPanel";

            Dock =
                DockStyle.Fill;

            // ========================================================
            // TOOLBAR
            // ========================================================

            _toolbar.Dock =
                DockStyle.Top;

            _toolbar.Height =
                46;

            // ========================================================
            // BUTTON REFRESH
            // ========================================================

            _btnRefresh.Left =
                8;

            // ========================================================
            // BUTTON CLEAR
            // ========================================================

            _btnClear.Left =
                _btnRefresh.Right + 6;

            // ========================================================
            // TOOLBAR CONTROLS
            // ========================================================

            _toolbar.Controls.Add(
                _btnRefresh);

            _toolbar.Controls.Add(
                _btnClear);

            // ========================================================
            // DIAGRAM
            // ========================================================

            _diagramControl.Dock =
                DockStyle.Fill;

            // ========================================================
            // PANEL CONTROLS
            // ========================================================

            Controls.Add(
                _diagramControl);

            Controls.Add(
                _toolbar);

            ResumeLayout(
                false);
        }

        // ============================================================
        // CREATE DIAGRAM CONTROL
        // ============================================================

        private MermaidDiagramControl CreateDiagramControl()
        {
            return new MermaidDiagramControl(
                GetPipeline());
        }

        // ============================================================
        // PIPELINE
        // ============================================================

        private IMermaidDiagramPipeline GetPipeline()
        {
            /*
             * MermaidDiagramControl bắt buộc nhận
             * IMermaidDiagramPipeline.
             *
             * Service đã giữ pipeline nhưng hiện tại
             * MermaidDiagramService không expose pipeline.
             *
             * Vì vậy không tạo MermaidDiagramControl
             * qua Service.
             */

            throw new InvalidOperationException(
                "DevExpressWorkflowPanel cần được khởi tạo " +
                "với MermaidDiagramControl đã được tạo từ " +
                "IMermaidDiagramPipeline.");
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

        private void SubscribeEvents()
        {
            _btnRefresh.Click +=
                OnRefreshClick;

            _btnClear.Click +=
                OnClearClick;

            _diagramControl.NodeClicked +=
                OnNodeClicked;

            _diagramControl.NodeHovered +=
                OnNodeHovered;
        }

        private void OnRefreshClick(
            object? sender,
            EventArgs e)
        {
            RefreshDiagram();
        }

        private void OnClearClick(
            object? sender,
            EventArgs e)
        {
            Clear();
        }

        private void OnNodeClicked(
            object? sender,
            MermaidNodeClickEventArgs e)
        {
            NodeClicked?.Invoke(
                this,
                e);
        }

        private void OnNodeHovered(
            object? sender,
            MermaidNodeHoverEventArgs e)
        {
            NodeHovered?.Invoke(
                this,
                e);
        }

        // ============================================================
        // DISPOSE
        // ============================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _btnRefresh.Click -=
                    OnRefreshClick;

                _btnClear.Click -=
                    OnClearClick;

                _diagramControl.NodeClicked -=
                    OnNodeClicked;

                _diagramControl.NodeHovered -=
                    OnNodeHovered;

                _diagramControl.Dispose();
            }

            base.Dispose(
                disposing);
        }
    }
}

