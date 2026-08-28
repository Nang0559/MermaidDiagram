using MermaidDiagram.Core.Layout.Result;
using System;
using System.Collections.Generic;


    namespace MermaidDiagram.Core.Layout.Layered
    {
        /// <summary>
        /// Chèn dummy node (virtual node) cho các cạnh vượt quá
        /// một layer theo mô hình Sugiyama.
        ///
        /// Ví dụ:
        ///
        ///     A (L1) ----------------> D (L4)
        ///
        /// được chuyển thành:
        ///
        ///     A (L1)
        ///        |
        ///     dummy (L2)
        ///        |
        ///     dummy (L3)
        ///        |
        ///     D (L4)
        ///
        /// Dummy node:
        ///     - Không phải node Mermaid thật.
        ///     - Không được renderer hiển thị.
        ///     - Có tọa độ để tham gia ordering / crossing / collision.
        ///     - Được EdgeRoutingEngine sử dụng làm waypoint.
        /// </summary>
        public sealed class DummyNodeInserter
        {
            private const float DummySize = 2f;

            // ============================================================
            // PUBLIC
            // ============================================================

            public void Insert(
                RootLayoutContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(
                        nameof(context));
                }

                if (context.Graph == null)
                {
                    throw new InvalidOperationException(
                        "RootLayoutContext.Graph is null.");
                }

                // --------------------------------------------------------
                // RESET
                // --------------------------------------------------------

                context.EffectiveAdjacency.Clear();
                context.EdgeDummyChain.Clear();

                RemovePreviousDummyNodes(
                    context);

                // --------------------------------------------------------
                // COPY ORIGINAL GRAPH
                // --------------------------------------------------------

                CopyOriginalAdjacency(
                    context);

                // --------------------------------------------------------
                // COLLECT ORIGINAL EDGES
                // --------------------------------------------------------

                List<Tuple<string, string>> edges =
                    CollectOriginalEdges(
                        context);

                // --------------------------------------------------------
                // SPLIT LONG EDGES
                // --------------------------------------------------------

                foreach (
                    Tuple<string, string> edge
                    in edges)
                {
                    ProcessEdge(
                        context,
                        edge.Item1,
                        edge.Item2);
                }
            }

            // ============================================================
            // REMOVE PREVIOUS DUMMIES
            // ============================================================

            private static void RemovePreviousDummyNodes(
                RootLayoutContext context)
            {
                List<string> dummyKeys =
                    context.Items
                        .Where(x =>
                            x.Value != null &&
                            x.Value.IsDummy)
                        .Select(x => x.Key)
                        .ToList();

                foreach (string key in dummyKeys)
                {
                    context.Items.Remove(key);
                    context.Layer.Remove(key);
                }

                foreach (
                    List<string> nodes
                    in context.Layers.Values)
                {
                    nodes.RemoveAll(
                        x => dummyKeys.Contains(
                            x,
                            StringComparer.OrdinalIgnoreCase));
                }

                foreach (
                    int layer in context.Layers.Keys.ToList())
                {
                    if (context.Layers[layer] == null ||
                        context.Layers[layer].Count == 0)
                    {
                        context.Layers.Remove(layer);
                    }
                }
            }

            // ============================================================
            // COPY ORIGINAL ADJACENCY
            // ============================================================

            private static void CopyOriginalAdjacency(
                RootLayoutContext context)
            {
                foreach (
                    KeyValuePair<string, List<string>> pair
                    in context.Graph.Adjacency)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                    {
                        continue;
                    }

                    List<string> targets =
                        pair.Value == null
                            ? new List<string>()
                            : pair.Value
                                .Where(
                                    x => !string.IsNullOrWhiteSpace(x))
                                .Distinct(
                                    StringComparer.OrdinalIgnoreCase)
                                .ToList();

                    context.EffectiveAdjacency[pair.Key] =
                        targets;
                }
            }

            // ============================================================
            // COLLECT EDGES
            // ============================================================

            private static List<Tuple<string, string>>
                CollectOriginalEdges(
                    RootLayoutContext context)
            {
                List<Tuple<string, string>> result =
                    new List<Tuple<string, string>>();

                foreach (
                    KeyValuePair<string, List<string>> pair
                    in context.Graph.Adjacency)
                {
                    string source =
                        pair.Key;

                    if (string.IsNullOrWhiteSpace(source) ||
                        pair.Value == null)
                    {
                        continue;
                    }

                    foreach (string target in pair.Value)
                    {
                        if (string.IsNullOrWhiteSpace(target))
                        {
                            continue;
                        }

                        result.Add(
                            Tuple.Create(
                                source,
                                target));
                    }
                }

                return result;
            }

            // ============================================================
            // PROCESS EDGE
            // ============================================================

            private static void ProcessEdge(
                RootLayoutContext context,
                string source,
                string target)
            {
                if (!context.Layer.TryGetValue(
                        source,
                        out int sourceLayer))
                {
                    return;
                }

                if (!context.Layer.TryGetValue(
                        target,
                        out int targetLayer))
                {
                    return;
                }

                // --------------------------------------------------------
                // Chỉ xử lý cạnh đi xuống nhiều layer.
                // --------------------------------------------------------

                int gap =
                    targetLayer - sourceLayer;

                if (gap <= 1)
                {
                    return;
                }

                string edgeKey =
                    CreateEdgeKey(
                        source,
                        target);

                if (context.EdgeDummyChain.ContainsKey(
                        edgeKey))
                {
                    return;
                }

                // --------------------------------------------------------
                // REMOVE ORIGINAL LONG EDGE
                // --------------------------------------------------------

                RemoveEffectiveEdge(
                    context,
                    source,
                    target);

                // --------------------------------------------------------
                // CREATE DUMMY CHAIN
                // --------------------------------------------------------

                List<string> dummyChain =
                    new List<string>();

                string previous =
                    source;

                for (
                    int layer = sourceLayer + 1;
                    layer < targetLayer;
                    layer++)
                {
                    string dummyKey =
                        CreateDummyKey(
                            source,
                            target,
                            layer);

                    EnsureDummyItem(
                        context,
                        dummyKey,
                        layer);

                    AppendEffectiveEdge(
                        context,
                        previous,
                        dummyKey);

                    dummyChain.Add(
                        dummyKey);

                    previous =
                        dummyKey;
                }

                // --------------------------------------------------------
                // LAST DUMMY -> TARGET
                // --------------------------------------------------------

                AppendEffectiveEdge(
                    context,
                    previous,
                    target);

                context.EdgeDummyChain[edgeKey] =
                    dummyChain;
            }

            // ============================================================
            // REMOVE EDGE
            // ============================================================

            private static void RemoveEffectiveEdge(
                RootLayoutContext context,
                string source,
                string target)
            {
                if (!context.EffectiveAdjacency.TryGetValue(
                        source,
                        out List<string> targets))
                {
                    return;
                }

                targets.RemoveAll(
                    x => string.Equals(
                        x,
                        target,
                        StringComparison.OrdinalIgnoreCase));
            }

            // ============================================================
            // ENSURE DUMMY
            // ============================================================

            private static void EnsureDummyItem(
                RootLayoutContext context,
                string dummyKey,
                int layer)
            {
                if (!context.Items.TryGetValue(
                        dummyKey,
                        out MermaidLayoutItem dummyItem))
                {
                    dummyItem =
                        new MermaidLayoutItem(
                            dummyKey,
                            string.Empty,
                            false,
                            DummySize,
                            DummySize);

                    dummyItem.IsDummy =
                        true;

                    context.Items[dummyKey] =
                        dummyItem;
                }
                else
                {
                    dummyItem.IsDummy =
                        true;
                }

                // --------------------------------------------------------
                // LAYER MAP
                // --------------------------------------------------------

                context.Layer[dummyKey] =
                    layer;

                // --------------------------------------------------------
                // LAYER BUCKET
                // --------------------------------------------------------

                if (!context.Layers.TryGetValue(
                        layer,
                        out List<string> nodes))
                {
                    nodes =
                        new List<string>();

                    context.Layers[layer] =
                        nodes;
                }

                if (!nodes.Contains(
                        dummyKey,
                        StringComparer.OrdinalIgnoreCase))
                {
                    nodes.Add(
                        dummyKey);
                }
            }

            // ============================================================
            // APPEND EFFECTIVE EDGE
            // ============================================================

            private static void AppendEffectiveEdge(
                RootLayoutContext context,
                string source,
                string target)
            {
                if (!context.EffectiveAdjacency.TryGetValue(
                        source,
                        out List<string> targets))
                {
                    targets =
                        new List<string>();

                    context.EffectiveAdjacency[source] =
                        targets;
                }

                if (!targets.Contains(
                        target,
                        StringComparer.OrdinalIgnoreCase))
                {
                    targets.Add(
                        target);
                }
            }

            // ============================================================
            // EDGE KEY
            // ============================================================

            private static string CreateEdgeKey(
                string source,
                string target)
            {
                return source +
                       "->" +
                       target;
            }

            // ============================================================
            // DUMMY KEY
            // ============================================================

            private static string CreateDummyKey(
                string source,
                string target,
                int layer)
            {
                return "__dummy__" +
                       source +
                       "__" +
                       target +
                       "__L" +
                       layer;
            }
        }
    }
