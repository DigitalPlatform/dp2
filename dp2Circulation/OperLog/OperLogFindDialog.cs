using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
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
                "<all>\t全部",
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

        public string Filters
        {
            get
            {
                return this.checkedComboBox_filter.Text;
            }
            set
            {
                this.checkedComboBox_filter.Text = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (StringUtil.IsInList("<all>", this.checkedComboBox_operations.Text)
                && this.checkedComboBox_operations.Text.IndexOf(",") != -1)
            {
                strError = "操作类型一旦选择了 <all>，就不应该再包含其他值了";
                goto ERROR1;
            }

            if ((StringUtil.IsInList("<无>", this.checkedComboBox_filter.Text) || StringUtil.IsInList("<none>", this.checkedComboBox_filter.Text))
    && this.checkedComboBox_filter.Text.IndexOf(",") != -1)
            {
                strError = "过滤方式一旦选择了 <无>，就不应该再包含其他值了";
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

        private void checkedComboBox_filter_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_filter.Items.Count > 0)
                return;

            this.checkedComboBox_filter.Items.AddRange(new string[] { 
            "<无>",
            "增998$t\t书目记录",
            });

        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.checkedComboBox_operations);
                controls.Add(this.checkedComboBox_filter);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.checkedComboBox_operations);
                controls.Add(this.checkedComboBox_filter);
                GuiState.SetUiState(controls, value);
            }
        }
    }
}
