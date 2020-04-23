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

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;

namespace dp2ManageCenter.Message
{
    public partial class MessageAccountForm : Form
    {
        // 配置文件名
        // public string CfgFileName { get; set; }

        public string Mode { get; set; }    // select/空

        public Account SelectedAccount { get; set; }

        public MessageAccountForm()
        {
            InitializeComponent();
        }

        public static string MessageAccountFileName
        {
            get
            {
                return Path.Combine(ClientInfo.UserDir, "message_accounts.json");
            }
        }

        private void MessageAccountForm_Load(object sender, EventArgs e)
        {
            FillList();

            if (this.Mode == "select")
            {
                listView_accounts_SelectedIndexChanged(sender, e);
            }
        }

        private void MessageAccountForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MessageAccountForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            SaveList();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public static List<Account> GetAccounts()
        {
            try
            {
                string value = File.ReadAllText(MessageAccountFileName, Encoding.UTF8);
                var accounts = JsonConvert.DeserializeObject<List<Account>>(value);
                if (accounts == null)
                    accounts = new List<Account>();
                return accounts;
            }
            catch(FileNotFoundException ex)
            {
                return new List<Account>();
            }
        }

        void FillList()
        {
            this.listView_accounts.Items.Clear();

            try
            {
                /*
                string value = File.ReadAllText(this.CfgFileName, Encoding.UTF8);
                var accounts = JsonConvert.DeserializeObject<List<Account>>(value);
                if (accounts == null)
                    accounts = new List<Account>();
                    */
                var accounts = GetAccounts();

                foreach (var account in accounts)
                {
                    AddItem(account);
                }
            }
            catch (DirectoryNotFoundException)
            {
                return;
            }
            catch (FileNotFoundException)
            {
                return;
            }
        }

        void AddItem(Account account)
        {
            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, COLUMN_SERVERURL, account.ServerUrl);
            ListViewUtil.ChangeItemText(item, COLUMN_USERNAME, account.UserName);
            item.Tag = account;
            this.listView_accounts.Items.Add(item);
        }

        void SaveList()
        {
            List<Account> accounts = new List<Account>();
            foreach (ListViewItem item in this.listView_accounts.Items)
            {
                var account = item.Tag as Account;
                if (account.SavePassword == false)
                    account.EncryptPassword = null;
                accounts.Add(account);
            }

            string value = JsonConvert.SerializeObject(accounts, Formatting.Indented);
            PathUtil.CreateDirIfNeed(Path.GetDirectoryName(MessageAccountFileName));
            File.WriteAllText(MessageAccountFileName, value);
        }

        private void listView_accounts_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            /*
            menuItem = new MenuItem("全选(&A)");
            menuItem.Tag = this.listView_operLogTasks;
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);
            */

            menuItem = new MenuItem("新建账户 (&A)");
            menuItem.Click += new System.EventHandler(this.MenuItem_newAccount_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("修改账户 [" + this.listView_accounts.SelectedItems.Count.ToString() + "] (&M)");
            menuItem.Click += new System.EventHandler(this.MenuItem_modifyAccount_Click);
            if (this.listView_accounts.SelectedItems.Count != 1)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除账户 [" + this.listView_accounts.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.MenuItem_deleteAccounts_Click);
            if (this.listView_accounts.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_accounts, new Point(e.X, e.Y));
        }

        const int COLUMN_SERVERURL = 0;
        const int COLUMN_USERNAME = 1;

        // 新建账户
        void MenuItem_newAccount_Click(object sender, EventArgs e)
        {
            using (LoginDlg dlg = new LoginDlg())
            {
                dlg.ServerUrl = "http://dp2003.com:8083/dp2mserver";
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                Account account = new Account
                {
                    ServerUrl = dlg.ServerUrl,
                    UserName = dlg.UserName,
                    Password = dlg.Password,
                    SavePassword = dlg.SavePassword,
                };

                if (account.SavePassword == false)
                    account.EncryptPassword = null;

                AddItem(account);
                /*
                {
                    ListViewItem item = new ListViewItem();
                    ListViewUtil.ChangeItemText(item, COLUMN_SERVERURL, dlg.ServerUrl);
                    ListViewUtil.ChangeItemText(item, COLUMN_USERNAME, dlg.UserName);
                    Account account = new Account
                    {
                        ServerUrl = dlg.ServerUrl,
                        UserName = dlg.UserName,
                        Password = dlg.Password
                    };
                    item.Tag = account;
                    this.listView_accounts.Items.Add(item);
                }
                */
            }
        }

        // 修改所选择的账户
        void MenuItem_modifyAccount_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_accounts.SelectedItems.Count != 1)
            {
                strError = "请选择一个需要修改的账户行";
                goto ERROR1;
            }

            ListViewItem item = this.listView_accounts.SelectedItems[0];
            using (LoginDlg dlg = new LoginDlg())
            {
                Account account = item.Tag as Account;
                dlg.ServerUrl = account.ServerUrl;
                dlg.UserName = account.UserName;
                dlg.Password = account.Password;
                dlg.SavePassword = account.SavePassword;

                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                account.ServerUrl = dlg.ServerUrl;
                account.UserName = dlg.UserName;
                account.Password = dlg.Password;
                account.SavePassword = dlg.SavePassword;

                if (account.SavePassword == false)
                    account.EncryptPassword = null;

                {
                    ListViewUtil.ChangeItemText(item, COLUMN_SERVERURL, account.ServerUrl);
                    ListViewUtil.ChangeItemText(item, COLUMN_USERNAME, account.UserName);
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 删除所选择的账户
        void MenuItem_deleteAccounts_Click(object sender, EventArgs e)
        {
            ListViewUtil.DeleteSelectedItems(this.listView_accounts);
        }

        private void listView_accounts_DoubleClick(object sender, EventArgs e)
        {
            MenuItem_modifyAccount_Click(sender, e);
        }

        private void listView_accounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_accounts.SelectedItems.Count != 1)
            {
                this.SelectedAccount = null;
                if (this.Mode == "select")
                    this.button_OK.Enabled = false;
            }
            else
            {
                this.SelectedAccount = this.listView_accounts.SelectedItems[0].Tag as Account;
                if (this.Mode == "select")
                    this.button_OK.Enabled = true;
            }
        }
    }

    public class Account
    {
        public string ServerUrl { get; set; }
        public string UserName { get; set; }
        public bool SavePassword { get; set; }
        public string EncryptPassword { get; set; }

        public string Password
        {
            get
            {
                try
                {
                    if (this.EncryptPassword == null)
                        return "";

                    return Cryptography.Decrypt(EncryptPassword, "message_key");
                }
                catch
                {
                    return "errorpassword";
                }
            }
            set
            {
                EncryptPassword = Cryptography.Encrypt(value, "message_key");
            }
        }
    }
}
