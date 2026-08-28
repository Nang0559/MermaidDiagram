namespace MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting
{
    /// <summary>
    /// Kết quả routing của một Mermaid edge.
    ///
    /// Không phụ thuộc DevExpress.
    /// </summary>
    public sealed class EdgeRoute
    {
        // ============================================================
        // EDGE
        // ============================================================

        public string SourceKey { get; }

        public string TargetKey { get; }

        // ============================================================
        // PORT
        // ============================================================

        public EdgePort SourcePort { get; set; }

        public EdgePort TargetPort { get; set; }

        // ============================================================
        // ROUTE
        // ============================================================

        public List<EdgeRoutePoint> Points { get; }

        // ============================================================
        // LABEL
        // ============================================================

        public float LabelX { get; set; }

        public float LabelY { get; set; }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public EdgeRoute(
            string sourceKey,
            string targetKey)
        {
            SourceKey =
                sourceKey
                ?? throw new ArgumentNullException(
                    nameof(sourceKey));

            TargetKey =
                targetKey
                ?? throw new ArgumentNullException(
                    nameof(targetKey));

            Points =
                new List<EdgeRoutePoint>();
        }
    }
}