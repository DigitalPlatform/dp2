using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

using Newtonsoft.Json;

using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using System.Management;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 管理 dp2library 服务器的对话框
    /// </summary>
    public class ServersDlg : System.Windows.Forms.Form
    {
        public string Mode = "config";  // config/select 两者之一

        public bool FirstRun = false;

        public dp2ServerCollection Servers = null;  // 引用

        bool m_bChanged = false;

        private DigitalPlatform.GUI.ListViewNF listView1;
        private System.Windows.Forms.ColumnHeader columnHeader_url;
        private System.Windows.Forms.ColumnHeader columnHeader_userName;
        private System.Windows.Forms.ColumnHeader columnHeader_savePassword;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private Button button_newServer;
        private ColumnHeader columnHeader_name;
        private IContainer components;
        private ColumnHeader columnHeader_uid;
        private MessageBalloon m_firstUseBalloon = null;


        public ServersDlg()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_newServer = new System.Windows.Forms.Button();
            this.listView1 = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_url = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_savePassword = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(560, 253);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 38);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(560, 300);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 35);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_newServer
            // 
            this.button_newServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_newServer.Location = new System.Drawing.Point(17, 253);
            this.button_newServer.Name = "button_newServer";
            this.button_newServer.Size = new System.Drawing.Size(206, 38);
            this.button_newServer.TabIndex = 3;
            this.button_newServer.Text = "新增服务器(&N)";
            this.button_newServer.UseVisualStyleBackColor = true;
            this.button_newServer.Click += new System.EventHandler(this.button_newServer_Click);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_url,
            this.columnHeader_userName,
            this.columnHeader_savePassword,
            this.columnHeader_uid});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(17, 16);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(681, 229);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "服务器名";
            this.columnHeader_name.Width = 200;
            // 
            // columnHeader_url
            // 
            this.columnHeader_url.Text = "服务器URL";
            this.columnHeader_url.Width = 300;
            // 
            // columnHeader_userName
            // 
            this.columnHeader_userName.Text = "用户名";
            this.columnHeader_userName.Width = 150;
            // 
            // columnHeader_savePassword
            // 
            this.columnHeader_savePassword.Text = "是否保存密码";
            this.columnHeader_savePassword.Width = 150;
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 200;
            // 
            // ServersDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(11, 24);
            this.ClientSize = new System.Drawing.Size(714, 352);
            this.Controls.Add(this.button_newServer);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView1);
            this.Name = "ServersDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "dp2library 服务器和默认帐户管理";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ServersDlg_Closing);
            this.Load += new System.EventHandler(this.ServersDlg_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private void ServersDlg_Load(object sender, System.EventArgs e)
        {
            FillList();

            if (this.Mode == "select")
            {
                this.button_newServer.Visible = false;
                RefreshButtons();
            }

            if (this.FirstRun == true)
            {
                // this.toolTip_firstUse.Show("请按此按钮创建一个新的服务器目标", this.button_newServer);
                ShowMessageTip();
            }
        }

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            // OK和Cancel退出本对话框,其实 Servers中的内容已经修改。
            // 为了让Cancel退出有放弃整体修改的效果，请调主在初始化Servers
            // 属性的时候用一个克隆的ServerCollection对象。

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        const int COLUMN_NAME = 0;
        const int COLUMN_URL = 1;
        const int COLUMN_USERNAME = 2;
        const int COLUMN_SAVEPASSWORD = 3;
        const int COLUMN_UID = 4;

        void FillList()
        {
            listView1.Items.Clear();

            if (Servers == null)
                return;

            for (int i = 0; i < Servers.Count; i++)
            {
                dp2Server server = (dp2Server)Servers[i];

                ListViewItem item = new ListViewItem(server.Name, 0);

                listView1.Items.Add(item);

                ListViewUtil.ChangeItemText(item, COLUMN_URL, server.Url);
                ListViewUtil.ChangeItemText(item, COLUMN_USERNAME, server.DefaultUserName);
                ListViewUtil.ChangeItemText(item, COLUMN_SAVEPASSWORD, server.SavePassword == true ? "是" : "否");
                ListViewUtil.ChangeItemText(item, COLUMN_UID, server.UID);
                //item.SubItems.Add(server.Url);
                //item.SubItems.Add(server.DefaultUserName);
                //item.SubItems.Add(server.SavePassword == true ? "是" : "否");
            }
        }

        private void listView1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            bool bSelected = listView1.SelectedItems.Count > 0;
            bool bReadOnly = (this.Mode == "select");

            //
            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyServer);
            if (bSelected == false || bReadOnly)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteServer);
            if (bSelected == false || bReadOnly)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            //
            menuItem = new MenuItem("新增(&N)");
            menuItem.Click += new System.EventHandler(this.menu_newServer);
            if (bReadOnly)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyJSONtoClipboard);
            if (bSelected == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴 [前插](&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromJSONClipboard);
            if (Clipboard.ContainsText() == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新 UID(&R)");
            menuItem.Click += new System.EventHandler(this.menu_refreshUID);
            if (bSelected == false || bReadOnly)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);




            contextMenu.Show(listView1, new Point(e.X, e.Y));
        }

        void menu_deleteServer(object sender, System.EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要删除的事项 ...");
                return;
            }

            DialogResult msgResult = MessageBox.Show(this,
                "确实要删除所选择的事项",
                "ServersDlg",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (msgResult != DialogResult.Yes)
            {
                return;
            }

            for (int i = listView1.SelectedIndices.Count - 1; i >= 0; i--)
            {
                Servers.RemoveAt(listView1.SelectedIndices[i]);
            }

            Servers.Changed = true;

            FillList();

            m_bChanged = true;
        }

        // 从 Windows 剪贴板中粘贴 JSON 格式
        void menu_pasteFromJSONClipboard(object sender, System.EventArgs e)
        {
            string strError;

            try
            {
                // 插入位置
                int index = this.listView1.Items.IndexOf(this.listView1.FocusedItem);
                if (index == -1 && this.listView1.SelectedIndices.Count > 0)
                    index = this.listView1.SelectedIndices[0];

                List<int> indices = new List<int>();
                string value = Clipboard.GetText();
                // Newtonsoft.Json.JsonReaderException
                var servers = JsonConvert.DeserializeObject<List<CopyServer>>(value);
                foreach (var source in servers)
                {
                    dp2Server server = Servers.NewServer(index);
                    source.CopyTo(server);
                    indices.Add(index);
                    index++;
                }

                if (indices.Count > 0)
                {
                    Servers.Changed = true;
                    FillList();
                    foreach (int i in indices)
                    {
                        var item = this.listView1.Items[i];
                        item.Selected = true;
                        item.EnsureVisible();
                    }
                }
                return;
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                strError = "剪贴板中的内容不是特定格式";
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_copyJSONtoClipboard(object sender, System.EventArgs e)
        {
            string strError;
            if (listView1.SelectedIndices.Count == 0)
            {
                strError = "尚未选择要刷新 UID 的事项 ...";
                goto ERROR1;
            }

            List<CopyServer> servers = new List<CopyServer>();
            foreach (int index in this.listView1.SelectedIndices)
            {
                dp2Server server = Servers[index] as dp2Server;
                servers.Add(CopyServer.From(server));
            }

            string value = JsonConvert.SerializeObject(servers, Formatting.Indented);
            Clipboard.SetText(value);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        async void menu_refreshUID(object sender, System.EventArgs e)
        {
            string strError = "";
            if (listView1.SelectedIndices.Count == 0)
            {
                strError = "尚未选择要刷新 UID 的事项 ...";
                goto ERROR1;
            }

            int change_count = 0;
            List<string> errors = new List<string>();

            using (MessageBar bar = MessageBar.Create(this, "正在刷新 UID"))
            {
                foreach (int index in this.listView1.SelectedIndices)
                {
                    dp2Server server = Servers[index] as dp2Server;

                    // 获得服务器 UID
                    string uid = "";
                    bar.SetMessageText($"正在获取服务器 {server.Url} 的 UID ...");
                    var result = await ServerDlg.GetServerUID(server.Url);
                    if (result.Value == -1)
                        errors.Add($"针对服务器 {server.Url} 获取服务器 UID 时出错: {result.ErrorInfo}");
                    else
                        uid = result.ErrorCode;

                    if (server.UID != uid)
                    {
                        server.UID = uid;
                        change_count++;
                    }
                }

                // TODO: 刷新后如果发现发生了 UID 重复，怎么处理?

                if (change_count > 0)
                {
                    Servers.Changed = true;
                    FillList();
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        async void menu_modifyServer(object sender, System.EventArgs e)
        {
            string strError;

            if (listView1.SelectedIndices.Count == 0)
            {
                strError = "尚未选择要修改的事项 ...";
                goto ERROR1;
            }

            int nActiveLine = listView1.SelectedIndices[0];
            // ListViewItem item = listView1.Items[nActiveLine];

            ServerDlg dlg = new ServerDlg();
            // GuiUtil.AutoSetDefaultFont(dlg); 
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "修改缺省帐户参数";

            dlg.ServerName = ((dp2Server)Servers[nActiveLine]).Name;
            dlg.Password = ((dp2Server)Servers[nActiveLine]).DefaultPassword;
            dlg.ServerUrl = ((dp2Server)Servers[nActiveLine]).Url;
            dlg.UserName = ((dp2Server)Servers[nActiveLine]).DefaultUserName;
            dlg.SavePassword = ((dp2Server)Servers[nActiveLine]).SavePassword;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            dp2Server server = ((dp2Server)Servers[nActiveLine]);

            server.Name = dlg.ServerName;
            server.DefaultPassword = dlg.Password;
            server.Url = dlg.ServerUrl;
            server.DefaultUserName = dlg.UserName;
            server.SavePassword = dlg.SavePassword;

            // 获取 UID
            using (MessageBar bar = MessageBar.Create(this,
                "获取 UID",
                $"正在获取服务器 {server.Url} 的 UID ..."))
            {
                var result = await ServerDlg.GetServerUID(server.Url);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
                server.UID = result.ErrorCode;
            }

            Servers.Changed = true;

            FillList();

            // 选择一行
            // parameters:
            //		nIndex	要设置选择标记的行。如果==-1，表示清除全部选择标记但不选择。
            //		bMoveFocus	是否同时移动focus标志到所选择行
            ListViewUtil.SelectLine(listView1,
                nActiveLine,
                true);

            m_bChanged = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        async void menu_newServer(object sender, System.EventArgs e)
        {
            List<string> errors = new List<string>();

            int nActiveLine = -1;
            if (listView1.SelectedIndices.Count != 0)
            {
                nActiveLine = listView1.SelectedIndices[0];
            }

            ServerDlg dlg = new ServerDlg();
            // GuiUtil.AutoSetDefaultFont(dlg); 
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "新增服务器地址和默认帐户";

            if (nActiveLine == -1)
            {
                // 无参考事项情形的新增
#if NO
                dlg.ServerName = "社科院联合编目中心";
                dlg.ServerUrl = "http://ssucs.org/dp2library";
                dlg.UserName = "test";
#endif
                dlg.ServerName = "单机版服务器";
                dlg.ServerUrl = "net.pipe://localhost/dp2library/xe";
                dlg.UserName = "supervisor";
            }
            else
            {
                dp2Server server = (dp2Server)Servers[nActiveLine];
                dlg.ServerName = server.Name;
                dlg.Password = server.DefaultPassword;
                dlg.ServerUrl = server.Url;
                dlg.UserName = server.DefaultUserName;
                dlg.SavePassword = server.SavePassword;
            }

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            List<string> urls = new List<string>();
            if (dlg.ServerUrl.IndexOf("\r\n") == -1)
                urls.Add(dlg.ServerUrl);
            else
            {
                urls = StringUtil.SplitList(dlg.ServerUrl.Replace("\r\n", "\r"), '\r');
                StringUtil.RemoveBlank(ref urls);
                StringUtil.RemoveDupNoSort(ref urls);
            }

            using (MessageBar bar = MessageBar.Create(this, "正在添加服务器节点"))
            {
                // 允许一次创建多个服务器节点
                int i = 0;
                foreach (string url in urls)
                {
                    // 获得服务器 UID
                    string uid = "";
                    bar.SetMessageText($"正在获取服务器 {url} 的 UID ...");
                    var result = await ServerDlg.GetServerUID(url);
                    if (result.Value == -1)
                        errors.Add($"针对服务器 {url} 获取服务器 UID 时出错: {result.ErrorInfo}");
                    else
                        uid = result.ErrorCode;

                    // 对 UID 进行查重
                    if (string.IsNullOrEmpty(uid) == false)
                    {
                        var dup_list = Servers.FindServerByUID(uid);
                        if (dup_list.Count > 0)
                        {
                            errors.Add($"拟添加的新服务器节点 '{url}' 因其 UID '{uid}' 和已有的服务器节点({dup_list[0].Name})重复，无法添加");
                            continue;
                        }
                    }

                    dp2Server server = Servers.NewServer(nActiveLine);
                    // TODO: 建议用 public 账户尝试从 dp2library 服务器获得服务器名字符串
                    string name = dlg.ServerName;
                    if (i > 0)
                        name = dlg.ServerName + (i + 1).ToString();
                    server.Name = name;
                    server.DefaultPassword = dlg.Password;
                    server.Url = url;
                    server.DefaultUserName = dlg.UserName;
                    server.SavePassword = dlg.SavePassword;
                    server.UID = uid;

                    i++;
                }

            }

            Servers.Changed = true;

            FillList();

            // 选择一行
            // parameters:
            //		nIndex	要设置选择标记的行。如果==-1，表示清除全部选择标记但不选择。
            //		bMoveFocus	是否同时移动focus标志到所选择行
            ListViewUtil.SelectLine(listView1,
                Servers.Count - 1,
                true);
            listView1.EnsureVisible(Servers.Count - 1);

            m_bChanged = true;

            if (errors.Count > 0)
                MessageDlg.Show(this, $"新增服务器节点时出错:\r\n{StringUtil.MakePathList(errors, "\r\n")}", "ServersDlg");
        }

        private void listView1_DoubleClick(object sender, System.EventArgs e)
        {
            menu_modifyServer(null, null);
        }

        private void ServersDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
            {
                if (m_bChanged == true)
                {
                    DialogResult msgResult = MessageBox.Show(this,
                        "要放弃在对话框中所做的全部修改么?",
                        "ServersDlg",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (msgResult == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        private void button_newServer_Click(object sender, EventArgs e)
        {
            HideMessageTip();

            menu_newServer(null, null);
        }

        void ShowMessageTip()
        {
            m_firstUseBalloon = new MessageBalloon();
            m_firstUseBalloon.Parent = this.button_newServer;
            m_firstUseBalloon.Title = "欢迎使用dp2编目前端";
            m_firstUseBalloon.TitleIcon = TooltipIcon.Info;
            m_firstUseBalloon.Text = "请按此按钮创建第一个 dp2library 服务器目标";

            m_firstUseBalloon.Align = BalloonAlignment.BottomRight;
            m_firstUseBalloon.CenterStem = false;
            m_firstUseBalloon.UseAbsolutePositioning = false;
            m_firstUseBalloon.Show();
        }

        void HideMessageTip()
        {
            if (m_firstUseBalloon == null)
                return;

            m_firstUseBalloon.Dispose();
            m_firstUseBalloon = null;
        }

        List<string> GetSelectedPathList()
        {
            List<string> results = new List<string>();
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                results.Add(ListViewUtil.GetItemText(item, 0));
            }

            return results;
        }

        public List<string> SelectedServerNames
        {
            get
            {
                return GetSelectedPathList();
            }
            set
            {
                foreach (ListViewItem item in this.listView1.Items)
                {
                    string name = ListViewUtil.GetItemText(item, 0);
                    if (value == null || value.IndexOf(name) == -1)
                        item.Selected = false;
                    else
                        item.Selected = true;
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshButtons();
        }

        void RefreshButtons()
        {
            if (this.Mode == "select")
            {
                if (this.listView1.SelectedItems.Count > 0)
                    this.button_OK.Enabled = true;
                else
                    this.button_OK.Enabled = false;
            }
        }
    }

    // 用于复制粘贴的 Server 格式
    public class CopyServer
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string UID { get; set; }
        public string UserName { get; set; }
        public bool SavePassword { get; set; }
        public string EncryptPassword { get; set; }

        static string _key = null;

        // key 字符串根据 CPU ID 确定，增强安全性
        public static string GetKey()
        {
            if (_key == null)
                _key = StringUtil.MakePathList(GetCpuID(), "|");
            return _key;
        }

        public static List<string> GetCpuID()
        {
            ManagementClass managClass = new ManagementClass("win32_processor");
            using (ManagementObjectCollection managCollec = managClass.GetInstances())
            {
                List<string> results = new List<string>();
                foreach (ManagementObject managObj in managCollec)
                {
                    results.Add(managObj.Properties["processorID"].Value.ToString());
                }
                return results;
            }
        }

        public string GetPassword()
        {
            if (this.SavePassword == true)
            {
                try
                {
                    return Cryptography.Decrypt(this.EncryptPassword, GetKey());
                }
                catch
                {
                    return Guid.NewGuid().ToString();
                }
            }

            return "";
        }

        public static CopyServer From(dp2Server server)
        {
            CopyServer result = new CopyServer
            {
                Name = server.Name,
                Url = server.Url,
                UID = server.UID,
                UserName = server.DefaultUserName,
                SavePassword = server.SavePassword,
                EncryptPassword = server.SavePassword == false ? null :
                Cryptography.Encrypt(server.DefaultPassword, GetKey()),
            };
            return result;
        }

        public void CopyTo(dp2Server server)
        {
            var source = this;
            server.Name = source.Name;
            server.Url = source.Url;
            server.UID = source.UID;
            server.DefaultUserName = source.UserName;
            server.SavePassword = source.SavePassword;
            server.DefaultPassword = source.GetPassword();
        }
    }
}
