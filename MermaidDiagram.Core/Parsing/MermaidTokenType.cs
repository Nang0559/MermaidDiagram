using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MermaidDiagram.Core.Parsing
{
    /// <summary>
    /// Loại token cơ bản trong Mermaid source.
    ///
    /// Token chỉ mô tả syntax.
    /// Không chứa:
    ///     - Workflow logic
    ///     - Layout logic
    ///     - DevExpress
    ///     - WinForms
    /// </summary>
    public enum MermaidTokenType
    {
        Unknown = 0,

        // ============================================================
        // DOCUMENT
        // ============================================================

        Flowchart,

        Graph,

        Direction,

        // ============================================================
        // IDENTIFIER / TEXT
        // ============================================================

        Identifier,

        Text,

        // ============================================================
        // NODE
        // ============================================================

        NodeDefinition,

        // ============================================================
        // EDGE
        // ============================================================

        EdgeOperator,

        EdgeLabel,

        // ============================================================
        // SUBGRAPH
        // ============================================================

        Subgraph,

        End,

        // ============================================================
        // STYLE
        // ============================================================

        Style,

        ClassDefinition,

        ClassApplication,

        // ============================================================
        // PUNCTUATION
        // ============================================================

        OpenBracket,

        CloseBracket,

        OpenParen,

        CloseParen,

        OpenBrace,

        CloseBrace,

        Pipe,

        Comma,

        // ============================================================
        // DOCUMENT STRUCTURE
        // ============================================================

        NewLine
    }
}
