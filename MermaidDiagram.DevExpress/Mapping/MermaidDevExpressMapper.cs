using DevExpress.Diagram.Core;
using DevExpress.Utils;
using DevExpress.XtraDiagram;
using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;

namespace MermaidDiagram.DevExpress.Mapping
{
    /// <summary>
    /// Cầu nối giữa MermaidDiagram.Core và DevExpress Diagram.
    ///
    /// Chịu trách nhiệm:
    ///     - MermaidNodeShape -> ShapeDescription
    ///     - MermaidEdgeStyle -> DiagramConnector
    ///     - MermaidLayoutItem -> DiagramItem geometry
    ///     - Mermaid node text -> DiagramShape
    ///
    /// Không chịu trách nhiệm:
    ///     - Parse Mermaid
    ///     - Graph analysis
    ///     - Layout
    ///     - Tạo DiagramShape
    ///     - Tạo DiagramConnector
    ///     - Tạo DiagramContainer
    ///     - Quản lý DiagramControl.Items
    ///     - Viewport fitting
    /// </summary>
    public sealed class MermaidDevExpressMapper
        : IMermaidDevExpressMapper
    {
        // ============================================================
        // MAP SHAPE
        // ============================================================

        /// <summary>
        /// Map MermaidNodeShape sang DevExpress ShapeDescription.
        /// </summary>
        public ShapeDescription MapShape(
            MermaidNodeShape shape)
        {
            return MermaidShapeMapper.Map(
                shape);
        }

        // ============================================================
        // EDGE STYLE
        // ============================================================

        /// <summary>
        /// Áp dụng style của Mermaid edge lên connector.
        /// </summary>
        public void ApplyEdgeStyle(
            DiagramConnector connector,
            MermaidEdgeStyle edgeStyle)
        {
            if (connector == null)
            {
                throw new ArgumentNullException(
                    nameof(connector));
            }

            MermaidEdgeStyleMapper.Apply(
                connector,
                edgeStyle);
        }

        // ============================================================
        // LAYOUT
        // ============================================================

        /// <summary>
        /// Áp dụng geometry đã được Core tính toán
        /// lên DevExpress DiagramItem.
        ///
        /// Không tự tính lại:
        ///     - X
        ///     - Y
        ///     - Width
        ///     - Height
        /// </summary>
        public void ApplyLayout(
            DiagramItem item,
            MermaidLayoutItem layoutItem)
        {
            if (item == null)
            {
                throw new ArgumentNullException(
                    nameof(item));
            }

            if (layoutItem == null)
            {
                throw new ArgumentNullException(
                    nameof(layoutItem));
            }

            item.Position =
                new PointFloat(
                    layoutItem.X,
                    layoutItem.Y);

            item.Size =
                new SizeF(
                    layoutItem.Width,
                    layoutItem.Height);
        }

        // ============================================================
        // TEXT
        // ============================================================

        /// <summary>
        /// Áp dụng text Mermaid lên DiagramShape.
        /// </summary>
        public void ApplyText(
            DiagramShape shape,
            string text)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(
                    nameof(shape));
            }

            shape.Content =
                text ?? string.Empty;
        }
    }
}