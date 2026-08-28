
using MermaidDiagram.Abstractions.Workflow.Views;
using MermaidDiagram.Abstractions.Workflow.Views.Schema;

namespace MermaidDiagram.Core.Workflow.Views.Schema
{
    /// <summary>
    /// Validator cho WorkflowViewSchema.
    ///
    /// Chịu trách nhiệm kiểm tra:
    ///
    ///     - View Schema hợp lệ
    ///     - ViewKey / ViewTitle
    ///     - FieldKey không trùng
    ///     - Field Definition hợp lệ
    ///     - Order hợp lệ
    ///     - Collection field hợp lệ ở mức metadata
    ///
    /// Không chịu trách nhiệm:
    ///
    ///     - Business validation
    ///     - Workflow transition
    ///     - Action execution
    ///     - UI framework
    ///     - DevExpress
    ///     - WinForms
    /// </summary>
    public sealed class WorkflowViewSchemaValidator
    {
        // ============================================================
        // VALIDATE SCHEMA
        // ============================================================

        /// <summary>
        /// Validate toàn bộ WorkflowViewSchema.
        ///
        /// Ném InvalidOperationException nếu schema không hợp lệ.
        /// </summary>
        public void Validate(
            WorkflowViewSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(
                    nameof(schema));
            }

            ValidateIdentity(
                schema);

            ValidateFields(
                schema);
        }

        // ============================================================
        // VALIDATE IDENTITY
        // ============================================================

        private static void ValidateIdentity(
            WorkflowViewSchema schema)
        {
            if (string.IsNullOrWhiteSpace(
                    schema.ViewKey))
            {
                throw new InvalidOperationException(
                    "WorkflowViewSchema không có ViewKey.");
            }

            if (string.IsNullOrWhiteSpace(
                    schema.ViewTitle))
            {
                throw new InvalidOperationException(
                    $"WorkflowViewSchema '{schema.ViewKey}' " +
                    "không có ViewTitle.");
            }
        }

        // ============================================================
        // VALIDATE FIELDS
        // ============================================================

        private static void ValidateFields(
            WorkflowViewSchema schema)
        {
            if (schema.Fields == null)
            {
                throw new InvalidOperationException(
                    $"WorkflowViewSchema '{schema.ViewKey}' " +
                    "không có Fields.");
            }

            HashSet<string> fieldKeys =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            int previousOrder = -1;

            for (int i = 0;
                 i < schema.Fields.Count;
                 i++)
            {
                WorkflowFieldDefinition field =
                    schema.Fields[i];

                if (field == null)
                {
                    throw new InvalidOperationException(
                        $"WorkflowViewSchema '{schema.ViewKey}' " +
                        $"chứa Field null tại index {i}.");
                }

                ValidateField(
                    schema,
                    field,
                    fieldKeys);

                // ----------------------------------------------------
                // ORDER
                // ----------------------------------------------------

                if (field.Order < 0)
                {
                    throw new InvalidOperationException(
                        $"View '{schema.ViewKey}', Field " +
                        $"'{field.FieldKey}' có Order không hợp lệ.");
                }

                if (field.Order < previousOrder)
                {
                    throw new InvalidOperationException(
                        $"View '{schema.ViewKey}' có thứ tự Field không hợp lệ. " +
                        $"Field '{field.FieldKey}' có Order={field.Order}, " +
                        $"nhỏ hơn Order trước đó={previousOrder}.");
                }

                previousOrder =
                    field.Order;
            }
        }

        // ============================================================
        // VALIDATE FIELD
        // ============================================================

        private static void ValidateField(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition field,
            HashSet<string> fieldKeys)
        {
            // --------------------------------------------------------
            // FIELD KEY
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    field.FieldKey))
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}' " +
                    "chứa Field không có FieldKey.");
            }

            if (!fieldKeys.Add(
                    field.FieldKey))
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}' có FieldKey " +
                    $"trùng '{field.FieldKey}'.");
            }

            // --------------------------------------------------------
            // FIELD TITLE
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    field.FieldTitle))
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}', Field " +
                    $"'{field.FieldKey}' không có FieldTitle.");
            }

            // --------------------------------------------------------
            // REQUIRED / READONLY
            // --------------------------------------------------------

            if (field.Required &&
                field.ReadOnly)
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}', Field " +
                    $"'{field.FieldKey}' không thể vừa Required " +
                    "vừa ReadOnly.");
            }

            // --------------------------------------------------------
            // HIDDEN
            // --------------------------------------------------------

            if (field.Kind ==
                WorkflowFieldKind.Hidden &&
                field.Visible)
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}', Field " +
                    $"'{field.FieldKey}' có Kind=Hidden " +
                    "nhưng Visible=true.");
            }

            // --------------------------------------------------------
            // FIELD DEFINITION
            // --------------------------------------------------------

            field.Validate();

            // --------------------------------------------------------
            // KIND-SPECIFIC VALIDATION
            // --------------------------------------------------------

            ValidateFieldKind(
                schema,
                field);
        }

        // ============================================================
        // VALIDATE FIELD KIND
        // ============================================================

        private static void ValidateFieldKind(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition field)
        {
            switch (field.Kind)
            {
                // ----------------------------------------------------
                // TEXT
                // ----------------------------------------------------

                case WorkflowFieldKind.Text:

                case WorkflowFieldKind.MultilineText:

                    ValidateStringField(
                        schema,
                        field);

                    break;

                // ----------------------------------------------------
                // NUMBER
                // ----------------------------------------------------

                case WorkflowFieldKind.Number:

                    ValidateNumberField(
                        schema,
                        field);

                    break;

                // ----------------------------------------------------
                // DECIMAL
                // ----------------------------------------------------

                case WorkflowFieldKind.Decimal:

                    ValidateDecimalField(
                        schema,
                        field);

                    break;

                // ----------------------------------------------------
                // DATE
                // ----------------------------------------------------

                case WorkflowFieldKind.Date:

                case WorkflowFieldKind.DateTime:

                    ValidateDateField(
                        schema,
                        field);

                    break;

                // ----------------------------------------------------
                // BOOLEAN
                // ----------------------------------------------------

                case WorkflowFieldKind.Boolean:

                    ValidateBooleanField(
                        schema,
                        field);

                    break;

                // ----------------------------------------------------
                // COLLECTION
                // ----------------------------------------------------

                case WorkflowFieldKind.Collection:

                    ValidateCollectionField(
                        schema,
                        field);

                    break;

                // ----------------------------------------------------
                // SELECT
                // ----------------------------------------------------

                case WorkflowFieldKind.Select:

                    ValidateSelectField(
                        schema,
                        field);

                    break;

                // ----------------------------------------------------
                // HIDDEN
                // ----------------------------------------------------

                case WorkflowFieldKind.Hidden:

                    break;

                // ----------------------------------------------------
                // UNKNOWN
                // ----------------------------------------------------

                default:

                    throw new InvalidOperationException(
                        $"View '{schema.ViewKey}', Field " +
                        $"'{field.FieldKey}' có WorkflowFieldKind " +
                        $"không được hỗ trợ: {field.Kind}.");
            }
        }

        // ============================================================
        // STRING
        // ============================================================

        private static void ValidateStringField(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition field)
        {
            if (field.DataType != null &&
                field.DataType != typeof(string))
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}', Field " +
                    $"'{field.FieldKey}' có Kind={field.Kind} " +
                    $"nhưng DataType='{field.DataType.Name}'. " +
                    "Expected=String.");
            }
        }

        // ============================================================
        // NUMBER
        // ============================================================

        private static void ValidateNumberField(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition field)
        {
            if (field.DataType != null &&
                field.DataType != typeof(int))
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}', Field " +
                    $"'{field.FieldKey}' có Kind=Number " +
                    $"nhưng DataType='{field.DataType.Name}'. " +
                    "Expected=Int32.");
            }
        }

        // ============================================================
        // DECIMAL
        // ============================================================

        private static void ValidateDecimalField(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition field)
        {
            if (field.DataType != null &&
                field.DataType != typeof(decimal))
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}', Field " +
                    $"'{field.FieldKey}' có Kind=Decimal " +
                    $"nhưng DataType='{field.DataType.Name}'. " +
                    "Expected=Decimal.");
            }
        }

        // ============================================================
        // DATE
        // ============================================================

        private static void ValidateDateField(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition field)
        {
            if (field.DataType != null &&
                field.DataType != typeof(DateTime))
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}', Field " +
                    $"'{field.FieldKey}' có Kind={field.Kind} " +
                    $"nhưng DataType='{field.DataType.Name}'. " +
                    "Expected=DateTime.");
            }
        }

        // ============================================================
        // BOOLEAN
        // ============================================================

        private static void ValidateBooleanField(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition field)
        {
            if (field.DataType != null &&
                field.DataType != typeof(bool))
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}', Field " +
                    $"'{field.FieldKey}' có Kind=Boolean " +
                    $"nhưng DataType='{field.DataType.Name}'. " +
                    "Expected=Boolean.");
            }
        }

        // ============================================================
        // COLLECTION
        // ============================================================

        private static void ValidateCollectionField(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition field)
        {
            // --------------------------------------------------------
            // IMPORTANT:
            //
            // WorkflowFieldDefinition KHÔNG có:
            //
            //     field.Collection
            //
            // Cấu trúc Collection được mô tả bởi
            // WorkflowCollectionDefinition.
            //
            // Vì vậy Validator của Field chỉ kiểm tra:
            //
            //     Kind == Collection
            //
            // Không tự truy cập field.Collection.
            // --------------------------------------------------------

            if (!field.IsCollection)
            {
                throw new InvalidOperationException(
                    $"View '{schema.ViewKey}', Field " +
                    $"'{field.FieldKey}' được xử lý như Collection " +
                    "nhưng IsCollection=false.");
            }

            if (field.Required &&
                field.ReadOnly)
            {
                throw new InvalidOperationException(
                    $"Collection Field '{field.FieldKey}' " +
                    "không thể vừa Required vừa ReadOnly.");
            }
        }

        // ============================================================
        // SELECT
        // ============================================================

        private static void ValidateSelectField(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition field)
        {
            // Select có thể là:
            //
            //     string
            //     int
            //     enum
            //
            // nên không ép một DataType duy nhất ở đây.
            //
            // Việc định nghĩa option/source của Select
            // thuộc schema mở rộng hoặc renderer contract.
        }
    }
}

