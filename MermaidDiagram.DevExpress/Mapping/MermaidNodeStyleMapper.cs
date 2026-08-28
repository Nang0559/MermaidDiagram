using DevExpress.Utils;
using DevExpress.XtraDiagram;
using MermaidDiagram.Core.Models;

namespace MermaidDiagram.DevExpress.Rendering
{
    /// <summary>
    /// Chịu trách nhiệm duy nhất cho visual style của MermaidNode.
    ///
    /// Bao gồm:
    ///     - Font
    ///     - Text alignment
    ///     - Word wrap
    ///     - Background
    ///     - Border
    ///     - Border size
    ///     - ForeColor
    ///     - Style theo MermaidNodeShape
    ///
    /// Không chịu trách nhiệm:
    ///     - Parse
    ///     - Graph
    ///     - Layout
    ///     - Position
    ///     - Shape mapping
    ///     - Edge
    /// </summary>
    public sealed class MermaidNodeStyleMapper
    {
        // ============================================================
        // FONT
        // ============================================================

        private const string NodeFontName =
            "Segoe UI Emoji";

        private const float NodeFontSize =
            9.0f;

        // ============================================================
        // COLORS
        // ============================================================

        private static readonly Color DefaultBackColor =
            Color.White;

        private static readonly Color DefaultBorderColor =
            Color.FromArgb(
                90,
                90,
                90);

        private static readonly Color FormBackColor =
            Color.FromArgb(
                232,
                245,
                255);

        private static readonly Color FormBorderColor =
            Color.FromArgb(
                21,
                101,
                192);

        private static readonly Color DefaultTextColor =
            Color.FromArgb(
                35,
                35,
                35);

        // ============================================================
        // BORDER
        // ============================================================

        private const int DefaultBorderSize =
            1;

        private const int FormBorderSize =
            2;

        // ============================================================
        // PUBLIC API
        // ============================================================

        public void ApplyStyle(
     DiagramShape shape,
     MermaidNode node)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(
                    nameof(shape));
            }

            if (node == null)
            {
                throw new ArgumentNullException(
                    nameof(node));
            }

            ApplyFont(
                shape);

            ApplyTextOptions(
                shape);

            ApplyShapeStyle(
                shape,
                node);

            ApplyMermaidStyle(
                shape,
                node.Style);

            EnableAppearanceOptions(
                shape);
        }
        private static void ApplyMermaidStyle(
    DiagramShape shape,
    MermaidNodeStyle style)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(
                    nameof(shape));
            }

            if (style == null)
            {
                return;
            }

            // --------------------------------------------------------
            // FILL
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    style.Fill))
            {
                if (TryParseColor(
                        style.Fill,
                        out Color fillColor))
                {
                    shape.Appearance.ContentBackground =
                        fillColor;

                    shape.Appearance.Options
                        .UseContentBackground =
                        true;

                    shape.Appearance.BackColor =
                        fillColor;

                    shape.Appearance.Options
                        .UseBackColor =
                        true;
                }
            }

            // --------------------------------------------------------
            // STROKE
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    style.Stroke))
            {
                if (TryParseColor(
                        style.Stroke,
                        out Color strokeColor))
                {
                    shape.Appearance.BorderColor =
                        strokeColor;

                    shape.Appearance.Options
                        .UseBorderColor =
                        true;
                }
            }

            // --------------------------------------------------------
            // TEXT COLOR
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    style.TextColor))
            {
                if (TryParseColor(
                        style.TextColor,
                        out Color textColor))
                {
                    shape.Appearance.ForeColor =
                        textColor;

                    shape.Appearance.Options
                        .UseForeColor =
                        true;
                }
            }

            // --------------------------------------------------------
            // STROKE WIDTH
            // --------------------------------------------------------

            if (style.StrokeWidth.HasValue)
            {
                int borderSize =
                    Math.Max(
                        1,
                        (int)Math.Round(
                            style.StrokeWidth.Value));

                shape.Appearance.BorderSize =
                    borderSize;

                shape.Appearance.Options
                    .UseBorderSize =
                    true;
            }
        }
        // ============================================================
        // FONT
        // ============================================================

        private static void ApplyFont(
            DiagramShape shape)
        {
            shape.Appearance.Font =
                new Font(
                    NodeFontName,
                    NodeFontSize,
                    FontStyle.Regular);

            shape.Appearance.Options.UseFont =
                true;
        }

        // ============================================================
        // TEXT
        // ============================================================

        private static void ApplyTextOptions(
            DiagramShape shape)
        {
            shape.Appearance.TextOptions.WordWrap =
                WordWrap.Wrap;

            shape.Appearance.TextOptions.HAlignment =
                HorzAlignment.Center;

            shape.Appearance.TextOptions.VAlignment =
                VertAlignment.Center;

            shape.Appearance.Options.UseTextOptions =
                true;
        }

        // ============================================================
        // SHAPE STYLE
        // ============================================================

        private static void ApplyShapeStyle(
            DiagramShape shape,
            MermaidNode node)
        {
            switch (node.Shape)
            {
                // ----------------------------------------------------
                // STANDARD
                // ----------------------------------------------------

                case MermaidNodeShape.Rectangle:

                    ApplyRectangleStyle(
                        shape,
                        node);

                    break;

                case MermaidNodeShape.Rounded:

                    ApplyRoundedStyle(
                        shape,
                        node);

                    break;

                // ----------------------------------------------------
                // SPECIAL MERMAID SHAPES
                // ----------------------------------------------------

                case MermaidNodeShape.Stadium:

                    ApplyStadiumStyle(
                        shape,
                        node);

                    break;

                case MermaidNodeShape.Circle:

                    ApplyCircleStyle(
                        shape);

                    break;

                case MermaidNodeShape.Diamond:

                    ApplyDiamondStyle(
                        shape);

                    break;

                case MermaidNodeShape.Hexagon:

                    ApplyHexagonStyle(
                        shape);

                    break;

                case MermaidNodeShape.Cylinder:

                    ApplyCylinderStyle(
                        shape);

                    break;

                case MermaidNodeShape.Subroutine:

                    ApplySubroutineStyle(
                        shape,
                        node);

                    break;

                case MermaidNodeShape.Asymmetric:

                    ApplyAsymmetricStyle(
                        shape,
                        node);

                    break;

                // ----------------------------------------------------
                // UNKNOWN
                // ----------------------------------------------------

                default:

                    ApplyDefaultStyle(
                        shape);

                    break;
            }
        }

        // ============================================================
        // RECTANGLE
        // ============================================================

        private static void ApplyRectangleStyle(
            DiagramShape shape,
            MermaidNode node)
        {
            if (IsFormNode(node))
            {
                ApplyFormStyle(shape);
                return;
            }

            ApplyDefaultStyle(shape);
        }

        // ============================================================
        // ROUNDED
        // ============================================================

        private static void ApplyRoundedStyle(
            DiagramShape shape,
            MermaidNode node)
        {
            if (IsFormNode(node))
            {
                ApplyFormStyle(shape);
                return;
            }

            ApplyDefaultStyle(shape);
        }

        // ============================================================
        // STADIUM
        // ============================================================

        private static void ApplyStadiumStyle(
            DiagramShape shape,
            MermaidNode node)
        {
            if (IsFormNode(node))
            {
                ApplyFormStyle(shape);
                return;
            }

            ApplyDefaultStyle(shape);
        }

        // ============================================================
        // CIRCLE
        // ============================================================

        private static void ApplyCircleStyle(
            DiagramShape shape)
        {
            ApplyColors(
                shape,
                DefaultBackColor,
                DefaultBorderColor,
                DefaultBorderSize,
                DefaultTextColor);
        }

        // ============================================================
        // DIAMOND
        // ============================================================

        private static void ApplyDiamondStyle(
            DiagramShape shape)
        {
            ApplyColors(
                shape,
                DefaultBackColor,
                DefaultBorderColor,
                DefaultBorderSize,
                DefaultTextColor);
        }

        // ============================================================
        // HEXAGON
        // ============================================================

        private static void ApplyHexagonStyle(
            DiagramShape shape)
        {
            ApplyColors(
                shape,
                DefaultBackColor,
                DefaultBorderColor,
                DefaultBorderSize,
                DefaultTextColor);
        }

        // ============================================================
        // CYLINDER
        // ============================================================

        private static void ApplyCylinderStyle(
            DiagramShape shape)
        {
            ApplyColors(
                shape,
                DefaultBackColor,
                DefaultBorderColor,
                DefaultBorderSize,
                DefaultTextColor);
        }

        // ============================================================
        // SUBROUTINE
        // ============================================================

        private static void ApplySubroutineStyle(
            DiagramShape shape,
            MermaidNode node)
        {
            if (IsFormNode(node))
            {
                ApplyFormStyle(shape);
                return;
            }

            ApplyDefaultStyle(shape);
        }

        // ============================================================
        // ASYMMETRIC
        // ============================================================

        private static void ApplyAsymmetricStyle(
            DiagramShape shape,
            MermaidNode node)
        {
            if (IsFormNode(node))
            {
                ApplyFormStyle(shape);
                return;
            }

            ApplyDefaultStyle(shape);
        }

        // ============================================================
        // FORM
        // ============================================================

        private static void ApplyFormStyle(
            DiagramShape shape)
        {
            ApplyColors(
                shape,
                FormBackColor,
                FormBorderColor,
                FormBorderSize,
                GetContrastingTextColor(
                    FormBackColor));
        }

        // ============================================================
        // DEFAULT
        // ============================================================

        private static void ApplyDefaultStyle(
            DiagramShape shape)
        {
            ApplyColors(
                shape,
                DefaultBackColor,
                DefaultBorderColor,
                DefaultBorderSize,
                GetContrastingTextColor(
                    DefaultBackColor));
        }

        // ============================================================
        // COMMON COLORS
        // ============================================================

        private static void ApplyColors(
            DiagramShape shape,
            Color backgroundColor,
            Color borderColor,
            int borderSize,
            Color textColor)
        {
            shape.Appearance.ContentBackground =
                backgroundColor;

            shape.Appearance.Options.UseContentBackground =
                true;

            shape.Appearance.BackColor =
                backgroundColor;

            shape.Appearance.Options.UseBackColor =
                true;

            shape.Appearance.BorderColor =
                borderColor;

            shape.Appearance.BorderSize =
                borderSize;

            shape.Appearance.Options.UseBorderColor =
                true;

            shape.Appearance.Options.UseBorderSize =
                true;

            shape.Appearance.ForeColor =
                textColor;

            shape.Appearance.Options.UseForeColor =
                true;
        }

        // ============================================================
        // APPEARANCE OPTIONS
        // ============================================================

        private static void EnableAppearanceOptions(
            DiagramShape shape)
        {
            shape.Appearance.Options.UseContentBackground =
                true;

            shape.Appearance.Options.UseBackColor =
                true;

            shape.Appearance.Options.UseBorderColor =
                true;

            shape.Appearance.Options.UseBorderSize =
                true;

            shape.Appearance.Options.UseForeColor =
                true;

            shape.Appearance.Options.UseFont =
                true;

            shape.Appearance.Options.UseTextOptions =
                true;
        }

        // ============================================================
        // FORM DETECTION
        // ============================================================

        private static bool IsFormNode(
            MermaidNode node)
        {
            if (node == null)
            {
                return false;
            }

            if (node.Shape == MermaidNodeShape.Circle ||
                node.Shape == MermaidNodeShape.Diamond ||
                node.Shape == MermaidNodeShape.Hexagon ||
                node.Shape == MermaidNodeShape.Cylinder ||
                node.Shape == MermaidNodeShape.Asymmetric)
            {
                return false;
            }

            return IsFormName(
                node.Text);
        }

        // ============================================================
        // FORM NAME
        // ============================================================

        private static bool IsFormName(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string value =
                text.Trim();

            return
                value.IndexOf(
                    "form",
                    StringComparison.OrdinalIgnoreCase) >= 0
                ||
                value.IndexOf(
                    "phiếu",
                    StringComparison.OrdinalIgnoreCase) >= 0
                ||
                value.IndexOf(
                    "phieu",
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // ============================================================
        // CONTRAST
        // ============================================================

        private static Color GetContrastingTextColor(
            Color background)
        {
            double luminance =
                (
                    0.299 * background.R +
                    0.587 * background.G +
                    0.114 * background.B
                ) / 255.0;

            if (luminance > 0.55)
            {
                return DefaultTextColor;
            }

            return Color.White;
        }
        // ============================================================
        // COLOR PARSER
        // ============================================================

        private static bool TryParseColor(
            string value,
            out Color color)
        {
            color =
                Color.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string text =
                value.Trim();

            // --------------------------------------------------------
            // HEX
            // --------------------------------------------------------

            if (text.StartsWith(
                    "#",
                    StringComparison.Ordinal))
            {
                try
                {
                    color =
                        ColorTranslator.FromHtml(
                            text);

                    return color != Color.Empty;
                }
                catch
                {
                    return false;
                }
            }

            // --------------------------------------------------------
            // RGB / RGBA
            // --------------------------------------------------------

            if (text.StartsWith(
                    "rgb(",
                    StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith(
                    "rgba(",
                    StringComparison.OrdinalIgnoreCase))
            {
                return TryParseRgbColor(
                    text,
                    out color);
            }

            // --------------------------------------------------------
            // COLOR NAME
            // --------------------------------------------------------

            Color namedColor =
                Color.FromName(
                    text);

            if (namedColor.IsKnownColor)
            {
                color =
                    namedColor;

                return true;
            }

            return false;
        }
        private static bool TryParseRgbColor(
    string value,
    out Color color)
        {
            color =
                Color.Empty;

            int openIndex =
                value.IndexOf('(');

            int closeIndex =
                value.LastIndexOf(')');

            if (openIndex < 0 ||
                closeIndex <= openIndex)
            {
                return false;
            }

            string content =
                value.Substring(
                    openIndex + 1,
                    closeIndex - openIndex - 1);

            string[] parts =
                content.Split(',');

            if (parts.Length < 3)
            {
                return false;
            }

            if (!TryParseRgbComponent(
                    parts[0],
                    out int r))
            {
                return false;
            }

            if (!TryParseRgbComponent(
                    parts[1],
                    out int g))
            {
                return false;
            }

            if (!TryParseRgbComponent(
                    parts[2],
                    out int b))
            {
                return false;
            }

            color =
                Color.FromArgb(
                    r,
                    g,
                    b);

            return true;
        }
        private static bool TryParseRgbComponent(
    string value,
    out int component)
        {
            component =
                0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string text =
                value.Trim();

            if (!int.TryParse(
                    text,
                    out int parsed))
            {
                return false;
            }

            component =
                Math.Max(
                    0,
                    Math.Min(
                        255,
                        parsed));

            return true;
        }
    }
}