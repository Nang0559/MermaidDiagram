namespace MermaidDiagram.Core.Layout.Contract
{
    /// <summary>
    /// Cấu hình cho toàn bộ quá trình Mermaid layout.
    ///
    /// Options chỉ chứa tham số cấu hình.
    /// Không chứa trạng thái layout runtime.
    /// </summary>
   
        /// <summary>
        /// Cấu hình cho toàn bộ quá trình Mermaid layout.
        ///
        /// Class này chỉ chứa configuration.
        /// Không chứa trạng thái runtime của layout.
        /// </summary>
        public sealed class MermaidLayoutOptions
        {
            // ============================================================
            // NODE SIZE
            // ============================================================

            /// <summary>
            /// Chiều rộng mặc định của node.
            /// </summary>
            public float NodeWidth { get; set; } = 150f;

            /// <summary>
            /// Chiều cao mặc định của node.
            /// </summary>
            public float NodeHeight { get; set; } = 70f;

            // ============================================================
            // MAIN PATH SPACING
            // ============================================================

            /// <summary>
            /// Khoảng cách theo trục ngang giữa các node
            /// của Main Path.
            /// </summary>
            public float HorizontalSpacing { get; set; } = 50f;

            /// <summary>
            /// Khoảng cách theo trục dọc giữa các node
            /// của Main Path.
            /// </summary>
            public float VerticalSpacing { get; set; } = 80f;

            // ============================================================
            // BRANCH SPACING
            // ============================================================

            /// <summary>
            /// Khoảng cách giữa các branch cùng layer.
            /// </summary>
            public float BranchSpacing { get; set; } = 35f;

            /// <summary>
            /// Khoảng cách giữa các layer của branch.
            ///
            /// Không dùng tên VerticalSpacing vì layout có thể
            /// chạy theo cả 4 hướng.
            /// </summary>
            public float BranchVerticalSpacing { get; set; } = 60f;

            // ============================================================
            // ROOT SPACING
            // ============================================================

            /// <summary>
            /// Khoảng cách giữa các root graph.
            /// </summary>
            public float RootSpacing { get; set; } = 80f;

            // ============================================================
            // COLLISION
            // ============================================================

            /// <summary>
            /// Khoảng cách tối thiểu mong muốn giữa hai item
            /// sau khi collision resolution hoàn tất.
            /// </summary>
            public float MinimumNodeGap { get; set; } = 20f;

            /// <summary>
            /// Padding bổ sung dùng khi phát hiện collision.
            /// </summary>
            public float CollisionPadding { get; set; } = 10f;

            /// <summary>
            /// Số vòng tối đa CollisionResolver được phép
            /// thực hiện để loại bỏ collision.
            /// </summary>
            public int CollisionMaxIterations { get; set; } = 50;

            // ============================================================
            // VIEWPORT MARGIN
            // ============================================================

            public float MarginLeft { get; set; } = 30f;

            public float MarginTop { get; set; } = 30f;

            public float MarginRight { get; set; } = 30f;

            public float MarginBottom { get; set; } = 30f;

            // ============================================================
            // ADAPTIVE LAYOUT
            // ============================================================

            /// <summary>
            /// Cho phép tự động điều chỉnh spacing theo
            /// kích thước graph và viewport.
            /// </summary>
            public bool UseAdaptiveSpacing { get; set; } = true;

            // ============================================================
            // VIEWPORT SCALE
            // ============================================================

            /// <summary>
            /// Cho phép tự động scale graph để fit viewport.
            /// </summary>
            public bool UseAdaptiveScale { get; set; } = true;

            /// <summary>
            /// Scale nhỏ nhất được phép.
            /// </summary>
            public float MinimumScale { get; set; } = 0.70f;

            /// <summary>
            /// Scale lớn nhất được phép.
            /// </summary>
            public float MaximumScale { get; set; } = 1.00f;

            // ============================================================
            // MAIN PATH
            // ============================================================

            /// <summary>
            /// Ưu tiên Main Path nằm gần trung tâm graph
            /// trước khi ViewportFitter xử lý viewport.
            /// </summary>
            public bool PreferMainPathCenter { get; set; } = true;

            // ============================================================
            // DEFAULT
            // ============================================================

            public static MermaidLayoutOptions Default
            {
                get
                {
                    return new MermaidLayoutOptions();
                }
            }

            // ============================================================
            // VALIDATION
            // ============================================================

            /// <summary>
            /// Kiểm tra tính hợp lệ của configuration.
            /// </summary>
            public void Validate()
            {
                // --------------------------------------------------------
                // NODE
                // --------------------------------------------------------

                if (NodeWidth <= 0f)
                {
                    throw new InvalidOperationException(
                        "NodeWidth must be greater than zero.");
                }

                if (NodeHeight <= 0f)
                {
                    throw new InvalidOperationException(
                        "NodeHeight must be greater than zero.");
                }

                // --------------------------------------------------------
                // MAIN PATH SPACING
                // --------------------------------------------------------

                if (HorizontalSpacing < 0f)
                {
                    throw new InvalidOperationException(
                        "HorizontalSpacing cannot be negative.");
                }

                if (VerticalSpacing < 0f)
                {
                    throw new InvalidOperationException(
                        "VerticalSpacing cannot be negative.");
                }

                // --------------------------------------------------------
                // BRANCH
                // --------------------------------------------------------

                if (BranchSpacing < 0f)
                {
                    throw new InvalidOperationException(
                        "BranchSpacing cannot be negative.");
                }

                if (BranchVerticalSpacing < 0f)
                {
                    throw new InvalidOperationException(
                        "BranchLayerSpacing cannot be negative.");
                }

                // --------------------------------------------------------
                // ROOT
                // --------------------------------------------------------

                if (RootSpacing < 0f)
                {
                    throw new InvalidOperationException(
                        "RootSpacing cannot be negative.");
                }

                // --------------------------------------------------------
                // COLLISION
                // --------------------------------------------------------

                if (MinimumNodeGap < 0f)
                {
                    throw new InvalidOperationException(
                        "MinimumNodeGap cannot be negative.");
                }

                if (CollisionPadding < 0f)
                {
                    throw new InvalidOperationException(
                        "CollisionPadding cannot be negative.");
                }

                if (CollisionMaxIterations <= 0)
                {
                    throw new InvalidOperationException(
                        "CollisionMaxIterations must be greater than zero.");
                }

                // --------------------------------------------------------
                // MARGIN
                // --------------------------------------------------------

                if (MarginLeft < 0f ||
                    MarginTop < 0f ||
                    MarginRight < 0f ||
                    MarginBottom < 0f)
                {
                    throw new InvalidOperationException(
                        "Margins cannot be negative.");
                }

                // --------------------------------------------------------
                // SCALE
                // --------------------------------------------------------

                if (MinimumScale <= 0f)
                {
                    throw new InvalidOperationException(
                        "MinimumScale must be greater than zero.");
                }

                if (MaximumScale <= 0f)
                {
                    throw new InvalidOperationException(
                        "MaximumScale must be greater than zero.");
                }

                if (MinimumScale > MaximumScale)
                {
                    throw new InvalidOperationException(
                        "MinimumScale cannot be greater than MaximumScale.");
                }
            }
        }
    
}
