using System;
using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Workflow.Views.Schema
{
    /// <summary>
    /// Định nghĩa một collection field trong Workflow View.
    ///
    /// Ví dụ:
    ///
    /// ChiTiet
    ///     ├── LotNo
    ///     ├── SanPham
    ///     ├── SoLuong
    ///     └── Slot
    /// </summary>
    public sealed class WorkflowCollectionDefinition
    {
        private readonly List<WorkflowFieldDefinition>
            _fields =
                new List<WorkflowFieldDefinition>();

        // ============================================================
        // IDENTITY
        // ============================================================

        public string CollectionKey
        {
            get;
        }

        public string Label
        {
            get;
        }

        // ============================================================
        // FIELDS
        // ============================================================

        public IReadOnlyList<WorkflowFieldDefinition>
            Fields
        {
            get
            {
                return _fields;
            }
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public WorkflowCollectionDefinition(
            string collectionKey,
            string label)
        {
            if (string.IsNullOrWhiteSpace(collectionKey))
            {
                throw new ArgumentException(
                    "CollectionKey không được rỗng.",
                    nameof(collectionKey));
            }

            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException(
                    "Label không được rỗng.",
                    nameof(label));
            }

            CollectionKey =
                collectionKey.Trim();

            Label =
                label.Trim();
        }

        // ============================================================
        // ADD FIELD
        // ============================================================

        public void AddField(
            WorkflowFieldDefinition field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(
                    nameof(field));
            }

            for (int i = 0;
                 i < _fields.Count;
                 i++)
            {
                if (string.Equals(
                        _fields[i].FieldKey,
                        field.FieldKey,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Field '{field.FieldKey}' " +
                        $"đã tồn tại trong collection " +
                        $"'{CollectionKey}'.");
                }
            }

            _fields.Add(
                field);
        }
    }
}