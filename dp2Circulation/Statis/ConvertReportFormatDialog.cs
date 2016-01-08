using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.CommonControl;
using System.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 转换报表格式对话框
    /// </summary>
    public partial class ConvertReportFormatDialog : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ConvertReportFormatDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_reportDirectory.Text) == true)
            {
                strError = "尚未指定报表目录";
                goto ERROR1;
            }

            if (this.checkBox_excel.Checked == false
                && this.checkBox_html.Checked == false)
            {
                strError = "请选定至少一个目标格式";
                goto ERROR1;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button_findReportDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定报表所在目录:";
            // dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = false;
            dir_dlg.SelectedPath = this.textBox_reportDirectory.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_reportDirectory.Text = dir_dlg.SelectedPath;
        }

        /// <summary>
        /// 选择的报表目录
        /// </summary>
        public string ReportDirectory
        {
            get
            {
                return this.textBox_reportDirectory.Text;
            }
            set
            {
                this.textBox_reportDirectory.Text = value;
            }
        }

        public bool ToHtml
        {
            get
            {
                return this.checkBox_html.Checked;
            }
            set
            {
                this.checkBox_html.Checked = value;
            }
        }

        public bool ToExcel
        {
            get
            {
                return this.checkBox_excel.Checked;
            }
            set
            {
                this.checkBox_excel.Checked = value;
            }
        }

        /// <summary>
        /// 获取或设置控件尺寸状态
        /// </summary>
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_reportDirectory);
                controls.Add(this.checkBox_excel);
                controls.Add(this.checkBox_html);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_reportDirectory);
                controls.Add(this.checkBox_excel);
                controls.Add(this.checkBox_html);
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_localReportDir_Click(object sender, EventArgs e)
        {
            // string strReportDir = Path.Combine(this.MainForm.UserDir, "reports");

            this.textBox_reportDirectory.Text = ReportForm.GetReportDir();
        }
    }
}
