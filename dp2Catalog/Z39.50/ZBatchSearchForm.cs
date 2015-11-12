using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Z3950;

using DigitalPlatform.Script;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;
using DigitalPlatform.CirculationClient;
using System.Collections;

namespace dp2Catalog
{
    public partial class ZBatchSearchForm : Form, ISearchForm, IZSearchForm
	{
        // 检索词文件是否太大
        bool m_bExceed = false;

        const int WORDS_TYPE_NOTFOUND = 0;
        const int WORDS_TYPE_FOUND = 1;
        const int WORDS_TYPE_ERROR = 2;

        const int WORDS_COLUMN_INDEX = 0;
        const int WORDS_COLUMN_WORD = 1;
        const int WORDS_COLUMN_HITCOUNT = 2;
        const int WORDS_COLUMN_GETCOUNT = 3;
        const int WORDS_COLUMN_ERRORINFO = 4;

        MainForm m_mainForm = null;

        public MainForm MainForm
        {
            get
            {
                return this.m_mainForm;
            }
            set
            {
                this.m_mainForm = value;
            }
        }

        DigitalPlatform.Stop stop = null;

        const int WM_LOADSIZE = API.WM_USER + 201;

        public ZConnectionCollection ZConnections = new ZConnectionCollection();
        public string CurrentRefID = "0";   // "1 0 116101 11 1";
        // MarcFilter对象缓冲池
        public FilterCollection Filters = new FilterCollection();
        public string BinDir = "";


		public ZBatchSearchForm()
		{
			InitializeComponent();

            this.dpTable_records.ImageList = this.imageList_browseItemType;
            this.dpTable_queryWords.ImageList = this.imageList_queryWords;
		}

        private void ZBatchSearchForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                GuiUtil.SetControlFont(this, this.MainForm.DefaultFont);
            }

            this.ZConnections.IZSearchForm = this;  //  this;
            this.ZConnections.LinkStop += new EventHandler(ZConnections_LinkStop);
            this.ZConnections.UnlinkStop += new EventHandler(ZConnections_UnlinkStop);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

            this.BinDir = Environment.CurrentDirectory;

            string strWidths = this.MainForm.AppInfo.GetString(
                "zbatchsearchform",
                "record_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                DpTable.SetColumnHeaderWidth(this.dpTable_records,
                    strWidths,
                    true);
            }

            strWidths = this.MainForm.AppInfo.GetString(
                "zbatchsearchform",
                "queryword_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                DpTable.SetColumnHeaderWidth(this.dpTable_queryWords,
                    strWidths,
                    true);
            }

            int nRet = 0;
            string strError = "";
            nRet = this.zTargetControl1.Load(Path.Combine(m_mainForm.UserDir,"zserver.xml"),
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            this.zTargetControl1.Marc8Encoding = this.m_mainForm.Marc8Encoding;
            this.zTargetControl1.MainForm = this.m_mainForm;  // 2007/12/16

            // 选定上次选定的树节点
            string strLastTargetPath = m_mainForm.AppInfo.GetString(
                "zbatchsearchform",
                "last_targetpath",
                "");
            if (String.IsNullOrEmpty(strLastTargetPath) == false)
            {
                TreeViewUtil.SelectTreeNode(this.zTargetControl1,
                    strLastTargetPath,
                    '\\');
            }

            // Checked节点
            this.zTargetControl1.CheckBoxes = MainForm.AppInfo.GetBoolean(
"zbatchsearchform",
"target_checkboxes",
false);
            if (this.zTargetControl1.CheckBoxes == true)
            {
                string strCheckedPaths = MainForm.AppInfo.GetString(
        "zbatchsearchform",
        "last_checked_targetpaths",
        "");
                if (string.IsNullOrEmpty(strCheckedPaths) == false)
                {
                    this.zTargetControl1.CheckNodes(StringUtil.SplitList(strCheckedPaths, ';'));
                }
            }

            UpdateSelectedServerCount();

            // *** 检索词
            this.textBox_queryLines_filename.Text = m_mainForm.AppInfo.GetString(
                "zbatchsearchform",
                "query_filename",
                "");

            // *** 特性

            // 检索途径
            string[] fromlist = this.m_mainForm.GetFromList();
            this.comboBox_features_from.Items.AddRange(fromlist);

            this.comboBox_features_from.Text = m_mainForm.AppInfo.GetString(
                "zbatchsearchform",
                "from",
                "");

            // 数据格式
            this.comboBox_features_syntax.Text = m_mainForm.AppInfo.GetString(
                "zbatchsearchform",
                "syntax",
                "");

            // 元素集名
            this.comboBox_features_elementSetName.Text = m_mainForm.AppInfo.GetString(
                "zbatchsearchform",
                "elementsetname",
                "");

            // 一词命中最多条数
            this.numericUpDown_features_oneWordMaxHitCount.Value = m_mainForm.AppInfo.GetInt(
                "zbatchsearchform",
                "oneword_maxhitcount",
                10);

            this.textBox_saveResult_singleHitFilename.Text = m_mainForm.AppInfo.GetString(
                "zbatchsearchform",
                "singlehit_filename",
                "");
            this.textBox_saveResult_multiHitFilename.Text = m_mainForm.AppInfo.GetString(
                "zbatchsearchform",
                "multihit_filename",
                "");
            this.textBox_saveResult_notHitFilename.Text = m_mainForm.AppInfo.GetString(
                "zbatchsearchform",
                "nothit_filename",
                "");

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);
        }

        void ZConnections_UnlinkStop(object sender, EventArgs e)
        {
        }

        void ZConnections_LinkStop(object sender, EventArgs e)
        {
            // 不为Connection分配专用的Stop
        }

        private void ZBatchSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }

#if NO
            ZConnection connection = this.GetCurrentZConnection();
            if (connection != null)
            {
                if (connection.Stop.State == 0)
                {
                    DialogResult result = MessageBox.Show(this,
"检索正在进行。需要先停止检索操作，才能关闭窗口。\r\n\r\n要停止检索操作么?",
"ZBatchSearchForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        connection.Stop.DoStop();
                    }
                    e.Cancel = true;
                    return;

                }
            }

            if (this.m_stops != null
                && this.m_stops.Count > 0)
            {
                DialogResult result = MessageBox.Show(this,
                "群检正在进行。需要先停止检索操作，才能关闭窗口。\r\n\r\n要停止检索操作么?",
                "ZBatchSearchForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    StopDirSearchStops(true);

                }
                e.Cancel = true;
                return;
            }

            if (this.m_stops != null)
            {
                StopDirSearchStops(true);
            }

#endif
            //// this.ZChannel.CloseSocket();
            this.ZConnections.CloseAllSocket();

        }

        private void ZBatchSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Style = StopStyle.None;    // 需要强制中断
                stop.DoStop();

                stop.Unregister();	// 和容器关联
                stop = null;
            }

            this.ZConnections.UnlinkAllStop();
            this.ZConnections.LinkStop -= new EventHandler(ZConnections_LinkStop);
            this.ZConnections.UnlinkStop -= new EventHandler(ZConnections_UnlinkStop);

            //// this.ZChannel.CommIdle -= new CommIdleEventHandle(ZChannel_CommIdle);

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                // 选择过的一个节点
                string strLastTargetPath = ZTargetControl.GetNodeFullPath(this.zTargetControl1.SelectedNode,
                    '\\');
                // TODO: applicationInfo有时为null
                MainForm.AppInfo.SetString(
                    "zbatchsearchform",
                    "last_targetpath",
                    strLastTargetPath);

                // Checked节点
                MainForm.AppInfo.SetBoolean(
                    "zbatchsearchform",
                    "target_checkboxes",
                    this.zTargetControl1.CheckBoxes);
                if (this.zTargetControl1.CheckBoxes == true)
                {
                    List<string> checked_paths = this.zTargetControl1.GetCheckedNodeFullPaths();
                    MainForm.AppInfo.SetString(
                        "zbatchsearchform",
                        "last_checked_targetpaths",
                        StringUtil.MakePathList(checked_paths, ";"));
                }
                else
                {
                    MainForm.AppInfo.SetString(
                        "zbatchsearchform",
                        "last_checked_targetpaths",
                        "");
                }

                string strWidths = DpTable.GetColumnWidthListString(this.dpTable_records);
                this.MainForm.AppInfo.SetString(
                    "zbatchsearchform",
                    "record_list_column_width",
                    strWidths);

                strWidths = DpTable.GetColumnWidthListString(this.dpTable_queryWords);
                this.MainForm.AppInfo.SetString(
                    "zbatchsearchform",
                    "queryword_list_column_width",
                    strWidths);

                // *** 检索词
                m_mainForm.AppInfo.SetString(
                    "zbatchsearchform",
                    "query_filename",
                    this.textBox_queryLines_filename.Text);

                // *** 特性

                // 检索途径
                m_mainForm.AppInfo.SetString(
                    "zbatchsearchform",
                    "from",
                    this.comboBox_features_from.Text);

                // 数据格式
                m_mainForm.AppInfo.SetString(
                    "zbatchsearchform",
                    "syntax",
                    this.comboBox_features_syntax.Text);

                // 元素集名
                m_mainForm.AppInfo.SetString(
                    "zbatchsearchform",
                    "elementsetname",
                    this.comboBox_features_elementSetName.Text);

                // 一词命中最多条数
                m_mainForm.AppInfo.SetInt(
                    "zbatchsearchform",
                    "oneword_maxhitcount",
                    (int)this.numericUpDown_features_oneWordMaxHitCount.Value);

                m_mainForm.AppInfo.SetString(
                    "zbatchsearchform",
                    "singlehit_filename",
                    this.textBox_saveResult_singleHitFilename.Text);
                m_mainForm.AppInfo.SetString(
                    "zbatchsearchform",
                    "multihit_filename",
                    this.textBox_saveResult_multiHitFilename.Text);
                m_mainForm.AppInfo.SetString(
                    "zbatchsearchform",
                    "nothit_filename",
                    this.textBox_saveResult_notHitFilename.Text);
            }
            SaveSize();

            this.zTargetControl1.Save();
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOADSIZE:
                    LoadSize();
                    return;
            }
            base.DefWndProc(ref m);
        }

        public void LoadSize()
        {
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state",
                MainForm.DefaultMdiWindowWidth,
                MainForm.DefaultMdiWindowHeight);

            this.MainForm.LoadSplitterPos(
                this.splitContainer_main,
                "zbatchsearchform",
                "splitContainer_main");

            if (this.zTargetControl1.SelectedNode != null)
                this.zTargetControl1.SelectedNode.EnsureVisible();

        }

        public void SaveSize()
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                MainForm.AppInfo.SaveMdiChildFormStates(this,
                    "mdi_form_state");

                // 保存splitContainer_main的状态
                this.MainForm.SaveSplitterPos(
                    this.splitContainer_main,
                    "zbatchsearchform",
                    "splitContainer_main");
            }
        }

        private void comboBox_features_from_SizeChanged(object sender, EventArgs e)
        {
            ComboBox combobox = (ComboBox)sender;

            combobox.Invalidate();
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.tabControl_steps.SelectedTab == this.tabPage_target)
            {
                if (this.zTargetControl1.SelectedNode == null)
                {
                    strError = "请选定检索目标";
                    goto ERROR1;
                }

                this.tabControl_steps.SelectedTab = this.tabPage_queryLines;
                return;
            }

            if (this.tabControl_steps.SelectedTab == this.tabPage_queryLines)
            {
                if (String.IsNullOrEmpty(this.textBox_queryLines_content.Text) == true
                    && String.IsNullOrEmpty(this.textBox_queryLines_filename.Text) == true)
                {
                    strError = "请输入至少一行检索词，或指定检索词文件名";
                    goto ERROR1;
                }

                this.tabControl_steps.SelectedTab = this.tabPage_features;
                return;
            }

            if (this.tabControl_steps.SelectedTab == this.tabPage_features)
            {
                if (String.IsNullOrEmpty(this.comboBox_features_from.Text) == true)
                {
                    strError = "请选定检索途径";
                    goto ERROR1;
                }
                if (String.IsNullOrEmpty(this.comboBox_features_syntax.Text) == true)
                {
                    strError = "请选定数据格式";
                    goto ERROR1;
                } 
                if (String.IsNullOrEmpty(this.comboBox_features_elementSetName.Text) == true)
                {
                    strError = "请选定元素集名";
                    goto ERROR1;
                }

                this.tabControl_steps.SelectedTab = this.tabPage_search;
                return;
            }

            if (this.tabControl_steps.SelectedTab == this.tabPage_search)
            {
                // return:
                //      -1  出错
                //      0   成功
                //      1   中断
                nRet = DoSearch(out strError);
                if (nRet != 0)
                    goto ERROR1;

                this.tabControl_steps.SelectedTab = this.tabPage_saveResults;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // return:
        //      -1  出错
        //      0   成功
        //      1   中断
        int DoSearch(out string strError)
        {
            int nRet = 0;
            strError = "";
            // string strErrorInfo = "";

            if (this.InSearching == true)
            {
                strError = "无法重复启动检索，请稍后再试";
                return -1;
            }

            this.EnableControlsInSearching(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            try
            {

                this.dpTable_records.Rows.Clear(); // TODO: 清除前，如果发现未保存，要警告

                TreeNode node = this.zTargetControl1.SelectedNode;

                node.Expand();

                stop.SetMessage("正在获得检索目标 ...");

                List<TreeNode> target_nodes = new List<TreeNode>();
                if (this.zTargetControl1.CheckBoxes == false)
                {
                    nRet = GetTargetNodes(
                        node,
                        ref target_nodes,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                else
                {
                    nRet = GetCheckedTargetNodes(
                        null,
                        ref target_nodes,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                stop.SetMessage("正在准备检索词 ...");

                List<QueryLine> lines = null;
                nRet = PrepareQueryLines(out lines,
                    out strError);
                if (nRet == -1)
                    return -1;

                stop.SetMessage("正在显示检索词 ...");

                this.dpTable_queryWords.Rows.Clear();
                int index = 0;
                foreach (QueryLine line in lines)
                {
                    DpRow row = new DpRow();
                    row.Tag = line;

                    DpCell cell = new DpCell();
                    cell.Text = (index++ + 1).ToString();
                    cell.ImageIndex = WORDS_TYPE_NOTFOUND;
                    row.Add(cell);

                    cell = new DpCell();
                    cell.Text = line.Word;
                    row.Add(cell);

                    cell = new DpCell();
                    cell.Text = "";
                    row.Add(cell);

                    cell = new DpCell();
                    cell.Text = "";
                    row.Add(cell);

                    cell = new DpCell();
                    cell.Text = "";
                    row.Add(cell);


                    this.dpTable_queryWords.Rows.Add(row);
                }

                long lProgressValue = 0;
                long lProgressMax = target_nodes.Count * lines.Count;
                stop.SetProgressRange(0, lProgressMax);

                for (int i = 0; i < target_nodes.Count; i++)
                {
                    TreeNode target_node = target_nodes[i];

                    string strServerName = "";
                    if (ZTargetControl.IsDatabaseType(target_node) == true)
                        strServerName = target_node.Parent.Text;
                    else
                        strServerName = target_node.Text;

                    ZConnection connection = null;

                    try
                    {
                        connection = this.GetZConnection(target_node);
                    }
                    catch (Exception ex)
                    {
                        strError = ExceptionUtil.GetAutoText(ex);
                        return -1;
                    }

                    Debug.Assert(connection.TargetInfo != null, "");

                    if (connection.TargetInfo.DbNames == null
                        || connection.TargetInfo.DbNames.Length == 0)
                    {
                        strError = "服务器节点 '" + target_node.Text + "' 下的 " + target_node.Nodes.Count.ToString() + "  个数据库节点全部为 '在全选时不参与检索' 属性，所以通过选定该服务器节点无法直接进行检索，只能通过选定其下的某个数据库节点进行检索";
                        // TODO: 是否可以让跳过？
                        return -1;
                    }

                    Debug.Assert(connection.TreeNode == target_node, "");

                    this.m_currentConnection = connection;

                REDO_INITIAL:
                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   希望中断
                    nRet = DoInitialOneServer(
                        connection,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "针对服务器 '" + strServerName + "' 的连接和初始化操作失败：" + strError;
                        DialogResult result = MessageBox.Show(this,
strError+"\r\n\r\n是否要重试连接和初始化?\r\n\r\n--------------------\r\n注：(是)重试  (否)跳过此服务器继续检索其他服务器  (取消)放弃整个检索操作",
"ZBatchSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            return -1;
                        if (result == System.Windows.Forms.DialogResult.No)
                            continue;
                        goto REDO_INITIAL;
                    }

                    // 针对一个服务器，检索每一行检索式
                    for (int j = 0; j < lines.Count; j++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                return 1;
                            }
                        }
                        
                        QueryLine line = lines[j];

                        if (line.ResultRows != null
                            && line.ResultRows.Count > 0)
                        {
                            lProgressValue++;
                            if (lProgressValue <= lProgressMax)
                                stop.SetProgressValue(lProgressValue);
                            continue;   // 已经命中的，不再检索
                        }

                        stop.SetMessage("正在对服务器 " + strServerName + " 检索 '"+line.Word+"' ...");

                        DpRow word_row = this.dpTable_queryWords.Rows[j];
                        // 检索一个服务器
                        // 启动检索以后等待检索结束后才返回
                        // thread:
                        //      界面线程
                        // return:
                        //      -1  出错
                        //      0   成功启动检索
                        nRet = DoSearchOneServer(
                            connection,
                            line,
                            word_row,
                            true,
                            out strError);
                        if (nRet == -1)
                        {
                            return -1;
                            // strErrorInfo += strError + "\r\n";
                        }
                        if (nRet == 1)
                        {
                            strError = "用户中断";
                            return 1;
                        }

                        lProgressValue++;
                        if (lProgressValue <= lProgressMax)
                            stop.SetProgressValue(lProgressValue);
                    }
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControlsInSearching(true);
            }


            // TODO: 出错信息统一汇报?

            return 0;
        }

        void DoStop(object sender, StopEventArgs e)
        {
        }

        bool _bInSearching = false;

        bool InSearching
        {
            get
            {
                return this._bInSearching;
            }
        }

        // 允许或者禁止大部分控件，除listview以外
        void EnableControlsInSearching(bool bEnable)
        {
            _bInSearching = !bEnable;

            this.comboBox_features_elementSetName.Enabled = bEnable;
            this.comboBox_features_from.Enabled = bEnable;
            this.comboBox_features_syntax.Enabled = bEnable;
            this.numericUpDown_features_oneWordMaxHitCount.Enabled = bEnable;

            this.textBox_queryLines_content.Enabled = bEnable;
            this.textBox_queryLines_filename.Enabled = bEnable;
            this.textBox_saveResult_multiHitFilename.Enabled = bEnable;
            this.textBox_saveResult_notHitFilename.Enabled = bEnable;
            this.textBox_saveResult_singleHitFilename.Enabled = bEnable;

            this.button_next.Enabled = bEnable;
            // this.button_queryLines_findFilename.Enabled = bEnable;
            this.button_queryLines_load.Enabled = bEnable;
            this.button_saveResult_findMultiHitFilename.Enabled = bEnable;
            this.button_saveResult_findNotHitFilename.Enabled = bEnable;
            this.button_saveResult_findSingleHitFilename.Enabled = bEnable;
            this.button_saveResult_saveMultiHitFile.Enabled = bEnable;
            this.button_saveResult_saveNotHitFile.Enabled = bEnable;
            this.button_saveResult_saveSingleHitFile.Enabled = bEnable;
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2catalog 
发送者 xxx 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.IO.FileNotFoundException
Message: 未能找到文件“C:\Documents and Settings\Administrator\桌面\工大1.txt”。
Stack:
在 System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
在 System.IO.FileStream.Init(String path, FileMode mode, FileAccess access, Int32 rights, Boolean useRights, FileShare share, Int32 bufferSize, FileOptions options, SECURITY_ATTRIBUTES secAttrs, String msgPath, Boolean bFromProxy, Boolean useLongPath)
在 System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share, Int32 bufferSize, FileOptions options)
在 System.IO.StreamReader..ctor(String path, Encoding encoding, Boolean detectEncodingFromByteOrderMarks, Int32 bufferSize)
在 System.IO.StreamReader..ctor(String path, Encoding encoding)
在 dp2Catalog.ZBatchSearchForm.PrepareQueryLines(List`1& lines, String& strError)
在 dp2Catalog.ZBatchSearchForm.DoSearch(String& strError)
在 dp2Catalog.ZBatchSearchForm.button_next_Click(Object sender, EventArgs e)
在 System.Windows.Forms.Control.OnClick(EventArgs e)
在 System.Windows.Forms.Button.OnClick(EventArgs e)
在 System.Windows.Forms.Button.OnMouseUp(MouseEventArgs mevent)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ButtonBase.WndProc(Message& m)
在 System.Windows.Forms.Button.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Catalog 版本: dp2Catalog, Version=2.4.5698.23777, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3 
操作时间 2015/8/11 9:28:18 (Tue, 11 Aug 2015 09:28:18 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 
         * */
        int PrepareQueryLines(out List<QueryLine> lines,
            out string strError)
        {
            strError = "";

            lines = new List<QueryLine>();

            if (this.m_bExceed == false
                && String.IsNullOrEmpty(this.textBox_queryLines_content.Text.Trim()) == false)
            {
                if (this.stop != null)
                    this.stop.SetProgressRange(0, this.textBox_queryLines_content.Lines.Length);
                for (int i = 0; i < this.textBox_queryLines_content.Lines.Length; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    string strWord = this.textBox_queryLines_content.Lines[i].Trim();
                    if (String.IsNullOrEmpty(strWord) == true)
                        continue;

                    string strQueryXml = BuildQueryXml(strWord);

                    QueryLine line = new QueryLine();
                    line.LineNo = i;
                    line.Word = strWord;
                    line.QueryXml = strQueryXml;
                    line.ResultRows = new List<DpRow>();

                    lines.Add(line);

                    if (this.stop != null)
                    {
                        stop.SetMessage("正在准备检索词 " + strWord);
                        stop.SetProgressValue(i);
                    }
                }
                if (this.stop != null)
                    this.stop.HideProgress();
            }
            else
            {
                Encoding encoding = FileUtil.DetectTextFileEncoding(this.textBox_queryLines_filename.Text);
                try
                {
                    // TODO: 是否要限定文件的最大行数？或者对于行数太多的文件，直接从文件分批读入处理，不进入内存?
                    using (StreamReader sr = new StreamReader(this.textBox_queryLines_filename.Text, encoding))
                    {
                        if (this.stop != null)
                            this.stop.SetProgressRange(0, sr.BaseStream.Length);
                        for (int i = 0; ; i++)
                        {
                            Application.DoEvents();	// 出让界面控制权

                            if (stop != null && stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            string strWord = sr.ReadLine();
                            if (strWord == null)
                                break;
                            strWord = strWord.Trim();

                            if (String.IsNullOrEmpty(strWord) == true)
                                continue;

                            string strQueryXml = BuildQueryXml(strWord);

                            QueryLine line = new QueryLine();
                            line.LineNo = i;
                            line.Word = strWord;
                            line.QueryXml = strQueryXml;
                            line.ResultRows = new List<DpRow>();

                            lines.Add(line);

                            if (this.stop != null)
                            {
                                stop.SetMessage("正在准备检索词 " + strWord);
                                stop.SetProgressValue(sr.BaseStream.Position);
                            }
                        }
                        if (this.stop != null)
                            this.stop.HideProgress();

                    }
                }
                catch (FileNotFoundException)
                {
                    strError = "文件 '" + this.textBox_queryLines_filename.Text + "' 不存在";
                    return -1;
                }
                catch (Exception ex)
                {
                    strError = "读取文件 '" + this.textBox_queryLines_filename.Text + "' 内容的过程出现异常: " + ex.Message;
                    return -1;
                }
            }

            return 0;
        }

        public string BuildQueryXml(string strWord)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            {
                XmlNode node = dom.CreateElement("line");
                dom.DocumentElement.AppendChild(node);

                string strLogic = "OR";
                string strFrom = this.comboBox_features_from.Text;

                DomUtil.SetAttr(node, "logic", strLogic);
                DomUtil.SetAttr(node, "word", strWord);
                DomUtil.SetAttr(node, "from", strFrom);
            }

            return dom.OuterXml;
        }

        // 获得一个目录节点以下的全部数据库类型的节点，或者返回开始节点当它是数据库类型或者服务器类型时
        public int GetTargetNodes(
            TreeNode start_node,
            ref List<TreeNode> result_nodes,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (ZTargetControl.IsDatabaseType(start_node) == true
                    || ZTargetControl.IsServerType(start_node) == true)
            {
                result_nodes.Add(start_node);
                return 0;
            }

            TreeNodeCollection nodes = start_node.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                TreeNode node = nodes[i];

                if (ZTargetControl.IsDirType(node) == true)
                {
                    nRet = GetTargetNodes(node,
                        ref result_nodes,
                        out strError);
                if (nRet == -1)
                    return -1; 
                }
                else if (ZTargetControl.IsDatabaseType(node) == true)
                {
                    result_nodes.Add(node);
                }
            }

            return 0;
        }


        public int GetCheckedTargetNodes(
    TreeNode start_node,
    ref List<TreeNode> result_nodes,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            if (start_node != null
                && ZTargetControl.IsServerType(start_node) == true)
            {
                result_nodes.Add(start_node);
                return 0;
            }

            TreeNodeCollection nodes = null;
            if (start_node != null)
                nodes = start_node.Nodes;
            else
                nodes = this.zTargetControl1.Nodes;

            for (int i = 0; i < nodes.Count; i++)
            {
                TreeNode node = nodes[i];

                if (ZTargetControl.IsDirType(node) == true)
                {
                    nRet = GetCheckedTargetNodes(node,
                        ref result_nodes,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                else if (ZTargetControl.IsServerType(node) == true
                    && node.Checked == true)
                {
                    // 
                    result_nodes.Add(node);
                }
            }

            return 0;
        }

        // 获得和一个服务器树节点相关的ZConnection
        // 如果没有ZConnection，自动创建
        ZConnection GetZConnection(TreeNode node)
        {
            ZConnection connection = this.ZConnections.GetZConnection(node);

            if (connection.TargetInfo == null
                && ZTargetControl.IsDirType(node) == false)
            {
                string strError = "";
                TargetInfo targetinfo = null;
                int nRet = this.zTargetControl1.GetTarget(
                    node,
                    out targetinfo,
                    out strError);
                if (nRet == -1)
                {
                    throw new Exception("GetCurrentZConnection() error: " + strError);
                    // return null;
                }

                connection.TargetInfo = targetinfo;
            }

            return connection;
        }

        // 临时激活Stop显示
        public void ActivateStopDisplay()
        {
            ZConnection connection = this.m_currentConnection;
            if (connection != null)
            {
                m_mainForm.stopManager.Active(connection.Stop);
            }
            else
            {
                m_mainForm.stopManager.Active(null);
            }
        }

        ZConnection m_currentConnection = null;

        // 检索一个服务器
        // 启动检索以后等待检索结束后才返回
        // thread:
        //      界面线程
        // parameters:
        //      nGroupIndex 命中结果组编号。也就是检索词的行号
        //      result_line 现有的浏览行。如果为空，则表示要创建新的浏览行
        //      word_row    检索词行。如果 == null 表示不更新检索词行的显示
        //      bPresent    是否执行 present。如果是刷新，则这里先不执行 present
        // return:
        //      -1  出错
        //      0   成功启动检索
        //      1   希望中断
        public int DoSearchOneServer(
            ZConnection connection,
            QueryLine line,
            // int nLineIndex,
            // QueryResult result_line,
            DpRow word_row,
            bool bPresent,
            out string strError)
        {
            strError = "";
            int nRet = 0;

#if NO
            TreeNode nodeServerOrDatabase,
            ZConnection connection = null;

            try
            {
                connection = this.GetZConnection(nodeServerOrDatabase);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            Debug.Assert(connection.TargetInfo != null, "");

            if (connection.TargetInfo.DbNames == null
    || connection.TargetInfo.DbNames.Length == 0)
            {
                strError = "服务器节点 '" + nodeServerOrDatabase.Text + "' 下的 " + nodeServerOrDatabase.Nodes.Count.ToString() + "  个数据库节点全部为 '在全选时不参与检索' 属性，所以通过选定该服务器节点无法直接进行检索，只能通过选定其下的某个数据库节点进行检索";
                return -1;
            }

            this.m_currentConnection = connection;
            Debug.Assert(connection.TreeNode == nodeServerOrDatabase, "");
#endif

            // m_mainForm.stopManager.Active(connection.Stop); // 缺省是不激活

            connection.Searching = 0;

            string strQueryString = "";

            connection.QueryXml = line.QueryXml;

            connection.TargetInfo.PreferredRecordSyntax = this.comboBox_features_syntax.Text;
            connection.TargetInfo.DefaultElementSetName = this.comboBox_features_elementSetName.Text;

            IsbnConvertInfo isbnconvertinfo = new IsbnConvertInfo();
            isbnconvertinfo.IsbnSplitter = this.m_mainForm.IsbnSplitter;
            isbnconvertinfo.ConvertStyle =
                (connection.TargetInfo.IsbnAddHyphen == true ? "addhyphen," : "")
                + (connection.TargetInfo.IsbnRemoveHyphen == true ? "removehyphen," : "")
                + (connection.TargetInfo.IsbnForce10 == true ? "force10," : "")
                + (connection.TargetInfo.IsbnForce13 == true ? "force13," : "")
                + (connection.TargetInfo.IsbnWild == true ? "wild," : "");

            nRet = ZQueryControl.GetQueryString(
                this.m_mainForm.Froms,
                connection.QueryXml,
                isbnconvertinfo,
                out strQueryString,
                out strError);
            if (nRet == -1)
                return -1;

            connection.QueryString = strQueryString;

            if (strQueryString == "")
            {
                strError = "尚未输入检索词";
                return -1;
            }

#if THREAD_POOLING
            List<string> commands = new List<string>();
            commands.Add("search");
            if (bPresent == true)
                commands.Add("present");

            connection.SetSearchParameters(
    connection.QueryString,
    connection.TargetInfo.DefaultQueryTermEncoding,
    connection.TargetInfo.DbNames,
    connection.TargetInfo.DefaultResultSetName);

            if (bPresent == true)
            {
                connection.SetPresentParameters(
        connection.TargetInfo.DefaultResultSetName,
        0, // nStart,
        (int)this.numericUpDown_features_oneWordMaxHitCount.Value, // nCount,
        connection.TargetInfo.PresentPerBatchCount,   // 推荐的每次数量
        connection.DefaultElementSetName,    // "F" strElementSetName,
        connection.PreferredRecordSyntax,
        true);
            }
            connection.Stop = this.stop;
            connection.OwnerStop = false;

            connection.BeginCommands(commands);
#else
            connection.Search();
#endif

            // 等待检索结束
            while (true)
            {
                Application.DoEvents();
                /*
                Thread.Sleep(500);
                if (connection.Searching == 2)
                {
                    break;
                }
                 * */
                bool bRet = connection.CompleteEvent.WaitOne(10, true);
                if (bRet == true)
                    break;
            }

            line.ResultCount += connection.ResultCount;


            if (word_row != null)
            {
                // DpRow word_row = this.dpTable_queryWords.Rows[nLineIndex];
                Debug.Assert(word_row.Tag == line, "");

                word_row[WORDS_COLUMN_HITCOUNT].Text = line.ResultCount.ToString();

                if (line.ResultCount == -1)
                {
                    /*
                    this.dpTable_queryWords.Rows[nLineIndex][WORDS_COLUMN_INDEX].ImageIndex = WORDS_TYPE_ERROR;
                    this.dpTable_queryWords.Rows[nLineIndex][WORDS_COLUMN_ERRORINFO].Text = connection.ErrorInfo;
                     * */
                    word_row[WORDS_COLUMN_INDEX].ImageIndex = WORDS_TYPE_ERROR;
                    word_row[WORDS_COLUMN_ERRORINFO].Text = connection.ErrorInfo;
                }
                else if (line.ResultCount > 0)
                {
                    // this.dpTable_queryWords.Rows[nLineIndex][WORDS_COLUMN_INDEX].ImageIndex = WORDS_TYPE_FOUND;
                    word_row[WORDS_COLUMN_INDEX].ImageIndex = WORDS_TYPE_FOUND;
                }
            }

            if (bPresent == true)
            {
                // 把命中结果合并到listview
                AppendSearchResult(line,
                    connection);
            }

            if (word_row != null)
                word_row[WORDS_COLUMN_GETCOUNT].Text = connection.VirtualItems.Count.ToString();

            bool bStopped = connection.Stopped;

            /*
            connection.CloseConnection();
            this.m_currentConnection = null;
             * */

            if (bStopped == true)
                return 1;
            return 0;
        }

        // 连接和初始化一个服务器
        // 启动以后等待结束才返回
        // thread:
        //      界面线程
        // parameters:
        // return:
        //      -1  出错
        //      0   成功
        //      1   希望中断
        public int DoInitialOneServer(
            ZConnection connection,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // m_mainForm.stopManager.Active(connection.Stop); // 缺省是不激活
            connection.CloseConnection();

            connection.Searching = 0;

#if THREAD_POOLING
            List<string> commands = new List<string>();

            connection.Stop = this.stop;
            connection.OwnerStop = false;

            connection.BeginCommands(commands);
#else
            
#endif

            // 等待初始化结束
            while (true)
            {
                Application.DoEvents();
                bool bRet = connection.CompleteEvent.WaitOne(10, true);
                if (bRet == true)
                    break;
            }

            strError = connection.ErrorInfo;
            if (string.IsNullOrEmpty(strError) == false)
                return -1;

            bool bStopped = connection.Stopped;
            if (bStopped == true)
                return 1;
            return 0;
        }

        // 加入检索结果
        int AppendSearchResult(QueryLine line,
            ZConnection connection)
        {
            int nSubIndex = 0;
            int nInsertPos = -1;
            int nRet = 0;

            if (line.ResultRows != null
                && line.ResultRows.Count > 0)
            {
                nSubIndex = line.ResultRows.Count;
                DpRow tail_row = line.ResultRows[line.ResultRows.Count - 1];
                nInsertPos = this.dpTable_records.Rows.IndexOf(tail_row) + 1;
            }

            if (nInsertPos == -1)
            {
                // 根据检索词行号找到插入点
                for (int i = 0; i < this.dpTable_records.Rows.Count; i++)
                {
                    DpRow row = this.dpTable_records.Rows[i];
                    if (row.Style == DpRowStyle.Seperator)
                        continue;
                    string strText = row[0].Text;
                    string strLineNo = "";
                    nRet = strText.IndexOf(":");
                    if (nRet != -1)
                        strLineNo = strText.Substring(0, nRet).Trim();
                    else
                        strLineNo = strText;

                    int nLineNo = -1;

                    if (Int32.TryParse(strLineNo, out nLineNo) == false)
                        throw new Exception("行号数字 '"+strLineNo+"' 格式不正确");

                    if (nLineNo > line.LineNo)
                    {
                        nInsertPos = i;
                        break;
                    }
                }

                // 如果没有找到，就追加在最后
                if (nInsertPos == -1)
                    nInsertPos = this.dpTable_records.Rows.Count;
            }

            connection.Stop.SetMessage("正在将检索结果填入表格 ...");

            for (int i = 0; i < connection.VirtualItems.Count; i++)
            {
                Application.DoEvents();

                if (i == 0)
                {
                    DpRow sep_row = new DpRow();
                    sep_row.Style = DpRowStyle.Seperator;
                    this.dpTable_records.Rows.Insert(nInsertPos++, sep_row);
                }

                VirtualItem v_item = connection.VirtualItems[i];

                // 浏览行
                DpRow new_row = new DpRow();

                RecordInfo record_info = new RecordInfo();
                record_info.ServerNode = connection.TargetInfo.ServerNode;  // 2013/11/24
                record_info.QueryLine = line;   // 2013/11/24
                record_info.Record = (DigitalPlatform.Z3950.Record)v_item.Tag;
                record_info.TargetInfo = connection.TargetInfo;
                record_info.RecordEncoding = connection.GetRecordsEncoding(
                    this.m_mainForm,
                    record_info.Record.m_strSyntaxOID);
                new_row.Tag = record_info;  // 浏览行的 .Tag 是 RecordInfo
                /*
                if (i == 0)
                    new_row.BackColor = Color.LightGray;
                 * */

                DpCell cell = new DpCell();
                cell.Text = (line.LineNo + 1).ToString() + " : " + (nSubIndex++ + 1).ToString();
                cell.ImageIndex = v_item.ImageIndex;
                new_row.Add(cell);

                for (int j = 1; j < v_item.SubItems.Count; j++)
                {
                    cell = new DpCell();
                    cell.Text = v_item.SubItems[j];
                    new_row.Add(cell);
                }

                this.dpTable_records.Rows.Insert(nInsertPos++, new_row);

                if (line.ResultRows == null)
                    line.ResultRows = new List<DpRow>();

                line.ResultRows.Add(new_row);
            }

            connection.Stop.SetMessage("检索结果 " + connection.VirtualItems.Count.ToString()+ " 项已填入表格 ...");
            return 0;
        }

        // 获得在结果集中的偏移，基于 0
        static int GetOffset(string strText)
        {
            int nRet = strText.IndexOf(":");
            if (nRet == -1)
                return -1;
            string strNumber = strText.Substring(nRet + 1).Trim();
            int v;
            if (int.TryParse(strNumber, out v) == false)
                return -1;

            return v - 1;
        }

        int PresentOneLine(
            DpRow row,
            int index,
            ZConnection connection,
            string strElementSetName)
        {
#if THREAD_POOLING
            List<string> commands = new List<string>();
            commands.Add("present");

            connection.SetPresentParameters(
    connection.TargetInfo.DefaultResultSetName,
    index, // nStart,
    1, // nCount,
    1,  // connection.TargetInfo.PresentPerBatchCount,   // 推荐的每次数量
    strElementSetName,    // "F" strElementSetName,
    connection.PreferredRecordSyntax,
    true);

            connection.Stop = this.stop;
            connection.OwnerStop = false;

            connection.BeginCommands(commands);
#else
            connection.Search();
#endif

            // 等待检索结束
            while (true)
            {
                Application.DoEvents();
                bool bRet = connection.CompleteEvent.WaitOne(10, true);
                if (bRet == true)
                    break;
            }

            if (string.IsNullOrEmpty(connection.ErrorInfo) == false)
                throw new Exception(connection.ErrorInfo);

            RecordInfo record_info = (RecordInfo)row.Tag;

            VirtualItem v_item = connection.VirtualItems[index];

            // TODO: 比较浏览信息是否改变?

            record_info.Record = (DigitalPlatform.Z3950.Record)v_item.Tag;
            row[0].ImageIndex = v_item.ImageIndex;  // 变为 Full ElementSet

            // 更新浏览列
            for (int j = 1; j < v_item.SubItems.Count; j++)
            {
                DpCell cell = null;
                if (j < row.Count)
                    cell = row[j];
                else
                {
                    cell = new DpCell();
                    row.Add(cell);
                }
                cell.Text = v_item.SubItems[j];
            }
            return 0;
        }

        // 刷新浏览行
        int UpdateSearchResult(QueryResult result_line,
            ZConnection connection,
            ref bool bDontAsk)
        {
            int nRet = 0;

            connection.Stop.SetMessage("正在将检索结果刷新到表格 ...");

            int nCount = 0;
            foreach (DpRow row in result_line.Rows)
            {
                string strText = row[0].Text;
                int index = GetOffset(strText);
                if (index == -1)
                    throw new Exception("行号数字 '" + strText + "' 格式不正确");

                if (index >= connection.ResultCount)
                {
                    if (bDontAsk == false)
                    {
                        string strLineText = row[0].Text;
                        if (row.Count >= 2)
                            strLineText += "  " + row[1].Text;
                        MessageDialog.Show(this,
                            "记录 '" + strLineText + "' 在刷新的过程中发现重新检索的结果集发生了变化，此行被迫放弃刷新。解决办法是重新发起检索",
                            "后面不再提示",
                            ref bDontAsk);
                    }
                    continue;
                }
                nRet = PresentOneLine(
                    row,
                    index,
                    connection,
                    "F"    // strElementSetName,
                    );

                nCount++;
            }

            connection.Stop.SetMessage("检索结果 " + nCount.ToString() + " 项已刷新到表格 ...");
            return 0;
        }

        #region IZSearchForm 接口实现

        public void EnableQueryControl(
    ZConnection connection,
    bool bEnable)
        {

        }

        public bool DisplayBrowseItems(ZConnection connection,
            bool bTriggerSelChanged = false)
        {
            return true;
        }


        public bool ShowMessageBox(ZConnection connection,
           string strText)
        {
            return false;
        }

        public delegate bool Delegate_ShowQueryResultInfo(ZConnection connection,
            string strText);

        // 显示查询结果信息
        bool __ShowQueryResultInfo(ZConnection connection,
            string strText)
        {
            // 修改treenode节点上的命中数显示
            ZTargetControl.SetNodeResultCount(connection.TreeNode,
                connection.ResultCount);

            // TODO: 显示到检索词的右端?
            return false;
        }

        public bool ShowQueryResultInfo(ZConnection connection,
           string strText)
        {
            if (this.IsDisposed == true)
                return false;

            object[] pList = { connection, strText };
            return (bool)this.Invoke(
                new Delegate_ShowQueryResultInfo(__ShowQueryResultInfo), pList);

        }

        // 根据不同格式自动创建浏览格式
        public int BuildBrowseText(
            ZConnection connection,
            DigitalPlatform.Z3950.Record record,
            string strStyle,
            out string strBrowseText,
            out int nImageIndex,
            out string strError)
        {
            strBrowseText = "";
            strError = "";
            int nRet = 0;

            nImageIndex = ZSearchForm.BROWSE_TYPE_NORMAL;

            if (record.m_nDiagCondition != 0)
            {
                strBrowseText = "诊断记录 condition=" + record.m_nDiagCondition.ToString() + "; addinfo=\"" + record.m_strAddInfo + "\"; diagSetOID=" + record.m_strDiagSetID;
                nImageIndex = ZSearchForm.BROWSE_TYPE_DIAG;
                return 0;
            }

            string strElementSetName = record.m_strElementSetName;

            if (strElementSetName == "B")
                nImageIndex = ZSearchForm.BROWSE_TYPE_BRIEF;
            else if (strElementSetName == "F")
                nImageIndex = ZSearchForm.BROWSE_TYPE_FULL;

            Encoding currrentEncoding = connection.GetRecordsEncoding(
                this.m_mainForm,
                record.m_strSyntaxOID);

            string strSytaxOID = record.m_strSyntaxOID;
            string strData = currrentEncoding.GetString(record.m_baRecord);

            // string strOutFormat = "";
            string strMARC = "";    // 暂存MARC机内格式数据

            // 如果为XML格式
            if (record.m_strSyntaxOID == "1.2.840.10003.5.109.10")
            {
                // 如果偏向MARC
                if (StringUtil.IsInList("marc", strStyle) == true)
                {
                    // 看根节点的名字空间，如果符合MARCXML, 就先转换为USMARC，否则，就直接根据名字空间找样式表加以转换
                    string strNameSpaceUri = "";
                    nRet = ZSearchForm.GetRootNamespace(strData,
                        out strNameSpaceUri,
                        out strError);
                    if (nRet == -1)
                    {
                        // 取根节点的名字空间时出错
                        return -1;
                    }

                    if (strNameSpaceUri == Ns.usmarcxml)
                    {
                        string strOutMarcSyntax = "";

                        // 将MARCXML格式的xml记录转换为marc机内格式字符串
                        // parameters:
                        //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                        //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                        //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                        nRet = MarcUtil.Xml2Marc(strData,
                            true,
                            "usmarc",
                            out strOutMarcSyntax,
                            out strMARC,
                            out strError);
                        if (nRet == -1)
                        {
                            // XML转换为MARC时出错
                            return -1;
                        }

                        // strOutFormat = "marc";
                        strSytaxOID = "1.2.840.10003.5.10";
                        goto DO_BROWSE;
                    }

                }

                // 不是MARCXML格式
                // strOutFormat = "xml";
                goto DO_BROWSE;
            }

            // SUTRS
            if (record.m_strSyntaxOID == "1.2.840.10003.5.101")
            {
                // strOutFormat = "sutrs";
                goto DO_BROWSE;
            }

            if (record.m_strSyntaxOID == "1.2.840.10003.5.1"    // unimarc
                || record.m_strSyntaxOID == "1.2.840.10003.5.10")  // usmarc
            {
                // ISO2709转换为机内格式
                nRet = Marc8Encoding.ConvertByteArrayToMarcRecord(
                    record.m_baRecord,
                    connection.GetRecordsEncoding(this.m_mainForm, record.m_strSyntaxOID),  // Encoding.GetEncoding(936),
                    true,
                    out strMARC,
                    out strError);
                if (nRet < 0)
                {
                    return -1;
                }

                // 如果需要自动探测MARC记录从属的格式：
                if (connection.TargetInfo.DetectMarcSyntax == true)
                {
                    // return:
                    //		-1	无法探测
                    //		1	UNIMARC	规则：包含200字段
                    //		10	USMARC	规则：包含008字段(innopac的UNIMARC格式也有一个奇怪的008)
                    nRet = ZSearchForm.DetectMARCSyntax(strMARC);
                    if (nRet == 1)
                        strSytaxOID = "1.2.840.10003.5.1";
                    else if (nRet == 10)
                        strSytaxOID = "1.2.840.10003.5.10";

                    // 把自动识别的结果保存下来
                    record.AutoDetectedSyntaxOID = strSytaxOID;
                }

                // strOutFormat = "marc";
                goto DO_BROWSE;
            }

            // 不能识别的格式。原样放置
            strBrowseText = strData;
            return 0;

        DO_BROWSE:

            if (strSytaxOID == "1.2.840.10003.5.1"    // unimarc
                || strSytaxOID == "1.2.840.10003.5.10")  // usmarc
            {
                return BuildMarcBrowseText(
                    strSytaxOID,
                    strMARC,
                    out strBrowseText,
                    out strError);
            }

            // XML还暂时没有转换办法
            strBrowseText = strData;
            return 0;
        }



        // 创建MARC格式记录的浏览格式
        // paramters:
        //      strMARC MARC机内格式
        public int BuildMarcBrowseText(
            string strSytaxOID,
            string strMARC,
            out string strBrowseText,
            out string strError)
        {
            strBrowseText = "";
            strError = "";

            FilterHost host = new FilterHost();
            host.ID = "";
            host.MainForm = this.m_mainForm;

            BrowseFilterDocument filter = null;

            string strFilterFileName = this.m_mainForm.DataDir + "\\" + strSytaxOID.Replace(".", "_") + "\\marc_browse.fltx";

            int nRet = this.PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            try
            {
                nRet = filter.DoRecord(null,
        strMARC,
        0,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                strBrowseText = host.ResultString;

            }
            finally
            {
                // 归还对象
                this.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            return -1;
        }

        public int PrepareMarcFilter(
FilterHost host,
string strFilterFileName,
out BrowseFilterDocument filter,
out string strError)
        {
            strError = "";

            // 看看是否有现成可用的对象
            filter = (BrowseFilterDocument)this.Filters.GetFilter(strFilterFileName);

            if (filter != null)
            {
                filter.FilterHost = host;
                return 1;
            }

            // 新创建
            // string strFilterFileContent = "";

            filter = new BrowseFilterDocument();

            filter.FilterHost = host;
            filter.strOtherDef = "FilterHost Host = null;";

            filter.strPreInitial = " BrowseFilterDocument doc = (BrowseFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = ("
                + "FilterHost" + ")doc.FilterHost;\r\n";

            // filter.Load(strFilterFileName);

            try
            {
                filter.Load(strFilterFileName);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string[] saAddRef1 = {
										 this.BinDir + "\\digitalplatform.marcdom.dll",
										 this.BinDir + "\\digitalplatform.marckernel.dll",
										 this.BinDir + "\\digitalplatform.dll",
										 this.BinDir + "\\digitalplatform.Text.dll",
										 this.BinDir + "\\digitalplatform.IO.dll",
										 this.BinDir + "\\digitalplatform.Xml.dll",
										 this.BinDir + "\\dp2catalog.exe" };

            Assembly assembly = null;
            string strWarning = "";
            string strLibPaths = "";

            string[] saRef2 = filter.GetRefs();

            string[] saRef = new string[saRef2.Length + saAddRef1.Length];
            Array.Copy(saRef2, saRef, saRef2.Length);
            Array.Copy(saAddRef1, 0, saRef, saRef2.Length, saAddRef1.Length);

            // 创建Script的Assembly
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                strLibPaths,
                out assembly,
                out strError,
                out strWarning);

            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                {
                    goto ERROR1;
                }
                // MessageBox.Show(this, strWarning);
            }

            filter.Assembly = assembly;

            return 0;
        ERROR1:
            return -1;
        }

        #endregion

        private void tabControl_steps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_steps.SelectedTab == this.tabPage_saveResults)
            {
                this.button_next.Enabled = false;
            }
            else
            {
                if (stop != null && stop.State == 0)    // 0 表示正在处理
                    this.button_next.Enabled = false;
                else
                    this.button_next.Enabled = true;
            }
        }

        private void dpTable_records_DoubleClick(object sender, EventArgs e)
        {
            int nIndex = -1;

            // TODO: 如果记录为SUTRS格式，则只能装入XML详窗；
            // 如果记录为MARCXML，则两种窗口都可以装，优选装入MARC详窗

            if (this.dpTable_records.SelectedRowIndices.Count > 0)
                nIndex = this.dpTable_records.SelectedRowIndices[0];
            else
            {
                if (this.dpTable_records.FocusedItem == null)
                    return;
                nIndex = this.dpTable_records.Rows.IndexOf((DpRow)this.dpTable_records.FocusedItem);
            }

            LoadDetail(nIndex);
        }

        // 自动根据情况，装载到MARC或者XML记录窗
        void LoadDetail(int index)
        {
            DpRow row = this.dpTable_records.Rows[index];

            RecordInfo info = null;
            if (index < this.dpTable_records.Rows.Count)
            {
                info = (RecordInfo)this.dpTable_records.Rows[index].Tag;
            }
            else
            {
                MessageBox.Show(this, "index越界");
                return;
            }

            if (info.Record.m_nDiagCondition != 0)
            {
                MessageBox.Show(this, "这是一条诊断记录");
                return;
            }

            // XML格式或者SUTRS格式
            if (info.Record.m_strSyntaxOID == "1.2.840.10003.5.109.10"
                || info.Record.m_strSyntaxOID == "1.2.840.10003.5.101")
            {
                XmlDetailForm form = new XmlDetailForm();

                form.MdiParent = this.m_mainForm;
                form.MainForm = this.m_mainForm;
                form.Show();

                form.LoadRecord(this, index);
                return;
            }


            {
                MarcDetailForm form = new MarcDetailForm();


                form.MdiParent = this.m_mainForm;
                form.MainForm = this.m_mainForm;

                // 继承自动识别的OID
                if (info.DetectMarcSyntax == true)
                {
                    //form.AutoDetectedMarcSyntaxOID = info.Record.AutoDetectedSyntaxOID;
                    form.UseAutoDetectedMarcSyntaxOID = true;
                }

                form.Show();

                form.LoadRecord(this, index);
            }
        }

        #region ISearchForm 接口函数

        // 对象、窗口是否还有效?
        public bool IsValid()
        {
            if (this.IsDisposed == true)
                return false;

            return true;
        }

        public string CurrentProtocol
        {
            get
            {
                return "Z39.50";
            }
        }

        public string CurrentResultsetPath
        {
            get
            {
                /*
                ZConnection connection = this.m_currentConnection;
                if (connection == null)
                    return "";

                return connection.TargetInfo.HostName
                    + ":" + connection.TargetInfo.Port.ToString()
                    + "/" + string.Join(",", connection.TargetInfo.DbNames)
                    + "/default";
                 * */
                return "default";   // TODO: 可以用检索词数和命中数等作为名字
            }
        }

        // 刷新一条MARC记录
        // parameters:
        //      strAction   refresh / delete
        // return:
        //      -2  不支持
        //      -1  error
        //      0   相关窗口已经销毁，没有必要刷新
        //      1   已经刷新
        //      2   在结果集中没有找到要刷新的记录
        public int RefreshOneRecord(
            string strPathParam,
            string strAction,
            out string strError)
        {
            strError = "";

            //int nRet = 0;

            if (this.IsDisposed == true)
            {
                strError = "相关的Z39.50检索窗已经销毁，没有必要刷新";
                return 0;
            }

            strError = "尚未实现 RefreshOneRecord()";
            return -2;
        }

        public int SyncOneRecord(string strPath,
            ref long lVersion,
            ref string strSyntax,
            ref string strMARC,
            out string strError)
        {
            strError = "";
            return 0;
        }

        // 获得一条MARC/XML记录
        // return:
        //      -1  error
        //      0   suceed
        //      1   为诊断记录
        //      2   分割条，需要跳过这条记录
        public int GetOneRecord(
            string strStyle,
            int nTest,
            string strPathParam,
            string strParameters,   // bool bHilightBrowseLine,
            out string strSavePath,
            out string strMARC,
            out string strXmlFragment,
            out string strOutStyle,
            out byte[] baTimestamp,
            out long lVersion,
            out DigitalPlatform.Z3950.Record record,
            out Encoding currrentEncoding,
            out LoginInfo logininfo,
            out string strError)
        {
            strXmlFragment = "";
            strMARC = "";
            record = null;
            strError = "";
            currrentEncoding = null;
            baTimestamp = null;
            strSavePath = "";
            strOutStyle = "";
            logininfo = new LoginInfo();
            lVersion = 0;

            int nRet = 0;

            int index = -1;
            string strPath = "";
            string strDirection = "";
            nRet = Global.ParsePathParam(strPathParam,
                out index,
                out strPath,
                out strDirection,
                out strError);
            if (nRet == -1)
                return -1;

            if (index == -1)
            {
                strError = "暂时不支持没有 index 的用法";
                return -1;
            }

            bool bHilightBrowseLine = StringUtil.IsInList("hilight_browse_line", strParameters);
            bool bForceFullElementSet = StringUtil.IsInList("force_full", strParameters);

            if (index >= this.dpTable_records.Rows.Count)
            {
                strError = "index越界";
                return -1;
            }

            DpRow current_row = this.dpTable_records.Rows[index];
            if (current_row.Style == DpRowStyle.Seperator)
            {
                strError = "index值 "+index.ToString()+" 为分割条位置，需要跳过";
                return 2;
            }



            if (bHilightBrowseLine == true)
            {
                List<int> selected = new List<int>();
                selected.AddRange(this.dpTable_records.SelectedRowIndices);
                // 修改 listview 中事项的选定状态
                for (int i = 0; i < selected.Count; i++)
                {
                    int temp_index = selected[i];
                    if (temp_index != index)
                    {
                        if (this.dpTable_records.Rows[temp_index].Selected != false)
                            this.dpTable_records.Rows[temp_index].Selected = false;
                    }
                }

                if (current_row.Selected != true)
                    current_row.Selected = true;
                current_row.EnsureVisible();
            }

            // 
            // strSavePath = (index+1).ToString();

            RecordInfo record_info = (RecordInfo)current_row.Tag;
            // 
            record = record_info.Record;

            if (record == null)
            {
                strError = "RecordInfo.Record为空";
                return -1;
            }

            if (record.m_nDiagCondition != 0)
            {
                strError = "这是一条诊断记录";
                strOutStyle = "marc";
                strMARC = "012345678901234567890123001这是一条诊断记录";
                return 1;
            }

            if (bForceFullElementSet == true)
            {
                if (record_info.Record.m_strElementSetName == "B")
                {
                    // 如果正在检索,则不能调用
                    if (this.InSearching == true)
                    {
                        strError = "正在检索的同时无法获取详细记录，请在检索结束后再试";
                        return -1;
                    }

                    // 构造 servers
                    ServerCollection servers = new ServerCollection();
                    servers.AddRow(current_row);

                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   中断
                    nRet = DoUpdateSearch(servers,
                        out strError);
                    if (nRet != 0)
                        return -1;
                    record = record_info.Record;
                }

            }

            byte[] baRecord = record.m_baRecord;    // Encoding.ASCII.GetBytes(record.m_strRecord);

            currrentEncoding = record_info.RecordEncoding;


            // 可能为XML格式
            if (record.m_strSyntaxOID == "1.2.840.10003.5.109.10")
            {

                // string strContent = Encoding.UTF8.GetString(baRecord);
                string strContent = currrentEncoding.GetString(baRecord);

                if (strStyle == "marc")
                {
                    // 看根节点的名字空间，如果符合MARCXML, 就先转换为USMARC，否则，就直接根据名字空间找样式表加以转换

                    string strNameSpaceUri = "";
                    nRet = ZSearchForm.GetRootNamespace(strContent,
                        out strNameSpaceUri,
                        out strError);
                    if (nRet == -1)
                    {
                        return -1;
                    }

                    if (strNameSpaceUri == Ns.usmarcxml)
                    {
                        string strOutMarcSyntax = "";

                        // 将MARCXML格式的xml记录转换为marc机内格式字符串
                        // parameters:
                        //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                        //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                        //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                        nRet = MarcUtil.Xml2Marc(strContent,
                            true,
                            "usmarc",
                            out strOutMarcSyntax,
                            out strMARC,
                            out strError);
                        if (nRet == -1)
                        {
                            return -1;
                        }

                        strOutStyle = "marc";
                        // currrentEncoding = connection.GetRecordsEncoding(this.MainForm, "1.2.840.10003.5.10");
                        return 0;
                    }
                }

                // 不是MARCXML格式
                // currrentEncoding = connection.GetRecordsEncoding(this.MainForm, record.m_strMarcSyntaxOID);
                strMARC = strContent;
                strOutStyle = "xml";
                return 0;
            }

            // SUTRS
            if (record.m_strSyntaxOID == "1.2.840.10003.5.101")
            {
                string strContent = currrentEncoding.GetString(baRecord);
                if (strStyle == "marc")
                {
                    // TODO: 按照回车草率转换为MARC
                    strMARC = strContent;

                    // strMarcSyntaxOID = "1.2.840.10003.5.10";
                    strOutStyle = "marc";
                    return 0;
                }

                // 不是MARCXML格式
                strMARC = strContent;
                strOutStyle = "xml";
                return 0;
            }

            // ISO2709转换为机内格式
            nRet = Marc8Encoding.ConvertByteArrayToMarcRecord(
                baRecord,
                currrentEncoding,
                true,
                out strMARC,
                out strError);
            if (nRet < 0)
            {
                return -1;
            }

            // 观察
            // connection.TargetInfo.UnionCatalogBindingDp2ServerUrl
            // 如果配置有绑定的dp2serverurl，则看看记录中有没有901字段，
            // 如果有，返还为strSavePath和baTimestamp
            if (record_info.TargetInfo != null
                && String.IsNullOrEmpty(record_info.TargetInfo.UnionCatalogBindingDp2ServerName) == false)
            {
                string strLocalPath = "";
                // 从MARC记录中得到901字段相关信息
                // return:
                //      -1  error
                //      0   not found field 901
                //      1   found field 901
                nRet = ZSearchForm.GetField901Info(strMARC,
                    out strLocalPath,
                    out baTimestamp,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "MARC记录中未包含901字段，无法完成绑定操作。要具备901字段，请为dp2ZServer服务器的相关数据库增加addField901='true'属性。要避免此报错，也可在Z39.50服务器属性中去掉联合编目绑定定义";
                    return -1;
                }
                strSavePath = "dp2library:" + strLocalPath + "@" + record_info.TargetInfo.UnionCatalogBindingDp2ServerName;
                logininfo.UserName = record_info.TargetInfo.UserName;
                logininfo.Password = record_info.TargetInfo.Password;
            }

            if (record_info.TargetInfo != null
    && String.IsNullOrEmpty(record_info.TargetInfo.UnionCatalogBindingUcServerUrl) == false)
            {
                string strLocalPath = "";
                // 从MARC记录中得到901字段相关信息
                // return:
                //      -1  error
                //      0   not found field 901
                //      1   found field 901
                nRet = ZSearchForm.GetField901Info(strMARC,
                    out strLocalPath,
                    out baTimestamp,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "MARC记录中未包含901字段，无法完成绑定操作。要具备901字段，请为dp2ZServer服务器的相关数据库增加addField901='true'属性。要避免此报错，也可在Z39.50服务器属性中去掉联合编目绑定定义";
                    return -1;
                }
                strSavePath = "unioncatalog:" + strLocalPath + "@" + record_info.TargetInfo.UnionCatalogBindingUcServerUrl;
                logininfo.UserName = record_info.TargetInfo.UserName;
                logininfo.Password = record_info.TargetInfo.Password;
            }


            strOutStyle = "marc";
            return 0;
        }

        #endregion

        private void dpTable_queryWords_DoubleClick(object sender, EventArgs e)
        {
            int nIndex = -1;

            if (this.dpTable_queryWords.SelectedRowIndices.Count > 0)
                nIndex = this.dpTable_queryWords.SelectedRowIndices[0];
            else
            {
                if (this.dpTable_queryWords.FocusedItem == null)
                    return;
                nIndex = this.dpTable_queryWords.Rows.IndexOf((DpRow)this.dpTable_queryWords.FocusedItem);
            }

            DpRow row = this.dpTable_queryWords.Rows[nIndex];
            QueryLine line = (QueryLine)row.Tag;
            if (line.ResultRows != null
                && line.ResultRows.Count > 0)
            {
                List<int> selected = new List<int>();
                selected.AddRange(this.dpTable_records.SelectedRowIndices);
                for (int i = 0; i < selected.Count; i++)
                {
                    int temp_index = selected[i];

                    if (this.dpTable_records.Rows[temp_index].Selected != false)
                        this.dpTable_records.Rows[temp_index].Selected = false;

                }
                foreach (DpRow current_row in line.ResultRows)
                {
                    current_row.Selected = true;
                }

                if (line.ResultRows.Count > 1)
                    line.ResultRows[line.ResultRows.Count - 1].EnsureVisible();

                line.ResultRows[0].EnsureVisible();
            }
        }

        private void button_saveResult_findSingleHitFilename_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的 '命中唯一的' MARC文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.textBox_saveResult_singleHitFilename.Text;
            dlg.Filter = "MARC(ISO2709)文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_saveResult_singleHitFilename.Text = dlg.FileName;
        }

        private void button_saveResult_saveSingleHitFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strFilename = this.textBox_saveResult_singleHitFilename.Text;

            // parameters:
            //      strStyle    singlehit multihit all selected
            int nRet = SaveMarcFile("singlehit",
                ref strFilename,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_saveResult_singleHitFilename.Text = strFilename;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // parameters:
        //      strStyle    singlehit multihit all selected
        int SaveMarcFile(string strStyle,
            ref string strFilename,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            int nCount = 0;
            bool bAppend = false;

            // bool bForceFull = false;

            List<DpRow> rows = GetRecordRows(strStyle);

            List<DpRow> brief_rows = this.GetBriefRows(rows);

            if (brief_rows.Count > 0)
            {
                DialogResult result = MessageBox.Show(this,
"即将保存的记录中有 "+brief_rows.Count.ToString()+" 个 Brief(简要)格式的记录，是否在保存前重新获取为 Full(完整) 格式的记录?\r\n\r\n(Yes: 是，要完整格式的记录; No: 否，依然保存简明格式的记录； Cancel: 取消，放弃整个保存操作",
"ZBatchSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return 0;
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // bForceFull = true;
                    // 构造 servers
                    ServerCollection servers = new ServerCollection();
                    foreach (DpRow row in brief_rows)
                    {
                        servers.AddRow(row);
                    }
                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   中断
                    nRet = DoUpdateSearch(servers,
                        out strError);
                    if (nRet != 0)
                        return -1;
                }
            }
#if NO
            if (this.InSearching == true)
            {
                strError = "当前有检索操作正在进行，无法进行保存到作，请稍候再试";
                return -1;
            } 
#endif
            this.EnableControlsInSearching(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存记录 ...");
            stop.BeginLoop();

            try
            {
#if NO
                List<DpRow> rows = new List<DpRow>();

                // 准备数据记录行集合
                if (strStyle == "selected")
                {
                    rows.AddRange(this.dpTable_records.SelectedRows);
                }
                else
                {
                    for (int i = 0; i < this.dpTable_queryWords.Rows.Count; i++)
                    {
                        DpRow word_row = this.dpTable_queryWords.Rows[i];
                        QueryLine line = (QueryLine)word_row.Tag;
                        if (strStyle == "singlehit")
                        {
                            if (line.ResultRows != null
                                && line.ResultRows.Count == 1)
                            {
                                rows.Add(line.ResultRows[0]);
                            }
                        }
                        else if (strStyle == "multihit")
                        {
                            if (line.ResultRows != null
                                && line.ResultRows.Count > 1)
                                rows.AddRange(line.ResultRows);
                        }
                        else if (strStyle == "all")
                        {
                            if (line.ResultRows != null
                                && line.ResultRows.Count > 0)
                                rows.AddRange(line.ResultRows);
                        }
                    }
                }

#endif

                // 获得首选的编码方式
                Encoding preferredEncoding = null;

                if (rows.Count > 0)
                {
                    // 观察要保存的第一条记录
                    DpRow record_row = rows[0];
                    RecordInfo record_info = (RecordInfo)record_row.Tag;
                    preferredEncoding = record_info.RecordEncoding;
                    Debug.Assert(preferredEncoding != null, "");
                }
                else
                {
                    if (strStyle == "singlehit")
                        strError = "当前没有要保存的 '命中唯一' 的记录";
                    else if (strStyle == "multihit")
                        strError = "当前没有要保存的 '命中多条' 的记录";
                    else if (strStyle == "selected")
                        strError = "当前尚未选定要保存的记录";
                    else if (strStyle == "all")
                        strError = "当前没有任何记录可保存";
                    return -1;
                }

                OpenMarcFileDlg dlg = new OpenMarcFileDlg();
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.GetEncoding -= new GetEncodingEventHandler(dlg_GetEncoding);
                dlg.GetEncoding += new GetEncodingEventHandler(dlg_GetEncoding);
                dlg.IsOutput = true;
                dlg.FileName = strFilename;
                dlg.CrLf = m_mainForm.LastCrLfIso2709;
                dlg.RemoveField998Visible = false;
                dlg.Mode880Visible = false; // 暂时不支持 880 模式转换
                dlg.EncodingListItems = Global.GetEncodingList(true);
                dlg.EncodingName = GetEncodingForm.GetEncodingName(preferredEncoding);
                dlg.EncodingComment = "注: 原始编码方式为 " + GetEncodingForm.GetEncodingName(preferredEncoding);
                dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
                dlg.EnableMarcSyntax = false;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                    return 0;

                Encoding targetEncoding = null;

                nRet = this.m_mainForm.GetEncoding(dlg.EncodingName,
                    out targetEncoding,
                    out strError);
                if (nRet == -1)
                {
                    return -1;
                }

                strFilename = dlg.FileName;

                bool bExist = File.Exists(dlg.FileName);

                if (bExist == true)
                {
                    DialogResult result = MessageBox.Show(this,
            "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
            "ZBatchSearchForm",
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
                        return -1;
                    }
                }
                Stream s = null;

                try
                {
                    s = File.Open(dlg.FileName,
                         FileMode.OpenOrCreate);
                    if (bAppend == false)
                        s.SetLength(0);
                    else
                        s.Seek(0, SeekOrigin.End);
                }
                catch (Exception ex)
                {
                    strError = "打开或创建文件 " + dlg.FileName + " 失败，原因: " + ex.Message;
                    return -1;
                }

                try
                {
                    stop.SetProgressRange(0, rows.Count);
                    for (int i = 0; i < rows.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

#if NO
                        if (bForceFull == true)
                        {
                            int index = this.dpTable_records.SelectedRowIndices[i];

                            byte[] baTimestamp = null;
                            string strSavePath = "";
                            string strOutStyle = "";
                            LoginInfo logininfo = null;
                            long lVersion = 0;
                            string strXmlFragment = "";
                            DigitalPlatform.Z3950.Record record = null;
                            Encoding currentEncoding = null;
                            string strMARC = "";

                            nRet = this.GetOneRecord(
                                "marc",
                                index,  // 即将废止
                                "index:" + index.ToString(),
                                bForceFull == true ? "force_full" : "", // false,
                                out strSavePath,
                                out strMARC,
                                out strXmlFragment,
                                out strOutStyle,
                                out baTimestamp,
                                out lVersion,
                                out record,
                                out currentEncoding,
                                out logininfo,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
#endif

                        DpRow record_row = rows[i];
                        RecordInfo record_info = (RecordInfo)record_row.Tag;
                        string strRecordName = record_row[0].Text;

                        byte[] baTarget = null;

                        Encoding sourceEncoding = record_info.RecordEncoding;

                        string strMarcSyntax = "";
                        if (record_info.Record.m_strSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        if (record_info.Record.m_strSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";

                        if (sourceEncoding.Equals(targetEncoding) == true)
                        {
                            // source和target编码方式相同，不用转换
                            // baTarget = record_info.Record.m_baRecord;

                            // 规范化 ISO2709 物理记录
                            // 主要是检查里面的记录结束符是否正确，去掉多余的记录结束符
                            baTarget = MarcUtil.CononicalizeIso2709Bytes(targetEncoding,
                                record_info.Record.m_baRecord);
                        }
                        else
                        {
                            // 转换为MARC-8需要特殊条件
                            if (dlg.EncodingName == "MARC-8"
        && sourceEncoding.Equals(this.m_mainForm.Marc8Encoding) == false)
                            {
                                strError = "记录 " + strRecordName + " 的原始编码方式 " + sourceEncoding.EncodingName + " 无法转换为MARC-8。只有在记录的原始编码方式为 MARC-8 时，才能使用这个编码方式保存记录。";
                                DialogResult result = MessageBox.Show(this,
    strError + "\r\n\r\n是否跳过这一条记录继续保存其他记录？\r\n\r\nYes 跳过并继续；No 中断保存操作",
    "ZBatchSearchForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                                if (result == System.Windows.Forms.DialogResult.No)
                                    return -1;
                                continue;
                            }

                            nRet = ZSearchForm.ChangeIso2709Encoding(
                                sourceEncoding,
                                record_info.Record.m_baRecord,
                                targetEncoding,
                                strMarcSyntax,
                                out baTarget,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                        s.Write(baTarget, 0,
                            baTarget.Length);

                        if (dlg.CrLf == true)
                        {
                            byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                            s.Write(baCrLf, 0,
                                baCrLf.Length);
                        }

                        nCount++;
                        stop.SetProgressValue(nCount);
                    }
                }
                catch (Exception ex)
                {
                    strError = "写入文件 " + dlg.FileName + " 失败，原因: " + ExceptionUtil.GetDebugText(ex);
                    return -1;
                }
                finally
                {
                    s.Close();
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControlsInSearching(true);
            }
            
            // 
            if (bAppend == true)
                m_mainForm.MessageText = nCount.ToString()
                    + "条记录成功追加到文件 " + strFilename + " 尾部";
            else
                m_mainForm.MessageText = nCount.ToString()
                    + "条记录成功保存到新文件 " + strFilename + " 尾部";

            return 0;
        }

        void dlg_GetEncoding(object sender, GetEncodingEventArgs e)
        {
            string strError = "";
            Encoding encoding = null;
            int nRet = this.m_mainForm.GetEncoding(e.EncodingName,
                out encoding,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }
            e.Encoding = encoding;
        }

        private void button_saveResult_findMultiHitFilename_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的 '命中多条的' MARC文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.textBox_saveResult_multiHitFilename.Text;
            dlg.Filter = "MARC(ISO2709)文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_saveResult_multiHitFilename.Text = dlg.FileName;

        }

        private void button_saveResult_saveMultiHitFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strFilename = this.textBox_saveResult_multiHitFilename.Text;

            // parameters:
            //      strStyle    singlehit multihit all selected
            int nRet = SaveMarcFile("multihit",
                ref strFilename,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_saveResult_multiHitFilename.Text = strFilename;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_saveResult_findNotHitFilename_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的 '未命中的检索词' 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.textBox_saveResult_notHitFilename.Text;
            dlg.Filter = "检索词文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_saveResult_notHitFilename.Text = dlg.FileName;
        }

        private void button_saveResult_saveNotHitFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            bool bAppend = false;
            int nCount = 0;

            this.EnableControlsInSearching(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存未命中的检索词 ...");
            stop.BeginLoop();

            try
            {
                List<string> words = new List<string>();
                for (int i = 0; i < this.dpTable_queryWords.Rows.Count; i++)
                {
                    DpRow word_row = this.dpTable_queryWords.Rows[i];
                    QueryLine line = (QueryLine)word_row.Tag;
                    if (line == null || line.ResultRows == null
                        || line.ResultRows.Count == 0)
                    {
                        words.Add(line.Word);
                    }
                }

                if (words.Count == 0)
                {
                    strError = "当前并不存在没有命中结果的检索词";
                    goto ERROR1;
                }

                nCount = words.Count;

                if (string.IsNullOrEmpty(this.textBox_saveResult_notHitFilename.Text) == true)
                {
                    // 询问文件名
                    SaveFileDialog dlg = new SaveFileDialog();

                    dlg.Title = "请指定要保存的 '未命中的检索词' 文件名";
                    dlg.CreatePrompt = false;
                    dlg.OverwritePrompt = false;
                    dlg.FileName = this.textBox_saveResult_notHitFilename.Text;
                    dlg.Filter = "检索词文件 (*.txt)|*.txt|All files (*.*)|*.*";

                    dlg.RestoreDirectory = true;

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;

                    this.textBox_saveResult_notHitFilename.Text = dlg.FileName;
                }

                bool bExist = File.Exists(this.textBox_saveResult_notHitFilename.Text);
                if (bExist == true)
                {
                    DialogResult result = MessageBox.Show(this,
            "文件 '" + this.textBox_saveResult_notHitFilename.Text + "' 已存在，是否以追加方式写入内容?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
            "ZBatchSearchForm",
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

                try
                {
                    using (StreamWriter sw = new StreamWriter(this.textBox_saveResult_notHitFilename.Text,
                        bAppend, Encoding.UTF8))
                    {
                        foreach (string word in words)
                        {
                            sw.WriteLine(word);
                        }
                    }
                }
                catch (Exception ex)
                {
                    strError = "写入文件 '" + this.textBox_saveResult_notHitFilename .Text+ "' 时出错: " + ex.Message;
                    goto ERROR1;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControlsInSearching(true);
            }

            // 
            if (bAppend == true)
                m_mainForm.MessageText = nCount.ToString()
                    + "条记录成功追加到文件 " + this.textBox_saveResult_notHitFilename.Text + " 尾部";
            else
                m_mainForm.MessageText = nCount.ToString()
                    + "条记录成功保存到新文件 " + this.textBox_saveResult_notHitFilename.Text + " 尾部";


            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void ZBatchSearchForm_Activated(object sender, EventArgs e)
        {
            if (stop != null)
                MainForm.stopManager.Active(this.stop);

            MainForm.SetMenuItemState();

            // 菜单
            if (this.dpTable_records.SelectedRows.Count == 0)
            {
                MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = false;
                MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = false;
            }
            else
            {
                MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = true;
                MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = true;
            }

            MainForm.MenuItem_font.Enabled = false;

            // 工具条按钮
            if (this.dpTable_records.SelectedRows.Count == 0)
            {
                MainForm.toolButton_saveTo.Enabled = false;
                MainForm.toolButton_delete.Enabled = false;
            }
            else
            {
                MainForm.toolButton_saveTo.Enabled = true;
                MainForm.toolButton_delete.Enabled = true;
            }

            MainForm.toolButton_refresh.Enabled = true;
            MainForm.toolButton_loadFullRecord.Enabled = false;
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2catalog 
发送者 xxx
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.InvalidOperationException
Message: 文件 C:\Documents and Settings\Administrator\桌面\ 是无效的文件名。
Stack:
在 System.Windows.Forms.OpenFileDialog.RunFileDialog(OPENFILENAME_I ofn)
在 System.Windows.Forms.FileDialog.RunDialogOld(IntPtr hWndOwner)
在 System.Windows.Forms.FileDialog.RunDialog(IntPtr hWndOwner)
在 System.Windows.Forms.CommonDialog.ShowDialog(IWin32Window owner)
在 System.Windows.Forms.CommonDialog.ShowDialog()
在 dp2Catalog.ZBatchSearchForm.button_queryLines_load_Click(Object sender, EventArgs e)
在 System.Windows.Forms.Control.OnClick(EventArgs e)
在 System.Windows.Forms.Button.OnClick(EventArgs e)
在 System.Windows.Forms.Button.OnMouseUp(MouseEventArgs mevent)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ButtonBase.WndProc(Message& m)
在 System.Windows.Forms.Button.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Catalog 版本: dp2Catalog, Version=2.4.5724.41026, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3 
操作时间 2015/9/10 7:46:22 (Thu, 10 Sep 2015 07:46:22 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 
 
         * */
        private void button_queryLines_load_Click(object sender, EventArgs e)
        {
            bool bRedo = false;

        REDO:
            this.m_bExceed = false;
            if (string.IsNullOrEmpty(this.textBox_queryLines_filename.Text) == true
                || bRedo == true)
            {
                OpenFileDialog dlg = new OpenFileDialog();

                dlg.Title = "请指定要打开的检索词文件名";
                dlg.FileName = this.textBox_queryLines_filename.Text;
                dlg.Filter = "检索词文件 (*.txt)|*.txt|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;

                try
                {
                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(this, ex.Message);
                    return;
                }

                this.textBox_queryLines_filename.Text = dlg.FileName;
            }

            string strError = "";
            string strContent = "";
            Encoding encoding = null;
            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // parameters:
            //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
            // return:
            //      -1  出错
            //      0   文件不存在
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(this.textBox_queryLines_filename.Text,
                100 * 1024, // 100K
                out strContent,
                out encoding,
                out strError);
            if (nRet == 1 || nRet == 2)
            {
                bool bExceed = nRet == 2;

                this.textBox_queryLines_content.Text =
                    (bExceed == true ? "文件尺寸太大，下面只显示了开头部分...\r\n" : "") + strContent;

                this.m_bExceed = bExceed;
            }
            else
                this.textBox_queryLines_content.Text = "";

            if (nRet == -1 || nRet == 0)
            {
                MessageBox.Show(this, strError);
                bRedo = true;
                goto REDO;
            }
        }

        private void zTargetControl1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            UpdateSelectedServerCount();

            if (ZTargetControl.IsServerType(e.Node.Parent) == true)
                RefreshTargetInfo(e.Node.Parent);
        }

        private void zTargetControl1_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateSelectedServerCount();
        }

        // 刷新缓存的TargetInfo
        void RefreshTargetInfo(TreeNode node)
        {
            if (node == null)
                return;

            ZConnection connection = this.ZConnections.FindZConnection(node);
            if (connection != null)
            {
                connection.TargetInfo = null;
            }
        }

        // 统计和选中的服务器个数
        void UpdateSelectedServerCount()
        {
            int nCount = 0;

            if (this.zTargetControl1.CheckBoxes == false)
            {
                nCount = (this.zTargetControl1.SelectedNode != null ? 1 : 0);
            }
            else
            {
                nCount = this.zTargetControl1.GetCheckedServerCount();
            }
            string strText = "检索目标";
            if (nCount > 0)
                strText += "(" + nCount.ToString() + ")";

            if (this.tabPage_target.Text != strText)
                this.tabPage_target.Text = strText;

        }

        private void dpTable_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            ToolStripSeparator sep = null;


            int nSelectedCount = this.dpTable_records.SelectedRows.Count;

            RecordInfo record_info = null;
            DigitalPlatform.Z3950.Record record = null;
            if (nSelectedCount > 0)
            {
                DpRow row = this.dpTable_records.SelectedRows[0];
                record_info = (RecordInfo)row.Tag;
                record = record_info.Record;
            }

            // 装入MARC记录窗
            menuItem = new ToolStripMenuItem("装入 MARC 记录窗(&M)");
            menuItem.Click += new EventHandler(dpTable_records_DoubleClick);
            if (record != null
                && (record.m_strSyntaxOID == "1.2.840.10003.5.1"
                || record.m_strSyntaxOID == "1.2.840.10003.5.10")
                )
            {
                menuItem.Enabled = true;
            }
            else if (record != null
                && record.m_strSyntaxOID == "1.2.840.10003.5.109.10")
            {
                // 还要细判断名字空间
                string strNameSpaceUri = "";
                string strContent = Encoding.UTF8.GetString(record.m_baRecord);
                string strError = "";
                int nRet = ZSearchForm.GetRootNamespace(strContent,
                    out strNameSpaceUri,
                    out strError);
                if (nRet != -1 && strNameSpaceUri == Ns.usmarcxml)
                    menuItem.Enabled = true;
                else
                    menuItem.Enabled = false;
            }
            else
                menuItem.Enabled = false;

            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 全选
            menuItem = new ToolStripMenuItem("全选(&A)");
            menuItem.Click += new EventHandler(menuItem_selectAll_Click);
            contextMenu.Items.Add(menuItem);

            // 选定命中唯一的
            menuItem = new ToolStripMenuItem("选定命中唯一的(&S)");
            menuItem.Click += new EventHandler(menuItem_selectSingleHit_Click);
            contextMenu.Items.Add(menuItem);

            // 选定命中多条的
            menuItem = new ToolStripMenuItem("选定命中多条的(&M)");
            menuItem.Click += new EventHandler(menuItem_selectMultiHit_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 装入Full元素集记录
            menuItem = new ToolStripMenuItem("重新装入完整格式记录 [" + nSelectedCount.ToString() + "] (&F)...");
            menuItem.Click += new System.EventHandler(this.menu_reloadFullElementSet_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

#if NO
            // 测试
            menuItem = new ToolStripMenuItem("测试 [" + nSelectedCount.ToString() + "] (&F)...");
            menuItem.Click += new System.EventHandler(this.menu_test_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);
#endif

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);


            // 追加保存到数据库
            menuItem = new ToolStripMenuItem("将选定的 " + nSelectedCount.ToString() + " 条记录以追加方式保存到数据库(&A)...");
            menuItem.Click += new System.EventHandler(this.menu_saveToDatabase_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 保存原始记录到ISO2709文件
            menuItem = new ToolStripMenuItem("保存选定的 "
                + nSelectedCount.ToString()
                + " 条记录到MARC文件(&S)");
            if (record != null
                && (record.m_strSyntaxOID == "1.2.840.10003.5.1"
                || record.m_strSyntaxOID == "1.2.840.10003.5.10"))
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_saveOriginRecordToIso2709_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Show(this.dpTable_records, e.Location);
        }

        void menu_test_Click(object sender, EventArgs e)
        {
            List<DpRow> rows = this.GetSelectedRecordRows();

            string strResult = "";
            foreach (DpRow row in rows)
            {
                RecordInfo info = (RecordInfo)row.Tag;
                strResult += info.Dump() + "\r\n";
            }

            MessageBox.Show(this, strResult);
        }

        // 获得当前选定的记录行
        // 排除了分隔行
        List<DpRow> GetSelectedRecordRows()
        {
            List<DpRow> rows = new List<DpRow>();
            foreach (DpRow row in this.dpTable_records.SelectedRows)
            {
                if (row.Style == DpRowStyle.Seperator)
                    continue;
                rows.Add(row);
            }

            return rows;
        }

        // parameters:
        //      rows    源集合。如果为 null，表示希望活当前选定的记录行中的 Brief 行
        List<DpRow> GetBriefRows(List<DpRow> source_rows = null)
        {
            List<DpRow> results = new List<DpRow>();

            if (source_rows == null)
                source_rows = this.GetSelectedRecordRows();

            for (int i = 0; i < source_rows.Count; i++)
            {
                DpRow row = source_rows[i];
                RecordInfo record_info = (RecordInfo)row.Tag;

                DigitalPlatform.Z3950.Record record = record_info.Record;
                if (record == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (record.m_strElementSetName == "B")
                    results.Add(row);
            }

            return results;
        }

#if NO
        // 选定的行中是否包含了Brief格式的记录
        // parameters:
        //      strStyle    selected 
        bool HasSelectionContainBriefRecords(List<DpRow> rows = null)
        {
            if (rows == null)
                rows = this.dpTable_records.SelectedRows;

            for (int i = 0; i < rows.Count; i++)
            {
                DpRow row = rows[i];
                RecordInfo record_info = (RecordInfo)row.Tag;

                DigitalPlatform.Z3950.Record record = record_info.Record;
                if (record == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (record.m_strElementSetName == "B")
                    return true;
            }

            return false;
        }
#endif


        // 追加保存到数据库
        void menu_saveToDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            string strLastSavePath = m_mainForm.LastSavePath;
            if (String.IsNullOrEmpty(strLastSavePath) == false)
            {
                string strOutputPath = "";
                nRet = MarcDetailForm.ChangePathToAppendStyle(strLastSavePath,
                    out strOutputPath,
                    out strError);
                if (nRet == -1)
                {
                    m_mainForm.LastSavePath = ""; // 避免下次继续出错
                    goto ERROR1;
                }
                strLastSavePath = strOutputPath;
            }


            SaveRecordDlg dlg = new SaveRecordDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.SaveToDbMode = true;    // 不允许在textbox中修改路径

            dlg.MainForm = this.m_mainForm;
            dlg.GetDtlpSearchParam += new GetDtlpSearchParamEventHandle(dlg_GetDtlpSearchParam);
            dlg.GetDp2SearchParam += new GetDp2SearchParamEventHandle(dlg_GetDp2SearchParam);
            {
                dlg.RecPath = strLastSavePath;
                dlg.Text = "请选择目标数据库";
            }
            // dlg.StartPosition = FormStartPosition.CenterScreen;
            this.MainForm.AppInfo.LinkFormState(dlg, "SaveRecordDlg_state");
            dlg.UiState = this.MainForm.AppInfo.GetString("ZBatchSearchForm", "SaveRecordDlg_uiState", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString("ZBatchSearchForm", "SaveRecordDlg_uiState", dlg.UiState);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            m_mainForm.LastSavePath = dlg.RecPath;

            string strProtocol = "";
            string strPath = "";
            nRet = Global.ParsePath(dlg.RecPath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.dpTable_records.SelectedRows.Count == 0)
            {
                strError = "尚未选定要保存记录的浏览行";
                goto ERROR1;
            }

            bool bForceFull = false;

            List<DpRow> brief_rows = this.GetBriefRows();

            if (brief_rows.Count > 0)
            {
                DialogResult result = MessageBox.Show(this,
"即将保存的记录中有 " + brief_rows.Count.ToString() + " 个 Brief(简要)格式的记录，是否在保存前重新获取为 Full(完整) 格式的记录?\r\n\r\n(Yes: 是，要完整格式的记录; No: 否，依然保存简明格式的记录； Cancel: 取消，放弃整个保存操作",
"ZBatchSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // bForceFull = true;
                    // 构造 servers
                    ServerCollection servers = new ServerCollection();
                    foreach (DpRow row in brief_rows)
                    {
                        servers.AddRow(row);
                    }
                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   中断
                    nRet = DoUpdateSearch(servers,
                        out strError);
                    if (nRet != 0)
                        goto ERROR1;
                }
            }

            // TODO: 禁止问号以外的其它ID
            DigitalPlatform.Stop stop = null;
            stop = new DigitalPlatform.Stop();
            stop.Register(m_mainForm.stopManager, true);	// 和容器关联

            stop.BeginLoop();

            this.EnableControlsInSearching(false);
            try
            {

                // dtlp协议的记录保存
                if (strProtocol.ToLower() == "dtlp")
                {
                    DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

                    if (dtlp_searchform == null)
                    {
                        strError = "没有连接的或者打开的DTLP检索窗，无法保存记录";
                        goto ERROR1;
                    }

                    for (int i = 0; i < this.dpTable_records.SelectedRowIndices.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        int index = this.dpTable_records.SelectedRowIndices[i];

                        byte[] baTimestamp = null;
                        string strSavePath = "";
                        string strOutStyle = "";
                        LoginInfo logininfo = null;
                        long lVersion = 0;
                        string strXmlFragment = "";
                        DigitalPlatform.Z3950.Record record = null;
                        Encoding currentEncoding = null;
                        string strMARC = "";

                        nRet = this.GetOneRecord(
                            "marc",
                            index,  // 即将废止
                            "index:" + index.ToString(),
                            bForceFull == true ? "force_full" : "", // false,
                            out strSavePath,
                            out strMARC,
                            out strXmlFragment,
                            out strOutStyle,
                            out baTimestamp,
                            out lVersion,
                            out record,
                            out currentEncoding,
                            out logininfo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        string strMarcSyntax = "";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";

                        // TODO: 有些格式不适合保存到目标数据库

                        byte[] baOutputTimestamp = null;
                        string strOutputPath = "";
                        nRet = dtlp_searchform.SaveMarcRecord(
                            strPath,
                            strMARC,
                            baTimestamp,
                            out strOutputPath,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    MessageBox.Show(this, "保存成功");
                    return;
                }
                else if (strProtocol.ToLower() == "dp2library")
                {
                    dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法保存记录";
                        goto ERROR1;
                    }

                    string strDp2ServerName = "";
                    string strPurePath = "";
                    // 解析记录路径。
                    // 记录路径为如下形态 "中文图书/1 @服务器"
                    dp2SearchForm.ParseRecPath(strPath,
                        out strDp2ServerName,
                        out strPurePath);

                    string strTargetMarcSyntax = "";

                    try
                    {
                        NormalDbProperty prop = dp2_searchform.GetDbProperty(strDp2ServerName,
             dp2SearchForm.GetDbName(strPurePath));
                        strTargetMarcSyntax = prop.Syntax;
                        if (string.IsNullOrEmpty(strTargetMarcSyntax) == true)
                            strTargetMarcSyntax = "unimarc";
                    }
                    catch (Exception ex)
                    {
                        strError = "在获得目标库特性时出错: " + ex.Message;
                        goto ERROR1;
                    }

                    bool bSkip = false;
                    int nSavedCount = 0;

                    for (int i = 0; i < this.dpTable_records.SelectedRowIndices.Count; i++)
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

                        int index = this.dpTable_records.SelectedRowIndices[i];

                        byte[] baTimestamp = null;
                        string strSavePath = "";
                        string strOutStyle = "";
                        LoginInfo logininfo = null;
                        long lVersion = 0;
                        string strXmlFragment = "";
                        DigitalPlatform.Z3950.Record record = null;
                        Encoding currentEncoding = null;
                        string strMARC = "";

                        nRet = this.GetOneRecord(
                            "marc",
                            index,  // 即将废止
                            "index:" + index.ToString(),
                            bForceFull == true ? "force_full" : "", // false,
                            out strSavePath,
                            out strMARC,
                            out strXmlFragment,
                            out strOutStyle,
                            out baTimestamp,
                            out lVersion,
                            out record,
                            out currentEncoding,
                            out logininfo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

#if NO
                        string strMarcSyntax = "";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";
#endif
                        string strMarcSyntax = MarcDetailForm.GetMarcSyntax(record.m_strSyntaxOID);

                        // 有些格式不适合保存到目标数据库
                        if (strTargetMarcSyntax != strMarcSyntax)
                        {
                            if (bSkip == true)
                                continue;
                            strError = "记录 " + (index + 1).ToString() + " 的格式类型为 '" + strMarcSyntax + "'，和目标库的格式类型 '" + strTargetMarcSyntax + "' 不符合，因此无法保存到目标库";
                            DialogResult result = MessageBox.Show(this,
        strError + "\r\n\r\n要跳过这些记录而继续保存后面的记录么?\r\n\r\n(Yes: 跳过格式不吻合的记录，继续保存后面的; No: 放弃整个保存操作)",
        "ZBatchSearchForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                goto ERROR1;
                            bSkip = true;
                            continue;
                        }

                        string strOutputPath = "";
                        byte[] baOutputTimestamp = null;
                        string strComment = "copy from " + strSavePath;
                        // return:
                        //      -2  timestamp mismatch
                        //      -1  error
                        //      0   succeed
                        nRet = dp2_searchform.SaveMarcRecord(
                            false,
                            strPath,
                            strMARC,
                            strMarcSyntax,
                            baTimestamp,
                            strXmlFragment,
                            strComment,
                            out strOutputPath,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        nSavedCount++;

                    }
                    MessageBox.Show(this, "共保存记录 " + nSavedCount.ToString() + " 条");
                    return;
                }
                else if (strProtocol.ToLower() == "z3950")
                {
                    strError = "目前暂不支持Z39.50协议的保存操作";
                    goto ERROR1;
                }
                else
                {
                    strError = "无法识别的协议名 '" + strProtocol + "'";
                    goto ERROR1;
                }
            }
            finally
            {
                stop.EndLoop();

                stop.Unregister();	// 和容器关联
                stop = null;

                this.EnableControlsInSearching(true);
            }

            // return 0;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        DtlpSearchForm GetDtlpSearchForm()
        {
            DtlpSearchForm dtlp_searchform = null;

            dtlp_searchform = this.m_mainForm.TopDtlpSearchForm;

            if (dtlp_searchform == null)
            {
                // 新开一个dtlp检索窗
                FormWindowState old_state = this.WindowState;

                dtlp_searchform = new DtlpSearchForm();
                dtlp_searchform.MainForm = this.m_mainForm;
                dtlp_searchform.MdiParent = this.m_mainForm;
                dtlp_searchform.WindowState = FormWindowState.Minimized;
                dtlp_searchform.Show();

                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                dtlp_searchform.WaitLoadFinish();
            }

            return dtlp_searchform;
        }

        dp2SearchForm GetDp2SearchForm()
        {
            dp2SearchForm dp2_searchform = null;


            dp2_searchform = this.m_mainForm.TopDp2SearchForm;

            if (dp2_searchform == null)
            {
                // 新开一个dp2检索窗
                FormWindowState old_state = this.WindowState;

                dp2_searchform = new dp2SearchForm();
                dp2_searchform.MainForm = this.m_mainForm;
                dp2_searchform.MdiParent = this.m_mainForm;
                dp2_searchform.WindowState = FormWindowState.Minimized;
                dp2_searchform.Show();

                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                dp2_searchform.WaitLoadFinish();
            }

            return dp2_searchform;
        }

        void dlg_GetDp2SearchParam(object sender, GetDp2SearchParamEventArgs e)
        {
            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

            e.dp2Channels = dp2_searchform.Channels;
            e.MainForm = this.m_mainForm;
        }

        void dlg_GetDtlpSearchParam(object sender, GetDtlpSearchParamEventArgs e)
        {
            DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

            e.DtlpChannels = dtlp_searchform.DtlpChannels;
            e.DtlpChannel = dtlp_searchform.DtlpChannel;
        }

        void menuItem_selectAll_Click(object sender,
    EventArgs e)
        {
            for (int i = 0; i < this.dpTable_records.Rows.Count; i++)
            {
                DpRow row = this.dpTable_records.Rows[i];
                if (row.Style == DpRowStyle.None)
                    row.Selected = true;
            }
        }

        void menuItem_selectSingleHit_Click(object sender,
EventArgs e)
        {
            List<DpRow> rows = this.GetRecordRows("singlehit");

            foreach (DpRow row in this.dpTable_records.Rows)
            {
                if (rows.IndexOf(row) != -1)
                    row.Selected = true;
                else
                    row.Selected = false;
            }

            if (rows.Count == 0)
                MessageBox.Show(this, "当前没有任何命中唯一的事项");
        }

        void menuItem_selectMultiHit_Click(object sender,
EventArgs e)
        {
            List<DpRow> rows = this.GetRecordRows("multihit");

            foreach (DpRow row in this.dpTable_records.Rows)
            {
                if (rows.IndexOf(row) != -1)
                    row.Selected = true;
                else
                    row.Selected = false;
            }
            if (rows.Count == 0)
                MessageBox.Show(this, "当前没有任何命中多条的事项");
        }

        // 用过的 保存选择的行到MARC文件 文件名
        string m_strSelectedSaveMarcFilename = "";

        void menuItem_saveOriginRecordToIso2709_Click(object sender, EventArgs e)
        {
            string strError = "";

            // parameters:
            //      strStyle    singlehit multihit all selected
            int nRet = SaveMarcFile("selected",
                ref m_strSelectedSaveMarcFilename,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        List<DpRow> GetQueryWordRows(string strStyle)
        {
            List<DpRow> results = new List<DpRow>();
            foreach (DpRow word_row in this.dpTable_queryWords.Rows)
            {
                if (word_row.Style == DpRowStyle.Seperator)
                {
                    // 检索词没有分隔行吧
                    continue;
                }

                QueryLine line = (QueryLine)word_row.Tag;
                if (strStyle == "notfound")
                {
                    if (line.ResultRows != null
                        && line.ResultRows.Count == 0)
                        results.Add(word_row);
                } 
                else if (strStyle == "singlehit")
                {
                    if (line.ResultRows != null
                        && line.ResultRows.Count == 1)
                        results.Add(word_row);
                }
                else if (strStyle == "multihit")
                {
                    if (line.ResultRows != null
                        && line.ResultRows.Count > 1)
                        results.Add(word_row);
                }
                else if (strStyle == "all")
                {
                    if (line.ResultRows != null
                        && line.ResultRows.Count > 0)
                        results.Add(word_row);
                }
                else throw new Exception("无法识别的 strStyle 值 '"+strStyle+"'");
            }

            return results;
        }

        // 获得指定范围的浏览行
        List<DpRow> GetRecordRows(string strStyle)
        {
            List<DpRow> rows = new List<DpRow>();

            // 准备数据记录行集合
            if (strStyle == "selected")
            {
#if NO
                foreach (DpRow row in this.dpTable_records.SelectedRows)
                {
                    if (row.Style == DpRowStyle.Seperator)
                        continue;
                    rows.Add(row);
                }
#endif
                rows = GetSelectedRecordRows();

                // rows.AddRange(this.dpTable_records.SelectedRows);

            }
            else
            {
                for (int i = 0; i < this.dpTable_queryWords.Rows.Count; i++)
                {
                    DpRow word_row = this.dpTable_queryWords.Rows[i];
                    if (word_row.Style == DpRowStyle.Seperator)
                    {
                        // 检索词没有分隔行吧
                        continue;
                    }

                    QueryLine line = (QueryLine)word_row.Tag;
                    if (strStyle == "singlehit")
                    {
                        if (line.ResultRows != null
                            && line.ResultRows.Count == 1)
                        {
                            rows.Add(line.ResultRows[0]);
                        }
                    }
                    else if (strStyle == "multihit")
                    {
                        if (line.ResultRows != null
                            && line.ResultRows.Count > 1)
                            rows.AddRange(line.ResultRows);
                    }
                    else if (strStyle == "all")
                    {
                        if (line.ResultRows != null
                            && line.ResultRows.Count > 0)
                            rows.AddRange(line.ResultRows);
                    }
                }
            }

            return rows;
        }

        void menu_reloadFullElementSet_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<DpRow> rows = GetRecordRows("selected");
            if (rows.Count == 0)
            {
                strError = "尚未选定要重新装载的事项";
                goto ERROR1;
            }

            // TODO: 挑选出那些 'B' 的事项

            // 构造 servers
            ServerCollection servers = new ServerCollection();
            foreach (DpRow row in rows)
            {
                servers.AddRow(row);
            }

            // return:
            //      -1  出错
            //      0   成功
            //      1   中断
            int nRet = DoUpdateSearch(servers,
                out strError);
            if (nRet != 0)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // return:
        //      -1  出错
        //      0   成功
        //      1   中断
        int DoUpdateSearch(ServerCollection servers,
            out string strError)
        {
            int nRet = 0;
            strError = "";

            bool bDontAsk = false;

            if (this.InSearching == true)
            {
                strError = "无法重复启动检索，请稍后再试";
                return -1;
            }
            this.EnableControlsInSearching(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            try
            {
                stop.SetMessage("正在获得检索目标 ...");

                // List<TreeNode> target_nodes = new List<TreeNode>();


                long lProgressValue = 0;
                long lProgressMax = servers.GetLineCount();
                stop.SetProgressRange(0, lProgressMax);

                for (int i = 0; i < servers.Count; i++)
                {
                    // TreeNode target_node = target_nodes[i];
                    OneServer server = servers[i];
                    TreeNode target_node = server.ServerNode;

                    string strServerName = "";
                    if (ZTargetControl.IsDatabaseType(target_node) == true)
                        strServerName = target_node.Parent.Text;
                    else
                        strServerName = target_node.Text;

                    ZConnection connection = null;
                    try
                    {
                        connection = this.GetZConnection(target_node);
                    }
                    catch (Exception ex)
                    {
                        strError = ExceptionUtil.GetAutoText(ex);
                        return -1;
                    }

                    Debug.Assert(connection.TargetInfo != null, "");

                    if (connection.TargetInfo.DbNames == null
                        || connection.TargetInfo.DbNames.Length == 0)
                    {
                        strError = "服务器节点 '" + target_node.Text + "' 下的 " + target_node.Nodes.Count.ToString() + "  个数据库节点全部为 '在全选时不参与检索' 属性，所以通过选定该服务器节点无法直接进行检索，只能通过选定其下的某个数据库节点进行检索";
                        // TODO: 是否可以让跳过？
                        return -1;
                    }

                    Debug.Assert(connection.TreeNode == target_node, "");

                    this.m_currentConnection = connection;

                REDO_INITIAL:
                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   希望中断
                    nRet = DoInitialOneServer(
                        connection,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "针对服务器 '" + strServerName + "' 的连接和初始化操作失败：" + strError;
                        DialogResult result = MessageBox.Show(this,
strError + "\r\n\r\n是否要重试连接和初始化?\r\n\r\n--------------------\r\n注：(是)重试  (否)跳过此服务器继续检索其他服务器  (取消)放弃整个检索操作",
"ZBatchSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            return -1;
                        if (result == System.Windows.Forms.DialogResult.No)
                            continue;
                        goto REDO_INITIAL;
                    }

                    // 针对一个服务器，检索每一行检索式
                    for (int j = 0; j < server.Lines.Count; j++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                            {
                                strError = "用户中断";
                                return 1;
                            }

                        // QueryLine line = lines[j];
                        QueryResult q_line = server.Lines[j];
                        QueryLine line = q_line.QueryLine;

#if NO
                        if (line.ResultRows != null
                            && line.ResultRows.Count > 0)
                        {
                            lProgressValue++;
                            if (lProgressValue <= lProgressMax)
                                stop.SetProgressValue(lProgressValue);
                            continue;   // 已经命中的，不再检索
                        }
#endif

                        stop.SetMessage("正在对服务器 " + strServerName + " 检索 '" + line.Word + "' ...");

                        // 检索一个服务器
                        // 启动检索以后等待检索结束后才返回
                        // thread:
                        //      界面线程
                        // return:
                        //      -1  出错
                        //      0   成功启动检索
                        nRet = DoSearchOneServer(
                            connection,
                            line,
                            null,
                            false,
                            out strError);
                        if (nRet == -1)
                        {
                            return -1;
                            // strErrorInfo += strError + "\r\n";
                        }
                        if (nRet == 1)
                        {
                            strError = "用户中断";
                            return 1;
                        }

                        // 刷新浏览行
                        UpdateSearchResult(q_line,
                            connection,
                            ref bDontAsk);


                        lProgressValue++;
                        if (lProgressValue <= lProgressMax)
                            stop.SetProgressValue(lProgressValue);
                    }
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControlsInSearching(true);
            }


            // TODO: 出错信息统一汇报?

            return 0;
        }

        private void dpTable_queryWords_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            ToolStripSeparator sep = null;

            int nSelectedCount = this.dpTable_queryWords.SelectedRows.Count;

            // 复制到剪贴板
            menuItem = new ToolStripMenuItem("复制检索词(&C)");
            menuItem.Click += new EventHandler(menuItem_queryWords_copy_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false; contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 全选
            menuItem = new ToolStripMenuItem("全选(&A)");
            menuItem.Click += new EventHandler(menuItem_queryWords_selectAll_Click);
            contextMenu.Items.Add(menuItem);

            // 选定没有命中的
            menuItem = new ToolStripMenuItem("选定没有命中的(&N)");
            menuItem.Click += new EventHandler(menuItem_queryWords_selectNotFound_Click);
            contextMenu.Items.Add(menuItem);


            // 选定命中唯一的
            menuItem = new ToolStripMenuItem("选定命中唯一的(&S)");
            menuItem.Click += new EventHandler(menuItem_queryWords_selectSingleHit_Click);
            contextMenu.Items.Add(menuItem);

            // 选定命中多条的
            menuItem = new ToolStripMenuItem("选定命中多条的(&M)");
            menuItem.Click += new EventHandler(menuItem_queryWords_selectMultiHit_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Show(this.dpTable_queryWords, e.Location);
        }

        void menuItem_queryWords_copy_Click(object sender, EventArgs e)
        {
            StringBuilder strTotal = new StringBuilder(4096);
            foreach (DpRow row in this.dpTable_queryWords.Rows)
            {
                if (row.Selected == true)
                {
                    QueryLine line = (QueryLine)row.Tag;
                    strTotal.Append(line.Word + "\r\n");
                }
            }

            Clipboard.SetDataObject(strTotal.ToString(), true);
        }

        void menuItem_queryWords_selectAll_Click(object sender, EventArgs e)
        {
            foreach (DpRow row in this.dpTable_queryWords.Rows)
            {
                if (row.Style == DpRowStyle.None)
                    row.Selected = true;
            }
        }

        void menuItem_queryWords_selectSingleHit_Click(object sender, EventArgs e)
        {
            List<DpRow> rows = GetQueryWordRows("singlehit");
            foreach (DpRow row in this.dpTable_queryWords.Rows)
            {
                if (rows.IndexOf(row) != -1)
                    row.Selected = true;
                else
                    row.Selected = false;
            }
            if (rows.Count == 0)
                MessageBox.Show(this, "当前没有任何命中唯一的检索词行");
        }

        void menuItem_queryWords_selectMultiHit_Click(object sender, EventArgs e)
        {
            List<DpRow> rows = GetQueryWordRows("multihit");
            foreach (DpRow row in this.dpTable_queryWords.Rows)
            {
                if (rows.IndexOf(row) != -1)
                    row.Selected = true;
                else
                    row.Selected = false;
            }
            if (rows.Count == 0)
                MessageBox.Show(this, "当前没有任何命中多条的检索词行");
        }

        void menuItem_queryWords_selectNotFound_Click(object sender, EventArgs e)
        {
            List<DpRow> rows = GetQueryWordRows("notfound");
            foreach (DpRow row in this.dpTable_queryWords.Rows)
            {
                if (rows.IndexOf(row) != -1)
                    row.Selected = true;
                else
                    row.Selected = false;
            }
            if (rows.Count == 0)
                MessageBox.Show(this, "当前没有任何没有命中的检索词行");
        }
        
	}

    public class QueryLine
    {
        public int LineNo = -1;
        public string Word = "";
        public string QueryXml = "";

        public int ResultCount = 0;
        public List<DpRow> ResultRows = null;

        public string Dump()
        {
            return "LineNo=" + LineNo.ToString()
                + " Word=" + Word
                + " QueryXml=" + QueryXml
                + " ResultCount=" + ResultCount.ToString();
        }
    }

    public class RecordInfo
    {
        public DigitalPlatform.Z3950.Record Record = null;

        public TargetInfo TargetInfo = null;
        public Encoding RecordEncoding = null;

        public bool DetectMarcSyntax = false;

        // 检索词信息
        public QueryLine QueryLine = null;
        // 服务器节点
        public TreeNode ServerNode = null;

        public string Dump()
        {
            return "服务器 [" + this.ServerNode.Text + "] 检索词行 [" + this.QueryLine.Dump() + "]";
        }
    }

    class ServerCollection : List<OneServer>
    {
        // 处理添加一个浏览行信息
        public void AddRow(DpRow row)
        {
            RecordInfo info = (RecordInfo)row.Tag;

            Debug.Assert(info.QueryLine != null, "");
            Debug.Assert(info.ServerNode != null, "");

            // 找到 Server
            OneServer server = FindServer(info.ServerNode);
            if (server == null)
            {
                server = new OneServer();
                server.ServerNode = info.ServerNode;
                this.Add(server);
            }

            // 添加 server.Lines
            server.AddQueryLine(row);
        }

        public OneServer FindServer(TreeNode server_node)
        {
            foreach (OneServer server in this)
            {
                if (server.ServerNode == server_node)
                    return server;
            }
            return null;
        }

        public int GetLineCount()
        {
            int result = 0;
            foreach (OneServer server in this)
            {
                result += server.Lines.Count;
            }

            return result;
        }
    }

    // 针对一个服务器进行检索时的若干检索词行
    class OneServer
    {
        // 服务器树节点
        public TreeNode ServerNode = null;

        // 检索词行
        public List<QueryResult> Lines = new List<QueryResult>();

        public QueryResult FindLine(QueryLine line)
        {
            foreach (QueryResult current in this.Lines)
            {
                if (current.QueryLine == line)
                    return current;
            }

            return null;
        }

        public void AddQueryLine(DpRow row)
        {
            RecordInfo info = (RecordInfo)row.Tag;

            Debug.Assert(info.QueryLine != null, "");

            QueryResult one_line = this.FindLine(info.QueryLine);
            if (one_line == null)
            {
                one_line = new QueryResult();
                one_line.QueryLine = info.QueryLine;
                this.Lines.Add(one_line);
            }
                
            one_line.Rows.Add(row);
        }
    }

    // 一个检索词行。携带若干相关内容行
    public class QueryResult
    {
        public QueryLine QueryLine = null;

        public List<DpRow> Rows = new List<DpRow>();    // 这些行都是和 QueryLine 相关的
    }
}
