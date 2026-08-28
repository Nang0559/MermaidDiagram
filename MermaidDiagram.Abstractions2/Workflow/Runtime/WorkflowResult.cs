
using System;
using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Workflow.Runtime
{
    /// <summary>
    /// Kết quả cuối cùng của một Workflow.
    ///
    /// Đây là kết quả ở cấp Workflow,
    /// khác với WorkflowDispatchResult là kết quả
    /// của một lần dispatch node.
    ///
    /// Không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    ///     - Business service cụ thể
    /// </summary>
    public sealed class WorkflowResult
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
        /// Thông báo kết quả Workflow.
        ///
        /// Có thể null nếu Workflow hoàn thành
        /// mà không cần thông báo.
        /// </summary>
        public string? Message
        {
            get;
        }

        // ============================================================
        // NODE
        // ============================================================

        /// <summary>
        /// Node cuối cùng được xử lý.
        ///
        /// Có thể null nếu Workflow thất bại
        /// trước khi xác định được node hiện tại.
        /// </summary>
        public string? CurrentNodeKey
        {
            get;
        }

        // ============================================================
        // DATA
        // ============================================================

        /// <summary>
        /// Dữ liệu kết quả cuối cùng của Workflow.
        ///
        /// Luôn trả về dictionary, kể cả khi không có dữ liệu.
        /// </summary>
        public IReadOnlyDictionary<string, object> Data
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        private WorkflowResult(
            bool isSuccess,
            bool isCancelled,
            bool isFailed,
            string? message,
            string? currentNodeKey,
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

            CurrentNodeKey =
                currentNodeKey;

            Data =
                data
                ?? new Dictionary<string, object>();
        }

        // ============================================================
        // SUCCESS
        // ============================================================

        public static WorkflowResult Success()
        {
            return new WorkflowResult(
                true,
                false,
                false,
                null,
                null,
                null);
        }

        public static WorkflowResult Success(
            string? currentNodeKey)
        {
            return new WorkflowResult(
                true,
                false,
                false,
                null,
                currentNodeKey,
                null);
        }

        public static WorkflowResult Success(
            string? currentNodeKey,
            IReadOnlyDictionary<string, object> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(
                    nameof(data));
            }

            return new WorkflowResult(
                true,
                false,
                false,
                null,
                currentNodeKey,
                data);
        }

        // ============================================================
        // CANCELLED
        // ============================================================

        public static WorkflowResult Cancelled(
            string? message = null,
            string? currentNodeKey = null)
        {
            return new WorkflowResult(
                false,
                true,
                false,
                message,
                currentNodeKey,
                null);
        }

        // ============================================================
        // FAILED
        // ============================================================

        public static WorkflowResult Failed(
            string message,
            string? currentNodeKey = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "Message không được rỗng.",
                    nameof(message));
            }

            return new WorkflowResult(
                false,
                false,
                true,
                message,
                currentNodeKey,
                null);
        }

        // ============================================================
        // HELPERS
        // ============================================================

        public bool HasCurrentNode
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(
                        CurrentNodeKey);
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

