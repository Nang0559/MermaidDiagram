using DevExpress.XtraEditors;
using MermaidDiagram.Core.Models;
using MermaidDiagram.DevExpress.ViewControls;
using System.Xml.Linq;

namespace MermaidDiagram.DevExpress.Controls
{
    /// <summary>
    /// Form DevExpress dùng để hiển thị workflow.
    ///
    /// Architecture:
    ///
    /// DevExpressWorkflowForm
    ///          ↓
    /// DevExpressWorkflowPanel
    ///          ↓
    /// MermaidDiagramControl
    ///
    /// Form chỉ chịu trách nhiệm:
    ///     - Window lifecycle
    ///     - Caption
    ///     - Size
    ///     - Show / ShowDialog
    ///
    /// Form không chịu trách nhiệm:
    ///     - Parse Mermaid
    ///     - Graph analysis
    ///     - Layout
    ///     - Rendering
    ///     - Mapping
    ///     - Business logic
    /// </summary>
    public sealed class DevExpressWorkflowForm : XtraForm
    {
        // ============================================================
        // PANEL
        // ============================================================

        private readonly DevExpressWorkflowPanel _workflowPanel;

        /// <summary>
        /// Workflow panel hiện tại.
        /// </summary>
        public DevExpressWorkflowPanel WorkflowPanel
        {
            get
            {
                return _workflowPanel;
            }
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public DevExpressWorkflowForm(
            DevExpressWorkflowPanel workflowPanel)
        {
            _workflowPanel =
                workflowPanel
                ?? throw new ArgumentNullException(
                    nameof(workflowPanel));

            InitializeForm();
        }

        // ============================================================
        // INITIALIZE
        // ============================================================

        private void InitializeForm()
        {
            SuspendLayout();

            try
            {
                // ----------------------------------------------------
                // FORM
                // ----------------------------------------------------

                Name =
                    "devExpressWorkflowForm";

                Text =
                    "Workflow";

                StartPosition =
                    FormStartPosition.CenterScreen;

                WindowState =
                    FormWindowState.Maximized;

                MinimizeBox =
                    true;

                MaximizeBox =
                    true;

                ShowInTaskbar =
                    true;

                // ----------------------------------------------------
                // PANEL
                // ----------------------------------------------------

                _workflowPanel.Dock =
                    DockStyle.Fill;

                Controls.Add(
                    _workflowPanel);
            }
            finally
            {
                ResumeLayout(
                    false);
            }
        }

        // ============================================================
        // LOAD SOURCE
        // ============================================================

        /// <summary>
        /// Load Mermaid source thông qua Panel.
        /// </summary>
        public void Load(
            string source)
        {
            _workflowPanel.Load(
                source);
        }

        // ============================================================
        // LOAD DOCUMENT
        // ============================================================

        /// <summary>
        /// Load MermaidDocument thông qua Panel.
        /// </summary>
        public void Load(
            MermaidDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(
                    nameof(document));
            }

            _workflowPanel.Load(
                document);
        }

        // ============================================================
        // CLEAR
        // ============================================================

        /// <summary>
        /// Xóa workflow hiện tại thông qua Panel.
        /// </summary>
        public void Clear()
        {
            _workflowPanel.Clear();
        }

        // ============================================================
        // SHOW
        // ============================================================

        /// <summary>
        /// Hiển thị workflow form.
        /// </summary>
        public void ShowWorkflow()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(
                    nameof(DevExpressWorkflowForm));
            }

            Show();
        }

        // ============================================================
        // SHOW DIALOG
        // ============================================================

        /// <summary>
        /// Hiển thị workflow dưới dạng modal dialog.
        /// </summary>
        public DialogResult ShowWorkflowDialog(
            IWin32Window owner)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(
                    nameof(DevExpressWorkflowForm));
            }

            return ShowDialog(
                owner);
        }
    }
}