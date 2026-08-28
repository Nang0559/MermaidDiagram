

namespace TestMd
{

    using DevExpress.Diagram.Core;
    using DevExpress.Diagram.Core.InteractiveLayout;
    using DevExpress.Diagram.Core.Layout;
    using DevExpress.DirectX.Common;
    using DevExpress.Utils;
    using DevExpress.XtraCharts;
    using DevExpress.XtraDiagram;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using DiagramLayoutDirection =
    DevExpress.Diagram.Core.Layout.LayoutDirection;

    public static class MermaidToDevExpressAdvancedParser
    {
        // C# 7.3 compatible.
        // Important: DiagramDoubleCollection does NOT use Add().
        // It must be created from an array, e.g.
        // new DiagramDoubleCollection(new double[] { 4.0, 3.0 });

        private static readonly Font NodeFont = new Font("Segoe UI Emoji", 9.0f, FontStyle.Regular);

        private static SizeF MeasureWrappedText(string text, Font font, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return new SizeF(maxWidth, 40f);

            using (var bmp = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bmp))
            {
                return g.MeasureString(text, font, Math.Max(40, (int)maxWidth));
            }
        }

      

        private sealed class NodeDef
        {
            public string Id;
            public string Text;
            public bool Circle;
            public bool Decision;
            public string GroupId;
        }
        private sealed class RootLayoutContext
        {
            public DiagramControl Diagram;

            public List<DiagramItem> RootItems;

            public Dictionary<string, NodeDef> Nodes;
            public List<EdgeDef> Edges;

            public Dictionary<GroupDef, DiagramContainer> ContainerMap;

            public DiagramLayoutDirection Direction;

            // ------------------------------------------------------------
            // Root item
            // ------------------------------------------------------------

            public Dictionary<string, DiagramItem> RootItemMap =
                new Dictionary<string, DiagramItem>(
                    StringComparer.OrdinalIgnoreCase);

            // ------------------------------------------------------------
            // Node -> root group/node
            // ------------------------------------------------------------

            public Dictionary<string, string> NodeRootKey =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            // ------------------------------------------------------------
            // Root graph
            // ------------------------------------------------------------

            public HashSet<string> RootKeys =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, List<string>> Adjacency =
                new Dictionary<string, List<string>>(
                    StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, List<string>> ReverseAdjacency =
                new Dictionary<string, List<string>>(
                    StringComparer.OrdinalIgnoreCase);

            // ------------------------------------------------------------
            // Layer
            // ------------------------------------------------------------

            public Dictionary<string, int> Layer =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            public Dictionary<int, List<string>> Layers =
                new Dictionary<int, List<string>>();

            // ------------------------------------------------------------
            // Main path
            // ------------------------------------------------------------

            public List<string> MainPath =
                new List<string>();

            public Dictionary<string, int> MainPathIndex =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            // ------------------------------------------------------------
            // Viewport
            // ------------------------------------------------------------

            public float ViewportWidth;
            public float ViewportHeight;
            public float ViewportAspect;

            // ------------------------------------------------------------
            // Spacing
            // ------------------------------------------------------------

            public float HorizontalSpacing;
            public float VerticalSpacing;

            // ------------------------------------------------------------
            // Calculated graph size
            // ------------------------------------------------------------

            public float GraphWidth;
            public float GraphHeight;
        }
        private sealed class EdgeDef
        {
            public string Source;
            public string Target;
            public string Label;
            public string Operator;
        }

        private sealed class GroupDef
        {
            public string Id;
            public string Title;
            public int Depth;
            public GroupDef Parent;
            public readonly List<string> NodeIds = new List<string>();
            public readonly List<GroupDef> Children = new List<GroupDef>();
        }

        private static readonly Regex SubgraphRegex = new Regex(
            @"^\s*subgraph\s+(?:(?<id>[A-Za-z_][A-Za-z0-9_-]*)\s*\[\s*""(?<title>.*?)""\s*\]|(?<plain>.+?))\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex NodeRegex = new Regex(
            @"(?<id>[A-Za-z_][A-Za-z0-9_-]*)(?:" +
            @"\(\(""(?<circle>.*?)""\)\)" +
            @"|\{""(?<decision>.*?)""\}" +
            @"|\[""(?<rect>.*?)""\]" +
            @"|\(""(?<round>.*?)""\)" +
            @"|\((?<roundRaw>.*?)\)" +
            @"|\[(?<rectRaw>.*?)\]" +
            @"|\{(?<decisionRaw>.*?)\})",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex EdgeRegex = new Regex(
            @"(?<src>[A-Za-z_][A-Za-z0-9_-]*)\s*" +
            @"(?<op>-->|-.->|==>|---|-.-)\s*" +
            @"(?:\|(?<label>.*?)\|\s*)?" +
            @"(?<dst>[A-Za-z_][A-Za-z0-9_-]*)",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex StyleRegex = new Regex(
            @"^\s*style\s+(?<id>[A-Za-z_][A-Za-z0-9_-]*)\s+(?<props>.+?)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ClassDefRegex = new Regex(
            @"^\s*classDef\s+(?<name>[A-Za-z_][A-Za-z0-9_-]*)\s+(?<props>.+?)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ClassApplyRegex = new Regex(
            @"^\s*class\s+(?<ids>[A-Za-z0-9_,\-\s]+)\s+(?<name>[A-Za-z_][A-Za-z0-9_-]*)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex FormNameRegex = new Regex(
            @"\bForm[A-Za-z0-9_]+\b",
            RegexOptions.Compiled);
        public static void ParseAndBuildDiagram(
            DiagramControl diagram,
            string mermaidText,
            bool autoDetectDirection = true)
        {
            if (diagram == null || string.IsNullOrWhiteSpace(mermaidText))
                return;

            diagram.BeginUpdate();

            try
            {
                diagram.Items.Clear();

                List<string> lines = NormalizeLines(mermaidText);

                string mermaidDirectionToken = null;

                foreach (string rawLine in lines)
                {
                    string lower = rawLine.ToLowerInvariant();

                    if (lower.StartsWith("graph ", StringComparison.Ordinal) ||
                        lower.StartsWith("flowchart ", StringComparison.Ordinal))
                    {
                        string[] parts = rawLine.Split(
                            new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length >= 2)
                            mermaidDirectionToken = parts[1];

                        break;
                    }
                }

                var nodes = new Dictionary<string, NodeDef>(StringComparer.OrdinalIgnoreCase);
                var edges = new List<EdgeDef>();
                var groups = new List<GroupDef>();
                var groupById = new Dictionary<string, GroupDef>(StringComparer.OrdinalIgnoreCase);
                var rootGroups = new List<GroupDef>();
                var groupStack = new Stack<GroupDef>();
                var styles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var classDefs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var classApplications = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                ParseMermaid(
                    lines, nodes, edges, groups, groupById,
                    rootGroups, groupStack, styles, classDefs, classApplications);

                // ============================================================
                // 1. Tạo container TRƯỚC, theo thứ tự từ nông -> sâu (Depth tăng dần).
                //    Container con được add vào container cha ngay lúc tạo,
                //    KHÔNG add vào diagram.Items nếu nó có cha.
                // ============================================================

                var containerMap = new Dictionary<GroupDef, DiagramContainer>();

                foreach (GroupDef group in groups.OrderBy(g => g.Depth))
                {
                    var container = new DiagramContainer
                    {
                        Shape = StandardContainers.Classic,
                        Header = group.Title,
                        ShowHeader = true,
                        CanAddItems = true,
                        ItemsCanChangeParent = true,
                        // Kích thước khởi tạo chỉ là placeholder,
                        // layout bên dưới sẽ tự tính lại theo nội dung.
                        AdjustBoundsBehavior = AdjustBoundaryBehavior.AutoAdjust,
                        //Width = 240.0f,
                        //Height = 140.0f
                    };

                    container.HeaderPadding = new Padding(8, 6, 8, 6);
                    container.Padding = new Padding(15, 22, 15, 15); // top=22 để chừa khoảng dưới header

                    container.Appearance.BackColor = Color.FromArgb(248, 249, 250);
                    container.Appearance.BorderColor = Color.FromArgb(190, 198, 208);
                    container.Appearance.BorderSize = 1;

                    container.Appearance.ForeColor = GetContrastingTextColor(container.Appearance.BackColor);
                    container.Appearance.Options.UseForeColor = true;

                    container.Appearance.Options.UseBackColor = true;
                    container.Appearance.Options.UseBorderColor = true;
                    container.Appearance.Options.UseBorderSize = true;

                    // Đổi Font Header của Container sang Segoe UI Emoji để hiện Icon trên tiêu đề Group
                    container.Appearance.Font = new Font("Segoe UI Emoji", 9.0f, FontStyle.Bold);
                    container.Appearance.Options.UseFont = true;

                    if (group.Parent != null &&
                        containerMap.TryGetValue(group.Parent, out DiagramContainer parentContainer))
                    {
                        parentContainer.Items.Add(container);
                    }
                    else
                    {
                        diagram.Items.Add(container);
                    }

                    containerMap[group] = container;
                }

                // ============================================================
                // 2. Tạo shape, add vào ĐÚNG container của nó (nếu có),
                //    hoặc vào diagram.Items nếu không thuộc group nào.
                // ============================================================

                var shapeMap = new Dictionary<string, DiagramShape>(StringComparer.OrdinalIgnoreCase);

                foreach (NodeDef node in nodes.Values)
                {
                    DiagramShape shape = CreateShape(node);

                    if (styles.TryGetValue(node.Id, out string styleText))
                        ApplyStyle(shape, styleText);

                    if (classApplications.TryGetValue(node.Id, out string className) &&
                        classDefs.TryGetValue(className, out string classProps))
                    {
                        ApplyStyle(shape, classProps);
                    }

                    GroupDef ownerGroup = null;

                    if (node.GroupId != null)
                        groupById.TryGetValue(node.GroupId, out ownerGroup);

                    if (ownerGroup != null && containerMap.TryGetValue(ownerGroup, out DiagramContainer owner))
                        owner.Items.Add(shape);
                    else
                        diagram.Items.Add(shape);

                    shapeMap[node.Id] = shape;
                }

                // ============================================================
                // 3. Tạo Connector, lưu vào map để dùng lại ở cả layout nội bộ
                //    lẫn layout cấp gốc. Connector luôn add vào diagram.Items (root)
                //    — DevExpress tự resolve theo tham chiếu Source/Target.
                // ============================================================

                var connectorMap = new Dictionary<EdgeDef, DiagramConnector>();

                foreach (EdgeDef edge in edges)
                {
                    if (!shapeMap.TryGetValue(edge.Source, out DiagramShape source) ||
                        !shapeMap.TryGetValue(edge.Target, out DiagramShape target))
                        continue;

                    DiagramConnector connector = CreateConnector(source, target, edge);
                    connectorMap[edge] = connector;
                    diagram.Items.Add(connector);
                }

                // ============================================================
                // 4. Khoảng cách layer/node.
                // ============================================================

                diagram.OptionsSugiyamaLayout.ColumnSpacing = 80;
                diagram.OptionsSugiyamaLayout.LayerSpacing = 60;

                DiagramLayoutDirection direction = DecideLayoutDirection(
                    nodes, edges, mermaidDirectionToken, autoDetectDirection);

                // ============================================================
                // 5. QUAN TRỌNG: DevExpress KHÔNG tự layout item bên trong container.
                //    Phải tự layout đệ quy: group lá trước, group cha sau.
                // ============================================================

                var consumedEdges = new HashSet<EdgeDef>();

                foreach (GroupDef rootGroup in rootGroups)
                {
                    LayoutGroupRecursive(
                        diagram, rootGroup, containerMap, shapeMap,
                        connectorMap, edges, consumedEdges, direction);
                }

                // ============================================================
                // 6. Layout cấp gốc: shape không thuộc group nào + container gốc
                //    (đã có kích thước thật từ bước 5, DevExpress coi nó là 1 node)
                //    + connector CHƯA bị "tiêu thụ" ở bước 5 (connector liên nhóm).
                // ============================================================

                var rootItems = new List<DiagramItem>();

                foreach (NodeDef node in nodes.Values)
                {
                    if (node.GroupId == null && shapeMap.TryGetValue(node.Id, out DiagramShape rootShape))
                        rootItems.Add(rootShape);
                }

                foreach (GroupDef rootGroup in rootGroups)
                {
                    if (containerMap.TryGetValue(rootGroup, out DiagramContainer rootContainer))
                        rootItems.Add(rootContainer);
                }
                // ============================================================
                // 6.5. QUAN TRỌNG: việc Remove/Add shape qua lại giữa container
                //      và diagram.Items ở bước 5 (layout nội bộ từng group) có thể
                //      khiến DevExpress tự động xoá các connector đang gắn vào
                //      shape đó (cascade-delete khi Remove khỏi Items, để tránh
                //      tham chiếu treo). Vì vậy KHÔNG dựa vào connector tạo ở bước 3
                //      còn sống sót — dọn sạch chúng và tạo lại 1 lần cuối, khi mọi
                //      vị trí đã ổn định và không còn shape nào bị di chuyển nữa.
                // ============================================================

                foreach (DiagramConnector oldConnector in connectorMap.Values)
                {
                    if (diagram.Items.Contains(oldConnector))
                        diagram.Items.Remove(oldConnector);
                }

                foreach (EdgeDef edge in edges)
                {
                    if (!shapeMap.TryGetValue(edge.Source, out DiagramShape source) ||
                        !shapeMap.TryGetValue(edge.Target, out DiagramShape target))
                        continue;

                    DiagramConnector connector = CreateConnector(source, target, edge);
                    diagram.Items.Add(connector);
                }

                // Sắp root items theo thứ tự luồng chính (topological), dùng lại
                // ComputeNodeLayers nhưng trên danh sách rootItems đã gộp nhóm.
                var rootLayers = ComputeGroupedLayers(nodes, edges); // hàm mới, xem bên dưới

                List<DiagramItem> orderedRootItems = rootItems
                    .OrderBy(item =>
                    {
                        string key = item is DiagramContainer c
                            ? "GROUP::" + containerMap.First(kv => kv.Value == c).Key.Id
                            : (string)((DiagramShape)item).Tag;

                        return rootLayers.TryGetValue(key, out int l) ? l : 0;
                    })
                    .ToList();

                //ArrangeRootItemsWrapped(
                //    orderedRootItems,
                //    targetRowCount: 2,      // chỉnh số này để rộng/hẹp theo ý bạn
                //    horizontalSpacing: 80,
                //    verticalSpacing: 60);
                ArrangeRootItemsAdaptive(
                    diagram,
                    rootItems,
                    nodes,
                    edges,
                    containerMap,
                    direction,
                    horizontalSpacing: 80f,
                    verticalSpacing: 60f);

                // ============================================================
                // 7. Fit toàn bộ diagram.
                // ============================================================

                if (diagram.Items.Count > 0)
                {
                    // Cài đặt lề (margin) xung quanh sơ đồ (ví dụ 10-20px)
                    diagram.OptionsView.FitToDrawingMargin = new System.Windows.Forms.Padding(15);

                    // Fit toàn bộ các item vừa khít tầm nhìn DiagramControl
                    diagram.FitToItems(diagram.Items);

                    // Bật thanh cuộn nếu sơ đồ rộng hơn Form
                    //diagram.OptionsBehavior.ScrollMode = DevExpress.XtraDiagram.DiagramScrollMode.Pixel;
                }
            }
            finally
            {
                diagram.EndUpdate();
            }
        }
        private static void LayoutGroupRecursive(
    DiagramControl diagram,
    GroupDef group,
    Dictionary<GroupDef, DiagramContainer> containerMap,
    Dictionary<string, DiagramShape> shapeMap,
    Dictionary<EdgeDef, DiagramConnector> connectorMap,
    List<EdgeDef> edges,
    HashSet<EdgeDef> consumedEdges,
    DiagramLayoutDirection direction)
        {
            // Layout group con TRƯỚC (lá -> gốc).
            foreach (GroupDef child in group.Children)
            {
                LayoutGroupRecursive(
                    diagram, child, containerMap, shapeMap,
                    connectorMap, edges, consumedEdges, direction);
            }

            if (!containerMap.TryGetValue(group, out DiagramContainer container))
                return;

            var idSet = new HashSet<string>(group.NodeIds, StringComparer.OrdinalIgnoreCase);

            // Shape/container con TRỰC TIẾP của group này (đang nằm trong container.Items).
            var childShapes = new List<DiagramItem>();

            foreach (string nodeId in group.NodeIds)
            {
                if (shapeMap.TryGetValue(nodeId, out DiagramShape shape))
                    childShapes.Add(shape);
            }

            foreach (GroupDef child in group.Children)
            {
                if (containerMap.TryGetValue(child, out DiagramContainer childContainer))
                    childShapes.Add(childContainer);
            }

            if (childShapes.Count == 0)
                return;

            // Connector "nội bộ": cả 2 đầu đều thuộc group này.
            var innerConnectors = new List<DiagramItem>();

            foreach (EdgeDef edge in edges)
            {
                if (consumedEdges.Contains(edge))
                    continue;

                if (idSet.Contains(edge.Source) && idSet.Contains(edge.Target) &&
                    connectorMap.TryGetValue(edge, out DiagramConnector connector))
                {
                    innerConnectors.Add(connector);
                    consumedEdges.Add(edge);
                }
            }

            // ------------------------------------------------------------------
            // BƯỚC QUAN TRỌNG NHẤT: DevExpress chỉ layout được item KHÔNG có
            // parent là container. Phải "nhấc" ra root, layout xong mới đưa lại.
            // ------------------------------------------------------------------

            foreach (DiagramItem item in childShapes)
            {
                container.Items.Remove(item);
                diagram.Items.Add(item);
            }

            var layoutItems = new List<DiagramItem>(childShapes);
            layoutItems.AddRange(innerConnectors);

            diagram.ApplySugiyamaLayout(direction, layoutItems);

            // Đo bounding box theo tọa độ global (item đang ở root).
            float minX = childShapes.Min(x => x.Position.X);
            float minY = childShapes.Min(x => x.Position.Y);
            float maxX = childShapes.Max(x => x.Position.X + x.Width);
            float maxY = childShapes.Max(x => x.Position.Y + x.Height);

            // Đưa item trở lại container: set Position cục bộ TRƯỚC khi Add,
            // vì Add() không tự quy đổi hệ tọa độ.
            foreach (DiagramItem item in childShapes)
            {
                float localX = item.Position.X - minX + container.Padding.Left;
                float localY = item.Position.Y - minY + container.Padding.Top;

                diagram.Items.Remove(item);

                item.Position = new DevExpress.Utils.PointFloat(localX, localY);

                container.Items.Add(item);
            }

            container.Width = Math.Max(
                200f,
                (maxX - minX) + container.Padding.Left + container.Padding.Right);

            container.Height = Math.Max(
                120f,
                (maxY - minY) + container.Padding.Top + container.Padding.Bottom);
        }
        //    private static void ArrangeRootItemsWrapped(
        //List<DiagramItem> orderedItems,
        //int targetRowCount, // Số dòng bạn muốn (VD: truyền vào 2 hoặc 3)
        //float horizontalSpacing,
        //float verticalSpacing)
        //    {
        //        if (orderedItems.Count == 0) return;

        //        // 1. Tính tổng độ rộng của tất cả các Container/Item gốc
        //        float totalWidth = orderedItems.Sum(i => i.Width) + (orderedItems.Count - 1) * horizontalSpacing;

        //        // 2. Độ rộng lý thuyết mục tiêu cho mỗi hàng
        //        float targetRowWidth = totalWidth / Math.Max(1, targetRowCount);

        //        var rows = new List<List<DiagramItem>>();
        //        var currentRow = new List<DiagramItem>();
        //        float currentRowWidth = 0f;

        //        foreach (DiagramItem item in orderedItems)
        //        {
        //            // Khi hàng hiện tại đã vượt targetRowWidth và đã có ít nhất 1 item -> Chuyển sang hàng mới
        //            if (currentRow.Count > 0 && (currentRowWidth + item.Width) > targetRowWidth)
        //            {
        //                rows.Add(currentRow);
        //                currentRow = new List<DiagramItem>();
        //                currentRowWidth = 0f;
        //            }

        //            currentRow.Add(item);
        //            currentRowWidth += item.Width + horizontalSpacing;
        //        }

        //        if (currentRow.Count > 0)
        //            rows.Add(currentRow);

        //        // 3. Tiến hành xếp vị trí theo các hàng đã phân chia
        //        float y = 0f;

        //        for (int r = 0; r < rows.Count; r++)
        //        {
        //            List<DiagramItem> itemsInRow = rows[r];

        //            // Hàng lẻ đảo ngược chiều (Rắn bò - Snake Flow)
        //            if (r % 2 == 1)
        //                itemsInRow.Reverse();

        //            float x = 0f;
        //            float maxHeightInRow = itemsInRow.Max(i => i.Height);

        //            foreach (DiagramItem item in itemsInRow)
        //            {
        //                item.Position = new DevExpress.Utils.PointFloat(x, y);
        //                x += item.Width + horizontalSpacing;
        //            }

        //            y += maxHeightInRow + verticalSpacing;
        //        }
        //    }
        private static void ArrangeRootItemsAdaptive(
     DiagramControl diagram,
     List<DiagramItem> rootItems,
     Dictionary<string, NodeDef> nodes,
     List<EdgeDef> edges,
     Dictionary<GroupDef, DiagramContainer> containerMap,
     DiagramLayoutDirection direction,
     float horizontalSpacing,
     float verticalSpacing)
        {
            if (diagram == null ||
                rootItems == null ||
                rootItems.Count == 0)
            {
                return;
            }

            var ctx = BuildRootLayoutContext(
                diagram,
                rootItems,
                nodes,
                edges,
                containerMap,
                direction,
                horizontalSpacing,
                verticalSpacing);

            BuildRootGraph(ctx);

            ComputeRootLayers(ctx);

            OrderRootLayers(ctx);

            ctx.MainPath =
                FindMainPath(ctx);

            ctx.MainPathIndex =
                BuildMainPathIndex(ctx.MainPath);

            CalculateAdaptiveSpacing(ctx);

            if (ctx.Direction ==
                DiagramLayoutDirection.LeftToRight)
            {
                LayoutLeftToRight(ctx);
            }
            else
            {
                LayoutTopToBottom(ctx);
            }

            NormalizeRootLayout(ctx);
        }
        private static void LayoutLeftToRight(
    RootLayoutContext ctx)
        {
            // Main path: trái -> phải
            LayoutMainPath(ctx);

            // Branch: trên / dưới main path
            LayoutBranches(ctx);

            // Chỉ xử lý overlap nhẹ.
            ResolveBranchOverlapsLeftToRight(ctx);

            UpdateGraphSize(ctx);
        }
        private static void ResolveBranchOverlapsLeftToRight(
    RootLayoutContext ctx)
        {
            if (ctx == null ||
                ctx.RootItemMap == null ||
                ctx.MainPath == null)
            {
                return;
            }

            const float padding = 15f;
            const float shift = 25f;

            // ------------------------------------------------------------
            // Chỉ xử lý branch.
            // MainPath tuyệt đối không di chuyển.
            // ------------------------------------------------------------
            var branches =
                ctx.RootItemMap
                    .Where(x =>
                        !ctx.MainPathIndex.ContainsKey(x.Key))
                    .Select(x => x.Value)
                    .ToList();

            foreach (DiagramItem branch in branches)
            {
                // --------------------------------------------------------
                // Tối đa 6 lần.
                // Không được dùng 30 lần như trước.
                // --------------------------------------------------------
                for (int pass = 0; pass < 6; pass++)
                {
                    DiagramItem collision = null;

                    foreach (DiagramItem other
                             in ctx.RootItemMap.Values)
                    {
                        if (ReferenceEquals(
                                branch,
                                other))
                        {
                            continue;
                        }

                        if (IsOverlapping(
                                branch,
                                other,
                                padding))
                        {
                            collision = other;
                            break;
                        }
                    }

                    if (collision == null)
                        break;

                    // ----------------------------------------------------
                    // Nếu branch đụng MAIN:
                    //
                    // LR => đẩy branch theo chiều DỌC.
                    //
                    // Tuyệt đối không đẩy branch theo X.
                    // ----------------------------------------------------
                    if (IsMainPathItem(
                            ctx,
                            collision))
                    {
                        float branchCenterY =
                            branch.Position.Y +
                            branch.Height / 2f;

                        float mainCenterY =
                            collision.Position.Y +
                            collision.Height / 2f;

                        if (branchCenterY < mainCenterY)
                        {
                            branch.Position =
                                new DevExpress.Utils.PointFloat(
                                    branch.Position.X,
                                    branch.Position.Y -
                                    shift);
                        }
                        else
                        {
                            branch.Position =
                                new DevExpress.Utils.PointFloat(
                                    branch.Position.X,
                                    branch.Position.Y +
                                    shift);
                        }

                        continue;
                    }

                    // ----------------------------------------------------
                    // Branch đụng branch:
                    // đẩy xuống một khoảng nhỏ.
                    // ----------------------------------------------------
                    float newY =
                        collision.Position.Y +
                        collision.Height +
                        ctx.VerticalSpacing;

                    branch.Position =
                        new DevExpress.Utils.PointFloat(
                            branch.Position.X,
                            newY);
                }
            }
        }
        private static void LayoutTopToBottom(
    RootLayoutContext ctx)
        {
            LayoutMainPathVertical(ctx);

            LayoutBranchesTopToBottom(ctx);

            ResolveBranchOverlapsTopToBottom(ctx);

            UpdateGraphSize(ctx);
        }
        private static void LayoutMainPathVertical(
    RootLayoutContext ctx)
        {
            if (ctx == null ||
                ctx.MainPath == null ||
                ctx.MainPath.Count == 0)
            {
                return;
            }

            // ------------------------------------------------------------
            // MainPath nằm giữa theo chiều ngang.
            // ------------------------------------------------------------
            float centerX =
                500f;

            // ------------------------------------------------------------
            // Bắt đầu từ phía trên.
            // ------------------------------------------------------------
            float y = 0f;

            foreach (string key in ctx.MainPath)
            {
                if (!ctx.RootItemMap.TryGetValue(
                        key,
                        out DiagramItem item))
                {
                    continue;
                }

                // --------------------------------------------------------
                // Căn giữa node theo trục X.
                // --------------------------------------------------------
                float x =
                    centerX -
                    item.Width / 2f;

                item.Position =
                    new DevExpress.Utils.PointFloat(
                        x,
                        y);

                // --------------------------------------------------------
                // Node tiếp theo nằm bên dưới.
                // --------------------------------------------------------
                y +=
                    item.Height +
                    ctx.VerticalSpacing;
            }

            ctx.GraphHeight =
                Math.Max(
                    ctx.GraphHeight,
                    y);

            ctx.GraphWidth =
                Math.Max(
                    ctx.GraphWidth,
                    centerX * 2f);
        }
        private static void LayoutBranchesTopToBottom(
    RootLayoutContext ctx)
        {
            if (ctx == null ||
                ctx.Layers == null ||
                ctx.Layers.Count == 0)
            {
                return;
            }

            var leftOffset =
                new Dictionary<string, float>(
                    StringComparer.OrdinalIgnoreCase);

            var rightOffset =
                new Dictionary<string, float>(
                    StringComparer.OrdinalIgnoreCase);

            // ============================================================
            // Xử lý branch theo thứ tự từ trên xuống dưới.
            // ============================================================
            for (int layer = 0;
                 layer <= GetMaxLayer(ctx);
                 layer++)
            {
                if (!ctx.Layers.TryGetValue(
                        layer,
                        out List<string> items))
                {
                    continue;
                }

                foreach (string key in items)
                {
                    // MainPath đã được LayoutMainPathVertical().
                    if (ctx.MainPathIndex.ContainsKey(key))
                        continue;

                    // ----------------------------------------------------
                    // Branch đặt sang trái/phải MainPath.
                    // ----------------------------------------------------
                    PlaceBranchTopToBottom(
                        ctx,
                        key,
                        leftOffset,
                        rightOffset);
                }
            }
        }
        private static bool IsOverlapping(
    DiagramItem a,
    DiagramItem b,
    float padding)
        {
            if (a == null || b == null)
                return false;

            float aLeft =
                a.Position.X - padding;

            float aTop =
                a.Position.Y - padding;

            float aRight =
                a.Position.X +
                a.Width +
                padding;

            float aBottom =
                a.Position.Y +
                a.Height +
                padding;

            float bLeft =
                b.Position.X - padding;

            float bTop =
                b.Position.Y - padding;

            float bRight =
                b.Position.X +
                b.Width +
                padding;

            float bBottom =
                b.Position.Y +
                b.Height +
                padding;

            return
                aLeft < bRight &&
                aRight > bLeft &&
                aTop < bBottom &&
                aBottom > bTop;
        }
        private static DiagramItem FindOverlappingItem(
    RootLayoutContext ctx,
    DiagramItem item,
    float padding)
        {
            foreach (DiagramItem other
                     in ctx.RootItemMap.Values)
            {
                if (ReferenceEquals(
                        item,
                        other))
                {
                    continue;
                }

                if (IsOverlapping(
                        item,
                        other,
                        padding))
                {
                    return other;
                }
            }

            return null;
        }
        private static void ResolveBranchOverlaps(
    RootLayoutContext ctx)
        {
            if (ctx == null ||
                ctx.RootItemMap == null)
            {
                return;
            }

            const float padding = 25f;
            const float shift = 35f;

            // ------------------------------------------------------------
            // MainPath được xem là FIXED.
            // Chỉ di chuyển branch.
            // ------------------------------------------------------------

            var movable =
                ctx.RootItemMap
                    .Where(x =>
                        !ctx.MainPathIndex.ContainsKey(x.Key))
                    .Select(x => x.Value)
                    .ToList();

            if (movable.Count == 0)
                return;


            // ------------------------------------------------------------
            // Nhiều vòng vì một node có thể bị đẩy
            // và sau đó lại đụng node khác.
            // ------------------------------------------------------------

            for (int pass = 0;
                 pass < 30;
                 pass++)
            {
                bool changed = false;

                foreach (DiagramItem item in movable)
                {
                    DiagramItem collision =
                        FindOverlappingItem(
                            ctx,
                            item,
                            padding);

                    if (collision == null)
                        continue;


                    // ----------------------------------------------------
                    // Không di chuyển main path.
                    // ----------------------------------------------------

                    if (IsMainPathItem(
                            ctx,
                            collision))
                    {
                        MoveBranchAwayFromMain(
                            ctx,
                            item,
                            collision,
                            shift);

                        changed = true;
                        continue;
                    }


                    // ----------------------------------------------------
                    // Hai branch đụng nhau.
                    // Đẩy item hiện tại xuống.
                    // ----------------------------------------------------

                    float newY =
                        collision.Position.Y +
                        collision.Height +
                        shift;

                    item.Position =
                        new DevExpress.Utils.PointFloat(
                            item.Position.X,
                            newY);

                    changed = true;
                }

                if (!changed)
                    break;
            }
        }
        private static bool IsMainPathItem(
    RootLayoutContext ctx,
    DiagramItem item)
        {
            foreach (string key in ctx.MainPath)
            {
                if (ctx.RootItemMap.TryGetValue(
                        key,
                        out DiagramItem mainItem))
                {
                    if (ReferenceEquals(
                            mainItem,
                            item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private static void MoveBranchAwayFromMain(
    RootLayoutContext ctx,
    DiagramItem branch,
    DiagramItem main,
    float distance)
        {
            float branchCenterX =
                branch.Position.X +
                branch.Width / 2f;

            float mainCenterX =
                main.Position.X +
                main.Width / 2f;

            float branchCenterY =
                branch.Position.Y +
                branch.Height / 2f;

            float mainCenterY =
                main.Position.Y +
                main.Height / 2f;


            // ------------------------------------------------------------
            // Branch nằm bên trái/phải main
            // ------------------------------------------------------------

            if (Math.Abs(
                    branchCenterX -
                    mainCenterX)
                >
                Math.Abs(
                    branchCenterY -
                    mainCenterY))
            {
                if (branchCenterX < mainCenterX)
                {
                    branch.Position =
                        new DevExpress.Utils.PointFloat(
                            branch.Position.X -
                            distance,
                            branch.Position.Y);
                }
                else
                {
                    branch.Position =
                        new DevExpress.Utils.PointFloat(
                            branch.Position.X +
                            distance,
                            branch.Position.Y);
                }

                return;
            }


            // ------------------------------------------------------------
            // Branch nằm trên/dưới main
            // ------------------------------------------------------------

            if (branchCenterY < mainCenterY)
            {
                branch.Position =
                    new DevExpress.Utils.PointFloat(
                        branch.Position.X,
                        branch.Position.Y -
                        distance);
            }
            else
            {
                branch.Position =
                    new DevExpress.Utils.PointFloat(
                        branch.Position.X,
                        branch.Position.Y +
                        distance);
            }
        }
        private static void PlaceBranchTopToBottom(
         RootLayoutContext ctx,
         string key,
         Dictionary<string, float> leftOffset,
         Dictionary<string, float> rightOffset)
        {
            if (!ctx.RootItemMap.TryGetValue(
                    key,
                    out DiagramItem item))
            {
                return;
            }

            // ------------------------------------------------------------
            // Tìm node main gần nhất.
            // ------------------------------------------------------------

            string anchorKey =
                FindNearestMainNode(
                    ctx,
                    key);

            if (anchorKey == null)
                return;

            if (!ctx.RootItemMap.TryGetValue(
                    anchorKey,
                    out DiagramItem anchor))
            {
                return;
            }


            // ------------------------------------------------------------
            // Xác định branch nằm bên trái hay bên phải.
            // ------------------------------------------------------------

            bool placeLeft =
                ShouldPlaceBranchLeft(
                    ctx,
                    key,
                    anchorKey);


            // ------------------------------------------------------------
            // Khoảng cách từ main node ra branch.
            // ------------------------------------------------------------

            float baseDistance =
                ctx.HorizontalSpacing;


            // ------------------------------------------------------------
            // LEFT
            // ------------------------------------------------------------

            if (placeLeft)
            {
                float used =
                    leftOffset.TryGetValue(
                        anchorKey,
                        out float current)
                        ? current
                        : 0f;

                float x =
                    anchor.Position.X -
                    baseDistance -
                    item.Width -
                    used;

                float y =
                    anchor.Position.Y +
                    anchor.Height / 2f -
                    item.Height / 2f;

                item.Position =
                    new DevExpress.Utils.PointFloat(
                        x,
                        y);

                leftOffset[anchorKey] =
                    used +
                    item.Width +
                    ctx.HorizontalSpacing;
            }

            // ------------------------------------------------------------
            // RIGHT
            // ------------------------------------------------------------

            else
            {
                float used =
                    rightOffset.TryGetValue(
                        anchorKey,
                        out float current)
                        ? current
                        : 0f;

                float x =
                    anchor.Position.X +
                    anchor.Width +
                    baseDistance +
                    used;

                float y =
                    anchor.Position.Y +
                    anchor.Height / 2f -
                    item.Height / 2f;

                item.Position =
                    new DevExpress.Utils.PointFloat(
                        x,
                        y);

                rightOffset[anchorKey] =
                    used +
                    item.Width +
                    ctx.HorizontalSpacing;
            }
        }
        private static float Clamp(
    float value,
    float min,
    float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
        private static void UpdateGraphSize(
    RootLayoutContext ctx)
        {
            float maxRight = 0f;
            float maxBottom = 0f;

            foreach (DiagramItem item
                     in ctx.RootItemMap.Values)
            {
                maxRight =
                    Math.Max(
                        maxRight,
                        item.Position.X +
                        item.Width);

                maxBottom =
                    Math.Max(
                        maxBottom,
                        item.Position.Y +
                        item.Height);
            }

            ctx.GraphWidth =
                maxRight;

            ctx.GraphHeight =
                maxBottom;
        }
        private static bool ShouldPlaceBranchLeft(
    RootLayoutContext ctx,
    string key,
    string anchorKey)
        {
            if (!ctx.Adjacency.TryGetValue(
                    anchorKey,
                    out List<string> nexts))
            {
                return true;
            }

            int index =
                nexts.FindIndex(
                    x => string.Equals(
                        x,
                        key,
                        StringComparison.OrdinalIgnoreCase));

            if (index < 0)
            {
                // Fallback ổn định
                return true;
            }

            // ------------------------------------------------------------
            // Nhánh đầu tiên bên trái
            // Nhánh thứ hai bên phải
            // Nhánh thứ ba bên trái...
            // ------------------------------------------------------------

            return (index % 2) == 0;
        }
        private static void LayoutMainPath(
    RootLayoutContext ctx)
        {
            if (ctx.MainPath.Count == 0)
                return;

            float x = 0f;

            float centerY =
                300f;

            foreach (string key in ctx.MainPath)
            {
                if (!ctx.RootItemMap.TryGetValue(
                        key,
                        out DiagramItem item))
                {
                    continue;
                }

                item.Position =
                    new DevExpress.Utils.PointFloat(
                        x,
                        centerY -
                        item.Height / 2f);

                x +=
                    item.Width +
                    ctx.HorizontalSpacing;
            }

            ctx.GraphWidth =
                Math.Max(
                    ctx.GraphWidth,
                    x);
        }
        private static RootLayoutContext BuildRootLayoutContext(
    DiagramControl diagram,
    List<DiagramItem> rootItems,
    Dictionary<string, NodeDef> nodes,
    List<EdgeDef> edges,
    Dictionary<GroupDef, DiagramContainer> containerMap,
    DiagramLayoutDirection direction,
    float horizontalSpacing,
    float verticalSpacing)
        {
            var ctx = new RootLayoutContext
            {
                Diagram = diagram,
                RootItems = rootItems,
                Nodes = nodes,
                Edges = edges,
                ContainerMap = containerMap,

                Direction = direction,

                ViewportWidth =
                    Math.Max(800f, diagram.ClientSize.Width),

                ViewportHeight =
                    Math.Max(500f, diagram.ClientSize.Height),

                HorizontalSpacing =
                    Math.Max(40f, horizontalSpacing),

                VerticalSpacing =
                    Math.Max(40f, verticalSpacing)
            };

            ctx.ViewportAspect =
                ctx.ViewportWidth /
                Math.Max(1f, ctx.ViewportHeight);

            BuildRootItemMap(ctx);

            BuildNodeRootMap(ctx);

            return ctx;
        }
        private static void BuildRootItemMap(
    RootLayoutContext ctx)
        {
            foreach (DiagramItem item in ctx.RootItems)
            {
                string key =
                    GetRootItemKey(
                        item,
                        ctx.ContainerMap);

                if (string.IsNullOrEmpty(key))
                    continue;

                if (!ctx.RootItemMap.ContainsKey(key))
                    ctx.RootItemMap[key] = item;

                ctx.RootKeys.Add(key);
            }
        }
        private static string GetRootItemKey(
    DiagramItem item,
    Dictionary<GroupDef, DiagramContainer> containerMap)
        {
            if (item is DiagramShape shape)
                return shape.Tag as string;

            if (item is DiagramContainer container)
            {
                foreach (var pair in containerMap)
                {
                    if (ReferenceEquals(pair.Value, container))
                        return "GROUP::" + pair.Key.Id;
                }
            }

            return null;
        }
        private static void BuildNodeRootMap(
    RootLayoutContext ctx)
        {
            var groupById =
                ctx.ContainerMap.Keys
                    .Where(g =>
                        g != null &&
                        !string.IsNullOrEmpty(g.Id))
                    .GroupBy(
                        g => g.Id,
                        StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First(),
                        StringComparer.OrdinalIgnoreCase);

            foreach (NodeDef node in ctx.Nodes.Values)
            {
                if (node == null)
                    continue;

                if (string.IsNullOrEmpty(node.GroupId))
                {
                    ctx.NodeRootKey[node.Id] = node.Id;
                    continue;
                }

                if (!groupById.TryGetValue(
                        node.GroupId,
                        out GroupDef group))
                {
                    ctx.NodeRootKey[node.Id] = node.Id;
                    continue;
                }

                GroupDef rootGroup = group;

                while (rootGroup.Parent != null)
                    rootGroup = rootGroup.Parent;

                ctx.NodeRootKey[node.Id] =
                    "GROUP::" + rootGroup.Id;
            }
        }
        private static void BuildRootGraph(
    RootLayoutContext ctx)
        {
            foreach (string key in ctx.RootKeys)
            {
                ctx.Adjacency[key] =
                    new List<string>();

                ctx.ReverseAdjacency[key] =
                    new List<string>();
            }

            foreach (EdgeDef edge in ctx.Edges)
            {
                if (edge == null)
                    continue;

                if (!ctx.NodeRootKey.TryGetValue(
                        edge.Source,
                        out string sourceRoot))
                {
                    continue;
                }

                if (!ctx.NodeRootKey.TryGetValue(
                        edge.Target,
                        out string targetRoot))
                {
                    continue;
                }

                if (string.Equals(
                        sourceRoot,
                        targetRoot,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!ctx.RootKeys.Contains(sourceRoot) ||
                    !ctx.RootKeys.Contains(targetRoot))
                {
                    continue;
                }

                if (!ctx.Adjacency[sourceRoot]
                        .Contains(
                            targetRoot,
                            StringComparer.OrdinalIgnoreCase))
                {
                    ctx.Adjacency[sourceRoot]
                        .Add(targetRoot);
                }

                if (!ctx.ReverseAdjacency[targetRoot]
                        .Contains(
                            sourceRoot,
                            StringComparer.OrdinalIgnoreCase))
                {
                    ctx.ReverseAdjacency[targetRoot]
                        .Add(sourceRoot);
                }
            }
        }
        private static void ComputeRootLayers(
    RootLayoutContext ctx)
        {
            var inDegree =
                ctx.RootKeys.ToDictionary(
                    key => key,
                    key => 0,
                    StringComparer.OrdinalIgnoreCase);

            foreach (string source in ctx.RootKeys)
            {
                foreach (string target
                         in ctx.Adjacency[source])
                {
                    if (inDegree.ContainsKey(target))
                        inDegree[target]++;
                }
            }

            foreach (string key in ctx.RootKeys)
                ctx.Layer[key] = 0;

            var queue =
                new Queue<string>(
                    inDegree
                        .Where(x => x.Value == 0)
                        .Select(x => x.Key));

            while (queue.Count > 0)
            {
                string current =
                    queue.Dequeue();

                foreach (string next
                         in ctx.Adjacency[current])
                {
                    ctx.Layer[next] =
                        Math.Max(
                            ctx.Layer[next],
                            ctx.Layer[current] + 1);

                    inDegree[next]--;

                    if (inDegree[next] == 0)
                        queue.Enqueue(next);
                }
            }

            // ------------------------------------------------------------
            // Gom layer
            // ------------------------------------------------------------

            ctx.Layers.Clear();

            foreach (string key in ctx.RootKeys)
            {
                int layer =
                    ctx.Layer.TryGetValue(
                        key,
                        out int value)
                        ? value
                        : 0;

                if (!ctx.Layers.TryGetValue(
                        layer,
                        out List<string> list))
                {
                    list = new List<string>();
                    ctx.Layers[layer] = list;
                }

                list.Add(key);
            }
        }
        private static void OrderRootLayers(
    RootLayoutContext ctx)
        {
            for (int pass = 0; pass < 4; pass++)
            {
                for (int layer = 0;
                     layer <= GetMaxLayer(ctx);
                     layer++)
                {
                    if (!ctx.Layers.TryGetValue(
                            layer,
                            out List<string> items))
                    {
                        continue;
                    }

                    if (items.Count <= 1)
                        continue;

                    var previousOrder =
                        BuildLayerOrder(
                            ctx.Layers,
                            layer - 1);

                    var nextOrder =
                        BuildLayerOrder(
                            ctx.Layers,
                            layer + 1);

                    items.Sort(
                        (a, b) =>
                        {
                            double ba =
                                CalculateBarycenter(
                                    a,
                                    ctx.ReverseAdjacency,
                                    ctx.Adjacency,
                                    previousOrder,
                                    nextOrder);

                            double bb =
                                CalculateBarycenter(
                                    b,
                                    ctx.ReverseAdjacency,
                                    ctx.Adjacency,
                                    previousOrder,
                                    nextOrder);

                            return ba.CompareTo(bb);
                        });
                }
            }
        }
        private static int GetMaxLayer(
    RootLayoutContext ctx)
        {
            return ctx.Layers.Count == 0
                ? 0
                : ctx.Layers.Keys.Max();
        }
        private static List<string> FindMainPath(
    RootLayoutContext ctx)
        {
            var bestPaths =
                new Dictionary<string, List<string>>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (string key in ctx.RootKeys)
            {
                bestPaths[key] =
                    new List<string> { key };
            }

            foreach (string key in ctx.RootKeys
                         .OrderBy(k => ctx.Layer[k]))
            {
                foreach (string next
                         in ctx.Adjacency[key])
                {
                    var candidate =
                        new List<string>(
                            bestPaths[key])
                        {
                    next
                        };

                    if (!bestPaths.TryGetValue(
                            next,
                            out List<string> current) ||
                        candidate.Count > current.Count)
                    {
                        bestPaths[next] = candidate;
                    }
                }
            }

            string end =
                bestPaths
                    .OrderByDescending(
                        x => x.Value.Count)
                    .Select(x => x.Key)
                    .FirstOrDefault();

            if (end == null)
                return new List<string>();

            return bestPaths[end];
        }
        private static Dictionary<string, int> BuildMainPathIndex(
    List<string> mainPath)
        {
            var result =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < mainPath.Count; i++)
                result[mainPath[i]] = i;

            return result;
        }
        private static void CalculateAdaptiveSpacing(
    RootLayoutContext ctx)
        {
            float graphAspect =
                CalculateGraphAspect(ctx);

            if (graphAspect <= 0)
                return;

            if (graphAspect < ctx.ViewportAspect)
            {
                ctx.HorizontalSpacing *=
                    Math.Min(
                        3.5f,
                        ctx.ViewportAspect /
                        Math.Max(
                            0.2f,
                            graphAspect));
            }
            else if (graphAspect > ctx.ViewportAspect)
            {
                ctx.VerticalSpacing *=
                    Math.Min(
                        3.5f,
                        graphAspect /
                        Math.Max(
                            0.2f,
                            ctx.ViewportAspect));
            }

            ctx.HorizontalSpacing =
                Clamp(
                    ctx.HorizontalSpacing,
                    40f,
                    260f);

            ctx.VerticalSpacing =
                Clamp(
                    ctx.VerticalSpacing,
                    40f,
                    260f);
        }
        private static float CalculateGraphAspect(
    RootLayoutContext ctx)
        {
            float width = 0f;
            float height = 0f;

            foreach (DiagramItem item
                     in ctx.RootItemMap.Values)
            {
                width += item.Width;
                height += item.Height;
            }

            if (height <= 0)
                return 0;

            return width / height;
        }
        private static Dictionary<string, int> BuildLayerOrder(
    Dictionary<int, List<string>> layers,
    int layer)
        {
            var result =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            if (!layers.TryGetValue(
                    layer,
                    out List<string> items))
            {
                return result;
            }

            for (int i = 0; i < items.Count; i++)
            {
                result[items[i]] = i;
            }

            return result;
        }
        private static double CalculateBarycenter(
    string key,
    Dictionary<string, List<string>> reverseAdjacency,
    Dictionary<string, List<string>> adjacency,
    Dictionary<string, int> previousOrder,
    Dictionary<string, int> nextOrder)
        {
            double sum = 0.0;
            int count = 0;

            // ------------------------------------------------------------
            // Predecessors
            // ------------------------------------------------------------

            if (reverseAdjacency.TryGetValue(
                    key,
                    out List<string> predecessors))
            {
                foreach (string predecessor in predecessors)
                {
                    if (previousOrder.TryGetValue(
                            predecessor,
                            out int index))
                    {
                        sum += index;
                        count++;
                    }
                }
            }


            // ------------------------------------------------------------
            // Successors
            // ------------------------------------------------------------

            if (adjacency.TryGetValue(
                    key,
                    out List<string> successors))
            {
                foreach (string successor in successors)
                {
                    if (nextOrder.TryGetValue(
                            successor,
                            out int index))
                    {
                        sum += index;
                        count++;
                    }
                }
            }


            if (count == 0)
                return double.MaxValue;

            return sum / count;
        }
        //    private static void ArrangeRootItemsWrapped(
        //List<DiagramItem> orderedItems,   // đã sắp theo thứ tự luồng chính
        //int maxColumnsPerRow,
        //float horizontalSpacing,
        //float verticalSpacing)
        //    {
        //        if (orderedItems.Count == 0) return;

        //        var rows = new List<List<DiagramItem>>();
        //        var currentRow = new List<DiagramItem>();

        //        foreach (DiagramItem item in orderedItems)
        //        {
        //            currentRow.Add(item);
        //            if (currentRow.Count >= maxColumnsPerRow)
        //            {
        //                rows.Add(currentRow);
        //                currentRow = new List<DiagramItem>();
        //            }
        //        }
        //        if (currentRow.Count > 0) rows.Add(currentRow);

        //        float y = 0f;

        //        for (int r = 0; r < rows.Count; r++)
        //        {
        //            List<DiagramItem> itemsInRow = rows[r];

        //            // Hàng lẻ đi ngược chiều (kiểu "rắn bò") để luồng đọc mượt hơn
        //            // khi mắt phải nhảy từ cuối hàng trên xuống đầu hàng dưới.
        //            if (r % 2 == 1)
        //                itemsInRow.Reverse();

        //            float x = 0f;
        //            float maxHeightInRow = itemsInRow.Max(i => i.Height);

        //            foreach (DiagramItem item in itemsInRow)
        //            {
        //                item.Position = new DevExpress.Utils.PointFloat(x, y);
        //                x += item.Width + horizontalSpacing;
        //            }

        //            y += maxHeightInRow + verticalSpacing;
        //        }
        //    }
        private static Dictionary<string, int> ComputeGroupedLayers(
    Dictionary<string, NodeDef> nodes,
    List<EdgeDef> edges)
        {
            string Collapse(NodeDef n) => n.GroupId == null ? n.Id : "GROUP::" + n.GroupId;

            var ids = new HashSet<string>(nodes.Values.Select(Collapse));
            var adjacency = ids.ToDictionary(id => id, id => new List<string>(), StringComparer.OrdinalIgnoreCase);
            var inDegree = ids.ToDictionary(id => id, id => 0, StringComparer.OrdinalIgnoreCase);

            foreach (EdgeDef e in edges)
            {
                if (!nodes.TryGetValue(e.Source, out NodeDef sn) || !nodes.TryGetValue(e.Target, out NodeDef tn))
                    continue;

                string a = Collapse(sn);
                string b = Collapse(tn);
                if (a == b) continue; // cạnh nội bộ trong cùng group -> bỏ qua

                adjacency[a].Add(b);
                inDegree[b]++;
            }

            var layer = ids.ToDictionary(id => id, id => 0, StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
            int guard = ids.Count * ids.Count + 10;

            while (queue.Count > 0 && guard-- > 0)
            {
                string cur = queue.Dequeue();
                foreach (string next in adjacency[cur])
                {
                    if (layer[next] < layer[cur] + 1) layer[next] = layer[cur] + 1;
                    if (--inDegree[next] == 0) queue.Enqueue(next);
                }
            }

            return layer;
        }
        // ================================================================
        // Tính layer (tầng) của từng node bằng longest-path layering,
        // dùng để ước lượng "depth" (số tầng) và "breadth" (số node/tầng rộng nhất).
        // ================================================================
        private static Dictionary<string, int> ComputeNodeLayers(
            Dictionary<string, NodeDef> nodes,
            List<EdgeDef> edges)
        {
            var layer = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var adjacency = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var remainingInDegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (string id in nodes.Keys)
            {
                adjacency[id] = new List<string>();
                remainingInDegree[id] = 0;
                layer[id] = 0;
            }

            foreach (EdgeDef edge in edges)
            {
                if (!nodes.ContainsKey(edge.Source) || !nodes.ContainsKey(edge.Target))
                    continue;

                adjacency[edge.Source].Add(edge.Target);
                remainingInDegree[edge.Target]++;
            }

            var queue = new Queue<string>(
                remainingInDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));

            // Bảo vệ vòng lặp vô hạn nếu mermaid lỡ có chu trình (cycle).
            int guard = nodes.Count * nodes.Count + 10;

            while (queue.Count > 0 && guard-- > 0)
            {
                string current = queue.Dequeue();

                foreach (string next in adjacency[current])
                {
                    if (layer[next] < layer[current] + 1)
                        layer[next] = layer[current] + 1;

                    remainingInDegree[next]--;

                    if (remainingInDegree[next] == 0)
                        queue.Enqueue(next);
                }
            }

            return layer;
        }

        private static DiagramLayoutDirection? MapMermaidDirection(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            switch (token.Trim().ToUpperInvariant())
            {
                case "TD":
                case "TB":
                    return DiagramLayoutDirection.TopToBottom;
                case "BT":
                    return DiagramLayoutDirection.BottomToTop;
                case "LR":
                    return DiagramLayoutDirection.LeftToRight;
                case "RL":
                    return DiagramLayoutDirection.RightToLeft;
                default:
                    return null;
            }
        }

        // depth > breadth * threshold => sơ đồ "dài mà hẹp" => nên xoay ngang.
        private const double AutoDirectionThreshold = 1.4;

        private static DiagramLayoutDirection DecideLayoutDirection(
     Dictionary<string, NodeDef> nodes,
     List<EdgeDef> edges,
     string mermaidDirectionToken,
     bool autoDetectDirection)
        {
            // ============================================================
            // Mermaid đã chỉ rõ hướng -> LUÔN tôn trọng hướng đó.
            // ============================================================
            DiagramLayoutDirection? mermaidDirection =
                MapMermaidDirection(
                    mermaidDirectionToken);

            if (mermaidDirection.HasValue)
                return mermaidDirection.Value;

            // ============================================================
            // Chỉ AutoDetect khi Mermaid không ghi hướng.
            // ============================================================
            if (!autoDetectDirection)
                return DiagramLayoutDirection.TopToBottom;

            Dictionary<string, int> layers =
                ComputeNodeLayers(
                    nodes,
                    edges);

            if (layers.Count == 0)
                return DiagramLayoutDirection.TopToBottom;

            int depth =
                layers.Values.Max() + 1;

            int breadth =
                layers.Values
                    .GroupBy(v => v)
                    .Max(g => g.Count());

            return
                depth > breadth * AutoDirectionThreshold
                    ? DiagramLayoutDirection.LeftToRight
                    : DiagramLayoutDirection.TopToBottom;
        }
        private static void ResolveBranchOverlapsTopToBottom(
    RootLayoutContext ctx)
        {
            if (ctx == null ||
                ctx.RootItemMap == null ||
                ctx.MainPath == null ||
                ctx.MainPath.Count == 0)
            {
                return;
            }

            const float padding = 15f;
            const float shift = 25f;

            // ------------------------------------------------------------
            // Chỉ lấy các node KHÔNG nằm trên MainPath.
            // MainPath tuyệt đối không được di chuyển.
            // ------------------------------------------------------------
            var branches =
                ctx.RootItemMap
                    .Where(x =>
                        !ctx.MainPathIndex.ContainsKey(x.Key))
                    .Select(x => x.Value)
                    .Where(x => x != null)
                    .ToList();

            // ------------------------------------------------------------
            // Xử lý từng branch.
            // ------------------------------------------------------------
            foreach (DiagramItem branch in branches)
            {
                for (int pass = 0; pass < 8; pass++)
                {
                    DiagramItem collision = null;

                    foreach (DiagramItem other
                             in ctx.RootItemMap.Values)
                    {
                        if (other == null ||
                            ReferenceEquals(branch, other))
                        {
                            continue;
                        }

                        // MainPath không cần xử lý như branch.
                        // Branch chỉ cần tránh đè lên nó.
                        if (IsItemOverlapping(
                                branch,
                                other,
                                padding))
                        {
                            collision = other;
                            break;
                        }
                    }

                    if (collision == null)
                        break;

                    // ====================================================
                    // TOP -> BOTTOM
                    //
                    // MainPath chạy dọc:
                    //
                    //       GD0
                    //        |
                    //       GD2
                    //        |
                    //       GD4
                    //
                    // Branch phải nằm TRÁI / PHẢI.
                    // Không được đẩy Y xuống.
                    // ====================================================

                    if (IsMainPathItem(
                            ctx,
                            collision))
                    {
                        float branchCenterX =
                            branch.Position.X +
                            branch.Width / 2f;

                        float mainCenterX =
                            collision.Position.X +
                            collision.Width / 2f;

                        if (branchCenterX < mainCenterX)
                        {
                            // Branch bên trái -> đẩy tiếp sang trái.
                            branch.Position =
                                new DevExpress.Utils.PointFloat(
                                    branch.Position.X - shift,
                                    branch.Position.Y);
                        }
                        else
                        {
                            // Branch bên phải -> đẩy tiếp sang phải.
                            branch.Position =
                                new DevExpress.Utils.PointFloat(
                                    branch.Position.X + shift,
                                    branch.Position.Y);
                        }

                        continue;
                    }

                    // ====================================================
                    // Branch đụng branch.
                    //
                    // Ưu tiên đẩy branch sang ngang.
                    // ====================================================

                    float branchCenterX2 =
                        branch.Position.X +
                        branch.Width / 2f;

                    float collisionCenterX =
                        collision.Position.X +
                        collision.Width / 2f;

                    if (branchCenterX2 < collisionCenterX)
                    {
                        branch.Position =
                            new DevExpress.Utils.PointFloat(
                                branch.Position.X - shift,
                                branch.Position.Y);
                    }
                    else
                    {
                        branch.Position =
                            new DevExpress.Utils.PointFloat(
                                branch.Position.X + shift,
                                branch.Position.Y);
                    }
                }
            }

            // ------------------------------------------------------------
            // Cập nhật lại graph bounds.
            // ------------------------------------------------------------
            UpdateGraphSize(ctx);
        }
        private static bool IsItemOverlapping(
    DiagramItem a,
    DiagramItem b,
    float padding)
        {
            if (a == null || b == null)
                return false;

            float aLeft =
                a.Position.X - padding;

            float aRight =
                a.Position.X +
                a.Width +
                padding;

            float aTop =
                a.Position.Y - padding;

            float aBottom =
                a.Position.Y +
                a.Height +
                padding;

            float bLeft =
                b.Position.X;

            float bRight =
                b.Position.X +
                b.Width;

            float bTop =
                b.Position.Y;

            float bBottom =
                b.Position.Y +
                b.Height;

            return
                aLeft < bRight &&
                aRight > bLeft &&
                aTop < bBottom &&
                aBottom > bTop;
        }
        private static List<string> NormalizeLines(string mermaidText)
        {
            string normalized = mermaidText
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");

            string[] rawLines = normalized.Split('\n');
            var result = new List<string>();

            foreach (string rawLine in rawLines)
            {
                string line = rawLine.Trim();

                if (line.Length == 0)
                    continue;

                if (line.StartsWith("%%", StringComparison.Ordinal))
                    continue;

                result.Add(line);
            }

            return result;
        }

        private static void ParseMermaid(
            List<string> lines,
            Dictionary<string, NodeDef> nodes,
            List<EdgeDef> edges,
            List<GroupDef> groups,
            Dictionary<string, GroupDef> groupById,
            List<GroupDef> rootGroups,
            Stack<GroupDef> groupStack,
            Dictionary<string, string> styles,
            Dictionary<string, string> classDefs,
            Dictionary<string, string> classApplications)
        {
            foreach (string line in lines)
            {
                string lower = line.ToLowerInvariant();

                if (lower == "graph td" ||
                    lower == "graph tb" ||
                    lower == "graph bt" ||
                    lower == "graph lr" ||
                    lower == "graph rl" ||
                    lower.StartsWith("graph ", StringComparison.Ordinal) ||
                    lower.StartsWith("flowchart ", StringComparison.Ordinal))
                {
                    continue;
                }

                if (lower == "end")
                {
                    if (groupStack.Count > 0)
                        groupStack.Pop();

                    continue;
                }

                // ------------------------------------------------------------
                // subgraph
                // ------------------------------------------------------------
                if (lower.StartsWith("subgraph ", StringComparison.Ordinal))
                {
                    GroupDef group =
                        ParseSubgraph(line, groupStack.Count);

                    if (group != null)
                    {
                        GroupDef parent =
                            groupStack.Count > 0
                                ? groupStack.Peek()
                                : null;

                        group.Parent = parent;

                        if (parent != null)
                            parent.Children.Add(group);
                        else
                            rootGroups.Add(group);

                        groups.Add(group);

                        string uniqueId = group.Id;
                        int suffix = 2;

                        while (groupById.ContainsKey(uniqueId))
                        {
                            uniqueId = group.Id + "_" + suffix;
                            suffix++;
                        }

                        group.Id = uniqueId;
                        groupById[uniqueId] = group;

                        groupStack.Push(group);
                    }

                    continue;
                }

                // ------------------------------------------------------------
                // style
                // ------------------------------------------------------------
                Match styleMatch = StyleRegex.Match(line);

                if (styleMatch.Success)
                {
                    styles[styleMatch.Groups["id"].Value] =
                        styleMatch.Groups["props"].Value;

                    continue;
                }

                // ------------------------------------------------------------
                // classDef
                // ------------------------------------------------------------
                Match classDefMatch = ClassDefRegex.Match(line);

                if (classDefMatch.Success)
                {
                    classDefs[classDefMatch.Groups["name"].Value] =
                        classDefMatch.Groups["props"].Value;

                    continue;
                }

                // ------------------------------------------------------------
                // class A,B className
                // ------------------------------------------------------------
                Match classApplyMatch = ClassApplyRegex.Match(line);

                if (classApplyMatch.Success)
                {
                    string className =
                        classApplyMatch.Groups["name"].Value;

                    string[] ids =
                        classApplyMatch.Groups["ids"].Value.Split(
                            new[] { ',' },
                            StringSplitOptions.RemoveEmptyEntries);

                    foreach (string rawId in ids)
                    {
                        string id = rawId.Trim();

                        if (id.Length > 0)
                            classApplications[id] = className;
                    }

                    continue;
                }

                // ------------------------------------------------------------
                // Node declarations
                // ------------------------------------------------------------
                foreach (Match nodeMatch in NodeRegex.Matches(line))
                {
                    string id = nodeMatch.Groups["id"].Value;

                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    string text = GetNodeText(nodeMatch);

                    if (text == null)
                        continue;

                    if (!nodes.ContainsKey(id))
                    {
                        var node = new NodeDef
                        {
                            Id = id,
                            Text = CleanMermaidText(text),

                            Decision =
                            nodeMatch.Groups["decision"].Success ||
                            nodeMatch.Groups["decisionRaw"].Success,

                                                Circle =
                            nodeMatch.Groups["circle"].Success,

                                                GroupId =
                            groupStack.Count > 0
                                ? groupStack.Peek().Id
                                : null
                        };

                        nodes.Add(id, node);

                        if (groupStack.Count > 0)
                            groupStack.Peek().NodeIds.Add(id);
                    }
                }

                // ------------------------------------------------------------
                // Edges
                // ------------------------------------------------------------
                foreach (Match edgeMatch in EdgeRegex.Matches(line))
                {
                    string source =
                        edgeMatch.Groups["src"].Value;

                    string target =
                        edgeMatch.Groups["dst"].Value;

                    if (string.IsNullOrWhiteSpace(source) ||
                        string.IsNullOrWhiteSpace(target))
                        continue;

                    edges.Add(new EdgeDef
                    {
                        Source = source,
                        Target = target,
                        Label =
                            CleanMermaidText(
                                edgeMatch.Groups["label"].Value),
                        Operator =
                            edgeMatch.Groups["op"].Value
                    });
                }
            }
        }
        private static void LayoutBranches(
    RootLayoutContext ctx)
        {
            for (int layer = 0;
                 layer <= GetMaxLayer(ctx);
                 layer++)
            {
                if (!ctx.Layers.TryGetValue(
                        layer,
                        out List<string> items))
                {
                    continue;
                }

                foreach (string key in items)
                {
                    if (ctx.MainPathIndex.ContainsKey(key))
                        continue;

                    PlaceBranchNode(
                        ctx,
                        key);
                }
            }
        }
        private static void PlaceBranchNode(
     RootLayoutContext ctx,
     string key)
        {
            if (!ctx.RootItemMap.TryGetValue(
                    key,
                    out DiagramItem item))
            {
                return;
            }

            string anchor =
                FindNearestMainNode(
                    ctx,
                    key);

            if (anchor == null ||
                !ctx.RootItemMap.TryGetValue(
                    anchor,
                    out DiagramItem anchorItem))
            {
                return;
            }

            bool placeAbove =
                ShouldPlaceBranchAbove(
                    ctx,
                    key,
                    anchor);

            // ------------------------------------------------------------
            // LR:
            //
            // branch nằm trên / dưới main path.
            // ------------------------------------------------------------

            float x =
                anchorItem.Position.X;

            float y;

            if (placeAbove)
            {
                y =
                    anchorItem.Position.Y -
                    ctx.VerticalSpacing -
                    item.Height;
            }
            else
            {
                y =
                    anchorItem.Position.Y +
                    anchorItem.Height +
                    ctx.VerticalSpacing;
            }

            item.Position =
                new DevExpress.Utils.PointFloat(
                    x,
                    y);
        }
        private static string FindNearestMainNode(
    RootLayoutContext ctx,
    string key)
        {
            if (ctx == null ||
                string.IsNullOrEmpty(key) ||
                ctx.MainPath == null ||
                ctx.MainPath.Count == 0)
            {
                return null;
            }

            // ------------------------------------------------------------
            // Nếu chính node này nằm trên main path
            // ------------------------------------------------------------

            if (ctx.MainPathIndex.ContainsKey(key))
                return key;


            // ------------------------------------------------------------
            // 1. Ưu tiên predecessor nằm trên main path
            //
            // Ví dụ:
            //
            // GD2 → GD4-6 → GD7
            //
            // Nếu GD3 là branch của GD2
            // thì anchor = GD2.
            // ------------------------------------------------------------

            if (ctx.ReverseAdjacency.TryGetValue(
                    key,
                    out List<string> predecessors))
            {
                foreach (string previous in predecessors)
                {
                    if (ctx.MainPathIndex.ContainsKey(previous))
                        return previous;
                }
            }


            // ------------------------------------------------------------
            // 2. Nếu không có predecessor trực tiếp,
            //    tìm predecessor gần nhất trên main path.
            //
            // Ví dụ:
            //
            // GD2 → BranchA → BranchB
            //
            // BranchB vẫn anchor về GD2.
            // ------------------------------------------------------------

            var visited =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            var queue =
                new Queue<string>();

            queue.Enqueue(key);
            visited.Add(key);

            while (queue.Count > 0)
            {
                string current =
                    queue.Dequeue();

                if (!ctx.ReverseAdjacency.TryGetValue(
                        current,
                        out List<string> parents))
                {
                    continue;
                }

                foreach (string parent in parents)
                {
                    if (!visited.Add(parent))
                        continue;

                    if (ctx.MainPathIndex.ContainsKey(parent))
                        return parent;

                    queue.Enqueue(parent);
                }
            }


            // ------------------------------------------------------------
            // 3. Nếu branch không nối ngược được về main path,
            //    tìm main node gần nhất theo layer.
            // ------------------------------------------------------------

            if (ctx.Layer.TryGetValue(
                    key,
                    out int branchLayer))
            {
                string nearest = null;
                int bestDistance = int.MaxValue;

                foreach (string mainNode in ctx.MainPath)
                {
                    if (!ctx.Layer.TryGetValue(
                            mainNode,
                            out int mainLayer))
                    {
                        continue;
                    }

                    int distance =
                        Math.Abs(
                            mainLayer -
                            branchLayer);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        nearest = mainNode;
                    }
                }

                if (nearest != null)
                    return nearest;
            }


            // ------------------------------------------------------------
            // 4. Fallback:
            //    dùng main node đầu tiên.
            // ------------------------------------------------------------

            return ctx.MainPath[0];
        }
        private static bool ShouldPlaceBranchAbove(
    RootLayoutContext ctx,
    string key,
    string anchor)
        {
            if (!ctx.Adjacency.TryGetValue(
                    anchor,
                    out List<string> nexts))
            {
                return false;
            }

            int index =
                nexts.FindIndex(
                    x => string.Equals(
                        x,
                        key,
                        StringComparison.OrdinalIgnoreCase));

            return index % 2 == 0;
        }
        private static void NormalizeRootLayout(
    RootLayoutContext ctx)
        {
            if (ctx.RootItemMap.Count == 0)
                return;

            float minX = float.MaxValue;
            float minY = float.MaxValue;

            foreach (DiagramItem item
                     in ctx.RootItemMap.Values)
            {
                minX =
                    Math.Min(
                        minX,
                        item.Position.X);

                minY =
                    Math.Min(
                        minY,
                        item.Position.Y);
            }

            const float padding = 40f;

            foreach (DiagramItem item
                     in ctx.RootItemMap.Values)
            {
                item.Position =
                    new DevExpress.Utils.PointFloat(
                        item.Position.X -
                        minX +
                        padding,

                        item.Position.Y -
                        minY +
                        padding);
            }
        }
        private static GroupDef ParseSubgraph(
            string line,
            int depth)
        {
            Match match = SubgraphRegex.Match(line);

            if (!match.Success)
                return null;

            string id;
            string title;

            if (match.Groups["id"].Success)
            {
                id = match.Groups["id"].Value;

                title =
                    match.Groups["title"].Success
                        ? match.Groups["title"].Value
                        : id;
            }
            else
            {
                string plain =
                    match.Groups["plain"].Value.Trim();

                if (plain.Length >= 2 &&
                    plain[0] == '"' &&
                    plain[plain.Length - 1] == '"')
                {
                    title =
                        plain.Substring(
                            1,
                            plain.Length - 2);

                    id =
                        "subgraph_" +
                        Math.Abs(title.GetHashCode());
                }
                else
                {
                    title = plain;
                    id = plain;
                }
            }

            return new GroupDef
            {
                Id = id,
                Title = CleanMermaidText(title),
                Depth = depth
            };
        }

        private static string GetNodeText(Match match)
        {
            string[] names =
            {
            "circle",
            "decision",
            "rect",
            "round",
            "roundRaw",
            "rectRaw",
            "decisionRaw"
        };

            foreach (string name in names)
            {
                Group group = match.Groups[name];

                if (group.Success)
                    return group.Value;
            }

            return null;
        }

        private static float CalculateNodeWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 180.0f;

            // Đo độ dài dòng thực tế
            string[] lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int maxLength = lines.Length > 0 ? lines.Max(x => x.Length) : 0;

            // Tăng khung rộng hơn để chứa đủ text tiếng Việt và Icon/Emoji
            if (maxLength >= 60) return 320.0f;
            if (maxLength >= 40) return 260.0f;
            if (maxLength >= 25) return 220.0f;

            return 180.0f;
        }

        private static float CalculateNodeHeight(string text, float width)
        {
            if (string.IsNullOrEmpty(text))
                return 65.0f;

            float usableWidth = Math.Max(40f, width - 40f);
            SizeF measured = MeasureWrappedText(text, NodeFont, usableWidth);

            // Ước lượng số dòng tối thiểu dựa trên số ký tự xuống dòng thủ công
            // (từ <br/> trong mermaid) — đảm bảo không bao giờ thấp hơn mức này.
            int explicitLineCount = text.Split('\n').Length;
            float minHeightForLines = (explicitLineCount * NodeFont.Height * 1.4f) + 48.0f;

            float height = Math.Max((measured.Height * 1.4f) + 48.0f, minHeightForLines);

            if (height < 65.0f) height = 65.0f;

            return height;
        }

        private static DiagramShape CreateShape(NodeDef node)
        {
            ShapeDescription shapeType;

            if (node.Circle)
            {
                shapeType = BasicShapes.Ellipse;
            }
            else if (node.Decision)
            {
                shapeType = BasicFlowchartShapes.Decision;
            }
            else
            {
                shapeType = BasicShapes.Rectangle;
            }

            float width;
            float height;

            if (node.Circle)
            {
                // Start / End node:
                // luôn là hình vuông để Ellipse hiển thị thành hình tròn.
                float diameter =
                    Math.Max(
                        90f,
                        Math.Min(
                            150f,
                            CalculateNodeWidth(node.Text)));

                width = diameter;
                height = diameter;
            }
            else
            {
                width = CalculateNodeWidth(node.Text);
                height = CalculateNodeHeight(node.Text, width);
            }

            var shape = new DiagramShape
            {
                Shape = shapeType,
                Content = node.Text,
                Tag = node.Id,
                Width = width,
                Height = height
            };

            shape.Appearance.Font =
                new Font(
                    "Segoe UI Emoji",
                    9.0f,
                    FontStyle.Regular);

            shape.Appearance.Options.UseFont = true;

            shape.Appearance.TextOptions.WordWrap =
                WordWrap.Wrap;

            shape.Appearance.TextOptions.HAlignment =
                HorzAlignment.Center;

            shape.Appearance.TextOptions.VAlignment =
                VertAlignment.Center;

            shape.Appearance.Options.UseTextOptions = true;

            // ------------------------------------------------------------
            // Form node
            // ------------------------------------------------------------

            bool isFormNode =
                !node.Decision &&
                !node.Circle &&
                FormNameRegex.IsMatch(node.Text);

            if (isFormNode)
            {
                shape.Appearance.BackColor =
                    Color.FromArgb(232, 245, 255);

                shape.Appearance.BorderColor =
                    Color.FromArgb(21, 101, 192);

                shape.Appearance.BorderSize = 2;
            }
            else
            {
                shape.Appearance.BackColor =
                    Color.White;

                shape.Appearance.BorderColor =
                    Color.FromArgb(90, 90, 90);

                shape.Appearance.BorderSize = 1;
            }

            shape.Appearance.ForeColor =
                GetContrastingTextColor(
                    shape.Appearance.BackColor);

            shape.Appearance.Options.UseForeColor = true;
            shape.Appearance.Options.UseBackColor = true;
            shape.Appearance.Options.UseBorderColor = true;
            shape.Appearance.Options.UseBorderSize = true;

            return shape;
        }
        /// <summary>
        /// Trích tên class Form (vd "FormNhapPhieuTraHangKhach") từ nội dung 1 shape
        /// trên diagram, nếu node đó là "form thao tác" thật sự. Trả về null nếu
        /// không phải (node quyết định, trạng thái kết thúc, ghi chú...).
        /// </summary>
        public static string TryGetFormName(DiagramShape shape)
        {
            if (shape == null || string.IsNullOrEmpty(shape.Content))
                return null;

            Match match = FormNameRegex.Match(shape.Content);
            return match.Success ? match.Value : null;
        }
        private static DiagramConnector CreateConnector(
            DiagramShape source,
            DiagramShape target,
            EdgeDef edge)
        {
            ConnectorType type =
                ConnectorType.RightAngle;

            if (edge.Operator == "-.->" ||
                edge.Operator == "-.-")
            {
                type = ConnectorType.Curved;
            }
            else if (edge.Operator == "---")
            {
                type = ConnectorType.Straight;
            }

            var connector =
                new DiagramConnector(
                    type,
                    source,
                    target);

            connector.Content = edge.Label;

            connector.EndArrow =
                ArrowDescriptions.Filled90;

            connector.Appearance.Font = new Font("Segoe UI Emoji", 8.5f, FontStyle.Regular);
            connector.Appearance.Options.UseFont = true;

            connector.Appearance.ForeColor = Color.FromArgb(60, 60, 60);
            connector.Appearance.Options.UseForeColor = true;

            connector.Appearance.BackColor = Color.White;
            connector.Appearance.Options.UseBackColor = true;

            // IMPORTANT:
            // Do not use:
            // connector.Appearance.BorderDashPattern.Add(...)
            //
            // DiagramDoubleCollection has no Add() in this API.
            if (edge.Operator == "-.->" ||
                edge.Operator == "-.-")
            {
                connector.Appearance.BorderDashPattern =
                    new DiagramDoubleCollection(
                        new double[] { 4.0, 3.0 });

                connector.Appearance.Options.UseBorderDashPattern =
                    true;
            }

            if (edge.Operator == "==>")
            {
                connector.Appearance.BorderSize = 2;
                connector.Appearance.Options.UseBorderSize = true;
            }

            return connector;
        }

        private static Color GetContrastingTextColor(Color background)
        {
            double luminance =
                (0.299 * background.R + 0.587 * background.G + 0.114 * background.B) / 255.0;

            return luminance > 0.55
                ? Color.FromArgb(33, 33, 33)   // nền sáng -> chữ tối
                : Color.White;                  // nền tối -> chữ trắng
        }

       

        private static string CleanMermaidText(
            string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace("<br/>", "\n")
                .Replace("<br />", "\n")
                .Replace("<br>", "\n")
                .Replace("&nbsp;", " ")
                .Trim();
        }

        private static void ApplyStyle(DiagramItem item, string propsRaw)
        {
            if (item == null || string.IsNullOrWhiteSpace(propsRaw))
                return;

            string[] parts = propsRaw.Split(',');
            bool fillChanged = false;
            bool explicitForeColor = false;

            foreach (string rawPart in parts)
            {
                string[] kv = rawPart.Split(new[] { ':' }, 2);
                if (kv.Length != 2) continue;

                string key = kv[0].Trim().ToLowerInvariant();
                string value = kv[1].Trim();

                switch (key)
                {
                    case "fill":
                        if (TryParseMermaidColor(value, out Color fill))
                        {
                            item.Appearance.BackColor = fill;
                            item.Appearance.Options.UseBackColor = true;
                            fillChanged = true;
                        }
                        break;

                    case "stroke":
                        if (TryParseMermaidColor(value, out Color stroke))
                        {
                            item.Appearance.BorderColor = stroke;
                            item.Appearance.Options.UseBorderColor = true;
                        }
                        break;

                    case "color":
                        if (TryParseMermaidColor(value, out Color fore))
                        {
                            item.Appearance.ForeColor = fore;
                            item.Appearance.Options.UseForeColor = true;
                            explicitForeColor = true;
                        }
                        break;

                    case "stroke-width":
                        if (TryParsePixelInt(value, out int borderSize))
                        {
                            item.Appearance.BorderSize = Math.Max(1, borderSize);
                            item.Appearance.Options.UseBorderSize = true;
                        }
                        break;
                }
            }

            // Không có "color:" tường minh nhưng fill đổi -> tự chọn chữ tương phản
            if (fillChanged && !explicitForeColor)
            {
                item.Appearance.ForeColor = GetContrastingTextColor(item.Appearance.BackColor);
                item.Appearance.Options.UseForeColor = true;
            }
        }

        private static bool TryParsePixelInt(
            string text,
            out int value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string clean =
                text
                    .Trim()
                    .ToLowerInvariant()
                    .Replace("px", string.Empty)
                    .Trim();

            return int.TryParse(
                clean,
                out value);
        }

        private static bool TryParseMermaidColor(
            string text,
            out Color color)
        {
            color = Color.Empty;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string value = text.Trim();

            if (value.StartsWith(
                "#",
                StringComparison.Ordinal))
            {
                string hex =
                    value.Substring(1);

                try
                {
                    if (hex.Length == 3)
                    {
                        int r =
                            Convert.ToInt32(
                                new string(hex[0], 2),
                                16);

                        int g =
                            Convert.ToInt32(
                                new string(hex[1], 2),
                                16);

                        int b =
                            Convert.ToInt32(
                                new string(hex[2], 2),
                                16);

                        color =
                            Color.FromArgb(
                                r,
                                g,
                                b);

                        return true;
                    }

                    if (hex.Length == 6)
                    {
                        int r =
                            Convert.ToInt32(
                                hex.Substring(0, 2),
                                16);

                        int g =
                            Convert.ToInt32(
                                hex.Substring(2, 2),
                                16);

                        int b =
                            Convert.ToInt32(
                                hex.Substring(4, 2),
                                16);

                        color =
                            Color.FromArgb(
                                r,
                                g,
                                b);

                        return true;
                    }
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            switch (value.ToLowerInvariant())
            {
                case "white":
                    color = Color.White;
                    return true;

                case "black":
                    color = Color.Black;
                    return true;

                case "red":
                    color = Color.Red;
                    return true;

                case "green":
                    color = Color.Green;
                    return true;

                case "blue":
                    color = Color.Blue;
                    return true;

                case "yellow":
                    color = Color.Yellow;
                    return true;

                case "orange":
                    color = Color.Orange;
                    return true;

                case "gray":
                case "grey":
                    color = Color.Gray;
                    return true;

                case "transparent":
                    color = Color.Transparent;
                    return true;
            }

            return false;
        }
    }
}
