using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MermaidDiagram.Core.Models
{
    public sealed class MermaidSubgraphStyle
    {
        public string Fill
        {
            get;
            set;
        } = string.Empty;

        public string Stroke
        {
            get;
            set;
        } = string.Empty;

        public string TextColor
        {
            get;
            set;
        } = string.Empty;

        public double? StrokeWidth
        {
            get;
            set;
        }

        public string CssClass
        {
            get;
            set;
        } = string.Empty;
    }
}
