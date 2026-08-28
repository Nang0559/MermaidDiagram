using MermaidDiagram.Core.Layout.Contract;
using MermaidDiagram.Core.Layout.Result;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting
{
    /// <summary>
    /// Tính routing path cho toàn bộ Mermaid edge.
    ///
    /// EdgeRoutingEngine CHỈ xử lý connector.
    ///
    /// Không chịu trách nhiệm:
    ///     - phân tích graph
    ///     - tìm MainPath
    ///     - layout node
    ///     - collision node
    ///     - viewport
    ///     - scale
    ///     - DevExpress
    ///
    /// Input:
    ///     RootLayoutContext.Items
    ///     MermaidGraph.Adjacency
    ///
    /// Output:
    ///     RootLayoutContext.EdgeRoutes
    ///
    /// Chiến lược (ĐÃ ĐƠN GIẢN HOÁ):
    ///
    /// Trong kiến trúc layered (Layer -> Ordering -> Crossing
    /// Minimization -> Coordinate), việc giảm crossing/tránh đè
    /// đã được giải quyết ở bước layout. EdgeRoutingEngine KHÔNG
    /// còn cần lane-stacking hay obstacle-scanning nữa — chỉ cần
    /// chọn cổng theo hướng hình học thực tế (dx/dy) và nối một
    /// đoạn gãy nhẹ nếu lệch trục.
    /// </summary>
    public sealed class EdgeRoutingEngine
    {
        private readonly EdgeRoutingOptions _options;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public EdgeRoutingEngine(
            EdgeRoutingOptions options)
        {
            _options =
                options
                ?? throw new ArgumentNullException(
                    nameof(options));

            _options.Validate();
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        public void Route(
            RootLayoutContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            context.EdgeRoutes.Clear();

            if (!_options.Enabled)
            {
                return;
            }

            if (context.Graph == null ||
                context.Graph.NodeKeys.Count == 0)
            {
                return;
            }

            List<EdgeDescriptor> edges =
                CollectEdges(context);

            foreach (EdgeDescriptor edge
                     in edges)
            {
                EdgeRoute route =
                    RouteEdge(
                        context,
                        edge);

                string key =
                    CreateEdgeKey(
                        edge.Source,
                        edge.Target);

                /*
                 * EdgeRoutes là Dictionary<string, EdgeRoute>.
                 *
                 * KHÔNG dùng:
                 *
                 *     EdgeRoutes.Add(route);
                 */
                context.EdgeRoutes[key] =
                    route;
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
        // COLLECT EDGES
        // ============================================================

        private static List<EdgeDescriptor> CollectEdges(
            RootLayoutContext context)
        {
            List<EdgeDescriptor> result =
                new List<EdgeDescriptor>();

            foreach (KeyValuePair<
                        string,
                         List<string> > pair
                     in context.Graph.Adjacency)
            {
                string source =
                    pair.Key;

                List<string> targets =
                    pair.Value;

                if (targets == null)
                {
                    continue;
                }

                foreach (string target
                         in targets)
                {
                    if (string.IsNullOrWhiteSpace(
                            target))
                    {
                        continue;
                    }

                    if (!context.Items.ContainsKey(
                            source))
                    {
                        continue;
                    }

                    if (!context.Items.ContainsKey(
                            target))
                    {
                        continue;
                    }

                    result.Add(
                        new EdgeDescriptor(
                            source,
                            target));
                }
            }

            /*
             * Deterministic ordering.
             */
            result =
                result
                    .OrderBy(
                        e => GetMainPathIndex(
                            context,
                            e.Source))
                    .ThenBy(
                        e => GetMainPathIndex(
                            context,
                            e.Target))
                    .ThenBy(
                        e => e.Source,
                        StringComparer.OrdinalIgnoreCase)
                    .ThenBy(
                        e => e.Target,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            return result;
        }

        // ============================================================
        // ROUTE EDGE — ĐƠN GIẢN HOÁ
        // ============================================================
        //
        // Không còn phân loại MainPath/Branch/Return/lane nữa.
        // Chỉ dựa vào dx/dy thực tế giữa source và target để chọn
        // cổng và vẽ 1 đoạn gãy nhẹ nếu 2 tâm không thẳng hàng.
        //

        private EdgeRoute RouteEdge(
      RootLayoutContext context,
      EdgeDescriptor edge)
        {
            MermaidLayoutItem sourceItem =
                GetItem(
                    context,
                    edge.Source);

            MermaidLayoutItem targetItem =
                GetItem(
                    context,
                    edge.Target);

            EdgeRoute route =
                new EdgeRoute(
                    edge.Source,
                    edge.Target);

            // ============================================================
            // DUMMY CHAIN
            // ============================================================
            //
            // Edge gốc:
            //
            //     A --------------------------> D
            //
            // Nếu:
            //
            //     A(L1) -> D(L4)
            //
            // DummyNodeInserter đã tạo:
            //
            //     A -> dummy(L2) -> dummy(L3) -> D
            //
            // Route phải đi qua:
            //
            //     A -> dummy1 -> dummy2 -> D
            //
            // để không xuyên qua node thật ở layer trung gian.
            //

            List<string> dummyChain =
                GetDummyChain(
                    context,
                    edge.Source,
                    edge.Target);

            if (dummyChain.Count == 0)
            {
                // --------------------------------------------------------
                // EDGE BÌNH THƯỜNG
                // --------------------------------------------------------

                ItemBounds source =
                    GetBounds(sourceItem);

                ItemBounds target =
                    GetBounds(targetItem);

                RouteDirectEdge(
                    source,
                    target,
                    route);

                InsertPortStubs(
                    route,
                    source,
                    target);

                CalculateLabelPosition(
                    route);

                return route;
            }

            // ============================================================
            // EDGE CÓ DUMMY
            // ============================================================

            RouteThroughDummyChain(
                context,
                edge,
                dummyChain,
                route);

            CalculateLabelPosition(
                route);

            return route;
        }
        private static List<string> GetDummyChain(
    RootLayoutContext context,
    string source,
    string target)
        {
            string edgeKey =
                CreateEdgeKey(
                    source,
                    target);

            if (!context.EdgeDummyChain.TryGetValue(
                    edgeKey,
                    out List<string>? chain))
            {
                return new List<string>();
            }

            if (chain == null ||
                chain.Count == 0)
            {
                return new List<string>();
            }

            return chain;
        }
        private static void RouteDirectEdge(
    ItemBounds source,
    ItemBounds target,
    EdgeRoute route)
        {
            float dx =
                target.CenterX -
                source.CenterX;

            float dy =
                target.CenterY -
                source.CenterY;

            if (Math.Abs(dy) >=
                Math.Abs(dx))
            {
                RouteVertical(
                    source,
                    target,
                    route,
                    dy >= 0f);
            }
            else
            {
                RouteHorizontal(
                    source,
                    target,
                    route,
                    dx >= 0f);
            }
        }
        private  void RouteThroughDummyChain(
    RootLayoutContext context,
    EdgeDescriptor edge,
    List<string> dummyChain,
    EdgeRoute route)
        {
            // ============================================================
            // TẠO CHUỖI NODE:
            //
            // source
            // dummy1
            // dummy2
            // ...
            // target
            // ============================================================

            List<string> chain =
                new List<string>();

            chain.Add(
                edge.Source);

            chain.AddRange(
                dummyChain);

            chain.Add(
                edge.Target);

            if (chain.Count < 2)
            {
                return;
            }

            // ============================================================
            // SOURCE / TARGET
            // ============================================================

            MermaidLayoutItem sourceItem =
                GetItem(
                    context,
                    edge.Source);

            MermaidLayoutItem targetItem =
                GetItem(
                    context,
                    edge.Target);

            ItemBounds source =
                GetBounds(sourceItem);

            ItemBounds target =
                GetBounds(targetItem);

            // ============================================================
            // XÁC ĐỊNH HƯỚNG CHUNG
            // ============================================================

            bool downward =
                target.CenterY >=
                source.CenterY;

            route.SourcePort =
                downward
                    ? EdgePort.Bottom
                    : EdgePort.Top;

            route.TargetPort =
                downward
                    ? EdgePort.Top
                    : EdgePort.Bottom;

            // ============================================================
            // SOURCE PORT
            // ============================================================

            AddPoint(
                route,
                source.CenterX,
                downward
                    ? source.Bottom
                    : source.Top);

            // ============================================================
            // DUMMY WAYPOINTS
            // ============================================================
            //
            // Không nối source -> target trực tiếp.
            //
            // Mỗi dummy là một waypoint.
            //

            for (int i = 1;
                 i < chain.Count - 1;
                 i++)
            {
                string key =
                    chain[i];

                MermaidLayoutItem dummyItem =
                    GetItem(
                        context,
                        key);

                ItemBounds dummy =
                    GetBounds(dummyItem);

                AddPoint(
                    route,
                    dummy.CenterX,
                    dummy.CenterY);
            }

            // ============================================================
            // TARGET PORT
            // ============================================================

            AddPoint(
                route,
                target.CenterX,
                downward
                    ? target.Top
                    : target.Bottom);

            // ============================================================
            // PORT STUBS
            // ============================================================

            /*
             * Dummy route đã có waypoint.
             *
             * Chỉ thêm stub ở source/target thật.
             */

            InsertPortStubs(
                route,
                source,
                target);
        }
        // ============================================================
        // VERTICAL (trục dọc chiếm ưu thế)
        // ============================================================

        private static void RouteVertical(
            ItemBounds source,
            ItemBounds target,
            EdgeRoute route,
            bool targetBelow)
        {
            if (targetBelow)
            {
                route.SourcePort =
                    EdgePort.Bottom;

                route.TargetPort =
                    EdgePort.Top;

                AddPoint(
                    route,
                    source.CenterX,
                    source.Bottom);

                if (!NearlyEqual(
                        source.CenterX,
                        target.CenterX))
                {
                    float midY =
                        (source.Bottom +
                         target.Top) /
                        2f;

                    AddPoint(
                        route,
                        source.CenterX,
                        midY);

                    AddPoint(
                        route,
                        target.CenterX,
                        midY);
                }

                AddPoint(
                    route,
                    target.CenterX,
                    target.Top);

                return;
            }

            route.SourcePort =
                EdgePort.Top;

            route.TargetPort =
                EdgePort.Bottom;

            AddPoint(
                route,
                source.CenterX,
                source.Top);

            if (!NearlyEqual(
                    source.CenterX,
                    target.CenterX))
            {
                float midY =
                    (source.Top +
                     target.Bottom) /
                    2f;

                AddPoint(
                    route,
                    source.CenterX,
                    midY);

                AddPoint(
                    route,
                    target.CenterX,
                    midY);
            }

            AddPoint(
                route,
                target.CenterX,
                target.Bottom);
        }

        // ============================================================
        // HORIZONTAL (trục ngang chiếm ưu thế — branch/convergence)
        // ============================================================

        private static void RouteHorizontal(
            ItemBounds source,
            ItemBounds target,
            EdgeRoute route,
            bool targetRight)
        {
            if (targetRight)
            {
                route.SourcePort =
                    EdgePort.Right;

                route.TargetPort =
                    EdgePort.Left;

                AddPoint(
                    route,
                    source.Right,
                    source.CenterY);

                if (!NearlyEqual(
                        source.CenterY,
                        target.CenterY))
                {
                    float midX =
                        (source.Right +
                         target.Left) /
                        2f;

                    AddPoint(
                        route,
                        midX,
                        source.CenterY);

                    AddPoint(
                        route,
                        midX,
                        target.CenterY);
                }

                AddPoint(
                    route,
                    target.Left,
                    target.CenterY);

                return;
            }

            route.SourcePort =
                EdgePort.Left;

            route.TargetPort =
                EdgePort.Right;

            AddPoint(
                route,
                source.Left,
                source.CenterY);

            if (!NearlyEqual(
                    source.CenterY,
                    target.CenterY))
            {
                float midX =
                    (source.Left +
                     target.Right) /
                    2f;

                AddPoint(
                    route,
                    midX,
                    source.CenterY);

                AddPoint(
                    route,
                    midX,
                    target.CenterY);
            }

            AddPoint(
                route,
                target.Right,
                target.CenterY);
        }

        // ============================================================
        // LABEL
        // ============================================================

        private static void CalculateLabelPosition(
            EdgeRoute route)
        {
            if (route.Points.Count == 0)
            {
                route.LabelX = 0f;
                route.LabelY = 0f;

                return;
            }

            if (route.Points.Count == 1)
            {
                route.LabelX =
                    route.Points[0].X;

                route.LabelY =
                    route.Points[0].Y;

                return;
            }

            float longest =
                -1f;

            EdgeRoutePoint bestA =
                route.Points[0];

            EdgeRoutePoint bestB =
                route.Points[1];

            for (int i = 0;
                 i < route.Points.Count - 1;
                 i++)
            {
                EdgeRoutePoint a =
                    route.Points[i];

                EdgeRoutePoint b =
                    route.Points[i + 1];

                float length =
                    Math.Abs(
                        b.X - a.X)
                    +
                    Math.Abs(
                        b.Y - a.Y);

                if (length > longest)
                {
                    longest =
                        length;

                    bestA =
                        a;

                    bestB =
                        b;
                }
            }

            route.LabelX =
                (bestA.X +
                 bestB.X) /
                2f;

            route.LabelY =
                (bestA.Y +
                 bestB.Y) /
                2f;
        }

        // ============================================================
        // BOUNDS
        // ============================================================

        private static ItemBounds GetBounds(
            MermaidLayoutItem item)
        {
            return new ItemBounds(
                item.X,
                item.Y,
                item.Width,
                item.Height);
        }

        // ============================================================
        // ITEM
        // ============================================================

        private static MermaidLayoutItem GetItem(
            RootLayoutContext context,
            string key)
        {
            if (!context.Items.TryGetValue(
                    key,
                    out MermaidLayoutItem? item))
            {
                throw new InvalidOperationException(
                    $"Edge routing item '{key}' " +
                    "was not found.");
            }

            return item;
        }

        // ============================================================
        // MAIN PATH INDEX
        // ============================================================

        private static int GetMainPathIndex(
            RootLayoutContext context,
            string key)
        {
            if (context.MainPathIndex.TryGetValue(
                    key,
                    out int index))
            {
                return index;
            }

            return int.MaxValue;
        }

        // ============================================================
        // POINT
        // ============================================================

        private static void AddPoint(
            EdgeRoute route,
            float x,
            float y)
        {
            if (route.Points.Count > 0)
            {
                EdgeRoutePoint previous =
                    route.Points[
                        route.Points.Count - 1];

                if (NearlyEqual(
                        previous.X,
                        x)
                    &&
                    NearlyEqual(
                        previous.Y,
                        y))
                {
                    return;
                }
            }

            route.Points.Add(
                new EdgeRoutePoint(
                    x,
                    y));
        }

        private static bool NearlyEqual(
            float a,
            float b)
        {
            return Math.Abs(a - b) < 0.01f;
        }

        // ============================================================
        // INTERNAL DESCRIPTOR
        // ============================================================

        private sealed class EdgeDescriptor
        {
            public string Source { get; }

            public string Target { get; }

            public EdgeDescriptor(
                string source,
                string target)
            {
                Source =
                    source;

                Target =
                    target;
            }
        }

        // ============================================================
        // PORT STUB — chống curve bo lấn vào shape
        // ============================================================

        private void InsertPortStubs(
            EdgeRoute route,
            ItemBounds source,
            ItemBounds target)
        {
            float offset =
                _options.PortOffset;

            if (offset <= 0f ||
                route.Points.Count < 2)
            {
                return;
            }

            // --- đầu route (rời source) ---

            EdgeRoutePoint firstPoint =
                route.Points[0];

            EdgeRoutePoint stubStart =
                GetStubPoint(
                    firstPoint,
                    route.SourcePort,
                    offset);

            EdgeRoutePoint nextPoint =
                route.Points[1];

            if (!NearlyEqual(stubStart.X, nextPoint.X) ||
                !NearlyEqual(stubStart.Y, nextPoint.Y))
            {
                route.Points.Insert(1, stubStart);
            }

            // --- cuối route (vào target) ---

            int lastIndex =
                route.Points.Count - 1;

            EdgeRoutePoint lastPoint =
                route.Points[lastIndex];

            EdgeRoutePoint stubEnd =
                GetStubPoint(
                    lastPoint,
                    route.TargetPort,
                    offset);

            EdgeRoutePoint prevPoint =
                route.Points[lastIndex - 1];

            if (!NearlyEqual(stubEnd.X, prevPoint.X) ||
                !NearlyEqual(stubEnd.Y, prevPoint.Y))
            {
                route.Points.Insert(lastIndex, stubEnd);
            }
        }

        private static EdgeRoutePoint GetStubPoint(
            EdgeRoutePoint portPoint,
            EdgePort port,
            float offset)
        {
            switch (port)
            {
                case EdgePort.Top:
                    return new EdgeRoutePoint(portPoint.X, portPoint.Y - offset);

                case EdgePort.Bottom:
                    return new EdgeRoutePoint(portPoint.X, portPoint.Y + offset);

                case EdgePort.Left:
                    return new EdgeRoutePoint(portPoint.X - offset, portPoint.Y);

                case EdgePort.Right:
                    return new EdgeRoutePoint(portPoint.X + offset, portPoint.Y);

                default:
                    return portPoint;
            }
        }
    }

    // ================================================================
    // ITEM BOUNDS
    // ================================================================

    internal readonly struct ItemBounds
    {
        public float Left { get; }

        public float Top { get; }

        public float Right { get; }

        public float Bottom { get; }

        public float CenterX { get; }

        public float CenterY { get; }

        public ItemBounds(
            float x,
            float y,
            float width,
            float height)
        {
            Left =
                x;

            Top =
                y;

            Right =
                x + width;

            Bottom =
                y + height;

            CenterX =
                x + width / 2f;

            CenterY =
                y + height / 2f;
        }
    }
}