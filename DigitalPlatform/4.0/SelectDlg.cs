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
    /// <summary>
    /// 通用多项选择对话框
    /// 2021/12/18 开始编写
    /// </summary>
    public partial class SelectDlg : Form
    {
        public SelectDlg()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex < 0
                && this.listBox1.SelectedIndex >= this.listBox1.Items.Count)
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
        //      其他    所选择的值
        public static string GetSelect(
            IWin32Window owner,
            string strDlgTitle,
            string strTitle,
            string[] values,
            int default_index,
            string strCheckBoxText,
            ref bool bCheckBox,
            Font font = null)
        {
            SelectDlg dlg = new SelectDlg();
            if (font != null)
                dlg.Font = font;

            if (strDlgTitle != null)
                dlg.Text = strDlgTitle;

            if (strTitle != null)
                dlg.label_title.Text = strTitle;

            dlg.Values = values;

            if (default_index >= 0)
                dlg.SelectIndex = default_index;

            if (strCheckBoxText == null)
                dlg.CheckBoxVisible = false;
            else
            {
                dlg.CheckBoxVisible = true;
                dlg.CheckBoxText = strCheckBoxText;
            }

            dlg.CheckBoxValue = bCheckBox;
            dlg.StartPosition = FormStartPosition.CenterScreen; // 2008/10/17

            if (owner == null)
                dlg.TopMost = true;

            dlg.ShowDialog(owner);

            if (dlg.DialogResult != DialogResult.OK)
                return null;

            bCheckBox = dlg.CheckBoxValue;
            return dlg.Values[dlg.SelectIndex];
        }

        public string[] Values
        {
            get
            {
                return this.listBox1.Items.Cast<string>().ToArray();
            }
            set
            {
                this.listBox1.Items.Clear();
                if (value != null)
                    this.listBox1.Items.AddRange(value);
            }
        }

        public int SelectIndex
        {
            get
            {
                return this.listBox1.SelectedIndex;
            }
            set
            {
                this.listBox1.SelectedIndex = value;
            }
        }

        public string CheckBoxText
        {
            get
            {
                return this.checkBox1.Text;
            }
            set
            {
                this.checkBox1.Text = value;
            }
        }

        public bool CheckBoxValue
        {
            get
            {
                return this.checkBox1.Checked;
            }
            set
            {
                this.checkBox1.Checked = value;
            }
        }

        public bool CheckBoxVisible
        {
            get
            {
                return this.checkBox1.Visible;
            }
            set
            {
                this.checkBox1.Visible = value;
            }
        }
    }
}
