
using MermaidDiagram.Abstractions.Workflow.Views;
using MermaidDiagram.Abstractions.Workflow.Views.Schema;

namespace MermaidDiagram.Core.Workflow.Views.Schema
{
    /// <summary>
    /// Builder tạo WorkflowViewSchema.
    ///
    /// Builder chỉ chịu trách nhiệm xây dựng schema.
    ///
    /// Không chứa:
    ///     - Business logic
    ///     - WorkflowActionDefinition
    ///     - UI framework
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    /// </summary>
    public sealed class WorkflowViewSchemaBuilder
    {
        // ============================================================
        // CREATE
        // ============================================================

        /// <summary>
        /// Tạo một WorkflowViewSchema mới.
        /// </summary>
        public WorkflowViewSchema Create(
            string viewKey,
            string viewTitle,
            WorkflowViewMode viewMode =
                WorkflowViewMode.Default)
        {
            return new WorkflowViewSchema(
                viewKey,
                viewTitle,
                viewMode);
        }

        // ============================================================
        // ADD FIELD
        // ============================================================

        /// <summary>
        /// Thêm một field vào schema.
        /// </summary>
        public WorkflowViewSchema AddField(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition field)
        {
            ValidateSchema(
                schema);

            if (field == null)
            {
                throw new ArgumentNullException(
                    nameof(field));
            }

            schema.AddField(
                field);

            return schema;
        }

        // ============================================================
        // ADD COLLECTION
        // ============================================================

        /// <summary>
        /// Thêm collection field vào schema.
        ///
        /// Collection được biểu diễn như một
        /// WorkflowFieldDefinition có type Collection,
        /// trong đó CollectionDefinition chứa các child fields.
        ///
        /// Builder không tự động tạo Action.
        /// </summary>
        public WorkflowViewSchema AddCollection(
            WorkflowViewSchema schema,
            WorkflowFieldDefinition collectionField)
        {
            ValidateSchema(
                schema);

            if (collectionField == null)
            {
                throw new ArgumentNullException(
                    nameof(collectionField));
            }

            schema.AddField(
                collectionField);

            return schema;
        }

        // ============================================================
        // BUILD
        // ============================================================

        /// <summary>
        /// Hoàn tất schema.
        ///
        /// Build sẽ validate schema trước khi trả về.
        /// </summary>
        public WorkflowViewSchema Build(
            WorkflowViewSchema schema)
        {
            ValidateSchema(
                schema);

            schema.Validate();

            return schema;
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private static void ValidateSchema(
            WorkflowViewSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(
                    nameof(schema));
            }
        }
    }
}

