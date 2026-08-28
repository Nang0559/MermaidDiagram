using MermaidDiagram.Core.Models;
using System.Text.RegularExpressions;

namespace MermaidDiagram.Core.Parsing
{
    /// <summary>
    /// Parser chịu trách nhiệm chuyển một Node Match
    /// thành MermaidNode.
    ///
    /// Parser này chỉ xử lý:
    ///     - NodeId
    ///     - Text
    ///     - Shape
    ///
    /// Không xử lý:
    ///     - Style
    ///     - Subgraph
    ///     - Layout
    ///     - Workflow
    ///     - DevExpress
    /// </summary>
    public sealed class MermaidNodeParser
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Parse một Match đã được xác nhận bởi
        /// MermaidRegex.NodeRegex.
        /// </summary>
        public MermaidNode Parse(
            Match match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(
                    nameof(match));
            }

            if (!match.Success)
            {
                throw new ArgumentException(
                    "Node match is not successful.",
                    nameof(match));
            }

            string id =
                match.Groups["id"].Value.Trim();

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException(
                    "Node id cannot be empty.");
            }

            MermaidNodeShape shape;

            string text;

            ParseShape(
                match,
                out shape,
                out text);

            return new MermaidNode
            {
                Id =
                    id,

                Text =
                    text.Trim(),

                Shape =
                    shape
            };
        }

        // ============================================================
        // SHAPE
        // ============================================================

        private void ParseShape(
            Match match,
            out MermaidNodeShape shape,
            out string text)
        {
            // --------------------------------------------------------
            // CIRCLE
            // (( "Text" ))
            // --------------------------------------------------------

            if (match.Groups["circle"].Success)
            {
                shape =
                    MermaidNodeShape.Circle;

                text =
                    match.Groups["circle"].Value;

                return;
            }

            // --------------------------------------------------------
            // DIAMOND
            // { "Text" }
            // --------------------------------------------------------

            if (match.Groups["decision"].Success)
            {
                shape =
                    MermaidNodeShape.Diamond;

                text =
                    match.Groups["decision"].Value;

                return;
            }

            // --------------------------------------------------------
            // RECTANGLE
            // [ "Text" ]
            // --------------------------------------------------------

            if (match.Groups["rect"].Success)
            {
                shape =
                    MermaidNodeShape.Rectangle;

                text =
                    match.Groups["rect"].Value;

                return;
            }

            // --------------------------------------------------------
            // ROUNDED
            // ( "Text" )
            // --------------------------------------------------------

            if (match.Groups["round"].Success)
            {
                shape =
                    MermaidNodeShape.Rounded;

                text =
                    match.Groups["round"].Value;

                return;
            }

            // --------------------------------------------------------
            // ROUNDED RAW
            // (Text)
            // --------------------------------------------------------

            if (match.Groups["roundRaw"].Success)
            {
                shape =
                    MermaidNodeShape.Rounded;

                text =
                    match.Groups["roundRaw"].Value;

                return;
            }

            // --------------------------------------------------------
            // RECTANGLE RAW
            // [Text]
            // --------------------------------------------------------

            if (match.Groups["rectRaw"].Success)
            {
                shape =
                    MermaidNodeShape.Rectangle;

                text =
                    match.Groups["rectRaw"].Value;

                return;
            }

            // --------------------------------------------------------
            // DIAMOND RAW
            // {Text}
            // --------------------------------------------------------

            if (match.Groups["decisionRaw"].Success)
            {
                shape =
                    MermaidNodeShape.Diamond;

                text =
                    match.Groups["decisionRaw"].Value;

                return;
            }

            // --------------------------------------------------------
            // FALLBACK
            // --------------------------------------------------------

            shape =
                MermaidNodeShape.Rectangle;

            text =
                match.Groups["id"].Value;
        }
    }
}