namespace MermaidDiagram.Core.Graph
{
    /// <summary>
    /// Orchestrator thực hiện toàn bộ quá trình phân tích MermaidGraph.
    ///
    /// Pipeline:
    ///
    /// MermaidGraph
    ///     ↓
    /// Find Roots
    ///     ↓
    /// Calculate Layers
    ///     ↓
    /// Find Main Path
    ///     ↓
    /// Build Main Path Index
    ///     ↓
    /// GraphAnalysisResult
    ///
    /// Không thực hiện layout.
    /// </summary>
    public sealed class GraphAnalysisPipeline
    {
        // ============================================================
        // SERVICES
        // ============================================================

        private readonly GraphAnalyzer _analyzer;

        private readonly GraphLayerCalculator _layerCalculator;

        private readonly MainPathCalculator _mainPathCalculator;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public GraphAnalysisPipeline()
        {
            _analyzer =
                new GraphAnalyzer();

            _layerCalculator =
                new GraphLayerCalculator();

            _mainPathCalculator =
                new MainPathCalculator();
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        public GraphAnalysisResult Analyze(
            MermaidGraph graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(
                    nameof(graph));
            }

            // ========================================================
            // 1. ROOT
            // ========================================================

            HashSet<string> roots =
                _analyzer.FindRootKeys(
                    graph);

            // ========================================================
            // 2. LAYER
            // ========================================================

            MermaidGraphLayer graphLayer =
                _layerCalculator.Calculate(
                    graph);

            // ========================================================
            // 3. MAIN PATH
            // ========================================================

            List<string> mainPath =
                _mainPathCalculator.Find(
                    graph,
                    graphLayer,
                    roots);

            // ========================================================
            // 4. MAIN PATH INDEX
            // ========================================================

            Dictionary<string, int> mainPathIndex =
                _mainPathCalculator.BuildIndex(
                    mainPath);

            // ========================================================
            // 5. RESULT
            // ========================================================

            return new GraphAnalysisResult(
                graph,
                roots,
                graphLayer.Layer,
                mainPath,
                mainPathIndex);
        }
    }
}