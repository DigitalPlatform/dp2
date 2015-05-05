using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.Library
{
    /// <summary>
    /// 对话框：从若干重复的册记录中选择一个
    /// </summary>
    public partial class SelectDupItemRecordDlg : Form
    {
        /// <summary>
        /// 路径的集合
        /// </summary>
        public List<DoublePath> Paths = null;

        /// <summary>
        /// 选择的路径(双路径)
        /// </summary>
        public DoublePath SelectedDoublePath = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SelectDupItemRecordDlg()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 消息文字
        /// </summary>
        public string MessageText
        {
            get
            {
                return this.label_message.Text;
            }
            set
            {
                this.label_message.Text = value;
            }
        }

        private void SelectDupItemRecord_Load(object sender, EventArgs e)
        {

            this.FillList();

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listView_paths.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择任何事项");
                return;
            }

            this.SelectedDoublePath = this.Paths[this.listView_paths.SelectedIndices[0]];

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void FillList()
        {
            this.listView_paths.Items.Clear();

            if (this.Paths == null)
                return;

            for (int i = 0; i < this.Paths.Count; i++)
            {
                DoublePath dpath = this.Paths[i];

                ListViewItem item = new ListViewItem(dpath.ItemRecPath);

                item.SubItems.Add(dpath.BiblioRecPath);

                this.listView_paths.Items.Add(item);
            }
        }

        private void listView_paths_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_paths.SelectedItems.Count > 0)
                this.button_OK.Enabled = true;
            else
                this.button_OK.Enabled = false;
        }

        private void listView_paths_DoubleClick(object sender, EventArgs e)
        {
            this.button_OK_Click(null, null);
        }
    }


    /// <summary>
    /// 专用于存储种、册相关记录路径的双路径结构
    /// </summary>
    public class DoublePath
    {
        /// <summary>
        /// 书目库路径
        /// </summary>
        public string BiblioRecPath = "";

        /// <summary>
        /// 实体库路径
        /// </summary>
        public string ItemRecPath = "";
    }
}