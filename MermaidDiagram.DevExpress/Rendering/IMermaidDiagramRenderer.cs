
using DevExpress.XtraDiagram;
using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;

namespace MermaidDiagram.DevExpress.Rendering
{
    /// <summary>
    /// Contract cấp cao chịu trách nhiệm render một Mermaid diagram
    /// vào DevExpress DiagramControl.
    ///
    /// Pipeline:
    ///
    /// MermaidDocument
    ///        +
    /// MermaidLayoutResult
    ///        ↓
    /// IMermaidDiagramRenderer
    ///        ↓
    /// DiagramShape
    /// DiagramConnector
    /// DiagramContainer
    ///        ↓
    /// DiagramControl
    ///
    /// Renderer chỉ điều phối quá trình render.
    ///
    /// Không chịu trách nhiệm:
    ///     - Parse Mermaid.
    ///     - Graph analysis.
    ///     - Tính layout.
    ///     - Tính spacing.
    ///     - Collision resolution.
    ///     - Viewport fitting.
    ///     - Workflow / business logic.
    /// </summary>
    public interface IMermaidDiagramRenderer
    {
        // ============================================================
        // RENDER
        // ============================================================

        /// <summary>
        /// Render toàn bộ Mermaid document với layout đã được Core
        /// tính toán vào DiagramControl.
        ///
        /// Renderer không tự tính lại geometry.
        /// </summary>
        void Render(
            MermaidDocument document,
            MermaidLayoutResult layout,
            DiagramControl diagram);
    }
}

