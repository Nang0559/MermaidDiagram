using System;
using System.Collections.Generic;

namespace MermaidDiagram.Abstractions.Workflow.Views.Validation
{
    /// <summary>
    /// Định nghĩa validation của một Workflow View.
    /// </summary>
    public sealed class WorkflowValidationDefinition
    {
        private readonly Dictionary<
            string,
            IReadOnlyList<WorkflowValidationRule>>
            _rules =
                new Dictionary<
                    string,
                    IReadOnlyList<WorkflowValidationRule>>(
                        StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<
            string,
            IReadOnlyList<WorkflowValidationRule>>
            Rules
        {
            get
            {
                return _rules;
            }
        }

        public void SetRules(
            string fieldKey,
            IEnumerable<WorkflowValidationRule> rules)
        {
            if (string.IsNullOrWhiteSpace(fieldKey))
            {
                throw new ArgumentException(
                    "FieldKey không được rỗng.",
                    nameof(fieldKey));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(
                    nameof(rules));
            }

            List<WorkflowValidationRule> copy =
                new List<WorkflowValidationRule>(
                    rules);

            _rules[fieldKey] =
                copy.AsReadOnly();
        }
    }
}