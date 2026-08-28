
namespace MermaidDiagram.Core.Layout.Result
{
    /// <summary>
    /// Đại diện cho một item đã được chuẩn bị cho layout.
    ///
    /// Item có thể là:
    ///     - Mermaid node
    ///     - Mermaid subgraph
    ///
    /// Class này hoàn toàn độc lập với DevExpress/UI.
    /// </summary>
    public sealed class MermaidLayoutItem
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// ID duy nhất của node hoặc subgraph.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Text hiển thị của item.
        /// </summary>
        public string Text { get; }

        // ============================================================
        // TYPE
        // ============================================================

        /// <summary>
        /// True nếu item đại diện cho Mermaid subgraph.
        /// False nếu là node.
        /// </summary>
        public bool IsGroup { get; }

        /// <summary>
        /// Key của subgraph cha trực tiếp.
        ///
        /// Empty nếu item thuộc root.
        /// </summary>
        public string ParentGroupKey { get; set; }

        // ============================================================
        // POSITION
        // ============================================================

        /// <summary>
        /// Tọa độ X hiện tại của item.
        ///
        /// Trước ViewportFitter:
        ///     graph coordinate.
        ///
        /// Sau ViewportFitter:
        ///     viewport coordinate.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Tọa độ Y hiện tại của item.
        /// </summary>
        public float Y { get; set; }

        // ============================================================
        // SIZE
        // ============================================================

        /// <summary>
        /// Chiều rộng hiện tại của item.
        ///
        /// Sau ViewportFitter có thể đã được scale.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Chiều cao hiện tại của item.
        ///
        /// Sau ViewportFitter có thể đã được scale.
        /// </summary>
        public float Height { get; set; }

        // ============================================================
        // GRAPH POSITION
        // ============================================================

        /// <summary>
        /// Layer của item trong graph.
        ///
        /// -1 nếu chưa được tính.
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// Root graph mà item thuộc về.
        ///
        /// Empty nếu chưa được xác định.
        /// </summary>
        public string RootKey { get; set; }

        // ============================================================
        // MAIN PATH
        // ============================================================

        /// <summary>
        /// True nếu item thuộc Main Path.
        /// </summary>
        public bool IsMainPath { get; set; }

        /// <summary>
        /// Vị trí của item trên Main Path.
        ///
        /// -1 nếu không thuộc Main Path.
        /// </summary>
        public int MainPathIndex { get; set; }

        // ============================================================
        // LAYOUT STATE
        // ============================================================

        /// <summary>
        /// True nếu item đã được đặt vị trí.
        /// </summary>
        public bool IsPositioned { get; set; }

        // ============================================================
        // ORIGINAL LAYOUT GEOMETRY
        // ============================================================

        /// <summary>
        /// X trước khi ViewportFitter áp dụng:
        ///     - scale
        ///     - translation
        ///     - centering
        ///
        /// Đây là geometry cuối cùng sau:
        ///     MainPath
        ///     Branch
        ///     Group
        ///     Collision
        /// </summary>
        public float LayoutX { get; set; }

        /// <summary>
        /// Y trước viewport transform.
        /// </summary>
        public float LayoutY { get; set; }

        /// <summary>
        /// Width trước viewport scale.
        /// </summary>
        public float LayoutWidth { get; set; }

        /// <summary>
        /// Height trước viewport scale.
        /// </summary>
        public float LayoutHeight { get; set; }

        /// <summary>
        /// Cho biết geometry layout gốc đã được snapshot hay chưa.
        ///
        /// false:
        ///     item chưa được snapshot.
        ///
        /// true:
        ///     LayoutX/LayoutY/LayoutWidth/LayoutHeight
        ///     là geometry trước ViewportFitter.
        /// </summary>
        public bool HasLayoutGeometry { get; set; }

        // ============================================================
        // GEOMETRY
        // ============================================================

        /// <summary>
        /// Tọa độ cạnh phải của geometry hiện tại.
        /// </summary>
        public float Right
        {
            get
            {
                return X + Width;
            }
        }

        /// <summary>
        /// Tọa độ cạnh dưới của geometry hiện tại.
        /// </summary>
        public float Bottom
        {
            get
            {
                return Y + Height;
            }
        }

        /// <summary>
        /// Tâm X của geometry hiện tại.
        /// </summary>
        public float CenterX
        {
            get
            {
                return X + Width / 2f;
            }
        }

        /// <summary>
        /// Tâm Y của geometry hiện tại.
        /// </summary>
        public float CenterY
        {
            get
            {
                return Y + Height / 2f;
            }
        }
        // ============================================================
        // DUMMY (MỚI)
        // ============================================================

        /// <summary>
        /// True nếu đây là node ảo (virtual/dummy node) do
        /// DummyNodeInserter tạo ra để làm waypoint cho cạnh
        /// vượt nhiều layer. Không tương ứng với node Mermaid thật,
        /// không render như node thường (renderer bỏ qua dummy khi
        /// tạo DiagramShape).
        /// </summary>
        public bool IsDummy { get; set; }
        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidLayoutItem(
            string key,
            string text,
            bool isGroup,
            float width,
            float height)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Item key cannot be null or empty.",
                    nameof(key));
            }

            if (width <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    width,
                    "Item width must be greater than zero.");
            }

            if (height <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(height),
                    height,
                    "Item height must be greater than zero.");
            }

            Key =
                key;

            Text =
                text ?? string.Empty;

            IsGroup =
                isGroup;

            ParentGroupKey =
                string.Empty;

            RootKey =
                string.Empty;

            X =
                0f;

            Y =
                0f;

            Width =
                width;

            Height =
                height;

            Layer =
                -1;

            MainPathIndex =
                -1;

            IsMainPath =
                false;

            IsPositioned =
                false;

            // --------------------------------------------------------
            // ORIGINAL LAYOUT GEOMETRY
            // --------------------------------------------------------

            LayoutX =
                0f;

            LayoutY =
                0f;

            LayoutWidth =
                width;

            LayoutHeight =
                height;

            HasLayoutGeometry =
                false;
        }

        // ============================================================
        // RESET VIEWPORT SNAPSHOT
        // ============================================================

        /// <summary>
        /// Xóa snapshot geometry của ViewportFitter.
        ///
        /// Phải gọi khi thực hiện một lần layout mới trên
        /// cùng MermaidLayoutItem.
        /// </summary>
        public void ResetLayoutGeometry()
        {
            LayoutX =
                0f;

            LayoutY =
                0f;

            LayoutWidth =
                Width;

            LayoutHeight =
                Height;

            HasLayoutGeometry =
                false;
        }
    }
}

