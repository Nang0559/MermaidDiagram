
using MermaidDiagram.Core.Graph;

namespace MermaidDiagram.Core.Layout.Contract
{
    /// <summary>
    /// Request đầu vào cho toàn bộ quá trình Mermaid Layout.
    ///
    /// Chứa:
    ///     - MermaidGraph topology
    ///     - MermaidLayoutOptions configuration
    ///     - Viewport size
    ///
    /// Không chứa:
    ///     - Layout result
    ///     - X/Y runtime state
    ///     - DevExpress
    ///     - WinForms
    /// </summary>
    public sealed class MermaidLayoutRequest
    {
        // ============================================================
        // GRAPH
        // ============================================================

        /// <summary>
        /// Graph topology đầu vào của Layout.
        ///
        /// Graph phải được build từ MermaidDocument
        /// và đã sẵn sàng cho Graph Analysis.
        /// </summary>
        public MermaidGraph Graph
        {
            get;
        }

        // ============================================================
        // OPTIONS
        // ============================================================

        /// <summary>
        /// Configuration cho Layout.
        /// </summary>
        public MermaidLayoutOptions Options
        {
            get;
        }

        // ============================================================
        // VIEWPORT
        // ============================================================

        /// <summary>
        /// Chiều rộng viewport mục tiêu.
        /// </summary>
        public float ViewportWidth
        {
            get;
        }

        /// <summary>
        /// Chiều cao viewport mục tiêu.
        /// </summary>
        public float ViewportHeight
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidLayoutRequest(
            MermaidGraph graph,
            MermaidLayoutOptions options,
            float viewportWidth,
            float viewportHeight)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(
                    nameof(graph));
            }

            if (options == null)
            {
                throw new ArgumentNullException(
                    nameof(options));
            }

            if (viewportWidth <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viewportWidth),
                    viewportWidth,
                    "ViewportWidth must be greater than zero.");
            }

            if (viewportHeight <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(viewportHeight),
                    viewportHeight,
                    "ViewportHeight must be greater than zero.");
            }

            options.Validate();

            Graph =
                graph;

            Options =
                options;

            ViewportWidth =
                viewportWidth;

            ViewportHeight =
                viewportHeight;
        }
    }
}

