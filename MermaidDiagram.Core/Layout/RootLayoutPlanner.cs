using MermaidDiagram.Core.Graph;
using MermaidDiagram.Core.Layout.Bounds;
using MermaidDiagram.Core.Layout.CoreLayout;
using MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting;
using MermaidDiagram.Core.Layout.Layered;
using MermaidDiagram.Core.Layout.Result;
using System;

namespace MermaidDiagram.Core.Layout
{
    /// <summary>
    /// Orchestrator của toàn bộ graph layout.
    ///
    /// Pipeline:
    ///
    ///     Layer Assignment
    ///       ↓
    ///     Dummy Node Insertion
    ///       ↓
    ///     Layer Ordering
    ///       ↓
    ///     Crossing Minimization
    ///       ↓
    ///     Coordinate Assignment
    ///       ↓
    ///     Collision
    ///       ↓
    ///     Group
    ///       ↓
    ///     Graph Bounds
    ///       ↓
    ///     Viewport
    ///       ↓
    ///     Edge Routing
    ///       ↓
    ///     Graph Bounds
    /// </summary>
    public sealed class RootLayoutPlanner
    {
        // ============================================================
        // LAYER
        // ============================================================

        private readonly LayerAssignmentEngine
            _layerAssignment;

        // ============================================================
        // DUMMY
        // ============================================================

        private readonly DummyNodeInserter
            _dummyNodeInserter;

        // ============================================================
        // ORDERING
        // ============================================================

        private readonly LayerOrderingEngine
            _layerOrdering;

        // ============================================================
        // CROSSING
        // ============================================================

        private readonly CrossingMinimizer
            _crossingMinimizer;

        // ============================================================
        // COORDINATE
        // ============================================================

        private readonly CoordinateAssignmentEngine
            _coordinateAssignment;

        // ============================================================
        // COLLISION
        // ============================================================

        private readonly CollisionResolver
            _collisionResolver;

        // ============================================================
        // GROUP
        // ============================================================

        private readonly GroupLayoutEngine
            _groupLayoutEngine;

        // ============================================================
        // BOUNDS
        // ============================================================

        private readonly GraphBoundsCalculator
            _boundsCalculator;

        // ============================================================
        // EDGE ROUTING
        // ============================================================

        private readonly EdgeRoutingEngine
            _edgeRouting;

        // ============================================================
        // VIEWPORT
        // ============================================================

        private readonly ViewportFitter
            _viewportFitter;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public RootLayoutPlanner(
            LayerAssignmentEngine layerAssignment,
            DummyNodeInserter dummyNodeInserter,
            LayerOrderingEngine layerOrdering,
            CrossingMinimizer crossingMinimizer,
            CoordinateAssignmentEngine coordinateAssignment,
            CollisionResolver collisionResolver,
            GroupLayoutEngine groupLayoutEngine,
            GraphBoundsCalculator boundsCalculator,
            EdgeRoutingEngine edgeRouting,
            ViewportFitter viewportFitter)
        {
            _layerAssignment =
                layerAssignment
                ?? throw new ArgumentNullException(
                    nameof(layerAssignment));

            _dummyNodeInserter =
                dummyNodeInserter
                ?? throw new ArgumentNullException(
                    nameof(dummyNodeInserter));

            _layerOrdering =
                layerOrdering
                ?? throw new ArgumentNullException(
                    nameof(layerOrdering));

            _crossingMinimizer =
                crossingMinimizer
                ?? throw new ArgumentNullException(
                    nameof(crossingMinimizer));

            _coordinateAssignment =
                coordinateAssignment
                ?? throw new ArgumentNullException(
                    nameof(coordinateAssignment));

            _collisionResolver =
                collisionResolver
                ?? throw new ArgumentNullException(
                    nameof(collisionResolver));

            _groupLayoutEngine =
                groupLayoutEngine
                ?? throw new ArgumentNullException(
                    nameof(groupLayoutEngine));

            _boundsCalculator =
                boundsCalculator
                ?? throw new ArgumentNullException(
                    nameof(boundsCalculator));

            _edgeRouting =
                edgeRouting
                ?? throw new ArgumentNullException(
                    nameof(edgeRouting));

            _viewportFitter =
                viewportFitter
                ?? throw new ArgumentNullException(
                    nameof(viewportFitter));
        }

        // ============================================================
        // PLAN
        // ============================================================

        public void Plan(
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

            // ========================================================
            // 1. LAYER ASSIGNMENT
            // ========================================================

            MermaidGraphLayer layered =
                _layerAssignment.Assign(
                    context);

            context.Layer.Clear();

            foreach (KeyValuePair<string, int> pair
                     in layered.Layer)
            {
                context.Layer[pair.Key] =
                    pair.Value;
            }

            context.Layers.Clear();

            foreach (KeyValuePair<int, List<string>> pair
                     in layered.Layers)
            {
                context.Layers[pair.Key] =
                    pair.Value;
            }

            // ========================================================
            // 2. DUMMY NODE INSERTION
            // ========================================================
            //
            // Đây là bước bắt buộc của Sugiyama.
            //
            // Ví dụ:
            //
            //     A(L1) ─────────────> D(L4)
            //
            // sẽ thành:
            //
            //     A(L1)
            //       │
            //     dummy(L2)
            //       │
            //     dummy(L3)
            //       │
            //       D(L4)
            //
            // DummyNodeInserter đồng thời:
            //
            //     context.Items
            //     context.Layer
            //     context.Layers
            //     context.EffectiveAdjacency
            //     context.EdgeDummyChain
            //
            // Vì vậy bước này phải chạy SAU LayerAssignment
            // nhưng TRƯỚC Ordering/Crossing/Coordinate.
            //

            _dummyNodeInserter.Insert(
                context);

            // ========================================================
            // 3. ORDERING
            // ========================================================
            //
            // LayerOrderingEngine đã được sửa để đọc:
            //
            //     context.EffectiveAdjacency
            //
            // thay vì Graph.Adjacency.
            //
            // Do đó dummy node thực sự tham gia ordering.
            //

            _layerOrdering.Order(
                context,
                context.Layers);

            // ========================================================
            // 4. CROSSING MINIMIZATION
            // ========================================================

            _crossingMinimizer.Minimize(
                context,
                context.Layers);

            // ========================================================
            // 5. COORDINATE ASSIGNMENT
            // ========================================================
            //
            // Dummy node đã nằm trong context.Layers nên nó sẽ
            // được cấp X/Y cùng với các node thật.
            //

            _coordinateAssignment.Assign(
                context,
                context.Layers);

            // ========================================================
            // 6. COLLISION
            // ========================================================

            _collisionResolver.Resolve(
                context,
                context.Layers);

            // ========================================================
            // 7. UPDATE ITEM METADATA
            // ========================================================

            UpdateLayerMetadata(
                context);

            // ========================================================
            // 8. GROUP LAYOUT
            // ========================================================

            _groupLayoutEngine.Apply(
                context);

            // ========================================================
            // 9. GRAPH BOUNDS
            // ========================================================

            _boundsCalculator.Calculate(
                context);

            // ========================================================
            // 10. VIEWPORT
            // ========================================================
            //
            // Viewport phải hoàn tất trước EdgeRouting.
            //

            _viewportFitter.Fit(
                context);

            // ========================================================
            // 11. GRAPH BOUNDS SAU VIEWPORT
            // ========================================================

            _boundsCalculator.Calculate(
                context);

            // ========================================================
            // 12. EDGE ROUTING
            // ========================================================
            //
            // EdgeRoutingEngine sẽ đọc:
            //
            //     context.EdgeDummyChain
            //
            // để route:
            //
            //     source
            //       ↓
            //     dummy1
            //       ↓
            //     dummy2
            //       ↓
            //     target
            //
            // thay vì nối source -> target trực tiếp.
            //

            _edgeRouting.Route(
                context);

            // ========================================================
            // 13. FINAL GRAPH BOUNDS
            // ========================================================

            _boundsCalculator.Calculate(
                context);
        }

        // ============================================================
        // EMPTY CONTEXT
        // ============================================================

        private static void ResetEmptyContext(
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

            context.EdgeRoutes.Clear();

            context.EffectiveAdjacency.Clear();

            context.EdgeDummyChain.Clear();
        }

        // ============================================================
        // METADATA
        // ============================================================

        private static void UpdateLayerMetadata(
            RootLayoutContext context)
        {
            foreach (KeyValuePair<int, List<string>> pair
                     in context.Layers)
            {
                int layerIndex =
                    pair.Key;

                foreach (string key
                         in pair.Value)
                {
                    if (!context.Items.TryGetValue(
                            key,
                            out MermaidLayoutItem? item))
                    {
                        continue;
                    }

                    item.Layer =
                        layerIndex;

                    item.IsPositioned =
                        true;
                }
            }
        }
    }
}