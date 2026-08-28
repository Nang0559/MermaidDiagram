
using MermaidDiagram.Abstractions.Workflow.Runtime;
using MermaidDiagram.Abstractions.Workflow.Views;
using MermaidDiagram.Abstractions.Workflow.Views.Schema;

namespace MermaidDiagram.Core.Workflow.Views.Schema
{
    /// <summary>
    /// Đọc giá trị field từ WorkflowContext.
    ///
    /// WorkflowFieldValueReader chỉ chịu trách nhiệm:
    ///
    ///     WorkflowContext
    ///          ↓
    ///     FieldKey
    ///          ↓
    ///     Value
    ///
    /// Không chịu trách nhiệm:
    ///     - Validate business
    ///     - Render UI
    ///     - Execute Action
    ///     - Execute View
    ///     - Convert sang control của UI framework
    /// </summary>
    public sealed class WorkflowFieldValueReader
    {
        // ============================================================
        // READ
        // ============================================================

        /// <summary>
        /// Đọc giá trị của một field từ WorkflowContext.
        ///
        /// Nếu context không chứa field:
        ///     trả về null.
        ///
        /// Nếu field không hợp lệ:
        ///     throw exception.
        /// </summary>
        public object? Read(
            WorkflowContext context,
            WorkflowFieldDefinition field)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (field == null)
            {
                throw new ArgumentNullException(
                    nameof(field));
            }

            if (string.IsNullOrWhiteSpace(
                    field.FieldKey))
            {
                throw new InvalidOperationException(
                    "WorkflowFieldDefinition không có FieldKey.");
            }

            return Read(
                context,
                field.FieldKey);
        }

        /// <summary>
        /// Đọc trực tiếp một value theo FieldKey.
        ///
        /// Không có key thì trả về null.
        /// </summary>
        public object? Read(
            WorkflowContext context,
            string fieldKey)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (string.IsNullOrWhiteSpace(
                    fieldKey))
            {
                throw new ArgumentException(
                    "FieldKey không được rỗng.",
                    nameof(fieldKey));
            }

            if (!context.Contains(fieldKey))
            {
                return null;
            }

            if (context.TryGet<object>(
                    fieldKey,
                    out object? value))
            {
                return value;
            }

            return null;
        }

        // ============================================================
        // TYPED READ
        // ============================================================

        /// <summary>
        /// Đọc value theo kiểu dữ liệu mong muốn.
        ///
        /// Nếu không tồn tại:
        ///     trả về default(T).
        ///
        /// Nếu tồn tại nhưng sai kiểu:
        ///     throw InvalidCastException.
        /// </summary>
        public T? Read<T>(
            WorkflowContext context,
            WorkflowFieldDefinition field)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (field == null)
            {
                throw new ArgumentNullException(
                    nameof(field));
            }

            return Read<T>(
                context,
                field.FieldKey);
        }

        /// <summary>
        /// Đọc value typed theo FieldKey.
        /// </summary>
        public T? Read<T>(
            WorkflowContext context,
            string fieldKey)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (string.IsNullOrWhiteSpace(
                    fieldKey))
            {
                throw new ArgumentException(
                    "FieldKey không được rỗng.",
                    nameof(fieldKey));
            }

            if (!context.Contains(fieldKey))
            {
                return default;
            }

            return context.Get<T>(
                fieldKey);
        }

        // ============================================================
        // TRY READ
        // ============================================================

        /// <summary>
        /// Thử đọc field.
        ///
        /// Không throw nếu:
        ///     - FieldKey rỗng
        ///     - Context không có value
        ///     - Value sai kiểu
        /// </summary>
        public bool TryRead<T>(
            WorkflowContext context,
            WorkflowFieldDefinition field,
            out T? value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (field == null)
            {
                throw new ArgumentNullException(
                    nameof(field));
            }

            return TryRead(
                context,
                field.FieldKey,
                out value);
        }

        /// <summary>
        /// Thử đọc value typed theo FieldKey.
        /// </summary>
        public bool TryRead<T>(
            WorkflowContext context,
            string fieldKey,
            out T? value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (string.IsNullOrWhiteSpace(
                    fieldKey))
            {
                value = default;

                return false;
            }

            return context.TryGet<T>(
                fieldKey,
                out value);
        }

        // ============================================================
        // EXISTS
        // ============================================================

        /// <summary>
        /// Kiểm tra field đã có value trong Context hay chưa.
        /// </summary>
        public bool Exists(
            WorkflowContext context,
            string fieldKey)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (string.IsNullOrWhiteSpace(
                    fieldKey))
            {
                return false;
            }

            return context.Contains(
                fieldKey);
        }
    }
}

