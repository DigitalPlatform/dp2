using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

using ClosedXML.Excel;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Marc;

using DigitalPlatform.IO;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using DigitalPlatform.MessageClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.Z3950.UI;
using DigitalPlatform.Z3950;
using static dp2Circulation.Order.ExportExcelFile;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Vml.Office;

namespace dp2Circulation
{
    /// <summary>
    /// 书目或规范查询窗
    /// </summary>
    public partial class BiblioSearchForm : MyForm
    {
        string _dbType = "biblio";
        public string DbType
        {
            get
            {
                return _dbType;
            }
            set
            {
                _dbType = value;
                SetTitle("");
                if (value == "biblio")
                    this.label_biblioDbName.Text = "书目库(&D)";
                if (value == "authority")
                    this.label_biblioDbName.Text = "规范库(&D)";
            }
        }

        Commander commander = null;

        CommentViewerForm m_commentViewer = null;

        Hashtable m_biblioTable = new Hashtable(); // 书目记录路径 --> 书目信息

        // const int WM_SELECT_INDEX_CHANGED = API.WM_USER + 200;

        // 最近使用过的记录路径文件名
        string m_strUsedRecPathFilename = "";

        bool m_bFirstColumnIsKey = false; // 当前listview浏览列的第一列是否应为key

        long m_lLoaded = 0; // 本次已经装入浏览框的条数
        long m_lHitCount = 0;   // 检索命中结果条数

        /*
        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();
         * */

        /// <summary>
        /// 最近导出过的记录路径文件路径
        /// </summary>
        public string ExportRecPathFilename = "";   // 使用过的导出路径文件
        /// <summary>
        /// 最近导出过的册记录路径文件路径
        /// </summary>
        public string ExportEntityRecPathFilename = ""; // 使用过的导出实体记录路径文件

        // BiblioDbFromInfo[] DbFromInfos = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        public BiblioSearchForm()
        {
            this.UseLooping = true; // 2022/10/30

            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_records.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);

            prop.CompareColumn -= new CompareEventHandler(SearchFormBase.prop_CompareColumn);
            prop.CompareColumn += new CompareEventHandler(SearchFormBase.prop_CompareColumn);
        }

#if NO
        void prop_CompareColumn(object sender, CompareEventArgs e)
        {
            // TODO: 实现 ISBN 排序。把 10 位和 13 位的归一化以后排序

            if (e.Column.SortStyle.Name == "call_number")
            {
                // 比较两个索取号的大小
                // return:
                //      <0  s1 < s2
                //      ==0 s1 == s2
                //      >0  s1 > s2
                e.Result = StringUtil.CompareAccessNo(e.String1, e.String2, true);
            }
            else if (e.Column.SortStyle.Name == "parent_id")
            {
                // 右对齐比较字符串
                // parameters:
                //      chFill  填充用的字符
                e.Result = StringUtil.CompareRecPath(e.String1, e.String2);
            }
            else if (e.Column.SortStyle.Name == "order_price")
            {
                e.Result = ItemSearchForm.CompareOrderPrice(e.String1, e.String2);
            }
            else if (e.Column.SortStyle.Name == "price")
            {
                e.Result = StringUtil.ComparePrice(e.String1, e.String2);
            }
            else
                e.Result = string.Compare(e.String1, e.String2);
        }
#endif

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            if (e.DbName == "<blank>")
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                e.ColumnTitles.Add("检索点");
                e.ColumnTitles.Add("数量");
                return;
            }

            // e.ColumnTitles = Program.MainForm.GetBrowseColumnNames(e.DbName);
            if (e.DbName.IndexOf("@") == -1)
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(e.DbName);
                if (temp != null)
                    e.ColumnTitles.AddRange(temp);  // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件
            }
            else
            {
                string strFormat = "";
                if (this.m_biblioTable != null)
                {
                    BiblioInfo info = this.m_biblioTable[e.DbName] as BiblioInfo;
                    if (info != null)
                        strFormat = info.Format;
                }
                e.ColumnTitles = new ColumnPropertyCollection();
                string strColumnTitles = (string)_browseTitleTable[strFormat];
                List<string> titles = StringUtil.SplitList(strColumnTitles, '\t');
                foreach (string s in titles)
                {
                    ColumnProperty property = new ColumnProperty(s);
                    e.ColumnTitles.Add(property);
                }
            }

            if (this.m_bFirstColumnIsKey == true)
                e.ColumnTitles.Insert(0, "命中的检索点");
        }

        void ClearListViewPropertyCache()
        {
            ListViewProperty prop = (ListViewProperty)this.listView_records.Tag;
            prop.ClearCache();
        }

        private async void BiblioSearchForm_Load(object sender, EventArgs e)
        {
            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            Program.MainForm.AppInfo.LoadMdiLayout += new EventHandler(AppInfo_LoadMdiLayout);
            Program.MainForm.AppInfo.SaveMdiLayout += new EventHandler(AppInfo_SaveMdiLayout);

            var ret = await Program.MainForm.EnsureConnectLibraryServerAsync();
            if (ret == false)
            {
                this.ShowMessage("连接到 dp2library 失败，本窗口部分功能无法使用", "red");
            }

            if (this._dbType == "biblio")
                Program.MainForm.FillBiblioFromList(this.comboBox_from);
            else if (this._dbType == "authority")
                FillAuthorityFromList(this.comboBox_from);

            this.m_strUsedMarcQueryFilename = Program.MainForm.AppInfo.GetString(
                this._dbType + "searchform",
                "usedMarcQueryFilename",
                "");

            // 恢复上次退出时保留的检索途径
            string strFrom = Program.MainForm.AppInfo.GetString(
                this._dbType + "searchform",
                "search_from",
                "");
            if (String.IsNullOrEmpty(strFrom) == false)
                this.comboBox_from.Text = strFrom;

            this.checkedComboBox_biblioDbNames.Text = Program.MainForm.AppInfo.GetString(
                this._dbType + "searchform",
                "biblio_db_name",
                "<全部>");

            this.comboBox_matchStyle.Text = Program.MainForm.AppInfo.GetString(
                this._dbType + "searchform",
                "match_style",
                "前方一致");

            bool bHideMatchStyle = Program.MainForm.AppInfo.GetBoolean(
                "biblio_search_form",
                "hide_matchstyle",
                false);

            if (bHideMatchStyle == true)
            {
                this.label_matchStyle.Visible = false;
                this.comboBox_matchStyle.Visible = false;
                this.comboBox_matchStyle.Text = "前方一致"; // 隐藏后，采用缺省值
            }

            string strSaveString = Program.MainForm.AppInfo.GetString(
                this._dbType + "searchform",
                "query_lines",
                "^^^");
            this.dp2QueryControl1.Restore(strSaveString);

            comboBox_matchStyle_TextChanged(null, null);

            /*
            // FillFromList();
            // this.BeginInvoke(new Delegate_FillFromList(FillFromList));
            EnableControls(false);
            API.PostMessage(this.Handle, API.WM_USER + 100, 0, 0);
             */
            if (Program.MainForm != null)
                Program.MainForm.FixedSelectedPageChanged += new EventHandler(MainForm_FixedSelectedPageChanged);

            UpdateSearchShareMenu();
            UpdateZ3950Menu();
#if NO
            if (Program.MainForm.NormalDbProperties == null
                || Program.MainForm.BiblioDbFromInfos == null
                || Program.MainForm.BiblioDbProperties == null)
            {
                this.tableLayoutPanel_main.Enabled = false;
            }
#endif

            {
                string strError = "";
                List<string> codes = null;
                // 获得全部可用的图书馆代码。注意，并不包含 "" (全局)
                int nRet = Program.MainForm.GetAllLibraryCodes(out codes,
                out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
                else
                {
                    this.comboBox_location.Items.Clear();
                    this.comboBox_location.Items.Add("<不筛选>");
                    foreach (string code in codes)
                    {
                        this.comboBox_location.Items.Add(code);
                    }
                }

                this.comboBox_location.Text = Program.MainForm.AppInfo.GetString(
                    this._dbType + "searchform",
    "location_filter",
    "<不筛选>");
            }

            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.44") >= 0)
                toolStripMenuItem_subrecords.Checked = DisplaySubrecords;
            else
            {
                toolStripMenuItem_subrecords.Enabled = false;
                DisplaySubrecords = false;
            }
        }

        // TabComboBox版本
        // 右边列出style名
        /// <summary>
        /// 填充书目库检索途径 TabComboBox 列表
        /// 每一行左边是检索途径名，右边是 style 名
        /// </summary>
        /// <param name="comboBox_from">TabComboBox对象</param>
        public static void FillBiblioFromList(DigitalPlatform.CommonControl.TabComboBox comboBox_from)
        {
            comboBox_from.Items.Clear();

            comboBox_from.Items.Add("<全部>");

            if (Program.MainForm.BiblioDbFromInfos == null)
                return;

            Debug.Assert(Program.MainForm.BiblioDbFromInfos != null);

            string strFirstItem = "";
            // 装入检索途径
            foreach (BiblioDbFromInfo info in Program.MainForm.BiblioDbFromInfos)
            {
                comboBox_from.Items.Add(info.Caption + "\t" + GetDisplayStyle(info.Style));

                if (string.IsNullOrEmpty(strFirstItem))
                    strFirstItem = info.Caption;
            }

            comboBox_from.Text = strFirstItem;
        }

        public static void FillAuthorityFromList(DigitalPlatform.CommonControl.TabComboBox comboBox_from)
        {
            comboBox_from.Items.Clear();

            comboBox_from.Items.Add("<全部>");

            if (Program.MainForm.AuthorityDbFromInfos == null)
                return;

            Debug.Assert(Program.MainForm.AuthorityDbFromInfos != null);

            string strFirstItem = "";
            // 装入检索途径
            foreach (BiblioDbFromInfo info in Program.MainForm.AuthorityDbFromInfos)
            {
                comboBox_from.Items.Add(info.Caption + "\t" + GetDisplayStyle(info.Style));

                if (string.IsNullOrEmpty(strFirstItem))
                    strFirstItem = info.Caption;
            }

            comboBox_from.Text = strFirstItem;
        }

        // 过滤掉 _ 开头的那些style子串
        // parameters:
        //      bRemove2    是否也要滤除 __ 前缀的
        //                  当出现在检索途径列表里面的时候，为了避免误会，要出现 __ 前缀的；而发送检索请求到 dp2library 的时候，为了避免连带也引起匹配其他检索途径，要把 __ 前缀的 style 滤除
        public static string GetDisplayStyle(string strStyles,
            bool bRemove2 = false)
        {
            string[] parts = strStyles.Split(new char[] { ',' });
            List<string> results = new List<string>();
            foreach (string part in parts)
            {
                string strText = part.Trim();
                if (String.IsNullOrEmpty(strText) == true)
                    continue;

                if (bRemove2 == false)
                {
                    // 只滤除 _ 开头的
                    if (StringUtil.HasHead(strText, "_") == true
                        && StringUtil.HasHead(strText, "__") == false)
                        continue;
                }
                else
                {
                    // 2013/12/30 _ 和 __ 开头的都被滤除
                    if (StringUtil.HasHead(strText, "_") == true)
                        continue;
                }

                results.Add(strText);
            }

            return StringUtil.MakePathList(results, ",");
        }

        void MainForm_FixedSelectedPageChanged(object sender, EventArgs e)
        {
            // 固定面板属性区域被显示出来后
            if (Program.MainForm.ActiveMdiChild == this && Program.MainForm.CanDisplayItemProperty() == true)
            {
                RefreshPropertyView(false);
            }
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_nInViewing > 0;
        }

        void AppInfo_SaveMdiLayout(object sender, EventArgs e)
        {
            if (sender != this)
                return;
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_records);
                Program.MainForm.AppInfo.SetString(
                    this._dbType + "searchform",
                    "record_list_column_width",
                    strWidths);

                Program.MainForm.SaveSplitterPos(
    this.splitContainer_main,
                    this._dbType + "searchform",
    "splitContainer_main_ratio");
            }
        }

        void AppInfo_LoadMdiLayout(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            string strWidths = Program.MainForm.AppInfo.GetString(
                    this._dbType + "searchform",
    "record_list_column_width",
    "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_records,
                    strWidths,
                    true);
            }


            Program.MainForm.LoadSplitterPos(
this.splitContainer_main,
                    this._dbType + "searchform",
"splitContainer_main_ratio");
        }

        private void BiblioSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            if (_stop != null)
            {
                if (_stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }
            */

            if (this.m_nChangedCount > 0)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 " + m_nChangedCount + " 项修改尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "BiblioSearchForm",
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

        private void BiblioSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _zsearcher?.Dispose();

            if (this.commander != null)
                this.commander.Destroy();

            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetString(
                    this._dbType + "searchform",
        "usedMarcQueryFilename",
        this.m_strUsedMarcQueryFilename);

                // 保存检索途径
                Program.MainForm.AppInfo.SetString(
                    this._dbType + "searchform",
                    "search_from",
                    this.comboBox_from.Text);

                Program.MainForm.AppInfo.SetString(
                    this._dbType + "searchform",
                    "biblio_db_name",
                    this.checkedComboBox_biblioDbNames.Text);

                Program.MainForm.AppInfo.SetString(
                    this._dbType + "searchform",
                    "match_style",
                    this.comboBox_matchStyle.Text);

                Program.MainForm.AppInfo.SetString(
                    this._dbType + "searchform",
    "query_lines",
    this.dp2QueryControl1.GetSaveString());

                Program.MainForm.AppInfo.SetString(
                    this._dbType + "searchform",
"location_filter",
this.comboBox_location.Text);


                Program.MainForm.AppInfo.LoadMdiLayout -= new EventHandler(AppInfo_LoadMdiLayout);
                Program.MainForm.AppInfo.SaveMdiLayout -= new EventHandler(AppInfo_SaveMdiLayout);
            }

            if (this.m_commentViewer != null)
                this.m_commentViewer.Close();

            if (Program.MainForm != null)
                Program.MainForm.FixedSelectedPageChanged -= new EventHandler(MainForm_FixedSelectedPageChanged);
        }


        /*
        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case API.WM_USER + 100:
                    {
                        FillFromList();
                    }
                    break;

            }
            base.DefWndProc(ref m);
        }
         */



        // public delegate void Delegate_FillFromList();

        /// <summary>
        /// 检索命中的最大记录数限制参数。-1 表示不限制
        /// </summary>
        public static int MaxSearchResultCount
        {
            get
            {
                return (int)Program.MainForm.AppInfo.GetInt(
                    "biblio_search_form",
                    "max_result_count",
                    -1);
            }
        }

        // 是否以推动的方式装入浏览列表
        // 2008/1/20
        /// <summary>
        /// 是否以推动的方式装入浏览列表
        /// </summary>
        public static bool PushFillingBrowse
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "biblio_search_form",
                    "push_filling_browse",
                    false);
            }
        }

        // 多行检索时单个检索词的最大命中条数
        public static int MultilineMaxSearchResultCount
        {
            get
            {
                return (int)Program.MainForm.AppInfo.GetInt(
                    "biblio_search_form",
                    "multiline_max_result_count",
                    10);
            }
        }

        // 是否要在固定面板区“属性”属性页显示(书目记录的)子记录
        public static bool DisplaySubrecords
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "biblio_search_form",
                    "display_subrecords",
                    false);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
                    "biblio_search_form",
                    "display_subrecords",
                    value);
            }
        }

        // 
        /// <summary>
        /// 获得可用于检索式的匹配方式字符串
        /// </summary>
        /// <param name="strText">组合框中的匹配方式字符串</param>
        /// <returns>可用于检索式的匹配方式字符串</returns>
        public static string GetCurrentMatchStyle(string strText)
        {
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
#if NO
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
#endif

        // 给外部使用
        /// <summary>
        /// 浏览框 ListView 对象
        /// </summary>
        public ListView ListViewRecords
        {
            get
            {
                return this.listView_records;
            }
        }

        public void ClearListViewItems()
        {
            this.TryInvoke(() =>
            {
                this.listView_records.Items.Clear();

                ListViewUtil.ClearSortColumns(this.listView_records);

                // 清除所有需要确定的栏标题
                for (int i = 1; i < this.listView_records.Columns.Count; i++)
                {
                    this.listView_records.Columns[i].Text = i.ToString();
                }

                this.m_biblioTable = new Hashtable();
                this.m_nChangedCount = 0;

                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
            });
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
Keys keyData)
        {
            if ((keyData == Keys.Enter || keyData == Keys.LineFeed)
                && this.tabControl_query.SelectedTab == this.tabPage_logic)
            {
                _ = this.DoLogicSearch(false);
                return true;
            }

            /*
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }
             * */

            return base.ProcessDialogKey(keyData);
        }

        public Task DoLogicSearch(bool bOutputKeyID)
        {
            // 注：只有这样才能在独立线程中执行
            return Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        _doLogicSearch(bOutputKeyID);
                    }
                    catch (Exception ex)
                    {
                        this.MessageBoxShow($"DoLogicSearch() 异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                },
                default,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }


        void _doLogicSearch(bool bOutputKeyID)
        {
            string strError = "";
            bool bQuickLoad = false;    // 是否快速装入
            bool bClear = true; // 是否清除浏览窗中已有的内容

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                bQuickLoad = true;

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                bClear = false;

            // 修改窗口标题
            // this.Text = "书目查询 逻辑检索";
            this.SetTitle("逻辑查询");

            if (bOutputKeyID == true)
                this.m_bFirstColumnIsKey = true;
            else
                this.m_bFirstColumnIsKey = false;
            this.ClearListViewPropertyCache();

            if (bClear == true)
            {
                if (this.m_nChangedCount > 0)
                {
                    DialogResult result = this.TryGet(() =>
                    {
                        return MessageBox.Show(this,
                        "当前命中记录列表中有 " + this.m_nChangedCount.ToString() + " 项修改尚未保存。\r\n\r\n是否继续操作?\r\n\r\n(Yes 清除，然后继续操作；No 放弃操作)",
                        "BiblioSearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    });
                    if (result == DialogResult.No)
                        return;
                }
                this.ClearListViewItems();

                ListViewUtil.ClearSortColumns(this.listView_records);
            }

            long lHitCount = 0;

            this.m_lHitCount = 0;
            this.m_lLoaded = 0;
            // _stop.HideProgress();

            this.LabelMessage = "";

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.Style = StopStyle.None;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在检索 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在检索 ...");

            this.EnableControlsInSearching(false);
            try
            {

                // string strBrowseStyle = "id,cols";
                string strOutputStyle = "";
                if (bOutputKeyID == true)
                {
                    strOutputStyle = "keyid";
                    // strBrowseStyle = "keyid,id,key,cols";
                }

                string strQueryXml = "";
                int nRet = (int)this.Invoke((Func<int>)(() =>
                    {
                        return dp2QueryControl1.BuildQueryXml(
            MaxSearchResultCount,
            "zh",
            out strQueryXml,
            out strError);
                    }));
                if (nRet == -1)
                    goto ERROR1;

                string strResultSetName = GetResultSetName(false);

                channel.Timeout = new TimeSpan(0, 5, 0);
                long lRet = channel.Search(looping.Progress,
                    strQueryXml,
                    strResultSetName,   // "default",
                    strOutputStyle,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lHitCount = lRet;

                this.m_lHitCount = lHitCount;

                this.LabelMessage = "检索共命中 " + lHitCount.ToString() + " 条书目记录";

                looping.Progress.SetProgressRange(0, lHitCount);
                looping.Progress.Style = StopStyle.EnableHalfStop;

                //long lStart = 0;
                //long lPerCount = Math.Min(50, lHitCount);
                //DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // bool bPushFillingBrowse = PushFillingBrowse;

                bool bOutputKeyCount = false;
                _outputKeyCount = bOutputKeyCount;
                _outputKeyID = bOutputKeyID;
                _query = null;
                long lTotalHitCount = lHitCount;

                nRet = LoadResultSet(
looping.Progress,
channel,
strResultSetName,
bOutputKeyCount,
bOutputKeyID,
bQuickLoad,
lHitCount,
0,
_query,
ref lTotalHitCount,
out strError);
                if (nRet == -1)
                    goto ERROR1;
#if REMOVED
                // 装入浏览格式
                if (lHitCount > 0)
                {
                    for (; ; )
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (looping.Stopped)
                        {
                            this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已装入 " + lStart.ToString() + " 条，用户中断...";
                            return;
                        }

                        looping.Progress.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                        bool bTempQuickLoad = bQuickLoad;

                        if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                            bTempQuickLoad = true;


                        string strBrowseStyle = "id,cols";
                        if (bTempQuickLoad == true)
                        {
                            if (bOutputKeyID == true)
                                strBrowseStyle = "keyid,id,key";
                            else
                                strBrowseStyle = "id";
                        }
                        else
                        {
                            // 
                            if (bOutputKeyID == true)
                                strBrowseStyle = "keyid,id,key,cols";
                            else
                                strBrowseStyle = "id,cols";
                        }

                        lRet = channel.GetSearchResult(
                            looping.Progress,
                            null,   // strResultSetName
                            lStart,
                            lPerCount,
                            strBrowseStyle,
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                        {
                            this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已装入 " + lStart.ToString() + " 条，" + strError;
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
                            DigitalPlatform.LibraryClient.localhost.Record searchresult = searchresults[i];

                            string[] cols = null;
                            if (bOutputKeyID == true)
                            {
                                // 输出keys
                                if (searchresult.Cols == null
                                    && bTempQuickLoad == false)
                                {
                                    strError = "要使用获取检索点功能，请将 dp2Library 应用服务器和 dp2Kernel 数据库内核升级到最新版本";
                                    goto ERROR1;
                                }
                                cols = new string[(searchresult.Cols == null ? 0 : searchresult.Cols.Length) + 1];
                                cols[0] = LibraryChannel.BuildDisplayKeyString(searchresult.Keys);
                                if (cols.Length > 1)
                                    Array.Copy(searchresult.Cols, 0, cols, 1, cols.Length - 1);
                            }
                            else
                            {
                                cols = searchresult.Cols;
                            }

                            if (bPushFillingBrowse == true)
                            {
                                if (bTempQuickLoad == true)
                                    Global.InsertNewLine(
                                        (ListView)this.listView_records,
                                        searchresult.Path,
                                        cols);
                                else
                                    Global.InsertNewLine(
                                        this.listView_records,
                                        searchresult.Path,
                                        cols);
                            }
                            else
                            {
                                if (bTempQuickLoad == true)
                                    Global.AppendNewLine(
                                        (ListView)this.listView_records,
                                        searchresult.Path,
                                        cols);
                                else
                                    Global.AppendNewLine(
                                        this.listView_records,
                                        searchresult.Path,
                                        cols);
                            }
                        }

                        lStart += searchresults.Length;
                        // lCount -= searchresults.Length;
                        if (lStart >= lHitCount || lPerCount <= 0)
                            break;

                        this.m_lLoaded = lStart;
                        looping.Progress.SetProgressValue(lStart);
                    }
                }

#endif

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
                this.LabelMessage = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已全部装入";
                return;
            }
            catch (InterruptException)
            {
                this.LabelMessage = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已装入 " + this.listView_records.Items.Count.ToString() + " 条，用户中断...";
                return;
            }
            catch (Exception ex)
            {
                strError = "检索出现异常: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                this.ReturnChannel(channel);

                this.EnableControlsInSearching(true);
            }
        ERROR1:
            this.MessageBoxShow(strError);
        }

        internal void SetTitle(string text)
        {
            this.TryInvoke(() =>
            {
                string title = "书目查询";
                if (this._dbType == "authority")
                    title = "规范查询";
                if (string.IsNullOrEmpty(text) == false)
                    this.Text = title + " " + text;
                else
                    this.Text = title;
            });
        }

        List<ItemQueryParam> m_queries = new List<ItemQueryParam>();
        int m_nQueryIndex = -1;

        void QueryToPanel(ItemQueryParam query)
        {
            this.TryInvoke(() =>
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
                try
                {
                    this.textBox_queryWord.Text = query.QueryWord;
                    this.checkedComboBox_biblioDbNames.Text = query.DbNames;
                    this.comboBox_from.Text = query.From;
                    this.comboBox_matchStyle.Text = query.MatchStyle;

                    if (this.m_nChangedCount > 0)
                    {
                        DialogResult result = MessageBox.Show(this,
                            "当前命中记录列表中有 " + this.listView_records.Items.Count.ToString() + " 项修改尚未保存。\r\n\r\n是否继续操作?\r\n\r\n(Yes 清除，然后继续操作；No 放弃操作)",
                            "BiblioSearchForm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.No)
                            return;
                    }
                    this.ClearListViewItems();

                    this.listView_records.BeginUpdate();
                    for (int i = 0; i < query.Items.Count; i++)
                    {
                        this.listView_records.Items.Add(query.Items[i]);
                    }
                    this.listView_records.EndUpdate();

                    this.m_bFirstColumnIsKey = query.FirstColumnIsKey;
                    this.ClearListViewPropertyCache();
                }
                finally
                {
                    this.Cursor = oldCursor;
                }
            });
        }

        ItemQueryParam PanelToQuery()
        {
            return (ItemQueryParam)this.Invoke((Func<ItemQueryParam>)(() =>
            {
                ItemQueryParam query = new ItemQueryParam();

                query.QueryWord = this.textBox_queryWord.Text;
                query.DbNames = this.checkedComboBox_biblioDbNames.Text;
                query.From = this.comboBox_from.Text;
                query.MatchStyle = this.comboBox_matchStyle.Text;
                query.FirstColumnIsKey = this.m_bFirstColumnIsKey;
                return query;
            }));
        }

        void PushQuery(ItemQueryParam query)
        {
            if (query == null)
                throw new Exception("query值不能为空");

            this.TryInvoke(() =>
            {
                // 截断尾部多余的
                if (this.m_nQueryIndex < this.m_queries.Count - 1)
                    this.m_queries.RemoveRange(this.m_nQueryIndex + 1, this.m_queries.Count - (this.m_nQueryIndex + 1));

                if (this.m_queries.Count > 100)
                {
                    int nDelta = this.m_queries.Count - 100;
                    this.m_queries.RemoveRange(0, nDelta);
                    if (this.m_nQueryIndex >= 0)
                    {
                        this.m_nQueryIndex -= nDelta;
                        Debug.Assert(this.m_nQueryIndex >= 0, "");
                    }
                }

                this.m_queries.Add(query);
                this.m_nQueryIndex++;
                SetQueryPrevNextState();
            });
        }

        ItemQueryParam PrevQuery()
        {
            return (ItemQueryParam)this.Invoke((Func<ItemQueryParam>)(() =>
            {

                if (this.m_queries.Count == 0)
                    return null;

                if (this.m_nQueryIndex <= 0)
                    return null;

                this.m_nQueryIndex--;
                ItemQueryParam query = this.m_queries[this.m_nQueryIndex];

                SetQueryPrevNextState();

                this.m_bFirstColumnIsKey = query.FirstColumnIsKey;
                this.ClearListViewPropertyCache();
                return query;
            }));
        }

        ItemQueryParam NextQuery()
        {
            if (this.m_queries.Count == 0)
                return null;

            if (this.m_nQueryIndex >= this.m_queries.Count - 1)
                return null;

            this.m_nQueryIndex++;
            ItemQueryParam query = this.m_queries[this.m_nQueryIndex];

            SetQueryPrevNextState();

            this.m_bFirstColumnIsKey = query.FirstColumnIsKey;
            this.ClearListViewPropertyCache();
            return query;
        }

        void SetQueryPrevNextState()
        {
            if (this.m_nQueryIndex < 0)
            {
                toolStripButton_nextQuery.Enabled = false;
                toolStripButton_prevQuery.Enabled = false;
                return;
            }

            if (this.m_nQueryIndex >= this.m_queries.Count - 1)
            {
                toolStripButton_nextQuery.Enabled = false;
            }
            else
                toolStripButton_nextQuery.Enabled = true;

            if (this.m_nQueryIndex <= 0)
            {
                toolStripButton_prevQuery.Enabled = false;
            }
            else
                toolStripButton_prevQuery.Enabled = true;
        }

        public static string GetBiblioFromStyle(string strCaptions)
        {
            return GetBiblioFromStyle("biblio", strCaptions);
        }

        public static string GetAuthorityFromStyle(string strCaptions)
        {
            return GetBiblioFromStyle("biblio", strCaptions);
        }

        // 
        // Exception:
        //     可能会抛出Exception异常
        /// <summary>
        /// 根据from名列表字符串得到from style列表字符串
        /// </summary>
        /// <param name="strDbType">数据库类型</param>
        /// <param name="strCaptions">检索途径名</param>
        /// <returns>style列表字符串</returns>
        public static string GetBiblioFromStyle(string strDbType,
            string strCaptions)
        {
            BiblioDbFromInfo[] infos = null;
            if (strDbType == "biblio")
            {
                if (Program.MainForm.BiblioDbFromInfos == null
                    || Program.MainForm.BiblioDbFromInfos.Length == 0)
                    throw new Exception("Program.MainForm.DbFromInfos 尚未初始化。这通常是因为刚进入内务时候初始化阶段出现错误导致的。请退出内务重新进入，并注意正确登录");

                Debug.Assert(Program.MainForm.BiblioDbFromInfos != null, "Program.MainForm.BiblioDbFromInfos 尚未初始化");

                infos = Program.MainForm.BiblioDbFromInfos;
            }
            if (strDbType == "authority")
            {
                if (Program.MainForm.AuthorityDbFromInfos == null)
                    throw new Exception("Program.MainForm.AuthorityDbFromInfos");

                Debug.Assert(Program.MainForm.AuthorityDbFromInfos != null, "Program.MainForm.AuthorityDbFromInfos 尚未初始化");

                infos = Program.MainForm.AuthorityDbFromInfos;
            }

            string strResult = "";

            string[] parts = strCaptions.Split(new char[] { ',' });
            for (int k = 0; k < parts.Length; k++)
            {
                string strCaption = parts[k].Trim();

                // 2009/9/23 
                // TODO: 是否可以直接使用\t后面的部分呢？
                // 规整一下caption字符串，切除后面可能有的\t部分
                int nRet = strCaption.IndexOf("\t");
                if (nRet != -1)
                    strCaption = strCaption.Substring(0, nRet).Trim();

                if (strCaption.ToLower() == "<all>"
                    || strCaption == "<全部>"
                    || String.IsNullOrEmpty(strCaption) == true)
                    return "<all>";

                foreach (BiblioDbFromInfo info in infos)
                {
                    if (strCaption == info.Caption)
                    {
                        if (string.IsNullOrEmpty(strResult) == false)
                            strResult += ",";
                        // strResult += GetDisplayStyle(info.Style, true);   // 注意，去掉 _ 和 __ 开头的那些，应该还剩下至少一个 style
                        strResult += GetDisplayStyle(info.Style, true, false);   // 注意，去掉 __ 开头的那些，应该还剩下至少一个 style。_ 开头的不要滤出
                    }
                }
            }

            return strResult;
        }

        // 过滤掉 _ 开头的那些style子串
        // parameters:
        //      bRemove2    是否滤除 __ 前缀的
        //      bRemove1    是否滤除 _ 前缀的
        static string GetDisplayStyle(string strStyles,
            bool bRemove2,
            bool bRemove1)
        {
            string[] parts = strStyles.Split(new char[] { ',' });
            List<string> results = new List<string>();
            foreach (string part in parts)
            {
                string strText = part.Trim();
                if (String.IsNullOrEmpty(strText) == true)
                    continue;

                if (strText[0] == '_')
                {
                    if (bRemove1 == true)
                    {
                        if (strText.Length >= 2 && /*strText[0] == '_' &&*/ strText[1] != '_')
                            continue;
#if NO
                        if (strText[0] == '_')
                            continue;
#endif
                        if (strText.Length == 1)
                            continue;
                    }

                    if (bRemove2 == true && strText.Length >= 2)
                    {
                        if (/*strText[0] == '_' && */ strText[1] == '_')
                            continue;
                    }
                }

                results.Add(strText);
            }

            return StringUtil.MakePathList(results, ",");
        }

        static string GetFirstQueryWord(string text)
        {
            return StringUtil.ParseTwoPart(text, "\r\n")[0];
        }

        // string _lastBrowseStyle = "";   // 最近一次 GetSearchResult() 所使用过的 browse style

        public async Task _doSearch(bool bOutputKeyCount,
            bool bOutputKeyID,
            ItemQueryParam input_query = null)
        {
            string strError = "";
            int nRet = 0;
            bool bDisplayClickableError = false;

            if (bOutputKeyCount == true
    && bOutputKeyID == true)
            {
                strError = "bOutputKeyCount 和 bOutputKeyID 不能同时为 true";
                goto ERROR1;
            }

            bool bQuickLoad = false;    // 是否快速装入
            bool bClear = true; // 是否清除浏览窗中已有的内容

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                bQuickLoad = true;

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                bClear = false;

            // 修改窗口标题
            //this.Text = "书目查询 " + this.textBox_queryWord.Text;
            this.SetTitle(GetFirstQueryWord(this.QueryWord));

            if (input_query != null)
            {
                QueryToPanel(input_query);
            }

            // 记忆下检索式
            this.m_bFirstColumnIsKey = bOutputKeyID;
            this.ClearListViewPropertyCache();

            ItemQueryParam query = PanelToQuery();
            PushQuery(query);

            if (bClear == true)
            {
                if (this.m_nChangedCount > 0)
                {
                    DialogResult result = this.TryGet(() =>
                    {
                        return MessageBox.Show(this,
                        "当前命中记录列表中有 " + this.listView_records.Items.Count.ToString() + " 项修改尚未保存。\r\n\r\n是否继续操作?\r\n\r\n(Yes 清除，然后继续操作；No 放弃操作)",
                        "BiblioSearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    });

                    if (result == DialogResult.No)
                        return;
                }
                this.ClearListViewItems();

                /*
                // 2008/11/22
                this.SortColumns.Clear();
                SortColumns.ClearColumnSortDisplay(this.listView_records.Columns);
                 * */
                ListViewUtil.ClearSortColumns(this.listView_records);

                this._browseTitleTable.Clear();
            }

            this.m_lHitCount = 0;
            this.m_lLoaded = 0;
            // _stop.HideProgress();

            this.LabelMessage = "";

#if NO
            LibraryChannel channel = this.GetChannel(".",
                ",",
                GetChannelStyle.GUI,
                "");  // ? "test:127.0.0.1"
#endif
            // zchannel --> ListViewItem
            Hashtable zchannelTable = new Hashtable();

            var first_query_word = GetFirstQueryWord(this.QueryWord);
            long lTotalHitCount = 0;
            bool multiline = this.MultiLine;

            LibraryChannel channel = this.GetChannel();
            var old_timeout = channel.Timeout;

            /*
            _stop.Style = StopStyle.None;
            _stop.OnStop += Stop_OnStop1;
            _stop.Initial("正在检索 '" + first_query_word + "' ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(Stop_OnStop1,
                "正在检索 '" + first_query_word + "' ...");

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
    + " 开始书目检索</div>");

            this.EnableControlsInSearching(false);
            try
            {
                string from = this.From;
                if (from == "")
                {
                    strError = "尚未选定检索途径";
                    goto ERROR1;
                }

                string strFromStyle = "";

                try
                {
                    strFromStyle = GetBiblioFromStyle(this._dbType, from);
                }
                catch (Exception ex)
                {
                    strError = "BiblioSearchForm GetBiblioFromStyle() exception: " + ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strError = "GetFromStyle()没有找到 '" + from + "' 对应的 style 字符串";
                    goto ERROR1;
                }

                // 注："null"只能在前端短暂存在，而内核是不认这个所谓的matchstyle的
                string strMatchStyle = GetCurrentMatchStyle(this.MatchStyle);

                if (this.QueryWord == "")
                {
                    if (strMatchStyle == "null")
                    {
                        this.QueryWord = "";

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
                        goto ERROR1;
                    }
                }

                // string strBrowseStyle = "id,cols";
                string strOutputStyle = "";
                if (bOutputKeyCount == true)
                {
                    strOutputStyle = "keycount";
                }
                else if (bOutputKeyID == true)
                {
                    strOutputStyle = "keyid";
                    // strBrowseStyle = "keyid,id,key,cols";
                }

                if (_idDesc)
                    strOutputStyle += ",desc";

                bool bNeedShareSearch = false;
                if (this.SearchShareBiblio == true
    && Program.MainForm != null && Program.MainForm.MessageHub != null
    && Program.MainForm.MessageHub.ShareBiblio == true
                    && bOutputKeyCount == false
                    && bOutputKeyID == false)
                {
                    bNeedShareSearch = true;
                }

                List<string> query_words = new List<string>();
                {
                    query_words = StringUtil.SplitList(this.QueryWord.Replace("\r\n", "\r"), '\r');
                    StringUtil.RemoveBlank(ref query_words);
                    if (query_words.Count == 0 /*&& multiline == false*/)
                        query_words.Add("");
                    if (multiline == false)
                    {
                        if (query_words.Count > 1)
                            query_words.RemoveRange(1, query_words.Count - 1);
                    }
                }

                if (multiline/*query_words.Count > 1*/)
                {
                    looping.Progress.SetProgressRange(0, query_words.Count);
                }
                looping.Progress.Style = StopStyle.EnableHalfStop;

                bool zserver_loaded = false;

                int word_index = 0;
                foreach (string query_word in query_words)
                {
                    // Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        this.LabelMessage = "检索共命中 " + lTotalHitCount.ToString() + " 条书目记录，已装入 " + this.listView_records.Items.Count.ToString() + " 条，用户中断...";
                        return;
                    }

                    if (multiline/*query_words.Count > 1*/)
                    {
                        looping.Progress?.SetProgressValue(word_index);
                        looping.Progress?.SetMessage($"正在检索 '{query_word}' ({word_index + 1}/{query_words.Count})...");

                        this.ShowMessage($"正在检索 '{query_word}' ({word_index + 1}/{query_words.Count})...");
                    }
                    else
                    {
                        looping.Progress?.SetMessage($"正在检索 '{query_word}' ...");
                        this.ShowMessage($"正在检索 '{query_word}' ...");
                    }

                    word_index++;

                    if (bNeedShareSearch == true)
                    {
                        // 开始检索共享书目
                        // return:
                        //      -1  出错
                        //      0   没有检索目标
                        //      1   成功启动检索
                        nRet = BeginSearchShareBiblio(
                            query_word, // this.textBox_queryWord.Text,
                            strFromStyle,
                            strMatchStyle,
                            out strError);
                        if (nRet == -1)
                        {
                            // 显示错误信息
                            this.ShowMessage(strError, "red", true);
                            bDisplayClickableError = true;
                        }
                    }

                    string strResultSetName = GetResultSetName(multiline);

                    channel.Timeout = new TimeSpan(0, 15, 0);

                    int max_count = MaxSearchResultCount;
                    if (multiline)
                        max_count = MultilineMaxSearchResultCount;

                    long lRet = channel.SearchBiblio(looping.Progress,
                        this.DbNames,
                        query_word, // this.textBox_queryWord.Text,
                        max_count,  // this.MaxSearchResultCount,  // 1000
                        strFromStyle,
                        strMatchStyle,  // "left", TODO: "exact" 和 strSearchStyle "desc" 组合 dp2library 会抛出异常
                        this.Lang,
                        strResultSetName,
                        "",    // strSearchStyle
                        strOutputStyle,
                        this.GetLocationFilter(),
                        out string strQueryXml,
                        out strError);
                    if (lRet == -1)
                    {
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"针对检索词 '{query_word}' 进行检索时出错: {strError}") + "</div>");
                        goto ERROR1;
                    }

                    long lHitCount = lRet;

                    lTotalHitCount += lHitCount;

                    this.m_lHitCount = lHitCount;

                    if (multiline == false/*query_words.Count <= 1*/)
                        this.LabelMessage = "检索共命中 " + lHitCount.ToString() + " 条书目记录";
                    else
                        this.LabelMessage = "检索已累积命中 " + lTotalHitCount.ToString() + " 条书目记录";

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode($"'{query_word}' 命中 {lHitCount} 条") + "</div>");

                    if (multiline == false/*query_words.Count <= 1*/)
                    {
                        looping.Progress.SetProgressRange(0, lHitCount);
                        looping.Progress.Style = StopStyle.EnableHalfStop;
                    }

                    // bool bPushFillingBrowse = PushFillingBrowse;

                    _outputKeyCount = bOutputKeyCount;
                    _outputKeyID = bOutputKeyID;
                    _query = query;

                    nRet = LoadResultSet(
    looping.Progress,
    channel,
    strResultSetName,
    bOutputKeyCount,
    bOutputKeyID,
    bQuickLoad,
    lHitCount,
    0,
    query,
    ref lTotalHitCount,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;

#if REMOVED
                    if (lHitCount > 0)
                    {
                        string strBrowseStyle = GetBrowseStyle(bOutputKeyCount,
                            bOutputKeyID,
                            bQuickLoad);

                        _lastBrowseStyle = strBrowseStyle;

                        // TODO: 改短一点
                        channel.Timeout = TimeSpan.FromMinutes(15);
                        ResultSetLoader loader = new ResultSetLoader(channel,
    looping.Progress,
    null,
    strBrowseStyle);
                        if (multiline)
                            loader.MaxResultCount = MultilineMaxSearchResultCount;

                        loader.Prompt += new MessagePromptEventHandler(loader_Prompt);
                        loader.Getting += (o1, e1) =>
                        {
                            this.listView_records.EndUpdate();
                            this.m_lLoaded = e1.Start;

                            if (multiline == false/*query_words.Count <= 1*/)
                            {
                                looping.Progress.SetMessage("正在装入浏览信息 " + (e1.Start + 1).ToString() + " - " + (e1.Start + e1.PerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");
                                looping.Progress.SetProgressValue(e1.Start);
                            }

                            // 允许中途(通过感知 Shift 键)改变详略级别
                            loader.FormatList = GetBrowseStyle(bOutputKeyCount,
    bOutputKeyID,
    bQuickLoad);
                            _lastBrowseStyle = loader.FormatList;


                            Application.DoEvents(); // 出让界面控制权

                            if (looping.Stopped)
                                e1.Cancelled = true;
                        };
                        loader.Getted += (o1, e1) =>
                        {
                            this.listView_records.BeginUpdate();
                        };

                        this.listView_records.BeginUpdate();
                        try
                        {
                            int count = 0;
                            foreach (DigitalPlatform.LibraryClient.localhost.Record searchresult in loader)
                            {
                                // 调整命中数
                                var new_result_count = loader.ResultCount;
                                if (new_result_count > 0)
                                {
                                    var delta = new_result_count - lHitCount;
                                    lHitCount = new_result_count;
                                    this.m_lHitCount = lHitCount;
                                    lTotalHitCount += delta;
                                }

                                bool bTempQuickLoad = bQuickLoad;

                                if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                                    bTempQuickLoad = true;

                                ListViewItem item = null;

                                string[] cols = null;
                                if (bOutputKeyCount == true)
                                {
                                    // 输出keys
                                    if (searchresult.Cols == null)
                                    {
                                        strError = "要使用获取检索点功能，请将 dp2Library 应用服务器和 dp2Kernel 数据库内核升级到最新版本";
                                        goto ERROR1;
                                    }
                                    cols = new string[(searchresult.Cols == null ? 0 : searchresult.Cols.Length) + 1];
                                    cols[0] = searchresult.Path;
                                    if (cols.Length > 1)
                                        Array.Copy(searchresult.Cols, 0, cols, 1, cols.Length - 1);

                                    if (bPushFillingBrowse == true)
                                        item = Global.InsertNewLine(
                                            this.listView_records,
                                            "",
                                            cols);
                                    else
                                        item = Global.AppendNewLine(
                                            this.listView_records,
                                            "",
                                            cols);
                                    item.Tag = query;
                                    goto CONTINUE;
                                }
                                else if (bOutputKeyID == true)
                                {
                                    // 输出keys
                                    if (searchresult.Cols == null
                                        && bTempQuickLoad == false)
                                    {
                                        strError = "要使用获取检索点功能，请将 dp2Library 应用服务器和 dp2Kernel 数据库内核升级到最新版本";
                                        goto ERROR1;
                                    }

                                    cols = new string[(searchresult.Cols == null ? 0 : searchresult.Cols.Length) + 1];
                                    cols[0] = LibraryChannel.BuildDisplayKeyString(searchresult.Keys);
                                    if (cols.Length > 1)
                                        Array.Copy(searchresult.Cols, 0, cols, 1, cols.Length - 1);
                                }
                                else
                                {
                                    cols = searchresult.Cols;
                                }

                                if (bPushFillingBrowse == true)
                                {
                                    if (bTempQuickLoad == true)
                                        item = Global.InsertNewLine(
                                            (ListView)this.listView_records,
                                            searchresult.Path,
                                            cols);
                                    else
                                        item = Global.InsertNewLine(
                                            this.listView_records,
                                            searchresult.Path,
                                            cols);
                                }
                                else
                                {
                                    if (bTempQuickLoad == true)
                                        item = Global.AppendNewLine(
                                            (ListView)this.listView_records,
                                            searchresult.Path,
                                            cols);
                                    else
                                        item = Global.AppendNewLine(
                                            this.listView_records,
                                            searchresult.Path,
                                            cols);
                                }

                            CONTINUE:
                                query.Items.Add(item);
                                count++;
                            }

                            // lTotalHitCount += count;
                        }
                        finally
                        {
                            this.listView_records.EndUpdate();
                            loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                        }
                    }

#endif

                    // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);

                    if (this.SearchZ3950 && string.IsNullOrEmpty(query_word/*this.textBox_queryWord.Text*/) == false)
                    {
                        if (zserver_loaded == false)
                        {
                            nRet = Program.MainForm.LoadUseList(true, out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            string xmlFileName = Path.Combine(Program.MainForm.UserDir, "zserver.xml");
                            var result = _zsearcher.LoadServer(xmlFileName, Program.MainForm.Marc8Encoding);
                            if (result.Value == -1)
                            {
                                strError = result.ErrorInfo;
                                goto ERROR1;
                                // this.ShowMessage(result.ErrorInfo, "red", true);
                            }
                            zserver_loaded = true;
                        }

                        this.ShowMessage("等待 Z39.50 检索响应 ...");

                        {
                            if (multiline)
                            {
                                _zsearcher.PresentBatchSize = max_count;
                                // 2021/3/9
                                _zsearcher.ClearChannelsFetched();

                                // testing
                                // _zsearcher.Stop();
                            }
                            else
                                _zsearcher.PresentBatchSize = 10;   // 单行检索还是 10

                            NormalResult result = await _zsearcher.SearchAsync(
            Program.MainForm.UseList,   // UseCollection useList,
            Program.MainForm.IsbnSplitter,
                            query_word, // this.textBox_queryWord.Text,
                            max_count,  // this.MaxSearchResultCount,  // 1000
                            strFromStyle,
                            strMatchStyle,
                            (c, r) =>
                            {
                                this.Invoke((Action)(() =>
                                {
                                    ListViewItem item = new ListViewItem();
                                    item.Tag = c;
                                    zchannelTable[c] = item;
                                    this.listView_records.Items.Add(item);
                                    UpdateCommandLine(item, c, r);
                                }));

                                Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode($"'{query_word}' 命中 {r.ResultCount} 条 (Z39.50 : {c.ServerName})") + "</div>");
                            },
                            (c, r) =>
                            {
                                this.Invoke((Action)(() =>
                                {
                                    ListViewItem item = (ListViewItem)zchannelTable[c];
                                    if (r.Records != null)
                                        FillList(c.TargetInfo,
                                            c._total_fetched,
                                            c._fetched,
                                            c.ZClient.ForcedRecordsEncoding == null ? c.TargetInfo.DefaultRecordsEncoding : c.ZClient.ForcedRecordsEncoding,
                                            c.ServerName,
                                            r.Records,
                                            item);
                                    UpdateCommandLine(item, c, r);
                                    if (multiline)
                                        item.Tag = null;
                                }));
                            }
                            );

                            // TODO: 如果 result 出错要显示出来?
                        }
                    }

                    if (bNeedShareSearch == true)
                    {
                        // this.ShowMessage("等待共享检索响应 ...");
                        // 结束检索共享书目
                        // return:
                        //      -1  出错
                        //      >=0 命中记录个数
                        nRet = EndSearchShareBiblio(out strError);
                        if (nRet == -1)
                        {
                            // 显示错误信息
                            this.ShowMessage(strError, "red", true);
                            bDisplayClickableError = true;
                        }
                        else
                        {
                            if (_searchParam._searchCount > 0)
                            {
                                this.ShowMessage("共享书目命中 " + _searchParam._searchCount + " 条", "green");
                                this._floatingMessage.DelayClear(new TimeSpan(0, 0, 3));
#if NO
                            Application.DoEvents();
                            // TODO: 延时一段自动删除
                            Thread.Sleep(1000);
#endif
                            }
                        }

                        lHitCount += _searchParam._searchCount;
                    }

                } // end foreach

                if (lTotalHitCount == 0)
                {
                    if (query_words.Count == 0)
                        this.ShowMessage("没有检索词参与检索", "yellow", true);
                    else
                        this.ShowMessage("未命中", "yellow", true);
                    bDisplayClickableError = true;
                }

                if (lTotalHitCount == 0)
                {
                    if (query_words.Count == 0)
                        this.LabelMessage = "没有检索词参与检索";
                    else
                        this.LabelMessage = "未命中";
                }
                else
                    this.LabelMessage = "检索共命中 " + lTotalHitCount.ToString() + " 条书目记录，已全部装入";
            }
            catch (InterruptException)
            {
                this.LabelMessage = "检索共命中 " + lTotalHitCount.ToString() + " 条书目记录，已装入 " + this.listView_records.Items.Count.ToString() + " 条，用户中断...";
                return;
            }
            catch (Exception ex)
            {
                // 注: ExceptionUtil.GetExceptionText 可以处理好 AggregationException 的显示
                strError = "检索出现异常: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                if (bDisplayClickableError == false
                    && this._floatingMessage?.InDelay() == false)
                    this.ClearMessage();
                if (Program.MainForm.MessageHub != null)
                    Program.MainForm.MessageHub.SearchResponseEvent -= MessageHub_SearchResponseEvent;

                /*
                if (_stop != null)
                {
                    _stop.EndLoop();
                    _stop.OnStop -= Stop_OnStop1;
                    _stop.Initial("");
                    _stop.HideProgress();
                    _stop.Style = StopStyle.None;
                }
                */
                if (looping != null)
                    EndLoop(looping);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
                this.EnableControlsInSearching(true);

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
    + " 结束书目检索</div>");
            }

            return;

        ERROR1:
            try
            {
                this.MessageBoxShow(strError);
            }
            catch
            {

            }
        }

        // 存储全局结果集名
        string _resultSetName = "";

        string GetResultSetName(bool multiline_search)
        {
            if (multiline_search)
                return "multiline";
            if (string.IsNullOrEmpty(_resultSetName))
                _resultSetName = "#" + Guid.NewGuid().ToString();
            return _resultSetName;
        }

        string GetBrowseStyle(bool bOutputKeyCount,
            bool bOutputKeyID,
            bool bQuickLoad)
        {
            bool bTempQuickLoad = bQuickLoad;

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                bTempQuickLoad = true;

            string strBrowseStyle = "id,cols";
            if (bOutputKeyCount == true)
                strBrowseStyle = "keycount";
            else
            {
                if (bTempQuickLoad == true)
                {
                    if (bOutputKeyID == true)
                        strBrowseStyle = "keyid,id,key";
                    else
                        strBrowseStyle = "id";
                }
                else
                {
                    // 
                    if (bOutputKeyID == true)
                        strBrowseStyle = "keyid,id,key,cols";
                    else
                        strBrowseStyle = "id,cols";
                }
            }

            /*
            // testing
            if (_idDesc)
                strBrowseStyle += ",sort:-1,sortmaxcount:1000";
            else
                strBrowseStyle += ",sort:1,sortmaxcount:1000";
            */

            return strBrowseStyle;
        }

        bool _outputKeyCount = false;
        bool _outputKeyID = false;
        ItemQueryParam _query = null;

        int LoadResultSet(
            Stop stop,
            LibraryChannel channel,
            string strResultSetName,
            bool bOutputKeyCount,
            bool bOutputKeyID,
            bool bQuickLoad,
            long lHitCount,
            long lStart,
            ItemQueryParam query,
            ref long lTotalHitCount,
            out string strError)
        {
            strError = "";

            // 2023/3/29
            if (lHitCount == 0)
            {
                _query = null;
                return 0;
            }

            bool bPushFillingBrowse = PushFillingBrowse;
            bool multiline = this.MultiLine;

            string strBrowseStyle = GetBrowseStyle(
                bOutputKeyCount,
                bOutputKeyID,
                bQuickLoad);

            // _lastBrowseStyle = strBrowseStyle;

            // TODO: 改短一点
            channel.Timeout = TimeSpan.FromMinutes(15);
            ResultSetLoader loader = new ResultSetLoader(channel,
stop,
strResultSetName,
strBrowseStyle);
            if (multiline)
                loader.MaxResultCount = MultilineMaxSearchResultCount;

            loader.Start = lStart;

            loader.Prompt += new MessagePromptEventHandler(loader_Prompt);
            loader.Getting += (o1, e1) =>
            {
                EndUpdate();
                this.m_lLoaded = e1.Start;

                if (multiline == false/*query_words.Count <= 1*/)
                {
                    stop?.SetMessage("正在装入浏览信息 " + (e1.Start + 1).ToString() + " - " + (e1.Start + e1.PerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");
                    stop?.SetProgressValue(e1.Start);
                }

                // 允许中途(通过感知 Shift 键)改变详略级别
                loader.FormatList = GetBrowseStyle(bOutputKeyCount,
bOutputKeyID,
bQuickLoad);
                // _lastBrowseStyle = loader.FormatList;


                // Application.DoEvents(); // 出让界面控制权

                if (stop != null && stop.IsStopped)
                    e1.Cancelled = true;
            };
            loader.Getted += (o1, e1) =>
            {
                BeginUpdate();
            };

            BeginUpdate();
            try
            {
                int count = 0;
                bool adjusted = false;
                foreach (DigitalPlatform.LibraryClient.localhost.Record searchresult in loader)
                {
                    if (stop != null && stop.IsStopped)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    // 调整命中数
                    if (adjusted == false)
                    {
                        var new_result_count = loader.ResultCount;
                        if (new_result_count > 0)
                        {
                            var delta = new_result_count - lHitCount;
                            lHitCount = new_result_count;
                            this.m_lHitCount = lHitCount;
                            lTotalHitCount += delta;
                        }
                        adjusted = true;
                    }

                    bool bTempQuickLoad = bQuickLoad;

                    if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                        bTempQuickLoad = true;

                    ListViewItem item = null;

                    string[] cols = null;
                    if (bOutputKeyCount == true)
                    {
                        // 输出keys
                        /*
                        if (searchresult.Cols == null)
                        {
                            strError = "要使用获取检索点功能，请将 dp2Library 应用服务器和 dp2Kernel 数据库内核升级到最新版本";
                            return -1;
                        }
                        */
                        cols = new string[(searchresult.Cols == null ? 0 : searchresult.Cols.Length) + 1];
                        cols[0] = searchresult.Path;
                        if (cols.Length > 1)
                            Array.Copy(searchresult.Cols, 0, cols, 1, cols.Length - 1);

                        if (bPushFillingBrowse == true)
                            item = Global.InsertNewLine(
                                this.listView_records,
                                "",
                                cols);
                        else
                            item = Global.AppendNewLine(
                                this.listView_records,
                                "",
                                cols);
                        item.Tag = query;
                        goto CONTINUE;
                    }
                    else if (bOutputKeyID == true)
                    {
                        // 输出keys
                        /*
                        if (searchresult.Cols == null
                            && bTempQuickLoad == false)
                        {
                            strError = "要使用获取检索点功能，请将 dp2Library 应用服务器和 dp2Kernel 数据库内核升级到最新版本";
                            return -1;
                        }
                        */
                        cols = new string[(searchresult.Cols == null ? 0 : searchresult.Cols.Length) + 1];
                        cols[0] = LibraryChannel.BuildDisplayKeyString(searchresult.Keys);
                        if (cols.Length > 1)
                            Array.Copy(searchresult.Cols, 0, cols, 1, cols.Length - 1);
                    }
                    else
                    {
                        cols = searchresult.Cols;
                    }

                    if (bPushFillingBrowse == true)
                    {
                        if (bTempQuickLoad == true)
                            item = Global.InsertNewLine(
                                (ListView)this.listView_records,
                                searchresult.Path,
                                cols);
                        else
                            item = Global.InsertNewLine(
                                this.listView_records,
                                searchresult.Path,
                                cols);
                    }
                    else
                    {
                        if (bTempQuickLoad == true)
                            item = Global.AppendNewLine(
                                (ListView)this.listView_records,
                                searchresult.Path,
                                cols);
                        else
                            item = Global.AppendNewLine(
                                this.listView_records,
                                searchresult.Path,
                                cols);
                    }

                CONTINUE:
                    if (query != null)
                    {
                        Debug.Assert(query != null);
                        query.Items.Add(item);
                    }
                    count++;
                }

                // lTotalHitCount += count;
                _query = null;
                return 0;
            }
            finally
            {
                EndUpdate();
                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
            }
        }


        private void Stop_OnStop1(object sender, StopEventArgs e)
        {
            _zsearcher.Stop();
            this.DoStop(sender, e);
        }

        void BeginUpdate()
        {
            this.TryInvoke(() =>
            {
                this.listView_records.BeginUpdate();
            });
        }

        void EndUpdate()
        {
            this.TryInvoke(() =>
            {
                this.listView_records.EndUpdate();
            });
        }


#if NO
        void FillBrowse(DigitalPlatform.Z3950.RecordCollection records,
            ListViewItem insert_pos)
        {
            int index = insert_pos.ListView.Items.IndexOf(insert_pos);
            foreach (var record in records)
            {
                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, 0, "new");
                insert_pos.ListView.Items.Insert(index, item);
            }
        }
#endif

        Z3950Searcher _zsearcher = new Z3950Searcher();

        // (Z39.50)填入浏览记录
        void FillList(
            TargetInfo targetInfo,
            int base_start,
            int start,
            Encoding encoding,
            string strLibraryName,
            DigitalPlatform.Z3950.RecordCollection records,
            ListViewItem insert_pos)
        {
            // int index = insert_pos.ListView.Items.IndexOf(insert_pos);

            bool multiline = this.MultiLine;

            int i = 0;
            if (multiline)
            {
                // 2021/4/1
                // i 需要是一个全局的行索引
                // i = insert_pos.ListView.Items.IndexOf(insert_pos);
                i = base_start;
            }
            foreach (var record in records)
            {
                string strRecPath = $"{start + i + 1}@{strLibraryName}";

                if (string.IsNullOrEmpty(record.m_strDiagSetID) == false)
                {
                    this.Invoke((Action)(() =>
                    {
                        int index = insert_pos.ListView.Items.IndexOf(insert_pos);

                        var item = Global.InsertNewLine(
        this.listView_records,
        strRecPath,
        new string[] { $"错误代码: {record.m_nDiagCondition} 错误信息: {record.m_strAddInfo}" },
        index);
                        if (item != null)
                        {
                            item.ForeColor = Color.White;
                            item.BackColor = Color.DarkRed;
                        }
                    }
                    ));

                    goto CONTINUE;
                }

                // 把byte[]类型的MARC记录转换为机内格式
                // return:
                //		-2	MARC格式错
                //		-1	一般错误
                //		0	正常
                int nRet = MarcLoader.ConvertIso2709ToMarcString(record.m_baRecord,
                    encoding ?? Encoding.GetEncoding(936),
                    true,
                    out string strMARC,
                    out string strError);
                if (nRet == -1)
                {
                    AddErrorLine("记录 " + strRecPath + " 转换为 MARC 机内格式时出错: " + strError);
                    goto CONTINUE;
                }

                string strMarcSyntax = "";
                if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                    strMarcSyntax = "unimarc";
                else if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                    strMarcSyntax = "usmarc";

                // 2020/6/9
                // 自动探测 MARC 格式
                if (targetInfo != null && targetInfo.DetectMarcSyntax)
                {
                    string strSytaxOID = record.m_strSyntaxOID;

                    // return:
                    //		-1	无法探测
                    //		1	UNIMARC	规则：包含200字段
                    //		10	USMARC	规则：包含008字段(innopac的UNIMARC格式也有一个奇怪的008)
                    nRet = EntityForm.DetectMARCSyntax(strMARC);
                    if (nRet == 1)
                    {
                        strMarcSyntax = "unimarc";
                        strSytaxOID = "1.2.840.10003.5.1";
                    }
                    else if (nRet == 10)
                    {
                        strMarcSyntax = "usmarc";
                        strSytaxOID = "1.2.840.10003.5.10";
                    }

                    // 把自动识别的结果保存下来
                    record.AutoDetectedSyntaxOID = strSytaxOID;
                }

                nRet = MyForm.BuildMarcBrowseText(
        strMarcSyntax,
        strMARC,
        out string strBrowseText,
        out string strColumnTitles,
        out strError);
                if (nRet == -1)
                {
                    AddErrorLine("记录 " + strRecPath + " 创建浏览格式时出错: " + strError);
                    goto CONTINUE;
                }

                _browseTitleTable[strMarcSyntax] = strColumnTitles;

                // 将书目记录放入 m_biblioTable
                {
                    // TODO: MARC 格式转换为 XML 格式
                    nRet = MarcUtil.Marc2Xml(strMARC,
                        strMarcSyntax,
                        out string strXml,
                        out strError);
                    if (nRet == -1)
                    {
                        AddErrorLine("记录 " + strRecPath + " 转换为 XML 格式时出错: " + strError);
                        goto CONTINUE;
                    }

                    BiblioInfo info = new BiblioInfo
                    {
                        OldXml = strXml,
                        RecPath = strRecPath,
                        Timestamp = null,
                        Format = strMarcSyntax
                    };
                    lock (this.m_biblioTable)
                    {
                        this.m_biblioTable[strRecPath] = info;
                    }
                }

                List<string> column_list = StringUtil.SplitList(strBrowseText, '\t');
                string[] cols = new string[column_list.Count];
                column_list.CopyTo(cols);

                this.Invoke((Action)(() =>
                {
                    int index = insert_pos.ListView.Items.IndexOf(insert_pos);

                    ListViewItem item = null;
                    item = Global.InsertNewLine(
        this.listView_records,
        strRecPath,
        cols,
        index);  // index + i
                    if (item != null)
                        item.BackColor = Color.LightGreen;
                }
                ));

            CONTINUE:
                i++;
            }

            // Debug.Assert(e.Start == _searchParam._searchCount, "");
            return;
        }


        string GetLocationFilter()
        {
            string value = this.LocationFilter;
            if (value == "<不筛选>")
                return "";
            return value;
        }

        // 开始检索共享书目
        // return:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功启动检索
        int BeginSearchShareBiblio(
            string strQueryWord,
            string strFromStyle,
            string strMatchStyle,
            out string strError)
        {
            strError = "";

            if (Program.MainForm.MessageHub == null)
            {
                strError = "MessageHub is null";
                return -1;
            }

            string strSearchID = Guid.NewGuid().ToString();
            _searchParam = new SearchParam();
            _searchParam._looping = BeginLoop(this.DoStop, null);
            _searchParam._searchID = strSearchID;
            _searchParam._searchComplete = false;
            _searchParam._searchCount = 0;
            _searchParam._serverPushEncoding = "utf-7";
            Program.MainForm.MessageHub.SearchResponseEvent += MessageHub_SearchResponseEvent;

            string strOutputSearchID = "";
            int nRet = Program.MainForm.MessageHub.BeginSearchBiblio(
                "*",
                new SearchRequest(strSearchID,
                    new LoginInfo("public", false),
                "searchBiblio",
                "<全部>",
        strQueryWord,
        strFromStyle,
        strMatchStyle,
        "",
        "id,xml",
        1000,
        0,
        -1,
        _searchParam._serverPushEncoding),
        out strOutputSearchID,
        out strError);
            if (nRet == -1)
            {
                // 检索过程结束
                _searchParam._searchComplete = true;
                return -1;
            }
            if (nRet == 0)
            {
                // 检索过程结束
                _searchParam._searchComplete = true;
                return 0;
            }

            if (_searchParam._manager.SetTargetCount(nRet) == true)
                _searchParam._searchComplete = true;

            return 1;
        }

        // 结束检索共享书目
        // return:
        //      -1  出错
        //      >=0 命中记录个数
        int EndSearchShareBiblio(out string strError)
        {
            strError = "";

            try
            {
                // 装入浏览记录
                TimeSpan timeout = new TimeSpan(0, 1, 0);
                DateTime start_time = DateTime.Now;
                while (_searchParam._searchComplete == false)
                {
                    var length = timeout - (DateTime.Now - start_time);
                    this.ShowMessage($"正在共享检索...\r\n已命中:{_searchParam._searchCount}  剩余秒数:{((int)length.TotalSeconds).ToString()}");

                    // Application.DoEvents();
                    Thread.Sleep(200);
                    if (DateTime.Now - start_time > timeout)    // 超时
                        break;
                    if (_searchParam._looping.Stopped)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }

                return _searchParam._searchCount;
            }
            finally
            {
                _searchParam._searchID = "";
                _searchParam._looping?.Dispose();
            }
        }

        class SearchParam
        {
            // 2022/11/5
            public Looping _looping = null;

            public string _searchID = "";

            public bool _searchComplete = false;
            public int _searchCount = 0;

            public string _serverPushEncoding = "";

            public ResultManager _manager = new ResultManager();

#if NO
            public Hashtable LibraryNameTable = new Hashtable();    //  图书馆 UID --> 图书馆名字

            // 将图书馆名和 UID 的对照关系记载下来
            public void SetLibraryNameTable(string strLongPath)
            {
                List<string> array1 = StringUtil.ParseTwoPart(strLongPath, "@");
                List<string> array2 = StringUtil.ParseTwoPart(array1[1], "|");
                string strLibraryName = array2[0];
                string strLibraryUID = array2[1];
                if (string.IsNullOrEmpty(strLibraryName) == false
                    && string.IsNullOrEmpty(strLibraryUID) == false)
                    this.LibraryNameTable[strLibraryUID] = strLibraryName;
            }

            // 将路径中的图书馆 UID 部分替换为图书馆名字
            public string BuildNamePath(string strLongPath)
            {
                List<string> array1 = StringUtil.ParseTwoPart(strLongPath, "@");
                string strShortPath = array1[0];
                List<string> array2 = StringUtil.ParseTwoPart(array1[1], "|");
                string strLibraryName = array2[0];
                if (string.IsNullOrEmpty(strLibraryName) == false)
                    return strShortPath + "@" + strLibraryName;

                string strLibraryUID = array2[1];
                strLibraryName = (string)this.LibraryNameTable[strLibraryUID];
                if (string.IsNullOrEmpty(strLibraryName) == false)
                    return strShortPath + "@" + strLibraryName;
                else
                    return strLongPath; // 实在不行还是维持原样
            }
#endif
        }

        SearchParam _searchParam = null;

        // 外来数据的浏览列标题的对照表。MARC 格式名 --> 列标题字符串
        Hashtable _browseTitleTable = new Hashtable();

        void MessageHub_SearchResponseEvent(object sender, SearchResponseEventArgs e)
        {
            if (e.TaskID != _searchParam._searchID)
                return;

            if (e.ResultCount == -1 && e.Start == -1)
            {
                // 检索过程结束
                _searchParam._searchComplete = true;
                _searchParam._searchCount = (int)_searchParam._manager.GetTotalCount();
                return;
            }
            string strError = "";

            // _searchParam.SetLibraryNameTable("@" + e.LibraryUID);
            List<string> array = StringUtil.ParseTwoPart(e.LibraryUID, "|");
            string strLibraryName = array[0];

            // 标记结束一个检索目标
            // return:
            //      0   尚未结束
            //      1   结束
            //      2   全部结束
            int nRet = _searchParam._manager.CompleteTarget(e.LibraryUID,
                e.ResultCount,
                e.Records == null ? 0 : e.Records.Count);

            _searchParam._searchCount = (int)_searchParam._manager.GetTotalCount();

            if (nRet == 2)
                _searchParam._searchComplete = true;

            if (e.ResultCount == -1)
            {
                strError = e.ErrorInfo;
                goto ERROR1;
            }

#if NO
            if (e.Records != null)
                _searchParam._searchCount += e.Records.Count;
#endif

            // 单独给一个线程来执行
            Task.Factory.StartNew(() => FillList(e.Start, strLibraryName, e.Records));
            return;
        ERROR1:
            AddErrorLine(strError);
        }

        // 加入一个错误文本行
        void AddErrorLine(string strError)
        {
            string[] cols = new string[1];
            cols[0] = strError;
            this.Invoke((Action)(() =>
            {
                ListViewItem item = Global.AppendNewLine(
        this.listView_records,
        "error",
        cols);
            }
        ));
        }

        void FillList(long lStart,
            string strLibraryName,
            IList<DigitalPlatform.MessageClient.Record> Records)
        {
            string strError = "";

            // lock (_searchParam)
            {
                // TODO: 注意来自共享网络的图书馆名不能和 servers.xml 中的名字冲突。另外需要检查，不同的 UID，图书馆名字不能相同，如果发生冲突，则需要给分配 ..1 ..2 这样的编号以示区别
                // 需要一直保存一个 UID 到图书馆命的对照表在内存备用
                // TODO: 来自共享网络的记录，图标或 @ 后面的名字应该有明显的形态区别
                foreach (DigitalPlatform.MessageClient.Record record in Records)
                {
                    MessageHub.DecodeRecord(record, _searchParam._serverPushEncoding);

                    string strRecPath = record.RecPath + "@" + strLibraryName;

                    // 校验一下 MD5
                    if (string.IsNullOrEmpty(record.MD5) == false)
                    {
                        string strMD5 = StringUtil.GetMd5(record.Data);
                        if (record.MD5 != strMD5)
                        {
                            strError = "dp2Circulation : 记录 '" + strRecPath + "' Data 的 MD5 校验出现异常";
                            AddErrorLine(strError);
                            continue;
                        }
                    }

                    string strXml = record.Data;

                    int nRet = BuildBrowseText(strXml,
        out string strBrowseText,
        out string strMarcSyntax,
        out string strColumnTitles,
        out strError);
                    if (nRet == -1)
                    {
                        AddErrorLine("记录 " + strRecPath + " 创建浏览格式时出: " + strError);
                        continue;
                    }

#if NO
                    // string strRecPath = record.RecPath + "@" + (string.IsNullOrEmpty(record.LibraryName) == false ? record.LibraryName : record.LibraryUID);
                    string strRecPath = // "lStart="+lStart.ToString() + " " + i + "/"+ Records.Count + " " +
                        _searchParam.BuildNamePath(record.RecPath);
#endif

#if NO
                string strDbName = ListViewProperty.GetDbName(strRecPath);
                _browseTitleTable[strDbName] = strColumnTitles;
#endif
                    _browseTitleTable[strMarcSyntax] = strColumnTitles;

                    // 将书目记录放入 m_biblioTable
                    {
                        BiblioInfo info = new BiblioInfo();
                        info.OldXml = strXml;
                        info.RecPath = strRecPath;
                        info.Timestamp = ByteArray.GetTimeStampByteArray(record.Timestamp);
                        info.Format = strMarcSyntax;
                        lock (this.m_biblioTable)
                        {
                            this.m_biblioTable[strRecPath] = info;
                        }
                    }

                    List<string> column_list = StringUtil.SplitList(strBrowseText, '\t');
                    string[] cols = new string[column_list.Count];
                    column_list.CopyTo(cols);

                    ListViewItem item = null;
                    this.Invoke((Action)(() =>
                    {
                        item = Global.AppendNewLine(
        this.listView_records,
        strRecPath,
        cols);
                    }
                    ));

                    if (item != null)
                        item.BackColor = Color.LightGreen;

#if NO
                RegisterBiblioInfo info = new RegisterBiblioInfo();
                info.OldXml = strXml;   // strMARC;
                info.Timestamp = ByteArray.GetTimeStampByteArray(record.Timestamp);
                info.RecPath = record.RecPath + "@" + (string.IsNullOrEmpty(record.LibraryName) == false ? record.LibraryName : record.LibraryUID);
                info.MarcSyntax = strMarcSyntax;
#endif
                }

                // Debug.Assert(e.Start == _searchParam._searchCount, "");
                return;
            }

            return;
#if NO
        ERROR1:
            AddErrorLine(strError);
#endif
        }

        // 2016/12/16
        // 新开一个 EntityForm
        EntityForm OpenEntityForm(bool bAuto, bool bFixed)
        {
            EntityForm form = null;
            EntityForm exist_fixed = Program.MainForm.FixedEntityForm;

            if (bFixed == true && exist_fixed != null)
                form = exist_fixed;
            else
            {
                if (bAuto == true && this.LoadToExistDetailWindow == true)
                    form = MainForm.GetTopChildWindow<EntityForm>();
            }

            if (form != null)
                Global.Activate(form);
            else
            {
                form = new EntityForm();

                form.MdiParent = Program.MainForm;
                form.MainForm = Program.MainForm;
                if (bFixed)
                {
                    form.Fixed = true;
                    form.SuppressSizeSetting = true;
                    Program.MainForm.SetMdiToNormal();
                }
                else
                {
                    // 在已经有左侧窗口的情况下，普通窗口需要显示在右侧
                    if (exist_fixed != null)
                    {
                        form.SuppressSizeSetting = true;
                        Program.MainForm.SetMdiToNormal();
                    }
                }

                form.Show();
                if (bFixed)
                    Program.MainForm.SetFixedPosition(form, "left");
                else
                {
                    // 在已经有左侧窗口的情况下，普通窗口需要显示在右侧
                    if (exist_fixed != null)
                    {
                        Program.MainForm.SetFixedPosition(form, "right");
                    }
                }
            }

            Debug.Assert(form != null, "");
            return form;
        }

        AuthorityForm OpenAuthorityForm(bool bAuto, bool bFixed)
        {
            AuthorityForm form = null;
            AuthorityForm exist_fixed = null;   // Program.MainForm.FixedEntityForm;

            if (bFixed == true && exist_fixed != null)
                form = exist_fixed;
            else
            {
                if (bAuto == true && this.LoadToExistDetailWindow == true)
                    form = MainForm.GetTopChildWindow<AuthorityForm>();
            }

            if (form != null)
                Global.Activate(form);
            else
            {
                form = new AuthorityForm();

                form.MdiParent = Program.MainForm;
                form.MainForm = Program.MainForm;
                if (bFixed)
                {
                    form.Fixed = true;
                    form.SuppressSizeSetting = true;
                    Program.MainForm.SetMdiToNormal();
                }
                else
                {
                    // 在已经有左侧窗口的情况下，普通窗口需要显示在右侧
                    if (exist_fixed != null)
                    {
                        form.SuppressSizeSetting = true;
                        Program.MainForm.SetMdiToNormal();
                    }
                }

                form.Show();
                if (bFixed)
                    Program.MainForm.SetFixedPosition(form, "left");
                else
                {
                    // 在已经有左侧窗口的情况下，普通窗口需要显示在右侧
                    if (exist_fixed != null)
                    {
                        Program.MainForm.SetFixedPosition(form, "right");
                    }
                }
            }

            Debug.Assert(form != null, "");
            return form;
        }

        string GetRelativeEntityFormName()
        {
            if (this._dbType == "biblio")
                return "种册窗";
            return "规范窗";
        }

        public static bool IsCmdLine(string strFirstColumn)
        {
            return strFirstColumn.StartsWith("Z39.50:");
        }

        private async void listView_records_DoubleClick(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要装入" + this.GetRelativeEntityFormName() + "的事项";
                goto ERROR1;
            }

            string strPath = this.listView_records.SelectedItems[0].SubItems[0].Text;

            if (IsCmdLine(strPath))
            {
                menu_loadNextBatch_Click(sender, e);
                return;
            }

            try
            {
                if (String.IsNullOrEmpty(strPath) == false)
                {
                    if (this._dbType == "biblio")
                    {
                        EntityForm form = OpenEntityForm(true, false);
                        Debug.Assert(form != null, "");

                        if (strPath.IndexOf("@") == -1)
                        {
                            form.CloseBrowseWindow();
                            // form.LoadRecordOld(strPath, "", true);
                            await form.LoadRecordAsync(strPath, "", true);
                        }
                        else
                        {
#if NO
                        // TODO: 可以允许在 BiblioSearchForm 中被修改过、尚未保存的书目记录装入种册窗进行继续修改
                        if (this.m_biblioTable == null)
                        {
                            strError = "m_biblioTable == null";
                            goto ERROR1;
                        }

                        BiblioInfo info = this.m_biblioTable[strPath] as BiblioInfo;
                        if (info == null)
                        {
                            strError = "m_biblioTable 中不存在 key 为 " + strPath + " 的 BiblioInfo 事项";
                            goto ERROR1;
                        }
#endif
                            var info = GetBiblioInfo(strPath);
                            if (info == null)
                            {
                                strError = "m_biblioTable 中不存在 key 为 " + strPath + " 的 BiblioInfo 事项";
                                goto ERROR1;
                            }

                            form.CloseBrowseWindow();
                            /*
                            int nRet = form.LoadRecord(info,
                                true,
                                out strError);
                            if (nRet != 1)
                                goto ERROR1;
                            */
                            var result = await form.LoadRecordAsync(info,
    true);
                            if (result.Value != 1)
                            {
                                strError = result.ErrorInfo;
                                goto ERROR1;
                            }
                        }
                    }
                    else
                    {
                        AuthorityForm form = OpenAuthorityForm(true, false);
                        Debug.Assert(form != null, "");

                        if (strPath.IndexOf("@") == -1)
                        {
                            // form.LoadRecordOld(strPath, "", true);
                            await form.LoadRecordAsync(strPath, "", true);
                        }
                        else
                        {
#if NO
                        // TODO: 可以允许在 BiblioSearchForm 中被修改过、尚未保存的书目记录装入种册窗进行继续修改
                        if (this.m_biblioTable == null)
                        {
                            strError = "m_biblioTable == null";
                            goto ERROR1;
                        }

                        BiblioInfo info = this.m_biblioTable[strPath] as BiblioInfo;
                        if (info == null)
                        {
                            strError = "m_biblioTable 中不存在 key 为 " + strPath + " 的 BiblioInfo 事项";
                            goto ERROR1;
                        }
#endif
                            var info = GetBiblioInfo(strPath);
                            if (info == null)
                            {
                                strError = "m_biblioTable 中不存在 key 为 " + strPath + " 的 BiblioInfo 事项";
                                goto ERROR1;
                            }

                            /*
                            int nRet = form.LoadRecord(info,
                                true,
                                out strError);
                            if (nRet != 1)
                                goto ERROR1;
                            */
                            var result = await form.LoadRecordAsync(info,
                                true);
                            if (result.Value != 1)
                            {
                                strError = result.ErrorInfo;
                                goto ERROR1;
                            }
                        }
                    }
                }
                else
                {
                    ItemQueryParam query = (ItemQueryParam)this.listView_records.SelectedItems[0].Tag;
                    Debug.Assert(query != null, "");

                    this.QueryWord = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);
                    if (query != null)
                    {
                        this.DbNames = query.DbNames;
                        this.From = query.From;
                    }

                    if (this.QueryWord == "")
                        this.MatchStyle = "空值";
                    else
                        this.MatchStyle = "精确一致";

                    await DoSearch(false, false, null);
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public BiblioInfo GetBiblioInfo(string strPath)
        {
            string strError = "";
            // TODO: 可以允许在 BiblioSearchForm 中被修改过、尚未保存的书目记录装入种册窗进行继续修改
            if (this.m_biblioTable == null)
            {
                strError = "m_biblioTable == null";
                throw new Exception(strError);
            }

            return this.m_biblioTable[strPath] as BiblioInfo;
            /*
            if (info == null)
            {
                strError = "m_biblioTable 中不存在 key 为 " + strPath + " 的 BiblioInfo 事项";
                goto ERROR1;
            }
            */
        }

        // 装入左侧固定的种册窗
        async void menu_loadToLeftEntityForm_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                this.MessageBoxShow("尚未选定要装入左侧实体窗的事项");
                return;
            }
            string strPath = this.listView_records.SelectedItems[0].SubItems[0].Text;

            EntityForm form = OpenEntityForm(false, true);    // fixed

            Debug.Assert(form != null, "");
            await form.LoadRecordOldAsync(strPath, "", true);
        }

        async void menu_loadToOpenedEntityForm_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                this.MessageBoxShow("尚未选定要装入实体窗口的事项");
                return;
            }
            string strPath = this.listView_records.SelectedItems[0].SubItems[0].Text;

            EntityForm form = null;

            form = MainForm.GetTopChildWindow<EntityForm>();
            if (form != null)
                Global.Activate(form);

            if (form == null)
            {
                form = OpenEntityForm(false, false);    // 新开一个窗口，普通窗口(不是左侧)
            }

            Debug.Assert(form != null, "");

            await form.LoadRecordOldAsync(strPath, "", true);
        }

        async void menu_loadToNewEntityForm_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装入实体窗口的事项");
                return;
            }
            string strPath = this.listView_records.SelectedItems[0].SubItems[0].Text;

            EntityForm form = null;

            if (form == null)
            {
                form = OpenEntityForm(false, false);    // 新开一个窗口，普通窗口(不是左侧)
            }

            Debug.Assert(form != null, "");

            await form.LoadRecordOldAsync(strPath, "", true);
        }

        // 是否优先装入已经打开的详细窗?
        /// <summary>
        /// 是否优先装入已经打开的详细窗?
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

        int _enableSearchingLevel = 0;

        bool NeedUpdateSearchingEnable(bool bEnable)
        {
            if (bEnable == false)
            {
                _enableSearchingLevel++;
                Debug.Assert(_enableSearchingLevel >= 0);
                if (_enableSearchingLevel == 1)
                    return true;
            }
            else
            {
                _enableSearchingLevel--;
                Debug.Assert(_enableSearchingLevel >= 0);
                if (_enableSearchingLevel == 0)
                    return true;
            }
            return false;
        }

        // 包括listview
        public override void EnableControls(bool bEnable)
        {
            EnableControlsInSearching(bEnable);

            {
                if (NeedUpdateEnable(bEnable) == false)
                    return;

                this.TryInvoke((Action)(() =>
                {
                    this.listView_records.Enabled = bEnable;
                }));
            }
        }

        // 注: listview除外
        void EnableControlsInSearching(bool bEnable)
        {
            if (NeedUpdateSearchingEnable(bEnable) == false)
                return;

            this.TryInvoke((Action)(() =>
            {
                UpdateSearchingEnable(bEnable);
            }));
        }

        void UpdateSearchingEnable(bool bEnable)
        {
            // this.button_search.Enabled = bEnable;
            this.toolStrip_search.Enabled = bEnable;

            this.comboBox_from.Enabled = bEnable;
            this.checkedComboBox_biblioDbNames.Enabled = bEnable;
            this.comboBox_matchStyle.Enabled = bEnable;

            this.comboBox_location.Enabled = bEnable;

            if (this.comboBox_matchStyle.Text == "空值")
                this.textBox_queryWord.Enabled = false;
            else
                this.textBox_queryWord.Enabled = bEnable;

            if (this.m_lHitCount <= this.listView_records.Items.Count)
                this.ToolStripMenuItem_continueLoad.Enabled = false;
            else
                this.ToolStripMenuItem_continueLoad.Enabled = true;

            this.dp2QueryControl1.Enabled = bEnable;
        }


        bool InSearching
        {
            get
            {
                return (bool)this.Invoke((Func<bool>)(() =>
                {
                    if (this.comboBox_from.Enabled == true)
                        return false;
                    return true;
                }));
            }
        }


        private void BiblioSearchForm_Activated(object sender, EventArgs e)
        {
            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;

            RefreshPropertyView(false);
        }

        private void button_viewBiblioDbProperty_Click(object sender, EventArgs e)
        {

        }

        // 书目库属性
        private void MenuItem_viewBiblioDbProperty_Click(object sender, EventArgs e)
        {
            HtmlViewerForm dlg = new HtmlViewerForm();
            MainForm.SetControlFont(dlg, this.Font, false);

            string strText = "<html><body>";

            // Debug.Assert(false, "");

            if (Program.MainForm.BiblioDbProperties != null)
            {
                foreach (var property in Program.MainForm.BiblioDbProperties)
                {
                    // BiblioDbProperty property = Program.MainForm.BiblioDbProperties[i];

                    strText += "<p>书目库名: " + property.DbName + "; 语法: " + property.Syntax + "</p>";
                }
            }

            strText += "</body></html>";

            dlg.HtmlString = strText;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog();   // ? this
        }

        private void checkedComboBox_biblioDbNames_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_biblioDbNames.Items.Count > 0)
                return;


            if (this._dbType == "biblio")
            {
                if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.6") >= 0)
                    this.checkedComboBox_biblioDbNames.Items.Add("<全部书目>");
                else
                    this.checkedComboBox_biblioDbNames.Items.Add("<全部>");

                if (Program.MainForm.BiblioDbProperties != null)
                {
                    foreach (BiblioDbProperty property in Program.MainForm.BiblioDbProperties)
                    {
                        this.checkedComboBox_biblioDbNames.Items.Add(property.DbName);
                    }
                }
            }

            if (this._dbType == "authority")
            {
                this.checkedComboBox_biblioDbNames.Items.Add("<全部规范>");
                if (Program.MainForm.AuthorityDbProperties != null)
                {
                    foreach (BiblioDbProperty property in Program.MainForm.AuthorityDbProperties)
                    {
                        this.checkedComboBox_biblioDbNames.Items.Add(property.DbName);
                    }
                }
            }
        }

        // 浏览行 为命令时候，应当出现的弹出菜单
        private void CommandPopupMenu(object sender, MouseEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSelectedItemCount = this.listView_records.SelectedItems.Count;
            string strFirstColumn = "";
            if (nSelectedItemCount > 0)
            {
                strFirstColumn = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);
            }

            menuItem = new MenuItem("载入下一批浏览行(&N)");
            menuItem.DefaultItem = true;
            menuItem.Click += new System.EventHandler(this.menu_loadNextBatch_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("载入余下全部浏览行(&A)");
            menuItem.Click += new System.EventHandler(this.menu_loadRestAllBatch_Click);
            contextMenu.MenuItems.Add(menuItem);

#if NO
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);
#endif
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyToClipboard_Click);
            if (this.listView_records.SelectedIndices.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("复制单列(&S)");
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

            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
        }

        async Task LoadNextBatch(bool all)
        {
            if (this.listView_records.SelectedItems.Count != 1)
                return;

            _zsearcher.InSearching = true;
            this.EnableControlsInSearching(false);
            /*
            _stop.OnStop += OnZ3950LoadStop;
            _stop.Initial("正在装载 Z39.50 检索内容 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(OnZ3950LoadStop, "正在装载 Z39.50 检索内容 ...");

            try
            {
                ListViewItem item = this.listView_records.SelectedItems[0];
                ZClientChannel channel = (ZClientChannel)item.Tag;

                if (channel == null)
                {
                    this.ShowMessage("无法载入下一批记录(多行模式)", "yellow", true);
                    return;
                }

                if (channel._fetched >= channel._resultCount)
                {
                    this.ShowMessage("已经全部载入", "yellow", true);
                    return;
                }

                while (channel._fetched < channel._resultCount)
                {
                    if (_zsearcher.InSearching == false)
                        break;

                    looping.Progress.SetMessage($"正在装载 Z39.50 检索内容({channel._fetched}-) ...");

                    var present_result = await Z3950Searcher.FetchRecords(channel,
                        all ? 50 : 10);

                    {
                        if (present_result.Records != null)
                            FillList(channel.TargetInfo,
                                channel._total_fetched,
                                channel._fetched,
                                channel.ZClient.ForcedRecordsEncoding == null ? channel.TargetInfo.DefaultRecordsEncoding : channel.ZClient.ForcedRecordsEncoding,
                                channel.ServerName,
                                present_result.Records,
                                item);
                        UpdateCommandLine(item, channel, present_result);
                    }

                    if (present_result.Value == -1)
                        break;
                    else
                        channel._fetched += present_result.Records.Count;

                    if (all == false)
                        break;
                }
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= OnZ3950LoadStop;
                _stop.Initial("");
                _stop.HideProgress();
                */
                EndLoop(looping);

                this.EnableControlsInSearching(true);
                _zsearcher.InSearching = false;
            }
        }

        private void OnZ3950LoadStop(object sender, StopEventArgs e)
        {
            _zsearcher.Stop();
        }

        async void menu_loadNextBatch_Click(object sender, EventArgs e)
        {
            await LoadNextBatch(false);
        }

        public static void UpdateCommandLine(ListViewItem item,
        ZClientChannel c,
        DigitalPlatform.Z3950.ZClient.SearchResult r)
        {
            item.BackColor = Color.Yellow;
            ListViewUtil.ChangeItemText(item, 0, $"Z39.50:{c.ServerName}");
            if (r.Value == -1 || r.Value == 0)
                ListViewUtil.ChangeItemText(item, 1, $"{r.Query} 检索出错 {r.ErrorInfo}");
            else
                ListViewUtil.ChangeItemText(item, 1, $"{r.Query} 检索命中 {r.ResultCount} 条");
        }

        public static void UpdateCommandLine(ListViewItem item,
            ZClientChannel c,
            DigitalPlatform.Z3950.ZClient.PresentResult r)
        {
            if (r.Value == -1)
                ListViewUtil.ChangeItemText(item, 1, $"{c._query} Present 出错 {r.ErrorInfo}");
            else
                ListViewUtil.ChangeItemText(item, 1, $"{c._query} 检索命中 {c._resultCount} 条，已装入 {c._fetched + r.Records.Count}");
        }

        async void menu_loadRestAllBatch_Click(object sender, EventArgs e)
        {
            await LoadNextBatch(true);
        }

        // listview上的右鼠标键菜单
        private void listView_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSelectedItemCount = this.listView_records.SelectedItems.Count;
            string strFirstColumn = "";
            if (nSelectedItemCount > 0)
            {
                strFirstColumn = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);
            }

            if (nSelectedItemCount == 1 && IsCmdLine(strFirstColumn))
            {
                CommandPopupMenu(sender, e);
                return;
            }

            menuItem = new MenuItem("装入已打开的种册窗(&E)");
            if (this.LoadToExistDetailWindow == true
                && Program.MainForm.GetTopChildWindow<EntityForm>() != null)
                menuItem.DefaultItem = true;
            menuItem.Click += new System.EventHandler(this.menu_loadToOpenedEntityForm_Click);
            if (this.listView_records.SelectedItems.Count == 0
                || Program.MainForm.GetTopChildWindow<EntityForm>() == null
                || String.IsNullOrEmpty(strFirstColumn) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入新开的种册窗(&N)");
            if (this.LoadToExistDetailWindow == false
                || Program.MainForm.GetTopChildWindow<EntityForm>() == null)
                menuItem.DefaultItem = true;
            menuItem.Click += new System.EventHandler(this.menu_loadToNewEntityForm_Click);
            if (this.listView_records.SelectedItems.Count == 0
                || String.IsNullOrEmpty(strFirstColumn) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入左侧固定种册窗(&L)");
            menuItem.Click += new System.EventHandler(this.menu_loadToLeftEntityForm_Click);
            if (this.listView_records.SelectedItems.Count == 0
                || String.IsNullOrEmpty(strFirstColumn) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("停靠 (&D)");
            if (this.Docked)
                menuItem.Checked = true;
            menuItem.Click += new System.EventHandler(this.menu_toggleDock_Click);
            contextMenu.MenuItems.Add(menuItem);

            if (String.IsNullOrEmpty(strFirstColumn) == true
        && nSelectedItemCount > 0)
            {
                string strKey = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);

                menuItem = new MenuItem("检索 '" + strKey + "' (&S)");
                menuItem.DefaultItem = true;
                menuItem.Click += new System.EventHandler(this.listView_records_DoubleClick);
                contextMenu.MenuItems.Add(menuItem);

                menuItem = new MenuItem("在新开的书目查询窗内 检索 '" + strKey + "' (&N)");
                menuItem.Click += new System.EventHandler(this.listView_searchKeysAtNewWindow_Click);
                contextMenu.MenuItems.Add(menuItem);
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

            menuItem = new MenuItem("反转选择(&R)");
            menuItem.Click += new System.EventHandler(this.menu_reverseSelectAllLines_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("移除所选择的 " + this.listView_records.SelectedItems.Count.ToString() + " 个事项(&R)");
            menuItem.Click += new System.EventHandler(this.menu_removeSelectedItems_Click);
            if (this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            if (this.DbType == "biblio")
            {
                menuItem = new MenuItem("功能(&F)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = new MenuItem("打印催询单 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&C)");
                subMenuItem.Click += new System.EventHandler(this.menu_printClaim_Click);
                if (this.listView_records.SelectedItems.Count == 0 || this.InSearching == true
                    )
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("校验书目记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&V)");
                subMenuItem.Click += new System.EventHandler(this.menu_verifyBiblioRecord_Click);
                if (this.listView_records.SelectedItems.Count == 0 || this.InSearching == true
                    )
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }


            // bool bLooping = (stop != null && stop.State == 0);    // 0 表示正在处理

            // 批处理
            // 正在检索的时候，不允许进行批处理操作。因为stop.BeginLoop()嵌套后的Min Max Value之间的保存恢复问题还没有解决
            if (this.DbType == "biblio")
            {
                menuItem = new MenuItem("批处理(&B)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = new MenuItem("快速修改书目记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&Q)");
                subMenuItem.Click += new System.EventHandler(this.menu_quickChangeRecords_Click);
                if (this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("执行 MarcQuery 脚本 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&M)");
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

                subMenuItem = new MenuItem("创建新的 MarcQuery 脚本文件 (&C)");
                subMenuItem.Click += new System.EventHandler(this.menu_createMarcQueryCsFile_Click);
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);


                subMenuItem = new MenuItem("执行 .fltx 脚本 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&F)");
                subMenuItem.Click += new System.EventHandler(this.menu_quickFilterRecords_Click);
                if (this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("复制到其它书目库 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&N)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveBiblioRecToAnotherDatabase_Click);
                if (this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("移动到其它书目库 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&M)...");
                subMenuItem.Click += new System.EventHandler(this.menu_moveBiblioRecToAnotherDatabase_Click);
                if (this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("删除书目记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&D)");
                subMenuItem.Click += new System.EventHandler(this.menu_deleteSelectedRecords_Click);
                if (this.listView_records.SelectedItems.Count == 0
                    || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }

            // 导出
            if (this.DbType == "biblio")
            {
                menuItem = new MenuItem("导出(&X)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = new MenuItem("导出到记录路径文件 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&S)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToRecordPathFile_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("导出书目记录下属的册记录路径到(实体库)记录路径文件 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&E)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToEntityRecordPathFile_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("导出书目记录下属的订购记录路径到(订购库)记录路径文件 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&X)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToOrderRecordPathFile_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("导出书目记录下属的期记录路径到(期库)记录路径文件 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&X)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToIssueRecordPathFile_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("导出书目记录下属的评注记录路径到(评注库)记录路径文件 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&X)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToCommentRecordPathFile_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("导出到 MARC(ISO2709) 文件 [" + this.listView_records.SelectedItems.Count.ToString() + " ] (&M)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToMarcFile_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("到 Excel 文件 [" + nSelectedItemCount.ToString() + "] (&E)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportExcelFile_Click);
                if (nSelectedItemCount == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("详情到 Excel 文件 竖向 [" + nSelectedItemCount.ToString() + "] (&E)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportDetailExcelFile_Click);
                if (nSelectedItemCount == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("详情到 Excel 文件 横向 [" + nSelectedItemCount.ToString() + "] (&E)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportDetailExcelFile_1_Click);
                if (nSelectedItemCount == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("订购分配表到 Excel 文件 [" + nSelectedItemCount.ToString() + "] (&E)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportDistributeExcelFile_Click);
                if (nSelectedItemCount == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);


                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("导出到 XML 文件 [" + this.listView_records.SelectedItems.Count.ToString() + " ] (&M)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToXmlFile_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("导出到新书通报 [" + this.listView_records.SelectedItems.Count.ToString() + " ] (&H)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToNewBookFile_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);


                subMenuItem = new MenuItem("导出书目转储(.bdf)文件 [" + this.listView_records.SelectedItems.Count.ToString() + " ] (&B)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToBiblioDumpFile_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }

            // 装入其它查询窗
            if (this.DbType == "biblio")
            {
                menuItem = new MenuItem("装入其它查询窗 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&L)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = new MenuItem("书目查询窗");
                subMenuItem.Click += new System.EventHandler(this.menu_exportToBiblioSearchForm_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("实体查询窗");
                subMenuItem.Click += new System.EventHandler(this.menu_exportToItemSearchForm_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("订购查询窗");
                subMenuItem.Click += new System.EventHandler(this.menu_exportToOrderSearchForm_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("期查询窗");
                subMenuItem.Click += new System.EventHandler(this.menu_exportToIssueSearchForm_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("评注查询窗");
                subMenuItem.Click += new System.EventHandler(this.menu_exportToCommentSearchForm_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("856 字段查询窗");
                subMenuItem.Click += new System.EventHandler(this.menu_exportTo856SearchForm_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("批订购窗");
                subMenuItem.Click += new System.EventHandler(this.menu_exportToBatchOrderForm_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }

            // 标记空下级记录的事项
            if (this.DbType == "biblio")
            {
                menuItem = new MenuItem("标记出下级记录为空的事项 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&L)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = new MenuItem("册记录为空");
                subMenuItem.Click += new System.EventHandler(this.menu_maskEmptySubItems_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("订购记录为空");
                subMenuItem.Click += new System.EventHandler(this.menu_maskEmptySubOrders_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("期记录为空");
                subMenuItem.Click += new System.EventHandler(this.menu_maskEmptySubIssues_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("评注记录为空");
                subMenuItem.Click += new System.EventHandler(this.menu_maskEmptySubComments_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }

            // 导入
            if (this.DbType == "biblio")
            {
                menuItem = new MenuItem("导入(&I)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = new MenuItem("从记录路径文件中导入(&I)...");
                subMenuItem.Click += new System.EventHandler(this.menu_importFromRecPathFile_Click);
                if (this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }

            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            /*

            menuItem = new MenuItem("刷新控件");
            menuItem.Click += new System.EventHandler(this.menu_refreshControls_Click);
            contextMenu.MenuItems.Add(menuItem);
             * */

            menuItem = new MenuItem("刷新浏览行 [" + nSelectedItemCount.ToString() + "] (&R)");
            menuItem.Click += new System.EventHandler(this.menu_refreshSelectedItems_Click);
            if (nSelectedItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
        }

        void menu_verifyBiblioRecord_Click(object sender, EventArgs e)
        {
            int nRet = VerifyBiblioRecord(out string strError);
            if (nRet == -1)
                goto ERROR1;
            MessageBox.Show(this, "共处理 " + nRet.ToString() + " 个书目记录");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // return:
        //      false   希望中断处理记录
        //      true    希望继续处理后面记录
        public delegate bool Delegate_processBiblio(string strRecPath,
        XmlDocument dom,
        byte[] timestamp,
        ListViewItem item);

        // parameters:
        //      items   要处理的事项集合。如果为 null，表示要处理当前 ListView 中已选择的行
        int ProcessBiblio(
            string looping_title,
            List<ListViewItem> items,
            Delegate_processBiblio func,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (items == null)
            {
                items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }
            }

            if (items.Count == 0)
            {
                strError = "尚未选定要进行批处理的事项";
                return -1;
            }

#if REMOVED
            // if (_stop != null && _stop.State == 0)    // 0 表示正在处理
            if (HasLooping())
            {
                strError = "目前有长操作正在进行，无法进行批处理书目记录的操作";
                return -1;
            }
#endif

            int nCount = 0;

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在进行批处理书目记录的操作 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop,
                looping_title,  // "正在进行批处理书目记录的操作 ...",
                "halfstop");
            this.EnableControls(false);
            try
            {
                /*
                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }
                */

                looping.Progress.SetProgressRange(0, items.Count);

                ListViewBiblioLoader loader = new ListViewBiblioLoader(
                    channel, // this.Channel,
                    looping.Progress,
                    items,
                    items.Count > MAX_CACHE_ITEMS ? null : this.m_biblioTable);
                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    if (this.InvokeRequired)
                        Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    BiblioInfo info = item.BiblioInfo;

                    XmlDocument itemdom = new XmlDocument();
                    try
                    {
                        string xml = info.OldXml;
                        if (string.IsNullOrEmpty(xml))
                            xml = "<root />";
                        itemdom.LoadXml(xml);
                    }
                    catch (Exception ex)
                    {
                        strError = "记录 '" + info.RecPath + "' 的 XML 装入 DOM 时出错: " + ex.Message;
                        return -1;
                    }

                    if (func != null)
                    {
                        if (func(info.RecPath, itemdom, info.Timestamp, item.ListViewItem) == false)
                            break;
                    }

                    nCount++;
                    looping.Progress.SetProgressValue(++i);
                }

                return nCount;
            }
            catch (Exception ex)
            {
                strError = $"ProcessBiblio() 出现异常: {ex.Message}";
                // 写入错误日志
                MainForm.WriteErrorLog($"ProcessBiblio() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return -1;
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                this.ReturnChannel(channel);

                this.EnableControls(true);
            }
        }

        // 2020/7/27
        // 校验一条书目记录
        // return:
        //      -1  校验过程出错
        //      0   校验成功
        //      1   校验发现记录有错
        static int VerifyBiblio(
            LibraryChannel channel,
            string strBiblioRecPath,
            string strXml,
            out string strError)
        {
            strError = "";

            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            int nRet = MarcUtil.Xml2Marc(strXml,    // info.OldXml,
                true,
                null,
                out string strMarcSyntax,
                out string strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            List<string> errors = new List<string>();

            MarcRecord record = new MarcRecord(strMARC);
            var targetRecPath = record.select("field[@name='998']/subfield[@name='t']").FirstContent;
            if (string.IsNullOrEmpty(targetRecPath) == false)
            {
                if (targetRecPath == strBiblioRecPath)
                    errors.Add($"本记录中 998$t 引用了自己 {strBiblioRecPath}");
                else
                {
                    // 检查所链接的记录的题名是否和本记录一致
                    string title = "";
                    if (strMarcSyntax == "unimarc")
                        title = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                    else
                        title = record.select("field[@name='245']/subfield[@name='a']").FirstContent;

                    // return:
                    //      -1  出错
                    //      0   书目记录没有找到
                    //      1   成功
                    nRet = GetLinkBiblioTitle(channel,
                        targetRecPath,
                        out string link_title,
                        out strError);
                    if (nRet == -1)
                        errors.Add($"尝试获得书目记录 {targetRecPath} 过程出错：{strError}");
                    else if (nRet == 0)
                        errors.Add($"(998$t 指向的)书目记录 {targetRecPath} 不存在");
                    else
                    {
                        if (title != link_title)
                            errors.Add($"本记录的题名 '{title}' 和 998$t({targetRecPath}) 指向的目标书目记录的题名 '{link_title}' 不一致。将来典藏转移的时候会出现错误转移");
                    }
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return 1;
            }
            return 0;
        }

        // return:
        //      -1  出错
        //      0   书目记录没有找到
        //      1   成功
        static int GetLinkBiblioTitle(LibraryChannel channel,
            string strBiblioRecPath,
            out string title,
            out string strError)
        {
            strError = "";
            title = "";

            long lRet = channel.GetBiblioInfos(
        null,   // stop,
        strBiblioRecPath,
        "",
        new string[] { "xml" },   // formats
        out string[] results,
        out byte[] baNewTimestamp,
        out strError);
            if (lRet == 0)
            {
                return 0;   // 记录不存在
            }
            if (lRet == -1)
                return -1;
            if (results == null || results.Length == 0)
            {
                strError = "results error";
                return -1;
            }
            string strXml = results[0];

            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            int nRet = MarcUtil.Xml2Marc(strXml,    // info.OldXml,
                true,
                null,
                out string strMarcSyntax,
                out string strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            MarcRecord record = new MarcRecord(strMARC);
            if (strMarcSyntax == "unimarc")
                title = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
            else
                title = record.select("field[@name='245']/subfield[@name='a']").FirstContent;

            return 1;
        }

        // 校验书目记录
        int VerifyBiblioRecord(out string strError)
        {
            strError = "";
            // int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要进行批处理的事项";
                return -1;
            }

            // if (_stop != null && _stop.State == 0)    // 0 表示正在处理
            if (HasLooping())
            {
                strError = "目前有长操作正在进行，无法进行校验书目记录的操作";
                return -1;
            }

            // 切换到“操作历史”属性页
            Program.MainForm.ActivateFixPage("history");

            int nCount = 0;

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
                + " 开始进行书目记录校验</div>");

            LibraryChannel channel = this.GetChannel();

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在进行校验书目记录的操作 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在进行校验书目记录的操作 ...", "halfstop");

            this.EnableControls(false);
            try
            {
                looping.Progress.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }
                ListViewBiblioLoader loader = new ListViewBiblioLoader(channel, // this.Channel,
                    looping.Progress,
                    items,
                    items.Count > MAX_CACHE_ITEMS ? null : this.m_biblioTable);
                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    BiblioInfo info = item.BiblioInfo;

                    XmlDocument itemdom = new XmlDocument();
                    try
                    {
                        string xml = info.OldXml;
                        if (string.IsNullOrEmpty(xml))
                            xml = "<root />";
                        itemdom.LoadXml(xml);
                    }
                    catch (Exception ex)
                    {
                        strError = "记录 '" + info.RecPath + "' 的 XML 装入 DOM 时出错: " + ex.Message;
                        return -1;
                    }

                    List<string> errors = new List<string>();

                    // 校验 XML 记录中是否有非法字符
                    string strReplaced = DomUtil.ReplaceControlCharsButCrLf(info.OldXml, '*');
                    if (strReplaced != info.OldXml)
                    {
                        errors.Add("XML 记录中有非法字符");
                    }

                    // 校验一条书目记录
                    // return:
                    //      -1  校验过程出错
                    //      0   校验成功
                    //      1   校验发现记录有错
                    int nRet = VerifyBiblio(
                        channel,
                        info.RecPath,
                        info.OldXml,
                        out strError);
                    if (nRet == -1 || nRet == 1)
                        errors.Add(strError);
#if NO
                    // 验证唯一性
                    {
                        byte[] baNewTimestamp = null;
                        string strOutputPath = "";
                        long lRet = channel.SetBiblioInfo(
                            stop,
                            "checkunique",
                            info.RecPath,
                            "xml",
                            "", // info.NewXml,
                            null,   // info.Timestamp,
                            "",
                            out strOutputPath,
                            out baNewTimestamp,
                            out strError);
                        if (lRet == -1 && channel.ErrorCode == ErrorCode.BiblioDup)
                        {

                        }
                    }
#endif

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
                    looping.Progress.SetProgressValue(++i);
                }

                return nCount;
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);


                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControls(true);

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
                    + " 结束执行书目记录校验</div>");
            }
        }

        // 修改 XML 中的 seller 元素值
        static string SetSeller(string strXml, string strSeller)
        {
            if (string.IsNullOrEmpty(strXml) || string.IsNullOrEmpty(strSeller))
                return strXml;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            DomUtil.SetElementText(dom.DocumentElement, "seller", strSeller);
            return dom.DocumentElement.OuterXml;
        }

        // parameters:
        //      strSellerFilter 书商筛选字符串。形态为 "*"，或者 "书商1,书商2"
        static string GetFirstSeller(string strSellerFilter)
        {
            if (string.IsNullOrEmpty(strSellerFilter))
                return "";
            if (strSellerFilter == "*")
                return "";
            if (strSellerFilter.IndexOf(",") == -1)
                return strSellerFilter;
            return StringUtil.FromListString(strSellerFilter)[0];
        }

        static string MacroXml(string strXml,
            string strMARC,
            string strMarcSyntax)
        {
            if (string.IsNullOrEmpty(strXml))
                return strXml;

            XmlDocument order_dom = new XmlDocument();
            order_dom.LoadXml(strXml);

            if (order_dom.DocumentElement == null)
                return strXml;

            XmlNodeList elements = order_dom.DocumentElement.SelectNodes("*");
            foreach (XmlElement element in elements)
            {
                string strValue = element.InnerText.Trim();
                if (strValue.StartsWith("@"))
                {
                    // 兑现宏
                    string strResult = DoGetMacroValue(strValue, strMARC, strMarcSyntax);
                    if (strResult != strValue)
                        element.InnerText = strResult;
                }
            }

            return order_dom.DocumentElement.OuterXml;
        }

        static string DoGetMacroValue(string strMacroName,
            string strMARC,
            string strMarcSyntax)
        {
            if (string.IsNullOrEmpty(strMarcSyntax) == true)
                return strMacroName;

            if (string.IsNullOrEmpty(strMARC) == false)
            {
                MarcRecord record = new MarcRecord(strMARC);

                string strValue = null;
                // UNIMARC 情形
                if (strMarcSyntax == "unimarc")
                {
                    if (strMacroName == "@price")
                        strValue = record.select("field[@name='010']/subfield[@name='d'] | field[@name='011']/subfield[@name='d']").FirstContent;
                }
                else if (strMarcSyntax == "usmarc")
                {
                    if (strMacroName == "@price")
                        strValue = record.select("field[@name='020']/subfield[@name='c'] | field[@name='022']/subfield[@name='c']").FirstContent;
                }

                if (string.IsNullOrEmpty(strValue) == false)
                    return strValue;
            }

            return strMacroName;
        }

        // return:
        //      -2  码洋和订购价货币单位不同，无法进行校验。
        //      -1  校验过程出错
        //      0   校验发现三者关系不正确
        //      1   校验三者关系正确
        public static int VerifyThreeFields(XmlDocument order_dom, out string strError)
        {
            strError = "";

            string strFixedPrice = DomUtil.GetElementText(order_dom.DocumentElement, "fixedPrice");
            string strPrice = DomUtil.GetElementText(order_dom.DocumentElement, "price");
            string strDiscount = DomUtil.GetElementText(order_dom.DocumentElement, "discount");

            if (string.IsNullOrEmpty(strFixedPrice)
                || string.IsNullOrEmpty(strPrice))
                return 1;

            // 检查码洋、折扣和单价之间的关系
            // return:
            //      -2  码洋和订购价货币单位不同，无法进行校验。
            //      -1  校验过程出错
            //      0   校验发现三者关系不正确
            //      1   校验三者关系正确
            int nRet = OrderDesignControl.VerifyOrderPriceByFixedPricePair(
                strFixedPrice,
                strDiscount,
                strPrice,
                "both",
                out strError);
            return nRet;
        }

        // 导出订购去向分配表 Excel 文件
        void menu_exportDistributeExcelFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要导出的事项";
                goto ERROR1;
            }

            Order.SaveDistributeExcelFileDialog dlg = new Order.SaveDistributeExcelFileDialog();
            MainForm.SetControlFont(dlg, this.Font);
            dlg.LibraryCodeList = Program.MainForm.GetAllLibraryCode();
            dlg.LibraryCode = Program.MainForm.FocusLibraryCode;
            dlg.UiState = Program.MainForm.AppInfo.GetString(
        "ItemSearchForm",
        "SaveDistributeExcelFileDialog_uiState",
        "");
            Program.MainForm.AppInfo.LinkFormState(dlg, "bibliosearchform_SaveDistributeExcelFileDialog");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);
            Program.MainForm.AppInfo.SetString(
        "ItemSearchForm",
        "SaveDistributeExcelFileDialog_uiState",
        dlg.UiState);
            if (dlg.DialogResult != System.Windows.Forms.DialogResult.OK)
                return;

            int nRet = GetLocationList(
        out List<string> location_list_param,
        out strError);
            if (nRet == -1)
            {
                strError = "获得馆藏地配置参数时出错: " + strError;
                goto ERROR1;
            }
            var location_list = Order.DistributeExcelFile.FilterLocationList(location_list_param, dlg.LibraryCode);
            if (location_list.Count == 0)
            {
                strError = "当前用户能管辖的馆藏地 '"
                    + StringUtil.MakePathList(location_list_param)
                    + "' 和您选择的馆藏地过滤 '" + dlg.LibraryCode + "' 没有任何共同部分";
                goto ERROR1;
            }

            bool bLaunchExcel = true;

            XLWorkbook doc = null;
            try
            {
                doc = new XLWorkbook(XLEventTracking.Disabled);
                File.Delete(dlg.OutputFileName);
            }
            catch (Exception ex)
            {
                strError = "BiblioSearchForm new XLWorkbook() exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            List<int> column_max_chars = new List<int>();   // 每个列的最大字符数            
            int nLineNumber = 0;    // 序号            
                                    // int nRowIndex = 2;  // 跟踪行号            
                                    // bool bDone = false; // 是否成功走完全部流程

            int nBiblioCount = 0;   // 导出书目计数
            int nOrderCount = 0;    // 导出订购记录计数
            int nNewOrderCount = 0; // 导出新订购记录计数(已包含在 nOrderCount 数值内)
            int nWriteNewOrderCount = 0;    // 导出前立即写入订购库的新订购记录数

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
        + " 开始导出订购去向 Excel 文件</div>");
            var looping = Looping(
                out LibraryChannel channel,
                "正在导出订购去向 Excel 文件 ...");
            try
            {
                IXLWorksheet sheet = doc.Worksheets.Add("订购去向分配表");
                // sheet.Protect().SetInsertRows().SetFormatColumns().SetFormatRows().SetFormatCells().SetScenarios();

                // 准备书目列标题
                Order.BiblioColumnOption biblio_column_option = new Order.BiblioColumnOption(Program.MainForm.UserDir,
                    "");
                biblio_column_option.LoadData(Program.MainForm.AppInfo,
                typeof(Order.BiblioColumnOption).ToString());

                List<Order.ColumnProperty> biblio_title_list = Order.DistributeExcelFile.BuildList(biblio_column_option.Columns);

                // 准备订购列标题
                Order.OrderColumnOption order_column_option = new Order.OrderColumnOption(Program.MainForm.UserDir,
        "");
                order_column_option.LoadData(Program.MainForm.AppInfo,
                typeof(Order.OrderColumnOption).ToString());

                List<Order.ColumnProperty> order_title_list = Order.DistributeExcelFile.BuildList(order_column_option.Columns);
                // 附加某些列的值列表
                {
                    // LibraryChannel channel = this.GetChannel();
                    try
                    {
                        if (Order.ColumnProperty.FillValueList(
                            channel,
                            dlg.LibraryCode,
                            order_title_list,
                            out strError) == -1)
                            goto ERROR1;
                    }
                    finally
                    {
                        // this.ReturnChannel(channel);
                    }
                }

                var context = new Order.DistributeExcelFile
                {
                    Sheet = sheet,
                    LocationList = location_list,
                    BiblioColList = biblio_title_list,
                    OrderColList = order_title_list,
                    ColumnMaxChars = column_max_chars,
                    RowIndex = 2,
                    OnlyOutputBlankStateOrderRecord = dlg.OnlyOutputBlankStateOrderRecord,
                };

                // 输出标题行
                context.OutputDistributeInfoTitleLine(
        // context,
        ""
        );

                /*
        * content_form_area
        * title_area
        * edition_area
        * material_specific_area
        * publication_area
        * material_description_area
        * series_area
        * notes_area
        * resource_identifier_area
        * * */

                string strSellerFilter = dlg.SellerFilter;  // "*";
                string strDefaultOrderXml = "";

                nRet = ProcessBiblio(
                    "正在导出订购去向 Excel 文件 ...",
                    null,
                    (strBiblioRecPath, biblio_dom, biblio_timestamp, item) =>
                    {
                        this.ShowMessage("正在处理书目记录 " + strBiblioRecPath);

                        Order.DistributeExcelFile.WarningRecPath("===", null);
                        Order.DistributeExcelFile.WarningRecPath("书目记录 " + strBiblioRecPath, null);

                        nOrderCount += context.OutputDistributeInfos(
                            // context,
                            this,
                            strSellerFilter,
                            dlg.LibraryCode,
                            //sheet,
                            strBiblioRecPath,
                            ref nLineNumber,
                            "",
                            (biblio_recpath, order_recpath) =>
                            {
                            REDO_0:
                                if (string.IsNullOrEmpty(strDefaultOrderXml))
                                {
                                REDO:
                                    // 看看即将插入的位置是图书还是期刊?
                                    string strPubType = OrderEditForm.GetPublicationType(biblio_recpath);

                                    OrderEditForm edit = new OrderEditForm();

                                    OrderEditForm.SetXml(edit.OrderEditControl,
                                        SetSeller(Program.MainForm.AppInfo.GetString("BiblioSearchForm", "orderRecord", "<root />"), GetFirstSeller(strSellerFilter)),
                                        strPubType);
                                    edit.Text = "新增订购事项";
                                    edit.DisplayMode = strPubType == "series" ? "simpleseries" : "simplebook";

                                    Program.MainForm.AppInfo.LinkFormState(edit, "BiblioSearchForm_OrderEditForm_state");
                                    edit.ShowDialog(this);

                                    strDefaultOrderXml = OrderEditForm.GetXml(edit.OrderEditControl);
                                    // 删除那些空内容的元素
                                    strDefaultOrderXml = DomUtil.RemoveEmptyElements(strDefaultOrderXml);

                                    Program.MainForm.AppInfo.SetString("BiblioSearchForm", "orderRecord", strDefaultOrderXml);

                                    if (edit.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                                    {
                                        strDefaultOrderXml = "<root />";
                                        throw new InterruptException("用户中断");
                                    }

                                    XmlDocument order_dom = new XmlDocument();
                                    order_dom.LoadXml(strDefaultOrderXml);

                                    string strDistribute = DomUtil.GetElementInnerText(order_dom.DocumentElement, "distribute");

                                    // 观察一个馆藏分配字符串，看看是否在指定用户权限的管辖范围内
                                    // return:
                                    //      -1  出错
                                    //      0   超过管辖范围。strError中有解释
                                    //      1   在管辖范围内
                                    nRet = dp2StringUtil.DistributeInControlled(strDistribute,
                                    dlg.LibraryCode,
                                    out bool bAllOutOf,
                                    out strError);
                                    if (nRet == -1)
                                        throw new Exception(strError);
                                    if (nRet == 0)
                                    {
                                        strError = "馆藏去向 '" + strDistribute + "' 越过当前用户的关注范围(馆代码) '" + dlg.LibraryCode + "'。请重新指定馆藏去向";
                                        this.MessageBoxShow(strError);
                                        goto REDO;
                                    }

                                    // TODO: 验证一下书商名称是否在合法值范围内?



                                }

                                // 兑现模板记录中的宏
                                string strResultXml = strDefaultOrderXml;
                                if (strDefaultOrderXml.IndexOf("@") != -1)
                                {
                                    // 将XML格式转换为MARC格式
                                    // 自动从数据记录中获得MARC语法
                                    nRet = MarcUtil.Xml2Marc(biblio_dom.OuterXml,
                                    true,
                                    null,
                                    out string strMarcSyntax,
                                    out string strMARC,
                                    out strError);
                                    if (nRet == -1)
                                    {
                                        strError = "XML转换到MARC记录时出错: " + strError;
                                        throw new Exception(strError);
                                    }

                                    // 兑现模板 XML 中的宏
                                    strResultXml = MacroXml(strDefaultOrderXml,
        strMARC,
        strMarcSyntax);
                                    XmlDocument temp_dom = new XmlDocument();
                                    temp_dom.LoadXml(strResultXml);
                                    // 验证三个字段之间的关系
                                    // return:
                                    //      -2  码洋和订购价货币单位不同，无法进行校验。
                                    //      -1  校验过程出错
                                    //      0   校验发现三者关系不正确
                                    //      1   校验三者关系正确
                                    nRet = VerifyThreeFields(temp_dom, out strError);
                                    if (nRet == 0 || nRet == -1)
                                    {
                                        strError = "校验三个字段关系时发现问题: " + strError + "\r\n\r\n请重新输入。特别注意关注码洋、折扣和单价之间的计算关系";
                                        this.MessageBoxShow(strError);
                                        strDefaultOrderXml = "";
                                        goto REDO_0;
                                    }
                                }

                                EntityInfo order = new EntityInfo
                                {
                                    OldRecord = strResultXml
                                };

                                // 实际写入订购记录
                                if (dlg.CreateNewOrderRecord)
                                {
                                    // LibraryChannel channel = this.GetChannel();
                                    try
                                    {
                                        nRet = MainForm.SaveItemRecord(
                                            looping.Progress,
                                            channel,
    biblio_recpath,
    "order",
    "", // strOrderRecPath,
    "",
    order.OldRecord,
    "",
    null,   // timestamp,
    out string strOutputOrderRecPath,
    out byte[] baNewTimestamp,
    out strError);
                                        if (nRet == -1)
                                            throw new Exception(strError);
                                        order.OldTimestamp = baNewTimestamp;
                                        order.OldRecPath = strOutputOrderRecPath;
                                    }
                                    finally
                                    {
                                        // this.ReturnChannel(channel);
                                    }
                                    nWriteNewOrderCount++;
                                }

                                Order.DistributeExcelFile.WarningGreen("因书目记录 '" + strBiblioRecPath + "' 没有符合条件的订购记录，所以导出一条新的订购记录，如下：");
                                Order.DistributeExcelFile.WarningRecPath(order.OldRecPath, DomUtil.GetIndentXml(DomUtil.RemoveEmptyElements(order.OldRecord)));

                                nNewOrderCount++;
                                return order;
                            }
                            );

                        nBiblioCount++;
                        return true;
                    },
                out strError);
                if (nRet == -1)
                    goto ERROR1;

                context.ContentEndRow = context.RowIndex - 1;

                context.OutputSumLine(
                    // context
                    );

                Order.DistributeExcelFile.AdjectColumnWidth(sheet, column_max_chars, 20);

                // bDone = true;

                if (doc != null)
                {
                    doc.SaveAs(dlg.OutputFileName);
                    doc.Dispose();
                }
            }
            catch (InterruptException ex)
            {
                strError = "导出去向分配表 Excel 时" + ex.Message;
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "导出去向分配表 Excel 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                looping.Dispose();
                /*
                if (_stop != null)
                    _stop.SetMessage("");
                */

                this.ClearMessage();

                Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
        + " 结束导出订购去向 Excel 文件</div>");
            }

            if (bLaunchExcel)
            {
                try
                {
                    System.Diagnostics.Process.Start(dlg.OutputFileName);
                }
                catch
                {

                }
            }

            // 提示完成和统计信息
            MessageDialog.Show(this,
                string.Format("导出完成。\r\n\r\n共处理书目记录 {0} 条, 导出订购记录 {1} 条。\r\n所导出的订购记录中，{2} 条是新订购记录， 其中 {3} 条已经写入订购库",
                nBiblioCount, nOrderCount, nNewOrderCount, nWriteNewOrderCount));
            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

        // 2023/4/7
        // 导出详情到 Excel 文件。横向。可配置列
        void menu_exportDetailExcelFile_1_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要导出的事项";
                goto ERROR1;
            }

            var dlg = new SaveBiblioExcelFileDialog();
            MainForm.SetControlFont(dlg, this.Font);

            dlg.UiState = Program.MainForm.AppInfo.GetString(
"BiblioSearchForm",
"ExportExcelFileDialog_uiState",
"");
            Program.MainForm.AppInfo.LinkFormState(dlg, "bibliosearchform_ExportExcelFileDialog");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);
            Program.MainForm.AppInfo.SetString(
"BiblioSearchForm",
"ExportExcelFileDialog_uiState",
dlg.UiState);
            if (dlg.DialogResult != System.Windows.Forms.DialogResult.OK)
                return;

            if (File.Exists(dlg.OutputFileName))
            {
                DialogResult result = MessageBox.Show(this,
$"文件 {dlg.OutputFileName} 已经存在。\r\n\r\n继续处理将会覆盖它。要继续处理么? (是：继续; 否: 放弃处理)",
"BiblioSearchForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
            }

            bool bLaunchExcel = true;

            XLWorkbook doc = null;
            try
            {
                doc = new XLWorkbook(XLEventTracking.Disabled);
                File.Delete(dlg.OutputFileName);
            }
            catch (Exception ex)
            {
                strError = "BiblioSearchForm new XLWorkbook() exception(2): " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("表格");

            // 准备书目列标题
            Order.ExportBiblioColumnOption biblio_column_option = new Order.ExportBiblioColumnOption(Program.MainForm.UserDir);
            biblio_column_option.LoadData(Program.MainForm.AppInfo,
            SaveBiblioExcelFileDialog.BiblioDefPath);

            List<Order.ColumnProperty> biblio_title_list = Order.DistributeExcelFile.BuildList(biblio_column_option.Columns);

            // 每个列的最大字符数
            List<int> column_max_chars = new List<int>();
            int nLineNumber = 0;    // 序号            

            var looping = Looping(out LibraryChannel channel,
                "");
            try
            {
                var context = new dp2Circulation.Order.ExportExcelFile
                {
                    Sheet = sheet,
                    BiblioColList = biblio_title_list,
                    OrderColList = null,
                    ColumnMaxChars = column_max_chars,
                    RowIndex = 2,
                };

                // 输出标题行
                context.OutputDistributeInfoTitleLine(
""
);


                int nRet = ProcessBiblio(
                    "正在导出详情到 Excel 文件",
                    null,
                    (strRecPath, dom, timestamp, item) =>
                    {
                        this.ShowMessage("正在处理书目记录 " + strRecPath);

                        string strTableXml = "";
                        {
                            // return:
                            //      -1  出错
                            //      0   没有找到
                            //      1   找到
                            nRet = this.GetTable(
                                strRecPath,
                                biblio_title_list,
                                null,
                                (Order.ColumnProperty c, out string v) =>
                                {
                                    v = null;
                                    if (c.Type == "biblio_accessNo")
                                    {
                                        /*
                                        // return:
                                        //      -1  出错
                                        //      0   没有找到
                                        //      1   成功
                                        var ret = Utility.GetSubRecords(
                        channel,
                        looping.Progress,
                        strRecPath,
                        "firstAccessNo",
                        out string strResult,
                        out string error);
                                        */
                                        // return:
                                        //      -1  出错
                                        //      0   没有找到
                                        //      1   成功
                                        var ret = Utility.GetFirstAccessNo(
                                            channel,
                                            looping.Progress,
                                            strRecPath,
                                            out string strResult,
                                            out string error);
                                        v = strResult;
                                        if (ret == -1)
                                            v = "error:" + error;
                                        return ProcessParts.Basic;
                                    }

                                    // 2023/11/9
                                    if (c.Type == "biblio_itemCount")
                                    {
                                        // return:
                                        //      -1  出错
                                        //      0   没有找到
                                        //      1   成功
                                        var ret = Utility.GetSubRecords(
                        channel,
                        looping.Progress,
                        strRecPath,
                        "itemCount",
                        out string strResult,
                        out string error);
                                        v = strResult;
                                        if (ret == -1)
                                            v = "error:" + error;
                                        return ProcessParts.Basic;
                                    }

                                    if (c.Type == "biblio_recpath")
                                    {
                                        v = strRecPath;
                                        return ProcessParts.Basic;
                                    }

                                    /*
                                    if (c.Type == "biblio_items"
                                    || c.Type == "items")
                                    {
                                        v = "placeholder";
                                        return ProcessParts.Basic;
                                    }
                                    */
                                    return ProcessParts.None;
                                },
                                out strTableXml,
                                out string strError1);
                            if (nRet == -1)
                                throw new Exception(strError1);
                        }

                        context.OutputDistributeInfo(
                                this,
                                strRecPath,
                                ref nLineNumber,
                                strTableXml,
                                "", // strStyle,
                                null,   // strEntityRecPath,
                                (biblio_recpath, order_recpath) =>
                                {
                                    return null;
                                }
                                );

                        /*
                        OutputBiblioInfoNew(sheet,
                            dom,
                            strRecPath,
                            nBiblioIndex++,
                            "",
                            ref nRowIndex);
                        */
                        context.RowIndex++;
                        return true;
                    },
                out strError);
                if (nRet == -1)
                    goto ERROR1;

                context.ContentEndRow = context.RowIndex - 1;
                Order.DistributeExcelFile.AdjectColumnWidth(sheet, column_max_chars, 20);

            }
            catch (Exception ex)
            {
                strError = "导出详情到 Excel 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                looping.Dispose();
                /*
                if (_stop != null)
                    _stop.SetMessage("");
                */

                this.ClearMessage();

                if (doc != null)
                {
                    doc.SaveAs(dlg.OutputFileName);
                    doc.Dispose();
                }

                if (bLaunchExcel)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(dlg.OutputFileName);
                    }
                    catch
                    {

                    }
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 2017/3/17
        // 导出详情到 Excel 文件。竖向
        void menu_exportDetailExcelFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要导出的事项";
                goto ERROR1;
            }

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            bool bLaunchExcel = true;

            XLWorkbook doc = null;
            try
            {
                doc = new XLWorkbook(XLEventTracking.Disabled);
                File.Delete(dlg.FileName);
            }
            catch (Exception ex)
            {
                strError = "BiblioSearchForm new XLWorkbook() {D8E22F4C-9810-4799-A243-3D8148047646} exception: " + ExceptionUtil.GetAutoText(ex);
                return;
            }

            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("表格");

            // 每个列的最大字符数
            List<int> column_max_chars = new List<int>();
            int nBiblioIndex = 0;
            int nRowIndex = 2;
            try
            {
                int nRet = ProcessBiblio(
                    "正在导出详情到 Excel 文件",
                    null,
                    (strRecPath, dom, timestamp, item) =>
                    {
                        this.ShowMessage("正在处理书目记录 " + strRecPath);

                        OutputBiblioInfo(sheet,
                            dom,
                            strRecPath,
                            nBiblioIndex++,
                            "",
                            ref nRowIndex);
                        nRowIndex++;    // 空行
                        return true;
                    },
                out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "导出详情到 Excel 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                /*
                if (_stop != null)
                    _stop.SetMessage("");
                */

                this.ClearMessage();

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

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 输出书目 Excel 内容。纵向 OPAC 方式
        void OutputBiblioInfo(IXLWorksheet sheet,
    XmlDocument dom,
    string strRecPath,
    int nBiblioIndex,
    string strStyle,
    ref int nRowIndex
    // ref List<int> column_max_chars
    )
        {
            string strError = "";

            string strTableXml = "";
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = GetTable(
                strRecPath,
                "",
                out strTableXml,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            ExcelUtility.OutputBiblioTable(
            strRecPath,
            strTableXml,
            nBiblioIndex,
            sheet,
            2,  // nColIndex,
            ref nRowIndex);

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
            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在导出选定的事项到 Excel 文件 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在导出选定的事项到 Excel 文件 ...", "halfstop");

            this.EnableControls(false);
            try
            {
                int nRet = ClosedXmlUtil.ExportToExcel(
                    looping.Progress,
                    items,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                this.EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_printClaim_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要装入打印催询窗的行";
                goto ERROR1;
            }

            string strIssueDbName = "";
            if (this.listView_records.SelectedItems.Count > 0)
            {
                string strFirstRecPath = "";
                strFirstRecPath = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);
                string strDbName = Global.GetDbName(strFirstRecPath);
                if (string.IsNullOrEmpty(strDbName) == false)
                    strIssueDbName = Program.MainForm.GetIssueDbName(strDbName);
            }

            List<string> recpaths = new List<string>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                string strRecPath = ListViewUtil.GetItemText(item, 0);
                if (string.IsNullOrEmpty(strRecPath) == false)
                    recpaths.Add(strRecPath);
            }

            PrintClaimForm form = new PrintClaimForm();
            form.MdiParent = Program.MainForm;
            form.Show();

            if (string.IsNullOrEmpty(strIssueDbName) == false)
                form.PublicationType = PublicationType.Series;
            else
                form.PublicationType = PublicationType.Book;

            form.EnableControls(false);
            form.SetBiblioRecPaths(recpaths);
            form.EnableControls(true);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 丢弃选定的修改
        void menu_clearSelectedChangedRecords_Click(object sender, EventArgs e)
        {
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
#if NO
                    string strRecPath = item.Text;
                    if (string.IsNullOrEmpty(strRecPath) == true)
                        continue;

                    BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
                    if (info == null)
                        continue;

                    if (String.IsNullOrEmpty(info.NewXml) == false)
                    {
                        info.NewXml = "";

                        item.BackColor = SystemColors.Window;
                        item.ForeColor = SystemColors.WindowText;

                        this.m_nChangedCount--;
                        Debug.Assert(this.m_nChangedCount >= 0, "");
                    }
#endif
                    ClearOneChange(item);
                }
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            RefreshPropertyView(false);
        }

        // 丢弃全部修改
        void menu_clearAllChangedRecords_Click(object sender, EventArgs e)
        {
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
#if NO
                    string strRecPath = item.Text;
                    if (string.IsNullOrEmpty(strRecPath) == true)
                        continue;

                    BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
                    if (info == null)
                        continue;

                    if (String.IsNullOrEmpty(info.NewXml) == false)
                    {
                        info.NewXml = "";

                        item.BackColor = SystemColors.Window;
                        item.ForeColor = SystemColors.WindowText;

                        this.m_nChangedCount--;
                        Debug.Assert(this.m_nChangedCount >= 0, "");
                    }
#endif
                    ClearOneChange(item);
                }
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            RefreshPropertyView(false);
        }

        // 保存选定事项的修改
        async void menu_saveSelectedChangedRecords_Click(object sender, EventArgs e)
        {
            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项需要保存");
                return;
            }

            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                items.Add(item);
            }

            /*
            int nRet = SaveChangedRecords(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            */
            var result = await SaveChangedRecordsAsync(items);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                goto ERROR1;
            }

            strError = "处理完成。\r\n\r\n" + strError;
            this.MessageBoxShow(strError);
            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

        // 保存全部修改事项
        async void menu_saveAllChangedRecords_Click(object sender, EventArgs e)
        {
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项需要保存");
                return;
            }

            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_records.Items)
            {
                items.Add(item);
            }

            /*
            int nRet = SaveChangedRecords(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            */
            var result = await SaveChangedRecordsAsync(items);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                goto ERROR1;
            }

            strError = "处理完成。\r\n\r\n" + strError;
            this.MessageBoxShow(strError);
            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

        Task<NormalResult> SaveChangedRecordsAsync(List<ListViewItem> items)
        {
            return Task.Factory.StartNew<NormalResult>(
                () =>
                {
                    try
                    {
                        return _saveChangedRecords(items);
                    }
                    catch (Exception ex)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"_saveChangedRecords() 异常: {ExceptionUtil.GetDebugText(ex)}"
                        };
                    }
                },
    default,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);
        }


        NormalResult _saveChangedRecords(List<ListViewItem> items)
        {
            string strError = "";

            int nReloadCount = 0;
            // int nSavedCount = 0;

            bool bDontPrompt = false;
            DialogResult dialog_result = DialogResult.Yes;  // yes no cancel
            List<ListViewItem> saved_items = new List<ListViewItem>();

            LibraryChannel channel = this.GetChannel();

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在保存书目记录 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在保存书目记录 ...", "halfstop");

            this.EnableControlsInSearching(false);
            this.TryInvoke(() =>
            {
                this.listView_records.Enabled = false;
            });
            try
            {
                looping.Progress.SetProgressRange(0, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    if (looping.Stopped)
                    {
                        strError = "已中断";
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError
                        };
                    }

                    ListViewItem item = items[i];
                    string strRecPath = ListViewUtil.GetItemText(item, 0);  //  item.Text;
                    if (string.IsNullOrEmpty(strRecPath) == true)
                    {
                        looping.Progress.SetProgressValue(i);
                        goto CONTINUE;
                    }

                    BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
                    if (info == null)
                        goto CONTINUE;

                    if (string.IsNullOrEmpty(info.NewXml) == true)
                        goto CONTINUE;

                    // 暂不处理外来记录的保存
                    // TODO: 此时警告不能保存?
                    if (info.RecPath.IndexOf("@") != -1)
                        goto CONTINUE;

                    looping.Progress.SetMessage("正在保存书目记录 " + strRecPath);

                    int nRedoCount = 0;
                REDO_SAVE:
                    long lRet = channel.SetBiblioInfo(
                        looping.Progress,
                        "change",
                        strRecPath,
                        "xml",
                        info.NewXml,
                        info.Timestamp,
                        "",
                        out string strOutputPath,
                        out byte[] baNewTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ErrorCode.TimestampMismatch)
                        {
                            var temp = strError;
                            DialogResult result = this.TryGet(() =>
                            {
                                return MessageBox.Show(this,
        "保存书目记录 " + strRecPath + " 时遭遇时间戳不匹配: " + temp + "。\r\n\r\n此记录已无法被保存。\r\n\r\n请问现在是否要顺便重新装载此记录? \r\n\r\n(Yes 重新装载；\r\nNo 不重新装载、但继续处理后面的记录保存; \r\nCancel 中断整批保存操作)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                            });
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                                break;
                            if (result == System.Windows.Forms.DialogResult.No)
                                goto CONTINUE;

                            // 重新装载书目记录到 OldXml
                            // byte[] baTimestamp = null;
                            lRet = channel.GetBiblioInfos(
                                looping.Progress,
                                strRecPath,
                                "",
                                new string[] { "xml" },   // formats
                                out string[] results,
                                out baNewTimestamp,
                                out strError);
                            if (lRet == 0)
                            {
                                // TODO: 警告后，把 item 行移除？
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError
                                };
                            }
                            if (lRet == -1)
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError
                                };
                            if (results == null || results.Length == 0)
                            {
                                strError = "results error";
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError
                                };
                            }
                            info.OldXml = results[0];
                            info.Timestamp = baNewTimestamp;
                            nReloadCount++;
                            goto CONTINUE;
                        }

                        // 
                        if (bDontPrompt == false || nRedoCount >= 10)
                        {
                            bool temp = bDontPrompt;
                            string message = $"保存书目记录 {strRecPath} 时出错:\r\n{strError}\r\n---\r\n\r\n是否重试?\r\n\r\n注：\r\n[重试] 重试保存\r\n[跳过] 放弃保存当前记录，但继续后面的处理\r\n[中断] 中断整批操作";
                            dialog_result = this.TryGet(() =>
                            {
                                return MessageDlg.Show(this,
                                message,
                                "BiblioSearchForm",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxDefaultButton.Button1,
                                ref temp,
                                new string[] { "重试", "跳过", "中断" },
                                "下次不再询问");
                            });
                            bDontPrompt = temp;
                        }
                        if (dialog_result == DialogResult.Yes)
                        {
                            nRedoCount++;
                            goto REDO_SAVE;
                        }
                        if (dialog_result == DialogResult.No)
                            continue;

                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError
                        };
                    }

                    // 检查是否有部分字段被拒绝
                    if (channel.ErrorCode == ErrorCode.PartialDenied)
                    {
                        DialogResult result = this.TryGet(() =>
                        {
                            return MessageBox.Show(this,
        "保存书目记录 " + strRecPath + " 时部分字段被拒绝。\r\n\r\n此记录已部分保存成功。\r\n\r\n请问现在是否要顺便重新装载此记录以便观察? \r\n\r\n(Yes 重新装载(到旧记录部分)；\r\nNo 不重新装载、但继续处理后面的记录保存; \r\nCancel 中断整批保存操作)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        });
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            break;
                        if (result == System.Windows.Forms.DialogResult.No)
                            goto CONTINUE;
                        // 重新装载书目记录到 OldXml
                        // byte[] baTimestamp = null;
                        lRet = channel.GetBiblioInfos(
                            looping.Progress,
                            strRecPath,
                            "",
                            new string[] { "xml" },   // formats
                            out string[] results,
                            out baNewTimestamp,
                            out strError);
                        if (lRet == 0)
                        {
                            // TODO: 警告后，把 item 行移除？
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = strError
                            };
                        }
                        if (lRet == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = strError
                            };
                        if (results == null || results.Length == 0)
                        {
                            strError = "results error";
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = strError
                            };
                        }
                        info.OldXml = results[0];
                        info.Timestamp = baNewTimestamp;
                        nReloadCount++;
                        goto CONTINUE;
                    }

                    info.Timestamp = baNewTimestamp;
                    info.OldXml = info.NewXml;
                    info.NewXml = "";

                    item.BackColor = SystemColors.Window;
                    item.ForeColor = SystemColors.WindowText;

                    // nSavedCount++;
                    saved_items.Add(item);

                    this.m_nChangedCount--;
                    Debug.Assert(this.m_nChangedCount >= 0, "");

                CONTINUE:
                    looping.Progress.SetProgressValue(i);
                }
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControlsInSearching(true);
                this.TryInvoke(() =>
                {
                    this.listView_records.Enabled = true;
                });
            }

            /*
            // 2013/10/22
            int nRet = RefreshListViewLines(saved_items,
        out strError);
            if (nRet == -1)
                return -1;
            */
            var ret = _refreshListViewLines(saved_items);
            if (ret.Value == -1)
                return ret;

            RefreshPropertyView(false);

            strError = "";
            if (saved_items.Count > 0)
                strError += "共保存书目记录 " + saved_items.Count + " 条";
            if (nReloadCount > 0)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    strError += " ; ";
                strError += "有 " + nReloadCount + " 条书目记录因为时间戳不匹配或部分字段被拒绝而重新装载旧记录部分(请观察后重新保存)";
            }

            return new NormalResult();
        }

        /// <summary>
        /// 刷新浏览行
        /// </summary>
        /// <param name="items_param">要刷新的 ListViewItem 集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int RefreshListViewLines(List<ListViewItem> items_param,
            out string strError)
        {
            var task = RefreshListViewLinesAsync(items_param);
            while (task.IsCompleted)
            {
                Application.DoEvents();
                Thread.Sleep(1);
            }
            var result = task.Result;
            strError = result.ErrorInfo;
            return result.Value;
        }

        Task<NormalResult> RefreshListViewLinesAsync(List<ListViewItem> items_param)
        {
            return Task.Factory.StartNew<NormalResult>(
                () =>
                {
                    try
                    {
                        return _refreshListViewLines(items_param);
                    }
                    catch (Exception ex)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"_refreshListViewLines() 异常: {ExceptionUtil.GetDebugText(ex)}"
                        };
                    }
                },
    default,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);
        }

        NormalResult _refreshListViewLines(List<ListViewItem> items_param)
        {
            string strError = "";

            if (items_param.Count == 0)
                return new NormalResult();

            LibraryChannel channel = this.GetChannel();

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在刷新浏览行 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在刷新浏览行 ...", "halfstop");

            this.EnableControlsInSearching(false);
            try
            {
                List<ListViewItem> items = new List<ListViewItem>();
                List<string> recpaths = new List<string>();
                foreach (ListViewItem item in items_param)
                {
                    var recpath = ListViewUtil.GetItemText(item, 0);
                    if (string.IsNullOrEmpty(recpath) == true
                        || recpath.StartsWith("error:"))
                        continue;
                    items.Add(item);
                    // recpaths.Add(recpath);

                    // 2023/4/3
                    var info = (BiblioInfo)this.m_biblioTable[recpath];

                    // 没有 BiblioInfo，或者处在未修改状态
                    if (info == null || info.Changed == false)
                        recpaths.Add(recpath);
                    else
                    {
                        Debug.Assert(info != null);
                        /*
                        if (info == null)
                        {
                            strError = $"在 biblioTable 缓存中没有找到路径为 '{recpath}' 的 XML 记录";
                            return -1;
                        }
                        */
                        string xml = info.NewXml;
                        if (string.IsNullOrEmpty(xml))
                            xml = info.OldXml;
                        recpaths.Add(recpath + ":" + xml);
                    }

                    // ClearOneChange(item, true);
                }

                if (looping.Progress != null)
                    looping.Progress.SetProgressRange(0, items.Count);

                BrowseLoader loader = new BrowseLoader();
                loader.Channel = channel;
                loader.Stop = looping.Progress;
                loader.RecPaths = recpaths;
                loader.Format = "id,cols";

                int i = 0;
                foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                {
                    if (this.InvokeRequired)
                        Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError
                        };
                    }

                    Debug.Assert(record.Path == BrowseLoader.GetPath(recpaths[i]), "");

                    if (looping.Progress != null)
                    {
                        looping.Progress.SetMessage("正在刷新浏览行 " + record.Path + " ...");
                        looping.Progress.SetProgressValue(i);
                    }

                    // TODO: 注意处理好 record.RecordBody.Result 带有出错信息的情形

                    ListViewItem item = items[i];
                    if (record.Cols == null)
                    {
                        int c = 0;
                        foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                        {
                            if (c != 0)
                                subitem.Text = "";
                            c++;
                        }
                    }
                    else
                    {
                        for (int c = 0; c < record.Cols.Length; c++)
                        {
                            ListViewUtil.ChangeItemText(item,
                            c + 1 + (m_bFirstColumnIsKey ? 1 : 0),
                            record.Cols[c]);
                        }

                        // TODO: 是否清除余下的列内容?
                    }

                    // 2021/9/28
                    // 确保列标题列数足够
                    ListViewUtil.EnsureColumns(item.ListView, item.SubItems.Count);

                    i++;
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "BiblioSearchForm RefreshListViewLines() exception: " + ExceptionUtil.GetAutoText(ex);
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControlsInSearching(true);
            }
        }

        // 清除一个事项的修改信息
        // parameters:
        //      bClearBiblioInfo    是否顺便清除事项的 BiblioInfo 信息
        void ClearOneChange(ListViewItem item,
            bool bClearBiblioInfo = false)
        {
            string strRecPath = item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return;

            BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
                return;

            if (String.IsNullOrEmpty(info.NewXml) == false)
            {
                info.NewXml = "";

                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;

                this.m_nChangedCount--;
                Debug.Assert(this.m_nChangedCount >= 0, "");
            }

            if (bClearBiblioInfo == true)
                this.m_biblioTable.Remove(strRecPath);
        }

        /// <summary>
        /// 刷新全部行
        /// </summary>
        public async Task RefreshAllLinesAsync()
        {
            await Task.Factory.StartNew(
            () =>
            {
                _refreshAllLines();
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        public void _refreshAllLines()
        {
            string strError = "";
            int nRet = 0;

            int nChangedCount = 0;
            List<ListViewItem> items = new List<ListViewItem>();
            var all_items = this.TryGet(() =>
            {
                return new List<ListViewItem>(this.listView_records.Items.Cast<ListViewItem>());
            });
            foreach (ListViewItem item in all_items/*this.listView_records.Items*/)
            {
                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;
                items.Add(item);

                if (IsItemChanged(item) == true)
                    nChangedCount++;
            }

            if (nChangedCount > 0)
            {
                DialogResult result = this.TryGet(() =>
                {
                    return MessageBox.Show(this,
        "要刷新的 " + this.listView_records.SelectedItems.Count.ToString() + " 个事项中有 " + nChangedCount.ToString() + " 项修改后尚未保存。如果刷新它们，修改内容会丢失。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
        "BiblioSearchForm",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                });
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
            }

            /*
            nRet = RefreshListViewLines(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            */
            var ret = _refreshListViewLines(items);
            if (ret.Value == -1)
            {
                strError = ret.ErrorInfo;
                goto ERROR1;
            }

            RefreshPropertyView(false);
            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }


        // 刷新所选择的浏览行。也就是重新从数据库中装载浏览列
        async void menu_refreshSelectedItems_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要刷新的浏览行";
                goto ERROR1;
            }

            // int nChangedCount = 0;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;
                items.Add(item);

                /*
                if (IsItemChanged(item) == true)
                    nChangedCount++;
                */
            }

            /*
            if (nChangedCount > 0)
            {
                DialogResult result = MessageBox.Show(this,
        "要刷新的 " + this.listView_records.SelectedItems.Count.ToString() + " 个事项中有 " + nChangedCount.ToString() + " 项修改后尚未保存。如果刷新它们，修改内容会丢失。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
        "BiblioSearchForm",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
            }
            */

            /*
            nRet = RefreshListViewLines(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            */
            var result = await RefreshListViewLinesAsync(items);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                goto ERROR1;
            }

            RefreshPropertyView(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 观察一个事项是否在内存中修改过
        bool IsItemChanged(ListViewItem item)
        {
            string strRecPath = ListViewUtil.GetItemText(item, 0);  //  item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return false;

            BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
                return false;

            if (string.IsNullOrEmpty(info.NewXml) == false)
                return true;

            return false;
        }

        // 在一个新开的书目查询窗内检索key
        async void listView_searchKeysAtNewWindow_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要操作的事项");
                return;
            }

            BiblioSearchForm form = new BiblioSearchForm();
            form.MdiParent = Program.MainForm;
            // form.MainForm = Program.MainForm;
            form.Show();

            ItemQueryParam query = (ItemQueryParam)this.listView_records.SelectedItems[0].Tag;
            Debug.Assert(query != null, "");

            ItemQueryParam input_query = new ItemQueryParam();

            input_query.QueryWord = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);
            input_query.DbNames = query.DbNames;
            input_query.From = query.From;
            input_query.MatchStyle = "精确一致";

            // 检索命中记录(而不是key)
            await form.DoSearch(false, false, input_query);
        }

        string m_strUsedMarcQueryFilename = "";

        // 装载书目以外的其它XML片断
        static int LoadXmlFragment(string strXml,
            out XmlDocument domXmlFragment,
            out string strError)
        {
            strError = "";

            domXmlFragment = null;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            nsmgr.AddNamespace("unimarc", DpNs.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//unimarc:leader | //unimarc:controlfield | //unimarc:datafield | //usmarc:leader | //usmarc:controlfield | //usmarc:datafield", nsmgr); // | //dprms:file
            foreach (XmlNode node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            domXmlFragment = new XmlDocument();
            domXmlFragment.LoadXml("<root />");
            domXmlFragment.DocumentElement.InnerXml = dom.DocumentElement.InnerXml;

            return 0;
        }

        // 创建一个新的 MarcQuery 脚本文件
        void menu_createMarcQueryCsFile_Click(object sender, EventArgs e)
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
                MarcQueryHost.CreateStartCsFile(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                return;
            }

            System.Diagnostics.Process.Start("notepad.exe", dlg.FileName);

            this.m_strUsedMarcQueryFilename = dlg.FileName;
        }

        internal int GetItemsChangeCount(List<ListViewItem> items)
        {
            if (this.m_nChangedCount == 0)
                return 0;   // 提高速度

            int nResult = 0;
            foreach (ListViewItem item in items)
            {
                if (IsItemChanged(item) == true)
                    nResult++;
            }
            return nResult;
        }

        int m_nChangedCount = 0;

        async void menu_quickMarcQueryRecords_Click(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        _runMarcQueryScript();
                    }
                    catch (Exception ex)
                    {
                        this.MessageBoxShow($"_runMarcQueryScript() 异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                },
                default,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        // https://learn.microsoft.com/en-us/dotnet/standard/assembly/unloadability
        void _runMarcQueryScript()
        {
            string strError = "";
            int nRet = 0;

            var selected_count = this.TryGet(() =>
            {
                return listView_records.SelectedItems.Count;
            });
            if (selected_count == 0)
            {
                strError = "尚未选择要执行 MarcQuery 脚本的事项";
                goto ERROR1;
            }

            // 书目信息缓存
            // 如果已经初始化，则保持
            if (this.m_biblioTable == null)
                this.m_biblioTable = new Hashtable();

            // this.m_biblioTable.Clear();

            OpenFileDialog dlg = this.TryGet(() =>
            {
                return new OpenFileDialog
                {
                    Title = "请指定 MarcQuery 脚本文件",
                    FileName = this.m_strUsedMarcQueryFilename,
                    Filter = "MarcQuery 脚本文件 (*.cs)|*.cs|All files (*.*)|*.*",
                    RestoreDirectory = true
                };
            });

            var dialog_result = this.TryGet(() =>
            {
                return dlg.ShowDialog();
            });
            if (dialog_result != DialogResult.OK)
                return;

            this.m_strUsedMarcQueryFilename = dlg.FileName;

            nRet = PrepareMarcQuery(this.m_strUsedMarcQueryFilename,
                out Assembly assembly,
                out MarcQueryHost host,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#if REMOVED
            var stream = new MemoryStream();
            nRet = PrepareMarcQuery(this.m_strUsedMarcQueryFilename,
    stream,
    out MarcQueryHost host,
    out strError);
            if (nRet == -1)
                goto ERROR1;


            var domain = CreateAppDomain();

            /*
            _stream = stream;
            domain.AssemblyResolve += Domain_AssemblyResolve; 
            */

            String name = Assembly.GetExecutingAssembly().GetName().FullName;
            var proxy = domain.CreateInstanceAndUnwrap(name, typeof(ScriptProxy).FullName) as ScriptProxy;
            host = proxy.Initialize(domain, stream);
            // host = Initialize(domain);
#endif

            host.CodeFileName = this.m_strUsedMarcQueryFilename;
            {
                // host.MainForm = Program.MainForm;
                host.UiForm = this;
                host.RecordPath = "";
                host.MarcRecord = null;
                host.MarcSyntax = "";
                host.Changed = false;
                host.UiItem = null;

                StatisEventArgs args = new StatisEventArgs();
                this.TryInvoke(host.UseUiThread, () =>
                {
                    host.OnInitial(this, args);
                });
                if (args.Continue == ContinueType.SkipAll)
                    return;
                if (args.Continue == ContinueType.Error)
                {
                    strError = args.ParamString;
                    goto ERROR1;
                }
            }

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行脚本 " + dlg.FileName + "</div>");

            LibraryChannel channel = this.GetChannel();

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在针对书目记录执行 MarcQuery 脚本 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在针对书目记录执行 MarcQuery 脚本 ...", "halfstop");

            this.EnableControls(false);

            EnableRecordList(false);
            try
            {
                if (looping.Progress != null)
                    looping.Progress.SetProgressRange(0, selected_count);

                {
                    host.MainForm = Program.MainForm;
                    host.RecordPath = "";
                    host.MarcRecord = null;
                    host.MarcSyntax = "";
                    host.Changed = false;
                    host.UiItem = null;

                    StatisEventArgs args = new StatisEventArgs();
                    this.TryInvoke(host.UseUiThread, () =>
                    {
                        host.OnBegin(this, args);
                    });
                    if (args.Continue == ContinueType.SkipAll)
                        return;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
                }

                var items = this.TryGet(() =>
                {
                    return this.listView_records.SelectedItems
                    .Cast<ListViewItem>()
                    .AsQueryable()
                    .Where(o => string.IsNullOrEmpty(o.Text) == false)
                    .ToList();
                });
                /*
                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }
                */

                bool bOldSource = true; // 是否要从 OldXml 开始做起

                int nChangeCount = this.GetItemsChangeCount(items);
                if (nChangeCount > 0)
                {
                    bool bHideMessageBox = true;
                    DialogResult result = MessageDialog.Show(this,
                        "当前选定的 " + items.Count.ToString() + " 个事项中有 " + nChangeCount + " 项修改尚未保存。\r\n\r\n请问如何进行修改? \r\n\r\n(重新修改) 重新进行修改，忽略以前内存中的修改; \r\n(继续修改) 以上次的修改为基础继续修改; \r\n(放弃) 放弃整个操作",
        MessageBoxButtons.YesNoCancel,
        MessageBoxDefaultButton.Button1,
        null,
        ref bHideMessageBox,
        new string[] { "重新修改", "继续修改", "放弃" });
                    if (result == DialogResult.Cancel)
                    {
                        // strError = "放弃";
                        return;
                    }
                    if (result == DialogResult.No)
                    {
                        bOldSource = false;
                    }
                }

                ListViewBiblioLoader loader = new ListViewBiblioLoader(channel, // this.Channel,
                    looping.Progress,
                    items,
                    // items.Count > MAX_CACHE_ITEMS ? null : this.m_biblioTable
                    this.m_biblioTable);
                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    // Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    looping.Progress.SetProgressValue(i);

                    BiblioInfo info = item.BiblioInfo;

                    string strXml = "";
                    if (bOldSource == true)
                    {
                        strXml = info.OldXml;
                        // 放弃上一次的修改
                        if (string.IsNullOrEmpty(info.NewXml) == false)
                        {
                            info.NewXml = "";
                            this.m_nChangedCount--;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(info.NewXml) == false)
                            strXml = info.NewXml;
                        else
                            strXml = info.OldXml;
                    }

                    // 将XML格式转换为MARC格式
                    // 自动从数据记录中获得MARC语法
                    nRet = MarcUtil.Xml2Marc(strXml,    // info.OldXml,
                        true,
                        null,
                        out string strMarcSyntax,
                        out string strMARC,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "XML转换到MARC记录时出错: " + strError;
                        goto ERROR1;
                    }

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    host.MainForm = Program.MainForm;
                    host.RecordPath = info.RecPath;
                    host.MarcRecord = new MarcRecord(strMARC);
                    host.MarcSyntax = strMarcSyntax;
                    host.Changed = false;
                    this.TryInvoke(() =>
                    {
                        host.UiItem = item.ListViewItem;
                    });
                    StatisEventArgs args = new StatisEventArgs();
                    this.TryInvoke(host.UseUiThread, () =>
                    {
                        host.OnRecord(this, args);
                    });
                    if (args.Continue == ContinueType.SkipAll)
                        break;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }

                    if (host.Changed == true)
                    {
                        strXml = info.OldXml;
                        nRet = MarcUtil.Marc2XmlEx(host.MarcRecord.Text,
                            strMarcSyntax,
                            ref strXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        if (info != null)
                        {
                            if (string.IsNullOrEmpty(info.NewXml) == true)
                                this.m_nChangedCount++;
                            info.NewXml = strXml;
                        }

                        this.TryInvoke(() =>
                        {
                            item.ListViewItem.BackColor = GlobalParameters.ChangedBackColor;    // SystemColors.Info;
                            item.ListViewItem.ForeColor = GlobalParameters.ChangedForeColor;     // SystemColors.InfoText;
                        });
                    }

                    // 显示为工作单形式
                    i++;
                }

                {
                    host.MainForm = Program.MainForm;
                    host.RecordPath = "";
                    host.MarcRecord = null;
                    host.MarcSyntax = "";
                    host.Changed = false;
                    host.UiItem = null;

                    StatisEventArgs args = new StatisEventArgs();
                    this.TryInvoke(host.UseUiThread, () =>
                    {
                        host.OnEnd(this, args);
                    });
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "执行 MarcQuery 脚本的过程中出现异常: " + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                if (host != null)
                    host.FreeResources();

                EnableRecordList(true);

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControls(true);

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行脚本 " + dlg.FileName + "</div>");

#if REMOVED
                DestoryAppDomain(domain);
#endif
            }

            RefreshPropertyView(false);
            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

        Stream _stream;

        private Assembly Domain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return null;
            Stream stream = _stream;
            Assembly assembly = null;
            stream.Seek(0, SeekOrigin.Begin);
            {
                byte[] buffer = new byte[(int)stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                // domain.Load(buffer);
                assembly = Assembly.Load(buffer);
            }

            return assembly;
        }

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
            if (e.Actions == "yes,no,cancel")
            {
#if NO
                DialogResult result = MessageBox.Show(this,
    e.MessageText + "\r\n\r\n是否重试操作?\r\n\r\n(是: 重试;  否: 跳过本次操作，继续后面的操作; 取消: 停止全部操作)",
    "ReportForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    e.ResultAction = "yes";
                else if (result == DialogResult.Cancel)
                    e.ResultAction = "cancel";
                else
                    e.ResultAction = "no";
#endif
                DialogResult result = this.TryGet(() =>
                {
                    return AutoCloseMessageBox.Show(this,
        e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
        20 * 1000,
        "BiblioSearchForm");
                });
                if (result == DialogResult.Cancel)
                    e.ResultAction = "no";
                else
                    e.ResultAction = "yes";
            }
        }

#if REMOVED

        AppDomain CreateAppDomain()
        {
            AppDomainSetup objSetup = new AppDomainSetup();
            objSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            // 打开 影像复制程序集 功能
            objSetup.ShadowCopyFiles = "true";
            // 虽然此方法已经被标记为过时方法， msdn备注也提倡不使用该方法，
            // 但是 以.net 4.0 + win10环境测试，还必须调用该方法 否则，即便卸载了应用程序域 dll 还是未被解除锁定
            // AppDomain.CurrentDomain.SetShadowCopyFiles();
            return AppDomain.CreateDomain("RemoteAppDomain", null, objSetup);
        }

        void DestoryAppDomain(AppDomain app_domain)
        {
            AppDomain.Unload(app_domain);
        }

        [Serializable]
        public class ScriptProxy : MarshalByRefObject
        {
            private Assembly _assembly;

            public MarcQueryHost Initialize(
                AppDomain domain)
            {
                Type entryClassType = null;
                // 得到Assembly中Host派生类Type
                foreach (var assembly in domain.GetAssemblies())
                {
                    entryClassType = ScriptManager.GetDerivedClassType(
                        assembly,
                        "dp2Circulation.MarcQueryHost");
                    if (entryClassType != null)
                        break;
                }
                if (entryClassType == null)
                {
                    throw new Exception("Assembly 中没有找到 dp2Circulation.MarcQueryHost 派生类");
                }

                return domain.CreateInstance("dp2Circulation.RemoteAppDomain", entryClassType.FullName).Unwrap() as MarcQueryHost;
            }

            public MarcQueryHost Initialize(
    AppDomain domain,
    Stream stream)
            {
                byte[] buffer = new byte[(int)stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                {
                    stream.Read(buffer, 0, (int)stream.Length);
                    // domain.Load(buffer);
                }

                Type entryClassType = null;
                var assembly = domain.Load(buffer);
                // 得到Assembly中Host派生类Type
                entryClassType = ScriptManager.GetDerivedClassType(
                    assembly,
                    "dp2Circulation.MarcQueryHost");

                if (entryClassType == null)
                {
                    throw new Exception("Assembly 中没有找到 dp2Circulation.MarcQueryHost 派生类");
                }

                /*
                var name = assembly.GetName().ToString();
                return domain.CreateInstance(name, entryClassType.FullName).Unwrap() as MarcQueryHost;
                */

                // new一个Host派生对象
                return (MarcQueryHost)entryClassType.InvokeMember(null,
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                    null);
            }

        }
#endif

        // 准备脚本环境
        // TODO: 检测同名的 .ref 文件
        int PrepareMarcQuery(string strCsFileName,
        out Assembly assembly,
        out MarcQueryHost host,
        out string strError)
        {
            assembly = null;
            strError = "";
            host = null;

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
                out string strContent,
                out Encoding encoding,
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
									Environment.CurrentDirectory + "\\digitalplatform.CommonControl.dll",  // 2015/11/20 新增
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
                "dp2Circulation.MarcQueryHost");
            if (entryClassType == null)
            {
                strError = strCsFileName + " 中没有找到 dp2Circulation.MarcQueryHost 派生类";
                goto ERROR1;
            }

            // new一个Host派生对象
            host = (MarcQueryHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            return 0;
        ERROR1:
            return -1;
        }

#if REMOVED
        static MarcQueryHost Initialize(
    AppDomain domain)
        {
            string assemblyName = "";
            Type entryClassType = null;
            // 得到Assembly中Host派生类Type
            foreach (var assembly in domain.GetAssemblies())
            {
                entryClassType = ScriptManager.GetDerivedClassType(
                    assembly,
                    "dp2Circulation.MarcQueryHost");
                if (entryClassType != null)
                {
                    assemblyName = assembly.GetName().ToString();
                    break;
                }
            }
            if (entryClassType == null)
            {
                throw new Exception("Assembly 中没有找到 dp2Circulation.MarcQueryHost 派生类");
            }

            return domain.CreateInstance(assemblyName, entryClassType.FullName).Unwrap() as MarcQueryHost;
        }

        int PrepareMarcQuery(string strCsFileName,
Stream stream,
out MarcQueryHost host,
out string strError)
        {
            strError = "";
            host = null;

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
                out string strContent,
                out Encoding encoding,
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
									Environment.CurrentDirectory + "\\digitalplatform.CommonControl.dll",  // 2015/11/20 新增
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
                Environment.CurrentDirectory + "\\dp2circulation.exe",
            };

            // 2013/12/16
            nRet = ScriptManager.GetRef(strCsFileName,
                ref saAddRef,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = ScriptUtility.CreateAssembly(
                strContent,
                saAddRef,
                stream,
                out strError,
                out strWarningInfo);
            if (nRet == -1)
                goto ERROR1;

            return 0;
        ERROR1:
            return -1;
        }

#endif


        string m_strUsedMarcFilterFilename = "";

        void menu_quickFilterRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要快速执行脚本的事项";
                goto ERROR1;
            }

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定 MARC 过滤器脚本文件";
            dlg.FileName = this.m_strUsedMarcFilterFilename;
            dlg.Filter = "MARC过滤器脚本文件 (*.fltx)|*.fltx|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedMarcFilterFilename = dlg.FileName;

            ColumnFilterDocument filter = new ColumnFilterDocument();

            nRet = PrepareMarcFilter(
                Program.MainForm.DataDir,
                this.m_strUsedMarcFilterFilename,
                filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            LibraryChannel channel = this.GetChannel();

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在针对书目记录执行 .fltx 脚本 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在针对书目记录执行 .fltx 脚本 ...", "halfstop");

            this.EnableControls(false);
            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 开始执行 .fltx 脚本</div>");
            this.listView_records.Enabled = false;
            try
            {
                if (looping.Progress != null)
                    looping.Progress.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }

                ListViewBiblioLoader loader = new ListViewBiblioLoader(channel, // this.Channel,
                    looping.Progress,
                    items,
                    // items.Count > MAX_CACHE_ITEMS ? null : this.m_biblioTable
                    this.m_biblioTable);
                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    BiblioInfo info = item.BiblioInfo;

                    string strXml = "";
                    {
                        if (string.IsNullOrEmpty(info.NewXml) == false)
                            strXml = info.NewXml;
                        else
                            strXml = info.OldXml;
                    }

                    // 将XML格式转换为MARC格式
                    // 自动从数据记录中获得MARC语法
                    nRet = MarcUtil.Xml2Marc(strXml,
                        true,
                        null,
                        out string strMarcSyntax,
                        out string strMARC,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "XML 转换到 MARC 记录时出错: " + strError;
                        goto ERROR1;
                    }

                    filter.Host = new ColumnFilterHost();
                    // 2022/11/10
                    filter.Host.RecPath = info.RecPath;
                    filter.Host.HostForm = this;
                    filter.Host.UiItem = item.ListViewItem;

                    filter.Host.ColumnTable = new System.Collections.Hashtable();
                    nRet = filter.DoRecord(
        null,
        strMARC,
        strMarcSyntax,
        i,
        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(item.BiblioInfo.RecPath) + "</div>");
                    foreach (string key in filter.Host.ColumnTable.Keys)
                    {
                        string strHtml = "<div>" + HttpUtility.HtmlEncode(key + "=" + (string)filter.Host.ColumnTable[key]) + "</div>";
                        Program.MainForm.OperHistory.AppendHtml(strHtml);
                    }

#if NO
                    if (nRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
        "刷新浏览内容时出错: " + strError + "。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
        "BiblioSearchForm",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            break;
                    }
#endif
                    i++;
                    looping.Progress.SetProgressValue(i);
                }
            }
            finally
            {
                this.listView_records.Enabled = true;
                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 结束执行 .fltx 脚本</div>");
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 准备脚本环境
        int PrepareMarcFilter(
            string strDataDir,
            string strFilterFileName,
            ColumnFilterDocument filter,
            out string strError)
        {
            strError = "";

            if (FileUtil.FileExist(strFilterFileName) == false)
            {
                strError = "文件 '" + strFilterFileName + "' 不存在";
                goto ERROR1;
            }

            string strWarning = "";

            string strLibPaths = "\"" + strDataDir + "\"";

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
                                         Environment.CurrentDirectory + "\\digitalplatform.core.dll",
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
            // 创建Script的Assembly
            // 本函数内对saRef不再进行宏替换
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saTotalFilterRef,
                strLibPaths,
                out Assembly assemblyFilter,
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
                this.MessageBoxShow(strWarning);
            }

            filter.Assembly = assemblyFilter;
            return 0;
        ERROR1:
            return -1;
        }

        // TODO: 应该改为直接在内存修改，并不直接保存，要专门保存命令才真正保存
        void menu_quickChangeRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要快速修改的事项";
                goto ERROR1;
            }

            LibraryChannel channel = this.GetChannel();

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在修改书目记录 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在修改书目记录 ...", "halfstop");

            this.EnableControls(false);

            this.listView_records.Enabled = false;
            try
            {
                StringBuilder strLines = new StringBuilder(4096);

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    strLines.Append(item.Text + "\r\n");
                }

                // 新打开一个快速修改书目窗口
                QuickChangeBiblioForm form = new QuickChangeBiblioForm();
                // form.MainForm = Program.MainForm;
                form.MdiParent = Program.MainForm;
                form.Show();

                form.RecPathLines = strLines.ToString();
                if (form.SetChangeParameters() == false)
                {
                    form.Close();
                    return;
                }

                // return:
                //      -1  出错
                //      0   放弃处理
                //      1   正常结束
                nRet = form.DoRecPathLines();
                form.Close();

                if (nRet == 0)
                    return;

                if (nRet == -1)
                {
                    DialogResult result = MessageBox.Show(this,
        "部分记录已经发生了修改，是否需要刷新浏览行? (OK 刷新；Cancel 放弃刷新)",
        "BiblioSearchForm",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.Cancel)
                        return;
                }

                if (looping.Progress != null)
                    looping.Progress.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    if (looping.Progress != null)
                    {
                        looping.Progress.SetMessage("正在刷新浏览行 " + item.Text + " ...");
                        looping.Progress.SetProgressValue(i++);
                    }

                    // 2017/3/8
                    ClearOneChange(item, true);

                    // TODO: 还应该把 BiblioInfo 失效，迫使固定面板区属性显示 XML 时候重新获取书目记录
                    nRet = RefreshBrowseLine(
                        channel,
                        item,
                        out strError);
                    if (nRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
        "刷新浏览内容时出错: " + strError + "。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
        "BiblioSearchForm",
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
                this.listView_records.Enabled = true;

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControls(true);
            }

            // 如果正好修改了处于选择状态的一行
            RefreshPropertyView(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 保存到其它书目库
        // parameters:
        //      bCopy   是否为复制。如果是 false，表示移动
        // return:
        //      -1  出错
        //      0   放弃
        //      1   成功
        int CopyToAnotherDatabase(
            bool bCopy,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            string strActionName = "";
            if (bCopy)
                strActionName = "复制";
            else
                strActionName = "移动";

            // 选择目标库名，还有选定是否要将下属记录也保存过去
            // 需要询问保存的路径
            BiblioSaveToDlg dlg = new BiblioSaveToDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            // dlg.MainForm = Program.MainForm;
            dlg.Text = strActionName + "书目记录到数据库";

            dlg.MessageText = "请指定书目记录要追加" + strActionName + "到的位置";
            dlg.EnableCopyChildRecords = true;

            dlg.BuildLink = false;

            if (bCopy)
                dlg.CopyChildRecords = false;
            else
                dlg.CopyChildRecords = true;

            if (bCopy)
                dlg.MessageText += "\r\n\r\n(注：本功能*可选择*是否复制书目记录下属的册、期、订购、实体记录和对象资源)\r\n\r\n将选定的书目记录复制到:";
            else
            {
                dlg.MessageText += "\r\n\r\n注：\r\n1) 当前执行的是移动而不是复制操作;\r\n2) 书目记录下属的册、期、订购、实体记录和对象资源会被一并移动到目标位置";
                dlg.EnableCopyChildRecords = false;
            }
            // TODO: 要让记录ID为问号，并且不可改动

            dlg.CurrentBiblioRecPath = "";  // 源记录路径？ 但是批处理情况下源记录路径并不确定阿
            Program.MainForm.AppInfo.LinkFormState(dlg, "BiblioSearchform_BiblioSaveToDlg_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            if (Global.IsAppendRecPath(dlg.RecPath) == false)
            {
                strError = "目标记录路径 '" + dlg.RecPath + "' 不合法。必须是追加方式的路径，也就是说 ID 部分必须为问号";
                goto ERROR1;
            }
            // TODO: 如果源和目标库名相同，要警告

            string strAction = "";
            if (bCopy)
            {
                if (dlg.CopyChildRecords == false)
                    strAction = "onlycopybiblio";
                else
                    strAction = "copy";
            }
            else
            {
                if (dlg.CopyChildRecords == false)
                    strAction = "onlymovebiblio";
                else
                    strAction = "move";
            }

            bool bHideMessageBox = false;
            bool bHideMessageBox1 = false;
            DialogResult copy_result = DialogResult.Cancel;

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在" + strActionName + "书目记录到数据库 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在" + strActionName + "书目记录到数据库 ...", "halfstop");

            this.EnableControlsInSearching(false);
            this.listView_records.Enabled = false;
            try
            {
                looping.Progress.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                List<ListViewItem> moved_items = new List<ListViewItem>();
                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strRecPath = ListViewUtil.GetItemText(item, 0);

                    if (string.IsNullOrEmpty(strRecPath) == true
                        || IsCmdLine(strRecPath) == true)
                        continue;

                    // 观察源记录是否有998$t ?

                    // 是否要自动创建998字段内容?

                    string strOutputBiblioRecPath = "";
                    byte[] baOutputTimestamp = null;
                    string strOutputBiblio = "";

                    looping.Progress.SetMessage("正在" + strActionName + "书目记录 '" + strRecPath + "' 到 '" + dlg.RecPath + "' ...");

                    // 2016/3/23
                    channel.Timeout = new TimeSpan(0, 40, 0);   // 复制的时候可能需要复制对象，所需时间一般很长

                REDO:
                    long lRet = 0;
                    if (strRecPath.IndexOf("@") != -1)
                    {
                        // 如果是移动操作，需要警告一下操作被转换为复制执行
                        if (bCopy == false)
                        {
                            if (bHideMessageBox1 == false)
                            {
                                copy_result = MessageDialog.Show(this,
                "不能移动书目记录 '" + strRecPath + " --> " + dlg.RecPath + "'(注: 可以用复制方式保存)。\r\n\r\n是否改为复制方式保存? (Yes 改为复制方式保存; No 跳过此条、继续后面处理；Cancel 放弃未完成的操作)",
                MessageBoxButtons.YesNoCancel,
                MessageBoxDefaultButton.Button1,
                "不再出现此对话框",
                ref bHideMessageBox1,
                new string[] { "改为复制方式", "跳过此条继续", "放弃" });
                            }

                            if (copy_result == System.Windows.Forms.DialogResult.Yes)
                            {
                                // 要保存此条
                            }
                            else if (copy_result == System.Windows.Forms.DialogResult.No)
                                goto CONTINUE;
                            else
                            {
                                strError = "中断处理";
                                goto ERROR1;
                            }
                        }

                        BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
                        if (info == null)
                            goto CONTINUE;
                        string strXml = info.NewXml;
                        if (string.IsNullOrEmpty(strXml))
                            strXml = info.OldXml;
                        if (string.IsNullOrEmpty(strXml) == true)
                            goto CONTINUE;
                        lRet = channel.SetBiblioInfo(
        looping.Progress,
        "new",
        dlg.RecPath,
        "xml",
        strXml,
        info.Timestamp,
        "",
        out string strOutputPath,
        out byte[] baNewTimestamp,
        out strError);
                        info.Timestamp = baNewTimestamp;
                        if (lRet != -1)
                        {
                            if (info.NewXml == strXml)
                            {
                                info.OldXml = strXml;
                                info.NewXml = "";
                            }
                        }
                    }
                    else
                    {
                        // result.Value:
                        //      -1  出错
                        //      0   成功，没有警告信息。
                        //      1   成功，有警告信息。警告信息在 result.ErrorInfo 中
                        lRet = channel.CopyBiblioInfo(
                            looping.Progress,
                            strAction,
                            strRecPath,
                            "xml",
                            null,
                            null,    // this.BiblioTimestamp,
                            dlg.RecPath,
                            null,   // strXml,
                            "file_reserve_source",  // 2017/4/19
                            out strOutputBiblio,
                            out strOutputBiblioRecPath,
                            out baOutputTimestamp,
                            out strError);
                    }
                    if (lRet == -1)
                    {
                        /*
                        DialogResult result = MessageBox.Show(this,
        "复制或移动书目记录 '" + strRecPath + " --> " + dlg.RecPath + "' 时出现错误: " + strError + "。\r\n\r\n是否重试? (Yes 重试；No 跳过此条、继续后面处理；Cancel 放弃未完成的操作)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
        */
                        DialogResult result = DialogResult.Cancel;

                        if (bHideMessageBox == false)
                        {
                            // TODO: 提示中出现书目记录的摘要信息？
                            string temp = strError;
                            result = MessageDialog.Show(this,
            "复制或移动书目记录 '" + strRecPath + " --> " + dlg.RecPath + "' 时出现错误: " + temp + "。\r\n\r\n是否重试? (Yes 重试；No 跳过此条、继续后面处理；Cancel 放弃未完成的操作)",
            MessageBoxButtons.YesNoCancel,
            MessageBoxDefaultButton.Button1,
            "不再出现此对话框",
            ref bHideMessageBox,
            new string[] { "重试", "跳过此条继续", "放弃" });
                        }
                        else
                            result = DialogResult.No;

                        if (result == System.Windows.Forms.DialogResult.Yes)
                            goto REDO;
                        if (result == System.Windows.Forms.DialogResult.No)
                            goto CONTINUE;
                        strError = $"已放弃处理。原因: {strError}";
                        goto ERROR1;
                    }

                    if (bCopy == false)
                        moved_items.Add(item);

                    CONTINUE:
                    looping.Progress.SetProgressValue(++i);
                }

                foreach (ListViewItem item in moved_items)
                {
                    item.Remove();
                }
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                this.ReturnChannel(channel);

                this.EnableControlsInSearching(true);
                this.listView_records.Enabled = true;
            }

            return 1;
        ERROR1:
            // MessageBox.Show(this, strError);
            return -1;
        }

        // 保存到其它书目库
        void menu_saveBiblioRecToAnotherDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 保存到其它书目库
            // parameters:
            //      bCopy   是否为复制。如果是 false，表示移动
            // return:
            //      -1  出错
            //      0   放弃
            //      1   成功
            nRet = CopyToAnotherDatabase(
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 移动到其它书目库
        void menu_moveBiblioRecToAnotherDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 保存到其它书目库
            // parameters:
            //      bCopy   是否为复制。如果是 false，表示移动
            // return:
            //      -1  出错
            //      0   放弃
            //      1   成功
            nRet = CopyToAnotherDatabase(
                false,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_maskEmptySubItems_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            int nRet = GetEmptySubItems("item",
                out items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            ListViewUtil.ClearSelection(this.listView_records);
            foreach (ListViewItem item in items)
            {
                item.Selected = true;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_maskEmptySubOrders_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            int nRet = GetEmptySubItems("order",
                out items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            ListViewUtil.ClearSelection(this.listView_records);
            foreach (ListViewItem item in items)
            {
                item.Selected = true;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_maskEmptySubIssues_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            int nRet = GetEmptySubItems("issue",
                out items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            ListViewUtil.ClearSelection(this.listView_records);
            foreach (ListViewItem item in items)
            {
                item.Selected = true;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_maskEmptySubComments_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            int nRet = GetEmptySubItems("comment",
                out items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            ListViewUtil.ClearSelection(this.listView_records);
            foreach (ListViewItem item in items)
            {
                item.Selected = true;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_exportToBatchOrderForm_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要装入其它查询窗的行";
                goto ERROR1;
            }

            BatchOrderForm form = new BatchOrderForm();
            form.MdiParent = Program.MainForm;
            form.Show();

            form.EnableControls(false);
            try
            {
                List<string> recpaths = new List<string>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    string strRecPath = ListViewUtil.GetItemText(item, 0);
                    // form.AddBiblio(strRecPath);
                    recpaths.Add(strRecPath);
                }
                int nRet = form.LoadLines(
                    recpaths,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                // form.BeginLoadLine(recpaths);
            }
            finally
            {
                form.EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_exportToBiblioSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要装入其它查询窗的行";
                goto ERROR1;
            }

            BiblioSearchForm form = new BiblioSearchForm();
            form.MdiParent = Program.MainForm;
            form.Show();

            form.EnableControls(false);
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                string strLine = Global.BuildLine(item);
                form.AddLineToBrowseList(strLine);
            }
            form.EnableControls(true);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_exportToItemSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = ExportToItemSearchForm("item",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }



        void menu_exportToOrderSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = ExportToItemSearchForm("order",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_exportToIssueSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = ExportToItemSearchForm("issue",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_exportToCommentSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = ExportToItemSearchForm("comment",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_exportTo856SearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = ExportTo856SearchForm(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 将所选择的 ? 个书目记录下属的册记录路径导出到(实体库)记录路径文件
        void menu_saveToEntityRecordPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = SaveToEntityRecordPathFile("item",
                null,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_saveToOrderRecordPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = SaveToEntityRecordPathFile("order",
                null,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_saveToIssueRecordPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = SaveToEntityRecordPathFile("issue",
                null,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_saveToCommentRecordPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = SaveToEntityRecordPathFile("comment",
                null,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int ExportToItemSearchForm(
        string strDbType,
        out string strError)
        {
            strError = "";
            string strTempFileName = Path.Combine(Program.MainForm.UserTempDir, // Program.MainForm.DataDir, 
                "~export_to_searchform.txt");
            int nRet = SaveToEntityRecordPathFile(strDbType,
                strTempFileName,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            // TODO: 最好为具体类型的 SearchForm 类。否则推出时保留的遗留出鞥口类型不对
            ItemSearchForm form = new ItemSearchForm();
            form.DbType = strDbType;
            form.MdiParent = Program.MainForm;
            form.Show();
#endif
            ItemSearchForm form = Program.MainForm.OpenItemSearchForm(strDbType);

            nRet = form.ImportFromRecPathFile(strTempFileName,
                "clear",
                out strError);
            if (nRet == -1)
                return -1;
            return 0;
        }

        int ExportTo856SearchForm(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要导出的事项";
                return -1;
            }

            Marc856SearchForm form = new Marc856SearchForm();
            form.MdiParent = Program.MainForm;
            form.MainForm = Program.MainForm;
            form.Show();

            this.EnableControls(false);

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在导出到 MARC 文件 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在导出到 MARC 文件 ...");


            try
            {
                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }

                ListViewBiblioLoader loader = new ListViewBiblioLoader(channel, // this.Channel,
        looping.Progress,
        items,
        items.Count > MAX_CACHE_ITEMS ? null : this.m_biblioTable);
                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                List<ListViewItem> new_items = new List<ListViewItem>();

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    string strBiblioSummary = ListViewUtil.GetItemText(item.ListViewItem, 1);

                    BiblioInfo info = item.BiblioInfo;

                    string strXml = "";
                    {
                        if (string.IsNullOrEmpty(info.NewXml) == false)
                            strXml = info.NewXml;
                        else
                            strXml = info.OldXml;
                    }

                    string strMARC = "";
                    string strMarcSyntax = "";
                    // 将XML格式转换为MARC格式
                    // 自动从数据记录中获得MARC语法
                    nRet = MarcUtil.Xml2Marc(strXml,
                        true,
                        null,
                        out strMarcSyntax,
                        out strMARC,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "XML转换到MARC记录时出错: " + strError;
                        return -1;
                    }

                    MarcRecord record = new MarcRecord(strMARC);
                    MarcNodeList fields = record.select("field[@name='856']");
                    if (fields.count == 0)
                        goto CONTINUE;

                    int index = 0;
                    foreach (MarcField field in fields)
                    {
                        ListViewItem new_item = form.AddLine(info.RecPath,
                            info,
                            field,
                            index);
                        new_items.Add(new_item);
                        index++;
                    }

                CONTINUE:
                    looping.Progress.SetProgressValue(++i);
                }

                nRet = form.FillBiblioSummaryColumn(
                    looping.Progress,
                    channel,
                    new_items,
                    0,
                    true,
                    // true,
                    out strError);
                if (nRet == -1)
                    return -1;
                return 0;
            }
            catch (Exception ex)
            {
                strError = "导出 856 字段的过程出现异常: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                */
                EndLoop(looping);

                this.ReturnChannel(channel);

                this.EnableControls(true);
            }
        }

        int SaveToEntityRecordPathFile(
            string strDbType,
            string strFileName,
            out string strError)
        {
            strError = "";
            int nCount = 0;
            int nRet = 0;

            string strDbTypeName = "";
            if (strDbType == "item")
                strDbTypeName = "实体";
            else if (strDbType == "order")
                strDbTypeName = "订购";
            else if (strDbType == "issue")
                strDbTypeName = "期";
            else if (strDbType == "comment")
                strDbTypeName = "评注";

            bool bAppend = true;

            if (string.IsNullOrEmpty(strFileName) == true)
            {
                // 询问文件名
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.Title = "请指定要保存的(" + strDbTypeName + "库)记录路径文件名";
                dlg.CreatePrompt = false;
                dlg.OverwritePrompt = false;
                dlg.FileName = this.ExportEntityRecPathFilename;
                // dlg.InitialDirectory = Environment.CurrentDirectory;
                dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";

                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return 0;

                this.ExportEntityRecPathFilename = dlg.FileName;

                if (File.Exists(this.ExportEntityRecPathFilename) == true)
                {
                    DialogResult result = this.TryGet(() =>
                    {
                        return MessageBox.Show(this,
                        "记录路径文件 '" + this.ExportEntityRecPathFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                        "BiblioSearchForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    });
                    if (result == DialogResult.Cancel)
                        return 0;
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
            }
            else
            {
                this.ExportEntityRecPathFilename = strFileName;

                bAppend = false;
            }

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获得记录路径 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在获得记录路径 ...", "halfstop");

            this.EnableControlsInSearching(false);
            this.listView_records.Enabled = false;

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportEntityRecPathFilename,
                bAppend,    // append
                System.Text.Encoding.UTF8);
            try
            {
                looping.Progress.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strBiblioRecPath = item.Text;

                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                        continue;

                    List<string> recpaths = null;

                    if (strDbType == "item")
                    {
                        // 获得一个书目记录下属的全部实体记录路径
                        // return:
                        //      -1  出错
                        //      0   没有装载
                        //      1   已经装载
                        nRet = EntityControl.GetEntityRecPaths(
                            looping.Progress,
                            channel,    // this.Channel,
                            strBiblioRecPath,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else if (strDbType == "order")
                    {
                        // return:
                        //      -1  出错
                        //      0   没有装载
                        //      1   已经装载
                        nRet = OrderControl.GetOrderRecPaths(
                            looping.Progress,
                            channel,    // this.Channel,
                            strBiblioRecPath,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else if (strDbType == "issue")
                    {
                        // return:
                        //      -1  出错
                        //      0   没有装载
                        //      1   已经装载
                        nRet = IssueControl.GetIssueRecPaths(
                            looping.Progress,
                            channel,    // this.Channel,
                            strBiblioRecPath,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else if (strDbType == "comment")
                    {
                        // return:
                        //      -1  出错
                        //      0   没有装载
                        //      1   已经装载
                        nRet = CommentControl.GetCommentRecPaths(
                            looping.Progress,
                            channel,    // this.Channel,
                            strBiblioRecPath,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    nCount += recpaths.Count;
                    foreach (string recpath in recpaths)
                    {
                        sw.WriteLine(recpath);
                    }

                    looping.Progress.SetProgressValue(++i);
                }
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                this.ReturnChannel(channel);

                this.EnableControlsInSearching(true);
                this.listView_records.Enabled = true;

                if (sw != null)
                    sw.Close();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            Program.MainForm.StatusBarMessage = strDbTypeName + "记录记录路径 " + nCount.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportEntityRecPathFilename;
            return 1;
        ERROR1:
            return -1;
        }

        int GetEmptySubItems(
            string strDbType,
            out List<ListViewItem> items,
            out string strError)
        {
            strError = "";
            int nCount = 0;
            int nRet = 0;

            items = new List<ListViewItem>();

            string strDbTypeName = "";
            if (strDbType == "item")
                strDbTypeName = "实体";
            else if (strDbType == "order")
                strDbTypeName = "订购";
            else if (strDbType == "issue")
                strDbTypeName = "期";
            else if (strDbType == "comment")
                strDbTypeName = "评注";

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获得记录路径 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在获得记录路径 ...", "halfstop");

            this.EnableControlsInSearching(false);
            this.listView_records.Enabled = false;

            try
            {
                looping.Progress.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strBiblioRecPath = item.Text;

                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                        continue;

                    List<string> recpaths = null;

                    if (strDbType == "item")
                    {
                        // 获得一个书目记录下属的全部实体记录路径
                        // return:
                        //      -1  出错
                        //      0   没有装载
                        //      1   已经装载
                        nRet = EntityControl.GetEntityRecPaths(
                            looping.Progress,
                            channel,    // this.Channel,
                            strBiblioRecPath,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else if (strDbType == "order")
                    {
                        // return:
                        //      -1  出错
                        //      0   没有装载
                        //      1   已经装载
                        nRet = OrderControl.GetOrderRecPaths(
                            looping.Progress,
                            channel,    // this.Channel,
                            strBiblioRecPath,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else if (strDbType == "issue")
                    {
                        // return:
                        //      -1  出错
                        //      0   没有装载
                        //      1   已经装载
                        nRet = IssueControl.GetIssueRecPaths(
                            looping.Progress,
                            channel,    // this.Channel,
                            strBiblioRecPath,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else if (strDbType == "comment")
                    {
                        // return:
                        //      -1  出错
                        //      0   没有装载
                        //      1   已经装载
                        nRet = CommentControl.GetCommentRecPaths(
                            looping.Progress,
                            channel,    // this.Channel,
                            strBiblioRecPath,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    if (recpaths.Count == 0)
                    {
                        items.Add(item);
                        nCount++;
                    }
                    looping.Progress.SetProgressValue(++i);
                }
            }

            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                this.ReturnChannel(channel);

                this.EnableControlsInSearching(true);
                this.listView_records.Enabled = true;
            }

            Program.MainForm.StatusBarMessage = "统计出下级" + strDbTypeName + "记录为空的书目记录 " + nCount.ToString() + "个";
            return 1;
        ERROR1:
            return -1;
        }

        // 从记录路径文件中导入
        async void menu_importFromRecPathFile_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的书目记录路径文件名";
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

            string strLibraryUrl = StringUtil.CanonicalizeHostUrl(Program.MainForm.LibraryServerUrl);

            // 需要刷新的行
            List<ListViewItem> items = new List<ListViewItem>();

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在导入记录路径 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在导入记录路径 ...", "halfstop");


            this.EnableControlsInSearching(false);
            BeginUpdate();
            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this.listView_records);
                looping.Progress.SetProgressRange(0, sr.BaseStream.Length);

                if (this.listView_records.Items.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "导入前是否要清除命中记录列表中的现有的 " + this.listView_records.Items.Count.ToString() + " 行?\r\n\r\n(如果不清除，则新导入的行将追加在已有行后面)\r\n\r\n(Yes 清除；No 不清除(追加)；Cancel 放弃导入)",
                        "BiblioSearchForm",
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

                for (; ; )
                {
                    if (this.InvokeRequired)
                        Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        MessageBox.Show(this, "用户中断");
                        return;
                    }

                    string strLine = sr.ReadLine();

                    looping.Progress.SetProgressValue(sr.BaseStream.Position);

                    if (strLine == null)
                        break;

                    strLine = strLine.Trim();
                    if (string.IsNullOrEmpty(strLine) == true)
                        continue;

                    string strRecPath = "";
                    bool bOtherCols = false;
                    nRet = strLine.IndexOf("\t");
                    if (nRet == -1)
                        strRecPath = strLine;
                    else
                    {
                        strRecPath = strLine.Substring(0, nRet);
                        bOtherCols = true;
                    }

                    // 兼容长路经
                    if (strRecPath.IndexOf("@") != -1)
                    {
                        string strPureRecPath = "";
                        string strUrl = "";
                        ParseLongRecPath(strRecPath,
                            out strUrl,
                            out strPureRecPath);
                        string strUrl0 = StringUtil.CanonicalizeHostUrl(strUrl);

                        if (string.Compare(strUrl0, strLibraryUrl, true) == 0)
                            strRecPath = strPureRecPath;
                        else
                        {
                            strError = "长路径 '" + strRecPath + "' 中的服务器 URL 部分 '" + strUrl + "' 和当前 dp2Circulation 服务器 URL '" + Program.MainForm.LibraryServerUrl + "' 不匹配，因此无法导入这个记录路径文件";
                            goto ERROR1;
                        }
                    }

                    // 检查路径的正确性，检查数据库是否为书目库之一
                    // 判断它是书目记录路径，还是实体记录路径？
                    string strDbName = Global.GetDbName(strRecPath);
                    if (string.IsNullOrEmpty(strDbName) == true)
                    {
                        strError = "'" + strRecPath + "' 不是合法的记录路径";
                        goto ERROR1;
                    }

                    if (Program.MainForm.IsBiblioDbName(strDbName) == false)
                    {
                        strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的书目库名。很可能所指定的文件不是书目库的记录路径文件";
                        goto ERROR1;
                    }

                    ListViewItem item = null;

                    if (bOtherCols == true)
                    {
                        item = Global.BuildListViewItem(
                            this.listView_records,
                            strLine);
                        this.listView_records.Items.Add(item);
                    }
                    else
                    {
                        item = new ListViewItem();
                        item.Text = strRecPath;

                        this.listView_records.Items.Add(item);

                        items.Add(item);
                    }
                }
            }
            finally
            {
                EndUpdate();

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                this.EnableControlsInSearching(true);

                if (sr != null)
                    sr.Close();
            }

            if (items.Count > 0)
            {
                /*
                nRet = RefreshListViewLines(items,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                */
                var result = await RefreshListViewLinesAsync(items);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        static void ParseLongRecPath(string strRecPath,
        out string strServerName,
        out string strPath)
        {
            int nRet = strRecPath.IndexOf("@");
            if (nRet == -1)
            {
                strServerName = "";
                strPath = strRecPath;
                return;
            }
            strServerName = strRecPath.Substring(nRet + 1).Trim();
            strPath = strRecPath.Substring(0, nRet).Trim();
        }

        // 兼容以前 API
        public int RefreshBrowseLine(
            ListViewItem item,
            out string strError)
        {
            LibraryChannel channel = this.GetChannel();
            try
            {
                return RefreshBrowseLine(
                    channel,
                    item,
                    out strError);
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        // 调用前，记录路径列已经有值
        /// <summary>
        /// 刷新一个浏览行。调用前，记录路径列已经有值
        /// </summary>
        /// <param name="channel">通讯通道</param>
        /// <param name="item">浏览行 ListViewItem 对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int RefreshBrowseLine(
            LibraryChannel channel,
            ListViewItem item,
            out string strError)
        {
            strError = "";

            string strRecPath = ListViewUtil.GetItemText(item, 0);
            string[] paths = new string[1];
            paths[0] = strRecPath;
            DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

            long lRet = channel.GetBrowseRecords(
                null,   // this._stop,
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

        void menu_reverseSelectAllLines_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                this.listView_records.SelectedIndexChanged -= new System.EventHandler(this.listView_records_SelectedIndexChanged);

                foreach (ListViewItem item in this.ListViewRecords.Items)
                {
                    if (item.Selected == true)
                        item.Selected = false;
                    else
                        item.Selected = true;
                }

                this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
                listView_records_SelectedIndexChanged(null, null);
            }
            finally
            {
                this.Cursor = oldCursor;
            }
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

        void menu_refreshControls_Click(object sender, EventArgs e)
        {
            Global.InvalidateAllControls(this);
        }

        // 删除所选择的书目记录
        async void menu_deleteSelectedRecords_Click(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        _deleteSelectedRecords();
                    }
                    catch (Exception ex)
                    {
                        this.MessageBoxShow($"_deleteSelectedRecords() 异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                },
    default,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);
        }

        void _deleteSelectedRecords()
        {
            string delete_style = "";
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                delete_style = "noeventlog";

            DialogResult result = this.TryGet(() =>
            {
                return MessageBox.Show(this,
        $"确实要从数据库中删除所选定的 {this.listView_records.SelectedItems.Count.ToString()} 个书目记录?\r\n\r\n{(string.IsNullOrEmpty(delete_style) == false ? "style:" + delete_style : "")}\r\n\r\n(警告：书目记录被删除后，无法恢复。如果删除书目记录，则其下属的册、期、订购、评注记录和对象资源会一并删除)\r\n\r\n(OK 删除；Cancel 取消)",
        "BiblioSearchForm",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
            });
            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            this.TryInvoke(() =>
            {
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    items.Add(item);
                }
            });

            string strError = "";
            int nDeleteCount = 0;

            // 检查前端权限
            bool bDeleteSub = StringUtil.IsInList("client_deletebibliosubrecords",
                // this.Channel.Rights
                Program.MainForm.GetCurrentUserRights()
                );

            // bool bDontPrompt = false;
            DialogResult dialog_result = DialogResult.Yes;  // yes no cancel
            List<string> skipped_recpath = new List<string>();

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在删除书目记录 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在删除书目记录 ...", "halfstop");


            this.EnableControlsInSearching(false);
            EnableRecordList(false);
            try
            {
                looping.Progress.SetProgressRange(0, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    if (looping.Stopped)
                    {
                        strError = "已中断";
                        goto ERROR1;
                    }

                    ListViewItem item = items[i];
                    string strRecPath = ListViewUtil.GetItemText(item, 0);

                    string[] results = null;
                    byte[] baTimestamp = null;
                    string strOutputPath = "";
                    string[] formats = null;
                    if (bDeleteSub == false
                        && StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.30") >= 0)
                    {
                        formats = new string[1];
                        formats[0] = "subcount";
                    }

                    looping.Progress.SetMessage("正在删除书目记录 " + strRecPath);

                    int nRedoCount = 0;
                REDO_LOAD:
                    long lRet = channel.GetBiblioInfos(
                        looping.Progress,
                        strRecPath,
                        "",
                        formats,   // formats
                        out results,
                        out baTimestamp,
                        out strError);
                    if (lRet == 0)
                    {
                        // 记录不存在。接着处理后面的删除
                        skipped_recpath.Add(strRecPath);
                        continue;
                        // goto ERROR1;
                    }
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ErrorCode.NotFound)
                        {
                            skipped_recpath.Add(strRecPath);
                            continue;
                        }

                        string message = $"在获得记录 {strRecPath} 时间戳时出错:\r\n{strError}\r\n---\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)";
                        dialog_result = this.TryGet(() =>
                        {
                            // return:
                            //      DialogResult.Retry 表示超时了
                            //      DialogResult.OK 表示点了 OK 按钮
                            //      DialogResult.Cancel 表示点了右上角的 Close 按钮
                            return AutoCloseMessageBox.Show(this,
message,
20 * 1000,
"BiblioSearchForm");
                        });
                        if (dialog_result != DialogResult.Cancel)
                        {
                            // 重试次数太多了还不行，就跳过
                            if (nRedoCount >= 10)
                            {
                                skipped_recpath.Add(strRecPath);
                                continue;
                            }

                            nRedoCount++;
                            goto REDO_LOAD;
                        }

                        goto ERROR1;
                        /*
                        result = MessageBox.Show(this,
        "在获得记录 '" + strRecPath + "' 的时间戳的过程中出现错误: " + strError + "。\r\n\r\n是否继续强行删除此记录? (Yes 强行删除；No 不删除；Cancel 放弃当前未完成的全部删除操作)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            goto ERROR1;
                        if (result == System.Windows.Forms.DialogResult.No)
                            continue;
                        */
                    }

                    if (bDeleteSub == false)
                    {
                        string strSubCount = "";
                        if (results != null && results.Length > 0)
                            strSubCount = results[0];

                        if (string.IsNullOrEmpty(strSubCount) == true || strSubCount == "0")
                        {
                        }
                        else
                        {
                            result = this.TryGet(() =>
                            {
                                return MessageBox.Show(this,
        "书目记录 '" + strRecPath + "' 包含 " + strSubCount + " 个下级记录，而当前用户并不具备 client_deletebibliosubrecords 权限，无法删除这条书目记录。\r\n\r\n是否继续后面的操作? \r\n\r\n(Yes 继续；No 终止未完成的全部删除操作)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                            });
                            if (result == System.Windows.Forms.DialogResult.No)
                            {
                                strError = "中断操作";
                                goto ERROR1;
                            }
                            continue;
                        }
                    }

                REDO_DELETE:
                    channel.Timeout = new TimeSpan(0, 5, 0);
                    lRet = channel.SetBiblioInfo(
                        looping.Progress,
                        "delete",
                        strRecPath,
                        "xml",
                        "", // strXml,
                        baTimestamp,
                        "",
                        delete_style,
                        out strOutputPath,
                        out byte[] baNewTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ErrorCode.NotFound)
                        {
                            skipped_recpath.Add(strRecPath);
                            continue;
                        }

                        string message = $"删除书目记录 {strRecPath} 时出错:\r\n{strError}\r\n---\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)";
                        dialog_result = this.TryGet(() =>
                        {
                            // return:
                            //      DialogResult.Retry 表示超时了
                            //      DialogResult.OK 表示点了 OK 按钮
                            //      DialogResult.Cancel 表示点了右上角的 Close 按钮
                            return AutoCloseMessageBox.Show(this,
message,
20 * 1000,
"BiblioSearchForm");
                        });
                        if (dialog_result != DialogResult.Cancel)
                        {
                            // 重试次数太多了还不行，就跳过
                            if (nRedoCount >= 10)
                            {
                                skipped_recpath.Add(strRecPath);
                                continue;
                            }

                            nRedoCount++;
                            if (channel.ErrorCode == ErrorCode.TimestampMismatch)
                                goto REDO_LOAD;

                            goto REDO_DELETE;
                        }

#if REMOVED
                        // 2020/12/22
                        if (bDontPrompt == false || nRedoCount >= 10)
                        {
                            bool temp = bDontPrompt;
                            string message = $"删除书目记录 {strRecPath} 时出错:\r\n{strError}\r\n---\r\n\r\n是否重试?\r\n\r\n注：\r\n[重试] 重试删除\r\n[跳过] 放弃删除当前记录，但继续后面的处理\r\n[中断] 中断整批操作";
                            dialog_result = (DialogResult)Application.OpenForms[0].Invoke(new Func<DialogResult>(() =>
                            {
                                return MessageDlg.Show(this,
                                message,
                                "BiblioSearchForm",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxDefaultButton.Button1,
                                ref temp,
                                new string[] { "重试", "跳过", "中断" },
                                "下次不再询问");
                            }));
                            bDontPrompt = temp;
                        }
                        if (dialog_result == DialogResult.Yes)
                        {
                            nRedoCount++;
                            if (channel.ErrorCode == ErrorCode.TimestampMismatch)
                                goto REDO_LOAD;

                            goto REDO_DELETE;
                        }
                        if (dialog_result == DialogResult.No)
                            continue;
#endif
                        goto ERROR1;
                    }

                    nDeleteCount++;

                    looping.Progress.SetProgressValue(i);

                    this.TryInvoke(() =>
                    {
                        this.listView_records.Items.Remove(item);
                    });
                }
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                this.ReturnChannel(channel);

                this.EnableControlsInSearching(true);
                EnableRecordList(true);
            }

            {
                string message = "成功删除书目记录 " + nDeleteCount + " 条";
                if (skipped_recpath.Count > 0)
                    message += $"。但有 {skipped_recpath} 条书目记录跳过了删除";
                this.MessageBoxShow(message);
            }
            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

        void EnableRecordList(bool enable)
        {
            this.TryInvoke(() =>
            {
                this.listView_records.Enabled = enable;
            });
        }

        // 从窗口中移走所选择的事项
        void menu_removeSelectedItems_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.listView_records.SelectedIndexChanged -= new System.EventHandler(this.listView_records_SelectedIndexChanged);
            // this.listView_records.BeginUpdate();
            try
            {
                ListViewUtil.DeleteSelectedItems(this.listView_records);
                /*
                var indices = this.listView_records.SelectedIndices.Cast<int>().Reverse();
                foreach(var index in indices)
                {
                    this.listView_records.Items.RemoveAt(index);
                }
                */

                /*
                for (int i = this.listView_records.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    this.listView_records.Items.RemoveAt(this.listView_records.SelectedIndices[i]);
                }
                */
            }
            finally
            {
                // this.listView_records.EndUpdate();
                this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
                this.Cursor = oldCursor;
            }
        }

        /*
        void menu_selectAll_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < this.listView_records.Items.Count; i++)
            {
                this.listView_records.Items[i].Selected = true;
            }

            this.Cursor = oldCursor;
        }*/

        // 当前缺省的编码方式
        Encoding CurrentEncoding = Encoding.UTF8;

        // 为了保存ISO2709文件服务的几个变量

        /// <summary>
        /// 最近保存过的 ISO2709 文件全路径
        /// </summary>
        public string LastIso2709FileName
        {
            get
            {
                return Program.MainForm.AppInfo.GetString(
                    this._dbType + "searchform",
                    "last_iso2709_filename",
                    "");
            }
            set
            {
                Program.MainForm.AppInfo.SetString(
                    this._dbType + "searchform",
                    "last_iso2709_filename",
                    value);
            }
        }

        /// <summary>
        /// 最近保存 ISO2709 文件时是否具有 CRLF
        /// </summary>
        public bool LastCrLfIso2709
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    this._dbType + "searchform",
                    "last_iso2709_crlf",
                    false);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
                    this._dbType + "searchform",
                    "last_iso2709_crlf",
                    value);
            }
        }

        /// <summary>
        /// 最近保存 ISO2709 文件时是否删除 998 字段
        /// </summary>
        public bool LastRemoveField998
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    this._dbType + "searchform",
                    "last_iso2709_removefield998",
                    false);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
                    this._dbType + "searchform",
                    "last_iso2709_removefield998",
                    value);
            }
        }

        /// <summary>
        /// 最近保存 ISO2709 文件时用过的编码方式名字
        /// </summary>
        public string LastEncodingName
        {
            get
            {
                return Program.MainForm.AppInfo.GetString(
                    this._dbType + "searchform",
                    "last_encoding_name",
                    "");
            }
            set
            {
                Program.MainForm.AppInfo.SetString(
                    this._dbType + "searchform",
                    "last_encoding_name",
                    value);
            }
        }

        /// <summary>
        /// 最近保存 ISO2709 文件时用过的编目规则
        /// </summary>
        public string LastCatalogingRule
        {
            get
            {
                return Program.MainForm.AppInfo.GetString(
                    this._dbType + "searchform",
                    "last_cataloging_rule",
                    "<无限制>");
            }
            set
            {
                Program.MainForm.AppInfo.SetString(
                    this._dbType + "searchform",
                    "last_cataloging_rule",
                    value);
            }
        }

        public static bool ExistFiles(string strDirectoryName)
        {
            if (Directory.Exists(strDirectoryName) == false)
                return false;
            DirectoryInfo di = new DirectoryInfo(strDirectoryName);
            FileInfo[] fis = di.GetFiles("*.*");
            if (fis != null & fis.Length > 0)
                return true;
            return false;
        }

        static int MAX_CACHE_ITEMS = 10000; // 批处理中，缓存最多的事项数。多于这个数，就不再使用缓存

        // 导出到书目转储文件
        async void menu_saveToBiblioDumpFile_Click(object sender, EventArgs e)
        {
            await ExportSelectedToBiblioDumpFileAsync();
        }

        public Task ExportSelectedToBiblioDumpFileAsync()
        {
            return Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        _exportSelectedToBiblioDumpFile();
                    }
                    catch (Exception ex)
                    {
                        this.MessageBoxShow($"_exportSelectedToBiblioDumpFile() 异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                },
    this.CancelToken,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);
        }

        void _exportSelectedToBiblioDumpFile()
        {
            string strError = "";
            int nRet = 0;

            var selected_items = ListViewUtil.GetSelectedItems(this.listView_records);

            if (selected_items.Count == 0)
            {
                strError = "尚未选定要导出的事项";
                goto ERROR1;
            }

            OpenBiblioDumpFileDialog dlg = null;
            var dialog_result = this.TryGet(() =>
            {
                dlg = new OpenBiblioDumpFileDialog();
                MainForm.SetControlFont(dlg, this.Font);
                dlg.CreateMode = true;

                dlg.UiState = Program.MainForm.AppInfo.GetString(
            "BiblioSearchForm",
            "OpenBiblioDumpFileDialog_uiState",
            "");
                Program.MainForm.AppInfo.LinkFormState(dlg, "bibliosearchform_OpenBiblioDumpFileDialog");
                dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(dlg);

                Program.MainForm.AppInfo.SetString(
            "BiblioSearchForm",
            "OpenBiblioDumpFileDialog_uiState",
            dlg.UiState);

                return dlg.DialogResult;
            });
            if (dialog_result != System.Windows.Forms.DialogResult.OK)
                return;

            bool bControl = (Control.ModifierKeys & Keys.Control) != 0;

            // 观察对象目录中是否已经存在文件
            if (dlg.IncludeObjectFile)
            {
                if (ExistFiles(dlg.ObjectDirectoryName) == true)
                {
                    DialogResult result = this.TryGet(() =>
                    {
                        return MessageBox.Show(this,
        "您选定的对象文件夹 " + dlg.ObjectDirectoryName + " 内已经存在一些文件。若用它来保存对象文件，则新旧文件会混杂在一起。\r\n\r\n要继续处理么? (是：继续; 否: 放弃处理)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                    });
                    if (result == System.Windows.Forms.DialogResult.No)
                        return;
                }
            }

            if (File.Exists(dlg.FileName))
            {
                DialogResult result = this.TryGet(() =>
                {
                    return MessageBox.Show(this,
    "文件 " + dlg.FileName + " 已经存在，本次导出会覆盖它原有的内容。\r\n\r\n要继续处理么? (是：继续，会覆盖它原有内容; 否: 放弃处理)",
    "BiblioSearchForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                });
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
            }

            int nProcessRemoteRecord = 0;   // 0: 本变量尚未使用; 1: 处理外部记录; -1: 跳过外部记录

            this.EnableControls(false);

            LibraryChannel channel = this.GetChannel();

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在导出到 .bdf 文件 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在导出到 .bdf 文件 ...");

            XmlTextWriter writer = null;

            try
            {
                writer = new XmlTextWriter(dlg.FileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "创建文件 " + dlg.FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                looping.Progress.SetProgressRange(0, selected_items.Count);

                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument();
                writer.WriteStartElement("dprms", "collection", DpNs.dprms);

                writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);
#if NO
                writer.WriteAttributeString("xmlns", "unimarc", null, DigitalPlatform.Xml.Ns.unimarcxml);
                writer.WriteAttributeString("xmlns", "marc21", null, DigitalPlatform.Xml.Ns.usmarcxml);
#endif

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in selected_items)
                {
                    string recpath = ListViewUtil.GetItemText(item, 0);
                    if (string.IsNullOrEmpty(recpath) == true)
                        continue;

                    items.Add(item);
                }

                ListViewBiblioLoader loader = new ListViewBiblioLoader(channel, // this.Channel,
                    looping.Progress,
                    items,
                    items.Count > MAX_CACHE_ITEMS ? null : this.m_biblioTable);
                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    // Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    BiblioInfo info = item.BiblioInfo;

                    string strXml = "";
                    {
                        if (string.IsNullOrEmpty(info.NewXml) == false)
                            strXml = info.NewXml;
                        else
                            strXml = info.OldXml;
                    }

                    if (string.IsNullOrEmpty(strXml) == true)
                        goto CONTINUE;

                    XmlDocument biblio_dom = new XmlDocument();
                    try
                    {
                        biblio_dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "书目记录 '" + info.RecPath + "' 的 XML 装入 DOM 时出错: " + ex.Message;
                        goto ERROR1;
                    }

                    if (biblio_dom.DocumentElement == null)
                        goto CONTINUE;

                    // 是否为外部记录
                    bool bRemote = info.RecPath.IndexOf("@") != -1;
                    if (bRemote == true && nProcessRemoteRecord == 0)
                    {
                        DialogResult temp_result = this.TryGet(() =>
                        {
                            return MessageBox.Show(this,
        "发现一些书目记录是来自共享检索的外部书目记录，是否要处理它们? (是: 处理它们; 否: 跳过它们)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        });
                        if (temp_result == System.Windows.Forms.DialogResult.Yes)
                            nProcessRemoteRecord = 1;
                        else
                            nProcessRemoteRecord = -1;
                    }

                    if (bRemote == true && nProcessRemoteRecord == -1)
                        goto CONTINUE;

                    if (dlg.IncludeObjectFile && bRemote == false)
                    {
                    REDO_WRITEOBJECTS:
                        // 将书目记录中的对象资源写入外部文件
                        nRet = WriteObjectFiles(looping.Progress,
                channel,
                info.RecPath,
                ref biblio_dom,
                dlg.ObjectDirectoryName,
                "", // dlg.MimeFileExtension ? "mimeFileExtension" : "",
                out strError);
                        if (nRet == -1)
                        {
                            if (looping.Stopped)
                                goto ERROR1;

                            DialogResult temp_result = this.TryGet(() =>
                            {
                                return MessageBox.Show(this,
        strError + "\r\n\r\n是否重试?\r\n\r\n(Yes: 重试; No: 放弃导出本记录的对象，但继续后面的处理; Cancel: 放弃全部处理)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                            });
                            if (temp_result == System.Windows.Forms.DialogResult.Yes)
                                goto REDO_WRITEOBJECTS;
                            if (temp_result == DialogResult.Cancel)
                                goto ERROR1;
                        }
                    }

                    // 写入 dprms:record 元素
                    writer.WriteStartElement("dprms", "record", DpNs.dprms);

                    {
                        // 写入 dprms:biblio 元素
                        writer.WriteStartElement("dprms", "biblio", DpNs.dprms);

                        if (bRemote == true)
                            writer.WriteAttributeString("path", item.BiblioInfo.RecPath);
                        else
                            writer.WriteAttributeString("path", Program.MainForm.LibraryServerUrl + "?" + item.BiblioInfo.RecPath);
                        writer.WriteAttributeString("timestamp", ByteArray.GetHexTimeStampString(item.BiblioInfo.Timestamp));

                        biblio_dom.DocumentElement.WriteTo(writer);
                        writer.WriteEndElement();
                    }

                    if (bRemote == false)
                    {
                        string strBiblioDbName = StringUtil.GetDbName(item.BiblioInfo.RecPath);
                        BiblioDbProperty prop = Program.MainForm.GetBiblioDbProperty(strBiblioDbName);
                        if (prop == null)
                        {
                            strError = "数据库名 '" + strBiblioDbName + "' 没有找到属性定义";
                            goto ERROR1;
                        }

                    REDO_OUTPUTENTITIES:
                        nRet = 0;
                        if (string.IsNullOrEmpty(prop.OrderDbName) == false
                            && dlg.IncludeOrders)
                        {
                            // dprms:orderCollection
                            nRet = OutputEntities(
                                looping.Progress,
                                channel,
                                item.BiblioInfo.RecPath,
                                "order",
                                writer,
                                dlg,
                                bControl ? "opac" : "",
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                        if (string.IsNullOrEmpty(prop.IssueDbName) == false
                            && dlg.IncludeIssues)
                        {
                            // dprms:issueCollection
                            nRet = OutputEntities(
                                looping.Progress,
                                channel,
                                item.BiblioInfo.RecPath,
                                "issue",
                                writer,
                                dlg,
                                bControl ? "opac" : "",
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                        if (string.IsNullOrEmpty(prop.ItemDbName) == false
                            && dlg.IncludeEntities)
                        {
                            // dprms:itemCollection
                            nRet = OutputEntities(
                                looping.Progress,
                                channel,
                                item.BiblioInfo.RecPath,
                                "item",
                                writer,
                                dlg,
                                bControl ? "opac" : "",
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                        if (string.IsNullOrEmpty(prop.CommentDbName) == false
                            && dlg.IncludeComments)
                        {
                            // dprms:commentCollection
                            nRet = OutputEntities(
                                looping.Progress,
                                channel,
                                item.BiblioInfo.RecPath,
                                "comment",
                                writer,
                                dlg,
                                bControl ? "opac" : "",
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }

#if REMOVED
                        if (nRet == -1)
                        {
                            if (looping.Stopped)
                                goto ERROR1;

                            // 注: OutputEntities() 函数内部已经做了重试处理
                            /*
                            DialogResult temp_result = MessageBox.Show(this,
        strError + "\r\n\r\n是否重试?\r\n\r\n(Yes: 重试; No: 放弃导出本记录的下级对象，但继续后面的处理; Cancel: 放弃全部处理)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                            if (temp_result == System.Windows.Forms.DialogResult.Yes)
                                goto REDO_OUTPUTENTITIES;
                            if (temp_result == DialogResult.Cancel)
                                goto ERROR1;
                            */
                            goto ERROR1;
                        }
#endif
                    }

                    // 收尾 dprms:record 元素
                    writer.WriteEndElement();
                CONTINUE:
                    looping.Progress.SetProgressValue(++i);
                }

                writer.WriteEndElement();   // </collection>
                writer.WriteEndDocument();
            }
            catch (Exception ex)
            {
                strError = "写入文件 " + dlg.FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                writer.Close();

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                */
                EndLoop(looping);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControls(true);
            }

            MainForm.StatusBarMessage = selected_items.Count.ToString()
                + "条记录成功保存到文件 " + dlg.FileName;
            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

        // 将 XML 记录中的对象资源写入外部文件
        // parameters:
        //      style   风格。
        //              mimeFileExtension   对象文件扩展名根据 MIME 类型决定
        //              usageFileExtension  对象文件扩展名根据 usage 决定
        public static int WriteObjectFiles(Stop stop,
            LibraryChannel channel,
            string strRecPath,
            ref XmlDocument dom,
            string strOutputDir,
            string style,
            out string strError)
        {
            strError = "";

            bool mimeFileExtension = StringUtil.IsInList("mimeFileExtension", style);
            bool usageFileExtension = StringUtil.IsInList("usageFileExtension", style);

            List<string> recpaths = new List<string>();
            List<string> errors = new List<string>();
            Hashtable usage_table = new Hashtable();    // recpath --> usage

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);

            foreach (XmlElement node in nodes)
            {
                string strID = DomUtil.GetAttr(node, "id");
                string strUsage = DomUtil.GetAttr(node, "usage");
                string strRights = DomUtil.GetAttr(node, "rights");

                string strResPath = strRecPath + "/object/" + strID;
                strResPath = strResPath.Replace(":", "/");
                recpaths.Add(strResPath);

                usage_table[strResPath] = strUsage;
            }

            if (recpaths.Count == 0)
                return 0;

            try
            {
                BrowseLoader loader = new BrowseLoader();
                loader.Channel = channel;
                loader.Stop = stop;
                loader.RecPaths = recpaths;
                loader.Format = "id,metadata,timestamp";

                int i = 0;
                foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                {
                    Application.DoEvents();

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    Debug.Assert(record.Path == recpaths[i], "");
                    XmlElement file = nodes[i] as XmlElement;

                    if (record.RecordBody.Result != null
                        && record.RecordBody.Result.ErrorCode != ErrorCodeValue.NoError)
                    {
                        if (record.RecordBody.Result.ErrorCode == ErrorCodeValue.NotFound)
                            goto CONTINUE;
                        strError = record.RecordBody.Result.ErrorString;
                        file.SetAttribute("_error", strError);
                        errors.Add(strError);
                        goto CONTINUE;
                    }

                    string strMetadataXml = record.RecordBody.Metadata;
                    byte[] baMetadataTimestamp = record.RecordBody.Timestamp;

                    file.SetAttribute("_timestamp", ByteArray.GetHexTimeStampString(baMetadataTimestamp));

                    // TODO: 另一种方法是用 URL 数据库名 ObjectID 等共同构造一个文件名
                    string strGUID = Guid.NewGuid().ToString();
                    string strName = record.Path.Replace("/", "_");
                    string strMetadataFileName = Path.Combine(strOutputDir, strName + ".met");
                    if (File.Exists(strMetadataFileName))
                        strMetadataFileName = Path.Combine(strOutputDir, strName + "_" + strGUID + ".met");

                    string ext = ".bin";
                    // 2021/6/4
                    // 根据 MIME 得到文件扩展名
                    if (mimeFileExtension || usageFileExtension)
                    {
                        var values = StringUtil.ParseMetaDataXml(strMetadataXml,
    out strError);
                        if (values != null)
                        {
                            if (mimeFileExtension)
                            {
                                string mime = (string)values["mimetype"];
                                ext = PathUtil.GetExtensionByMime(mime);
                                if (string.IsNullOrEmpty(ext))
                                    ext = ".bin";
                            }

                            if (usageFileExtension)
                            {
                                string usage = (string)usage_table[record.Path];
                                if (string.IsNullOrEmpty(usage) == false)
                                    ext = "." + usage + ext;
                            }
                        }
                    }

                    string strObjectFileName = Path.Combine(strOutputDir, strName + ext);
                    if (File.Exists(strObjectFileName))
                        strObjectFileName = Path.Combine(strOutputDir, strName + "_" + strGUID + ".bin");

                    //string strMetadataFileName = Path.Combine(strOutputDir, strGUID + ".met");
                    //string strObjectFileName = Path.Combine(strOutputDir, strGUID + ".bin");

                    // metadata 写入外部文件
                    if (string.IsNullOrEmpty(strMetadataXml) == false)
                    {
                        PathUtil.TryCreateDir(Path.GetDirectoryName(strMetadataFileName));
                        using (StreamWriter sw = new StreamWriter(strMetadataFileName))
                        {
                            sw.Write(strMetadataXml);
                        }
                    }

                    file.SetAttribute("_metadataFile", Path.GetFileName(strMetadataFileName));

                    // 对象内容写入外部文件
                    int nRet = DownloadObject(stop,
            channel,
            record.Path,
            strObjectFileName,
            out strError);
                    if (nRet == -1)
                    {
                        // TODO: 是否要重试几次?
                        file.SetAttribute("error", strError);
                        errors.Add(strError);
                        goto CONTINUE;
                    }

                    file.SetAttribute("_objectFile", Path.GetFileName(strObjectFileName));

#if NO
                    // 取metadata值
                    Hashtable values = StringUtil.ParseMedaDataXml(strMetadataXml,
                        out strError);
                    if (values == null)
                    {
                        file.SetAttribute("error", strError);
                        errors.Add(strError);
                        continue;
                    }

                    // metadata 中的一些属性，写入 file 元素属性？ _ 打头
                    // localpath
                    ListViewUtil.ChangeItemText(item, COLUMN_LOCALPATH, (string)values["localpath"]);

                    // size
                    ListViewUtil.ChangeItemText(item, COLUMN_SIZE, (string)values["size"]);

                    // mime
                    ListViewUtil.ChangeItemText(item, COLUMN_MIME, (string)values["mimetype"]);

                    // tiemstamp
                    string strTimestamp = ByteArray.GetHexTimeStampString(baMetadataTimestamp);
                    ListViewUtil.ChangeItemText(item, COLUMN_TIMESTAMP, strTimestamp);
#endif

                CONTINUE:
                    i++;
                }

                if (errors.Count > 0)
                {
                    strError = StringUtil.MakePathList(errors, "; ");
                    return -1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                // TODO: 出现异常后，是否改为用原来的方法一个一个对象地获取 metadata?
                strError = ex.Message;
                return -1;
            }
        }

        static int DownloadObject(Stop stop,
            LibraryChannel channel,
            string strResPath,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            PathUtil.TryCreateDir(Path.GetDirectoryName(strOutputFileName));

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);

            if (stop != null)
                stop.Initial("正在下载对象 " + strResPath);
            try
            {
                byte[] baOutputTimeStamp = null;
                string strMetaData = "";
                string strOutputPath = "";

                long lRet = channel.GetRes(
                    stop,
                    strResPath,
                    strOutputFileName,
                    "content,data,metadata,timestamp,outputpath,gzip",  // 2017/10/7 增加 gzip
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "下载对象 '" + strResPath + "' 到文件失败，原因: " + strError;
                    return -1;
                }
                return 0;
            }
            finally
            {
                channel.Timeout = old_timeout;
                if (stop != null)
                    stop.Initial("");
            }
        }

        public int OutputEntities(
            Stop stop,
            LibraryChannel channel,
            string strBiblioRecPath,
            string strDbType,
            XmlTextWriter writer,
            OpenBiblioDumpFileDialog dlg,
            string strStyle,
            out string strError)
        {
            strError = "";

            bool bBegin = false;

            SubItemLoader loader = new SubItemLoader();
            loader.BiblioRecPath = strBiblioRecPath;
            loader.Channel = channel;
            loader.Stop = stop;
            loader.DbType = strDbType;
            loader.Format = strStyle;

            loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
            loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

            foreach (EntityInfo info in loader)
            {
                if (info.ErrorCode != ErrorCodeValue.NoError)
                {
                    strError = "路径为 '" + info.OldRecPath + "' 的册记录装载中发生错误: " + info.ErrorInfo; // NewRecPath
                    return -1;
                }

                if (bBegin == false)
                {
                    writer.WriteStartElement("dprms", strDbType + "Collection", DpNs.dprms);
                    bBegin = true;
                }

                XmlDocument item_dom = new XmlDocument();
                try
                {
                    item_dom.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "路径为 '" + info.OldRecPath + "' 的册记录 XML 装载到 XMLDOM 中时发生错误: " + ex.Message;

                    if (stop != null && stop.State != 0)
                        return -1;

                    // 2022/11/30
                    var temp = strError;
                    DialogResult temp_result = this.TryGet(() =>
                    {
                        return MessageBox.Show(this,
    temp + "\r\n\r\n是否跳过这条册记录继续后面处理?\r\n\r\n(Yes: 放弃导出本条册记录，但继续后面的处理; No: 放弃全部处理)",
    "BiblioSearchForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    });
                    if (temp_result == System.Windows.Forms.DialogResult.Yes)
                        continue;
                    if (temp_result == DialogResult.No)
                        return -1;

                    return -1;
                }

                if (dlg.IncludeObjectFile)
                {
                REDO_WRITEOBJECTS:
                    // 将记录中的对象资源写入外部文件
                    int nRet = WriteObjectFiles(stop,
            channel,
            info.OldRecPath,
            ref item_dom,
            dlg.ObjectDirectoryName,
            "", // dlg.MimeFileExtension ? "mimeFileExtension" : "",
            out strError);
                    if (nRet == -1)
                    {
                        if (stop != null && stop.State != 0)
                            return -1;

                        var temp = strError;
                        DialogResult temp_result = this.TryGet(() =>
                        {
                            return MessageBox.Show(this,
        temp + "\r\n\r\n是否重试?\r\n\r\n(Yes: 重试; No: 放弃导出本记录的对象，但继续后面的处理; Cancel: 放弃全部处理)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        });
                        if (temp_result == System.Windows.Forms.DialogResult.Yes)
                            goto REDO_WRITEOBJECTS;
                        if (temp_result == DialogResult.Cancel)
                            return -1;
                    }
                }

                if (item_dom.DocumentElement != null)
                {
                    writer.WriteStartElement("dprms", strDbType, DpNs.dprms);
                    writer.WriteAttributeString("path", info.OldRecPath);
                    writer.WriteAttributeString("timestamp", ByteArray.GetHexTimeStampString(info.OldTimestamp));
                    DomUtil.RemoveEmptyElements(item_dom.DocumentElement);
                    item_dom.DocumentElement.WriteContentTo(writer);
                    writer.WriteEndElement();
                }
                else
                {
                    // TODO: 是否警告 DocumentElement 为空
                    Debug.Assert(false, "");
                }
            }

            if (bBegin == true)
                writer.WriteEndElement();

            return 1;
        }

#if NO
        public static int OutputEntities(
            Stop stop,
            LibraryChannel channel,
            string strBiblioRecPath,
            string strDbType,
            XmlTextWriter writer,
            out string strError)
        {
            strError = "";

            bool bBegin = false;

            long lPerCount = 100; // 每批获得多少个
            long lStart = 0;
            long lResultCount = 0;
            long lCount = -1;
            for (; ; )
            {
                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                EntityInfo[] entities = null;

                long lRet = 0;

                channel.Timeout = new TimeSpan(0, 5, 0);
                if (strDbType == "item")
                {
                    lRet = channel.GetEntities(
                         stop,
                         strBiblioRecPath,
                         lStart,
                         lCount,
                         "", // "onlygetpath",
                         "zh",
                         out entities,
                         out strError);
                }
                if (strDbType == "order")
                {
                    lRet = channel.GetOrders(
                         stop,
                         strBiblioRecPath,
                         lStart,
                         lCount,
                         "", // "onlygetpath",
                         "zh",
                         out entities,
                         out strError);
                }
                if (strDbType == "issue")
                {
                    lRet = channel.GetIssues(
                         stop,
                         strBiblioRecPath,
                         lStart,
                         lCount,
                         "", // "onlygetpath",
                         "zh",
                         out entities,
                         out strError);
                }
                if (strDbType == "comment")
                {
                    lRet = channel.GetComments(
                         stop,
                         strBiblioRecPath,
                         lStart,
                         lCount,
                         "", // "onlygetpath",
                         "zh",
                         out entities,
                         out strError);
                }
                if (lRet == -1)
                    return -1;

                lResultCount = lRet;

                if (lRet == 0)
                    return 0;

                Debug.Assert(entities != null, "");

                foreach (EntityInfo info in entities)
                {
                    if (info.ErrorCode != ErrorCodeValue.NoError)
                    {
                        strError = "路径为 '" + info.OldRecPath + "' 的册记录装载中发生错误: " + info.ErrorInfo;  // NewRecPath
                        return -1;
                    }

                    if (bBegin == false)
                    {
                        writer.WriteStartElement("dprms", strDbType + "Collection", DpNs.dprms);
                        bBegin = true;
                    }

                    XmlDocument item_dom = new XmlDocument();
                    item_dom.LoadXml(info.OldRecord);

                    writer.WriteStartElement("dprms", strDbType, DpNs.dprms);
                    writer.WriteAttributeString("path", info.OldRecPath);
                    writer.WriteAttributeString("timestamp", ByteArray.GetHexTimeStampString(info.OldTimestamp));
                    DomUtil.RemoveEmptyElements(item_dom.DocumentElement);
                    item_dom.DocumentElement.WriteContentTo(writer);
                    writer.WriteEndElement();
                }

                lStart += entities.Length;
                if (lStart >= lResultCount)
                    break;

                if (lCount == -1)
                    lCount = lPerCount;

                if (lStart + lCount > lResultCount)
                    lCount = lResultCount - lStart;
            }

            if (bBegin == true)
                writer.WriteEndElement();

            return 1;
        }

#endif

        // 输出 HTML/docx 新书通报
        void menu_saveToNewBookFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要导出的事项";
                goto ERROR1;
            }

            List<string> biblioRecPathList = new List<string>();

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;

                string strRecPath = item.Text;

                if (string.IsNullOrEmpty(strRecPath) == true)
                    continue;

                biblioRecPathList.Add(strRecPath);
            }

            var result = _saveToNewBookFile(biblioRecPathList,
                null);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                goto ERROR1;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        void menu_saveToXmlFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            //int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要导出的事项";
                goto ERROR1;
            }

#if NO
            Encoding preferredEncoding = this.CurrentEncoding;

            {
                // 观察要保存的第一条记录的marc syntax
            }
#endif

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的 XML 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = "";
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "XML 文件 (*.xml)|*.xml|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

#if NO
            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "BiblioSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }

#endif
            this.EnableControls(false);

            LibraryChannel channel = this.GetChannel();

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在导出到 XML 文件 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在导出到 XML 文件 ...");

            XmlTextWriter writer = null;

            try
            {
                writer = new XmlTextWriter(dlg.FileName, Encoding.UTF8);

            }
            catch (Exception ex)
            {
                strError = "创建文件 " + dlg.FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                looping.Progress.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument();
                writer.WriteStartElement("dprms", "collection", DpNs.dprms);

                writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);
#if NO
                writer.WriteAttributeString("xmlns", "unimarc", null, DigitalPlatform.Xml.Ns.unimarcxml);
                writer.WriteAttributeString("xmlns", "marc21", null, DigitalPlatform.Xml.Ns.usmarcxml);
#endif

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }

                ListViewBiblioLoader loader = new ListViewBiblioLoader(channel, // this.Channel,
                    looping.Progress,
                    items,
                    items.Count > MAX_CACHE_ITEMS ? null : this.m_biblioTable);
                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

#if NO
                    string strRecPath = item.Text;

                    if (string.IsNullOrEmpty(strRecPath) == true)
                        continue;

                    string[] results = null;
                    byte[] baTimestamp = null;

                    stop.SetMessage("正在获取书目记录 " + strRecPath);

                    long lRet = Channel.GetBiblioInfos(
                        stop,
                        strRecPath,
                        "",
                        new string[] { "xml" },   // formats
                        out results,
                        out baTimestamp,
                        out strError);
                    if (lRet == 0)
                        goto ERROR1;
                    if (lRet == -1)
                        goto ERROR1;

                    if (results == null || results.Length == 0)
                    {
                        strError = "results error";
                        goto ERROR1;
                    }

                    string strXml = results[0];
#endif
                    BiblioInfo info = item.BiblioInfo;

                    string strXml = "";
                    {
                        if (string.IsNullOrEmpty(info.NewXml) == false)
                            strXml = info.NewXml;
                        else
                            strXml = info.OldXml;
                    }

                    if (string.IsNullOrEmpty(strXml) == false)
                    {
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "XML 装入 DOM 时出错: " + ex.Message;
                            goto ERROR1;
                        }

                        if (dom.DocumentElement != null)
                        {
                            // 给根元素设置几个参数
                            DomUtil.SetAttr(dom.DocumentElement, "path", DpNs.dprms, Program.MainForm.LibraryServerUrl + "?" + item.BiblioInfo.RecPath);  // strRecPath
                            DomUtil.SetAttr(dom.DocumentElement, "timestamp", DpNs.dprms, ByteArray.GetHexTimeStampString(item.BiblioInfo.Timestamp));   // baTimestamp

                            dom.DocumentElement.WriteTo(writer);
                        }
                    }

                    looping.Progress.SetProgressValue(++i);
                }

                writer.WriteEndElement();   // </collection>
                writer.WriteEndDocument();
            }
            catch (Exception ex)
            {
                strError = "写入文件 " + dlg.FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                writer.Close();

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                */
                EndLoop(looping);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControls(true);
            }

            MainForm.StatusBarMessage = this.listView_records.SelectedItems.Count.ToString()
                + "条记录成功保存到文件 " + dlg.FileName;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 2015/10/10
        // 保存到 MARC 文件
        void menu_saveToMarcFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要导出的事项";
                goto ERROR1;
            }

            Encoding preferredEncoding = this.CurrentEncoding;

            {
                // 观察要保存的第一条记录的marc syntax
            }

            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            MainForm.SetControlFont(dlg, this.Font);
            dlg.IsOutput = true;
            dlg.AddG01Visible = true;
            dlg.RuleVisible = true;
            dlg.Rule = this.LastCatalogingRule;
            dlg.FileName = this.LastIso2709FileName;
            // dlg.CrLf = this.LastCrLfIso2709;
            dlg.CrLfVisible = false;   // 2020/3/9
            dlg.RemoveField998 = this.LastRemoveField998;
            dlg.EncodingListItems = Global.GetEncodingList(false);
            dlg.EncodingName =
                (String.IsNullOrEmpty(this.LastEncodingName) == true ? Global.GetEncodingName(preferredEncoding) : this.LastEncodingName);
            dlg.EncodingComment = "注: 原始编码方式为 " + Global.GetEncodingName(preferredEncoding);
            dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            dlg.EnableMarcSyntax = false;
            dlg.ShowDialog(this);

            // 放在这里的意图是即便 Cancel 了，对话框里面的内容也能被记住
            this.LastIso2709FileName = dlg.FileName;
            this.LastCrLfIso2709 = dlg.CrLf;
            this.LastEncodingName = dlg.EncodingName;
            this.LastCatalogingRule = dlg.Rule;
            this.LastRemoveField998 = dlg.RemoveField998;

            if (dlg.DialogResult != DialogResult.OK)
                return;

            bool unimarc_modify_100 = dlg.UnimarcModify100;

            string strCatalogingRule = dlg.Rule;
            if (strCatalogingRule == "<无限制>")
                strCatalogingRule = null;

            Encoding targetEncoding = null;

            nRet = Global.GetEncoding(dlg.EncodingName,
                out targetEncoding,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            string strLastFileName = this.LastIso2709FileName;
            string strLastEncodingName = this.LastEncodingName;

            ExportMarcHoldingDialog dlg_905 = new ExportMarcHoldingDialog();
            {
                MainForm.SetControlFont(dlg_905, this.Font);

                dlg_905.UiState = Program.MainForm.AppInfo.GetString(
                    "BiblioSearchForm",
                    "ExportMarcHoldingDialog_uiState",
                    "");

                Program.MainForm.AppInfo.LinkFormState(dlg_905, "BiblioSearchForm_ExportMarcHoldingDialog_state");
                dlg_905.ShowDialog(this);

                Program.MainForm.AppInfo.SetString(
                    "BiblioSearchForm",
                    "ExportMarcHoldingDialog_uiState",
                    dlg_905.UiState);

                if (dlg_905.DialogResult != DialogResult.OK)
                    return;
            }

            bool bExist = File.Exists(dlg.FileName);
            bool bAppend = false;

            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "BiblioSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }

            // 检查同一个文件连续存时候的编码方式一致性
            if (strLastFileName == dlg.FileName
                && bAppend == true)
            {
                if (strLastEncodingName != ""
                    && strLastEncodingName != dlg.EncodingName)
                {
                    DialogResult result = MessageBox.Show(this,
                        "文件 '" + dlg.FileName + "' 已在先前已经用 " + strLastEncodingName + " 编码方式存储了记录，现在又以不同的编码方式 " + dlg.EncodingName + " 追加记录，这样会造成同一文件中存在不同编码方式的记录，可能会令它无法被正确读取。\r\n\r\n是否继续? (是)追加  (否)放弃操作",
                        "BiblioSearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        strError = "放弃处理...";
                        goto ERROR1;
                    }

                }
            }

            Stream s = null;

            try
            {
                s = File.Open(this.LastIso2709FileName,
                     FileMode.OpenOrCreate);
                if (bAppend == false)
                    s.SetLength(0);
                else
                    s.Seek(0, SeekOrigin.End);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 " + this.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            int nCount = 0;

            LibraryChannel channel = this.GetChannel();

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            this.EnableControls(false);

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在导出到 MARC 文件 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在导出到 MARC 文件 ...");


            try
            {
                looping.Progress.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    // 2019/5/22
                    if (IsCmdLine(item.Text))
                        continue;

                    items.Add(item);
                }

                ListViewBiblioLoader loader = new ListViewBiblioLoader(channel, // this.Channel,
                    looping.Progress,
                    items,
                    items.Count > MAX_CACHE_ITEMS ? null : this.m_biblioTable);
                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    BiblioInfo info = item.BiblioInfo;

                    string strXml = "";
                    {
                        if (string.IsNullOrEmpty(info.NewXml) == false)
                            strXml = info.NewXml;
                        else
                            strXml = info.OldXml;
                    }

                    // 2017/1/18
                    if (string.IsNullOrEmpty(strXml) == true)
                        continue;   // 并发删除书目记录的时候会碰到

                    // 将XML格式转换为MARC格式
                    // 自动从数据记录中获得MARC语法
                    nRet = MarcUtil.Xml2Marc(strXml,
                        true,
                        null,
                        out string strMarcSyntax,
                        out string strMARC,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "XML转换到MARC记录时出错: " + strError;
                        goto ERROR1;
                    }

                    byte[] baTarget = null;

                    Debug.Assert(strMarcSyntax != "", "");

                    // 按照编目规则过滤
                    // 获得一个特定风格的 MARC 记录
                    // parameters:
                    //      strStyle    要匹配的style值。如果为null，表示任何$*值都匹配，实际上效果是去除$*并返回全部字段内容
                    // return:
                    //      0   没有实质性修改
                    //      1   有实质性修改
                    nRet = MarcUtil.GetMappedRecord(ref strMARC,
                        strCatalogingRule);

                    if (dlg.RemoveField998 == true)
                    {
                        MarcRecord record = new MarcRecord(strMARC);
                        record.select("field[@name='998']").detach();
                        record.select("field[@name='997']").detach();
                        strMARC = record.Text;
                    }
                    if (dlg.Mode880 == true && strMarcSyntax == "usmarc")
                    {
                        MarcRecord record = new MarcRecord(strMARC);
                        MarcQuery.To880(record);
                        strMARC = record.Text;
                    }

                    // 2021/4/8
                    if (dlg.AddG01 == true)
                    {
                        string verify = BuildVerifyString(); // 用于防止小语种字符被修改的验证字符串
                        MarcRecord record = new MarcRecord(strMARC);
                        record.Fields.insertSequence(new MarcField($"-01{item.BiblioInfo.RecPath},verify:{verify}"));
                        strMARC = record.Text;
                    }

                    // 2019/5/22
                    // 是否为本地系统路径
                    bool isLocal = info.RecPath.IndexOf("@") == -1 && IsCmdLine(info.RecPath) == false;

                    if ((dlg_905.Create905 || dlg_905.Create906)
                        && isLocal)
                    {
                        MarcRecord record = new MarcRecord(strMARC);

                        if (dlg_905.Create905 && dlg_905.RemoveOld905)
                            record.select("field[@name='905']").detach();

                        if (dlg_905.Create906)
                            record.select("field[@name='906']").detach();

                        nRet = OutputEntities(
                            looping.Progress,
                            channel,
                            info.RecPath,
                            "item",
                            dlg_905.Create905,
                            dlg_905.Style905,
                            dlg_905.Create906,
                            record,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        strMARC = record.Text;
                    }

                    // 将MARC机内格式转换为ISO2709格式
                    // parameters:
                    //      strSourceMARC   [in]机内格式MARC记录。
                    //      strMarcSyntax   [in]为"unimarc"或"usmarc"
                    //      targetEncoding  [in]输出ISO2709的编码方式。为UTF8、codepage-936等等
                    //      baResult    [out]输出的ISO2709记录。编码方式受targetEncoding参数控制。注意，缓冲区末尾不包含0字符。
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = MarcUtil.CvtJineiToISO2709(
                        strMARC,
                        strMarcSyntax,
                        targetEncoding,
                        unimarc_modify_100 ? "unimarc_100" : "",
                        out baTarget,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    s.Write(baTarget, 0,
                        baTarget.Length);

                    if (dlg.CrLf == true)
                    {
                        byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                        s.Write(baCrLf, 0,
                            baCrLf.Length);
                    }

                    looping.Progress.SetProgressValue(++i);
                    nCount++;
                }
            }
            catch (Exception ex)
            {
                strError = "写入文件 " + this.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                s.Close();

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                */
                EndLoop(looping);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControls(true);
            }

            // 
            if (bAppend == true)
                MainForm.StatusBarMessage = nCount.ToString()
                    + "条记录成功追加到文件 " + this.LastIso2709FileName + " 尾部";
            else
                MainForm.StatusBarMessage = nCount.ToString()
                    + "条记录成功保存到新文件 " + this.LastIso2709FileName;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 构造校验字符串
        public static string BuildVerifyString()
        {
            string source = "väter";
            var bytes = Encoding.UTF8.GetBytes(source);
            return source + "=" + ByteArray.GetHexTimeStampString(bytes);
        }

        // 验证校验字符串是否完好
        public static bool VerifyString(string text)
        {
            var parts = StringUtil.ParseTwoPart(text, "=");
            string source = parts[0];
            string encoded = parts[1];
            var bytes = Encoding.UTF8.GetBytes(source);
            return ByteArray.Compare(bytes, ByteArray.GetTimeStampByteArray(encoded)) == 0;
        }

        public int OutputEntities(
            Stop stop,
            LibraryChannel channel,
            string strBiblioRecPath,
            string strDbType,
            bool bCreate905,
            string str905Style,
            bool bCreate906,
            MarcRecord record,
            out string strError)
        {
            strError = "";

            /*
        只创建单个 905 字段
        每册一个 905 字段
             * * */

            try
            {
                SubItemLoader loader = new SubItemLoader();
                loader.BiblioRecPath = strBiblioRecPath;
                loader.Channel = channel;
                loader.Stop = stop;
                loader.DbType = strDbType;

                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                if (bCreate905)
                {
                    if (str905Style == "每册一个 905 字段")
                    {
                        foreach (EntityInfo info in loader)
                        {
                            if (info.ErrorCode != ErrorCodeValue.NoError)
                            {
                                strError = "路径为 '" + info.OldRecPath + "' 的册记录装载中发生错误: " + info.ErrorInfo;  // NewRecPath
                                return -1;
                            }

                            XmlDocument item_dom = new XmlDocument();
                            item_dom.LoadXml(info.OldRecord);

                            string strAccessNo = DomUtil.GetElementText(item_dom.DocumentElement, "accessNo");
                            string strLocation = DomUtil.GetElementText(item_dom.DocumentElement, "location");
                            strLocation = StringUtil.GetPureLocation(strLocation);

                            dp2Circulation.MainForm.AccessNoInfo accessNoInfo = null;
                            // 解析索取号字符串
                            // return:
                            //      -1  error
                            //      0   排架体系定义没有找到
                            //      1   成功
                            int nRet = Program.MainForm.ParseAccessNo(
                                strLocation,
                                strAccessNo,
                                out accessNoInfo,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            string strBarcode = DomUtil.GetElementText(item_dom.DocumentElement, "barcode");

                            MarcField field = new MarcField("905", "  ");

                            if (accessNoInfo.HasHeadLine)
                            {
                                field.add(new MarcSubfield("a", accessNoInfo.HeadLine));
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(strLocation) == false)
                                    field.add(new MarcSubfield("a", strLocation));
                            }

                            if (string.IsNullOrEmpty(strAccessNo) == false)
                            {
                                field.add(new MarcSubfield("d", accessNoInfo.ClassLine));
                                field.add(new MarcSubfield("e", accessNoInfo.QufenhaoLine));
                            }
                            if (string.IsNullOrEmpty(strBarcode) == false)
                                field.add(new MarcSubfield("b", strBarcode));
                            if (field.Subfields.count > 0)
                                record.add(field);
                        }
                    }
                    else if (str905Style == "只创建单个 905 字段"
                        || string.IsNullOrEmpty(str905Style))
                    {
                        dp2Circulation.MainForm.AccessNoInfo first_accessNoInfo = null;
                        List<string> barcodes = new List<string>();
                        string strFirstLocation = "";

                        foreach (EntityInfo info in loader)
                        {
                            if (info.ErrorCode != ErrorCodeValue.NoError)
                            {
                                strError = "路径为 '" + info.OldRecPath + "' 的册记录装载中发生错误: " + info.ErrorInfo;  // NewRecPath
                                return -1;
                            }

                            XmlDocument item_dom = new XmlDocument();
                            item_dom.LoadXml(info.OldRecord);

                            string strLocation = DomUtil.GetElementText(item_dom.DocumentElement, "location");
                            strLocation = StringUtil.GetPureLocation(strLocation);

                            if (string.IsNullOrEmpty(strFirstLocation))
                            {
                                if (string.IsNullOrEmpty(strLocation) == false)
                                    strFirstLocation = strLocation;
                            }

                            // TODO: 要按照排架体系定义，分析出索取号的各行。比如三行的索取号
                            if (first_accessNoInfo == null)
                            {
                                string strAccessNo = DomUtil.GetElementText(item_dom.DocumentElement, "accessNo");
                                if (string.IsNullOrEmpty(strAccessNo) == false)
                                {
                                    // 解析索取号字符串
                                    // return:
                                    //      -1  error
                                    //      0   排架体系定义没有找到
                                    //      1   成功
                                    int nRet = Program.MainForm.ParseAccessNo(
                                        strLocation,
                                        strAccessNo,
                                        out first_accessNoInfo,
                                        out strError);
                                    if (nRet == -1)
                                        return -1;
                                }
                            }
                            string strBarcode = DomUtil.GetElementText(item_dom.DocumentElement, "barcode");
                            if (string.IsNullOrEmpty(strBarcode) == false)
                                barcodes.Add(strBarcode);
                        }

                        {

                            MarcField field = new MarcField("905", "  ");

                            if (first_accessNoInfo != null
                                && first_accessNoInfo.HasHeadLine)
                            {
                                field.add(new MarcSubfield("a", first_accessNoInfo.HeadLine));
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(strFirstLocation) == false)
                                    field.add(new MarcSubfield("a", strFirstLocation));
                            }

                            if (first_accessNoInfo != null)
                            {
                                field.add(new MarcSubfield("d", first_accessNoInfo.ClassLine));
                                field.add(new MarcSubfield("e", first_accessNoInfo.QufenhaoLine));
                            }
                            foreach (string strBarcode in barcodes)
                            {
                                field.add(new MarcSubfield("b", strBarcode));
                            }
                            record.add(field);
                        }
                    }
                    else
                    {
                        strError = "无法识别的 str905Style '" + str905Style + "'";
                        return -1;
                    }
                }

                if (bCreate906)
                {
                    MarcField field = new MarcField("906", "  ");

                    foreach (EntityInfo info in loader)
                    {
                        if (info.ErrorCode != ErrorCodeValue.NoError)
                        {
                            strError = "路径为 '" + info.OldRecPath + "' 的册记录装载中发生错误: " + info.ErrorInfo;  // NewRecPath
                            return -1;
                        }

                        XmlDocument item_dom = new XmlDocument();
                        item_dom.LoadXml(info.OldRecord);

                        // $a 册条码号
                        {
                            string strBarcode = DomUtil.GetElementText(item_dom.DocumentElement, "barcode");
                            field.add(new MarcSubfield("a", strBarcode));
                        }

                        // $b 入藏库
                        {
                            string strLocation = DomUtil.GetElementText(item_dom.DocumentElement, "location");
                            strLocation = StringUtil.GetPureLocation(strLocation);
                            if (string.IsNullOrEmpty(strLocation) == false)
                                field.add(new MarcSubfield("b", strLocation));
                        }

                        // TODO: 要按照排架体系定义，分析出索取号的各行。比如三行的索取号
                        // $c架位
                        {
                            string strAccessNo = DomUtil.GetElementText(item_dom.DocumentElement, "accessNo");

                            strAccessNo = StringUtil.BuildLocationClassEntry(strAccessNo);

                            if (string.IsNullOrEmpty(strAccessNo) == false)
                                field.add(new MarcSubfield("c", strAccessNo));
                        }

                        // $d册价格
                        {
                            string strPrice = DomUtil.GetElementText(item_dom.DocumentElement, "price");
                            if (string.IsNullOrEmpty(strPrice) == false)
                                field.add(new MarcSubfield("d", strPrice));
                        }

                        // $f册类型
                        {
                            string strBookType = DomUtil.GetElementText(item_dom.DocumentElement, "bookType");
                            if (string.IsNullOrEmpty(strBookType) == false)
                                field.add(new MarcSubfield("f", strBookType));
                        }

                        // $h登录号
                        {
                            string strRegisterNo = DomUtil.GetElementText(item_dom.DocumentElement, "registerNo");
                            if (String.IsNullOrEmpty(strRegisterNo) == false)
                                field.add(new MarcSubfield("h", strRegisterNo));
                        }

                        // $r借阅者条码号
                        {
                            string strBorrower = DomUtil.GetElementText(item_dom.DocumentElement, "borrower");
                            if (String.IsNullOrEmpty(strBorrower) == false)
                                field.add(new MarcSubfield("r", strBorrower));
                        }

                        // $s图书状态
                        {
                            string strState = DomUtil.GetElementText(item_dom.DocumentElement, "state");
                            if (String.IsNullOrEmpty(strState) == false)
                                field.add(new MarcSubfield("s", strState));
                        }

                        // $z附注
                        {
                            string strComment = DomUtil.GetElementText(item_dom.DocumentElement, "comment");
                            if (String.IsNullOrEmpty(strComment) == false)
                                field.add(new MarcSubfield("z", strComment));
                        }

                        if (field.Subfields.count > 0)
                            record.add(field);
                    }

                }

                return 1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }

        // 保存到记录路径文件
        void menu_saveToRecordPathFile_Click(object sender, EventArgs e)
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
                    "BiblioSearchForm",
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
                bAppend,    // append
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

            Program.MainForm.StatusBarMessage = "书目记录路径 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportRecPathFilename;
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

        public override void OnSelectedIndexChanged()
        {
            if (this.listView_records.SelectedIndices.Count == 0)
                this.LabelMessage = "";
            else
            {
                if (this.listView_records.SelectedIndices.Count == 1)
                {
                    this.LabelMessage = "第 " + (this.listView_records.SelectedIndices[0] + 1).ToString() + " 行";
                }
                else
                {
                    this.LabelMessage = "从 " + (this.listView_records.SelectedIndices[0] + 1).ToString() + " 行开始，共选中 " + this.listView_records.SelectedIndices.Count.ToString() + " 个事项";
                }
            }

            ListViewUtil.OnSelectedIndexChanged(this.listView_records,
                0,
                null);

            if (this.m_biblioTable != null)
            {
                // if (CanCallNew(commander, WM_SELECT_INDEX_CHANGED) == true)
                RefreshPropertyView(false);
            }
        }

#if NO
        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SELECT_INDEX_CHANGED:
                    {
                        if (this.listView_records.SelectedIndices.Count == 0)
                            this.label_message.Text = "";
                        else
                        {
                            if (this.listView_records.SelectedIndices.Count == 1)
                            {
                                this.label_message.Text = "第 " + (this.listView_records.SelectedIndices[0] + 1).ToString() + " 行";
                            }
                            else
                            {
                                this.label_message.Text = "从 " + (this.listView_records.SelectedIndices[0] + 1).ToString() + " 行开始，共选中 " + this.listView_records.SelectedIndices.Count.ToString() + " 个事项";
                            }
                        }

                        ListViewUtil.OnSelectedIndexChanged(this.listView_records,
                            0,
                            null);

                        if (this.m_biblioTable != null)
                        {
                            if (CanCallNew(commander, m.Msg) == true)
                                RefreshPropertyView(false);
                        }
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }
#endif

        /*public*/
        bool CanCallNew(Commander commander, int msg)
        {
            if (this.m_nInViewing > 0)
            {
                // 缓兵之计
                // this.Stop();
                commander.AddMessage(msg);
                return false;   // 还不能启动
            }

            return true;    // 可以启动
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


            /*
            // 清除以前累积的消息
            while (API.PeekMessage(ref msg,
                this.Handle,
                (uint)WM_SELECT_INDEX_CHANGED,
                (uint)WM_SELECT_INDEX_CHANGED,
                API.PM_REMOVE)) ;
            API.PostMessage(this.Handle, WM_SELECT_INDEX_CHANGED, 0, 0);
            */
#endif

            // this.commander.AddMessage(WM_SELECT_INDEX_CHANGED);
            this.TriggerSelectedIndexChanged();
        }

        private void textBox_queryWord_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_search;
        }

        private void textBox_queryWord_Leave(object sender, EventArgs e)
        {
            this.AcceptButton = null;
        }

        private void textBox_queryWord_TextChanged(object sender, EventArgs e)
        {
            // this.Text = "书目查询 " + this.textBox_queryWord.Text;
            this.SetTitle(GetFirstQueryWord(this.textBox_queryWord.Text));
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

        // 调节各个事项之间的并存冲突
        private void checkedComboBox_biblioDbNames_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            CheckedComboBox.ProcessItemChecked(e, "<全部>,<all>,<全部书目>,<all biblio>".ToLower());
#if NO
            ListView list = e.Item.ListView;

            if (e.Item.Text == "<全部>" || e.Item.Text.ToLower() == "<all>")
            {
                if (e.Item.Checked == true)
                {
                    // 如果当前勾选了“全部”，则清除其余全部事项的勾选
                    for (int i = 0; i < list.Items.Count; i++)
                    {
                        ListViewItem item = list.Items[i];
                        if (item.Text == "<全部>" || item.Text.ToLower() == "<all>")
                            continue;
                        if (item.Checked != false)
                            item.Checked = false;
                    }
                }
            }
            else
            {
                if (e.Item.Checked == true)
                {
                    // 如果勾选的不是“全部”，则要清除“全部”上可能的勾选
                    for (int i = 0; i < list.Items.Count; i++)
                    {
                        ListViewItem item = list.Items[i];
                        if (item.Text == "<全部>" || item.Text.ToLower() == "<all>")
                        {
                            if (item.Checked != false)
                                item.Checked = false;
                        }
                    }
                }
            }
#endif
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

        private void ToolStripMenuItem_searchKeyID_Click(object sender, EventArgs e)
        {
        }



        // 普通检索(不返回key部分)
        private async void toolStripButton_search_Click(object sender, EventArgs e)
        {
            await DoSearch(false, false);
        }

        public Task DoSearch(bool bOutputKeyCount,
            bool bOutputKeyID,
            ItemQueryParam input_query = null)
        {
            // 注：只有这样才能在独立线程中执行
            return Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        await _doSearch(bOutputKeyCount,
                            bOutputKeyID,
                            input_query);
                    }
                    catch (Exception ex)
                    {
                        this.MessageBoxShow($"DoSearch() 异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                },
                default,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        // 继续装入
        async private void ToolStripMenuItem_continueLoad_Click(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(
                () =>
                {
                    string strError = "";

                    string strResultSetName = GetResultSetName(false);

                    long lHitCount = this.m_lHitCount;
                    if (this.listView_records.Items.Count >= lHitCount)
                    {
                        strError = "浏览信息已经全部装载完毕了，没有必要继续装载";
                        goto ERROR1;
                    }

                    bool bQuickLoad = false;
                    if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                        bQuickLoad = true;

                    var looping = Looping(out LibraryChannel channel,
                        "正在继续装入浏览信息 ...",
                        "timeout:0:2:0,halfstop");
                    this.EnableControlsInSearching(false);
                    try
                    {
                        long lTotalHitCount = lHitCount;

                        // TODO: 应该是上次中断时保存一个已装入的总数，这样可以避免 listView 中后来移除过行以后对继续装入的起点发生影响
                        long lStart = this.listView_records.Items.Count;

                        looping.Progress.SetProgressRange(0, lHitCount);

                        this.LabelMessage = "正在继续装入浏览信息...";
                        int nRet = LoadResultSet(
        looping.Progress,
        channel,
        strResultSetName,
        _outputKeyCount,
        _outputKeyID,
        bQuickLoad,
        lHitCount,
        lStart,
        _query,
        ref lTotalHitCount,
        out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
                        this.LabelMessage = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已全部装入";
                        return;
                    }
                    catch (InterruptException)
                    {
                        this.LabelMessage = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已装入 " + this.listView_records.Items.Count.ToString() + " 条，用户中断...";
                        return;
                    }
                    catch (Exception ex)
                    {
                        strError = "装入浏览信息出现异常: " + ExceptionUtil.GetExceptionText(ex);
                        goto ERROR1;
                    }
                    finally
                    {
                        this.EnableControlsInSearching(true);
                        looping.Dispose();
                    }
                ERROR1:
                    this.MessageBoxShow(strError);
                },
    default,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);
        }

#if OLDCODE
        // 继续装入
        private void ToolStripMenuItem_continueLoad_Click(object sender, EventArgs e)
        {
            string strError = "";

            long lHitCount = this.m_lHitCount;
            if (this.listView_records.Items.Count >= lHitCount)
            {
                strError = "浏览信息已经全部装载完毕了，没有必要继续装载";
                goto ERROR1;
            }


            bool bQuickLoad = false;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                bQuickLoad = true;

            LibraryChannel channel = this.GetChannel();

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在检索 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在检索 ...", "halfstop");


            this.EnableControlsInSearching(false);
            try
            {
                looping.Progress.SetProgressRange(0, lHitCount);

                long lStart = this.listView_records.Items.Count;
                long lPerCount = Math.Min(50, lHitCount);
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                bool bPushFillingBrowse = PushFillingBrowse;

                this.label_message.Text = "正在继续装入浏览信息...";

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已装入 " + lStart.ToString() + " 条，用户中断...";
                        return;
                    }

                    looping.Progress.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    /*
                    string strStyle = "id,cols";
                    if (this.m_bFirstColumnIsKey == true)
                        strStyle = "keyid,id,key,cols";
                     * */

                    bool bTempQuickLoad = bQuickLoad;

                    if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                        bTempQuickLoad = true;

                    /*
                    string strStyle = "id,cols";
                    if (bTempQuickLoad == true)
                    {
                        if (this.m_bFirstColumnIsKey == true)
                            strStyle = "keyid,id,key";    // 没有了cols部分
                        else
                            strStyle = "id";
                    }
                    else
                    {
                        // 
                        if (this.m_bFirstColumnIsKey == true)
                            strStyle = "keyid,id,key,cols";
                        else
                            strStyle = "id,cols";
                    }
                    */

                    channel.Timeout = new TimeSpan(0, 5, 0);
                    long lRet = channel.GetSearchResult(
                        looping.Progress,
                        null,   // strResultSetName
                        lStart,
                        lPerCount,
                        _lastBrowseStyle,
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        this.label_message.Text = "检索 dp2 共命中 " + lHitCount.ToString() + " 条书目记录，已装入 " + lStart.ToString() + " 条，" + strError;
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        MessageBox.Show(this, "dp2 未命中");
                        return;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DigitalPlatform.LibraryClient.localhost.Record searchresult = searchresults[i];

                        string[] cols = null;
                        if (this.m_bFirstColumnIsKey == true)
                        {
                            // 输出keys
                            if (searchresult.Cols == null
                                && bTempQuickLoad == false)
                            {
                                strError = "要使用获取检索点功能，请将 dp2Library 应用服务器和 dp2Kernel 数据库内核升级到最新版本";
                                goto ERROR1;
                            }
                            cols = new string[(searchresult.Cols == null ? 0 : searchresult.Cols.Length) + 1];
                            cols[0] = LibraryChannel.BuildDisplayKeyString(searchresult.Keys);
                            if (cols.Length > 1)
                                Array.Copy(searchresult.Cols, 0, cols, 1, cols.Length - 1);
                        }
                        else
                        {
                            cols = searchresult.Cols;
                        }

                        if (bPushFillingBrowse == true)
                        {
                            if (bTempQuickLoad == true)
                                Global.InsertNewLine(
                                    (ListView)this.listView_records,
                                    searchresult.Path,
                                    cols);
                            else
                                Global.InsertNewLine(
                                    this.listView_records,
                                    searchresult.Path,
                                    cols);
                        }
                        else
                        {
                            if (bTempQuickLoad == true)
                                Global.AppendNewLine(
                                    (ListView)this.listView_records,
                                    searchresult.Path,
                                    cols);
                            else
                                Global.AppendNewLine(
                                this.listView_records,
                                searchresult.Path,
                                cols);
                        }
                    }

                    lStart += searchresults.Length;
                    // lCount -= searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;

                    this.m_lLoaded = lStart;
                    looping.Progress.SetProgressValue(lStart);
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
                this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已全部装入";
                return;
            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControlsInSearching(true);
            }
        ERROR1:
            MessageBoxShow(strError);
        }

#endif

        private async void toolStripMenuItem_searchKeyID_Click(object sender, EventArgs e)
        {
            await DoSearch(false, true);
        }

        // 尺寸为0,0的按钮，为了满足this.AcceptButton
        private async void button_search_Click(object sender, EventArgs e)
        {
            await DoSearch(false, false);
        }

        private void dp2QueryControl1_GetList(object sender, DigitalPlatform.CommonControl.GetListEventArgs e)
        {
            // 获得所有数据库名
            if (string.IsNullOrEmpty(e.Path) == true)
            {
                if (Program.MainForm.BiblioDbProperties != null)
                {
                    foreach (var property in Program.MainForm.BiblioDbProperties)
                    {
                        // BiblioDbProperty property = Program.MainForm.BiblioDbProperties[i];
                        e.Values.Add(property.DbName);
                    }
                }
            }
            else
            {
                // 获得特定数据库的检索途径
                // 每个库都一样
                for (int i = 0; i < Program.MainForm.BiblioDbFromInfos.Length; i++)
                {
                    BiblioDbFromInfo info = Program.MainForm.BiblioDbFromInfos[i];
                    e.Values.Add(info.Caption);   // + "\t" + info.Style);
                }
            }
        }

        private void dp2QueryControl1_ViewXml(object sender, EventArgs e)
        {
            int nRet = dp2QueryControl1.BuildQueryXml(
        MaxSearchResultCount,
        "zh",
        out string strQueryXml,
        out string strError);
            if (nRet == -1)
            {
                strError = "在创建XML检索式的过程中出错: " + strError;
                goto ERROR1;
            }

            XmlViewerForm dlg = new XmlViewerForm();

            dlg.Text = "检索式XML";
            // dlg.MainForm = Program.MainForm;
            dlg.XmlString = strQueryXml;
            // dlg.StartPosition = FormStartPosition.CenterScreen;

            Program.MainForm.AppInfo.LinkFormState(dlg, "bibliosearchform_viewqueryxml");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

        private void dp2QueryControl1_AppendMenu(object sender, AppendMenuEventArgs e)
        {
            MenuItem menuItem = null;

            menuItem = new MenuItem("检索(&S)");
            menuItem.Click += new System.EventHandler(this.menu_logicSearch_Click);
            e.ContextMenu.MenuItems.Add(menuItem);

            // 继续装入
            menuItem = new MenuItem("继续装入(&C)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_continueLoad_Click);
            if (this.m_lHitCount <= this.listView_records.Items.Count)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            e.ContextMenu.MenuItems.Add(menuItem);

            // 带检索点的检索
            menuItem = new MenuItem("带检索点的检索(&K)");
            menuItem.Click += new System.EventHandler(this.menu_logicSearchKeyID_Click);
            e.ContextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            e.ContextMenu.MenuItems.Add(menuItem);
        }

        async void menu_logicSearch_Click(object sender, EventArgs e)
        {
            await this.DoLogicSearch(false);
        }

        async void menu_logicSearchKeyID_Click(object sender, EventArgs e)
        {
            await this.DoLogicSearch(true);
        }

        // 
        /// <summary>
        /// 向浏览框末尾新加入一行
        /// </summary>
        /// <param name="strLine">要加入的行内容。每列内容之间用字符 '\t' 间隔</param>
        /// <returns>新创建的 ListViewItem 对象</returns>
        public ListViewItem AddLineToBrowseList(string strLine)
        {
            ListViewItem item = Global.BuildListViewItem(
        this.listView_records,
        strLine);

            this.listView_records.Items.Add(item);
            return item;
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
            Program.MainForm.AppInfo.LinkFormState(dlg, "searchbiblioform_gettimedialog_single");
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

            Program.MainForm.AppInfo.LinkFormState(dlg, "searchbiblioform_gettimedialog_single");
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
            Program.MainForm.AppInfo.LinkFormState(dlg, "searchbiblioform_gettimedialog_range");
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

            Program.MainForm.AppInfo.LinkFormState(dlg, "searchbiblioform_gettimedialog_range");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_queryWord.Text = dlg.uString;
        }

        private async void toolStripMenuItem_searchKeys_Click(object sender, EventArgs e)
        {
            await DoSearch(true, false);
        }

        private void toolStripButton_prevQuery_Click(object sender, EventArgs e)
        {
            ItemQueryParam query = PrevQuery();
            if (query != null)
            {
                QueryToPanel(query);
            }
        }

        private void toolStripButton_nextQuery_Click(object sender, EventArgs e)
        {
            ItemQueryParam query = NextQuery();
            if (query != null)
            {
                QueryToPanel(query);
            }
        }

        private void dp2QueryControl1_GetFromStyle(object sender, GetFromStyleArgs e)
        {
            e.FromStyles = GetBiblioFromStyle(this._dbType, e.FromCaption);
        }

        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetItemMarc(
            ListViewItem item,
            out string strMARC,
            out string strMarcSyntax,
            out string strError)
        {
            strError = "";
            strMARC = "";
            strMarcSyntax = "";

            BiblioInfo info = null;

            int nRet = GetBiblioInfo(
                true,
                item,
                out info,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            nRet = MarcUtil.Xml2Marc(info.OldXml,    // info.OldXml,
                true,
                null,
                out strMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            return 1;
        }

        // 根据 ListViewItem 对象得到 BiblioInfo 对象
        int GetBiblioInfo(
            bool bCheckSearching,
            ListViewItem item,
            out BiblioInfo info,
            out string strError)
        {
            strError = "";
            info = null;

            if (this.m_biblioTable == null)
                return 0;

            string strRecPath = item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return 0;

            // 存储所获得书目记录 XML
            info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
            {
                info = new BiblioInfo();
                info.RecPath = strRecPath;
                this.m_biblioTable[strRecPath] = info;
            }

            if (string.IsNullOrEmpty(info.OldXml) == true)
            {
                if (bCheckSearching == true)
                {
                    if (this.InSearching == true)
                        return 0;
                }

                LibraryChannel channel = this.GetChannel();
                try
                {
                    // 获得书目记录
                    long lRet = channel.GetBiblioInfos(
                        null,   // _stop,
                        strRecPath,
                        "",
                        new string[] { "xml" },   // formats
                        out string[] results,
                        out byte[] baTimestamp,
                        out strError);
                    if (lRet == 0)
                        return -1;  // 是否设定为特殊状态?
                    if (lRet == -1)
                        return -1;

                    if (results == null || results.Length == 0)
                    {
                        strError = "results error";
                        return -1;
                    }

                    string strXml = results[0];
                    info.OldXml = strXml;
                    info.Timestamp = baTimestamp;
                    info.RecPath = strRecPath;
                }
                finally
                {
                    this.ReturnChannel(channel);
                }
            }

            return 1;
        }

        int m_nInViewing = 0;

        /// <summary>
        /// 显示当前选定行的属性
        /// </summary>
        /// <param name="bOpenWindow">是否要打开浮动对话框</param>
        public void RefreshPropertyView(bool bOpenWindow)
        {
            m_nInViewing++;
            try
            {
                ListViewItem item = null;
                this.TryInvoke(() =>
                {
                    if (this.listView_records.SelectedItems.Count == 1)
                        item = this.listView_records.SelectedItems[0];
                });

                _doViewProperty(item, bOpenWindow);
            }
            finally
            {
                m_nInViewing--;
            }
        }

        public void DisplayProperty(ListViewItem item, bool bOpenWindow)
        {
            m_nInViewing++;
            try
            {
                _doViewProperty(item, bOpenWindow);
            }
            finally
            {
                m_nInViewing--;
            }
        }

        void _doViewProperty(ListViewItem item, bool bOpenWindow)
        {
            if (this.m_biblioTable == null)
                return;

            if (item == null)
                return; // 是否要显示一个空画面?

            Program.MainForm.OpenCommentViewer(bOpenWindow);

            string strRecPath = ListViewUtil.GetItemText(item, 0);  //  item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return;

            // TODO: 可触发显示检索式详情
            if (IsCmdLine(strRecPath))
            {
                Program.MainForm.PropertyTaskList.AddTask(new BiblioPropertyTask { Stop = null/*this._stop*/ }, true);
                return;
            }

            // 存储所获得书目记录 XML
            BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
            {
                info = new BiblioInfo();
                info.RecPath = strRecPath;
                this.m_biblioTable[strRecPath] = info;  // 后面任务中会填充 info 的内容，如果必要的话
            }

            BiblioPropertyTask task = new BiblioPropertyTask();
            task.BiblioInfo = info;
            task.Stop = null;   /* this._stop;*/
            task.DisplaySubrecords = DisplaySubrecords;

            Program.MainForm.PropertyTaskList.AddTask(task, true);
        }

#if NO
        void _doViewProperty(ListViewItem item, bool bOpenWindow)
        {
            string strError = "";
            string strHtml = "";
            // string strXml = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (Program.MainForm.PanelFixedVisible == false
                    && (m_commentViewer == null || m_commentViewer.Visible == false))
                    return;
                // 2013/3/7
                if (Program.MainForm.CanDisplayItemProperty() == false)
                    return;
            }

            if (this.m_biblioTable == null
                || item == null)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }

            // BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            BiblioInfo info = null;
            int nRet = GetBiblioInfo(
                true,
                item,
                out info,
                out strError);
            if (info == null)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }

            string strXml1 = "";
            string strHtml2 = "";
            string strXml2 = "";

            if (nRet == -1)
            {
                strHtml2 = HttpUtility.HtmlEncode(strError);
            }
            else
            {
                nRet = GetXmlHtml(info,
                    out strXml1,
                    out strXml2,
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            strHtml = "<html>" +
    GetHeadString() +
    "<body>" +
    strHtml2 +
    EntityForm.GetTimestampHtml(info.Timestamp) +
    "</body></html>";
            bool bNew = false;
            if (this.m_commentViewer == null
                || (bOpenWindow == true && this.m_commentViewer.Visible == false))
            {
                m_commentViewer = new CommentViewerForm();
                MainForm.SetControlFont(m_commentViewer, this.Font, false);
                bNew = true;
            }

            m_commentViewer.MainForm = Program.MainForm;  // 必须是第一句

            if (bNew == true)
                m_commentViewer.InitialWebBrowser();

            m_commentViewer.Text = "MARC内容 '" + info.RecPath + "'";
            m_commentViewer.HtmlString = strHtml;
            m_commentViewer.XmlString = MergeXml(strXml1, strXml2);
            m_commentViewer.FormClosed -= new FormClosedEventHandler(marc_viewer_FormClosed);
            m_commentViewer.FormClosed += new FormClosedEventHandler(marc_viewer_FormClosed);
            // Program.MainForm.AppInfo.LinkFormState(m_viewer, "comment_viewer_state");
            // m_viewer.ShowDialog(this);
            // Program.MainForm.AppInfo.UnlinkFormState(m_viewer);
            if (bOpenWindow == true)
            {
                if (m_commentViewer.Visible == false)
                {
                    Program.MainForm.AppInfo.LinkFormState(m_commentViewer, "marc_viewer_state");
                    m_commentViewer.Show(this);
                    m_commentViewer.Activate();

                    Program.MainForm.CurrentPropertyControl = null;
                }
                else
                {
                    if (m_commentViewer.WindowState == FormWindowState.Minimized)
                        m_commentViewer.WindowState = FormWindowState.Normal;
                    m_commentViewer.Activate();
                }
            }
            else
            {
                if (m_commentViewer.Visible == true)
                {

                }
                else
                {
                    if (Program.MainForm.CurrentPropertyControl != m_commentViewer.MainControl)
                        m_commentViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, "_doViewProperty() 出错: " + strError);
        }
#endif

        internal static string MergeXml(string strXml1,
            string strXml2)
        {
            if (string.IsNullOrEmpty(strXml1) == true)
                return strXml2;
            if (string.IsNullOrEmpty(strXml2) == true)
                return strXml1;

            return strXml1; // 临时这样
        }

        internal static int GetXmlHtml(BiblioInfo info,
            out string strXml1,
            out string strXml2,
            out string strHtml2,
            out string strError)
        {
            strError = "";
            strXml1 = "";
            strXml2 = "";
            strHtml2 = "";
            int nRet = 0;

            strXml1 = info.OldXml;
            strXml2 = info.NewXml;

            string strOldMARC = "";
            string strOldFragmentXml = "";
            if (string.IsNullOrEmpty(strXml1) == false)
            {
                string strOutMarcSyntax = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                nRet = MarcUtil.Xml2Marc(strXml1,
                    MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                    "",
                    out strOutMarcSyntax,
                    out strOldMARC,
                    out strOldFragmentXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML转换到MARC记录时出错: " + strError;
                    return -1;
                }
            }

            string strNewMARC = "";
            string strNewFragmentXml = "";
            if (string.IsNullOrEmpty(strXml2) == false)
            {
                string strOutMarcSyntax = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                nRet = MarcUtil.Xml2Marc(strXml2,
                    MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                    "",
                    out strOutMarcSyntax,
                    out strNewMARC,
                    out strNewFragmentXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML转换到MARC记录时出错: " + strError;
                    return -1;
                }
            }

            if (string.IsNullOrEmpty(strOldMARC) == false
                && string.IsNullOrEmpty(strNewMARC) == false)
            {
                string strOldImageFragment = GetCoverImageHtmlFragment(
        info.RecPath,
        strOldMARC);
                string strNewImageFragment = GetCoverImageHtmlFragment(
        info.RecPath,
        strNewMARC);

                // 创建展示两个 MARC 记录差异的 HTML 字符串
                // return:
                //      -1  出错
                //      0   成功
                nRet = MarcDiff.DiffHtml(
                    strOldMARC,
                    strOldFragmentXml,
                    strOldImageFragment,
                    strNewMARC,
                    strNewFragmentXml,
                    strNewImageFragment,
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else if (string.IsNullOrEmpty(strOldMARC) == false
        && string.IsNullOrEmpty(strNewMARC) == true)
            {
                string strImageFragment = GetCoverImageHtmlFragment(
                    info.RecPath,
                    strOldMARC);
                strHtml2 = MarcUtil.GetHtmlOfMarc(strOldMARC,
                    strOldFragmentXml,
                    strImageFragment,
                    false);
            }
            else if (string.IsNullOrEmpty(strOldMARC) == true
                && string.IsNullOrEmpty(strNewMARC) == false)
            {
                string strImageFragment = GetCoverImageHtmlFragment(
        info.RecPath,
        strNewMARC);
                strHtml2 = MarcUtil.GetHtmlOfMarc(strNewMARC,
                    strNewFragmentXml,
                    strImageFragment,
                    false);
            }
            return 0;
        }

        public static string GetIsbnImageHtmlFragment(string strMARC,
            string strMarcSyntax)
        {
            List<string> isbns = new List<string>();
            MarcRecord record = new MarcRecord(strMARC);
            if (strMarcSyntax == "usmarc")
            {

            }
            else
            {
                // unimarc

                MarcNodeList subfields = record.select("field[@name='010']/subfield[@name='a']");
                foreach (MarcSubfield subfield in subfields)
                {
                    if (string.IsNullOrEmpty(subfield.Content) == false)
                        isbns.Add(IsbnSplitter.GetISBnBarcode(subfield.Content));   // 如果必要，转换为 13 位
                }
            }

            StringBuilder text = new StringBuilder();
            foreach (string isbn in isbns)
            {
                Hashtable table = new Hashtable();
                table["code"] = isbn;
                table["type"] = "ean_13";
                table["width"] = "300";
                table["height"] = "80";
                string path = StringUtil.BuildParameterString(table, ',', '=', "url");
                text.Append("<img src='barcode:" + path + "'></img><br/>");
            }

            return text.ToString();
        }

        public static string GetCoverImageHtmlFragment(
            string strBiblioRecPath,
            string strMARC)
        {
            string strImageUrl = ScriptUtil.GetCoverImageUrl(strMARC, "MediumImage");

            if (string.IsNullOrEmpty(strImageUrl) == true)
                return "";

            if (StringUtil.IsHttpUrl(strImageUrl) == true)
                return "<img src='" + strImageUrl + "'></img>";

            string strUri = ScriptUtil.MakeObjectUrl(strBiblioRecPath,
                  strImageUrl);
            return "<img class='pending' name='"
            + (strBiblioRecPath == "?" ? "?" : "object-path:" + strUri)
            + "' src='%mappeddir%\\images\\ajax-loader.gif' alt='封面图片'></img>";
        }

        void marc_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_commentViewer != null)
            {
                Program.MainForm.AppInfo.UnlinkFormState(m_commentViewer);
                this.m_commentViewer = null;
            }
        }

        string GetHeadString(bool bAjax = true)
        {
            return Program.MainForm.GetMarcHtmlHeadString(bAjax);
#if NO
            string strCssFilePath = PathUtil.MergePath(Program.MainForm.DataDir, "operloghtml.css");

            if (bAjax == true)
                return
                    "<head>" +
                    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
                    "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>" +
                    "<script type='text/javascript' charset='UTF-8' src='%datadir%\\getsummary.js" + "'></script>" +
                    "</head>";
            return
    "<head>" +
    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
    "</head>";
#endif
        }

        public string QueryWord
        {
            get
            {
                return (string)this.Invoke((Func<string>)(() =>
                {
                    return this.textBox_queryWord.Text;
                }));
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.textBox_queryWord.Text = value;
                });
            }
        }

        // 2022/11/14
        public string DbNames
        {
            get
            {
                return (string)this.Invoke((Func<string>)(() =>
                {
                    return this.checkedComboBox_biblioDbNames.Text;
                }));
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkedComboBox_biblioDbNames.Text = value;
                });
            }
        }

        public string From
        {
            get
            {
                return (string)this.Invoke((Func<string>)(() =>
                {
                    return this.comboBox_from.Text;
                }));
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.comboBox_from.Text = value;
                });
            }
        }

        public string MatchStyle
        {
            get
            {
                return (string)this.Invoke((Func<string>)(() =>
                {
                    return this.comboBox_matchStyle.Text;
                }));
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.comboBox_matchStyle.Text = value;
                });
            }
        }

        // 2022/11/14
        public string LocationFilter
        {
            get
            {
                return (string)this.Invoke((Func<string>)(() =>
                {
                    return this.comboBox_location.Text;
                }));
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.comboBox_location.Text = value;
                });
            }
        }

        // 2022/11/14
        public bool MultiLine
        {
            get
            {
                return (bool)this.Invoke((Func<bool>)(() =>
                {
                    return this.toolStripButton_multiLine.Checked;
                }));
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.toolStripButton_multiLine.Checked = value;
                });
            }
        }

        public string LabelMessage
        {
            get
            {
                return (string)this.Invoke((Func<string>)(() =>
                {
                    return this.label_message.Text;
                }));
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.label_message.Text = value;
                });
            }
        }

        private void ToolStripMenuItem_searchShareBiblio_Click(object sender, EventArgs e)
        {
            if (this.SearchShareBiblio == false)
            {
                // 先检查一下当前用户是否允许开放共享书目
                if (Program.MainForm != null && Program.MainForm.MessageHub != null
        && Program.MainForm.MessageHub.ShareBiblio == false)
                {
                    DialogResult result = MessageBox.Show(this,
        "使用共享书目检索功能须知：为检索网络中共享书目记录，您必须明确同意允许他人检索您所在图书馆的全部书目记录。\r\n\r\n请问您是否允许他人从现在起一直能访问您所在图书馆的书目记录？\r\n\r\n(是: 同意；否：不同意)\r\n\r\n(除了在这里设定相关参数，您还可以稍后用“参数配置”对话框的“消息”属性页来设定是否允许共享书目数据)",
        "BiblioSearchForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        this.ShowMessage("已放弃使用共享网络", "");
                        this._floatingMessage.DelayClear(new TimeSpan(0, 0, 3));
                        return;
                    }
                    Program.MainForm.MessageHub.ShareBiblio = true;
                }

                this.SearchShareBiblio = true;
            }
            else
                this.SearchShareBiblio = false;

            UpdateSearchShareMenu();
        }

        // 更新菜单状态
        void UpdateSearchShareMenu()
        {
            this.ToolStripMenuItem_searchShareBiblio.Checked = this.SearchShareBiblio;
            if (Program.MainForm != null && Program.MainForm.MessageHub != null)
            {
                if (Program.MainForm.MessageHub.ShareBiblio == false)
                    this.ToolStripMenuItem_searchShareBiblio.Text = "使用共享网络 [暂时被禁用]";
                else
                    this.ToolStripMenuItem_searchShareBiblio.Text = "使用共享网络";
            }
        }

        // 前移或后移 Selection Item
        public static ListViewItem MoveSelectedItem(
            ListView list,
            string strStyle)
        {
            if (list.Items.Count == 0)
                return null;
            ListViewItem item = null;
            if (list.SelectedItems.Count == 0)
            {
                item = list.Items[0];
                item.Selected = true;
                return item;
            }

            item = list.SelectedItems[0];
            if (list.SelectedItems.Count > 1)
                ListViewUtil.SelectLine(item, true);

            bool bRet = ListViewUtil.MoveSelectedUpDown(
                list,
                strStyle == "prev" ? true : false);
            if (bRet == false)
                return null;
            return list.SelectedItems[0];
        }

        #region 停靠

        List<Control> _freeControls = new List<Control>();

        public bool Docked { get; set; }

        void menu_toggleDock_Click(object sender, EventArgs e)
        {
            if (this.Docked == false)
                DoDock(true);
            else
                UnDock();
        }

        /// <summary>
        /// 进行停靠
        /// </summary>
        /// <param name="bShowFixedPanel">是否同时促成显示固定面板</param>
        public void DoDock(bool bShowFixedPanel)
        {
            // 已有 Dock 的 BiblioSearchForm
            if (Program.MainForm.CurrentBrowseControl != null)
            {
                BiblioSearchForm exist = Program.MainForm.GetOwnerBiblioSearchForm(Program.MainForm.CurrentBrowseControl);
                if (exist == this)
                    return;
                if (exist != null)
                    exist.UnDock();
            }

            if (Program.MainForm.CurrentBrowseControl != this.listView_records)
            {
                Program.MainForm.CurrentBrowseControl = this.listView_records;
                // 防止内存泄漏
                ControlExtention.AddFreeControl(_freeControls, this.listView_records);
            }

            if (bShowFixedPanel == true
                && Program.MainForm.PanelFixedVisible == false)
                Program.MainForm.PanelFixedVisible = true;

            Program.MainForm.ActivateFixPage("browse");

            this.Docked = true;
            this.Visible = false;
            // this..MainForm = Program.MainForm;   //
            this.MdiParent = null;
            Debug.Assert(Program.MainForm != null, "");

            Program.MainForm._dockedBiblioSearchForm = this;
        }

        public void UnDock()
        {
            Program.MainForm.CurrentBrowseControl = null;
            // 防止内存泄漏
            ControlExtention.RemoveFreeControl(_freeControls, this.listView_records);
            //this.Controls.Add(this.listView_records);
            this.Docked = false;
            this.MdiParent = Program.MainForm;
            this.Visible = true;
            Debug.Assert(Program.MainForm != null, "");

            this.splitContainer_main.Panel2.Controls.Add(this.listView_records);
            //this.listView_records.ResumeLayout(false);
            //this.listView_records.PerformLayout();
            Program.MainForm._dockedBiblioSearchForm = null;
        }

        #endregion

        private void BiblioSearchForm_VisibleChanged(object sender, EventArgs e)
        {
            int i = 0;
            i++;
        }

        // 200
        // 200$a
        // 200$a$(1)
        // 200$a$(1,2)
        // (1,2)
        // 200$(1,2)

        static void ParsePosition(string text,
            out string field_name,
            out string subfield_name,
            out string char_range)
        {
            field_name = "";
            subfield_name = "";
            char_range = "";
            var parameters = text.Split(new char[] { '$' });
            foreach (string s in parameters)
            {
                if (s.StartsWith("("))
                {
                    if (string.IsNullOrEmpty(char_range) == false)
                        throw new Exception($"'{text}' 格式错误，圆括号部分('{s}')重复出现");
                    char_range = StringUtil.Unquote(s, "()");
                }
                else if (s.Length == 1)
                {
                    if (string.IsNullOrEmpty(subfield_name) == false)
                        throw new Exception($"'{text}' 格式错误，子字段名部分('{s}')重复出现");

                    subfield_name = s;
                }
                else if (s.Length == 3)
                {
                    if (string.IsNullOrEmpty(field_name) == false)
                        throw new Exception($"'{text}' 格式错误，字段名部分('{s}')重复出现");

                    field_name = s;
                }
                else
                    throw new Exception($"'{s}' 格式错误，应为 3 字符或 1 字符或圆括号括住的文字");
            }
        }

        static string BuildContains(string words, string char_range)
        {
            string content = "@content";
            if (string.IsNullOrEmpty(char_range) == false)
            {
                var ranges = char_range.Split(new char[] { ',' });
                if (ranges.Length == 1)
                {
                    content = $"substring(@content, {ranges[0]})";
                }
                else if (ranges.Length == 2)
                {
                    content = $"substring(@content, {ranges[0]}, {ranges[1]})";
                }
                else
                    throw new Exception($"'{char_range}' 不合法。应该是 '-' 间隔的最多两个部分");
            }

            List<string> results = new List<string>();
            var parameters = words.Split(new char[] { '|' });
            foreach (var word in parameters)
            {
                results.Add($"contains({content}, '{word}')");
            }
            return "(" + StringUtil.MakePathList(results, " or ") + ")";
        }

        static string BuildNames(string words, string positions)
        {
            List<string> results = new List<string>();
            var parameters = positions.Split(new char[] { '|' });
            foreach (var position in parameters)
            {
                ParsePosition(position,
                    out string field_name,
                    out string subfield_name,
                    out string char_range);
                if (string.IsNullOrEmpty(field_name) == false
                    && string.IsNullOrEmpty(subfield_name) == true)
                {
                    // *** 字段名不为空，子字段名为空
                    var contains = BuildContains(words, char_range);
                    results.Add($"field[@name='{position}' and {contains}]");
                    results.Add($"field[@name='{position}']/subfield[{contains}]");
                }
                else if (string.IsNullOrEmpty(field_name) == true
    && string.IsNullOrEmpty(subfield_name) == false)
                {
                    // *** 字段名为空，子字段名不为空
                    var contains = BuildContains(words, char_range);
                    results.Add($"field/subfield[@name='{subfield_name}' and {contains}]");
                }
                else if (string.IsNullOrEmpty(field_name) == true
    && string.IsNullOrEmpty(subfield_name) == true)
                {
                    // *** 字段名为空，子字段名为空
                    var contains = BuildContains(words, char_range);
                    results.Add($"field[{contains}]");
                    results.Add($"field/subfield[{contains}]");
                }
                else if (string.IsNullOrEmpty(field_name) == false
                    && string.IsNullOrEmpty(subfield_name) == false)
                {
                    // *** 字段名和子字段名都不为空
                    var contains = BuildContains(words, char_range);
                    // 子字段内容中包含一个指定的子串
                    results.Add($"field[@name='{field_name}']/subfield[@name='{subfield_name}' and {contains}]");
                }
                else
                    throw new Exception($"参数2 '{position}' 格式不正确，出现了意外的情况(field_name='{field_name}', subfield_name='{subfield_name}', char_range='{char_range}')");
            }
            return StringUtil.MakePathList(results, " | ");
        }

        // xpath:field[(@name='200' or @name='300') and (contains(@content, '中学')  or  contains(@content, '大水') )  ]
        // | field[@name='200']/subfield[(@name='a' or @name='b') and contains(@content, '中学')]
        public static string BuildXPath(params string[] parameters)
        {
            if (parameters.Length == 1)
            {
                // 字段内容中包含一个指定的子串
                // (contains(@content, '中学') or contains(@content, '大水') )
                return $"field[{BuildContains(parameters[0], null)}]";
            }
            else if (parameters.Length == 2)
            {
                string word = parameters[0];
                string position = parameters[1];
                return BuildNames(word, position);
            }
            else
                throw new Exception($"不支持参数数量 {parameters.Length}");
        }

#if OLD
        // 根据输入到 textbox 中的用户态命令，构造 XPath
        public static string BuildXPath(params string[] parameters)
        {
            if (parameters.Length == 1)
            {
                // 字段内容中包含一个指定的子串
                return $"field[contains(@content, '{parameters[0]}')]";
            }
            else if (parameters.Length == 2)
            {
                string word = parameters[0];
                string position = parameters[1];
                if (position.Length == 3)
                {
                    // 字段内容中包含一个指定的子串
                    return $"field[@name='{position}' and contains(@content, '{parameters[0]}')]";
                }
                else if (position.Length == 5)
                {
                    // 子字段内容中包含一个指定的子串
                    string field_name = position.Substring(0, 3);
                    string subfield_name = position[4].ToString();
                    return $"field[@name='{field_name}']/subfield[@name='{subfield_name}' and contains(@content, '{parameters[0]}')]";
                }
                else
                    throw new Exception($"参数2 '{position}' 格式不正确，应为 3 或 5 字符");
            }
            else
                throw new Exception($"不支持参数数量 {parameters.Length}");
        }
#endif
        // 筛选
        private void ToolStripMenuItem_filterRecords_Click(object sender, EventArgs e)
        {
            string strError = "";

            try
            {
                int nBiblioCount = 0;
                string query_string = this.textBox_queryWord.Text;
                string xpath = "";
                if (query_string.StartsWith("xpath:"))
                {
                    xpath = query_string.Substring("xpath:".Length);
                }
                else
                {
                    var parameters = query_string.Split(new char[] { ' ' });
                    xpath = BuildXPath(parameters);

                    // 按住 Ctrl 键使用这个功能，会先显示实际用到的 XPath 式子
                    var control = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                    if (control)
                        MessageDlg.Show(this, $"xpath:{xpath}", "筛选");
                }

                var items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.Items)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }

                if (items.Count == 0)
                {
                    strError = "没有需要筛选的事项";
                    goto ERROR1;
                }

                ListViewUtil.ClearSelection(this.listView_records);

                int nRet = ProcessBiblio(
                    "正在筛选书目记录 ...",
                    items,
                    (strBiblioRecPath, biblio_dom, biblio_timestamp, item) =>
                    {
                        this.ShowMessage("正在过滤书目记录 " + strBiblioRecPath);

                        // 将XML格式转换为MARC格式
                        // 自动从数据记录中获得MARC语法
                        nRet = MarcUtil.Xml2Marc(biblio_dom.OuterXml,
                                true,
                                null,
                                out string strMarcSyntax,
                                out string strMARC,
                                out string strError1);
                        if (nRet == -1)
                            return true;

                        MarcRecord record = new MarcRecord(strMARC);

                        // field/subfield[contains(@content, '111')]
                        // field[@name='200']/subfield[@name='a' and contains(@content, '111')]
                        MarcNodeList nodes = record.select(xpath);
                        if (nodes.count > 0)
                            item.Selected = true;
                        else
                            item.Selected = false;

                        nBiblioCount++;
                        return true;
                    }, out strError);

                this.ClearMessage();

                if (nRet == -1)
                    goto ERROR1;
                return;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

        ERROR1:
            {
                this.ShowMessage(strError, "red", true);
            }
        }

        private void toolStripMenuItem_searchZ3950_Click(object sender, EventArgs e)
        {
            if (this.SearchZ3950 == false)
            {
                // TODO: 检查当前是否配置了 Z39.50 服务器列表。如果没有配置，提醒进行配置

                this.SearchZ3950 = true;
            }
            else
                this.SearchZ3950 = false;

            this.UpdateZ3950Menu();
        }

        void UpdateZ3950Menu()
        {
            this.toolStripMenuItem_searchZ3950.Checked = this.SearchZ3950;
        }

        private void ToolStripMenuItem_z3950ServerList_Click(object sender, EventArgs e)
        {
            using (ZServerListDialog dlg = new ZServerListDialog())
            {
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.XmlFileName = Path.Combine(Program.MainForm.UserDir, "zserver.xml");
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog(this);
            }
        }

        private void toolStripButton_multiLine_CheckedChanged(object sender, EventArgs e)
        {
            bool multiline = this.toolStripButton_multiLine.Checked;
            if (this.textBox_queryWord.Multiline != multiline)
            {
                int old_height = this.textBox_queryWord.Height;
                this.textBox_queryWord.Multiline = multiline;
                if (multiline)
                {
                    this.textBox_queryWord.Height = old_height * 3;
                    this.textBox_queryWord.AcceptsReturn = true;
                    this.textBox_queryWord.ScrollBars = ScrollBars.Vertical;
                }
                else
                {
                    this.textBox_queryWord.Height = old_height / 3;
                    this.textBox_queryWord.AcceptsReturn = false;
                    this.textBox_queryWord.ScrollBars = ScrollBars.None;

                    // 2021/3/9
                    // 如果原先内容是多行，只保留第一行
                    int index = this.textBox_queryWord.Text.IndexOfAny(new char[] { '\r', '\n' });
                    if (index != -1)
                        this.textBox_queryWord.Text = this.textBox_queryWord.Text.Substring(0, index);
                }
            }
        }

        private void toolStripMenuItem_subrecords_CheckedChanged(object sender, EventArgs e)
        {
            DisplaySubrecords = toolStripMenuItem_subrecords.Checked;
        }

        private void toolStripMenuItem_findInList_Click(object sender, EventArgs e)
        {
            ListViewUtil.FindAndSelect(this.listView_records, this.textBox_queryWord.Text);
        }

        // 命中结果返回前(在 dp2library 一端)是否按照 ID 降序排序
        bool _idDesc = false;

        private void toolStripMenuItem_idOrder_Click(object sender, EventArgs e)
        {
            _idDesc = !_idDesc;
            this.toolStripMenuItem_idOrder.Text = _idDesc ? "降序" : "升序";
        }

        // Dock 停靠以后，this.Visible == true，只能用 listView_records
        void TryInvoke(Action method)
        {
            this.listView_records.TryInvoke(method);
        }

        T TryGet<T>(Func<T> func)
        {
            return this.listView_records.TryGet(func);
        }
    }

    // 为一行存储的书目信息
    /// <summary>
    /// 在内存中缓存一条书目信息。能够表示新旧记录的修改关系
    /// </summary>
    public class BiblioInfo
    {
        /// <summary>
        /// 记录路径
        /// </summary>
        public string RecPath = "";
        /// <summary>
        /// 旧的记录 XML
        /// </summary>
        public string OldXml = "";
        /// <summary>
        /// 新的记录 XML
        /// </summary>
        public string NewXml = "";

        // 2015/8/12
        /// <summary>
        /// 数据格式
        /// </summary>
        public string Format = "";

        /// <summary>
        /// 时间戳
        /// </summary>
        public byte[] Timestamp = null;

        // 2021/2/4
        public string Subrecords = "";

        public BiblioInfo()
        {
        }

        public BiblioInfo(string strRecPath,
            string strOldXml,
            string strNewXml,
            byte[] timestamp)
        {
            this.RecPath = strRecPath;
            this.OldXml = strOldXml;
            this.NewXml = strNewXml;
            this.Timestamp = timestamp;
        }

        // 拷贝构造
        public BiblioInfo(BiblioInfo ref_obj)
        {
            this.RecPath = ref_obj.RecPath;
            this.OldXml = ref_obj.OldXml;
            this.NewXml = ref_obj.NewXml;
            this.Timestamp = ref_obj.Timestamp;
        }

        public bool Changed
        {
            get
            {
                if (string.IsNullOrEmpty(this.NewXml) == false)
                    return true;
                return false;
            }
        }

        // 获得问号前的部分。例如，针对 "读者/?1" 返回 "读者/?"
        public static string GetPath(string path)
        {
            if (path == null)
                return null;
            int index = path.IndexOf("?");
            if (index == -1)
                return path;
            return path.Substring(0, index + 1);
        }

    }

    /// <summary>
    /// 一个 loader 事项
    /// </summary>
    public class LoaderItem
    {
        /// <summary>
        /// 书目信息
        /// </summary>
        public BiblioInfo BiblioInfo = null;
        /// <summary>
        /// 关联的 ListViewItem 对象
        /// </summary>
        public ListViewItem ListViewItem = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info">书目信息</param>
        /// <param name="item">关联的 ListViewItem 对象</param>
        public LoaderItem(BiblioInfo info, ListViewItem item)
        {
            this.BiblioInfo = info;
            this.ListViewItem = item;
        }
    }

    /// <summary>
    /// 根据 ListViewItem 数组获得书目记录信息的枚举器。
    /// 可以利用缓存机制
    /// </summary>
    public class ListViewBiblioLoader : IEnumerable
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

        /// <summary>
        /// ListViewItem 集合
        /// </summary>
        public List<ListViewItem> Items
        {
            get;
            set;
        }

        /// <summary>
        /// 用于缓存的 Hashtable 对象
        /// </summary>
        public Hashtable CacheTable
        {
            get;
            set;
        }

        string _format = "xml";
        public string Format
        {
            get
            {
                return _format;
            }
            set
            {
                _format = value;
            }
        }

        BiblioLoader m_loader = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channel">通讯通道</param>
        /// <param name="stop">停止对象</param>
        /// <param name="items">ListViewItem 集合</param>
        /// <param name="cacheTable">用于缓存的 Hashtable 对象</param>
        public ListViewBiblioLoader(LibraryChannel channel,
            Stop stop,
            List<ListViewItem> items,
            Hashtable cacheTable)
        {
            m_loader = new BiblioLoader();
            m_loader.Channel = channel;
            m_loader.Stop = stop;
            m_loader.Format = _format;  // "xml";
            m_loader.GetBiblioInfoStyle = GetBiblioInfoStyle.Timestamp; // 附加信息只取得 timestamp
            m_loader.Prompt += new MessagePromptEventHandler(m_loader_Prompt);

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

            Hashtable dup_table = new Hashtable();  // 确保 recpaths 中不会出现重复的路径

            List<string> recpaths = new List<string>(); // 缓存中没有包含的那些记录
            foreach (ListViewItem item in this.Items)
            {
                string strRecPath = item.Text;
                Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");

                if (dup_table.ContainsKey(strRecPath) == true)
                    continue;
                BiblioInfo info = null;
                if (this.CacheTable != null)
                    info = (BiblioInfo)this.CacheTable[strRecPath];
                if (info == null || string.IsNullOrEmpty(info.OldXml) == true)
                {
                    recpaths.Add(strRecPath);
                    dup_table[strRecPath] = true;
                }
            }

            // 注： Hashtable 在这一段时间内不应该被修改。否则会破坏 m_loader 和 items 之间的锁定对应关系

            m_loader.RecPaths = recpaths;

            var enumerator = m_loader.GetEnumerator();

            // 开始循环
            foreach (ListViewItem item in this.Items)
            {
                string strRecPath = item.Text;
                Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");

                BiblioInfo info = null;
                if (this.CacheTable != null)
                    info = (BiblioInfo)this.CacheTable[strRecPath];
                if (info == null || string.IsNullOrEmpty(info.OldXml) == true)
                {
                    if (m_loader.Stop != null)
                    {
                        m_loader.Stop.SetMessage("正在获取书目记录 " + strRecPath);
                    }
                    bool bRet = enumerator.MoveNext();
                    if (bRet == false)
                    {
                        Debug.Assert(false, "还没有到结尾, MoveNext() 不应该返回 false");
                        // TODO: 这时候也可以采用返回一个带没有找到的错误码的元素
                        yield break;
                    }

                    BiblioItem biblio = (BiblioItem)enumerator.Current;
                    Debug.Assert(biblio.RecPath == strRecPath, "m_loader 和 items 的元素之间 记录路径存在严格的锁定对应关系");

                    // 需要放入缓存
                    if (info == null)
                    {
                        info = new BiblioInfo();
                        info.RecPath = biblio.RecPath;
                    }
                    info.OldXml = biblio.Content;
                    info.Timestamp = biblio.Timestamp;
                    if (this.CacheTable != null)
                        this.CacheTable[strRecPath] = info;
                    yield return new LoaderItem(info, item);
                }
                else
                    yield return new LoaderItem(info, item);
            }
        }
    }
}