
using System;
using System.Collections.Generic;
using System.Linq;

namespace MermaidDiagram.Test.Cases
{
    /// <summary>
    /// Chọn MermaidTestCase để thực thi.
    ///
    /// Selector chỉ chịu trách nhiệm:
    /// - quản lý danh sách test case;
    /// - tìm test case;
    /// - chọn test case.
    ///
    /// Selector không chịu trách nhiệm:
    /// - đọc Markdown;
    /// - parse Mermaid;
    /// - render diagram;
    /// - chạy test.
    ///
    /// Architecture:
    ///
    /// MermaidTestCaseLoader
    ///          ↓
    /// MermaidTestCaseSelector
    ///          ↓
    /// MermaidTestCase
    ///          ↓
    /// MermaidTestRunner
    /// </summary>
    public sealed class MermaidTestCaseSelector
    {
        // ============================================================
        // STATE
        // ============================================================

        private readonly List<MermaidTestCase> _testCases;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidTestCaseSelector(
            IEnumerable<MermaidTestCase> testCases)
        {
            if (testCases == null)
            {
                throw new ArgumentNullException(
                    nameof(testCases));
            }

            _testCases =
                testCases
                    .Where(x => x != null)
                    .ToList();
        }

        // ============================================================
        // COUNT
        // ============================================================

        public int Count
        {
            get
            {
                return _testCases.Count;
            }
        }

        // ============================================================
        // ALL
        // ============================================================

        /// <summary>
        /// Trả về toàn bộ test case.
        /// </summary>
        public IReadOnlyList<MermaidTestCase> All
        {
            get
            {
                return _testCases.AsReadOnly();
            }
        }

        // ============================================================
        // BY INDEX
        // ============================================================

        /// <summary>
        /// Lấy test case theo vị trí trong danh sách.
        /// </summary>
        public MermaidTestCase GetAt(
            int index)
        {
            if (index < 0 ||
                index >= _testCases.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index));
            }

            return _testCases[index];
        }

        // ============================================================
        // BY ID
        // ============================================================

        /// <summary>
        /// Tìm test case theo Id.
        /// </summary>
        public MermaidTestCase GetById(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "Test case id cannot be null or empty.",
                    nameof(id));
            }

            MermaidTestCase result =
                _testCases.FirstOrDefault(
                    x => string.Equals(
                        x.Id,
                        id,
                        StringComparison.OrdinalIgnoreCase));

            if (result == null)
            {
                throw new KeyNotFoundException(
                    $"Mermaid test case '{id}' was not found.");
            }

            return result;
        }

        // ============================================================
        // TRY BY ID
        // ============================================================

        /// <summary>
        /// Tìm test case theo Id nhưng không throw.
        /// </summary>
        public bool TryGetById(
            string id,
            out MermaidTestCase testCase)
        {
            testCase = null;

            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            MermaidTestCase result =
                _testCases.FirstOrDefault(
                    x => string.Equals(
                        x.Id,
                        id,
                        StringComparison.OrdinalIgnoreCase));

            if (result == null)
            {
                return false;
            }

            testCase =
                result;

            return true;
        }

        // ============================================================
        // SEARCH
        // ============================================================

        /// <summary>
        /// Tìm các test case theo Id hoặc Name.
        /// </summary>
        public IReadOnlyList<MermaidTestCase> Search(
            string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Array.Empty<MermaidTestCase>();
            }

            string value =
                keyword.Trim();

            List<MermaidTestCase> result =
                _testCases
                    .Where(
                        x =>
                            x.Id.IndexOf(
                                value,
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            x.Name.IndexOf(
                                value,
                                StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

            return result.AsReadOnly();
        }

        // ============================================================
        // FIRST
        // ============================================================

        /// <summary>
        /// Lấy test case đầu tiên.
        /// </summary>
        public MermaidTestCase First()
        {
            if (_testCases.Count == 0)
            {
                throw new InvalidOperationException(
                    "No Mermaid test cases are available.");
            }

            return _testCases[0];
        }

        // ============================================================
        // SELECT ALL
        // ============================================================

        /// <summary>
        /// Chọn toàn bộ test case.
        /// </summary>
        public IReadOnlyList<MermaidTestCase> SelectAll()
        {
            return _testCases.AsReadOnly();
        }

        // ============================================================
        // SELECT BY IDS
        // ============================================================

        /// <summary>
        /// Chọn nhiều test case theo danh sách Id.
        /// Thứ tự kết quả theo thứ tự Id được truyền vào.
        /// </summary>
        public IReadOnlyList<MermaidTestCase> SelectByIds(
            IEnumerable<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(
                    nameof(ids));
            }

            List<MermaidTestCase> result =
                new List<MermaidTestCase>();

            foreach (string id in ids)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                result.Add(
                    GetById(id));
            }

            return result.AsReadOnly();
        }

        // ============================================================
        // SELECT BY MARKDOWN FILE
        // ============================================================

        /// <summary>
        /// Chọn toàn bộ test case thuộc một Markdown file.
        /// </summary>
        public IReadOnlyList<MermaidTestCase> SelectByMarkdownFile(
            string markdownFilePath)
        {
            if (string.IsNullOrWhiteSpace(
                    markdownFilePath))
            {
                throw new ArgumentException(
                    "Markdown file path cannot be null or empty.",
                    nameof(markdownFilePath));
            }

            List<MermaidTestCase> result =
                _testCases
                    .Where(
                        x =>
                            string.Equals(
                                x.MarkdownFilePath,
                                markdownFilePath,
                                StringComparison.OrdinalIgnoreCase))
                    .ToList();

            return result.AsReadOnly();
        }
    }
}

