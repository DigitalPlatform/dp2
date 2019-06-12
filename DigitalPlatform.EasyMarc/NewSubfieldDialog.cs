using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using DigitalPlatform.Xml;

namespace DigitalPlatform.EasyMarc
{
    public partial class NewSubfieldDialog : Form
    {
        public XmlDocument MarcDefDom = null;
        public string Lang = "zh";

        int nNested = 0;

        public NewSubfieldDialog()
        {
            InitializeComponent();
        }

        private void NewSubfieldDialog_Load(object sender, EventArgs e)
        {
            if (this.textBox_name.Text == "")
                this.textBox_name.Text = "???";

            this.AcceptButton = this.button_OK;

            string strError = "";
            int nRet = LoadNameList(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            if (string.IsNullOrEmpty(this.textBox_name.Text) == false)
                HilightListItems(this.textBox_name.Text);
        }

        int LoadNameList(out string strError)
        {
            strError = "";

            this.listView_nameList.Items.Clear();

            if (this.MarcDefDom == null)
                return 0;

            XmlNodeList nodes = null;
            
            if (string.IsNullOrEmpty(this.ParentNameString) == true)
                nodes = this.MarcDefDom.DocumentElement.SelectNodes("Field");
            else
                nodes = this.MarcDefDom.DocumentElement.SelectNodes("Field[@name='"+this.ParentNameString+"']/Subfield");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");

                if (string.IsNullOrEmpty(this.ParentNameString) == true)
                {
                    if (strName == "###")
                        continue;   // 跳过头标区
                }

                string strLabel = "";

                XmlNode nodeProperty = node.SelectSingleNode("Property");
                if (nodeProperty != null)
                {
                    // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode的InnerText
                    // parameters:
                    //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
                    strLabel = DomUtil.GetXmlLangedNodeText(
                this.Lang,
                nodeProperty,
                "Label",
                true);
                }

                ListViewItem item = new ListViewItem(strName);
                item.SubItems.Add(strLabel);

                this.listView_nameList.Items.Add(item);
            }

            return 0;
        }

        string NameTypeString
        {
            get
            {
                if (string.IsNullOrEmpty(this.ParentNameString) == true)
                    return "字段名";
                return "子字段名";
            }
        }

        int NameStringLength
        {
            get
            {
                if (string.IsNullOrEmpty(this.ParentNameString) == true)
                    return 3;
                return 1;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_name.Text == "")
            {
                MessageBox.Show(this, "尚未输入" + NameTypeString);
                return;
            }

            if (string.IsNullOrEmpty(this.ParentNameString) == true)
            {
                if (this.textBox_name.Text.Length != 3)
                {
                    MessageBox.Show(this, NameTypeString + "应为3字符");
                    return;
                }
            }
            else
            {
                if (this.textBox_name.Text.Length != 1)
                {
                    MessageBox.Show(this, NameTypeString + "应为1字符");
                    return;
                }

            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        string _parentNameString = "";

        /// <summary>
        /// 父级对象的名字。
        /// 如果主要对象是子字段，这里就是字段的名字。如果主要对象是字段，则这里为空
        /// </summary>
        public string ParentNameString
        {
            get
            {
                return this._parentNameString;
            }
            set
            {
                this._parentNameString = value;
            }
        }

        public string NameString
        {
            get
            {
                return this.textBox_name.Text;
            }
            set
            {
                this.textBox_name.Text = value;
            }
        }

        public bool InsertBefore
        {
            get
            {
                if (this.radioButton_insertBefore.Checked == true)
                    return true;
                return false;
            }
            set
            {
                this.radioButton_insertBefore.Checked = value;
            }
        }


        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
            Keys keyData)
        {
            // 2006/11/14 changed
            if (keyData == Keys.Enter || keyData == Keys.LineFeed)
            {
                button_OK_Click(null, null);
                return true;
            }

            if (keyData == Keys.Insert)
            {
                button_OK_Click(null, null);
                return true;
            }

            if (keyData == Keys.Escape)
            {
                button_Cancel_Click(null, null);
                return true;
            }


            return false;
        }

        private void listView_fieldNameList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (nNested > 0)
                return; // 防止没有必要的重入

            if (this.listView_nameList.SelectedItems.Count == 0)
                this.textBox_name.Text = "";
            else
                this.textBox_name.Text = this.listView_nameList.SelectedItems[0].Text;

        }

        private void listView_fieldNameList_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(null, null);

        }

        private void textBox_fieldName_KeyUp(object sender, KeyEventArgs e)
        {
            // 自动结束
            if (string.IsNullOrEmpty(this.ParentNameString) == true)
            {
                if (this.AutoComplete == true)
                {
                    if (this.textBox_name.SelectionStart >= 2
                        && this.textBox_name.Text.Length == 3)
                    {
                        button_OK_Click(null, null);
                    }
                }
            }
            else
            {
                if (this.AutoComplete == true)
                {
                    if (this.textBox_name.SelectionStart >= 0
                        && this.textBox_name.Text.Length == 1)
                    {
                        button_OK_Click(null, null);
                    }
                }
            }

        }
        // 是否在字段名输入到最后一个字符时自动结束对话框
        public bool AutoComplete
        {
            get
            {
                return checkBox_autoComplete.Checked;
            }
            set
            {
                checkBox_autoComplete.Checked = value;
            }
        }

        private void textBox_name_TextChanged(object sender, EventArgs e)
        {
            if (nNested > 0)
                return; // 防止死循环

            nNested++;

            try
            {
                HilightListItems(this.textBox_name.Text);
            }
            finally
            {
                nNested--;
            }

        }

        // 加亮名字相关的行
        void HilightListItems(string strName)
        {
            // bool bScrolled = false;
            int nStart = -1;
            int nEnd = -1;
            for (int i = 0; i < this.listView_nameList.Items.Count; i++)
            {
                string strText = this.listView_nameList.Items[i].Text;

                bool bHilight = false;

                if (string.IsNullOrEmpty(strName) == true)
                {
                    bHilight = false;
                    goto CHANGE_COLOR;
                }
                else
                {
                    if (strName.Length < NameStringLength)
                    {
                        string strPart = strText.Substring(0, strName.Length);

                        if (strName == strPart)
                        {
                            bHilight = true;
                            goto CHANGE_COLOR;
                        }
                    }

                    if (strName == strText)
                    {
                        bHilight = true;
                        goto CHANGE_COLOR;
                    }
                }

            CHANGE_COLOR:
                if (bHilight == false)
                {
                    if (this.listView_nameList.Items[i].BackColor != SystemColors.Window)
                        this.listView_nameList.Items[i].BackColor = SystemColors.Window;
                    /*
                    if (this.listView_fieldNameList.Items[i].Selected != false)
                        this.listView_fieldNameList.Items[i].Selected = false;
                     * */
                }
                else
                {
                    this.listView_nameList.Items[i].BackColor = this.HilightColor;

                    // this.listView_fieldNameList.Items[i].Selected = true;
                    /*
                    if (bScrolled == false)
                    {
                        this.listView_fieldNameList.EnsureVisible(i);
                        bScrolled = true;
                    }
                     * */

                    if (nStart == -1)
                        nStart = i;
                    nEnd = i;
                }
            }

            if (nEnd != -1)
                this.listView_nameList.EnsureVisible(nEnd);
            if (nStart != -1)
                this.listView_nameList.EnsureVisible(nStart);
        }

        public Color HilightColor = Color.Red;
    }
}
