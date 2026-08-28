
using System;

namespace MermaidDiagram.Test.Markdown
{
    /// <summary>
    /// Đại diện cho một Mermaid code block
    /// được trích xuất từ file Markdown.
    ///
    /// Ví dụ:
    ///
    /// ```mermaid
    /// graph LR
    ///     A --> B
    /// ```
    ///
    /// Một MermaidCodeBlock tương ứng với phần:
    ///
    /// graph LR
    /// A --> B
    /// </summary>
    public sealed class MermaidCodeBlock
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        /// <summary>
        /// Thứ tự của Mermaid block trong file Markdown.
        ///
        /// Block đầu tiên:
        ///     0
        ///
        /// Block thứ hai:
        ///     1
        /// </summary>
        public int Index { get; }

        // ============================================================
        // SOURCE
        // ============================================================

        /// <summary>
        /// Nội dung Mermaid thuần.
        ///
        /// Không bao gồm:
        ///     ```mermaid
        ///     ```
        /// </summary>
        public string Source { get; }

        // ============================================================
        // MARKDOWN LOCATION
        // ============================================================

        /// <summary>
        /// Dòng bắt đầu của code block trong file Markdown.
        ///
        /// Tính theo line của file Markdown.
        /// </summary>
        public int StartLine { get; }

        /// <summary>
        /// Dòng kết thúc của code block trong file Markdown.
        /// </summary>
        public int EndLine { get; }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidCodeBlock(
            int index,
            string source,
            int startLine,
            int endLine)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    "Mermaid block index cannot be negative.");
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(
                    "Mermaid source cannot be null or empty.",
                    nameof(source));
            }

            if (startLine <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startLine),
                    "Start line must be greater than zero.");
            }

            if (endLine < startLine)
            {
                throw new ArgumentException(
                    "End line cannot be smaller than start line.",
                    nameof(endLine));
            }

            Index =
                index;

            Source =
                source.Trim();

            StartLine =
                startLine;

            EndLine =
                endLine;
        }

        // ============================================================
        // DISPLAY
        // ============================================================

        public override string ToString()
        {
            return
                $"Diagram #{Index + 1} " +
                $"(Lines {StartLine}-{EndLine})";
        }
    }
}

