using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace UpgradeDt1000ToDp2
{
    /// <summary>
    /// 设置源数据库的属性(类型等)
    /// </summary>
    public partial class SourceDatabasePropertyDialog : Form
    {
        // 数据库名是否可以修改?
        public bool DatabaseNameChangable = false;

        public string TypeString = "";  // 注意：不包含是否参与流通特性

        public SourceDatabasePropertyDialog()
        {
            InitializeComponent();
        }

        private void SourceDatabasePropertyDialog_Load(object sender, EventArgs e)
        {
            string strError = "";
            bool bBiblio = false;
            bool bAuthority = false;

            // 角色
            if (StringUtil.IsInList("书目库", this.TypeString) == true)
            {
                this.Role = "书目库";
                bBiblio = true;
            }

            // 2008/10/7 new add
            if (StringUtil.IsInList("规范库", this.TypeString) == true)
            {
                this.Role = "规范库";
                bAuthority = true;
            }

            if (bBiblio == true && bAuthority == true)
            {
                strError = "书目库 和 规范库 两个角色不应同时存在";
                goto ERROR1;
            }

            if (StringUtil.IsInList("读者库", this.TypeString) == true)
            {
                this.Role = "读者库";
            }
            if (StringUtil.IsInList("辅助库", this.TypeString) == true)
            {
                this.Role = "辅助库";
            }

            // 图书/期刊
            if (StringUtil.IsInList("图书", this.TypeString) == true)
            {
                if (bBiblio == false)
                {
                    strError = "只有书目库才能有'图书'特性";
                    goto ERROR1;
                }
                this.BookOrSeries = "图书";
            }

            if (StringUtil.IsInList("期刊", this.TypeString) == true)
            {
                if (bBiblio == false)
                {
                    strError = "只有书目库才能有'期刊'特性";
                    goto ERROR1;
                }
                this.BookOrSeries = "期刊";
            }

            // MARC格式
            if (StringUtil.IsInList("usmarc", this.TypeString, true) == true)
            {
                if (bBiblio == false && bAuthority == false)
                {
                    strError = "只有书目库或规范库才能有'USMARC'特性";
                    goto ERROR1;
                }
                this.MarcSyntax = "usmarc";
            }
            if (StringUtil.IsInList("unimarc", this.TypeString, true) == true)
            {
                if (bBiblio == false && bAuthority == false)
                {
                    strError = "只有书目库或规范库才能有'UNIMARC'特性";
                    goto ERROR1;
                }
                this.MarcSyntax = "unimarc";
            }

            // 是否包含实体库
            if (StringUtil.IsInList("实体", this.TypeString, true) == true)
            {
                if (bBiblio == false)
                {
                    strError = "只有书目库才能有'实体'特性";
                    goto ERROR1;
                }
                this.HasEntityDb = true;
            }
            else
                this.HasEntityDb = false;


            // 是否参与采购
            if (StringUtil.IsInList("采购", this.TypeString, true) == true)
            {
                if (bBiblio == false)
                {
                    strError = "只有书目库才能有'采购'特性";
                    goto ERROR1;
                }
                this.IsOrder = true;
            }
            else
                this.IsOrder = false;

            // 是否参与流通不包含在TypeString中，在this.IsCirculation中表达


            if (this.DatabaseNameChangable == false)
                this.textBox_databaseName.ReadOnly = true;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (String.IsNullOrEmpty(this.DatabaseName) == true)
            {
                strError = "尚未指定数据库名";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(this.Role) == true)
            {
                strError = "尚未指定角色";
                goto ERROR1;
            }

            // 角色
            this.TypeString = this.Role;

            if (this.Role == "书目库")
            {
                if (String.IsNullOrEmpty(this.BookOrSeries) == true)
                {
                    strError = "尚未指定 图书 /期刊";
                    goto ERROR1;
                }

                // 图书/期刊
                this.TypeString += "," + this.BookOrSeries;

                if (String.IsNullOrEmpty(this.MarcSyntax) == true)
                {
                    strError = "尚未指定 MARC格式";
                    goto ERROR1;
                }

                // MARC格式
                this.TypeString += "," + this.MarcSyntax.ToUpper();

                // 是否包含实体库
                if (this.HasEntityDb == true)
                    this.TypeString += ",实体";
                else
                {
                    if (this.InCirculation == true)
                    {
                        strError = "要参与流通的书目库，必须包含实体。请修改设置";
                        this.checkBox_hasEntityDb.Focus();
                        goto ERROR1;
                    }
                }

                // 是否参与采购
                if (this.IsOrder == true)
                    this.TypeString += ",采购";

            }

            if (this.Role == "规范库")
            {
                // MARC格式
                this.TypeString += "," + this.MarcSyntax.ToUpper();
            }

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

        public string DatabaseName
        {
            get
            {
                return this.textBox_databaseName.Text;
            }
            set
            {
                this.textBox_databaseName.Text = value;
            }
        }

        // 角色
        public string Role
        {
            get
            {
                return this.comboBox_role.Text;
            }
            set
            {
                this.comboBox_role.Text = value;
                OnRoleChanged();
            }
        }

        // 图书/期刊
        public string BookOrSeries
        {
            get
            {
                if (this.Role != "书目库")
                    return null;
                return this.comboBox_bookOrSeries.Text;
            }
            set
            {
                this.comboBox_bookOrSeries.Text = value;
            }
        }

        // MARC格式
        public string MarcSyntax
        {
            get
            {
                if (this.Role != "书目库"
                    && this.Role != "规范库")
                    return null;

                return this.comboBox_marcSyntax.Text.ToLower();
            }
            set
            {
                this.comboBox_marcSyntax.Text = value.ToUpper();
            }
        }

        // 是否参与采购
        public bool IsOrder
        {
            get
            {
                if (this.Role != "书目库")
                    return false;

                return this.checkBox_order.Checked;
            }
            set
            {
                this.checkBox_order.Checked = value;
            }
        }

        // 是否包含实体库
        public bool HasEntityDb
        {
            get
            {
                if (this.Role != "书目库")
                    return false;

                return this.checkBox_hasEntityDb.Checked;
            }
            set
            {
                this.checkBox_hasEntityDb.Checked = value;
            }
        }

        // 是否参与流通
        public bool InCirculation
        {
            get
            {
                if (this.Role != "书目库"
                    && this.Role != "读者库")
                    return false;

                return this.checkBox_circulation.Checked;
            }
            set
            {
                this.checkBox_circulation.Checked = value;
            }
        }

        void OnRoleChanged()
        {
            if (this.comboBox_role.Text == "书目库")
            {
                this.groupBox_biblioDatabaseProperty.Text = " 书目库特性 ";
                this.groupBox_biblioDatabaseProperty.Visible = true;

                this.comboBox_bookOrSeries.Enabled = true;
                this.checkBox_order.Enabled = true;

                this.checkBox_circulation.Visible = true;

                this.checkBox_hasEntityDb.Visible = true;
            }
            else if (this.comboBox_role.Text == "规范库")
            {
                this.groupBox_biblioDatabaseProperty.Text = " 规范库特性 ";
                this.groupBox_biblioDatabaseProperty.Visible = true;

                this.comboBox_bookOrSeries.Enabled = false;
                this.checkBox_order.Enabled = false;

                this.checkBox_circulation.Visible = false;

                this.checkBox_hasEntityDb.Visible = false;
            }
            else
            {
                if (this.comboBox_role.Text == "读者库")
                    this.checkBox_circulation.Visible = true;
                else
                    this.checkBox_circulation.Visible = false;

                this.groupBox_biblioDatabaseProperty.Visible = false;
            }
        }

        private void comboBox_role_TextChanged(object sender, EventArgs e)
        {
            OnRoleChanged();
        }

        private void checkBox_circulation_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Role == "书目库")
            {
                if (this.checkBox_circulation.Checked == true)
                {
                    this.checkBox_hasEntityDb.Checked = true;
                    this.checkBox_hasEntityDb.Enabled = false;
                }
                else
                {
                    this.checkBox_hasEntityDb.Enabled = true;
                }
            }
            else
            {
                this.checkBox_hasEntityDb.Enabled = true;
            }
        }
    }
}