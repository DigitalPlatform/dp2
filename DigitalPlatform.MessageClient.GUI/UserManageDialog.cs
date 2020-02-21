using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.GUI;

namespace DigitalPlatform.MessageClient
{
    public partial class UserManageDialog : Form
    {
        public MessageConnection Connection { get; set; }

        public bool Changed
        {
            get;
            set;
        }

        public UserManageDialog()
        {
            InitializeComponent();
        }

        private async void UsersDialog_Load(object sender, EventArgs e)
        {
            listView1_SelectedIndexChanged(this, new EventArgs());

            // this.BeginInvoke(new Action(ListAllUsers));
            await ListAllUsers();
        }

        async Task ListAllUsers()
        {
            string strError = "";

            this.EnableControls(false);
            try
            {
                this.listView1.Items.Clear();

                int start = 0;
                while (true)
                {
                    var result = await this.Connection.GetUsers("*", start, 100);
                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        goto ERROR1;
                    }
                    if (result.Users.Count == 0)
                        break;
                    start += result.Users.Count;

                    foreach (User user in result.Users)
                    {
#if NO
                        ListViewItem item = new ListViewItem();
                        item.SubItems.Add(user.userName);
                        item.SubItems.Add(user.department);
                        item.SubItems.Add(user.rights);

                        this.listView1.Items.Add(item);
#endif
                        ChangeItem(null, user);
                    }
                }
                return;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "ListAllUsers() 出现异常: " + ex.Message;
            }
            finally
            {
                this.EnableControls(true);
            }
            ERROR1:
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, strError);
            }
));
        }

        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {
            ListAllUsers();
        }

        private void toolStripButton_new_Click(object sender, EventArgs e)
        {
            NewUser();
        }

        async Task NewUser()
        {
            string strError = "";

            UserDialog dlg = new UserDialog();
            dlg.Font = this.Font;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
            List<User> users = new List<User>();
            User user = dlg.UserItem;
            users.Add(user);

            this.EnableControls(false);
            try
            {
                MessageResult result = await this.Connection.SetUsers("create", users);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "NewUser() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                this.EnableControls(true);
            }

            // 更新显示
            ListViewItem item = ChangeItem(null, user);
            this.listView1.SelectedItems.Clear();
            item.Selected = true;

            this.Changed = true;
            return;
            ERROR1:
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, strError);
            }
));
        }

        const int COLUMN_USERNAME = 0;
        const int COLUMN_DEPARTMENT = 1;
        const int COLUMN_RIGHTS = 2;
        const int COLUMN_DUTY = 3;
        const int COLUMN_TEL = 4;
        const int COLUMN_COMMENT = 5;

        // 修改 ListViewItem 值，或者新创建一个 ListViewItem
        // parameters:
        //      item_param  如果为 null，表示要新创建一个 item，否则就只用 user 来修改它的值
        ListViewItem ChangeItem(ListViewItem item_param, User user)
        {
            ListViewItem item = item_param;
            if (item == null)
                item = new ListViewItem();

            ListViewUtil.ChangeItemText(item, COLUMN_USERNAME, user.userName);
            ListViewUtil.ChangeItemText(item, COLUMN_DEPARTMENT, user.department);
            ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, user.rights);
            ListViewUtil.ChangeItemText(item, COLUMN_DUTY, user.duty);
            ListViewUtil.ChangeItemText(item, COLUMN_TEL, user.tel);
            ListViewUtil.ChangeItemText(item, COLUMN_COMMENT, user.comment);

            item.Tag = user;

            if (item_param == null)
                this.listView1.Items.Add(item);
            return item;
        }

        void EnableControls(bool bEnable)
        {
            this.Invoke((Action)(() =>
            {
                this.listView1.Enabled = bEnable;
                this.toolStrip1.Enabled = bEnable;
            }
));
        }

        private async void toolStripButton_modify_Click(object sender, EventArgs e)
        {
            await ModifyUser();
        }

        async Task ModifyUser()
        {
            string strError = "";

            if (this.listView1.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView1.SelectedItems[0];
            UserDialog dlg = new UserDialog();
            dlg.Font = this.Font;
            dlg.ChangeMode = true;
            dlg.UserItem = (User)item.Tag;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            if (dlg.Changed == false && dlg.ChangePassword == false)
            {
                MessageBox.Show(this, "没有发生修改");
                return;
            }

            List<User> users = new List<User>();
            User user = dlg.UserItem;
            users.Add(user);

            this.EnableControls(false);
            try
            {
                if (dlg.Changed == true)
                {
                    MessageResult result = await this.Connection.SetUsers("change", users);
                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        // 如果这里返回出错仅仅是因为权限不够，还需要尝试继续执行后面的修改密码的操作
                        if (result.String != "Denied")
                            goto ERROR1;
                    }
                }

                if (dlg.ChangePassword)
                {
                    MessageResult result = await this.Connection.SetUsers("changePassword", users);
                    if (result.Value == -1)
                    {
                        if (string.IsNullOrEmpty(strError) == false)
                            strError += "; ";
                        strError += result.ErrorInfo;
                        goto ERROR1;
                    }
                }

                if (string.IsNullOrEmpty(strError) == false)
                    goto ERROR1;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "ModifyUser() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                this.EnableControls(true);
            }

            ChangeItem(item, dlg.UserItem);

            this.Changed = true;
            return;
            ERROR1:
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, strError);
            }
));
        }

        private async void toolStripButton_delete_Click(object sender, EventArgs e)
        {
            await this.DeleteUser();
        }

        async Task DeleteUser()
        {
            string strError = "";

            if (this.listView1.SelectedItems.Count == 0)
            {
                strError = "尚未选定要删除的事项";
                goto ERROR1;
            }

            {
                DialogResult result = MessageBox.Show(this,
    "确实要删除选定的 " + this.listView1.SelectedItems.Count.ToString() + " 个用户?",
    "UserManageDialog",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;
            }

            List<User> users = new List<User>();
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                User user = (User)item.Tag;
                users.Add(user);
            }

            this.EnableControls(false);
            try
            {
                MessageResult result = await this.Connection.SetUsers("delete", users);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "DeleteUser() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                this.EnableControls(true);
            }

            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                this.listView1.Items.Remove(item);
            }

            this.Changed = true;
            return;
            ERROR1:
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, strError);
            }
));
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ModifyUser();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
            {
                this.toolStripButton_modify.Enabled = false;
                this.toolStripButton_delete.Enabled = false;
            }
            else
            {
                this.toolStripButton_modify.Enabled = true;
                this.toolStripButton_delete.Enabled = true;
            }
        }

#if NO
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView1);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView1);
                GuiState.SetUiState(controls, value);
            }
        }
#endif

    }
}
