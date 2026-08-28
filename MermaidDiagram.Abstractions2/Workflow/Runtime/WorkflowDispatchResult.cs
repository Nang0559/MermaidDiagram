
using System;
using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Workflow.Runtime
{
    /// <summary>
    /// Kết quả sau khi WorkflowNodeDispatcher
    /// xử lý một Workflow Node.
    ///
    /// Chỉ chứa kết quả runtime.
    /// Không chứa business logic.
    /// Không phụ thuộc UI framework.
    /// </summary>
    public sealed class WorkflowDispatchResult
    {
        // ============================================================
        // STATUS
        // ============================================================

        public bool IsSuccess
        {
            get;
        }

        public bool IsCancelled
        {
            get;
        }

        public bool IsFailed
        {
            get;
        }

        // ============================================================
        // MESSAGE
        // ============================================================

        /// <summary>
        /// Thông báo kết quả.
        ///
        /// Có thể null nếu operation thành công
        /// và không cần thông báo.
        /// </summary>
        public string? Message
        {
            get;
        }

        // ============================================================
        // NODE
        // ============================================================

        /// <summary>
        /// Node vừa được Dispatcher xử lý.
        ///
        /// Có thể null nếu lỗi xảy ra trước khi xác định node.
        /// </summary>
        public string? NodeKey
        {
            get;
        }

        /// <summary>
        /// Node tiếp theo mà Workflow có thể chuyển tới.
        ///
        /// Có thể null nếu:
        ///     - workflow kết thúc;
        ///     - operation bị cancel;
        ///     - operation thất bại;
        ///     - chưa xác định node tiếp theo.
        /// </summary>
        public string? NextNodeKey
        {
            get;
        }

        // ============================================================
        // DATA
        // ============================================================

        /// <summary>
        /// Dữ liệu runtime bổ sung được trả về.
        ///
        /// Luôn tồn tại một dictionary, kể cả khi
        /// không có dữ liệu.
        /// </summary>
        public IReadOnlyDictionary<string, object> Data
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        private WorkflowDispatchResult(
            bool isSuccess,
            bool isCancelled,
            bool isFailed,
            string? message,
            string? nodeKey,
            string? nextNodeKey,
            IReadOnlyDictionary<string, object>? data)
        {
            IsSuccess =
                isSuccess;

            IsCancelled =
                isCancelled;

            IsFailed =
                isFailed;

            Message =
                message;

            NodeKey =
                nodeKey;

            NextNodeKey =
                nextNodeKey;

            Data =
                data
                ?? new Dictionary<string, object>();
        }

        // ============================================================
        // SUCCESS
        // ============================================================

        public static WorkflowDispatchResult Success(
            string? nodeKey = null,
            string? nextNodeKey = null)
        {
            return new WorkflowDispatchResult(
                true,
                false,
                false,
                null,
                nodeKey,
                nextNodeKey,
                null);
        }

        public static WorkflowDispatchResult Success(
            string? nodeKey,
            string? nextNodeKey,
            IReadOnlyDictionary<string, object> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(
                    nameof(data));
            }

            return new WorkflowDispatchResult(
                true,
                false,
                false,
                null,
                nodeKey,
                nextNodeKey,
                data);
        }

        // ============================================================
        // CANCELLED
        // ============================================================

        public static WorkflowDispatchResult Cancelled(
            string? message = null,
            string? nodeKey = null)
        {
            return new WorkflowDispatchResult(
                false,
                true,
                false,
                message,
                nodeKey,
                null,
                null);
        }

        // ============================================================
        // FAILED
        // ============================================================

        public static WorkflowDispatchResult Failed(
            string message,
            string? nodeKey = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "Message không được rỗng.",
                    nameof(message));
            }

            return new WorkflowDispatchResult(
                false,
                false,
                true,
                message,
                nodeKey,
                null,
                null);
        }

        // ============================================================
        // HELPERS
        // ============================================================

        public bool HasNextNode
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(
                        NextNodeKey);
            }
        }

        public bool HasData
        {
            get
            {
                return Data.Count > 0;
            }
        }
    }
}

