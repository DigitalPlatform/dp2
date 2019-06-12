using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using ClosedXML.Excel;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Core;

// 2017/4/9 从 this.Channel 用法改造为 ChannelPool 用法

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
                prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetBaseListColumnTitles);
                prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetBaseListColumnTitles);

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
            ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(e.DbName);
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

        void prop_GetBaseListColumnTitles(object sender, GetColumnTitlesEventArgs e)
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
            ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties("[inventory_item]");  // e.DbName
            if (temp != null)
            {
                if (m_bBiblioSummaryColumn == true)
                    e.ColumnTitles.Insert(0, "书目摘要");
                e.ColumnTitles.AddRange(temp);  // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件
            }

#if NO
            if (this.m_bFirstColumnIsKey == true)
                e.ColumnTitles.Insert(0, "命中的检索点");
#endif
            // 右侧补充的列
            if (this._defs != null)
            {
                ColumnPropertyCollection inventory_properties = Program.MainForm.GetBrowseColumnProperties(this._defs.InventoryDbName);
                if (inventory_properties != null)
                {
                    foreach (int index in this._defs.source_indices)
                    {
                        if (index < inventory_properties.Count)
                        {
                            ColumnProperty source_prop = inventory_properties[index];
                            e.ColumnTitles.Add(source_prop);
                        }
                    }
                }
            }
        }


        private void InventoryForm_Load(object sender, EventArgs e)
        {
            this._chargingForm.SuppressSizeSetting = true; // 避免保存窗口尺寸
            this._chargingForm.MainForm = Program.MainForm;
            this._chargingForm.ShowInTaskbar = false;
            this._chargingForm.Show();  // 有了此句对话框的 xxx_load 才能被执行
            this._chargingForm.Hide();  // 有了此句可避免主窗口背后显示一个空对话框窗口
            // this._chargingForm.WindowState = FormWindowState.Minimized;
            this._chargingForm.SmartFuncState = FuncState.InventoryBook;
            this._chargingForm.FloatingMessageForm = this.FloatingMessageForm;
#if NO
                        // 输入的ISO2709文件名
            this._openMarcFileDialog.FileName = Program.MainForm.AppInfo.GetString(
                "iso2709statisform",
                "input_iso2709_filename",
                "");
#endif
            {
                List<string> librarycodes = GetOwnerLibraryCodes();
                this.inventoryBatchNoControl_start_batchNo.LibraryCodeList = librarycodes;
                this.inventoryBatchNoControl_start_batchNo.Text = Program.MainForm.AppInfo.GetString("inventory_form", "batch_no", "");
                if (librarycodes.Count == 1)
                {
                    if (this.inventoryBatchNoControl_start_batchNo.LibraryCodeText != librarycodes[0])
                        this.inventoryBatchNoControl_start_batchNo.Text = librarycodes[0] + "-";
                    this.inventoryBatchNoControl_start_batchNo.LibaryCodeEanbled = false;
                }
            }
            this.UiState = Program.MainForm.AppInfo.GetString(
    "inventory_form",
    "ui_state",
    "");

            this._statisInfo = null;
            this.SetButtonState(null);

            this.Channel = null;    // testing

            this.BeginInvoke(new Action(Initial));
        }

        ColumnDefs _defs = null;

        // 初始化
        void Initial()
        {
            string strError = "";
            ColumnDefs defs = null;
            int nRet = PrepareColumnDefs(out defs, out strError);
            if (nRet == -1)
            {
                this.ShowMessage(strError, "red", true);
                this.tabControl_main.Enabled = false;
                this._defs = null;
                return;
            }

            this._defs = defs;
        }

        private void InventoryForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void InventoryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._chargingForm.Close();

            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetString(
    "inventory_form",
    "ui_state",
    this.UiState);
                Program.MainForm.AppInfo.SetString(
                    "inventory_form",
                    "batch_no",
                    this.inventoryBatchNoControl_start_batchNo.Text);
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this._chargingForm.MainPanel.Enabled = bEnable;

            this.inventoryBatchNoControl_start_batchNo.Enabled = bEnable;
            this.textBox_start_locations.Enabled = bEnable;
            this.button_start_setLocations.Enabled = bEnable;
            this.button_start_restoreCfgs.Enabled = bEnable;

            this.textBox_inventoryList_batchNo.Enabled = bEnable;
            this.button_inventoryList_getBatchNos.Enabled = bEnable;
            this.button_inventoryList_search.Enabled = bEnable;

            this.textBox_baseList_locations.Enabled = bEnable;
            this.button_baseList_getLocations.Enabled = bEnable;
            this.button_baseList_search.Enabled = bEnable;

            this.textBox_operLog_dateRange.Enabled = bEnable;
            this.button_operLog_load.Enabled = bEnable;

            this.SetButtonState(bEnable == false ? null : this._statisInfo,
                false);   // 只改变 Enabled 状态，不修改按钮文字
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
            if (keyData == Keys.Enter 
                || keyData == Keys.LineFeed)  // 2019/6/12
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


        private void button_list_getBatchNos_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strInventoryDbName = this._defs.InventoryDbName;
            if (string.IsNullOrEmpty(strInventoryDbName) == true)
            {
                strError = "尚未定义盘点库";
                goto ERROR1;
            }

            LibraryChannel channel = this.GetChannel();

            try
            {
                SelectBatchNoDialog dlg = new SelectBatchNoDialog();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.Channel = channel;
                dlg.Stop = this.stop;
                dlg.InventoryDbName = strInventoryDbName;
                dlg.LibraryCodeList = GetOwnerLibraryCodes();
                Program.MainForm.AppInfo.LinkFormState(dlg, "SelectBatchNoDialog_state");
                dlg.ShowDialog(this);

                this.textBox_inventoryList_batchNo.Text = StringUtil.MakePathList(dlg.SelectedBatchNo, "\r\n");
                return;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
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

            this._statisInfo = null;
            this.SetButtonState(null);

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

        internal void ClearBaseListViewItems()
        {
            this.listView_baseList_records.Items.Clear();

            ListViewUtil.ClearSortColumns(this.listView_baseList_records);

            // 清除所有需要确定的栏标题
            for (int i = 1; i < this.listView_baseList_records.Columns.Count; i++)
            {
                this.listView_baseList_records.Columns[i].Text = i.ToString();
            }

            ClearBiblioTable();
            ClearCommentViewer();
        }

        int DoSearchInventory(List<string> batchNo_list,
            out string strError)
        {
            strError = "";

            bool bAccessBiblioSummaryDenied = false;

            ClearInventoryListViewItems();

            string strDbName = this._defs.InventoryDbName;
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

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            // stop.OnStop += new StopEventHandler(this.Channel.DoStop);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索盘点记录 ...");
            stop.BeginLoop();

            this.listView_inventoryList_records.BeginUpdate();
            this._listview = this.listView_inventoryList_records;
            this.timer_qu.Start();
            try
            {
                // 开始检索
                long lRet = channel.Search(
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

                    lRet = channel.GetSearchResult(
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
                        int nRet = _fillBiblioSummaryColumn0(
                            channel,
                            items,
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
                // stop.OnStop -= new StopEventHandler(this.Channel.DoStop);
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

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
        internal int _fillBiblioSummaryColumn0(
            LibraryChannel channel,
            List<ListViewItem> items,
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

            List<ListViewItem> all_items = new List<ListViewItem>();
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
                    ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
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
                    strBiblioRecPath = Program.MainForm.GetBiblioDbNameFromItemDbName(this.DbType, strItemDbName);
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
                //int nCol = -1;
                string strBiblioRecPath = "";
                // 获得事项所从属的书目记录的路径
                // return:
                //      -1  出错
                //      0   相关数据库没有配置 parent id 浏览列
                //      1   找到
                nRet = GetBiblioRecPath(
                    channel,
                    item,
                    out strBiblioRecPath,
                    out strError);
                if (nRet == -1)
                {
                    ListViewUtil.ChangeItemText(item,
    this.m_bFirstColumnIsKey == false ? 1 : 2,
    "!error: " + strError);
                    continue;
                    // return -1;
                }
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
                all_items.Add(item);
            }

            if (biblio_recpaths.Count == 0)
                return 1;

            CacheableBiblioLoader loader = new CacheableBiblioLoader();
            loader.Channel = channel;
            loader.Stop = this.stop;
            loader.Format = "summary";
            loader.GetBiblioInfoStyle = GetBiblioInfoStyle.None;
            loader.RecPaths = biblio_recpaths;

            loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
            loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

            var enumerator = loader.GetEnumerator();

            int i = 0;
            foreach (ListViewItem item in all_items)
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
                    strError = "enumerator.MoveNext() exception: " + ExceptionUtil.GetAutoText(ex);
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
        int GetBiblioRecPath(
            LibraryChannel channel,
            ListViewItem item,
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
                if (Program.MainForm.NormalDbProperties == null)
                {
                    strError = "普通数据库属性尚未初始化";
                    return -1;
                }
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
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
                    channel,
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
        private void listView_inventoryList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewUtil.OnSelectedIndexChanged(this.listView_inventoryList_records,
    0,
    null);
        }

        private void listView_inventoryList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_inventoryList_records, e);
        }

        private void button_baseList_getLocations_Click(object sender, EventArgs e)
        {
            // string strError = "";

            LibraryChannel channel = this.GetChannel();

            try
            {
                SelectBatchNoDialog dlg = new SelectBatchNoDialog();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.Channel = channel;
                dlg.Stop = this.stop;
                dlg.InventoryDbName = "";
                Program.MainForm.AppInfo.LinkFormState(dlg, "SelectBatchNoDialog_location_state");
                dlg.ShowDialog(this);

                this.textBox_baseList_locations.Text = StringUtil.MakePathList(dlg.SelectedBatchNo, "\r\n");
                return;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
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

            for (int i = 0; i < location_list.Count; i++)
            {
                string location = location_list[i];
                if (location == "[空]" || location == "[blank]")
                    location_list[i] = "";
            }

            this._statisInfo = null;
            this.SetButtonState(null);

            int nRet = DoSearchBaseItems(location_list,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // return:
        //      -1  出错
        //      0   没有找到定义
        //      1   找到
        public int GetColumnDefString(out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            if (Program.MainForm.NormalDbProperties == null)
            {
                strError = "普通数据库属性尚未初始化";
                return -1;
            }

            ColumnPropertyCollection defs = Program.MainForm.GetBrowseColumnProperties("[inventory_item]");
            if (defs == null)
            {
                strError = "没有找到 [inventory_item] 的列定义";
                return 0;
            }

            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (ColumnProperty prop in defs)
            {
                if (i > 0)
                    text.Append("|");
                text.Append(prop.XPath);
                if (string.IsNullOrEmpty(prop.Convert) == false)
                    text.Append("->" + prop.Convert);
                i++;
            }

            strResult = text.ToString();
            return 1;
        }

        // 检索基准集合
        int DoSearchBaseItems(List<string> location_list,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            long lTotalCount = 0;   // 总共命中的记录数

            bool bAccessBiblioSummaryDenied = false;

            ClearBaseListViewItems();

            EnableControls(false);

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索册记录 ...");
            stop.BeginLoop();

            this.DbType = "item";
            this.listView_baseList_records.BeginUpdate();
            this._listview = this.listView_baseList_records;
            this.timer_qu.Start();
            try
            {

#if NO
                string strColumnDef = "";
                // return:
                //      -1  出错
                //      0   没有找到定义
                //      1   找到
                nRet = GetColumnDefString(out strColumnDef,
            out strError);
                if (nRet == -1)
                    return -1;
#endif

                string strBrowseStyle = "id,cols,format:@coldef:" + this._defs.BrowseColumnDef;

                foreach (string location in location_list)
                {
                    string strResultSetName = "default";

                    long lRet = channel.SearchItem(stop,
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

                        lRet = channel.GetSearchResult(
                            stop,
                            strResultSetName,   // strResultSetName
                            lStart,
                            lPerCount,
                            strBrowseStyle,   // "id,cols"
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
                            nRet = _fillBiblioSummaryColumn(
                                channel,
                                items,
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
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                EnableControls(true);
            }
        }

        // 获得事项所从属的书目记录的路径
        // parameters:
        //      bAutoSearch 当没有 parent id 列的时候，是否自动进行检索以便获得书目记录路径
        // return:
        //      -1  出错
        //      0   相关数据库没有配置 parent id 浏览列
        //      1   找到
        public override int GetBiblioRecPath(
            LibraryChannel channel,
            ListViewItem item,
            bool bAutoSearch,
            out int nCol,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            nCol = -1;
            strBiblioRecPath = "";
            int nRet = 0;

            // 这是事项记录路径
            string strRecPath = ListViewUtil.GetItemText(item, 0);

            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                strError = "浏览行记录路径列没有内容，无法获得书目记录路径";
                return -1;
            }

            // 根据记录路径获得数据库名
            string strItemDbName = Global.GetDbName(strRecPath);
            // 根据数据库名获得 parent id 列号

            object o = m_tableSummaryColIndex[strItemDbName];
            if (o == null)
            {
                if (Program.MainForm.NormalDbProperties == null)
                {
                    strError = "普通数据库属性尚未初始化";
                    return -1;
                }
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties("[inventory_item]");
                if (temp == null)
                {
                    strError = "实体库 '" + strItemDbName + "' 没有找到列定义 [inventory_item]";
                    return -1;
                }

                nCol = temp.FindColumnByType("parent_id");
                if (nCol == -1)
                {
                    if (bAutoSearch == false)
                    {
                        strError = "实体库 " + strItemDbName + " 的浏览格式没有配置 parent id 列";
                        return 0;   // 这个实体库没有 parent id 列
                    }

                    // TODO: 这里关于浏览列 type 的判断应该通盘考虑，设计为通用功能，减少对特定库类型的判断依赖

                    string strQueryString = "";
                    if (this.DbType == "item")
                    {
                        Debug.Assert(this.DbType == "item", "");

                        strQueryString = "@path:" + strRecPath;
                    }
                    else if (this.DbType == "arrive")
                    {
                        int nRefIDCol = temp.FindColumnByType("item_refid");
                        if (nRefIDCol == -1)
                        {
                            strError = "预约到书库 " + strItemDbName + " 的浏览格式没有配置 item_refid 列";
                            return 0;
                        }

                        strQueryString = ListViewUtil.GetItemText(item, nRefIDCol + 2);

                        if (string.IsNullOrEmpty(strQueryString) == false)
                        {
                            strQueryString = "@refID:" + strQueryString;
                        }
                        else
                        {
                            int nItemBarcodeCol = temp.FindColumnByType("item_barcode");
                            if (nRefIDCol == -1)
                            {
                                strError = "预约到书库 " + strItemDbName + " 的浏览格式没有配置 item_barcode 列";
                                return 0;
                            }
                            strQueryString = ListViewUtil.GetItemText(item, nItemBarcodeCol + 2);
                            if (string.IsNullOrEmpty(strQueryString) == true)
                            {
                                strError = "册条码号栏为空，无法获得书目记录路径";
                                return 0;
                            }
                        }
                    }

                    string strItemRecPath = "";
                    if (string.IsNullOrEmpty(strQueryString) == false)
                    {
                        Debug.Assert(this.DbType == "item" || this.DbType == "arrive", "");

                        nRet = SearchTwoRecPathByBarcode(
                            this.stop,
                            channel,
                            strQueryString,    // "@path:" + strRecPath,
                            out strItemRecPath,
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
                    else
                    {
                        nRet = SearchBiblioRecPath(
                            this.stop,
                            channel,
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

                    return 1;
                }

                nCol += 2;
                if (this.m_bFirstColumnIsKey == true)
                    nCol++; // 2013/11/12
                m_tableSummaryColIndex[strItemDbName] = nCol;   // 储存起来
            }
            else
                nCol = (int)o;

            Debug.Assert(nCol > 0, "");

            // 获得 parent id
            string strText = ListViewUtil.GetItemText(item, nCol);

            // 看看是否已经是路径 ?
            if (strText.IndexOf("/") == -1)
            {
                // 获得对应的书目库名
                strBiblioRecPath = Program.MainForm.GetBiblioDbNameFromItemDbName(this.DbType, strItemDbName);
                if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                {
                    strError = this.DbTypeCaption + "类型的数据库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                    return -1;
                }
                strBiblioRecPath = strBiblioRecPath + "/" + strText;

                ListViewUtil.ChangeItemText(item, nCol, strBiblioRecPath);
            }
            else
                strBiblioRecPath = strText;

            return 1;
        }

        private void listView_baseList_records_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewUtil.OnSelectedIndexChanged(this.listView_baseList_records,
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

        // 行状态
        enum LineType
        {
            Origin = 0, // 原始的，没有经过任何验证的册
            Verified = 1,   // 经过验证存在的册
            Borrowed = 2,   // 外借状态
            Lost = 3,   // 丢失了的册
            OutOfRange = 4, // 超出基准集范围的册。有可能是被上架上错到了盘点范围的书架上的本应放在其他地点的册
        }

        StatisInfo _statisInfo = new StatisInfo();

        // TODO: 本来就是 丢失 或 注销 状态的册，是否不必给它们加上红色底色? 或者用不同的红色?
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

            this._statisInfo = new StatisInfo();

            this._statisInfo.ItemsVerified = this.listView_inventoryList_records.Items.Count;
            this._statisInfo.ItemsBase = this.listView_baseList_records.Items.Count;
#if NO
            string strInventoryDbName = Program.MainForm.GetUtilDbName("inventory");
            if (string.IsNullOrEmpty(strInventoryDbName) == true)
            {
                strError = "尚未定义盘点库";
                return -1;
            }
#endif

            string strInventoryDbName = this._defs.InventoryDbName;

            int nStateColumnIndex = this._defs._base_colmun_defs.FindColumnByType("item_state");
            if (nStateColumnIndex == -1)
            {
                strError = "基准集列表中 type 为 item_state 的列没有定义 ...";
                return -1;
            }
            int nBorrowerColumnIndex = this._defs._base_colmun_defs.FindColumnByType("borrower");
            if (nBorrowerColumnIndex == -1)
            {
                strError = "基准集列表中 type 为 borrower 的列没有定义 ...";
                return -1;
            }

            int nTemp = this._defs.source_types.IndexOf("oper_time");
            if (nTemp == -1)
            {
                strError = "盘点集列表中 type 为 oper_time 的列没有定义 ...";
                return -1;
            }
            int nOpertimeColumnIndex = this._defs._base_colmun_defs.Count + nTemp + 2;

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行交叉运算 ...");
            stop.BeginLoop();

            this.ShowMessage("正在进行交叉运算 ...");
            Application.DoEvents();

            try
            {
                this.ShowMessage("正在清除标记 ...");
                Application.DoEvents();

                List<ListViewItem> delete_items = new List<ListViewItem>();
                // *** 预备，清除以前的标记
                foreach (ListViewItem item in this.listView_baseList_records.Items)
                {
                    string strState = ListViewUtil.GetItemText(item, nStateColumnIndex + 2);
                    if (string.IsNullOrEmpty(strState) == false
                        && (strState[0] == '+' || strState[0] == '-'))
                    {
                        ListViewUtil.ChangeItemText(item, nStateColumnIndex + 2, strState.Substring(1));
                        item.Font = this.Font;
                    }

                    if (item.Tag == null)
                        continue;

                    // 需要删除的 item 
                    LineType type = (LineType)item.Tag;
                    if (type == LineType.OutOfRange)
                        delete_items.Add(item);

                    item.Tag = null;
                    item.BackColor = SystemColors.Window;
                }

                foreach (ListViewItem item in delete_items)
                {
                    this.listView_baseList_records.Items.Remove(item);
                }

                foreach (ListViewItem item in this.listView_inventoryList_records.Items)
                {
                    item.Tag = null;
                    item.BackColor = SystemColors.Window;
                }

                // *** 第一步，准备好记录路径的 Hashtable
                this.ShowMessage("正在准备 Hashtable ...");
                Application.DoEvents();

                Hashtable base_recpath_table = new Hashtable();  // recpath --> ListViewItem
                foreach (ListViewItem item in this.listView_baseList_records.Items)
                {
                    string strRecPath = item.Text;
                    Debug.Assert(string.IsNullOrEmpty(strRecPath) == false, "");
                    if (string.IsNullOrEmpty(strRecPath) == false)
                        base_recpath_table[strRecPath] = item;
                }

                // *** 第二步，标记盘点过的事项
                this.ShowMessage("正在标记盘点册 ...");
                Application.DoEvents();

                // 获得 册记录路径 的列号
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strInventoryDbName);
                int nCol = temp.FindColumnByType("item_recpath");
                if (nCol == -1)
                {
                    strError = "盘点库 '" + strInventoryDbName + "' 的 browse 配置文件中未定义 type 为 item_recpath 的列";
                    return -1;
                }
                nCol += 2;

                // 确保基准列表的列标题个数足够
                int nMax = this._defs._base_colmun_defs.Count + 2 + this._defs.source_indices.Count;
                ListViewUtil.EnsureColumns(this.listView_baseList_records, nMax);

                List<ListViewItem> outofrange_source_items = new List<ListViewItem>();
                // List<string> outof_range_recpaths = new List<string>();
                foreach (ListViewItem item in this.listView_inventoryList_records.Items)
                {
                    string strItemRecPath = ListViewUtil.GetItemText(item, nCol);
                    if (string.IsNullOrEmpty(strItemRecPath) == true)
                    {
                        strError = "发现册记录路径为空的 盘点记录行。操作中断";
                        return -1;
                    }
                    ListViewItem found = (ListViewItem)base_recpath_table[strItemRecPath];
                    if (found != null)
                    {
                        // 事项被验证
                        found.Tag = LineType.Verified;
                        SetLineColor(found, LineType.Verified);

                        // 补充列
                        AppendColumns(this._defs, found, item);
                    }
                    else
                    {
                        // 事项超出基准集的范围了，设为黄色背景
                        item.Tag = LineType.OutOfRange;
                        SetLineColor(item, LineType.OutOfRange);
                        outofrange_source_items.Add(item);
                    }
                }

                this.ShowMessage("正在标记超范围册 ...");
                Application.DoEvents();

                // 将超出范围的册记录添加到基准集中
                nRet = AddOutOfRangeItemsToBaseList(
                    this._defs,
                    outofrange_source_items,
                    out strError);
                if (nRet == -1)
                    return -1;

                this._statisInfo.ItemsOutofRange = nRet;

                // *** 第三步，标记操作日志中最后动作为 return 的行，这也是一种类似盘点的验证
                // 一个册，在盘点动作后时间之后，又发生了借出动作，表明这一册的状态就不是验证过的状态了；
                // 发生了还回操作表示这是验证过的状态。等于要把盘点动作纳入时间线考察，如果后来被借出代替了就要修改为未验证状态
                foreach (OperLogData data in _operLogItems)
                {
                    ListViewItem found = (ListViewItem)base_recpath_table[data.ItemRecPath];
                    if (found != null)
                    {
                        if (found.Tag != null)
                        {
                            LineType type = (LineType)found.Tag;
                            if (type == LineType.OutOfRange)
                                continue;
                        }

                        string strOperTime = ListViewUtil.GetItemText(found, nOpertimeColumnIndex);
                        if (string.CompareOrdinal(data.OperTime, strOperTime) < 0)
                            continue;   // 只让时间靠后的发生作用

                        if (data.Action == "return")
                        {
                            // 观察它的状态，如果是超范围的册则不做处理
                            if (found.Tag != null)
                            {
                                LineType type = (LineType)found.Tag;
                                if (type == LineType.OutOfRange
                                    || type == LineType.Verified)
                                    continue;
                            }

                            // 事项被验证
                            found.Tag = LineType.Verified;
                            SetLineColor(found, LineType.Verified);

                            // 补充列。补充操作者 操作时间列
                            AppendExtraColumns(found, data);

                            this._statisInfo.ItemsVerified++;
                        }

                        if (data.Action == "borrow")
                        {
                            if (found.Tag != null)
                            {
                                LineType type = (LineType)found.Tag;
                                // 把已经处于验证状态的行修改为原始状态
                                if (type == LineType.Verified)
                                {
                                    found.Tag = null;   // 表示没有被验证
                                    SetLineColor(found, LineType.Origin);
                                    // 补充列。补充操作者 操作时间列
                                    AppendExtraColumns(found, data);

                                    this._statisInfo.ItemsVerified--;
                                }
                            }
                        }
                    }
                }

                // *** 第四步，标记借出状态的行 标记丢失状态的行
                this.ShowMessage("正在标记外借和丢失册 ...");
                Application.DoEvents();

                this._statisInfo.ItemsBorrowed = 0;
                this._statisInfo.ItemsLost = 0;
                this._statisInfo.ItemsNeedAddLostState = 0;

                foreach (ListViewItem item in this.listView_baseList_records.Items)
                {
                    // 没有验证过的行
                    if (item.Tag == null)
                    {
#if NO
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
#endif
                        string strBorrower = ListViewUtil.GetItemText(item, nBorrowerColumnIndex + 2);

                        if (string.IsNullOrEmpty(strBorrower) == false)
                        {
                            item.Tag = LineType.Borrowed;
                            this._statisInfo.ItemsBorrowed++;
                        }
                        else
                        {
                            item.Tag = LineType.Lost;
                            this._statisInfo.ItemsLost++;
                        }
                        SetLineColor(item, (LineType)item.Tag);

                        // 进一步判断状态，标注那些新丢失的册
                        if ((LineType)item.Tag == LineType.Lost)
                        {
                            string strState = ListViewUtil.GetItemText(item, nStateColumnIndex + 2);

                            if (StringUtil.IsInList("丢失", strState) == false
    && StringUtil.IsInList("注销", strState) == false
    && string.IsNullOrEmpty(strBorrower) == true)
                            {
                                ListViewUtil.ChangeItemText(item, nStateColumnIndex + 2, "+" + strState);
                                item.Font = new Font(this.Font, FontStyle.Bold);
                                this._statisInfo.ItemsNeedAddLostState++;
                            }
                        }
                    }
                }

                // 微调外借状态
                // 在盘点期间，最后操作动作为 借 的册，如果先前它曾被盘点验证过，最终也只能算作外借状态的册
                // TODO: 如果盘点动作也进入操作日志体系的话，则从操作日志是能精确判断动作先后顺序的。因此能确切知道哪个动作在后。而现在的方法，当盘点完成后如果间隔了一段再进行统计，则册的外借状态就可能会失真了 --- 因为后面可能会发生新的借还操作，还可能不被纳入统计(日志时间)范围
                foreach (OperLogData data in _operLogItems)
                {
                    ListViewItem found = (ListViewItem)base_recpath_table[data.ItemRecPath];
                    if (found != null && data.Action == "borrow")
                    {
                        if (found.Tag == null)
                            continue;

                        string strBorrower = ListViewUtil.GetItemText(found, nBorrowerColumnIndex + 2);

                        LineType type = (LineType)found.Tag;
                        if (type == LineType.Verified
                            && string.IsNullOrEmpty(strBorrower) == false)
                        {
                            found.Tag = LineType.Borrowed;
                            SetLineColor(found, (LineType)found.Tag);
                            this._statisInfo.ItemsBorrowed++;
                            this._statisInfo.ItemsVerified--;
                        }
                    }
                }

                this._statisInfo.ItemsNeedRemoveLostState = 0;
                this._statisInfo.ItemsNeedReturn = 0;
                // 检查盘点过的册中，状态值 包含 丢失 和 注销 的，给状态值打上特殊标记，以便提醒操作者可以单独处理它们
                foreach (ListViewItem item in this.listView_baseList_records.Items)
                {
                    if (item.Tag == null)
                        continue;

                    LineType type = (LineType)item.Tag;
                    if (type == LineType.Verified)
                    {
                        string strState = ListViewUtil.GetItemText(item, nStateColumnIndex + 2);
                        if (StringUtil.IsInList("丢失", strState) == true
                            || StringUtil.IsInList("注销", strState) == true)
                        {
                            item.Font = new Font(this.Font, FontStyle.Bold);
                            ListViewUtil.ChangeItemText(item, nStateColumnIndex + 2, "-" + strState);
                            this._statisInfo.ItemsNeedRemoveLostState++;
                        }

                        string strBorrower = ListViewUtil.GetItemText(item, nBorrowerColumnIndex + 2);
                        if (string.IsNullOrEmpty(strBorrower) == false)
                            this._statisInfo.ItemsNeedReturn++;
                    }
                }

                SetButtonState(this._statisInfo);

                this.ShowMessage("完成", "green", true);
                return 0;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }
        }

        // 设置最后一个属性页的几个按钮的状态
        void SetButtonState(StatisInfo info, bool bSetText = true)
        {
            if (info == null)
            {
                this.button_statis_crossCompute.Enabled = true;

                this.button_statis_maskItems.Enabled = false;
                if (bSetText)
                    this.button_statis_maskItems.Text = "修改册状态";

                this.button_statis_return.Enabled = false;
                if (bSetText)
                    this.button_statis_return.Text = "补做还书";

                this.button_statis_outputExcel.Enabled = false;
                this.button_statis_defOutputColumns.Enabled = false;
                return;
            }

            this.button_statis_crossCompute.Enabled = true;

            this.button_statis_maskItems.Enabled = true;
            if (bSetText)
                this.button_statis_maskItems.Text = "修改册状态 (+" + info.ItemsNeedAddLostState + ", -" + info.ItemsNeedRemoveLostState + ")";

            this.button_statis_return.Enabled = true;
            if (bSetText)
                this.button_statis_return.Text = "补做还书 (" + info.ItemsNeedReturn + ")";

            this.button_statis_outputExcel.Enabled = true;
            this.button_statis_defOutputColumns.Enabled = true;
        }

        void AppendExtraColumns(ListViewItem item, OperLogData data)
        {
            if (this._defs == null)
                return;

            int nBatchNoIndex = this._defs.source_types.IndexOf("batch_no");
            int nOperatorIndex = this._defs.source_types.IndexOf("operator");
            int nOperTimeIndex = this._defs.source_types.IndexOf("oper_time");

            int target_index = this._defs._base_colmun_defs.Count + 2;
            if (nBatchNoIndex != -1)
                ListViewUtil.ChangeItemText(item, target_index + nBatchNoIndex, "#operlog");
            if (nOperatorIndex != -1)
                ListViewUtil.ChangeItemText(item, target_index + nOperatorIndex, data.Operator);
            if (nOperTimeIndex != -1)
                ListViewUtil.ChangeItemText(item, target_index + nOperTimeIndex, data.OperTime);
        }

        class ColumnDefs
        {
            public string InventoryDbName = "";
            public ColumnPropertyCollection _base_colmun_defs = null;
            public ColumnPropertyCollection _inventory_column_defs = null;

            public List<int> source_indices = new List<int>();  // 补充列的 index
            public List<string> source_types = new List<string>();  // 补充列的 type

            public int InventoryItemRecPathColumnIndex = -1;    // "盘点库 的 browse 配置文件中未定义 type 为 item_recpath 的列";

            public string BrowseColumnDef = ""; // 基准集浏览列定义。用于 GetSearchResult()

#if NO
            // 通过 type 找到一个补充列的 index
            public int FindTypeIndex(string type)
            {
                if (this.source_types == null)
                    return -1;
                Debug.Assert(this.source_types.Count == this.source_indices.Count, "");
                int i = 0;
                foreach(string s in this.source_types)
                {
                    if (s == type)
                        return this.source_indices[i];
                }
                return -1;  // not found
            }
#endif
        }

        int PrepareColumnDefs(out ColumnDefs defs, out string strError)
        {
            defs = null;
            strError = "";

            defs = new ColumnDefs();

            defs.InventoryDbName = Program.MainForm.GetUtilDbName("inventory");
            if (string.IsNullOrEmpty(defs.InventoryDbName) == true)
            {
                strError = "尚未定义盘点库";
                return -1;
            }

            {
                // 获得 册记录路径 的列号
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(defs.InventoryDbName);
                defs.InventoryItemRecPathColumnIndex = temp.FindColumnByType("item_recpath");
                if (defs.InventoryItemRecPathColumnIndex == -1)
                {
                    strError = "盘点库 '" + defs.InventoryDbName + "' 的 browse 配置文件中未定义 type 为 item_recpath 的列";
                    return -1;
                }

            }

            {
                defs._base_colmun_defs = Program.MainForm.GetBrowseColumnProperties("[inventory_item]");
                if (defs._base_colmun_defs == null)
                {
                    strError = "[inventory_item] 库列定义没有找到";
                    return -1;
                }
            }

            {
                // 获得 册记录路径 的列号
                defs._inventory_column_defs = Program.MainForm.GetBrowseColumnProperties(defs.InventoryDbName);
                if (defs._inventory_column_defs == null)
                {
                    strError = defs.InventoryDbName + " 库列定义没有找到";
                    return -1;
                }

                List<string> types = new List<string>();
                types.Add("batch_no");
                types.Add("operator");
                types.Add("oper_time");

                foreach (string type in types)
                {
                    int nCol = defs._inventory_column_defs.FindColumnByType(type);
                    if (nCol == -1)
                    {
                        strError = defs.InventoryDbName + " 库中 type 为 " + type + " 的列定义没有找到";
                        return -1;
                    }

                    defs.source_indices.Add(nCol);
                    defs.source_types.Add(type);
                }
            }

            {
                string strColumnDef = "";
                // return:
                //      -1  出错
                //      0   没有找到定义
                //      1   找到
                int nRet = GetColumnDefString(out strColumnDef,
            out strError);
                if (nRet == -1)
                    return -1;
                defs.BrowseColumnDef = strColumnDef;
            }

            return 0;
        }

        void AppendColumns(ColumnDefs defs, ListViewItem target, ListViewItem source)
        {
            int target_index = defs._base_colmun_defs.Count + 2;
            foreach (int source_index in defs.source_indices)
            {
                string strText = ListViewUtil.GetItemText(source, source_index + 2);
                ListViewUtil.ChangeItemText(target, target_index++, strText);
            }
        }

        // return:
        //      -1  出错
        //      其他  超出范围的事项的个数
        int AddOutOfRangeItemsToBaseList(
            ColumnDefs defs,
            List<ListViewItem> outofrange_source_items,
            // List<string> recpaths,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            int nCount = 0;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem source_item in outofrange_source_items)
            {
                string strItemRecPath = ListViewUtil.GetItemText(source_item, defs.InventoryItemRecPathColumnIndex + 2);
                ListViewItem item = new ListViewItem();
                item.Text = strItemRecPath;

                this.listView_baseList_records.Items.Add(item);

                items.Add(item);
                // 事项超出基准集的范围了，设为黄色背景
                item.Tag = LineType.OutOfRange;
                SetLineColor(item, LineType.OutOfRange);

                AppendColumns(defs, item, source_item);

                // 复制书目摘要列
                ListViewUtil.ChangeItemText(item, 1, ListViewUtil.GetItemText(source_item, 1));
                nCount++;
            }


            string strBrowseStyle = "id,cols,format:@coldef:" + this._defs.BrowseColumnDef;

            // 刷新浏览行
            nRet = RefreshListViewLines(
                null,
                items,
                strBrowseStyle,
                false,
                false,  // 不清除右侧多出来的列内容
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            // 刷新书目摘要
            bool bAccessBiblioSummaryDenied = false;
            if (bAccessBiblioSummaryDenied == false)
            {
                this.DbType = "item";
                try
                {
                    // return:
                    //      -2  获得书目摘要的权限不够
                    //      -1  出错
                    //      0   用户中断
                    //      1   完成
                    nRet = _fillBiblioSummaryColumn(items,
                        0,
                        false,
                        true,   // false,  // bAutoSearch
                        out strError);
                }
                finally
                {
                    this.DbType = "inventory";
                }
                if (nRet == -1)
                    return -1;
                if (nRet == -2)
                    bAccessBiblioSummaryDenied = true;

                if (nRet == 0)
                    this.ShowMessage("用户中断刷新书目摘要...", "yellow", true);
            }
#endif

            return nCount;
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
            else if (type == LineType.Origin)
                item.BackColor = SystemColors.Window;
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
                controls.Add(this.textBox_start_locations);
                controls.Add(this.textBox_baseList_locations);
                controls.Add(this.textBox_inventoryList_batchNo);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_inventoryList_records);
                controls.Add(this.listView_baseList_records);
                controls.Add(this.textBox_start_locations);
                controls.Add(this.textBox_baseList_locations);
                controls.Add(this.textBox_inventoryList_batchNo);
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
                strError = "new XLWorkbook() exception: " + ExceptionUtil.GetAutoText(ex);
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
            foreach (ListViewItem item in this.listView_baseList_records.Items)
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
            if (this.listView_baseList_records.SelectedItems.Count == 0
                && this.listView_baseList_records.Items.Count > 0)
                this.listView_baseList_records.Items[0].Selected = true;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_baseList_records.Items)
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
                        strError = "output_columns 数组中有非数字的字符串 '" + s + "'，格式错误";
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
            Program.MainForm.AppInfo.LinkFormState(dlg, "SelectColumnDialog_state");
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
                return Program.MainForm.AppInfo.GetString("inventory_form", "output_columns", "<all>");
            }
            set
            {
                Program.MainForm.AppInfo.SetString("inventory_form", "output_columns", value);
            }
        }

        private void button_operLog_setDateRange_Click(object sender, EventArgs e)
        {
            GetOperLogFilenameDlg dlg = new GetOperLogFilenameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Text = "请指定日志起止日期范围";
            dlg.DateRange = this.textBox_operLog_dateRange.Text;
            Program.MainForm.AppInfo.LinkFormState(dlg, "GetOperLogFilenameDlg_state");
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_operLog_dateRange.Text = dlg.DateRange;
        }

        private void button_operLog_load_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_operLog_dateRange.Text) == true)
            {
                strError = "尚未指定日志日期范围";
                goto ERROR1;
            }

            this._statisInfo = null;
            this.SetButtonState(null);

            ClearHtml();

            string strStart = "";
            string strEnd = "";
            StringUtil.ParseTwoPart(this.textBox_operLog_dateRange.Text, "-", out strStart, out strEnd);

            string strWarning = "";
            List<string> dates = null;
            int nRet = OperLogLoader.MakeLogFileNames(strStart,
                strEnd,
                false,  // 是否包含扩展名 ".log"
                out dates,
                out strWarning,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = LoadOperLogs(dates,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            FillOperLogHtml();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        class OperLogData
        {
            // 操作时间
            public string OperTime = "";
            // 操作者
            public string Operator = "";
            // 
            public string Action = "";

            public string ItemRecPath = "";

            public string ItemBarcode = "";

            public int OperCount = 0;   // 选定的阶段内总共操作过多少次
        }

        // 存储日志记录。会根据 册记录路径来归并，和同一册记录相关的仅仅保留一个 OperLogData 对象
        List<OperLogData> _operLogItems = new List<OperLogData>();
        // 册记录路径 --> OperLogData
        Hashtable _operLogTable = new Hashtable();

        int LoadOperLogs(List<string> dates,
            out string strError)
        {
            strError = "";

            _operLogItems = new List<OperLogData>();
            _operLogTable = new Hashtable();

            EnableControls(false);

            LibraryChannel channel = this.GetChannel();

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装载日志记录 ...");
            stop.BeginLoop();
            try
            {
                ProgressEstimate estimate = new ProgressEstimate();

                OperLogLoader loader = new OperLogLoader();
                loader.Channel = channel;
                loader.Stop = this.Progress;
                loader.Estimate = estimate;
                loader.Dates = dates;
                loader.Level = 2;  // Program.MainForm.OperLogLevel;
                loader.AutoCache = false;
                loader.CacheDir = "";
                loader.Filter = "borrow,return";    // 2017/4/9

                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                foreach (OperLogItem item in loader)
                {
                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return 0;
                    }

                    if (stop != null)
                        stop.SetMessage("正在获取 " + item.Date + " " + item.Index.ToString() + " " + estimate.Text + "...");

                    if (string.IsNullOrEmpty(item.Xml) == true)
                        continue;


                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(item.Xml);
                    }
                    catch (Exception ex)
                    {
                        strError = "日志记录 " + item.Date + " " + item.Index.ToString() + " XML 装入 DOM 的时候发生错误: " + ex.Message;
                        DialogResult result = MessageBox.Show(this,
    strError + "\r\n\r\n是否跳过此条记录继续处理?",
    "ReportForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.No)
                            return -1;

                        // 记入日志，继续处理
                        // this.GetErrorInfoForm().WriteHtml(strError + "\r\n");
                        continue;
                    }

                    string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
                    if (strOperation != "borrow" && strOperation != "return")
                        continue;
                    string strAction = DomUtil.GetElementText(dom.DocumentElement,
        "action");
                    string strOperator = DomUtil.GetElementText(dom.DocumentElement,
        "operator");
                    string strOperTime = DomUtil.GetElementText(dom.DocumentElement,
        "operTime");
                    string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
"itemBarcode");
                    string strItemRecPath = "";
                    XmlNode node = dom.DocumentElement.SelectSingleNode("itemRecord/@recPath");
                    if (node == null)
                    {
                        strError = "缺乏 itemRecord 元素的 recPath 属性";
                        continue;
                    }
                    else
                        strItemRecPath = node.Value;

                    OperLogData data = (OperLogData)_operLogTable[strItemRecPath];
                    if (data == null)
                    {
                        data = new OperLogData();
                        data.ItemRecPath = strItemRecPath;
                        _operLogItems.Add(data);
                        _operLogTable[strItemRecPath] = data;
                    }

                    data.ItemBarcode = strItemBarcode;
                    data.Action = strAction;
                    data.Operator = strOperator;
                    data.OperTime = SQLiteUtil.GetLocalTime(strOperTime);
                    data.OperCount++;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "获取日志记录的过程中出现异常: " + ex.Message;
                return -1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.Style = StopStyle.None;

                this.ReturnChannel(channel);

                EnableControls(true);
            }

        }

        void FillOperLogHtml()
        {
            ClearHtml();

            AppendHtml("<table>");
            AppendHtml("<tr>");
            AppendHtml("<td>册记录路径</td>");
            AppendHtml("<td>操作类型</td>");
            AppendHtml("<td>册条码号</td>");
            AppendHtml("<td>操作者</td>");
            AppendHtml("<td>操作时间</td>");
            AppendHtml("<td>操作次数</td>");
            AppendHtml("</tr>");

            foreach (OperLogData data in _operLogItems)
            {
                AppendHtml("<tr>");
                AppendHtml("<td>" + data.ItemRecPath + "</td>");
                AppendHtml("<td>" + data.Action + "</td>");
                AppendHtml("<td>" + data.ItemBarcode + "</td>");
                AppendHtml("<td>" + data.Operator + "</td>");
                AppendHtml("<td>" + data.OperTime + "</td>");
                AppendHtml("<td>" + data.OperCount.ToString() + "</td>");
                AppendHtml("</tr>");

            }
            AppendHtml("</table>");
        }

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(Program.MainForm.DataDir, "default\\inventory.css");   // ?? 还是用的 default 目录的文件啊，没有用用户目录的同名文件。似乎启动时不用拷贝了
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";

            {
                HtmlDocument doc = webBrowser1.Document;

                if (doc == null)
                {
                    webBrowser1.Navigate("about:blank");
                    doc = webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            Global.WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }


        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendHtml), strText);
                return;
            }

            Global.WriteHtml(this.webBrowser1,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
                this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
            if (e.Actions == "yes,no,cancel")
            {
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
            }
        }

        // 根据盘点结果调整册状态
        private void button_statis_maskItems_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;


            // *** 将本次盘点验证存在的册，去除 注销/丢失 状态

            // return:
            //      -1  出错
            //      0   放弃操作
            //      1   操作成功
            nRet = ModifyState("remove", out strError);
            if (nRet == -1)
                goto ERROR1;

            // *** 将本地盘点没有验证的(并且状态中没有“注销”或“丢失”的)册，加上 注销 状态
            // return:
            //      -1  出错
            //      0   放弃操作
            //      1   操作成功
            nRet = ModifyState("add", out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 修改册记录的状态
        // 将本次盘点验证存在的册，去除 注销/丢失 状态；或将没有盘点到的册，打上 注销 标记
        // parameters:
        //      strAction   动作。remove/add 之一
        // return:
        //      -1  出错
        //      0   放弃操作
        //      1   操作成功
        int ModifyState(
            string strAction,
            out string strError)
        {
            int nRet = 0;

            if (strAction != "remove" && strAction != "add")
            {
                strError = "未知的 strAction 值 '" + strAction + "'";
                return -1;
            }

            // TODO: 这几个栏目，作为必备栏目，应该在窗口打开的早期进行验证是否具备，如果不具备要及时警告
            // 在参考手册中也要说明哪些栏目是必备的，必须配置在 inventory_item_borrow.xml 配置文件中
            int nStateColumnIndex = this._defs._base_colmun_defs.FindColumnByType("item_state");
            if (nStateColumnIndex == -1)
            {
                strError = "基准集列表中 type 为 item_state 的列没有定义 ...";
                return -1;
            }
            int nBorrowerColumnIndex = this._defs._base_colmun_defs.FindColumnByType("borrower");
            if (nBorrowerColumnIndex == -1)
            {
                strError = "基准集列表中 type 为 borrower 的列没有定义 ...";
                return -1;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_baseList_records.Items)
            {
                if (item.Tag == null)
                    continue;

                string strState = ListViewUtil.GetItemText(item, nStateColumnIndex + 2);
                if (string.IsNullOrEmpty(strState) == false
                    && (strState[0] == '+' || strState[0] == '-'))
                    strState = strState.Substring(1);

                string strBorrower = ListViewUtil.GetItemText(item, nBorrowerColumnIndex + 2);

                LineType type = (LineType)item.Tag;
                if (strAction == "remove" && type == LineType.Verified)
                {

                    if ((StringUtil.IsInList("丢失", strState) == true
                        || StringUtil.IsInList("注销", strState) == true)
                        && string.IsNullOrEmpty(strBorrower) == true)
                    {
                        items.Add(item);
                    }
                }

                if (strAction == "add" && type == LineType.Lost)
                {
                    if (StringUtil.IsInList("丢失", strState) == false
    && StringUtil.IsInList("注销", strState) == false
    && string.IsNullOrEmpty(strBorrower) == true)
                    {
                        items.Add(item);
                    }
                }
            }

            if (items.Count == 0)
            {
                strError = "没有需要操作的事项";
                return 1;
            }

            {
                string strText = "";
                if (strAction == "remove")
                    strText = "盘点验证过的册中，有 " + items.Count.ToString() + " 册当前是 “丢失” 或 “注销” 状态。\r\n\r\n是否要去掉这些册的“丢失”或“注销”状态?";
                if (strAction == "add")
                    strText = "经盘点推断新丢失 " + items.Count.ToString() + " 册。\r\n\r\n是否要给这些册加上“注销”状态?";

                DialogResult result = MessageBox.Show(this,
strText,
"InventoryForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    strError = "放弃操作";
                    return 0;
                }
            }

            EnableControls(false);

            LibraryChannel channel = this.GetChannel();

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在修改册记录状态 ...");
            stop.BeginLoop();
            try
            {
                stop.SetProgressRange(0, items.Count);

                ListViewPatronLoader loader = new ListViewPatronLoader(channel,
    stop,
    items,
    this.m_biblioTable);
                loader.DbTypeCaption = "实体库";

                List<ListViewItem> changed_items = new List<ListViewItem>();
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

                    stop.SetProgressValue(i);

                    BiblioInfo info = item.BiblioInfo;

                    Debug.Assert(item.ListViewItem == items[i], "");

                    string strXml = info.NewXml;
                    if (string.IsNullOrEmpty(strXml) == true)
                        strXml = info.OldXml;

                    XmlDocument item_dom = new XmlDocument();
                    try
                    {
                        item_dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "册记录 '" + info.OldXml + "' XML 装入 XMLDOM 时出错: " + ex.Message;
                        return -1;
                    }

                    string strState = DomUtil.GetElementText(item_dom.DocumentElement, "state");

                    if (strAction == "remove")
                    {
                        bool bChanged = RemoveStyle(ref strState, "丢失,注销");
                        if (bChanged == false)
                            goto CONTINUE;

                        DomUtil.SetElementText(item_dom.DocumentElement, "state", strState);
                    }
                    if (strAction == "add")
                    {
                        StringUtil.SetInList(ref strState, "注销", true);
                        DomUtil.SetElementText(item_dom.DocumentElement, "state", strState);
                    }

                    string strComment = DomUtil.GetElementText(item_dom.DocumentElement, "comment");
                    if (strAction == "remove")
                    {
                        strComment = AppendComment(strComment,
                            DateTime.Now.ToString() + " 去除丢失或注销状态。");
                    }
                    if (strAction == "add")
                    {
                        strComment = AppendComment(strComment,
                            DateTime.Now.ToString() + " 添加注销状态。");
                    }
                    DomUtil.SetElementText(item_dom.DocumentElement, "comment", strComment);

                    info.NewXml = item_dom.DocumentElement.OuterXml;

                    byte[] baNewTimestamp = null;
                    // 保存一条记录
                    // 保存成功后， info.Timestamp 会被更新
                    // return:
                    //      -2  时间戳不匹配
                    //      -1  出错
                    //      0   成功
                    nRet = SaveItemRecord(
                        channel,
                        info.RecPath,
                    info,
                    out baNewTimestamp,
                    out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == -2)
                    {
                        // TODO: 时间戳不匹配。警告重做?
                        return -1;
                    }

                    info.Timestamp = baNewTimestamp;

                    this.m_nChangedCount++;
                    AcceptOneChange(item.ListViewItem);

                    // 刷新显示
                    changed_items.Add(item.ListViewItem);

                CONTINUE:
                    i++;
                }

                {
                    // TODO: 要保护被刷新行原先的背景颜色

                    string strBrowseStyle = "id,cols,format:@coldef:" + this._defs.BrowseColumnDef;

                    // 刷新浏览行
                    nRet = RefreshListViewLines(
                        channel,
                        changed_items,
                        strBrowseStyle,
                        false,
                        false,  // 不清除右侧多出来的列内容
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.Style = StopStyle.None;

                this.ReturnChannel(channel);

                EnableControls(true);
            }

            if (strAction == "add")
                this._statisInfo.ItemsNeedAddLostState = 0;
            else if (strAction == "remove")
                this._statisInfo.ItemsNeedRemoveLostState = 0;
            this.SetButtonState(this._statisInfo);

            strError = "";
            return 1;
        }

        static string AppendComment(string strComment, string strNewContent)
        {
            if (strComment == null)
                strComment = "";
            if (string.IsNullOrEmpty(strComment) == false)
                strComment += "; ";
            strComment += strNewContent;
            return strComment;
        }

        // 从 strText 中移走 strList 中的每个列举值
        // return:
        //      true 表示 strText 发生了修改；false 表示strText 没有发生修改
        public static bool RemoveStyle(ref string strText, string strList)
        {
            bool bChanged = false;
            List<string> list = StringUtil.SplitList(strList);
            foreach (string value in list)
            {
                if (StringUtil.RemoveFromInList(value, true, ref strText) == true)
                    bChanged = true;
            }

            return bChanged;
        }

        // 保存一条记录
        // 保存成功后， info.Timestamp 会被更新
        // return:
        //      -2  时间戳不匹配
        //      -1  出错
        //      0   成功
        int SaveItemRecord(
            LibraryChannel channel,
            string strRecPath,
            BiblioInfo info,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";
            baNewTimestamp = null;

            List<EntityInfo> entityArray = new List<EntityInfo>();

            {
                EntityInfo item_info = new EntityInfo();

                item_info.OldRecPath = strRecPath;
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

            lRet = channel.SetEntities(
                 this.stop,   // this.BiblioStatisForm.stop,
                 "",
                 entities,
                 out errorinfos,
                 out strError);

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

            info.Timestamp = baNewTimestamp;

            if (String.IsNullOrEmpty(strError) == false)
                return -1;

            return 0;
        }

        private void listView_baseList_records_DoubleClick(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_baseList_records.SelectedItems.Count == 0)
            {
                strError = "尚未在基准列表中选定要操作的事项";
                goto ERROR1;
            }

            string strFirstColumn = ListViewUtil.GetItemText(this.listView_baseList_records.SelectedItems[0], 0);

            if (String.IsNullOrEmpty(strFirstColumn) == true)
            {
                strError = "第一列没有内容";
                goto ERROR1;
            }

            {
#if NO
                string strOpenStyle = "new";
                if (this.LoadToExistWindow == true)
                    strOpenStyle = "exist";
#endif
                string strOpenStyle = "exist";

                // bool bLoadToItemWindow = this.LoadToItemWindow;
                bool bLoadToItemWindow = false;

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
            return;
        ERROR1:
            MessageBox.Show(this, strError);
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

            if (this.listView_baseList_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要装入" + strTargetFormName + "的行");
                return;
            }

            string strBarcodeOrRecPath = "";

            {
                Debug.Assert(strIdType == "recpath", "");
                // recpath
                strBarcodeOrRecPath = ListViewUtil.GetItemText(this.listView_baseList_records.SelectedItems[0], 0);
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

                    // parameters:
                    //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    form.LoadItemByRecPath(strBarcodeOrRecPath, false);
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

                form.DbType = "item";

                if (strIdType == "barcode")
                {
                    Debug.Assert(this.DbType == "item" || this.DbType == "arrive", "");
                    form.LoadRecord(strBarcodeOrRecPath);
                }
                else
                {
                    Debug.Assert(strIdType == "recpath", "");

                    form.LoadRecordByRecPath(strBarcodeOrRecPath, "");
                }
            }
        }

        private void button_start_setLocations_Click(object sender, EventArgs e)
        {
            LibraryChannel channel = this.GetChannel();

            try
            {
                SelectBatchNoDialog dlg = new SelectBatchNoDialog();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.Channel = channel;
                dlg.Stop = this.stop;
                dlg.InventoryDbName = "";
                dlg.LibraryCodeList = GetOwnerLibraryCodes();
                Program.MainForm.AppInfo.LinkFormState(dlg, "SelectBatchNoDialog_location_state");
                dlg.ShowDialog(this);

                this.textBox_start_locations.Text = StringUtil.MakePathList(dlg.SelectedBatchNo, "\r\n");
                return;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        private void inventoryBatchNoControl_start_batchNo_TextChanged(object sender, EventArgs e)
        {
            if (this._chargingForm != null)
                this._chargingForm.BatchNo = this.inventoryBatchNoControl_start_batchNo.Text;
        }

        private void textBox_start_locations_TextChanged(object sender, EventArgs e)
        {
            if (this._chargingForm != null)
            {
                List<string> list = StringUtil.SplitList(this.textBox_start_locations.Text.Replace("\r\n", "\n"), '\n');
                for (int i = 0; i < list.Count; i++)
                {
                    string value = list[i];
                    if (value == "[空]" || value == "[blank]")
                        list[i] = "";
                }
                this._chargingForm.FilterLocations = list;
            }
        }

        private void button_statis_return_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            int nBorrowerColumnIndex = this._defs._base_colmun_defs.FindColumnByType("borrower");
            if (nBorrowerColumnIndex == -1)
            {
                strError = "基准集列表中 type 为 borrower 的列没有定义 ...";
                goto ERROR1;
            }

            int nBarcodeColumnIndex = this._defs._base_colmun_defs.FindColumnByType("item_barcode");
            if (nBarcodeColumnIndex == -1)
            {
                strError = "基准集列表中 type 为 item_barcode 的列没有定义 ...";
                goto ERROR1;
            }

            int nRefIDColumnIndex = this._defs._base_colmun_defs.FindColumnByType("item_refid");
            if (nRefIDColumnIndex == -1)
            {
                strError = "基准集列表中 type 为 item_refid 的列没有定义 ...";
                goto ERROR1;
            }

            // 先初步得到符合条件的行。有可能经过上次处理后，浏览行还没有来得及刷新，因此要预先刷新一次
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_baseList_records.Items)
            {
                if (item.Tag == null)
                    continue;

                LineType type = (LineType)item.Tag;
                if (type == LineType.Verified)
                {
                    string strBorrower = ListViewUtil.GetItemText(item, nBorrowerColumnIndex + 2);

                    if (string.IsNullOrEmpty(strBorrower) == false)
                        items.Add(item);
                }
            }

            if (items.Count == 0)
            {
                strError = "当前没有需要操作的事项";
                goto ERROR1;
            }

            {
                string strBrowseStyle = "id,cols,format:@coldef:" + this._defs.BrowseColumnDef;

                // 刷新浏览行
                nRet = RefreshListViewLines(
                    null,
                    items,
                    strBrowseStyle,
                    true,   // bBeginLoop
                    false,  // 不清除右侧多出来的列内容
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 刷新后正式处理一次
            // *** 将本次盘点验证存在的册，其中处于在借状态的，追加一次还书操作。最好注释说明这是因为盘点发现在架而特意补充还书操作的。有个问题就是这样的册如果发生超期，是不是就豁免读者违约金了？因为实际上架时间已经不可考了
            items.Clear();
            List<string> barcode_list = new List<string>();
            foreach (ListViewItem item in this.listView_baseList_records.Items)
            {
                if (item.Tag == null)
                    continue;

                LineType type = (LineType)item.Tag;
                if (type == LineType.Verified)
                {
                    string strBorrower = ListViewUtil.GetItemText(item, nBorrowerColumnIndex + 2);

                    if (string.IsNullOrEmpty(strBorrower) == false)
                    {
                        string strBarcode = ListViewUtil.GetItemText(item, nBarcodeColumnIndex + 2);
                        string strRefID = ListViewUtil.GetItemText(item, nRefIDColumnIndex + 2);
                        if (string.IsNullOrEmpty(strBarcode) == false)
                            barcode_list.Add(strBarcode);
                        else
                            barcode_list.Add("@refID:" + strRefID);

                        // items.Add(item);
                    }

                }
            }

            this._statisInfo.ItemsNeedReturn = barcode_list.Count;
            this.SetButtonState(this._statisInfo);

            if (barcode_list.Count == 0)
            {
                strError = "当前没有需要操作的事项";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
"当前有 " + barcode_list.Count + " 条经过盘点验证在架的册记录处于外借状态。\r\n\r\n是否要对这些册立即补做还书操作?",
"InventoryForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.No)
            {
                strError = "放弃操作";
                goto ERROR1;
            }

            this.tabControl_main.SelectedTab = this.tabPage_scan;
            this._chargingForm.ClearTaskList(null);
            nRet = this._chargingForm.DoReturn(barcode_list, out strError);
            if (nRet == -1)
                goto ERROR1;

            // TODO: 有办法等待完成后，再刷新相关行么?
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 重载函数和基类函数的区别是，重载函数不会改变 item 的颜色
        // 清除一个事项的修改信息
        // parameters:
        //      bClearBiblioInfo    是否顺便清除事项的 BiblioInfo 信息
        public override void ClearOneChange(ListViewItem item,
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

                //item.BackColor = SystemColors.Window;
                //item.ForeColor = SystemColors.WindowText;

                this.m_nChangedCount--;
                Debug.Assert(this.m_nChangedCount >= 0, "");
            }

            if (bClearBiblioInfo == true)
                this.m_biblioTable.Remove(strRecPath);
        }

        // 统计运算的结果
        class StatisInfo
        {
            // 需要移走 丢失/注销 状态的已被盘点验证存在的册数量
            public long ItemsNeedRemoveLostState = 0;
            // 需要增加 注销 状态的新发现的丢失册数量
            public long ItemsNeedAddLostState = 0;

            // 需要追加还书操作的，尚处于外借状态的册数量。这些册是经过盘点确认已经存在于书架的
            public long ItemsNeedReturn = 0;

            // 盘点验证过的总册数
            public long ItemsVerified = 0;
            // 基准集总册数
            public long ItemsBase = 0;
            // 盘点验证的册中超过基准集的部分册数
            public long ItemsOutofRange = 0;
            // 从基准集中排除已验证的册和外借状态的册以后的册数。这里面包括以前已经为丢失和注销状态的册
            public long ItemsLost = 0;
            // 基准集中处于外借状态的册数。中间包含已经验证的、但处于外借状态的册数
            public long ItemsBorrowed = 0;
        }

        private void listView_inventoryList_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAllInventoryLines_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("移除事项 [" + this.listView_inventoryList_records.SelectedItems.Count.ToString() + "] (&R)");
            menuItem.Click += new System.EventHandler(this.menu_removeSelectedInventoryItems_Click);
            if (this.listView_inventoryList_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("从盘点库中删除所选择事项 [" + this.listView_inventoryList_records.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteSelectedInventoryItems_Click);
            if (this.listView_inventoryList_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_inventoryList_records, new Point(e.X, e.Y));
        }

        void menu_selectAllInventoryLines_Click(object sender, EventArgs e)
        {
            this.listView_inventoryList_records.SelectedIndexChanged -= new System.EventHandler(this.listView_inventoryList_SelectedIndexChanged);
            ListViewUtil.SelectAllLines(this.listView_inventoryList_records);
            this.listView_inventoryList_records.SelectedIndexChanged += new System.EventHandler(this.listView_inventoryList_SelectedIndexChanged);
        }

        void menu_removeSelectedInventoryItems_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
"确实要移除所选定的 " + this.listView_inventoryList_records.SelectedItems.Count.ToString() + " 个记录? (移除只是从窗口内移走，不是从数据库中删除)",
"InventoryForm",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewUtil.DeleteSelectedItems(this.listView_inventoryList_records);
        }

        void menu_deleteSelectedInventoryItems_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
"确实要从盘点库中删除所选定的 " + this.listView_inventoryList_records.SelectedItems.Count.ToString() + " 个记录?",
"InventoryForm",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_inventoryList_records.SelectedItems)
            {
                items.Add(item);
            }

            string strError = "";

            LibraryChannel channel = this.GetChannel();

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除盘点记录 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            this.listView_inventoryList_records.Enabled = false;
            try
            {
                stop.SetProgressRange(0, items.Count);

                ListViewPatronLoader loader = new ListViewPatronLoader(channel,
    stop,
    items,
    this.m_biblioTable);
                loader.DbTypeCaption = "盘点";

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

#if NO
                    entity.RefID = "";

                    if (String.IsNullOrEmpty(entity.RefID) == true)
                        entity.RefID = BookItem.GenRefID();
#endif

                    stop.SetMessage("正在删除盘点记录 " + info.RecPath);

                    long lRet = 0;

                    string strOutputResPath = "";
                    byte[] output_timestamp = null;
                    lRet = channel.WriteRes(
                         stop,
                         info.RecPath,
                         "",
                         0,
                         null,
                         "",
                         "delete",
                         info.Timestamp,
                         out strOutputResPath,
                         out output_timestamp,
                         out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    stop.SetProgressValue(i);

                    this.listView_inventoryList_records.Items.Remove(item.ListViewItem);
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
                this.listView_inventoryList_records.Enabled = true;

                this.ReturnChannel(channel);
            }

            MessageBox.Show(this, "成功删除盘点记录 " + items.Count + " 条");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_start_restoreCfgs_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<string> filenames = new List<string>();
            filenames.Add("inventory_item_browse.xml");
            filenames.Add("inventory.css");

            int nRet = Program.MainForm.CopyDefaultCfgFiles(filenames,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            this.ShowMessage("操作完成", "green", true);
            return;
        ERROR1:
            this.ShowMessage(strError, "red", true);
        }

        private void listView_baseList_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAllBaseListLines_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入实体查询窗 [" + this.listView_baseList_records.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_loadSelectedBaseListItemsToItemSearchForm_Click);
            if (this.listView_baseList_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新 [" + this.listView_baseList_records.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_refreshSelectedBaseListItems_Click);
            if (this.listView_baseList_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_baseList_records, new Point(e.X, e.Y));

        }

        void menu_selectAllBaseListLines_Click(object sender, EventArgs e)
        {
            this.listView_baseList_records.SelectedIndexChanged -= new System.EventHandler(this.listView_baseList_records_SelectedIndexChanged);
            ListViewUtil.SelectAllLines(this.listView_baseList_records);
            this.listView_baseList_records.SelectedIndexChanged += new System.EventHandler(this.listView_baseList_records_SelectedIndexChanged);
        }

        void menu_loadSelectedBaseListItemsToItemSearchForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_baseList_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要装入实体查询窗的事项";
                goto ERROR1;
            }

            string strTempFileName = Program.MainForm.GetTempFileName("baselist");
            try
            {
                using (StreamWriter sw = new StreamWriter(strTempFileName, false, Encoding.UTF8))
                {
                    foreach (ListViewItem item in this.listView_baseList_records.SelectedItems)
                    {
                        sw.WriteLine(item.Text);
                    }
                }
                ItemSearchForm form = Program.MainForm.OpenItemSearchForm("item");
                // form.Activate();
                int nRet = form.ImportFromRecPathFile(strTempFileName,
                    "clear",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                File.Delete(strTempFileName);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_refreshSelectedBaseListItems_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_baseList_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要刷新的事项";
                goto ERROR1;
            }
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_baseList_records.SelectedItems)
            {
                items.Add(item);
            }
            string strBrowseStyle = "id,cols,format:@coldef:" + this._defs.BrowseColumnDef;

            // 刷新浏览行
            int nRet = RefreshListViewLines(
                null,
                items,
                strBrowseStyle,
                true,
                false,  // 不清除右侧多出来的列内容
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
