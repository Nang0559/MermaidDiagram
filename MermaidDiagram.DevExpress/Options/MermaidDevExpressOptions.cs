namespace MermaidDiagram.DevExpress.Options
{
    public sealed class MermaidDevExpressOptions
    {
        /// <summary>
        /// Có tự động xóa diagram cũ trước khi render hay không.
        /// </summary>
        public bool ClearBeforeRender { get; set; } = true;

        /// <summary>
        /// Có tự động zoom/focus toàn bộ diagram sau render hay không.
        /// </summary>
        public bool AutoFit { get; set; } = true;

        /// <summary>
        /// Cho phép chọn node.
        /// </summary>
        public bool AllowNodeSelection { get; set; } = true;

        /// <summary>
        /// Cho phép chọn edge.
        /// </summary>
        public bool AllowEdgeSelection { get; set; } = true;
    }
}