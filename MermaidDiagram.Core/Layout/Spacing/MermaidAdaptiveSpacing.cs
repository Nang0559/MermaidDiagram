using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MermaidDiagram.Core.Layout
{
    /// <summary>
    /// Kết quả spacing được tính toán động cho layout.
    /// </summary>
    public sealed class MermaidAdaptiveSpacing
    {
        public float HorizontalSpacing { get; }

        public float VerticalSpacing { get; }

        public float BranchSpacing { get; }

        public float BranchLayerSpacing { get; }

        public float RootSpacing { get; }

        public MermaidAdaptiveSpacing(
            float horizontalSpacing,
            float verticalSpacing,
            float branchSpacing,
            float branchLayerSpacing,
            float rootSpacing)
        {
            HorizontalSpacing =
                horizontalSpacing;

            VerticalSpacing =
                verticalSpacing;

            BranchSpacing =
                branchSpacing;

            BranchLayerSpacing =
                branchLayerSpacing;

            RootSpacing =
                rootSpacing;
        }
    }
}
