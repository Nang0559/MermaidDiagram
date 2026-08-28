
using System;
using MermaidDiagram.Core.Models;
using MermaidDiagram.DevExpress.Controls;
using MermaidDiagram.DevExpress.Pipeline;

namespace MermaidDiagram.DevExpress.Services
{
    /// <summary>
    /// Facade cấp application cho Mermaid Diagram.
    ///
    /// Application không cần biết:
    /// - Parser
    /// - Graph
    /// - Analysis
    /// - Layout
    /// - Renderer
    ///
    /// Architecture:
    ///
    /// Application
    ///      ↓
    /// MermaidDiagramService
    ///      ↓
    /// IMermaidDiagramPipeline
    ///      ↓
    /// Parser
    /// Graph
    /// Analysis
    /// Layout
    /// Renderer
    ///      ↓
    /// MermaidDiagramControl
    /// </summary>
    public sealed class MermaidDiagramService
    {
        // ============================================================
        // PIPELINE
        // ============================================================

        private readonly IMermaidDiagramPipeline _pipeline;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidDiagramService(
            IMermaidDiagramPipeline pipeline)
        {
            _pipeline =
                pipeline
                ?? throw new ArgumentNullException(
                    nameof(pipeline));
        }

        // ============================================================
        // LOAD SOURCE
        // ============================================================

        /// <summary>
        /// Parse và render Mermaid source.
        /// </summary>
        public void Load(
            MermaidDiagramControl control,
            string source)
        {
            if (control == null)
            {
                throw new ArgumentNullException(
                    nameof(control));
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                Clear(control);

                return;
            }

            _pipeline.Render(
                control,
                source);
        }

        // ============================================================
        // LOAD DOCUMENT
        // ============================================================

        /// <summary>
        /// Render MermaidDocument đã được parse.
        /// </summary>
        public void Load(
            MermaidDiagramControl control,
            MermaidDocument document)
        {
            if (control == null)
            {
                throw new ArgumentNullException(
                    nameof(control));
            }

            if (document == null)
            {
                throw new ArgumentNullException(
                    nameof(document));
            }

            _pipeline.Render(
                control,
                document);
        }

        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Xóa diagram hiện tại.
        /// </summary>
        public void Clear(
            MermaidDiagramControl control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(
                    nameof(control));
            }

            _pipeline.Clear(
                control);
        }
    }
}

