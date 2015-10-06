using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.GUI;
using System.Collections;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 检索命中多条记录时的浏览选择对话框
    /// </summary>
    internal partial class BrowseSearchResultForm : Form
    {
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
        public MainForm MainForm = null;

        public Stop stop = null;
        /// <summary>
        /// 打开详细窗
        /// </summary>
        public event OpenDetailEventHandler OpenDetail = null;

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
                e.ColumnTitles = this.MainForm.GetBrowseColumnProperties(e.DbName);
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

        /// <summary>
        /// 装入第一条记录到详细窗
        /// </summary>
        /// <param name="bCloseWindow">是否顺便关闭本窗口</param>
        public void LoadFirstDetail(bool bCloseWindow)
        {
            if (this.listView_records.Items.Count == 0)
                return;

            string strError = "";

            string strPath = this.listView_records.Items[0].Text;
            if (strPath.IndexOf("@") == -1)
            {
                string[] paths = new string[1];
                paths[0] = strPath;

                OpenDetailEventArgs args = new OpenDetailEventArgs();
                args.Paths = paths;
                args.OpenNew = false;

                this.listView_records.Enabled = false;
                this.OpenDetail(this, args);
                this.listView_records.Enabled = true;
            }
            else
            {
                BiblioInfo info = m_biblioTable[strPath] as BiblioInfo;
                if (info == null)
                {
                    strError = "路径为 '"+strPath+"' 的事项在 m_biblioTable 中没有找到";
                    goto ERROR1;
                }

                OpenDetailEventArgs args = new OpenDetailEventArgs();
                args.Paths = null;
                args.BiblioInfos = new List<BiblioInfo>();
                args.BiblioInfos.Add(info);
                args.OpenNew = false;

                this.listView_records.Enabled = false;
                this.OpenDetail(this, args);
                this.listView_records.Enabled = true;

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

        void OnLoadDetail()
        {
            if (this.OpenDetail == null)
                return;

            if (this.listView_records.SelectedItems.Count == 0)
                return;

            if (this.stop != null)
                stop.DoStop();

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
                    BiblioInfo info = m_biblioTable[strPath] as BiblioInfo;
                    if (info == null)
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

            this.listView_records.Enabled = false;
            this.OpenDetail(this, args);
            this.listView_records.Enabled = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_records_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();

            OnLoadDetail();
        }

        private void listView_records_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count > 0)
                this.button_OK.Enabled = true;
            else
                this.button_OK.Enabled = false;

            ListViewUtil.OnSeletedIndexChanged(this.listView_records,
                0,
                null);

        }

        private void BrowseSearchResultForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.stop != null)
                this.stop.DoStop();
        }

        private void listView_records_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_records, e);
        }

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