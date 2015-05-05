using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 期刊特殊订购渠道设计器 对话框
    /// 
    /// </summary>
    public partial class SpecialSourceSeriesDialog : Form
    {
        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        public string DbName = "";  // 用于获得值列表

        // 地址XML片段
        public string AddressXml = "";

        public string Seller = "";
        public string Source = "";

        public SpecialSourceSeriesDialog()
        {
            InitializeComponent();
        }

        private void SpecialSourceSeriesDialog_Load(object sender, EventArgs e)
        {
            string strError = "";
            // 综合各种信息，设置状态
            int nRet = SetType(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

        }

        private void SpecialSourceSeriesDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void SpecialSourceSeriesDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 获得用户选择的状态
            int nRet = GetType(out strError);
            if (nRet == -1)
                goto ERROR1;

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

        // 获得用户选择的状态
        int GetType(out string strError)
        {
            strError = "";

            if (this.personAddressControl.Changed == true)
            {
                // 获得编辑后的数据
                try
                {
                    this.AddressXml = this.personAddressControl.DataDom.DocumentElement.OuterXml;
                }
                catch (Exception ex)
                {
                    strError = "获得AddressXml数据时出错: " + ex.Message;
                    return -1;
                }
            }

            // 普通渠道
            if (this.comboBox_specialSource.Text == "普通")
            {
                if (this.comboBox_seller.Text == "")
                {
                    strError = "普通渠道类型时，渠道名不能为空";
                    return -1;
                }

                if (this.comboBox_source.Text == "")
                {
                    strError = "普通渠道类型时，经费来源不能为空";
                    return -1;
                }

                ///

                if (this.comboBox_seller.Text == "直订")
                {
                    strError = "普通渠道类型时，渠道名不能为 '" + this.comboBox_seller.Text + "'";
                    return -1;
                }

                if (this.comboBox_seller.Text == "交换")
                {
                    strError = "普通渠道类型时，渠道名不能为 '" + this.comboBox_seller.Text + "'";
                    return -1;
                }

                if (this.comboBox_seller.Text == "赠")
                {
                    strError = "普通渠道类型时，渠道名不能为 '" + this.comboBox_seller.Text + "'";
                    return -1;
                }

                this.Source = this.comboBox_source.Text;
                this.Seller = this.comboBox_seller.Text;
                // 地址不变
                return 0;
            }

            // 直订
            if (this.comboBox_specialSource.Text == "直订")
            {
                if (this.comboBox_source.Text == "")
                {
                    strError = "直订时，经费来源不能为空";
                    return -1;
                }

                if (this.comboBox_seller.Text == "交换")
                {
                    strError = "直订时，渠道名不能为 '" + this.comboBox_seller.Text + "'";
                    return -1;
                }

                if (this.comboBox_seller.Text == "赠")
                {
                    strError = "直订时，渠道名不能为 '" + this.comboBox_seller.Text + "'";
                    return -1;
                }

                this.Source = this.comboBox_source.Text;

                this.Seller = "直订";
                // TODO: 合成地址
                return 0;
            }

            // 交换
            if (this.comboBox_specialSource.Text == "交换")
            {
                this.Seller = "交换";
                this.Source = "";
                return 0;
            }

            // 赠
            if (this.comboBox_specialSource.Text == "赠")
            {
                this.Seller = "赠";
                this.Source = "";
                return 0;
            }

            strError = "不合法的渠道类型 '" + this.comboBox_specialSource.Text + "'";
            return -1;
        }

        // 综合各种信息，设置状态
        int SetType(out string strError)
        {
            strError = "";
            int nRet = 0;

            this.comboBox_seller.Text = this.Seller;
            this.comboBox_source.Text = this.Source;

            // 装入地址信息
            if (String.IsNullOrEmpty(this.AddressXml) == false)
            {
                nRet = this.personAddressControl.SetData(this.AddressXml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (this.Seller == "直订")
            {
                this.comboBox_specialSource.Text = "直订";
                return 0;
            }

            if (this.Seller == "交换")
            {
                this.comboBox_specialSource.Text = "交换";
                return 0;
            }

            if (this.Seller == "赠")
            {
                this.comboBox_specialSource.Text = "赠";
                return 0;
            }

            this.comboBox_specialSource.Text = "普通";
            return 0;
        }

        // 防止重入 2009/7/19 new add
        int m_nInDropDown = 0;

        private void comboBox_DropDown(object sender, EventArgs e)
        {
            // 防止重入 2009/7/19 new add
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                ComboBox combobox = (ComboBox)sender;

                if (combobox.Items.Count == 0
                    && this.GetValueTable != null)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    e1.DbName = this.DbName;

                    if (combobox == this.comboBox_source)
                        e1.TableName = "orderSource";
                    else if (combobox == this.comboBox_seller)
                        e1.TableName = "orderSeller";
                    else
                    {
                        Debug.Assert(false, "不支持的sender");
                        return;
                    }

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

        private void comboBox_seller_DropDown(object sender, EventArgs e)
        {
            comboBox_DropDown(sender, e);
        }

        private void comboBox_source_DropDown(object sender, EventArgs e)
        {
            comboBox_DropDown(sender, e);
        }

        private void comboBox_specialSource_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_specialSource.Text == "直订")
            {
                this.comboBox_seller.Enabled = false;
                this.comboBox_seller.Visible = false;
                this.label_seller.Visible = false;

                this.comboBox_source.Enabled = true;
                this.comboBox_source.Visible = true;
                this.label_source.Visible = true;
            }
            else if (this.comboBox_specialSource.Text == "交换")
            {
                this.comboBox_seller.Enabled = false;
                this.comboBox_seller.Visible = false;
                this.label_seller.Visible = false;

                this.comboBox_source.Enabled = false;
                this.comboBox_source.Visible = false;
                this.label_source.Visible = false;
            }
            else if (this.comboBox_specialSource.Text == "赠")
            {
                this.comboBox_seller.Enabled = false;
                this.comboBox_seller.Visible = false;
                this.label_seller.Visible = false;

                this.comboBox_source.Enabled = false;
                this.comboBox_source.Visible = false;
                this.label_source.Visible = false;
            }
            else if (this.comboBox_specialSource.Text == "普通")
            {
                this.comboBox_seller.Enabled = true;
                this.comboBox_seller.Visible = true;
                this.label_seller.Visible = true;

                this.comboBox_source.Enabled = true;
                this.comboBox_source.Visible = true;
                this.label_source.Visible = true;

                if (this.comboBox_seller.Text == "直订")
                    this.comboBox_seller.Text = "";

                if (this.comboBox_seller.Text == "交换"
                    || this.comboBox_seller.Text == "赠")
                    this.comboBox_seller.Text = "";
            }
            else
            {
                // 其他不合法的渠道名
                this.comboBox_seller.Enabled = false;
                this.comboBox_seller.Visible = false;
                this.label_seller.Visible = false;

                this.comboBox_source.Enabled = false;
                this.comboBox_source.Visible = false;
                this.label_source.Visible = false;
            }

        }

    }
}