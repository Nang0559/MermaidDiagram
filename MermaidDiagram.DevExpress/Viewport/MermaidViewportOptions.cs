namespace MermaidDiagram.DevExpress.Viewport
{
    /// <summary>
    /// Các tùy chọn hiển thị cho Mermaid Diagram Viewport.
    ///
    /// Không chứa layout algorithm.
    /// Không chứa workflow configuration.
    /// </summary>
    public sealed class MermaidViewportOptions
    {
        public double ZoomStep
        {
            get;
        }

        public double MinZoom
        {
            get;
        }

        public double MaxZoom
        {
            get;
        }

        public MermaidViewportOptions(
            double zoomStep = 0.1,
            double minZoom = 0.1,
            double maxZoom = 5.0)
        {
            if (zoomStep <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(zoomStep));
            }

            if (minZoom <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minZoom));
            }

            if (maxZoom <= minZoom)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxZoom));
            }

            ZoomStep =
                zoomStep;

            MinZoom =
                minZoom;

            MaxZoom =
                maxZoom;
        }
    }
}