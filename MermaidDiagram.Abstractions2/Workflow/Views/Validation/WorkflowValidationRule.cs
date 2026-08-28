namespace MermaidDiagram.Abstractions.Workflow.Views.Validation
{
    /// <summary>
    /// Một validation rule áp dụng cho field.
    ///
    /// Chỉ mô tả rule.
    /// Không thực hiện validation và không hiển thị UI.
    /// </summary>
    public sealed class WorkflowValidationRule
    {
        public WorkflowValidationRuleType RuleType
        {
            get;
        }

        public object? Value
        {
            get;
        }

        public string? Message
        {
            get;
        }

        public WorkflowValidationRule(
            WorkflowValidationRuleType ruleType,
            object? value = null,
            string? message = null)
        {
            RuleType =
                ruleType;

            Value =
                value;

            Message =
                message;
        }
    }
}