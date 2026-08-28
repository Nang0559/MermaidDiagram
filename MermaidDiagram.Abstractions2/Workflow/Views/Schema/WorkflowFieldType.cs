namespace MermaidDiagram.Abstractions.Workflow.Views.Schema
{
    /// <summary>
    /// Kiểu dữ liệu trình bày của một field trong Workflow View.
    ///
    /// Đây là abstraction cấp UI.
    /// Không phụ thuộc DevExpress, WinForms, MAUI hoặc Blazor.
    /// </summary>
    public enum WorkflowFieldType
    {
        Text = 0,

        MultilineText = 10,

        Number = 20,

        Date = 30,

        Boolean = 40,

        Select = 50,

        Collection = 60
    }
}