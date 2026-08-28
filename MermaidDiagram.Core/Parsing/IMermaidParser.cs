using MermaidDiagram.Core.Models;

namespace MermaidDiagram.Core.Parsing
{
    /// <summary>
    /// Parser Mermaid text thành MermaidDocument.
    ///
    /// Parser không thực hiện:
    /// - Graph analysis
    /// - Layout
    /// - Viewport fitting
    /// - DevExpress rendering
    /// </summary>
    public interface IMermaidParser
    {
        MermaidDocument Parse(string mermaidText);
    }
}