using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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

            ListViewProperty prop = new ListViewProperty();
            this.listView_inventoryList_records.Tag = prop;
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
            ListViewProperty prop = (ListViewProperty)this.listView_inventoryList_records.Tag;
            prop.ClearCache();
        }

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
        }

        private void InventoryForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void InventoryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._chargingForm.Close();
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

            int nRet = DoSearch(batchNo_list,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        internal new void ClearListViewItems()
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

        int DoSearch(List<string> batchNo_list,
            out string strError)
        {
            strError = "";

            bool bAccessBiblioSummaryDenied = false;

            ClearListViewItems();

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
                    // stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");
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

        // parameters:
        //      lStartIndex 调用前已经做过的事项数。为了准确显示 Progress
        // return:
        //      -2  获得书目摘要的权限不够
        //      -1  出错
        //      0   用户中断
        //      1   完成
        internal new int _fillBiblioSummaryColumn(List<ListViewItem> items,
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

    }
}
