using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 快速修改书目 动作参数 对话框
    /// </summary>
    internal partial class ChangeBiblioActionDialog : Form
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

        public ChangeBiblioActionDialog()
        {
            InitializeComponent();
        }

        private void ChangeBiblioActionDialog_Load(object sender, EventArgs e)
        {
            // 装载值

            // state
            this.comboBox_state.Text = this.MainForm.AppInfo.GetString(
                "change_biblio_param",
                "state",
                "<不改变>");

            this.checkedComboBox_stateAdd.Text = this.MainForm.AppInfo.GetString(
                "change_biblio_param",
                "state_add",
                "");
            this.checkedComboBox_stateRemove.Text = this.MainForm.AppInfo.GetString(
    "change_biblio_param",
    "state_remove",
    "");

            // opertime
            this.comboBox_opertime.Text = this.MainForm.AppInfo.GetString(
    "change_biblio_param",
    "opertime",
    "<不改变>");
            this.dateTimePicker1.Text = this.MainForm.AppInfo.GetString(
    "change_biblio_param",
    "opertime_value",
    "");

            // batchno
            this.comboBox_batchNo.Text = this.MainForm.AppInfo.GetString(
"change_biblio_param",
"batchNo",
"<不改变>");

            comboBox_state_TextChanged(null, null);
            comboBox_opertime_TextChanged(null, null);
            comboBox_batchNo_TextChanged(null, null);
        }

        private void checkedComboBox_stateAdd_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_stateAdd.Items.Count > 0)
                return;

            /*
            this.checkedComboBox_stateAdd.Items.Add("订购征询");
            this.checkedComboBox_stateAdd.Items.Add("读者创建");
             * */
            FillBiblioStateDropDown(this.checkedComboBox_stateAdd);
        }

        private void checkedComboBox_stateRemove_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_stateRemove.Items.Count > 0)
                return;

            /*
            this.checkedComboBox_stateRemove.Items.Add("订购征询");
            this.checkedComboBox_stateRemove.Items.Add("读者创建");
             * */
            FillBiblioStateDropDown(this.checkedComboBox_stateRemove);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 保存值

            // state
            this.MainForm.AppInfo.SetString(
                "change_biblio_param",
                "state",
                this.comboBox_state.Text);
            this.MainForm.AppInfo.SetString(
    "change_biblio_param",
    "state_add",
    this.checkedComboBox_stateAdd.Text);
            this.MainForm.AppInfo.SetString(
"change_biblio_param",
"state_remove",
this.checkedComboBox_stateRemove.Text);

            // opertime
            this.MainForm.AppInfo.SetString(
    "change_biblio_param",
    "opertime",
    this.comboBox_opertime.Text);
            this.MainForm.AppInfo.SetString(
"change_biblio_param",
"opertime_value",
this.dateTimePicker1.Text);

            // batchno
            this.MainForm.AppInfo.SetString(
    "change_biblio_param",
    "batchNo",
    this.comboBox_batchNo.Text);

            this.DialogResult = DialogResult.OK;
            this.Close();
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

        private void comboBox_opertime_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_opertime.Invalidate();
        }

        private void comboBox_state_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_state.Invalidate();
        }

        private void comboBox_batchNo_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_batchNo.Invalidate();
        }

        private void comboBox_opertime_TextChanged(object sender, EventArgs e)
        {
            string strText = this.comboBox_opertime.Text;

            if (strText == "<指定时间>")
            {
                this.dateTimePicker1.Enabled = true;
            }
            else
            {
                // this.dateTimePicker1.Text = "";
                this.dateTimePicker1.Enabled = false;
            }

            if (strText == "<不改变>")
                this.label_operTime.BackColor = this.BackColor;
            else
                this.label_operTime.BackColor = Color.Green;
        }

        private void comboBox_batchNo_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_batchNo.Text == "<不改变>")
                this.label_batchNo.BackColor = this.BackColor;
            else
                this.label_batchNo.BackColor = Color.Green;
        }

        int m_nInDropDown = 0;
        void FillBiblioStateDropDown(CheckedComboBox combobox)
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

                    e1.TableName = "biblioState";

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

#if NO
        delegate void Delegate_filterValue(Control control);

        // 过滤掉 {} 包围的部分
        void FileterValue(Control control)
        {
            string strText = Global.GetPureSeletedValue(control.Text);
            if (control.Text != strText)
                control.Text = strText;
        }

        // 过滤掉 {} 包围的部分
        // 还有列表值去重的功能
        void FileterValueList(Control control)
        {
            List<string> results = StringUtil.FromListString(Global.GetPureSeletedValue(control.Text));
            StringUtil.RemoveDupNoSort(ref results);
            string strText = StringUtil.MakePathList(results);
            if (control.Text != strText)
                control.Text = strText;
        }
#endif

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

    }
}