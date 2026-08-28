
using System;
using System.Collections.Generic;
using System.IO;
using MermaidDiagram.Test.Markdown;

namespace MermaidDiagram.Test.Cases
{
    /// <summary>
    /// Tìm các Markdown file và tạo MermaidTestCase
    /// từ các Mermaid code block bên trong.
    ///
    /// Luồng:
    ///
    /// TestCases/
    ///     *.md
    ///        ↓
    /// MermaidMarkdownLoader
    ///        ↓
    /// MermaidCodeBlock
    ///        ↓
    /// MermaidTestCase
    ///
    /// Class này không parse Mermaid.
    /// </summary>
    public sealed class MermaidTestCaseLoader
    {
        // ============================================================
        // DEPENDENCY
        // ============================================================

        private readonly IMermaidMarkdownLoader _markdownLoader;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidTestCaseLoader(
            IMermaidMarkdownLoader markdownLoader)
        {
            _markdownLoader =
                markdownLoader
                ?? throw new ArgumentNullException(
                    nameof(markdownLoader));
        }

        // ============================================================
        // LOAD DIRECTORY
        // ============================================================

        /// <summary>
        /// Load tất cả Markdown file trực tiếp trong thư mục.
        ///
        /// Không tìm thư mục con.
        /// </summary>
        public IReadOnlyList<MermaidTestCase> LoadDirectory(
            string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException(
                    "Test case directory cannot be null or empty.",
                    nameof(directoryPath));
            }

            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(
                    "Test case directory was not found: " +
                    directoryPath);
            }

            string[] files =
                Directory.GetFiles(
                    directoryPath,
                    "*.md",
                    SearchOption.TopDirectoryOnly);

            Array.Sort(
                files,
                StringComparer.OrdinalIgnoreCase);

            List<MermaidTestCase> result =
                new List<MermaidTestCase>();

            foreach (string filePath in files)
            {
                IReadOnlyList<MermaidTestCase> cases =
                    LoadFile(filePath);

                result.AddRange(
                    cases);
            }

            return result;
        }

        // ============================================================
        // LOAD DIRECTORY RECURSIVE
        // ============================================================

        /// <summary>
        /// Load Markdown file trong thư mục và toàn bộ
        /// thư mục con.
        ///
        /// Dùng khi TestCases được tổ chức theo nhóm.
        /// </summary>
        public IReadOnlyList<MermaidTestCase> LoadDirectoryRecursive(
            string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException(
                    "Test case directory cannot be null or empty.",
                    nameof(directoryPath));
            }

            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(
                    "Test case directory was not found: " +
                    directoryPath);
            }

            string[] files =
                Directory.GetFiles(
                    directoryPath,
                    "*.md",
                    SearchOption.AllDirectories);

            Array.Sort(
                files,
                StringComparer.OrdinalIgnoreCase);

            List<MermaidTestCase> result =
                new List<MermaidTestCase>();

            foreach (string filePath in files)
            {
                IReadOnlyList<MermaidTestCase> cases =
                    LoadFile(filePath);

                result.AddRange(
                    cases);
            }

            return result;
        }

        // ============================================================
        // LOAD FILE
        // ============================================================

        /// <summary>
        /// Load một Markdown file.
        ///
        /// Mỗi Mermaid block tạo thành một test case.
        /// </summary>
        public IReadOnlyList<MermaidTestCase> LoadFile(
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
                    "Markdown test file was not found.",
                    filePath);
            }

            IReadOnlyList<MermaidCodeBlock> blocks =
                _markdownLoader.Load(
                    filePath);

            List<MermaidTestCase> result =
                new List<MermaidTestCase>();

            string fileName =
                Path.GetFileNameWithoutExtension(
                    filePath);

            for (int i = 0;
                 i < blocks.Count;
                 i++)
            {
                MermaidCodeBlock block =
                    blocks[i];

                MermaidTestCase testCase =
                    CreateTestCase(
                        filePath,
                        fileName,
                        block);

                result.Add(
                    testCase);
            }

            return result;
        }

        // ============================================================
        // CREATE TEST CASE
        // ============================================================

        private MermaidTestCase CreateTestCase(
            string filePath,
            string fileName,
            MermaidCodeBlock block)
        {
            string id =
                CreateTestCaseId(
                    fileName,
                    block.Index);

            string name =
                CreateTestCaseName(
                    fileName,
                    block.Index);

            return new MermaidTestCase(
                id,
                name,
                filePath,
                block);
        }

        // ============================================================
        // ID
        // ============================================================

        private string CreateTestCaseId(
            string fileName,
            int diagramIndex)
        {
            return
                fileName +
                "_" +
                (diagramIndex + 1).ToString("D3");
        }

        // ============================================================
        // NAME
        // ============================================================

        private string CreateTestCaseName(
            string fileName,
            int diagramIndex)
        {
            return
                fileName +
                " - Diagram #" +
                (diagramIndex + 1);
        }
    }
}

