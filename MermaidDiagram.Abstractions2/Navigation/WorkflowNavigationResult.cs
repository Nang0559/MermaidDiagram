using System;

using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Navigation
{
    /// <summary>
    /// Kết quả trả về sau khi Workflow Navigator
    /// xử lý một WorkflowNavigationRequest.
    ///
    /// Không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    ///     - UI framework cụ thể.
    /// </summary>
    public sealed class WorkflowNavigationResult
    {
        // ============================================================
        // STATUS
        // ============================================================

        /// <summary>
        /// Navigation được thực hiện thành công.
        /// </summary>
        public bool IsSuccess
        {
            get;
        }

        /// <summary>
        /// Navigation bị người dùng hoặc Workflow hủy.
        /// </summary>
        public bool IsCancelled
        {
            get;
        }

        /// <summary>
        /// Navigation thất bại do lỗi hoặc không thể
        /// xác định destination.
        /// </summary>
        public bool IsFailed
        {
            get;
        }

        // ============================================================
        // DESTINATION
        // ============================================================

        /// <summary>
        /// Key của View được navigation tới.
        ///
        /// Có thể null khi:
        ///     - Cancelled
        ///     - Failed
        ///     - Không có View đích.
        /// </summary>
        public string? ViewKey
        {
            get;
        }

        // ============================================================
        // MESSAGE
        // ============================================================

        /// <summary>
        /// Thông báo kết quả navigation.
        /// </summary>
        public string? Message
        {
            get;
        }

        // ============================================================
        // DATA
        // ============================================================

        /// <summary>
        /// Dữ liệu bổ sung do Navigator trả về.
        ///
        /// Ví dụ:
        ///     PreviousViewKey
        ///     NavigationMode
        ///     WorkflowNodeKey
        /// </summary>
        public IReadOnlyDictionary<string, object> Data
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        private WorkflowNavigationResult(
            bool isSuccess,
            bool isCancelled,
            bool isFailed,
            string? viewKey,
            string? message,
            IReadOnlyDictionary<string, object>? data)
        {
            IsSuccess =
                isSuccess;

            IsCancelled =
                isCancelled;

            IsFailed =
                isFailed;

            ViewKey =
                viewKey;

            Message =
                message;

            Data =
                data
                ?? new Dictionary<string, object>();
        }

        // ============================================================
        // SUCCESS
        // ============================================================

        public static WorkflowNavigationResult Success(
            string viewKey)
        {
            if (string.IsNullOrWhiteSpace(viewKey))
            {
                throw new ArgumentException(
                    "ViewKey không được rỗng.",
                    nameof(viewKey));
            }

            return new WorkflowNavigationResult(
                true,
                false,
                false,
                viewKey,
                null,
                null);
        }

        public static WorkflowNavigationResult Success(
            string viewKey,
            IDictionary<string, object> data)
        {
            if (string.IsNullOrWhiteSpace(viewKey))
            {
                throw new ArgumentException(
                    "ViewKey không được rỗng.",
                    nameof(viewKey));
            }

            if (data == null)
            {
                throw new ArgumentNullException(
                    nameof(data));
            }

            return new WorkflowNavigationResult(
                true,
                false,
                false,
                viewKey,
                null,
                new Dictionary<string, object>(data));
        }

        // ============================================================
        // CANCEL
        // ============================================================

        public static WorkflowNavigationResult Cancelled(
            string? message = null)
        {
            return new WorkflowNavigationResult(
                false,
                true,
                false,
                null,
                message,
                null);
        }

        // ============================================================
        // FAILURE
        // ============================================================

        public static WorkflowNavigationResult Failed(
            string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "Message không được rỗng.",
                    nameof(message));
            }

            return new WorkflowNavigationResult(
                false,
                false,
                true,
                null,
                message,
                null);
        }
    }
}

