using System;
using System.Collections.Generic;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;

namespace dp2Circulation
{
    public partial class SaveEntityNewBookFileDialog : Form
    {
        public SaveEntityNewBookFileDialog()
        {
            InitializeComponent();
        }

        private void button_getOutputFileName_Click(object sender, EventArgs e)
        {

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的新书通报文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = "";
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "HTML 文件 (*.html)|*.html|Word 文件 (*.docx)|*.docx|All files (*.*)|*.*";

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
                strError = "尚未指定新书通报文件名";
                goto ERROR1;
            }

            /*
            // 检查输出文件名
            string strOutputFileName = this.textBox_outputFileName.Text;
            strError = PathUtil.CheckXlsxFileName(ref strOutputFileName);
            if (strError != null)
            {
                strError = "输出文件名 '" + strOutputFileName + "' 不合法: " + strError;
                goto ERROR1;
            }
            this.textBox_outputFileName.Text = strOutputFileName;
            */

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

        /*
不输出
没有册时不输出
没有册时输出(无)
        * */
        public string ItemsAreaStyle
        {
            get
            {
                string value = this.comboBox_items_style.Text;
                if (string.IsNullOrEmpty(value))
                    return "没有册时输出(无)";
                return value;
            }
        }

        /* 布局风格:
 * 独立表格  每个书目单独一个 table
 * 整体表格  所有书目共用一个 table (方便 paste 到 word 以后改变名字列宽度)
 * 自然段
 * */
        public string LayoutStyle
        {
            get
            {
                string value = this.comboBox_layout_style.Text;
                if (string.IsNullOrEmpty(value))
                    return "独立表格";
                return value;
            }
        }

        // 不输出书目字段名列
        public bool HideBiblioFieldName
        {
            get
            {
                return this.checkBox_hideBiblioFieldName.Checked;
            }
        }


        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_outputFileName);
                controls.Add(this.comboBox_items_style);
                controls.Add(this.comboBox_layout_style);
                controls.Add(this.checkBox_hideBiblioFieldName);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_outputFileName);
                controls.Add(this.comboBox_items_style);
                controls.Add(this.comboBox_layout_style);
                controls.Add(this.checkBox_hideBiblioFieldName);
                GuiState.SetUiState(controls, value);
            }
        }

        public static string BiblioDefPath
        {
            get
            {
                return "html_" + typeof(Order.BiblioColumnOption).ToString();
            }
        }

        public static string EntityDefPath
        {
            get
            {
                return "html_entity_" + typeof(Order.EntityColumnOption).ToString();
            }
        }

        private void button_biblioColumns_Click(object sender, EventArgs e)
        {
            var option = new NewBookColumnOption(Program.MainForm.UserDir);
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
"save_entity",
"columnDialog_uiState",
"");
            Program.MainForm.AppInfo.LinkFormState(dlg, "distribute_biblio_outputoption_formstate");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            Program.MainForm.AppInfo.SetString(
"save_entity",
"columnDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(Program.MainForm.AppInfo,
                BiblioDefPath);
        }

        private void button_orderColumns_Click(object sender, EventArgs e)
        {
            Order.EntityColumnOption option = new Order.EntityColumnOption(Program.MainForm.UserDir,
    "");
            option.LoadData(Program.MainForm.AppInfo,
                EntityDefPath);

            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.HidePage("tabPage_normal");
            dlg.HidePage("tabPage_templates");
            dlg.Text = "册信息列";
            dlg.PrintOption = option;
            dlg.DataDir = Program.MainForm.UserDir;
            dlg.ColumnItems = option.GetAllColumnItems();

            dlg.UiState = Program.MainForm.AppInfo.GetString(
"save_entity",
"columnDialog_uiState",
"");

            Program.MainForm.AppInfo.LinkFormState(dlg, "distribute_order_outputoption_formstate");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            Program.MainForm.AppInfo.SetString(
"save_entity",
"columnDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(Program.MainForm.AppInfo,
                EntityDefPath);
        }

        private void SaveDistributeExcelFileDialog_Load(object sender, EventArgs e)
        {

        }
    }

    // 适合导出册信息的书目列定义
    internal class NewBookColumnOption : Order.ExportBiblioColumnOption
    {
        public NewBookColumnOption(string strDataDir) : base(strDataDir)
        {
            this.DataDir = strDataDir;

            // Columns缺省值
            Columns.Clear();
            this.Columns.AddRange(GetAllColumns(true));
        }

        public override List<Column> GetAllColumns(bool bDefault)
        {
            var results = base.GetAllColumns(bDefault);

            if (bDefault == false)
            {
                /*
                {
                    Column column = new Column();
                    column.Name = "biblio_accessNo -- 索取号";
                    column.Caption = GetRightPart(column.Name);
                    column.MaxChars = -1;
                    results.Add(column);
                }

                {
                    // 2023/11/9
                    Column column = new Column();
                    column.Name = "biblio_itemCount -- 册数";
                    column.Caption = GetRightPart(column.Name);
                    column.MaxChars = -1;
                    results.Add(column);
                }
                */
            }

            {
                Column column = new Column();
                column.Name = "biblio_coverimageurl -- 封面图像";
                column.Caption = GetRightPart(column.Name);
                column.MaxChars = -1;
                results.Insert(0, column);
            }

            {
                Column column = new Column();
                column.Name = "biblio_items -- 册";
                column.Caption = GetRightPart(column.Name);
                column.MaxChars = -1;
                results.Add(column);
            }

            return results;
        }
    }

}
