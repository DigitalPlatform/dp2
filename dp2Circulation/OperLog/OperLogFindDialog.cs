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
    /// 查找日志记录的对话框
    /// </summary>
    internal partial class OperLogFindDialog : Form
    {
        public OperLogFindDialog()
        {
            InitializeComponent();
        }

        private void checkedComboBox_operations_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_operations.Items.Count > 0)
                return;

            this.checkedComboBox_operations.Items.AddRange(new string[] { 
            "borrow\t借书(或续借)",
            "return\t还书(或声明丢失)",
            "reservation\t预约",
            "amerce\t违约金操作",
            "changeReaderPassword\t修改读者密码",
            "hire\t创建租金交费请求",
            "foregift\t创建押金交费请求",
            "settlement\t结算",
            "passgate\t入馆登记",
            "setBiblioInfo\t设置书目信息",
            "setReaderInfo\t设置读者记录",
            "setEntity\t设置册记录",
            "setOrder\t设置订购记录",
            "setIssue\t设置期记录",
            "setComment\t设置评注记录",
            "devolveReaderInfo\t转移借阅信息",
            "repairBorrowInfo\t修复借阅信息",
            "writeRes\t写入对象资源",
            "setUser\t设置用户",
            });
        }

        public string Operations
        {
            get
            {
                return this.checkedComboBox_operations.Text;
            }
            set
            {
                this.checkedComboBox_operations.Text = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
