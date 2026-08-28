
using System;

namespace MermaidDiagram.Abstractions.Interaction
{
    /// <summary>
    /// Thông tin cơ bản của một interaction
    /// xảy ra trên Mermaid node.
    ///
    /// Đây là base EventArgs dùng chung cho:
    ///     - Click
    ///     - Hover
    ///     - Leave
    ///
    /// Không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - WPF
    ///     - MAUI
    ///     - Blazor
    /// </summary>
    public class MermaidNodeEventArgs
    {
        // ============================================================
        // NODE
        // ============================================================

        /// <summary>
        /// Key định danh Mermaid node.
        ///
        /// Đây là ID logic của node trong MermaidDocument.
        /// </summary>
        public string NodeKey
        {
            get;
        }

        // ============================================================
        // TEXT
        // ============================================================

        /// <summary>
        /// Nội dung hiển thị của node.
        /// </summary>
        public string NodeText
        {
            get;
        }

        // ============================================================
        // WORKFLOW
        // ============================================================

        /// <summary>
        /// Key của Workflow Node tương ứng, nếu node
        /// được ánh xạ vào Workflow.
        ///
        /// Có thể null đối với Mermaid node thuần túy
        /// không thuộc Workflow.
        /// </summary>
        public string? WorkflowNodeKey
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidNodeEventArgs(
            string nodeKey,
            string nodeText,
            string? workflowNodeKey = null)
        {
            if (string.IsNullOrWhiteSpace(nodeKey))
            {
                throw new ArgumentException(
                    "NodeKey không được rỗng.",
                    nameof(nodeKey));
            }

            NodeKey =
                nodeKey;

            NodeText =
                nodeText ?? string.Empty;

            WorkflowNodeKey =
                workflowNodeKey;
        }
    }
}
