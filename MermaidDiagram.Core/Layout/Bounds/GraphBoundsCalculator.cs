
using MermaidDiagram.Core.Layout.Result;
using System;

namespace MermaidDiagram.Core.Layout.Bounds
{
    /// <summary>
    /// Tính bounding box cuối cùng của toàn bộ layout.
    ///
    /// GraphBoundsCalculator chỉ chịu trách nhiệm:
    ///
    ///     - đọc geometry của LayoutItem
    ///     - tìm minX / minY
    ///     - tìm maxX / maxY
    ///     - cập nhật GraphWidth / GraphHeight
    ///
    /// Không chịu trách nhiệm:
    ///
    ///     - layout node
    ///     - MainPath
    ///     - BranchLayout
    ///     - GroupLayout
    ///     - EdgeRouting
    ///     - collision
    ///     - spacing
    ///     - viewport fitting
    ///     - scale
    ///     - DevExpress
    /// </summary>
    public sealed class GraphBoundsCalculator
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Tính bounds hiện tại của toàn bộ layout.
        ///
        /// Method này được gọi:
        ///
        ///     1. Sau GroupLayoutEngine
        ///     2. Trước ViewportFitter
        ///     3. Sau ViewportFitter
        ///
        /// Không làm thay đổi vị trí node.
        /// </summary>
        public void Calculate(
            RootLayoutContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (context.Items.Count == 0)
            {
                ResetBounds(
                    context);

                return;
            }

            bool hasPositionedItem =
                false;

            float minX =
                float.MaxValue;

            float minY =
                float.MaxValue;

            float maxX =
                float.MinValue;

            float maxY =
                float.MinValue;

            foreach (MermaidLayoutItem item
                     in context.Items.Values)
            {
                if (!HasValidGeometry(
                        item))
                {
                    continue;
                }

                hasPositionedItem =
                    true;

                float left =
                    item.X;

                float top =
                    item.Y;

                float right =
                    item.X +
                    item.Width;

                float bottom =
                    item.Y +
                    item.Height;

                minX =
                    Math.Min(
                        minX,
                        left);

                minY =
                    Math.Min(
                        minY,
                        top);

                maxX =
                    Math.Max(
                        maxX,
                        right);

                maxY =
                    Math.Max(
                        maxY,
                        bottom);
            }

            if (!hasPositionedItem)
            {
                ResetBounds(
                    context);

                return;
            }

            context.GraphMinX =
                minX;

            context.GraphMinY =
                minY;

            context.GraphMaxX =
                maxX;

            context.GraphMaxY =
                maxY;

            context.GraphWidth =
                Math.Max(
                    0f,
                    maxX - minX);

            context.GraphHeight =
                Math.Max(
                    0f,
                    maxY - minY);
        }

        // ============================================================
        // VALID GEOMETRY
        // ============================================================

        /// <summary>
        /// Chỉ geometry hợp lệ mới được đưa vào graph bounds.
        ///
        /// Điều này bảo vệ GraphBoundsCalculator khỏi:
        ///
        ///     NaN
        ///     Infinity
        ///     Width <= 0
        ///     Height <= 0
        ///     item chưa layout
        /// </summary>
        private static bool HasValidGeometry(
            MermaidLayoutItem item)
        {
            if (item == null)
            {
                return false;
            }

            if (!item.IsPositioned)
            {
                return false;
            }

            if (float.IsNaN(item.X) ||
                float.IsNaN(item.Y) ||
                float.IsNaN(item.Width) ||
                float.IsNaN(item.Height))
            {
                return false;
            }

            if (float.IsInfinity(item.X) ||
                float.IsInfinity(item.Y) ||
                float.IsInfinity(item.Width) ||
                float.IsInfinity(item.Height))
            {
                return false;
            }

            if (item.Width <= 0f ||
                item.Height <= 0f)
            {
                return false;
            }

            return true;
        }

        // ============================================================
        // RESET
        // ============================================================

        private static void ResetBounds(
            RootLayoutContext context)
        {
            context.GraphMinX =
                0f;

            context.GraphMinY =
                0f;

            context.GraphMaxX =
                0f;

            context.GraphMaxY =
                0f;

            context.GraphWidth =
                0f;

            context.GraphHeight =
                0f;
        }
    }
}

