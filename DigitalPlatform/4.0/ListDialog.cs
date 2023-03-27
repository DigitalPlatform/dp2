using System;
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

        }

        private void ListDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ListDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
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
            ListDialog dlg = new ListDialog();
            if (font != null)
                dlg.Font = font;

            if (strDlgTitle != null)
                dlg.Text = strDlgTitle;

            if (strTitle != null)
                dlg.label1.Text = strTitle;

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
    }
}
