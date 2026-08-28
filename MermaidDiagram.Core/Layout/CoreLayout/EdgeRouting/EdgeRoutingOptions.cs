namespace MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting
{
    /// <summary>
    /// Cấu hình cho EdgeRoutingEngine.
    ///
    /// Chỉ chứa thông số routing connector.
    /// Không chứa spacing của node layout.
    /// </summary>
    public sealed class EdgeRoutingOptions
    {
        // ============================================================
        // ENABLE
        // ============================================================

        /// <summary>
        /// Bật routing connector.
        /// </summary>
        public bool Enabled { get; set; } = true;

        // ============================================================
        // GENERAL
        // ============================================================

        /// <summary>
        /// Khoảng cách tối thiểu từ connector tới node khác.
        /// </summary>
        public float ObstacleClearance { get; set; } = 18f;

        /// <summary>
        /// Khoảng cách tối thiểu giữa hai routing lane.
        /// </summary>
        public float LaneSpacing { get; set; } = 14f;

        /// <summary>
        /// Khoảng cách connector đi ra khỏi node
        /// trước khi chuyển hướng.
        /// </summary>
        public float PortOffset { get; set; } = 18f;

        /// <summary>
        /// Khoảng cách tối thiểu dành cho label
        /// so với node / connector khác.
        /// </summary>
        public float LabelClearance { get; set; } = 8f;

        // ============================================================
        // RETURN EDGE
        // ============================================================

        /// <summary>
        /// Khoảng cách lane dành cho các edge quay về
        /// terminal / main path.
        /// </summary>
        public float ReturnLaneSpacing { get; set; } = 16f;

        /// <summary>
        /// Có cho phép return edge đi vòng ra ngoài graph hay không.
        /// </summary>
        public bool AllowOuterReturnLane { get; set; } = true;

        // ============================================================
        // BRANCH
        // ============================================================

        /// <summary>
        /// Khoảng cách tối thiểu giữa branch connector
        /// và main path.
        /// </summary>
        public float BranchClearance { get; set; } = 24f;

        /// <summary>
        /// Khoảng cách connector nằm ngoài branch node
        /// trước khi quay về main path.
        /// </summary>
        public float BranchLaneSpacing { get; set; } = 14f;

        // ============================================================
        // PORT SELECTION
        // ============================================================

        /// <summary>
        /// Ưu tiên connector dọc cho edge thuộc main path.
        /// </summary>
        public bool PreferVerticalMainPath { get; set; } = true;

        /// <summary>
        /// Ưu tiên connector ngang cho side branch.
        /// </summary>
        public bool PreferHorizontalBranch { get; set; } = true;

        /// <summary>
        /// Ưu tiên connector Bottom -> Top
        /// cho workflow TopToBottom.
        /// </summary>
        public bool PreferBottomToTop { get; set; } = true;

        // ============================================================
        // LABEL
        // ============================================================

        /// <summary>
        /// Đưa label vào giữa đoạn routing dài nhất.
        /// </summary>
        public bool PlaceLabelOnLongestSegment { get; set; } = true;

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Kiểm tra configuration.
        /// </summary>
        public void Validate()
        {
            ObstacleClearance =
                Math.Max(
                    0f,
                    ObstacleClearance);

            LaneSpacing =
                Math.Max(
                    1f,
                    LaneSpacing);

            PortOffset =
                Math.Max(
                    0f,
                    PortOffset);

            LabelClearance =
                Math.Max(
                    0f,
                    LabelClearance);

            ReturnLaneSpacing =
                Math.Max(
                    1f,
                    ReturnLaneSpacing);

            BranchClearance =
                Math.Max(
                    0f,
                    BranchClearance);

            BranchLaneSpacing =
                Math.Max(
                    1f,
                    BranchLaneSpacing);
        }
    }
}