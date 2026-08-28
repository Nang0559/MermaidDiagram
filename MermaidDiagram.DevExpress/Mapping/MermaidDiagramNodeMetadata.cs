namespace MermaidDiagram.DevExpress.Mapping
{
    /// <summary>
    /// Metadata của một Mermaid node sau khi Mapping
    /// sang DevExpress DiagramItem.
    ///
    /// Metadata này là cầu nối giữa:
    ///
    ///     MermaidDocument
    ///          ↓
    ///     Mapping
    ///          ↓
    ///     DiagramItem.Tag
    ///          ↓
    ///     Interaction
    ///
    /// Không phải abstraction public.
    /// Không chứa DevExpress control.
    /// Không chứa business logic.
    /// </summary>
    internal sealed class MermaidDiagramNodeMetadata
    {
        // ============================================================
        // MERMAID NODE
        // ============================================================

        public string NodeKey
        {
            get;
        }

        public string NodeText
        {
            get;
        }

        // ============================================================
        // WORKFLOW
        // ============================================================

        public string? WorkflowNodeKey
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidDiagramNodeMetadata(
            string nodeKey,
            string nodeText,
            string? workflowNodeKey)
        {
            if (string.IsNullOrWhiteSpace(nodeKey))
            {
                throw new ArgumentException(
                    "NodeKey không được rỗng.",
                    nameof(nodeKey));
            }

            NodeKey =
                nodeKey.Trim();

            NodeText =
                nodeText ?? string.Empty;

            WorkflowNodeKey =
                string.IsNullOrWhiteSpace(workflowNodeKey)
                    ? null
                    : workflowNodeKey.Trim();
        }
    }
}