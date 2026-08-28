using System;
using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Workflow.Runtime
{
    /// <summary>
    /// Context runtime của một Workflow.
    ///
    /// Chứa dữ liệu được truyền xuyên suốt quá trình
    /// Workflow đang được thực thi.
    ///
    /// Không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    ///     - Repository
    ///     - Service cụ thể
    /// </summary>
    public sealed class WorkflowContext
    {
        private readonly Dictionary<string, object> _data =
            new Dictionary<string, object>(
                StringComparer.OrdinalIgnoreCase);

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public WorkflowContext(
            string workflowKey)
        {
            if (string.IsNullOrWhiteSpace(workflowKey))
            {
                throw new ArgumentException(
                    "WorkflowKey không được rỗng.",
                    nameof(workflowKey));
            }

            WorkflowKey = workflowKey;
        }

        // ============================================================
        // WORKFLOW
        // ============================================================

        /// <summary>
        /// Key của Workflow đang chạy.
        /// </summary>
        public string WorkflowKey
        {
            get;
        }

        /// <summary>
        /// Node hiện tại đang được xử lý.
        /// </summary>
        public string? CurrentNodeKey
        {
            get;
            set;
        }

        /// <summary>
        /// Node trước đó.
        /// </summary>
        public string? PreviousNodeKey
        {
            get;
            set;
        }

        // ============================================================
        // DATA
        // ============================================================

        /// <summary>
        /// Dữ liệu runtime dùng chung giữa các node.
        ///
        /// Ví dụ:
        ///     PhieuXuLyId
        ///     LotNo
        ///     SoLuong
        ///     SlotId
        ///     NguoiThucHien
        /// </summary>
        public IReadOnlyDictionary<string, object> Data
        {
            get
            {
                return _data;
            }
        }

        // ============================================================
        // SET
        // ============================================================

        /// <summary>
        /// Lưu một giá trị vào context.
        /// </summary>
        public void Set(
            string key,
            object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Context key không được rỗng.",
                    nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(
                    nameof(value));
            }

            _data[key] = value;
        }

        // ============================================================
        // CONTAINS
        // ============================================================

        /// <summary>
        /// Kiểm tra context có chứa key hay không.
        /// </summary>
        public bool Contains(
            string key)
        {
            return
                !string.IsNullOrWhiteSpace(key) &&
                _data.ContainsKey(key);
        }

        // ============================================================
        // GET
        // ============================================================

        /// <summary>
        /// Lấy giá trị từ context.
        ///
        /// Nếu key không tồn tại hoặc sai kiểu,
        /// phương thức sẽ ném exception.
        /// </summary>
        public T Get<T>(
            string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Context key không được rỗng.",
                    nameof(key));
            }

            if (!_data.TryGetValue(
                    key,
                    out object? rawValue))
            {
                throw new KeyNotFoundException(
                    $"WorkflowContext không tìm thấy key '{key}'.");
            }

            if (rawValue is not T typedValue)
            {
                throw new InvalidCastException(
                    $"Context key '{key}' không có kiểu '{typeof(T).Name}'.");
            }

            return typedValue;
        }

        // ============================================================
        // TRY GET
        // ============================================================

        /// <summary>
        /// Thử lấy giá trị từ context.
        ///
        /// Không ném exception nếu:
        ///     - key không tồn tại;
        ///     - giá trị không đúng kiểu.
        /// </summary>
        public bool TryGet<T>(
            string key,
            out T? value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = default;

                return false;
            }

            if (!_data.TryGetValue(
                    key,
                    out object? rawValue))
            {
                value = default;

                return false;
            }

            if (rawValue is not T typedValue)
            {
                value = default;

                return false;
            }

            value = typedValue;

            return true;
        }

        // ============================================================
        // REMOVE
        // ============================================================

        /// <summary>
        /// Xóa một giá trị khỏi context.
        /// </summary>
        public bool Remove(
            string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            return _data.Remove(key);
        }

        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Xóa toàn bộ dữ liệu runtime.
        /// </summary>
        public void Clear()
        {
            _data.Clear();
        }
    }
}