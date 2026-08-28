using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;


namespace MermaidDiagram.Core.Layout.Contract
{
    /// <summary>
    /// Engine thực hiện toàn bộ quá trình layout Mermaid.
    ///
    /// Pipeline:
    ///
    /// MermaidDocument
    ///      ↓
    /// MermaidGraphBuilder
    ///      ↓
    /// GraphAnalysisPipeline
    ///      ↓
    /// GraphAnalysisResult
    ///      ↓
    /// RootLayoutContext
    ///      ↓
    /// RootLayoutPlanner
    ///      ↓
    /// ViewportFitter
    ///      ↓
    /// MermaidLayoutResult
    /// </summary>
    public interface IMermaidLayoutEngine
    {
        MermaidLayoutResult Layout(
            string mermaidText,
            MermaidLayoutOptions options,
            MermaidLayoutDirection direction,
            float viewportWidth,
            float viewportHeight);
    }
}
