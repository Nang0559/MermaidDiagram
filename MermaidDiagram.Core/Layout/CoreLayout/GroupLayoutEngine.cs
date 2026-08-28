using MermaidDiagram.Core.Layout.Bounds;

using MermaidDiagram.Core.Layout.Result;

namespace MermaidDiagram.Core.Layout.CoreLayout
{
    /// <summary>
    /// Tính geometry cho Mermaid subgraph/group.
    ///
    /// GroupLayoutEngine chỉ chịu trách nhiệm:
    ///
    ///     - tìm group
    ///     - tìm direct children
    ///     - xử lý group lồng nhau
    ///     - tính children bounds
    ///     - áp padding
    ///     - cập nhật group geometry
    ///     - cập nhật preliminary GraphWidth / GraphHeight
    ///
    /// Không chịu trách nhiệm:
    ///
    ///     - parse Mermaid
    ///     - graph analysis
    ///     - MainPath
    ///     - BranchLayout
    ///     - EdgeRouting
    ///     - CollisionResolver
    ///     - ViewportFitter
    ///     - DevExpress
    ///
    /// Pipeline:
    ///
    ///     MainPath
    ///         ↓
    ///     BranchTree
    ///         ↓
    ///     GroupLayoutEngine #1
    ///         ↓
    ///     CollisionResolver
    ///         ↓
    ///     GroupLayoutEngine #2
    ///         ↓
    ///     GraphBoundsCalculator
    ///         ↓
    ///     ViewportFitter
    ///         ↓
    ///     GraphBoundsCalculator
    ///         ↓
    ///     EdgeRoutingEngine
    ///
    /// Lưu ý:
    ///
    /// GroupLayoutEngine KHÔNG route edge.
    ///
    /// EdgeRoutingEngine phải chạy sau khi toàn bộ geometry
    /// node/group đã ổn định.
    /// </summary>
    public sealed class GroupLayoutEngine
    {
        // ============================================================
        // CONSTANTS
        // ============================================================

        private const float DefaultHorizontalPadding = 25f;

        private const float DefaultVerticalPadding = 35f;

        private const float MinimumGeometry = 1f;

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Tính geometry cho toàn bộ group.
        ///
        /// Group con luôn được xử lý trước group cha.
        /// </summary>
        public void Apply(
            RootLayoutContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            List<MermaidLayoutItem> groups =
                GetGroups(context);

            if (groups.Count == 0)
            {
                UpdatePreliminaryGraphSize(
                    context);

                return;
            }

            /*
             * Group sâu nhất xử lý trước.
             *
             * Ví dụ:
             *
             * Group A
             *     └── Group B
             *             └── Group C
             *
             * Thứ tự:
             *
             *     C
             *     B
             *     A
             */
            groups =
                groups
                    .OrderByDescending(
                        group => GetGroupDepth(
                            context,
                            group))
                    .ThenBy(
                        group => group.Key,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            foreach (MermaidLayoutItem group
                     in groups)
            {
                LayoutGroup(
                    context,
                    group);
            }

            /*
             * Đây chỉ là preliminary size.
             *
             * GraphBoundsCalculator vẫn chịu trách nhiệm
             * tính bounds chính thức.
             */
            UpdatePreliminaryGraphSize(
                context);
        }

        // ============================================================
        // GET GROUPS
        // ============================================================

        private static List<MermaidLayoutItem> GetGroups(
            RootLayoutContext context)
        {
            return context.Items.Values
                .Where(
                    item =>
                        item != null &&
                        item.IsGroup)
                .OrderBy(
                    item => item.Key,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // ============================================================
        // GROUP DEPTH
        // ============================================================

        /// <summary>
        /// Tính depth của group.
        ///
        /// Root:
        ///     0
        ///
        /// Child:
        ///     1
        ///
        /// Grandchild:
        ///     2
        ///
        /// Có cycle protection.
        /// </summary>
        private static int GetGroupDepth(
            RootLayoutContext context,
            MermaidLayoutItem group)
        {
            int depth = 0;

            string parentKey =
                group.ParentGroupKey;

            HashSet<string> visited =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            while (!string.IsNullOrWhiteSpace(
                       parentKey))
            {
                if (!visited.Add(
                        parentKey))
                {
                    /*
                     * Group hierarchy bị cycle.
                     *
                     * Không throw tại đây để tránh làm hỏng
                     * toàn bộ layout.
                     *
                     * Graph/parser validation có thể xử lý
                     * vấn đề topology ở layer khác.
                     */
                    break;
                }

                depth++;

                if (!context.Items.TryGetValue(
                        parentKey,
                        out MermaidLayoutItem parent))
                {
                    break;
                }

                if (!parent.IsGroup)
                {
                    break;
                }

                parentKey =
                    parent.ParentGroupKey;
            }

            return depth;
        }

        // ============================================================
        // LAYOUT GROUP
        // ============================================================

        private static void LayoutGroup(
            RootLayoutContext context,
            MermaidLayoutItem group)
        {
            List<MermaidLayoutItem> children =
                GetDirectChildren(
                    context,
                    group);

            /*
             * Group rỗng.
             */
            if (children.Count == 0)
            {
                EnsureEmptyGroupGeometry(
                    context,
                    group);

                return;
            }

            MermaidBounds childrenBounds =
                CalculateChildrenBounds(
                    children);

            /*
             * Không có child nào có geometry hợp lệ.
             */
            if (!childrenBounds.IsValid)
            {
                EnsureEmptyGroupGeometry(
                    context,
                    group);

                return;
            }

            ApplyGroupBounds(
                context,
                group,
                childrenBounds);
        }

        // ============================================================
        // DIRECT CHILDREN
        // ============================================================

        /// <summary>
        /// Chỉ lấy child trực tiếp của group.
        ///
        /// Không lấy descendant sâu hơn.
        ///
        /// Ví dụ:
        ///
        ///     A
        ///     └── B
        ///         └── C
        ///
        /// DirectChildren(A)
        ///     = B
        ///
        /// C không xuất hiện ở đây.
        ///
        /// Vì B đã bao gồm C trong geometry của B.
        /// </summary>
        private static List<MermaidLayoutItem> GetDirectChildren(
            RootLayoutContext context,
            MermaidLayoutItem group)
        {
            return context.Items.Values
                .Where(
                    item =>
                        item != null &&
                        !ReferenceEquals(
                            item,
                            group) &&
                        string.Equals(
                            item.ParentGroupKey,
                            group.Key,
                            StringComparison.OrdinalIgnoreCase))
                .OrderBy(
                    item => item.Key,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // ============================================================
        // CHILDREN BOUNDS
        // ============================================================

        /// <summary>
        /// Tính bounding rectangle chứa toàn bộ direct children.
        ///
        /// Group con đã được xử lý trước nên geometry của group con
        /// đã bao gồm descendants của nó.
        /// </summary>
        private static MermaidBounds CalculateChildrenBounds(
            List<MermaidLayoutItem> children)
        {
            bool hasGeometry =
                false;

            float minX =
                float.MaxValue;

            float minY =
                float.MaxValue;

            float maxX =
                float.MinValue;

            float maxY =
                float.MinValue;

            foreach (MermaidLayoutItem child
                     in children)
            {
                if (!HasValidGeometry(
                        child))
                {
                    continue;
                }

                hasGeometry =
                    true;

                float left =
                    child.X;

                float top =
                    child.Y;

                float right =
                    child.X +
                    child.Width;

                float bottom =
                    child.Y +
                    child.Height;

                minX =
                    Math.Min(
                        minX,
                        left);

                minY =
                    Math.Min(
                        minY,
                        top);

                maxX =
                    Math.Max(
                        maxX,
                        right);

                maxY =
                    Math.Max(
                        maxY,
                        bottom);
            }

            if (!hasGeometry)
            {
                return MermaidBounds.Empty;
            }

            return new MermaidBounds(
                minX,
                minY,
                Math.Max(
                    MinimumGeometry,
                    maxX - minX),
                Math.Max(
                    MinimumGeometry,
                    maxY - minY));
        }

        // ============================================================
        // APPLY GROUP BOUNDS
        // ============================================================

        /// <summary>
        /// Áp padding vào children bounds để tạo geometry group.
        ///
        /// Không di chuyển child.
        ///
        /// Không thay đổi branch layout.
        ///
        /// Không thay đổi edge route.
        /// </summary>
        private static void ApplyGroupBounds(
            RootLayoutContext context,
            MermaidLayoutItem group,
            MermaidBounds childrenBounds)
        {
            float horizontalPadding =
                CalculateHorizontalPadding(
                    context);

            float verticalPadding =
                CalculateVerticalPadding(
                    context);

            group.X =
                childrenBounds.X -
                horizontalPadding;

            group.Y =
                childrenBounds.Y -
                verticalPadding;

            group.Width =
                Math.Max(
                    MinimumGeometry,
                    childrenBounds.Width +
                    horizontalPadding * 2f);

            group.Height =
                Math.Max(
                    MinimumGeometry,
                    childrenBounds.Height +
                    verticalPadding * 2f);

            group.IsPositioned =
                true;
        }

        // ============================================================
        // PADDING
        // ============================================================

        private static float CalculateHorizontalPadding(
            RootLayoutContext context)
        {
            return Math.Max(
                0f,
                DefaultHorizontalPadding +
                context.Options.CollisionPadding);
        }

        private static float CalculateVerticalPadding(
            RootLayoutContext context)
        {
            return Math.Max(
                0f,
                DefaultVerticalPadding +
                context.Options.CollisionPadding);
        }

        // ============================================================
        // EMPTY GROUP
        // ============================================================

        /// <summary>
        /// Đảm bảo group rỗng vẫn có geometry hợp lệ.
        ///
        /// Không reset X/Y về 0 nếu group đã có vị trí.
        ///
        /// Điều này rất quan trọng vì BranchLayout hoặc
        /// collision phase có thể đã đặt group trước đó.
        /// </summary>
        private static void EnsureEmptyGroupGeometry(
            RootLayoutContext context,
            MermaidLayoutItem group)
        {
            float minimumWidth =
                CalculateMinimumGroupWidth(
                    context);

            float minimumHeight =
                CalculateMinimumGroupHeight(
                    context);

            group.Width =
                Math.Max(
                    group.Width,
                    minimumWidth);

            group.Height =
                Math.Max(
                    group.Height,
                    minimumHeight);

            /*
             * Chỉ fallback về origin nếu group hoàn toàn
             * chưa có geometry.
             *
             * Không reset group đã được layout.
             */
            if (!group.IsPositioned)
            {
                group.X =
                    0f;

                group.Y =
                    0f;
            }

            group.IsPositioned =
                true;
        }

        private static float CalculateMinimumGroupWidth(
            RootLayoutContext context)
        {
            return Math.Max(
                MinimumGeometry,
                context.Options.NodeWidth +
                CalculateHorizontalPadding(
                    context) * 2f);
        }

        private static float CalculateMinimumGroupHeight(
            RootLayoutContext context)
        {
            return Math.Max(
                MinimumGeometry,
                context.Options.NodeHeight +
                CalculateVerticalPadding(
                    context) * 2f);
        }

        // ============================================================
        // VALID GEOMETRY
        // ============================================================

        private static bool HasValidGeometry(
            MermaidLayoutItem item)
        {
            if (item == null)
            {
                return false;
            }

            if (!item.IsPositioned)
            {
                return false;
            }

            if (float.IsNaN(item.X) ||
                float.IsNaN(item.Y) ||
                float.IsNaN(item.Width) ||
                float.IsNaN(item.Height))
            {
                return false;
            }

            if (float.IsInfinity(item.X) ||
                float.IsInfinity(item.Y) ||
                float.IsInfinity(item.Width) ||
                float.IsInfinity(item.Height))
            {
                return false;
            }

            if (item.Width <= 0f ||
                item.Height <= 0f)
            {
                return false;
            }

            return true;
        }

        // ============================================================
        // PRELIMINARY GRAPH SIZE
        // ============================================================

        /// <summary>
        /// Tính preliminary graph size.
        ///
        /// Đây KHÔNG phải final graph bounds.
        ///
        /// GraphBoundsCalculator sẽ tính lại sau đó.
        /// </summary>
        private static void UpdatePreliminaryGraphSize(
            RootLayoutContext context)
        {
            bool hasGeometry =
                false;

            float minX =
                float.MaxValue;

            float minY =
                float.MaxValue;

            float maxRight =
                float.MinValue;

            float maxBottom =
                float.MinValue;

            foreach (MermaidLayoutItem item
                     in context.Items.Values)
            {
                if (!HasValidGeometry(
                        item))
                {
                    continue;
                }

                hasGeometry =
                    true;

                float right =
                    item.X +
                    item.Width;

                float bottom =
                    item.Y +
                    item.Height;

                minX =
                    Math.Min(
                        minX,
                        item.X);

                minY =
                    Math.Min(
                        minY,
                        item.Y);

                maxRight =
                    Math.Max(
                        maxRight,
                        right);

                maxBottom =
                    Math.Max(
                        maxBottom,
                        bottom);
            }

            if (!hasGeometry)
            {
                context.GraphWidth =
                    0f;

                context.GraphHeight =
                    0f;

                return;
            }

            context.GraphWidth =
                Math.Max(
                    0f,
                    maxRight - minX);

            context.GraphHeight =
                Math.Max(
                    0f,
                    maxBottom - minY);
        }
    }
}

