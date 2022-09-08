using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Data.Sql;
using System.Threading.Tasks;

namespace DigitalPlatform
{
    public partial class GetSqlServerDlg : Form
    {
        public GetSqlServerDlg()
        {
            InitializeComponent();
        }

        private async void SqlServerDlg_Load(object sender, EventArgs e)
        {
            // this.BeginInvoke(new Delegate_FillList(FillList));
            await FillList();
        }

        public delegate void Delegate_FillList();

        async Task FillList()
        {
            EnableControls(false);
            this.listView_sqlServers.Items.Clear();
            ListViewItem item = new ListViewItem("正在获取SQL服务器信息 ...");
            this.listView_sqlServers.Items.Add(item);

            Application.DoEvents();
            this.Update();

            System.Data.DataTable table = null;

            await Task.Factory.StartNew(() =>
            {
                SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
                table = instance.GetDataSources();
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);

            this.listView_sqlServers.Items.Clear();

            foreach (System.Data.DataRow row in table.Rows)
            {
                string strServerName = row["ServerName"].ToString();
                string strInstanceName = row["InstanceName"].ToString();
                string strIsClustered = row["IsClustered"].ToString();
                string strVersion = row["Version"].ToString();

                item = new ListViewItem(strServerName);

                item.SubItems.Add(strInstanceName);
                item.SubItems.Add(strIsClustered);
                item.SubItems.Add(strVersion);
                this.listView_sqlServers.Items.Add(item);

                // 如果和本地计算机名相同，并且没有instancename，则事项字体加粗
                if (strServerName == SystemInformation.ComputerName
                    && String.IsNullOrEmpty(strInstanceName) == true)
                    item.Font = new Font(item.Font, FontStyle.Bold);
            }

            EnableControls(true);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.textBox_sqlServerName.Enabled = bEnable;
            this.listView_sqlServers.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
            this.button_OK.Enabled = bEnable;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.textBox_sqlServerName.Text))
            {
                MessageBox.Show(this, "尚未指定SQL服务器名");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string SqlServerName
        {
            get
            {
                return this.textBox_sqlServerName.Text;
            }
            set
            {
                this.textBox_sqlServerName.Text = value;
            }
        }

        private void listView_sqlServers_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (this.listView_sqlServers.SelectedItems.Count == 0)
                this.textBox_sqlServerName.Text = "";
            else
            {
                string strServerName = this.listView_sqlServers.SelectedItems[0].Text;
                if (this.listView_sqlServers.SelectedItems[0].SubItems[1].Text != "")
                    strServerName += "\\" + this.listView_sqlServers.SelectedItems[0].SubItems[1].Text;

                this.textBox_sqlServerName.Text = strServerName;
            }
        }

        // 双击
        private void listView_sqlServers_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(sender, e);
        }
    }
}