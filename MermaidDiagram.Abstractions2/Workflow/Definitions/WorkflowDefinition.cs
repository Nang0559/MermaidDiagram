
using System;
using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Workflow.Definitions
{
    /// <summary>
    /// Mô tả tĩnh của một Workflow.
    ///
    /// WorkflowDefinition chỉ chứa cấu trúc và metadata.
    ///
    /// Không chứa:
    ///     - Business logic
    ///     - Repository
    ///     - Service
    ///     - UI
    ///     - Runtime execution
    ///
    /// Runtime thuộc:
    ///     WorkflowNodeDispatcher
    ///     WorkflowContext
    ///     IWorkflowView
    ///     IWorkflowAction
    /// </summary>
    public sealed class WorkflowDefinition
    {
        private readonly List<WorkflowNodeDefinition> _nodes =
            new List<WorkflowNodeDefinition>();

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public WorkflowDefinition(
            string workflowKey,
            string workflowTitle)
        {
            if (string.IsNullOrWhiteSpace(workflowKey))
            {
                throw new ArgumentException(
                    "WorkflowKey không được rỗng.",
                    nameof(workflowKey));
            }

            if (string.IsNullOrWhiteSpace(workflowTitle))
            {
                throw new ArgumentException(
                    "WorkflowTitle không được rỗng.",
                    nameof(workflowTitle));
            }

            WorkflowKey = workflowKey;
            WorkflowTitle = workflowTitle;
        }

        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// Key duy nhất của Workflow.
        ///
        /// Ví dụ:
        ///     "QTCHUNG_HANG_LOI"
        ///     "REWORK_PROCESS"
        ///     "DELIVERY_PROCESS"
        /// </summary>
        public string WorkflowKey
        {
            get;
        }

        /// <summary>
        /// Tên hiển thị của Workflow.
        /// </summary>
        public string WorkflowTitle
        {
            get;
        }

        /// <summary>
        /// Version của Workflow Definition.
        ///
        /// Cho phép nhiều version của cùng một workflow
        /// tồn tại độc lập.
        /// </summary>
        public int Version
        {
            get;
            set;
        } = 1;

        /// <summary>
        /// Mô tả Workflow.
        /// </summary>
        public string? Description
        {
            get;
            set;
        }

        // ============================================================
        // START NODE
        // ============================================================

        /// <summary>
        /// Node bắt đầu Workflow.
        ///
        /// Giá trị phải trùng với WorkflowNodeDefinition.NodeKey.
        /// </summary>
        public string? StartNodeKey
        {
            get;
            set;
        }

        // ============================================================
        // NODES
        // ============================================================

        /// <summary>
        /// Danh sách node thuộc Workflow.
        /// </summary>
        public IReadOnlyList<WorkflowNodeDefinition> Nodes
        {
            get
            {
                return _nodes;
            }
        }

        /// <summary>
        /// Thêm một node vào Workflow.
        /// </summary>
        public void AddNode(
            WorkflowNodeDefinition node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(
                    nameof(node));
            }

            if (string.IsNullOrWhiteSpace(node.NodeKey))
            {
                throw new ArgumentException(
                    "Workflow node key không được rỗng.",
                    nameof(node));
            }

            if (FindNode(node.NodeKey) != null)
            {
                throw new InvalidOperationException(
                    $"Workflow '{WorkflowKey}' " +
                    $"đã tồn tại node '{node.NodeKey}'.");
            }

            _nodes.Add(node);
        }

        // ============================================================
        // FIND NODE
        // ============================================================

        /// <summary>
        /// Tìm Workflow node theo NodeKey.
        /// </summary>
        public WorkflowNodeDefinition? FindNode(
            string nodeKey)
        {
            if (string.IsNullOrWhiteSpace(nodeKey))
            {
                return null;
            }

            for (int i = 0;
                 i < _nodes.Count;
                 i++)
            {
                WorkflowNodeDefinition node =
                    _nodes[i];

                if (string.Equals(
                        node.NodeKey,
                        nodeKey,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }
            }

            return null;
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Kiểm tra tính hợp lệ về mặt cấu trúc
        /// của Workflow Definition.
        ///
        /// Không kiểm tra:
        ///     - Business rule
        ///     - Permission
        ///     - Database
        ///     - Service
        ///     - Runtime state
        /// </summary>
        public void Validate()
        {
            if (_nodes.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Workflow '{WorkflowKey}' chưa có node.");
            }

            if (string.IsNullOrWhiteSpace(StartNodeKey))
            {
                throw new InvalidOperationException(
                    $"Workflow '{WorkflowKey}' " +
                    "chưa xác định StartNodeKey.");
            }

            WorkflowNodeDefinition? startNode =
                FindNode(StartNodeKey);

            if (startNode == null)
            {
                throw new InvalidOperationException(
                    $"Workflow '{WorkflowKey}' không tìm thấy " +
                    $"StartNode '{StartNodeKey}'.");
            }

            for (int i = 0;
                 i < _nodes.Count;
                 i++)
            {
                WorkflowNodeDefinition node =
                    _nodes[i];

                node.Validate();

                ValidateNodeReferences(node);
            }
        }

        // ============================================================
        // NODE REFERENCES
        // ============================================================

        private void ValidateNodeReferences(
            WorkflowNodeDefinition node)
        {
            foreach (string nextNodeKey
                     in node.NextNodeKeys)
            {
                if (string.IsNullOrWhiteSpace(nextNodeKey))
                {
                    continue;
                }

                if (FindNode(nextNodeKey) == null)
                {
                    throw new InvalidOperationException(
                        $"Workflow '{WorkflowKey}', " +
                        $"node '{node.NodeKey}' tham chiếu tới " +
                        $"node '{nextNodeKey}' nhưng node này " +
                        "không tồn tại.");
                }
            }
        }
    }
}

