using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Marc;

using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.IO;
using DigitalPlatform.Script;
using System.Web;
using DigitalPlatform.Text;
using DigitalPlatform.MessageClient;
using System.Threading;

namespace dp2Circulation
{
    /// <summary>
    /// 书目查询窗
    /// </summary>
    public partial class BiblioSearchForm : MyForm
    {
        Commander commander = null;

        CommentViewerForm m_commentViewer = null;

        Hashtable m_biblioTable = new Hashtable(); // 书目记录路径 --> 书目信息

        const int WM_SELECT_INDEX_CHANGED = API.WM_USER + 200;

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
            InitializeComponent();

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

            // e.ColumnTitles = this.MainForm.GetBrowseColumnNames(e.DbName);
            if (e.DbName.IndexOf("@") == -1)
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                ColumnPropertyCollection temp = this.MainForm.GetBrowseColumnProperties(e.DbName);
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
                foreach(string s in titles)
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

        private void BiblioSearchForm_Load(object sender, EventArgs e)
        {
            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            this.MainForm.AppInfo.LoadMdiSize += new EventHandler(AppInfo_LoadMdiSize);
            this.MainForm.AppInfo.SaveMdiSize += new EventHandler(AppInfo_SaveMdiSize);

            this.MainForm.FillBiblioFromList(this.comboBox_from);

            this.m_strUsedMarcQueryFilename = this.MainForm.AppInfo.GetString(
                "bibliosearchform",
                "usedMarcQueryFilename",
                "");

            // 恢复上次退出时保留的检索途径
            string strFrom = this.MainForm.AppInfo.GetString(
                "bibliosearchform",
                "search_from",
                "");
            if (String.IsNullOrEmpty(strFrom) == false)
                this.comboBox_from.Text = strFrom;

            this.checkedComboBox_biblioDbNames.Text = this.MainForm.AppInfo.GetString(
                "bibliosearchform",
                "biblio_db_name",
                "<全部>");

            this.comboBox_matchStyle.Text = this.MainForm.AppInfo.GetString(
                "bibliosearchform",
                "match_style",
                "前方一致");

            bool bHideMatchStyle = this.MainForm.AppInfo.GetBoolean(
                "biblio_search_form",
                "hide_matchstyle",
                false);

            if (bHideMatchStyle == true)
            {
                this.label_matchStyle.Visible = false;
                this.comboBox_matchStyle.Visible = false;
                this.comboBox_matchStyle.Text = "前方一致"; // 隐藏后，采用缺省值
            }

            string strSaveString = this.MainForm.AppInfo.GetString(
"bibliosearchform",
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
            if (this.MainForm != null)
                this.MainForm.FixedSelectedPageChanged += new EventHandler(MainForm_FixedSelectedPageChanged);

            UpdateMenu();

#if NO
            if (this.MainForm.NormalDbProperties == null
                || this.MainForm.BiblioDbFromInfos == null
                || this.MainForm.BiblioDbProperties == null)
            {
                this.tableLayoutPanel_main.Enabled = false;
            }
#endif
        }

        void MainForm_FixedSelectedPageChanged(object sender, EventArgs e)
        {
            // 固定面板属性区域被显示出来后
            if (this.MainForm.ActiveMdiChild == this && this.MainForm.CanDisplayItemProperty() == true)
            {
                RefreshPropertyView(false);
            }
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_nInViewing > 0;
        }

        void AppInfo_SaveMdiSize(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_records);
            this.MainForm.AppInfo.SetString(
                "bibliosearchform",
                "record_list_column_width",
                strWidths);

            this.MainForm.SaveSplitterPos(
this.splitContainer_main,
"bibliosearchform",
"splitContainer_main_ratio");
        }

        void AppInfo_LoadMdiSize(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            string strWidths = this.MainForm.AppInfo.GetString(
    "bibliosearchform",
    "record_list_column_width",
    "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_records,
                    strWidths,
                    true);
            }


            this.MainForm.LoadSplitterPos(
this.splitContainer_main,
"bibliosearchform",
"splitContainer_main_ratio");
        }

        private void BiblioSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }

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
            this.commander.Destroy();

            this.MainForm.AppInfo.SetString(
    "bibliosearchform",
    "usedMarcQueryFilename",
    this.m_strUsedMarcQueryFilename);

            // 保存检索途径
            this.MainForm.AppInfo.SetString(
                "bibliosearchform",
                "search_from",
                this.comboBox_from.Text);

            this.MainForm.AppInfo.SetString(
                "bibliosearchform",
                "biblio_db_name",
                this.checkedComboBox_biblioDbNames.Text);

            this.MainForm.AppInfo.SetString(
                "bibliosearchform",
                "match_style",
                this.comboBox_matchStyle.Text);



            this.MainForm.AppInfo.SetString(
"bibliosearchform",
"query_lines",
this.dp2QueryControl1.GetSaveString());

            if (this.m_commentViewer != null)
                this.m_commentViewer.Close();

            this.MainForm.AppInfo.LoadMdiSize -= new EventHandler(AppInfo_LoadMdiSize);
            this.MainForm.AppInfo.SaveMdiSize -= new EventHandler(AppInfo_SaveMdiSize);

            this.MainForm.FixedSelectedPageChanged -= new EventHandler(MainForm_FixedSelectedPageChanged);
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
        public int MaxSearchResultCount
        {
            get
            {
                return (int)this.MainForm.AppInfo.GetInt(
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
        public bool PushFillingBrowse
        {
            get
            {
                return MainForm.AppInfo.GetBoolean(
                    "biblio_search_form",
                    "push_filling_browse",
                    false);
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

        void ClearListViewItems()
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
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
Keys keyData)
        {
            if (keyData == Keys.Enter
                && this.tabControl_query.SelectedTab == this.tabPage_logic)
            {
                this.DoLogicSearch(false);
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

        void DoLogicSearch(bool bOutputKeyID)
        {
            string strError = "";
            bool bQuickLoad = false;    // 是否快速装入
            bool bClear = true; // 是否清除浏览窗中已有的内容

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                bQuickLoad = true;

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                bClear = false;


            // 修改窗口标题
            this.Text = "书目查询 逻辑检索";

            if (bOutputKeyID == true)
                this.m_bFirstColumnIsKey = true;
            else
                this.m_bFirstColumnIsKey = false;
            this.ClearListViewPropertyCache();

            if (bClear == true)
            {
                if (this.m_nChangedCount > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "当前命中记录列表中有 " + this.m_nChangedCount.ToString() + " 项修改尚未保存。\r\n\r\n是否继续操作?\r\n\r\n(Yes 清除，然后继续操作；No 放弃操作)",
                        "BiblioSearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                }
                this.ClearListViewItems();

                ListViewUtil.ClearSortColumns(this.listView_records);
            }

            this.m_lHitCount = 0;
            this.m_lLoaded = 0;
            stop.HideProgress();

            this.label_message.Text = "";

            stop.Style = StopStyle.None;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

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
                int nRet = dp2QueryControl1.BuildQueryXml(
    this.MaxSearchResultCount,
    "zh",
    out strQueryXml,
    out strError);
                if (nRet == -1)
                    goto ERROR1;

                long lRet = Channel.Search(stop,
                    strQueryXml,
                    "default",
                    strOutputStyle,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                this.m_lHitCount = lHitCount;

                this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录";

                stop.SetProgressRange(0, lHitCount);
                stop.Style = StopStyle.EnableHalfStop;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                bool bPushFillingBrowse = this.PushFillingBrowse;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            // MessageBox.Show(this, "用户中断");
                            this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已装入 " + lStart.ToString() + " 条，用户中断...";
                            return;
                        }
                    }

                    stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

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

                    lRet = Channel.GetSearchResult(
                        stop,
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
                        DigitalPlatform.CirculationClient.localhost.Record searchresult = searchresults[i];

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
                    stop.SetProgressValue(lStart);
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
                this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已全部装入";
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;


                this.EnableControlsInSearching(true);
            }

            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        List<ItemQueryParam> m_queries = new List<ItemQueryParam>();
        int m_nQueryIndex = -1;

        void QueryToPanel(ItemQueryParam query)
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
        }

        ItemQueryParam PanelToQuery()
        {
            ItemQueryParam query = new ItemQueryParam();

            query.QueryWord = this.textBox_queryWord.Text;
            query.DbNames = this.checkedComboBox_biblioDbNames.Text;
            query.From = this.comboBox_from.Text;
            query.MatchStyle = this.comboBox_matchStyle.Text;
            query.FirstColumnIsKey = this.m_bFirstColumnIsKey;
            return query;
        }

        void PushQuery(ItemQueryParam query)
        {
            if (query == null)
                throw new Exception("query值不能为空");

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
        }

        ItemQueryParam PrevQuery()
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


        public void DoSearch(bool bOutputKeyCount,
            bool bOutputKeyID,
            ItemQueryParam input_query = null)
        {
            string strError = "";
            int nRet = 0;
            bool bDisplayClickableError = false;

            if (bOutputKeyCount == true
    && bOutputKeyID == true)
            {
                strError = "bOutputKeyCount和bOutputKeyID不能同时为true";
                goto ERROR1;
            }

            bool bQuickLoad = false;    // 是否快速装入
            bool bClear = true; // 是否清除浏览窗中已有的内容

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                bQuickLoad = true;

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                bClear = false;

            // 修改窗口标题
            this.Text = "书目查询 " + this.textBox_queryWord.Text;

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
            stop.HideProgress();

            this.label_message.Text = "";

            stop.Style = StopStyle.None;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 '" + this.textBox_queryWord.Text + "' ...");
            stop.BeginLoop();

            this.ShowMessage("正在检索 '" + this.textBox_queryWord.Text + "' ...");

            this.EnableControlsInSearching(false);
            try
            {
                if (this.comboBox_from.Text == "")
                {
                    strError = "尚未选定检索途径";
                    goto ERROR1;
                }

                string strFromStyle = "";

                try
                {
                    strFromStyle = this.MainForm.GetBiblioFromStyle(this.comboBox_from.Text);
                }
                catch (Exception ex)
                {
                    strError = "BiblioSearchForm GetBiblioFromStyle() exception: " + ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strError = "GetFromStyle()没有找到 '" + this.comboBox_from.Text + "' 对应的style字符串";
                    goto ERROR1;
                }

                // 注："null"只能在前端短暂存在，而内核是不认这个所谓的matchstyle的
                string strMatchStyle = GetCurrentMatchStyle(this.comboBox_matchStyle.Text);

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

                bool bNeedShareSearch = false;
                if (this.SearchShareBiblio == true
    && this.MainForm != null && this.MainForm.MessageHub != null
    && this.MainForm.MessageHub.ShareBiblio == true
                    && bOutputKeyCount == false
                    && bOutputKeyID == false)
                {
                    bNeedShareSearch = true;
                }

                if (bNeedShareSearch == true)
                {
                    // 开始检索共享书目
                    // return:
                    //      -1  出错
                    //      0   没有检索目标
                    //      1   成功启动检索
                    nRet = BeginSearchShareBiblio(
                        this.textBox_queryWord.Text,
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

                string strQueryXml = "";
                long lRet = Channel.SearchBiblio(stop,
                    this.checkedComboBox_biblioDbNames.Text,
                    this.textBox_queryWord.Text,
                    this.MaxSearchResultCount,  // 1000
                    strFromStyle,
                    strMatchStyle,  // "left",
                    this.Lang,
                    null,   // strResultSetName
                    "",    // strSearchStyle
                    strOutputStyle,
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                this.m_lHitCount = lHitCount;

                this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录";

                stop.SetProgressRange(0, lHitCount);
                stop.Style = StopStyle.EnableHalfStop;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                bool bPushFillingBrowse = this.PushFillingBrowse;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        // MessageBox.Show(this, "用户中断");
                        this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已装入 " + lStart.ToString() + " 条，用户中断...";
                        return;
                    }

                    stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

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

                    lRet = Channel.GetSearchResult(
                        stop,
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
                        break;

                    if (lRet > 0)
                    {
                        this.listView_records.BeginUpdate();
                        try
                        {
                            // 处理浏览结果
                            for (int i = 0; i < searchresults.Length; i++)
                            {
                                ListViewItem item = null;

                                DigitalPlatform.CirculationClient.localhost.Record searchresult = searchresults[i];

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
                            }
                        }
                        finally
                        {
                            this.listView_records.EndUpdate();
                        }

                        lStart += searchresults.Length;
                        // lCount -= searchresults.Length;
                        if (lStart >= lHitCount || lPerCount <= 0)
                            break;

                        this.m_lLoaded = lStart;
                        stop.SetProgressValue(lStart);
                    }
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);

                if (bNeedShareSearch == true)
                {
                    this.ShowMessage("等待共享检索响应 ...");
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
                            this.ShowMessage("共享书目命中 "+_searchParam._searchCount+" 条", "green");
                            this._floatingMessage.DelayClear(new TimeSpan(0,0,3));
#if NO
                            Application.DoEvents();
                            // TODO: 延时一段自动删除
                            Thread.Sleep(1000);
#endif
                        }
                    }

                    lHitCount += _searchParam._searchCount;
                }

                if (lHitCount == 0)
                {
                    this.ShowMessage("未命中", "yellow", true);
                    bDisplayClickableError = true;
                }

                if (lHitCount == 0)
                    this.label_message.Text = "未命中";
                else
                    this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已全部装入";

            }
            finally
            {
                if (bDisplayClickableError == false
                    && this._floatingMessage.InDelay() == false)
                    this.ClearMessage();
                if (this.MainForm.MessageHub != null)
                    this.MainForm.MessageHub.SearchResponseEvent -= MessageHub_SearchResponseEvent;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControlsInSearching(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
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

            if (this.MainForm.MessageHub == null)
            {
                strError = "MessageHub is null";
                return -1;
            }

            string strSearchID = Guid.NewGuid().ToString();
            _searchParam = new SearchParam();
            _searchParam._searchID = strSearchID;
            _searchParam._searchComplete = false;
            _searchParam._searchCount = 0;
            this.MainForm.MessageHub.SearchResponseEvent += MessageHub_SearchResponseEvent;

            string strOutputSearchID = "";
            int nRet = this.MainForm.MessageHub.BeginSearchBiblio(
                strSearchID,
                "<全部>",
strQueryWord,
strFromStyle,
strMatchStyle,
"",
1000,
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
                    Application.DoEvents();
                    Thread.Sleep(200);
                    if (DateTime.Now - start_time > timeout)    // 超时
                        break;
                    if (this.Progress != null && this.Progress.State != 0)
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
            }
        }

        class SearchParam
        {
            public string _searchID = "";
            // public bool _autoSetFocus = false;
            public bool _searchComplete = false;
            public int _searchCount = 0;
        }

        SearchParam _searchParam = null;

        // 外来数据的浏览列标题的对照表。MARC 格式名 --> 列标题字符串
        Hashtable _browseTitleTable = new Hashtable();

        void MessageHub_SearchResponseEvent(object sender, SearchResponseEventArgs e)
        {
            if (e.SsearchID != _searchParam._searchID)
                return;
            if (e.ResultCount == -1 && e.Start == -1)
            {
                // 检索过程结束
                _searchParam._searchComplete = true;
                return;
            }
            string strError = "";

            if (e.ResultCount == -1)
            {
                strError = e.ErrorInfo;
                goto ERROR1;
            }

            // TODO: 注意来自共享网络的图书馆名不能和 servers.xml 中的名字冲突。另外需要检查，不同的 UID，图书馆名字不能相同，如果发生冲突，则需要给分配 ..1 ..2 这样的编号以示区别
            // 需要一直保存一个 UID 到图书馆命的对照表在内存备用
            // TODO: 来自共享网络的记录，图标或 @ 后面的名字应该有明显的形态区别
            foreach (BiblioRecord record in e.Records)
            {
                string strXml = record.Data;

                string strMarcSyntax = "";
                string strBrowseText = "";
                string strColumnTitles = "";
                int nRet = BuildBrowseText(strXml,
out strBrowseText,
out strMarcSyntax,
out strColumnTitles,
out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strRecPath = record.RecPath + "@" + (string.IsNullOrEmpty(record.LibraryName) == false ? record.LibraryName : record.LibraryUID);

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
                    this.m_biblioTable[strRecPath] = info;
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
                _searchParam._searchCount++;
            }

            return;
        ERROR1:
            // 加入一个文本行
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
        }

        private void listView_records_DoubleClick(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要装入种册窗的事项";
                goto ERROR1;
            }

            string strPath = this.listView_records.SelectedItems[0].SubItems[0].Text;

            if (String.IsNullOrEmpty(strPath) == false)
            {
                EntityForm form = null;

                if (this.LoadToExistDetailWindow == true)
                {
                    form = MainForm.GetTopChildWindow<EntityForm>();
                    if (form != null)
                        Global.Activate(form);

                }

                if (form == null)
                {
                    form = new EntityForm();

                    form.MdiParent = this.MainForm;

                    form.MainForm = this.MainForm;
                    form.Show();
                }

                Debug.Assert(form != null, "");

                if (strPath.IndexOf("@") == -1)
                    form.LoadRecordOld(strPath, "", true);
                else
                {
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

                    int nRet = form.LoadRecord(info,
                        true,
                        out strError);
                    if (nRet != 1)
                        goto ERROR1;

                }

            }
            else
            {
                ItemQueryParam query = (ItemQueryParam)this.listView_records.SelectedItems[0].Tag;
                Debug.Assert(query != null, "");

                this.textBox_queryWord.Text = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);
                if (query != null)
                {
                    this.checkedComboBox_biblioDbNames.Text = query.DbNames;
                    this.comboBox_from.Text = query.From;
                }

                if (this.textBox_queryWord.Text == "")
                    this.comboBox_matchStyle.Text = "空值";
                else
                    this.comboBox_matchStyle.Text = "精确一致";

                DoSearch(false, false, null);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_loadToOpenedEntityForm_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装入实体窗口的事项");
                return;
            }
            string strPath = this.listView_records.SelectedItems[0].SubItems[0].Text;

            EntityForm form = null;

            form = MainForm.GetTopChildWindow<EntityForm>();
            if (form != null)
                Global.Activate(form);

            if (form == null)
            {
                form = new EntityForm();

                form.MdiParent = this.MainForm;
                form.MainForm = this.MainForm;
                form.Show();
            }

            Debug.Assert(form != null, "");

            form.LoadRecordOld(strPath, "", true);
        }

        void menu_loadToNewEntityForm_Click(object sender, EventArgs e)
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
                form = new EntityForm();

                form.MdiParent = this.MainForm;
                form.MainForm = this.MainForm;
                form.Show();
            }

            Debug.Assert(form != null, "");

            form.LoadRecordOld(strPath, "", true);
        }

        // 是否优先装入已经打开的详细窗?
        /// <summary>
        /// 是否优先装入已经打开的详细窗?
        /// </summary>
        public bool LoadToExistDetailWindow
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "all_search_form",
                    "load_to_exist_detailwindow",
                    true);
            }
        }

        // 包括listview
        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            EnableControlsInSearching(bEnable);
            this.listView_records.Enabled = bEnable;

            /*
            this.toolStrip_search.Enabled = bEnable;
            this.listView_records.Enabled = bEnable;

            this.comboBox_from.Enabled = bEnable;
            this.checkedComboBox_biblioDbNames.Enabled = bEnable;
            this.comboBox_matchStyle.Enabled = bEnable;

            if (this.comboBox_matchStyle.Text == "空值")
                this.textBox_queryWord.Enabled = false;
            else
                this.textBox_queryWord.Enabled = bEnable;
            */
        }

        bool InSearching
        {
            get
            {
                if (this.comboBox_from.Enabled == true)
                    return false;
                return true;
            }
        }

        // 注: listview除外
        void EnableControlsInSearching(bool bEnable)
        {
            // this.button_search.Enabled = bEnable;
            this.toolStrip_search.Enabled = bEnable;

            this.comboBox_from.Enabled = bEnable;
            this.checkedComboBox_biblioDbNames.Enabled = bEnable;
            this.comboBox_matchStyle.Enabled = bEnable;

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

        private void BiblioSearchForm_Activated(object sender, EventArgs e)
        {
            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;

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

            if (this.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty property = this.MainForm.BiblioDbProperties[i];

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

            this.checkedComboBox_biblioDbNames.Items.Add("<全部>");

            if (this.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty property = this.MainForm.BiblioDbProperties[i];
                    this.checkedComboBox_biblioDbNames.Items.Add(property.DbName);
                }
            }
        }

        // listview上的右鼠标键菜单
        private void listView_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSeletedItemCount = this.listView_records.SelectedItems.Count;
            string strFirstColumn = "";
            if (nSeletedItemCount > 0)
            {
                strFirstColumn = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);
            }

            menuItem = new MenuItem("装入已打开的种册窗(&L)");
            if (this.LoadToExistDetailWindow == true
                && this.MainForm.GetTopChildWindow<EntityForm>() != null)
                menuItem.DefaultItem = true;
            menuItem.Click += new System.EventHandler(this.menu_loadToOpenedEntityForm_Click);
            if (this.listView_records.SelectedItems.Count == 0
                || this.MainForm.GetTopChildWindow<EntityForm>() == null
                || String.IsNullOrEmpty(strFirstColumn) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入新开的种册窗(&L)");
            if (this.LoadToExistDetailWindow == false
                || this.MainForm.GetTopChildWindow<EntityForm>() == null)
                menuItem.DefaultItem = true;
            menuItem.Click += new System.EventHandler(this.menu_loadToNewEntityForm_Click);
            if (this.listView_records.SelectedItems.Count == 0
                || String.IsNullOrEmpty(strFirstColumn) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            if (String.IsNullOrEmpty(strFirstColumn) == true
    && nSeletedItemCount > 0)
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

            }


            // bool bLooping = (stop != null && stop.State == 0);    // 0 表示正在处理

            // 批处理
            // 正在检索的时候，不允许进行批处理操作。因为stop.BeginLoop()嵌套后的Min Max Value之间的保存恢复问题还没有解决
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

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("导出到 XML 文件 [" + this.listView_records.SelectedItems.Count.ToString() + " ] (&M)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToXmlFile_Click);
                if (this.listView_records.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }

            // 装入其它查询窗
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
            }

            // 标记空下级记录的事项
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

            menuItem = new MenuItem("刷新浏览行 [" + nSeletedItemCount.ToString() + "] (&R)");
            menuItem.Click += new System.EventHandler(this.menu_refreshSelectedItems_Click);
            if (nSeletedItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
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
                    strIssueDbName = this.MainForm.GetIssueDbName(strDbName);
            }

            List<string> recpaths = new List<string>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                string strRecPath = ListViewUtil.GetItemText(item, 0);
                if (string.IsNullOrEmpty(strRecPath) == false)
                    recpaths.Add(strRecPath);
            }

            PrintClaimForm form = new PrintClaimForm();
            form.MdiParent = this.MainForm;
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
        void menu_saveSelectedChangedRecords_Click(object sender, EventArgs e)
        {
            // TODO: 确实要?

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

            int nRet = SaveChangedRecords(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            strError = "处理完成。\r\n\r\n" + strError;
            MessageBox.Show(this, strError);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 保存全部修改事项
        void menu_saveAllChangedRecords_Click(object sender, EventArgs e)
        {
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项需要保存");
                return;
            }

            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            foreach(ListViewItem item in this.listView_records.Items)
            {
                items.Add(item);
            }

            int nRet = SaveChangedRecords(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            strError = "处理完成。\r\n\r\n" + strError;
            MessageBox.Show(this, strError);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int SaveChangedRecords(List<ListViewItem> items,
            out string strError)
        {
            strError = "";

            int nReloadCount = 0;
            int nSavedCount = 0;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存书目记录 ...");
            stop.BeginLoop();

            this.EnableControlsInSearching(false);
            this.listView_records.Enabled = false;
            try
            {
                stop.SetProgressRange(0, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    if (stop != null && stop.State != 0)
                    {
                        strError = "已中断";
                        return -1;
                    }

                    ListViewItem item = items[i];
                    string strRecPath = item.Text;
                    if (string.IsNullOrEmpty(strRecPath) == true)
                    {
                        stop.SetProgressValue(i);
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

                    string strOutputPath = "";

                    stop.SetMessage("正在保存书目记录 " + strRecPath);

                    byte[] baNewTimestamp = null;

                    long lRet = Channel.SetBiblioInfo(
                        stop,
                        "change",
                        strRecPath,
                        "xml",
                        info.NewXml,
                        info.Timestamp,
                        "",
                        out strOutputPath,
                        out baNewTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (Channel.ErrorCode == ErrorCode.TimestampMismatch)
                        {
                            DialogResult result = MessageBox.Show(this,
    "保存书目记录 "+strRecPath+" 时遭遇时间戳不匹配: " + strError + "。\r\n\r\n此记录已无法被保存。\r\n\r\n请问现在是否要顺便重新装载此记录? \r\n\r\n(Yes 重新装载；\r\nNo 不重新装载、但继续处理后面的记录保存; \r\nCancel 中断整批保存操作)",
    "BiblioSearchForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                                break;
                            if (result == System.Windows.Forms.DialogResult.No)
                                goto CONTINUE;

                            // 重新装载书目记录到 OldXml
                            string[] results = null;
                            // byte[] baTimestamp = null;
                            lRet = Channel.GetBiblioInfos(
                                stop,
                                strRecPath,
                                "",
                                new string[] { "xml" },   // formats
                                out results,
                                out baNewTimestamp,
                                out strError);
                            if (lRet == 0)
                            {
                                // TODO: 警告后，把 item 行移除？
                                return -1;
                            }
                            if (lRet == -1)
                                return -1;
                            if (results == null || results.Length == 0)
                            {
                                strError = "results error";
                                return -1;
                            }
                            info.OldXml = results[0];
                            info.Timestamp = baNewTimestamp;
                            nReloadCount++;
                            goto CONTINUE;
                        }

                        return -1;
                    }

                    // 检查是否有部分字段被拒绝
                    if (Channel.ErrorCode == ErrorCode.PartialDenied)
                    {
                        DialogResult result = MessageBox.Show(this,
"保存书目记录 " + strRecPath + " 时部分字段被拒绝。\r\n\r\n此记录已部分保存成功。\r\n\r\n请问现在是否要顺便重新装载此记录以便观察? \r\n\r\n(Yes 重新装载(到旧记录部分)；\r\nNo 不重新装载、但继续处理后面的记录保存; \r\nCancel 中断整批保存操作)",
"BiblioSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            break;
                        if (result == System.Windows.Forms.DialogResult.No)
                            goto CONTINUE;
                        // 重新装载书目记录到 OldXml
                        string[] results = null;
                        // byte[] baTimestamp = null;
                        lRet = Channel.GetBiblioInfos(
                            stop,
                            strRecPath,
                            "",
                            new string[] { "xml" },   // formats
                            out results,
                            out baNewTimestamp,
                            out strError);
                        if (lRet == 0)
                        {
                            // TODO: 警告后，把 item 行移除？
                            return -1;
                        }
                        if (lRet == -1)
                            return -1;
                        if (results == null || results.Length == 0)
                        {
                            strError = "results error";
                            return -1;
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

                    nSavedCount++;

                    this.m_nChangedCount--;
                    Debug.Assert(this.m_nChangedCount >= 0, "");

                CONTINUE:
                    stop.SetProgressValue(i);
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControlsInSearching(true);
                this.listView_records.Enabled = true;
            }

            // 2013/10/22
            int nRet = RefreshListViewLines(items,
    out strError);
            if (nRet == -1)
                return -1;

            RefreshPropertyView(false);

            strError = "";
            if (nSavedCount > 0)
                strError += "共保存书目记录 " + nSavedCount + " 条";
            if (nReloadCount > 0)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    strError += " ; ";
                strError += "有 " + nReloadCount + " 条书目记录因为时间戳不匹配或部分字段被拒绝而重新装载旧记录部分(请观察后重新保存)";
            }

            return 0;
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
            strError = "";

            if (items_param.Count == 0)
                return 0;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新浏览行 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                List<ListViewItem> items = new List<ListViewItem>();
                List<string> recpaths = new List<string>();
                foreach (ListViewItem item in items_param)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;
                    items.Add(item);
                    recpaths.Add(item.Text);

                    ClearOneChange(item, true);
                }

                if (stop != null)
                    stop.SetProgressRange(0, items.Count);

                BrowseLoader loader = new BrowseLoader();
                loader.Channel = Channel;
                loader.Stop = stop;
                loader.RecPaths = recpaths;
                loader.Format = "id,cols";

                int i = 0;
                foreach (DigitalPlatform.CirculationClient.localhost.Record record in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    Debug.Assert(record.Path == recpaths[i], "");

                    if (stop != null)
                    {
                        stop.SetMessage("正在刷新浏览行 " + record.Path + " ...");
                        stop.SetProgressValue(i);
                    }

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
                            c + 1,
                            record.Cols[c]);
                        }

                        // TODO: 是否清除余下的列内容?
                    }


                    i++;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "BiblioSearchForm RefreshListViewLines() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
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
        public void RefreshAllLines()
        {
            string strError = "";
            int nRet = 0;

            int nChangedCount = 0;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_records.Items)
            {
                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;
                items.Add(item);

                if (IsItemChanged(item) == true)
                    nChangedCount++;
            }

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

            nRet = RefreshListViewLines(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            RefreshPropertyView(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 刷新所选择的浏览行。也就是重新从数据库中装载浏览列
        void menu_refreshSelectedItems_Click(object sender, EventArgs e)
        {
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

            nRet = RefreshListViewLines(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            RefreshPropertyView(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 观察一个事项是否在内存中修改过
        bool IsItemChanged(ListViewItem item)
        {
            string strRecPath = item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return false;

            BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
                return false;

            if (string.IsNullOrEmpty(info.NewXml) == false)
                return true;

            return false;
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
                    nRet = RefreshOneBrowseLine(item,
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

        // 在一个新开的书目查询窗内检索key
        void listView_searchKeysAtNewWindow_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要操作的事项");
                return;
            }

            BiblioSearchForm form = new BiblioSearchForm();
            form.MdiParent = this.MainForm;
            // form.MainForm = this.MainForm;
            form.Show();

            ItemQueryParam query = (ItemQueryParam)this.listView_records.SelectedItems[0].Tag;
            Debug.Assert(query != null, "");

            ItemQueryParam input_query = new ItemQueryParam();

            input_query.QueryWord = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);
            input_query.DbNames = query.DbNames;
            input_query.From = query.From;
            input_query.MatchStyle = "精确一致";

            // 检索命中记录(而不是key)
            form.DoSearch(false, false, input_query);
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

        void menu_quickMarcQueryRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要执行 MarcQuery 脚本的事项";
                goto ERROR1;
            }

            // 书目信息缓存
            // 如果已经初始化，则保持
            if (this.m_biblioTable == null)
                this.m_biblioTable = new Hashtable();

            // this.m_biblioTable.Clear();

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定 MarcQuery 脚本文件";
            dlg.FileName = this.m_strUsedMarcQueryFilename;
            dlg.Filter = "MarcQuery 脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedMarcQueryFilename = dlg.FileName;

            MarcQueryHost host = null;
            Assembly assembly = null;

            nRet = PrepareMarcQuery(this.m_strUsedMarcQueryFilename,
                out assembly,
                out host,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            host.CodeFileName = this.m_strUsedMarcQueryFilename;
            {
                host.MainForm = this.MainForm;
                host.UiForm = this;
                host.RecordPath = "";
                host.MarcRecord = null;
                host.MarcSyntax = "";
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

            this.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行脚本 " + dlg.FileName + "</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在针对书目记录执行 MarcQuery 脚本 ...");
            stop.BeginLoop();

            this.EnableControls(false);

            this.listView_records.Enabled = false;
            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                {
                    host.MainForm = this.MainForm;
                    host.RecordPath = "";
                    host.MarcRecord = null;
                    host.MarcSyntax = "";
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


                ListViewBiblioLoader loader = new ListViewBiblioLoader(this.Channel,
                    stop,
                    items,
                    this.m_biblioTable);
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

                    string strMARC = "";
                    string strMarcSyntax = "";
                    // 将XML格式转换为MARC格式
                    // 自动从数据记录中获得MARC语法
                    nRet = MarcUtil.Xml2Marc(strXml,    // info.OldXml,
                        true,
                        null,
                        out strMarcSyntax,
                        out strMARC,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "XML转换到MARC记录时出错: " + strError;
                        goto ERROR1;
                    }

                    this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    host.MainForm = this.MainForm;
                    host.RecordPath = info.RecPath;
                    host.MarcRecord = new MarcRecord(strMARC);
                    host.MarcSyntax = strMarcSyntax;
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

                        item.ListViewItem.BackColor = SystemColors.Info;
                        item.ListViewItem.ForeColor = SystemColors.InfoText;
                    }

                    // 显示为工作单形式
                    i++;
                }

                {
                    host.MainForm = this.MainForm;
                    host.RecordPath = "";
                    host.MarcRecord = null;
                    host.MarcSyntax = "";
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
                strError = "执行 MarcQuery 脚本的过程中出现异常: " + ExceptionUtil.GetDebugText(ex);
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

                this.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行脚本 " + dlg.FileName + "</div>");
            }

            RefreshPropertyView(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
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
                DialogResult result = AutoCloseMessageBox.Show(this,
    e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
    20 * 1000,
    "BiblioSearchForm");
                if (result == DialogResult.Cancel)
                    e.ResultAction = "no";
                else
                    e.ResultAction = "yes";
            }
        }

#if NO
        void menu_quickMarcQueryRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要执行 MarcQuery 脚本的事项";
                goto ERROR1;
            }

            // 书目信息缓存
            // 如果已经初始化，则保持
            if (this.m_biblioTable == null)
                this.m_biblioTable = new Hashtable();

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定 MarcQuery 脚本文件";
            dlg.FileName = this.m_strUsedMarcQueryFilename;
            dlg.Filter = "MarcQuery 脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedMarcQueryFilename = dlg.FileName;

            MarcQueryHost host = null;
            Assembly assembly = null;

            nRet = PrepareMarcQuery(this.m_strUsedMarcQueryFilename,
                out assembly,
                out host,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在针对书目记录执行 MarcQuery 脚本 ...");
            stop.BeginLoop();

            this.EnableControls(false);

            this.listView_records.Enabled = false;
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

                    string strRecPath = item.Text;

                    if (string.IsNullOrEmpty(strRecPath) == true)
                        continue;

                    if (stop != null)
                    {
                        stop.SetMessage("正在获取书目记录 " + strRecPath);
                        stop.SetProgressValue(i);
                    }

                    BiblioInfo info = null;
                    nRet = GetBiblioInfo(
                        false,
                        item,
                        out info,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (info == null)
                        continue;
#if NO
                    string[] results = null;
                    byte[] baTimestamp = null;
                    // 获得书目记录
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

                    XmlDocument domXmlFragment = null;

                    // 装载书目以外的其它XML片断
                    nRet = LoadXmlFragment(strXml,
            out domXmlFragment,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

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
                        goto ERROR1;
                    }

                    // 存储所获得书目记录 XML
                    BiblioInfo info = null;
                    if (this.m_biblioTable != null)
                    {
                        info = (BiblioInfo)this.m_biblioTable[strRecPath];
                        if (info == null)
                        {
                            info = new BiblioInfo();
                            info.RecPath = strRecPath;
                            this.m_biblioTable[strRecPath] = info;
                        }

                        info.OldXml = strXml;
                        info.NewXml = "";
                    }
#endif
                    string strMARC = "";
                    string strMarcSyntax = "";
                    // 将XML格式转换为MARC格式
                    // 自动从数据记录中获得MARC语法
                    nRet = MarcUtil.Xml2Marc(info.OldXml,
                        true,
                        null,
                        out strMarcSyntax,
                        out strMARC,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "XML转换到MARC记录时出错: " + strError;
                        goto ERROR1;
                    }

                    host.MainForm = this.MainForm;
                    host.MarcRecord = new MarcRecord(strMARC);
                    host.MarcSyntax = strMarcSyntax;
                    host.Changed = false;

                    host.Main();

                    if (host.Changed == true)
                    {
                        string strXml = info.OldXml;
                        nRet = MarcUtil.Marc2XmlEx(host.MarcRecord.Text,
                            strMarcSyntax,
                            ref strXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

#if NO
                        // 合成其它XML片断
                        if (domXmlFragment != null
                            && string.IsNullOrEmpty(domXmlFragment.DocumentElement.InnerXml) == false)
                        {
                            XmlDocumentFragment fragment = domMarc.CreateDocumentFragment();
                            try
                            {
                                fragment.InnerXml = domXmlFragment.DocumentElement.InnerXml;
                            }
                            catch (Exception ex)
                            {
                                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                                goto ERROR1;
                            }

                            domMarc.DocumentElement.AppendChild(fragment);
                        }


                        strXml = domMarc.OuterXml;
#endif

                        if (info != null)
                        {
                            if (string.IsNullOrEmpty(info.NewXml) == true)
                                this.m_nChangedCount++;
                            info.NewXml = strXml;
                        }

                        item.BackColor = SystemColors.Info;
                        item.ForeColor = SystemColors.InfoText;
#if NO
                        byte[] baNewTimestamp = null;
                        string strOutputPath = "";
                        lRet = Channel.SetBiblioInfo(
                            stop,
                            "change",
                            strRecPath,
                            "xml",
                            strXml,
                            baTimestamp,
                            "",
                            out strOutputPath,
                            out baNewTimestamp,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
#endif
                    }

                    this.MainForm.OperHistory.AppendHtml("<p>" + HttpUtility.HtmlEncode(strRecPath) + "</p>");

                    // 显示为工作单形式

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
                }
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
            }

            DoViewComment(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
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

									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
   									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
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
                this,
                this.MainForm.DataDir,
                this.m_strUsedMarcFilterFilename,
                filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在针对书目记录执行 .fltx 脚本 ...");
            stop.BeginLoop();

            this.EnableControls(false);

            this.listView_records.Enabled = false;
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

                    string strRecPath = item.Text;

                    if (string.IsNullOrEmpty(strRecPath) == true)
                        continue;

                    if (stop != null)
                    {
                        stop.SetMessage("正在获取书目记录 " + strRecPath);
                        stop.SetProgressValue(i);
                    }

                    string[] results = null;
                    byte[] baTimestamp = null;
                    // 获得书目记录
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
                        goto ERROR1;
                    }

                    filter.Host = new ColumnFilterHost();
                    filter.Host.ColumnTable = new System.Collections.Hashtable();
                    nRet = filter.DoRecord(
    null,
    strMARC,
    strMarcSyntax,
    i,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    this.MainForm.OperHistory.AppendHtml("<p>" + HttpUtility.HtmlEncode(strRecPath) + "</p>");
                    foreach(string key in filter.Host.ColumnTable.Keys)
                    {
                        string strHtml = "<p>" + HttpUtility.HtmlEncode(key + "=" + (string)filter.Host.ColumnTable[key]) + "</p>";
                        this.MainForm.OperHistory.AppendHtml(strHtml);
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
                }
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
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 准备脚本环境
        static int PrepareMarcFilter(
            IWin32Window owner,
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
                MessageBox.Show(owner, strWarning);
            }

            filter.Assembly = assemblyFilter;
            return 0;
        ERROR1:
            return -1;
        }

        void menu_quickChangeRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要快速修改的事项";
                goto ERROR1;
            }

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在修改书目记录 ...");
            stop.BeginLoop();

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
                // form.MainForm = this.MainForm;
                form.MdiParent = this.MainForm;
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

            dlg.MainForm = this.MainForm;
            dlg.Text = strActionName + "书目记录到数据库";

            dlg.MessageText = "请指定书目记录要追加"+strActionName+"到的位置";
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
            this.MainForm.AppInfo.LinkFormState(dlg, "BiblioSearchform_BiblioSaveToDlg_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            if (Global.IsAppendRecPath(dlg.RecPath) == false)
            {
                strError = "目标记录路径 '"+dlg.RecPath+"' 不合法。必须是追加方式的路径，也就是说 ID 部分必须为问号";
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

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在"+strActionName+"书目记录到数据库 ...");
            stop.BeginLoop();

            this.EnableControlsInSearching(false);
            this.listView_records.Enabled = false;
            try
            {
                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                List<ListViewItem> moved_items = new List<ListViewItem>();
                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strRecPath = ListViewUtil.GetItemText(item, 0);

                    if (string.IsNullOrEmpty(strRecPath) == true)
                        continue;

                    // 观察源记录是否有998$t ?

                    // 是否要自动创建998字段内容?

                    string strOutputBiblioRecPath = "";
                    byte[] baOutputTimestamp = null;
                    string strOutputBiblio = "";

                    stop.SetMessage("正在"+strActionName+"书目记录 '" + strRecPath + "' 到 '" + dlg.RecPath + "' ...");

                    // result.Value:
                    //      -1  出错
                    //      0   成功，没有警告信息。
                    //      1   成功，有警告信息。警告信息在 result.ErrorInfo 中
                    long lRet = this.Channel.CopyBiblioInfo(
                        this.stop,
                        strAction,
                        strRecPath,
                        "xml",
                        null,
                        null,    // this.BiblioTimestamp,
                        dlg.RecPath,
                        null,   // strXml,
                        "",
                        out strOutputBiblio,
                        out strOutputBiblioRecPath,
                        out baOutputTimestamp,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (bCopy == false)
                        moved_items.Add(item);

                    stop.SetProgressValue(++i);
                }

                foreach (ListViewItem item in moved_items)
                {
                    item.Remove();
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

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

        void menu_exportToBiblioSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要装入其它查询窗的行";
                goto ERROR1;
            }

            BiblioSearchForm form = new BiblioSearchForm();
            form.MdiParent = this.MainForm;
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
            string strTempFileName = Path.Combine(this.MainForm.DataDir, "~export_to_searchform.txt");
            int nRet = SaveToEntityRecordPathFile(strDbType,
                strTempFileName,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            // TODO: 最好为具体类型的 SearchForm 类。否则推出时保留的遗留出鞥口类型不对
            ItemSearchForm form = new ItemSearchForm();
            form.DbType = strDbType;
            form.MdiParent = this.MainForm;
            form.Show();
#endif
            ItemSearchForm form = this.MainForm.OpenItemSearchForm(strDbType);

            nRet = form.ImportFromRecPathFile(strTempFileName,
            out strError);
            if (nRet == -1)
                return -1;
            return 0;
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
                    DialogResult result = MessageBox.Show(this,
                        "记录路径文件 '" + this.ExportEntityRecPathFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                        "BiblioSearchForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
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



            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得记录路径 ...");
            stop.BeginLoop();

            this.EnableControlsInSearching(false);
            this.listView_records.Enabled = false;

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportEntityRecPathFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
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
                            strError = "用户中断";
                            goto ERROR1;
                        }
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
                            stop,
                            this.Channel,
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
                            stop,
                            this.Channel,
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
                            stop,
                            this.Channel,
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
                            stop,
                            this.Channel,
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

                    stop.SetProgressValue(++i);
                }
            }

            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControlsInSearching(true);
                this.listView_records.Enabled = true;

                if (sw != null)
                    sw.Close();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            this.MainForm.StatusBarMessage = strDbTypeName + "记录记录路径 " + nCount.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportEntityRecPathFilename;
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

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得记录路径 ...");
            stop.BeginLoop();

            this.EnableControlsInSearching(false);
            this.listView_records.Enabled = false;

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
                            strError = "用户中断";
                            goto ERROR1;
                        }
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
                            stop,
                            this.Channel,
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
                            stop,
                            this.Channel,
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
                            stop,
                            this.Channel,
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
                            stop,
                            this.Channel,
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
                    stop.SetProgressValue(++i);
                }
            }

            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControlsInSearching(true);
                this.listView_records.Enabled = true;
            }

            this.MainForm.StatusBarMessage = "统计出下级" + strDbTypeName + "记录为空的书目记录 " + nCount.ToString() + "个";
            return 1;
        ERROR1:
            return -1;
        }

        // 从记录路径文件中导入
        void menu_importFromRecPathFile_Click(object sender, EventArgs e)
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

            string strLibraryUrl = StringUtil.CanonicalizeHostUrl(this.MainForm.LibraryServerUrl);

            // 需要刷新的行
            List<ListViewItem> items = new List<ListViewItem>();

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入记录路径 ...");
            stop.BeginLoop();

            this.EnableControlsInSearching(false);

            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this.listView_records);
                stop.SetProgressRange(0, sr.BaseStream.Length);

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
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            MessageBox.Show(this, "用户中断");
                            return;
                        }
                    }


                    string strLine = sr.ReadLine();

                    stop.SetProgressValue(sr.BaseStream.Position);

                    if (strLine == null)
                        break;

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
                            strError = "长路径 '"+strRecPath+"' 中的服务器 URL 部分 '"+strUrl+"' 和当前 dp2Circulation 服务器 URL '"+this.MainForm.LibraryServerUrl+"' 不匹配，因此无法导入这个记录路径文件";
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

                    if (this.MainForm.IsBiblioDbName(strDbName) == false)
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
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControlsInSearching(true);

                if (sr != null)
                    sr.Close();
            }

            if (items.Count > 0)
            {
                nRet = RefreshListViewLines(items,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
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
#if NO
        // 从记录路径文件中导入
        void menu_importFromRecPathFile_Click(object sender, EventArgs e)
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
            bool bSkipBrowse = false;

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

            this.EnableControlsInSearching(false);

            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this.listView_records);
                stop.SetProgressRange(0, sr.BaseStream.Length);

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
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            MessageBox.Show(this, "用户中断");
                            return;
                        }
                    }


                    string strLine = sr.ReadLine();

                    stop.SetProgressValue(sr.BaseStream.Position);

                    if (strLine == null)
                        break;

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

                    // 检查路径的正确性，检查数据库是否为书目库之一
                    // 判断它是书目记录路径，还是实体记录路径？
                    string strDbName = Global.GetDbName(strRecPath);
                    if (string.IsNullOrEmpty(strDbName) == true)
                    {
                        strError = "'" + strRecPath + "' 不是合法的记录路径";
                        goto ERROR1;
                    }

                    if (this.MainForm.IsBiblioDbName(strDbName) == false)
                    {
                        strError = "路径 '"+strRecPath+"' 中的数据库名 '" + strDbName + "' 不是合法的书目库名。很可能所指定的文件不是书目库的记录路径文件";
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

                        if (bSkipBrowse == false
                            && !(Control.ModifierKeys == Keys.Shift))
                        {
                            nRet = RefreshOneBrowseLine(item,
                    out strError);
                            if (nRet == -1)
                            {
                                DialogResult result = MessageBox.Show(this,
            "获得浏览内容时出错: " + strError + "。\r\n\r\n是否继续获取浏览内容? (Yes 获取；No 不获取；Cancel 放弃导入)",
            "BiblioSearchForm",
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

                this.EnableControlsInSearching(true);

                if (sr != null)
                    sr.Close();
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif


        // 调用前，记录路径列已经有值
        /// <summary>
        /// 刷新一个浏览行。调用前，记录路径列已经有值
        /// </summary>
        /// <param name="item">浏览行 ListViewItem 对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int RefreshBrowseLine(ListViewItem item,
            out string strError)
        {
            strError = "";

            string strRecPath = ListViewUtil.GetItemText(item, 0);
            string[] paths = new string[1];
            paths[0] = strRecPath;
            DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

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

        void menu_reverseSelectAllLines_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in this.ListViewRecords.Items)
                {
                    if (item.Selected == true)
                        item.Selected = false;
                    else
                        item.Selected = true;
                }
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
                ListViewUtil.SelectAllLines(this.listView_records);
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
        void menu_deleteSelectedRecords_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
    "确实要从数据库中删除所选定的 " + this.listView_records.SelectedItems.Count.ToString() + " 个书目记录?\r\n\r\n(警告：书目记录被删除后，无法恢复。如果删除书目记录，则其下属的册、期、订购、评注记录和对象资源会一并删除)\r\n\r\n(OK 删除；Cancel 取消)",
    "BiblioSearchForm",
    MessageBoxButtons.OKCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach(ListViewItem item in this.listView_records.SelectedItems)
            {
                items.Add(item);
            }

            string strError = "";
            int nDeleteCount = 0;

            // 检查前端权限
            bool bDeleteSub = StringUtil.IsInList("client_deletebibliosubrecords", this.Channel.Rights);

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除书目记录 ...");
            stop.BeginLoop();

            this.EnableControlsInSearching(false);
            this.listView_records.Enabled = false;
            try
            {
                stop.SetProgressRange(0, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "已中断";
                            goto ERROR1;
                        }
                    }

                    ListViewItem item = items[i];
                    string strRecPath = ListViewUtil.GetItemText(item, 0);

                    string[] results = null;
                    byte[] baTimestamp = null;
                    string strOutputPath = "";
                    string[] formats = null;
                    if (bDeleteSub == false && this.MainForm.ServerVersion >= 2.30)
                    {
                        formats = new string[1];
                        formats[0] = "subcount";
                    }

                    stop.SetMessage("正在删除书目记录 " + strRecPath);

                    long lRet = Channel.GetBiblioInfos(
                        stop,
                        strRecPath,
                        "",
                        formats,   // formats
                        out results,
                        out baTimestamp,
                        out strError);
                    if (lRet == 0)
                        goto ERROR1;
                    if (lRet == -1)
                    {
                        result = MessageBox.Show(this,
    "在获得记录 '" + strRecPath + "' 的时间戳的过程中出现错误: "+strError+"。\r\n\r\n是否继续强行删除此记录? (Yes 强行删除；No 不删除；Cancel 放弃当前未完成的全部删除操作)",
    "BiblioSearchForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            goto ERROR1;
                        if (result == System.Windows.Forms.DialogResult.No)
                            continue;
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
                            result = MessageBox.Show(this,
"书目记录 '" + strRecPath + "' 包含 " + strSubCount + " 个下级记录，而当前用户并不具备 client_deletebibliosubrecords 权限，无法删除这条书目记录。\r\n\r\n是否继续后面的操作? \r\n\r\n(Yes 继续；No 终止未完成的全部删除操作)",
"BiblioSearchForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                            if (result == System.Windows.Forms.DialogResult.No)
                            {
                                strError = "中断操作";
                                goto ERROR1;
                            }
                            continue;
                        }
                    }

                    byte[] baNewTimestamp = null;

                    lRet = Channel.SetBiblioInfo(
                        stop,
                        "delete",
                        strRecPath,
                        "xml",
                        "", // strXml,
                        baTimestamp,
                        "",
                        out strOutputPath,
                        out baNewTimestamp,
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


                this.EnableControlsInSearching(true);
                this.listView_records.Enabled = true;
            }

            MessageBox.Show(this, "成功删除书目记录 " + nDeleteCount + " 条");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从窗口中移走所选择的事项
        void menu_removeSelectedItems_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            for (int i = this.listView_records.SelectedIndices.Count - 1; i >= 0; i--)
            {
                this.listView_records.Items.RemoveAt(this.listView_records.SelectedIndices[i]);
            }

            this.Cursor = oldCursor;
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
                return this.MainForm.AppInfo.GetString(
                    "bibliosearchform",
                    "last_iso2709_filename",
                    "");
            }
            set
            {
                this.MainForm.AppInfo.SetString(
                    "bibliosearchform",
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
                return this.MainForm.AppInfo.GetBoolean(
                    "bibliosearchform",
                    "last_iso2709_crlf",
                    false);
            }
            set
            {
                this.MainForm.AppInfo.SetBoolean(
                    "bibliosearchform",
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
                return this.MainForm.AppInfo.GetBoolean(
                    "bibliosearchform",
                    "last_iso2709_removefield998",
                    false);
            }
            set
            {
                this.MainForm.AppInfo.SetBoolean(
                    "bibliosearchform",
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
                return this.MainForm.AppInfo.GetString(
                    "bibliosearchform",
                    "last_encoding_name",
                    "");
            }
            set
            {
                this.MainForm.AppInfo.SetString(
                    "bibliosearchform",
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
                return this.MainForm.AppInfo.GetString(
                    "bibliosearchform",
                    "last_cataloging_rule",
                    "<无限制>");
            }
            set
            {
                this.MainForm.AppInfo.SetString(
                    "bibliosearchform",
                    "last_cataloging_rule",
                    value);
            }
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

            Encoding preferredEncoding = this.CurrentEncoding;

            {
                // 观察要保存的第一条记录的marc syntax
            }

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

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导出到 XML 文件 ...");
            stop.BeginLoop();

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
                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument();
                writer.WriteStartElement("dprms", "collection", DpNs.dprms);

                writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);
#if NO
                writer.WriteAttributeString("xmlns", "unimarc", null, DigitalPlatform.Xml.Ns.unimarcxml);
                writer.WriteAttributeString("xmlns", "marc21", null, DigitalPlatform.Xml.Ns.usmarcxml);
#endif

                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

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
                            DomUtil.SetAttr(dom.DocumentElement, "path", DpNs.dprms, this.MainForm.LibraryServerUrl + "?" + strRecPath);
                            DomUtil.SetAttr(dom.DocumentElement, "timestamp", DpNs.dprms, ByteArray.GetHexTimeStampString(baTimestamp));

                            dom.DocumentElement.WriteTo(writer);
                        }
                    }

                    stop.SetProgressValue(++i);
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

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
            }

            MainForm.StatusBarMessage = this.listView_records.SelectedItems.Count.ToString()
                + "条记录成功保存到文件 " + dlg.FileName;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 2012/2/14
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
            dlg.AddG01Visible = false;
            dlg.RuleVisible = true;
            dlg.Rule = this.LastCatalogingRule;
            dlg.FileName = this.LastIso2709FileName;
            dlg.CrLf = this.LastCrLfIso2709;
            dlg.RemoveField998 = this.LastRemoveField998;
            dlg.EncodingListItems = Global.GetEncodingList(false);
            dlg.EncodingName =
                (String.IsNullOrEmpty(this.LastEncodingName) == true ? Global.GetEncodingName(preferredEncoding) : this.LastEncodingName);
            dlg.EncodingComment = "注: 原始编码方式为 " + Global.GetEncodingName(preferredEncoding);
            dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            dlg.EnableMarcSyntax = false;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

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

            this.LastIso2709FileName = dlg.FileName;
            this.LastCrLfIso2709 = dlg.CrLf;
            this.LastEncodingName = dlg.EncodingName;
            this.LastCatalogingRule = dlg.Rule;
            this.LastRemoveField998 = dlg.RemoveField998;

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导出到 MARC 文件 ...");
            stop.BeginLoop();

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
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

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
                        strMARC = record.Text;
                    }
                    if (dlg.Mode880 == true && strMarcSyntax == "usmarc")
                    {
                        MarcRecord record = new MarcRecord(strMARC);
                        MarcQuery.To880(record);
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
                        out baTarget,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    /*
                    Encoding sourceEncoding = connection.GetRecordsEncoding(
                        this.MainForm,
                        record.m_strSyntaxOID);


                    if (sourceEncoding.Equals(targetEncoding) == true)
                    {
                        // source和target编码方式相同，不用转换
                        baTarget = record.m_baRecord;
                    }
                    else
                    {
                        nRet = ChangeIso2709Encoding(
                            sourceEncoding,
                            record.m_baRecord,
                            targetEncoding,
                            strMarcSyntax,
                            out baTarget,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }*/

                    s.Write(baTarget, 0,
                        baTarget.Length);

                    if (dlg.CrLf == true)
                    {
                        byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                        s.Write(baCrLf, 0,
                            baCrLf.Length);
                    }

                    stop.SetProgressValue(++i);
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

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
            }

            // 
            if (bAppend == true)
                MainForm.StatusBarMessage = this.listView_records.SelectedItems.Count.ToString()
                    + "条记录成功追加到文件 " + this.LastIso2709FileName + " 尾部";
            else
                MainForm.StatusBarMessage = this.listView_records.SelectedItems.Count.ToString()
                    + "条记录成功保存到新文件 " + this.LastIso2709FileName;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
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
            StreamWriter sw = new StreamWriter(this.ExportRecPathFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
            try
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    sw.WriteLine(item.Text);
                }

                this.Cursor = oldCursor;
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            this.MainForm.StatusBarMessage = "书目记录路径 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportRecPathFilename;
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

            ListViewUtil.OnSeletedIndexChanged(this.listView_records,
                0,
                null);

            if (this.m_biblioTable != null)
            {
                // if (CanCallNew(commander, WM_SELECT_INDEX_CHANGED) == true)
                    RefreshPropertyView(false);
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

                        ListViewUtil.OnSeletedIndexChanged(this.listView_records,
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

        /*public*/ bool CanCallNew(Commander commander, int msg)
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
            this.Text = "书目查询 " + this.textBox_queryWord.Text;
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
        private void toolStripButton_search_Click(object sender, EventArgs e)
        {
            DoSearch(false, false);
        }

#if NO
        // 在浏览窗中继续装入没有装完的部分
        private void MenuItem_continueLoad_Click(object sender, EventArgs e)
        {

        }
#endif

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

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            this.EnableControlsInSearching(false);
            try
            {
                stop.SetProgressRange(0, lHitCount);

                long lStart = this.listView_records.Items.Count;
                long lPerCount = Math.Min(50, lHitCount);
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                bool bPushFillingBrowse = this.PushFillingBrowse;

                this.label_message.Text = "正在继续装入浏览信息...";

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            // MessageBox.Show(this, "用户中断");
                            this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已装入 " + lStart.ToString() + " 条，用户中断...";
                            return;
                        }
                    }

                    stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    /*
                    string strStyle = "id,cols";
                    if (this.m_bFirstColumnIsKey == true)
                        strStyle = "keyid,id,key,cols";
                     * */

                    bool bTempQuickLoad = bQuickLoad;

                    if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                        bTempQuickLoad = true;

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

                    long lRet = Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lPerCount,
                        strStyle,
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
                        DigitalPlatform.CirculationClient.localhost.Record searchresult = searchresults[i];

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
                    stop.SetProgressValue(lStart);
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
                this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已全部装入";
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControlsInSearching(true);
            }

            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripMenuItem_searchKeyID_Click(object sender, EventArgs e)
        {
            DoSearch(false, true);
        }

        // 尺寸为0,0的按钮，为了满足this.AcceptButton
        private void button_search_Click(object sender, EventArgs e)
        {
            DoSearch(false, false);
        }

        private void dp2QueryControl1_GetList(object sender, DigitalPlatform.CommonControl.GetListEventArgs e)
        {
            // 获得所有数据库名
            if (string.IsNullOrEmpty(e.Path) == true)
            {
                if (this.MainForm.BiblioDbProperties != null)
                {
                    for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                    {
                        BiblioDbProperty property = this.MainForm.BiblioDbProperties[i];
                        e.Values.Add(property.DbName);
                    }
                }
            }
            else
            {
                // 获得特定数据库的检索途径
                // 每个库都一样
                for (int i = 0; i < this.MainForm.BiblioDbFromInfos.Length; i++)
                {
                    BiblioDbFromInfo info = this.MainForm.BiblioDbFromInfos[i];
                    e.Values.Add(info.Caption);   // + "\t" + info.Style);
                }
            }
        }

        private void dp2QueryControl1_ViewXml(object sender, EventArgs e)
        {
            string strError = "";
            string strQueryXml = "";

            int nRet = dp2QueryControl1.BuildQueryXml(
this.MaxSearchResultCount,
"zh",
out strQueryXml,
out strError);
            if (nRet == -1)
            {
                strError = "在创建XML检索式的过程中出错: " + strError;
                goto ERROR1;
            }

            XmlViewerForm dlg = new XmlViewerForm();

            dlg.Text = "检索式XML";
            dlg.MainForm = this.MainForm;
            dlg.XmlString = strQueryXml;
            // dlg.StartPosition = FormStartPosition.CenterScreen;

            this.MainForm.AppInfo.LinkFormState(dlg, "bibliosearchform_viewqueryxml");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
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

        void menu_logicSearch_Click(object sender, EventArgs e)
        {
            this.DoLogicSearch(false);
        }

        void menu_logicSearchKeyID_Click(object sender, EventArgs e)
        {
            this.DoLogicSearch(true);
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
            this.MainForm.AppInfo.LinkFormState(dlg, "searchbiblioform_gettimedialog_single");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

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

            this.MainForm.AppInfo.LinkFormState(dlg, "searchbiblioform_gettimedialog_single");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

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
            this.MainForm.AppInfo.LinkFormState(dlg, "searchbiblioform_gettimedialog_range");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

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

            this.MainForm.AppInfo.LinkFormState(dlg, "searchbiblioform_gettimedialog_range");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_queryWord.Text = dlg.uString;

        }

        private void toolStripMenuItem_searchKeys_Click(object sender, EventArgs e)
        {
            DoSearch(true, false);
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
            e.FromStyles = this.MainForm.GetBiblioFromStyle(e.FromCaption);
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

                string[] results = null;
                byte[] baTimestamp = null;
                // 获得书目记录
                long lRet = Channel.GetBiblioInfos(
                    stop,
                    strRecPath,
                    "",
                    new string[] { "xml" },   // formats
                    out results,
                    out baTimestamp,
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
                if (this.listView_records.SelectedItems.Count == 1)
                    item = this.listView_records.SelectedItems[0];

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
            string strError = "";
            string strHtml = "";
            // string strXml = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (this.MainForm.PanelFixedVisible == false
                    && (m_commentViewer == null || m_commentViewer.Visible == false))
                    return;
                // 2013/3/7
                if (this.MainForm.CanDisplayItemProperty() == false)
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

            m_commentViewer.MainForm = this.MainForm;  // 必须是第一句

            if (bNew == true)
                m_commentViewer.InitialWebBrowser();

            m_commentViewer.Text = "MARC内容 '" + info.RecPath + "'";
            m_commentViewer.HtmlString = strHtml;
            m_commentViewer.XmlString = MergeXml(strXml1, strXml2);
            m_commentViewer.FormClosed -= new FormClosedEventHandler(marc_viewer_FormClosed);
            m_commentViewer.FormClosed += new FormClosedEventHandler(marc_viewer_FormClosed);
            // this.MainForm.AppInfo.LinkFormState(m_viewer, "comment_viewer_state");
            // m_viewer.ShowDialog(this);
            // this.MainForm.AppInfo.UnlinkFormState(m_viewer);
            if (bOpenWindow == true)
            {
                if (m_commentViewer.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(m_commentViewer, "marc_viewer_state");
                    m_commentViewer.Show(this);
                    m_commentViewer.Activate();

                    this.MainForm.CurrentPropertyControl = null;
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
                    if (this.MainForm.CurrentPropertyControl != m_commentViewer.MainControl)
                        m_commentViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, "DoViewComment() 出错: " + strError);
        }

#if NO
        void _doViewProperty(bool bOpenWindow)
        {
            string strError = "";
            string strHtml = "";
            // string strXml = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (this.MainForm.PanelFixedVisible == false
                    && (m_commentViewer == null || m_commentViewer.Visible == false))
                    return;
                // 2013/3/7
                if (this.MainForm.CanDisplayItemProperty() == false)
                    return;
            }

            if (this.m_biblioTable == null
                || this.listView_records.SelectedItems.Count != 1)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }

            ListViewItem item = this.listView_records.SelectedItems[0];
#if NO
            string strRecPath = this.listView_records.SelectedItems[0].Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }
#endif

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

            m_commentViewer.MainForm = this.MainForm;  // 必须是第一句

            if (bNew == true)
                m_commentViewer.InitialWebBrowser();

            m_commentViewer.Text = "MARC内容 '" + info.RecPath + "'";
            m_commentViewer.HtmlString = strHtml;
            m_commentViewer.XmlString = MergeXml(strXml1, strXml2);
            m_commentViewer.FormClosed -= new FormClosedEventHandler(marc_viewer_FormClosed);
            m_commentViewer.FormClosed += new FormClosedEventHandler(marc_viewer_FormClosed);
            // this.MainForm.AppInfo.LinkFormState(m_viewer, "comment_viewer_state");
            // m_viewer.ShowDialog(this);
            // this.MainForm.AppInfo.UnlinkFormState(m_viewer);
            if (bOpenWindow == true)
            {
                if (m_commentViewer.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(m_commentViewer, "marc_viewer_state");
                    m_commentViewer.Show(this);
                    m_commentViewer.Activate();

                    this.MainForm.CurrentPropertyControl = null;
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
                    if (this.MainForm.CurrentPropertyControl != m_commentViewer.MainControl)
                        m_commentViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, "DoViewComment() 出错: " + strError);
        }
#endif

        static string MergeXml(string strXml1,
            string strXml2)
        {
            if (string.IsNullOrEmpty(strXml1) == true)
                return strXml2;
            if (string.IsNullOrEmpty(strXml2) == true)
                return strXml1;

            return strXml1; // 临时这样
        }

        int GetXmlHtml(BiblioInfo info,
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
                string strOldImageFragment = GetImageHtmlFragment(
    info.RecPath,
    strOldMARC);
                string strNewImageFragment = GetImageHtmlFragment(
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
                string strImageFragment = GetImageHtmlFragment(
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
                string strImageFragment = GetImageHtmlFragment(
    info.RecPath,
    strNewMARC);
                strHtml2 = MarcUtil.GetHtmlOfMarc(strNewMARC,
                    strNewFragmentXml,
                    strImageFragment,
                    false);
            }
            return 0;
        }

        public static string GetImageHtmlFragment(
            string strBiblioRecPath,
            string strMARC)
        {
            string strImageUrl = ScriptUtil.GetCoverImageUrl(strMARC, "MediumImage");

            if (string.IsNullOrEmpty(strImageUrl) == true)
                return "";

            if (StringUtil.HasHead(strImageUrl, "http:") == true)
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
                this.MainForm.AppInfo.UnlinkFormState(m_commentViewer);
                this.m_commentViewer = null;
            }
        }

        string GetHeadString(bool bAjax = true)
        {
            string strCssFilePath = PathUtil.MergePath(this.MainForm.DataDir, "operloghtml.css");

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
        }

        public string QueryWord
        {
            get
            {
                return this.textBox_queryWord.Text;
            }
            set
            {
                this.textBox_queryWord.Text = value;
            }
        }

        public string From
        {
            get
            {
                return this.comboBox_from.Text;
            }
            set
            {
                this.comboBox_from.Text = value;
            }
        }

        public string MatchStyle
        {
            get
            {
                return this.comboBox_matchStyle.Text;
            }
            set
            {
                this.comboBox_matchStyle.Text = value;
            }
        }

        private void ToolStripMenuItem_searchShareBiblio_Click(object sender, EventArgs e)
        {
            if (this.SearchShareBiblio == false)
            {
                // 先检查一下当前用户是否允许开放共享书目
                if (this.MainForm != null && this.MainForm.MessageHub != null
    && this.MainForm.MessageHub.ShareBiblio == false)
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
                    this.MainForm.MessageHub.ShareBiblio = true;
                }

                this.SearchShareBiblio = true;
            }
            else
                this.SearchShareBiblio = false;

            UpdateMenu();
        }

        // 更新菜单状态
        void UpdateMenu()
        {
            this.ToolStripMenuItem_searchShareBiblio.Checked = this.SearchShareBiblio;
            if (this.MainForm != null && this.MainForm.MessageHub != null)
            {
                if (this.MainForm.MessageHub.ShareBiblio == false)
                    this.ToolStripMenuItem_searchShareBiblio.Text = "使用共享网络 [暂时被禁用]";
                else
                    this.ToolStripMenuItem_searchShareBiblio.Text = "使用共享网络";
            }
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
            m_loader.Format = "xml";
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
                BiblioInfo info = (BiblioInfo)this.CacheTable[strRecPath];
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

                BiblioInfo info = (BiblioInfo)this.CacheTable[strRecPath];
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
                    this.CacheTable[strRecPath] = info;
                    yield return new LoaderItem(info, item);
                }
                else
                    yield return new LoaderItem(info, item);
            }
        }
    }
}