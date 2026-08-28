using System;
using System.Collections.Generic;
using System.Text;

namespace MermaidDiagram.Test.Cases
{
    /// <summary>
    /// Ghi nhận log của quá trình chạy Mermaid test.
    ///
    /// MermaidTestLog không thực hiện:
    /// - Parse
    /// - Graph analysis
    /// - Layout
    /// - Render
    /// - Execute test
    ///
    /// Nó chỉ lưu lại thông tin execution.
    /// </summary>
    public sealed class MermaidTestLog
    {
        // ============================================================
        // STATE
        // ============================================================

        private readonly List<Entry> _entries;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidTestLog()
        {
            _entries =
                new List<Entry>();
        }

        // ============================================================
        // ENTRIES
        // ============================================================

        public IReadOnlyList<Entry> Entries
        {
            get
            {
                return _entries.AsReadOnly();
            }
        }

        // ============================================================
        // COUNT
        // ============================================================

        public int Count
        {
            get
            {
                return _entries.Count;
            }
        }

        // ============================================================
        // STARTED
        // ============================================================

        public void Started(
            MermaidTestCase testCase)
        {
            if (testCase == null)
            {
                throw new ArgumentNullException(
                    nameof(testCase));
            }

            Add(
                TestLogLevel.Info,
                testCase,
                "Test started.");
        }

        // ============================================================
        // RESULT
        // ============================================================

        /// <summary>
        /// Ghi log dựa trực tiếp trên MermaidTestResult.
        ///
        /// Đây là API chính mà MermaidTestRunner nên sử dụng.
        /// </summary>
        public void Result(
            MermaidTestResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(
                    nameof(result));
            }

            if (result.Success)
            {
                Passed(result);
            }
            else
            {
                Failed(result);
            }
        }

        // ============================================================
        // PASSED
        // ============================================================

        public void Passed(
            MermaidTestResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(
                    nameof(result));
            }

            Add(
                TestLogLevel.Success,
                result.TestCase,
                BuildPassedMessage(result));
        }

        // ============================================================
        // FAILED
        // ============================================================

        public void Failed(
            MermaidTestResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(
                    nameof(result));
            }

            Add(
                TestLogLevel.Error,
                result.TestCase,
                BuildFailedMessage(result));
        }

        // ============================================================
        // MESSAGE
        // ============================================================

        public void Message(
            MermaidTestCase testCase,
            string message)
        {
            if (testCase == null)
            {
                throw new ArgumentNullException(
                    nameof(testCase));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            Add(
                TestLogLevel.Info,
                testCase,
                message);
        }

        // ============================================================
        // CLEAR
        // ============================================================

        public void Clear()
        {
            _entries.Clear();
        }

        // ============================================================
        // ADD
        // ============================================================

        private void Add(
            TestLogLevel level,
            MermaidTestCase testCase,
            string message)
        {
            _entries.Add(
                new Entry(
                    DateTime.Now,
                    level,
                    testCase.Id,
                    testCase.Name,
                    testCase.MarkdownFilePath,
                    testCase.DiagramIndex,
                    message));
        }

        // ============================================================
        // BUILD PASSED MESSAGE
        // ============================================================

        private string BuildPassedMessage(
            MermaidTestResult result)
        {
            return
                "Test passed. " +
                "Duration=" +
                result.Duration
                    .TotalMilliseconds
                    .ToString("0") +
                " ms.";
        }

        // ============================================================
        // BUILD FAILED MESSAGE
        // ============================================================

        private string BuildFailedMessage(
            MermaidTestResult result)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.Append(
                "Test failed. ");

            builder.Append(
                "Duration=");

            builder.Append(
                result.Duration
                    .TotalMilliseconds
                    .ToString("0"));

            builder.Append(
                " ms.");

            if (result.Error != null)
            {
                builder.Append(
                    " Error=");

                builder.Append(
                    result.ErrorMessage);
            }

            return builder.ToString();
        }

        // ============================================================
        // ENTRY
        // ============================================================

        public sealed class Entry
        {
            public DateTime Timestamp
            {
                get;
            }

            public TestLogLevel Level
            {
                get;
            }

            public string TestCaseId
            {
                get;
            }

            public string TestCaseName
            {
                get;
            }

            public string MarkdownFilePath
            {
                get;
            }

            public int DiagramIndex
            {
                get;
            }

            public string Message
            {
                get;
            }

            public Entry(
                DateTime timestamp,
                TestLogLevel level,
                string testCaseId,
                string testCaseName,
                string markdownFilePath,
                int diagramIndex,
                string message)
            {
                Timestamp =
                    timestamp;

                Level =
                    level;

                TestCaseId =
                    testCaseId;

                TestCaseName =
                    testCaseName;

                MarkdownFilePath =
                    markdownFilePath;

                DiagramIndex =
                    diagramIndex;

                Message =
                    message;
            }

            public override string ToString()
            {
                return
                    $"[{Timestamp:HH:mm:ss}] " +
                    $"[{Level}] " +
                    $"{TestCaseId} - " +
                    $"{Message}";
            }
        }

        // ============================================================
        // LOG LEVEL
        // ============================================================

        public enum TestLogLevel
        {
            Info = 0,
            Success = 1,
            Warning = 2,
            Error = 3
        }
    }
}