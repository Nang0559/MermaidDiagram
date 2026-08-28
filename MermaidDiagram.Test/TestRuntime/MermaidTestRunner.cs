
using System;
using System.Diagnostics;
using MermaidDiagram.DevExpress.Controls;
using MermaidDiagram.DevExpress.Services;

namespace MermaidDiagram.Test.Cases
{
    /// <summary>
    /// Chạy một MermaidTestCase trên MermaidDiagramControl.
    /// </summary>
    public sealed class MermaidTestRunner
    {
        // ============================================================
        // DEPENDENCY
        // ============================================================

        private readonly MermaidDiagramService _diagramService;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidTestRunner(
            MermaidDiagramService diagramService)
        {
            _diagramService =
                diagramService
                ?? throw new ArgumentNullException(
                    nameof(diagramService));
        }

        // ============================================================
        // RUN
        // ============================================================

        public MermaidTestResult Run(
        MermaidTestCase testCase,
        MermaidDiagramControl control)
        {
            if (testCase == null)
            {
                throw new ArgumentNullException(
                    nameof(testCase));
            }

            if (control == null)
            {
                throw new ArgumentNullException(
                    nameof(control));
            }

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            try
            {
                _diagramService.Load(
                    control,
                    testCase.Source);

                stopwatch.Stop();

                return MermaidTestResult.Passed(
                    testCase,
                    stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                return MermaidTestResult.Failed(
                    testCase,
                    stopwatch.Elapsed,
                    ex);
            }
        }

        // ============================================================
        // CLEAR
        // ============================================================

        public void Clear(
            MermaidDiagramControl control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(
                    nameof(control));
            }

            _diagramService.Clear(
                control);
        }
    }
}

