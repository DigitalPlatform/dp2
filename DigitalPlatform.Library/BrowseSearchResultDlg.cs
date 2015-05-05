using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.rms.Client;

namespace DigitalPlatform.Library
{
    /// <summary>
    /// 浏览检索命中的结果的窗口
    /// </summary>
    public partial class BrowseSearchResultDlg : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public SearchPanel SearchPannel = null;

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

        /// <summary>
        /// 构造函数
        /// </summary>
        public BrowseSearchResultDlg()
        {
            InitializeComponent();
        }

        private void BrowseSearchResultDlg_Load(object sender, EventArgs e)
        {
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择事项");
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
        /// <param name="strID"></param>
        /// <param name="others"></param>
        public void NewLine(string strID,
            string[] others)
        {
            EnsureColumns(others.Length + 1);

            ListViewItem item = new ListViewItem(ResPath.GetReverseRecordPath(strID), 0);

            this.listView_records.Items.Add(item);

            for (int i = 0; i < others.Length; i++)
            {
                item.SubItems.Add(others[i]);
            }
        }

        private void listView_records_DoubleClick(object sender, EventArgs e)
        {
            OnLoadDetail();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 装入第一条记录到详细窗
        /// </summary>
        /// <param name="bCloseWindow"></param>
        public void LoadFirstDetail(bool bCloseWindow)
        {
            if (this.listView_records.Items.Count == 0)
                return;

            string[] paths = new string[1];
            paths[0] = ResPath.GetRegularRecordPath(this.listView_records.Items[0].Text);

            OpenDetailEventArgs args = new OpenDetailEventArgs();
            args.Paths = paths;
            args.OpenNew = false;

            this.listView_records.Enabled = false;
            this.OpenDetail(this, args);
            this.listView_records.Enabled = true;

            if (bCloseWindow == true)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        void OnLoadDetail()
        {
            if (this.OpenDetail == null)
                return;

            if (this.listView_records.SelectedItems.Count == 0)
                return;

            string[] paths = new string[this.listView_records.SelectedItems.Count];
            for (int i = 0; i < this.listView_records.SelectedItems.Count; i++)
            {
                string strPath = this.listView_records.SelectedItems[i].Text;

                paths[i] = ResPath.GetRegularRecordPath(strPath);

            }

            OpenDetailEventArgs args = new OpenDetailEventArgs();
            args.Paths = paths;
            args.OpenNew = true;

            this.listView_records.Enabled = false;
            this.OpenDetail(this, args);
            this.listView_records.Enabled = true;
        }

    }
}