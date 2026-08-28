
using MermaidDiagram.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MermaidDiagram.Core.Parsing
{
    /// <summary>
    /// Parser chính của Mermaid.
    ///
    /// Nhiệm vụ:
    ///     Mermaid source
    ///         ↓
    ///     MermaidDocument
    ///
    /// Parser này chịu trách nhiệm điều phối:
    ///     - MermaidTokenizer
    ///     - MermaidRegex
    ///     - MermaidNodeParser
    ///     - MermaidEdgeParser
    ///     - MermaidSubgraphParser
    ///
    /// Không chứa:
    ///     - Layout
    ///     - DevExpress
    ///     - WinForms
    ///     - Workflow logic
    ///     - Business logic
    /// </summary>
    public sealed class MermaidParser : IMermaidParser
    {
        // ============================================================
        // SERVICES
        // ============================================================

        private readonly MermaidTokenizer _tokenizer;

        private readonly MermaidNodeParser _nodeParser;

        private readonly MermaidEdgeParser _edgeParser;

        private readonly MermaidSubgraphParser _subgraphParser;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidParser()
        {
            _tokenizer =
                new MermaidTokenizer();

            _nodeParser =
                new MermaidNodeParser();

            _edgeParser =
                new MermaidEdgeParser();

            _subgraphParser =
                new MermaidSubgraphParser();
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Parse toàn bộ Mermaid source thành MermaidDocument.
        /// </summary>
        public MermaidDocument Parse(
     string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(
                    "Mermaid source cannot be null or empty.",
                    nameof(source));
            }

            List<string> lines =
                _tokenizer.Tokenize(source);

            if (lines.Count == 0)
            {
                throw new InvalidOperationException(
                    "Mermaid source không chứa dòng hợp lệ.");
            }

            MermaidDocument document =
                new MermaidDocument();

            ParseLines(
                lines,
                document);

            ApplyStyles(
                document);

            ValidateDocument(
                document);

            return document;
        }

        // ============================================================
        // MAIN PARSE LOOP
        // ============================================================

        private void ParseLines(
            List<string> lines,
            MermaidDocument document)
        {
            if (lines == null)
            {
                throw new ArgumentNullException(
                    nameof(lines));
            }

            if (document == null)
            {
                throw new ArgumentNullException(
                    nameof(document));
            }

            Stack<MermaidSubgraph> subgraphStack =
                new Stack<MermaidSubgraph>();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // ----------------------------------------------------
                // DOCUMENT HEADER
                // ----------------------------------------------------

                if (TryParseDiagramHeader(
                        line,
                        document))
                {
                    continue;
                }

                // ----------------------------------------------------
                // SUBGRAPH END
                // ----------------------------------------------------

                if (IsEndLine(line))
                {
                    CloseSubgraph(
                        subgraphStack);

                    continue;
                }

                // ----------------------------------------------------
                // SUBGRAPH
                // ----------------------------------------------------

                if (TryParseSubgraph(
                        line,
                        document,
                        subgraphStack))
                {
                    continue;
                }

                // ----------------------------------------------------
                // STYLE
                // ----------------------------------------------------

                if (TryParseStyle(
                        line,
                        document))
                {
                    continue;
                }

                // ----------------------------------------------------
                // CLASS DEFINITION
                // ----------------------------------------------------

                if (TryParseClassDefinition(
                        line,
                        document))
                {
                    continue;
                }

                // ----------------------------------------------------
                // CLASS APPLICATION
                // ----------------------------------------------------

                if (TryParseClassApplication(
                        line,
                        document))
                {
                    continue;
                }

                // ----------------------------------------------------
                // EDGE
                // ----------------------------------------------------

                ParseEdges(
                    line,
                    document);

                // ----------------------------------------------------
                // NODE
                // ----------------------------------------------------

                ParseNodes(
                    line,
                    document,
                    subgraphStack);
            }
        }

        // ============================================================
        // HEADER
        // ============================================================

        private bool TryParseDiagramHeader(
            string line,
            MermaidDocument document)
        {
            Match flowchartMatch =
                Regex.Match(
                    line,
                    @"^\s*flowchart\s+(?<direction>[A-Za-z]+)\b",
                    RegexOptions.IgnoreCase);

            if (flowchartMatch.Success)
            {
                document.DirectionToken =
                    flowchartMatch
                        .Groups["direction"]
                        .Value
                        .Trim();

                return true;
            }

            Match graphMatch =
                Regex.Match(
                    line,
                    @"^\s*graph\s+(?<direction>[A-Za-z]+)\b",
                    RegexOptions.IgnoreCase);

            if (graphMatch.Success)
            {
                document.DirectionToken =
                    graphMatch
                        .Groups["direction"]
                        .Value
                        .Trim();

                return true;
            }

            return false;
        }
        // ============================================================
        // APPLY NODE STYLES
        // ============================================================

        private void ApplyStyles(
            MermaidDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(
                    nameof(document));
            }

            if (document.Styles == null ||
                document.Styles.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, string> pair
                     in document.Styles)
            {
                string nodeId =
                    pair.Key;

                string props =
                    pair.Value;

                MermaidNode node =
                    FindNode(
                        document,
                        nodeId);

                if (node == null)
                {
                    continue;
                }

                node.Style =
                    ParseNodeStyle(
                        props);
            }
        }
        // ============================================================
        // PARSE NODE STYLE
        // ============================================================

        private MermaidNodeStyle ParseNodeStyle(
            string props)
        {
            MermaidNodeStyle style =
                new MermaidNodeStyle();

            if (string.IsNullOrWhiteSpace(props))
            {
                return style;
            }

            string[] declarations =
                props.Split(
                    new[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (string declaration
                     in declarations)
            {
                string[] parts =
                    declaration.Split(
                        new[] { ':' },
                        2,
                        StringSplitOptions.None);

                if (parts.Length != 2)
                {
                    continue;
                }

                string property =
                    parts[0]
                        .Trim()
                        .ToLowerInvariant();

                string value =
                    parts[1]
                        .Trim();

                if (value.Length == 0)
                {
                    continue;
                }

                switch (property)
                {
                    case "fill":

                        style.Fill =
                            value;

                        break;

                    case "color":

                        style.TextColor =
                            value;

                        break;

                    case "stroke":

                        style.Stroke =
                            value;

                        break;

                    case "stroke-width":

                        style.StrokeWidth =
                            ParseStrokeWidth(
                                value);

                        break;
                }
            }

            return style;
        }
        private double? ParseStrokeWidth(
    string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string normalized =
                value.Trim();

            if (normalized.EndsWith(
                    "px",
                    StringComparison.OrdinalIgnoreCase))
            {
                normalized =
                    normalized.Substring(
                        0,
                        normalized.Length - 2);
            }

            if (double.TryParse(
                    normalized,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double result))
            {
                return result;
            }

            return null;
        }
        // ============================================================
        // SUBGRAPH
        // ============================================================

        private bool TryParseSubgraph(
            string line,
            MermaidDocument document,
            Stack<MermaidSubgraph> subgraphStack)
        {
            Match match =
                MermaidRegex.SubgraphRegex.Match(
                    line);

            if (!match.Success)
            {
                return false;
            }

            MermaidSubgraph subgraph =
                _subgraphParser.Parse(
                    match);

            if (subgraphStack.Count > 0)
            {
                MermaidSubgraph parent =
                    subgraphStack.Peek();

                subgraph.ParentKey =
                    parent.Key;

                subgraph.Depth =
                    parent.Depth + 1;

                parent.ChildSubgraphKeys.Add(
                    subgraph.Key);
            }

            document.Subgraphs.Add(
                subgraph);

            subgraphStack.Push(
                subgraph);

            return true;
        }

        // ============================================================
        // SUBGRAPH END
        // ============================================================

        private bool IsEndLine(
            string line)
        {
            return string.Equals(
                line.Trim(),
                "end",
                StringComparison.OrdinalIgnoreCase);
        }

        private void CloseSubgraph(
            Stack<MermaidSubgraph> subgraphStack)
        {
            if (subgraphStack.Count == 0)
            {
                throw new InvalidOperationException(
                    "Mermaid chứa 'end' nhưng không có " +
                    "subgraph đang mở.");
            }

            subgraphStack.Pop();
        }

        // ============================================================
        // STYLE
        // ============================================================

        private bool TryParseStyle(
            string line,
            MermaidDocument document)
        {
            Match match =
                MermaidRegex.StyleRegex.Match(
                    line);

            if (!match.Success)
            {
                return false;
            }

            string id =
                match.Groups["id"]
                    .Value
                    .Trim();

            string props =
                match.Groups["props"]
                    .Value
                    .Trim();

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException(
                    "Style directive không có node id.");
            }

            document.Styles[id] =
                props;

            return true;
        }

        // ============================================================
        // CLASS DEFINITION
        // ============================================================

        private bool TryParseClassDefinition(
            string line,
            MermaidDocument document)
        {
            Match match =
                MermaidRegex.ClassDefRegex.Match(
                    line);

            if (!match.Success)
            {
                return false;
            }

            string name =
                match.Groups["name"]
                    .Value
                    .Trim();

            string props =
                match.Groups["props"]
                    .Value
                    .Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException(
                    "classDef không có class name.");
            }

            document.ClassDefinitions[name] =
                props;

            return true;
        }

        // ============================================================
        // CLASS APPLICATION
        // ============================================================

        private bool TryParseClassApplication(
            string line,
            MermaidDocument document)
        {
            Match match =
                MermaidRegex.ClassApplyRegex.Match(
                    line);

            if (!match.Success)
            {
                return false;
            }

            string ids =
                match.Groups["ids"]
                    .Value
                    .Trim();

            string className =
                match.Groups["name"]
                    .Value
                    .Trim();

            if (string.IsNullOrWhiteSpace(ids))
            {
                throw new InvalidOperationException(
                    "class directive không có node id.");
            }

            if (string.IsNullOrWhiteSpace(className))
            {
                throw new InvalidOperationException(
                    "class directive không có class name.");
            }

            string[] nodeIds =
                ids.Split(
                    new[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawId in nodeIds)
            {
                string nodeId =
                    rawId.Trim();

                if (nodeId.Length == 0)
                {
                    continue;
                }

                document.ClassApplications[nodeId] =
                    className;
            }

            return true;
        }

        // ============================================================
        // EDGES
        // ============================================================

        private void ParseEdges(
            string line,
            MermaidDocument document)
        {
            MatchCollection matches =
                MermaidRegex.EdgeRegex.Matches(
                    line);

            foreach (Match match in matches)
            {
                MermaidEdge edge =
                    _edgeParser.Parse(
                        match);

                if (ContainsEdge(
                        document,
                        edge))
                {
                    continue;
                }

                document.Edges.Add(
                    edge);
            }
        }

        // ============================================================
        // NODES
        // ============================================================

        private void ParseNodes(
            string line,
            MermaidDocument document,
            Stack<MermaidSubgraph> subgraphStack)
        {
            MatchCollection matches =
                MermaidRegex.NodeRegex.Matches(
                    line);

            foreach (Match match in matches)
            {
                MermaidNode node =
                    _nodeParser.Parse(
                        match);

                MermaidNode? existing =
                    FindNode(
                        document,
                        node.Id);

                if (existing != null)
                {
                    ApplySubgraphMembership(
                        existing,
                        subgraphStack);

                    continue;
                }

                ApplySubgraphMembership(
                    node,
                    subgraphStack);

                document.Nodes.Add(
                    node);

                AddNodeToSubgraph(
                    node,
                    document,
                    subgraphStack);
            }
        }

        // ============================================================
        // SUBGRAPH MEMBERSHIP
        // ============================================================

        private void ApplySubgraphMembership(
            MermaidNode node,
            Stack<MermaidSubgraph> subgraphStack)
        {
            if (subgraphStack.Count == 0)
            {
                node.SubgraphId =
                    null;

                return;
            }

            node.SubgraphId =
                subgraphStack.Peek().Key;
        }

        private void AddNodeToSubgraph(
            MermaidNode node,
            MermaidDocument document,
            Stack<MermaidSubgraph> subgraphStack)
        {
            if (subgraphStack.Count == 0)
            {
                return;
            }

            MermaidSubgraph subgraph =
                subgraphStack.Peek();

            if (!subgraph.NodeKeys.Contains(
                    node.Id))
            {
                subgraph.NodeKeys.Add(
                    node.Id);
            }
        }

        // ============================================================
        // LOOKUP
        // ============================================================

        private MermaidNode? FindNode(
            MermaidDocument document,
            string nodeId)
        {
            foreach (MermaidNode node in document.Nodes)
            {
                if (string.Equals(
                        node.Id,
                        nodeId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }
            }

            return null;
        }

        // ============================================================
        // EDGE DUPLICATE
        // ============================================================

        private bool ContainsEdge(
            MermaidDocument document,
            MermaidEdge candidate)
        {
            foreach (MermaidEdge edge in document.Edges)
            {
                if (!string.Equals(
                        edge.Source,
                        candidate.Source,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.Equals(
                        edge.Target,
                        candidate.Target,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.Equals(
                        edge.Label,
                        candidate.Label,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.Equals(
                        edge.Operator,
                        candidate.Operator,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private void ValidateDocument(
            MermaidDocument document)
        {
            if (string.IsNullOrWhiteSpace(
                    document.DirectionToken))
            {
                throw new InvalidOperationException(
                    "Mermaid document không có direction.");
            }

            ValidateSubgraphs(
                document);

            ValidateEdges(
                document);
        }

        private void ValidateSubgraphs(
            MermaidDocument document)
        {
            HashSet<string> keys =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (MermaidSubgraph subgraph
                     in document.Subgraphs)
            {
                if (!keys.Add(
                        subgraph.Key))
                {
                    throw new InvalidOperationException(
                        $"Subgraph '{subgraph.Key}' bị trùng.");
                }
            }
        }

        private void ValidateEdges(
            MermaidDocument document)
        {
            HashSet<string> nodeIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (MermaidNode node
                     in document.Nodes)
            {
                nodeIds.Add(
                    node.Id);
            }

            foreach (MermaidEdge edge
                     in document.Edges)
            {
                if (!nodeIds.Contains(
                        edge.Source))
                {
                    throw new InvalidOperationException(
                        $"Edge source '{edge.Source}' " +
                        "không tồn tại.");
                }

                if (!nodeIds.Contains(
                        edge.Target))
                {
                    throw new InvalidOperationException(
                        $"Edge target '" +
                        edge.Target +
                        "' không tồn tại.");
                }
            }
        }
    }
}
