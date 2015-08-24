using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

        private void SelectBatchNoDialog_Load(object sender, EventArgs e)
        {
            this.SetTitle();
            this.BeginInvoke(new Action(FillList));
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public LibraryChannel Channel
        {
            get;
            set;
        }

        public Stop Stop
        {
            get;
            set;
        }

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

        void FillList()
        {
            if (this.Channel == null
                || this.Stop == null)
                return;

            string strError = "";
            int nRet = 0;
            if (string.IsNullOrEmpty(this.InventoryDbName) == false)
                nRet = SearchAllBatchNo(
                     this.Channel,
                     this.Stop,
                     this.InventoryDbName,
                     out strError);
            else
                nRet = SearchAllLocation(
     this.Channel,
     this.Stop,
     out strError);

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

            this.listView_records.Items.Clear();

            // EnableControls(false);
            stop.OnStop += new StopEventHandler(channel.DoStop);
            stop.Initial("正在列出全部馆藏地 ...");
            stop.BeginLoop();

            try
            {
                long lRet = Channel.SearchItem(
    stop,
    "<all>",
    "", // strBatchNo
    -1,
    "馆藏地点",
    "left",
    "zh",
    "batchno",   // strResultSetName
    "", // "desc",
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
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

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
                            strError = "请更新应用服务器和数据库内核到最新版本，才能使用列出馆藏地的功能";
                            return -1;
                        }

                        ListViewItem item = new ListViewItem();
                        item.Text = record.Path;
                        ListViewUtil.ChangeItemText(item, 1, record.Cols[0]);

                        this.listView_records.Items.Add(item);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(channel.DoStop);
                stop.Initial("");

                // EnableControls(true);
            }
            return 1;
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

            // EnableControls(false);
            stop.OnStop += new StopEventHandler(channel.DoStop);
            stop.Initial("正在列出全部批次号 ...");
            stop.BeginLoop();

            try
            {
                // 构造检索式
                string strQueryXml = "<target list='"
                        + StringUtil.GetXmlStringSimple(strInventoryDbName + ":" + "批次号")
                        + "'><item><word>"
                        + StringUtil.GetXmlStringSimple("")
                        + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";
                long lRet = channel.Search(
                    stop,
                    strQueryXml,
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
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

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

                        ListViewItem item = new ListViewItem();
                        item.Text = record.Path;
                        ListViewUtil.ChangeItemText(item, 1, record.Cols[0]);

                        this.listView_records.Items.Add(item);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(channel.DoStop);
                stop.Initial("");

                // EnableControls(true);
            }
            return 1;
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
    }
}
