using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.CirculationClient;

namespace dp2Circulation
{
    /// <summary>
    /// 图书/期刊(采购)验收窗口
    /// </summary>
    public partial class AcceptForm : Form, ILoopingHost, IChannelHost, IEnableControl
    {

        /// <summary>
        /// 获取批次号key+count值列表
        /// </summary>
        public event GetKeyCountListEventHandler GetBatchNoTable = null;

        OrderDbInfos db_infos = new OrderDbInfos();

        long m_lLoaded = 0; // 本次已经装入浏览框的条数
        long m_lHitCount = 0;   // 检索命中结果条数

#if REMOVED
        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = new LibraryChannel();
#endif
        /// <summary>
        /// 当前界面语言
        /// </summary>
        public string Lang = "zh";

#if REMOVED
        DigitalPlatform.Stop stop = null;
#endif

        EntityForm m_detailWindow = null;

        int m_nMdiClientWidth = 0;
        int m_nMdiClientHeight = 0;
        int m_nAcceptWindowHeight = 0;

        // 浏览列表，的栏目定义
        const int COLUMN_RECPATH = 0;
        const int COLUMN_ROLE = 1;
        const int COLUMN_TARGETRECPATH = 2;

        const int RESERVE_COLUMN_COUNT = 3;

        const int WM_LOAD_DETAIL = API.WM_USER + 200;
#if NOOOOOOOOOOO
        const int WM_LOAD_FINISH = API.WM_USER + 201;
#endif
        const int WM_RESTORE_SELECTION = API.WM_USER + 202;

        // ListViewItem imageindex值
        const int TYPE_SOURCE = 0;
        const int TYPE_TARGET = 1;
        const int TYPE_SOURCE_AND_TARGET = 2;
        const int TYPE_SOURCEBIBLIO = 3;   // 来自外源书目库 2009/11/5
        const int TYPE_NOT_ORDER = 4;   // 来自和采购无关的数据库 2009/11/5 changed

        /// <summary>
        /// 构造函数
        /// </summary>
        public AcceptForm()
        {
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_accept_records.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);

            Program.MainForm.AppInfo.AppInfoChanged += AppInfo_AppInfoChanged;
        }

        private void AppInfo_AppInfoChanged(object sender, DigitalPlatform.Xml.AppInfoChangedEventArgs e)
        {
            if (e.Path == "entityform_optiondlg"
                && e.Name == "quickRegister_default")
            {
                // 批次号可能被外部修改，需要刷新
                string strNewValue = EntityFormOptionDlg.GetFieldValue("quickRegister_default",
                    "batchNo");
                if (this.tabComboBox_prepare_batchNo.Text != strNewValue)
                    this.tabComboBox_prepare_batchNo.Text = strNewValue;
            }
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

        /*
        public void WaitLoadFinish()
        {
            for (; ; )
            {
                Application.DoEvents();
                bool bRet = this.EventFinish.WaitOne(10, true);
                if (bRet == true)
                    break;
            }
        }
         * */

        private void AcceptForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

#if REMOVED
            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(Program.MainForm.stopManager, true);	// 和容器关联
#endif

            bool bRet = InitialSizeParam();
            Debug.Assert(bRet == true, "");

            int nAcceptWindowHeight = Program.MainForm.AppInfo.GetInt(
                "AcceptForm",
                "accept_window_height",
                0);
            if (nAcceptWindowHeight <= 0 || nAcceptWindowHeight >= m_nMdiClientHeight)
                nAcceptWindowHeight = (int)((float)m_nMdiClientHeight * 0.3f);  // 初始化为1/3 客户区高度

            this.m_nAcceptWindowHeight = nAcceptWindowHeight;

            this.Location = new Point(0, 0);
            this.Size = new Size(m_nMdiClientWidth, m_nAcceptWindowHeight);

            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.Manual;

            this.db_infos = new OrderDbInfos();
            this.db_infos.Build(Program.MainForm);

            // batchno
            this.GetBatchNoTable -= new GetKeyCountListEventHandler(AcceptForm_GetBatchNoTable);
            this.GetBatchNoTable += new GetKeyCountListEventHandler(AcceptForm_GetBatchNoTable);

#if NOOOOOOOOOOOO
            API.PostMessage(this.Handle, WM_LOAD_FINISH, 0, 0);
#endif

#if NO
            this.tabComboBox_prepare_batchNo.Text = Program.MainForm.AppInfo.GetString(
                "accept_form",
                "batchno",
                "");

            this.comboBox_prepare_type.Text = Program.MainForm.AppInfo.GetString(
                "accept_form",
                "item_type",
                "图书");

            this.comboBox_prepare_priceDefault.Text = Program.MainForm.AppInfo.GetString(
    "accept_form",
    "price_default",
    "验收价");


            this.checkBox_prepare_inputItemBarcode.Checked = Program.MainForm.AppInfo.GetBoolean(
                "accept_form",
                "input_item_barcode",
                true);


            this.checkBox_prepare_createCallNumber.Checked = Program.MainForm.AppInfo.GetBoolean(
    "accept_form",
    "create_callnumber",
    false);


            string strFrom = Program.MainForm.AppInfo.GetString(
                "accept_form",
                "search_from",
                "");
            if (String.IsNullOrEmpty(strFrom) == false)
                this.comboBox_accept_from.Text = strFrom;

            this.comboBox_accept_matchStyle.Text = Program.MainForm.AppInfo.GetString(
                "accept_form",
                "match_style",
                "精确一致");

            SetTabPageEnabled(this.tabPage_accept, false);
            SetTabPageEnabled(this.tabPage_finish, false);
#endif
            FillDbNameList();
            FillSellerList();

            this.UiState = Program.MainForm.AppInfo.GetString(
                "accept_form",
                "ui_state",
                "");

            // 是否添加加工中状态，是例外
            this.checkBox_prepare_setProcessingState.Checked = SetProcessingState;

            // 批次号是例外，需要从默认值模板里面获得
            this.tabComboBox_prepare_batchNo.Text = EntityFormOptionDlg.GetFieldValue("quickRegister_default",
                "batchNo");

            if (string.IsNullOrEmpty(this.comboBox_accept_matchStyle.Text) == true)
                this.comboBox_accept_matchStyle.Text = "精确一致";

            SetWindowTitle();

            this.SetNextButtonEnable();

#if NO
            string strWidths = Program.MainForm.AppInfo.GetString(
                "accept_form",
                "record_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_accept_records,
                    strWidths,
                    true);
            }
#endif

            Program.MainForm.FillBiblioFromList(this.comboBox_accept_from);
            comboBox_accept_matchStyle_TextChanged(null, null);

            SetCheckBoxEnable();
        }


        void AcceptForm_GetBatchNoTable(object sender, GetKeyCountListEventArgs e)
        {
            using (var looping = Looping(out LibraryChannel channel))
            {
                Global.GetBatchNoTable(e,
                    this,
                    this.comboBox_prepare_type.Text,    // 和出版物类型有关
                    "item",
                    looping.Progress,
                    channel);
            }
        }

        bool InitialSizeParam()
        {
            /*
            // 2008/12/10 如果没有这一句，则1024X768小字体情况下会抛出异常
            if (this.MdiParent == null)
                return;

            Type t = typeof(Form);
            PropertyInfo pi = t.GetProperty("MdiClient", BindingFlags.Instance | BindingFlags.NonPublic);
            MdiClient cli = (MdiClient)pi.GetValue(this.MdiParent, null);

            this.m_nMdiClientWidth = cli.Width - 4;
            this.m_nMdiClientHeight = cli.Height - 4;
             * */

            if (Program.MainForm == null)
                return false;

            MdiClient cli = Program.MainForm.MdiClient;
            this.m_nMdiClientWidth = cli.Width - 4;
            this.m_nMdiClientHeight = cli.Height - 4;
            return true;
        }

        private void AcceptForm_FormClosing(object sender, FormClosingEventArgs e)
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
            }
            */

            // 关闭 关联的EntityForm
            bool bRet = CloseDetailWindow();
            // 如果没有关闭成功
            if (bRet == false)
                e.Cancel = true;
        }

        private void AcceptForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if REMOVED
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif

            // 2017/4/23
            this.m_detailWindow = null;

            // 2015/11/7
            if (this.panel_main != null && Program.MainForm != null)
            {
                // 如果当前固定面板拥有 panel_main，则要先解除它的拥有关系，否则怕本 Form 摧毁的时候无法 Dispose() 它
                if (Program.MainForm.CurrentAcceptControl == this.panel_main)
                    Program.MainForm.CurrentAcceptControl = null;
            }

            //
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetInt(
                    "AcceptForm",
                    "accept_window_height",
                    this.Size.Height);

#if NO
            Program.MainForm.AppInfo.SetString(
                "accept_form",
                "batchno",
                this.tabComboBox_prepare_batchNo.Text);

            Program.MainForm.AppInfo.SetString(
                "accept_form",
                "item_type",
                this.comboBox_prepare_type.Text);

            Program.MainForm.AppInfo.SetString(
"accept_form",
"price_default",
this.comboBox_prepare_priceDefault.Text);

            Program.MainForm.AppInfo.SetBoolean(
                "accept_form",
                "input_item_barcode",
                this.checkBox_prepare_inputItemBarcode.Checked);

            Program.MainForm.AppInfo.SetBoolean(
                "accept_form",
                "set_processing_state",
                this.checkBox_prepare_setProcessingState.Checked);

            Program.MainForm.AppInfo.SetBoolean(
"accept_form",
"create_callnumber",
this.checkBox_prepare_createCallNumber.Checked);

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_accept_records);
            Program.MainForm.AppInfo.SetString(
                "accept_form",
                "record_list_column_width",
                strWidths);

            Program.MainForm.AppInfo.SetString(
                "accept_form",
                "search_from",
                this.comboBox_accept_from.Text);

            Program.MainForm.AppInfo.SetString(
                "accept_form",
                "match_style",
                this.comboBox_accept_matchStyle.Text);
#endif
                Program.MainForm.AppInfo.SetString(
        "accept_form",
        "ui_state",
        this.UiState);

                Program.MainForm.AppInfo.AppInfoChanged -= AppInfo_AppInfoChanged;

                // 是否添加加工中状态，是例外，要单独保存
                SetProcessingState = this.checkBox_prepare_setProcessingState.Checked;

                // 批次号是例外，需要保存到默认值模板
                EntityFormOptionDlg.SetFieldValue("quickRegister_default",
    "batchNo",
    this.tabComboBox_prepare_batchNo.Text);
            }
        }

        /// <summary>
        /// 获取或设置控件尺寸状态
        /// </summary>
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.tabComboBox_prepare_batchNo);
                controls.Add(this.comboBox_prepare_type);
                controls.Add(this.comboBox_prepare_priceDefault);
                controls.Add(this.checkBox_prepare_inputItemBarcode);
                // controls.Add(checkBox_prepare_setProcessingState);
                controls.Add(checkBox_prepare_createCallNumber);
                controls.Add(listView_accept_records);
                controls.Add(new ComboBoxText(comboBox_accept_from));
                controls.Add(comboBox_accept_matchStyle);
                controls.Add(this.checkedListBox_prepare_dbNames);
                controls.Add(this.comboBox_sellerFilter);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.tabComboBox_prepare_batchNo);
                controls.Add(this.comboBox_prepare_type);
                controls.Add(this.comboBox_prepare_priceDefault);
                controls.Add(this.checkBox_prepare_inputItemBarcode);
                // controls.Add(checkBox_prepare_setProcessingState);
                controls.Add(checkBox_prepare_createCallNumber);
                controls.Add(listView_accept_records);
                controls.Add(new ComboBoxText(comboBox_accept_from));
                controls.Add(comboBox_accept_matchStyle);
                controls.Add(this.checkedListBox_prepare_dbNames);
                controls.Add(this.comboBox_sellerFilter);
                GuiState.SetUiState(controls, value);
            }
        }

#if REMOVED

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            Program.MainForm.Channel_BeforeLogin(sender, e);    // 2015/11/8
        }

        void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            Program.MainForm.Channel_AfterLogin(sender, e);    // 2015/11/8
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif
        public void TryInvoke(Action method)
        {
            if (this.InvokeRequired)
                this.Invoke((Action)(method));
            else
                method.Invoke();
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            TryInvoke(() =>
            {
                // page prepare
                this.tabComboBox_prepare_batchNo.Enabled = bEnable;
                this.comboBox_prepare_type.Enabled = bEnable;
                this.checkBox_prepare_inputItemBarcode.Enabled = bEnable;
                this.checkBox_prepare_setProcessingState.Enabled = bEnable;
                this.checkedListBox_prepare_dbNames.Enabled = bEnable;

                // page accept
                this.textBox_accept_queryWord.Enabled = bEnable;
                this.button_accept_searchISBN.Enabled = bEnable;
                // this.listView_accept_records.Enabled = bEnable;

                this.comboBox_accept_from.Enabled = bEnable;
                this.comboBox_accept_matchStyle.Enabled = bEnable;

                // page finish
                this.button_finish_printAcceptList.Enabled = bEnable;

                // next button
                if (bEnable == true)
                {
                    SetNextButtonEnable();
                }
                else
                    this.button_next.Enabled = false;
            });
        }

        static void SetTabPageEnabled(TabPage page,
            bool bEnable)
        {
            foreach (Control control in page.Controls)
            {
                control.Enabled = bEnable;
            }
        }

        void SetNextButtonEnable()
        {
            // string strError = "";

            this.button_next.Text = "下一环节(&N)";

            if (this.tabComboBox_prepare_batchNo.Text == ""
    || this.comboBox_prepare_type.Text == "")
            {
                this.button_next.Enabled = true;

                // this.button_next.Enabled = false;
                SetTabPageEnabled(this.tabPage_accept, false);
                SetTabPageEnabled(this.tabPage_finish, false);
            }
            else
            {
                this.button_next.Enabled = true;

                // this.button_next.Enabled = true;
                SetTabPageEnabled(this.tabPage_accept, true);
                SetTabPageEnabled(this.tabPage_finish, true);
            }


            if (this.tabControl_main.SelectedTab == this.tabPage_prepare)
            {
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_accept)
            {
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_finish)
            {
                this.button_next.Text = "关闭(&X)";
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }

        }

        private void textBox_accept_isbn_Enter(object sender, EventArgs e)
        {
            // 把“检索按钮”设为缺省按钮
            Button oldButton = (Button)this.AcceptButton;

            this.AcceptButton = this.button_accept_searchISBN;

            ((Button)this.AcceptButton).Font = new Font(((Button)this.AcceptButton).Font,
                FontStyle.Bold);
            if (oldButton != null)
            {
                oldButton.Font = new Font(oldButton.Font,
                    FontStyle.Regular);
            }
        }

        private void textBox_accept_isbn_Leave(object sender, EventArgs e)
        {
            // 把“下一环节”设为缺省按钮
            Button oldButton = (Button)this.AcceptButton;

            this.AcceptButton = this.button_next;

            ((Button)this.AcceptButton).Font = new Font(((Button)this.AcceptButton).Font,
                FontStyle.Bold);
            if (oldButton != null)
            {
                oldButton.Font = new Font(oldButton.Font,
                    FontStyle.Regular);
            }
        }

        // 
        /// <summary>
        /// 在 ListView 最前面插入一行
        /// </summary>
        /// <param name="list">ListView 对象</param>
        /// <param name="strID">ID列内容</param>
        /// <param name="others">其他列内容</param>
        /// <returns>新插入的 ListViewItem 对象</returns>
        public static ListViewItem InsertNewLine(
            ListView list,
            string strID,
            string[] others)
        {
            if (others != null)
                ListViewUtil.EnsureColumns(list, others.Length + RESERVE_COLUMN_COUNT);

            ListViewItem item = new ListViewItem(strID, 0);

            // item.SubItems.Add("");  // 角色
            list.Items.Insert(0, item);

            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    ListViewUtil.ChangeItemText(item,
                        i + RESERVE_COLUMN_COUNT,
                        others[i]);
                }
            }

            return item;
        }

        // 
        /// <summary>
        /// 在 ListView 最后追加一行
        /// </summary>
        /// <param name="list">ListView 对象</param>
        /// <param name="strID">ID列内容</param>
        /// <param name="others">其他列内容</param>
        /// <returns>新插入的 ListViewItem 对象</returns>
        public static ListViewItem AppendNewLine(
            ListView list,
            string strID,
            string[] others)
        {
            if (others != null)
                ListViewUtil.EnsureColumns(list, others.Length + RESERVE_COLUMN_COUNT);

            ListViewItem item = new ListViewItem(strID, 0);
            // item.SubItems.Add("");  // 角色

            list.Items.Add(item);

            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    ListViewUtil.ChangeItemText(item,
                        i + RESERVE_COLUMN_COUNT,
                        others[i]);
                }
            }

            return item;
        }

        string GetCurrentMatchStyle()
        {
            string strText = this.comboBox_accept_matchStyle.Text;

            // 2009/8/6
            if (strText == "空值")
                return "null";

            if (String.IsNullOrEmpty(strText) == true)
                return "exact"; // 缺省时认为是 精确一致

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

        // return:
        //      -1  error
        //      0   未命中
        //      >0  命中记录条数
        int DoSearch(out string strError)
        {
            strError = "";
            long lHitCount = 0;

            bool bQuickLoad = false;

            // 修改窗口标题
            this.Text = "正在验收 " + this.textBox_accept_queryWord.Text;

            this.listView_accept_records.Items.Clear();
            ListViewUtil.ClearSortColumns(this.listView_accept_records);

            this.m_lHitCount = 0;
            this.m_lLoaded = 0;

            /*
            stop.HideProgress();
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 " + this.textBox_accept_queryWord.Text + " ...");
            stop.BeginLoop();

            this.EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在检索 " + this.textBox_accept_queryWord.Text + " ...",
                "disableControl");
            try
            {
                if (this.comboBox_accept_from.Text == "")
                {
                    strError = "尚未选定检索途径";
                    return -1;
                }

                string strFromStyle = "";

                try
                {
                    strFromStyle = BiblioSearchForm.GetBiblioFromStyle(this.comboBox_accept_from.Text);
                }
                catch (Exception ex)
                {
                    strError = "AcceptForm GetBiblioFromStyle() exception: " + ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strError = "GetFromStyle()没有找到 '" + this.comboBox_accept_from.Text + "' 对应的style字符串";
                    return -1;
                }

                /*
                string strFromStyle = "isbn";

                if (this.comboBox_prepare_type.Text == "图书")
                    strFromStyle = "isbn";
                else
                {
                    Debug.Assert(this.comboBox_prepare_type.Text == "连续出版物", "");
                    strFromStyle = "issn";
                }*/

                // 注："null"只能在前端短暂存在，而内核是不认这个所谓的matchstyle的
                string strMatchStyle = GetCurrentMatchStyle();

                if (this.textBox_accept_queryWord.Text == "")
                {
                    if (strMatchStyle == "null")
                    {
                        this.textBox_accept_queryWord.Text = "";

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
                    // 2009/11/5
                    if (strMatchStyle == "null")
                    {
                        strError = "检索空值的时候，请保持检索词为空";
                        return -1;
                    }
                }

                long lRet = channel.SearchBiblio(
                    looping.Progress,
                    GetDbNameListString(),  // "<全部>",
                    this.textBox_accept_queryWord.Text,
                    1000,   // this.MaxSearchResultCount,  // 1000
                    strFromStyle,
                    strMatchStyle,  // "exact",
                    this.Lang,
                    "accept",   // strResultSetName
                    "",    // strSearchStyle
                    "", // strOutputStyle
                    "",
                    out string strQueryXml,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                {
                    strError = "未命中";
                    return 0;
                }

                lHitCount = lRet;

                this.m_lHitCount = lHitCount;

                // 显示前半程
                looping.Progress.SetProgressRange(0, lHitCount * 2);

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                Record[] searchresults = null;

                bool bPushFillingBrowse = true; //  this.PushFillingBrowse;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    looping.Progress.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    string strStyle = "id,cols";

                    if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                        strStyle = "id";

                    lRet = channel.GetSearchResult(
                        looping.Progress,
                        "accept",   // strResultSetName
                        lStart,
                        lPerCount,
                        strStyle,
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        return 0;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        ListViewItem item = null;
                        if (bPushFillingBrowse == true)
                        {
                            if (bQuickLoad == true)
                                item = InsertNewLine(
                                    (ListView)this.listView_accept_records,
                                    searchresults[i].Path,
                                    searchresults[i].Cols);
                            else
                                item = InsertNewLine(
                                    this.listView_accept_records,
                                    searchresults[i].Path,
                                    searchresults[i].Cols);
                        }
                        else
                        {
                            if (bQuickLoad == true)
                                item = AppendNewLine(
                                    (ListView)this.listView_accept_records,
                                    searchresults[i].Path,
                                    searchresults[i].Cols);
                            else
                                item = AppendNewLine(
                                    this.listView_accept_records,
                                    searchresults[i].Path,
                                    searchresults[i].Cols);
                        }

                        // 
                        // 根据记录路径，获得ListViewItem事项的imageindex下标
                        // return:
                        //      -2  根本不是书目库
                        //      -1  不是采购源或目标的其它书目库
                        //      0   源
                        //      1   目标
                        //      2   同时为源和目标
                        //      3   外源
                        int image_index = this.db_infos.GetItemType(searchresults[i].Path,
                            this.comboBox_prepare_type.Text);
                        Debug.Assert(image_index != -2, "居然检索到非书目库的记录?");
                        item.ImageIndex = image_index;

                        SetItemColor(item); //
                    }

                    lStart += searchresults.Length;
                    // lCount -= searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;

                    this.m_lLoaded = lStart;
                    looping.Progress.SetProgressValue(lStart);
                }

                // this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录";
                return (int)lHitCount;
            }
            finally
            {
                looping.Dispose();
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
                */
            }
        }

        int FilterOneItem(
            Stop stop,
            LibraryChannel channel,
            ListViewItem item,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strRecPath = ListViewUtil.GetItemText(item,
                COLUMN_RECPATH);
            // 根据记录路径，获得ListViewItem事项的imageindex下标
            // return:
            //      -2  根本不是书目库
            //      -1  不是采购源或目标的其它书目库
            //      0   源
            //      1   目标
            //      2   同时为源和目标
            //      3   外源
            int image_index = this.db_infos.GetItemType(strRecPath,
                            this.comboBox_prepare_type.Text);
            // 是潜在的源
            if (image_index == 0 || image_index == 2)
            {
                // 获得998$t
                string strTargetRecPath = "";
                long lRet = channel.GetBiblioInfo(
                    stop,
                    strRecPath,
                    "", // strBiblioXml
                    "targetrecpath",   // strResultType
                    out strTargetRecPath,
                    out strError);
                ListViewUtil.ChangeItemText(item, COLUMN_TARGETRECPATH, strTargetRecPath);
            }

            // 是潜在的源
            if (image_index == 0 || image_index == 2)
            {
                // 检查是否具备采购信息
                // 装入订购记录，检查是否有订购信息
                // parameters:
                //      strSellerList   书商名称列表。逗号分割的字符串。如果为null，表示不对书商名称进行过滤
                // return:
                //      -1  出错
                //      0   没有(符合要求的)订购信息
                //      >0  有这么多条符合要求的订购信息
                nRet = LoadOrderRecords(
                    stop,
                    channel,
                    strRecPath,
                    null,   // strSellerList,
                    out strError);
                if (nRet == -1)
                    return -1;

                RecordInfo info = GetRecordInfo(item);
                info.HasOrderInfo = ((nRet == 0) ? false : true);

                if (nRet == 0)
                    SetItemColor(item);
            }

            return 0;
        }

        // 过滤所有潜在源记录，如果没有采购信息，或者采购信息和特定渠道不吻合，则行变为灰色
        int FilterOrderInfo(out string strError)
        {
            strError = "";
            int nRet = 0;

            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在过滤记录 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在过滤记录 ...",
                "disableControl");

            try
            {
                // 显示后半程
                looping.Progress.SetProgressRange(0, this.listView_accept_records.Items.Count * 2);
                for (int i = 0; i < this.listView_accept_records.Items.Count; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (looping.Stopped)
                        return 0;

                    ListViewItem item = this.listView_accept_records.Items[i];

                    /*
                    string strRecPath = ListViewUtil.GetItemText(item,
                        COLUMN_RECPATH);
                    // 根据记录路径，获得ListViewItem事项的imageindex下标
                    // return:
                    //      -2  根本不是书目库
                    //      -1  不是采购源或目标的其它书目库
                    //      0   源
                    //      1   目标
                    //      2   同时为源和目标
                    //      3   外源
                    int image_index = this.db_infos.GetItemType(strRecPath,
                                    this.comboBox_prepare_type.Text);
                    // 是潜在的源
                    if (image_index == 0 || image_index == 2)
                    {
                        // 检查是否具备采购信息
                        // 装入订购记录，检查是否有订购信息
                        // parameters:
                        //      strSellerList   书商名称列表。逗号分割的字符串。如果为null，表示不对书商名称进行过滤
                        // return:
                        //      -1  出错
                        //      0   没有(符合要求的)订购信息
                        //      >0  有这么多条符合要求的订购信息
                        nRet = LoadOrderRecords(strRecPath,
                            null,   // strSellerList,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        RecordInfo info = GetRecordInfo(item);
                        info.HasOrderInfo = ((nRet == 0) ? false : true);

                        if (nRet == 0)
                            SetItemColor(item);
                    }


                    // 是潜在的源
                    if (image_index == 0 || image_index == 2)
                    {
                        // 获得998$t
                        string strTargetRecPath = "";
                        long lRet = Channel.GetBiblioInfo(
                            stop,
                            strRecPath,
                            "", // strBiblioXml
                            "targetrecpath",   // strResultType
                            out strTargetRecPath,
                            out strError);
                        ListViewUtil.ChangeItemText(item, COLUMN_TARGETRECPATH, strTargetRecPath);
                    }
                     * */
                    nRet = FilterOneItem(
                        looping.Progress,
                        channel,
                        item, out strError);
                    if (nRet == -1)
                        return -1;

                    /*
                    string strRole = ListViewUtil.GetItemText(item,
        COLUMN_ROLE);
                     * */

                    looping.Progress.SetProgressValue(this.listView_accept_records.Items.Count + i);
                }
                return 1;
            }
            finally
            {
                looping.Dispose();
                /*
                this.EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                */
            }

        }

        // 装入订购记录，检查是否有订购信息
        // parameters:
        //      strSellerList   书商名称列表。逗号分割的字符串。如果为null，表示不对书商名称进行过滤
        // return:
        //      -1  出错
        //      0   没有(符合要求的)订购信息
        //      >0  有这么多条符合要求的订购信息
        /*public*/
        int LoadOrderRecords(
            Stop stop,
            LibraryChannel channel,
            string strBiblioRecPath,
            string strSellerList,
            out string strError)
        {
            int nCount = 0;

            stop?.SetMessage("正在装入书目记录 '" + strBiblioRecPath + "' 下属的订购信息 ...");

            // string strHtml = "";
            long lStart = 0;
            long lResultCount = 0;
            long lCount = -1;

            // 2012/5/9 改写为循环方式
            for (; ; )
            {
                long lRet = channel.GetOrders(
                    stop,
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    "",
                    "zh",
                    out EntityInfo[] orders,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                    return 0;

                lResultCount = lRet;

                Debug.Assert(orders != null, "");

                // 优化：如果不需要过滤书商名，就不必装入XML记录到DOM中进行剖析了
                if (strSellerList == null)
                    return orders.Length;

                for (int i = 0; i < orders.Length; i++)
                {
                    if (orders[i].ErrorCode != ErrorCodeValue.NoError)
                    {
                        strError = "路径为 '" + orders[i].OldRecPath + "' 的订购记录装载中发生错误: " + orders[i].ErrorInfo;  // NewRecPath
                        return -1;
                    }

                    // 剖析一个订购xml记录，取出有关信息放入listview中
                    OrderItem orderitem = new OrderItem();

                    int nRet = orderitem.SetData(orders[i].OldRecPath, // NewRecPath
                             orders[i].OldRecord,
                             orders[i].OldTimestamp,
                             out strError);
                    if (nRet == -1)
                        return -1;

                    if (orders[i].ErrorCode == ErrorCodeValue.NoError)
                        orderitem.Error = null;
                    else
                        orderitem.Error = orders[i];

                    if (strSellerList != null)
                    {
                        if (StringUtil.IsInList(orderitem.Seller, strSellerList) == false)
                            continue;
                    }

                    // TODO: 已经数量到齐的，是否不计入订购信息？

                    nCount++;
                    /*
                    this.orderitems.Add(orderitem);
                    orderitem.AddToListView(this.ListView);
                     * */
                }

                lStart += orders.Length;
                if (lStart >= lResultCount)
                    break;
            }
            return nCount;
        ERROR1:
            return -1;
        }

        private void button_accept_searchISBN_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_accept_queryWord.Text == ""
                && this.comboBox_accept_matchStyle.Text != "空值")
            {
                strError = "尚未输入检索词";
                goto ERROR1;
            }

            if (m_detailWindow != null
                && m_detailWindow.IsDisposed == false)
            {
#if NO
                if (m_detailWindow.IsLoading == true)
                {
                    strError = "当前种册窗正在装载记录，请稍候再重试检索";
                    goto ERROR1;
                }
#endif
            }

            // 迫使detailWindow保存
            // TODO: 将来是否允许用户checkbox决定是否“自动保存”?
            SaveDetailWindowChanges();

            // 当前detailWindow内容清空
            ClearDetailWindow(true);

            ClearSourceTarget();    // 2009/6/2

            int nRet = DoSearch(out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
                MessageBox.Show(this, "检索词 '" + this.textBox_accept_queryWord.Text + "' 未命中任何记录");

            nRet = FilterOrderInfo(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /*
        // 交换垂直/水平布局风格
        private void splitContainer_accept_multiRecords_DoubleClick(object sender, EventArgs e)
        {
            if (this.splitContainer_accept_multiRecords.Orientation == Orientation.Horizontal)
                this.splitContainer_accept_multiRecords.Orientation = Orientation.Vertical;
            else
                this.splitContainer_accept_multiRecords.Orientation = Orientation.Horizontal;
        }*/

        bool CloseDetailWindow()
        {
            // 关闭 关联的EntityForm
            if (m_detailWindow != null)
            {
                if (m_detailWindow.IsDisposed == false)
                {
                    m_detailWindow.Close();

                    // 2009/2/3
                    if (m_detailWindow.IsDisposed == false)
                        return false;   // 没有关闭成功。比方说警告了尚未保存？用户选择Cancel


                    // TODO: 要看看是否真关闭了?
                    // 通过Hashcode或者对象指针来观察?

                    m_detailWindow = null;
                }
                else
                    m_detailWindow = null;
            }

            return true;
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
#if NOOOOOOOOOO
                case WM_LOAD_FINISH:
                    EventFinish.Set();
                    return;
#endif

                case WM_RESTORE_SELECTION:
                    RestoreSelection();
                    return;
                case WM_LOAD_DETAIL:
                    {
                        /*
                        if (this.listView_accept_records.Enabled == false)
                            return; // 丢失
                         * */

                        int index = m.LParam.ToInt32();

                        if (index == -1)
                        {
                            this.LoadDetail(index);
                            return;
                        }

                        /*
                        // 保存焦点状态
                        bool bFouced = this.listView_accept_records.Focused;

                        this.listView_accept_records.Enabled = false;
                         * */

                        bool bRet = this.LoadDetail(index);

                        /*
                        this.listView_accept_records.Enabled = true;

                        if (bRet == false && index != -1)
                        {
                            API.PostMessage(this.Handle, WM_RESTORE_SELECTION, 0, 0);
                            return;
                        }

                        if (this.m_detailWindow != null)
                            this.m_detailWindow.TargetRecPath = this.GetTargetRecPath();

                        // 恢复焦点状态
                        if (bFouced == true)
                            this.listView_accept_records.Focus();
                         * */
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        // 清空当前详细窗内容
        // 2009/2/3
        bool ClearDetailWindow(bool bWarningNotSave)
        {
            if (this.m_detailWindow == null)
                return true;

            if (m_detailWindow != null
                && m_detailWindow.IsDisposed == true)
            {
                m_detailWindow = null;
                return true;
            }

            bool bRet = this.m_detailWindow.Clear(bWarningNotSave);
            if (bRet == false)
                return false;

            this.m_detailWindow.BiblioRecPath = "";
            return true;
        }

        // 强制当前详细窗内保存发生过的修改
        // 2009/2/3
        void SaveDetailWindowChanges()
        {
            if (this.m_detailWindow == null)
                return;

            if (m_detailWindow != null
                && m_detailWindow.IsDisposed == true)
            {
                m_detailWindow = null;
                return;
            }

            if (this.m_detailWindow.Changed == true)
            {
                this.EnableControls(false); // 防止反复按检索按钮
                try
                {
                    this.m_detailWindow.DoSaveAll();
                }
                finally
                {
                    this.EnableControls(true);
                }
            }

        }

        // 根据详细窗内的书目记录路径，恢复对listview内对应事项的选择
        void RestoreSelection()
        {
            if (this.m_detailWindow == null)
                return;

            if (m_detailWindow != null
                && m_detailWindow.IsDisposed == true)
            {
                m_detailWindow = null;
                return;
            }

            string strBiblioRecPath = this.m_detailWindow.BiblioRecPath;

            ListViewItem item = ListViewUtil.FindItem(this.listView_accept_records,
                strBiblioRecPath,
                COLUMN_RECPATH);
            if (item == null)
                return;

            if (item.Selected == true && this.listView_accept_records.SelectedItems.Count == 1)
                return;

            // this.listView_accept_records.SelectedItems.Clear();
            for (int i = 0; i < this.listView_accept_records.Items.Count; i++)
            {
                ListViewItem cur_item = this.listView_accept_records.Items[i];
                if (cur_item == item)
                    continue;
                cur_item.Selected = false;
            }
            item.Selected = true;
        }

        /*public*/
        delegate int Delegate_SafeLoadRecord(string strBiblioRecPath,
            string strPrevNextStyle);

        bool LoadDetail(int index)
        {
            if (index == -1 || index >= this.listView_accept_records.Items.Count)
            {
                if (this.SingleClickLoadDetail == true)
                {
                    EntityForm detail_window = this.DetailWindow;

                    if (detail_window != null)
                        detail_window.ReadOnly = true;
                }
                else
                    CloseDetailWindow();

                return false;
            }

            string strPath = this.listView_accept_records.Items[index].SubItems[0].Text;

            OpenDetailWindow();

            // 详细窗内本来就是这条记录，不用反复装载
            if (m_detailWindow.BiblioRecPath == strPath)
                return true;

            Delegate_SafeLoadRecord d = new Delegate_SafeLoadRecord(m_detailWindow.SafeLoadRecord);
            m_detailWindow.BeginInvoke(d, new object[] { strPath,
                "" });
            return true;

            /*
            m_detailWindow.SafeLoadRecord(strPath, "");
            return true;
             * */

            /*

            // return:
            //      -1  出错。已经用MessageBox报错
            //      0   没有装载
            //      1   成功装载
            int nRet = m_detailWindow.LoadRecord(strPath,
                "",
                true);
            if (nRet != 1)
                return false;


            return true;
             * */

            // TODO: 不管是否验收，预先设好EntityForm内的AcceptBatchNo?
        }

        // 用户在list records中点选了记录
        private void listView_accept_records_SelectedIndexChanged(object sender, EventArgs e)
        {

            List<int> protect_column_numbers = new List<int>();
            protect_column_numbers.Add(COLUMN_ROLE);  // 保护“角色”列
            protect_column_numbers.Add(COLUMN_TARGETRECPATH);  // 保护“目标路径”列
            ListViewUtil.OnSelectedIndexChanged(this.listView_accept_records, 0, protect_column_numbers);

            if (this.SingleClickLoadDetail == false)
                return;

            if (this.listView_accept_records.SelectedItems.Count == 0
                || this.listView_accept_records.SelectedItems.Count > 1)    // 2009/2/3 多选时也要禁止进入详细窗
            {
                /*
                EntityForm detail_window = this.DetailWindow;

                if (detail_window != null)
                    detail_window.Enabled = false;
                */
                API.PostMessage(this.Handle, WM_LOAD_DETAIL, 0, -1);
                return;
            }

            /*
            string strPath = this.listView_accept_records.SelectedItems[0].SubItems[0].Text;

            OpenDetailWindow();

            // return:
            //      -1  出错。已经用MessageBox报错
            //      0   没有装载
            //      1   成功装载
            m_detailWindow.LoadRecord(strPath,
                "");
             * */
            API.PostMessage(this.Handle, WM_LOAD_DETAIL, 0, this.listView_accept_records.SelectedIndices[0]);

        }

        void OpenDetailWindow()
        {
            if (m_detailWindow != null)
            {
                if (m_detailWindow.IsDisposed == true)
                {
                    m_detailWindow = null;
                }
            }


            // TODO: 打开一个EntityForm，然后定位在预定的位置。和当前窗口是兄弟关系
            if (m_detailWindow == null)
            {
                bool bExistOldEntityForm = false;
                if (Program.MainForm.GetTopChildWindow<EntityForm>() != null)
                {
                    bExistOldEntityForm = true;
                }

                m_detailWindow = new EntityForm();

                m_detailWindow.AcceptMode = true;
                m_detailWindow.MainForm = Program.MainForm;
                m_detailWindow.MdiParent = Program.MainForm;
#if ACCEPT_MODE

                m_detailWindow.FormBorderStyle = FormBorderStyle.None;

                m_detailWindow.Location = new Point(0, m_nAcceptWindowHeight);
                m_detailWindow.Size = new Size(m_nMdiClientWidth, m_nMdiClientHeight - m_nAcceptWindowHeight);
                m_detailWindow.StartPosition = FormStartPosition.Manual;
#else

#endif

                m_detailWindow.Show();
                if (true)
                {
                    /*
                     * 2011/4/14 邮件
                     *2.2）先打开“验收”功能窗，然后打开一个其他窗口，如“种册
窗”，把种册窗最大化，之后再关闭，在“验收”功能窗“验收”阶段输入ISBN号检
索，双击订购记录浏览的信息打开详细窗，子窗口没有铺满。（截图见附件2.jpg）
                     * */
#if ACCEPT_MODE
                    m_detailWindow.WindowState = FormWindowState.Normal;
                    m_detailWindow.Location = new Point(0, m_nAcceptWindowHeight);
                    m_detailWindow.Size = new Size(m_nMdiClientWidth, m_nMdiClientHeight - m_nAcceptWindowHeight);
#else
                    m_detailWindow.WindowState = FormWindowState.Maximized;
#endif
                }


                m_detailWindow.OrderControl.PrepareAccept -= new PrepareAcceptEventHandler(m_detailWindow_PrepareAccept);
                m_detailWindow.OrderControl.PrepareAccept += new PrepareAcceptEventHandler(m_detailWindow_PrepareAccept);

                m_detailWindow.IssueControl.PrepareAccept -= new PrepareAcceptEventHandler(m_detailWindow_PrepareAccept);
                m_detailWindow.IssueControl.PrepareAccept += new PrepareAcceptEventHandler(m_detailWindow_PrepareAccept);

                // 2011/4/18
                if (bExistOldEntityForm == true)
                {
                    /*
                     * 2011/4/14 邮件
                    2.1）对订购信息进行验收操作中，先打开一个其他窗口，如“种册
窗”，然后打开“验收”功能窗（批处理-验收），激活打开的“种册窗”，使“种册
窗”成为当前窗口，并将它最大化，使用工具菜单“窗口-验收”切换回“验收”功能
窗，在“验收”阶段输入ISBN号检索，双击订购记录浏览的信息打开详细窗，这时下面
的种册窗也翻了上来。（截图见附件1.jpg）。 
                     * */
                    this.Activate();
                    m_detailWindow.Activate();
                }
            }
            else
            {
                if (m_detailWindow.ReadOnly == true)
                    m_detailWindow.ReadOnly = false;
            }

        }

        // 检查源记录的998$t，看看当前列表中是否已经有这条记录，如果没有，则需要装入
        // return:
        //      -1  出错(但是事项可能已经加入)
        //      0   源记录没有998$t
        //      1   本函数调用前目标记录已经在列表中存在
        //      2   新装入了目标记录
        int AutoLoadTarget(string strSourceRecPath,
            out string strTargetRecPath,
            out string strError)
        {
            strError = "";
            strTargetRecPath = "";
            int nRet = 0;

            ListViewItem source_item = ListViewUtil.FindItem(this.listView_accept_records,
                strSourceRecPath, COLUMN_RECPATH);
            if (source_item == null)
            {
                strError = "当前列表中居然没有路径为 '" + strSourceRecPath + "' 的事项";
                return -1;
            }

            strTargetRecPath = ListViewUtil.GetItemText(source_item,
                COLUMN_TARGETRECPATH);
            if (String.IsNullOrEmpty(strTargetRecPath) == true)
                return 0;

            ListViewItem target_item = ListViewUtil.FindItem(this.listView_accept_records,
                strTargetRecPath, COLUMN_RECPATH);

            if (target_item != null)
                return 1;
            else
            {
                // 插入一行，在源记录行以后
                target_item = new ListViewItem();
                ListViewUtil.ChangeItemText(target_item, COLUMN_RECPATH, strTargetRecPath);
                int index = this.listView_accept_records.Items.IndexOf(source_item);
                Debug.Assert(index != -1, "");
                index++;
                this.listView_accept_records.Items.Insert(index, target_item);

                /*
                this.EnableControls(false);
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在装载记录 '" + strTargetRecPath + "' ...");
                stop.BeginLoop();
                */
                var looping = Looping(
                    out LibraryChannel channel,
                    "正在装载记录 '" + strTargetRecPath + "' ...",
                    "disableControl");
                try
                {
                    nRet = RefreshBrowseLine(
                        looping.Progress,
                        channel,
                        target_item, out strError);
                    if (nRet == -1)
                    {
                        ListViewUtil.ChangeItemText(target_item, 2, strError);
                        return -1;
                    }
                }
                finally
                {
                    looping.Dispose();
                    /*
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                    this.EnableControls(true);
                    */
                }

                return 2;
            }
        }

        // 自动准备目标记录
        // parameters:
        //      strBiblioSourceRecPath  [out]当前列表中操作者设定的外源书目记录路径
        // return:
        //      -1  error
        //      0   函数调用前已经选定了目标事项，不必自动准备目标
        //      1   准备好了目标事项
        //      2   无法准备目标事项，条件不具备
        int AutoPrepareAccept(
            string strSourceRecPath,
            out string strTargetRecPath,
            out string strBiblioSourceRecPath,
            out string strError)
        {
            strError = "";
            strTargetRecPath = "";
            strBiblioSourceRecPath = "";

            int nRet = 0;

            bool bSeriesMode = false;
            if (this.comboBox_prepare_type.Text == "连续出版物")
                bSeriesMode = true;
            else
                bSeriesMode = false;

            strBiblioSourceRecPath = GetBiblioSourceRecPath();
            if (bSeriesMode == true
                && String.IsNullOrEmpty(strBiblioSourceRecPath) == false)
            {
                strError = "对于连续出版物，不支持 外源角色";
                return -1;
            }

            if (String.IsNullOrEmpty(strSourceRecPath) == true)
            {
                strError = "strSourceRecPath参数值不能为空";
                return -1;
            }

            string str998TargetRecPath = "";
            // 检查源记录的998$t，看看当前列表中是否已经有这条记录，如果没有，则需要装入
            // return:
            //      -1  出错(但是事项可能已经加入)
            //      0   源记录没有998$t
            //      1   本函数调用前目标记录已经在列表中存在
            //      2   新装入了目标记录
            nRet = AutoLoadTarget(strSourceRecPath,
                out str998TargetRecPath,
                out strError);
            if (nRet == -1)
                return -1;

            strTargetRecPath = GetTargetRecPath();
            if (String.IsNullOrEmpty(strTargetRecPath) == false)
            {
                // 当前操作者已经选定了目标事项，就不属于本函数要操心的情形了
                strError = "当前已经选定了目标事项";
                return 0;
            }

            string strSourceDbName = Global.GetDbName(strSourceRecPath);
            OrderDbInfo source_dbinfo = this.db_infos.LocateByBiblioDbName(strSourceDbName);
            if (source_dbinfo == null)
            {
                strError = "在this.db_infos中竟然没有找到名字为 " + strSourceDbName + " 的书目库对象";
                return -1;
            }

#if DEBUG
            if (String.IsNullOrEmpty(source_dbinfo.IssueDbName) == false)
            {
                Debug.Assert(this.comboBox_prepare_type.Text == "连续出版物", "");
            }
            else
            {
                Debug.Assert(this.comboBox_prepare_type.Text == "图书", "");
            }
#endif

            // 源记录来自 采购工作库
            if (source_dbinfo.IsOrderWork == true)
            {
                // 即便源记录的998$t有指向，也自动采用源记录作为目标
                strError = "源记录和目标记录是同一个: " + strSourceRecPath + "。源记录来自采购工作库";
                strTargetRecPath = strSourceRecPath;
                nRet = SetTarget(strTargetRecPath, out strError);
                if (nRet == -1)
                    return -1;
                return 1;
            }

            // 源记录并不是来自采购工作库，那么就要尽量依源记录中的998$t定义
            if (String.IsNullOrEmpty(str998TargetRecPath) == false)
            {
                strTargetRecPath = str998TargetRecPath;
                nRet = SetTarget(strTargetRecPath, out strError);
                if (nRet == -1)
                    return -1;
                strError = "源记录 '" + strSourceRecPath + "' 中的998$t指向 '" + str998TargetRecPath + "'，那么就把后者作为目标记录了";
                return 1;
            }

            // 看看源库是不是同时也是目标库？如果是，直接把源记录作为目标记录
            if (source_dbinfo.IsSourceAndTarget == true)
            {
                strError = "源记录的所在库同时具备源和目标的角色，因此源记录和目标记录是同一个: " + strSourceRecPath;
                strTargetRecPath = strSourceRecPath;
                nRet = SetTarget(strTargetRecPath, out strError);
                if (nRet == -1)
                    return -1;
                return 1;
            }

            int nSourceItemCount = 0;   // 源角色事项数目。不包括双角色事项数目
            int nTargetItemCount = 0;   // 目标角色事项数目。不包括双角色事项数目
            int nSourceAndTargetItemCount = 0;    // 同时具备两个角色的事项数目
            for (int i = 0; i < this.listView_accept_records.Items.Count; i++)
            {
                ListViewItem item = this.listView_accept_records.Items[i];

                if (item.ImageIndex == TYPE_SOURCE)
                    nSourceItemCount++;
                if (item.ImageIndex == TYPE_TARGET)
                    nTargetItemCount++;
                if (item.ImageIndex == TYPE_SOURCE_AND_TARGET)
                    nSourceAndTargetItemCount++;
            }

            // 当前列表内根本没有潜在目标记录
            if (nTargetItemCount + nSourceAndTargetItemCount == 0)
            {
                // 需要找到一个目标库，构造新记录路径
                // 如果潜在的目标库很多，则需要用户选择
                List<string> target_dbnames = this.db_infos.GetTargetDbNames();
                if (target_dbnames.Count == 0)
                {
                    strError = "当前服务器中没有配置适合作为验收目标库(也就是包含实体库)的书目库，无法进行验收";
                    return 2;
                }

                if (target_dbnames.Count == 1)
                {
                    strTargetRecPath = target_dbnames[0] + "/?";
                    strError = "将在 " + target_dbnames[0] + " 中创建一个新的目标记录";
                    return 1;
                }

                Debug.Assert(target_dbnames.Count > 1, "");

                GetAcceptTargetDbNameDlg dlg = new GetAcceptTargetDbNameDlg();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.SeriesMode = bSeriesMode;
                dlg.Text = "请选定一个目标书目库，验收时将在其中创建一条新的书目记录";
                // dlg.MainForm = Program.MainForm;
                dlg.MarcSyntax = source_dbinfo.Syntax;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.Cancel)
                {
                    strError = "用户放弃选择目标库";
                    return -1;
                }

                strTargetRecPath = dlg.DbName + "/?";

                strError = "将在 " + dlg.DbName + " 中创建一个新的目标记录";
                return 1;
            }

            // 如果当前仅有一个潜在目标事项
            if (nTargetItemCount + nSourceAndTargetItemCount == 1)
            {
                for (int i = 0; i < this.listView_accept_records.Items.Count; i++)
                {
                    ListViewItem item = this.listView_accept_records.Items[i];

                    if (item.ImageIndex == TYPE_SOURCE_AND_TARGET
                        || item.ImageIndex == TYPE_TARGET)
                    {
                        strTargetRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
                        nRet = SetTarget(item, "set", out strError);
                        if (nRet == -1)
                            return -1;
                        strError = "唯一的潜在目标记录 '" + strTargetRecPath + "' 被自动作为目标记录了";
                        return 1;
                    }
                }

                strError = "TYPE_TARGET或TYPE_SOURCE_AND_TARGET的事项居然没有找到...";
                return -1;
            }

            if (nTargetItemCount + nSourceAndTargetItemCount > 1)
            {
                strError = "请先在命中记录列表中设定好目标记录，然后再进行验收";
                return 2;
            }

            strError = "请先在命中记录列表中设定好目标记录，然后再进行验收 --";
            return 2;
        }

        // TODO: 编写一个函数，在没有明确设定目标记录，但是当前条件可以推导出目标记录的时候，
        // 给出目标记录路径。
        // 条件：1) 当前只有一个潜在目标记录 2) 当前没有潜在目标记录，但是可以充当目标库的库只有一个。如果源和目标库重合，优先用源记录作为目标记录；否则用“目标库/?”作为目标记录路径
        //  3) 当前没有潜在目标记录，并且可以充当目标库的有多个。这时候需要出现对话框，让操作者选择一个目标库。对话框需要保持先前选过的状态，以便操作者提高操作速度
        //  4) 当前没有潜在目标记录，并且没有任何库可以充当目标库。报错，放弃操作。
        // 应当可以允许从其他窗口拖入一个记录路径到当前列表中。这样，就为设定源或者目标库提供了更多的条件。可以避免单纯通过ISBN检索的局限性。
        void m_detailWindow_PrepareAccept(object sender,
            PrepareAcceptEventArgs e)
        {
            // MessageBox.Show(this, "Prepare accept");
            string strError = "";
            string strWarning = "";
            int nRet = 0;

            // 验收批次号
            e.AcceptBatchNo = this.tabComboBox_prepare_batchNo.Text;

            // 是否需要在验收末段出现输入册条码号的界面
            e.InputItemsBarcode = this.checkBox_prepare_inputItemBarcode.Checked;

            // 2010/12/5
            e.PriceDefault = this.comboBox_prepare_priceDefault.Text;

            /*
            // 为新创建的册记录设置“加工中”状态
            e.SetProcessingState = this.checkBox_prepare_setProcessingState.Checked;
            */

            // 2012/5/7
            e.CreateCallNumber = this.checkBox_prepare_createCallNumber.Checked;

            e.SellerFilter = this.comboBox_sellerFilter.Text;

            Debug.Assert(String.IsNullOrEmpty(e.SourceRecPath) == false, "");
            // e.SourceRecPath 中是种册窗内当前记录，有强烈的倾向把它作为源，但是和当前AcceprtForm的浏览列表中可能已经设定的源不是同一个

            string strTargetRecPath = "";
            string strBiblioSourceRecPath = ""; // 当前列表中操作者设定的外源书目记录路径

            // 自动准备目标记录
            // return:
            //      -1  error
            //      0   函数调用前已经选定了目标事项，不必自动准备目标
            //      1   准备好了目标事项
            //      2   无法准备目标事项，条件不具备
            nRet = AutoPrepareAccept(
                e.SourceRecPath,
                out strTargetRecPath,
                out strBiblioSourceRecPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 2)
            {
                goto ERROR1;
            }

            // 2009/2/16
            // 看看AcceptForm中已经设定的源是否和e.SourceRecPath中的一致
            string strExistSourceRecPath = GetSourceRecPath();
            if (String.IsNullOrEmpty(strExistSourceRecPath) == false)
            {
                if (strExistSourceRecPath != e.SourceRecPath)
                {
                    /*
                    ListViewItem old_source_item = ListViewUtil.FindItem(this.listView_accept_records,
                        strExistSourceRecPath,
                        COLUMN_RECPATH);
                    if (old_source_item == null)
                    {
                        strError = "列表中居然没有找到路径为 '" + strExistSourceRecPath + "' 的事项";
                        goto ERROR1;
                    }
                    // 看看最初的源事项是否已经为斜体
                    RecordInfo old_source_info = GetRecordInfo(old_source_item);
                    if (old_source_info.TitleMatch == false)
                    {
                        // 警告title不一致现象
                        strWarning = "源记录 " + e.SourceRecPath + " 的题名和目标记录 " + strTargetRecPath + " 的题名不吻合";
                    }
                    */

                    strWarning = "当前种册窗内的记录 " + e.SourceRecPath + " 并不是验收窗内已设定为源角色的记录 " + strExistSourceRecPath + "。\r\n\r\n确实要改设前者为源角色并继续进行验收?";
                    // TODO: 警告最好在这里进行，因为要决定后面是否实施SetSource()操作
                    DialogResult result = MessageBox.Show(this,
                        strWarning,
                        "AcceptForm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            // 把源设置为 e.SourceRecPath
            ListViewItem source_item = ListViewUtil.FindItem(this.listView_accept_records,
                e.SourceRecPath,
                COLUMN_RECPATH);
            if (source_item == null)
            {
                strError = "列表中居然没有找到路径为 '" + e.SourceRecPath + "' 的事项";
                goto ERROR1;
            }

            // 2009/10/23
            // 检查当前行是否合适设置为源角色
            nRet = WarningSetSource(source_item);
            if (nRet == 0)
            {
                e.Cancel = true;
                return;
            }

            // 2009/2/16 移动到这里
            nRet = SetSource(source_item,
                "set",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 设置源之后，看看源事项是否已经为斜体
            RecordInfo source_info = GetRecordInfo(source_item);
            if (source_info.TitleMatch == false)
            {
                // 警告title不一致现象
                strWarning = "源记录 '" + e.SourceRecPath + "' 的题名和目标记录 '" + strTargetRecPath + "' 的题名不吻合";
                // TODO: 警告最好在这里进行，因为要决定后面是否实施SetSource()操作
                DialogResult result = MessageBox.Show(this,
                    strWarning + "\r\n\r\n继续验收? ",
                    "AcceptForm",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            ListViewItem target_item = ListViewUtil.FindItem(this.listView_accept_records,
                strTargetRecPath,
                COLUMN_RECPATH);
            if (target_item != null)    // 2008/12/3
            {
                // 看看目标事项是否已经为斜体
                RecordInfo target_info = GetRecordInfo(target_item);
                if (target_info.TitleMatch == false)
                {
                    strWarning = "目标记录 '" + strTargetRecPath + "' 的题名和源记录 '" + e.SourceRecPath + "' 的题名不吻合";
                    DialogResult result = MessageBox.Show(this,
                        strWarning + "\r\n\r\n继续验收? ",
                        "AcceptForm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            e.TargetRecPath = strTargetRecPath;

            // 2009/11/5
            if (String.IsNullOrEmpty(strBiblioSourceRecPath) == false)
            {
                ListViewItem biblioSource_item = ListViewUtil.FindItem(this.listView_accept_records,
                    strBiblioSourceRecPath,
                    COLUMN_RECPATH);
                if (biblioSource_item != null)
                {
                    // 看看外源事项是否已经为斜体
                    RecordInfo biblioSource_info = GetRecordInfo(biblioSource_item);
                    if (biblioSource_info.TitleMatch == false)
                    {
                        strWarning = "外源记录 '" + strBiblioSourceRecPath + "' 的题名和源记录 '" + e.SourceRecPath + "' 的题名不吻合";
                        DialogResult result = MessageBox.Show(this,
                            strWarning + "\r\n\r\n继续验收? ",
                            "AcceptForm",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.Cancel)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }
            }

            e.BiblioSourceRecPath = strBiblioSourceRecPath;

            if (String.IsNullOrEmpty(e.BiblioSourceRecPath) == false)
            {
                string strXml = "";
                // 获得一个书目记录
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetBiblioXml(e.BiblioSourceRecPath,
                    out strXml,
                    out strError);
                if (nRet == 0 || nRet == -1)
                {
                    strError = "验收操作被拒绝。获取外源记录 '" + e.BiblioSourceRecPath + "' 时出错: " + strError;
                    goto ERROR1;
                }
                e.BiblioSourceRecord = strXml;
                e.BiblioSourceSyntax = "xml";
            }

            bool bSeriesMode = false;
            if (this.comboBox_prepare_type.Text == "连续出版物")
                bSeriesMode = true;

            // 期刊情况下，源角色和目标角色必须为同一条，这是一个额外的要求。2009/2/17
            if (bSeriesMode == true)
            {
                if (e.TargetRecPath != e.SourceRecPath)
                {
                    strError = "验收操作被拒绝。出版物类型为期刊时，源记录和目标记录必须为同一条。(可是现在源记录为 " + e.SourceRecPath + "，目标记录为 " + e.TargetRecPath + ")";
                    goto ERROR1;
                }
            }

            string str998TargetRecPath = "";
            // 检查源记录的998$t，看看当前列表中是否已经有这条记录，如果没有，则需要装入
            // return:
            //      -1  出错(但是事项可能已经加入)
            //      0   源记录没有998$t
            //      1   本函数调用前目标记录已经在列表中存在
            //      2   新装入了目标记录
            nRet = AutoLoadTarget(e.SourceRecPath,
                out str998TargetRecPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strSourceDbName = Global.GetDbName(e.SourceRecPath);
            OrderDbInfo source_dbinfo = this.db_infos.LocateByBiblioDbName(strSourceDbName);
            if (source_dbinfo == null)
            {
                strError = "在this.db_infos中竟然没有找到名字为 " + strSourceDbName + " 的书目库对象";
                goto ERROR1;
            }

            // 检查 采购工作库 情况
            if (bSeriesMode == false)
            {
                // 源记录来自采购工作库，目标记录和源记录不是同一条
                if (source_dbinfo.IsOrderWork == true
                    && e.SourceRecPath != e.TargetRecPath)
                {
                    // 并且，目标记录不是源记录998$t指向的那条
                    if (String.IsNullOrEmpty(str998TargetRecPath) == false
                        && e.TargetRecPath != str998TargetRecPath)
                    {
                        strWarning = "书目库 '" + strSourceDbName + "' 的角色为采购工作库，一般情况下此库中的源记录(" + e.SourceRecPath + ")也应同时作为目标记录。\r\n\r\n是否真的要将(您设定的)记录 '" + e.TargetRecPath + "' 作为目标，直接在其中创建册信息?\r\n\r\n------\r\n是(Yes): 将记录 '" + e.TargetRecPath + "' 作为目标，并在验收操作中将源记录中的998$t(目前内容为 '" + str998TargetRecPath + "')改设为指向您选定的这个目标(" + e.TargetRecPath + ")；\r\n否(No): 放弃验收";
                        DialogResult result = MessageBox.Show(this,
                            strWarning,
                            "AcceptForm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }

                    // 目标记录正好是源记录998$t指向的那条
                    else if (String.IsNullOrEmpty(str998TargetRecPath) == false
                        && e.TargetRecPath == str998TargetRecPath)
                    {
                        strWarning = "书目库 '" + strSourceDbName + "' 的角色为采购工作库，一般情况下此库中的源记录(" + e.SourceRecPath + ")也应同时作为目标记录。待到后期的转移操作，自然会将这条工作库记录转移到最终的目标记录 '" + str998TargetRecPath + "'，而不必现在操心。\r\n\r\n是否真的要将(您设定的)记录 '" + e.TargetRecPath + "' 作为目标，直接在其中创建册信息?\r\n\r\n------\r\n是(Yes): 将记录 '" + e.TargetRecPath + "' 作为目标；\r\n否(No): 放弃验收";
                        DialogResult result = MessageBox.Show(this,
                            strWarning,
                            "AcceptForm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }

                    }
                    else
                    {
                        // 源记录没有998$t。此时如果急于给它寻找一个最终的目标记录，可能是因为在订购以后，中央库新增了某些书目记录，正好是目前订购的这一种

                        // TODO: 
                        // 需要检查e.SourceRecPath记录中的998$t，如果本来就指向e.TargetRecPath，那么MessageBox的提示就简略；
                        // 而如果指向不是e.TargetRecPath的记录，则在(yes)选择情况下，要补充提示，如下：
                        // ，并(即将在验收过程中)重设源记录("+e.SourceRecPath+")中的目标信息(998$t)

                        strWarning = "书目库 '" + strSourceDbName + "' 的角色为采购工作库，一般情况下此库中的源记录(" + e.SourceRecPath + ")也应同时作为目标记录。\r\n\r\n是否真的要将(您设定的)记录 '" + e.TargetRecPath + "' 作为目标，直接在其中创建册信息?\r\n\r\n------\r\n是(Yes): 将记录 '" + e.TargetRecPath + "' 作为目标\r\n否(No): 改为将记录 '" + e.SourceRecPath + "' 作为目标\r\n取消(Cancel): 放弃验收";
                        DialogResult result = MessageBox.Show(this,
                            strWarning,
                            "AcceptForm",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.Cancel)
                        {
                            e.Cancel = true;
                            return;
                        }

                        // 恢复source作为target
                        if (result == DialogResult.No)
                        {
                            e.TargetRecPath = e.SourceRecPath;
                            nRet = SetTarget(source_item,
                                "set",
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            target_item = source_item;
                        }
                    }
                }
            }

            // 检查 非采购工作库 情况
            // 源记录来自 非采购工作库，目标记录和源记录不是同一条
            if (source_dbinfo.IsOrderWork == false
                && e.SourceRecPath != e.TargetRecPath)
            {
                // 并且，目标记录不是源记录998$t指向的那条
                if (String.IsNullOrEmpty(str998TargetRecPath) == false
                    && e.TargetRecPath != str998TargetRecPath)
                {
                    strWarning = "源记录 '" + e.SourceRecPath + "' 中998$t指向的目标记录为 '" + str998TargetRecPath + "'，而您设定了一个不同的目标记录 '" + e.TargetRecPath + "'。\r\n\r\n是否真的要将(您设定的)记录 '" + e.TargetRecPath + "' 作为目标，直接在其中创建册信息?\r\n\r\n------\r\n是(Yes): 将记录 '" + e.TargetRecPath + "' 作为目标，并在验收操作中将源记录中的998$t(目前内容为 '" + str998TargetRecPath + "')改设为指向您选定的这个目标(" + e.TargetRecPath + ")；\r\n否(No): 放弃验收";
                    DialogResult result = MessageBox.Show(this,
                        strWarning,
                        "AcceptForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            return;
        ERROR1:
            e.ErrorInfo = strError;
            e.Cancel = true;
        }

        /// <summary>
        /// 当前窗口所关联的种册窗
        /// </summary>
        public EntityForm DetailWindow
        {
            get
            {
                if (m_detailWindow != null)
                {
                    if (m_detailWindow.IsDisposed == true)
                    {
                        m_detailWindow = null;
                        return null;
                    }
                    return m_detailWindow;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// 响应 MdiClient 尺寸变化
        /// </summary>
        public void OnMdiClientSizeChanged()
        {
            AcceptForm_SizeChanged(this, null);
        }

        private void AcceptForm_SizeChanged(object sender, EventArgs e)
        {
#if ACCEPT_MODE

            bool bRet = InitialSizeParam();

            if (bRet == true)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                }

                this.m_nAcceptWindowHeight = this.Size.Height;


                this.Location = new Point(0, 0);
                this.Size = new Size(m_nMdiClientWidth, this.Size.Height);

                if (m_detailWindow != null)
                {
                    if (m_detailWindow.IsDisposed == true)
                    {
                        m_detailWindow = null;
                    }
                }

                if (m_detailWindow != null)
                {
                    m_detailWindow.Location = new Point(0, this.Size.Height);
                    m_detailWindow.Size = new Size(m_nMdiClientWidth, m_nMdiClientHeight - this.Size.Height);
                }
            }
#endif
        }

        /*
        public void EnableProgress()
        {
            Program.MainForm.stopManager.Active(this.stop);
        }
        */

        private void AcceptForm_Activated(object sender, EventArgs e)
        {
#if NO
            // 2009/8/13
            Program.MainForm.stopManager.Active(this.stop);
            EnableProgress();
#endif

            if (m_detailWindow != null)
            {
                if (m_detailWindow.IsDisposed == true)
                {
                    m_detailWindow = null;
                }
            }

            if (m_detailWindow != null)
            {
                if (Program.MainForm.IsTopTwoChildWindow(m_detailWindow) == false)
                    m_detailWindow.Activate();
            }
        }

        private void listView_accept_records_MouseDown(object sender, MouseEventArgs e)
        {

        }

        // popup menu
        private void listView_accept_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            string strRole = "";
            string strRecPath = "";
            if (this.listView_accept_records.SelectedItems.Count > 0)
            {
                strRole = ListViewUtil.GetItemText(this.listView_accept_records.SelectedItems[0],
                    COLUMN_ROLE);
                strRecPath = ListViewUtil.GetItemText(this.listView_accept_records.SelectedItems[0],
                    COLUMN_RECPATH);
            }

            // 根据记录路径，获得ListViewItem事项的imageindex下标
            // return:
            //      -2  根本不是书目库
            //      -1  不是采购源或目标的其它书目库
            //      0   源
            //      1   目标
            //      2   同时为源和目标
            //      3   外源
            int image_index = this.db_infos.GetItemType(strRecPath,
                            this.comboBox_prepare_type.Text);

            string strText = "";
            bool bEnable = true;

            if (StringUtil.IsInList("源", strRole) == true)
                strText = "去除角色“源”(&S)";
            else
            {
                if (image_index != 0 && image_index != 2)
                    bEnable = false;
                strText = "设置角色“源”(&S)";
            }

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.menu_toggleSource_Click);
            if (this.listView_accept_records.SelectedItems.Count == 0
                || bEnable == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            bEnable = true;
            if (StringUtil.IsInList("目标", strRole) == true)
                strText = "去除角色“目标”(&T)";
            else
            {
                if (image_index != 1 && image_index != 2)
                    bEnable = false;
                strText = "设置角色“目标”(&T)";
            }

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.menu_toggleTarget_Click);
            if (this.listView_accept_records.SelectedItems.Count == 0
                || bEnable == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            bEnable = true;
            if (StringUtil.IsInList("外源", strRole) == true)
                strText = "去除角色“外源”(&T)";
            else
            {
                if (image_index != 3)
                    bEnable = false;
                strText = "设置角色“外源”(&T)";
            }

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.menu_toggleBiblioSource_Click);
            if (this.listView_accept_records.SelectedItems.Count == 0
                || bEnable == false
                || this.comboBox_prepare_type.Text == "连续出版物")
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新所选择的 " + this.listView_accept_records.SelectedItems.Count.ToString() + " 个事项(&R)");
            menuItem.Click += new System.EventHandler(this.menu_refreshSelectedItems_Click);
            if (this.listView_accept_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            /*
            // cut 剪切
            menuItem = new MenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // copy 复制
            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(ClipboardBookItemCollection)) == false)
                bHasClipboardObject = false;
            else
                bHasClipboardObject = true;

            // paste 粘贴
            menuItem = new MenuItem("粘贴(&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteEntity_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);
             * 
             * */

            menuItem = new MenuItem("移除所选择的 " + this.listView_accept_records.SelectedItems.Count.ToString() + " 个事项(&R)");
            menuItem.Click += new System.EventHandler(this.menu_removeSelectedItems_Click);
            if (this.listView_accept_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("关闭种册窗(&C)");
            menuItem.Click += new System.EventHandler(this.menu_closeDetailWindow_Click);
            if (this.m_detailWindow == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_accept_records, new Point(e.X, e.Y));
        }

        // 刷新所选择的事项
        void menu_refreshSelectedItems_Click(object sender, EventArgs e)
        {
            /*
            this.EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新...");
            stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在刷新...",
                "disableControl");
            try
            {
                foreach (ListViewItem item in this.listView_accept_records.SelectedItems)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (looping.Stopped)
                        return;

                    // ListViewItem item = this.listView_accept_records.SelectedItems[i];

                    string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

                    string strError = "";
                    int nRet = RefreshBrowseLine(
                        looping.Progress,
                        channel,
                        item,
                        out strError);
                    if (nRet == -1)
                        ListViewUtil.ChangeItemText(item, 2, strError);
                }
            }
            finally
            {
                looping.Dispose();
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                this.EnableControls(true);
                */
            }
        }

        // 调用前，记录路径列已经有值
        /*public*/
        int RefreshBrowseLine(
            Stop stop,
            LibraryChannel channel,
            ListViewItem item,
            out string strError)
        {
            strError = "";

            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
            string[] paths = new string[1];
            paths[0] = strRecPath;
            Record[] searchresults = null;

            long lRet = channel.GetBrowseRecords(
                stop,
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
                    i + RESERVE_COLUMN_COUNT,
                    searchresults[0].Cols[i]);
            }

            int nRet = FilterOneItem(
                stop,
                channel,
                item,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        void menu_closeDetailWindow_Click(object sender, EventArgs e)
        {
            // 关闭 关联的EntityForm
            CloseDetailWindow();
        }

        void menu_removeSelectedItems_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                for (int i = this.listView_accept_records.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    ListViewItem item = this.listView_accept_records.SelectedItems[i];
                    string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

                    // 看看是否为外源角色
                    if (this.label_biblioSource.Tag != null)
                    {
                        if (strRecPath == (string)this.label_biblioSource.Tag)
                        {
                            this.SetLabelBiblioSource(null);
                            Debug.Assert(this.label_biblioSource.Tag == null, "");
                        }

                    }
                    // 看看是否为源角色
                    if (this.label_source.Tag != null)
                    {
                        if (strRecPath == (string)this.label_source.Tag)
                        {
                            this.SetLabelSource(null);
                            Debug.Assert(this.label_source.Tag == null, "");
                        }
                    }
                    // 看看是否为目标角色
                    if (this.label_target.Tag != null)
                    {
                        if (strRecPath == (string)this.label_target.Tag)
                        {
                            this.SetLabelTarget(null);
                            Debug.Assert(this.label_target.Tag == null, "");
                        }
                    }

                    // 看看是否已经装入下方的种册窗
                    if (this.m_detailWindow != null
                        && m_detailWindow.IsDisposed == false
                        && m_detailWindow.BiblioRecPath == strRecPath)
                    {
#if NO
                        if (this.SingleClickLoadDetail == true)
                            this.m_detailWindow.ReadOnly = true;
                        else
#endif
                        CloseDetailWindow();
                    }

                    this.listView_accept_records.Items.RemoveAt(this.listView_accept_records.SelectedIndices[i]);
                }

            }
            finally
            {
                this.Cursor = oldCursor;
            }
        }

        // TODO: 设置的时候需要检查一下，看看角色是否符合数据库的定义。
        // 例如，没有包含订购库的书目库，不能作为源；没有包含实体库的书目库，不能作为目标
        void menu_toggleSource_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_accept_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要切换角色“源”的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_accept_records.SelectedItems[0];

            int nRet = WarningSetSource(item);
            if (nRet == 0)
                return;
            /*
            RecordInfo info = GetRecordInfo(item);
            string strRole = ListViewUtil.GetItemText(item,
                COLUMN_ROLE);
            // 即将设置“源”
            if (StringUtil.IsInList("源",strRole) == false)
            {
                // 看看是否有订购信息
                if (info.HasOrderInfo == false)
                {
                    DialogResult result = MessageBox.Show(this,
                        "当前记录中未包含相应的订购信息，一般情况下不适合作为“源”记录。\r\n\r\n实际要强行设置为“源”？",
                        "AcceptForm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                        return;
                }
            }
             * */

            nRet = SetSource(item,
                "toggle",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            /*
            string strRole = ListViewUtil.GetItemText(item,
                COLUMN_ROLE);

            if (StringUtil.IsInList("源", strRole) == true)
            {
                StringUtil.SetInList(ref strRole, "源", false);

                // 记忆在label中
                SetLabelSource(null);
            }
            else
            {
                // 添加前，看看所属数据库是否具备相应的特性
                string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
                // 根据记录路径，获得ListViewItem事项的imageindex下标
                // return:
                //      -2  根本不是书目库
                //      -1  不是采购源或目标的其它书目库
                //      0   源
                //      1   目标
                //      2   同时为源和目标
        //      3   外源
                int nRet = this.db_infos.GetItemType(strRecPath,
                            this.comboBox_prepare_type.Text);
                if (nRet == -2 || nRet == -1)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库根本和采购业务无关，不能添加 源 角色";
                    goto ERROR1;
                }
                if (nRet == 1)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库为“目标”类型，不能添加 源 角色";
                    goto ERROR1;
                }


                // 添加
                StringUtil.SetInList(ref strRole, "源", true);

                // 清除其它事项里面可能有的角色“源”
                ClearRole("源", item);

                // 记忆在label中
                SetLabelSource(ListViewUtil.GetItemText(item, COLUMN_RECPATH));
            }

            ListViewUtil.ChangeItemText(item,
                COLUMN_ROLE,
                strRole);
            SetItemColor(item);
             * */

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_toggleBiblioSource_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.comboBox_prepare_type.Text == "连续出版物")
            {
                strError = "对于连续出版物，不支持 外源角色";
                goto ERROR1;
            }

            if (this.listView_accept_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要切换角色“外源”的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_accept_records.SelectedItems[0];

            int nRet = SetBiblioSource(item,
                "toggle",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        void menu_toggleTarget_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_accept_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要切换角色“目标”的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_accept_records.SelectedItems[0];

            int nRet = SetTarget(item,
                "toggle",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            /*

            string strRole = ListViewUtil.GetItemText(item,
                COLUMN_ROLE);

            if (StringUtil.IsInList("目标", strRole) == true)
            {
                // 去掉
                StringUtil.SetInList(ref strRole, "目标", false);

                // 记忆在label中
                SetLabelTarget(null);
            }
            else
            {
                // 添加前，看看所属数据库是否具备相应的特性
                string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
                // 根据记录路径，获得ListViewItem事项的imageindex下标
                // return:
                //      -2  根本不是书目库
                //      -1  不是采购源或目标的其它书目库
                //      0   源
                //      1   目标
                //      2   同时为源和目标
         //      3   外源
               int nRet = this.db_infos.GetItemType(strRecPath,
                            this.comboBox_prepare_type.Text);
                if (nRet == -2 || nRet == -1)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库根本和采购业务无关，不能添加 目标 角色";
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库为“源”类型，不能添加 目标 角色";
                    goto ERROR1;
                }
  

                // 添加
                StringUtil.SetInList(ref strRole, "目标", true);

                // 清除其它事项里面可能有的角色“目标”
                ClearRole("目标", item);

                // 记忆在label中
                SetLabelTarget( ListViewUtil.GetItemText(item, COLUMN_RECPATH));
            }

            ListViewUtil.ChangeItemText(item,
                COLUMN_ROLE,
                strRole);
            SetItemColor(item);
             * */

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        static void SetItemColor(ListViewItem item)
        {
            string strRole = ListViewUtil.GetItemText(item, 1);

            RecordInfo info = GetRecordInfo(item);



            if (StringUtil.IsInList("源", strRole) == true)
            {
                // 包括 源,目标 都在role中的情况
                item.BackColor = Color.LightBlue;
                if (info.TitleMatch == true)
                    item.Font = new Font(item.Font, FontStyle.Bold);
                else
                    item.Font = new Font(item.Font, FontStyle.Bold | FontStyle.Italic);
            }
            else if (StringUtil.IsInList("目标", strRole) == true)
            {
                item.BackColor = Color.LightPink;
                if (info.TitleMatch == true)
                    item.Font = new Font(item.Font, FontStyle.Bold);
                else
                    item.Font = new Font(item.Font, FontStyle.Bold | FontStyle.Italic);
            }
            else if (StringUtil.IsInList("外源", strRole) == true)
            {
                item.BackColor = Color.LightGreen;
                if (info.TitleMatch == true)
                    item.Font = new Font(item.Font, FontStyle.Bold);
                else
                    item.Font = new Font(item.Font, FontStyle.Bold | FontStyle.Italic);
            }
            else
            {
                item.BackColor = SystemColors.Window;

                if (info.TitleMatch == true)
                    item.Font = new Font(item.Font, FontStyle.Regular);
                else
                    item.Font = new Font(item.Font, FontStyle.Regular | FontStyle.Italic);
            }

            // imageindex value:
            //      -2  根本不是书目库
            //      -1  不是采购源或目标的其它书目库
            //      0   源
            //      1   目标
            //      2   同时为源和目标
            //      3   外源
            if (item.ImageIndex == TYPE_NOT_ORDER
                || item.ImageIndex < 0
                || info.TitleMatch == false
                || (info.HasOrderInfo == false && item.ImageIndex == 0))  // 单纯的源，如果不包含订购信息，则发灰 2009/10/23
                item.ForeColor = SystemColors.GrayText;
            else
                item.ForeColor = SystemColors.WindowText;
        }

        // 清除残余的源、目标信息
        void ClearSourceTarget()
        {
            this.label_source.Tag = null;
            this.label_target.Tag = null;
        }

        // 飞出tips source
        private void label_source_MouseHover(object sender, EventArgs e)
        {
            object o = this.label_source.Tag;
            string strText = "";
            if (o == null)
                strText = "源尚未设置";
            else
                strText = "\r\n源为 " + (string)o + "\r\n";

            this.toolTip_info.Show(strText, this.label_source, 1000);
        }

        // 飞出 tips target
        private void label_target_MouseHover(object sender, EventArgs e)
        {
            object o = this.label_target.Tag;
            string strText = "";
            if (o == null)
                strText = "目标尚未设置";
            else
                strText = "\r\n目标为 " + (string)o + "\r\n";

            this.toolTip_info.Show(strText, this.label_target, 1000);
        }

        // 点一下鼠标 source
        private void label_source_MouseClick(object sender, MouseEventArgs e)
        {
            OnClickLabel(this.label_source, false);
        }

        // 点一下鼠标 target
        private void label_target_MouseClick(object sender, MouseEventArgs e)
        {
            OnClickLabel(this.label_target, false);
        }


        // 双击鼠标 source
        private void label_source_DoubleClick(object sender, EventArgs e)
        {
            OnClickLabel(this.label_source, true);

        }

        // 双击鼠标 target
        private void label_target_DoubleClick(object sender, EventArgs e)
        {
            OnClickLabel(this.label_target, true);
        }

        // 双击鼠标 bibloSource
        private void label_biblioSource_DoubleClick(object sender, EventArgs e)
        {
            OnClickLabel(this.label_biblioSource, true);
        }
        private void label_biblioSource_MouseClick(object sender, MouseEventArgs e)
        {
            OnClickLabel(this.label_biblioSource, false);

        }

        // 飞出 tips biblioSource
        private void label_biblioSource_MouseHover(object sender, EventArgs e)
        {
            object o = this.label_biblioSource.Tag;
            string strText = "";
            if (o == null)
                strText = "外源尚未设置";
            else
                strText = "\r\n外源为 " + (string)o + "\r\n";

            this.toolTip_info.Show(strText, this.label_target, 1000);
        }


        private void button_viewDatabaseDefs_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strOutputInfo = "";

            int nRet = GetAllDatabaseInfo(
                out strOutputInfo,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            XmlViewerForm xml_viewer = new XmlViewerForm();

            // xml_viewer.MainForm = Program.MainForm;
            xml_viewer.XmlString = strOutputInfo;
            xml_viewer.ShowDialog(this);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int GetAllDatabaseInfo(out string strOutputInfo,
    out string strError)
        {
            strError = "";

            /*
            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取全部数据库名 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获取全部数据库名 ...",
                "disableControl");
            try
            {
                long lRet = channel.ManageDatabase(
                    looping.Progress,
                    "getinfo",
                    "",
                    "",
                    "",
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    return -1;
                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
                */
            }
        }

        /// <summary>
        /// 激活第一个属性页
        /// </summary>
        public void ActivateFirstPage()
        {
            this.tabControl_main.SelectedTab = this.tabPage_prepare;
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_prepare)
            {
                if (String.IsNullOrEmpty(this.tabComboBox_prepare_batchNo.Text) == true)
                {
                    strError = "尚未指定验收批次号";
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(this.comboBox_prepare_type.Text) == true)
                {
                    strError = "尚未指定出版物类型";
                    goto ERROR1;
                }

                if (this.comboBox_prepare_type.Text != "图书"
                    && this.comboBox_prepare_type.Text != "连续出版物")
                {
                    strError = "未知的出版物类型 '" + this.comboBox_prepare_type.Text + "'";
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(GetDbNameListString()) == true)
                {
                    strError = "尚未选定参与检索的书目库";
                    goto ERROR1;
                }

                // 切换到下一个page
                this.tabControl_main.SelectedTab = this.tabPage_accept;
                return;
            }
            if (this.tabControl_main.SelectedTab == this.tabPage_accept)
            {
                // 迫使detailWindow保存
                SaveDetailWindowChanges();




                // 切换到下一个page
                this.tabControl_main.SelectedTab = this.tabPage_finish;
                return;
            }
            if (this.tabControl_main.SelectedTab == this.tabPage_finish)
            {
#if !ACCEPT_MODE
                Program.MainForm.CurrentAcceptControl = null;
#endif
                this.Close();
                return;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#region source and target 相关


        // 自动设置各行的角色
        // TODO: 简化概念
        void AutoSetLinesRole()
        {
            int nSourceItemCount = 0;   // 源角色事项数目。不包括双角色事项数目
            int nTargetItemCount = 0;   // 目标角色事项数目。不包括双角色事项数目
            int nSourceAndTargetItemCount = 0;    // 同时具备两个角色的事项数目
            for (int i = 0; i < this.listView_accept_records.Items.Count; i++)
            {
                ListViewItem item = this.listView_accept_records.Items[i];

                if (item.ImageIndex == TYPE_SOURCE)
                    nSourceItemCount++;
                if (item.ImageIndex == TYPE_TARGET)
                    nTargetItemCount++;
                if (item.ImageIndex == TYPE_SOURCE_AND_TARGET)
                    nSourceAndTargetItemCount++;
            }

            // 如果当前仅有一个同时为源和目标的事项
            if (nSourceItemCount == 0 && nTargetItemCount == 0
                && nSourceAndTargetItemCount == 1)
            {
                string strRecPath = "";
                for (int i = 0; i < this.listView_accept_records.Items.Count; i++)
                {
                    ListViewItem item = this.listView_accept_records.Items[i];

                    if (item.ImageIndex == TYPE_SOURCE_AND_TARGET)
                    {
                        strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
                        break;
                    }
                }

                SetLabelSource(strRecPath);
                SetLabelTarget(strRecPath);
                return;
            }

            // 如果当前仅有一个源事项，没有目标事项。也没有双重事项
            if (nSourceItemCount == 1 && nTargetItemCount == 0
                && nSourceAndTargetItemCount == 0)
            {
                string strRecPath = "";
                for (int i = 0; i < this.listView_accept_records.Items.Count; i++)
                {
                    ListViewItem item = this.listView_accept_records.Items[i];

                    if (item.ImageIndex == TYPE_SOURCE)
                    {
                        strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
                        break;
                    }
                }

                SetLabelSource(strRecPath);
                SetLabelTarget("");
                // 表明需要创建目标记录
                return;
            }


            // 如果当前仅有若干个目标事项，没有源事项。也没有双重事项
            // 这种情况很奇怪，只能说明指定ISBN号的图书并没有被订购(但是以前曾经收藏过)
            // 给出一定的提示
            if (nSourceItemCount == 0 && nTargetItemCount >= 1
                && nSourceAndTargetItemCount == 0)
            {
            }


            // 有一个源事项，但是有多个目标事项。这时候只能自动设置源事项，而提示选择目标事项。
            // 如果不选择目标事项，则不允许进行实质性验收。
        }

        // 在列表中清除全部事项中特定的角色字符串
        // parameters:
        //      exclude 清除的时候，要跳过这个事项
        void ClearRole(string strText,
            ListViewItem exclude)
        {
            for (int i = 0; i < this.listView_accept_records.Items.Count; i++)
            {
                ListViewItem item = this.listView_accept_records.Items[i];

                if (item == exclude)
                    continue;

                string strRole = ListViewUtil.GetItemText(item, COLUMN_ROLE);

                if (StringUtil.IsInList(strText, strRole) == true)
                {
                    StringUtil.SetInList(ref strRole, strText, false);
                    ListViewUtil.ChangeItemText(item, COLUMN_ROLE, strRole);
                    SetItemColor(item);
                }
            }
        }

        void OnClickLabel(System.Windows.Forms.Label label,
            bool bDoubleClick)
        {
            string strError = "";
            object o = label.Tag;
            if (o == null)
            {
                // 发出警告性的响声
                Console.Beep();
                return;
            }

            string strRecPath = (string)o;

            ListViewItem item = ListViewUtil.FindItem(this.listView_accept_records, strRecPath, COLUMN_RECPATH);
            if (item == null)
            {
                strError = "列表中居然没有找到路径为 '" + strRecPath + "' 的事项";
                goto ERROR1;
            }

            if (bDoubleClick == false)
            {
                // 表示为当前焦点行(但是并不直接选中它，以免惊扰listview引起详细窗联动)
                this.listView_accept_records.FocusedItem = item;
            }
            else
            {
                ListViewUtil.SelectLine(
                    item,
                    true);
            }
            item.EnsureVisible();

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 观察一个listview事项是否能够被设置为biblioSource?
        bool IsBiblioSourceable(ListViewItem item)
        {
            if (item == null)
                return false;

            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

            // 根据记录路径，获得ListViewItem事项的imageindex下标
            // return:
            //      -2  根本不是书目库
            //      -1  不是采购源或目标的其它书目库
            //      0   源
            //      1   目标
            //      2   同时为源和目标
            //      3   外源
            int nRet = this.db_infos.GetItemType(strRecPath,
                        this.comboBox_prepare_type.Text);
            if (nRet == 3)
                return true;
            return false;
        }

        // 观察一个listview事项是否能够被设置为target?
        bool IsTargetable(ListViewItem item)
        {
            if (item == null)
                return false;

            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

            // 根据记录路径，获得ListViewItem事项的imageindex下标
            // return:
            //      -2  根本不是书目库
            //      -1  不是采购源或目标的其它书目库
            //      0   源
            //      1   目标
            //      2   同时为源和目标
            //      3   外源
            int nRet = this.db_infos.GetItemType(strRecPath,
                        this.comboBox_prepare_type.Text);
            if (nRet == 1 || nRet == 2)
                return true;
            return false;
        }

        // 观察一个listview事项是否能够被设置为source?
        bool IsSourceable(ListViewItem item)
        {
            if (item == null)
                return false;

            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

            // 根据记录路径，获得ListViewItem事项的imageindex下标
            // return:
            //      -2  根本不是书目库
            //      -1  不是采购源或目标的其它书目库
            //      0   源
            //      1   目标
            //      2   同时为源和目标
            //      3   外源
            int nRet = this.db_infos.GetItemType(strRecPath,
                        this.comboBox_prepare_type.Text);
            if (nRet == 0 || nRet == 2)
                return true;
            return false;
        }

        static RecordInfo GetRecordInfo(ListViewItem item)
        {
            RecordInfo info = (RecordInfo)item.Tag;
            if (info == null)
            {
                info = new RecordInfo();
                item.Tag = info;
            }

            return info;
        }

        // return:
        //      -1  error
        //      0   not found title
        //      1   found title
        int GetRecordTitle(ListViewItem item,
            out string strTitle,
            out string strError)
        {
            strError = "";
            strTitle = "";

            RecordInfo info = GetRecordInfo(item);

            if (String.IsNullOrEmpty(info.BiblioTitle) == false)
            {
                // 已经获得过了
                strTitle = info.BiblioTitle;
                return 1;
            }

            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
            // 获得一个书目记录的title
            // return:
            //      -1  error
            //      0   not found title
            //      1   found title
            int nRet = GetBiblioTitle(strRecPath,
                out strTitle,
                out strError);
            if (nRet == -1)
                return -1;

            return nRet;
        }

        // 根据source事项的title，把理论上的target事项过滤一番，突显title符合的，淡化title不符合的
        // 或者根据target事项的title，把理论上的source事项过滤一番，突显title符合的，淡化title不符合的
        // parameters:
        //      start_item  引起变化的ListViewItem事项。如果为source事项，则本函数会去修改若干target事项的显示状态；如果为target事项，则本函数会去修改若干source事项的显示状态
        //      strRoles   要去修改的角色。可用值source target biblioSource。和start_item的角色相反。可以使用逗号间隔的列表
        //      strAction   "set" 或者 "reset"。"set"表示需要根据start_item的title去筛选确定其他若干事项的显示状态，而"reset"表示全部恢复“加重”显示状态即可。
        int FilterTitle(
            ListViewItem start_item,
            string strRoles,
            string strAction,
            out string strError)
        {
            strError = "";

            Debug.Assert(strAction == "set" || strAction == "reset", "");

            string strSourceTitle = "";

            if (start_item != null)
            {
                Debug.Assert(strAction == "set", "当参数start_item不为空的时候，strAction参数应当为set");

                // return:
                //      -1  error
                //      0   not found title
                //      1   found title
                int nRet = GetRecordTitle(start_item,
                    out strSourceTitle,
                    out strError);
                if (nRet == -1)
                    return -1;

                RecordInfo source_info = GetRecordInfo(start_item);
                source_info.TitleMatch = true;

                SetItemColor(start_item);
            }
            else
            {
                Debug.Assert(strAction == "reset", "当参数start_item为空的时候，strAction参数应当为reset");
            }


            for (int i = 0; i < this.listView_accept_records.Items.Count; i++)
            {
                ListViewItem item = this.listView_accept_records.Items[i];

                if (item == start_item)
                    continue;

                string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

                // 
                // 根据记录路径，获得ListViewItem事项的imageindex下标
                // return:
                //      -2  根本不是书目库
                //      -1  不是采购源或目标的其它书目库
                //      0   源
                //      1   目标
                //      2   同时为源和目标
                //      3   外源
                int nItemType = this.db_infos.GetItemType(strRecPath,
                    this.comboBox_prepare_type.Text);

                bool bFound = false;
                if (StringUtil.IsInList("target", strRoles) == true)
                {
                    if (nItemType == 1 || nItemType == 2)
                        bFound = true;
                }
                if (StringUtil.IsInList("source", strRoles) == true)
                {
                    if (nItemType == 0 || nItemType == 2)
                        bFound = true;
                }
                if (StringUtil.IsInList("biblioSource", strRoles) == true)
                {
                    if (nItemType == 3)
                        bFound = true;
                }

                if (bFound == false)
                    continue;

                string strCurrentTitle = "";

                // return:
                //      -1  error
                //      0   not found title
                //      1   found title
                int nRet = GetRecordTitle(item,
                    out strCurrentTitle,
                    out strError);
                if (nRet == -1)
                    return -1;

                RecordInfo info = GetRecordInfo(item);

                if (strAction == "reset")
                {
                    if (info.TitleMatch != true)
                    {
                        info.TitleMatch = true;
                        SetItemColor(item);
                    }
                    continue;
                }

                if (strSourceTitle.ToLower() == strCurrentTitle.ToLower())
                {
                    // 显示为 重
                    info.TitleMatch = true;
                }
                else
                {
                    // 显示为 轻
                    info.TitleMatch = false;
                }

                SetItemColor(item);

            }

            return 0;
        }

        // 2008/10/22
        // 将具有指定路径的事项设置为目标记录
        int SetTarget(string strTargetRecPath,
            out string strError)
        {
            strError = "";

            ListViewItem item = ListViewUtil.FindItem(this.listView_accept_records,
                strTargetRecPath, COLUMN_RECPATH);
            if (item == null)
            {
                strError = "路径为 " + strTargetRecPath + " 的ListViewItem事项没有找到";
                return -1;
            }

            int nRet = SetTarget(item,
                "set",
                out strError);
            if (nRet == -1)
                return -1;

            return nRet;
        }

        // 给一个listview item事项设置、去除、切换 biblioSource 角色
        int SetBiblioSource(ListViewItem item,
            string strAction,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (item == null)
            {
                strError = "item == null";
                return -1;
            }

            if (this.comboBox_prepare_type.Text == "连续出版物")
            {
                strError = "对于连续出版物，不支持 外源角色";
                return -1;
            }

            Debug.Assert(item != null, "");
            Debug.Assert(strAction == "set" || strAction == "clear" || strAction == "toggle", "");

            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

            string strRole = ListViewUtil.GetItemText(item,
                COLUMN_ROLE);

            if (StringUtil.IsInList("外源", strRole) == true)
            {
                if (strAction == "set")
                {
                    // 看看是不是strRecPath
                    string strExistRecPath = GetTargetRecPath();
                    if (strExistRecPath == strRecPath)
                        return 0;

                    // 去掉
                    StringUtil.SetInList(ref strRole, "外源", false);

                    // 记忆在label中
                    SetLabelBiblioSource(null);

                    // 添加
                    StringUtil.SetInList(ref strRole, "外源", true);

                    // 记忆在label中
                    SetLabelBiblioSource(ListViewUtil.GetItemText(item, COLUMN_RECPATH));

                    nRet = FilterTitle(
                        item,
                        "source,target",
                        "set",
                        out strError);
                    if (nRet == -1)
                        return -1;


                    goto END1;

                }
                else if (strAction == "toggle" || strAction == "clear")
                {
                    // 去掉
                    StringUtil.SetInList(ref strRole, "外源", false);

                    // 记忆在label中
                    SetLabelBiblioSource(null);

                    nRet = FilterTitle(
                        null,
                        "source,target",
                        "reset",
                        out strError);
                    if (nRet == -1)
                        return -1;

                    goto END1;
                }

            }
            else
            {
                if (strAction == "clear")
                    return 0; // 本来就没有

                // 添加前，看看所属数据库是否具备相应的特性
                // string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

                // 根据记录路径，获得ListViewItem事项的imageindex下标
                // return:
                //      -2  根本不是书目库
                //      -1  不是采购源或目标的其它书目库
                //      0   源
                //      1   目标
                //      2   同时为源和目标
                //      3   外源
                nRet = this.db_infos.GetItemType(strRecPath,
                            this.comboBox_prepare_type.Text);
                if (nRet == -2 || nRet == -1)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库根本和" + this.comboBox_prepare_type.Text + "采购业务无关，不能添加 外源 角色";
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库为“源”类型，不能添加 外源 角色";
                    goto ERROR1;
                }
                if (nRet == 1 || nRet == 2)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库为“目标”类型，不能添加 外源 角色";
                    goto ERROR1;
                }

                // 添加
                StringUtil.SetInList(ref strRole, "外源", true);

                // 清除其它事项里面可能有的角色“外源”
                ClearRole("外源", item);

                // 记忆在label中
                SetLabelBiblioSource(ListViewUtil.GetItemText(item, COLUMN_RECPATH));

                nRet = FilterTitle(
                    item,
                    "source,target",
                    "set",
                    out strError);
                if (nRet == -1)
                    return -1;

            }

        END1:
            ListViewUtil.ChangeItemText(item,
                COLUMN_ROLE,
                strRole);
            SetItemColor(item);

            return 0;
        ERROR1:
            return -1;
        }

        // 给一个listview item事项设置、去除、切换 target 角色
        int SetTarget(ListViewItem item,
            string strAction,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (item == null)
            {
                strError = "item == null";
                return -1;
            }

            Debug.Assert(item != null, "");
            Debug.Assert(strAction == "set" || strAction == "clear" || strAction == "toggle", "");

            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

            string strRole = ListViewUtil.GetItemText(item,
                COLUMN_ROLE);

            if (StringUtil.IsInList("目标", strRole) == true)
            {
                if (strAction == "set")
                {
                    // 看看是不是strRecPath
                    string strExistRecPath = GetTargetRecPath();
                    if (strExistRecPath == strRecPath)
                        return 0;

                    // 去掉
                    StringUtil.SetInList(ref strRole, "目标", false);

                    // 记忆在label中
                    SetLabelTarget(null);

                    // 添加
                    StringUtil.SetInList(ref strRole, "目标", true);

                    // 记忆在label中
                    SetLabelTarget(ListViewUtil.GetItemText(item, COLUMN_RECPATH));

                    nRet = FilterTitle(
                        item,
                        "source,biblioSource", // "source",
                        "set",
                        out strError);
                    if (nRet == -1)
                        return -1;


                    goto END1;

                }
                else if (strAction == "toggle" || strAction == "clear")
                {
                    // 去掉
                    StringUtil.SetInList(ref strRole, "目标", false);

                    // 记忆在label中
                    SetLabelTarget(null);

                    nRet = FilterTitle(
                        null,
                        "source,biblioSource", // "source",
                        "reset",
                        out strError);
                    if (nRet == -1)
                        return -1;


                    goto END1;
                }

            }
            else
            {
                if (strAction == "clear")
                    return 0; // 本来就没有

                // 添加前，看看所属数据库是否具备相应的特性
                // string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

                // 根据记录路径，获得ListViewItem事项的imageindex下标
                // return:
                //      -2  根本不是书目库
                //      -1  不是采购源或目标的其它书目库
                //      0   源
                //      1   目标
                //      2   同时为源和目标
                //      3   外源
                nRet = this.db_infos.GetItemType(strRecPath,
                            this.comboBox_prepare_type.Text);
                if (nRet == -2 || nRet == -1)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库根本和" + this.comboBox_prepare_type.Text + "采购业务无关，不能添加 目标 角色";
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库为“源”类型，不能添加 目标 角色";
                    goto ERROR1;
                }
                if (nRet == 3)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库为“外源”类型，不能添加 目标 角色";
                    goto ERROR1;
                }

                // 添加
                StringUtil.SetInList(ref strRole, "目标", true);

                // 清除其它事项里面可能有的角色“目标”
                ClearRole("目标", item);

                // 记忆在label中
                SetLabelTarget(ListViewUtil.GetItemText(item, COLUMN_RECPATH));

                nRet = FilterTitle(
                    item,
                    "source,biblioSource",  // "source",
                    "set",
                    out strError);
                if (nRet == -1)
                    return -1;

            }

        END1:
            ListViewUtil.ChangeItemText(item,
                COLUMN_ROLE,
                strRole);
            SetItemColor(item);

            return 0;
        ERROR1:
            return -1;
        }

        // 给一个listview item事项设置(set)、去除(clear)、切换(toggle) source 角色
        // toggle的意思是在有和无之间切换
        int SetSource(ListViewItem item,
            string strAction,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (item == null)
            {
                strError = "item == null";
                return -1;
            }

            Debug.Assert(item != null, "");
            Debug.Assert(strAction == "set" || strAction == "clear" || strAction == "toggle", "");

            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

            string strRole = ListViewUtil.GetItemText(item,
                COLUMN_ROLE);

            if (StringUtil.IsInList("源", strRole) == true)
            {

                if (strAction == "set")
                {
                    // 看看是不是strRecPath
                    string strExistRecPath = GetSourceRecPath();
                    if (strExistRecPath == strRecPath)
                        return 0;

                    // 去掉
                    StringUtil.SetInList(ref strRole, "源", false);

                    // 记忆在label中
                    SetLabelSource(null);

                    // 添加
                    StringUtil.SetInList(ref strRole, "源", true);

                    // 记忆在label中
                    SetLabelSource(strRecPath);

                    nRet = FilterTitle(
                        item,
                        "target,biblioSource", // "target",
                        "set",
                        out strError);
                    if (nRet == -1)
                        return -1;


                    goto END1;

                }
                else if (strAction == "toggle" || strAction == "clear")
                {
                    // 去掉
                    StringUtil.SetInList(ref strRole, "源", false);

                    // 记忆在label中
                    SetLabelSource(null);

                    nRet = FilterTitle(
                        null,
                        "target,biblioSource", //"target",
                        "reset",
                        out strError);
                    if (nRet == -1)
                        return -1;


                    goto END1;
                }
            }
            else
            {
                if (strAction == "clear")
                    return 0; // 本来就没有

                // 添加前，看看所属数据库是否具备相应的特性
                // string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

                // 根据记录路径，获得ListViewItem事项的imageindex下标
                // return:
                //      -2  根本不是书目库
                //      -1  不是采购源或目标的其它书目库
                //      0   源
                //      1   目标
                //      2   同时为源和目标
                //      3   外源
                nRet = this.db_infos.GetItemType(strRecPath,
                            this.comboBox_prepare_type.Text);
                if (nRet == -2 || nRet == -1)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库根本和" + this.comboBox_prepare_type.Text + "采购业务无关，不能添加 源 角色";
                    goto ERROR1;
                }
                if (nRet == 1)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库为“目标”类型，不能添加 源 角色";
                    goto ERROR1;
                }
                if (nRet == 3)
                {
                    strError = "记录 '" + strRecPath + "' 所在的数据库为“外源”类型，不能添加 源 角色";
                    goto ERROR1;
                }

                // 添加
                StringUtil.SetInList(ref strRole, "源", true);

                // 清除其它事项里面可能有的角色“源”
                ClearRole("源", item);

                // 记忆在label中
                SetLabelSource(ListViewUtil.GetItemText(item, COLUMN_RECPATH));

                nRet = FilterTitle(
                    item,
                    "target,biblioSource", // "target",
                    "set",
                    out strError);
                if (nRet == -1)
                    return -1;

            }

        END1:
            ListViewUtil.ChangeItemText(item,
                COLUMN_ROLE,
                strRole);
            SetItemColor(item);

            return 0;
        ERROR1:
            return -1;
        }

        void SetLabelSource(string strRecPath)
        {
            if (String.IsNullOrEmpty(strRecPath) == true)
            {
                this.label_source.Text = "源(空)";
                this.label_source.Font = new Font(this.label_source.Font, FontStyle.Regular);
                this.label_source.Tag = null;
            }
            else
            {
                this.label_source.Text = "源";
                this.label_source.Font = new Font(this.label_source.Font, FontStyle.Bold);
                this.label_source.Tag = strRecPath;
            }
        }

        void SetLabelBiblioSource(string strRecPath)
        {
            if (String.IsNullOrEmpty(strRecPath) == true)
            {
                this.label_biblioSource.Text = "外源(空)";
                this.label_biblioSource.Font = new Font(this.label_target.Font, FontStyle.Regular);
                this.label_biblioSource.Tag = null;
            }
            else
            {
                this.label_biblioSource.Text = "外源";
                this.label_biblioSource.Font = new Font(this.label_target.Font, FontStyle.Bold);
                this.label_biblioSource.Tag = strRecPath;
            }
        }

        void SetLabelTarget(string strRecPath)
        {
            if (String.IsNullOrEmpty(strRecPath) == true)
            {
                this.label_target.Text = "目标(空)";
                this.label_target.Font = new Font(this.label_target.Font, FontStyle.Regular);
                this.label_target.Tag = null;
            }
            else
            {
                this.label_target.Text = "目标";
                this.label_target.Font = new Font(this.label_target.Font, FontStyle.Bold);
                this.label_target.Tag = strRecPath;
            }
        }

        string GetBiblioSourceRecPath()
        {
            object o = this.label_biblioSource.Tag;
            if (o == null)
                return "";
            return (string)o;
        }

        string GetTargetRecPath()
        {
            object o = this.label_target.Tag;
            if (o == null)
                return "";
            return (string)o;
        }

        string GetSourceRecPath()
        {
            object o = this.label_source.Tag;
            if (o == null)
                return "";
            return (string)o;
        }

        // 当设置 源 角色前，警告没有订购信息
        // return:
        //      0   放弃
        //      1   继续
        int WarningSetSource(ListViewItem item)
        {
            RecordInfo info = GetRecordInfo(item);
            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
            string strRole = ListViewUtil.GetItemText(item,
                COLUMN_ROLE);
            // 即将设置“源”
            if (StringUtil.IsInList("源", strRole) == false)
            {
                // 看看是否有订购信息
                if (info.HasOrderInfo == false)
                {
                    DialogResult result = MessageBox.Show(this,
                        "当前记录 '" + strRecPath + "' 中未包含相应的订购信息，一般情况下不适合作为“源”记录。\r\n\r\n实际要强行设置为“源”？",
                        "AcceptForm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                        return 0;
                }
            }

            return 1;
        }

#endregion

#region drag and drop 相关

        private void label_source_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                string strRecPath = (String)e.Data.GetData("Text");

                ListViewItem item = ListViewUtil.FindItem(this.listView_accept_records, strRecPath, COLUMN_RECPATH);

                // 观察一个listview事项是否能够被设置为source?
                if (IsSourceable(item) == false)
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }

                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void label_source_DragDrop(object sender, DragEventArgs e)
        {
            string strError = "";

            string strRecPath = (String)e.Data.GetData("Text");

            ListViewItem item = ListViewUtil.FindItem(this.listView_accept_records, strRecPath, COLUMN_RECPATH);
            if (item == null)
            {
                strError = "列表中居然没有找到路径为 '" + strRecPath + "' 的事项";
                goto ERROR1;
            }

            int nRet = WarningSetSource(item);
            if (nRet == 0)
                return;
            /*
            RecordInfo info = GetRecordInfo(item);
            string strRole = ListViewUtil.GetItemText(item,
                COLUMN_ROLE);
            // 即将设置“源”
            if (StringUtil.IsInList("源", strRole) == false)
            {
                // 看看是否有订购信息
                if (info.HasOrderInfo == false)
                {
                    DialogResult result = MessageBox.Show(this,
                        "当前记录 '"+strRecPath+"' 中未包含相应的订购信息，一般情况下不适合作为“源”记录。\r\n\r\n实际要强行设置为“源”？",
                        "AcceptForm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                        return;
                }
            }
             * */

            nRet = SetSource(item,
                "toggle",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        private void label_target_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                string strRecPath = (String)e.Data.GetData("Text");

                ListViewItem item = ListViewUtil.FindItem(this.listView_accept_records, strRecPath, COLUMN_RECPATH);

                // 观察一个listview事项是否能够被设置为target?
                if (IsTargetable(item) == false)
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }

                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void label_target_DragDrop(object sender, DragEventArgs e)
        {
            string strError = "";

            string strRecPath = (String)e.Data.GetData("Text");

            ListViewItem item = ListViewUtil.FindItem(this.listView_accept_records, strRecPath, COLUMN_RECPATH);
            if (item == null)
            {
                strError = "列表中居然没有找到路径为 '" + strRecPath + "' 的事项";
                goto ERROR1;
            }

            int nRet = SetTarget(item,
                "toggle",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }



        private void label_biblioSource_DragDrop(object sender, DragEventArgs e)
        {
            string strError = "";

            string strRecPath = (String)e.Data.GetData("Text");

            ListViewItem item = ListViewUtil.FindItem(this.listView_accept_records, strRecPath, COLUMN_RECPATH);
            if (item == null)
            {
                strError = "列表中居然没有找到路径为 '" + strRecPath + "' 的事项";
                goto ERROR1;
            }

            int nRet = SetBiblioSource(item,
                "toggle",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void label_biblioSource_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                string strRecPath = (String)e.Data.GetData("Text");

                ListViewItem item = ListViewUtil.FindItem(this.listView_accept_records, strRecPath, COLUMN_RECPATH);

                // 观察一个listview事项是否能够被设置为target?
                if (IsBiblioSourceable(item) == false)
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }

                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;
        }


        private void listView_accept_records_ItemDrag(object sender,
            ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.listView_accept_records.DoDragDrop(
                    ListViewUtil.GetItemText((ListViewItem)e.Item, COLUMN_RECPATH),
                    DragDropEffects.Link);
            }
        }

        private void listView_accept_records_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void listView_accept_records_DragDrop(object sender, DragEventArgs e)
        {
            // TODO: 如何识别从自己drag出来的情况?

            string strWhole = (String)e.Data.GetData("Text");

            DoPasteTabbedText(strWhole,
                false);
            return;
        }


#endregion

        // 获得一个书目记录
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetBiblioXml(string strRecPath,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取书目记录 ...");
            stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获取书目记录 ...");
            try
            {
                string[] formats = new string[1];
                formats[0] = "xml";
                string[] results = null;
                byte[] timestamp = null;

                Debug.Assert(String.IsNullOrEmpty(strRecPath) == false, "strRecPath值不能为空");

                long lRet = channel.GetBiblioInfos(
                    looping.Progress,
                    strRecPath,
                    "",
                    formats,
                    out results,
                    out timestamp,
                    out strError);
                if (lRet == 0)
                {
                    if (String.IsNullOrEmpty(strError) == true)
                        strError = "记录 '" + strRecPath + "' 没有找到";
                    return 0;
                }

                if (lRet == -1)
                {
                    strError = "获得书目xml时发生错误: " + strError;
                    return -1;
                }
                Debug.Assert(results != null && results.Length == 1, "results必须包含1个元素");
                strXml = results[0];

                return 1;
            }
            finally
            {
                looping.Dispose();
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                */
            }
        }

        // 获得一个书目记录的title
        // return:
        //      -1  error
        //      0   not found title
        //      1   found title
        int GetBiblioTitle(string strRecPath,
            out string strTitle,
            out string strError)
        {
            strError = "";
            strTitle = "";

            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取书目题名 ...");
            stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获取书目题名 ...");
            try
            {
                string[] formats = new string[1];
                formats[0] = "@title";
                string[] results = null;
                byte[] timestamp = null;

                Debug.Assert(String.IsNullOrEmpty(strRecPath) == false, "strRecPath值不能为空");

                long lRet = channel.GetBiblioInfos(
                    looping.Progress,
                    strRecPath,
                    "",
                    formats,
                    out results,
                    out timestamp,
                    out strError);
                if (lRet == 0)
                {
                    return 0;
                }

                if (lRet == -1)
                {
                    strError = "获得书目title时发生错误: " + strError;
                    return -1;
                }
                Debug.Assert(results != null && results.Length == 1, "results必须包含1个元素");
                strTitle = results[0];

                return 1;
            }
            finally
            {
                looping.Dispose();
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                */
            }
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 检查出版物类型值
            if (this.comboBox_prepare_type.Text != "图书"
                && this.comboBox_prepare_type.Text != "连续出版物")
            {
                MessageBox.Show(this, "出版物类型必须为 图书 或 连续出版物");
                this.button_next.Enabled = false;
                return;
            }

            /*
            // 设置ISBN/ISSN标签
            if (this.comboBox_prepare_type.Text == "图书")
                this.label_isbnIssn.Text = "ISBN(&I):";
            else
            {
                Debug.Assert(this.comboBox_prepare_type.Text == "连续出版物", "");
                this.label_isbnIssn.Text = "ISSN(&I):";
            }
             * */
            if (this.tabControl_main.SelectedTab == this.tabPage_accept)
            {
                /*
                // 检查检索途径是否和出版物类型矛盾
                string strFromStyle = "";

                try
                {
                    strFromStyle = Program.MainForm.GetBiblioFromStyle(this.comboBox_accept_from.Text);
                }
                catch
                {
                }

                if (this.comboBox_prepare_type.Text == "图书")
                {
                    if (strFromStyle.ToLower() == "issn")
                    {
                        MessageBox.Show(this, "警告：出版物类型为 图书 时，检索 ISSN 恐怕难以命中。请重新选择适当的检索途径");
                        this.comboBox_accept_from.Focus();
                    }
                }
                else
                {
                    if (strFromStyle.ToLower() == "isbn")
                    {
                        MessageBox.Show(this, "警告：出版物类型为 连续出版物 时，检索 ISBN 恐怕难以命中。请重新选择适当的检索途径");
                        this.comboBox_accept_from.Focus();
                    }
                }*/

                // 观察检索途径，如果图书时为ISSN，期刊时为ISBN，则修改它
                try
                {
                    string strFromStyle = "";
                    string strFromCaption = "";

                    strFromStyle = BiblioSearchForm.GetBiblioFromStyle(this.comboBox_accept_from.Text);


                    if (this.comboBox_prepare_type.Text == "图书")
                    {
                        if (strFromStyle.ToLower() == "issn")
                        {
                            strFromCaption = Program.MainForm.GetBiblioFromCaption("isbn");
                            if (String.IsNullOrEmpty(strFromCaption) == false)
                                this.comboBox_accept_from.Text = strFromCaption;
                        }
                    }
                    else
                    {
                        if (strFromStyle.ToLower() == "isbn")
                        {
                            strFromCaption = Program.MainForm.GetBiblioFromCaption("issn");
                            if (String.IsNullOrEmpty(strFromCaption) == false)
                                this.comboBox_accept_from.Text = strFromCaption;
                        }
                    }
                }
                catch
                {
                }


                if (this.comboBox_prepare_type.Text == "连续出版物")
                {
                    this.label_biblioSource.Visible = false;
                }
                else
                {
                    this.label_biblioSource.Visible = true;
                }
            }

            this.SetNextButtonEnable();
        }

        private void button_finish_printAcceptList_Click(object sender, EventArgs e)
        {
            PrintAcceptForm print_form = Program.MainForm.EnsurePrintAcceptForm();

            Debug.Assert(print_form != null, "");

            print_form.Activate();

            // 2009/2/4
            print_form.PublicationType = this.comboBox_prepare_type.Text;

            // 根据批次号检索装载数据
            // parameters:
            //      strDefaultBatchNo   缺省的批次号。如果为null，则表示不使用这个参数。
            print_form.LoadFromAcceptBatchNo(this.tabComboBox_prepare_batchNo.Text);

        }

        private void tabComboBox_prepare_batchNo_TextChanged(object sender, EventArgs e)
        {
            SetWindowTitle();
            this.SetNextButtonEnable();
        }

        private void comboBox_prepare_type_TextChanged(object sender, EventArgs e)
        {
            SetWindowTitle();
            this.SetNextButtonEnable();
        }

        void SetWindowTitle()
        {
            this.Text = "验收";

            if (this.tabComboBox_prepare_batchNo.Text != "")
                this.Text += " 批次号: " + this.tabComboBox_prepare_batchNo.Text;
            if (this.comboBox_prepare_type.Text != "")
                this.Text += " 类型: " + this.comboBox_prepare_type.Text;
        }

        int m_nInDropDown = 0;

        private void tabComboBox_prepare_batchNo_DropDown(object sender, EventArgs e)
        {
            // 防止重入
            if (this.m_nInDropDown > 0)
                return;

            ComboBox combobox = (ComboBox)sender;
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                if (combobox.Items.Count == 0
                    && this.GetBatchNoTable != null)
                {
                    GetKeyCountListEventArgs e1 = new GetKeyCountListEventArgs();
                    this.GetBatchNoTable(this, e1);

                    if (e1.KeyCounts != null)
                    {
                        for (int i = 0; i < e1.KeyCounts.Count; i++)
                        {
                            KeyCount item = e1.KeyCounts[i];
                            combobox.Items.Add(item.Key + "\t" + item.Count.ToString() + "笔");
                        }
                    }
                    else
                    {
                        combobox.Items.Add("<not found>");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }


        // 根据字符串构造ListViewItem。
        // 本函数和Global.BuildListViewItem的区别，是增加了一个特殊的第二栏目，用于显示角色信息
        // 字符串的格式为\t间隔的
        // parameters:
        //      list    可以为null。如果为null，就没有自动扩展列标题数目的功能
        static ListViewItem BuildAcceptListViewItem(
            ListView list,
            string strLine)
        {
            ListViewItem item = new ListViewItem();
            string[] parts = strLine.Split(new char[] { '\t' });
            for (int i = 0, j = 0; i < parts.Length; i++, j++)
            {
                // 跳过第二列
                if (j == 1)
                    j++;

                ListViewUtil.ChangeItemText(item, j, parts[i]);

                // 确保列标题数目够
                if (list != null)
                    ListViewUtil.EnsureColumns(list, parts.Length, 100);

            }

            return item;
        }

        void DoPasteTabbedText(string strWhole,
            bool bInsertBefore)
        {
            int index = -1;

            int nSkipCount = 0;

            if (this.listView_accept_records.SelectedIndices.Count > 0)
                index = this.listView_accept_records.SelectedIndices[0];

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            /*
            this.EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装载记录 ...");
            stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在装载记录 ...",
                "disableControl");
            try
            {
                string[] lines = strWhole.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    ListViewItem item = BuildAcceptListViewItem(
                        this.listView_accept_records,
                        lines[i]);

                    string strPath = item.Text;

                    // 检查数据库名是否为合法书目库名
                    string strDbName = Global.GetDbName(strPath);
                    if (Program.MainForm.IsBiblioDbName(strDbName) == false)
                    {
                        nSkipCount++;
                        continue;
                    }

                    if (index == -1)
                        this.listView_accept_records.Items.Add(item);
                    else
                    {
                        if (bInsertBefore == true)
                            this.listView_accept_records.Items.Insert(index, item);
                        else
                            this.listView_accept_records.Items.Insert(index + 1, item);

                        index++;
                    }

                    // 
                    // 根据记录路径，获得ListViewItem事项的imageindex下标
                    // return:
                    //      -2  根本不是书目库
                    //      -1  不是采购源或目标的其它书目库
                    //      0   源
                    //      1   目标
                    //      2   同时为源和目标
                    //      3   外源
                    int image_index = this.db_infos.GetItemType(strPath,
                        this.comboBox_prepare_type.Text);
                    // Debug.Assert(image_index != -2, "居然检索到非书目库的记录?");
                    item.ImageIndex = image_index;

                    SetItemColor(item); //

                    string strError = "";
                    int nRet = RefreshBrowseLine(
                        looping.Progress,
                        channel,
                        item,
                        out strError);
                    if (nRet == -1)
                    {
                        ListViewUtil.ChangeItemText(item, 2, strError);
                    }

                    item.Selected = true;
                }
            }
            finally
            {
                looping.Dispose();
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                this.EnableControls(true);
                */
                this.Cursor = oldCursor;
            }

            if (nSkipCount > 0)
            {
                MessageBox.Show(this, "有 " + nSkipCount.ToString() + " 个不是书目库的事项被忽略");
            }
        }

        private void comboBox_prepare_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 出版物类型变化，列表中原有的事项必须清除，以免发生误会
            this.listView_accept_records.Items.Clear();

            // 迫使重新获得验收批次号列表
            this.tabComboBox_prepare_batchNo.Items.Clear();

            // 刷新参与检索的书目库名列表
            this.FillDbNameList();
        }

        private void listView_accept_records_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_accept_records, e);
        }

        private void checkedListBox_prepare_dbNames_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue == CheckState.Unchecked
                && e.NewValue == CheckState.Checked)
            {
                string strText = (string)this.checkedListBox_prepare_dbNames.Items[e.Index];
                if (strText.Length > 0 && strText[0] == '<')
                {
                    // 将其余事项的checked清除
                    for (int i = 0; i < this.checkedListBox_prepare_dbNames.Items.Count; i++)
                    {
                        if (i == e.Index)
                            continue;
                        if (this.checkedListBox_prepare_dbNames.GetItemChecked(i) == true)
                            this.checkedListBox_prepare_dbNames.SetItemChecked(i, false);
                    }
                }
                else
                {
                    // 将"<...>"事项的checked清除
                    string strFirstItemText = (string)this.checkedListBox_prepare_dbNames.Items[0];
                    if (strFirstItemText.Length > 0 && strFirstItemText[0] == '<')
                    {
                        if (this.checkedListBox_prepare_dbNames.GetItemChecked(0) == true)
                            this.checkedListBox_prepare_dbNames.SetItemChecked(0, false);
                    }
                }
            }
        }

        string GetDbNameListString()
        {
            string strResult = "";
            for (int i = 0; i < this.checkedListBox_prepare_dbNames.CheckedItems.Count; i++)
            {
                if (i > 0)
                    strResult += ",";
                strResult += (string)this.checkedListBox_prepare_dbNames.CheckedItems[i];
            }

            return strResult;
        }

        void FillSellerList()
        {
            this.comboBox_sellerFilter.Items.Clear();

            this.comboBox_sellerFilter.Items.Add("<不过滤>");

            {
                int nRet = Program.MainForm.GetValueTable("orderSeller",
                    "",
                    out string[] values,
                    out string strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                var list = new List<string>(values);

                // 去掉每个元素内的 {} 部分
                list = StringUtil.FromListString(StringUtil.GetPureSelectedValue(StringUtil.MakePathList(list)));

                this.comboBox_sellerFilter.Items.AddRange(list.ToArray());
            }
        }


        void FillDbNameList()
        {
            this.checkedListBox_prepare_dbNames.Items.Clear();

            if (Program.MainForm.BiblioDbProperties != null)
            {
                foreach (var prop in Program.MainForm.BiblioDbProperties)
                {
                    // BiblioDbProperty prop = Program.MainForm.BiblioDbProperties[i];

                    if (this.comboBox_prepare_type.Text == "图书")
                    {
                        if (String.IsNullOrEmpty(prop.IssueDbName) == false)
                            continue;

                    }
                    else
                    {
                        if (String.IsNullOrEmpty(prop.IssueDbName) == true)
                            continue;

                    }

                    this.checkedListBox_prepare_dbNames.Items.Add(prop.DbName);
                }
            }

            // 加入第一项
            if (this.checkedListBox_prepare_dbNames.Items.Count > 1)
            {
                if (this.comboBox_prepare_type.Text == "图书")
                {
                    this.checkedListBox_prepare_dbNames.Items.Insert(0, "<全部图书>");
                }
                else
                {
                    this.checkedListBox_prepare_dbNames.Items.Insert(0, "<全部期刊>");
                }

                // 缺省勾选第一项
                this.checkedListBox_prepare_dbNames.SetItemChecked(0, true);
            }
            else
            {
                // 缺省勾选全部事项
                for (int i = 0; i < this.checkedListBox_prepare_dbNames.Items.Count; i++)
                {
                    this.checkedListBox_prepare_dbNames.SetItemChecked(i, true);
                }
            }
        }

        private void comboBox_accept_matchStyle_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_accept_matchStyle.Text == "空值")
            {
                this.textBox_accept_queryWord.Text = "";
                this.textBox_accept_queryWord.Enabled = false;
            }
            else
            {
                this.textBox_accept_queryWord.Enabled = true;
            }
        }

        // 是否单击浏览框列表行即可装入详细窗。
        // 如果==false，表示要双击才能装入
        /// <summary>
        /// 是否单击浏览框列表行即可装入详细窗。如果为 false，表示要双击才能装入
        /// </summary>
        public bool SingleClickLoadDetail
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "accept_form",
                    "single_click_load_detail",
                    false);
            }
        }

        // 2020/9/13
        // 创建册记录的时候是否要自动给 state 字段添加“加工中”状态值
        public static bool SetProcessingState
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                     "accept_form",
    "set_processing_state",
    true);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
     "accept_form",
"set_processing_state",
value);
            }
        }

        private void listView_accept_records_DoubleClick(object sender, EventArgs e)
        {
            if (this.SingleClickLoadDetail == true)
                return;

            if (this.listView_accept_records.SelectedItems.Count == 0
                || this.listView_accept_records.SelectedItems.Count > 1)    // 2009/2/3 多选时也要禁止进入详细窗
            {
                API.PostMessage(this.Handle, WM_LOAD_DETAIL, 0, -1);
                return;
            }
            API.PostMessage(this.Handle, WM_LOAD_DETAIL, 0, this.listView_accept_records.SelectedIndices[0]);
        }

        // 记录信息
        // 包括：title
        /*public*/
        class RecordInfo
        {
            public string BiblioTitle = "";

            // 本事项的title是否和关键事项的title匹配了？如果匹配了，显示为实在的状态；否则显示为发虚的状态
            public bool TitleMatch = true;

            // 是否含有订购信息?
            public bool HasOrderInfo = true;
        }

        /// <summary>
        /// 是否已经停靠
        /// </summary>
        public bool Docked = false;

        List<Control> _freeControls = new List<Control>();

        /// <summary>
        /// 进行停靠
        /// </summary>
        /// <param name="bShowFixedPanel">是否同时促成显示固定面板</param>
        public void DoDock(bool bShowFixedPanel)
        {
            // return; // 测试内存泄漏

            if (Program.MainForm.CurrentAcceptControl != this.panel_main)
            {
                Program.MainForm.CurrentAcceptControl = this.panel_main;
                // 防止内存泄漏
                ControlExtention.AddFreeControl(_freeControls, this.panel_main);
            }

            if (bShowFixedPanel == true
                && Program.MainForm.PanelFixedVisible == false)
                Program.MainForm.PanelFixedVisible = true;

            Program.MainForm.ActivateAcceptPage();

            this.Docked = true;
            this.Visible = false;
        }

        void DisposeFreeControls()
        {
            // 2015/11/7
            if (this.panel_main != null && Program.MainForm != null)
            {
                // 如果当前固定面板拥有 tabcontrol，则要先解除它的拥有关系，否则怕本 Form 摧毁的时候无法 Dispose() 它
                if (Program.MainForm.CurrentAcceptControl == this.panel_main)
                    Program.MainForm.CurrentAcceptControl = null;
            }

            ControlExtention.DisposeFreeControls(_freeControls);
        }

        /// <summary>
        /// 从停靠状态恢复成浮动状态
        /// </summary>
        public void DoFloating()
        {
            if (this.Docked == true)
            {
                if (Program.MainForm.CurrentAcceptControl == this.panel_main)
                    Program.MainForm.CurrentAcceptControl = null;

                this.Docked = false;

                if (this.Controls.IndexOf(this.panel_main) == -1)
                    this.Controls.Add(this.panel_main);

                this.Visible = true;
            }
        }

        /// <summary>
        /// TabControl
        /// </summary>
        public Control MainControl
        {
            get
            {
                return this.panel_main;
            }
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }

            if (keyData == Keys.Enter || keyData == Keys.LineFeed)
            {
                DoEnterKey();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        /// <summary>
        /// 处理回车键
        /// </summary>
        public void DoEnterKey()
        {
            if (this.textBox_accept_queryWord.Focused == true)
                button_accept_searchISBN_Click(this, new EventArgs());
            else if (this.listView_accept_records.Focused == true)
                listView_accept_records_DoubleClick(this, new EventArgs());
        }

        private void button_defaultEntityFields_Click(object sender, EventArgs e)
        {
            using (EntityFormOptionDlg dlg = new EntityFormOptionDlg())
            {
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.DisplayStyle = "quick_entity";
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult == DialogResult.Cancel)
                    return;

                // 批次号可能被对话框修改，需要刷新
                //this.tabComboBox_prepare_batchNo.Text = EntityFormOptionDlg.GetFieldValue("quickRegister_default",
                //    "batchNo");

                SetCheckBoxEnable();
            }
        }

        private void tabComboBox_prepare_batchNo_Leave(object sender, EventArgs e)
        {
            EntityFormOptionDlg.SetFieldValue("quickRegister_default",
                "batchNo",
                this.tabComboBox_prepare_batchNo.Text);
        }

        // 根据默认记录的内容，将两个 checkbox 的状态设置为可修改或者不可修改状态。这样可以避免用户设置出和默认只记录矛盾的 checkbox 状态
        void SetCheckBoxEnable()
        {
            string state = EntityFormOptionDlg.GetFieldValue("quickRegister_default",
"state");
            if (StringUtil.IsInList("加工中", state))
            {
                this.checkBox_prepare_setProcessingState.Checked = true;
                this.checkBox_prepare_setProcessingState.Enabled = false;
            }
            else
            {
                this.checkBox_prepare_setProcessingState.Enabled = true;
            }

            string accessNo = EntityFormOptionDlg.GetFieldValue("quickRegister_default",
"accessNo");
            if (accessNo == "@accessNo")
            {
                this.checkBox_prepare_createCallNumber.Checked = true;
                this.checkBox_prepare_createCallNumber.Enabled = false;
            }
            else
            {
                this.checkBox_prepare_createCallNumber.Enabled = true;
            }
        }

        private void checkBox_prepare_setProcessingState_CheckedChanged(object sender, EventArgs e)
        {
            // 是否添加加工中状态，是例外，要单独保存
            SetProcessingState = this.checkBox_prepare_setProcessingState.Checked;
        }

        private void AcceptForm_Enter(object sender, EventArgs e)
        {
        }

        private void AcceptForm_Leave(object sender, EventArgs e)
        {
        }

        private void tabPage_prepare_Enter(object sender, EventArgs e)
        {
            if (this.checkBox_prepare_setProcessingState.Checked != AcceptForm.SetProcessingState)
                this.checkBox_prepare_setProcessingState.Checked = AcceptForm.SetProcessingState;
        }

        private void tabPage_prepare_Leave(object sender, EventArgs e)
        {
            if (AcceptForm.SetProcessingState != this.checkBox_prepare_setProcessingState.Checked)
                AcceptForm.SetProcessingState = this.checkBox_prepare_setProcessingState.Checked;
        }

#region 新风格的 ChannelPool

        ChannelList _channelList = new ChannelList();

        // parameters:
        //      strStyle    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public virtual LibraryChannel GetChannel(string strServerUrl = ".",
            string strUserName = ".",
            GetChannelStyle style = GetChannelStyle.None,
            string strClientIP = "")
        {
            LibraryChannel channel = Program.MainForm.GetChannel(strServerUrl, strUserName, style, strClientIP);
            _channelList.AddChannel(channel);
            // TODO: 检查数组是否溢出
            return channel;
        }

        public virtual void ReturnChannel(LibraryChannel channel)
        {
            Program.MainForm.ReturnChannel(channel);
            _channelList.RemoveChannel(channel);
        }


        public void DoStop(object sender, StopEventArgs e)
        {
            _channelList.AbortAll();
        }

        public string CurrentUserName
        {
            get
            {
                return Program.MainForm?._currentUserName;
            }
        }

        // 当前用户能管辖的一个或者多个馆代码
        public string CurrentLibraryCodeList
        {
            get
            {
                return Program.MainForm?._currentLibraryCodeList;
            }
        }

        public string CurrentRights
        {
            get
            {
                return Program.MainForm?._currentUserRights;
            }
        }

#endregion

#region looping

        // 三种动作: GetChannel() BeginLoop() 和 EnableControl()
        // parameters:
        //          style 可以有如下子参数:
        //              disableControl
        //              timeout:hh:mm:ss 确保超时参数在 hh:mm:ss 以长
        // https://learn.microsoft.com/en-us/dotnet/api/system.timespan.parse?view=net-6.0
        // [ws][-]{ d | [d.]hh:mm[:ss[.ff]] }[ws]
        public Looping Looping(
            out LibraryChannel channel,
            string text = "",
            string style = null,
            StopEventHandler handler = null)
        {
            var controlDisabled = StringUtil.IsInList("disableControl", style);
            var timeout_string = StringUtil.GetParameterByPrefix(style, "timeout"); // 不小于这么多
            var settimeout_string = StringUtil.GetParameterByPrefix(style, "settimeout");   // 设置为这么多

            var serverUrl = StringUtil.GetParameterByPrefix(style, "serverUrl");
            if (string.IsNullOrEmpty(serverUrl) == false)
                serverUrl = StringUtil.UnescapeString(serverUrl);
            var userName = StringUtil.GetParameterByPrefix(style, "userName");

            channel = this.GetChannel(serverUrl, userName);

            var old_timeout = channel.Timeout;
            bool timeout_changed = false;
            if (string.IsNullOrEmpty(timeout_string) == false)
            {
                var new_timeout = TimeSpan.Parse(timeout_string);
                if (new_timeout > old_timeout)
                {
                    channel.Timeout = new_timeout;
                    timeout_changed = true;
                }
            }
            if (string.IsNullOrEmpty(settimeout_string) == false)
            {
                var new_timeout = TimeSpan.Parse(settimeout_string);
                if (new_timeout != old_timeout)
                {
                    channel.Timeout = new_timeout;
                    timeout_changed = true;
                }
            }

            var looping = _loopingHost.BeginLoop(
                handler == null ? this.DoStop : handler,
                text,
                style);

            if (controlDisabled)
                this.EnableControls(false);

            var channel_param = channel;
            looping.Closed = () =>
            {
                if (controlDisabled)
                    this.EnableControls(true);
                if (timeout_changed)
                    channel_param.Timeout = old_timeout;
                this.ReturnChannel(channel_param);
            };

            return looping;
        }

        // 两种动作: BeginLoop() 和 EnableControl()
        public Looping Looping(string text,
            string style = null,
            StopEventHandler handler = null)
        {
            var controlDisabled = StringUtil.IsInList("disableControl", style);

            var looping = _loopingHost.BeginLoop(
                handler == null ? this.DoStop : handler,
                text,
                style);

            if (controlDisabled)
                this.EnableControls(false);

            looping.Closed = () =>
            {
                if (controlDisabled)
                    this.EnableControls(true);
            };

            return looping;
        }


        internal LoopingHost _loopingHost = new LoopingHost();

        public Looping BeginLoop(StopEventHandler handler,
string text,
string style = null)
        {
            return _loopingHost.BeginLoop(handler, text, style);
        }

        public void EndLoop(Looping looping)
        {
            _loopingHost.EndLoop(looping);
        }

        public bool HasLooping()
        {
            return _loopingHost.HasLooping();
        }

        public Looping TopLooping
        {
            get
            {
                return _loopingHost.TopLooping;
            }
        }

#endregion
    }



    // 采购数据库信息容器
    /*public*/
    class OrderDbInfos : List<OrderDbInfo>
    {
        public void Build(MainForm mainform)
        {
            this.Clear();
            if (mainform.BiblioDbProperties != null)
            {
                foreach (var property in mainform.BiblioDbProperties)
                {
                    // BiblioDbProperty property = mainform.BiblioDbProperties[i];

                    OrderDbInfo info = new OrderDbInfo();
                    info.BiblioDbName = property.DbName;
                    info.OrderDbName = property.OrderDbName;
                    info.EntityDbName = property.ItemDbName;
                    info.IssueDbName = property.IssueDbName;

                    info.Syntax = property.Syntax;
                    if (String.IsNullOrEmpty(info.Syntax) == true)
                        info.Syntax = "unimarc";
                    // info.InCirculation = property.InCirculation;

                    info.Role = property.Role;  // 2009/10/23

                    this.Add(info);
                }
            }
        }

        public OrderDbInfo LocateByBiblioDbName(string strBiblioDbName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                OrderDbInfo info = this[i];

                if (info.BiblioDbName == strBiblioDbName)
                    return info;
            }

            return null;
        }

        // 获得可以作为目标库的书目库名
        public List<string> GetTargetDbNames()
        {
            List<string> results = new List<string>();
            for (int i = 0; i < this.Count; i++)
            {
                OrderDbInfo info = this[i];

                if (info.IsTarget == true || info.IsSourceAndTarget == true)
                    results.Add(info.BiblioDbName);
            }

            return results;
        }

        // 根据记录路径，获得ListViewItem事项的imageindex下标
        // return:
        //      -2  根本不是书目库
        //      -1  不是采购源或目标的其它书目库
        //      0   源
        //      1   目标
        //      2   同时为源和目标
        //      3   外源
        public int GetItemType(string strRecPath,
            string strPubType)
        {
            Debug.Assert(strPubType == "连续出版物" || strPubType == "图书", "");

            string strDbName = Global.GetDbName(strRecPath);
            for (int i = 0; i < this.Count; i++)
            {
                OrderDbInfo info = this[i];


                if (info.BiblioDbName == strDbName)
                {
                    // 注：如果是源或目标库，千万不要定义为“外源”角色，因为外源角色会优先
                    if (StringUtil.IsInList("biblioSource", info.Role) == true)
                        return 3;

                    if (info.IsSourceAndTarget == true)
                    {
                        if (strPubType == "图书")
                        {
                            if (String.IsNullOrEmpty(info.IssueDbName) == true)
                                return 2;

                            return -1;   // 因有期库，不能当作“图书”类型的目标或者源
                        }
                        else
                        {
                            Debug.Assert(strPubType == "连续出版物", "");

                            if (String.IsNullOrEmpty(info.IssueDbName) == false)
                                return 2;

                            // return 0;   // 因没有期库，不能当作“连续出版物”类型的目标，但是可以当作源
                            return -1;  // 因没有期库，不能当作“连续出版物”类型的目标或源 2009/2/3 changed
                        }
                        // return 2;
                    }
                    if (info.IsSource == true)
                    {
                        if (strPubType == "图书")
                        {
                            if (String.IsNullOrEmpty(info.IssueDbName) == true)
                                return 0;

                            return -1;   // 因有期库，不能当作“图书”类型的源
                        }
                        else
                        {
                            Debug.Assert(strPubType == "连续出版物", "");

                            if (String.IsNullOrEmpty(info.IssueDbName) == false)
                                return 0;

                            // return 0;   // 因没有期库，不能当作“连续出版物”类型的目标，但是可以当作源
                            return -1;   // 因没有期库，不能当作“连续出版物”类型的目标或源 2009/2/3 changed
                        }

                        // return 0;
                    }
                    if (info.IsTarget == true)
                    {
                        if (strPubType == "图书")
                        {
                            if (String.IsNullOrEmpty(info.IssueDbName) == true)
                                return 1;

                            return -1;   // 因有期库，不能当作“图书”类型的目标
                        }
                        else
                        {
                            Debug.Assert(strPubType == "连续出版物", "");

                            if (String.IsNullOrEmpty(info.IssueDbName) == false)
                                return 1;

                            return -1;   // 因没有期库，不能当作“连续出版物”类型的目标
                        }

                        // return 1;
                    }

                    return -1;  // -1 表示根本不是采购的源或者目标库，也就是说，所匹配的这个库，既没有包含订购库，也没有包含实体库
                }
            }

            return -2;  // 根本没有找到这个书目库名
        }
    }

    // 采购数据库信息
    /*public*/
    class OrderDbInfo
    {
        public string BiblioDbName = "";
        public string OrderDbName = "";
        public string EntityDbName = "";
        public string IssueDbName = "";

        public string Syntax = "";
        public bool InCirculation = false;

        public string Role = "";    // 角色 2009/10/23

        public bool IsOrderWork
        {
            get
            {
                if (StringUtil.IsInList("orderWork", this.Role) == true)
                    return true;
                return false;
            }
        }

        // 是否为源
        public bool IsSource
        {
            get
            {
                if (String.IsNullOrEmpty(this.OrderDbName) == false)
                    return true;
                return false;
            }
        }

        // 是否为目标
        public bool IsTarget
        {
            get
            {
                if (String.IsNullOrEmpty(this.EntityDbName) == false)
                    return true;
                return false;
            }
        }

        // 是否既是源、也是目标？
        public bool IsSourceAndTarget
        {
            get
            {
                if (this.IsSource == true && this.IsTarget == true)
                    return true;
                return false;
            }
        }
    }
}