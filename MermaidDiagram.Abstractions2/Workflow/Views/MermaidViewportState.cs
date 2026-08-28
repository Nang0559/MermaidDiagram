
namespace MermaidDiagram.Abstractions.Workflow.Views
{
    /// <summary>
    /// Trạng thái hiện tại của Mermaid Diagram Viewport.
    ///
    /// Đây là state thuần dữ liệu, không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - WPF
    ///     - MAUI
    ///     - Blazor
    ///
    /// State được sử dụng để lưu và khôi phục vị trí
    /// của Workflow Diagram khi người dùng chuyển qua
    /// lại giữa Diagram và Workflow View.
    /// </summary>
    public sealed class MermaidViewportState
    {
        // ============================================================
        // FOCUS
        // ============================================================

        /// <summary>
        /// Node hiện đang được focus.
        ///
        /// null nếu viewport không focus node nào.
        /// </summary>
        public string? FocusedNodeKey
        {
            get;
            set;
        }

        // ============================================================
        // ZOOM
        // ============================================================

        /// <summary>
        /// Mức zoom hiện tại.
        ///
        /// Ví dụ:
        ///     1.0 = 100%
        ///     1.5 = 150%
        ///     2.0 = 200%
        /// </summary>
        public double Zoom
        {
            get;
            set;
        }

        // ============================================================
        // POSITION
        // ============================================================

        /// <summary>
        /// Vị trí ngang hiện tại của viewport.
        ///
        /// Không sử dụng kiểu Point của UI framework.
        /// </summary>
        public double OffsetX
        {
            get;
            set;
        }

        /// <summary>
        /// Vị trí dọc hiện tại của viewport.
        /// </summary>
        public double OffsetY
        {
            get;
            set;
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidViewportState()
        {
            Zoom = 1.0;
            OffsetX = 0.0;
            OffsetY = 0.0;
        }

        // ============================================================
        // COPY
        // ============================================================

        /// <summary>
        /// Tạo một bản sao độc lập của state.
        /// </summary>
        public MermaidViewportState Clone()
        {
            return new MermaidViewportState
            {
                FocusedNodeKey =
                    FocusedNodeKey,

                Zoom =
                    Zoom,

                OffsetX =
                    OffsetX,

                OffsetY =
                    OffsetY
            };
        }
    }
}

