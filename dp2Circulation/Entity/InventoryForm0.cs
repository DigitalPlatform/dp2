using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;

// this.Channel 用法

namespace dp2Circulation
{
    public partial class InventoryForm0 : BatchPrintFormBase
    {
        string SourceStyle = "";    // "batchno" "barcodefile"

        /// <summary>
        /// 事项图标下标: 出错
        /// </summary>
        public const int TYPE_ERROR = 0;
        /// <summary>
        /// 事项图标下标: 正常
        /// </summary>
        public const int TYPE_NORMAL = 1;
        /// <summary>
        /// 事项图标下标: 已通过校验
        /// </summary>
        public const int TYPE_VERIFIED = 2;

        /// <summary>
        /// 事项图标下标: 尚未办理还书手续 但已通过校验
        /// </summary>
        public const int TYPE_NOTRETURN = 3;
        

        /// <summary>
        /// 最近使用过的条码文件全路径
        /// </summary>
        public string BarcodeFilePath = "";
        /// <summary>
        /// 最近使用过的记录路径文件全路径
        /// </summary>
        public string RecPathFilePath = "";

        // 参与排序的列号数组
        SortColumns SortColumns_in = new SortColumns();
        SortColumns SortColumns_outof = new SortColumns();

        #region 列号

        const int WM_LOADSIZE = API.WM_USER + 201;


        /// <summary>
        /// 列号: 册条码号
        /// </summary>
        public static int COLUMN_BARCODE = 0;    // 册条码号
        /// <summary>
        /// 列号: 摘要
        /// </summary>
        public static int COLUMN_SUMMARY = 1;    // 摘要
        /// <summary>
        /// 列号: 错误信息
        /// </summary>
        public static int COLUMN_ERRORINFO = 1;  // 错误信息
        /// <summary>
        /// 列号: ISBN/ISSN
        /// </summary>
        public static int COLUMN_ISBNISSN = 2;           // ISBN/ISSN

        /// <summary>
        /// 列号: 状态
        /// </summary>
        public static int COLUMN_STATE = 3;      // 状态
        /// <summary>
        /// 列号: 馆藏地点
        /// </summary>
        public static int COLUMN_LOCATION = 4;   // 馆藏地点
        /// <summary>
        /// 列号: 价格
        /// </summary>
        public static int COLUMN_PRICE = 5;      // 价格
        /// <summary>
        /// 列号: 册类型
        /// </summary>
        public static int COLUMN_BOOKTYPE = 6;   // 册类型
        /// <summary>
        /// 列号: 登录号
        /// </summary>
        public static int COLUMN_REGISTERNO = 7; // 登录号
        /// <summary>
        /// 列号: 注释
        /// </summary>
        public static int COLUMN_COMMENT = 8;    // 注释
        /// <summary>
        /// 列号: 合并注释
        /// </summary>
        public static int COLUMN_MERGECOMMENT = 9;   // 合并注释
        /// <summary>
        /// 列号: 批次号
        /// </summary>
        public static int COLUMN_BATCHNO = 10;    // 批次号
        /// <summary>
        /// 列号: 借阅者
        /// </summary>
        public static int COLUMN_BORROWER = 11;  // 借阅者
        /// <summary>
        /// 列号: 借阅日期
        /// </summary>
        public static int COLUMN_BORROWDATE = 12;    // 借阅日期
        /// <summary>
        /// 列号: 借阅期限
        /// </summary>
        public static int COLUMN_BORROWPERIOD = 13;  // 借阅期限
        /// <summary>
        /// 列号: 册记录路径
        /// </summary>
        public static int COLUMN_RECPATH = 14;   // 册记录路径
        /// <summary>
        /// 列号: 种记录路径
        /// </summary>
        public static int COLUMN_BIBLIORECPATH = 15; // 种记录路径
        /// <summary>
        /// 列号: 索取号
        /// </summary>
        public static int COLUMN_ACCESSNO = 16; // 索取号
        /// <summary>
        /// 列号: 目标记录路径
        /// </summary>
        public static int COLUMN_TARGETRECPATH = 17; // 目标记录路径

        #endregion

        public InventoryForm0()
        {
            InitializeComponent();
        }

        private void InventoryForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            CreateColumnHeader(this.listView_in);

            CreateColumnHeader(this.listView_outof);

            this.comboBox_load_type.Text = this.MainForm.AppInfo.GetString(
    "inventory_form",
    "publication_type",
    "图书");

            this.BarcodeFilePath = this.MainForm.AppInfo.GetString(
    "inventory_form",
    "barcode_filepath",
    "");

            this.RecPathFilePath = this.MainForm.AppInfo.GetString(
    "inventory_form",
    "recpath_filepath",
    "");

            this.checkBox_verify_autoUppercaseBarcode.Checked =
    this.MainForm.AppInfo.GetBoolean(
    "inventory_form",
    "auto_uppercase_barcode",
    true);
        }

        private void InventoryForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void InventoryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.MainForm.AppInfo.SetString(
    "inventory_form",
    "publication_type",
    this.comboBox_load_type.Text);

            this.MainForm.AppInfo.SetString(
    "inventory_form",
    "barcode_filepath",
    this.BarcodeFilePath);

            this.MainForm.AppInfo.SetString(
                "inventory_form",
                "recpath_filepath",
                this.RecPathFilePath);

            this.MainForm.AppInfo.SetBoolean(
    "inventory_form",
    "auto_uppercase_barcode",
    this.checkBox_verify_autoUppercaseBarcode.Checked);
        }

        private void button_load_loadFromBarcodeFile_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";

            bool bClearBefore = true;
            if (Control.ModifierKeys == Keys.Control)
                bClearBefore = false;

            if (bClearBefore == true)
                ClearBefore();
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的条码号文件名";
            dlg.FileName = this.BarcodeFilePath;
            // dlg.InitialDirectory = 
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.SourceStyle = "barcodefile";

            int nDupCount = 0;
            string strRecPathFilename = Path.GetTempFileName();
            try
            {
                nRet = ConvertBarcodeFile(
                    this.Channel,   // ...
                    dlg.FileName,
                    strRecPathFilename,
                    out nDupCount,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = LoadFromRecPathFile(
                    this.Channel,
                    strRecPathFilename,
                    this.comboBox_load_type.Text,
                    true,
                    new string[] { "summary", "@isbnissn", "targetrecpath" },
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                if (string.IsNullOrEmpty(strRecPathFilename) == false)
                {
                    File.Delete(strRecPathFilename);
                    strRecPathFilename = "";
                }
            }

            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " + nDupCount.ToString() + "个重复条码事项被忽略。");
            }

            // 记忆文件名
            this.BarcodeFilePath = dlg.FileName;
            this.Text = "典藏移交 " + Path.GetFileName(this.BarcodeFilePath);

            // 汇报数据装载情况。
            // return:
            //      0   尚未装载任何数据    
            //      1   装载已经完成
            //      2   虽然装载了数据，但是其中有错误事项
            int nState = ReportLoadState(out strError);
            if (nState != 1)
                goto ERROR1;
            return;
        ERROR1:
            this.Text = "典藏移交";
            MessageBox.Show(this, strError);

        }

        private void button_load_loadFromRecPathFile_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的册记录路径文件名";
            dlg.FileName = this.RecPathFilePath;
            // dlg.InitialDirectory = 
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.SourceStyle = "recpathfile";

            // int nDupCount = 0;
            nRet = LoadFromRecPathFile(
                this.Channel,
                dlg.FileName,
                this.comboBox_load_type.Text,
                true,
                new string[] { "summary", "@isbnissn", "targetrecpath" },
                (Control.ModifierKeys == Keys.Control ? false : true),
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 记忆文件名
            this.RecPathFilePath = dlg.FileName;
            this.Text = "典藏移交 " + Path.GetFileName(this.RecPathFilePath);
            /*
            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " + nDupCount.ToString() + "个重复条码事项被忽略。");
            }
             * */

            // 汇报数据装载情况。
            // return:
            //      0   尚未装载任何数据    
            //      1   装载已经完成
            //      2   虽然装载了数据，但是其中有错误事项
            int nState = ReportLoadState(out strError);
            if (nState != 1)
                goto ERROR1;
            return;
        ERROR1:
            this.Text = "典藏移交";
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 清除列表中现存的内容，准备装入新内容
        /// </summary>
        public override void ClearBefore()
        {
            base.ClearBefore();

            this.listView_in.Items.Clear();
            this.SortColumns_in.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_in.Columns);

            this.listView_outof.Items.Clear();
            this.SortColumns_outof.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_outof.Columns);
        }

        // 汇报数据装载情况。
        // return:
        //      0   尚未装载任何数据    
        //      1   装载已经完成
        //      2   虽然装载了数据，但是其中有错误事项
        int ReportLoadState(out string strError)
        {
            strError = "";

            int nRedCount = 0;
            int nWhiteCount = 0;

            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                ListViewItem item = this.listView_in.Items[i];

                if (item.ImageIndex == TYPE_ERROR)
                    nRedCount++;
                else
                    nWhiteCount++;
            }

            if (nRedCount != 0)
            {
                strError = "列表中有 " + nRedCount + " 个错误事项(红色行)。请修改数据后重新装载。";
                return 2;
            }

            if (nWhiteCount == 0)
            {
                strError = "尚未装载数据事项。";
                return 0;
            }

            strError = "数据事项装载正确。";
            return 1;
        }

        void LoadSize()
        {
            string strWidths = this.MainForm.AppInfo.GetString(
                "inventory_form",
                "list_in_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_in,
                    strWidths,
                    true);
            }

            strWidths = this.MainForm.AppInfo.GetString(
    "inventory_form",
    "list_outof_width",
    "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_outof,
                    strWidths,
                    true);
            }

            this.MainForm.LoadSplitterPos(
this.splitContainer_main,
"inventory_form",
"splitContainer_main_ratio");

            this.MainForm.LoadSplitterPos(
this.splitContainer_inAndOutof,
"inventory_form",
"splitContainer_inandoutof_ratio");
        }

        void SaveSize()
        {

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_in);
            this.MainForm.AppInfo.SetString(
                "inventory_form",
                "list_in_width",
                strWidths);

            strWidths = ListViewUtil.GetColumnWidthListString(this.listView_outof);
            this.MainForm.AppInfo.SetString(
                "inventory_form",
                "list_outof_width",
                strWidths);

            this.MainForm.SaveSplitterPos(
this.splitContainer_main,
"inventory_form",
"splitContainer_main_ratio");

            this.MainForm.SaveSplitterPos(
this.splitContainer_inAndOutof,
"inventory_form",
"splitContainer_inandoutof_ratio");
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOADSIZE:
                    LoadSize();
                    return;
            }
            base.DefWndProc(ref m);
        }

        void CreateColumnHeader(ListView list)
        {
            ColumnHeader columnHeader_barcode = new ColumnHeader();
            ColumnHeader columnHeader_isbnIssn = new ColumnHeader();
            ColumnHeader columnHeader_state = new ColumnHeader();
            ColumnHeader columnHeader_location = new ColumnHeader();
            ColumnHeader columnHeader_price = new ColumnHeader();
            ColumnHeader columnHeader_bookType = new ColumnHeader();
            ColumnHeader columnHeader_comment = new ColumnHeader();
            ColumnHeader columnHeader_borrower = new ColumnHeader();
            ColumnHeader columnHeader_borrowDate = new ColumnHeader();
            ColumnHeader columnHeader_borrowPeriod = new ColumnHeader();
            ColumnHeader columnHeader_recpath = new ColumnHeader();
            ColumnHeader columnHeader_errorInfo = new ColumnHeader();
            ColumnHeader columnHeader_registerNo = new ColumnHeader();
            ColumnHeader columnHeader_mergeComment = new ColumnHeader();
            ColumnHeader columnHeader_batchNo = new ColumnHeader();
            ColumnHeader columnHeader_biblioRecpath = new ColumnHeader();
            ColumnHeader columnHeader_accessno = new ColumnHeader();

            // 2009/10/27 
            ColumnHeader columnHeader_targetRecpath = new ColumnHeader();


            list.Columns.Clear();

            list.Columns.AddRange(new ColumnHeader[] {
            columnHeader_barcode,
            columnHeader_errorInfo,
            columnHeader_isbnIssn,
            columnHeader_state,
            columnHeader_location,
            columnHeader_price,
            columnHeader_bookType,
            columnHeader_registerNo,
            columnHeader_comment,
            columnHeader_mergeComment,
            columnHeader_batchNo,
            columnHeader_borrower,
            columnHeader_borrowDate,
            columnHeader_borrowPeriod,
            columnHeader_recpath,
            columnHeader_biblioRecpath,
            columnHeader_accessno,
            columnHeader_targetRecpath});


            // 
            // columnHeader_barcode
            // 
            columnHeader_barcode.Text = "册条码号";
            columnHeader_barcode.Width = 150;
            // 
            // columnHeader_errorInfo
            // 
            columnHeader_errorInfo.Text = "摘要/错误信息";
            columnHeader_errorInfo.Width = 200;
            // 
            // columnHeader_isbnIssn
            // 
            columnHeader_isbnIssn.Text = "ISBN/ISSN";
            columnHeader_isbnIssn.Width = 160;
            // 
            // columnHeader_state
            // 
            columnHeader_state.Text = "状态";
            columnHeader_state.Width = 100;
            // 
            // columnHeader_location
            // 
            columnHeader_location.Text = "馆藏地点";
            columnHeader_location.Width = 150;
            // 
            // columnHeader_price
            // 
            columnHeader_price.Text = "册价格";
            columnHeader_price.Width = 150;
            // 
            // columnHeader_bookType
            // 
            columnHeader_bookType.Text = "册类型";
            columnHeader_bookType.Width = 150;
            // 
            // columnHeader_registerNo
            // 
            columnHeader_registerNo.Text = "登录号";
            columnHeader_registerNo.Width = 150;
            // 
            // columnHeader_comment
            // 
            columnHeader_comment.Text = "附注";
            columnHeader_comment.Width = 150;
            // 
            // columnHeader_mergeComment
            // 
            columnHeader_mergeComment.Text = "合并注释";
            columnHeader_mergeComment.Width = 150;
            // 
            // columnHeader_batchNo
            // 
            columnHeader_batchNo.Text = "批次号";
            // 
            // columnHeader_borrower
            // 
            columnHeader_borrower.Text = "借阅者";
            columnHeader_borrower.Width = 150;
            // 
            // columnHeader_borrowDate
            // 
            columnHeader_borrowDate.Text = "借阅日期";
            columnHeader_borrowDate.Width = 150;
            // 
            // columnHeader_borrowPeriod
            // 
            columnHeader_borrowPeriod.Text = "借阅期限";
            columnHeader_borrowPeriod.Width = 150;
            // 
            // columnHeader_recpath
            // 
            columnHeader_recpath.Text = "册记录路径";
            columnHeader_recpath.Width = 200;
            // 
            // columnHeader_biblioRecpath
            // 
            columnHeader_biblioRecpath.Text = "种记录路径";
            columnHeader_biblioRecpath.Width = 200;
            // 
            // columnHeader_accessno
            // 
            columnHeader_accessno.Text = "索取号";
            columnHeader_accessno.Width = 200;
            // 
            // columnHeader_targetRecpath
            // 
            columnHeader_targetRecpath.Text = "目标书目记录路径";
            columnHeader_targetRecpath.Width = 200;
        }


        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            // load page
            this.comboBox_load_type.Enabled = bEnable;
            // this.button_load_loadFromBatchNo.Enabled = bEnable;
            this.button_load_loadFromBarcodeFile.Enabled = bEnable;
            this.button_load_loadFromRecPathFile.Enabled = bEnable;

            // next button
            if (bEnable == true)
                SetNextButtonEnable();
            else
                this.button_next.Enabled = false;

            // verify page
            this.textBox_verify_itemBarcode.Enabled = bEnable;
            this.button_verify_load.Enabled = bEnable;
            this.checkBox_verify_autoUppercaseBarcode.Enabled = bEnable;
            this.button_verify_loadFromBarcodeFile.Enabled = bEnable;

            // print page
            this.button_print_option.Enabled = bEnable;
            this.button_print_printList.Enabled = bEnable;
        }

        void SetNextButtonEnable()
        {
            string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_load)
            {
                // 汇报数据装载情况。
                // return:
                //      0   尚未装载任何数据    
                //      1   装载已经完成
                //      2   虽然装载了数据，但是其中有错误事项
                int nState = ReportLoadState(out strError);

                if (nState != 1)
                {
                    this.button_next.Enabled = false;
                }
                else
                    this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {

                bool bOK = ReportVerifyState(out strError);

                if (bOK == false)
                {
                    this.button_next.Enabled = false;
                }
                else
                    this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                this.button_next.Enabled = false;
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }
        }

        // 汇报校验进行情况。
        // return:
        //      true    校验已经完成
        //      false   校验尚未完成
        bool ReportVerifyState(out string strError)
        {
            strError = "";

            // 全部listview事项都是绿色状态，并且没有任何集合外事项, 才表明校验已经完成
            int nGreenCount = 0;
            int nRedCount = 0;
            int nWhiteCount = 0;

            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                ListViewItem item = this.listView_in.Items[i];

                if (item.ImageIndex == TYPE_VERIFIED)
                    nGreenCount++;
                else if (item.ImageIndex == TYPE_ERROR)
                    nRedCount++;
                else if (item.ImageIndex == TYPE_NORMAL)
                    nWhiteCount++;
            }

            if (nGreenCount == this.listView_in.Items.Count
                && this.listView_outof.Items.Count == 0)
                return true;

            strError = "验证尚未完成。\r\n\r\n列表中有:\r\n已验证事项(绿色) " + nGreenCount.ToString() + " 个\r\n错误事项(红色) " + nRedCount.ToString() + "个\r\n未验证事项(白色) " + nWhiteCount.ToString() + "个\r\n集合外事项(位于下方列表内) " + this.listView_outof.Items.Count + "个\r\n\r\n(只有全部事项都为已验证状态(绿色)，才表明验证已经完成)";
            return false;
        }

        // 检查路径所从属书目库是否为图书/期刊库？
        // return:
        //      -1  error
        //      0   不符合要求。提示信息在strError中
        //      1   符合要求
        internal override int CheckItemRecPath(string strPubType,
            string strItemRecPath,
            out string strError)
        {
            strError = "";

            string strItemDbName = Global.GetDbName(strItemRecPath);
            string strBiblioDbName = this.MainForm.GetBiblioDbNameFromItemDbName(strItemDbName);
            if (String.IsNullOrEmpty(strBiblioDbName) == true)
            {
                strError = "实体库 '" + strItemDbName + "' 未找到对应的书目库名";
                return -1;
            }

            string strIssueDbName = this.MainForm.GetIssueDbName(strBiblioDbName);

            if (strPubType == "图书")
            {
                if (String.IsNullOrEmpty(strIssueDbName) == false)
                {
                    strError = "路径 '" + strItemRecPath + "' 所从属的书目库 '" + strBiblioDbName + "' 为期刊型，和当前出版物类型 '" + strPubType + "' 不一致";
                    return 0;
                }
                return 1;
            }

            if (strPubType == "连续出版物")
            {
                if (String.IsNullOrEmpty(strIssueDbName) == true)
                {
                    strError = "路径 '" + strItemRecPath + "' 所从属的书目库 '" + strBiblioDbName + "' 为图书型，和当前出版物类型 '" + strPubType + "' 不一致";
                    return 0;
                }
                return 1;
            }

            strError = "CheckItemRecPath() 未知的出版物类型 '" + strPubType + "'";
            return -1;
        }

        internal override void SetError(ListView list,
    ref ListViewItem item,
    string strBarcodeOrRecPath,
    string strError)
        {
            if (item == null)
            {
                item = new ListViewItem(strBarcodeOrRecPath, 0);
                list.Items.Add(item);
            }
            else
            {
                Debug.Assert(item.ListView == list, "");
            }

            // item.SubItems.Add(strError);
            ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, strError);

            SetItemColor(item, TYPE_ERROR);

            // 将新加入的事项滚入视野
            list.EnsureVisible(list.Items.IndexOf(item));
        }

        internal override int VerifyItem(
            string strPubType,
            string strBarcodeOrRecPath,
            ListViewItem item,
            XmlDocument item_dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 设置timestamp/xml
            OriginItemData data = (OriginItemData)item.Tag;
            Debug.Assert(data != null, "");

            if (strPubType == "连续出版物")
            {
                // 检查是否为合订册记录或者单册记录。不能为合订成员
                // return:
                //      0   不是。图标已经设置为TYPE_ERROR
                //      1   是。图标尚未设置
                nRet = CheckBindingItem(item);
                if (nRet == 1)
                {
                    // 图标
                    // SetItemColor(item, TYPE_NORMAL);
                }
            }
            else
            {
                Debug.Assert(strPubType == "图书", "");
                // 图标
                // SetItemColor(item, TYPE_NORMAL);
            }

            // 检查条码号
            bool bIsRecPath = StringUtil.HasHead(strBarcodeOrRecPath, "@path:");
            if (bIsRecPath == false)
            {
                string strBarcode = DomUtil.GetElementText(item_dom.DocumentElement,
                    "barcode");
                if (strBarcode != strBarcodeOrRecPath)
                {
                    if (strBarcode.ToUpper() == strBarcodeOrRecPath.ToUpper())
                        strError = "用于检索的条码号 '" + strBarcodeOrRecPath + "' 和册记录中的条码号 '" + strBarcode + "' 大小写不一致";
                    else
                        strError = "用于检索的条码号 '" + strBarcodeOrRecPath + "' 和册记录中的条码号 '" + strBarcode + "' 不一致";
                    ListViewUtil.ChangeItemText(item,
                        COLUMN_ERRORINFO,
                        strError);
                    SetItemColor(item, TYPE_ERROR);
                    return -1;
                }
            }

            return 0;
        }

        // 检查是否为合订册记录或者单册记录。不能为合订成员
        // return:
        //      0   不是。即为合订成员。图标已经设置为TYPE_ERROR
        //      1   是。即为为合订册记录或者单册记录。图标未修改过
        int CheckBindingItem(ListViewItem item)
        {
            string strError = "";
            /*
            string strPublishTime = ListViewUtil.GetItemText(item, COLUMN_PUBLISHTIME);
            if (strPublishTime.IndexOf("-") == -1)
            {
                strError = "不是合订册。出版日期 '" + strPublishTime + "' 不是范围形式";
                goto ERROR1;
            }
             * */

            string strState = ListViewUtil.GetItemText(item, COLUMN_STATE);
            if (StringUtil.IsInList("合订成员", strState) == true)
            {
                strError = "不是合订册或单册。状态 '" + strState + "' 中具有'合订成员'值";
                goto ERROR1;
            }

            OriginItemData data = (OriginItemData)item.Tag;
            if (data != null && String.IsNullOrEmpty(data.Xml) == false)
            {
                // 将item record xml装入DOM，然后select出每个<item>元素
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(data.Xml);
                }
                catch
                {
                    // XML装入DOM出错，就不检查了
                    goto CONTINUE;
                }

                XmlNode node = dom.DocumentElement.SelectSingleNode("binding/bindingParent");
                if (node != null)
                {
                    strError = "不是合订册或单册。<binding>元素中具有<bindingParent>元素";
                    goto ERROR1;
                }
            }

        CONTINUE:
            // 如果必要，继续进行其他检查

            return 1;
        ERROR1:
            SetItemColor(item, TYPE_ERROR);

            // 不破坏原来的列内容，而只是增补到前面
            string strOldSummary = ListViewUtil.GetItemText(item, COLUMN_ERRORINFO);
            if (String.IsNullOrEmpty(strOldSummary) == false)
                strError = strError + " | " + strOldSummary;
            ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, strError);
            return 0;
        }

        // 设置事项的背景、前景颜色，和图标
        static void SetItemColor(ListViewItem item,
            int nType)
        {
            item.ImageIndex = nType;    // 2009/11/1

            if (nType == TYPE_ERROR)
            {
                item.BackColor = Color.Red;
                item.ForeColor = Color.White;
                item.ImageIndex = TYPE_ERROR;
            }
            else if (nType == TYPE_VERIFIED)
            {
                item.BackColor = Color.Green;
                item.ForeColor = Color.White;
                item.ImageIndex = TYPE_VERIFIED;
            }
            else if (nType == TYPE_NOTRETURN)
            {
                item.BackColor = Color.Yellow;
                item.ForeColor = Color.Black;
                item.ImageIndex = TYPE_NOTRETURN;
            }
            else if (nType == TYPE_NORMAL)
            {
                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;
                item.ImageIndex = TYPE_NORMAL;
            }
            else
            {
                Debug.Assert(false, "未知的image type");
            }

        }

        internal override void SetListViewItemText(XmlDocument dom,
    byte[] baTimestamp,
    bool bSetBarcodeColumn,
    string strRecPath,
    string strBiblioRecPath,
    string[] summary_col_names,
    SummaryInfo summary,
    ListViewItem item)
        {
            string strBiblioSummary = "";
            string strISBnISSN = "";
            string strTargetRecPath = "";

            if (summary != null && summary.Values != null)
            {
                if (summary.Values.Length > 0)
                    strBiblioSummary = summary.Values[0];
                if (summary.Values.Length > 1)
                    strISBnISSN = summary.Values[1];
                if (summary.Values.Length > 2)
                    strTargetRecPath = summary.Values[2];
            }

            OriginItemData data = null;
            data = (OriginItemData)item.Tag;
            if (data == null)
            {
                data = new OriginItemData();
                item.Tag = data;
            }
            else
            {
                data.Changed = false;
            }

            data.Xml = dom.OuterXml;    //
            data.Timestamp = baTimestamp;

            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");
            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");
            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            string strBookType = DomUtil.GetElementText(dom.DocumentElement,
                "bookType");
            string strRegisterNo = DomUtil.GetElementText(dom.DocumentElement,
                "registerNo");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");
            string strMergeComment = DomUtil.GetElementText(dom.DocumentElement,
                "mergeComment");
            string strBatchNo = DomUtil.GetElementText(dom.DocumentElement,
                "batchNo");
            string strBorrower = DomUtil.GetElementText(dom.DocumentElement,
                "borrower");
            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement,
                "borrowDate");
            string strAccessNo = DomUtil.GetElementText(dom.DocumentElement,
                "accessNo");

            // 2007/6/20 
            strBorrowDate = DateTimeUtil.LocalTime(strBorrowDate, "u");

            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
                "borrowPeriod");

            ListViewUtil.ChangeItemText(item, COLUMN_SUMMARY, strBiblioSummary);

            ListViewUtil.ChangeItemText(item, COLUMN_ACCESSNO, strAccessNo);
            ListViewUtil.ChangeItemText(item, COLUMN_ISBNISSN, strISBnISSN);

            ListViewUtil.ChangeItemText(item, COLUMN_STATE, strState);
            ListViewUtil.ChangeItemText(item, COLUMN_LOCATION, strLocation);
            ListViewUtil.ChangeItemText(item, COLUMN_PRICE, strPrice);
            ListViewUtil.ChangeItemText(item, COLUMN_BOOKTYPE, strBookType);
            ListViewUtil.ChangeItemText(item, COLUMN_REGISTERNO, strRegisterNo);
            ListViewUtil.ChangeItemText(item, COLUMN_COMMENT, strComment);
            ListViewUtil.ChangeItemText(item, COLUMN_MERGECOMMENT, strMergeComment);
            ListViewUtil.ChangeItemText(item, COLUMN_BATCHNO, strBatchNo);

            ListViewUtil.ChangeItemText(item, COLUMN_BORROWER, strBorrower);
            ListViewUtil.ChangeItemText(item, COLUMN_BORROWDATE, strBorrowDate);
            ListViewUtil.ChangeItemText(item, COLUMN_BORROWPERIOD, strBorrowPeriod);
            ListViewUtil.ChangeItemText(item, COLUMN_RECPATH, strRecPath);

            ListViewUtil.ChangeItemText(item, COLUMN_BIBLIORECPATH, strBiblioRecPath);

            ListViewUtil.ChangeItemText(item, COLUMN_TARGETRECPATH, strTargetRecPath);

            bool bBarcodeChanged = false;
            if (bSetBarcodeColumn == true)
            {
                string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
                    "barcode");
                string strOldBarcode = ListViewUtil.GetItemText(item, COLUMN_BARCODE);
                if (strBarcode != strOldBarcode)
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_BARCODE, strBarcode);
                    bBarcodeChanged = true;
                }
            }

            if (item.ImageIndex != TYPE_VERIFIED || bBarcodeChanged == true)   // 2012/4/8 原来不是验证后状态，或者条码号栏内容修改过，均可以重设置颜色。否则不应港重设颜色 --- 主要目的是保留以前验证后的状态
                SetItemColor(item, TYPE_NORMAL);
        }

        internal override ListViewItem AddToListView(ListView list,
            XmlDocument dom,
            byte[] baTimestamp,
            string strRecPath,
            string strBiblioRecPath,
            string[] summary_col_names,
            SummaryInfo summary)
        {
            string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
    "barcode");

            ListViewItem item = new ListViewItem(strBarcode, 0);

            SetListViewItemText(dom,
                baTimestamp,
                false,
                strRecPath,
                strBiblioRecPath,
                summary_col_names,
                summary,
                item);
            list.Items.Add(item);

            return item;
        }

        // 处理一小批记录的装入
        internal override int DoLoadRecords(List<string> lines,
            List<ListViewItem> items,
            bool bFillSummaryColumn,
            string[] summary_col_names,
            out string strError)
        {
            strError = "";

#if DEBUG
            if (items != null)
            {
                Debug.Assert(lines.Count == items.Count, "");
            }
#endif

            List<DigitalPlatform.LibraryClient.localhost.Record> records = new List<DigitalPlatform.LibraryClient.localhost.Record>();

            // 集中获取全部册记录信息
            for (; ; )
            {
                Application.DoEvents();

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断1";
                    return -1;
                }

                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                string[] paths = new string[lines.Count];
                lines.CopyTo(paths);
            REDO_GETRECORDS:
                long lRet = this.Channel.GetBrowseRecords(
                    this.stop,
                    paths,
                    "id,xml,timestamp", // 注意，包含了 timestamp
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    DialogResult temp_result = MessageBox.Show(this,
    strError + "\r\n\r\n是否重试?",
    this.FormCaption,
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_GETRECORDS;
                    return -1;
                }


                records.AddRange(searchresults);

                // 去掉已经做过的一部分
                lines.RemoveRange(0, searchresults.Length);

                if (lines.Count == 0)
                    break;
            }

            // 准备DOM和书目摘要等
            List<RecordInfo> infos = null;
            int nRet = GetSummaries(
                bFillSummaryColumn,
                summary_col_names,
                records,
                out infos,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(records.Count == infos.Count, "");

            List<dp2Circulation.AccountBookForm.OrderInfo> orderinfos = new List<dp2Circulation.AccountBookForm.OrderInfo>();

            this.listView_in.BeginUpdate();
            try
            {

                for (int i = 0; i < infos.Count; i++)
                {
                    Application.DoEvents();

                    if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断1";
                            return -1;
                        }

                    RecordInfo info = infos[i];

                    if (info.Record.RecordBody == null)
                    {
                        strError = "请升级 dp2Kernel 到最新版本";
                        return -1;
                    }
                    // stop.SetMessage("正在装入路径 " + strLine + " 对应的记录...");


                    string strOutputItemRecPath = "";
                    ListViewItem item = null;

                    if (items != null)
                        item = items[i];

                    // 根据册条码号，装入册记录
                    // return: 
                    //      -2  册条码号已经在list中存在了
                    //      -1  出错
                    //      1   成功
                    nRet = LoadOneItem(
                        this.comboBox_load_type.Text,
                        bFillSummaryColumn,
                        summary_col_names,
                        "@path:" + info.Record.Path,
                        info,
                        this.listView_in,
                        null,
                        out strOutputItemRecPath,
                        ref item,
                        out strError);


#if NO
                    // 准备装入订购信息
                    if (nRet != -1 && this.checkBox_load_fillOrderInfo.Checked == true)
                    {
                        Debug.Assert(item != null, "");
                        string strRefID = ListViewUtil.GetItemText(item, COLUMN_REFID);
                        if (String.IsNullOrEmpty(strRefID) == false)
                        {
                            OrderInfo order_info = new OrderInfo();
                            order_info.ItemRefID = strRefID;
                            order_info.ListViewItem = item;
                            orderinfos.Add(order_info);
                        }
                    }
#endif

                }
            }
            finally
            {
                this.listView_in.EndUpdate();
            }

#if NO
            // 从服务器获得订购记录的路径
            if (orderinfos.Count > 0)
            {
                nRet = LoadOrderInfo(
                    orderinfos,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
#endif

            return 0;
        }

#if NO
        private void button_verify_load_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.textBox_verify_itemBarcode.Text == "")
            {
                strError = "册条码号不能为空。";
                goto ERROR1;
            }

            // 2009/11/27
            if (this.checkBox_verify_autoUppercaseBarcode.Checked == true)
            {
                string strUpper = this.textBox_verify_itemBarcode.Text.ToUpper();
                if (this.textBox_verify_itemBarcode.Text != strUpper)
                    this.textBox_verify_itemBarcode.Text = strUpper;
            }

            // TODO: 验证册条码号

            // 查找集合内
            ListViewItem item = FindItem(this.listView_in,
                this.textBox_verify_itemBarcode.Text);

            if (item == null)
            {

                EnableControls(false);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在装载册 "
                    + this.textBox_verify_itemBarcode.Text
                    + " ...");
                stop.BeginLoop();

                try
                {
                    string strOutputItemRecPath = "";
                    // 根据册条码号，装入册记录
                    // return: 
                    //      -2  册条码号已经在list中存在了
                    //      -1  出错
                    //      1   成功
                    int nRet = LoadOneItem(
                        this.comboBox_load_type.Text,
                        true,
                        new string[] { "summary", "@isbnissn", "targetrecpath" },
                        this.textBox_verify_itemBarcode.Text,
                        null,   // info,
                        this.listView_outof,
                        null,
                        out strOutputItemRecPath,
                        ref item,
                        out strError);
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    EnableControls(true);
                }

                // 将新加入的事项滚入视野
                if (this.listView_outof.Items.Count != 0)
                    this.listView_outof.EnsureVisible(this.listView_outof.Items.Count - 1);

                strError = "条码为 "
                    + this.textBox_verify_itemBarcode.Text
                    + " 的册记录不在集合内。已加入到集合外列表。";
                goto ERROR1;
            }
            else
            {
                // 找到。改变icon
                /*
                item.ImageIndex = TYPE_CHECKED;
                item.BackColor = Color.Green;
                 * */
                if (item.ImageIndex == TYPE_ERROR)
                {
                    strError = "条码 "
                        + this.textBox_verify_itemBarcode.Text
                        + " 所对应的事项虽然已经包含在集合内，但数据有误，无法通过验证。\r\n\r\n请对该条码相关数据进行修改，然后刷新事项，并重新扫入条码进行验证。";
                    // 选定该事项
                    item.Selected = true;
                    // 将事项滚入视野
                    this.listView_in.EnsureVisible(this.listView_in.Items.IndexOf(item));
                    goto ERROR1;
                }

                SetItemColor(item, TYPE_VERIFIED);

                // 将新变色事项滚入视野
                this.listView_in.EnsureVisible(this.listView_in.Items.IndexOf(item));

                // this.m_nGreenItemCount++;

                SetVerifyPageNextButtonEnabled();
            }

            this.textBox_verify_itemBarcode.SelectAll();
            this.textBox_verify_itemBarcode.Focus();
            return;
        ERROR1:
            MessageBox.Show(this, strError);

            this.textBox_verify_itemBarcode.SelectAll();
            this.textBox_verify_itemBarcode.Focus();
        }

#endif
        // 查找条码匹配的ListViewItem事项
        static ListViewItem FindItem(ListView list,
            string strBarcode)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];
                if (strBarcode == item.Text)
                    return item;
            }

            return null;    // not found
        }

        void SetVerifyPageNextButtonEnabled()
        {
            if (GetGreenItemCount() >= this.listView_in.Items.Count
    && this.listView_outof.Items.Count == 0)
                this.button_next.Enabled = true;
            else
                this.button_next.Enabled = false;
        }

        // 计算绿色事项的数目
        int GetGreenItemCount()
        {
            int nCount = 0;
            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                if (this.listView_in.Items[i].ImageIndex == TYPE_VERIFIED)
                    nCount++;
            }
            return nCount;
        }

        private void button_verify_loadFromBarcodeFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的条码号文件名";
            dlg.FileName = this.BarcodeFilePath;
            // dlg.InitialDirectory = 
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            EnableControls(false);
            // MainForm.ShowProgress(true);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在验证采集到的册条码号 ...");
            stop.BeginLoop();

            try
            {
                using (StreamReader sr = new StreamReader(dlg.FileName))
                {
                    stop.SetProgressRange(0, sr.BaseStream.Length);

                    for (; ; )
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                            {
                                strError = "用户中断";
                                goto ERROR1;
                            }

                        string strLine = "";
                        strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strLine[0] == '#')
                            continue;   // 注释行

                        stop.SetMessage("正在验证册 " + strLine + " ...");
                        stop.SetProgressValue(sr.BaseStream.Position);

                        nRet = VerifyOneBarcode(ref strLine,
                            true,
                            out strError);
                        if (nRet == -1)
                        {
                            this.GetErrorInfoForm().WriteHtml(strError + "\r\n");
                        }
                    }
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("验证完成。");
                stop.HideProgress();

                EnableControls(true);
                // MainForm.ShowProgress(false);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // parameters:
        //      strBarcode  调用后可能被改变大小写
        int VerifyOneBarcode(ref string strBarcode, 
            bool bScrollInVisible,
            out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(strBarcode) == true)
            {
                strError = "册条码号不能为空。";
                return -1;
            }

            // 2009/11/27
            if (this.checkBox_verify_autoUppercaseBarcode.Checked == true)
            {
                strBarcode = strBarcode.ToUpper();
            }

            // TODO: 验证册条码号

            // 查找集合内
            ListViewItem item = FindItem(this.listView_in,
                strBarcode);

            if (item == null)
            {
#if NO
                stop.SetMessage("正在装载册 "
                    + strBarcode
                    + " ...");
#endif

                    string strOutputItemRecPath = "";
                    // 根据册条码号，装入册记录
                    // return: 
                    //      -2  册条码号已经在list中存在了
                    //      -1  出错
                    //      1   成功
                    int nRet = LoadOneItem(
                        this.comboBox_load_type.Text,
                        true,
                        new string[] { "summary", "@isbnissn", "targetrecpath" },
                        strBarcode,
                        null,   // info,
                        this.listView_outof,
                        null,
                        out strOutputItemRecPath,
                        ref item,
                        out strError);
                    if (nRet == -1)
                        return -1;

                // 将新加入的事项滚入视野
                    if (bScrollInVisible == true)
                    {
                        if (this.listView_outof.Items.Count != 0)
                            this.listView_outof.EnsureVisible(this.listView_outof.Items.Count - 1);
                    }

                strError = "条码为 "
                    + strBarcode
                    + " 的册记录不在集合内。已加入到集合外列表。";
                return -1;
            }
            else
            {
                // 找到。改变icon
                /*
                item.ImageIndex = TYPE_CHECKED;
                item.BackColor = Color.Green;
                 * */
                if (item.ImageIndex == TYPE_ERROR)
                {
                    strError = "条码 "
                        + strBarcode
                        + " 所对应的事项虽然已经包含在集合内，但数据有误，无法通过验证。\r\n\r\n请对该条码相关数据进行修改，然后刷新事项，并重新扫入条码进行验证。";
                    // 选定该事项
                    item.Selected = true;
                    // 将事项滚入视野
                    this.listView_in.EnsureVisible(this.listView_in.Items.IndexOf(item));
                    return -1;
                }

                string strBorrower = ListViewUtil.GetItemText(item, COLUMN_BORROWER);
                if (string.IsNullOrEmpty(strBorrower) == false)
                    SetItemColor(item, TYPE_NOTRETURN);
                else
                    SetItemColor(item, TYPE_VERIFIED);

                // 将新变色事项滚入视野
                if (bScrollInVisible == true)
                    this.listView_in.EnsureVisible(this.listView_in.Items.IndexOf(item));
            }


            return 0;
        }

        // 验证一个册条码号
        private void button_verify_load_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装载册 "
                + this.textBox_verify_itemBarcode.Text
                + " ...");
            stop.BeginLoop();

            try
            {

                string strBarcode = this.textBox_verify_itemBarcode.Text;
                nRet = VerifyOneBarcode(ref strBarcode,
                    true,
                    out strError);
                if (this.textBox_verify_itemBarcode.Text != strBarcode)
                    this.textBox_verify_itemBarcode.Text = strBarcode;
                if (nRet == -1)
                    goto ERROR1;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            this.textBox_verify_itemBarcode.SelectAll();
            this.textBox_verify_itemBarcode.Focus();
            return;
        ERROR1:
            MessageBox.Show(this, strError);

            this.textBox_verify_itemBarcode.SelectAll();
            this.textBox_verify_itemBarcode.Focus();
        }

        private void listView_in_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns_in.SetFirstColumn(nClickColumn,
                this.listView_in.Columns);

            // 排序
            this.listView_in.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_in);

            this.listView_in.ListViewItemSorter = null;

#if NO
            SetGroupBackcolor(
                this.listView_in,
                nClickColumn);
#endif
        }

        private void listView_outof_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns_outof.SetFirstColumn(nClickColumn,
                this.listView_outof.Columns);

            // 排序
            this.listView_outof.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_outof);

            this.listView_outof.ListViewItemSorter = null;

#if NO
            SetGroupBackcolor(
                this.listView_outof,
                nClickColumn);
#endif
        }

        private void button_print_printList_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<ListViewItem> in_items = new List<ListViewItem>();
            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                ListViewItem item = this.listView_in.Items[i];

                in_items.Add(item);
            }

            List<ListViewItem> outof_items = new List<ListViewItem>();
            for (int i = 0; i < this.listView_outof.Items.Count; i++)
            {
                ListViewItem item = this.listView_outof.Items[i];

                outof_items.Add(item);
            }
#if NO
            if (in_items.Count == 0)
            {
                MessageBox.Show(this, "警告：当前并不存在已验证的事项(绿色行)。");
            }
#endif

            PrintList("已验证清单", in_items, outof_items);

            return;
#if NO
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        void PrintList(
            string strTitle,
            List<ListViewItem> items,
            List<ListViewItem> outof_items)
        {
            string strError = "";

            // 创建一个html文件，并显示在HtmlPrintForm中。
            EnableControls(false);
            List<string> filenames = null;
            try
            {
                // Debug.Assert(false, "");

                // 构造html页面
                int nRet = BuildHtml(
                    items,
                    outof_items,
                    out filenames,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                HtmlPrintForm printform = new HtmlPrintForm();

                printform.Text = "打印" + strTitle;
                printform.MainForm = this.MainForm;
                printform.Filenames = filenames;

                this.MainForm.AppInfo.LinkFormState(printform, "printform_state");
                printform.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(printform);
            }
            finally
            {
                if (filenames != null)
                    Global.DeleteFiles(filenames);
                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int BuildHtml(
            List<ListViewItem> in_items,
            List<ListViewItem> outof_items,
            out List<string> filenames,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            Hashtable macro_table = new Hashtable();

            string strNamePath = "inventory_printoption";

            // 获得打印参数
            PrintOption option = new ItemHandoverPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);

            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            macro_table["%barcodefilepath%"] = "";
            macro_table["%barcodefilename%"] = "";
            macro_table["%recpathfilepath%"] = "";
            macro_table["%recpathfilename%"] = "";

            if (this.SourceStyle == "barcodefile")
            {
                macro_table["%barcodefilepath%"] = this.BarcodeFilePath;
                macro_table["%barcodefilename%"] = Path.GetFileName(this.BarcodeFilePath);
            }
            else if (this.SourceStyle == "recpathfile")
            {
                macro_table["%recpathfilepath%"] = this.RecPathFilePath;
                macro_table["%recpathfilename%"] = Path.GetFileName(this.RecPathFilePath);
            }

            filenames = new List<string>();    // 这个数组存放了所有文件名

            string strFileNamePrefix = this.MainForm.DataDir + "\\~inventory";

            string strFileName = "";

            Collections in_cols = new Collections();
            Collections out_cols = new Collections();
            // 统计集合
            nRet = Count(in_items,
                ref in_cols,
                out strError);
            if (nRet == -1)
                return -1;

            // 统计集合
            nRet = Count(outof_items,
                ref out_cols,
                out strError);
            if (nRet == -1)
                return -1;

            // 输出统计信息页
            {
                macro_table["%scancount%"] = (in_cols.Scaned.Count + out_cols.DataBorrowed.Count + out_cols.DataOnShelf.Count).ToString();
                macro_table["%incount%"] = in_items.Count.ToString();

                macro_table["%borrowedcount%"] = in_cols.Borrowed.Count.ToString();
                macro_table["%onshelfcount%"] = in_cols.OnShelf.Count.ToString();
                macro_table["%notreturncount%"] = in_cols.OnShelfBorrowed.Count.ToString();
                macro_table["%lostcount%"] = in_cols.Lost.Count.ToString();
                macro_table["%outcount%"] = outof_items.Count.ToString();

                macro_table["%datadir%"] = this.MainForm.DataDir;   // 便于引用datadir下templates目录内的某些文件
                macro_table["%cssfilepath%"] = this.GetAutoCssUrl(option, "itemhandover.css");  // 便于引用服务器端或“css”模板的CSS文件

                strFileName = strFileNamePrefix + "0" + ".html";

                filenames.Add(strFileName);

                string strTemplateFilePath = option.GetTemplatePageFilePath("统计页");
                    string strContent = "";
                if (String.IsNullOrEmpty(strTemplateFilePath) == true)
                    strContent = @"<html>
<head>
	<LINK href='%cssfilepath%' type='text/css' rel='stylesheet'>
</head>
<body>
	<div class='pageheader'>%date% 盘点概况</div>
	<div class='tabletitle'>%date% 盘点概况 -- %barcodefilepath%</div>
	<div class='scancount'>数据采集册数: %scancount%</div>
	<div class='sepline'><hr/></div>
	<div class='incount'>集合内册数: %incount%</div>
	<div class='borrowedcount'>借出册数: %borrowedcount%</div>
	<div class='onshelfcount'>在架册数: %onshelfcount%</div>
	<div class='notreturncount'>在架错为外借册数: %notreturncount%</div>
	<div class='lostcount'>丢失册数: %lostcount%</div>
	<div class='sepline'><hr/></div>
	<div class='outcount'>集合外册数: %outcount%</div>
	<div class='sepline'><hr/></div>
	<div class='pagefooter'></div>
</body>
</html>";
                else
                {
                    // 根据模板打印
                    // 能自动识别文件内容的编码方式的读入文本文件内容模块
                    // return:
                    //      -1  出错
                    //      0   文件不存在
                    //      1   文件存在
                    nRet = Global.ReadTextFileContent(strTemplateFilePath,
                        out strContent,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                string strResult = StringUtil.MacroString(macro_table,
                        strContent);
                    StreamUtil.WriteText(strFileName,
                        strResult);

            }

#if NO
            // 表格页循环
            for (int i = 0; i < nTablePageCount; i++)
            {
                macro_table["%pageno%"] = (i + 1 + 1).ToString();

                strFileName = strFileNamePrefix + (i + 1).ToString() + ".html";

                filenames.Add(strFileName);

                BuildPageTop(option,
                    macro_table,
                    strFileName,
                    true);
                // 行循环
                for (int j = 0; j < option.LinesPerPage; j++)
                {
                    BuildTableLine(option,
                        in_items,
                        strFileName, i, j);
                }

                BuildPageBottom(option,
                    macro_table,
                    strFileName,
                    true);
            }

            /*
            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {

            }
             * */
#endif

            return 0;
        }

        // 统计集合
        int Count(List<ListViewItem> items,
            ref Collections cols,
            out string strError)
        {
            strError = "";

            foreach (ListViewItem item in items)
            {
                string strBorrower = ListViewUtil.GetItemText(item, COLUMN_BORROWER);

                int nImageIndex = item.ImageIndex;

                if (nImageIndex == TYPE_ERROR)
                {
                    cols.Error.Add(item);
                    continue;
                }
                else if (nImageIndex == TYPE_VERIFIED)
                {
                    cols.Scaned.Add(item);
                    cols.OnShelf.Add(item);
                }
                else if (nImageIndex == TYPE_NOTRETURN)
                {
                    cols.Scaned.Add(item);
                    cols.OnShelf.Add(item);
                    cols.OnShelfBorrowed.Add(item);
                }
                else if (nImageIndex == TYPE_NORMAL)
                {
                    if (string.IsNullOrEmpty(strBorrower) == false)
                        cols.Borrowed.Add(item);
                    else
                        cols.Lost.Add(item);
                }

                if (string.IsNullOrEmpty(strBorrower) == false)
                    cols.DataBorrowed.Add(item);
                else
                    cols.DataOnShelf.Add(item);
            }

            return 0;
        }


        class Collections
        {
            // 理论在架的
            public List<ListViewItem> DataOnShelf = new List<ListViewItem>();

            // 理论外借的
            public List<ListViewItem> DataBorrowed = new List<ListViewItem>();


            // 数据采集的集合
            // 包括集合内匹配的和集合外的
            public List<ListViewItem> Scaned = new List<ListViewItem>();

            // 没有被扫描到的，处于外借状态的册
            public List<ListViewItem> Borrowed = new List<ListViewItem>();

            // 被扫描到的，实际在架的册
            public List<ListViewItem> OnShelf = new List<ListViewItem>();

            // 实际在架的中，状态错误为外借状态的。需要补充还书手续
            public List<ListViewItem> OnShelfBorrowed = new List<ListViewItem>();

            // 发现丢失的册。等于那些既不是实际在架的，也不是状态外借的册记录
            public List<ListViewItem> Lost = new List<ListViewItem>();

            // 出错的册。一般是找不到数据
            public List<ListViewItem> Error = new List<ListViewItem>();
        }


        int BuildPageTop(PrintOption option,
    Hashtable macro_table,
    string strFileName,
    bool bOutputTable)
        {
            string strCssUrl = GetAutoCssUrl(option, "itemhandover.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            StreamUtil.WriteText(strFileName,
                "<html><head>" + strLink + "</head><body>");


            // 页眉
            string strPageHeaderText = option.PageHeader;

            if (String.IsNullOrEmpty(strPageHeaderText) == false)
            {
                strPageHeaderText = StringUtil.MacroString(macro_table,
                    strPageHeaderText);

                StreamUtil.WriteText(strFileName,
                    "<div class='pageheader'>" + strPageHeaderText + "</div>");
            }

            // 表格标题
            string strTableTitleText = option.TableTitle;

            if (String.IsNullOrEmpty(strTableTitleText) == false)
            {

                strTableTitleText = StringUtil.MacroString(macro_table,
                    strTableTitleText);

                StreamUtil.WriteText(strFileName,
                    "<div class='tabletitle'>" + strTableTitleText + "</div>");
            }

            if (bOutputTable == true)
            {

                // 表格开始
                StreamUtil.WriteText(strFileName,
                    "<table class='table'>");   //   border='1'

                // 栏目标题
                StreamUtil.WriteText(strFileName,
                    "<tr class='column'>");

                for (int i = 0; i < option.Columns.Count; i++)
                {
                    Column column = option.Columns[i];

                    string strCaption = column.Caption;

                    // 如果没有caption定义，就挪用name定义
                    if (String.IsNullOrEmpty(strCaption) == true)
                        strCaption = column.Name;

                    string strClass = StringUtil.GetLeft(column.Name);

                    StreamUtil.WriteText(strFileName,
                        "<td class='" + strClass + "'>" + strCaption + "</td>");
                }

                StreamUtil.WriteText(strFileName,
                    "</tr>");
            }

            return 0;
        }

        int BuildPageBottom(PrintOption option,
    Hashtable macro_table,
    string strFileName,
    bool bOutputTable)
        {


            if (bOutputTable == true)
            {
                // 表格结束
                StreamUtil.WriteText(strFileName,
                    "</table>");
            }

            // 页脚
            string strPageFooterText = option.PageFooter;

            if (String.IsNullOrEmpty(strPageFooterText) == false)
            {
                /*
                StreamUtil.WriteText(strFileName,
                    "<hr class='pagefooter' />");
                 * */


                strPageFooterText = StringUtil.MacroString(macro_table,
                    strPageFooterText);

                StreamUtil.WriteText(strFileName,
        "<div class='pagefooter'>" + strPageFooterText + "</div>");
            }


            StreamUtil.WriteText(strFileName, "</body></html>");

            return 0;
        }

        // 获得css文件的路径(或者http:// 地址)。将根据是否具有“统计页”来自动处理
        // parameters:
        //      strDefaultCssFileName   “css”模板缺省情况下，将采用的虚拟目录中的css文件名，纯文件名
        string GetAutoCssUrl(PrintOption option,
            string strDefaultCssFileName)
        {
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                return strCssFilePath;
            else
            {
                return PathUtil.MergePath(this.MainForm.DataDir, strDefaultCssFileName);    // 缺省的
            }
        }

        private void button_print_option_Click(object sender, EventArgs e)
        {
            string strNamePath = "inventory_printoption";

            // 配置标题和风格
            PrintOption option = new ItemHandoverPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);


            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.MainForm = this.MainForm;
            dlg.Text = this.comboBox_load_type.Text + " 打印配置";
            dlg.DataDir = this.MainForm.DataDir;    // 允许新增模板页
            dlg.PrintOption = option;
            dlg.ColumnItems = new string[] {
                "no -- 序号",
                "barcode -- 册条码号",
                "summary -- 摘要",
                "isbnIssn -- ISBN/ISSN",
                "accessNo -- 索取号",
                "state -- 状态",
                "location -- 馆藏地点",
                "price -- 册价格",
                "bookType -- 册类型",
                "registerNo -- 登录号",
                "comment -- 注释",
                "mergeComment -- 合并注释",
                "batchNo -- 批次号",
                "borrower -- 借阅者",
                "borrowDate -- 借阅日期",
                "borrowPeriod -- 借阅期限",
                "recpath -- 册记录路径",
                "biblioRecpath -- 种记录路径",
                "biblioPrice -- 种价格"
            };


            this.MainForm.AppInfo.LinkFormState(dlg, "inventory_printoption_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(this.MainForm.AppInfo,
                strNamePath);

        }

    }
}
