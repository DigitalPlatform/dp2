using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Threading;
using System.Reflection;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.Drawing;

namespace DigitalPlatform.Marc
{

	public class MarcFixedFieldControl : System.Windows.Forms.ScrollableControl
	{
        public event GetTemplateDefEventHandler GetTemplateDef = null;  // 外部接口，获取一个特定模板的XML定义
        public event EventHandler ResetSize = null;
        public event ResetTitleEventHandler ResetTitle = null;


		#region 成员变量

        XmlNode nodeTemplateDef = null;

        List<XmlNode> CurValueListNodes = null;    // 当前正在显示的值列表节点
        string CurActiveValueUnit = "";  // 当前已经加亮的值事项

		public XmlDocument MarcDefDom = null;  // 配置文件的dom
		public string Lang = "";

		TemplateRoot templateRoot = null;
		private System.Windows.Forms.ListView listView_values = null;
		public int nCurLine = -1;

        Font m_fontDefaultInfo = null;
		// 文字的字体字号
        public Font DefaultInfoFont
        {
            get
            {
                if (this.m_fontDefaultInfo == null)
                    this.m_fontDefaultInfo = this.Font;

                return this.m_fontDefaultInfo;
            }
        }

        Font m_fontDefaultValue = null;
        private ImageList imageList_lineState;
        private IContainer components;
    
		public Font DefaultValueFont
        {
            get
            {
                if (this.m_fontDefaultValue == null)
                    this.m_fontDefaultValue = new Font("Courier New", this.Font.SizeInPoints, GraphicsUnit.Point);

                return this.m_fontDefaultValue;
            }
        }
            
            
            
        /*
        public Font DefaultValueFont
        {
            get
            {
                return this.Font;
            }
            set
            {
                this.Font = value;
            }
        }*/

		#endregion

        /// <summary>
        /// 解析宏
        /// </summary>
        public event ParseMacroEventHandler ParseMacro = null;

        // public event GetConfigFileEventHandle GetConfigFile = null;

        /// <summary>
        /// 获得配置文件的 XmlDocument 对象
        /// </summary>
        public event GetConfigDomEventHandle GetConfigDom = null;

		public MarcFixedFieldControl()
		{
			// 该调用是 Windows.Forms 窗体设计器所必需的。
			InitializeComponent();

			// TODO: 在 InitComponent 调用后添加任何初始化
		}


		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region 组件设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器 
		/// 修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MarcFixedFieldControl));
            this.imageList_lineState = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // imageList_lineState
            // 
            this.imageList_lineState.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_lineState.ImageStream")));
            this.imageList_lineState.TransparentColor = System.Drawing.Color.White;
            this.imageList_lineState.Images.SetKeyName(0, "macro.bmp");
            this.imageList_lineState.Images.SetKeyName(1, "sensitive.bmp");
            this.imageList_lineState.Images.SetKeyName(2, "macro_and_sensitive.bmp");
            // 
            // MarcFixedFieldControl
            // 
            this.Name = "MarcFixedFieldControl";
            this.Size = new System.Drawing.Size(168, 160);
            this.Enter += new System.EventHandler(this.MarcFixedFieldControl_Enter);
            this.ResumeLayout(false);

		}
		#endregion

		#region 函数

#if NO
		// 拆分一个ref字符串为来源和ValueList的名称
		// parameters:
		//		strRef	ref字符串
		//		strFirst5	当有来源时，来源的前5个字符
		//		strSource	来源
		//		strValueListName	ValueList有名称
		//		strError	出错信息
		// return:
		//		-1	失败
		//		0	成功
		// 注: 来源与ValueList名称之间用#分隔
		public int SplitRef(string strRef,
			out string strFirst5,
			out string strSource,
			out string strValueListName,
			out string strError)
		{
			strError = "";

			strFirst5 = "";
			strSource = "";
			strValueListName = "";
			

			int nIndex = strRef.LastIndexOf('#');
			if (nIndex == -1)
			{
				if (strRef.Length < 5)
				{
					strFirst5 = "";
					strSource = "";
					strValueListName = strRef;
					return 0;
				}
				else
				{
					strFirst5 = strRef.Substring(0,5);
					if (strFirst5 == "http:"
						|| strFirst5 == "file:")
					{
						strSource = strRef;
						strValueListName = "";
						return 0;
					}
					else
					{
						strSource = "";
						strValueListName = strRef;
						return 0;
					}
				}
			}

			strSource = strRef.Substring(0,nIndex);
			strValueListName = strRef.Substring(nIndex + 1);
			if (strSource.Length < 5)
			{
                strError = "来源'" + strSource + "'不合法，前5个字符必须是'http:'或'file:'";
				return -1;					
			}
			else
			{
				strFirst5 = strSource.Substring(0,5);
				if (strFirst5 != "http:"
					&& strFirst5 != "file:")
				{
					strError = "来源'" + strRef + "'不合法，前5个字符必须是'http:'或'file:'";
					return -1;					
				}
			}

			if (strSource == ""
				&& strFirst5 != "")
			{
				strError = "ref来源'" + strRef + "'不合法";
				return -1;
			}
			return 0;
		}
#endif

		// 得到Label的最大宽度
		public void GetMaxWidth(//Graphics g,
            out int nMaxLabelWidth,
            out int nMaxNameWidth)
		{
			nMaxLabelWidth = 0;
            nMaxNameWidth = 0;
			for(int i=0;i<this.templateRoot.Lines.Count;i++)
			{
				TemplateLine line = (TemplateLine)this.templateRoot.Lines[i];

                string strTemp = line.m_strLabel;
                // strTemp = strTemp.Replace(" ", "M");    // +"M";   // 2008/7/16
                int nOneLabelWidth = GraphicsUtil.GetWidth(//g,
                    this.DefaultInfoFont,
                    strTemp /*line.m_strLabel*/);   // +strTemp.Length * 2;

				if (nOneLabelWidth > nMaxLabelWidth)
					nMaxLabelWidth = nOneLabelWidth;

                // 
                int nOneNameWidth = GraphicsUtil.GetWidth(//g,
                    this.DefaultInfoFont,
                    line.m_strName);

                if (nOneNameWidth > nMaxNameWidth)
                    nMaxNameWidth = nOneNameWidth;
			}
		}


		// 初始化控件
		// paramters:
		//		fieldNode	Field节点
		//		strLang	语言版本
		// return:
		//		-1	出错
		//		0	不是定长字段
		//		1	成功
		public int Initial(XmlNode node,
			string strLang,
            out int nResultWidth,
            out int nResultHeight,
			out string strError)
		{
            nResultWidth = 0;
            nResultHeight = 0;
			
			strError = "";

			this.Lang = strLang;

            this.Controls.Clear();

			this.templateRoot = new TemplateRoot(this);
			int nRet = this.templateRoot.Initial(node,
				strLang,
				out strError);
			if (nRet == 0)
				return 0;
			if (nRet == -1)
				return -1;

            for (int i = 0; i < this.templateRoot.Lines.Count; i++)
            {
                TemplateLine line = (TemplateLine)this.templateRoot.Lines[i];

                /*
                string strTempValue = line.m_strValue;
                strTempValue = strTempValue.Replace(' ', 'M');   // ?

                nValueWidth = GraphicsUtil.GetWidth(//g,
                    this.DefaultValueFont,
                    strTempValue);

                if (nMaxValueWidth < nValueWidth)
                    nMaxValueWidth = nValueWidth;

                int nHeight = this.DefaultValueFont.Height;
                 * */

                ValueEditBox textBox_value = new ValueEditBox();
                textBox_value.Font = this.DefaultValueFont;
                textBox_value.Name = "value_" + Convert.ToString(i);
                textBox_value.TabIndex = i;
                textBox_value.Text = line.m_strValue;
                textBox_value.MaxLength = line.m_nValueLength;
                textBox_value.nIndex = i;
                textBox_value.fixedFieldCtrl = this;
                this.Controls.Add(textBox_value);
                line.TextBox_value = textBox_value;
                textBox_value.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                textBox_value.ForeColor = SystemColors.WindowText;
                textBox_value.BackColor = SystemColors.Window;
                textBox_value.TextChanged -= new EventHandler(textBox_value_TextChanged);
                textBox_value.TextChanged += new EventHandler(textBox_value_TextChanged);

                Label label_label = new Label();
                label_label.Font = this.DefaultInfoFont;
                label_label.Name = "label_" + Convert.ToString(i);
                label_label.TabIndex = i;
                label_label.Text = line.m_strLabel;
                label_label.TextAlign = ContentAlignment.MiddleRight;
                // label_label.BorderStyle = BorderStyle.FixedSingle;
                this.Controls.Add(label_label);
                line.Label_label = label_label;

                Label label_name = new Label();
                label_name.Font = this.DefaultInfoFont;
                label_name.Name = "name_" + Convert.ToString(i);
                label_name.TabIndex = i;
                label_name.Text = line.m_strName;
                // label_name.BorderStyle = BorderStyle.Fixed3D;
                label_name.BorderStyle = BorderStyle.None;
                // label_name.BackColor = SystemColors.Window;
                label_name.ForeColor = SystemColors.GrayText;
                this.Controls.Add(label_name);
                line.Label_Name = label_name;

                Label label_state = new Label();
                label_state.Font = this.DefaultInfoFont;
                label_state.Name = "state_" + Convert.ToString(i);
                label_state.TabIndex = i;
                label_state.Text = "";
                label_state.BorderStyle = BorderStyle.None;
                label_state.ImageList = this.imageList_lineState;
                this.Controls.Add(label_state);
                line.Label_state = label_state;
                line.LineState = line.LineState;
            }

            this.listView_values = new ListView();
            this.listView_values.Name = "listView_values";
            this.listView_values.TabIndex = 100;
            this.listView_values.View = View.Details;
            this.listView_values.Columns.Add("值", 50, HorizontalAlignment.Left);
            this.listView_values.Columns.Add("说明", 700, HorizontalAlignment.Left);
            this.listView_values.FullRowSelect = true;
            this.listView_values.HideSelection = false;
            this.AutoScroll = true;
            this.listView_values.DoubleClick += new System.EventHandler(this.ValueList_DoubleClick);
            // this.listView_values.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            this.listView_values.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.listView_values.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.listView_values);

#if NO
            this.SuspendLayout();


			// Graphics g = Graphics.FromHwnd(this.Handle);

			int nJianGeWidth = 8;
			// int nNameWidth = 60;
            int nNameWidth = GraphicsUtil.GetWidth(this.DefaultInfoFont,
					"99/99");

            int nLabelWidth = this.GetMaxLabelWidth(//g,
                this.DefaultInfoFont);  // +8;
			int nValueWidth = 0;

			int nMaxValueWidth = 0;
			int x = 0;
			int y = 0;
			for(int i=0;i<this.templateRoot.Lines.Count;i++)
			{
				TemplateLine line = (TemplateLine)this.templateRoot.Lines[i];

				string strTempValue = line.m_strValue;
				// strTempValue = strTempValue.Replace(' ','m');   // ?
                /*
                string strTemp = "";
                strTempValue = strTemp.PadLeft(strTempValue.Length+1, 'M');   // 2008/7/16
                 * */
                strTempValue = strTempValue.Replace(' ','M');   // ?

				nValueWidth = GraphicsUtil.GetWidth(//g,
					this.DefaultValueFont,
					strTempValue);

				if (nMaxValueWidth < nValueWidth)
					nMaxValueWidth = nValueWidth;

				int nHeight = this.DefaultValueFont.Height;

				ValueEditBox textBox_value = new ValueEditBox();

                // textBox_value.BorderStyle = BorderStyle.None;
                // ((TextBoxBase)textBox_value).AutoSize = false;
				textBox_value.Font = this.DefaultValueFont;
				textBox_value.Location = new System.Drawing.Point(x + nLabelWidth + nJianGeWidth + nNameWidth + nJianGeWidth, 0 + y);
				textBox_value.Name = "value_" + Convert.ToString(i);
				textBox_value.TabIndex = i;
				textBox_value.Text = line.m_strValue;
				textBox_value.MaxLength = line.m_nValueLength;
				textBox_value.nIndex = i;
				textBox_value.fixedFieldCtrl = this;
				this.Controls.Add(textBox_value);
				line.TextBox_value = textBox_value;
				textBox_value.ClientSize = new System.Drawing.Size(nValueWidth, 
                    textBox_value.ClientSize.Height);
                    // nHeight); // changed
                textBox_value.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                textBox_value.ForeColor = SystemColors.WindowText;
                textBox_value.BackColor = SystemColors.Window;
                textBox_value.TextChanged -= new EventHandler(textBox_value_TextChanged);
                textBox_value.TextChanged += new EventHandler(textBox_value_TextChanged);

				nHeight = textBox_value.Size.Height;

				Label label_label = new Label();
				label_label.Font = this.DefaultInfoFont;
				label_label.Location = new System.Drawing.Point(x + 0, 0 + y);
				label_label.Size = new Size(nLabelWidth,nHeight);
				label_label.Name = "label_" + Convert.ToString(i);
				label_label.TabIndex = i;
				label_label.Text = line.m_strLabel;
				label_label.TextAlign = ContentAlignment.MiddleRight;
				// label_label.BorderStyle = BorderStyle.FixedSingle;
				this.Controls.Add(label_label);
				line.Label_label = label_label;

				Label label_name = new Label();
				label_name.Font = this.DefaultInfoFont;
				label_name.Location = new System.Drawing.Point(x + nLabelWidth + nJianGeWidth, 0 + y);
				label_name.Size = new Size(nNameWidth,nHeight);
				label_name.Name = "name_" + Convert.ToString(i);
				label_name.TabIndex = i;
				label_name.Text = line.m_strName;
				// label_name.BorderStyle = BorderStyle.Fixed3D;
                label_name.BorderStyle = BorderStyle.None;
                label_name.BackColor = SystemColors.Window;
                label_name.ForeColor = SystemColors.GrayText;
                this.Controls.Add(label_name);
				line.Label_Name = label_name;

			

				y = y + nHeight;
			}

			int nListViewOrgX = nLabelWidth + nJianGeWidth + nNameWidth + nJianGeWidth + nMaxValueWidth + 10;
			int nListViewOrgY = 0;

			int nListViewWidth = 200;
			int nListViewHeight = Math.Max(y, 10 * this.DefaultValueFont.Height);  // 保证高度不能太小

			this.listView_values = new ListView();
			this.listView_values.Location = new System.Drawing.Point(nListViewOrgX,nListViewOrgY);
			this.listView_values.Name = "listView_values";
			this.listView_values.TabIndex = 100;
			//this.listView_values.Size = new Size(nListViewWidth,nListViewHeight);
            this.listView_values.Size = new Size(nListViewWidth, nListViewHeight);

            this.listView_values.MinimumSize = new Size(nListViewWidth/2, nListViewHeight);
            this.listView_values.MaximumSize = new Size(1024, 768);


			this.listView_values.View = View.Details;
			this.listView_values.Columns.Add("值", 50, HorizontalAlignment.Left);
			this.listView_values.Columns.Add("说明",700, HorizontalAlignment.Left);
			this.listView_values.FullRowSelect = true;
			this.listView_values.HideSelection = false;
            this.AutoScroll = true;
			this.listView_values.DoubleClick += new System.EventHandler(this.ValueList_DoubleClick);
            this.listView_values.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

			this.Controls.Add(this.listView_values);



			int nDocumentWith = nListViewOrgX + nListViewWidth;
            int nDocumentHeight = nListViewHeight;

			//this.Size = new Size(nDocumentWith,
			//	nDocumentHeight);
            nResultWidth = nDocumentWith + 100;
            nResultHeight = nDocumentHeight;   // xietao change

            this.ResumeLayout(false);
            this.PerformLayout();
#endif
            this.nodeTemplateDef = node;    // 保存定义节点

            AdjustTextboxSize(
                true,
             out nResultWidth,
             out nResultHeight);

            return 1;
		}

        internal int m_nDisableTextChanged = 0;

        void textBox_value_TextChanged(object sender, EventArgs e)
        {
            if (this.m_nDisableTextChanged > 0)
                return;

            m_nDisableTextChanged++;
            try
            {
                string strError = "";

                TextBox textbox = (TextBox)sender;

                if (string.IsNullOrEmpty(textbox.Text) == true)
                    return;

                TemplateLine changed_line = null;
                foreach (TemplateLine line in this.templateRoot.Lines)
                {
                    if (line.TextBox_value == textbox)
                    {
                        changed_line = line;
                        goto FOUND;
                    }
                }
                return;
            FOUND:
                Debug.Assert(changed_line != null, "");
                if (changed_line.IsSensitive == true
                    && this.GetTemplateDef != null)
                {
                    // 需要重新装载模板定义
                    GetTemplateDefEventArgs e1 = new GetTemplateDefEventArgs();
                    if (this.nodeTemplateDef != null)
                    {
                        if (this.nodeTemplateDef.Name == "Field")
                        {
                            e1.FieldName = DomUtil.GetAttr(this.nodeTemplateDef, "name");
                        }
                        else if (this.nodeTemplateDef.Name == "Subfield")
                        {
                            e1.FieldName = DomUtil.GetAttr(this.nodeTemplateDef.ParentNode, "name");
                            e1.SubfieldName = DomUtil.GetAttr(this.nodeTemplateDef, "name");
                        }
                        else
                        {
                            strError = "模板定义节点为意外的 <" + this.nodeTemplateDef.Name + "> 元素，无法进行敏感处理";
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        strError = "当前没有模板定义节点信息，无法进行敏感处理";
                        goto ERROR1;
                    }

                    string strValue = this.Value + this.AdditionalValue;
                    e1.Value = strValue;

                    this.GetTemplateDef(this, e1);

                    if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                    {
                        strError = "获得模板定义时出错: " + e1.ErrorInfo;
                        goto ERROR1;
                    }

                    if (e1.Canceled == true)
                        return;

                    if (e1.DefNode == this.nodeTemplateDef)
                        return; // 模板定义没有发生变化

                    int nResultWidth = 0;
                    int nResultHeight = 0;

                    // paramters:
                    //		fieldNode	Field节点
                    //		strLang	语言版本
                    // return:
                    //		-1	出错
                    //		0	不是定长字段
                    //		1	成功
                    int nRet = this.Initial(e1.DefNode,
                this.Lang,
                out nResultWidth,
                out nResultHeight,
                out strError);
                    if (nRet != 1)
                        goto ERROR1;

                    this.Value = strValue;

                    // 通知Form尺寸发生了变化
                    if (this.ResetSize != null)
                    {
                        this.ResetSize(this, new EventArgs());
                    }

                    // 通知标题变化
                    if (this.ResetTitle != null)
                    {
                        ResetTitleEventArgs e2 = new ResetTitleEventArgs();
                        e2.Title = e1.Title;
                        this.ResetTitle(this, e2);
                    }
                }
                return;
            ERROR1:
                MessageBox.Show(this, strError);
            }
            finally
            {
                m_nDisableTextChanged--;
            }
        }

        TemplateLine GetLine(ValueEditBox textbox)
        {
            foreach (TemplateLine line in this.templateRoot.Lines)
            {
                if (line.TextBox_value == textbox)
                {
                    return line;
                }
            }

            return null;
        }

        // 获得缺省值
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetDefaultValue(int index,
            out string strValue,
            out string strError)
        {
            strValue = "";
            strError = "";

            TemplateLine line = (TemplateLine)this.templateRoot.Lines[index];
			if (line == null)
			{
                strError = "未找到序号为'" + Convert.ToString(index) + "'行";
                return -1;
			}

            if (line.DefaultValue == null)
            {
                strError = "缺省值未定义";
                return 0;
            }

            if (this.ParseMacro == null)
            {
                strError = "没有挂接事件";
                return 0;    // 没有挂接事件
            }

            ParseMacroEventArgs e = new ParseMacroEventArgs();
            e.Macro = line.DefaultValue;
            this.ParseMacro(this, e);
            if (String.IsNullOrEmpty(e.ErrorInfo) == false)
            {
                strError = e.ErrorInfo;
                return -1;
            }

            strValue = e.Value;
            return 1;
        }

        public void BeginShowValueList(
            int nCurLine,
            int nCaretPosition,
            int nLineChars,
            string strCurLine)
        {
            this.nCurLine = nCurLine;

            object[] pList = { nCaretPosition, nLineChars, strCurLine };

            this.BeginInvoke(new Delegate_ShowValueList(ShowValueList),
                pList);
        }

        public delegate void Delegate_ShowValueList(
            int nCaretPosition,
            int nLineChars,
            string strCurLine);

        int m_nInShowValueList = 0;

		// 显示值列表
		public void ShowValueList(
            int nCaretPosition,
            int nLineChars,
            string strCurLine)
		{
			this.Update();

			if (this.nCurLine == -1)
				return;

            if (this.m_nInShowValueList > 0)
            {
                // TODO: 是否Post一个消息出去，稍后再作?
                return;
            }


            this.m_nInShowValueList++;
            try
            {
                string strError = "";

                TemplateLine line = (TemplateLine)this.templateRoot.Lines[this.nCurLine];
                if (line == null)
                {
                    strError = "未找到序号为'" + Convert.ToString(this.nCurLine) + "'行";
                    return;
                }

                int nValueChars = 0;    // 值的字符数

                if (line.ValueListNodes == null
                    || line.ValueListNodes.Count == 0)
                {
                    this.listView_values.Items.Clear();
                    return;
                }

                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                if (this.CurValueListNodes != null
                    && this.CurValueListNodes == line.ValueListNodes)
                {
                    if (this.listView_values.Items.Count > 0)
                    {
                        nValueChars = this.listView_values.Items[0].Text.Length;
                        goto SELECT; // 优化
                    }
                }

                this.listView_values.Items.Clear();

                // 解决ref
                while (true)
                {
                    bool bFoundNew = false;
                    for (int i = 0; i < line.ValueListNodes.Count; i++)
                    {
                        XmlNode valuelist_node = line.ValueListNodes[i];
                        string strRef = DomUtil.GetAttr(valuelist_node, "ref");
                        if (string.IsNullOrEmpty(strRef) == true)
                            continue;

                        {
                            if (this.GetConfigDom == null)
                                return;

                            GetConfigDomEventArgs ar = new GetConfigDomEventArgs();
                            ar.Path = strRef;
                            ar.XmlDocument = null;
                            this.GetConfigDom(this, ar);
                            if (string.IsNullOrEmpty(ar.ErrorInfo) == false)
                            {
                                strError = "获取 '" + line.m_strName + "' 对应的ValueList出错，原因:" + ar.ErrorInfo;
                                goto END1;
                            }
                            if (ar.XmlDocument == null)
                                return; // ??

                            string strSource = "";
                            string strValueListName = "";
                            int nIndex = strRef.IndexOf('#');
                            if (nIndex != -1)
                            {
                                strSource = strRef.Substring(0, nIndex);
                                strValueListName = strRef.Substring(nIndex + 1);
                            }
                            else
                            {
                                strValueListName = strRef;
                            }
                            XmlNode valueListNode = ar.XmlDocument.SelectSingleNode("//ValueList[@name='" + strValueListName + "']");
                            if (valueListNode == null)
                            {
                                strError = "未找到路径为'" + strRef + "'的节点。";
                                goto END1;
                            }

                            // 替换
                            line.ValueListNodes[i] = valueListNode;
                            bFoundNew = true;
                        }
                    }

                    if (bFoundNew == false)
                        break;
                }

                this.listView_values.Items.Clear(); // 2006/5/30 add

                foreach (XmlNode valuelist_node in line.ValueListNodes)
                {
                    XmlNodeList itemList = valuelist_node.SelectNodes("Item");
                    foreach (XmlNode itemNode in itemList)
                    {
                        string strItemLable = "";

                        // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode的InnerText
                        // parameters:
                        //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
                        strItemLable = DomUtil.GetXmlLangedNodeText(
                    this.Lang,
                    itemNode,
                    "Label",
                    true);
                        if (string.IsNullOrEmpty(strItemLable) == true)
                            strItemLable = "<尚未定义>";
                        else
                            strItemLable = StringUtil.Trim(strItemLable);

#if NO
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                        nsmgr.AddNamespace("xml", Ns.xml);
                        XmlNode itemLabelNode = itemNode.SelectSingleNode("Label[@xml:lang='" + this.Lang + "']", nsmgr);
                        if (itemLabelNode == null
                            || string.IsNullOrEmpty(itemLabelNode.InnerText.Trim()) == true)
                        {
                            // 如果找不到，则找到第一个有值的
                            XmlNodeList nodes = itemNode.SelectNodes("Label", nsmgr);
                            foreach (XmlNode temp_node in nodes)
                            {
                                if (string.IsNullOrEmpty(temp_node.InnerText.Trim()) == false)
                                {
                                    itemLabelNode = temp_node;
                                    break;
                                }
                            }
                        }

                        if (itemLabelNode == null)
                            strItemLable = "<尚未定义>";
                        else
                            strItemLable = StringUtil.Trim(itemLabelNode.InnerText);
#endif


                        XmlNode itemValueNode = itemNode.SelectSingleNode("Value");
                        string strItemValue = StringUtil.Trim(itemValueNode.InnerText);

                        if (String.IsNullOrEmpty(strItemValue) == false)
                        {
                            nValueChars = strItemValue.Length;
                        }

                        // 加入listview
                        ListViewItem item = new ListViewItem(strItemValue);
                        item.SubItems.Add(strItemLable);
                        this.listView_values.Items.Add(item);
                    }
                }

                this.CurValueListNodes = line.ValueListNodes;

            SELECT:

                // 加亮当前事项
                if (nValueChars != 0)
                {
                    // 算出当前插入符在第几个单元的值上
                    int nIndex = nCaretPosition / nValueChars;
                    if ((nIndex * nValueChars) + nValueChars <= strCurLine.Length)
                    {

                        string strCurUnit = strCurLine.Substring(nIndex * nValueChars, nValueChars);

                        // 在listview中找到这个值, 并选择它。
                        for (int i = 0; i < this.listView_values.Items.Count; i++)
                        {
                            if (this.listView_values.Items[i].Text.Replace("_", " ") == strCurUnit)
                            {
                                this.CurActiveValueUnit = strCurUnit;

                                this.listView_values.Items[i].Selected = true;
                                // 滚入视线内
                                this.listView_values.EnsureVisible(i);
                            }
                            else
                            {
                                if (this.listView_values.Items[i].Selected != false)
                                    this.listView_values.Items[i].Selected = false;
                            }
                        }
                    }

                }

            END1:
                /////////////////////////////////////
                // 触发EndGetValueList事件
                ///////////////////////////////////////
                EndGetValueListEventArgs argsEnd = new EndGetValueListEventArgs();
                if (strError != "")
                    argsEnd.Ref = strError;
                else
                    argsEnd.Ref = "";
                this.fireEndGetValueList(this, argsEnd);
                this.Cursor = oldCursor;
            }
            finally
            {
                this.m_nInShowValueList--;
            }
		}

        public int LineCount
        {
            get
            {
                return this.templateRoot.Lines.Count;
            }
        }

        ValueEditBox m_currentTextBox = null;   // 当前处于焦点的TextBox

        public void ChangeFocusDisplay(ValueEditBox textbox)
        {
            if (this.m_currentTextBox != null)
            {
                this.m_currentTextBox.Font = this.DefaultValueFont;
                this.m_currentTextBox.ForeColor = SystemColors.WindowText;
                this.m_currentTextBox.BackColor = SystemColors.Window;
            }

            this.m_currentTextBox = textbox;
            this.m_currentTextBox.Font = new Font(this.DefaultValueFont, FontStyle.Bold);
            this.m_currentTextBox.BackColor = Color.LightYellow;
            this.m_currentTextBox.ForeColor = Color.DarkRed;
        }

        public ValueEditBox SwitchFocus(int nLineIndex,
            CaretPosition caretposition)
        {
            if (nLineIndex >= templateRoot.Lines.Count)
                return null; // 下标越界

            TemplateLine line = (TemplateLine)templateRoot.Lines[nLineIndex];

            ChangeFocusDisplay(line.TextBox_value);

            line.TextBox_value.Focus();
            // this.ScrollToControl(line.TextBox_value);


            if (caretposition == CaretPosition.FirstChar)
            {
                line.TextBox_value.SelectionStart = 0;
                line.TextBox_value.SelectionLength = 0;
            }

            if (caretposition == CaretPosition.LastChar)
            {
                line.TextBox_value.SelectionStart = line.TextBox_value.MaxLength - 1;
                line.TextBox_value.SelectionLength = 0;
            }

            return line.TextBox_value;
        }


        /*
操作类型 crashReport -- 异常报告 
主题 dp2catalog 
发送者 xxx
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.ArgumentOutOfRangeException
Message: InvalidArgument=“0”的值对于“index”无效。
参数名: index
Stack:
在 System.Windows.Forms.ListView.SelectedListViewItemCollection.get_Item(Int32 index)
在 DigitalPlatform.Marc.MarcFixedFieldControl.ValueList_DoubleClick(Object sender, EventArgs e)
在 System.Windows.Forms.Control.OnDoubleClick(EventArgs e)
在 System.Windows.Forms.ListView.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Catalog 版本: dp2Catalog, Version=2.4.5714.24078, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3 
操作时间 2015/9/17 16:38:54 (Thu, 17 Sep 2015 16:38:54 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 

         * */
        // 当在ValueList列表中双击某项时把值回到对应的地方
		private void ValueList_DoubleClick(object sender, System.EventArgs e)
		{
            string strError = "";

            if (this.listView_values.SelectedItems.Count == 0)
            {
                strError = "尚未选择事项";
                goto ERROR1;
            }

			ListViewItem item = this.listView_values.SelectedItems[0];

            // 配置文件中可以使用 '_' 代替空格
			string strValue = item.Text.Trim().Replace("_", " ");
			//MessageBox.Show(this,strValue);

            if (string.IsNullOrEmpty(strValue) == true)
            {
                strError = "值为空。请定义一个值字符串";
                goto ERROR1;
            }

			TemplateLine line = (TemplateLine)this.templateRoot.Lines[this.nCurLine];
			int nPosition = line.TextBox_value.SelectionStart;

			if (nPosition == line.m_nValueLength)
				nPosition = line.m_nValueLength -1;

			// 得到正确的插入符位置
			if (line.TextBox_value.MaxLength % strValue.Length != 0)
			{
                strError = "值字符数必须是配置值的整倍数";  // 2009/9/21 add
				goto ERROR1;
			}

			string strTempValue = line.m_strValue;
			if (strTempValue.Length < line.m_nValueLength)
				strTempValue = strTempValue + new string(' ',line.m_nValueLength - strTempValue.Length);

			nPosition = (nPosition/strValue.Length)*strValue.Length;

			strTempValue = strTempValue.Remove(nPosition,strValue.Length);
			strTempValue = strTempValue.Insert(nPosition,strValue);

			line.m_strValue = strTempValue;
			line.TextBox_value.Text = strTempValue;
			line.TextBox_value.SelectionStart = nPosition + strValue.Length;

			line.TextBox_value.Focus();
            line.TextBox_value.NextLineIfNeed();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
		}
		
		// 得到字段的值
		public string Value
		{
			get
			{
                if (this.templateRoot != null)
				    return this.templateRoot.GetValue();

                return "";
			}
			set
			{
                if (templateRoot != null)
                {
                    m_nDisableTextChanged++;
                    this.templateRoot.SetValue(value);
                    m_nDisableTextChanged--;
                }
			}
		}

        // 多处来的字符串部分
        public string AdditionalValue
        {
            get
            {
                if (this.templateRoot != null)
                    return this.templateRoot.AdditionalValue;

                return "";
            }
            set
            {
                if (templateRoot != null)
                    this.templateRoot.AdditionalValue = value;
            }
        }

		#endregion

		#region 事件

		public event BeginGetValueListEventHandle BeginGetValueList;
		public void fireBeginGetValueList(object sender,
			BeginGetValueListEventArgs args)
		{
			if (BeginGetValueList != null)
			{
				this.BeginGetValueList(sender,args);
			}
		}

		public event EndGetValueListEventHandle EndGetValueList;
		public void fireEndGetValueList(object sender,
			EndGetValueListEventArgs args)
		{
			if (EndGetValueList != null)
			{
				this.EndGetValueList(sender,args);
			}
		}

		#endregion

        private void MarcFixedFieldControl_Enter(object sender, EventArgs e)
        {
            if (this.templateRoot.Lines.Count > 0)
            {

                TemplateLine line = null;
                if (this.m_currentTextBox != null)
                    line = GetLine(this.m_currentTextBox);
                else 
                    line = (TemplateLine)this.templateRoot.Lines[0];

                if (line == null)
                    return;

                ChangeFocusDisplay(line.TextBox_value);
                line.TextBox_value.Focus();
            }
        }

        public void FocusFirstLine()
        {
            // 如果有闲暇，可以编为第一次就针对性焦点在某个行上
            if (this.templateRoot.Lines.Count > 0)
            {
                TemplateLine line = (TemplateLine)this.templateRoot.Lines[0];
                line.TextBox_value.SelectionStart = 0;
                line.TextBox_value.SelectionLength = 0;

                ChangeFocusDisplay(line.TextBox_value);
                line.TextBox_value.Focus();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            AdjuetListViewSize();
        }

        public void AdjuetListViewSize()
        {
            if (listView_values != null)
            {
                int nWidth = this.Width - this.listView_values.Location.X;
                this.listView_values.Size = new Size(nWidth, this.Size.Height);
            }
        }

        public void AdjustTextboxSize(
            bool bSimulate,
            out int nResultWidth,
            out int nResultHeight
            )
        {
            this.SuspendLayout();

            int nLabelWidth = 0;
            int nNameWidth = 0;
            GetMaxWidth(//Graphics g,
  out nLabelWidth,
  out nNameWidth);

            int y = this.Padding.Top;
            int nBlankWidth = 8;
            int nLineSep = 4;
            int nStateWidth = 8;

            int nMaxValueWidth = 0;
            for (int i = 0; i < this.templateRoot.Lines.Count; i++)
            {
                if (i != 0)
                    y += nLineSep;

                TemplateLine line = (TemplateLine)this.templateRoot.Lines[i];

                string strTempValue = line.m_strValue;
                // strTempValue = strTempValue.Replace(' ','m');   // ?
                /*
                string strTemp = "";
                strTempValue = strTemp.PadLeft(strTempValue.Length+1, 'M');   // 2008/7/16
                 * */
                strTempValue = strTempValue.Replace(' ', 'M');   // ?

                int nValueWidth = GraphicsUtil.GetWidth(//g,
                    this.DefaultValueFont,
                    strTempValue);

                if (nMaxValueWidth < nValueWidth)
                    nMaxValueWidth = nValueWidth + line.TextBox_value.Size.Width - line.TextBox_value.ClientSize.Width;

                if (bSimulate == false)
                {
                    line.TextBox_value.ClientSize = new Size(nValueWidth, line.TextBox_value.ClientSize.Height);
                    line.TextBox_value.Location = new System.Drawing.Point(
                        this.Padding.Left + nLabelWidth + nBlankWidth + nNameWidth + nStateWidth,
                        y);
                }

                int nHeight = line.TextBox_value.Size.Height;

                if (bSimulate == false)
                {
                    // line.Label_label.AutoSize = true;
                    line.Label_label.Size = new Size(nLabelWidth, nHeight);
                    line.Label_label.Location = new System.Drawing.Point(this.Padding.Left, y);

                    line.Label_Name.AutoSize = true;
                    // line.Label_Name.Size = new Size(nNameWidth, nHeight);
                    line.Label_Name.Location = new System.Drawing.Point(
                        this.Padding.Left + nLabelWidth + nBlankWidth,
                        y);

                    line.Label_state.Size = new Size(nStateWidth, nHeight);
                    line.Label_state.Location = new System.Drawing.Point(
                        this.Padding.Left + nLabelWidth + nBlankWidth + nNameWidth,
                        y);

                }

                y = y + nHeight;
            }

            int nListViewOrgX = this.Padding.Left + nLabelWidth + nBlankWidth + nNameWidth + nStateWidth + nMaxValueWidth + 4 + this.Padding.Right;
            int nListViewOrgY = 0;

            int nListViewWidth = Math.Min(this.Width - nListViewOrgX - 10, 200);
            int nListViewHeight = Math.Max(y+this.Padding.Bottom, 10 * this.DefaultValueFont.Height);  // 保证高度不能太小

            if (bSimulate == false)
            {
                this.listView_values.Location = new System.Drawing.Point(
                    nListViewOrgX,
                    nListViewOrgY);
                this.listView_values.Size = new Size(nListViewWidth, nListViewHeight);
                this.listView_values.MinimumSize = new Size(50, nListViewHeight);
                this.listView_values.MaximumSize = new Size(1024, 768);
            }

            int nDocumentWith = nListViewOrgX + nListViewWidth;
            int nDocumentHeight = nListViewHeight;

            nResultWidth = nDocumentWith + 100;
            nResultHeight = nDocumentHeight;


            // TODO: 修改label的宽度?
            if (bSimulate == false)
            {
                this.ResumeLayout(false);
                this.PerformLayout();
            }
        }

        protected override void OnFontChanged(
EventArgs e)
        {
            base.OnFontChanged(e);

            // 迫使采用最新的字体
            this.m_fontDefaultInfo = null;
            this.m_fontDefaultValue = null;

            // Font textbox_font = new Font("Courier New", this.Font.SizeInPoints, GraphicsUnit.Point);
            for (int i = 0; i < this.templateRoot.Lines.Count; i++)
            {
                TemplateLine line = (TemplateLine)this.templateRoot.Lines[i];

                string strTempValue = line.m_strValue;

                line.TextBox_value.Font = this.DefaultValueFont;
                line.Label_label.Font = this.DefaultInfoFont;
                line.Label_Name.Font = this.DefaultInfoFont;
            }
        }
	}


	public delegate void BeginGetValueListEventHandle(object sender,
	BeginGetValueListEventArgs e);
	public class BeginGetValueListEventArgs: EventArgs
	{
		public string Ref = "";
	}

	public delegate void EndGetValueListEventHandle(object sender,
	EndGetValueListEventArgs e);
	public class EndGetValueListEventArgs: EventArgs
	{
		public string Ref = "";
	}

    //
    public delegate void GetConfigFileEventHandle(object sender,
GetConfigFileEventArgs e);

    public class GetConfigFileEventArgs : EventArgs
    {
        public string Path = "";
        // public bool Found = false;

        public Stream Stream = null;

        public string ErrorInfo = "";
    }

    //
    public delegate void GetConfigDomEventHandle(object sender,
GetConfigDomEventArgs e);

    public class GetConfigDomEventArgs : EventArgs
    {
        public string Path = "";    // [in]

        public XmlDocument XmlDocument = null;  // [out]

        public string ErrorInfo = "";   // [out]
    }


    // 获得宏的实际值
    public delegate void ParseMacroEventHandler(object sender,
        ParseMacroEventArgs e);

    public class ParseMacroEventArgs : EventArgs
    {
        public string Macro = "";   // 宏
        public bool Simulate = false;   // 是否为模拟方式? 在模拟方式下, 种子号增量将变为获得种子号来执行,也就是不会改变种子值
        public string Value = "";   // [out]兑现后的值
        public string ErrorInfo = "";   // [out]出错信息
    }


    public enum CaretPosition
    {
        None = 0,
        FirstChar = 1,
        LastChar = 2,
    }

    /// <summary>
    /// 获取一个特定模板的 XML 定义的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetTemplateDefEventHandler(object sender,
GetTemplateDefEventArgs e);

    /// <summary>
    /// 获取一个特定模板的 XML 定义的事件的参数
    /// </summary>
    public class GetTemplateDefEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 字段名
        /// </summary>
        public string FieldName = "";           // [in]
        /// <summary>
        /// [in] 子字段名
        /// </summary>
        public string SubfieldName = "";        // [in]
        /// <summary>
        /// [in] 用于辅助判断的字符串。一般是模板中当前的全部内容
        /// </summary>
        public string Value = "";   // [in]用于辅助判断的字符串。一般是模板中当前的全部内容

        /// <summary>
        /// [out] 返回错误信息
        /// </summary>
        public string ErrorInfo = "";   // [out]错误信息
        /// <summary>
        /// [out] 事件中是否放弃了对该字段/子字段模板的处理
        /// </summary>
        public bool Canceled = false;   // [out]事件中是否放弃了对该字段/子字段模板的处理。

        /// <summary>
        /// [out] 定义节点
        /// </summary>
        public XmlNode DefNode = null;   // [out]定义节点
        /// <summary>
        /// [out] 模板窗口标题。一般是文献类型之类内容
        /// </summary>
        public string Title = "";       // [out]模板窗口标题。一般是文献类型之类内容
    }

    // 
    public delegate void ResetTitleEventHandler(object sender,
ResetTitleEventArgs e);

    public class ResetTitleEventArgs : EventArgs
    {
        public string Title = "";
    }
}
