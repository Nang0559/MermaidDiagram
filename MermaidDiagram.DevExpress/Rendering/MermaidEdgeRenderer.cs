using DevExpress.Diagram.Core;
using global::DevExpress.Utils;
using global::DevExpress.XtraDiagram;
using MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting;
using MermaidDiagram.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MermaidDiagram.DevExpress.Rendering
{
    /// <summary>
    /// Render MermaidEdge thành DiagramConnector.
    ///
    /// MermaidEdgeRenderer KHÔNG tính route.
    /// Nó chỉ:
    ///
    ///     EdgeRoute
    ///          ↓
    ///     DiagramConnector
    ///
    /// Core chịu trách nhiệm:
    ///     - node geometry
    ///     - port
    ///     - turn points
    ///     - routing
    ///
    /// Renderer chịu trách nhiệm:
    ///     - map Core -> DevExpress
    /// </summary>
    public sealed class MermaidEdgeRenderer
    {
        private readonly MermaidDiagramItemFactory _itemFactory;

        public MermaidEdgeRenderer(
            MermaidDiagramItemFactory itemFactory)
        {
            _itemFactory =
                itemFactory
                ?? throw new ArgumentNullException(
                    nameof(itemFactory));
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        public DiagramConnector Render(
            MermaidEdge edge,
            DiagramItem beginItem,
            DiagramItem endItem,
            EdgeRoute route)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            if (beginItem == null)
                throw new ArgumentNullException(nameof(beginItem));

            if (endItem == null)
                throw new ArgumentNullException(nameof(endItem));

            if (route == null)
                throw new ArgumentNullException(nameof(route));

            ValidateRoute(edge, route);

            DiagramConnector connector =
                _itemFactory.CreateEdge(
                    edge,
                    beginItem,
                    endItem);

            if (connector == null)
            {
                throw new InvalidOperationException(
                    "MermaidDiagramItemFactory.CreateEdge returned null.");
            }

            ApplyRoute(
                connector,
                route);

            return connector;
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private static void ValidateRoute(
            MermaidEdge edge,
            EdgeRoute route)
        {
            if (!string.Equals(
                    route.SourceKey,
                    edge.Source,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "EdgeRoute.SourceKey does not match MermaidEdge.Source.");
            }

            if (!string.Equals(
                    route.TargetKey,
                    edge.Target,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "EdgeRoute.TargetKey does not match MermaidEdge.Target.");
            }
        }

        // ============================================================
        // ROUTE
        // ============================================================

        private static void ApplyRoute(
        DiagramConnector connector,
        EdgeRoute route)
        {
            if (connector == null)
                throw new ArgumentNullException(nameof(connector));

            if (route == null)
                throw new ArgumentNullException(nameof(route));

            // --------------------------------------------------------
            // 0. CONNECTOR TYPE
            // --------------------------------------------------------
            //
            // Curved: vẫn dùng route.Points làm turn point,
            // nhưng nội suy mượt qua các điểm đó thay vì
            // bẻ góc vuông (RightAngle mặc định).
            //

            connector.Type = ConnectorType.Curved;

            // --------------------------------------------------------
            // 1. PORTS
            // --------------------------------------------------------

            ApplyPorts(
                connector,
                route);

            // --------------------------------------------------------
            // 2. TURN POINTS
            // --------------------------------------------------------

            ApplyPoints(
                connector,
                route);

            // --------------------------------------------------------
            // 3. LOCK
            // --------------------------------------------------------

            connector.CanChangeRoute = false;
            connector.CanDragBeginPoint = false;
            connector.CanDragEndPoint = false;
        }

        // ============================================================
        // PORTS
        // ============================================================

        private static void ApplyPorts(
            DiagramConnector connector,
            EdgeRoute route)
        {
            //connector.BeginItemPointIndex =
            //    GetConnectionPointIndex(
            //        route.SourcePort);

            //connector.EndItemPointIndex =
            //    GetConnectionPointIndex(
            //        route.TargetPort);
            connector.BeginItemPointIndex = -1;
            connector.EndItemPointIndex = -1;
        }

        // ============================================================
        // CONNECTION POINT INDEX
        // ============================================================

        private static int GetConnectionPointIndex(
            EdgePort port)
        {
            switch (port)
            {
                case EdgePort.Top:
                    return 0;

                case EdgePort.Right:
                    return 1;

                case EdgePort.Bottom:
                    return 2;

                case EdgePort.Left:
                    return 3;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(port),
                        port,
                        "Unsupported EdgePort.");
            }
        }

        // ============================================================
        // TURN POINTS
        // ============================================================

        private static void ApplyPoints(
            DiagramConnector connector,
            EdgeRoute route)
        {
            if (route.Points == null ||
                route.Points.Count == 0)
            {
                connector.Points = null;
                return;
            }

            var points =
                new List<PointFloat>(
                    route.Points.Count);

            foreach (EdgeRoutePoint point
                     in route.Points)
            {
                points.Add(
                    new PointFloat(
                        point.X,
                        point.Y));
            }

            connector.Points =
                new PointCollection(
                    points.ToArray());
        }
    }
}