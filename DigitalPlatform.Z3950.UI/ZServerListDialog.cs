using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace DigitalPlatform.Z3950.UI
{
    public partial class ZServerListDialog : Form
    {
        public string XmlFileName { get; set; }
        public bool Changed { get; set; }

        XmlDocument _dom = new XmlDocument();

        const int COLUMN_NAME = 0;
        const int COLUMN_DATABASE = 1;

        public ZServerListDialog()
        {
            InitializeComponent();
        }

        private void ZServerListDialog_Load(object sender, EventArgs e)
        {
            var result = FillList();
            if (result.Value == -1)
                MessageBox.Show(this, result.ErrorInfo);
        }

        private void ZServerListDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ZServerListDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            var result = Save();
            if (result.Value == -1)
                goto ERROR1;
            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
            ERROR1:
            MessageBox.Show(this, result.ErrorInfo);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        NormalResult Save()
        {
            _dom.Save(this.XmlFileName);
            return new NormalResult();
        }


        NormalResult FillList()
        {
            this.listView1.Items.Clear();

            if (string.IsNullOrEmpty(this.XmlFileName))
                return new NormalResult();

            _dom = new XmlDocument();
            try
            {
                _dom.Load(this.XmlFileName);
            }
            catch (FileNotFoundException)
            {
                _dom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                return new NormalResult { Value = -1, ErrorInfo = $"装载配置文件 {this.XmlFileName} 出现异常: {ex.Message}" };
            }

            XmlNodeList servers = _dom.DocumentElement.SelectNodes("server");
            foreach (XmlElement server in servers)
            {
                string name = server.GetAttribute("name");
                ListViewItem item = new ListViewItem();
                item.Tag = server;
                ListViewUtil.ChangeItemText(item, COLUMN_NAME, name);
                ListViewUtil.ChangeItemText(item, COLUMN_DATABASE, ZServerUtil.GetDatabaseList(server));

                this.listView1.Items.Add(item);
            }

            this.Changed = false;
            return new NormalResult();
        }

        private void toolStripButton_new_Click(object sender, EventArgs e)
        {
            XmlElement server = _dom.CreateElement("server");
            _dom.DocumentElement.AppendChild(server);
            server.SetAttribute("recsperbatch", "10");

            ZServerPropertyForm dlg = new ZServerPropertyForm();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.XmlNode = server;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
            {
                server.ParentNode.RemoveChild(server);
                return;
            }

            {
                ListViewItem item = new ListViewItem();
                item.Tag = server;
                ListViewUtil.ChangeItemText(item, COLUMN_NAME, server.GetAttribute("name"));
                ListViewUtil.ChangeItemText(item, COLUMN_DATABASE, ZServerUtil.GetDatabaseList(server));

                this.listView1.Items.Add(item);
            }

            this.Changed = true;
        }

        private void toolStripButton_modify_Click(object sender, EventArgs e)
        {
            string strError;
            if (this.listView1.SelectedItems.Count == 0)
            {
                strError = "尚未选择要修改的服务器";
                goto ERROR1;
            }
            ListViewItem item = this.listView1.SelectedItems[0];

            XmlElement server = (XmlElement)item.Tag;

            ZServerPropertyForm dlg = new ZServerPropertyForm();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.XmlNode = server;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            {
                ListViewUtil.ChangeItemText(item, COLUMN_NAME, server.GetAttribute("name"));
                ListViewUtil.ChangeItemText(item, COLUMN_DATABASE, ZServerUtil.GetDatabaseList(server));
            }

            this.Changed = true;
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_delete_Click(object sender, EventArgs e)
        {
            string strError;
            if (this.listView1.SelectedItems.Count == 0)
            {
                strError = "尚未选择要删除的服务器";
                goto ERROR1;
            }

            // TODO: 询问是否确实要删除

            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                XmlElement server = (XmlElement)item.Tag;

                server.ParentNode.RemoveChild(server);
                this.listView1.Items.Remove(item);
                this.Changed = true;
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }


    }
}
