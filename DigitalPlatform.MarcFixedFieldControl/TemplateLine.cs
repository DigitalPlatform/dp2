using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;
using System.Windows.Forms;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.Marc
{
    // 模板行
	public class TemplateLine : IComparable
	{
		public TemplateRoot container = null;

        public bool IsSensitive = false;
        internal LineState m_lineState = LineState.None;

		private XmlNode m_charNode = null;
		private string m_strLang = null;

		internal string m_strLabel = null;
		internal string m_strName = null;
		internal string m_strValue = null;

        // public XmlNode ValueListNode1 = null;   // 由TemplateLine.Initial()初始化
        public List<XmlNode> ValueListNodes = null;   // 由TemplateLine.Initial()初始化

		public Label Label_label = null;
		public Label Label_Name = null;
        public Label Label_state = null;
        public ValueEditBox TextBox_value = null;   // changed 2006/5/15

		internal int m_nValueLength = 0;
		internal int m_nStart = 0;

        public string DefaultValue = null;

		// parameter:
		//		node	char节点
		//		strLang	语言版本
		public TemplateLine(TemplateRoot templateRoot,
			XmlNode node,
			string strLang)
		{
			this.container = templateRoot;
			this.m_charNode = node;
			this.m_strLang = strLang;

			string strError;
			// 通过一个Char节点，初始化本行的值
			// parameter:
			//		node	char节点
			//		strLang	语言版本
			//		strError	出错信息
			// return:
			//		-1	失败
			//		0	成功
			int nRet = this.Initial(this.m_charNode,
				this.m_strLang,
				out strError);
			if (nRet == -1)
				throw new Exception(strError);
		}

        static string Trim(string s)
        {
            if (string.IsNullOrEmpty(s) == true)
                return s;
            return s.Trim();
        }

        public LineState LineState
        {
            get
            {
                return this.m_lineState;
            }
            set
            {
                this.m_lineState = value;

                if (this.Label_state != null)
                {
                    if (value == Marc.LineState.Macro)
                        this.Label_state.ImageIndex = 0;
                    else if (value == Marc.LineState.Sensitive)
                        this.Label_state.ImageIndex = 1;
                    else if (value == (Marc.LineState.Macro | Marc.LineState.Sensitive))
                        this.Label_state.ImageIndex = 2;
                    else
                        this.Label_state.ImageIndex = -1;
                }
            }
        }


/*
		<Char name='0/5'>
			<Property>
				<Label xml:lang='en'>?</Label>
				<Label xml:lang='cn'>记录长度</Label>
				<Help xml:lang='cn'></Help>
				<ValueList name='header_0/5'>
					<Item>
						<Value>?????</Value>
						<Label xml:lang='cn'>由软件自动填写</Label>
					</Item>
				</ValueList>
			</Property>
		</Char>
*/
		// 通过一个Char节点，初始化本行的值
		// parameter:
		//		node	char节点
		//		strLang	语言版本
		//		strError	出错信息
		// return:
		//		-1	失败
		//		0	成功
		public int Initial(XmlNode node,
			string strLang,
			out string strError)
		{
			strError = "";

			if (node == null)
			{
				strError = "调用错误，node参数不能为null";
				Debug.Assert(false,strError);
				return -1;
			}

			this.m_strName = Trim(DomUtil.GetAttr(node,"name"));
			if (this.m_strName == "")
			{
				strError = "<Char>元素的name属性可能不存在或者值为空，配置文件不合法。";
				Debug.Assert(false,strError);
				return -1;					
			}

			XmlNode propertyNode = node.SelectSingleNode("Property");
			if (propertyNode == null)
			{
				strError = "<Char>元素下级未定义<Property>元素，配置文件不合法";
				Debug.Assert(false,strError);
				return -1;
			}

            // <Property>/<sensitive>
            if (propertyNode.SelectSingleNode("sensitive") != null)
            {
                this.IsSensitive = true;
                this.m_lineState |= LineState.Sensitive;
            }
            else
                this.IsSensitive = false;

            // <Property>/<DefaultValue>
            if (propertyNode.SelectSingleNode("DefaultValue") != null)
                this.m_lineState |= LineState.Macro;

            // <Property>/<DefaultValue>
            XmlNode nodeDefaultValue = propertyNode.SelectSingleNode("DefaultValue");
            if (nodeDefaultValue != null)
                this.DefaultValue = nodeDefaultValue.InnerText;

            // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode的InnerText
            // parameters:
            //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
            this.m_strLabel = DomUtil.GetXmlLangedNodeText(
        strLang,
        propertyNode,
        "Label",
        true);
            if (string.IsNullOrEmpty(this.m_strLabel) == true)
                this.m_strLabel = "<尚未定义>";
            else
                this.m_strLabel = StringUtil.Trim(this.m_strLabel);
#if NO
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
			nsmgr.AddNamespace("xml", Ns.xml);
			XmlNode labelNode = propertyNode.SelectSingleNode("Label[@xml:lang='" + strLang + "']",nsmgr);
			if (labelNode == null
                || string.IsNullOrEmpty(labelNode.InnerText.Trim()) == true)
			{
                // 如果找不到，则找到第一个有值的
                XmlNodeList nodes = propertyNode.SelectNodes("Label", nsmgr);
                foreach (XmlNode temp_node in nodes)
                {
                    if (string.IsNullOrEmpty(temp_node.InnerText.Trim()) == false)
                    {
                        labelNode = temp_node;
                        break;
                    }
                }

				//Debug.Assert(false,"名称为'" + this.m_strName + "'的<char>元素未定义Label的'" + strLang + "'语言版本的值");
			}
            if (labelNode == null)
                this.m_strLabel = "<尚未定义>";
            else
                this.m_strLabel = Trim(DomUtil.GetNodeText(labelNode));
#endif

			// 给value赋初值
			int nIndex = this.m_strName.IndexOf("/");
			if (nIndex >= 0)
			{
				string strLetterCount = this.m_strName.Substring(nIndex+1);
				this.m_nValueLength = Convert.ToInt32(strLetterCount);
				this.m_nStart = Convert.ToInt32(this.m_strName.Substring(0,nIndex));
			}
			if (this.m_strValue == null)
				this.m_strValue = new string('*',this.m_nValueLength);


            XmlNodeList valuelist_nodes = propertyNode.SelectNodes("ValueList");
            this.ValueListNodes = new List<XmlNode>();
            foreach (XmlNode valuelist_node in valuelist_nodes)
            {
                this.ValueListNodes.Add(valuelist_node);
            }

            return 0;

		}


		// 比较
		public int CompareTo(object obj)
		{
			TemplateLine line = (TemplateLine)obj;

			return this.m_nStart - line.m_nStart;
		}
	}

    // 值对象
    public class ValueItem
    {
        public string Lable = null;
        public string Value = null;

        public ValueItem(string strLable,
            string strValue)
        {
            this.Lable = strLable;
            this.Value = strValue;
        }
    }

    // 行的状态
    [Flags]
    public enum LineState
    {
        None = 0,
        Macro = 0x01,
        Sensitive = 0x02,
    }
}
