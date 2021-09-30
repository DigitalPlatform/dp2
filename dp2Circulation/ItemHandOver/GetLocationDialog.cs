using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 获得馆藏地点输入的对话框
    /// </summary>
    internal partial class GetLocationDialog : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        // 保存信息的小节名
        public string CfgSectionName = "GetLocationDialog";


        public event GetValueTableEventHandler GetLocationValueTable = null;

        public string RefDbName = "";

        public GetLocationDialog()
        {
            InitializeComponent();
        }

        private void SearchByBatchnoForm_Load(object sender, EventArgs e)
        {
#if NO
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
#endif
            // 2021/9/27
            FillDropDown(this.comboBox_location);

            this.comboBox_location.Text = Program.MainForm.AppInfo.GetString(
                this.CfgSectionName,
                "location",
                "");
        }

        private void SearchByBatchnoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.MainForm.AppInfo.SetString(
                this.CfgSectionName,
                "location",
                this.comboBox_location.Text);
        }

        public string ItemLocation
        {
            get
            {
                return this.comboBox_location.Text;
            }
            set
            {
                this.comboBox_location.Text = value;
            }
        }

        // 2021/9/28
        // 批次号。用于写入 dp2library 操作日志
        public string BatchNo
        {
            get
            {
                return this.textBox_batchNo.Text;
            }
            set
            {
                this.textBox_batchNo.Text = value;
            }
        }

        private void comboBox_location_DropDown(object sender, EventArgs e)
        {
            FillDropDown((ComboBox)sender);
        }

        // 防止重入 2009/7/19
        int m_nInDropDown = 0;

        void FillDropDown(ComboBox combobox)
        {
            // 防止重入 2009/7/19
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                if (combobox.Items.Count == 0
                    && this.GetLocationValueTable != null)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    e1.DbName = this.RefDbName;

                    if (combobox == this.comboBox_location)
                        e1.TableName = "location";
                    else
                    {

                        Debug.Assert(false, "不支持的combobox");
                    }


                    this.GetLocationValueTable(this, e1);

                    // combobox.Items.Add("<不指定>");

                    if (e1.values != null)
                    {
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            combobox.Items.Add(e1.values[i]);
                        }
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

        private void comboBox_location_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_location.Invalidate();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.comboBox_location.Text) == true)
            {
                MessageBox.Show(this, "请指定馆藏地点");
                return;
            }

            // 2021/9/27
            var error = SelectLocationDialog.VerifyLocation(this.comboBox_location.Items.Cast<string>(), this.comboBox_location.Text);
            if (error != null)
            {
                MessageBox.Show(this, $"{error}。请重新选择或者输入");
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void textBox_batchNo_Validating(object sender, CancelEventArgs e)
        {
            if (this.textBox_batchNo.Text.IndexOfAny(new char[] { ':', ',' }) != -1)
            {
                MessageBox.Show(this, $"批次号内容 '{this.textBox_batchNo.Text}' 中包含非法字符");
                e.Cancel = true;
            }
        }
    }
}