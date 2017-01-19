using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Reflection;
using System.Web;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.CommonControl;

using DigitalPlatform.Script;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 实体查询窗、订购查询窗、期查询窗、评注查询窗
    /// </summary>
    public partial class ItemSearchForm : ItemSearchFormBase
    {
        // const int WM_SELECT_INDEX_CHANGED = API.WM_USER + 200;

        List<ItemQueryParam> m_queries = new List<ItemQueryParam>();
        int m_nQueryIndex = -1;

        /// <summary>
        /// 最近一次导出到条码号文件时使用过的文件名
        /// </summary>
        public string ExportBarcodeFilename = "";

        /// <summary>
        /// 最近一次导出到记录路径文件时使用过的文件名
        /// </summary>
        public string ExportRecPathFilename = "";

        /// <summary>
        /// 最近一次导出到文本文件时使用过的文件名
        /// </summary>
        public string ExportTextFilename = "";

        /// <summary>
        /// 最近一次导出到书目库记录路径文件时使用过的文件名
        /// </summary>
        public string ExportBiblioRecPathFilename = "";

        /// <summary>
        /// 最近一次导出到实体库记录路径文件时使用过的文件名
        /// </summary>
        public string ExportItemRecPathFilename = "";

        /// <summary>
        /// 最近一次导出到书目转储文件时使用过的文件名
        /// </summary>
        public string ExportBiblioDumpFilename = "";

        /// <summary>
        /// 浏览框。显示检索命中记录的浏览格式
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
        public ItemSearchForm()
        {
            InitializeComponent();

            _listviewRecords = this.listView_records;

            ListViewProperty prop = new ListViewProperty();
            this.listView_records.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);

            prop.CompareColumn -= new CompareEventHandler(prop_CompareColumn);
            prop.CompareColumn += new CompareEventHandler(prop_CompareColumn);
        }

        void prop_CompareColumn(object sender, CompareEventArgs e)
        {
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
            else
                e.Result = string.Compare(e.String1, e.String2);
        }

        void ClearListViewPropertyCache()
        {
            ListViewProperty prop = (ListViewProperty)this.listView_records.Tag;
            prop.ClearCache();
        }

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            if (e.DbName == "<blank>")
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                e.ColumnTitles.Add("检索点");
                e.ColumnTitles.Add("数量");
                // 数量列的排序
                e.ListViewProperty.SetSortStyle(2, ColumnSortStyle.RightAlign);
                return;
            }

            e.ColumnTitles = new ColumnPropertyCollection();
            ColumnPropertyCollection temp = this.MainForm.GetBrowseColumnProperties(e.DbName);
            if (temp != null)
            {
                if (m_bBiblioSummaryColumn == true)
                    e.ColumnTitles.Insert(0, "书目摘要");
                e.ColumnTitles.AddRange(temp);  // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件
            }

            if (this.m_bFirstColumnIsKey == true)
                e.ColumnTitles.Insert(0, "命中的检索点");

            // e.ListViewProperty.SetSortStyle(2, ColumnSortStyle.LeftAlign);   // 应该根据 type为item_barcode 来决定排序方式
        }

        private void ItemSearchForm_Load(object sender, EventArgs e)
        {
            /*
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
             * */
            this.FillFromList();

            string strDefaultFrom = "";
            if (this.DbType == "item")
                strDefaultFrom = "册条码";
            else if (this.DbType == "comment")
                strDefaultFrom = "标题";
            else if (this.DbType == "order")
                strDefaultFrom = "书商";
            else if (this.DbType == "issue")
                strDefaultFrom = "期号";
            else if (this.DbType == "arrive")
                strDefaultFrom = "册条码号";
            else
                throw new Exception("未知的DbType '" + this.DbType + "'");


            this.comboBox_from.Text = this.MainForm.AppInfo.GetString(
                this.DbType + "_search_form",
                "from",
                strDefaultFrom);

            this.comboBox_entityDbName.Text = this.MainForm.AppInfo.GetString(
                this.DbType + "_search_form",
                "entity_db_name",
                "<全部>");

            this.comboBox_matchStyle.Text = this.MainForm.AppInfo.GetString(
                this.DbType + "_search_form",
                "match_style",
                "精确一致");

            if (this.DbType != "arrive")
            {
                bool bHideMatchStyle = this.MainForm.AppInfo.GetBoolean(
                    this.DbType + "_search_form",
                    "hide_matchstyle_and_dbname",
                    false);
                if (bHideMatchStyle == true)
                {
                    this.label_matchStyle.Visible = false;
                    this.comboBox_matchStyle.Visible = false;
                    this.comboBox_matchStyle.Text = "精确一致"; // 隐藏后，采用缺省值

                    this.label_entityDbName.Visible = false;
                    this.comboBox_entityDbName.Visible = false;
                    this.comboBox_entityDbName.Text = "<全部>"; // 隐藏后，采用缺省值

                    string strName = this.DbTypeCaption;
                    if (this.DbType == "item")
                        strName = "实体";

                    this.label_message.Text = "当前检索所采用的匹配方式为 '精确一致'，针对全部" + strName + "库";
                }
            }

#if NO
            string strWidths = this.MainForm.AppInfo.GetString(
                this.DbType + "_search_form",
                "record_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_records,
                    strWidths,
                    true);
            }
#endif
            this.UiState = this.MainForm.AppInfo.GetString(
    this.DbType + "_search_form",
    "ui_state",
    "");
            string strSaveString = this.MainForm.AppInfo.GetString(
this.DbType + "_search_form",
"query_lines",
"^^^");
            this.dp2QueryControl1.Restore(strSaveString);

            comboBox_matchStyle_TextChanged(null, null);

            this.SetWindowTitle();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.splitContainer_main);
                controls.Add(this.tabControl_query);
                controls.Add(this.listView_records);

                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.splitContainer_main);
                controls.Add(this.tabControl_query);
                controls.Add(this.listView_records);
                GuiState.SetUiState(controls, value);
            }
        }


        void SetWindowTitle()
        {
            string strLogic = "";
            if (this.tabControl_query.SelectedTab == this.tabPage_logic)
                strLogic = " 逻辑检索";

            if (this.DbType == "item")
            {
                this.Text = "实体查询" + strLogic;
                this.label_entityDbName.Text = "实体库(&D)";
            }
            else
            {
                this.Text = this.DbTypeCaption + "查询" + strLogic;
                this.label_entityDbName.Text = this.DbTypeCaption + "库(&D)";
            }
        }

        static string[] item_froms = {
                                       "册条码",
                                       "批次号",
                                       "登录号",
                                       "索取号",
                                       "参考ID",
                                       "馆藏地点",
                                       "索取类号",
                                       "父记录",
                                       "状态",
                                       "__id"
                                   };


        static string[] comment_froms = {
                "标题",
                "作者",
                "作者显示名",
                "正文",
                "参考ID",
                "最后修改时间",
                "父记录",
                "状态",
                "__id"
                                      };
        static string[] order_froms = {
                "书商",
                "批次号",
                "册参考ID",
                "订购时间",
                "参考ID",
                "父记录",
                "状态",
                "__id"
                                    };
        static string[] issue_froms = {
                "出版时间",
                "期号",
                "总期号",
                "卷号",
                "册参考ID",
                "参考ID",
                "批次号",
                "父记录",
                "状态",
                "__id"
                                    };

        List<string> GetFromList()
        {
            List<string> results = new List<string>();

            // 填入从服务器获得的最新froms
            if (this.MainForm != null)
            {
                BiblioDbFromInfo[] infos = null;
                if (this.DbType == "item")
                    infos = this.MainForm.ItemDbFromInfos;
                else if (this.DbType == "comment")
                    infos = this.MainForm.CommentDbFromInfos;
                else if (this.DbType == "order")
                    infos = this.MainForm.OrderDbFromInfos;
                else if (this.DbType == "issue")
                    infos = this.MainForm.IssueDbFromInfos;
                else if (this.DbType == "arrive")
                    infos = this.MainForm.ArrivedDbFromInfos;
                else
                    throw new Exception("未知的DbType '" + this.DbType + "'");

                if (infos != null && infos.Length > 0)
                {
                    for (int i = 0; i < infos.Length; i++)
                    {
                        string strCaption = infos[i].Caption;
                        results.Add(strCaption);
                    }

                    return results;
                }
            }

            if (this.DbType == "item")
                return new List<string>(item_froms);
            else if (this.DbType == "comment")
                return new List<string>(comment_froms);
            else if (this.DbType == "order")
                return new List<string>(order_froms);
            else if (this.DbType == "issue")
                return new List<string>(issue_froms);
            else if (this.DbType == "arrive")
                return new List<string>(arrive_froms);
            else
                throw new Exception("未知的DbType '" + this.DbType + "'");
        }

        void FillFromList()
        {
#if NO
            string[] item_froms = {
                                       "册条码",
                                       "批次号",
                                       "登录号",
                                       "索取号",
                                       "参考ID",
                                       "馆藏地点",
                                       "索取类号",
                                       "父记录",
                                       "状态",
                                       "__id"
                                   };
            string[] comment_froms = {
                "标题",
                "作者",
                "作者显示名",
                "正文",
                "参考ID",
                "最后修改时间",
                "父记录",
                "状态",
                "__id"
                                      };
            string[] order_froms = {
                "书商",
                "批次号",
                "册参考ID",
                "订购时间",
                "参考ID",
                "父记录",
                "状态",
                "__id"
                                    };
            string[] issue_froms = {
                "出版时间",
                "期号",
                "总期号",
                "卷号",
                "册参考ID",
                "参考ID",
                "批次号",
                "父记录",
                "状态",
                "__id"
                                    };

            string[] froms = null;

            if (this.DbType == "item")
                froms = item_froms;
            else if (this.DbType == "comment")
                froms = comment_froms;
            else if (this.DbType == "order")
                froms = order_froms;
            else if (this.DbType == "issue")
                froms = issue_froms;
            else
                throw new Exception("未知的DbType '" + this.DbType + "'");

            this.comboBox_from.Items.Clear();
            foreach (string from in froms)
            {
                this.comboBox_from.Items.Add(from);
            }

            // 填入从服务器获得的最新froms
            if (this.MainForm != null)
            {
                BiblioDbFromInfo[] infos = null;
                if (this.DbType == "item")
                    infos = this.MainForm.ItemDbFromInfos;
                else if (this.DbType == "comment")
                    infos = this.MainForm.CommentDbFromInfos;
                else if (this.DbType == "order")
                    infos = this.MainForm.OrderDbFromInfos;
                else if (this.DbType == "issue")
                    infos = this.MainForm.IssueDbFromInfos;
                else
                    throw new Exception("未知的DbType '" + this.DbType + "'");

                if (infos != null && infos.Length > 0)
                {
                    this.comboBox_from.Items.Clear();
                    for (int i = 0; i < infos.Length; i++)
                    {
                        string strCaption = infos[i].Caption;
                        this.comboBox_from.Items.Add(strCaption);
                    }
                }
            }
#endif
            this.comboBox_from.Items.Clear();
            List<string> froms = GetFromList();
            foreach (string from in froms)
            {
                this.comboBox_from.Items.Add(from);
            }
            // this.comboBox_from.Items.AddRange(GetFromList());

        }

        private void ItemSearchForm_FormClosing(object sender, FormClosingEventArgs e)
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
        }

        private void ItemSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            /*
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }*/

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetString(
                    this.DbType + "_search_form",
                    "from",
                    this.comboBox_from.Text);

                this.MainForm.AppInfo.SetString(
                    this.DbType + "_search_form",
                    "entity_db_name",
                    this.comboBox_entityDbName.Text);

                this.MainForm.AppInfo.SetString(
                    this.DbType + "_search_form",
                    "match_style",
                    this.comboBox_matchStyle.Text);

#if NO
            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_records);
            this.MainForm.AppInfo.SetString(
                this.DbType + "_search_form",
                "record_list_column_width",
                strWidths);
#endif
                this.MainForm.AppInfo.SetString(
                    this.DbType + "_search_form",
                    "ui_state",
                    this.UiState);

                this.MainForm.AppInfo.SetString(
    this.DbType + "_search_form",
    "query_lines",
    this.dp2QueryControl1.GetSaveString());
            }
        }

        /// <summary>
        /// 获得配置参数：当前查询窗单次检索最大命中记录条数定义。-1表示不限制
        /// </summary>
        public int MaxSearchResultCount
        {
            get
            {
                return (int)this.MainForm.AppInfo.GetInt(
                this.DbType + "_search_form",
                "max_result_count",
                -1);
            }
        }

        // 2008/1/20 
        /// <summary>
        /// 获得配置参数：当前查询窗检索时，是否以推动的方式装入浏览列表
        /// </summary>
        public bool PushFillingBrowse
        {
            get
            {
                return MainForm.AppInfo.GetBoolean(
                this.DbType + "_search_form",
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

        private void button_search_Click(object sender, EventArgs e)
        {
            DoSearch(false, false, null);
        }

        ItemQueryParam PanelToQuery()
        {
            ItemQueryParam query = new ItemQueryParam();

            query.QueryWord = this.textBox_queryWord.Text;
            query.DbNames = this.comboBox_entityDbName.Text;
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

        public static int SearchOneLocationItems(
            MainForm main_form,
            LibraryChannel channel,
            Stop stop,
            string strLocation,
            string strOutputStyle,
            out List<string> results,
            out string strError)
        {
            strError = "";
            results = new List<string>();

            long lRet = channel.SearchItem(stop,
                "<all>",
                strLocation, // 
                -1,
                "馆藏地点",
                "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
                "zh",
                null,   // strResultSetName
                "",    // strSearchStyle
                "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                out strError);
            if (lRet == -1)
                return -1;
            long lHitCount = lRet;

            long lStart = 0;
            long lCount = lHitCount;
            DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

            bool bOutputBiblioRecPath = false;
            bool bOutputItemRecPath = false;
            string strStyle = "";
            if (strOutputStyle == "bibliorecpath")
            {
                bOutputBiblioRecPath = true;
                strStyle = "id,cols,format:@coldef:*/parent";
            }
            else
            {
                bOutputItemRecPath = true;
                strStyle = "id";
            }

            // 实体库名 --> 书目库名
            Hashtable dbname_table = new Hashtable();

            // 书目库记录路径，用于去重
            Hashtable bilio_recpath_table = new Hashtable();

            // 装入浏览格式
            for (; ; )
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null && stop.State != 0)
                {
                    strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...";
                    return -1;
                }


                lRet = channel.GetSearchResult(
                    stop,
                    null,   // strResultSetName
                    lStart,
                    lCount,
                    strStyle, // bOutputKeyCount == true ? "keycount" : "id,cols",
                    "zh",
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，" + strError;
                    return -1;
                }

                if (lRet == 0)
                {
                    return 0;
                }

                // 处理浏览结果

                for (int i = 0; i < searchresults.Length; i++)
                {
                    DigitalPlatform.LibraryClient.localhost.Record searchresult = searchresults[i];

                    if (bOutputBiblioRecPath == true)
                    {
                        string strItemDbName = Global.GetDbName(searchresult.Path);
                        string strBiblioDbName = (string)dbname_table[strItemDbName];
                        if (string.IsNullOrEmpty(strBiblioDbName) == true)
                        {
                            strBiblioDbName = main_form.GetBiblioDbNameFromItemDbName(strItemDbName);
                            dbname_table[strItemDbName] = strBiblioDbName;
                        }

                        string strBiblioRecPath = strBiblioDbName + "/" + searchresult.Cols[0];

                        if (bilio_recpath_table.ContainsKey(strBiblioRecPath) == false)
                        {
                            results.Add(strBiblioRecPath);
                            bilio_recpath_table[strBiblioRecPath] = true;
                        }
                    }
                    else if (bOutputItemRecPath == true)
                        results.Add(searchresult.Path);
                }

                lStart += searchresults.Length;
                lCount -= searchresults.Length;

                stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                if (lStart >= lHitCount || lCount <= 0)
                    break;
            }

            return 0;
        }

        /// <summary>
        /// 执行一次检索
        /// </summary>
        /// <param name="bOutputKeyCount">是否要输出为 key+count 形态</param>
        /// <param name="bOutputKeyID">是否为 keyid 形态</param>
        /// <param name="input_query">检索式</param>
        /// <param name="bClearList">是否要在检索开始时清空浏览列表</param>
        /// <returns>-1:出错 0:中断 1:正常结束</returns>
        public int DoSearch(bool bOutputKeyCount,
            bool bOutputKeyID,
            ItemQueryParam input_query,
            bool bClearList = true)
        {
            string strError = "";
            int nRet = 0;

            if (bOutputKeyCount == true
                && bOutputKeyID == true)
            {
                strError = "bOutputKeyCount和bOutputKeyID不能同时为true";
                goto ERROR1;
            }

            bool bQuickLoad = false;    // 是否快速装入

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                bQuickLoad = true;

            /*
            bool bOutputKeyCount = false;
            if (Control.ModifierKeys == Keys.Control)
                bOutputKeyCount = true;
             * */

            if (input_query != null)
            {
                QueryToPanel(input_query, bClearList);
            }

            // 记忆下检索式
            this.m_bFirstColumnIsKey = bOutputKeyID;
            this.ClearListViewPropertyCache();

            ItemQueryParam query = PanelToQuery();
            PushQuery(query);

            if (bClearList == true)
            {
                ClearListViewItems();
                m_tableSummaryColIndex.Clear();
            }

            EnableControls(false);
            this.label_message.Text = "";

            stop.Style = StopStyle.None;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            try
            {
                string strMatchStyle = "";

                strMatchStyle = GetCurrentMatchStyle();

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

                // string strBrowseStyle = "id, cols";
                string strOutputStyle = "";
                if (bOutputKeyCount == true)
                {
                    strOutputStyle = "keycount";
                    //strBrowseStyle = "keycount";
                }
                else if (bOutputKeyID == true)
                {
                    strOutputStyle = "keyid";
                    //strBrowseStyle = "keyid,key,id,cols";
                }

                long lRet = 0;

                if (this.DbType == "item")
                {
                    lRet = Channel.SearchItem(stop,
                        this.comboBox_entityDbName.Text, // "<all>",
                        this.textBox_queryWord.Text,
                        this.MaxSearchResultCount,
                        this.comboBox_from.Text,
                        strMatchStyle, // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
                        this.Lang,
                        null,   // strResultSetName
                        "",    // strSearchStyle
                        strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                        out strError);
                }
                else if (this.DbType == "comment")
                {
                    lRet = Channel.SearchComment(stop,
                        this.comboBox_entityDbName.Text,
                        this.textBox_queryWord.Text,
                        this.MaxSearchResultCount,
                        this.comboBox_from.Text,
                        strMatchStyle,
                        this.Lang,
                        null,
                        "",
                        strOutputStyle,
                        out strError);
                }
                else if (this.DbType == "order")
                {
                    lRet = Channel.SearchOrder(stop,
                        this.comboBox_entityDbName.Text,
                        this.textBox_queryWord.Text,
                        this.MaxSearchResultCount,
                        this.comboBox_from.Text,
                        strMatchStyle,
                        this.Lang,
                        null,
                        "",
                        strOutputStyle,
                        out strError);
                }
                else if (this.DbType == "issue")
                {
                    lRet = Channel.SearchIssue(stop,
                        this.comboBox_entityDbName.Text,
                        this.textBox_queryWord.Text,
                        this.MaxSearchResultCount,
                        this.comboBox_from.Text,
                        strMatchStyle,
                        this.Lang,
                        null,
                        "",
                        strOutputStyle,
                        out strError);
                }
                else if (this.DbType == "arrive")
                {
#if NO
                    string strArrivedDbName = "";
                    // return:
                    //      -1  出错
                    //      0   没有配置
                    //      1   找到
                    nRet = GetArrivedDbName(false, out strArrivedDbName, out strError);
                    if (nRet == -1 || nRet == 0)
                        goto ERROR1;
#endif
                    if (string.IsNullOrEmpty(this.MainForm.ArrivedDbName) == true)
                    {
                        strError = "当前服务器尚未配置预约到书库名";
                        goto ERROR1;
                    }

                    string strQueryXml = "<target list='" + this.MainForm.ArrivedDbName + ":" + this.comboBox_from.Text + "'><item><word>"
        + StringUtil.GetXmlStringSimple(this.textBox_queryWord.Text)
        + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>"
                    + this.MaxSearchResultCount + "</maxCount></item><lang>" + this.Lang + "</lang></target>";
                    // strOutputStyle ?
                    lRet = Channel.Search(stop,
                        strQueryXml,
                        "",
                        strOutputStyle,
                        out strError);
                }
                else
                    throw new Exception("未知的 DbType '" + this.DbType + "'");

                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                // return:
                //      -1  出错
                //      0   用户中断
                //      1   正常完成
                nRet = FillBrowseList(
                    query,
                    lHitCount,
                    bOutputKeyCount,
                    bOutputKeyID,
                    bQuickLoad,
                    out strError);
                if (nRet == 0)
                    return 0;
                if (nRet == -1)
                    this.ShowMessage("填充浏览列时出错: " + strError, "red", true);

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
                this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条，已全部装入";
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                EnableControls(true);
            }

            return 1;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        #region 预约到书库相关

        static string[] arrive_froms = {
                "册条码号",
                "读者证条码号",
                "册参考ID",
                "状态",
                "__id"};

#if NO
        /// <summary>
        /// 获得预约到书库的库名
        /// 注意使用此属性的时候，会 BeginLoop，如果外层曾经 BeginLoop，会打乱 Stop 的状态。以后这里可以改进为使用 Pooling Channel 就好了
        /// </summary>
        string ArrivedDbName
        {
            get
            {

                string strError = "";
                string strArrivedDbName = "";
                // return:
                //      -1  出错
                //      0   没有配置
                //      1   找到
                int nRet = GetArrivedDbName(true, out strArrivedDbName, out strError);
                if (nRet == -1 || nRet == 0)
                    throw new Exception(strError);

                return strArrivedDbName;
            }
        }

        string _arrivedDbName = "";

        // return:
        //      -1  出错
        //      0   没有配置
        //      1   找到
        int GetArrivedDbName(
            bool bBeginLoop,
            out string strDbName,
            out string strError)
        {
            strError = "";
            strDbName = "";

            if (string.IsNullOrEmpty(this._arrivedDbName) == false)
            {
                strDbName = this._arrivedDbName;
                return 1;
            }

            if (bBeginLoop == true)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.SetMessage("正在获取预约到书库名 ...");
                stop.BeginLoop();
            }

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "arrived",
                    "dbname",
                    out strDbName,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0
                    || string.IsNullOrEmpty(strDbName) == true)
                {
                    strError = "预约到书库名没有配置";
                    return 0;   // not found
                }
                this._arrivedDbName = strDbName;
                return 1;
            }
            finally
            {
                if (bBeginLoop)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }
        }
#endif

        #endregion

        // return:
        //      -1  出错
        //      0   用户中断或者未命中
        //      1   正常完成
        int FillBrowseList(
            ItemQueryParam query,
            long lHitCount,
            bool bOutputKeyCount,
            bool bOutputKeyID,
            bool bQuickLoad,
            out string strError)
        {
            strError = "";

            bool bAccessBiblioSummaryDenied = false;

            string strBrowseStyle = "id, cols";
            //string strOutputStyle = "";
            if (bOutputKeyCount == true)
            {
                //strOutputStyle = "keycount";
                strBrowseStyle = "keycount";
            }
            else if (bOutputKeyID == true)
            {
                //strOutputStyle = "keyid";
                strBrowseStyle = "keyid,key,id,cols";
            }
            //
            this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条";
            stop.SetProgressRange(0, lHitCount);
            stop.Style = StopStyle.EnableHalfStop;

            bool bSelectFirstLine = false;
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
                    // MessageBox.Show(this, "用户中断");
                    this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...";
                    return 0;
                }

                bool bTempQuickLoad = bQuickLoad;

                if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                    bTempQuickLoad = true;

                string strTempBrowseStyle = strBrowseStyle;
                if (bTempQuickLoad)
                    StringUtil.RemoveFromInList("cols", false, ref strTempBrowseStyle);

                long lRet = Channel.GetSearchResult(
                    stop,
                    null,   // strResultSetName
                    lStart,
                    lCount,
                    strTempBrowseStyle, // bOutputKeyCount == true ? "keycount" : "id,cols",
                    this.Lang,
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，" + strError;
                    goto ERROR1;
                }

                if (lRet == 0)
                {
                    MessageBox.Show(this, "未命中");
                    return 0;
                }

                // 处理浏览结果
                this.listView_records.BeginUpdate();
                try
                {
                    List<ListViewItem> items = new List<ListViewItem>();
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        ListViewItem item = null;

                        DigitalPlatform.LibraryClient.localhost.Record searchresult = searchresults[i];

                        if (bOutputKeyCount == false
                            && bOutputKeyID == false)
                        {
                            if (bPushFillingBrowse == true)
                                item = Global.InsertNewLine(
                                    this.listView_records,
                                    searchresult.Path,
                                    this.m_bBiblioSummaryColumn == true ? Global.InsertBlankColumn(searchresult.Cols) : searchresult.Cols);
                            else
                                item = Global.AppendNewLine(
                                    this.listView_records,
                                    searchresult.Path,
                                    this.m_bBiblioSummaryColumn == true ? Global.InsertBlankColumn(searchresult.Cols) : searchresult.Cols);
                        }
                        else if (bOutputKeyCount == true)
                        {
                            // 输出keys
                            if (searchresult.Cols == null)
                            {
                                strError = "要使用获取检索点功能，请将 dp2Library 应用服务器和 dp2Kernel 数据库内核升级到最新版本";
                                goto ERROR1;
                            }
                            string[] cols = new string[(searchresult.Cols == null ? 0 : searchresult.Cols.Length) + 1];
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
                        }
                        else if (bOutputKeyID == true)
                        {
                            if (searchresult.Cols == null)
                            {
                                strError = "要使用带有检索点的检索功能，请将 dp2Library 应用服务器和 dp2Kernel 数据库内核升级到最新版本";
                                goto ERROR1;
                            }


#if NO
                                string[] cols = new string[(searchresult.Cols == null ? 0 : searchresult.Cols.Length) + 1];
                                cols[0] = LibraryChannel.BuildDisplayKeyString(searchresult.Keys);
                                if (cols.Length > 1)
                                    Array.Copy(searchresult.Cols, 0, cols, 1, cols.Length - 1);
#endif
                            string[] cols = this.m_bBiblioSummaryColumn == true ? Global.InsertBlankColumn(searchresult.Cols, 2) : searchresult.Cols;
                            cols[0] = LibraryChannel.BuildDisplayKeyString(searchresult.Keys);

                            if (bPushFillingBrowse == true)
                                item = Global.InsertNewLine(
                                    this.listView_records,
                                    searchresult.Path,
                                    cols);
                            else
                                item = Global.AppendNewLine(
                                    this.listView_records,
                                    searchresult.Path,
                                    cols);
                            item.Tag = query;
                        }

                        query.Items.Add(item);
                        items.Add(item);
                        stop.SetProgressValue(lStart + i);
                    }

                    if (bOutputKeyCount == false
                        && bAccessBiblioSummaryDenied == false
                        && bTempQuickLoad == false)
                    {
                        // return:
                        //      -2  获得书目摘要的权限不够
                        //      -1  出错
                        //      0   用户中断
                        //      1   完成
                        int nRet = _fillBiblioSummaryColumn(items,
                            0,
                            false,
                            true,   // false,  // bAutoSearch
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == -2)
                            bAccessBiblioSummaryDenied = true;

                        if (nRet == 0)
                        {
                            this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...";
                            return 0;
                        }
                    }
                }
                finally
                {
                    this.listView_records.EndUpdate();
                }

                if (bSelectFirstLine == false && this.listView_records.Items.Count > 0)
                {
                    if (this.listView_records.SelectedItems.Count == 0)
                        this.listView_records.Items[0].Selected = true;
                    bSelectFirstLine = true;
                }

                lStart += searchresults.Length;
                lCount -= searchresults.Length;

                stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                if (lStart >= lHitCount || lCount <= 0)
                    break;
                stop.SetProgressValue(lStart);
            }

            if (bAccessBiblioSummaryDenied == true)
                MessageBox.Show(this, "当前用户不具备获取书目摘要的权限");

            return 1;
        ERROR1:
            return -1;
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.button_search.Enabled = bEnable;
            this.comboBox_from.Enabled = bEnable;

            // 2008/11/21 
            this.comboBox_entityDbName.Enabled = bEnable;
            this.comboBox_matchStyle.Enabled = bEnable;

            this.toolStrip_search.Enabled = bEnable;

            if (this.comboBox_matchStyle.Text == "空值")
            {
                this.textBox_queryWord.Enabled = false;
            }
            else
            {
                this.textBox_queryWord.Enabled = bEnable;
            }

            this.dp2QueryControl1.Enabled = bEnable;
        }

        /*
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }*/


        private void textBox_queryWord_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_search;
        }

        private void listView_records_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = null;
        }


#if NOOOOOOOOOOOOOOOOOOOOOOOOOO

        void menu_loadToItemInfoFormByRecPath_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要装入册信息窗的册事项");
                return;
            }

            string strRecPath = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);

            ItemInfoForm form = null;

            if (this.LoadToExistDetailWindow == true)
            {
                form = MainForm.TopItemInfoForm;
                if (form != null)
                    form.Activate();
            }

            if (form == null)
            {
                form = new ItemInfoForm();

                form.MdiParent = this.MainForm;

                form.MainForm = this.MainForm;
                form.Show();
            }

            form.LoadRecordByRecPath(strRecPath);
        }

        void menu_loadToItemInfoFormByBarcode_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要装入册信息窗的册事项");
                return;
            }

            string strBarcode = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);

            ItemInfoForm form = null;

            if (this.LoadToExistDetailWindow == true)
            {
                form = MainForm.TopItemInfoForm;
                if (form != null)
                    form.Activate();
            }

            if (form == null)
            {
                form = new ItemInfoForm();

                form.MdiParent = this.MainForm;

                form.MainForm = this.MainForm;
                form.Show();
            }

            form.LoadRecord(strBarcode);
        }

        // 装入种册窗，用册记录路径
        // 自动判断该打开新窗口还是占用已有的窗口
        void menu_loadToEntityFormByRecPath_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要装入种册窗的册事项");
                return;
            }

            string strItemRecPath = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);

            EntityForm form = null;

            if (this.LoadToExistDetailWindow == true)
            {
                form = MainForm.TopEntityForm;
                if (form != null)
                    form.Activate();
            }

            if (form == null)
            {
                form = new EntityForm();

                form.MdiParent = this.MainForm;

                form.MainForm = this.MainForm;
                form.Show();
            }

            // parameters:
            //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            form.LoadItemByRecPath(strItemRecPath, false);
        }

        // 装入种册窗，用册条码号
        void menu_loadToEntityFormByBarcode_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要装入种册窗的册事项");
                return;
            }

            string strBarcode = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);

            EntityForm form = null;

            if (this.LoadToExistDetailWindow == true)
            {
                form = MainForm.TopEntityForm;
                if (form != null)
                    form.Activate();
            }

            if (form == null)
            {
                form = new EntityForm();

                form.MdiParent = this.MainForm;

                form.MainForm = this.MainForm;
                form.Show();
            }

            // 装载一个册，连带装入种
            // parameters:
            //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            form.LoadItem(strBarcode, false);
        }

#endif

        /// <summary>
        /// 获得配置参数：当在浏览框中双击鼠标左键或者回车打开一个窗口观察当前行的时候，是否优先装入已经打开的种册窗/册窗/订购窗/期窗/评注窗?
        /// 这个配置参数是对所有类型的查询窗窗都适用的
        /// </summary>
        public bool LoadToExistWindow
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "all_search_form",
                    "load_to_exist_detailwindow",
                    true);
            }
        }

        /// <summary>
        /// 获得配置参数：是否装入册窗/订购窗/期窗/评注窗(而不是种册窗)?
        /// 这个配置参数仅对 ItemSearchForm 适用
        /// </summary>
        public bool LoadToItemWindow
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "item_search_form",
                    "load_to_itemwindow",
                    false);
            }
            set
            {
                this.MainForm.AppInfo.SetBoolean(
                    "item_search_form",
                    "load_to_itemwindow",
                    value);
            }

        }

        private void listView_records_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要操作的事项");
                return;
            }

            string strFirstColumn = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);

            if (String.IsNullOrEmpty(strFirstColumn) == false)
            {
                string strOpenStyle = "new";
                if (this.LoadToExistWindow == true)
                    strOpenStyle = "exist";

                bool bLoadToItemWindow = this.LoadToItemWindow;

                if (bLoadToItemWindow == true)
                {
                    LoadRecord("ItemInfoForm",
                        "recpath",
                        strOpenStyle);
                    return;
                }

                // 装入种册窗/实体窗，用册条码号/记录路径
                // parameters:
                //      strTargetFormType   目标窗口类型 "EntityForm" "ItemInfoForm"
                //      strIdType   标识类型 "barcode" "recpath"
                //      strOpenType 打开窗口的方式 "new" "exist"
                LoadRecord("EntityForm",
                    "recpath",
                    strOpenStyle);
            }
            else
            {
                ItemQueryParam query = (ItemQueryParam)this.listView_records.SelectedItems[0].Tag;
                Debug.Assert(query != null, "");

                this.textBox_queryWord.Text = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);
                if (query != null)
                {
                    this.comboBox_entityDbName.Text = query.DbNames;
                    this.comboBox_from.Text = query.From;
                }

                if (this.textBox_queryWord.Text == "")    // 2009/8/6 
                    this.comboBox_matchStyle.Text = "空值";
                else
                    this.comboBox_matchStyle.Text = "精确一致";

                DoSearch(false, false, null);
            }
        }

        private void ItemSearchForm_Activated(object sender, EventArgs e)
        {
            // this.MainForm.stopManager.Active(this.stop);

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            // this.MainForm.MenuItem_font.Enabled = false;
            // this.MainForm.MenuItem_restoreDefaultFont.Enabled = false;

            this.MainForm.toolStripDropDownButton_barcodeLoadStyle.Enabled = true;
            this.MainForm.toolStripTextBox_barcode.Enabled = true;
        }

        // 装入种册窗/实体窗，用册条码号/记录路径
        // parameters:
        //      strTargetFormType   目标窗口类型 "EntityForm" "ItemInfoForm"
        //      strIdType   标识类型 "barcode" "recpath"
        //      strOpenType 打开窗口的方式 "new" "exist"
        void LoadRecord(string strTargetFormType,
            string strIdType,
            string strOpenType)
        {
            string strTargetFormName = "种册窗";

            if (strTargetFormType == "ItemInfoForm")
                strTargetFormName = "实体窗";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要装入" + strTargetFormName + "的行");
                return;
            }

            string strBarcodeOrRecPath = "";

            if (strIdType == "barcode")
            {
                // barcode
                // strBarcodeOrRecPath = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);
                string strError = "";
                // 根据 ListViewItem 对象，获得册条码号列的内容
                int nRet = GetItemBarcodeOrRefID(
                    this.listView_records.SelectedItems[0],
                    true,
                    out strBarcodeOrRecPath,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }
            else
            {
                Debug.Assert(strIdType == "recpath", "");
                // recpath
                strBarcodeOrRecPath = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);
            }

            if (strTargetFormType == "EntityForm")
            {
                EntityForm form = null;

                if (strOpenType == "exist")
                {
                    form = MainForm.GetTopChildWindow<EntityForm>();
                    if (form != null)
                        Global.Activate(form);
                }
                else
                {
                    Debug.Assert(strOpenType == "new", "");
                }

                if (form == null)
                {
                    form = new EntityForm();

                    form.MdiParent = this.MainForm;

                    form.MainForm = this.MainForm;
                    form.Show();
                }

                if (strIdType == "barcode")
                {
                    // 装载一个册，连带装入种
                    // parameters:
                    //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    form.LoadItemByBarcode(strBarcodeOrRecPath, false);
                }
                else
                {
                    Debug.Assert(strIdType == "recpath", "");

                    if (this.DbType == "item")
                    {
                        // parameters:
                        //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   found
                        form.LoadItemByRecPath(strBarcodeOrRecPath, false);
                    }
                    else if (this.DbType == "comment")
                        form.LoadCommentByRecPath(strBarcodeOrRecPath, false);
                    else if (this.DbType == "order")
                        form.LoadOrderByRecPath(strBarcodeOrRecPath, false);
                    else if (this.DbType == "issue")
                        form.LoadIssueByRecPath(strBarcodeOrRecPath, false);
                    else
                        throw new Exception("未知的DbType '" + this.DbType + "'");
                }
            }
            else
            {
                Debug.Assert(strTargetFormType == "ItemInfoForm", "");

                ItemInfoForm form = null;

                if (strOpenType == "exist")
                {
                    form = MainForm.GetTopChildWindow<ItemInfoForm>();
                    if (form != null)
                        Global.Activate(form);
                }
                else
                {
                    Debug.Assert(strOpenType == "new", "");
                }

                if (form == null)
                {
                    form = new ItemInfoForm();

                    form.MdiParent = this.MainForm;

                    form.MainForm = this.MainForm;
                    form.Show();
                }

                if (this.DbType == "arrive")
                    form.DbType = "item";
                else
                    form.DbType = this.DbType;

                if (strIdType == "barcode")
                {
                    Debug.Assert(this.DbType == "item" || this.DbType == "arrive", "");
                    form.LoadRecord(strBarcodeOrRecPath);
                }
                else
                {
                    Debug.Assert(strIdType == "recpath", "");

                    // TODO: 需要改造为适应多种记录
                    form.LoadRecordByRecPath(strBarcodeOrRecPath, "");
                }
            }
        }

        void menu_itemInfoForm_recPath_newly_Click(object sender, EventArgs e)
        {
            LoadRecord("ItemInfoForm",
                "recpath",
                "new");
            this.LoadToItemWindow = true;
        }

        void menu_itemInfoForm_barcode_newly_Click(object sender, EventArgs e)
        {
            LoadRecord("ItemInfoForm",
                "barcode",
                "new");
            this.LoadToItemWindow = true;
        }

        void menu_entityForm_recPath_newly_Click(object sender, EventArgs e)
        {
            LoadRecord("EntityForm",
                "recpath",
                "new");
            this.LoadToItemWindow = false;
        }

        void menu_entityForm_barcode_newly_Click(object sender, EventArgs e)
        {
            LoadRecord("EntityForm",
                "barcode",
                "new");
            this.LoadToItemWindow = false;
        }

        void menu_itemInfoForm_recPath_exist_Click(object sender, EventArgs e)
        {
            LoadRecord("ItemInfoForm",
                "recpath",
                "exist");
            this.LoadToItemWindow = true;
        }

        void menu_itemInfoForm_barcode_exist_Click(object sender, EventArgs e)
        {
            LoadRecord("ItemInfoForm",
                "barcode",
                "exist");
            this.LoadToItemWindow = true;
        }

        void menu_entityForm_recPath_exist_Click(object sender, EventArgs e)
        {
            LoadRecord("EntityForm",
                "recpath",
                "exist");
            this.LoadToItemWindow = false;
        }

        void menu_entityForm_barcode_exist_Click(object sender, EventArgs e)
        {
            LoadRecord("EntityForm",
                "barcode",
                "exist");
            this.LoadToItemWindow = false;
        }

        int GetItemBarcodeOrRefID(ListViewItem item,
            bool bWarning,
            out string strBarcode,
            out string strError)
        {
            // strBarcode = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);
            int nRet = GetItemBarcode(
item,
bWarning,
out strBarcode,
out strError);
            if (nRet == -1)
                return -1;
            // 2015/6/14
            // 如果册条码号为空，则尝试用 参考ID
            if (string.IsNullOrEmpty(strBarcode) == true)
            {
                // return:
                //      -2  没有找到列 type
                //      -1  出错
                //      >=0 列号
                nRet = GetColumnText(item,
"item_refid",
out strBarcode,
out strError);
                if (nRet >= 0 && string.IsNullOrEmpty(strBarcode) == false)
                {
                    strBarcode = "@refID:" + strBarcode;
                }
            }

            return 0;
        }

        private void listView_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSelectedItemCount = this.listView_records.SelectedItems.Count;
            bool bSelected = false;
            string strFirstColumn = "";
            if (nSelectedItemCount > 0)
            {
                bSelected = true;
                strFirstColumn = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);
            }

            if (String.IsNullOrEmpty(strFirstColumn) == false)
            {
                string strRecPath = "";
                if (bSelected == true)
                {
                    if (this.DbType != "arrive")
                        strRecPath = this.listView_records.SelectedItems[0].Text;
                }

                string strOpenStyle = "新开的";
                if (this.LoadToExistWindow == true)
                    strOpenStyle = "已打开的";

                bool bLoadToItemWindow = this.LoadToItemWindow;

                menuItem = new MenuItem("打开 [根据" + this.DbTypeCaption + "记录路径 '" + strRecPath + "' 装入到" + strOpenStyle
                    + (bLoadToItemWindow == true ? DbTypeCaption + "窗" : "种册窗")
                    + "](&O)");
                menuItem.DefaultItem = true;
                menuItem.Click += new System.EventHandler(this.listView_records_DoubleClick);
                if (String.IsNullOrEmpty(strRecPath) == true)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);

                string strBarcode = "";
                if (bSelected == true)
                {
                    string strError = "";
                    // strBarcode = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);
                    int nRet = GetItemBarcodeOrRefID(
    this.listView_records.SelectedItems[0],
    false,
    out strBarcode,
    out strError);
                }

                bool bExistEntityForm = (this.MainForm.GetTopChildWindow<EntityForm>() != null);
                bool bExistItemInfoForm = (this.MainForm.GetTopChildWindow<ItemInfoForm>() != null);

                //
                menuItem = new MenuItem("打开方式(&T)");
                contextMenu.MenuItems.Add(menuItem);

                // 第一级子菜单

                strOpenStyle = "新开的";

                // 到册窗，记录路径
                MenuItem subMenuItem = new MenuItem("装入" + strOpenStyle + this.DbTypeCaption + "窗，根据记录路径 '" + strRecPath + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_itemInfoForm_recPath_newly_Click);
                if (String.IsNullOrEmpty(strRecPath) == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 到册窗，条码
                // if (this.DbType == "item")
                if (string.IsNullOrEmpty(strBarcode) == false)
                {
                    subMenuItem = new MenuItem("装入" + strOpenStyle + this.DbTypeCaption + "窗，根据册条码号 '" + strBarcode + "'");
                    subMenuItem.Click += new System.EventHandler(this.menu_itemInfoForm_barcode_newly_Click);
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);
                }

                // 到种册窗，记录路径
                subMenuItem = new MenuItem("装入" + strOpenStyle + "种册窗，根据记录路径 '" + strRecPath + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_entityForm_recPath_newly_Click);
                if (String.IsNullOrEmpty(strRecPath) == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 到种册窗，条码
                // if (this.DbType == "item")
                if (string.IsNullOrEmpty(strBarcode) == false)
                {
                    subMenuItem = new MenuItem("装入" + strOpenStyle + "种册窗，根据册条码号 '" + strBarcode + "'");
                    subMenuItem.Click += new System.EventHandler(this.menu_entityForm_barcode_newly_Click);
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);
                }

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                strOpenStyle = "已打开的";

                // 到册窗，记录路径
                subMenuItem = new MenuItem("装入" + strOpenStyle + this.DbTypeCaption + "窗，根据记录路径 '" + strRecPath + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_itemInfoForm_recPath_exist_Click);
                if (String.IsNullOrEmpty(strRecPath) == true
                    || bExistItemInfoForm == false)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 到册窗，条码
                //if (this.DbType == "item")
                if (string.IsNullOrEmpty(strBarcode) == false)
                {
                    subMenuItem = new MenuItem("装入" + strOpenStyle + this.DbTypeCaption + "窗，根据册条码号 '" + strBarcode + "'");
                    subMenuItem.Click += new System.EventHandler(this.menu_itemInfoForm_barcode_exist_Click);
                    if (String.IsNullOrEmpty(strBarcode) == true
                        || bExistItemInfoForm == false)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);
                }

                // 到种册窗，记录路径
                subMenuItem = new MenuItem("装入" + strOpenStyle + "种册窗，根据记录路径 '" + strRecPath + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_entityForm_recPath_exist_Click);
                if (String.IsNullOrEmpty(strRecPath) == true
                    || bExistEntityForm == false)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 到种册窗，条码
                //                if (this.DbType == "item")
                if (string.IsNullOrEmpty(strBarcode) == false)
                {
                    subMenuItem = new MenuItem("装入" + strOpenStyle + "种册窗，根据册条码号 '" + strBarcode + "'");
                    subMenuItem.Click += new System.EventHandler(this.menu_entityForm_barcode_exist_Click);
                    if (String.IsNullOrEmpty(strBarcode) == true
                        || bExistEntityForm == false)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);
                }

            }

            if (String.IsNullOrEmpty(strFirstColumn) == true
                && this.listView_records.SelectedItems.Count > 0)
            {
                string strKey = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);

                menuItem = new MenuItem("检索 '" + strKey + "' (&S)");
                menuItem.DefaultItem = true;
                menuItem.Click += new System.EventHandler(this.listView_records_DoubleClick);
                contextMenu.MenuItems.Add(menuItem);

                menuItem = new MenuItem("在新开的" + this.DbTypeCaption + "查询窗内 检索 '" + strKey + "' (&N)");
                menuItem.Click += new System.EventHandler(this.listView_searchKeysAtNewWindow_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            // // //

#if NOOOOOOOOOOO
            menuItem = new MenuItem("根据册条码号 '" + strBarcode + "' 装入到"+strOpenStyle+"种册窗(&E)");
            menuItem.Click += new System.EventHandler(this.menu_loadToEntityFormByBarcode_Click);
            if (String.IsNullOrEmpty(strBarcode) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("根据册记录路径 '" + strRecPath + "' 装入到"+strOpenStyle+"实体窗(&P)");
            menuItem.Click += new System.EventHandler(this.menu_loadToItemInfoFormByRecPath_Click);
            if (String.IsNullOrEmpty(strRecPath) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("根据册条码号 '" + strBarcode + "' 装入到"+strOpenStyle+"实体窗(&B)");
            menuItem.Click += new System.EventHandler(this.menu_loadToItemInfoFormByBarcode_Click);
            if (String.IsNullOrEmpty(strBarcode) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

#endif

            /*
            int nPathItemCount = 0;
            int nKeyItemCount = 0;
            GetSelectedItemCount(out nPathItemCount,
                out nKeyItemCount);
             * */
            int nPathItemCount = nSelectedItemCount;
            if (nSelectedItemCount > 0 && String.IsNullOrEmpty(strFirstColumn) == true)
                nPathItemCount = -1;    // 表示不清楚

            if (contextMenu.MenuItems.Count > 0)
            {
                // ---
                menuItem = new MenuItem("-");
                contextMenu.MenuItems.Add(menuItem);
            }

            menuItem = new MenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutToClipboard_Click);
            if (nSelectedItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyToClipboard_Click);
            if (nSelectedItemCount == 0)
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


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            bool bLooping = (stop != null && stop.State == 0);    // 0 表示正在处理

            {
                menuItem = new MenuItem("功能(&F)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = null;

                if (this.DbType == "item")
                {
#if NO
                    subMenuItem = new MenuItem("快速修改册记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&Q)");
                    subMenuItem.Click += new System.EventHandler(this.menu_quickChangeRecords_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);
#endif

                    subMenuItem = new MenuItem("创建索取号 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&C)");
                    subMenuItem.Click += new System.EventHandler(this.menu_createCallNumber_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);


                    subMenuItem = new MenuItem("校验册记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&V)");
                    subMenuItem.Click += new System.EventHandler(this.menu_verifyItemRecord_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);


                    // ---
                    subMenuItem = new MenuItem("-");
                    menuItem.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("模拟借书 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&B)");
                    subMenuItem.Click += new System.EventHandler(this.menu_borrow_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("模拟还书 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&R)");
                    subMenuItem.Click += new System.EventHandler(this.menu_return_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("模拟盘点 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&R)");
                    subMenuItem.Click += new System.EventHandler(this.menu_inventory_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);

                }

                if (this.DbType == "order")
                {
                    subMenuItem = new MenuItem("打印订单 (&O)");
                    subMenuItem.Click += new System.EventHandler(this.menu_printOrderForm_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("打印订单[验收情况] (&O)");
                    subMenuItem.Click += new System.EventHandler(this.menu_printOrderFormAccept_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("打印验收单 (&A)");
                    subMenuItem.Click += new System.EventHandler(this.menu_printAcceptForm_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("打印催询单 (&C)");
                    subMenuItem.Click += new System.EventHandler(this.menu_printClaimForm_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);

                }
            }


            // if (this.DbType == "item" || this.DbType == "order")
            {
                menuItem = new MenuItem("批处理(&B)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = null;

                {
                    subMenuItem = new MenuItem("快速修改" + this.DbTypeCaption + "记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&Q)");
                    subMenuItem.Click += new System.EventHandler(this.menu_quickChangeItemRecords_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);
                }

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("执行 C# 脚本 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&M)");
                subMenuItem.Click += new System.EventHandler(this.menu_quickMarcQueryRecords_Click);
                if (this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

#if NO
                subMenuItem = new MenuItem("内存中接受修改 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&C)");
                subMenuItem.Click += new System.EventHandler(this.menu_acceptSelectedChangedRecords_Click);
                if (this.m_nChangedCount == 0 || this.listView_records.SelectedItems.Count == 0 || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
#endif

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

                subMenuItem = new MenuItem("删除" + this.DbTypeCaption + "记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&D)");
                subMenuItem.Click += new System.EventHandler(this.menu_deleteSelectedRecords_Click);
                if (this.listView_records.SelectedItems.Count == 0
                    || this.InSearching == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                menuItem = null;
            }


            MenuItem menuItemExport = new MenuItem("导出(&X)");
            contextMenu.MenuItems.Add(menuItemExport);

            {
                MenuItem subMenuItem = null;

                if (this.DbType == "item")
                {
                    subMenuItem = new MenuItem("到条码号文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&B)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_exportBarcodeFile_Click);
                    if (nPathItemCount == 0)
                        subMenuItem.Enabled = false;
                    menuItemExport.MenuItems.Add(subMenuItem);
                }

                subMenuItem = new MenuItem("到记录路径文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&S)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToRecordPathFile_Click);
                if (nPathItemCount == 0)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("到文本文件 [" + nSelectedItemCount.ToString() + "] (&T)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportTextFile_Click);
                if (nSelectedItemCount == 0)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("到 Excel 文件 [" + nSelectedItemCount.ToString() + "] (&E)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportExcelFile_Click);
                if (nSelectedItemCount == 0)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);

                if (this.DbType == "item")
                {
                    subMenuItem = new MenuItem("到书目转储文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&B)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_exportBiblioDumpFile_Click);
                    if (nPathItemCount == 0)
                        subMenuItem.Enabled = false;
                    menuItemExport.MenuItems.Add(subMenuItem);
                }

                // ---
                subMenuItem = new MenuItem("-");
                menuItemExport.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("装入书目查询窗，将所从属的书目记录 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&B)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportToBiblioSearchForm_Click);
                if (nPathItemCount == 0)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);


                subMenuItem = new MenuItem("将所从属的书目记录路径归并后导出到(书目库)记录路径文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&B)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToBiblioRecordPathFile_Click);
                if (nPathItemCount == 0)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("将所从属的书目记录导出到 MARC(ISO2709)文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&M)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveBiblioRecordToMarcFile_Click);
                if (nPathItemCount == 0)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);

                if (this.DbType == "order")
                {
                    // ---
                    subMenuItem = new MenuItem("-");
                    menuItemExport.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("将关联的已验收册记录路径导出到(册)记录路径文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&I)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_saveToAcceptItemRecordPathFile_Click);
                    if (nPathItemCount == 0)
                        subMenuItem.Enabled = false;
                    menuItemExport.MenuItems.Add(subMenuItem);
                }

                if (this.DbType == "item")
                {
                    // ---
                    subMenuItem = new MenuItem("-");

                    menuItemExport.MenuItems.Add(subMenuItem); subMenuItem = new MenuItem("装入读者查询窗，将借者记录 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&B)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_exportToReaderSearchForm_Click);
                    if (nPathItemCount == 0)
                        subMenuItem.Enabled = false;
                    menuItemExport.MenuItems.Add(subMenuItem);

                    menuItemExport.MenuItems.Add(subMenuItem); subMenuItem = new MenuItem("输出到 Excel 文件，将借者记录 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&R)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_exportToReaderExcelFile_Click);
                    if (nPathItemCount == 0)
                        subMenuItem.Enabled = false;
                    menuItemExport.MenuItems.Add(subMenuItem);

                }
            }

            MenuItem menuItemImport = new MenuItem("导入(&I)");
            contextMenu.MenuItems.Add(menuItemImport);

            {
                MenuItem subMenuItem = new MenuItem("从记录路径文件中导入(&I)...");
                subMenuItem.Click += new System.EventHandler(this.menu_importFromRecPathFile_Click);
                menuItemImport.MenuItems.Add(subMenuItem);

                if (this.DbType == "item")
                {
                    // ---
                    subMenuItem = new MenuItem("-");
                    menuItemImport.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("从条码号文件中导入(&R)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_importFromBarcodeFile_Click);
                    menuItemImport.MenuItems.Add(subMenuItem);
                }
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("移除 [" + nSelectedItemCount.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_removeSelectedItems_Click);
            if (nSelectedItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("清除命中列表(&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearList_Click);
            if (nSelectedItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新 [" + nSelectedItemCount.ToString() + "] (&R)");
            menuItem.Click += new System.EventHandler(this.menu_refreshSelectedItems_Click);
            if (nSelectedItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新书目摘要 [" + nSelectedItemCount.ToString() + "] (&R)");
            menuItem.Click += new System.EventHandler(this.menu_refreshSelectedItemsBiblioSummary_Click);
            if (nSelectedItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
        }

        void menu_verifyItemRecord_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = VerifyItemRecord(out strError);
            if (nRet == -1)
                goto ERROR1;
            MessageBox.Show(this, "共处理 " + nRet.ToString() + " 个册记录");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_borrow_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = DoCirculation("borrow", out strError);
            if (nRet == -1)
                goto ERROR1;
            MessageBox.Show(this, "共处理 " + nRet.ToString() + " 个册记录");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // TODO: 显示处理耗费的时间
        void menu_return_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = DoCirculation("return", out strError);
            if (nRet == -1)
                goto ERROR1;
            MessageBox.Show(this, "共处理 " + nRet.ToString() + " 个册记录");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_inventory_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = DoCirculation("inventory", out strError);
            if (nRet == -1)
                goto ERROR1;
            MessageBox.Show(this, "共处理 " + nRet.ToString() + " 个册记录");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int VerifyItemRecord(out string strError)
        {
            strError = "";
            int nRet = 0;

            Debug.Assert(this.DbType == "item", "");

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要进行批处理的事项";
                return -1;
            }

            if (stop != null && stop.State == 0)    // 0 表示正在处理
            {
                strError = "目前有长操作正在进行，无法进行校验册记录的操作";
                return -1;
            }

            // 切换到“操作历史”属性页
            this.MainForm.ActivateFixPage("history");

            int nCount = 0;

            this.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
                + " 开始进行册记录校验</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行校验册记录的操作 ...");
            stop.BeginLoop();

            this.EnableControls(false);
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
                loader.DbTypeCaption = this.DbTypeCaption;

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    BiblioInfo info = item.BiblioInfo;

                    XmlDocument itemdom = new XmlDocument();
                    try
                    {
                        itemdom.LoadXml(info.OldXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "记录 '" + info.RecPath + "' 的 XML 装入 DOM 时出错: " + ex.Message;
                        return -1;
                    }

                    List<string> errors = new List<string>();

                    // 检查根元素下的元素名是否有重复的
                    nRet = VerifyDupElementName(itemdom,
            out strError);
                    if (nRet == -1)
                        errors.Add(strError);

                    // 校验 XML 记录中是否有非法字符
                    string strReplaced = DomUtil.ReplaceControlCharsButCrLf(info.OldXml, '*');
                    if (strReplaced != info.OldXml)
                    {
                        errors.Add("XML 记录中有非法字符");
                    }

                    // 校验借书时间字符串是否合法
                    string borrowDate = DomUtil.GetElementText(itemdom.DocumentElement, "borrowDate");
                    if (string.IsNullOrEmpty(borrowDate) == false)
                    {
                        try
                        {
                            DateTime time = DateTimeUtil.FromRfc1123DateTimeString(borrowDate).ToLocalTime();
                            if (time > DateTime.Now)
                            {
                                errors.Add("借书时间 '" + time.ToString() + "' 比当前时间还靠后");
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add("borrow 元素的 borrowDate 属性值 '" + borrowDate + "' 不合法: " + ex.Message);
                        }
                    }

                    string strBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");

                    if (string.IsNullOrEmpty(strBarcode) == false)
                    {
                        string strLocation = DomUtil.GetElementText(itemdom.DocumentElement, "location");
                        strLocation = StringUtil.GetPureLocationString(strLocation);

                        string strLibraryCode = "";
                        string strRoom = "";
                        // 解析
                        Global.ParseCalendarName(strLocation,
                    out strLibraryCode,
                    out strRoom);

                        // <para>-2  服务器没有配置校验方法，无法校验</para>
                        // <para>-1  出错</para>
                        // <para>0   不是合法的条码号</para>
                        // <para>1   是合法的读者证条码号</para>
                        // <para>2   是合法的册条码号</para>
                        nRet = this.MainForm.VerifyBarcode(
        this.stop,
        this.Channel,
        strLibraryCode,
        strBarcode,
        null,
        out strError);
                        if (nRet == -2)
                            return -1;
                        if (nRet != 2)
                        {
                            if (nRet == 1 && string.IsNullOrEmpty(strError) == true)
                                strError = strLibraryCode + ": 这看起来是一个证条码号";

                            errors.Add("册条码号 '" + strBarcode + "' 不合法: " + strError);

                        }
                    }

                    // 模拟删除一些元素
                    nRet = SimulateDeleteElement(itemdom,
out strError);
                    if (nRet == -1)
                        errors.Add(strError);

                    if (errors.Count > 0)
                    {
                        this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");
                        foreach (string error in errors)
                        {
                            this.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(error) + "</div>");
                        }

                        {
                            item.ListViewItem.BackColor = Color.FromArgb(155, 0, 0);
                            item.ListViewItem.ForeColor = Color.FromArgb(255, 255, 255);
                        }
                    }

                    nCount++;
                    stop.SetProgressValue(++i);
                }

                return nCount;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControls(true);

                this.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
                    + " 结束执行册记录校验</div>");
            }
        }

        int SimulateDeleteElement(XmlDocument dom,
            out string strError)
        {
            strError = "";

            string[] names = {
                "borrower",
                "borrowerReaderType",
                "borrowerRecPath",
                "borrowDate",
                "borrowPeriod",
                "returningDate",
                "lastReturningDate",
                "operator",
                "no",
                "renewComment"};

            List<string> errors = new List<string>();
            foreach (string name in names)
            {
#if NO
                DomUtil.DeleteElement(dom.DocumentElement, name);
                XmlNodeList nodes = dom.DocumentElement.SelectNodes(name);
                if (nodes.Count > 0)
                    errors.Add("根元素下的 " + name + " 元素模拟删除一次后依然存在");
#endif
                DomUtil.DeleteElements(dom.DocumentElement, name);
                XmlNodeList nodes = dom.DocumentElement.SelectNodes(name);
                if (nodes.Count > 0)
                    errors.Add("根元素下的 " + name + " 元素模拟 DeleteElements 删除后依然存在");
            }

            if (errors.Count == 0)
                return 0;
            strError = StringUtil.MakePathList(errors, "; ");
            return -1;
        }

        int VerifyDupElementName(XmlDocument dom,
            out string strError)
        {
            strError = "";
            if (dom.DocumentElement == null)
                return 0;

            List<string> errors = new List<string>();
            foreach (XmlNode node in dom.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    XmlNodeList nodes = dom.DocumentElement.SelectNodes(node.Name); // 2016/11/18 修改 bug
                    if (nodes.Count > 1)
                    {
                        errors.Add("根元素下的 " + node.Name + " 元素出现了多次 " + nodes.Count);
                    }
                }
            }

            if (errors.Count == 0)
                return 0;
            strError = StringUtil.MakePathList(errors, "; ");
            return -1;
        }

        // 进行流通操作
        int DoCirculation(string strAction,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            Debug.Assert(this.DbType == "item", "");

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要进行批处理的事项";
                return -1;
            }

            if (stop != null && stop.State == 0)    // 0 表示正在处理
            {
                strError = "目前有长操作正在进行，无法进行借书或者还书的操作";
                return -1;
            }

            string strOperName = "";
            if (strAction == "borrow")
                strOperName = "借书";
            else if (strAction == "return")
                strOperName = "还书";
            else if (strAction == "inventory")
                strOperName = "盘点";
            else
            {
                strError = "未知的 strAction 值 '" + strAction + "'";
                return -1;
            }

            int nCount = 0;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行" + strOperName + "操作 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {

                // 如果必要，促使登录一次，以便观察当前用户的权限
                if (string.IsNullOrEmpty(this.Channel.Rights) == true)
                {
                    string strValue = "";
                    long lRet = Channel.GetSystemParameter(stop,
                        "library",
                        "name",
                        out strValue,
                        out strError);
                }

                // 检查前端权限
                if (StringUtil.IsInList("client_simulateborrow", this.Channel.Rights) == false)
                {
                    strError = "当前用户不具备 client_simulateborrow 权限，无法进行模拟" + strOperName + "的操作";
                    return -1;
                }

                // 打开一个新的快捷出纳窗
                QuickChargingForm form = new QuickChargingForm();
                form.MdiParent = this.MainForm;
                form.MainForm = this.MainForm;
                form.Show();

                string strReaderBarcode = "";

                if (strAction == "borrow")
                {
                    strReaderBarcode = InputDlg.GetInput(
         this,
         "批处理" + strOperName,
         "请输入读者证条码号:",
         "",
         this.MainForm.DefaultFont);
                    if (strReaderBarcode == null)
                    {
                        form.Close();
                        strError = "用户放弃操作";
                        return -1;
                    }

                    form.SmartFuncState = FuncState.Borrow;
                    string strID = Guid.NewGuid().ToString();
                    form.AsyncDoAction(FuncState.Borrow, strReaderBarcode, strID);
                    DateTime start = DateTime.Now;
                    // 等待任务完成
                    while (true)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        string strState = form.GetTaskState(strID);
                        if (strState != null)
                        {
                            if (strState == "error")
                            {
                                strError = "输入读者证条码号的过程有误";
                                return -1;
                            }

                            if (strState == "finish" || strState == "error")
                                break;
                        }
                        Thread.Sleep(1000);
                        TimeSpan delta = DateTime.Now - start;
                        if (delta.TotalSeconds > 30)
                        {
                            strError = "任务长时间没有反应，后继操作被放弃";
                            return -1;
                        }
                    }
                }
                else if (strAction == "return")
                {
                    form.SmartFuncState = FuncState.Return;
                }
                else if (strAction == "inventory")
                {
                    form.SmartFuncState = FuncState.InventoryBook;
                    string strBatchNo = InputDlg.GetInput(
this,
"批处理" + strOperName,
"请输入批次号:",
"",
this.MainForm.DefaultFont);
                    if (strBatchNo == null)
                    {
                        form.Close();
                        strError = "用户放弃操作";
                        return -1;
                    }
                    form.BatchNo = strBatchNo;
                }

                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    string strRecPath = ListViewUtil.GetItemText(item, 0);

                    string strItemBarcode = "";
                    // 根据 ListViewItem 对象，获得册条码号列的内容
                    nRet = GetItemBarcodeOrRefID(
                        item,
                        true,
                        out strItemBarcode,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (String.IsNullOrEmpty(strItemBarcode) == true)
                        continue;

                    form.AsyncDoAction(form.SmartFuncState, strItemBarcode);

                    stop.SetProgressValue(++i);

                    nCount++;
                }

                // form.Close();
                return nCount;
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

        int GetSelectedReaderBarcodes(out List<string> reader_barcodes,
    ref int nWarningLineCount,
    ref int nDupCount,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            int nDelta = 0;
            if (m_bBiblioSummaryColumn == false)
                nDelta += 1;
            else
                nDelta += 2;

            stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

            reader_barcodes = new List<string>();
            Hashtable table = new Hashtable();
            int i = 0;
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                stop.SetProgressValue(i++);

                Application.DoEvents();	// 出让界面控制权

                if (stop != null
                    && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;

                string strReaderBarcode = "";


                // 获得指定类型的的列的值
                // 通过 browse 配置文件中的类型来指定
                // return:
                //      -1  出错
                //      0   指定的列没有找到
                //      1   找到
                nRet = GetTypedColumnText(
                    item,
                    "borrower",
                    nDelta,
                    out strReaderBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    // TODO: 转而采用检索法获得书目记录路径
                    nWarningLineCount++;
                    continue;
                }

                if (string.IsNullOrEmpty(strReaderBarcode) == true)
                    continue;

                // 去重，并保持原始顺序
                if (table.ContainsKey(strReaderBarcode) == false)
                {
                    reader_barcodes.Add(strReaderBarcode);
                    table[strReaderBarcode] = 1;
                }
                else
                    nDupCount++;
            }

            return 0;
        }

        int GetSelectedBiblioRecPath(out List<string> biblio_recpaths,
            ref int nWarningLineCount,
            ref int nDupCount,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

            biblio_recpaths = new List<string>();
            Hashtable table = new Hashtable();
            int i = 0;
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                stop.SetProgressValue(i++);

                Application.DoEvents();	// 出让界面控制权

                if (stop != null
                    && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;

                int nCol = -1;
                string strBiblioRecPath = "";
                // 获得事项所从属的书目记录的路径
                // return:
                //      -1  出错
                //      0   相关数据库没有配置 parent id 浏览列
                //      1   找到
                nRet = GetBiblioRecPath(item,
                    true,
                    out nCol,
                    out strBiblioRecPath,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    // TODO: 转而采用检索法获得书目记录路径
                    nWarningLineCount++;
                    continue;
                }

                // 去重，并保持原始顺序
                if (table.ContainsKey(strBiblioRecPath) == false)
                {
                    biblio_recpaths.Add(strBiblioRecPath);
                    table[strBiblioRecPath] = 1;
                }
                else
                    nDupCount++;
            }

            return 0;
        }

        // 输出读者详情到 Excel 文件。读者所借阅的各册图书
        void menu_exportToReaderExcelFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            int nWarningLineCount = 0;
            int nDupCount = 0;
            string strText = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要装入读者查询窗的行";
                goto ERROR1;
            }

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装入记录到读者查询窗 ...");
            stop.BeginLoop();

            try
            {
                List<string> reader_barcodes = new List<string>();
                nRet = GetSelectedReaderBarcodes(out reader_barcodes,
            ref nWarningLineCount,
            ref nDupCount,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nWarningLineCount > 0)
                    strText = "有 " + nWarningLineCount.ToString() + " 行因为相关库浏览格式没有定义 type 为 borrower 的栏而被忽略";
                if (nDupCount > 0)
                {
                    if (string.IsNullOrEmpty(strText) == false)
                        strText += "\r\n\r\n";
                    strText += "读者记录有 " + nDupCount.ToString() + " 项重复被忽略";
                }

                if (reader_barcodes.Count == 0)
                {
                    strError = "没有找到相关的读者记录";
                    goto ERROR1;
                }

                ReaderSearchForm form = new ReaderSearchForm();
                form.MdiParent = this.MainForm;
                form.Show();

                // return:
                //      -1  出错
                //      0   用户中断
                //      1   成功
                nRet = form.CreateReaderDetailExcelFile(reader_barcodes,
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
            }

            if (string.IsNullOrEmpty(strText) == false)
                MessageBox.Show(this, strText);

            return;
        ERROR1:
            if (string.IsNullOrEmpty(strText) == false)
                MessageBox.Show(this, strText);

            MessageBox.Show(this, strError);
        }

        // 装入读者查询窗口
        void menu_exportToReaderSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            int nWarningLineCount = 0;
            int nDupCount = 0;
            string strText = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要装入读者查询窗的行";
                goto ERROR1;
            }

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装入记录到读者查询窗 ...");
            stop.BeginLoop();

            try
            {
                List<string> reader_barcodes = new List<string>();
                nRet = GetSelectedReaderBarcodes(out reader_barcodes,
            ref nWarningLineCount,
            ref nDupCount,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nWarningLineCount > 0)
                    strText = "有 " + nWarningLineCount.ToString() + " 行因为相关库浏览格式没有定义 type 为 borrower 的栏而被忽略";
                if (nDupCount > 0)
                {
                    if (string.IsNullOrEmpty(strText) == false)
                        strText += "\r\n\r\n";
                    strText += "读者记录有 " + nDupCount.ToString() + " 项重复被忽略";
                }

                if (reader_barcodes.Count == 0)
                {
                    strError = "没有找到相关的读者记录";
                    goto ERROR1;
                }

                ReaderSearchForm form = new ReaderSearchForm();
                form.MdiParent = this.MainForm;
                form.Show();

                form.EnableControls(false);
                foreach (string barcode in reader_barcodes)
                {
                    form.AddBarcodeToBrowseList(barcode);
                }
                form.EnableControls(true);
                form.RrefreshAllItems();

            }
            finally
            {

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;
            }

            if (string.IsNullOrEmpty(strText) == false)
                MessageBox.Show(this, strText);

            return;
        ERROR1:
            if (string.IsNullOrEmpty(strText) == false)
                MessageBox.Show(this, strText);

            MessageBox.Show(this, strError);
        }


        // TODO: 优化后，和导出书目记录路径文件合并代码
        // 将从属的书目记录装入书目查询窗
        void menu_exportToBiblioSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要装入书目查询窗的行";
                goto ERROR1;
            }

            BiblioSearchForm form = new BiblioSearchForm();
            form.MdiParent = this.MainForm;
            form.Show();

            int nWarningLineCount = 0;
            int nDupCount = 0;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装入记录到书目查询窗 ...");
            stop.BeginLoop();

            form.EnableControls(false);
            try
            {
#if NO
                List<string> biblio_recpaths = new List<string>();
                Hashtable table = new Hashtable();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    int nCol = -1;
                    string strBiblioRecPath = "";
                    // 获得事项所从属的书目记录的路径
                    // return:
                    //      -1  出错
                    //      0   相关数据库没有配置 parent id 浏览列
                    //      1   找到
                    nRet = GetBiblioRecPath(item,
                        true,
                        out nCol,
                        out strBiblioRecPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        // TODO: 转而采用检索法获得书目记录路径
                        nWarningLineCount++;
                        continue;
                    }

                    // 去重，并保持原始顺序
                    if (table.ContainsKey(strBiblioRecPath) == false)
                    {
                        biblio_recpaths.Add(strBiblioRecPath);
                        table[strBiblioRecPath] = 1;
                    }
                    else
                        nDupCount++;
                }
#endif
                List<string> biblio_recpaths = new List<string>();
                nRet = GetSelectedBiblioRecPath(out biblio_recpaths,
            ref nWarningLineCount,
            ref nDupCount,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                foreach (string path in biblio_recpaths)
                {
                    form.AddLineToBrowseList(path);
                }
            }
            finally
            {
                form.EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;
            }

            form.RefreshAllLines();

            string strText = "";
            if (nWarningLineCount > 0)
                strText = "有 " + nWarningLineCount.ToString() + " 行因为相关库浏览格式没有包含父记录 ID 列而被忽略";
            if (nDupCount > 0)
            {
                if (string.IsNullOrEmpty(strText) == false)
                    strText += "\r\n\r\n";
                strText += "书目记录有 " + nDupCount.ToString() + " 项重复被忽略";
            }

            if (string.IsNullOrEmpty(strText) == false)
                MessageBox.Show(this, strText);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 删除所选择的记录
        void menu_deleteSelectedRecords_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
    "确实要从数据库中删除所选定的 " + this.listView_records.SelectedItems.Count.ToString() + " 个" + this.DbTypeCaption + "记录?\r\n\r\n(OK 删除；Cancel 取消)",
    "BiblioSearchForm",
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

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除" + this.DbTypeCaption + "记录 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            this.listView_records.Enabled = false;
            try
            {
                stop.SetProgressRange(0, items.Count);

                ListViewPatronLoader loader = new ListViewPatronLoader(this.Channel,
    stop,
    items,
    this.m_biblioTable);
                loader.DbTypeCaption = this.DbTypeCaption;

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

                    Debug.Assert(item.ListViewItem == items[i], "");
                    //string strRecPath = ListViewUtil.GetItemText(item, 0);

                    EntityInfo entity = new EntityInfo();

                    EntityInfo[] entities = new EntityInfo[1];
                    entities[0] = entity;
                    entity.Action = "delete";
                    entity.OldRecPath = info.RecPath;
                    entity.NewRecord = "";
                    entity.NewTimestamp = null;
                    entity.OldRecord = info.OldXml;
                    entity.OldTimestamp = info.Timestamp;
#if NO
                    entity.RefID = "";

                    if (String.IsNullOrEmpty(entity.RefID) == true)
                        entity.RefID = BookItem.GenRefID();
#endif

                    stop.SetMessage("正在删除" + this.DbTypeCaption + "记录 " + info.RecPath);

                    string strBiblioRecPath = "";
                    EntityInfo[] errorinfos = null;

                    long lRet = 0;

                    if (this.DbType == "item")
                    {
                        lRet = Channel.SetEntities(
                             stop,
                             strBiblioRecPath,
                             entities,
                             out errorinfos,
                             out strError);
                    }
                    else if (this.DbType == "order")
                    {
                        lRet = Channel.SetOrders(
        stop,
        strBiblioRecPath,
        entities,
        out errorinfos,
        out strError);
                    }
                    else if (this.DbType == "issue")
                    {
                        lRet = Channel.SetIssues(
        stop,
        strBiblioRecPath,
        entities,
        out errorinfos,
        out strError);
                    }
                    else if (this.DbType == "comment")
                    {
                        lRet = Channel.SetComments(
        stop,
        strBiblioRecPath,
        entities,
        out errorinfos,
        out strError);
                    }
                    else
                    {
                        strError = "未知的事项类型 '" + this.DbType + "'";
                        goto ERROR1;
                    }

                    if (lRet == -1)
                        goto ERROR1;
                    if (errorinfos != null)
                    {
                        foreach (EntityInfo error in errorinfos)
                        {
                            if (error.ErrorCode != ErrorCodeValue.NoError)
                                strError += error.ErrorInfo;
                            goto ERROR1;
                        }
                    }

                    stop.SetProgressValue(i);

                    this.listView_records.Items.Remove(item.ListViewItem);
                    i++;
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
                this.listView_records.Enabled = true;
            }

            MessageBox.Show(this, "成功删除" + this.DbTypeCaption + "记录 " + items.Count + " 条");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 快速修改记录
        void menu_quickChangeItemRecords_Click(object sender, EventArgs e)
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
        void menu_quickChangeItemRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            // bool bSkipUpdateBrowse = false; // 是否要跳过更新浏览行

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要快速修改的"+this.DbTypeCaption+"记录事项";
                goto ERROR1;
            }

            List<OneAction> actions = null;
            XmlDocument cfg_dom = null;

            if (this.DbType == "item"
                || this.DbType == "order"
                || this.DbType == "issue"
                || this.DbType == "comment")
            {
                ChangeItemActionDialog dlg = new ChangeItemActionDialog();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.DbType = this.DbType;
                dlg.Text = "快速修改" + this.DbTypeCaption + "记录 -- 请指定动作参数";
                dlg.MainForm = this.MainForm;
                dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
                dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

                this.MainForm.AppInfo.LinkFormState(dlg, "itemsearchform_quickchange"+this.DbType+"dialog_state");
                dlg.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(dlg);

                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;

                actions = dlg.Actions;
                cfg_dom = dlg.CfgDom;
            }

            DateTime now = DateTime.Now;

            // TODO: 检查一下，看看是否一项修改动作都没有
            this.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行快速修改" + this.DbTypeCaption + "记录</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("快速修改" + this.DbTypeCaption + "记录 ...");
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

                    this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

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
                    if (this.DbType == "item"
                || this.DbType == "order"
                || this.DbType == "issue"
                || this.DbType == "comment")
                    {
                        // 修改一个订购记录 XmlDocument
                        // return:
                        //      -1  出错
                        //      0   没有实质性修改
                        //      1   发生了修改
                        nRet = ModifyOrderRecord(
                            cfg_dom,
                            actions,
                            ref dom,
                            now,
                            out strDebugInfo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    this.MainForm.OperHistory.AppendHtml("<div class='debug normal'>" + HttpUtility.HtmlEncode(strDebugInfo).Replace("\r\n", "<br/>") + "</div>");

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

                this.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束快速修改" + this.DbTypeCaption + "记录</div>");
            }

            DoViewComment(false);
            MessageBox.Show(this, "修改" + this.DbTypeCaption + "记录 " + nChangedCount.ToString() + " 条 (共处理 " + nProcessCount.ToString() + " 条)\r\n\r\n(注意修改并未自动保存。请在观察确认后，使用保存命令将修改保存回" + this.DbTypeCaption + "库)");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
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
#endif




#if NO
        // 修改一个订购记录 XmlDocument
        // return:
        //      -1  出错
        //      0   没有实质性修改
        //      1   发生了修改
        int ModifyOrderRecord(
            List<OneAction> actions,
            ref XmlDocument dom,
            DateTime now,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";

            bool bChanged = false;

            StringBuilder debug = new StringBuilder(4096);

            // state
            string strStateAction = this.MainForm.AppInfo.GetString(
                "change_order_param",
                "state",
                "<不改变>");
            if (strStateAction != "<不改变>")
            {
                string strState = DomUtil.GetElementText(dom.DocumentElement,
                    "state");

                if (strStateAction == "<增、减>")
                {
                    string strAdd = this.MainForm.AppInfo.GetString(
                "change_order_param",
                "state_add",
                "");
                    string strRemove = this.MainForm.AppInfo.GetString(
            "change_order_param",
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

            // 其它字段
            string strFieldName = this.MainForm.AppInfo.GetString(
"change_order_param",
"field_name",
"<不使用>");

            if (strFieldName != "<不使用>")
            {
                string strFieldValue = this.MainForm.AppInfo.GetString(
    "change_order_param",
    "field_value",
    "");
                if (strFieldName == "编号")
                {
                    ChangeField(ref dom,
            "index",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "书目号")
                {
                    ChangeField(ref dom,
            "catalogNo",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "渠道")
                {
                    ChangeField(ref dom,
            "seller",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "经费来源")
                {
                    ChangeField(ref dom,
            "source",
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

                if (strFieldName == "时间范围")
                {
                    ChangeField(ref dom,
            "range",
            strFieldValue,
            ref debug,
            ref bChanged);
                }



                if (strFieldName == "包含期数")
                {
                    ChangeField(ref dom,
            "issueCount",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "复本数")
                {
                    ChangeField(ref dom,
            "copy",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "单价")
                {
                    ChangeField(ref dom,
            "price",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "总价格")
                {
                    ChangeField(ref dom,
            "totalPrice",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "订购时间")
                {
                    ChangeField(ref dom,
            "orderTime",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "订单号")
                {
                    ChangeField(ref dom,
            "orderID",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "馆藏分配")
                {
                    ChangeField(ref dom,
            "distribute",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "类别")
                {
                    ChangeField(ref dom,
            "class",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "渠道地址")
                {
                    ChangeField(ref dom,
            "sellerAddress",
            strFieldValue,
            ref debug,
            ref bChanged);
                }

                if (strFieldName == "批次号")
                {
                    ChangeField(ref dom,
            "batchNo",
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

        // 调用打印订单窗口 [验收情况]
        void menu_printOrderFormAccept_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strFilename = PathUtil.MergePath(this.MainForm.DataDir, "~orderrecpath.txt");
            bool bAppend = false;   // 不希望出现询问追加的对话框，直接覆盖
            // 导出到订购记录路径文件
            // return:
            //      -1  出错
            //      0   放弃导出
            //      >0  导出成功，数字是已经导出的条数
            int nRet = ExportToRecPathFile(
                strFilename,
                ref bAppend,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                strError = "所指定的订购记录中没有包含任何已验收的册记录信息";
                goto ERROR1;
            }

            PrintOrderForm form = new PrintOrderForm();
            // form.MainForm = this.MainForm;
            form.MdiParent = this.MainForm;
            form.Show();

            form.AcceptCondition = true;

            // 从(已验收的)册记录路径文件装载
            // return:
            //      -1  出错
            //      0   放弃
            //      1   装载成功
            nRet = form.LoadFromOrderRecPathFile(
                true,
                strFilename,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 调用打印订单窗口
        void menu_printOrderForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strFilename = PathUtil.MergePath(this.MainForm.DataDir, "~orderrecpath.txt");
            bool bAppend = false;   // 不希望出现询问追加的对话框，直接覆盖
            // 导出到订购记录路径文件
            // return:
            //      -1  出错
            //      0   放弃导出
            //      >0  导出成功，数字是已经导出的条数
            int nRet = ExportToRecPathFile(
                strFilename,
                ref bAppend,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                strError = "所指定的订购记录中没有包含任何已验收的册记录信息";
                goto ERROR1;
            }

            PrintOrderForm form = new PrintOrderForm();
            // form.MainForm = this.MainForm;
            form.MdiParent = this.MainForm;
            form.Show();

            form.AcceptCondition = false;

            // 从(已验收的)册记录路径文件装载
            // return:
            //      -1  出错
            //      0   放弃
            //      1   装载成功
            nRet = form.LoadFromOrderRecPathFile(
                true,
                strFilename,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 调用打印催询单窗口
        void menu_printClaimForm_Click(object sender, EventArgs e)
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
                string strOrderDbName = Global.GetDbName(strFirstRecPath);
                if (string.IsNullOrEmpty(strOrderDbName) == false)
                {
                    string strBiblioDbName = this.MainForm.GetBiblioDbNameFromOrderDbName(strOrderDbName);
                    strIssueDbName = this.MainForm.GetIssueDbName(strBiblioDbName);
                }
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
            form.SetOrderRecPaths(recpaths);
            form.EnableControls(true);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 调用打印验收单窗口
        void menu_printAcceptForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strFilename = PathUtil.MergePath(this.MainForm.DataDir, "~itemrecpath.txt");
            bool bAppend = false;   // 不希望出现询问追加的对话框，直接覆盖
            // 导出到(已验收的)册记录路径文件
            // return:
            //      -1  出错
            //      0   放弃导出
            //      >0  导出成功，数字是已经导出的条数
            int nRet = ExportToItemRecPathFile(
                strFilename,
                ref bAppend,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                strError = "所指定的订购记录中没有包含任何已验收的册记录信息";
                goto ERROR1;
            }

            PrintAcceptForm form = new PrintAcceptForm();
            // form.MainForm = this.MainForm;
            form.MdiParent = this.MainForm;
            form.Show();

            // 从(已验收的)册记录路径文件装载
            // return:
            //      -1  出错
            //      0   放弃
            //      1   装载成功
            nRet = form.LoadFromItemRecPathFile(
                true,
                strFilename,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 导出到(已验收的)册记录路径文件
        void menu_saveToAcceptItemRecordPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的册记录路径文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportItemRecPathFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "册记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportItemRecPathFilename = dlg.FileName;

            bool bAppend = true;    // 希望出现对话框询问追加，如果文件已经存在的话
            // 导出到(已验收的)册记录路径文件
            // return:
            //      -1  出错
            //      0   放弃导出
            //      >0  导出成功，数字是已经导出的条数
            nRet = ExportToItemRecPathFile(
                this.ExportItemRecPathFilename,
                ref bAppend,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            this.MainForm.StatusBarMessage = "册记录路径 " + nRet.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportItemRecPathFilename;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 导出到(已验收的)册记录路径文件
        // parameters:
        //      bAppend [in]如果可能的话是否尽量追加到已经存在的文件末尾 [out]实际采用的是否为追加方式
        // return:
        //      -1  出错
        //      0   放弃导出
        //      >0  导出成功，数字是已经导出的条数
        int ExportToItemRecPathFile(
            string strFilename,
            ref bool bAppend,
            out string strError)
        {
            strError = "";
            int nCount = 0;

            if (File.Exists(strFilename) == true && bAppend == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "册记录路径文件 '" + strFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    this.DbType + "SearchForm",
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

            // 创建文件
            using (StreamWriter sw = new StreamWriter(strFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8))
            {
                stop.Style = StopStyle.EnableHalfStop;
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在导出已验收的册记录路径 ...");
                stop.BeginLoop();

                this.EnableControls(false);
                try
                {
                    foreach (ListViewItem item in this.listView_records.SelectedItems)
                    {
                        if (String.IsNullOrEmpty(item.Text) == true)
                            continue;

                        List<string> itemrecpaths = null;

                        // 根据订购记录路径，检索出订购记录，并且获得相关联的已验收册记录路径
                        // parameters:
                        // return: 
                        //      -1  出错
                        //      1   成功
                        int nRet = LoadOrderItem(item.Text,
                out itemrecpaths,
                out strError);
                        if (nRet == -1)
                            return -1;

                        foreach (string strPath in itemrecpaths)
                        {
                            sw.WriteLine(strPath);
                            nCount++;
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
            }
            return nCount;
        }

        // 根据订购记录路径，检索出订购记录，并且获得相关联的已验收册记录路径
        // parameters:
        //      strRecPath  订购库记录路径
        // return: 
        //      -1  出错
        //      1   成功
        int LoadOrderItem(string strRecPath,
            out List<string> itemrecpaths,
            out string strError)
        {
            strError = "";
            itemrecpaths = new List<string>();
            int nRet = 0;

            string strOrderXml = "";
            string strBiblioText = "";

            string strOutputOrderRecPath = "";
            string strOutputBiblioRecPath = "";

            byte[] item_timestamp = null;

            long lRet = Channel.GetOrderInfo(
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
                strError = "获取订购记录 " + strRecPath + " 时出错: " + strError;
                return -1;
            }

            // 剖析一个订购xml记录，取出有关信息
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strOrderXml);
            }
            catch (Exception ex)
            {
                strError = "订购记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            // 观察订购库是不是期刊库类型
            // return:
            //      -1  不是订购库
            //      0   图书类型
            //      1   期刊类型
            nRet = this.MainForm.IsSeriesTypeFromOrderDbName(Global.GetDbName(strRecPath));
            if (nRet == -1)
            {
                strError = "IsSeriesTypeFromOrderDbName() '" + strRecPath + "' error";
                return -1;
            }


            List<string> distributes = new List<string>();

            if (nRet == 1)
            {
                string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
                if (string.IsNullOrEmpty(strRefID) == true)
                {
                    strError = "订购记录 '" + strRecPath + "' 中没有<refID>元素";
                    return -1;
                }

                string strBiblioDbName = this.MainForm.GetBiblioDbNameFromOrderDbName(Global.GetDbName(strRecPath));
                string strIssueDbName = this.MainForm.GetIssueDbName(strBiblioDbName);

                // 如果是期刊的订购库，还需要通过订购记录的refid获得期记录，从期记录中才能得到馆藏分配信息
                string strOutputStyle = "";
                lRet = Channel.SearchIssue(stop,
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

                    lRet = Channel.GetSearchResult(
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

                        lRet = Channel.GetIssueInfo(
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

            foreach (string strDistribute in distributes)
            {
                LocationCollection locations = new LocationCollection();
                nRet = locations.Build(strDistribute,
                    out strError);
                if (nRet == -1)
                    return -1;

                for (int i = 0; i < locations.Count; i++)
                {
                    Location location = locations[i];

                    if (string.IsNullOrEmpty(location.RefID) == true)
                        continue;

                    // 2012/9/4
                    string[] parts = location.RefID.Split(new char[] { '|' });
                    foreach (string text in parts)
                    {
                        string strRefID = text.Trim();
                        if (string.IsNullOrEmpty(strRefID) == true)
                            continue;

                        // 根据册记录的refid装入册记录
                        string strItemXml = "";
                        string strOutputItemRecPath = "";

                        lRet = Channel.GetItemInfo(
                            stop,
                            "@refID:" + strRefID,
                            "", // "xml",
                            out strItemXml,
                            out strOutputItemRecPath,
                            out item_timestamp,
                            "recpath",
                            out strBiblioText,
                            out strOutputBiblioRecPath,
                            out strError);
                        if (lRet == -1 || lRet == 0)
                        {
                            strError = "获取册记录 " + strRefID + " 时出错: " + strError;
                        }

                        itemrecpaths.Add(strOutputItemRecPath);
                    }
                }
            }

            return 1;
        }

        void menu_createCallNumber_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            Debug.Assert(this.DbType == "item", "");

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要进行批处理的事项";
                goto ERROR1;
            }

            if (stop != null && stop.State == 0)    // 0 表示正在处理
            {
                strError = "目前有长操作正在进行，无法进行创建索取号的操作";
                goto ERROR1;
            }

            bool bOverwrite = false;
            {
                DialogResult result = MessageBox.Show(this,
    "在后面即将进行的处理过程中，对已经存在索取号的册记录，是否要重新创建索取号?\r\n\r\n(Yes: 要重新创建；No: 不重新创建，即：跳过；Cancel: 现在就放弃本次批处理)",
    this.DbType + "SearchForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.Yes)
                    bOverwrite = true;
                else if (result == System.Windows.Forms.DialogResult.No)
                    bOverwrite = false;
                else
                    return;

            }

            int nCount = 0;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建索取号 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {

                // 打开一个新的种册窗
                EntityForm form = null;

                form = new EntityForm();

                form.MdiParent = this.MainForm;

                form.MainForm = this.MainForm;
                form.Show();

                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                ItemSearchForm itemsearchform = null;
                bool bHideMessageBox = false;

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

                    string strRecPath = ListViewUtil.GetItemText(item, 0);

                    // parameters:
                    //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    form.LoadItemByRecPath(strRecPath, true);

                    // 为当前选定的事项创建索取号
                    // return:
                    //      -1  出错
                    //      0   放弃处理
                    //      1   已经处理
                    nRet = form.EntityControl.CreateCallNumber(bOverwrite, out strError);
                    if (nRet == -1)
                        goto ERROR;

                    if (nRet == 1)
                    {
                        nCount++;
                        // form.DoSaveAll();
                        nRet = form.EntityControl.SaveItems(
                            this.Channel,
                            out strError);
                        if (nRet == -1)
                            goto ERROR;

                        nRet = RefreshBrowseLine(item,
    out strError);
                        if (nRet == -1)
                        {
                            strError = "刷新浏览内容时出错: " + strError;
                            goto ERROR;
#if NO
                            DialogResult result = MessageBox.Show(this,
                                "刷新浏览内容时出错: " + strError + "。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
                                this.DbType + "SearchForm",
                                MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                                break;
#endif
                        }
                    }

                ERROR:
                    if (nRet == -1)
                    {
                        if (itemsearchform == null)
                        {
                            Form active_mdi = this.MainForm.ActiveMdiChild;

                            itemsearchform = new ItemSearchForm();
                            itemsearchform.MdiParent = this.MainForm;
                            itemsearchform.MainForm = this.MainForm;
                            itemsearchform.Show();
                            itemsearchform.QueryWordString = "创建索取号过程中出错的册记录";

                            active_mdi.Activate();
                        }

                        ListViewItem new_item = itemsearchform.AddLineToBrowseList(Global.BuildLine(item));
                        ListViewUtil.ChangeItemText(new_item, 1, strError);

                        this.OutputText(strRecPath + " : " + strError, 2);

                        strError = "在为册记录 " + strRecPath + " 创建索取号时出错: " + strError;

                        if (bHideMessageBox == false)
                        {
                            DialogResult result = MessageDialog.Show(this,
                                strError + "。\r\n\r\n是否继续处理后面的记录? (继续： 继续；停止： 停止整个批处理)",
            MessageBoxButtons.OKCancel,
            MessageBoxDefaultButton.Button1,
            "不再出现此对话框",
            ref bHideMessageBox,
            new string[] { "继续", "停止" });
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                                goto ERROR1;
                        }
                        form.EntitiesChanged = false;
                    }

                    stop.SetProgressValue(++i);
                }

                // form.DoSaveAll();
                form.Close();
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

            MessageBox.Show(this, "共处理 " + nCount.ToString() + " 个册记录");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public string QueryWordString
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

#if NO
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
#endif

        // string m_strTempQuickBarcodeFilename = "";

#if NO
        void menu_quickChangeRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            Debug.Assert(this.DbType == "item", "");

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要快速修改的事项";
                goto ERROR1;
            }

            if (stop != null && stop.State == 0)    // 0 表示正在处理
            {
                strError = "目前有长操作正在进行，无法进行快速修改";
                goto ERROR1;
            }


            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在修改册记录 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                if (string.IsNullOrEmpty(m_strTempQuickBarcodeFilename) == true)
                {
                    m_strTempQuickBarcodeFilename = PathUtil.MergePath(this.MainForm.DataDir, "~" + Guid.NewGuid().ToString());
                }

                File.Delete(m_strTempQuickBarcodeFilename);
                using (StreamWriter sr = new StreamWriter(m_strTempQuickBarcodeFilename))
                {
                    if (stop != null)
                        stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);
                    {
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
                                stop.SetMessage("正在构造记录路径文件 " + item.Text + " ...");
                                stop.SetProgressValue(i++);
                            }

                            /*
                        // 将册条码号写入文件
                            string strBarcode = ListViewUtil.GetItemText(item, 1);
                            if (string.IsNullOrEmpty(strBarcode) == true)
                                continue;

                            sr.WriteLine(strBarcode);
                             * */
                            // 将记录路径写入文件
                            string strRecPath = ListViewUtil.GetItemText(item, 0);
                            if (string.IsNullOrEmpty(strRecPath) == true)
                                continue;

                            sr.WriteLine(strRecPath);
                        }
                    }
                }

                if (stop != null)
                {
                    stop.SetMessage("正在调用快速修改册窗进行批处理 ...");
                    stop.SetProgressValue(0);
                }

                // 新打开一个快速修改册窗口
                QuickChangeEntityForm form = new QuickChangeEntityForm();
                // form.MainForm = this.MainForm;
                form.MdiParent = this.MainForm;
                form.Show();

                if (form.SetChangeParameters() == false)
                {
                    form.Close();
                    return;
                }

                // form.DoBarcodeFile(m_strTempQuickBarcodeFilename);
                // return:
                //      -1  出错
                //      0   放弃处理
                //      >=1 处理的条数
                nRet = form.DoRecPathFile(m_strTempQuickBarcodeFilename);
                form.Close();

                if (nRet == 0)
                    return;

                if (nRet == -1)
                {
                    DialogResult result = MessageBox.Show(this,
"部分记录已经发生了修改，是否需要刷新浏览行? (OK 刷新；Cancel 放弃刷新)",
this.DbType + "SearchForm",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.Cancel)
                        return;
                }

                if (stop != null)
                    stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                {
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
                                this.DbType + "SearchForm",
                                MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                                break;
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

                this.EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif

        void menu_selectAllLines_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            // m_nInSelectedIndexChanged++;    // 禁止事件响应
            try
            {
                this.listView_records.SelectedIndexChanged -= new System.EventHandler(this.listView_records_SelectedIndexChanged);

                ListViewUtil.SelectAllLines(this.listView_records);

                this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
                listView_records_SelectedIndexChanged(null, null);
            }
            finally
            {
                // m_nInSelectedIndexChanged--;
                this.Cursor = oldCursor;
            }
        }

        // 刷新书目摘要
        void menu_refreshSelectedItemsBiblioSummary_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this._listviewRecords.SelectedItems.Count == 0)
            {
                strError = "尚未选择要刷新的浏览行";
                goto ERROR1;
            }

            int nChangedCount = 0;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this._listviewRecords.SelectedItems)
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
    "要刷新的 " + this._listviewRecords.SelectedItems.Count.ToString() + " 个事项中有 " + nChangedCount.ToString() + " 项修改后尚未保存。如果刷新它们，修改内容会丢失。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
    "ItemSearchForm",
    MessageBoxButtons.OKCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
            }

            m_tableSummaryColIndex.Clear();

            nRet = FillBiblioSummaryColumn(items,
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            DoViewComment(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 刷新所选择的行。也就是重新从数据库中装载浏览列
        void menu_refreshSelectedItems_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";
            int nRet = 0;

            Debug.Assert(this.DbType == "item", "");

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
                            this.DbType + "SearchForm",
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
#endif
            RrefreshSelectedItems();
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

        // 在一个新开的实体查询窗内检索key
        void listView_searchKeysAtNewWindow_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要操作的事项");
                return;
            }

            ItemSearchForm form = new ItemSearchForm();
            form.DbType = this.DbType;
            form.MdiParent = this.MainForm;
            // form.MainForm = this.MainForm;
            form.Show();

            int i = 0;
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                ItemQueryParam query = (ItemQueryParam)item.Tag;
                Debug.Assert(query != null, "");

                ItemQueryParam input_query = new ItemQueryParam();

                input_query.QueryWord = ListViewUtil.GetItemText(item, 1);
                input_query.DbNames = query.DbNames;
                input_query.From = query.From;
                input_query.MatchStyle = "精确一致";

                // 2015/1/17
                if (string.IsNullOrEmpty(input_query.QueryWord) == true)
                    input_query.MatchStyle = "空值";
                else
                    input_query.MatchStyle = "精确一致";


                // 检索命中记录(而不是key)
                int nRet = form.DoSearch(false, false, input_query, i == 0 ? true : false);
                if (nRet != 1)
                    break;

                i++;
            }
        }

#if NO
        void GetSelectedItemCount(out int nPathItemCount,
            out int nKeyItemCount)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            nPathItemCount = 0;
            nKeyItemCount = 0;
            for (int i = 0; i < this.listView_records.SelectedItems.Count; i++)
            {
                if (String.IsNullOrEmpty(this.listView_records.SelectedItems[i].Text) == false)
                    nPathItemCount++;
                else
                    nKeyItemCount++;
            }

            this.Cursor = oldCursor;
        }
#endif

        void menu_clearList_Click(object sender, EventArgs e)
        {
            ClearListViewItems();
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
            DateTime start_time = DateTime.Now;
            this.listView_records.SelectedIndexChanged -= new System.EventHandler(this.listView_records_SelectedIndexChanged);

            Global.CopyLinesToClipboard(this, this.listView_records, true);

            this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
            listView_records_SelectedIndexChanged(null, null);
            TimeSpan delta = DateTime.Now - start_time;
            this.MainForm.StatusBarMessage = "剪切操作 耗时 " + delta.TotalSeconds.ToString() + " 秒";
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

        // 往列表中追加若干册条码号
        // return:
        //      -1  出错
        //      0   成功
        //      1   成功，但有警告，警告在 strError 中返回
        public int AppendBarcodes(List<string> barcodes,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            SetStatusMessage("");   // 清除以前残留的显示

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入条码号 ...");
            stop.BeginLoop();

            try
            {
                // 准备查重
                Hashtable table = new Hashtable();
                foreach (ListViewItem item in this.listView_records.Items)
                {
                    string strBarcode = "";
                    // 根据 ListViewItem 对象，获得册条码号列的内容
                    nRet = GetItemBarcodeOrRefID(
                        item,
                        true,
                        out strBarcode,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    table[strBarcode] = 1;
                }

                List<string> dups = new List<string>();
                List<string> errors = new List<string>();

                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this.listView_records);

                stop.SetProgressRange(0, barcodes.Count);

                List<ListViewItem> items = new List<ListViewItem>();

                int i = 0;
                foreach (string strBarcode in barcodes)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    stop.SetProgressValue(i);

                    if (strBarcode == null)
                        break;

                    if (table.ContainsKey(strBarcode) == true)
                    {
                        dups.Add(strBarcode);
                        continue;
                    }

                    ListViewItem item = new ListViewItem();
                    item.Text = "";

                    // return:
                    //      false   出现错误
                    //      true    成功
                    if (FillLineByBarcode(strBarcode, item) == true)
                    {
                        this.listView_records.Items.Add(item);
                        items.Add(item);
                    }
                    else
                        errors.Add(item.SubItems[2].Text);

                    i++;
                }

                // 刷新浏览行
                nRet = RefreshListViewLines(items,
                    "",
                    false,
                    true,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 刷新书目摘要
                nRet = FillBiblioSummaryColumn(items,
                    false,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (errors.Count > 0)
                {
                    strError = StringUtil.MakePathList(errors, "\r\n");
                    return -1;
                }

                if (dups.Count > 0)
                {
                    strError = "下列册条码号已经在列表中存在了: " + StringUtil.MakePathList(dups);
                    return 1;
                }
                return 0;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }
        }


        // TODO: 优化速度
        void menu_importFromBarcodeFile_Click(object sender, EventArgs e)
        {
            SetStatusMessage("");   // 清除以前残留的显示

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
                        this.DbType + "SearchForm",
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

                    if (stop != null && stop.State != 0)
                    {
                        MessageBox.Show(this, "用户中断");
                        return;
                    }

                    string strBarcode = sr.ReadLine();

                    stop.SetProgressValue(sr.BaseStream.Position);

                    if (strBarcode == null)
                        break;

                    ListViewItem item = new ListViewItem();
                    item.Text = "";
                    // ListViewUtil.ChangeItemText(item, 1, strBarcode);

                    this.listView_records.Items.Add(item);

                    FillLineByBarcode(strBarcode, item);

                    items.Add(item);
                }

                // 刷新浏览行
                int nRet = RefreshListViewLines(items,
                    "",
                    false,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2014/1/15
                // 刷新书目摘要
                nRet = FillBiblioSummaryColumn(items,
                    false,
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

        // 从记录路径文件中导入
        void menu_importFromRecPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = ImportFromRecPathFile(null,
                "clear",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 调用前，记录路径列已经有值
        /// <summary>
        /// 刷新一个浏览行的各列信息。
        /// 也就是从数据库中重新获取相关信息。
        /// 不刷新书目摘要列
        /// </summary>
        /// <param name="item">要刷新的 ListViewItem 对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
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
                if (this.m_bBiblioSummaryColumn == false)
                    ListViewUtil.ChangeItemText(item,
                    i + 1,
                    searchresults[0].Cols[i]);
                else
                    ListViewUtil.ChangeItemText(item,
                    i + 2,
                    searchresults[0].Cols[i]);
            }

            return 0;
        }

        // 导出选择的行中有路径的部分行 的条码栏内容 为条码号文件
        void menu_exportBarcodeFile_Click(object sender, EventArgs e)
        {
            Debug.Assert(this.DbType == "item", "");

            string strError = "";

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
                    "文件 '" + this.ExportBarcodeFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    this.DbType + "SearchForm",
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

            // m_tableBarcodeColIndex.Clear();
            ClearColumnIndexCache();

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportBarcodeFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (String.IsNullOrEmpty(item.Text) == true)
                        continue;

#if NO
                    string strRecPath = ListViewUtil.GetItemText(item, 0);
                    // 根据记录路径获得数据库名
                    string strItemDbName = Global.GetDbName(strRecPath);
                    // 根据数据库名获得 册条码号 列号

                    int nCol = -1;
                    object o = m_tableBarcodeColIndex[strItemDbName];
                    if (o == null)
                    {
                        ColumnPropertyCollection temp = this.MainForm.GetBrowseColumnProperties(strItemDbName);
                        nCol = temp.FindColumnByType("item_barcode");
                        if (nCol == -1)
                        {
                            // 这个实体库没有在 browse 文件中 册条码号 列
                            strError = "警告：实体库 '"+strItemDbName+"' 的 browse 配置文件中没有定义 type 为 item_barcode 的列。请注意刷新或修改此配置文件";
                            MessageBox.Show(this, strError);

                            nCol = 0;   // 这个大部分情况能奏效
                        }
                        if (m_bBiblioSummaryColumn == false)
                            nCol += 1;
                        else 
                            nCol += 2;

                        m_tableBarcodeColIndex[strItemDbName] = nCol;   // 储存起来
                    }
                    else
                        nCol = (int)o;

                    Debug.Assert(nCol > 0, "");

                    string strBarcode = ListViewUtil.GetItemText(item, nCol);
#endif

                    string strBarcode = "";
                    // 根据 ListViewItem 对象，获得册条码号列的内容
                    int nRet = GetItemBarcodeOrRefID(
                        item,
                        true,
                        out strBarcode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;
                    sw.WriteLine(strBarcode);   // BUG!!!
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                if (sw != null)
                    sw.Close();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            this.MainForm.StatusBarMessage = "册条码号 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportBarcodeFilename;
            return;
        ERROR1:
            MessageBox.Show(strError);
        }


        // 当前缺省的编码方式
        Encoding CurrentEncoding = Encoding.UTF8;

        // 为了保存ISO2709文件服务的几个变量
        /// <summary>
        /// 获取或设置配置参数：最近一次使用过的 ISO2709 文件名
        /// 这是 ItemSearchForm 和 BiblioSearchForm 都适用的一个配置参数
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
        /// 获取或设置配置参数：醉意近一次输出到 ISO2709 文件时是否要在记录间插入回车换行符号
        /// 这是 ItemSearchForm 和 BiblioSearchForm 都适用的一个配置参数
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
        /// 获取或设置配置参数：最近一次使用过的编码方式名称
        /// 这是 ItemSearchForm 和 BiblioSearchForm 都适用的一个配置参数
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
        /// 获取或设置配置参数：最近一次使用过的编目规则名称
        /// 这是 ItemSearchForm 和 BiblioSearchForm 都适用的一个配置参数
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

        // 将馆藏地点名和编目规则名的对照表装入内存
        // return:
        //      -1  出错
        //      0   文件不存在
        //      1   成功
        static int LoadRuleNameTable(string strFilename,
            out Hashtable table,
            out string strError)
        {
            strError = "";
            table = new Hashtable();

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                strError = "装载XML文件 '" + strFilename + "' 时发生错误: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("location");
            foreach (XmlNode node in nodes)
            {
                string strLocationName = DomUtil.GetAttr(node, "name");
                string strRuleName = DomUtil.GetAttr(node, "catalogingRule");

                table[strLocationName] = strRuleName;
            }

            return 1;
        }

        int GetSelectedBiblioRecPath(
            ref List<string> biblioRecPathList,// 按照出现先后的顺序存储书目记录路径
            ref Hashtable groupTable, // 书目记录路径 --> List<string> (册记录路径列表)
            out string strError)
        {
            strError = "";
            int nRet = 0;

            stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

            int i = 0;
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                stop.SetProgressValue(i++);

                Application.DoEvents();	// 出让界面控制权

                if (stop != null
                    && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;

                int nCol = -1;
                string strBiblioRecPath = "";
                // 获得事项所从属的书目记录的路径
                // return:
                //      -1  出错
                //      0   相关数据库没有配置 parent id 浏览列
                //      1   找到
                nRet = GetBiblioRecPath(item,
                    true,
                    out nCol,
                    out strBiblioRecPath,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    return -1;

                List<string> item_recpaths = (List<string>)groupTable[strBiblioRecPath];
                if (item_recpaths == null)
                {
                    biblioRecPathList.Add(strBiblioRecPath);
                    item_recpaths = new List<string>();
                    groupTable[strBiblioRecPath] = item_recpaths;
                }

                item_recpaths.Add(item.Text);
            }

            return 0;
        }

        // 保存选择的行中的有路径的部分行 到书目转储文件
        // 需要先将册记录按照书目记录路径聚集
        void menu_exportBiblioDumpFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            List<string> biblioRecPathList = new List<string>();   // 按照出现先后的顺序存储书目记录路径

            Hashtable groupTable = new Hashtable();   // 书目记录路径 --> List<string> (册记录路径列表)


            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的书目转储文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = this.ExportBiblioDumpFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "书目转储文件 (*.bdf)|*.bdf|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportBiblioDumpFilename = dlg.FileName;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导出书目转储记录 ...");
            stop.BeginLoop();

            try
            {
                using (XmlTextWriter w = new XmlTextWriter(
                    this.ExportBiblioDumpFilename, Encoding.UTF8))
                {
                    w.Formatting = Formatting.Indented;
                    w.Indentation = 4;

                    w.WriteStartDocument();
                    w.WriteStartElement("dprms", "collection", DpNs.dprms);
                    w.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);

                    nRet = GetSelectedBiblioRecPath(
                        ref biblioRecPathList,// 按照出现先后的顺序存储书目记录路径
                        ref groupTable, // 书目记录路径 --> List<string> (册记录路径列表)
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    foreach (string strBiblioRecPath in biblioRecPathList)
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        string[] results = null;
                        byte[] baTimestamp = null;

                        stop.SetMessage("正在获取书目记录 " + strBiblioRecPath);

                        long lRet = Channel.GetBiblioInfos(
                            stop,
                            strBiblioRecPath,
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
                        XmlDocument domBiblio = new XmlDocument();
                        domBiblio.LoadXml(strXml);

                        // 写入 dprms:record 元素
                        w.WriteStartElement("dprms", "record", DpNs.dprms);

                        // 写入 dprms:biblio 元素
                        w.WriteStartElement("dprms", "biblio", DpNs.dprms);

#if NO
                        // 给根元素设置几个参数
                        DomUtil.SetAttr(domBiblio.DocumentElement, "path", DpNs.dprms, this.MainForm.LibraryServerUrl + "?" + item.BiblioInfo.RecPath);  // strRecPath
                        DomUtil.SetAttr(domBiblio.DocumentElement, "timestamp", DpNs.dprms, ByteArray.GetHexTimeStampString(item.BiblioInfo.Timestamp));   // baTimestamp
#endif

                        w.WriteAttributeString("path", this.MainForm.LibraryServerUrl + "?" + strBiblioRecPath);
                        w.WriteAttributeString("timestamp", ByteArray.GetHexTimeStampString(baTimestamp));

                        domBiblio.DocumentElement.WriteTo(w);
                        w.WriteEndElement();

                        w.WriteStartElement("dprms", "itemCollection", DpNs.dprms);
                        List<string> item_recpaths = (List<string>)groupTable[strBiblioRecPath];
                        foreach (string item_recpath in item_recpaths)
                        {

                            string strItemXml = "";
                            byte[] baItemTimestamp = null;
                            // 获得一条记录
                            //return:
                            //      -1  出错
                            //      0   没有找到
                            //      1   找到
                            nRet = GetRecord(
            item_recpath,
            out strItemXml,
            out baItemTimestamp,
            out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            XmlDocument item_dom = new XmlDocument();
                            item_dom.LoadXml(strItemXml);

                            w.WriteStartElement("dprms", "item", DpNs.dprms);
                            w.WriteAttributeString("path", item_recpath);
                            w.WriteAttributeString("timestamp", ByteArray.GetHexTimeStampString(baItemTimestamp));
                            DomUtil.RemoveEmptyElements(item_dom.DocumentElement);
                            item_dom.DocumentElement.WriteContentTo(w);
                            w.WriteEndElement();
                        }
                        w.WriteEndElement();

                        // 收尾 dprms:record 元素
                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                    w.WriteEndDocument();
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }

            this.MainForm.StatusBarMessage = "书目记录 " + groupTable.Count.ToString() + "个 已成功导出到文件 " + this.ExportBiblioDumpFilename;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 将从属的书目记录保存到MARC文件
        void menu_saveBiblioRecordToMarcFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要保存的事项";
                goto ERROR1;
            }

            Hashtable rule_name_table = null;
            bool bTableExists = false;
            // 将馆藏地点名和编目规则名的对照表装入内存
            // return:
            //      -1  出错
            //      0   文件不存在
            //      1   成功
            nRet = LoadRuleNameTable(PathUtil.MergePath(this.MainForm.DataDir, "cataloging_rules.xml"),
                out rule_name_table,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
                bTableExists = true;

            Debug.Assert(rule_name_table != null, "");

            Encoding preferredEncoding = this.CurrentEncoding;

            {
                // 观察要保存的第一条记录的marc syntax
            }

            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            MainForm.SetControlFont(dlg, this.Font);

            dlg.IsOutput = true;
            dlg.AddG01Visible = false;
            if (bTableExists == false)
            {
                dlg.RuleVisible = true;
                dlg.Rule = this.LastCatalogingRule;
            }
            dlg.FileName = this.LastIso2709FileName;
            dlg.CrLf = this.LastCrLfIso2709;
            dlg.EncodingListItems = Global.GetEncodingList(false);
            dlg.EncodingName =
                (String.IsNullOrEmpty(this.LastEncodingName) == true ? Global.GetEncodingName(preferredEncoding) : this.LastEncodingName);
            dlg.EncodingComment = "注: 原始编码方式为 " + Global.GetEncodingName(preferredEncoding);
            dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            dlg.EnableMarcSyntax = false;
            dlg.ShowDialog(this);

            this.LastIso2709FileName = dlg.FileName;
            this.LastCrLfIso2709 = dlg.CrLf;
            this.LastEncodingName = dlg.EncodingName;
            this.LastCatalogingRule = dlg.Rule;

            if (dlg.DialogResult != DialogResult.OK)
                return;

            string strCatalogingRule = "";

            if (bTableExists == false)
            {
                strCatalogingRule = dlg.Rule;
                if (strCatalogingRule == "<无限制>")
                    strCatalogingRule = null;
            }

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
                    this.DbType + "SearchForm",
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
                        this.DbType + "SearchForm",
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

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存到 MARC 文件 ...");
            stop.BeginLoop();

            Stream s = null;

            int nOutputCount = 0;

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
                List<string> biblioRecPathList = new List<string>();   // 按照出现先后的顺序存储书目记录路径

                Hashtable groupTable = new Hashtable();   // 书目记录路径 --> List<string> (册记录路径列表)

                nRet = GetSelectedBiblioRecPath(
    ref biblioRecPathList,// 按照出现先后的顺序存储书目记录路径
    ref groupTable, // 书目记录路径 --> List<string> (册记录路径列表)
    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strMARC = "";
                string strMarcSyntax = "";
                MarcRecord record = null;
                int nItemIndex = 0;
                List<BiblioInfo> sub_items = new List<BiblioInfo>();

                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                nRet = DumpBiblioAndPartsSubItems.Dump(this.Channel,
                    stop,
                    this.DbType,
                    biblioRecPathList,
                    groupTable,
                    // 书目记录到来
                    (biblio_info) =>
                    {

                        strMARC = "";
                        strMarcSyntax = "";
                        // 将XML格式转换为MARC格式
                        // 自动从数据记录中获得MARC语法
                        nRet = MarcUtil.Xml2Marc(biblio_info.OldXml,
                            true,
                            null,
                            out strMarcSyntax,
                            out strMARC,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "XML转换到MARC记录时出错: " + strError;
                            throw new Exception(strError);
                        }

                        Debug.Assert(strMarcSyntax != "", "");

                        record = new MarcRecord(strMARC);

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
                            record.select("field[@name='998']").detach();
                            record.select("field[@name='997']").detach();
                        }
                        if (dlg.Mode880 == true && strMarcSyntax == "usmarc")
                        {
                            MarcQuery.To880(record);
                        }

                        sub_items.Clear();
                        return true;
                    },
                    // 每一次册记录到来
                    (biblio_info, item_info) =>
                    {
                        sub_items.Add(item_info);

                        XmlDocument item_dom = new XmlDocument();
                        item_dom.LoadXml(item_info.OldXml);

                        string strLocation = DomUtil.GetElementText(item_dom.DocumentElement, "location");
                        strLocation = StringUtil.GetPureLocation(strLocation);

                        if (bTableExists == true)
                        {
                            strCatalogingRule = "";
                            // 根据馆藏地点获得编目规则名
                            if (this.DbType == "item"
                                && string.IsNullOrEmpty(strLocation) == false)
                            {
                                // 
                                strCatalogingRule = (string)rule_name_table[strLocation];
                                if (string.IsNullOrEmpty(strCatalogingRule) == true)
                                {
                                    strCatalogingRule = InputDlg.GetInput(
                                        this,
                                        null,
                                        "请输入馆藏地点 '" + strLocation + "' 所对应的编目规则名称:",
                                        "NLC",
                                        this.MainForm.DefaultFont);
                                    if (strCatalogingRule == null)
                                    {
                                        DialogResult result = MessageBox.Show(this,
                                            "由于您没有指定馆藏地点 '" + strLocation + "' 所对应的编目规则名，此馆藏地点被当作 <无限制> 编目规则来处理。\r\n\r\n是否继续操作? (OK 继续；Cancel 放弃整个导出操作)",
                                            this.DbType + "SearchForm",
                                            MessageBoxButtons.OKCancel,
                                            MessageBoxIcon.Question,
                                            MessageBoxDefaultButton.Button1);
                                        if (result == System.Windows.Forms.DialogResult.Cancel)
                                            throw new InterruptException("中断");
                                        strCatalogingRule = "";
                                    }

                                    rule_name_table[strLocation] = strCatalogingRule; // 储存到内存，后面就不再作相同的询问了
                                }
                            }
                        }

                        nItemIndex++;
                        return true;
                    },
                    // 结束书目记录处理
                    (biblio_info) =>
                    {
                        if (dlg_905.Create905 && dlg_905.RemoveOld905)
                            record.select("field[@name='905']").detach();

                        if (dlg_905.Create906)
                            record.select("field[@name='906']").detach();

                        Create905(
                            dlg_905.Create905,
                            dlg_905.Style905,
                            dlg_905.Create906,
                            record,
                            sub_items);

                        byte[] baTarget = null;
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
                            record.Text,
                            strMarcSyntax,
                            targetEncoding,
                            out baTarget,
                            out strError);
                        if (nRet == -1)
                            throw new Exception(strError);

                        s.Write(baTarget, 0,
                            baTarget.Length);

                        if (dlg.CrLf == true)
                        {
                            byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                            s.Write(baCrLf, 0,
                                baCrLf.Length);
                        }

                        stop.SetProgressValue(nItemIndex);

                        nOutputCount++;

                        return true;
                    },
                    () => { Application.DoEvents(); return true; },
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

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
                MainForm.StatusBarMessage = nOutputCount.ToString()
                    + "条记录成功追加到文件 " + this.LastIso2709FileName + " 尾部";
            else
                MainForm.StatusBarMessage = nOutputCount.ToString()
                    + "条记录成功保存到新文件 " + this.LastIso2709FileName + " 尾部";

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        static void Create905(
            bool bCreate905,
            string str905Style,
            bool bCreate906,
            MarcRecord record,
            List<BiblioInfo> sub_items)
        {
            string strError = "";

            if (bCreate905)
            {
                if (str905Style == "每册一个 905 字段")
                {
                    foreach (BiblioInfo info in sub_items)
                    {
                        XmlDocument item_dom = new XmlDocument();
                        item_dom.LoadXml(info.OldXml);

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
                            throw new Exception(strError);

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

                    foreach (BiblioInfo info in sub_items)
                    {
                        XmlDocument item_dom = new XmlDocument();
                        item_dom.LoadXml(info.OldXml);

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
                                    throw new Exception(strError);
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
                    throw new Exception(strError);
                }
            }

            if (bCreate906)
            {
                MarcField field = new MarcField("906", "  ");

                foreach (BiblioInfo info in sub_items)
                {
                    XmlDocument item_dom = new XmlDocument();
                    item_dom.LoadXml(info.OldXml);

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
        }

#if NO
        // 将从属的书目记录保存到MARC文件
        void menu_saveBiblioRecordToMarcFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要保存的事项";
                goto ERROR1;
            }

            // 为了书目记录路径去重服务
            Hashtable biblio_recpath_table = new Hashtable();

            Hashtable rule_name_table = null;
            bool bTableExists = false;
            // 将馆藏地点名和编目规则名的对照表装入内存
            // return:
            //      -1  出错
            //      0   文件不存在
            //      1   成功
            nRet = LoadRuleNameTable(PathUtil.MergePath(this.MainForm.DataDir, "cataloging_rules.xml"),
                out rule_name_table,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
                bTableExists = true;

            Debug.Assert(rule_name_table != null, "");

            Encoding preferredEncoding = this.CurrentEncoding;

            {
                // 观察要保存的第一条记录的marc syntax
            }

            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            MainForm.SetControlFont(dlg, this.Font);

            dlg.IsOutput = true;
            dlg.AddG01Visible = false;
            if (bTableExists == false)
            {
                dlg.RuleVisible = true;
                dlg.Rule = this.LastCatalogingRule;
            }
            dlg.FileName = this.LastIso2709FileName;
            dlg.CrLf = this.LastCrLfIso2709;
            dlg.EncodingListItems = Global.GetEncodingList(false);
            dlg.EncodingName =
                (String.IsNullOrEmpty(this.LastEncodingName) == true ? Global.GetEncodingName(preferredEncoding) : this.LastEncodingName);
            dlg.EncodingComment = "注: 原始编码方式为 " + Global.GetEncodingName(preferredEncoding);
            dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            dlg.EnableMarcSyntax = false;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            string strCatalogingRule = "";

            if (bTableExists == false)
            {
                strCatalogingRule = dlg.Rule;
                if (strCatalogingRule == "<无限制>")
                    strCatalogingRule = null;
            }

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
                    this.DbType + "SearchForm",
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
                        this.DbType + "SearchForm",
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

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存到 MARC 文件 ...");
            stop.BeginLoop();

            Stream s = null;

            int nOutputCount = 0;

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

                    if (String.IsNullOrEmpty(strRecPath) == true)
                        goto CONTINUE;

                    stop.SetMessage("正在准备记录 " + strRecPath + " 的书目记录路径 ...");
                    stop.SetProgressValue(i);

                    string strItemRecPath = "";
                    string strBiblioRecPath = "";
                    string strLocation = "";
                    if (this.DbType == "item")
                    {
                        Debug.Assert(this.DbType == "item", "");

                        nRet = SearchTwoRecPathByBarcode(
                            this.stop,
                            this.Channel,
                            "@path:" + strRecPath,
                out strItemRecPath,
                out strLocation,
                out strBiblioRecPath,
                out strError);
                    }
                    else
                    {
                        nRet = SearchBiblioRecPath(
                            this.stop,
                            this.Channel,
                            this.DbType,
                            strRecPath,
out strBiblioRecPath,
out strError);
                    }
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }
                    else if (nRet == 0)
                    {
                        strError = "记录路径 '" + strRecPath + "' 没有找到记录";
                        goto ERROR1;
                    }
                    else if (nRet == 1)
                    {
                        item.Text = strItemRecPath;
                    }
                    else if (nRet > 1) // 命中发生重复
                    {
                        strError = "记录路径 '" + strRecPath + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误";
                    }

                    // 去重
                    if (biblio_recpath_table.ContainsKey(strBiblioRecPath) == true)
                        goto CONTINUE;

                    if (bTableExists == true)
                    {
                        strCatalogingRule = "";
                        // 根据馆藏地点获得编目规则名
                        if (this.DbType == "item"
                            && string.IsNullOrEmpty(strLocation) == false)
                        {
                            // 
                            strCatalogingRule = (string)rule_name_table[strLocation];
                            if (string.IsNullOrEmpty(strCatalogingRule) == true)
                            {
                                strCatalogingRule = InputDlg.GetInput(
                                    this,
                                    null,
                                    "请输入馆藏地点 '" + strLocation + "' 所对应的编目规则名称:",
                                    "NLC",
                                    this.MainForm.DefaultFont);
                                if (strCatalogingRule == null)
                                {
                                    DialogResult result = MessageBox.Show(this,
                                        "由于您没有指定馆藏地点 '" + strLocation + "' 所对应的编目规则名，此馆藏地点被当作 <无限制> 编目规则来处理。\r\n\r\n是否继续操作? (OK 继续；Cancel 放弃整个导出操作)",
                                        this.DbType + "SearchForm",
                                        MessageBoxButtons.OKCancel,
                                        MessageBoxIcon.Question,
                                        MessageBoxDefaultButton.Button1);
                                    if (result == System.Windows.Forms.DialogResult.Cancel)
                                        break;
                                    strCatalogingRule = "";
                                }

                                rule_name_table[strLocation] = strCatalogingRule; // 储存到内存，后面就不再作相同的询问了
                            }
                        }
                    }

                    string[] results = null;
                    byte[] baTimestamp = null;

                    stop.SetMessage("正在获取书目记录 " + strBiblioRecPath);

                    long lRet = Channel.GetBiblioInfos(
                        stop,
                        strBiblioRecPath,
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
                        record.select("field[@name='997']").detach();
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

                    stop.SetProgressValue(i + 1);

                    nOutputCount++;

                CONTINUE:
                    i++;
                    // biblio_recpath_table[strBiblioRecPath] = 1;
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
                MainForm.StatusBarMessage = nOutputCount.ToString()
                    + "条记录成功追加到文件 " + this.LastIso2709FileName + " 尾部";
            else
                MainForm.StatusBarMessage = nOutputCount.ToString()
                    + "条记录成功保存到新文件 " + this.LastIso2709FileName + " 尾部";

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#endif

        // 保存选择的行中的有路径的部分行 到书目库记录路径文件
        void menu_saveToBiblioRecordPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的(书目库)记录路径文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportBiblioRecPathFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportBiblioRecPathFilename = dlg.FileName;

            bool bAppend = true;

            // List<string> paths = new List<string>();
            List<string> biblio_recpaths = new List<string>();

            if (File.Exists(this.ExportBiblioRecPathFilename) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "(书目库)记录路径文件 '" + this.ExportBiblioRecPathFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    this.DbType + "SearchForm",
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

            int nWarningLineCount = 0;
            int nDupCount = 0;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导出书目记录路径 ...");
            stop.BeginLoop();

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportBiblioRecPathFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
            try
            {

#if NO
                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count
                    + this.listView_records.SelectedItems.Count / 10);

                {
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

                        if (String.IsNullOrEmpty(strRecPath) == true)
                            continue;

                        stop.SetMessage("正在准备记录 " + strRecPath + " 的书目记录路径 ...");
                        stop.SetProgressValue(i++);

                        string strItemRecPath = "";
                        string strBiblioRecPath = "";
                        int nRet = 0;
                        if (this.DbType == "item")
                        {
                            nRet = SearchTwoRecPathByBarcode("@path:" + strRecPath,
                    out strItemRecPath,
                    out strBiblioRecPath,
                    out strError);
                        }
                        else
                        {
                            nRet = SearchBiblioRecPath(strRecPath,
    out strBiblioRecPath,
    out strError);
                        }
                        if (nRet == -1)
                        {
                            goto ERROR1;
                        }
                        else if (nRet == 0)
                        {
                            strError = "记录路径 '" + strRecPath + "' 没有找到记录";
                            goto ERROR1;
                        }
                        else if (nRet == 1)
                        {
                            item.Text = strItemRecPath;
                        }
                        else if (nRet > 1) // 命中发生重复
                        {
                            strError = "记录路径 '" + strRecPath + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误";
                        }

                        paths.Add(strBiblioRecPath);
                    }
                }

                /*
                paths.Sort();
                StringUtil.RemoveDup(ref paths);
                 * */
                stop.SetMessage("正在归并...");
                StringUtil.RemoveDupNoSort(ref paths);

                stop.SetMessage("正在写入文件...");
                int nBase = this.listView_records.SelectedItems.Count;

                stop.SetProgressRange(0, nBase + paths.Count / 10);

                for (int i = 0; i < paths.Count; i++)
                {
                    Application.DoEvents();

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    sw.WriteLine(paths[i]);
                    stop.SetProgressValue(nBase + i / 10);
                }
#endif
                nRet = GetSelectedBiblioRecPath(out biblio_recpaths,
            ref nWarningLineCount,
            ref nDupCount,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                foreach (string path in biblio_recpaths)
                {
                    Application.DoEvents();

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    sw.WriteLine(path);
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                if (sw != null)
                    sw.Close();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            this.MainForm.StatusBarMessage = "书目记录路径 " + biblio_recpaths.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportBiblioRecPathFilename;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 导出到记录路径文件。和当前窗口的类型有关
        // parameters:
        //      bAppend [in]如果可能的话是否尽量追加到已经存在的文件末尾 [out]实际采用的是否为追加方式
        // return:
        //      -1  出错
        //      0   放弃导出
        //      >0  导出成功，数字是已经导出的条数
        /// <summary>
        /// 将当前选定的事项导出到记录路径文件
        /// </summary>
        /// <param name="strFilename">记录路径文件名</param>
        /// <param name="bAppend">[in]如果可能的话是否尽量追加到已经存在的文件末尾 [out]实际采用的是否为追加方式</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中; 0: 放弃导出; >0 导出成功，本方法的此时的返回值是已经导出的记录数</returns>
        public int ExportToRecPathFile(string strFilename,
            ref bool bAppend,
            out string strError)
        {
            strError = "";
            int nCount = 0;

            if (File.Exists(this.ExportRecPathFilename) == true && bAppend == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "记录路径文件 '" + this.ExportRecPathFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    this.DbType + "SearchForm",
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

            // 创建文件
            using (StreamWriter sw = new StreamWriter(strFilename,
    bAppend,	// append
    System.Text.Encoding.UTF8))
            {
                stop.Style = StopStyle.EnableHalfStop;
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在导出记录路径 ...");
                stop.BeginLoop();

                this.EnableControls(false);
                try
                {

                    foreach (ListViewItem item in this.listView_records.SelectedItems)
                    {
                        if (String.IsNullOrEmpty(item.Text) == true)
                            continue;
                        sw.WriteLine(item.Text);
                        nCount++;
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
            }

            return nCount;
        }

        // 保存选择的行中的有路径的部分行 到记录路径文件
        void menu_saveToRecordPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";

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

            // return:
            //      -1  出错
            //      0   放弃导出
            //      >0  导出成功，数字是已经导出的条数
            int nRet = ExportToRecPathFile(
                this.ExportRecPathFilename,
                ref bAppend,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            this.MainForm.StatusBarMessage = "册记录路径 " + nRet.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportRecPathFilename;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NOOOOOOOOOOO

        // 把ListViewItem文本内容构造为tab字符分割的字符串
        static string BuildLine(ListViewItem item)
        {
            string strLine = "";
            for (int i = 0; i < item.SubItems.Count; i++)
            {
                if (i != 0)
                    strLine += "\t";
                strLine += item.SubItems[i].Text;
            }

            return strLine;
        }

        // 根据字符串构造ListViewItem。
        // 字符串的格式为\t间隔的
        static ListViewItem BuildListViewItem(string strLine)
        {
            ListViewItem item = new ListViewItem();
            string[] parts = strLine.Split(new char[] {'\t'});
            for (int i = 0; i < parts.Length; i++)
            {
                ListViewUtil.ChangeItemText(item, i, parts[i]);
            }

            return item;
        }

#endif

        // 保存选择的行到文本文件
        void menu_exportTextFile_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的文本文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportTextFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "文本文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportTextFilename = dlg.FileName;

            bool bAppend = true;

            if (File.Exists(this.ExportTextFilename) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "文本文件 '" + this.ExportTextFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    this.DbType + "SearchForm",
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
            using (StreamWriter sw = new StreamWriter(this.ExportTextFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8))
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    string strLine = Global.BuildLine(item);
                    sw.WriteLine(strLine);
                }

                this.Cursor = oldCursor;
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            this.MainForm.StatusBarMessage = "行内容 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文本文件 " + this.ExportTextFilename;
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

        private void comboBox_entityDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_entityDbName.Items.Count > 0)
                return;

            this.comboBox_entityDbName.Items.Add("<全部>");

            if (this.DbType == "arrive"
                && string.IsNullOrEmpty(this.MainForm.ArrivedDbName) == false)
            {
                this.comboBox_entityDbName.Items.Add(this.MainForm.ArrivedDbName);
                return;
            }

            if (this.DbType != "issue")
                this.comboBox_entityDbName.Items.Add("<全部图书>");

            this.comboBox_entityDbName.Items.Add("<全部期刊>");

            if (this.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty property = this.MainForm.BiblioDbProperties[i];

                    if (this.DbType == "item")
                    {
                        if (String.IsNullOrEmpty(property.ItemDbName) == false)
                            this.comboBox_entityDbName.Items.Add(property.ItemDbName);
                    }
                    else if (this.DbType == "comment")
                    {
                        if (String.IsNullOrEmpty(property.CommentDbName) == false)
                            this.comboBox_entityDbName.Items.Add(property.CommentDbName);
                    }
                    else if (this.DbType == "order")
                    {
                        if (String.IsNullOrEmpty(property.OrderDbName) == false)
                            this.comboBox_entityDbName.Items.Add(property.OrderDbName);
                    }
                    else if (this.DbType == "issue")
                    {
                        if (String.IsNullOrEmpty(property.IssueDbName) == false)
                            this.comboBox_entityDbName.Items.Add(property.IssueDbName);
                    }
                    else
                        throw new Exception("未知的DbType '" + this.DbType + "'");

                }
            }
        }

        private void toolStripButton_search_Click(object sender, EventArgs e)
        {
            if (CheckProperties() == true)
                DoSearch(false, false, null);
        }

        bool CheckProperties()
        {
            string strError = "";
            if (this.MainForm.NormalDbProperties == null)
            {
                strError = "普通数据库属性尚未初始化";
                goto ERROR1;
            }

            return true;
        ERROR1:
            MessageBox.Show(this, strError);
            return false;
        }

        private void ToolStripMenuItem_searchKeys_Click(object sender, EventArgs e)
        {
            DoSearch(true, false, null);
        }

        // 将 ItemQueryParam 中的信息恢复到面板中
        void QueryToPanel(ItemQueryParam query,
            bool bClearList = true)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                this.textBox_queryWord.Text = query.QueryWord;
                this.comboBox_entityDbName.Text = query.DbNames;
                this.comboBox_from.Text = query.From;
                this.comboBox_matchStyle.Text = query.MatchStyle;

                if (bClearList == true)
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

        private void textBox_queryWord_TextChanged(object sender, EventArgs e)
        {
            if (this.DbType == "item")
                this.Text = "实体查询 " + this.textBox_queryWord.Text;
            else
                this.Text = this.DbTypeCaption + "查询 " + this.textBox_queryWord.Text;
        }

        // int m_nInSelectedIndexChanged = 0;

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
            OnListViewSelectedIndexChanged(sender, e);
        }

        private void listView_records_ItemDrag(object sender,
            ItemDragEventArgs e)
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

        // 消除残余图像
        private void comboBox_entityDbName_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_entityDbName.Invalidate();
        }

        // 消除残余图像
        private void comboBox_from_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_from.Invalidate();
        }

        // 消除残余图像
        private void comboBox_matchStyle_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_matchStyle.Invalidate();
        }

        private void ToolStripMenuItem_searchKeyID_Click(object sender, EventArgs e)
        {
            DoSearch(false, true, null);
        }

        private void textBox_queryWord_KeyPress(object sender, KeyPressEventArgs e)
        {

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
            this.MainForm.AppInfo.LinkFormState(dlg, "searchitemform_gettimedialog_single");
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

            this.MainForm.AppInfo.LinkFormState(dlg, "searchitemform_gettimedialog_single");
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
            this.MainForm.AppInfo.LinkFormState(dlg, "searchitemform_gettimedialog_range");
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

            this.MainForm.AppInfo.LinkFormState(dlg, "searchitemform_gettimedialog_range");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_queryWord.Text = dlg.uString;
        }

        private void listView_records_ColumnContextMenuClicked(object sender, ColumnHeader columnHeader)
        {
            ColumnClickEventArgs e = new ColumnClickEventArgs(this.listView_records.Columns.IndexOf(columnHeader));
            ListViewUtil.OnColumnContextMenuClick(this.listView_records, e);
        }

        ////////

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
            string strCssFilePath = PathUtil.MergePath(this.MainForm.DataDir, "operloghtml.css");

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

        // 获得一条记录
        //return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        internal override int GetRecord(
            string strRecPath,
            out string strXml,
            out byte[] baTimestamp,
            out string strError)
        {
            strError = "";
            strXml = "";

            baTimestamp = null;
            string strOutputRecPath = "";
            string strBiblio = "";
            string strBiblioRecPath = "";
            // 获得册记录
            long lRet = 0;

            if (this.DbType == "item")
            {
                lRet = Channel.GetItemInfo(
     stop,
     "@path:" + strRecPath,
     "xml",
     out strXml,
     out strOutputRecPath,
     out baTimestamp,
     "",
     out strBiblio,
     out strBiblioRecPath,
     out strError);
            }
            else if (this.DbType == "order")
            {
                lRet = Channel.GetOrderInfo(
     stop,
     "@path:" + strRecPath,
     "xml",
     out strXml,
     out strOutputRecPath,
     out baTimestamp,
     "",
     out strBiblio,
     out strBiblioRecPath,
     out strError);
            }
            else if (this.DbType == "issue")
            {
                lRet = Channel.GetIssueInfo(
     stop,
     "@path:" + strRecPath,
     "xml",
     out strXml,
     out strOutputRecPath,
     out baTimestamp,
     "",
     out strBiblio,
     out strBiblioRecPath,
     out strError);
            }
            else if (this.DbType == "comment")
            {
                lRet = Channel.GetCommentInfo(
     stop,
     "@path:" + strRecPath,
     "xml",
     out strXml,
     out strOutputRecPath,
     out baTimestamp,
     "",
     out strBiblio,
     out strBiblioRecPath,
     out strError);
            }

            if (lRet == 0)
                return 0;  // 是否设定为特殊状态?
            if (lRet == -1)
                return -1;

            return 1;
        }

        // 保存一条记录
        // 保存成功后， info.Timestamp 会被更新
        // parameters:
        //      strStyle force/空
        // return:
        //      -2  时间戳不匹配
        //      -1  出错
        //      0   成功
        internal override int SaveRecord(string strRecPath,
            BiblioInfo info,
            string strStyle,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";
            baNewTimestamp = null;

            List<EntityInfo> entityArray = new List<EntityInfo>();

            {
                EntityInfo item_info = new EntityInfo();

                item_info.OldRecPath = strRecPath;

                // 2016/9/7
                if (StringUtil.IsInList("force", strStyle))
                    item_info.Action = "forcechange";
                else
                    item_info.Action = "change";

                item_info.NewRecPath = strRecPath;

                item_info.NewRecord = info.NewXml;
                item_info.NewTimestamp = null;

                item_info.OldRecord = info.OldXml;
                item_info.OldTimestamp = info.Timestamp;

                entityArray.Add(item_info);
            }

            // 复制到目标
            EntityInfo[] entities = null;
            entities = new EntityInfo[entityArray.Count];
            for (int i = 0; i < entityArray.Count; i++)
            {
                entities[i] = entityArray[i];
            }

            EntityInfo[] errorinfos = null;

            long lRet = 0;

            if (this.DbType == "item")
                lRet = this.Channel.SetEntities(
                     null,   // this.BiblioStatisForm.stop,
                     "",
                     entities,
                     out errorinfos,
                     out strError);
            else if (this.DbType == "order")
                lRet = this.Channel.SetOrders(
                     null,   // this.BiblioStatisForm.stop,
                     "",
                     entities,
                     out errorinfos,
                     out strError);
            else if (this.DbType == "issue")
                lRet = this.Channel.SetIssues(
                     null,   // this.BiblioStatisForm.stop,
                     "",
                     entities,
                     out errorinfos,
                     out strError);
            else if (this.DbType == "comment")
                lRet = this.Channel.SetComments(
                     null,   // this.BiblioStatisForm.stop,
                     "",
                     entities,
                     out errorinfos,
                     out strError);
            else
            {
                strError = "未知的数据库类型 '" + this.DbType + "'";
                return -1;
            }
            if (lRet == -1)
                return -1;

            // string strWarning = ""; // 警告信息

            if (errorinfos == null)
                return 0;

            strError = "";
            for (int i = 0; i < errorinfos.Length; i++)
            {
#if NO
                if (String.IsNullOrEmpty(errorinfos[i].RefID) == true)
                {
                    strError = "服务器返回的EntityInfo结构中RefID为空";
                    return -1;
                }
#endif
                if (i == 0)
                    baNewTimestamp = errorinfos[i].NewTimestamp;

                // 正常信息处理
                if (errorinfos[i].ErrorCode == ErrorCodeValue.NoError)
                    continue;

                strError += errorinfos[i].RefID + "在提交保存过程中发生错误 -- " + errorinfos[i].ErrorInfo + "\r\n";
            }

            if (baNewTimestamp != null) // 2016/9/3
                info.Timestamp = baNewTimestamp;    // 2013/10/17

            if (String.IsNullOrEmpty(strError) == false)
                return -1;

            return 0;
        }

        // 在状态行显示文字信息
        internal override void SetStatusMessage(string strMessage)
        {
            this.label_message.Text = strMessage;
        }

        // 丢弃选定的修改
        void menu_clearSelectedChangedRecords_Click(object sender, EventArgs e)
        {
            ClearSelectedChangedRecords();
        }

#if NO
        // 接收选定的修改
        // 此功能难以被一般用户理解。接受了的为何反而不能保存了？
        void menu_acceptSelectedChangedRecords_Click(object sender, EventArgs e)
        {
            AcceptSelectedChangedRecords();
        }
#endif

        // 丢弃全部修改
        void menu_clearAllChangedRecords_Click(object sender, EventArgs e)
        {
            ClearAllChangedRecords();
        }

        // 保存选定事项的修改
        void menu_saveSelectedChangedRecords_Click(object sender, EventArgs e)
        {
            SaveSelectedChangedRecords(Control.ModifierKeys == Keys.Control ? "force" : "");
        }

        // 保存全部修改事项
        void menu_saveAllChangedRecords_Click(object sender, EventArgs e)
        {
            SaveAllChangedRecords(Control.ModifierKeys == Keys.Control ? "force" : "");
        }

        // 创建一个新的 C# 脚本文件
        void menu_createMarcQueryCsFile_Click(object sender, EventArgs e)
        {
            CreateStartCsFile();
        }

        // 
        /// <summary>
        /// 创建一个新的 C# 脚本文件。会弹出对话框询问文件名。
        /// 代码中的类从 ItemHost 类派生
        /// </summary>
        public void CreateStartCsFile()
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的脚本文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = "";
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "C# 脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                ItemHost.CreateStartCsFile(dlg.FileName, this.DbType, this.DbTypeCaption);
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

            ItemHost host = null;
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
                host.DbType = this.DbType;
                host.RecordPath = "";
                host.ItemDom = null;
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
            stop.Initial("正在针对" + this.DbTypeCaption + "记录执行 C# 脚本 ...");
            stop.BeginLoop();

            this.EnableControls(false);

            this.listView_records.Enabled = false;
            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                {
                    host.MainForm = this.MainForm;
                    host.DbType = this.DbType;
                    host.RecordPath = "";
                    host.ItemDom = null;
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
                        return;
                    if (result == DialogResult.No)
                    {
                        bOldSource = false;
                    }
                }

                ListViewPatronLoader loader = new ListViewPatronLoader(this.Channel,
                    stop,
                    items,
                    this.m_biblioTable);
                loader.DbTypeCaption = this.DbTypeCaption;

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

                    this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    host.MainForm = this.MainForm;
                    host.DbType = this.DbType;
                    host.RecordPath = info.RecPath;
                    host.ItemDom = new XmlDocument();
                    if (bOldSource == true)
                    {
                        host.ItemDom.LoadXml(info.OldXml);
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
                            host.ItemDom.LoadXml(info.NewXml);
                        else
                            host.ItemDom.LoadXml(info.OldXml);
                    }
                    // host.ItemDom.LoadXml(info.OldXml);
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
                        string strXml = host.ItemDom.OuterXml;
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
                    host.DbType = this.DbType;
                    host.RecordPath = "";
                    host.ItemDom = null;
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

            DoViewComment(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 准备脚本环境
        int PrepareMarcQuery(string strCsFileName,
            out Assembly assembly,
            out ItemHost host,
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
                "dp2Circulation.ItemHost");
            if (entryClassType == null)
            {
                strError = strCsFileName + " 中没有找到 dp2Circulation.ItemHost 派生类";
                goto ERROR1;
            }

            // new一个Host派生对象
            host = (ItemHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            return 0;
        ERROR1:
            return -1;
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
                this.DoLogicSearch(false, false, null);
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        //long m_lLoaded = 0; // 本次已经装入浏览框的条数
        //long m_lHitCount = 0;   // 检索命中结果条数


        /// <summary>
        /// 执行一次逻辑检索
        /// </summary>
        /// <param name="bOutputKeyCount">是否要输出为 key+count 形态</param>
        /// <param name="bOutputKeyID">是否为 keyid 形态</param>
        /// <param name="input_query">检索式</param>
        public void DoLogicSearch(bool bOutputKeyCount,
            bool bOutputKeyID,
            ItemQueryParam input_query)
        {
            string strError = "";

            if (bOutputKeyCount == true
    && bOutputKeyID == true)
            {
                strError = "bOutputKeyCount和bOutputKeyID不能同时为true";
                goto ERROR1;
            }

            if (input_query != null)
            {
                QueryToPanel(input_query);
            }

            bool bQuickLoad = false;    // 是否快速装入
            bool bClear = true; // 是否清除浏览窗中已有的内容

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                bQuickLoad = true;

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                bClear = false;


            // 修改窗口标题
            // this.Text = "书目查询 逻辑检索";

            this.m_bFirstColumnIsKey = bOutputKeyID;
            this.ClearListViewPropertyCache();

            ItemQueryParam query = PanelToQuery();
            PushQuery(query);

            if (bClear == true)
            {
                if (this.m_nChangedCount > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "当前命中记录列表中有 " + this.m_nChangedCount.ToString() + " 项修改尚未保存。\r\n\r\n是否继续操作?\r\n\r\n(Yes 清除，然后继续操作；No 放弃操作)",
                        "ItemSearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                }
                this.ClearListViewItems();

                ListViewUtil.ClearSortColumns(this.listView_records);
            }

            //this.m_lHitCount = 0;
            //this.m_lLoaded = 0;
            stop.HideProgress();

            this.label_message.Text = "";

            stop.Style = StopStyle.None;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                // string strBrowseStyle = "id,cols";
                string strOutputStyle = "";
                if (bOutputKeyCount == true)
                {
                    strOutputStyle = "keycount";
                    // strBrowseStyle = "keycount";
                }
                else if (bOutputKeyID == true)
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

                // return:
                //      -1  出错
                //      0   用户中断
                //      1   正常完成
                nRet = FillBrowseList(
                    query,
                    lHitCount,
                    bOutputKeyCount,
                    bOutputKeyID,
                    bQuickLoad,
                    out strError);
                if (nRet == 0)
                    return;

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
                this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条，已全部装入";
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

        private void dp2QueryControl1_GetList(object sender, GetListEventArgs e)
        {
            // 获得所有数据库名
            if (string.IsNullOrEmpty(e.Path) == true)
            {
                if (this.MainForm.BiblioDbProperties != null)
                {
                    for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                    {
                        BiblioDbProperty property = this.MainForm.BiblioDbProperties[i];

                        if (this.DbType == "item")
                        {
                            if (String.IsNullOrEmpty(property.ItemDbName) == false)
                                e.Values.Add(property.ItemDbName);
                        }
                        else if (this.DbType == "comment")
                        {
                            if (String.IsNullOrEmpty(property.CommentDbName) == false)
                                e.Values.Add(property.CommentDbName);
                        }
                        else if (this.DbType == "order")
                        {
                            if (String.IsNullOrEmpty(property.OrderDbName) == false)
                                e.Values.Add(property.OrderDbName);
                        }
                        else if (this.DbType == "issue")
                        {
                            if (String.IsNullOrEmpty(property.IssueDbName) == false)
                                e.Values.Add(property.IssueDbName);
                        }
                        else
                            throw new Exception("未知的DbType '" + this.DbType + "'");


                    }
                }
            }
            else
            {
                // 获得特定数据库的检索途径
                // 每个库都一样
                List<string> froms = GetFromList();
                foreach (string from in froms)
                {
                    e.Values.Add(from);
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

#if NO
            // 继续装入
            menuItem = new MenuItem("继续装入(&C)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_continueLoad_Click);
            if (this.m_lHitCount <= this.listView_records.Items.Count)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            e.ContextMenu.MenuItems.Add(menuItem);
#endif

            // 仅获得检索点
            menuItem = new MenuItem("仅获得检索点(&C)");
            menuItem.Click += new System.EventHandler(this.menu_logicSearchKeyCount_Click);
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
            this.DoLogicSearch(false, false, null);
        }

        // 仅获得检索点
        void menu_logicSearchKeyCount_Click(object sender, EventArgs e)
        {
            this.DoLogicSearch(true, false, null);
        }

        // key + id 检索，带有检索点的检索
        void menu_logicSearchKeyID_Click(object sender, EventArgs e)
        {
            this.DoLogicSearch(false, true, null);
        }
    }

    /// <summary>
    /// 查询时使用的一个检索事项
    /// </summary>
    public class ItemQueryParam
    {
        /// <summary>
        /// 检索词
        /// </summary>
        public string QueryWord = "";

        /// <summary>
        /// 数据库名
        /// </summary>
        public string DbNames = "";

        /// <summary>
        /// 检索途径
        /// </summary>
        public string From = "";

        /// <summary>
        /// 匹配方式
        /// </summary>
        public string MatchStyle = "";

        /// <summary>
        /// 浏览列的第一列是否为key
        /// </summary>
        public bool FirstColumnIsKey = false;    // 浏览列的第一列是否为key

        /// <summary>
        /// 检索命中后装入 ListView 的 ListViewItem 事项
        /// </summary>
        public List<ListViewItem> Items = new List<ListViewItem>();
    }


}