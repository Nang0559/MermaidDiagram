using MermaidDiagram.Core.Graph;
using MermaidDiagram.Core.Layout.Contract;
using MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting;
using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;

namespace MermaidDiagram.Core.Layout
{
    /// <summary>
    /// Context dùng xuyên suốt một layout session.
    ///
    /// RootLayoutContext chỉ chứa:
    ///     - graph analysis;
    ///     - layout items;
    ///     - layout options;
    ///     - viewport;
    ///     - spacing;
    ///     - bounds;
    ///     - effective adjacency;
    ///     - dummy edge chains;
    ///     - edge routing result;
    ///     - scale.
    ///
    /// Context KHÔNG chứa thuật toán.
    /// </summary>
    public sealed class RootLayoutContext
    {
        // ============================================================
        // GRAPH ANALYSIS
        // ============================================================

        public GraphAnalysisResult Analysis
        {
            get;
        }

        public MermaidGraph Graph
        {
            get
            {
                return Analysis.Graph;
            }
        }

        // ============================================================
        // LAYOUT DIRECTION
        // ============================================================

        public MermaidLayoutDirection Direction
        {
            get;
        }

        // ============================================================
        // LAYOUT ITEMS
        // ============================================================

        public Dictionary<string, MermaidLayoutItem> Items
        {
            get;
        }

        // ============================================================
        // ROOT MAPPING
        // ============================================================

        public Dictionary<string, string> NodeRootKey
        {
            get;
        }

        // ============================================================
        // GRAPH ANALYSIS SHORTCUTS
        // ============================================================

        public HashSet<string> RootKeys
        {
            get
            {
                return Analysis.RootKeys;
            }
        }

        public Dictionary<string, int> Layer
        {
            get
            {
                return Analysis.Layer;
            }
        }

        public Dictionary<int, List<string>> Layers
        {
            get
            {
                return Analysis.Layers;
            }
        }

        public List<string> MainPath
        {
            get
            {
                return Analysis.MainPath;
            }
        }

        public Dictionary<string, int> MainPathIndex
        {
            get
            {
                return Analysis.MainPathIndex;
            }
        }

        // ============================================================
        // VIEWPORT
        // ============================================================

        public float ViewportWidth
        {
            get;
        }

        public float ViewportHeight
        {
            get;
        }

        public float ViewportAspect
        {
            get
            {
                if (ViewportHeight <= 0f)
                {
                    return 1f;
                }

                return ViewportWidth /
                       ViewportHeight;
            }
        }

        // ============================================================
        // CALCULATED SPACING
        // ============================================================

        public float HorizontalSpacing
        {
            get;
            set;
        }

        public float VerticalSpacing
        {
            get;
            set;
        }

        public float BranchSpacing
        {
            get;
            set;
        }

        public float BranchVerticalSpacing
        {
            get;
            set;
        }

        public float RootSpacing
        {
            get;
            set;
        }

        // ============================================================
        // GRAPH BOUNDS
        // ============================================================

        public float GraphMinX
        {
            get;
            set;
        }

        public float GraphMinY
        {
            get;
            set;
        }

        public float GraphMaxX
        {
            get;
            set;
        }

        public float GraphMaxY
        {
            get;
            set;
        }

        public float GraphWidth
        {
            get;
            set;
        }

        public float GraphHeight
        {
            get;
            set;
        }

        // ============================================================
        // EFFECTIVE ADJACENCY
        // ============================================================

        /// <summary>
        /// Adjacency thực tế dùng cho layered layout.
        ///
        /// Khác với Graph.Adjacency:
        ///
        ///     A -> D
        ///
        /// có thể trở thành:
        ///
        ///     A -> dummy1 -> dummy2 -> D
        ///
        /// DummyNodeInserter ghi dữ liệu.
        ///
        /// LayerOrderingEngine,
        /// CrossingMinimizer,
        /// CoordinateAssignmentEngine
        /// và EdgeRoutingEngine có thể đọc dữ liệu này
        /// tùy theo trách nhiệm của từng engine.
        /// </summary>
        public Dictionary<string, List<string>> EffectiveAdjacency
        {
            get;
        }

        // ============================================================
        // EDGE DUMMY CHAIN
        // ============================================================

        /// <summary>
        /// Chuỗi dummy của từng edge Mermaid gốc.
        ///
        /// Key:
        ///     source->target
        ///
        /// Value:
        ///     dummy1, dummy2, ...
        ///
        /// Ví dụ:
        ///
        ///     A -> D
        ///
        /// trở thành:
        ///
        ///     A -> dummy1 -> dummy2 -> D
        ///
        /// thì:
        ///
        ///     EdgeDummyChain["A->D"]
        ///
        /// chứa:
        ///
        ///     dummy1
        ///     dummy2
        /// </summary>
        public Dictionary<string, List<string>> EdgeDummyChain
        {
            get;
        }

        // ============================================================
        // EDGE ROUTING
        // ============================================================

        /// <summary>
        /// Kết quả routing cuối cùng của từng edge.
        ///
        /// Key:
        ///     Source -> Target
        ///
        /// EdgeRoutingEngine ghi vào.
        /// Renderer chỉ đọc.
        /// </summary>
        public Dictionary<string, EdgeRoute> EdgeRoutes
        {
            get;
        }

        // ============================================================
        // SCALE
        // ============================================================

        public float Scale
        {
            get;
            set;
        }

        // ============================================================
        // OPTIONS
        // ============================================================

        public MermaidLayoutOptions Options
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public RootLayoutContext(
            GraphAnalysisResult analysis,
            MermaidLayoutOptions options,
            MermaidLayoutDirection direction,
            float viewportWidth,
            float viewportHeight)
        {
            // --------------------------------------------------------
            // ANALYSIS
            // --------------------------------------------------------

            Analysis =
                analysis
                ?? throw new ArgumentNullException(
                    nameof(analysis));

            // --------------------------------------------------------
            // OPTIONS
            // --------------------------------------------------------

            Options =
                options
                ?? throw new ArgumentNullException(
                    nameof(options));

            Options.Validate();

            // --------------------------------------------------------
            // DIRECTION
            // --------------------------------------------------------

            Direction =
                direction;

            // --------------------------------------------------------
            // VIEWPORT
            // --------------------------------------------------------

            ViewportWidth =
                Math.Max(
                    0f,
                    viewportWidth);

            ViewportHeight =
                Math.Max(
                    0f,
                    viewportHeight);

            // --------------------------------------------------------
            // ITEMS
            // --------------------------------------------------------

            Items =
                new Dictionary<string, MermaidLayoutItem>(
                    StringComparer.OrdinalIgnoreCase);

            // --------------------------------------------------------
            // ROOT MAPPING
            // --------------------------------------------------------

            NodeRootKey =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            // --------------------------------------------------------
            // EFFECTIVE ADJACENCY
            // --------------------------------------------------------
            //
            // QUAN TRỌNG:
            // DummyNodeInserter sẽ gọi:
            //
            //     context.EffectiveAdjacency.Clear();
            //
            // nên dictionary bắt buộc phải được khởi tạo
            // ngay khi tạo context.
            //

            EffectiveAdjacency =
                new Dictionary<string, List<string>>(
                    StringComparer.OrdinalIgnoreCase);

            // --------------------------------------------------------
            // EDGE DUMMY CHAIN
            // --------------------------------------------------------
            //
            // DummyNodeInserter sẽ gọi:
            //
            //     context.EdgeDummyChain.Clear();
            //
            // nên cũng phải được khởi tạo tại đây.
            //

            EdgeDummyChain =
                new Dictionary<string, List<string>>(
                    StringComparer.OrdinalIgnoreCase);

            // --------------------------------------------------------
            // SPACING
            // --------------------------------------------------------

            HorizontalSpacing =
                Options.HorizontalSpacing;

            VerticalSpacing =
                Options.VerticalSpacing;

            BranchSpacing =
                Options.BranchSpacing;

            BranchVerticalSpacing =
                Options.BranchVerticalSpacing;

            RootSpacing =
                Options.RootSpacing;

            // --------------------------------------------------------
            // BOUNDS
            // --------------------------------------------------------

            GraphMinX =
                0f;

            GraphMinY =
                0f;

            GraphMaxX =
                0f;

            GraphMaxY =
                0f;

            GraphWidth =
                0f;

            GraphHeight =
                0f;

            // --------------------------------------------------------
            // EDGE ROUTING
            // --------------------------------------------------------

            EdgeRoutes =
                new Dictionary<string, EdgeRoute>(
                    StringComparer.OrdinalIgnoreCase);

            // --------------------------------------------------------
            // SCALE
            // --------------------------------------------------------

            Scale =
                1f;
        }

        // ============================================================
        // EDGE KEY
        // ============================================================

        public static string GetEdgeKey(
            string source,
            string target)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(
                    "Source không được rỗng.",
                    nameof(source));
            }

            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException(
                    "Target không được rỗng.",
                    nameof(target));
            }

            return
                source.Trim() +
                "->" +
                target.Trim();
        }

        public static string GetEdgeKey(
            MermaidEdge edge)
        {
            if (edge == null)
            {
                throw new ArgumentNullException(
                    nameof(edge));
            }

            return GetEdgeKey(
                edge.Source,
                edge.Target);
        }

        // ============================================================
        // ROUTING RESET
        // ============================================================

        public void ClearEdgeRouting()
        {
            EdgeRoutes.Clear();
        }

        // ============================================================
        // LAYOUT RESET
        // ============================================================

        /// <summary>
        /// Xóa dữ liệu tạm của dummy/layout.
        ///
        /// Không xóa GraphAnalysisResult.
        /// Không xóa Items của node thật.
        /// </summary>
        public void ClearEffectiveLayoutData()
        {
            EffectiveAdjacency.Clear();
            EdgeDummyChain.Clear();
        }
    }
}