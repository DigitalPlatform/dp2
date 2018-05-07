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

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

// 2017/4/9 从 this.Channel 用法改造为 ChannelPool 用法

namespace dp2Circulation
{
    // 打印(采购)验收单
    /// <summary>
    /// 打印验收单窗
    /// </summary>
    public partial class PrintAcceptForm : BatchPrintFormBase
    {
        // 装载数据时的方式
        string SourceStyle = "";    // "batchno" "barcodefile" "recpathfile"

        /// <summary>
        /// 最近用过的记录路径文件全路径
        /// </summary>
        public string RecPathFilePath = "";

        // refid -- 订购记录path 对照表
        Hashtable refid_table = new Hashtable();
        // 订购记录path -- 订购记录XML对照表
        Hashtable orderxml_table = new Hashtable();

        string BatchNo = "";    // 最近在检索面板输入过的批次号

        /// <summary>
        /// 事项图标 ImageIndex : 错误
        /// </summary>
        public const int TYPE_ERROR = 0;
        /// <summary>
        /// 事项图标 ImageIndex : 普通。书本合上摸样
        /// </summary>
        public const int TYPE_NORMAL = 1;   // 书本合上摸样
        /// <summary>
        /// 事项图标 ImageIndex : 修改过。书本翻开摸样。表示原始记录已在窗口中发生了修改，并尚未保存
        /// </summary>
        public const int TYPE_CHANGED = 2;  // 书本翻开摸样。表示原始记录已在窗口中发生了修改，并尚未保存


        // 参与排序的列号数组
        SortColumns SortColumns_origin = new SortColumns();
        SortColumns SortColumns_merged = new SortColumns();

        #region 原始数据listview列号
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
        /// 原始数据列号: 出版时间
        /// </summary>
        public static int ORIGIN_COLUMN_PUBLISHTIME = 4;          // 出版时间
        /// <summary>
        /// 原始数据列号: 卷期
        /// </summary>
        public static int ORIGIN_COLUMN_VOLUME = 5;          // 卷期
        /// <summary>
        /// 原始数据列号: 馆藏地点
        /// </summary>
        public static int ORIGIN_COLUMN_LOCATION = 6;       // 馆藏地点
        /// <summary>
        /// 原始数据列号: 渠道
        /// </summary>
        public static int ORIGIN_COLUMN_SELLER = 7;        // 渠道
        /// <summary>
        /// 原始数据列号: 经费来源
        /// </summary>
        public static int ORIGIN_COLUMN_SOURCE = 8;        // 经费来源
        /// <summary>
        /// 原始数据列号: 单价
        /// </summary>
        public static int ORIGIN_COLUMN_ITEMPRICE = 9;             // 单价
        /// <summary>
        /// 原始数据列号: 附注
        /// </summary>
        public static int ORIGIN_COLUMN_COMMENT = 10;          // 附注
        /// <summary>
        /// 原始数据列号: (验收)批次号
        /// </summary>
        public static int ORIGIN_COLUMN_BATCHNO = 11;          // (验收)批次号
        /// <summary>
        /// 原始数据列号: 参考ID
        /// </summary>
        public static int ORIGIN_COLUMN_REFID = 12;          // 参考ID
        /// <summary>
        /// 原始数据列号: 种记录路径
        /// </summary>
        public static int ORIGIN_COLUMN_BIBLIORECPATH = 13;    // 种记录路径

        /// <summary>
        /// 原始数据列号: 书目号
        /// </summary>
        public static int ORIGIN_COLUMN_CATALOGNO = 14;    // 书目号
        /// <summary>
        /// 原始数据列号: 订单号
        /// </summary>
        public static int ORIGIN_COLUMN_ORDERID = 15;    // 订单号
        /// <summary>
        /// 原始数据列号: 订购类别
        /// </summary>
        public static int ORIGIN_COLUMN_ORDERCLASS = 16;    // 订购类别
        /// <summary>
        /// 原始数据列号: 订购时间
        /// </summary>
        public static int ORIGIN_COLUMN_ORDERTIME = 17;    // 订购时间
        /// <summary>
        /// 原始数据列号: (订购记录中的)订购价
        /// </summary>
        public static int ORIGIN_COLUMN_ORDERPRICE = 18;    // (订购记录中的)订购价
        /// <summary>
        /// 原始数据列号: (订购记录中的)到书价
        /// </summary>
        public static int ORIGIN_COLUMN_ACCEPTPRICE = 19;    // (订购记录中的)到书价
        /// <summary>
        /// 原始数据列号: 所关联的订购记录的渠道地址
        /// </summary>
        public static int ORIGIN_COLUMN_SELLERADDRESS = 20;    // 所关联的订购记录的渠道地址
        /// <summary>
        /// 原始数据列号: 所关联的订购记录路径
        /// </summary>
        public static int ORIGIN_COLUMN_ORDERRECPATH = 21;    // 所关联的订购记录路径
        /// <summary>
        /// 原始数据列号: 套内详情
        /// </summary>
        public static int ORIGIN_COLUMN_ACCEPTSUBCOPY = 22;    // 套内详情 1:2 第一套内的第二册
        /// <summary>
        /// 原始数据列号: 订购时的每套册数
        /// </summary>
        public static int ORIGIN_COLUMN_ORDERSUBCOPY = 23;    // 订购时的每套册数

        #endregion

        #region 合并后数据listview列号

        /// <summary>
        /// 合并后数据列号: 渠道
        /// </summary>
        public static int MERGED_COLUMN_SELLER = 0;             // 渠道
        /// <summary>
        /// 合并后数据列号: 书目号
        /// </summary>
        public static int MERGED_COLUMN_CATALOGNO = 1;          // 书目号
        /// <summary>
        /// 合并后数据列号: 摘要
        /// </summary>
        public static int MERGED_COLUMN_SUMMARY = 2;    // 摘要
        /// <summary>
        /// 合并后数据列号: 错误信息
        /// </summary>
        public static int MERGED_COLUMN_ERRORINFO = 2;  // 错误信息
        /// <summary>
        /// 合并后数据列号: ISBN/ISSN
        /// </summary>
        public static int MERGED_COLUMN_ISBNISSN = 3;           // ISBN/ISSN
        /// <summary>
        /// 合并后数据列号: 出版时间
        /// </summary>
        public static int MERGED_COLUMN_PUBLISHTIME = 4;          // 出版时间
        /// <summary>
        /// 合并后数据列号: 卷期
        /// </summary>
        public static int MERGED_COLUMN_VOLUME = 5;          // 卷期

        /// <summary>
        /// 合并后数据列号: 合并注释
        /// </summary>
        public static int MERGED_COLUMN_MERGECOMMENT = 6;      // 合并注释

        /// <summary>
        /// 合并后数据列号: 时间范围
        /// </summary>
        public static int MERGED_COLUMN_RANGE = 7;      // 时间范围
        /// <summary>
        /// 合并后数据列号: 包含期数
        /// </summary>
        public static int MERGED_COLUMN_ISSUECOUNT = 8;      // 包含期数

        /// <summary>
        /// 合并后数据列号: 复本数
        /// </summary>
        public static int MERGED_COLUMN_COPY = 9;              // 复本数
        /// <summary>
        /// 合并后数据列号: 每套册数
        /// </summary>
        public static int MERGED_COLUMN_SUBCOPY = 10;              // 每套册数
        /// <summary>
        /// 合并后数据列号: 订购单价
        /// </summary>
        public static int MERGED_COLUMN_ORDERPRICE = 11;             // 订购单价
        /// <summary>
        /// 合并后数据列号: (验收)单价
        /// </summary>
        public static int MERGED_COLUMN_PRICE = 12;             // (验收)单价
        /// <summary>
        /// 合并后数据列号: 总价格
        /// </summary>
        public static int MERGED_COLUMN_TOTALPRICE = 13;        // 总价格
        /// <summary>
        /// 合并后数据列号: 实体(册)单价
        /// </summary>
        public static int MERGED_COLUMN_ITEMPRICE = 14;             // 实体(册)单价
        /// <summary>
        /// 合并后数据列号: 订购时间
        /// </summary>
        public static int MERGED_COLUMN_ORDERTIME = 15;        // 订购时间
        /// <summary>
        /// 合并后数据列号: 订单号
        /// </summary>
        public static int MERGED_COLUMN_ORDERID = 16;          // 订单号
        /// <summary>
        /// 合并后数据列号: 馆藏分配
        /// </summary>
        public static int MERGED_COLUMN_DISTRIBUTE = 17;       // 馆藏分配
        /// <summary>
        /// 合并后数据列号: 类别
        /// </summary>
        public static int MERGED_COLUMN_CLASS = 18;             // 类别
        /// <summary>
        /// 合并后数据列号: 附注
        /// </summary>
        public static int MERGED_COLUMN_COMMENT = 19;          // 附注
        /// <summary>
        /// 合并后数据列号: 渠道地址
        /// </summary>
        public static int MERGED_COLUMN_SELLERADDRESS = 20;          // 渠道地址
        /// <summary>
        /// 合并后数据列号: 种记录路径
        /// </summary>
        public static int MERGED_COLUMN_BIBLIORECPATH = 21;    // 种记录路径

        #endregion

        const int WM_LOADSIZE = API.WM_USER + 201;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PrintAcceptForm()
        {
            InitializeComponent();
        }

        private void PrintAcceptForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
            CreateOriginColumnHeader(this.listView_origin);
            CreateMergedColumnHeader(this.listView_merged);

            this.comboBox_load_type.Text = Program.MainForm.AppInfo.GetString(
                "printaccept_form",
                "publication_type",
                "图书");

            comboBox_load_type_SelectedIndexChanged(null, null);

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);

            this.Channel = null;
        }

        private void PrintAcceptForm_FormClosing(object sender, FormClosingEventArgs e)
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
                    "PrintAcceptForm",
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

        private void PrintAcceptForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetString(
                    "printaccept_form",
                    "publication_type",
                    this.comboBox_load_type.Text);
            }

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

            string strWidths = Program.MainForm.AppInfo.GetString(
                "printaccept_form",
                "list_origin_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_origin,
                    strWidths,
                    true);
            }

            strWidths = Program.MainForm.AppInfo.GetString(
    "printaccept_form",
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
            if (this.WindowState != Program.MainForm.MdiWindowState)
                this.WindowState = Program.MainForm.MdiWindowState;
             * */

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_origin);
            Program.MainForm.AppInfo.SetString(
                "printaccept_form",
                "list_origin_width",
                strWidths);

            strWidths = ListViewUtil.GetColumnWidthListString(this.listView_merged);
            Program.MainForm.AppInfo.SetString(
                "printaccept_form",
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

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            // load page
            this.button_load_loadFromBatchNo.Enabled = bEnable;
            this.button_load_loadFromRecPathFile.Enabled = bEnable;
            this.button_load_loadFromOrderBatchNo.Enabled = bEnable;
            this.comboBox_load_type.Enabled = bEnable;

            // next button
            if (bEnable == true)
                SetNextButtonEnable();
            else
                this.button_next.Enabled = false;

            // print page
            this.button_print_Option.Enabled = bEnable;
            this.button_print_originOption.Enabled = bEnable;
            this.button_print_printAcceptList.Enabled = bEnable;
            this.button_print_printOriginList.Enabled = bEnable;
            this.button_print_exchangeRateStatis.Enabled = bEnable;
            this.button_print_exchangeRateOption.Enabled = bEnable;
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

                bool bOK = ReportSaveChangeState(out strError);

                if (bOK == false)
                {
                    this.button_next.Enabled = false;
                    this.button_saveChange_saveChange.Enabled = true;
                }
                else
                {
                    this.button_next.Enabled = true;
                    this.button_saveChange_saveChange.Enabled = false;
                }

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

        // 汇报保存进行情况。
        // return:
        //      true    保存已经完成
        //      false   保存尚未完成
        bool ReportSaveChangeState(out string strError)
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

            if (nWhiteCount == this.listView_origin.Items.Count)
            {
                strError = "没有发生过修改";
                return true;
            }

            strError = "原始数据列表中有发生了修改后尚未保存的事项，或有错误事项。\r\n\r\n列表中有:\r\n发生过修改的事项(淡黄色背景) " + nYellowCount.ToString() + " 个\r\n错误事项(红色背景) " + nRedCount.ToString() + "个\r\n\r\n(只有全部事项都为普通状态(白色背景)，才表明保存操作已经完成)";
            return false;
        }

        // 从(已验收的)册记录路径文件装载
        // parameters:
        //      bAutoSetSeriesType  是否根据文件第一行中的路径中的数据库名来自动设置出版物类型 Combobox_type
        // return:
        //      -1  出错
        //      0   放弃
        //      1   装载成功
        /// <summary>
        /// 从(已验收的)册记录路径文件装载
        /// </summary>
        /// <param name="bAutoSetSeriesType">是否根据文件第一行中的路径中的数据库名来自动设置出版物类型 Combobox_type</param>
        /// <param name="strFilename">文件全路径</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:  出错</para>
        /// <para>0:   放弃</para>
        /// <para>1:   装载成功</para>
        /// </returns>
        public int LoadFromItemRecPathFile(
            bool bAutoSetSeriesType,
            string strFilename,
            out string strError)
        {
            strError = "";

            this.SourceStyle = "recpathfile";

            int nDupCount = 0;
            int nRet = 0;

            LibraryChannel channel = this.GetChannel();

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
                Program.MainForm.Update();

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
                                "PrintAcceptForm",
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
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断1";
                            return -1;
                        }

                        string strLine = "";
                        strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (string.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strLine[0] == '#')
                            continue;   // 注释行

                        string strItemDbName = Global.GetDbName(strLine);
                        // 检查数据库类型(图书/期刊)
                        // 观察实体库是不是期刊库类型
                        // return:
                        //      -1  不是实体库
                        //      0   图书类型
                        //      1   期刊类型
                        nRet = Program.MainForm.IsSeriesTypeFromItemDbName(strItemDbName);
                        if (nRet == -1)
                        {
                            strError = "记录路径 '" + strLine + "' 中的数据库名 '" + strItemDbName + "' 不是实体库名";
                            return -1;
                        }

                        // 自动设置 图书/期刊 类型
                        if (bAutoSetSeriesType == true && nLineCount == 0)
                        {
                            if (nRet == 0)
                                this.comboBox_load_type.Text = "图书";
                            else
                                this.comboBox_load_type.Text = "连续出版物";
                        }

                        if (this.comboBox_load_type.Text == "图书")
                        {
                            if (nRet != 0)
                            {
                                strError = "记录路径 '" + strLine + "' 中的数据库名 '" + strItemDbName + "' 不是一个图书类型的实体库名";
                                return -1;
                            }
                        }
                        else
                        {
                            if (nRet != 1)
                            {
                                strError = "记录路径 '" + strLine + "' 中的数据库名 '" + strItemDbName + "' 不是一个连续出版物类型的实体库名";
                                return -1;
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

                    for (int i = 0; ; )
                    {
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断2";
                            return -1;
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

                        stop.SetMessage("正在装入路径 " + strLine + " 对应的记录...");

                        // 根据记录路径，装入订购记录
                        // return: 
                        //      -2  路径已经在list中存在了
                        //      -1  出错
                        //      1   成功
                        nRet = LoadOneItem(
                            channel,
                            this.comboBox_load_type.Text,
                            strLine,
                            this.listView_origin,
                            out strError);
                        if (nRet == -2)
                            nDupCount++;

                        i++;
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
                strError = "PrintAcceptForm LoadFromItemRecPathFile() exception: " + ex.Message;
                return -1;
            }
            finally
            {
                sr.Close();
                sr = null;

                this.ReturnChannel(channel);
                channel = null;
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
                return -1;

            // 汇报数据装载情况。
            // return:
            //      0   尚未装载任何数据    
            //      1   装载已经完成
            //      2   虽然装载了数据，但是其中有错误事项
            int nState = ReportLoadState(out strError);
            if (nState != 1)
                return -1;

            return 1;
        }

        // TODO: 需要检查记录路径是否来自实体库？
        private void button_load_loadFromRecPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.BatchNo = "";  // 表示不是根据批次号获得的内容

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的记录路径文件名";
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            // 从(已验收的)册记录路径文件装载
            // return:
            //      -1  出错
            //      0   放弃
            //      1   装载成功
            int nRet = LoadFromItemRecPathFile(
                true,
                dlg.FileName,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            this.Text = "打印验收单";
            MessageBox.Show(this, strError);
        }

        // 根据记录路径，装入册记录
        // return: 
        //      -2  路径已经在list中存在了
        //      -1  出错
        //      1   成功
        int LoadOneItem(
            LibraryChannel channel,
            string strPubType,
            string strRecPath,
            ListView list,
            out string strError)
        {
            strError = "";

            string strItemXml = "";
            string strBiblioText = "";

            string strOutputOrderRecPath = "";
            string strOutputBiblioRecPath = "";

            byte[] item_timestamp = null;

            long lRet = channel.GetItemInfo(
                stop,
                (strRecPath[0] != '@' ? "@path:" + strRecPath : strRecPath),
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

                OriginAcceptItemData data = new OriginAcceptItemData();
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

            if (strRecPath[0] == '@')
                strRecPath = strOutputOrderRecPath;

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
                    if (ListViewUtil.GetItemText(curitem, ORIGIN_COLUMN_BIBLIORECPATH) == strOutputBiblioRecPath)
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

                lRet = channel.GetBiblioInfos(
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

            // 剖析一个册xml记录，取出有关信息放入listview中

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "PrintAcceptForm dom.LoadXml() {F238B934-C67E-482A-84AC-E2810AF4875E} exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            {
                ListViewItem item = AddToListView(list,
                    dom,
                    strOutputOrderRecPath,
                    strBiblioSummary,
                    strISBnISSN,
                    strOutputBiblioRecPath);

                // 设置timestamp/xml
                OriginAcceptItemData data = (OriginAcceptItemData)item.Tag;
                Debug.Assert(data != null, "");
                data.Timestamp = item_timestamp;
                data.Xml = strItemXml;

                // 将新加入的事项滚入视野
                list.EnsureVisible(list.Items.Count - 1);

                // 填充需要从订购库获得的栏目信息
                FillOrderColumns(channel, item, strPubType);
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 填充需要从订购库获得的栏目信息
        void FillOrderColumns(
            LibraryChannel channel,
            ListViewItem item,
            string strPubType)
        {
            string strRefID = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_REFID);
            if (String.IsNullOrEmpty(strRefID) == true)
                return;

            bool bSeries = false;
            if (strPubType == "连续出版物")
                bSeries = true;

            string strError = "";
            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            string strOrderOrIssueRecPath = "";

            // 图书
            if (bSeries == false)
            {
                // 获得所连接的一条订购记录(的路径)
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetLinkedOrderRecordRecPath(
                    channel,
                    strRefID,
                    out strOrderOrIssueRecPath,
                    out strError);
                if (nRet != 1)
                {
                    goto ERROR1;
                }

                string strItemXml = "";
                // 根据记录路径获得一条订购记录
                nRet = GetOrderRecord(
                    channel,
                    strOrderOrIssueRecPath,
                    out strItemXml,
                    out strError);
                if (nRet != 1)
                    goto ERROR1;

                try
                {
                    dom.LoadXml(strItemXml);
                }
                catch (Exception ex)
                {
                    strError = "订购记录XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }
            }
            else
            {
                // 期刊

                string strPublishTime = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_PUBLISHTIME);

                if (String.IsNullOrEmpty(strPublishTime) == true)
                {
                    strError = "出版日期为空，无法定位期记录";
                    goto ERROR1;
                }

                // 获得所连接的一条期记录(的路径)
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetLinkedIssueRecordRecPath(
                    channel,
                    strRefID,
                    strPublishTime,
                    out strOrderOrIssueRecPath,
                    out strError);
                if (nRet != 1)
                {
                    goto ERROR1;
                }

                string strItemXml = "";
                // 根据记录路径获得一条期记录
                nRet = GetIssueRecord(
                    channel,
                    strOrderOrIssueRecPath,
                    out strItemXml,
                    out strError);
                if (nRet != 1)
                {
                    goto ERROR1;
                }

                try
                {
                    dom.LoadXml(strItemXml);
                }
                catch (Exception ex)
                {
                    strError = "期记录XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                // 要通过refid定位到具体的一个订购xml片段

                string strOrderXml = "";
                // 从期记录中获得和一个refid有关的订购记录片段
                // parameters:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetSubOrderRecord(dom,
                    strRefID,
                    out strOrderXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "从期记录中获得包含 '" + strRefID + "' 的订购记录片段时出错: " + strError;
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    strError = "从期记录中没有找到包含 '" + strRefID + "' 的订购记录片段";
                    goto ERROR1;
                }

                try
                {
                    dom.LoadXml(strOrderXml);
                }
                catch (Exception ex)
                {
                    strError = "(从期记录中)获得的订购片断XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }
            }

            string strCatalogNo = DomUtil.GetElementText(dom.DocumentElement,
                "catalogNo");
            string strOrderID = DomUtil.GetElementText(dom.DocumentElement,
                "orderID");
            string strClass = DomUtil.GetElementText(dom.DocumentElement,
                "class");
            string strOrderTime = DomUtil.GetElementText(dom.DocumentElement,
                "orderTime");
            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            string strSellerAddress = DomUtil.GetElementInnerXml(dom.DocumentElement,
                "sellerAddress");

            string strOrderPrice = "";  // 订购记录中的订购价
            string strAcceptPrice = "";    // 订购记录中的到书价

            // 检查total price是否正确
            {
                string strCurrentOldPrice = "";
                string strCurrentNewPrice = "";

                // 分离 "old[new]" 内的两个值
                OrderDesignControl.ParseOldNewValue(strPrice,
                    out strCurrentOldPrice,
                    out strCurrentNewPrice);

                strOrderPrice = strCurrentOldPrice;  // 订购记录中的订购价
                strAcceptPrice = strCurrentNewPrice;    // 订购记录中的到书价
            }

            // 2010/4/27
            // 如果期刊缺乏到书价
            if (bSeries == true && string.IsNullOrEmpty(strAcceptPrice) == true)
            {
                // 先取册价格
                strAcceptPrice = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ITEMPRICE);
                // 然后取订购价
                if (String.IsNullOrEmpty(strAcceptPrice) == true)
                    strAcceptPrice = strOrderPrice;
            }

            try
            {
                strOrderTime = DateTimeUtil.LocalTime(strOrderTime);
            }
            catch (Exception ex)
            {
                strOrderTime = "时间字符串 '" + strOrderTime + "' 格式错误: " + ex.Message;
            }

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_CATALOGNO, strCatalogNo);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ORDERID, strOrderID);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ORDERCLASS, strClass);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ORDERTIME, strOrderTime);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ORDERPRICE, strOrderPrice);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ACCEPTPRICE, strAcceptPrice);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_SELLERADDRESS, strSellerAddress);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ORDERRECPATH, strOrderOrIssueRecPath);

            string strDistribute = DomUtil.GetElementText(dom.DocumentElement,
                "distribute");
            if (String.IsNullOrEmpty(strDistribute) == false)
            {
                // 增补refid -- 记录路径对照关系
                List<string> refids = GetLocationRefIDs(strDistribute);

                for (int i = 0; i < refids.Count; i++)
                {
                    string strCurrentRefID = refids[i];
                    this.refid_table[strCurrentRefID] = strOrderOrIssueRecPath;
                }
            }

            string strCopyDetail = "";
            // return:
            //      -1  出错
            //      0   一般册，不是套内的册。strResult中为空
            //      1   套内册。strResult中返回了值
            nRet = GetCopyDetail(strDistribute,
                strRefID,
                out strCopyDetail,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            {
                string strCopy = DomUtil.GetElementText(dom.DocumentElement,
    "copy");
                string strOldCopy = "";
                string strNewCopy = "";

                // 分离 "old[new]" 内的两个值
                OrderDesignControl.ParseOldNewValue(strCopy,
                    out strOldCopy,
                    out strNewCopy);

                // 订购时每套包含册数 2012/9/4
                string strOrderRightCopy = OrderDesignControl.GetRightFromCopyString(strOldCopy);
                if (string.IsNullOrEmpty(strOrderRightCopy) == true)
                    strOrderRightCopy = "1";

                ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ORDERSUBCOPY, strOrderRightCopy);

                if (String.IsNullOrEmpty(strCopyDetail) == false)
                {
                    // 一套包含的册数
                    string strRightCopy = OrderDesignControl.GetRightFromCopyString(strNewCopy);
                    if (String.IsNullOrEmpty(strRightCopy) == false)
                        strCopyDetail += "/" + strRightCopy;
                }
            }

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ACCEPTSUBCOPY, strCopyDetail);
            return;
        ERROR1:
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ERRORINFO, strError);
            SetItemColor(item, TYPE_ERROR);
        }

        // 剖析套内详情的各个部分
        // 0:1/7
        static int ParseSubCopy(string strText,
            out string strNo,
            out string strIndex,
            out string strCopy,
            out string strError)
        {
            strNo = "";
            strIndex = "";
            strCopy = "";
            strError = "";

            int nRet = strText.IndexOf(":");
            if (nRet == -1)
            {
                strError = "没有冒号";
                return -1;
            }

            strNo = strText.Substring(0, nRet).Trim();
            strText = strText.Substring(nRet + 1).Trim();

            nRet = strText.IndexOf("/");
            if (nRet == -1)
            {
                strError = "没有/号";
                return -1;
            }
            strIndex = strText.Substring(0, nRet).Trim();
            strCopy = strText.Substring(nRet + 1).Trim();
            return 0;
        }

        // 从期记录中获得和一个refid有关的订购记录片段
        // parameters:
        //      -1  error
        //      0   not found
        //      1   found
        static int GetSubOrderRecord(XmlDocument dom,
            string strRefID,
            out string strOrderXml,
            out string strError)
        {
            strError = "";
            strOrderXml = "";

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("orderInfo/*/distribute");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strDistribute = node.InnerText.Trim();
                if (String.IsNullOrEmpty(strDistribute) == true)
                    continue;

                LocationCollection locations = new LocationCollection();
                int nRet = locations.Build(strDistribute,
                    out strError);
                if (nRet == -1)
                    return -1;

                for (int j = 0; j < locations.Count; j++)
                {
                    DigitalPlatform.Text.Location location = locations[j];

                    if (string.IsNullOrEmpty(location.RefID) == true)
                        continue;

                    // 2012/9/4
                    string[] parts = location.RefID.Split(new char[] { '|' });
                    foreach (string text in parts)
                    {
                        string strCurrentRefID = text.Trim();
                        if (string.IsNullOrEmpty(strCurrentRefID) == true)
                            continue;

                        if (strCurrentRefID == strRefID)
                        {
                            strOrderXml = node.ParentNode.OuterXml;
                            return 1;
                        }
                    }
                }
            }

            return 0;
        }

        // 2009/2/2
        // 获得所连接的一条期记录(的路径)
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetLinkedIssueRecordRecPath(
            LibraryChannel channel,
            string strRefID,
            string strPublishTime,
            out string strIssueRecPath,
            out string strError)
        {
            strError = "";
            strIssueRecPath = "";

            // 先从cache中找
            strIssueRecPath = (string)this.refid_table[strRefID];
            if (String.IsNullOrEmpty(strIssueRecPath) == false)
                return 1;

            long lRet = channel.SearchIssue(
                stop,
                "<all>",
                strRefID,
                -1,
                "册参考ID",
                "exact",
                this.Lang,
                "refid",   // strResultSetName
                "",    // strSearchStyle
                "", // strOutputStyle
                out strError);
            if (lRet == -1)
                return -1;

            if (lRet == 0)
            {
                strError = "参考ID '" + strRefID + "' 没有命中任何期记录";
                return 0;
            }

            if (lRet > 1)
            {
                strError = "参考ID '" + strRefID + "' 命中多条(" + lRet.ToString() + ")订购记录";
                return -1;
            }

            long lHitCount = lRet;

            Record[] searchresults = null;

            // 装入浏览格式

            lRet = channel.GetSearchResult(
                stop,
                "refid",   // strResultSetName
                0,
                lHitCount,
                "id",   // "id,cols",
                this.Lang,
                out searchresults,
                out strError);
            if (lRet == -1)
            {
                strError = "GetSearchResult() error: " + strError;
                return -1;
            }

            if (lRet == 0)
            {
                strError = "GetSearchResult() error : 未命中";
                return -1;
            }

            strIssueRecPath = searchresults[0].Path;

            // 加入cache
            if (this.refid_table.Count > 1000)
                this.refid_table.Clear();   // 限制hashtable的最大尺寸
            this.refid_table[strRefID] = strIssueRecPath;

            return (int)lHitCount;
        }

        // 2009/2/2
        // 根据记录路径获得一条期刊记录
        int GetIssueRecord(
            LibraryChannel channel,
            string strRecPath,
            out string strItemXml,
            out string strError)
        {
            strError = "";
            strItemXml = "";

            // 先从cache中找
            strItemXml = (string)this.orderxml_table[strRecPath];
            if (String.IsNullOrEmpty(strItemXml) == false)
                return 1;


            string strBiblioText = "";

            string strItemRecPath = "";
            string strBiblioRecPath = "";

            byte[] item_timestamp = null;

            string strIndex = "@path:" + strRecPath;

            long lRet = channel.GetIssueInfo(
                stop,
                strIndex,   // strPublishTime
                // "", // strBiblioRecPath
                "xml",
                out strItemXml,
                out strItemRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strBiblioRecPath,
                out strError);
            if (lRet == -1 || lRet == 0)
                return (int)lRet;

            // 加入cache
            if (this.orderxml_table.Count > 500)
                this.orderxml_table.Clear();   // 限制hashtable的最大尺寸
            this.orderxml_table[strRecPath] = strItemXml;

            return 1;
        }

        // 获得所连接的一条订购记录(的路径)
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetLinkedOrderRecordRecPath(
            LibraryChannel channel,
            string strRefID,
            out string strOrderRecPath,
            out string strError)
        {
            strError = "";
            strOrderRecPath = "";

            // 先从cache中找
            strOrderRecPath = (string)this.refid_table[strRefID];
            if (String.IsNullOrEmpty(strOrderRecPath) == false)
                return 1;

            long lRet = channel.SearchOrder(
                stop,
                "<all>",
                strRefID,
                -1,
                "册参考ID",
                "exact",
                this.Lang,
                "refid",   // strResultSetName
                "",    // strSearchStyle
                "", // strOutputStyle
                out strError);
            if (lRet == -1)
                return -1;

            if (lRet == 0)
            {
                strError = "参考ID '" + strRefID + "' 没有命中任何订购记录";
                return 0;
            }

            if (lRet > 1)
            {
                strError = "参考ID '" + strRefID + "' 命中多条(" + lRet.ToString() + ")订购记录";
                return -1;
            }

            long lHitCount = lRet;

            Record[] searchresults = null;

            // 装入浏览格式

            lRet = channel.GetSearchResult(
                stop,
                "refid",   // strResultSetName
                0,
                lHitCount,
                "id",   // "id,cols",
                this.Lang,
                out searchresults,
                out strError);
            if (lRet == -1)
            {
                strError = "GetSearchResult() error: " + strError;
                return -1;
            }

            if (lRet == 0)
            {
                strError = "GetSearchResult() error : 未命中";
                return -1;
            }

            strOrderRecPath = searchresults[0].Path;

            // 加入cache
            if (this.refid_table.Count > 1000)
                this.refid_table.Clear();   // 限制hashtable的最大尺寸
            this.refid_table[strRefID] = strOrderRecPath;

            return (int)lHitCount;
        }

        // 根据记录路径获得一条订购记录
        int GetOrderRecord(
            LibraryChannel channel,
            string strRecPath,
            out string strItemXml,
            out string strError)
        {
            strError = "";
            strItemXml = "";

            // 先从cache中找
            strItemXml = (string)this.orderxml_table[strRecPath];
            if (String.IsNullOrEmpty(strItemXml) == false)
                return 1;

            string strBiblioText = "";

            string strItemRecPath = "";
            string strBiblioRecPath = "";

            byte[] item_timestamp = null;

            string strIndex = "@path:" + strRecPath;

            long lRet = channel.GetOrderInfo(
                stop,
                strIndex,
                // "",
                "xml",
                out strItemXml,
                out strItemRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strBiblioRecPath,
                out strError);
            if (lRet == -1 || lRet == 0)
                return (int)lRet;

            // 加入cache
            if (this.orderxml_table.Count > 500)
                this.orderxml_table.Clear();   // 限制hashtable的最大尺寸
            this.orderxml_table[strRecPath] = strItemXml;

            return 1;
        }

        // TODO: 此函数和 LocationCollection 中的功能重复了，考虑用后者替代
        // 将采购馆藏字符串中的refid解析出来
        /// <summary>
        /// 将采购馆藏字符串中的参考 ID 解析出来
        /// </summary>
        /// <param name="strText">馆藏字符串</param>
        /// <returns>表示参考 ID 的字符串数组</returns>
        public static List<string> GetLocationRefIDs(string strText)
        {
            List<string> results = new List<string>();

            if (String.IsNullOrEmpty(strText) == true)
                return results;

            int nStart = 0;
            int nEnd = 0;
            int nPos = 0;
            for (; ; )
            {
                nStart = strText.IndexOf("{", nPos);
                if (nStart == -1)
                    break;
                nPos = nStart + 1;
                nEnd = strText.IndexOf("}", nPos);
                if (nEnd == -1)
                    break;
                nPos = nEnd + 1;
                if (nEnd <= nStart + 1)
                    continue;
                string strPart = strText.Substring(nStart + 1, nEnd - nStart - 1).Trim();

                if (String.IsNullOrEmpty(strPart) == true)
                    continue;

                string[] ids = strPart.Split(new char[] { ',', '|' });
                for (int j = 0; j < ids.Length; j++)
                {
                    string strID = ids[j].Trim();
                    if (String.IsNullOrEmpty(strID) == true)
                        continue;

                    results.Add(strID);
                }
            }

            return results;
        }

        // 获得一个册的套内详情字符串
        // parameters:
        //      strDistribute     馆藏分配字符串
        //      strRefID    要关注的册的refid
        //      strResult   返回详情字符串。格式为“1:2” 表示第一套内的第二册 序号从1开始计数
        // return:
        //      -1  出错
        //      0   一般册，不是套内的册。strResult中为空
        //      1   套内册。strResult中返回了值
        /*public*/
        static int GetCopyDetail(string strDistribute,
            string strRefID,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            if (String.IsNullOrEmpty(strRefID) == true)
            {
                strError = "strRefID参数不应为空";
                return -1;
            }

            if (String.IsNullOrEmpty(strDistribute) == true)
            {
                strError = "strText参数不应为空";
                return -1;
            }

            List<string> results = new List<string>();

            int nStart = 0;
            int nEnd = 0;
            int nPos = 0;
            for (; ; )
            {
                nStart = strDistribute.IndexOf("{", nPos);
                if (nStart == -1)
                    break;
                nPos = nStart + 1;
                nEnd = strDistribute.IndexOf("}", nPos);
                if (nEnd == -1)
                    break;
                nPos = nEnd + 1;
                if (nEnd <= nStart + 1)
                    continue;
                string strPart = strDistribute.Substring(nStart + 1, nEnd - nStart - 1).Trim();

                if (String.IsNullOrEmpty(strPart) == true)
                    continue;

                string[] ids = strPart.Split(new char[] { ',' });
                for (int j = 0; j < ids.Length; j++)
                {
                    string strID = ids[j].Trim();
                    if (String.IsNullOrEmpty(strID) == true)
                        continue;

                    results.Add(strID);
                }
            }

            // 寻找位置
            int iTao = -1;
            for (int i = 0; i < results.Count; i++)
            {
                string strSegment = results[i];

                bool bTao = false;
                int nRet = strSegment.IndexOf("|");
                if (nRet == -1)
                    bTao = false;
                else
                {
                    bTao = true;
                    iTao++;
                }

                if (bTao == false)
                {
                    if (strRefID == strSegment)
                        return 0;
                    continue;
                }

                string[] ids = strSegment.Split(new char[] { '|' });
                for (int j = 0; j < ids.Length; j++)
                {
                    string strID = ids[j].Trim();
                    if (String.IsNullOrEmpty(strID) == true)
                        continue;

                    if (strID == strRefID)
                    {
                        strResult = (iTao + 1).ToString() + ":" + (j + 1).ToString();
                        return 1;
                    }
                }
            }

            strError = "refid '" + strRefID + "' 在字符串 '" + strDistribute + "' 中没有找到";
            return -1;    // not found
        }

        static System.Drawing.Color GetItemForeColor(int nType)
        {
            if (nType == TYPE_ERROR)
            {
                return System.Drawing.Color.White;
            }
            else if (nType == TYPE_CHANGED)
            {
                return SystemColors.WindowText;
            }
            else if (nType == TYPE_NORMAL)
            {
                return SystemColors.WindowText;
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
                return SystemColors.Window;
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
                if (item.Tag is OriginAcceptItemData
                    && nType != TYPE_ERROR)
                {
                    OriginAcceptItemData data = (OriginAcceptItemData)item.Tag;

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

        /*public*/
        static ListViewItem AddToListView(ListView list,
            XmlDocument dom,
            string strRecPath,
            string strBiblioSummary,
            string strISBnISSN,
            string strBiblioRecPath)
        {
            ListViewItem item = new ListViewItem(strRecPath, TYPE_NORMAL);

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

        // 根据订购记录DOM设置ListViewItem除第一列以外的文字
        // parameters:
        //      bSetBarcodeColumn   是否要设置第一列记录路径的内容
        /*public*/
        static void SetListViewItemText(XmlDocument dom,
            bool bSetRecPathColumn,
            string strRecPath,
            string strBiblioSummary,
            string strISBnISSN,
            string strBiblioRecPath,
            ListViewItem item)
        {
            OriginAcceptItemData data = null;
            data = (OriginAcceptItemData)item.Tag;
            if (data == null)
            {
                data = new OriginAcceptItemData();
                item.Tag = data;
            }
            else
            {
                data.Changed = false;   // 2008/9/5
            }

            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");
            string strPublishTime = DomUtil.GetElementText(dom.DocumentElement,
                "publishTime");
            string strVolume = DomUtil.GetElementText(dom.DocumentElement,
                "volume");

            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");
            string strSeller = DomUtil.GetElementText(dom.DocumentElement,
                "seller");
            string strSource = DomUtil.GetElementText(dom.DocumentElement,
                "source");
            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");

            List<int> textchanged_columns = new List<int>();

            /*
            // 检查和修改 状态
            {
                if (strState == "已验收")
                {
                    strBiblioSummary = "本记录状态为 '已验收'，不能再参与订单打印";
                    SetItemColor(item,
                            TYPE_ERROR);
                }
                else
                {
                    string strNewState = "已订购";

                    if (strState != strNewState)
                    {
                        strState = strNewState;
                        data.Changed = true;
                        SetItemColor(item,
                            TYPE_CHANGED); // 表示状态被改变了
                        textchanged_columns.Add(ORIGIN_COLUMN_STATE);
                    }
                }
            }*/

            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");
            string strBatchNo = DomUtil.GetElementText(dom.DocumentElement,
                "batchNo");
            string strRefID = DomUtil.GetElementText(dom.DocumentElement,
                "refID");

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_SUMMARY, strBiblioSummary);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ISBNISSN, strISBnISSN);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_STATE, strState);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_PUBLISHTIME, strPublishTime);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_VOLUME, strVolume);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_LOCATION, strLocation);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_SELLER, strSeller);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_SOURCE, strSource);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_ITEMPRICE, strPrice);

            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_COMMENT, strComment);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_BATCHNO, strBatchNo);
            ListViewUtil.ChangeItemText(item, ORIGIN_COLUMN_REFID, strRefID);

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
            ColumnHeader columnHeader_publishTime = new ColumnHeader();
            ColumnHeader columnHeader_volume = new ColumnHeader();
            ColumnHeader columnHeader_seller = new ColumnHeader();
            ColumnHeader columnHeader_source = new ColumnHeader();
            ColumnHeader columnHeader_price = new ColumnHeader();

            ColumnHeader columnHeader_location = new ColumnHeader();

            ColumnHeader columnHeader_comment = new ColumnHeader();
            ColumnHeader columnHeader_batchNo = new ColumnHeader();
            ColumnHeader columnHeader_refID = new ColumnHeader();
            ColumnHeader columnHeader_biblioRecpath = new ColumnHeader();

            ColumnHeader columnHeader_catalogNo = new ColumnHeader();
            ColumnHeader columnHeader_orderID = new ColumnHeader();
            ColumnHeader columnHeader_orderClass = new ColumnHeader();
            ColumnHeader columnHeader_orderTime = new ColumnHeader();
            ColumnHeader columnHeader_orderPrice = new ColumnHeader();
            ColumnHeader columnHeader_acceptPrice = new ColumnHeader();
            ColumnHeader columnHeader_sellerAddress = new ColumnHeader();
            ColumnHeader columnHeader_orderRecpath = new ColumnHeader();
            ColumnHeader columnHeader_acceptSubCopy = new ColumnHeader();
            ColumnHeader columnHeader_orderSubCopy = new ColumnHeader();


            list.Columns.Clear();

            list.Columns.AddRange(new ColumnHeader[] {
            columnHeader_recpath,
            columnHeader_errorInfo,
            columnHeader_isbnIssn,
            columnHeader_state,
            columnHeader_publishTime,
            columnHeader_volume,
            columnHeader_location,
            columnHeader_seller,
            columnHeader_source,
            columnHeader_price,
            columnHeader_comment,
            columnHeader_batchNo,
            columnHeader_refID,
            columnHeader_biblioRecpath,
            columnHeader_catalogNo,
            columnHeader_orderID,
            columnHeader_orderClass,
            columnHeader_orderTime,
            columnHeader_orderPrice,
            columnHeader_acceptPrice,
            columnHeader_sellerAddress,
            columnHeader_orderRecpath,
            columnHeader_acceptSubCopy,
            columnHeader_orderSubCopy});


            // 
            // columnHeader_recpath
            // 
            columnHeader_recpath.Text = "册记录路径";
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
            // columnHeader_publishTime
            // 
            columnHeader_publishTime.Text = "出版时间";
            columnHeader_publishTime.Width = 100;
            // 
            // columnHeader_volume
            // 
            columnHeader_volume.Text = "卷期";
            columnHeader_volume.Width = 100;
            // 
            // columnHeader_location
            // 
            columnHeader_location.Text = "馆藏地点";
            columnHeader_location.Width = 150;
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
            // columnHeader_price
            // 
            columnHeader_price.Text = "册价格";
            columnHeader_price.Width = 150;
            columnHeader_price.TextAlign = HorizontalAlignment.Right;
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
            // columnHeader_refID
            // 
            columnHeader_refID.Text = "参考ID";
            columnHeader_refID.Width = 100;
            // 
            // columnHeader_biblioRecpath
            // 
            columnHeader_biblioRecpath.Text = "种记录路径";
            columnHeader_biblioRecpath.Width = 200;

            // 
            // columnHeader_catalogNo
            // 
            columnHeader_catalogNo.Text = "书目号";
            columnHeader_catalogNo.Width = 150;
            // 
            // columnHeader_orderID
            // 
            columnHeader_orderID.Text = "订单号";
            columnHeader_orderID.Width = 150;
            // 
            // columnHeader_orderClass
            // 
            columnHeader_orderClass.Text = "订购类目";
            columnHeader_orderClass.Width = 150;
            // 
            // columnHeader_orderTime
            // 
            columnHeader_orderTime.Text = "订购时间";
            columnHeader_orderTime.Width = 150;
            // 
            // columnHeader_orderPrice
            // 
            columnHeader_orderPrice.Text = "订购价";
            columnHeader_orderPrice.Width = 150;
            columnHeader_orderPrice.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_acceptPrice
            // 
            columnHeader_acceptPrice.Text = "到书价";
            columnHeader_acceptPrice.Width = 150;
            columnHeader_acceptPrice.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_sellerAddress
            // 
            columnHeader_sellerAddress.Text = "渠道地址";
            columnHeader_sellerAddress.Width = 200;
            // 
            // columnHeader_orderRecpath
            // 
            columnHeader_orderRecpath.Text = "订购记录路径";
            columnHeader_orderRecpath.Width = 200;
            // 
            // columnHeader_acceptSubCopy
            // 
            columnHeader_acceptSubCopy.Text = "套内详情";
            columnHeader_acceptSubCopy.Width = 200;
            // 
            // columnHeader_orderSubCopy
            // 
            columnHeader_orderSubCopy.Text = "订购时每套内册数";
            columnHeader_orderSubCopy.Width = 200;

        }

        // 设置 合并后数据listview 的栏目标题
        void CreateMergedColumnHeader(ListView list)
        {
            ColumnHeader columnHeader_seller = new ColumnHeader();
            ColumnHeader columnHeader_catalogNo = new ColumnHeader();
            ColumnHeader columnHeader_errorInfo = new ColumnHeader();
            ColumnHeader columnHeader_isbnIssn = new ColumnHeader();
            ColumnHeader columnHeader_publishTime = new ColumnHeader();
            ColumnHeader columnHeader_volume = new ColumnHeader();
            ColumnHeader columnHeader_mergeComment = new ColumnHeader();
            ColumnHeader columnHeader_range = new ColumnHeader();
            ColumnHeader columnHeader_issueCount = new ColumnHeader();
            ColumnHeader columnHeader_copy = new ColumnHeader();
            ColumnHeader columnHeader_subcopy = new ColumnHeader();
            ColumnHeader columnHeader_orderPrice = new ColumnHeader();
            ColumnHeader columnHeader_price = new ColumnHeader();
            ColumnHeader columnHeader_totalPrice = new ColumnHeader();
            ColumnHeader columnHeader_itemPrice = new ColumnHeader();
            ColumnHeader columnHeader_orderTime = new ColumnHeader();
            ColumnHeader columnHeader_orderID = new ColumnHeader();
            ColumnHeader columnHeader_distribute = new ColumnHeader();
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
            columnHeader_publishTime,
            columnHeader_volume,
            columnHeader_mergeComment,
            columnHeader_range,
            columnHeader_issueCount,
            columnHeader_copy,
            columnHeader_subcopy,
            columnHeader_orderPrice,
            columnHeader_price,
            columnHeader_totalPrice,
            columnHeader_itemPrice,
            columnHeader_orderTime,
            columnHeader_orderID,
            columnHeader_distribute,
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
            // columnHeader_publishTime
            // 
            columnHeader_publishTime.Text = "出版时间";
            columnHeader_publishTime.Width = 100;
            // 
            // columnHeader_volume
            // 
            columnHeader_volume.Text = "卷期";
            columnHeader_volume.Width = 100;
            // 
            // columnHeader_mergeComment
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
            // columnHeader_orderPrice
            // 
            columnHeader_orderPrice.Text = "订购单价";
            columnHeader_orderPrice.Width = 150;
            columnHeader_orderPrice.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_price
            // 
            columnHeader_price.Text = "验收单价";
            columnHeader_price.Width = 150;
            columnHeader_price.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_totalPrice
            // 
            columnHeader_totalPrice.Text = "总价";
            columnHeader_totalPrice.Width = 150;
            columnHeader_totalPrice.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader_itemPrice
            // 
            columnHeader_itemPrice.Text = "实体单价";
            columnHeader_itemPrice.Width = 150;
            columnHeader_itemPrice.TextAlign = HorizontalAlignment.Right;
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
                this.button_print_printAcceptList.Focus();
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                MessageBox.Show(this, "已经在最后一个page");
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }
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
                this.SetNextButtonEnable();
                // this.button_next.Enabled = true;

                // 强制显示出原始数据列表，以便用户正确地关联概念
                this.tabControl_items.SelectedTab = this.tabPage_originItems;
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


        /*public*/
        class NamedListViewItems : List<ListViewItem>
        {
            public string Seller = "";
            List<ListViewItem> Items = new List<ListViewItem>();
        }

        // 根据渠道(书商)名分入多个不同的List<ListViewItem>
        /*public*/
        class NamedListViewItemsCollection : List<NamedListViewItems>
        {
            public void AddItem(string strSeller,
                ListViewItem item)
            {
                NamedListViewItems list = null;
                bool bFound = false;

                // 定位
                for (int i = 0; i < this.Count; i++)
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

        private void button_print_printAcceptList_Click(object sender, EventArgs e)
        {
            int nErrorCount = 0;

            this.tabControl_items.SelectedTab = this.tabPage_mergedItems;

            NamedListViewItemsCollection lists = new NamedListViewItemsCollection();

            // 先检查是否有错误事项，顺便构建item列表
            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < this.listView_merged.Items.Count; i++)
            {
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

            string strError = "";
            List<string> filenames = new List<string>();
            try
            {
                for (int i = 0; i < lists.Count; i++)
                {
                    List<string> temp_filenames = null;
                    int nRet = PrintMergedList(lists[i],
                        out temp_filenames,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    filenames.AddRange(temp_filenames);
                }

                HtmlPrintForm printform = new HtmlPrintForm();

                printform.Text = "打印验收单";
                // printform.MainForm = Program.MainForm;
                printform.Filenames = filenames;
                Program.MainForm.AppInfo.LinkFormState(printform, "printaccept_htmlprint_formstate");
                printform.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(printform);

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

        int PrintMergedList(NamedListViewItems items,
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
                int nRet = BuildMergedHtml(
                    items,
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

        // 验收单打印选项
        private void button_print_Option_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            string strNamePath = "printaccept_printoption";

            PrintOption option = new PrintAcceptPrintOption(Program.MainForm.UserDir, // Program.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(Program.MainForm.AppInfo,
                strNamePath);


            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            // dlg.MainForm = Program.MainForm;
            dlg.DataDir = Program.MainForm.UserDir; // .DataDir;
            dlg.Text = this.comboBox_load_type.Text + " 验收单 打印参数";
            dlg.PrintOption = option;
            dlg.ColumnItems = new string[] {
                "no -- 序号",
                "seller -- 渠道",
                "catalogNo -- 书目号",
                "summary -- 摘要",
                "isbnIssn -- ISBN/ISSN",
                "publishTime -- 出版时间",
                "volume -- 卷期",
                "mergeComment -- 合并注释",
                "copy -- 复本数",
                "series -- 套数",
                "subcopy -- 每套册数",

                "orderPrice -- 订购单价",
                "acceptPrice -- 验收单价",
                "totalPrice -- 总价格",
                "itemPrice -- 实体单价",
                "orderTime -- 订购时间",
                "orderID -- 订单号",
                "distribute -- 馆藏分配",
                "orderClass -- 类别",
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


            Program.MainForm.AppInfo.LinkFormState(dlg, "printorder_printoption_formstate");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(Program.MainForm.AppInfo,
                strNamePath);
        }

        // 关于来源的描述。
        // 如果为"batchno"方式，则为批次号；如果为"barcodefile"方式，则为条码号文件名(纯文件名); 如果为"recpathfile"方式，则为记录路径文件名(纯文件名)
        /// <summary>
        /// 关于来源的描述文字。
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

                    /*
                    if (String.IsNullOrEmpty(this.LocationString) == false
                        && this.LocationString != "<不指定>")
                    {
                        if (String.IsNullOrEmpty(strText) == false)
                            strText += "; ";
                        strText += "馆藏地 " + this.LocationString;
                    }*/

                    return this.BatchNo;
                }
                /*
            else if (this.SourceStyle == "barcodefile")
            {
                return "条码号文件 " + Path.GetFileName(this.BarcodeFilePath);
            }
                 * */
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

        // 构造html页面
        // 打印验收单
        int BuildMergedHtml(
            NamedListViewItems items,
            out List<string> filenames,
            out string strError)
        {
            strError = "";

            Hashtable macro_table = new Hashtable();

            string strNamePath = "printaccept_printoption";

            // 获得打印参数
            PrintOption option = new PrintAcceptPrintOption(Program.MainForm.UserDir, // Program.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(Program.MainForm.AppInfo,
                strNamePath);

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

            // 2009/7/30 changed
            if (this.SourceStyle == "batchno")
            {
                macro_table["%batchno%"] = HttpUtility.HtmlEncode(this.BatchNo); // 批次号
            }
            else
            {
                Debug.Assert(this.SourceStyle == "barcodefile"
                    || this.SourceStyle == "recpathfile"
                    || this.SourceStyle == "",
                    "");

                macro_table["%batchno%"] = "";
            }
            macro_table["%seller%"] = items.Seller; // 渠道名
            macro_table["%pagecount%"] = nPageCount.ToString();
            macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            // 2009/7/30 changed
            if (this.SourceStyle == "recpathfile")
            {
                macro_table["%recpathfilepath%"] = this.RecPathFilePath;
                macro_table["%recpathfilename%"] = Path.GetFileName(this.RecPathFilePath);
            }
            else
            {
                Debug.Assert(this.SourceStyle == "batchno"
                    || this.SourceStyle == "barcodefile"
                    || this.SourceStyle == "",
                    "");

                macro_table["%recpathfilepath%"] = "";
                macro_table["%recpathfilename%"] = "";
            }

            // 批次号或文件名 多个宏不方便判断后选用，只好合并为一个宏
            macro_table["%batchno_or_recpathfilename%"] = String.IsNullOrEmpty(this.BatchNo) == true ? Path.GetFileName(this.RecPathFilePath) : this.BatchNo;
            macro_table["%sourcedescription%"] = this.SourceDescription;

            filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名

            // 需要将属于不同渠道的文件名前缀区别开来
            string strFileNamePrefix = Program.MainForm.DataDir + "\\~printaccept_" + items.GetHashCode().ToString() + "_";

            string strFileName = "";

            // 输出信息页
            {
                int nItemCount = items.Count;
                int nTotalCopies = GetMergedTotalCopies(items);
                int nTotalSeries = GetMergedTotalSeries(items);
                int nBiblioCount = GetMergedBiblioCount(items);
                string strTotalPrice = GetMergedTotalPrice(items);

                macro_table["%itemcount%"] = nItemCount.ToString(); // 事项数
                macro_table["%totalcopies%"] = nTotalCopies.ToString(); // 总册数(注意每套可以有多册)
                macro_table["%totalseries%"] = nTotalSeries.ToString(); // 总套数(注意每项可以有多套)
                macro_table["%bibliocount%"] = nBiblioCount.ToString(); // 种数
                macro_table["%totalprice%"] = strTotalPrice;    // 总价格 可能为多个币种的价格串联形态


                macro_table["%pageno%"] = "1";

                // 2009/7/30
                macro_table["%datadir%"] = Program.MainForm.DataDir;   // 便于引用datadir下templates目录内的某些文件
                ////macro_table["%libraryserverdir%"] = Program.MainForm.LibraryServerDir;  // 便于引用服务器端的CSS文件
                // 2009/10/10
                macro_table["%cssfilepath%"] = this.GetAutoCssUrl(option, "printaccept.css");  // 便于引用服务器端或“css”模板的CSS文件

                strFileName = strFileNamePrefix + "0" + ".html";

                filenames.Add(strFileName);

                string strTemplateFilePath = option.GetTemplatePageFilePath("统计页");
                if (String.IsNullOrEmpty(strTemplateFilePath) == false)
                {
                    /*
<html>
<head>
    老用法<LINK href='%libraryserverdir%/printaccept.css' type='text/css' rel='stylesheet'>
	新用法<LINK href='%cssfilepath%' type='text/css' rel='stylesheet'>
</head>
<body>
    <div class='pageheader'>%date% %seller% 验收单 - 来源: %sourcedescription% - (共 %pagecount% 页)</div>
    <div class='tabletitle'>%date% %seller% 验收单</div>
    <div class='seller'>书商: %seller%</div>
    <div class='copies'>册数: %totalcopies%</div>
    <div class='bibliocount'>种数: %bibliocount%</div>
    <div class='totalprice'>总价: %totalprice%</div>
    <div class='sepline'><hr/></div>
    <div class='batchno'>批次号: %batchno%</div>
    <div class='location'>记录路径文件: %recpathfilepath%</div>
    <div class='sepline'><hr/></div>
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

                    BuildMergedPageTop(option,
                        macro_table,
                        strFileName,
                        false);

                    // 内容行
                    StreamUtil.WriteText(strFileName,
                        "<div class='seller'>渠道: " + items.Seller + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='bibliocount'>种数: " + nBiblioCount.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='series'>套数: " + nTotalSeries.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='copies'>册数: " + nTotalCopies.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='totalprice'>总价: " + strTotalPrice + "</div>");

                    BuildMergedPageBottom(option,
                        macro_table,
                        strFileName,
                        false);
                }

            }


            string strMarcFilterFilePath = option.GetTemplatePageFilePath("MARC过滤器");
            if (String.IsNullOrEmpty(strMarcFilterFilePath) == false)
            {
                int nRet = PrepareMarcFilter(strMarcFilterFilePath, out strError);
                if (nRet == -1)
                    return -1;
            }

            // 表格页循环
            for (int i = 0; i < nTablePageCount; i++)
            {
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
                    BuildMergedTableLine(option,
                        items,
                        strFileName, i, j);
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
                // return Program.MainForm.LibraryServerDir + "/" + strDefaultCssFileName;    // 缺省的
                return PathUtil.MergePath(Program.MainForm.DataDir, strDefaultCssFileName);    // 缺省的
            }
        }

        int BuildMergedPageTop(PrintOption option,
            Hashtable macro_table,
            string strFileName,
            bool bOutputTable)
        {
            /*
            string strLibraryServerUrl = Program.MainForm.AppInfo.GetString(
    "config",
    "circulation_server_url",
    "");
            int pos = strLibraryServerUrl.LastIndexOf("/");
            if (pos != -1)
                strLibraryServerUrl = strLibraryServerUrl.Substring(0, pos);
             * */

            // string strCssUrl = Program.MainForm.LibraryServerDir + "/printaccept.css";
            // 2009/10/10 changed
            string strCssUrl = GetAutoCssUrl(option, "printaccept.css");

            /*
            // 2009/10/9
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                strCssUrl = strCssFilePath;
            else
                strCssUrl = Program.MainForm.LibraryServerDir + "/printaccept.css";    // 缺省的
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

                    string strClass = PrintOrderForm.GetClass(column.Name);

                    StreamUtil.WriteText(strFileName,
                        "<td class='" + strClass + "'>" + strCaption + "</td>");
                }

                StreamUtil.WriteText(strFileName,
                    "</tr>");

            }

            return 0;
        }

        int BuildMergedTableLine(PrintOption option,
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

            this.ColumnTable.Clear();   // 清除上一记录处理时残余的内容

            if (this.MarcFilter != null)
            {
                string strError = "";
                string strMARC = "";
                string strOutMarcSyntax = "";

                // TODO: 有错误要明显报出来，否则容易在打印出来后才发现，就晚了

                // 获得MARC格式书目记录
                string strBiblioRecPath = ListViewUtil.GetItemText(item, MERGED_COLUMN_BIBLIORECPATH);

                if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                {
                    nRet = GetMarc(strBiblioRecPath,
                        out strMARC,
                        out strOutMarcSyntax,
                        out strError);
                    if (nRet == -1)
                    {
                        strLineContent = strError;
                        goto END1;
                    }

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

                string strClass = PrintOrderForm.GetClass(column.Name);

                strLineContent +=
                    "<td class='" + strClass + "'>" + strContent + "</td>";
            }

        END1:
            StreamUtil.WriteText(strFileName,
    "<tr class='content'>");

            StreamUtil.WriteText(strFileName,
    strLineContent);

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
            PrintOrderForm.ParseNameParam(StringUtil.GetLeft(strColumnName),
                out strName,
                out strParameters);

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
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER);

                    case "catalogNo":
                    case "书目号":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_CATALOGNO);

                    case "errorInfo":
                    case "summary":
                    case "摘要":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_SUMMARY);

                    case "isbnIssn":
                    case "ISBN/ISSN":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_ISBNISSN);

                    case "publishTime":
                    case "出版时间":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_PUBLISHTIME);

                    case "volume":
                    case "卷期":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_VOLUME);


                    case "mergeComment":
                    case "合并注释":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_MERGECOMMENT);

                    case "copy":
                    case "复本数":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_COPY);

                    case "subcopy":
                    case "每套册数":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_SUBCOPY);

                    case "series":
                    case "套数":
                        {
                            string strCopy = ListViewUtil.GetItemText(item, MERGED_COLUMN_COPY);
                            string strSubCopy = ListViewUtil.GetItemText(item, MERGED_COLUMN_SUBCOPY);
                            if (String.IsNullOrEmpty(strSubCopy) == true)
                                return strCopy;

                            return strCopy + "(每套含 " + strSubCopy + " 册)";
                        }

                    case "orderPrice":
                    case "订购单价":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_ORDERPRICE);


                    case "price":
                    case "acceptPrice":
                    case "单价":
                    case "验收单价":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_PRICE);

                    case "totalPrice":
                    case "总价格":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_TOTALPRICE);

                    // 2013/5/31
                    case "itemPrice":
                    case "实体单价":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_ITEMPRICE);


                    case "orderTime":
                    case "订购时间":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_ORDERTIME);

                    case "orderID":
                    case "订单号":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_ORDERID);


                    // 没有recpath记录路径。因为recpath已经归入“合并注释”栏
                    // 没有state状态。因为state将被全部重设为“已订购”
                    // 没有source经费来源。因为已经归入“合并注释”栏
                    // 没有batchNo批次号，因为原始事项已经合并，多个原始事项不一定具有相同的批次号

                    case "distribute":
                    case "馆藏分配":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_DISTRIBUTE);

                    case "class":
                    case "orderClass":
                    case "类别":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_CLASS);


                    case "comment":
                    case "注释":
                    case "附注":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_COMMENT);

                    case "biblioRecpath":
                    case "种记录路径":
                        return ListViewUtil.GetItemText(item, MERGED_COLUMN_BIBLIORECPATH);

                    // 格式化以后的渠道地址
                    case "sellerAddress":
                    case "渠道地址":
                        return PrintOrderForm.GetPrintableSellerAddress(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            strParameters);

                    case "sellerAddress:zipcode":
                    case "渠道地址:邮政编码":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "zipcode");

                    case "sellerAddress:address":
                    case "渠道地址:地址":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "address");

                    case "sellerAddress:department":
                    case "渠道地址:单位":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "department");

                    case "sellerAddress:name":
                    case "渠道地址:联系人":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "name");

                    case "sellerAddress:email":
                    case "渠道地址:Email地址":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "email");

                    case "sellerAddress:bank":
                    case "渠道地址:开户行":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "bank");

                    case "sellerAddress:accounts":
                    case "渠道地址:银行账号":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "accounts");

                    case "sellerAddress:payStyle":
                    case "渠道地址:汇款方式":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "payStyle");

                    case "sellerAddress:comment":
                    case "渠道地址:附注":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, MERGED_COLUMN_SELLER),
                            "comment");
                    default:
                        {
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
        "<div class='pagefooter'>" + strPageFooterText + "</div>");
            }


            StreamUtil.WriteText(strFileName, "</body></html>");

            return 0;
        }

        static int GetMergedBiblioCount(NamedListViewItems items)
        {
            List<string> paths = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strText = "";

                try
                {
                    strText = ListViewUtil.GetItemText(item, MERGED_COLUMN_BIBLIORECPATH);
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

        // 合并后的总套数
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
                    strCopy = ListViewUtil.GetItemText(item, MERGED_COLUMN_COPY);

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

        // 合并后的总册数
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
                    strCopy = ListViewUtil.GetItemText(item, MERGED_COLUMN_COPY);

                    // TODO: 注意检查是否有[]符号?
                    nCopy = Convert.ToInt32(strCopy);
                }
                catch
                {
                    continue;
                }

                int nSubCopy = 1;
                string strSubCopy = "";
                strSubCopy = ListViewUtil.GetItemText(item, MERGED_COLUMN_SUBCOPY);
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

                total += nCopy * nSubCopy;
            }

            return total;
        }


        static string GetMergedTotalPrice(NamedListViewItems items)
        {
            List<string> prices = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strPrice = "";

                try
                {
                    strPrice = ListViewUtil.GetItemText(item, MERGED_COLUMN_TOTALPRICE);
                }
                catch
                {
                    continue;
                }

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                prices.Add(strPrice);
            }

            return PriceUtil.TotalPrice(prices);
        }

        // 根据验收批次号检索装载
        private void button_load_loadFromAcceptBatchNo_Click(object sender, EventArgs e)
        {
            LoadFromAcceptBatchNo(null);
        }

        // 根据订购批次号检索装载
        private void button_load_loadFromOrderBatchNo_Click(object sender, EventArgs e)
        {
            LoadFromOrderBatchNo(null);
        }

        // 根据验收批次号检索装载数据
        // parameters:
        //      bAutoSetSeriesType  是否根据命中的第一条记录的路径中的数据库名来自东设置Combobox_type
        //      strDefaultBatchNo   缺省的批次号。如果为null，则表示不使用这个参数。
        /// <summary>
        /// 根据验收批次号检索装载数据
        /// </summary>
        /// <param name="strDefaultBatchNo">缺省的批次号。如果为 null，则表示不使用这个参数</param>
        public void LoadFromAcceptBatchNo(
            // bool bAutoSetSeriesType,
            string strDefaultBatchNo)
        {
            SearchByBatchnoForm dlg = new SearchByBatchnoForm();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.CfgSectionName = "PrintAcceptForm_SearchByAcceptBatchnoForm";
            this.BatchNo = "";

            if (strDefaultBatchNo != null)
                dlg.BatchNo = strDefaultBatchNo;

            dlg.Text = "根据验收(册)批次号检索出已验收的册记录";
            dlg.DisplayLocationList = false;

            dlg.GetLocationValueTable -= new GetValueTableEventHandler(dlg_GetLocationValueTable);
            dlg.GetLocationValueTable += new GetValueTableEventHandler(dlg_GetLocationValueTable);

            dlg.GetBatchNoTable -= new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.GetBatchNoTable += new GetKeyCountListEventHandler(dlg_GetBatchNoTable);

            dlg.RefDbName = "";
            // dlg.MainForm = Program.MainForm;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.SourceStyle = "batchno";

            this.BatchNo = dlg.BatchNo;

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
                        "PrintAcceptForm",
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

                this.refid_table.Clear();
                this.orderxml_table.Clear();
            }

            EnableControls(false);

            LibraryChannel channel = this.GetChannel();

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
                    lRet = channel.SearchItem(
                        stop,
                        this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
                        //"<all>",
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
                    lRet = channel.SearchItem(
stop,
this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
                        //"<all>",
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

                // TODO: 下面可以用 ResultSetLoader 改造

                long lStart = 0;
                long lCount = lHitCount;
                Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        MessageBox.Show(this, "用户中断");
                        return;
                    }

                    lRet = channel.GetSearchResult(
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

                    // 2017/5/18
                    if (searchresults == null)
                    {
                        strError = "searchresults == null";
                        goto ERROR1;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        string strRecPath = searchresults[i].Path;

#if NO
                        // 因为检索的时候分了类型，所以这里没有必要对类型进行检查了
                        string strItemDbName = Global.GetDbName(strRecPath);
                        // 检查数据库类型(图书/期刊)
                        // 观察实体库是不是期刊库类型
                        // return:
                        //      -1  不是实体库
                        //      0   图书类型
                        //      1   期刊类型
                        nRet = Program.MainForm.IsSeriesTypeFromItemDbName(strItemDbName);
                        if (nRet == -1)
                        {
                            strError = "记录路径 '" + strRecPath + "' 中的数据库名 '" + strItemDbName + "' 不是实体库名";
                            goto ERROR1;
                        }

                        // 自动设置 图书/期刊 类型
                        if (bAutoSetSeriesType == true && lStart + i == 0)
                        {
                            if (nRet == 0)
                                this.comboBox_load_type.Text = "图书";
                            else
                                this.comboBox_load_type.Text = "连续出版物";
                        }

                        // 检查记录路径
                        if (this.comboBox_load_type.Text == "图书")
                        {
                            if (nRet != 0)
                            {
                                strError = "记录路径 '" + strRecPath + "' 中的数据库名 '" + strItemDbName + "' 不是一个图书类型的实体库名";
                                goto ERROR1;
                            }
                        }
                        else
                        {
                            if (nRet != 1)
                            {
                                strError = "记录路径 '" + strRecPath + "' 中的数据库名 '" + strItemDbName + "' 不是一个连续出版物类型的实体库名";
                                goto ERROR1;
                            }
                        }
#endif

                        // 根据记录路径，装入册记录
                        // return: 
                        //      -2  路径已经在list中存在了
                        //      -1  出错
                        //      1   成功
                        nRet = LoadOneItem(
                            channel,
                            this.comboBox_load_type.Text,
                            strRecPath,
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

                // 检查套内册完整性
                stop.SetMessage("正在 检查套内册完整性...");
                nRet = CheckSubCopy(out strError);
                if (nRet == -1)
                    goto ERROR1;

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

                this.ReturnChannel(channel);

                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void dlg_GetBatchNoTable(object sender, GetKeyCountListEventArgs e)
        {
            LibraryChannel channel = this.GetChannel();

            try
            {
                Global.GetBatchNoTable(e,
                    this,
                    this.comboBox_load_type.Text,
                    "item",
                    this.stop,
                    channel);
            }
            finally
            {
                this.ReturnChannel(channel);
            }

#if NOOOOOOOOOOOOOOOOOOOOOOOOOO
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
                    this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
                    // "<all>",
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

        // 根据订购批次号检索装载数据
        // parameters:
        //      strDefaultBatchNo   缺省的批次号。如果为null，则表示不使用这个参数。
        /// <summary>
        /// 根据订购批次号检索装载数据
        /// </summary>
        /// <param name="strDefaultBatchNo">缺省的批次号。如果为 null，则表示不使用这个参数</param>
        public void LoadFromOrderBatchNo(string strDefaultBatchNo)
        {
            SearchByBatchnoForm dlg = new SearchByBatchnoForm();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.CfgSectionName = "PrintAcceptForm_SearchByOrderBatchnoForm";
            this.BatchNo = "";

            dlg.Text = "根据订购批次号检索出已验收的册记录";
            dlg.DisplayLocationList = false;

            dlg.GetLocationValueTable -= new GetValueTableEventHandler(dlg_GetOrderLocationValueTable);
            dlg.GetLocationValueTable += new GetValueTableEventHandler(dlg_GetOrderLocationValueTable);

            dlg.GetBatchNoTable -= new GetKeyCountListEventHandler(dlg_GetOrderBatchNoTable);
            dlg.GetBatchNoTable += new GetKeyCountListEventHandler(dlg_GetOrderBatchNoTable);

            dlg.RefDbName = "";
            // dlg.MainForm = Program.MainForm;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.BatchNo = dlg.BatchNo;

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
                        "PrintAcceptForm",
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

                this.refid_table.Clear();
                this.orderxml_table.Clear();
            }

            EnableControls(false);

            LibraryChannel channel = this.GetChannel();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            try
            {
                stop.SetProgressRange(0, 100);

                long lRet = 0;
                if (this.BatchNo == "<不指定>")
                {
                    // 2013/3/27
                    lRet = channel.SearchOrder(
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
                        strError = "检索全部 '" + this.comboBox_load_type.Text + "' 类型的订购记录没有命中记录。";
                        goto ERROR1;
                    }
                }
                else
                {
                    lRet = channel.SearchOrder(
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

                int nOrderRecCount = 0;
                int nItemRecCount = 0;

                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        MessageBox.Show(this, "用户中断");
                        return;
                    }

                    lRet = channel.GetSearchResult(
                        stop,
                        "batchno",   // strResultSetName
                        lStart,
                        lCount,
                        "id", // "id,cols",
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

                        // 根据订购记录路径，装入订购记录关联的已验收册记录
                        // return: 
                        //      -2  路径已经在list中存在了
                        //      -1  出错
                        //      1   成功
                        nRet = LoadOrderItem(
                            channel,
                            strRecPath,
                            this.listView_origin,
                            ref nOrderRecCount,
                            ref nItemRecCount,
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

                // 检查套内册完整性
                stop.SetMessage("正在 检查套内册完整性...");
                nRet = CheckSubCopy(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 填充合并后数据列表
                stop.SetMessage("正在合并数据...");
                nRet = FillMergedList(out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nItemRecCount == 0)
                {
                    MessageBox.Show(this, "未装入任何已验收的册记录 (处理订购记录 " + nOrderRecCount.ToString() + " 条)");
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.ReturnChannel(channel);

                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 根据记录路径，检索出订购记录，并且装入相关联的已验收册记录
        // parameters:
        //      nOrderRecPath   总共检索的订购记录条数
        //      nItemRecCount   总共装入的册记录条数
        // return: 
        //      -2  路径已经在list中存在了
        //      -1  出错
        //      1   成功
        int LoadOrderItem(
            LibraryChannel channel,
            string strRecPath,
            ListView list,
            ref int nOrderRecCount,
            ref int nItemRecCount,
            out string strError)
        {
            strError = "";

            string strOrderXml = "";
            string strBiblioText = "";

            string strOutputOrderRecPath = "";
            string strOutputBiblioRecPath = "";

            byte[] item_timestamp = null;

            long lRet = channel.GetOrderInfo(
                stop,
                "@path:" + strRecPath,
                // "",
                "xml",
                out strOrderXml,
                out strOutputOrderRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strOutputBiblioRecPath,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                ListViewItem item = new ListViewItem(strRecPath, 0);

                OriginAcceptItemData data = new OriginAcceptItemData();
                item.Tag = data;
                data.Timestamp = item_timestamp;
                data.Xml = strOrderXml;

                item.SubItems.Add("获取订购记录 " + strRecPath + " 时出错: " + strError);

                SetItemColor(item, TYPE_ERROR);
                list.Items.Add(item);

                // 将新加入的事项滚入视野
                list.EnsureVisible(list.Items.Count - 1);

                return -1;
            }

            // 剖析一个订购xml记录，取出有关信息放入listview中
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strOrderXml);
            }
            catch (Exception ex)
            {
                strError = "订购记录XML装入DOM时出错: " + ex.Message;
                goto ERROR1;
            }

            List<string> distributes = new List<string>();

            if (this.comboBox_load_type.Text == "连续出版物")
            {
                string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
                if (string.IsNullOrEmpty(strRefID) == true)
                {
                    strError = "订购记录 '" + strRecPath + "' 中没有<refID>元素";
                    return -1;
                }

                string strBiblioDbName = Program.MainForm.GetBiblioDbNameFromOrderDbName(Global.GetDbName(strRecPath));
                string strIssueDbName = Program.MainForm.GetIssueDbName(strBiblioDbName);

                // 如果是期刊的订购库，还需要通过订购记录的refid获得期记录，从期记录中才能得到馆藏分配信息
                string strOutputStyle = "";
                lRet = channel.SearchIssue(stop,
    strIssueDbName, // "<全部>",
    strRefID,
    -1,
    "订购参考ID",
    "exact",
    this.Lang,
    "tempissue",
    "",
    strOutputStyle,
    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    strError = "检索 订购参考ID 为 " + strRefID + " 的期记录时出错: " + strError;
                    return -1;
                }

                long lHitCount = lRet;
                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 获取命中结果
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    lRet = channel.GetSearchResult(
                        stop,
                        "tempissue",
                        lStart,
                        lCount,
                        "id",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获取结果集时出错: " + strError;
                        return -1;
                    }
                    if (lRet == 0)
                    {
                        strError = "获取结果集时出错: lRet = 0";
                        return -1;
                    }

                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DigitalPlatform.LibraryClient.localhost.Record searchresult = searchresults[i];

                        string strIssueRecPath = searchresult.Path;

                        string strIssueXml = "";
                        string strOutputIssueRecPath = "";

                        lRet = channel.GetIssueInfo(
    stop,
    "@path:" + strIssueRecPath,
                            // "",
    "xml",
    out strIssueXml,
    out strOutputIssueRecPath,
    out item_timestamp,
    "recpath",
    out strBiblioText,
    out strOutputBiblioRecPath,
    out strError);
                        if (lRet == -1 || lRet == 0)
                        {
                            strError = "获取期记录 " + strIssueRecPath + " 时出错: " + strError;
                            return -1;
                        }

                        // 剖析一个期刊xml记录，取出有关信息
                        XmlDocument issue_dom = new XmlDocument();
                        try
                        {
                            issue_dom.LoadXml(strIssueXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "期记录 '" + strOutputIssueRecPath + "' XML装入DOM时出错: " + ex.Message;
                            return -1;
                        }

                        // 寻找 /orderInfo/* 元素
                        XmlNode nodeRoot = issue_dom.DocumentElement.SelectSingleNode("orderInfo/*[refID/text()='" + strRefID + "']");
                        if (nodeRoot == null)
                        {
                            strError = "期记录 '" + strOutputIssueRecPath + "' 中没有找到<refID>元素值为 '" + strRefID + "' 的订购内容节点...";
                            return -1;
                        }

                        string strDistribute = DomUtil.GetElementText(nodeRoot, "distribute");

                        distributes.Add(strDistribute);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }
            }
            else
            {
                string strDistribute = DomUtil.GetElementText(dom.DocumentElement, "distribute");
                distributes.Add(strDistribute);
            }

            if (distributes.Count == 0)
                return 0;

            nOrderRecCount++;

#if NO
            string strDistribute = DomUtil.GetElementText(dom.DocumentElement, "distribute");
            if (string.IsNullOrEmpty(strDistribute) == true)
                return 0;
#endif
            foreach (string strDistribute in distributes)
            {

                LocationCollection locations = new LocationCollection();
                int nRet = locations.Build(strDistribute,
                    out strError);
                if (nRet == -1)
                    return -1;

                for (int i = 0; i < locations.Count; i++)
                {
                    DigitalPlatform.Text.Location location = locations[i];

                    if (string.IsNullOrEmpty(location.RefID) == true)
                        continue;

                    string[] parts = location.RefID.Split(new char[] { '|' });
                    foreach (string text in parts)
                    {
                        string strRefID = text.Trim();
                        if (string.IsNullOrEmpty(strRefID) == true)
                            continue;

                        // 根据册记录的refid装入册记录
                        // return: 
                        //      -2  路径已经在list中存在了
                        //      -1  出错
                        //      1   成功
                        nRet = LoadOneItem(
                            channel,
                            this.comboBox_load_type.Text,
                            "@refID:" + strRefID,
                            list,
                            out strError);
                        if (nRet == -2)
                            continue;
                        if (nRet == -1)
                            continue;

                        nItemRecCount++;
                    }
                }
            }

            return 1;
        ERROR1:
            {
                ListViewItem item = new ListViewItem(strRecPath, 0);

                OriginAcceptItemData data = new OriginAcceptItemData();
                item.Tag = data;
                data.Timestamp = item_timestamp;
                data.Xml = strOrderXml;

                item.SubItems.Add(strError);

                SetItemColor(item, TYPE_ERROR);
                list.Items.Add(item);

                // 将新加入的事项滚入视野
                list.EnsureVisible(list.Items.Count - 1);
            }
            return -1;
        }

        void dlg_GetOrderBatchNoTable(object sender, GetKeyCountListEventArgs e)
        {
            LibraryChannel channel = this.GetChannel();
            try
            {
                Global.GetBatchNoTable(e,
                    this,
                    this.comboBox_load_type.Text,
                    "order",
                    this.stop,
                    channel);
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        void dlg_GetOrderLocationValueTable(object sender, GetValueTableEventArgs e)
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
                    strThisText = ListViewUtil.GetItemText(item, nSortColumn);
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

            bool in_loop = this.stop != null ? this.stop.IsInLoop : false;

            string strItemRecPath = "";
            if (this.listView_origin.SelectedItems.Count > 0)
            {
                strItemRecPath = ListViewUtil.GetItemText(this.listView_origin.SelectedItems[0], ORIGIN_COLUMN_RECPATH);
            }
            menuItem = new MenuItem("打开种册窗，观察册记录 '" + strItemRecPath + "' (&I)");
            menuItem.Tag = this.listView_origin;
            menuItem.Click += new System.EventHandler(this.menu_loadItemRecord_Click);
            if (this.listView_origin.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            string strOrderRecPath = "";
            if (this.listView_origin.SelectedItems.Count > 0)
            {
                strOrderRecPath = ListViewUtil.GetItemText(this.listView_origin.SelectedItems[0], ORIGIN_COLUMN_ORDERRECPATH);
            }
            menuItem = new MenuItem("打开种册窗，观察订购记录 '" + strOrderRecPath + "' (&O)");
            menuItem.Tag = this.listView_origin;
            menuItem.Click += new System.EventHandler(this.menu_loadOrderRecord_Click);
            if (this.listView_origin.SelectedItems.Count == 0
                || String.IsNullOrEmpty(strOrderRecPath) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("刷新选定的行(&S)");
            menuItem.Tag = this.listView_origin;
            menuItem.Click += new System.EventHandler(this.menu_refreshSelected_Click);
            if (this.listView_origin.SelectedItems.Count == 0
                || in_loop == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新全部行(&R)");
            menuItem.Tag = this.listView_origin;
            if (in_loop == true)
                menuItem.Enabled = false;
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

        // 打开种册窗，观察册记录
        void menu_loadItemRecord_Click(object sender, EventArgs e)
        {
            LoadItemToEntityForm(this.listView_origin);
        }

        void LoadItemToEntityForm(ListView list)
        {
            if (list.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装载到种册窗的事项");
                return;
            }

            string strRecPath = ListViewUtil.GetItemText(list.SelectedItems[0], ORIGIN_COLUMN_RECPATH);

            EntityForm form = new EntityForm();

            form.MainForm = Program.MainForm;
            form.MdiParent = Program.MainForm;
            form.Show();

            form.LoadItemByRecPath(strRecPath, false);
        }

#if NO
        void LoadToEntityForm(ListView list)
        {
            if (list.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装载的事项");
                return;
            }

            string strBarcode = "";
            string strRecPath = ListViewUtil.GetItemText(list.SelectedItems[0], ORIGIN_COLUMN_RECPATH);
            string strRefID = ListViewUtil.GetItemText(list.SelectedItems[0], ORIGIN_COLUMN_REFID);

            EntityForm form = new EntityForm();

            form.MainForm = Program.MainForm;
            form.MdiParent = Program.MainForm;
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
#endif

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

            string strRecPath = ListViewUtil.GetItemText(list.SelectedItems[0], ORIGIN_COLUMN_ORDERRECPATH);

            EntityForm form = new EntityForm();

            form.MainForm = Program.MainForm;
            form.MdiParent = Program.MainForm;
            form.Show();

            if (this.comboBox_load_type.Text == "图书")
                form.LoadOrderByRecPath(strRecPath, false);
            else
                form.LoadIssueByRecPath(strRecPath, false);
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
                MessageBox.Show(this, "尚未选定要移除的事项");
                return;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in list.SelectedItems)
            {
                items.Add(item);
            }

            DialogResult result = MessageBox.Show(this,
                "确实要在原始数据列表窗口内移除选定的 " + items.Count.ToString() + " 个事项?",
                "PrintAcceptForm",
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

            LibraryChannel channel = this.GetChannel();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新 ...");
            stop.BeginLoop();

            try
            {
                stop.SetProgressRange(0, items.Count);

                for (int i = 0; i < items.Count; i++)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断1";
                        goto ERROR1;
                    }


                    ListViewItem item = items[i];

                    stop.SetMessage("正在刷新 " + item.Text + " ...");

                    int nRet = RefreshOneItem(channel, item, out strError);
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

                this.ReturnChannel(channel);

                EnableControls(true);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int RefreshOneItem(
            LibraryChannel channel,
            ListViewItem item,
            out string strError)
        {
            strError = "";

            string strItemText = "";
            string strBiblioText = "";

            string strItemRecPath = "";
            string strBiblioRecPath = "";

            byte[] item_timestamp = null;

            string strIndex = "@path:" + ListViewUtil.GetItemText(item, ORIGIN_COLUMN_RECPATH);

            long lRet = channel.GetOrderInfo(
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
                ListViewUtil.ChangeItemText(item, 1, strError); // 1 对于两种listview都管用
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

                lRet = channel.GetBiblioInfos(
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
                strError = "PrintAcceptForm dom.LoadXml() {D08724A8-33B0-4DC8-8CD0-F58082E5DA7B} exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            {
                SetListViewItemText(dom,
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

        int OriginIndex(ListViewItem item)
        {
            return this.listView_origin.Items.IndexOf(item);
        }

        class Tao
        {
            public string OrderRecPath = "";    // 订购记录路径
            public string No = "";  // 套序号 从1开始计数
            public int Count = 0;   // 册数
            public List<string> IDs = new List<string>();   // 所包含的册序号。从1开始计数
            public string ErrorInfo = "";
        }

        static int AddOneItem(ref List<Tao> taos,
            string strOrderRecPath,
            string strNo,
            string strID,
            string strCount,
            out string strError)
        {
            strError = "";

            Tao tao = null;
            for (int i = 0; i < taos.Count; i++)
            {
                Tao current_tao = taos[i];
                if (current_tao.OrderRecPath == strOrderRecPath
                    && current_tao.No == strNo)
                {
                    tao = current_tao;
                    break;  // found
                }
            }

            if (tao == null)
            {
                tao = new Tao();
                tao.OrderRecPath = strOrderRecPath;
                tao.No = strNo;
                taos.Add(tao);
            }

            int nCount = 0;

            try
            {
                nCount = Convert.ToInt32(strCount);
            }
            catch
            {
                strError = "strCount '" + strCount + "' 格式不正确。应当为整数";
                return -1;
            }

            if (tao.Count == 0)
                tao.Count = nCount;
            else
            {
                if (nCount != tao.Count)
                    tao.ErrorInfo = "出现不一致的册数 '" + strCount + "'";
            }

            tao.IDs.Add(strID);
            return 0;
        }

        // 检查套内册的完整性
        int CheckSubCopy(out string strError)
        {
            strError = "";
            int nRet = 0;

            List<Tao> taos = new List<Tao>();

            for (int i = 0; i < this.listView_origin.Items.Count; i++)
            {
                ListViewItem source = this.listView_origin.Items[i];

                string strSubCopy = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_ACCEPTSUBCOPY);
                if (String.IsNullOrEmpty(strSubCopy) == true)
                    continue;

                string strNo = "";
                string strIndex = "";
                string strCopy = "";
                nRet = ParseSubCopy(strSubCopy,
        out strNo,
        out strIndex,
        out strCopy,
        out strError);
                if (nRet == -1)
                {
                    strError = "事项 " + (i + 1).ToString() + " 的套内详情格式错误: '" + strError + "'，请先排除问题...";
                    return -1;
                }

                string strOrderRecPath = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_ORDERRECPATH);

                nRet = AddOneItem(ref taos,
                    strOrderRecPath,
                    strNo,
                    strIndex,
                    strCopy,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            // 检查
            for (int i = 0; i < taos.Count; i++)
            {
                Tao tao = taos[i];
                if (String.IsNullOrEmpty(tao.ErrorInfo) == false)
                {
                    strError = "订购记录 '" + tao.OrderRecPath + "' 内第 " + tao.No + " 套 " + tao.ErrorInfo;
                    return -1;
                }

                // 检查序号连续性
                // 2014/6/21 修改排序方法
                tao.IDs.Sort(
                    delegate(string s1, string s2)
                    {
                        return StringUtil.RightAlignCompare(s1, s2);
                    }
                    );
                for (int j = 0; j < tao.IDs.Count; j++)
                {
                    if (j > 0)
                    {
                        string strID1 = tao.IDs[j - 1];
                        string strID2 = tao.IDs[j];

                        int v1 = 0;
                        int v2 = 0;
                        try
                        {
                            v1 = Convert.ToInt32(strID1);
                        }
                        catch
                        {
                            strError = "序号 '" + strID1 + "' 格式错误";
                            return -1;
                        }
                        try
                        {
                            v2 = Convert.ToInt32(strID2);
                        }
                        catch
                        {
                            strError = "序号 '" + strID2 + "' 格式错误";
                            return -1;
                        }

                        if (v1 + 1 != v2)
                        {
                            strError = "序号 '" + strID1 + "' 和 '" + strID2 + "' 之间不连续";
                            return -1;
                        }
                    }
                }

                if (tao.Count != tao.IDs.Count)
                {
                    strError = "订购记录 '" + tao.OrderRecPath + "' 内第 " + tao.No + " 套应有 " + tao.Count.ToString() + " 册，但当前只有 " + tao.IDs.Count.ToString() + " 册，出现了不完整情况";
                    return -1;
                }

            }

            return 0;
        }

        // 填充合并后数据列表
        int FillMergedList(out string strError)
        {
            strError = "";
            int nRet = 0;
            List<int> null_acceptprice_lineindexs = new List<int>();

            DateTime now = DateTime.Now;
            // int nOrderIdSeed = 1;

            this.listView_merged.Items.Clear();
            // 2008/11/22
            this.SortColumns_merged.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_merged.Columns);

            // 先将原始数据列表按照 bibliorecpath/seller/price 列排序
            SortOriginListForMerge();


            // 只提取一套一册的，和一套的第一册
            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < this.listView_origin.Items.Count; i++)
            {
                ListViewItem source = this.listView_origin.Items[i];

                string strSubCopy = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_ACCEPTSUBCOPY);
                if (String.IsNullOrEmpty(strSubCopy) == false)
                {
                    string strNo = "";
                    string strIndex = "";
                    string strCopy = "";
                    nRet = ParseSubCopy(strSubCopy,
            out strNo,
            out strIndex,
            out strCopy,
            out strError);
                    if (nRet == -1)
                    {
                        strError = "事项 " + (i + 1).ToString() + " 的套内详情格式错误: '" + strError + "'，请先排除问题...";
                        return -1;
                    }

                    // 不是各套的第一册，就跳过
                    if (strIndex != "1")
                        continue;
                }

                items.Add(source);
            }

            // 循环
            for (int i = 0; i < items.Count; i++)
            {
                int nCopy = 0;

                ListViewItem source = items[i];

                if (source.ImageIndex == TYPE_ERROR)
                {
                    strError = "事项 " + (OriginIndex(source) + 1).ToString() + " 的状态为错误，请先排除问题...";
                    return -1;
                }

                string strSubCopyDetail = ListViewUtil.GetItemText(source,
    ORIGIN_COLUMN_ACCEPTSUBCOPY);
                string strSubCopy = "";
                if (String.IsNullOrEmpty(strSubCopyDetail) == false)
                {
                    string strNo = "";
                    string strIndex = "";
                    nRet = ParseSubCopy(strSubCopyDetail,
            out strNo,
            out strIndex,
            out strSubCopy,
            out strError);
                    if (nRet == -1)
                    {
                        strError = "事项 " + (OriginIndex(source) + 1).ToString() + " 的套内详情格式错误: '" + strError + "'，请先排除问题...";
                        return -1;
                    }

                    Debug.Assert(strIndex == "1", "");
                }

                // 渠道
                string strSeller = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_SELLER);

                // 渠道地址
                string strSellerAddress = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_SELLERADDRESS);

                // 2013/5/31
                // 单价，实体价
                string strItemPrice = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_ITEMPRICE);


                // 2013/5/31
                // 单价，订购价
                string strOrderPrice = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_ORDERPRICE);


                // 单价，到书价
                string strAcceptPrice = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_ACCEPTPRICE);  // 2009/11/23 changed

                string strPublishTime = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_PUBLISHTIME);

                string strVolume = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_VOLUME);

                /*
                // price取其中的验收价部分
                {
                    string strOldPrice = "";
                    string strNewPrice = "";

                    // 分离 "old[new]" 内的两个值
                    OrderDesignControl.ParseOldNewValue(strPrice,
                        out strOldPrice,
                        out strNewPrice);

                    strPrice = strNewPrice;
                }*/

                // 书目记录路径
                string strBiblioRecPath = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_BIBLIORECPATH);

                // 书目号
                string strCatalogNo = ListViewUtil.GetItemText(source,
                    ORIGIN_COLUMN_CATALOGNO);

                string strMergeComment = "";    // 合并注释
                List<string> totalprices = new List<string>();  // 累积的价格字符串
                List<ListViewItem> origin_items = new List<ListViewItem>();

                string strComments = "";    // 原始注释(积累)
                string strDistributes = ""; // 合并的馆藏分配字符串

                // 发现biblioRecPath、price和seller均相同的区段
                int nStart = i; // 区段开始位置
                int nLength = 0;    // 区段内事项个数

                for (int j = i; j < items.Count; j++)
                {
                    ListViewItem current_source = items[j];

                    if (current_source.ImageIndex == TYPE_ERROR)
                    {
                        strError = "事项 " + (OriginIndex(current_source) + 1).ToString() + " 的状态为错误，请先排除问题...";
                        return -1;
                    }

                    // 渠道
                    string strCurrentSeller = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_SELLER);

                    // 渠道地址
                    string strCurrentSellerAddress = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_SELLERADDRESS);

                    string strCurrentItemPrice = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_ITEMPRICE);

                    string strCurrentOrderPrice = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_ORDERPRICE);

                    // 单价(册记录中的)
                    string strCurrentAcceptPrice = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_ACCEPTPRICE); // 2009/11/23 changed

                    string strCurrentPublishTime = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_PUBLISHTIME);

                    string strCurrentVolume = ListViewUtil.GetItemText(current_source,
                       ORIGIN_COLUMN_VOLUME);
                    /*
                    // price取其中的验收价部分
                    {
                        string strCurrentOldPrice = "";
                        string strCurrentNewPrice = "";

                        // 分离 "old[new]" 内的两个值
                        OrderDesignControl.ParseOldNewValue(strCurrentPrice,
                            out strCurrentOldPrice,
                            out strCurrentNewPrice);

                        strCurrentPrice = strCurrentNewPrice;
                    }*/

                    // 书目记录路径
                    string strCurrentBiblioRecPath = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_BIBLIORECPATH);

                    // 书目号
                    string strCurrentCatalogNo = ListViewUtil.GetItemText(current_source,
                        ORIGIN_COLUMN_CATALOGNO);

                    if (this.comboBox_load_type.Text == "图书")
                    {
                        // 七元组判断
                        if (strBiblioRecPath != strCurrentBiblioRecPath
                            || strSeller != strCurrentSeller
                            || strItemPrice != strCurrentItemPrice
                            || strOrderPrice != strCurrentOrderPrice
                            || strAcceptPrice != strCurrentAcceptPrice
                            || strCatalogNo != strCurrentCatalogNo
                            || PrintOrderForm.CompareAddress(strSellerAddress, strCurrentSellerAddress) != 0)
                            break;
                    }
                    else
                    {
                        Debug.Assert(this.comboBox_load_type.Text == "连续出版物", "");
                        // 九元组判断
                        if (strBiblioRecPath != strCurrentBiblioRecPath
                            || strSeller != strCurrentSeller
                            || strItemPrice != strCurrentItemPrice
                            || strOrderPrice != strCurrentOrderPrice
                            || strAcceptPrice != strCurrentAcceptPrice
                            || strCatalogNo != strCurrentCatalogNo
                            || strPublishTime != strCurrentPublishTime
                            || strVolume != strCurrentVolume
                            || PrintOrderForm.CompareAddress(strSellerAddress, strCurrentSellerAddress) != 0)
                            break;
                    }

                    int nCurCopy = 1;

                    // 汇总复本数
                    nCopy += nCurCopy;

                    // 汇总合并注释
                    string strSource = ListViewUtil.GetItemText(current_source, ORIGIN_COLUMN_SOURCE);
                    string strRecPath = ListViewUtil.GetItemText(current_source, ORIGIN_COLUMN_RECPATH);
                    if (String.IsNullOrEmpty(strMergeComment) == false)
                        strMergeComment += "; ";
                    strMergeComment += strSource + ", " + nCurCopy.ToString() + "册 (" + strRecPath + ")";

                    if (String.IsNullOrEmpty(strCurrentAcceptPrice) == true)
                    {
                        // 记载具有空到书价的行号
                        null_acceptprice_lineindexs.Add(j);
                    }
                    else
                    {
                        // 汇总价格
                        totalprices.Add(strCurrentAcceptPrice);
                    }

                    // 汇总注释
                    string strComment = ListViewUtil.GetItemText(current_source, ORIGIN_COLUMN_COMMENT);
                    if (String.IsNullOrEmpty(strComment) == false)
                    {
                        if (String.IsNullOrEmpty(strComments) == false)
                            strComments += "; ";
                        strComments += strComment + " @" + strRecPath;
                    }

                    // 汇总馆藏分配字符串
                    string strCurDistribute = ListViewUtil.GetItemText(current_source, ORIGIN_COLUMN_LOCATION) + ":1";
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

                // publish time
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_PUBLISHTIME,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_PUBLISHTIME));

                // volume
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_VOLUME,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_VOLUME));


                // merge comment
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_MERGECOMMENT,
                    strMergeComment);

                // copy
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_COPY,
                    nCopy.ToString());

                // subcopy
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_SUBCOPY,
                    strSubCopy);

                // item price
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ITEMPRICE,
                    strItemPrice);

                // order price
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ORDERPRICE,
                    strOrderPrice);

                // price
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_PRICE,
                    strAcceptPrice);

                List<string> sum_prices = null;
                nRet = TotalPrice(totalprices,
                    out sum_prices,
                    out strError);
                if (nRet == -1)
                {
                    // TODO: 报错的时候要列出数组中的每个字符串，便于诊断
                    return -1;
                }

                string strSumPrice = "";

                if (sum_prices.Count > 0)
                {
                    // TODO: 这里是否允许多种货币并存？
                    //Debug.Assert(sum_prices.Count == 1, "");
                    //strSumPrice = sum_prices[0];
                    strSumPrice = PriceUtil.JoinPriceString(sum_prices);    // 2017/2/23
                }
                // total price
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_TOTALPRICE,
                    strSumPrice);

                /* 打印验收单时间
                // order time
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ORDERTIME,
                    now.ToShortDateString());   // TODO: 注意这个时间要返回到原始数据中
                 * */

                // order time 需要归并汇总
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ORDERTIME,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_ORDERTIME));


                // order id 需要归并汇总
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_ORDERID,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_ORDERID));


                // distribute
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_DISTRIBUTE,
                    strDistributes);

                // class 需要归并汇总
                ListViewUtil.ChangeItemText(target, MERGED_COLUMN_CLASS,
                    ListViewUtil.GetItemText(source, ORIGIN_COLUMN_ORDERCLASS));

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

            if (null_acceptprice_lineindexs.Count > 0)
            {
                ListViewUtil.ClearSelection(this.listView_origin);
                for (int i = 0; i < null_acceptprice_lineindexs.Count; i++)
                {
                    this.listView_origin.Items[null_acceptprice_lineindexs[i]].Selected = true;
                }
                MessageBox.Show(this, "原始数据列表中共有 " + null_acceptprice_lineindexs.Count.ToString() + "  行的到书价为空，已被选定，请注意检查");
            }

            return 0;
        }


        // 设置listview item的changed状态
        static void SetItemChanged(ListViewItem item,
            bool bChanged)
        {
            OriginAcceptItemData data = (OriginAcceptItemData)item.Tag;
            if (data == null)
            {
                data = new OriginAcceptItemData();
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

        // 汇总价格
        // 货币单位不同的，互相独立
        static int TotalPrice(List<string> prices,
            out List<string> results,
            out string strError)
        {
            strError = "";
            results = new List<string>();

            List<PriceItem> items = new List<PriceItem>();

            // 变换为PriceItem
            for (int i = 0; i < prices.Count; i++)
            {
                string strPrefix = "";
                string strValue = "";
                string strPostfix = "";
                int nRet = PriceUtil.ParsePriceUnit(prices[i],
                    out strPrefix,
                    out strValue,
                    out strPostfix,
                    out strError);
                if (nRet == -1)
                    return -1;
                decimal value = 0;
                try
                {
                    value = Convert.ToDecimal(strValue);
                }
                catch
                {
                    strError = "数字 '" + strValue + "' 格式不正确";
                    return -1;
                }

                PriceItem item = new PriceItem();
                item.Prefix = strPrefix;
                item.Postfix = strPostfix;
                item.Value = value;

                items.Add(item);
            }

            // 汇总
            for (int i = 0; i < items.Count; i++)
            {
                PriceItem item = items[i];

                for (int j = i + 1; j < items.Count; j++)
                {
                    PriceItem current_item = items[j];
                    if (current_item.Prefix == item.Prefix
                        && current_item.Postfix == item.Postfix)
                    {
                        item.Value += current_item.Value;
                        items.RemoveAt(j);
                        j--;
                    }
                    else
                        break;
                }
            }

            // 输出
            for (int i = 0; i < items.Count; i++)
            {
                PriceItem item = items[i];

                results.Add(item.Prefix + item.Value.ToString() + item.Postfix);
            }

            return 0;
        }

        class PriceItem
        {
            public string Prefix = "";
            public string Postfix = "";
            public decimal Value = 0;
        }
#if NOOOOOOOOOOOOOOO
        // 计算价格乘积
        static int MultiPrice(string strPrice,
            int nCopy,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";
            int nRet = PriceUtil.ParsePriceUnit(strPrice,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return -1;

            decimal value = 0;
            try
            {
                value = Convert.ToDecimal(strValue);
            }
            catch
            {
                strError = "数字 '" + strValue + "' 格式不正确";
                return -1;
            }

            value *= (decimal)nCopy;

            strResult = strPrefix + value.ToString() + strPostfix;
            return 0;
        }
#endif

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
            column.No = ORIGIN_COLUMN_ACCEPTPRICE;  // 2009/11/23 changed
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
                MessageBox.Show(this, "尚未选定要移除的事项");
                return;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in list.SelectedItems)
            {
                items.Add(item);
            }

            DialogResult result = MessageBox.Show(this,
                "确实要在合并后列表窗口内移除选定的 " + items.Count.ToString() + " 个事项?",
                "PrintAcceptForm",
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

        // 原始数据 打印选项
        private void button_print_originOption_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            string strNamePath = "printaccept_origin_printoption";

            PrintOption option = new AcceptOriginPrintOption(Program.MainForm.UserDir, // Program.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(Program.MainForm.AppInfo,
                strNamePath);

            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            // dlg.MainForm = Program.MainForm;
            dlg.DataDir = Program.MainForm.UserDir; // .DataDir;
            dlg.Text = this.comboBox_load_type.Text + " 原始数据 打印参数";
            dlg.PrintOption = option;
            dlg.ColumnItems = new string[] {
                "no -- 序号",
                "recpath -- 册记录路径",
                "summary -- 摘要",
                "state -- 状态",
                "isbnIssn -- ISBN/ISSN",
                "publishTime -- 出版时间",
                "volume -- 卷期",
                "location -- 馆藏地点",
                "seller -- 渠道",
                "source -- 经费来源",

                "acceptPrice -- 验收单价",    // 验收单价
                "itemPrice -- 实体单价",
                "orderPrice -- 订购单价",

                "comment -- 注释",
                "batchNo -- 验收批次号",
                "refID -- 参考ID",
                "biblioRecpath -- 种记录路径",

                "catalogNo -- 书目号",
                "orderID -- 订单号",
                "orderClass -- 订购类别",
                "orderTime -- 订购时间",
                "orderPrice -- 订购价",
                "acceptPrice -- 到书价",

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

                "orderRecpath -- 订购记录路径"

            };


            Program.MainForm.AppInfo.LinkFormState(dlg, "orderorigin_printoption_formstate");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(Program.MainForm.AppInfo,
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

                printform.Text = "打印原始验收数据(册信息)";
                // printform.MainForm = Program.MainForm;
                printform.Filenames = filenames;

                Program.MainForm.AppInfo.LinkFormState(printform, "printaccept_htmlprint_formstate");
                printform.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(printform);
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
        // 打印原始数据
        int BuildOriginHtml(
            List<ListViewItem> items,
            out List<string> filenames,
            out string strError)
        {
            strError = "";

            Hashtable macro_table = new Hashtable();

            // 获得打印参数
            PrintOption option = new AcceptOriginPrintOption(Program.MainForm.UserDir, // Program.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(Program.MainForm.AppInfo,
                "printaccept_origin_printoption");

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

            // 2009/7/30 changed
            if (this.SourceStyle == "batchno")
            {
                macro_table["%batchno%"] = HttpUtility.HtmlEncode(this.BatchNo); // 批次号
            }
            else
            {
                Debug.Assert(this.SourceStyle == "barcodefile"
                    || this.SourceStyle == "recpathfile"
                    || this.SourceStyle == "",
                    "");

                macro_table["%batchno%"] = "";
            }

            macro_table["%pagecount%"] = nPageCount.ToString();
            macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            // 2009/7/30 changed
            if (this.SourceStyle == "recpathfile")
            {
                macro_table["%recpathfilepath%"] = this.RecPathFilePath;
                macro_table["%recpathfilename%"] = Path.GetFileName(this.RecPathFilePath);
            }
            else
            {
                Debug.Assert(this.SourceStyle == "batchno"
                    || this.SourceStyle == "barcodefile"
                    || this.SourceStyle == "",
                    "");

                macro_table["%recpathfilepath%"] = "";
                macro_table["%recpathfilename%"] = "";
            }

            // 批次号或文件名 多个宏不方便判断后选用，只好合并为一个宏
            macro_table["%batchno_or_recpathfilename%"] = String.IsNullOrEmpty(this.BatchNo) == true ? Path.GetFileName(this.RecPathFilePath) : this.BatchNo;
            macro_table["%sourcedescription%"] = this.SourceDescription;

            filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名

            string strFileNamePrefix = Program.MainForm.DataDir + "\\~printaccept";

            string strFileName = "";

            // 输出信息页
            {
                int nItemCount = items.Count;
                int nTotalCopies = GetOriginTotalCopies(items);
                int nTotalSeries = GetOriginTotalSeries(items);
                int nBiblioCount = GetOriginBiblioCount(items);
                string strTotalPrice = GetOriginTotalPrice(items);

                macro_table["%itemcount%"] = nItemCount.ToString(); // 事项数
                macro_table["%totalcopies%"] = nTotalCopies.ToString(); // 总册数
                macro_table["%totalseries%"] = nTotalSeries.ToString(); // 总套数
                macro_table["%bibliocount%"] = nBiblioCount.ToString(); // 种数
                macro_table["%totalprice%"] = strTotalPrice;    // 总价格

                macro_table["%pageno%"] = "1";

                // 2009/7/30
                macro_table["%datadir%"] = Program.MainForm.DataDir;   // 便于引用datadir下templates目录内的某些文件
                ////macro_table["%libraryserverdir%"] = Program.MainForm.LibraryServerDir;  // 便于引用服务器端的CSS文件
                // 2009/10/10
                macro_table["%cssfilepath%"] = this.GetAutoCssUrl(option, "acceptorigin.css");  // 便于引用服务器端或“css”模板的CSS文件

                strFileName = strFileNamePrefix + "0" + ".html";

                filenames.Add(strFileName);

                string strTemplateFilePath = option.GetTemplatePageFilePath("统计页");
                if (String.IsNullOrEmpty(strTemplateFilePath) == false)
                {
                    /*
<html>
<head>
    老用法<LINK href='%libraryserverdir%/printorigin.css' type='text/css' rel='stylesheet'>
	新用法<LINK href='%cssfilepath%' type='text/css' rel='stylesheet'>
</head>
<body>
    <div class='pageheader'>%date% 原始验收数据 - 来源: %sourcedescription% - (共 %pagecount% 页)</div>
    <div class='tabletitle'>%date% 原始验收数据</div>
    <div class='copies'>册数: %totalcopies%</div>
    <div class='bibliocount'>种数: %bibliocount%</div>
    <div class='totalprice'>总价: %totalprice%</div>
    <div class='sepline'><hr/></div>
    <div class='batchno'>批次号: %batchno%</div>
    <div class='location'>记录路径文件: %recpathfilepath%</div>
    <div class='sepline'><hr/></div>
    <div class='pagefooter'>%pageno%/%pagecount%</div>
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


                    BuildOriginPageTop(option,
                        macro_table,
                        strFileName,
                        false);

                    // 内容行

                    StreamUtil.WriteText(strFileName,
                        "<div class='itemcount'>事项数: " + nItemCount.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='bibliocount'>种数: " + nBiblioCount.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='series'>套数: " + nTotalSeries.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='copies'>册数: " + nTotalCopies.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='totalprice'>总价: " + strTotalPrice + "</div>");

                    BuildOriginPageBottom(option,
                        macro_table,
                        strFileName,
                        false);
                }

            }

            string strMarcFilterFilePath = option.GetTemplatePageFilePath("MARC过滤器");
            if (String.IsNullOrEmpty(strMarcFilterFilePath) == false)
            {
                int nRet = PrepareMarcFilter(strMarcFilterFilePath, out strError);
                if (nRet == -1)
                    return -1;
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
            string strLibraryServerUrl = Program.MainForm.AppInfo.GetString(
    "config",
    "circulation_server_url",
    "");
            int pos = strLibraryServerUrl.LastIndexOf("/");
            if (pos != -1)
                strLibraryServerUrl = strLibraryServerUrl.Substring(0, pos);
            */

            // string strCssUrl = Program.MainForm.LibraryServerDir + "/acceptorigin.css";
            // 2009/10/10 changed
            string strCssUrl = GetAutoCssUrl(option, "acceptorigin.css");

            /*
            // 2009/10/9
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                strCssUrl = strCssFilePath;
            else
                strCssUrl = Program.MainForm.LibraryServerDir + "/acceptorigin.css";    // 缺省的
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

                    string strClass = PrintOrderForm.GetClass(column.Name);

                    StreamUtil.WriteText(strFileName,
                        "<td class='" + strClass + "'>" + strCaption + "</td>");
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

            this.ColumnTable.Clear();   // 清除上一记录处理时残余的内容

            if (this.MarcFilter != null)
            {
                string strError = "";
                string strMARC = "";
                string strOutMarcSyntax = "";

                // TODO: 有错误要明显报出来，否则容易在打印出来后才发现，就晚了

                // 获得MARC格式书目记录
                string strBiblioRecPath = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_BIBLIORECPATH);

                if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                {
                    nRet = GetMarc(strBiblioRecPath,
                        out strMARC,
                        out strOutMarcSyntax,
                        out strError);
                    if (nRet == -1)
                    {
                        strLineContent = strError;
                        goto END1;
                    }

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

                string strClass = PrintOrderForm.GetClass(column.Name);

                strLineContent +=
                    "<td class='" + strClass + "'>" + strContent + "</td>";
            }

        END1:

            StreamUtil.WriteText(strFileName,
    "<tr class='content'>");

            StreamUtil.WriteText(strFileName,
    strLineContent);

            StreamUtil.WriteText(strFileName,
                "</tr>");

            return 0;
        }

        // 获得原始数据 栏目内容
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
            PrintOrderForm.ParseNameParam(StringUtil.GetLeft(strColumnName),
                out strName,
                out strParameters);

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
                    case "册记录路径":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_RECPATH);

                    case "errorInfo":
                    case "summary":
                    case "摘要":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SUMMARY);

                    case "state":
                    case "状态":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_STATE);

                    case "isbnIssn":
                    case "ISBN/ISSN":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ISBNISSN);

                    case "publishTime":
                    case "出版时间":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_PUBLISHTIME);

                    case "volume":
                    case "卷期":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_VOLUME);

                    case "location":
                    case "馆藏地点":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_LOCATION);

                    case "seller":
                    case "书商":
                    case "渠道":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER);

                    case "source":
                    case "经费来源":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SOURCE);

                    // 验收价
                    case "price":
                    case "acceptprice":
                    case "acceptPrice":
                    case "单价":
                    case "验收单价":
                    case "到书价":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ACCEPTPRICE);

                    // 实体记录中的价格
                    case "itemprice":
                    case "itemPrice":
                    case "实体单价":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ITEMPRICE);

                    // 订购价
                    case "orderprice":
                    case "orderPrice":
                    case "订购单价":
                    case "订购价":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERPRICE);


                    case "comment":
                    case "注释":
                    case "附注":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_COMMENT);

                    // 验收批次号
                    case "batchNo":
                    case "批次号":
                    case "验收批次号":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_BATCHNO);

                    case "refID":
                    case "参考ID":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_REFID);


                    case "biblioRecpath":
                    case "种记录路径":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_BIBLIORECPATH);

                    case "catalogNo":
                    case "书目号":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_CATALOGNO);

                    case "orderId":
                    case "orderID":
                    case "订单号":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERID);

                    case "class":
                    case "orderClass":
                    case "订购类别":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERCLASS);

                    case "orderTime":
                    case "订购时间":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERTIME);

#if NO
                    case "orderPrice":
                    case "订购价":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERPRICE);

                    case "acceptPrice":
                    case "到书价":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ACCEPTPRICE);
#endif

                    case "orderRecpath":
                    case "订购记录路径":
                        return ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERRECPATH);

                    // 格式化以后的渠道地址
                    case "sellerAddress":
                    case "渠道地址":
                        return PrintOrderForm.GetPrintableSellerAddress(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            strParameters);

                    case "sellerAddress:zipcode":
                    case "渠道地址:邮政编码":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "zipcode");

                    case "sellerAddress:address":
                    case "渠道地址:地址":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "address");

                    case "sellerAddress:department":
                    case "渠道地址:单位":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "department");

                    case "sellerAddress:name":
                    case "渠道地址:联系人":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "name");

                    case "sellerAddress:email":
                    case "渠道地址:Email地址":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "email");

                    case "sellerAddress:bank":
                    case "渠道地址:开户行":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "bank");

                    case "sellerAddress:accounts":
                    case "渠道地址:银行账号":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "accounts");

                    case "sellerAddress:payStyle":
                    case "渠道地址:汇款方式":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "payStyle");

                    case "sellerAddress:comment":
                    case "渠道地址:附注":
                        return PrintOrderForm.GetSellerAddressInnerValue(ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLERADDRESS),
                            ListViewUtil.GetItemText(item, ORIGIN_COLUMN_SELLER),
                            "comment");
                    default:
                        {
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
        "<div class='pagefooter'>" + strPageFooterText + "</div>");
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
                    strText = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_BIBLIORECPATH);
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

        // 获得原始列表中的总套数
        static int GetOriginTotalSeries(List<ListViewItem> items)
        {
            int total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strSubCopyDetail = ListViewUtil.GetItemText(item,
ORIGIN_COLUMN_ACCEPTSUBCOPY);
                if (String.IsNullOrEmpty(strSubCopyDetail) == true)
                {
                    total++;
                    continue;
                }

                string strNo = "";
                string strIndex = "";
                string strCopy = "";
                string strError = "";
                int nRet = ParseSubCopy(strSubCopyDetail,
        out strNo,
        out strIndex,
        out strCopy,
        out strError);
                if (nRet == -1)
                    continue;

                // 不是各套的第一册，就跳过
                if (strIndex == "1")
                    total++;
            }

            return total;
        }

        // 获得原始列表中的总册数
        static int GetOriginTotalCopies(List<ListViewItem> items)
        {
            return items.Count; // 2009/7/30 changed
            /*
            int total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];


                total++;
            }

            return total;
             * */
        }

        static string GetOriginTotalPrice(List<ListViewItem> items)
        {
            List<String> prices = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strPrice = "";

                try
                {
                    // 用到书价进行计算
                    strPrice = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ACCEPTPRICE);
                }
                catch
                {
                    continue;
                }

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                string strSubCopyDetail = ListViewUtil.GetItemText(item,
ORIGIN_COLUMN_ACCEPTSUBCOPY);
                if (String.IsNullOrEmpty(strSubCopyDetail) == true)
                {
                    prices.Add(strPrice);
                    continue;
                }

                string strNo = "";
                string strIndex = "";
                string strCopy = "";
                string strError = "";
                int nRet = ParseSubCopy(strSubCopyDetail,
        out strNo,
        out strIndex,
        out strCopy,
        out strError);
                if (nRet == -1)
                    continue;

                // 不是各套的第一册，就跳过
                if (strIndex == "1")
                    prices.Add(strPrice);

            }

            return PriceUtil.TotalPrice(prices);
        }

        private void button_saveChange_saveChange_Click(object sender, EventArgs e)
        {
            // 组织成批保存 SetOrders
            string strError = "";
            int nRet = SaveOrders(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
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

        // 保存对原始订购记录的修改
        int SaveOrders(out string strError)
        {
            strError = "";
            int nRet = 0;

            EnableControls(false);

            LibraryChannel channel = this.GetChannel();

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
                        strError = "原始数据列表中，第 " + (i + 1).ToString() + " 个事项为错误状态。需要先排除问题才能进行保存。";
                        return -1;
                    }

                    OriginAcceptItemData data = (OriginAcceptItemData)item.Tag;
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
                        nRet = SaveOneBatchOrders(
                            channel,
                            entity_list,
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
                        "state", ListViewUtil.GetItemText(item, ORIGIN_COLUMN_STATE));

                    EntityInfo info = new EntityInfo();

                    if (String.IsNullOrEmpty(data.RefID) == true)
                    {
                        data.RefID = Guid.NewGuid().ToString();
                    }

                    info.RefID = data.RefID;
                    info.Action = "change";
                    info.OldRecPath = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_RECPATH);
                    info.NewRecPath = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_RECPATH); ;

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
                    nRet = SaveOneBatchOrders(
                        channel,
                        entity_list,
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

                this.ReturnChannel(channel);

                EnableControls(true);
            }

            return 0;
        }

        int SaveOneBatchOrders(
            LibraryChannel channel,
            List<EntityInfo> entity_list,
            string strBiblioRecPath,
            out string strError)
        {
            strError = "";

            EntityInfo[] entities = new EntityInfo[entity_list.Count];
            entity_list.CopyTo(entities);

            EntityInfo[] errorinfos = null;
            long lRet = channel.SetOrders(
                stop,
                strBiblioRecPath,
                entities,
                out errorinfos,
                out strError);
            if (lRet == -1)
                return -1;

            string strErrorText = "";
            for (int i = 0; i < errorinfos.Length; i++)
            {
                EntityInfo errorinfo = errorinfos[i];

                Debug.Assert(String.IsNullOrEmpty(errorinfo.RefID) == false, "");

                ListViewItem item = null;
                OriginAcceptItemData data = FindDataByRefID(errorinfo.RefID, out item);
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
        OriginAcceptItemData FindDataByRefID(string strRefID,
            out ListViewItem item)
        {
            item = null;
            for (int i = 0; i < this.listView_origin.Items.Count; i++)
            {
                item = this.listView_origin.Items[i];
                OriginAcceptItemData data = (OriginAcceptItemData)item.Tag;
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
                    OriginAcceptItemData data = (OriginAcceptItemData)this.listView_origin.Items[i].Tag;
                    if (data.Changed == true)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 当前出版物类型
        /// </summary>
        public string PublicationType
        {
            get
            {
                return this.comboBox_load_type.Text;
            }
            set
            {
                if (value != "图书" && value != "连续出版物")
                {
                    throw new Exception("不合法的PublicationType值 '" + value + "'。必须为 '图书' 或 '连续出版物'");
                }

                this.comboBox_load_type.Text = value;
            }
        }

        private void PrintAcceptForm_Activated(object sender, EventArgs e)
        {
            // 2009/8/13
            Program.MainForm.stopManager.Active(this.stop);
        }

        private void comboBox_load_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*
            if (this.comboBox_load_type.Text == "图书")
            {
                this.button_load_loadFromOrderBatchNo.Enabled = true;
            }
            else
            {
                this.button_load_loadFromOrderBatchNo.Enabled = false;
            }
             * */
        }

        private void button_print_exchangeRateStatis_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = PrintExchangeRate("html", out strError);
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

        // 打印汇率统计表
        // parameters:
        //      strStyle    excel / html 之一或者逗号联接组合。 excel: 输出 Excel 文件
        int PrintExchangeRate(
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
                // string filepath = Path.Combine(Program.MainForm.UserDir, "test.xlsx");
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
                    strError = "PrintAcceptForm ExcelDocument.Create() exception: " + ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                doc.Stylesheet = GenerateStyleSheet();
                // doc.Initial();
            }


            this.tabControl_items.SelectedTab = this.tabPage_originItems;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建汇率表 ...");
            stop.BeginLoop();

            try
            {
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

                PrintExchangeRateList(items,
                    ref doc);
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            if (doc != null)
            {
                // Close the document.
                doc.Close();
            }

            return 1;
        }

        void PrintExchangeRateList(List<ListViewItem> items,
            ref ExcelDocument doc)
        {
            string strError = "";

            // 创建一个html文件，并显示在HtmlPrintForm中。
            List<string> filenames = null;
            try
            {
                // Debug.Assert(false, "");

                // 构造html页面
                int nRet = BuildExchangeRateHtml(
                    items,
                    ref doc,
                    out filenames,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                    goto ERROR1;

                if (doc == null)
                {
                    HtmlPrintForm printform = new HtmlPrintForm();

                    printform.Text = "打印汇率统计表";
                    // printform.MainForm = Program.MainForm;
                    printform.Filenames = filenames;

                    Program.MainForm.AppInfo.LinkFormState(printform, "printaccept_htmlprint_formstate");
                    printform.ShowDialog(this);
                    Program.MainForm.AppInfo.UnlinkFormState(printform);
                }
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

        Hashtable m_exchangeTable = new Hashtable();

        // 累计一个金额字符串
        int AddCurrency(string strCurrentcyString1,
            int nSubCopy1,
            string strCurrentcyString2,
            int nSubCopy2,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strCurrentcyString1) == true
                || string.IsNullOrEmpty(strCurrentcyString2) == true)
                return 0;

            string strPrefix1 = "";
            string strValue1 = "";
            string strPostfix1 = "";
            int nRet = PriceUtil.ParsePriceUnit(strCurrentcyString1,
                out strPrefix1,
                out strValue1,
                out strPostfix1,
                out strError);
            if (nRet == -1)
                return -1;

            double value1 = 0;
            try
            {
                value1 = Convert.ToDouble(strValue1);
            }
            catch
            {
                strError = "数字 '" + strValue1 + "' 格式不正确";
                return -1;
            }

            if (value1 == 0)
                return 0;

            value1 = value1 / (double)nSubCopy1;

            //
            string strPrefix2 = "";
            string strValue2 = "";
            string strPostfix2 = "";
            nRet = PriceUtil.ParsePriceUnit(strCurrentcyString2,
                out strPrefix2,
                out strValue2,
                out strPostfix2,
                out strError);
            if (nRet == -1)
                return -1;

            double value2 = 0;
            try
            {
                value2 = Convert.ToDouble(strValue2);
            }
            catch
            {
                strError = "数字 '" + strValue2 + "' 格式不正确";
                return -1;
            }

            if (value2 == 0)
                return 0;

            value2 = value2 / (double)nSubCopy2;

            if (strPrefix1 == strPrefix2
                && strPostfix1 == strPostfix2)
                return 0;   // 源和目标相同的币种不进入累计

            string strKey = strPrefix1 + "|" + strPostfix1 + "-->" + strPrefix2 + "|" + strPostfix2;
            ExchangeInfo info = (ExchangeInfo)this.m_exchangeTable[strKey];
            if (info == null)
            {
                info = new ExchangeInfo();
                this.m_exchangeTable[strKey] = info;
                info.OriginCurrency = strPrefix1 + " " + strPostfix1;
                info.TargetCurrency = strPrefix2 + " " + strPostfix2;
            }

            info.OrginValue += value1;
            info.TargetValue += value2;

            return 1;
        }

        // 输出 Excel 页面头部信息
        int BuildExchangeRateExcelPageTop(PrintOption option,
            Hashtable macro_table,
            ref ExcelDocument doc,
            int nTitleCols)
        {
            // 表格标题
            string strTableTitleText = "%date% 汇率表";

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

        // 构造html页面
        // 打印汇率统计表
        // return:
        //      -1  出错
        //      0   没有内容
        //      1   成功
        int BuildExchangeRateHtml(
            List<ListViewItem> items,
            ref ExcelDocument doc,
            out List<string> filenames,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名

            // 获得打印参数
            string strNamePath = "printaccept_exchangerate_printoption";
            ExchangeRatePrintOption option = new ExchangeRatePrintOption(Program.MainForm.UserDir, // Program.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(Program.MainForm.AppInfo,
                strNamePath);

            this.m_exchangeTable.Clear();

            Hashtable macro_table = new Hashtable();

            // 2009/7/30 changed
            if (this.SourceStyle == "batchno")
            {
                macro_table["%batchno%"] = HttpUtility.HtmlEncode(this.BatchNo); // 批次号
            }
            else
            {
                Debug.Assert(this.SourceStyle == "barcodefile"
                    || this.SourceStyle == "recpathfile"
                    || this.SourceStyle == "",
                    "");

                macro_table["%batchno%"] = "";
            }

            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            if (this.SourceStyle == "recpathfile")
            {
                macro_table["%recpathfilepath%"] = this.RecPathFilePath;
                macro_table["%recpathfilename%"] = Path.GetFileName(this.RecPathFilePath);
            }
            else
            {
                Debug.Assert(this.SourceStyle == "batchno"
                    || this.SourceStyle == "barcodefile"
                    || this.SourceStyle == "",
                    "");

                macro_table["%recpathfilepath%"] = "";
                macro_table["%recpathfilename%"] = "";
            }

            // 批次号或文件名 多个宏不方便判断后选用，只好合并为一个宏
            macro_table["%batchno_or_recpathfilename%"] = String.IsNullOrEmpty(this.BatchNo) == true ? Path.GetFileName(this.RecPathFilePath) : this.BatchNo;
            macro_table["%sourcedescription%"] = this.SourceDescription;

            string strCssUrl = GetAutoCssUrl(option, "exchangeratetable.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            macro_table["%link%"] = strLink;

            string strResult = "";

            // 准备模板页
            string strStatisTemplateFilePath = "";

            strStatisTemplateFilePath = option.GetTemplatePageFilePath("汇率表");

            if (String.IsNullOrEmpty(strStatisTemplateFilePath) == true)
            {
                strStatisTemplateFilePath = PathUtil.MergePath(Program.MainForm.DataDir, "default_printaccept_exchangeratetable.template");
            }

            Debug.Assert(String.IsNullOrEmpty(strStatisTemplateFilePath) == false, "");

            if (File.Exists(strStatisTemplateFilePath) == false)
            {
                strError = "分类统计模板文件 '" + strStatisTemplateFilePath + "' 不存在，创建汇率表失败";
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


            stop.SetProgressValue(0);
            stop.SetProgressRange(0, items.Count);

            stop.SetMessage("正在遍历原始数据行 ...");
            for (int i = 0; i < items.Count; i++)
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                ListViewItem item = items[i];

                string strOrderRecPath = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERRECPATH);
                if (string.IsNullOrEmpty(strOrderRecPath) == true)
                    continue;

                string strOrderPrice = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERPRICE);
                string strAcceptPrice = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ACCEPTPRICE);

                string strOrderSubCopy = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ORDERSUBCOPY);
                string strAcceptSubCopy = ListViewUtil.GetItemText(item, ORIGIN_COLUMN_ACCEPTSUBCOPY);

                // ACCEPTSUBCOPY需要加工一下。原始形态为 1:1/3 需要取出'/'右边的数字
                {
                    strAcceptSubCopy = GetRightFromAccptSubCopyString(strAcceptSubCopy);
                    if (string.IsNullOrEmpty(strAcceptSubCopy) == true)
                        strAcceptSubCopy = "1";
                }

                int nOrderSubCopy = 1;
                if (Int32.TryParse(strOrderSubCopy, out nOrderSubCopy) == false)
                {
                    strError = "记录 " + strOrderRecPath + " 中订购时套内册数字符串 '" + strOrderSubCopy + "' 格式错误";
                    return -1;
                }

                int nAcceptSubCopy = 1;
                if (Int32.TryParse(strAcceptSubCopy, out nAcceptSubCopy) == false)
                {
                    strError = "记录 " + strOrderRecPath + " 中验收时套内册数字符串 '" + strAcceptSubCopy + "' 格式错误";
                    return -1;
                }

                // 2014/2/18
                nOrderSubCopy = nAcceptSubCopy;

                // 累计一个金额字符串
                nRet = AddCurrency(strOrderPrice,
                    nOrderSubCopy,
                    strAcceptPrice,
                    nAcceptSubCopy,
                    out strError);
                if (nRet == -1)
                {
                    return -1;
                }
            }

            if (this.m_exchangeTable.Count == 0)
            {
                strError = "没有可打印的内容";
                return 0;
            }

            string strFileName = Program.MainForm.DataDir + "\\~printaccept_exchangerate";
            filenames.Add(strFileName);

            Sheet sheet = null;
            if (doc != null)
                sheet = doc.NewSheet("汇率表");

            int nLineIndex = 2;
            if (doc != null)
            {
                BuildExchangeRateExcelPageTop(option,
    macro_table,
    ref doc,
    4);

                {
                    List<string> captions = new List<string>();
                    captions.Add("对照关系");
                    captions.Add("源金额");
                    captions.Add("目标金额");
                    captions.Add("汇率");
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


            string strTableContent = "<table class='exchangerate'>";

            // 栏目标题行
            {
                strTableContent += "<tr class='column'>";
                strTableContent += "<td class='relation'>对照关系</td>";
                strTableContent += "<td class='origincurrency'>源金额</td>";
                strTableContent += "<td class='targetcurrency'>目标金额</td>";
                strTableContent += "<td class='exchangerate'>汇率</td>";
            }

            // 排序事项?
            string[] keys = new string[this.m_exchangeTable.Keys.Count];
            this.m_exchangeTable.Keys.CopyTo(keys, 0);
            Array.Sort(keys);


            stop.SetMessage("正在输出统计页 HTML ...");
            foreach (string key in keys)
            {
                ExchangeInfo info = (ExchangeInfo)this.m_exchangeTable[key];
                strTableContent += "<tr class='content' >";
                strTableContent += "<td class='relation'>" + HttpUtility.HtmlEncode(info.OriginCurrency + " / " + info.TargetCurrency) + "</td>";
                strTableContent += "<td class='origincurrency'>" + HttpUtility.HtmlEncode(info.OrginValue.ToString()) + "</td>";
                strTableContent += "<td class='targetcurrency'>" + HttpUtility.HtmlEncode(info.TargetValue.ToString()) + "</td>";
                strTableContent += "<td class='exchangerate'>" +
                    (info.TargetValue / info.OrginValue).ToString() + "</td>";
                strTableContent += "</tr>";

                if (doc != null)
                {
                    nLineIndex++;
                    int i = 0;
                    doc.WriteExcelCell(
            nLineIndex,
            i++,
            info.OriginCurrency + " / " + info.TargetCurrency,
            true);
                    doc.WriteExcelCell(
nLineIndex,
i++,
info.OrginValue.ToString(),
false);
                    doc.WriteExcelCell(
nLineIndex,
i++,
info.TargetValue.ToString(),
false);
                    doc.WriteExcelCell(
nLineIndex,
i++,
(info.TargetValue / info.OrginValue).ToString(),
false);
                }

            }

            strTableContent += "</table>";

            strResult = strResult.Replace("{table}", strTableContent);

            // 内容行
            StreamUtil.WriteText(strFileName,
                strResult);

            return 1;
        }

        // 从套内详情字符串中得到套内册数部分
        // 也就是 "1:1/3"返回"3"部分。如果没有找到'/'，就返回""
        static string GetRightFromAccptSubCopyString(string strText)
        {
            int nRet = strText.IndexOf("/");
            if (nRet == -1)
                return "";

            return strText.Substring(nRet + 1).Trim();
        }

        private void listView_origin_DoubleClick(object sender, EventArgs e)
        {
            LoadItemToEntityForm(this.listView_origin);
        }

        // 汇率表 打印选项
        private void button_print_exchangeRateOption_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            string strNamePath = "printaccept_exchangerate_printoption";

            PrintOption option = new ExchangeRatePrintOption(Program.MainForm.UserDir, // Program.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(Program.MainForm.AppInfo,
                strNamePath);

            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            // dlg.MainForm = Program.MainForm;
            dlg.DataDir = Program.MainForm.UserDir; // .DataDir;
            dlg.Text = this.comboBox_load_type.Text + " 汇率表 打印参数";
            dlg.PrintOption = option;
            dlg.ColumnItems = new string[] {
            };

            Program.MainForm.AppInfo.LinkFormState(dlg, "order_exchangerate_printoption_formstate");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(Program.MainForm.AppInfo,
                strNamePath);
        }

        private void toolStripMenuItem_printExchangeRate_outputExcel_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = PrintExchangeRate("excel", out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

    }

    // 合并后数据打印 定义了特定缺省值的PrintOption派生类
    internal class PrintAcceptPrintOption : PrintOption
    {
        string PublicationType = "图书"; // 图书 连续出版物

        public PrintAcceptPrintOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            this.PageHeaderDefault = "%date% %seller% 验收单 - 来源: %sourcedescription% - (共 %pagecount% 页)";
            this.PageFooterDefault = "%pageno%/%pagecount%";

            this.TableTitleDefault = "%date% %seller% 验收单";

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

            if (this.PublicationType == "连续出版物")
            {
                // "publishTime -- 出版时间"
                column = new Column();
                column.Name = "publishTime -- 出版时间";
                column.Caption = "出版时间";
                column.MaxChars = -1;
                this.Columns.Add(column);

                // "volume -- 卷期"
                column = new Column();
                column.Name = "volume -- 卷期";
                column.Caption = "卷期";
                column.MaxChars = -1;
                this.Columns.Add(column);
            }

            // "price -- 单价"
            // "acceptPrice -- 验收单价"
            column = new Column();
            column.Name = "acceptPrice -- 验收单价";
            column.Caption = "验收单价";
            column.MaxChars = -1;
            this.Columns.Add(column);

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

    // 原始数据打印 定义了特定缺省值的PrintOption派生类
    internal class AcceptOriginPrintOption : PrintOption
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

        public AcceptOriginPrintOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            this.PageHeaderDefault = "%date% 原始验收数据 - 来源: %sourcedescription% - (共 %pagecount% 页)";
            this.PageFooterDefault = "%pageno%/%pagecount%";

            this.TableTitleDefault = "%date% 原始验收数据";

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

            if (this.PublicationType == "连续出版物")
            {
                // "publishTime -- 出版时间"
                column = new Column();
                column.Name = "publishTime -- 出版时间";
                column.Caption = "出版时间";
                column.MaxChars = -1;
                this.Columns.Add(column);

                // "volume -- 卷期"
                column = new Column();
                column.Name = "volume -- 卷期";
                column.Caption = "卷期";
                column.MaxChars = -1;
                this.Columns.Add(column);
            }

            // "price -- 单价"
            // "acceptPrice -- 验收单价"
            column = new Column();
            column.Name = "acceptPrice -- 验收单价";
            column.Caption = "验收单价";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // 原始数据的每条就是一册，所以没有复本和总价字段
            /*
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
             * */

            // "orderClass -- 类别"
            column = new Column();
            column.Name = "orderClass -- 类别";
            column.Caption = "类别";
            column.MaxChars = -1;
            this.Columns.Add(column);

        }
    }

    // 原始数据listviewitem的Tag所携带的数据结构
    /*public*/
    class OriginAcceptItemData
    {
        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed = false;
        public byte[] Timestamp = null;
        public string Xml = ""; // 订购记录的XML记录体
        public string RefID = "";   // 保存记录时候用的refid
    }

    // 汇率表数据打印 定义了特定缺省值的PrintOption派生类
    internal class ExchangeRatePrintOption : PrintOption
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

        public ExchangeRatePrintOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            this.PageHeaderDefault = "";
            this.PageFooterDefault = "";

            this.TableTitleDefault = "";
            // this.TableTitleDefault = "%date% 汇率统计表";

            this.LinesPerPageDefault = 0;

            // Columns缺省值
            Columns.Clear();
        }
    }


    /*public*/
    class ExchangeInfo
    {
        // 源货币
        public string OriginCurrency = "";
        public double OrginValue = 0;

        public string TargetCurrency = "";
        public double TargetValue = 0;
    }
}