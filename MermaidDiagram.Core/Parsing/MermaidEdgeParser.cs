using MermaidDiagram.Core.Models;
using System.Text.RegularExpressions;

namespace MermaidDiagram.Core.Parsing
{
    /// <summary>
    /// Parse Mermaid edge syntax thành MermaidEdge.
    ///
    /// Parser chỉ chịu trách nhiệm:
    ///     Mermaid syntax
    ///         ↓
    ///     MermaidEdge
    ///
    /// Không chứa:
    ///     - Workflow logic
    ///     - Layout logic
    ///     - DevExpress
    ///     - WinForms
    /// </summary>
    public sealed class MermaidEdgeParser
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Parse một Match đã được xác nhận bởi
        /// MermaidRegex.EdgeRegex.
        /// </summary>
        public MermaidEdge Parse(
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
                    "Edge match is not successful.",
                    nameof(match));
            }

            string source =
                GetGroupValue(
                    match,
                    "src");

            string target =
                GetGroupValue(
                    match,
                    "dst");

            string label =
                GetGroupValue(
                    match,
                    "label");

            string operatorToken =
                GetGroupValue(
                    match,
                    "op");

            // ========================================================
            // VALIDATION
            // ========================================================

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new InvalidOperationException(
                    "Edge source cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(target))
            {
                throw new InvalidOperationException(
                    "Edge target cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(operatorToken))
            {
                throw new InvalidOperationException(
                    "Edge operator cannot be empty.");
            }

            // ========================================================
            // RESULT
            // ========================================================

            return new MermaidEdge
            {
                Source =
                    source,

                Target =
                    target,

                Label =
                    label,

                Operator =
                    operatorToken,

                Style =
                    GetEdgeStyle(
                        operatorToken)
            };
        }

        // ============================================================
        // EDGE STYLE
        // ============================================================

        /// <summary>
        /// Chuyển Mermaid operator thành MermaidEdgeStyle.
        ///
        /// Đây chỉ là mapping:
        ///
        ///     Mermaid syntax
        ///         ↓
        ///     MermaidEdgeStyle
        ///
        /// Không liên quan đến:
        ///     - Layout
        ///     - Rendering
        ///     - Workflow
        /// </summary>
        private MermaidEdgeStyle GetEdgeStyle(
            string operatorToken)
        {
            switch (operatorToken)
            {
                case "-.->":
                case "-.-":
                    return MermaidEdgeStyle.Dashed;

                case "==>":
                    return MermaidEdgeStyle.Thick;

                case "-->":
                case "---":
                default:
                    return MermaidEdgeStyle.Solid;
            }
        }

        // ============================================================
        // GROUP HELPER
        // ============================================================

        /// <summary>
        /// Lấy giá trị của Regex group.
        ///
        /// Group không tồn tại hoặc không match
        /// sẽ trả về chuỗi rỗng.
        /// </summary>
        private string GetGroupValue(
            Match match,
            string groupName)
        {
            Group group =
                match.Groups[groupName];

            if (!group.Success)
            {
                return string.Empty;
            }

            return group.Value.Trim();
        }
    }
}