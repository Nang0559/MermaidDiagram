using DevExpress.Diagram.Core;
using DevExpress.XtraDiagram;
using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;

namespace MermaidDiagram.DevExpress.Mapping
{
    /// <summary>
    /// Contract ánh xạ dữ liệu từ MermaidDiagram.Core
    /// sang đối tượng DevExpress Diagram.
    ///
    /// Mapper chỉ chịu trách nhiệm chuyển đổi representation.
    ///
    /// Không chịu trách nhiệm:
    ///     - Parse Mermaid
    ///     - Graph analysis
    ///     - Workflow
    ///     - Layout calculation
    ///     - Collision resolution
    ///     - Viewport fitting
    ///     - Rendering pipeline
    ///     - Business logic
    /// </summary>
    public interface IMermaidDevExpressMapper
    {
        // ============================================================
        // SHAPE
        // ============================================================

        /// <summary>
        /// Ánh xạ MermaidNodeShape sang
        /// DevExpress ShapeDescription.
        /// </summary>
        ShapeDescription MapShape(
            MermaidNodeShape shape);

        // ============================================================
        // EDGE STYLE
        // ============================================================

        /// <summary>
        /// Áp dụng style của Mermaid edge
        /// lên DevExpress DiagramConnector.
        /// </summary>
        void ApplyEdgeStyle(
            DiagramConnector connector,
            MermaidEdgeStyle edgeStyle);

        // ============================================================
        // LAYOUT
        // ============================================================

        /// <summary>
        /// Áp dụng geometry đã được MermaidDiagram.Core
        /// tính toán lên DevExpress DiagramItem.
        ///
        /// Mapper KHÔNG được tự tính lại layout.
        ///
        /// Bao gồm:
        ///     - Position
        ///     - Size
        /// </summary>
        void ApplyLayout(
            DiagramItem item,
            MermaidLayoutItem layoutItem);

        // ============================================================
        // TEXT
        // ============================================================

        /// <summary>
        /// Áp dụng text của Mermaid node
        /// lên DevExpress DiagramShape.
        /// </summary>
        void ApplyText(
            DiagramShape shape,
            string text);
    }
}