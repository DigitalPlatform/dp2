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
using DigitalPlatform.LibraryServer;
using ClosedXML.Excel;

// 2013/3/16 添加 XML 注释
// 2017/4/16 将 this.Channel 改造为 this.GetChannel() 用法

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
            ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(e.DbName);
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


            this.comboBox_from.Text = Program.MainForm.AppInfo.GetString(
                this.DbType + "_search_form",
                "from",
                strDefaultFrom);

            this.comboBox_entityDbName.Text = Program.MainForm.AppInfo.GetString(
                this.DbType + "_search_form",
                "entity_db_name",
                "<全部>");

            this.comboBox_matchStyle.Text = Program.MainForm.AppInfo.GetString(
                this.DbType + "_search_form",
                "match_style",
                "精确一致");

            if (this.DbType != "arrive")
            {
                bool bHideMatchStyle = Program.MainForm.AppInfo.GetBoolean(
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
            string strWidths = Program.MainForm.AppInfo.GetString(
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
            this.UiState = Program.MainForm.AppInfo.GetString(
    this.DbType + "_search_form",
    "ui_state",
    "");
            string strSaveString = Program.MainForm.AppInfo.GetString(
this.DbType + "_search_form",
"query_lines",
"^^^");
            this.dp2QueryControl1.Restore(strSaveString);

            comboBox_matchStyle_TextChanged(null, null);

            this.SetWindowTitle();

            this.Channel = null;    // testing
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
            if (Program.MainForm != null)
            {
                BiblioDbFromInfo[] infos = null;
                if (this.DbType == "item")
                    infos = Program.MainForm.ItemDbFromInfos;
                else if (this.DbType == "comment")
                    infos = Program.MainForm.CommentDbFromInfos;
                else if (this.DbType == "order")
                    infos = Program.MainForm.OrderDbFromInfos;
                else if (this.DbType == "issue")
                    infos = Program.MainForm.IssueDbFromInfos;
                else if (this.DbType == "arrive")
                    infos = Program.MainForm.ArrivedDbFromInfos;
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
            if (Program.MainForm != null)
            {
                BiblioDbFromInfo[] infos = null;
                if (this.DbType == "item")
                    infos = Program.MainForm.ItemDbFromInfos;
                else if (this.DbType == "comment")
                    infos = Program.MainForm.CommentDbFromInfos;
                else if (this.DbType == "order")
                    infos = Program.MainForm.OrderDbFromInfos;
                else if (this.DbType == "issue")
                    infos = Program.MainForm.IssueDbFromInfos;
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

            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetString(
                    this.DbType + "_search_form",
                    "from",
                    this.comboBox_from.Text);

                Program.MainForm.AppInfo.SetString(
                    this.DbType + "_search_form",
                    "entity_db_name",
                    this.comboBox_entityDbName.Text);

                Program.MainForm.AppInfo.SetString(
                    this.DbType + "_search_form",
                    "match_style",
                    this.comboBox_matchStyle.Text);

#if NO
            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_records);
            Program.MainForm.AppInfo.SetString(
                this.DbType + "_search_form",
                "record_list_column_width",
                strWidths);
#endif
                Program.MainForm.AppInfo.SetString(
                    this.DbType + "_search_form",
                    "ui_state",
                    this.UiState);

                Program.MainForm.AppInfo.SetString(
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
                return (int)Program.MainForm.AppInfo.GetInt(
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

            query.QueryWord = this.tabComboBox_queryWord.Text;
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

            this.label_message.Text = "";

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.Style = StopStyle.None;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(20);

            EnableControls(false);
            try
            {
                string strMatchStyle = "";

                strMatchStyle = GetCurrentMatchStyle();

                if (this.tabComboBox_queryWord.Text == "")
                {
                    if (strMatchStyle == "null")
                    {
                        this.tabComboBox_queryWord.Text = "";

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
                    lRet = channel.SearchItem(stop,
                        this.comboBox_entityDbName.Text, // "<all>",
                        this.tabComboBox_queryWord.Text,
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
                    lRet = channel.SearchComment(stop,
                        this.comboBox_entityDbName.Text,
                        this.tabComboBox_queryWord.Text,
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
                    lRet = channel.SearchOrder(stop,
                        this.comboBox_entityDbName.Text,
                        this.tabComboBox_queryWord.Text,
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
                    lRet = channel.SearchIssue(stop,
                        this.comboBox_entityDbName.Text,
                        this.tabComboBox_queryWord.Text,
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
                    if (string.IsNullOrEmpty(Program.MainForm.ArrivedDbName) == true)
                    {
                        strError = "当前服务器尚未配置预约到书库名";
                        goto ERROR1;
                    }

                    string strQueryXml = "<target list='" + Program.MainForm.ArrivedDbName + ":" + this.comboBox_from.Text + "'><item><word>"
        + StringUtil.GetXmlStringSimple(this.tabComboBox_queryWord.Text)
        + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>"
                    + this.MaxSearchResultCount + "</maxCount></item><lang>" + this.Lang + "</lang></target>";
                    // strOutputStyle ?
                    lRet = channel.Search(stop,
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
                    channel,
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
                EnableControls(true);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;
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
            LibraryChannel channel,
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

            long lMaxPerCount = 500;

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
                {
                    // StringUtil.RemoveFromInList("cols", false, ref strTempBrowseStyle);
                    strTempBrowseStyle += ",format:@coldef:*/parent";
                }

                long lRet = channel.GetSearchResult(
                    stop,
                    null,   // strResultSetName
                    lStart,
                    Math.Min(lMaxPerCount, lCount),
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

                        // 2017/2/21
                        // 填入 parent_id 列内容
                        if (bTempQuickLoad)
                        {
                            int nCol = -1;
                            string strBiblioRecPath = "";
                            // 获得事项所从属的书目记录的路径
                            // parameters:
                            //      bAutoSearch 当没有 parent id 列的时候，是否自动进行检索以便获得书目记录路径
                            // return:
                            //      -1  出错
                            //      0   相关数据库没有配置 parent id 浏览列
                            //      1   找到
                            int nRet = GetBiblioRecPath(
                                channel,
                                item,
            false,
            out nCol,
            out strBiblioRecPath,
            out strError);
                            if (nRet == 1)
                            {
                                int nTempCol = this.m_bBiblioSummaryColumn == true ? 2 : 1;
                                string strParentID = ListViewUtil.GetItemText(item, nTempCol);
                                ListViewUtil.ChangeItemText(item, nCol, strParentID);
                                ListViewUtil.ChangeItemText(item, nTempCol, "");
                            }
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
                        int nRet = _fillBiblioSummaryColumn(
                            channel,
                            items,
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
                this.tabComboBox_queryWord.Enabled = false;
            }
            else
            {
                this.tabComboBox_queryWord.Enabled = bEnable;
            }

            this.dp2QueryControl1.Enabled = bEnable;
        }

        /*
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }*/

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

                form.MdiParent = Program.MainForm;

                form.MainForm = Program.MainForm;
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

                form.MdiParent = Program.MainForm;

                form.MainForm = Program.MainForm;
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

                form.MdiParent = Program.MainForm;

                form.MainForm = Program.MainForm;
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

                form.MdiParent = Program.MainForm;

                form.MainForm = Program.MainForm;
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
                return Program.MainForm.AppInfo.GetBoolean(
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
                return Program.MainForm.AppInfo.GetBoolean(
                    "item_search_form",
                    "load_to_itemwindow",
                    false);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
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

                if (bLoadToItemWindow == true || this.DbType == "arrive")
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

                this.tabComboBox_queryWord.Text = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);
                if (query != null)
                {
                    this.comboBox_entityDbName.Text = query.DbNames;
                    this.comboBox_from.Text = query.From;
                }

                if (this.tabComboBox_queryWord.Text == "")    // 2009/8/6 
                    this.comboBox_matchStyle.Text = "空值";
                else
                    this.comboBox_matchStyle.Text = "精确一致";

                DoSearch(false, false, null);
            }
        }

        private void ItemSearchForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            // Program.MainForm.MenuItem_font.Enabled = false;
            // Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;

            Program.MainForm.toolStripDropDownButton_barcodeLoadStyle.Enabled = true;
            Program.MainForm.toolStripTextBox_barcode.Enabled = true;
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
                if (this.DbType == "arrive")
                {
                    MessageBox.Show(this, "预约到书库记录无法装入 实体窗");
                    return;
                }

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

                    form.MdiParent = Program.MainForm;

                    form.MainForm = Program.MainForm;
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

                    form.MdiParent = Program.MainForm;

                    form.MainForm = Program.MainForm;
                    form.Show();
                }

                if (this.DbType == "arrive")
                    form.DbType = "item";   // ??
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

            bool bLooping = (stop != null && stop.State == 0);    // 0 表示正在处理

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

                bool bExistEntityForm = (Program.MainForm.GetTopChildWindow<EntityForm>() != null);
                bool bExistItemInfoForm = (Program.MainForm.GetTopChildWindow<ItemInfoForm>() != null);

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
                if (string.IsNullOrEmpty(strBarcode) == false
                    && this.DbType == "item")
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
                if (string.IsNullOrEmpty(strBarcode) == false
                    && this.DbType == "item")
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
                if (string.IsNullOrEmpty(strBarcode) == false
                    && this.DbType == "item")
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
                if (string.IsNullOrEmpty(strBarcode) == false
                    && this.DbType == "item")
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

            if (this.DbType == "item")
            {
                menuItem = new MenuItem("粘贴册条码号");
                menuItem.Click += new System.EventHandler(this.menu_pasteBarcodeFromClipboard_Click);
                if (bHasClipboardObject == false || bLooping == true)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAllLines_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

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
                    subMenuItem.Click += new System.EventHandler(this.menu_verifyRecord_Click);
                    if (this.listView_records.SelectedItems.Count == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItem.MenuItems.Add(subMenuItem);

                    // ---
                    subMenuItem = new MenuItem("-");
                    menuItem.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("分类统计 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&S)");
                    subMenuItem.Click += new System.EventHandler(this.menu_classStatis_Click);
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


                if (this.DbType == "issue")
                {
                    subMenuItem = new MenuItem("校验期记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&V)");
                    subMenuItem.Click += new System.EventHandler(this.menu_verifyRecord_Click);
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

                    subMenuItem = new MenuItem("校验订购记录 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&V)");
                    subMenuItem.Click += new System.EventHandler(this.menu_verifyRecord_Click);
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
                    if (nPathItemCount == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItemExport.MenuItems.Add(subMenuItem);
                }

                subMenuItem = new MenuItem("到记录路径文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&S)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToRecordPathFile_Click);
                if (nPathItemCount == 0 || bLooping == true)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("到文本文件 [" + nSelectedItemCount.ToString() + "] (&T)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportTextFile_Click);
                if (nSelectedItemCount == 0 || bLooping == true)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("到 Excel 文件 [" + nSelectedItemCount.ToString() + "] (&E)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportExcelFile_Click);
                if (nSelectedItemCount == 0 || bLooping == true)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);

                if (this.DbType == "item")
                {
                    subMenuItem = new MenuItem("到书目转储文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&B)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_exportBiblioDumpFile_Click);
                    if (nPathItemCount == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItemExport.MenuItems.Add(subMenuItem);
                }

                // ---
                subMenuItem = new MenuItem("-");
                menuItemExport.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("装入书目查询窗，将所从属的书目记录 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&B)...");
                subMenuItem.Click += new System.EventHandler(this.menu_exportToBiblioSearchForm_Click);
                if (nPathItemCount == 0 || bLooping == true)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);


                subMenuItem = new MenuItem("将所从属的书目记录路径归并后导出到(书目库)记录路径文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&B)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveToBiblioRecordPathFile_Click);
                if (nPathItemCount == 0 || bLooping == true)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);

                subMenuItem = new MenuItem("将所从属的书目记录导出到 MARC(ISO2709)文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&M)...");
                subMenuItem.Click += new System.EventHandler(this.menu_saveBiblioRecordToMarcFile_Click);
                if (nPathItemCount == 0 || bLooping == true)
                    subMenuItem.Enabled = false;
                menuItemExport.MenuItems.Add(subMenuItem);

                if (this.DbType == "order")
                {
                    // ---
                    subMenuItem = new MenuItem("-");
                    menuItemExport.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("将关联的已验收册记录路径导出到(册)记录路径文件 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&I)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_saveToAcceptItemRecordPathFile_Click);
                    if (nPathItemCount == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItemExport.MenuItems.Add(subMenuItem);
                }

                if (this.DbType == "item")
                {
                    // ---
                    subMenuItem = new MenuItem("-");

                    menuItemExport.MenuItems.Add(subMenuItem);
                    subMenuItem = new MenuItem("装入读者查询窗，将借者记录 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&B)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_exportToReaderSearchForm_Click);
                    if (nPathItemCount == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItemExport.MenuItems.Add(subMenuItem);

                    menuItemExport.MenuItems.Add(subMenuItem); subMenuItem = new MenuItem("输出到 Excel 文件，将借者记录 [" + (nPathItemCount == -1 ? "?" : nPathItemCount.ToString()) + "] (&R)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_exportToReaderExcelFile_Click);
                    if (nPathItemCount == 0 || bLooping == true)
                        subMenuItem.Enabled = false;
                    menuItemExport.MenuItems.Add(subMenuItem);

                }
            }

            MenuItem menuItemImport = new MenuItem("导入(&I)");
            contextMenu.MenuItems.Add(menuItemImport);

            {
                MenuItem subMenuItem = new MenuItem("从记录路径文件中导入(&I)...");
                subMenuItem.Click += new System.EventHandler(this.menu_importFromRecPathFile_Click);
                if (bLooping == true)
                    subMenuItem.Enabled = false;
                menuItemImport.MenuItems.Add(subMenuItem);

                if (this.DbType == "item")
                {
                    // ---
                    subMenuItem = new MenuItem("-");
                    menuItemImport.MenuItems.Add(subMenuItem);

                    subMenuItem = new MenuItem("从条码号文件中导入(&R)...");
                    subMenuItem.Click += new System.EventHandler(this.menu_importFromBarcodeFile_Click);
                    if (bLooping == true)
                        subMenuItem.Enabled = false;
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
            if (nSelectedItemCount == 0 || bLooping == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新书目摘要 [" + nSelectedItemCount.ToString() + "] (&R)");
            menuItem.Click += new System.EventHandler(this.menu_refreshSelectedItemsBiblioSummary_Click);
            if (nSelectedItemCount == 0 || bLooping == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
        }

        void menu_verifyRecord_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            delegate_verifyItemDom func = null;
            if (this.DbType == "order")
                func = VerifyOneOrder;
            else if (this.DbType == "issue")
                func = VerifyOneIssue;
            else if (this.DbType == "item")
                func = VerifyOneEntity;
            else
            {
                strError = "暂时不能处理 '" + this.DbType + "' 类型的记录校验";
                goto ERROR1;
            }

            nRet = VerifyItems(
                func,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            MessageBox.Show(this, "共处理 " + nRet.ToString() + " 个" + this.DbType + "记录");
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

        delegate void delegate_verifyItemDom(
            object param,
            LibraryChannel channel,
            string strItemRecPath,
            XmlDocument itemdom,
            List<string> errors,
            bool bAutoModify,
            ref bool bChanged);

        int VerifyItems(delegate_verifyItemDom func,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            bool bControl = Control.ModifierKeys == Keys.Control;

            // Debug.Assert(this.DbType == "order", "");

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要进行批处理的事项";
                return -1;
            }

            if (stop != null && stop.State == 0)    // 0 表示正在处理
            {
                strError = "目前有长操作正在进行，无法进行校验订购记录的操作";
                return -1;
            }

            VerifyEntityDialog dlg = new VerifyEntityDialog();
            MainForm.SetControlFont(dlg, this.Font);

            dlg.UiState = Program.MainForm.AppInfo.GetString(
                "ItemSearchForm",
                "VerifyEntityDialog_uiState",
                "");

            if (bControl == true)
                dlg.AutoModify = true;

            Program.MainForm.AppInfo.LinkFormState(dlg, "ItemSearchForm_VerifyEntityDialog_state");
            dlg.ShowDialog(this);

            Program.MainForm.AppInfo.SetString(
                "ItemSearchForm",
                "VerifyEntityDialog_uiState",
                dlg.UiState);

            // 切换到“操作历史”属性页
            Program.MainForm.ActivateFixPage("history");

            int nCount = 0;
            int nModifyCount = 0;

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
                + " 开始进行" + this.DbTypeCaption + "记录校验</div>");

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                return -1;
            }
            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行校验" + this.DbTypeCaption + "记录的操作 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

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

                bool bOldSource = true; // 是否要从 OldXml 开始做起

                if (dlg.AutoModify)
                {
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
                            strError = "放弃";
                            return 0;
                        }
                        if (result == DialogResult.No)
                        {
                            bOldSource = false;
                        }
                    }
                }

                ListViewPatronLoader loader = new ListViewPatronLoader(channel,
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

                    BiblioInfo info = item.BiblioInfo;

                    XmlDocument itemdom = new XmlDocument();
                    try
                    {
                        // itemdom.LoadXml(info.OldXml);
                        if (bOldSource == true)
                        {
                            itemdom.LoadXml(info.OldXml);
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
                                itemdom.LoadXml(info.NewXml);
                            else
                                itemdom.LoadXml(info.OldXml);
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "记录 '" + info.RecPath + "' 的 XML 装入 DOM 时出错: " + ex.Message;
                        return -1;
                    }

                    List<string> errors = new List<string>();
                    bool bChanged = false;

                    try
                    {
                        func(dlg, channel, info.RecPath, itemdom, errors, dlg.AutoModify, ref bChanged);
                    }
                    catch (Exception ex)
                    {
                        strError = ex.Message;
                        return -1;
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

                    if (bChanged == true)
                    {
                        string strXml = itemdom.OuterXml;
                        Debug.Assert(info != null, "");
                        if (info != null)
                        {
                            if (string.IsNullOrEmpty(info.NewXml) == true)
                                this.m_nChangedCount++;
                            info.NewXml = strXml;
                            // TODO: new 和 old 对比，看 borrower 等敏感元素是否被修改。如果发生了修改，要提示用 force 方式保存，否则这些字段的修改在保存阶段会被忽略。
                        }

                        item.ListViewItem.BackColor = SystemColors.Info;
                        item.ListViewItem.ForeColor = SystemColors.InfoText;
                        nModifyCount++;
                    }

                    nCount++;
                    stop.SetProgressValue(++i);
                }

                return nCount;
            }
            finally
            {
                this.EnableControls(true);

                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
                    + " 结束执行" + this.DbTypeCaption + "记录校验</div>");
                if (nModifyCount > 0)
                    Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
                        + " 发生修改 " + nModifyCount + " 条</div>");
            }
        }

        // 校验一条订购记录
        void VerifyOneOrder(
            object param,
            LibraryChannel channel,
            string strItemRecPath,
            XmlDocument itemdom,
            List<string> errors,
            bool bAutoModify,
            ref bool bChanged)
        {
            string strError = "";
            int nRet = 0;

            // 检查根元素下的元素名是否有重复的
            nRet = VerifyDupElementName(itemdom,
                bAutoModify,
                ref bChanged,
                out strError);
            if (nRet == -1)
                errors.Add(strError);

            // 校验 XML 记录中是否有非法字符
            if (itemdom.DocumentElement != null)
            {
                string strXml = itemdom.DocumentElement.OuterXml;
                string strReplaced = DomUtil.ReplaceControlCharsButCrLf(strXml, '*');
                if (strReplaced != strXml)
                {
                    errors.Add("XML 记录中有非法字符");
                }
            }

            nRet = VerifyRefID(itemdom,
                bAutoModify,
                ref bChanged,
                out strError);
            if (nRet == -1)
            {
                errors.Add(strError);
            }

            nRet = VerifyOrder(
                strItemRecPath,
                itemdom,
                bAutoModify,
                ref bChanged,
                out strError);
            if (nRet == -1)
                errors.Add(strError);

            // 顺便清除空元素
            if (bChanged)
            {
                DomUtil.RemoveEmptyElements(itemdom.DocumentElement, false);
            }
        }

        // 校验一条期记录
        void VerifyOneIssue(
            object param,
            LibraryChannel channel,
            string strItemRecPath,
            XmlDocument itemdom,
            List<string> errors,
            bool bAutoModify,
            ref bool bChanged)
        {
            string strError = "";
            int nRet = 0;

            // 检查根元素下的元素名是否有重复的
            nRet = VerifyDupElementName(itemdom,
                bAutoModify,
                ref bChanged,
                out strError);
            if (nRet == -1)
                errors.Add(strError);

            // 校验 XML 记录中是否有非法字符
            if (itemdom.DocumentElement != null)
            {
                string strXml = itemdom.DocumentElement.OuterXml;
                string strReplaced = DomUtil.ReplaceControlCharsButCrLf(strXml, '*');
                if (strReplaced != strXml)
                {
                    errors.Add("XML 记录中有非法字符");
                }
            }

#if NO
            nRet = VerifyRefID(itemdom,
                bAutoModify,
                ref bChanged,
                out strError);
            if (nRet == -1)
            {
                errors.Add(strError);
            }
#endif

            // 检查根元素下的元素名是否有重复的
            nRet = VerifyDupElementName(itemdom,
                bAutoModify,
                ref bChanged,
                out strError);
            if (nRet == -1)
                errors.Add(strError);

            string strParentID = DomUtil.GetElementText(itemdom.DocumentElement, "parent");
            if (string.IsNullOrEmpty(strParentID))
            {
                errors.Add("缺乏 parent 元素");
            }

            {
                string strBiblioRecPath = Program.MainForm.BuildBiblioRecPath(
                    this.DbType,
                    strItemRecPath,
                    strParentID);
                if (string.IsNullOrEmpty(strBiblioRecPath))
                {
                    errors.Add("获取对应的书目记录路径时出错");
                }

                nRet = VerifyIssue(
                    channel,
                    strBiblioRecPath,
                    itemdom,
                    bAutoModify,
                    ref bChanged,
                    out strError);
                if (nRet == -1)
                    errors.Add(strError);
            }

        }

        // 校验一条册记录
        void VerifyOneEntity(
            object param,
            LibraryChannel channel,
            string strItemRecPath,
            XmlDocument itemdom,
            List<string> errors,
            bool bAutoModify,
            ref bool bChanged)
        {
            string strError = "";
            int nRet = 0;

            VerifyEntityDialog dlg = (VerifyEntityDialog)param;

            // 模拟删除一些元素
            if (itemdom.DocumentElement != null)
            {
                nRet = SimulateDeleteElement(itemdom.OuterXml,
    out strError);
                if (nRet == -1)
                    errors.Add(strError);
            }

            // 检查根元素下的元素名是否有重复的
            nRet = VerifyDupElementName(itemdom,
                bAutoModify,
                ref bChanged,
                out strError);
            if (nRet == -1)
                errors.Add(strError);

#if NO
            nRet = RestoreFromComment(itemdom,
    bAutoModify,
    ref bChanged,
    out strError);
            if (nRet == -1)
                errors.Add(strError);
#endif

            // 校验 XML 记录中是否有非法字符
            if (itemdom.DocumentElement != null)
            {
                string strXml = itemdom.DocumentElement.OuterXml;
                string strReplaced = DomUtil.ReplaceControlCharsButCrLf(strXml, '*');
                if (strReplaced != strXml)
                {
                    errors.Add("XML 记录中有非法字符");
                }
            }

            nRet = VerifyRefID(itemdom,
                bAutoModify,
                ref bChanged,
                out strError);
            if (nRet == -1)
            {
                errors.Add(strError);
            }

            nRet = VerifyBlankChar(itemdom,
                new List<string>() { "barcode", "registerNo" },
    bAutoModify,
    ref bChanged,
    out strError);
            if (nRet == -1)
            {
                errors.Add(strError);
            }

#if NO
            // 检查根元素下的元素名是否有重复的
            nRet = VerifyDupElementName(itemdom,
                bAutoModify,
                ref bChanged,
                out strError);
            if (nRet == -1)
                errors.Add(strError);
#endif

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

            if (dlg.VerifyItemBarcode == true
                && string.IsNullOrEmpty(strBarcode) == false)
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
                nRet = Program.MainForm.VerifyBarcode(
this.stop,
channel,
strLibraryCode,
strBarcode,
null,
out strError);
                if (nRet == -2)
                {
                    // throw new Exception(strError);
                    errors.Add(strError);   // TODO: 是否可以统一报错(不要每个册都报错)?
                }

                if (nRet != 2)
                {
                    if (nRet == 1 && string.IsNullOrEmpty(strError) == true)
                        strError = strLibraryCode + ": 这看起来是一个证条码号";

                    errors.Add("册条码号 '" + strBarcode + "' 不合法: " + strError);
                }
            }

            //// 模拟删除

            // 顺便清除空元素
            if (bChanged)
            {
                DomUtil.RemoveEmptyElements(itemdom.DocumentElement, false);
            }
        }

        // return:
        //      -1  发现错误
        //      0   没有发现错误
        int VerifyBlankChar(XmlDocument dom,
            List<string> element_names,
            bool bModify,
            ref bool bChanged,
            out string strError)
        {
            strError = "";

            if (dom.DocumentElement == null)
            {
                strError = "XML 记录为空";
                return -1;
            }

            List<string> errors = new List<string>();
            foreach (string element_name in element_names)
            {
                string strValue = DomUtil.GetElementText(dom.DocumentElement, element_name);
                if (string.IsNullOrEmpty(strValue) == false)
                {
                    string strTrimed = strValue.Trim();
                    if (strTrimed == strValue)
                        continue;

                    errors.Add(element_name + "元素内文本 '" + strValue + "' 中包含空格字符");
                    if (bModify)
                    {
                        DomUtil.SetElementText(dom.DocumentElement, element_name, strTrimed);
                        bChanged = true;
                    }
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return -1;
            }
            return 0;
        }

        // return:
        //      -1  发现错误
        //      0   没有发现错误
        int VerifyRefID(XmlDocument dom,
            bool bModify,
            ref bool bChanged,
            out string strError)
        {
            strError = "";

            if (dom.DocumentElement == null)
            {
                strError = "XML 记录为空";
                return -1;
            }

            string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strRefID))
            {
                if (bModify)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "refID", Guid.NewGuid().ToString());
                    bChanged = true;
                }
                strError = "refID 元素为空";
                return -1;
            }

            return 0;
        }

        // return:
        //      0   没有发现错误
        //      -1  发现错误
        int VerifyOrder(
            string strOrderRecPath,
            XmlDocument dom,
            bool bModify,
            ref bool bChanged,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (dom.DocumentElement == null)
            {
                strError = "XML 记录为空";
                return -1;
            }

            List<string> errors = new List<string>();

            string strOrderTime = DomUtil.GetElementText(dom.DocumentElement, "orderTime");
            if (string.IsNullOrEmpty(strOrderTime) == false)
            {
                strError = "";
                try
                {
                    DateTimeUtil.FromRfc1123DateTimeString(strOrderTime);
                }
                catch (Exception ex)
                {
                    strError = "订购日期字符串 '" + strOrderTime + "' 格式错误: " + ex.Message;
                }

                if (string.IsNullOrEmpty(strError) == false)
                {
                    errors.Add(strError);

                    if (bModify)
                    {
#if NO
                        // 尝试纠正
                        DateTime time;
                        if (strOrderTime.Length == 8)
                        {
                            try
                            {
                                time = DateTimeUtil.Long8ToDateTime(strOrderTime);
                                DomUtil.SetElementText(dom.DocumentElement, "orderTime", DateTimeUtil.Rfc1123DateTimeStringEx(time));
                                bChanged = true;
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            if (DateTime.TryParse(strOrderTime, out time) == true)
                            {
                                DomUtil.SetElementText(dom.DocumentElement, "orderTime", DateTimeUtil.Rfc1123DateTimeStringEx(time));
                                bChanged = true;
                            }
                        }
#endif
                        string strRfc1123 = "";
                        // 尝试解析混乱的时间字符串
                        // return:
                        //      false   解析失败
                        //      true    解析成功
                        if (TryParseTimeString(strOrderTime, out strRfc1123) == true)
                        {
                            DomUtil.SetElementText(dom.DocumentElement, "orderTime", strRfc1123);
                            bChanged = true;
                        }

                    }
                }
            }

            string strRange = DomUtil.GetElementText(dom.DocumentElement, "range");
            if (string.IsNullOrEmpty(strRange) == false)
            {
                string strDbName = Global.GetDbName(strOrderRecPath);
                string strPubType = Program.MainForm.GetPubTypeFromOrderDbName(strDbName);

                bool bError = false;
                // 检查单个出版日期字符串是否合法
                // return:
                //      -1  出错
                //      0   正确
                nRet = LibraryServerUtil.CheckPublishTimeRange(strRange,
                    strPubType == "book" ? true : false,
                    out strError);
                if (nRet == -1)
                {
                    strError = "时间范围字符串 '" + strRange + "' 格式错误: " + strError;
                    errors.Add(strError);
                    bError = true;
                }

                if (bError && bModify)
                {
                    if (strPubType == "book")
                    {
                        // 图书类型，干脆清除 range 元素 
                        DomUtil.DeleteElement(dom.DocumentElement, "range");
                        bChanged = true;
                    }
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return -1;
            }
            return 0;
        }

        // 尝试解析混乱的时间字符串
        // return:
        //      false   解析失败
        //      true    解析成功
        static bool TryParseTimeString(string strOrderTime, out string strRfc1123)
        {
            strRfc1123 = "";

            try
            {
                // 尝试纠正
                DateTime time;
                if (strOrderTime.Length == 8)
                {
                    time = DateTimeUtil.Long8ToDateTime(strOrderTime);
                    strRfc1123 = DateTimeUtil.Rfc1123DateTimeStringEx(time);
                    return true;

                }
                else if (strOrderTime.Length == 4)
                {
                    time = new DateTime(Convert.ToInt32(strOrderTime), 1, 1);
                    strRfc1123 = DateTimeUtil.Rfc1123DateTimeStringEx(time);
                    return true;
                }
                else if (strOrderTime.Length == 6)
                {
                    time = new DateTime(Convert.ToInt32(strOrderTime.Substring(0, 4)),
                        Convert.ToInt32(strOrderTime.Substring(4)),
                        1);
                    strRfc1123 = DateTimeUtil.Rfc1123DateTimeStringEx(time);
                    return true;
                }
                else
                {
                    if (DateTime.TryParse(strOrderTime, out time) == true)
                    {
                        strRfc1123 = DateTimeUtil.Rfc1123DateTimeStringEx(time);
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // return:
        //      0   没有发现错误
        //      -1  发现错误
        int VerifyIssue(
            LibraryChannel channel,
            string strBiblioRecPath,
            XmlDocument dom,
            bool bModify,
            ref bool bChanged,
            out string strError)
        {
            strError = "";

            if (dom.DocumentElement == null)
            {
                strError = "XML 记录为空";
                return -1;
            }

            List<string> errors = new List<string>();

            List<string> refids = new List<string>();   // 内嵌订购记录的 refid

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("orderInfo/*");
            foreach (XmlElement record in nodes)
            {
                string strRefID = DomUtil.GetElementText(record, "refID");
                if (string.IsNullOrEmpty(strRefID) == true)
                {
                    strError = "期记录中内嵌的订购记录缺乏 refID 元素";
                    errors.Add(strError);
                    // continue;
                }
                else
                {
                    refids.Add(strRefID);
                    continue;
                }

                if (IsZero(record) == true)
                {
                    strError = "期记录中内嵌的订购记录出现了废弃的 0 copy 部分";
                    errors.Add(strError);

                    if (bModify)
                    {
                        record.ParentNode.RemoveChild(record);
                        bChanged = true;
                        continue;
                    }
                }

                if (bModify && string.IsNullOrEmpty(strBiblioRecPath) == false)
                {
                    List<PathAndDom> results = null;

                    // 检索相关的订购记录
                    // parameters:
                    //      nest_order  期记录里面内嵌的订购记录的根元素
                    //      results   返回检索命中并匹配上 nest_order 特征的一个或者多个订购记录
                    int nRet = SearchRelationOrder(
                        channel,
                        strBiblioRecPath,
                        record,
                        out results,
                        out strError);
                    if (nRet == -1)
                    {
                        errors.Add("试图修正(期记录内嵌 refID)记录时出错: " + strError);
                        continue;
                    }

                    if (results.Count == 0)
                    {
                        errors.Add("试图修正(期记录内嵌 refID)记录时没有匹配上源订购记录");
                        continue;
                    }
                    else if (results.Count == 1)
                    {
                        PathAndDom obj = results[0];
                        strRefID = DomUtil.GetElementText(obj.Dom.DocumentElement, "refID");
                        if (string.IsNullOrEmpty(strRefID))
                        {
                            errors.Add("试图修正(期记录内嵌 refID)记录时发现源订购记录(" + obj.RecPath + ")也没有 refID 元素。请先对所有订购记录进行修正，再执行期记录修正");
                            continue;
                        }

                        if (refids.IndexOf(strRefID) != -1)
                        {
                            errors.Add("试图修正(期记录内嵌 refID)记录时，所匹配的源订购记录(" + obj.RecPath + ")的 refID 元素(值'" + strRefID + "')和其他内嵌订购记录的 refID 元素发生了重复，因此无法修复此问题");
                            continue;
                        }
#if NO
                        // 测试
                        string strOldRefID = DomUtil.GetElementText(record, "refID");
                        if (strRefID != strOldRefID)
                        {
                            errors.Add("验证搜寻订购记录时候，发现命中的 refID 不一致");
                            continue;
                        }
#endif

                        DomUtil.SetElementText(record, "refID", strRefID);
                        {
                            // 写入 comment 元素
                            List<string> comments = new List<string>();
                            string strOldComment = DomUtil.GetElementText(record, "comment");
                            if (string.IsNullOrEmpty(strOldComment) == false)
                                comments.Add(strOldComment);
                            comments.Add(DateTime.Now.ToString() + " 由程序填写 refID 元素");
                            DomUtil.SetElementText(record, "comment", StringUtil.MakePathList(comments, ";"));
                        }
                        bChanged = true;
                    }
                    else if (results.Count > 1)
                    {
                        errors.Add("试图修正(期记录内嵌 refID)记录时发现匹配的源订购记录多于一条。请手动执行修正吧");
                        continue;
                    }

                }
            }

            // 检查内嵌参考 ID 是否发生了重复
            if (refids.Count > 0)
            {
                refids.Sort();
                if (StringUtil.HasDup(refids) == true)
                    errors.Add("期记录内嵌的(多个)订购记录中参考 ID 之间发生了重复");
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return -1;
            }
            return 0;
        }

        class PathAndDom
        {
            public string RecPath { get; set; }
            public XmlDocument Dom { get; set; }
        }

        // 检索相关的订购记录
        // parameters:
        //      nest_order  期记录里面内嵌的订购记录的根元素
        //      results   返回检索命中并匹配上 nest_order 特征的一个或者多个订购记录
        int SearchRelationOrder(
            LibraryChannel channel,
            string strBiblioRecPath,
            XmlElement nest_order,
            out List<PathAndDom> results,
            out string strError)
        {
            strError = "";
            results = new List<PathAndDom>();

            string strSourceRange = DomUtil.GetElementText(nest_order, "range");
            string strSource = BuildCompareString(nest_order);

            List<PathAndDom> samerange_list = new List<PathAndDom>(); // range 相同的那些记录

            // 装入订购记录
            SubItemLoader sub_loader = new SubItemLoader();
            sub_loader.BiblioRecPath = strBiblioRecPath;
            sub_loader.Channel = channel;
            sub_loader.Stop = stop;
            sub_loader.DbType = "order";

            sub_loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
            sub_loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

            // 第一次循环，将 range 相同的记录记录下来
            foreach (EntityInfo info in sub_loader)
            {
                if (info.ErrorCode != ErrorCodeValue.NoError)
                {
                    strError = "路径为 '" + info.OldRecPath + "' 的册记录装载中发生错误: " + info.ErrorInfo;  // NewRecPath
                    return -1;
                }

                if (string.IsNullOrEmpty(info.OldRecord))
                    continue;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "订购记录装入 XMLDOM 时出错: " + ex.Message;
                    return -1;
                }

                string strRange = DomUtil.GetElementText(dom.DocumentElement, "range");
                if (strRange == strSourceRange)
                {
                    PathAndDom obj = new PathAndDom();
                    obj.RecPath = info.OldRecPath;
                    obj.Dom = dom;
                    samerange_list.Add(obj);
                }
            }

            if (nest_order.ParentNode != null)
            {
                XmlNodeList nodes = nest_order.ParentNode.SelectNodes("*");
                if (nodes.Count == 1 && samerange_list.Count == 1)
                {
                    // 如果期记录内的内嵌订购记录只有一个，而且所有订购记录中符合 range 的也只有一个，那就是它了。不必进行精确匹配了
                    results.Add(samerange_list[0]);
                    return results.Count;
                }
            }

            // 第二次循环，进行比较精确的字段匹配
            foreach (PathAndDom obj in samerange_list)
            {
                string strCompare = BuildCompareString(obj.Dom.DocumentElement);
                if (strSource == strCompare)
                    results.Add(obj);
            }

            // TODO: 如果一个符合的也没有找到，要出现对话框让用户自己选择

            return results.Count;
        }

        // 是否为空的、无用的订购组信息
        static bool IsZero(XmlElement nest_order)
        {
            string strCopy = DomUtil.GetElementText(nest_order, "copy");

            string strOldValue = "";
            string strNewValue = "";

            // 分离 "old[new]" 内的两个值
            OrderDesignControl.ParseOldNewValue(strCopy,
                out strOldValue,
                out strNewValue);
            if ((string.IsNullOrEmpty(strOldValue) == true || strOldValue == "0")
                && (string.IsNullOrEmpty(strNewValue) == true || strNewValue == "0"))
                return true;
            return false;
        }

        // 构造一个用于比较订购记录关键字段的字符串
        static string BuildCompareString(XmlElement root)
        {
            string strRange = DomUtil.GetElementText(root, "range");
            //string strParent = DomUtil.GetElementText(root, "parent");
            //string strBatchNo = DomUtil.GetElementText(root, "batchNo");
            //string strCatalogNo = DomUtil.GetElementText(root, "catalogNo");
            string strSeller = DomUtil.GetElementText(root, "seller");
            string strSource = GetOldPart(DomUtil.GetElementText(root, "source"));
            string strCopy = GetOldPart(DomUtil.GetElementText(root, "copy"));
            string strPrice = GetOldPart(DomUtil.GetElementText(root, "price"));
            string strIssueCount = GetOldPart(DomUtil.GetElementText(root, "issueCount"));
            // string strDistribute = DomUtil.GetElementText(root, "distribute");

            return strRange
                //+ "|" + strParent
                //+ "|" + strBatchNo
                //+ "|" + strCatalogNo
                + "|" + strSeller
                + "|" + strSource
                + "|" + strCopy
                + "|" + strPrice
                + "|" + strIssueCount;
        }

        // 获得新旧值的新部分
        static string GetOldPart(string strValue)
        {
            string strOldValue = "";
            string strNewValue = "";

            // 分离 "old[new]" 内的两个值
            OrderDesignControl.ParseOldNewValue(strValue,
                out strOldValue,
                out strNewValue);

            return strOldValue;
        }



        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
            if (e.Actions == "yes,no,cancel")
            {
                DialogResult result = AutoCloseMessageBox.Show(this,
    e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
    20 * 1000,
    "ItemSearchForm");
                if (result == DialogResult.Cancel)
                    e.ResultAction = "no";
                else
                    e.ResultAction = "yes";
            }
        }

        // return:
        //      -1  有错
        //      0   没有错
        int SimulateDeleteElement(string strXml,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strXml))
                return 0;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

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

        /*
         * 2017/5/4 18:17:17 整理前的 2 个重复元素: ; 1) <borrower></borrower>; 2) <borrower>DZ0567</borrower>。整理前的 2 个重复元素: ; 1) <borrowDate></borrowDate>; 2) <borrowDate>Tue, 31 Dec 2019 16:00:00 GMT</borrowDate>。整理前的 2 个重复元素: ; 1) <borrowPeriod></borrowPeriod>; 2) <borrowPeriod>1day</borrowPeriod>
         * * */
        int RestoreFromComment(XmlDocument dom,
    bool bModify,
    ref bool bChanged,
    out string strError)
        {
            strError = "";
            if (dom.DocumentElement == null)
                return 0;

            string strComment = DomUtil.GetElementText(dom.DocumentElement, "comment");
            if (string.IsNullOrEmpty(strComment))
                return 0;

            List<string> errors = new List<string>();

            // 把 comment 元素中的内容构造回一个临时 XmlDocument
            XmlDocument tempdom = new XmlDocument();
            tempdom.LoadXml("<root />");

            List<string> segs = StringUtil.SplitList(strComment.Replace("。", ";"), ";");
            foreach (string seg in segs)
            {
                if (string.IsNullOrEmpty(seg))
                    continue;
                string strLine = seg.Trim();
                int nRet = strLine.IndexOf(")");
                if (nRet == -1)
                    continue;
                strLine = strLine.Substring(nRet + 1).Trim();

                XmlDocumentFragment frag = tempdom.CreateDocumentFragment();
                frag.InnerXml = strLine;

                tempdom.DocumentElement.AppendChild(frag);
            }

            XmlNodeList nodes1 = tempdom.DocumentElement.SelectNodes("*");
            if (nodes1.Count > 0)
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);

                List<XmlElement> deletes = new List<XmlElement>();
                List<string> processed_names = new List<string>();
                foreach (XmlNode node in tempdom.DocumentElement.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        // dprms:file 元素是允许重复的
                        if (node.NamespaceURI == DpNs.dprms && node.LocalName == "file")
                            continue;

                        if (string.IsNullOrEmpty(node.Prefix) == false)
                            nsmgr.AddNamespace(node.Prefix, node.NamespaceURI);

                        XmlNodeList nodes = tempdom.DocumentElement.SelectNodes(node.Name, nsmgr);
                        if (nodes.Count > 1 && processed_names.IndexOf(node.Name) == -1)
                        {
                            // 保证了不重复处理
                            processed_names.Add(node.Name);

                            // 挑选一个加以保留，其他的删除
                            deletes.AddRange(RemoveDupNodes(nodes));
                        }
                    }
                }

                if (deletes.Count > 0)
                {
                    foreach (XmlElement node in deletes)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                }

                if (bModify)
                {
                    XmlNodeList origins = tempdom.DocumentElement.SelectNodes("*");
                    foreach (XmlElement origin in origins)
                    {
                        string strSource = origin.OuterXml.Trim();
                        XmlElement target = dom.DocumentElement.SelectSingleNode(origin.Name) as XmlElement;
                        if (target == null)
                        {
                            if (string.IsNullOrEmpty(strSource) == false)
                            {
                                DomUtil.SetElementText(dom.DocumentElement, origin.Name, "test");
                                DomUtil.SetElementOuterXml(target, strSource);
                                bChanged = true;
                                errors.Add("元素 " + origin.Name + " 被创建: " + strSource);
                            }

                        }
                        else
                        {
                            if (string.IsNullOrEmpty(strSource))
                            {
                                target.ParentNode.RemoveChild(target);
                                bChanged = true;
                                errors.Add("元素 " + origin.Name + " 被删除");
                            }
                            else
                            {
                                if (target.OuterXml.Trim() != strSource)
                                {
                                    DomUtil.SetElementOuterXml(target, strSource);
                                    bChanged = true;
                                    errors.Add("元素 " + origin.Name + " 被覆盖: " + strSource);
                                }
                            }
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return -1;
            }

            return 0;
        }

        int VerifyDupElementName(XmlDocument dom,
            bool bModify,
            ref bool bChanged,
            out string strError)
        {
            strError = "";
            if (dom.DocumentElement == null)
                return 0;

            StringBuilder comment = new StringBuilder();
            List<XmlElement> deletes = new List<XmlElement>();
            List<string> processed_names = new List<string>();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);

            List<string> errors = new List<string>();
            foreach (XmlNode node in dom.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element
                    // && string.IsNullOrEmpty(node.Prefix) == true
                    )
                {
                    // dprms:file 元素是允许重复的
                    if (node.NamespaceURI == DpNs.dprms && node.LocalName == "file")
                        continue;

                    if (string.IsNullOrEmpty(node.Prefix) == false)
                        nsmgr.AddNamespace(node.Prefix, node.NamespaceURI);

                    XmlNodeList nodes = dom.DocumentElement.SelectNodes(node.Name, nsmgr); // 2016/11/18 修改 bug
                    if (nodes.Count > 1 && processed_names.IndexOf(node.Name) == -1)
                    {
                        errors.Add("根元素下的 " + node.Name + " 元素出现了多次 " + nodes.Count);

                        // 保证了不重复处理
                        processed_names.Add(node.Name);

                        if (bModify)
                        {
                            // 保留整理前的状态
                            if (comment.Length > 0)
                                comment.Append("。");
                            comment.Append("整理前的 " + BuildComment(nodes));

                            // 挑选一个加以保留，其他的删除
                            deletes.AddRange(RemoveDupNodes(nodes));
                        }
                    }
                }
            }

            if (bModify)
            {
                if (deletes.Count > 0)
                {
                    foreach (XmlElement node in deletes)
                    {
                        node.ParentNode.RemoveChild(node);
                        bChanged = true;
                    }

                    // 记入 comment 元素
                    if (comment.Length > 0)
                    {
                        string strOldComment = DomUtil.GetElementText(dom.DocumentElement, "comment");
                        if (string.IsNullOrEmpty(strOldComment) == false)
                            strOldComment += "。";
                        DomUtil.SetElementText(dom.DocumentElement,
                            "comment",
                            strOldComment + DateTime.Now.ToString() + " " + comment.ToString());
                        bChanged = true;
                    }
                }

            }

            if (errors.Count == 0)
                return 0;

            strError = StringUtil.MakePathList(errors, "; ");
            return -1;
        }

        class DeleteItem
        {
            public int Index { get; set; }  // 事项原先的序号
            public XmlElement Element { get; set; } // XML 元素
            public int Length { get; set; } // InnerXml 部分长度
        }

        static string BuildComment(XmlNodeList nodes)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            text.Append(nodes.Count.ToString() + " 个重复元素: ");
            foreach (XmlElement node in nodes)
            {
                if (text.Length > 0)
                    text.Append("; ");
                text.Append((i + 1).ToString() + ") " + node.OuterXml);
                i++;
            }

            return text.ToString();
        }

        static List<XmlElement> RemoveDupNodes(XmlNodeList nodes)
        {
            List<XmlElement> results = new List<XmlElement>();
            // 除了第一个，后面都纳入返回
            int i = 0;
            foreach (XmlElement node in nodes)
            {
                if (i > 0)
                    results.Add(node);

                i++;
            }

            return results;
        }

#if NO
        static List<XmlElement> RemoveDupNodes(XmlNodeList nodes)
        {
            List<XmlElement> results = new List<XmlElement>();
            List<XmlElement> rests = new List<XmlElement>();
            // 删除空元素
            foreach (XmlElement node in nodes)
            {
                if (String.IsNullOrEmpty(node.InnerXml.Trim()))
                    results.Add(node);
                else
                    rests.Add(node);
            }

            // 已经删除了只剩下一个了，结束处理
            if (results.Count >= nodes.Count - 1)
                return results;

            List<DeleteItem> delete = new List<DeleteItem>();
            // 按照下级 XML 尺寸排序，最大的保留，其余删除
            int index = 0;
            foreach (XmlElement node in rests)
            {
                DeleteItem item = new DeleteItem();
                item.Element = node;
                item.Length = node.InnerXml.Trim().Length;
                item.Index = index++;
                delete.Add(item);
            }

            // 排序。若内容长度一样，则 index 小的在前。这是因为假定更新的修改在 XML 结构中会靠前
            delete.Sort((x, y) =>
            {
                int nRet = 0;
                if (x.Length > 0 && y.Length > 0)
                    nRet = x.Index - y.Index;   // 序号小在前
                if (nRet != 0)
                    return nRet;
                return -1 * (x.Length - y.Length);  // 长度长的在前
            });

            if (delete.Count > 0)
                delete.RemoveAt(0); // 去掉最靠前的一个，也就是不应该删除的一个

            foreach (DeleteItem item in delete)
            {
                results.Add(item.Element);
            }

            return results;
        }
#endif

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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                return -1;
            }
            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行" + strOperName + "操作 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
#if NO
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
#endif

                // 检查前端权限
                if (StringUtil.IsInList("client_simulateborrow", this.CurrentRights) == false)
                {
                    strError = "当前用户不具备 client_simulateborrow 权限，无法进行模拟" + strOperName + "的操作";
                    return -1;
                }

                // 打开一个新的快捷出纳窗
                QuickChargingForm form = new QuickChargingForm();
                form.MdiParent = Program.MainForm;
                form.MainForm = Program.MainForm;
                form.Show();

                string strReaderBarcode = "";

                if (strAction == "borrow")
                {
                    strReaderBarcode = InputDlg.GetInput(
         this,
         "批处理" + strOperName,
         "请输入读者证条码号:",
         "",
         Program.MainForm.DefaultFont);
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
Program.MainForm.DefaultFont);
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

        int GetSelectedBiblioRecPath(
            LibraryChannel channel,
            out List<string> biblio_recpaths,
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

                if (stop != null && stop.State != 0)
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
                nRet = GetBiblioRecPath(
                    channel,
                    item,
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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
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
                form.MdiParent = Program.MainForm;
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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
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
                form.MdiParent = Program.MainForm;
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
            form.MdiParent = Program.MainForm;
            form.Show();

            int nWarningLineCount = 0;
            int nDupCount = 0;

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装入记录到书目查询窗 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

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
                nRet = GetSelectedBiblioRecPath(
                    channel,
                    out biblio_recpaths,
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

                this.ReturnChannel(channel);

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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除" + this.DbTypeCaption + "记录 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

            this.EnableControls(false);
            this.listView_records.Enabled = false;
            try
            {
                stop.SetProgressRange(0, items.Count);

                ListViewPatronLoader loader = new ListViewPatronLoader(channel,
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
                        lRet = channel.SetEntities(
                             stop,
                             strBiblioRecPath,
                             entities,
                             out errorinfos,
                             out strError);
                    }
                    else if (this.DbType == "order")
                    {
                        lRet = channel.SetOrders(
        stop,
        strBiblioRecPath,
        entities,
        out errorinfos,
        out strError);
                    }
                    else if (this.DbType == "issue")
                    {
                        lRet = channel.SetIssues(
        stop,
        strBiblioRecPath,
        entities,
        out errorinfos,
        out strError);
                    }
                    else if (this.DbType == "comment")
                    {
                        lRet = channel.SetComments(
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
                this.EnableControls(true);
                this.listView_records.Enabled = true;

                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;
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
                dlg.MainForm = Program.MainForm;
                dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
                dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

                Program.MainForm.AppInfo.LinkFormState(dlg, "itemsearchform_quickchange"+this.DbType+"dialog_state");
                dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(dlg);

                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;

                actions = dlg.Actions;
                cfg_dom = dlg.CfgDom;
            }

            DateTime now = DateTime.Now;

            // TODO: 检查一下，看看是否一项修改动作都没有
            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行快速修改" + this.DbTypeCaption + "记录</div>");

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

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束快速修改" + this.DbTypeCaption + "记录</div>");
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
            string strStateAction = Program.MainForm.AppInfo.GetString(
                "change_order_param",
                "state",
                "<不改变>");
            if (strStateAction != "<不改变>")
            {
                string strState = DomUtil.GetElementText(dom.DocumentElement,
                    "state");

                if (strStateAction == "<增、减>")
                {
                    string strAdd = Program.MainForm.AppInfo.GetString(
                "change_order_param",
                "state_add",
                "");
                    string strRemove = Program.MainForm.AppInfo.GetString(
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
            string strFieldName = Program.MainForm.AppInfo.GetString(
"change_order_param",
"field_name",
"<不使用>");

            if (strFieldName != "<不使用>")
            {
                string strFieldValue = Program.MainForm.AppInfo.GetString(
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
            string strFilename = PathUtil.MergePath(Program.MainForm.DataDir, "~orderrecpath.txt");
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
            // form.MainForm = Program.MainForm;
            form.MdiParent = Program.MainForm;
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
            string strFilename = PathUtil.MergePath(Program.MainForm.DataDir, "~orderrecpath.txt");
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
            // form.MainForm = Program.MainForm;
            form.MdiParent = Program.MainForm;
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
                    string strBiblioDbName = Program.MainForm.GetBiblioDbNameFromOrderDbName(strOrderDbName);
                    strIssueDbName = Program.MainForm.GetIssueDbName(strBiblioDbName);
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
            form.MdiParent = Program.MainForm;
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
            string strFilename = PathUtil.MergePath(Program.MainForm.DataDir, "~itemrecpath.txt");
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
            // form.MainForm = Program.MainForm;
            form.MdiParent = Program.MainForm;
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

            Program.MainForm.StatusBarMessage = "册记录路径 " + nRet.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportItemRecPathFilename;
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
                if (stop.IsInLoop == true)
                {
                    strError = "无法重复进入循环";
                    return -1;
                }
                stop.Style = StopStyle.EnableHalfStop;
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在导出已验收的册记录路径 ...");
                stop.BeginLoop();

                LibraryChannel channel = this.GetChannel();

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
                        int nRet = LoadOrderItem(
                            channel,
                            item.Text,
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
                    this.EnableControls(true);

                    this.ReturnChannel(channel);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                    stop.HideProgress();
                    stop.Style = StopStyle.None;
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
        int LoadOrderItem(
            LibraryChannel channel,
            string strRecPath,
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
            nRet = Program.MainForm.IsSeriesTypeFromOrderDbName(Global.GetDbName(strRecPath));
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

                        lRet = channel.GetItemInfo(
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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建索取号 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

            this.EnableControls(false);
            try
            {
                // 打开一个新的种册窗
                EntityForm form = null;

                form = new EntityForm();

                form.MdiParent = Program.MainForm;

                form.MainForm = Program.MainForm;
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
                            channel,
                            out strError);
                        if (nRet == -1)
                            goto ERROR;

                        nRet = RefreshBrowseLine(
                            channel,
                            item,
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
                            Form active_mdi = Program.MainForm.ActiveMdiChild;

                            itemsearchform = new ItemSearchForm();
                            itemsearchform.MdiParent = Program.MainForm;
                            itemsearchform.MainForm = Program.MainForm;
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
                this.EnableControls(true);

                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;
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
                return this.tabComboBox_queryWord.Text;
            }
            set
            {
                this.tabComboBox_queryWord.Text = value;
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
                    m_strTempQuickBarcodeFilename = PathUtil.MergePath(Program.MainForm.DataDir, "~" + Guid.NewGuid().ToString());
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
                // form.MainForm = Program.MainForm;
                form.MdiParent = Program.MainForm;
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

            LibraryChannel channel = this.GetChannel();
            try
            {
                nRet = FillBiblioSummaryColumn(
                    channel,
                    items,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.ReturnChannel(channel);
            }

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
            form.MdiParent = Program.MainForm;
            // form.MainForm = Program.MainForm;
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
            Program.MainForm.StatusBarMessage = "剪切操作 耗时 " + delta.TotalSeconds.ToString() + " 秒";
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


        // 从剪贴板粘贴条码号。每行一个条码号
        void menu_pasteBarcodeFromClipboard_Click(object sender, EventArgs e)
        {
            string strError = "";
            this.listView_records.SelectedIndexChanged -= new System.EventHandler(this.listView_records_SelectedIndexChanged);

            try
            {
                int nRet = PasteBarcodeLinesFromClipboard(
         true,
         out strError);
                if (nRet == -1)
                    goto ERROR1;
                return;
            }
            finally
            {
                this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
                listView_records_SelectedIndexChanged(null, null);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public int PasteBarcodeLinesFromClipboard(
            bool bInsertBefore,
            out string strError)
        {
            strError = "";

            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.UnicodeText) == false)
            {
                strError = "剪贴板中没有内容";
                return -1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在粘贴条码号 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

            try
            {

                string strWhole = (string)ido.GetData(DataFormats.UnicodeText);

                int index = -1;

                if (this.listView_records.SelectedIndices.Count > 0)
                    index = this.listView_records.SelectedIndices[0];

                List<ListViewItem> items = new List<ListViewItem>();

                this.listView_records.SelectedItems.Clear();

                //Cursor oldCursor = this.Cursor;
                //this.Cursor = Cursors.WaitCursor;

                // this.listView_records.BeginUpdate();
                try
                {
                    string[] lines = strWhole.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    stop.SetProgressRange(0, lines.Length);
                    int i = 0;
                    foreach (string line in lines)
                    {
                        string strLine = "";
                        if (string.IsNullOrEmpty(line))
                            goto CONTINUE;
                        strLine = line.Trim();
                        if (string.IsNullOrEmpty(strLine))
                            goto CONTINUE;

                        ListViewItem item = new ListViewItem();
                        item.Text = "";

                        this.listView_records.Items.Add(item);

                        FillLineByBarcode(channel,
                            strLine,
                            item);

                        items.Add(item);

                        item.Selected = true;

                    CONTINUE:
                        if (stop != null)
                        {
                            stop.SetMessage(strLine);
                            stop.SetProgressValue(i);
                        }
                        i++;
                    }

                }
                finally
                {
                    // this.listView_records.EndUpdate();

                    // this.Cursor = oldCursor;
                }

                // 刷新浏览行
                int nRet = RefreshListViewLines(
                    channel,
                    items,
                    "",
                    false,
                    true,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 刷新书目摘要
                nRet = FillBiblioSummaryColumn(
                    channel,
                    items,
                    false,
                    out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }
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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                return -1;
            }
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入条码号 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

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
                    if (FillLineByBarcode(channel, strBarcode, item) == true)
                    {
                        this.listView_records.Items.Add(item);
                        items.Add(item);
                    }
                    else
                        errors.Add(item.SubItems[2].Text);

                    i++;
                }

                // 刷新浏览行
                nRet = RefreshListViewLines(
                    channel,
                    items,
                    "",
                    false,
                    true,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 刷新书目摘要
                nRet = FillBiblioSummaryColumn(
                    channel,
                    items,
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
                this.ReturnChannel(channel);

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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入条码号 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

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

                    FillLineByBarcode(channel,
                        strBarcode,
                        item);

                    items.Add(item);
                }

                // 刷新浏览行
                int nRet = RefreshListViewLines(
                    channel,
                    items,
                    "",
                    false,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2014/1/15
                // 刷新书目摘要
                nRet = FillBiblioSummaryColumn(
                    channel,
                    items,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.ReturnChannel(channel);

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

        // TODO: 不具有 channel 参数的版本是否被脚本调用？可能需要改造脚本
        // 调用前，记录路径列已经有值
        /// <summary>
        /// 刷新一个浏览行的各列信息。
        /// 也就是从数据库中重新获取相关信息。
        /// 不刷新书目摘要列
        /// </summary>
        /// <param name="channel">channel</param>
        /// <param name="item">要刷新的 ListViewItem 对象</param>
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
                        ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
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

            Program.MainForm.StatusBarMessage = "册条码号 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportBarcodeFilename;
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
                return Program.MainForm.AppInfo.GetString(
                    "bibliosearchform",
                    "last_iso2709_filename",
                    "");
            }
            set
            {
                Program.MainForm.AppInfo.SetString(
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
                return Program.MainForm.AppInfo.GetBoolean(
                    "bibliosearchform",
                    "last_iso2709_crlf",
                    false);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
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
                return Program.MainForm.AppInfo.GetString(
                    "bibliosearchform",
                    "last_encoding_name",
                    "");
            }
            set
            {
                Program.MainForm.AppInfo.SetString(
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
                return Program.MainForm.AppInfo.GetString(
                    "bibliosearchform",
                    "last_cataloging_rule",
                    "<无限制>");
            }
            set
            {
                Program.MainForm.AppInfo.SetString(
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

        public static int GetCallNumberClassSource(
            string strClassType,
            string strMarcSyntax,
            out string strFieldName,
            out string strSubfieldName,
            out string strError)
        {
            strError = "";
            strFieldName = "";
            strSubfieldName = "";

            if (strMarcSyntax == "unimarc")
            {
                if (strClassType == "中图法")
                {
                    strFieldName = "690";
                    strSubfieldName = "a";
                }
                else if (strClassType == "科图法")
                {
                    strFieldName = "692";
                    strSubfieldName = "a";
                }
                else if (strClassType == "人大法")
                {
                    strFieldName = "694";
                    strSubfieldName = "a";
                }
                else if (strClassType == "其它" || strClassType == "红泥巴")
                {
                    strFieldName = "686";
                    strSubfieldName = "a";
                }
                else if (strClassType == "石头汤分类法"
                    || strClassType == "石头汤分类号"
                    || strClassType == "石头汤")
                {
                    strFieldName = "687";
                    strSubfieldName = "a";
                }
                else
                {
                    strError = "UNIMARC下未知的分类法 '" + strClassType + "'";
                    return -1;
                }
            }
            else if (strMarcSyntax == "usmarc")
            {
                if (strClassType == "杜威十进分类号"
                    || strClassType == "杜威十进分类法"
                    || strClassType == "DDC")
                {
                    strFieldName = "082";
                    strSubfieldName = "a";
                }
                else if (strClassType == "国际十进分类号"
                    || strClassType == "国际十进分类法"
                    || strClassType == "UDC")
                {
                    strFieldName = "080";
                    strSubfieldName = "a";
                }
                else if (strClassType == "国会图书馆分类法"
                    || strClassType == "美国国会图书馆分类法"
                    || strClassType == "LCC")
                {
                    strFieldName = "050";
                    strSubfieldName = "a";
                }
                else if (strClassType == "中图法")
                {
                    strFieldName = "093";
                    strSubfieldName = "a";
                }
                else if (strClassType == "科图法")
                {
                    strFieldName = "094";
                    strSubfieldName = "a";
                }
                else if (strClassType == "人大法")
                {
                    strFieldName = "095";
                    strSubfieldName = "a";
                }
                else if (strClassType == "其它" || strClassType == "红泥巴")
                {
                    strFieldName = "084";
                    strSubfieldName = "a";
                }
                else if (strClassType == "石头汤分类法"
                    || strClassType == "石头汤分类号"
                    || strClassType == "石头汤")
                {
                    strFieldName = "087";
                    strSubfieldName = "a";
                }
                else
                {
                    strError = "USMARC下未知的分类法 '" + strClassType + "'";
                    return -1;
                }
            }
            else
            {
                strError = "未知的MARC格式 '" + strMarcSyntax + "'";
                return -1;
            }

            return 1;
        }

        // 一个分类法针对特定 MARC 格式的具体信息
        class ClassTypeInfo
        {
            public string ClassType { get; set; }
            public string MarcSyntax { get; set; }
            public string FieldName { get; set; }
            public string SubfieldName { get; set; }

            public ClassTypeInfo(string strClassType,
                string strMarcSyntax)
            {
                this.MarcSyntax = strMarcSyntax;
                this.ClassType = strClassType;

                string strFieldName = "";
                string strSubfieldName = "";

                string strError = "";
                int nRet = GetCallNumberClassSource(
                    this.ClassType,
                    this.MarcSyntax,
        out strFieldName,
        out strSubfieldName,
        out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                this.FieldName = strFieldName;
                this.SubfieldName = strSubfieldName;
            }
        }

        // 分类统计
        void menu_classStatis_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.108") < 0)
            {
                strError = "本功能只能和 dp2library 2.08 或以上版本配套使用";
                goto ERROR1;
            }

            List<string> biblioRecPathList = new List<string>();   // 按照出现先后的顺序存储书目记录路径

            Hashtable groupTable = new Hashtable();   // 书目记录路径 --> List<string> (册记录路径列表)


            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
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
                return;
#endif

            // 询问统计参数。按照什么分类法统计？统计表中的分类号截断为多少字符？是否有预置的分类表?

            ItemClassStatisDialog dlg = new ItemClassStatisDialog();
            MainForm.SetControlFont(dlg, Program.MainForm.Font, false);
            dlg.ClassListFileName = Path.Combine(Program.MainForm.UserDir, "class_list.xml");
            dlg.UiState = Program.MainForm.AppInfo.GetString(
        "ItemSearchForm_" + this.DbType,
        "ItemClassStatisDialog_uiState",
        "");

            Program.MainForm.AppInfo.LinkFormState(dlg, "ItemSearchForm_ItemClassStatisDialog_uiState_state");
            dlg.ShowDialog(Program.MainForm);

            Program.MainForm.AppInfo.SetString(
        "ItemSearchForm_" + this.DbType,
"ItemClassStatisDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            string strFileName = dlg.FileName;
            List<string> class_list = dlg.ClassList;

            ClassTypeInfo unimarc = new ClassTypeInfo(dlg.ClassType, "unimarc");
            ClassTypeInfo marc21 = new ClassTypeInfo(dlg.ClassType, "usmarc");

            // 切换到“操作历史”属性页
            Program.MainForm.ActivateFixPage("history");

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
    + " 开始进行" + this.DbTypeCaption + "记录分类统计</div>");

            Table table = new Table(3);	// 类号 种数 册数 价格

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行分类统计 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();
            try
            {
                stop.SetMessage("正在汇总书目和册记录路径 ...");

                nRet = GetSelectedBiblioRecPath(
                    channel,
                    ref biblioRecPathList,// 按照出现先后的顺序存储书目记录路径
                    ref groupTable, // 书目记录路径 --> List<string> (册记录路径列表)
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 先行获得所有册记录信息
                // 先建立一个没有重复的册记录路径列表
                Hashtable item_recpath_table = new Hashtable();
                List<string> all_item_recpaths = new List<string>();
                foreach (string key in groupTable.Keys)
                {
                    List<string> item_recpaths = (List<string>)groupTable[key];
                    foreach (string recpath in item_recpaths)
                    {
                        if (item_recpath_table.ContainsKey(recpath) == false)
                        {
                            item_recpath_table.Add(recpath, null);
                            all_item_recpaths.Add(recpath);
                        }
                    }
                }

                item_recpath_table.Clear();

                if (dlg.OutputPrice)
                {
                    stop.SetProgressRange(0, all_item_recpaths.Count);
                    stop.SetMessage("正在获取册记录 ...");

                    BrowseLoader item_loader = new BrowseLoader();
                    item_loader.Channel = channel;
                    item_loader.Stop = this.Progress;
                    item_loader.Format = "id,cols,format:@coldef:*/price";

                    item_loader.RecPaths = all_item_recpaths;

                    item_loader.Prompt += new MessagePromptEventHandler(loader_Prompt);
                    try
                    {
                        int i = 0;
                        foreach (DigitalPlatform.LibraryClient.localhost.Record entity in item_loader)
                        {
                            string strPrice = "";
                            if (entity.Cols != null && entity.Cols.Length > 0)
                                strPrice = entity.Cols[0];
                            if (string.IsNullOrEmpty(entity.Path))
                                goto CONTINUE;
                            item_recpath_table[entity.Path] = strPrice;
                        CONTINUE:
                            i++;
                            stop.SetProgressValue(i);
                        }

                    }
                    finally
                    {
                        item_loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                    }
                }


                stop.SetProgressRange(0, biblioRecPathList.Count);
                stop.SetMessage("正在获取书目记录 ...");

                // 获得书目记录
#if NO
                BiblioLoader loader = new BiblioLoader();
                loader.Channel = channel;
                loader.Stop = this.Progress;
                loader.Format = "xml";
                loader.GetBiblioInfoStyle = GetBiblioInfoStyle.Timestamp;
                loader.RecPaths = biblioRecPathList;
#endif
                BrowseLoader loader = new BrowseLoader();
                loader.Channel = channel;
                loader.Stop = this.Progress;
                // loader.Format = "id,cols,format:@coldef://marc:record/marc:datafield[@tag='690']/marc:subfield[@code='a']->nl:marc=http://dp2003.com/UNIMARC->dm:\t|//marc:record/marc:datafield[@tag='093']/marc:subfield[@code='a']->nl:marc=http://www.loc.gov/MARC21/slim->dm:\t";
                loader.Format = "id,cols,format:@coldef://marc:record/marc:datafield[@tag='" + unimarc.FieldName + "']/marc:subfield[@code='" + unimarc.SubfieldName + "']->nl:marc=http://dp2003.com/UNIMARC->dm:\t|//marc:record/marc:datafield[@tag='" + marc21.FieldName + "']/marc:subfield[@code='" + marc21.SubfieldName + "']->nl:marc=http://www.loc.gov/MARC21/slim->dm:\t";
                loader.RecPaths = biblioRecPathList;

                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                {
                    int i = 0;
                    foreach (DigitalPlatform.LibraryClient.localhost.Record item in loader)
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        // stop.SetMessage("正在获取书目记录 " + item.Path);
                        // Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(item.Path) + "</div>");

#if NO
                    bool bNullBiblio = false;   // 书目记录是否为空
                    string strXml = item.Content;
                    if (string.IsNullOrEmpty(strXml))
                    {
                        strXml = "<root />";
                        bNullBiblio = true;
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode("书目记录 " + item.RecPath + " 不存在。被当作空记录继续处理了") + "</div>");
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
                        strError = "XML 转换到 MARC 记录时出错: " + strError;
                        goto ERROR1;
                    }

                    if (bNullBiblio == true && string.IsNullOrEmpty(strMarcSyntax))
                        strMarcSyntax = "unimarc";

                    // 获得分类号
                    string strFieldName = "";
                    string strSubfieldName = "";

                    nRet = GetCallNumberClassSource(
                        strClassType,
                        strMarcSyntax,
            out strFieldName,
            out strSubfieldName,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    MarcRecord record = new MarcRecord(strMARC);
                    string strClass = record.select("field[@name='" + strFieldName + "']/subfield[@name='" + strSubfieldName + "']").FirstContent;
#endif
                        string strClass = "";

                        bool bNullBiblio = false;   // 书目记录是否为空
                        if (item.RecordBody != null && item.RecordBody.Result != null
                            && item.RecordBody.Result.ErrorCode == ErrorCodeValue.NotFound)
                        {
                            bNullBiblio = true;
                            Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode("书目记录 '" + item.Path + "' 不存在。被当作空记录继续处理了") + "</div>");
                        }

                        if (bNullBiblio == false && item.Cols != null)
                        {
                            if (item.Cols.Length > 0)
                                strClass = item.Cols[0];
                            if (string.IsNullOrEmpty(strClass) && item.Cols.Length > 1)
                                strClass = item.Cols[1];
                            // 多个分类号只取出第一个
                            if (string.IsNullOrEmpty(strClass) == false)
                            {
                                nRet = strClass.IndexOf('\t');
                                if (nRet != -1)
                                    strClass = strClass.Substring(0, nRet);
                            }
                        }

                        List<string> heads = new List<string>();
                        if (string.IsNullOrEmpty(strClass) == false)
                        {
                            if (class_list.Count == 0)
                                heads.Add(strClass.Substring(0, 1));
                            else
                            {
                                // 从 class_list 中找到预定义的前缀字符串。可能会找到多个
                                heads = ItemClassStatisDialog.GetClassHead(class_list,
                                    strClass);
                            }
                        }

                        if (heads.Count == 0)
                        {
                            heads.Add("(空)");
                            if (bNullBiblio == false)
                                Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode("书目记录 " + item.Path + " 中缺乏分类号字段") + "</div>");
                        }
                        Debug.Assert(heads.Count > 0, "");
#if NO
                        if (string.IsNullOrEmpty(strClass) == true)
                        {
                            strClass = "(空)";
                            // if (bNullBiblio == false)
                            //    Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode("书目记录 " + item.Path + " 中缺乏子字段 '" + strFieldName + "$" + strSubfieldName + "' ") + "</div>");
                            if (bNullBiblio == false)
                                Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode("书目记录 " + item.Path + " 中缺乏分类号字段") + "</div>");
                        }
#endif

                        foreach (string head in heads)
                        {
                            List<string> item_recpaths = (List<string>)groupTable[item.Path];

                            // 种数 列号0
                            table.IncValue(head, 0, 1, 1);

                            // 册数 列号1
                            table.IncValue(head, 1, item_recpaths.Count, item_recpaths.Count);

                            // 遍历下属的册记录
                            if (dlg.OutputPrice)
                            {
                                foreach (string recpath in item_recpaths)
                                {
                                    string strPrice = (string)item_recpath_table[recpath];

                                    if (strPrice == null)
                                    {
                                        strError = "路径为 '" + recpath + "' 的册信息在 hashtable 中没有找到";
                                        goto ERROR1;
                                    }

                                    string strPricePure = strPrice;
                                    if (ItemClassStatisDialog.CorrectPrice(ref strPricePure) == true)
                                    {
                                        Program.MainForm.OperHistory.AppendHtml("<div class='debug warning'>" + HttpUtility.HtmlEncode("册记录 " + recpath + " 中的价格字符串 '" + strPrice + "' 被自动变换为 '" + strPricePure + "'") + "</div>");
                                    }

                                    // 价格 列号2
                                    try
                                    {
                                        table.IncCurrency(head, 2, strPricePure, strPricePure);
                                    }
                                    catch (Exception ex)
                                    {
                                        //this.m_lErrorCount++;
                                        //sw_error.Write("册记录 " + this.CurrentRecPath + " 中的价格字符串 '" + strPrice + "' 在汇总时出错：" + ex.Message + "\r\n");
                                        Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode("册记录 " + recpath + " 中的价格字符串 '" + strPrice + "' 在汇总时出错：" + ex.Message) + "</div>");
                                    }
                                }
                            }
                        }

                        stop.SetProgressValue(++i);
                    }
                }

            }
            catch (ChannelException ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
    + " 结束执行" + this.DbTypeCaption + "记录分类统计</div>");
            }

            table.Sort();

            Report report = Report.BuildReport(table,
    "类目||class,种||title,册||item,价格||price",
    "",
    true);

            if (report == null)	// 空表格
                return;

            report[3].DataType = DataType.Currency;

            report.SumCell -= new SumCellEventHandler(SumCell);
            report.SumCell += new SumCellEventHandler(SumCell);

            DigitalPlatform.dp2.Statis.Report.ExcelTableConfig config = new DigitalPlatform.dp2.Statis.Report.ExcelTableConfig();
            config.StartCol = 1;
            config.StartRow = 1;


            XLWorkbook doc = null;

            try
            {
                doc = new XLWorkbook(XLEventTracking.Disabled);
                File.Delete(dlg.FileName);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            try
            {
                IXLWorksheet sheet = null;
                sheet = doc.Worksheets.Add("表格");

                nRet = report.ExportToExcel(
                table,
                config,
                sheet,
                out strError);
                if (nRet == -1)
                    goto ERROR1;


                doc.SaveAs(dlg.FileName);
            }
            finally
            {
                doc.Dispose();
            }

            // 启动 Excel
            if (string.IsNullOrEmpty(strFileName) == false)
            {
                try
                {
                    System.Diagnostics.Process.Start(strFileName);
                }
                catch
                {

                }
            }

            Program.MainForm.StatusBarMessage = "书目记录 " + groupTable.Count.ToString() + "个 已成功导出到文件 " + this.ExportBiblioDumpFilename;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        void SumCell(object sender, SumCellEventArgs e)
        {
            if (String.IsNullOrEmpty(e.Line.Entry) == true)
                return;

            // 忽略多于1字符的类目名
            if (e.Line.Entry.Length > 1 && e.Line.Entry != "(空)")
                e.Value = null;
        }

        // 注: 本函数中要修改 stop 的 ProgressRange
        int GetSelectedBiblioRecPath(
            LibraryChannel channel,
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
                nRet = GetBiblioRecPath(
                    channel,
                    item,
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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导出书目转储记录 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();
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
                        channel,
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

                        long lRet = channel.GetBiblioInfos(
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
                        DomUtil.SetAttr(domBiblio.DocumentElement, "path", DpNs.dprms, Program.MainForm.LibraryServerUrl + "?" + item.BiblioInfo.RecPath);  // strRecPath
                        DomUtil.SetAttr(domBiblio.DocumentElement, "timestamp", DpNs.dprms, ByteArray.GetHexTimeStampString(item.BiblioInfo.Timestamp));   // baTimestamp
#endif

                        w.WriteAttributeString("path", Program.MainForm.LibraryServerUrl + "?" + strBiblioRecPath);
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
                                channel,
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
                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }

            Program.MainForm.StatusBarMessage = "书目记录 " + groupTable.Count.ToString() + "个 已成功导出到文件 " + this.ExportBiblioDumpFilename;
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
            nRet = LoadRuleNameTable(PathUtil.MergePath(Program.MainForm.DataDir, "cataloging_rules.xml"),
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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }


            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存到 MARC 文件 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

            this.EnableControls(false);
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
                    channel,
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

                nRet = DumpBiblioAndPartsSubItems.Dump(channel,
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
                                        Program.MainForm.DefaultFont);
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
                this.EnableControls(true);
                s.Close();

                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
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
            nRet = LoadRuleNameTable(PathUtil.MergePath(Program.MainForm.DataDir, "cataloging_rules.xml"),
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
                                    Program.MainForm.DefaultFont);
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
                        Program.MainForm,
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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导出书目记录路径 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportBiblioRecPathFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
            try
            {

                nRet = GetSelectedBiblioRecPath(
                    channel,
                    out biblio_recpaths,
            ref nWarningLineCount,
            ref nDupCount,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                foreach (string path in biblio_recpaths)
                {
                    Application.DoEvents();

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    sw.WriteLine(path);
                }
            }
            finally
            {
                if (sw != null)
                    sw.Close();

                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            Program.MainForm.StatusBarMessage = "书目记录路径 " + biblio_recpaths.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportBiblioRecPathFilename;
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
                if (stop.IsInLoop == true)
                {
                    strError = "无法重复进入循环";
                    return -1;
                }
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
                    this.EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                    stop.HideProgress();
                    stop.Style = StopStyle.None;
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

            Program.MainForm.StatusBarMessage = "册记录路径 " + nRet.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportRecPathFilename;
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

            Program.MainForm.StatusBarMessage = "行内容 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文本文件 " + this.ExportTextFilename;
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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
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
                this.EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;
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
                && string.IsNullOrEmpty(Program.MainForm.ArrivedDbName) == false)
            {
                this.comboBox_entityDbName.Items.Add(Program.MainForm.ArrivedDbName);
                return;
            }

            if (this.DbType != "issue")
                this.comboBox_entityDbName.Items.Add("<全部图书>");

            this.comboBox_entityDbName.Items.Add("<全部期刊>");

            if (Program.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < Program.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty property = Program.MainForm.BiblioDbProperties[i];

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
            if (Program.MainForm.NormalDbProperties == null)
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
                this.tabComboBox_queryWord.Text = query.QueryWord;
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
                this.Text = "实体查询 " + this.tabComboBox_queryWord.Text;
            else
                this.Text = this.DbTypeCaption + "查询 " + this.tabComboBox_queryWord.Text;
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
                this.tabComboBox_queryWord.Text = "";
                this.tabComboBox_queryWord.Enabled = false;
            }
            else
            {
                this.tabComboBox_queryWord.Enabled = true;
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
                dlg.Rfc1123String = this.tabComboBox_queryWord.Text;
            }
            catch
            {
                this.tabComboBox_queryWord.Text = "";
            }
            Program.MainForm.AppInfo.LinkFormState(dlg, "searchitemform_gettimedialog_single");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.tabComboBox_queryWord.Text = dlg.Rfc1123String;

        }

        private void ToolStripMenuItem_uSingle_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            dlg.RangeMode = false;
            try
            {
                dlg.uString = this.tabComboBox_queryWord.Text;
            }
            catch
            {
                this.tabComboBox_queryWord.Text = "";
            }

            Program.MainForm.AppInfo.LinkFormState(dlg, "searchitemform_gettimedialog_single");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.tabComboBox_queryWord.Text = dlg.uString;

        }

        private void ToolStripMenuItem_rfc1123Range_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            dlg.RangeMode = true;
            // 分割为两个字符串
            try
            {
                dlg.Rfc1123String = this.tabComboBox_queryWord.Text;
            }
            catch
            {
                this.tabComboBox_queryWord.Text = "";
            }
            Program.MainForm.AppInfo.LinkFormState(dlg, "searchitemform_gettimedialog_range");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.tabComboBox_queryWord.Text = dlg.Rfc1123String;
        }

        private void ToolStripMenuItem_uRange_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            dlg.RangeMode = true;
            try
            {
                dlg.uString = this.tabComboBox_queryWord.Text;
            }
            catch
            {
                this.tabComboBox_queryWord.Text = "";
            }

            Program.MainForm.AppInfo.LinkFormState(dlg, "searchitemform_gettimedialog_range");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.tabComboBox_queryWord.Text = dlg.uString;
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

            baTimestamp = null;
            string strOutputRecPath = "";
            string strBiblio = "";
            string strBiblioRecPath = "";
            // 获得册记录
            long lRet = 0;

            if (this.DbType == "item")
            {
                lRet = channel.GetItemInfo(
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
                lRet = channel.GetOrderInfo(
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
                lRet = channel.GetIssueInfo(
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
                lRet = channel.GetCommentInfo(
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
        internal override int SaveRecord(
            LibraryChannel channel,
            string strRecPath,
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
                lRet = channel.SetEntities(
                     null,   // this.BiblioStatisForm.stop,
                     "",
                     entities,
                     out errorinfos,
                     out strError);
            else if (this.DbType == "order")
                lRet = channel.SetOrders(
                     null,   // this.BiblioStatisForm.stop,
                     "",
                     entities,
                     out errorinfos,
                     out strError);
            else if (this.DbType == "issue")
                lRet = channel.SetIssues(
                     null,   // this.BiblioStatisForm.stop,
                     "",
                     entities,
                     out errorinfos,
                     out strError);
            else if (this.DbType == "comment")
                lRet = channel.SetComments(
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
                // host.MainForm = Program.MainForm;
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

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行脚本 " + dlg.FileName + "</div>");

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在针对" + this.DbTypeCaption + "记录执行 C# 脚本 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

            this.EnableControls(false);

            this.listView_records.Enabled = false;
            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                {
                    // host.MainForm = Program.MainForm;
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

                ListViewPatronLoader loader = new ListViewPatronLoader(channel,
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
                        goto ERROR1;
                    }

                    stop.SetProgressValue(i);

                    BiblioInfo info = item.BiblioInfo;

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    // host.MainForm = Program.MainForm;
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
                    host.MainForm = Program.MainForm;
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
                this.EnableControls(true);

                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行脚本 " + dlg.FileName + "</div>");
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

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.Style = StopStyle.None;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(20);

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

                long lRet = channel.Search(stop,
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
                    channel,
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
                this.EnableControls(true);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;
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
                if (Program.MainForm.BiblioDbProperties != null)
                {
                    for (int i = 0; i < Program.MainForm.BiblioDbProperties.Count; i++)
                    {
                        BiblioDbProperty property = Program.MainForm.BiblioDbProperties[i];

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
            // dlg.MainForm = Program.MainForm;
            dlg.XmlString = strQueryXml;
            // dlg.StartPosition = FormStartPosition.CenterScreen;

            Program.MainForm.AppInfo.LinkFormState(dlg, "bibliosearchform_viewqueryxml");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

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

        private void tabComboBox_queryWord_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_search;
        }

        // 列出当前全部检索点
        public int ListKeys()
        {
            string strError = "";
            int nRet = 0;

            string strQueryWord = "";
            int nMaxCount = 500;

            string strQuery = this.comboBox_entityDbName.Text + "|" + strQueryWord + "|" + this.comboBox_from.Text + "|" + nMaxCount;
            if (strQuery == _keysListQuery)
                return 0;   // 不需要检索填充，当前列表内容已经有了

            this.tabComboBox_queryWord.Items.Clear();

            this.label_message.Text = "";

            if (stop.IsInLoop == true)
            {
                strError = "无法重复进入循环";
                goto ERROR1;
            }
            stop.Style = StopStyle.None;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在列出检索点 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            EnableControls(false);
            try
            {
                string strMatchStyle = "";

                // 为了在检索词为空的时候，检索出全部的记录
                strMatchStyle = "left";

                string strOutputStyle = "";

                strOutputStyle = "keycount";

                long lRet = 0;

                if (this.DbType == "item")
                {
                    lRet = channel.SearchItem(stop,
                        this.comboBox_entityDbName.Text, // "<all>",
                        strQueryWord,
                        nMaxCount,
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
                    lRet = channel.SearchComment(stop,
                        this.comboBox_entityDbName.Text,
                        strQueryWord,
                        nMaxCount,
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
                    lRet = channel.SearchOrder(stop,
                        this.comboBox_entityDbName.Text,
                        strQueryWord,
                        nMaxCount,
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
                    lRet = channel.SearchIssue(stop,
                        this.comboBox_entityDbName.Text,
                        strQueryWord,
                        nMaxCount,
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
                    if (string.IsNullOrEmpty(Program.MainForm.ArrivedDbName) == true)
                    {
                        strError = "当前服务器尚未配置预约到书库名";
                        goto ERROR1;
                    }

                    string strQueryXml = "<target list='" + Program.MainForm.ArrivedDbName + ":" + this.comboBox_from.Text + "'><item><word>"
        + StringUtil.GetXmlStringSimple(strQueryWord)
        + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>"
                    + this.MaxSearchResultCount + "</maxCount></item><lang>" + this.Lang + "</lang></target>";
                    // strOutputStyle ?
                    lRet = channel.Search(stop,
                        strQueryXml,
                        "",
                        strOutputStyle,
                        out strError);
                }
                else
                    throw new Exception("未知的 DbType '" + this.DbType + "'");

                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    _keysListQuery = strQuery;
                    return 1;
                }

                long lHitCount = lRet;

                // return:
                //      -1  出错
                //      0   用户中断
                //      1   正常完成
                nRet = FillKeysList(
                    channel,
                    lHitCount,
                    out strError);
                if (nRet == 0)
                    return 0;
                if (nRet == -1)
                    goto ERROR1;
                else
                    _keysListQuery = strQuery;
            }
            finally
            {
                EnableControls(true);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                // stop.HideProgress();
                stop.Style = StopStyle.None;
            }

            return 1;
        ERROR1:
            this.ShowMessage(strError, "red", true);
            return -1;
        }

        // return:
        //      -1  出错
        //      0   用户中断或者未命中
        //      1   正常完成
        int FillKeysList(
            LibraryChannel channel,
            long lHitCount,
            out string strError)
        {
            strError = "";

            string strBrowseStyle = "keycount";

            long lStart = 0;
            long lCount = lHitCount;

            long lMaxPerCount = 500;

            DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

            for (; ; )
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null && stop.State != 0)
                    return 0;

                long lRet = channel.GetSearchResult(
                    stop,
                    null,   // strResultSetName
                    lStart,
                    Math.Min(lMaxPerCount, lCount),
                    strBrowseStyle,
                    this.Lang,
                    out searchresults,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                    return 0;

                // 处理浏览结果
                try
                {
                    List<ListViewItem> items = new List<ListViewItem>();
                    foreach (DigitalPlatform.LibraryClient.localhost.Record searchresult in searchresults)
                    {

                        // 输出keys
                        if (searchresult.Cols == null)
                        {
                            strError = "要使用获取检索点功能，请将 dp2Library 应用服务器和 dp2Kernel 数据库内核升级到最新版本";
                            return -1;
                        }

                        KeyCount keycount = new KeyCount();
                        string strKey = searchresult.Path;
                        int count = Convert.ToInt32(searchresult.Cols[0]);

                        this.tabComboBox_queryWord.Items.Add(strKey + "\t" + count.ToString() + "笔");
                    }

                }
                finally
                {
                }

                lStart += searchresults.Length;
                lCount -= searchresults.Length;

                if (lStart >= lHitCount || lCount <= 0)
                    break;
            }

            return 1;
        }

        // 当前检索点列表对应的检索式
        string _keysListQuery = null;

        private void tabComboBox_queryWord_DropDown(object sender, EventArgs e)
        {
            ListKeys();
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