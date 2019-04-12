using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Generic;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using System.Text;
using DigitalPlatform.Core;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// Summary description for PropertyDlg.
    /// </summary>
    public class PropertyDlg : System.Windows.Forms.Form
    {
        public string CfgFileName = "";
        public string Lang = "zh";

        public string PropertyString = "";

        // List<string> _propertyNameList = null;	// 配置中出现过的属性名
        Hashtable _propertyNameTable = new Hashtable(); // 配置中出现过的属性名。属性名 --> ListViewItem

        List<string> _langNameList = null;	// 配置中出现过的语言类型

        ListViewItem tipsItem = null;

        private System.Windows.Forms.Label label_property;
        private System.Windows.Forms.TextBox textBox_property;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private DigitalPlatform.GUI.ListViewNF listView_property;
        private System.Windows.Forms.ToolTip toolTip_comment;
        private System.Windows.Forms.Button button_checkAll;
        private System.Windows.Forms.Button button_uncheckAll;
        private SplitContainer splitContainer_main;
        private Panel panel_up;
        private Panel panel_down;
        private ToolStrip toolStrip1;
        private ToolStripDropDownButton toolStripDropDownButton_quickSet;
        private System.ComponentModel.IContainer components;

        public PropertyDlg()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyDlg));
            this.label_property = new System.Windows.Forms.Label();
            this.textBox_property = new System.Windows.Forms.TextBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.listView_property = new DigitalPlatform.GUI.ListViewNF();
            this.toolTip_comment = new System.Windows.Forms.ToolTip(this.components);
            this.button_checkAll = new System.Windows.Forms.Button();
            this.button_uncheckAll = new System.Windows.Forms.Button();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.panel_up = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton_quickSet = new System.Windows.Forms.ToolStripDropDownButton();
            this.panel_down = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.panel_up.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panel_down.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_property
            // 
            this.label_property.AutoSize = true;
            this.label_property.Location = new System.Drawing.Point(-2, 4);
            this.label_property.Name = "label_property";
            this.label_property.Size = new System.Drawing.Size(41, 12);
            this.label_property.TabIndex = 1;
            this.label_property.Text = "值(&V):";
            this.label_property.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_property
            // 
            this.textBox_property.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_property.Location = new System.Drawing.Point(0, 18);
            this.textBox_property.MaxLength = 0;
            this.textBox_property.Multiline = true;
            this.textBox_property.Name = "textBox_property";
            this.textBox_property.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_property.Size = new System.Drawing.Size(586, 81);
            this.textBox_property.TabIndex = 2;
            this.textBox_property.TextChanged += new System.EventHandler(this.textBox_property_TextChanged);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.Location = new System.Drawing.Point(535, 373);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(60, 22);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(471, 373);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(60, 22);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // listView_property
            // 
            this.listView_property.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_property.CheckBoxes = true;
            this.listView_property.FullRowSelect = true;
            this.listView_property.HideSelection = false;
            this.listView_property.Location = new System.Drawing.Point(0, 0);
            this.listView_property.Name = "listView_property";
            this.listView_property.Size = new System.Drawing.Size(586, 207);
            this.listView_property.TabIndex = 0;
            this.listView_property.UseCompatibleStateImageBehavior = false;
            this.listView_property.View = System.Windows.Forms.View.Details;
            this.listView_property.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_property_ColumnClick);
            this.listView_property.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listView_property_ItemCheck);
            this.listView_property.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView_property_MouseMove);
            this.listView_property.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_property_MouseUp);
            // 
            // toolTip_comment
            // 
            this.toolTip_comment.AutomaticDelay = 1000;
            this.toolTip_comment.AutoPopDelay = 5000;
            this.toolTip_comment.InitialDelay = 1000;
            this.toolTip_comment.IsBalloon = true;
            this.toolTip_comment.ReshowDelay = 1000;
            this.toolTip_comment.ShowAlways = true;
            this.toolTip_comment.UseAnimation = false;
            this.toolTip_comment.UseFading = false;
            // 
            // button_checkAll
            // 
            this.button_checkAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_checkAll.AutoSize = true;
            this.button_checkAll.Location = new System.Drawing.Point(0, 212);
            this.button_checkAll.Name = "button_checkAll";
            this.button_checkAll.Size = new System.Drawing.Size(68, 22);
            this.button_checkAll.TabIndex = 5;
            this.button_checkAll.Text = "全选(&A)";
            this.button_checkAll.Click += new System.EventHandler(this.button_checkAll_Click);
            // 
            // button_uncheckAll
            // 
            this.button_uncheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_uncheckAll.AutoSize = true;
            this.button_uncheckAll.Location = new System.Drawing.Point(73, 212);
            this.button_uncheckAll.Name = "button_uncheckAll";
            this.button_uncheckAll.Size = new System.Drawing.Size(66, 22);
            this.button_uncheckAll.TabIndex = 6;
            this.button_uncheckAll.Text = "清除(&C)";
            this.button_uncheckAll.Click += new System.EventHandler(this.button_uncheckAll_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(9, 10);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.panel_up);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.panel_down);
            this.splitContainer_main.Size = new System.Drawing.Size(586, 341);
            this.splitContainer_main.SplitterDistance = 234;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 7;
            // 
            // panel_up
            // 
            this.panel_up.Controls.Add(this.toolStrip1);
            this.panel_up.Controls.Add(this.listView_property);
            this.panel_up.Controls.Add(this.button_uncheckAll);
            this.panel_up.Controls.Add(this.button_checkAll);
            this.panel_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_up.Location = new System.Drawing.Point(0, 0);
            this.panel_up.Name = "panel_up";
            this.panel_up.Size = new System.Drawing.Size(586, 234);
            this.panel_up.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton_quickSet});
            this.toolStrip1.Location = new System.Drawing.Point(505, 209);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(81, 25);
            this.toolStrip1.TabIndex = 7;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton_quickSet
            // 
            this.toolStripDropDownButton_quickSet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_quickSet.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_quickSet.Image")));
            this.toolStripDropDownButton_quickSet.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_quickSet.Name = "toolStripDropDownButton_quickSet";
            this.toolStripDropDownButton_quickSet.Size = new System.Drawing.Size(69, 22);
            this.toolStripDropDownButton_quickSet.Text = "快速设定";
            // 
            // panel_down
            // 
            this.panel_down.Controls.Add(this.label_property);
            this.panel_down.Controls.Add(this.textBox_property);
            this.panel_down.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_down.Location = new System.Drawing.Point(0, 0);
            this.panel_down.Name = "panel_down";
            this.panel_down.Size = new System.Drawing.Size(586, 99);
            this.panel_down.TabIndex = 0;
            // 
            // PropertyDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(604, 404);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PropertyDlg";
            this.ShowInTaskbar = false;
            this.Text = "PropertyDlg";
            this.Load += new System.EventHandler(this.PropertyDlg_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.panel_up.ResumeLayout(false);
            this.panel_up.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel_down.ResumeLayout(false);
            this.panel_down.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void PropertyDlg_Load(object sender, System.EventArgs e)
        {
            SetListViewTitle(listView_property);

            toolTip_comment.SetToolTip(this.listView_property, "tool tip text");

            this.BeginInvoke(new Action(Initial));
        }

        void Initial()
        {
            LoadXml();

            textBox_property.Text = PropertyString;

            ChangeColor();
        }

        public void SetListViewTitle(ListView listView)
        {
            listView.Columns.Add("属性值", 200, HorizontalAlignment.Left);
            listView.Columns.Add("说明", 900, HorizontalAlignment.Left);
        }

        // 一个事项的附加数据
        class ItemInfo
        {
            public List<string> AliasList { get; set; }
        }

        // 装载一个 XML 文件
        int LoadOneXml(string strFileName,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            XmlNodeList propertyList = dom.SelectNodes("root/property");

            // int j;
            foreach (XmlElement node in propertyList)
            {
                // 找到事项名字
                string strName = DomUtil.GetAttr(node, "name");

                if (string.IsNullOrEmpty(strName))
                    continue;

                // 按照语言找到comment字符串
                XmlNode nodeComment = null;

                if (Lang == "")
                    nodeComment = node.SelectSingleNode("comment");
                else
                {
                    nodeComment = node.SelectSingleNode("comment[@lang='" + Lang + "']");
                    if (nodeComment == null)	// 按照指定的语言找，但是没有找到
                        nodeComment = node.SelectSingleNode("comment");
                }

                string strComment = "";

                if (nodeComment != null)
                {
                    strComment = DomUtil.GetNodeText(nodeComment);
                }


                ListViewItem item = new ListViewItem(strName, 0);
                item.SubItems.Add(strComment);

                // 2017/4/20
                string strAlias = node.GetAttribute("alias");
                if (string.IsNullOrEmpty(strAlias) == false)
                {
                    ItemInfo info = new ItemInfo();
                    info.AliasList = StringUtil.SplitList(strAlias);
                    item.Tag = info;
                }

                listView_property.Items.Add(item);
            }

            // 创建语言数组
            XmlNodeList commentList = dom.SelectNodes("root/property/comment");

            _langNameList = new List<string>(); // = new ArrayList();

            foreach (XmlNode node in commentList)
            {
                // 找到事项名字
                string strLang = DomUtil.GetAttr(node, "lang");

                if (strLang == "")
                    continue;

#if NO
                bool bFound = false;
                for (j = 0; j < _langNameList.Count; j++)
                {
                    if (strLang == _langNameList[j])
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                    _langNameList.Add(strLang);
#endif
                if (_langNameList.IndexOf(strLang) == -1)
                    _langNameList.Add(strLang);

            }

            return 0;
        }

        void LoadXml()
        {
            _langNameList = new List<string>();
            // _propertyNameList = new List<string>(); // = new string[listView_property.Items.Count];
            _propertyNameTable = new Hashtable();

            listView_property.Items.Clear();
            this.toolStripDropDownButton_quickSet.DropDownItems.Clear();

            string strError = "";

            if (string.IsNullOrEmpty(this.CfgFileName) == true)
                return;

            string[] filenames = this.CfgFileName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            this.listView_property.BeginUpdate();
            foreach (string filename in filenames)
            {
                // 装载一个 XML 文件
                int nRet = LoadOneXml(filename,
            out strError);
                if (nRet == -1)
                    goto ERROR1;
                nRet = FillQuickSetMenu(filename,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            this.listView_property.EndUpdate();

#if NO
			XmlDocument dom = new XmlDocument();

			try 
			{
				dom.Load(CfgFileName);
			}
			catch (Exception ex) 
			{
				strErrorInfo = ex.Message;
				goto ERROR1;

			}

			XmlNodeList propertyList = dom.SelectNodes("root/property");

			int i,j;
			for(i=0;i<propertyList.Count;i++) 
			{
				// 找到事项名字
				string strName = DomUtil.GetAttr(propertyList[i], "name");

				if (strName == "")
					continue;

				// 按照语言找到comment字符串
				XmlNode nodeComment = null;

				if (Lang == "") 
					nodeComment = propertyList[i].SelectSingleNode("comment");
				else 
				{
					nodeComment = propertyList[i].SelectSingleNode("comment[@lang='"+Lang+"']");
					if (nodeComment == null)	// 按照指定的语言找，但是没有找到
						nodeComment = propertyList[i].SelectSingleNode("comment");
				}

				string strComment = "";

				if (nodeComment != null) 
				{
					strComment = DomUtil.GetNodeText(nodeComment);
				}

				ListViewItem item =
					new ListViewItem(strName,
					0);

				item.SubItems.Add(strComment);

				listView_property.Items.Add(item);	

			}
#endif

            // 创建字符串数组，便于随时查重
            foreach (ListViewItem item in listView_property.Items)
            {
                // _propertyNameList[j] = listView_property.Items[j].Text;

                // _propertyNameList.Add(item.Text);

                _propertyNameTable[item.Text.ToLower()] = item;

                // 2017/4/20
                // 别名也要加入 hashtable
                ItemInfo info = (ItemInfo)item.Tag;
                if (info != null && info.AliasList != null)
                {
                    foreach (string alias in info.AliasList)
                    {
                        if (string.IsNullOrEmpty(alias) == false)
                            _propertyNameTable[alias.ToLower()] = item;
                    }
                }
            }

#if NO
			// 创建语言数组
			XmlNodeList commentList = dom.SelectNodes("//property/comment");

			aLangName = new ArrayList();

			for(i=0;i<propertyList.Count;i++) 
			{
				// 找到事项名字
				string strLang = DomUtil.GetAttr(commentList[i], "lang");

				if (strLang == "")
					continue;

				bool bFound = false;
				for(j=0;j<aLangName.Count;j++) 
				{
					if (strLang == (string)aLangName[j]) 
					{
						bFound = true;
						break;
					}
				}

				if (bFound == false)
					aLangName.Add(strLang);

			}
#endif

            if (this.toolStripDropDownButton_quickSet.DropDownItems.Count == 0)
                this.toolStripDropDownButton_quickSet.Visible = false;
            else
                this.toolStripDropDownButton_quickSet.Visible = true;
            return;
        ERROR1:
            MessageBox.Show(strError);
        }

#if NO
        // 是否为已经定义的属性名
        bool IsDefinedPropertyName(string strName)
        {
            if (this._propertyNameList == null)
                return false;

            foreach (string name in _propertyNameList)
            {
                if (String.Compare(strName, name, true) == 0)
                    return true;
            }

            return false;
        }
#endif
        // 是否为已经定义的属性名
        bool IsDefinedPropertyName(string strName)
        {
            if (this._propertyNameTable == null)
                return false;

            return _propertyNameTable.ContainsKey(strName.ToLower());
        }

        // 获得一个列表中属于当前没有定义的属性名
        List<string> GetNoDefinedPropertyNames(string strList)
        {
            List<string> aResult = new List<string>();
            List<string> names = StringUtil.SplitList(strList);
            // string[] aName = strList.Split(new Char[] { ',' });

            // for (int i = 0; i < aName.Length; i++)
            foreach (string name in names)
            {
                string strName = name.Trim();
                if (string.IsNullOrEmpty(strName))
                    continue;

                if (IsDefinedPropertyName(strName) == true)
                    continue;

                aResult.Add(strName);
            }

            return aResult;
        }

        // 获得一个列表中属于当前定义的属性名
        List<string> GetDefinedPropertyNames(string strList)
        {
            List<string> aResult = new List<string>();
            string[] aName = strList.Split(new Char[] { ',' });

            foreach (string s in aName)
            {
                // string strName = aName[i];
                string strName = s.Trim();
                if (strName == "")
                    continue;

                if (IsDefinedPropertyName(strName) == false)
                    continue;

                aResult.Add(strName);
            }

            return aResult;
        }

        int _skipItemChecked = 0;   // 2016/3/26

        private void listView_property_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
        {
            if (_skipItemChecked > 0)
                return;

            // 得到checked事项
            List<ListViewItem> checkedItems = GetCheckedItems(listView_property, e);

            // 获得edit中属于没有定义的部分
            List<string> aNotDefined = GetNoDefinedPropertyNames(textBox_property.Text);

            StringBuilder text = new StringBuilder();

            // checked组合为字符串
            foreach (ListViewItem item in checkedItems)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append(item.Text);
            }

            // 没有定义部分组合为字符串
            foreach (string s in aNotDefined)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append(s);
            }
            string[] aOld = textBox_property.Text.Split(new Char[] { ',' });
            string[] aNew = text.ToString().Split(new Char[] { ',' });
            Array.Sort(aOld);
            Array.Sort(aNew);

            if (String.Compare(String.Join(",", aOld), String.Join(",", aNew), true) != 0)
            {
                textBox_property.Text = text.ToString();
            }

            ChangeColor();
        }

        void ChangeColor()
        {
            for (int i = 0; i < this.listView_property.Items.Count; i++)
            {
                if (this.listView_property.Items[i].Checked == false)
                {
                    this.listView_property.Items[i].ForeColor = SystemColors.WindowText;
                    this.listView_property.Items[i].BackColor = SystemColors.Window;
                }
                else
                {
                    this.listView_property.Items[i].ForeColor = SystemColors.MenuText;
                    this.listView_property.Items[i].BackColor = SystemColors.Menu;
                }
            }
        }

        bool FindNameInList(string strItemName, List<string> aDefined)
        {
            for (int k = 0; k < aDefined.Count; k++)
            {
                if (String.Compare(strItemName, (string)aDefined[k], true) == 0)
                    return true;
            }

            return false;
        }

        bool FindNamesInList(List<string> names, List<string> list)
        {
            foreach (string name in names)
            {
                if (FindNameInList(name, list))
                    return true;
            }

            return false;
        }

        private void textBox_property_TextChanged(object sender, System.EventArgs e)
        {
            //bool bChanged = false;
            // 提取已定义的部分
            List<string> aDefined = GetDefinedPropertyNames(textBox_property.Text);

            // check
            foreach (string strName in aDefined)
            {
                // string strName = (string)aDefined[i];

                //bool bRet = 
                CheckItem(strName, true);
                //if (bRet == true)
                //	bChanged = true;
            }

            // uncheck
            foreach (ListViewItem item in listView_property.Items)
            {
                List<string> alias_list = new List<string>();
                ItemInfo info = (ItemInfo)item.Tag;
                if (info != null)
                    alias_list = info.AliasList;

                alias_list.Insert(0, item.Text);

#if NO
                bool bFound = false;
                for (int k = 0; k < aDefined.Count; k++)
                {
                    if (String.Compare(strItemName, (string)aDefined[k], true) == 0)
                    {
                        bFound = true;	// 属于需要on的事项
                        break;
                    }
                }
#endif
                bool bFound = FindNamesInList(alias_list, aDefined);

                // 属于需要off的事项
                if (bFound == false)
                {
                    if (item.Checked == false)
                        continue;
                    item.Checked = false;
                    //bChanged = true;
                }

            }

        }

#if NO
        bool CheckItem(string strName, bool bChecked)
        {
            foreach (ListViewItem item in listView_property.Items)
            {
                if (String.Compare(strName, item.Text, true) == 0)
                {
                    if (item.Checked == bChecked)
                        return false;	// 没有改变状态
                    else
                    {
                        item.Checked = bChecked;
                        return true;	// 改变了状态
                    }
                }
            }

            return false;	// 没有找到事项
        }
#endif

        // 2016/3/26 优化速度
        bool CheckItem(string strName, bool bChecked)
        {
            ListViewItem item = (ListViewItem)_propertyNameTable[strName.ToLower()];
            if (item == null)
                return false;	// 没有找到事项

            if (item.Checked == bChecked)
                return false;	// 没有改变状态
            else
            {
                _skipItemChecked++;
                item.Checked = bChecked;
                _skipItemChecked--;
                return true;	// 改变了状态
            }
        }

        // 得到所有checked Item
        // 是当前的所有checked item加上考虑e参数中可能增加和减除的item，合并而成
        // e可以为null，表示不关心这个特殊情况
        List<ListViewItem> GetCheckedItems(System.Windows.Forms.ListView listview,
            System.Windows.Forms.ItemCheckEventArgs e)
        {
            List<ListViewItem> selectedItems = new List<ListViewItem>();
            foreach (ListViewItem item in listview.CheckedItems)
            {
                selectedItems.Add(item);
            }
            // selectedItems.AddRange(listview.CheckedItems);

            if (e != null)
            {
                if (e.NewValue == CheckState.Checked)
                    selectedItems.Add(listview.Items[e.Index]);
                else
                {
                    selectedItems.Remove(listview.Items[e.Index]);
                }
            }

            return selectedItems;
        }

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            PropertyString = textBox_property.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void listView_property_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ListViewItem selection = listView_property.GetItemAt(e.X, e.Y);

            // If the user selects an item in the ListView, display
            // the image in the PictureBox.
            if (selection != null)
            {
                if (selection != tipsItem)
                {
                    toolTip_comment.SetToolTip(this.listView_property, selection.SubItems[1].Text);
                }
            }
            else
            {
                toolTip_comment.SetToolTip(listView_property, "");
            }

            tipsItem = selection;
        }

        private void listView_property_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("选择语言(&L)");
            contextMenu.MenuItems.Add(menuItem);

            // 子菜单
            for (int i = 0; i < _langNameList.Count; i++)
            {
                MenuItem menuItemSub = new MenuItem(_langNameList[i]);
                menuItemSub.Click += new System.EventHandler(this.menu_selectLanguage_Click);

                menuItem.MenuItems.Add(menuItemSub);

                if (_langNameList[i] == Lang)
                {
                    menuItemSub.Enabled = false;
                    menuItemSub.Checked = true;
                }

            }

            contextMenu.Show(listView_property, new Point(e.X, e.Y));
        }

        // 选择了语言
        private void menu_selectLanguage_Click(object sender, System.EventArgs e)
        {
            if (sender is MenuItem)
            {
                string strSave = textBox_property.Text;

                MenuItem menuItem = (MenuItem)sender;
                // MessageBox.Show(menuItem.Text);
                Lang = menuItem.Text;
                LoadXml();

                textBox_property.Text = strSave;
                textBox_property_TextChanged(null, null);
            }

        }

        private void button_checkAll_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < this.listView_property.Items.Count; i++)
            {
                this.listView_property.Items[i].Checked = true;
            }

        }

        private void button_uncheckAll_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < this.listView_property.Items.Count; i++)
            {
                this.listView_property.Items[i].Checked = false;
            }
        }

        SortColumns SortColumns = new SortColumns();

        private void listView_property_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns.SetFirstColumn(nClickColumn,
                this.listView_property.Columns);

            // 排序
            this.listView_property.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView_property.ListViewItemSorter = null;
        }

        int FillQuickSetMenu(string strFileName, out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strFileName + "' 时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList groups = dom.DocumentElement.SelectNodes("groups/group");
            foreach (XmlElement group in groups)
            {
                string strCaption = DomUtil.GetCaption(this.Lang, group);
                ToolStripItem item = new ToolStripMenuItem(strCaption);
                item.Tag = group.GetAttribute("value");
                item.Click += item_Click;
                this.toolStripDropDownButton_quickSet.DropDownItems.Add(item);
            }
            return 0;
        }

        void item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string strValue = item.Tag as string;
            if (Control.ModifierKeys == Keys.Control)
                textBox_property.Text = StringUtil.MergeList(textBox_property.Text, strValue, false);
            else
                textBox_property.Text = strValue;
        }
    }
}
