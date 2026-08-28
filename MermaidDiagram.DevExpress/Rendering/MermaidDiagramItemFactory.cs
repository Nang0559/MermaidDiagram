using DevExpress.XtraDiagram;
using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;
using MermaidDiagram.DevExpress.Mapping;

namespace MermaidDiagram.DevExpress.Rendering
{
    /// <summary>
    /// Factory tạo các DevExpress DiagramItem từ model Mermaid.
    ///
    /// Trách nhiệm duy nhất:
    ///     - Tạo DiagramShape cho MermaidNode.
    ///     - Tạo DiagramConnector cho MermaidEdge.
    ///     - Tạo DiagramContainer cho MermaidSubgraph.
    ///     - Gắn application data vào DiagramItem.
    ///     - Áp dụng shape / edge style / layout thông qua mapper.
    ///
    /// Không chịu trách nhiệm:
    ///     - Parse Mermaid.
    ///     - Phân tích graph.
    ///     - Tính layout.
    ///     - Tính spacing.
    ///     - Collision resolution.
    ///     - Viewport fitting.
    ///     - Node visual style.
    ///     - Interaction.
    ///     - Workflow / business logic.
    ///
    /// Visual style của node thuộc:
    ///     MermaidNodeRenderer
    ///     MermaidNodeStyleMapper
    /// </summary>
    public sealed class MermaidDiagramItemFactory
    {
        // ============================================================
        // MAPPER
        // ============================================================

        private readonly IMermaidDevExpressMapper _mapper;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidDiagramItemFactory(
            IMermaidDevExpressMapper mapper)
        {
            _mapper =
                mapper
                ?? throw new ArgumentNullException(
                    nameof(mapper));
        }

        // ============================================================
        // NODE
        // ============================================================

        /// <summary>
        /// Tạo DiagramShape cơ bản từ MermaidNode.
        ///
        /// Không áp dụng layout.
        /// Không áp dụng visual style.
        /// </summary>
        public DiagramShape CreateNode(
            MermaidNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(
                    nameof(node));
            }

            DiagramShape shape =
                new DiagramShape();

            // --------------------------------------------------------
            // IDENTITY
            // --------------------------------------------------------

            shape.Tag =
                node.Id;

            // --------------------------------------------------------
            // APPLICATION DATA
            // --------------------------------------------------------

            shape.DataContext =
                node;

            // --------------------------------------------------------
            // TEXT
            // --------------------------------------------------------

            shape.Content =
                GetNodeText(node);

            // --------------------------------------------------------
            // SHAPE
            // --------------------------------------------------------

            shape.Shape =
                _mapper.MapShape(
                    node.Shape);

            // --------------------------------------------------------
            // COMMON BEHAVIOR
            // --------------------------------------------------------

            shape.CanMove =
                true;

            shape.CanResize =
                true;

            shape.CanRotate =
                false;

            shape.CanSelect =
                true;

            return shape;
        }

        // ============================================================
        // NODE WITH LAYOUT
        // ============================================================

        /// <summary>
        /// Tạo DiagramShape và áp dụng geometry
        /// đã được MermaidDiagram.Core tính toán.
        ///
        /// Factory không tự tính:
        ///     - X
        ///     - Y
        ///     - Width
        ///     - Height
        /// </summary>
        public DiagramShape CreateNode(
            MermaidNode node,
            MermaidLayoutItem layoutItem)
        {
            if (node == null)
            {
                throw new ArgumentNullException(
                    nameof(node));
            }

            if (layoutItem == null)
            {
                throw new ArgumentNullException(
                    nameof(layoutItem));
            }

            DiagramShape shape =
                CreateNode(node);

            _mapper.ApplyLayout(
                shape,
                layoutItem);

            return shape;
        }

        // ============================================================
        // EDGE
        // ============================================================

        /// <summary>
        /// Tạo DiagramConnector nối hai DiagramItem.
        ///
        /// Style lấy từ:
        ///
        ///     MermaidEdge.Style
        ///
        /// Không sử dụng MermaidEdgeType.
        /// </summary>
        public DiagramConnector CreateEdge(
            MermaidEdge edge,
            DiagramItem beginItem,
            DiagramItem endItem)
        {
            if (edge == null)
            {
                throw new ArgumentNullException(
                    nameof(edge));
            }

            if (beginItem == null)
            {
                throw new ArgumentNullException(
                    nameof(beginItem));
            }

            if (endItem == null)
            {
                throw new ArgumentNullException(
                    nameof(endItem));
            }

            DiagramConnector connector =
                new DiagramConnector();

            // --------------------------------------------------------
            // CONNECTION
            // --------------------------------------------------------

            connector.BeginItem =
                beginItem;

            connector.EndItem =
                endItem;

            // --------------------------------------------------------
            // LABEL
            // --------------------------------------------------------

            connector.Content =
                edge.Label ??
                string.Empty;

            // --------------------------------------------------------
            // EDGE STYLE
            // --------------------------------------------------------

            _mapper.ApplyEdgeStyle(
                connector,
                edge.Style);

            // --------------------------------------------------------
            // APPLICATION DATA
            // --------------------------------------------------------

            connector.Tag =
                edge;

            connector.DataContext =
                edge;

            return connector;
        }

        // ============================================================
        // GROUP
        // ============================================================

        /// <summary>
        /// Tạo DiagramContainer cơ bản từ MermaidSubgraph.
        ///
        /// Không tự tính geometry.
        /// </summary>
        public DiagramContainer CreateGroup(
            MermaidSubgraph subgraph)
        {
            if (subgraph == null)
            {
                throw new ArgumentNullException(
                    nameof(subgraph));
            }

            DiagramContainer container =
                new DiagramContainer();

            // --------------------------------------------------------
            // IDENTITY
            // --------------------------------------------------------

            container.Tag =
                subgraph.Key;

            // --------------------------------------------------------
            // APPLICATION DATA
            // --------------------------------------------------------

            container.DataContext =
                subgraph;

            // --------------------------------------------------------
            // HEADER
            // --------------------------------------------------------

            container.Header =
                GetGroupHeader(
                    subgraph);

            container.ShowHeader =
                true;

            // --------------------------------------------------------
            // GROUP BEHAVIOR
            // --------------------------------------------------------

            container.ItemsCanMove =
                true;

            container.ItemsCanSelect =
                true;

            container.ItemsCanResize =
                true;

            container.ItemsCanRotate =
                false;

            container.ItemsCanEdit =
                true;

            container.MoveWithSubordinates =
                true;

            return container;
        }

        // ============================================================
        // GROUP WITH LAYOUT
        // ============================================================

        /// <summary>
        /// Tạo DiagramContainer và áp dụng geometry
        /// do MermaidDiagram.Core tính toán.
        ///
        /// Không hard-code:
        ///     - X
        ///     - Y
        ///     - Width
        ///     - Height
        /// </summary>
        public DiagramContainer CreateGroup(
            MermaidSubgraph subgraph,
            MermaidLayoutItem layoutItem)
        {
            if (subgraph == null)
            {
                throw new ArgumentNullException(
                    nameof(subgraph));
            }

            if (layoutItem == null)
            {
                throw new ArgumentNullException(
                    nameof(layoutItem));
            }

            DiagramContainer container =
                CreateGroup(
                    subgraph);

            _mapper.ApplyLayout(
                container,
                layoutItem);

            return container;
        }

        // ============================================================
        // NODE TEXT
        // ============================================================

        private static string GetNodeText(
            MermaidNode node)
        {
            if (!string.IsNullOrWhiteSpace(
                    node.Text))
            {
                return node.Text;
            }

            return node.Id ??
                   string.Empty;
        }

        // ============================================================
        // GROUP HEADER
        // ============================================================

        private static string GetGroupHeader(
            MermaidSubgraph subgraph)
        {
            if (!string.IsNullOrWhiteSpace(
                    subgraph.Title))
            {
                return subgraph.Title;
            }

            return subgraph.Key ??
                   string.Empty;
        }
    }
}