
using System.Collections.Generic;

namespace MermaidDiagram.Test.Markdown
{
    /// <summary>
    /// Đọc nội dung Markdown và trích xuất các Mermaid code block.
    ///
    /// Interface này thuộc MermaidDiagram.Test.
    ///
    /// Không phụ thuộc:
    ///     - MermaidDiagram.Core
    ///     - MermaidDiagram.DevExpress
    ///     - DevExpress
    ///
    /// Mục đích:
    ///     Markdown
    ///         ↓
    ///     Mermaid code blocks
    /// </summary>
    public interface IMermaidMarkdownLoader
    {
        /// <summary>
        /// Đọc một file Markdown và trích xuất tất cả
        /// các Mermaid code block.
        ///
        /// Ví dụ:
        ///
        /// ```mermaid
        /// graph LR
        ///     A --> B
        /// ```
        ///
        /// Một file có nhiều block sẽ trả về nhiều
        /// MermaidCodeBlock.
        /// </summary>
        IReadOnlyList<MermaidCodeBlock> Load(
            string filePath);

        /// <summary>
        /// Trích xuất Mermaid code block trực tiếp
        /// từ nội dung Markdown.
        ///
        /// Dùng khi Test đã có sẵn nội dung file trong memory.
        /// </summary>
        IReadOnlyList<MermaidCodeBlock> LoadFromContent(
            string markdown);
    }
}

