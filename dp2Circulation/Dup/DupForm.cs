using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 查重窗
    /// </summary>
    public partial class DupForm : MyForm
    {
        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();

        /*
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;
         * */

        string m_strXmlRecord = "";

        /// <summary>
        /// 检索结束信号
        /// </summary>
        public AutoResetEvent EventFinish = new AutoResetEvent(false);

        /// <summary>
        /// 是否要(在窗口打开后)自动启动检索
        /// </summary>
        public bool AutoBeginSearch = false;

        const int WM_INITIAL = API.WM_USER + 201;

        const int ITEMTYPE_NORMAL = 0;  // 普通事项
        const int ITEMTYPE_OVERTHRESHOLD = 1; // 权值超过阈值的事项

        #region 外部接口

        /// <summary>
        /// 查重方案名
        /// </summary>
        public string ProjectName
        {
            get
            {
                return this.comboBox_projectName.Text;
            }
            set
            {
                this.comboBox_projectName.Text = value;
            }
        }

        /// <summary>
        /// 发起查重的记录路径。id可以为?。主要用来模拟出keys
        /// </summary>
        public string RecordPath
        {
            get
            {
                return this.textBox_recordPath.Text;
            }
            set
            {
                this.textBox_recordPath.Text = value;
                this.Text = "查重: " + value;
            }
        }

        /// <summary>
        /// 发起查重的XML记录
        /// </summary>
        public string XmlRecord
        {
            get
            {
                return m_strXmlRecord;
            }
            set
            {
                m_strXmlRecord = value;
            }
        }

        /// <summary>
        /// 获得查重结果：所命中的权值超过阈值的记录路径的集合
        /// </summary>
        public string[] DupPaths
        {
            get
            {
                int i;
                List<string> aPath = new List<string>();
                for (i = 0; i < this.listView_browse.Items.Count; i++)
                {
                    ListViewItem item = this.listView_browse.Items[i];

                    if (item.ImageIndex == ITEMTYPE_OVERTHRESHOLD)
                    {
                        aPath.Add(item.Text);
                    }
                    else
                        break;  // 假定超过阈值的事项都在前部，这里可以优化中断
                }

                if (aPath.Count == 0)
                    return new string[0];

                string[] result = new string[aPath.Count];
                aPath.CopyTo(result);

                return result;
            }
        }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public DupForm()
        {
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_browse.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            // 第二列特殊，权值和，右对齐
            prop.SetSortStyle(1, ColumnSortStyle.RightAlign);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);

        }

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            e.ColumnTitles = new ColumnPropertyCollection();
            ColumnPropertyCollection temp = this.MainForm.GetBrowseColumnProperties(e.DbName);
            if (temp != null)
                e.ColumnTitles.AddRange(temp);  // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件

            e.ColumnTitles.Insert(0, "权值和");
        }

        void ClearListViewPropertyCache()
        {
            ListViewProperty prop = (ListViewProperty)this.listView_browse.Tag;
            prop.ClearCache();
        }

        private void DupForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

#if NO
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.checkBox_includeLowCols.Checked = this.MainForm.AppInfo.GetBoolean(
                "dup_form",
                "include_low_cols",
                true);
            this.checkBox_returnAllRecords.Checked = this.MainForm.AppInfo.GetBoolean(
    "dup_form",
    "return_all_records",
    true);

            if (String.IsNullOrEmpty(this.comboBox_projectName.Text) == true)
            {
                this.comboBox_projectName.Text = this.MainForm.AppInfo.GetString(
                        "dup_form",
                        "projectname",
                        "");
            }

            string strWidths = this.MainForm.AppInfo.GetString(
    "dup_form",
    "browse_list_column_width",
    "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_browse,
                    strWidths,
                    true);
            }

            // 自动启动查重
            if (this.AutoBeginSearch == true)
            {
                API.PostMessage(this.Handle, WM_INITIAL, 0, 0);
            }
        }

        /// <summary>
        /// 开始检索
        /// </summary>
        public void BeginSearch()
        {
            API.PostMessage(this.Handle, WM_INITIAL, 0, 0);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_INITIAL:
                    {
                        this.button_search_Click(null, null);
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void DupForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }
#endif
        }

        private void DupForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif

            this.MainForm.AppInfo.SetBoolean(
    "dup_form",
    "include_low_cols",
    this.checkBox_includeLowCols.Checked);
            this.MainForm.AppInfo.SetBoolean(
    "dup_form",
    "return_all_records",
    this.checkBox_returnAllRecords.Checked);

            this.MainForm.AppInfo.SetString(
                "dup_form",
                "projectname",
                this.comboBox_projectName.Text);

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_browse);
            this.MainForm.AppInfo.SetString(
                "dup_form",
                "browse_list_column_width",
                strWidths);

            EventFinish.Set();
        }

#if NO
        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            this.MainForm.Channel_BeforeLogin(this, e);
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        /// <summary>
        /// 启动检索
        /// </summary>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int DoSearch(out string strError)
        {
            strError = "";
            string strUsedProjectName = "";

            this.EventFinish.Reset();
            try
            {

                int nRet = DoSearch(this.comboBox_projectName.Text,
                    this.textBox_recordPath.Text,
                    this.XmlRecord,
                    out strUsedProjectName,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(strUsedProjectName) == false)
                    this.ProjectName = strUsedProjectName;
            }
            finally
            {
                this.EventFinish.Set();
            }

            return 0;
        }

        private void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = this.DoSearch(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.button_search.Enabled = bEnable;
            // this.button_stop.Enabled = bEnable;

            this.comboBox_projectName.Enabled = bEnable;
            this.textBox_recordPath.Enabled = bEnable;
        }

 
        // 检索
        // return:
        //      -1  error
        //      0   succeed
        /// <summary>
        /// 启动查重
        /// </summary>
        /// <param name="strProjectName">查重方案名</param>
        /// <param name="strRecPath">发起记录路径</param>
        /// <param name="strXml">发起记录的 XML</param>
        /// <param name="strUsedProjectName">返回实际使用的方案名</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; >=0: 命中的记录数</returns>
        public int DoSearch(string strProjectName,
            string strRecPath,
            string strXml,
            out string strUsedProjectName,
            out string strError)
        {
            strError = "";
            strUsedProjectName = "";

            if (strProjectName == "<默认>"
                || strProjectName == "<default>")
                strProjectName = "";

            EventFinish.Reset();

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行查重 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                this.ClearDupState();

                this.listView_browse.Items.Clear();
                // 2008/11/22 new add
                this.SortColumns.Clear();
                SortColumns.ClearColumnSortDisplay(this.listView_browse.Columns);

                string strBrowseStyle = "cols";
                if (this.checkBox_includeLowCols.Checked == false)
                    strBrowseStyle += ",excludecolsoflowthreshold";

                long lRet = Channel.SearchDup(
                    stop,
                    strRecPath,
                    strXml,
                    strProjectName,
                    "includeoriginrecord", // includeoriginrecord
                    out strUsedProjectName,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                if (lHitCount == 0)
                    goto END1;   // 查重发现没有命中

                if (stop != null)
                    stop.SetProgressRange(0, lHitCount);

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    DupSearchResult[] searchresults = null;

                    lRet = Channel.GetDupSearchResult(
                        stop,
                        lStart,
                        lPerCount,
                        strBrowseStyle, // "cols,excludecolsoflowthreshold",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                        break;

                    Debug.Assert(searchresults != null, "");

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DupSearchResult result = searchresults[i];

                        ListViewUtil.EnsureColumns(this.listView_browse,
                            2 + (result.Cols == null ? 0 : result.Cols.Length),
                            200);

                        if (this.checkBox_returnAllRecords.Checked == false)
                        {
                            // 遇到第一个权值较低的，就中断全部获取浏览过程
                            if (result.Weight < result.Threshold)
                                goto END1;
                        }

                        ListViewItem item = new ListViewItem();
                        item.Text = result.Path;
                        item.SubItems.Add(result.Weight.ToString());
                        if (result.Cols != null)
                        {
                            for (int j = 0; j < result.Cols.Length; j++)
                            {
                                item.SubItems.Add(result.Cols[j]);
                            }
                        }
                        this.listView_browse.Items.Add(item);



                        if (item.Text == this.RecordPath)
                        {
                            // 如果就是发起记录自己  2008/2/29 new add
                            item.ImageIndex = ITEMTYPE_OVERTHRESHOLD;
                            item.BackColor = Color.LightGoldenrodYellow;
                            item.ForeColor = SystemColors.GrayText; // 表示就是发起记录自己
                        }
                        else if (result.Weight >= result.Threshold)
                        {
                            item.ImageIndex = ITEMTYPE_OVERTHRESHOLD;
                            item.BackColor = Color.LightGoldenrodYellow;
                        }
                        else
                        {
                            item.ImageIndex = ITEMTYPE_NORMAL;
                        }

                        if (stop != null)
                            stop.SetProgressValue(lStart + i + 1);
                    }

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;

                }

            END1:
                this.SetDupState();

                return (int)lHitCount;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EventFinish.Set();

                EnableControls(true);
            }


        ERROR1:
            return -1;
        }

        private void comboBox_projectName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_projectName.Items.Count > 0)
                return;

            string strError = "";
            int nRet = 0;

            string[] projectnames = null;
            // 列出可用的查重方案名
            nRet = ListProjectNames(this.RecordPath,
                out projectnames,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            for (int i = 0; i < projectnames.Length; i++)
            {
                this.comboBox_projectName.Items.Add(projectnames[i]);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        // 
        /// <summary>
        /// 列出可用的查重方案名
        /// </summary>
        /// <param name="strRecPath">发起记录路径</param>
        /// <param name="projectnames">返回可用的查重方案名字符串数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; >=0: 成功</returns>
        public int ListProjectNames(string strRecPath,
            out string [] projectnames,
            out string strError)
        {
            strError = "";
            projectnames = null;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取可用的查重方案名 ...");
            stop.BeginLoop();

            try
            {
                DupProjectInfo[] dpis = null;

                string strBiblioDbName = Global.GetDbName(strRecPath);

                long lRet = Channel.ListDupProjectInfos(
                    stop,
                    strBiblioDbName,
                    out dpis,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                projectnames = new string[dpis.Length];
                for (int i = 0; i < projectnames.Length; i++)
                {
                    projectnames[i] = dpis[i].Name;
                }

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }


        ERROR1:
            return -1;
        }

        private void textBox_recordPath_TextChanged(object sender, EventArgs e)
        {
            // 记录路径会影响到方案名列表
            // 修改记录路径的时候，迫使方案名下拉列表清空，这样当用到下拉列表的时候会自动去获取新内容
            this.comboBox_projectName.Items.Clear();
        }

        /// <summary>
        /// 等待检索结束
        /// </summary>
        public void WaitSearchFinish()
        {
            for (; ; )
            {
                Application.DoEvents();
                bool bRet = this.EventFinish.WaitOne(10, true);
                if (bRet == true)
                    break;
            }
        }

        private void button_viewXmlRecord_Click(object sender, EventArgs e)
        {
            XmlViewerForm dlg = new XmlViewerForm();

            dlg.Text = "当前XML数据";
            dlg.MainForm = this.MainForm;
            dlg.XmlString = this.XmlRecord;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);   // ?? this
            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
        }

        void ClearDupState()
        {
            this.label_dupMessage.Text = "尚未查重";
        }

        /// <summary>
        /// 获得重复数
        /// </summary>
        /// <returns>重复数</returns>
        public int GetDupCount()
        {
            int nCount = 0;
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                ListViewItem item = this.listView_browse.Items[i];

                if (item.Text == this.RecordPath)
                    continue;   // 不包含发起记录自己 2008/2/29 new add

                if (item.ImageIndex == ITEMTYPE_OVERTHRESHOLD)
                    nCount++;
                else
                    break;  // 假定超过权值的事项都在前部，一旦发现一个不是的事项，循环就结束
            }

            return nCount;
        }

        // 设置查重状态
        void SetDupState()
        {
            /*
            int nCount = 0;
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                ListViewItem item = this.listView_browse.Items[i];

                if (item.Text == this.RecordPath)
                    continue;   // 不包含发起记录自己 2008/2/29 new add

                if (item.ImageIndex == ITEMTYPE_OVERTHRESHOLD)
                    nCount++;
                else
                    break;  // 假定超过权值的事项都在前部，一旦发现一个不是的事项，循环就结束
            }
             * */

            int nCount = GetDupCount();

            if (nCount > 0)
                this.label_dupMessage.Text = "有 " + Convert.ToString(nCount) + " 条重复记录。";
            else
                this.label_dupMessage.Text = "没有重复记录。";

        }

        // 双击
        private void listView_browse_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView_browse.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装入详细窗的事项");
                return;
            }
            string strPath = this.listView_browse.SelectedItems[0].SubItems[0].Text;

            EntityForm form = new EntityForm();

            form.MdiParent = this.MainForm;

            form.MainForm = this.MainForm;
            form.Show();
            form.LoadRecordOld(strPath, "", true);
        }

        private void listView_browse_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;
            else if (nClickColumn == 1)
                sortStyle = ColumnSortStyle.RightAlign;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_browse.Columns,
                true);

            // 排序
            this.listView_browse.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView_browse.ListViewItemSorter = null;

        }

        private void DupForm_Activated(object sender, EventArgs e)
        {
#if NO
            // 2009/8/13 new add
            this.MainForm.stopManager.Active(this.stop);
#endif

        }

        private void listView_browse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_browse.SelectedItems.Count == 1)
            {
                ListViewItem item = this.listView_browse.SelectedItems[0];
                int nLineNo = this.listView_browse.SelectedIndices[0] + 1;
                if (item.ImageIndex == ITEMTYPE_OVERTHRESHOLD)
                {
                    if (item.Text == this.RecordPath)
                    {
                        this.label_message.Text = "序号 " + nLineNo.ToString() + ": 发起查重的记录(自己)";
                    }
                    else
                    {
                        this.label_message.Text = "序号 " + nLineNo.ToString() + ": 重复的记录";
                    }
                }
                else
                {
                    this.label_message.Text = "序号 " + nLineNo.ToString();
                }
            }
            else
            {
                this.label_message.Text = "";
            }

            // 装入(未装入的)浏览列
            if (this.listView_browse.SelectedItems.Count > 0)
            {
                List<string> pathlist = new List<string>();
                List<ListViewItem> itemlist = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_browse.SelectedItems)
                {
                    string strFirstCol = ListViewUtil.GetItemText(item, 2);
                    if (string.IsNullOrEmpty(strFirstCol) == false)
                        continue;
                    pathlist.Add(item.Text);
                    itemlist.Add(item);
                }

                if (pathlist.Count > 0)
                {
                    string strError = "";
                    int nRet = GetBrowseCols(pathlist,
                        itemlist,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                }
            }

            ListViewUtil.OnSeletedIndexChanged(this.listView_browse,
    0,
    null);
        }

        // 右鼠标键菜单
        private void listView_browse_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("装入新开的种册窗(&N)");
            menuItem.Click += new System.EventHandler(this.menu_loadToNewDetailWindow_Click);
            if (this.listView_browse.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            menuItem.DefaultItem = true;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入已经打开的种册窗(&E)");
            menuItem.Click += new System.EventHandler(this.menu_loadToExistDetailWindow_Click);
            if (this.listView_browse.SelectedItems.Count == 0
                || this.MainForm.GetTopChildWindow<EntityForm>() == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("填充全部浏览列(&F)");
            menuItem.Click += new System.EventHandler(this.menu_fillBrowseCols_Click);
            /*
            if (this.listView_browse.SelectedItems.Count == 0)
                menuItem.Enabled = false;
             * */
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_browse, new Point(e.X, e.Y));
        }

        void menu_loadToNewDetailWindow_Click(object sender, EventArgs e)
        {
            if (this.listView_browse.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装入种册窗的事项");
                return;
            }
            string strPath = this.listView_browse.SelectedItems[0].SubItems[0].Text;

            EntityForm form = new EntityForm();

            form.MdiParent = this.MainForm;
            form.MainForm = this.MainForm;
            form.Show();
            form.LoadRecordOld(strPath, "", true);
        }

        void menu_loadToExistDetailWindow_Click(object sender, EventArgs e)
        {
            if (this.listView_browse.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装入种册窗的事项");
                return;
            }
            string strPath = this.listView_browse.SelectedItems[0].SubItems[0].Text;

            EntityForm form = this.MainForm.GetTopChildWindow<EntityForm>();
            if (form == null)
            {
                MessageBox.Show(this, "目前并没有已经打开的种册窗");
                return;
            }
            Global.Activate(form);
            form.LoadRecordOld(strPath, "", true);
        }

        void menu_fillBrowseCols_Click(object sender, EventArgs e)
        {
            List<string> pathlist = new List<string>();
            List<ListViewItem> itemlist = new List<ListViewItem>();
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                ListViewItem item = this.listView_browse.Items[i];
                string strFirstCol = ListViewUtil.GetItemText(item, 2);
                if (string.IsNullOrEmpty(strFirstCol) == false)
                    continue;
                pathlist.Add(item.Text);
                itemlist.Add(item);
            }

            if (pathlist.Count > 0)
            {
                string strError = "";
                int nRet = GetBrowseCols(pathlist,
                    itemlist,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
            }
        }

        int GetBrowseCols(List<string> pathlist,
            List<ListViewItem> itemlist,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在填充浏览列 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {


                int nStart = 0;
                int nCount = 0;
                for (; ; )
                {
                    nCount = pathlist.Count - nStart;
                    if (nCount > 100)
                        nCount = 100;
                    if (nCount <= 0)
                        break;

                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }
                    }

                    stop.SetMessage("正在装入浏览信息 " + (nStart + 1).ToString() + " - " + (nStart + nCount).ToString());




                    string[] paths = new string[nCount];
                    pathlist.CopyTo(nStart, paths, 0, nCount);

                    Record[] searchresults = null;

                    long lRet = this.Channel.GetBrowseRecords(
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

                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        Record record = searchresults[i];

                        ListViewUtil.EnsureColumns(this.listView_browse,
                            2 + (record.Cols == null ? 0 : record.Cols.Length),
                            200);

                        ListViewItem item = itemlist[nStart + i];
                        item.Text = record.Path;
                        if (record.Cols != null)
                        {
                            for (int j = 0; j < record.Cols.Length; j++)
                            {
                                item.SubItems.Add(record.Cols[j]);
                            }
                        }
                    }


                    nStart += searchresults.Length;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            return 0;
        }
    }
}