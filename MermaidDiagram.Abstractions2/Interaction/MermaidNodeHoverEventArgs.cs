
namespace MermaidDiagram.Abstractions.Interaction
{
    /// <summary>
    /// Event arguments khi người dùng hover
    /// lên một Mermaid node.
    ///
    /// Đây là abstraction contract, không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    ///     - Business logic
    /// </summary>
    public sealed class MermaidNodeHoverEventArgs
        : MermaidNodeEventArgs
    {
        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidNodeHoverEventArgs(
            string nodeKey,
            string nodeText,
            string? workflowNodeKey = null)
            : base(
                nodeKey,
                nodeText,
                workflowNodeKey)
        {
        }
    }
}

