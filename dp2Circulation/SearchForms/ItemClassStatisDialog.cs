using DigitalPlatform.CommonControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    public partial class ItemClassStatisDialog : Form
    {
        public ItemClassStatisDialog()
        {
            InitializeComponent();
        }

        public bool OverwritePrompt
        {
            get;
            set;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.comboBox_classType.Text) == true)
            {
                strError = "尚未指定分类法";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_outputExcelFileName.Text) == true)
            {
                strError = "尚未指定输出文件名";
                goto ERROR1;
            }

            string strOutputFileName = "";
            // return:
            //      -1  出错
            //      0   文件名不合法
            //      1   文件名合法
            int nRet = ExportPatronExcelDialog.CheckExcelFileName(this.textBox_outputExcelFileName.Text,
                true,
                out strOutputFileName,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            this.textBox_outputExcelFileName.Text = strOutputFileName;

            // 提醒覆盖文件
            if (this.OverwritePrompt == true
                && File.Exists(this.FileName) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "文件 '" + this.FileName + "' 已经存在。继续操作将覆盖此文件。\r\n\r\n请问是否要覆盖此文件? (OK 覆盖；Cancel 放弃操作)",
                    "导出读者详情",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
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

        private void button_getOutputExcelFileName_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.textBox_outputExcelFileName.Text;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_outputExcelFileName.Text = dlg.FileName;
        }

        public string FileName
        {
            get
            {
                return this.textBox_outputExcelFileName.Text;
            }
            set
            {
                this.textBox_outputExcelFileName.Text = value;
            }
        }

        public bool OutputPrice
        {
            get
            {
                return this.checkBox_price.Checked;
            }
            set
            {
                this.checkBox_price.Checked = value;
            }
        }

        public string ClassType
        {
            get
            {
                return this.comboBox_classType.Text;
            }
            set
            {
                this.comboBox_classType.Text = value;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_outputExcelFileName);
                controls.Add(this.comboBox_classType);
                controls.Add(this.checkBox_price);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_outputExcelFileName);
                controls.Add(this.comboBox_classType);
                controls.Add(this.checkBox_price);
                GuiState.SetUiState(controls, value);
            }
        }
    }
}
