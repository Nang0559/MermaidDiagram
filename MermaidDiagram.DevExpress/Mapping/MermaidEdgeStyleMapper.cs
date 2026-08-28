using DevExpress.Diagram.Core;
using DevExpress.XtraDiagram;
using MermaidDiagram.Core.Models;

namespace MermaidDiagram.DevExpress.Mapping
{
    /// <summary>
    /// Map MermaidEdgeStyle sang cấu hình DiagramConnector
    /// của DevExpress Diagram.
    ///
    /// Input:
    ///     MermaidEdgeStyle
    ///
    /// Output:
    ///     DiagramConnector
    ///
    /// Chỉ chịu trách nhiệm:
    ///     - Connector type
    ///     - Các thuộc tính visual mà DevExpress
    ///       thực sự hỗ trợ trực tiếp
    ///
    /// Không chịu trách nhiệm:
    ///     - Parse Mermaid
    ///     - Graph
    ///     - Layout
    ///     - Workflow
    ///     - Tạo connector
    ///     - Xác định Source / Target
    ///     - Arrow đầu / cuối
    /// </summary>
    public static class MermaidEdgeStyleMapper
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Áp dụng MermaidEdgeStyle lên connector.
        ///
        /// Lưu ý:
        /// MermaidEdgeStyle.Dashed và Thick vẫn được bảo toàn
        /// ở Core nhưng chỉ được render thực tế nếu DevExpress
        /// expose API tương ứng.
        /// </summary>
        public static void Apply(
            DiagramConnector connector,
            MermaidEdgeStyle style)
        {
            if (connector == null)
            {
                throw new ArgumentNullException(
                    nameof(connector));
            }

            switch (style)
            {
                case MermaidEdgeStyle.Solid:

                    ApplySolid(
                        connector);

                    break;

                case MermaidEdgeStyle.Dashed:

                    ApplyDashed(
                        connector);

                    break;

                case MermaidEdgeStyle.Thick:

                    ApplyThick(
                        connector);

                    break;

                default:

                    ApplySolid(
                        connector);

                    break;
            }
        }

        // ============================================================
        // SOLID
        // ============================================================

        private static void ApplySolid(
            DiagramConnector connector)
        {
            connector.Type =
                ConnectorType.Straight;
        }

        // ============================================================
        // DASHED
        // ============================================================

        private static void ApplyDashed(
            DiagramConnector connector)
        {
            /*
             * MermaidEdgeStyle.Dashed được giữ ở Core.
             *
             * Tuy nhiên DiagramConnector abstraction hiện tại
             * chưa expose API DashStyle / LineStyle mà chúng ta
             * có thể sử dụng trực tiếp.
             *
             * Vì vậy KHÔNG giả lập.
             */

            connector.Type =
                ConnectorType.Straight;
        }

        // ============================================================
        // THICK
        // ============================================================

        private static void ApplyThick(
            DiagramConnector connector)
        {
            /*
             * MermaidEdgeStyle.Thick được giữ ở Core.
             *
             * Nếu DiagramConnector hiện tại không expose
             * thuộc tính độ dày đường nối thì không tự tạo
             * property hoặc API giả.
             *
             * Việc render thick thực tế có thể bổ sung sau
             * ở Rendering layer nếu cần.
             */

            connector.Type =
                ConnectorType.Straight;
        }
    }
}