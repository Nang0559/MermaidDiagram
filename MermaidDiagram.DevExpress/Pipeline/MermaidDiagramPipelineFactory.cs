
using MermaidDiagram.Core.Graph;
using MermaidDiagram.Core.Layout;
using MermaidDiagram.Core.Layout.Bounds;
using MermaidDiagram.Core.Layout.CoreLayout;
using MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting;
using MermaidDiagram.Core.Layout.Layered;
using MermaidDiagram.Core.Layout.Spacing;
using MermaidDiagram.Core.Parsing;
using MermaidDiagram.DevExpress.Mapping;
using MermaidDiagram.DevExpress.Rendering;

namespace MermaidDiagram.DevExpress.Pipeline
{
    /// <summary>
    /// Factory tạo toàn bộ Mermaid Diagram pipeline cho DevExpress.
    ///
    /// Factory là composition root duy nhất.
    ///
    /// Dependency flow:
    ///
    ///     MermaidParser
    ///          ↓
    ///     MermaidGraphBuilder
    ///          ↓
    ///     GraphAnalysisPipeline
    ///          ↓
    ///     RootLayoutPlanner
    ///          ↓
    ///     MermaidDiagramRenderer
    ///
    /// RootLayoutPlanner:
    ///
    ///     LayerAssignment
    ///          ↓
    ///     DummyNodeInsertion
    ///          ↓
    ///     LayerOrdering
    ///          ↓
    ///     CrossingMinimization
    ///          ↓
    ///     CoordinateAssignment
    ///          ↓
    ///     Collision
    ///          ↓
    ///     GroupLayout
    ///          ↓
    ///     Bounds
    ///          ↓
    ///     Viewport
    ///          ↓
    ///     EdgeRouting
    ///
    /// DevExpress:
    ///
    ///     Mapper
    ///       ↓
    ///     ItemFactory
    ///       ↓
    ///     NodeRenderer
    ///     GroupRenderer
    ///     EdgeRenderer
    ///       ↓
    ///     MermaidDiagramRenderer
    ///
    /// Factory không chứa:
    ///     - parse;
    ///     - graph analysis;
    ///     - layout algorithm;
    ///     - rendering logic;
    ///     - interaction;
    ///     - workflow / business logic.
    /// </summary>
    public static class MermaidDiagramPipelineFactory
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Tạo MermaidDiagramPipeline hoàn chỉnh.
        /// </summary>
        public static IMermaidDiagramPipeline Create()
        {
            // ========================================================
            // CORE - PARSER
            // ========================================================

            IMermaidParser parser =
                new MermaidParser();

            // ========================================================
            // CORE - GRAPH
            // ========================================================

            MermaidGraphBuilder graphBuilder =
                new MermaidGraphBuilder();

            GraphAnalysisPipeline analysisPipeline =
                new GraphAnalysisPipeline();

            // ========================================================
            // CORE - LAYOUT
            // ========================================================

            RootLayoutPlanner rootLayoutPlanner =
                CreateRootLayoutPlanner();

            // ========================================================
            // DEVEXPRESS - MAPPER
            // ========================================================

            IMermaidDevExpressMapper mapper =
                new MermaidDevExpressMapper();

            // ========================================================
            // DEVEXPRESS - ITEM FACTORY
            // ========================================================

            MermaidDiagramItemFactory itemFactory =
                new MermaidDiagramItemFactory(
                    mapper);

            // ========================================================
            // DEVEXPRESS - NODE RENDERER
            // ========================================================

            MermaidNodeRenderer nodeRenderer =
                new MermaidNodeRenderer(
                    itemFactory);

            // ========================================================
            // DEVEXPRESS - GROUP RENDERER
            // ========================================================

            MermaidGroupRenderer groupRenderer =
                new MermaidGroupRenderer(
                    itemFactory);

            // ========================================================
            // DEVEXPRESS - EDGE RENDERER
            // ========================================================

            MermaidEdgeRenderer edgeRenderer =
                new MermaidEdgeRenderer(
                    itemFactory);

            // ========================================================
            // DEVEXPRESS - ROOT RENDERER
            // ========================================================

            MermaidDiagramRenderer renderer =
                new MermaidDiagramRenderer(
                    nodeRenderer,
                    edgeRenderer,
                    groupRenderer);

            // ========================================================
            // PIPELINE
            // ========================================================

            return new MermaidDiagramPipeline(
                parser,
                graphBuilder,
                analysisPipeline,
                rootLayoutPlanner,
                renderer);
        }

        // ============================================================
        // ROOT LAYOUT PLANNER
        // ============================================================

        private static RootLayoutPlanner CreateRootLayoutPlanner()
        {
            // ========================================================
            // LAYER ASSIGNMENT
            // ========================================================

            LayerAssignmentEngine layerAssignment =
                new LayerAssignmentEngine();

            // ========================================================
            // DUMMY NODE INSERTION
            // ========================================================
            //
            // DummyNodeInserter phải được inject vào
            // RootLayoutPlanner.
            //
            // Không gọi Insert() tại đây.
            //
            // RootLayoutPlanner.Plan() sẽ quyết định
            // thời điểm thực thi.
            //

            DummyNodeInserter dummyNodeInserter =
                new DummyNodeInserter();

            // ========================================================
            // LAYER ORDERING
            // ========================================================

            LayerOrderingEngine layerOrdering =
                new LayerOrderingEngine();

            // ========================================================
            // CROSSING MINIMIZATION
            // ========================================================

            CrossingMinimizer crossingMinimizer =
                new CrossingMinimizer(
                    layerOrdering,
                    6);

            // ========================================================
            // COORDINATE ASSIGNMENT
            // ========================================================

            CoordinateAssignmentEngine coordinateAssignment =
                new CoordinateAssignmentEngine();

            // ========================================================
            // COLLISION
            // ========================================================

            CollisionResolver collisionResolver =
                new CollisionResolver();

            // ========================================================
            // GROUP LAYOUT
            // ============================================================

            GroupLayoutEngine groupLayoutEngine =
                new GroupLayoutEngine();

            // ========================================================
            // GRAPH BOUNDS
            // ========================================================

            GraphBoundsCalculator boundsCalculator =
                new GraphBoundsCalculator();

            // ========================================================
            // EDGE ROUTING
            // ========================================================

            EdgeRoutingOptions edgeRoutingOptions =
                new EdgeRoutingOptions();

            EdgeRoutingEngine edgeRoutingEngine =
                new EdgeRoutingEngine(
                    edgeRoutingOptions);

            // ========================================================
            // VIEWPORT
            // ========================================================

            ViewportFitter viewportFitter =
                new ViewportFitter();

            // ========================================================
            // ROOT PLANNER
            // ========================================================

            return new RootLayoutPlanner(
                layerAssignment,
                dummyNodeInserter,
                layerOrdering,
                crossingMinimizer,
                coordinateAssignment,
                collisionResolver,
                groupLayoutEngine,
                boundsCalculator,
                edgeRoutingEngine,
                viewportFitter);
        }
    }
}

