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
    /// 加好友的对话框
    /// </summary>
    public partial class AddFriendsDialog : Form
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AddFriendsDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 读者证条码号
        /// </summary>
        public string ReaderBarcode
        {
            get
            {
                return this.textBox_readerBarcode.Text;
            }
            set
            {
                this.textBox_readerBarcode.Text = value;
            }
        }

        /// <summary>
        /// 附言
        /// </summary>
        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }

        private void AddFriendsDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.textBox_readerBarcode.Text) == true)
            {
                strError = "请输入读者证条码号";
                goto ERROR1;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
