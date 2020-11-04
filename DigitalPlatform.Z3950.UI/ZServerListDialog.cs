using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.GUI;
using DigitalPlatform.Xml;

namespace DigitalPlatform.Z3950.UI
{
    public partial class ZServerListDialog : Form
    {
        public string XmlFileName { get; set; }
        public bool Changed { get; set; }

        XmlDocument _dom = new XmlDocument();

        const int COLUMN_NAME = 0;
        const int COLUMN_DATABASE = 1;
        const int COLUMN_ENABLED = 2;

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
            if (this.Changed)
            {
                DialogResult result = MessageBox.Show(this,
"当前有修改尚未保存。\r\n\r\n确实要放弃保存修改?\r\n\r\n[是]放弃修改，对话框关闭；[否]不关闭对话框",
"ZServerListDialog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
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
            this.Changed = false;
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
            FillServers(servers);

            this.Changed = false;
            return new NormalResult();
        }

        public static bool IsEnabled(string enabled, bool default_value)
        {
            if (string.IsNullOrEmpty(enabled))
                return default_value;
            if (enabled == "yes" || enabled == "on")
                return true;
            return false;
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

            using (ZServerPropertyForm dlg = new ZServerPropertyForm())
            {
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.UnionCatalogPageVisible = false;
                dlg.XmlNode = server;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult == DialogResult.Cancel)
                    return;

                // 对 server name 进行查重
                string name = server.GetAttribute("name");
                if (SearchDup(ref name, item) == true)
                    server.SetAttribute("name", name);

                {
                    ListViewUtil.ChangeItemText(item, COLUMN_NAME, server.GetAttribute("name"));
                    ListViewUtil.ChangeItemText(item, COLUMN_DATABASE, ZServerUtil.GetDatabaseList(server));
                }

                this.Changed = true;
                return;
            }
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

            // 询问是否确实要删除
            DialogResult result = MessageBox.Show(this,
$"确实要删除选定的 {this.listView1.SelectedItems.Count} 个服务器?",
"ZServerListDialog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

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

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                this.toolStripButton_delete.Enabled = true;
                this.toolStripButton_modify.Enabled = true;
                this.toolStripButton_export.Enabled = true;
            }
            else
            {
                this.toolStripButton_delete.Enabled = false;
                this.toolStripButton_modify.Enabled = false;
                this.toolStripButton_export.Enabled = false;
            }

            if (this.listView1.SelectedItems.Count == 0)
            {
                this.toolStripButton_enabled.Enabled = false;
                this.toolStripButton_enabled.CheckState = CheckState.Unchecked;
            }
            else if (this.listView1.SelectedItems.Count == 1)
            {
                this.toolStripButton_enabled.Enabled = true;

                var item = this.listView1.SelectedItems[0];
                this.toolStripButton_enabled.CheckState = IsEnabled(item) ? CheckState.Checked : CheckState.Unchecked;
            }
            else
            {
                // 多个事项
                this.toolStripButton_enabled.Enabled = true;

                this.toolStripButton_enabled.CheckState = GetCheckState(this.listView1.SelectedItems.Cast<ListViewItem>());
            }

            if (this.listView1.SelectedItems.Count == 1)
            {
                if (this.listView1.SelectedIndices[0] <= 0)
                    this.toolStripButton_moveUp.Enabled = false;
                else
                    this.toolStripButton_moveUp.Enabled = true;

                if (this.listView1.SelectedIndices[0] >= this.listView1.Items.Count - 1)
                    this.toolStripButton_moveDown.Enabled = false;
                else
                    this.toolStripButton_moveDown.Enabled = true;

            }
            else
            {
                this.toolStripButton_moveUp.Enabled = false;
                this.toolStripButton_moveDown.Enabled = false;
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            toolStripButton_modify_Click(sender, e);
        }

        private void toolStripButton_enabled_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_enabled.CheckState == CheckState.Indeterminate)
                return;
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                XmlElement server = (XmlElement)item.Tag;
                bool old_enabled = IsEnabled(item);
                bool new_enabled = this.toolStripButton_enabled.CheckState == CheckState.Checked;
                if (old_enabled != new_enabled)
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_ENABLED, new_enabled ? "是" : "否");
                    SetItemColor(item);
                    server.SetAttribute("enabled", new_enabled ? "yes" : "no");
                    this.Changed = true;
                }
            }
        }

        static bool IsEnabled(ListViewItem item)
        {
            XmlElement server = (XmlElement)item.Tag;
            return IsEnabled(server.GetAttribute("enabled"), true);
        }

        // 获得若干个事项的“启用状态”
        static CheckState GetCheckState(IEnumerable<ListViewItem> items)
        {
            string last_state = "";
            foreach (ListViewItem item in items)
            {
                string current_state = IsEnabled(item) ? "enabled" : "disabled";
                if (string.IsNullOrEmpty(last_state) == false
                    && current_state != last_state)
                    return CheckState.Indeterminate;

                last_state = current_state;
            }

            if (string.IsNullOrEmpty(last_state))
                return CheckState.Indeterminate;

            return last_state == "enabled" ? CheckState.Checked : CheckState.Unchecked;
        }

        private void toolStripButton_moveUp_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 1)
                MoveUpDown(this.listView1.SelectedItems[0], true);
        }

        private void toolStripButton_moveDown_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 1)
                MoveUpDown(this.listView1.SelectedItems[0], false);
        }

        bool MoveUpDown(ListViewItem item, bool up)
        {
            int index = this.listView1.Items.IndexOf(item);
            if (index == -1)
                return false;
            if (up && index == 0)
                return false;
            if (up == false && index >= this.listView1.Items.Count)
                return false;
            this.listView1.Items.Remove(item);


            if (up)
                index--;
            else
                index++;
            this.listView1.Items.Insert(index, item);
            ListViewUtil.SelectLine(item, true);

            XmlElement ref_server = null;
            // 找到基点 ListViewItem
            {
                ListViewItem ref_item = null;
                if (index + 1 < this.listView1.Items.Count)
                {
                    ref_item = this.listView1.Items[index + 1];
                    ref_server = (XmlElement)ref_item.Tag;
                }
            }

            XmlElement server = (XmlElement)item.Tag;
            // 修改 XML
            _dom.DocumentElement.RemoveChild(server);
            if (ref_server != null)
                _dom.DocumentElement.InsertBefore(server, ref_server);
            else
                _dom.DocumentElement.AppendChild(server);

            this.Changed = true;
            return true;
        }

        private void toolStripButton_export_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定需要导出的服务器");
                return;
            }
            // 询问文件名
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "请指定要保存的 XML 文件名";
                dlg.CreatePrompt = false;
                dlg.OverwritePrompt = true;
                // dlg.FileName = this.ExportTextFilename;
                dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                foreach (ListViewItem item in this.listView1.SelectedItems)
                {
                    XmlElement server = (XmlElement)item.Tag;
                    XmlElement export_server = dom.CreateElement("server");
                    dom.DocumentElement.AppendChild(export_server);
                    export_server = DomUtil.SetElementOuterXml(export_server, server.OuterXml);
                    export_server.RemoveAttribute("enabled");
                }

                dom.Save(dlg.FileName);
            }
        }

        private void toolStripButton_import_Click(object sender, EventArgs e)
        {
            string strError = "";

            try
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "请指定要导入的 XML 文件名";
                    // dlg.FileName = this.RecPathFilePath;
                    dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";
                    dlg.RestoreDirectory = true;

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;

                    XmlDocument dom = new XmlDocument();
                    dom.Load(dlg.FileName);

                    XmlNodeList servers = dom.DocumentElement.SelectNodes("server");
                    if (servers.Count == 0)
                    {
                        strError = $"文件 {dlg.FileName} 中不存在 */server 元素";
                        goto ERROR1;
                    }

                    FillServers(servers);
                    this.Changed = true;
                }
                return;
            }
            catch (Exception ex)
            {
                strError = "导入过程出现异常: " + ex.Message;
                goto ERROR1;
            }
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // parameters:
        void FillServers(XmlNodeList servers)
        {
            foreach (XmlElement server in servers)
            {
                string name = server.GetAttribute("name");
                string enabled = server.GetAttribute("enabled");

                ListViewItem item = new ListViewItem();
                if (server.OwnerDocument != _dom)
                {
                    // 对 server name 进行查重
                    if (SearchDup(ref name, null) == true)
                        server.SetAttribute("name", name);

                    XmlElement new_server = _dom.CreateElement("server");
                    _dom.DocumentElement.AppendChild(new_server);
                    DomUtil.SetElementOuterXml(new_server, server.OuterXml);
                    item.Tag = new_server;
                }
                else
                    item.Tag = server;

                ListViewUtil.ChangeItemText(item, COLUMN_NAME, name);
                ListViewUtil.ChangeItemText(item, COLUMN_DATABASE, ZServerUtil.GetDatabaseList(server));
                ListViewUtil.ChangeItemText(item, COLUMN_ENABLED, IsEnabled(enabled, true) ? "是" : "否");
                SetItemColor(item);
                this.listView1.Items.Add(item);
            }
        }

        static void SetItemColor(ListViewItem item)
        {
            var enabled = ListViewUtil.GetItemText(item, COLUMN_ENABLED);
            if (enabled == "是")
            {
                item.BackColor = System.Drawing.Color.DarkGreen;
                item.ForeColor = System.Drawing.Color.White;
            }    
            else
            {
                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;
            }
        }

        // return:
        //      true    发生过修改
        //      false   没有发生修改
        bool SearchDup(ref string name, ListViewItem exclude)
        {
            bool changed = false;
            while (true)
            {
                ListViewItem dup = ListViewUtil.FindItem(this.listView1, name, COLUMN_NAME);
                if (dup == null)
                    return changed;
                if (dup == exclude)
                    return changed;
                // 修改一下 name
                name = ModifyName(name);
                changed = true;
            }
        }

        // 修改一下名字。如果名字中含有数字，则增量这个数字。如果没有数字，则变换为 原名字1 形态
        static string ModifyName(string name)
        {
            SplitName(name, out string prefix, out string number);
            if (string.IsNullOrEmpty(number))
                return prefix + "1";
            if (int.TryParse(number, out int value) == false)
                return name + "1";
            return $"{prefix}{value + 1}";
        }

        static void SplitName(string name, out string prefix, out string number)
        {
            prefix = "";
            number = "";

            if (string.IsNullOrEmpty(name))
                return;

            for (int i = name.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(name[i]) == false)
                {
                    prefix = name.Substring(0, i + 1);
                    break;
                }
                number = name[i] + number;
            }
        }

        private void toolStripSplitButton_new1_ButtonClick(object sender, EventArgs e)
        {
            XmlElement server = _dom.CreateElement("server");
            _dom.DocumentElement.AppendChild(server);
            // server.SetAttribute("recsperbatch", "10");

            using (ZServerPropertyForm dlg = new ZServerPropertyForm())
            {
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.UnionCatalogPageVisible = false;
                dlg.XmlNode = server;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult == DialogResult.Cancel)
                {
                    server.ParentNode.RemoveChild(server);
                    return;
                }

                // 对 server name 进行查重
                string name = server.GetAttribute("name");
                if (SearchDup(ref name, null) == true)
                    server.SetAttribute("name", name);

                {
                    ListViewItem item = new ListViewItem();
                    item.Tag = server;
                    ListViewUtil.ChangeItemText(item, COLUMN_NAME, name);
                    ListViewUtil.ChangeItemText(item, COLUMN_DATABASE, ZServerUtil.GetDatabaseList(server));
                    ListViewUtil.ChangeItemText(item, COLUMN_ENABLED, "是");
                    SetItemColor(item);

                    this.listView1.Items.Add(item);
                }

                this.Changed = true;
            }
        }

        private void ToolStripMenuItem_new_hongniba_Click(object sender, EventArgs e)
        {
            string server_xml = @"<server 
name='红泥巴数字平台云' 
addr='58.87.101.80' 
port='210'
username='@hnb'>
    <database name='cbook' />
    <database name='ebook' />
</server>";

            CreateServer(server_xml);
        }

        void CreateServer(string server_xml)
        {
            XmlElement server = _dom.CreateElement("server");
            _dom.DocumentElement.AppendChild(server);
            server = DomUtil.SetElementOuterXml(server, server_xml);

            {
                // 对 server name 进行查重
                string name = server.GetAttribute("name");
                if (SearchDup(ref name, null) == true)
                    server.SetAttribute("name", name);

                {
                    ListViewItem item = new ListViewItem();
                    item.Tag = server;
                    ListViewUtil.ChangeItemText(item, COLUMN_NAME, name);
                    ListViewUtil.ChangeItemText(item, COLUMN_DATABASE, ZServerUtil.GetDatabaseList(server));
                    ListViewUtil.ChangeItemText(item, COLUMN_ENABLED, "是");
                    SetItemColor(item);

                    this.listView1.Items.Add(item);
                }

                this.Changed = true;
            }
        }

        private void ToolStripMenuItem_new_nlc_Click(object sender, EventArgs e)
        {
            // http://olcc.nlc.cn/news/73.html
            string server_xml = @"<server 
name='国图联编(UCS01U)' 
addr='202.96.31.28' 
port='9991' 
defaultEncoding='utf-8' 
queryTermEncoding='utf-8'>
    <database name='UCS01U' />
</server>";

            CreateServer(server_xml);

            MessageBox.Show(this, "该服务器需要用户名和密码才能访问，请您稍后用“修改”按钮设置");
        }

        private void ToolStripMenuItem_wangzhong_Click(object sender, EventArgs e)
        {
            string server_xml = @"<server 
name='网众' 
addr='118.25.225.224' 
port='2100'>
    <database name='uc_bib' />
    <database name='cipmarc' />
    <database name='ucs09' />
</server>";

            CreateServer(server_xml);

            MessageBox.Show(this, "该服务器需要用户名和密码才能访问，请您稍后用“修改”按钮设置");
        }
  
        private void ToolStripMenuItem_new_calis_Click(object sender, EventArgs e)
        {
            string server_xml = @"
  <server name='CALIS' addr='zserver1.calis.edu.cn' port='2200'>
    <database name='cn_cat' />
    <database name='we_cat' />
    <database name='jp_cat' />
    <database name='ru_cat' />
  </server>";

            CreateServer(server_xml);

            MessageBox.Show(this, "该服务器需要用户名和密码才能访问，请您稍后用“修改”按钮设置");
        }

        private void ToolStripMenuItem_new_nbinet_Click(object sender, EventArgs e)
        {
            string server_xml = @"
  <server name='NBINET' 
addr='nbinet3.ncl.edu.tw' 
port='210' 
defaultEncoding='utf-8' 
queryTermEncoding='utf-8'
detectmarcsyntax='1'
>
    <database name='innopac' />
  </server>";

            CreateServer(server_xml);
        }

        private void ToolStripMenuItem_new_lc_Click(object sender, EventArgs e)
        {
            string server_xml = @"
  <server name='Library of Congress' addr='lx2.loc.gov' port='210' defaultEncoding='utf-8' queryTermEncoding='utf-8'>
    <database name='LCDB' />
  </server>";

            CreateServer(server_xml);
        }
    }
}
