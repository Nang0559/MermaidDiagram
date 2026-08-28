
namespace MermaidDiagram.Core.Layout.Contract
{
    public static class MermaidLayoutDirectionExtensions
    {
        public static bool IsHorizontal(
            this MermaidLayoutDirection direction)
        {
            return direction == MermaidLayoutDirection.LeftToRight
                || direction == MermaidLayoutDirection.RightToLeft;
        }

        public static bool IsVertical(
            this MermaidLayoutDirection direction)
        {
            return direction == MermaidLayoutDirection.TopToBottom
                || direction == MermaidLayoutDirection.BottomToTop;
        }

        public static bool IsReversed(
            this MermaidLayoutDirection direction)
        {
            return direction == MermaidLayoutDirection.RightToLeft
                || direction == MermaidLayoutDirection.BottomToTop;
        }
    }
}
