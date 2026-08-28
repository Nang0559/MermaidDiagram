namespace MermaidDiagram.Abstractions.Workflow.Views
{
    /// <summary>
    /// Mô tả một yêu cầu điều khiển viewport.
    /// Không phụ thuộc UI framework.
    /// </summary>
    public sealed class MermaidViewportRequest
    {
        public MermaidViewportAction Action
        {
            get;
        }

        /// <summary>
        /// Node cần focus.
        /// Chỉ có giá trị đối với các action liên quan đến node.
        /// </summary>
        public string? NodeKey
        {
            get;
        }

        /// <summary>
        /// Mức zoom mong muốn.
        /// </summary>
        public double? Zoom
        {
            get;
        }

        public MermaidViewportRequest(
            MermaidViewportAction action,
            string? nodeKey = null,
            double? zoom = null)
        {
            Action =
                action;

            NodeKey =
                string.IsNullOrWhiteSpace(nodeKey)
                    ? null
                    : nodeKey;

            Zoom =
                zoom;
        }
    }
}