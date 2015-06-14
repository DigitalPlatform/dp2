using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;

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
        public MainForm MainForm = null;

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
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
#endif

            this.comboBox_location.Text = this.MainForm.AppInfo.GetString(
                this.CfgSectionName,
                "location",
                "");
        }

        private void SearchByBatchnoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.MainForm.AppInfo.SetString(
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