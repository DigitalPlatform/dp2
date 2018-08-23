using System;
using System.Collections.Generic;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;

namespace dp2Circulation.Order
{
    public partial class SaveDistributeExcelFileDialog : Form
    {
        private List<string> _libraryCodeList;

        public List<string> LibraryCodeList
        {
            get
            {
                return _libraryCodeList;
            }
            set
            {
                _libraryCodeList = value;
                this.FillLibraryCodeList();
            }
        }

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

        // 注：[总馆] 相当于 [总馆和全部分馆]。到外部会被替换为空。
        // [仅总馆] 是新增加的值。到外部也是这个值
        public string LibraryCode
        {
            get
            {
                string strValue = this.comboBox_libraryCode.Text;
                if (strValue == "[总馆]" || strValue == "[总馆和全部分馆]")
                    strValue = "";
                return strValue;
            }
            set
            {
                string strValue = value;
                if (string.IsNullOrEmpty(strValue))
                    strValue = "[总馆和全部分馆]";

                this.comboBox_libraryCode.Text = strValue;
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

            this.FillSellerList();
        }

        void FillSellerList()
        {
            this.comboBox_seller.Items.Clear();

            this.comboBox_seller.Items.Add("<全部>");

            {
                int nRet = Program.MainForm.GetValueTable("orderSeller",
                    "",
                    out string[] values,
                    out string strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                var list = new List<string>(values);

                if (this.LibraryCode == "[仅总馆]")
                    list = Global.FilterValuesWithLibraryCode("", new List<string>(values));

                // 去掉每个元素内的 {} 部分
                // list = StringUtil.FromListString(StringUtil.GetPureSelectedValue(StringUtil.MakePathList(list)));

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
                controls.Add(this.checkBox_onlyOutputBlankStateOrderRecord);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.comboBox_seller);
                controls.Add(this.textBox_outputFileName);
                controls.Add(this.checkBox_createNewOrderRecord);
                controls.Add(this.checkBox_onlyOutputBlankStateOrderRecord);
                GuiState.SetUiState(controls, value);
            }
        }

        public bool OnlyOutputBlankStateOrderRecord
        {
            get
            {
                return this.checkBox_onlyOutputBlankStateOrderRecord.Checked;
            }
            set
            {
                this.checkBox_onlyOutputBlankStateOrderRecord.Checked = value;
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
            dlg.ColumnItems = option.GetAllColumnItems();
#if NO
            dlg.ColumnItems = new string[] {
                "biblio_recpath -- 书目记录路径",
                "biblio_title -- 题名",
                "biblio_author -- 责任者",
                "biblio_publication_area -- 出版者",
            };
#endif


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
            dlg.ColumnItems = option.GetAllColumnItems();
#if NO
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
#endif

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

        private void SaveDistributeExcelFileDialog_Load(object sender, EventArgs e)
        {

        }

        void FillLibraryCodeList()
        {
            this.comboBox_libraryCode.Items.Clear();

            this.comboBox_libraryCode.Items.Add("[仅总馆]");
            if (this.LibraryCodeList != null)
                _libraryCodeList.ForEach((o) =>
                {
                    if (o == "")
                        o = "[总馆和全部分馆]";
                    this.comboBox_libraryCode.Items.Add(o);
                });

        }

        private void comboBox_libraryCode_TextChanged(object sender, EventArgs e)
        {
            this.comboBox_seller.Text = "<全部>";

            FillSellerList();
        }

        private void comboBox_seller_TextChanged(object sender, EventArgs e)
        {
            Global.FilterValue(this, (Control)sender);
        }

        private void comboBox_seller_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            CheckedComboBox.ProcessItemChecked(e, "<全部>,<all>".ToLower());
            CheckedComboBox.ProcessItemChecked(e, "<空>");
        }


    }
}
