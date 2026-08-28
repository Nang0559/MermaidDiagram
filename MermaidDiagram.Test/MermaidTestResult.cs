
using System;

namespace MermaidDiagram.Test.Cases
{
    /// <summary>
    /// Kết quả thực thi một MermaidTestCase.
    /// </summary>
    public sealed class MermaidTestResult
    {
        // ============================================================
        // TEST CASE
        // ============================================================

        public MermaidTestCase TestCase
        {
            get;
        }

        // ============================================================
        // STATUS
        // ============================================================

        public bool Success
        {
            get;
        }

        // ============================================================
        // TIMING
        // ============================================================

        public TimeSpan Duration
        {
            get;
        }

        // ============================================================
        // ERROR
        // ============================================================

        public Exception Error
        {
            get;
        }

        public string ErrorMessage
        {
            get
            {
                return Error == null
                    ? string.Empty
                    : Error.Message;
            }
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        private MermaidTestResult(
            MermaidTestCase testCase,
            bool success,
            TimeSpan duration,
            Exception error)
        {
            TestCase =
                testCase
                ?? throw new ArgumentNullException(
                    nameof(testCase));

            Success =
                success;

            Duration =
                duration;

            Error =
                error;
        }

        // ============================================================
        // FACTORY - PASSED
        // ============================================================

        public static MermaidTestResult Passed(
            MermaidTestCase testCase,
            TimeSpan duration)
        {
            return new MermaidTestResult(
                testCase,
                true,
                duration,
                null);
        }

        // ============================================================
        // FACTORY - FAILED
        // ============================================================

        public static MermaidTestResult Failed(
            MermaidTestCase testCase,
            TimeSpan duration,
            Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(
                    nameof(error));
            }

            return new MermaidTestResult(
                testCase,
                false,
                duration,
                error);
        }

        // ============================================================
        // DISPLAY
        // ============================================================

        public override string ToString()
        {
            if (Success)
            {
                return
                    $"{TestCase.Name}: PASS " +
                    $"({Duration.TotalMilliseconds:0} ms)";
            }

            return
                $"{TestCase.Name}: FAIL - " +
                $"{ErrorMessage}";
        }
    }
}

