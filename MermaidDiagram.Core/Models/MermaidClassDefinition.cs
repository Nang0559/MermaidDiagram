using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MermaidDiagram.Core.Models
{
    public sealed class MermaidClassDefinition
    {
        public string Name { get; }

        public string Style { get; }

        public MermaidClassDefinition(
            string name,
            string style)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Class name không được rỗng.",
                    nameof(name));
            }

            Name = name;
            Style = style ?? string.Empty;
        }
    }
}
