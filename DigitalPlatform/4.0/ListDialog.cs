using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalPlatform
{
    public partial class ListDialog : Form
    {
        public ListDialog()
        {
            InitializeComponent();
        }

        private void ListDialog_Load(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                this.TryInvoke(() =>
                {
                    this.listBox1.Focus();
                    if (this.listBox1.Items.Count > 0)
                        this.listBox1.SelectedIndex = 0;
                });
            });
        }

        private void ListDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ListDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listBox1.Items.Count > 0
                && this.listBox1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "请选择一个事项");
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

        // return:
        //      null    用户取消对话框
        //      其他    所输入的值
        public static string GetInput(
            Control owner,
            string strDlgTitle,
            string strTitle,
            IEnumerable<string> values,
            int default_index = -1,
            Font font = null)
        {
            return owner.TryGet(() =>
            {
                return GetInput(
                    (IWin32Window)owner,
                    strDlgTitle,
                    strTitle,
                    values,
                    default_index,
                    null,
                    null,
                    out _,
                    font);
            });
        }

        // return:
        //      null    用户取消对话框
        //      其他    所输入的值
        public static string GetInput(
            IWin32Window owner,
            string strDlgTitle,
            string strTitle,
            IEnumerable<string> values,
            int default_index = -1,
            Font font = null)
        {
            return GetInput(
            owner,
            strDlgTitle,
            strTitle,
            values,
            default_index,
            null,
            null,
            out _,
            font);
        }

#if REMOVED
        // return:
        //      null    用户取消对话框
        //      其他    所输入的值
        public static string GetInput(
            IWin32Window owner,
            string strDlgTitle,
            string strTitle,
            IEnumerable<string> values,
            int default_index = -1,
            Font font = null)
        {
            ListDialog dlg = new ListDialog();
            if (font != null)
                dlg.Font = font;

            if (strDlgTitle != null)
                dlg.Text = strDlgTitle;

            if (strTitle != null)
                dlg.label_listBox.Text = strTitle;

            if (values != null)
            {
                dlg.listBox1.Items.Clear();
                foreach(var s in values)
                {
                    dlg.listBox1.Items.Add(s);
                }
            }

            if (default_index != -1)
            {
                dlg.listBox1.SelectedIndex = default_index;
            }

            dlg.StartPosition = FormStartPosition.CenterScreen; // 2008/10/17

            if (owner == null)
                dlg.TopMost = true;

            dlg.ShowDialog(owner);

            if (dlg.DialogResult != DialogResult.OK)
                return null;

            return dlg.listBox1.SelectedItem as string;
        }

#endif

        public static string GetInput(
    Control owner,
    string strDlgTitle,
    string strTitle,
    IEnumerable<string> values,
    int default_index, // = -1,
    string text_title,  // 如果为 null 表示不改变 label 默认文字
    string text_default,    // 如果为 null 表示不启用 textbox
    out string text,
    Font font = null)
        {
            string temp = null;
            var ret = owner.TryGet(() =>
            {
                return GetInput(
                (IWin32Window)owner,
                strDlgTitle,
                strTitle,
                values,
                default_index, // = -1,
                text_title,
                text_default,    // 如果为 null 表示不启用 textbox
                out temp,
                font);
            });
            text = temp;
            return ret;
        }

        // 2023/8/24
        // 接受 textbox 输入的版本
        // return:
        //      null    用户取消对话框
        //      其他    所输入的值
        public static string GetInput(
            IWin32Window owner,
            string strDlgTitle,
            string strTitle,
            IEnumerable<string> values,
            int default_index, // = -1,
            string text_title,  // 如果为 null 表示不改变 label 默认文字
            string text_default,    // 如果为 null 表示不启用 textbox
            out string text,
            Font font = null)
        {
            text = null;

            ListDialog dlg = new ListDialog();
            if (font != null)
                dlg.Font = font;

            if (strDlgTitle != null)
                dlg.Text = strDlgTitle;

            if (strTitle != null)
                dlg.label_listBox.Text = strTitle;

            if (values != null)
            {
                dlg.listBox1.Items.Clear();
                foreach (var s in values)
                {
                    dlg.listBox1.Items.Add(s);
                }
            }

            if (default_index != -1)
            {
                dlg.listBox1.SelectedIndex = default_index;
            }

            if (text_title != null)
                dlg.label_textbox.Text = text_title;

            if (text_default != null)
            {
                dlg.textBox1.Visible = true;
                dlg.label_textbox.Visible = true;
                dlg.textBox1.Text = text_default;
            }

            dlg.StartPosition = FormStartPosition.CenterScreen; // 2008/10/17

            if (owner == null)
                dlg.TopMost = true;

            dlg.ShowDialog(owner);

            if (dlg.DialogResult != DialogResult.OK)
                return null;

            text = dlg.textBox1.Text;
            return dlg.listBox1.SelectedItem as string;
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(sender, e);
        }
    }
}
