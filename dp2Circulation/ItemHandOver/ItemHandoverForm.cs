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

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;

using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;

using DigitalPlatform.LibraryClient.localhost;
using System.Threading;

namespace dp2Circulation
{
    /// <summary>
    /// 典藏移交窗
    /// </summary>
    public partial class ItemHandoverForm : BatchPrintFormBase
    {
        FillThread _fillThread = null;

        // 源和目标书目记录的对照表。已经进行了覆盖提问和处理的，进入其中
        Hashtable m_biblioRecPathTable = new Hashtable();

        // 源书目库和目标书目库的对照表
        Hashtable m_targetDbNameTable = new Hashtable();

        string SourceStyle = "";    // "batchno" "barcodefile"

        string BatchNo = "";    // 面板输入的批次号
        string LocationString = ""; // 面板输入的馆藏地点

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

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
        /// <summary>
        /// 事项图标下标: 已通过校验
        /// </summary>
        public const int TYPE_VERIFIED = 2;

        /// <summary>
        /// 最近使用过的条码文件全路径
        /// </summary>
        public string BarcodeFilePath = "";
        /// <summary>
        /// 最近使用过的记录路径文件全路径
        /// </summary>
        public string RecPathFilePath = "";

        // int m_nGreenItemCount = 0;

        // 参与排序的列号数组
        SortColumns SortColumns_in = new SortColumns();
        SortColumns SortColumns_outof = new SortColumns();

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

        const int WM_LOADSIZE = API.WM_USER + 201;

        /*
        // 表示tab页进入到已经可以继续下一步的状态
        bool PageLoadOK 
        {
            get
            {
                // 只要list中有事项，就表明装入了数据
                if (this.listView_in.Items.Count > 0)
                    return true;
                return false;
            }
        }

        bool PageVerifyOK 
        {
            get
            {
                string strError = "";
                return ReportVerifyState(out strError);
            }
        }

        // bool PagePrintOK = false;
         * */
        /// <summary>
        /// 构造函数
        /// </summary>
        public ItemHandoverForm()
        {
            InitializeComponent();
        }


        private void ItemHandoverForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            CreateColumnHeader(this.listView_in);

            CreateColumnHeader(this.listView_outof);

#if NO
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            // 2009/2/2
            this.comboBox_load_type.Text = this.MainForm.AppInfo.GetString(
                "itemhandoverform",
                "publication_type",
                "图书");


            this.BarcodeFilePath = this.MainForm.AppInfo.GetString(
                "itemhandoverform",
                "barcode_filepath",
                "");

            this.RecPathFilePath = this.MainForm.AppInfo.GetString(
    "itemhandoverform",
    "recpath_filepath",
    "");

            this.BatchNo = this.MainForm.AppInfo.GetString(
                "itemhandoverform",
                "batchno",
                "");

            this.LocationString = this.MainForm.AppInfo.GetString(
                "itemhandoverform",
                "location_string",
                "");

            this.checkBox_verify_autoUppercaseBarcode.Checked = 
                this.MainForm.AppInfo.GetBoolean(
                "itemhandoverform",
                "auto_uppercase_barcode",
                true);

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);
        }

        private void ItemHandoverForm_FormClosing(object sender, 
            FormClosingEventArgs e)
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

            if (this.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有册信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    this.FormCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void ItemHandoverForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            if (this._scanBarcodeForm != null)
                this._scanBarcodeForm.Close();

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                // 2009/2/2
                this.MainForm.AppInfo.SetString(
                    "itemhandoverform",
                    "publication_type",
                    this.comboBox_load_type.Text);

                this.MainForm.AppInfo.SetString(
                    "itemhandoverform",
                    "barcode_filepath",
                    this.BarcodeFilePath);

                this.MainForm.AppInfo.SetString(
                    "itemhandoverform",
                    "recpath_filepath",
                    this.RecPathFilePath);

                this.MainForm.AppInfo.SetString(
                    "itemhandoverform",
                    "batchno",
                    this.BatchNo);

                this.MainForm.AppInfo.SetString(
                    "itemhandoverform",
                    "location_string",
                    this.LocationString);

                this.MainForm.AppInfo.SetBoolean(
                    "itemhandoverform",
                    "auto_uppercase_barcode",
                    this.checkBox_verify_autoUppercaseBarcode.Checked);
            }

            SaveSize();
        }

        /*public*/ void LoadSize()
        {
#if NO
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state");
#endif

            string strWidths = this.MainForm.AppInfo.GetString(
                "itemhandoverform",
                "list_in_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_in,
                    strWidths,
                    true);
            }

            strWidths = this.MainForm.AppInfo.GetString(
    "itemhandoverform",
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
"itemhandoverform",
"splitContainer_main_ratio");

            this.MainForm.LoadSplitterPos(
this.splitContainer_inAndOutof,
"itemhandoverform",
"splitContainer_inandoutof_ratio");
        }

        /*public*/ void SaveSize()
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_in);
                this.MainForm.AppInfo.SetString(
                    "itemhandoverform",
                    "list_in_width",
                    strWidths);

                strWidths = ListViewUtil.GetColumnWidthListString(this.listView_outof);
                this.MainForm.AppInfo.SetString(
                    "itemhandoverform",
                    "list_outof_width",
                    strWidths);

                this.MainForm.SaveSplitterPos(
    this.splitContainer_main,
    "itemhandoverform",
    "splitContainer_main_ratio");

                this.MainForm.SaveSplitterPos(
    this.splitContainer_inAndOutof,
    "itemhandoverform",
    "splitContainer_inandoutof_ratio");
            }
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
            this.comboBox_load_type.Enabled = this.ScanMode == true ? false : bEnable;
            this.button_load_loadFromBatchNo.Enabled = this.ScanMode == true ? false : bEnable;
            this.button_load_loadFromBarcodeFile.Enabled = this.ScanMode == true ? false : bEnable;
            this.button_load_loadFromRecPathFile.Enabled = this.ScanMode == true ? false : bEnable;
            this.button_load_scanBarcode.Enabled = this.ScanMode == true ? false : bEnable;

            // next button
            if (bEnable == true)
                SetNextButtonEnable();
            else
                this.button_next.Enabled = false;

            // verify page
            this.textBox_verify_itemBarcode.Enabled = bEnable;
            this.button_verify_load.Enabled = bEnable;
            this.checkBox_verify_autoUppercaseBarcode.Enabled = bEnable;


            // print page
            this.button_print_option.Enabled = bEnable;
            this.button_print_printCheckedList.Enabled = bEnable;

            this.button_print_printNormalList.Enabled = bEnable;

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


        // 根据条码号文件装载
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
                nRet = ConvertBarcodeFile(dlg.FileName,
                    strRecPathFilename,
                    out nDupCount,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = LoadFromRecPathFile(strRecPathFilename,
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


#if NO
        // 根据条码号文件装载
        private void button_load_loadFromFile_Click(object sender, EventArgs e)
        {
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
            StreamReader sr = null;
            try
            {
                // 打开文件
                sr = new StreamReader(dlg.FileName);


                EnableControls(false);
                // MainForm.ShowProgress(true);

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

                        this.listView_in.Items.Clear();
                        this.SortColumns_in.Clear();
                        SortColumns.ClearColumnSortDisplay(this.listView_in.Columns);

                        this.listView_outof.Items.Clear();
                        this.SortColumns_outof.Clear();
                        SortColumns.ClearColumnSortDisplay(this.listView_outof.Columns);
                    }


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
                        // stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");
                    }

                    // 设置进度范围
                    stop.SetProgressRange(0, nLineCount);
                    // stop.SetProgressValue(0);

                    // 逐行处理
                    // 文件回头?
                    // sr.BaseStream.Seek(0, SeekOrigin.Begin);

                    sr.Close();

                    sr = new StreamReader(dlg.FileName);


                    for (int i=0; ; i++)
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


                        // 根据册条码号，装入册记录
                        // return: 
                        //      -2  册条码号已经在list中存在了
                        //      -1  出错
                        //      1   成功
                        int nRet = LoadOneItem(strLine,
                            this.listView_in,
                            null,
                            out strError);
                        if (nRet == -2)
                            nDupCount++;
                        /*
                        if (nRet == -1)
                            goto ERROR1;
                         * */

                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("装入完成。");
                    stop.HideProgress();

                    EnableControls(true);
                    // MainForm.ShowProgress(false);
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                sr.Close();
            }

            // 记忆文件名
            this.BarcodeFilePath = dlg.FileName;
            this.Text = "典藏移交 " + Path.GetFileName(this.BarcodeFilePath);

            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " +nDupCount.ToString() + "个重复条码事项被忽略。");
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
            this.Text = "典藏移交";
            MessageBox.Show(this, strError);
        }
#endif
        public string LoadType
        {
            get
            {
                return (string)Invoke(new Func<string>(GetLoadType));
            }
        }

        string GetLoadType()
        {
            return this.comboBox_load_type.Text;
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
                    if (this.InvokeRequired == false)
                    {
                        DialogResult temp_result = MessageBox.Show(this,
        strError + "\r\n\r\n是否重试?",
        this.FormCaption,
        MessageBoxButtons.RetryCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_GETRECORDS;
                    }
                    return -1;
                }

                records.AddRange(searchresults);

                // 去掉已经做过的一部分
                lines.RemoveRange(0, searchresults.Length);

                if (lines.Count == 0)
                    break;
            }

            // 准备 DOM 和书目摘要等
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

            if (this.InvokeRequired == false)
                this.listView_in.BeginUpdate();
            try
            {

                for (int i = 0; i < infos.Count; i++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断1";
                            return -1;
                        }
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
                        this.LoadType,  // this.comboBox_load_type.Text,
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
                if (this.InvokeRequired == false)
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

        #region 以前的代码

#if NO

        // 根据册条码号或者记录路径，装入册记录
        // parameters:
        //      strBarcodeOrRecPath 册条码号或者记录路径。如果内容前缀为"@path:"则表示为路径
        //      strMatchLocation    附加的馆藏地点匹配条件。如果==null，表示没有这个附加条件(注意，""和null含义不同，""表示确实要匹配这个值)
        // return: 
        //      -2  册条码号已经在list中存在了(行没有加入listview中)
        //      -1  出错(注意表示出错的行已经加入listview中了)
        //      0   因为馆藏地点不匹配，没有加入list中
        //      1   成功
        public int LoadOneItem(
            string strBarcodeOrRecPath,
            ListView list,
            string strMatchLocation,
            out string strError)
        {
            strError = "";

            // 判断是否有 @path: 前缀，便于后面分支处理
            bool bIsRecPath = StringUtil.HasHead(strBarcodeOrRecPath, "@path:"); ;

            string strItemXml = "";
            string strBiblioText = "";

            string strItemRecPath = "";
            string strBiblioRecPath = "";

            byte[] item_timestamp = null;

            long lRet = Channel.GetItemInfo(
                stop,
                strBarcodeOrRecPath,
                "xml",
                out strItemXml,
                out strItemRecPath,
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
            string strTargetRecPath = "";

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
                        strTargetRecPath = ListViewUtil.GetItemText(curitem, COLUMN_TARGETRECPATH);
                    }
                }
            }

            if (strBiblioSummary == "")
            {
                string[] formats = new string[3];
                formats[0] = "summary";
                formats[1] = "@isbnissn";
                formats[2] = "targetrecpath";
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
                    Debug.Assert(results != null && results.Length == 3, "results必须包含3个元素");
                    strBiblioSummary = results[0];
                    strISBnISSN = results[1];
                    strTargetRecPath = results[2];
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
                strError = ex.Message;
                goto ERROR1;
            }


            // 附加的馆藏地点匹配
            if (strMatchLocation != null)
            {
                string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                    "location");

                // 2013/3/26
                if (strLocation == null)
                    strLocation = "";

                if (strMatchLocation != strLocation)
                    return 0;
            }

            {
                ListViewItem item = AddToListView(list,
                    dom,
                    strItemRecPath,
                    strBiblioSummary,
                    strISBnISSN,
                    strBiblioRecPath,
                    strTargetRecPath);

                // 设置timestamp/xml
                OriginItemData data = (OriginItemData)item.Tag;
                Debug.Assert(data != null, "");
                data.Timestamp = item_timestamp;
                data.Xml = strItemXml;

                if (this.comboBox_load_type.Text == "连续出版物")
                {
                    // 检查是否为合订册记录或者单册记录。不能为合订成员
                    // return:
                    //      0   不是。图标已经设置为TYPE_ERROR
                    //      1   是。图标尚未设置
                    int nRet = CheckBindingItem(item);
                    if (nRet == 1)
                    {
                        // 图标
                        SetItemColor(item, TYPE_NORMAL);
                    }
                }
                else
                {
                    Debug.Assert(this.comboBox_load_type.Text == "图书", "");
                    // 图标
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

                // 即将废止
        // 根据册记录DOM设置ListViewItem除第一列以外的文字
        // 本函数会自动把事项的data.Changed设置为false
        // parameters:
        //      bSetBarcodeColumn   是否要设置条码列内容(第一列)
        static void SetListViewItemText(XmlDocument dom,
            bool bSetBarcodeColumn,
            string strRecPath,
            string strBiblioSummary,
            string strISBnISSN,
            string strBiblioRecPath,
            string strTargetRecPath,
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

            if (bSetBarcodeColumn == true)
            {
                string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
                    "barcode");
                ListViewUtil.ChangeItemText(item, COLUMN_BARCODE, strBarcode);
            }

            SetItemColor(item, TYPE_NORMAL);

        }

        // 即将废止
        static ListViewItem AddToListView(ListView list,
            XmlDocument dom,
            string strRecPath,
            string strBiblioSummary,
            string strISBnISSN,
            string strBiblioRecPath,
            string strTargetRecPath)
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
                strTargetRecPath,
                item);
            list.Items.Add(item);
            return item;
        }
#endif

        #endregion // 以前的代码

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
            // 确保线程安全 2014/9/3
            if (item.ListView != null && item.ListView.InvokeRequired)
            {
                item.ListView.BeginInvoke(new Action<ListViewItem, int>(SetItemColor), item, nType);
                return;
            }

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
            byte [] baTimestamp,
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
            else if (this.tabControl_main.SelectedTab == this.tabPage_move)
            {
                int nProcessiingCount = -1;
                if (this.comboBox_load_type.Text == "图书")
                {
                    // 看看有没有至少一个目标路径、或者来自工作库的实体记录
                    int nNeedMoveCount = 0;
                    // 看看有多少个“加工中”状态的行
                    nProcessiingCount = 0;
                    foreach (ListViewItem item in this.listView_in.Items)
                    {
                        string strTargetRecPath = ListViewUtil.GetItemText(item, COLUMN_TARGETRECPATH);
                        if (String.IsNullOrEmpty(strTargetRecPath) == false)
                            nNeedMoveCount++;
                        else
                        {
                            string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);
                            string strBiblioDbName = Global.GetDbName(strBiblioRecPath);
                            if (this.MainForm.IsOrderWorkDb(strBiblioDbName) == true)
                                nNeedMoveCount++;
                        }

                        string strState = ListViewUtil.GetItemText(item, COLUMN_STATE);
                        if (String.IsNullOrEmpty(strState) == false
                            && Global.IncludeStateProcessing(strState) == true)
                            nProcessiingCount++;
                    }

                    if (nNeedMoveCount > 0)
                    {
                        this.button_move_moveAll.Enabled = true;
                        this.button_move_moveAll.Text = "转移到目标库 ["+nNeedMoveCount.ToString()+"] (&M)...";
                    }
                    else
                    {
                        this.button_move_moveAll.Enabled = false;
                        this.button_move_moveAll.Text = "转移到目标库(&M)...";
                    }
                }
                else
                {
                    this.button_move_moveAll.Enabled = false;
                    this.button_move_moveAll.Text = "转移到目标库(&M)...";
                }

                {
                    if (nProcessiingCount == -1)
                    {
                        nProcessiingCount = 0;
                        foreach (ListViewItem item in this.listView_in.Items)
                        {
                            string strState = ListViewUtil.GetItemText(item, COLUMN_STATE);
                            if (String.IsNullOrEmpty(strState) == false
                                && Global.IncludeStateProcessing(strState) == true)
                                nProcessiingCount++;
                        }
                    }

                    if (nProcessiingCount > 0)
                    {
                        this.button_move_changeStateAll.Enabled = true;
                        this.button_move_changeStateAll.Text = "清除“加工中”状态 ["+nProcessiingCount.ToString()+"] (&C)...";
                    }
                    else
                    {
                        this.button_move_changeStateAll.Enabled = false;
                        this.button_move_changeStateAll.Text = "清除“加工中”状态(&C)...";
                    }
                }

                if (this.listView_in.Items.Count > 0)
                {
                    this.button_move_notifyReader.Enabled = true;
                    this.button_move_notifyReader.Text = "通知荐购读者 [" + this.listView_in.Items.Count .ToString()+ "] (&N)...";
                }
                else
                {
                    this.button_move_notifyReader.Enabled = false;
                    this.button_move_notifyReader.Text = "通知荐购读者(&N)...";
                }

                if (this.listView_in.Items.Count > 0)
                {
                    this.button_move_changeLocation.Enabled = true;
                    this.button_move_changeLocation.Text = "修改馆藏地 [" + this.listView_in.Items.Count.ToString() + "] (&L)...";
                }
                else
                {
                    this.button_move_changeLocation.Enabled = false;
                    this.button_move_changeLocation.Text = "修改馆藏地(&L)...";
                }

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

        // 下一步
        private void button_next_Click(object sender, EventArgs e)
        {
            // string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_load)
            {
                this.tabControl_main.SelectedTab = this.tabPage_verify;
                this.button_next.Enabled = true;
                this.textBox_verify_itemBarcode.Focus();
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {
                this.tabControl_main.SelectedTab = this.tabPage_move;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_move)
            {
                this.tabControl_main.SelectedTab = this.tabPage_print;
                this.button_next.Enabled = false;
                this.button_print_printCheckedList.Focus();
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }

            this.SetNextButtonEnable();
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
                strError = "列表中有 " +nRedCount+ " 个错误事项(红色行)。请修改数据后重新装载。";
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

            strError = "验证尚未完成。\r\n\r\n列表中有:\r\n已验证事项(绿色) "+nGreenCount.ToString()+" 个\r\n错误事项(红色) " +nRedCount.ToString()+ "个\r\n未验证事项(白色) " +nWhiteCount.ToString()+ "个\r\n集合外事项(位于下方列表内) " +this.listView_outof.Items.Count+ "个\r\n\r\n(只有全部事项都为已验证状态(绿色)，才表明验证已经完成)";
            return false;
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_load)
            {
                this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {
                this.button_next.Enabled = true;
                if (this.ScanMode == true)
                    this.ScanMode = false;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_move)
            {
                this.SetNextButtonEnable();
                if (this.ScanMode == true)
                    this.ScanMode = false;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                this.button_next.Enabled = false;
                if (this.ScanMode == true)
                    this.ScanMode = false;
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }
        }

        private void textBox_verify_itemBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_verify_load;
        }

        // 扫入一个册条码号
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
                    +this.textBox_verify_itemBarcode.Text
                    +" ...");
                stop.BeginLoop();

                try
                {
                    // Debug.Assert(false, "");

#if NO
                    // 没有找到。加入out of list
                    int nRet = LoadOneItem(
                        this.textBox_verify_itemBarcode.Text,
                        this.listView_outof,
                        null,
                        out strError);
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
                        +" 所对应的事项虽然已经包含在集合内，但数据有误，无法通过验证。\r\n\r\n请对该条码相关数据进行修改，然后刷新事项，并重新扫入条码进行验证。";
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

        void SetVerifyPageNextButtonEnabled()
        {
            if (GetGreenItemCount() >= this.listView_in.Items.Count
    && this.listView_outof.Items.Count == 0)
                this.button_next.Enabled = true;
            else
                this.button_next.Enabled = false;
        }

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

        // 打印全部事项清单
        private void button_print_printNormalList_Click(object sender, EventArgs e)
        {
            // string strError = "";

            int nErrorCount = 0;
            int nUncheckedCount = 0;

            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                ListViewItem item = this.listView_in.Items[i];

                items.Add(item);

                if (item.ImageIndex == TYPE_ERROR)
                    nErrorCount++;
                if (item.ImageIndex == TYPE_NORMAL)
                    nUncheckedCount++;
            }

            if (nErrorCount != 0
                || nUncheckedCount != 0
                || this.listView_outof.Items.Count != 0)
            {
                MessageBox.Show(this, "警告：这里打印出的清单，其事项并未全部经过验证步骤。\r\n\r\n签字验收前，请务必完成验证步骤。");
            }


            PrintList("全部事项清单", items);
            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
        }

        // 打印已验证清单
        private void button_print_printCheckedList_Click(object sender, EventArgs e)
        {
            // 检查、警告
            string strError = "";

            bool bOK = ReportVerifyState(out strError);

            if (bOK == false)
            {
                string strWarning = strError + "\r\n\r\n" 
                    + "是否仍要打印已验证的部分(绿色行)?";
                DialogResult result = MessageBox.Show(this,
strWarning,
this.FormCaption,
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    strError = "放弃打印。";
                    goto ERROR1;
                }
            }

            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                ListViewItem item = this.listView_in.Items[i];

                if (item.ImageIndex == TYPE_VERIFIED)
                    items.Add(item);
            }

            if (items.Count == 0)
            {
                MessageBox.Show(this, "警告：当前并不存在已验证的事项(绿色行)。");
            }

            PrintList("已验证清单", items);

            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        void PrintList(
            string strTitle,
            List<ListViewItem> items)
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

        // 配置打印选项
        private void button_print_option_Click(object sender, EventArgs e)
        {
            string strNamePath = "handover_printoption";

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


            this.MainForm.AppInfo.LinkFormState(dlg, "handover_printoption_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(this.MainForm.AppInfo,
                strNamePath);
        }

        // 构造html页面
        // 无论是“打印已验证清单”还是“打印全部事项清单”都调用本函数
        int BuildHtml(
            List<ListViewItem> items,
            out List<string> filenames,
            out string strError)
        {
            strError = "";

            Hashtable macro_table = new Hashtable();

            string strNamePath = "handover_printoption";

            // 获得打印参数
            PrintOption option = new ItemHandoverPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);

            // 检查当前排序状态和包含种价格列之间是否存在矛盾
            if (bHasBiblioPriceColumn(option) == true)
            {

                if (this.SortColumns_in.Count != 0
                    && this.SortColumns_in[0].No == COLUMN_BIBLIORECPATH)
                {
                }
                else
                {
                    MessageBox.Show(this, "由于当前打印用到了 “种价格”列，为保证打印结果的准确，程序自动按 ‘种记录路径’ 列对全部列表事项进行一次自动排序。\r\n\r\n为避免这里的自动排序，可在打印前用鼠标左键点栏标题进行符合自己意愿的排序，只要最后一次点的是‘种记录路径’栏标题即可。");
                    ForceSortColumnsIn(COLUMN_BIBLIORECPATH);
                }


            }


            // 计算出页总数
            int nTablePageCount = items.Count / option.LinesPerPage;
            if ((items.Count % option.LinesPerPage) != 0)
                nTablePageCount++;

            int nPageCount = nTablePageCount + 1;

            // 2009/7/24 changed
            if (this.SourceStyle == "batchno")
            {
                // 2008/11/22
                macro_table["%batchno%"] = HttpUtility.HtmlEncode(this.BatchNo); // 批次号
                macro_table["%location%"] = HttpUtility.HtmlEncode(this.LocationString); // 馆藏地点 用HtmlEncode()的原因是要防止里面出现的“<不指定>”字样
            }
            else
            {
                macro_table["%batchno%"] = "";
                macro_table["%location%"] = "";
            }

            macro_table["%pagecount%"] = nPageCount.ToString();
            macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            // 2009/7/24 changed
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


            filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名

            string strFileNamePrefix = this.MainForm.DataDir + "\\~itemhandover";

            string strFileName = "";

            // 输出统计信息页
            {
                int nItemCount = items.Count;
                int nBiblioCount = GetBiblioCount(items);
                string strTotalPrice = GetTotalPrice(items).ToString();

                macro_table["%itemcount%"] = nItemCount.ToString();
                macro_table["%bibliocount%"] = nBiblioCount.ToString();
                macro_table["%totalprice%"] = strTotalPrice;

                macro_table["%pageno%"] = "1";

                // 2008/11/23
                macro_table["%datadir%"] = this.MainForm.DataDir;   // 便于引用datadir下templates目录内的某些文件
                ////macro_table["%libraryserverdir%"] = this.MainForm.LibraryServerDir;  // 便于引用服务器端的CSS文件
                // 2009/10/10
                macro_table["%cssfilepath%"] = this.GetAutoCssUrl(option, "itemhandover.css");  // 便于引用服务器端或“css”模板的CSS文件

                strFileName = strFileNamePrefix + "0" + ".html";

                filenames.Add(strFileName);

                string strTemplateFilePath = option.GetTemplatePageFilePath("统计页");
                if (String.IsNullOrEmpty(strTemplateFilePath) == false)
                {
                    /*
<html>
<head>
	老用法<LINK href='%libraryserverdir%/itemhandover.css' type='text/css' rel='stylesheet'>
	新用法<LINK href='%cssfilepath%' type='text/css' rel='stylesheet'>
</head>
<body>
	<div class='pageheader'>%date% 册移交清单 -- 批: %batchno% -- 地: %location% -- (共 %pagecount% 页)</div>
	<div class='tabletitle'>%date% 册移交清单 -- %barcodefilepath%</div>
	<div class='itemcount'>册数: %itemcount%</div>
	<div class='bibliocount'>种数: %bibliocount%</div>
	<div class='totalprice'>总价: %totalprice%</div>
	<div class='sepline'><hr/></div>
	<div class='batchno'>批次号: %batchno%</div>
	<div class='location'>馆藏地点: %location%</div>
	<div class='sepline'><hr/></div>
	<div class='sender'>移交者: </div>
	<div class='recipient'>接受者: </div>
	<div class='pagefooter'>%pageno%/%pagecount%</div>
</body>
</html>
                     * * */

                    // 根据模板打印
                    string strContent = "";
                    // 能自动识别文件内容的编码方式的读入文本文件内容模块
                    // return:
                    //      -1  出错
                    //      0   文件不存在
                    //      1   文件存在
                    int nRet = Global.ReadTextFileContent(strTemplateFilePath,
                        out strContent,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    string strResult = StringUtil.MacroString(macro_table,
                        strContent);
                    StreamUtil.WriteText(strFileName,
                        strResult);
                }
                else
                {
                    // 缺省的固定内容打印

                    BuildPageTop(option,
                        macro_table,
                        strFileName,
                        false);

                    // 内容行

                    StreamUtil.WriteText(strFileName,
                        "<div class='itemcount'>册数: " + nItemCount.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='bibliocount'>种数: " + nBiblioCount.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='totalprice'>总价: " + strTotalPrice + "</div>");

                    StreamUtil.WriteText(strFileName,
                        "<div class='sepline'><hr/></div>");

                    if (this.SourceStyle == "batchno")
                    {
                        // 2008/11/22
                        if (String.IsNullOrEmpty(this.BatchNo) == false)
                        {
                            StreamUtil.WriteText(strFileName,
                                "<div class='batchno'>批次号: " + this.BatchNo + "</div>");
                        }
                        if (String.IsNullOrEmpty(this.LocationString) == false)
                        {
                            StreamUtil.WriteText(strFileName,
                                "<div class='location'>馆藏地点: " + this.LocationString + "</div>");
                        }
                    }

                    if (this.SourceStyle == "barcodefile")
                    {
                        if (String.IsNullOrEmpty(this.BarcodeFilePath) == false)
                        {
                            StreamUtil.WriteText(strFileName,
                                "<div class='barcodefilepath'>条码号文件: " + this.BarcodeFilePath + "</div>");
                        }
                    }

                    StreamUtil.WriteText(strFileName,
                        "<div class='sepline'><hr/></div>");


                    StreamUtil.WriteText(strFileName,
                        "<div class='sender'>移交者: </div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='recipient'>接受者: </div>");


                    BuildPageBottom(option,
                        macro_table,
                        strFileName,
                        false);
                }

            }

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
                        items,
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


            return 0;
        }

        // 2009/10/10
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
            string strFileName,
            bool bOutputTable)
        {
            /*
            string strLibraryServerUrl = this.MainForm.AppInfo.GetString(
    "config",
    "circulation_server_url",
    "");
            int pos = strLibraryServerUrl.LastIndexOf("/");
            if (pos != -1)
                strLibraryServerUrl = strLibraryServerUrl.Substring(0, pos);
             * */

            // string strCssUrl = this.MainForm.LibraryServerDir + "/itemhandover.css";
            // 2009/10/10 changed
            string strCssUrl = GetAutoCssUrl(option, "itemhandover.css");

            /*
            // 2009/10/9
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                strCssUrl = strCssFilePath;
            else
                strCssUrl = this.MainForm.LibraryServerDir + "/itemhandover.css";    // 缺省的
             * */

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

                /*
                StreamUtil.WriteText(strFileName,
                    "<hr class='pageheader' />");
                 * */
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

        // 汇总种价格。假定nIndex处在切换行(同一种内最后一行)
        static decimal ComputeBiblioPrice(List<ListViewItem> items,
            int nIndex)
        {
            decimal total = 0;
            string strBiblioRecPath = GetColumnContent(items[nIndex], "biblioRecpath");
            for (int i = nIndex; i>=0; i--)
            {
                ListViewItem item = items[i];

                string strCurBiblioRecPath = GetColumnContent(item, "biblioRecpath");

                if (strCurBiblioRecPath != strBiblioRecPath)
                    break;

                string strPrice = GetColumnContent(item, "price");

                // 提取出纯数字
                string strPurePrice = PriceUtil.GetPurePrice(strPrice);

                if (String.IsNullOrEmpty(strPurePrice) == true)
                    continue;

                total += Convert.ToDecimal(strPurePrice);
            }

            return total;
        }

        // 是否包含种价格列?
        static bool bHasBiblioPriceColumn(PrintOption option)
        {
            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                string strText = StringUtil.GetLeft(column.Name);


                if (strText == "biblioPrice"
                    || strText == "种价格")
                    return true;
            }

            return false;
        }

        int BuildTableLine(PrintOption option,
            List<ListViewItem> items,
            string strFileName,
            int nPage,
            int nLine)
        {
            // 栏目内容
            string strLineContent = "";

            bool bBiblioSumLine = false;    // 是否为种的最后一行(汇总行)

            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                int nIndex = nPage * option.LinesPerPage + nLine;

                if (nIndex >= items.Count)
                    break;

                ListViewItem item = items[nIndex];

                string strContent = GetColumnContent(item,
                    column.Name);

                if (strContent == "!!!#")
                    strContent = ((nPage * option.LinesPerPage) + nLine + 1).ToString();

                if (strContent == "!!!biblioPrice")
                {
                    // 看看自己是不是处在切换边沿
                    string strCurLineBiblioRecPath = GetColumnContent(item, "biblioRecpath");

                    string strNextLineBiblioRecPath = "";

                    if (nIndex < items.Count - 1)
                    {
                        ListViewItem next_item = items[nIndex + 1];
                        strNextLineBiblioRecPath = GetColumnContent(next_item, "biblioRecpath");
                    }

                    if (strCurLineBiblioRecPath != strNextLineBiblioRecPath)
                    {
                        // 处在切换边沿

                        // 汇总前面的册价格
                        strContent = ComputeBiblioPrice(items, nIndex).ToString();
                        bBiblioSumLine = true;
                    }
                    else
                    {
                        // 其他普通行
                        strContent = "&nbsp;";
                    }


                }

                // 截断字符串
                if (column.MaxChars != -1)
                {
                    if (strContent.Length > column.MaxChars)
                    {
                        strContent = strContent.Substring(0, column.MaxChars);
                        strContent += "...";
                    }
                }

                if (String.IsNullOrEmpty(strContent) == true)
                    strContent = "&nbsp;";

                string strClass = StringUtil.GetLeft(column.Name);

                strLineContent +=
                    "<td class='" + strClass + "'>" + strContent + "</td>";
            }

            if (bBiblioSumLine == false)
            {
                StreamUtil.WriteText(strFileName,
        "<tr class='content'>");
            }
            else
            {
                StreamUtil.WriteText(strFileName,
        "<tr class='content_biblio_sum'>");
            }

            StreamUtil.WriteText(strFileName,
    strLineContent);

            StreamUtil.WriteText(strFileName,
                "</tr>");

            return 0;
        }


        // 获得栏目内容
        static string GetColumnContent(ListViewItem item,
            string strColumnName)
        {
            // 去掉"-- ?????"部分
            /*
            string strText = strColumnName;
            int nRet = strText.IndexOf("--", 0);
            if (nRet != -1)
                strText = strText.Substring(0, nRet).Trim();
             * */

            string strText = StringUtil.GetLeft(strColumnName);

            try
            {

                // 要中英文都可以
                switch (strText)
                {
                    case "no":
                    case "序号":
                        return "!!!#";  // 特殊值，表示序号
                    case "barcode":
                    case "册条码号":
                        return item.SubItems[COLUMN_BARCODE].Text;
                    case "errorInfo":
                    case "summary":
                    case "摘要":
                        return item.SubItems[COLUMN_SUMMARY].Text;
                    case "isbnIssn":
                    case "ISBN/ISSN":
                        return item.SubItems[COLUMN_ISBNISSN].Text;
                    case "state":
                        return item.SubItems[COLUMN_STATE].Text;
                    case "location":
                    case "馆藏地点":
                        return ListViewUtil.GetItemText(item, COLUMN_LOCATION);
                    case "price":
                    case "册价格":
                        return item.SubItems[COLUMN_PRICE].Text;
                    case "bookType":
                        return item.SubItems[COLUMN_BOOKTYPE].Text;
                    case "registerNo":
                        return item.SubItems[COLUMN_REGISTERNO].Text;
                    case "comment":
                        return item.SubItems[COLUMN_COMMENT].Text;
                    case "mergeComment":
                        return item.SubItems[COLUMN_MERGECOMMENT].Text;
                    case "batchNo":
                        return item.SubItems[COLUMN_BATCHNO].Text;
                    case "borrower":
                        return item.SubItems[COLUMN_BORROWER].Text;
                    case "borrowDate":
                        return item.SubItems[COLUMN_BORROWDATE].Text;
                    case "borrowPeriod":
                        return item.SubItems[COLUMN_BORROWPERIOD].Text;
                    case "recpath":
                        return item.SubItems[COLUMN_RECPATH].Text;
                    case "biblioRecpath":
                    case "种记录路径":
                        return item.SubItems[COLUMN_BIBLIORECPATH].Text;
                    case "accessNo":
                    case "索书号":
                    case "索取号":
                        // 打印前要去掉 {ns} 等命令部分 2014/9/6
                        return StringUtil.GetPlainTextCallNumber(ListViewUtil.GetItemText(item, COLUMN_ACCESSNO));
                    case "biblioPrice":
                    case "种价格":
                        return "!!!biblioPrice";  // 特殊值，表示种价格
                    default:
                        return "undefined column";
                }
            }

            catch
            {
                return null;    // 表示没有这个subitem下标
            }

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

        static int GetBiblioCount(List<ListViewItem> items)
        {
            List<string> paths = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strText = "";

                try
                {
                    strText = item.SubItems[COLUMN_BIBLIORECPATH].Text;
                }
                catch
                {
                    continue;
                }
                paths.Add(strText);
            }

            // 排序
            paths.Sort();

            int nCount = 0;
            string strPrev = "";
            for (int i = 0; i < paths.Count; i++)
            {
                if (strPrev != paths[i])
                {
                    nCount++;
                    strPrev = paths[i];
                }
            }

            return nCount;
        }

        static decimal GetTotalPrice(List<ListViewItem> items)
        {
            decimal total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strPrice = "";

                try
                {
                    strPrice = item.SubItems[COLUMN_PRICE].Text;
                }
                catch
                {
                    continue;
                }

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                // 提取出纯数字
                string strPurePrice = PriceUtil.GetPurePrice(strPrice);

                if (String.IsNullOrEmpty(strPurePrice) == true)
                    continue;

                total += Convert.ToDecimal(strPurePrice);
            }

            return total;
        }

        private void listView_in_DoubleClick(object sender, EventArgs e)
        {
            LoadToEntityForm(this.listView_in);
        }

        private void listView_outof_DoubleClick(object sender, EventArgs e)
        {
            LoadToEntityForm(this.listView_outof);
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

            EntityForm form = new EntityForm();

            form.MainForm = this.MainForm;
            form.MdiParent = this.MainForm;
            form.Show();

            if (String.IsNullOrEmpty(strBarcode) == false)
                form.LoadItemByBarcode(strBarcode, false);
            else
                form.LoadItemByRecPath(strRecPath, false);
        }

        // 根据批次号检索装载
        private void button_load_loadFromBatchNo_Click(object sender, EventArgs e)
        {
            SearchByBatchnoForm dlg = new SearchByBatchnoForm();
            MainForm.SetControlFont(dlg, this.Font, false);

            // 2008/11/30
            dlg.BatchNo = this.BatchNo;
            dlg.ItemLocation = this.LocationString;

            dlg.CfgSectionName = "ItemHandoverForm_SearchByBatchnoForm";
            dlg.GetLocationValueTable -= new GetValueTableEventHandler(dlg_GetLocationValueTable);
            dlg.GetLocationValueTable += new GetValueTableEventHandler(dlg_GetLocationValueTable);

            dlg.GetBatchNoTable -= new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.GetBatchNoTable += new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.RefDbName = "";
            /*
                dlg.RefDbName = EntityForm.GetDbName(this.entityEditControl1.RecPath);
             * */
            dlg.MainForm = this.MainForm;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.SourceStyle = "batchno";

            // 2008/11/22
            this.BatchNo = dlg.BatchNo;
            this.LocationString = dlg.ItemLocation;

            string strMatchLocation = dlg.ItemLocation;
            if (strMatchLocation == "<不指定>")
                strMatchLocation = null;    // null和""的区别很大

            string strBatchNo = dlg.BatchNo;
            if (strBatchNo == "<不指定>")
                strBatchNo = null;    // null和""的区别很大

            string strError = "";

            bool bClearBefore = true;
            if (Control.ModifierKeys == Keys.Control)
                bClearBefore = false;

            if (bClearBefore == true)
                ClearBefore();

            string strRecPathFilename = Path.GetTempFileName();

            try
            {
                // 检索 批次号 和 馆藏地点 将命中的记录路径写入文件
                int nRet = SearchBatchNoAndLocation(
                    this.comboBox_load_type.Text,
                    strBatchNo,
                    strMatchLocation,
                    strRecPathFilename,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = LoadFromRecPathFile(strRecPathFilename,
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
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        // 根据批次号检索装载
        private void button_load_loadFromBatchNo_Click(object sender, EventArgs e)
        {
            SearchByBatchnoForm dlg = new SearchByBatchnoForm();
            MainForm.SetControlFont(dlg, this.Font, false);

            // 2008/11/30
            dlg.BatchNo = this.BatchNo;
            dlg.ItemLocation = this.LocationString;

            dlg.CfgSectionName = "ItemHandoverForm_SearchByBatchnoForm";
            dlg.GetLocationValueTable -= new GetValueTableEventHandler(dlg_GetLocationValueTable);
            dlg.GetLocationValueTable += new GetValueTableEventHandler(dlg_GetLocationValueTable);

            dlg.GetBatchNoTable -= new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.GetBatchNoTable += new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.RefDbName = "";
            /*
                dlg.RefDbName = EntityForm.GetDbName(this.entityEditControl1.RecPath);
             * */
            dlg.MainForm = this.MainForm;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.SourceStyle = "batchno";

            // 2008/11/22
            this.BatchNo = dlg.BatchNo;
            this.LocationString = dlg.ItemLocation;

            string strMatchLocation = dlg.ItemLocation;

            if (strMatchLocation == "<不指定>")
                strMatchLocation = null;    // null和""的区别很大

            string strError = "";

            if (Control.ModifierKeys == Keys.Control)
            {
            }
            else
            {
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

                this.listView_in.Items.Clear();
                this.SortColumns_in.Clear();
                SortColumns.ClearColumnSortDisplay(this.listView_in.Columns);

                this.listView_outof.Items.Clear();
                this.SortColumns_outof.Clear();
                SortColumns.ClearColumnSortDisplay(this.listView_outof.Columns);
            }

            EnableControls(false);
            //MainForm.ShowProgress(true);

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
     this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
    "", // dlg.BatchNo,
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
                        strError = "检索全部 '" + this.comboBox_load_type.Text + "' 类型的册记录没有命中记录。";
                        goto ERROR1;
                    }
                }
                else
                {

                    lRet = Channel.SearchItem(
                        stop,
                        // 2010/2/25 changed
                         this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
                        // "<all>",
                        dlg.BatchNo,
                        -1,
                        "批次号",
                        "exact",
                        this.Lang,
                        "batchno",   // strResultSetName
                        "",    // strSearchStyle
                        "", // strOutputStyle
                        out strError);
                }
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "批次号 '"+dlg.BatchNo+"' 没有命中记录。";
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

                        /*
                        // 如果册条码号为空，则改用路径装载
                        // 2009/8/6
                        if (String.IsNullOrEmpty(strBarcode) == true)
                        {
                            strBarcode = "@path:" + strRecPath;
                        }*/

                        // 加速
                        strBarcode = "@path:" + strRecPath;


                        // 根据册条码号或者记录路径，装入册记录
                        // return: 
                        //      -2  册条码号已经在list中存在了
                        //      -1  出错
                        //      0   因为馆藏地点不匹配，没有加入list中
                        //      1   成功
                        int nRet = LoadOneItem(strBarcode,
                            this.listView_in,
                            strMatchLocation,
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

                if (this.listView_in.Items.Count == 0
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

            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif

        void dlg_GetBatchNoTable(object sender, GetKeyCountListEventArgs e)
        {
            Global.GetBatchNoTable(e,
                this,
                this.comboBox_load_type.Text,
                "item",
                this.stop,
                this.Channel);

#if NOOOOOOOOOOOOOOOOOOO
            string strError = "";

            if (e.KeyCounts == null)
                e.KeyCounts = new List<KeyCount>();

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在列出全部册批次号 ...");
            stop.BeginLoop();

            try
            {
                MainForm.SetProgressRange(100);
                MainForm.SetProgressValue(0);

                long lRet = Channel.SearchItem(
                    stop,
                    "<all>",
                    "", // strBatchNo
                    2000,  // -1,
                    "批次号",
                    "left",
                    this.Lang,
                    "batchno",   // strResultSetName
                    "keycount", // strOutputStyle
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "没有找到任何册批次号检索点";
                    return;
                }

                long lHitCount = lRet;

                long lStart = 0;
                long lCount = lHitCount;
                SearchResult[] searchresults = null;

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
                        "keycount",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSearchResult() error: " + strError;
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        // MessageBox.Show(this, "未命中");
                        return;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        if (searchresults[i].Cols == null)
                        {
                            strError = "请更新应用服务器和数据库内核到最新版本";
                            goto ERROR1;
                        }

                        KeyCount keycount = new KeyCount();
                        keycount.Key = searchresults[i].Path;
                        keycount.Count = Convert.ToInt32(searchresults[i].Cols[0]);
                        e.KeyCounts.Add(keycount);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
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

        void ForceSortColumnsIn(int nClickColumn)
        {
            // 2009/7/25 changed
            // 注：本函数如果发现第一列已经设置好，则不改变其方向。不过，这并不意味着其方向一定是升序
            this.SortColumns_in.SetFirstColumn(nClickColumn,
                this.listView_in.Columns,
                false);

            // 排序
            this.listView_in.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_in);
            this.listView_in.ListViewItemSorter = null;

            SetGroupBackcolor(
                this.listView_in,
                nClickColumn);
        }

        /*
        // 修改排序数组，设置第一列，把原来的列号推后
        public static void ChangeSortColumns(ref List<int> SortColumns,
            int nFirstColumn)
        {
            // 从数组中移走已经存在的值
            SortColumns.Remove(nFirstColumn);
            // 放到首部
            SortColumns.Insert(0, nFirstColumn);
        }
         * */

        // 根据排序键值的变化分组设置颜色
        static void SetGroupBackcolor(
            ListView list,
            int nSortColumn)
        {
            string strPrevText = "";
            bool bDark = false;
            for(int i=0;i<list.Items.Count;i++)
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

        // 对集合内列表进行排序
        private void listView_in_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns_in.SetFirstColumn(nClickColumn,
                this.listView_in.Columns);

            // 排序
            this.listView_in.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_in);

            this.listView_in.ListViewItemSorter = null;

            SetGroupBackcolor(
                this.listView_in,
                nClickColumn);

        }

        // 对集合外列表进行排序
        private void listView_outof_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns_outof.SetFirstColumn(nClickColumn,
                this.listView_outof.Columns);

            // 排序
            this.listView_outof.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_outof);

            this.listView_outof.ListViewItemSorter = null;

            SetGroupBackcolor(
                this.listView_outof,
                nClickColumn);
        }

        // 集合内列表的上下文菜单
        private void listView_in_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新选定的行(&S)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_refreshSelected_Click);
            if (this.listView_in.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新全部行(&R)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_refreshAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("修改馆藏地，对选定的 "
+ this.listView_in.SelectedItems.Count.ToString()
+ " 行(&C)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_changeLocation_Click);
            if (this.listView_in.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("清除已验证状态，对选定的 "
                + this.listView_in.SelectedItems.Count.ToString()
                + " 行(&L)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_clearVerifiedSelected_Click);
            if (this.listView_in.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);



            menuItem = new MenuItem("转移到目标库，对选定的 "
                + this.listView_in.SelectedItems.Count.ToString()
                + " 行(&M)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_moveToTargetDb_Click);
            if (this.listView_in.SelectedItems.Count == 0
                || this.comboBox_load_type.Text == "连续出版物")
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("清除“加工中”状态，对选定的 "
    + this.listView_in.SelectedItems.Count.ToString()
    + " 行(&C)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_clearProccessingState_Click);
            if (this.listView_in.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("通知荐购读者，对选定的 "
    + this.listView_in.SelectedItems.Count.ToString()
    + " 行(&C)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_notifyReader_Click);
            if (this.listView_in.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("移除(&D)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_deleteSelected_Click);
            if (this.listView_in.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_in, new Point(e.X, e.Y));		
        }

        static List<ListViewItem> GetSelectedItems(ListView list)
        {
            List<ListViewItem> results = new List<ListViewItem>();
            foreach (ListViewItem item in list.SelectedItems)
            {
                results.Add(item);
            }
            return results;
        }

        static List<ListViewItem> GetAllItems(ListView list)
        {
            List<ListViewItem> results = new List<ListViewItem>();
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];
                results.Add(item);
            }
            return results;
        }

        void menu_notifyReader_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_in.SelectedItems.Count == 0)
            {
                strError = "当前没有选定任何事项";
                goto ERROR1;
            }

            List<ListViewItem> items = GetSelectedItems(this.listView_in);

            int nNotifiedCount = 0;
            nRet = NotifyReader(
                items,
                out nNotifiedCount,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "成功通知书目记录 " + nNotifiedCount + " 个");
            return;
        ERROR1:
            this.SetNextButtonEnable();
            MessageBox.Show(this, strError);
        }

        // 对选定的行执行 修改馆藏地
        void menu_changeLocation_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_in.SelectedItems.Count == 0)
            {
                strError = "当前没有选定任何事项";
                goto ERROR1;
            }

            List<ListViewItem> items = GetSelectedItems(this.listView_in);

            // return:
            //      0   放弃修改
            //      1   发生了修改
            nRet = ChangeLocation(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
                return;

            this.SetNextButtonEnable();

            if (string.IsNullOrEmpty(strError) == false)
                MessageBox.Show(this, strError);
            return;
        ERROR1:
            this.SetNextButtonEnable();
            MessageBox.Show(this, strError);
        }

        // parameters:
        // return:
        //      0   放弃修改
        //      1   发生了修改
        int ChangeLocation(List<ListViewItem> items,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 汇报数据装载情况。
            // return:
            //      0   尚未装载任何数据    
            //      1   装载已经完成
            //      2   虽然装载了数据，但是其中有错误事项
            int nState = ReportLoadState(out strError);
            if (nState != 1)
            {
                DialogResult result = MessageBox.Show(this,
                    "在操作前，" + strError + "。\r\n\r\n确实要继续? ",
                    this.FormCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }
            // 在修改馆藏地点前，检查记录的情况
            // return:
            //      0   尚未装载任何数据    
            //      1   所有行符合条件
            //      2   其中有阻碍操作的事项
            nState = CheckBeforeChangeLocation(out strError);
            if (nState != 1)
            {
                DialogResult result = MessageBox.Show(this,
                    "在操作前，" + strError + "。\r\n\r\n确实要继续? ",
                    this.FormCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
                if (nState == 2)
                {
                    result = MessageBox.Show(this,
        "是否要转为对这些在借状态的图书执行还书操作? \r\n\r\n[是] 转为还书操作; [否] 继续进行修改馆藏地的操作\r\n\r\n注意：如果转为进行还书操作，则修改馆藏地的操作会被取消。请等待还书操作完成后，再重新执行修改馆藏地的操作",
        this.FormCaption,
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        nRet = DoReturn(items,
            out strError);
                        if (nRet == -1)
                            return -1;
                        MessageBox.Show(this.MainForm, "修改馆藏地的操作已被取消。请等待还书操作完全完成后，再重新执行修改馆藏地的操作。\r\n\r\n注: 还书操作完成后，如果有红色的事项，需要专门处理");
                        return 0;
                    }
                }
            }

            // 先检查是不是已经有修改的。提醒，如果继续操作，那些修改也会一并保存
            int nChangedCount = GetChangedCount(items);
            if (nChangedCount > 0)
            {
                DialogResult result = MessageBox.Show(this,
                    "在操作前，所选定的范围内有 " + nChangedCount.ToString() + " 个册信息被修改后尚未保存。若继续操作，这些修改会被一并保存兑现。\r\n\r\n确实要继续? ",
                    this.FormCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.No)
                    return 0;
            }

            GetLocationDialog dlg = new GetLocationDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ItemLocation = ListViewUtil.GetItemText(items[0], COLUMN_LOCATION); // 对话框中出现第一行的馆藏地作为参考

            dlg.GetLocationValueTable -= new GetValueTableEventHandler(dlg_GetLocationValueTable);
            dlg.GetLocationValueTable += new GetValueTableEventHandler(dlg_GetLocationValueTable);

            dlg.RefDbName = "";
            /*
                dlg.RefDbName = EntityForm.GetDbName(this.entityEditControl1.RecPath);
             * */
            dlg.MainForm = this.MainForm;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            nChangedCount = ChangeLocation(items, dlg.ItemLocation);

            ListViewUtil.ClearSelection(this.listView_in);  // 清除全部选择标志

            int nSavedCount = 0;
            // return:
            //      -1  出错
            //      0   成功
            //      1   中断
            nRet = SaveItemRecords(
                items,  // 保存范围可能比本次clear的稍大
                out nSavedCount,
                out strError);
            if (nRet == -1)
                return -1;
            else
            {
                // 刷新Origin的深浅间隔色
                if (this.SortColumns_in.Count > 0)
                {
                    SetGroupBackcolor(
                        this.listView_in,
                        this.SortColumns_in[0].No);
                }

                this.SetNextButtonEnable();

                if (nChangedCount > 0)
                    strError = "共修改 " + nChangedCount.ToString() + " 项，保存 " + nSavedCount.ToString() + " 项";
                else
                    strError = "没有发生修改和保存";
            }

            return 1;
        }

        // 在修改馆藏地点前，检查记录的情况
        // return:
        //      0   尚未装载任何数据    
        //      1   所有行符合条件
        //      2   其中有阻碍操作的事项
        int CheckBeforeChangeLocation(out string strError)
        {
            strError = "";

            int nBorrowCount = 0;
            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                ListViewItem item = this.listView_in.Items[i];

                if (item.ImageIndex == TYPE_ERROR)
                    continue;

                string strBorrower = ListViewUtil.GetItemText(item, COLUMN_BORROWER);
                if (string.IsNullOrEmpty(strBorrower) == false)
                    nBorrowCount++;
            }

            if (nBorrowCount != 0)
            {
                strError = "列表中有 " + nBorrowCount + " 个在借状态的行。这些行无法被修改馆藏地字段。请在操作前排除这些行，或者将它们执行还书手续后重新装载";
                return 2;
            }

            strError = "数据事项装载正确。";
            return 1;
        }

        // 对选定的行执行 清除“加工中”状态
        void menu_clearProccessingState_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_in.SelectedItems.Count == 0)
            {
                strError = "当前没有选定任何事项";
                goto ERROR1;
            }

            List<ListViewItem> items = GetSelectedItems(this.listView_in);

            // 先检查是不是已经有修改的。提醒，如果继续操作，那些修改也会一并保存
            int nChangedCount = GetChangedCount(items);
            if (nChangedCount > 0)
            {
                DialogResult result = MessageBox.Show(this,
                    "在操作前，所选定的范围内有 " + nChangedCount.ToString() + " 个册信息被修改后尚未保存。若继续操作，这些修改会被一并保存兑现。\r\n\r\n确实要继续? ",
                    this.FormCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.No)
                    return;
            }

            int nClearCount = ClearProcessingState(items);

            ListViewUtil.ClearSelection(this.listView_in);  // 清除全部选择标志

            int nSavedCount = 0;
            // return:
            //      -1  出错
            //      0   成功
            //      1   中断
            nRet = SaveItemRecords(
                items,  // 保存范围可能比本次clear的稍大
                out nSavedCount,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            else
            {
                // 刷新Origin的深浅间隔色
                if (this.SortColumns_in.Count > 0)
                {
                    SetGroupBackcolor(
                        this.listView_in,
                        this.SortColumns_in[0].No);
                }

                this.SetNextButtonEnable();

                if (nClearCount > 0)
                    MessageBox.Show(this, "共修改 " + nClearCount.ToString() + " 项，保存 " + nSavedCount.ToString() + " 项");
                else
                    MessageBox.Show(this, "没有发生修改和保存");
            }
            return;
        ERROR1:
            this.SetNextButtonEnable();
            MessageBox.Show(this, strError);
        }

        // 对选定的行执行 移动到目标库
        void menu_moveToTargetDb_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_in.Items.Count == 0)
            {
                strError = "当前没有任何事项可供操作";
                goto ERROR1;
            }

            if (this.comboBox_load_type.Text == "连续出版物")
            {
                strError = "不能对连续出版物进行转移到目标库的操作";
                goto ERROR1;
            }

            List<ListViewItem> items = GetSelectedItems(this.listView_in);

            // 先检查是不是已经有修改的。提醒，如果继续操作，那些修改也会一并保存
            int nChangedCount = GetChangedCount(items);
            if (nChangedCount > 0)
            {
                DialogResult result = MessageBox.Show(this,
                    "在操作前，所选定的范围内有 " + nChangedCount.ToString() + " 项册信息被修改后尚未保存。若继续操作，这些修改会被一并保存兑现。\r\n\r\n确实要继续? ",
                    this.FormCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.No)
                    return;
            }

            int nMovedCount = 0;
            // 转移
            // return:
            //      -1  出错。出错的时候，nMovedCount如果>0，表示已经转移的事项数
            //      0   成功
            //      1   中途放弃
            nRet = DoMove(
                items,
                out nMovedCount,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nMovedCount > 0)
                MessageBox.Show(this, "共转移 " + nMovedCount.ToString() + " 项");
            else
                MessageBox.Show(this, "没有发生转移");


            // 重新设置next和其他按钮的状态
            this.SetNextButtonEnable();
            return;
        ERROR1:
            this.SetNextButtonEnable();
            MessageBox.Show(this, strError);
        }


        // 清除 已验证 状态，对所选定的行
        void menu_clearVerifiedSelected_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView_in.SelectedItems)
            {
                if (item.ImageIndex == TYPE_VERIFIED)
                    SetItemColor(item, TYPE_NORMAL);
                // 注：对错误状态的行，不清除其状态
            }
            if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {
                SetVerifyPageNextButtonEnabled();
            }
        }

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;
            ListViewUtil.SelectAllLines(list);
        }

        void menu_refreshSelected_Click(object sender, EventArgs e)
        {
#if NO
            ListView list = (ListView)((MenuItem)sender).Tag;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in list.SelectedItems)
            {
                items.Add(item);

            }
            RefreshLines(items);

            if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {
                SetVerifyPageNextButtonEnabled();
            }
#endif
            ListView list = (ListView)((MenuItem)sender).Tag;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in list.SelectedItems)
            {
                items.Add(item);
            }
            RefreshLines(COLUMN_RECPATH,
                items,
                true,
                new string[] { "summary", "@isbnissn", "targetrecpath"});
            if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {
                SetVerifyPageNextButtonEnabled();
            }
        }

        void menu_refreshAll_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;

            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < list.Items.Count; i++)
            {
                items.Add(list.Items[i]);
            }
            // RefreshLines(items);
            RefreshLines(COLUMN_RECPATH,
                items,
                true,
                new string[] { "summary", "@isbnissn", "targetrecpath" });

            if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {
                SetVerifyPageNextButtonEnabled();
            }
        }

        // 移除集合内列表中已经选定的行
        void menu_deleteSelected_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;


            if (list.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要移除的事项。");
                return;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in list.SelectedItems)
            {
                items.Add(item);
            }

            DialogResult result = MessageBox.Show(this,
"确实要移除选定的 "+items.Count.ToString()+" 个事项?",
this.FormCaption,
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            DeleteLines(items);

            // if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {
                SetVerifyPageNextButtonEnabled();
            }
        }

        static void DeleteLines(List<ListViewItem> items)
        {
            if (items.Count == 0)
                return;
            ListView list = items[0].ListView;

            for (int i = 0; i < items.Count; i++)
            {
                list.Items.Remove(items[i]);
            }
        }

#if NO
        void RefreshLines(List<ListViewItem> items)
        {
            string strError = "";

            EnableControls(false);
            // MainForm.ShowProgress(true);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新 ...");
            stop.BeginLoop();

            try
            {
                stop.SetProgressRange(0, items.Count);
                // stop.SetProgressValue(0);


                for (int i = 0; i < items.Count; i++)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断1";
                        goto ERROR1;
                    }


                    ListViewItem item = items[i];

                    stop.SetMessage("正在刷新 " + item.Text + " ...");

                    int nRet = RefreshOneItem(item, out strError);
                    /*
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                     * */

                    stop.SetProgressValue(i);
                }

                stop.SetProgressValue(items.Count);

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("刷新完成。");
                stop.HideProgress();

                EnableControls(true);
                // MainForm.ShowProgress(false);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }
#endif

#if NO
        public int RefreshOneItem(ListViewItem item,
            out string strError)
        {
            strError = "";

            string strItemXml = "";
            string strBiblioText = "";

            string strItemRecPath = "";
            string strBiblioRecPath = "";

            byte[] item_timestamp = null;

            // string strBarcode = item.Text;

            // 2007/5/11 new changed
            string strBarcode = "@path:" + item.SubItems[COLUMN_RECPATH].Text;

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
            if (lRet == -1 || lRet == 0)
            {
                ListViewUtil.ChangeItemText(item,
                    COLUMN_ERRORINFO,
                    strError);
                SetItemColor(item, TYPE_ERROR);

                // 设置timestamp/xml
                OriginItemData data = (OriginItemData)item.Tag;
                Debug.Assert(data != null, "");
                data.Timestamp = item_timestamp;
                data.Xml = strItemXml;
                data.Changed = false;

                goto ERROR1;
            }

            string strBiblioSummary = "";
            string strISBnISSN = "";
            string strTargetRecPath = "";

            if (strBiblioSummary == "")
            {
                /*
                lRet = Channel.GetBiblioSummary(
                    stop,
                    strBarcode,
                    "",
                    "",
                    out strBiblioRecPath,
                    out strBiblioSummary,
                    out strError);
                if (lRet == -1)
                {
                    strBiblioSummary = "获得书目摘要时发生错误: " + strError;
                }
                 * */
                /*
                lRet = Channel.GetBiblioSummary(
                    stop,
                    strBarcode,
                    "",
                    "",
                    out strBiblioRecPath,
                    out strBiblioSummary,
                    out strError);
                if (lRet == -1)
                {
                    strBiblioSummary = "获得书目摘要时发生错误: " + strError;
                }*/
                string[] formats = new string[3];
                formats[0] = "summary";
                formats[1] = "@isbnissn";
                formats[2] = "targetrecpath";
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
                    Debug.Assert(results != null && results.Length == 3, "results必须包含2个元素");
                    strBiblioSummary = results[0];
                    strISBnISSN = results[1];
                    strTargetRecPath = results[2];
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
                strError = ex.Message;
                goto ERROR1;
            }


            {
                SetListViewItemText(dom,
                    item_timestamp,
                    true,
                    strItemRecPath,
                    strBiblioSummary,
                    strISBnISSN,
                    strBiblioRecPath,
                    strTargetRecPath,
                    item);
            }

            {

                // 设置timestamp/xml
                OriginItemData data = (OriginItemData)item.Tag;
                Debug.Assert(data != null, "");
                data.Timestamp = item_timestamp;
                data.Xml = strItemXml;
                data.Changed = false;

            }

            // 图标
            // item.ImageIndex = TYPE_NORMAL;

            // 如果是TYPE_CHECKED，则保持不变
            // 2009/11/23 changed
            if (item.ImageIndex == TYPE_ERROR)
                SetItemColor(item, TYPE_NORMAL);

            return 1;
        ERROR1:
            return -1;
        }
#endif

        // out_of 列表上的右鼠标键popup菜单
        private void listView_outof_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Tag = this.listView_outof;
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新选定的行(&S)");
            menuItem.Tag = this.listView_outof;
            menuItem.Click += new System.EventHandler(this.menu_refreshSelected_Click);
            if (this.listView_outof.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新全部行(&R)");
            menuItem.Tag = this.listView_outof;
            menuItem.Click += new System.EventHandler(this.menu_refreshAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("移除(&D)");
            menuItem.Tag = this.listView_outof;
            menuItem.Click += new System.EventHandler(this.menu_deleteSelected_Click);
            if (this.listView_outof.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_outof, new Point(e.X, e.Y));		
        }

        private void ItemHandoverForm_Activated(object sender, EventArgs e)
        {
            // 2009/8/13
            this.MainForm.stopManager.Active(this.stop);

        }

        // 转移
        // return:
        //      -1  出错。出错的时候，nMovedCount如果>0，表示已经转移的事项数
        //      0   成功
        //      1   中途放弃
        int DoMove(
            List<ListViewItem> items,
            out int nMovedCount,
            out string strError)
        {
            strError = "";
            int nErrorCount = 0;
            nMovedCount = 0;

            ListViewUtil.ClearSelection(this.listView_in);

            this.m_biblioRecPathTable.Clear();
            this.m_targetDbNameTable.Clear();

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行转移 ...");
            stop.BeginLoop();

            try
            {

                stop.SetProgressRange(0, this.listView_in.Items.Count);
                stop.SetProgressValue(0);

                for (int i = 0; i < items.Count; i++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断1";
                            return 1;
                        }
                    }


                    ListViewItem item = items[i];

                    /*
                    string strTargetRecPath = ListViewUtil.GetItemText(item, COLUMN_TARGETRECPATH);
                    if (String.IsNullOrEmpty(strTargetRecPath) == true)
                        continue;
                     * */

                    string strNewItemRecPath = "";
                    // return:
                    //      -1	出错
                    //      0	没有必要转移。说明文字在strError中返回
                    //      1	成功转移
                    //      2   canceled
                    int nRet = MoveOneRecord(item,
                        out strNewItemRecPath,
                        out strError);
                    if (nRet == -1)
                    {
                        ListViewUtil.ChangeItemText(item,
                            COLUMN_ERRORINFO,
                            strError);
                        SetItemColor(item, TYPE_ERROR);
                        item.EnsureVisible();
                        nErrorCount++;
                        continue;
                    }

                    if (nRet == 0)
                        continue;

                    if (nRet == 2)
                    {
                        strError = "用户中断2";
                        return 1;
                    }

                    if (String.IsNullOrEmpty(strNewItemRecPath) == false)
                        ListViewUtil.ChangeItemText(item,
                            COLUMN_RECPATH,
                            strNewItemRecPath);

                    // 转移后所从属的新书目记录应该不会再有目标
                    /*
                    ListViewUtil.ChangeItemText(item,
                        COLUMN_TARGETRECPATH,
                        "");
                     * */
                    // 通过刷新该行来自动实现，更可靠
                    // nRet = RefreshOneItem(item, out strError);

                    nMovedCount++;
                    item.Selected = true;

                    stop.SetProgressValue(i+1);
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



            if (nErrorCount > 0)
            {
                strError = "处理过程中发生 " + nErrorCount.ToString() + " 项错误，请看列表中红色背景的行的错误信息栏\r\n\r\n问题处理完以后，请注意使用上下文菜单命令刷新相关行，以便观察记录的最新状态";
                return -1;
            }
            else
            {
                // 2014/8/27
                // 有红色背景行的时候，不刷新全部行。
                // TODO: 小马建议此时只刷新不出错的行？
                RefreshLines(COLUMN_RECPATH,
        items,
        true,
        new string[] { "summary", "@isbnissn", "targetrecpath" });
            }

            return 0;
        }

        // 从现有listview事项中找出一个源书目库和目标书目库的对照关系
        string SearchExistRelation(string strSourceBiblioDbName)
        {
            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                ListViewItem item = this.listView_in.Items[i];
                string strSourceBiblioRecPath = ListViewUtil.GetItemText(item,
                    COLUMN_BIBLIORECPATH);
                if (String.IsNullOrEmpty(strSourceBiblioRecPath) == true)
                    continue;

                string strTempSourceBiblioDbName = Global.GetDbName(strSourceBiblioRecPath);

                if (String.IsNullOrEmpty(strTempSourceBiblioDbName) == true)
                    continue;

                if (strTempSourceBiblioDbName == strSourceBiblioDbName)
                {
                    string strTargetBiblioRecPath = ListViewUtil.GetItemText(item,
                        COLUMN_TARGETRECPATH);
                    if (String.IsNullOrEmpty(strTargetBiblioRecPath) == true)
                        continue;

                    string strTargetBiblioDbName = Global.GetDbName(strTargetBiblioRecPath);

                    if (String.IsNullOrEmpty(strTargetBiblioDbName) == true)
                        continue;

                    return strTargetBiblioDbName;
                }
            }

            return null;    // not found
        }

        string GetTargetBiblioDbName(string strSourceBiblioDbName)
        {
            string strTargetBiblioDbName = (string)this.m_targetDbNameTable[strSourceBiblioDbName];
            if (String.IsNullOrEmpty(strTargetBiblioDbName) == false)
                return strTargetBiblioDbName;

            SelectTargetBiblioDbNameDialog dlg = new SelectTargetBiblioDbNameDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MessageText = "请指定来自书目库 '" + strSourceBiblioDbName + "' 中的记录要转移去的目标书目库";
            dlg.SourceBiblioDbName = strSourceBiblioDbName;
            dlg.MainForm = this.MainForm;
            dlg.TargetBiblioDbName = SearchExistRelation(strSourceBiblioDbName);    // 从已有的事项中搜索出对照关系
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return null;    // 表示cancel

            this.m_targetDbNameTable[strSourceBiblioDbName] = dlg.TargetBiblioDbName;

            return dlg.TargetBiblioDbName;
        }

        // 复制书目记录内容
        // parameters:
        // return:
        //      -1  error
        //      0   canceled
        //      1   已经复制
        //      2   没有复制。因为两条记录内容相同，或者在对话框上点了“不覆盖”按钮
        //      3   没有复制。因为先前已经复制过了，或者处理过了
        int CopyOneBiblioRecord(string strSourceBiblioRecPath,
            string strTargetBiblioRecPath,
            out string strError)
        {
            strError = "";

            Debug.Assert(String.IsNullOrEmpty(strSourceBiblioRecPath) == false, "strSourceBiblioRecPath值不能为空");
            Debug.Assert(String.IsNullOrEmpty(strTargetBiblioRecPath) == false, "");

            object o = m_biblioRecPathTable[strSourceBiblioRecPath + " --> " + strTargetBiblioRecPath];
            if (o != null)
                return 3;

            // 先获得源书目记录
            string[] formats = new string[1];
            formats[0] = "xml";
            string[] results = null;
            byte[] timestamp = null;

            long lRet = Channel.GetBiblioInfos(
                stop,
                strSourceBiblioRecPath,
                "",
                formats,
                out results,
                out timestamp,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                {
                    strError = "书目记录 '" + strSourceBiblioRecPath + "' 不存在";
                    return -1;
                }

                return -1;
            }

            Debug.Assert(results != null && results.Length == 1, "results必须包含1个元素");
            string strSourceXml = results[0];   // 源书目数据

            string strSourceBiblioDbName = Global.GetDbName(strSourceBiblioRecPath);
            if (String.IsNullOrEmpty(strSourceBiblioDbName) == true)
            {
                strError = "从路径 '" + strSourceBiblioRecPath + "' 中获得数据库名时出错";
                return -1;
            }

            // 转换为MARC格式
            // TODO: 是否可以直接在XML上进行修改?
            string strSourceSyntax = this.MainForm.GetBiblioSyntax(strSourceBiblioDbName);
            string strOutMarcSyntax = "";
            string strSourceMarc = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            int nRet = MarcUtil.Xml2Marc(strSourceXml,
                true,
                strSourceSyntax,
                out strOutMarcSyntax,
                out strSourceMarc,
                out strError);
            if (nRet == -1)
            {
                strError = "将记录 '"+strTargetBiblioRecPath+"' 的XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            // 然后获得目标书目记录
            formats = new string[1];
            formats[0] = "xml";
            results = null;
            byte[] target_timestamp = null;

            lRet = Channel.GetBiblioInfos(
                stop,
                strTargetBiblioRecPath,
                "",
                formats,
                out results,
                out target_timestamp,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                {
                    strError = "书目记录 '" + strTargetBiblioRecPath + "' 不存在";
                    return -1;
                }

                return -1;
            }

            Debug.Assert(results != null && results.Length == 1, "results必须包含1个元素");
            string strTargetXml = results[0];   // 目标书目数据

            string strTargetBiblioDbName = Global.GetDbName(strTargetBiblioRecPath);
            if (String.IsNullOrEmpty(strTargetBiblioDbName) == true)
            {
                strError = "从路径 '"+strTargetBiblioRecPath+"' 中获得数据库名时出错";
                return -1;
            }

            // 转换为MARC格式
            string strTargetSyntax = this.MainForm.GetBiblioSyntax(strTargetBiblioDbName);
            string strTargetMarc = "";

            if (strSourceSyntax.ToLower() != strTargetSyntax.ToLower())
            {
                strError = "源书目记录 '"+strSourceBiblioRecPath+"' 所在库的格式 '"+strSourceSyntax+"' 和 目标数据记录 '"+strTargetBiblioRecPath+"' 所在库的格式 '"+strTargetSyntax+"' 不同";
                return -1;
            }

            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            nRet = MarcUtil.Xml2Marc(strTargetXml,
                true,
                strTargetSyntax,
                out strOutMarcSyntax,
                out strTargetMarc,
                out strError);
            if (nRet == -1)
            {
                strError = "将记录 '"+strTargetBiblioRecPath+"' 的XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            // 将两个MARC记录去掉一些非要害的字段，例如001-005/801/905/998以后进行比较，看看是不是一样
            // 如果不一样，则需要弹出对话框让操作者进行评估和选择
            // return:
            //      -1  出错
            //      0   一致
            //      1   不一致
            nRet = CompareTwoMarc(
                strTargetSyntax,
                strSourceMarc,
                strTargetMarc,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                m_biblioRecPathTable[strSourceBiblioRecPath + " --> " + strTargetBiblioRecPath] = true;
                return 2;
            }

            // 保留原来的998字段内容
            string strField998 = "";
            string strNextFieldName = "";
            nRet = MarcUtil.GetField(strTargetMarc,
                "998",
                0,
                out strField998,
                out strNextFieldName);

            TwoBiblioDialog dlg = new TwoBiblioDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "转移书目记录";
            dlg.MessageText = "书目源记录和目标记录内容不同。\r\n\r\n请问是否要用源记录覆盖目标记录?";
            dlg.LabelSourceText = "源 " + strSourceBiblioRecPath;
            dlg.LabelTargetText = "目标 " + strTargetBiblioRecPath;
            dlg.MarcSource = strSourceMarc;
            dlg.MarcTarget = strTargetMarc;
            dlg.ReadOnlyTarget = true;   // 目标MARC编辑器不让进行修改。其实也可以让修改？比如仅仅从左边复制一点东西过来？
            // dlg.StartPosition = FormStartPosition.CenterScreen;

            this.MainForm.AppInfo.LinkFormState(dlg, "TwoBiblioDialog_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult == DialogResult.Cancel)
                return 0;   // 全部放弃

            if (dlg.DialogResult == DialogResult.No)
            {
                m_biblioRecPathTable[strSourceBiblioRecPath + " --> " + strTargetBiblioRecPath] = true;
                return 2;
            }

            string strFinalMarc = "";

            if (dlg.EditTarget == false)
                strFinalMarc = dlg.MarcSource;
            else
                strFinalMarc = dlg.MarcTarget;

            // 还原原来目标记录中的998字段内容？
            MarcUtil.ReplaceField(ref strFinalMarc,
                "998",
                0,
                strField998);

            // 转换回XML
            XmlDocument domMarc = null;
            nRet = MarcUtil.Marc2Xml(strFinalMarc,
                strSourceSyntax,
                out domMarc,
                out strError);
            if (nRet == -1)
                return -1;

            XmlDocument domTarget = new XmlDocument();
            try
            {
                domTarget.LoadXml(strTargetXml);
            }
            catch (Exception ex)
            {
                strError = "target biblio record XML load to DOM error: " + ex.Message;
                return -1;
            }

            // 需要还原原来target记录中的<dprms:file>元素？
            // 而保留原记录的856字段等，是操作者的责任
            // parameters:
            //      source  存储有<dprms:file>元素的DOM
            //      marc    存储有MARC结构元素的DOM
            nRet = MergeTwoXml(
                strSourceSyntax,
                ref domTarget,
                domMarc,
                out strError);
            if (nRet == -1)
                return -1;

            byte[] baNewTimestamp = null;
            string strOutputPath = "";
            lRet = Channel.SetBiblioInfo(
                stop,
                "change",
                strTargetBiblioRecPath,
                "xml",
                domTarget.DocumentElement.OuterXml,
                target_timestamp,
                "",
                out strOutputPath,
                out baNewTimestamp,
                out strError);
            if (lRet == -1)
            {
                strError = "修改书目记录 '" + strTargetBiblioRecPath + "' 时出错: " + strError;
                return -1;
            }

            m_biblioRecPathTable[strSourceBiblioRecPath + " --> " + strTargetBiblioRecPath] = true;
            return 1;
        }

        // 比较前需要删除的UNIMARC字段
        static string[] deleting_unimarc_fieldnames = new string[] {
                "-01", 
                "001", 
                "005", 
                "801", 
                "905", 
                "998", 
        };

        // 比较前需要删除的USMARC字段
        static string[] deleting_usmarc_fieldnames = new string[] {
                "-01", 
                "001", 
                "005", 
                "801", 
                "905", 
                "998", 
        };

        static void DeleteField(ref string strMARC,
            string strFieldName)
        {
            string strField = "";
            string strNextFieldName = "";

            while (true)
            {
                // return:
                //		-1	出错
                //		0	所指定的字段没有找到
                //		1	找到。找到的字段返回在strField参数中
                int nRet = MarcUtil.GetField(strMARC,
                    strFieldName,
                    0,
                    out strField,
                    out strNextFieldName);
                if (nRet == 0 || nRet == -1)
                    break;
                MarcUtil.ReplaceField(ref strMARC,
                    strFieldName,
                    0,
                    null);
            }

        }
        // 比较两条MARC记录，看看是否有本质性的差异
        // 将两个MARC记录去掉一些非要害的字段，例如001-005/801/905/998以后进行比较，看看是不是一样
        // return:
        //      -1  出错
        //      0   一致
        //      1   不一致
        static int CompareTwoMarc(
            string strSyntax,
            string strMARC1,
            string strMARC2,
            out string strError)
        {
            strError = "";

            string[] deleting_fieldnames = null;

            if (strSyntax.ToLower() == "unimarc")
            {
                deleting_fieldnames = deleting_unimarc_fieldnames;
            }
            else if (strSyntax.ToLower() == "usmarc")
            {
                deleting_fieldnames = deleting_unimarc_fieldnames;
            }
            else
            {
                strError = "未知的MARC格式 '" + strSyntax + "'";
                return -1;
            }

            for (int i = 0; i < deleting_fieldnames.Length; i++)
            {
                DeleteField(ref strMARC1,
                    deleting_fieldnames[i]);
                DeleteField(ref strMARC2,
                    deleting_fieldnames[i]);
            }

            string strContent1 = "";
            string strContent2 = "";

            if (strMARC1.Length > 24)
                strContent1 = strMARC1.Substring(24);

            if (strMARC2.Length > 24)
                strContent2 = strMARC2.Substring(24);

            if (String.Compare(strContent1, strContent2) != 0)
                return 1;

            return 0;
        }


        // 转移书目记录
        // parameters:
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        int MoveOneBiblioRecord(string strSourceBiblioRecPath,
            out string strTargetBiblioRecPath,
            out string strError)
        {
            strError = "";
            strTargetBiblioRecPath = "";

            Debug.Assert(String.IsNullOrEmpty(strSourceBiblioRecPath) == false, "");

            string strSourceBiblioDbName = Global.GetDbName(strSourceBiblioRecPath);

            string strTargetBiblioDbName = GetTargetBiblioDbName(strSourceBiblioDbName);
            if (String.IsNullOrEmpty(strTargetBiblioDbName) == true)
                return 0;   // canceled

            // 先获得书目记录
            string[] formats = new string[1];
            formats[0] = "xml";
            string[] results = null;
            byte[] timestamp = null;

            Debug.Assert(String.IsNullOrEmpty(strSourceBiblioRecPath) == false, "strSourceBiblioRecPath值不能为空");

            long lRet = Channel.GetBiblioInfos(
                stop,
                strSourceBiblioRecPath,
                "",
                formats,
                out results,
                out timestamp,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                {
                    strError = "书目记录 '" + strSourceBiblioRecPath + "' 不存在";
                    return -1;
                }

                return -1;
            }

            Debug.Assert(results != null && results.Length == 1, "results必须包含1个元素");
            string strSourceXml = results[0];   // 源书目数据

            // 转换为MARC格式
            // TODO: 是否可以直接在XML上进行修改?
            string strSourceSyntax = this.MainForm.GetBiblioSyntax(strSourceBiblioDbName);
            string strOutMarcSyntax = "";
            string strMarc = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            int nRet = MarcUtil.Xml2Marc(strSourceXml,
                true,
                strSourceSyntax,
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            // 创建目标书目记录
            // 直接将没有998$t的MARC记录用于创建
            // TODO: 为了保险，是否特意删除一下998$t?
            strTargetBiblioRecPath = strTargetBiblioDbName + "/?";
            string strOutputPath = "";
            byte[] baNewTimestamp = null;
            string strOutputBiblio = "";
            lRet = Channel.CopyBiblioInfo(
                stop,
                "onlycopybiblio",   // 只复制书目记录，不复制下属的实体记录等
                strSourceBiblioRecPath,
                "xml",
                null,   // strSourceXml,
                timestamp,
                strTargetBiblioRecPath,
                null,   // strSourceXml,
                        "",
                        out strOutputBiblio,
                out strOutputPath,
                out baNewTimestamp,
                out strError);

            /*
            lRet = Channel.SetBiblioInfo(
                stop,
                "new",
                strTargetBiblioRecPath,
                "xml",
                strSourceXml,
                null,
                out strOutputPath,
                out baNewTimestamp,
                out strError);
             * */
            if (lRet == -1)
            {
                strError = "创建书目记录 '" + strTargetBiblioRecPath + "' 时出错: " + strError;
                return -1;
            }

            strTargetBiblioRecPath = strOutputPath;

            // 修改源书目记录
            // 修改/增补998$t
            string strField = "";
            string strNextFieldName = "";
            nRet = MarcUtil.GetField(strMarc,
                "998",
                0,
                out strField,
                out strNextFieldName);
            if (nRet == 0)
                strField = "998  ";

            MarcUtil.ReplaceSubfield(ref strField,
                "t",
                0,
                "t" + strTargetBiblioRecPath);
            MarcUtil.ReplaceField(ref strMarc,
                "998",
                0,
                strField);

            // 转换回XML
            XmlDocument domMarc = null;
            nRet = MarcUtil.Marc2Xml(strMarc,
                strSourceSyntax,
                out domMarc,
                out strError);
            if (nRet == -1)
                return -1;

            // 将根下的unimarc:元素全部删除，但是保留其它元素
            XmlDocument domSource = new XmlDocument();
            try
            {
                domSource.LoadXml(strSourceXml);
            }
            catch (Exception ex)
            {
                strError = "source biblio record XML load to DOM error: " + ex.Message;
                return -1;
            }

            nRet = MergeTwoXml(
                strSourceSyntax,
                ref domSource,
                domMarc,
                out strError);
            if (nRet == -1)
                return -1;

            // TODO: 以后看情况可以删除源书目记录?
            lRet = Channel.SetBiblioInfo(
                stop,
                "change",
                strSourceBiblioRecPath,
                "xml",
                domSource.DocumentElement.OuterXml,
                timestamp,
                "",
                out strOutputPath,
                out baNewTimestamp,
                out strError);
            if (lRet == -1)
            {
                strError = "修改书目记录 '" + strSourceBiblioRecPath + "' 时出错: " + strError;
                return -1;
            }

            return 1;
        }

        // parameters:
        //      source  存储有<dprms:file>元素的DOM
        //      marc    存储有MARC结构元素的DOM
        int MergeTwoXml(
            string strMarcSyntax,
            ref XmlDocument source,
            XmlDocument marc,
            out string strError)
        {
            strError = "";

            string strNamespaceURI = "";
            string strPrefix = "";
            if (strMarcSyntax == "unimarc")
            {
                strNamespaceURI = DpNs.unimarcxml;
                strPrefix = "unimarc";
            }
            else if (strMarcSyntax == "usmarc")
            {
                strNamespaceURI = Ns.usmarcxml;
                strPrefix = "usmarc";
            }
            else
            {
                strError = "未知的marcsyntax '" + strMarcSyntax + "'";
                return -1;
            }

            // 删除根元素下所有名字空间为unimarc或者usmarc的节点
            for (int i = 0; i < source.DocumentElement.ChildNodes.Count; i++)
            {
                XmlNode node = source.DocumentElement.ChildNodes[i];
                if (node.NamespaceURI == strNamespaceURI)
                {
                    source.DocumentElement.RemoveChild(node);
                    i--;
                }
            }

            // 插入marc节点
            for (int i = 0; i < marc.DocumentElement.ChildNodes.Count; i++)
            {
                XmlNode source_node = marc.DocumentElement.ChildNodes[i];
                if (source_node.NodeType != XmlNodeType.Element)
                    continue;
                if (source_node.NamespaceURI != strNamespaceURI)
                    continue;

                XmlNode new_node = source.CreateElement(strPrefix, 
                    source_node.LocalName,
                    strNamespaceURI);
                source.DocumentElement.AppendChild(new_node);
                DomUtil.SetElementOuterXml(new_node, source_node.OuterXml);
            }

            

            return 0;
        }

        // 移动一条册记录
        // 注意：调用本函数前，需要在外围准备好stop
        // parameters:
        //      strNewItemRecPath   移动后新的实体记录路径
        // return:
        //      -1	出错
        //      0	没有必要转移。说明文字在strError中返回
        //      1	成功转移
        //      2   canceled
        int MoveOneRecord(ListViewItem item,
            out string strNewItemRecPath,
            out string strError)
        {
            strError = "";
            strNewItemRecPath = "";
            long lRet = 0;
            int nRet = 0;

            string strItemRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

            string strTargetBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_TARGETRECPATH);

            string strSourceBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);
            string strSourceBiblioDbName = Global.GetDbName(strSourceBiblioRecPath);

            // 如果没有目标书目记录，表示需要新创建目标书目记录
            if (String.IsNullOrEmpty(strTargetBiblioRecPath) == true)
            {
                // 看看源书目库是不是属于工作库
                if (this.MainForm.IsOrderWorkDb(strSourceBiblioDbName) == false)
                {
                    strError = "记录 '" + strItemRecPath + "' 因其从属的书目记录不具备目标记录，并且不是采购工作库角色，不必转移";
                    return 0;
                }

                // 转移(创建)书目记录
                // parameters:
                // return:
                //      -1  error
                //      0   canceled
                //      1   succeed
                nRet = MoveOneBiblioRecord(strSourceBiblioRecPath,
                    out strTargetBiblioRecPath,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    return 2;

                // 修改列表中所有从属于此源书目记录的行的“目标记录路径”列
                for (int i = 0; i < this.listView_in.Items.Count; i++)
                {
                    ListViewItem temp_item = this.listView_in.Items[i];
                    string strBiblioRecPath = ListViewUtil.GetItemText(temp_item, COLUMN_BIBLIORECPATH);
                    if (strBiblioRecPath == strSourceBiblioRecPath)
                    {
                        /*
                        ListViewUtil.ChangeItemText(temp_item, 
                            COLUMN_BIBLIORECPATH,
                            strTargetBiblioRecPath);
                         * */
                        ListViewUtil.ChangeItemText(temp_item,
                            COLUMN_TARGETRECPATH,
                            strTargetBiblioRecPath);
                    }
                }

                // 书目记录新增(某种意义的转移)成功后，目标书目记录路径就有了
            }
            else
            {
                // 已经有目标书目记录。
                // 需要看源书目记录和目标书目记录内容是否相同。
                // 如果内容不同，则需要询问是否要将源记录内容复制到目标记录

                // 复制书目记录内容
                // parameters:
                // return:
                //      -1  error
                //      0   canceled
                //      1   已经复制
                //      2   没有复制。因为两条记录内容相同
                nRet = CopyOneBiblioRecord(strSourceBiblioRecPath,
                    strTargetBiblioRecPath,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    // 放弃整个处理过程
                    return 2;
                }

                if (nRet == 2)
                {
                    // 仅仅是放弃了复制书目记录，而不是放弃整个处理过程
                }
            }

            /*
            // 为转移，重新获得一次册记录
            string strItemXml = "";
            string strBiblioText = "";

            string strOutputItemRecPath = "";
            string strSourceBiblioRecPath = "";

            byte[] item_timestamp = null;

            lRet = Channel.GetItemInfo(
                stop,
                "@path:" + strItemRecPath,
                "xml",
                out strItemXml,
                out strOutputItemRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strSourceBiblioRecPath,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                return -1;
            }
             * */

            OriginItemData data = (OriginItemData)item.Tag;
            if (data == null)
            {
                strError = "item.Tag == null";
                return -1;
            }
            Debug.Assert(data != null, "");

            string strItemXml = data.Xml;

            Debug.Assert(String.IsNullOrEmpty(strSourceBiblioRecPath) == false, "");

            byte[] item_timestamp = data.Timestamp;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "册记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            string strTargetBiblioDbName = Global.GetDbName(strTargetBiblioRecPath);


            bool bMove = false; // 是否需要移动册记录
            string strTargetItemDbName = "";
            if (strSourceBiblioDbName != strTargetBiblioDbName)
            {
                // 书目库发生了改变，才有必要移动。否则仅仅修改实体记录的<parent>即可
                bMove = true;
                strTargetItemDbName = MainForm.GetItemDbName(strTargetBiblioDbName);

                if (String.IsNullOrEmpty(strTargetItemDbName) == true)
                {
                    strError = "书目库 '" + strTargetBiblioDbName + "' 并没有从属的实体库定义。操作失败";
                    return -1;
                }
            }

            EntityInfo info = new EntityInfo();
            info.RefID = Guid.NewGuid().ToString();

            string strTargetBiblioRecID = Global.GetRecordID(strTargetBiblioRecPath);

            DomUtil.SetElementText(dom.DocumentElement,
                "parent", strTargetBiblioRecID);

            string strNewItemXml = dom.OuterXml;

            info.OldRecPath = strItemRecPath;
            if (bMove == false)
            {
                info.Action = "change";
                info.NewRecPath = strItemRecPath;
            }
            else
            {
                info.Action = "move";
                Debug.Assert(String.IsNullOrEmpty(strTargetItemDbName) == false, "");
                info.NewRecPath = strTargetItemDbName + "/?";  // 把实体记录移动到另一个实体库中，追加成一条新记录，而旧记录自动被删除
            }

            info.NewRecord = strNewItemXml;
            info.NewTimestamp = null;

            info.OldRecord = strItemXml;
            info.OldTimestamp = item_timestamp;

            // 
            EntityInfo[] entities = new EntityInfo[1];
            entities[0] = info;

            EntityInfo[] errorinfos = null;

            lRet = Channel.SetEntities(
                stop,
                strTargetBiblioRecPath,
                entities,
                out errorinfos,
                out strError);
            if (lRet == -1)
                return -1;
            
            if (errorinfos != null && errorinfos.Length > 0)
            {
                int nErrorCount = 0;
                for (int i = 0; i < errorinfos.Length; i++)
                {
                    EntityInfo error = errorinfos[i];
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        if (String.IsNullOrEmpty(strError) == false)
                            strError += "; ";
                        strError += errorinfos[0].ErrorInfo;
                        nErrorCount++;
                    }
                    else
                    {
                        strNewItemRecPath = error.NewRecPath;
                    }
                }
                if (nErrorCount > 0)
                {
                    return -1;
                }
            }

            return 1;
        }

        private void button_move_moveAll_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_in.Items.Count == 0)
            {
                strError = "当前没有任何事项可供操作";
                goto ERROR1;
            }
            if (this.comboBox_load_type.Text == "连续出版物")
            {
                strError = "不能对连续出版物进行转移到目标库的操作";
                goto ERROR1;
            }

            List<ListViewItem> items = GetAllItems(this.listView_in);

            // 先检查是不是已经有修改的。提醒，如果继续操作，那些修改也会一并保存
            int nChangedCount = GetChangedCount(items);
            if (nChangedCount > 0)
            {
                DialogResult result = MessageBox.Show(this,
                    "在操作前，当前窗口内有 " + nChangedCount.ToString() + " 项册信息被修改后尚未保存。若继续操作，这些修改会被一并保存兑现。\r\n\r\n确实要继续? ",
                    this.FormCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.No)
                    return;
            }

            int nMovedCount = 0;
            // 转移
            // return:
            //      -1  出错。出错的时候，nMovedCount如果>0，表示已经转移的事项数
            //      0   成功
            //      1   中途放弃
            nRet = DoMove(
                items,
                out nMovedCount,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nMovedCount > 0)
                MessageBox.Show(this, "共转移 " + nMovedCount.ToString() + " 项");
            else
                MessageBox.Show(this, "没有发生转移");

            // 重新设置next和其他按钮的状态
            this.SetNextButtonEnable();
            return;
        ERROR1:
            this.SetNextButtonEnable();
            MessageBox.Show(this, strError);
        }

        // 清除“加工中”状态
        private void button_move_changeStateAll_Click(object sender, EventArgs e)
        {
            // 组织成批保存 SaveItemRecords
            string strError = "";
            int nRet = 0;

            if (this.listView_in.Items.Count == 0)
            {
                strError = "当前没有任何事项可供操作";
                goto ERROR1;
            }

            List<ListViewItem> items = GetAllItems(this.listView_in);

            // 先检查是不是已经有修改的。提醒，如果继续操作，那些修改也会一并保存
            int nChangedCount = GetChangedCount(items);
            if (nChangedCount > 0)
            {
                DialogResult result = MessageBox.Show(this,
                    "在操作前，当前窗口内有 " + nChangedCount.ToString() + " 项册信息被修改后尚未保存。若继续操作，这些修改会被一并保存兑现。\r\n\r\n确实要继续? ",
                    this.FormCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.No)
                    return;
            }

            int nClearCount = ClearProcessingState(items);

            ListViewUtil.ClearSelection(this.listView_in);  // 清除全部选择标志
            
            // TODO: 记忆下所有的书目记录路径，然后去重。对这些书目记录通知推荐者 


            int nSavedCount = 0;
            // return:
            //      -1  出错
            //      0   成功
            //      1   中断
            nRet = SaveItemRecords(
                items,  // 保存范围可能比本次clear的稍大
                out nSavedCount,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            else
            {
                // 刷新Origin的深浅间隔色
                if (this.SortColumns_in.Count > 0)
                {
                    SetGroupBackcolor(
                        this.listView_in,
                        this.SortColumns_in[0].No);
                }

                this.SetNextButtonEnable();

                if (nClearCount > 0)
                    MessageBox.Show(this, "共修改 " + nClearCount.ToString() + " 项，保存 " + nSavedCount.ToString() + " 项");
                else
                    MessageBox.Show(this, "没有发生修改和保存");
            }
            return;
        ERROR1:
            this.SetNextButtonEnable();
            MessageBox.Show(this, strError);
        }

        // 设置listview item的changed状态
        // TODO: 是否需要设置一个指示那些部分被修改的说明性字符串?
        static void SetItemChanged(ListViewItem item,
            bool bChanged)
        {
            OriginItemData data = (OriginItemData)item.Tag;
            if (data == null)
            {
                data = new OriginItemData();
                item.Tag = data;
            }

            data.Changed = bChanged;

            /*
            if (item.ImageIndex == TYPE_ERROR)
            {
                // 如果本来就是TYPE_ERROR，则不修改颜色
            }
            else if (item.ImageIndex == TYPE_NORMAL)
            {
                if (bChanged == true)
                    SetItemColor(item, TYPE_CHANGED);    // TODO: 这里会修改深浅间隔的颜色。是否需要重刷一遍颜色?
            }
            if (item.ImageIndex == TYPE_CHANGED)
            {
                if (bChanged == false)
                    SetItemColor(item, TYPE_NORMAL);    // TODO: 这里会修改深浅间隔的颜色。是否需要重刷一遍颜色?
            }
             * */
        }

        // return:
        //      本次修改了状态的事项个数
        int ChangeLocation(List<ListViewItem> items,
            string strNewLocation)
        {
            int nChangedCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strLocation = ListViewUtil.GetItemText(item, COLUMN_LOCATION);
                if (strLocation != strNewLocation)
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_LOCATION, strNewLocation);
                    SetItemChanged(item, true);
                    nChangedCount++;
                }
            }

            return nChangedCount;
        }


        // return:
        //      本次修改了状态的事项个数
        int ClearProcessingState(List<ListViewItem> items)
        {
            int nChangedCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strState = ListViewUtil.GetItemText(item, COLUMN_STATE);
                string strNewState = Global.RemoveStateProcessing(strState);

                if (strState != strNewState)
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_STATE, strNewState);
                    SetItemChanged(item, true);
                    nChangedCount++;
                }
            }

            return nChangedCount;
        }

        // 保存对原始实体记录的修改
        // return:
        //      -1  出错
        //      0   成功
        //      1   中断
        int SaveItemRecords(
            List<ListViewItem> items,
            out int nSavedCount,
            out string strError)
        {
            strError = "";
            nSavedCount = 0;
            int nRet = 0;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存实体记录 ...");
            stop.BeginLoop();

            try
            {
                string strPrevBiblioRecPath = "";
                List<EntityInfo> entity_list = new List<EntityInfo>();
                for (int i = 0; i < items.Count; i++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断1";
                            return 1;
                        }
                    }

                    ListViewItem item = items[i];

                    if (item.ImageIndex == TYPE_ERROR)
                    {
                        strError = "原始数据列表中，第 " + (i + 1).ToString() + " 个事项为错误状态。需要先排除问题才能进行保存。";
                        return -1;
                    }

                    OriginItemData data = (OriginItemData)item.Tag;
                    if (data == null)
                    {
                        Debug.Assert(false, "");
                        continue;
                    }
                    if (data.Changed == false)
                        continue;

                    item.Selected = true;   // 对要操作的事项加上选择标志
                    nSavedCount++;

                    // Debug.Assert(item.ImageIndex != TYPE_NORMAL, "data.Changed状态为true的事项，ImageIndex不应为TYPE_NORMAL");

                    string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

                    if (strBiblioRecPath != strPrevBiblioRecPath
                        && entity_list.Count > 0)
                    {
                        // 保存一个批次
                        nRet = SaveOneBatchOrders(entity_list,
                            strPrevBiblioRecPath,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        entity_list.Clear();
                    }

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(data.Xml);
                    }
                    catch (Exception ex)
                    {
                        strError = "item record XML装载到DOM时发生错误: " + ex.Message;
                        return -1;
                    }

                    DomUtil.SetElementText(dom.DocumentElement,
                        "state", ListViewUtil.GetItemText(item, COLUMN_STATE));

                    // 馆藏地点 2014/9/3
                    DomUtil.SetElementText(dom.DocumentElement,
                        "location", ListViewUtil.GetItemText(item, COLUMN_LOCATION));

                    // TODO: 还要保存<operations>元素

                    EntityInfo info = new EntityInfo();

                    if (String.IsNullOrEmpty(data.RefID) == true)
                    {
                        data.RefID = Guid.NewGuid().ToString();
                    }

                    info.RefID = data.RefID;
                    info.Action = "change";
                    info.OldRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
                    info.NewRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH); ;

                    info.NewRecord = dom.OuterXml;
                    info.NewTimestamp = null;

                    info.OldRecord = data.Xml;
                    info.OldTimestamp = data.Timestamp;

                    entity_list.Add(info);

                    strPrevBiblioRecPath = strBiblioRecPath;
                }

                // 最后一个批次
                if (String.IsNullOrEmpty(strPrevBiblioRecPath) == false
                        && entity_list.Count > 0)
                {
                    // 保存一个批次
                    nRet = SaveOneBatchOrders(entity_list,
                        strPrevBiblioRecPath,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    entity_list.Clear();
                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            return 0;
        }

        int SaveOneBatchOrders(List<EntityInfo> entity_list,
            string strBiblioRecPath,
            out string strError)
        {
            strError = "";

            EntityInfo[] entities = new EntityInfo[entity_list.Count];
            entity_list.CopyTo(entities);

            EntityInfo[] errorinfos = null;
            long lRet = Channel.SetEntities(
                stop,
                strBiblioRecPath,
                entities,
                out errorinfos,
                out strError);
            if (lRet == -1)
            {
                return -1;
            }

            string strErrorText = "";
            for (int i = 0; i < errorinfos.Length; i++)
            {
                EntityInfo errorinfo = errorinfos[i];

                Debug.Assert(String.IsNullOrEmpty(errorinfo.RefID) == false, "");

                ListViewItem item = null;
                OriginItemData data = FindDataByRefID(errorinfo.RefID, out item);
                if (data == null)
                {
                    strError = "RefID '" + errorinfo.RefID + "' 居然在原始数据列表中找不到对应的事项";
                    return -1;
                }

                // 正常信息处理
                if (errorinfo.ErrorCode == ErrorCodeValue.NoError)
                {
                    data.Timestamp = errorinfo.NewTimestamp;    // 刷新timestamp，以便后面发生修改后继续保存
                    data.Changed = false;
                    Debug.Assert(String.IsNullOrEmpty(errorinfo.NewRecord) == false, "");
                    data.Xml = errorinfo.NewRecord;
                    if (item.ImageIndex != TYPE_VERIFIED)   // 2012/3/19
                        SetItemColor(item, TYPE_NORMAL);
                    continue;
                }

                if (errorinfos[0].ErrorCode == ErrorCodeValue.TimestampMismatch)
                {
                    // 时间戳冲突
                    if (String.IsNullOrEmpty(strErrorText) == false)
                        strErrorText += ";\r\n";
                    strErrorText += "事项 '" + ListViewUtil.GetItemText(item, COLUMN_RECPATH) + "' 在保存过程中出现时间戳冲突。请重新装载原始数据，然后进行修改和保存。";
                }
                else
                {
                    // 其他错误
                    if (String.IsNullOrEmpty(strErrorText) == false)
                        strErrorText += ";\r\n";
                    strErrorText += "事项 '" + ListViewUtil.GetItemText(item, COLUMN_RECPATH) + "' 在保存过程中发生错误: " + errorinfo.ErrorInfo;
                }

                ListViewUtil.ChangeItemText(item, 
                    COLUMN_ERRORINFO,
                    errorinfo.ErrorInfo);
                SetItemColor(item, TYPE_ERROR);
            }

            if (String.IsNullOrEmpty(strErrorText) == false)
            {
                strError = strErrorText;
                return -1;
            }

            return 0;
        }

        // 根据refid定位到ListViewItem的Tag对象
        OriginItemData FindDataByRefID(string strRefID,
            out ListViewItem item)
        {
            item = null;
            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                item = this.listView_in.Items[i];
                OriginItemData data = (OriginItemData)item.Tag;
                if (data == null)
                    continue;
                if (data.RefID == strRefID)
                    return data;
            }

            item = null;
            return null;
        }

        // 统计出指定范围内事项中Changed==true的个数
        static int GetChangedCount(List<ListViewItem> items)
        {
            int nCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                OriginItemData data = (OriginItemData)items[i].Tag;
                if (data == null)   // 2015/9/15
                    continue;
                if (data.Changed == true)
                    nCount ++;
            }

            return nCount;
        }

        // 是否有事项改变而未保存?
        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                for (int i = 0; i < this.listView_in.Items.Count; i++)
                {
                    OriginItemData data = (OriginItemData)this.listView_in.Items[i].Tag;
                    if (data != null && data.Changed == true)
                        return true;
                }

                return false;
            }
        }

        private void button_move_notifyReader_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_in.Items.Count == 0)
            {
                strError = "当前没有任何事项可供操作";
                goto ERROR1;
            }

            int nNotifiedCount = 0;
            List<ListViewItem> items = GetAllItems(this.listView_in);
            nRet = NotifyReader(
                items,
                out nNotifiedCount,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "成功通知书目记录 "+nNotifiedCount+" 个");
            return;
        ERROR1:
            this.SetNextButtonEnable();
            MessageBox.Show(this, strError);
        }

        // 一条书目记录下的若干馆代码
        // 从册记录中的馆藏地点字段搜集汇总而来
        class OneBiblio
        {
            public List<string> LibrayCodeList = new List<string>();
        }

        // TODO: 可以建立一个hashtable，里面有已经通知过的书目记录路径，如果重复通知，会警告
        int NotifyReader(
    List<ListViewItem> items,
    out int nNotifiedCount,
    out string strError)
        {
            strError = "";
            nNotifiedCount = 0;
            // int nRet = 0;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在通知推荐订购的读者 ...");
            stop.BeginLoop();

            try
            {

                Hashtable biblio_table = new Hashtable();
                foreach (ListViewItem item in items)
                {
                    string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);
                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                        continue;

                    string strLocation = ListViewUtil.GetItemText(item, COLUMN_LOCATION);

                    strLocation = StringUtil.GetPureLocation(strLocation);
                    string strLibraryCode = Global.GetLibraryCode(strLocation);

                    OneBiblio biblio = (OneBiblio)biblio_table[strBiblioRecPath];
                    if (biblio == null)
                    {
                        biblio = new OneBiblio();
                        biblio_table[strBiblioRecPath] = biblio;
                    }

                    if (biblio.LibrayCodeList.IndexOf(strLibraryCode) == -1)
                        biblio.LibrayCodeList.Add(strLibraryCode);
                }

                if (biblio_table.Count == 0)
                    return 0;   // 没有任何需要通知的事项

                foreach (string strBiblioRecPath in biblio_table.Keys)
                {
                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断1";
                        return 1;
                    }

                    OneBiblio biblio = (OneBiblio)biblio_table[strBiblioRecPath];

                    Debug.Assert(biblio != null, "");

                    byte[] baNewTimestamp = null;
                    string strOutputPath = "";
                    long lRet = Channel.SetBiblioInfo(
                        stop,
                        "notifynewbook",
                        strBiblioRecPath,
                        StringUtil.MakePathList(biblio.LibrayCodeList),
                        "",
                        null,
                        "",
                        out strOutputPath,
                        out baNewTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "通知读者过程中处理书目记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                        return -1;
                    }

                    nNotifiedCount++;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            return 0;
        }

        // 从册记录路径文件装载
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
            nRet = LoadFromRecPathFile(dlg.FileName,
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

        delegate void Delegate_SetError(ListView list,
    ref ListViewItem item,
    string strBarcodeOrRecPath,
    string strError);

        internal override void SetError(ListView list,
    ref ListViewItem item,
    string strBarcodeOrRecPath,
    string strError)
        {
            // 确保线程安全 2014/9/3
            if (list != null && list.InvokeRequired)
            {
                Delegate_SetError d = new Delegate_SetError(SetError);
                object[] args = new object[4];
                args[0] = list;
                args[1] = item;
                args[2] = strBarcodeOrRecPath;
                args[3] = strError;
                this.Invoke(d, args);

                // 取出 ref 参数值
                item = (ListViewItem)args[1];
                return;
            }

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
#if DEBUG
            OriginItemData data = (OriginItemData)item.Tag;
            Debug.Assert(data != null, "");
#endif

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

        ScanBarcodeForm _scanBarcodeForm = null;

        // 装载方式 扫入册条码
        private void button_load_scanBarcode_Click(object sender, EventArgs e)
        {
            if (this._scanBarcodeForm == null)
            {
                this._scanBarcodeForm = new ScanBarcodeForm();
                MainForm.SetControlFont(this._scanBarcodeForm, this.Font, false);
                this._scanBarcodeForm.BarcodeScaned += new ScanedEventHandler(_scanBarcodeForm_BarcodeScaned);
                this._scanBarcodeForm.FormClosed += new FormClosedEventHandler(_scanBarcodeForm_FormClosed);
                this._scanBarcodeForm.Show(this);
            }
            else
            {
                if (this._scanBarcodeForm.WindowState == FormWindowState.Minimized)
                    this._scanBarcodeForm.WindowState = FormWindowState.Normal;
            }

            this.ScanMode = true;

            if (this._fillThread == null)
            {
                this._fillThread = new FillThread();
                this._fillThread.Container = this;
                this._fillThread.BeginThread();
            }
        }

        void _scanBarcodeForm_BarcodeScaned(object sender, ScanedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Barcode) == true)
            {
                Console.Beep();
                return;
            }

            // 把册条码号直接加入行中，然后等待专门的线程来装载刷新
            // 要查重
            ListViewItem dup = ListViewUtil.FindItem(this.listView_in, e.Barcode, COLUMN_BARCODE);
            if (dup != null)
            {
                Console.Beep();
                ListViewUtil.SelectLine(dup, true);
                MessageBox.Show(this, "您扫入的册条码号 ‘"+e.Barcode+"’ 在列表中已经存在了，请注意不要重复扫入");
                this._scanBarcodeForm.Activate();
                return;
            }

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, COLUMN_BARCODE, e.Barcode);
            this.listView_in.Items.Add(item);
            ListViewUtil.SelectLine(item, true);
            item.EnsureVisible();

            if (this._fillThread != null)
                this._fillThread.Activate();
        }

        void _scanBarcodeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._scanBarcodeForm = null;

            this._fillThread.StopThread(true);
            this._fillThread = null;

            this.ScanMode = false;
        }

        delegate void Delegate_GetNewlyLines(out List<ListViewItem> items,
            out List<string> barcodes);

        internal void GetNewlyLines(out List<ListViewItem> items,
            out List<string> barcodes)
        {
            if (this.InvokeRequired)
            {
                Delegate_GetNewlyLines d = new Delegate_GetNewlyLines(GetNewlyLines);
                object[] args = new object[2];
                args[0] = null;
                args[1] = null;
                this.Invoke(d, args);


                // 取出out参数值
                items = (List<ListViewItem>)args[0];
                barcodes = (List<string>)args[1];
                return;
            }

            items = new List<ListViewItem>();
            barcodes = new List<string>();

            foreach (ListViewItem item in this.listView_in.Items)
            {
                string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
                if (string.IsNullOrEmpty(strRecPath) == false)
                    continue;

                string strBarcode = ListViewUtil.GetItemText(item, COLUMN_BARCODE);
                if (string.IsNullOrEmpty(strBarcode) == true)
                    continue;

                items.Add(item);
                barcodes.Add(strBarcode);
            }
        }

        delegate void Delegate_SetRecPathColumn(List<ListViewItem> items,
            List<string> recpaths);
        internal void SetRecPathColumn(List<ListViewItem> items,
            List<string> recpaths)
        {
            if (this.InvokeRequired == true)
            {
                Delegate_SetRecPathColumn d = new Delegate_SetRecPathColumn(SetRecPathColumn);
                this.BeginInvoke(d,
                    new object[] { 
                        items,
                        recpaths }
                    );
                return;
            }

            //
            int i = 0;
            foreach (ListViewItem item in items)
            {
                string strRecPath = recpaths[i];
                if (string.IsNullOrEmpty(strRecPath) == false
                    && strRecPath[0] == '!')
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, strRecPath);
                    SetItemColor(item, TYPE_ERROR);
                }
                else
                    ListViewUtil.ChangeItemText(item, COLUMN_RECPATH, strRecPath);
                i++;
            }
        }

        #region 装载 listview_in 事项的线程

        class FillThread : ThreadBase
        {
            internal ReaderWriterLock m_lock = new ReaderWriterLock();
            internal static int m_nLockTimeout = 5000;	// 5000=5秒

            public ItemHandoverForm Container = null;

            // 工作线程每一轮循环的实质性工作
            public override void Worker()
            {
                string strError = "";
                int nRet = 0;

                this.m_lock.AcquireWriterLock(m_nLockTimeout);
                try
                {
                    if (this.Stopped == true)
                        return;

                    // 找到那些需要补充内容的行。也就是 COLUMN_RECPATH 为空的行
                    List<ListViewItem> items = new List<ListViewItem>();
                    List<string> barcodes = new List<string>();
                    this.Container.GetNewlyLines(out items,
            out barcodes);

                    if (barcodes.Count > 0)
                    {
                        // 转换为记录路径
                        List<string> recpaths = new List<string>();
                        nRet = this.Container.ConvertItemBarcodeToRecPath(
                            barcodes,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        Debug.Assert(barcodes.Count == recpaths.Count, "");

                        // 将 recpath 列内容填入
                        this.Container.SetRecPathColumn(items, recpaths);

                        // 刷新指定的行
                        this.Container.RefreshLines(COLUMN_RECPATH,
    items,
    true,
    new string[] { "summary", "@isbnissn", "targetrecpath" });
                    }

                    // m_bStopThread = true;   // 只作一轮就停止
                }
                finally
                {
                    this.m_lock.ReleaseWriterLock();
                }

            ERROR1:
                // Safe_setError(this.Container.listView_in, strError);
                return;
            }

        }

        bool _scanMode = false;

        /// <summary>
        /// 是否处在扫描条码的状态？
        /// </summary>
        public bool ScanMode
        {
            get
            {
                return this._scanMode;
            }
            set
            {
                if (this._scanMode == value)
                    return;

                this._scanMode = value;

                this.comboBox_load_type.Enabled = !this._scanMode;
                this.button_load_loadFromBarcodeFile.Enabled = !this._scanMode;
                this.button_load_loadFromBatchNo.Enabled = !this._scanMode;
                this.button_load_loadFromRecPathFile.Enabled = !this._scanMode;
                this.button_load_scanBarcode.Enabled = !this._scanMode;

                if (this._scanMode == false)
                {
                    if (this._scanBarcodeForm != null)
                        this._scanBarcodeForm.Close();
                }
                else
                {
                    button_load_scanBarcode_Click(this, new EventArgs());
                }
            }
        }

        // 进行还书操作
        // return:
        //      -1  出错
        //      其他  处理的事项数
        int DoReturn(List<ListViewItem> items,
            out string strError)
        {
            strError = "";
            //int nRet = 0;

            if (items.Count == 0)
            {
                strError = "尚未给出要还书的事项";
                return -1;
            }

            if (stop != null && stop.State == 0)    // 0 表示正在处理
            {
                strError = "目前有长操作正在进行，无法进行还书的操作";
                return -1;
            }

            string strOperName = "还书";

            int nCount = 0;
            List<ListViewItem> oper_items = new List<ListViewItem>();

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行" + strOperName + "操作 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                // 打开一个新的快捷出纳窗
                QuickChargingForm form = new QuickChargingForm();
                form.MdiParent = this.MainForm;
                form.MainForm = this.MainForm;
                form.BorrowComplete -= new BorrowCompleteEventHandler(form_BorrowComplete);
                form.BorrowComplete += new BorrowCompleteEventHandler(form_BorrowComplete);
                form.Show();

                form.SmartFuncState = FuncState.Return;

                stop.SetProgressRange(0, items.Count);

                int i = 0;
                foreach (ListViewItem item in items)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    string strBorrower = ListViewUtil.GetItemText(item, COLUMN_BORROWER);
                    if (string.IsNullOrEmpty(strBorrower) == true)
                        continue;

                    string strItemBarcode = ListViewUtil.GetItemText(item, COLUMN_BARCODE);

                    if (String.IsNullOrEmpty(strItemBarcode) == true)
                        continue;

                    form.AsyncDoAction(form.SmartFuncState, strItemBarcode);

                    stop.SetProgressValue(++i);

                    nCount++;
                    oper_items.Add(item);
                }

                // form.Close();
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControls(true);
            }

            // 注：由于不知道还书操作最终什么时候完成，所以此处无法刷新 items 的显示
            return nCount;
        }

        void form_BorrowComplete(object sender, BorrowCompleteEventArgs e)
        {
            if (string.IsNullOrEmpty(e.ItemBarcode) == true)
                return;

            ListViewItem item = ListViewUtil.FindItem(this.listView_in, e.ItemBarcode, COLUMN_BARCODE);
            if (item == null)
                return;
            List<ListViewItem> items = new List<ListViewItem>();
            items.Add(item);
            RefreshLines(COLUMN_RECPATH,
items,
false,
new string[] { "summary", "@isbnissn", "targetrecpath" });
        }

        #endregion

        // 修改全部事项的馆藏地
        private void button_move_changeLocation_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_in.Items.Count == 0)
            {
                strError = "当前没有任何事项可供操作";
                goto ERROR1;
            }

            List<ListViewItem> items = GetAllItems(this.listView_in);

            // return:
            //      0   放弃修改
            //      1   发生了修改
            nRet = ChangeLocation(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
                return;

            this.SetNextButtonEnable();

            if (string.IsNullOrEmpty(strError) == false)
                MessageBox.Show(this, strError);
            return;
        ERROR1:
            this.SetNextButtonEnable();
            MessageBox.Show(this, strError);
        }

    }

    // 定义了特定缺省值的PrintOption派生类
    internal class ItemHandoverPrintOption : PrintOption
    {
        string PublicationType = "图书"; // 图书 连续出版物

        public ItemHandoverPrintOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            this.PageHeaderDefault = "%date% 册移交清单 -- 批次号: %batchno% -- 馆藏地点: %location% -- (共 %pagecount% 页)";
            this.PageFooterDefault = "%pageno%/%pagecount%";

            this.TableTitleDefault = "%date% 册移交清单";

            this.LinesPerPageDefault = 20;

            // 2008/9/5
            // Columns缺省值
            Columns.Clear();

            // "no -- 序号",
            Column column = new Column();
            column.Name = "no -- 序号";
            column.Caption = "序号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "barcode -- 册条码号"
            column = new Column();
            column.Name = "barcode -- 册条码号";
            column.Caption = "册条码号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "accessNo -- 索取号"
            column = new Column();
            column.Name = "accessNo -- 索取号";
            column.Caption = "索取号";
            column.MaxChars = -1;
            this.Columns.Add(column);



            // "summary -- 摘要"
            column = new Column();
            column.Name = "summary -- 摘要";
            column.Caption = "摘要";
            column.MaxChars = 50;
            this.Columns.Add(column);


            // "location -- 馆藏地点"
            column = new Column();
            column.Name = "location -- 馆藏地点";
            column.Caption = "-----馆藏地点-----";  // 确保列的宽度的一种简单办法
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "price -- 册价格"
            column = new Column();
            column.Name = "price -- 册价格";
            column.Caption = "册价格";
            column.MaxChars = -1;
            this.Columns.Add(column);

            /* 缺省时不包含种价格
            // "biblioPrice -- 种价格"
            column = new Column();
            column.Name = "biblioPrice -- 种价格";
            column.Caption = "种价格";
            column.MaxChars = -1;
            this.Columns.Add(column);
             * */

            // "biblioRecpath -- 种记录路径"
            column = new Column();
            column.Name = "biblioRecpath -- 种记录路径";
            column.Caption = "种记录路径";
            column.MaxChars = -1;
            this.Columns.Add(column);
        }

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
    }
}