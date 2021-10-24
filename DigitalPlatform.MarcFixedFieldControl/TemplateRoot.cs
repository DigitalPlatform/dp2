using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.Xml;

namespace DigitalPlatform.Marc
{

    // 模板根对象
    public class TemplateRoot
    {
        public MarcFixedFieldControl control = null;

        private XmlNode m_fieldNode = null;
        private string m_strLang = null;

        private string m_strName = null;
        private string m_strLabel = null;

        public List<TemplateLine> Lines = new List<TemplateLine>();   // 原来是 ArrayList

        public TemplateRoot(MarcFixedFieldControl ctrl)
        {
            this.control = ctrl;
        }

        /*
            <Field name='###' length='24' mandatory='yes' repeatable='no'>
                <Property>
                    <Label xml:lang='en'>RECORD IDENTIFIER</Label>
                    <Label xml:lang='cn'>头标区</Label>
                    <Help xml:lang='cn'>帮助信息</Help>
                </Property>
                <Char name='0/5'>
                </Char>
                ....
            </Field>
        */
        // 初始化TemplateRoot对象
        // parameters:
        //		fieldNode	Field节点
        //		strLang	语言版本
        //		strError	出错信息
        // return:
        //		-1	失败
        //		0	不是定长字段
        //		1	成功
        public int Initial(XmlNode node,
            string strLang,
            out string strError)
        {
            strError = "";

            Debug.Assert(node != null, "调用错误，node不能为null");

            this.m_fieldNode = node;
            this.m_strLang = strLang;


            this.m_strName = DomUtil.GetAttr(node, "name");
            if (this.m_strName == "")
            {
                strError = "<" + node.Name + ">元素的name属性可能不存在或者值为空，配置文件不合法。";
                Debug.Assert(false, strError);
                return -1;
            }

            XmlNode propertyNode = node.SelectSingleNode("Property");
            if (propertyNode == null)
            {
                // TODO：　需要显示更详细的信息
                XmlNode temp = node.Clone();
                while (temp.ChildNodes.Count > 0)
                    temp.RemoveChild(temp.ChildNodes[0]);
                strError = "在 " + temp.OuterXml + " 元素下级未定义<Property>元素,配置文件不合法.";
                // strError = "在<" + node.Name + ">元素下级未定义<Property>元素,配置文件不合法.";
                // Debug.Assert(false,strError);
                return -1;
            }

            XmlNodeList charList = node.SelectNodes("Char");
            // 没有<Char>元素，不是字长字段或子字段
            if (charList.Count == 0)
                return 0;
#if NO
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
			nsmgr.AddNamespace("xml", Ns.xml);
			XmlNode labelNode = propertyNode.SelectSingleNode("Label[@xml:lang='" + strLang + "']",nsmgr);
			if (labelNode == null)
			{
				this.m_strLabel = "????????";
				Debug.Assert(false,"名称为'" + this.m_strName + "'的<" + node.Name +">元素未定义Label的'" + strLang + "'语言版本的值");
			}
			this.m_strLabel = DomUtil.GetNodeText(labelNode);

#endif
            // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode的InnerText
            // parameters:
            //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
            this.m_strLabel = DomUtil.GetXmlLangedNodeText(
        strLang,
        propertyNode,
        "Label",
        true);

            if (this.Lines == null)
                this.Lines = new List<TemplateLine>();
            else
                this.Lines.Clear();
            foreach (XmlNode charNode in charList)
            {
                TemplateLine line = new TemplateLine(this,
                    charNode,
                    strLang);
                this.Lines.Add(line);
            }

            return 1;
        }


        public string GetValue()
        {
            string strValue = "";

            this.Lines.Sort();

            for (int i = 0; i < this.Lines.Count; i++)
            {
                TemplateLine line = this.Lines[i];
                strValue += line.TextBox_value.Text;
            }
            return strValue;
        }

        public string AdditionalValue = "";

        public int SetValue(string strValue)
        {

            int nTotalLength = 0;
            for (int i = 0; i < this.Lines.Count; i++)
            {
                nTotalLength += (this.Lines[i]).m_nValueLength;
            }

            if (strValue == null)
                strValue = "";

            // 补齐字符
            if (strValue.Length < nTotalLength)
                strValue = strValue + new string(' ', nTotalLength - strValue.Length);

            // 多余的字符
            if (strValue.Length > nTotalLength)
                AdditionalValue = strValue.Substring(nTotalLength);
            else
                AdditionalValue = "";

            for (int i = 0; i < this.Lines.Count; i++)
            {
                TemplateLine line = this.Lines[i];
                line.m_strValue = strValue.Substring(line.m_nStart, line.m_nValueLength);
                line.TextBox_value.Text = line.m_strValue;
            }

            return 0;
        }

    }
}
