namespace MermaidDiagram.Abstractions.Interaction
{
    /// <summary>
    /// Event arguments cho interaction click trên một Mermaid node.
    ///
    /// Đây là contract UI-independent.
    ///
    /// Không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Blazor
    ///     - MermaidDiagram.Core
    ///
    /// Adapter của từng UI framework chịu trách nhiệm
    /// chuyển interaction thực tế thành object này.
    /// </summary>
    public sealed class MermaidNodeClickEventArgs
        : MermaidNodeEventArgs
    {
        // ============================================================
        // MOUSE / POINTER
        // ============================================================

        /// <summary>
        /// Cho biết click có phải là click chuột trái hay không.
        ///
        /// Với các nền tảng không có khái niệm chuột,
        /// adapter có thể sử dụng giá trị mặc định phù hợp.
        /// </summary>
        public bool IsPrimaryClick
        {
            get;
        }

        /// <summary>
        /// Cho biết interaction có phải double-click hay không.
        /// </summary>
        public bool IsDoubleClick
        {
            get;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidNodeClickEventArgs(
            string nodeKey,
            string nodeText,
            bool isPrimaryClick = true,
            bool isDoubleClick = false)
            : base(
                nodeKey,
                nodeText)
        {
            IsPrimaryClick =
                isPrimaryClick;

            IsDoubleClick =
                isDoubleClick;
        }
    }
}