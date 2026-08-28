
using MermaidDiagram.Core.Layout.Result;

namespace MermaidDiagram.Core.Layout
{
    /// <summary>
    /// Đưa graph đã layout vào viewport.
    ///
    /// Chỉ xử lý:
    ///     - Scale
    ///     - Translation
    ///     - Centering
    ///
    /// Không thay đổi:
    ///     - topology
    ///     - thứ tự node
    ///     - layer
    ///     - main path
    ///     - quan hệ branch
    ///
    /// ViewportFitter là idempotent:
    ///
    ///     Fit(context)
    ///     Fit(context)
    ///     Fit(context)
    ///
    /// đều cho cùng một kết quả, không scale cộng dồn.
    /// </summary>
    public sealed class ViewportFitter
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Scale và căn giữa toàn bộ graph trong viewport.
        ///
        /// Geometry layout gốc được giữ riêng trong
        /// MermaidLayoutItem.LayoutX/LayoutY/LayoutWidth/LayoutHeight.
        /// </summary>
        public void Fit(
            RootLayoutContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (context.Items.Count == 0)
            {
                ResetEmptyContext(
                    context);

                return;
            }

            /*
             * Snapshot geometry layout cuối cùng trước viewport transform.
             *
             * Sau bước này:
             *
             * LayoutX/LayoutY/LayoutWidth/LayoutHeight
             *     = geometry trước viewport
             *
             * X/Y/Width/Height
             *     = geometry hiện tại sau viewport transform
             */
            CaptureLayoutGeometry(
                context);

            if (context.ViewportWidth <= 0f ||
                context.ViewportHeight <= 0f)
            {
                RestoreLayoutGeometry(
                    context);

                context.Scale =
                    1f;

                CalculateFinalBounds(
                    context);

                return;
            }

            // --------------------------------------------------------
            // 1. Calculate source graph bounds
            // --------------------------------------------------------

            GraphGeometry source =
                CalculateLayoutBounds(
                    context);

            if (source.Width <= 0f ||
                source.Height <= 0f)
            {
                RestoreLayoutGeometry(
                    context);

                context.Scale =
                    1f;

                CalculateFinalBounds(
                    context);

                return;
            }

            // --------------------------------------------------------
            // 2. Calculate scale
            // --------------------------------------------------------

            float scale =
                CalculateScale(
                    context,
                    source);

            context.Scale =
                scale;

            // --------------------------------------------------------
            // 3. Apply scale + center
            // --------------------------------------------------------

            ApplyTransform(
                context,
                source,
                scale);

            // --------------------------------------------------------
            // 4. Calculate final bounds
            // --------------------------------------------------------

            CalculateFinalBounds(
                context);
        }

        // ============================================================
        // CAPTURE
        // ============================================================

        /// <summary>
        /// Lưu geometry layout gốc.
        ///
        /// Chỉ snapshot một lần cho mỗi context.
        ///
        /// Điều này ngăn việc:
        ///
        /// Fit()
        ///   -> scale
        /// Fit()
        ///   -> scale lần nữa
        ///
        /// </summary>
        private void CaptureLayoutGeometry(
            RootLayoutContext context)
        {
            foreach (MermaidLayoutItem item
                     in context.Items.Values)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.HasLayoutGeometry)
                {
                    continue;
                }

                item.LayoutX =
                    item.X;

                item.LayoutY =
                    item.Y;

                item.LayoutWidth =
                    item.Width;

                item.LayoutHeight =
                    item.Height;

                item.HasLayoutGeometry =
                    true;
            }
        }

        // ============================================================
        // RESTORE
        // ============================================================

        /// <summary>
        /// Khôi phục geometry layout trước viewport transform.
        /// </summary>
        private void RestoreLayoutGeometry(
            RootLayoutContext context)
        {
            foreach (MermaidLayoutItem item
                     in context.Items.Values)
            {
                if (item == null ||
                    !item.HasLayoutGeometry)
                {
                    continue;
                }

                item.X =
                    item.LayoutX;

                item.Y =
                    item.LayoutY;

                item.Width =
                    item.LayoutWidth;

                item.Height =
                    item.LayoutHeight;
            }
        }

        // ============================================================
        // SOURCE BOUNDS
        // ============================================================

        /// <summary>
        /// Tính bounds từ geometry layout gốc.
        ///
        /// Không dùng X/Y/Width/Height hiện tại vì chúng có thể
        /// đã qua viewport transform từ lần Fit trước.
        /// </summary>
        private GraphGeometry CalculateLayoutBounds(
            RootLayoutContext context)
        {
            bool hasItem =
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
                if (item == null ||
                    !item.HasLayoutGeometry ||
                    !item.IsPositioned)
                {
                    continue;
                }

                hasItem =
                    true;

                minX =
                    Math.Min(
                        minX,
                        item.LayoutX);

                minY =
                    Math.Min(
                        minY,
                        item.LayoutY);

                maxX =
                    Math.Max(
                        maxX,
                        item.LayoutX +
                        item.LayoutWidth);

                maxY =
                    Math.Max(
                        maxY,
                        item.LayoutY +
                        item.LayoutHeight);
            }

            if (!hasItem)
            {
                return new GraphGeometry(
                    0f,
                    0f,
                    0f,
                    0f);
            }

            return new GraphGeometry(
                minX,
                minY,
                Math.Max(
                    0f,
                    maxX - minX),
                Math.Max(
                    0f,
                    maxY - minY));
        }

        // ============================================================
        // SCALE
        // ============================================================

        private float CalculateScale(
            RootLayoutContext context,
            GraphGeometry source)
        {
            if (!context.Options.UseAdaptiveScale)
            {
                return ClampScale(
                    context,
                    1f,
                    source);
            }

            float availableWidth =
                context.ViewportWidth
                - context.Options.MarginLeft
                - context.Options.MarginRight;

            float availableHeight =
                context.ViewportHeight
                - context.Options.MarginTop
                - context.Options.MarginBottom;

            if (availableWidth <= 0f ||
                availableHeight <= 0f)
            {
                return 1f;
            }

            float scaleX =
                availableWidth /
                source.Width;

            float scaleY =
                availableHeight /
                source.Height;

            float scale =
                Math.Min(
                    scaleX,
                    scaleY);

            return ClampScale(
                context,
                scale,
                source);
        }

        // ============================================================
        // SCALE CLAMP
        // ============================================================

        private float ClampScale(
            RootLayoutContext context,
            float scale,
            GraphGeometry source)
        {
            if (scale <= 0f)
            {
                scale =
                    1f;
            }

            /*
             * MaximumScale.
             */
            if (scale >
                context.Options.MaximumScale)
            {
                scale =
                    context.Options.MaximumScale;
            }

            /*
             * MinimumScale.
             *
             * Không ép lên MinimumScale nếu graph
             * không thể vừa viewport.
             */
            if (scale <
                context.Options.MinimumScale)
            {
                float minimum =
                    context.Options.MinimumScale;

                float fitScale =
                    CalculateFitScale(
                        context,
                        source);

                if (fitScale >= minimum)
                {
                    scale =
                        minimum;
                }
                else
                {
                    scale =
                        fitScale;
                }
            }

            return scale;
        }

        // ============================================================
        // PURE FIT SCALE
        // ============================================================

        private float CalculateFitScale(
            RootLayoutContext context,
            GraphGeometry source)
        {
            float availableWidth =
                context.ViewportWidth
                - context.Options.MarginLeft
                - context.Options.MarginRight;

            float availableHeight =
                context.ViewportHeight
                - context.Options.MarginTop
                - context.Options.MarginBottom;

            if (availableWidth <= 0f ||
                availableHeight <= 0f ||
                source.Width <= 0f ||
                source.Height <= 0f)
            {
                return 1f;
            }

            float scaleX =
                availableWidth /
                source.Width;

            float scaleY =
                availableHeight /
                source.Height;

            return Math.Min(
                scaleX,
                scaleY);
        }

        // ============================================================
        // TRANSFORM
        // ============================================================

        private void ApplyTransform(
            RootLayoutContext context,
            GraphGeometry source,
            float scale)
        {
            float graphWidth =
                source.Width *
                scale;

            float graphHeight =
                source.Height *
                scale;

            float availableWidth =
                context.ViewportWidth
                - context.Options.MarginLeft
                - context.Options.MarginRight;

            float availableHeight =
                context.ViewportHeight
                - context.Options.MarginTop
                - context.Options.MarginBottom;

            float offsetX =
                context.Options.MarginLeft
                + (availableWidth - graphWidth) / 2f;

            float offsetY =
                context.Options.MarginTop
                + (availableHeight - graphHeight) / 2f;

            foreach (MermaidLayoutItem item
                     in context.Items.Values)
            {
                if (item == null ||
                    !item.HasLayoutGeometry ||
                    !item.IsPositioned)
                {
                    continue;
                }

                float relativeX =
                    item.LayoutX -
                    source.X;

                float relativeY =
                    item.LayoutY -
                    source.Y;

                item.X =
                    offsetX +
                    relativeX * scale;

                item.Y =
                    offsetY +
                    relativeY * scale;

                item.Width =
                    item.LayoutWidth *
                    scale;

                item.Height =
                    item.LayoutHeight *
                    scale;
            }
        }

        // ============================================================
        // FINAL BOUNDS
        // ============================================================

        private void CalculateFinalBounds(
            RootLayoutContext context)
        {
            bool hasItem =
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
                if (item == null ||
                    !item.IsPositioned)
                {
                    continue;
                }

                hasItem =
                    true;

                minX =
                    Math.Min(
                        minX,
                        item.X);

                minY =
                    Math.Min(
                        minY,
                        item.Y);

                maxX =
                    Math.Max(
                        maxX,
                        item.Right);

                maxY =
                    Math.Max(
                        maxY,
                        item.Bottom);
            }

            if (!hasItem)
            {
                ResetEmptyContext(
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
        // EMPTY CONTEXT
        // ============================================================

        private void ResetEmptyContext(
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

            context.Scale =
                1f;
        }

        // ============================================================
        // GEOMETRY
        // ============================================================

        private sealed class GraphGeometry
        {
            public float X { get; }

            public float Y { get; }

            public float Width { get; }

            public float Height { get; }

            public GraphGeometry(
                float x,
                float y,
                float width,
                float height)
            {
                X =
                    x;

                Y =
                    y;

                Width =
                    width;

                Height =
                    height;
            }
        }
    }
}

