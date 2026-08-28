
using MermaidDiagram.Abstractions.Workflow.Definitions;

namespace MermaidDiagram.Core.Workflow.Registry
{
    /// <summary>
    /// Registry lưu trữ và tra cứu WorkflowNodeDefinition.
    ///
    /// WorkflowNodeRegistry chỉ chịu trách nhiệm:
    ///
    ///     - Register node
    ///     - Replace node
    ///     - Get node
    ///     - TryGet node
    ///     - Kiểm tra node tồn tại
    ///     - Remove node
    ///     - Clear registry
    ///
    /// Không chịu trách nhiệm:
    ///
    ///     - Business logic
    ///     - Workflow transition
    ///     - Execute View
    ///     - Execute Action
    ///     - Parse Mermaid
    ///     - Validate toàn bộ WorkflowDefinition
    /// </summary>
    public sealed class WorkflowNodeRegistry
    {
        // ============================================================
        // STORAGE
        // ============================================================

        private readonly Dictionary<
            string,
            WorkflowNodeDefinition> _nodes =
            new Dictionary<
                string,
                WorkflowNodeDefinition>(
                    StringComparer.OrdinalIgnoreCase);

        // ============================================================
        // REGISTER
        // ============================================================

        /// <summary>
        /// Đăng ký một Workflow Node mới.
        ///
        /// Không cho phép đăng ký trùng NodeKey.
        /// </summary>
        public void Register(
            WorkflowNodeDefinition definition)
        {
            ValidateDefinition(
                definition);

            if (_nodes.ContainsKey(
                    definition.NodeKey))
            {
                throw new InvalidOperationException(
                    $"Workflow node '{definition.NodeKey}' " +
                    "đã được đăng ký.");
            }

            _nodes.Add(
                definition.NodeKey,
                definition);
        }

        /// <summary>
        /// Đăng ký node hoặc thay thế node hiện tại
        /// nếu NodeKey đã tồn tại.
        /// </summary>
        public void RegisterOrReplace(
            WorkflowNodeDefinition definition)
        {
            ValidateDefinition(
                definition);

            _nodes[
                definition.NodeKey] =
                definition;
        }

        // ============================================================
        // GET
        // ============================================================

        /// <summary>
        /// Lấy Workflow Node theo NodeKey.
        ///
        /// Throw KeyNotFoundException nếu không tồn tại.
        /// </summary>
        public WorkflowNodeDefinition Get(
            string nodeKey)
        {
            ValidateNodeKey(
                nodeKey);

            if (!_nodes.TryGetValue(
                    nodeKey,
                    out WorkflowNodeDefinition? definition))
            {
                throw new KeyNotFoundException(
                    $"Không tìm thấy workflow node '{nodeKey}'.");
            }

            return definition;
        }

        /// <summary>
        /// Thử lấy Workflow Node theo NodeKey.
        ///
        /// Không throw nếu node không tồn tại.
        /// </summary>
        public bool TryGet(
            string nodeKey,
            out WorkflowNodeDefinition? definition)
        {
            if (string.IsNullOrWhiteSpace(
                    nodeKey))
            {
                definition = null;

                return false;
            }

            return _nodes.TryGetValue(
                nodeKey,
                out definition);
        }

        // ============================================================
        // QUERY
        // ============================================================

        /// <summary>
        /// Lấy snapshot toàn bộ Workflow Node hiện tại.
        ///
        /// Caller không thể thay đổi collection nội bộ
        /// của Registry thông qua kết quả trả về.
        /// </summary>
        public IReadOnlyCollection<WorkflowNodeDefinition>
            GetAll()
        {
            return new List<WorkflowNodeDefinition>(
                _nodes.Values);
        }

        /// <summary>
        /// Kiểm tra Workflow Node có tồn tại hay không.
        /// </summary>
        public bool Contains(
            string nodeKey)
        {
            return
                !string.IsNullOrWhiteSpace(
                    nodeKey)
                &&
                _nodes.ContainsKey(
                    nodeKey);
        }

        // ============================================================
        // REMOVE
        // ============================================================

        /// <summary>
        /// Xóa Workflow Node theo NodeKey.
        ///
        /// Trả về true nếu node tồn tại và đã được xóa.
        /// </summary>
        public bool Remove(
            string nodeKey)
        {
            if (string.IsNullOrWhiteSpace(
                    nodeKey))
            {
                return false;
            }

            return _nodes.Remove(
                nodeKey);
        }

        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Xóa toàn bộ Workflow Node khỏi Registry.
        /// </summary>
        public void Clear()
        {
            _nodes.Clear();
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private static void ValidateDefinition(
            WorkflowNodeDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            ValidateNodeKey(
                definition.NodeKey);
        }

        private static void ValidateNodeKey(
            string nodeKey)
        {
            if (string.IsNullOrWhiteSpace(
                    nodeKey))
            {
                throw new ArgumentException(
                    "NodeKey không được rỗng.",
                    nameof(nodeKey));
            }
        }
    }
}

