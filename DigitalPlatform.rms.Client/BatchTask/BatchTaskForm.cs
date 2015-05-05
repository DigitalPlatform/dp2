using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.GUI;

namespace DigitalPlatform.rms.Client
{
    public partial class BatchTaskForm : Form
    {
        public RmsChannel Channel = null;
        public Stop Stop = null;

        public BatchTaskForm()
        {
            InitializeComponent();
        }

        private void BatchTaskForm_Load(object sender, EventArgs e)
        {

        }

        private void BatchTaskForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void BatchTaskForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 列出后台任务
        private void toolStripButton_listTasks_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = ListTasks(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int ListTasks(out string strError)
        {
            strError = "";

            this.listView_tasks.Items.Clear();

            TaskInfo [] results = null;

            int nRet = this.Channel.DoBatchTask("",
    "getinfo",
    null,
    out results,
    out strError);
            if (nRet == -1)
                return -1;

            foreach (TaskInfo info in results)
            {
                ListViewItem item = new ListViewItem();
                item.Text = info.Name;
                ListViewUtil.ChangeItemText(item, 1, info.ID);
                ListViewUtil.ChangeItemText(item, 2, info.State);
                this.listView_tasks.Items.Add(item);
            }

            return 0;
        }
    }
}
