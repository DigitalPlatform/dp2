using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 快速修改册 动作参数 对话框
    /// 被 QuickChangeEntityForm 所使用
    /// </summary>
    internal partial class ChangeEntityActionDialog : Form
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

        public ChangeEntityActionDialog()
        {
            InitializeComponent();
        }

        private void ChangeParamDlg_Load(object sender, EventArgs e)
        {
            // 填充几个combobox

            // 装载值
            this.comboBox_state.Text = this.MainForm.AppInfo.GetString(
                "change_param",
                "state",
                "<不改变>");
            this.checkedComboBox_stateAdd.Text = this.MainForm.AppInfo.GetString(
    "change_param",
    "state_add",
    "");
            this.checkedComboBox_stateRemove.Text = this.MainForm.AppInfo.GetString(
    "change_param",
    "state_remove",
    "");

            this.comboBox_location.Text = this.MainForm.AppInfo.GetString(
    "change_param",
    "location",
    "<不改变>");

            this.comboBox_bookType.Text = this.MainForm.AppInfo.GetString(
    "change_param",
    "bookType",
    "<不改变>");

            this.comboBox_batchNo.Text = this.MainForm.AppInfo.GetString(
"change_param",
"batchNo",
"<不改变>");

            this.comboBox_focusAction.Text = this.MainForm.AppInfo.GetString(
    "change_param",
    "focusAction",
    "册条码号，并全选");

            this.comboBox_state_TextChanged(null, null);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 保存值
            this.MainForm.AppInfo.SetString(
                "change_param",
                "state",
                this.comboBox_state.Text);
            this.MainForm.AppInfo.SetString(
    "change_param",
    "state_add",
    this.checkedComboBox_stateAdd.Text);
            this.MainForm.AppInfo.SetString(
    "change_param",
    "state_remove",
    this.checkedComboBox_stateRemove.Text);

            this.MainForm.AppInfo.SetString(
    "change_param",
    "location",
    this.comboBox_location.Text);

            this.MainForm.AppInfo.SetString(
    "change_param",
    "bookType",
    this.comboBox_bookType.Text);

            this.MainForm.AppInfo.SetString(
    "change_param",
    "batchNo",
    this.comboBox_batchNo.Text);


            this.MainForm.AppInfo.SetString(
    "change_param",
    "focusAction",
    this.comboBox_focusAction.Text);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 2009/7/19 new add
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
                if (combobox.Items.Count == 0
                    && this.GetValueTable != null)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    e1.DbName = this.RefDbName;

                    if (combobox == this.comboBox_bookType)
                        e1.TableName = "bookType";
                    else if (combobox == this.comboBox_location)
                        e1.TableName = "location";
                    else if (combobox == this.comboBox_state)
                        e1.TableName = "state";
                    else
                    {
                        Debug.Assert(false, "不支持的combobox");
                    }

                    this.GetValueTable(this, e1);

                    combobox.Items.Add("<不改变>");
                    if (combobox == this.comboBox_state)
                    {
                        combobox.Items.Add("<增、减>");
                    }

                    if (e1.values != null)
                    {
                        List<string> results = null;

                        string strLibraryCode = "";
                        string strPureName = "";

                        string strLocationString = this.comboBox_location.Text;
                        if (strLocationString == "<不改变>")
                            strLocationString = "";

                        Global.ParseCalendarName(strLocationString,
                    out strLibraryCode,
                    out strPureName);

                        if (combobox != this.comboBox_location  // 馆藏地的列表不要被过滤
                            && String.IsNullOrEmpty(strLocationString) == false)
                        {
                            // 过滤出符合馆代码的那些值字符串
                            results = Global.FilterValuesWithLibraryCode(strLibraryCode,
                                StringUtil.FromStringArray(e1.values));
                        }
                        else
                        {
                            results = StringUtil.FromStringArray(e1.values);
                        }

                        foreach (string s in results)
                        {
                            combobox.Items.Add(s);
                        }

#if NO
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            combobox.Items.Add(e1.values[i]);
                        }
#endif
                    }
                    else
                    {
                        combobox.Items.Add("{not found}");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

        private void comboBox_location_DropDown(object sender, EventArgs e)
        {
            FillDropDown((ComboBox)sender);
        }

        private void comboBox_bookType_DropDown(object sender, EventArgs e)
        {
            FillDropDown((ComboBox)sender);
        }

        private void comboBox_state_DropDown(object sender, EventArgs e)
        {
            FillDropDown((ComboBox)sender);
        }

        private void checkedComboBox_stateAdd_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_stateAdd.Items.Count > 0)
                return;
            FillItemStateDropDown(this.checkedComboBox_stateAdd);
        }

        private void checkedComboBox_stateRemove_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_stateRemove.Items.Count > 0)
                return;
            FillItemStateDropDown(this.checkedComboBox_stateRemove);
        }

        void FillItemStateDropDown(CheckedComboBox combobox)
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

                    e1.TableName = "state";

                    this.GetValueTable(this, e1);

                    if (e1.values != null)
                    {
                        List<string> results = null;

                        string strLibraryCode = "";
                        string strPureName = "";

                        string strLocationString = this.comboBox_location.Text;
                        if (strLocationString == "<不改变>")
                            strLocationString = "";

                        Global.ParseCalendarName(strLocationString,
                    out strLibraryCode,
                    out strPureName);

                        if (String.IsNullOrEmpty(strLocationString) == false)
                        {
                            // 过滤出符合馆代码的那些值字符串
                            results = Global.FilterValuesWithLibraryCode(strLibraryCode,
                                StringUtil.FromStringArray(e1.values));
                        }
                        else
                        {
                            results = StringUtil.FromStringArray(e1.values);
                        }

                        foreach (string s in results)
                        {
                            combobox.Items.Add(s);
                        }
#if NO
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            combobox.Items.Add(e1.values[i]);
                        }
#endif
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

        private void comboBox_location_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_location.Invalidate();
        }

        private void comboBox_bookType_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_bookType.Invalidate();
        }

        private void comboBox_state_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_state.Invalidate();
        }

        private void comboBox_batchNo_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_batchNo.Invalidate();
        }

        private void comboBox_focusAction_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_focusAction.Invalidate();
        }

        private void comboBox_location_TextChanged(object sender, EventArgs e)
        {
            this.comboBox_state.Items.Clear();
            this.checkedComboBox_stateAdd.Items.Clear();
            this.checkedComboBox_stateRemove.Items.Clear();
            this.comboBox_bookType.Items.Clear();

            string strText = this.comboBox_location.Text;

            if (strText == "<不改变>")
                this.label_location.BackColor = this.BackColor;
            else
                this.label_location.BackColor = Color.Green;
        }

        private void comboBox_bookType_TextChanged(object sender, EventArgs e)
        {
            string strText = this.comboBox_bookType.Text;

            if (strText == "<不改变>")
                this.label_bookType.BackColor = this.BackColor;
            else
                this.label_bookType.BackColor = Color.Green;

        }

        private void comboBox_batchNo_TextChanged(object sender, EventArgs e)
        {
            string strText = this.comboBox_batchNo.Text;

            if (strText == "<不改变>")
                this.label_batchNo.BackColor = this.BackColor;
            else
                this.label_batchNo.BackColor = Color.Green;
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

        private void comboBox_bookType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.FilterValue(this, (Control)sender);
#if NO
            Delegate_filterValue d = new Delegate_filterValue(FileterValue);
            this.BeginInvoke(d, new object[] { sender });
#endif
        }

        private void comboBox_state_SelectedIndexChanged(object sender, EventArgs e)
        {
            Global.FilterValue(this, (Control)sender);
#if NO
            Delegate_filterValue d = new Delegate_filterValue(FileterValue);
            this.BeginInvoke(d, new object[] { sender });
#endif
        }

    }
}