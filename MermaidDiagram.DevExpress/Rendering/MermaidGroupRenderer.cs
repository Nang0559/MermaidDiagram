using DevExpress.XtraDiagram;
using MermaidDiagram.Core.Layout.Result;

namespace MermaidDiagram.DevExpress.Rendering
{
    /// <summary>
    /// Render MermaidSubgraph thành DiagramContainer.
    ///
    /// MermaidGroupRenderer chịu trách nhiệm:
    ///     - yêu cầu MermaidDiagramItemFactory tạo group;
    ///     - điều phối group rendering.
    ///
    /// Không chịu trách nhiệm:
    ///     - parse;
    ///     - graph analysis;
    ///     - layout calculation;
    ///     - tạo DiagramContainer trực tiếp;
    ///     - tính bounds;
    ///     - node rendering;
    ///     - edge rendering;
    ///     - interaction;
    ///     - workflow / business logic.
    /// </summary>
    public sealed class MermaidGroupRenderer
    {
        // ============================================================
        // FACTORY
        // ============================================================

        private readonly MermaidDiagramItemFactory _itemFactory;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidGroupRenderer(
            MermaidDiagramItemFactory itemFactory)
        {
            _itemFactory =
                itemFactory
                ?? throw new ArgumentNullException(
                    nameof(itemFactory));
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Render một MermaidSubgraph bằng geometry
        /// đã được MermaidDiagram.Core tính toán.
        /// </summary>
        public DiagramContainer Render(
            MermaidSubgraph subgraph,
            MermaidLayoutItem layoutItem)
        {
            if (subgraph == null)
            {
                throw new ArgumentNullException(
                    nameof(subgraph));
            }

            if (layoutItem == null)
            {
                throw new ArgumentNullException(
                    nameof(layoutItem));
            }

            return _itemFactory.CreateGroup(
                subgraph,
                layoutItem);
        }

        // ============================================================
        // PUBLIC API - WITHOUT LAYOUT
        // ============================================================

        /// <summary>
        /// Tạo group chưa có geometry layout.
        ///
        /// Dùng khi cần tạo DiagramContainer trước,
        /// sau đó mới áp dụng layout ở bước khác.
        /// </summary>
        public DiagramContainer Render(
            MermaidSubgraph subgraph)
        {
            if (subgraph == null)
            {
                throw new ArgumentNullException(
                    nameof(subgraph));
            }

            return _itemFactory.CreateGroup(
                subgraph);
        }
    }
}