using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 检索命中多条记录时的浏览选择对话框
    /// </summary>
    internal partial class BrowseSearchResultForm : Form
    {
#if NO
        /// <summary>
        /// 窗口关闭前，停止通道前触发的事件
        /// </summary>
        public event EventHandler BoforeStop = null;
#endif

        // 2015/8/14
        Hashtable m_biblioTable = new Hashtable(); // 书目记录路径 --> 书目信息

        public Hashtable BiblioTable
        {
            get
            {
                return this.m_biblioTable;
            }
        }

        // 外来数据的浏览列标题的对照表。MARC 格式名 --> 列标题字符串
        Hashtable _browseTitleTable = new Hashtable();
        public Hashtable BrowseTitleTable
        {
            get
            {
                return this._browseTitleTable;
            }
            set
            {
                this._browseTitleTable = value;
            }
        }

        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        public Stop stop = null;
        /// <summary>
        /// 打开详细窗
        /// </summary>
        public event OpenDetailEventHandler OpenDetail = null;

        public event LoadNextBatchEventHandler LoadNext = null;

        /// <summary>
        /// 显示记录的ListView窗
        /// </summary>
        public ListView RecordsList
        {
            get
            {
                return this.listView_records;
            }
        }

        public BrowseSearchResultForm()
        {
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_records.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);
        }

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            if (e.DbName == "<blank>")
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                e.ColumnTitles.Add("检索点");
                e.ColumnTitles.Add("数量");
                return;
            }

            if (e.DbName.IndexOf("@") == -1)
                e.ColumnTitles = Program.MainForm.GetBrowseColumnProperties(e.DbName);
            else
            {
                string strFormat = "";
                if (this.m_biblioTable != null)
                {
                    BiblioInfo info = this.m_biblioTable[e.DbName] as BiblioInfo;
                    if (info != null)
                        strFormat = info.Format;
                }
                e.ColumnTitles = new ColumnPropertyCollection();
                string strColumnTitles = (string)_browseTitleTable[strFormat];
                List<string> titles = StringUtil.SplitList(strColumnTitles, '\t');
                foreach (string s in titles)
                {
                    ColumnProperty property = new ColumnProperty(s);
                    e.ColumnTitles.Add(property);
                }
            }

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.button_OK.Enabled = false;
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择事项");
                this.button_OK.Enabled = true;
                return;
            }

            if (this.listView_records.SelectedItems.Count == 1)
            {
                string strPath = this.listView_records.SelectedItems[0].SubItems[0].Text;

                if (this.LoadNext != null
                    && BiblioSearchForm.IsCmdLine(strPath))
                {
                    LoadNextBatchEventArgs e1 = new LoadNextBatchEventArgs { All = false };
                    this.LoadNext(this, e1);
                    this.button_OK.Enabled = true;
                    return;
                }
            }

            OnLoadDetail();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 确保列标题数量足够
        void EnsureColumns(int nCount)
        {
            if (this.listView_records.Columns.Count >= nCount)
                return;

            for (int i = this.listView_records.Columns.Count; i < nCount; i++)
            {
                string strText = "";
                if (i == 0)
                {
                    strText = "记录路径";
                }
                else
                {
                    strText = Convert.ToString(i);
                }

                ColumnHeader col = new ColumnHeader();
                col.Text = strText;
                col.Width = 200;
                this.listView_records.Columns.Add(col);
            }

        }

        /// <summary>
        /// 在listview最后追加一行
        /// </summary>
        /// <param name="strID">ID</param>
        /// <param name="others">其他列内容</param>
        public void NewLine(string strID,
            string[] others)
        {
            EnsureColumns(others.Length + 1);

            ListViewItem item = new ListViewItem(strID, 0);

            this.listView_records.Items.Add(item);

            for (int i = 0; i < others.Length; i++)
            {
                item.SubItems.Add(others[i]);
            }
        }

        /*
        private void listView_records_DoubleClick(object sender, EventArgs e)
        {
            OnLoadDetail();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
         */

        string GetFirstRecordPath()
        {
            foreach(ListViewItem item in this.listView_records.Items)
            {
                var path = item.Text;
                if (BiblioSearchForm.IsCmdLine(path))
                    continue;
                return path;
            }

            return null;
        }

        /// <summary>
        /// 装入第一条记录到详细窗
        /// </summary>
        /// <param name="bCloseWindow">是否顺便关闭本窗口</param>
        public void LoadFirstDetail(bool bCloseWindow)
        {
            if (this.listView_records.Items.Count == 0)
                return;

            string strError = "";

            // 找到第一个记录行。注意可能有命令行，需要跳过
            string strPath = GetFirstRecordPath();  // this.listView_records.Items[0].Text;
            // 2019/5/23
            if (string.IsNullOrEmpty(strPath))
            {
                strError = "当前浏览结果中没有任何记录行";
                goto ERROR1;
            }
            if (strPath.IndexOf("@") == -1)
            {
                string[] paths = new string[1];
                paths[0] = strPath;

                OpenDetailEventArgs args = new OpenDetailEventArgs
                {
                    Paths = paths,
                    OpenNew = false
                };

#if NO
                this.listView_records.Enabled = false;
                this.OpenDetail(this, args);
                this.listView_records.Enabled = true;
#endif
                DoOpenDetail(args);

            }
            else
            {
                if (!(m_biblioTable[strPath] is BiblioInfo info))
                {
                    strError = "路径为 '" + strPath + "' 的事项在 m_biblioTable 中没有找到";
                    goto ERROR1;
                }

                OpenDetailEventArgs args = new OpenDetailEventArgs
                {
                    Paths = null,
                    BiblioInfos = new List<BiblioInfo>()
                };
                args.BiblioInfos.Add(info);
                args.OpenNew = false;

                DoOpenDetail(args);
            }

            if (bCloseWindow == true)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void DoOpenDetail(OpenDetailEventArgs args)
        {
            this.Visible = false;   // 2018/10/31 避免在 OpenDetail 事件执行期间出现的 MessageBox.Show() 被本窗口遮挡出现无法用鼠标点按按钮的问题
            this.listView_records.Enabled = false;
            this.OpenDetail(this, args);
            this.listView_records.Enabled = true;
            this.Visible = true;
        }

        void OnLoadDetail()
        {
            if (this.OpenDetail == null)
                return;

            if (this.listView_records.SelectedItems.Count == 0)
                return;

            DoStop();

            string strError = "";
            // string[] paths = new string[this.listView_records.SelectedItems.Count];
            List<string> path_list = new List<string>();
            List<BiblioInfo> biblioInfo_list = new List<BiblioInfo>();
            //int i = 0;
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                string strPath = item.Text;
                if (strPath.IndexOf("@") == -1)
                    path_list.Add(strPath);
                else
                {
                    if (!(m_biblioTable[strPath] is BiblioInfo info))
                    {
                        strError = "路径为 '" + strPath + "' 的事项在 m_biblioTable 中没有找到";
                        goto ERROR1;
                    }
                    biblioInfo_list.Add(info);
                }
                // paths[i++] = strPath;
            }

            OpenDetailEventArgs args = new OpenDetailEventArgs();
            string[] paths = new string[path_list.Count];
            path_list.CopyTo(paths);
            args.Paths = paths;
            args.BiblioInfos = biblioInfo_list;
            args.OpenNew = true;

#if NO
            this.listView_records.Enabled = false;
            this.OpenDetail(this, args);
            this.listView_records.Enabled = true;
#endif
            DoOpenDetail(args);

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_records_DoubleClick(object sender, EventArgs e)
        {
            /*
            if (this.listView_records.SelectedItems.Count == 1)
            {
                string strPath = this.listView_records.SelectedItems[0].SubItems[0].Text;

                if (this.LoadNext != null
                    && BiblioSearchForm.IsCmdLine(strPath))
                {
                    LoadNextBatchEventArgs e1 = new LoadNextBatchEventArgs { All = false };
                    this.LoadNext(this, e1);
                    return;
                }
            }
            */

            button_OK_Click(sender, e);
        }

        int _currentIndex = -1;

        List<string> _recpaths = new List<string>();

        public List<string> RecPaths
        {
            get
            {
                return _recpaths;
            }
        }

        public void StoreList()
        {
            _recpaths.Clear();
            foreach (ListViewItem item in this.listView_records.Items)
            {
                _recpaths.Add(ListViewUtil.GetItemText(item, 0));
            }
        }

        private void listView_records_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedIndices.Count == 0)
                _currentIndex = -1;
            else
                _currentIndex = this.listView_records.SelectedIndices[0];

            if (this.listView_records.SelectedItems.Count > 0)
                this.button_OK.Enabled = true;
            else
                this.button_OK.Enabled = false;

            ListViewUtil.OnSelectedIndexChanged(this.listView_records,
                0,
                null);
        }

        private void BrowseSearchResultForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            DoStop();
        }

        void DoStop()
        {
            if (this.stop != null)
            {
#if NO
                if (this.BoforeStop != null)
                {
                    this.BoforeStop(this, new EventArgs());
                }
#endif
                stop.DoStop(this);
                this.stop = null;
            }
        }

        private void listView_records_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_records, e);
        }

        private void listView_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSelectedItemCount = this.listView_records.SelectedItems.Count;
            string strFirstColumn = "";
            if (nSelectedItemCount > 0)
            {
                strFirstColumn = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);
            }

            if (nSelectedItemCount == 1 && BiblioSearchForm.IsCmdLine(strFirstColumn))
            {
                CommandPopupMenu(sender, e);
                return;
            }
        }

        // 浏览行 为命令时候，应当出现的弹出菜单
        private void CommandPopupMenu(object sender, MouseEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSelectedItemCount = this.listView_records.SelectedItems.Count;
            string strFirstColumn = "";
            if (nSelectedItemCount > 0)
            {
                strFirstColumn = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);
            }

            menuItem = new MenuItem("载入下一批浏览行(&N)");
            menuItem.DefaultItem = true;
            menuItem.Click += new System.EventHandler(this.menu_loadNextBatch_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("载入余下全部浏览行(&A)");
            menuItem.Click += new System.EventHandler(this.menu_loadRestAllBatch_Click);
            contextMenu.MenuItems.Add(menuItem);

#if NO
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);
#endif

            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
        }

        void menu_loadNextBatch_Click(object sender, EventArgs e)
        {
            if (this.LoadNext != null)
            {
                LoadNextBatchEventArgs e1 = new LoadNextBatchEventArgs { All = false };
                this.LoadNext(this, e1);
            }
        }

        void menu_loadRestAllBatch_Click(object sender, EventArgs e)
        {
            if (this.LoadNext != null)
            {
                LoadNextBatchEventArgs e1 = new LoadNextBatchEventArgs { All = true };
                this.LoadNext(this, e1);
            }
        }

#if NO
        public string GetPrevNextRecPath(string strStyle)
        {
            REDO:
            ListViewItem item = BiblioSearchForm.MoveSelectedItem(this.listView_records, strStyle);
            if (item == null)
                return "";
            string text = ListViewUtil.GetItemText(item, 0);
            // 遇到 Z39.50 命令行，要跳过去
            if (BiblioSearchForm.IsCmdLine(text))
                goto REDO;
            return text;
        }
#endif

        public string GetPrevNextRecPath(string strStyle)
        {
            if (_recpaths.Count == 0)
                return "";

            while (true)
            {
                if (strStyle == "prev")
                    _currentIndex--;
                else
                    _currentIndex++;
                if (_currentIndex >= _recpaths.Count
                    || _currentIndex < 0)
                    return "";
                string recpath = _recpaths[_currentIndex];
                // 遇到 Z39.50 命令行，要跳过去
                if (BiblioSearchForm.IsCmdLine(recpath) == false)
                    return recpath;
            }
        }
        public BiblioInfo GetBiblioInfo(string strPath)
        {
            string strError = "";
            if (this.m_biblioTable == null)
            {
                strError = "m_biblioTable == null";
                throw new Exception(strError);
            }

            return this.m_biblioTable[strPath] as BiblioInfo;
        }

        public void ShowMessage(string strMessage,
    string strColor = "",
    bool bClickClose = false)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Action(() =>
                {
                    this.label_message.Text = strMessage;
                }));
            else
                this.label_message.Text = strMessage;

        }

        public void ShowMessageBox(string text)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(this, text);
                }));
            else
                MessageBox.Show(this, text);

        }
    }

    public delegate void LoadNextBatchEventHandler(object sender,
LoadNextBatchEventArgs e);

    /// <summary>
    /// 打开详细窗事件的参数
    /// </summary>
    public class LoadNextBatchEventArgs : EventArgs
    {
        // [in] 是否要获取余下的全部记录
        public bool All { get; set; }
    }

    /// <summary>
    /// 打开详细窗事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void OpenDetailEventHandler(object sender,
    OpenDetailEventArgs e);

    /// <summary>
    /// 打开详细窗事件的参数
    /// </summary>
    public class OpenDetailEventArgs : EventArgs
    {
        /// <summary>
        /// 记录全路径集合。
        /// </summary>
        public string[] Paths = null;

        /// <summary>
        /// 是否开为新窗口
        /// </summary>
        public bool OpenNew = false;

        /// <summary>
        /// BiblioInfo 对象的集合
        /// </summary>
        public List<BiblioInfo> BiblioInfos = null;    // 如果 Paths 为空，则根据这里打开详细窗 2015/8/14
    }

}