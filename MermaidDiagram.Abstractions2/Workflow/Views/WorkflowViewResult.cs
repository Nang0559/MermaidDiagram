
using System;
using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Views
{
    /// <summary>
    /// Kết quả trả về từ một Workflow View
    /// sau khi người dùng hoàn thành hoặc hủy thao tác.
    ///
    /// View chỉ trả kết quả.
    /// Workflow chịu trách nhiệm quyết định bước tiếp theo.
    /// </summary>
    public sealed class WorkflowViewResult
    {
        // ============================================================
        // STATUS
        // ============================================================

        /// <summary>
        /// View thực hiện thao tác thành công.
        /// </summary>
        public bool IsSuccess
        {
            get;
        }

        /// <summary>
        /// Người dùng chủ động hủy thao tác.
        /// </summary>
        public bool IsCancelled
        {
            get;
        }

        /// <summary>
        /// View không thể hoàn thành thao tác
        /// do lỗi nghiệp vụ hoặc lỗi thực thi.
        /// </summary>
        public bool IsFailed
        {
            get;
        }

        // ============================================================
        // MESSAGE
        // ============================================================

        /// <summary>
        /// Thông báo kết quả cho tầng Workflow/UI.
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
        ///     "CanRework"
        ///     "ChiGiaoBu"
        ///     "TuChoiGiaoBu"
        ///
        /// View chỉ trả về giá trị.
        /// Workflow quyết định action đó có hợp lệ hay không.
        /// </summary>
        public string? NextAction
        {
            get;
        }

        // ============================================================
        // DATA
        // ============================================================

        /// <summary>
        /// Dữ liệu bổ sung mà View trả về cho Workflow.
        ///
        /// Ví dụ:
        ///     SoLuongOK
        ///     SoLuongNG
        ///     QTChungStatus
        ///     SlotId
        /// </summary>
        public IReadOnlyDictionary<string, object> Data
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        private WorkflowViewResult(
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

        public static WorkflowViewResult Success()
        {
            return new WorkflowViewResult(
                true,
                false,
                false,
                null,
                null,
                null);
        }

        public static WorkflowViewResult Success(
            string nextAction)
        {
            return new WorkflowViewResult(
                true,
                false,
                false,
                null,
                nextAction,
                null);
        }

        public static WorkflowViewResult Success(
            string nextAction,
            IDictionary<string, object> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(
                    nameof(data));
            }

            return new WorkflowViewResult(
                true,
                false,
                false,
                null,
                nextAction,
                new Dictionary<string, object>(data));
        }

        // ============================================================
        // CANCEL
        // ============================================================

        public static WorkflowViewResult Cancelled(
            string? message = null)
        {
            return new WorkflowViewResult(
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

        public static WorkflowViewResult Failed(
            string message)
        {
            return new WorkflowViewResult(
                false,
                false,
                true,
                message,
                null,
                null);
        }
    }
}

