﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Core;
using System.Collections;

namespace dp2Circulation
{
    /// <summary>
    /// 交费窗
    /// </summary>
    public partial class AmerceForm : MyForm
    {
        CommentViewerForm m_operlogViewer = null;
        const string NOTSUPPORT = "<html><body><p>[暂不支持]</p></body></html>";

        /*
        bool m_bStopFilling = true;
        internal Thread threadFillSummary = null;
         * */

        volatile bool m_bStopFillAmercing = true;
        internal Thread threadFillAmercing = null;
        FillAmercingParam FillAmercingParam = null;

        volatile bool m_bStopFillAmerced = true;
        internal Thread threadFillAmerced = null;
        FillAmercedParam FillAmercedParam = null;

        // 图标下标
        const int ITEMTYPE_AMERCED = 0;
        const int ITEMTYPE_NEWLY_SETTLEMENTED = 1;
        const int ITEMTYPE_OLD_SETTLEMENTED = 2;
        const int ITEMTYPE_UNKNOWN = 3;
        const int ITEMTYPE_ERROR = 3;

        int m_nChannelInUse = 0;

        Commander commander = null;

        const int WM_LOADSIZE = API.WM_USER + 201;

        const int WM_LOAD = API.WM_USER + 300;
        const int WM_UNDO_AMERCE = API.WM_USER + 301;
        const int WM_MODIFY_PRICE_AND_COMMENT = API.WM_USER + 302;
        const int WM_AMERCE = API.WM_USER + 303;


        WebExternalHost m_webExternalHost = new WebExternalHost();

        #region 待交费列表的列号
        /// <summary>
        /// 待交费列表的列号：册条码号
        /// </summary>
        public const int COLUMN_AMERCING_ITEMBARCODE = 0;
        /// <summary>
        /// 待交费列表的列号: 书目摘要
        /// </summary>
        public const int COLUMN_AMERCING_BIBLIOSUMMARY = 1;
        /// <summary>
        /// 待交费列表的列号: 金额
        /// </summary>
        public const int COLUMN_AMERCING_PRICE = 2;
        /// <summary>
        /// 待交费列表的列号: 注释
        /// </summary>
        public const int COLUMN_AMERCING_COMMENT = 3;
        /// <summary>
        /// 待交费列表的列号: 事由
        /// </summary>
        public const int COLUMN_AMERCING_REASON = 4;
        /// <summary>
        /// 待交费列表的列号: 开始时间
        /// </summary>
        public const int COLUMN_AMERCING_BORROWDATE = 5;
        /// <summary>
        /// 待交费列表的列号: 持续时间
        /// </summary>
        public const int COLUMN_AMERCING_BORROWPERIOD = 6;
        /// <summary>
        /// 待交费列表的列号: 开始操作者
        /// </summary>
        public const int COLUMN_AMERCING_BORROWOPERATOR = 7;    //
        /// <summary>
        /// 待交费列表的列号: 结束时间
        /// </summary>
        public const int COLUMN_AMERCING_RETURNDATE = 8;
        /// <summary>
        /// 待交费列表的列号: 结束操作者
        /// </summary>
        public const int COLUMN_AMERCING_RETURNOPERATOR = 9;    //
        /// <summary>
        /// 待交费列表的列号: 交费 ID
        /// </summary>
        public const int COLUMN_AMERCING_ID = 10;

        #endregion

        #region 已交费列表的列号
        /// <summary>
        /// 已交费列表的列号: 册条码号
        /// </summary>
        public const int COLUMN_AMERCED_ITEMBARCODE = 0;
        /// <summary>
        /// 已交费列表的列号: 书目摘要
        /// </summary>
        public const int COLUMN_AMERCED_BIBLIOSUMMARY = 1;
        /// <summary>
        /// 已交费列表的列号: 金额
        /// </summary>
        public const int COLUMN_AMERCED_PRICE = 2;
        /// <summary>
        /// 已交费列表的列号: 注释
        /// </summary>
        public const int COLUMN_AMERCED_COMMENT = 3;
        /// <summary>
        /// 已交费列表的列号: 事由
        /// </summary>
        public const int COLUMN_AMERCED_REASON = 4;
        /// <summary>
        /// 已交费列表的列号: 开始时间
        /// </summary>
        public const int COLUMN_AMERCED_BORROWDATE = 5;
        /// <summary>
        /// 已交费列表的列号: 持续时间
        /// </summary>
        public const int COLUMN_AMERCED_BORROWPERIOD = 6;
        /// <summary>
        /// 已交费列表的列号: 结束时间
        /// </summary>
        public const int COLUMN_AMERCED_RETURNDATE = 7;
        /// <summary>
        /// 已交费列表的列号: 交费 ID
        /// </summary>
        public const int COLUMN_AMERCED_ID = 8;
        /// <summary>
        /// 已交费列表的列号: 结束操作者
        /// </summary>
        public const int COLUMN_AMERCED_RETURNOPERATOR = 9;
        /// <summary>
        /// 已交费列表的列号: 状态
        /// </summary>
        public const int COLUMN_AMERCED_STATE = 10;
        /// <summary>
        /// 已交费列表的列号: 交费操作者
        /// </summary>
        public const int COLUMN_AMERCED_AMERCEOPERATOR = 11;
        /// <summary>
        /// 已交费列表的列号: 交费时间
        /// </summary>
        public const int COLUMN_AMERCED_AMERCETIME = 12;
        /// <summary>
        /// 已交费列表的列号: 结算操作者
        /// </summary>
        public const int COLUMN_AMERCED_SETTLEMENTOPERATOR = 13;
        /// <summary>
        /// 已交费列表的列号: 结算时间
        /// </summary>
        public const int COLUMN_AMERCED_SETTLEMENTTIME = 14;
        /// <summary>
        /// 已交费列表的列号: 交费记录路径
        /// </summary>
        public const int COLUMN_AMERCED_RECPATH = 15;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public AmerceForm()
        {
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_amerced.Tag = prop;
            prop.SetSortStyle(COLUMN_AMERCED_RECPATH, ColumnSortStyle.RecPath);
            prop.SetSortStyle(COLUMN_AMERCED_PRICE, ColumnSortStyle.RightAlign);
            prop.SetSortStyle(COLUMN_AMERCED_BORROWPERIOD, ColumnSortStyle.RightAlign);

            ListViewProperty prop_1 = new ListViewProperty();
            this.listView_overdues.Tag = prop_1;
            prop_1.SetSortStyle(COLUMN_AMERCING_PRICE, ColumnSortStyle.RightAlign);
            prop_1.SetSortStyle(COLUMN_AMERCING_BORROWPERIOD, ColumnSortStyle.RightAlign);
        }

        private void AmerceForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
#if NO
            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            // webbrowser
            this.m_webExternalHost.Initial(// Program.MainForm, 
                this.webBrowser_readerInfo);
            this.webBrowser_readerInfo.ObjectForScripting = this.m_webExternalHost;

            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);

            /*
            this.listView_amerced.SmallImageList = this.imageList_itemType;
            this.listView_amerced.LargeImageList = this.imageList_itemType;
             * */

            this.checkBox_fillSummary.Checked = Program.MainForm.AppInfo.GetBoolean(
                "amerce_form",
                "fill_summary",
                true);

            if (this.LayoutMode == "左右分布")
                this.splitContainer_main.Orientation = Orientation.Vertical;
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_webExternalHost.ChannelInUse;
        }

        /*public*/
        void LoadSize()
        {
#if NO
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state");
#endif

            try
            {
                // 获得splitContainer_main的状态
                Program.MainForm.LoadSplitterPos(
    this.splitContainer_main,
    "amerceform_state",
    "splitContainer_main_ratio");

                // 获得splitContainer_upper的状态
                Program.MainForm.LoadSplitterPos(
this.splitContainer_lists,
"amerceform_state",
"splitContainer_lists_ratio");

            }
            catch
            {
            }

            string strWidths = Program.MainForm.AppInfo.GetString(
                "amerce_form",
                "amerced_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_amerced,
                    strWidths,
                    true);
            }

            strWidths = Program.MainForm.AppInfo.GetString(
                "amerce_form",
                "overdues_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_overdues,
                    strWidths,
                    true);
            }

            //this.panel_amerced_command.PerformLayout();
            //this.panel_amercing_command.PerformLayout();

            //this.tableLayoutPanel_amerced.PerformLayout();
            //this.tableLayoutPanel_amercingOverdue.PerformLayout();

            // this.PerformLayout();
        }

        /*public*/
        void SaveSize()
        {
            if (Program.MainForm != null)
            {
                // 保存splitContainer_main的状态
                Program.MainForm.SaveSplitterPos(
                    this.splitContainer_main,
                    "amerceform_state",
                    "splitContainer_main_ratio");
                // 保存splitContainer_upper的状态
                Program.MainForm.SaveSplitterPos(
                    this.splitContainer_lists,
                    "amerceform_state",
                    "splitContainer_lists_ratio");

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_amerced);
                Program.MainForm.AppInfo?.SetString(
                    "amerce_form",
                    "amerced_list_column_width",
                    strWidths);

                strWidths = ListViewUtil.GetColumnWidthListString(this.listView_overdues);
                Program.MainForm.AppInfo?.SetString(
                    "amerce_form",
                    "overdues_list_column_width",
                    strWidths);
            }
        }

        // 
        /// <summary>
        /// 交费窗布局方式
        /// </summary>
        public string LayoutMode
        {
            get
            {
                return Program.MainForm.AppInfo.GetString("amerce_form",
        "layout",
        "左右分布");
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
                case WM_LOAD:
                    if (this.m_webExternalHost.CanCallNew(
                        this.commander,
                        m.Msg) == true)
                    {
                        LoadReader(this.textBox_readerBarcode.Text, true);
                    }
                    return;
                case WM_UNDO_AMERCE:
                    if (this.m_webExternalHost.CanCallNew(
                        this.commander,
                        m.Msg) == true)
                    {
                        UndoAmerce();
                    }
                    return;
                case WM_MODIFY_PRICE_AND_COMMENT:
                    if (this.m_webExternalHost.CanCallNew(
                        this.commander,
                        m.Msg) == true)
                    {
                        ModifyPriceAndComment();
                    }
                    return;
                case WM_AMERCE:
                    if (this.m_webExternalHost.CanCallNew(
                        this.commander,
                        m.Msg) == true)
                    {
                        AmerceSubmit();
                    }
                    return;

            }
            base.DefWndProc(ref m);
        }

        private void AmerceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopFillAmercing(true);

            StopFillAmerced(true);

            /*
            this.m_bStopFilling = true;

            if (this.threadFillSummary != null)
                this.threadFillSummary.Abort();
             * */
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

            string strError = "";
            string strInfo = "";
            // 获得当前窗口中是否有修改后未兑现的状态(是、否)和信息(具体哪些行有修改信息)
            // return:
            //      -1  执行过程发生错误
            //      0   没有修改
            //      >0  修改过，发生过修改的行数。详细信息在strInfo中
            int nRet = GetChangedInfo(
                out strInfo,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, "GetChangedInfo() error : " + strError);
                return;
            }

            if (nRet > 0)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前窗口内有下列行发生了修改，但尚未提交到服务器:\r\n---\r\n"
    + strInfo
    + "\r\n---"
    + "\r\n\r\n警告: 若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "AmerceForm",
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

        private void AmerceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetBoolean(
        "amerce_form",
        "fill_summary",
        this.checkBox_fillSummary.Checked);
            }

            if (this.commander != null)
                this.commander.Destroy();

            if (this.m_webExternalHost != null)
                this.m_webExternalHost.Destroy();
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif

            SaveSize();
        }

        // 从读者记录 XML 字符串中取出读者证条码号
        // return:
        //      -1  出错
        //      0   成功
        static int GetReaderBarcode(string strReaderXml,
            out string strReaderBarcode,
            out string strError)
        {
            strError = "";
            strReaderBarcode = "";
            if (string.IsNullOrEmpty(strReaderXml) == true)
                return 0;

            XmlDocument reader_dom = new XmlDocument();
            try
            {
                reader_dom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "读者记录 XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }
            strReaderBarcode = DomUtil.GetElementText(reader_dom.DocumentElement, "barcode");
            return 0;
        }

        // 装入读者记录
        // parameters:
        //      strBarcode  [in][out] 输入证条码号，或者身份证号等其他号码。输出证条码号
        // return:
        //      -1  error
        //      0   not found
        //      >=1 命中的读者记录条数
        int LoadReaderHtmlRecord(ref string strBarcode,
            out string strXml,
            out string strError)
        {
            strXml = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strBarcode) == true)
            {
                strError = "strBarcode参数不能为空";
                goto ERROR1;
            }

#if NO
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在初始化浏览器组件 ...");
            stop.BeginLoop();
#endif

            //this.Update();
            //Program.MainForm.Update();

            int nRecordCount = 0;

            try
            {

                Global.ClearHtmlPage(this.webBrowser_readerInfo, Program.MainForm.DataDir);

                byte[] baTimestamp = null;
                string strOutputRecPath = "";
                int nRedoCount = 0;

                REDO:

                stop.SetMessage("正在装入读者记录 " + strBarcode + " ...");
                string[] results = null;

                long lRet = Channel.GetReaderInfo(
                    stop,
                    strBarcode,
                    "html,xml",
                    out results,
                    out strOutputRecPath,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                {
                    // Global.SetHtmlString(this.webBrowser_readerInfo, "证条码号为 '" + strBarcode + "' 的读者记录没有找到 ...");
                    this.m_webExternalHost.SetTextString("证条码号为 '" + strBarcode + "' 的读者记录没有找到 ...");

                    return 0;   // not found
                }

                if (lRet == -1)
                    goto ERROR1;

                nRecordCount = (int)lRet;

                if (lRet > 1 && nRedoCount == 0)
                {
                    SelectPatronDialog dlg = new SelectPatronDialog();

                    dlg.Overflow = StringUtil.SplitList(strOutputRecPath).Count < lRet;
                    nRet = dlg.Initial(
                        // Program.MainForm,
                        StringUtil.SplitList(strOutputRecPath),
                        "请选择一个读者记录",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    // TODO: 保存窗口内的尺寸状态
                    Program.MainForm.AppInfo.LinkFormState(dlg, "AmerceForm_SelectPatronDialog_state");
                    dlg.ShowDialog(this);
                    Program.MainForm.AppInfo.UnlinkFormState(dlg);

                    if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        strError = "放弃选择";
                        return 0;
                    }

                    // strBarcode = dlg.SelectedBarcode;
                    strBarcode = "@path:" + dlg.SelectedRecPath;   // 2015/11/16

                    nRedoCount++;
                    goto REDO;
                }


                if (results == null || results.Length < 2)
                {
                    strError = "返回的results不正常。";
                    goto ERROR1;
                }

                string strHtml = "";
                strHtml = results[0];
                strXml = results[1];

                // 2013/10/14
                string strOutputBarcode = "";
                // 从读者记录 XML 字符串中取出读者证条码号
                // return:
                //      -1  出错
                //      0   成功
                nRet = GetReaderBarcode(strXml,
                    out strOutputBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;

                strBarcode = strOutputBarcode;

#if NO
                Global.SetHtmlString(this.webBrowser_readerInfo,
                    strHtml,
                    Program.MainForm.DataDir,
                    "amercing_reader");
#endif
                this.m_webExternalHost.SetHtmlString(strHtml,
                    "amercing_reader");
            }
            finally
            {
#if NO
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
#endif
            }

            return nRecordCount;
            ERROR1:
            return -1;
        }

        void ClearAllDisplay()
        {
            //this.listView_overdues.Items.Clear();
            //this.listView_amerced.Items.Clear();
            Global.ClearHtmlPage(this.webBrowser_readerInfo,
                Program.MainForm.DataDir);
            /*
            this.button_amerced_undoAmerce.Enabled = false;
            this.button_amercingOverdue_submit.Enabled = false;
             * */
            SetAmercedButtonsEnable();
            SetOverduesButtonsEnable();
        }

        void ClearAllDisplay1()
        {
            Global.ClearHtmlPage(this.webBrowser_readerInfo,
                Program.MainForm.DataDir);

            SetAmercedButtonsEnable();
            SetOverduesButtonsEnable();
        }

        void ClearHtmlAndAmercingDisplay()
        {
            //this.listView_overdues.Items.Clear();
            Global.ClearHtmlPage(this.webBrowser_readerInfo,
                Program.MainForm.DataDir);
            this.toolStripButton_submit.Enabled = false;
        }

        // 装载一个读者的相关信息
        // parameters:
        //      bForceLoad  遇到条码命中多条记录的时候，是否强行装入?
        // return:
        //      -1  error
        //      0   not found
        //      1   found and loaded
        /// <summary>
        /// 装载一个读者的相关信息
        /// </summary>
        /// <param name="strReaderBarcode">读者证条码号。或者身份证号等其他号码</param>
        /// <param name="bForceLoad">遇到条码命中多条记录的时候，是否强行装入?</param>
        /// <returns>
        /// <para>-1: 出错</para>
        /// <para>0: 没有找到</para>
        /// <para>1: 找到，并已经装载</para>
        /// </returns>
        public int LoadReader(string strReaderBarcode,
            bool bForceLoad)
        {
            string strError = "";

            // this.StopFillSummary();
            StopFillAmercing(false);
            StopFillAmerced(false);

            this.m_nChannelInUse++;
            if (this.m_nChannelInUse > 1)
            {
                this.m_nChannelInUse--;
                MessageBox.Show(this, "通道已经被占用。请稍后重试");
                return -1;
            }
            try
            {
                if (this.textBox_readerBarcode.Text != strReaderBarcode)
                    this.textBox_readerBarcode.Text = strReaderBarcode;

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在装载读者信息 ...");
                stop.BeginLoop();

                EnableControls(false);
                try
                {
                    // 搜索出记录，显示在窗口中
                    ClearAllDisplay();

                    // 装入读者记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      >=1 命中的读者记录条数
                    int nRet = LoadReaderHtmlRecord(ref strReaderBarcode,
                        out string strXml,
                        out strError);

                    if (this.textBox_readerBarcode.Text != strReaderBarcode)
                        this.textBox_readerBarcode.Text = strReaderBarcode;

                    if (nRet == -1)
                    {
#if NO
                        Global.SetHtmlString(this.webBrowser_readerInfo,
                            "装载读者记录发生错误: " + strError);
#endif
                        this.m_webExternalHost.SetTextString("装载读者记录发生错误: " + strError);
                    }

                    if (nRet == 0)
                        return 0;

                    if (nRet > 1)
                    {
                        if (bForceLoad == true)
                        {
                            strError = "条码 " + strReaderBarcode + " 命中记录 " + nRet.ToString() + " 条，但仍装入其中第一条读者记录。\r\n\r\n这是一个严重错误，请系统管理员尽快排除。";
                            MessageBox.Show(this, strError);    // 警告后继续装入第一条 
                        }
                        else
                        {
                            strError = "条码 " + strReaderBarcode + " 命中记录 " + nRet.ToString() + " 条，放弃装入读者记录。\r\n\r\n注意这是一个严重错误，请系统管理员尽快排除。";
                            goto ERROR1;    // 当出错处理
                        }
                    }

                    if (String.IsNullOrEmpty(strXml) == false)
                    {
#if NO
                        nRet = FillAmercingList(strXml,
                            out strError);
                        if (nRet == -1)
                        {
                            // strError = "FillAmercingList()发生错误: " + strError;
                            // goto ERROR1;
                            SetError(this.listView_overdues, strError);
                        }
#endif
                        BeginFillAmercing(strXml);
                    }

#if NO
                    nRet = LoadAmercedRecords(this.textBox_readerBarcode.Text,
                        out strError);
                    if (nRet == -1)
                    {
                        //strError = "LoadAmercedRecords()发生错误: " + strError;
                        //goto ERROR1;
                        SetError(this.listView_amerced, strError);
                    }
#endif
                    BeginFillAmerced(this.textBox_readerBarcode.Text, null);

#if NO
                    if (this.checkBox_fillSummary.Checked == true)
                        this.BeginFillSummary();
#endif

                }
                finally
                {
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }
            finally
            {
                this.m_nChannelInUse--;
            }

            return 1;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }


        private void button_load_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.textBox_readerBarcode.Text) == true)
            {
                MessageBox.Show(this, "尚未输入读者证条码号");
                return;
            }

            this.button_load.Enabled = false;

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_LOAD);
        }

#if NO
        // 从“违约金”库检索出已经交了违约金的记录，并显示在listiview中
        // return:
        //      -1  error
        //      0   not found
        //      1   找到并填入
        int LoadAmercedRecords(string strReaderBarcode,
            out string strError)
        {
            strError = "";

            this.listView_amerced.Items.Clear();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("正在检索已交费用记录 " + strReaderBarcode + " ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();


            try
            {
                string strDbName = "违约金";
                string strFrom = "读者证条码";
                string strMatchStyle = "exact";
                string strLang = "zh";
                string strQueryXml = "";

                long lRet = Channel.GetSystemParameter(
                    stop,
                    "amerce",
                    "dbname",
                    out strDbName,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 2010/12/16 change
                if (lRet == 0 || String.IsNullOrEmpty(strDbName) == true)
                {
                    if (String.IsNullOrEmpty(strError) == true)
                        strError = "违约金库名没有配置。";
                    goto ERROR1;
                }

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                strQueryXml = "<target list='" + strDbName + ":" + strFrom + "'><item><word>"
    + StringUtil.GetXmlStringSimple(strReaderBarcode)
    + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + strLang + "</lang></target>";

                lRet = Channel.Search(
                    stop,
                    strQueryXml,
                    "amerced",
                    "", // strOutputStyle
                    out strError);
                if (lRet == 0)
                {
                    strError = "not found";
                    return 0;   // not found
                }
                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                Record[] searchresults = null;


                // 获得结果集，装入listview
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    lRet = Channel.GetSearchResult(
                        stop,
                        "amerced",   // strResultSetName
                        lStart,
                        lPerCount,
                        "id",   // "id,cols"
                        strLang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        return 0;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        string strPath = searchresults[i].Path;

                        byte[] timestamp = null;
                        string strXml = "";

                        lRet = Channel.GetRecord(stop,
                            strPath,
                            out timestamp,
                            out strXml,
                            out strError);
                        if (lRet == -1)
                        {
                            goto ERROR1;
                        }

                        int nRet = FillAmercedLine(
                            stop,
                            strXml,
                            strPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                    }

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 从“违约金”库检索出部分已经交了违约金的记录，并追加显示在listiview中
        // return:
        //      -1  error
        //      0   not found
        //      1   找到并填入
        int LoadAmercedRecords(
            List<string> ids,
            out string strError)
        {
            strError = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("正在获取已交费用记录 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();


            try
            {
                string strDbName = "违约金";
                string strFrom = "ID";
                string strMatchStyle = "exact";
                string strLang = "zh";
                string strQueryXml = "";

                long lRet = Channel.GetSystemParameter(
                    stop,
                    "amerce",
                    "dbname",
                    out strDbName,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0 || String.IsNullOrEmpty(strDbName) == true)
                {
                    if (String.IsNullOrEmpty(strError) == true)
                        strError = "违约金库名没有配置。";
                    goto ERROR1;
                }

                strQueryXml = "<target list='" + strDbName + ":" + strFrom + "'>";
                for (int i = 0; i < ids.Count;i++ )
                {
                    string strID = ids[i];

                    if (i > 0)
                        strQueryXml += "<operator value='OR' />";

                    strQueryXml += "<item><word>"
        + StringUtil.GetXmlStringSimple(strID)
        + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + strLang + "</lang>";
                }
                strQueryXml += "</target>";

                lRet = Channel.Search(
                    stop,
                    strQueryXml,
                    "amerced",
                    "", // strOutputStyle
                    out strError);
                if (lRet == 0)
                {
                    strError = "not found";
                    return 0;   // not found
                }
                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                Record[] searchresults = null;


                // 获得结果集，装入listview
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    lRet = Channel.GetSearchResult(
                        stop,
                        "amerced",   // strResultSetName
                        lStart,
                        lPerCount,
                        "id",   // "id,cols"
                        strLang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        return 0;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        string strPath = searchresults[i].Path;

                        byte[] timestamp = null;
                        string strXml = "";

                        lRet = Channel.GetRecord(stop,
                            strPath,
                            out timestamp,
                            out strXml,
                            out strError);
                        if (lRet == -1)
                        {
                            goto ERROR1;
                        }

                        int nRet = FillAmercedLine(
                            stop,
                            strXml,
                            strPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                    }

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

#endif

        #region 装载已经交费事项的线程

        void StopFillAmerced(bool bForce)
        {
            // 如果以前在做，立即停止
            m_bStopFillAmerced = true;


            if (bForce == true)
            {
                if (this.threadFillAmerced != null)
                {
                    if (!this.threadFillAmerced.Join(2000))
                        this.threadFillAmerced.Abort();

                    this.threadFillAmerced = null;
                }
            }
        }

        // 开始填充已交费信息
        // 如果有ids，则表示追加它们。否则为重新装载strReaderBaroode指明的读者的已交费记录
        void BeginFillAmerced(string strReaderBarcode,
            List<string> ids)
        {
            // 如果以前在做，立即停止
            StopFillAmerced(true);

            this.FillAmercedParam = new FillAmercedParam();
            this.FillAmercedParam.ReaderBarcode = strReaderBarcode;
            this.FillAmercedParam.IDs = ids;
            this.FillAmercedParam.FillSummary = this.checkBox_fillSummary.Checked;

            this.threadFillAmerced =
        new Thread(new ThreadStart(this.ThreadFillAmercedMain));
            this.threadFillAmerced.Start();
        }

        void ThreadFillAmercedMain()
        {
            string strError = "";
            m_bStopFillAmerced = false;

            LibraryChannel channel = new LibraryChannel();
            channel.Url = Program.MainForm.LibraryServerUrl;

            channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            try
            {
                string strResultSetName = "";
                // 获得一些系统参数
                string strDbName = "违约金";
                string strQueryXml = "";
                string strLang = "zh";

                long lRet = Channel.GetSystemParameter(
                    stop,
                    "amerce",
                    "dbname",
                    out strDbName,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (m_bStopFillAmerced == true)
                    return;

                // 2010/12/16 change
                if (lRet == 0 || String.IsNullOrEmpty(strDbName) == true)
                {
                    if (String.IsNullOrEmpty(strError) == true)
                        strError = "违约金库名没有配置。";
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(this.FillAmercedParam.ReaderBarcode) == false)
                {
                    ClearList(this.listView_amerced);

                    string strFrom = "读者证条码";
                    string strMatchStyle = "exact";

                    // 2007/4/5 改造 加上了 GetXmlStringSimple()
                    strQueryXml = "<target list='" + strDbName + ":" + strFrom + "'><item><word>"
        + StringUtil.GetXmlStringSimple(this.FillAmercedParam.ReaderBarcode)
        + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + strLang + "</lang></target>";

                    strResultSetName = "amercing";
                } // end of strReaderBarcode != ""
                else
                {
                    if (this.FillAmercedParam.IDs == null || this.FillAmercedParam.IDs.Count == 0)
                    {
                        strError = "IDs 参数不能为空";
                        goto ERROR1;
                    }

                    string strFrom = "ID";
                    string strMatchStyle = "exact";

                    strQueryXml = "<target list='" + strDbName + ":" + strFrom + "'>";
                    for (int i = 0; i < this.FillAmercedParam.IDs.Count; i++)
                    {
                        string strID = this.FillAmercedParam.IDs[i];

                        if (i > 0)
                            strQueryXml += "<operator value='OR' />";

                        strQueryXml += "<item><word>"
            + StringUtil.GetXmlStringSimple(strID)
            + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + strLang + "</lang>";
                    }
                    strQueryXml += "</target>";

                    strResultSetName = "amerced";
                }

                // 开始检索
                lRet = channel.Search(
    stop,
    strQueryXml,
    strResultSetName,
    "", // strOutputStyle
    out strError);
                if (lRet == 0)
                {
                    strError = "not found";
                    return;   // not found
                }
                if (lRet == -1)
                    goto ERROR1;

                if (m_bStopFillAmerced == true)
                    return;

                long lHitCount = lRet;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                Record[] searchresults = null;

                // 获得结果集，装入listview
                for (; ; )
                {

                    if (m_bStopFillAmerced == true)
                    {
                        strError = "中断，列表不完整...";
                        goto ERROR1;
                    }
                    // stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    lRet = channel.GetSearchResult(
                        stop,
                        strResultSetName,   // strResultSetName
                        lStart,
                        lPerCount,
                        "id",   // "id,cols"
                        strLang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        return;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        if (m_bStopFillAmerced == true)
                        {
                            strError = "中断，列表不完整...";
                            goto ERROR1;
                        }

                        string strPath = searchresults[i].Path;

                        byte[] timestamp = null;
                        string strXml = "";

                        lRet = channel.GetRecord(stop,
                            strPath,
                            out timestamp,
                            out strXml,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ErrorCode.AccessDenied)
                            {
                                // 2018/11/6
                                SetError(listView_amerced, "获取记录 '" + strPath + "' 时权限不够: " + strError);
                                continue;
                            }
                            goto ERROR1;
                        }

                        int nRet = Safe_fillAmercedLine(
                            stop,
                            strXml,
                            strPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
                }


                // 第二阶段，填充摘要
                if (this.FillAmercedParam.FillSummary == true)
                {
                    List<ListViewItem> items = GetItemList(this.listView_amerced);

                    for (int i = 0; i < items.Count; i++)
                    {
                        if (this.m_bStopFillAmerced == true)
                            return;

                        ListViewItem item = items[i];

                        string strSummary = "";
                        string strItemBarcode = "";

                        Safe_getBarcodeAndSummary(listView_amerced,
        item,
        out strItemBarcode,
        out strSummary);

                        // 已经有内容了，就不刷新了
                        if (String.IsNullOrEmpty(strSummary) == false)
                            continue;

                        if (String.IsNullOrEmpty(strItemBarcode) == true
                            /*&& String.IsNullOrEmpty(strItemRecPath) == true*/)
                            continue;

                        try
                        {
                            string strBiblioRecPath = "";
                            lRet = channel.GetBiblioSummary(
                                null,
                                strItemBarcode,
                                "", // strItemRecPath,
                                null,
                                out strBiblioRecPath,
                                out strSummary,
                                out strError);
                            if (lRet == -1)
                            {
                                strSummary = strError;  // 2009/3/13 changed
                                // return -1;
                            }

                        }
                        finally
                        {
                        }

                        ChangeItemText(item, COLUMN_AMERCING_BIBLIOSUMMARY, strSummary);
                    }
                }
                return;
            }
            finally
            {
                channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
                channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
                channel.Close();
                m_bStopFillAmerced = true;
            }

            ERROR1:
            SetError(this.listView_amerced, strError);
            // Safe_errorBox(strError);
        }

        // FillAmercedLine
        delegate int Delegate_FillAmercedLine(Stop stop,
            string strXml,
            string strRecPath,
            out string strError);


        int Safe_fillAmercedLine(Stop stop,
            string strXml,
            string strRecPath,
            out string strError)
        {
            //string strItemBarcodeParam = "";
            //string strSummaryParam = "";
            Delegate_FillAmercedLine d = new Delegate_FillAmercedLine(FillAmercedLine);
            object[] args = new object[] { stop, strXml, strRecPath, "" };
            int nRet = (int)this.Invoke(d, args);
            strError = (string)args[3];
            return nRet;
        }


        #endregion

        #region 装载未交费事项的线程

        void StopFillAmercing(bool bForce)
        {
            // 如果以前在做，立即停止
            m_bStopFillAmercing = true;

            if (bForce == true)
            {
                if (this.threadFillAmercing != null)
                {
                    if (!this.threadFillAmercing.Join(2000))
                        this.threadFillAmercing.Abort();
                    this.threadFillAmercing = null;
                }
            }
        }

        void BeginFillAmercing(string strXml)
        {
            // 如果以前在做，立即停止
            StopFillAmercing(true);

            FillAmercingParam = new FillAmercingParam();
            this.FillAmercingParam.Xml = strXml;
            this.FillAmercingParam.FillSummary = this.checkBox_fillSummary.Checked;

            this.threadFillAmercing =
        new Thread(new ThreadStart(this.ThreadFillAmercingMain));
            this.threadFillAmercing.Start();
        }

        // ClearList
        // delegate void Delegate_ClearList(ListView list);

        void ClearList(ListView list)
        {
            this.Invoke((Action)(() =>
            {
                list.Items.Clear();
            }));
        }

#if NO
        void Safe_clearList(ListView list)
        {
            Delegate_ClearList d = new Delegate_ClearList(ClearList);
            this.Invoke(d, new object[] { list });
        }
#endif

        // ErrorBox
        // delegate void Delegate_ErrorBox(string strText);

        void ErrorBox(string strText)
        {
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, strText);
            }));
        }

#if NO
        void Safe_errorBox(string strText)
        {
            Delegate_ErrorBox d = new Delegate_ErrorBox(ErrorBox);
            this.Invoke(d, new object[] { strText });
        }
#endif

        // AddListItem
        // delegate void Delegate_AddListItem(ListView list, ListViewItem item);

        void AddListItem(ListView list, ListViewItem item)
        {
            this.Invoke((Action)(() =>
            {
                list.Items.Add(item);
            }));
        }

#if NO
        void Safe_addListItem(ListView list, ListViewItem item)
        {
            Delegate_AddListItem d = new Delegate_AddListItem(AddListItem);
            this.Invoke(d, new object[] { list, item });
        }
#endif

        // GetBarcodeAndSummary
        delegate void Delegate_GetBarcodeAndSummary(ListView list,
            ListViewItem item,
            out string strItemBarcode,
            out string strSummary);

        void GetBarcodeAndSummary(ListView list,
            ListViewItem item,
            out string strItemBarcode,
            out string strSummary)
        {
            if (list == this.listView_overdues)
            {
                strSummary = ListViewUtil.GetItemText(item, COLUMN_AMERCING_BIBLIOSUMMARY);
                strItemBarcode = ListViewUtil.GetItemText(item, COLUMN_AMERCING_ITEMBARCODE);
            }
            else
            {
                strSummary = ListViewUtil.GetItemText(item, COLUMN_AMERCED_BIBLIOSUMMARY);
                strItemBarcode = ListViewUtil.GetItemText(item, COLUMN_AMERCED_ITEMBARCODE);
            }
        }

        void Safe_getBarcodeAndSummary(ListView list,
            ListViewItem item,
            out string strItemBarcode,
            out string strSummary)
        {
            //string strItemBarcodeParam = "";
            //string strSummaryParam = "";
            Delegate_GetBarcodeAndSummary d = new Delegate_GetBarcodeAndSummary(GetBarcodeAndSummary);
            object[] args = new object[] { list, item, "", "" };
            this.Invoke(d, args);
            strItemBarcode = (string)args[2];
            strSummary = (string)args[3];
        }

        // ChangeItemText
        // delegate void Delegate_ChangeItemText(ListViewItem item, int nCol, string strText);

        void ChangeItemText(ListViewItem item, int nCol, string strText)
        {
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, nCol, strText);
            }));
        }

#if NO
        void Safe_changeItemText(ListViewItem item, int nCol, string strText)
        {
            Delegate_ChangeItemText d = new Delegate_ChangeItemText(ChangeItemText);
            this.Invoke(d, new object[] { item, nCol, strText });
        }
#endif

        // GetItemList
        // delegate List<ListViewItem> Delegate_GetItemList(ListView list);

        List<ListViewItem> GetItemList(ListView list)
        {
            return (List<ListViewItem>)this.Invoke((Func<List<ListViewItem>>)(() =>
            {
                List<ListViewItem> results = new List<ListViewItem>();
                foreach (ListViewItem item in list.Items)
                {
                    results.Add(item);
                }
                return results;
            }));
        }

#if NO
        List<ListViewItem> Safe_getItemList(ListView list)
        {
            Delegate_GetItemList d = new Delegate_GetItemList(GetItemList);
            return (List<ListViewItem>)this.Invoke(d, new object[] { list });
        }
#endif

#if NO
        // SetError
        delegate void Delegate_SetError(ListView list,
            string strError);
#endif

        // 设置错误字符串显示
        static void SetError(ListView list,
            string strError)
        {
            list.Invoke((Action)(() =>
            {
                ListViewItem item = new ListViewItem();
                // item.ImageIndex = ITEMTYPE_ERROR;
                item.Text = "";
                item.SubItems.Add("错误: " + strError);
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCED_STATE, "error");
                list.Items.Add(item);
            }));
        }

#if NO
        void Safe_setError(ListView list,
            string strError)
        {
            Delegate_SetError d = new Delegate_SetError(SetError);
            this.Invoke(d, new object[] { list, strError });
        }
#endif

        /*public*/
        void ThreadFillAmercingMain()
        {
            string strError = "";
            m_bStopFillAmercing = false;

            LibraryChannel channel = new LibraryChannel();
            channel.Url = Program.MainForm.LibraryServerUrl;

            channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            try
            {
                ClearList(this.listView_overdues);

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(this.FillAmercingParam.Xml);
                }
                catch (Exception ex)
                {
                    strError = "读者XML记录装入XMLDOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                List<string> dup_ids = new List<string>();

                // 选出所有<overdue>元素
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");

                for (int i = 0; i < nodes.Count; i++)
                {
                    if (this.m_bStopFillAmercing == true)
                    {
                        strError = "中断，列表不完整...";
                        goto ERROR1;
                    }

                    XmlNode node = nodes[i];
                    string strItemBarcode = DomUtil.GetAttr(node, "barcode");
                    string strItemRecPath = DomUtil.GetAttr(node, "recPath");
                    string strReason = DomUtil.GetAttr(node, "reason");
                    string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");

                    strBorrowDate = DateTimeUtil.LocalTime(strBorrowDate, "u");

                    string strBorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                    string strReturnDate = DomUtil.GetAttr(node, "returnDate");

                    strReturnDate = DateTimeUtil.LocalTime(strReturnDate, "u");

                    string strID = DomUtil.GetAttr(node, "id");
                    string strPrice = DomUtil.GetAttr(node, "price");
                    string strComment = DomUtil.GetAttr(node, "comment");

                    string strBorrowOperator = DomUtil.GetAttr(node, "borrowOperator");
                    string strReturnOperator = DomUtil.GetAttr(node, "operator");

                    XmlNodeList dup_nodes = dom.DocumentElement.SelectNodes("overdues/overdue[@id='" + strID + "']");
                    if (dup_nodes.Count > 1)
                    {
                        dup_ids.Add(strID);
                    }

                    // TODO: 摘要建议异步作，或者在全部数据装载完成后单独扫描一遍做
                    string strSummary = "";


                    ListViewItem item = new ListViewItem(strItemBarcode);

                    // 摘要
                    // item.SubItems.Add(strSummary);
                    ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_BIBLIOSUMMARY, strSummary);

                    // 金额
                    // item.SubItems.Add(strPrice);
                    ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_PRICE, strPrice);

                    // 注释
                    // item.SubItems.Add(strComment);
                    ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_COMMENT, strComment);

                    // 违约原因
                    // item.SubItems.Add(strReason);
                    ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_REASON, strReason);

                    // 借阅日期
                    // item.SubItems.Add(strBorrowDate);
                    ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_BORROWDATE, strBorrowDate);

                    // 借阅时限
                    // item.SubItems.Add(strBorrowPeriod);
                    ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_BORROWPERIOD, strBorrowPeriod);

                    // 还书日期
                    // item.SubItems.Add(strReturnDate);
                    ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_RETURNDATE, strReturnDate);

                    // id
                    // item.SubItems.Add(strID);
                    ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_ID, strID);

                    ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_BORROWOPERATOR, strBorrowOperator);
                    ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_RETURNOPERATOR, strReturnOperator);

                    // 储存原始价格和注释备用
                    AmercingItemInfo info = new AmercingItemInfo();
                    info.Price = strPrice;
                    info.Comment = strComment;
                    info.Xml = node.OuterXml;
                    item.Tag = info;

                    AddListItem(this.listView_overdues, item);
                }

                /*
                if (dup_ids.Count > 0)
                {
                    StringUtil.RemoveDupNoSort(ref dup_ids);
                    Debug.Assert(dup_ids.Count >= 1, "");
                    strError = "未交费用列表中发现下列ID出现了重复，这是一个严重错误，请系统管理员尽快排除。\r\n---\r\n" + StringUtil.MakePathList(dup_ids, "; ");
                    goto ERROR1;
                }
                */

                // 第二阶段，填充摘要
                if (this.FillAmercingParam.FillSummary == true)
                {
                    List<ListViewItem> items = GetItemList(listView_overdues);

                    for (int i = 0; i < items.Count; i++)
                    {
                        if (this.m_bStopFillAmercing == true)
                            return;

                        ListViewItem item = items[i];

                        Safe_getBarcodeAndSummary(listView_overdues,
        item,
        out string strItemBarcode,
        out string strSummary);

                        // 已经有内容了，就不刷新了
                        if (String.IsNullOrEmpty(strSummary) == false)
                            continue;

                        if (String.IsNullOrEmpty(strItemBarcode) == true
                            /*&& String.IsNullOrEmpty(strItemRecPath) == true*/)
                            continue;

                        try
                        {
                            long lRet = channel.GetBiblioSummary(
                                null,
                                strItemBarcode,
                                "", // strItemRecPath,
                                null,
                                out string strBiblioRecPath,
                                out strSummary,
                                out strError);
                            if (lRet == -1)
                            {
                                strSummary = strError;  // 2009/3/13 changed
                                // return -1;
                            }

                        }
                        finally
                        {
                        }

                        ChangeItemText(item, COLUMN_AMERCING_BIBLIOSUMMARY, strSummary);
                    }
                }

                if (dup_ids.Count > 0)
                {
                    StringUtil.RemoveDupNoSort(ref dup_ids);
                    Debug.Assert(dup_ids.Count >= 1, "");
                    string warning = "未交费用列表中发现下列ID出现了重复，这是一个严重错误，请系统管理员尽快排除。\r\n---\r\n" + StringUtil.MakePathList(dup_ids, "; ");
                    this.ShowMessage(warning, "red", true);    // 2019/9/19
                }

                return;
            }
            finally
            {
                channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
                channel.Close();
                m_bStopFillAmercing = true;
            }

            ERROR1:
            SetError(this.listView_overdues, strError);
            this.ShowMessage(strError, "red", true);    // 2019/9/19
            // Safe_errorBox(strError);
        }

        #endregion

#if NO
        void StopFillSummary()
        {
            // 如果以前在做，立即停止
            m_bStopFilling = true;
        }

        void BeginFillSummary()
        {
            // 如果以前在做，立即停止
            m_bStopFilling = true;

            if (this.threadFillSummary != null)
            {
                this.threadFillSummary.Abort();
                this.threadFillSummary = null;
            }


            this.threadFillSummary =
        new Thread(new ThreadStart(this.ThreadFillSummaryMain));
            this.threadFillSummary.Start();
        }


        public void ThreadFillSummaryMain()
        {
            m_bStopFilling = false;

            LibraryChannel channel = new LibraryChannel();
            channel.Url = Program.MainForm.LibraryServerUrl;

            channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            try
            {

#if NOOOOOOOOO
                Delegate_FillSummary d = new Delegate_FillSummary(FillSummary);
                this.Invoke(d, new object[] { this.listView_overdues,
                channel,
                COLUMN_AMERCING_ITEMBARCODE,
                COLUMN_AMERCING_BIBLIOSUMMARY });


                if (m_bStopFilling == true)
                    return;
#endif

                /*
                FillSummary(
                this.listView_amerced,
                COLUMN_AMERCED_ITEMBARCODE,
                COLUMN_AMERCED_BIBLIOSUMMARY);
                 * */

                Delegate_FillSummary d = new Delegate_FillSummary(FillSummary);
                this.Invoke(d, new object[] { this.listView_amerced,
                channel,
                COLUMN_AMERCED_ITEMBARCODE,
                COLUMN_AMERCED_BIBLIOSUMMARY });
                m_bStopFilling = true;
            }
            finally
            {
                channel.Close();
            }
        }

        delegate void Delegate_FillSummary(ListView list,
            LibraryChannel channel,
            int iColumnBarcode,
            int iColumnSummary);

        void FillSummary(
            ListView list,
            LibraryChannel channel,
            int iColumnBarcode,
            int iColumnSummary)
        {
            string strError = "";

            for (int i = 0; i < list.Items.Count; i++)
            {
                if (m_bStopFilling == true)
                    return;
                
                ListViewItem item = list.Items[i];

                string strSummary = ListViewUtil.GetItemText(item, iColumnSummary);
                string strItemBarcode = ListViewUtil.GetItemText(item, iColumnBarcode);
                // string strItemRecPath = ListViewUtil.GetItemText(item, iColumnSummary);

                if (String.IsNullOrEmpty(strSummary) == false)
                    continue;

                if (String.IsNullOrEmpty(strItemBarcode) == true
                    /*&& String.IsNullOrEmpty(strItemRecPath) == true*/)
                    continue;

                this.stop.SetMessage("正在后台获取摘要 " + strItemBarcode + " ...");

                try
                {

                    string strBiblioRecPath = "";
                    long lRet = channel.GetBiblioSummary(
                        null,
                        strItemBarcode,
                        "", // strItemRecPath,
                        null,
                        out strBiblioRecPath,
                        out strSummary,
                        out strError);
                    if (lRet == -1)
                    {
                        strSummary = strError;  // 2009/3/13 changed
                        // return -1;
                    }

                }
                finally
                {
                }

                ListViewUtil.ChangeItemText(item, iColumnSummary, strSummary);
            }

            this.stop.SetMessage("");
        }
#endif

        // 填充一个新的amerced行
        // stop已经被外层BeginLoop()了
        // TODO: Summary获得时出错，最好作为警告而不是错误。
        int FillAmercedLine(
            Stop stop,
            string strXml,
            string strRecPath,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装载到DOM时发生错误: " + ex.Message;
                return -1;
            }

            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "itemBarcode");
            string strItemRecPath = DomUtil.GetElementText(dom.DocumentElement, "itemRecPath");
            string strSummary = "";
            string strPrice = DomUtil.GetElementText(dom.DocumentElement, "price");
            string strComment = DomUtil.GetElementText(dom.DocumentElement, "comment");
            string strReason = DomUtil.GetElementText(dom.DocumentElement, "reason");
            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement, "borrowDate");

            strBorrowDate = DateTimeUtil.LocalTime(strBorrowDate, "u");

            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement, "borrowPeriod");
            string strReturnDate = DomUtil.GetElementText(dom.DocumentElement, "returnDate");

            strReturnDate = DateTimeUtil.LocalTime(strReturnDate, "u");

            string strID = DomUtil.GetElementText(dom.DocumentElement, "id");
            string strReturnOperator = DomUtil.GetElementText(dom.DocumentElement, "returnOperator");
            string strState = DomUtil.GetElementText(dom.DocumentElement, "state");

            // 2007/6/18
            string strAmerceOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strAmerceTime = DomUtil.GetElementText(dom.DocumentElement, "operTime");

            strAmerceTime = DateTimeUtil.LocalTime(strAmerceTime, "u");

            string strSettlementOperator = DomUtil.GetElementText(dom.DocumentElement, "settlementOperator");
            string strSettlementTime = DomUtil.GetElementText(dom.DocumentElement, "settlementOperTime");

            strSettlementTime = DateTimeUtil.LocalTime(strSettlementTime, "u");

#if NO
            if (String.IsNullOrEmpty(strItemBarcode) == false)
            {
                // stop.OnStop += new StopEventHandler(this.DoStop);
                stop.SetMessage("正在获取摘要 " + strItemBarcode + " ...");
                // stop.BeginLoop();

                try
                {

                    string strBiblioRecPath = "";
                    long lRet = Channel.GetBiblioSummary(
                        stop,
                        strItemBarcode,
                        strItemRecPath,
                        null,
                        out strBiblioRecPath,
                        out strSummary,
                        out strError);
                    if (lRet == -1)
                    {
                        strSummary = strError;
                        // return -1;
                    }

                }
                finally
                {
                    // stop.EndLoop();
                    // stop.OnStop -= new StopEventHandler(this.DoStop);
                    // stop.Initial("");
                }
            }
#endif

            ListViewItem item = new ListViewItem(strItemBarcode, 0);

            /*
            item.SubItems.Add(strSummary);
            item.SubItems.Add(strPrice);
            item.SubItems.Add(strComment);
            item.SubItems.Add(strReason);
            item.SubItems.Add(strBorrowDate);
            item.SubItems.Add(strBorrowPeriod);
            item.SubItems.Add(strReturnDate);
            item.SubItems.Add(strID);
            item.SubItems.Add(strReturnOperator);
            item.SubItems.Add(strState);

            item.SubItems.Add(strAmerceOperator);
            item.SubItems.Add(strAmerceTime);
            item.SubItems.Add(strSettlementOperator);
            item.SubItems.Add(strSettlementTime);

            item.SubItems.Add(strRecPath);
             * */

            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_BIBLIOSUMMARY,
                strSummary);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_PRICE,
                strPrice);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_COMMENT,
                strComment);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_REASON,
                strReason);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_BORROWDATE,
                strBorrowDate);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_BORROWPERIOD,
                strBorrowPeriod);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_RETURNDATE,
                strReturnDate);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_ID,
                strID);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_RETURNOPERATOR,
                strReturnOperator);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_STATE,
                strState);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_AMERCEOPERATOR,
                strAmerceOperator);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_AMERCETIME,
                strAmerceTime);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_SETTLEMENTOPERATOR,
                strSettlementOperator);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_SETTLEMENTTIME,
                strSettlementTime);
            ListViewUtil.ChangeItemText(item,
                COLUMN_AMERCED_RECPATH,
                strRecPath);

            // 2012/10/8
            AmercedItemInfo info = new AmercedItemInfo();
            info.Xml = strXml;
            item.Tag = info;

            this.listView_amerced.Items.Add(item);

            return 0;
        }

#if NOOOOOOOOOOOOOOOOOO
        // 装入读者XML记录
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int LoadReaderXmlRecord(string strBarcode,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("正在装入读者XML记录 " + strBarcode + " ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();


            try
            {

                // ChargingForm.SetHtmlString(this.webBrowser_readerInfo, "(空)");

                long lRet = Channel.GetReaderInfo(
                    stop,
                    strBarcode,
                    "xml",
                    out strXml,
                    out strError);
                if (lRet == 0)
                {
                    strError = "not found";
                    return 0;   // not found
                }
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

#endif



#if NO
        // 旧的、界面线程版本
        // 填充“未交费用”列表
        int FillAmercingList(string strXml,
            out string strError)
        {
            strError = "";

            this.listView_overdues.Items.Clear();

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "读者XML记录装入XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

            List<string> dup_ids = new List<string>();

            // 选出所有<overdue>元素
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strItemBarcode = DomUtil.GetAttr(node, "barcode");
                string strItemRecPath = DomUtil.GetAttr(node, "recPath");
                string strReason = DomUtil.GetAttr(node, "reason");
                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");

                strBorrowDate = DateTimeUtil.LocalTime(strBorrowDate, "u");

                string strBorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strReturnDate = DomUtil.GetAttr(node, "returnDate");

                strReturnDate = DateTimeUtil.LocalTime(strReturnDate, "u");


                string strID = DomUtil.GetAttr(node, "id");
                string strPrice = DomUtil.GetAttr(node, "price");
                string strComment = DomUtil.GetAttr(node, "comment");

                string strBorrowOperator = DomUtil.GetAttr(node, "borrowOperator");
                string strReturnOperator = DomUtil.GetAttr(node, "operator");

                XmlNodeList dup_nodes = dom.DocumentElement.SelectNodes("overdues/overdue[@id='"+strID+"']");
                if (dup_nodes.Count > 1)
                {
                    dup_ids.Add(strID);
                }


                // TODO: 摘要建议异步作，或者在全部数据装载完成后单独扫描一遍做
                string strSummary = "";

#if NOOOOOOOOO
                if (String.IsNullOrEmpty(strItemBarcode) == false)
                {
                    stop.OnStop += new StopEventHandler(this.DoStop);
                    stop.SetMessage("正在获取摘要 " + strItemBarcode + " ...");
                    stop.BeginLoop();

                    try
                    {

                        string strBiblioRecPath = "";
                        long lRet = Channel.GetBiblioSummary(
                            stop,
                            strItemBarcode,
                            strItemRecPath,
                            null,
                            out strBiblioRecPath,
                            out strSummary,
                            out strError);
                        if (lRet == -1)
                        {
                            strSummary = strError;  // 2009/3/13 changed
                            // return -1;
                        }

                    }
                    finally
                    {
                        stop.EndLoop();
                        stop.OnStop -= new StopEventHandler(this.DoStop);
                        stop.Initial("");
                    }
                }
#endif

                ListViewItem item = new ListViewItem(strItemBarcode);

                this.listView_overdues.Items.Add(item);

                // 摘要
                // item.SubItems.Add(strSummary);
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_BIBLIOSUMMARY, strSummary);

                // 金额
                // item.SubItems.Add(strPrice);
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_PRICE, strPrice);

                // 注释
                // item.SubItems.Add(strComment);
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_COMMENT, strComment);

                // 违约原因
                // item.SubItems.Add(strReason);
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_REASON, strReason);

                // 借阅日期
                // item.SubItems.Add(strBorrowDate);
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_BORROWDATE, strBorrowDate);

                // 借阅时限
                // item.SubItems.Add(strBorrowPeriod);
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_BORROWPERIOD, strBorrowPeriod);

                // 还书日期
                // item.SubItems.Add(strReturnDate);
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_RETURNDATE, strReturnDate);

                // id
                // item.SubItems.Add(strID);
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_ID, strID);

                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_BORROWOPERATOR, strBorrowOperator);
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_RETURNOPERATOR, strReturnOperator);

                // 储存原始价格和注释备用
                AmercingItemInfo info = new AmercingItemInfo();
                info.Price = strPrice;
                info.Comment = strComment;
                item.Tag = info;
            }

            if (dup_ids.Count > 0)
            {
                StringUtil.RemoveDupNoSort(ref dup_ids);
                Debug.Assert(dup_ids.Count >= 1, "");
                strError = "未交费用列表中发现下列ID出现了重复，这是一个严重错误，请系统管理员尽快排除。\r\n---\r\n" + StringUtil.MakePathList(dup_ids, "; ");
                MessageBox.Show(this, strError);
            }

            return 0;
        }
#endif

        // checkbox被选择
        private void listView_overdues_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.toolStripLabel_amercingMessage.Text = GetAmercingPriceMessage();

            /*
            string strError = "";
            List<AmerceItem> amerce_items = null;
            int nRet = GetCheckedIdList(this.listView_overdues,
                out amerce_items,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            if (amerce_items == null || amerce_items.Count == 0)
                this.button_amercingOverdue_submit.Enabled = false;
            else
                this.button_amercingOverdue_submit.Enabled = true;
             * */
            SetOverduesButtonsEnable();

            ResetAmercingItemsBackColor(this.listView_overdues);
        }


        // 计算出价格合计值
        static string GetTotalPrice(List<OverdueItemInfo> item_infos)
        {
            List<string> prices = new List<string>();
            for (int i = 0; i < item_infos.Count; i++)
            {
                string strPrice = item_infos[i].Price;

                prices.Add(strPrice);
            }

            return PriceUtil.TotalPrice(prices);
        }

        // 计算出 已交费 价格合计值，构成提示信息
        string GetAmercedPriceMessage()
        {
            List<string> prices = new List<string>();
            int count = 0;
            for (int i = 0; i < this.listView_amerced.Items.Count; i++)
            {
                ListViewItem item = this.listView_amerced.Items[i];
                if (item.Checked == false)
                    continue;

                count++;

                string strPrice = ListViewUtil.GetItemText(item, COLUMN_AMERCED_PRICE);

                // 去掉字符串后面可能有的注释部分
                int nRet = strPrice.IndexOf("|");
                if (nRet != -1)
                    strPrice = strPrice.Substring(0, nRet);

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                prices.Add(strPrice);
            }

            if (count == 0)
                return "尚未选定任何事项。当费用事项被勾选后，按纽才可用 -->";

            return "选中共 " + count.ToString() + " 项, 合计金额: " + PriceUtil.TotalPrice(prices);
        }


        // 计算出 未交费 价格合计值，构成提示信息
        string GetAmercingPriceMessage()
        {
            List<string> prices = new List<string>();
            // double total = 0;
            int count = 0;
            for (int i = 0; i < this.listView_overdues.Items.Count; i++)
            {
                ListViewItem item = this.listView_overdues.Items[i];
                if (item.Checked == false)
                    continue;

                count++;

                string strPrice = ListViewUtil.GetItemText(item, COLUMN_AMERCING_PRICE);    //  this.listView_overdues.Items[i].SubItems[2].Text;

#if NO
                // 去掉字符串后面可能有的注释部分
                int nRet = strPrice.IndexOf("|");
                if (nRet != -1)
                    strPrice = strPrice.Substring(0, nRet);
#endif

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                // 有星号的是变更金额，要去掉星号
                /*
                if (strPrice.Length > 0 && strPrice[0] == '*')
                {
                    strPrice = strPrice.Substring(1);
                }
                 * */
                strPrice = RemoveChangedMask(strPrice);

                /*
                // 提取出纯数字
                string strPurePrice = Global.GetPurePrice(strPrice);

                if (String.IsNullOrEmpty(strPurePrice) == true)
                    continue;

                total += Convert.ToDouble(strPurePrice);
                 * */
                prices.Add(strPrice);
            }
            if (count == 0)
                return "尚未选定任何事项。当费用事项被勾选后，按纽才可用 -->";

            // return "选中共 " + count.ToString() + " 项, 合计金额: " + total.ToString();
            return "选中共 " + count.ToString() + " 项, 合计金额: " + PriceUtil.TotalPrice(prices);
        }

        void SelectAll(ListView listview)
        {
            for (int i = 0; i < listview.Items.Count; i++)
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    // 功能反转 -- Unselect all
                    if (listview.Items[i].Checked == true)
                        listview.Items[i].Checked = false;
                }
                else
                {
                    // 正常功能 -- Select all
                    if (listview.Items[i].Checked == false)
                        listview.Items[i].Checked = true;
                }

                /*
                // TODO: 需要用统一的模块
                if (listview.Items[i].Checked == false)
                {
                    listview.Items[i].BackColor = SystemColors.Window;
                }
                else
                {
                    listview.Items[i].BackColor = Color.Yellow;
                }
                 * */
            }

            if (listview == this.listView_overdues)
                ResetAmercingItemsBackColor(listview);
            else if (listview == this.listView_amerced)
                ResetAmercedItemsBackColor(listview);
            else
            {
                Debug.Assert(false, "未知的listview");
            }
        }


        // 获得显示用的信息
        // parameters:
        int GetCheckedOverdueInfos(ListView listview,
            out List<OverdueItemInfo> overdue_infos,
            out string strError)
        {
            strError = "";
            overdue_infos = new List<OverdueItemInfo>();
            int nCheckedCount = 0;

            // 目前两个listview的id列都还是8
            for (int i = 0; i < listview.Items.Count; i++)
            {
                ListViewItem item = listview.Items[i];
                if (item.Checked == false)
                    continue;

                string strID = "";
                string strPrice = "";
                string strComment = "";
                if (listview == this.listView_amerced)
                {
                    // strID = listview.Items[i].SubItems[8].Text;
                    // strPriceComment = listview.Items[i].SubItems[2].Text;
                    strID = ListViewUtil.GetItemText(item, COLUMN_AMERCED_ID);
                    strPrice = ListViewUtil.GetItemText(item, COLUMN_AMERCED_PRICE);
                    if (string.IsNullOrEmpty(strPrice) == false)
                    {
                        string strResultPrice = "";
                        // 将形如"-123.4+10.55-20.3"的价格字符串反转正负号
                        // parameters:
                        //      bSum    是否要顺便汇总? true表示要汇总
                        int nRet = PriceUtil.NegativePrices(strPrice,
                            false,
                            out strResultPrice,
                            out strError);
                        if (nRet == -1)
                            strPrice = "-(" + strPrice + ")";
                        else
                            strPrice = strResultPrice;
                    }
                    strComment = ListViewUtil.GetItemText(item, COLUMN_AMERCED_COMMENT);
                    if (string.IsNullOrEmpty(strComment) == true)
                        strComment = "撤回交费";
                    else
                        strComment = "撤回交费 (" + strComment + ")";
                }
                else
                {
                    Debug.Assert(listview == this.listView_overdues, "");
                    strID = ListViewUtil.GetItemText(item, COLUMN_AMERCING_ID);
                    strPrice = ListViewUtil.GetItemText(item, COLUMN_AMERCING_PRICE);
                    strComment = ListViewUtil.GetItemText(item, COLUMN_AMERCING_COMMENT);
                }

                if (String.IsNullOrEmpty(strID) == true)
                {
                    strError = "出现了id为空的行。";
                    return -1;
                }

                // 有星号的是变更金额
                strPrice = RemoveChangedMask(strPrice);

                OverdueItemInfo info = new OverdueItemInfo();
                info.Price = strPrice;
                info.ItemBarcode = ListViewUtil.GetItemText(item,
                    COLUMN_AMERCING_ITEMBARCODE);
                info.RecPath = ""; // recPath
                info.Reason = ListViewUtil.GetItemText(item,
                    COLUMN_AMERCING_REASON);

                info.BorrowDate = ListViewUtil.GetItemText(item,
                    COLUMN_AMERCING_BORROWDATE);
                info.BorrowPeriod = ListViewUtil.GetItemText(item,
                    COLUMN_AMERCING_BORROWPERIOD);
                info.ReturnDate = ListViewUtil.GetItemText(item,
                    COLUMN_AMERCING_RETURNDATE);
                info.BorrowOperator = ListViewUtil.GetItemText(item,
                    COLUMN_AMERCING_BORROWOPERATOR);  // borrowOperator
                info.ReturnOperator = ListViewUtil.GetItemText(item,
                    COLUMN_AMERCING_RETURNOPERATOR);    // operator
                info.ID = ListViewUtil.GetItemText(item,
                    COLUMN_AMERCING_ID);

                // 2008/11/15
                info.Comment = strComment;

                overdue_infos.Add(info);

                nCheckedCount++;
            }

            return nCheckedCount;
        }

        // 获得当前窗口中是否有修改后未兑现的状态(是、否)和信息(具体哪些行有修改信息)
        // return:
        //      -1  执行过程发生错误
        //      0   没有修改
        //      >0  修改过，发生过修改的行数。详细信息在strInfo中
        int GetChangedInfo(
            out string strInfo,
            out string strError)
        {
            strError = "";
            strInfo = "";
            int nChangedCount = 0;

            ListView listview = this.listView_overdues;

            // 目前两个listview的id列都还是8

            for (int i = 0; i < listview.Items.Count; i++)
            {
                ListViewItem item = listview.Items[i];

                AmercingItemInfo info = (AmercingItemInfo)item.Tag;
                if (info == null)
                    continue;

                Debug.Assert(info != null, "");

                string strID = ListViewUtil.GetItemText(item, COLUMN_AMERCING_ID);  // listview.Items[i].SubItems[8].Text;
                string strPrice = ListViewUtil.GetItemText(item, COLUMN_AMERCING_PRICE);  // listview.Items[i].SubItems[2].Text;
                string strComment = ListViewUtil.GetItemText(item, COLUMN_AMERCING_COMMENT);

                if (String.IsNullOrEmpty(strID) == true)
                {
                    strError = "出现了id为空的行。";
                    return -1;
                }

                bool bChanged = false;

                string strExistComment = "";
                string strAppendComment = "";
                ParseCommentString(strComment,
                    out strExistComment,
                    out strAppendComment);

                bool bAppendComment = false;
                bool bCommentChanged = false;   // 注释是否发生过修改?

                if (strExistComment != info.Comment)
                    bCommentChanged = true;

                if (string.IsNullOrEmpty(strAppendComment) == false)
                    bAppendComment = true;

                /*
                if (bCommentChanged == true || bAppendComment == true)
                    bChanged = true;
                */

                // 有星号的才是变更金额
                string strNewPrice = "";

                // 只要两个条件具备之一
                if ((strPrice.Length > 0 && strPrice[0] == '*')
                    || (bCommentChanged == true || bAppendComment == true))
                {
                    if (String.IsNullOrEmpty(strInfo) == false)
                        strInfo += ";\r\n";
                    strInfo += "第 " + (i + 1).ToString() + " 行";
                    bChanged = true;
                }

                int nFragmentCount = 0;

                // 第一个条件
                if (strPrice.Length > 0 && strPrice[0] == '*')
                {
                    strNewPrice = strPrice.Substring(1);

                    strInfo += "价格被修改为 " + strNewPrice + " ";
                    nFragmentCount++;
                    /*
                    if (bCommentChanged == true)
                    {
                        if (bAppendComment == false)
                            strInfo += "，并且注释被修改为 '" + strExistComment + "'";
                        else
                            strInfo += "，并且注释要追加内容 '" + strAppendComment + "'";
                    }
                     * */

                }

                // 第二个条件
                if (bCommentChanged == true || bAppendComment == true)
                {
                    if (nFragmentCount > 0)
                        strInfo += ", ";

                    if (bCommentChanged == true)
                    {
                        strInfo += "注释被修改为 '" + strExistComment + "'";
                        if (bAppendComment == true)
                            strInfo += ", ";
                    }

                    if (bAppendComment == true)
                        strInfo += "注释要追加内容 '" + strAppendComment + "'";
                }


                if (bChanged == true)
                    nChangedCount++;
            }

            return nChangedCount;
        }

        // 构造用于提交到dp2library的comment字符串
        static string BuildCommitComment(
            bool bExistCommentChanged,
            string strExistComment,
            bool bAppendCommentChanged,
            string strAppendComment)
        {
            string strResult = "";
            if (bExistCommentChanged)
                strResult += "<" + strExistComment;
            if (bAppendCommentChanged)
                strResult += ">" + strAppendComment;

            return strResult;
        }

        // TODO: 对 ID 进行查重
        // parameters:
        //      strFunction 为"amerce" "modifyprice" "modifycomment" 之一
        int GetCheckedIdList(ListView listview,
            string strFunction,
            out List<AmerceItem> amerce_items,
            out string strError)
        {
            strError = "";
            amerce_items = new List<AmerceItem>();
            // 目前两个listview的id列都还是8

            // ID --> ListViewItem
            Hashtable id_table = new Hashtable();

            for (int i = 0; i < listview.Items.Count; i++)
            {
                ListViewItem item = listview.Items[i];
                if (item.Checked == false)
                    continue;

                // 2019/9/19
                if (item.Tag == null)
                    continue;

                AmercingItemInfo info = null;

                string strID = "";
                string strPrice = "";
                string strComment = "";
                if (listview == this.listView_amerced)
                {
                    // strID = listview.Items[i].SubItems[8].Text;
                    // strPriceComment = listview.Items[i].SubItems[2].Text;
                    strID = ListViewUtil.GetItemText(item, COLUMN_AMERCED_ID);
                    strPrice = ListViewUtil.GetItemText(item, COLUMN_AMERCED_PRICE);
                    strComment = ListViewUtil.GetItemText(item, COLUMN_AMERCED_COMMENT);
                }
                else
                {
                    Debug.Assert(listview == this.listView_overdues, "");
                    strID = ListViewUtil.GetItemText(item, COLUMN_AMERCING_ID);
                    strPrice = ListViewUtil.GetItemText(item, COLUMN_AMERCING_PRICE);
                    strComment = ListViewUtil.GetItemText(item, COLUMN_AMERCING_COMMENT);

                    info = (AmercingItemInfo)item.Tag;
                    Debug.Assert(info != null, "");
                }

                if (String.IsNullOrEmpty(strID) == true)
                {
                    strError = "出现了id为空的行。";
                    return -1;
                }

                // 如果出现了 ID 重复的行
                if (id_table.ContainsKey(strID))
                {
                    // TODO: 如何警告?
                    continue;
                }

                id_table[strID] = item;

                // 有星号的才是变更金额
                string strNewPrice = "";
                if (strPrice.Length > 0 && strPrice[0] == '*')
                    strNewPrice = strPrice.Substring(1);

                if (strFunction == "modifyprice")
                {
                    // 如果价格没有变化，则不列入modifyprice
                    if (strNewPrice == "")
                        continue;
                }

                ParseCommentString(strComment,
                    out string strExistComment,
                    out string strAppendComment);

                bool bAppendComment = false;
                bool bCommentChanged = false;   // 注释是否发生过修改?

                if (info != null)
                {
                    if (strExistComment != info.Comment)
                        bCommentChanged = true;
                }

                if (string.IsNullOrEmpty(strAppendComment) == false)
                    bAppendComment = true;

                AmerceItem amerceItem = new AmerceItem();
                amerceItem.ID = strID;

                if (strFunction == "amerce")
                {
                    amerceItem.NewPrice = strNewPrice;
                    if (bCommentChanged == true || bAppendComment == true)
                        amerceItem.NewComment = BuildCommitComment(
                            bCommentChanged,
                            strExistComment,
                            bAppendComment,
                            strAppendComment);
                }
                else if (strFunction == "modifyprice")
                {
                    amerceItem.NewPrice = strNewPrice;
                    if (bCommentChanged == true || bAppendComment == true)
                        amerceItem.NewComment = BuildCommitComment(
                            bCommentChanged,
                            strExistComment,
                            bAppendComment,
                            strAppendComment);
                }
                else if (strFunction == "modifycomment")
                {
                    if (bCommentChanged == true || bAppendComment == true)
                    {
                    }
                    else
                        continue;

                    // 如果已经算做修改价格，则修改注释的功能已经就达到了，这里不必再做
                    if (String.IsNullOrEmpty(strNewPrice) == false)
                        continue;

                    if (bCommentChanged == true || bAppendComment == true)
                        amerceItem.NewComment = BuildCommitComment(
                            bCommentChanged,
                            strExistComment,
                            bAppendComment,
                            strAppendComment);
                }

                amerce_items.Add(amerceItem);
            }

            return amerce_items.Count;
        }

        void AmerceSubmit()
        {
            int nRet = 0;
            string strError = "";

            // 看看是不是符合服务器要求的交费接口
            if (String.IsNullOrEmpty(Program.MainForm.ClientFineInterfaceName) == false)
            {
                // 注：如果服务器端要配置要求前端不用接口，则应明显配置为“<无>”
                string strThisInterface = this.AmerceInterface;
                if (String.IsNullOrEmpty(strThisInterface) == true)
                    strThisInterface = "<无>";

                if (string.Compare(Program.MainForm.ClientFineInterfaceName, "cardCenter", true) == 0)
                {
                    if (strThisInterface == "<无>")
                    {
                        strError = "应用服务器要求前端必须采用 CardCenter 类型交费接口 '" + Program.MainForm.ClientFineInterfaceName + "'。然而本前端当前配置的交费接口为'" + this.AmerceInterface + "'";
                        goto ERROR1;
                    }

                    // TODO: 是否要排除“迪科远望” 类型 ?
                }
                else if (Program.MainForm.ClientFineInterfaceName != strThisInterface)
                {
                    strError = "应用服务器要求前端必须采用交费接口 '" + Program.MainForm.ClientFineInterfaceName + "'。然而本前端当前配置的交费接口为'" + this.AmerceInterface + "'";
                    goto ERROR1;
                }
            }


            if (this.listView_overdues.CheckedItems.Count == 0)
            {
                strError = "尚未勾选任何要交费的事项";
                goto ERROR1;
            }

            nRet = GetCheckedIdList(this.listView_overdues,
                "amerce",
                out List<AmerceItem> amerce_items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                strError = "所勾选的要交费事项中，没有符合条件的事项";
                goto ERROR1;
            }

            AmerceItem[] amerce_items_param = new AmerceItem[amerce_items.Count];
            amerce_items.CopyTo(amerce_items_param);

            // 显示用
            nRet = GetCheckedOverdueInfos(this.listView_overdues,
                out List<OverdueItemInfo> overdue_infos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0
                || amerce_items == null
                || amerce_items.Count == 0)
            {
                strError = "尚未选择要交费的事项，或者所选择的事项没有有效的id";
                goto ERROR1;
            }

            // 从IC卡扣款
            if (this.AmerceInterface == "迪科远望")
            {
                string strPrice = GetTotalPrice(overdue_infos);
                // return:
                //      -1  error
                //      0   canceled
                //      1   writed
                nRet = WriteDkywCard(
                    amerce_items_param,
                    overdue_infos,
                    this.textBox_readerBarcode.Text,
                    strPrice,
                    out strError);
                if (nRet == 0)
                    return; // 放弃

                if (nRet == -1)
                    goto ERROR1;
            }
            else if (string.IsNullOrEmpty(this.AmerceInterface) == false
                && this.AmerceInterface != "<无>")
            {
                string strPrice = GetTotalPrice(overdue_infos);
                // return:
                //      -1  error
                //      0   canceled
                //      1   writed
                nRet = WriteCardCenter(
                    this.AmerceInterface,
                    amerce_items_param,
                    overdue_infos,
                    this.textBox_readerBarcode.Text,
                    strPrice,
                    out strError);
                if (nRet == 0)
                    return; // 放弃

                if (nRet == -1)
                    goto ERROR1;
            }
            else
            {
                nRet = Submit(amerce_items_param,
                    overdue_infos,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // parameters:
        //      bRefreshAll 是否要重新获取两个列表?
        // return:
        //      -1  error
        //      0   succeed
        //      1   partial succeed (strError中没有信息，中途已经MessageBox()报错了)
        internal int Submit(AmerceItem[] amerce_items,
            List<OverdueItemInfo> overdue_infos,
            bool bRefreshAll,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            bool bPartialSucceed = false;

            // this.StopFillSummary();
            StopFillAmercing(false);
            StopFillAmerced(false);

            DateTime start_time = DateTime.Now;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行 交费 操作: " + this.textBox_readerBarcode.Text + " ...");
            stop.BeginLoop();

            try
            {
                long lRet = Channel.Amerce(
                    stop,
                    "amerce",
                    this.textBox_readerBarcode.Text,
                    amerce_items,
                    out AmerceItem[] failed_items,
                    out string strReaderXml,
                    out strError);
                if (lRet == -1)
                {
                    /*
                    if (this.AmerceInterface == "迪科远望")
                    {
                        string strPrice = GetTotalPrice(overdue_infos);

                        strError += "\r\n\r\n但是当前读者的IC卡已经被扣款 " + strPrice + "。请联系卡中心取消该次IC卡扣款。";
                    }
                     * */
                    goto ERROR1;
                }
                // 部分成功
                if (lRet == 1)
                {
                    bPartialSucceed = true;
                    MessageBox.Show(this, strError);
                    // 只打印成功的部分行
                    if (failed_items != null)
                    {
                        foreach (AmerceItem item in failed_items)
                        {
                            foreach (OverdueItemInfo info in overdue_infos)
                            {
                                if (info.ID == item.ID)
                                {
                                    overdue_infos.Remove(info);
                                    break;
                                }
                            }
                        }
                    }
                }

                DateTime end_time = DateTime.Now;

                string strReaderSummary = "";
                if (String.IsNullOrEmpty(strReaderXml) == false)
                    strReaderSummary = Global.GetReaderSummary(strReaderXml);

                string strAmerceOperator = "";
                if (this.Channel != null)
                    strAmerceOperator = this.Channel.UserName;

                List<string> ids = new List<string>();
                // 为数组中每个元素添加AmerceOperator
                if (overdue_infos != null)
                {
                    foreach (OverdueItemInfo item in overdue_infos)
                    {
                        item.AmerceOperator = this.Channel.UserName;
                        ids.Add(item.ID);
                    }
                }

                Program.MainForm.OperHistory.AmerceAsync(
                    this.textBox_readerBarcode.Text,
                    strReaderSummary,
                    overdue_infos,
                    strAmerceOperator,
                    start_time,
                    end_time);

                if (bRefreshAll == true)
                    ClearAllDisplay();
                else
                    ClearAllDisplay1();

                if (bRefreshAll == true)
                {
#if NO
                    // 刷新列表?
                    nRet = FillAmercingList(strReaderXml,
        out strError);
                    if (nRet == -1)
                    {
                        strError = "FillList()发生错误: " + strError;
                        goto ERROR1;
                    }
#endif
                    BeginFillAmercing(strReaderXml);

                }
                else
                {
                    // 简单去掉那些已经交费的项目
                    foreach (string id in ids)
                    {
                        ListViewItem item = ListViewUtil.FindItem(this.listView_overdues,
                            id,
                            COLUMN_AMERCING_ID);
                        if (item != null)
                            this.listView_overdues.Items.Remove(item);
                    }

                    // 更新选定信息显示 2013/10/26
                    this.toolStripLabel_amercingMessage.Text = GetAmercingPriceMessage();
                }

                string strXml = "";
                string strReaderBarcode = this.textBox_readerBarcode.Text;
                // 刷新html?
                nRet = LoadReaderHtmlRecord(ref strReaderBarcode,
                    out strXml,
                    out strError);
                if (this.textBox_readerBarcode.Text != strReaderBarcode)
                    this.textBox_readerBarcode.Text = strReaderBarcode;

                if (nRet == -1)
                {
#if NO
                    Global.SetHtmlString(this.webBrowser_readerInfo,
                        "装载读者记录发生错误: " + strError);
#endif
                    this.m_webExternalHost.SetTextString("装载读者记录发生错误: " + strError);
                    goto ERROR1;
                }

                if (bRefreshAll == true)
                {
#if NO
                    // 刷新amerced
                    nRet = LoadAmercedRecords(this.textBox_readerBarcode.Text,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "LoadAmercedRecords()发生错误: " + strError;
                        goto ERROR1;
                    }
#endif
                    BeginFillAmerced(this.textBox_readerBarcode.Text, null);

                }
                else
                {
#if NO
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   找到并填入
                    nRet = LoadAmercedRecords(
                        ids,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "LoadAmercedRecords(ids)发生错误: " + strError;
                        goto ERROR1;
                    }
#endif
                    BeginFillAmerced("", ids);
                }

#if NO
                if (this.checkBox_fillSummary.Checked == true)
                    this.BeginFillSummary();
#endif

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            if (bPartialSucceed == true)
                return 1;

            return 0;
            ERROR1:
            return -1;
        }

        // 回滚
        // return:
        //      -2  回滚成功，但是刷新显示失败
        //      -1  回滚失败
        //      0   成功
        internal int RollBack(out string strError)
        {
            // this.StopFillSummary();
            StopFillAmercing(false);
            StopFillAmerced(false);

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行 回滚 操作");
            stop.BeginLoop();

            try
            {
                AmerceItem[] failed_items = null;

                string strReaderXml = "";
                int nRet = (int)Channel.Amerce(
                    stop,
                    "rollback",
                    "", // strReaderBarcode,
                    null,   // amerce_items,
                    out failed_items,
                    out strReaderXml,
                    out strError);
                if (nRet == -1 || nRet == 1)
                    return -1;

                ClearAllDisplay();

#if NO
                // 刷新列表?
                nRet = FillAmercingList(strReaderXml,
    out strError);
                if (nRet == -1)
                {
                    strError = "FillList()发生错误: " + strError;
                    return -2;
                }
#endif
                BeginFillAmercing(strReaderXml);


                string strXml = "";
                string strReaderBarcode = this.textBox_readerBarcode.Text;
                // 刷新html?
                nRet = LoadReaderHtmlRecord(ref strReaderBarcode,
                    out strXml,
                    out strError);
                if (this.textBox_readerBarcode.Text != strReaderBarcode)
                    this.textBox_readerBarcode.Text = strReaderBarcode;
                if (nRet == -1)
                {
#if NO
                    Global.SetHtmlString(this.webBrowser_readerInfo,
                        "装载读者记录发生错误: " + strError);
#endif
                    this.m_webExternalHost.SetTextString("装载读者记录发生错误: " + strError);
                    return -2;
                }

#if NO
                // 刷新amerced
                nRet = LoadAmercedRecords(this.textBox_readerBarcode.Text,
                    out strError);
                if (nRet == -1)
                {
                    strError = "LoadAmercedRecords()发生错误: " + strError;
                    return -2;
                }
#endif
                BeginFillAmerced(this.textBox_readerBarcode.Text, null);

#if NO
                if (this.checkBox_fillSummary.Checked == true)
                    this.BeginFillSummary();
#endif
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

        // 利用卡中心接口进行扣款操作
        // return:
        //      -1  error
        //      0   canceled
        //      1   writed
        int WriteCardCenter(
            string strUrl,
            AmerceItem[] AmerceItems,
            List<OverdueItemInfo> OverdueInfos,
            string strReaderBarcode,
            string strPrice,
            out string strError)
        {
            strError = "";

            AmerceCardDialog dlg = new AmerceCardDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.InterfaceUrl = strUrl;
            dlg.AmerceForm = this;
            dlg.AmerceItems = AmerceItems;
            dlg.OverdueInfos = OverdueInfos;
            dlg.CardNumber = strReaderBarcode;
            dlg.SubmitPrice = strPrice; //  PriceUtil.GetPurePrice(strPrice); // 是否要去除货币单位?
            dlg.StartPosition = FormStartPosition.CenterScreen;
            Program.MainForm.AppInfo.LinkFormState(dlg, "AmerceCardDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            return 1;
        }

        // return:
        //      -1  error
        //      0   canceled
        //      1   writed
        int WriteDkywCard(
            AmerceItem[] AmerceItems,
            List<OverdueItemInfo> OverdueInfos,
            string strReaderBarcode,
            string strPrice,
            out string strError)
        {
            strError = "";

            DkywAmerceCardDialog dlg = new DkywAmerceCardDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.AmerceForm = this;
            dlg.AmerceItems = AmerceItems;
            dlg.OverdueInfos = OverdueInfos;
            dlg.CardNumber = strReaderBarcode;
            dlg.SubmitPrice = PriceUtil.GetPurePrice(strPrice); // 是否要去除货币单位?
            dlg.StartPosition = FormStartPosition.CenterScreen;
            Program.MainForm.AppInfo.LinkFormState(dlg, "AmerceCardDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            return 1;
        }


        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.textBox_readerBarcode.Enabled = bEnable;
            this.button_load.Enabled = bEnable;

            this.listView_overdues.Enabled = bEnable;
            this.toolStripButton_amercing_selectAll.Enabled = bEnable;

            /*
            this.button_amercingOverdue_submit.Enabled = bEnable;
            this.button_amercingOverdue_modifyPrice.Enabled = bEnable;
             * */
            if (bEnable == false)
            {
                this.toolStripButton_submit.Enabled = false;
                this.toolStripButton_modifyPriceAndComment.Enabled = false;
            }
            else
            {
                SetOverduesButtonsEnable();
            }

            this.listView_amerced.Enabled = bEnable;
            this.toolStripButton_amerced_selectAll.Enabled = bEnable;

            if (bEnable == false)
            {
                this.toolStripButton_undoAmerce.Enabled = false;
            }
            else
            {
                SetAmercedButtonsEnable();
            }

        }



        // return:
        //      -1  error
        //      0   succeed
        //      1   partial succeed
        int UndoAmerce()
        {
            int nRet = 0;
            string strError = "";
            bool bPartialSucceed = false;

            // this.StopFillSummary();
            StopFillAmercing(false);
            StopFillAmerced(false);


            if (this.listView_amerced.CheckedItems.Count == 0)
            {
                strError = "尚未选择要撤回交费的事项";
                goto ERROR1;
            }

            // 2013/12/20
            // 显示用
            List<OverdueItemInfo> overdue_infos = null;
            nRet = GetCheckedOverdueInfos(this.listView_amerced,
                out overdue_infos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            List<AmerceItem> amerce_items = null;
            nRet = GetCheckedIdList(this.listView_amerced,
                "amerce",
                out amerce_items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0
                || amerce_items == null
                || amerce_items.Count == 0)
            {
                strError = "所选择的要撤回交费的事项，没有有效的id";
                goto ERROR1;
            }

            EnableControls(false);

            DateTime start_time = DateTime.Now;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行 撤回交费 操作: " + this.textBox_readerBarcode.Text + " ...");
            stop.BeginLoop();

            try
            {
                string strReaderXml = "";

                AmerceItem[] amerce_items_param = new AmerceItem[amerce_items.Count];
                amerce_items.CopyTo(amerce_items_param);

                AmerceItem[] failed_items = null;

                long lRet = Channel.Amerce(
                    stop,
                    "undo",
                    this.textBox_readerBarcode.Text,
                    amerce_items_param,
                    out failed_items,
                    out strReaderXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                // 部分成功
                if (lRet == 1)
                {
                    bPartialSucceed = true;
                    MessageBox.Show(this, strError);
                }

                DateTime end_time = DateTime.Now;

                {
                    // 只打印成功的部分行
                    if (failed_items != null)
                    {
                        foreach (AmerceItem item in failed_items)
                        {
                            foreach (OverdueItemInfo info in overdue_infos)
                            {
                                if (info.ID == item.ID)
                                {
                                    overdue_infos.Remove(info);
                                    break;
                                }
                            }
                        }
                    }

                }

                string strReaderSummary = "";
                if (String.IsNullOrEmpty(strReaderXml) == false)
                    strReaderSummary = Global.GetReaderSummary(strReaderXml);

                string strAmerceOperator = "";
                if (this.Channel != null)
                    strAmerceOperator = this.Channel.UserName;

                List<string> ids = new List<string>();
                // 为数组中每个元素添加AmerceOperator
                if (overdue_infos != null)
                {
                    foreach (OverdueItemInfo item in overdue_infos)
                    {
                        item.AmerceOperator = this.Channel.UserName;
                        ids.Add(item.ID);
                    }
                }

                Program.MainForm.OperHistory.AmerceAsync(
                    this.textBox_readerBarcode.Text,
                    strReaderSummary,
                    overdue_infos,
                    strAmerceOperator,
                    start_time,
                    end_time);

                ClearAllDisplay();

#if NO
                // 刷新列表?
                nRet = FillAmercingList(strReaderXml,
    out strError);
                if (nRet == -1)
                {
                    strError = "FillList()发生错误: " + strError;
                    goto ERROR1;
                }
#endif
                BeginFillAmercing(strReaderXml);


                string strXml = "";
                string strReaderBarcode = this.textBox_readerBarcode.Text;
                // 刷新html?
                nRet = LoadReaderHtmlRecord(ref strReaderBarcode,
                    out strXml,
                    out strError);
                if (this.textBox_readerBarcode.Text != strReaderBarcode)
                    this.textBox_readerBarcode.Text = strReaderBarcode;
                if (nRet == -1)
                {
#if NO
                    Global.SetHtmlString(this.webBrowser_readerInfo,
                        "装载读者记录发生错误: " + strError);
#endif
                    this.m_webExternalHost.SetTextString("装载读者记录发生错误: " + strError);
                    goto ERROR1;
                }

#if NO
                // 刷新amerced
                nRet = LoadAmercedRecords(this.textBox_readerBarcode.Text,
                    out strError);
                if (nRet == -1)
                {
                    strError = "LoadAmercedRecords()发生错误: " + strError;
                    goto ERROR1;
                }
#endif
                BeginFillAmerced(this.textBox_readerBarcode.Text, null);

#if NO
                if (this.checkBox_fillSummary.Checked == true)
                    this.BeginFillSummary();
#endif

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            if (bPartialSucceed == true)
                return 1;

            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        private void listView_amerced_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.toolStripLabel_amercedMessage.Text = GetAmercedPriceMessage();

            /*
            string strError = "";
            List<AmerceItem> amerce_items = null;
            int nRet = GetCheckedIdList(this.listView_amerced,
                out amerce_items,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }


            if (amerce_items == null || amerce_items.Count == 0)
                this.button_amerced_undoAmerce.Enabled = false;
            else
                this.button_amerced_undoAmerce.Enabled = true;
            */
            this.SetAmercedButtonsEnable();

            ResetAmercedItemsBackColor(this.listView_amerced);
        }

        static void ResetAmercingItemsBackColor(ListView list)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];

                if (item.Checked == false)
                    item.BackColor = SystemColors.Window;
                else
                    item.BackColor = Color.Yellow;
            }
        }

        static void ResetAmercedItemsBackColor(ListView list)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];

                if (item.Checked == false)
                {
                    string strState = ListViewUtil.GetItemText(item,
COLUMN_AMERCED_STATE);
                    if (strState == "settlemented")
                    {
                        item.ForeColor = SystemColors.GrayText;
                        item.ImageIndex = ITEMTYPE_OLD_SETTLEMENTED;    // 全部都是以前settlemented的
                    }
                    else if (strState == "error")
                    {
                        item.ForeColor = SystemColors.GrayText;
                        item.ImageIndex = ITEMTYPE_ERROR;    // 出错的行
                    }
                    else
                    {
                        item.ForeColor = SystemColors.WindowText;
                        item.ImageIndex = ITEMTYPE_AMERCED;
                    }

                    item.BackColor = SystemColors.Window;
                }
                else
                    item.BackColor = Color.Yellow;
            }
        }

        private void textBox_readerBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_load;
            Program.MainForm.EnterPatronIdEdit(InputType.PQR);
        }

        private void textBox_readerBarcode_Leave(object sender, EventArgs e)
        {
            this.AcceptButton = null;
            Program.MainForm.LeavePatronIdEdit();
        }

        private void AmerceForm_Activated(object sender, EventArgs e)
        {
            Program.MainForm.stopManager.Active(this.stop);

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

        // 右鼠标键菜单
        private void listView_overdues_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("察看记录详情(&V)");
            menuItem.Click += new System.EventHandler(this.menu_viewAmercing_Click);
            if (this.listView_overdues.SelectedItems.Count != 1)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("变更金额(&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyPrice_Click);
            if (this.listView_overdues.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("变更注释(&C)");
            menuItem.Click += new System.EventHandler(this.menu_modifyComment_Click);
            if (this.listView_overdues.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_overdues, new Point(e.X, e.Y));
        }

        void menu_viewAmercing_Click(object sender, EventArgs e)
        {
            DoViewOperlog(true);
        }


        // 将价格和注释合成为一个字符串
        // 注意没有给前方加上*符号
        static string MergePriceCommentString(string strPrice,
            string strNewComment)
        {
            if (String.IsNullOrEmpty(strNewComment) == false)
                return strPrice + "|" + strNewComment;

            return strPrice;
        }

#if NO
        // 从合成的(价格+注释)字符串中析出两个部分
        // 并且去掉价格字符串头部的*符号
        // 2007/4/19
        static void ParsePriceCommentString(string strText,
            bool bClearChangedChar,
            out string strPrice,
            out string strComment)
        {
            strPrice = "";
            strComment = "";

            int nRet = strText.IndexOf("|");
            if (nRet != -1)
            {
                strComment = strText.Substring(nRet + 1);
                strPrice = strText.Substring(0, nRet);
            }
            else
            {
                strPrice = strText;
            }

            if (bClearChangedChar == true)
            {
                // 去掉价格字符串头部的*符号
                if (strPrice.Length > 0 && strPrice[0] == '*')
                    strPrice = strPrice.Substring(1);
                // 去掉注释字符串头部的<或者>符号
                if (strComment.Length > 0 &&
                    (strComment[0] == '<' || strComment[0] == '>'))
                    strComment = strComment.Substring(1);

            }

        }

#endif

        // 修改一个事项的金额
        void menu_modifyPrice_Click(object sender, EventArgs e)
        {
            if (this.listView_overdues.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要变更金额的事项。");
                return;
            }

            if (this.listView_overdues.SelectedIndices.Count > 1)
            {
                MessageBox.Show(this, "每次只能选定一项来变更金额。");
                return;
            }

            int index = this.listView_overdues.SelectedIndices[0];
            ListViewItem item = listView_overdues.Items[index];

            AmercingItemInfo info = (AmercingItemInfo)item.Tag;
            Debug.Assert(info != null, "");

            string strID = ListViewUtil.GetItemText(item, COLUMN_AMERCING_ID);  // .SubItems[8].Text;


#if NO
            string strPriceComment = ListViewUtil.GetItemText(item, COLUMN_AMERCING_PRICE);  // listView_overdues.Items[index].SubItems[2].Text;


            // 从strPrice字符串中析出注释字符串
            // 2007/4/19 changed
            string strNewComment = "";
            string strPrice = "";

            /*
            int nRet = strPrice.IndexOf("|");
            if (nRet != -1)
            {
                strComment = strPrice.Substring(nRet + 1);
                strPrice = strPrice.Substring(0, nRet);
            }

            // 去掉价格字符串头部的*符号
            if (strPrice.Length > 0 && strPrice[0] == '*')
                strPrice = strPrice.Substring(1);
             * */
            ParsePriceCommentString(strPriceComment,
                true,
                out strPrice,
                out strNewComment);
#endif
            string strComment = ListViewUtil.GetItemText(item, COLUMN_AMERCING_COMMENT); // listView_overdues.Items[index].SubItems[3].Text;

            string strExistComment = "";
            string strAppendComment = "";
            ParseCommentString(strComment,
                out strExistComment,
                out strAppendComment);

            ModifyPriceDlg dlg = new ModifyPriceDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.ID = strID;
            dlg.OldPrice = RemoveChangedMask(ListViewUtil.GetItemText(item, COLUMN_AMERCING_PRICE)); // strPrice;
            dlg.Price = RemoveChangedMask(ListViewUtil.GetItemText(item, COLUMN_AMERCING_PRICE));
            dlg.Comment = strExistComment;
            dlg.AppendComment = strAppendComment;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

#if NO
            // 将价格和注释合成为一个字符串
            string strNewPrice = "";

            if (strPrice != dlg.Price)
                strNewPrice = "*" + dlg.Price;
            else
                strNewPrice = dlg.Price;

            string strNewText = MergePriceCommentString(strNewPrice,
                 ">"+dlg.NewComment);

            // listView_overdues.Items[index].SubItems[2].Text = strNewText;
            ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_PRICE, strNewText);
#endif
            if (info.Price != dlg.Price)
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_PRICE, "*" + dlg.Price);
            else
                ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_PRICE, dlg.Price);

            if (info.Comment != dlg.Comment)
                strComment = "<" + dlg.Comment;
            else
                strComment = dlg.Comment;

            if (string.IsNullOrEmpty(dlg.AppendComment) == false)
                strComment += ">" + dlg.AppendComment;    // TODO: 似乎不能允许既修改也追加

            ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_COMMENT, strComment);
            item.Checked = true;    // 顺便勾选上这个事项
        }

        // 去掉价格字符串前方表示修改的*字符
        static string RemoveChangedMask(string strPrice)
        {
            // 有星号的是变更金额，要去掉星号
            if (strPrice.Length > 0 && strPrice[0] == '*')
            {
                return strPrice.Substring(1);
            }

            return strPrice;
        }

        // 解析listview栏目中的注释字符串
        // parameters:
        //      strText 待解析的字符串。为 <comment>appendcomment 或者 <comment 或者 comment 或者 >appendcomment
        static void ParseCommentString(string strText,
            out string strComment,
            out string strAppendComment)
        {
            strComment = "";
            strAppendComment = "";

            int nRet = strText.IndexOf(">");
            if (nRet == -1)
                strComment = strText;
            else
            {
                strComment = strText.Substring(0, nRet);
                strAppendComment = strText.Substring(nRet + 1);
            }

            if (strComment.Length > 0 && strComment[0] == '<')
                strComment = strComment.Substring(1);
        }

        void menu_modifyComment_Click(object sender, EventArgs e)
        {
            if (this.listView_overdues.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要变更注释的事项。");
                return;
            }

            if (this.listView_overdues.SelectedIndices.Count > 1)
            {
                MessageBox.Show(this, "每次只能选定一项来变更注释。");
                return;
            }

            int index = this.listView_overdues.SelectedIndices[0];
            ListViewItem item = this.listView_overdues.Items[index];

            AmercingItemInfo info = (AmercingItemInfo)item.Tag;
            Debug.Assert(info != null, "");

            string strID = ListViewUtil.GetItemText(item, COLUMN_AMERCING_ID);  // listView_overdues.Items[index].SubItems[8].Text;

#if NO
            string strPriceComment = ListViewUtil.GetItemText(item, COLUMN_AMERCING_PRICE); // listView_overdues.Items[index].SubItems[2].Text;
            string strOldComment = ListViewUtil.GetItemText(item, COLUMN_AMERCING_COMMENT); // listView_overdues.Items[index].SubItems[3].Text;

            // 从strPrice字符串中析出注释字符串
            string strNewComment = "";
            string strPrice = "";

            ParsePriceCommentString(strPriceComment,
                false,
                out strPrice,
                out strNewComment);

            bool bAppend = true;

            // 根本没有修改过。证据是：strNewComment中没有任何符号；而修改过是有符号的
            if (String.IsNullOrEmpty(strNewComment) == true)
            {
                bAppend = true; // 2008/6/24 new changed
                strNewComment = ""; // 2008/6/24 new changed
            }
            else if (String.IsNullOrEmpty(strNewComment) == false
                && strNewComment[0] == '<')
            {
                bAppend = false;
                strNewComment = strNewComment.Substring(1);
            }
            else if (String.IsNullOrEmpty(strNewComment) == false
                && strNewComment[0] == '>')
            {
                bAppend = true;
                strNewComment = strNewComment.Substring(1);
            }
#endif

            bool bAppend = true;
            string strComment = ListViewUtil.GetItemText(item, COLUMN_AMERCING_COMMENT); // listView_overdues.Items[index].SubItems[3].Text;

            if (String.IsNullOrEmpty(strComment) == false
                && strComment[0] == '<')
            {
                bAppend = false;
            }
            else if (String.IsNullOrEmpty(strComment) == false
                && strComment[0] == '>')
            {
                bAppend = true;
            }

            string strExistComment = "";
            string strAppendComment = "";
            ParseCommentString(strComment,
                out strExistComment,
                out strAppendComment);

            ModifyCommentDialog dlg = new ModifyCommentDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.ID = strID;
            dlg.Price = RemoveChangedMask(ListViewUtil.GetItemText(item, COLUMN_AMERCING_PRICE));
            dlg.IsAppend = bAppend;
            dlg.OriginOldComment = info.Comment;

            dlg.Comment = strExistComment;
            dlg.AppendComment = strAppendComment;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

#if NO
            // 将价格和注释合成为一个字符串
            string strNewText = "";

            bAppend = dlg.IsAppend;
            
            if (bAppend == true)
                strNewText = MergePriceCommentString(strPrice,
                    ">" + dlg.AppendComment);
            else
                strNewText = MergePriceCommentString(strPrice,
                    "<" + dlg.Comment);

            // listView_overdues.Items[index].SubItems[2].Text = strNewText;
            ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_PRICE, strNewText);
#endif

            if (info.Comment != dlg.Comment)
                strComment = "<" + dlg.Comment;
            else
                strComment = dlg.Comment;

            if (string.IsNullOrEmpty(dlg.AppendComment) == false)
                strComment += ">" + dlg.AppendComment;    // TODO: 似乎不能允许既修改也追加

            ListViewUtil.ChangeItemText(item, COLUMN_AMERCING_COMMENT, strComment);
            item.Checked = true;    // 顺便勾选上这个事项
        }



        // 只修改金额和注释
        // return:
        //      -1  error
        //      0   succeed
        //      1   partial succeed
        int ModifyPriceAndComment()
        {
            int nRet = 0;
            string strError = "";
            bool bPartialSucceed = false;

            if (this.listView_overdues.CheckedItems.Count == 0)
            {
                strError = "尚未勾选任何事项";
                goto ERROR1;
            }

            // this.StopFillSummary();
            StopFillAmercing(false);
            StopFillAmerced(false);

            List<AmerceItem> modifyprice_items = null;
            nRet = GetCheckedIdList(this.listView_overdues,
                "modifyprice",
                out modifyprice_items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            Debug.Assert(modifyprice_items != null, "");

            List<AmerceItem> modifycomment_items = null;
            nRet = GetCheckedIdList(this.listView_overdues,
                "modifycomment",
                out modifycomment_items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            Debug.Assert(modifycomment_items != null, "");

            /*
            List<AmerceItem> appendcomment_items = null;
            nRet = GetCheckedIdList(this.listView_overdues,
                "appendcomment",
                out appendcomment_items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            Debug.Assert(appendcomment_items != null, "");

             * */
            if (modifyprice_items.Count + modifycomment_items.Count/* + appendcomment_items.Count */ == 0)
            {
                strError = "所勾选的事项中，没有任何发生过价格或注释追加/修改的事项";
                goto ERROR1;
            }

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行 修改金额/注释 的操作: " + this.textBox_readerBarcode.Text + " ...");
            stop.BeginLoop();

            try
            {
                string strReaderXml = "";

                if (modifyprice_items.Count > 0)
                {
                    AmerceItem[] amerce_items_param = new AmerceItem[modifyprice_items.Count];
                    modifyprice_items.CopyTo(amerce_items_param);
                    AmerceItem[] failed_items = null;

                    long lRet = Channel.Amerce(
                        stop,
                        "modifyprice",
                        this.textBox_readerBarcode.Text,
                        amerce_items_param,
                        out failed_items,
                        out strReaderXml,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                    // 部分成功
                    if (lRet == 1)
                    {
                        bPartialSucceed = true;
                        MessageBox.Show(this, strError);
                    }
                }

                /*
                if (appendcomment_items.Count > 0)
                {
                    AmerceItem[] amerce_items_param = new AmerceItem[appendcomment_items.Count];
                    appendcomment_items.CopyTo(amerce_items_param);

                    long lRet = Channel.Amerce(
                        stop,
                        "appendcomment",
                        this.textBox_readerBarcode.Text,
                        amerce_items_param,
                        out strReaderXml,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }*/

                // 需要的权限高一些，因此放在后面
                if (modifycomment_items.Count > 0)
                {
                    AmerceItem[] amerce_items_param = new AmerceItem[modifycomment_items.Count];
                    modifycomment_items.CopyTo(amerce_items_param);
                    AmerceItem[] failed_items = null;

                    long lRet = Channel.Amerce(
                        stop,
                        "modifycomment",
                        this.textBox_readerBarcode.Text,
                        amerce_items_param,
                        out failed_items,
                        out strReaderXml,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                ClearHtmlAndAmercingDisplay();

#if NO
                // 刷新列表?
                nRet = FillAmercingList(strReaderXml,
    out strError);
                if (nRet == -1)
                {
                    strError = "FillList()发生错误: " + strError;
                    goto ERROR1;
                }
#endif
                BeginFillAmercing(strReaderXml);


                string strXml = "";
                string strReaderBarcode = this.textBox_readerBarcode.Text;
                // 刷新html?
                nRet = LoadReaderHtmlRecord(ref strReaderBarcode,
                    out strXml,
                    out strError);
                if (this.textBox_readerBarcode.Text != strReaderBarcode)
                    this.textBox_readerBarcode.Text = strReaderBarcode;
                if (nRet == -1)
                {
#if NO
                    Global.SetHtmlString(this.webBrowser_readerInfo,
                        "装载读者记录发生错误: " + strError);
#endif
                    this.m_webExternalHost.SetTextString("装载读者记录发生错误: " + strError);
                    goto ERROR1;
                }

                /*
                // 刷新amerced
                nRet = LoadAmercedRecords(this.textBox_readerBarcode.Text,
                    out strError);
                if (nRet == -1)
                {
                    strError = "LoadAmercedRecords()发生错误: " + strError;
                    goto ERROR1;
                }
                 * */

#if NO
                if (this.checkBox_fillSummary.Checked == true)
                    this.BeginFillSummary();
#endif

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            if (bPartialSucceed == true)
                return 1;
            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        void SetOverduesButtonsEnable()
        {
            if (this.listView_overdues.CheckedItems.Count == 0)
            {
                this.toolStripButton_submit.Enabled = false;
                this.toolStripButton_modifyPriceAndComment.Enabled = false;
            }
            else
            {
                this.toolStripButton_submit.Enabled = true;
                this.toolStripButton_modifyPriceAndComment.Enabled = true;
            }

        }

        void SetAmercedButtonsEnable()
        {
            if (this.listView_amerced.CheckedItems.Count == 0)
                this.toolStripButton_undoAmerce.Enabled = false;
            else
                this.toolStripButton_undoAmerce.Enabled = true;
        }

        private void listView_overdues_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoViewOperlog(false);
        }

        private void listView_amerced_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoViewOperlog(false);
        }


        // 
        /// <summary>
        /// 打印借还、违约金凭条
        /// </summary>
        public void Print()
        {
            // 触发历史动作
            Program.MainForm.OperHistory.Print();
        }

        /// <summary>
        /// 交费接口配置字符串
        /// </summary>
        public string AmerceInterface
        {
            get
            {
                // amerce
                return Program.MainForm.AppInfo.GetString("config",
                    "amerce_interface",
                    "<无>");
            }
        }

        private void listView_amerced_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_amerced, e);
        }

        private void listView_overdues_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_overdues, e);
        }

        private void button_beginFillSummary_Click(object sender, EventArgs e)
        {
            // BeginFillSummary();
        }

        private void checkBox_fillSummary_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_fillSummary.Checked == true)
            {
                // BeginFillSummary();
            }
        }

        private void webBrowser_readerInfo_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error -= new HtmlElementErrorEventHandler(Window_Error);
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }

        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            if (Program.MainForm.SuppressScriptErrors == true)
                e.Handled = true;
        }

        void DoViewOperlog(bool bOpenWindow)
        {
            string strError = "";
            string strHtml = "";
            string strXml = "";
            int nRet = 0;

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (Program.MainForm.PanelFixedVisible == false
                    && (m_operlogViewer == null || m_operlogViewer.Visible == false))
                    return;
            }

            ListView list = null;
            if (this.listView_amerced.Focused == true)
                list = this.listView_amerced;
            else if (this.listView_overdues.Focused == true)
                list = this.listView_overdues;
            else
                list = null;

            if (list == null || list.SelectedItems.Count != 1)
            {
                // 2012/10/2
                if (this.m_operlogViewer != null)
                    this.m_operlogViewer.Clear();

                return;
            }

            ListViewItem item = list.SelectedItems[0];
            string strTitle = "";
            {

                // 创建解释事项内容的 HTML 字符串
                nRet = GetHtmlString(item,
                    out strTitle,
                    out strHtml,
                    out strXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            bool bNew = false;
            if (this.m_operlogViewer == null
                || (bOpenWindow == true && this.m_operlogViewer.Visible == false))
            {
                m_operlogViewer = new CommentViewerForm();
                MainForm.SetControlFont(m_operlogViewer, this.Font, false);
                m_operlogViewer.SuppressScriptErrors = Program.MainForm.SuppressScriptErrors;
                bNew = true;
            }

            // m_operlogViewer.MainForm = Program.MainForm;  // 必须是第一句

            if (bNew == true)
                m_operlogViewer.InitialWebBrowser();

            m_operlogViewer.Text = strTitle;
            m_operlogViewer.HtmlString = (string.IsNullOrEmpty(strHtml) == true ? NOTSUPPORT : strHtml);
            m_operlogViewer.XmlString = strXml;
            m_operlogViewer.FormClosed -= new FormClosedEventHandler(m_viewer_FormClosed);
            m_operlogViewer.FormClosed += new FormClosedEventHandler(m_viewer_FormClosed);

            if (bOpenWindow == true)
            {
                if (m_operlogViewer.Visible == false)
                {
                    Program.MainForm.AppInfo.LinkFormState(m_operlogViewer, "operlog_viewer_state");
                    m_operlogViewer.Show(this);
                    m_operlogViewer.Activate();

                    Program.MainForm.CurrentPropertyControl = null;
                }
                else
                {
                    if (m_operlogViewer.WindowState == FormWindowState.Minimized)
                        m_operlogViewer.WindowState = FormWindowState.Normal;
                    m_operlogViewer.Activate();
                }
            }
            else
            {
                if (m_operlogViewer.Visible == true)
                {

                }
                else
                {
                    if (Program.MainForm.CurrentPropertyControl != m_operlogViewer.MainControl)
                        m_operlogViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
            return;
            ERROR1:
            MessageBox.Show(this, "DoViewOperlog() 出错: " + strError);
        }

        void m_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_operlogViewer != null)
            {
                Program.MainForm.AppInfo.UnlinkFormState(m_operlogViewer);
                this.m_operlogViewer = null;
            }
        }

        int GetHtmlString(ListViewItem item,
                    out string strTitle,
                    out string strHtml,
                    out string strXml,
                    out string strError)
        {
            strTitle = "";
            strHtml = "";
            strXml = "";
            strError = "";
            int nRet = 0;

            if (item.ListView == this.listView_amerced)
            {
                strTitle = "已交费记录 " + ListViewUtil.GetItemText(item, COLUMN_AMERCED_ID);

                // 已交费
                AmercedItemInfo info = (AmercedItemInfo)item.Tag;
                if (info == null)
                    return 0;
                strXml = info.Xml;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "违约金记录XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                nRet = GetAmerceInfoString(dom, out strHtml, out strError);

                if (nRet == -1)
                    return -1;

                {
                    if (string.IsNullOrEmpty(strHtml) == true)
                        return 0;
                    strHtml = "<html>" +
                        GetHeadString() +
                        "<body>" +
                        strHtml +
                        "</body></html>";
                }
            }
            else
            {
                Debug.Assert(item.ListView == this.listView_overdues, "");

                strTitle = "待交费事项 " + ListViewUtil.GetItemText(item, COLUMN_AMERCING_ID);

                // 尚未交费
                AmercingItemInfo info = (AmercingItemInfo)item.Tag;
                if (info == null)
                    return 0;
                strXml = info.Xml;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "待交费信息XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                nRet = GetOverdueInfoString(dom.DocumentElement,
                    true,
                    out strHtml, out strError);

                if (nRet == -1)
                    return -1;

                {
                    if (string.IsNullOrEmpty(strHtml) == true)
                        return 0;
                    strHtml = "<html>" +
                        GetHeadString() +
                        "<body>" +
                        strHtml +
                        "</body></html>";
                }

            }

            return 0;
        }

        string GetHeadString(bool bAjax = true)
        {
            string strCssFilePath = PathUtil.MergePath(Program.MainForm.DataDir, "amercehtml.css");

            if (bAjax == true)
                return
                    "<head>" +
                    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
                    "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>" +
                    //"<script type='text/javascript' src='%datadir%/jquery.js'></script>" +
                    "<script type='text/javascript' charset='UTF-8' src='%datadir%\\getsummary.js" + "'></script>" +
                    "</head>";
            return
    "<head>" +
    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
    "</head>";

        }

        // 获得<overdue>元素对应的 HTML 字符串。不包括外面的<html><body>
        /// <summary>
        /// 获得 overdue 元素对应的 HTML 字符串
        /// </summary>
        /// <param name="root">XML 根节点 XmlNode 对象</param>
        /// <param name="bSummary">是否产生书目摘要</param>
        /// <param name="strHtml">返回 HTML 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int GetOverdueInfoString(XmlNode root,
            bool bSummary,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            string strItemBarcode = DomUtil.GetAttr(root, "barcode");
            string strItemRecPath = DomUtil.GetAttr(root, "recPath");
            string strReason = DomUtil.GetAttr(root, "reason");
            string strOverduePeriod = DomUtil.GetAttr(root, "overduePeriod");
            string strPrice = DomUtil.GetAttr(root, "price");
            string strBorrowDate = OperLogForm.GetRfc1123DisplayString(
                DomUtil.GetAttr(root, "borrowDate"));
            string strBorrowPeriod = DomUtil.GetAttr(root, "borrowPeriod");
            string strBorrowOperator = DomUtil.GetAttr(root, "borrowOperator");


            string strReturnDate = OperLogForm.GetRfc1123DisplayString(
    DomUtil.GetAttr(root, "returnDate"));
            string strReturnOperator = DomUtil.GetAttr(root, "operator");

            string strID = DomUtil.GetAttr(root, "id");

            string strComment = DomUtil.GetAttr(root, "comment");

            strHtml =
                "<table class='overdueinfo'>" +
                OperLogForm.BuildHtmlEncodedLine("册条码号", OperLogForm.BuildItemBarcodeLink(strItemBarcode)) +
                (bSummary == true ? OperLogForm.BuildHtmlPendingLine("(书目摘要)", BuildPendingBiblioSummary(strItemBarcode, "")) : "") +
                OperLogForm.BuildHtmlLine("ID", strID) +
                OperLogForm.BuildHtmlLine("原因", strReason) +
                OperLogForm.BuildHtmlLine("超期", strOverduePeriod) +
                OperLogForm.BuildHtmlLine("金额", strPrice) +
                OperLogForm.BuildHtmlLine("注释", strComment) +

                OperLogForm.BuildHtmlLine("起点操作者", strBorrowOperator) +
                OperLogForm.BuildHtmlLine("起点日期", strBorrowDate) +
                OperLogForm.BuildHtmlLine("期限", strBorrowPeriod) +

                OperLogForm.BuildHtmlLine("终点操作者", strReturnOperator) +
                OperLogForm.BuildHtmlLine("终点日期", strReturnDate) +

                "</table>";

            return 0;
        }

        // 获得违约金记录 HTML 字符串。不包括外面的<html><body>
        static int GetAmerceInfoString(XmlDocument amerce_dom,
out string strHtml,
out string strError)
        {
            strHtml = "";
            strError = "";

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(amerce_dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strItemBarcode = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "itemBarcode");
            string strReaderBarcode = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "readerBarcode");
            string strState = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "state");
            string strID = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "id");
            string strReason = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "reason");
            string strOverduePeriod = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "overduePeriod");

            string strPrice = DomUtil.GetElementInnerXml(amerce_dom.DocumentElement, "price");
            string strComment = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "comment");

            string strBorrowDate = OperLogForm.GetRfc1123DisplayString(
                DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "borrowDate"));
            string strBorrowPeriod = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "borrowPeriod");
            string strBorrowOperator = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "borrowOperator");
            string strReturnDate = OperLogForm.GetRfc1123DisplayString(
    DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "returnDate"));
            string strReturnOperator = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "returnOperator");

            string strOperator = DomUtil.GetElementText(amerce_dom.DocumentElement, "operator");
            string strOperTime = OperLogForm.GetRfc1123DisplayString(
                DomUtil.GetElementText(amerce_dom.DocumentElement, "operTime"));

            string strSettlementOperator = DomUtil.GetElementText(amerce_dom.DocumentElement, "settlementOperator");
            string strSettlementOperTime = OperLogForm.GetRfc1123DisplayString(
                DomUtil.GetElementText(amerce_dom.DocumentElement, "settlementOperTime"));

            string strUndoSettlementOperator = DomUtil.GetElementText(amerce_dom.DocumentElement, "undoSettlementOperator");
            string strUndoSettlementOperTime = OperLogForm.GetRfc1123DisplayString(
                DomUtil.GetElementText(amerce_dom.DocumentElement, "undoSettlementOperTime"));

            strHtml =
                "<table class='amerceinfo'>" +
                OperLogForm.BuildHtmlLine("馆代码", strLibraryCode) +
                OperLogForm.BuildHtmlEncodedLine("册条码号", OperLogForm.BuildItemBarcodeLink(strItemBarcode)) +
                OperLogForm.BuildHtmlPendingLine("(书目摘要)", BuildPendingBiblioSummary(strItemBarcode, "")) +
                OperLogForm.BuildHtmlEncodedLine("读者证条码号", OperLogForm.BuildReaderBarcodeLink(strReaderBarcode)) +
                OperLogForm.BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +
                OperLogForm.BuildHtmlLine("状态", strState) +
                OperLogForm.BuildHtmlLine("ID", strID) +
                OperLogForm.BuildHtmlLine("原因", strReason) +
                OperLogForm.BuildHtmlLine("超期", strOverduePeriod) +
                OperLogForm.BuildHtmlLine("金额", strPrice) +
                OperLogForm.BuildHtmlLine("注释", strComment) +

                OperLogForm.BuildHtmlLine("起点操作者", strBorrowOperator) +
                OperLogForm.BuildHtmlLine("起点日期", strBorrowDate) +
                OperLogForm.BuildHtmlLine("期限", strBorrowPeriod) +

                OperLogForm.BuildHtmlLine("终点操作者", strReturnOperator) +
                OperLogForm.BuildHtmlLine("终点日期", strReturnDate) +

                OperLogForm.BuildHtmlLine("收取违约金操作者", strOperator) +
                OperLogForm.BuildHtmlLine("收取违约金操作时间", strOperTime) +

                OperLogForm.BuildHtmlLine("结算操作者", strSettlementOperator) +
                OperLogForm.BuildHtmlLine("结算操作时间", strSettlementOperTime) +

                OperLogForm.BuildHtmlLine("撤销结算操作者", strUndoSettlementOperator) +
                OperLogForm.BuildHtmlLine("撤销结算操作时间", strUndoSettlementOperTime) +
                "</table>";

            return 0;
        }

        static string BuildPendingBiblioSummary(string strItemBarcode,
    string strItemRecPath)
        {
            if (string.IsNullOrEmpty(strItemBarcode) == true)
                return "";
            string strCommand = "B:" + strItemBarcode + "|" + strItemRecPath;
            return strCommand;
        }

        static string BuildPendingReaderSummary(string strReaderBarcode)
        {
            if (string.IsNullOrEmpty(strReaderBarcode) == true)
                return "";
            string strCommand = "P:" + strReaderBarcode;
            return strCommand;
        }

        private void listView_amerced_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("察看记录详情(&V)");
            menuItem.Click += new System.EventHandler(this.menu_viewAmercing_Click);
            if (this.listView_amerced.SelectedItems.Count != 1)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_amerced, new Point(e.X, e.Y));
        }

        private void toolStripButton_amerced_selectAll_Click(object sender, EventArgs e)
        {
            SelectAll(this.listView_amerced);
        }

        private void toolStripButton_undoAmerce_Click(object sender, EventArgs e)
        {
            this.toolStripButton_undoAmerce.Enabled = false;

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_UNDO_AMERCE);
        }

        private void toolStripButton_amercing_selectAll_Click(object sender, EventArgs e)
        {
            SelectAll(this.listView_overdues);

        }

        // 只修改金额和注释
        private void toolStripButton_modifyPriceAndComment_Click(object sender, EventArgs e)
        {
            this.toolStripButton_modifyPriceAndComment.Enabled = false;

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_MODIFY_PRICE_AND_COMMENT);

        }

        private void toolStripButton_submit_Click(object sender, EventArgs e)
        {
            this.toolStripButton_submit.Enabled = false;

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_AMERCE);
        }
    }

    class AmercingItemInfo
    {
        public string Price = "";
        public string Comment = "";
        public string Xml = ""; // 相关node的OuterXml，也就是<overdue>元素XML片断
    }

    class AmercedItemInfo
    {
        public string Xml = ""; // 违约金记录XML
    }

    class FillAmercingParam
    {
        public string Xml = ""; // [in]读者记录
        public bool FillSummary = true;
    }

    class FillAmercedParam
    {
        public string ReaderBarcode = "";
        public List<string> IDs = null;
        public bool FillSummary = true;
    }
}