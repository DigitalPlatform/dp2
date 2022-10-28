using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace dp2Circulation.Reader
{
    public partial class PropertyTableDialog : Form
    {
        // 合法的参数名列表
        // 注: 用空字符串元素表示允许默认参数名，默认集合第一个元素。例如 "name1,name2," 表示默认 name1
        List<string> _propertyNameList = new List<string>();
        public List<string> PropertyNameList
        {
            get
            {
                return _propertyNameList;
            }
            set
            {
                this._propertyNameList.Clear();
                if (value != null)
                    this._propertyNameList.AddRange(value);

                _default_caption = BuildDefaultCaption(this._propertyNameList);
            }
        }

        public string PropertyString
        {
            get
            {
                return GetPropertyString();
            }
            set
            {
                SetPropertyString(value);
            }
        }

        public static string BuildDefaultCaption(List<string> nameList)
        {
            if (nameList.Count > 0
    && PropertyTableLineDialog.AllowEmptyName(nameList))
                return $"[默认:{nameList[0]}]";
            else
                return "";
        }

        public PropertyTableDialog()
        {
            InitializeComponent();
        }

        private void PropertyTableDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            var control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            if (control == false)
            {
                // 检查每行是否有错误
                List<string> errors = new List<string>();
                int number = 1;
                foreach (ListViewItem item in this.listView1.Items)
                {
                    var result = VerifyItem(item);
                    if (string.IsNullOrEmpty(result) == false)
                        errors.Add($"行 {number}: {result}");

                    number++;
                }

                if (errors.Count > 0)
                {
                    MessageDlg.Show(this, StringUtil.MakePathList(errors, "\r\n"), "发现参数错误");
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        string _default_caption = "";

        void SetPropertyString(string text)
        {
            this.listView1.Items.Clear();

            var segments = text.Split(',');
            foreach (string s in segments)
            {
                string name;
                string value;
                if (s.Contains(":") == false)
                {
                    name = _default_caption;
                    value = s.Trim();
                }
                else
                {
                    var parts = StringUtil.ParseTwoPart(s, ":");
                    name = parts[0];
                    value = parts[1];
                }

                ListViewItem item = new ListViewItem(name);
                ListViewUtil.ChangeItemText(item, 1, value);
                this.listView1.Items.Add(item);

                VerifyItem(item);
            }
        }

        string GetPropertyString()
        {
            List<string> results = new List<string>();
            foreach (ListViewItem item in this.listView1.Items)
            {
                string name = ListViewUtil.GetItemText(item, 0);
                string value = ListViewUtil.GetItemText(item, 1);
                if (name == _default_caption || string.IsNullOrEmpty(name))
                    results.Add(value);
                else
                    results.Add($"{name}:{value}");
            }

            return StringUtil.MakePathList(results, ",");
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("新增");
            menuItem.Click += new System.EventHandler(this.menu_newLine_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("修改");
            menuItem.Click += new System.EventHandler(this.menu_modifyLine_Click);
            if (this.listView1.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("上移");
            menuItem.Click += new System.EventHandler(this.menu_moveUp_Click);
            if (this.listView1.SelectedItems.Count == 0
                || this.listView1.SelectedIndices[0] == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("下移");
            menuItem.Click += new System.EventHandler(this.menu_moveDown_Click);
            if (this.listView1.SelectedItems.Count == 0
                || this.listView1.SelectedIndices[0] >= this.listView1.Items.Count - 1)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("删除");
            menuItem.Click += new System.EventHandler(this.menu_deleteLines_Click);
            if (this.listView1.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView1, new Point(e.X, e.Y));
        }

        void menu_moveUp_Click(object sender, EventArgs e)
        {
            ListViewUtil.MoveItemUpDown(this.listView1,
                true,
                out _,
                out string strError);
        }

        void menu_moveDown_Click(object sender, EventArgs e)
        {
            ListViewUtil.MoveItemUpDown(this.listView1,
                false,
                out _,
                out string strError);
        }

        void menu_newLine_Click(object sender, EventArgs e)
        {
            using (PropertyTableLineDialog dlg = new PropertyTableLineDialog())
            {
                dlg.PropertyNameList = this.PropertyNameList;
                dlg.Font = this.Font;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.Cancel)
                    return;

                ListViewItem item = new ListViewItem(GetDisplayName(dlg.ParameterName));
                ListViewUtil.ChangeItemText(item, 1, dlg.ParameterValue);
                this.listView1.Items.Add(item);
                ListViewUtil.SelectLine(item, true);
                VerifyItem(item);
            }
        }

        string GetDisplayName(string name)
        {
            return string.IsNullOrEmpty(name) ? _default_caption : name;
        }

        string GetPureName(string caption)
        {
            if (caption == _default_caption)
                return "";
            return caption;
        }

        void menu_modifyLine_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView1.SelectedItems.Count == 0)
            {
                strError = "尚未选择要修改的事项";
                goto ERROR1;
            }
            ListViewItem item = this.listView1.SelectedItems[0];

            using (PropertyTableLineDialog dlg = new PropertyTableLineDialog())
            {
                dlg.PropertyNameList = this.PropertyNameList;
                dlg.Font = this.Font;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ParameterName = GetPureName(ListViewUtil.GetItemText(item, 0));
                dlg.ParameterValue = ListViewUtil.GetItemText(item, 1);
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.Cancel)
                    return;

                ListViewUtil.ChangeItemText(item, 0, GetDisplayName(dlg.ParameterName));
                ListViewUtil.ChangeItemText(item, 1, dlg.ParameterValue);
                ListViewUtil.SelectLine(item, true);
                VerifyItem(item);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_deleteLines_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView1.SelectedItems.Count == 0)
            {
                strError = "尚未选择要删除的事项";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
                $"确实要删除所选择的 {this.listView1.SelectedItems.Count} 行参数？",
                "PropertyTableDialog",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            ListViewUtil.DeleteSelectedItems(this.listView1);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count == 0)
            {
                this.toolStripButton_delete.Enabled = false;
                this.toolStripButton_modify.Enabled = false;
            }
            else
            {
                this.toolStripButton_delete.Enabled = true;
                this.toolStripButton_modify.Enabled = true;
            }

            if (this.listView1.SelectedItems.Count == 0
                || this.listView1.SelectedIndices[0] == 0)
                this.toolStripButton_moveUp.Enabled = false;
            else
                this.toolStripButton_moveUp.Enabled = true;

            if (this.listView1.SelectedItems.Count == 0
                || this.listView1.SelectedIndices[0] >= this.listView1.Items.Count - 1)
                this.toolStripButton_moveDown.Enabled = false;
            else
                this.toolStripButton_moveDown.Enabled = true;

        }

        private void toolStripButton_new_Click(object sender, EventArgs e)
        {
            menu_newLine_Click(sender, e);
        }

        private void toolStripButton_modify_Click(object sender, EventArgs e)
        {
            menu_modifyLine_Click(sender, e);
        }

        private void toolStripButton_delete_Click(object sender, EventArgs e)
        {
            menu_deleteLines_Click(sender, e);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            menu_modifyLine_Click(sender, e);
        }

        public static List<string> VerifyString(
            string validNameList,
            string text)
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrEmpty(validNameList))
                return errors;
            if (string.IsNullOrEmpty(text))
                return errors;
            var propertyNameList = StringUtil.SplitList(validNameList);
            var segments = text.Split(',');
            foreach (string s in segments)
            {
                // 没有冒号的，被当作 email 值
                if (s.Contains(":") == false)
                {
                    // TODO: email 地址格式校验
                    continue;
                }

                var parts = StringUtil.ParseTwoPart(s, ":");
                string name = parts[0];
                string value = parts[1];

                if (propertyNameList.IndexOf(name) == -1)
                {
                    errors.Add($"子参数 '{s}' 中参数名 '{name}' 部分不合法");
                }
            }

            return errors;
        }

        string VerifyItem(ListViewItem item)
        {
            // 当前没有 PropertyNameList，无法校验
            if (this._propertyNameList == null || this._propertyNameList.Count == 0)
                return null;

            string error;
            string name = GetPureName(ListViewUtil.GetItemText(item, 0));
            string value = ListViewUtil.GetItemText(item, 1);
            if (this._propertyNameList.IndexOf(name) == -1)
            {
                item.ForeColor = Color.White;
                item.BackColor = Color.DarkRed;
                error = $"参数名 '{name}' 不合法";
            }
            else
            {
                item.ForeColor = SystemColors.WindowText;
                item.BackColor = SystemColors.Window;
                error = "";
            }
            ListViewUtil.ChangeItemText(item, 2, error);
            return error;
        }

        private void toolStripButton_moveUp_Click(object sender, EventArgs e)
        {
            menu_moveUp_Click(sender, e);
        }

        private void toolStripButton_moveDown_Click(object sender, EventArgs e)
        {
            menu_moveDown_Click(sender, e);
        }

        // 处理 "name:value1,value2" 形态的字符串，为缺乏子参数名的添加默认子参数名
        public static string AddDefaultName(
            string text,
            string default_name)
        {
            List<string> results = new List<string>();
            var segments = text.Split(',');
            foreach (string s in segments)
            {
                if (string.IsNullOrEmpty(s) == false
                    && s.Contains(":") == false)
                    results.Add($"{default_name}:{s}");
                else
                    results.Add(s);
            }

            return string.Join(",", results.ToArray());
        }

    }
}
