
using System;

namespace MermaidDiagram.Abstractions.Navigation
{
    /// <summary>
    /// Request yêu cầu Workflow thực hiện navigation
    /// từ một Mermaid node.
    ///
    /// Request này không phụ thuộc UI framework.
    ///
    /// Không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    /// </summary>
    public sealed class WorkflowNavigationRequest
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
        /// Nội dung hiển thị của Mermaid node.
        /// </summary>
        public string NodeText
        {
            get;
        }

        // ============================================================
        // WORKFLOW
        // ============================================================

        /// <summary>
        /// Key của Workflow Node tương ứng.
        ///
        /// Có thể null nếu Mermaid node không được
        /// ánh xạ vào Workflow Node.
        /// </summary>
        public string? WorkflowNodeKey
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public WorkflowNavigationRequest(
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

            if (string.IsNullOrWhiteSpace(nodeText))
            {
                throw new ArgumentException(
                    "NodeText không được rỗng.",
                    nameof(nodeText));
            }

            NodeKey =
                nodeKey;

            NodeText =
                nodeText;

            WorkflowNodeKey =
                workflowNodeKey;
        }
    }
}

