using MermaidDiagram.Core.Graph;
using MermaidDiagram.Core.Layout.Contract;
using MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting;
using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;
using MermaidDiagram.Core.Parsing;
namespace MermaidDiagram.Core.Layout.CoreLayout
{
    /// <summary>
    /// Entry point duy nhất của hệ thống layout Mermaid.
    ///
    /// MermaidLayoutEngine chỉ chịu trách nhiệm orchestration:
    ///
    ///     Parse
    ///       ↓
    ///     Build Graph
    ///       ↓
    ///     Analyze Graph
    ///       ↓
    ///     Create RootLayoutContext
    ///       ↓
    ///     Build Layout Items
    ///       ↓
    ///     RootLayoutPlanner
    ///       ↓
    ///     MermaidLayoutResult
    ///
    /// Không chứa thuật toán:
    ///     - graph analysis
    ///     - main path
    ///     - branch layout
    ///     - group layout
    ///     - collision
    ///     - edge routing
    ///     - bounds
    ///     - viewport fitting
    /// </summary>
    public sealed class MermaidLayoutEngine
        : IMermaidLayoutEngine
    {
        // ============================================================
        // COMPONENTS
        // ============================================================

        private readonly IMermaidParser _parser;

        private readonly MermaidGraphBuilder _graphBuilder;

        private readonly GraphAnalysisPipeline _analysisPipeline;

        private readonly RootLayoutPlanner _rootPlanner;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidLayoutEngine(
            IMermaidParser parser,
            MermaidGraphBuilder graphBuilder,
            GraphAnalysisPipeline analysisPipeline,
            RootLayoutPlanner rootPlanner)
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

            _rootPlanner =
                rootPlanner
                ?? throw new ArgumentNullException(
                    nameof(rootPlanner));
        }

        // ============================================================
        // LAYOUT SOURCE
        // ============================================================

        public MermaidLayoutResult Layout(
            string mermaidText,
            MermaidLayoutOptions options,
            MermaidLayoutDirection direction,
            float viewportWidth,
            float viewportHeight)
        {
            if (string.IsNullOrWhiteSpace(mermaidText))
            {
                throw new ArgumentException(
                    "Mermaid text cannot be null or empty.",
                    nameof(mermaidText));
            }

            if (options == null)
            {
                throw new ArgumentNullException(
                    nameof(options));
            }

            MermaidDocument document =
                _parser.Parse(
                    mermaidText);

            if (document == null)
            {
                throw new InvalidOperationException(
                    "MermaidParser không trả về MermaidDocument.");
            }

            return Layout(
                document,
                options,
                direction,
                viewportWidth,
                viewportHeight);
        }

        // ============================================================
        // LAYOUT DOCUMENT
        // ============================================================

        public MermaidLayoutResult Layout(
            MermaidDocument document,
            MermaidLayoutOptions options,
            MermaidLayoutDirection direction,
            float viewportWidth,
            float viewportHeight)
        {
            if (document == null)
            {
                throw new ArgumentNullException(
                    nameof(document));
            }

            if (options == null)
            {
                throw new ArgumentNullException(
                    nameof(options));
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
                    "MermaidGraphBuilder không trả về MermaidGraph.");
            }

            // ========================================================
            // 2. ANALYZE GRAPH
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
            // 3. CREATE CONTEXT
            // ========================================================

            RootLayoutContext context =
                new RootLayoutContext(
                    analysis,
                    options,
                    direction,
                    viewportWidth,
                    viewportHeight);

            // ========================================================
            // 4. CREATE LAYOUT ITEMS
            // ========================================================

            BuildLayoutItems(
                context,
                document);

            // ========================================================
            // 5. ROOT LAYOUT
            // ========================================================

            _rootPlanner.Plan(
                context);

            // ========================================================
            // 6. RESULT
            // ========================================================

            return new MermaidLayoutResult(
                context.Items,
                context.GraphWidth,
                context.GraphHeight,
                context.Scale,
                context.EdgeRoutes);
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
    }
}

