using MermaidDiagram.Core.Graph;
using MermaidDiagram.Core.Layout;
using MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting;
using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;
using MermaidDiagram.Core.Parsing;
using MermaidDiagram.DevExpress.Controls;
using MermaidDiagram.DevExpress.Rendering;
using System.Runtime.Remoting.Contexts;



namespace MermaidDiagram.DevExpress.Pipeline
{
    /// <summary>
    /// Orchestration pipeline cuối cùng của Mermaid Diagram.
    ///
    /// Pipeline:
    ///
    ///     Mermaid source
    ///          ↓
    ///     MermaidParser
    ///          ↓
    ///     MermaidGraphBuilder
    ///          ↓
    ///     GraphAnalysisPipeline
    ///          ↓
    ///     RootLayoutContext
    ///          ↓
    ///     BuildLayoutItems
    ///          ↓
    ///     RootLayoutPlanner
    ///          ↓
    ///     MermaidLayoutResult
    ///          ↓
    ///     MermaidDiagramRenderer
    ///          ↓
    ///     DiagramControl
    ///
    /// RootLayoutPlanner chịu trách nhiệm toàn bộ layout,
    /// bao gồm EdgeRoutingEngine.
    ///
    /// MermaidDiagramPipeline KHÔNG trực tiếp gọi:
    ///     - MainPathLayout
    ///     - BranchLayoutEngine
    ///     - GroupLayoutEngine
    ///     - CollisionResolver
    ///     - GraphBoundsCalculator
    ///     - EdgeRoutingEngine
    ///     - ViewportFitter
    /// </summary>
    public sealed class MermaidDiagramPipeline
        : IMermaidDiagramPipeline
    {
        // ============================================================
        // DEPENDENCIES
        // ============================================================

        private readonly IMermaidParser _parser;

        private readonly MermaidGraphBuilder _graphBuilder;

        private readonly GraphAnalysisPipeline _analysisPipeline;

        private readonly RootLayoutPlanner _rootLayoutPlanner;

        private readonly MermaidDiagramRenderer _renderer;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidDiagramPipeline(
            IMermaidParser parser,
            MermaidGraphBuilder graphBuilder,
            GraphAnalysisPipeline analysisPipeline,
            RootLayoutPlanner rootLayoutPlanner,
            MermaidDiagramRenderer renderer)
        {
            _parser =
                parser
                ?? throw new ArgumentNullException(
                    nameof(parser));

            _graphBuilder =
                graphBuilder
                ?? throw new ArgumentNullException(
                    nameof(graphBuilder));

            _analysisPipeline =
                analysisPipeline
                ?? throw new ArgumentNullException(
                    nameof(analysisPipeline));

            _rootLayoutPlanner =
                rootLayoutPlanner
                ?? throw new ArgumentNullException(
                    nameof(rootLayoutPlanner));

            _renderer =
                renderer
                ?? throw new ArgumentNullException(
                    nameof(renderer));
        }

        // ============================================================
        // RENDER SOURCE
        // ============================================================

        public void Render(
            MermaidDiagramControl control,
            string source)
        {
            if (control == null)
            {
                throw new ArgumentNullException(
                    nameof(control));
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                Clear(control);
                return;
            }

            MermaidDocument document =
                _parser.Parse(source);

            if (document == null)
            {
                throw new InvalidOperationException(
                    "MermaidParser không trả về MermaidDocument.");
            }

            Render(
                control,
                document);
        }

        // ============================================================
        // RENDER DOCUMENT
        // ============================================================

        public void Render(
            MermaidDiagramControl control,
            MermaidDocument document)
        {
            if (control == null)
            {
                throw new ArgumentNullException(
                    nameof(control));
            }

            if (document == null)
            {
                throw new ArgumentNullException(
                    nameof(document));
            }

            // ========================================================
            // 1. BUILD GRAPH
            // ========================================================

            MermaidGraph graph =
                _graphBuilder.Build(
                    document);

            if (graph == null)
            {
                throw new InvalidOperationException(
                    "MermaidGraphBuilder không tạo được MermaidGraph.");
            }

            // ========================================================
            // 2. GRAPH ANALYSIS
            // ========================================================

            GraphAnalysisResult analysis =
                _analysisPipeline.Analyze(
                    graph);

            if (analysis == null)
            {
                throw new InvalidOperationException(
                    "GraphAnalysisPipeline không trả về GraphAnalysisResult.");
            }

            // ========================================================
            // 3. CREATE LAYOUT CONTEXT
            // ========================================================

            RootLayoutContext context =
                CreateLayoutContext(
                    control,
                    analysis);

            // ========================================================
            // 4. BUILD LAYOUT ITEMS
            // ========================================================

            BuildLayoutItems(
                context,
                document);

            if (context.Items.Count == 0)
            {
                Clear(control);
                return;
            }

            // ========================================================
            // 5. COMPLETE LAYOUT
            // ========================================================
            //
            // RootLayoutPlanner chịu trách nhiệm:
            //
            //     AdaptiveSpacing
            //          ↓
            //     MainPathLayout
            //          ↓
            //     BranchLayoutEngine
            //          ↓
            //     GroupLayoutEngine
            //          ↓
            //     CollisionResolver
            //          ↓
            //     GroupLayoutEngine
            //          ↓
            //     GraphBoundsCalculator
            //          ↓
            //     EdgeRoutingEngine
            //          ↓
            //     ViewportFitter
            //          ↓
            //     GraphBoundsCalculator
            //
            // Pipeline không gọi EdgeRoutingEngine trực tiếp.

            _rootLayoutPlanner.Plan(
                context);

            // ========================================================
            // 6. CREATE LAYOUT RESULT
            // ========================================================

            MermaidLayoutResult layout =
                CreateLayoutResult(
                    context);

            // ========================================================
            // 7. RENDER
            // ========================================================

            _renderer.Render(
                document,
                layout,
                control.DiagramControl);

            // ========================================================
            // 8. UPDATE CONTROL STATE
            // ========================================================

            control.SetDocument(
                document);

            control.SetLayoutResult(
                layout);
        }

        // ============================================================
        // CLEAR
        // ============================================================

        public void Clear(
            MermaidDiagramControl control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(
                    nameof(control));
            }

            control.DiagramControl.Items.Clear();

            control.ClearDocument();
        }

        // ============================================================
        // CREATE LAYOUT CONTEXT
        // ============================================================

        private static RootLayoutContext CreateLayoutContext(
            MermaidDiagramControl control,
            GraphAnalysisResult analysis)
        {
            if (control == null)
            {
                throw new ArgumentNullException(
                    nameof(control));
            }

            if (analysis == null)
            {
                throw new ArgumentNullException(
                    nameof(analysis));
            }

            // --------------------------------------------------------
            // EDGE ROUTING CONFIGURATION
            // --------------------------------------------------------

            EdgeRoutingOptions edgeRoutingOptions =
                control.EdgeRoutingOptions;

            if (edgeRoutingOptions == null)
            {
                throw new InvalidOperationException(
                    "MermaidDiagramControl chưa cung cấp EdgeRoutingOptions.");
            }

            edgeRoutingOptions.Validate();

            // --------------------------------------------------------
            // CONTEXT
            // --------------------------------------------------------

            return new RootLayoutContext(
                analysis,
                control.LayoutOptions,
                control.LayoutDirection,
                control.ViewportWidth,
                control.ViewportHeight);
        }

        // ============================================================
        // BUILD LAYOUT ITEMS
        // ============================================================

        private static void BuildLayoutItems(
            RootLayoutContext context,
            MermaidDocument document)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (document == null)
            {
                throw new ArgumentNullException(
                    nameof(document));
            }

            if (document.Nodes == null)
            {
                return;
            }

            foreach (MermaidNode node
                     in document.Nodes)
            {
                if (node == null)
                {
                    continue;
                }

                string nodeId =
                    node.Id?.Trim()
                    ?? string.Empty;

                if (nodeId.Length == 0)
                {
                    continue;
                }

                if (context.Items.ContainsKey(
                        nodeId))
                {
                    continue;
                }

                MermaidLayoutItem item =
                    new MermaidLayoutItem(
                        nodeId,
                        node.Text,
                        false,
                        context.Options.NodeWidth,
                        context.Options.NodeHeight);

                context.Items.Add(
                    nodeId,
                    item);
            }
        }

        // ============================================================
        // CREATE RESULT
        // ============================================================

        private static MermaidLayoutResult CreateLayoutResult(
            RootLayoutContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            return new MermaidLayoutResult(
                context.Items,
                context.GraphWidth,
                context.GraphHeight,
                context.Scale,
                context.EdgeRoutes);
        }
    }
}
