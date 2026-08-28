using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestMd
{
    public partial class Form1 : XtraForm
    {
        public string connectionString = @"Data Source=192.168.200.57;Initial Catalog=B7R2_FCC;User ID=sa;Password=fccbrv";
        public Form1()
        {
            InitializeComponent();
            diagramControl1.Dock = DockStyle.Fill;
            diagramControl1.Resize += DiagramControl1_Resize;
        }
        private void DiagramControl1_Resize(object sender, EventArgs e)
        {
            if (diagramControl1.Items.Count > 0)
                diagramControl1.FitToDrawing();
        }
        private void LoadWorkflowFromMarkdown(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Không tìm thấy file quy trình tại:\n{filePath}",
                                "Lỗi tải sơ đồ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Đọc toàn bộ nội dung file .md với bảng mã UTF-8
                string rawContent = File.ReadAllText(filePath, Encoding.UTF8);

                // Làm sạch nội dung Markdown
                string mermaidText = CleanMermaidMarkdown(rawContent);

                // Dựng sơ đồ lên DevExpress DiagramControl
                MermaidToDevExpressAdvancedParser.ParseAndBuildDiagram(diagramControl1, mermaidText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đọc file sơ đồ: {ex.Message}",
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string CleanMermaidMarkdown(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            // Loại bỏ khối mã ```mermaid ... ``` và khoảng trắng thừa
            string cleaned = content.Replace("```mermaid", "")
                                    .Replace("```", "")
                                    .Trim();
            return cleaned;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 1. Đường dẫn tới file .md chứa mã Mermaid
            // Có thể dùng đường dẫn tương đối (trong folder ứng dụng) hoặc đường dẫn tuyệt đối
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WORKFLOW_MAIN.md");
            // Tắt trang in và lưới caro
            diagramControl1.OptionsView.ShowPageBreaks = false;
            diagramControl1.OptionsView.ShowGrid = false;
            diagramControl1.OptionsView.ShowRulers = false;
            
            // 2. Gọi đúng hàm xử lý file Markdown
            LoadWorkflowFromMarkdown(filePath);
            //diagramControl1.FitToControl();
        }
    }
}
