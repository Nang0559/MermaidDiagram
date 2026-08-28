using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MermaidDiagram.Core.Models
{
    /// <summary>
    /// Root model đại diện cho toàn bộ Mermaid document.
    ///
    /// Chỉ chứa dữ liệu cấu trúc được parse từ Mermaid.
    /// Không chứa:
    ///     - Layout
    ///     - DevExpress
    ///     - WinForms
    ///     - Workflow business logic
    /// </summary>
    public sealed class MermaidDocument
    {
        // ============================================================
        // DIAGRAM
        // ============================================================

        /// <summary>
        /// Direction nguyên bản của Mermaid.
        ///
        /// Ví dụ:
        ///     TD
        ///     TB
        ///     LR
        ///     RL
        ///     BT
        ///
        /// Đây là raw token từ Mermaid.
        /// Layout engine sẽ chuyển sang MermaidLayoutDirection.
        /// </summary>
        public string DirectionToken
        {
            get;
            set;
        } = string.Empty;

        // ============================================================
        // NODES
        // ============================================================

        /// <summary>
        /// Các node được parse từ Mermaid.
        /// </summary>
        public List<MermaidNode> Nodes
        {
            get;
        } =
            new List<MermaidNode>();

        // ============================================================
        // EDGES
        // ============================================================

        /// <summary>
        /// Các edge được parse từ Mermaid.
        /// </summary>
        public List<MermaidEdge> Edges
        {
            get;
        } =
            new List<MermaidEdge>();

        // ============================================================
        // SUBGRAPHS
        // ============================================================

        /// <summary>
        /// Các subgraph được parse từ Mermaid.
        /// </summary>
        public List<MermaidSubgraph> Subgraphs
        {
            get;
        } =
            new List<MermaidSubgraph>();

        // ============================================================
        // STYLE DEFINITIONS
        // ============================================================

        /// <summary>
        /// Các style directive nguyên bản của Mermaid.
        ///
        /// Ví dụ:
        ///     style A fill:#fff
        ///     style B stroke:#333
        ///
        /// Key:
        ///     NodeId
        ///
        /// Value:
        ///     Raw style definition.
        /// </summary>
        public Dictionary<string, string> Styles
        {
            get;
        } =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

        // ============================================================
        // CLASS DEFINITIONS
        // ============================================================

        /// <summary>
        /// Các classDef được khai báo trong Mermaid.
        ///
        /// Ví dụ:
        ///     classDef process fill:#fff,stroke:#333
        ///
        /// Key:
        ///     ClassName
        ///
        /// Value:
        ///     Raw class definition.
        /// </summary>
        public Dictionary<string, string> ClassDefinitions
        {
            get;
        } =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

        // ============================================================
        // CLASS APPLICATIONS
        // ============================================================

        /// <summary>
        /// Mapping node -> class.
        ///
        /// Ví dụ:
        ///     class A process
        ///
        /// Key:
        ///     NodeId
        ///
        /// Value:
        ///     ClassName
        /// </summary>
        public Dictionary<string, string> ClassApplications
        {
            get;
        } =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
    }
}
