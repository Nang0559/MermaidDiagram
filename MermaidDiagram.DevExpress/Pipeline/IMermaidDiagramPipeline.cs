using MermaidDiagram.Core.Models;
using MermaidDiagram.DevExpress.Controls;

namespace MermaidDiagram.DevExpress.Pipeline
{
    /// <summary>
    /// Contract điều phối toàn bộ pipeline
    /// render Mermaid diagram.
    ///
    /// Pipeline chịu trách nhiệm orchestration:
    ///
    ///     Mermaid source
    ///          ↓
    ///     MermaidDocument
    ///          ↓
    ///     Graph Analysis
    ///          ↓
    ///     Layout
    ///          ↓
    ///     MermaidLayoutResult
    ///          ↓
    ///     DevExpress Mapping / Rendering
    ///          ↓
    ///     MermaidDiagramControl
    ///
    /// Không chứa:
    ///     - parser implementation;
    ///     - graph algorithm;
    ///     - layout algorithm;
    ///     - mapping logic;
    ///     - rendering logic;
    ///     - interaction logic;
    ///     - workflow / business logic.
    /// </summary>
    public interface IMermaidDiagramPipeline
    {
        // ============================================================
        // RENDER SOURCE
        // ============================================================

        /// <summary>
        /// Parse source Mermaid và render diagram.
        ///
        /// Pipeline thực hiện toàn bộ orchestration
        /// từ source đến DevExpress diagram.
        /// </summary>
        void Render(
            MermaidDiagramControl control,
            string source);

        // ============================================================
        // RENDER DOCUMENT
        // ============================================================

        /// <summary>
        /// Render MermaidDocument đã được parse.
        ///
        /// Không parse lại source.
        /// Pipeline tiếp tục từ Document.
        /// </summary>
        void Render(
            MermaidDiagramControl control,
            MermaidDocument document);

        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Xóa toàn bộ diagram và state hiện tại
        /// của MermaidDiagramControl.
        /// </summary>
        void Clear(
            MermaidDiagramControl control);
    }
}