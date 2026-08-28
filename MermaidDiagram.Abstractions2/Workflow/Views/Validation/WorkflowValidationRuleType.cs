namespace MermaidDiagram.Abstractions.Workflow.Views.Validation
{
    /// <summary>
    /// Loại validation rule của Workflow View.
    /// </summary>
    public enum WorkflowValidationRuleType
    {
        Required = 0,

        MinLength = 10,

        MaxLength = 20,

        MinValue = 30,

        MaxValue = 40,

        Positive = 50
    }
}