using MermaidDiagram.Core.Layout.Contract;
using MermaidDiagram.Core.Layout.Result;

namespace MermaidDiagram.Core.Layout.CoreLayout
{
    /// <summary>
    /// Layout các item thuộc Main Path.
    ///
    /// MainPathLayout KHÔNG xác định Main Path.
    /// Main Path đã được tính bởi MainPathCalculator
    /// và lưu trong GraphAnalysisResult.
    ///
    /// Class này chỉ chịu trách nhiệm:
    ///
    ///     1. Đặt các item theo thứ tự Main Path.
    ///     2. Hỗ trợ 4 hướng layout.
    ///     3. Đánh dấu item thuộc Main Path.
    ///     4. Cập nhật MainPathIndex runtime.
    ///     5. Tính kích thước Main Path.
    ///
    /// Không chịu trách nhiệm:
    ///
    ///     - phân tích graph
    ///     - xác định Main Path
    ///     - layout branch
    ///     - layout group
    ///     - collision
    ///     - bounds
    ///     - viewport
    ///     - scale
    ///     - DevExpress
    /// </summary>
    public sealed class MainPathLayout
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Layout toàn bộ Main Path.
        /// </summary>
        public void Apply(
            RootLayoutContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (context.MainPath == null ||
                context.MainPath.Count == 0)
            {
                return;
            }

            ResetMainPathState(
                context);

            switch (context.Direction)
            {
                case MermaidLayoutDirection.LeftToRight:

                    LayoutLeftToRight(
                        context);

                    break;

                case MermaidLayoutDirection.RightToLeft:

                    LayoutRightToLeft(
                        context);

                    break;

                case MermaidLayoutDirection.TopToBottom:

                    LayoutTopToBottom(
                        context);

                    break;

                case MermaidLayoutDirection.BottomToTop:

                    LayoutBottomToTop(
                        context);

                    break;

                default:

                    throw new InvalidOperationException(
                        $"Unsupported layout direction: " +
                        $"{context.Direction}");
            }

            UpdateMainPathIndex(
                context);
        }

        // ============================================================
        // TOP -> BOTTOM
        // ============================================================

        private static void LayoutTopToBottom(
            RootLayoutContext context)
        {
            float currentY = 0f;

            float centerX =
                CalculateMainPathAxisCenterX(
                    context);

            for (int i = 0;
                 i < context.MainPath.Count;
                 i++)
            {
                MermaidLayoutItem item =
                    GetItem(
                        context,
                        context.MainPath[i]);

                item.X =
                    centerX -
                    item.Width / 2f;

                item.Y =
                    currentY;

                item.IsPositioned =
                    true;

                currentY +=
                    item.Height;

                if (i <
                    context.MainPath.Count - 1)
                {
                    currentY +=
                        GetVerticalSpacing(
                            context);
                }
            }
        }

        // ============================================================
        // BOTTOM -> TOP
        // ============================================================

        private static void LayoutBottomToTop(
            RootLayoutContext context)
        {
            float currentY = 0f;

            float centerX =
                CalculateMainPathAxisCenterX(
                    context);

            for (int i =
                     context.MainPath.Count - 1;
                 i >= 0;
                 i--)
            {
                MermaidLayoutItem item =
                    GetItem(
                        context,
                        context.MainPath[i]);

                item.X =
                    centerX -
                    item.Width / 2f;

                item.Y =
                    currentY;

                item.IsPositioned =
                    true;

                currentY +=
                    item.Height;

                if (i > 0)
                {
                    currentY +=
                        GetVerticalSpacing(
                            context);
                }
            }
        }

        // ============================================================
        // LEFT -> RIGHT
        // ============================================================

        private static void LayoutLeftToRight(
            RootLayoutContext context)
        {
            float currentX = 0f;

            float centerY =
                CalculateMainPathAxisCenterY(
                    context);

            for (int i = 0;
                 i < context.MainPath.Count;
                 i++)
            {
                MermaidLayoutItem item =
                    GetItem(
                        context,
                        context.MainPath[i]);

                item.X =
                    currentX;

                item.Y =
                    centerY -
                    item.Height / 2f;

                item.IsPositioned =
                    true;

                currentX +=
                    item.Width;

                if (i <
                    context.MainPath.Count - 1)
                {
                    currentX +=
                        GetHorizontalSpacing(
                            context);
                }
            }
        }

        // ============================================================
        // RIGHT -> LEFT
        // ============================================================

        private static void LayoutRightToLeft(
            RootLayoutContext context)
        {
            float currentX = 0f;

            float centerY =
                CalculateMainPathAxisCenterY(
                    context);

            for (int i =
                     context.MainPath.Count - 1;
                 i >= 0;
                 i--)
            {
                MermaidLayoutItem item =
                    GetItem(
                        context,
                        context.MainPath[i]);

                item.X =
                    currentX;

                item.Y =
                    centerY -
                    item.Height / 2f;

                item.IsPositioned =
                    true;

                currentX +=
                    item.Width;

                if (i > 0)
                {
                    currentX +=
                        GetHorizontalSpacing(
                            context);
                }
            }
        }

        // ============================================================
        // MAIN PATH AXIS CENTER
        // ============================================================

        /// <summary>
        /// Tính trục X local dùng để căn giữa Main Path
        /// theo hướng TopToBottom / BottomToTop.
        ///
        /// Đây không phải viewport center.
        /// </summary>
        private static float CalculateMainPathAxisCenterX(
            RootLayoutContext context)
        {
            float maxWidth = 0f;

            foreach (string key
                     in context.MainPath)
            {
                MermaidLayoutItem item =
                    GetItem(
                        context,
                        key);

                if (item.Width > maxWidth)
                {
                    maxWidth =
                        item.Width;
                }
            }

            return maxWidth / 2f;
        }

        /// <summary>
        /// Tính trục Y local dùng để căn giữa Main Path
        /// theo hướng LeftToRight / RightToLeft.
        ///
        /// Đây không phải viewport center.
        /// </summary>
        private static float CalculateMainPathAxisCenterY(
            RootLayoutContext context)
        {
            float maxHeight = 0f;

            foreach (string key
                     in context.MainPath)
            {
                MermaidLayoutItem item =
                    GetItem(
                        context,
                        key);

                if (item.Height > maxHeight)
                {
                    maxHeight =
                        item.Height;
                }
            }

            return maxHeight / 2f;
        }

        // ============================================================
        // ITEM
        // ============================================================

        private static MermaidLayoutItem GetItem(
            RootLayoutContext context,
            string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException(
                    "Main Path contains an empty node key.");
            }

            if (!context.Items.TryGetValue(
                    key,
                    out MermaidLayoutItem? item))
            {
                throw new InvalidOperationException(
                    $"Layout item '{key}' " +
                    $"was not found.");
            }

            return item;
        }

        // ============================================================
        // MAIN PATH STATE
        // ============================================================

        private static void ResetMainPathState(
            RootLayoutContext context)
        {
            foreach (KeyValuePair<
                         string,
                         MermaidLayoutItem> pair
                     in context.Items)
            {
                pair.Value.IsMainPath =
                    false;

                pair.Value.MainPathIndex =
                    -1;
            }
        }

        // ============================================================
        // MAIN PATH INDEX
        // ============================================================

        private static void UpdateMainPathIndex(
            RootLayoutContext context)
        {
            for (int i = 0;
                 i < context.MainPath.Count;
                 i++)
            {
                string key =
                    context.MainPath[i];

                context.MainPathIndex[key] =
                    i;

                MermaidLayoutItem item =
                    GetItem(
                        context,
                        key);

                item.IsMainPath =
                    true;

                item.MainPathIndex =
                    i;
            }
        }

        // ============================================================
        // SPACING
        // ============================================================

        /// <summary>
        /// Lấy horizontal spacing đã được tính toán.
        ///
        /// Không đọc trực tiếp Options vì Options chỉ là
        /// configuration ban đầu.
        /// </summary>
        private static float GetHorizontalSpacing(
            RootLayoutContext context)
        {
            return Math.Max(
                0f,
                context.HorizontalSpacing);
        }

        /// <summary>
        /// Lấy vertical spacing đã được tính toán.
        /// </summary>
        private static float GetVerticalSpacing(
            RootLayoutContext context)
        {
            return Math.Max(
                0f,
                context.VerticalSpacing);
        }

        // ============================================================
        // SIZE
        // ============================================================

        /// <summary>
        /// Tính chiều rộng Main Path.
        ///
        /// Không thay đổi vị trí item.
        /// </summary>
        public float CalculateWidth(
            RootLayoutContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (context.MainPath == null ||
                context.MainPath.Count == 0)
            {
                return 0f;
            }

            float width = 0f;

            foreach (string key
                     in context.MainPath)
            {
                MermaidLayoutItem item =
                    GetItem(
                        context,
                        key);

                width +=
                    item.Width;
            }

            width +=
                GetHorizontalSpacing(
                    context) *
                Math.Max(
                    0,
                    context.MainPath.Count - 1);

            return width;
        }

        /// <summary>
        /// Tính chiều cao Main Path.
        ///
        /// Không thay đổi vị trí item.
        /// </summary>
        public float CalculateHeight(
            RootLayoutContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (context.MainPath == null ||
                context.MainPath.Count == 0)
            {
                return 0f;
            }

            float height = 0f;

            foreach (string key
                     in context.MainPath)
            {
                MermaidLayoutItem item =
                    GetItem(
                        context,
                        key);

                height +=
                    item.Height;
            }

            height +=
                GetVerticalSpacing(
                    context) *
                Math.Max(
                    0,
                    context.MainPath.Count - 1);

            return height;
        }
    }
}