
using DevExpress.XtraDiagram;
using DevExpress.XtraEditors;
using MermaidDiagram.Abstractions.Interaction;
using MermaidDiagram.Core.Layout.Contract;
using MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting;
using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;
using MermaidDiagram.DevExpress.Interaction;
using MermaidDiagram.DevExpress.Pipeline;
using System.ComponentModel;

namespace MermaidDiagram.DevExpress.Controls
{
    /// <summary>
    /// Control DevExpress dùng để hiển thị Mermaid diagram.
    ///
    /// MermaidDiagramControl là public facade của
    /// MermaidDiagram.DevExpress.
    ///
    /// Control chịu trách nhiệm:
    ///     - Quản lý DiagramControl.
    ///     - Giữ MermaidDocument hiện tại.
    ///     - Giữ MermaidLayoutResult hiện tại.
    ///     - Giữ cấu hình layout.
    ///     - Giữ cấu hình interaction.
    ///     - Quản lý MermaidNodeInteractionHandler.
    ///     - Expose viewport dimensions.
    ///     - Expose interaction events cho application.
    ///
    /// Control KHÔNG chịu trách nhiệm:
    ///     - Parse Mermaid.
    ///     - Graph analysis.
    ///     - Layout calculation.
    ///     - Mapping.
    ///     - Rendering logic.
    ///     - Workflow dispatch.
    ///     - Business logic.
    ///     - Hit testing.
    ///     - Node metadata extraction.
    /// </summary>
    public sealed class MermaidDiagramControl :
        XtraUserControl
    {
        // ============================================================
        // DEVEXPRESS
        // ============================================================

        private readonly DiagramControl
            _diagramControl;

        /// <summary>
        /// DiagramControl nội bộ của DevExpress.
        ///
        /// Chỉ các thành phần trong
        /// MermaidDiagram.DevExpress được truy cập trực tiếp.
        /// </summary>
        internal DiagramControl DiagramControl
        {
            get
            {
                return _diagramControl;
            }
        }

        // ============================================================
        // PIPELINE
        // ============================================================

        private readonly IMermaidDiagramPipeline
            _pipeline;

        // ============================================================
        // DOCUMENT STATE
        // ============================================================

        private MermaidDocument? _document;

        /// <summary>
        /// MermaidDocument hiện tại.
        ///
        /// Null khi:
        ///     - chưa Load;
        ///     - hoặc đã Clear.
        /// </summary>
        public MermaidDocument? Document
        {
            get
            {
                return _document;
            }
        }

        // ============================================================
        // LAYOUT STATE
        // ============================================================

        private MermaidLayoutResult? _layoutResult;
        // ============================================================
        // EDGE ROUTING OPTIONS
        // ============================================================

        private EdgeRoutingOptions
            _edgeRoutingOptions;
        /// <summary>
        /// Cấu hình routing edge.
        ///
        /// Control chỉ lưu cấu hình.
        /// EdgeRoutingEngine thuộc Core Layout mới chịu trách nhiệm
        /// tính toán route thực tế.
        ///
        /// Không thực hiện routing tại Control.
        /// </summary>
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public EdgeRoutingOptions EdgeRoutingOptions
        {
            get
            {
                return _edgeRoutingOptions;
            }

            set
            {
                _edgeRoutingOptions =
                    value
                    ?? throw new ArgumentNullException(
                        nameof(value));
            }
        }
        /// <summary>
        /// Layout result hiện tại của diagram.
        ///
        /// Được tạo bởi MermaidDiagram.Core.
        ///
        /// Control chỉ giữ reference,
        /// không tự tính lại layout.
        /// </summary>
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public MermaidLayoutResult? LayoutResult
        {
            get
            {
                return _layoutResult;
            }
        }

        // ============================================================
        // LAYOUT OPTIONS
        // ============================================================

        private MermaidLayoutDirection
            _layoutDirection;

        /// <summary>
        /// Hướng layout của diagram.
        /// </summary>
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public MermaidLayoutDirection LayoutDirection
        {
            get
            {
                return _layoutDirection;
            }

            set
            {
                _layoutDirection =
                    value;
            }
        }

        private MermaidLayoutOptions
            _layoutOptions;

        /// <summary>
        /// Cấu hình layout.
        /// </summary>
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public MermaidLayoutOptions LayoutOptions
        {
            get
            {
                return _layoutOptions;
            }

            set
            {
                _layoutOptions =
                    value
                    ?? throw new ArgumentNullException(
                        nameof(value));
            }
        }

        // ============================================================
        // VIEWPORT
        // ============================================================

        /// <summary>
        /// Chiều rộng viewport hiện tại.
        /// </summary>
        public float ViewportWidth
        {
            get
            {
                return Math.Max(
                    0f,
                    ClientSize.Width);
            }
        }

        /// <summary>
        /// Chiều cao viewport hiện tại.
        /// </summary>
        public float ViewportHeight
        {
            get
            {
                return Math.Max(
                    0f,
                    ClientSize.Height);
            }
        }

        // ============================================================
        // INTERACTION OPTIONS
        // ============================================================

        private MermaidInteractionOptions
            _interactionOptions;

        /// <summary>
        /// Cấu hình interaction hiện tại.
        /// </summary>
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public MermaidInteractionOptions
            InteractionOptions
        {
            get
            {
                return _interactionOptions;
            }
        }

        // ============================================================
        // INTERACTION HANDLER
        // ============================================================

        private readonly
            MermaidNodeInteractionHandler
            _interactionHandler;

        // ============================================================
        // EVENTS
        // ============================================================

        /// <summary>
        /// Xảy ra khi người dùng click vào Mermaid node.
        /// </summary>
        public event
            EventHandler<MermaidNodeClickEventArgs>?
            NodeClicked;

        /// <summary>
        /// Xảy ra khi trạng thái hover của Mermaid node thay đổi.
        ///
        /// IsEntering = true:
        ///     mouse enter node.
        ///
        /// IsEntering = false:
        ///     mouse leave node.
        ///
        /// Lưu ý:
        ///     Event args hiện tại chỉ được định nghĩa cho
        ///     MermaidNodeHoverEventArgs.
        /// </summary>
        public event
            EventHandler<MermaidNodeHoverEventArgs>?
            NodeHovered;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidDiagramControl(
            IMermaidDiagramPipeline pipeline)
        {
            _pipeline =
                pipeline
                ?? throw new ArgumentNullException(
                    nameof(pipeline));

            // --------------------------------------------------------
            // STATE
            // --------------------------------------------------------

            _document =
                null;

            _layoutResult =
                null;

            _interactionOptions =
                new MermaidInteractionOptions();

            _layoutDirection =
                MermaidLayoutDirection.TopToBottom;

            _layoutOptions =
                new MermaidLayoutOptions();
            _edgeRoutingOptions =
             new EdgeRoutingOptions();
            // --------------------------------------------------------
            // DEVEXPRESS
            // --------------------------------------------------------

            _diagramControl =
                new DiagramControl();

            // --------------------------------------------------------
            // UI
            // --------------------------------------------------------

            InitializeControl();

            // --------------------------------------------------------
            // INTERACTION
            // --------------------------------------------------------

            _interactionHandler =
                new MermaidNodeInteractionHandler(
                    this);
        }

        // ============================================================
        // INITIALIZE
        // ============================================================

        private void InitializeControl()
        {
            SuspendLayout();

            try
            {
                // ----------------------------------------------------
                // CONTROL
                // ----------------------------------------------------

                Name =
                    "mermaidDiagramControl";

                Dock =
                    DockStyle.Fill;

                // ----------------------------------------------------
                // DIAGRAM CONTROL
                // ----------------------------------------------------

                _diagramControl.Name =
                    "diagramControl";

                _diagramControl.Dock =
                    DockStyle.Fill;

                _diagramControl.OptionsView.ShowGrid =
                    false;

                _diagramControl.OptionsView.ShowRulers =
                    false;

                Controls.Add(
                    _diagramControl);
            }
            finally
            {
                ResumeLayout(
                    false);
            }
        }

        // ============================================================
        // LOAD SOURCE
        // ============================================================

        /// <summary>
        /// Parse và render Mermaid source.
        ///
        /// Toàn bộ quá trình được thực hiện bởi pipeline.
        /// </summary>
        public void Load(
            string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                Clear();

                return;
            }

            _pipeline.Render(
                this,
                source);
        }

        // ============================================================
        // LOAD DOCUMENT
        // ============================================================

        /// <summary>
        /// Render MermaidDocument đã được parse.
        ///
        /// Pipeline chịu trách nhiệm:
        ///
        ///     Document
        ///         ↓
        ///     Graph
        ///         ↓
        ///     Analysis
        ///         ↓
        ///     Layout
        ///         ↓
        ///     Viewport
        ///         ↓
        ///     DevExpress
        /// </summary>
        public void Load(
            MermaidDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(
                    nameof(document));
            }

            _pipeline.Render(
                this,
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
            _pipeline.Clear(
                this);
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
            MermaidDocument? document =
                _document;

            if (document == null)
            {
                Clear();

                return;
            }

            _pipeline.Render(
                this,
                document);
        }

        // ============================================================
        // INTERACTION OPTIONS
        // ============================================================

        /// <summary>
        /// Cập nhật cấu hình interaction.
        /// </summary>
        public void SetInteractionOptions(
            MermaidInteractionOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(
                    nameof(options));
            }

            _interactionOptions =
                options;
        }

        // ============================================================
        // INTERNAL STATE
        // ============================================================

        /// <summary>
        /// Pipeline sử dụng method này để cập nhật
        /// document hiện tại sau khi Render.
        /// </summary>
        internal void SetDocument(
            MermaidDocument document)
        {
            _document =
                document
                ?? throw new ArgumentNullException(
                    nameof(document));
        }

        /// <summary>
        /// Pipeline sử dụng method này để cập nhật
        /// layout result hiện tại.
        /// </summary>
        internal void SetLayoutResult(
            MermaidLayoutResult layoutResult)
        {
            _layoutResult =
                layoutResult
                ?? throw new ArgumentNullException(
                    nameof(layoutResult));
        }

        /// <summary>
        /// Pipeline sử dụng method này khi Clear.
        /// </summary>
        internal void ClearDocument()
        {
            _document =
                null;

            _layoutResult =
                null;
        }

        // ============================================================
        // EVENT DISPATCH
        // ============================================================

        /// <summary>
        /// InteractionHandler sử dụng method này
        /// để phát click event ra application.
        /// </summary>
        internal void RaiseNodeClicked(
            MermaidNodeClickEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            NodeClicked?.Invoke(
                this,
                e);
        }

        /// <summary>
        /// InteractionHandler sử dụng method này
        /// để phát hover event ra application.
        /// </summary>
        internal void RaiseNodeHovered(
            MermaidNodeHoverEventArgs e)
        {
            if (e == null)
            {
                return;
            }

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
                // ----------------------------------------------------
                // INTERACTION
                // ----------------------------------------------------

                _interactionHandler.Dispose();

                // ----------------------------------------------------
                // DEVEXPRESS
                // ----------------------------------------------------

                _diagramControl.Dispose();
            }

            base.Dispose(
                disposing);
        }
    }
}


