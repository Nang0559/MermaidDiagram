using DevExpress.XtraEditors;
using MermaidDiagram.DevExpress.Controls;
using MermaidDiagram.DevExpress.Pipeline;
using MermaidDiagram.DevExpress.Services;
using MermaidDiagram.Test.Cases;
using MermaidDiagram.Test.Markdown;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MermaidDiagram.Test
{
    /// <summary>
    /// Form test chính cho MermaidDiagram.
    ///
    /// Luồng:
    ///
    /// Markdown .md
    ///      ↓
    /// MermaidMarkdownLoader
    ///      ↓
    /// MermaidTestCaseLoader
    ///      ↓
    /// List<MermaidTestCase>
    ///      ↓
    /// MermaidTestCase
    ///      ↓
    /// MermaidTestRunner
    ///      ↓
    /// MermaidDiagramService
    ///      ↓
    /// IMermaidDiagramPipeline
    ///      ↓
    /// MermaidDiagramControl
    ///
    /// Form chỉ chịu trách nhiệm UI/test.
    /// Không chứa parser/layout/rendering logic.
    /// </summary>
    public sealed class FormMain : XtraForm
    {
        // ============================================================
        // TEST INFRASTRUCTURE
        // ============================================================

        private readonly MermaidTestCaseLoader _caseLoader;
        private readonly MermaidTestCaseSelector _caseSelector;
        private readonly MermaidTestRunner _testRunner;

        // ============================================================
        // UI
        // ============================================================

        private PanelControl _panelTop;
        private PanelControl _panelLeft;
        private PanelControl _panelCenter;
        private PanelControl _panelBottom;

        private LabelControl _lblMarkdown;
        private LabelControl _lblDiagram;

        private ButtonEdit _txtMarkdown;

        private SimpleButton _btnOpenMarkdown;
        private SimpleButton _btnLoad;
        private SimpleButton _btnRender;
        private SimpleButton _btnClear;

        private ListBoxControl _lstCases;

        private MemoEdit _txtLog;

        private MermaidDiagramControl _diagramControl;

        // ============================================================
        // STATE
        // ============================================================

        private readonly List<MermaidTestCase> _testCases;

        private MermaidTestCase _selectedTestCase;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public FormMain()
        {
            // ========================================================
            // STATE
            // ========================================================

            _testCases =
                new List<MermaidTestCase>();

            _selectedTestCase =
                null;

            // ========================================================
            // PIPELINE
            // ========================================================

            IMermaidDiagramPipeline pipeline =
                MermaidDiagramPipelineFactory.Create();

            if (pipeline == null)
            {
                throw new InvalidOperationException(
                    "MermaidDiagramPipelineFactory.Create() " +
                    "returned null.");
            }

            // ========================================================
            // SERVICE
            // ========================================================

            MermaidDiagramService diagramService =
                new MermaidDiagramService(
                    pipeline);

            // ========================================================
            // MARKDOWN LOADER
            // ========================================================

            IMermaidMarkdownLoader markdownLoader =
                new MermaidMarkdownLoader();

            // ========================================================
            // TEST CASE LOADER
            // ========================================================

            _caseLoader =
                new MermaidTestCaseLoader(
                    markdownLoader);

            // ========================================================
            // TEST CASE SELECTOR
            // ========================================================

            //_caseSelector =
            //    new MermaidTestCaseSelector();

            // ========================================================
            // TEST RUNNER
            // ========================================================

            _testRunner =
                new MermaidTestRunner(
                    diagramService);

            // ========================================================
            // FORM
            // ========================================================

            InitializeComponent();

            // ========================================================
            // DIAGRAM CONTROL
            // ========================================================

            _diagramControl =
                new MermaidDiagramControl(
                    pipeline);

            _diagramControl.Dock =
                DockStyle.Fill;

            _panelCenter.Controls.Add(
                _diagramControl);

            // ========================================================
            // EVENTS
            // ========================================================

            WireEvents();
        }

        // ============================================================
        // INITIALIZE COMPONENT
        // ============================================================

        private void InitializeComponent()
        {
            SuspendLayout();

            // ========================================================
            // FORM
            // ========================================================

            Name =
                "FormMain";

            Text =
                "Mermaid Diagram Test";

            StartPosition =
                FormStartPosition.CenterScreen;

            WindowState =
                FormWindowState.Maximized;

            MinimumSize =
                new Size(
                    1200,
                    700);

            // ========================================================
            // TOP PANEL
            // ========================================================

            _panelTop =
                new PanelControl();

            _panelTop.Dock =
                DockStyle.Top;

            _panelTop.Height =
                58;

            // ========================================================
            // MARKDOWN LABEL
            // ========================================================

            _lblMarkdown =
                new LabelControl();

            _lblMarkdown.Text =
                "Markdown:";

            _lblMarkdown.Location =
                new Point(
                    12,
                    20);

            // ========================================================
            // MARKDOWN PATH
            // ========================================================

            _txtMarkdown =
                new ButtonEdit();

            _txtMarkdown.Location =
                new Point(
                    85,
                    14);

            _txtMarkdown.Size =
                new Size(
                    520,
                    30);

            _txtMarkdown.Properties.Buttons.Clear();

            // ========================================================
            // OPEN
            // ========================================================

            _btnOpenMarkdown =
                new SimpleButton();

            _btnOpenMarkdown.Text =
                "Open...";

            _btnOpenMarkdown.Location =
                new Point(
                    615,
                    14);

            _btnOpenMarkdown.Size =
                new Size(
                    90,
                    30);

            // ========================================================
            // LOAD
            // ========================================================

            _btnLoad =
                new SimpleButton();

            _btnLoad.Text =
                "Load";

            _btnLoad.Location =
                new Point(
                    715,
                    14);

            _btnLoad.Size =
                new Size(
                    80,
                    30);

            // ========================================================
            // RENDER
            // ========================================================

            _btnRender =
                new SimpleButton();

            _btnRender.Text =
                "Render";

            _btnRender.Location =
                new Point(
                    805,
                    14);

            _btnRender.Size =
                new Size(
                    90,
                    30);

            // ========================================================
            // CLEAR
            // ========================================================

            _btnClear =
                new SimpleButton();

            _btnClear.Text =
                "Clear";

            _btnClear.Location =
                new Point(
                    905,
                    14);

            _btnClear.Size =
                new Size(
                    80,
                    30);

            // ========================================================
            // ADD TOP CONTROLS
            // ========================================================

            _panelTop.Controls.Add(
                _lblMarkdown);

            _panelTop.Controls.Add(
                _txtMarkdown);

            _panelTop.Controls.Add(
                _btnOpenMarkdown);

            _panelTop.Controls.Add(
                _btnLoad);

            _panelTop.Controls.Add(
                _btnRender);

            _panelTop.Controls.Add(
                _btnClear);

            // ========================================================
            // LEFT PANEL
            // ========================================================

            _panelLeft =
                new PanelControl();

            _panelLeft.Dock =
                DockStyle.Left;

            _panelLeft.Width =
                320;

            // ========================================================
            // DIAGRAM LABEL
            // ========================================================

            _lblDiagram =
                new LabelControl();

            _lblDiagram.Text =
                "Mermaid diagrams:";

            _lblDiagram.Dock =
                DockStyle.Top;

            _lblDiagram.Height =
                35;

            _lblDiagram.Padding =
                new Padding(
                    10,
                    10,
                    0,
                    0);

            // ========================================================
            // TEST CASE LIST
            // ========================================================

            _lstCases =
                new ListBoxControl();

            _lstCases.Dock =
                DockStyle.Fill;

            // ========================================================
            // LEFT CONTROLS
            // ========================================================

            _panelLeft.Controls.Add(
                _lstCases);

            _panelLeft.Controls.Add(
                _lblDiagram);

            // ========================================================
            // CENTER PANEL
            // ========================================================

            _panelCenter =
                new PanelControl();

            _panelCenter.Dock =
                DockStyle.Fill;

            // ========================================================
            // BOTTOM PANEL
            // ========================================================

            _panelBottom =
                new PanelControl();

            _panelBottom.Dock =
                DockStyle.Bottom;

            _panelBottom.Height =
                150;

            // ========================================================
            // LOG
            // ========================================================

            _txtLog =
                new MemoEdit();

            _txtLog.Dock =
                DockStyle.Fill;

            _txtLog.Properties.ReadOnly =
                true;

            _txtLog.Properties.ScrollBars =
                ScrollBars.Both;

            _panelBottom.Controls.Add(
                _txtLog);

            // ========================================================
            // FORM CONTROLS
            // ========================================================

            Controls.Add(
                _panelCenter);

            Controls.Add(
                _panelLeft);

            Controls.Add(
                _panelBottom);

            Controls.Add(
                _panelTop);

            ResumeLayout(
                false);
        }

        // ============================================================
        // EVENTS
        // ============================================================

        private void WireEvents()
        {
            _btnOpenMarkdown.Click +=
                BtnOpenMarkdown_Click;

            _btnLoad.Click +=
                BtnLoad_Click;

            _btnRender.Click +=
                BtnRender_Click;

            _btnClear.Click +=
                BtnClear_Click;

            _lstCases.SelectedIndexChanged +=
                LstCases_SelectedIndexChanged;
        }

        // ============================================================
        // OPEN MARKDOWN
        // ============================================================

        private void BtnOpenMarkdown_Click(
            object sender,
            EventArgs e)
        {
            using (var dialog =
                   new OpenFileDialog())
            {
                dialog.Filter =
                    "Markdown files (*.md)|*.md|" +
                    "All files (*.*)|*.*";

                dialog.Title =
                    "Chọn Mermaid Markdown Test File";

                if (dialog.ShowDialog() !=
                    DialogResult.OK)
                {
                    return;
                }

                // ----------------------------------------------------
                // Hiển thị path lên UI
                // ----------------------------------------------------

                _txtMarkdown.Text =
                    dialog.FileName;

                // ----------------------------------------------------
                // Load ngay file vừa chọn
                // ----------------------------------------------------

                LoadMarkdownFile(
                    dialog.FileName);
            }
        }

        // ============================================================
        // LOAD BUTTON
        // ============================================================

        private void BtnLoad_Click(
            object sender,
            EventArgs e)
        {
            string path =
                _txtMarkdown.Text == null
                    ? string.Empty
                    : _txtMarkdown.Text.Trim();

            if (string.IsNullOrWhiteSpace(path))
            {
                XtraMessageBox.Show(
                    this,
                    "Chưa chọn file Markdown.",
                    "Mermaid Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            LoadMarkdownFile(
                path);
        }

        // ============================================================
        // LOAD MARKDOWN FILE
        // ============================================================

        private void LoadMarkdownFile(
            string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                        filePath))
                {
                    return;
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException(
                        "Không tìm thấy Markdown file.",
                        filePath);
                }

                // ----------------------------------------------------
                // LOAD TEST CASES
                // ----------------------------------------------------

                IReadOnlyList<MermaidTestCase> cases =
                    _caseLoader.LoadFile(
                        filePath);

                // ----------------------------------------------------
                // REPLACE CURRENT CASES
                // ----------------------------------------------------

                _testCases.Clear();

                _testCases.AddRange(
                    cases);

                _selectedTestCase =
                    null;

                // ----------------------------------------------------
                // UPDATE PATH
                // ----------------------------------------------------

                _txtMarkdown.Text =
                    filePath;

                // ----------------------------------------------------
                // UPDATE LIST
                // ----------------------------------------------------

                UpdateTestCaseList();

                // ----------------------------------------------------
                // CLEAR CURRENT DIAGRAM
                // ----------------------------------------------------

                _testRunner.Clear(
                    _diagramControl);

                // ----------------------------------------------------
                // LOG
                // ----------------------------------------------------

                AppendLog(
                    "Loaded: " +
                    Path.GetFileName(filePath));

                AppendLog(
                    "Mermaid diagrams: " +
                    _testCases.Count);

                // ----------------------------------------------------
                // AUTO SELECT FIRST CASE
                // ----------------------------------------------------

                if (_testCases.Count > 0)
                {
                    _lstCases.SelectedIndex =
                        0;
                }
            }
            catch (Exception ex)
            {
                AppendError(
                    ex);

                XtraMessageBox.Show(
                    this,
                    ex.Message,
                    "Load Markdown Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ============================================================
        // UPDATE TEST CASE LIST
        // ============================================================

        private void UpdateTestCaseList()
        {
            _lstCases.Items.Clear();

            foreach (MermaidTestCase testCase
                     in _testCases)
            {
                _lstCases.Items.Add(
                    testCase);
            }
        }

        // ============================================================
        // SELECT CASE
        // ============================================================

        private void LstCases_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            int index =
                _lstCases.SelectedIndex;

            if (index < 0 ||
                index >= _testCases.Count)
            {
                _selectedTestCase =
                    null;

                return;
            }

            _selectedTestCase =
                _testCases[index];

            AppendLog(
                "Selected: " +
                _selectedTestCase.Name);
        }

        // ============================================================
        // RENDER
        // ============================================================

        private void BtnRender_Click(
            object sender,
            EventArgs e)
        {
            if (_selectedTestCase == null)
            {
                XtraMessageBox.Show(
                    this,
                    "Chưa chọn Mermaid test case.",
                    "Mermaid Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                MermaidTestResult result =
                    _testRunner.Run(
                        _selectedTestCase,
                        _diagramControl);

                AppendLog(
                    result.ToString());

                if (!result.Success &&
                    result.Error != null)
                {
                    AppendError(
                        result.Error);
                }
            }
            catch (Exception ex)
            {
                AppendError(
                    ex);

                XtraMessageBox.Show(
                    this,
                    ex.Message,
                    "Render Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ============================================================
        // CLEAR
        // ============================================================

        private void BtnClear_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                _testRunner.Clear(
                    _diagramControl);

                AppendLog(
                    "Diagram cleared.");
            }
            catch (Exception ex)
            {
                AppendError(
                    ex);
            }
        }

        // ============================================================
        // LOG
        // ============================================================

        private void AppendLog(
            string message)
        {
            if (string.IsNullOrWhiteSpace(
                    message))
            {
                return;
            }

            _txtLog.AppendText(
                "[" +
                DateTime.Now.ToString(
                    "HH:mm:ss") +
                "] " +
                message +
                Environment.NewLine);
        }

        // ============================================================
        // ERROR
        // ============================================================

        private void AppendError(
            Exception error)
        {
            if (error == null)
            {
                return;
            }

            AppendLog(
                "ERROR: " +
                error.Message);

            if (error.InnerException != null)
            {
                AppendLog(
                    "INNER: " +
                    error.InnerException.Message);
            }
        }

        // ============================================================
        // DISPOSE
        // ============================================================

        protected override void Dispose(
            bool disposing)
        {
            // --------------------------------------------------------
            // Không Dispose _diagramControl thủ công.
            //
            // Nó là child của _panelCenter.
            // WinForms/DevExpress sẽ dispose theo parent.
            // --------------------------------------------------------

            base.Dispose(
                disposing);
        }
    }
}