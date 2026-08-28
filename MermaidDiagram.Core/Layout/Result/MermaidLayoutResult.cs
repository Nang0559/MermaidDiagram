using MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting;

namespace MermaidDiagram.Core.Layout.Result
{
    /// <summary>
    /// Kết quả cuối cùng sau khi Mermaid graph được layout.
    ///
    /// MermaidLayoutResult không phụ thuộc UI hoặc DevExpress.
    ///
    /// Đây là contract giữa:
    ///
    ///     MermaidDiagram.Core
    ///             ↓
    ///     MermaidDiagram.DevExpress
    ///
    /// Items chứa tọa độ và kích thước cuối cùng
    /// sau khi toàn bộ layout + collision + viewport fitting
    /// đã hoàn tất.
    /// </summary>
    public sealed class MermaidLayoutResult
    {
        // ============================================================
        // ITEMS
        // ============================================================

        /// <summary>
        /// Toàn bộ node/group sau khi hoàn tất layout.
        ///
        /// Key:
        ///     NodeId hoặc GroupId.
        ///
        /// X/Y:
        ///     Tọa độ cuối cùng của item.
        ///
        /// Width/Height:
        ///     Kích thước cuối cùng của item.
        /// </summary>
        public IReadOnlyDictionary<
            string,
            MermaidLayoutItem> Items
        {
            get;
        }

        // ============================================================
        // EDGE ROUTES
        // ============================================================

        /// <summary>
        /// Routing result của toàn bộ edge sau layout.
        ///
        /// Key:
        ///     SourceId + "->" + TargetId.
        ///
        /// EdgeRoute chứa:
        ///     - SourcePort
        ///     - TargetPort
        ///     - Points
        ///     - LabelX
        ///     - LabelY
        ///
        /// EdgeRoutes được tạo bởi:
        ///
        ///     RootLayoutPlanner
        ///          ↓
        ///     EdgeRoutingEngine
        ///          ↓
        ///     RootLayoutContext.EdgeRoutes
        ///          ↓
        ///     MermaidLayoutResult.EdgeRoutes
        ///
        /// Renderer chỉ đọc kết quả này.
        /// </summary>
        public IReadOnlyDictionary<
            string,
            EdgeRoute> EdgeRoutes
        {
            get;
        }

        // ============================================================
        // GRAPH SIZE
        // ============================================================

        /// <summary>
        /// Chiều rộng graph cuối cùng.
        /// </summary>
        public float GraphWidth
        {
            get;
        }

        /// <summary>
        /// Chiều cao graph cuối cùng.
        /// </summary>
        public float GraphHeight
        {
            get;
        }

        // ============================================================
        // SCALE
        // ============================================================

        /// <summary>
        /// Scale cuối cùng sau viewport fitting.
        /// </summary>
        public float Scale
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidLayoutResult(
            IReadOnlyDictionary<
                string,
                MermaidLayoutItem> items,
            float graphWidth,
            float graphHeight,
            float scale,
            IReadOnlyDictionary<
                string,
                EdgeRoute> edgeRoutes)
        {
            Items =
                items
                ?? throw new ArgumentNullException(
                    nameof(items));

            EdgeRoutes =
                edgeRoutes
                ?? throw new ArgumentNullException(
                    nameof(edgeRoutes));

            GraphWidth =
                graphWidth;

            GraphHeight =
                graphHeight;

            Scale =
                scale;
        }
    }
}