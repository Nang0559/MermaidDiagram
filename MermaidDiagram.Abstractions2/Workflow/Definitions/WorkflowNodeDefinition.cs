
using System;
using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Workflow.Definitions
{
    /// <summary>
    /// Mô tả tĩnh của một node trong Workflow.
    ///
    /// WorkflowNodeDefinition chỉ chứa metadata cấu hình.
    /// Không chứa business logic và không phụ thuộc UI framework.
    /// </summary>
    public sealed class WorkflowNodeDefinition
    {
        private readonly List<string> _nextNodeKeys =
            new List<string>();

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public WorkflowNodeDefinition(
            string nodeKey)
        {
            if (string.IsNullOrWhiteSpace(nodeKey))
            {
                throw new ArgumentException(
                    "NodeKey không được rỗng.",
                    nameof(nodeKey));
            }

            NodeKey = nodeKey;
        }

        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// Key định danh duy nhất của node trong Workflow.
        ///
        /// Thông thường có thể tương ứng với Mermaid node key.
        /// </summary>
        public string NodeKey
        {
            get;
        }

        /// <summary>
        /// Tên hiển thị của node.
        /// </summary>
        public string? DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Mô tả node.
        /// </summary>
        public string? Description
        {
            get;
            set;
        }

        /// <summary>
        /// Loại node.
        /// </summary>
        public WorkflowNodeKind Kind
        {
            get;
            set;
        } = WorkflowNodeKind.Information;

        // ============================================================
        // VIEW
        // ============================================================

        /// <summary>
        /// Key dùng để IWorkflowViewFactory tạo View.
        ///
        /// Chỉ có giá trị khi node có View.
        /// </summary>
        public string? ViewKey
        {
            get;
            set;
        }

        // ============================================================
        // ACTION
        // ============================================================

        /// <summary>
        /// Key dùng để IWorkflowActionFactory tạo Action.
        ///
        /// Chỉ có giá trị khi node có Action.
        /// </summary>
        public string? ActionKey
        {
            get;
            set;
        }

        // ============================================================
        // TRANSITIONS
        // ============================================================

        /// <summary>
        /// Các Workflow node có thể đi tiếp từ node hiện tại.
        ///
        /// Đây chỉ là static graph definition.
        /// Việc transition có hợp lệ về nghiệp vụ hay không
        /// thuộc runtime/business layer.
        /// </summary>
        public IReadOnlyList<string> NextNodeKeys
        {
            get
            {
                return _nextNodeKeys;
            }
        }

        /// <summary>
        /// Thêm một node tiếp theo.
        /// </summary>
        public void AddNextNode(
            string nextNodeKey)
        {
            if (string.IsNullOrWhiteSpace(nextNodeKey))
            {
                throw new ArgumentException(
                    "NextNodeKey không được rỗng.",
                    nameof(nextNodeKey));
            }

            for (int i = 0;
                 i < _nextNodeKeys.Count;
                 i++)
            {
                if (string.Equals(
                        _nextNodeKeys[i],
                        nextNodeKey,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            _nextNodeKeys.Add(
                nextNodeKey);
        }

        /// <summary>
        /// Xóa một transition.
        /// </summary>
        public bool RemoveNextNode(
            string nextNodeKey)
        {
            if (string.IsNullOrWhiteSpace(nextNodeKey))
            {
                return false;
            }

            for (int i = 0;
                 i < _nextNodeKeys.Count;
                 i++)
            {
                if (string.Equals(
                        _nextNodeKeys[i],
                        nextNodeKey,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _nextNodeKeys.RemoveAt(i);

                    return true;
                }
            }

            return false;
        }

        // ============================================================
        // HELPERS
        // ============================================================

        /// <summary>
        /// Node có View được cấu hình hay không.
        /// </summary>
        public bool HasView
        {
            get
            {
                return !string.IsNullOrWhiteSpace(
                    ViewKey);
            }
        }

        /// <summary>
        /// Node có Action được cấu hình hay không.
        /// </summary>
        public bool HasAction
        {
            get
            {
                return !string.IsNullOrWhiteSpace(
                    ActionKey);
            }
        }

        /// <summary>
        /// Node là node kết thúc Workflow.
        /// </summary>
        public bool IsTerminal
        {
            get
            {
                return Kind ==
                       WorkflowNodeKind.Terminal;
            }
        }

        /// <summary>
        /// Node là node quyết định.
        /// </summary>
        public bool IsDecision
        {
            get
            {
                return Kind ==
                       WorkflowNodeKind.Decision;
            }
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Kiểm tra tính hợp lệ nội tại của node.
        ///
        /// Không kiểm tra NextNodeKeys có tồn tại
        /// trong WorkflowDefinition hay không.
        /// </summary>
        public void Validate()
        {
            switch (Kind)
            {
                case WorkflowNodeKind.View:

                    RequireView();

                    break;

                case WorkflowNodeKind.Action:

                    RequireAction();

                    break;

                case WorkflowNodeKind.Interactive:

                    if (!HasView &&
                        !HasAction)
                    {
                        throw new InvalidOperationException(
                            $"Node '{NodeKey}' có Kind=Interactive " +
                            "nhưng chưa cấu hình ViewKey hoặc ActionKey.");
                    }

                    break;

                case WorkflowNodeKind.Automatic:

                    RequireAction();

                    break;

                case WorkflowNodeKind.Information:

                case WorkflowNodeKind.Decision:

                case WorkflowNodeKind.Terminal:

                    break;

                default:

                    throw new InvalidOperationException(
                        $"Node '{NodeKey}' có WorkflowNodeKind không hợp lệ.");
            }
        }

        // ============================================================
        // VALIDATION HELPERS
        // ============================================================

        private void RequireView()
        {
            if (!HasView)
            {
                throw new InvalidOperationException(
                    $"Node '{NodeKey}' có Kind=View " +
                    "nhưng chưa cấu hình ViewKey.");
            }
        }

        private void RequireAction()
        {
            if (!HasAction)
            {
                throw new InvalidOperationException(
                    $"Node '{NodeKey}' có Kind={Kind} " +
                    "nhưng chưa cấu hình ActionKey.");
            }
        }
    }
}

