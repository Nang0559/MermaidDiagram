using DevExpress.Diagram.Core;
using MermaidDiagram.Core.Models;

namespace MermaidDiagram.DevExpress.Mapping
{
    /// <summary>
    /// Map MermaidNodeShape sang ShapeDescription
    /// của DevExpress Diagram.
    ///
    /// Trách nhiệm duy nhất:
    ///
    ///     MermaidNodeShape
    ///             ↓
    ///     DevExpress ShapeDescription
    ///
    /// Không:
    ///     - tạo DiagramShape
    ///     - xử lý position
    ///     - xử lý size
    ///     - xử lý text
    ///     - xử lý visual style
    ///     - xử lý layout
    /// </summary>
    public static class MermaidShapeMapper
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Map MermaidNodeShape sang ShapeDescription
        /// tương ứng của DevExpress Diagram.
        ///
        /// Một số Mermaid shape không có shape tương đương
        /// trực tiếp trong DevExpress. Khi đó sử dụng shape
        /// gần nhất hoặc fallback an toàn.
        /// </summary>
        public static ShapeDescription Map(
            MermaidNodeShape shape)
        {
            switch (shape)
            {
                // ----------------------------------------------------
                // RECTANGLE
                // ----------------------------------------------------

                case MermaidNodeShape.Rectangle:

                    return BasicShapes.Rectangle;

                // ----------------------------------------------------
                // ROUNDED
                // ----------------------------------------------------

                case MermaidNodeShape.Rounded:

                    return BasicShapes.RoundedRectangle;

                // ----------------------------------------------------
                // STADIUM
                // ----------------------------------------------------

                case MermaidNodeShape.Stadium:

                    return BasicFlowchartShapes.StartEnd;

                // ----------------------------------------------------
                // CIRCLE
                // ----------------------------------------------------

                case MermaidNodeShape.Circle:

                    return BasicShapes.Ellipse;

                // ----------------------------------------------------
                // DIAMOND
                // ----------------------------------------------------

                case MermaidNodeShape.Diamond:

                    return BasicShapes.Diamond;

                // ----------------------------------------------------
                // HEXAGON
                // ----------------------------------------------------

                case MermaidNodeShape.Hexagon:

                    return BasicShapes.Hexagon;

                // ----------------------------------------------------
                // CYLINDER
                // ----------------------------------------------------

                case MermaidNodeShape.Cylinder:

                    return BasicShapes.Can;

                // ----------------------------------------------------
                // SUBROUTINE
                // ----------------------------------------------------

                case MermaidNodeShape.Subroutine:

                    return BasicFlowchartShapes.Subprocess;

                // ----------------------------------------------------
                // ASYMMETRIC
                // ----------------------------------------------------

                case MermaidNodeShape.Asymmetric:

                    /*
                     * DevExpress không có shape tương đương
                     * trực tiếp trong mapping hiện tại.
                     *
                     * Rectangle là fallback có chủ đích.
                     */
                    return BasicShapes.Rectangle;

                // ----------------------------------------------------
                // FALLBACK
                // ----------------------------------------------------

                default:

                    return BasicShapes.Rectangle;
            }
        }

        // ============================================================
        // TYPE HELPERS
        // ============================================================

        public static bool IsCircle(
            MermaidNodeShape shape)
        {
            return shape ==
                   MermaidNodeShape.Circle;
        }

        public static bool IsRectangle(
            MermaidNodeShape shape)
        {
            return shape ==
                   MermaidNodeShape.Rectangle;
        }

        public static bool IsRounded(
            MermaidNodeShape shape)
        {
            return shape ==
                   MermaidNodeShape.Rounded;
        }

        public static bool IsStadium(
            MermaidNodeShape shape)
        {
            return shape ==
                   MermaidNodeShape.Stadium;
        }

        public static bool IsDiamond(
            MermaidNodeShape shape)
        {
            return shape ==
                   MermaidNodeShape.Diamond;
        }

        public static bool IsHexagon(
            MermaidNodeShape shape)
        {
            return shape ==
                   MermaidNodeShape.Hexagon;
        }

        public static bool IsCylinder(
            MermaidNodeShape shape)
        {
            return shape ==
                   MermaidNodeShape.Cylinder;
        }

        public static bool IsSubroutine(
            MermaidNodeShape shape)
        {
            return shape ==
                   MermaidNodeShape.Subroutine;
        }

        public static bool IsAsymmetric(
            MermaidNodeShape shape)
        {
            return shape ==
                   MermaidNodeShape.Asymmetric;
        }
    }
}