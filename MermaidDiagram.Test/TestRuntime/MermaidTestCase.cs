
using MermaidDiagram.Test.Markdown;
using System;

namespace MermaidDiagram.Test.Cases
{
    /// <summary>
    /// Đại diện cho một Mermaid test case.
    ///
    /// Một test case tương ứng với:
    ///
    ///     Markdown file
    ///          +
    ///     một MermaidCodeBlock
    ///
    /// Ví dụ:
    ///
    ///     WORKFLOW_MAIN.md
    ///          └── Diagram #1
    ///
    /// MermaidTestCase không phụ thuộc:
    ///     - DevExpress
    ///     - DiagramControl
    ///     - Mermaid parser implementation
    /// </summary>
    public sealed class MermaidTestCase
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// ID duy nhất của test case.
        ///
        /// Ví dụ:
        ///
        ///     WORKFLOW_MAIN_001
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Tên hiển thị của test case.
        ///
        /// Ví dụ:
        ///
        ///     WORKFLOW_MAIN - Diagram #1
        /// </summary>
        public string Name { get; }

        // ============================================================
        // SOURCE
        // ============================================================

        /// <summary>
        /// Đường dẫn tới file Markdown gốc.
        /// </summary>
        public string MarkdownFilePath { get; }

        /// <summary>
        /// Mermaid block được lấy từ Markdown.
        /// </summary>
        public MermaidCodeBlock CodeBlock { get; }

        // ============================================================
        // CONVENIENCE
        // ============================================================

        /// <summary>
        /// Mermaid source dùng để đưa vào MermaidDiagramPipeline.
        /// </summary>
        public string Source
        {
            get
            {
                return CodeBlock.Source;
            }
        }

        /// <summary>
        /// Số thứ tự diagram trong Markdown.
        ///
        /// Giá trị 0-based.
        /// </summary>
        public int DiagramIndex
        {
            get
            {
                return CodeBlock.Index;
            }
        }

        /// <summary>
        /// Dòng bắt đầu trong Markdown.
        /// </summary>
        public int StartLine
        {
            get
            {
                return CodeBlock.StartLine;
            }
        }

        /// <summary>
        /// Dòng kết thúc trong Markdown.
        /// </summary>
        public int EndLine
        {
            get
            {
                return CodeBlock.EndLine;
            }
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidTestCase(
            string id,
            string name,
            string markdownFilePath,
            MermaidCodeBlock codeBlock)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "Test case id cannot be null or empty.",
                    nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Test case name cannot be null or empty.",
                    nameof(name));
            }

            if (string.IsNullOrWhiteSpace(markdownFilePath))
            {
                throw new ArgumentException(
                    "Markdown file path cannot be null or empty.",
                    nameof(markdownFilePath));
            }

            if (codeBlock == null)
            {
                throw new ArgumentNullException(
                    nameof(codeBlock));
            }

            Id =
                id;

            Name =
                name;

            MarkdownFilePath =
                markdownFilePath;

            CodeBlock =
                codeBlock;
        }

        // ============================================================
        // DISPLAY
        // ============================================================

        public override string ToString()
        {
            return Name;
        }
    }
}

