using System.Text.RegularExpressions;

namespace MermaidDiagram.Core.Parsing
{
    /// <summary>
    /// Parser chịu trách nhiệm chuyển một Subgraph Match
    /// thành MermaidSubgraph.
    ///
    /// Parser này chỉ xử lý:
    ///     - Subgraph key
    ///     - Subgraph title
    ///
    /// Không xử lý:
    ///     - ParentKey
    ///     - Depth
    ///     - NodeKeys
    ///     - ChildSubgraphKeys
    ///     - Layout
    ///     - Workflow
    ///     - DevExpress
    ///     - WinForms
    /// </summary>
    public sealed class MermaidSubgraphParser
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Parse một Match đã được xác nhận bởi
        /// MermaidRegex.SubgraphRegex.
        /// </summary>
        public MermaidSubgraph Parse(
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
                    "Subgraph match is not successful.",
                    nameof(match));
            }

            string key =
                GetKey(match);

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException(
                    "Subgraph key cannot be empty.");
            }

            string title =
                GetTitle(
                    match,
                    key);

            return new MermaidSubgraph(
                key,
                title);
        }

        // ============================================================
        // KEY
        // ============================================================

        /// <summary>
        /// Lấy key của subgraph.
        ///
        /// Ưu tiên:
        ///     1. id
        ///     2. plain
        /// </summary>
        private string GetKey(
            Match match)
        {
            Group idGroup =
                match.Groups["id"];

            if (idGroup.Success)
            {
                string id =
                    idGroup.Value.Trim();

                if (!string.IsNullOrWhiteSpace(id))
                {
                    return id;
                }
            }

            Group plainGroup =
                match.Groups["plain"];

            if (plainGroup.Success)
            {
                string plain =
                    plainGroup.Value.Trim();

                if (!string.IsNullOrWhiteSpace(plain))
                {
                    return plain;
                }
            }

            return string.Empty;
        }

        // ============================================================
        // TITLE
        // ============================================================

        /// <summary>
        /// Lấy title của subgraph.
        ///
        /// Nếu Mermaid chỉ cung cấp plain name
        /// thì dùng key làm title.
        /// </summary>
        private string GetTitle(
            Match match,
            string key)
        {
            Group titleGroup =
                match.Groups["title"];

            if (titleGroup.Success)
            {
                string title =
                    titleGroup.Value.Trim();

                if (!string.IsNullOrWhiteSpace(title))
                {
                    return title;
                }
            }

            return key;
        }
    }
}