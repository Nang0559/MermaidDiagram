
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MermaidDiagram.Test.Markdown
{
    /// <summary>
    /// Đọc file Markdown và trích xuất các Mermaid code block.
    ///
    /// Chỉ chịu trách nhiệm:
    ///
    ///     Markdown file
    ///          ↓
    ///     MermaidCodeBlock
    ///
    /// Không parse Mermaid.
    /// Không tạo MermaidDocument.
    /// Không phụ thuộc DevExpress.
    /// </summary>
    public sealed class MermaidMarkdownLoader
        : IMermaidMarkdownLoader
    {
        // ============================================================
        // CONSTANTS
        // ============================================================

        private const string MermaidFence =
            "```mermaid";

        private const string ClosingFence =
            "```";

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Đọc một file Markdown và lấy tất cả Mermaid code block.
        /// </summary>
        public IReadOnlyList<MermaidCodeBlock> Load(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "Markdown file path cannot be null or empty.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Markdown file was not found.",
                    filePath);
            }

            string markdown =
                File.ReadAllText(
                    filePath,
                    Encoding.UTF8);

            return LoadFromContent(markdown);
        }

        /// <summary>
        /// Đọc trực tiếp nội dung Markdown.
        /// </summary>
        public IReadOnlyList<MermaidCodeBlock> LoadFromContent(
            string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return Array.Empty<MermaidCodeBlock>();
            }

            string normalized =
                NormalizeLineEndings(markdown);

            string[] lines =
                normalized.Split(
                    '\n');

            List<MermaidCodeBlock> blocks =
                new List<MermaidCodeBlock>();

            bool insideMermaid =
                false;

            int startLine =
                0;

            StringBuilder sourceBuilder =
                null;

            for (int i = 0;
                 i < lines.Length;
                 i++)
            {
                string line =
                    lines[i];

                int lineNumber =
                    i + 1;

                // ====================================================
                // OUTSIDE MERMAID BLOCK
                // ====================================================

                if (!insideMermaid)
                {
                    if (IsMermaidOpeningFence(line))
                    {
                        insideMermaid =
                            true;

                        startLine =
                            lineNumber;

                        sourceBuilder =
                            new StringBuilder();
                    }

                    continue;
                }

                // ====================================================
                // INSIDE MERMAID BLOCK
                // ====================================================

                if (IsClosingFence(line))
                {
                    int endLine =
                        lineNumber;

                    string source =
                        sourceBuilder
                            .ToString()
                            .Trim();

                    if (!string.IsNullOrWhiteSpace(source))
                    {
                        blocks.Add(
                            new MermaidCodeBlock(
                                blocks.Count,
                                source,
                                startLine,
                                endLine));
                    }

                    insideMermaid =
                        false;

                    startLine =
                        0;

                    sourceBuilder =
                        null;

                    continue;
                }

                // ----------------------------------------------------
                // Mermaid source line
                // ----------------------------------------------------

                if (sourceBuilder.Length > 0)
                {
                    sourceBuilder.AppendLine();
                }

                sourceBuilder.Append(
                    line);
            }

            // ========================================================
            // UNCLOSED BLOCK
            // ========================================================

            if (insideMermaid)
            {
                throw new FormatException(
                    "Markdown contains an unclosed Mermaid " +
                    "code block starting at line " +
                    startLine +
                    ".");
            }

            return blocks;
        }

        // ============================================================
        // FENCE DETECTION
        // ============================================================

        private bool IsMermaidOpeningFence(
            string line)
        {
            if (line == null)
            {
                return false;
            }

            string value =
                line.Trim();

            return string.Equals(
                value,
                MermaidFence,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool IsClosingFence(
            string line)
        {
            if (line == null)
            {
                return false;
            }

            string value =
                line.Trim();

            return string.Equals(
                value,
                ClosingFence,
                StringComparison.Ordinal);
        }

        // ============================================================
        // NORMALIZE
        // ============================================================

        private string NormalizeLineEndings(
            string value)
        {
            return value
                .Replace(
                    "\r\n",
                    "\n")
                .Replace(
                    "\r",
                    "\n");
        }
    }
}

