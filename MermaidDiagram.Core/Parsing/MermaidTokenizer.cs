using System;
using System.Collections.Generic;


namespace MermaidDiagram.Core.Parsing
{
    /// <summary>
    /// Chuẩn hóa Mermaid source thành danh sách
    /// các dòng logic dùng cho parser.
    ///
    /// Đây là tokenizer cấp dòng.
    /// Chưa thực hiện lexical tokenization.
    /// </summary>
    public sealed class MermaidTokenizer
    {
        public List<string> Tokenize(
            string source)
        {
            List<string> result =
                new List<string>();

            if (string.IsNullOrWhiteSpace(source))
            {
                return result;
            }

            string normalized =
                source
                    .Replace(
                        "\r\n",
                        "\n")
                    .Replace(
                        "\r",
                        "\n");

            string[] lines =
                normalized.Split(
                    new[] { '\n' },
                    StringSplitOptions.None);

            foreach (string rawLine in lines)
            {
                if (rawLine == null)
                {
                    continue;
                }

                string line =
                    rawLine.Trim();

                if (line.Length == 0)
                {
                    continue;
                }

                // ----------------------------------------------------
                // MERMAID COMMENT
                // ----------------------------------------------------

                if (line.StartsWith(
                        "%%",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                result.Add(line);
            }

            return result;
        }
    }
}