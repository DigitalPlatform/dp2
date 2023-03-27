using ClosedXML.Excel;
using DigitalPlatform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace dp2Circulation.Reader
{
    /// <summary>
    /// 从外部文件导入读者信息 的 对话框
    /// </summary>
    public partial class ImportPatronDialog : Form
    {
        public ImportPatronDialog()
        {
            InitializeComponent();
        }

        private void ImportPatronDialog_Load(object sender, EventArgs e)
        {

        }

        private void ImportPatronDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ImportPatronDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton_load_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "请指定要导入的文件",
                // dlg.FileName = this.RecPathFilePath;
                // dlg.InitialDirectory = 
                // Multiselect = true,
                Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            LoadExcel(dlg.FileName);
        }

        public void LoadExcel(string filename)
        {
            var doc = new XLWorkbook(filename);

            string sheet_name = null;
            var sheet_names = doc.Worksheets.Select(x => x.Name).ToList();
            if (sheet_names.Count > 0)
            {
                // 选定一个 sheet
                sheet_name = ListDialog.GetInput(
                this,
                $"从 {filename} 装载",
                "请选择一个 Sheet",
                sheet_names,
                0,
                this.Font);
                if (sheet_name == null)
                    return;
            }
            else
                sheet_name = sheet_names[0];

            var sheet = doc.Worksheets.Where(x => x.Name == sheet_name).FirstOrDefault();
            foreach(var row in sheet.Rows())
            {

            }
        }
    }
}
