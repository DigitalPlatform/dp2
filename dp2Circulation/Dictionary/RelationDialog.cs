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

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Drawing;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 通过词典库对照关系，在现有 MARC 记录中创建新字段的对话框
    /// </summary>
    public partial class RelationDialog : Form
    {
        // 是否为批处理模式。在批处理模式下，OK和Cancel按钮都被禁用了
        bool _batchMode = false;
        public bool BatchMode
        {
            get
            {
                return this._batchMode;
            }
            set
            {
                this._batchMode = value;
                this.button_OK.Enabled = !value;
                this.button_Cancel.Enabled = !value;
            }
        }

        // 是否需要停止批处理
        // 在 Process() 中途关闭窗口，此成员会被设置为 true
        public bool Stopped = false;

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

        public LibraryChannel Channel = null;   // 外部提供
        public delegate_SearchDictionary ProcSearchDictionary = null;
        // public delegate_DoStop ProcDoStop = null;

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

        // 作为模式对话框打开的时候，传递给 Process() 函数的 strStyle 值
        string _defaultStyle = "auto_select,shangtu,expand_2"; // exact_match
        //                  shangtu 表示使用上图专用的算法
        //                  expand_all_search 表示需要扩展检索。即截断后面若干位以后检索。如果没有这个值，表示使用精确检索
        //                  exact_match 表示精确一致检索。当 exact_match 结合上 expand_all_search 时，中途都是前方一致的，最后一次才会精确一致
        public string DefaultStyle
        {
            get
            {
                return this._defaultStyle;
            }
            set
            {
                this._defaultStyle = value;
            }
        }

        DigitalPlatform.StopManager _stopManager = new DigitalPlatform.StopManager();
        DigitalPlatform.Stop _stop = null;

        private void RelationDialog_Load(object sender, EventArgs e)
        {
            string strError = "";
#if SN
            // return:
            //      -1  出错
            //      0   放弃
            //      1   成功
            int nRet = Program.MainForm.VerifySerialCode("relationdialog",
                false,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                MessageBox.Show(this, "RelationDialog 需要先设置序列号才能使用");
                API.PostMessage(this.Handle, API.WM_CLOSE, 0, 0);
                return;
            }
#endif

            _stopManager.Initial(
                this,
                this.toolStripButton_stop,
(object)this.toolStripLabel_message,
(object)null);

            _stop = new DigitalPlatform.Stop();
            _stop.Register(this._stopManager, "");	// 和容器关联

            {
                _floatingMessage = new FloatingMessageForm(this, true);
                _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }

            this.BeginInvoke(new Action<string>(Process), this._defaultStyle);
        }

        private void RelationDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this._processing > 0)
            {
                if (this._stopManager != null)
                    this._stopManager.DoStopAll(null);
                this.Stopped = true;
                e.Cancel = true;
            }
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

        #region FloatingMessage

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

        #endregion

        void EnableControls(bool bEnable)
        {
            foreach(ToolStripItem item in this.toolStrip1.Items)
            {
                if (item != this.toolStripButton_stop
                    && item != this.toolStripLabel_message
                    && item != this.toolStripButton_exact)
                    item.Enabled = bEnable;
            }
        }

        #region 外部接口

        public List<RelationControl> RelationControls
        {
            get
            {
                List<RelationControl> results = new List<RelationControl>();
                foreach(RelationControl control in this.flowLayoutPanel_relationList.Controls)
                {
                    results.Add(control);
                }

                return results;
            }
        }

        public static string BuildDebugInfo(RelationDialog dlg)
        {
            StringBuilder text = new StringBuilder();
            List<RelationControl> controls = dlg.RelationControls;

            // text.Append("*** 书目记录路径 " + strBiblioRecPath + "\r\n");

            text.Append("* 位数调整 " + dlg.ExpandLevel + "\r\n\r\n");

            int i = 0;
            foreach(RelationControl control in controls)
            {
                RelationDialog.ControlInfo info = (RelationDialog.ControlInfo)control.Tag;

                text.Append("* 关系"+(i+1)+":\r\n");
                text.Append(" 库名=[" + info.Relation.DbName + "] \r\n");
                text.Append(" 源分类号=[" + control.SourceTextOrigin + "] \r\n");
                text.Append(" 源分类号(缩位后的)=[" + control.SourceText + "] \r\n");
                text.Append(" 目标分类号(自动选定)=[" + control.TargetText + "] \r\n");

                if (info.Rows != null)
                {
                    text.Append(" 命中事项(" + info.Rows.Count + "):\r\n");
                    int j = 0;
                    foreach (DpRow row in info.Rows)
                    {
                        text.Append(
                            "   " + (j + 1).ToString() + ") " + BuildRowLine(row) + "\r\n");
                        j++;
                    }
                }

                i++;
            }

            text.Append("\r\n\r\n");

            return text.ToString();
        }

        static string BuildRowLine(DpRow row)
        {
            StringBuilder text = new StringBuilder();
            text.Append("source_key 源=[" + row[RelationDialog.COLUMN_KEY].Text + "] ");
            text.Append("target_key 目标=[" + row[RelationDialog.COLUMN_REL].Text + "] ");
            text.Append("weight权值 =[" + row[RelationDialog.COLUMN_WEIGHT].Text + "] ");
            text.Append("level级别 =[" + row[RelationDialog.COLUMN_LEVEL].Text + "] ");
            return text.ToString();
        }

        static string BuildRowLines(List<DpRow> rows)
        {
            StringBuilder text = new StringBuilder();

                text.Append(" 命中事项(" + rows.Count + "):\r\n");
                int j = 0;
                foreach (DpRow row in rows)
                {
                    text.Append(
                        "   " + (j + 1).ToString() + ") " + BuildRowLine(row) + "\r\n");
                    j++;
                }

                return text.ToString();
        }

        #endregion

        int _processing = 0;

        // parameters:
        //      strStyle auto_select/expand_1/expand_2
        public void Process(string strStyle 
            // = "auto_select,expand_all_search"
            )
        {
            string strError = "";

            this.OutputMARC = "";

            this._processing++;
            this.ShowMessage("正在检索 ...");
            this.EnableControls(false);
            try
            {
                DisplayMarc(this.RelationCollection.MARC,
                    null);

                // 填充关系列表
                int nRet = FillRelationList(out strError);
                if (nRet == -1)
                    goto ERROR1;

                bool bExpand_2 = StringUtil.IsInList("expand_2", strStyle);

                if (bExpand_2 == true)
                {
                    this.toolStripButton_expand_1.Checked = false;
                    this.toolStripButton_expand_2.Checked = true;

                    this.ExpandLevel = 2;
                    // 针对所有关系，检索出事项，并存储起来备用
                    // return:
                    //      -2  key 已经截断到极限
                    //      -1  出错
                    //      >=0 命中总数
                    nRet = SearchAll(strStyle, this.ExpandLevel, out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    this.toolStripButton_expand_1.Checked = true;
                    this.toolStripButton_expand_2.Checked = false;

                    this.ExpandLevel = 1;  // 1 表示不截断的精确一致; 0 表示不截断的前方一致; -1 表示截断一个字符，前方一致
                    while (true)
                    {
                        // 针对所有关系，检索出事项，并存储起来备用
                        // return:
                        //      -2  key 已经截断到极限
                        //      -1  出错
                        //      >=0 命中总数
                        nRet = SearchAll(strStyle, this.ExpandLevel, out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == -2)
                            break;
                        if (nRet > 0)
                            break;
                        this.ExpandLevel--;
                    }
                }

                // 选取第一个关系，填充事项列表
                if (this.flowLayoutPanel_relationList.Controls.Count > 0)
                {
                    RelationControl control = (RelationControl)this.flowLayoutPanel_relationList.Controls[0];
                    SelectControl(control);
                }

                if (StringUtil.IsInList("auto_select", strStyle) == true)
                {
                    AutoSelect(strStyle);
                }
            }
            finally
            {
                this.EnableControls(true);
                this.ClearMessage();
                this._processing--;
            }
            this.dpTable1.BackColor = SystemColors.Window;
            return;
        ERROR1:
            this.dpTable1.BackColor = Color.Red;
            this.ShowMessage(strError, "red", true);
        }

        class RowWrapper
        {
            public RelationControl Control = null;
            public DpRow Row = null;
        }

        class Merged
        {
            public string Rel = "";
            public int Weight = 0;   // 权值之和
            public List<string> SourceClassTypes = new List<string>(); // 源分类号类型。例如 "DDC"
            public List<RowWrapper> Wrappers = new List<RowWrapper>();
            public double ClusterWeight = 0;    // 聚集权重
        }

        /*
谢老师，下午与庄老师和张老师确定了这样一个实现方案：
         * LC类号和DDC类号都是全号精确匹配获得CLC，结果取两者对应LCC类号内容相同，并且权值和最大的项。
         * 如果权值和最大的有多项，则取LC（以后也有可能是DDC）权值最大的，如果这还有多项，则任选一项。
         * 降级取号的事情稍后再做。
         * * */
        // 自动选择事项
        // TODO: 可以只管选择 row 而不管当前的 RelationControl，等全部选择完成后，再统一刷新一次每个 Control 的 TargetText
        // parameters:
        //      strStyle shangtu
        // return:
        //      被选中的事项个数
        public int AutoSelect(string strStyle)
        {
            _disableSelectionChanged++; // 防止 control 的 TargetText 被清掉
            try
            {
                if (StringUtil.IsInList("shangtu", strStyle) == true)
                {
                    List<RowWrapper> result_rows = new List<RowWrapper>();
                    // *** 第一步，将每个列表中级次最高的加入待处理列表
                    foreach (RelationControl control in this.flowLayoutPanel_relationList.Controls)
                    {
                        ControlInfo info = (ControlInfo)control.Tag;
                        Debug.Assert(info != null, "");

                        int nFirstItemLevel = -1;
                        foreach (DpRow row in info.Rows)
                        {
                            int nCurrentLevel = Int32.Parse(row[COLUMN_LEVEL].Text);
                            if (nFirstItemLevel == -1)
                                nFirstItemLevel = nCurrentLevel;
                            else
                            {
                                // 小于最大级次的事项被舍弃
                                if (nCurrentLevel < nFirstItemLevel)
                                    break;
                            }

                            row.Selected = false;   // 先清除全部已有的选择
                            RowWrapper wrapper = new RowWrapper();
                            wrapper.Control = control;
                            wrapper.Row = row;
                            result_rows.Add(wrapper);
                        }
                    }

                    // *** 第二步，合并一些 rel 相同的事项，但要保留原始 row 便于细节分析
                    // 可能需要另一种 Item Class 来描述合并后的对象

                    // 按照 rel 列排序
                    result_rows.Sort((x, y) =>
                    {
                        return string.Compare(x.Row[COLUMN_REL].Text, y.Row[COLUMN_REL].Text);
                    });

                    // 按照 rel 列合并
                    List<Merged> merged_list = new List<Merged>();
                    Merged current = null;
                    foreach (RowWrapper wrapper in result_rows)
                    {
                        if (current != null
                            && wrapper.Row[COLUMN_REL].Text == current.Rel)
                        {
                        }
                        else
                        {
                            current = new Merged();
                            current.Rel = wrapper.Row[COLUMN_REL].Text;
                            merged_list.Add(current);
                        }

                        current.Weight += Int32.Parse(wrapper.Row[COLUMN_WEIGHT].Text);
                        current.Wrappers.Add(wrapper);

                        ControlInfo info = (ControlInfo)wrapper.Control.Tag;
                        string strType = GetSourceClassType(info.Relation.DbName);
                        if (current.SourceClassTypes.IndexOf(strType) == -1)
                            current.SourceClassTypes.Add(strType);
                    }

#if DEBUG
                    foreach(Merged item in merged_list)
                    {
                        Debug.Assert(item != null, "");
                    }
#endif
                    SetClusterWeight(merged_list);

                    // merged_list 按照 weight 排序。大在前
                    merged_list.Sort((x, y) =>
                    {
                        int nRet = x.Weight - y.Weight;
                        if (nRet != 0)
                            return -1*nRet; // 大在前
                        // 来源于 LCC 的靠前
                        bool bRet1 = x.SourceClassTypes.IndexOf("LCC") != -1;
                        bool bRet2 = y.SourceClassTypes.IndexOf("LCC") != -1;
                        if (bRet1 == true && bRet2 == false)
                            return -1;
                        if (bRet2 == true && bRet1 == false)
                            return 1;
                        Debug.Assert(bRet1 == bRet2, "");
                        double ret = x.ClusterWeight - y.ClusterWeight;
                        return -1*((int)ret);
                    });

                    // 选定第一个
                    if (merged_list.Count > 0)
                        merged_list[0].Wrappers[0].Row.Selected = true;
                }
            }
            finally
            {
                _disableSelectionChanged--;
            }

            RefreshAllTargetText();
            return 0;
        }

        // 为 Merged 队列里面的每个事项设置 聚集权重
        // 所谓聚集权重，就是对一个事项，评估其余事项的偏向性。第一级字符每出现一次，增加 1，第二级字符每出现一次，增加 0.1；然后是 0.01 等，类推
        static void SetClusterWeight(List<Merged> merged_list)
        {
            foreach(Merged current in merged_list)
            {
                SetClusterWeight(merged_list, current);
            }
        }

        static void SetClusterWeight(List<Merged> merged_list, Merged start)
        {
            string strStartClass = start.Rel;
            double total_weight = 0;
            for (int key_length = 1; key_length<strStartClass.Length;key_length++ )
            {
                string strLead = strStartClass.Substring(0, key_length);
                double weight_facter = GetWeightFacter(key_length);
                int nHitCount = 0;
                foreach (Merged current in merged_list)
                {
                    if (current == start)
                        continue;
                    if (current.Rel.StartsWith(strLead) == true)
                    {
                        total_weight += weight_facter;
                        nHitCount++;
                    }
                }
                if (nHitCount == 0)
                    break;  // 某一轮一次也没有匹配的，就遁出
            }
            start.ClusterWeight = total_weight;
        }

        static double GetWeightFacter(int length)
        {
            double result = 10;
            for(int i=0;i<length;i++)
            {
                result = result / 10;
            }
            return result;
        }

        static string GetSourceClassType(string strDbName)
        {
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strDbName, "-", out strLeft, out strRight);
            return strLeft;
        }

        // 根据urow 的 Selected 状态刷新全部 RelationControl 的 TargetText
        bool RefreshAllTargetText()
        {
            bool bChanged = false;
            foreach (RelationControl control in this.flowLayoutPanel_relationList.Controls)
            {
                ControlInfo info = (ControlInfo)control.Tag;
                Debug.Assert(info != null, "");
                string strTargetText = BuildTargetText(info.Rows);
                if (strTargetText != control.TargetText)
                {
                    control.TargetText = strTargetText;
                    bChanged = true;
                }
            }

            if (bChanged == true)
                SimulateAddClassNumber();

            return bChanged;
        }
#if NO
        // 自动选择事项
        // TODO: 可以只管选择 row 而不管当前的 RelationControl，等全部选择完成后，再统一刷新一次每个 Control 的 TargetText
        // parameters:
        //      strStyle remove_dup/select_top_weight
        // return:
        //      被选中的事项个数
        public int AutoSelect(string strStyle)
        {
            int nSelectedCount = 0;
            List<SelectedItem> items = new List<SelectedItem>();
            int i = 0;
            foreach(RelationControl control in this.flowLayoutPanel_relationList.Controls)
            {
                SelectControl(control);
                if (this.dpTable1.Rows.Count == 0)
                    continue;
                int index = 0;
#if NO
                if (i == 0)
                    index = 1;
#endif
                SelectedItem item = SelectItem(control, index, true);
                items.Add(item);
                i++;
            }

            nSelectedCount = items.Count;

            // 按照目标分类号类型聚集
            Hashtable table = new Hashtable();  // 目标分类号类型 --> List<SelectedItem>
            foreach(SelectedItem item in items)
            {
                ControlInfo info = (ControlInfo)item.Control.Tag;
                string strDbName = info.Relation.DbName;

                string strSourceType = "";
                string strTargetType = "";
                StringUtil.ParseTwoPart(strDbName, "-", out strSourceType, out strTargetType);

                List<SelectedItem> array = (List<SelectedItem>)table[strTargetType];
                if (array == null)
                {
                    array = new List<SelectedItem>();
                    table[strTargetType] = array;
                }
                array.Add(item);
            }

            // 对每一种类型，进行去重处理
            foreach(string type in table.Keys)
            {
                List<SelectedItem> array = (List<SelectedItem>)table[type];
                List<SelectedItem> removed = RemoveDup(array, strStyle);
                foreach (SelectedItem item in removed)
                {
                    SelectControl(item.Control);
                    SelectItem(item.Control,
                        item.Index,
                        false);
                    nSelectedCount--;
                }
            }

            return nSelectedCount;
        }
#endif

        // 去掉重复的 key
        // TODO: 可以增加输出调试信息的功能，便于测试和排错
        List<SelectedItem> RemoveDup(List<SelectedItem> items, string strStyle)
        {
            List<SelectedItem> results = new List<SelectedItem>();

            if (StringUtil.IsInList("remove_dup", strStyle) == true)
            {
                // 按照 rel 字符串排序
                items.Sort((x, y) =>
                {
                    return string.CompareOrdinal(x.Rel, y.Rel);
                });
                for (int i = 0; i < items.Count; i++)
                {
                    string strRel1 = items[i].Rel;
                    for (int j = i + 1; j < items.Count; j++)
                    {
                        string strRel2 = items[j].Rel;
                        if (strRel1 == strRel2)
                        {
                            results.Add(items[j]);
                            items.RemoveAt(j);
                            j--;
                        }
                        else
                        {
                            i = j - 1;
                            break;
                        }
                    }
                }
            }

            if (StringUtil.IsInList("select_top_weight", strStyle) == true)
            {
                // 按照 weight 字符串排序
                items.Sort((x, y) =>
                {
                    // weight
                    return -1*(x.Weight - y.Weight);  // 大在前
                });
                // 删除第一个以后的所有元素
                for (int i = 1; i < items.Count; i++)
                {
                    results.Add(items[i]);
                    items.RemoveAt(i);
                    i--;
                }
            }

            return results;
        }

        // 在 dpTable 中选择一行，然后返回这行的信息。或者去掉对一行的选择
        SelectedItem SelectItem(RelationControl control,
            int index,
            bool bSelect)
        {
            DpRow row = this.dpTable1.Rows[index];
            if (bSelect == false)
            {
                row.Selected = false;
                return null;
            }
            SelectedItem item = new SelectedItem();
            item.Control = control;
            item.Index = index;
            item.Key = row[COLUMN_KEY].Text;
            item.Rel = row[COLUMN_REL].Text;
            item.Level = Int32.Parse(row[COLUMN_LEVEL].Text);
            item.Weight = Int32.Parse(row[COLUMN_WEIGHT].Text);
            row.Selected = true;
            return item;
        }

        class SelectedItem
        {
            public RelationControl Control = null;
            public int Index = -1;
            public string Key = "";
            public string Rel = "";
            public int Level = 0;
            public int Weight = 0;
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
            this._currentControl = null;
            this.dpTable1.Rows.Clear();
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

#if NO
        static string CanonicalizeLCC(string strText)
        {
            // 第一阶段，处理空格
            int nRet = strText.IndexOf(" ");
            if (nRet != -1)
                strText = strText.Substring(0, nRet);

            // 第二阶段，处理 点+字母
            string[] parts = strText.Split(new char[] { '.' });
            // 倒着找到 点+字母位置
            int i = parts.Length - 1;
            for (; i >= 0; i--)
            {
                string strPart = parts[i];
                if (string.IsNullOrEmpty(strPart) == true)
                    continue;

                if (char.IsLetter(strPart[0]) == true)
                    goto FOUND;
            }
            return strText;

        FOUND:
            if (i == 0)
                return strText;

            // 装配出结果
            StringBuilder text = new StringBuilder();
            for (int j = 0; j < i; j++)
            {
                if (j > 0)
                    text.Append(".");
                text.Append(parts[j]);
            }
            return text.ToString();
        }
#endif
        static string CanonicalizeLCC(string strText)
        {
            // 第一阶段，处理空格
            int nRet = strText.IndexOf(" ");
            if (nRet != -1)
                strText = strText.Substring(0, nRet);

            // 第二阶段，处理 点+字母
            string[] parts = strText.Split(new char[] { '.' });
            // 从第二个片段开始找 点+字母位置
            int i = 1;
            for (; i < parts.Length; i++)
            {
                string strPart = parts[i];
                if (string.IsNullOrEmpty(strPart) == true)
                    continue;

                if (char.IsLetter(strPart[0]) == true)
                    goto FOUND;
            }
            return strText;

        FOUND:
            if (i == 0)
                return strText;

            // 装配出结果
            StringBuilder text = new StringBuilder();
            for (int j = 0; j < i; j++)
            {
                if (j > 0)
                    text.Append(".");
                text.Append(parts[j]);
            }
            return text.ToString();
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

                    string strKey = key;
                    if (relation.DbName.StartsWith("DDC") == true)
                        strKey = strKey.Replace("/", "");
                    else if (relation.DbName.StartsWith("LCC") == true)
                    {
                        //string strSave = strKey;
                        strKey = CanonicalizeLCC(strKey);
                        //if (strSave != strKey)
                        //    MessageBox.Show(this, "old=" + strSave + ",new=" + strKey);
                    }

                    RelationControl control = new RelationControl();
                    control.Tag = info;
                    control.TitleText = relation.DbName;
                    if (string.IsNullOrEmpty(relation.Color) == false)
                        control.TitleBackColor = ColorUtil.String2Color(relation.Color);
                    control.SourceTextOrigin = strKey;
                    control.SourceText = strKey;
                    control.BackColor = SystemColors.Window;
                    AddEvents(control, true);

                    this.flowLayoutPanel_relationList.Controls.Add(control);
                }
            }

            return 0;
        }

        // 计算 strKey1 和 strKey2 的左端一致的匹配字符数，也就是级次。级次越高，表示匹配的字符越多。
        static int GetLevel(string strKey1, string strKey2)
        {
            if (string.IsNullOrEmpty(strKey1) == true
                || string.IsNullOrEmpty(strKey2) == true)
                return 0;

            // Debug.Assert(strKey2 != "621.3675", "");

            int nLength = Math.Min(strKey1.Length, strKey2.Length);   // 较小的那个长度
            for(int i=1;i<=nLength;i++)
            {
                if (strKey2.StartsWith(strKey1.Substring(0, i)) == false)
                    return i - 1;
            }

            return nLength;
        }

        // 准备 dpTable Rows 事项。但不急于填充
        static int PrepareRows(RelationControl control,
            List<ResultItem> items,
            out List<DpRow> rows,
            out string strError)
        {
            strError = "";
            rows = new List<DpRow>();

            ControlInfo info = (ControlInfo)control.Tag;

            // List<ResultItem> items = info.ResultItems;
            if (items == null || items.Count == 0)
                return 0;

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
                    return -1;
                }

                XmlElement node = dom.DocumentElement.SelectSingleNode("key") as XmlElement;

                string strKey = node.GetAttribute("name");
                // string strKeyCaption = DomUtil.GetCaption(this.Lang, node);

                if (info.Relation.DbName.StartsWith("DDC") == true)
                    strKey = strKey.Replace("/", "");

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
                        int nLevel = GetLevel(strControlKey, strKey);
                        Debug.Assert(nLevel <= strControlKey.Length && nLevel <= strKey.Length, "");
                        cell.Text = nLevel.ToString();
                        row.Add(cell);
                    }

                    rows.Add(row);
                }
            }

            rows.Sort(CompareEntrys);

            // 如果 control 还没有初始化过 hitcounts，则初始化一次
            if (control.HitCounts.Count == 0)
                control.HitCounts = BuildHitCountList(rows, strControlKey);

#if NO
            // 选定特定行
            if (info.SelectedLines != null && info.SelectedLines.Count > 0)
            {
                foreach (string line in info.SelectedLines)
                {
                    SelectRowByKey(line);
                }
            }
#endif
            return 0;
        }

        // 为当前选定的 RelationControl 填充 dpTable Rows 事项
        void FillEntryList(RelationControl control)
        {
            // string strError = "";

            _disableSelectionChanged++; // 防止 control 的 TargetText 被清掉
            this.dpTable1.Rows.Clear();
            _disableSelectionChanged--;

            ControlInfo info = (ControlInfo)control.Tag;

            List<DpRow> rows = info.Rows;
            if (rows == null || rows.Count == 0)
                return;

            foreach(DpRow row in rows)
            {
                this.dpTable1.Rows.Add(row);
            }
        }

#if NO
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
                        int nLevel = GetLevel(strControlKey, strKey);
                        Debug.Assert(nLevel <= strControlKey.Length && nLevel <= strKey.Length, "");
                        cell.Text = nLevel.ToString();
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

#endif

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

        static void VerifyLevelString(string strLevel)
        {
            int index = Int32.Parse(strLevel);
            if (index <= 0)
                throw new Exception("strLevel 字符串 '" + strLevel + "' 不合法。应为大于等于 1 的数字");
        }

        static List<string> BuildHitCountList(List<DpRow> rows, string strSourceKey)
        {
            try
            {
                List<string> list = new List<string>();
                Hashtable table = new Hashtable();  // strKey --> int
                foreach (DpRow row in rows)
                {
                    string strLevel = row[COLUMN_LEVEL].Text;

                    // throw new Exception("test");
                    VerifyLevelString(strLevel);

                    int value = 0;
                    if (table.ContainsKey(strLevel) == true)
                        value = (int)table[strLevel];
                    table[strLevel] = value + 1;
                }

                foreach (string key in table.Keys)
                {
                    int index = Int32.Parse(key);
                    int value = (int)table[key];
                    SetValue(list, index - 1, value);
                }

                return list;
            }
            catch(Exception ex)
            {
                throw new Exception("BuildHitCountList() 出现异常: "
                    + "strSourceKey=["+strSourceKey+"]"
                    + BuildRowLines(rows), ex);
            }
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

        static int CompareEntrys(DpRow x, DpRow y)
        {
            // level
            int level1 = Int32.Parse(x[COLUMN_LEVEL].Text);
            int level2 = Int32.Parse(y[COLUMN_LEVEL].Text);
            int nDelta = (level1 - level2);
            if (nDelta != 0)
                return -1 * nDelta; // 降序

            // 级次一样的，还要看是否精确命中。TODO: 这个要考虑到最后阶段
            string strKey1 = x[COLUMN_KEY].Text;
            string strKey2 = y[COLUMN_KEY].Text;

            int nRestLength1 = strKey1.Length - level1;
            int nRestLength2 = strKey2.Length - level2;
            nDelta = nRestLength1 - nRestLength2;
            if (nDelta != 0)
                return nDelta; // 升序

            // key
            nDelta = string.CompareOrdinal(strKey1, strKey2);
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

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        public int ExpandLevel = 1; // 1 表示不截断的精确一致; 0 表示不截断的前方一致; -1 表示截断一个字符，前方一致

        // Hashtable _table = new Hashtable(); // RelationControl --> List<ResultItem>

        // 针对所有关系，检索出事项，并存储起来备用
        // parameters:
        //      strStyle    
        //      nExpandLevel    2 表示每个检索词独立自动截断探索; 1 表示不截断的精确一致; 0 表示不截断的前方一致; -1 表示截断一个字符，前方一致
        // return:
        //      -2  key 已经截断到极限
        //      -1  出错
        //      >=0 命中总数
        int SearchAll(string strStyle,
            int nExpandLevel,
            out string strError)
        {
            strError = "";

            // this._table.Clear();
            lock (this._stop)
            {
                _stop.OnStop += new StopEventHandler(this.DoStop);
                _stop.Initial("正在检索词条 ...");
                _stop.BeginLoop();

                this.Channel.Timeout = new TimeSpan(0, 5, 0);   // 5 分钟
                try
                {
                    int nCount = 0;
                    int nKeyCount = 0;  // 实际参与检索的 key 个数
                    foreach (RelationControl control in this.flowLayoutPanel_relationList.Controls)
                    {
                        Application.DoEvents();
                        if (_stop.State != 0)
                        {
                            this.Stopped = true;
                            strError = "中断";
                            return -1;
                        }

                        string key = control.SourceTextOrigin;
                        ControlInfo info = (ControlInfo)control.Tag;

                        // 如果必要，缩减 key 长度
                        if (nExpandLevel < 0)
                        {
                            Debug.Assert(string.IsNullOrEmpty(key) == false, "");
                            int nLength = key.Length + nExpandLevel;
                            if (nLength < 1)
                            {
                                control.SourceText = "";
                                info.Rows = new List<DpRow>();
                                continue;
                            }
                            key = key.Substring(0, nLength);
                        }

                        nKeyCount++;

                        string strOneStyle = "";
                        if (nExpandLevel == 2)
                            strOneStyle = "auto_expand";
                        else if (nExpandLevel == 1)
                            strOneStyle = "exact";
                        else
                            strOneStyle = "left";

                        string strOutputKey = "";
                        List<ResultItem> results = null;
                        // 针对一个 key 字符串进行检索
                        // return:
                        //      -2  中断
                        //      -1  出错
                        //      0   成功
                        int nRet = SearchOneKey(
                    info.Relation,
                    key,
                    strOneStyle,
                    out strOutputKey,
                    out results,
                    out strError);
                        if (nRet == -1)
                            return -1;

                        control.SourceText = strOutputKey;

                        List<DpRow> rows = null;
                        nRet = PrepareRows(control,
                            results,
                            out rows,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        info.Rows = rows;
                        nCount += rows.Count;
                    }

                    if (nKeyCount == 0)
                        return -2;
                    return nCount;
                }
#if NO
                catch (Exception ex)
                {
                    string strContent = BuildDebugInfo(this);
                    throw new Exception(strContent, ex);
                }
#endif
                finally
                {
                    _stop.EndLoop();
                    _stop.OnStop -= new StopEventHandler(this.DoStop);
                    _stop.Initial("");
                }
            }
        }

        // 一个命中结果事项
        class ResultItem
        {
            public string RecPath = "";
            public string Xml = "";
        }

        // RelationControl 所携带的附加信息
        public class ControlInfo
        {
            // 对照关系定义
            public Relation Relation = null;

#if NO
            // 检索命中结果集
            public List<ResultItem> ResultItems = null;
#endif
            public List<DpRow> Rows = null;

#if NO
            // 选中状态的行
            public List<string> SelectedLines = null;
#endif
        }

        public int MaxHitCount = 500;

        // 一个检索事项
        class SearchItem
        {
            public string Key = ""; // 检索 key
            public string MatchStyle = "exact"; // 匹配方式
            public string Style = "";   // stop 若命中后就停止检索。否则还要继续检索
        }

        // 针对一个 key 字符串进行检索
        // parameters:
        //      strMatchStyle   auto_expand/exact/left 分别是自动截断探索/精确一致/前方一致
        //      strStyle    expand_all_search 表示需要扩展检索。即截断后面若干位以后检索。如果没有这个值，表示使用精确检索
        //                  exact_match 表示精确一致检索。当 exact_match 结合上 expand_all_search 时，中途都是前方一致的，最后一次才会精确一致
        //      strOutputKey    [out]经过加工后的 key。可能和 strKey 内容不同
        // return:
        //      -1  出错
        //      0   成功
        int SearchOneKey(
            Relation relation,
            string strKey,
            // string strStyle,
            string strMatchStyle,
            out string strOutputKey,
            out List<ResultItem> results,
            out string strError)
        {
            strError = "";
            results = new List<ResultItem>();
            strOutputKey = strKey;

            if (string.IsNullOrEmpty(strKey) == true)
            {
                strError = "strKey 不能为空";
                return -1;
            }

#if NO
            bool bExpandAllSearch = StringUtil.IsInList("expand_all_search", strStyle);
            bool bExpandSearch = StringUtil.IsInList("expand_search", strStyle);
            bool bExactMatch = StringUtil.IsInList("exact_match", strStyle);

            if (bExpandAllSearch == true && bExpandSearch == true)
            {
                strError = "strStyle 参数中 expand_all_search 和 expand_search 只能使用其中一个";
                return -1;
            }

            // 需要执行检索的 key 的数组
            // List<string> keys = new List<string>();
            List<SearchItem> keys = new List<SearchItem>();
            if (bExpandAllSearch == true)
            {
#if NO
                // 如要实现扩展检索，则先把全部可能的级次的 key 都准备好
                for (int i = 1; i < strKey.Length; i++)
                {
                    keys.Add(strKey.Substring(0, i));
                }
#endif
                for (int i = strKey.Length; i > 0; i--)
                {
                    if (i == strKey.Length)
                    {
                        SearchItem key = new SearchItem();
                        key.Key = strKey;
                        key.MatchStyle = "exact";
                        keys.Add(key);
                    }

                    {
                        SearchItem key = new SearchItem();
                        key.Key = strKey.Substring(0, i);
                        key.MatchStyle = "left";
                        keys.Add(key);
                    }

                }
            }
            else if (bExpandSearch == true)
            {
                // 先检索较长的 key
                for (int i = strKey.Length; i > 0; i--)
                {
                    if (i == strKey.Length)
                    {
                        SearchItem key = new SearchItem();
                        key.Key = strKey;
                        key.MatchStyle = "exact";
                        key.Style = "stop";
                        keys.Add(key);
                    }

                    {
                        SearchItem key = new SearchItem();
                        key.Key = strKey.Substring(0, i);

                        key.MatchStyle = "left";
                        if (i < strKey.Length)
                            key.Style = "stop"; // 命中则停止探索
                        else
                            key.Style = ""; // 如果是最长的 key，则不精确检索即便命中也不停止后面的继续探索
                        keys.Add(key);
                    }

                }
            }
            else
            {
                {
                    SearchItem key = new SearchItem();
                    key.Key = strKey;
                    key.MatchStyle = "exact";
                    key.Style = "stop";
                    keys.Add(key);
                }
            }
#endif
            List<SearchItem> keys = new List<SearchItem>();

            if (strMatchStyle == "exact" || strMatchStyle == "left")
            {
                SearchItem key = new SearchItem();
                key.Key = strKey;
                key.MatchStyle = strMatchStyle; // "exact";
                key.Style = "stop";
                keys.Add(key);
            }
            else if (strMatchStyle == "auto_expand")
            {
                // 先检索较长的 key
                for (int i = strKey.Length; i > 0; i--)
                {
                    if (i == strKey.Length)
                    {
                        SearchItem key = new SearchItem();
                        key.Key = strKey;
                        key.MatchStyle = "exact";
                        key.Style = "stop";
                        keys.Add(key);
                    }

                    {
                        SearchItem key = new SearchItem();
                        key.Key = strKey.Substring(0, i);

                        key.MatchStyle = "left";
                        if (i < strKey.Length)
                            key.Style = "stop"; // 命中则停止探索
                        else
                            key.Style = ""; // 如果是最长的 key，则不精确检索即便命中也不停止后面的继续探索
                        keys.Add(key);
                    }

                }
            }
            else
            {
                strError = "无法识别的 strMatchStyle ["+strMatchStyle+"]";
                return -1;
            }

            // 用于去重
            Hashtable recpath_table = new Hashtable();

            {
                int i = 0;
                foreach (SearchItem key in keys)
                {
                    Application.DoEvents();
                    if (_stop.State != 0)
                    {
                        this.Stopped = true;
                        strError = "中断";
                        return -1;
                    }

                    // string strPartKey = strKey.Substring(0, i);
                    List<string> temp_results = new List<string>();
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      >0  命中的条数
                    int nRet = Search(relation.DbName,
                        key.Key,
                        key.MatchStyle,  // "left",
                        MaxHitCount + 1,
                        ref temp_results,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 去重并加入最后集合
                    foreach (string s in temp_results)
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

                    if (key.Style == "stop" && nRet > 0)
                    {
                        strOutputKey = key.Key; // 实际命中的 key
                        break;
                    }

#if NO
                    // 在扩展检索情形下，如果一次检索命中结果超过极限，说明还需要继续检索下一个 key(这是担心结果集不足以概括更下级的类目)。继续检索下去直到一次检索的结果数量小于极限
                    if (bExpandAllSearch == true && nRet < MaxHitCount + 1)
                        break;
#endif

                    i++;
                }
            }

            return 0;
        }

        public delegate int delegate_SearchDictionary(
            LibraryChannel channel,
            Stop stop,
            string strDbName,
            string strKey,
            string strMatchStyle,
            int nMaxCount,
            ref List<string> results,
            out string strError);

        // public delegate void delegate_DoStop(object sender, StopEventArgs e);

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

            string strMessage = "检索 " + strDbName + " '" + strKey + "' ...";
            this.ShowMessage(strMessage);
            _stop.SetMessage(strMessage);

            // Application.DoEvents();

            int nRet = this.ProcSearchDictionary(
                this.Channel,
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
#if NO
                List<string> rel_numbers = new List<string>();  // rel 栏字符串的集合
                // List<string> keys = new List<string>(); // key + "|" + rel 栏字符串的集合
                foreach (DpRow row in this.dpTable1.SelectedRows)
                {
                    string strRel = row[COLUMN_REL].Text;
                    // string strKey = row[COLUMN_KEY].Text;
                    rel_numbers.Add(strRel);
                    // keys.Add(strKey + "|" + strRel);
                }

                this._currentControl.TargetText = StringUtil.MakePathList(rel_numbers);
#endif
                this._currentControl.TargetText = BuildTargetText(this.dpTable1.SelectedRows);

#if NO
                // 记忆全部处于选择状态的行
                ControlInfo info = (ControlInfo)this._currentControl.Tag;
                Debug.Assert(info != null, "");
                // info.SelectedLines = keys;
#endif

                SimulateAddClassNumber();
            }
        }

        // 根据 Selected == true 状态构造 target text 字符串
        static string BuildTargetText(List<DpRow> rows)
        {
            if (rows == null || rows.Count == 0)
                return "";

            List<string> rel_numbers = new List<string>();  // rel 栏字符串的集合
            foreach (DpRow row in rows)
            {
                if (row.Selected == false)
                    continue;
                string strRel = row[COLUMN_REL].Text;
                rel_numbers.Add(strRel);
            }

            return StringUtil.MakePathList(rel_numbers);
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

        private void toolStripButton_expand_1_Click(object sender, EventArgs e)
        {
#if NO
            string strStyle = _defaultStyle;
            StringUtil.RemoveFromInList("expand_search", true, ref strStyle);
            StringUtil.SetInList(ref strStyle, "expand_all_search", true);
            this.Process(strStyle);
#endif
            string strStyle = _defaultStyle;
            StringUtil.RemoveFromInList("expand_2", true, ref strStyle);
            StringUtil.SetInList(ref strStyle, "expand_1", true);
            this.Process(strStyle);
        }

        private void toolStripButton_exact_Click(object sender, EventArgs e)
        {
            string strStyle = _defaultStyle;
            StringUtil.RemoveFromInList("expand_all_search", true, ref strStyle);
            StringUtil.RemoveFromInList("expand_search", true, ref strStyle);
            this.Process(strStyle);
        }

        private void toolStripButton_expand_2_Click(object sender, EventArgs e)
        {
#if NO
            string strStyle = _defaultStyle;
            StringUtil.RemoveFromInList("expand_all_search", true, ref strStyle);
            StringUtil.SetInList(ref strStyle, "expand_search", true);
            this.Process(strStyle);
#endif
            string strStyle = _defaultStyle;
            StringUtil.RemoveFromInList("expand_1", true, ref strStyle);
            StringUtil.SetInList(ref strStyle, "expand_2", true);
            this.Process(strStyle);
        }

        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
            if (this._stopManager != null)
                this._stopManager.DoStopAll(null);
        }

    }
}
