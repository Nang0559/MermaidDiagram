using DevExpress.XtraDiagram;
using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;

namespace MermaidDiagram.DevExpress.Rendering
{
    /// <summary>
    /// Render một MermaidNode thành DiagramShape hoàn chỉnh.
    ///
    /// Trách nhiệm:
    ///     - yêu cầu Factory tạo DiagramShape.
    ///     - áp dụng node visual style.
    ///
    /// Không chịu trách nhiệm:
    ///     - Parse Mermaid.
    ///     - Graph analysis.
    ///     - Layout calculation.
    ///     - Spacing.
    ///     - Collision resolution.
    ///     - Edge rendering.
    ///     - Group rendering.
    ///     - Viewport fitting.
    ///     - Interaction.
    ///     - Workflow / business logic.
    ///
    /// Dummy node:
    ///     - Không được render.
    ///     - Dummy chỉ tồn tại trong Core Layout để làm waypoint
    ///       cho edge vượt nhiều layer.
    /// </summary>
    public sealed class MermaidNodeRenderer
    {
        // ============================================================
        // FACTORY
        // ============================================================

        private readonly MermaidDiagramItemFactory _itemFactory;

        // ============================================================
        // STYLE MAPPER
        // ============================================================

        private readonly MermaidNodeStyleMapper _styleMapper;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidNodeRenderer(
            MermaidDiagramItemFactory itemFactory)
            : this(
                itemFactory,
                new MermaidNodeStyleMapper())
        {
        }

        public MermaidNodeRenderer(
            MermaidDiagramItemFactory itemFactory,
            MermaidNodeStyleMapper styleMapper)
        {
            _itemFactory =
                itemFactory
                ?? throw new ArgumentNullException(
                    nameof(itemFactory));

            _styleMapper =
                styleMapper
                ?? throw new ArgumentNullException(
                    nameof(styleMapper));
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Render node bằng geometry đã được Core layout tính toán.
        ///
        /// Dummy node không được render.
        /// </summary>
        public DiagramShape? Render(
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

            // --------------------------------------------------------
            // DUMMY NODE
            // --------------------------------------------------------
            //
            // Dummy chỉ phục vụ:
            //
            //     Layer
            //       ↓
            //     Ordering
            //       ↓
            //     Crossing
            //       ↓
            //     Coordinate
            //       ↓
            //     Edge Routing
            //
            // Không tương ứng với MermaidNode thật.
            //
            if (layoutItem.IsDummy)
            {
                return null;
            }

            // --------------------------------------------------------
            // FACTORY
            // --------------------------------------------------------

            DiagramShape shape =
                _itemFactory.CreateNode(
                    node,
                    layoutItem);

            // --------------------------------------------------------
            // VISUAL STYLE
            // --------------------------------------------------------

            _styleMapper.ApplyStyle(
                shape,
                node);

            return shape;
        }

        // ============================================================
        // PUBLIC API - WITHOUT LAYOUT
        // ============================================================

        /// <summary>
        /// Render node chưa có geometry layout.
        ///
        /// Method này chỉ dùng cho MermaidNode thật,
        /// không dùng cho dummy.
        /// </summary>
        public DiagramShape Render(
            MermaidNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(
                    nameof(node));
            }

            DiagramShape shape =
                _itemFactory.CreateNode(
                    node);

            _styleMapper.ApplyStyle(
                shape,
                node);

            return shape;
        }
    }
}