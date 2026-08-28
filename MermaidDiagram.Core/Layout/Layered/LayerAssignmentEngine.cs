using MermaidDiagram.Core.Graph;
using MermaidDiagram.Core.Layout.CoreLayout;
using MermaidDiagram.Core.Layout.Result;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MermaidDiagram.Core.Layout.Layered
{
    /// <summary>
    /// Gán layer cho toàn bộ graph.
    ///
    /// Layer:
    ///
    ///     0
    ///     1
    ///     2
    ///     3
    ///
    /// Không phụ thuộc MainPath.
    ///
    /// Mục tiêu:
    ///     - toàn graph dùng chung hệ layer
    ///     - branch có thể nằm cùng layer với node phù hợp
    ///     - convergence node được đặt vào layer hợp lý
    /// </summary>
    public sealed class LayerAssignmentEngine
    {
        public MermaidGraphLayer Assign(
            RootLayoutContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            MermaidGraphLayer result =
                new MermaidGraphLayer();

            if (context.Graph == null ||
                context.Graph.NodeKeys == null ||
                context.Graph.NodeKeys.Count == 0)
            {
                return result;
            }

            List<string> nodes =
                context.Graph.NodeKeys
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(
                        x => x,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            Dictionary<string, int> indegree =
                BuildIndegree(
                    context,
                    nodes);

            Queue<string> queue =
                new Queue<string>(
                    nodes.Where(
                        x => indegree[x] == 0));

            HashSet<string> processed =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            /*
             * Layer của node.
             */
            foreach (string node in nodes)
            {
                result.Layer[node] = 0;
            }

            /*
             * Topological traversal.
             *
             * Nếu graph có cycle:
             * cycle edge sẽ không làm traversal vô hạn.
             */
            while (queue.Count > 0)
            {
                string current =
                    queue.Dequeue();

                if (!processed.Add(current))
                {
                    continue;
                }

                int currentLayer =
                    result.Layer[current];

                foreach (string target
                         in GetTargets(
                             context,
                             current))
                {
                    if (!result.Layer.ContainsKey(target))
                    {
                        result.Layer[target] =
                            currentLayer + 1;
                    }
                    else
                    {
                        result.Layer[target] =
                            Math.Max(
                                result.Layer[target],
                                currentLayer + 1);
                    }

                    if (indegree.ContainsKey(target))
                    {
                        indegree[target]--;

                        if (indegree[target] <= 0)
                        {
                            queue.Enqueue(target);
                        }
                    }
                }
            }

            /*
             * Graph có cycle hoặc disconnected component.
             *
             * Đảm bảo mọi node đều có layer.
             */
            AssignRemainingNodes(
                context,
                result,
                nodes,
                processed);

            /*
             * Build reverse mapping.
             */
            foreach (KeyValuePair<string, int> pair
                     in result.Layer)
            {
                if (!result.Layers.TryGetValue(
                        pair.Value,
                        out List<string> layer))
                {
                    layer =
                        new List<string>();

                    result.Layers.Add(
                        pair.Value,
                        layer);
                }

                layer.Add(
                    pair.Key);
            }

            foreach (List<string> layer
                     in result.Layers.Values)
            {
                layer.Sort(
                    StringComparer.OrdinalIgnoreCase);
            }

            return result;
        }

        // ============================================================
        // INDEGREE
        // ============================================================

        private static Dictionary<string, int> BuildIndegree(
            RootLayoutContext context,
            List<string> nodes)
        {
            Dictionary<string, int> result =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (string node in nodes)
            {
                result[node] = 0;
            }

            foreach (string source in nodes)
            {
                foreach (string target
                         in GetTargets(
                             context,
                             source))
                {
                    if (!result.ContainsKey(target))
                    {
                        continue;
                    }

                    result[target]++;
                }
            }

            return result;
        }

        // ============================================================
        // REMAINING
        // ============================================================

        private static void AssignRemainingNodes(
            RootLayoutContext context,
            MermaidGraphLayer result,
            List<string> nodes,
            HashSet<string> processed)
        {
            /*
             * Node chưa processed:
             *
             * - cycle
             * - disconnected
             *
             * Ta không để chúng bị bỏ layer.
             */
            foreach (string node in nodes)
            {
                if (processed.Contains(node))
                {
                    continue;
                }

                int layer =
                    CalculateCycleSafeLayer(
                        context,
                        result,
                        node);

                result.Layer[node] =
                    layer;

                processed.Add(node);
            }
        }

        // ============================================================
        // CYCLE SAFE LAYER
        // ============================================================

        private static int CalculateCycleSafeLayer(
            RootLayoutContext context,
            MermaidGraphLayer result,
            string node)
        {
            int best =
                0;

            foreach (KeyValuePair<
                         string,
                         List<string>> pair
                     in context.Graph.Adjacency)
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

                if (result.Layer.TryGetValue(
                        pair.Key,
                        out int parentLayer))
                {
                    best =
                        Math.Max(
                            best,
                            parentLayer + 1);
                }
            }

            return best;
        }

        // ============================================================
        // TARGETS
        // ============================================================

        private static IEnumerable<string> GetTargets(
            RootLayoutContext context,
            string source)
        {
            if (!context.Graph.Adjacency.TryGetValue(
                    source,
                    out List<string> targets))
            {
                yield break;
            }

            if (targets == null)
            {
                yield break;
            }

            foreach (string target in targets)
            {
                if (string.IsNullOrWhiteSpace(target))
                {
                    continue;
                }

                if (!context.Items.ContainsKey(target))
                {
                    continue;
                }

                yield return target;
            }
        }
    }
}