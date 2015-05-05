using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Xml;
using System.Diagnostics;
using DigitalPlatform.Xml;
//using DigitalPlatform.Text;
using DigitalPlatform.Text.SectionPropertyString;

namespace DigitalPlatform.CommonDialog
{
    public partial class CategoryPropertyDlg : Form
    {
        static Color CheckedBackColor = Color.FromArgb(180, 255, 180);
        static Color UncheckedBackColor = SystemColors.Window;
        string m_strCfgFileName = "";
        string m_strLang = "zh";

        XmlDocument CfgDom = null;

        DigitalPlatform.Text.SectionPropertyString.PropertyCollection m_pc = new DigitalPlatform.Text.SectionPropertyString.PropertyCollection();

        int DisableEvent = 0;

        public CategoryPropertyDlg()
        {
            InitializeComponent();
        }

        public string Lang
        {
            get
            {
                return m_strLang;
            }
            set
            {
                if (this.m_strLang == value)
                    return;

                this.m_strLang = value;

                if (this.Visible == false)
                    return;

                if (this.CfgDom == null)
                    return;

                // 刷新显示
                RefreshDisplay();

            }
        }

        public string CfgFileName
        {
            get
            {
                return m_strCfgFileName;
            }
            set
            {
                m_strCfgFileName = value;

                if (this.Visible == false)
                    return;

                string strError = "";
                // 装载配置文件
                // return:
                //      -1  error
                //      0   CfgFileName尚未指定
                //      1   成功
                int nRet = LoadCfgXml(out strError);
                if (nRet == -1)
                    throw new Exception(strError);
                if (nRet == 0)
                    return;


                this.RefreshDisplay();
            }
        }

        public void RefreshDisplay()
        {
            // 刷新显示
            string strError;
            int nRet = FillCategoryList(this.Lang,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            RefreshCategoryComboText();

            nRet = FillPropertyList(this.comboBox_category.Text,
                this.Lang,
                out strError);
            if (nRet == -1)
                goto ERROR1;

//             SetCheckList(this.m_pc);


            return;

        ERROR1:
            throw new Exception(strError);
        }

        // 刷新combo edit中冒号后面和语种有关的文字
        private void RefreshCategoryComboText()
        {
            if (this.CfgDom == null)
                return;

            if (String.IsNullOrEmpty(this.comboBox_category.Text) == true)
                return;

            int nRet = this.comboBox_category.Text.IndexOf(":");
            if (nRet == -1)
                return;

            string strName = this.comboBox_category.Text.Substring(0, nRet).Trim();
            if (String.IsNullOrEmpty(strName) == true)
                return;

            XmlNode node = CfgDom.DocumentElement.SelectSingleNode("//category[@name='"+strName+"']");
            if (node == null)
                return;

            string strComment = GetComment(node, this.Lang);
            if (String.IsNullOrEmpty(strComment) == true)
                return;

            this.comboBox_category.Text = strName + ": " + strComment;
        }

        public string PropertyString
        {
            get
            {
                if (this.m_pc == null)
                    return "";

                return this.m_pc.ToString();
            }
            set
            {
                this.m_pc = new DigitalPlatform.Text.SectionPropertyString.PropertyCollection(
                    "this", value, DelimiterFormat.Mix);

                if (this.Visible == false)
                    return;

                // 兑现到list中
                SetCheckList(this.m_pc);

            }
        }

        void SetCheckList(DigitalPlatform.Text.SectionPropertyString.PropertyCollection pc)
        {
            PrepareTag();

            string strCurCategory = this.SelectedCategory;
            if (String.IsNullOrEmpty(strCurCategory))
                strCurCategory = "*";

            for (int i = 0; i < m_pc.Count; i++)
            {
                Section section = m_pc[i];

                // 析出名字
                string strSectionName = section.Name;
                string strValues = section.ToString();

                if (String.IsNullOrEmpty(strSectionName) == true)
                    strSectionName = "this";

                // 看看是否在可显示范围
                if (strCurCategory == "*"
                    || String.Compare(strCurCategory, strSectionName, true) == 0)
                {
                }
                else
                    continue;

                for (int j = 0; j < section.Count; j++)
                {
                    Item propertyItem = section[j];

                    ListViewItem item = LocateListItem(strSectionName, propertyItem.Value);
                    if (item == null)
                    {
                        // 未定义的保留值
                    }
                    else
                    {
                        if (item.Checked != true)
                        {
                            item.BackColor = CheckedBackColor;
                            item.Checked = true;
                            item.Tag = null;    // 标记，表示本次触及到了
                        }
                        else
                            item.Tag = null;    // 不用动
                    }

                }
            }

            // uncheck本次没有on的
            for (int i = 0; i < this.listView_property.Items.Count; i++)
            {
                ListViewItem item = this.listView_property.Items[i];
                if (item.Tag == null)
                {

                }
                else {
                    if ((bool)item.Tag == false && item.Checked != false)
                    {
                        item.BackColor = UncheckedBackColor;
                        item.Checked = false;
                    }

                    if ((bool)item.Tag == true && item.Checked != true)
                    {
                        item.BackColor = CheckedBackColor;
                        item.Checked = true;
                    }

                }

           }
        }

        void PrepareTag()
        {
            for (int i = 0; i < this.listView_property.Items.Count; i++)
            {
                ListViewItem item = this.listView_property.Items[i];
                item.Tag = false;
            }
        }


        // 根据目录名和值定位listview item
        ListViewItem LocateListItem(string strCategory, string strValue)
        {
            for (int i = 0; i < this.listView_property.Items.Count; i++)
            {
                ListViewItem item = this.listView_property.Items[i];
                if (String.Compare(strCategory, item.SubItems[0].Text, true) != 0)
                    continue;
                if (String.Compare(strValue, item.SubItems[1].Text, true) == 0)
                    return item;
            }

            return null;
        }

        public string SelectedCategory
        {
            get
            {
                if (String.IsNullOrEmpty(this.comboBox_category.Text) == true)
                    return "";

                int nRet = this.comboBox_category.Text.IndexOf(":");
                if (nRet == -1)
                    return this.comboBox_category.Text;

                string strName = this.comboBox_category.Text.Substring(0, nRet).Trim();
                if (String.IsNullOrEmpty(strName) == true)
                    return "";

                return strName;
            }
            set
            {
                this.comboBox_category.Text = value;

                // 包含冒号的全名称
                for (int i = 0; i < this.comboBox_category.Items.Count; i++)
                {
                    string strLine = (string)this.comboBox_category.Items[i];
                    string strCurName = "";
                    int nRet = strLine.IndexOf(":");
                    if (nRet == -1)
                        strCurName = strLine.Trim();
                    else
                        strCurName = strLine.Substring(0, nRet).Trim();

                    if (String.IsNullOrEmpty(strCurName) == true)
                        continue;

                    if (value == strCurName)
                    {
                        this.comboBox_category.Text = strLine;
                        break;
                    }
                }
            }
        }

        private void CategoryPropertyDlg_Load(object sender, EventArgs e)
        {
            string strError = "";

            // 装载配置文件
        // return:
        //      -1  error
        //      0   CfgFileName尚未指定
        //      1   成功
            int nRet = LoadCfgXml(out strError);
            if (nRet == 0)
                return;
            if (nRet == -1)
                goto ERROR1;

            nRet = FillCategoryList(this.Lang,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            SelectFirstCategory();

            nRet = FillPropertyList(this.comboBox_category.Text,
                this.Lang,
                out strError);

            if (nRet == -1)
                goto ERROR1;

            // SetCheckList(this.m_pc);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }



        private void button_checkAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listView_property.Items.Count; i++)
            {
                this.listView_property.Items[i].Checked = true;
            }
        }

        private void button_uncheckAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listView_property.Items.Count; i++)
            {
                this.listView_property.Items[i].Checked = false;
            }
        }

        int FillCategoryList(string strLang,
            out string strError)
        {
            strError = "";

            Debug.Assert(this.CfgDom != null, "配置文件CfgDom尚未初始化...");


            string strXPath = "";

            this.comboBox_category.Items.Clear();



            strXPath = "//category";
            XmlNodeList nodes = this.CfgDom.DocumentElement.SelectNodes(strXPath);

            bool bFoundWildchar = false;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strComment = GetComment(node, strLang);

                if (strName == "*")
                    bFoundWildchar = true;

                string strText = strName;
                if (String.IsNullOrEmpty(strComment) == false)
                    strText += ": " + strComment;

                this.comboBox_category.Items.Add(strText);
            }

            if (bFoundWildchar == false)
            {
                // 表示全部类目的缺省事项
                this.comboBox_category.Items.Insert(0, "*: 全部事项");
            }


            return 0;
        }

        // 选择Combo列表中第一个事项
        int SelectFirstCategory()
        {
            if (this.comboBox_category.Items.Count == 0)
                return 0;
            this.comboBox_category.Text = (string)this.comboBox_category.Items[0];
            return 1;
        }

        // 填充listview
        // parameters:
        //      strCategory 类目名称。如果=="*"，表示全部类目
        int FillPropertyList(string strCategory,
            string strLang,
            out string strError)
        {
            strError = "";
            string strXPath = "";

            Debug.Assert(this.CfgDom != null, "配置文件CfgDom尚未初始化...");

            this.listView_property.Items.Clear();

            // 去掉冒号以后的部分
            if (String.IsNullOrEmpty(strCategory) == false)
            {
                int nRet = strCategory.IndexOf(":");
                if (nRet != -1)
                    strCategory = strCategory.Substring(0, nRet).Trim();
            }

            if (strCategory == "*" || String.IsNullOrEmpty(strCategory))
                strXPath = "//category/property";
            else
                strXPath = "//category[@name='" + strCategory + "']/property";
            XmlNodeList nodes = this.CfgDom.DocumentElement.SelectNodes(strXPath);

            if (nodes.Count == 0)
                return 0;

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strCategoryName = DomUtil.GetAttr(node.ParentNode, "name");
                string strValue = DomUtil.GetAttr(node, "name");

                string strComment = GetComment(node, strLang);

                ListViewItem item = new ListViewItem(strCategoryName, 0);

                item.SubItems.Add(strValue);
                item.SubItems.Add(strComment);

                ItemState state = this.m_pc.GetItemState(strCategoryName,
                    strValue);
                if (state == ItemState.On)
                    item.Checked = true;

                item.Tag = false;   // 为下次作准备

                DisableEvent ++;
                this.listView_property.Items.Add(item);
                DisableEvent --;
            }

            return 0;
        }

        // 获得一个节点(有关语言的)的注释值
        private string GetComment(XmlNode node, string strLang)
        {
            XmlNode nodeComment = node.SelectSingleNode("comment[@lang='"+strLang+"']");
            if (nodeComment == null)
            {
                nodeComment = node.SelectSingleNode("comment");
            }

            if (nodeComment == null)
                return "";

            return DomUtil.GetNodeText(nodeComment);
        }

        // 装载配置文件
        // return:
        //      -1  error
        //      0   CfgFileName尚未指定
        //      1   成功
        int LoadCfgXml(out string strError)
        {
            strError = "";

            if (CfgFileName == "")
                return 0;

            this.CfgDom = new XmlDocument();

            try
            {
                this.CfgDom.Load(CfgFileName);
            }
            catch (Exception ex)
            {
                strError = "配置文件 '" + this.CfgFileName + "' 装载入XmlDocument时发生错误: " + ex.Message;
                return -1;
            }

            return 1;
        }

        private void comboBox_category_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = FillPropertyList(this.comboBox_category.Text,
                this.Lang,
                out strError);

            if (nRet == -1)
                goto ERROR1;

            SetTextBoxString();

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        void SetTextBoxString()
        {
            this.DisableEvent++;

            // 重新显示出textbox字符串
            string strCurCategory = this.SelectedCategory;
            if (String.IsNullOrEmpty(strCurCategory))
                strCurCategory = "*";

            string strCurResult = "";

            if (strCurCategory == "*")
            {
                this.label_property.Text = "所有类目之值(&V):";

            }
            else
            {
                this.label_property.Text = "类目 '" + strCurCategory + "' 之值(&V):";

                // strCurResult = this.m_pc[this.SelectedCategory].Value;

            }

            // 针对当前已选择类目的字符串结果
            strCurResult = this.m_pc.ToString(DelimiterFormat.CrLf,
                strCurCategory);

            if (this.textBox_property.Text != strCurResult) // if 可避免消息引起死循环
                this.textBox_property.Text = strCurResult;  // 当前选择类目的字符串

            this.DisableEvent --;
        }

        private void textBox_property_TextChanged(object sender, EventArgs e)
        {
            if (this.DisableEvent > 0)
                return;

            REDO:
            string strCurCategory = this.SelectedCategory;
            if (String.IsNullOrEmpty(strCurCategory))
                strCurCategory = "*";

            if (strCurCategory == "*")
            {
                this.m_pc = new DigitalPlatform.Text.SectionPropertyString.PropertyCollection(
                    "this", this.textBox_property.Text, DelimiterFormat.Mix);
            }
            else
            {
                // 观察是否有两个以上的冒号
                int nRet = this.textBox_property.Text.IndexOf(":");
                if (nRet != -1)
                {
                    nRet = this.textBox_property.Text.IndexOf(":", nRet + 1);
                    if (nRet != -1)
                    {
                        // 先切换为*类目，然后继续执行changetext功能
                        string strSaveText = this.textBox_property.Text;
                        this.SelectedCategory = "*";
                        this.textBox_property.Text = strSaveText;
                        goto REDO;
                    }
                }
                m_pc.NewSection("this", 
                    strCurCategory, 
                    this.textBox_property.Text);
            }

            SetCheckList(this.m_pc);
        }

        private void listView_property_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Checked == true)
            {
                e.Item.BackColor = CheckedBackColor;
            }
            else
            {
                e.Item.BackColor = UncheckedBackColor;
            }

            if (DisableEvent > 0)
                return;
            // 修改字符串
            ModiPropertyString(e.Item.Text, e.Item.SubItems[1].Text, e.Item.Checked);
        }

        // 修改属性字符串
        private void ModiPropertyString(string strCategory, string strValue, bool bAdd)
        {
            if (this.m_pc == null)
                return;
            // bool bFoundCategory = false;
            bool bFoundValue = false;

            for (int i = 0; i < this.m_pc.Count; i++)
            {
                Section section = this.m_pc[i];

                if (section.Count == 0)
                    continue;

                if (String.Compare(section.Name, strCategory, true) == 0)
                {
                    // bFoundCategory = true;

                    for (int j = 0; j < section.Count; j++)
                    {
                        Item item = section[j];

                        string strCurValue = item.Value;

                        if (String.Compare(strCurValue, strValue, true) == 0)
                        {
                            bFoundValue = true;

                            if (bAdd == true)   // 要加入，但是已经存在了 :-(
                                return;
                            if (bAdd == false)
                            {
                                // 去掉位置j的即可
                                section.RemoveAt(j);
                                break;
                            }
                        }

                    }

                }
            }

            if (bFoundValue == false && bAdd == true)
            {
                Item item = m_pc.NewItem(strCategory, strValue);
                Debug.Assert(item != null, "NewItem()失败");
            }

            /*
            string strCurCategory = this.SelectedCategory;
            if (String.IsNullOrEmpty(strCurCategory))
                strCurCategory = "*";

            // 针对当前已选择类目的字符串结果
            string strCurResult = this.m_pc.ToString(DelimiterFormat.CrLf,
                strCurCategory);

            if (this.textBox_property.Text != strCurResult) // if 可避免消息引起死循环
                this.textBox_property.Text = strCurResult;  // 当前选择类目的字符串
             */
            SetTextBoxString();

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

 
}