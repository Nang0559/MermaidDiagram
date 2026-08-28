using MermaidDiagram.Core.Layout.Contract;
using MermaidDiagram.Core.Layout.CoreLayout;
using MermaidDiagram.Core.Layout.Result;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MermaidDiagram.Core.Layout.Layered
{
    /// <summary>
    /// Resolve collision giữa các node cùng layer.
    ///
    /// Không thay đổi:
    ///     - graph topology
    ///     - layer
    ///     - ordering
    ///
    /// Chỉ điều chỉnh coordinate.
    /// </summary>
    public sealed class CollisionResolver
    {
        public void Resolve(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (layers == null)
            {
                throw new ArgumentNullException(
                    nameof(layers));
            }

            if (layers.Count == 0)
            {
                return;
            }

            if (IsHorizontalDirection(
                    context.Direction))
            {
                ResolveHorizontal(
                    context,
                    layers);

                return;
            }

            ResolveVertical(
                context,
                layers);
        }

        // ============================================================
        // VERTICAL
        // ============================================================

        private static void ResolveVertical(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers)
        {
            foreach (List<string> layer
                     in layers.Values)
            {
                if (layer == null ||
                    layer.Count <= 1)
                {
                    continue;
                }

                List<MermaidLayoutItem> items =
                    layer
                        .Where(
                            x => context.Items.ContainsKey(x))
                        .Select(
                            x => context.Items[x])
                        .Where(
                            x => x != null &&
                                 x.IsPositioned)
                        .OrderBy(
                            x => x.X)
                        .ToList();

                for (int i = 1;
                     i < items.Count;
                     i++)
                {
                    MermaidLayoutItem previous =
                        items[i - 1];

                    MermaidLayoutItem current =
                        items[i];

                    float minimumX =
                        previous.Right +
                        context.HorizontalSpacing;

                    if (current.X < minimumX)
                    {
                        current.X =
                            minimumX;
                    }
                }

                CenterLayerHorizontally(
                    items);
            }
        }

        // ============================================================
        // HORIZONTAL
        // ============================================================

        private static void ResolveHorizontal(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers)
        {
            foreach (List<string> layer
                     in layers.Values)
            {
                if (layer == null ||
                    layer.Count <= 1)
                {
                    continue;
                }

                List<MermaidLayoutItem> items =
                    layer
                        .Where(
                            x => context.Items.ContainsKey(x))
                        .Select(
                            x => context.Items[x])
                        .Where(
                            x => x != null &&
                                 x.IsPositioned)
                        .OrderBy(
                            x => x.Y)
                        .ToList();

                for (int i = 1;
                     i < items.Count;
                     i++)
                {
                    MermaidLayoutItem previous =
                        items[i - 1];

                    MermaidLayoutItem current =
                        items[i];

                    float minimumY =
                        previous.Bottom +
                        context.VerticalSpacing;

                    if (current.Y < minimumY)
                    {
                        current.Y =
                            minimumY;
                    }
                }

                CenterLayerVertically(
                    items);
            }
        }

        // ============================================================
        // CENTER LAYER X
        // ============================================================

        private static void CenterLayerHorizontally(
            List<MermaidLayoutItem> items)
        {
            if (items == null ||
                items.Count == 0)
            {
                return;
            }

            float minX =
                items.Min(
                    x => x.X);

            float maxX =
                items.Max(
                    x => x.Right);

            float center =
                (minX + maxX) / 2f;

            foreach (MermaidLayoutItem item
                     in items)
            {
                item.X -= center;
            }
        }

        // ============================================================
        // CENTER LAYER Y
        // ============================================================

        private static void CenterLayerVertically(
            List<MermaidLayoutItem> items)
        {
            if (items == null ||
                items.Count == 0)
            {
                return;
            }

            float minY =
                items.Min(
                    x => x.Y);

            float maxY =
                items.Max(
                    x => x.Bottom);

            float center =
                (minY + maxY) / 2f;

            foreach (MermaidLayoutItem item
                     in items)
            {
                item.Y -= center;
            }
        }

        // ============================================================
        // DIRECTION
        // ============================================================

        private static bool IsHorizontalDirection(
            MermaidLayoutDirection direction)
        {
            return
                direction ==
                    MermaidLayoutDirection.LeftToRight
                ||
                direction ==
                    MermaidLayoutDirection.RightToLeft;
        }
    }
}