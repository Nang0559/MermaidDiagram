using MermaidDiagram.Abstractions.Workflow.Views;
using MermaidDiagram.DevExpress.Controls;

namespace MermaidDiagram.DevExpress.Viewport
{
    /// <summary>
    /// DevExpress implementation của Workflow Diagram Viewport.
    ///
    /// MermaidDiagramViewport chỉ chịu trách nhiệm:
    ///     - Fit content
    ///     - Center content
    ///     - Reset viewport
    ///     - Zoom In
    ///     - Zoom Out
    ///
    /// Không chịu trách nhiệm:
    ///     - Parse Mermaid
    ///     - Graph analysis
    ///     - Layout calculation
    ///     - Mapping node/edge
    ///     - Render DiagramItem
    ///     - Workflow transition
    ///     - View Schema
    ///     - Action
    /// </summary>
    public sealed class MermaidDiagramViewport
        : IMermaidDiagramViewport
    {
        private readonly MermaidDiagramControl _control;

        public MermaidDiagramViewport(
            MermaidDiagramControl control)
        {
            _control =
                control
                ?? throw new ArgumentNullException(
                    nameof(control));
        }

        public void FitToContent()
        {
            // DevExpress viewport logic
        }

        public void CenterContent()
        {
            // DevExpress viewport logic
        }

        public void Reset()
        {
            // DevExpress viewport logic
        }

        public void ZoomIn()
        {
            // DevExpress viewport logic
        }

        public void ZoomOut()
        {
            // DevExpress viewport logic
        }
    }
}