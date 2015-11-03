using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 快速修改读者记录 -- 指定动作参数对话框
    /// </summary>
    internal partial class ChangeReaderActionDialog : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;
        public string RefDbName = "";

        public ChangeReaderActionDialog()
        {
            InitializeComponent();
        }

        private void ChangeReaderActionDialog_Load(object sender, EventArgs e)
        {
            // 装载值

            // state
            this.comboBox_state.Text = this.MainForm.AppInfo.GetString(
                "change_reader_param",
                "state",
                "<不改变>");

            this.checkedComboBox_stateAdd.Text = this.MainForm.AppInfo.GetString(
                "change_reader_param",
                "state_add",
                "");
            this.checkedComboBox_stateRemove.Text = this.MainForm.AppInfo.GetString(
    "change_reader_param",
    "state_remove",
    "");

            // expire date
            this.comboBox_expireDate.Text = this.MainForm.AppInfo.GetString(
    "change_reader_param",
    "expire_date",
    "<不改变>");
            this.dateControl_expireDate.Text = this.MainForm.AppInfo.GetString(
    "change_reader_param",
    "expire_date_value",
    "");

            // reader type
            this.comboBox_readerType.Text = this.MainForm.AppInfo.GetString(
"change_reader_param",
"reader_type",
"<不改变>");

            // 其它字段
            this.comboBox_fieldName.Text = this.MainForm.AppInfo.GetString(
"change_reader_param",
"field_name",
"<不使用>");
            this.textBox_fieldValue.Text = this.MainForm.AppInfo.GetString(
"change_reader_param",
"field_value",
"");

            comboBox_state_TextChanged(null, null);
            comboBox_expireDate_TextChanged(null, null);

        }

        private void ChangeReaderActionDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.comboBox_expireDate.Text == "<不改变>"
                && this.comboBox_readerType.Text == "<不改变>"
                && this.comboBox_state.Text == "<不改变>"
                && this.comboBox_fieldName.Text == "<不使用>")
            {
                strError = "需要设定至少一个要改变的事项";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.comboBox_fieldName.Text) == true)
            {
                strError = "尚未指定字段名";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_fieldValue.Text) == true
                && this.comboBox_fieldName.Text != "<不使用>")
            {
                DialogResult result = MessageBox.Show(this,
                    "确实要将 '" + this.comboBox_fieldName.Text + "' 字段的内容修改为空? (Yes 是；No 放弃)",
                    "ReaderSearchForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
            }

            // 检查时间格式
            if (string.IsNullOrEmpty(this.textBox_fieldValue.Text) == false)
            {
                if (this.comboBox_fieldName.Text == "发证日期"
                    || this.comboBox_fieldName.Text == "失效日期"
                    || this.comboBox_fieldName.Text == "租金失效期"
                    || this.comboBox_fieldName.Text == "出生日期")
                {
                    try
                    {
                        DateTimeUtil.FromRfc1123DateTimeString(this.textBox_fieldValue.Text);
                    }
                    catch
                    {
                        strError = "时间字符串 '"+this.textBox_fieldValue.Text+"' 不是合法的 RFC1123 格式 ...";
                        goto ERROR1;
                    }
                }
            }

            // state
            this.MainForm.AppInfo.SetString(
                "change_reader_param",
                "state",
                this.comboBox_state.Text);

            this.MainForm.AppInfo.SetString(
                "change_reader_param",
                "state_add",
                this.checkedComboBox_stateAdd.Text);
            this.MainForm.AppInfo.SetString(
    "change_reader_param",
    "state_remove",
    this.checkedComboBox_stateRemove.Text);

            // expire date
            this.MainForm.AppInfo.SetString(
    "change_reader_param",
    "expire_date",
    this.comboBox_expireDate.Text);
            this.MainForm.AppInfo.SetString(
    "change_reader_param",
    "expire_date_value",
    this.dateControl_expireDate.Text);

            // reader type
            this.MainForm.AppInfo.SetString(
"change_reader_param",
"reader_type",
this.comboBox_readerType.Text);

            // 其它字段
            this.MainForm.AppInfo.SetString(
"change_reader_param",
"field_name",
this.comboBox_fieldName.Text);
            this.MainForm.AppInfo.SetString(
"change_reader_param",
"field_value",
this.textBox_fieldValue.Text);

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void comboBox_state_TextChanged(object sender, EventArgs e)
        {
            string strText = this.comboBox_state.Text;

            if (strText == "<增、减>")
            {
                this.checkedComboBox_stateAdd.Enabled = true;
                this.checkedComboBox_stateRemove.Enabled = true;
            }
            else
            {
                this.checkedComboBox_stateAdd.Text = "";
                this.checkedComboBox_stateAdd.Enabled = false;

                this.checkedComboBox_stateRemove.Text = "";
                this.checkedComboBox_stateRemove.Enabled = false;
            }

            if (strText == "<不改变>")
                this.label_state.BackColor = this.BackColor;
            else
                this.label_state.BackColor = Color.Green;
        }

        private void comboBox_expireDate_TextChanged(object sender, EventArgs e)
        {
            string strText = this.comboBox_expireDate.Text;

            if (strText == "<指定时间>")
            {
                this.dateControl_expireDate.Enabled = true;
            }
            else
            {
                this.dateControl_expireDate.Enabled = false;
            }

            if (strText == "<不改变>")
                this.label_expireDate.BackColor = this.BackColor;
            else
                this.label_expireDate.BackColor = Color.Green;
        }

        private void comboBox_expireDate_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_expireDate.Invalidate();
        }

        private void comboBox_state_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_state.Invalidate();
        }

        private void comboBox_readerType_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_readerType.Invalidate();
        }

        private void checkedComboBox_stateAdd_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_stateAdd.Items.Count > 0)
                return;
            /*
            this.checkedComboBox_stateAdd.Items.Add("注销");
            this.checkedComboBox_stateAdd.Items.Add("挂失");
            this.checkedComboBox_stateAdd.Items.Add("停借");
             * */
            FillReaderStateDropDown(this.checkedComboBox_stateAdd);
        }

        private void checkedComboBox_stateRemove_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_stateRemove.Items.Count > 0)
                return;

            /*
            this.checkedComboBox_stateRemove.Items.Add("注销");
            this.checkedComboBox_stateRemove.Items.Add("挂失");
            this.checkedComboBox_stateRemove.Items.Add("停借");
             * */
            FillReaderStateDropDown(this.checkedComboBox_stateRemove);
        }

        void FillReaderStateDropDown(CheckedComboBox combobox)
        {
            // 防止重入
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                if (combobox.Items.Count <= 0
                    && this.GetValueTable != null)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    e1.DbName = this.RefDbName;

                    e1.TableName = "readerState";

                    this.GetValueTable(this, e1);

                    if (e1.values != null)
                    {
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            combobox.Items.Add(e1.values[i]);
                        }
                    }
                    else
                    {
                        // combobox.Items.Add("{not found}");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

        private void comboBox_readerType_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_readerType.Text == "<不改变>")
                this.label_readerType.BackColor = this.BackColor;
            else
                this.label_readerType.BackColor = Color.Green;
        }

        int m_nInDropDown = 0;
        void FillDropDown(ComboBox combobox)
        {
            // 防止重入
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                if (combobox.Items.Count <= 0
                    && this.GetValueTable != null)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    e1.DbName = this.RefDbName;

                    if (combobox == this.comboBox_readerType)
                        e1.TableName = "readerType";
                    else
                    {
                        Debug.Assert(false, "不支持的combobox");
                    }

                    this.GetValueTable(this, e1);

                    combobox.Items.Add("<不改变>");

                    if (e1.values != null)
                    {
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            combobox.Items.Add(e1.values[i]);
                        }
                    }
                    else
                    {
                        // combobox.Items.Add("{not found}");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

        private void comboBox_readerType_DropDown(object sender, EventArgs e)
        {
            ComboBox combobox = (ComboBox)sender;
            if (combobox.Items.Count == 0)
                FillDropDown(combobox);
        }

        private void checkedComboBox_stateAdd_TextChanged(object sender, EventArgs e)
        {
            Global.FilterValueList(this, (Control)sender);
#if NO
            Delegate_filterValue d = new Delegate_filterValue(FileterValueList);
            this.BeginInvoke(d, new object[] { sender });
#endif
        }

        private void checkedComboBox_stateRemove_TextChanged(object sender, EventArgs e)
        {
            Global.FilterValueList(this, (Control)sender);
#if NO
            Delegate_filterValue d = new Delegate_filterValue(FileterValueList);
            this.BeginInvoke(d, new object[] { sender });
#endif
        }

        private void comboBox_readerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.FilterValue(this, (Control)sender);
#if NO
            Delegate_filterValue d = new Delegate_filterValue(FileterValue);
            this.BeginInvoke(d, new object[] { sender });
#endif
        }

        private void comboBox_fieldName_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_fieldName.Text == "<不使用>")
            {
                this.textBox_fieldValue.Text = "";
                this.textBox_fieldValue.Enabled = false;

                this.label_fieldName.BackColor = this.BackColor;
            }
            else
            {
                this.textBox_fieldValue.Enabled = true;

                this.label_fieldName.BackColor = Color.Green;
            }
        }

        private void ToolStripMenuItem_rfc1123Single_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            dlg.RangeMode = false;
            try
            {
                dlg.Rfc1123String = this.textBox_fieldValue.Text;
            }
            catch
            {
                this.textBox_fieldValue.Text = "";
            }
            this.MainForm.AppInfo.LinkFormState(dlg, "ChangeReaderActionDialog_gettimedialog");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_fieldValue.Text = dlg.Rfc1123String;
        }
    }
}
