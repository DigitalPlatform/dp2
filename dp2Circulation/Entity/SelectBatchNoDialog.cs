using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 盘点窗中用于选择批次号的对话框
    /// </summary>
    public partial class SelectBatchNoDialog : Form
    {
        public SelectBatchNoDialog()
        {
            InitializeComponent();
        }

        List<string> _libraryCodeList = new List<string>();

        public List<string> LibraryCodeList
        {
            get
            {
                return this._libraryCodeList;
            }
            set
            {
                this._libraryCodeList = value;
            }
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        private void SelectBatchNoDialog_Load(object sender, EventArgs e)
        {
            this.SetTitle();
            // this.BeginInvoke(new Action(FillList));
            var token = this._cancel.Token;
            _ = Task.Factory.StartNew(() =>
            {
                // 显示“请等待”
                this.TryInvoke(() =>
                {
                    this.listView_records.Enabled = false;
                    this.listView_records.Items.Add(new ListViewItem("正在获取列表 ..."));
                });

                FillList(token);

                this.TryInvoke(() =>
                {
                    this.listView_records.Enabled = true;
                });
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择任何事项");
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        /*
        // 引用。调用本对话框的调主提供
        public LibraryChannel Channel
        {
            get;
            set;
        }

        // 引用。调用本对话框的调主提供
        public Stop Stop
        {
            get;
            set;
        }
        */

        public string InventoryDbName
        {
            get;
            set;
        }

        void SetTitle()
        {
            if (string.IsNullOrEmpty(this.InventoryDbName) == false)
            {
                if (string.IsNullOrEmpty(this.Text) == true)
                    this.Text = "选择批次号";
                this.listView_records.Columns[0].Text = "批次号";
            }
            else
            {
                if (string.IsNullOrEmpty(this.Text) == true)
                    this.Text = "选择馆藏地点";
                this.listView_records.Columns[0].Text = "馆藏地点";
            }
        }

        void FillList(CancellationToken token)
        {
            /*
            if (this.Channel == null
                || this.Stop == null)
                return;
            */

            // testing
            // Thread.Sleep(5000);

            string strError = "";
            int nRet = 0;
            var looping = Program.MainForm.Looping(
                out LibraryChannel channel,
                "",
                "settimeout:0:5:0");
            token.Register(() =>
            {
                looping?.Progress?.DoStop();
            });
            try
            {
                if (string.IsNullOrEmpty(this.InventoryDbName) == false)
                    nRet = SearchAllBatchNo(
                        channel,
                        looping.Progress,
                        this.InventoryDbName,
                        out strError);
                else
                    nRet = SearchAllLocation(
                        channel,
                        looping.Progress,
                        out strError);
            }
            finally
            {
                looping.Dispose();
            }

            if (nRet == -1)
                MessageBox.Show(this, strError);

            if (this._selectedBatchNo != null)
            {
                SelectItems(this._selectedBatchNo);
                this._selectedBatchNo = null;
            }
        }

        // 检索出实体库全部可用的馆藏地名称
        int SearchAllLocation(
    LibraryChannel channel,
    Stop stop,
    out string strError)
        {
            strError = "";

            long lTotalCount = 0;

            this.listView_records.Items.Clear();

            /*
            // EnableControls(false);
            stop.OnStop += new StopEventHandler(channel.DoStop);
            stop.Initial("正在列出全部馆藏地 ...");
            stop.BeginLoop();
            */
            stop?.SetMessage("正在列出全部馆藏地 ...");
            try
            {
                for (int i = 0; i < 1/*2*/; i++)
                {
                    // 注: 两次检索的时间都可能较长，要提供中断的机会
                    long lRet = channel.SearchItem(
        stop,
        "<all>",
        "", // strBatchNo
        -1,
        "馆藏地点",
        i == 0 ? "left" : "exact",  // 第二次为检索空值
        "zh",
        "batchno",   // strResultSetName
        "", // "desc",
        "keycount", // strOutputStyle
        out strError);
                    if (lRet == 0)
                    {
#if NO
                        strError = "not found";
                        return 0;   // not found
#endif 
                        continue;
                    }
                    if (lRet == -1)
                        return -1;

                    long lHitCount = lRet;

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

                        lRet = channel.GetSearchResult(
                            stop,
                            "batchno",   // strResultSetName
                            lStart,
                            lCount,
                            "keycount",
                            "zh",
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "GetSearchResult() error: " + strError;
                            return -1;
                        }

                        if (lRet == 0)
                        {
                            // MessageBox.Show(this, "未命中");
                            continue;
                        }

                        // 处理浏览结果
                        foreach (Record record in searchresults)
                        {
                            if (record.Cols == null)
                            {
                                strError = "请更新应用服务器和数据库内核到最新版本，才能使用列出馆藏地的功能";
                                return -1;
                            }

                            if (this._libraryCodeList.Count > 0
                                && MatchLibraryCode(this._libraryCodeList, record.Path) == false)
                                continue;

                            // 跳过数字为 0 的事项
                            if (record.Cols.Length > 0 && record.Cols[0] == "0")
                                continue;

                            ListViewItem item = new ListViewItem();
                            item.Text = string.IsNullOrEmpty(record.Path) == false ? record.Path : "[空]";
                            ListViewUtil.ChangeItemText(item, 1, record.Cols[0]);

                            this.listView_records.Items.Add(item);
                        }

                        lStart += searchresults.Length;
                        lCount -= searchresults.Length;

                        stop?.SetMessage("共命中 " + (lTotalCount + lHitCount).ToString() + " 条，已装入 " + (lTotalCount + lStart).ToString() + " 条");

                        if (lStart >= lHitCount || lCount <= 0)
                            break;
                    }

                    lTotalCount += lHitCount;
                }

                if (lTotalCount == 0)
                {
                    strError = "not found";
                    return 0;
                }
            }
            finally
            {
                stop?.SetMessage("");
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(channel.DoStop);
                stop.Initial("");

                // EnableControls(true);
                */
            }
            return 1;
        }

        static bool MatchLibraryCode(string strLibraryCode, string strLocation)
        {
            if (Global.IsGlobalUser(strLibraryCode) == true)
                return true;
            string strCurrentLibraryCode = Global.GetLibraryCode(strLocation);
            if (strLibraryCode == strCurrentLibraryCode)
                return true;
            return false;
        }

        static bool MatchLibraryCode(List<string> librarycodes, string strLocation)
        {
            foreach (string librarycode in librarycodes)
            {
                if (MatchLibraryCode(librarycode, strLocation) == true)
                    return true;
            }
            return false;
        }

        // 检索出盘点库内全部批次号名称
        int SearchAllBatchNo(
            LibraryChannel channel,
            Stop stop,
            string strInventoryDbName,
            out string strError)
        {
            strError = "";

            this.listView_records.Items.Clear();

            /*
            // EnableControls(false);
            stop.OnStop += new StopEventHandler(channel.DoStop);
            stop.Initial("正在列出全部批次号 ...");
            stop.BeginLoop();
            */
            stop?.SetMessage("正在列出全部批次号 ...");
            try
            {
                // 构造检索式
                StringBuilder text = new StringBuilder();

                text.Append("<target list='"
                        + StringUtil.GetXmlStringSimple(strInventoryDbName + ":" + "批次号")
                        + "'>");
                // 当前是否为全局用户
                bool bGlobalUser = this._libraryCodeList.Count == 0 || this._libraryCodeList.IndexOf("") != -1;
                // 全局用户只认列表中 "" 一个值。这样可以检索出全部批次号，包括各个分馆的
                if (bGlobalUser == true && this._libraryCodeList.Count != 1)
                {
                    this._libraryCodeList.Clear();
                    this._libraryCodeList.Add("");
                }
                int i = 0;
                foreach (string librarycode in this.LibraryCodeList)
                {
                    if (i > 0)
                        text.Append("<operator value='OR' />");

                    text.Append("<item><word>"
                        + StringUtil.GetXmlStringSimple(bGlobalUser ? "" : librarycode + "-")
                        + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang>");
                    i++;
                }

                if (bGlobalUser == true)
                {
                    if (i > 0)
                        text.Append("<operator value='OR' />");

                    // 针对空批次号的检索。空只能被全局用户可见
                    text.Append("<item><word>"
            + StringUtil.GetXmlStringSimple("")
            + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang>");
                    i++;
                }

                text.Append("</target>");

#if NO
                // 构造检索式
                string strQueryXml = "<target list='"
                        + StringUtil.GetXmlStringSimple(strInventoryDbName + ":" + "批次号")
                        + "'><item><word>"
                        + StringUtil.GetXmlStringSimple("")
                        + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang>";
                strQueryXml += "<operator value='OR' />";
                strQueryXml += "<item><word>"
        + StringUtil.GetXmlStringSimple("")
        + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";
#endif

                long lRet = channel.Search(
                    stop,
                    text.ToString(),
                    "batchno",
                    "keycount", // strOutputStyle
                    out strError);
                if (lRet == 0)
                {
                    strError = "not found";
                    return 0;   // not found
                }
                if (lRet == -1)
                    return -1;

                long lHitCount = lRet;

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

                    lRet = channel.GetSearchResult(
                        stop,
                        "batchno",   // strResultSetName
                        lStart,
                        lCount,
                        "keycount",
                        "zh",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSearchResult() error: " + strError;
                        return -1;
                    }

                    if (lRet == 0)
                    {
                        // MessageBox.Show(this, "未命中");
                        return 0;
                    }

                    // 处理浏览结果
                    foreach (Record record in searchresults)
                    {
                        if (record.Cols == null)
                        {
                            strError = "请更新应用服务器和数据库内核到最新版本，才能使用列出批次号的功能";
                            return -1;
                        }

                        // 跳过数字为 0 的事项
                        if (record.Cols.Length > 0 && record.Cols[0] == "0")
                            continue;

                        ListViewItem item = new ListViewItem();
                        item.Text = string.IsNullOrEmpty(record.Path) == false ? record.Path : "[空]";
                        ListViewUtil.ChangeItemText(item, 1, record.Cols[0]);

                        this.listView_records.Items.Add(item);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop?.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }
                return 1;
            }
            finally
            {
                stop?.SetMessage("");
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(channel.DoStop);
                stop.Initial("");

                // EnableControls(true);
                */
            }
        }

        List<string> _selectedBatchNo = null;

        public List<string> SelectedBatchNo
        {
            get
            {
                List<string> results = new List<string>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    results.Add(item.Text);
                }
                return results;
            }
            set
            {
                if (this.listView_records.Items.Count == 0)
                {
                    this._selectedBatchNo = value;  // 储存起来等 _load 时用
                    return;
                }
                SelectItems(value);
                _selectedBatchNo = null;
            }
        }

        void SelectItems(List<string> value)
        {
            ListViewUtil.ClearSelection(this.listView_records);
            foreach (string s in value)
            {
                ListViewItem item = ListViewUtil.FindItem(this.listView_records, s, 0);
                if (item != null)
                    item.Selected = true;
            }
        }

        private void toolStripButton_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllLines(this.listView_records);
        }

        private void toolStripButton_unselectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.ClearSelection(this.listView_records);
        }

        SortColumns SortColumns = new SortColumns();

        private void listView_records_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为普通字符串，第二列为数字字符串
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.LeftAlign;
            else if (nClickColumn == 1)
                sortStyle = ColumnSortStyle.RightAlign;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_records.Columns,
                true);

            // 排序
            this.listView_records.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);
            this.listView_records.ListViewItemSorter = null;
        }

        private void SelectBatchNoDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            this._cancel.Cancel();
            this._cancel.Dispose();
            this._cancel = null;
        }
    }
}
