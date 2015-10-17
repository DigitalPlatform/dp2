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
using System.Reflection;
using System.Web;   // HttpUtility

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;

using DigitalPlatform.IO;
using DigitalPlatform.CommonControl;

using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.Marc;

using DigitalPlatform.CirculationClient.localhost;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DigitalPlatform.dp2.Statis;
using DocumentFormat.OpenXml;

namespace dp2Circulation
{
    /// <summary>
    /// 打印订单
    /// </summary>
    public partial class PrintOrderForm : BatchPrintFormBase
    {
        List<string> UsedAssemblyFilenames = new List<string>();

        /// <summary>
        /// 脚本管理器
        /// </summary>
        public ScriptManager ScriptManager = new ScriptManager();

        string BatchNo = "";    // 最近在检索面板输入过的批次号

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        public MainForm MainForm
        {
            get
            {
                return (MainForm)this.MdiParent;
            }
        }
        
        DigitalPlatform.Stop stop = null;
#endif

        /// <summary>
        /// 事项图标下标: 出错
        /// </summary>
        public const int TYPE_ERROR = 0;
        /// <summary>
        /// 事项图标下标: 普通
        /// </summary>
        public const int TYPE_NORMAL = 1;   // 书本合上摸样
        /// <summary>
        /// 事项图标下标: 发生过修改
        /// </summary>
        public const int TYPE_CHANGED = 2;  // 书本翻开摸样。表示原始记录已在窗口中发生了修改，并尚未保存

        /// <summary>
        /// 最近使用过的记录路径文件全路径
        /// </summary>
        public string RecPathFilePath = "";

        // 参与排序的列号数组
        SortColumns SortColumns_origin = new SortColumns();
        SortColumns SortColumns_merged = new SortColumns();

        #region 原始数据 ListView 列号

        /// <summary>
        /// 原始数据列号: 记录路径
        /// </summary>
        public static int ORIGIN_COLUMN_RECPATH = 0;    // 记录路径
        /// <summary>
        /// 原始数据列号: 摘要
        /// </summary>
        public static int ORIGIN_COLUMN_SUMMARY = 1;    // 摘要
        /// <summary>
        /// 原始数据列号: 错误信息
        /// </summary>
        public static int ORIGIN_COLUMN_ERRORINFO = 1;  // 错误信息
        /// <summary>
        /// 原始数据列号: ISBN/ISSN
        /// </summary>
        public static int ORIGIN_COLUMN_ISBNISSN = 2;           // ISBN/ISSN
        /// <summary>
        /// 原始数据列号: 状态
        /// </summary>
        public static int ORIGIN_COLUMN_STATE = 3;      // 状态
        /// <summary>
        /// 原始数据列号: 书目号
        /// </summary>
        public static int ORIGIN_COLUMN_CATALOGNO = 4;          // 书目号
        /// <summary>
        /// 原始数据列号: 渠道
        /// </summary>
        public static int ORIGIN_COLUMN_SELLER = 5;        // 渠道
        /// <summary>
        /// 原始数据列号: 经费来源
        /// </summary>
        public static int ORIGIN_COLUMN_SOURCE = 6;        // 经费来源
        /// <summary>
        /// 原始数据列号: 时间范围
        /// </summary>
        public static int ORIGIN_COLUMN_RANGE = 7;        // 时间范围
        /// <summary>
        /// 原始数据列号: 包含期数
        /// </summary>
        public static int ORIGIN_COLUMN_ISSUECOUNT = 8;        // 包含期数
        /// <summary>
        /// 原始数据列号: 复本数
        /// </summary>
        public static int ORIGIN_COLUMN_COPY = 9;              // 复本数
        /// <summary>
        /// 原始数据列号: 单价
        /// </summary>
        public static int ORIGIN_COLUMN_PRICE = 10;             // 单价
        /// <summary>
        /// 原始数据列号: 总价格
        /// </summary>
        public static int ORIGIN_COLUMN_TOTALPRICE = 11;        // 总价格
        /// <summary>
        /// 原始数据列号: 订购时间
        /// </summary>
        public static int ORIGIN_COLUMN_ORDERTIME = 12;        // 订购时间
        /// <summary>
        /// 原始数据列号: 订单号
        /// </summary>
        public static int ORIGIN_COLUMN_ORDERID = 13;          // 订单号
        /// <summary>
        /// 原始数据列号: 馆藏分配
        /// </summary>
        public static int ORIGIN_COLUMN_DISTRIBUTE = 14;       // 馆藏分配
        /// <summary>
        /// 原始数据列号: 类别
        /// </summary>
        public static int ORIGIN_COLUMN_CLASS = 15;             // 类别
        /// <summary>
        /// 原始数据列号: 附注
        /// </summary>
        public static int ORIGIN_COLUMN_COMMENT = 16;          // 附注
        /// <summary>
        /// 原始数据列号: 批次号
        /// </summary>
        public static int ORIGIN_COLUMN_BATCHNO = 17;          // 批次号
        /// <summary>
        /// 原始数据列号: 渠道地址
        /// </summary>
        public static int ORIGIN_COLUMN_SELLERADDRESS = 18;    // 渠道地址
        /// <summary>
        /// 原始数据列号: 种记录路径
        /// </summary>
        public static int ORIGIN_COLUMN_BIBLIORECPATH = 19;    // 种记录路径

        #endregion

        #region 合并后数据 ListView 列号

        /// <summary>
        /// 合并后数据的列号: 渠道
        /// </summary>
        public static int MERGED_COLUMN_SELLER = 0;             // 渠道
        /// <summary>
        /// 合并后数据的列号: 书目号
        /// </summary>
        public static int MERGED_COLUMN_CATALOGNO = 1;          // 书目号
        /// <summary>
        /// 合并后数据的列号: 摘要
        /// </summary>
        public static int MERGED_COLUMN_SUMMARY = 2;    // 摘要
        /// <summary>
        /// 合并后数据的列号: 错误信息
        /// </summary>
        public static int MERGED_COLUMN_ERRORINFO = 2;  // 错误信息
        /// <summary>
        /// 合并后数据的列号: ISBN/ISSN
        /// </summary>
        public static int MERGED_COLUMN_ISBNISSN = 3;           // ISBN/ISSN
        /// <summary>
        /// 合并后数据的列号: 合并注释
        /// </summary>
        public static int MERGED_COLUMN_MERGECOMMENT = 4;      // 合并注释
        /// <summary>
        /// 合并后数据的列号: 时间范围
        /// </summary>
        public static int MERGED_COLUMN_RANGE = 5;        // 时间范围
        /// <summary>
        /// 合并后数据的列号: 包含期数
        /// </summary>
        public static int MERGED_COLUMN_ISSUECOUNT = 6;        // 包含期数
        /// <summary>
        /// 合并后数据的列号: 复本数
        /// </summary>
        public static int MERGED_COLUMN_COPY = 7;              // 复本数
        /// <summary>
        /// 合并后数据的列号: 每套册数
        /// </summary>
        public static int MERGED_COLUMN_SUBCOPY = 8;              // 每套册数
        /// <summary>
        /// 合并后数据的列号: 单价
        /// </summary>
        public static int MERGED_COLUMN_PRICE = 9;             // 单价
        /// <summary>
        /// 合并后数据的列号: 总价格
        /// </summary>
        public static int MERGED_COLUMN_TOTALPRICE = 10;        // 总价格
        /// <summary>
        /// 合并后数据的列号: 订购时间
        /// </summary>
        public static int MERGED_COLUMN_ORDERTIME = 11;        // 订购时间
        /// <summary>
        /// 合并后数据的列号: 订单号
        /// </summary>
        public static int MERGED_COLUMN_ORDERID = 12;          // 订单号
        /// <summary>
        /// 合并后数据的列号: 馆藏分配
        /// </summary>
        public static int MERGED_COLUMN_DISTRIBUTE = 13;       // 馆藏分配
        /// <summary>
        /// 合并后数据的列号: 已到的套数
        /// </summary>
        public static int MERGED_COLUMN_ACCEPTCOPY = 14;       // 已到的套数

        /// <summary>
        /// 合并后数据的列号: 已到的每套册数
        /// </summary>
        public static int MERGED_COLUMN_ACCEPTSUBCOPY = 15;       // 已到的每套册数

        /// <summary>
        /// 合并后数据的列号: 到书单价
        /// </summary>
        public static int MERGED_COLUMN_ACCEPTPRICE = 16;       // 到书单价

        /// <summary>
        /// 合并后数据的列号: 类别
        /// </summary>
        public static int MERGED_COLUMN_CLASS = 17;             // 类别
        /// <summary>
        /// 合并后数据的列号: 附注
        /// </summary>
        public static int MERGED_COLUMN_COMMENT = 18;          // 附注
        /// <summary>
        /// 合并后数据的列号: 渠道地址
        /// </summary>
        public static int MERGED_COLUMN_SELLERADDRESS = 19;    // 渠道地址
        /// <summary>
        /// 合并后数据的列号: 种记录路径
        /// </summary>
        public static int MERGED_COLUMN_BIBLIORECPATH = 20;    // 种记录路径

        #endregion

        const int WM_LOADSIZE = API.WM_USER + 201;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PrintOrderForm()
        {
            InitializeComponent();
        }

        private void PrintOrderForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            CreateOriginColumnHeader(this.listView_origin);
            CreateMergedColumnHeader(this.listView_merged);

#if NO
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.comboBox_load_type.Text = this.MainForm.AppInfo.GetString(
                "printorder_form",
                "publication_type",
                "图书");

            // 验收情况
            this.checkBox_print_accepted.Checked = this.MainForm.AppInfo.GetBoolean(
                "printorder_form",
                "print_accepted",
                false);

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);

            ScriptManager.applicationInfo = this.MainForm.AppInfo;
            ScriptManager.CfgFilePath =
                this.MainForm.DataDir + "\\output_order_projects.xml";  // 导入的方案，是不分出版物类型的
            ScriptManager.DataDir = this.MainForm.DataDir;

            ScriptManager.CreateDefaultContent -= new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);
            ScriptManager.CreateDefaultContent += new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);

            try
            {
                ScriptManager.Load();
            }
            catch (FileNotFoundException)
            {
                // 不必报错
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void PrintOrderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {
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
                    "当前窗口内有原始信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "PrintOrderForm",
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

        private void PrintOrderForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif

            this.MainForm.AppInfo.SetString(
                "printorder_form",
                "publication_type",
                this.comboBox_load_type.Text);

            this.MainForm.AppInfo.SetBoolean(
    "printorder_form",
    "print_accepted",
    this.checkBox_print_accepted.Checked);

            SaveSize();
        }

        /*public*/
        void LoadSize()
        {
#if NO
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state");
#endif

            string strWidths = this.MainForm.AppInfo.GetString(
                "printorder_form",
                "list_origin_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_origin,
                    strWidths,
                    true);
            }

            strWidths = this.MainForm.AppInfo.GetString(
    "printorder_form",
    "list_merged_width",
    "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_merged,
                    strWidths,
                    true);
            }
        }

        /*public*/
        void SaveSize()
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

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_origin);
            this.MainForm.AppInfo.SetString(
                "printorder_form",
                "list_origin_width",
                strWidths);

            strWidths = ListViewUtil.GetColumnWidthListString(this.listView_merged);
            this.MainForm.AppInfo.SetString(
                "printorder_form",
                "list_merged_width",
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

#if NO
        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            this.MainForm.Channel_BeforeLogin(this, e);
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        bool m_bEnabled = true;

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.m_bEnabled = bEnable;

            // load page
            this.button_load_loadFromBatchNo.Enabled = bEnable;
            this.button_load_loadFromFile.Enabled = bEnable;

            this.comboBox_load_type.Enabled = bEnable;

            // next button
            if (bEnable == true)
                SetNextButtonEnable();
            else
            {
                this.button_next.Enabled = false;

                //
                this.button_saveChange_saveChange.Enabled = false;
            }

            // print page
            this.button_print_mergedOption.Enabled = bEnable;
            this.button_print_printOrderList.Enabled = bEnable;
            this.button_print_originOption.Enabled = bEnable;
            this.button_print_printOriginList.Enabled = bEnable;
            this.button_print_outputOrderOption.Enabled = bEnable;
            this.button_print_outputOrder.Enabled = bEnable;
            this.button_print_arriveRatioStatis.Enabled = bEnable;

            if (this.checkBox_print_accepted.Checked == true)
                this.checkBox_print_accepted.Enabled = bEnable;
            else
                this.checkBox_print_accepted.Enabled = false;
        }

        /// <summary>
        /// 是否为验收情形。即 “验收情况” checkbox 是否勾选
        /// </summary>
        public bool AcceptCondition
        {
            get
            {
                return this.checkBox_print_accepted.Checked;
            }
            set
            {
                this.checkBox_print_accepted.Checked = value;
            }
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
            else if (this.tabControl_main.SelectedTab == this.tabPage_saveChange)
            {

                //      0   没有发生过修改
                //      1   发生了修改，并且尚未保存
                //      2   发生了修改，已经保存过了，并没有新的修改
                int nState = ReportSaveChangeState(out strError);
                int nErrorCount = GetErrorLineCount();
                if (nState == 1)
                {
                    this.button_saveChange_saveChange.Enabled = true;
                }
                else
                {
                    this.button_saveChange_saveChange.Enabled = false;
                }

                // 没有错误事项，没有修改或者修改已经保存
                if (nErrorCount == 0 && nState != 1)
                    this.button_next.Enabled = true;
                else
                    this.button_next.Enabled = false;

                if (nErrorCount > 0)
                    strError += "\r\n\r\n原始数据列表中有错误事项(红色背景) " + nErrorCount.ToString() + "个";

                this.textBox_saveChange_info.Text = strError;
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

            for (int i = 0; i < this.listView_origin.Items.Count; i++)
            {
                ListViewItem item = this.listView_origin.Items[i];

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

        int m_nSavedCount = 0;  // 先前保存过的次数

        // 汇报保存进行情况。
        // return:
        //      true    保存已经完成
        //      false   保存尚未完成
        //      0   没有发生过修改
        //      1   发生了修改，并且尚未保存
        //      2   发生了修改，已经保存过了，并没有新的修改
        int ReportSaveChangeState(out string strError)
        {
            strError = "";

            // 全部listview事项都是TYPE_NORMAL状态，才表明保存已经完成
            int nYellowCount = 0;   // 发生过修改的事项
            int nRedCount = 0;  // 有错误信息的事项
            int nWhiteCount = 0;    // 普通事项

            for (int i = 0; i < this.listView_origin.Items.Count; i++)
            {
                ListViewItem item = this.listView_origin.Items[i];

                if (item.ImageIndex == TYPE_CHANGED)
                    nYellowCount++;
                else if (item.ImageIndex == TYPE_ERROR)
                    nRedCount++;
                else if (item.ImageIndex == TYPE_NORMAL)
                    nWhiteCount++;
            }

            if (nYellowCount == 0)
            {
                if (this.m_nSavedCount == 0)
                {
                    strError = "没有发生过修改";
                    // return true;
                    return 0;
                }

                strError = "已经保存";
                return 2;
            }

            // strError = "原始数据列表中有发生了修改后尚未保存的事项，或有错误事项。\r\n\r\n列表中有:\r\n发生过修改的事项(淡黄色背景) " + nYellowCount.ToString() + " 个\r\n错误事项(红色背景) " + nRedCount.ToString() + "个\r\n\r\n(只有全部事项都为普通状态(白色背景)，才表明保存操作已经完成)";
            // return false;
            strError = "原始数据列表中有发生了修改后尚未保存的事项(淡黄色背景) " + nYellowCount.ToString() + " 个\r\n\r\n(只有当列表中全部事项都为普通状态(白色背景)，才表明保存操作已经完成)";
            return 1;
        }

        // 错误状态行数目
        int GetErrorLineCount()
        {
            int nRedCount = 0;  // 有错误信息的事项

            for (int i = 0; i < this.listView_origin.Items.Count; i++)
            {
                ListViewItem item = this.listView_origin.Items[i];

                if (item.ImageIndex == TYPE_ERROR)
                    nRedCount++;
            }

            return nRedCount;
        }

        // parameters:
        //      bAutoSetSeriesType  是否根据文件第一行中的路径中的数据库名来自东设置Combobox_type
        // return:
        //      -1  出错
        //      0   放弃处理
        //      1   成功
        /// <summary>
        /// 从订购记录路径文件中装载数据
        /// </summary>
        /// <param name="bAutoSetSeriesType">是否自动根据文件内容设置出版物类型</param>
        /// <param name="strFilename">订购记录路径文件名全路径</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:  出错</para>
        /// <para>0:   放弃处理</para>
        /// <para>1:   成功</para>
        /// </returns>
        public int LoadFromOrderRecPathFile(
            bool bAutoSetSeriesType,
            string strFilename,
            out string strError)
        {
            strError = "";

            int nDupCount = 0;
            int nRet = 0;

            StreamReader sr = null;
            try
            {
                // 打开文件
                sr = new StreamReader(strFilename);

                EnableControls(false);
                // MainForm.ShowProgress(true);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在初始化浏览器组件 ...");
                stop.BeginLoop();
                this.Update();
                this.MainForm.Update();

                try
                {
                    if (System.Windows.Forms.Control.ModifierKeys == Keys.Control)
                    {
                    }
                    else
                    {
                        if (this.Changed == true)
                        {
                            // 警告尚未保存
                            DialogResult result = MessageBox.Show(this,
                                "当前窗口内有原始信息被修改后尚未保存。若此时为装载新内容而清除原有信息，则未保存信息将丢失。\r\n\r\n确实要装载新内容? ",
                                "PrintOrderForm",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            if (result != DialogResult.Yes)
                            {
                                return 0; // 放弃
                            }
                        }

                        this.listView_origin.Items.Clear();
                        // 2008/11/22
                        this.SortColumns_origin.Clear();
                        SortColumns.ClearColumnSortDisplay(this.listView_origin.Columns);

                    }

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


                        string strOrderRecPath = "";
                        strOrderRecPath = sr.ReadLine();

                        if (strOrderRecPath == null)
                            break;

                        strOrderRecPath = strOrderRecPath.Trim();
                        if (String.IsNullOrEmpty(strOrderRecPath) == true)
                            continue;

                        if (strOrderRecPath[0] == '#')
                            continue;   // 注释行

                        // 检查订购库路径
                        {
                            string strDbName = Global.GetDbName(strOrderRecPath);
                            string strBiblioDbName = this.MainForm.GetBiblioDbNameFromOrderDbName(strDbName);
                            if (string.IsNullOrEmpty(strBiblioDbName) == true)
                            {
                                strError = "记录路径 '" + strOrderRecPath + "' 中的数据库名 '" + strDbName + "' 不是订购库名";
                                goto ERROR1;
                            }
                            BiblioDbProperty prop = this.MainForm.GetBiblioDbProperty(strBiblioDbName);
                            if (prop == null)
                            {
                                strError = "数据库名 '" + strBiblioDbName + "' 不是书目库名";
                                goto ERROR1;
                            }

                            // 自动设置 图书/期刊 类型
                            if (bAutoSetSeriesType == true && nLineCount == 0)
                            {
                                if (string.IsNullOrEmpty(prop.IssueDbName) == true)
                                    this.comboBox_load_type.Text = "图书";
                                else
                                    this.comboBox_load_type.Text = "连续出版物";
                            }

                            if (string.IsNullOrEmpty(prop.IssueDbName) == false)
                            {
                                // 期刊库
                                if (this.comboBox_load_type.Text != "连续出版物")
                                {
                                    strError = "记录路径 '" + strOrderRecPath + "' 中的订购库名 '" + strDbName + "' 不是图书类型";
                                    goto ERROR1;
                                }
                            }
                            else
                            {
                                // 图书库
                                if (this.comboBox_load_type.Text != "图书")
                                {
                                    strError = "记录路径 '" + strOrderRecPath + "' 中的订购库名 '" + strDbName + "' 不是期刊类型";
                                    goto ERROR1;
                                }
                            }
                        }

                        nLineCount++;
                    }

                    // 设置进度范围
                    stop.SetProgressRange(0, nLineCount);
                    // stop.SetProgressValue(0);

                    // 逐行处理
                    // 文件回头?
                    // sr.BaseStream.Seek(0, SeekOrigin.Begin);

                    sr.Close();

                    sr = new StreamReader(strFilename);


                    for (int i = 0; ; i++)
                    {
                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断2";
                                goto ERROR1;
                            }
                        }

                        string strOrderRecPath = "";
                        strOrderRecPath = sr.ReadLine();

                        stop.SetProgressValue(i);

                        if (strOrderRecPath == null)
                            break;

                        strOrderRecPath = strOrderRecPath.Trim();
                        if (String.IsNullOrEmpty(strOrderRecPath) == true)
                            continue;

                        if (strOrderRecPath[0] == '#')
                            continue;   // 注释行

                        stop.SetMessage("正在装入路径 " + strOrderRecPath + " 对应的记录...");


                        // 根据记录路径，装入订购记录
                        // return: 
                        //      -2  路径已经在list中存在了
                        //      -1  出错
                        //      1   成功
                        nRet = LoadOneItem(strOrderRecPath,
                            this.listView_origin,
                            out strError);
                        if (nRet == -2)
                            nDupCount++;
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
                strError = "PrintOrderForm LoadFromOrderRecPathFile() exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }
            finally
            {
                sr.Close();
            }

            // 记忆文件名
            this.RecPathFilePath = strFilename;
            this.Text = "打印订单 " + Path.GetFileName(this.RecPathFilePath);

            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " + nDupCount.ToString() + "个重复记录路径的事项被忽略。");
            }

            // 填充合并后数据列表
            stop.SetMessage("正在合并数据...");
            nRet = FillMergedList(out strError);
            stop.SetMessage("");
            if (nRet == -1)
                goto ERROR1;


            // 汇报数据装载情况。
            // return:
            //      0   尚未装载任何数据    
            //      1   装载已经完成
            //      2   虽然装载了数据，但是其中有错误事项
            int nState = ReportLoadState(out strError);
            if (nState != 1)
                goto ERROR1;

            return 1;
        ERROR1:
            return -1;
        }

        // 从订购库记录路径文件装载
        private void button_load_loadFromFile_Click(object sender, EventArgs e)
        {
            this.BatchNo = "";  // 表示不是根据批次号获得的内容

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的订购库记录路径文件名";
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string strError = "";

            // return:
            //      -1  出错
            //      0   放弃
            //      1   装载成功
            int nRet = LoadFromOrderRecPathFile(
                true,
                dlg.FileName,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            this.Text = "打印订单";
            MessageBox.Show(this, strError);
        }

        // 
        /// <summary>
        /// 将书目记录 XML 格式转换为 MARC 格式。
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strXml">书目记录 XML</param>
        /// <param name="strMarc">返回 MARC 记录(机内格式)</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错: 0: 成功</returns>
        public int ConvertXmlToMarc(
            string strBiblioRecPath,
            string strXml,
            out string strMarc,
            out string strError)
        {
            strError = "";
            strMarc = "";
            int nRet = 0;

            // strXml中为书目记录
            string strBiblioDbName = Global.GetDbName(strBiblioRecPath);

            string strSyntax = this.MainForm.GetBiblioSyntax(strBiblioDbName);
            if (String.IsNullOrEmpty(strSyntax) == true)
                strSyntax = "unimarc";

            if (strSyntax == "usmarc" || strSyntax == "unimarc")
            {
                // 将XML书目记录转换为MARC格式
                string strOutMarcSyntax = "";

                // 将MARCXML格式的xml记录转换为marc机内格式字符串
                // parameters:
                //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                nRet = MarcUtil.Xml2Marc(strXml,
                    true,   // 2013/1/12 修改为true
                    "", // strMarcSyntax
                    out strOutMarcSyntax,
                    out strMarc,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(strOutMarcSyntax) == false)
                {
                    if (strOutMarcSyntax != strSyntax)
                    {
                        strError = "书目记录 " + strBiblioRecPath + " 的syntax '" + strOutMarcSyntax + "' 和其所属数据库 '" + strBiblioDbName + "' 的定义syntax '" + strSyntax + "' 不一致";
                        return -1;
                    }
                }

                return 0;
            }

            strError = "书目库 '" + strBiblioDbName + "' 的格式不是MARC格式。(而是 '" + strSyntax + "')";
            return -1;
        }

        // 获得书目数据(XML格式)
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 从 dpLibrary 服务器获得一条书目记录。
        /// 请参考 dp2Library API GetBiblioInfos() 的详细介绍
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strXmlRecord">书目记录 XML</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:  出错</para>
        /// <para>0:   没有找到</para>
        /// <para>1:   找到</para>
        /// </returns>
        public int GetBiblioRecord(string strBiblioRecPath,
            out string strXmlRecord,
            out string strError)
        {
            strError = "";
            strXmlRecord = "";

            string[] formats = new string[1];
            formats[0] = "xml";
            string[] results = null;
            byte[] timestamp = null;

            if (String.IsNullOrEmpty(strBiblioRecPath) == true)
            {
                strError = "strBiblioRecPath参数值不能为空";
                return -1;
            }

            long lRet = Channel.GetBiblioInfos(
                stop,
                strBiblioRecPath,
                "",
                formats,
                out results,
                out timestamp,
                out strError);
            if (lRet == 0)
            {
                if (String.IsNullOrEmpty(strError) == true)
                    strError = "记录 " + strBiblioRecPath + " 没有找到";

                return 0;   // not found
            }
            if (lRet == -1)
            {
                strError = "获得书目记录时发生错误: " + strError;
                return -1;
            }
            else
            {
                Debug.Assert(results != null && results.Length == 1, "results必须包含1个元素");
                strXmlRecord = results[0];
            }

            return (int)lRet;
        }

        // 根据记录路径，装入订购记录
        // return: 
        //      -2  路径已经在list中存在了
        //      -1  出错
        //      1   成功
        int LoadOneItem(string strRecPath,
            ListView list,
            out string strError)
        {
            strError = "";

            string strItemXml = "";
            string strBiblioText = "";

            string strOutputOrderRecPath = "";
            string strOutputBiblioRecPath = "";

            byte[] item_timestamp = null;

            long lRet = Channel.GetOrderInfo(
                stop,
                "@path:" + strRecPath,
                // "",
                "xml",
                out strItemXml,
                out strOutputOrderRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strOutputBiblioRecPath,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                ListViewItem item = new ListViewItem(strRecPath, 0);

                OriginItemData data = new OriginItemData();
                item.Tag = data;
                data.Timestamp = item_timestamp;
                data.Xml = strItemXml;

                item.SubItems.Add(strError);

                SetItemColor(item, TYPE_ERROR);
                list.Items.Add(item);

                // 将新加入的事项滚入视野
                list.EnsureVisible(list.Items.Count - 1);

                goto ERROR1;
            }

            string strBiblioSummary = "";
            string strISBnISSN = "";

            // 看看记录路径是否有重复?
            // 顺便获得同种的事项
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem curitem = list.Items[i];

                if (strRecPath == curitem.Text)
                {
                    strError = "记录路径 " + strRecPath + " 发生重复";
                    return -2;
                }

                if (strBiblioSummary == "" && curitem.ImageIndex != TYPE_ERROR)
                {
                    if (curitem.SubItems[ORIGIN_COLUMN_BIBLIORECPATH].Text == strOutputBiblioRecPath)
                    {
                        strBiblioSummary = ListViewUtil.GetItemText(curitem, ORIGIN_COLUMN_SUMMARY);
                        strISBnISSN = ListViewUtil.GetItemText(curitem, ORIGIN_COLUMN_ISBNISSN);
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

                Debug.Assert(String.IsNullOrEmpty(strOutputBiblioRecPath) == false, "strBiblioRecPath值不能为空");

                lRet = Channel.GetBiblioInfos(
                    stop,
                    strOutputBiblioRecPath,
                    "",
                    formats,
                    out results,
                    out timestamp,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                        strError = "书目记录 '" + strOutputBiblioRecPath + "' 不存在";

                    strBiblioSummary = "获得书目摘要时发生错误: " + strError;
                }
                else
                {
                    Debug.Assert(results != null && results.Length == 2, "results必须包含2个元素");
                    strBiblioSummary = results[0];
                    strISBnISSN = results[1];
                }
            }

            // 剖析一个订购xml记录，取出有关信息放入listview中

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "PrintOrderForm dom.LoadXml() {13C10700-D098-4613-9495-9E133644B8D2} exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            {
                ListViewItem item = AddToListView(
                    this.comboBox_load_type.Text,
                    this.checkBox_print_accepted.Checked,
                    list,
                    dom,
                    strOutputOrderRecPath,
                    strBiblioSummary,
                    strISBnISSN,
                    strOutputBiblioRecPath);

                // 设置timestamp/xml
                OriginItemData data = (OriginItemData)item.Tag;
                Debug.Assert(data != null, "");
                data.Timestamp = item_timestamp;
                data.Xml = strItemXml;

                /*
                // 图标
                SetItemColor(item, TYPE_NORMAL);
                 * */

                // 将新加入的事项滚入视野
                list.EnsureVisible(list.Items.Count - 1);
            }

            return 1;
        ERROR1:
            return -1;
        }

        static System.Drawing.Color GetItemForeColor(int nType)
        {
            if (nType == TYPE_ERROR)
            {
                return System.Drawing.Color.White;
            }
            else if (nType == TYPE_CHANGED)
            {
                return System.Drawing.SystemColors.WindowText;
            }
            else if (nType == TYPE_NORMAL)
            {
                return System.Drawing.SystemColors.WindowText;
            }
            else
            {
                throw new Exception("未知的image type");
            }
        }

        static System.Drawing.Color GetItemBackColor(int nType)
        {
            if (nType == TYPE_ERROR)
            {
                return System.Drawing.Color.Red;
            }
            else if (nType == TYPE_CHANGED)
            {
                return System.Drawing.Color.LightYellow;
            }
            else if (nType == TYPE_NORMAL)
            {
                return System.Drawing.SystemColors.Window;
            }
            else
            {
                throw new Exception("未知的image type");
            }

        }

        // 设置事项的背景、前景颜色，和图标
        static void SetItemColor(ListViewItem item,
            int nType)
        {
            item.BackColor = GetItemBackColor(nType);
            item.ForeColor = GetItemForeColor(nType);
            item.ImageIndex = nType;

            // 顺便检查调用本函数前的data.Changed值是否正确
#if DEBUG
            {
                if (item.Tag is OriginItemData
                    && nType != TYPE_ERROR)
                {
                    OriginItemData data = (OriginItemData)item.Tag;

                    Debug.Assert(data != null, "");

                    if (data != null)
                    {
                        if (nType == TYPE_CHANGED)
                        {
                            Debug.Assert(data.Changed == true, "");
                        }
                        if (nType == TYPE_NORMAL)
                        {
                            Debug.Assert(data.Changed == false, "");
                        }
                    }
                }
            }
#endif

            /*
            if (nType == TYPE_ERROR)
            {
                item.ForeColor = Color.White;
                item.ImageIndex = TYPE_ERROR;
            }
            else if (nType == TYPE_CHANGED)
            {
                item.BackColor = Color.LightYellow;
                item.ForeColor = SystemColors.WindowText;
                item.ImageIndex = TYPE_CHANGED;
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
             * */

        }

        /*public*/ static ListViewItem AddToListView(
            string strPubType,
            bool bAccepted,
            ListView list,
            XmlDocument dom,
            string strRecPath,
            string strBiblioSummary,
            string strISBnISSN,
            string strBiblioRecPath)
        {
            ListViewItem item = new ListViewItem(strRecPath, TYPE_NORMAL);

            SetListViewItemText(
                strPubType,
                bAccepted,
                dom,
                false,
                strRecPath,
                strBiblioSummary,
                strISBnISSN,
                strBiblioRecPath,
                item);
            list.Items.Add(item);

            return item;
        }

        // 根据订购记录DOM设置ListViewItem除第一列以外的文字
        // parameters:
        //      bSetBarcodeColumn   是否要设置第一列记录路径的内容
        /*public*/ static void SetListViewItemText(
            string strPubType,
            bool bAccepted,
            XmlDocument dom,
            bool bSetRecPathColumn,
            string strRecPath,
            string strBiblioSummary,
            string strISBnISSN,
            string strBiblioRecPath,
            ListViewItem item)
        {
            int nRet = 0;
            OriginItemData data = null;
            data = (OriginItemData)item.Tag;
            if (data == null)
            {
                data = new OriginItemData();
                item.Tag = data;
            }
            else
            {
                data.Changed = false;   // 2008/9/5
            }

            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");
            string strCatalogNo = DomUtil.GetElementText(dom.DocumentElement,
                "catalogNo");
            string strSeller = DomUtil.GetElementText(dom.DocumentElement,
                "seller");
            string strSource = DomUtil.GetElementText(dom.DocumentElement,
                "source");
            string strRange = DomUtil.GetElementText(dom.DocumentElement,
                "range");
            string strIssueCount = DomUtil.GetElementText(dom.DocumentElement,
                "issueCount");

            // TODO: 是否只将订购复本字符串放入复本列?
            string strCopy = DomUtil.GetElementText(dom.DocumentElement,
                "copy");

            // TODO: 是否只将订购价放入价格列?
            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            string strTotalPrice = DomUtil.GetElementText(dom.DocumentElement,
                "totalPrice");

            // 2010/12/8
            if (strState == "已验收" && bAccepted == false)
            {
                strBiblioSummary = "本记录状态为 '已验收'，不能再参与订单打印";
                SetItemColor(item,
                        TYPE_ERROR);
            }

            List<int> textchanged_columns = new List<int>();


            int nIssueCount = 1;
            if (strPubType == "连续出版物")
            {
                try
                {
                    nIssueCount = Convert.ToInt32(strIssueCount);
                }
                catch (Exception ex)
                {
                    strBiblioSummary = "包含期数 '" + strIssueCount + "' 格式不正确: " + ex.Message;
                    SetItemColor(item,
                            TYPE_ERROR);
                }
            }
            else
            {
                Debug.Assert(strPubType == "图书", "");
            }

            {
                string strOldCopy = "";
                string strNewCopy = "";
                // 分离 "old[new]" 内的两个值
                OrderDesignControl.ParseOldNewValue(strCopy,
                    out strOldCopy,
                    out strNewCopy);
                // strCopy = strOldCopy;

                int nCopy = 0;
                try
                {
                    nCopy = Convert.ToInt32(OrderDesignControl.GetCopyFromCopyString(strOldCopy));
                }
                catch (Exception ex)
                {
                    strBiblioSummary = "订购复本数字 '" + strOldCopy + "' 格式不正确: " + ex.Message;
                    SetItemColor(item,
                            TYPE_ERROR);
                }

                string strCurrentOldPrice = "";
                string strCurrentNewPrice = "";

                // 分离 "old[new]" 内的两个值
                OrderDesignControl.ParseOldNewValue(strPrice,
                    out strCurrentOldPrice,
                    out strCurrentNewPrice);

                string strCurrentPrice = strCurrentOldPrice;


                // 汇总价格
                string strCurTotalPrice = "";
                string strError = "";

                // 2009/11/9 changed
                // 只有原始数据中总价格为空时，才有必要汇总价格
                if (String.IsNullOrEmpty(strTotalPrice) == true)
                {
                    nRet = PriceUtil.MultiPrice(strCurrentPrice,
                        nCopy,
                        out strCurTotalPrice,
                        out strError);
                    if (nRet == -1)
                    {
                        strBiblioSummary = "价格字符串 '" + strCurrentPrice + "' 格式不正确: " + strError;
                        SetItemColor(item,
                                TYPE_ERROR);
                    }

                    // 再算上期数
                    if (nIssueCount != 1)
                    {
                        Debug.Assert(strPubType == "连续出版物", "");

                        string strTempPrice = strCurTotalPrice;
                        nRet = PriceUtil.MultiPrice(strTempPrice,
                            nIssueCount,
                            out strCurTotalPrice,
                            out strError);
                        if (nRet == -1)
                        {
                            strBiblioSummary = "价格字符串 '" + strTempPrice + "' 格式不正确: " + strError;
                            SetItemColor(item,
                                    TYPE_ERROR);
                        }
                    }

                    if (item.ImageIndex != TYPE_ERROR)
                    {
                        if (strTotalPrice != strCurTotalPrice)
                        {
                            strTotalPrice = "*" + strCurTotalPrice; // 2014/11/5
                            data.Changed = true;
                            SetItemColor(item,
                                TYPE_CHANGED); // 表示总价格被改变了

                            /*
                            // 加粗字体
                            item.SubItems[ORIGIN_COLUMN_TOTALPRICE].Font = 
                                new Font(item.SubItems[ORIGIN_COLUMN_TOTALPRICE].Font, FontStyle.Bold);
                             * */
                            textchanged_columns.Add(ORIGIN_COLUMN_TOTALPRICE);
                        }
                    }

                }


            }

            // 检查和修改 状态
            if (item.ImageIndex != TYPE_ERROR)  // 2009/11/23
            {
                if (strState == "已验收")
                {
                    /*
                    strBiblioSummary = "本记录状态为 '已验收'，不能再参与订单打印";
                    SetItemColor(item,
                            TYPE_ERROR);
                     * */
                }
                else if (bAccepted == false)
                {
                    string strNewState = "已订购";

                    if (strState != strNewState)
                    {
                        strState = "*" + strNewState;   // 2014/11/5
                        data.Changed = true;
                        SetItemColor(item,
                            TYPE_CHANGED); // 表示状态被改变了

                        textchanged_columns.Add(ORIGIN_COLUMN_STATE);
                    }
                }
            }

            string strOrderTime = DomUtil.GetElementText(dom.DocumentElement,
                "orderTime");

            if (string.IsNullOrEmpty(strOrderTime) == false)
            {
                // 转化为本地时间格式 2009/1/5
                try
                {
                    DateTime order_time = DateTimeUtil.FromRfc1123DateTimeString(strOrderTime);
                    strOrderTime = order_time.ToLocalTime().ToShortDateString();
                }
                catch (Exception /*ex*/)
                {
                    strOrderTime = "时间字符串 '" + strOrderTime + "' 格式错误，不是RFC1123格式";
                }
            }

            string strOrderID = DomUtil.GetElementText(dom.DocumentElement,
                "orderID");
            string strDistribute = DomUtil.GetElementText(dom.DocumentElement,
                "distribute");
            string strClass = DomUtil.GetElementText(dom.DocumentElement,
                "class");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");
            string strBatchNo = DomUtil.GetElementText(dom.DocumentElement,
                "batchNo");
            string strSellerAddress = DomUtil.GetElementInnerXml(dom.DocumentElement,
                "sellerAddress");

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_SUMMARY, strBiblioSummary);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ISBNISSN, strISBnISSN);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_STATE, strState);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_CATALOGNO, strCatalogNo);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_SELLER, strSeller);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_SOURCE, strSource);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_RANGE, strRange);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ISSUECOUNT, strIssueCount);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_COPY, strCopy);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_PRICE, strPrice);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_TOTALPRICE, strTotalPrice);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ORDERTIME, strOrderTime);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ORDERID, strOrderID);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_DISTRIBUTE, strDistribute);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_CLASS, strClass);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_COMMENT, strComment);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_BATCHNO, strBatchNo);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_SELLERADDRESS, strSellerAddress);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_BIBLIORECPATH, strBiblioRecPath);

            if (bSetRecPathColumn == true)
            {
                ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_RECPATH, strRecPath);
            }

            // 加粗字体
            for (int i = 0; i < textchanged_columns.Count; i++)
            {
                int index = textchanged_columns[i];
                item.SubItems[index].Font =
                    new System.Drawing.Font(item.SubItems[index].Font, FontStyle.Bold);
            }



            if (item.ImageIndex == TYPE_NORMAL)
                SetItemColor(item, TYPE_NORMAL);

        }

        // 设置 原始数据listview 的栏目标题
        void CreateOriginColumnHeader(ListView list)
        {
            ColumnHeader columnHeader_recpath = new ColumnHeader();
            ColumnHeader columnHeader_errorInfo = new ColumnHeader();
            ColumnHeader columnHeader_isbnIssn = new ColumnHeader();
            ColumnHeader columnHeader_state = new ColumnHeader();
            ColumnHeader columnHeader_catalogNo = new ColumnHeader();
            ColumnHeader columnHeader_seller = new ColumnHeader();
            ColumnHeader columnHeader_source = new ColumnHeader();
            ColumnHeader columnHeader_range = new ColumnHeader();
            ColumnHeader columnHeader_issueCount = new ColumnHeader();
            ColumnHeader columnHeader_copy = new ColumnHeader();
            ColumnHeader columnHeader_price = new ColumnHeader();

            ColumnHeader columnHeader_totalPrice = new ColumnHeader();
            ColumnHeader columnHeader_orderTime = new ColumnHeader();
            ColumnHeader columnHeader_orderID = new ColumnHeader();
            ColumnHeader columnHeader_distribute = new ColumnHeader();

            ColumnHeader columnHeader_class = new ColumnHeader();
            ColumnHeader columnHeader_comment = new ColumnHeader();
            ColumnHeader columnHeader_batchNo = new ColumnHeader();
            ColumnHeader columnHeader_sellerAddress = new ColumnHeader();
            ColumnHeader columnHeader_biblioRecpath = new ColumnHeader();

            list.Columns.Clear();

            list.Columns.AddRange(new ColumnHeader[] {
            columnHeader_recpath,
            columnHeader_errorInfo,
            columnHeader_isbnIssn,
            columnHeader_state,
            columnHeader_catalogNo,
            columnHeader_seller,
            columnHeader_source,
            columnHeader_range,
            columnHeader_issueCount,
            columnHeader_copy,
            columnHeader_price,
            columnHeader_totalPrice,
            columnHeader_orderTime,
            columnHeader_orderID,
            columnHeader_distribute,
            columnHeader_class,
            columnHeader_comment,
            columnHeader_batchNo,
            columnHeader_sellerAddress,
            columnHeader_biblioRecpath});


            // 
            // columnHeader_recpath
            // 
            columnHeader_recpath.Text = "订购记录路径";
            columnHeader_recpath.Width = 200;
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
            // columnHeader_catalogNo
            // 
            columnHeader_catalogNo.Text = "书目号";
            columnHeader_catalogNo.Width = 100;
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
            // columnHeader_range
            // 
            columnHeader_range.Text = "时间范围";
            columnHeader_range.Width = 150;

            // 
            // columnHeader_issueCount
            // 
            columnHeader_issueCount.Text = "包含期数";
            columnHeader_issueCount.Width = 150;
            columnHeader_issueCount.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_copy
            // 
            columnHeader_copy.Text = "复本数";
            columnHeader_copy.Width = 150;
            columnHeader_copy.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_price
            // 
            columnHeader_price.Text = "单价";
            columnHeader_price.Width = 150;
            columnHeader_price.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_totalPrice
            // 
            columnHeader_totalPrice.Text = "总价";
            columnHeader_totalPrice.Width = 150;
            columnHeader_totalPrice.TextAlign = HorizontalAlignment.Right;
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

            // 
            // columnHeader_distribute
            // 
            columnHeader_distribute.Text = "馆藏分配";
            columnHeader_distribute.Width = 150;

            // 
            // columnHeader_class
            // 
            columnHeader_class.Text = "类别";
            columnHeader_class.Width = 100;

            // 
            // columnHeader_comment
            // 
            columnHeader_comment.Text = "附注";
            columnHeader_comment.Width = 150;
            // 
            // columnHeader_batchNo
            // 
            columnHeader_batchNo.Text = "批次号";
            columnHeader_batchNo.Width = 100;
            // 
            // columnHeader_sellerAddress
            // 
            columnHeader_sellerAddress.Text = "渠道地址";
            columnHeader_sellerAddress.Width = 200;
            // 
            // columnHeader_biblioRecpath
            // 
            columnHeader_biblioRecpath.Text = "种记录路径";
            columnHeader_biblioRecpath.Width = 200;
        }

        // 设置 合并后数据listview 的栏目标题
        void CreateMergedColumnHeader(ListView list)
        {
            ColumnHeader columnHeader_seller = new ColumnHeader();
            ColumnHeader columnHeader_catalogNo = new ColumnHeader();
            ColumnHeader columnHeader_errorInfo = new ColumnHeader();
            ColumnHeader columnHeader_isbnIssn = new ColumnHeader();
            ColumnHeader columnHeader_mergeComment = new ColumnHeader();
            ColumnHeader columnHeader_range = new ColumnHeader();
            ColumnHeader columnHeader_issueCount = new ColumnHeader();
            ColumnHeader columnHeader_copy = new ColumnHeader();
            ColumnHeader columnHeader_subcopy = new ColumnHeader();
            ColumnHeader columnHeader_price = new ColumnHeader();
            ColumnHeader columnHeader_totalPrice = new ColumnHeader();
            ColumnHeader columnHeader_orderTime = new ColumnHeader();
            ColumnHeader columnHeader_orderID = new ColumnHeader();
            ColumnHeader columnHeader_distribute = new ColumnHeader();
            ColumnHeader columnHeader_acceptcopy = new ColumnHeader();
            ColumnHeader columnHeader_acceptsubcopy = new ColumnHeader();
            ColumnHeader columnHeader_acceptprice = new ColumnHeader();
            ColumnHeader columnHeader_class = new ColumnHeader();
            ColumnHeader columnHeader_comment = new ColumnHeader();
            ColumnHeader columnHeader_sellerAddress = new ColumnHeader();
            ColumnHeader columnHeader_biblioRecpath = new ColumnHeader();

            list.Columns.Clear();

            list.Columns.AddRange(new ColumnHeader[] {
            columnHeader_seller,
            columnHeader_catalogNo,
            columnHeader_errorInfo,
            columnHeader_isbnIssn,
            columnHeader_mergeComment,
            columnHeader_range,
            columnHeader_issueCount,
            columnHeader_copy,
            columnHeader_subcopy,
            columnHeader_price,
            columnHeader_totalPrice,
            columnHeader_orderTime,
            columnHeader_orderID,
            columnHeader_distribute,
            columnHeader_acceptcopy,
            columnHeader_acceptsubcopy,
            columnHeader_acceptprice,
            columnHeader_class,
            columnHeader_comment,
            columnHeader_sellerAddress,
            columnHeader_biblioRecpath});


            // 
            // columnHeader_seller
            // 
            columnHeader_seller.Text = "渠道";
            columnHeader_seller.Width = 150;
            // 
            // columnHeader_catalogNo
            // 
            columnHeader_catalogNo.Text = "书目号";
            columnHeader_catalogNo.Width = 100;
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
            // columnHeader_source
            // 
            columnHeader_mergeComment.Text = "合并注释";
            columnHeader_mergeComment.Width = 150;
            // 
            // columnHeader_range
            // 
            columnHeader_range.Text = "时间范围";
            columnHeader_range.Width = 150;
            // 
            // columnHeader_issueCount
            // 
            columnHeader_issueCount.Text = "包含期数";
            columnHeader_issueCount.Width = 150;
            columnHeader_issueCount.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_copy
            // 
            columnHeader_copy.Text = "复本数";
            columnHeader_copy.Width = 100;
            columnHeader_copy.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_subcopy
            // 
            columnHeader_subcopy.Text = "每套册数";
            columnHeader_subcopy.Width = 100;
            columnHeader_subcopy.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_price
            // 
            columnHeader_price.Text = "单价";
            columnHeader_price.Width = 150;
            columnHeader_price.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_totalPrice
            // 
            columnHeader_totalPrice.Text = "总价";
            columnHeader_totalPrice.Width = 150;
            columnHeader_totalPrice.TextAlign = HorizontalAlignment.Right;
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
            // 
            // columnHeader_distribute
            // 
            columnHeader_distribute.Text = "馆藏分配";
            columnHeader_distribute.Width = 150;
            // 
            // columnHeader_acceptcopy
            // 
            columnHeader_acceptcopy.Text = "已到套数";
            columnHeader_acceptcopy.Width = 100;
            // 
            // columnHeader_acceptsubcopy
            // 
            columnHeader_acceptsubcopy.Text = "已到每套册数";
            columnHeader_acceptsubcopy.Width = 100;
            // 
            // columnHeader_acceptprice
            // 
            columnHeader_acceptprice.Text = "到书单价";
            columnHeader_acceptprice.Width = 100;
            // 
            // columnHeader_class
            // 
            columnHeader_class.Text = "类别";
            columnHeader_class.Width = 100;
            // 
            // columnHeader_comment
            // 
            columnHeader_comment.Text = "附注";
            columnHeader_comment.Width = 150;
            // 
            // columnHeader_sellerAddress
            // 
            columnHeader_sellerAddress.Text = "渠道地址";
            columnHeader_sellerAddress.Width = 200;
            // 
            // columnHeader_biblioRecpath
            // 
            columnHeader_biblioRecpath.Text = "种记录路径";
            columnHeader_biblioRecpath.Width = 200;
        }


        private void button_next_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_load)
            {
                this.tabControl_main.SelectedTab = this.tabPage_saveChange;
                // this.textBox_verify_itemBarcode.Focus();
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_saveChange)
            {
                this.tabControl_main.SelectedTab = this.tabPage_print;
                this.button_print_printOrderList.Focus();
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                MessageBox.Show(this, "已经在最后一个page");
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }

            // this.SetNextButtonEnable();
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_load)
            {
                this.SetNextButtonEnable();
                // this.button_next.Enabled = true;
                // 强制显示出原始数据列表，以便用户正确地关联概念
                this.tabControl_items.SelectedTab = this.tabPage_originItems;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_saveChange)
            {
                if (this.m_bEnabled == true)
                {
                    this.SetNextButtonEnable();
                    // this.button_next.Enabled = true;

                    // 强制显示出原始数据列表，以便用户正确地关联概念
                    this.tabControl_items.SelectedTab = this.tabPage_originItems;
                }
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                this.SetNextButtonEnable();
                // this.button_next.Enabled = false;
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }
        }

        /*public*/ class NamedListViewItems : List<ListViewItem>
        {
            public string Seller = "";
            // List<ListViewItem> Items = new List<ListViewItem>();
        }

        // 根据渠道(书商)名分入多个不同的List<ListViewItem>
        /*public*/ class NamedListViewItemsCollection : List<NamedListViewItems>
        {
            public void AddItem(string strSeller,
                ListViewItem item)
            {
                NamedListViewItems list = null;
                bool bFound = false;

                // 定位
                for(int i=0;i<this.Count;i++)
                {
                    list = this[i];
                    if (list.Seller == strSeller)
                    {
                        bFound = true;
                        break;
                    }
                }

                // 如果不存在，则新创建一个list
                if (bFound == false)
                {
                    list = new NamedListViewItems();
                    list.Seller = strSeller;
                    this.Add(list);
                }

                list.Add(item);
            }
        }

        // 打印订单
        private void button_print_printOrderList_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = PrintOrder("html", out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private static Stylesheet GenerateStyleSheet()
        {
            return new Stylesheet(
                new Fonts(
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 0 - The default font.
                        new FontSize() { Val = 11 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 1 - The bold font.
                        new Bold(),
                        new FontSize() { Val = 11 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 2 - The Italic font.
                        new Italic(),
                        new FontSize() { Val = 11 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 2 - The Times Roman font. with 16 size
                        new FontSize() { Val = 16 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Times New Roman" })
                ),
                new Fills(
                    new Fill(                                                           // Index 0 - The default fill.
                        new PatternFill() { PatternType = PatternValues.None }),
                    new Fill(                                                           // Index 1 - The default fill of gray 125 (required)
                        new PatternFill() { PatternType = PatternValues.Gray125 }),
                    new Fill(                                                           // Index 2 - The yellow fill.
                        new PatternFill(
                            new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "FFFFFF00" } }
                        ) { PatternType = PatternValues.Solid })
                ),
                new Borders(
                    new Border(                                                         // Index 0 - The default border.
                        new LeftBorder(),
                        new RightBorder(),
                        new TopBorder(),
                        new BottomBorder(),
                        new DiagonalBorder()),
                    new Border(                                                         // Index 1 - Applies a Left, Right, Top, Bottom border to a cell
                        new LeftBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new RightBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new TopBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new BottomBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder())
                ),
                new CellFormats(
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 },                          // Index 0 - The default cell style.  If a cell does not have a style index applied it will use this style combination instead
                    new CellFormat() { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 1 - Bold 
                    new CellFormat() { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 2 - Italic
                    new CellFormat() { FontId = 3, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 3 - Times Roman
                    new CellFormat() { FontId = 0, FillId = 2, BorderId = 0, ApplyFill = true },       // Index 4 - Yellow Fill
                    new CellFormat(                                                                   // Index 5 - Alignment
                        new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }
                    ) { /*FontId = 1, FillId = 0, BorderId = 0, */ApplyAlignment = true },
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }      // Index 6 - Border
                )
            ); // return
        }

        string ExportExcelFilename = "";

        // 打印订单
        // parameters:
        //      strStyle    excel / html 之一或者逗号联接组合。 excel: 输出 Excel 文件
        int PrintOrder(
            string strStyle,
            out string strError)
        {
            strError = "";

            int nErrorCount = 0;

            ExcelDocument doc = null;

            if (StringUtil.IsInList("excel", strStyle) == true)
            {
                // 询问文件名
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.Title = "请指定要输出的 Excel 文件名";
                dlg.CreatePrompt = false;
                dlg.OverwritePrompt = true;
                dlg.FileName = this.ExportExcelFilename;
                // dlg.InitialDirectory = Environment.CurrentDirectory;
                dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return 0;

                this.ExportExcelFilename = dlg.FileName;

#if NO
                // string filepath = Path.Combine(this.MainForm.UserDir, "test.xlsx");
                SpreadsheetDocument spreadsheetDocument = null;
                spreadsheetDocument = SpreadsheetDocument.Create(this.ExportExcelFilename, SpreadsheetDocumentType.Workbook);

                doc = new ExcelDocument(spreadsheetDocument);
#endif
                try
                {
                    doc = ExcelDocument.Create(this.ExportExcelFilename);
                }
                catch (Exception ex)
                {
                    strError = "PrintOrderForm ExcelDocument.Create() exception: " + ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

                doc.Stylesheet = GenerateStyleSheet();
                // doc.Initial();
            }

            this.tabControl_items.SelectedTab = this.tabPage_mergedItems;

            EnableControls(false);
            // MainForm.ShowProgress(true);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在构造订单 ...");
            stop.BeginLoop();

            try
            {

                NamedListViewItemsCollection lists = new NamedListViewItemsCollection();

                // 先检查是否有错误事项，顺便构建item列表
                // List<ListViewItem> items = new List<ListViewItem>();
                for (int i = 0; i < this.listView_merged.Items.Count; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    ListViewItem item = this.listView_merged.Items[i];

                    if (item.ImageIndex == TYPE_ERROR)
                        nErrorCount++;

                    lists.AddItem(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                        item);
                }

                if (nErrorCount != 0)
                {
                    MessageBox.Show(this, "警告：这里打印出的清单，含有 " + nErrorCount.ToString() + " 个包含错误信息的事项。");
                }

                List<string> filenames = new List<string>();
                try
                {
                    // 按渠道打印订单
                    for (int i = 0; i < lists.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        List<string> temp_filenames = null;
                        int nRet = PrintMergedList(lists[i],
                            ref doc,
                            out temp_filenames,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        filenames.AddRange(temp_filenames);
                    }

                    for (int i = 0; i < lists.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        // 按渠道打印分类统计页
                        List<string> temp_filenames = null;
                        int nRet = PrintClassStatisList(
                            "class",
                            lists[i],
                            ref doc,
                            out temp_filenames,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (temp_filenames == null || temp_filenames.Count == 0)
                            continue;

                        Debug.Assert(temp_filenames != null);
                        filenames.AddRange(temp_filenames);

                        // 按渠道打印出版社统计页
                        temp_filenames = null;
                        nRet = PrintClassStatisList(
                            "publisher",
                            lists[i],
                            ref doc,
                            out temp_filenames,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (temp_filenames == null || temp_filenames.Count == 0)
                            continue;

                        Debug.Assert(temp_filenames != null);
                        filenames.AddRange(temp_filenames);
                    }

                    if (doc == null)
                    {
                        HtmlPrintForm printform = new HtmlPrintForm();

                        printform.Text = "打印订单";
                        printform.MainForm = this.MainForm;
                        printform.Filenames = filenames;
                        this.MainForm.AppInfo.LinkFormState(printform, "printorder_htmlprint_formstate");
                        printform.ShowDialog(this);
                        this.MainForm.AppInfo.UnlinkFormState(printform);
                    }
                }
                finally
                {
                    if (filenames != null)
                    {
                        Global.DeleteFiles(filenames);
                        filenames.Clear();
                    }
                }
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

            //END1:
            if (doc != null)
            {
                // Close the document.
                doc.Close();
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 打印一个渠道的分类统计表
        int PrintClassStatisList(
            string strStatisType,
            NamedListViewItems items,
            ref ExcelDocument doc,
            out List<string> filenames,
            out string strError)
        {
            strError = "";
            filenames = null;
            bool bError = true;


            // 创建一个html文件，以便函数返回后显示在HtmlPrintForm中。

            try
            {
                // Debug.Assert(false, "");

                // 构造html页面
                int nRet = BuildStatisHtml(
                    strStatisType,
                    items,
                    ref doc,
                    out filenames,
                    out strError);
                if (nRet == -1)
                    return -1;
                bError = false;
            }
            finally
            {
                // 错误处理
                if (filenames != null && bError == true)
                {
                    Global.DeleteFiles(filenames);
                    filenames.Clear();
                }
            }

            return 0;
        }

        // 打印一个渠道的订单
        int PrintMergedList(NamedListViewItems items,
            ref ExcelDocument doc,
            out List<string> html_filenames,
            out string strError)
        {
            strError = "";
            html_filenames = null;
            bool bError = true;

            // 创建一个html文件，以便函数返回后显示在HtmlPrintForm中。

            try
            {
                // Debug.Assert(false, "");

                // 构造html页面
                int nRet = BuildMergedHtml(
                    items,
                    ref doc,
                    out html_filenames,
                    out strError);
                if (nRet == -1)
                    return -1;
                bError = false;
            }
            finally
            {
                // 错误处理
                if (html_filenames != null && bError == true)
                {
                    Global.DeleteFiles(html_filenames);
                    html_filenames.Clear();
                }
            }

            return 0;
        }

        private void button_merged_print_option_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            string strNamePath = "printorder_printoption";

            PrintOrderPrintOption option = new PrintOrderPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);


            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.MainForm = this.MainForm;
            dlg.Text = this.comboBox_load_type.Text + " 订单 打印参数";
            dlg.PrintOption = option;
            dlg.DataDir = this.MainForm.DataDir;
            dlg.ColumnItems = new string[] {
                "no -- 序号",
                "seller -- 渠道",
                "catalogNo -- 书目号",
                "summary -- 摘要",
                "isbnIssn -- ISBN/ISSN",
                "mergeComment -- 合并注释",
                "range -- 时间范围",
                "issueCount -- 包含期数",
                "copy -- 复本数",
                                "subcopy -- 每套册数",
                                "series -- 套数",

                "price -- 单价",
                "totalPrice -- 总价格",
                "orderTime -- 订购时间",
                "orderID -- 订单号",
                "distribute -- 馆藏分配",
                "acceptCopy -- 已到套数",  // 2012/8/29
                "class -- 类别",

                "comment -- 注释",

                "sellerAddress -- 渠道地址",
                "sellerAddress:zipcode -- 渠道地址:邮政编码",
                "sellerAddress:address -- 渠道地址:地址",
                "sellerAddress:department -- 渠道地址:单位",
                "sellerAddress:name -- 渠道地址:联系人",
                "sellerAddress:tel -- 渠道地址:电话",
                "sellerAddress:email -- 渠道地址:Email地址",
                "sellerAddress:bank -- 渠道地址:开户行",
                "sellerAddress:accounts -- 渠道地址:银行账号",
                "sellerAddress:payStyle -- 渠道地址:汇款方式",
                "sellerAddress:comment -- 渠道地址:附注",

                "biblioRecpath -- 种记录路径"
            };


            this.MainForm.AppInfo.LinkFormState(dlg, "printorder_printoption_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(this.MainForm.AppInfo,
                strNamePath);
        }

        // 将分类号表文件装载到内存
        int LoadClassTable(string strFilename,
            out List<StatisLine> lines,
            out string strError)
        {
            lines = new List<StatisLine>();
            strError = "";

            try
            {
                using (StreamReader sr = new StreamReader(strFilename))
                {
                    for (int i=0; ; i++)
                    {
                        string strLine = "";
                        strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();

                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        StatisLine line = new StatisLine();
                        if (strLine[0] == '!')
                        {
                            line.Class = strLine.Substring(1);
                            line.AllowSum = false;
                        }
                        else
                        {
                            line.Class = strLine;
                            line.AllowSum = true;
                        }
                        lines.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "读取文件 '" + strFilename + "' 出错：" + ex.Message;
                    return -1;
            }

            return 0;
        }

#if NO
        public static void CreateDefaultClassFilterFile(string strFileName)
        {
            StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8);
            sw.WriteLine("using System;");
            sw.WriteLine("using System.Windows.Forms;");
            sw.WriteLine("using System.IO;");
            sw.WriteLine("using System.Text;");
            sw.WriteLine("using System.Xml;");
            sw.WriteLine("");

            //sw.WriteLine("using DigitalPlatform.MarcDom;");
            //sw.WriteLine("using DigitalPlatform.Statis;");
            sw.WriteLine("using dp2Circulation;");

            sw.WriteLine("using DigitalPlatform.Xml;");


            sw.WriteLine("public class MyOutputOrder : OutputOrder");

            sw.WriteLine("{");

            sw.WriteLine("	public override void Output()");
            sw.WriteLine("	{");
            sw.WriteLine("	}");


            sw.WriteLine("}");
            sw.Close();
        }
#endif

        static bool ComparePublisher(string strText1, string strText2)
        {
            if (strText1 != null)
                strText1 = strText1.ToLower().Replace(" ", "");

            if (strText2 != null)
                strText2 = strText2.ToLower().Replace(" ", "");

            return string.Compare(strText1, strText2) == 0;
        }

        // 用分类号匹配统计结果行。
        // parameters:
        //      lines   匹配模式行
        //      bExactMatch 是否精确匹配。如果 == false，表示前方一致
        //      bPublisherMatch 是否为出版社名匹配方式。所谓出版社名匹配方式就是忽略空格，忽略大小写
        // return:
        //      -1  出错
        //      >=0 匹配上的行数
        int MatchStatisLine(string strClass,
            List<StatisLine> lines,
            bool bExactMatch,
            bool bPublisherMatch,
            out List<StatisLine> results,
            out string strError)
        {
            strError = "";
            results = new List<StatisLine>();
            foreach (StatisLine line in lines)
            {
                if (line.Class == "*")
                {
                    results.Add(line);
                    continue;
                }

                if (bExactMatch == true)
                {
                    if (bPublisherMatch == true)
                    {
                        if (ComparePublisher(strClass, line.Class) == true)
                            results.Add(line);
                    }
                    else if (strClass == line.Class)
                    {
                        results.Add(line);
                    }
                }
                else
                {
                    if (bPublisherMatch == true)
                    {
                        if (StringUtil.HasHead(strClass.ToLower().Replace(" ", ""), line.Class.ToLower().Replace(" ", "")) == true)
                            results.Add(line);
                    } 
                    else if (StringUtil.HasHead(strClass, line.Class) == true)
                    {
                        results.Add(line);
                    }
                }
            }

            return results.Count;
        }

        // 用于匹配上 “其他” 这一行
        int MatchOtherLine(string strClass,
    List<StatisLine> lines,
    out List<StatisLine> results,
    out string strError)
        {
            strError = "";
            results = new List<StatisLine>();
            foreach (StatisLine line in lines)
            {
                if (strClass == line.Class)
                {
                    results.Add(line);
                }
            }

            return results.Count;
        }

        // 构造分类统计html页面
        // parameters:
        //      strStatisType   统计表类型 "class" "publisher"
        int BuildStatisHtml(
            string strStatisType,
            NamedListViewItems items,
            ref ExcelDocument doc,
            out List<string> filenames,
            out string strError)
        {
            strError = "";
            filenames = null;

            int nRet = 0;

            // 获得打印参数
            PrintOrderPrintOption option = new PrintOrderPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                "printorder_printoption");

#if NO
            // 准备一般的 MARC 过滤器
            {
                string strMarcFilterFilePath = option.GetTemplatePageFilePath("MARC过滤器");
                if (String.IsNullOrEmpty(strMarcFilterFilePath) == false)
                {
                    Debug.Assert(String.IsNullOrEmpty(strMarcFilterFilePath) == false, "");
                    nRet = PrepareMarcFilter(strMarcFilterFilePath, out strError);
                    if (nRet == -1)
                        return -1;
                }
            }
#endif

            List<StatisLine> main_lines = null; // 主要模式行数组
            List<StatisLine> secondary_lines = null; // 次要模式行数组

            string strStatisTypeName = "";
            if (strStatisType == "class")
            {
                // 观察模板文件是否存在
                string strTemplateFilePath = option.GetTemplatePageFilePath("分类号表");
                if (String.IsNullOrEmpty(strTemplateFilePath) == true)
                    return 0;   // 没有必要打印

                // 将分类号表文件装载到内存
                nRet = LoadClassTable(strTemplateFilePath,
                    out main_lines,
                    out strError);
                if (nRet == -1)
                    return -1;
                strStatisTypeName = "分类号";
            }
            else if (strStatisType == "publisher")
            {
                // 观察模板文件是否存在
                string strTemplateFilePath = option.GetTemplatePageFilePath("出版社表");
                if (String.IsNullOrEmpty(strTemplateFilePath) == true)
                    return 0;   // 没有必要打印

                // 将出版社表文件装载到内存
                nRet = LoadClassTable(strTemplateFilePath,
                    out main_lines,
                    out strError);
                if (nRet == -1)
                    return -1;
                strStatisTypeName = "出版社";

                //
                // 观察次要模板文件是否存在
                string strSecondaryTemplateFilePath = option.GetTemplatePageFilePath("分类号表");
                if (String.IsNullOrEmpty(strSecondaryTemplateFilePath) == false)
                {
                    // 将分类号表文件装载到内存
                    nRet = LoadClassTable(strSecondaryTemplateFilePath,
                        out secondary_lines,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

            }
            else
            {
                strError = "未知的 strStatisType 类型  '" + strStatisType + "'";
                return -1;
            }

            // 准备 publisher 方式下特定的 MARC 过滤器
            // 注：如果因为多种用途定义了一个 MARC 过滤器，它要满足 publisher 这里的要求，可参见 default_getclass.fltx
            if (this.MarcFilter == null)
            {
                // 
                string strMarcFilterFilePath = option.GetTemplatePageFilePath("MARC过滤器");
                if (String.IsNullOrEmpty(strMarcFilterFilePath) == true)
                {
                    /*
                    // 如果没有配置MARC过滤器，则使用一个缺省的能支持UNIMARC和USMARC的提取中图法类号的过滤器
                    strMarcFilterFilePath = PathUtil.MergePath(this.MainForm.DataDir, "~printorder_default_class_filter.fltx");
                    CreateDefaultClassFilterFile(strMarcFilterFilePath);
                     * */
                    strMarcFilterFilePath = PathUtil.MergePath(this.MainForm.DataDir, "default_getclass.fltx");
                }
                if (File.Exists(strMarcFilterFilePath) == false)
                {
                    strError = "MARC过滤器文件 '" + strMarcFilterFilePath + "' 不存在，创建" + strStatisTypeName + "统计页失败";
                    return -1;
                }

                Debug.Assert(String.IsNullOrEmpty(strMarcFilterFilePath) == false, "");

                {
                    nRet = PrepareMarcFilter(strMarcFilterFilePath, out strError);
                    if (nRet == -1)
                        return -1;
                }
            }

            Hashtable macro_table = new Hashtable();
            macro_table["%batchno%"] = this.BatchNo; // 批次号
            macro_table["%seller%"] = items.Seller; // 渠道名
            macro_table["%date%"] = DateTime.Now.ToLongDateString();
            macro_table["%pageno%"] = "1";
            macro_table["%pagecount%"] = "1";

            macro_table["%recpathfilepath%"] = this.RecPathFilePath;
            string strRecPathFilename = Path.GetFileName(this.RecPathFilePath);
            macro_table["%recpathfilename%"] = strRecPathFilename;

            // 批次号或文件名 多个宏不方便判断后选用，只好合并为一个宏
            macro_table["%batchno_or_recpathfilename%"] = String.IsNullOrEmpty(this.BatchNo) == true ? strRecPathFilename : this.BatchNo;

            string strCssUrl = GetAutoCssUrl(option, "printorder.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            macro_table["%link%"] = strLink;

            string strResult = "";

            // 准备模板页
            string strStatisTemplateFilePath = "";
            string strSheetName = "";
            string strTableTitle = "";

            if (strStatisType == "class")
            {
                strTableTitle = "%date% %seller% 分类统计表";
                if (this.checkBox_print_accepted.Checked == false)
                {
                    strSheetName = "分类统计页";
                    strStatisTemplateFilePath = option.GetTemplatePageFilePath("分类统计页");
                }
                else
                {
                    strSheetName = "分类统计页(含验收)";    // 不允许使用方括号
                    strStatisTemplateFilePath = option.GetTemplatePageFilePath("分类统计页[含验收]");
                }

                if (String.IsNullOrEmpty(strStatisTemplateFilePath) == true)
                {
                    if (this.checkBox_print_accepted.Checked == false)
                        strStatisTemplateFilePath = PathUtil.MergePath(this.MainForm.DataDir, "default_printorder_classstatis.template");
                    else
                        strStatisTemplateFilePath = PathUtil.MergePath(this.MainForm.DataDir, "default_printorder_classstatis_accept.template");
                }

            }
            else if (strStatisType == "publisher")
            {
                strTableTitle = "%date% %seller% 出版社统计表";
                if (this.checkBox_print_accepted.Checked == false)
                {
                    strSheetName = "出版社统计页";
                    strStatisTemplateFilePath = option.GetTemplatePageFilePath("出版社统计页");
                }
                else
                {
                    strSheetName = "出版社统计页(含验收)"; // 不能使用方括号
                    strStatisTemplateFilePath = option.GetTemplatePageFilePath("出版社统计页[含验收]");
                }

                if (String.IsNullOrEmpty(strStatisTemplateFilePath) == true)
                {
                    if (this.checkBox_print_accepted.Checked == false)
                        strStatisTemplateFilePath = PathUtil.MergePath(this.MainForm.DataDir, "default_printorder_publisherstatis.template");
                    else
                        strStatisTemplateFilePath = PathUtil.MergePath(this.MainForm.DataDir, "default_printorder_publisherstatis_accept.template");
                }
            }

            strTableTitle = StringUtil.MacroString(macro_table,
    strTableTitle); 

            Debug.Assert(String.IsNullOrEmpty(strStatisTemplateFilePath) == false, "");

            if (File.Exists(strStatisTemplateFilePath) == false)
            {
                strError = strStatisTypeName + "统计模板文件 '" + strStatisTemplateFilePath + "' 不存在，创建" + strStatisTypeName + "统计页失败";
                return -1;
            }
            {
                // 根据模板打印
                string strContent = "";
                // 能自动识别文件内容的编码方式的读入文本文件内容模块
                // return:
                //      -1  出错
                //      0   文件不存在
                //      1   文件存在
                nRet = Global.ReadTextFileContent(strStatisTemplateFilePath,
                    out strContent,
                    out strError);
                if (nRet == -1)
                    return -1;

                strResult = StringUtil.MacroString(macro_table,
                    strContent);
            }

            filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名

            // 需要将属于不同渠道的文件名前缀区别开来
            string strFileName = this.MainForm.DataDir + "\\~printorder_" + items.GetHashCode().ToString() + "_" + strStatisType + "statis";

            filenames.Add(strFileName);

            Sheet sheet = null;
            if (doc != null)
                sheet = doc.NewSheet(strSheetName);
            
            bool bWiledMatched = false; // 是否遇到过通配符

            stop.SetProgressValue(0);
            stop.SetProgressRange(0, items.Count);

            stop.SetMessage("正在遍历合并行 ...");
            for (int i = 0; i < items.Count; i++)
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                ListViewItem item = items[i];

                // 获得种记录中的主要分类依据。分类号或者出版社名
                string strKey = ""; // 主要分类依据
                string strSecondaryKey = "";    // 次要分类依据
                if (this.MarcFilter != null)
                {
                    string strMARC = "";
                    string strOutMarcSyntax = "";

                    // 获得MARC格式书目记录
                    string strBiblioRecPath = ListViewUtil.GetItemText(item, MERGED_COLUMN_BIBLIORECPATH);
                    // 获得MARC格式书目记录
                    // return:
                    //      -1  出错
                    //      0   空记录
                    //      1   成功
                    nRet = GetMarc(strBiblioRecPath,
                        out strMARC,
                        out strOutMarcSyntax,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    this.ColumnTable.Clear();   // 清除上一记录处理时残余的内容

                    if (nRet != 0)
                    {
                        // 触发filter中的Record相关动作
                        nRet = this.MarcFilter.DoRecord(
                            null,
                            strMARC,
                            strOutMarcSyntax,
                            i,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (strStatisType == "class")
                            strKey = (string)this.ColumnTable["biblioclass"];
                        else if (strStatisType == "publisher")
                        {
                            strKey = (string)this.ColumnTable["bibliopublisher"];
                            strSecondaryKey = (string)this.ColumnTable["biblioclass"];
                        }
                    }

                    stop.SetProgressValue(i + 1);
                }

                // 匹配行
                List<StatisLine> results = null;
                // 用分类号匹配统计结果行。
                // return:
                //      -1  出错
                //      >=0 匹配上的行数
                nRet = MatchStatisLine(strKey,
                    main_lines,
                    strStatisType == "class" ? false : true,
                    strStatisType == "class" ? false : true,
                    out results,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    // 如果没有命中任何行

                    // 试图找到 Class 为 “其他”的行 2013/1/8
                    nRet = MatchOtherLine("其他",
                        main_lines,
                        out results,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        StatisLine line = new StatisLine();
                        line.Class = "其他";
                        main_lines.Add(line);

                        results.Add(line);
                    }
                }
                else
                {
                    if (results.Count == 1 && results[0].Class == "*")
                    {
                        StatisLine line = new StatisLine();
                        line.Class = strKey;
                        main_lines.Add(line);

                        results[0] = line;
                        bWiledMatched = true;
                    }
                }

                // 创建次要行
                List<StatisLine> new_results = new List<StatisLine>();
                if (secondary_lines != null)
                {

                    foreach (StatisLine line in results)
                    {
                        if (line.InnerLines == null)
                        {
                            line.InnerLines = new List<StatisLine>();
                            // 新创立一个数组
                            foreach (StatisLine l in secondary_lines)
                            {
                                StatisLine n = new StatisLine();
                                n.Class = l.Class;
                                line.InnerLines.Add(n);
                            }
                        }

                        List<StatisLine> temp_results = null;
                        // 匹配行
                        // 用分类号匹配统计结果行。
                        // return:
                        //      -1  出错
                        //      >=0 匹配上的行数
                        nRet = MatchStatisLine(strSecondaryKey,
                            line.InnerLines,
                            false,
                            false,
                            out temp_results,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                        {
                            // 如果没有命中任何行

                            // 试图找到 Class 为 “其他”的行 2013/1/8
                            nRet = MatchOtherLine("其他",
                                line.InnerLines,
                                out temp_results,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            if (nRet == 0)
                            {
                                StatisLine new_line = new StatisLine();
                                new_line.Class = "其他";
                                line.InnerLines.Add(new_line);

                                temp_results.Add(new_line);
                            }
                        }
                        else
                        {
                            if (temp_results.Count == 1 && temp_results[0].Class == "*")
                            {
                                StatisLine new_line = new StatisLine();
                                new_line.Class = strKey;
                                line.InnerLines.Add(new_line);

                                temp_results[0] = new_line;
                                bWiledMatched = true;
                            }
                        }

                        new_results.AddRange(temp_results);
                    }


                }

                string strTotalPrice = ListViewUtil.GetItemText(item, MERGED_COLUMN_TOTALPRICE);
                string strCopy = ListViewUtil.GetItemText(item, MERGED_COLUMN_COPY);
                string strSubCopy = ListViewUtil.GetItemText(item, MERGED_COLUMN_SUBCOPY);

                string strAcceptCopy = ListViewUtil.GetItemText(item, MERGED_COLUMN_ACCEPTCOPY);
                string strAcceptSubCopy = ListViewUtil.GetItemText(item, MERGED_COLUMN_ACCEPTSUBCOPY);
                string strPrice = ListViewUtil.GetItemText(item, MERGED_COLUMN_PRICE);
                // string strAcceptPrice = ListViewUtil.GetItemText(item, MERGED_COLUMN_ACCEPTPRICE);

                // 套数
                int nSeries = 0;
                Int32.TryParse(strCopy, out nSeries);
                int nAcceptSeries = 0;
                Int32.TryParse(strAcceptCopy, out nAcceptSeries);
                int nSubCopy = 1;
                Int32.TryParse(strSubCopy, out nSubCopy);
                int nAcceptSubCopy = 1;
                Int32.TryParse(strAcceptSubCopy, out nAcceptSubCopy);

                ////
                nRet = AddValue(
                    1,
                    nSeries,
                    nSeries * nSubCopy,
                    strTotalPrice,
                    1,
                    nAcceptSeries,
                    nAcceptSeries * nAcceptSubCopy,
                    strPrice,
                    results,
                    out strError);

                if (new_results.Count > 0)
                {
                    nRet = AddValue(
    1,
    nSeries,
    nSeries * nSubCopy,
    strTotalPrice,
    1,
    nAcceptSeries,
    nAcceptSeries * nAcceptSubCopy,
    strPrice,
    new_results,
    out strError);
                }
            }

            string strTableContent = "<table class='" + strStatisType + "statis'>";

            // 栏目标题行
            {
                #region 输出 HTML

                strTableContent += "<tr class='column'>";
                strTableContent += "<td class='class'>" + strStatisTypeName + "</td>";
                strTableContent += "<td class='bibliocount'>种数</td>";
                strTableContent += "<td class='seriescount'>套数</td>";
                strTableContent += "<td class='itemcount'>册数</td>";
                strTableContent += "<td class='orderprice'>订购价</td>";
                if (this.checkBox_print_accepted.Checked == true)
                {
                    strTableContent += "<td class='accept_bibliocount'>已到种数</td>";
                    strTableContent += "<td class='accept_seriescount'>已到套数</td>";
                    strTableContent += "<td class='accept_itemcount'>已到册数</td>";
                    strTableContent += "<td class='accept_orderprice'>已到订购价</td>";
                }

                #endregion

                #region 输出 Excel

                if (doc != null)
                {
                    int nColIndex = 0;

                    List<string> cols = new List<string>();
                    cols.Add(strStatisTypeName);
                    cols.Add("种数");
                    cols.Add("套数");
                    cols.Add("册数");
                    cols.Add("订购价");

                    if (this.checkBox_print_accepted.Checked == true)
                    {
                        cols.Add("已到种数");
                        cols.Add("已到套数");
                        cols.Add("已到册数");
                        cols.Add("已到订购价");
                    }

                    // 输出标题
                    doc.WriteExcelTitle(0,
    cols.Count,
    strTableTitle,
    5);

                    foreach (string s in cols)
                    {
                        doc.WriteExcelCell(
            2,
            nColIndex++,
            s,
            true);
                    }
                }

                #endregion
            }

            string strSumPrice = "";
            long lBiblioCount = 0;
            long lSeriesCount = 0;
            long lItemCount = 0;

            string strAcceptSumPrice = "";
            long lAcceptBiblioCount = 0;
            long lAcceptSeriesCount = 0;
            long lAcceptItemCount = 0;

            if (bWiledMatched == true)
            {
                main_lines.Sort(new CellStatisLineComparer());
            }

            // 嵌套的子表
            List<InnerTableLine> inner_tables = new List<InnerTableLine>();

            int nExcelLineIndex = 3;
            stop.SetMessage("正在输出统计页 HTML ...");
            foreach (StatisLine line in main_lines)
            {
                if (line.Class == "*")
                    continue;

                string strCurrentPrices = "";
                // 2012/3/7
                // 将形如"-123.4+10.55-20.3"的价格字符串归并汇总
                nRet = PriceUtil.SumPrices(line.Price,
        out strCurrentPrices,
        out strError);
                if (nRet == -1)
                    strCurrentPrices = strError;

                string strAcceptCurrentPrices = "";

                if (this.checkBox_print_accepted.Checked == true)
                {
                    // 将形如"-123.4+10.55-20.3"的价格字符串归并汇总
                    nRet = PriceUtil.SumPrices(line.AcceptPrice,
            out strAcceptCurrentPrices,
            out strError);
                    if (nRet == -1)
                        strAcceptCurrentPrices = strError;
                }

                string strNoSumClass = "";
                if (line.AllowSum == false)
                    strNoSumClass = " nosum";
                else
                    strNoSumClass = " sum";

                #region 输出 HTML

                strTableContent += "<tr class='content" + HttpUtility.HtmlEncode(strNoSumClass) + "'>";
                strTableContent += "<td class='class'>" + HttpUtility.HtmlEncode(line.Class) + "</td>";
                strTableContent += "<td class='bibliocount'>" + GetTdValueString(line.BiblioCount) + "</td>";
                strTableContent += "<td class='seriescount'>" + GetTdValueString(line.SeriesCount) + "</td>";
                strTableContent += "<td class='itemcount'>" + GetTdValueString(line.ItemCount) + "</td>";
                strTableContent += "<td class='orderprice'>" + HttpUtility.HtmlEncode(strCurrentPrices) + "</td>";
                if (this.checkBox_print_accepted.Checked == true)
                {
                    strTableContent += "<td class='accept_bibliocount'>" + GetTdValueString(line.AcceptBiblioCount) + "</td>";
                    strTableContent += "<td class='accept_seriescount'>" + GetTdValueString(line.AcceptSeriesCount) + "</td>";
                    strTableContent += "<td class='accept_itemcount'>" + GetTdValueString(line.AcceptItemCount) + "</td>";
                    strTableContent += "<td class='accept_orderprice'>" + HttpUtility.HtmlEncode(strAcceptCurrentPrices) + "</td>";
                }

                #endregion

                #region 输出 Excel

                if (doc != null)
                {
                    int nColIndex = 0;
                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
line.Class,
true);
                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
line.BiblioCount.ToString());
                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
line.SeriesCount.ToString());
                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
line.ItemCount.ToString());
                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
strCurrentPrices);

                    if (this.checkBox_print_accepted.Checked == true)
                    {
                        doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
line.AcceptBiblioCount.ToString());
                        doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
line.AcceptSeriesCount.ToString());
                        doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
line.AcceptItemCount.ToString());
                        doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
strAcceptCurrentPrices);
                    }

                    nExcelLineIndex++;
                }

                #endregion

                if (secondary_lines != null
                    && line.InnerLines != null) // 2013/9/11
                {
                    InnerTableLine inner_line = new InnerTableLine();
                    inner_line.Key = line.Class;
                    inner_line.lines = line.InnerLines;
                    inner_line.AllowSum = line.AllowSum;
                    inner_tables.Add(inner_line);
                }

                strTableContent += "</tr>";

                if (line.AllowSum == true)
                {
                    strSumPrice = PriceUtil.JoinPriceString(strSumPrice,
    strCurrentPrices);
                    lBiblioCount += line.BiblioCount;
                    lSeriesCount += line.SeriesCount;
                    lItemCount += line.ItemCount;

                    if (this.checkBox_print_accepted.Checked == true)
                    {
                        strAcceptSumPrice = PriceUtil.JoinPriceString(strAcceptSumPrice,
                            strAcceptCurrentPrices);
                        lAcceptBiblioCount += line.AcceptBiblioCount;
                        lAcceptSeriesCount += line.AcceptSeriesCount;
                        lAcceptItemCount += line.AcceptItemCount;
                    }

                }
            }

            // 汇总行
            #region 输出 HTML
            {
                string strOutputPrice = "";
                nRet = PriceUtil.SumPrices(strSumPrice,
        out strOutputPrice,
        out strError);
                if (nRet == -1)
                    strOutputPrice = strError;

                strTableContent += "<tr class='totalize'>";
                strTableContent += "<td class='class'>合计</td>";
                strTableContent += "<td class='bibliocount'>" + GetTdValueString(lBiblioCount) + "</td>";
                strTableContent += "<td class='seriescount'>" + GetTdValueString(lSeriesCount) + "</td>";
                strTableContent += "<td class='itemcount'>" + GetTdValueString(lItemCount) + "</td>";
                strTableContent += "<td class='orderprice'>" + HttpUtility.HtmlEncode(strOutputPrice) + "</td>";

                if (this.checkBox_print_accepted.Checked == true)
                {
                    string strAcceptOutputPrice = "";
                    nRet = PriceUtil.SumPrices(strAcceptSumPrice,
            out strAcceptOutputPrice,
            out strError);
                    if (nRet == -1)
                        strAcceptOutputPrice = strError;

                    strTableContent += "<td class='accept_bibliocount'>" + GetTdValueString(lAcceptBiblioCount) + "</td>";
                    strTableContent += "<td class='accept_seriescount'>" + GetTdValueString(lAcceptSeriesCount) + "</td>";
                    strTableContent += "<td class='accept_itemcount'>" + GetTdValueString(lAcceptItemCount) + "</td>";
                    strTableContent += "<td class='accept_orderprice'>" + HttpUtility.HtmlEncode(strAcceptOutputPrice) + "</td>";
                }
            }
            #endregion

            #region 输出 Excel
            if (doc != null)
            {
                string strOutputPrice = "";
                nRet = PriceUtil.SumPrices(strSumPrice,
        out strOutputPrice,
        out strError);
                if (nRet == -1)
                    strOutputPrice = strError;

                int nColIndex = 0;
                doc.WriteExcelCell(
    nExcelLineIndex,
    nColIndex++,
    "合计",
    true);
                doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
lBiblioCount.ToString());

                doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
lSeriesCount.ToString());

                doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
lItemCount.ToString());

                doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
strOutputPrice); 

                if (this.checkBox_print_accepted.Checked == true)
                {
                    string strAcceptOutputPrice = "";
                    nRet = PriceUtil.SumPrices(strAcceptSumPrice,
            out strAcceptOutputPrice,
            out strError);
                    if (nRet == -1)
                        strAcceptOutputPrice = strError;

                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
lAcceptBiblioCount.ToString());

                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
lAcceptSeriesCount.ToString());
                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
lAcceptItemCount.ToString());
                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
strAcceptOutputPrice); 
                }
            }
            #endregion

            strTableContent += "</tr>";
            strTableContent += "</table>";

            strResult = strResult.Replace("{table}", strTableContent);

            // 内容行
            StreamUtil.WriteText(strFileName,
                strResult);

            if (secondary_lines != null)
            {
                strResult = "";

                // 准备模板页
                strStatisTemplateFilePath = "";
                if (strStatisType == "publisher")
                {
                    if (this.checkBox_print_accepted.Checked == false)
                    {
                        strSheetName = "出版社分类统计页";
                        strStatisTemplateFilePath = option.GetTemplatePageFilePath("出版社分类统计页");
                    }
                    else
                    {
                        strSheetName = "出版社分类统计页(含验收)"; // 不能使用方括号
                        strStatisTemplateFilePath = option.GetTemplatePageFilePath("出版社分类统计页[含验收]");
                    }

                    if (String.IsNullOrEmpty(strStatisTemplateFilePath) == true)
                    {
                        if (this.checkBox_print_accepted.Checked == false)
                            strStatisTemplateFilePath = PathUtil.MergePath(this.MainForm.DataDir, "default_printorder_publisherclassstatis.template");
                        else
                            strStatisTemplateFilePath = PathUtil.MergePath(this.MainForm.DataDir, "default_printorder_publisherclassstatis_accept.template");
                    }
                }


                Debug.Assert(String.IsNullOrEmpty(strStatisTemplateFilePath) == false, "");

                if (File.Exists(strStatisTemplateFilePath) == false)
                {
                    strError = strStatisTypeName + "统计模板文件 '" + strStatisTemplateFilePath + "' 不存在，创建" + strStatisTypeName + "的嵌套统计页失败";
                    return -1;
                }
                {
                    // 根据模板打印
                    string strContent = "";
                    // 能自动识别文件内容的编码方式的读入文本文件内容模块
                    // return:
                    //      -1  出错
                    //      0   文件不存在
                    //      1   文件存在
                    nRet = Global.ReadTextFileContent(strStatisTemplateFilePath,
                        out strContent,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    strResult = StringUtil.MacroString(macro_table,
                        strContent);
                }

                if (doc != null)
                    sheet = doc.NewSheet(strSheetName);

                strTableTitle = "%date% %seller% 出版社分类号统计表";
                strTableTitle = StringUtil.MacroString(macro_table,
                    strTableTitle);

                // 创建嵌套表格
                strTableContent = "";
                nRet = BuildSecondaryPage(
                    strStatisType,
                    strStatisTypeName,
                    inner_tables,
                    strTableTitle,
                    ref doc,
                    out strTableContent,
                    out strError);
                if (nRet == -1)
                    return -1;

                strResult = strResult.Replace("{table}", strTableContent);
                string strInnerFileName = this.MainForm.DataDir + "\\~printorder_" + items.GetHashCode().ToString() + "_" + strStatisType + "statis_inner";
                filenames.Add(strInnerFileName);
                StreamUtil.WriteText(strInnerFileName,
                    strResult);
            }
            return 0;
        }

        // 创建嵌套表格
        // parameters:
        //      strTableTitle   只有当输出 Excel 时需要
        // return:
        //      -1  出错
        //      0   成功
        int BuildSecondaryPage(
            string strStatisType,
            string strStatisTypeName,
            List<InnerTableLine> table,
            string strTableTitle,
            ref ExcelDocument doc,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            // int nTitleColCount = 0; // 列标题行的列数

            List<StatisLine> first_lines = null;    // 第一个分类号行数组
            foreach(InnerTableLine line in table)
            {
                if (line.lines != null && line.lines.Count > 0)
                {
                    first_lines = line.lines;
                    break;
                }
            }

            if (first_lines == null)
                return 0;   // 没有任何内容

            StringBuilder strTableContent = new StringBuilder(4096);
            strTableContent.Append("<table class='" + strStatisType + "extendstatis'>");

            // 栏目标题行
            {
                strTableContent.Append("<tr class='column'>");
                strTableContent.Append("<td class='class'>" + strStatisTypeName + "</td>");

                // 类号标题列
                foreach(StatisLine line in first_lines)
                {
                    strTableContent.Append("<td class='columntitle' colspan='2'>" + HttpUtility.HtmlEncode(line.Class) + "</td>");
                }
            }


            #region 输出 Excel

            if (doc != null)
            {
                int nColIndex = 0;

                // 表格标题
                doc.WriteExcelTitle(0,
first_lines.Count * 2 + 1,
strTableTitle,
5);

                doc.WriteExcelCell(
    2,
    nColIndex++,
    strStatisTypeName,
    true);
                // 类号标题列
                foreach (StatisLine line in first_lines)
                {
                    int nFirstCol = nColIndex;
                    doc.WriteExcelCell(
    2,
    nColIndex++,
    line.Class,
    true,
    5); // 居中
                    // 空白单元
                    doc.WriteExcelCell(
    2,
    nColIndex++,
    "",
    true);
                    // 把两个单元连接起来
                    doc.InsertMergeCell(
             2,
             nFirstCol,
             2);
                }

                // nTitleColCount = nColIndex;
            }
            #endregion

            List<StatisLine> sum_items = new List<StatisLine>();

            int nExcelLineIndex = 3;
            foreach (InnerTableLine inner_line in table)
            {
                if (inner_line.Key == "*")
                    continue;

                string strNoSumClass = "";
                if (inner_line.AllowSum == false)
                    strNoSumClass = " nosum";
                else
                    strNoSumClass = " sum";

                strTableContent.Append("<tr class='content" + HttpUtility.HtmlAttributeEncode(strNoSumClass) + "'>");
                strTableContent.Append("<td class='class'>" + HttpUtility.HtmlEncode(inner_line.Key) + "</td>");

                #region 输出 Excel

                int nColIndex = 0;
                if (doc != null)
                {
                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
inner_line.Key,
true);
                }

                #endregion

                int i = 0;
                foreach (StatisLine line in inner_line.lines)
                {
                    strTableContent.Append("<td class='bibliocount'>" + GetTdValueString(line.BiblioCount) + "</td>");
                    strTableContent.Append("<td class='itemcount'>" + GetTdValueString(line.ItemCount) + "</td>");

                    #region 输出 Excel

                    if (doc != null)
                    {
                        doc.WriteExcelCell(
    nExcelLineIndex,
    nColIndex++,
    line.BiblioCount.ToString());
                        doc.WriteExcelCell(
    nExcelLineIndex,
    nColIndex++,
    line.ItemCount.ToString());
                    }

                    #endregion

                    // 汇总
                    if (inner_line.AllowSum == true)
                    {
                        while (sum_items.Count < i + 1)
                        {
                            sum_items.Add(new StatisLine());
                        }
                        StatisLine sum_item = sum_items[i];
                        sum_item.BiblioCount += line.BiblioCount;
                        sum_item.ItemCount += line.ItemCount;
                    }

                    i++;
                }

                strTableContent.Append("</tr>");

                if (doc != null)
                    nExcelLineIndex++;
            }

            // 汇总行
            {
                strTableContent.Append("<tr class='totalize'>");
                strTableContent.Append("<td class='class'>合计</td>");

                #region 输出 Excel

                int nColIndex = 0;
                if (doc != null)
                {
                    doc.WriteExcelCell(
nExcelLineIndex,
nColIndex++,
"合计",
true);
                }

                #endregion
                foreach (StatisLine line in sum_items)
                {
                    strTableContent.Append("<td class='bibliocount'>" + GetTdValueString(line.BiblioCount) + "</td>");
                    strTableContent.Append("<td class='itemcount'>" + GetTdValueString(line.ItemCount) + "</td>");
                    #region 输出 Excel
                    if (doc != null)
                    {
                        doc.WriteExcelCell(
    nExcelLineIndex,
    nColIndex++,
    line.BiblioCount.ToString());
                        doc.WriteExcelCell(
    nExcelLineIndex,
    nColIndex++,
    line.ItemCount.ToString());
                    }
                    #endregion
                }
                strTableContent.Append("</tr>");
            }

            strTableContent.Append("</tr>");
            strTableContent.Append("</table>");

            strResult = strTableContent.ToString();
            return 0;   //  nTitleColCount;
        }

        class InnerTableLine
        {
            public bool AllowSum = false;
            public string Key = ""; // 左方标题
            public List<StatisLine> lines = null;   // 子表
        }

        string BuilInnerLine(List<StatisLine> lines)
        {
            StringBuilder result = new StringBuilder(4096);
            foreach (StatisLine line in lines)
            {
                if (line.BiblioCount == 0)
                    continue;   // 压缩没有值的行
                result.Append(line.Class + ":" + line.BiblioCount + ","
                    + line.SeriesCount + ","
                    + line.ItemCount + ","
                    + line.Price + ";");
            }

            return result.ToString();
        }

        int AddValue(
            int nBiblioCount,
            int nSeriesCount,
            int nItemCount,
            string strPrice,
            int nAcceptBiblioCount,
            int nAcceptSeriesCount,
            int nAcceptItemCount,
            string strAcceptPrice,
            List<StatisLine> results,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            foreach (StatisLine line in results)
            {
                if (line.Class == "*")
                    continue;

                line.BiblioCount += nBiblioCount;
                line.SeriesCount += nSeriesCount;
                line.ItemCount += nItemCount;
                line.Price = PriceUtil.JoinPriceString(line.Price,
                    strPrice);
                if (this.checkBox_print_accepted.Checked == true)
                {
                    if (nAcceptSeriesCount > 0)
                    {
                        line.AcceptBiblioCount += nAcceptBiblioCount;
                        line.AcceptSeriesCount += nAcceptSeriesCount;
                        line.AcceptItemCount += nAcceptItemCount;

                        string strTemp = "";
                        nRet = PriceUtil.MultiPrice(strAcceptPrice,
        nAcceptSeriesCount,
        out strTemp,
        out strError);
                        if (nRet != -1)
                        {
                            line.AcceptPrice = PriceUtil.JoinPriceString(line.AcceptPrice,
                                strTemp);
                        }
                        // TODO: 是否报错?
                    }
                }
            }

            return 0;
        }

        static string GetTdValueString(long v)
        {
            if (v == 0)
                return "&nbsp;";
            return v.ToString();
        }

        static string GetRatioString(double v1, double v2)
        {
            double ratio = v1 / v2;

            return String.Format("{0,3:N}", ratio * (double)100) + "%";
        }

        // 计算两个价格的比率
        static string GetRatioString(string strPrice1, string strPrice2)
        {
            if (strPrice1.IndexOfAny(new char[] { '+', '-', '*' }) != -1)
                return "无法计算比率";
            if (strPrice2.IndexOfAny(new char[] { '+', '-', '*' }) != -1)
                return "无法计算比率";

            if (string.IsNullOrEmpty(strPrice1) == true)
            {
                if (string.IsNullOrEmpty(strPrice2) == false)
                    return "0.00%";

                strPrice1 = "0.00";
            }

            if (string.IsNullOrEmpty(strPrice2) == true)
                strPrice2 = "0.00";

            string strError = "";
            string strPrefix1 = "";
            string strValue1 = "";
            string strPostfix1 = "";
            int nRet = PriceUtil.ParsePriceUnit(strPrice1,
                out strPrefix1,
                out strValue1,
                out strPostfix1,
                out strError);
            if (nRet == -1)
                return "strPrice1 '"+strPrice1+"' 格式错误: " + strError;

            decimal value1 = 0;
            try
            {
                value1 = Convert.ToDecimal(strValue1);
            }
            catch
            {
                strError = "数字 '" + strValue1 + "' 格式不正确";
                return strError;
            }

            string strPrefix2 = "";
            string strValue2 = "";
            string strPostfix2 = "";
            nRet = PriceUtil.ParsePriceUnit(strPrice2,
                out strPrefix2,
                out strValue2,
                out strPostfix2,
                out strError);
            if (nRet == -1)
                return "strPrice2 '" + strPrice2 + "' 格式错误: " + strError;

            decimal value2 = 0;
            try
            {
                value2 = Convert.ToDecimal(strValue2);
            }
            catch
            {
                strError = "数字 '" + strValue2 + "' 格式不正确";
                return strError;
            }

            if (strPrefix1 != strPrefix2)
            {
                return "strPrice1 '"+strPrice1+"' 和 strPrice2 '"+strPrice2+"' 的前缀不一致，无法计算比率";
            }

            if (strPostfix1 != strPostfix2)
            {
                return "strPrice1 '" + strPrice1 + "' 和 strPrice2 '" + strPrice2 + "' 的后缀不一致，无法计算比率";
            }

            return String.Format("{0,3:N}", ((double)value1 / (double)value2) * (double)100) + "%";
        }

        static void WriteExcelLine(WorkbookPart wp,
            Worksheet ws,
            int nLineIndex,
            string strName,
            string strValue,
            bool bString)
        {
            ExcelUtil.UpdateValue(
                wp,
                ws,
                "A" + (nLineIndex + 1).ToString(),
                strName,
                0,
                true);
            ExcelUtil.UpdateValue(
                wp,
                ws,
                "B" + (nLineIndex + 1).ToString(),
                strValue,
                0,
                bString);
        }

        // 构造html页面
        int BuildMergedHtml(
            NamedListViewItems items,
            ref ExcelDocument doc,
            out List<string> filenames,
            out string strError)
        {
            strError = "";
            filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名
            int nRet = 0;

            stop.SetMessage("正在构造订单 ...");

            Hashtable macro_table = new Hashtable();

            // 获得打印参数
            PrintOrderPrintOption option = new PrintOrderPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                "printorder_printoption");

            // 准备一般的 MARC 过滤器
            {
                string strMarcFilterFilePath = option.GetTemplatePageFilePath("MARC过滤器");
                if (String.IsNullOrEmpty(strMarcFilterFilePath) == false)
                {
                    Debug.Assert(String.IsNullOrEmpty(strMarcFilterFilePath) == false, "");
                    nRet = PrepareMarcFilter(strMarcFilterFilePath, out strError);
                    if (nRet == -1)
                        return -1;
                }
            }

            /*
            // 检查当前排序状态和包含种价格列之间是否存在矛盾
            if (bHasBiblioPriceColumn(option) == true)
            {

                if (this.SortColumns_in.Count != 0
                    && this.SortColumns_in[0].No == COLUMN_BIBLIORECPATH)
                {
                }
                else
                {
                    MessageBox.Show(this, "警告：打印列中要求了‘种价格’列，但打印前集合内事项并未按‘种记录路径’排序，这样打印出的‘种价格’栏内容将会不准确。\r\n\r\n要避免这种情况，可在打印前用鼠标左键点‘种记录路径’栏标题，确保按其排序。");
                }
            }*/


            // 计算出页总数
            int nTablePageCount = items.Count / option.LinesPerPage;
            if ((items.Count % option.LinesPerPage) != 0)
                nTablePageCount++;

            int nPageCount = nTablePageCount + 1;

            macro_table["%batchno%"] = this.BatchNo; // 批次号
            macro_table["%seller%"] = items.Seller; // 渠道名
            macro_table["%pagecount%"] = nPageCount.ToString();
            macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            macro_table["%recpathfilepath%"] = this.RecPathFilePath;
            string strRecPathFilename = Path.GetFileName(this.RecPathFilePath);
            macro_table["%recpathfilename%"] = strRecPathFilename;

            // 批次号或文件名 多个宏不方便判断后选用，只好合并为一个宏
            macro_table["%batchno_or_recpathfilename%"] = String.IsNullOrEmpty(this.BatchNo) == true ? strRecPathFilename : this.BatchNo;


            // filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名

            // 需要将属于不同渠道的文件名前缀区别开来
            string strFileNamePrefix = this.MainForm.DataDir + "\\~printorder_" + items.GetHashCode().ToString() + "_";

            string strFileName = "";

            // 初始化 Excel 文件
#if NO
            Sheets sheets = null;
            WorksheetPart worksheetPart = null;
            {
                // Add a WorkbookPart to the document.
                WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Add Sheets to the Workbook.
                sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
            }

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet() { 
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = (UInt32)filenames.Count + 1,
                Name = "统计页" };
            sheets.Append(sheet);
#endif

            Sheet sheet = null;

            if (doc != null)
                sheet = doc.NewSheet("统计页");

            // 输出信息页
            // TODO: 要增加“统计页”模板功能
            {
                int nItemCount = items.Count;
                int nTotalSeries = GetMergedTotalSeries(items);
                int nTotalCopies = GetMergedTotalCopies(items);
                int nBiblioCount = GetMergedBiblioCount(items);
                string strTotalPrice = GetMergedTotalPrice(items);

                macro_table["%itemcount%"] = nItemCount.ToString(); // 事项数
                macro_table["%totalcopies%"] = nTotalCopies.ToString(); // 总册数
                macro_table["%totalseries%"] = nTotalSeries.ToString(); // 总套数
                macro_table["%bibliocount%"] = nBiblioCount.ToString(); // 种数
                macro_table["%totalprice%"] = strTotalPrice;    // 总价格 可能为多个币种的价格串联形态

                // 已验收的
                int nAcceptItemCount = items.Count;
                int nAcceptTotalSeries = GetMergedAcceptTotalSeries(items);
                int nAcceptTotalCopies = GetMergedAcceptTotalCopies(items);
                int nAcceptBiblioCount = GetMergedAcceptBiblioCount(items);
                string strAcceptTotalPrice = GetMergedAcceptTotalPrice(items);

                macro_table["%accept_itemcount%"] = nAcceptItemCount.ToString(); // 事项数
                macro_table["%accept_totalcopies%"] = nAcceptTotalCopies.ToString(); // 总册数
                macro_table["%accept_totalseries%"] = nAcceptTotalSeries.ToString(); // 总套数
                macro_table["%accept_bibliocount%"] = nAcceptBiblioCount.ToString(); // 种数
                macro_table["%accept_totalprice%"] = strAcceptTotalPrice;    // 总价格 可能为多个币种的价格串联形态

                // 到货率
                macro_table["%ratio_itemcount%"] = GetRatioString(nAcceptItemCount, nItemCount); // 事项数
                macro_table["%ratio_totalcopies%"] = GetRatioString(nAcceptTotalCopies, nTotalCopies); // 总册数
                macro_table["%ratio_totalseries%"] = GetRatioString(nAcceptTotalSeries, nTotalSeries); // 总套数
                macro_table["%ratio_bibliocount%"] = GetRatioString(nAcceptBiblioCount, nBiblioCount); // 种数
                macro_table["%ratio_totalprice%"] = GetRatioString(strAcceptTotalPrice, strTotalPrice);    // 总价格 可能为多个币种的价格串联形态


                macro_table["%pageno%"] = "1";

                strFileName = strFileNamePrefix + "0" + ".html";

                filenames.Add(strFileName);

                BuildMergedPageTop(option,
                    macro_table,
                    strFileName,
                    false);

                // 内容行
                StreamUtil.WriteText(strFileName,
                    "<div class='seller'>渠道: " + HttpUtility.HtmlEncode(items.Seller) + "</div>");
                StreamUtil.WriteText(strFileName,
                    "<div class='bibliocount'>种数: " + nBiblioCount.ToString() + "</div>");
                StreamUtil.WriteText(strFileName,
                   "<div class='seriescount'>套数: " + nTotalSeries.ToString() + "</div>");  // 2009/1/5 changed
                StreamUtil.WriteText(strFileName,
                    "<div class='itemcount'>册数: " + nTotalCopies.ToString() + "</div>");  // 2009/1/5 changed
                StreamUtil.WriteText(strFileName,
                    "<div class='totalprice'>总价: " + HttpUtility.HtmlEncode(strTotalPrice) + "</div>");

                int nLineIndex = 2;
                
                if (doc != null)
                {
                    BuildMergedExcelPageTop(option,
                        macro_table,
                        ref doc,
                        4,
                        false);

                    doc.WriteExcelLine(
                    nLineIndex++,
                    "渠道",
                    items.Seller);

                    doc.WriteExcelLine(
    nLineIndex++,
        "种数",
        nBiblioCount.ToString());

                    doc.WriteExcelLine(
    nLineIndex++,
    "套数",
    nTotalSeries.ToString());

                    doc.WriteExcelLine(
    nLineIndex++,
    "册数",
    nTotalCopies.ToString());

                    doc.WriteExcelLine(
    nLineIndex++,
    "总价",
    strTotalPrice);
                }

                if (this.checkBox_print_accepted.Checked == true)
                {
                    StreamUtil.WriteText(strFileName,
    "<div class='accept_bibliocount'>已验收种数: " + nAcceptBiblioCount.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                       "<div class='accept_seriescount'>已验收套数: " + nAcceptTotalSeries.ToString() + "</div>");  // 2009/1/5 changed
                    StreamUtil.WriteText(strFileName,
                        "<div class='accept_itemcount'>已验收册数: " + nAcceptTotalCopies.ToString() + "</div>");  // 2009/1/5 changed
                    StreamUtil.WriteText(strFileName,
                        "<div class='accept_totalprice'>已验收总价: " + HttpUtility.HtmlEncode(strAcceptTotalPrice) + "</div>");

                    // 到货率
                    StreamUtil.WriteText(strFileName,
"<div class='ratio_bibliocount'>种数到货率: " + HttpUtility.HtmlEncode((string)macro_table["%ratio_bibliocount%"]) + "</div>");
                    StreamUtil.WriteText(strFileName,
                       "<div class='ratio_seriescount'>套数到货率: " + HttpUtility.HtmlEncode((string)macro_table["%ratio_totalseries%"]) + "</div>");  // 2009/1/5 changed
                    StreamUtil.WriteText(strFileName,
                        "<div class='ratio_itemcount'>册数到货率: " + HttpUtility.HtmlEncode((string)macro_table["%ratio_totalcopies%"]) + "</div>");  // 2009/1/5 changed
                    StreamUtil.WriteText(strFileName,
                        "<div class='ratio_totalprice'>总价到货率: " + HttpUtility.HtmlEncode((string)macro_table["%ratio_totalprice%"]) + "</div>");

                    if (doc != null)
                    {
                        doc.WriteExcelLine(
nLineIndex++,
"已验收种数",
nAcceptBiblioCount.ToString());
                        doc.WriteExcelLine(
nLineIndex++,
"已验收套数",
nAcceptTotalSeries.ToString());
                        doc.WriteExcelLine(
nLineIndex++,
"已验收册数",
nAcceptTotalCopies.ToString());
                        doc.WriteExcelLine(
nLineIndex++,
"已验收总价",
strAcceptTotalPrice);

                        doc.WriteExcelLine(
nLineIndex++,
"种数到货率",
(string)macro_table["%ratio_bibliocount%"]);

                        doc.WriteExcelLine(
nLineIndex++,
"套数到货率",
(string)macro_table["%ratio_totalseries%"]);

                        doc.WriteExcelLine(
nLineIndex++,
"册数到货率",
(string)macro_table["%ratio_totalcopies%"]);
                        doc.WriteExcelLine(
nLineIndex++,
"总价到货率",
(string)macro_table["%ratio_totalprice%"]);
                    }
                }

                BuildMergedPageBottom(option,
                    macro_table,
                    strFileName,
                    false);
            }

            if (doc != null)
            {
                sheet = doc.NewSheet("订单"); // "表1"
                BuildMergedExcelPageTop(option,
    macro_table,
    ref doc,
    option.Columns.Count,
    true);
            }

            // 表格页循环
            for (int i = 0; i < nTablePageCount; i++)
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

#if NO
                // Append a new worksheet and associate it with the workbook.
                sheet = new Sheet()
                {
                    Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = (UInt32)filenames.Count + 1,
                    Name = "表" + (i+1).ToString()
                };
                sheets.Append(sheet);
#endif


#if NO
                SheetInfo sheetinfo = new SheetInfo();
                sheetinfo.wp = spreadsheetDocument.WorkbookPart;
                sheetinfo.ws = worksheetPart.Worksheet;
#endif

                macro_table["%pageno%"] = (i + 1 + 1).ToString();

                strFileName = strFileNamePrefix + (i + 1).ToString() + ".html";

                filenames.Add(strFileName);

                BuildMergedPageTop(option,
                    macro_table,
                    strFileName,
                    true);

                // 行循环
                for (int j = 0; j < option.LinesPerPage; j++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    BuildMergedTableLine(option,
                        items,
                        strFileName,
                        doc,    // sheetinfo,
                        i, j, 3);
                }

                BuildMergedPageBottom(option,
                    macro_table,
                    strFileName,
                    true);
            }

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

        // 根据名字获得class名
        // 获得 "llll -- rrrrr"的左边部分，并且把左边部分可能有的参数部分丢弃
        /// <summary>
        /// 根据名字获得 class 名。
        /// 算法为获得 "llll -- rrrrr" 的左边部分，并且把左边部分可能有的参数部分丢弃
        /// 参数丢弃的算法是 "name(param1, param2)" --&gt; "name"
        /// </summary>
        /// <param name="strText">要处理的字符串</param>
        /// <returns>class 名部分</returns>
        public static string GetClass(string strText)
        {
            string strLeft = StringUtil.GetLeft(strText);
            string strName = "";
            string strParameters = "";
            ParseNameParam(strLeft,
                out strName,
                out strParameters);
            return strName;
        }

        // 解析 name(param1, param2) 这样的字符串
        /// <summary>
        /// 解析 name(param1, param2) 这样的字符串
        /// </summary>
        /// <param name="strText">要解析的字符串</param>
        /// <param name="strName">返回 name 部分</param>
        /// <param name="strParameters">返回 (param1, param2) 部分</param>
        public static void ParseNameParam(string strText,
            out string strName,
            out string strParameters)
        {
            strName = "";
            strParameters = "";

            int nRet = strText.IndexOf("(");
            if (nRet == -1)
            {
                strName = strText;
                return;
            }
            strName = strText.Substring(0, nRet).Trim();
            strParameters = strText.Substring(nRet + 1).Trim();
            nRet = strParameters.LastIndexOf(")");
            if (nRet == -1)
            {
                strName = strText;
                return;
            }
            strParameters = strParameters.Substring(0, nRet).Trim();
        }

        // 输出 Excel 页面头部信息
        int BuildMergedExcelPageTop(PrintOption option,
            Hashtable macro_table,
            ref ExcelDocument doc,
            int nTitleCols,
            bool bOutputTable)
        {

            // 页眉
            string strPageHeaderText = option.PageHeader;

            if (String.IsNullOrEmpty(strPageHeaderText) == false)
            {
                strPageHeaderText = StringUtil.MacroString(macro_table,
                    strPageHeaderText);

                /*
                StreamUtil.WriteText(strFileName,
                    "<div class='pageheader'>" + HttpUtility.HtmlEncode(strPageHeaderText) + "</div>");
                */
            }

            // 表格标题
            string strTableTitleText = option.TableTitle;

            // 第一行，表格标题
            if (String.IsNullOrEmpty(strTableTitleText) == false)
            {

                strTableTitleText = StringUtil.MacroString(macro_table,
                    strTableTitleText);

                doc.WriteExcelTitle(0,
                    nTitleCols,
                    strTableTitleText,
                    5);

            }

            if (bOutputTable == true)
            {
                for (int i = 0; i < option.Columns.Count; i++)
                {
                    Column column = option.Columns[i];

                    string strCaption = column.Caption;

                    // 如果没有caption定义，就挪用name定义
                    if (String.IsNullOrEmpty(strCaption) == true)
                        strCaption = column.Name;

                    string strClass = GetClass(column.Name);

                    if (this.checkBox_print_accepted.Checked == false)
                    {
                        // 不输出 已到套数 列
                        if (strClass == "acceptCopy")
                            continue;
                    }

                    doc.WriteExcelCell(
            2,
            i,
            strCaption,
            true);
                }
            }

            return 0;
        }

        int BuildMergedPageTop(PrintOption option,
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

            // string strCssUrl = this.MainForm.LibraryServerDir + "/printorder.css";
            // 2009/10/10 changed
            string strCssUrl = GetAutoCssUrl(option, "printorder.css");

            /*
            // 2009/10/9
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                strCssUrl = strCssFilePath;
            else
                strCssUrl = this.MainForm.LibraryServerDir + "/printorder.css";    // 缺省的
             * */

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
                    "<div class='pageheader'>" + HttpUtility.HtmlEncode(strPageHeaderText) + "</div>");

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
                    "<div class='tabletitle'>" + HttpUtility.HtmlEncode(strTableTitleText) + "</div>");
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

                    string strClass = GetClass(column.Name);

                    if (this.checkBox_print_accepted.Checked == false)
                    {
                        // 不输出 已到套数 列
                        if (strClass == "acceptCopy")
                            continue;
                    }

                    StreamUtil.WriteText(strFileName,
                        "<td class='" + strClass + "'>" + HttpUtility.HtmlEncode(strCaption) + "</td>");
                }

                StreamUtil.WriteText(strFileName,
                    "</tr>");

            }

            return 0;
        }

#if NO
        class SheetInfo
        {
            public WorkbookPart wp = null;
            public Worksheet ws = null;
        }
#endif

        // parameters:
        //      nTopBlankLines  Excel 页面上方留出的空白行数。空白行用于表格标题和栏标题
        int BuildMergedTableLine(PrintOption option,
            List<ListViewItem> items,
            string strFileName,
            // SheetInfo sheetinfo,
            ExcelDocument doc,
            int nPage,
            int nLine,
            int nTopBlankLines)
        {
            string strHtmlLineContent = "";
            int nRet = 0;

            int nIndex = nPage * option.LinesPerPage + nLine;

            if (nIndex >= items.Count)
                goto END1;

            ListViewItem item = items[nIndex];

            if (this.MarcFilter != null)
            {
                string strError = "";
                string strMARC = "";
                string strOutMarcSyntax = "";

                // TODO: 有错误要明显报出来，否则容易在打印出来后才发现，就晚了

                // 获得MARC格式书目记录
                string strBiblioRecPath = ListViewUtil.GetItemText(item, MERGED_COLUMN_BIBLIORECPATH);

                nRet = GetMarc(strBiblioRecPath,
                    out strMARC,
                    out strOutMarcSyntax,
                    out strError);
                if (nRet == -1)
                {
                    strHtmlLineContent = strError;
                    goto END1;
                }

                this.ColumnTable.Clear();   // 清除上一记录处理时残余的内容

                // 触发filter中的Record相关动作
                nRet = this.MarcFilter.DoRecord(
                    null,
                    strMARC,
                    strOutMarcSyntax,
                    nIndex,
                    out strError);
                if (nRet == -1)
                {
                    strHtmlLineContent = strError;
                    goto END1;
                }
            }


            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                /*
                int nIndex = nPage * option.LinesPerPage + nLine;

                if (nIndex >= items.Count)
                    break;

                ListViewItem item = items[nIndex];
                 * */

                string strContent = GetMergedColumnContent(item,
                    column.Name);

                if (strContent == "!!!#")
                    strContent = ((nPage * option.LinesPerPage) + nLine + 1).ToString();

                /*
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
                 * */

                // 截断字符串
                if (column.MaxChars != -1)
                {
                    if (strContent.Length > column.MaxChars)
                    {
                        strContent = strContent.Substring(0, column.MaxChars);
                        strContent += "...";
                    }
                }

                string strClass = GetClass(column.Name);

                if (this.checkBox_print_accepted.Checked == false)
                {
                    // 不输出 已到套数 列
                    if (strClass == "acceptCopy")
                        continue;
                }

                if (doc != null)
                {
                    int nLineIndex = (nPage * option.LinesPerPage) + nLine;
                    ExcelUtil.UpdateValue(
                        doc.workbookpart,
                        doc.worksheetPart.Worksheet,
                        ExcelDocument.GetCellName(i, nLineIndex + nTopBlankLines),
                        strContent,
                        0,
                        StringUtil.IsNumber(strContent) ? false : true);
                }

                if (String.IsNullOrEmpty(strContent) == true)
                    strContent = "&nbsp;";
                else
                    strContent = HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>");

                strHtmlLineContent +=
                    "<td class='" + strClass + "'>" + strContent + "</td>";
            }

            END1:

            StreamUtil.WriteText(strFileName,
    "<tr class='content'>");

            /* 如果要打印空行
            if (string.IsNullOrEmpty(strLineContent) == true)
            {
                strLineContent += "<td class='blank' colspan='" + option.Columns.Count.ToString() + "'>&nbsp;</td>";
            }
             * */
            StreamUtil.WriteText(strFileName,
                strHtmlLineContent);

            StreamUtil.WriteText(strFileName,
                "</tr>");

            return 0;
        }

        // 获得栏目内容(合并后)
        string GetMergedColumnContent(ListViewItem item,
            string strColumnName)
        {
            // 去掉"-- ?????"部分
            /*
            string strText = strColumnName;
            int nRet = strText.IndexOf("--", 0);
            if (nRet != -1)
                strText = strText.Substring(0, nRet).Trim();
             * */

            string strName = "";
            string strParameters = "";
            ParseNameParam(StringUtil.GetLeft(strColumnName),
                out strName,
                out strParameters);

            // 2013/3/29
            // 要求ColumnTable值
            if (strName.Length > 0 && strName[0] == '@')
            {
                strName = strName.Substring(1);
                return (string)this.ColumnTable[strName];
            }

            try
            {
                // TODO: 需要修改
                // 要中英文都可以
                switch (strName)
                {
                    case "no":
                    case "序号":
                        return "!!!#";  // 特殊值，表示序号

                    case "seller":
                    case "书商":
                    case "渠道":
                        return item.SubItems[MERGED_COLUMN_SELLER].Text;

                    case "catalogNo":
                    case "书目号":
                        return item.SubItems[MERGED_COLUMN_CATALOGNO].Text;

                    case "errorInfo":
                    case "summary":
                    case "摘要":
                        return item.SubItems[MERGED_COLUMN_SUMMARY].Text;

                    case "isbnIssn":
                    case "ISBN/ISSN":
                        return item.SubItems[MERGED_COLUMN_ISBNISSN].Text;

                    case "mergeComment":
                    case "合并注释":
                        return item.SubItems[MERGED_COLUMN_MERGECOMMENT].Text;



                        // 没有recpath记录路径。因为recpath已经归入“合并注释”栏
                        // 没有state状态。因为state将被全部重设为“已订购”
                        // 没有source经费来源。因为已经归入“合并注释”栏
                        // 没有batchNo批次号，因为原始事项已经合并，多个原始事项不一定具有相同的批次号

                    case "range":
                    case "时间范围":
                        return item.SubItems[MERGED_COLUMN_RANGE].Text;

                    case "issueCount":
                    case "包含期数":
                        return item.SubItems[MERGED_COLUMN_ISSUECOUNT].Text;

                    case "series":
                    case "套数":
                        {
                            string strCopy = item.SubItems[MERGED_COLUMN_COPY].Text;
                            string strSubCopy = item.SubItems[MERGED_COLUMN_SUBCOPY].Text;
                            if (String.IsNullOrEmpty(strSubCopy) == true)
                                return strCopy;

                            return strCopy + "(每套含 " + strSubCopy + " 册)";
                        }

                    case "copy":
                    case "复本数":
                        return item.SubItems[MERGED_COLUMN_COPY].Text;

                    case "subcopy":
                    case "每套册数":
                        return item.SubItems[MERGED_COLUMN_SUBCOPY].Text;

                    case "price":
                    case "单价":
                        return item.SubItems[MERGED_COLUMN_PRICE].Text;

                    case "totalPrice":
                    case "总价格":
                        return item.SubItems[MERGED_COLUMN_TOTALPRICE].Text;

                    case "orderTime":
                    case "订购时间":
                        return item.SubItems[MERGED_COLUMN_ORDERTIME].Text;

                    case "orderID":
                    case "订单号":
                        return item.SubItems[MERGED_COLUMN_ORDERID].Text;

                    case "distribute":
                    case "馆藏分配":
                        return item.SubItems[MERGED_COLUMN_DISTRIBUTE].Text;

                    case "acceptCopy":
                    case "已到套数":
                    case "已到复本数":
                        return item.SubItems[MERGED_COLUMN_ACCEPTCOPY].Text;

                    case "acceptSubCopy":
                    case "已到每套册数":
                        return item.SubItems[MERGED_COLUMN_ACCEPTSUBCOPY].Text;

                    case "acceptPrice":
                    case "到书单价":
                        return item.SubItems[MERGED_COLUMN_ACCEPTPRICE].Text;

                    case "class":
                    case "类别":
                        return item.SubItems[MERGED_COLUMN_CLASS].Text;


                    case "comment":
                    case "注释":
                    case "附注":
                        return item.SubItems[MERGED_COLUMN_COMMENT].Text;

                    case "biblioRecpath":
                    case "种记录路径":
                        return item.SubItems[MERGED_COLUMN_BIBLIORECPATH].Text;

                    // 格式化以后的渠道地址
                    case "sellerAddress":
                    case "渠道地址":
                        return GetPrintableSellerAddress(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            strParameters);

                    case "sellerAddress:zipcode":
                    case "渠道地址:邮政编码":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "zipcode");

                    case "sellerAddress:address":
                    case "渠道地址:地址":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "address");

                    case "sellerAddress:department":
                    case "渠道地址:单位":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "department");

                    case "sellerAddress:name":
                    case "渠道地址:联系人":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "name");

                    case "sellerAddress:email":
                    case "渠道地址:Email地址":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "email");

                    case "sellerAddress:bank":
                    case "渠道地址:开户行":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "bank");

                    case "sellerAddress:accounts":
                    case "渠道地址:银行账号":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "accounts");

                    case "sellerAddress:payStyle":
                    case "渠道地址:汇款方式":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "payStyle");

                    case "sellerAddress:comment":
                    case "渠道地址:附注":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "comment");
                    default:
                        {
                            // 2013/3/29
                            if (this.ColumnTable.Contains(strName) == false)
                                return "未知栏目 '" + strName + "'";

                            return (string)this.ColumnTable[strName];
                        }
                }
            }

            catch
            {
                return null;    // 表示没有这个subitem下标
            }

        }

        /// <summary>
        /// 获得渠道地址 XML 片断中的内嵌元素值
        /// </summary>
        /// <param name="strXmlFragment">渠道地址 XML 片断</param>
        /// <param name="strSeller">渠道名</param>
        /// <param name="strElementName">内嵌元素名</param>
        /// <returns>内嵌元素的正文值</returns>
        public static string GetSellerAddressInnerValue(string strXmlFragment,
    string strSeller,
    string strElementName)
        {
            if (string.IsNullOrEmpty(strXmlFragment) == true)
                return "";

            XmlDocument dom1 = new XmlDocument();
            try
            {
                dom1.LoadXml("<root>" + strXmlFragment + "</root>");
            }
            catch (Exception ex)
            {
                return "渠道地址XML字符串 '" + strXmlFragment + "' 格式不正确: " + ex.Message;
            }


            return DomUtil.GetElementText(dom1.DocumentElement, strElementName);
        }

        /// <summary>
        /// 构造便于显示打印的渠道地址字符串
        /// </summary>
        /// <param name="strXmlFragment">渠道地址 XML 片断</param>
        /// <param name="strSeller">渠道名</param>
        /// <param name="strParameters">要筛选的元素名列表。逗号间隔的字符串</param>
        /// <returns>便于显示和打印的文本内容。有回车换行符号</returns>
        public static string GetPrintableSellerAddress(string strXmlFragment,
            string strSeller,
            string strParameters)
        {
            if (string.IsNullOrEmpty(strXmlFragment) == true)
                return "";

            XmlDocument dom1 = new XmlDocument();
            try
            {
                dom1.LoadXml("<root>" + strXmlFragment + "</root>");
            }
            catch (Exception ex)
            {
                return "渠道地址XML字符串 '" + strXmlFragment + "' 格式不正确: " + ex.Message;
            }

            string[] elements = new string[] {
            "zipcode", "邮政编码",
            "address", "地址",
            "department", "单位",
            "name", "联系人",
            "tel", "电话",
            "email", "Email地址",
            "bank", "开户行",
            "accounts", "银行账号",
            "payStyle", "汇款方式",
            "comment", "附注"};

            StringBuilder result = new StringBuilder(4096);

            List<string> selectors = StringUtil.FromListString(strParameters);

            for (int i = 0; i < elements.Length / 2; i++)
            {
                string strElementName = elements[i * 2];
                string strCaption = elements[i * 2 + 1];

                // 修正caption
                if (strSeller == "赠")
                {
                    if (strElementName == "name")
                        strCaption = "赠书人";
                }

                // 对元素名进行筛选
                if (string.IsNullOrEmpty(strParameters) == false)
                {
                    if (selectors.IndexOf(strElementName) == -1)
                        continue;
                }

                string text = DomUtil.GetElementText(dom1.DocumentElement, strElementName);
                if (string.IsNullOrEmpty(text) == false)
                {
                    if (result.Length > 0)
                        result.Append("\r\n");
                    result.Append(strCaption + ": " + text);
                }
            }

            return result.ToString();
        }

        int BuildMergedPageBottom(PrintOption option,
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
        "<div class='pagefooter'>" + HttpUtility.HtmlEncode(strPageFooterText) + "</div>");
            }


            StreamUtil.WriteText(strFileName, "</body></html>");

            return 0;
        }

        // 获得已经订购的总种数
        static int GetMergedBiblioCount(NamedListViewItems items)
        {
            List<string> paths = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strText = "";

                try
                {
                    strText = item.SubItems[MERGED_COLUMN_BIBLIORECPATH].Text;
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

        // 获得已经验收至少一册以上的总种数
        static int GetMergedAcceptBiblioCount(NamedListViewItems items)
        {
            List<string> paths = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                int nAcceptSeries = 0;

                try
                {
                    string strAcceptCopy = "";
                    strAcceptCopy = item.SubItems[MERGED_COLUMN_ACCEPTCOPY].Text;

                    // TODO: 注意检查是否有[]符号?
                    nAcceptSeries = Convert.ToInt32(strAcceptCopy);
                }
                catch
                {
                    continue;
                }

                if (nAcceptSeries == 0)
                    continue;


                string strText = "";

                try
                {
                    strText = item.SubItems[MERGED_COLUMN_BIBLIORECPATH].Text;
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


        // 获得已订购的复本总数
        static int GetMergedTotalCopies(NamedListViewItems items)
        {
            int total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                int nCopy = 0;

                try
                {
                    string strCopy = "";
                    strCopy = item.SubItems[MERGED_COLUMN_COPY].Text;

                    // TODO: 注意检查是否有[]符号?
                    nCopy = Convert.ToInt32(strCopy);
                }
                catch
                {
                    continue;
                }

                int nSubCopy = 1;
                string strSubCopy = "";
                strSubCopy = item.SubItems[MERGED_COLUMN_SUBCOPY].Text;

                if (String.IsNullOrEmpty(strSubCopy) == false)
                {
                    try
                    {
                        nSubCopy = Convert.ToInt32(strSubCopy);
                    }
                    catch
                    {
                    }
                }

                total += nCopy*nSubCopy;
            }

            return total;
        }

        // 获得已验收的册数总数
        static int GetMergedAcceptTotalCopies(NamedListViewItems items)
        {
            int total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                int nAcceptSeries = 0;

                try
                {
                    string strAcceptCopy = "";
                    strAcceptCopy = item.SubItems[MERGED_COLUMN_ACCEPTCOPY].Text;

                    // TODO: 注意检查是否有[]符号?
                    nAcceptSeries = Convert.ToInt32(strAcceptCopy);
                }
                catch
                {
                    continue;
                }

                int nSubCopy = 1;
                string strSubCopy = "";
                // strSubCopy = item.SubItems[MERGED_COLUMN_SUBCOPY].Text;
                strSubCopy = item.SubItems[MERGED_COLUMN_ACCEPTSUBCOPY].Text;

                if (String.IsNullOrEmpty(strSubCopy) == false)
                {
                    try
                    {
                        nSubCopy = Convert.ToInt32(strSubCopy);
                    }
                    catch
                    {
                    }
                }

                total += nAcceptSeries * nSubCopy;  // 不精确
            }

            return total;
        }

        // 获得已订购的总套数
        static int GetMergedTotalSeries(NamedListViewItems items)
        {
            int total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                int nCopy = 0;

                try
                {
                    string strCopy = "";
                    strCopy = item.SubItems[MERGED_COLUMN_COPY].Text;

                    // TODO: 注意检查是否有[]符号?
                    nCopy = Convert.ToInt32(strCopy);
                }
                catch
                {
                    continue;
                }

                total += nCopy;
            }

            return total;
        }

        // 获得已验收的总套数
        static int GetMergedAcceptTotalSeries(NamedListViewItems items)
        {
            int total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                int nCopy = 0;

                try
                {
                    string strAcceptCopy = "";
                    strAcceptCopy = item.SubItems[MERGED_COLUMN_ACCEPTCOPY].Text;

                    // TODO: 注意检查是否有[]符号?
                    nCopy = Convert.ToInt32(strAcceptCopy);
                }
                catch
                {
                    continue;
                }

                total += nCopy;
            }

            return total;
        }


        /*
        static double GetMergedTotalPrice(NamedListViewItems items)
        {
            double total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strPrice = "";

                try
                {
                    strPrice = item.SubItems[MERGED_COLUMN_TOTALPRICE].Text;
                }
                catch
                {
                    continue;
                }

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                // 提取出纯数字
                string strPurePrice = Global.GetPurePrice(strPrice);

                if (String.IsNullOrEmpty(strPurePrice) == true)
                    continue;

                total += Convert.ToDouble(strPurePrice);
            }

            return total;
        }
         * */

        // 获得已经订购的总价格
        static string GetMergedTotalPrice(NamedListViewItems items)
        {
            List<string> prices = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strPrice = "";

                try
                {
                    strPrice = item.SubItems[MERGED_COLUMN_TOTALPRICE].Text;
                }
                catch
                {
                    continue;
                }

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                prices.Add(strPrice);
            }

            List<string> results = null;
            string strError = "";
            // 汇总价格
            // 货币单位不同的，互相独立
            int nRet = PriceUtil.TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return strError;

            string strResult = "";
            for (int i = 0; i < results.Count; i++)
            {
                string strPrice = results[i];
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += "+";
                strResult += strPrice;
            }

            return strResult;
        }

        // 获得已经验收的总价格
        static string GetMergedAcceptTotalPrice(NamedListViewItems items)
        {
            string strError = "";
            int nRet = 0;

            List<string> prices = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                int nAcceptSeries = 0;

                try
                {
                    string strAcceptCopy = "";
                    strAcceptCopy = item.SubItems[MERGED_COLUMN_ACCEPTCOPY].Text;

                    // TODO: 注意检查是否有[]符号?
                    nAcceptSeries = Convert.ToInt32(strAcceptCopy);
                }
                catch
                {
                    continue;
                }

                if (nAcceptSeries == 0)
                    continue;

                string strPrice = "";

                try
                {
                    strPrice = item.SubItems[MERGED_COLUMN_ACCEPTPRICE].Text;
                }
                catch
                {
                    continue;
                }

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                {
                    string strTemp = "";
                    nRet = PriceUtil.MultiPrice(strPrice,
                        nAcceptSeries,
                        out strTemp,
                        out strError);
                    if (nRet == -1)
                        return strError;

                    prices.Add(strTemp);
                }
            }

            List<string> results = null;
            // 汇总价格
            // 货币单位不同的，互相独立
            nRet = PriceUtil.TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return strError;

                string strResult = "";
                for (int i = 0; i < results.Count; i++)
                {
                    string strPrice = results[i];
                    if (String.IsNullOrEmpty(strResult) == false)
                        strResult += "+";
                    strResult += strPrice;
                }

            return strResult;
        }


        private void button_load_loadFromBatchNo_Click(object sender, EventArgs e)
        {
            SearchByBatchnoForm dlg = new SearchByBatchnoForm();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.CfgSectionName = "PrintOrderForm_SearchByBatchnoForm";
            this.BatchNo = "";

            dlg.Text = "根据订购批次号检索出订购记录";
            dlg.DisplayLocationList = false;

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

            this.BatchNo = dlg.BatchNo;
            /*
            string strMatchLocation = dlg.ItemLocation;

            if (strMatchLocation == "<不指定>")
                strMatchLocation = null;    // null和""的区别很大
             * */

            string strError = "";
            int nRet = 0;

            if (System.Windows.Forms.Control.ModifierKeys == Keys.Control)
            {
            }
            else
            {
                if (this.Changed == true)
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
                        "当前窗口内有原始信息被修改后尚未保存。若此时为装载新内容而清除原有信息，则未保存信息将丢失。\r\n\r\n确实要装载新内容? ",
                        "PrintOrderForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                    {
                        return; // 放弃
                    }
                }

                this.listView_origin.Items.Clear();
                // 2008/11/22
                this.SortColumns_origin.Clear();
                SortColumns.ClearColumnSortDisplay(this.listView_origin.Columns);
            }

            EnableControls(false);
            // MainForm.ShowProgress(true);

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
                    lRet = Channel.SearchOrder(
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
                    lRet = Channel.SearchOrder(
                        stop,
                        this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
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
                    strError = "批次号 '" + dlg.BatchNo + "' 没有命中记录。";
                    goto ERROR1;
                }

                int nDupCount = 0;

                long lHitCount = lRet;

                stop.SetProgressRange(0, lHitCount);
                // stop.SetProgressValue(0);


                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

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
                        string strRecPath = searchresults[i].Path;
                        // 根据记录路径，装入订购记录
                        // return: 
                        //      -2  路径已经在list中存在了
                        //      -1  出错
                        //      1   成功
                        nRet = LoadOneItem(strRecPath,
                            this.listView_origin,
                            out strError);
                        if (nRet == -2)
                            nDupCount++;

                        stop.SetProgressValue(lStart + i + 1);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                /*
                if (this.listView_in.Items.Count == 0
                    && strMatchLocation != null)
                {
                    strError = "虽然批次号 '" + dlg.BatchNo + "' 命中了记录 " + lHitCount.ToString() + " 条, 但它们均未能匹配馆藏地点 '" + strMatchLocation + "' 。";
                    goto ERROR1;
                }*/


                // 填充合并后数据列表
                stop.SetMessage("正在合并数据...");
                nRet = FillMergedList(out strError);
                if (nRet == -1)
                    goto ERROR1;

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

        void dlg_GetBatchNoTable(object sender, GetKeyCountListEventArgs e)
        {
            Global.GetBatchNoTable(e,
                this,
                this.comboBox_load_type.Text,
                "order",
                this.stop,
                this.Channel);

#if NOOOOOOOOOOOOOOOOOOOOOOOOO
            string strError = "";

            if (e.KeyCounts == null)
                e.KeyCounts = new List<KeyCount>();

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在列出全部订购批次号 ...");
            stop.BeginLoop();

            try
            {
                MainForm.SetProgressRange(100);
                MainForm.SetProgressValue(0);

                long lRet = Channel.SearchOrder(
                    stop,
                    "<all>",
                    "", // strBatchNo
                    2000,   // -1,
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
                    strError = "没有找到任何订购批次号检索点";
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

        // 根据排序键值的变化分组设置颜色
        // 算法是将原本的背景颜色变深或者浅一点
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
                    item.BackColor = Global.Dark(GetItemBackColor(item.ImageIndex), 0.05F);

                    /*
                    item.BackColor = Color.LightGray;

                    if (item.ForeColor == item.BackColor)
                        item.ForeColor = Color.Black;
                     * */
                }
                else
                {
                    item.BackColor = Global.Light(GetItemBackColor(item.ImageIndex), 0.05F);

                    /*
                    item.BackColor = Color.White;

                    if (item.ForeColor == item.BackColor)
                        item.ForeColor = Color.Black;
                     * */
                }
            }
        }


        private void listView_origin_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns_origin.SetFirstColumn(nClickColumn,
                this.listView_origin.Columns);

            // 排序
            this.listView_origin.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_origin);

            this.listView_origin.ListViewItemSorter = null;

            SetGroupBackcolor(
                this.listView_origin,
                nClickColumn);
        }

        private void listView_origin_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            string strOrderRecPath = "";
            if (this.listView_origin.SelectedItems.Count > 0)
            {
                strOrderRecPath = ListViewUtil.GetItemText(this.listView_origin.SelectedItems[0], ORIGIN_COLUMN_RECPATH);
            }
            menuItem = new MenuItem("打开种册窗，观察订购记录 '" + strOrderRecPath + "' (&O)");
            menuItem.Tag = this.listView_origin;
            menuItem.Click += new System.EventHandler(this.menu_loadOrderRecord_Click);
            if (this.listView_origin.SelectedItems.Count == 0
                || String.IsNullOrEmpty(strOrderRecPath) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("刷新选定的行(&S)");
            menuItem.Tag = this.listView_origin;
            menuItem.Click += new System.EventHandler(this.menu_refreshSelected_Click);
            if (this.listView_origin.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新全部行(&R)");
            menuItem.Tag = this.listView_origin;
            menuItem.Click += new System.EventHandler(this.menu_refreshAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("重新进行合并(&M)");
            menuItem.Tag = this.listView_origin;
            menuItem.Click += new System.EventHandler(this.menu_merge_Click);
            if (this.listView_origin.Items.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("按合并风格排序(&S)");
            menuItem.Tag = this.listView_origin;
            menuItem.Click += new System.EventHandler(this.menu_sort_for_merge_Click);
            if (this.listView_origin.Items.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("移除(&D)");
            menuItem.Tag = this.listView_origin;
            menuItem.Click += new System.EventHandler(this.menu_deleteSelected_Click);
            if (this.listView_origin.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_origin, new Point(e.X, e.Y));		
        }

        // 打开种册窗，观察订购记录
        void menu_loadOrderRecord_Click(object sender, EventArgs e)
        {
            LoadOrderToEntityForm(this.listView_origin);
        }

        void LoadOrderToEntityForm(ListView list)
        {
            if (list.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装载到种册窗的事项");
                return;
            }

            string strRecPath = ListViewUtil.GetItemText(list.SelectedItems[0], ORIGIN_COLUMN_RECPATH);

            EntityForm form = new EntityForm();

            form.MainForm = this.MainForm;
            form.MdiParent = this.MainForm;
            form.Show();

            if (this.comboBox_load_type.Text == "图书")
                form.LoadOrderByRecPath(strRecPath, false);
            else
                form.LoadIssueByRecPath(strRecPath, false);
        }

        // 重新进行合并
        void menu_merge_Click(object sender, EventArgs e)
        {
            string strError = "";
            stop.SetMessage("正在合并数据...");
            int nRet = FillMergedList(out strError);
            stop.SetMessage("");
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
            else
            {
                // 切换到 已合并 列表
                this.tabControl_items.SelectedTab = this.tabPage_mergedItems;
            }
        }

        // 按照合并的风格(要求)对原始数据事项排序
        void menu_sort_for_merge_Click(object sender, EventArgs e)
        {
            this.SortOriginListForMerge();
        }

        void menu_refreshSelected_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in list.SelectedItems)
            {
                items.Add(item);

            }
            RefreshLines(items);

            // 填充合并后数据列表
            string strError = "";
            stop.SetMessage("正在合并数据...");
            int nRet = FillMergedList(out strError);
            stop.SetMessage("");
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            /*
            if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {
                SetVerifyPageNextButtonEnabled();
            }
             * */
        }

        void menu_refreshAll_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;

            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < list.Items.Count; i++)
            {
                items.Add(list.Items[i]);
            }
            RefreshLines(items);

            // 填充合并后数据列表
            string strError = "";
            stop.SetMessage("正在合并数据...");
            int nRet = FillMergedList(out strError);
            stop.SetMessage("");
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            /*
            if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {
                SetVerifyPageNextButtonEnabled();
            }*/
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
"确实要在原始数据列表窗口内移除选定的 " + items.Count.ToString() + " 个事项?",
"dp2Circulation",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            DeleteLines(items);

            // 填充合并后数据列表
            string strError = "";
            stop.SetMessage("正在合并数据...");
            int nRet = FillMergedList(out strError);
            stop.SetMessage("");
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            SetNextButtonEnable();  // 2008/12/22

            /*
            if (this.tabControl_main.SelectedTab == this.tabPage_verify)
            {
                SetVerifyPageNextButtonEnabled();
            }*/
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

        /*public*/ int RefreshOneItem(ListViewItem item,
            out string strError)
        {
            strError = "";

            string strItemText = "";
            string strBiblioText = "";

            string strItemRecPath = "";
            string strBiblioRecPath = "";

            byte[] item_timestamp = null;

            string strIndex = "@path:" + item.SubItems[ORIGIN_COLUMN_RECPATH].Text;

            long lRet = Channel.GetOrderInfo(
                stop,
                strIndex,
                // "",
                "xml",
                out strItemText,
                out strItemRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strBiblioRecPath,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                ListViewUtil.ChangeItemText(item, 1, strError);

                SetItemColor(item, TYPE_ERROR);
                goto ERROR1;
            }

            string strBiblioSummary = "";
            string strISBnISSN = "";

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
                    Debug.Assert(results != null && results.Length == 2, "results必须包含2个元素");
                    strBiblioSummary = results[0];
                    strISBnISSN = results[1];
                }

            }

            // 剖析一个册的xml记录，取出有关信息放入listview中

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemText);
            }
            catch (Exception ex)
            {
                strError = "PrintOrderForm dom.LoadXml() {24820B1C-0D1C-45CA-AB9C-916FF3A14078} exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }


            {
                SetListViewItemText(
                    this.comboBox_load_type.Text,
                    this.checkBox_print_accepted.Checked,
                    dom,
                    true,
                    strItemRecPath,
                    strBiblioSummary,
                    strISBnISSN,
                    strBiblioRecPath,
                    item);
            }

            // 图标
            // item.ImageIndex = TYPE_NORMAL;
            SetItemColor(item, TYPE_NORMAL);
            return 1;
        ERROR1:
            return -1;
        }

        // 
        /// <summary>
        /// 比较两个渠道地址是否完全一致
        /// </summary>
        /// <param name="strXml1">渠道地址 XML 片断1</param>
        /// <param name="strXml2">渠道地址 XML 片断2</param>
        /// <returns>0: 完全一致; 1: 不完全一致</returns>
        public static int CompareAddress(string strXml1, string strXml2)
        {
            if (string.IsNullOrEmpty(strXml1) == true && string.IsNullOrEmpty(strXml2) == true)
                return 0;
            if (string.IsNullOrEmpty(strXml1) == true && string.IsNullOrEmpty(strXml2) == false)
                return 1;
            if (string.IsNullOrEmpty(strXml1) == false && string.IsNullOrEmpty(strXml2) == true)
                return 1;
            XmlDocument dom1 = new XmlDocument();
            XmlDocument dom2 = new XmlDocument();

            try
            {
                dom1.LoadXml("<root>" + strXml1 + "</root>");
            }
            catch (Exception ex)
            {
                throw new Exception("渠道地址XML字符串 '"+strXml1+"' 格式不正确: " + ex.Message);
            }

            try
            {
                dom2.LoadXml("<root>" + strXml2 + "</root>");
            }
            catch (Exception ex)
            {
                throw new Exception("渠道地址XML字符串 '" + strXml2 + "' 格式不正确: " + ex.Message);
            }

            string[] elements = new string[] {
            "zipcode",
            "address",
            "department",
            "name",
            "tel",
            "email",
            "bank",
            "accounts",
            "payStyle",
            "comment"};

            foreach (string element in elements)
            {
                string v1 = DomUtil.GetElementText(dom1.DocumentElement, element);
                string v2 = DomUtil.GetElementText(dom2.DocumentElement, element);
                if (string.IsNullOrEmpty(v1) == true && string.IsNullOrEmpty(v2) == true)
                    continue;
                if (v1 != v2)
                    return 1;

            }

            return 0;
        }

        // 填充合并后数据列表
        int FillMergedList(out string strError)
        {
            strError = "";
            int nRet = 0;

            DateTime now = DateTime.Now;
            int nOrderIdSeed = 1;

            this.listView_merged.Items.Clear();
            // 2008/11/22
            this.SortColumns_merged.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_merged.Columns);


            // 先将原始数据列表按照 seller/price 列排序
            SortOriginListForMerge();


            // 循环
            for (int i = 0; i < this.listView_origin.Items.Count; i++)
            {
                int nCopy = 0;

                ListViewItem source = this.listView_origin.Items[i];

                if (source.ImageIndex == TYPE_ERROR)
                {
                    strError = "事项 " + (i + 1).ToString() + " 的状态为错误，请先排除问题...";
                    return -1;
                }

                // 渠道
                string strSeller = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_SELLER);

                // 渠道地址
                string strSellerAddress = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_SELLERADDRESS);


                // 单价
                string strPrice = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_PRICE);

                string strAcceptPrice = "";

                // price取其中的订购价部分
                {
                    string strOldPrice = "";
                    string strNewPrice = "";

                    // 分离 "old[new]" 内的两个值
                    OrderDesignControl.ParseOldNewValue(strPrice,
                        out strOldPrice,
                        out strNewPrice);

                    strPrice = strOldPrice;
                    strAcceptPrice = strNewPrice;
                }

                string strIssueCount = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_ISSUECOUNT);
                string strRange = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_RANGE);

                // 书目记录路径
                string strBiblioRecPath = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_BIBLIORECPATH);

                // 书目号
                string strCatalogNo = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_CATALOGNO);

                // 2012/8/30
                string strOrderTime = ListViewUtil.GetItemText(source,
    ORIGIN_COLUMN_ORDERTIME);   // 已经是本地时间格式


                string strTempCopy = ListViewUtil.GetItemText(source,
ORIGIN_COLUMN_COPY);
                string strTempAcceptCopy = "";
                {
                    string strOldCopy = "";
                    string strNewCopy = "";
                    // 分离 "old[new]" 内的两个值
                    OrderDesignControl.ParseOldNewValue(strTempCopy,
                        out strOldCopy,
                        out strNewCopy);
                    strTempCopy = strOldCopy;
                    strTempAcceptCopy = strNewCopy;
                }

                int nSubCopy = 1;
                {
                    string strRightCopy = OrderDesignControl.GetRightFromCopyString(strTempCopy);
                    if (String.IsNullOrEmpty(strRightCopy) == false)
                    {
                        try
                        {
                            nSubCopy = Convert.ToInt32(strRightCopy);
                        }
                        catch (Exception ex)
                        {
                            strError = "原始数据事项 " + (i + 1).ToString() + " 内每套册数 '" + strRightCopy + "' 格式不正确: " + ex.Message;
                            return -1;
                        }
                    }
                }

                // 2014/2/19
                int nAcceptSubCopy = 1;
                {
                    string strRightCopy = OrderDesignControl.GetRightFromCopyString(strTempAcceptCopy);
                    if (String.IsNullOrEmpty(strRightCopy) == false)
                    {
                        try
                        {
                            nAcceptSubCopy = Convert.ToInt32(strRightCopy);
                        }
                        catch (Exception ex)
                        {
                            strError = "原始数据事项 " + (i + 1).ToString() + " 内已到每套册数 '" + strRightCopy + "' 格式不正确: " + ex.Message;
                            return -1;
                        }
                    }
                }

                string strMergeComment = "";    // 合并注释
                List<string> totalprices = new List<string>();  // 累积的价格字符串
                List<ListViewItem> origin_items = new List<ListViewItem>();

                string strComments = "";    // 原始注释(积累)
                string strDistributes = ""; // 合并的馆藏分配字符串

                // 发现biblioRecPath、price、seller、catalogno均相同的区段
                // 如果是连续出版物，还要issuecount和range相同
                int nStart = i; // 区段开始位置
                int nLength = 0;    // 区段内事项个数

                for (int j = i; j < this.listView_origin.Items.Count; j++)
                {
                    ListViewItem current_source = this.listView_origin.Items[j];

                    if (current_source.ImageIndex == TYPE_ERROR)
                    {
                        strError = "事项 " + (i + 1).ToString() + " 的状态为错误，请先排除问题...";
                        return -1;
                    }


                    // 渠道
                    string strCurrentSeller = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_SELLER);

                    // 渠道地址
                    string strCurrentSellerAddress = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_SELLERADDRESS);

                    // 单价
                    string strCurrentPrice = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_PRICE);

                    string strCurrentAcceptPrice = "";
                    // price取其中的订购价部分
                    {
                        string strCurrentOldPrice = "";
                        string strCurrentNewPrice = "";

                        // 分离 "old[new]" 内的两个值
                        OrderDesignControl.ParseOldNewValue(strCurrentPrice,
                            out strCurrentOldPrice,
                            out strCurrentNewPrice);

                        strCurrentPrice = strCurrentOldPrice;
                        strCurrentAcceptPrice = strCurrentNewPrice;
                    }

                    string strCurrentIssueCount = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_ISSUECOUNT);
                    string strCurrentRange = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_RANGE);

                    // 书目记录路径
                    string strCurrentBiblioRecPath = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_BIBLIORECPATH);

                    // 书目号
                    string strCurrentCatalogNo = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_CATALOGNO);

                    string strTempCurCopy = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_COPY);

                    {
                        string strOldCopy = "";
                        string strNewCopy = "";
                        // 分离 "old[new]" 内的两个值
                        OrderDesignControl.ParseOldNewValue(strTempCurCopy,
                            out strOldCopy,
                            out strNewCopy);
                        strTempCurCopy = strOldCopy;
                    }

                    int nCurCopy = 0;
                    string strLeftCopy = OrderDesignControl.GetCopyFromCopyString(strTempCurCopy);
                    try
                    {
                        nCurCopy = Convert.ToInt32(strLeftCopy);
                    }
                    catch (Exception ex)
                    {
                        strError = "原始数据事项 " + (i + 1).ToString() + " 内复本数字 '" + strLeftCopy + "' 格式不正确: " + ex.Message;
                        return -1;
                    }

                    int nCurSubCopy = 1;
                    string strRightCopy = OrderDesignControl.GetRightFromCopyString(strTempCurCopy);
                    if (String.IsNullOrEmpty(strRightCopy) == false)
                    {
                        try
                        {
                            nCurSubCopy = Convert.ToInt32(strRightCopy);
                        }
                        catch (Exception ex)
                        {
                            strError = "原始数据事项 " + (i + 1).ToString() + " 内每套册数 '" + strRightCopy + "' 格式不正确: " + ex.Message;
                            return -1;
                        }
                    }

                    if (this.comboBox_load_type.Text == "图书")
                    {
                        // 七元组判断 // 五元组判断 // 四元组判断
                        if (strBiblioRecPath != strCurrentBiblioRecPath
                            || strSeller != strCurrentSeller
                            || strPrice != strCurrentPrice
                            || strAcceptPrice != strCurrentAcceptPrice
                            || strCatalogNo != strCurrentCatalogNo
                            || nSubCopy != nCurSubCopy
                            || CompareAddress(strSellerAddress, strCurrentSellerAddress) != 0)
                            break;

                    }
                    else
                    {
                        // 九元组判断 // 七元组判断 // 六元组判断
                        if (strBiblioRecPath != strCurrentBiblioRecPath
                            || strSeller != strCurrentSeller
                            || strPrice != strCurrentPrice
                            || strAcceptPrice != strCurrentAcceptPrice
                            || strCatalogNo != strCurrentCatalogNo
                            || strIssueCount != strCurrentIssueCount
                            || strRange != strCurrentRange
                            || nSubCopy != nCurSubCopy
                            || CompareAddress(strSellerAddress, strCurrentSellerAddress) != 0)
                            break;
                    }



                    int nIssueCount = 1;
                    if (this.comboBox_load_type.Text != "图书")
                    {
                        try
                        {
                            nIssueCount = Convert.ToInt32(strIssueCount);
                        }
                        catch (Exception ex)
                        {
                            strError = "原始数据事项 " + (i + 1).ToString() + " 内期数 '" + strIssueCount + "' 格式不正确: " + ex.Message;
                            return -1;
                        }
                    }

                    // 汇总复本数
                    nCopy += nCurCopy;

                    // 汇总合并注释
                    string strSource = ListViewUtil.GetItemText(current_source, ORIGIN_COLUMN_SOURCE);
                    string strRecPath = ListViewUtil.GetItemText(current_source, ORIGIN_COLUMN_RECPATH);
                    if (String.IsNullOrEmpty(strMergeComment) == false)
                        strMergeComment += "; ";
                    strMergeComment += strSource + ", " + nCurCopy.ToString() + "册 (" + strRecPath + ")";

                    // 汇总价格
                    string strTotalPrice = "";
                    // 2009/11/9 changed
                    if (String.IsNullOrEmpty(strCurrentPrice) == false)
                    {
                        nRet = PriceUtil.MultiPrice(strCurrentPrice,
                            nCurCopy * nIssueCount,
                            out strTotalPrice,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "原始数据事项 " + (i + 1).ToString() + " 内价格字符串 '" + strCurrentPrice + "' 格式不正确: " + strError;
                            return -1;
                        }
                    }
                    else
                    {
                        // 2009/11/9
                        // 原始数据中的总价
                        strTotalPrice = ListViewUtil.GetItemText(current_source,
                            ORIGIN_COLUMN_TOTALPRICE);
                        if (String.IsNullOrEmpty(strTotalPrice) == true)
                        {
                            strError = "原始数据事项 " + (i + 1).ToString() + " 内，当价格字符串为空时，总价格字符串不应为空";
                            return -1;
                        }
                    }

                    totalprices.Add(strTotalPrice);

                    // 汇总注释
                    string strComment = ListViewUtil.GetItemText(current_source, ORIGIN_COLUMN_COMMENT);
                    if (String.IsNullOrEmpty(strComment) == false)
                    {
                        if (String.IsNullOrEmpty(strComments) == false)
                            strComments += "; ";
                        strComments += strComment + " @" + strRecPath;
                    }

                    // 汇总馆藏分配字符串
                    string strCurDistribute = ListViewUtil.GetItemText(current_source, ORIGIN_COLUMN_DISTRIBUTE);
                    if (String.IsNullOrEmpty(strCurDistribute) == false)
                    {
                        if (String.IsNullOrEmpty(strDistributes) == true)
                            strDistributes = strCurDistribute;
                        else
                        {
                            string strLocationString = "";
                            nRet = LocationCollection.MergeTwoLocationString(strDistributes,
                                strCurDistribute,
                                false,
                                out strLocationString,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            strDistributes = strLocationString;
                        }
                    }

                    // 汇总原始事项
                    origin_items.Add(current_source);

                    nLength++;
                }

                ListViewItem target = new ListViewItem();

                if (source.ImageIndex == TYPE_ERROR)
                    target.ImageIndex = TYPE_ERROR;
                else
                    target.ImageIndex = TYPE_NORMAL;  // 

                // seller
                target.Text = strSeller;

                // catalog no 
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_CATALOGNO,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_CATALOGNO));

                // summary
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_SUMMARY,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_SUMMARY));

                // isbn issn
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ISBNISSN,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_ISBNISSN));


                // merge comment
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_MERGECOMMENT,
                    strMergeComment);

                // range
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_RANGE,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_RANGE));

                // issue count
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ISSUECOUNT,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_ISSUECOUNT));

                // copy
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_COPY,
                    nCopy.ToString());

                // subcopy
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_SUBCOPY,
                    nSubCopy.ToString());

                // price
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_PRICE,
                    strPrice);

                List<string> sum_prices = null;
                nRet = PriceUtil.TotalPrice(totalprices,
                    out sum_prices,
                    out strError);
                if (nRet == -1)
                {
                    return -1;
                }

                Debug.Assert(sum_prices.Count == 1, "");
                // total price
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_TOTALPRICE,
                    sum_prices[0]);

                // order time
                if (this.checkBox_print_accepted.Checked == false)
                {
                    ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ORDERTIME,
                        now.ToShortDateString());   // TODO: 注意这个时间要返回到原始数据中
                }
                else
                {
                    ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ORDERTIME,
                        strOrderTime);
                }

                // order id
                string strOrderID = nOrderIdSeed.ToString();
                nOrderIdSeed++;
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ORDERID,
                    strOrderID);    // TODO: 注意这个编号要返回到原始数据中

                // distribute
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_DISTRIBUTE,
                    strDistributes);

                string strAcceptSeries = "";
                
                if (string.IsNullOrEmpty(strDistributes) == false)
                {
                    LocationCollection locations = new LocationCollection();
                    nRet = locations.Build(strDistributes,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "馆藏分配字符串 '"+strDistributes+"' 格式错误: " + strError;
                        return -1;
                    }

                    // 对于套内多册的验收情况，refid用竖线隔开，形成一组。本函数返回的应该理解为套数，不是册数。但套内可能验收不足
                    strAcceptSeries = locations.GetArrivedCopy().ToString();
                }

                // acceptcopy
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ACCEPTCOPY,
                    strAcceptSeries);

                // acceptsubcopy
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ACCEPTSUBCOPY,
                    nAcceptSubCopy.ToString());

                // acceptprice
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ACCEPTPRICE,
                    strAcceptPrice);

                // class
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_CLASS,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_CLASS));

                // comment
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_COMMENT,
                    strComments);

                // sellerAddress
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_SELLERADDRESS,
                    strSellerAddress);


                // biblio record path
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_BIBLIORECPATH,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_BIBLIORECPATH));

                // 每个合并后事项的Tag都保留了其来源ListViewItem的列表
                target.Tag = origin_items;

                // 修改原始事项的orderTime orderID
                if (this.checkBox_print_accepted.Checked == false)
                {
                    for (int k = 0; k < origin_items.Count; k++)
                    {
                        ListViewItem origin_item = origin_items[k];

                        bool bChanged = false;
                        string strOldOrderTime = ListViewUtil.GetItemText(origin_item, ORIGIN_COLUMN_ORDERTIME);
                        if (strOldOrderTime != now.ToShortDateString())
                        {
                            ListViewUtil.ChangeItemText(origin_item, ORIGIN_COLUMN_ORDERTIME,
                                now.ToShortDateString());
                            bChanged = true;

                            origin_item.SubItems[ORIGIN_COLUMN_ORDERTIME].BackColor = System.Drawing.Color.Red;

                            // 加粗字体
                            origin_item.SubItems[ORIGIN_COLUMN_ORDERTIME].Font =
                                new System.Drawing.Font(origin_item.SubItems[ORIGIN_COLUMN_ORDERTIME].Font, FontStyle.Bold);
                        }

                        string strOldOrderID = ListViewUtil.GetItemText(origin_item, ORIGIN_COLUMN_ORDERID);
                        if (strOrderID != strOldOrderID)
                        {
                            ListViewUtil.ChangeItemText(origin_item, ORIGIN_COLUMN_ORDERID,
                                strOrderID);
                            bChanged = true;

                            // 加粗字体
                            origin_item.SubItems[ORIGIN_COLUMN_ORDERID].Font =
                                new System.Drawing.Font(origin_item.SubItems[ORIGIN_COLUMN_ORDERID].Font, FontStyle.Bold);
                        }

                        if (bChanged == true)
                            SetItemChanged(origin_item, true);
                    }
                }

                this.listView_merged.Items.Add(target);

                i = nStart + nLength - 1;
            }

            // 刷新Origin的深浅间隔色
            if (this.SortColumns_origin.Count > 0)
            {
                SetGroupBackcolor(
                    this.listView_origin,
                    this.SortColumns_origin[0].No);
            }

            return 0;
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
        }

        void SortOriginListForMerge()
        {
            SortColumns sort_columns = new SortColumns();

            DigitalPlatform.Column column = new DigitalPlatform.Column();
            column.Asc = true;
            column.No = ORIGIN_COLUMN_BIBLIORECPATH;
            column.SortStyle = ColumnSortStyle.RecPath;
            sort_columns.Add(column);

            column = new DigitalPlatform.Column();
            column.Asc = true;
            column.No = ORIGIN_COLUMN_SELLER;
            column.SortStyle = ColumnSortStyle.LeftAlign;
            sort_columns.Add(column);

            column = new DigitalPlatform.Column();
            column.Asc = true;
            column.No = ORIGIN_COLUMN_PRICE;
            column.SortStyle = ColumnSortStyle.LeftAlign;
            sort_columns.Add(column);

            // 排序
            this.listView_origin.ListViewItemSorter = new SortColumnsComparer(sort_columns);

            this.listView_origin.ListViewItemSorter = null;

            SetGroupBackcolor(
                this.listView_origin,
                ORIGIN_COLUMN_BIBLIORECPATH);

            this.SortColumns_origin = sort_columns;
        }

        // 排序 合并后列表
        private void listView_merged_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns_merged.SetFirstColumn(nClickColumn,
                this.listView_merged.Columns);

            // 排序
            this.listView_merged.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_merged);

            this.listView_merged.ListViewItemSorter = null;

            SetGroupBackcolor(
                this.listView_merged,
                nClickColumn);
        }

        // 合并后列表上的popupmemu
        private void listView_merged_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("按打印风格排序(&S)");
            menuItem.Tag = this.listView_merged;
            menuItem.Click += new System.EventHandler(this.menu_sort_merged_for_print_Click);
            if (this.listView_merged.Items.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("移除(&D)");
            menuItem.Tag = this.listView_merged;
            menuItem.Click += new System.EventHandler(this.menu_deleteSelected_merged_Click);
            if (this.listView_merged.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_merged, new Point(e.X, e.Y));		
        }

        // 移除 合并后列表中选定的事项
        void menu_deleteSelected_merged_Click(object sender, EventArgs e)
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
"确实要在合并后列表窗口内移除选定的 " + items.Count.ToString() + " 个事项?",
"dp2Circulation",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            DeleteLines(items);
        }

        // 按照打印输出的风格(要求)对合并后数据事项排序
        void menu_sort_merged_for_print_Click(object sender, EventArgs e)
        {
            this.SortMergedListForOutput();
        }

        // 按照打印输出的风格(要求)对合并后数据事项排序
        void SortMergedListForOutput()
        {
            SortColumns sort_columns = new SortColumns();

            DigitalPlatform.Column column = new DigitalPlatform.Column();
            column.Asc = true;
            column.No = MERGED_COLUMN_SELLER;
            column.SortStyle = ColumnSortStyle.LeftAlign;
            sort_columns.Add(column);

            column = new DigitalPlatform.Column();
            column.Asc = true;
            column.No = MERGED_COLUMN_BIBLIORECPATH;
            column.SortStyle = ColumnSortStyle.RecPath;
            sort_columns.Add(column);


            column = new DigitalPlatform.Column();
            column.Asc = true;
            column.No = MERGED_COLUMN_PRICE;
            column.SortStyle = ColumnSortStyle.LeftAlign;
            sort_columns.Add(column);

            // 排序
            this.listView_merged.ListViewItemSorter = new SortColumnsComparer(sort_columns);

            this.listView_merged.ListViewItemSorter = null;

            SetGroupBackcolor(
                this.listView_merged,
                MERGED_COLUMN_SELLER);
        }

        #region 原始数据

        // 打印原始数据
        private void button_print_printOriginList_Click(object sender, EventArgs e)
        {
            int nErrorCount = 0;

            this.tabControl_items.SelectedTab = this.tabPage_originItems;

            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < this.listView_origin.Items.Count; i++)
            {
                ListViewItem item = this.listView_origin.Items[i];

                items.Add(item);

                if (item.ImageIndex == TYPE_ERROR)
                    nErrorCount++;
            }

            if (nErrorCount != 0)
            {
                MessageBox.Show(this, "警告：这里打印出的清单，有包含错误信息的事项。");
            }

            PrintOriginList(items);
            return;
        }

        // 原始数据选项
        private void button_print_originOption_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            string strNamePath = "orderorigin_printoption";

            OrderOriginPrintOption option = new OrderOriginPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);

            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.MainForm = this.MainForm;
            dlg.Text = this.comboBox_load_type.Text + " 原始数据 打印参数";
            dlg.PrintOption = option;
            dlg.DataDir = this.MainForm.DataDir;
            dlg.ColumnItems = new string[] {
                "no -- 序号",
                "recpath -- 记录路径",
                "summary -- 摘要",
                "isbnIssn -- ISBN/ISSN",
                "state -- 状态",
                "catalogNo -- 书目号",
                "seller -- 渠道",
                "source -- 经费来源",
                "range -- 时间范围",
                "issueCount -- 包含期数",
                "copy -- 复本数",

                "price -- 单价",
                "totalPrice -- 总价格",
                "orderTime -- 订购时间",
                "orderID -- 订单号",
                "distribute -- 馆藏分配",
                "class -- 类别",

                "comment -- 注释",
                "batchNo -- 批次号",

                "sellerAddress -- 渠道地址",
                "sellerAddress:zipcode -- 渠道地址:邮政编码",
                "sellerAddress:address -- 渠道地址:地址",
                "sellerAddress:department -- 渠道地址:单位",
                "sellerAddress:name -- 渠道地址:联系人",
                "sellerAddress:tel -- 渠道地址:电话",
                "sellerAddress:email -- 渠道地址:Email地址",
                "sellerAddress:bank -- 渠道地址:开户行",
                "sellerAddress:accounts -- 渠道地址:银行账号",
                "sellerAddress:payStyle -- 渠道地址:汇款方式",
                "sellerAddress:comment -- 渠道地址:附注",

                "biblioRecpath -- 种记录路径"
            };


            this.MainForm.AppInfo.LinkFormState(dlg, "orderorigin_printoption_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(this.MainForm.AppInfo,
                strNamePath);
        }

        void PrintOriginList(List<ListViewItem> items)
        {
            string strError = "";

            // 创建一个html文件，并显示在HtmlPrintForm中。
            List<string> filenames = null;
            try
            {
                // Debug.Assert(false, "");

                // 构造html页面
                int nRet = BuildOriginHtml(
                    items,
                    out filenames,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                HtmlPrintForm printform = new HtmlPrintForm();

                printform.Text = "打印原始订购数据";
                printform.MainForm = this.MainForm;
                printform.Filenames = filenames;
                this.MainForm.AppInfo.LinkFormState(printform, "printorder_htmlprint_formstate");
                printform.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(printform);
            }

            finally
            {
                if (filenames != null)
                    Global.DeleteFiles(filenames);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 构造html页面
        int BuildOriginHtml(
            List<ListViewItem> items,
            out List<string> filenames,
            out string strError)
        {
            strError = "";
            filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名
            int nRet = 0;

            Hashtable macro_table = new Hashtable();

            // 获得打印参数
            OrderOriginPrintOption option = new OrderOriginPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                "orderorigin_printoption");

            // 准备一般的 MARC 过滤器
            {
                string strMarcFilterFilePath = option.GetTemplatePageFilePath("MARC过滤器");
                if (String.IsNullOrEmpty(strMarcFilterFilePath) == false)
                {
                    Debug.Assert(String.IsNullOrEmpty(strMarcFilterFilePath) == false, "");
                    nRet = PrepareMarcFilter(strMarcFilterFilePath, out strError);
                    if (nRet == -1)
                        return -1;
                }
            }

            /*
            // 检查当前排序状态和包含种价格列之间是否存在矛盾
            if (bHasBiblioPriceColumn(option) == true)
            {

                if (this.SortColumns_in.Count != 0
                    && this.SortColumns_in[0].No == COLUMN_BIBLIORECPATH)
                {
                }
                else
                {
                    MessageBox.Show(this, "警告：打印列中要求了‘种价格’列，但打印前集合内事项并未按‘种记录路径’排序，这样打印出的‘种价格’栏内容将会不准确。\r\n\r\n要避免这种情况，可在打印前用鼠标左键点‘种记录路径’栏标题，确保按其排序。");
                }
            }*/


            // 计算出页总数
            int nTablePageCount = items.Count / option.LinesPerPage;
            if ((items.Count % option.LinesPerPage) != 0)
                nTablePageCount++;

            int nPageCount = nTablePageCount + 1;

            macro_table["%batchno%"] = this.BatchNo; // 批次号
            macro_table["%pagecount%"] = nPageCount.ToString();
            macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            macro_table["%recpathfilepath%"] = this.RecPathFilePath;
            string strRecPathFilename = Path.GetFileName(this.RecPathFilePath);
            macro_table["%recpathfilename%"] = strRecPathFilename;
            // 批次号或文件名 多个宏不方便判断后选用，只好合并为一个宏
            macro_table["%batchno_or_recpathfilename%"] = String.IsNullOrEmpty(this.BatchNo) == true ? strRecPathFilename : this.BatchNo;


            string strFileNamePrefix = this.MainForm.DataDir + "\\~printorder";

            string strFileName = "";

            // 输出信息页
            {
                int nItemCount = items.Count;
                int nTotalCopies = GetOriginTotalCopies(items);
                int nTotalSeries = GetOriginTotalSeries(items);
                int nBiblioCount = GetOriginBiblioCount(items);
                string strTotalPrice = GetOriginTotalPrice(items).ToString();

                macro_table["%itemcount%"] = nItemCount.ToString(); // 事项数
                macro_table["%totalcopies%"] = nTotalCopies.ToString(); // 总册数(注意每项可以有多册)
                macro_table["%totalseries%"] = nTotalSeries.ToString(); // 套数
                macro_table["%bibliocount%"] = nBiblioCount.ToString(); // 种数
                macro_table["%totalprice%"] = strTotalPrice;    // 总价格


                macro_table["%pageno%"] = "1";

                strFileName = strFileNamePrefix + "0" + ".html";

                filenames.Add(strFileName);

                BuildOriginPageTop(option,
                    macro_table,
                    strFileName,
                    false);

                // 内容行

                StreamUtil.WriteText(strFileName,
                    "<div class='bibliocount'>种数: " + nBiblioCount.ToString() + "</div>");
                StreamUtil.WriteText(strFileName,
                    "<div class='seriescount'>套数: " + nTotalSeries.ToString() + "</div>");
                StreamUtil.WriteText(strFileName,
                    "<div class='itemcount'>册数: " + nTotalCopies.ToString() + "</div>");
                StreamUtil.WriteText(strFileName,
                    "<div class='totalprice'>总价: " + HttpUtility.HtmlEncode(strTotalPrice) + "</div>");

                BuildOriginPageBottom(option,
                    macro_table,
                    strFileName,
                    false);

            }

            // 表格页循环
            for (int i = 0; i < nTablePageCount; i++)
            {
                macro_table["%pageno%"] = (i + 1 + 1).ToString();

                strFileName = strFileNamePrefix + (i + 1).ToString() + ".html";

                filenames.Add(strFileName);

                BuildOriginPageTop(option,
                    macro_table,
                    strFileName,
                    true);
                // 行循环
                for (int j = 0; j < option.LinesPerPage; j++)
                {
                    BuildOriginTableLine(option,
                        items,
                        strFileName, i, j);
                }

                BuildOriginPageBottom(option,
                    macro_table,
                    strFileName,
                    true);
            }

            return 0;
        }

        int BuildOriginPageTop(PrintOption option,
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

            // string strCssUrl = this.MainForm.LibraryServerDir + "/orderorigin.css";
            // 2009/10/10 changed
            string strCssUrl = GetAutoCssUrl(option, "orderorigin.css");

            /*
            // 2009/10/9
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                strCssUrl = strCssFilePath;
            else
                strCssUrl = this.MainForm.LibraryServerDir + "/orderorigin.css";    // 缺省的
             * */

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
                    "<div class='pageheader'>" + HttpUtility.HtmlEncode(strPageHeaderText) + "</div>");

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
                    "<div class='tabletitle'>" + HttpUtility.HtmlEncode(strTableTitleText) + "</div>");
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
                        "<td class='" + strClass + "'>" + HttpUtility.HtmlEncode(strCaption) + "</td>");
                }

                StreamUtil.WriteText(strFileName,
                    "</tr>");

            }

            return 0;
        }

        int BuildOriginTableLine(PrintOption option,
            List<ListViewItem> items,
            string strFileName,
            int nPage,
            int nLine)
        {
            // 栏目内容
            string strLineContent = "";
            int nRet = 0;

            int nIndex = nPage * option.LinesPerPage + nLine;

            if (nIndex >= items.Count)
                goto END1;

            ListViewItem item = items[nIndex];

            if (this.MarcFilter != null)
            {
                string strError = "";
                string strMARC = "";
                string strOutMarcSyntax = "";

                // TODO: 有错误要明显报出来，否则容易在打印出来后才发现，就晚了

                // 获得MARC格式书目记录
                string strBiblioRecPath = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_BIBLIORECPATH);

                nRet = GetMarc(strBiblioRecPath,
                    out strMARC,
                    out strOutMarcSyntax,
                    out strError);
                if (nRet == -1)
                {
                    strLineContent = strError;
                    goto END1;
                }

                this.ColumnTable.Clear();   // 清除上一记录处理时残余的内容

                // 触发filter中的Record相关动作
                nRet = this.MarcFilter.DoRecord(
                    null,
                    strMARC,
                    strOutMarcSyntax,
                    nIndex,
                    out strError);
                if (nRet == -1)
                {
                    strLineContent = strError;
                    goto END1;
                }
            }


            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                /*
                int nIndex = nPage * option.LinesPerPage + nLine;

                if (nIndex >= items.Count)
                    break;

                ListViewItem item = items[nIndex];
                 * */

                string strContent = GetOriginColumnContent(item,
                    column.Name);

                if (strContent == "!!!#")
                    strContent = ((nPage * option.LinesPerPage) + nLine + 1).ToString();

                /*
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
                 * */

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
                else
                    strContent = HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>");

                string strClass = StringUtil.GetLeft(column.Name);

                strLineContent +=
                    "<td class='" + strClass + "'>" + strContent + "</td>";
            }

            END1:

            StreamUtil.WriteText(strFileName,
    "<tr class='content'>");

            /* 如果要打印空行
            if (string.IsNullOrEmpty(strLineContent) == true)
            {
                strLineContent += "<td class='blank' colspan='" + option.Columns.Count.ToString() + "'>&nbsp;</td>";
            }
             * */
            StreamUtil.WriteText(strFileName,
    strLineContent);

            StreamUtil.WriteText(strFileName,
                "</tr>");

            return 0;
        }

        // 获得栏目内容
        string GetOriginColumnContent(ListViewItem item,
            string strColumnName)
        {
            // 去掉"-- ?????"部分
            /*
            string strText = strColumnName;
            int nRet = strText.IndexOf("--", 0);
            if (nRet != -1)
                strText = strText.Substring(0, nRet).Trim();
             * */

            string strName = "";
            string strParameters = "";
            ParseNameParam(StringUtil.GetLeft(strColumnName),
                out strName,
                out strParameters);

            // 2013/3/29
            // 要求ColumnTable值
            if (strName.Length > 0 && strName[0] == '@')
            {
                strName = strName.Substring(1);
                return (string)this.ColumnTable[strName];
            }

            try
            {

                // 要中英文都可以
                switch (strName)
                {
                    case "no":
                    case "序号":
                        return "!!!#";  // 特殊值，表示序号

                    case "recpath":
                    case "记录路径":
                        return item.SubItems[ORIGIN_COLUMN_RECPATH].Text;

                    case "errorInfo":
                    case "summary":
                    case "摘要":
                        return item.SubItems[ORIGIN_COLUMN_SUMMARY].Text;

                    case "isbnIssn":
                    case "ISBN/ISSN":
                        return item.SubItems[ORIGIN_COLUMN_ISBNISSN].Text;


                    case "state":
                    case "状态":
                        return item.SubItems[ORIGIN_COLUMN_STATE].Text;

                    case "catalogNo":
                    case "书目号":
                        return item.SubItems[ORIGIN_COLUMN_CATALOGNO].Text;

                    case "seller":
                    case "书商":
                    case "渠道":
                        return item.SubItems[ORIGIN_COLUMN_SELLER].Text;

                    case "source":
                    case "经费来源":
                        return item.SubItems[ORIGIN_COLUMN_SOURCE].Text;

                    case "range":
                    case "时间范围":
                        return item.SubItems[ORIGIN_COLUMN_RANGE].Text;

                    case "issueCount":
                    case "包含期数":
                        return item.SubItems[ORIGIN_COLUMN_ISSUECOUNT].Text;

                    case "copy":
                    case "复本数":
                        return item.SubItems[ORIGIN_COLUMN_COPY].Text;

                    case "price":
                    case "单价":
                        return item.SubItems[ORIGIN_COLUMN_PRICE].Text;

                    case "totalPrice":
                    case "总价格":
                        return item.SubItems[ORIGIN_COLUMN_TOTALPRICE].Text;

                    case "orderTime":
                    case "订购时间":
                        return item.SubItems[ORIGIN_COLUMN_ORDERTIME].Text;

                    case "orderID":
                    case "订单号":
                        return item.SubItems[ORIGIN_COLUMN_ORDERID].Text;

                    case "distribute":
                    case "馆藏分配":
                        return item.SubItems[ORIGIN_COLUMN_DISTRIBUTE].Text;

                    case "class":
                    case "类别":
                        return item.SubItems[ORIGIN_COLUMN_CLASS].Text;

                    case "comment":
                    case "注释":
                    case "附注":
                        return item.SubItems[ORIGIN_COLUMN_COMMENT].Text;

                    case "batchNo":
                    case "批次号":
                    case "订购次号":
                        return item.SubItems[ORIGIN_COLUMN_BATCHNO].Text;

                    case "biblioRecpath":
                    case "种记录路径":
                        return item.SubItems[ORIGIN_COLUMN_BIBLIORECPATH].Text;

                    // 格式化以后的渠道地址
                    case "sellerAddress":
                    case "渠道地址":
                        return GetPrintableSellerAddress(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            strParameters);

                    case "sellerAddress:zipcode":
                    case "渠道地址:邮政编码":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "zipcode");

                    case "sellerAddress:address":
                    case "渠道地址:地址":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "address");

                    case "sellerAddress:department":
                    case "渠道地址:单位":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "department");

                    case "sellerAddress:name":
                    case "渠道地址:联系人":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "name");

                    case "sellerAddress:email":
                    case "渠道地址:Email地址":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "email");

                    case "sellerAddress:bank":
                    case "渠道地址:开户行":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "bank");

                    case "sellerAddress:accounts":
                    case "渠道地址:银行账号":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "accounts");

                    case "sellerAddress:payStyle":
                    case "渠道地址:汇款方式":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "payStyle");

                    case "sellerAddress:comment":
                    case "渠道地址:附注":
                        return GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "comment");
                    default:
                        {
                            // 2013/3/29
                            if (this.ColumnTable.Contains(strName) == false)
                                return "未知栏目 '" + strName + "'";

                            return (string)this.ColumnTable[strName];
                        }
                }
            }

            catch
            {
                return null;    // 表示没有这个subitem下标
            }

        }

        int BuildOriginPageBottom(PrintOption option,
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
        "<div class='pagefooter'>" + HttpUtility.HtmlEncode(strPageFooterText) + "</div>");
            }


            StreamUtil.WriteText(strFileName, "</body></html>");

            return 0;
        }

        static int GetOriginBiblioCount(List<ListViewItem> items)
        {
            List<string> paths = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strText = "";

                try
                {
                    strText = item.SubItems[ORIGIN_COLUMN_BIBLIORECPATH].Text;
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

        // 获得原始列表中的套数总和
        static int GetOriginTotalSeries(List<ListViewItem> items)
        {
            int total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strCopy = "";
                strCopy = item.SubItems[ORIGIN_COLUMN_COPY].Text;
                    // TODO: 注意检查是否有[]符号?

                string strLeftCopy = OrderDesignControl.GetCopyFromCopyString(strCopy);
                int nLeftCopy = 0;
                try
                {
                    nLeftCopy = Convert.ToInt32(strLeftCopy);
                }
                catch
                {
                    continue;
                }

                total += nLeftCopy;
            }

            return total;
        }

        // 获得原始列表中的册数总和
        static int GetOriginTotalCopies(List<ListViewItem> items)
        {
            int total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strCopy = "";
                strCopy = item.SubItems[ORIGIN_COLUMN_COPY].Text;
                // TODO: 注意检查是否有[]符号?

                string strLeftCopy = OrderDesignControl.GetCopyFromCopyString(strCopy);
                string strRightCopy = OrderDesignControl.GetRightFromCopyString(strCopy);
                int nLeftCopy = 0;
                try
                {
                    nLeftCopy = Convert.ToInt32(strLeftCopy);
                }
                catch
                {
                    continue;
                }

                int nRightCopy = 1;
                if (String.IsNullOrEmpty(strRightCopy) == false)
                {
                    try
                    {
                        nRightCopy = Convert.ToInt32(strRightCopy);
                    }
                    catch
                    {
                    }
                }

                total += nLeftCopy * nRightCopy;
            }

            return total;
        }

        static decimal GetOriginTotalPrice(List<ListViewItem> items)
        {
            decimal total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strPrice = "";

                try
                {
                    strPrice = item.SubItems[ORIGIN_COLUMN_TOTALPRICE].Text;
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


        #endregion

        // 保存对原始数据的修改
        private void button_saveChange_saveChange_Click(object sender, EventArgs e)
        {
            // 组织成批保存 SetOrders
            string strError = "";
            int nRet = SaveOrders(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
                this.m_nSavedCount++;

                // 警告 验收情况 状态
                if (this.checkBox_print_accepted.Checked == true)
                {
                    MessageBox.Show(this, "注意，当前状态为 “打印验收情况”，您所保存的对数据的修改，未包含对订购记录状态和订购时间字段的修改。也就是说，保存数据修改后，相关新订购数据并未变成“已订购”状态，也就不能用于验收(原来已经打印过订单的订购数据不受影响，可以验收)。\r\n\r\n如果要进行普通订单打印，需要先在本窗口“装载”属性页中清除对“验收情况”的勾选，然后重新装载数据");
                }

                // 刷新Origin的深浅间隔色
                if (this.SortColumns_origin.Count > 0)
                {
                    SetGroupBackcolor(
                        this.listView_origin,
                        this.SortColumns_origin[0].No);
                }

                this.SetNextButtonEnable();
            }
        }

        // 去除第一个字符。这个字符是表示内容是否修改过
        static string RemoveChangedChar(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return strText;
            if (strText[0] == '*')
                return strText.Substring(1);
            return strText;
        }

        static void RemoveChangedChar(ListViewItem item, int nCol)
        {
            ListViewUtil.ChangeItemText(item, nCol,
                RemoveChangedChar(ListViewUtil.GetItemText(item, nCol)));
        }

        // 保存对原始订购记录的修改
        int SaveOrders(out string strError)
        {
            strError = "";
            int nRet = 0;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存原始记录 ...");
            stop.BeginLoop();

            try
            {
                string strPrevBiblioRecPath = "";
                List<EntityInfo> entity_list = new List<EntityInfo>();
                for (int i = 0; i < this.listView_origin.Items.Count; i++)
                {
                    ListViewItem item = this.listView_origin.Items[i];

                    if (item.ImageIndex == TYPE_ERROR)
                    {
                        strError = "原始数据列表中，第 " + (i+1).ToString() + " 个事项为错误状态。需要先排除问题才能进行保存。";
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
                    Debug.Assert(item.ImageIndex != TYPE_NORMAL, "data.Changed状态为true的事项，ImageIndex不应为TYPE_NORMAL");

                    string strBiblioRecPath = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_BIBLIORECPATH);

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
                        strError = "order record XML装载到DOM时发生错误: " + ex.Message;
                        return -1;
                    }

                    DomUtil.SetElementText(dom.DocumentElement,
                        "state",
                        RemoveChangedChar(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_STATE)));
                    RemoveChangedChar(item, ORIGIN_COLUMN_STATE);

                    DomUtil.SetElementText(dom.DocumentElement,
                        "totalPrice",
                        RemoveChangedChar(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_TOTALPRICE)));
                    RemoveChangedChar(item, ORIGIN_COLUMN_TOTALPRICE);

                    string strOrderTime = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERTIME);
                    if (string.IsNullOrEmpty(strOrderTime) == false)
                    {
                        DateTime order_time;
                        try
                        {
                            order_time = DateTime.Parse(strOrderTime);
                        }
                        catch (Exception ex)
                        {
                            strError = "日期字符串 '" + strOrderTime + "' 格式错误：" + ex.Message;
                            return -1;
                        }

                        DomUtil.SetElementText(dom.DocumentElement,
                            "orderTime",
                            // DateTimeUtil.Rfc1123DateTimeString(order_time.ToUniversalTime()));
                            DateTimeUtil.Rfc1123DateTimeStringEx(order_time));
                    }

                    DomUtil.SetElementText(dom.DocumentElement,
                        "orderID", 
                        ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERID));

                    EntityInfo info = new EntityInfo();

                    if (String.IsNullOrEmpty(data.RefID) == true)
                    {
                        data.RefID = Guid.NewGuid().ToString();
                    }

                    info.RefID = data.RefID;
                    info.Action = "change";
                    info.OldRecPath = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_RECPATH);
                    info.NewRecPath = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_RECPATH);

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
            long lRet = Channel.SetOrders(
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
                    SetItemColor(item, TYPE_NORMAL);
                    continue;
                }

                if (errorinfos[0].ErrorCode == ErrorCodeValue.TimestampMismatch)
                {
                    // 时间戳冲突
                    if (String.IsNullOrEmpty(strErrorText) == false)
                        strErrorText += ";\r\n";
                    strErrorText += "事项 '" + ListViewUtil.GetItemText(item, ORIGIN_COLUMN_RECPATH) + "' 在保存过程中出现时间戳冲突。请重新装载原始数据，然后进行修改和保存。";
                }
                else
                {
                    // 其他错误
                    if (String.IsNullOrEmpty(strErrorText) == false)
                        strErrorText += ";\r\n";
                    strErrorText += "事项 '" + ListViewUtil.GetItemText(item, ORIGIN_COLUMN_RECPATH) + "' 在保存过程中发生错误: " + errorinfo.ErrorInfo;
                }

                ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ERRORINFO, errorinfo.ErrorInfo);
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
            for (int i = 0; i < this.listView_origin.Items.Count; i++)
            {
                item = this.listView_origin.Items[i];
                OriginItemData data = (OriginItemData)item.Tag;
                if (data.RefID == strRefID)
                    return data;
            }

            item = null;
            return null;
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                for (int i = 0; i < this.listView_origin.Items.Count; i++)
                {
                    OriginItemData data = (OriginItemData)this.listView_origin.Items[i].Tag;
                    if (data.Changed == true)
                        return true;
                }

                return false;
            }
        }

        List<OutputProjectData> formats = new List<OutputProjectData>();


        int PrepareFormats(List<OutputItem> OutputItems,
            out string strError)
        {
            strError = "";

            // OutputProjectData数组，先根据对话框创建一个，可能其中事项会不全，也有可能某些事项因为数据中渠道没有出现而用不到
            // 在输出订单过程中，再根据需要增补OutputProjectData数组的事项，不过，增补的事项肯定不是具有c#代码的事项，而是一些内置格式事项(特征是格式名有<>符号包着)

            this.formats.Clear();
            for (int i = 0; i < OutputItems.Count; i++)
            {
                OutputItem item = OutputItems[i];
                OutputProjectData format = new OutputProjectData();
                format.Seller = item.Seller;
                format.ProjectName = item.OutputFormat;

                if (String.IsNullOrEmpty(format.ProjectName) == true)
                    format.ProjectName = "<default>";

                if (format.ProjectName[0] != '<')
                {
                    // 
                    string strProjectLocate = "";
                    // 获得方案参数
                    // strProjectNamePath	方案名，或者路径
                    // return:
                    //		-1	error
                    //		0	not found project
                    //		1	found
                    int nRet = this.ScriptManager.GetProjectData(
                        format.ProjectName,
                        out strProjectLocate);
                    if (nRet == 0)
                    {
                        strError = "方案 " + format.ProjectName + " 没有找到...";
                        return -1;
                    }
                    if (nRet == -1)
                    {
                        strError = "scriptManager.GetProjectData() error ...";
                        return -1;
                    }

                    format.ProjectLocate = strProjectLocate;

                    OutputOrder objOutputOrder = null;
                    Assembly AssemblyMain = null;

                            // 准备脚本环境
                    nRet = PrepareScript(format.ProjectName,
                        format.ProjectLocate,
                        out objOutputOrder,
                        out AssemblyMain,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    format.Assembly = AssemblyMain;
                    format.OutputOrder = objOutputOrder;

                    // 触发Assembly初始化动作
                    format.OutputOrder.PrintOrderForm = this;
                    format.OutputOrder.PubType = this.comboBox_load_type.Text;
                    format.OutputOrder.DataDir = this.MainForm.DataDir;
                    format.OutputOrder.XmlFilename = "";    // 尚未进行输出
                    format.OutputOrder.OutputDir = "";  // 尚未进行输出
                    bool bRet = format.OutputOrder.Initial(out strError);
                    if (bRet == false)
                    {
                        strError = "初始化输出格式 '" + format.ProjectName + "' 失败: " + strError;
                        return -1;
                    }

                }

                this.formats.Add(format);
            }

            return 0;
        }

        // 输出订单
        private void button_print_outputOrder_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 设定订单输出格式
            OrderFileDialog dlg = new OrderFileDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.ScriptManager = this.ScriptManager;
            dlg.AppInfo = this.MainForm.AppInfo;
            dlg.DataDir = this.MainForm.DataDir;

            string strPrefix = "";
            if (this.comboBox_load_type.Text == "图书")
                strPrefix = "book";
            else
                strPrefix = "series";

            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

            dlg.CfgFileName = PathUtil.MergePath(this.MainForm.DataDir, strPrefix + "_order_output_def.xml");   // 格式配置信息是要分出版物类型的
            dlg.Text = this.comboBox_load_type.Text + " 订单输出格式";
            dlg.RunMode = true;
            // dlg.StartPosition = FormStartPosition.CenterScreen;
            this.MainForm.AppInfo.LinkFormState(dlg, "printorder_outputorder_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // OutputProjectData数组，先根据对话框创建一个，可能其中事项会不全，也有可能某些事项因为数据中渠道没有出现而用不到
            // 在输出订单过程中，再根据需要增补OutputProjectData数组的事项，不过，增补的事项肯定不是具有c#代码的事项，而是一些内置格式事项(特征是格式名有<>符号包着)
            nRet = PrepareFormats(dlg.OutputItems,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 输出订单
            nRet = OutputOrder(dlg.OutputFolder,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 打开订单输出目录文件夹
            DialogResult result = MessageBox.Show(this,
                "订单输出完成。共输出订单 "+nRet.ToString()+" 个。\r\n\r\n是否立即打开订单输出目录文件夹? ",
                "PrintOrderForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(dlg.OutputFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 输出选项
        private void button_print_outputOrderOption_Click(object sender, EventArgs e)
        {
            // string strError = "";
            // int nRet = 0;

            // 设定订单输出格式
            OrderFileDialog dlg = new OrderFileDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.ScriptManager = this.ScriptManager;
            dlg.AppInfo = this.MainForm.AppInfo;
            dlg.DataDir = this.MainForm.DataDir;

            string strPrefix = "";
            if (this.comboBox_load_type.Text == "图书")
                strPrefix = "book";
            else
                strPrefix = "series";

            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

            dlg.CfgFileName = PathUtil.MergePath(this.MainForm.DataDir, strPrefix + "_order_output_def.xml");   // 格式配置信息是要分出版物类型的
            dlg.Text = "配置 " + this.comboBox_load_type.Text + " 订单输出格式";
            dlg.RunMode = false;
            // dlg.StartPosition = FormStartPosition.CenterScreen;
            this.MainForm.AppInfo.LinkFormState(dlg, "printorder_outputorder_potion_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult != DialogResult.OK)
                return;

            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
        }

        void dlg_GetValueTable(object sender, GetValueTableEventArgs e)
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

        private void scriptManager_CreateDefaultContent(object sender, CreateDefaultContentEventArgs e)
        {
            string strPureFileName = Path.GetFileName(e.FileName);

            if (String.Compare(strPureFileName, "main.cs", true) == 0)
            {
                CreateDefaultMainCsFile(e.FileName);
                e.Created = true;
            }
                /*
            else if (String.Compare(strPureFileName, "marcfilter.fltx", true) == 0)
            {
                CreateDefaultMarcFilterFile(e.FileName);
                e.Created = true;
            }*/
            else
            {
                e.Created = false;
            }

        }

        // 创建缺省的main.cs文件
        /// <summary>
        /// 创建缺省的 main.cs 文件
        /// </summary>
        /// <param name="strFileName">文件名全路径</param>
        public static void CreateDefaultMainCsFile(string strFileName)
        {
            StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8);
            sw.WriteLine("using System;");
            sw.WriteLine("using System.Windows.Forms;");
            sw.WriteLine("using System.IO;");
            sw.WriteLine("using System.Text;");
            sw.WriteLine("using System.Xml;");
            sw.WriteLine("");

            //sw.WriteLine("using DigitalPlatform.MarcDom;");
            //sw.WriteLine("using DigitalPlatform.Statis;");
            sw.WriteLine("using dp2Circulation;");

            sw.WriteLine("using DigitalPlatform.Xml;");


            sw.WriteLine("public class MyOutputOrder : OutputOrder");

            sw.WriteLine("{");

            sw.WriteLine("	public override void Output()");
            sw.WriteLine("	{");
            sw.WriteLine("	}");


            sw.WriteLine("}");
            sw.Close();
        }

        // 删除输出目录中的全部文件
        void DeleteAllFiles(string strDir)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(strDir);

                FileInfo[] fis = di.GetFiles();

                if (fis.Length == 0)
                    return;

                // 警告要删除
                DialogResult result = MessageBox.Show(this,
                    "输出订单前，确实要删除输出目录 "+strDir+" 内已有的全部 "+fis.Length.ToString()+" 个文件?",
                    "PrintOrderForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
  
                for (int i = 0; i < fis.Length; i++)
                {
                    try
                    {
                        File.Delete(fis[i].FullName);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        // 输出电子订单
        int OutputOrder(
            string strOutputDir,
            out string strError)
        {
            strError = "";

            if (this.listView_merged.Items.Count == 0)
            {
                strError = "“合并后”列表中没有任何事项，无订单内容可输出";
                return -1;
            }

            // 确保目录存在
            PathUtil.CreateDirIfNeed(strOutputDir);

            // 提示是否删除输出目录中的现有文件
            // 如果正好为当前数据记录，则不删除，以免无意删除很多有用的文件
            if (this.MainForm.DataDir.ToLower() != strOutputDir.ToLower())
                DeleteAllFiles(strOutputDir);

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在输出订单 ...");
            stop.BeginLoop();

            NamedListViewItemsCollection lists = new NamedListViewItemsCollection();

            try
            {

                int nErrorCount = 0;

                this.tabControl_items.SelectedTab = this.tabPage_mergedItems;

                // 先检查是否有错误事项，顺便构建item列表
                List<ListViewItem> items = new List<ListViewItem>();

                stop.SetMessage("正在构造Item列表...");
                for (int i = 0; i < this.listView_merged.Items.Count; i++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断1";
                            return -1;
                        }
                    }

                    ListViewItem item = this.listView_merged.Items[i];

                    if (item.ImageIndex == TYPE_ERROR)
                        nErrorCount++;

                    lists.AddItem(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                        item);
                }

                if (nErrorCount != 0)
                {
                    MessageBox.Show(this, "警告：这里输出的订单，含有 " + nErrorCount.ToString() + " 个有错误信息的事项。");
                }

                for (int i = 0; i < lists.Count; i++)
                {
                    string strOutputFilename = PathUtil.MergePath(strOutputDir, lists[i].Seller + ".xml");

                    int nRet = OutputOneOrder(lists[i],
                        strOutputDir,
                        strOutputFilename,
                        out strError);
                    if (nRet == -1)
                        return -1;

                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("输出订单完成。共输出订单 " + lists.Count.ToString() + " 个");
                stop.HideProgress();

                EnableControls(true);
            }

            return lists.Count;
        }

        // 输出有关一个渠道的订单
        int OutputOneOrder(NamedListViewItems items,
            string strOutputDir,
            string strOutputFilename,
            out string strError)
        {
            strError = "";

            XmlTextWriter writer = new XmlTextWriter(strOutputFilename, Encoding.UTF8);
			writer.Formatting = Formatting.Indented;
			writer.Indentation = 4;

            writer.WriteStartDocument();
            writer.WriteStartElement("order");

            string strSeller = items.Seller;

            writer.WriteStartElement("generalInfo");

            // 渠道名
            writer.WriteElementString("seller", strSeller);
            // 创建日期
            writer.WriteElementString("createDate", DateTime.Now.ToLongDateString());
            // 批次号
            writer.WriteElementString("batchNo", this.BatchNo);
            // 记录路径文件名 全路径
            writer.WriteElementString("recPathFilename", this.RecPathFilePath);

            writer.WriteEndElement();

            // 统计信息
            {
                writer.WriteStartElement("statisInfo");

                int nItemCount = items.Count;
                int nTotalSeries = GetMergedTotalSeries(items);
                int nTotalCopies = GetMergedTotalCopies(items);
                int nBiblioCount = GetMergedBiblioCount(items);
                string strTotalPrice = GetMergedTotalPrice(items);

                // 事项数
                writer.WriteElementString("itemCount", nItemCount.ToString());
                // 总套数(注意每项可以有多套。一套内可能有多册)
                writer.WriteElementString("totalseries", nTotalSeries.ToString());
                // 总册数(注意一套内可能有多册)
                writer.WriteElementString("totalcopies", nTotalCopies.ToString());
                // 种数
                writer.WriteElementString("titleCount", nBiblioCount.ToString());
                // 总价格 可能为多个币种的价格串联形态
                writer.WriteElementString("totalPrice", strTotalPrice);

                writer.WriteEndElement();
            }

            stop.SetProgressRange(0, items.Count);

            // 内容行
            for (int i = 0; i < items.Count; i++)
            {
                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断2";
                        return -1;
                    }
                }

                ListViewItem item = items[i];

                writer.WriteStartElement("item");

                // 序号，从0开始
                writer.WriteAttributeString("index", i.ToString());

                // catalogNo
                writer.WriteElementString("catalogNo",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_CATALOGNO));

                // summary
                writer.WriteElementString("summary",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_SUMMARY));

                // isbn/issn
                writer.WriteElementString("isbnIssn",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_ISBNISSN));

                // merge comment
                writer.WriteElementString("mergeComment",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_MERGECOMMENT));

                // range
                writer.WriteElementString("range",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_RANGE));

                // issue count
                writer.WriteElementString("issueCount",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_ISSUECOUNT));

                // copy
                writer.WriteElementString("copy",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_COPY));

                // price
                writer.WriteElementString("price",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_PRICE));

                // total price
                writer.WriteElementString("totalPrice",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_TOTALPRICE));

                // order time
                writer.WriteElementString("orderTime",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_ORDERTIME));

                // order ID
                writer.WriteElementString("orderID",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_ORDERID));

                // distribute
                writer.WriteElementString("distribute",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_DISTRIBUTE));

                // class
                writer.WriteElementString("class",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_CLASS));

                // comment
                writer.WriteElementString("comment",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_COMMENT));

                // biblio recpath
                writer.WriteElementString("biblioRecpath",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_BIBLIORECPATH));

                writer.WriteEndElement();   // of item

                stop.SetMessage("正在输出事项 " + strSeller + " " + (i + 1).ToString());

                stop.SetProgressValue(i);
            }

            writer.WriteEndElement();   // of order
            writer.WriteEndDocument();
            writer.Close();
            writer = null;

            // 执行脚本
            OutputProjectData format = GetFormat(strSeller);
            if (format == null)
                return 0;   // 缺省格式。是否还需要对数据进行添加呢？

            // 内置的格式
            if (format.ProjectName[0] == '<')
            {
                if (format.ProjectName == "<default>"
                    || format.ProjectName == "<缺省>")
                    return 0;

                strError = "未知的内置格式 '" + format.ProjectName + "'";
                return -1;
            }

            // 运行Script

            try
            {
                Debug.Assert(format.OutputOrder != null, "");
                Debug.Assert(format.Assembly != null, "");

                format.OutputOrder.XmlFilename = strOutputFilename;
                format.OutputOrder.Seller = format.Seller;
                format.OutputOrder.DataDir = this.MainForm.DataDir;
                format.OutputOrder.OutputDir = strOutputDir;
                format.OutputOrder.PubType = this.comboBox_load_type.Text;

                // 执行脚本的Output()
                format.OutputOrder.Output();
            }
            catch (Exception ex)
            {
                strError = "脚本执行过程抛出异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        /*
        // 输出有关一个渠道的订单
        int OutputOneOrder(NamedListViewItems items,
            string strOutputFilename,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<order />");

            string strSeller = items.Seller;

            // 渠道名
            DomUtil.SetElementText(dom.DocumentElement,
                "seller", strSeller);
            // 创建日期
            DomUtil.SetElementText(dom.DocumentElement,
                "createDate", DateTime.Now.ToLongDateString());
            // 批次号
            DomUtil.SetElementText(dom.DocumentElement, 
                "batchNo", this.BatchNo);
            // 记录路径文件名 全路径
            DomUtil.SetElementText(dom.DocumentElement,
                "recPathFilename", this.RecPathFilePath);

            // 统计信息
            {
                int nItemCount = items.Count;
                int nTotalCopies = GetMergedTotalCopies(items);
                int nBiblioCount = GetMergedBiblioCount(items);
                string strTotalPrice = GetMergedTotalPrice(items);

                // 事项数
                DomUtil.SetElementText(dom.DocumentElement,
                    "itemCount", nItemCount.ToString());
                // 总册数(注意每项可以有多册)
                DomUtil.SetElementText(dom.DocumentElement,
                    "totalcopies", nTotalCopies.ToString());
                // 种数
                DomUtil.SetElementText(dom.DocumentElement,
                   "titleCount", nBiblioCount.ToString());
                // 总价格 可能为多个币种的价格串联形态
                DomUtil.SetElementText(dom.DocumentElement,
                   "totalPrice", strTotalPrice);
            }

            stop.SetProgressRange(0, items.Count);

            // 内容行
            for (int i = 0; i < items.Count; i++)
            {
                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断2";
                        return -1;
                    }
                }

                ListViewItem item = items[i];

                XmlNode node = dom.CreateElement("item");
                dom.DocumentElement.AppendChild(node);

                // 序号，从0开始
                DomUtil.SetAttr(node, "index", i.ToString());

                // catalogNo
                DomUtil.SetElementText(node,
                    "catalogNo",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_CATALOGNO));

                // summary
                DomUtil.SetElementText(node,
                    "summary",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_SUMMARY));

                // isbn/issn
                DomUtil.SetElementText(node,
                    "isbnIssn",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_ISBNISSN));

                // merge comment
                DomUtil.SetElementText(node,
                    "mergeComment",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_MERGECOMMENT));

                // range
                DomUtil.SetElementText(node,
                    "range",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_RANGE));

                // issue count
                DomUtil.SetElementText(node,
                    "issueCount",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_ISSUECOUNT));

                // copy
                DomUtil.SetElementText(node,
                    "copy",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_COPY));

                // price
                DomUtil.SetElementText(node,
                    "price",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_PRICE));
                    
                // total price
                DomUtil.SetElementText(node,
                    "totalPrice",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_TOTALPRICE));

                // order time
                DomUtil.SetElementText(node,
                    "orderTime",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_ORDERTIME));

                // order ID
                DomUtil.SetElementText(node,
                    "orderID",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_ORDERID));

                // distribute
                DomUtil.SetElementText(node,
                    "distribute",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_DISTRIBUTE));

                // class
                DomUtil.SetElementText(node,
                    "class",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_CLASS));

                // comment
                DomUtil.SetElementText(node,
                    "comment",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_COMMENT));

                // biblio recpath
                DomUtil.SetElementText(node,
                    "biblioRecpath",
                    ListViewUtil.GetItemText(item, MERGED_COLUMN_BIBLIORECPATH));

                stop.SetMessage("正在输出事项 " + strSeller + " " + (i + 1).ToString());

                stop.SetProgressValue(i);
            }

            dom.Save(strOutputFilename);

            // 执行脚本
            OutputProjectData format = GetFormat(strSeller);
            if (format == null)
                return 0;   // 缺省格式。是否还需要对数据进行添加呢？

            // 内置的格式
            if (format.ProjectName[0] == '<')
            {
                if (format.ProjectName == "<default>"
                    || format.ProjectName == "<缺省>")
                    return 0;

                strError = "未知的内置格式 '" + format.ProjectName + "'";
                return -1;
            }

            // 运行Script

            try
            {
                Debug.Assert(format.OutputOrder != null, "");
                Debug.Assert(format.Assembly != null, "");

                format.OutputOrder.XmlFilename = strOutputFilename;
                format.OutputOrder.Seller = format.Seller;
                format.OutputOrder.DataDir = this.MainForm.DataDir;

                // 执行脚本的Output()
                format.OutputOrder.Output();
            }
            catch (Exception ex)
            {
                strError = "脚本执行过程抛出异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }
         * */

        OutputProjectData GetFormat(string strSeller)
        {
            for (int i = 0; i < this.formats.Count; i++)
            {
                if (strSeller == formats[i].Seller)
                    return formats[i];
            }

            return null;
        }

        // 准备脚本环境
        int PrepareScript(string strProjectName,
            string strProjectLocate,
            out OutputOrder objOutputOrder,
            out Assembly AssemblyMain,
            out string strError)
        {
            AssemblyMain = null;
            objOutputOrder = null;
            strError = "";

            string strWarning = "";
            string strMainCsDllName = "";

            for (int AssemblyVersion = 0; ; AssemblyVersion++)
            {
                strMainCsDllName = strProjectLocate + "\\~output_order_main_" + this.GetHashCode().ToString() + "_"+ Convert.ToString(AssemblyVersion++) + ".dll";    // ++
                bool bFound = false;
                for (int i = 0; i < UsedAssemblyFilenames.Count; i++)
                {
                    string strName = this.UsedAssemblyFilenames[i];
                    if (strMainCsDllName == strName)
                    {
                        bFound = true;
                        break;
                    }
                }
                if (bFound == false)
                {
                    this.UsedAssemblyFilenames.Add(strMainCsDllName);
                    break;
                }
            }


            string strLibPaths = "\"" + this.MainForm.DataDir + "\""
                + ","
                + "\"" + strProjectLocate + "\"";

            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",

									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe",
            };


            // 创建Project中Script main.cs的Assembly
            // return:
            //		-2	出错，但是已经提示过错误信息了。
            //		-1	出错
            int nRet = ScriptManager.BuildAssembly(
                "PrintOrderForm",
                strProjectName,
                "main.cs",
                saAddRef,
                strLibPaths,
                strMainCsDllName,
                out strError,
                out strWarning);
            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                    goto ERROR1;
                MessageBox.Show(this, strWarning);
            }

            AssemblyMain = Assembly.LoadFrom(strMainCsDllName);
            if (AssemblyMain == null)
            {
                strError = "LoadFrom " + strMainCsDllName + " fail";
                goto ERROR1;
            }

            // 得到Assembly中XmlStatis派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                AssemblyMain,
                "dp2Circulation.OutputOrder");
            if (entryClassType == null)
            {
                strError = strMainCsDllName + "中没有找到 dp2Circulation.OutputOrder 的派生类。";
                goto ERROR1;
            }
            // new一个OutputOrder派生对象
            objOutputOrder = (OutputOrder)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为OutputOrder派生类设置参数
            objOutputOrder.PrintOrderForm = this;
            objOutputOrder.ProjectDir = strProjectLocate;

            return 0;
        ERROR1:
            return -1;
        }

        // 当出版物类型改变后，要清除遗留的列表框事项
        private void comboBox_load_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.listView_origin.Items.Clear();
            this.SortColumns_origin.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_origin.Columns);


            this.listView_merged.Items.Clear();
            this.SortColumns_merged.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_merged.Columns);
        }

        private void PrintOrderForm_Activated(object sender, EventArgs e)
        {
            // 2009/8/13
            this.MainForm.stopManager.Active(this.stop);

        }

        private void checkBox_print_accepted_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_print_accepted.Checked == true)
            {
                // TODO：是否提醒重新装载或者刷新？ 因为有些列可能没有装入进来？
                this.button_print_printOrderList.Text = "打印验收情况清单(&P)...";
                this.button_print_arriveRatioStatis.Enabled = true;
            }
            else
            {
                this.button_print_printOrderList.Text = "打印订单(&P)...";
                this.button_print_arriveRatioStatis.Enabled = false;
            }
        }

        private void listView_origin_DoubleClick(object sender, EventArgs e)
        {
            LoadOrderToEntityForm(this.listView_origin);
        }

        #region 到书率分时间片统计

        // 到货率统计
        private void button_print_arriveRatioStatis_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = PrintArriveRatio("html", out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 到货率统计
        // parameters:
        //      strStyle    excel / html 之一或者逗号联接组合。 excel: 输出 Excel 文件
        int PrintArriveRatio(
            string strStyle,
            out string strError)
        {
            strError = "";
            int nErrorCount = 0;

            ExcelDocument doc = null;

            if (StringUtil.IsInList("excel", strStyle) == true)
            {
                // 询问文件名
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.Title = "请指定要输出的 Excel 文件名";
                dlg.CreatePrompt = false;
                dlg.OverwritePrompt = true;
                dlg.FileName = this.ExportExcelFilename;
                // dlg.InitialDirectory = Environment.CurrentDirectory;
                dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return 0;

                this.ExportExcelFilename = dlg.FileName;

#if NO
                SpreadsheetDocument spreadsheetDocument = null;
                spreadsheetDocument = SpreadsheetDocument.Create(this.ExportExcelFilename, SpreadsheetDocumentType.Workbook);

                doc = new ExcelDocument(spreadsheetDocument);
#endif
                try
                {
                    doc = ExcelDocument.Create(this.ExportExcelFilename);
                }
                catch (Exception ex)
                {
                    strError = "PrintOrderForm ExcelDocument.Create() {41833E03-BE49-47A6-8DD6-22BB3D6ED007} exception: " + ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

                doc.Stylesheet = GenerateStyleSheet();
            }

            this.tabControl_items.SelectedTab = this.tabPage_mergedItems;

            EnableControls(false);
            // MainForm.ShowProgress(true);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在归并订购时间 ...");
            stop.BeginLoop();

            try
            {
                NamedListViewItemsCollection lists = new NamedListViewItemsCollection();

                DateTime last_ordertime = new DateTime(0);
                // 先检查是否有错误事项，顺便构建item列表
                // List<ListViewItem> items = new List<ListViewItem>();
                for (int i = 0; i < this.listView_merged.Items.Count; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    ListViewItem item = this.listView_merged.Items[i];

                    if (item.ImageIndex == TYPE_ERROR)
                        nErrorCount++;

                    // 测算最早的订购时间
                    string strOrderTime = ListViewUtil.GetItemText(item, MERGED_COLUMN_ORDERTIME);
                    if (string.IsNullOrEmpty(strOrderTime) == false)
                    {
                        DateTime order_time;
                        try
                        {
                            order_time = DateTime.Parse(strOrderTime);
                        }
                        catch (Exception ex)
                        {
                            strError = "行 "+(i+1).ToString()+" 日期字符串 '" + strOrderTime + "' 格式错误：" + ex.Message;
                            goto ERROR1;
                        }

                        if (last_ordertime == new DateTime(0))
                            last_ordertime = order_time;
                        else
                        {
                            if (order_time < last_ordertime)
                                last_ordertime = order_time;
                        }
                    }

                    lists.AddItem(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                        item);
                }

                if (nErrorCount != 0)
                {
                    MessageBox.Show(this, "警告：这里打印出的清单，含有 " + nErrorCount.ToString() + " 个包含错误信息的事项。");
                }

                DateSliceDialog dlg = new DateSliceDialog();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.OrderDate = last_ordertime;
                dlg.StartTime = last_ordertime;
                dlg.EndTime = DateTime.Now;
                dlg.QuickSet = "订购日至今日";
                dlg.Slice = this.MainForm.AppInfo.GetString(
                    "printorder_form",
                    "slice",
                    "月");
                dlg.ShowDialog(this);

                if (dlg.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return 0;

                this.MainForm.AppInfo.SetString(
                    "printorder_form",
                    "slice",
                    dlg.Slice);

                List<TimeSlice> time_ranges = dlg.TimeSlices;

                List<string> filenames = new List<string>();
                try
                {
                    // 按渠道统计到货率
                    for (int i = 0; i < lists.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        List<string> temp_filenames = null;
                        int nRet = BuildArriveHtml(lists[i],
                            ref doc,
                            time_ranges,
                            out temp_filenames,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        filenames.AddRange(temp_filenames);
                    }

                    if (doc == null)
                    {
                        HtmlPrintForm printform = new HtmlPrintForm();

                        printform.Text = "到货率统计";
                        printform.MainForm = this.MainForm;
                        printform.Filenames = filenames;
                        this.MainForm.AppInfo.LinkFormState(printform, "printorder_htmlprint_formstate");
                        printform.ShowDialog(this);
                        this.MainForm.AppInfo.UnlinkFormState(printform);
                    }
                }
                finally
                {
                    if (filenames != null)
                    {
                        Global.DeleteFiles(filenames);
                        filenames.Clear();
                    }
                }
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

            if (doc != null)
            {
                // Close the document.
                doc.Close();
            }

            return 1;
        ERROR1:
            // MessageBox.Show(this, strError);
            return -1;
        }

        // 每一个合并行内的用于统计的关键信息
        class OneLine
        {
            public ListViewItem Item = null;
            public List<OneBook> Books = null;
            public DateTime OrderTime = new DateTime(0);
        }

        // 每一个册信息
        class OneBook
        {
            public string RefID = "";
            public DateTime CreateTime = new DateTime(0);
        }


        // 获得全部册记录的 refid --> 到书时间 对照表。所谓到书时间就是记录创建的时间。如果没有记录创建时间，用最后修改时间代替
        // 册refid --> 到书时间
        int GetArriveTimes(NamedListViewItems items,
            out List<OneLine> infos,
            out string strError)
        {
            strError = "";
            infos = new List<OneLine>();
            int nRet = 0;

            foreach( ListViewItem item in items)
            {
                string strDistributes = item.SubItems[MERGED_COLUMN_DISTRIBUTE].Text;
                if (string.IsNullOrEmpty(strDistributes) == true)
                    continue;

                LocationCollection locations = new LocationCollection();
                nRet = locations.Build(strDistributes,
                    out strError);
                if (nRet == -1)
                {
                    strError = "馆藏分配字符串 '" + strDistributes + "' 格式错误: " + strError;
                    return -1;
                }

                OneLine info = new OneLine();
                info.Item = item;
                info.Books = new List<OneBook>();

                infos.Add(info);
                List<string> refids = locations.GetRefIDs();
                foreach (string s in refids)
                {
                    OneBook book = new OneBook();
                    book.RefID = s;
                    info.Books.Add(book);
                }
            }

            Hashtable table = new Hashtable();
            List<string> temp_refids = new List<string>();
            foreach (OneLine line in infos)
            {
                if (line.Books == null)
                    continue;
                foreach(OneBook book in line.Books)
                {
                    if (string.IsNullOrEmpty(book.RefID) == true)
                        continue;
                    temp_refids.Add(book.RefID);
                }

                if (temp_refids.Count >= 100)
                {
                    nRet = GetRecordTimes(temp_refids,
            ref table,
            out strError);
                    if (nRet == -1)
                        return -1;
                    temp_refids.Clear();
                }
            }

            // 最后一批
            if (temp_refids.Count > 0)
            {
                nRet = GetRecordTimes(temp_refids,
        ref table,
        out strError);
                if (nRet == -1)
                    return -1;
                temp_refids.Clear();
            }

            // 装入结构
            foreach (OneLine line in infos)
            {
                if (line.Books == null)
                    continue;
                foreach (OneBook book in line.Books)
                {
                    if (string.IsNullOrEmpty(book.RefID) == true)
                        continue;
                    object obj = table[book.RefID];
                    if (obj == null)
                        continue;   // 先前曾经出现过refid没有对应册记录的情况。book.CreateTime中就是缺省值
                    DateTime time = (DateTime)obj;
                    book.CreateTime = time;
                }
            }

            return 0;
        }

        // 根据 refid 获得一批记录的创建时间，追加到 Hashtable 中
        int GetRecordTimes(List<string> refids,
            ref Hashtable result_table,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (result_table == null)
                result_table = new Hashtable();

            Hashtable table = null;
            // 获得册记录信息
            nRet = LoadItemRecord(
                refids,
                ref table,
                out strError);
            if (nRet == -1)
                return -1;
            foreach (string key in table.Keys)
            {
                string strXml = (string)table[key];
                if (string.IsNullOrEmpty(strXml) == true)
                {
                    Debug.Assert(false, "");
                    continue;
                }
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "item xml load to dom error: " + ex.Message;
                    return -1;
                }

                XmlNode node = dom.DocumentElement.SelectSingleNode("operations/operation[@name='create']");
                if (node == null)
                    node = dom.DocumentElement.SelectSingleNode("operations/operation");

                if (node == null)
                    continue;
                string strTime = DomUtil.GetAttr(node, "time");
                try
                {
                    DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strTime);
                    result_table[key] = time;
                }
                catch(Exception /*ex*/)
                {
                    strError = "refid为 '"+key+"' 的记录中RFC1123字符串 '"+strTime+"' 格式不正确";
                    return -1;
                }

            }

            return 0;
        }

        class OneItemRecord
        {
            public string RefID = "";
            public string RecPath = "";
            public string Xml = "";
        }

        // 根据册记录refid，转换为册记录的recpath，然后获得册记录XML
        int LoadItemRecord(
            List<string> refids,
            ref Hashtable table,
            out string strError)
        {
            strError = "";
            if (table == null)
                table = new Hashtable();

        REDO_GETITEMINFO:
            string strBiblio = "";
            string strResult = "";
            long lRet = this.Channel.GetItemInfo(stop,
                "@refid-list:" + StringUtil.MakePathList(refids),
                "get-path-list",
                out strResult,
                "", // strBiblioType,
                out strBiblio,
                out strError);
            if (lRet == -1)
                return -1;

            List<string> recpaths = StringUtil.SplitList(strResult);
            Debug.Assert(refids.Count == recpaths.Count, "");

            List<OneItemRecord> records = new List<OneItemRecord>(); 
            List<string> notfound_refids = new List<string>();
            List<string> errors = new List<string>();
            {
                int i = 0;
                foreach (string recpath in recpaths)
                {
                    if (string.IsNullOrEmpty(recpath) == true)
                        notfound_refids.Add(refids[i]);
                    else if (recpath[0] == '!')
                        errors.Add(recpath.Substring(1));
                    else
                    {
                        OneItemRecord record = new OneItemRecord();
                        record.RefID = refids[i];
                        record.RecPath = recpath;
                        records.Add(record);
                    }
                    i++;
                }
            }

            if (errors.Count > 0)
            {
                strError = "转换参考ID的过程发生错误: " + StringUtil.MakePathList(errors);

                DialogResult temp_result = MessageBox.Show(this,
strError + "\r\n\r\n是否重试?",
"PrintOrderForm",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (temp_result == DialogResult.Retry)
                    goto REDO_GETITEMINFO;
                return -1;
            }

            if (notfound_refids.Count > 0)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    strError += ";\r\n";

                strError += "下列册记录参考ID没有找到: " + StringUtil.MakePathList(notfound_refids);
                DialogResult temp_result = MessageBox.Show(this,
strError + "\r\n\r\n是否继续处理?",
"PrintOrderForm",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (temp_result == DialogResult.Cancel)
                    return -1;
            }

            // 成批获得册记录
            List<string> item_recpaths = new List<string>();
            foreach (OneItemRecord record in records)
            {
                if (String.IsNullOrEmpty(record.RecPath) == false)
                    item_recpaths.Add(record.RecPath);
            }

            if (item_recpaths.Count > 0)
            {
                // 记录路径 --> XML记录体
                Hashtable result_table = new Hashtable();
                List<DigitalPlatform.CirculationClient.localhost.Record> results = new List<DigitalPlatform.CirculationClient.localhost.Record>();

                // 集中获取全部册记录信息
                for (; ; )
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断1";
                            return -1;
                        }
                    }

                    DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                    string[] paths = new string[item_recpaths.Count];
                    item_recpaths.CopyTo(paths);
                REDO_GETRECORDS:
                    lRet = this.Channel.GetBrowseRecords(
                        this.stop,
                        paths,
                        "id,xml",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        DialogResult temp_result = MessageBox.Show(this,
        strError + "\r\n\r\n是否重试?",
        "AccountBookForm",
        MessageBoxButtons.RetryCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_GETRECORDS;
                        return -1;
                    }

                    foreach (DigitalPlatform.CirculationClient.localhost.Record record in searchresults)
                    {
                        if (record.RecordBody == null || string.IsNullOrEmpty(record.Path) == true)
                            continue;

                        result_table[record.Path] = record.RecordBody.Xml;
                    }

                    item_recpaths.RemoveRange(0, searchresults.Length);

                    if (item_recpaths.Count == 0)
                        break;
                }

                // 设置好XML字符串
                foreach (OneItemRecord record in records)
                {
                    if (String.IsNullOrEmpty(record.RecPath) == true)
                        continue;

                    string strXml = (string)result_table[record.RecPath];
                    record.Xml = strXml;
                }

                // 最后放入table中
                foreach (OneItemRecord record in records)
                {
                    if (String.IsNullOrEmpty(record.RecPath) == true
                        || string.IsNullOrEmpty(record.Xml) == true)
                        continue;

                    table[record.RefID] = record.Xml;
                }
            }

            return 0;
        }

        // 计算特定时间范围内到达的种册数
        static int GetValues(DateTime start,
            DateTime end,
            List<OneLine> infos,
            out long lBiblioCount,
            out long lItemCount,
            out string strError)
        {
            strError = "";
            lBiblioCount = 0;
            lItemCount = 0;

            foreach (OneLine line in infos)
            {
                if (line.Books == null)
                    continue;
                long lCurItemCount = 0;
                foreach (OneBook book in line.Books)
                {
                    if (book.CreateTime >= start && book.CreateTime < end)
                    {
                        lCurItemCount++;
                    }
                }
                if (lCurItemCount > 0)
                {
                    lBiblioCount++;
                    lItemCount += lCurItemCount;
                }
            }

            return 0;
        }

        // 到货率统计
        int BuildArriveHtml(
            NamedListViewItems items,
            ref ExcelDocument doc,
            List<TimeSlice> time_ranges,
            out List<string> filenames,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名

            stop.SetMessage("正在进行到货率统计 ...");

            // 准备册记录
            List<OneLine> infos = null;
            nRet = GetArriveTimes(items,
                out infos,
                out strError);
            if (nRet == -1)
                return -1;

            Hashtable macro_table = new Hashtable();

            // 获得打印参数
            PrintOrderPrintOption option = new PrintOrderPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                "printorder_printoption");


            macro_table["%batchno%"] = this.BatchNo; // 批次号
            macro_table["%seller%"] = items.Seller; // 渠道名
            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            macro_table["%recpathfilepath%"] = this.RecPathFilePath;
            string strRecPathFilename = Path.GetFileName(this.RecPathFilePath);
            macro_table["%recpathfilename%"] = strRecPathFilename;

            // 批次号或文件名 多个宏不方便判断后选用，只好合并为一个宏
            macro_table["%batchno_or_recpathfilename%"] = String.IsNullOrEmpty(this.BatchNo) == true ? strRecPathFilename : this.BatchNo;



            // 需要将属于不同渠道的文件名前缀区别开来
            string strFileNamePrefix = this.MainForm.DataDir + "\\~printorder_" + items.GetHashCode().ToString() + "_";

            string strFileName = "";

            Sheet sheet = null;
            if (doc != null)
                sheet = doc.NewSheet("到货率统计表");


            // 输出信息页
            // TODO: 要增加“统计页”模板功能
            {
                int nItemCount = items.Count;
                int nTotalSeries = GetMergedTotalSeries(items);
                int nTotalCopies = GetMergedTotalCopies(items);
                int nBiblioCount = GetMergedBiblioCount(items);

                macro_table["%itemcount%"] = nItemCount.ToString(); // 事项数
                macro_table["%totalcopies%"] = nTotalCopies.ToString(); // 总册数
                macro_table["%totalseries%"] = nTotalSeries.ToString(); // 总套数
                macro_table["%bibliocount%"] = nBiblioCount.ToString(); // 种数

#if NO
                // 已验收的
                int nAcceptItemCount = items.Count;
                int nAcceptTotalSeries = GetMergedAcceptTotalSeries(items);
                int nAcceptTotalCopies = GetMergedAcceptTotalCopies(items);
                int nAcceptBiblioCount = GetMergedAcceptBiblioCount(items);

                macro_table["%accept_itemcount%"] = nAcceptItemCount.ToString(); // 事项数
                macro_table["%accept_totalcopies%"] = nAcceptTotalCopies.ToString(); // 总册数
                macro_table["%accept_totalseries%"] = nAcceptTotalSeries.ToString(); // 总套数
                macro_table["%accept_bibliocount%"] = nAcceptBiblioCount.ToString(); // 种数

                // 到货率
                macro_table["%ratio_itemcount%"] = GetRatioString(nAcceptItemCount, nItemCount); // 事项数
                macro_table["%ratio_totalcopies%"] = GetRatioString(nAcceptTotalCopies, nTotalCopies); // 总册数
                macro_table["%ratio_totalseries%"] = GetRatioString(nAcceptTotalSeries, nTotalSeries); // 总套数
                macro_table["%ratio_bibliocount%"] = GetRatioString(nAcceptBiblioCount, nBiblioCount); // 种数
#endif

                macro_table["%pageno%"] = "1";

                strFileName = strFileNamePrefix + "0" + ".html";

                filenames.Add(strFileName);

                BuildSlicePageTop(option,
                    macro_table,
                    strFileName);

                int nLineIndex = 2;
                if (doc != null)
                {
                    BuildSliceExcelPageTop(option,
        macro_table,
        ref doc,
        5);

                    {
                        List<string> captions = new List<string>();
                        captions.Add("时间");
                        captions.Add("种数");
                        captions.Add("种到书率");
                        captions.Add("册数");
                        captions.Add("册到书率");
                        int i = 0;
                        foreach (string strCaption in captions)
                        {
                            doc.WriteExcelCell(
                    nLineIndex,
                    i++,
                    strCaption,
                    true);
                        }
                    }
                }

                StringBuilder table_content = new StringBuilder(4096);

                table_content.Append("<table class='slice'>");

                // 表格标题行
                {
                    table_content.Append("<tr class='column'>");

                    table_content.Append("<td class='slice'>时间</td>");
                    table_content.Append("<td class='bibliocount'>种数</td>");
                    table_content.Append("<td class='biblioratio'>种到书率</td>");
                    table_content.Append("<td class='itemcount'>册数</td>");
                    table_content.Append("<td class='itemratio'>册到书率</td>");
                    table_content.Append("</tr>");
                }

                // 订购数。也就是到书率的分母部分
                {
                    table_content.Append("<tr class='order'>");

                    table_content.Append("<td class='slice'>订购数</td>");
                    table_content.Append("<td class='bibliocount'>" + nBiblioCount.ToString() + "</td>");
                    table_content.Append("<td class='biblioratio'>&nbsp;</td>");
                    table_content.Append("<td class='itemcount'>" + nTotalCopies.ToString() + "</td>");
                    table_content.Append("<td class='itemratio'>&nbsp;</td>");
                    table_content.Append("</tr>");
                }
                if (doc != null)
                {
                    nLineIndex++;
                    int i = 0;
                    doc.WriteExcelCell(
            nLineIndex,
            i++,
            "订购数",
            true);
                    doc.WriteExcelCell(
nLineIndex,
i++,
nBiblioCount.ToString(),
false);
                    doc.WriteExcelCell(
nLineIndex,
i++,
"",
true);
                    doc.WriteExcelCell(
nLineIndex,
i++,
nTotalCopies.ToString(),
false);
                }

                Debug.Assert(time_ranges.Count > 0, "");
                DateTime start = time_ranges[0].Start;
                foreach (TimeSlice slice in time_ranges)
                {
                    DateTime end = slice.Start + slice.Length;

                    long lBiblioCount = 0;
                    long lItemCount = 0;
                    // 计算特定时间范围内到达的种册数
                    nRet = GetValues(start,
                        end,
                        infos,
            out lBiblioCount,
            out lItemCount,
            out strError);
                    if (nRet == -1)
                        return -1;

                    string strTrClass = "";
                    if (string.IsNullOrEmpty(slice.Style) == false)
                        strTrClass = " class='" + slice.Style + "' ";

                    table_content.Append("<tr"+strTrClass+">");

                    table_content.Append("<td class='slice'>" + HttpUtility.HtmlEncode(slice.Caption) + "</td>");

                    string strRatioItem = GetRatioString(lItemCount, nTotalCopies); // 事项数
                    string strRatioBiblio = GetRatioString(lBiblioCount, nBiblioCount); // 种数

                    table_content.Append("<td class='bibliocount'>" + lBiblioCount.ToString() + "</td>");
                    table_content.Append("<td class='biblioratio'>" + HttpUtility.HtmlEncode(strRatioBiblio) + "</td>");
                    table_content.Append("<td class='itemcount'>" + lItemCount.ToString() + "</td>");
                    table_content.Append("<td class='itemratio'>" + HttpUtility.HtmlEncode(strRatioItem) + "</td>");

                    table_content.Append("</tr>");

                    if (doc != null)
                    {
                        nLineIndex++;
                        int i = 0;
                        doc.WriteExcelCell(
                nLineIndex,
                i++,
                slice.Caption,
                true);
                        doc.WriteExcelCell(
    nLineIndex,
    i++,
    lBiblioCount.ToString(),
    false);
                        doc.WriteExcelCell(
    nLineIndex,
    i++,
    strRatioBiblio,
    true);
                        doc.WriteExcelCell(
    nLineIndex,
    i++,
    lItemCount.ToString(),
    false);
                        doc.WriteExcelCell(
    nLineIndex,
    i++,
    strRatioItem,
    true);

                    }
                }

                table_content.Append("</table>");

                StreamUtil.WriteText(strFileName,
                    table_content.ToString());

                BuildSlicePageBottom(option,
                    macro_table,
                    strFileName);
            }

            return 0;
        }

        // 输出 Excel 页面头部信息
        int BuildSliceExcelPageTop(PrintOption option,
            Hashtable macro_table,
            ref ExcelDocument doc,
            int nTitleCols)
        {

            // 页眉
            string strPageHeaderText = "%date% %seller% 到货率统计表 - 批次号或文件名: %batchno_or_recpathfilename%";

            if (String.IsNullOrEmpty(strPageHeaderText) == false)
            {
                strPageHeaderText = StringUtil.MacroString(macro_table,
                    strPageHeaderText);
            }

            // 表格标题
            string strTableTitleText = "%date% %seller% 到货率统计表";

            // 第一行，表格标题
            if (String.IsNullOrEmpty(strTableTitleText) == false)
            {

                strTableTitleText = StringUtil.MacroString(macro_table,
                    strTableTitleText);

                doc.WriteExcelTitle(0,
                    nTitleCols,
                    strTableTitleText,
                    5);

            }




            return 0;
        }

        int BuildSlicePageTop(PrintOption option,
    Hashtable macro_table,
    string strFileName)
        {

            string strCssUrl = GetAutoCssUrl(option, "printslice.css");

            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            StreamUtil.WriteText(strFileName,
                "<!DOCTYPE html PUBLIC \" -//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">"
                + "<html><head>" + strLink + "</head><body>");

            // 页眉
            string strPageHeaderText = "%date% %seller% 到货率统计表 - 批次号或文件名: %batchno_or_recpathfilename%";

            if (String.IsNullOrEmpty(strPageHeaderText) == false)
            {
                strPageHeaderText = StringUtil.MacroString(macro_table,
                    strPageHeaderText);

                StreamUtil.WriteText(strFileName,
                    "<div class='pageheader'>" + HttpUtility.HtmlEncode(strPageHeaderText) + "</div>");
            }

            // 表格标题
            string strTableTitleText = "%date% %seller% 到货率统计表";

            if (String.IsNullOrEmpty(strTableTitleText) == false)
            {

                strTableTitleText = StringUtil.MacroString(macro_table,
                    strTableTitleText);

                StreamUtil.WriteText(strFileName,
                    "<div class='tabletitle'>" + HttpUtility.HtmlEncode(strTableTitleText) + "</div>");
            }

            return 0;
        }

        int BuildSlicePageBottom(PrintOption option,
Hashtable macro_table,
string strFileName)
        {
            // 页脚
            string strPageFooterText = "";

            if (String.IsNullOrEmpty(strPageFooterText) == false)
            {
                strPageFooterText = StringUtil.MacroString(macro_table,
                    strPageFooterText);

                StreamUtil.WriteText(strFileName,
        "<div class='pagefooter'>" + HttpUtility.HtmlEncode(strPageFooterText) + "</div>");
            }

            StreamUtil.WriteText(strFileName, "</body></html>");
            return 0;
        }

#endregion

        // 打印订单 -- 输出 Excel 文件
        private void toolStripMenuItem_outputExcel_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = PrintOrder("excel", out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 到货率统计 -- 输出 Excel 文件
        private void toolStripMenuItem_arriveRatio_outputExcel_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = PrintArriveRatio("excel", out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }

    internal class OutputProjectData
    {
        public string Seller = "";  // 渠道名

        public string ProjectName = ""; // 二次开发方案名。也是输出格式名。如果为"<default>"格式，表示为内置的输出格式，不是二次开发方案
        public string ProjectLocate = "";   // 方案所在目录

        public OutputOrder OutputOrder = null; // 宿主对象
        public Assembly Assembly = null;   // Assembly对象

        // public int AssemblyVersion = 0; // Assembly对象的版本号。这是为无法覆盖以前的Assembly文件而设置的补丁措施
    }

    // 合并后数据打印 定义了特定缺省值的PrintOption派生类
    internal class PrintOrderPrintOption : PrintOption
    {
        string PublicationType = "图书"; // 图书 连续出版物

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

        public PrintOrderPrintOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            this.PageHeaderDefault = "%date% %seller% 订单 - 批次号或文件名: %batchno_or_recpathfilename% - (共 %pagecount% 页)";
            this.PageFooterDefault = "%pageno%/%pagecount%";

            this.TableTitleDefault = "%date% %seller% 订单";

            this.LinesPerPageDefault = 20;

            // Columns缺省值
            Columns.Clear();

            // "no -- 序号",
            Column column = new Column();
            column.Name = "no -- 序号";
            column.Caption = "序号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "catalogNo -- 书目号"
            column = new Column();
            column.Name = "catalogNo -- 书目号";
            column.Caption = "书目号";
            column.MaxChars = -1;
            this.Columns.Add(column);


            // "summary -- 摘要"
            column = new Column();
            column.Name = "summary -- 摘要";
            column.Caption = "摘要";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "isbnIssn -- ISBN/ISSN"
            column = new Column();
            column.Name = "isbnIssn -- ISBN/ISSN";
            column.Caption = "ISBN/ISSN";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "price -- 单价"
            column = new Column();
            column.Name = "price -- 单价";
            column.Caption = "单价";
            column.MaxChars = -1;
            this.Columns.Add(column);


            if (this.PublicationType == "连续出版物")
            {
                // "range -- 时间范围"
                column = new Column();
                column.Name = "range -- 时间范围";
                column.Caption = "时间范围";
                column.MaxChars = -1;
                this.Columns.Add(column);

                // "issueCount -- 包含期数"
                column = new Column();
                column.Name = "issueCount -- 包含期数";
                column.Caption = "包含期数";
                column.MaxChars = -1;
                this.Columns.Add(column);
            }

            /*
            // "copy -- 复本数"
            column = new Column();
            column.Name = "copy -- 复本数";
            column.Caption = "复本数";
            column.MaxChars = -1;
            this.Columns.Add(column);
             * */

            // "series -- 套数"
            column = new Column();
            column.Name = "series -- 套数";
            column.Caption = "套数";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "totalPrice -- 总价格"
            column = new Column();
            column.Name = "totalPrice -- 总价格";
            column.Caption = "总价格";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "acceptCopy -- 已到套数"
            column = new Column();
            column.Name = "acceptCopy -- 已到套数";
            column.Caption = "已到套数";
            column.MaxChars = -1;
            this.Columns.Add(column);
        }
    }

    // 原始数据打印 定义了特定缺省值的PrintOption派生类
    internal class OrderOriginPrintOption : PrintOption
    {
        string PublicationType = "图书"; // 图书 连续出版物

        public OrderOriginPrintOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            this.PageHeaderDefault = "%date% 原始订购数据 - %recpathfilename% - (共 %pagecount% 页)";
            this.PageFooterDefault = "%pageno%/%pagecount%";

            this.TableTitleDefault = "%date% 原始订购数据";

            this.LinesPerPageDefault = 20;

            // Columns缺省值
            Columns.Clear();

            // "no -- 序号",
            Column column = new Column();
            column.Name = "no -- 序号";
            column.Caption = "序号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "catalogNo -- 书目号"
            column = new Column();
            column.Name = "catalogNo -- 书目号";
            column.Caption = "书目号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "summary -- 摘要"
            column = new Column();
            column.Name = "summary -- 摘要";
            column.Caption = "摘要";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "isbnIssn -- ISBN/ISSN"
            column = new Column();
            column.Name = "isbnIssn -- ISBN/ISSN";
            column.Caption = "ISBN/ISSN";
            column.MaxChars = -1;
            this.Columns.Add(column);


            // "price -- 单价"
            column = new Column();
            column.Name = "price -- 单价";
            column.Caption = "单价";
            column.MaxChars = -1;
            this.Columns.Add(column);

            if (this.PublicationType == "连续出版物")
            {
                // "range -- 时间范围"
                column = new Column();
                column.Name = "range -- 时间范围";
                column.Caption = "时间范围";
                column.MaxChars = -1;
                this.Columns.Add(column);

                // "issueCount -- 包含期数"
                column = new Column();
                column.Name = "issueCount -- 包含期数";
                column.Caption = "包含期数";
                column.MaxChars = -1;
                this.Columns.Add(column);
            }

            // "copy -- 复本数"
            column = new Column();
            column.Name = "copy -- 复本数";
            column.Caption = "复本数";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "totalPrice -- 总价格"
            column = new Column();
            column.Name = "totalPrice -- 总价格";
            column.Caption = "总价格";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "class -- 类别"
            column = new Column();
            column.Name = "class -- 类别";
            column.Caption = "类别";
            column.MaxChars = -1;
            this.Columns.Add(column);
        }
    }

    // 原始数据listviewitem的Tag所携带的数据结构
    internal class OriginItemData
    {
        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed = false;
        public byte[] Timestamp = null;
        public string Xml = ""; // 订购记录的XML记录体
        public string RefID = "";   // 保存记录时候用的refid
    }

    internal class StatisLine
    {
        public string Class = "";   // 分类号
        public long BiblioCount = 0;    // 种数
        public long SeriesCount = 0;      // 套数
        public long ItemCount = 0;      // 册数
        public bool AllowSum = true;    // 是否参与汇总
        public string Price = "";       // 价格字符串

        public long AcceptBiblioCount = 0;    // 已到种数
        public long AcceptSeriesCount = 0;      // 已到套数
        public long AcceptItemCount = 0;      // 已到册数
        public string AcceptPrice = "";       // 已到价格字符串

        public List<StatisLine> InnerLines = null;  // 嵌套的子表
    }

    // 排序类名。小的在前
    internal class CellStatisLineComparer : IComparer<StatisLine>
    {
        int IComparer<StatisLine>.Compare(StatisLine x, StatisLine y)
        {
            string s1 = x.Class;
            string s2 = y.Class;

            return String.Compare(s1, s2);
        }
    }
}