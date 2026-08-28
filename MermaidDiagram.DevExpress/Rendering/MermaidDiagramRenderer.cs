using DevExpress.XtraDiagram;
using MermaidDiagram.Core.Layout;
using MermaidDiagram.Core.Layout.CoreLayout.EdgeRouting;
using MermaidDiagram.Core.Layout.Result;
using MermaidDiagram.Core.Models;



namespace MermaidDiagram.DevExpress.Rendering
{
    /// <summary>
    /// Renderer tổng của Mermaid Diagram.
    ///
    /// Chịu trách nhiệm orchestration:
    ///
    ///     MermaidDocument
    ///          +
    ///     MermaidLayoutResult
    ///          ↓
    ///     NodeRenderer
    ///     GroupRenderer
    ///     EdgeRenderer
    ///          ↓
    ///     DiagramControl
    ///
    /// Renderer KHÔNG tính layout.
    ///
    /// Dummy node:
    ///     - Có tồn tại trong Core layout.
    ///     - Được dùng để ordering / crossing / coordinate / routing.
    ///     - KHÔNG được render thành DiagramShape.
    ///     - EdgeRenderer sử dụng route.Points để đi qua dummy.
    /// </summary>
    public sealed class MermaidDiagramRenderer
        : IMermaidDiagramRenderer
    {
        // ============================================================
        // RENDERERS
        // ============================================================

        private readonly MermaidNodeRenderer
            _nodeRenderer;

        private readonly MermaidEdgeRenderer
            _edgeRenderer;

        private readonly MermaidGroupRenderer
            _groupRenderer;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidDiagramRenderer(
            MermaidNodeRenderer nodeRenderer,
            MermaidEdgeRenderer edgeRenderer,
            MermaidGroupRenderer groupRenderer)
        {
            _nodeRenderer =
                nodeRenderer
                ?? throw new ArgumentNullException(
                    nameof(nodeRenderer));

            _edgeRenderer =
                edgeRenderer
                ?? throw new ArgumentNullException(
                    nameof(edgeRenderer));

            _groupRenderer =
                groupRenderer
                ?? throw new ArgumentNullException(
                    nameof(groupRenderer));
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        public void Render(
            MermaidDocument document,
            MermaidLayoutResult layout,
            DiagramControl diagram)
        {
            if (document == null)
            {
                throw new ArgumentNullException(
                    nameof(document));
            }

            if (layout == null)
            {
                throw new ArgumentNullException(
                    nameof(layout));
            }

            if (diagram == null)
            {
                throw new ArgumentNullException(
                    nameof(diagram));
            }

            // ========================================================
            // CLEAR
            // ========================================================

            diagram.Items.Clear();

            Dictionary<string, DiagramItem> items =
                new Dictionary<string, DiagramItem>(
                    StringComparer.OrdinalIgnoreCase);

            // ========================================================
            // 1. GROUPS
            // ========================================================

            RenderGroups(
                document,
                layout,
                items);

            // ========================================================
            // 2. REAL NODES
            //
            // KHÔNG render dummy node.
            // Dummy chỉ tồn tại trong Core layout/routing.
            // ========================================================

            RenderNodes(
                document,
                layout,
                items);

            // ========================================================
            // 3. GROUP HIERARCHY
            // ========================================================

            BuildGroupHierarchy(
                document,
                items);

            // ========================================================
            // 4. ROOT ITEMS
            // ========================================================

            AddRootItems(
                items,
                diagram);

            // ========================================================
            // 5. EDGES
            //
            // EdgeRenderer nhận EdgeRoute đã được Core tính.
            // Route có thể chứa waypoint dummy.
            // ========================================================

            RenderEdges(
                document,
                layout,
                items,
                diagram);
        }

        // ============================================================
        // GROUPS
        // ============================================================

        private void RenderGroups(
            MermaidDocument document,
            MermaidLayoutResult layout,
            Dictionary<string, DiagramItem> items)
        {
            if (document.Subgraphs == null)
            {
                return;
            }

            foreach (MermaidSubgraph subgraph
                     in document.Subgraphs)
            {
                if (subgraph == null ||
                    string.IsNullOrWhiteSpace(
                        subgraph.Key))
                {
                    continue;
                }

                if (!layout.Items.TryGetValue(
                        subgraph.Key,
                        out MermaidLayoutItem layoutItem))
                {
                    continue;
                }

                /*
                 * Group không bao giờ là dummy.
                 */
                if (layoutItem.IsDummy)
                {
                    continue;
                }

                DiagramContainer container =
                    _groupRenderer.Render(
                        subgraph,
                        layoutItem);

                if (container == null)
                {
                    continue;
                }

                items[subgraph.Key] =
                    container;
            }
        }

        // ============================================================
        // NODES
        // ============================================================

        private void RenderNodes(
            MermaidDocument document,
            MermaidLayoutResult layout,
            Dictionary<string, DiagramItem> items)
        {
            if (document.Nodes == null)
            {
                return;
            }

            foreach (MermaidNode node
                     in document.Nodes)
            {
                if (node == null ||
                    string.IsNullOrWhiteSpace(
                        node.Id))
                {
                    continue;
                }

                if (!layout.Items.TryGetValue(
                        node.Id,
                        out MermaidLayoutItem layoutItem))
                {
                    continue;
                }

                // ====================================================
                // DUMMY NODE
                //
                // Dummy được tạo bởi DummyNodeInserter,
                // không thuộc MermaidDocument.Nodes.
                //
                // Defensive check để tuyệt đối không render dummy.
                // ====================================================

                if (layoutItem.IsDummy)
                {
                    continue;
                }

                DiagramShape shape =
                    _nodeRenderer.Render(
                        node,
                        layoutItem);

                if (shape == null)
                {
                    continue;
                }

                items[node.Id] =
                    shape;
            }
        }

        // ============================================================
        // ROOT ITEMS
        // ============================================================

        private void AddRootItems(
            Dictionary<string, DiagramItem> items,
            DiagramControl diagram)
        {
            foreach (DiagramItem item
                     in items.Values)
            {
                if (item == null)
                {
                    continue;
                }

                bool isChild =
                    IsContainedByAnyGroup(
                        item,
                        items);

                if (isChild)
                {
                    continue;
                }

                diagram.Items.Add(
                    item);
            }
        }

        // ============================================================
        // GROUP HIERARCHY
        // ============================================================

        private void BuildGroupHierarchy(
            MermaidDocument document,
            Dictionary<string, DiagramItem> items)
        {
            if (document.Subgraphs == null)
            {
                return;
            }

            // ========================================================
            // 1. CHILD SUBGRAPHS
            // ========================================================

            foreach (MermaidSubgraph subgraph
                     in document.Subgraphs)
            {
                if (subgraph == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                        subgraph.ParentKey))
                {
                    continue;
                }

                if (!items.TryGetValue(
                        subgraph.ParentKey,
                        out DiagramItem parentItem))
                {
                    continue;
                }

                if (!(parentItem
                      is DiagramContainer parentContainer))
                {
                    continue;
                }

                if (!items.TryGetValue(
                        subgraph.Key,
                        out DiagramItem childItem))
                {
                    continue;
                }

                if (childItem
                    is DiagramContainer childContainer)
                {
                    AddToContainer(
                        parentContainer,
                        childContainer);
                }
            }

            // ========================================================
            // 2. DIRECT NODES
            // ========================================================

            foreach (MermaidSubgraph subgraph
                     in document.Subgraphs)
            {
                if (subgraph == null)
                {
                    continue;
                }

                if (!items.TryGetValue(
                        subgraph.Key,
                        out DiagramItem groupItem))
                {
                    continue;
                }

                if (!(groupItem
                      is DiagramContainer container))
                {
                    continue;
                }

                if (subgraph.NodeKeys == null)
                {
                    continue;
                }

                foreach (string nodeKey
                         in subgraph.NodeKeys)
                {
                    if (string.IsNullOrWhiteSpace(
                            nodeKey))
                    {
                        continue;
                    }

                    if (!items.TryGetValue(
                            nodeKey,
                            out DiagramItem nodeItem))
                    {
                        continue;
                    }

                    AddToContainer(
                        container,
                        nodeItem);
                }
            }
        }

        // ============================================================
        // CONTAINMENT
        // ============================================================

        private bool IsContainedByAnyGroup(
            DiagramItem item,
            Dictionary<string, DiagramItem> items)
        {
            foreach (DiagramItem candidate
                     in items.Values)
            {
                if (!(candidate
                      is DiagramContainer container))
                {
                    continue;
                }

                if (container.Items.Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

        // ============================================================
        // ADD TO CONTAINER
        // ============================================================

        private static void AddToContainer(
            DiagramContainer container,
            DiagramItem item)
        {
            if (container == null ||
                item == null)
            {
                return;
            }

            if (ReferenceEquals(
                    container,
                    item))
            {
                return;
            }

            if (!container.Items.Contains(item))
            {
                container.Items.Add(item);
            }
        }

        // ============================================================
        // EDGES
        // ============================================================

        private void RenderEdges(
            MermaidDocument document,
            MermaidLayoutResult layout,
            Dictionary<string, DiagramItem> items,
            DiagramControl diagram)
        {
            if (document.Edges == null)
            {
                return;
            }

            foreach (MermaidEdge edge
                     in document.Edges)
            {
                if (edge == null)
                {
                    continue;
                }

                // ====================================================
                // SOURCE
                // ====================================================

                if (!items.TryGetValue(
                        edge.Source,
                        out DiagramItem source))
                {
                    continue;
                }

                // ====================================================
                // TARGET
                // ====================================================

                if (!items.TryGetValue(
                        edge.Target,
                        out DiagramItem target))
                {
                    continue;
                }

                // ====================================================
                // ROUTE KEY
                // ====================================================

                string edgeKey =
                    RootLayoutContext.GetEdgeKey(
                        edge);

                if (!layout.EdgeRoutes.TryGetValue(
                        edgeKey,
                        out EdgeRoute route))
                {
                    /*
                     * Renderer không tự route.
                     */
                    continue;
                }

                if (route == null ||
                    route.Points == null ||
                    route.Points.Count < 2)
                {
                    continue;
                }

                // ====================================================
                // EDGE RENDERER
                // ====================================================

                DiagramConnector connector =
                    _edgeRenderer.Render(
                        edge,
                        source,
                        target,
                        route);

                if (connector == null)
                {
                    continue;
                }

                diagram.Items.Add(
                    connector);
            }
        }
    }
}



