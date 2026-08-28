using MermaidDiagram.Core.Layout.Contract;
using MermaidDiagram.Core.Layout.CoreLayout;
using MermaidDiagram.Core.Layout.Result;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MermaidDiagram.Core.Layout.Layered
{
    /// <summary>
    /// Chuyển layer + ordering thành tọa độ X/Y.
    ///
    /// Mô hình:
    ///
    ///     Layer
    ///       ↓
    ///     Ordering
    ///       ↓
    ///     Barycenter alignment
    ///       ↓
    ///     Coordinate
    ///
    /// Dummy node được coi như node hình học trong quá trình
    /// coordinate assignment để giữ "lane" cho cạnh dài.
    ///
    /// Không routing edge.
    /// Không collision cuối cùng.
    /// </summary>
    public sealed class CoordinateAssignmentEngine
    {
        private const int RelaxationIterations = 8;

        private const float AlignmentFactor = 0.75f;

        // ============================================================
        // PUBLIC
        // ============================================================

        public void Assign(
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
                AssignHorizontal(
                    context,
                    layers);

                return;
            }

            AssignVertical(
                context,
                layers);
        }

        // ============================================================
        // VERTICAL
        // ============================================================

        private static void AssignVertical(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers)
        {
            List<int> orderedLayers =
                layers.Keys
                    .OrderBy(x => x)
                    .ToList();

            if (orderedLayers.Count == 0)
            {
                return;
            }

            // --------------------------------------------------------
            // 1. Y cố định theo layer.
            // --------------------------------------------------------

            AssignVerticalY(
                context,
                layers,
                orderedLayers);

            // --------------------------------------------------------
            // 2. X initial theo ordering.
            // --------------------------------------------------------

            InitializeVerticalX(
                context,
                layers,
                orderedLayers);

            // --------------------------------------------------------
            // 3. Relaxation theo barycenter.
            // --------------------------------------------------------

            RelaxVerticalX(
                context,
                layers,
                orderedLayers);

            // --------------------------------------------------------
            // 4. Ép spacing cuối cùng.
            // --------------------------------------------------------

            CompactVerticalLayers(
                context,
                layers,
                orderedLayers);

            // --------------------------------------------------------
            // 5. Center toàn graph.
            // --------------------------------------------------------

            CenterGraphHorizontally(
                context);
        }

        // ============================================================
        // VERTICAL Y
        // ============================================================

        private static void AssignVerticalY(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers,
            List<int> orderedLayers)
        {
            float currentY =
                0f;

            foreach (int layerIndex in orderedLayers)
            {
                List<string> nodes =
                    layers[layerIndex];

                if (nodes == null ||
                    nodes.Count == 0)
                {
                    continue;
                }

                float maxHeight =
                    GetMaxHeight(
                        context,
                        nodes);

                foreach (string key in nodes)
                {
                    if (!TryGetItem(
                            context,
                            key,
                            out MermaidLayoutItem item))
                    {
                        continue;
                    }

                    item.Y =
                        currentY;

                    item.Layer =
                        layerIndex;

                    item.IsPositioned =
                        true;
                }

                currentY +=
                    maxHeight +
                    Math.Max(
                        0f,
                        context.VerticalSpacing);
            }
        }

        // ============================================================
        // INITIAL X
        // ============================================================

        private static void InitializeVerticalX(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers,
            List<int> orderedLayers)
        {
            foreach (int layerIndex in orderedLayers)
            {
                List<string> nodes =
                    layers[layerIndex];

                if (nodes == null ||
                    nodes.Count == 0)
                {
                    continue;
                }

                float totalWidth =
                    CalculateLayerWidth(
                        context,
                        nodes);

                float startX =
                    -totalWidth / 2f;

                foreach (string key in nodes)
                {
                    if (!TryGetItem(
                            context,
                            key,
                            out MermaidLayoutItem item))
                    {
                        continue;
                    }

                    item.X =
                        startX;

                    startX +=
                        item.Width +
                        GetHorizontalSpacing(
                            context);
                }
            }
        }

        // ============================================================
        // RELAX VERTICAL
        // ============================================================

        private static void RelaxVerticalX(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers,
            List<int> orderedLayers)
        {
            for (int pass = 0;
                 pass < RelaxationIterations;
                 pass++)
            {
                bool downward =
                    pass % 2 == 0;

                IEnumerable<int> sweep =
                    downward
                        ? orderedLayers
                        : orderedLayers.AsEnumerable().Reverse();

                foreach (int layerIndex in sweep)
                {
                    List<string> nodes =
                        layers[layerIndex];

                    if (nodes == null ||
                        nodes.Count == 0)
                    {
                        continue;
                    }

                    AlignVerticalLayer(
                        context,
                        layers,
                        orderedLayers,
                        layerIndex,
                        nodes,
                        downward);
                }
            }
        }

        // ============================================================
        // ALIGN VERTICAL LAYER
        // ============================================================

        private static void AlignVerticalLayer(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers,
            List<int> orderedLayers,
            int layerIndex,
            List<string> nodes,
            bool downward)
        {
            int layerPosition =
                orderedLayers.IndexOf(
                    layerIndex);

            if (layerPosition < 0)
            {
                return;
            }

            List<string> previous =
                layerPosition > 0
                    ? layers[orderedLayers[layerPosition - 1]]
                    : null;

            List<string> next =
                layerPosition < orderedLayers.Count - 1
                    ? layers[orderedLayers[layerPosition + 1]]
                    : null;

            foreach (string key in nodes)
            {
                if (!TryGetItem(
                        context,
                        key,
                        out MermaidLayoutItem item))
                {
                    continue;
                }

                float currentCenter =
                    GetCenterX(item);

                List<float> neighborCenters =
                    new List<float>();

                if (previous != null)
                {
                    AddParentLayerNeighbors(
                        context,
                        key,
                        previous,
                        neighborCenters);
                }

                if (next != null)
                {
                    AddChildLayerNeighbors(
                        context,
                        key,
                        next,
                        neighborCenters);
                }

                if (neighborCenters.Count == 0)
                {
                    continue;
                }

                float barycenter =
                    neighborCenters.Average();

                float targetCenter =
                    currentCenter +
                    (barycenter - currentCenter) *
                    AlignmentFactor;

                if (IsMainPathNode(
                        context,
                        key))
                {
                    /*
                     * MainPath giữ gần trục X = 0.
                     *
                     * Không khóa cứng vì vẫn cần cho graph
                     * hội tụ theo barycenter.
                     */
                    targetCenter =
                        targetCenter * 0.35f;
                }

                float delta =
                    targetCenter -
                    currentCenter;

                item.X +=
                    delta;
            }

            EnforceHorizontalOrder(
                context,
                nodes);
        }

        // ============================================================
        // COMPACT VERTICAL
        // ============================================================

        private static void CompactVerticalLayers(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers,
            List<int> orderedLayers)
        {
            foreach (int layerIndex in orderedLayers)
            {
                List<string> nodes =
                    layers[layerIndex];

                if (nodes == null ||
                    nodes.Count == 0)
                {
                    continue;
                }

                EnforceHorizontalOrder(
                    context,
                    nodes);
            }
        }

        // ============================================================
        // HORIZONTAL
        // ============================================================

        private static void AssignHorizontal(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers)
        {
            List<int> orderedLayers =
                layers.Keys
                    .OrderBy(x => x)
                    .ToList();

            if (orderedLayers.Count == 0)
            {
                return;
            }

            // --------------------------------------------------------
            // 1. X theo layer.
            // --------------------------------------------------------

            AssignHorizontalX(
                context,
                layers,
                orderedLayers);

            // --------------------------------------------------------
            // 2. Y initial theo ordering.
            // --------------------------------------------------------

            InitializeHorizontalY(
                context,
                layers,
                orderedLayers);

            // --------------------------------------------------------
            // 3. Relaxation theo barycenter.
            // --------------------------------------------------------

            RelaxHorizontalY(
                context,
                layers,
                orderedLayers);

            // --------------------------------------------------------
            // 4. Spacing cuối.
            // --------------------------------------------------------

            CompactHorizontalLayers(
                context,
                layers,
                orderedLayers);

            // --------------------------------------------------------
            // 5. Center.
            // --------------------------------------------------------

            CenterGraphVertically(
                context);
        }

        // ============================================================
        // HORIZONTAL X
        // ============================================================

        private static void AssignHorizontalX(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers,
            List<int> orderedLayers)
        {
            float currentX =
                0f;

            foreach (int layerIndex in orderedLayers)
            {
                List<string> nodes =
                    layers[layerIndex];

                if (nodes == null ||
                    nodes.Count == 0)
                {
                    continue;
                }

                float maxWidth =
                    GetMaxWidth(
                        context,
                        nodes);

                foreach (string key in nodes)
                {
                    if (!TryGetItem(
                            context,
                            key,
                            out MermaidLayoutItem item))
                    {
                        continue;
                    }

                    item.X =
                        currentX;

                    item.Layer =
                        layerIndex;

                    item.IsPositioned =
                        true;
                }

                currentX +=
                    maxWidth +
                    GetHorizontalSpacing(
                        context);
            }
        }

        // ============================================================
        // INITIAL Y
        // ============================================================

        private static void InitializeHorizontalY(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers,
            List<int> orderedLayers)
        {
            foreach (int layerIndex in orderedLayers)
            {
                List<string> nodes =
                    layers[layerIndex];

                if (nodes == null ||
                    nodes.Count == 0)
                {
                    continue;
                }

                float totalHeight =
                    CalculateLayerHeight(
                        context,
                        nodes);

                float startY =
                    -totalHeight / 2f;

                foreach (string key in nodes)
                {
                    if (!TryGetItem(
                            context,
                            key,
                            out MermaidLayoutItem item))
                    {
                        continue;
                    }

                    item.Y =
                        startY;

                    startY +=
                        item.Height +
                        GetVerticalSpacing(
                            context);
                }
            }
        }

        // ============================================================
        // RELAX HORIZONTAL
        // ============================================================

        private static void RelaxHorizontalY(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers,
            List<int> orderedLayers)
        {
            for (int pass = 0;
                 pass < RelaxationIterations;
                 pass++)
            {
                bool forward =
                    pass % 2 == 0;

                IEnumerable<int> sweep =
                    forward
                        ? orderedLayers
                        : orderedLayers.AsEnumerable().Reverse();

                foreach (int layerIndex in sweep)
                {
                    List<string> nodes =
                        layers[layerIndex];

                    if (nodes == null ||
                        nodes.Count == 0)
                    {
                        continue;
                    }

                    AlignHorizontalLayer(
                        context,
                        layers,
                        orderedLayers,
                        layerIndex,
                        nodes);
                }
            }
        }

        // ============================================================
        // ALIGN HORIZONTAL LAYER
        // ============================================================

        private static void AlignHorizontalLayer(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers,
            List<int> orderedLayers,
            int layerIndex,
            List<string> nodes)
        {
            int layerPosition =
                orderedLayers.IndexOf(
                    layerIndex);

            if (layerPosition < 0)
            {
                return;
            }

            List<string> previous =
                layerPosition > 0
                    ? layers[orderedLayers[layerPosition - 1]]
                    : null;

            List<string> next =
                layerPosition < orderedLayers.Count - 1
                    ? layers[orderedLayers[layerPosition + 1]]
                    : null;

            foreach (string key in nodes)
            {
                if (!TryGetItem(
                        context,
                        key,
                        out MermaidLayoutItem item))
                {
                    continue;
                }

                float currentCenter =
                    GetCenterY(item);

                List<float> neighborCenters =
                    new List<float>();

                if (previous != null)
                {
                    AddParentLayerNeighborsY(
                        context,
                        key,
                        previous,
                        neighborCenters);
                }

                if (next != null)
                {
                    AddChildLayerNeighborsY(
                        context,
                        key,
                        next,
                        neighborCenters);
                }

                if (neighborCenters.Count == 0)
                {
                    continue;
                }

                float barycenter =
                    neighborCenters.Average();

                float targetCenter =
                    currentCenter +
                    (barycenter - currentCenter) *
                    AlignmentFactor;

                if (IsMainPathNode(
                        context,
                        key))
                {
                    targetCenter =
                        targetCenter * 0.35f;
                }

                item.Y +=
                    targetCenter -
                    currentCenter;
            }

            EnforceVerticalOrder(
                context,
                nodes);
        }

        // ============================================================
        // COMPACT HORIZONTAL
        // ============================================================

        private static void CompactHorizontalLayers(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers,
            List<int> orderedLayers)
        {
            foreach (int layerIndex in orderedLayers)
            {
                List<string> nodes =
                    layers[layerIndex];

                if (nodes == null ||
                    nodes.Count == 0)
                {
                    continue;
                }

                EnforceVerticalOrder(
                    context,
                    nodes);
            }
        }

        // ============================================================
        // NEIGHBORS - X
        // ============================================================

        private static void AddParentLayerNeighbors(
            RootLayoutContext context,
            string key,
            List<string> previousLayer,
            List<float> result)
        {
            if (context.EffectiveAdjacency == null)
            {
                return;
            }

            foreach (string parent in previousLayer)
            {
                if (!context.EffectiveAdjacency.TryGetValue(
                        parent,
                        out List<string> children))
                {
                    continue;
                }

                if (children == null)
                {
                    continue;
                }

                if (!ContainsIgnoreCase(
                        children,
                        key))
                {
                    continue;
                }

                if (!TryGetItem(
                        context,
                        parent,
                        out MermaidLayoutItem item))
                {
                    continue;
                }

                if (!item.IsPositioned)
                {
                    continue;
                }

                result.Add(
                    GetCenterX(item));
            }
        }

        private static void AddChildLayerNeighbors(
            RootLayoutContext context,
            string key,
            List<string> nextLayer,
            List<float> result)
        {
            if (!context.EffectiveAdjacency.TryGetValue(
                    key,
                    out List<string> children))
            {
                return;
            }

            if (children == null)
            {
                return;
            }

            foreach (string child in children)
            {
                if (!ContainsIgnoreCase(
                        nextLayer,
                        child))
                {
                    continue;
                }

                if (!TryGetItem(
                        context,
                        child,
                        out MermaidLayoutItem item))
                {
                    continue;
                }

                if (!item.IsPositioned)
                {
                    continue;
                }

                result.Add(
                    GetCenterX(item));
            }
        }

        // ============================================================
        // NEIGHBORS - Y
        // ============================================================

        private static void AddParentLayerNeighborsY(
            RootLayoutContext context,
            string key,
            List<string> previousLayer,
            List<float> result)
        {
            foreach (string parent in previousLayer)
            {
                if (!context.EffectiveAdjacency.TryGetValue(
                        parent,
                        out List<string> children))
                {
                    continue;
                }

                if (children == null)
                {
                    continue;
                }

                if (!ContainsIgnoreCase(
                        children,
                        key))
                {
                    continue;
                }

                if (!TryGetItem(
                        context,
                        parent,
                        out MermaidLayoutItem item))
                {
                    continue;
                }

                if (!item.IsPositioned)
                {
                    continue;
                }

                result.Add(
                    GetCenterY(item));
            }
        }

        private static void AddChildLayerNeighborsY(
            RootLayoutContext context,
            string key,
            List<string> nextLayer,
            List<float> result)
        {
            if (!context.EffectiveAdjacency.TryGetValue(
                    key,
                    out List<string> children))
            {
                return;
            }

            if (children == null)
            {
                return;
            }

            foreach (string child in children)
            {
                if (!ContainsIgnoreCase(
                        nextLayer,
                        child))
                {
                    continue;
                }

                if (!TryGetItem(
                        context,
                        child,
                        out MermaidLayoutItem item))
                {
                    continue;
                }

                if (!item.IsPositioned)
                {
                    continue;
                }

                result.Add(
                    GetCenterY(item));
            }
        }

        // ============================================================
        // ORDER ENFORCEMENT - X
        // ============================================================

        private static void EnforceHorizontalOrder(
            RootLayoutContext context,
            List<string> nodes)
        {
            float spacing =
                GetHorizontalSpacing(
                    context);

            MermaidLayoutItem previous =
                null;

            foreach (string key in nodes)
            {
                if (!TryGetItem(
                        context,
                        key,
                        out MermaidLayoutItem current))
                {
                    continue;
                }

                if (previous != null)
                {
                    float minimumX =
                        previous.Right +
                        spacing;

                    if (current.X < minimumX)
                    {
                        current.X =
                            minimumX;
                    }
                }

                previous =
                    current;
            }
        }

        // ============================================================
        // ORDER ENFORCEMENT - Y
        // ============================================================

        private static void EnforceVerticalOrder(
            RootLayoutContext context,
            List<string> nodes)
        {
            float spacing =
                GetVerticalSpacing(
                    context);

            MermaidLayoutItem previous =
                null;

            foreach (string key in nodes)
            {
                if (!TryGetItem(
                        context,
                        key,
                        out MermaidLayoutItem current))
                {
                    continue;
                }

                if (previous != null)
                {
                    float minimumY =
                        previous.Bottom +
                        spacing;

                    if (current.Y < minimumY)
                    {
                        current.Y =
                            minimumY;
                    }
                }

                previous =
                    current;
            }
        }

        // ============================================================
        // WIDTH
        // ============================================================

        private static float CalculateLayerWidth(
            RootLayoutContext context,
            List<string> nodes)
        {
            float result =
                0f;

            int validCount =
                0;

            foreach (string key in nodes)
            {
                if (!TryGetItem(
                        context,
                        key,
                        out MermaidLayoutItem item))
                {
                    continue;
                }

                result +=
                    item.Width;

                validCount++;
            }

            if (validCount > 1)
            {
                result +=
                    (validCount - 1) *
                    GetHorizontalSpacing(
                        context);
            }

            return result;
        }

        // ============================================================
        // HEIGHT
        // ============================================================

        private static float CalculateLayerHeight(
            RootLayoutContext context,
            List<string> nodes)
        {
            float result =
                0f;

            int validCount =
                0;

            foreach (string key in nodes)
            {
                if (!TryGetItem(
                        context,
                        key,
                        out MermaidLayoutItem item))
                {
                    continue;
                }

                result +=
                    item.Height;

                validCount++;
            }

            if (validCount > 1)
            {
                result +=
                    (validCount - 1) *
                    GetVerticalSpacing(
                        context);
            }

            return result;
        }

        // ============================================================
        // MAX SIZE
        // ============================================================

        private static float GetMaxHeight(
            RootLayoutContext context,
            List<string> nodes)
        {
            float max =
                0f;

            foreach (string key in nodes)
            {
                if (!TryGetItem(
                        context,
                        key,
                        out MermaidLayoutItem item))
                {
                    continue;
                }

                max =
                    Math.Max(
                        max,
                        item.Height);
            }

            return max > 0f
                ? max
                : context.Options.NodeHeight;
        }

        private static float GetMaxWidth(
            RootLayoutContext context,
            List<string> nodes)
        {
            float max =
                0f;

            foreach (string key in nodes)
            {
                if (!TryGetItem(
                        context,
                        key,
                        out MermaidLayoutItem item))
                {
                    continue;
                }

                max =
                    Math.Max(
                        max,
                        item.Width);
            }

            return max > 0f
                ? max
                : context.Options.NodeWidth;
        }

        // ============================================================
        // CENTER GRAPH X
        // ============================================================

        private static void CenterGraphHorizontally(
            RootLayoutContext context)
        {
            bool found =
                false;

            float minX =
                float.MaxValue;

            float maxRight =
                float.MinValue;

            foreach (MermaidLayoutItem item
                     in context.Items.Values)
            {
                if (item == null ||
                    !item.IsPositioned)
                {
                    continue;
                }

                found =
                    true;

                minX =
                    Math.Min(
                        minX,
                        item.X);

                maxRight =
                    Math.Max(
                        maxRight,
                        item.Right);
            }

            if (!found)
            {
                return;
            }

            float center =
                (minX + maxRight) / 2f;

            foreach (MermaidLayoutItem item
                     in context.Items.Values)
            {
                if (item == null ||
                    !item.IsPositioned)
                {
                    continue;
                }

                item.X -=
                    center;
            }
        }

        // ============================================================
        // CENTER GRAPH Y
        // ============================================================

        private static void CenterGraphVertically(
            RootLayoutContext context)
        {
            bool found =
                false;

            float minY =
                float.MaxValue;

            float maxBottom =
                float.MinValue;

            foreach (MermaidLayoutItem item
                     in context.Items.Values)
            {
                if (item == null ||
                    !item.IsPositioned)
                {
                    continue;
                }

                found =
                    true;

                minY =
                    Math.Min(
                        minY,
                        item.Y);

                maxBottom =
                    Math.Max(
                        maxBottom,
                        item.Bottom);
            }

            if (!found)
            {
                return;
            }

            float center =
                (minY + maxBottom) / 2f;

            foreach (MermaidLayoutItem item
                     in context.Items.Values)
            {
                if (item == null ||
                    !item.IsPositioned)
                {
                    continue;
                }

                item.Y -=
                    center;
            }
        }

        // ============================================================
        // ITEM HELPERS
        // ============================================================

        private static bool TryGetItem(
            RootLayoutContext context,
            string key,
            out MermaidLayoutItem item)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                item =
                    null;

                return false;
            }

            return context.Items.TryGetValue(
                key,
                out item);
        }

        private static bool IsMainPathNode(
            RootLayoutContext context,
            string key)
        {
            return context.MainPathIndex != null &&
                   context.MainPathIndex.ContainsKey(
                       key);
        }

        private static bool ContainsIgnoreCase(
            List<string> values,
            string key)
        {
            return values != null &&
                   values.Any(
                       x => string.Equals(
                           x,
                           key,
                           StringComparison.OrdinalIgnoreCase));
        }

        // ============================================================
        // GEOMETRY
        // ============================================================

        private static float GetCenterX(
            MermaidLayoutItem item)
        {
            return item.X +
                   item.Width / 2f;
        }

        private static float GetCenterY(
            MermaidLayoutItem item)
        {
            return item.Y +
                   item.Height / 2f;
        }

        // ============================================================
        // SPACING
        // ============================================================

        private static float GetHorizontalSpacing(
            RootLayoutContext context)
        {
            return Math.Max(
                0f,
                context.HorizontalSpacing);
        }

        private static float GetVerticalSpacing(
            RootLayoutContext context)
        {
            return Math.Max(
                0f,
                context.VerticalSpacing);
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