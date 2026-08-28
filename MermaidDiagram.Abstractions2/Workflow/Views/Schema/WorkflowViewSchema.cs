using MermaidDiagram.Abstractions.Workflow.Views.Schema;
using System;
using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Workflow.Views
{
    /// <summary>
    /// Schema mô tả cấu trúc của một Workflow View.
    ///
    /// WorkflowViewSchema chỉ mô tả:
    ///     - View identity
    ///     - Fields
    ///     - Collections
    ///     - Validation
    ///     - Presentation
    ///
    /// Không chứa:
    ///     - Business logic
    ///     - WorkflowActionDefinition
    ///     - UI framework cụ thể
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    ///
    /// Action của View được khai báo riêng trong
    /// WorkflowActionDefinition / Workflow Node Definition.
    /// </summary>
    public sealed class WorkflowViewSchema
    {
        private readonly List<WorkflowFieldDefinition> _fields =
            new List<WorkflowFieldDefinition>();

        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// Key định danh của View Schema.
        ///
        /// Thông thường trùng với WorkflowViewDefinition.ViewKey.
        /// </summary>
        public string ViewKey
        {
            get;
        }

        /// <summary>
        /// Tên hiển thị của View.
        /// </summary>
        public string ViewTitle
        {
            get;
        }

        // ============================================================
        // FIELDS
        // ============================================================

        /// <summary>
        /// Các field hiển thị trên View.
        ///
        /// Bao gồm:
        ///     - Text
        ///     - MultilineText
        ///     - Number
        ///     - Date
        ///     - Selection
        ///     - Collection
        ///     ...
        /// </summary>
        public IReadOnlyList<WorkflowFieldDefinition> Fields
        {
            get
            {
                return _fields;
            }
        }

        // ============================================================
        // PRESENTATION
        // ============================================================

        /// <summary>
        /// Chế độ hiển thị View.
        ///
        /// Renderer quyết định cách ánh xạ
        /// schema này sang Form / Panel / Dialog / Component.
        /// </summary>
        public WorkflowViewMode ViewMode
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public WorkflowViewSchema(
            string viewKey,
            string viewTitle,
            WorkflowViewMode viewMode =
                WorkflowViewMode.Default)
        {
            if (string.IsNullOrWhiteSpace(viewKey))
            {
                throw new ArgumentException(
                    "ViewKey không được rỗng.",
                    nameof(viewKey));
            }

            if (string.IsNullOrWhiteSpace(viewTitle))
            {
                throw new ArgumentException(
                    "ViewTitle không được rỗng.",
                    nameof(viewTitle));
            }

            ViewKey =
                viewKey.Trim();

            ViewTitle =
                viewTitle.Trim();

            ViewMode =
                viewMode;
        }

        // ============================================================
        // FIELD
        // ============================================================

        /// <summary>
        /// Thêm một field vào View Schema.
        /// </summary>
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
                        $"đã tồn tại trong View '{ViewKey}'.");
                }
            }

            _fields.Add(
                field);
        }

        // ============================================================
        // REMOVE FIELD
        // ============================================================

        /// <summary>
        /// Xóa field theo FieldKey.
        /// </summary>
        public bool RemoveField(
            string fieldKey)
        {
            if (string.IsNullOrWhiteSpace(fieldKey))
            {
                return false;
            }

            for (int i = 0;
                 i < _fields.Count;
                 i++)
            {
                if (string.Equals(
                        _fields[i].FieldKey,
                        fieldKey,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _fields.RemoveAt(i);

                    return true;
                }
            }

            return false;
        }

        // ============================================================
        // FIND FIELD
        // ============================================================

        /// <summary>
        /// Tìm field theo FieldKey.
        /// </summary>
        public WorkflowFieldDefinition? FindField(
            string fieldKey)
        {
            if (string.IsNullOrWhiteSpace(fieldKey))
            {
                return null;
            }

            for (int i = 0;
                 i < _fields.Count;
                 i++)
            {
                WorkflowFieldDefinition field =
                    _fields[i];

                if (string.Equals(
                        field.FieldKey,
                        fieldKey,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return field;
                }
            }

            return null;
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Validate tính hợp lệ nội tại của View Schema.
        ///
        /// Không validate business rule.
        /// Không validate Workflow transition.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ViewKey))
            {
                throw new InvalidOperationException(
                    "WorkflowViewSchema không có ViewKey.");
            }

            if (string.IsNullOrWhiteSpace(ViewTitle))
            {
                throw new InvalidOperationException(
                    $"WorkflowViewSchema '{ViewKey}' " +
                    "không có ViewTitle.");
            }

            HashSet<string> fieldKeys =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0;
                 i < _fields.Count;
                 i++)
            {
                WorkflowFieldDefinition field =
                    _fields[i];

                if (field == null)
                {
                    throw new InvalidOperationException(
                        $"View '{ViewKey}' chứa Field null.");
                }

                if (string.IsNullOrWhiteSpace(
                        field.FieldKey))
                {
                    throw new InvalidOperationException(
                        $"View '{ViewKey}' chứa Field không có FieldKey.");
                }

                if (!fieldKeys.Add(
                        field.FieldKey))
                {
                    throw new InvalidOperationException(
                        $"View '{ViewKey}' có FieldKey " +
                        $"trùng '{field.FieldKey}'.");
                }

                field.Validate();
            }
        }

        // ============================================================
        // DISPLAY
        // ============================================================

        public override string ToString()
        {
            return ViewKey;
        }
    }
}