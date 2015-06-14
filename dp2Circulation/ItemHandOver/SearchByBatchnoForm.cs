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
    /// 通过批次号进行检索
    /// </summary>
    internal partial class SearchByBatchnoForm : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 获取批次号key+count值列表
        /// </summary>
        public event GetKeyCountListEventHandler GetBatchNoTable = null;

        // 保存信息的小节名
        public string CfgSectionName = "SearchByBatchnoForm";

        public event GetValueTableEventHandler GetLocationValueTable = null;

        public string RefDbName = "";

        public SearchByBatchnoForm()
        {
            InitializeComponent();
        }

        private void SearchByBatchnoForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            if (this.comboBox_batchNo.Text == "")
            {
                this.comboBox_batchNo.Text = this.MainForm.AppInfo.GetString(
                    this.CfgSectionName, // "SearchByBatchnoForm",
                    "batchno",
                    "");
                this.comboBox_location.Text = this.MainForm.AppInfo.GetString(
                    this.CfgSectionName, // "SearchByBatchnoForm",
                    "location",
                    "<不指定>");
            }
            else
            {
                // 当batchno中有预先准备的值的时候，location就需要变成“不指定”了，以免用到以前残留的值
                this.comboBox_location.Text = "<不指定>";
            }
        }

        private void SearchByBatchnoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.MainForm.AppInfo.SetString(
                this.CfgSectionName,    // "SearchByBatchnoForm",
                "batchno",
                this.comboBox_batchNo.Text);
            this.MainForm.AppInfo.SetString(
                this.CfgSectionName,    // "SearchByBatchnoForm",
                "location",
                this.comboBox_location.Text);

        }

        public string BatchNo
        {
            get
            {
                return this.comboBox_batchNo.Text;
            }
            set
            {
                this.comboBox_batchNo.Text = value;
            }
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

        private void button_search_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
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

                    /*
                    if (combobox == this.comboBox_bookType)
                        e1.TableName = "bookType";
                    else if (combobox == this.comboBox_location)
                        e1.TableName = "location";
                    else if (combobox == this.comboBox_state)
                        e1.TableName = "state";
                    else
                    {
                        Debug.Assert(false, "不支持的combobox");
                    }*/

                    if (combobox == this.comboBox_location)
                        e1.TableName = "location";
                    else
                    {

                        Debug.Assert(false, "不支持的combobox");
                    }


                    this.GetLocationValueTable(this, e1);

                    combobox.Items.Add("<不指定>");

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

        // 是否显示馆藏地点 ComboBox
        public bool DisplayLocationList
        {
            get
            {
                return this.comboBox_location.Visible;
            }
            set
            {
                this.comboBox_location.Visible = value;
                this.label_location.Visible = value;
            }
        }

        // dropdown事件中如果进行combobox.Enabled的修改，会造成无法留住下拉状态。可以改用防止重入的整数
        private void comboBox_batchNo_DropDown(object sender, EventArgs e)
        {
            // 防止重入
            if (this.m_nInDropDown > 0)
                return;

            ComboBox combobox = (ComboBox)sender;
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                if (combobox.Items.Count == 0
                    && this.GetBatchNoTable != null)
                {
                    GetKeyCountListEventArgs e1 = new GetKeyCountListEventArgs();
                    this.GetBatchNoTable(this, e1);

                    // 2013/3/25
                    // 当两个 ComboBox 都显示的时候，才对 批次号 列表加入这个事项
                    if (this.comboBox_location.Visible == true)
                        combobox.Items.Add("<不指定>");

                    if (e1.KeyCounts != null)
                    {
                        for (int i = 0; i < e1.KeyCounts.Count; i++)
                        {
                            KeyCount item = e1.KeyCounts[i];
                            combobox.Items.Add(item.Key + "\t" + item.Count.ToString() + "笔");
                        }
                    }
                    else
                    {
                        combobox.Items.Add("<not found>");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

        private void comboBox_batchNo_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_batchNo.Invalidate();
        }

        private void comboBox_location_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_location.Invalidate();
        }
    }

    /// <summary>
    /// 关键词和数量组合值
    /// </summary>
    public class KeyCount
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string Key = "";

        /// <summary>
        /// 数量
        /// </summary>
        public int Count = 0;
    }

    /// <summary>
    /// 获得key+count值列表事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetKeyCountListEventHandler(object sender,
        GetKeyCountListEventArgs e);

    /// <summary>
    /// 获得key+count值列表事件的参数
    /// </summary>
    public class GetKeyCountListEventArgs : EventArgs
    {
        /// <summary>
        /// 值列表
        /// </summary>
        public List<KeyCount> KeyCounts = null;
    }
}