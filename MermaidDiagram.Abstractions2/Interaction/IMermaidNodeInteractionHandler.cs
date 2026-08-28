


namespace MermaidDiagram.Abstractions.Interaction
{
    /// <summary>
    /// Contract xử lý tương tác của người dùng
    /// với một Mermaid node.
    ///
    /// Abstraction này không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - WPF
    ///     - MAUI
    ///     - Blazor
    ///
    /// Implementation ở từng UI platform chịu trách nhiệm
    /// kết nối event của framework tương ứng vào contract này.
    /// </summary>
    public interface IMermaidNodeInteractionHandler
    {
        // ============================================================
        // CLICK
        // ============================================================

        /// <summary>
        /// Xử lý khi người dùng click vào một Mermaid node.
        /// </summary>
        /// <param name="e">
        /// Thông tin node được click.
        /// </param>
        void OnNodeClick(
            MermaidNodeClickEventArgs e);

        // ============================================================
        // HOVER
        // ============================================================

        /// <summary>
        /// Xử lý khi con trỏ đi vào Mermaid node.
        /// </summary>
        /// <param name="e">
        /// Thông tin node được hover.
        /// </param>
        void OnNodeHover(
            MermaidNodeHoverEventArgs e);

        // ============================================================
        // LEAVE
        // ============================================================

        /// <summary>
        /// Xử lý khi con trỏ rời khỏi Mermaid node.
        /// </summary>
        /// <param name="e">
        /// Thông tin node tương tác.
        /// </param>
        void OnNodeLeave(
            MermaidNodeEventArgs e);
    }
}

