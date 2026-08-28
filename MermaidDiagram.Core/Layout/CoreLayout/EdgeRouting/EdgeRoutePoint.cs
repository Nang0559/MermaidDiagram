namespace MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting
{
    /// <summary>
    /// Một điểm trên routing path của connector.
    /// </summary>
    public readonly struct EdgeRoutePoint
    {
        public float X { get; }

        public float Y { get; }

        public EdgeRoutePoint(
            float x,
            float y)
        {
            X = x;
            Y = y;
        }
    }
}