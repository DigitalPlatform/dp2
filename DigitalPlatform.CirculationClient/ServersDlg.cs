using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

using DigitalPlatform.GUI;
using System.Collections.Generic;
using DigitalPlatform.Text;

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
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(574, 265);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(125, 33);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(574, 306);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(125, 31);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_newServer
            // 
            this.button_newServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_newServer.Location = new System.Drawing.Point(15, 265);
            this.button_newServer.Name = "button_newServer";
            this.button_newServer.Size = new System.Drawing.Size(188, 33);
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
            this.columnHeader_savePassword});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(15, 14);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(684, 244);
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
            // ServersDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(10, 21);
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

                item.SubItems.Add(server.Url);
                item.SubItems.Add(server.DefaultUserName);
                item.SubItems.Add(server.SavePassword == true ? "是" : "否");
            }
        }

        private void listView1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            bool bSelected = listView1.SelectedItems.Count > 0;

            //
            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyServer);
            if (bSelected == false)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteServer);
            if (bSelected == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            //
            menuItem = new MenuItem("新增(&N)");
            menuItem.Click += new System.EventHandler(this.menu_newServer);
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


        void menu_modifyServer(object sender, System.EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要修改的事项 ...");
                return;
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

            ((dp2Server)Servers[nActiveLine]).Name = dlg.ServerName;
            ((dp2Server)Servers[nActiveLine]).DefaultPassword = dlg.Password;
            ((dp2Server)Servers[nActiveLine]).Url = dlg.ServerUrl;
            ((dp2Server)Servers[nActiveLine]).DefaultUserName = dlg.UserName;
            ((dp2Server)Servers[nActiveLine]).SavePassword = dlg.SavePassword;

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
        }


        void menu_newServer(object sender, System.EventArgs e)
        {
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

            // 允许一次创建多个服务器节点
            int i = 0;
            foreach(string url in urls)
            {
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
                i++;
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

            m_bChanged = true;
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
            foreach(ListViewItem item in this.listView1.SelectedItems)
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
                foreach(ListViewItem item in this.listView1.Items)
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
}
