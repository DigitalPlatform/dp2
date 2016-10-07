using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Web;
using System.Reflection;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;

using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;
using DigitalPlatform.Script;

using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 打印装订单
    /// </summary>
    public partial class PrintBindingForm : MyForm
    {
        // refid和XmlDocument之间的对照表
        Hashtable ItemXmlTable = new Hashtable();

        /// <summary>
        /// 列值表
        /// </summary>
        public Hashtable ColumnTable = new Hashtable();

        Assembly AssemblyFilter = null;
        ColumnFilterDocument MarcFilter = null;

        string SourceStyle = "";    // "batchno" "barcodefile"

        string BatchNo = "";    // 面板输入的批次号
        string LocationString = ""; // 面板输入的馆藏地点

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;
#endif
        /// <summary>
        /// 事项图标下标: 出错
        /// </summary>
        public const int TYPE_ERROR = 0;
        /// <summary>
        /// 事项图标下标: 正常
        /// </summary>
        public const int TYPE_NORMAL = 1;
        // public const int TYPE_VERIFIED = 2;

        /// <summary>
        /// 最近使用过的条码号文件全路径
        /// </summary>
        public string BarcodeFilePath = "";
        /// <summary>
        /// 最近使用过的记录路径文件全路径
        /// </summary>
        public string RecPathFilePath = "";

        // int m_nGreenItemCount = 0;

        // 参与排序的列号数组
        SortColumns SortColumns_parent = new SortColumns();
        SortColumns SortColumns_member = new SortColumns();

        #region 列号
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
        /// 列号: 索取号
        /// </summary>
        public static int COLUMN_ACCESSNO = 4; // 索取号
        /// <summary>
        /// 列号: 出版时间
        /// </summary>
        public static int COLUMN_PUBLISHTIME = 5;          // 出版时间
        /// <summary>
        /// 列号: 卷期
        /// </summary>
        public static int COLUMN_VOLUME = 6;          // 卷期

        /// <summary>
        /// 列号: 馆藏地点
        /// </summary>
        public static int COLUMN_LOCATION = 7;   // 馆藏地点
        /// <summary>
        /// 列号: 价格
        /// </summary>
        public static int COLUMN_PRICE = 8;      // 价格
        /// <summary>
        /// 列号: 册类型
        /// </summary>
        public static int COLUMN_BOOKTYPE = 9;   // 册类型
        /// <summary>
        /// 列号: 登录号
        /// </summary>
        public static int COLUMN_REGISTERNO = 10; // 登录号
        /// <summary>
        /// 列号: 注释
        /// </summary>
        public static int COLUMN_COMMENT = 11;    // 注释
        /// <summary>
        /// 列号: 合并注释
        /// </summary>
        public static int COLUMN_MERGECOMMENT = 12;   // 合并注释
        /// <summary>
        /// 列号: 批次号
        /// </summary>
        public static int COLUMN_BATCHNO = 13;    // 批次号

        /*
        public static int COLUMN_BORROWER = 10;  // 借阅者
        public static int COLUMN_BORROWDATE = 11;    // 借阅日期
        public static int COLUMN_BORROWPERIOD = 12;  // 借阅期限
         * */

        /// <summary>
        /// 列号: 册记录路径
        /// </summary>
        public static int COLUMN_RECPATH = 14;   // 册记录路径
        /// <summary>
        /// 列号: 书目记录路径
        /// </summary>
        public static int COLUMN_BIBLIORECPATH = 15; // 种记录路径

        /// <summary>
        /// 列号: 参考ID
        /// </summary>
        public static int COLUMN_REFID = 16; // 参考ID

        /*
        public static int MERGED_COLUMN_CLASS = 17;             // 类别
        public static int MERGED_COLUMN_CATALOGNO = 18;          // 书目号
        public static int MERGED_COLUMN_ORDERTIME = 19;        // 订购时间
        public static int MERGED_COLUMN_ORDERID = 20;          // 订单号
         * */

        /// <summary>
        /// 列号: 渠道
        /// </summary>
        public static int COLUMN_SELLER = 17;             // 渠道
        /// <summary>
        /// 列号: 经费来源
        /// </summary>
        public static int COLUMN_SOURCE = 18;             // 经费来源
        /// <summary>
        /// 列号: 完好率
        /// </summary>
        public static int COLUMN_INTACT = 19;        // 完好率
        /*
        public static int COLUMN_BARCODE = 0;    // 册条码号
        public static int COLUMN_SUMMARY = 1;    // 摘要
        public static int COLUMN_ERRORINFO = 1;  // 错误信息
        public static int COLUMN_ISBNISSN = 2;           // ISBN/ISSN

        public static int COLUMN_STATE = 3;      // 状态
        public static int COLUMN_LOCATION = 4;   // 馆藏地点
        public static int COLUMN_PRICE = 5;      // 价格
        public static int COLUMN_BOOKTYPE = 6;   // 册类型
        public static int COLUMN_REGISTERNO = 7; // 登录号
        public static int COLUMN_COMMENT = 8;    // 注释
        public static int COLUMN_MERGECOMMENT = 9;   // 合并注释
        public static int COLUMN_BATCHNO = 10;    // 批次号
        public static int COLUMN_BORROWER = 11;  // 借阅者
        public static int COLUMN_BORROWDATE = 12;    // 借阅日期
        public static int COLUMN_BORROWPERIOD = 13;  // 借阅期限
        public static int COLUMN_RECPATH = 14;   // 册记录路径
        public static int COLUMN_BIBLIORECPATH = 15; // 种记录路径
        public static int COLUMN_ACCESSNO = 16; // 索取号
        public static int COLUMN_TARGETRECPATH = 17; // 目标记录路径
         * */
#endregion

        const int WM_LOADSIZE = API.WM_USER + 201;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PrintBindingForm()
        {
            InitializeComponent();
        }

        private void PrintBindingForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            CreateColumnHeader(this.listView_parent);

            CreateColumnHeader(this.listView_member);

#if NO
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.BarcodeFilePath = this.MainForm.AppInfo.GetString(
                "printbindingform",
                "barcode_filepath",
                "");

            this.BatchNo = this.MainForm.AppInfo.GetString(
                "printbindingform",
                "batchno",
                "");

            this.LocationString = this.MainForm.AppInfo.GetString(
                "printbindingform",
                "location_string",
                "");
            this.comboBox_sort_sortStyle.Text = this.MainForm.AppInfo.GetString(
                "printbindingform",
                "sort_style",
                "书目记录路径");

            this.checkBox_print_barcodeFix.Checked = this.MainForm.AppInfo.GetBoolean(
                "printbindingform",
                "barcode_fix",
                false);

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);
        }

        private void PrintBindingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetString(
                    "printbindingform",
                    "barcode_filepath",
                    this.BarcodeFilePath);

                this.MainForm.AppInfo.SetString(
                    "printbindingform",
                    "batchno",
                    this.BatchNo);

                this.MainForm.AppInfo.SetString(
                    "printbindingform",
                    "location_string",
                    this.LocationString);

                this.MainForm.AppInfo.SetString(
        "printbindingform",
        "sort_style",
        this.comboBox_sort_sortStyle.Text);

                this.MainForm.AppInfo.SetBoolean(
        "printbindingform",
        "barcode_fix",
        this.checkBox_print_barcodeFix.Checked);
            }

            SaveSize();

            if (this.ErrorInfoForm != null)
            {
                try
                {
                    this.ErrorInfoForm.Close();
                }
                catch
                {
                }
            }
        }

        private void PrintBindingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {
                // Debug.Assert(false, "");
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }
#endif

            /*
            if (this.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有册信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "printbindingform",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
             * */
        }

        /*public*/ void LoadSize()
        {
#if NO
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state");
#endif

            string strWidths = this.MainForm.AppInfo.GetString(
                "printbindingform",
                "list_parent_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_parent,
                    strWidths,
                    true);
            }

            strWidths = this.MainForm.AppInfo.GetString(
    "printbindingform",
    "list_member_width",
    "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_member,
                    strWidths,
                    true);
            }
        }

        /*public*/ void SaveSize()
        {
#if NO
            MainForm.AppInfo.SaveMdiChildFormStates(this,
                "mdi_form_state");
#endif
            /*
            // 如果MDI子窗口不是MainForm刚刚准备退出时的状态，恢复它。为了记忆尺寸做准备
            if (this.WindowState != this.MainForm.MdiWindowState)
                this.WindowState = this.MainForm.MdiWindowState;
             * */

            string strWidths = ListViewUtil.GetColumnWidthListStringExt(this.listView_parent);
            this.MainForm.AppInfo.SetString(
                "printbindingform",
                "list_parent_width",
                strWidths);

            strWidths = ListViewUtil.GetColumnWidthListStringExt(this.listView_member);
            this.MainForm.AppInfo.SetString(
                "printbindingform",
                "list_member_width",
                strWidths);
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

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            // load page
            this.button_load_loadFromBatchNo.Enabled = bEnable;
            this.button_load_loadFromBarcodeFile.Enabled = bEnable;
            this.button_load_loadFromRecPathFile.Enabled = bEnable;

            // next button
            if (bEnable == true)
                SetNextButtonEnable();
            else
                this.button_next.Enabled = false;


            // print page
            this.button_print_option.Enabled = bEnable;
            this.button_print_print.Enabled = bEnable;

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
            else if (this.tabControl_main.SelectedTab == this.tabPage_sort)
            {
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

            for (int i = 0; i < this.listView_parent.Items.Count; i++)
            {
                ListViewItem item = this.listView_parent.Items[i];

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

        private void button_load_loadFromBatchNo_Click(object sender, EventArgs e)
        {
            SearchByBatchnoForm dlg = new SearchByBatchnoForm();
            MainForm.SetControlFont(dlg, this.Font, false);

            this.ClearErrorInfoForm();

            dlg.BatchNo = this.BatchNo;
            dlg.ItemLocation = this.LocationString;

            dlg.CfgSectionName = "PrintBindingForm_SearchByBatchnoForm";
            dlg.GetLocationValueTable -= new GetValueTableEventHandler(dlg_GetLocationValueTable);
            dlg.GetLocationValueTable += new GetValueTableEventHandler(dlg_GetLocationValueTable);

            dlg.GetBatchNoTable -= new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.GetBatchNoTable += new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.RefDbName = "";

            dlg.MainForm = this.MainForm;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.SourceStyle = "batchno";

            // 记忆
            this.BatchNo = dlg.BatchNo;
            this.LocationString = dlg.ItemLocation;

            string strMatchLocation = dlg.ItemLocation;

            if (strMatchLocation == "<不指定>")
                strMatchLocation = null;    // null和""的区别很大

            string strError = "";
            int nRet = 0;

            if (Control.ModifierKeys == Keys.Control)
            {
            }
            else
            {
                /*
                if (this.Changed == true)
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
                        "当前窗口内有册信息被修改后尚未保存。若此时为装载新内容而清除原有信息，则未保存信息将丢失。\r\n\r\n确实要装载新内容? ",
                        "ItemHandoverForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                    {
                        return; // 放弃
                    }
                }
                 * */

                this.listView_parent.Items.Clear();
                this.SortColumns_parent.Clear();
                SortColumns.ClearColumnSortDisplay(this.listView_parent.Columns);

                this.listView_member.Items.Clear();
                this.SortColumns_member.Clear();
                SortColumns.ClearColumnSortDisplay(this.listView_member.Columns);
            }

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            try
            {
                stop.SetProgressRange(0, 100);
                // stop.SetProgressValue(0);

                long lRet = 0;
                if (this.BatchNo == "<不指定>")
                {
                    // 2013/3/25
                    lRet = Channel.SearchItem(
                    stop,
                        // 2010/2/25 changed
                     "<all series>",
                    "", //
                    -1,
                    "__id",
                    "left",
                    this.Lang,
                    "batchno",   // strResultSetName
                    "",    // strSearchStyle
                    "", // strOutputStyle
                    out strError);
                    if (lRet == 0)
                    {
                        // TODO: 优化提示
                        strError = "检索全部 '连续出版物' 类型的册记录没有命中记录。";
                        goto ERROR1;
                    }
                }
                else
                    lRet = Channel.SearchItem(
                        stop,
                        // 2010/2/25 changed
                         "<all series>",
                        dlg.BatchNo,
                        -1,
                        "批次号",
                        "exact",
                        this.Lang,
                        "batchno",   // strResultSetName
                        "",    // strSearchStyle
                        "", // strOutputStyle
                        out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "批次号 '" + dlg.BatchNo + "' 没有命中记录。";
                    goto ERROR1;
                }

                int nDupCount = 0;

                long lHitCount = lRet;

                stop.SetProgressRange(0, lHitCount);
                stop.SetProgressValue(0);


                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            MessageBox.Show(this, "用户中断");
                            return;
                        }
                    }


                    lRet = Channel.GetSearchResult(
                        stop,
                        "batchno",   // strResultSetName
                        lStart,
                        lCount,
                        "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSearchResult() error: " + strError;
                        // Debug.Assert(false, "");
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        MessageBox.Show(this, "未命中");
                        return;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DigitalPlatform.LibraryClient.localhost.Record result_item = searchresults[i];

                        string strBarcode = result_item.Cols[0];
                        string strRecPath = result_item.Path;

                        // 检查路径所从属书目库是否为图书/期刊库？
                        // return:
                        //      -1  error
                        //      0   不符合要求。提示信息在strError中
                        //      1   符合要求
                        nRet = CheckItemRecPath("连续出版物",
                            strRecPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            GetErrorInfoForm().WriteHtml("路径为 " + strRecPath + " 的册记录 " + strError + "\r\n");
                        }

                        /*
                        // 如果册条码号为空，则改用路径装载
                        // 2009/8/6
                        if (String.IsNullOrEmpty(strBarcode) == true)
                        {
                            strBarcode = "@path:" + strRecPath;
                        }*/

                        // 加速
                        strBarcode = "@path:" + strRecPath;


                        string strOutputItemRecPath = "";
                        // 根据册条码号或者记录路径，装入册记录
                        // return: 
                        //      -2  册条码号已经在list中存在了
                        //      -1  出错
                        //      0   因为馆藏地点不匹配，没有加入list中
                        //      1   成功
                        nRet = LoadOneItem(strBarcode,
                            this.listView_parent,
                            strMatchLocation,
                            out strOutputItemRecPath,
                            out strError);
                        if (nRet == -2)
                            nDupCount++;
                        /*
                        ReaderSearchForm.NewLine(
                            this.listView_records,
                            searchresults[i].Path,
                            searchresults[i].Cols);
                         * */
                        stop.SetProgressValue(lStart + i + 1);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                if (this.listView_parent.Items.Count == 0
                    && strMatchLocation != null)
                {
                    strError = "虽然批次号 '" + dlg.BatchNo + "' 命中了记录 " + lHitCount.ToString() + " 条, 但它们均未能匹配馆藏地点 '" + strMatchLocation + "' 。";
                    goto ERROR1;
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
                // MainForm.ShowProgress(false);
            }

            this.Text = "打印装订单 -- " + this.SourceDescription;

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
            this.Text = "打印装订单";
            MessageBox.Show(this, strError);
        }

        void dlg_GetBatchNoTable(object sender, GetKeyCountListEventArgs e)
        {
            Global.GetBatchNoTable(e,
                this,
                "连续出版物",
                "item",
                this.stop,
                this.Channel);
        }

        void dlg_GetLocationValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            e.values = values;
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_load)
            {
                this.tabControl_main.SelectedTab = this.tabPage_sort;
                this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_sort)
            {
                // 进行排序
                // return:
                //      -1  出错
                //      0   没有必要排序
                //      1   已完成排序
                int nRet = DoSort(this.comboBox_sort_sortStyle.Text,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.tabControl_main.SelectedTab = this.tabPage_print;
                this.button_next.Enabled = false;
                this.button_print_print.Focus();
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }

            this.SetNextButtonEnable();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // return:
        //      -1  出错
        //      0   没有必要排序
        //      1   已完成排序
        int DoSort(string strSortStyle,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strSortStyle) == true
                || strSortStyle == "<无>")
                return 0;

            if (strSortStyle == "书目记录路径")
            {
                // 注：本函数如果发现第一列已经设置好，则不改变其方向。不过，这并不意味着其方向一定是升序
                this.SortColumns_parent.SetFirstColumn(COLUMN_BIBLIORECPATH,
                    this.listView_parent.Columns,
                    false);
            }
            else
            {
                strError = "未知的排序风格 '" + strSortStyle + "'";
                return -1;
            }

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            try
            {

                // 排序
                this.listView_parent.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_parent);
                this.listView_parent.ListViewItemSorter = null;
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            SetGroupBackcolor(
                this.listView_parent,
                this.SortColumns_parent[0].No);

            return 1;
        }

        void ForceSortColumnsParent(int nClickColumn)
        {
            // 注：本函数如果发现第一列已经设置好，则不改变其方向。不过，这并不意味着其方向一定是升序
            this.SortColumns_parent.SetFirstColumn(nClickColumn,
                this.listView_parent.Columns,
                false);

            // 排序
            this.listView_parent.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_parent);
            this.listView_parent.ListViewItemSorter = null;

            SetGroupBackcolor(
                this.listView_parent,
                nClickColumn);
        }

        // 根据排序键值的变化分组设置颜色
        static void SetGroupBackcolor(
            ListView list,
            int nSortColumn)
        {
            string strPrevText = "";
            bool bDark = false;
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];

                if (item.ImageIndex == TYPE_ERROR)
                    continue;

                string strThisText = "";
                try
                {
                    strThisText = item.SubItems[nSortColumn].Text;
                }
                catch
                {
                }

                if (strThisText != strPrevText)
                {
                    // 变化颜色
                    if (bDark == true)
                        bDark = false;
                    else
                        bDark = true;

                    strPrevText = strThisText;
                }

                if (bDark == true)
                {
                    item.BackColor = Color.LightGray;

                    if (item.ForeColor == item.BackColor)
                        item.ForeColor = Color.Black;
                }
                else
                {
                    item.BackColor = Color.White;

                    if (item.ForeColor == item.BackColor)
                        item.ForeColor = Color.Black;
                }
            }
        }


        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_load)
            {
                this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_sort)
            {
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

        // 设置listview栏目标题
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

            ColumnHeader columnHeader_refID = new ColumnHeader();
            ColumnHeader columnHeader_publishTime = new ColumnHeader();
            ColumnHeader columnHeader_volume = new ColumnHeader();
            /*
            ColumnHeader columnHeader_class = new ColumnHeader();
            ColumnHeader columnHeader_catalogNo = new ColumnHeader();
            ColumnHeader columnHeader_orderTime = new ColumnHeader();
            ColumnHeader columnHeader_orderID = new ColumnHeader();
             * */
            ColumnHeader columnHeader_seller = new ColumnHeader();
            ColumnHeader columnHeader_source = new ColumnHeader();
            ColumnHeader columnHeader_intact = new ColumnHeader();


            list.Columns.Clear();

            list.Columns.AddRange(new ColumnHeader[] {
            columnHeader_barcode,
            columnHeader_errorInfo,
            columnHeader_isbnIssn,
            columnHeader_state,

                            columnHeader_accessno,
            columnHeader_publishTime,
            columnHeader_volume,


            columnHeader_location,
            columnHeader_price,
            columnHeader_bookType,
            columnHeader_registerNo,
            columnHeader_comment,
            columnHeader_mergeComment,
            columnHeader_batchNo,

                /*
            columnHeader_borrower,
            columnHeader_borrowDate,
            columnHeader_borrowPeriod,
                 * */

            columnHeader_recpath,
            columnHeader_biblioRecpath,
                            columnHeader_refID,

                /*
            columnHeader_class,
            columnHeader_catalogNo,
            columnHeader_orderTime,
            columnHeader_orderID,
                 * */
            columnHeader_seller,
            columnHeader_source,
            columnHeader_intact});

            // 
            // columnHeader_isbnIssn
            // 
            columnHeader_isbnIssn.Text = "ISBN/ISSN";
            columnHeader_isbnIssn.Width = 160;
            // 
            // columnHeader_volume
            // 
            columnHeader_volume.Text = "卷期";
            columnHeader_volume.Width = 100;
            // 
            // columnHeader_publishTime
            // 
            columnHeader_publishTime.Text = "出版时间";
            columnHeader_publishTime.Width = 100;
            /*
            // 
            // columnHeader_class
            // 
            columnHeader_class.Text = "类别";
            columnHeader_class.Width = 100;

            // 
            // columnHeader_catalogNo
            // 
            columnHeader_catalogNo.Text = "书目号";
            columnHeader_catalogNo.Width = 100;

            // 
            // columnHeader_orderTime
            // 
            columnHeader_orderTime.Text = "订购时间";
            columnHeader_orderTime.Width = 150;
            // 
            // columnHeader_orderID
            // 
            columnHeader_orderID.Text = "订单号";
            columnHeader_orderID.Width = 150;
             * */
            // 
            // columnHeader_seller
            // 
            columnHeader_seller.Text = "渠道";
            columnHeader_seller.Width = 150;
            // 
            // columnHeader_source
            // 
            columnHeader_source.Text = "经费来源";
            columnHeader_source.Width = 150;
            // 
            // columnHeader_intact
            // 
            columnHeader_intact.Text = "完好率";
            columnHeader_intact.Width = 150;
            // 
            // columnHeader_refID
            // 
            columnHeader_refID.Text = "参考ID";
            columnHeader_refID.Width = 100;



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
            /*
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
             * */

        }

        // 根据册条码号或者记录路径，装入册记录
        // parameters:
        //      strBarcodeOrRecPath 册条码号或者记录路径。如果内容前缀为"@path:"则表示为路径
        //      strMatchLocation    附加的馆藏地点匹配条件。如果==null，表示没有这个附加条件(注意，""和null含义不同，""表示确实要匹配这个值)
        // return: 
        //      -2  册条码号已经在list中存在了(行没有加入listview中)
        //      -1  出错(注意表示出错的行已经加入listview中了)
        //      0   因为馆藏地点不匹配，没有加入list中
        //      1   成功
        /*public*/ int LoadOneItem(
            string strBarcodeOrRecPath,
            ListView list,
            string strMatchLocation,
            out string strOutputItemRecPath,
            out string strError)
        {
            strError = "";
            strOutputItemRecPath = "";

            // 判断是否有 @path: 前缀，便于后面分支处理
            bool bIsRecPath = StringUtil.HasHead(strBarcodeOrRecPath, "@path:"); ;

            string strItemXml = "";
            string strBiblioText = "";

            string strBiblioRecPath = "";

            byte[] item_timestamp = null;

            long lRet = Channel.GetItemInfo(
                stop,
                strBarcodeOrRecPath,
                "xml",
                out strItemXml,
                out strOutputItemRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strBiblioRecPath,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                ListViewItem item = null;

                if (bIsRecPath == false)
                    item = new ListViewItem(strBarcodeOrRecPath, 0);
                else
                    item = new ListViewItem("", 0); // 暂时还没有办法知道条码

                // 2009/10/29
                OriginItemData data = new OriginItemData();
                item.Tag = data;
                data.Timestamp = item_timestamp;
                data.Xml = strItemXml;

                ListViewUtil.ChangeItemText(item,
                    COLUMN_ERRORINFO,
                    strError);
                // item.SubItems.Add(strError);

                SetItemColor(item, TYPE_ERROR);
                list.Items.Add(item);

                // 将新加入的事项滚入视野
                list.EnsureVisible(list.Items.Count - 1);

                goto ERROR1;
            }

            string strBiblioSummary = "";
            string strISBnISSN = "";
            // string strTargetRecPath = "";

            // 看看册条码号是否有重复?
            // 顺便获得同种的事项
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem curitem = list.Items[i];

                if (bIsRecPath == false)
                {
                    if (strBarcodeOrRecPath == curitem.Text)
                    {
                        strError = "册条码号 " + strBarcodeOrRecPath + " 发生重复";
                        return -2;
                    }
                }
                else
                {
                    if (strBarcodeOrRecPath == ListViewUtil.GetItemText(curitem, COLUMN_RECPATH))
                    {
                        strError = "记录路径 " + strBarcodeOrRecPath + " 发生重复";
                        return -2;
                    }
                }

                if (strBiblioSummary == "" && curitem.ImageIndex != TYPE_ERROR)
                {
                    if (curitem.SubItems[COLUMN_BIBLIORECPATH].Text == strBiblioRecPath)
                    {
                        strBiblioSummary = ListViewUtil.GetItemText(curitem, COLUMN_SUMMARY);
                        strISBnISSN = ListViewUtil.GetItemText(curitem, COLUMN_ISBNISSN);
                        // strTargetRecPath = ListViewUtil.GetItemText(curitem, COLUMN_TARGETRECPATH);
                    }
                }
            }

            if (strBiblioSummary == "")
            {
                string[] formats = new string[2];
                formats[0] = "summary";
                formats[1] = "@isbnissn";
                string[] results = null;
                byte[] timestamp = null;

                Debug.Assert(String.IsNullOrEmpty(strBiblioRecPath) == false, "strBiblioRecPath值不能为空");

                lRet = Channel.GetBiblioInfos(
                    stop,
                    strBiblioRecPath,
                    "",
                    formats,
                    out results,
                    out timestamp,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                        strError = "书目记录 '" + strBiblioRecPath + "' 不存在";

                    strBiblioSummary = "获得书目摘要时发生错误: " + strError;
                }
                else
                {
                    Debug.Assert(results != null && results.Length == 2, "results必须包含3个元素");
                    strBiblioSummary = results[0];
                    strISBnISSN = results[1];
                    // strTargetRecPath = results[2];
                }
            }

            // 剖析一个册的xml记录，取出有关信息放入listview中

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "PrintBindingForm dom.LoadXml() {9A208D62-AB50-420B-A83D-E82E6A00A9AF} exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }


            // 附加的馆藏地点匹配
            if (strMatchLocation != null)
            {
                string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                    "location");

                // 2013/3/25
                if (strLocation == null)
                    strLocation = "";

                if (strMatchLocation != strLocation)
                    return 0;
            }

            {
                ListViewItem item = AddToListView(list,
                    dom,
                    strOutputItemRecPath,
                    strBiblioSummary,
                    strISBnISSN,
                    strBiblioRecPath);

                // 设置timestamp/xml
                OriginItemData data = (OriginItemData)item.Tag;
                Debug.Assert(data != null, "");
                data.Timestamp = item_timestamp;
                data.Xml = strItemXml;

                // 检查是否为合订册记录
                // return:
                //      0   不是。图标已经设置为TYPE_ERROR
                //      1   是。图标尚未设置
                int nRet = CheckBindingItem(item);
                if (nRet == 1)
                {
                    // 图标
                    // item.ImageIndex = TYPE_NORMAL;
                    SetItemColor(item, TYPE_NORMAL);
                }

                // 将新加入的事项滚入视野
                list.EnsureVisible(list.Items.Count - 1);

                // 检查条码号
                if (bIsRecPath == false)
                {
                    string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
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
                        goto ERROR1;
                    }
                }
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 检查是否为合订册记录
        // return:
        //      0   不是。图标已经设置为TYPE_ERROR
        //      1   是。图标尚未设置
        int CheckBindingItem(ListViewItem item)
        {
            string strError = "";
            string strPublishTime = ListViewUtil.GetItemText(item, COLUMN_PUBLISHTIME);
            if (strPublishTime.IndexOf("-") == -1)
            {
                strError = "不是合订册。出版日期 '"+strPublishTime+"' 不是范围形式";
                goto ERROR1;
            }

            string strState = ListViewUtil.GetItemText(item, COLUMN_STATE);
            if (StringUtil.IsInList("合订成员", strState) == true)
            {
                strError = "不是合订册。状态 '" + strState + "' 中具有'合订成员'值";
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
                    strError = "不是合订册。<binding>元素中具有<bindingParent>元素";
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
#if NO
            else if (nType == TYPE_VERIFIED)
            {
                item.BackColor = Color.Green;
                item.ForeColor = Color.White;
                item.ImageIndex = TYPE_VERIFIED;
            }
#endif
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

        /*public*/ static ListViewItem AddToListView(ListView list,
    XmlDocument dom,
    string strRecPath,
    string strBiblioSummary,
    string strISBnISSN,
    string strBiblioRecPath)
        {
            string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
    "barcode");

            ListViewItem item = new ListViewItem(strBarcode, 0);

            SetListViewItemText(dom,
                false,
                strRecPath,
                strBiblioSummary,
                strISBnISSN,
                strBiblioRecPath,
                item);
            list.Items.Add(item);

            return item;
        }

        // 根据册记录DOM设置ListViewItem除第一列以外的文字
        // 本函数会自动把事项的data.Changed设置为false
        // parameters:
        //      bSetBarcodeColumn   是否要设置条码列内容(第一列)
        /*public*/ static void SetListViewItemText(XmlDocument dom,
            bool bSetBarcodeColumn,
            string strRecPath,
            string strBiblioSummary,
            string strISBnISSN,
            string strBiblioRecPath,
            ListViewItem item)
        {
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

            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");
            string strPublishTime = DomUtil.GetElementText(dom.DocumentElement,
                "publishTime");
            string strVolume = DomUtil.GetElementText(dom.DocumentElement,
                "volume");


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
            string strAccessNo = DomUtil.GetElementText(dom.DocumentElement,
                "accessNo");
            string strRefID = DomUtil.GetElementText(dom.DocumentElement,
                "refID");
            /*
            string strBinding = DomUtil.GetElementInnerXml(dom.DocumentElement,
                "binding");
             * */

            string strIntact = DomUtil.GetElementText(dom.DocumentElement,
                "intact");
            string strSeller = DomUtil.GetElementText(dom.DocumentElement,
                "seller");
            string strSource = DomUtil.GetElementText(dom.DocumentElement,
                "source");
            /*
            string strBorrower = DomUtil.GetElementText(dom.DocumentElement,
                "borrower");
            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement,
                "borrowDate");
            // 2007/6/20
            strBorrowDate = DateTimeUtil.LocalTime(strBorrowDate, "u");
            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
                "borrowPeriod");
            strBorrowDate = DateTimeUtil.LocalTime(strBorrowDate, "u");
            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
                "borrowPeriod");
             * */



            ListViewUtil.ChangeItemText(item, COLUMN_SUMMARY, strBiblioSummary);

            ListViewUtil.ChangeItemText(item, COLUMN_STATE, strState);
            ListViewUtil.ChangeItemText(item, COLUMN_ACCESSNO, strAccessNo);
            ListViewUtil.ChangeItemText(item, COLUMN_PUBLISHTIME, strPublishTime);
            ListViewUtil.ChangeItemText(item, COLUMN_VOLUME, strVolume);

            ListViewUtil.ChangeItemText(item, COLUMN_ISBNISSN, strISBnISSN);


            ListViewUtil.ChangeItemText(item, COLUMN_LOCATION, strLocation);
            ListViewUtil.ChangeItemText(item, COLUMN_PRICE, strPrice);
            ListViewUtil.ChangeItemText(item, COLUMN_BOOKTYPE, strBookType);
            ListViewUtil.ChangeItemText(item, COLUMN_REGISTERNO, strRegisterNo);
            ListViewUtil.ChangeItemText(item, COLUMN_COMMENT, strComment);
            ListViewUtil.ChangeItemText(item, COLUMN_MERGECOMMENT, strMergeComment);
            ListViewUtil.ChangeItemText(item, COLUMN_BATCHNO, strBatchNo);

            /*
            ListViewUtil.ChangeItemText(item, COLUMN_BORROWER, strBorrower);
            ListViewUtil.ChangeItemText(item, COLUMN_BORROWDATE, strBorrowDate);
            ListViewUtil.ChangeItemText(item, COLUMN_BORROWPERIOD, strBorrowPeriod);
             * */

            ListViewUtil.ChangeItemText(item, COLUMN_RECPATH, strRecPath);
            ListViewUtil.ChangeItemText(item, COLUMN_BIBLIORECPATH, strBiblioRecPath);
            ListViewUtil.ChangeItemText(item, COLUMN_REFID, strRefID);

            ListViewUtil.ChangeItemText(item, COLUMN_SELLER, strSeller);
            ListViewUtil.ChangeItemText(item, COLUMN_SOURCE, strSource);
            ListViewUtil.ChangeItemText(item, COLUMN_INTACT, strIntact);

            if (bSetBarcodeColumn == true)
            {
                string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
                    "barcode");
                ListViewUtil.ChangeItemText(item, COLUMN_BARCODE, strBarcode);
            }
        }

        // 准备脚本环境
        int PrepareMarcFilter(string strFilterFileName,
            out ColumnFilterDocument filter,
            out string strError)
        {
            strError = "";
            filter = null;

            if (FileUtil.FileExist(strFilterFileName) == false)
            {
                strError = "文件 '" + strFilterFileName + "' 不存在";
                goto ERROR1;
            }

            string strWarning = "";

            string strLibPaths = "\"" + this.MainForm.DataDir + "\"";
            Type entryClassType = this.GetType();



            filter = new ColumnFilterDocument();
            filter.Host = new ColumnFilterHost();
            filter.Host.ColumnTable = this.ColumnTable;

            filter.strOtherDef = "dp2Circulation.ColumnFilterHost Host = null;";
            filter.strPreInitial = " ColumnFilterDocument doc = (ColumnFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = doc.Host;\r\n";
            /*
            filter.strOtherDef = entryClassType.FullName + " Host = null;";

            filter.strPreInitial = " ColumnFilterDocument doc = (ColumnFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = ("
                + entryClassType.FullName + ")doc.Host;\r\n";
             * */

            try
            {
                filter.Load(strFilterFileName);
            }
            catch (Exception ex)
            {
                strError = "文件 " + strFilterFileName + " 装载到MarcFilter时发生错误: " + ex.Message;
                goto ERROR1;
            }

            string strCode = "";    // c#代码
            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 一些必要的链接库
            string[] saAddRef1 = {
										 Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
										 //Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
										 //Environment.CurrentDirectory + "\\digitalplatform.library.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
										 // Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
										 Environment.CurrentDirectory + "\\dp2circulation.exe"
                };

            // fltx文件里显式增补的链接库
            string[] saAdditionalRef = filter.GetRefs();

            // 合并的链接库
            string[] saTotalFilterRef = new string[saAddRef1.Length + saAdditionalRef.Length];
            Array.Copy(saAddRef1, saTotalFilterRef, saAddRef1.Length);
            Array.Copy(saAdditionalRef, 0,
                saTotalFilterRef, saAddRef1.Length,
                saAdditionalRef.Length);

            Assembly assemblyFilter = null;

            // 创建Script的Assembly
            // 本函数内对saRef不再进行宏替换
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saTotalFilterRef,
                strLibPaths,
                out assemblyFilter,
                out strError,
                out strWarning);
            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                {
                    goto ERROR1;
                }
                MessageBox.Show(this, strWarning);
            }

            filter.Assembly = assemblyFilter;

            return 0;
        ERROR1:
            return -1;
        }

        // 用于缩进格式的tab字符串
        static string IndentString(int nLevel)
        {
            if (nLevel <= 0)
                return "";
            return new string('\t', nLevel);
        }

        // 关于来源的描述。
        // 如果为"batchno"方式，则为批次号；如果为"barcodefile"方式，则为条码号文件名(纯文件名); 如果为"recpathfile"方式，则为记录路径文件名(纯文件名)
        /// <summary>
        /// 关于来源的描述。
        /// 如果为"batchno"方式，则为批次号；如果为"barcodefile"方式，则为条码号文件名(纯文件名); 如果为"recpathfile"方式，则为记录路径文件名(纯文件名)
        /// </summary>
        public string SourceDescription
        {
            get
            {
                if (this.SourceStyle == "batchno")
                {
                    string strText = "";

                    if (String.IsNullOrEmpty(this.BatchNo) == false)
                        strText += "批次号 " + this.BatchNo;

                    if (String.IsNullOrEmpty(this.LocationString) == false
                        && this.LocationString != "<不指定>")
                    {
                        if (String.IsNullOrEmpty(strText) == false)
                            strText += "; ";
                        strText += "馆藏地 " + this.LocationString;
                    }

                    return this.BatchNo;
                }
                else if (this.SourceStyle == "barcodefile")
                {
                    return "条码号文件 " + Path.GetFileName(this.BarcodeFilePath);
                }
                else if (this.SourceStyle == "recpathfile")
                {
                    return "记录路径文件 " + Path.GetFileName(this.RecPathFilePath);
                }
                else
                {
                    Debug.Assert(this.SourceStyle == "", "");
                    return "";
                }
            }
        }

        private void button_print_print_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strWarning = "";
            int nRet = 0;

            if (this.listView_parent.Items.Count == 0)
            {
                strError = "目前没有可打印的内容";
                goto ERROR1;
            }

            // TODO: 是否要警告有TYPE_ERROR的事项，不参与打印?
            int nSkipCount = 0;

            this.ItemXmlTable.Clear();  // 防止缓冲的册信息在两次批处理之间保留

            Hashtable macro_table = new Hashtable();

            // 批次号或文件名 多个宏不方便判断后选用，只好合并为一个宏
            macro_table["%sourcedescription%"] = this.SourceDescription;

            macro_table["%libraryname%"] = this.MainForm.LibraryName;
            macro_table["%date%"] = DateTime.Now.ToLongDateString();
            macro_table["%datadir%"] = this.MainForm.DataDir;   // 便于引用datadir下templates目录内的某些文件
            ////macro_table["%libraryserverdir%"] = this.MainForm.LibraryServerDir;  // 便于引用服务器端的CSS文件

            // 获得打印参数
            string strPubType = "连续出版物";
            PrintBindingPrintOption option = new PrintBindingPrintOption(this.MainForm.DataDir,
                strPubType);
            option.LoadData(this.MainForm.AppInfo,
                "printbinding_printoption");

            /*
            macro_table["%pagecount%"] = nPageCount.ToString();
            macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
             * */

            string strMarcFilterFilePath = option.GetTemplatePageFilePath("MARC过滤器");
            if (String.IsNullOrEmpty(strMarcFilterFilePath) == false)
            {
                ColumnFilterDocument filter = null;

                this.ColumnTable = new Hashtable();
                nRet = PrepareMarcFilter(strMarcFilterFilePath,
                    out filter,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                //
                if (filter != null)
                    this.AssemblyFilter = filter.Assembly;
                else
                    this.AssemblyFilter = null;

                this.MarcFilter = filter;
            }

            List<string> filenames = new List<string>();

            try
            {
                EnableControls(false);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在构造HTML页面 ...");
                stop.BeginLoop();

                try
                {
                    stop.SetProgressRange(0, this.listView_parent.Items.Count);
                    stop.SetProgressValue(0);
                    for (int i = 0; i < this.listView_parent.Items.Count; i++)
                    {
                        ListViewItem item = this.listView_parent.Items[i];
                        if (item.ImageIndex == TYPE_ERROR)
                        {
                            nSkipCount++;
                            continue;
                        }

                        string strFilename = "";
                        string strOneWarning = "";
                        nRet = PrintOneBinding(
                                option,
                                macro_table,
                                item,
                                i,
                                out strFilename,
                                out strOneWarning,
                                out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        if (String.IsNullOrEmpty(strOneWarning) == false)
                        {
                            if (String.IsNullOrEmpty(strWarning) == false)
                                strWarning += "\r\n";
                            strWarning += strOneWarning;
                        }

                        filenames.Add(strFilename);

                        stop.SetProgressValue(i + 1);
                    }
                }
                finally
                {

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                    stop.HideProgress();

                    EnableControls(true);
                }

                if (nSkipCount > 0)
                {
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += "\r\n";
                    strWarning += "打印过程中，有 "+nSkipCount.ToString()+" 个错误状态的事项被跳过";
                }

                if (String.IsNullOrEmpty(strWarning) == false)
                {
                    // TODO: 如果警告文字的行数太多，需要截断，以便正常显示在MessageBox()中。但是进入文件的内容没有必要截断
                    MessageBox.Show(this, "警告:\r\n" + strWarning);
                    string strErrorFilename = this.MainForm.DataDir + "\\~printbinding_" + "warning.txt";
                    StreamUtil.WriteText(strErrorFilename, "警告:\r\n" + strWarning);
                    filenames.Insert(0, strErrorFilename);
                }

                HtmlPrintForm printform = new HtmlPrintForm();

                printform.Text = "打印装订单";
                printform.MainForm = this.MainForm;
                printform.Filenames = filenames;
                this.MainForm.AppInfo.LinkFormState(printform, "printbinding_htmlprint_formstate");
                printform.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(printform);

            }
            finally
            {
                if (filenames != null)
                {
                    Global.DeleteFiles(filenames);
                    filenames.Clear();
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 打印一个合订册的装订单
        int PrintOneBinding(
            PrintBindingPrintOption option,
            Hashtable macro_table,
            ListViewItem item,
            int nIndex,
            out string strFilename,
            out string strWarning,
            out string strError)
        {
            strWarning = "";
            strError = "";
            strFilename = "";

            int nRet = 0;

            macro_table["%bindingissn%"] = ListViewUtil.GetItemText(item, COLUMN_ISBNISSN);
            macro_table["%bindingsummary%"] = ListViewUtil.GetItemText(item, COLUMN_SUMMARY);
            macro_table["%bindingaccessno%"] = ListViewUtil.GetItemText(item, COLUMN_ACCESSNO);

            // 2012/5/29
            string strBidningBarcode = ListViewUtil.GetItemText(item, COLUMN_BARCODE);
            string strBindingBarcodeStyle = "";
            if (this.checkBox_print_barcodeFix.Checked == true)
            {
                if (string.IsNullOrEmpty(strBidningBarcode) == false)
                    strBidningBarcode = "*" + strBidningBarcode + "*";

                // strStyle = " style=\"font-family: C39HrP24DhTt; \"";
            }
            macro_table["%bindingbarcode%"] = strBidningBarcode;

            macro_table["%bindingintact%"] = ListViewUtil.GetItemText(item, COLUMN_INTACT);
            macro_table["%bindingprice%"] = ListViewUtil.GetItemText(item, COLUMN_PRICE);
            macro_table["%bindingvolume%"] = ListViewUtil.GetItemText(item, COLUMN_VOLUME);
            macro_table["%bindingpublishtime%"] = ListViewUtil.GetItemText(item, COLUMN_PUBLISHTIME);
            macro_table["%bindinglocation%"] = ListViewUtil.GetItemText(item, COLUMN_LOCATION);
            macro_table["%bindingrefid%"] = ListViewUtil.GetItemText(item, COLUMN_REFID);

            // string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);
            // TODO: 获得书目记录，创建某种格式，并获得某些宏内容？

            if (this.MarcFilter != null)
            {
                string strMARC = "";
                string strOutMarcSyntax = "";

                // TODO: 有错误要明显报出来，否则容易在打印出来后才发现，就晚了

                // 获得MARC格式书目记录
                nRet = GetMarc(item,
                    out strMARC,
                    out strOutMarcSyntax,
                    out strError);
                if (nRet == -1)
                    return -1;

                /*
                // 清除上次曾经加入到macro_table中的内容
                foreach (string key in this.ColumnTable.Keys)
                {
                    macro_table.Remove("%" + key + "%");
                }
                 * */

                this.ColumnTable.Clear();   // 清除上一记录处理时残余的内容

                // 让脚本能够感知到准备的宏
                foreach (string key in macro_table.Keys)
                {
                    this.ColumnTable.Add(key.Replace("%", ""), macro_table[key]);
                }

                // 触发filter中的Record相关动作
                nRet = this.MarcFilter.DoRecord(
                    null,
                    strMARC,
                    strOutMarcSyntax,
                    nIndex,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 追加到macro_table中
                foreach(string key in this.ColumnTable.Keys)
                {
                    macro_table.Remove("%" + key + "%");

                    macro_table.Add("%" + key + "%", this.ColumnTable[key]);
                }
            }

            // 需要将属于合订册的文件名前缀区别开来
            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
            string strFileNamePrefix = this.MainForm.DataDir + "\\~printbinding_" + strRecPath.Replace("/", "_") + "_";

            strFilename = strFileNamePrefix + "0" + ".html";

            // 提前创建成员表格的内容，因为里面也要顺便创建若干和统计数据有关的宏值
            // 内容暂时不输出
            string strMemberTableResult = "";
            // 构造下属成员的详情表格
            // return:
            //      实际到达的成员册数
            nRet = BuildMembersTable(
                option,
                macro_table,
                item,
                out strMemberTableResult,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
            {
                strWarning = "合订册 (记录路径="+strRecPath+"; 参考ID="
                    +ListViewUtil.GetItemText(item, COLUMN_REFID)+") 中没有包含任何(实到的)成员册";
            }

            BuildPageTop(option,
                macro_table,
                strFilename);

            // 输出信函内容
            {

                /*
                // 期刊种数
                macro_table["%seriescount%"] = seller.Count.ToString();
                // 相关的期数
                macro_table["%issuecount%"] = GetIssueCount(seller).ToString();
                // 缺的册数
                macro_table["%missingitemcount%"] = GetMissingItemCount(seller).ToString();
                */


                string strTemplateFilePath = option.GetTemplatePageFilePath("合订册正文");
                if (String.IsNullOrEmpty(strTemplateFilePath) == false)
                {
                    /*
                     * 期刊名 ISSN 
                     * 索取号
                     * 合订册的册条码号
                     * 合订册的完好率
                     * 合订册的价格
                     * 包含的期范围
                     * 缺期范围
                     * */


                    /*
<div class='binding_table_title'>合订册</div>
<table class='binding'>
<tr class='biblio'>
	<td class='name'>期刊</td>
	<td class='value'>%bindingsummary%</td>
</tr>
<tr class='issn'>
	<td class='name'>ISSN</td>
	<td class='value'>%bindingissn%</td>
</tr>
<tr class='accessno'>
	<td class='name'>索取号</td>
	<td class='value'>%bindingaccessno%</td>
</tr>
<tr class='location'>
	<td class='name'>馆藏地点</td>
	<td class='value'>%bindinglocation%</td>
</tr>
<tr class='barcode'>
	<td class='name'>册条码号</td>
	<td class='value'>%bindingbarcode%</td>
</tr>
<tr class='refid'>
	<td class='name'>参考ID</td>
	<td class='value'>%bindingrefid%</td>
</tr>
<tr class='intact'>
	<td class='name'>完好率</td>
	<td class='value'>%bindingintact%</td>
</tr>
<tr class='bindingprice'>
	<td class='name'>合订价格</td>
	<td class='value'>%bindingprice%</td>
</tr>
<tr class='publishtime'>
	<td class='name'>出版时间</td>
	<td class='value'>%bindingpublishtime%</td>
</tr>
<tr class='bindingissuecount'>
	<td class='name'>期数</td>
	<td class='value'>实含数: %arrivecount%; 缺期数: %missingcount%; 理论数: %issuecount%; 缺期号: %missingvolume%</td>
</tr>
<tr class='volume'>
	<td class='name'>包含期号</td>
	<td class='value'>%bindingvolume%</td>
</tr>
</table>
                     * */

                    // 根据模板打印
                    string strContent = "";
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

                    string strResult = StringUtil.MacroString(macro_table,
                        strContent);
                    StreamUtil.WriteText(strFilename,
                        strResult);
                }
                else
                {
                    // 缺省的固定内容打印

                    string strTableTitle = option.TableTitle;

                    if (String.IsNullOrEmpty(strTableTitle) == false)
                    {
                        strTableTitle = StringUtil.MacroString(macro_table,
                            strTableTitle);
                    }

                    // 表格开始
                    StreamUtil.WriteText(strFilename,
                        "<div class='binding_table_title'>" + HttpUtility.HtmlEncode(strTableTitle) + "</div>");


                    // 表格开始
                    StreamUtil.WriteText(strFilename,
                        "<table class='binding'>");

                    // 期刊信息
                    StreamUtil.WriteText(strFilename,
                        "<tr class='biblio'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>期刊</td>");
                    StreamUtil.WriteText(strFilename,
                      "<td class='value'>"+HttpUtility.HtmlEncode(
                      (string)macro_table["%bindingsummary%"]) + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");

                    // ISSN
                    StreamUtil.WriteText(strFilename,
                        "<tr class='issn'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>ISSN</td>");
                    StreamUtil.WriteText(strFilename,
                      "<td class='value'>" + HttpUtility.HtmlEncode(
                      (string)macro_table["%bindingissn%"]) + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");


                    // 索取号
                    StreamUtil.WriteText(strFilename,
                        "<tr class='accessno'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>索取号</td>");
                    StreamUtil.WriteText(strFilename,
                      "<td class='value'>" + HttpUtility.HtmlEncode(
                      (string)macro_table["%bindingaccessno%"]) + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");

                    // 馆藏地点
                    StreamUtil.WriteText(strFilename,
                        "<tr class='location'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>馆藏地点</td>");
                    StreamUtil.WriteText(strFilename,
                      "<td class='value'>" + HttpUtility.HtmlEncode(
                      (string)macro_table["%bindinglocation%"]) + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");

                    // 条码号
                    StreamUtil.WriteText(strFilename,
                        "<tr class='barcode'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>册条码号</td>");

                    StreamUtil.WriteText(strFilename,
                      "<td class='value' " + strBindingBarcodeStyle + " >" + HttpUtility.HtmlEncode((string)macro_table["%bindingbarcode%"])
                        + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");

                    // 参考ID
                    StreamUtil.WriteText(strFilename,
                        "<tr class='refid'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>参考ID</td>");
                    StreamUtil.WriteText(strFilename,
                      "<td class='value'>" + HttpUtility.HtmlEncode(
                      (string)macro_table["%bindingrefid%"]) + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");

                    // 完好率
                    StreamUtil.WriteText(strFilename,
                        "<tr class='intact'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>完好率</td>");
                    StreamUtil.WriteText(strFilename,
                      "<td class='value'>" + HttpUtility.HtmlEncode(
                      (string)macro_table["%bindingintact%"]) + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");

                    // 价格
                    StreamUtil.WriteText(strFilename,
                        "<tr class='bindingprice'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>合订价格</td>");
                    StreamUtil.WriteText(strFilename,
                      "<td class='value'>" + HttpUtility.HtmlEncode(
                      (string)macro_table["%bindingprice%"]) + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");

                    // 出版时间
                    StreamUtil.WriteText(strFilename,
                        "<tr class='publishtime'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>出版时间</td>");
                    StreamUtil.WriteText(strFilename,
                      "<td class='value'>" + HttpUtility.HtmlEncode(
                      (string)macro_table["%bindingpublishtime%"]) + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");

                    // 期数
                    string strValue = "实含数: " + (string)macro_table["%arrivecount%"] + "; "
                    + "缺期数: " + (string)macro_table["%missingcount%"] + "; "
                    + "理论数: " + (string)macro_table["%issuecount%"];
                    string strMissingVolume = (string)macro_table["%missingvolume%"];
                    if (String.IsNullOrEmpty(strMissingVolume) == false)
                        strValue += "; 缺期号: " + strMissingVolume;
                    StreamUtil.WriteText(strFilename,
                        "<tr class='bindingissuecount'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>期数</td>");
                    StreamUtil.WriteText(strFilename,
                      "<td class='value'>" + HttpUtility.HtmlEncode(strValue) + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");


                    // 包含的期
                    StreamUtil.WriteText(strFilename,
                        "<tr class='volume'>");
                    StreamUtil.WriteText(strFilename,
                       "<td class='name'>包含期号</td>");
                    StreamUtil.WriteText(strFilename,
                      "<td class='value'>" + HttpUtility.HtmlEncode(
                      (string)macro_table["%bindingvolume%"]) + "</td>");
                    StreamUtil.WriteText(strFilename,
                        "</tr>");

                    // 表格结束
                    StreamUtil.WriteText(strFilename,
                        "</table>");
                }

            }

            // 这时候才输出成员表格内容
            StreamUtil.WriteText(strFilename,
                strMemberTableResult);


            BuildPageBottom(option,
                macro_table,
                strFilename);

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
                // return this.MainForm.LibraryServerDir + "/" + strDefaultCssFileName;    // 缺省的
                return PathUtil.MergePath(this.MainForm.DataDir, strDefaultCssFileName);    // 缺省的
            }
        }

        int BuildPageTop(PrintOption option,
    Hashtable macro_table,
    string strFileName)
        {
            string strCssUrl = GetAutoCssUrl(option, "printbinding.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            StreamUtil.WriteText(strFileName,
                "<!DOCTYPE html PUBLIC \" -//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">"
                + "<html><head>" + strLink + "</head><body>");

            // 页眉
            string strPageHeaderText = option.PageHeader;

            if (String.IsNullOrEmpty(strPageHeaderText) == false)
            {
                strPageHeaderText = StringUtil.MacroString(macro_table,
                    strPageHeaderText);

                StreamUtil.WriteText(strFileName,
                    "<div class='pageheader'>" + strPageHeaderText + "</div>");
            }

            /*
            // 书商名称
            StreamUtil.WriteText(strFileName,
    "<div class='seller'>" + GetPureSellerName(seller.Seller) + "</div>");
             * */

            return 0;
        }


        int BuildPageBottom(PrintOption option,
            Hashtable macro_table,
            string strFileName)
        {

            // 页脚
            string strPageFooterText = option.PageFooter;

            if (String.IsNullOrEmpty(strPageFooterText) == false)
            {
                strPageFooterText = StringUtil.MacroString(macro_table,
                    strPageFooterText);

                StreamUtil.WriteText(strFileName,
        "<div class='pagefooter'>" + strPageFooterText + "</div>");
            }

            StreamUtil.WriteText(strFileName, "</body></html>");

            return 0;
        }

        // 解析当年期号、总期号、卷号的字符串
        // 注意这是一个特殊版本，能够识别里面的"y."
        /*public*/ static void ParseItemVolumeString(string strVolumeString,
            out string strYear,
            out string strIssue,
            out string strZong,
            out string strVolume)
        {
            strYear = "";
            strIssue = "";
            strZong = "";
            strVolume = "";

            string[] segments = strVolumeString.Split(new char[] { ';',',','=' });
            for (int i = 0; i < segments.Length; i++)
            {
                string strSegment = segments[i].Trim();

                if (StringUtil.HasHead(strSegment, "y.") == true)
                    strYear = strSegment.Substring(2).Trim();
                else if (StringUtil.HasHead(strSegment, "no.") == true)
                    strIssue = strSegment.Substring(3).Trim();
                else if (StringUtil.HasHead(strSegment, "总.") == true)
                    strZong = strSegment.Substring(2).Trim();
                else if (StringUtil.HasHead(strSegment, "v.") == true)
                    strVolume = strSegment.Substring(2).Trim();
            }
        }

        // 创建表示范围的 期号，卷号，总期号字符串
        // 各个序列间用等号连接
        /*public*/ static string BuildVolumeRangeString(List<string> volumes)
        {
            Hashtable no_list_table = new Hashtable();
            List<string> volume_list = new List<string>();
            List<string> zong_list = new List<string>();

            for(int i=0;i<volumes.Count;i++)
            {
                // 解析单册的volumestring
                string strYear = "";
                string strNo = "";
                string strZong = "";
                string strSingleVolume = "";

                ParseItemVolumeString(volumes[i],
                    out strYear,
                    out strNo,
                    out strZong,
                    out strSingleVolume);

                List<string> no_list = (List<string>)no_list_table[strYear];
                if (no_list == null)
                {
                    no_list = new List<string>();
                    no_list_table[strYear] = no_list;
                }

                no_list.Add(strNo);
                volume_list.Add(strSingleVolume);
                zong_list.Add(strZong);
            }


            List<string> keys = new List<string>();
            foreach (string key in no_list_table.Keys)
            {
                keys.Add(key);
            }
            keys.Sort();

            string strNoString = "";
            for (int i = 0; i < keys.Count; i++)
            {
                string strYear = keys[i];
                List<string> no_list = (List<string>)no_list_table[strYear];
                Debug.Assert(no_list != null);

                if (String.IsNullOrEmpty(strNoString) == false)
                    strNoString += ";";
                strNoString += (String.IsNullOrEmpty(strYear) == false ? strYear + ":" : "")
                    + "no." + Global.BuildNumberRangeString(no_list);
            }

            string strVolumeString = Global.BuildNumberRangeString(volume_list);
            string strZongString = Global.BuildNumberRangeString(zong_list);

            string strValue = strNoString;

            if (String.IsNullOrEmpty(strZongString) == false)
            {
                if (String.IsNullOrEmpty(strValue) == false)
                    strValue += "=";
                strValue += "总." + strZongString;
            }

            if (String.IsNullOrEmpty(strVolumeString) == false)
            {
                if (String.IsNullOrEmpty(strValue) == false)
                    strValue += "=";
                strValue += "v." + strVolumeString;
            }

            return strValue;
        }

        // 构造下属成员的详情表格
        // return:
        //      实际到达的成员册数
        int BuildMembersTable(
            // string strFilename,
            PrintOption option,
            Hashtable macro_table,
            ListViewItem parent_item,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";


            OriginItemData data = (OriginItemData)parent_item.Tag;
            Debug.Assert(data != null, "");

            if (String.IsNullOrEmpty(data.Xml) == true)
            {
                strError = "data.Xml为空";
                return -1;
            }

            // 将item record xml装入DOM，然后select出每个<item>元素
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(data.Xml);
            }
            catch (Exception ex)
            {
                string strRecPath = ListViewUtil.GetItemText(parent_item, COLUMN_RECPATH);
                strError = "路径为 '" + strRecPath + "' 的册记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("binding/item");
            if (nodes.Count == 0)
                return 0;

            int nArriveCount = 0;   // 到达(并参与了装订的)的册数
            int nMissingCount = 0;  // 缺期的册数
            int nIssueCount = nodes.Count;   // 理论上应含多少册

            // 表格开始
            strResult +=
                "<div class='members_table_title'>所含单册</div>";


            // 表格开始
            strResult +=
                "<table class='members'>";

            // 栏目标题
            strResult +=
                "<tr class='column'>";

            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                string strCaption = column.Caption;

                // 如果没有caption定义，就挪用name定义
                if (String.IsNullOrEmpty(strCaption) == true)
                    strCaption = column.Name;

                string strClass = StringUtil.GetLeft(column.Name);

            strResult +=
                    "<td class='" + strClass + "'>" + strCaption + "</td>";
            }

            strResult += "</tr>";

            List<string> missing_volumes = new List<string>();
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                Hashtable value_table = new Hashtable();

                bool bMissing = false;
                // 获得布尔型的属性参数值
                // return:
                //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                DomUtil.GetBooleanParam(node,
                    "missing",
                    false,
                    out bMissing,
                    out strError);
                /*
                if (bMissing == true)
                    continue;
                 * */

                if (bMissing == true)
                    nMissingCount++;
                else
                    nArriveCount++;

                string strRefID = DomUtil.GetAttr(node, "refID");
                string strVolumeString = DomUtil.GetAttr(node, "volume");
                string strPublishTime = DomUtil.GetAttr(node, "publishTime");
                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strRegisterNo = DomUtil.GetAttr(node, "registerNo");

                if (bMissing == true)
                {
                    // 年
                    string strYear = dp2StringUtil.GetYearPart(strPublishTime);
                    missing_volumes.Add("y." + strYear + "," + strVolumeString);
                }

                value_table["%missing%"] = bMissing == true ? "缺" : "";
                value_table["%refID%"] = strRefID;
                value_table["%volume%"] = strVolumeString;
                value_table["%publishTime%"] = BindingControl.GetDisplayPublishTime(strPublishTime);
                if (String.IsNullOrEmpty(strBarcode) == false)
                    value_table["%barcode%"] = strBarcode;
                if (String.IsNullOrEmpty(strRegisterNo) == false)
                    value_table["%registerNo%"] = strRegisterNo;

                if (bMissing == true)
                    strResult += "<tr class='content missing'>";
                else
                    strResult += "<tr class='content'>";

                for (int j = 0; j < option.Columns.Count; j++)
                {
                    Column column = option.Columns[j];

                    List<Hashtable> value_tables = new List<Hashtable>();
                    value_tables.Add(value_table);
                    value_tables.Add(macro_table);
                    value_tables.Add(this.ColumnTable);

                    string strContent = GetColumnContent(value_tables,
                        strRefID,
                        StringUtil.GetLeft(column.Name));

                    string strClass = StringUtil.GetLeft(column.Name);
            strResult +=
                        "<td class='" + strClass + "'>" + strContent + "</td>";

                }

                strResult += "</tr>";
            }


            // 表格结束
            strResult +=
                "</table>";

            macro_table["%arrivecount%"] = nArriveCount.ToString();
            macro_table["%missingcount%"] = nMissingCount.ToString();
            macro_table["%issuecount%"] = nIssueCount.ToString();
            macro_table["%missingvolume%"] = BuildVolumeRangeString(missing_volumes);

            return nArriveCount;
        }

        /*public*/ int GetItemXmlByRefID(
            string strRefID,
            out string strItemXml,
            out string strError)
        {
            strError = "";
            strItemXml = "";

            string strBiblioText = "";
            string strItemRecPath = "";
            string strBiblioRecPath = "";
            byte[] item_timestamp = null;
            string strBarcode = "@refID:" + strRefID;

            if (this.stop != null)
                this.stop.SetMessage("正在获取参考ID为 '"+strRefID+"' 的实体记录... ");

            long lRet = Channel.GetItemInfo(
                stop,
                strBarcode,
                "xml",
                out strItemXml,
                out strItemRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strBiblioRecPath,
                out strError);

            if (this.stop != null)
                this.stop.SetMessage("");

            return (int)lRet;
        }

        // 获得栏目内容
        // paramters:
        //      strRefID    成员册的参考ID
        string GetColumnContent(List<Hashtable> value_tables,
            string strRefID,
            string strColumnName)
        {
            string strName = StringUtil.GetLeft(strColumnName);

            for (int i = 0; i < value_tables.Count; i++)
            {
                Hashtable value_table = value_tables[i];
                if (value_table.ContainsKey(strName) == true)
                    return (string)value_table[strName];

                if (strName.Length > 0 && strName[0] != '%')
                {
                    string strTemp = "%" + strName + "%";
                    if (value_table.ContainsKey(strTemp) == true)
                        return (string)value_table[strTemp];
                }
            }

            // 
            if (String.IsNullOrEmpty(strRefID) == true)
                return "";

            // TODO: 先检查strName是否在实体记录定义的字段名之列

            int nRet = 0;
            string strError = "";


            string strItemXml = "";
            XmlDocument dom = (XmlDocument)this.ItemXmlTable[strRefID];
            if (dom == null)
            {
                if (this.ItemXmlTable.ContainsKey(strRefID) == true)
                    return "";  // 缓存中已经存在这个条目，因为以前找过但是没有找到或者出错

                nRet = GetItemXmlByRefID(
                    strRefID,
                    out strItemXml,
                    out strError);
                if (nRet == 0 || nRet == -1)
                {
                    this.ItemXmlTable[strRefID] = null;
                    if (nRet == -1)
                        return "error: " + strError;
                    Debug.Assert(nRet == 0, "");
                    return "";  // 没有找到实体记录
                }
                else
                {
                    dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strItemXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "ItemXml装入DOM时出错: " + ex.Message;
                        return strError;
                    }

                    // 防止缓冲区过大
                    if (this.ItemXmlTable.Count > 100)
                        this.ItemXmlTable.Clear();

                    this.ItemXmlTable[strRefID] = dom;
                }
            }

            Debug.Assert(dom != null, "");
            return DomUtil.GetElementText(dom.DocumentElement,
                strName);
        }


        // 获得MARC格式书目记录
        int GetMarc(ListViewItem item,
            out string strMARC,
            out string strOutMarcSyntax,
            out string strError)
        {
            strError = "";
            strMARC = "";
            strOutMarcSyntax = "";
            int nRet = 0;

            string[] formats = new string[1];
            formats[0] = "xml";

            string[] results = null;
            byte[] timestamp = null;

            string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

            Debug.Assert(String.IsNullOrEmpty(strBiblioRecPath) == false, "strBiblioRecPath值不能为空");

            long lRet = Channel.GetBiblioInfos(
                    null, // stop,
                    strBiblioRecPath,
                    "",
                    formats,
                    out results,
                    out timestamp,
                    out strError);
            if (lRet == -1 || lRet == 0)
            {
                if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                    strError = "书目记录 '" + strBiblioRecPath + "' 不存在";

                strError = "获得书目记录时发生错误: " + strError;
                return -1;
            }

            string strXml = results[0];

            // 转换为MARC格式
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            nRet = MarcUtil.Xml2Marc(strXml,
                true,   // 2013/1/12 修改为true
                "", // strMarcSyntax
                out strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        private void button_print_option_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            string strNamePath = "printbinding_printoption";
            string strPubType = "连续出版物";

            PrintBindingPrintOption option = new PrintBindingPrintOption(this.MainForm.DataDir,
                strPubType);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);


            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            dlg.DataDir = this.MainForm.DataDir;
            dlg.Text = strPubType + " 装订单 打印参数";
            dlg.PrintOption = option;
            dlg.ColumnItems = new string[] {
                "missing -- 缺期状态",
                "publishTime -- 出版日期",
                "volume -- 卷期号",
                "barcode -- 册条码号",
                "intact -- 完好率",
                "refID -- 参考ID",
            };


            this.MainForm.AppInfo.LinkFormState(dlg, "printbinding_printoption_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(this.MainForm.AppInfo,
                strNamePath);
        }

        /// <summary>
        /// 错误信息窗
        /// </summary>
        public HtmlViewerForm ErrorInfoForm = null;

        // 获得错误信息窗
        HtmlViewerForm GetErrorInfoForm()
        {
            if (this.ErrorInfoForm == null
                || this.ErrorInfoForm.IsDisposed == true
                || this.ErrorInfoForm.IsHandleCreated == false)
            {
                this.ErrorInfoForm = new HtmlViewerForm();
                this.ErrorInfoForm.ShowInTaskbar = false;
                this.ErrorInfoForm.Text = "错误信息";
                this.ErrorInfoForm.Show(this);
                this.ErrorInfoForm.WriteHtml("<pre>");  // 准备文本输出
            }

            return this.ErrorInfoForm;
        }

        void ClearErrorInfoForm()
        {
            // 清除错误信息窗口中残余的内容
            if (this.ErrorInfoForm != null)
            {
                try
                {
                    this.ErrorInfoForm.HtmlString = "<pre>";
                }
                catch
                {
                }
            }
        }

        // 检查路径所从属书目库是否为图书/期刊库？
        // return:
        //      -1  error
        //      0   不符合要求。提示信息在strError中
        //      1   符合要求
        int CheckItemRecPath(string strLoadType,
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

            if (strLoadType == "图书")
            {
                if (String.IsNullOrEmpty(strIssueDbName) == false)
                {
                    strError = "路径 '" + strItemRecPath + "' 所从属的书目库 '" + strBiblioDbName + "' 为期刊型，和当前出版物类型 '" + strLoadType + "' 不一致";
                    return 0;
                }
                return 1;
            }

            if (strLoadType == "连续出版物")
            {
                if (String.IsNullOrEmpty(strIssueDbName) == true)
                {
                    strError = "路径 '" + strItemRecPath + "' 所从属的书目库 '" + strBiblioDbName + "' 为图书型，和当前出版物类型 '" + strLoadType + "' 不一致";
                    return 0;
                }
                return 1;
            }

            strError = "CheckItemRecPath() 未知的出版物类型 '" + strLoadType + "'";
            return -1;
        }

        private void button_load_loadFromRecPathFile_Click(object sender, EventArgs e)
        {
            int nRet = 0;

            this.ClearErrorInfoForm();

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的记录路径文件名";
            dlg.FileName = this.RecPathFilePath;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.SourceStyle = "recpathfile";

            int nDupCount = 0;

            string strError = "";
            // StreamReader sr = null;
            try
            {
                // 打开文件
                // sr = new StreamReader(dlg.FileName);

                EnableControls(false);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在初始化浏览器组件 ...");
                stop.BeginLoop();
                this.Update();
                this.MainForm.Update();

                try
                {
                    if (Control.ModifierKeys == Keys.Control)
                    {
                    }
                    else
                    {
                        this.listView_parent.Items.Clear();
                        this.SortColumns_parent.Clear();
                        SortColumns.ClearColumnSortDisplay(this.listView_parent.Columns);

                        /*
                        this.refid_table.Clear();
                        this.orderxml_table.Clear();
                         * */
                    }

                    using (StreamReader sr = new StreamReader(dlg.FileName))
                    {
                        // this.m_nGreenItemCount = 0;

                        // 逐行读入文件内容
                        // 测算文件行数
                        int nLineCount = 0;
                        for (; ; )
                        {
                            if (stop != null)
                            {
                                if (stop.State != 0)
                                {
                                    strError = "用户中断1";
                                    goto ERROR1;
                                }
                            }


                            string strLine = "";
                            strLine = sr.ReadLine();

                            if (strLine == null)
                                break;

                            nLineCount++;
                        }

                        // 设置进度范围
                        stop.SetProgressRange(0, nLineCount);
                    }

                    using (StreamReader sr = new StreamReader(dlg.FileName))
                    {
                        for (int i = 0; ; i++)
                        {
                            if (stop != null)
                            {
                                if (stop.State != 0)
                                {
                                    strError = "用户中断1";
                                    goto ERROR1;
                                }
                            }

                            string strLine = "";
                            strLine = sr.ReadLine();

                            stop.SetProgressValue(i);

                            if (strLine == null)
                                break;

                            strLine = strLine.Trim();
                            if (String.IsNullOrEmpty(strLine) == true)
                                continue;

                            if (strLine[0] == '#')
                                continue;   // 注释行

                            // 检查路径所从属书目库是否为图书/期刊库？
                            // return:
                            //      -1  error
                            //      0   不符合要求。提示信息在strError中
                            //      1   符合要求
                            nRet = CheckItemRecPath("连续出版物",
                                strLine,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            if (nRet == 0)
                            {
                                GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            }

                            stop.SetMessage("正在装入路径 " + strLine + " 对应的记录...");


                            string strOutputItemRecPath = "";
                            // 根据册条码号，装入册记录
                            // return: 
                            //      -2  册条码号已经在list中存在了
                            //      -1  出错
                            //      1   成功
                            nRet = LoadOneItem(
                                "@path:" + strLine,
                                this.listView_parent,
                                null,
                                out strOutputItemRecPath,
                                out strError);
                            if (nRet == -2)
                                nDupCount++;
                        }
                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("装入完成。");
                    stop.HideProgress();

                    EnableControls(true);
                }
            }
            catch (Exception ex)
            {
                strError = "PrintBindingForm {A56F4878-8E46-4770-9C68-0D303AE48B43} exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }
            finally
            {
                // sr.Close();
            }

            // 记忆文件名
            this.RecPathFilePath = dlg.FileName;
            // this.Text = "打印装订单 " + Path.GetFileName(this.RecPathFilePath);
            this.Text = "打印装订单 -- " + this.SourceDescription;

            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " + nDupCount.ToString() + "个重复事项被忽略。");
            }

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
            this.Text = "打印装订单";
            MessageBox.Show(this, strError);
        }

        private void button_load_loadFromBarcodeFile_Click(object sender, EventArgs e)
        {
            int nRet = 0;

            this.ClearErrorInfoForm();

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

            string strError = "";
            //StreamReader sr = null;
            try
            {
                // 打开文件
                // sr = new StreamReader(dlg.FileName);


                EnableControls(false);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在初始化浏览器组件 ...");
                stop.BeginLoop();
                this.Update();
                this.MainForm.Update();

                try
                {
                    if (Control.ModifierKeys == Keys.Control)
                    {
                    }
                    else
                    {
                        this.listView_parent.Items.Clear();
                        this.SortColumns_parent.Clear();
                        SortColumns.ClearColumnSortDisplay(this.listView_parent.Columns);

                        /*
                        this.refid_table.Clear();
                        this.orderxml_table.Clear();
                         * */
                    }

                    using (StreamReader sr = new StreamReader(dlg.FileName))
                    {
                        // 逐行读入文件内容
                        // 测算文件行数
                        int nLineCount = 0;
                        for (; ; )
                        {
                            if (stop != null)
                            {
                                if (stop.State != 0)
                                {
                                    strError = "用户中断1";
                                    goto ERROR1;
                                }
                            }


                            string strLine = "";
                            strLine = sr.ReadLine();

                            if (strLine == null)
                                break;

                            nLineCount++;
                        }

                        // 设置进度范围
                        stop.SetProgressRange(0, nLineCount);

                    }

                    using (StreamReader sr = new StreamReader(dlg.FileName))
                    {

                        for (int i = 0; ; i++)
                        {
                            if (stop != null)
                            {
                                if (stop.State != 0)
                                {
                                    strError = "用户中断1";
                                    goto ERROR1;
                                }
                            }

                            string strLine = "";
                            strLine = sr.ReadLine();

                            stop.SetProgressValue(i);

                            if (strLine == null)
                                break;

                            strLine = strLine.Trim();
                            if (String.IsNullOrEmpty(strLine) == true)
                                continue;

                            if (strLine[0] == '#')
                                continue;   // 注释行

                            stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");


                            string strOutputItemRecPath = "";
                            // 根据册条码号，装入册记录
                            // return: 
                            //      -2  册条码号已经在list中存在了
                            //      -1  出错
                            //      1   成功
                            nRet = LoadOneItem(
                                strLine,
                                this.listView_parent,
                                null,
                                out strOutputItemRecPath,
                                out strError);
                            if (nRet == -2)
                                nDupCount++;

                            // 检查路径所从属书目库是否为图书/期刊库？
                            // return:
                            //      -1  error
                            //      0   不符合要求。提示信息在strError中
                            //      1   符合要求
                            nRet = CheckItemRecPath("连续出版物",
                                strOutputItemRecPath,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            if (nRet == 0)
                            {
                                GetErrorInfoForm().WriteHtml("册条码号为 " + strLine + " 的册记录 " + strError + "\r\n");
                            }
                        }
                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("装入完成。");
                    stop.HideProgress();

                    EnableControls(true);
                }
            }
            catch (Exception ex)
            {
                strError = "PrintBindingForm {5D761CDB-EBB1-448D-956E-2F864AA3FED6} exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }
            finally
            {
                // sr.Close();
            }

            // 记忆文件名
            this.BarcodeFilePath = dlg.FileName;
            // this.Text = "打印装订单 " + Path.GetFileName(this.BarcodeFilePath);
            this.Text = "打印装订单 -- " + this.SourceDescription;

            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " + nDupCount.ToString() + "个重复条码事项被忽略。");
            }

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
            this.Text = "打印装订单";
            MessageBox.Show(this, strError);
        }

        void FillMemberListViewItems(ListViewItem parent_item)
        {
            string strError = "";

            this.listView_member.Items.Clear();

            OriginItemData data = (OriginItemData)parent_item.Tag;

            // 将item record xml装入DOM，然后select出每个<item>元素
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(data.Xml);
            }
            catch (Exception ex)
            {
                string strRecPath = ListViewUtil.GetItemText(parent_item, COLUMN_RECPATH);
                strError = "路径为 '" + strRecPath + "' 的册记录XML装入DOM时出错: " + ex.Message;
                ListViewItem item = new ListViewItem();
                item.Text = strError;
                item.ImageIndex = TYPE_ERROR;
                this.listView_member.Items.Add(item);
                return;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("binding/item");
            if (nodes.Count == 0)
                return;

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                bool bMissing = false;
                // 获得布尔型的属性参数值
                // return:
                //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                DomUtil.GetBooleanParam(node,
                    "missing",
                    false,
                    out bMissing,
                    out strError);
                string strRefID = DomUtil.GetAttr(node, "refID");
                string strVolumeString = DomUtil.GetAttr(node, "volume");
                string strPublishTime = DomUtil.GetAttr(node, "publishTime");
                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strRegisterNo = DomUtil.GetAttr(node, "registerNo");

                ListViewItem item = new ListViewItem();
                item.ImageIndex = TYPE_NORMAL;
                this.listView_member.Items.Add(item);

                if (bMissing == true)
                    ListViewUtil.ChangeItemText(item, COLUMN_STATE, "缺");

                ListViewUtil.ChangeItemText(item, COLUMN_REFID, strRefID);
                ListViewUtil.ChangeItemText(item, COLUMN_VOLUME, strVolumeString);
                ListViewUtil.ChangeItemText(item, COLUMN_PUBLISHTIME, strPublishTime);
                ListViewUtil.ChangeItemText(item, COLUMN_BARCODE, strBarcode);
                ListViewUtil.ChangeItemText(item, COLUMN_REGISTERNO, strRegisterNo);
            }

        }

        private void listView_parent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_parent.SelectedItems.Count == 1)
            {
                FillMemberListViewItems(this.listView_parent.SelectedItems[0]);
            }
            else
            {
                this.listView_member.Items.Clear();
            }
        }

        private void listView_parent_DoubleClick(object sender, EventArgs e)
        {
            LoadToEntityForm(this.listView_parent);

        }

        private void listView_member_DoubleClick(object sender, EventArgs e)
        {
            LoadToEntityForm(this.listView_member);
        }

        void LoadToEntityForm(ListView list)
        {
            if (list.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装载的事项");
                return;
            }

            string strBarcode = list.SelectedItems[0].Text;
            string strRecPath = ListViewUtil.GetItemText(list.SelectedItems[0], COLUMN_RECPATH);
            string strRefID = ListViewUtil.GetItemText(list.SelectedItems[0], COLUMN_REFID);

            EntityForm form = new EntityForm();

            form.MainForm = this.MainForm;
            form.MdiParent = this.MainForm;
            form.Show();

            if (String.IsNullOrEmpty(strBarcode) == false)
                form.LoadItemByBarcode(strBarcode, false);
            else if (String.IsNullOrEmpty(strRecPath) == false)
                form.LoadItemByRecPath(strRecPath, false);
            else if (String.IsNullOrEmpty(strRefID) == false)
                form.LoadItemByRefID(strRefID, false);
            else
            {
                form.Close();
                MessageBox.Show(this, "所选定行的条码号、记录路径、参考ID全都为空，无法定位记录");
            }
        }

        private void listView_parent_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSelectedCount = this.listView_parent.SelectedItems.Count;

            // 
            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_parent_selectAll_Click);
            if (nSelectedCount == this.listView_parent.Items.Count)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("选定全部出错状态的事项(&E)");
            menuItem.Click += new System.EventHandler(this.menu_parent_selectAllErrorLines_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 
            menuItem = new MenuItem("移除(&R)");
            menuItem.Click += new System.EventHandler(this.menu_parent_removeSelectedLines_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_parent, new Point(e.X, e.Y));		

        }

        // 移除选定的行
        void menu_parent_removeSelectedLines_Click(object sender, EventArgs e)
        {
            if (this.listView_parent.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定任何需要移除的事项");
                return;
            }

            DialogResult result = MessageBox.Show(
                this,
                "确实要从列表中移除所选定的 " + this.listView_parent.SelectedItems.Count.ToString() + " 个事项?\r\n\r\n(注：本操作不会从数据库中删除记录)",
                "SettlementForm",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Cancel)
                return;

            ListViewUtil.DeleteSelectedItems(this.listView_parent);

            SetNextButtonEnable();
        }

        // 选定所有状态为错误的行
        void menu_parent_selectAllErrorLines_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView_parent.Items)
            {
                if (item.ImageIndex == TYPE_ERROR)
                    item.Selected = true;
                else
                {
                    if (item.Selected == true)
                        item.Selected = false;
                }
            }
        }

        // 全选
        void menu_parent_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllLines(this.listView_parent);
        }

        private void listView_member_MouseUp(object sender, MouseEventArgs e)
        {

        }
    }

    // 装订单打印 定义了特定缺省值的PrintOption派生类
    internal class PrintBindingPrintOption : PrintOption
    {
        string PublicationType = "连续出版物"; // 图书 连续出版物

        public override void LoadData(ApplicationInfo ai,
            string strPath)
        {
            string strNamePath = strPath;
            if (this.PublicationType != "图书")
                strNamePath = "series_" + strNamePath;
            base.LoadData(ai, strNamePath);
        }

        public override void SaveData(ApplicationInfo ai,
            string strPath)
        {
            string strNamePath = strPath;
            if (this.PublicationType != "图书")
                strNamePath = "series_" + strNamePath;
            base.SaveData(ai, strNamePath);
        }

        public PrintBindingPrintOption(string strDataDir,
            string strPublicationType)
        {
            Debug.Assert(this.PublicationType == "连续出版物", "目前仅支持连续出版物");


            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            this.PageHeaderDefault = "%date% 装订单 - 来自: %sourcedescription%";
            this.PageFooterDefault = "";

            this.TableTitleDefault = "合订册";

            this.LinesPerPageDefault = 20;

            // Columns缺省值
            Columns.Clear();

            // "missing -- 缺期状态",
            Column column = new Column();
            column.Name = "missing -- 缺期状态";
            column.Caption = "缺期状态";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "publishTime -- 出版日期",
            column = new Column();
            column.Name = "publishTime -- 出版日期";
            column.Caption = "出版日期";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "volume -- 卷期号"
            column = new Column();
            column.Name = "volume -- 卷期号";
            column.Caption = "卷期号";
            column.MaxChars = -1;
            this.Columns.Add(column);


            // "barcode -- 册条码号"
            column = new Column();
            column.Name = "barcode -- 册条码号";
            column.Caption = "册条码号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "intact -- 完好率"
            column = new Column();
            column.Name = "intact -- 完好率";
            column.Caption = "完好率";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "refID -- 参考ID"
            column = new Column();
            column.Name = "refID -- 参考ID";
            column.Caption = "参考ID";
            column.MaxChars = -1;
            this.Columns.Add(column);

        }
    }
}