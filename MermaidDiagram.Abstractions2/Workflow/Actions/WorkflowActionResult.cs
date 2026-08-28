using System;
using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Workflow.Actions
{
    /// <summary>
    /// Kết quả trả về sau khi Workflow Action được thực thi.
    /// </summary>
    public sealed class WorkflowActionResult
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
        /// Thông báo kết quả cho tầng phía trên.
        /// </summary>
        public string? Message
        {
            get;
        }

        // ============================================================
        // NEXT ACTION
        // ============================================================

        /// <summary>
        /// Action tiếp theo mà Workflow có thể thực hiện.
        ///
        /// Ví dụ:
        ///     "QCDinhHuong"
        ///     "XuatKhoRework"
        ///     "NhapLaiHangNG"
        /// </summary>
        public string? NextAction
        {
            get;
        }

        // ============================================================
        // DATA
        // ============================================================

        /// <summary>
        /// Dữ liệu bổ sung mà Action trả về.
        ///
        /// Ví dụ:
        ///     SoLuongOK
        ///     SoLuongNG
        ///     SlotId
        ///     QTChungStatus
        /// </summary>
        public IReadOnlyDictionary<string, object> Data
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        private WorkflowActionResult(
            bool isSuccess,
            bool isCancelled,
            bool isFailed,
            string? message,
            string? nextAction,
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

            NextAction =
                nextAction;

            Data =
                data
                ?? new Dictionary<string, object>();
        }

        // ============================================================
        // SUCCESS
        // ============================================================

        public static WorkflowActionResult Success()
        {
            return new WorkflowActionResult(
                true,
                false,
                false,
                null,
                null,
                null);
        }

        public static WorkflowActionResult Success(
            string nextAction)
        {
            return new WorkflowActionResult(
                true,
                false,
                false,
                null,
                nextAction,
                null);
        }

        public static WorkflowActionResult Success(
            string nextAction,
            IDictionary<string, object> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(
                    nameof(data));
            }

            return new WorkflowActionResult(
                true,
                false,
                false,
                null,
                nextAction,
                new Dictionary<string, object>(
                    data));
        }

        // ============================================================
        // CANCEL
        // ============================================================

        public static WorkflowActionResult Cancelled(
            string? message = null)
        {
            return new WorkflowActionResult(
                false,
                true,
                false,
                message,
                null,
                null);
        }

        // ============================================================
        // FAILURE
        // ============================================================

        public static WorkflowActionResult Failed(
            string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "Message không được rỗng.",
                    nameof(message));
            }

            return new WorkflowActionResult(
                false,
                false,
                true,
                message,
                null,
                null);
        }
    }
}