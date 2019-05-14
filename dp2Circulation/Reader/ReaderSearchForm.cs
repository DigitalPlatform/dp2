using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Web;
using System.Reflection;

using ClosedXML.Excel;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.ResultSet;
using DigitalPlatform.Interfaces;
using DigitalPlatform.Marc;
using DigitalPlatform.Script;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.LibraryServer;
using static dp2Circulation.ReaderInfoForm;
using DigitalPlatform.Core;

namespace dp2Circulation
{
    /// <summary>
    /// 读者查询窗
    /// </summary>
    public partial class ReaderSearchForm : SearchFormBase
    {
        /// <summary>
        /// 是否为指纹模式？如果为指纹模式，需要采用代理帐户登录
        /// </summary>
        public bool FingerPrintMode = false;    // 是否为指纹模式？如果为指纹模式，需要采用代理帐户登录

        /*
        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();
         * */

        /// <summary>
        /// 最近用过的输出条码号文件名
        /// </summary>
        public string ExportBarcodeFilename = "";

        /// <summary>
        /// 最近用过的输出记录路径文件名
        /// </summary>
        public string ExportRecPathFilename = "";

        string m_strUsedRecPathFilename = "";
        string m_strUsedBarcodeFilename = "";

        /// <summary>
        /// 浏览列表 ListView
        /// </summary>
        public ListView ListViewRecords
        {
            get
            {
                return this.listView_records;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReaderSearchForm()
        {
            this.DbType = "patron";

            InitializeComponent();

            _listviewRecords = this.listView_records;

            ListViewProperty prop = new ListViewProperty();
            this.listView_records.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);
        }

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            if (e.DbName == "<blank>")
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                e.ColumnTitles.Add("检索点");
                e.ColumnTitles.Add("数量");
                return;
            }

            e.ColumnTitles = Program.MainForm.GetBrowseColumnProperties(e.DbName);
        }

        // 在状态行显示文字信息
        internal override void SetStatusMessage(string strMessage)
        {
            this.label_message.Text = strMessage;
        }


        private void ReaderSearchForm_Load(object sender, EventArgs e)
        {
            this.comboBox_readerDbName.Text = Program.MainForm.AppInfo.GetString(
                "readersearchform",
                "readerdbname",
                "<全部>");

            this.comboBox_from.Text = Program.MainForm.AppInfo.GetString(
                "readersearchform",
                "from",
                "");

            this.comboBox_matchStyle.Text = Program.MainForm.AppInfo.GetString(
                "readersearchform",
                "match_style",
                "前方一致");

            bool bHideMatchStyle = Program.MainForm.AppInfo.GetBoolean(
                "reader_search_form",
                "hide_matchstyle",
                false);

            if (bHideMatchStyle == true)
            {
                this.label_matchStyle.Visible = false;
                this.comboBox_matchStyle.Visible = false;
                this.comboBox_matchStyle.Text = "前方一致"; // 隐藏后，采用缺省值
            }

            string strWidths = Program.MainForm.AppInfo.GetString(
                "readersearchform",
                "record_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_records,
                    strWidths,
                    true);
            }

            comboBox_matchStyle_TextChanged(null, null);

            if (Program.MainForm.ReaderDbFromInfos != null)
            {
                FillReaderDbFroms();
            }
        }

        /// <summary>
        /// 重载的 Channel_BeforeLogin()
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        public override void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (this.FingerPrintMode == false)
                base.Channel_BeforeLogin(this, e);
            else
            {
                if (string.IsNullOrEmpty(Program.MainForm.FingerprintUserName) == false
                    && Program.MainForm.FingerprintUserName != Program.MainForm.DefaultUserName)
                    MyBeforeLogin(this, e);
                else
                    base.Channel_BeforeLogin(this, e);
            }
        }

        void MyBeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            // 只有当代理帐户有密码的时候，才进行第一次试探
            if (e.FirstTry == true && string.IsNullOrEmpty(Program.MainForm.FingerprintPassword) == false)
            {
                e.UserName = Program.MainForm.FingerprintUserName;
                e.Password = Program.MainForm.FingerprintPassword;

                bool bIsReader = false; // 工作人员方式

                string strLocation = Program.MainForm.AppInfo.GetString(
                    "default_account",
                    "location",
                    "");    // 工作台号和缺省帐户一致
                e.Parameters = "location=" + strLocation;
                if (bIsReader == true)
                    e.Parameters += ",type=reader";

                e.Parameters += ",client=dp2circulation|" + Program.ClientVersion;
                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // 
            IWin32Window owner = null;

            if (sender is IWin32Window)
                owner = (IWin32Window)sender;
            else
                owner = this;

            string strComment = "为初始化指纹缓存，需要用户 " + Program.MainForm.FingerprintUserName + " 亲自进行登录";

            CirculationLoginDlg dlg = SetFingerprintAccount(
                e.LibraryServerUrl,
                strComment,
                string.IsNullOrEmpty(e.ErrorInfo) == false ? e.ErrorInfo : strComment,
                e.LoginFailCondition,
                owner);
            if (dlg == null)
            {
                e.Cancel = true;
                return;
            }

            e.UserName = dlg.UserName;
            e.Password = dlg.Password;
            e.SavePasswordShort = dlg.SavePasswordShort;
            e.Parameters = "location=" + dlg.OperLocation;
            if (dlg.IsReader == true)
                e.Parameters += ",type=reader";

            e.Parameters += ",client=dp2circulation|" + Program.ClientVersion;

            e.SavePasswordLong = dlg.SavePasswordLong;
            e.LibraryServerUrl = dlg.ServerUrl;
        }

        // parameters:
        CirculationLoginDlg SetFingerprintAccount(
            string strServerUrl,
            string strTitle,
            string strComment,
            LoginFailCondition fail_contidion,
            IWin32Window owner)
        {
            CirculationLoginDlg dlg = new CirculationLoginDlg();
            MainForm.SetControlFont(dlg, Program.MainForm.DefaultFont);

            if (String.IsNullOrEmpty(strServerUrl) == true)
            {
                dlg.ServerUrl =
        Program.MainForm.AppInfo.GetString("config",
        "circulation_server_url",
        "http://localhost:8001/dp2library");
            }
            else
            {
                dlg.ServerUrl = strServerUrl;
            }

            if (owner == null)
                owner = this;

            if (String.IsNullOrEmpty(strTitle) == false)
                dlg.Text = strTitle;

            dlg.Comment = strComment;
            dlg.UserName = Program.MainForm.FingerprintUserName;

            dlg.IsReaderEnabled = false;

            dlg.SavePasswordShort = false;
            dlg.SavePasswordShortEnabled = false;

            dlg.SavePasswordLong = false;

            dlg.Password = Program.MainForm.FingerprintPassword;

            dlg.IsReader = false;
            dlg.OperLocation = Program.MainForm.AppInfo.GetString(
                "default_account",
                "location",
                "");

            Program.MainForm.AppInfo.LinkFormState(dlg,
                "logindlg_state");

            dlg.ShowDialog(owner);

            Program.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult == DialogResult.Cancel)
            {
                return null;
            }

            Program.MainForm.FingerprintUserName = dlg.UserName;

            if (dlg.SavePasswordLong == true)
                Program.MainForm.FingerprintPassword = dlg.Password;

            // server url的修改不要记忆

            return dlg;
        }

        void FillReaderDbFroms()
        {
            this.comboBox_from.Items.Clear();
            this.comboBox_from.Items.Add("<全部>");   // 2013/5/24
            for (int i = 0; i < Program.MainForm.ReaderDbFromInfos.Length; i++)
            {
                string strCaption = Program.MainForm.ReaderDbFromInfos[i].Caption;
                this.comboBox_from.Items.Add(strCaption);
            }
        }

        private void ReaderSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }

            }*/


        }

        private void ReaderSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetString(
                    "readersearchform",
                    "readerdbname",
                    this.comboBox_readerDbName.Text);

                Program.MainForm.AppInfo.SetString(
                    "readersearchform",
                    "from",
                    this.comboBox_from.Text);

                Program.MainForm.AppInfo.SetString(
                    "readersearchform",
                    "match_style",
                    this.comboBox_matchStyle.Text);

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_records);
                Program.MainForm.AppInfo.SetString(
                    "readersearchform",
                    "record_list_column_width",
                    strWidths);
            }
        }

        /// <summary>
        /// 检索命中的最大记录数限制参数。-1 表示不限制
        /// </summary>
        public int MaxSearchResultCount
        {
            get
            {
                return (int)Program.MainForm.AppInfo.GetInt(
                    "reader_search_form",
                    "max_result_count",
                    -1);
            }
        }

        // 是否以推动的方式装入浏览列表
        // 2008/1/20 
        /// <summary>
        /// 是否以推动的方式装入浏览列表
        /// </summary>
        public bool PushFillingBrowse
        {
            get
            {
                return MainForm.AppInfo.GetBoolean(
                    "reader_search_form",
                    "push_filling_browse",
                    false);
            }
        }

        string GetCurrentMatchStyle()
        {
            string strText = this.comboBox_matchStyle.Text;

            // 2009/8/6 
            if (strText == "空值")
                return "null";

            if (String.IsNullOrEmpty(strText) == true)
                return "left"; // 缺省时认为是 前方一致

            if (strText == "前方一致")
                return "left";
            if (strText == "中间一致")
                return "middle";
            if (strText == "后方一致")
                return "right";
            if (strText == "精确一致")
                return "exact";

            return strText; // 直接返回原文
        }

        // 检索
        private void toolStripButton_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            {
                if (this.m_nChangedCount > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "当前命中记录列表中有 " + this.m_nChangedCount.ToString() + " 项修改尚未保存。\r\n\r\n是否继续操作?\r\n\r\n(Yes 清除，然后继续操作；No 放弃操作)",
                        "ReaderSearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                }
                this.ClearListViewItems();

                ListViewUtil.ClearSortColumns(this.listView_records);
            }
            /*
            this.listView_records.Items.Clear();
            ListViewUtil.ClearSortColumns(this.listView_records);
             * */
            ClearListViewItems();

            this.label_message.Text = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 '" + this.textBox_queryWord.Text + "'...");
            stop.BeginLoop();

            EnableControls(false);
            try
            {
                string strMatchStyle = GetCurrentMatchStyle();

                if (this.textBox_queryWord.Text == "")
                {
                    if (strMatchStyle == "null")
                    {
                        this.textBox_queryWord.Text = "";

                        // 专门检索空值
                        strMatchStyle = "exact";
                    }
                    else
                    {
                        // 为了在检索词为空的时候，检索出全部的记录
                        strMatchStyle = "left";
                    }
                }
                else
                {
                    // 2012/3/31
                    if (strMatchStyle == "null")
                    {
                        strError = "检索空值的时候，请保持检索词为空";
                        goto ERROR1;
                    }
                }

                long lRet = Channel.SearchReader(stop,
                    this.comboBox_readerDbName.Text,
                    this.textBox_queryWord.Text,
                    this.MaxSearchResultCount, // -1,
                    this.comboBox_from.Text,
                    strMatchStyle,  // "left",
                    this.Lang,
                    null,   // strResultSetName
                    "", // strOutputStyle
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条读者记录";

                stop.SetProgressRange(0, lHitCount);

                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                bool bPushFillingBrowse = this.PushFillingBrowse;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条读者记录，已装入 " + lStart.ToString() + " 条，用户中断...";
                        MessageBox.Show(this, "用户中断");
                        return;
                    }

                    lRet = Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条读者记录，已装入 " + lStart.ToString() + " 条，" + strError;
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        MessageBox.Show(this, "未命中");
                        return;
                    }

                    Debug.Assert(searchresults != null, "");
                    Debug.Assert(searchresults.Length > 0, "");

                    // 处理浏览结果
                    this.listView_records.BeginUpdate();
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        if (bPushFillingBrowse == true)
                            Global.InsertNewLine(this.listView_records,
                                searchresults[i].Path,
                                searchresults[i].Cols);
                        else
                            Global.AppendNewLine(this.listView_records,
                                searchresults[i].Path,
                                searchresults[i].Cols);
                    }
                    this.listView_records.EndUpdate();

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                    stop.SetProgressValue(lStart);
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
                this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条读者记录，已全部装入";
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }


        /// <summary>
        /// 当前窗口查询的数据库类型，用于显示的名称形态
        /// </summary>
        public override string DbTypeCaption
        {
            get
            {
                Debug.Assert(this.DbType == "patron", "");
                return "读者";
            }
        }

        // 获得一条记录
        //return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        internal override int GetRecord(
            LibraryChannel channel,
            string strRecPath,
            out string strXml,
            out byte[] baTimestamp,
            out string strError)
        {
            strError = "";
            strXml = "";

            string[] results = null;
            baTimestamp = null;
            string strOutputRecPath = "";
            // 获得读者记录
            long lRet = channel.GetReaderInfo(
stop,
"@path:" + strRecPath,
"xml",
out results,
out strOutputRecPath,
out baTimestamp,
out strError);

            if (lRet == 0)
                return 0;  // 是否设定为特殊状态?
            if (lRet == -1)
                return -1;

            if (results == null || results.Length == 0)
            {
                strError = "results error";
                return -1;
            }

            strXml = results[0];
            return 1;
        }

        // bool _forceSave = false;

        // return:
        //      -2  时间戳不匹配
        //      -1  出错
        //      0   成功
        internal override int SaveRecord(
            LibraryChannel channel,
            string strRecPath,
            BiblioInfo info,
            string strStyle,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";

            ErrorCodeValue kernel_errorcode;
            string strOutputPath = "";

            baNewTimestamp = null;
            string strExistingXml = "";
            string strSavedXml = "";
            long lRet = channel.SetReaderInfo(
stop,
StringUtil.IsInList("force", strStyle) ? "changereaderbarcode" : "change",
strRecPath,
info.NewXml,
info.OldXml,
info.Timestamp,
out strExistingXml,
out strSavedXml,
out strOutputPath,
out baNewTimestamp,
out kernel_errorcode,
out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ErrorCode.TimestampMismatch)
                    return -2;
                return -1;
            }

            info.Timestamp = baNewTimestamp;    // 2013/10/17
            return 0;
        }

        private void textBox_queryWord_Enter(object sender, EventArgs e)
        {
            // this.AcceptButton = this.button_search;
        }

        private void listView_records_Enter(object sender, EventArgs e)
        {
            // this.AcceptButton = null;
        }

        /// <summary>
        /// 是否优先装入已经打开的详细窗
        /// </summary>
        public bool LoadToExistDetailWindow
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "all_search_form",
                    "load_to_exist_detailwindow",
                    true);
            }
        }

        // (根据读者证条码号)把读者记录装入读者窗
        private void listView_records_DoubleClick(object sender, EventArgs e)
        {
            string strOpenStyle = "new";
            if (this.LoadToExistDetailWindow == true)
                strOpenStyle = "exist";

            /*
            LoadRecordToReaderInfoForm(strOpenStyle,
                "barcode"); // 双击这里故意用barcode方式，就是为了警告重复的读者证条码号
             * */
            LoadRecordToReaderInfoForm(strOpenStyle,
                "auto"); // 双击这里故意用auto或barcode方式，就是为了警告重复的读者证条码号
        }

        // 将记录装载到读者窗
        // parameters:
        //      strIdType   标识类型 "barcode" "recpath" "auto"
        //      strOpenStyle 打开窗口的方式 "new" "exist"
        void LoadRecordToReaderInfoForm(string strOpenStyle,
            string strIdType)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装入读者窗的事项");
                return;
            }

            string strBarcodeOrRecPath = "";

            if (strIdType == "auto")
            {
                strBarcodeOrRecPath = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);  // this.listView_records.SelectedItems[0].SubItems[1].Text;

                // 如果条码号为空
                if (String.IsNullOrEmpty(strBarcodeOrRecPath) == true)
                {
                    strBarcodeOrRecPath = this.listView_records.SelectedItems[0].SubItems[0].Text;
                    strIdType = "recpath";
                }
                else
                {
                    strIdType = "barcode";
                }

                Debug.Assert(strIdType != "auto", "auto类型后面不能用了");
            }
            else if (strIdType == "barcode")
                strBarcodeOrRecPath = this.listView_records.SelectedItems[0].SubItems[1].Text;
            else
            {
                Debug.Assert(strIdType == "recpath", "");
                strBarcodeOrRecPath = this.listView_records.SelectedItems[0].SubItems[0].Text;
            }

            ReaderInfoForm form = null;

            if (strOpenStyle == "exist")
            {
                form = MainForm.GetTopChildWindow<ReaderInfoForm>();
                if (form != null)
                    Global.Activate(form);
            }

            if (form == null)
            {
                form = new ReaderInfoForm();

                form.MdiParent = Program.MainForm;

                form.MainForm = Program.MainForm;
                form.Show();
            }

            if (strIdType == "barcode")
            {
                form.LoadRecord(strBarcodeOrRecPath, false); // 发生重复条码时，不强行装入，起到警告工作人员的作用
                // form.AsyncLoadRecord(strBarcodeOrRecPath);
            }
            else
            {
                Debug.Assert(strIdType == "recpath", "");

                // form.LoadRecord("@path:" + strRecPath, false);   // 这个办法有问题，ReaderInfoForm.ReaderBarcode有误
                form.LoadRecordByRecPath(strBarcodeOrRecPath, "");
            }
        }

        // 将记录装载到交费窗
        // parameters:
        //      strIdType   标识类型 "barcode" "recpath"
        //      strOpenStyle 打开窗口的方式 "new" "exist"
        void LoadRecordToAmerceForm(string strOpenStyle,
            string strIdType)
        {

            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装入交费窗的事项");
                return;
            }

            string strBarcodeOrRecPath = "";

            if (strIdType == "barcode")
                strBarcodeOrRecPath = this.listView_records.SelectedItems[0].SubItems[1].Text;
            else
            {
                Debug.Assert(strIdType == "recpath", "");
                strBarcodeOrRecPath = this.listView_records.SelectedItems[0].SubItems[0].Text;
            }

            AmerceForm form = null;

            if (strOpenStyle == "exist")
            {
                form = MainForm.GetTopChildWindow<AmerceForm>();
                if (form != null)
                    Global.Activate(form);
            }

            if (form == null)
            {
                form = new AmerceForm();

                form.MdiParent = Program.MainForm;

                form.MainForm = Program.MainForm;
                form.Show();
            }

            if (strIdType == "barcode")
                form.LoadReader(strBarcodeOrRecPath, false); // 发生重复条码时，不强行装入，起到警告工作人员的作用
            else
            {
                Debug.Assert(strIdType == "recpath", "");

                Debug.Assert(false, "目前尚未支持");
                form.LoadReader("@path:" + strBarcodeOrRecPath, false);   // 凑合的，尚未测试
                // form.LoadRecordByRecPath(strBarcodeOrRecPath, "");
            }
        }


        // 装入读者窗 1
        void menu_recPath_newly_Click(object sender, EventArgs e)
        {
            LoadRecordToReaderInfoForm("new",
                "recpath");
        }

        // 2
        void menu_barcode_newly_Click(object sender, EventArgs e)
        {
            LoadRecordToReaderInfoForm("new",
                "barcode");
        }

        // 3
        void menu_recPath_exist_Click(object sender, EventArgs e)
        {
            LoadRecordToReaderInfoForm("exist",
                "recpath");
        }

        // 4
        void menu_barcode_exist_Click(object sender, EventArgs e)
        {
            LoadRecordToReaderInfoForm("exist",
                "barcode");
        }

        // 装入交费窗 1
        void menu_amerce_by_barcode_newly_Click(object sender, EventArgs e)
        {
            LoadRecordToAmerceForm("new",
                "barcode");
        }

        // 装入交费窗 2
        void menu_amerce_by_barcode_exist_Click(object sender, EventArgs e)
        {
            LoadRecordToAmerceForm("exist",
                "barcode");
        }

        // 注:不包括listview
        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.toolStrip_search.Enabled = bEnable;
            this.comboBox_readerDbName.Enabled = bEnable;
            this.comboBox_from.Enabled = bEnable;
            this.comboBox_matchStyle.Enabled = bEnable;

            if (this.comboBox_matchStyle.Text == "空值")
                this.textBox_queryWord.Enabled = false;
            else
                this.textBox_queryWord.Enabled = bEnable;
        }

        private void ReaderSearchForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            // Program.MainForm.MenuItem_font.Enabled = false;
        }

        private void comboBox_readerDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_readerDbName.Items.Count > 0)
                return;

            this.comboBox_readerDbName.Items.Add("<全部>");

            if (Program.MainForm.ReaderDbNames != null)    // 2009/3/29 
            {
                for (int i = 0; i < Program.MainForm.ReaderDbNames.Length; i++)
                {
                    this.comboBox_readerDbName.Items.Add(Program.MainForm.ReaderDbNames[i]);
                }
            }
        }

        private void listView_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            // bool bSearching = !this.textBox_queryWord.Enabled;
            bool bSearching = this.InSearching;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            ListViewItem selected_item = null;

            string strBarcode = "";
            string strRecPath = "";

            if (this.listView_records.SelectedItems.Count > 0)
            {
                selected_item = this.listView_records.SelectedItems[0];
                strBarcode = ListViewUtil.GetItemText(selected_item, 1);
                strRecPath = ListViewUtil.GetItemText(selected_item, 0);
            }

            string strOpenStyle = "新开的";
            if (this.LoadToExistDetailWindow == true)
                strOpenStyle = "已打开的";

            menuItem = new MenuItem("打开 [根据证条码号 '" + strBarcode + "' 装入" + strOpenStyle + "读者窗] (&O)");
            menuItem.DefaultItem = true;
            menuItem.Click += new System.EventHandler(this.listView_records_DoubleClick);
            if (String.IsNullOrEmpty(strBarcode) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            {
                //
                menuItem = new MenuItem("打开方式(&T)");
                contextMenu.MenuItems.Add(menuItem);

                // 子菜单

                // *** 读者窗
                strOpenStyle = "新开的";

                // 记录路径
                MenuItem subMenuItem = new MenuItem("装入" + strOpenStyle + "读者窗，根据记录路径 '" + strRecPath + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_recPath_newly_Click);
                if (String.IsNullOrEmpty(strRecPath) == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 条码
                subMenuItem = new MenuItem("装入" + strOpenStyle + "读者窗，根据证条码号 '" + strBarcode + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_barcode_newly_Click);
                if (String.IsNullOrEmpty(strBarcode) == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                strOpenStyle = "已打开的";

                bool bHasOpendReaderInfoForm = (Program.MainForm.GetTopChildWindow<ReaderInfoForm>() != null);

                // 记录路径
                subMenuItem = new MenuItem("装入" + strOpenStyle + "读者窗，根据记录路径 '" + strRecPath + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_recPath_exist_Click);
                if (String.IsNullOrEmpty(strRecPath) == true
                    || bHasOpendReaderInfoForm == false)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 条码
                subMenuItem = new MenuItem("装入" + strOpenStyle + "读者窗，根据证条码号 '" + strBarcode + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_barcode_exist_Click);
                if (String.IsNullOrEmpty(strBarcode) == true
                    || bHasOpendReaderInfoForm == false)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                /*
                menuItem = new MenuItem("打开 [根据记录路径 '"+strRecPath+"' 装入到读者窗] (&P)");
                menuItem.Click += new System.EventHandler(this.menu_loadReaderInfoByRecPath_Click);
                if (String.IsNullOrEmpty(strRecPath) == true)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
                 * */

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);


                // *** 交费窗
                strOpenStyle = "新开的";

                // 条码
                subMenuItem = new MenuItem("装入" + strOpenStyle + "交费窗，根据证条码号 '" + strBarcode + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_amerce_by_barcode_newly_Click);
                if (String.IsNullOrEmpty(strBarcode) == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                strOpenStyle = "已打开的";

                // 条码
                subMenuItem = new MenuItem("装入" + strOpenStyle + "交费窗，根据证条码号 '" + strBarcode + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_amerce_by_barcode_exist_Click);
                if (String.IsNullOrEmpty(strBarcode) == true
                    || Program.MainForm.GetTopChildWindow<AmerceForm>() == null)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutToClipboard_Click);
            if (this.listView_records.SelectedIndices.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyToClipboard_Click);
            if (this.listView_records.SelectedIndices.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("复制单列(&S)");
            // menuItem.Click += new System.EventHandler(this.menu_copySingleColumnToClipboard_Click);
            if (this.listView_records.SelectedIndices.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            for (int i = 0; i < this.listView_records.Columns.Count; i++)
            {
                MenuItem subMenuItem = new MenuItem("复制列 '" + this.listView_records.Columns[i].Text + "'");
                subMenuItem.Tag = i;
                subMenuItem.Click += new System.EventHandler(this.menu_copySingleColumnToClipboard_Click);
                menuItem.MenuItems.Add(subMenuItem);
            }

            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(string)) == false)
                bHasClipboardObject = false;
            else
                bHasClipboardObject = true;

            menuItem = new MenuItem("粘贴[前插](&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromClipboard_insertBefore_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴[后插](&V)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromClipboard_insertAfter_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAllLines_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            bool bLooping = (stop != null && stop.State == 0);    // 0 表示正在处理

            {
                menuItem = new MenuItem("功能(&F)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = null;

                subMenuItem = new MenuItem("校验读者记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&V)");
                subMenuItem.Click += new System.EventHandler(this.menu_verifyPatronRecord_Click);
                if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }

            // 批处理
            // 正在检索的时候，不允许进行批处理操作。因为stop.BeginLoop()嵌套后的Min Max Value之间的保存恢复问题还没有解决
            {
                menuItem = new MenuItem("批处理(&B)");
                menuItem.Enabled = !bSearching;  //  this.textBox_queryWord.Enabled;  // 在检索阶段，不允许使用批处理菜单
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = new MenuItem("快速修改读者记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&Q)");
                subMenuItem.Click += new System.EventHandler(this.menu_quickChangeRecords_Click);
                if (this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("执行 C# 脚本 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&M)");
                subMenuItem.Click += new System.EventHandler(this.menu_quickMarcQueryRecords_Click);
                if (this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("丢弃修改 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&C)");
                subMenuItem.Click += new System.EventHandler(this.menu_clearSelectedChangedRecords_Click);
                if (this.m_nChangedCount == 0 || this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("丢弃全部修改 [" + this.m_nChangedCount.ToString() + "] (&L)");
                subMenuItem.Click += new System.EventHandler(this.menu_clearAllChangedRecords_Click);
                if (this.m_nChangedCount == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("保存选定的修改 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&S)");
                subMenuItem.Click += new System.EventHandler(this.menu_saveSelectedChangedRecords_Click);
                if (this.m_nChangedCount == 0 || this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("保存全部修改 [" + this.m_nChangedCount.ToString() + "] (&A)");
                subMenuItem.Click += new System.EventHandler(this.menu_saveAllChangedRecords_Click);
                if (this.m_nChangedCount == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("创建新的 C# 脚本文件 (&C)");
                subMenuItem.Click += new System.EventHandler(this.menu_createMarcQueryCsFile_Click);
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("移动所选择的 " + this.listView_records.SelectedItems.Count.ToString() + " 个读者记录(&M)");
                subMenuItem.Click += new System.EventHandler(this.menu_moveRecords_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("删除读者记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&D)");
                subMenuItem.Click += new System.EventHandler(this.menu_deleteSelectedRecords_Click);
                if (this.listView_records.SelectedItems.Count == 0
                    || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("宏定义 (&M)");
                subMenuItem.Click += new System.EventHandler(this.menu_macroDef_Click);
                menuItem.MenuItems.Add(subMenuItem);
            }

#if NO
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("快速修改所选择的 " + this.listView_records.SelectedItems.Count.ToString() + " 个读者记录(&Q)");
            menuItem.Click += new System.EventHandler(this.menu_quickChangeRecords_Click);
            if (this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);
#endif

            // 导出
            {
                menuItem = new MenuItem("导出(&X)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = new MenuItem("导出到条码号文件 [" + this.listView_records.SelectedItems.Count.ToString() + " ] (&B)");
                subMenuItem.Click += new System.EventHandler(this.menu_exportBarcodeFile_Click);
                if (this.listView_records.SelectedItems.Count == 0
                    || bSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("导出到记录路径文件 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&P)");
                subMenuItem.Click += new System.EventHandler(this.menu_exportRecPathFile_Click);
                if (this.listView_records.SelectedItems.Count == 0
                    || bSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("到 Excel 文件 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&E)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportExcelFile_Click);
                if (this.listView_records.SelectedItems.Count == 0
                    || bSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("导出读者详情到 Excel 文件 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&D)");
                subMenuItem.Click += new System.EventHandler(this.menu_exportReaderInfoToExcelFile_Click);
                if (this.listView_records.SelectedItems.Count == 0
                    || bSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("打印读者账簿 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&D)");
                if (this.listView_records.SelectedItems.Count == 0
                    || bSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                List<string> names = GetPatronSheetNames();
                foreach (string name in names)
                {
                    MenuItem sheetMenuItem = new MenuItem(name);
                    sheetMenuItem.Click += new System.EventHandler(this.menu_printReaderSheet_Click);
                    sheetMenuItem.Tag = name;
                    if (this.listView_records.SelectedItems.Count == 0
    || bSearching == true)
                        sheetMenuItem.Enabled = false;

                    subMenuItem.MenuItems.Add(sheetMenuItem);
                }

#if NO
            menuItem = new MenuItem("导出借阅历史到 Excel 文件 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_exportBorrowHistoryToExcelFile_Click);
            if (this.listView_records.SelectedItems.Count == 0
                || bSearching == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);
#endif
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 装入其它查询窗
            {
                menuItem = new MenuItem("装入其它查询窗 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&L)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = new MenuItem("借阅历史 --> 实体查询窗");
                subMenuItem.Click += new System.EventHandler(this.menu_exportChargingHistoryToItemSearchForm_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("筛选 --> 读者查询窗");
                subMenuItem.Click += new System.EventHandler(this.menu_filterToAnotherReaderSearchForm_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("从记录路径文件中导入(&I)...");
            menuItem.Click += new System.EventHandler(this.menu_importFromRecPathFile_Click);
            if (bSearching == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("从条码号文件中导入(&R)...");
            menuItem.Click += new System.EventHandler(this.menu_importFromBarcodeFile_Click);
            if (bSearching == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&R)");
            menuItem.Click += new System.EventHandler(this.menu_refreshSelectedItems_Click);
            if (this.listView_records.SelectedItems.Count == 0
                || bSearching == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
        }

        List<string> GetPatronSheetNames()
        {
            try
            {
                // 标配部分
                string strDirectory = Path.Combine(Program.MainForm.DataDir, "patronSheetLayout");
                List<string> results = getPatronSheetNames(strDirectory);

                // 2019/4/23
                // 定制部分
                strDirectory = Path.Combine(Program.MainForm.UserDir, "patronSheetLayout");
                results.AddRange(getPatronSheetNames(strDirectory));

                return results;
            }
            catch
            {
                return new List<string>();
            }
        }

        List<string> getPatronSheetNames(string strDirectory)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(strDirectory);
                FileInfo[] fis = di.GetFiles("*.xml");  // patronSheetLayout
                List<string> results = new List<string>();
                foreach (FileInfo fi in fis)
                {
                    /*
                    List<string> parts = StringUtil.ParseTwoPart(fi.Name, "_");
                    parts = StringUtil.ParseTwoPart(parts[1], ".");
                    results.Add(parts[0]);
                    */
                    var parts = StringUtil.ParseTwoPart(fi.Name, ".");
                    results.Add(parts[0]);
                }

                return results;
            }
            catch
            {
                return new List<string>();
            }
        }

        // 删除所选定的读者记录
        void menu_deleteSelectedRecords_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
"确实要从数据库中删除所选定的 " + this.listView_records.SelectedItems.Count.ToString() + " 个读者记录?\r\n\r\n(警告：读者记录被删除后，无法恢复)\r\n\r\n(OK 删除；Cancel 取消)",
"ReaderSearchForm",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                items.Add(item);
            }

            string strError = "";
            int nDeleteCount = 0;

            // LibraryChannel channel = this.GetChannel();

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除读者记录 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            this.listView_records.Enabled = false;
            try
            {
                stop.SetProgressRange(0, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    if (stop != null && stop.State != 0)
                    {
                        strError = "已中断";
                        goto ERROR1;
                    }

                    ListViewItem item = items[i];
                    string strRecPath = ListViewUtil.GetItemText(item, 0);

                    string[] results = null;
                    byte[] baTimestamp = null;
                    string strOutputPath = "";


                    stop.SetMessage("正在删除读者记录 " + strRecPath);

                    long lRet = this.Channel.GetReaderInfo(
                        stop,
                        "@path:" + strRecPath,
                        "",
                        out results,
                        out strOutputPath,
                        out baTimestamp,
                        out strError);
                    if (lRet == 0)
                        goto ERROR1;

                    if (lRet == -1)
                    {
                        result = MessageBox.Show(this,
    "在获得记录 '" + strRecPath + "' 的时间戳的过程中出现错误: " + strError + "。\r\n\r\n是否继续强行删除此记录? (Yes 强行删除；No 不删除；Cancel 放弃当前未完成的全部删除操作)",
    "ReaderSearchForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            goto ERROR1;
                        if (result == System.Windows.Forms.DialogResult.No)
                            continue;
                    }

                    byte[] baNewTimestamp = null;
                    string strExistingXml = "";
                    string strSavedXml = "";
                    ErrorCodeValue error_code = ErrorCodeValue.NoError;

                    // channel.Timeout = new TimeSpan(0, 5, 0);
                    lRet = this.Channel.SetReaderInfo(
                        stop,
                        "delete",
                        strRecPath,
                        "", // strNewXml
                        "", // strOldXml,
                        baTimestamp,
                        out strExistingXml,
                        out strSavedXml,
                        out strOutputPath,
                        out baNewTimestamp,
                        out error_code,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    nDeleteCount++;

                    stop.SetProgressValue(i);

                    this.listView_records.Items.Remove(item);
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                // this.ReturnChannel(channel);

                this.EnableControls(true);
                this.listView_records.Enabled = true;
            }

            MessageBox.Show(this, "成功删除读者记录 " + nDeleteCount + " 条");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 导出选择的行到 Excel 文件
        void menu_exportExcelFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要导出的事项";
                goto ERROR1;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                items.Add(item);
            }
            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导出选定的事项到 Excel 文件 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                int nRet = ClosedXmlUtil.ExportToExcel(
                    stop,
                    items,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
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

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_verifyPatronRecord_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要校验的读者记录事项";
                goto ERROR1;
            }

            // 读者信息缓存
            // 如果已经初始化，则保持
            if (this.m_biblioTable == null)
                this.m_biblioTable = new Hashtable();

            int nCount = 0;

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
                + " 开始进行读者记录校验</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在校验读者记录 ...");
            stop.BeginLoop();

            this.EnableControls(false);

            this.listView_records.Enabled = false;
            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }

                ListViewPatronLoader loader = new ListViewPatronLoader(this.Channel,
                    stop,
                    items,
                    this.m_biblioTable);
                loader.DbTypeCaption = this.DbTypeCaption;

                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    stop.SetProgressValue(i);

                    BiblioInfo info = item.BiblioInfo;

                    // Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(info.OldXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "记录 '" + info.RecPath + "' 的 XML 装入 DOM 时出错: " + ex.Message;
                        goto ERROR1;
                    }

                    List<string> errors = new List<string>();

                    // 校验 XML 记录中是否有非法字符
                    string strReplaced = DomUtil.ReplaceControlCharsButCrLf(info.OldXml, '*');
                    if (strReplaced != info.OldXml)
                    {
                        errors.Add("XML 记录中有非法字符");
                    }

                    // 校验借书时间字符串是否合法
                    XmlNodeList borrows = dom.DocumentElement.SelectNodes("borrows/borrow");
                    foreach (XmlElement borrow in borrows)
                    {
                        DateTime borrow_time = new DateTime(0);

                        {
                            string borrowDate = borrow.GetAttribute("borrowDate");
                            if (string.IsNullOrEmpty(borrowDate))
                            {
                                errors.Add("出现了 borrow 元素的 borrowDate 属性值为空的情况");
                                continue;
                            }

                            try
                            {
                                borrow_time = DateTimeUtil.FromRfc1123DateTimeString(borrowDate).ToLocalTime();
                                if (borrow_time > DateTime.Now)
                                {
                                    errors.Add("借书时间 '" + borrow_time.ToString() + "' 比当前时间还靠后");
                                }
                            }
                            catch (Exception ex)
                            {
                                errors.Add("borrow 元素的 borrowDate 属性值 '" + borrowDate + "' 不合法: " + ex.Message);
                            }
                        }

                        {
                            string returningDate = borrow.GetAttribute("returningDate");
                            if (string.IsNullOrEmpty(returningDate))
                            {
                                errors.Add("出现了 borrow 元素的 returningDate 属性值为空的情况");
                                continue;
                            }

                            try
                            {
                                DateTime time = DateTimeUtil.FromRfc1123DateTimeString(returningDate).ToLocalTime();
                                if (time < borrow_time)
                                {
                                    errors.Add("应还书时间 '" + time.ToString() + "' 比借书时间还靠前");
                                }
                            }
                            catch (Exception ex)
                            {
                                errors.Add("borrow 元素的 returningDate 属性值 '" + returningDate + "' 不合法: " + ex.Message);
                            }
                        }
                    }

                    string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");

                    if (string.IsNullOrEmpty(strBarcode) == false)
                    {
                        string strReaderDbName = Global.GetDbName(info.RecPath);
                        string strLibraryCode = Program.MainForm.GetReaderDbLibraryCode(strReaderDbName);

                        // <para>-2  服务器没有配置校验方法，无法校验</para>
                        // <para>-1  出错</para>
                        // <para>0   不是合法的条码号</para>
                        // <para>1   是合法的读者证条码号</para>
                        // <para>2   是合法的册条码号</para>
                        nRet = Program.MainForm.VerifyBarcode(
        this.stop,
        this.Channel,
        strLibraryCode,
        strBarcode,
        null,
        out strError);
                        if (nRet == -2)
                            goto ERROR1;
                        if (nRet != 1)
                        {
                            if (nRet == 2 && string.IsNullOrEmpty(strError) == true)
                                strError = strLibraryCode + ": 这看起来是一个册条码号";

                            errors.Add("证条码号 '" + strBarcode + "' 不合法: " + strError);
                        }
                    }

                    if (errors.Count > 0)
                    {
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");
                        foreach (string error in errors)
                        {
                            Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(error) + "</div>");
                        }

                        {
                            item.ListViewItem.BackColor = Color.FromArgb(155, 0, 0);
                            item.ListViewItem.ForeColor = Color.FromArgb(255, 255, 255);
                        }
                    }

                    nCount++;

                    i++;
                }

            }
            catch (Exception ex)
            {
                strError = "校验读者记录的过程中出现异常: " + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this.listView_records.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControls(true);

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
                    + " 结束执行读者记录校验</div>");
            }

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
            if (e.Actions == "yes,no,cancel")
            {
                DialogResult result = AutoCloseMessageBox.Show(this,
    e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
    20 * 1000,
    "ReaderSearchForm");
                if (result == DialogResult.Cancel)
                    e.ResultAction = "no";
                else
                    e.ResultAction = "yes";
            }
        }


        // 筛选 --> 读者查询窗
        void menu_filterToAnotherReaderSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";

            FilterPatronDialog dlg = new FilterPatronDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            Program.MainForm.AppInfo.LinkFormState(dlg, "readersearchform_FilterPatronDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ReaderSearchForm form = new ReaderSearchForm();
            form.MdiParent = Program.MainForm;
            form.Show();

            form.EnableControls(false);
            try
            {
                int nCount = 0;

                // return:
                //      -1  出错。包括用户中断的情况
                //      >=0 实际处理的读者记录数
                int nRet = this.ProcessSelectedPatrons(
                    (strRecPath, dom, timestamp) =>
                    {
                        this.ShowMessage("正在处理读者记录 " + strRecPath);
                        form.ShowMessage("正在处理读者记录 " + strRecPath);

                        string barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                        XmlNodeList borrows = dom.DocumentElement.SelectNodes("borrows/borrow");
                        XmlNodeList overdues = dom.DocumentElement.SelectNodes("overdues/overdue");

                        bool bOn = false;

                        // “无在借册和违约金的”
                        if (dlg.NoBorrowAndOverdueItem == true)
                        {
                            if (borrows.Count == 0 && overdues.Count == 0)  // 2018/4/26 从 || 修改为 &&
                                bOn = true;
                        }

                        // “有在借册的 / 已超期”

                        if (dlg.OutofPeriodItem == true)
                        {
                            if (borrows.Count > 0)
                            {
                                foreach (XmlElement borrow in borrows)
                                {
                                    // 2016/6/12
                                    {
                                        string strBorrowDate = borrow.GetAttribute("borrowDate");
                                        string strPeriod = borrow.GetAttribute("borrowPeriod");
                                        if (string.IsNullOrEmpty(strBorrowDate) == true
                                            || string.IsNullOrEmpty(strPeriod) == true)
                                        {
                                            // 记入错误日志
                                            continue;
                                        }
                                        nRet = Global.IsOverdue(strBorrowDate,
                                            strPeriod,
                                            out strError);
                                        if (nRet == -1)
                                        {
                                            // 记入错误日志
                                            continue;
                                        }
                                        if (nRet == 1)
                                        {
                                            bOn = true;
                                            break;
                                        }
                                        continue;
                                    }

#if NO
                                    string strReturningDate = borrow.GetAttribute("returningDate");
                                    // 注: returningDate 为空，这是不正常状态，或者早期的数据。但依然可以通过 borrowDate 和 period 来测算
                                    if (string.IsNullOrEmpty(strReturningDate) == true)
                                        continue;
                                    try
                                    {
                                        // TODO: 处理抛出异常
                                        DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strReturningDate);
                                        // TODO: 时间要正规化以后再比较
                                        if (time.ToLocalTime() < DateTime.Now)
                                        {
                                            bOn = true;
                                            break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // TODO: 记入错误日志
                                    }
#endif
                                }
                            }
                        }

                        // “有在借册的 / 未超期”是否被勾选
                        if (dlg.InPeriodItem == true)
                        {
                            if (borrows.Count > 0)
                            {
                                int nOverdueCount = 0;
                                foreach (XmlElement borrow in borrows)
                                {
#if NO
                                    string strReturningDate = borrow.GetAttribute("returningDate");
                                    // TODO: 处理抛出异常
                                    DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strReturningDate);
                                    // TODO: 时间要正规化以后再比较
                                    if (time.ToLocalTime() < DateTime.Now)
                                        nOverdueCount++;
#endif
                                    string strBorrowDate = borrow.GetAttribute("borrowDate");
                                    string strPeriod = borrow.GetAttribute("borrowPeriod");
                                    if (string.IsNullOrEmpty(strBorrowDate) == true
                                        || string.IsNullOrEmpty(strPeriod) == true)
                                    {
                                        // 记入错误日志
                                        continue;
                                    }
                                    nRet = Global.IsOverdue(strBorrowDate,
                                        strPeriod,
                                        out strError);
                                    if (nRet == -1)
                                    {
                                        // 记入错误日志
                                        continue;
                                    }
                                    if (nRet == 1)
                                        nOverdueCount++;
                                }
                                if (nOverdueCount == 0)
                                    bOn = true;
                            }
                        }

                        // “有违约金的”是否被勾选
                        if (dlg.HasOverdueItem == true)
                        {
                            if (overdues.Count > 0)
                                bOn = true;
                        }

                        if (bOn == true)
                        {
                            form.AddBarcodeToBrowseList(barcode);
                            nCount++;
                        }
                        return true;
                    },
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                form.RrefreshAllItems();
                form.ShowMessage("共装入读者记录 " + nCount + " 个", "green", true);
            }
            finally
            {
                form.EnableControls(true);
            }

            this.ShowMessage("完成", "green", true);
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        // 有在借册的 --> 读者查询窗
        void menu_exportBorrowingToReaderSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<string> barcodes = new List<string>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                // TODO: 用 style 来识别列
                barcodes.Add(item.SubItems[1].Text);
            }

            int nCount = 0;

            ReaderSearchForm form = new ReaderSearchForm();
            form.MdiParent = Program.MainForm;
            form.Show();

            form.EnableControls(false);
            try
            {
                // return:
                //      -1  出错。包括用户中断的情况
                //      >=0 实际处理的读者记录数
                int nRet = this.ProcessPatrons(barcodes,
                    (strRecPath, dom, timestamp) =>
                    {
                        this.ShowMessage("正在处理读者记录 " + strRecPath);
                        form.ShowMessage("正在处理读者记录 " + strRecPath);

                        string barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                        XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
                        if (nodes.Count > 0)
                        {
                            form.AddBarcodeToBrowseList(barcode);
                            nCount++;
                        }
                        return true;
                    },
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                form.RrefreshAllItems();
                form.ShowMessage("共装入读者记录 " + nCount + " 个", "green", true);
            }
            finally
            {
                form.EnableControls(true);
            }

            this.ShowMessage("完成", "green", true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#endif
        // 借阅历史 --> 实体查询窗
        void menu_exportChargingHistoryToItemSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<string> barcodes = new List<string>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                // TODO: 用 style 来识别列
                barcodes.Add(item.SubItems[1].Text);
            }

            // return:
            //      -1  出错
            //      0   用户中断
            //      1   成功
            int nRet = this.ExportChargingHistoryToItemSearchForm(barcodes,
                out strError);
            if (nRet != 1)
                goto ERROR1;

            // MessageBox.Show(this, "导出完成");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 修改宏定义
        void menu_macroDef_Click(object sender, EventArgs e)
        {
            MacroTableDialog dlg = new MacroTableDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.XmlFileName = Path.Combine(Program.MainForm.UserDir, "patron_macrotable.xml");

            Program.MainForm.AppInfo.LinkFormState(dlg, "readersearchform_MacroTableDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
        }

        // 刷新所选择的行。也就是重新从数据库中装载浏览列
        void menu_refreshSelectedItems_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要刷新的浏览行";
                goto ERROR1;
            }

            int nChangedCount = 0;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;
                items.Add(item);

                if (IsItemChanged(item) == true)
                    nChangedCount++;
            }

            // 警告未保存的内容会丢失
            if (nChangedCount > 0)
            {
                DialogResult result = MessageBox.Show(this,
    "要刷新的 " + this.listView_records.SelectedItems.Count.ToString()+ " 个事项中有 " + nChangedCount.ToString() + " 项修改后尚未保存。如果刷新它们，修改内容会丢失。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
    "ReaderSearchForm",
    MessageBoxButtons.OKCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
            }

            nRet = RefreshListViewLines(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            DoViewComment(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            RrefreshSelectedItems();
        }



#if NO
        // 刷新所选择的行。也就是重新从数据库中装载浏览列
        void menu_refreshSelectedItems_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要刷新浏览列的事项";
                goto ERROR1;
            }

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新浏览列 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);
                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    if (stop != null)
                    {
                        stop.SetMessage("正在刷新浏览行 " + item.Text + " ...");
                        stop.SetProgressValue(i++);
                    }
                    nRet = RefreshBrowseLine(item,
    out strError);
                    if (nRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
                            "刷新浏览内容时出错: " + strError + "。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
                            "ReaderSearchForm",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            break;
                    }
                }
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

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#endif

        void menu_importFromBarcodeFile_Click(object sender, EventArgs e)
        {
            SetStatusMessage("");   // 清除以前残留的显示

            // bool bSkipBrowe = false;
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的条码号文件名";
            dlg.FileName = this.m_strUsedBarcodeFilename;
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedBarcodeFilename = dlg.FileName;

            StreamReader sr = null;
            string strError = "";

            try
            {
                // TODO: 最好自动探测文件的编码方式?
                sr = new StreamReader(dlg.FileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + dlg.FileName + " 失败: " + ex.Message;
                goto ERROR1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入条码号 ...");
            stop.BeginLoop();

            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this.listView_records);

                if (this.listView_records.Items.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "导入前是否要清除命中记录列表中的现有的 " + this.listView_records.Items.Count.ToString() + " 行?\r\n\r\n(如果不清除，则新导入的行将追加在已有行后面)\r\n(Yes 清除；No 不清除(追加)；Cancel 放弃导入)",
                        "ReaderSearchForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return;
                    if (result == DialogResult.Yes)
                    {
                        ClearListViewItems();
                    }
                }

                stop.SetProgressRange(0, sr.BaseStream.Length);

                List<ListViewItem> items = new List<ListViewItem>();

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

                    string strBarcode = sr.ReadLine();

                    stop.SetProgressValue(sr.BaseStream.Position);


                    if (strBarcode == null)
                        break;

                    // 


                    ListViewItem item = new ListViewItem();
                    item.Text = "";
                    ListViewUtil.ChangeItemText(item, 1, strBarcode);

                    this.listView_records.Items.Add(item);

                    // if (FillLineByBarcode(strBarcode, item, ref bSkipBrowe) == true)
                    //     break;
                    FillLineByBarcode(strBarcode, item);

                    items.Add(item);
                }

                // 刷新浏览行
                int nRet = RefreshListViewLines(
                    this.Channel,
                    items,
                    "",
                    false,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                if (sr != null)
                    sr.Close();
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 向浏览框末尾新加入一行
        /// </summary>
        /// <param name="strBarcode">证条码号</param>
        /// <returns>新创建的 ListViewItem 对象</returns>
        public ListViewItem AddBarcodeToBrowseList(string strBarcode)
        {
            ListViewItem item = new ListViewItem();

            // TODO: 需要改造为根据列定义知道列号
            ListViewUtil.ChangeItemText(item, 1, strBarcode);
            FillLineByBarcode(strBarcode, item);

            this._listviewRecords.Items.Add(item);
            return item;
        }

        bool FillLineByBarcode(string strBarcode,
    ListViewItem item)
        {
            string strError = "";
            string strReaderRecPath = "";

            // 检索册条码号，检索出其从属的书目记录路径。
            int nRet = SearchRecPathByBarcode(strBarcode,
            out strReaderRecPath,
            out strError);
            if (nRet == -1)
            {
                ListViewUtil.ChangeItemText(item, 2, strError);
            }
            else if (nRet == 0)
            {
                ListViewUtil.ChangeItemText(item, 2, "条码号 '" + strBarcode + "' 没有找到记录");
            }
            else if (nRet == 1)
            {
                item.Text = strReaderRecPath;
            }
            else if (nRet > 1) // 命中发生重复
            {
                ListViewUtil.ChangeItemText(item, 2, "条码号 '" + strBarcode + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误");
                return false;
            }

            return true;
        }

#if NO
        // return:
        //      true    要中断
        //      false   不中断
        bool FillLineByBarcode(string strBarcode,
            ListViewItem item,
            ref bool bSkipBrowse)
        {
            string strError = "";
            string strReaderRecPath = "";


            // 检索册条码号，检索出其从属的书目记录路径。
            int nRet = SearchRecPathByBarcode(strBarcode,
            out strReaderRecPath,
            out strError);
            if (nRet == -1)
            {
                ListViewUtil.ChangeItemText(item, 2, strError);
            }
            else if (nRet == 0)
            {
                ListViewUtil.ChangeItemText(item, 2, "条码号 '" + strBarcode + "' 没有找到记录");
            }
            else if (nRet == 1)
            {
                item.Text = strReaderRecPath;

                if (bSkipBrowse == false
    && !(Control.ModifierKeys == Keys.Control))
                {
                    nRet = RefreshBrowseLine(item,
            out strError);
                    if (nRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
    "获得浏览内容时出错: " + strError + "。\r\n\r\n是否继续获取浏览内容? (Yes 获取；No 不获取；Cancel 放弃导入)",
    "ReaderSearchForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.No)
                            bSkipBrowse = true;
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                        {
                            return true;
                        }
                    }
                }

            }
            else if (nRet > 1) // 命中发生重复
            {
                ListViewUtil.ChangeItemText(item, 2, "条码号 '" + strBarcode + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误");
            }

            return false;
        }

#endif

        // 根据读者证条码号，检索出其册记录路径
        int SearchRecPathByBarcode(string strBarcode,
            out string strReaderRecPath,
            out string strError)
        {
            strError = "";
            strReaderRecPath = "";

            try
            {
                byte[] baTimestamp = null;

                string[] results = null;
                long lRet = Channel.GetReaderInfo(
                    stop,
                    strBarcode,
                    "",
                    out results,
                    out strReaderRecPath,
                    out baTimestamp,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;   // 
            }
            finally
            {
            }
        }

        // 从记录路径文件中导入
        void menu_importFromRecPathFile_Click(object sender, EventArgs e)
        {
            SetStatusMessage("");   // 清除以前残留的显示

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的读者记录路径文件名";
            dlg.FileName = this.m_strUsedRecPathFilename;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedRecPathFilename = dlg.FileName;

            StreamReader sr = null;
            string strError = "";
            // bool bSkipBrowse = false;

            try
            {
                // TODO: 最好自动探测文件的编码方式?
                sr = new StreamReader(dlg.FileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + dlg.FileName + " 失败: " + ex.Message;
                goto ERROR1;
            }

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入记录路径 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this.listView_records);
                stop.SetProgressRange(0, sr.BaseStream.Length);


                if (this.listView_records.Items.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "导入前是否要清除命中记录列表中的现有的 " + this.listView_records.Items.Count.ToString() + " 行?\r\n\r\n(如果不清除，则新导入的行将追加在已有行后面)\r\n(Yes 清除；No 不清除(追加)；Cancel 放弃导入)",
                        "ReaderSearchForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return;
                    if (result == DialogResult.Yes)
                    {
                        ClearListViewItems();
                    }
                }

                List<ListViewItem> items = new List<ListViewItem>();
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

                    string strRecPath = sr.ReadLine();

                    stop.SetProgressValue(sr.BaseStream.Position);

                    if (strRecPath == null)
                        break;

                    // 检查路径的正确性，检查数据库是否为读者库之一
                    string strDbName = Global.GetDbName(strRecPath);
                    if (string.IsNullOrEmpty(strDbName) == true)
                    {
                        strError = "'" + strRecPath + "' 不是合法的记录路径";
                        goto ERROR1;
                    }

                    if (Program.MainForm.IsReaderDbName(strDbName) == false)
                    {
                        strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的读者库名。很可能所指定的文件不是读者库的记录路径文件";
                        goto ERROR1;
                    }

                    ListViewItem item = new ListViewItem();
                    item.Text = strRecPath;

                    this.listView_records.Items.Add(item);

#if NO
                    if (bSkipBrowse == false
                        && !(Control.ModifierKeys == Keys.Control))
                    {
                        int nRet = RefreshBrowseLine(item,
                out strError);
                        if (nRet == -1)
                        {
                            DialogResult result = MessageBox.Show(this,
        "获得浏览内容时出错: " + strError + "。\r\n\r\n是否继续获取浏览内容? (Yes 获取；No 不获取；Cancel 放弃导入)",
        "ReaderSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                bSkipBrowse = true;
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                            {
                                strError = "已中断";
                                break;
                            }
                        }
                    }
#endif
                    items.Add(item);

                }

                int nRet = RefreshListViewLines(
                    this.Channel,
                    items,
                    "",
                    false,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControls(true);

                if (sr != null)
                    sr.Close();
            }

            DoViewComment(false);
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        // 调用前，记录路径列已经有值
        public int RefreshBrowseLine(ListViewItem item,
            out string strError)
        {
            strError = "";

            string strRecPath = ListViewUtil.GetItemText(item, 0);
            string[] paths = new string[1];
            paths[0] = strRecPath;
            DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

            long lRet = this.Channel.GetBrowseRecords(
                this.stop,
                paths,
                "id,cols",
                out searchresults,
                out strError);
            if (lRet == -1)
                return -1;

            if (searchresults == null || searchresults.Length == 0)
            {
                strError = "searchresults == null || searchresults.Length == 0";
                return -1;
            }

            for (int i = 0; i < searchresults[0].Cols.Length; i++)
            {
                ListViewUtil.ChangeItemText(item,
                    i + 1,
                    searchresults[0].Cols[i]);
            }

            return 0;
        }
#endif

        void ClearListViewItems()
        {
            this.listView_records.Items.Clear();

            ListViewUtil.ClearSortColumns(this.listView_records);

            // 清除所有需要确定的栏标题
            for (int i = 1; i < this.listView_records.Columns.Count; i++)
            {
                this.listView_records.Columns[i].Text = i.ToString();
            }

#if NO
            this.m_biblioTable = new Hashtable();
            this.m_nChangedCount = 0;

            if (this.m_commentViewer != null)
                this.m_commentViewer.Clear();
#endif
            ClearBiblioTable();
            ClearCommentViewer();
        }

        void menu_selectAllLines_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                this.listView_records.SelectedIndexChanged -= new System.EventHandler(this.listView_records_SelectedIndexChanged);

                ListViewUtil.SelectAllLines(this.listView_records);

                this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
                listView_records_SelectedIndexChanged(null, null);
            }
            finally
            {
                this.Cursor = oldCursor;
            }
        }

        void menu_copyToClipboard_Click(object sender, EventArgs e)
        {
            Global.CopyLinesToClipboard(this, this.listView_records, false);
        }

        void menu_copySingleColumnToClipboard_Click(object sender, EventArgs e)
        {
            int nColumn = (int)((MenuItem)sender).Tag;

            Global.CopyLinesToClipboard(this, nColumn, this.listView_records, false);
        }

        void menu_cutToClipboard_Click(object sender, EventArgs e)
        {
            this.listView_records.SelectedIndexChanged -= new System.EventHandler(this.listView_records_SelectedIndexChanged);

            Global.CopyLinesToClipboard(this, this.listView_records, true);

            this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
            listView_records_SelectedIndexChanged(null, null);
        }

        void menu_pasteFromClipboard_insertBefore_Click(object sender, EventArgs e)
        {
            this.listView_records.SelectedIndexChanged -= new System.EventHandler(this.listView_records_SelectedIndexChanged);

            Global.PasteLinesFromClipboard(this, this.listView_records, true);

            this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
            listView_records_SelectedIndexChanged(null, null);

        }

        void menu_pasteFromClipboard_insertAfter_Click(object sender, EventArgs e)
        {
            this.listView_records.SelectedIndexChanged -= new System.EventHandler(this.listView_records_SelectedIndexChanged);

            Global.PasteLinesFromClipboard(this, this.listView_records, false);

            this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
            listView_records_SelectedIndexChanged(null, null);
        }

        // (根据记录路径)装入到读者窗
        void menu_loadReaderInfoByRecPath_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装入读者窗的事项");
                return;
            }
            string strRecPath = this.listView_records.SelectedItems[0].SubItems[0].Text;

            ReaderInfoForm form = new ReaderInfoForm();

            form.MdiParent = Program.MainForm;

            form.MainForm = Program.MainForm;
            form.Show();

            // form.LoadRecord("@path:" + strRecPath, false);   // 这个办法有问题，ReaderInfoForm.ReaderBarcode有误

            form.LoadRecordByRecPath(strRecPath, "");
        }

#if NO
        // 快速修改记录
        void menu_quickChangeRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            bool bSkipUpdateBrowse = false; // 是否要跳过更新浏览行

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要快速修改的读者记录事项";
                goto ERROR1;
            }

            ChangeReaderActionDialog dlg = new ChangeReaderActionDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Text = "快速修改读者记录 -- 请指定动作参数";
            dlg.MainForm = Program.MainForm;
            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

            Program.MainForm.AppInfo.LinkFormState(dlg, "readersearchform_quickchangedialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            DateTime now = DateTime.Now;

            // TODO: 检查一下，看看是否一项修改动作都没有

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("快速修改读者记录 ...");
            stop.BeginLoop();

            EnableControls(false);
            this.listView_records.Enabled = false;

            int nProcessCount = 0;
            int nChangedCount = 0;
            try
            {
                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
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

                    string strRecPath = item.Text;

                    if (string.IsNullOrEmpty(strRecPath) == true)
                    {
                        // Debug.Assert(false, "");
                        continue;
                    }

                REDO_CHANGE:
                    // 获得读者记录
                    byte[] baTimestamp = null;
                    string strOutputRecPath = "";

                    stop.SetMessage("正在装入读者记录 " + strRecPath + " ...");

                    string[] results = null;
                    long lRet = Channel.GetReaderInfo(
                        stop,
                        "@path:" + strRecPath,
                        "xml",
                        out results,
                        out strOutputRecPath,
                        out baTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        goto ERROR1;
                    }

                    if (lRet > 1)   // 不可能发生吧?
                    {
                        strError = "记录路径 " + strRecPath + " 命中记录 " + lRet.ToString() + " 条，放弃装入读者记录。\r\n\r\n注意这是一个严重错误，请系统管理员尽快排除。";
                        goto ERROR1;
                    }
                    if (results == null || results.Length < 1)
                    {
                        strError = "返回的results不正常。";
                        goto ERROR1;
                    }
                    string strXml = results[0];

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载XML到DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    // 修改一个读者记录XmlDocument
                    // return:
                    //      -1  出错
                    //      0   没有实质性修改
                    //      1   发生了修改
                    nRet = ModifyRecord(ref dom,
                        now,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    nProcessCount++;

                    if (nRet == 0)
                        continue;

                    Debug.Assert(nRet == 1, "");


                    ErrorCodeValue kernel_errorcode;

                    byte[] baNewTimestamp = null;
                    string strExistingXml = "";
                    string strSavedXml = "";
                    string strSavedPath = "";
                    lRet = Channel.SetReaderInfo(
    stop,
    "change",
    strRecPath,
    dom.OuterXml,
    strXml,
    baTimestamp,
    out strExistingXml,
    out strSavedXml,
    out strSavedPath,
    out baNewTimestamp,
    out kernel_errorcode,
    out strError);
                    if (lRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
"保存读者记录 '" + strRecPath + "' 时出错: " + strError + "。\r\n\r\n是否重试保存? (Yes 重试；No 忽略此条记录的保存，但是急需处理后面的记录；Cancel 中断批修改操作)",
"ReaderSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                            goto REDO_CHANGE;

                        if (result == System.Windows.Forms.DialogResult.No)
                            continue;
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            goto ERROR1;
                    }

                    // 刷新浏览行
                    if (bSkipUpdateBrowse == false)
                    {
                        nRet = RefreshBrowseLine(item,
    out strError);
                        if (nRet == -1)
                        {
                            DialogResult result = MessageBox.Show(this,
        "刷新浏览内容时出错: " + strError + "。\r\n\r\n后面是否继续(在修改操作后)刷新浏览内容? (Yes 获取；No 不获取；Cancel 中断批修改操作)",
        "ReaderSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                bSkipUpdateBrowse = true;
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                                break;
                        }
                    }

                    stop.SetProgressValue(++i);
                    nChangedCount++;
                }
            }
            finally
            {
                EnableControls(true);
                this.listView_records.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;
            }
            MessageBox.Show(this, "成功修改读者记录 " + nChangedCount.ToString() + " 条 (共处理 "+nProcessCount.ToString()+" 条)");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif
        // 快速修改记录
        void menu_quickChangeRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = QuickChangeItemRecords(out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet != 0)
                MessageBox.Show(this, strError);
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        // 快速修改记录
        void menu_quickChangeRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            // bool bSkipUpdateBrowse = false; // 是否要跳过更新浏览行

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要快速修改的读者记录事项";
                goto ERROR1;
            }

            ChangeReaderActionDialog dlg = new ChangeReaderActionDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Text = "快速修改读者记录 -- 请指定动作参数";
            dlg.MainForm = Program.MainForm;
            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

            Program.MainForm.AppInfo.LinkFormState(dlg, "readersearchform_quickchangedialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            DateTime now = DateTime.Now;

            // TODO: 检查一下，看看是否一项修改动作都没有
            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行快速修改读者记录</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("快速修改读者记录 ...");
            stop.BeginLoop();

            EnableControls(false);
            this.listView_records.Enabled = false;

            int nProcessCount = 0;
            int nChangedCount = 0;
            try
            {
                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }

                ListViewPatronLoader loader = new ListViewPatronLoader(this.Channel,
                    stop,
                    items,
                    this.m_biblioTable);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    stop.SetProgressValue(i);

                    BiblioInfo info = item.BiblioInfo;

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(info.OldXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载XML到DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    string strDebugInfo = "";
                    // 修改一个读者记录XmlDocument
                    // return:
                    //      -1  出错
                    //      0   没有实质性修改
                    //      1   发生了修改
                    nRet = ModifyRecord(ref dom,
                        now,
                        out strDebugInfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug normal'>" + HttpUtility.HtmlEncode(strDebugInfo).Replace("\r\n", "<br/>") + "</div>");

                    nProcessCount++;

                    if (nRet == 1)
                    {
                        string strXml = dom.OuterXml;
                        if (info != null)
                        {
                            if (string.IsNullOrEmpty(info.NewXml) == true)
                                this.m_nChangedCount++;
                            info.NewXml = strXml;
                        }

                        item.ListViewItem.BackColor = SystemColors.Info;
                        item.ListViewItem.ForeColor = SystemColors.InfoText;
                    }

                    i++;
                    nChangedCount++;
                }
            }
            finally
            {
                EnableControls(true);
                this.listView_records.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束快速修改读者记录</div>");
            }

            DoViewComment(false);
            MessageBox.Show(this, "修改读者记录 " + nChangedCount.ToString() + " 条 (共处理 " + nProcessCount.ToString() + " 条)\r\n\r\n(注意修改并未自动保存。请在观察确认后，使用保存命令将修改保存回读者库)");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#endif


#if NO
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
#endif

#if NO
        // 修改一个读者记录XmlDocument
        // return:
        //      -1  出错
        //      0   没有实质性修改
        //      1   发生了修改
        int ModifyRecord(ref XmlDocument dom,
            DateTime now,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";

            bool bChanged = false;

            StringBuilder debug = new StringBuilder(4096);

            // state
            string strStateAction = Program.MainForm.AppInfo.GetString(
                "change_reader_param",
                "state",
                "<不改变>");
            if (strStateAction != "<不改变>")
            {
                string strState = DomUtil.GetElementText(dom.DocumentElement,
                    "state");

                if (strStateAction == "<增、减>")
                {
                    string strAdd = Program.MainForm.AppInfo.GetString(
                "change_reader_param",
                "state_add",
                "");
                    string strRemove = Program.MainForm.AppInfo.GetString(
            "change_reader_param",
            "state_remove",
            "");

                    string strOldState = strState;

                    if (String.IsNullOrEmpty(strAdd) == false)
                        StringUtil.SetInList(ref strState, strAdd, true);
                    if (String.IsNullOrEmpty(strRemove) == false)
                        StringUtil.SetInList(ref strState, strRemove, false);

                    if (strOldState != strState)
                    {
                        DomUtil.SetElementText(dom.DocumentElement,
                            "state",
                            strState);
                        bChanged = true;

                        debug.Append("<state> '" + strOldState + "' --> '" + strState + "'\r\n");
                    }
                }
                else
                {
                    if (strStateAction != strState)
                    {
                        DomUtil.SetElementText(dom.DocumentElement,
                            "state",
                            strStateAction);
                        bChanged = true;

                        debug.Append("<state> '" + strState + "' --> '" + strStateAction + "'\r\n");
                    }
                }
            }

            // expire date
            string strTimeAction = Program.MainForm.AppInfo.GetString(
    "change_reader_param",
    "expire_date",
    "<不改变>");
            if (strTimeAction != "<不改变>")
            {
                string strTime = DomUtil.GetElementText(dom.DocumentElement,
                    "expireDate");
                DateTime time = new DateTime(0);
                if (strTimeAction == "<当前时间>")
                {
                    time = now;
                }
                else if (strTimeAction == "<清除>")
                {

                }
                else if (strTimeAction == "<指定时间>")
                {
                    string strValue = Program.MainForm.AppInfo.GetString(
                        "change_reader_param",
                        "expire_date_value",
                        "");
                    if (String.IsNullOrEmpty(strValue) == true)
                    {
                        strError = "当进行 <指定时间> 方式的修改时，所指定的时间值不能为空";
                        return -1;
                    }
                    try
                    {
                        time = DateTime.Parse(strValue);
                    }
                    catch (Exception ex)
                    {
                        strError = "无法解析时间字符串 '" + strValue + "' :" + ex.Message;
                        return -1;
                    }
                }
                else
                {
                    // 不支持
                    strError = "不支持的时间动作 '" + strTimeAction + "'";
                    return -1;
                }

                string strOldTime = strTime;

                if (strTimeAction == "<清除>")
                    strTime = "";
                else
                    strTime = DateTimeUtil.Rfc1123DateTimeStringEx(time);

                if (strOldTime != strTime)
                {
                    DomUtil.SetElementText(dom.DocumentElement,
                        "expireDate",
                        strTime);
                    bChanged = true;

                    debug.Append("<expireDate> '" + strOldTime + "' --> '" + strTime + "'\r\n");
                }
            }

            // reader type
            string strReaderTypeAction = Program.MainForm.AppInfo.GetString(
"change_reader_param",
"reader_type",
"<不改变>");
            if (strReaderTypeAction != "<不改变>")
            {
                string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
                    "readerType");

                if (strReaderType != strReaderTypeAction)
                {
                    DomUtil.SetElementText(dom.DocumentElement,
                        "readerType",
                        strReaderTypeAction);
                    bChanged = true;

                    debug.Append("<readerType> '" + strReaderType + "' --> '" + strReaderTypeAction + "'\r\n");
                }
            }

            // 其它字段
            string strFieldName = Program.MainForm.AppInfo.GetString(
"change_reader_param",
"field_name",
"<不使用>");
            if (strFieldName != "<不使用>")
            {
                string strFieldValue = Program.MainForm.AppInfo.GetString(
    "change_reader_param",
    "field_value",
    "");
                if (strFieldName == "证条码号")
                {
                    ChangeField(ref dom,
            "barcode",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "证号")
                {
                    ChangeField(ref dom,
            "cardNumber",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "发证日期")
                {
                    ChangeField(ref dom,
            "createDate",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "失效日期")
                {
                    ChangeField(ref dom,
            "expireDate",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "注释")
                {
                    ChangeField(ref dom,
            "comment",
            strFieldValue,
            ref debug,
            ref bChanged);
                }


                if (strFieldName == "租金周期")
                {
                    // hire 元素的 period 属性
                    ChangeField(ref dom,
            "hire",
            "period",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "租金失效期")
                {
                    // hire 元素的 expireDate 属性

                    ChangeField(ref dom,
            "hire",
            "expireDate",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "押金余额")
                {
                    ChangeField(ref dom,
            "foregift",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "姓名")
                {
                    ChangeField(ref dom,
            "name",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "性别")
                {
                    ChangeField(ref dom,
            "gender",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "出生日期")
                {
                    ChangeField(ref dom,
            "dateOfBirth",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "身份证号")
                {
                    ChangeField(ref dom,
            "idCardNumber",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "单位")
                {
                    ChangeField(ref dom,
            "department",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "职务")
                {
                    ChangeField(ref dom,
            "post",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "地址")
                {
                    ChangeField(ref dom,
            "address",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "电话")
                {
                    ChangeField(ref dom,
            "tel",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "Email地址")
                {
                    ChangeField(ref dom,
            "email",
            strFieldValue,
            ref debug,
            ref bChanged);
                }
            }

            strDebugInfo = debug.ToString();

            if (bChanged == true)
                return 1;

            return 0;
        }
#endif

        // 成批移动读者记录
        void menu_moveRecords_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要移动的读者记录事项";
                goto ERROR1;
            }

            if (this.m_nChangedCount > 0)
            {
                // 警告尚未保存
                strError = "当前窗口内有 " + m_nChangedCount + " 项修改尚未保存。若此时移动读者记录，现有未保存信息可能会丢失。\r\n\r\n请先保存记录，或者放弃修改后，再重新执行本命令";
                goto ERROR1;
            }

            // 得到选定范围的第一条记录的路径
            string strFirstRecPath = this.listView_records.SelectedItems[0].Text;

            // 出现对话框，让用户可以选择目标库
            ReaderSaveToDialog saveto_dlg = new ReaderSaveToDialog();
            MainForm.SetControlFont(saveto_dlg, this.Font, false);
            saveto_dlg.Text = "移动读者记录";
            saveto_dlg.MessageText = "请选择要移动去的目标记录位置";
            // saveto_dlg.MainForm = Program.MainForm;
            saveto_dlg.RecPath = strFirstRecPath;
            saveto_dlg.RecID = "?";
            if (this.listView_records.SelectedItems.Count > 1)
                saveto_dlg.EnableRecID = false; // 处理记录多于一条的情况下，问号ID不让修改

            Program.MainForm.AppInfo.LinkFormState(saveto_dlg, "readersearchform_movetodialog_state");
            saveto_dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(saveto_dlg);

            if (saveto_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("移动读者记录 ...");
            stop.BeginLoop();

            EnableControls(false);
            this.listView_records.Enabled = false;

            int nCount = 0;
            try
            {
                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
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

                    string strCurrentRecPath = item.Text;

                    if (string.IsNullOrEmpty(strCurrentRecPath) == true)
                    {
                        // Debug.Assert(false, "");
                        continue;
                    }

                    string strTargetRecPath = saveto_dlg.RecPath;

                    stop.SetMessage("正在移动读者记录 '" + strCurrentRecPath + "' ...");

                    byte[] target_timestamp = null;
                    long lRet = Channel.MoveReaderInfo(
        stop,
        strCurrentRecPath,
        ref strTargetRecPath,
        out target_timestamp,
        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    Debug.Assert(string.IsNullOrEmpty(strTargetRecPath) == false, "");

                    item.Text = strTargetRecPath;   // 刷新浏览行的记录路径部分

                    stop.SetProgressValue(++i);
                    nCount++;
                }
            }
            finally
            {
                EnableControls(true);
                this.listView_records.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;
            }
            MessageBox.Show(this, "成功移动读者记录 " + nCount.ToString() + " 条");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 打印读者账簿
        // 账簿适合图书馆集中保管，用于小学低年级借书
        void menu_printReaderSheet_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            string strError = "";
            List<string> barcodes = new List<string>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                // TODO: 用 style 来识别列
                barcodes.Add(item.SubItems[1].Text);
            }

            // return:
            //      -1  出错
            //      0   用户中断
            //      1   成功
            int nRet = this.PrintReaderSheet(
                menuItem.Tag as string,
                barcodes,
                true,
                out strError);
            if (nRet != 1)
                goto ERROR1;

            // MessageBox.Show(this, "导出完成");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }


        // 导出读者详情到 Excel 文件
        void menu_exportReaderInfoToExcelFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<string> barcodes = new List<string>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                // TODO: 用 style 来识别列
                barcodes.Add(item.SubItems[1].Text);
            }

            // return:
            //      -1  出错
            //      0   用户中断
            //      1   成功
            int nRet = this.CreateReaderDetailExcelFile(barcodes,
                true,
                out strError);
            if (nRet != 1)
                goto ERROR1;

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 导出为条码号文件
        void menu_exportBarcodeFile_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的条码号文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportBarcodeFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportBarcodeFilename = dlg.FileName;

            bool bAppend = true;

            if (File.Exists(this.ExportBarcodeFilename) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "条码号文件 '" + this.ExportBarcodeFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    "ReaderSearchForm",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.No)
                    bAppend = false;
                else if (result == DialogResult.Yes)
                    bAppend = true;
                else
                {
                    Debug.Assert(false, "");
                }
            }
            else
                bAppend = false;

            // 创建文件
            using (StreamWriter sw = new StreamWriter(this.ExportBarcodeFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8))
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    sw.WriteLine(item.SubItems[1].Text);
                }

                this.Cursor = oldCursor;
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            Program.MainForm.StatusBarMessage = "读者证条码号 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportBarcodeFilename;
        }

        // 导出为记录路径文件
        void menu_exportRecPathFile_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的记录路径文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportRecPathFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportRecPathFilename = dlg.FileName;

            bool bAppend = true;

            if (File.Exists(this.ExportRecPathFilename) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "记录路径文件 '" + this.ExportRecPathFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    "ReaderSearchForm",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.No)
                    bAppend = false;
                else if (result == DialogResult.Yes)
                    bAppend = true;
                else
                {
                    Debug.Assert(false, "");
                }
            }
            else
                bAppend = false;

            // 创建文件
            using (StreamWriter sw = new StreamWriter(this.ExportRecPathFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8))
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    sw.WriteLine(item.Text);
                }

                this.Cursor = oldCursor;
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            Program.MainForm.StatusBarMessage = "读者记录路径 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportRecPathFilename;
        }

        private void listView_records_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            /*
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_records.Columns);

            // 排序
            this.listView_records.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView_records.ListViewItemSorter = null;
            */
            ListViewUtil.OnColumnClick(this.listView_records, e);

        }



        private void listView_records_SelectedIndexChanged(object sender, EventArgs e)
        {
#if NO
            API.MSG msg = new API.MSG();
            bool bRet = API.PeekMessage(ref msg, 
                this.Handle, 
                (uint)WM_SELECT_INDEX_CHANGED,
                (uint)WM_SELECT_INDEX_CHANGED, 
                0);
            if (bRet == false)
                API.PostMessage(this.Handle, WM_SELECT_INDEX_CHANGED, 0, 0);
#endif
            // this.commander.AddMessage(WM_SELECT_INDEX_CHANGED);

            OnListViewSelectedIndexChanged(sender, e);
        }

        private void textBox_queryWord_TextChanged(object sender, EventArgs e)
        {
            this.Text = "读者查询 " + this.textBox_queryWord.Text;
        }

        private void listView_records_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                string strTotal = "";
                if (this.listView_records.SelectedIndices.Count > 0)
                {
                    for (int i = 0; i < this.listView_records.SelectedIndices.Count; i++)
                    {
                        int index = this.listView_records.SelectedIndices[i];

                        ListViewItem item = this.listView_records.Items[index];
                        string strLine = Global.BuildLine(item);
                        strTotal += strLine + "\r\n";
                    }
                }
                else
                {
                    strTotal = Global.BuildLine((ListViewItem)e.Item);
                }

                this.listView_records.DoDragDrop(
                    strTotal,
                    DragDropEffects.Link);
            }
        }

        private void comboBox_matchStyle_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_matchStyle.Text == "空值")
            {
                this.textBox_queryWord.Text = "";
                this.textBox_queryWord.Enabled = false;
            }
            else
            {
                this.textBox_queryWord.Enabled = true;
            }

        }

        // 清除残余图像
        private void comboBox_readerDbName_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_readerDbName.Invalidate();
        }

        // 清除残余图像
        private void comboBox_from_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_from.Invalidate();
        }

        // 清除残余图像
        private void comboBox_matchStyle_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_matchStyle.Invalidate();
        }

        private void textBox_queryWord_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                // 回车
                case (char)Keys.Enter:
                    toolStripButton_search_Click(sender, e);
                    break;
            }
        }

        private void ToolStripMenuItem_rfc1123Single_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            dlg.RangeMode = false;
            try
            {
                dlg.Rfc1123String = this.textBox_queryWord.Text;
            }
            catch
            {
                this.textBox_queryWord.Text = "";
            }
            Program.MainForm.AppInfo.LinkFormState(dlg, "searchreaderform_gettimedialog_single");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_queryWord.Text = dlg.Rfc1123String;
        }

        private void ToolStripMenuItem_uSingle_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            dlg.RangeMode = false;
            try
            {
                dlg.uString = this.textBox_queryWord.Text;
            }
            catch
            {
                this.textBox_queryWord.Text = "";
            }

            Program.MainForm.AppInfo.LinkFormState(dlg, "searchreaderform_gettimedialog_single");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_queryWord.Text = dlg.uString;
        }

        private void ToolStripMenuItem_rfc1123Range_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            dlg.RangeMode = true;
            // 分割为两个字符串
            try
            {
                dlg.Rfc1123String = this.textBox_queryWord.Text;
            }
            catch
            {
                this.textBox_queryWord.Text = "";
            }
            Program.MainForm.AppInfo.LinkFormState(dlg, "searchreaderform_gettimedialog_range");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_queryWord.Text = dlg.Rfc1123String;
        }

        private void ToolStripMenuItem_uRange_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            dlg.RangeMode = true;
            try
            {
                dlg.uString = this.textBox_queryWord.Text;
            }
            catch
            {
                this.textBox_queryWord.Text = "";
            }

            Program.MainForm.AppInfo.LinkFormState(dlg, "searchreaderform_gettimedialog_range");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_queryWord.Text = dlg.uString;
        }

        #region 指纹缓存相关功能

        System.Windows.Forms.Label m_labelPrompt = null;

        // 整个窗口出现一个大的提示
        // parameters:
        //      strText 提示内容。如果为null，表示恢复不提示的状态
        void Prompt(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
            {
                if (m_labelPrompt != null
                    && this.Controls.IndexOf(this.m_labelPrompt) != -1)
                {
                    this.Controls.Remove(this.m_labelPrompt);
                    this.AddFreeControl(this.m_labelPrompt);    // 2015/11/7
                    this.m_labelPrompt = null;
                }
                return;
            }
            if (m_labelPrompt == null)
            {
                m_labelPrompt = new Label();
                m_labelPrompt.BackColor = SystemColors.Highlight;
                m_labelPrompt.ForeColor = SystemColors.HighlightText;
                //m_labelPrompt.BackColor = Color.White;
                //m_labelPrompt.ForeColor = Color.FromArgb(100,100,100);
                m_labelPrompt.Font = new Font(this.Font.FontFamily, (float)12, FontStyle.Bold);
                m_labelPrompt.TextAlign = ContentAlignment.MiddleCenter;
                /*
                string strFilename = PathUtil.MergePath(Program.MainForm.DataDir, "fingerprint-cache-loading.gif");
                if (File.Exists(strFilename) == true)
                {
                    m_labelPrompt.ImageAlign = ContentAlignment.TopCenter;
                    m_labelPrompt.Image = Image.FromFile(strFilename, false);
                }
                 * */
            }
            m_labelPrompt.Text = strText;
            if (this.Controls.IndexOf(this.m_labelPrompt) == -1)
            {
                m_labelPrompt.Dock = DockStyle.Fill;

                this.Controls.Add(m_labelPrompt);
                this.m_labelPrompt.PerformLayout();
                this.ResumeLayout(false);
                this.m_labelPrompt.BringToFront();
            }
            Application.DoEvents();
            this.Update();
        }

        private void ToolStripMenuItem_initFingerprintCache_Click(object sender, EventArgs e)
        {
            // return:
            //      -2  remoting服务器连接失败。指纹接口程序尚未启动
            //      -1  出错
            //      >=0   成功
            int nRet = InitFingerprintCache(false, out string strError);
            if (nRet < 0)
                goto ERROR1;

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // parameters:
        //      bDelayShow  延迟显示当前窗口。如果为 false，表示不操心显示的事儿
        // return:
        //      -2  remoting服务器连接失败。指纹接口程序尚未启动
        //      -1  出错
        //      0   成功
        /// <summary>
        /// 初始化指纹缓存
        /// </summary>
        /// <param name="bDelayShow">是否延迟显示当前窗口</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-2:  remoting服务器连接失败。指纹接口程序尚未启动</para>
        /// <para>-1:  出错</para>
        /// <para>&gt;0:   成功。返回值表示初始化的事项总数</para>
        /// </returns>
        public int InitFingerprintCache(
            bool bDelayShow,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(Program.MainForm.FingerprintReaderUrl) == true)
            {
                strError = "尚未配置 指纹阅读器接口URL 参数";
                return -1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建指纹数据缓存 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                // 清空以前的全部缓存内容，以便重新建立
                // return:
                //      -3  新版本 fingerprintcenter，不需要进行缓存初始化
                //      -2  remoting服务器连接失败。驱动程序尚未启动
                //      -1  出错
                //      >=0 实际发送给接口程序的事项数目
                int nRet = CreateFingerprintCache(null,
                    out strError);
                if (nRet == -1 || nRet == -2)
                    return nRet;
                if (nRet == -3)
                {
                    strError = "新版本 fingerprintcenter 不需要进行指纹缓存初始化";
                    return 0;
                }

                // TODO: 这里延迟显示
                if (bDelayShow == true)
                {
                    this.Opacity = 1;
                }

                this.Prompt("正在初始化指纹缓存 ...\r\n请不要关闭本窗口\r\n\r\n(在此过程中，与指纹识别无关的窗口和功能不受影响，可前往使用)\r\n");

                nRet = GetCurrentOwnerReaderNameList(
                    out List<string> readerdbnames,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (readerdbnames.Count == 0)
                {
                    strError = "因当前用户没有管辖任何读者库，初始化指纹缓存的操作无法完成";
                    return -1;
                }

                int nCount = 0;
                // 对这些读者库逐个进行高速缓存的初始化
                // 使用 特殊的 browse 格式，以便获得读者记录中的 fingerprint timestamp字符串，或者兼获得 fingerprint string
                // <fingerprint timestamp='XXXX'></fingerprint>
                foreach (string strReaderDbName in readerdbnames)
                {
                    // 初始化一个读者库的指纹缓存
                    // return:
                    //      -1  出错
                    //      >=0 实际发送给接口程序的事项数目
                    nRet = BuildOneDbCache(
                        strReaderDbName,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    nCount += nRet;
                }


                if (nCount == 0)
                {
                    strError = "因当前用户管辖的读者库 " + StringUtil.MakePathList(readerdbnames) + " 中没有任何具有指纹信息的读者记录，初始化指纹缓存的操作没有完成";
                    return -1;
                }

                return nCount;
            }
            finally
            {
                this.Prompt(null);
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }

            return 0;
        }

        // 初始化一个读者库的指纹缓存
        // return:
        //      -1  出错
        //      >=0 实际发送给接口程序的事项数目
        int BuildOneDbCache(
            string strReaderDbName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            DpResultSet resultset = null;
            bool bCreate = false;

            Hashtable timestamp_table = new Hashtable();    // recpath --> fingerprint timestamp

            string strDir = Program.MainForm.FingerPrintCacheDir;  // PathUtil.MergePath(Program.MainForm.DataDir, "fingerprintcache");
            PathUtil.TryCreateDir(strDir);

            // 结果集文件名
            string strResultsetFilename = PathUtil.MergePath(strDir, strReaderDbName);

            if (File.Exists(strResultsetFilename) == false)
            {
                resultset = new DpResultSet(false, false);
                resultset.Create(strResultsetFilename,
                    strResultsetFilename + ".index");
                bCreate = true;
            }
            else
                bCreate = false;

            // *** 第一阶段， 创建新的结果集文件；或者获取全部读者记录中的指纹时间戳

            bool bDone = false;    // 创建情形下 是否完成了写入操作
            try
            {
                /*
                long lRet = Channel.SearchReader(stop,
        strReaderDbName,
        "1-9999999999",
        -1,
        "__id",
        "left",
        this.Lang,
        null,   // strResultSetName
        "", // strOutputStyle
        out strError);
                */
                long lRet = Channel.SearchReader(stop,
strReaderDbName,
"",
-1,
"指纹时间戳",
"left",
this.Lang,
null,   // strResultSetName
"", // strOutputStyle
out strError);
                if (lRet == -1)
                {
                    if (Channel.ErrorCode == ErrorCode.AccessDenied)
                        strError = "用户 " + Channel.UserName + " 权限不足: " + strError;
                    return -1;
                }

                if (lRet == 0)
                {
                    // TODO: 这时候如果以前有结果集文件还会残留，但不会影响功能正确性，可以改进为把残留的结果集文件删除
                    return 0;
                }

                long lHitCount = lRet;
                stop.SetProgressRange(0, lHitCount);

                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }


                    lRet = Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        bCreate == true ? "id,cols,format:cfgs/browse_fingerprint" : "id,cols,format:cfgs/browse_fingerprinttimestamp",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    if (lRet == 0)
                    {
                        strError = "GetSearchResult() return 0";
                        return -1;
                    }

                    Debug.Assert(searchresults != null, "");
                    Debug.Assert(searchresults.Length > 0, "");

                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DigitalPlatform.LibraryClient.localhost.Record record = searchresults[i];
                        if (bCreate == true)
                        {
                            if (record.Cols == null || record.Cols.Length < 3)
                            {
                                strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprint";
                                return -1;
                            }
                            if (string.IsNullOrEmpty(record.Cols[0]) == true)
                                continue;   // 读者记录中没有指纹信息
                            DpRecord item = new DpRecord(record.Path);
                            // timestamp | barcode | fingerprint
                            item.BrowseText = record.Cols[0] + "|" + record.Cols[1] + "|" + record.Cols[2];
                            resultset.Add(item);
                        }
                        else
                        {
                            if (record.Cols == null || record.Cols.Length < 1)
                            {
                                strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprinttimestamp";
                                return -1;
                            }
                            if (record.Cols.Length < 2)
                            {
                                strError = "record.Cols error ... 需要刷新配置文件 cfgs/browse_fingerprinttimestamp 到最新版本";
                                return -1;
                            }
                            if (string.IsNullOrEmpty(record.Cols[0]) == true)
                                continue;   // 读者记录中没有指纹信息

                            // 记载时间戳
                            // timestamp | barcode 
                            timestamp_table[record.Path] = record.Cols[0] + "|" + record.Cols[1];
                        }
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage(strReaderDbName + " 包含记录 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                    stop.SetProgressValue(lStart);

                }

                if (bCreate == true)
                    bDone = true;

                if (bCreate == true)
                {
                    // return:
                    //      -2  remoting服务器连接失败。驱动程序尚未启动
                    //      -1  出错
                    //      >=0 实际发送给接口程序的事项数目
                    nRet = CreateFingerprintCache(resultset,
    out strError);
                    if (nRet == -1 || nRet == -2)
                        return -1;

                    return nRet;
                }
            }
            finally
            {
                if (bCreate == true)
                {
                    Debug.Assert(resultset != null, "");
                    if (bDone == true)
                    {
                        string strTemp1 = "";
                        string strTemp2 = "";
                        resultset.Detach(out strTemp1,
                            out strTemp2);
                    }
                    else
                    {
                        // 否则文件会被删除
                        resultset.Close();
                    }
                }
            }

            // 比对时间戳，更新结果集文件
            Hashtable update_table = new Hashtable();   // 需要更新的事项。recpath --> 1
            resultset = new DpResultSet(false, false);
            resultset.Attach(strResultsetFilename,
    strResultsetFilename + ".index");
            try
            {
                long nCount = resultset.Count;
                for (long i = 0; i < nCount; i++)
                {
                    DpRecord record = resultset[i];

                    string strRecPath = record.ID;
                    // timestamp | barcode 
                    string strNewTimestamp = (string)timestamp_table[strRecPath];
                    if (strNewTimestamp == null)
                    {
                        // 最新状态下，读者记录已经不存在，需要从结果集中删除
                        resultset.RemoveAt((int)i);
                        i--;
                        nCount--;
                        continue;
                    }

                    // 拆分出证条码号 2013/1/28
                    string strNewBarcode = "";
                    nRet = strNewTimestamp.IndexOf("|");
                    if (nRet != -1)
                    {
                        strNewBarcode = strNewTimestamp.Substring(nRet + 1);
                        strNewTimestamp = strNewTimestamp.Substring(0, nRet);
                    }

                    // 最新读者记录中已经没有指纹信息。例如读者记录中的指纹元素被删除了
                    if (string.IsNullOrEmpty(strNewTimestamp) == true)
                    {
                        // 删除现有事项
                        resultset.RemoveAt((int)i);
                        i--;
                        nCount--;

                        timestamp_table.Remove(strRecPath);
                        continue;
                    }

                    // 取得结果集文件中的原有时间戳字符串
                    string strText = record.BrowseText; // timestamp | barcode | fingerprint
                    nRet = strText.IndexOf("|");
                    if (nRet == -1)
                    {
                        strError = "browsetext 错误，没有 '|' 字符";
                        return -1;
                    }
                    string strOldTimestamp = strText.Substring(0, nRet);
                    // timestamp | barcode | fingerprint
                    string strOldBarcode = strText.Substring(nRet + 1);
                    nRet = strOldBarcode.IndexOf("|");
                    if (nRet != -1)
                    {
                        strOldBarcode = strOldBarcode.Substring(0, nRet);
                    }

                    // 时间戳发生变化，需要更新事项
                    if (strNewTimestamp != strOldTimestamp
                        || strNewBarcode != strOldBarcode)
                    {
                        // 如果证条码号为空，无法建立对照关系，要跳过
                        if (string.IsNullOrEmpty(strNewBarcode) == false)
                            update_table[strRecPath] = 1;

                        // 删除现有事项
                        resultset.RemoveAt((int)i);
                        i--;
                        nCount--;
                    }
                    timestamp_table.Remove(strRecPath);
                }

                // 循环结束后，timestamp_table中剩余的是当前结果集文件中没有包含的那些读者记录路径

                if (update_table.Count > 0)
                {
                    // 获取指纹信息，追加到结果集文件的尾部
                    // parameters:
                    //      update_table   key为读者记录路径
                    nRet = AppendFingerprintInfo(resultset,
                        update_table,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                // 如果服务器端新增了指纹信息,需要获取后追加到结果集文件尾部
                if (timestamp_table.Count > 0)
                {
                    // 获取指纹信息，追加到结果集文件的尾部
                    // parameters:
                    //      update_table   key为读者记录路径
                    nRet = AppendFingerprintInfo(resultset,
                        timestamp_table,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                // return:
                //      -2  remoting服务器连接失败。驱动程序尚未启动
                //      -1  出错
                //      >=0 实际发送给接口程序的事项数目
                nRet = CreateFingerprintCache(resultset,
            out strError);
                if (nRet == -1 || nRet == -2)
                    return -1;

                return nRet;
            }
            finally
            {
                string strTemp1 = "";
                string strTemp2 = "";
                resultset.Detach(out strTemp1, out strTemp2);
            }
        }

        // 获取指纹信息，追加到结果集文件的尾部
        // parameters:
        //      update_table   key为读者记录路径
        int AppendFingerprintInfo(DpResultSet resultset,
            Hashtable update_table,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 需要获得更新的事项，然后追加到结果集文件的尾部
            // 注意，需要定期彻底重建结果集文件，以便回收多余空间
            List<string> lines = new List<string>();
            foreach (string recpath in update_table.Keys)
            {
                lines.Add(recpath);
                if (lines.Count >= 100)
                {
                    List<DigitalPlatform.LibraryClient.localhost.Record> records = null;
                    nRet = GetSomeFingerprintData(lines,
    out records,
    out strError);
                    if (nRet == -1)
                        return -1;
                    foreach (DigitalPlatform.LibraryClient.localhost.Record record in records)
                    {
                        if (record.Cols == null || record.Cols.Length < 3)
                        {
                            strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprint";
                            // TODO: 并发操作的情况下，会在中途出现读者记录被别的前端修改的情况，这里似乎可以continue
                            return -1;
                        }

                        // 如果证条码号为空，无法建立对照关系，要跳过
                        if (string.IsNullOrEmpty(record.Cols[1]) == true)
                            continue;

                        DpRecord item = new DpRecord(record.Path);
                        // timestamp | barcode | fingerprint
                        item.BrowseText = record.Cols[0] + "|" + record.Cols[1] + "|" + record.Cols[2];
                        resultset.Add(item);
                    }
                    lines.Clear();
                }
            }

            if (lines.Count > 0)
            {
                List<DigitalPlatform.LibraryClient.localhost.Record> records = null;
                nRet = GetSomeFingerprintData(lines,
out records,
out strError);
                if (nRet == -1)
                    return -1;
                foreach (DigitalPlatform.LibraryClient.localhost.Record record in records)
                {
                    if (record.Cols == null || record.Cols.Length < 3)
                    {
                        strError = "record.Cols error ... 有可能是因为读者库缺乏配置文件 cfgs/browse_fingerprint";
                        // TODO: 并发操作的情况下，会在中途出现读者记录被别的前端修改的情况，这里似乎可以continue
                        return -1;
                    }
                    DpRecord item = new DpRecord(record.Path);
                    // timestamp | barcode | fingerprint
                    item.BrowseText = record.Cols[0] + "|" + record.Cols[1] + "|" + record.Cols[2];
                    resultset.Add(item);
                }
            }

            return 0;
        }

        // 处理一小批指纹数据的装入
        // parameters:
        int GetSomeFingerprintData(List<string> lines,
            out List<DigitalPlatform.LibraryClient.localhost.Record> records,
            out string strError)
        {
            strError = "";

            records = new List<DigitalPlatform.LibraryClient.localhost.Record>();
            // List<DigitalPlatform.LibraryClient.localhost.Record> records = new List<DigitalPlatform.LibraryClient.localhost.Record>();

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
                    "id,cols,format:cfgs/browse_fingerprint",
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    DialogResult temp_result = MessageBox.Show(this,
    strError + "\r\n\r\n是否重试?",
    "ReaderSearchForm",
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

            return 0;
        }

        // 获得当前帐户所管辖的读者库名字
        // 为了确保Channel自动登录，故意访问了一次服务器获得readerdbgroup定义。其实相应的定义信息在MainForm中是有的
        int GetCurrentOwnerReaderNameList(
            out List<string> readerdbnames,
            out string strError)
        {
            strError = "";
            readerdbnames = new List<string>();
            // int nRet = 0;

            // 确保登录一次
            string strValue = "";
            long lRet = Channel.GetSystemParameter(stop,
    "system",
    "readerDbGroup",
    out strValue,
    out strError);
            if (lRet == -1)
                return -1;

            // 新方法
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            try
            {
                dom.DocumentElement.InnerXml = strValue;
            }
            catch (Exception ex)
            {
                strError = "category=system,name=readerDbGroup所返回的XML片段在装入InnerXml时出错: " + ex.Message;
                return -1;
            }

            string strLibraryCodeList = this.Channel.LibraryCodeList;


            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");

            foreach (XmlNode node in nodes)
            {
                string strLibraryCode = DomUtil.GetAttr(node, "libraryCode");

                if (Global.IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                        continue;
                }

                string strDbName = DomUtil.GetAttr(node, "name");
                readerdbnames.Add(strDbName);
                /*
                bool bValue = true;
                nRet = DomUtil.GetBooleanParam(node,
                    "inCirculation",
                    true,
                    out bValue,
                    out strError);
                if (bValue == false)
                    continue;   // 跳过不参加流通的库
                 * */
            }

            return 0;
        }

        static void ParseResultItemString(string strText,
            out string strTimestamp,
            out string strBarcode,
            out string strFingerprint)
        {
            strTimestamp = "";
            strBarcode = "";
            strFingerprint = "";

            string[] parts = strText.Split(new char[] { '|' });
            if (parts.Length > 0)
                strTimestamp = parts[0];
            if (parts.Length > 1)
                strBarcode = parts[1];
            if (parts.Length > 2)
                strFingerprint = parts[2];
        }

        // return:
        //      -3  新版本 fingerprintcenter，不需要进行缓存初始化
        //      -2  remoting服务器连接失败。驱动程序尚未启动
        //      -1  出错
        //      >=0 实际发送给接口程序的事项数目
        int CreateFingerprintCache(DpResultSet resultset,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(Program.MainForm.FingerprintReaderUrl) == true)
            {
                strError = "尚未配置 指纹阅读器URL 系统参数，无法创建指纹高速缓存";
                return -1;
            }

            FingerprintChannel channel = StartFingerprintChannel(
                Program.MainForm.FingerprintReaderUrl,
                out strError);
            if (channel == null)
                return -1;

            try
            {
                GetVersionResult result = CallGetVersion(channel);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                if (StringUtil.IsInList("selfInitCache", result.CfgInfo) == true)
                    return -3;

                if (resultset == null)
                {
                    // 清空以前的全部缓存内容，以便重新建立
                    // return:
                    //      -2  remoting服务器连接失败。驱动程序尚未启动
                    //      -1  出错
                    //      0   成功
                    nRet = AddItems(
                        channel,
                        null,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == -2)
                        return -2;

                    return 0;
                }

                int nSendCount = 0;
                long nCount = resultset.Count;
                List<FingerprintItem> items = new List<FingerprintItem>();
                for (long i = 0; i < nCount; i++)
                {
                    DpRecord record = resultset[i];

                    string strTimestamp = "";
                    string strBarcode = "";
                    string strFingerprint = "";
                    ParseResultItemString(record.BrowseText,
out strTimestamp,
out strBarcode,
out strFingerprint);
                    // TODO: 注意读者证条码号为空的，不要发送出去


                    FingerprintItem item = new FingerprintItem();
                    item.ReaderBarcode = strBarcode;
                    item.FingerprintString = strFingerprint;

                    items.Add(item);
                    if (items.Count >= 100)
                    {
                        // return:
                        //      -2  remoting服务器连接失败。驱动程序尚未启动
                        //      -1  出错
                        //      0   成功
                        nRet = AddItems(
                            channel,
                            items,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == -2)
                            return -2;
                        nSendCount += items.Count;
                        items.Clear();
                    }
                }

                if (items.Count > 0)
                {
                    // return:
                    //      -2  remoting服务器连接失败。驱动程序尚未启动
                    //      -1  出错
                    //      0   成功
                    nRet = AddItems(
                        channel,
                        items,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == -2)
                        return -2;
                    nSendCount += items.Count;
                }

                // Console.Beep(); // 表示读取成功
                return nSendCount;
            }
            finally
            {
                EndFingerprintChannel(channel);
            }
        }

#if NO
        // return:
        //      -2  remoting服务器连接失败。驱动程序尚未启动
        //      -1  出错
        //      0   成功
        int AddItems(List<FingerprintItem> items,
            out string strError)
        {
            strError = "";

            try
            {
                int nRet = m_fingerPrintObj.AddItems(items,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            // [System.Runtime.Remoting.RemotingException] = {"连接到 IPC 端口失败: 系统找不到指定的文件。\r\n "}
            catch (System.Runtime.Remoting.RemotingException ex)
            {
                strError = "针对 " + Program.MainForm.FingerprintReaderUrl + " 的 AddItems() 操作失败: " + ex.Message;
                return -2;
            }
            catch (Exception ex)
            {
                strError = "针对 " + Program.MainForm.FingerprintReaderUrl + " 的 AddItems() 操作失败: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        IpcClientChannel m_fingerPrintChannel = new IpcClientChannel();
        IFingerprint m_fingerPrintObj = null;

        int StartFingerprintChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            if (this.m_fingerPrintChannel == null)
                this.m_fingerPrintChannel = new IpcClientChannel(Guid.NewGuid().ToString(), // 随机的名字，令多个 Channel 对象可以并存 
                    new BinaryClientFormatterSinkProvider());

            ChannelServices.RegisterChannel(m_fingerPrintChannel, false);

            try
            {
                m_fingerPrintObj = (IFingerprint)Activator.GetObject(typeof(IFingerprint),
                    strUrl);
                if (m_fingerPrintObj == null)
                {
                    strError = "无法连接到服务器 " + strUrl;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                EndFingerprintChannel();

                strError = "StartFingerprintChannel() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        void EndFingerprintChannel()
        {
            if (this.m_fingerPrintChannel != null)
            {
                ChannelServices.UnregisterChannel(m_fingerPrintChannel);
                this.m_fingerPrintChannel = null;
            }
        }

#endif

        #endregion

        #region 属性区有关功能

        internal override bool InSearching
        {
            get
            {
                if (this.comboBox_from.Enabled == true)
                    return false;
                return true;
            }
        }





        static string MergeXml(string strXml1,
    string strXml2)
        {
            if (string.IsNullOrEmpty(strXml1) == true)
                return strXml2;
            if (string.IsNullOrEmpty(strXml2) == true)
                return strXml1;

            return strXml1; // 临时这样
        }

        internal override string GetHeadString(bool bAjax = true)
        {
            string strCssFilePath = PathUtil.MergePath(Program.MainForm.DataDir, "operloghtml.css");

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

        internal static int GetXmlHtml(BiblioInfo info,
    out string strXml,
    out string strHtml2,
    out string strError)
        {
            strError = "";
            strXml = "";
            strHtml2 = "";

            string strOldXml = "";
            string strNewXml = "";

            int nRet = 0;

            strOldXml = info.OldXml;
            strNewXml = info.NewXml;

            if (string.IsNullOrEmpty(strOldXml) == false
                && string.IsNullOrEmpty(strNewXml) == false)
            {
                // 创建展示两个 MARC 记录差异的 HTML 字符串
                // return:
                //      -1  出错
                //      0   成功
                nRet = MarcDiff.DiffXml(
                    strOldXml,
                    strNewXml,
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else if (string.IsNullOrEmpty(strOldXml) == false
    && string.IsNullOrEmpty(strNewXml) == true)
            {
                strHtml2 = MarcUtil.GetHtmlOfXml(strOldXml,
                    false);
            }
            else if (string.IsNullOrEmpty(strOldXml) == true
                && string.IsNullOrEmpty(strNewXml) == false)
            {
                strHtml2 = MarcUtil.GetHtmlOfXml(strNewXml,
                    false);
            }

            strXml = MergeXml(strOldXml, strNewXml);
            return 0;
        }

        #endregion

        #region C# 脚本程序



        // 创建一个新的 C# 脚本文件
        void menu_createMarcQueryCsFile_Click(object sender, EventArgs e)
        {
#if NO
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的脚本文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = "";
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "C#脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                PatronHost.CreateStartCsFile(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }

            System.Diagnostics.Process.Start("notepad.exe", dlg.FileName);

            this.m_strUsedMarcQueryFilename = dlg.FileName;
#endif
            CreateMarcQueryCsFile();
        }

        // 创建一个新的 C# 脚本文件
        /// <summary>
        /// 创建一个新的 C# 脚本文件。会弹出对话框询问文件名。
        /// 代码中的类从 PatronHost 类派生
        /// </summary>
        public void CreateMarcQueryCsFile()
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的脚本文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = "";
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "C#脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                PatronHost.CreateStartCsFile(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                return;
            }

            System.Diagnostics.Process.Start("notepad.exe", dlg.FileName);

            this.m_strUsedMarcQueryFilename = dlg.FileName;
        }

        void menu_quickMarcQueryRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要执行 C# 脚本的事项";
                goto ERROR1;
            }

            // 读者信息缓存
            // 如果已经初始化，则保持
            if (this.m_biblioTable == null)
                this.m_biblioTable = new Hashtable();

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定 C# 脚本文件";
            dlg.FileName = this.m_strUsedMarcQueryFilename;
            dlg.Filter = "C# 脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedMarcQueryFilename = dlg.FileName;

            PatronHost host = null;
            Assembly assembly = null;

            nRet = PrepareMarcQuery(this.m_strUsedMarcQueryFilename,
                out assembly,
                out host,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            {
                host.MainForm = Program.MainForm;
                host.UiForm = this;
                host.RecordPath = "";
                host.PatronDom = null;
                host.Changed = false;
                host.UiItem = null;

                StatisEventArgs args = new StatisEventArgs();
                host.OnInitial(this, args);
                if (args.Continue == ContinueType.SkipAll)
                    return;
                if (args.Continue == ContinueType.Error)
                {
                    strError = args.ParamString;
                    goto ERROR1;
                }
            }

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行脚本 " + dlg.FileName + "</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在针对读者记录执行 C# 脚本 ...");
            stop.BeginLoop();

            this.EnableControls(false);

            this.listView_records.Enabled = false;
            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                host.CodeFileName = this.m_strUsedMarcQueryFilename;
                {
                    host.MainForm = Program.MainForm;
                    host.RecordPath = "";
                    host.PatronDom = null;
                    host.Changed = false;
                    host.UiItem = null;

                    StatisEventArgs args = new StatisEventArgs();
                    host.OnBegin(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        return;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
                }

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }

                ListViewPatronLoader loader = new ListViewPatronLoader(this.Channel,
                    stop,
                    items,
                    this.m_biblioTable);
                loader.DbTypeCaption = this.DbTypeCaption;

                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    stop.SetProgressValue(i);

                    BiblioInfo info = item.BiblioInfo;

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    host.MainForm = Program.MainForm;
                    host.RecordPath = info.RecPath;
                    host.PatronDom = new XmlDocument();
                    host.PatronDom.LoadXml(info.OldXml);
                    host.Changed = false;
                    host.UiItem = item.ListViewItem;

                    StatisEventArgs args = new StatisEventArgs();
                    host.OnRecord(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        break;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }

                    if (host.Changed == true)
                    {
                        string strXml = host.PatronDom.OuterXml;
                        if (info != null)
                        {
                            if (string.IsNullOrEmpty(info.NewXml) == true)
                                this.m_nChangedCount++;
                            info.NewXml = strXml;
                        }

                        item.ListViewItem.BackColor = SystemColors.Info;
                        item.ListViewItem.ForeColor = SystemColors.InfoText;
                    }

                    // 显示为工作单形式
                    i++;
                }

                {
                    host.MainForm = Program.MainForm;
                    host.RecordPath = "";
                    host.PatronDom = null;
                    host.Changed = false;
                    host.UiItem = null;

                    StatisEventArgs args = new StatisEventArgs();
                    host.OnEnd(this, args);
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "执行 C# 脚本的过程中出现异常: " + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                if (host != null)
                    host.FreeResources();

                this.listView_records.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControls(true);

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行脚本 " + dlg.FileName + "</div>");
            }

            DoViewComment(false);
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 丢弃选定的修改
        void menu_clearSelectedChangedRecords_Click(object sender, EventArgs e)
        {
#if NO
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项可丢弃");
                return;
            }
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    ClearOneChange(item);
                }
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            DoViewComment(false);
#endif
            ClearSelectedChangedRecords();
        }





        // 丢弃全部修改
        void menu_clearAllChangedRecords_Click(object sender, EventArgs e)
        {
#if NO
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项可丢弃");
                return;
            }
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in this.listView_records.Items)
                {
                    ClearOneChange(item);
                }
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            DoViewComment(false);
#endif
            ClearAllChangedRecords();
        }

        // 保存选定事项的修改
        void menu_saveSelectedChangedRecords_Click(object sender, EventArgs e)
        {
            // this._forceSave = Control.ModifierKeys == Keys.Control;
            SaveSelectedChangedRecords(Control.ModifierKeys == Keys.Control ? "force" : "");
        }

        // 保存全部修改事项
        void menu_saveAllChangedRecords_Click(object sender, EventArgs e)
        {
            // this._forceSave = Control.ModifierKeys == Keys.Control;
            SaveAllChangedRecords(Control.ModifierKeys == Keys.Control ? "force" : "");
        }


        // 准备脚本环境
        int PrepareMarcQuery(string strCsFileName,
            out Assembly assembly,
            out PatronHost host,
            out string strError)
        {
            assembly = null;
            strError = "";
            host = null;


            string strContent = "";
            Encoding encoding;
            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // parameters:
            //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
            // return:
            //      -1  出错 strError中有返回值
            //      0   文件不存在 strError中有返回值
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(strCsFileName,
                -1,
                out strContent,
                out encoding,
                out strError);
            if (nRet == -1)
                return -1;

            string strWarningInfo = "";
            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",

                                    Environment.CurrentDirectory + "\\digitalplatform.core.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
                                       Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",

                                    Environment.CurrentDirectory + "\\digitalplatform.Script.dll",  // 2011/8/25 新增
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
                Environment.CurrentDirectory + "\\dp2circulation.exe",
            };

            // 2013/12/16
            nRet = ScriptManager.GetRef(strCsFileName,
    ref saAddRef,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            // 直接编译到内存
            // parameters:
            //		refs	附加的refs文件路径。路径中可能包含宏%installdir%
            nRet = ScriptManager.CreateAssembly_1(strContent,
                saAddRef,
                "",
                out assembly,
                out strError,
                out strWarningInfo);
            if (nRet == -1)
                goto ERROR1;

            // 得到Assembly中Host派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "dp2Circulation.PatronHost");
            if (entryClassType == null)
            {
                strError = strCsFileName + " 中没有找到 dp2Circulation.PatronHost 派生类";
                goto ERROR1;
            }

            // new一个Host派生对象
            host = (PatronHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            return 0;
            ERROR1:
            return -1;
        }

        #endregion

        // 创建读者详情 Excel 文件。这是便于被外部调用的版本，只需要提供读者 XML 记录即可
        // return:
        //      -1  出错
        //      0   用户中断
        //      1   成功
        public static int CreateReaderDetailExcelFile(List<string> xmls,
            Delegate_GetBiblioSummary procGetBiblioSummary,
            Stop stop,
            bool bAdvanceXml,
            bool bLaunchExcel,
            out string strError)
        {
            strError = "";
            //int nRet = 0;

            ExportPatronExcelDialog dlg = new ExportPatronExcelDialog();
            MainForm.SetControlFont(dlg, Program.MainForm.Font, false);
            dlg.UiState = Program.MainForm.AppInfo.GetString(
        "ReaderSearchForm",
        "ExportPatronExcelDialog_uiState",
        "");

            Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderSearchForm_ExportPatronExcelDialog_uiState_state");
            dlg.ShowDialog(Program.MainForm);

            Program.MainForm.AppInfo.SetString(
"ReaderSearchForm",
"ExportPatronExcelDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                strError = "放弃操作";
                return 0;
            }

            string strTimeRange = "";

            try
            {
                strTimeRange = GetTimeRange(dlg.ChargingHistoryDateRange);
            }
            catch (Exception ex)
            {
                strError = "日期范围字符串 '" + dlg.ChargingHistoryDateRange + "' 格式不合法: " + ex.Message;
                return -1;
            }

            XLWorkbook doc = null;

            try
            {
                doc = new XLWorkbook(XLEventTracking.Disabled);
                File.Delete(dlg.FileName);
            }
            catch (Exception ex)
            {
                strError = "ReaderSearchForm new XLWorkbook() {0BD1CB34-DF8A-4DDB-B884-8A9CF830D7C7} exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始导出读者详情</div>");

            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("表格");

            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, xmls.Count);

                // 每个列的最大字符数
                List<int> column_max_chars = new List<int>();

                // TODO: 表的标题，创建时间

                int nRowIndex = 3;  // 空出前两行
                //int nColIndex = 1;

                int nReaderIndex = 0;
                foreach (string strXml in xmls)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                        return 0;

                    if (string.IsNullOrEmpty(strXml) == true)
                        continue;

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载读者记录 XML 到 DOM 时发生错误: " + ex.Message;
                        return -1;
                    }

                    try
                    {
                        // 过滤读者记录
                        // parameters:
                        //      filtering   过滤特征。空表示不过滤。amerce,borrowing,overdue
                        // return:
                        //      true 表示通过了过滤
                        //      false   表示没有通过过滤
                        // exception:
                        //      可能会抛出 Exception 异常
                        if (FilterPatron(dom, dlg.Filtering) == false)
                        {
                            Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode($"读者记录 {strXml} 不符合过滤条件 '{dlg.Filtering}'，被跳过") + "</div>");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = $"处理读者记录 {strXml} 时出现异常: {ex.Message}";
                        return -1;
                    }

                    string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");

                    // 
                    if (dlg.ExportReaderInfo)
                    {
                        OutputReaderInfo(sheet,
                dom,
                nReaderIndex,
                dlg.PrintReaderBarcodeLabel ? "barcode" : "",
                ref nRowIndex,
                ref column_max_chars);
                    }

                    // 输出在借册表格
                    if (dlg.ExportBorrowInfo)
                    {
                        OutputBorrows(sheet,
                dom,
                procGetBiblioSummary,
                bAdvanceXml,
                ref nRowIndex,
                ref column_max_chars);
                    }

                    // 输出违约金表格
                    if (dlg.ExportOverdueInfo)
                    {
                        OutputOverdues(sheet,
                dom,
                procGetBiblioSummary,
                ref nRowIndex,
                ref column_max_chars);
                    }

                    if (dlg.ExportChargingHistory)
                    {
                        LibraryChannel channel = Program.MainForm.GetChannel();
                        try
                        {
                            ChargingHistoryLoader history_loader = new ChargingHistoryLoader();
                            history_loader.Channel = channel;
                            history_loader.Stop = stop;
                            history_loader.PatronBarcode = strBarcode;
                            history_loader.TimeRange = strTimeRange;
                            history_loader.Actions = "return,lost";
                            history_loader.Order = "descending";

                            CacheableBiblioLoader summary_loader = new CacheableBiblioLoader();
                            summary_loader.Channel = channel;
                            summary_loader.Stop = stop;
                            summary_loader.Format = "summary";
                            summary_loader.GetBiblioInfoStyle = GetBiblioInfoStyle.None;
                            // summary_loader.RecPaths = biblio_recpaths;

                            // 输出借阅历史表格
                            // 可能会抛出异常，例如权限不够
                            OutputBorrowHistory(sheet,
                    dom,
                    history_loader,
                    // Program.MainForm.GetBiblioSummary,
                    summary_loader,
                    ref nRowIndex,
                    ref column_max_chars);
                        }
                        catch (Exception ex)
                        {
                            strError = "输出借阅历史时出现异常: " + ex.Message;
                            return -1;
                        }
                        finally
                        {
                            Program.MainForm.ReturnChannel(channel);
                        }
                    }

                    nRowIndex++;    // 读者之间的空行

                    nReaderIndex++;
                    if (stop != null)
                        stop.SetProgressValue(nReaderIndex);
                }

                {
                    if (stop != null)
                        stop.SetMessage("正在调整列宽度 ...");
                    Application.DoEvents();

                    // 字符数太多的列不要做 width auto adjust
                    foreach (IXLColumn column in sheet.Columns())
                    {
                        int MAX_CHARS = 50;   // 60

                        int nIndex = column.FirstCell().Address.ColumnNumber - 1;
                        if (nIndex >= column_max_chars.Count)
                            break;
                        int nChars = column_max_chars[nIndex];

                        if (nIndex == 1)
                        {
                            column.Width = 10;
                            continue;
                        }

                        if (nIndex == 3)
                            MAX_CHARS = 50;
                        else
                            MAX_CHARS = 24;

                        if (nChars < MAX_CHARS)
                            column.AdjustToContents();
                        else
                            column.Width = Math.Min(MAX_CHARS, nChars);
                    }
                }
            }
            finally
            {
                if (doc != null)
                {
                    doc.SaveAs(dlg.FileName);
                    doc.Dispose();
                }

                if (bLaunchExcel)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(dlg.FileName);
                    }
                    catch
                    {

                    }
                }

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束导出读者详情</div>");
            }
            return 1;
        }

        // return:
        //      -1  出错
        //      0   用户中断
        //      1   成功
        public int ExportChargingHistoryToItemSearchForm(List<string> reader_barcodes,
            out string strError)
        {
            strError = "";
            //int nRet = 0;

            ItemSearchForm form = Program.MainForm.OpenItemSearchForm("item");
            form.Enabled = false;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("导出读者借阅历史 ...");
            stop.BeginLoop();

            EnableControls(false);
            this.listView_records.Enabled = false;
            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, reader_barcodes.Count);

                int nReaderIndex = 0;
                foreach (string strBarcode in reader_barcodes)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return 0;
                    }

                    if (string.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    // 获得读者记录
                    byte[] baTimestamp = null;
                    string strOutputRecPath = "";

                    stop.SetMessage("正在处理读者记录 " + strBarcode + " ...");

                    string[] results = null;
                    long lRet = Channel.GetReaderInfo(
                        stop,
                        strBarcode,
                        "advancexml",
                        out results,
                        out strOutputRecPath,
                        out baTimestamp,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    if (lRet == 0)
                        return -1;

                    if (lRet > 1)   // 不可能发生吧?
                    {
                        strError = "读者证条码号 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，放弃装入读者记录。\r\n\r\n注意这是一个严重错误，请系统管理员尽快排除。";
                        return -1;
                    }
                    if (results == null || results.Length < 1)
                    {
                        strError = "返回的results不正常。";
                        return -1;
                    }
                    string strXml = results[0];

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载读者记录 XML 到 DOM 时发生错误: " + ex.Message;
                        return -1;
                    }

                    try
                    {
                        ChargingHistoryLoader history_loader = new ChargingHistoryLoader();
                        history_loader.Channel = this.Channel;
                        history_loader.Stop = this.stop;
                        history_loader.PatronBarcode = strBarcode;
                        history_loader.TimeRange = "~"; // strTimeRange;
                        history_loader.Actions = "return,lost";
                        history_loader.Order = "descending";

                        ItemBarcodeLoader barcode_loader = new ItemBarcodeLoader();
                        barcode_loader.Channel = this.Channel;
                        barcode_loader.Stop = this.stop;

                        // 输出借阅历史表格
                        int nRet = OutputBorrowHistory(form,
                            dom,
                history_loader,
                barcode_loader,
               out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "输出借阅历史时出现异常: " + ex.Message;
                        return -1;
                    }

                    nReaderIndex++;
                    if (stop != null)
                        stop.SetProgressValue(nReaderIndex);
                }
            }
            finally
            {
                EnableControls(true);
                this.listView_records.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                form.Enabled = true;
            }
            return 1;
        }

        // 将一个读者的操作历史输出到实体查询窗
        // parameters:
        static int OutputBorrowHistory(
            ItemSearchForm form,
            XmlDocument reader_dom,
            ChargingHistoryLoader history_loader,
            ItemBarcodeLoader barcode_loader,
            out string strError)
        {
            strError = "";

            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.67") < 0)
            {
                strError = "输出操作历史到实体查询窗要求 dp2library 为 2.67 或以上版本";
                return -1;
            }

            List<string> barcodes = new List<string>();
            foreach (ChargingItemWrapper wrapper in history_loader)
            {
                ChargingItem item = wrapper.Item;

                string strItemBarcode = item.ItemBarcode;
#if NO
                ChargingItem rel = wrapper.RelatedItem;
                string strBorrowDate = rel == null ? "" : rel.OperTime;
                string strBorrowPeriod = GetDisplayTimePeriodString(rel == null ? "" : rel.Period);
                string strReturnDate = item.OperTime;
#endif

                barcodes.Add(strItemBarcode);
            }

            string strTempFileName = Program.MainForm.GetTempFileName("exphis");
            try
            {
                int nCount = 0;
                using (StreamWriter sw = new StreamWriter(strTempFileName, false, Encoding.UTF8))
                {
                    barcode_loader.Barcodes = barcodes;
                    foreach (EntityItem info in barcode_loader)
                    {
                        string strRecPath = info.RecPath;
                        if (string.IsNullOrEmpty(strRecPath) == true)
                            continue;   // TODO: 是否要警告?
                        sw.WriteLine(strRecPath);
                        nCount++;
                    }
                }

                if (nCount > 0)
                {
                    int nRet = form.ImportFromRecPathFile(strTempFileName,
                        "",
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                return 0;
            }
            finally
            {
                File.Delete(strTempFileName);
            }
        }

        public delegate bool Delegate_processPatron(string strRecPath,
            XmlDocument dom,
            byte[] timestamp);

        // 通用的，遍历和处理读者记录的函数
        // 该函数的弱点是不能按照特殊风格来获取读者记录，而 ProcessPatrons() 能做到
        // 优点是速度特别快
        // return:
        //      -1  出错。包括用户中断的情况
        //      >=0 实际处理的读者记录数
        public int ProcessSelectedPatrons(
            Delegate_processPatron func,
            out string strError)
        {
            strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要处理的读者记录事项";
                return -1;
            }

            // 读者信息缓存
            // 如果已经初始化，则保持
            if (this.m_biblioTable == null)
                this.m_biblioTable = new Hashtable();

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始进行读者记录校验</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在处理读者记录 ...");
            stop.BeginLoop();

            this.EnableControls(false);

            this.listView_records.Enabled = false;
            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }

                ListViewPatronLoader loader = new ListViewPatronLoader(this.Channel,
                    stop,
                    items,
                    this.m_biblioTable);
                loader.DbTypeCaption = this.DbTypeCaption;

                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    stop.SetProgressValue(i);

                    BiblioInfo info = item.BiblioInfo;

                    // Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(info.OldXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载读者记录 XML 到 DOM 时发生错误: " + ex.Message;
                        return -1;
                    }

                    if (func != null)
                    {
                        if (func(info.RecPath,
                dom,
                info.Timestamp) == false)
                            break;
                    }

                    i++;
                }

                return i;
            }
            catch (Exception ex)
            {
                strError = "处理读者记录的过程中出现异常: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                this.listView_records.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControls(true);

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行读者记录处理</div>");
            }
        }

        // 通用的，遍历和处理读者记录的函数
        // 该函数的弱点是速度慢，优点是能按照特殊风格来获取读者记录
        // return:
        //      -1  出错。包括用户中断的情况
        //      >=0 实际处理的读者记录数
        public int ProcessPatrons(List<string> reader_barcodes,
            string strGetReaderInfoStyle,
            Delegate_processPatron func,
            out string strError)
        {
            strError = "";

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在处理读者记录 ...");
            stop.BeginLoop();

            EnableControls(false);
            this.listView_records.Enabled = false;

            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, reader_barcodes.Count);

                int nReaderIndex = 0;
                foreach (string strBarcode in reader_barcodes)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    if (string.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    // 获得读者记录
                    byte[] baTimestamp = null;
                    string strOutputRecPath = "";

                    stop.SetMessage("正在处理读者记录 " + strBarcode + " ...");

                    string[] results = null;
                    long lRet = Channel.GetReaderInfo(
                        stop,
                        strBarcode,
                        string.IsNullOrEmpty(strGetReaderInfoStyle) ?
                        "advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary"
                        : strGetReaderInfoStyle, // advancexml_history_bibliosummary
                        out results,
                        out strOutputRecPath,
                        out baTimestamp,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    if (lRet == 0)
                        return -1;

                    if (lRet > 1)   // 不可能发生吧?
                    {
                        strError = "读者证条码号 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，放弃装入读者记录。\r\n\r\n注意这是一个严重错误，请系统管理员尽快排除。";
                        return -1;
                    }
                    if (results == null || results.Length < 1)
                    {
                        strError = "返回的results不正常。";
                        return -1;
                    }

                    string strXml = results[0];

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载读者记录 XML 到 DOM 时发生错误: " + ex.Message;
                        return -1;
                    }

                    if (func != null)
                    {
                        if (func(strOutputRecPath,
                dom,
                baTimestamp) == false)
                            break;
                    }

                    nReaderIndex++;
                    if (stop != null)
                        stop.SetProgressValue(nReaderIndex);
                }

                return nReaderIndex;    // 实际处理的读者记录数
            }
            catch (ChannelException ex)
            {
                // 2017/4/27
                strError = ex.Message;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "ProcessPatrons() 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
            }
            finally
            {
                EnableControls(true);
                this.listView_records.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;
            }
        }

        static int GetCardPhotoFile(LibraryChannel channel,
            Stop stop,
            string strResPath,
            string strLocalFilePath,
            out string strError)
        {
            strError = "";

            byte[] baOutputTimeStamp = null;
            string strMetaData = "";
            string strTempOutputPath = "";

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 0, 60);

            try
            {
                long lRet = channel.GetRes(
                    stop,
                    strResPath,
                    strLocalFilePath,
                    "content,data,metadata,timestamp,outputpath,gzip",  // 2017/10/7 增加 gzip
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strTempOutputPath,
                    out strError);
                return (int)lRet;
            }
            finally
            {
                channel.Timeout = old_timeout;
            }
        }

        // 2017/5/8
        // 从读者记录 XML 中获得读者卡片头像的路径。例如 "读者/1/object/0"
        static string GetCardPhotoPath(XmlDocument readerdom,
            string strRecPath)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("//dprms:file[@usage='cardphoto']", nsmgr);

            if (nodes.Count == 0)
                return null;

            string strID = DomUtil.GetAttr(nodes[0], "id");
            if (string.IsNullOrEmpty(strID) == true)
                return null;

            string strResPath = strRecPath + "/object/" + strID;
            return strResPath.Replace(":", "/");
        }

        // 创建读者账簿
        // parameters:
        //      strSheetDefName 打印方案名称
        // return:
        //      -1  出错
        //      0   用户中断
        //      1   成功
        public int PrintReaderSheet(
            string strSheetDefName,
            List<string> reader_barcodes,
            bool bLaunchExcel,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            PrintReaderSheetDialog dlg = new PrintReaderSheetDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.UiState = Program.MainForm.AppInfo.GetString(
        "ReaderSearchForm",
        "PrintReaderSheetDialog_uiState",
        "");

            Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderSearchForm_PrintReaderSheetDialog_state");
            dlg.ShowDialog(this);

            Program.MainForm.AppInfo.SetString(
"ReaderSearchForm",
"PrintReaderSheetDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                strError = "放弃操作";
                return 0;
            }

            ReaderSheetCollection sheets = new ReaderSheetCollection();
            string strTempDataFileName = Path.Combine(Program.MainForm.UserTempDir, "~readersheetdata.txt");

            //      strTemplate 模板文件内容。如果为 null 表示不使用模板文件
            string strTemplate = "";

            {

                string strTemplateFileName = Path.Combine(Program.MainForm.DataDir, "patronSheetLayout\\" + strSheetDefName + ".template");
                if (File.Exists(strTemplateFileName) == false)
                    strTemplateFileName = Path.Combine(Program.MainForm.UserDir, "patronSheetLayout\\" + strSheetDefName + ".template");

                if (File.Exists(strTemplateFileName) == false)
                    strTemplate = null;
                else
                {
                    // 根据模板打印
                    // 能自动识别文件内容的编码方式的读入文本文件内容模块
                    // return:
                    //      -1  出错
                    //      0   文件不存在
                    //      1   文件存在
                    nRet = Global.ReadTextFileContent(strTemplateFileName,
                        out strTemplate,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }

            try
            {
                int nReaderCount = 0;
                using (StreamWriter sw = new StreamWriter(strTempDataFileName, false, Encoding.UTF8))
                {
                    // return:
                    //      -1  出错。包括用户中断的情况
                    //      >=0 实际处理的读者记录数
                    nRet = this.ProcessPatrons(
                        reader_barcodes,
                        "", // advancexml_history_bibliosummary
                        (strRecPath, dom, timestamp) =>
                        {
                            this.ShowMessage("正在处理读者记录 " + strRecPath);


                            // 下载读者照片
                            string strCardPhotoPath = "";
                            {
                                string strError0 = "";
                                string strObjectPath = GetCardPhotoPath(dom, strRecPath);
                                if (string.IsNullOrEmpty(strObjectPath) == false)
                                {
                                    string strLocalFilePath = Path.Combine(Program.MainForm.UserTempDir, "~cp_" + Guid.NewGuid().ToString());
                                    LibraryChannel channel = this.GetChannel();
                                    try
                                    {
                                        int nRet0 = GetCardPhotoFile(channel,
                        stop,
                        strObjectPath,
                        strLocalFilePath,
                        out strError0);
                                        if (nRet0 == -1)
                                        {
                                            MessageBox.Show(this, strError0);
                                            // 删除临时文件
                                        }
                                        strCardPhotoPath = Path.GetFileName(strLocalFilePath);
                                    }
                                    finally
                                    {
                                        this.ReturnChannel(channel);
                                    }
                                }
                            }

                            string strDepartment = DomUtil.GetElementText(dom.DocumentElement, "department");

                            if (dlg.GroupByDepartment == false)
                            {
                                sheets.AddItem("", dom.OuterXml, strCardPhotoPath);
                            }
                            else
                            {
                                sheets.AddItem(strDepartment, dom.OuterXml, strCardPhotoPath);
                            }


                            nReaderCount++;
                            return true;
                        },
                        out strError);
                    if (nRet == -1)
                        return -1;

                    {
                        foreach (ReaderSheetInfo info in sheets)
                        {
                            if (string.IsNullOrEmpty(strTemplate))
                                info.Output(sw, strSheetDefName);
                            else
                                info.OutputByTemplate(sw, strTemplate);

                            if (sheets.IsTail(info) == false)   // 最后一个元素末尾不需要换页
                            {
                                sw.WriteLine("{newPage}");  // 换页命令
                                sw.WriteLine("***");
                            }
                        }
                    }

                    this.ShowMessage("共处理读者记录 " + nReaderCount + " 个", "green", true);
                }

                LabelPrintForm labelPrintForm = Program.MainForm.EnsureLabelPrintForm();
                // labelPrintForm.LabelDefFilename = Path.Combine(Program.MainForm.DataDir, "reader\\patronSheetLayout_" + strSheetDefName + ".xml");
                labelPrintForm.LabelDefFilename = GetLabelDefFileName(strSheetDefName);
                labelPrintForm.LabelFilename = strTempDataFileName;
                labelPrintForm.MdiParent = Program.MainForm;
                labelPrintForm.Show();
            }
            catch (Exception ex)
            {
                strError = "CreateSheetExcelFile() 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
            }
            finally
            {

            }

            // TODO: sheet 可以按照单位来区分。例如按照班级
            return 1;
        }

        string GetLabelDefFileName(string strSheetDefName)
        {
            string filename = Path.Combine(Program.MainForm.DataDir, "patronSheetLayout\\" + strSheetDefName + ".xml");
            if (File.Exists(filename) == false)
                filename = Path.Combine(Program.MainForm.UserDir, "patronSheetLayout\\" + strSheetDefName + ".xml");
            return filename;
        }

#if NO
        // 创建读者账簿
        // return:
        //      -1  出错
        //      0   用户中断
        //      1   成功
        public int CreateReaderSheetExcelFile(List<string> reader_barcodes,
            bool bLaunchExcel,
            out string strError)
        {
            strError = "";
            //int nRet = 0;

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            // dlg.FileName = this.ExportExcelFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return 0;

            XLWorkbook doc = null;
            try
            {
                doc = new XLWorkbook(XLEventTracking.Disabled);
                File.Delete(dlg.FileName);
            }
            catch (Exception ex)
            {
                strError = "ReaderSearchForm new XLWorkbook() {} exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("表格");

            // 每个列的最大字符数
            List<int> column_max_chars = new List<int>();

            // TODO: 表的标题，创建时间

            int nRowIndex = 3;  // 空出前两行
            //int nColIndex = 1;

            int nReaderIndex = 0;

            try
            {
                // return:
                //      -1  出错。包括用户中断的情况
                //      >=0 实际处理的读者记录数
                int nRet = this.ProcessPatrons(
                    reader_barcodes,
                    "advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary", // advancexml_history_bibliosummary
                    (strRecPath, dom, timestamp) =>
                    {
                        this.ShowMessage("正在处理读者记录 " + strRecPath);

                        string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");

                        OutputReaderSheet2(sheet,
                dom,
                nReaderIndex,
                "barcode",
                ref nRowIndex,
                ref column_max_chars);

                        nRowIndex++;    // 读者之间的空行

                        nReaderIndex++;
                        return true;
                    },
                    out strError);
                if (nRet == -1)
                    return -1;

                {
                    if (stop != null)
                        stop.SetMessage("正在调整列宽度 ...");
                    Application.DoEvents();

                    //double char_width = GetAverageCharPixelWidth(list);

                    // 字符数太多的列不要做 width auto adjust
                    foreach (IXLColumn column in sheet.Columns())
                    {
                        int MAX_CHARS = 50;   // 60

                        int nIndex = column.FirstCell().Address.ColumnNumber - 1;
                        if (nIndex >= column_max_chars.Count)
                            break;
                        int nChars = column_max_chars[nIndex];

                        if (nIndex == 1)
                        {
                            column.Width = 10;
                            continue;
                        }

                        if (nIndex == 3)
                            MAX_CHARS = 50;
                        else
                            MAX_CHARS = 24;

                        if (nChars < MAX_CHARS)
                            column.AdjustToContents();
                        else
                            column.Width = Math.Min(MAX_CHARS, nChars);

                        //else
                        //    column.Width = (double)list.Columns[i].Width / char_width;  // Math.Min(MAX_CHARS, nChars);
                    }
                }

                this.ShowMessage("共导出读者记录 " + nReaderIndex + " 个", "green", true);
            }
            catch (Exception ex)
            {
                strError = "CreateSheetExcelFile() 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
            }
            finally
            {
                if (stop != null)
                    stop.SetMessage("");

                if (doc != null)
                {
                    doc.SaveAs(dlg.FileName);
                    doc.Dispose();
                }

                if (bLaunchExcel)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(dlg.FileName);
                    }
                    catch
                    {

                    }
                }
            }

            // TODO: sheet 可以按照单位来区分。例如按照班级
            return 1;
        }

#endif

#if NO
        // return:
        //      -1  出错
        //      0   用户中断
        //      1   成功
        public int CreateReaderDetailExcelFile(List<string> reader_barcodes,
            bool bLaunchExcel,
            out string strError)
        {
            strError = "";
            //int nRet = 0;

            ExportPatronExcelDialog dlg = new ExportPatronExcelDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.OverwritePrompt = true;
            dlg.UiState = Program.MainForm.AppInfo.GetString(
        "ReaderSearchForm",
        "ExportPatronExcelDialog_uiState",
        "");

            Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderSearchForm_ExportPatronExcelDialog_uiState_state");
            dlg.ShowDialog(this);

            Program.MainForm.AppInfo.SetString(
"ReaderSearchForm",
"ExportPatronExcelDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                strError = "放弃操作";
                return 0;
            }

#if NO
            // 提醒覆盖文件
            if (File.Exists(dlg.FileName) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "文件 '" + dlg.FileName + "' 已经存在。继续操作将覆盖此文件。\r\n\r\n请问是否要覆盖此文件? (OK 覆盖；Cancel 放弃操作)",
                    "ReaderSearchForm",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                {
                    strError = "放弃操作";
                    return 0;
                }
            }
#endif

            string strTimeRange = "";

            try
            {
                strTimeRange = GetTimeRange(dlg.ChargingHistoryDateRange);
            }
            catch (Exception ex)
            {
                strError = "日期范围字符串 '" + dlg.ChargingHistoryDateRange + "' 格式不合法: " + ex.Message;
                return -1;
            }

#if NO
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            // dlg.FileName = this.ExportExcelFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return 0;
#endif

            XLWorkbook doc = null;
            try
            {
                doc = new XLWorkbook(XLEventTracking.Disabled);
                File.Delete(dlg.FileName);
            }
            catch (Exception ex)
            {
                strError = "ReaderSearchForm new XLWorkbook() {39D0940F-33FF-4A10-8F61-1FFFEEBFF4D0} exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("表格");

            // TODO: sheet 可以按照单位来区分。例如按照班级

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("导出读者详情 ...");
            stop.BeginLoop();

            EnableControls(false);
            this.listView_records.Enabled = false;

            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, reader_barcodes.Count);

                // 每个列的最大字符数
                List<int> column_max_chars = new List<int>();

                // TODO: 表的标题，创建时间

                int nRowIndex = 3;  // 空出前两行
                //int nColIndex = 1;

                int nReaderIndex = 0;
                foreach (string strBarcode in reader_barcodes)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return 0;
                    }

                    if (string.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    // 获得读者记录
                    byte[] baTimestamp = null;
                    string strOutputRecPath = "";

                    stop.SetMessage("正在处理读者记录 " + strBarcode + " ...");

                    string[] results = null;
                    long lRet = Channel.GetReaderInfo(
                        stop,
                        strBarcode,
                        "advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary", // advancexml_history_bibliosummary
                        out results,
                        out strOutputRecPath,
                        out baTimestamp,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    if (lRet == 0)
                        return -1;

                    if (lRet > 1)   // 不可能发生吧?
                    {
                        strError = "读者证条码号 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，放弃装入读者记录。\r\n\r\n注意这是一个严重错误，请系统管理员尽快排除。";
                        return -1;
                    }
                    if (results == null || results.Length < 1)
                    {
                        strError = "返回的results不正常。";
                        return -1;
                    }
                    string strXml = results[0];

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载读者记录 XML 到 DOM 时发生错误: " + ex.Message;
                        return -1;
                    }

                    // 
                    if (dlg.ExportReaderInfo)
                    {
                        OutputReaderInfo(sheet,
                dom,
                nReaderIndex,
                ref nRowIndex,
                ref column_max_chars);
                    }

                    // 输出在借册表格
                    if (dlg.ExportBorrowInfo)
                    {
                        OutputBorrows(sheet,
                dom,
                Program.MainForm.GetBiblioSummary,
                true,
                ref nRowIndex,
                ref column_max_chars);
                    }

                    // 输出违约金表格
                    if (dlg.ExportOverdueInfo)
                    {
                        OutputOverdues(sheet,
                dom,
                Program.MainForm.GetBiblioSummary,
                ref nRowIndex,
                ref column_max_chars);
                    }

                    if (dlg.ExportChargingHistory)
                    {
                        try
                        {
                            ChargingHistoryLoader history_loader = new ChargingHistoryLoader();
                            history_loader.Channel = this.Channel;
                            history_loader.Stop = this.stop;
                            history_loader.PatronBarcode = strBarcode;
                            history_loader.TimeRange = strTimeRange;
                            history_loader.Actions = "return,lost";
                            history_loader.Order = "descending";

                            CacheableBiblioLoader summary_loader = new CacheableBiblioLoader();
                            summary_loader.Channel = this.Channel;
                            summary_loader.Stop = this.stop;
                            summary_loader.Format = "summary";
                            summary_loader.GetBiblioInfoStyle = GetBiblioInfoStyle.None;
                            // summary_loader.RecPaths = biblio_recpaths;

                            // 输出借阅历史表格
                            OutputBorrowHistory(sheet,
                    dom,
                    history_loader,
                                // Program.MainForm.GetBiblioSummary,
                    summary_loader,
                    ref nRowIndex,
                    ref column_max_chars);
                        }
                        catch (Exception ex)
                        {
                            strError = "输出借阅历史时出现异常: " + ex.Message;
                            return -1;
                        }
                    }

                    nRowIndex++;    // 读者之间的空行

                    nReaderIndex++;
                    if (stop != null)
                        stop.SetProgressValue(nReaderIndex);
                }

                {
                    if (stop != null)
                        stop.SetMessage("正在调整列宽度 ...");
                    Application.DoEvents();

                    //double char_width = GetAverageCharPixelWidth(list);

                    // 字符数太多的列不要做 width auto adjust
                    foreach (IXLColumn column in sheet.Columns())
                    {
                        int MAX_CHARS = 50;   // 60

                        int nIndex = column.FirstCell().Address.ColumnNumber - 1;
                        if (nIndex >= column_max_chars.Count)
                            break;
                        int nChars = column_max_chars[nIndex];

                        if (nIndex == 1)
                        {
                            column.Width = 10;
                            continue;
                        }

                        if (nIndex == 3)
                            MAX_CHARS = 50;
                        else
                            MAX_CHARS = 24;

                        if (nChars < MAX_CHARS)
                            column.AdjustToContents();
                        else
                            column.Width = Math.Min(MAX_CHARS, nChars);

                        //else
                        //    column.Width = (double)list.Columns[i].Width / char_width;  // Math.Min(MAX_CHARS, nChars);
                    }
                }
            }
            finally
            {
                EnableControls(true);
                this.listView_records.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                if (doc != null)
                {
                    doc.SaveAs(dlg.FileName);
                    doc.Dispose();
                }

                if (bLaunchExcel)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(dlg.FileName);
                    }
                    catch
                    {

                    }
                }

            }
            return 1;
        }

#endif

        // 过滤读者记录
        // parameters:
        //      filtering   过滤特征。空表示不过滤。amerce,borrowing,overdue
        // return:
        //      true 表示通过了过滤
        //      false   表示没有通过过滤
        // exception:
        //      可能会抛出 Exception 异常
        static bool FilterPatron(XmlDocument dom,
            string filtering)
        {
            if (string.IsNullOrEmpty(filtering))
                return true;

            // 有未交费用
            if (StringUtil.IsInList("amerce", filtering))
            {
                XmlNodeList overdues = dom.DocumentElement.SelectNodes("overdues/overdue");
                if (overdues.Count > 0)
                    return true;
            }

            // 有在借册
            if (StringUtil.IsInList("borrowing", filtering))
            {
                XmlNodeList borrows = dom.DocumentElement.SelectNodes("borrows/borrow");
                if (borrows.Count > 0)
                    return true;
            }

            // 有过期未还的册
            if (StringUtil.IsInList("overdue", filtering))
            {
                XmlNodeList borrows = dom.DocumentElement.SelectNodes("borrows/borrow");
                foreach (XmlElement borrow in borrows)
                {
                    string borrowDate = borrow.GetAttribute("borrowDate");
                    string period = borrow.GetAttribute("borrowPeriod");

                    // 看现在是否已经超期
                    // return:
                    //      -1  检测过程出错(是否超期则未知)
                    //      1   超期
                    //      0   没有超期
                    int nRet = Global.IsOverdue(borrowDate,
                        period,
                        out string strError);
                    if (nRet == -1)
                        throw new Exception($"在检查是否超期过程中发现读者记录错误: {strError}");
                    if (nRet == 1)
                        return true;
                }
            }

            return false;
        }

        // return:
        //      -1  出错
        //      0   用户中断
        //      1   成功
        public int CreateReaderDetailExcelFile(List<string> reader_barcodes,
            bool bLaunchExcel,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            ExportPatronExcelDialog dlg = new ExportPatronExcelDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.OverwritePrompt = true;
            dlg.UiState = Program.MainForm.AppInfo.GetString(
        "ReaderSearchForm",
        "ExportPatronExcelDialog_uiState",
        "");

            Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderSearchForm_ExportPatronExcelDialog_uiState_state");
            dlg.ShowDialog(this);

            Program.MainForm.AppInfo.SetString(
"ReaderSearchForm",
"ExportPatronExcelDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                strError = "放弃操作";
                return 0;
            }

            string strTimeRange = "";

            try
            {
                strTimeRange = GetTimeRange(dlg.ChargingHistoryDateRange);
            }
            catch (Exception ex)
            {
                strError = "日期范围字符串 '" + dlg.ChargingHistoryDateRange + "' 格式不合法: " + ex.Message;
                return -1;
            }

            string strOutputFileName = dlg.FileName;

            XLWorkbook doc = null;
            try
            {
                // 提前保存一下，如果此时文件扩展名不正确，就能当时抛出异常
                File.Delete(strOutputFileName);
                using (doc = new XLWorkbook(XLEventTracking.Disabled))
                {
                    doc.Worksheets.Add("表格");
                    doc.SaveAs(strOutputFileName);
                }

                File.Delete(strOutputFileName);
                doc = new XLWorkbook(XLEventTracking.Disabled);
            }
            catch (Exception ex)
            {
                strError = "ReaderSearchForm new XLWorkbook() {39D0940F-33FF-4A10-8F61-1FFFEEBFF4D0} exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("表格");

            // 每个列的最大字符数
            List<int> column_max_chars = new List<int>();

            // TODO: 表的标题，创建时间

            int nRowIndex = 3;  // 空出前两行
            //int nColIndex = 1;

            int nReaderIndex = 0;

            try
            {
                Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始导出读者详情</div>");

                // return:
                //      -1  出错。包括用户中断的情况
                //      >=0 实际处理的读者记录数
                nRet = this.ProcessPatrons(
                    reader_barcodes,
                    "advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary", // advancexml_history_bibliosummary
                    (strRecPath, dom, timestamp) =>
                    {
                        this.ShowMessage("正在处理读者记录 " + strRecPath);

                        try
                        {
                            // 过滤读者记录
                            // parameters:
                            //      filtering   过滤特征。空表示不过滤。amerce,borrowing,overdue
                            // return:
                            //      true 表示通过了过滤
                            //      false   表示没有通过过滤
                            // exception:
                            //      可能会抛出 Exception 异常
                            if (FilterPatron(dom, dlg.Filtering) == false)
                            {
                                Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode($"读者记录 {strRecPath} 不符合过滤条件 '{dlg.Filtering}'，被跳过") + "</div>");
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            string strErrorText = $"处理读者记录 {strRecPath} 时出现异常: {ex.Message}";
                            throw new Exception(strErrorText, ex);
                        }

                        string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");

                        // 
                        if (dlg.ExportReaderInfo)
                        {
                            OutputReaderInfo(sheet,
                    dom,
                    nReaderIndex,
                    dlg.PrintReaderBarcodeLabel ? "barcode" : "",
                    ref nRowIndex,
                    ref column_max_chars);
                        }

                        // 输出在借册表格
                        if (dlg.ExportBorrowInfo)
                        {
                            OutputBorrows(sheet,
                    dom,
                    Program.MainForm.GetBiblioSummary,
                    true,
                    ref nRowIndex,
                    ref column_max_chars);
                        }

                        // 输出违约金表格
                        if (dlg.ExportOverdueInfo)
                        {
                            OutputOverdues(sheet,
                    dom,
                    Program.MainForm.GetBiblioSummary,
                    ref nRowIndex,
                    ref column_max_chars);
                        }

                        if (dlg.ExportChargingHistory)
                        {
                            try
                            {
                                ChargingHistoryLoader history_loader = new ChargingHistoryLoader();
                                history_loader.Channel = this.Channel;
                                history_loader.Stop = this.stop;
                                history_loader.PatronBarcode = strBarcode;
                                history_loader.TimeRange = strTimeRange;
                                history_loader.Actions = "return,lost";
                                history_loader.Order = "descending";

                                CacheableBiblioLoader summary_loader = new CacheableBiblioLoader();
                                summary_loader.Channel = this.Channel;
                                summary_loader.Stop = this.stop;
                                summary_loader.Format = "summary";
                                summary_loader.GetBiblioInfoStyle = GetBiblioInfoStyle.None;
                                // summary_loader.RecPaths = biblio_recpaths;

                                // 输出借阅历史表格
                                // 可能会抛出异常，例如权限不够
                                OutputBorrowHistory(sheet,
                        dom,
                        history_loader,
                        // Program.MainForm.GetBiblioSummary,
                        summary_loader,
                        ref nRowIndex,
                        ref column_max_chars);
                            }
                            catch (ChannelException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                string strErrorText = "输出借阅历史时出现异常: " + ex.Message;
                                throw new Exception(strErrorText, ex);
                            }
                        }

                        nRowIndex++;    // 读者之间的空行

                        nReaderIndex++;
                        return true;
                    },
                    out strError);
                if (nRet == -1)
                {
                    doc.Dispose();
                    doc = null;
                    return -1;
                }

                {
                    if (stop != null)
                        stop.SetMessage("正在调整列宽度 ...");
                    Application.DoEvents();

                    //double char_width = GetAverageCharPixelWidth(list);

                    // 字符数太多的列不要做 width auto adjust
                    foreach (IXLColumn column in sheet.Columns())
                    {
                        int MAX_CHARS = 50;   // 60

                        int nIndex = column.FirstCell().Address.ColumnNumber - 1;
                        if (nIndex >= column_max_chars.Count)
                            break;
                        int nChars = column_max_chars[nIndex];

                        if (nIndex == 1)
                        {
                            column.Width = 10;
                            continue;
                        }

                        if (nIndex == 3)
                            MAX_CHARS = 50;
                        else
                            MAX_CHARS = 24;

                        if (nChars < MAX_CHARS)
                            column.AdjustToContents();
                        else
                            column.Width = Math.Min(MAX_CHARS, nChars);

                        //else
                        //    column.Width = (double)list.Columns[i].Width / char_width;  // Math.Min(MAX_CHARS, nChars);
                    }
                }

            }
            catch (Exception ex)
            {
                strError = "CreateDetailExcelFile() 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
            }
            finally
            {
                if (stop != null)
                    stop.SetMessage("");

                if (doc != null)
                {
                    doc.SaveAs(strOutputFileName);
                    doc.Dispose();

                    if (bLaunchExcel)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(strOutputFileName);
                        }
                        catch
                        {

                        }
                    }
                }

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束导出读者详情</div>");
                this.ClearMessage();
            }

            // TODO: sheet 可以按照单位来区分。例如按照班级
            this.ShowMessage("共导出读者记录 " + nReaderIndex + " 个", "green", true);
            return 1;
        }

        // 将 20120101 - 20151231 这样的日期字符串形态转换为 SearchCharging() API 能接受的时间范围字符串形态
        // 注意 SearchCharging() API 要的时间范围字符串，中间是一个波浪号而不是横杠
        static string GetTimeRange(string strText)
        {
            string strStart = "";
            string strEnd = "";
            StringUtil.ParseTwoPart(strText, "-", out strStart, out strEnd);
            strStart = strStart.Trim();
            strEnd = strEnd.Trim();
            if (string.IsNullOrEmpty(strStart) == false)
            {
                DateTime time = DateTimeUtil.Long8ToDateTime(strStart);
                strStart = time.ToString("G");
            }
            if (string.IsNullOrEmpty(strEnd) == false)
            {
                DateTime time = DateTimeUtil.Long8ToDateTime(strEnd);
                strEnd = time.AddDays(1).ToString("G"); // 注意 ~ 后面是 < 的意思，所以要放到第二天的零点
            }

            return strStart + "~" + strEnd;
        }

        static string GetContactString(XmlDocument dom)
        {
            string strTel = DomUtil.GetElementText(dom.DocumentElement,
"tel");
            string strEmail = DomUtil.GetElementText(dom.DocumentElement,
"email");
            // 2017/2/25
            strEmail = LibraryServerUtil.GetEmailAddress(strEmail);

            string strAddress = DomUtil.GetElementText(dom.DocumentElement,
"address");
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(strTel) == false)
                list.Add(strTel);
            if (string.IsNullOrEmpty(strEmail) == false)
                list.Add(strEmail);
            if (string.IsNullOrEmpty(strAddress) == false)
                list.Add(strAddress);
            return StringUtil.MakePathList(list, "; ");
        }

        static void OutputTitleLine(IXLWorksheet sheet,
            ref int nRowIndex,
            string strTitle,
            XLColor text_color,
            int nStartIndex,
            int nColumnCount)
        {
            // 读者序号
            int nColIndex = nStartIndex;
            {
                IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(strTitle);
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = text_color;
                //cell.Style.Font.FontName = strFontName;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                // cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                nColIndex++;
            }

            // 合并格子
            {
                var rngData = sheet.Range(nRowIndex, nStartIndex, nRowIndex, nStartIndex + nColumnCount - 1);
                rngData.Merge();
                // rngData.LastColumn().Style.Border.RightBorder = XLBorderStyleValues.Hair;
            }

            nRowIndex++;
        }

#if NO
        static void OutputReaderSheet1(IXLWorksheet sheet,
    XmlDocument dom,
    int nReaderIndex,
    string strStyle,
    ref int nRowIndex,
    ref List<int> column_max_chars)
        {
            bool bBarcode = StringUtil.IsInList("barcode", strStyle);

            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
            string strName = DomUtil.GetElementText(dom.DocumentElement,
                "name");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement,
                "department");

            List<IXLCell> cells = new List<IXLCell>();

            // 读者序号
            // IXLCell cell_no = null;
            int nColIndex = 2;
            {
                IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(nReaderIndex + 1);
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 20;
                cells.Add(cell);
                nColIndex++;
            }

            // 最大字符数
            SetMaxChars(ref column_max_chars, 1, (nReaderIndex + 1).ToString().Length * 2);

            // 序号的右边竖线
            {
                var rngData = sheet.Range(nRowIndex, 2, nRowIndex + 3, 2);
                rngData.Merge();
                rngData.LastColumn().Style.Border.RightBorder = XLBorderStyleValues.Hair;

                // 第一行上面的横线
                rngData = sheet.Range(nRowIndex, 2, nRowIndex, 2 + 7 - 1);
                rngData.FirstRow().Style.Border.TopBorder = XLBorderStyleValues.Medium;
            }

            int nFirstRow = nRowIndex;
            {
                List<string> subtitles = new List<string>();
                subtitles.Add("姓名");
                subtitles.Add("证条码号");
                subtitles.Add("部门");

                List<string> subcols = new List<string>();
                subcols.Add(strName);
                if (bBarcode)
                    subcols.Add("*" + strReaderBarcode + "*");
                else
                    subcols.Add(strReaderBarcode);
                subcols.Add(strDepartment);

                for (int line = 0; line < subtitles.Count; line++)
                {
                    nColIndex = 3;
                    {
                        IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(subtitles[line]);
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontColor = XLColor.DarkGray;
                        //cell.Style.Font.FontName = strFontName;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        // cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        nColIndex++;
                        cells.Add(cell);
                    }
                    {
                        string strText = subcols[line];

                        // 最大字符数
                        SetMaxChars(ref column_max_chars, nColIndex - 1, strText.Length);

                        IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(strText);
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //cell.Style.Font.FontName = strFontName;
                        //cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                        if (line == 0)
                        {
                            cell.Style.Font.FontName = "微软雅黑";
                            cell.Style.Font.FontSize = 20;
                        }
                        else if (bBarcode && line == 1)
                        {
                            cell.Style.Font.FontName = "C39HrP24DhTt";
                            cell.Style.Font.FontSize = 40;
                        }
                        nColIndex++;
                        cells.Add(cell);
                    }
                    nRowIndex++;
                }
            }

        }

        static void OutputReaderSheet2(IXLWorksheet sheet,
XmlDocument dom,
int nReaderIndex,
string strStyle,
ref int nRowIndex,
ref List<int> column_max_chars)
        {
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
            string strName = DomUtil.GetElementText(dom.DocumentElement,
                "name");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement,
                "department");

            List<IXLCell> cells = new List<IXLCell>();

            int nFirstRow = nRowIndex;
            {

                int nColIndex = 1;

                // 姓名
                {
                    string strText = strName;

                    // 最大字符数
                    SetMaxChars(ref column_max_chars, nColIndex - 1, strText.Length);

                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(strText);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.FontName = "微软雅黑";
                    cell.Style.Font.FontSize = 20;

                    nColIndex++;
                    cells.Add(cell);
                }

                {

                    // 第一行上面的横线
                    var rngData = sheet.Range(nRowIndex, 1, nRowIndex, 1);
                    rngData.FirstRow().Style.Border.TopBorder = XLBorderStyleValues.Medium;
                }

                nRowIndex++;

                // 单位
                {
                    nColIndex = 1;
                    string strText = strDepartment;

                    // 最大字符数
                    SetMaxChars(ref column_max_chars, nColIndex - 1, strText.Length);

                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(strText);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.FontName = "微软雅黑";
                    cell.Style.Font.FontSize = 12;

                    nColIndex++;
                    cells.Add(cell);
                }

                nRowIndex++;

                // 条码
                {
                    nColIndex = 1;
                    string strText = "*" + strReaderBarcode + "*";

                    // 最大字符数
                    SetMaxChars(ref column_max_chars, nColIndex - 1, strText.Length);

                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(strText);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    cell.Style.Font.FontName = "C39HrP24DhTt";
                    cell.Style.Font.FontSize = 40;
                    nColIndex++;
                    cells.Add(cell);
                }

#if NO
                {
                    var rngData = sheet.Range(nRowIndex, 3, nRowIndex, 4);
                    rngData.Merge();
                    // rngData.LastColumn().Style.Border.RightBorder = XLBorderStyleValues.Hair;
                }
#endif

                nRowIndex++;

            }

        }

#endif

        static void OutputReaderInfo(IXLWorksheet sheet,
            XmlDocument dom,
            int nReaderIndex,
            string strStyle,
            ref int nRowIndex,
            ref List<int> column_max_chars)
        {
            bool bBarcode = StringUtil.IsInList("barcode", strStyle);

            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
            string strName = DomUtil.GetElementText(dom.DocumentElement,
                "name");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement,
                "department");
            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");
            string strCreateDate = ToLocalTime(DomUtil.GetElementText(dom.DocumentElement,
                "createDate"), "yyyy/MM/dd");
            string strExpireDate = ToLocalTime(DomUtil.GetElementText(dom.DocumentElement,
                "expireDate"), "yyyy/MM/dd");
            string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
                "readerType");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");

            List<IXLCell> cells = new List<IXLCell>();

            // 读者序号
            // IXLCell cell_no = null;
            int nColIndex = 2;
            {
                IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(nReaderIndex + 1);
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 20;
                //cell.Style.Font.FontName = strFontName;
                //cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                // cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cells.Add(cell);
                // cell_no = cell;
                nColIndex++;
            }

            // 最大字符数
            SetMaxChars(ref column_max_chars, 1, (nReaderIndex + 1).ToString().Length * 2);

            // 序号的右边竖线
            {
                var rngData = sheet.Range(nRowIndex, 2, nRowIndex + 3, 2);
                rngData.Merge();
                rngData.LastColumn().Style.Border.RightBorder = XLBorderStyleValues.Hair;

                // 第一行上面的横线
                rngData = sheet.Range(nRowIndex, 2, nRowIndex, 2 + 7 - 1);
                rngData.FirstRow().Style.Border.TopBorder = XLBorderStyleValues.Medium;
            }

#if NO
            // 特殊状态时的整个底色
            if (string.IsNullOrEmpty(strState) == false)
            {
                var rngData = sheet.Range(nRowIndex, 2, nRowIndex + 3, 2 + 7 - 1);
                rngData.Style.Fill.BackgroundColor = XLColor.LightBrown;
            }
#endif

            int nFirstRow = nRowIndex;
            {
                List<string> subtitles = new List<string>();
                subtitles.Add("姓名");
                subtitles.Add("证条码号");
                subtitles.Add("部门");
                subtitles.Add("联系方式");

                List<string> subcols = new List<string>();
                subcols.Add(strName);
                if (bBarcode)
                    subcols.Add("*" + strReaderBarcode + "*");
                else
                    subcols.Add(strReaderBarcode);
                subcols.Add(strDepartment);
                subcols.Add(GetContactString(dom));

                for (int line = 0; line < subtitles.Count; line++)
                {
                    nColIndex = 3;
                    {
                        IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(subtitles[line]);
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontColor = XLColor.DarkGray;
                        //cell.Style.Font.FontName = strFontName;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        // cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        nColIndex++;
                        cells.Add(cell);
                    }
                    {
                        string strText = subcols[line];

                        // 最大字符数
                        SetMaxChars(ref column_max_chars, nColIndex - 1, strText.Length);

                        IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(strText);
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        //cell.Style.Font.FontName = strFontName;
                        //cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                        if (line == 0)
                        {
                            cell.Style.Font.FontName = "微软雅黑";
                            cell.Style.Font.FontSize = 20;
                        }
                        else if (bBarcode && line == 1)
                        {
                            cell.Style.Font.FontName = "C39HrP24DhTt";
                            cell.Style.Font.FontSize = 40;
                        }
                        nColIndex++;
                        cells.Add(cell);
                    }
                    nRowIndex++;
                }

                //    

                //var rngData = sheet.Range(cells[0], cells[cells.Count - 1]);
                //rngData.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            }

            nRowIndex = nFirstRow;
            {
                List<string> subtitles = new List<string>();
                subtitles.Add("状态");
                subtitles.Add("有效期");
                subtitles.Add("读者类别");
                subtitles.Add("注释");

                List<string> subcols = new List<string>();
                subcols.Add(strState);
                subcols.Add(strCreateDate + "~" + strExpireDate);
                subcols.Add(strReaderType);
                subcols.Add(strComment);

                for (int line = 0; line < subtitles.Count; line++)
                {
                    nColIndex = 7;
                    {
                        IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(subtitles[line]);
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontColor = XLColor.DarkGray;
                        //cell.Style.Font.FontName = strFontName;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        // cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        nColIndex++;
                        cells.Add(cell);
                    }
                    {
                        string strText = subcols[line];
                        // 最大字符数
                        SetMaxChars(ref column_max_chars, nColIndex - 1, strText.Length);

                        IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(strText);
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        if (line == 0)
                        {
                            cell.Style.Font.FontName = "微软雅黑";
                            cell.Style.Font.FontSize = 20;
                            if (string.IsNullOrEmpty(strState) == false)
                            {
                                cell.Style.Font.FontColor = XLColor.White;
                                cell.Style.Fill.BackgroundColor = XLColor.DarkRed;
                            }
                        }
                        nColIndex++;
                        cells.Add(cell);
                    }
                    nRowIndex++;
                }
            }
        }

        // parameters:
        //      bAdvanceXml 是否为 AdvanceXml 情况
        static void OutputBorrowHistory(
            IXLWorksheet sheet,
            XmlDocument reader_dom,
            ChargingHistoryLoader history_loader,
            CacheableBiblioLoader summary_loader,
            // Delegate_GetBiblioSummary procGetBiblioSummary,
            ref int nRowIndex,
            ref List<int> column_max_chars)
        {
            int nStartRow = nRowIndex;

            OutputTitleLine(sheet,
ref nRowIndex,
"--- 借阅历史 --- " + history_loader.GetCount(),
XLColor.DarkGreen,
2,
7);


            List<IXLCell> cells = new List<IXLCell>();

            // 册信息若干行的标题
            {
                List<string> titles = new List<string>();
                titles.Add("序号");
                titles.Add("册条码号");
                titles.Add("书目摘要");
                titles.Add("借阅时间");
                titles.Add("期限");
                titles.Add("借阅操作者");
                titles.Add("还书时间");
                titles.Add("还书操作者");

                int nColIndex = 2;
                foreach (string s in titles)
                {
                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(s);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.DarkGray;
                    //cell.Style.Font.FontName = strFontName;
                    //cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                    cells.Add(cell);
                }
                nRowIndex++;
            }

            List<string> item_barcodes = new List<string>();
            List<Point> points = new List<Point>();
            int nItemIndex = 0;
            foreach (ChargingItemWrapper wrapper in history_loader)
            {
                ChargingItem item = wrapper.Item;
                ChargingItem rel = wrapper.RelatedItem;

                string strItemBarcode = item.ItemBarcode;
                string strBorrowDate = rel == null ? "" : rel.OperTime;
                string strBorrowPeriod = GetDisplayTimePeriodString(rel == null ? "" : rel.Period);
                string strReturnDate = item.OperTime;
                //string strRecPath = borrow.GetAttribute("recPath");
                //string strIsOverdue = borrow.GetAttribute("isOverdue");
                //bool bIsOverdue = DomUtil.IsBooleanTrue(strIsOverdue, false);
                //string strOverdueInfo = borrow.GetAttribute("overdueInfo1");

                string strSummary = "";
#if NO
                if (string.IsNullOrEmpty(strItemBarcode) == false
                    && string.IsNullOrEmpty(strSummary) == true)
                {
                    string strError = "";
                    int nRet = procGetBiblioSummary(strItemBarcode,
                        "", // strConfirmItemRecPath,
                        false,
                        out strSummary,
                        out strError);
                    if (nRet == -1)
                        strSummary = strError;
                }
#endif
                item_barcodes.Add("@itemBarcode:" + strItemBarcode);

                List<string> cols = new List<string>();
                cols.Add((nItemIndex + 1).ToString());
                cols.Add(strItemBarcode);
                cols.Add(strSummary);

                cols.Add(strBorrowDate);
                cols.Add(strBorrowPeriod);
                cols.Add(rel == null ? "" : rel.Operator);

                cols.Add(strReturnDate);
                cols.Add(item.Operator);

                int nColIndex = 2;
                points.Add(new Point(nColIndex + 2, nRowIndex));
                foreach (string s in cols)
                {
                    // 统计最大字符数
                    SetMaxChars(ref column_max_chars, nColIndex - 1, GetCharWidth(s));

                    IXLCell cell = null;
                    if (nColIndex == 2)
                    {
                        cell = sheet.Cell(nRowIndex, nColIndex).SetValue(nItemIndex + 1);
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                        cell = sheet.Cell(nRowIndex, nColIndex).SetValue(s);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //cell.Style.Font.FontName = strFontName;
                    //cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                    cells.Add(cell);
                }

#if NO
                // 超期的行为黄色背景
                if (bIsOverdue)
                {
                    var line = sheet.Range(nRowIndex, 2, nRowIndex, 2 + cols.Count - 1);
                    line.Style.Fill.BackgroundColor = XLColor.Yellow;
                }
#endif

                nItemIndex++;
                nRowIndex++;
            }

            // 加入书目摘要
            summary_loader.RecPaths = item_barcodes;
            int i = 0;
            foreach (BiblioItem biblio in summary_loader)
            {
                Point point = points[i];
                int nColIndex = point.X;
                // 统计最大字符数
                SetMaxChars(ref column_max_chars, nColIndex - 1, GetCharWidth(biblio.Content));

                IXLCell cell = null;
                cell = sheet.Cell(point.Y, nColIndex).SetValue(biblio.Content);
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                i++;
            }

            // 册信息标题下的虚线
            var rngData = sheet.Range(cells[0], cells[cells.Count - 1]);
            rngData.FirstRow().Style.Border.TopBorder = XLBorderStyleValues.Dotted;

            sheet.Rows(nStartRow + 1, nRowIndex - 1).Group();
        }

        // TODO: 是否超期列也应能从 returningDate 计算出，以适应非 AdvanceXml 的情况
        // parameters:
        //      bAdvanceXml 是否为 AdvanceXml 情况
        static void OutputBorrows(IXLWorksheet sheet,
            XmlDocument dom,
            Delegate_GetBiblioSummary procGetBiblioSummary,
            bool bAdvanceXml,
            ref int nRowIndex,
            ref List<int> column_max_chars)
        {
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
            if (nodes.Count == 0)
                return;

            int nStartRow = nRowIndex;

            OutputTitleLine(sheet,
ref nRowIndex,
"--- 在借 --- " + nodes.Count,
XLColor.DarkGreen,
2,
7);

            List<IXLCell> cells = new List<IXLCell>();

            // 册信息若干行的标题
            {
                List<string> titles = new List<string>();
                titles.Add("序号");
                titles.Add("册条码号");
                titles.Add("书目摘要");
                titles.Add("借阅时间");
                titles.Add("借期");
                titles.Add("应还时间");
                titles.Add("是否超期");

                int nColIndex = 2;
                foreach (string s in titles)
                {
                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(s);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.DarkGray;
                    //cell.Style.Font.FontName = strFontName;
                    //cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                    cells.Add(cell);
                }
                nRowIndex++;
            }

            int nItemIndex = 0;
            foreach (XmlElement borrow in nodes)
            {
                string strItemBarcode = borrow.GetAttribute("barcode");
                string strBorrowDate = ToLocalTime(borrow.GetAttribute("borrowDate"), "yyyy-MM-dd HH:mm");
                string strBorrowPeriod = GetDisplayTimePeriodString(borrow.GetAttribute("borrowPeriod"));
                string strReturningDate = ToLocalTime(borrow.GetAttribute("returningDate"), "yyyy-MM-dd");
                string strRecPath = borrow.GetAttribute("recPath");
                string strIsOverdue = borrow.GetAttribute("isOverdue");
                bool bIsOverdue = DomUtil.IsBooleanTrue(strIsOverdue, false);
                string strOverdueInfo = borrow.GetAttribute("overdueInfo1");

                if (bAdvanceXml == false)
                {
                    string strPeriod = borrow.GetAttribute("borrowPeriod");
                    string strRfc1123String = borrow.GetAttribute("returningDate");

                    if (string.IsNullOrEmpty(strRfc1123String) == false)
                    {
                        try
                        {
                            DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strRfc1123String);
                            TimeSpan delta = DateTime.Now - time.ToLocalTime();
                            if (strPeriod.IndexOf("hour") != -1)
                            {
                                if (delta.Hours > 0)
                                {
                                    strOverdueInfo = "已超期 " + delta.Hours + " 小时";
                                    bIsOverdue = true;
                                }
                            }
                            else
                            {
                                if (delta.Days > 0)
                                {
                                    strOverdueInfo = "已超期 " + delta.Days + " 天";
                                    bIsOverdue = true;
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }

                string strSummary = borrow.GetAttribute("summary");
                if (string.IsNullOrEmpty(strItemBarcode) == false
                    && string.IsNullOrEmpty(strSummary) == true)
                {
                    string strError = "";
                    int nRet = procGetBiblioSummary(strItemBarcode,
                        strRecPath, // strConfirmItemRecPath,
                        false,
                        out strSummary,
                        out strError);
                    if (nRet == -1)
                        strSummary = strError;
                }

                List<string> cols = new List<string>();
                cols.Add((nItemIndex + 1).ToString());
                cols.Add(strItemBarcode);
                cols.Add(strSummary);
                cols.Add(strBorrowDate);
                cols.Add(strBorrowPeriod);
                cols.Add(strReturningDate);
                if (bIsOverdue)
                    cols.Add(strOverdueInfo);
                else
                    cols.Add("");

                int nColIndex = 2;
                foreach (string s in cols)
                {
                    // 统计最大字符数
                    SetMaxChars(ref column_max_chars, nColIndex - 1, GetCharWidth(s));

                    IXLCell cell = null;
                    if (nColIndex == 2)
                    {
                        cell = sheet.Cell(nRowIndex, nColIndex).SetValue(nItemIndex + 1);
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                        cell = sheet.Cell(nRowIndex, nColIndex).SetValue(s);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //cell.Style.Font.FontName = strFontName;
                    //cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                    cells.Add(cell);

                }

                // 超期的行为黄色背景
                if (bIsOverdue)
                {
                    var line = sheet.Range(nRowIndex, 2, nRowIndex, 2 + cols.Count - 1);
                    line.Style.Fill.BackgroundColor = XLColor.Yellow;
                }

                nItemIndex++;
                nRowIndex++;
            }

            // 册信息标题下的虚线
            var rngData = sheet.Range(cells[0], cells[cells.Count - 1]);
            rngData.FirstRow().Style.Border.TopBorder = XLBorderStyleValues.Dotted;

#if NO
            // 第一行上面的横线
            rngData = sheet.Range(cell_no, cells[cells.Count - 1]);
            rngData.FirstRow().Style.Border.TopBorder = XLBorderStyleValues.Medium;
#endif
            sheet.Rows(nStartRow + 1, nRowIndex - 1).Group();
        }

        public delegate int Delegate_GetBiblioSummary(string strItemBarcode,
    string strConfirmItemRecPath,
    bool bDisplayProgress,
    out string strSummary,
    out string strError);

        static void OutputOverdues(IXLWorksheet sheet,
            XmlDocument dom,
            Delegate_GetBiblioSummary procGetBiblioSummary,
            ref int nRowIndex,
            ref List<int> column_max_chars)
        {
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");
            if (nodes.Count == 0)
                return;

            int nStartRow = nRowIndex;

            OutputTitleLine(sheet,
                ref nRowIndex,
                "--- 费用 --- " + nodes.Count,
                XLColor.DarkRed,
                2,
                6);

            int nRet = 0;

            List<IXLCell> cells = new List<IXLCell>();

            // 栏目标题
            {
                List<string> titles = new List<string>();
                titles.Add("序号");
                titles.Add("册条码号");
                titles.Add("书目摘要");
                titles.Add("说明");
                titles.Add("金额");
                titles.Add("ID");

#if NO
                titles.Add("以停代金情况");
                titles.Add("起点日期");
                titles.Add("期限");
                titles.Add("终点日期");
#endif

                int nColIndex = 2;
                foreach (string s in titles)
                {
                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(s);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.DarkGray;
                    nColIndex++;
                    cells.Add(cell);
                }
                nRowIndex++;
            }

            int nItemIndex = 0;
            foreach (XmlElement borrow in nodes)
            {
                string strItemBarcode = borrow.GetAttribute("barcode");
                string strReason = borrow.GetAttribute("reason");
                string strPrice = borrow.GetAttribute("price");
                string strID = borrow.GetAttribute("id");
                string strRecPath = borrow.GetAttribute("recPath");

                string strSummary = borrow.GetAttribute("summary");
                if (string.IsNullOrEmpty(strItemBarcode) == false
                    && string.IsNullOrEmpty(strSummary) == true)
                {
                    string strError = "";
                    nRet = procGetBiblioSummary(strItemBarcode,
                        strRecPath, // strConfirmItemRecPath,
                        false,
                        out strSummary,
                        out strError);
                    if (nRet == -1)
                        strSummary = strError;
                }

                List<string> cols = new List<string>();
                cols.Add((nItemIndex + 1).ToString());
                cols.Add(strItemBarcode);
                cols.Add(strSummary);
                cols.Add(strReason);
                cols.Add(strPrice);
                cols.Add(strID);

                int nColIndex = 2;
                foreach (string s in cols)
                {
                    // 统计最大字符数
                    SetMaxChars(ref column_max_chars, nColIndex - 1, GetCharWidth(s));

                    IXLCell cell = null;
                    if (nColIndex == 2)
                    {
                        cell = sheet.Cell(nRowIndex, nColIndex).SetValue(nItemIndex + 1);
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else
                        cell = sheet.Cell(nRowIndex, nColIndex).SetValue(s);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    nColIndex++;
                    cells.Add(cell);
                }

                nItemIndex++;
                nRowIndex++;
            }

            // 标题行下的虚线
            var rngData = sheet.Range(cells[0], cells[cells.Count - 1]);
            rngData.FirstRow().Style.Border.TopBorder = XLBorderStyleValues.Dotted;

            sheet.Rows(nStartRow + 1, nRowIndex - 1).Group();
        }

        static string GetDisplayTimePeriodString(string strText)
        {
            strText = strText.Replace("day", "天");

            return strText.Replace("hour", "小时");
        }

        // 计算一个字符串的“西文字符宽度”。汉字相当于两个西文字符宽度
        public static int GetCharWidth(string strText)
        {
            int result = 0;
            foreach (char c in strText)
            {
                result += StringUtil.IsHanzi(c) == true ? 2 : 1;
            }

            return result;
        }

        public static void SetMaxChars(ref List<int> column_max_chars, int index, int chars)
        {
            // 确保空间足够
            while (column_max_chars.Count < index + 1)
            {
                column_max_chars.Add(0);
            }

            // 统计最大字符数
            int nOldChars = column_max_chars[index];
            if (chars > nOldChars)
            {
                column_max_chars[index] = chars;
            }
        }

        static string ToLocalTime(string strRfc1123, string strFormat)
        {
            try
            {
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123, strFormat);
            }
            catch (Exception ex)
            {
                return "时间字符串 '" + strRfc1123 + "' 格式不正确: " + ex.Message;
            }
        }
#if NO
        int GetEntityInfo(
            Stop stop,
            string strItemBarcode,
            out List<string> cols,
            out string strError)
        {
            cols = new List<string>();
            strError = "";

            string strXml = "";
            string strOutputRecPath = "";
            byte[] baTimestamp = null;
            string strBiblio = "";
            string strBiblioRecPath = "";
            long lRet = Channel.GetItemInfo(
stop,
strItemBarcode,
"xml",
out strXml,
out strOutputRecPath,
out baTimestamp,
"",
out strBiblio,
out strBiblioRecPath,
out strError);
            if (lRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "装载册记录 XML 到 DOM 时发生错误: " + ex.Message;
                return -1;
            }

            // 列

            /*

读者证条码号 姓名 部门 读者类型

册条码号 书目摘要 借阅时间 借期 应还时间

书目摘要可以扩展为 书名 作者 出版社 出版日期 ISBN
             * */
            cols.Add(DomUtil.GetElementText(dom.DocumentElement));

            return 0;
        }
#endif

    }

    /// <summary>
    /// 根据 ListViewItem 数组获得读者记录信息的枚举器
    /// 可以利用缓存机制
    /// </summary>
    public class ListViewPatronLoader : IEnumerable
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

        /// <summary>
        /// 数据库类型，用于显示的文字。缺省为空
        /// </summary>
        public string DbTypeCaption
        {
            get;
            set;
        }
        /// <summary>
        /// ListViewItem 事项数组
        /// </summary>
        public List<ListViewItem> Items
        {
            get;
            set;
        }

        /// <summary>
        /// 缓存表
        /// </summary>
        public Hashtable CacheTable
        {
            get;
            set;
        }

        BrowseLoader m_loader = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channel">通讯通道</param>
        /// <param name="stop">停止对象</param>
        /// <param name="items">ListViewItem 数组</param>
        /// <param name="cacheTable">用于缓存的 Hashtable</param>
        public ListViewPatronLoader(LibraryChannel channel,
            Stop stop,
            List<ListViewItem> items,
            Hashtable cacheTable)
        {
            m_loader = new BrowseLoader();
            m_loader.Channel = channel;
            m_loader.Stop = stop;
            m_loader.Format = "id,xml,timestamp";
#if NO
            if (this.Prompt != null)
                m_loader.Prompt += m_loader_Prompt;
#endif
            // m_loader.GetBiblioInfoStyle = GetBiblioInfoStyle.Timestamp; // 附加信息只取得 timestamp

            this.Items = items;
            this.CacheTable = cacheTable;
        }

        void m_loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            if (this.Prompt != null)
                this.Prompt(sender, e);
        }

        /// <summary>
        /// 获得枚举接口
        /// </summary>
        /// <returns>枚举接口</returns>
        public IEnumerator GetEnumerator()
        {
            Debug.Assert(m_loader != null, "");

            // 2017/5/5
            if (this.Prompt != null)
                m_loader.Prompt += m_loader_Prompt;

            // 2017/5/15
            // 用于在遍历中途临时存储 BiblioInfo 对象，避免(在直接用 CacheTable 的情况下)因为其他功能并发用到 CacheTable 而造成锁定关系被破坏
            // 复制(快照)比锁定方法效果更好
            Hashtable temp_cacheTable = new Hashtable();
            try
            {

                List<string> recpaths = new List<string>(); // 缓存中没有包含的那些记录
                foreach (ListViewItem item in this.Items)
                {
                    string strRecPath = item.Text;
                    if (string.IsNullOrEmpty(strRecPath))
                    {
                        throw new Exception("Items 中包含了 路径为空的事项");
                    }

                    Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");

                    BiblioInfo info = (BiblioInfo)this.CacheTable[strRecPath];
                    if (info == null || string.IsNullOrEmpty(info.OldXml) == true)
                        recpaths.Add(strRecPath);
                    else
                        temp_cacheTable[strRecPath] = info;  // 临时存储
                }

                // 注： Hashtable 在这一段时间内不应该被修改。否则会破坏 m_loader 和 items 之间的锁定对应关系

                m_loader.RecPaths = recpaths;

                var enumerator = m_loader.GetEnumerator();

                // 开始循环
                foreach (ListViewItem item in this.Items)
                {
                    string strRecPath = item.Text;
                    Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");

                    BiblioInfo info = (BiblioInfo)temp_cacheTable[strRecPath]; // (BiblioInfo)this.CacheTable[strRecPath];
                    if (info == null || string.IsNullOrEmpty(info.OldXml) == true)
                    {
                        if (m_loader.Stop != null)
                        {
                            m_loader.Stop.SetMessage("正在获取" + this.DbTypeCaption + "记录 " + strRecPath);
                        }
                        bool bRet = enumerator.MoveNext();
                        if (bRet == false)
                        {
                            Debug.Assert(false, "还没有到结尾, MoveNext() 不应该返回 false");
                            // TODO: 这时候也可以采用返回一个带没有找到的错误码的元素
                            yield break;
                        }

                        DigitalPlatform.LibraryClient.localhost.Record biblio = (DigitalPlatform.LibraryClient.localhost.Record)enumerator.Current;
                        Debug.Assert(biblio.Path == strRecPath, "m_loader 和 items 的元素之间 记录路径存在严格的锁定对应关系");

                        // 需要放入缓存
                        if (info == null)
                        {
                            info = new BiblioInfo();
                            info.RecPath = biblio.Path;
                        }
                        info.OldXml = biblio.RecordBody.Xml;
                        info.Timestamp = biblio.RecordBody.Timestamp;
                        this.CacheTable[strRecPath] = info;
                        yield return new LoaderItem(info, item);
                    }
                    else
                        yield return new LoaderItem(info, item);
                }
            }
            finally
            {
                if (this.Prompt != null)
                    m_loader.Prompt -= m_loader_Prompt;
            }
        }
    }
}