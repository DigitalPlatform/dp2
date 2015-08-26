using ClosedXML.Excel;
using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.CommonControl;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    public partial class InventoryForm : ItemSearchFormBase
    {
        QuickChargingForm _chargingForm = null;

        public InventoryForm()
        {
            InitializeComponent();

            this.DbType = "inventory";

            _chargingForm = new QuickChargingForm();
            this.tabPage_scan.Padding = new Padding(4, 4, 4, 4);
            this.tabPage_scan.Controls.Add(_chargingForm.MainPanel);
            _chargingForm.MainPanel.Dock = DockStyle.Fill;

            {
                ListViewProperty prop = new ListViewProperty();
                this.listView_inventoryList_records.Tag = prop;
                // 第一列特殊，记录路径
                prop.SetSortStyle(0, ColumnSortStyle.RecPath);
                prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
                prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);

                prop.CompareColumn -= new CompareEventHandler(prop_CompareColumn);
                prop.CompareColumn += new CompareEventHandler(prop_CompareColumn);
            }

            {
                ListViewProperty prop = new ListViewProperty();
                this.listView_baseList_records.Tag = prop;
                // 第一列特殊，记录路径
                prop.SetSortStyle(0, ColumnSortStyle.RecPath);
                prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles2);
                prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles2);

                prop.CompareColumn -= new CompareEventHandler(prop_CompareColumn2);
                prop.CompareColumn += new CompareEventHandler(prop_CompareColumn2);
            }

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

#if NO
        void ClearListViewPropertyCache()
        {
            ListViewProperty prop = (ListViewProperty)this.listView_inventoryList_records.Tag;
            prop.ClearCache();
        }
#endif

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
#if NO
            if (e.DbName == "<blank>")
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                e.ColumnTitles.Add("检索点");
                e.ColumnTitles.Add("数量");
                // 数量列的排序
                e.ListViewProperty.SetSortStyle(2, ColumnSortStyle.RightAlign);
                return;
            }
#endif

            e.ColumnTitles = new ColumnPropertyCollection();
            ColumnPropertyCollection temp = this.MainForm.GetBrowseColumnProperties(e.DbName);
            if (temp != null)
            {
                e.ColumnTitles.Insert(0, "书目摘要");
                e.ColumnTitles.AddRange(temp);  // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件
            }
        }

        void prop_CompareColumn2(object sender, CompareEventArgs e)
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

        void prop_GetColumnTitles2(object sender, GetColumnTitlesEventArgs e)
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
        }


        private void InventoryForm_Load(object sender, EventArgs e)
        {
            this._chargingForm.SupressSizeSetting = true; // 避免保存窗口尺寸
            this._chargingForm.MainForm = this.MainForm;
            this._chargingForm.ShowInTaskbar = false;
            this._chargingForm.Show();  // 有了此句对话框的 xxx_load 才能被执行
            this._chargingForm.Hide();  // 有了此句可避免主窗口背后显示一个空对话框窗口
            // this._chargingForm.WindowState = FormWindowState.Minimized;
            this._chargingForm.SmartFuncState = FuncState.InventoryBook;
#if NO
                        // 输入的ISO2709文件名
            this._openMarcFileDialog.FileName = this.MainForm.AppInfo.GetString(
                "iso2709statisform",
                "input_iso2709_filename",
                "");
#endif
            this.UiState = this.MainForm.AppInfo.GetString(
    "inventory_form",
    "ui_state",
    "");
        }

        private void InventoryForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void InventoryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._chargingForm.Close();

            this.MainForm.AppInfo.SetString(
"inventory_form",
"ui_state",
this.UiState);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this._chargingForm.MainPanel.Enabled = bEnable;

            this.tabComboBox_inputBatchNo.Enabled = bEnable;

            this.textBox_inventoryList_batchNo.Enabled = bEnable;
            this.button_inventoryList_getBatchNos.Enabled = bEnable;
            this.button_inventoryList_search.Enabled = bEnable;

            this.textBox_baseList_locations.Enabled = bEnable;
            this.button_baseList_getLocations.Enabled = bEnable;
            this.button_baseList_search.Enabled = bEnable;

            this.button_statis_crossCompute.Enabled = bEnable;
#if NO
            this.button_getProjectName.Enabled = bEnable;

            this.button_next.Enabled = bEnable;

            this.button_projectManage.Enabled = bEnable;
#endif
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
    Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                // MessageBox.Show(this, "test");
                if (this.tabControl_main.SelectedTab == this.tabPage_scan)
                    this._chargingForm.DoEnter();
                return true;
            }

#if NO
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }
#endif

            // return false;
            return base.ProcessDialogKey(keyData);
        }

        private void tabComboBox_inputBatchNo_TextChanged(object sender, EventArgs e)
        {
            if (this._chargingForm != null)
                this._chargingForm.BatchNo = this.tabComboBox_inputBatchNo.Text;
        }

        private void button_list_getBatchNos_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strInventoryDbName = this.MainForm.GetUtilDbName("inventory");
            if (string.IsNullOrEmpty(strInventoryDbName) == true)
            {
                strError = "尚未定义盘点库";
                goto ERROR1;
            }

            SelectBatchNoDialog dlg = new SelectBatchNoDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Channel = this.Channel;
            dlg.Stop = this.stop;
            dlg.InventoryDbName = strInventoryDbName;
            this.MainForm.AppInfo.LinkFormState(dlg, "SelectBatchNoDialog_state");
            dlg.ShowDialog(this);

            this.textBox_inventoryList_batchNo.Text = StringUtil.MakePathList(dlg.SelectedBatchNo, "\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_list_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<string> batchNo_list = StringUtil.SplitList(this.textBox_inventoryList_batchNo.Text.Replace("\r\n", "\n"), '\n');
            StringUtil.RemoveBlank(ref batchNo_list);
            if (batchNo_list.Count == 0)
            {
                // TODO: 没有指定批次号则检索出全部?
                strError = "请指定至少一个批次号";
                goto ERROR1;
            }

            for (int i = 0; i < batchNo_list.Count; i++)
            {
                string batchNo = batchNo_list[i];
                if (batchNo == "[空]" || batchNo == "[blank]")
                    batchNo_list[i] = "";
            }

            int nRet = DoSearchInventory(batchNo_list,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        internal void ClearInventoryListViewItems()
        {
            this.listView_inventoryList_records.Items.Clear();

            ListViewUtil.ClearSortColumns(this.listView_inventoryList_records);

            // 清除所有需要确定的栏标题
            for (int i = 1; i < this.listView_inventoryList_records.Columns.Count; i++)
            {
                this.listView_inventoryList_records.Columns[i].Text = i.ToString();
            }

            //ClearBiblioTable();
            //ClearCommentViewer();
        }

        internal void ClearItemListViewItems()
        {
            this.listView_baseList_records.Items.Clear();

            ListViewUtil.ClearSortColumns(this.listView_baseList_records);

            // 清除所有需要确定的栏标题
            for (int i = 1; i < this.listView_baseList_records.Columns.Count; i++)
            {
                this.listView_baseList_records.Columns[i].Text = i.ToString();
            }

            //ClearBiblioTable();
            //ClearCommentViewer();
        }

        int DoSearchInventory(List<string> batchNo_list,
            out string strError)
        {
            strError = "";

            bool bAccessBiblioSummaryDenied = false;

            ClearInventoryListViewItems();

            string strDbName = this.MainForm.GetUtilDbName("inventory");
            if (string.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未配置盘点库";
                return -1;
            }
            string strFrom = "批次号";
            string strMatchStyle = "exact";
            string strLang = "zh";

            string strResultSetName = "inventory";

            string strQueryXml = "<target list='" + strDbName + ":" + strFrom + "'>";
            {
                int i = 0;
                foreach (string batchNo in batchNo_list)
                {
                    if (i > 0)
                        strQueryXml += "<operator value='OR' />";

                    strQueryXml += "<item><word>"
        + StringUtil.GetXmlStringSimple(batchNo)
        + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + strLang + "</lang>";

                    i++;
                }
            }
            strQueryXml += "</target>";

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.Channel.DoStop);
            stop.Initial("正在检索盘点记录 ...");
            stop.BeginLoop();

            this.listView_inventoryList_records.BeginUpdate();
            this._listview = this.listView_inventoryList_records;
            this.timer_qu.Start();
            try
            {
                // 开始检索
                long lRet = this.Channel.Search(
    stop,
    strQueryXml,
    strResultSetName,
    "", // strOutputStyle
    out strError);
                if (lRet == 0)
                {
                    strError = "not found";
                    return 0;   // not found
                }
                if (lRet == -1)
                    return -1;

                long lHitCount = lRet;

                stop.SetProgressRange(0, lHitCount);
                stop.Style = StopStyle.EnableHalfStop;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                Record[] searchresults = null;


                // 获得结果集，装入listview
                for (; ; )
                {
                    stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    lRet = this.Channel.GetSearchResult(
                        stop,
                        strResultSetName,   // strResultSetName
                        lStart,
                        lPerCount,
                        "id,cols",   // "id,cols"
                        strLang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        return 0;
                    }

                    List<ListViewItem> items = new List<ListViewItem>();

                    // 处理浏览结果
                    int i = 0;
                    foreach (Record record in searchresults)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        ListViewItem item = Global.AppendNewLine(
    this.listView_inventoryList_records,
    record.Path,
    Global.InsertBlankColumn(record.Cols));

                        items.Add(item);
                        stop.SetProgressValue(lStart + i);
                        i++;
                    }

                    if (bAccessBiblioSummaryDenied == false)
                    {
                        // return:
                        //      -2  获得书目摘要的权限不够
                        //      -1  出错
                        //      0   用户中断
                        //      1   完成
                        int nRet = _fillBiblioSummaryColumn0(items,
                            0,
                            false,
                            true,   // false,  // bAutoSearch
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == -2)
                            bAccessBiblioSummaryDenied = true;

                        if (nRet == 0)
                        {
                            // this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...";
                            this.ShowMessage("检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...", "yellow", true);
                            return 0;
                        }
                    }


                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
                }

                return 0;
            }
            finally
            {
                this.timer_qu.Stop();
                this.listView_inventoryList_records.EndUpdate();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.Channel.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        }

        // parameters:
        //      lStartIndex 调用前已经做过的事项数。为了准确显示 Progress
        // return:
        //      -2  获得书目摘要的权限不够
        //      -1  出错
        //      0   用户中断
        //      1   完成
        internal int _fillBiblioSummaryColumn0(List<ListViewItem> items,
            long lStartIndex,
            bool bDisplayMessage,
            bool bAutoSearch,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (m_bBiblioSummaryColumn == false)
                return 0;

            Debug.Assert(this.DbType == "inventory",
                "");

            List<string> biblio_recpaths = new List<string>();  // 尺寸可能比 items 数组小，没有包含里面不具有 parent id 列的事项
            // List<int> colindex_list = new List<int>();  // 存储每个 item 对应的 parent id colindex。数组大小等于 items 数组大小
            foreach (ListViewItem item in items)
            {
#if NO
                string strRecPath = ListViewUtil.GetItemText(item, 0);
                // 根据记录路径获得数据库名
                string strItemDbName = Global.GetDbName(strRecPath);
                // 根据数据库名获得 parent id 列号

                int nCol = -1;
                object o = m_tableSummaryColIndex[strItemDbName];
                if (o == null)
                {
                    ColumnPropertyCollection temp = this.MainForm.GetBrowseColumnProperties(strItemDbName);
                    nCol = temp.FindColumnByType("parent_id");
                    if (nCol == -1)
                    {
                        colindex_list.Add(-1);
                        continue;   // 这个实体库没有 parent id 列
                    }
                    nCol += 2;
                    if (this.m_bFirstColumnIsKey == true)
                        nCol++; // 2013/11/12
                    m_tableSummaryColIndex[strItemDbName] = nCol;   // 储存起来
                }
                else
                    nCol = (int)o;

                Debug.Assert(nCol > 0, "");

                colindex_list.Add(nCol);

                // 获得 parent id
                string strText = ListViewUtil.GetItemText(item, nCol);

                string strBiblioRecPath = "";
                // 看看是否已经是路径 ?
                if (strText.IndexOf("/") == -1)
                {
                    // 获得对应的书目库名
                    strBiblioRecPath = this.MainForm.GetBiblioDbNameFromItemDbName(this.DbType, strItemDbName);
                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                    {
                        strError = "数据库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                        return -1;
                    }
                    strBiblioRecPath = strBiblioRecPath + "/" + strText;

                    ListViewUtil.ChangeItemText(item, nCol, strBiblioRecPath);
                }
                else
                    strBiblioRecPath = strText;
#endif
                int nCol = -1;
                string strBiblioRecPath = "";
                // 获得事项所从属的书目记录的路径
                // return:
                //      -1  出错
                //      0   相关数据库没有配置 parent id 浏览列
                //      1   找到
                nRet = GetBiblioRecPath(item,
                    // bAutoSearch,   // true 如果遇到没有 parent id 列的时候速度较慢
                    // out nCol,
                    out strBiblioRecPath,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    // colindex_list.Add(-1);
                    continue;
                }

#if NO
                if (string.IsNullOrEmpty(strBiblioRecPath) == false
                    && nCol == -1)
                    colindex_list.Add(0);
                else
                    colindex_list.Add(nCol);
#endif

                biblio_recpaths.Add(strBiblioRecPath);
            }

            CacheableBiblioLoader loader = new CacheableBiblioLoader();
            loader.Channel = this.Channel;
            loader.Stop = this.stop;
            loader.Format = "summary";
            loader.GetBiblioInfoStyle = GetBiblioInfoStyle.None;
            loader.RecPaths = biblio_recpaths;

            var enumerator = loader.GetEnumerator();

            int i = 0;
            foreach (ListViewItem item in items)
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null
                    && stop.State != 0)
                {
                    strError = "用户中断";
                    return 0;
                }

                string strRecPath = ListViewUtil.GetItemText(item, 0);
                if (stop != null && bDisplayMessage == true)
                {
                    stop.SetMessage("正在刷新浏览行 " + strRecPath + " 的书目摘要 ...");
                    stop.SetProgressValue(lStartIndex + i);
                }

#if NO
                // int nCol = colindex_list[i];
                int nCol = -1;
                if (nCol == -1)
                {
                    ListViewUtil.ChangeItemText(item,
                        this.m_bFirstColumnIsKey == false ? 1 : 2,
                        "");
                    ClearOneChange(item, true); // 清除内存中的修改
                    i++;
                    continue;
                }
#endif

                try
                {
                    bool bRet = enumerator.MoveNext();
                    if (bRet == false)
                    {
                        Debug.Assert(false, "还没有到结尾, MoveNext() 不应该返回 false");
                        // TODO: 这时候也可以采用返回一个带没有找到的错误码的元素
                        strError = "error 1";
                        return -1;
                    }
                }
                catch (ChannelException ex)
                {
                    strError = ex.Message;
                    if (ex.ErrorCode == ErrorCode.AccessDenied)
                        return -2;
                    return -1;
                }

                BiblioItem biblio = (BiblioItem)enumerator.Current;
                // Debug.Assert(biblio.RecPath == strRecPath, "m_loader 和 items 的元素之间 记录路径存在严格的锁定对应关系");

                ListViewUtil.ChangeItemText(item,
                    this.m_bFirstColumnIsKey == false ? 1 : 2,
                    biblio.Content);

                ClearOneChange(item, true); // 清除内存中的修改
                i++;
            }

            return 1;
        }

        // 获得事项所从属的书目记录的路径
        // parameters:
        //      bAutoSearch 当没有 parent id 列的时候，是否自动进行检索以便获得书目记录路径
        // return:
        //      -1  出错
        //      0   相关数据库没有配置 parent id 浏览列
        //      1   找到
        int GetBiblioRecPath(ListViewItem item,
            // bool bAutoSearch,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            // nCol = -1;
            strBiblioRecPath = "";
            int nRet = 0;

            // 这是事项记录路径
            string strRecPath = ListViewUtil.GetItemText(item, 0);

            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                strError = "浏览行(盘点库)记录路径列没有内容，无法获得书目记录路径";
                return -1;
            }

            int nItemRecPathCol = -1;

            // 根据记录路径获得(盘点库)数据库名
            string strItemDbName = Global.GetDbName(strRecPath);

            object o = m_tableSummaryColIndex[strItemDbName];
            if (o == null)
            {
                if (this.MainForm.NormalDbProperties == null)
                {
                    strError = "普通数据库属性尚未初始化";
                    return -1;
                }
                ColumnPropertyCollection temp = this.MainForm.GetBrowseColumnProperties(strItemDbName);
                if (temp == null)
                {
                    strError = "盘点库 '" + strItemDbName + "' 没有找到列定义";
                    return -1;
                }

                nItemRecPathCol = temp.FindColumnByType("item_recpath");
                if (nItemRecPathCol == -1)
                {
                    strError = "盘点库 '' 的 browse 配置文件没有定义 type 为 item_recpath 的列。因此无法获得册记录路径";
                    return -1;
                }

                m_tableSummaryColIndex[strItemDbName] = nItemRecPathCol;   // 储存起来
            }
            else
                nItemRecPathCol = (int)o;

            Debug.Assert(nItemRecPathCol > 0, "");

            string strItemRecPath = ListViewUtil.GetItemText(item, nItemRecPathCol + 2);

            string strQueryString = "";
            strQueryString = "@path:" + strItemRecPath;

            string strOutputItemRecPath = "";
            if (string.IsNullOrEmpty(strQueryString) == false)
            {
                // Debug.Assert(this.DbType == "item" || this.DbType == "arrive", "");

                nRet = SearchTwoRecPathByBarcode(
                    this.stop,
                    this.Channel,
                    strQueryString,    // "@path:" + strRecPath,
                    out strOutputItemRecPath,
                    out strBiblioRecPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获得书目记录路径时出错: " + strError;
                    return -1;
                }
                else if (nRet == 0)
                {
                    strError = "检索词 '" + strQueryString + "' 没有找到记录";
                    return -1;
                }
                else if (nRet > 1) // 命中发生重复
                {
                    strError = "检索词 '" + strQueryString + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误";
                    return -1;
                }
            }

#if NO
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
                        strError = "获得书目记录路径时出错: " + strError;
                        return -1;
                    }
                    else if (nRet == 0)
                    {
                        strError = "记录路径 '" + strRecPath + "' 没有找到记录";
                        return -1;
                    }
                    else if (nRet > 1) // 命中发生重复
                    {
                        strError = "记录路径 '" + strRecPath + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误";
                        return -1;
                    }
#endif

            return 1;
        }


        // TODO: 如果被调用太密集，可在需要的时候解挂事件
        private void listView_records_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewUtil.OnSeletedIndexChanged(this.listView_inventoryList_records,
    0,
    null);
        }

        private void listView_records_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_inventoryList_records, e);
        }

        private void button_baseList_getLocations_Click(object sender, EventArgs e)
        {
            string strError = "";

            SelectBatchNoDialog dlg = new SelectBatchNoDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Channel = this.Channel;
            dlg.Stop = this.stop;
            dlg.InventoryDbName = "";
            this.MainForm.AppInfo.LinkFormState(dlg, "SelectBatchNoDialog_location_state");
            dlg.ShowDialog(this);

            this.textBox_baseList_locations.Text = StringUtil.MakePathList(dlg.SelectedBatchNo, "\r\n");
            return;
#if NO
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void button_baseList_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<string> location_list = StringUtil.SplitList(this.textBox_baseList_locations.Text.Replace("\r\n", "\n"), '\n');
            StringUtil.RemoveBlank(ref location_list);
            if (location_list.Count == 0)
            {
                // TODO: 没有指定馆藏地则检索出全部? 还是检索出全局的馆藏地(不包含分馆的)?
                strError = "请指定至少一个馆藏地";
                goto ERROR1;
            }

            for(int i = 0;i<location_list.Count;i++)
            {
                string location = location_list[i];
                if (location == "[空]" || location == "[blank]")
                    location_list[i] = "";
            }

            int nRet = DoSearchItems(location_list,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int DoSearchItems(List<string> location_list,
    out string strError)
        {
            strError = "";

            long lTotalCount = 0;   // 总共命中的记录数

            bool bAccessBiblioSummaryDenied = false;

            ClearItemListViewItems();

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.Channel.DoStop);
            stop.Initial("正在检索册记录 ...");
            stop.BeginLoop();

            this.DbType = "item";
            this.listView_baseList_records.BeginUpdate();
            this._listview = this.listView_baseList_records;
            this.timer_qu.Start();
            try
            {
                foreach (string location in location_list)
                {
                    string strResultSetName = "default";

                    long lRet = Channel.SearchItem(stop,
        "<all>",
        location,
        -1,
        "馆藏地点",
        "exact", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
        this.Lang,
        strResultSetName,
        "",    // strSearchStyle
        "", // strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
        out strError);

                    if (lRet == 0)
                        continue;

                    if (lRet == -1)
                        return -1;

                    long lHitCount = lRet;

                    stop.SetProgressRange(0, lTotalCount + lHitCount);
                    stop.Style = StopStyle.EnableHalfStop;

                    long lStart = 0;
                    long lPerCount = Math.Min(50, lHitCount);
                    Record[] searchresults = null;

                    // 获得结果集，装入listview
                    for (; ; )
                    {
                        stop.SetMessage("正在装入浏览信息 " + (lStart + lTotalCount + 1).ToString() + " - " + (lStart + lTotalCount + lPerCount).ToString() + " (命中 " + (lTotalCount + lHitCount).ToString() + " 条记录) ...");
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        lRet = this.Channel.GetSearchResult(
                            stop,
                            strResultSetName,   // strResultSetName
                            lStart,
                            lPerCount,
                            "id,cols",   // "id,cols"
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

                        List<ListViewItem> items = new List<ListViewItem>();

                        // 处理浏览结果
                        int i = 0;
                        foreach (Record record in searchresults)
                        {
                            Application.DoEvents();	// 出让界面控制权

                            if (stop != null && stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            ListViewItem item = Global.AppendNewLine(
        this.listView_baseList_records,
        record.Path,
        Global.InsertBlankColumn(record.Cols));

                            items.Add(item);
                            stop.SetProgressValue(lStart + i);
                            i++;
                        }

                        if (bAccessBiblioSummaryDenied == false)
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
                                return -1;
                            if (nRet == -2)
                                bAccessBiblioSummaryDenied = true;

                            if (nRet == 0)
                            {
                                // this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...";
                                this.ShowMessage("检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...", "yellow", true);
                                return 0;
                            }
                        }


                        lStart += searchresults.Length;
                        if (lStart >= lHitCount || lPerCount <= 0)
                            break;
                    }

                    lTotalCount += lHitCount;
                }
                return 0;
            }
            finally
            {
                this.timer_qu.Stop();
                this.listView_baseList_records.EndUpdate();
                this.DbType = "inventory";

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.Channel.DoStop);
                stop.Initial("");

                EnableControls(true);
            }
        }

        private void listView_baseList_records_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewUtil.OnSeletedIndexChanged(this.listView_baseList_records,
0,
null);

        }

        private void listView_baseList_records_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_baseList_records, e);
        }

        // 交叉运算
        // 用基准结果集减去盘点结果集，得到丢失册的集合
        // 验证存在的册，是绿色；丢失的册，是红色；外借状态的册，是蓝色
        private void button_statis_crossCompute_Click(object sender, EventArgs e)
        {
            this.ClearMessage();

            string strError = "";
            int nRet = DoCrossCompute(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            this.ShowMessage(strError, "red", true);
            // MessageBox.Show(this, strError);
        }

        enum LineType
        {
            Verified = 0,   // 经过验证存在的册
            Borrowed = 1,   // 外借状态
            Lost = 2,   // 丢失了的册
            OutOfRange = 3, // 超出基准集范围的册。有可能是被上架上错到了盘点范围的书架上的本应放在其他地点的册
        }

        int DoCrossCompute(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.listView_inventoryList_records.Items.Count == 0)
            {
                strError = "盘点集尚未装载";
                return -1;
            }

            if (this.listView_baseList_records.Items.Count == 0)
            {
                strError = "基准集尚未装载";
                return -1;
            }

            string strInventoryDbName = this.MainForm.GetUtilDbName("inventory");
            if (string.IsNullOrEmpty(strInventoryDbName) == true)
            {
                strError = "尚未定义盘点库";
                return -1;
            }

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.Channel.DoStop);
            stop.Initial("正在进行交叉运算 ...");
            stop.BeginLoop();

            this.ShowMessage("正在进行交叉运算 ...");
            Application.DoEvents();

            try
            {
                this.ShowMessage("正在清除标记 ...");
                Application.DoEvents();

                // *** 预备，清除以前的标记
                foreach (ListViewItem item in this.listView_baseList_records.Items)
                {
                    item.Tag = null;
                    item.BackColor = SystemColors.Window;
                }

                foreach (ListViewItem item in this.listView_inventoryList_records.Items)
                {
                    item.Tag = null;
                    item.BackColor = SystemColors.Window;
                }


                this.ShowMessage("正在准备 Hashtable ...");
                Application.DoEvents();

                // *** 第一步，准备好记录路径的 Hashtable
                Hashtable recpath_table = new Hashtable();
                foreach(ListViewItem item in this.listView_baseList_records.Items)
                {
                    string strRecPath = item.Text;
                    Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");
                    if (string.IsNullOrEmpty(strRecPath) == false)
                        recpath_table[strRecPath] = item;
                }

                this.ShowMessage("正在标记盘点册 ...");
                Application.DoEvents();
                // *** 第二步，标记盘点过的事项

                // 获得 册记录路径 的列号
                ColumnPropertyCollection temp = this.MainForm.GetBrowseColumnProperties(strInventoryDbName);
                int nCol = temp.FindColumnByType("item_recpath");
                if (nCol == -1)
                {
                    strError = "盘点库 '"+strInventoryDbName+"' 的 browse 配置文件中未定义 type 为 item_recpath 的列";
                    return -1;
                }
                nCol += 2;

                foreach(ListViewItem item in this.listView_inventoryList_records.Items)
                {
                    string strItemRecPath = ListViewUtil.GetItemText(item, nCol);
                    if (string.IsNullOrEmpty(strItemRecPath) == true)
                    {
                        strError = "发现册记录路径为空的 盘点记录行。操作中断";
                        return -1;
                    }
                    ListViewItem found = (ListViewItem)recpath_table[strItemRecPath];
                    if (found != null)
                    {
                        // 事项被验证
                        found.Tag = LineType.Verified;
                        SetLineColor(found, LineType.Verified);
                    }
                    else
                    {
                        // 事项超出基准集的范围了，设为黄色背景
                        item.Tag = LineType.OutOfRange;
                        SetLineColor(item, LineType.OutOfRange);
                    }
                }


                this.ShowMessage("正在标记外借和丢失册 ...");
                Application.DoEvents();

                // *** 第三步，标记借出状态的行 标记丢失状态的行

                foreach(ListViewItem item in this.listView_baseList_records.Items)
                {
                    // 没有验证过的行
                    if (item.Tag == null)
                    {
                        string strBorrower = "";
                        // 观察借阅者列
                        // return:
                        //      -2  没有找到列 type
                        //      -1  出错
                        //      >=0 列号
                        nRet = GetColumnText(item,
        "borrower",
        out strBorrower,
        out strError);
                        if (nRet == -2 || nRet == -1)
                            return -1;
                        if (string.IsNullOrEmpty(strBorrower) == false)
                            item.Tag = LineType.Borrowed;
                        else
                            item.Tag = LineType.Lost;
                        SetLineColor(item, (LineType)item.Tag);
                    }

                }

                this.ShowMessage("完成", "green", true);
                return 0;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.Channel.DoStop);
                stop.Initial("");

                EnableControls(true);
            }
        }

        static void SetLineColor(ListViewItem item, LineType type)
        {
            if (type == LineType.Verified)
                item.BackColor = Color.LightGreen;
            else if (type == LineType.Borrowed)
                item.BackColor = Color.LightSkyBlue;
            else if (type == LineType.Lost)
                item.BackColor = Color.LightCoral;
            else if (type == LineType.OutOfRange)
                item.BackColor = Color.Yellow;
        }

        ListViewQU _listview = null;

        private void timer_qu_Tick(object sender, EventArgs e)
        {
            if (_listview != null)
                _listview.ForceUpdate();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_inventoryList_records);
                controls.Add(this.listView_baseList_records);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_inventoryList_records);
                controls.Add(this.listView_baseList_records);
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_statis_outputExcel_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bLaunchExcel = true;
            this.ClearMessage();

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

            XLWorkbook doc = null;

            try
            {
                doc = new XLWorkbook(XLEventTracking.Disabled);
                File.Delete(dlg.FileName);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

            this.ShowMessage("正在创建 Excel 报表");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建 Excel 报表 ...");
            stop.BeginLoop();

            EnableControls(false);
            this.listView_baseList_records.Enabled = false;
            this.listView_inventoryList_records.Enabled = false;
            try
            {
                List<string> output_columns = StringUtil.SplitList(this.OutputColumns);

                // return:
                //      -1  出错
                //      0   放弃或中断
                //      1   成功
                nRet = CreateLostSheet(
                    stop,
                    doc,
                    output_columns,
                    out strError);
                if (nRet != 1)
                    goto ERROR1;

                // return:
                //      -1  出错
                //      0   放弃或中断
                //      1   成功
                nRet = CreateOutOfRangeSheet(
                    stop,
                    doc,
                    output_columns,
                    out strError);
                if (nRet != 1)
                    goto ERROR1;

                // return:
                //      -1  出错
                //      0   放弃或中断
                //      1   成功
                nRet = CreateBorrowedSheet(
                    stop,
                    doc,
                    output_columns,
                    out strError);
                if (nRet != 1)
                    goto ERROR1;

                // return:
                //      -1  出错
                //      0   放弃或中断
                //      1   成功
                nRet = CreateVerifiedSheet(
                    stop,
                    doc,
                    output_columns,
                    out strError);
                if (nRet != 1)
                    goto ERROR1;
            }
            finally
            {
                EnableControls(true);
                this.listView_baseList_records.Enabled = true;
                this.listView_inventoryList_records.Enabled = true;

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

            this.ShowMessage("创建完成", "green", true);
            return;
        ERROR1:
            // MessageBox.Show(this, strError);
            this.ShowMessage(strError, "red", true);
        }

        // return:
        //      -1  出错
        //      0   放弃或中断
        //      1   成功
        int CreateLostSheet(
            Stop stop,
            XLWorkbook doc,
            List<string> output_columns,
            out string strError)
        {
            strError = "";
            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("丢失的册");

            // 为了让标题列文字正确出现，需要确保至少选择了一个行
            if (this.listView_baseList_records.SelectedItems.Count == 0
                && this.listView_baseList_records.Items.Count > 0)
                this.listView_baseList_records.Items[0].Selected = true;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach(ListViewItem item in this.listView_baseList_records.Items)
            {
                if (item.Tag == null)
                    continue;
                LineType type = (LineType)item.Tag;
                if (type == LineType.Lost)
                    items.Add(item);
            }

            // return:
            //      -1  出错
            //      0   放弃或中断
            //      1   成功
            int nRet = ExportToExcel(
                stop,
                this.listView_baseList_records,
                output_columns,
                items,
                sheet,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            return 1;
        }

        // return:
        //      -1  出错
        //      0   放弃或中断
        //      1   成功
        int CreateVerifiedSheet(
            Stop stop,
            XLWorkbook doc,
            List<string> output_columns,
            out string strError)
        {
            strError = "";
            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("验证存在的册");

            // 为了让标题列文字正确出现，需要确保至少选择了一个行
            if (this.listView_baseList_records.SelectedItems.Count == 0
                && this.listView_baseList_records.Items.Count > 0)
                this.listView_baseList_records.Items[0].Selected = true;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_baseList_records.Items)
            {
                if (item.Tag == null)
                    continue;
                LineType type = (LineType)item.Tag;
                if (type == LineType.Verified)
                    items.Add(item);
            }

            // return:
            //      -1  出错
            //      0   放弃或中断
            //      1   成功
            int nRet = ExportToExcel(
                stop,
                this.listView_baseList_records,
                output_columns,
                items,
                sheet,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            return 1;
        }

        // return:
        //      -1  出错
        //      0   放弃或中断
        //      1   成功
        int CreateBorrowedSheet(
            Stop stop,
            XLWorkbook doc,
            List<string> output_columns,
            out string strError)
        {
            strError = "";
            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("在借状态的册");

            // 为了让标题列文字正确出现，需要确保至少选择了一个行
            if (this.listView_baseList_records.SelectedItems.Count == 0
                && this.listView_baseList_records.Items.Count > 0)
                this.listView_baseList_records.Items[0].Selected = true;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_baseList_records.Items)
            {
                if (item.Tag == null)
                    continue;
                LineType type = (LineType)item.Tag;
                if (type == LineType.Borrowed)
                    items.Add(item);
            }

            // return:
            //      -1  出错
            //      0   放弃或中断
            //      1   成功
            int nRet = ExportToExcel(
                stop,
                this.listView_baseList_records,
                output_columns,
                items,
                sheet,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            return 1;
        }

        // return:
        //      -1  出错
        //      0   放弃或中断
        //      1   成功
        int CreateOutOfRangeSheet(
            Stop stop,
            XLWorkbook doc,
            List<string> output_columns,
            out string strError)
        {
            strError = "";
            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("超出盘点馆藏地范围的册");

            // 为了让标题列文字正确出现，需要确保至少选择了一个行
            if (this.listView_inventoryList_records.SelectedItems.Count == 0
                && this.listView_inventoryList_records.Items.Count > 0)
                this.listView_inventoryList_records.Items[0].Selected = true;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_inventoryList_records.Items)
            {
                if (item.Tag == null)
                    continue;
                LineType type = (LineType)item.Tag;
                if (type == LineType.OutOfRange)
                    items.Add(item);
            }

            // return:
            //      -1  出错
            //      0   放弃或中断
            //      1   成功
            int nRet = ExportToExcel(
                stop,
                this.listView_inventoryList_records,
                output_columns,
                items,
                sheet,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            return 1;
        }

        // parameters:
        //      output_columns    输出列定义。如果为空，表示全部输出。这是一个数字列表，例如 "1,2,3,5"
        // return:
        //      -1  出错
        //      0   放弃或中断
        //      1   成功
        public static int ExportToExcel(
            Stop stop,
            ListView list,
            List<string> output_columns,
            List<ListViewItem> items,
            IXLWorksheet sheet,
            out string strError)
        {
            strError = "";
#if NO
            if (items == null || items.Count == 0)
            {
                strError = "items == null || items.Count == 0";
                return -1;
            }
#endif

            // ListView list = items[0].ListView;
            if (stop != null)
                stop.SetProgressRange(0, items.Count);

            List<int> indices = new List<int>();
            if (output_columns == null && output_columns.Count == 0)
            {
                int i = 0;
                foreach (ColumnHeader header in list.Columns)
                {
                    indices.Add(i++);
                }
            }
            else
            {
                foreach (string s in output_columns)
                {
                    int v = 0;
                    if (Int32.TryParse(s, out v) == false)
                    {
                        strError = "output_columns 数组中有非数字的字符串，格式错误";
                        return -1;
                    }
                    indices.Add(v - 1);   // 从 0 开始计数
                }
            }

            // 每个列的最大字符数
            List<int> column_max_chars = new List<int>();

            List<XLAlignmentHorizontalValues> alignments = new List<XLAlignmentHorizontalValues>();
            //foreach (ColumnHeader header in list.Columns)
            foreach (int index in indices)
            {
                ColumnHeader header = list.Columns[index];

                if (header.TextAlign == HorizontalAlignment.Center)
                    alignments.Add(XLAlignmentHorizontalValues.Center);
                else if (header.TextAlign == HorizontalAlignment.Right)
                    alignments.Add(XLAlignmentHorizontalValues.Right);
                else
                    alignments.Add(XLAlignmentHorizontalValues.Left);

                column_max_chars.Add(0);
            }

            string strFontName = list.Font.FontFamily.Name;

            int nRowIndex = 1;
            int nColIndex = 1;
            // foreach (ColumnHeader header in list.Columns)
            foreach (int index in indices)
            {
                ColumnHeader header = list.Columns[index];

                IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(header.Text);
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontName = strFontName;
                cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                nColIndex++;
            }
            nRowIndex++;

            //if (stop != null)
            //    stop.SetMessage("");
            foreach (ListViewItem item in items)
            {
                Application.DoEvents();

                if (stop != null
    && stop.State != 0)
                {
                    strError = "用户中断";
                    return 0;
                }

                // List<CellData> cells = new List<CellData>();

                nColIndex = 1;
                // foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                foreach (int index in indices)
                {
                    string strText = "";
                    ListViewItem.ListViewSubItem subitem = null;
                    if (index < item.SubItems.Count)
                    {
                        subitem = item.SubItems[index];
                        strText = subitem.Text;
                    }
                    else
                    {
                    }

                    // 统计最大字符数
                    int nChars = column_max_chars[nColIndex - 1];
                    if (string.IsNullOrEmpty(strText) == false && strText.Length > nChars)
                    {
                        column_max_chars[nColIndex - 1] = strText.Length;
                    }
                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(strText);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.FontName = strFontName;
                    cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                }

                if (stop != null)
                    stop.SetProgressValue(nRowIndex - 1);

                nRowIndex++;
            }

            if (stop != null)
                stop.SetMessage("正在调整列宽度 ...");
            Application.DoEvents();

            double char_width = ClosedXmlUtil.GetAverageCharPixelWidth(list);

            // 字符数太多的列不要做 width auto adjust
            const int MAX_CHARS = 30;   // 60
            {
                int i = 0;
                foreach (IXLColumn column in sheet.Columns())
                {
                    int nChars = column_max_chars[i];
                    if (nChars < MAX_CHARS)
                        column.AdjustToContents();
                    else
                        column.Width = (double)list.Columns[i].Width / char_width;  // Math.Min(MAX_CHARS, nChars);
                    i++;
                }
            }

            return 1;
        }

        private void button_statis_defOutputColumns_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_baseList_records.Items.Count == 0)
            {
                strError = "基准集列表中必须有至少一行内容，才能对报表栏目进行配置";
                goto ERROR1;
            }

            // 确保选中至少一行，这样列标题才能被初始化
            if (this.listView_baseList_records.SelectedItems.Count == 0)
                this.listView_baseList_records.Items[0].Selected = true;

            SelectColumnDialog dlg = new SelectColumnDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ListView = this.listView_baseList_records;
            dlg.NumberList = StringUtil.SplitList(this.OutputColumns);
            this.MainForm.AppInfo.LinkFormState(dlg, "SelectColumnDialog_state");
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.OutputColumns = StringUtil.MakePathList(dlg.NumberList);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public string OutputColumns
        {
            get
            {
                return this.MainForm.AppInfo.GetString("inventory_form", "output_columns", "<all>");
            }
            set
            {
                this.MainForm.AppInfo.SetString("inventory_form", "output_columns", value);
            }
        }
    }
}
