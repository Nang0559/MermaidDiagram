

using DevExpress.XtraEditors;

using MermaidDiagram.Abstractions.Workflow.Views;
using MermaidDiagram.Abstractions.Workflow.Views.Schema;

namespace MermaidDiagram.DevExpress.ViewRendering
{
    /// <summary>
    /// Renderer DevExpress cho Workflow View.
    ///
    /// Trách nhiệm:
    ///
    ///     WorkflowViewSchema
    ///             ↓
    ///     Field / Collection
    ///             ↓
    ///     DevExpress Controls
    ///
    /// Renderer này chỉ chịu trách nhiệm:
    ///
    ///     - tạo container View;
    ///     - render các WorkflowFieldDefinition;
    ///     - render WorkflowCollectionDefinition được truyền vào;
    ///     - sắp xếp field theo Order;
    ///     - bỏ qua field Visible=false;
    ///     - không render Hidden field;
    ///     - phối hợp với các renderer chuyên biệt.
    ///
    /// Không chịu trách nhiệm:
    ///
    ///     - parse Mermaid;
    ///     - workflow dispatch;
    ///     - action execution;
    ///     - business logic;
    ///     - validation business;
    ///     - navigation;
    ///     - tạo WorkflowDefinition;
    ///     - tạo WorkflowViewSchema.
    ///
    /// IMPORTANT:
    ///
    /// WorkflowViewSchema hiện tại chỉ chứa:
    ///
    ///     IReadOnlyList<WorkflowFieldDefinition> Fields
    ///
    /// WorkflowCollectionDefinition không nằm trong WorkflowViewSchema.
    ///
    /// Vì vậy collection definitions phải được truyền riêng cho renderer.
    /// </summary>
    public sealed class DevExpressWorkflowViewRenderer
    {
        // ============================================================
        // DEPENDENCIES
        // ============================================================

        private readonly DevExpressControlFactory _controlFactory;

        private readonly DevExpressFieldRenderer _fieldRenderer;

        private readonly DevExpressCollectionRenderer
            _collectionRenderer;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public DevExpressWorkflowViewRenderer(
            DevExpressControlFactory controlFactory,
            DevExpressFieldRenderer fieldRenderer,
            DevExpressCollectionRenderer collectionRenderer)
        {
            _controlFactory =
                controlFactory
                ?? throw new ArgumentNullException(
                    nameof(controlFactory));

            _fieldRenderer =
                fieldRenderer
                ?? throw new ArgumentNullException(
                    nameof(fieldRenderer));

            _collectionRenderer =
                collectionRenderer
                ?? throw new ArgumentNullException(
                    nameof(collectionRenderer));
        }

        // ============================================================
        // RENDER
        // ============================================================

        /// <summary>
        /// Render WorkflowViewSchema vào container DevExpress.
        ///
        /// Collection definitions được truyền riêng vì
        /// WorkflowViewSchema hiện tại không sở hữu collection list.
        /// </summary>
        public PanelControl Render(
            WorkflowViewSchema schema,
            IEnumerable<WorkflowCollectionDefinition>?
                collectionDefinitions = null)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(
                    nameof(schema));
            }

            schema.Validate();

            PanelControl viewPanel =
                _controlFactory.CreateViewPanel();

            RenderInto(
                viewPanel,
                schema,
                collectionDefinitions);

            return viewPanel;
        }

        // ============================================================
        // RENDER INTO
        // ============================================================

        /// <summary>
        /// Render schema vào một container có sẵn.
        /// </summary>
        public void RenderInto(
            Control container,
            WorkflowViewSchema schema,
            IEnumerable<WorkflowCollectionDefinition>?
                collectionDefinitions = null)
        {
            if (container == null)
            {
                throw new ArgumentNullException(
                    nameof(container));
            }

            if (schema == null)
            {
                throw new ArgumentNullException(
                    nameof(schema));
            }

            schema.Validate();

            IReadOnlyList<WorkflowCollectionDefinition>
                collections =
                    NormalizeCollections(
                        collectionDefinitions);

            RenderFields(
                container,
                schema,
                collections);
        }

        // ============================================================
        // FIELDS
        // ============================================================

        /// <summary>
        /// Render toàn bộ field theo Order.
        /// </summary>
        private void RenderFields(
            Control container,
            WorkflowViewSchema schema,
            IReadOnlyList<WorkflowCollectionDefinition>
                collections)
        {
            List<WorkflowFieldDefinition> fields =
                schema.Fields
                    .Where(
                        field =>
                            field != null)
                    .OrderBy(
                        field =>
                            field.Order)
                    .ToList();

            for (int i = 0;
                 i < fields.Count;
                 i++)
            {
                WorkflowFieldDefinition field =
                    fields[i];

                // ----------------------------------------------------
                // INVISIBLE
                // ----------------------------------------------------

                if (!field.Visible)
                {
                    continue;
                }

                // ----------------------------------------------------
                // HIDDEN
                // ----------------------------------------------------

                if (field.IsHidden)
                {
                    continue;
                }

                // ----------------------------------------------------
                // COLLECTION
                // ----------------------------------------------------

                if (field.IsCollection)
                {
                    RenderCollection(
                        container,
                        field,
                        collections);

                    continue;
                }

                // ----------------------------------------------------
                // NORMAL FIELD
                // ----------------------------------------------------

                RenderField(
                    container,
                    field);
            }
        }

        // ============================================================
        // NORMAL FIELD
        // ============================================================

        /// <summary>
        /// Render một WorkflowFieldDefinition thông thường.
        /// </summary>
        private void RenderField(
            Control container,
            WorkflowFieldDefinition field)
        {
            Control fieldControl =
                _fieldRenderer.Render(
                    field);

            if (fieldControl == null)
            {
                return;
            }

            AddControl(
                container,
                fieldControl);
        }

        // ============================================================
        // COLLECTION
        // ============================================================

        /// <summary>
        /// Render collection field.
        ///
        /// WorkflowFieldDefinition chỉ mô tả:
        ///
        ///     Kind = Collection
        ///
        /// WorkflowCollectionDefinition mô tả:
        ///
        ///     - CollectionKey
        ///     - Label
        ///     - child Fields
        ///
        /// Hai object này không được giả định là có reference
        /// trực tiếp tới nhau.
        /// </summary>
        private void RenderCollection(
            Control container,
            WorkflowFieldDefinition field,
            IReadOnlyList<WorkflowCollectionDefinition>
                collections)
        {
            WorkflowCollectionDefinition?
                definition =
                    FindCollectionDefinition(
                        field,
                        collections);

            if (definition == null)
            {
                throw new InvalidOperationException(
                    $"Workflow View chứa Collection field " +
                    $"'{field.FieldKey}' nhưng không tìm thấy " +
                    $"WorkflowCollectionDefinition tương ứng.");
            }

            Control collectionControl =
                _collectionRenderer.Render(
                    definition);

            if (collectionControl == null)
            {
                return;
            }

            AddControl(
                container,
                collectionControl);
        }

        // ============================================================
        // FIND COLLECTION
        // ============================================================

        /// <summary>
        /// Tìm CollectionDefinition tương ứng với Collection Field.
        ///
        /// Contract hiện tại không có:
        ///
        ///     field.CollectionKey
        ///
        /// nên mapping được thực hiện bằng:
        ///
        ///     WorkflowFieldDefinition.FieldKey
        ///             ==
        ///     WorkflowCollectionDefinition.CollectionKey
        ///
        /// Đây là mapping identity duy nhất có thể xác định
        /// từ contract hiện tại mà không thêm property mới.
        /// </summary>
        private static WorkflowCollectionDefinition?
            FindCollectionDefinition(
                WorkflowFieldDefinition field,
                IReadOnlyList<WorkflowCollectionDefinition>
                    collections)
        {
            for (int i = 0;
                 i < collections.Count;
                 i++)
            {
                WorkflowCollectionDefinition collection =
                    collections[i];

                if (string.Equals(
                        collection.CollectionKey,
                        field.FieldKey,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return collection;
                }
            }

            return null;
        }

        // ============================================================
        // COLLECTION NORMALIZATION
        // ============================================================

        private static IReadOnlyList<WorkflowCollectionDefinition>
            NormalizeCollections(
                IEnumerable<WorkflowCollectionDefinition>?
                    definitions)
        {
            if (definitions == null)
            {
                return Array.Empty<
                    WorkflowCollectionDefinition>();
            }

            return definitions
                .Where(
                    item =>
                        item != null)
                .ToList();
        }

        // ============================================================
        // ADD CONTROL
        // ============================================================

        /// <summary>
        /// Add control vào container.
        ///
        /// Không phụ thuộc vào một DevExpress container cụ thể.
        /// </summary>
        private static void AddControl(
            Control container,
            Control child)
        {
            child.Dock =
                DockStyle.Top;

            container.Controls.Add(
                child);

            // --------------------------------------------------------
            // Vì DockStyle.Top xếp theo thứ tự reverse trong
            // WinForms Controls collection, đưa control mới lên đầu
            // để giữ đúng thứ tự Order.
            // --------------------------------------------------------

            child.BringToFront();
        }
    }
}

