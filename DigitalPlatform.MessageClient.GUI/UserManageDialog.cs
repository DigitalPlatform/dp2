using DigitalPlatform.GUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

        private void UsersDialog_Load(object sender, EventArgs e)
        {
            listView1_SelectedIndexChanged(this, new EventArgs());

            this.BeginInvoke(new Action(ListAllUsers));
        }

        void ListAllUsers()
        {
            string strError = "";

            this.EnableControls(false);
            try
            {
                this.listView1.Items.Clear();

                int start = 0;
                while (true)
                {
                    var result = this.Connection.GetUsers("*", start, 100);
                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        goto ERROR1;
                    }
                    if (result.Users.Count == 0)
                        break;
                    start += result.Users.Count;

                    foreach (UserItem user in result.Users)
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
                strError = ex.Message;
            }
            finally
            {
                this.EnableControls(true);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {
            ListAllUsers();
        }

        private void toolStripButton_new_Click(object sender, EventArgs e)
        {
            NewUser();
        }

        void NewUser()
        {
            string strError = "";

            UserDialog dlg = new UserDialog();
            dlg.Font = this.Font;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
            List<UserItem> users = new List<UserItem>();
            UserItem user = dlg.UserItem;
            users.Add(user);

            this.EnableControls(false);
            try
            {
                this.Connection.SetUsers("create", users);
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
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
            MessageBox.Show(this, strError);
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
        ListViewItem ChangeItem(ListViewItem item_param, UserItem user)
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
            this.listView1.Enabled = bEnable;
            this.toolStrip1.Enabled = bEnable;
        }

        private void toolStripButton_modify_Click(object sender, EventArgs e)
        {
            ModifyUser();
        }

        void ModifyUser()
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
            dlg.UserItem = (UserItem)item.Tag;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            List<UserItem> users = new List<UserItem>();
            UserItem user = dlg.UserItem;
            users.Add(user);

            this.EnableControls(false);
            try
            {
                this.Connection.SetUsers("change", users);
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
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
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_delete_Click(object sender, EventArgs e)
        {
            this.DeleteUser();
        }

        void DeleteUser()
        {
            string strError = "";

            if (this.listView1.SelectedItems.Count == 0)
            {
                strError = "尚未选定要删除的事项";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
"确实要删除选定的 " + this.listView1.SelectedItems.Count.ToString() + " 个用户?",
"UserManageDialog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            List<UserItem> users = new List<UserItem>();
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                UserItem user = (UserItem)item.Tag;
                users.Add(user);
            }

            this.EnableControls(false);
            try
            {
                this.Connection.SetUsers("delete", users);
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
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
            MessageBox.Show(this, strError);
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
