using DevExpress.XtraDiagram;
using MermaidDiagram.Abstractions.Interaction;
using MermaidDiagram.DevExpress.Controls;
using MermaidDiagram.DevExpress.Mapping;

namespace MermaidDiagram.DevExpress.Interaction
{
    /// <summary>
    /// Adapter chuyển interaction của DevExpress DiagramControl
    /// thành MermaidDiagram.Abstractions.Interaction contract.
    ///
    /// Trách nhiệm:
    ///     - nhận MouseClick từ DiagramControl;
    ///     - nhận MouseMove từ DiagramControl;
    ///     - xác định DiagramItem tại vị trí chuột;
    ///     - đọc MermaidDiagramNodeMetadata;
    ///     - chuyển thành MermaidNode interaction;
    ///     - gọi OnNodeClick / OnNodeHover / OnNodeLeave.
    ///
    /// Không chịu trách nhiệm:
    ///     - parse Mermaid;
    ///     - graph analysis;
    ///     - layout;
    ///     - rendering;
    ///     - workflow dispatch;
    ///     - navigation;
    ///     - business logic.
    /// </summary>
    public sealed class MermaidNodeInteractionHandler :
        IMermaidNodeInteractionHandler,
        IDisposable
    {
        // ============================================================
        // OWNER
        // ============================================================

        private readonly MermaidDiagramControl _owner;

        // ============================================================
        // DEVEXPRESS
        // ============================================================

        private DiagramControl DiagramControl
        {
            get
            {
                return _owner.DiagramControl;
            }
        }

        // ============================================================
        // HOVER STATE
        // ============================================================

        private DiagramItem? _hoveredItem;

        // ============================================================
        // STATE
        // ============================================================

        private bool _attached;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidNodeInteractionHandler(
            MermaidDiagramControl owner)
        {
            _owner =
                owner
                ?? throw new ArgumentNullException(
                    nameof(owner));

            Attach();
        }

        // ============================================================
        // ATTACH
        // ============================================================

        private void Attach()
        {
            if (_attached)
            {
                return;
            }

            DiagramControl.MouseClick +=
                DiagramControl_MouseClick;

            DiagramControl.MouseMove +=
                DiagramControl_MouseMove;

            _attached =
                true;
        }

        // ============================================================
        // CLICK
        // ============================================================

        private void DiagramControl_MouseClick(
            object? sender,
            MouseEventArgs e)
        {
            if (e.Button !=
                MouseButtons.Left)
            {
                return;
            }

            DiagramItem? item =
                HitTestItem(
                    e.Location);

            if (item == null)
            {
                return;
            }

            if (!TryGetNodeMetadata(
                    item,
                    out MermaidDiagramNodeMetadata metadata))
            {
                return;
            }

            MermaidNodeClickEventArgs args =
                CreateClickEventArgs(
                    metadata,
                    e);

            OnNodeClick(
                args);
        }

        // ============================================================
        // MOUSE MOVE
        // ============================================================

        private void DiagramControl_MouseMove(
            object? sender,
            MouseEventArgs e)
        {
            DiagramItem? currentItem =
                HitTestItem(
                    e.Location);

            // --------------------------------------------------------
            // SAME ITEM
            // --------------------------------------------------------

            if (ReferenceEquals(
                    currentItem,
                    _hoveredItem))
            {
                return;
            }

            // --------------------------------------------------------
            // LEAVE OLD ITEM
            // --------------------------------------------------------

            if (_hoveredItem != null)
            {
                if (TryGetNodeMetadata(
                        _hoveredItem,
                        out MermaidDiagramNodeMetadata oldMetadata))
                {
                    MermaidNodeEventArgs leaveArgs =
                        CreateLeaveEventArgs(
                            oldMetadata);

                    OnNodeLeave(
                        leaveArgs);
                }

                _hoveredItem =
                    null;
            }

            // --------------------------------------------------------
            // NO NEW ITEM
            // --------------------------------------------------------

            if (currentItem == null)
            {
                return;
            }

            // --------------------------------------------------------
            // GET METADATA
            // --------------------------------------------------------

            if (!TryGetNodeMetadata(
                    currentItem,
                    out MermaidDiagramNodeMetadata metadata))
            {
                return;
            }

            // --------------------------------------------------------
            // STORE HOVERED ITEM
            // --------------------------------------------------------

            _hoveredItem =
                currentItem;

            // --------------------------------------------------------
            // ENTER NEW ITEM
            // --------------------------------------------------------

            MermaidNodeHoverEventArgs hoverArgs =
                CreateHoverEventArgs(
                    metadata);

            OnNodeHover(
                hoverArgs);
        }

        // ============================================================
        // HIT TEST
        // ============================================================

        private DiagramItem? HitTestItem(
            Point location)
        {
            /*
             * DevExpress version hiện tại của project
             * không có DiagramItemMouseEventArgs cho
             * MouseClick / MouseMove.
             *
             * Vì vậy không tạo adapter giả cho type đó.
             *
             * Implementation hit-test cụ thể sẽ phụ thuộc
             * API DiagramControl của version DevExpress đang dùng.
             */

            return null;
        }

        // ============================================================
        // NODE METADATA
        // ============================================================

        private static bool TryGetNodeMetadata(
            DiagramItem item,
            out MermaidDiagramNodeMetadata metadata)
        {
            metadata = null!;

            if (item == null)
            {
                return false;
            }

            object? value =
                item.Tag;

            MermaidDiagramNodeMetadata? nodeMetadata =
                value as MermaidDiagramNodeMetadata;

            if (nodeMetadata == null)
            {
                return false;
            }

            metadata =
                nodeMetadata;

            return true;
        }

        // ============================================================
        // CLICK CONTRACT
        // ============================================================

        public void OnNodeClick(
            MermaidNodeClickEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            _owner.RaiseNodeClicked(
                e);
        }

        // ============================================================
        // HOVER CONTRACT
        // ============================================================

        public void OnNodeHover(
            MermaidNodeHoverEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            _owner.RaiseNodeHovered(
                e);
        }

        // ============================================================
        // LEAVE CONTRACT
        // ============================================================

        public void OnNodeLeave(
            MermaidNodeEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            /*
             * MermaidDiagramControl hiện tại chưa có
             * NodeLeft event.
             *
             * Vì vậy giữ đúng IMermaidNodeInteractionHandler
             * nhưng chưa dispatch ra application.
             */
        }

        // ============================================================
        // EVENT ARGS
        // ============================================================

        private static MermaidNodeClickEventArgs
            CreateClickEventArgs(
                MermaidDiagramNodeMetadata node,
                MouseEventArgs e)
        {
            bool isPrimaryClick =
                e.Button ==
                MouseButtons.Left;

            bool isDoubleClick =
                e.Clicks >= 2;

            return new MermaidNodeClickEventArgs(
                node.NodeKey,
                node.NodeText,
                isPrimaryClick,
                isDoubleClick);
        }

        private static MermaidNodeHoverEventArgs
            CreateHoverEventArgs(
                MermaidDiagramNodeMetadata node)
        {
            return new MermaidNodeHoverEventArgs(
                node.NodeKey,
                node.NodeText,
                node.WorkflowNodeKey);
        }

        private static MermaidNodeEventArgs
            CreateLeaveEventArgs(
                MermaidDiagramNodeMetadata node)
        {
            return new MermaidNodeEventArgs(
                node.NodeKey,
                node.NodeText,
                node.WorkflowNodeKey);
        }

        // ============================================================
        // DETACH
        // ============================================================

        private void Detach()
        {
            if (!_attached)
            {
                return;
            }

            DiagramControl.MouseClick -=
                DiagramControl_MouseClick;

            DiagramControl.MouseMove -=
                DiagramControl_MouseMove;

            _attached =
                false;

            _hoveredItem =
                null;
        }

        // ============================================================
        // DISPOSE
        // ============================================================

        public void Dispose()
        {
            Detach();
        }
    }
}