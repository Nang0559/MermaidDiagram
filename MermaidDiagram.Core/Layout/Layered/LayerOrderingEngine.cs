using MermaidDiagram.Core.Layout.CoreLayout;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MermaidDiagram.Core.Layout.Layered
{
    /// <summary>
    /// Sap xep node trong tung layer.
    ///
    /// Khong tinh X/Y.
    ///
    /// Doc context.EffectiveAdjacency (khong phai Graph.Adjacency
    /// truc tiep) de dummy node (tu DummyNodeInserter) duoc coi
    /// nhu node that khi tinh barycenter.
    /// </summary>
    public sealed class LayerOrderingEngine
    {
        public void Order(
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

            List<int> orderedLayers =
                layers.Keys
                    .OrderBy(x => x)
                    .ToList();

            /*
             * Initial deterministic order.
             */
            foreach (int layerIndex
                     in orderedLayers)
            {
                layers[layerIndex] =
                    layers[layerIndex]
                        .Distinct(
                            StringComparer.OrdinalIgnoreCase)
                        .OrderBy(
                            x => x,
                            StringComparer.OrdinalIgnoreCase)
                        .ToList();
            }

            /*
             * Downward sweep.
             */
            for (int i = 1;
                 i < orderedLayers.Count;
                 i++)
            {
                int layerIndex =
                    orderedLayers[i];

                int previousLayer =
                    orderedLayers[i - 1];

                ReorderUsingPreviousLayer(
                    context,
                    layers[layerIndex],
                    layers[previousLayer]);
            }

            /*
             * Upward sweep.
             */
            for (int i = orderedLayers.Count - 2;
                 i >= 0;
                 i--)
            {
                int layerIndex =
                    orderedLayers[i];

                int nextLayer =
                    orderedLayers[i + 1];

                ReorderUsingNextLayer(
                    context,
                    layers[layerIndex],
                    layers[nextLayer]);
            }
        }

        // ============================================================
        // PREVIOUS
        // ============================================================

        private static void ReorderUsingPreviousLayer(
            RootLayoutContext context,
            List<string> current,
            List<string> previous)
        {
            Dictionary<string, int> previousIndex =
                BuildIndex(previous);

            current.Sort(
                (a, b) =>
                {
                    double ca =
                        CalculateBarycenter(
                            context,
                            a,
                            previousIndex);

                    double cb =
                        CalculateBarycenter(
                            context,
                            b,
                            previousIndex);

                    int compare =
                        ca.CompareTo(cb);

                    if (compare != 0)
                    {
                        return compare;
                    }

                    return StringComparer.OrdinalIgnoreCase
                        .Compare(a, b);
                });
        }

        // ============================================================
        // NEXT
        // ============================================================

        private static void ReorderUsingNextLayer(
            RootLayoutContext context,
            List<string> current,
            List<string> next)
        {
            Dictionary<string, int> nextIndex =
                BuildIndex(next);

            current.Sort(
                (a, b) =>
                {
                    double ca =
                        CalculateReverseBarycenter(
                            context,
                            a,
                            nextIndex);

                    double cb =
                        CalculateReverseBarycenter(
                            context,
                            b,
                            nextIndex);

                    int compare =
                        ca.CompareTo(cb);

                    if (compare != 0)
                    {
                        return compare;
                    }

                    return StringComparer.OrdinalIgnoreCase
                        .Compare(a, b);
                });
        }

        // ============================================================
        // BARYCENTER
        // ============================================================

        private static double CalculateBarycenter(
            RootLayoutContext context,
            string node,
            Dictionary<string, int> previousIndex)
        {
            List<double> indexes =
                new List<double>();

            /*
             * MOI: doc EffectiveAdjacency thay vi Graph.Adjacency,
             * de dummy node cung tham gia tinh barycenter.
             */
            foreach (KeyValuePair<
                         string,
                         List<string>> pair
                     in context.EffectiveAdjacency)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                if (!pair.Value.Any(
                        x => string.Equals(
                            x,
                            node,
                            StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (previousIndex.TryGetValue(
                        pair.Key,
                        out int index))
                {
                    indexes.Add(index);
                }
            }

            if (indexes.Count == 0)
            {
                return double.MaxValue;
            }

            return indexes.Average();
        }

        // ============================================================
        // REVERSE BARYCENTER
        // ============================================================

        private static double CalculateReverseBarycenter(
     RootLayoutContext context,
     string node,
     Dictionary<string, int> nextIndex)
        {
            List<double> indexes =
                new List<double>();

            /*
             * MOI: doc EffectiveAdjacency thay vi Graph.Adjacency.
             */
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

            foreach (string target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                if (nextIndex.TryGetValue(
                        target,
                        out int index))
                {
                    indexes.Add(index);
                }
            }

            if (indexes.Count == 0)
            {
                return double.MaxValue;
            }

            return indexes.Average();
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

            for (int i = 0;
                 i < nodes.Count;
                 i++)
            {
                result[nodes[i]] = i;
            }

            return result;
        }
    }
}