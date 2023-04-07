using System;
using System.Collections.Generic;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;

namespace dp2Circulation
{
    public partial class SaveBiblioExcelFileDialog : Form
    {
        public SaveBiblioExcelFileDialog()
        {
            InitializeComponent();
        }

        private void button_getOutputFileName_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的书目信息表 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = this.textBox_outputFileName.Text;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_outputFileName.Text = dlg.FileName;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_outputFileName.Text) == true)
            {
                strError = "尚未指定 Excel 文件名";
                goto ERROR1;
            }

            // 检查输出文件名
            string strOutputFileName = this.textBox_outputFileName.Text;
            strError = PathUtil.CheckXlsxFileName(ref strOutputFileName);
            if (strError != null)
            {
                strError = "输出文件名 '" + strOutputFileName + "' 不合法: " + strError;
                goto ERROR1;
            }
            this.textBox_outputFileName.Text = strOutputFileName;

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

        public string OutputFileName
        {
            get
            {
                return this.textBox_outputFileName.Text;
            }
            set
            {
                this.textBox_outputFileName.Text = value;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_outputFileName);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_outputFileName);
                GuiState.SetUiState(controls, value);
            }
        }

        public static string BiblioDefPath
        {
            get
            {
                return "biblio_" + typeof(Order.BiblioColumnOption).ToString();
            }
        }

        private void button_biblioColumns_Click(object sender, EventArgs e)
        {
            Order.ExportBiblioColumnOption option = new Order.ExportBiblioColumnOption(Program.MainForm.UserDir);
            option.LoadData(Program.MainForm.AppInfo,
                BiblioDefPath);

            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.HidePage("tabPage_normal");
            dlg.HidePage("tabPage_templates");

            dlg.Text = "书目信息列";
            dlg.PrintOption = option;
            dlg.DataDir = Program.MainForm.UserDir;
            dlg.ColumnItems = option.GetAllColumnItems();

            dlg.UiState = Program.MainForm.AppInfo.GetString(
"save_biblio",
"columnDialog_uiState",
"");
            Program.MainForm.AppInfo.LinkFormState(dlg, "biblio_outputoption_formstate");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            Program.MainForm.AppInfo.SetString(
"save_biblio",
"columnDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(Program.MainForm.AppInfo,
                BiblioDefPath);
        }

    }
}
