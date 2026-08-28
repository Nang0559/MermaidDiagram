using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MermaidDiagram.Core.Models
{
    public sealed class MermaidClassApplication
    {
        public string NodeId { get; }

        public string ClassName { get; }

        public MermaidClassApplication(
            string nodeId,
            string className)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                throw new ArgumentException(
                    "NodeId không được rỗng.",
                    nameof(nodeId));
            }

            if (string.IsNullOrWhiteSpace(className))
            {
                throw new ArgumentException(
                    "ClassName không được rỗng.",
                    nameof(className));
            }

            NodeId = nodeId;
            ClassName = className;
        }
    }
}
