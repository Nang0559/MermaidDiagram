using MermaidDiagram.Core.Layout.Contract;

namespace MermaidDiagram.Core.Layout.Spacing
{
    /// <summary>
    /// Tính toán spacing động cho layout Mermaid.
    ///
    /// Input:
    ///     - MermaidLayoutOptions
    ///     - ViewportWidth
    ///     - ViewportHeight
    ///
    /// Output:
    ///     - AdaptiveSpacingResult
    ///
    /// Class này chỉ tính toán spacing.
    ///
    /// Không chịu trách nhiệm:
    ///     - đặt X/Y
    ///     - layout Main Path
    ///     - layout Branch
    ///     - layout Group
    ///     - collision
    ///     - bounds
    ///     - viewport fitting
    ///     - DevExpress
    /// </summary>
    public sealed class AdaptiveSpacingCalculator
    {
        // ============================================================
        // CONSTANTS
        // ============================================================

        private const float ReferenceViewportWidth = 1200f;

        private const float ReferenceViewportHeight = 700f;

        private const float GeneralMinimumFactor = 0.70f;

        private const float GeneralMaximumFactor = 1.30f;

        private const float BranchMinimumFactor = 0.75f;

        private const float BranchMaximumFactor = 1.25f;

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Tính spacing thực tế cho layout.
        ///
        /// UseAdaptiveSpacing = false:
        ///     sử dụng trực tiếp giá trị từ Options.
        ///
        /// UseAdaptiveSpacing = true:
        ///     điều chỉnh spacing theo viewport.
        /// </summary>
        public AdaptiveSpacingResult Calculate(
            MermaidLayoutOptions options,
            float viewportWidth,
            float viewportHeight)
        {
            if (options == null)
            {
                throw new ArgumentNullException(
                    nameof(options));
            }

            options.Validate();

            if (viewportWidth < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viewportWidth),
                    viewportWidth,
                    "Viewport width cannot be negative.");
            }

            if (viewportHeight < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viewportHeight),
                    viewportHeight,
                    "Viewport height cannot be negative.");
            }

            // --------------------------------------------------------
            // DEFAULT
            // --------------------------------------------------------

            float horizontalSpacing =
                options.HorizontalSpacing;

            float verticalSpacing =
                options.VerticalSpacing;

            float branchSpacing =
                options.BranchSpacing;

            float branchVerticalSpacing =
                options.BranchVerticalSpacing;

            float rootSpacing =
                options.RootSpacing;

            // --------------------------------------------------------
            // ADAPTIVE
            // --------------------------------------------------------

            if (options.UseAdaptiveSpacing)
            {
                horizontalSpacing =
                    CalculateHorizontalSpacing(
                        options,
                        viewportWidth);

                verticalSpacing =
                    CalculateVerticalSpacing(
                        options,
                        viewportHeight);

                branchSpacing =
                    CalculateBranchSpacing(
                        options,
                        viewportWidth);

                branchVerticalSpacing =
                    CalculateBranchVerticalSpacing(
                        options,
                        viewportHeight);

                rootSpacing =
                    CalculateRootSpacing(
                        options,
                        viewportWidth);
            }

            // --------------------------------------------------------
            // RESULT
            // --------------------------------------------------------

            return new AdaptiveSpacingResult
            {
                HorizontalSpacing =
                    horizontalSpacing,

                VerticalSpacing =
                    verticalSpacing,

                BranchSpacing =
                    branchSpacing,

                BranchVerticalSpacing =
                    branchVerticalSpacing,

                RootSpacing =
                    rootSpacing
            };
        }

        // ============================================================
        // MAIN PATH - HORIZONTAL
        // ============================================================

        private static float CalculateHorizontalSpacing(
            MermaidLayoutOptions options,
            float viewportWidth)
        {
            if (viewportWidth <= 0f)
            {
                return options.HorizontalSpacing;
            }

            float factor =
                viewportWidth /
                ReferenceViewportWidth;

            factor =
                Clamp(
                    factor,
                    GeneralMinimumFactor,
                    GeneralMaximumFactor);

            return
                options.HorizontalSpacing *
                factor;
        }

        // ============================================================
        // MAIN PATH - VERTICAL
        // ============================================================

        private static float CalculateVerticalSpacing(
            MermaidLayoutOptions options,
            float viewportHeight)
        {
            if (viewportHeight <= 0f)
            {
                return options.VerticalSpacing;
            }

            float factor =
                viewportHeight /
                ReferenceViewportHeight;

            factor =
                Clamp(
                    factor,
                    GeneralMinimumFactor,
                    GeneralMaximumFactor);

            return
                options.VerticalSpacing *
                factor;
        }

        // ============================================================
        // BRANCH - HORIZONTAL
        // ============================================================

        private static float CalculateBranchSpacing(
            MermaidLayoutOptions options,
            float viewportWidth)
        {
            if (viewportWidth <= 0f)
            {
                return options.BranchSpacing;
            }

            float factor =
                viewportWidth /
                ReferenceViewportWidth;

            factor =
                Clamp(
                    factor,
                    BranchMinimumFactor,
                    BranchMaximumFactor);

            return
                options.BranchSpacing *
                factor;
        }

        // ============================================================
        // BRANCH - VERTICAL
        // ============================================================

        private static float CalculateBranchVerticalSpacing(
            MermaidLayoutOptions options,
            float viewportHeight)
        {
            if (viewportHeight <= 0f)
            {
                return options.BranchVerticalSpacing;
            }

            float factor =
                viewportHeight /
                ReferenceViewportHeight;

            factor =
                Clamp(
                    factor,
                    BranchMinimumFactor,
                    BranchMaximumFactor);

            return
                options.BranchVerticalSpacing *
                factor;
        }

        // ============================================================
        // ROOT
        // ============================================================

        private static float CalculateRootSpacing(
            MermaidLayoutOptions options,
            float viewportWidth)
        {
            if (viewportWidth <= 0f)
            {
                return options.RootSpacing;
            }

            float factor =
                viewportWidth /
                ReferenceViewportWidth;

            factor =
                Clamp(
                    factor,
                    BranchMinimumFactor,
                    BranchMaximumFactor);

            return
                options.RootSpacing *
                factor;
        }

        // ============================================================
        // CLAMP
        // ============================================================

        /// <summary>
        /// Clamp thủ công để tương thích các target framework
        /// không hỗ trợ Math.Clamp.
        /// </summary>
        private static float Clamp(
            float value,
            float minimum,
            float maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            if (value > maximum)
            {
                return maximum;
            }

            return value;
        }
    }
}