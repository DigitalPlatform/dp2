using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 用户编辑多行文本的对话框
    /// </summary>
    public partial class EditDialog : Form
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public EditDialog()
        {
            InitializeComponent();
        }

        private void EditDialog_Load(object sender, EventArgs e)
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
        /// <summary>
        /// 获得用户输入
        /// </summary>
        /// <param name="owner">宿主窗口</param>
        /// <param name="strDlgTitle">对话框标题文字</param>
        /// <param name="strTitle">标题文字</param>
        /// <param name="strDefaultValue">缺省值</param>
        /// <param name="font">字体</param>
        /// <returns>返回所输入的文字。如果为 null，表示用户放弃输入</returns>
        public static string GetInput(
            IWin32Window owner,
            string strDlgTitle,
            string strTitle,
            string strDefaultValue,
            Font font = null)
        {
            EditDialog dlg = new EditDialog();
            if (font != null)
                dlg.Font = font;

            if (strDlgTitle != null)
                dlg.Text = strDlgTitle;

            if (strTitle != null)
                dlg.label_lines.Text = strTitle;

            if (strDefaultValue != null)
                dlg.textBox_lines.Text = strDefaultValue;

            dlg.StartPosition = FormStartPosition.CenterScreen; // 2008/10/17
            dlg.ShowDialog(owner);

            if (dlg.DialogResult != DialogResult.OK)
                return null;

            return dlg.textBox_lines.Text;
        }
    }
}
