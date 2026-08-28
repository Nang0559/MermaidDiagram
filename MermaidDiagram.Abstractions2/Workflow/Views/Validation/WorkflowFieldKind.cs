
namespace MermaidDiagram.Abstractions.Workflow.Views
{
    /// <summary>
    /// Loại Field mà Generic View Renderer có thể render.
    ///
    /// Đây là contract presentation-neutral.
    /// Adapter cụ thể sẽ quyết định:
    ///
    ///     Text         -> TextBox / Entry / Input
    ///     MultilineText-> MemoEdit / Editor / TextArea
    ///     Number       -> SpinEdit / NumericInput
    ///     Collection   -> Grid / CollectionView / Table
    ///     Select       -> ComboBox / Select
    /// </summary>
    public enum WorkflowFieldKind
    {
        Text = 0,

        MultilineText = 1,

        Number = 2,

        Decimal = 3,

        Date = 4,

        DateTime = 5,

        Boolean = 6,

        Select = 7,

        Collection = 8,

        Hidden = 9
    }
}

