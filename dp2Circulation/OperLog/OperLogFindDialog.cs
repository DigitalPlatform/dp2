using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;

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
            "hire\t创建租金交费请求",
            "foregift\t创建押金交费请求",
            "settlement\t结算",
            "passgate\t入馆登记",
            "setBiblioInfo\t设置书目信息",
            "setReaderInfo\t设置读者记录",
            "changeReaderPassword\t修改读者密码",
            "changeReaderTempPassword\t修改用户临时密码",
            "devolveReaderInfo\t转移读者记录",
            "setEntity\t设置册记录",
            "setOrder\t设置订购记录",
            "setIssue\t设置期记录",
            "setComment\t设置评注记录",
            "devolveReaderInfo\t转移借阅信息",
            "repairBorrowInfo\t修复借阅信息",
            "getRes\t获取对象资源",
            "writeRes\t写入对象资源",
            "setUser\t设置用户",
            "crashReport\t崩溃报告",
            "memo\t注记",
            "manageDatabase\t管理数据库",
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

            if (CheckFilterRelation(this.checkedComboBox_filter.Text,
                this.checkedComboBox_operations.Text,
                out strError) == false)
                goto ERROR1;

            this.RecPathList = StringUtil.SplitList(this.textBox_recPathList.Text, "\r\n");

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
            "册修改历史\t册记录",
            });

        }

        static string[] relations = new string[]
        {
            "册修改历史", "setEntity,borrow,return",
            "增998$t", "setBiblioInfo",
        };

        bool CheckFilterRelation(string filter_name_list,
    string operations,
    out string strError)
        {
            strError = "";
            List<string> errors = new List<string>();
            List<string> filters = StringUtil.SplitList(filter_name_list);
            foreach (string filter_name in filters)
            {
                for (int i = 0; i < relations.Length / 2; i++)
                {
                    string name = relations[i * 2];
                    string list = relations[i * 2 + 1];

                    if (name == filter_name)
                    {
                        List<string> missing_list = new List<string>();
                        // 每个要求的 operation 都应该在 operations 中具备
                        foreach (string s in StringUtil.SplitList(list))
                        {
                            if (StringUtil.IsInList(s, operations) == false)
                                missing_list.Add(s);
                        }

                        if (missing_list.Count > 0)
                        {
                            strError = "和过滤方式 '" + filter_name + "' 对应的操作类型中缺乏必备的 '" + StringUtil.MakePathList(missing_list) + "'";
                            errors.Add(strError);
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return false;
            }

            return true;
        }

        string GetFilterRelation(string filter_name_list)
        {
            List<string> results = new List<string>();
            List<string> filters = StringUtil.SplitList(filter_name_list);
            foreach (string filter_name in filters)
            {
                for (int i = 0; i < relations.Length / 2; i++)
                {
                    string name = relations[i * 2];
                    string value_list = relations[i * 2 + 1];

                    if (name == filter_name)
                        results.Add(value_list);
                }
            }

            if (results.Count == 0)
                return null;

            // 将 x1,x2 , y1,y2 串联后重新完全拆解
            string strText = StringUtil.MakePathList(results);
            results = StringUtil.SplitList(strText);

            StringUtil.RemoveDupNoSort(ref results);

            return StringUtil.MakePathList(results);
        }


        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.checkedComboBox_operations);
                controls.Add(this.checkedComboBox_filter);
                controls.Add(this.textBox_recPathList);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.checkedComboBox_operations);
                controls.Add(this.checkedComboBox_filter);
                controls.Add(this.textBox_recPathList);
                GuiState.SetUiState(controls, value);
            }
        }

        public List<string> RecPathList
        {
            get;
            set;
        }

        private void checkedComboBox_filter_TextChanged(object sender, EventArgs e)
        {
            string operations = GetFilterRelation(this.checkedComboBox_filter.Text);
            if (string.IsNullOrEmpty(operations) == false)
            {
                // TODO: 检查 _operations.Text 是否已经涵盖了 operations 内容。如果没有涵盖，则用 operations 设置它
                this.checkedComboBox_operations.Text = operations;
            }
        }

        private void checkedComboBox_operations_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            CheckedComboBox.ProcessItemChecked(e, "<全部>,<all>".ToLower());
        }

        private void checkedComboBox_filter_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            CheckedComboBox.ProcessItemChecked(e, "<无>");
        }

        private void button_dontFilter_Click(object sender, EventArgs e)
        {
            this.checkedComboBox_filter.Text = "<无>";
            this.checkedComboBox_operations.Text = "<all>";
            this.textBox_recPathList.Text = "";
        }

    }
}
