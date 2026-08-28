
using System;

namespace MermaidDiagram.Abstractions.Workflow.Views.Schema
{
    /// <summary>
    /// Định nghĩa một Field thuộc Workflow View Schema.
    ///
    /// WorkflowFieldDefinition chỉ chứa metadata của field.
    ///
    /// Không chứa:
    ///     - UI control
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    ///     - Business service
    ///     - Business logic
    ///
    /// Field Definition được Generic View Renderer sử dụng để biết:
    ///
    ///     - field nào cần render;
    ///     - kiểu dữ liệu;
    ///     - label;
    ///     - required;
    ///     - readonly;
    ///     - visible;
    ///     - order;
    ///     - placeholder;
    ///     - validation metadata.
    /// </summary>
    public sealed class WorkflowFieldDefinition
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        public string FieldKey
        {
            get;
        }

        public string FieldTitle
        {
            get;
        }

        public string? Description
        {
            get;
        }

        // ============================================================
        // TYPE
        // ============================================================

        public WorkflowFieldKind Kind
        {
            get;
        }

        /// <summary>
        /// Kiểu dữ liệu runtime của field.
        ///
        /// Có thể null đối với một số field mà renderer
        /// không cần kiểu dữ liệu cụ thể, ví dụ Collection.
        /// </summary>
        public Type? DataType
        {
            get;
        }

        // ============================================================
        // PRESENTATION
        // ============================================================

        public bool Visible
        {
            get;
        }

        public bool Required
        {
            get;
        }

        public bool ReadOnly
        {
            get;
        }

        public int Order
        {
            get;
        }

        public string? Placeholder
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public WorkflowFieldDefinition(
            string fieldKey,
            string fieldTitle,
            WorkflowFieldKind kind,
            Type? dataType = null,
            bool visible = true,
            bool required = false,
            bool readOnly = false,
            int order = 0,
            string? description = null,
            string? placeholder = null)
        {
            if (string.IsNullOrWhiteSpace(fieldKey))
            {
                throw new ArgumentException(
                    "FieldKey không được rỗng.",
                    nameof(fieldKey));
            }

            if (string.IsNullOrWhiteSpace(fieldTitle))
            {
                throw new ArgumentException(
                    "FieldTitle không được rỗng.",
                    nameof(fieldTitle));
            }

            if (order < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(order),
                    "Order không được nhỏ hơn 0.");
            }

            FieldKey =
                fieldKey.Trim();

            FieldTitle =
                fieldTitle.Trim();

            Description =
                string.IsNullOrWhiteSpace(description)
                    ? null
                    : description.Trim();

            Kind =
                kind;

            DataType =
                dataType;

            Visible =
                visible;

            Required =
                required;

            ReadOnly =
                readOnly;

            Order =
                order;

            Placeholder =
                string.IsNullOrWhiteSpace(placeholder)
                    ? null
                    : placeholder.Trim();

            Validate();
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Kiểm tra tính hợp lệ nội tại của Field Definition.
        ///
        /// Chỉ kiểm tra metadata/configuration.
        /// Không kiểm tra giá trị runtime.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FieldKey))
            {
                throw new InvalidOperationException(
                    "WorkflowFieldDefinition không có FieldKey.");
            }

            if (string.IsNullOrWhiteSpace(FieldTitle))
            {
                throw new InvalidOperationException(
                    $"Field '{FieldKey}' không có FieldTitle.");
            }

            if (Order < 0)
            {
                throw new InvalidOperationException(
                    $"Field '{FieldKey}' có Order không hợp lệ.");
            }

            if (Required &&
                ReadOnly)
            {
                throw new InvalidOperationException(
                    $"Field '{FieldKey}' không thể vừa Required vừa ReadOnly.");
            }

            ValidateKind();
        }

        // ============================================================
        // KIND VALIDATION
        // ============================================================

        private void ValidateKind()
        {
            switch (Kind)
            {
                case WorkflowFieldKind.Text:

                case WorkflowFieldKind.MultilineText:

                    ValidateExpectedType(
                        typeof(string));

                    break;

                case WorkflowFieldKind.Number:

                    ValidateExpectedType(
                        typeof(int));

                    break;

                case WorkflowFieldKind.Decimal:

                    ValidateExpectedType(
                        typeof(decimal));

                    break;

                case WorkflowFieldKind.Date:

                    ValidateExpectedType(
                        typeof(DateTime));

                    break;

                case WorkflowFieldKind.DateTime:

                    ValidateExpectedType(
                        typeof(DateTime));

                    break;

                case WorkflowFieldKind.Boolean:

                    ValidateExpectedType(
                        typeof(bool));

                    break;

                case WorkflowFieldKind.Collection:

                    // Collection được mô tả bởi
                    // WorkflowCollectionDefinition.
                    //
                    // Không ép DataType tại đây.
                    break;

                case WorkflowFieldKind.Select:

                    // Select có thể sử dụng string/int/enum
                    // tùy renderer và schema bổ sung.
                    break;

                case WorkflowFieldKind.Hidden:

                    // Hidden không yêu cầu DataType cụ thể.
                    break;

                default:

                    throw new InvalidOperationException(
                        $"Field '{FieldKey}' có WorkflowFieldKind " +
                        $"không hợp lệ: {Kind}.");
            }
        }

        // ============================================================
        // TYPE VALIDATION
        // ============================================================

        private void ValidateExpectedType(
            Type expectedType)
        {
            if (DataType == null)
            {
                return;
            }

            if (DataType != expectedType)
            {
                throw new InvalidOperationException(
                    $"Field '{FieldKey}' có Kind={Kind} " +
                    $"nhưng DataType='{DataType.Name}' " +
                    $"không phù hợp. " +
                    $"Expected='{expectedType.Name}'.");
            }
        }

        // ============================================================
        // HELPERS
        // ============================================================

        /// <summary>
        /// Field có phải Collection hay không.
        /// </summary>
        public bool IsCollection
        {
            get
            {
                return Kind ==
                       WorkflowFieldKind.Collection;
            }
        }

        /// <summary>
        /// Field có phải field nhập liệu hay không.
        /// </summary>
        public bool IsInput
        {
            get
            {
                return
                    Visible &&
                    !ReadOnly &&
                    Kind != WorkflowFieldKind.Hidden;
            }
        }

        /// <summary>
        /// Field có phải field ẩn hay không.
        /// </summary>
        public bool IsHidden
        {
            get
            {
                return Kind ==
                       WorkflowFieldKind.Hidden;
            }
        }

        // ============================================================
        // DISPLAY
        // ============================================================

        public override string ToString()
        {
            return FieldKey;
        }
    }
}

