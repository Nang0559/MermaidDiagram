namespace MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting
{
    /// <summary>
    /// Điểm xuất / nhập của connector trên một layout item.
    ///
    /// Core chỉ biết vị trí logic của connector.
    /// DevExpress renderer sẽ chuyển EdgePort
    /// thành điểm thực tế trên DiagramShape.
    /// </summary>
    public enum EdgePort
    {
        Top = 0,
        Right = 1,
        Bottom = 2,
        Left = 3
    }
}