using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MermaidDiagram.Core.Parsing
{
    /// <summary>
    /// Một token được nhận diện từ Mermaid source.
    /// </summary>
    public sealed class MermaidToken
    {
        public MermaidTokenType Type
        {
            get;
        }

        public string Value
        {
            get;
        }

        public int Position
        {
            get;
        }

        public int Line
        {
            get;
        }

        public MermaidToken(
            MermaidTokenType type,
            string value,
            int position,
            int line)
        {
            Type =
                type;

            Value =
                value ?? string.Empty;

            Position =
                position;

            Line =
                line;
        }

        public override string ToString()
        {
            return
                $"{Type}: '{Value}' " +
                $"(Line {Line}, Pos {Position})";
        }
    }
}
