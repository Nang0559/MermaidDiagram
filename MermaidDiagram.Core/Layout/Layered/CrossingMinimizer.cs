using MermaidDiagram.Core.Layout.CoreLayout;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MermaidDiagram.Core.Layout.Layered
{
    /// <summary>
    /// Giảm số edge crossing giữa các layer.
    ///
    /// Đây là bước rất quan trọng trong layered layout
    /// theo mô hình Sugiyama.
    ///
    /// LƯU Ý:
    ///     Sau khi DummyNodeInserter chạy,
    ///     mọi quan hệ edge dùng cho ordering/crossing
    ///     phải đọc từ RootLayoutContext.EffectiveAdjacency.
    ///
    /// Không được đọc Graph.Adjacency trực tiếp ở đây,
    /// vì Graph.Adjacency chỉ chứa edge Mermaid gốc.
    ///
    /// Ví dụ:
    ///
    ///     A(L1) -----------------> D(L4)
    ///
    /// sẽ trở thành:
    ///
    ///     A(L1) -> dummy(L2) -> dummy(L3) -> D(L4)
    ///
    /// CrossingMinimizer phải nhìn thấy chuỗi này.
    /// </summary>
    public sealed class CrossingMinimizer
    {
        private readonly LayerOrderingEngine _orderingEngine;

        private readonly int _iterations;

        public CrossingMinimizer(
            LayerOrderingEngine orderingEngine,
            int iterations = 8)
        {
            _orderingEngine =
                orderingEngine
                ?? throw new ArgumentNullException(
                    nameof(orderingEngine));

            _iterations =
                Math.Max(
                    1,
                    iterations);
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        public void Minimize(
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

            if (layers.Count <= 1)
            {
                return;
            }

            // --------------------------------------------------------
            // INITIAL ORDER
            // --------------------------------------------------------

            _orderingEngine.Order(
                context,
                layers);

            int previousCrossings =
                CountCrossings(
                    context,
                    layers);

            // --------------------------------------------------------
            // ITERATIVE BARYCENTER / MEDIAN SWEEP
            // --------------------------------------------------------

            for (int iteration = 0;
                 iteration < _iterations;
                 iteration++)
            {
                // ----------------------------------------------------
                // DOWNWARD
                // ----------------------------------------------------

                DownwardSweep(
                    context,
                    layers);

                // ----------------------------------------------------
                // UPWARD
                // ----------------------------------------------------

                UpwardSweep(
                    context,
                    layers);

                // ----------------------------------------------------
                // COUNT
                // ----------------------------------------------------

                int currentCrossings =
                    CountCrossings(
                        context,
                        layers);

                // ----------------------------------------------------
                // STOP IF NO IMPROVEMENT
                // ----------------------------------------------------

                if (currentCrossings >=
                    previousCrossings)
                {
                    break;
                }

                previousCrossings =
                    currentCrossings;
            }
        }

        // ============================================================
        // DOWNWARD SWEEP
        // ============================================================

        private static void DownwardSweep(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers)
        {
            List<int> ordered =
                layers.Keys
                    .OrderBy(x => x)
                    .ToList();

            for (int i = 1;
                 i < ordered.Count;
                 i++)
            {
                List<string> upper =
                    layers[ordered[i - 1]];

                List<string> current =
                    layers[ordered[i]];

                ReorderByIncomingMedian(
                    context,
                    current,
                    upper);
            }
        }

        // ============================================================
        // UPWARD SWEEP
        // ============================================================

        private static void UpwardSweep(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers)
        {
            List<int> ordered =
                layers.Keys
                    .OrderBy(x => x)
                    .ToList();

            for (int i = ordered.Count - 2;
                 i >= 0;
                 i--)
            {
                List<string> current =
                    layers[ordered[i]];

                List<string> lower =
                    layers[ordered[i + 1]];

                ReorderByOutgoingMedian(
                    context,
                    current,
                    lower);
            }
        }

        // ============================================================
        // INCOMING MEDIAN
        // ============================================================

        private static void ReorderByIncomingMedian(
            RootLayoutContext context,
            List<string> current,
            List<string> upper)
        {
            Dictionary<string, int> index =
                BuildIndex(upper);

            current.Sort(
                (a, b) =>
                {
                    double ma =
                        CalculateIncomingMedian(
                            context,
                            a,
                            index);

                    double mb =
                        CalculateIncomingMedian(
                            context,
                            b,
                            index);

                    int compare =
                        ma.CompareTo(mb);

                    if (compare != 0)
                    {
                        return compare;
                    }

                    return StringComparer.OrdinalIgnoreCase
                        .Compare(a, b);
                });
        }

        // ============================================================
        // OUTGOING MEDIAN
        // ============================================================

        private static void ReorderByOutgoingMedian(
            RootLayoutContext context,
            List<string> current,
            List<string> lower)
        {
            Dictionary<string, int> index =
                BuildIndex(lower);

            current.Sort(
                (a, b) =>
                {
                    double ma =
                        CalculateOutgoingMedian(
                            context,
                            a,
                            index);

                    double mb =
                        CalculateOutgoingMedian(
                            context,
                            b,
                            index);

                    int compare =
                        ma.CompareTo(mb);

                    if (compare != 0)
                    {
                        return compare;
                    }

                    return StringComparer.OrdinalIgnoreCase
                        .Compare(a, b);
                });
        }

        // ============================================================
        // INCOMING MEDIAN
        // ============================================================

        private static double CalculateIncomingMedian(
            RootLayoutContext context,
            string node,
            Dictionary<string, int> upperIndex)
        {
            List<int> indexes =
                new List<int>();

            // ========================================================
            // QUAN TRỌNG:
            //
            // KHÔNG dùng:
            //
            //     context.Graph.Adjacency
            //
            // Phải dùng:
            //
            //     context.EffectiveAdjacency
            //
            // để dummy node được tham gia ordering.
            // ========================================================

            foreach (KeyValuePair<
                         string,
                         List<string>> pair
                     in context.EffectiveAdjacency)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                bool containsNode =
                    pair.Value.Any(
                        x => string.Equals(
                            x,
                            node,
                            StringComparison.OrdinalIgnoreCase));

                if (!containsNode)
                {
                    continue;
                }

                if (upperIndex.TryGetValue(
                        pair.Key,
                        out int index))
                {
                    indexes.Add(index);
                }
            }

            return CalculateMedian(
                indexes);
        }

        // ============================================================
        // OUTGOING MEDIAN
        // ============================================================

        private static double CalculateOutgoingMedian(
            RootLayoutContext context,
            string node,
            Dictionary<string, int> lowerIndex)
        {
            // ========================================================
            // QUAN TRỌNG:
            //
            // Dùng EffectiveAdjacency để dummy chain được nhìn thấy.
            // ========================================================

            if (!context.EffectiveAdjacency.TryGetValue(
                    node,
                    out List<string> targets))
            {
                return double.MaxValue;
            }

            if (targets == null)
            {
                return double.MaxValue;
            }

            List<int> indexes =
                new List<int>();

            foreach (string target in targets)
            {
                if (string.IsNullOrWhiteSpace(target))
                {
                    continue;
                }

                if (lowerIndex.TryGetValue(
                        target,
                        out int index))
                {
                    indexes.Add(index);
                }
            }

            return CalculateMedian(
                indexes);
        }

        // ============================================================
        // MEDIAN
        // ============================================================

        private static double CalculateMedian(
            List<int> values)
        {
            if (values == null ||
                values.Count == 0)
            {
                return double.MaxValue;
            }

            values.Sort();

            int middle =
                values.Count / 2;

            if (values.Count % 2 == 1)
            {
                return values[middle];
            }

            return
                (values[middle - 1] +
                 values[middle]) /
                2.0;
        }

        // ============================================================
        // CROSSINGS
        // ============================================================

        private static int CountCrossings(
            RootLayoutContext context,
            Dictionary<int, List<string>> layers)
        {
            int crossings = 0;

            List<int> ordered =
                layers.Keys
                    .OrderBy(x => x)
                    .ToList();

            for (int i = 0;
                 i < ordered.Count - 1;
                 i++)
            {
                List<string> upper =
                    layers[ordered[i]];

                List<string> lower =
                    layers[ordered[i + 1]];

                crossings +=
                    CountLayerCrossings(
                        context,
                        upper,
                        lower);
            }

            return crossings;
        }

        // ============================================================
        // CROSSINGS BETWEEN TWO ADJACENT LAYERS
        // ============================================================

        private static int CountLayerCrossings(
            RootLayoutContext context,
            List<string> upper,
            List<string> lower)
        {
            Dictionary<string, int> lowerIndex =
                BuildIndex(lower);

            List<Tuple<int, int>> edges =
                new List<Tuple<int, int>>();

            // ========================================================
            // QUAN TRỌNG:
            //
            // EffectiveAdjacency chứa:
            //
            //     A -> dummy
            //     dummy -> dummy
            //     dummy -> D
            //
            // thay vì:
            //
            //     A -> D
            //
            // Nhờ vậy crossing được tính theo từng layer liên tiếp.
            // ========================================================

            for (int upperIndex = 0;
                 upperIndex < upper.Count;
                 upperIndex++)
            {
                string source =
                    upper[upperIndex];

                if (!context.EffectiveAdjacency.TryGetValue(
                        source,
                        out List<string> targets))
                {
                    continue;
                }

                if (targets == null)
                {
                    continue;
                }

                foreach (string target in targets)
                {
                    if (string.IsNullOrWhiteSpace(target))
                    {
                        continue;
                    }

                    if (!lowerIndex.TryGetValue(
                            target,
                            out int targetIndex))
                    {
                        continue;
                    }

                    edges.Add(
                        Tuple.Create(
                            upperIndex,
                            targetIndex));
                }
            }

            // ========================================================
            // COUNT INVERSION
            // ========================================================

            int result = 0;

            for (int i = 0;
                 i < edges.Count;
                 i++)
            {
                for (int j = i + 1;
                     j < edges.Count;
                     j++)
                {
                    Tuple<int, int> a =
                        edges[i];

                    Tuple<int, int> b =
                        edges[j];

                    if ((a.Item1 < b.Item1 &&
                         a.Item2 > b.Item2)
                        ||
                        (a.Item1 > b.Item1 &&
                         a.Item2 < b.Item2))
                    {
                        result++;
                    }
                }
            }

            return result;
        }

        // ============================================================
        // INDEX
        // ============================================================

        private static Dictionary<string, int> BuildIndex(
            List<string> nodes)
        {
            Dictionary<string, int> result =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            if (nodes == null)
            {
                return result;
            }

            for (int i = 0;
                 i < nodes.Count;
                 i++)
            {
                string node =
                    nodes[i];

                if (string.IsNullOrWhiteSpace(node))
                {
                    continue;
                }

                result[node] =
                    i;
            }

            return result;
        }
    }
}