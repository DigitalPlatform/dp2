using System;
using System.Collections.Generic;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;

namespace dp2Circulation.Order
{
    public partial class SaveDistributeExcelFileDialog : Form
    {
        public SaveDistributeExcelFileDialog()
        {
            InitializeComponent();
        }

        private void button_getOutputFileName_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的去向分配表 Excel 文件名";
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

        public string SellerFilter
        {
            get
            {
                if (comboBox_seller.Text == "<空>")
                    return "";

                if (string.IsNullOrEmpty(comboBox_seller.Text) == true)
                    return "*";
                return this.comboBox_seller.Text.Replace("<全部>", "*");
            }
            set
            {
                if (value == "<空>")
                    this.comboBox_seller.Text = "";
                else if (string.IsNullOrEmpty(value) == true)
                    this.comboBox_seller.Text = "<全部>";
                else
                    this.comboBox_seller.Text = value.Replace("*", "<全部>");
            }
        }

        public bool CreateNewOrderRecord
        {
            get
            {
                return this.checkBox_createNewOrderRecord.Checked;
            }
            set
            {
                this.checkBox_createNewOrderRecord.Checked = value;
            }
        }

        public string LibraryCode
        {
            get
            {
                return this.comboBox_libraryCode.Text;
            }
            set
            {
                this.comboBox_libraryCode.Text = value;
            }
        }

        public bool CreateNewOrderRecordVisible
        {
            get
            {
                return this.checkBox_createNewOrderRecord.Visible;
            }
            set
            {
                this.checkBox_createNewOrderRecord.Visible = value;
            }
        }

        private void comboBox_seller_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_seller.Items.Count > 0)
                return;

            this.comboBox_seller.Items.Add("<全部>");

            {
                int nRet = Program.MainForm.GetValueTable("orderSeller",
                    "",
                    out string[] values,
                    out string strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                var list = Global.FilterValuesWithLibraryCode(this.LibraryCode, new List<string>(values));

                // 去掉每个元素内的 {} 部分
                list = StringUtil.FromListString(StringUtil.GetPureSelectedValue(StringUtil.MakePathList(list)));

                this.comboBox_seller.Items.AddRange(list);
            }

            this.comboBox_seller.Items.Add("<空>");
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.comboBox_seller);
                controls.Add(this.textBox_outputFileName);
                controls.Add(this.checkBox_createNewOrderRecord);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.comboBox_seller);
                controls.Add(this.textBox_outputFileName);
                controls.Add(this.checkBox_createNewOrderRecord);
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_biblioColumns_Click(object sender, EventArgs e)
        {
            BiblioColumnOption option = new BiblioColumnOption(Program.MainForm.UserDir,
                "");
            option.LoadData(Program.MainForm.AppInfo,
                typeof(BiblioColumnOption).ToString());

            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.HidePage("tabPage_normal");
            dlg.HidePage("tabPage_templates");

            dlg.Text = "书目信息列";
            dlg.PrintOption = option;
            dlg.DataDir = Program.MainForm.UserDir;
            dlg.ColumnItems = new string[] {
                "biblio_recpath -- 书目记录路径",
                "biblio_title -- 题名",
                "biblio_author -- 责任者",
                "biblio_publication_area -- 出版者",
            };


            dlg.UiState = Program.MainForm.AppInfo.GetString(
"save_distribute",
"columnDialog_uiState",
"");
            Program.MainForm.AppInfo.LinkFormState(dlg, "distribute_biblio_outputoption_formstate");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            Program.MainForm.AppInfo.SetString(
"save_distribute",
"columnDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(Program.MainForm.AppInfo,
                typeof(BiblioColumnOption).ToString());
        }

        private void button_orderColumns_Click(object sender, EventArgs e)
        {
            OrderColumnOption option = new OrderColumnOption(Program.MainForm.UserDir,
    "");
            option.LoadData(Program.MainForm.AppInfo,
                typeof(OrderColumnOption).ToString());

            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.HidePage("tabPage_normal");
            dlg.HidePage("tabPage_templates");
            dlg.Text = "订购信息列";
            dlg.PrintOption = option;
            dlg.DataDir = Program.MainForm.UserDir;
            dlg.ColumnItems = new string[] {
                "order_recpath -- 订购记录路径",
                "order_seller -- 渠道(书商)",
                "order_price -- 订购价",
                "order_source -- 经费来源",
                "order_copy -- 复本数",
                "order_orderID -- 订单号",
                "order_class -- 类别",
                "order_batchNo -- 批次号",
                "order_catalogNo -- 书目号",
                "order_comment -- 附注",
            };

            dlg.UiState = Program.MainForm.AppInfo.GetString(
"save_distribute",
"columnDialog_uiState",
"");

            Program.MainForm.AppInfo.LinkFormState(dlg, "distribute_order_outputoption_formstate");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            Program.MainForm.AppInfo.SetString(
"save_distribute",
"columnDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(Program.MainForm.AppInfo,
                typeof(OrderColumnOption).ToString());

        }
    }
}
