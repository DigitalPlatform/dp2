﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// 新增字段的对话框
    /// </summary>
    internal partial class NewFieldDlg : Form
    {
        public XmlDocument MarcDefDom = null;
        public string Lang = "zh";

        int nNested = 0;

        public NewFieldDlg()
        {
            InitializeComponent();
        }

        private void NewFieldDlg_Load(object sender, EventArgs e)
        {
            if (this.textBox_fieldName.Text == "")
                this.textBox_fieldName.Text = "???";

            this.AcceptButton = this.button_OK;

            string strError = "";
            int nRet = LoadFieldNames(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
        }

        int LoadFieldNames(out string strError)
        {
            strError = "";

            this.listView_fieldNameList.Items.Clear();

            if (this.MarcDefDom == null)
                return 0;

            XmlNodeList nodes = this.MarcDefDom.DocumentElement.SelectNodes("Field");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");

                if (strName == "###")
                    continue;   // 跳过头标区

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

#if NO
                XmlNode nodeLabel = null;
                try
                {
                    if (this.Lang == "")
                        nodeLabel = node.SelectSingleNode("Property/Label");
                    else
                    {
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                        nsmgr.AddNamespace("xml", Ns.xml);
                        nodeLabel = node.SelectSingleNode("Property/Label[@xml:lang='" + this.Lang + "']", nsmgr);
                    }
                }
                catch // 防止字段名中不合法字符用于xpath抛出异常
                {
                    nodeLabel = null;
                }

                string strLabel = "";
                if (nodeLabel != null)
                {
                    strLabel = DomUtil.GetNodeText(nodeLabel);
                }
#endif

                ListViewItem item = new ListViewItem(strName);
                item.SubItems.Add(strLabel);

                this.listView_fieldNameList.Items.Add(item);
            }

            return 0;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_fieldName.Text))
            {
                strError = "尚未输入字段名";
                goto ERROR1;
            }

            if (this.textBox_fieldName.Text.Length != 3)
            {
                strError = "字段名应为3字符";
                goto ERROR1;
            }

            if (this.textBox_fieldName.Text == "###")
            {
                strError = "不允许创建名字为 '###' 的字段";
                goto ERROR1;
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

        public string FieldName
        {
            get
            {
                return this.textBox_fieldName.Text;
            }
            set
            {
                this.textBox_fieldName.Text = value;
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

            if (this.listView_fieldNameList.SelectedItems.Count == 0)
                this.textBox_fieldName.Text = "";
            else
                this.textBox_fieldName.Text = this.listView_fieldNameList.SelectedItems[0].Text;

        }

        private void listView_fieldNameList_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(null, null);
        }

        private void textBox_fieldName_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void textBox_fieldName_KeyUp(object sender, KeyEventArgs e)
        {
            // 自动结束
            if (this.AutoComplete == true)
            {
                if (this.textBox_fieldName.SelectionStart >= 2
                    && this.textBox_fieldName.Text.Length == 3)
                {
                    button_OK_Click(null, null);
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

        // listview中的选定事项，跟随文本变化
        private void textBox_fieldName_TextChanged(object sender, EventArgs e)
        {
            if (nNested > 0)
                return; // 防止死循环

            nNested++;

            try
            {
                // bool bScrolled = false;
                int nStart = -1;
                int nEnd = -1;
                for (int i = 0; i < this.listView_fieldNameList.Items.Count; i++)
                {
                    string strText = this.listView_fieldNameList.Items[i].Text;

                    bool bHilight = false;

                    if (this.textBox_fieldName.Text.Length == 0)
                    {
                        bHilight = false;
                        goto CHANGE_COLOR;
                    }
                    else
                    {
                        if (this.textBox_fieldName.Text.Length < 3)
                        {
                            string strPart = strText.Substring(0, this.textBox_fieldName.Text.Length);

                            if (this.textBox_fieldName.Text == strPart)
                            {
                                bHilight = true;
                                goto CHANGE_COLOR;
                            }
                        }

                        if (this.textBox_fieldName.Text == strText)
                        {
                            bHilight = true;
                            goto CHANGE_COLOR;
                        }
                    }

                CHANGE_COLOR:
                    if (bHilight == false)
                    {
                        if (this.listView_fieldNameList.Items[i].BackColor != SystemColors.Window)
                            this.listView_fieldNameList.Items[i].BackColor = SystemColors.Window;
                        /*
                        if (this.listView_fieldNameList.Items[i].Selected != false)
                            this.listView_fieldNameList.Items[i].Selected = false;
                         * */
                    }
                    else
                    {
                        this.listView_fieldNameList.Items[i].BackColor = SystemColors.Info;

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
                    this.listView_fieldNameList.EnsureVisible(nEnd);
                if (nStart != -1)
                    this.listView_fieldNameList.EnsureVisible(nStart);

            }
            finally
            {
                nNested--;
            }
        }

    }
}