using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.DTLP
{
    /// <summary>
    /// 观察检索点信息的窗口
    /// </summary>
    public partial class ViewAccessPointForm : Form
    {
        // 检索点字符串。格式为 一个 key 一个from，奇数、偶数这样排列 
        public List<string> AccessPoints = new List<string>();


        public ViewAccessPointForm()
        {
            InitializeComponent();
        }

        private void ViewAccessPointForm_Load(object sender, EventArgs e)
        {
            FillList();
        }

        void FillList()
        {
            this.listView1.Items.Clear();

            if (this.AccessPoints == null)
                return;

            for (int i = 0; i < this.AccessPoints.Count / 2; i++)
            {
                string strKey = this.AccessPoints[i * 2];
                string strFrom = this.AccessPoints[i * 2 + 1].Replace((char)31, '$');

                ListViewItem item = new ListViewItem(strKey, 0);
                item.SubItems.Add(strFrom);

                this.listView1.Items.Add(item);
            }
        }
    }
}