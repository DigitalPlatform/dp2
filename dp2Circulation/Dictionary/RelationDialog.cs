using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Drawing;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// 通过词典库对照关系，在现有 MARC 记录中创建新字段的对话框
    /// </summary>
    public partial class RelationDialog : Form
    {
        internal FloatingMessageForm _floatingMessage = null;

        public FloatingMessageForm FloatingMessageForm
        {
            get
            {
                return this._floatingMessage;
            }
            set
            {
                this._floatingMessage = value;
            }
        }

        // 修改后的结果 MARC 字符串。本对话框 DialogResult.OK 返回后，这里存储的是修改结果
        public string OutputMARC
        {
            get;
            set;
        }

        public delegate_SearchDictionary ProcSearchDictionary = null;
        public delegate_DoStop ProcDoStop = null;

        RelationCollection _relationCollection = new RelationCollection();
        public RelationCollection RelationCollection
        {
            get
            {
                return this._relationCollection;
            }
            set
            {
                this._relationCollection = value;
            }
        }

        public RelationDialog()
        {
            InitializeComponent();
        }

        DigitalPlatform.StopManager _stopManager = new DigitalPlatform.StopManager();
        DigitalPlatform.Stop _stop = null;

        private void RelationDialog_Load(object sender, EventArgs e)
        {
            _stopManager.Initial(this.toolStripButton_stop,
(object)this.toolStripLabel_message,
(object)null);

            _stop = new DigitalPlatform.Stop();
            _stop.Register(this._stopManager, true);	// 和容器关联

            {
                _floatingMessage = new FloatingMessageForm(this, true);
                // _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }

            this.BeginInvoke(new Action(Begin));
        }

        private void RelationDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void RelationDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_stop != null) // 脱离关联
            {
                _stop.Unregister();	// 和容器关联
                _stop = null;
            }

            CloseFloatingMessage();
        }

        public void CloseFloatingMessage()
        {
            if (_floatingMessage != null)
            {
                _floatingMessage.Close();
                _floatingMessage.Dispose();
                _floatingMessage = null;
            }
        }

        public void ShowMessage(string strMessage,
string strColor = "",
bool bClickClose = false)
        {
            if (this._floatingMessage == null)
                return;

            Color color = Color.FromArgb(80, 80, 80);

            if (strColor == "red")          // 出错
                color = Color.DarkRed;
            else if (strColor == "yellow")  // 成功，提醒
                color = Color.DarkGoldenrod;
            else if (strColor == "green")   // 成功
                color = Color.Green;
            else if (strColor == "progress")    // 处理过程
                color = Color.FromArgb(80, 80, 80);

            this._floatingMessage.SetMessage(strMessage, color, bClickClose);
        }

        public void ClearMessage()
        {
            this._floatingMessage.Text = "";
        }

        public void AppendFloatingMessage(string strText)
        {
            this._floatingMessage.Text += strText;
        }

        public string FloatingMessage
        {
            get
            {
                if (this._floatingMessage == null)
                    return "";
                return this._floatingMessage.Text;
            }
            set
            {
                if (this._floatingMessage != null)
                    this._floatingMessage.Text = value;
            }
        }


        void Begin()
        {
            string strError = "";

            this.ShowMessage("正在检索 ...");
            try
            {

                DisplayMarc(this.RelationCollection.MARC,
                    null);

                // 填充关系列表
                int nRet = FillRelationList(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 针对所有关系，检索出事项，并存储起来备用
                nRet = SearchAll(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 选取第一个关系，填充事项列表
                if (this.flowLayoutPanel_relationList.Controls.Count > 0)
                {
                    RelationControl control = (RelationControl)this.flowLayoutPanel_relationList.Controls[0];
                    SelectControl(control);
                }
            }
            finally
            {
                this.ClearMessage();
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void SelectControl(RelationControl control)
        {
            if (control == this._currentControl)
                return;

            if (this._currentControl != null)
                this._currentControl.Selected = false;
            if (control != null)
                control.Selected = true;
            this._currentControl = control;

            // 填充事项列表
            FillEntryList(control);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 针对 TargetText 内列举值进行查重。警告。

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        void ClearRelationList()
        {
            foreach (Control control in this.flowLayoutPanel_relationList.Controls)
            {
                AddEvents((RelationControl)control, false);
                control.Dispose();
            }
            this.flowLayoutPanel_relationList.Controls.Clear();
        }

        void AddEvents(RelationControl control, bool bAdd)
        {
            if (bAdd)
            {
                control.Click += control_Click;
            }
            else
            {
                control.Click -= control_Click;
            }
        }

        RelationControl _currentControl = null;

        void control_Click(object sender, EventArgs e)
        {
#if NO
            RelationControl control = (RelationControl)sender;
            if (control == _currentControl)
                return;

            _currentControl = control;

            // 填充事项列表
            FillEntryList(control);
#endif
            SelectControl((RelationControl)sender);
        }

        // 填充关系列表
        int FillRelationList(out string strError)
        {
            strError = "";

            ClearRelationList();

            foreach(Relation relation in this._relationCollection)
            {
                foreach (string key in relation.Keys)
                {
                    ControlInfo info = new ControlInfo();
                    info.Relation = relation;

                    RelationControl control = new RelationControl();
                    control.Tag = info;
                    control.TitleText = relation.DbName;
                    if (string.IsNullOrEmpty(relation.Color) == false)
                        control.TitleBackColor = ColorUtil.String2Color(relation.Color);
                    control.SourceText = key;
                    control.BackColor = SystemColors.Window;
                    AddEvents(control, true);

                    this.flowLayoutPanel_relationList.Controls.Add(control);
                }
            }

            return 0;
        }

        // 计算 strLongKey 和 strKey 的匹配级次。级次越高，表示匹配的字符越多。
        static int GetLevel(string strKey, string strLongKey)
        {
            if (string.IsNullOrEmpty(strKey) == true
                || string.IsNullOrEmpty(strLongKey) == true)
                return 0;
            for(int i=1;i<strKey.Length;i++)
            {
                if (strLongKey.StartsWith(strKey.Substring(0, i)) == false)
                    return i - 1;
            }

            return strKey.Length;
        }

        void FillEntryList(RelationControl control)
        {
            string strError = "";

            _disableSelectionChanged++; // 防止 control 的 TargetText 被清掉
            this.dpTable1.Rows.Clear();
            _disableSelectionChanged--;

            ControlInfo info = (ControlInfo)control.Tag;

            List<ResultItem> items = info.ResultItems;
            if (items == null || items.Count == 0)
                return;

            string strControlKey = control.SourceText;

            foreach (ResultItem item in items)
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(item.Xml);
                }
                catch (Exception ex)
                {
                    strError = "XML 装入 DOM 时出错: " + ex.Message;
                    goto ERROR1;
                }

                XmlElement node = dom.DocumentElement.SelectSingleNode("key") as XmlElement;

                string strKey = node.GetAttribute("name");
                // string strKeyCaption = DomUtil.GetCaption(this.Lang, node);

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("rel");
                foreach (XmlElement rel_node in nodes)
                {
                    string strRel = rel_node.GetAttribute("name");
                    // string strRelCaption = DomUtil.GetCaption(this.Lang, rel_node);
                    string strWeight = rel_node.GetAttribute("weight");

                    DpRow row = new DpRow();
                    // key
                    {
                        DpCell cell = new DpCell();
                        cell.Text = strKey;
                        cell.OwnerDraw = true;
                        row.Add(cell);
                    }
                    // rel
                    {
                        DpCell cell = new DpCell();
                        cell.Text = strRel;
                        row.Add(cell);
                    }
                    // weight
                    {
                        DpCell cell = new DpCell();
                        cell.Text = strWeight;
                        row.Add(cell);
                    }
                    // level
                    {
                        DpCell cell = new DpCell();
                        cell.Text = GetLevel(strControlKey, strKey).ToString();
                        row.Add(cell);
                    }

                    this.dpTable1.Rows.Add(row);

                }
            }

            this.dpTable1.Rows.Sort(CompareEntrys);

            // 如果 control 还没有初始化过 hitcounts，则初始化一次
            if (control.HitCounts.Count == 0)
                control.HitCounts = BuildHitCountList(strControlKey);

            // 选定特定行
            if (info.SelectedLines != null && info.SelectedLines.Count > 0)
            {
                foreach (string line in info.SelectedLines)
                {
                    SelectRowByKey(line);
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 根据 key + "|" + rel 字符串选定一个行
        bool SelectRowByKey(string strText)
        {
            string strKey = "";
            string strRel = "";
            StringUtil.ParseTwoPart(strText, "|", out strKey, out strRel);
            foreach(DpRow row in this.dpTable1.Rows)
            {
                if (row[COLUMN_KEY].Text == strKey
                    && row[COLUMN_REL].Text == strRel)
                {
                    row.Selected = true;
                    return true;
                }
            }

            return false;
        }

        List<string> BuildHitCountList(string strSourceKey)
        {
            List<string> list = new List<string>();
            Hashtable table = new Hashtable();  // strKey --> int
            foreach(DpRow row in this.dpTable1.Rows)
            {
                string strLevel = row[COLUMN_LEVEL].Text;
                int value = 0;
                if (table.ContainsKey(strLevel) == true)
                    value = (int)table[strLevel];
                table[strLevel] = value + 1;
            }

            foreach(string key in table.Keys)
            {
                int index = Int32.Parse(key);
                int value = (int)table[key];
                SetValue(list, index-1, value);
            }

            return list;
        }

        static void SetValue(List<string> list, int index, int value)
        {
            Debug.Assert(index >= 0, "");
            while(list.Count <= index)
            {
                list.Add("0");
            }
            list[index] = value.ToString();
        }

        public const int COLUMN_KEY = 0;
        public const int COLUMN_REL = 1;
        public const int COLUMN_WEIGHT = 2;
        public const int COLUMN_LEVEL = 3;

        int CompareEntrys(DpRow x, DpRow y)
        {
            // level
            int level1 = Int32.Parse(x[COLUMN_LEVEL].Text);
            int level2 = Int32.Parse(y[COLUMN_LEVEL].Text);
            int nDelta = (level1 - level2);
            if (nDelta != 0)
                return -1 * nDelta; // 降序

            // key
            nDelta = string.CompareOrdinal(x[COLUMN_KEY].Text,y[COLUMN_KEY].Text);
            if (nDelta != 0)
                return nDelta;  // 升序

            // weight
            int weight1 = Int32.Parse(x[COLUMN_WEIGHT].Text);
            int weight2 = Int32.Parse(y[COLUMN_WEIGHT].Text);
            nDelta = (weight1 - weight2);

            if (nDelta != 0)
                return -1 * nDelta; // 降序

            // rel
            nDelta = string.CompareOrdinal(x[COLUMN_REL].Text, y[COLUMN_REL].Text);
            return nDelta;  // 升序
        }

        // Hashtable _table = new Hashtable(); // RelationControl --> List<ResultItem>

        // 针对所有关系，检索出事项，并存储起来备用
        int SearchAll(out string strError)
        {
            strError = "";

            // this._table.Clear();

            foreach(RelationControl control in this.flowLayoutPanel_relationList.Controls)
            {
                string key = control.SourceText;
                ControlInfo info = (ControlInfo)control.Tag;

                List<ResultItem> results = null;
                // 针对一个 key 字符串进行检索
                int nRet = SearchOneKey(
            info.Relation,
            key,
            out results,
            out strError);
                if (nRet == -1)
                    return -1;
                info.ResultItems = results;
            }

            return 0;
        }

        // 一个命中结果事项
        class ResultItem
        {
            public string RecPath = "";
            public string Xml = "";
        }

        // RelationControl 所携带的附加信息
        class ControlInfo
        {
            // 对照关系定义
            public Relation Relation = null;

            // 检索命中结果集
            public List<ResultItem> ResultItems = null;

            // 选中状态的行
            public List<string> SelectedLines = null;
        }

        // 针对一个 key 字符串进行检索
        int SearchOneKey(
            Relation relation,
            string strKey,
            out List<ResultItem> results,
            out string strError)
        {
            strError = "";
            results = new List<ResultItem>();

            if (string.IsNullOrEmpty(strKey) == true)
            {
                strError = "strKey 不能为空";
                return -1;
            }

            // 用于去重
            Hashtable recpath_table = new Hashtable();

            int i = 1;
            while (true)
            {
                string strPartKey = strKey.Substring(0, i);
                List<string> temp = new List<string>();
                int nRet = Search(relation.DbName,
                    strPartKey,
                    "left",
                    1001,
                    ref temp,
                    out strError);
                if (nRet == -1)
                    return -1;
                // 去重并加入最后集合
                foreach(string s in temp)
                {
                    string strRecPath = "";
                    string strXml = "";
                    StringUtil.ParseTwoPart(s, "|", out strRecPath, out strXml);
                    if (recpath_table.ContainsKey(strRecPath) == true)
                        continue;
                    recpath_table.Add(strRecPath, 1);
                    ResultItem item = new ResultItem();
                    item.RecPath = strRecPath;
                    item.Xml = strXml;
                    results.Add(item);
                }

                if (nRet < 1001)
                    break;
                if (i >= strKey.Length)
                    break;
                i++;
            }
            return 0;
        }

        public delegate int delegate_SearchDictionary(
            Stop stop,
            string strDbName,
            string strKey,
            string strMatchStyle,
            int nMaxCount,
            ref List<string> results,
            out string strError);

        public delegate void delegate_DoStop(object sender, StopEventArgs e);

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

            this.ShowMessage("正在针对数据库 "+strDbName+" 检索词条 '" + strKey + "' ...");

            lock (this._stop)
            {
                _stop.OnStop += new StopEventHandler(this.ProcDoStop);
                _stop.Initial("正在检索词条 '" + strKey + "' ...");
                _stop.BeginLoop();
                try
                {
                    int nRet = this.ProcSearchDictionary(
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
                    _stop.OnStop -= new StopEventHandler(this.ProcDoStop);
                    _stop.Initial("");
                }
            }
        }

        private void dpTable1_PaintRegion(object sender, PaintRegionArgs e)
        {
            if (e.Action == "query")
            {
                DpCell cell = e.Item as DpCell;
#if NO
                e.Height = BAR_HEIGHT + TOP_BLANK;
                DpRow row = cell.Container;
#endif
                SizeF size = e.pe.Graphics.MeasureString(cell.Text, this.dpTable1.Font);
                e.Height = (int)size.Height - e.Height;
                return;
            }

#if NO
            {
                DpCell cell = e.Item as DpCell;
                if (cell == null)
                {
                    Debug.Assert(false, "");
                    return;
                }

                int nLevel = Int32.Parse(cell.Container[COLUMN_LEVEL].Text);

                // 绘制带有下划线的分类号文字
                RelationControl.PaintSourceText(e.pe.Graphics,
                    this.dpTable1.Font,
                    this.dpTable1.ForeColor,
                    e.X,
                    e.Y - e.YOffset,
                    cell.Text,
                    nLevel);
            }
#endif
        }

        private void dpTable1_PaintBack(object sender, PaintBackArgs e)
        {
            if (e.Item is DpRow)
            {
                DpRow row = e.Item as DpRow;
                row.PaintBackground(e.pe.Graphics, e.Rect);
                return;
            }

            DpCell cell = e.Item as DpCell;
            if (cell == null)
            {
                // Debug.Assert(false, "");
                return;
            }
            if (cell.OwnerDraw == false)
                return;

            bool bSelected = cell.Container.Selected;
            // cell.PaintBackground(e.pe.Graphics, e.Rect, cell.Container.Selected);

            int nLevel = Int32.Parse(cell.Container[COLUMN_LEVEL].Text);

            // 绘制下划线
            RelationControl.PaintSourceTextUnderline(e.pe.Graphics,
                this.dpTable1.Font,
                bSelected ? Color.LightGreen : Color.DarkGreen,
                e.Rect.X + 2,
                e.Rect.Y + e.Rect.Height - 2,
                cell.Text,
                nLevel);
        }

        int _disableSelectionChanged = 0;

        private void dpTable1_SelectionChanged(object sender, EventArgs e)
        {
            if (_disableSelectionChanged == 0
                && this._currentControl != null)
            {
                List<string> rel_numbers = new List<string>();  // rel 栏字符串的集合
                List<string> keys = new List<string>(); // key + "|" + rel 栏字符串的集合
                foreach (DpRow row in this.dpTable1.SelectedRows)
                {
                    string strRel = row[COLUMN_REL].Text;
                    string strKey = row[COLUMN_KEY].Text;
                    rel_numbers.Add(strRel);
                    keys.Add(strKey + "|" + strRel);
                }

                this._currentControl.TargetText = StringUtil.MakePathList(rel_numbers);

                // 记忆全部处于选择状态的行
                ControlInfo info = (ControlInfo)this._currentControl.Tag;
                Debug.Assert(info != null, "");
                info.SelectedLines = keys;

                SimulateAddClassNumber();
            }
        }

        void DisplayMarc(string strOldMARC, 
            string strNewMARC)
        {
            string strError = "";

            string strHtml2 = "";

            if (string.IsNullOrEmpty(strOldMARC) == false
   && string.IsNullOrEmpty(strNewMARC) == true)
            {
                strHtml2 = MarcUtil.GetHtmlOfMarc(strOldMARC,
                    null,
                    null,
                    false);
            }
            else
            {
                // 创建展示两个 MARC 记录差异的 HTML 字符串
                // return:
                //      -1  出错
                //      0   成功
                int nRet = MarcDiff.DiffHtml(
                    strOldMARC,
                    null,
                    null,
                    strNewMARC,
                    null,
                    null,
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                {
                }
            }

            string strHtml = "<html>" +
this.MarcHtmlHead +
"<body>" +
strHtml2 +
"</body></html>";

            this.webBrowser1.Stop();
            Global.SetHtmlString(this.webBrowser1,
    strHtml,
    this.TempDir,
    "temp_html");
        }

        public string TempDir
        {
            get;
            set;
        }

        public string MarcHtmlHead
        {
            get;
            set;
        }

        // 模拟进行插入分类号的操作，并显示出 MARC 新旧对照格式
        void SimulateAddClassNumber()
        {
            MarcRecord record = new MarcRecord(this.RelationCollection.MARC);
            bool bChanged = false;
            foreach(RelationControl control in this.flowLayoutPanel_relationList.Controls)
            {
                string strNewClass = control.TargetText;
                if (string.IsNullOrEmpty(strNewClass) == true)
                    continue;
                ControlInfo info = (ControlInfo)control.Tag;
                string strTargetFieldName = info.Relation.TargetDef.Substring(0, 3);
                string strTargetSubfieldName = info.Relation.TargetDef.Substring(3);

                List<string> class_list = StringUtil.SplitList(strNewClass);
                foreach(string text in class_list)
                {
                    MarcField field = new MarcField('$', strTargetFieldName + "  " + "$" + strTargetSubfieldName + text);
                    record.ChildNodes.insertSequence(field, InsertSequenceStyle.PreferTail);
                    bChanged = true;
                }
            }

            if (bChanged)
                this.OutputMARC = record.Text;
            else
                this.OutputMARC = "";

            DisplayMarc(this.RelationCollection.MARC, 
                bChanged ? this.OutputMARC : null);
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.splitContainer_main);
                controls.Add(this.splitContainer_relation);
                controls.Add(this.dpTable1);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.splitContainer_main);
                controls.Add(this.splitContainer_relation);
                controls.Add(this.dpTable1);
                GuiState.SetUiState(controls, value);
            }
        }

    }
}
