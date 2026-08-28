

using DevExpress.XtraEditors;

using MermaidDiagram.Abstractions.Workflow.Views.Schema;

namespace MermaidDiagram.DevExpress.ViewRendering
{
    /// <summary>
    /// Render WorkflowCollectionDefinition thành DevExpress controls.
    ///
    /// Trách nhiệm:
    ///     - tạo container cho collection;
    ///     - hiển thị Label của collection;
    ///     - lưu CollectionKey vào Tag;
    ///     - chuẩn bị vùng chứa các field của collection.
    ///
    /// Không chịu trách nhiệm:
    ///     - đọc collection data runtime;
    ///     - tạo row;
    ///     - binding data;
    ///     - render field;
    ///     - validation;
    ///     - workflow dispatch;
    ///     - navigation;
    ///     - business logic.
    ///
    /// Field thuộc collection được render bởi
    /// DevExpressFieldRenderer.
    /// </summary>
    public sealed class DevExpressCollectionRenderer
    {
        // ============================================================
        // FACTORY
        // ============================================================

        private readonly DevExpressControlFactory _controlFactory;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public DevExpressCollectionRenderer(
            DevExpressControlFactory controlFactory)
        {
            _controlFactory =
                controlFactory
                ?? throw new ArgumentNullException(
                    nameof(controlFactory));
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Render schema của một WorkflowCollection.
        ///
        /// Kết quả là PanelControl chứa:
        ///
        ///     Collection Label
        ///          ↓
        ///     Fields container
        ///
        /// Chưa có runtime rows.
        /// </summary>
        public PanelControl Render(
            WorkflowCollectionDefinition collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(
                    nameof(collection));
            }

            // --------------------------------------------------------
            // ROOT PANEL
            // --------------------------------------------------------

            PanelControl panel =
                _controlFactory.CreateCollectionPanel();

            // --------------------------------------------------------
            // IDENTITY
            // --------------------------------------------------------

            panel.Tag =
                collection.CollectionKey;

            // --------------------------------------------------------
            // HEADER
            // --------------------------------------------------------

            LabelControl label =
                _controlFactory.CreateLabel(
                    collection.Label);

            panel.Controls.Add(
                label);

            // --------------------------------------------------------
            // FIELD CONTAINER
            // --------------------------------------------------------

            PanelControl fieldPanel =
                _controlFactory.CreateFieldPanel();

            panel.Controls.Add(
                fieldPanel);

            return panel;
        }

        // ============================================================
        // ADD FIELD HOST
        // ============================================================

        /// <summary>
        /// Tạo vùng chứa cho các field của collection.
        ///
        /// Method này chỉ tạo container.
        /// Việc render WorkflowFieldDefinition thuộc
        /// DevExpressFieldRenderer.
        /// </summary>
        public PanelControl CreateFieldContainer()
        {
            return _controlFactory.CreateFieldPanel();
        }

        // ============================================================
        // ADD FIELD CONTROL
        // ============================================================

        /// <summary>
        /// Thêm control field đã được render vào collection.
        ///
        /// Renderer không biết field là TextEdit,
        /// MemoEdit, ComboBoxEdit hay control khác.
        /// </summary>
        public void AddField(
            PanelControl fieldContainer,
            Control fieldControl)
        {
            if (fieldContainer == null)
            {
                throw new ArgumentNullException(
                    nameof(fieldContainer));
            }

            if (fieldControl == null)
            {
                throw new ArgumentNullException(
                    nameof(fieldControl));
            }

            fieldContainer.Controls.Add(
                fieldControl);
        }
    }
}
