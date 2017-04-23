using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Collections;

using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 选择一个或者多个字典事项的对话框
    /// </summary>
    public partial class SelectDictionaryItemDialog : Form
    {
        /// <summary>
        /// MainForm 对象
        /// </summary>
        // public MainForm MainForm = null;

        /// <summary>
        /// 词典数据库名
        /// </summary>
        public string DbName = "";

        /// <summary>
        /// 起始的 Key
        /// </summary>
        public string Key = "";

        /// <summary>
        /// 当前界面语言代码
        /// </summary>
        public string Lang = "zh";

        /// <summary>
        /// [in] [out] 界面状态字符串。栏目宽度数字列表，逗号间隔的字符串；前方一致的状态；Split 位置
        /// </summary>
        public string UiStates = "";

#if NO
        /// <summary>
        /// [in] 词典记录 XML 字符串集合
        /// </summary>
        public List<string> Xmls = new List<string>();
#endif

        /// <summary>
        /// [out] 选中的 rel 元素的 name 属性，字符串集合
        /// </summary>
        public List<string> ResultRelations = new List<string>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public SelectDictionaryItemDialog()
        {
            InitializeComponent();
        }

        DigitalPlatform.StopManager _stopManager = new DigitalPlatform.StopManager();
        DigitalPlatform.Stop _stop = null;

        private void SelectDictionaryItemDialog_Load(object sender, EventArgs e)
        {
#if NO
            // 第一次检索装载
            {
                this.toolStripLabel_currentKey.Text = this.Key;

                string strError = "";
                List<string> results = new List<string>();
                int nRet = Search(this.DbName,
                    this.Key,
                    "exact",
                    1000,
                    ref results,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                FillListView(results);
            }
#endif
            _stopManager.Initial(this.toolStripButton_stop,
    (object)this.label_message,
    (object)null);

            _stop = new DigitalPlatform.Stop();
            _stop.Register(this._stopManager, true);	// 和容器关联


#if NO
            if (string.IsNullOrEmpty(ColumnWidthList) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_list,
    ColumnWidthList,
    true);
            }
#endif
            this.SetUiState(this.UiStates);

            this.BeginInvoke(new Delegate_FillLevels(this.FillLevels));
        }


        void SetUiState(string strStates)
        {
            Hashtable table = StringUtil.ParseParameters(strStates, ';', '=');

            string strColumnWidthList = (string)table["l_c_w"];
            if (string.IsNullOrEmpty(strColumnWidthList) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_list,
strColumnWidthList,
true);
            }

            strColumnWidthList = (string)table["v_c_w"];
            if (string.IsNullOrEmpty(strColumnWidthList) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_levels,
strColumnWidthList,
true);
            }

            string strSplitter = (string)table["s_c"];
            if (string.IsNullOrEmpty(strSplitter) == false)
            {
                float f = 0.5F;
                float.TryParse(strSplitter, out f);
                GuiUtil.SetSplitterState(this.splitContainer1, f);
            }

            string strLeft = (string)table["l"];
            if (string.IsNullOrEmpty(strLeft) == false)
            {
                if (strLeft == "yes")
                    this.toolStripButton_wild.Checked = true;
                else
                    this.toolStripButton_wild.Checked = false;
            }
        }

        string GetUiState()
        {
            Hashtable table = new Hashtable();

            table["l_c_w"] = ListViewUtil.GetColumnWidthListString(this.listView_list);
            table["v_c_w"] = ListViewUtil.GetColumnWidthListString(this.listView_levels);

            table["s_c"] = GuiUtil.GetSplitterState(this.splitContainer1).ToString();
            table["l"] = this.toolStripButton_wild.Checked == true ? "yes" : "no";

            return StringUtil.BuildParameterString(table,
                    ';',
                    '=');
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listView_list.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "请选定一个事项");
                return;
            }

            this.ResultRelations.Clear();

            foreach (ListViewItem item in this.listView_list.SelectedItems)
            {
                string strRel = ListViewUtil.GetItemText(item, 2);
                this.ResultRelations.Add(strRel);
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        void DoStop(object sender, StopEventArgs e)
        {
#if NO
            if (Program.MainForm.Channel != null)
                Program.MainForm.Channel.Abort();
#endif
        }

        // 检索
        // return:
        //      -1  出错
        //      0   没有找到
        //      >0  命中的条数
        int Search(
            string strDbName,
            string strKey,
            string strMatchStyle,
            int nMax,
            ref List<string> results,
            out string strError)
        {
            strError = "";

            lock (this._stop)
            {
                LibraryChannel channel = Program.MainForm.GetChannel();

                _stop.OnStop += new StopEventHandler(this.DoStop);
                _stop.Initial("正在检索词条 '" + strKey + "' ...");
                _stop.BeginLoop();
                try
                {

                    int nRet = Program.MainForm.SearchDictionary(
                        channel,
                        this._stop,
                        strDbName,
                        strKey,
                        strMatchStyle,
                        nMax,
                        ref results,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        strError = "词条 '" + strKey + "' 在 " + strDbName + "库中没有找到";
                        return 0;
                    }

                    return nRet;
                }
                finally
                {
                    _stop.EndLoop();
                    _stop.OnStop -= new StopEventHandler(this.DoStop);
                    _stop.Initial("");

                    Program.MainForm.ReturnChannel(channel);
                }

            }
        }

        void FillListView(List<string> results)
        {
            string strError = "";
            // int nRet = 0;

            this.listView_list.Items.Clear();

            if (results == null || results.Count == 0)
                return;

            this.listView_list.ListViewItemSorter = new ItemComparer();

            foreach (string line in results)
            {
                string strRecPath = "";
                string strXml = "";
                StringUtil.ParseTwoPart(line, "|", out strRecPath, out strXml);

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "XML 装入 DOM 时出错: " +ex.Message;
                    goto ERROR1;
                }

                XmlNode node = dom.DocumentElement.SelectSingleNode("key");

                string strKey = DomUtil.GetAttr(node, "name");
                string strKeyCaption = DomUtil.GetCaption(this.Lang, node);

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("rel");
                foreach (XmlNode rel_node in nodes)
                {
                    string strRel = DomUtil.GetAttr(rel_node, "name");
                    string strRelCaption = DomUtil.GetCaption(this.Lang, rel_node);
                    string strWeight = DomUtil.GetAttr(rel_node, "weight");

                    ListViewItem item = new ListViewItem();
                    ListViewUtil.ChangeItemText(item, 0, strKey);
                    ListViewUtil.ChangeItemText(item, 1, strKeyCaption);
                    ListViewUtil.ChangeItemText(item, 2, strRel);
                    ListViewUtil.ChangeItemText(item, 3, strWeight);
                    ListViewUtil.ChangeItemText(item, 4, strRelCaption);

                    this.listView_list.Items.Add(item);
                }
            }

            // 按照 weight 排序

            // 自动选中第一项
            if (this.listView_list.Items.Count > 0)
                this.listView_list.Items[0].Selected = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(sender, e);
        }

        private void SelectDictionaryItemDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            // this.ColumnWidthList = ListViewUtil.GetColumnWidthListString(this.listView_list);
            this.UiStates = GetUiState();

            if (_stop != null) // 脱离关联
            {
                _stop.Unregister();	// 和容器关联
                _stop = null;
            }
        }

        private void toolStripButton_upLevel_Click(object sender, EventArgs e)
        {
            if (this.listView_levels.SelectedItems.Count == 0)
            {
                ListViewItem tail = null;

                if (this.listView_levels.Items.Count > 0)
                    tail = this.listView_levels.Items[this.listView_levels.Items.Count - 1];

                if (tail != null)
                    ListViewUtil.SelectLine(tail, true);

                return;
            }

            int index = this.listView_levels.SelectedIndices[0];
            index--;
            if (index >= 0)
                ListViewUtil.SelectLine(this.listView_levels, index, true);
        }

        private void toolStripButton_downLevel_Click(object sender, EventArgs e)
        {
            if (this.listView_levels.SelectedItems.Count == 0)
            {
                if (this.listView_levels.Items.Count > 0)
                    ListViewUtil.SelectLine(this.listView_levels, 0, true);
                return;
            }

            int index = this.listView_levels.SelectedIndices[0];
            index++;
            if (index < this.listView_levels.Items.Count)
                ListViewUtil.SelectLine(this.listView_levels, index, true);
        }

        delegate void Delegate_FillLevels();

        void FillLevels()
        {
            string strError = "";

            this.listView_levels.Items.Clear();
            this.listView_list.Items.Clear();

            string strKey = this.Key;

            for (int i = strKey.Length; i > 0; i--)
            {
                string strText = strKey.Substring(0, i);
                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, 0, strText);

                this.listView_levels.Items.Insert(0, item);

                List<string> results = null;
                int nRet = Search(this.DbName,
                    strText,
                    this.toolStripButton_wild.Checked == true ? "left" : "exact",
                    1000,
                    ref results,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet > 0)
                    ListViewUtil.ChangeItemText(item, 1, nRet.ToString());
            }

            // 自动选中最后一项
            if (this.listView_levels.Items.Count > 0)
                ListViewUtil.SelectLine(this.listView_levels, this.listView_levels.Items.Count - 1, true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_levels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_levels.SelectedItems.Count == 0)
            {
                this.listView_list.Items.Clear();
                return;
            }
            string strKey = this.listView_levels.SelectedItems[0].Text;

            this.toolStripLabel_currentKey.Text = strKey;

            List<string> results = new List<string>();

            string strError = "";
            int nRet = Search(this.DbName,
                this.toolStripLabel_currentKey.Text,
                    this.toolStripButton_wild.Checked == true ? "left" : "exact",
                1000,
                ref results,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            FillListView(results);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_wild_CheckedChanged(object sender, EventArgs e)
        {
            FillLevels();
        }

        private void SelectDictionaryItemDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_stop != null)
            {
                if (_stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }
        }
    }

    class ItemComparer : IComparer
    {
        public ItemComparer()
        {
        }

        public int Compare(object x, object y)
        {
            ListViewItem item1 = (ListViewItem)x;
            ListViewItem item2 = (ListViewItem)y;

            string strKey1 = ListViewUtil.GetItemText(item1, 0);
            string strKey2 = ListViewUtil.GetItemText(item2, 0);

            int nRet = string.Compare(strKey1, strKey2);
            if (nRet != 0)
                return nRet;

            string strWeight1 = ListViewUtil.GetItemText(item1, 3);
            string strWeight2 = ListViewUtil.GetItemText(item2, 3);

            return -1 * StringUtil.RightAlignCompare(strWeight1, strWeight2);
        }
    }
}
