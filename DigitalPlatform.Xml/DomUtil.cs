using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DigitalPlatform.Xml
{
    // DomUtil类包含XML DOM的一些扩展功能函数
    public class DomUtil
    {
        public static XmlNode RenameNode(XmlNode node,
            string namespaceURI,
            string qualifiedName)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                XmlElement oldElement = (XmlElement)node;
                XmlElement newElement =
                node.OwnerDocument.CreateElement(qualifiedName, namespaceURI);

                while (oldElement.HasAttributes)
                {
                    newElement.SetAttributeNode(oldElement.RemoveAttributeNode(oldElement.Attributes[0]));
                }

                while (oldElement.HasChildNodes)
                {
                    newElement.AppendChild(oldElement.FirstChild);
                }

                if (oldElement.ParentNode != null)
                {
                    oldElement.ParentNode.ReplaceChild(newElement, oldElement);
                }

                return newElement;
            }
            else
            {
                return null;
            }
        }


        // 2010/12/18
        // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode的InnerText
        // parameters:
        //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
        public static string GetLangedNodeText(
            string strLang,
            XmlNode parent,
            string strElementName,
            bool bReturnFirstNode = true)
        {
            XmlNode node = GetLangedNode(
        strLang,
        parent,
        strElementName,
        bReturnFirstNode);
            if (node == null)
                return null;
            return node.InnerText;
        }

        // 2010/12/18
        // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode
        // parameters:
        //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
        public static XmlNode GetLangedNode(
            string strLang,
            XmlNode parent,
            string strElementName,
            bool bReturnFirstNode = true)
        {
            XmlNode node = null;

            if (String.IsNullOrEmpty(strLang) == true)
            {
                return parent.SelectSingleNode(strElementName);  // 第一个strElementName元素
            }
            else
            {
                node = parent.SelectSingleNode(strElementName + "[@lang='" + strLang + "']");

                if (node != null)
                    return node;
            }

            string strLangLeft = "";
            string strLangRight = "";

            SplitLang(strLang,
               out strLangLeft,
               out strLangRight);

            // 所有<caption>元素
            XmlNodeList nodes = parent.SelectNodes(strElementName);

            for (int i = 0; i < nodes.Count; i++)
            {
                string strThisLang = DomUtil.GetAttr(nodes[i], "lang");

                string strThisLangLeft = "";
                string strThisLangRight = "";

                SplitLang(strThisLang,
                   out strThisLangLeft,
                   out strThisLangRight);

                // 是不是左右都匹配则更好?如果不行才是第一个左边匹配的

                if (strThisLangLeft == strLangLeft)
                    return nodes[i];
            }

            if (bReturnFirstNode == true)
            {
                // 实在不行，则选第一个<caption>的文字值
                node = parent.SelectSingleNode(strElementName);
                if (node != null)
                    return node;
            }

            return null;    // not found
        }

        // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode的InnerText
        // parameters:
        //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
        public static string GetXmlLangedNodeText(
            string strLang,
            XmlNode parent,
            string strElementName,
            bool bReturnFirstNode = true)
        {
            XmlNode node = GetXmlLangedNode(
        strLang,
        parent,
        strElementName,
        bReturnFirstNode);
            if (node == null)
                return null;
            return node.InnerText;
        }

        // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode
        // parameters:
        //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
        public static XmlNode GetXmlLangedNode(
            string strLang,
            XmlNode parent,
            string strElementName,
            bool bReturnFirstNode = true)
        {
            XmlNode node = null;

            if (String.IsNullOrEmpty(strLang) == true)
            {
                return parent.SelectSingleNode(strElementName);  // 第一个strElementName元素
            }
            else
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("xml", Ns.xml);

                node = parent.SelectSingleNode(strElementName + "[@xml:lang='" + strLang + "']", nsmgr);

                if (node != null)
                    return node;
            }

            string strLangLeft = "";
            string strLangRight = "";

            SplitLang(strLang,
               out strLangLeft,
               out strLangRight);

            // 所有<strElementName>元素
            XmlNodeList nodes = parent.SelectNodes(strElementName);

            for (int i = 0; i < nodes.Count; i++)
            {
                string strThisLang = DomUtil.GetAttr(Ns.xml, nodes[i], "lang");

                string strThisLangLeft = "";
                string strThisLangRight = "";

                SplitLang(strThisLang,
                   out strThisLangLeft,
                   out strThisLangRight);

                // 是不是左右都匹配则更好?如果不行才是第一个左边匹配的

                if (strThisLangLeft == strLangLeft)
                    return nodes[i];
            }

            if (bReturnFirstNode == true)
            {
                // 实在不行，则选第一个<strElementName>的文字值
                node = parent.SelectSingleNode(strElementName);
                if (node != null)
                    return node;
            }

            return null;    // not found
        }

        // 从一个元素的下级<caption>元素中, 提取语言符合的文字值
        // 和GetCaption()函数的差异，在于如果找不到相关语言的，不做返回第一个<caption>
        public static string GetCaptionExt(string strLang,
            XmlNode parent)
        {
            XmlNode node = null;

            if (String.IsNullOrEmpty(strLang) == true)
            {
                node = parent.SelectSingleNode("caption");  // 第一个caption元素
                if (node != null)
                    return node.InnerText;

                return null;
            }
            else
            {
                node = parent.SelectSingleNode("caption[@lang='" + strLang + "']");

                if (node != null)
                    return node.InnerText;
            }

            string strLangLeft = "";
            string strLangRight = "";

            SplitLang(strLang,
               out strLangLeft,
               out strLangRight);

            // 所有<caption>元素
            XmlNodeList nodes = parent.SelectNodes("caption");

            for (int i = 0; i < nodes.Count; i++)
            {
                string strThisLang = DomUtil.GetAttr(nodes[i], "lang");

                string strThisLangLeft = "";
                string strThisLangRight = "";

                SplitLang(strThisLang,
                   out strThisLangLeft,
                   out strThisLangRight);

                // 是不是左右都匹配则更好?如果不行才是第一个左边匹配的

                if (strThisLangLeft == strLangLeft)
                    return nodes[i].InnerText;
            }

            /*
            // 实在不行，则选第一个<caption>的文字值
            node = parent.SelectSingleNode("caption");
            if (node != null)
                return node.InnerText;
             * */

            return null;    // not found
        }

        // 从一个元素的下级<caption>元素中, 提取语言符合的文字值
        public static string GetCaption(string strLang,
            XmlNode parent)
        {
            XmlNode node = null;

            if (String.IsNullOrEmpty(strLang) == true)
            {
                node = parent.SelectSingleNode("caption");  // 第一个caption元素
                if (node != null)
                    return node.InnerText;

                return null;
            }
            else
            {
                node = parent.SelectSingleNode("caption[@lang='" + strLang + "']");

                if (node != null)
                    return node.InnerText;
            }

            string strLangLeft = "";
            string strLangRight = "";

            SplitLang(strLang,
               out strLangLeft,
               out strLangRight);

            // 所有<caption>元素
            XmlNodeList nodes = parent.SelectNodes("caption");

            for (int i = 0; i < nodes.Count; i++)
            {
                string strThisLang = DomUtil.GetAttr(nodes[i], "lang");

                string strThisLangLeft = "";
                string strThisLangRight = "";

                SplitLang(strThisLang,
                   out strThisLangLeft,
                   out strThisLangRight);

                // 是不是左右都匹配则更好?如果不行才是第一个左边匹配的

                if (strThisLangLeft == strLangLeft)
                    return nodes[i].InnerText;
            }

            // 实在不行，则选第一个<caption>的文字值
            node = parent.SelectSingleNode("caption");
            if (node != null)
                return node.InnerText;

            return null;    // not found
        }

        public static void SplitLang(string strLang,
    out string strLangLeft,
    out string strLangRight)
        {
            strLangLeft = "";
            strLangRight = "";

            int nRet = strLang.IndexOf("-");
            if (nRet == -1)
                strLangLeft = strLang;
            else
            {
                strLangLeft = strLang.Substring(0, nRet);
                strLangRight = strLang.Substring(nRet + 1);
            }
        }

        // 把表示布尔值的字符串翻译为布尔值
        // 注意，strValue不能为空，本函数无法解释缺省值
        public static bool IsBooleanTrue(string strValue)
        {
            // 2008/6/4
            if (String.IsNullOrEmpty(strValue) == true)
                throw new Exception("DomUtil.IsBoolean() 不能接受空字符串参数");

            strValue = strValue.ToLower();  // 2008/6/4

            if (strValue == "yes" || strValue == "on"
                    || strValue == "1" || strValue == "true")
                return true;

            return false;
        }

        public static bool IsBooleanTrue(string strValue, bool bDefaultValue)
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return bDefaultValue;
            return IsBooleanTrue(strValue);
        }

        // 包装版本
        public static bool GetBooleanParam(XmlNode node,
            string strParamName,
            bool bDefaultValue)
        {
            bool bValue = bDefaultValue;
            string strError = "";
            GetBooleanParam(node,
                strParamName,
                bDefaultValue,
                out bValue,
                out strError);
            return bValue;
        }

        // 包装后的版本。不用事先获得元素的 Node
        public static bool GetBooleanParam(
            XmlNode root,
            string strElementPath,
            string strParamName,
            bool bDefaultValue)
        {
            XmlNode node = root.SelectSingleNode(strElementPath);
            if (node == null)
                return bDefaultValue;
            return GetBooleanParam(node,
                strParamName,
                bDefaultValue);
        }

        // 设置 bool 类型的参数
        // parameters:
        //      root    起点 XmlNode
        //      strElementPath  元素路径
        //      strParamName    属性名
        //      bValue  要设置的值
        // return:
        //      本次是否创建了新的元素
        public static bool SetBooleanParam(
            XmlNode root,
            string strElementPath,
            string strParamName,
            bool bValue)
        {
            bool bCreateElement = false;
            XmlNode node = root.SelectSingleNode(strElementPath);
            if (node == null)
            {
                string[] aNodeName = strElementPath.Split(new Char[] { '/' });
                node = CreateNode(root, aNodeName);
                bCreateElement = true;
            }
            if (node == null)
            {
                throw (new Exception("SetBooleanParam() CreateNode error"));
            }

            SetAttr(node, strParamName, bValue == true ? "true" : "false");
            return bCreateElement;
        }

        // 获得布尔型的属性参数值
        // return:
        //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
        //      0   正常获得明确定义的参数值
        //      1   参数没有定义，因此代替以缺省参数值返回
        public static int GetBooleanParam(XmlNode node,
            string strParamName,
            bool bDefaultValue,
            out bool bValue,
            out string strError)
        {
            strError = "";
            bValue = bDefaultValue;

            string strValue = DomUtil.GetAttr(node, strParamName);

            strValue = strValue.Trim();

            if (String.IsNullOrEmpty(strValue) == true)
            {
                bValue = bDefaultValue;
                return 1;
            }

            strValue = strValue.ToLower();

            if (strValue == "yes" || strValue == "on"
                || strValue == "1" || strValue == "true")
            {
                bValue = true;
                return 0;
            }

            // TODO: 可以检查字符串，要在规定的值范围内

            bValue = false;
            return 0;
        }



        // 获得整数型的属性参数值
        // return:
        //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
        //      0   正常获得明确定义的参数值
        //      1   参数没有定义，因此代替以缺省参数值返回
        public static int GetIntegerParam(XmlNode node,
            string strParamName,
            int nDefaultValue,
            out int nValue,
            out string strError)
        {
            strError = "";
            nValue = nDefaultValue;

            string strValue = DomUtil.GetAttr(node, strParamName);


            if (String.IsNullOrEmpty(strValue) == true)
            {
                nValue = nDefaultValue;
                return 1;
            }

            try
            {
                nValue = Convert.ToInt32(strValue);
            }
            catch (Exception ex)
            {
                strError = "属性 " + strParamName + " 的值应当为数值型。出错信息: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // 获得整数型的属性参数值
        // return:
        //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
        //      0   正常获得明确定义的参数值
        //      1   参数没有定义，因此代替以缺省参数值返回
        public static int GetIntegerParam(XmlNode node,
            string strParamName,
            long nDefaultValue,
            out long nValue,
            out string strError)
        {
            strError = "";
            nValue = nDefaultValue;

            string strValue = DomUtil.GetAttr(node, strParamName);


            if (String.IsNullOrEmpty(strValue) == true)
            {
                nValue = nDefaultValue;
                return 1;
            }

            try
            {
                nValue = Convert.ToInt64(strValue);
            }
            catch (Exception ex)
            {
                strError = "属性 " + strParamName + " 的值应当为数值型。出错信息: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // 获得浮点数型的属性参数值
        // return:
        //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
        //      0   正常获得明确定义的参数值
        //      1   参数没有定义，因此代替以缺省参数值返回
        public static int GetDoubleParam(XmlNode node,
            string strParamName,
            double nDefaultValue,
            out double nValue,
            out string strError)
        {
            strError = "";
            nValue = nDefaultValue;

            string strValue = DomUtil.GetAttr(node, strParamName);


            if (String.IsNullOrEmpty(strValue) == true)
            {
                nValue = nDefaultValue;
                return 1;
            }

            try
            {
                nValue = Convert.ToDouble(strValue);
            }
            catch (Exception ex)
            {
                strError = "属性 " + strParamName + " 的值应当为(浮点)数值型。出错信息: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // 包装后的版本
        // 不包含prolog
        public static int GetIndentXml(string strXml,
            out string strOutXml,
            out string strError)
        {
            return GetIndentXml(strXml,
                false,
                out strOutXml,
                out strError);
        }

        public static string GetIndentXml(string strXml)
        {
            string strOutXml = "";
            string strError = "";
            int nRet = GetIndentXml(strXml,
    false,
    out strOutXml,
    out strError);
            if (nRet == -1)
                return strError;
            return strOutXml;
        }

        // parameters:
        //      bHasProlog  是否prolog
        public static int GetIndentXml(string strXml,
            bool bHasProlog,
            out string strOutXml,
            out string strError)
        {
            strOutXml = "";
            strError = "";

            if (String.IsNullOrEmpty(strXml) == true)
            {
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            if (bHasProlog == true)
                strOutXml = GetIndentXml(dom);
            else
                strOutXml = GetIndentXml(dom.DocumentElement);

            return 0;
        }

        // 获得缩进的XML源代码
        public static string GetIndentXml(XmlNode node)
        {
            using (MemoryStream m = new MemoryStream())
            using (XmlTextWriter w = new XmlTextWriter(m, Encoding.UTF8))
            {
                w.Formatting = Formatting.Indented;
                w.Indentation = 4;
                node.WriteTo(w);
                w.Flush();

                m.Seek(0, SeekOrigin.Begin);

                using (StreamReader sr = new StreamReader(m, Encoding.UTF8))
                {
                    string strText = sr.ReadToEnd();

                    //sr.Close();
                    //w.Close();

                    return strText;
                }
                // 注意，此后 m 已经关闭
            }
        }

        public static string GetIndentInnerXml(XmlNode node)
        {
            using (MemoryStream m = new MemoryStream())
            using (XmlTextWriter w = new XmlTextWriter(m, Encoding.UTF8))
            {
                w.Formatting = Formatting.Indented;
                w.Indentation = 4;
                node.WriteContentTo(w);
                w.Flush();

                m.Seek(0, SeekOrigin.Begin);

                using (StreamReader sr = new StreamReader(m, Encoding.UTF8))
                {
                    string strText = sr.ReadToEnd();
                    //sr.Close();
                    //w.Close();

                    return strText;
                }
                // 注意，此后 m 已经关闭
            }
        }

        public static string GetDomEncodingString(XmlDocument dom)
        {
            if (dom.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
            {
                XmlDeclaration dec = (XmlDeclaration)dom.FirstChild;
                return dec.Encoding;
            }

            return null;
        }

        public static bool SetDomEncodingString(XmlDocument dom,
            string strEncoding)
        {
            if (dom.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
            {
                XmlDeclaration dec = (XmlDeclaration)dom.FirstChild;
                dec.Encoding = strEncoding;
                return true;
            }

            return false;
        }

        // 获得缩进的XML源代码
        // 注：包含prolog等。如果不想包含这些，请用GetIndentXml(XmlNode)版本
        public static string GetIndentXml(XmlDocument dom)
        {
            string strEncoding = GetDomEncodingString(dom);
            Encoding encoding = Encoding.UTF8;
            if (string.IsNullOrEmpty(strEncoding) == false)
            {
                try
                {
                    encoding = Encoding.GetEncoding(strEncoding);
                }
                catch
                {
                    encoding = Encoding.UTF8;
                }
            }

            // 
            using (MemoryStream m = new MemoryStream())
            using (XmlTextWriter w = new XmlTextWriter(m, encoding))
            {
                w.Formatting = Formatting.Indented;
                w.Indentation = 4;
                dom.Save(w);
                w.Flush();

                m.Seek(0, SeekOrigin.Begin);

                using (StreamReader sr = new StreamReader(m, encoding))
                {
                    string strText = sr.ReadToEnd();
                    //sr.Close();
                    //w.Close();

                    return strText;
                }
                // 注意，此后 m 已经关闭
            }
        }


        // 得到一个节点在父亲的儿子集中的序号 从0开始
        // parameters:
        //      node    儿子节点
        // return:
        //      返回在父亲的儿子集中的序号，-1没找到
        // 编写者: 任延华
        public static int GetIndex(XmlNode node)
        {
            Debug.Assert(node != null, "GetIndex()调用出错，node参数值不能为null。");

            XmlNode parentNode = node.ParentNode;
            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                XmlNode curNode = parentNode.ChildNodes[i];
                if (curNode == node)
                    return i;
            }
            return -1;
        }


        // 得到parentNode的第一个element儿子节点
        // parameter:
        //		parentNode	父亲节点
        // return:
        //		第一个element儿子节点，未找到返回null
        // 编写者: 任延华
        public static XmlElement GetFirstElementChild(XmlNode parentNode)
        {
            Debug.Assert(parentNode != null, "GetFirstElementChild()出错，parentNode参数值不能为null。");

            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                XmlNode node = parentNode.ChildNodes[i];
                if (node.NodeType == XmlNodeType.Element)
                    return (XmlElement)node;
            }
            return null;
        }

        // 得到parentNode的第一个CDATA儿子节点
        // parameter:
        //		parentNode	父亲节点
        // return:
        //		第一个XmlCDataSection儿子节点
        // 编写者: 任延华
        public static XmlCDataSection GetFirstCDATAChild(XmlNode parentNode)
        {
            Debug.Assert(parentNode != null, "GetFirstCDATAChild()出错，parentNode参数值不能为null。");

            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                XmlNode node = parentNode.ChildNodes[i];
                if (node.NodeType == XmlNodeType.CDATA)
                    return (XmlCDataSection)node;
            }
            return null;
        }


        // 从根节点开始，根据指定的元素节点xpath和属性名，得到属性值
        // 编写者：谢涛
        public static string GetAttr(XmlNode nodeRoot,
            string strNodePath,
            string strAttrName)
        {
            XmlNode node = nodeRoot.SelectSingleNode(strNodePath);

            if (node == null)
                return "";

            return GetAttr(node, strAttrName);
        }

        // 探测XmlNode节点的指定属性是否存在
        // parameters:
        //      node        XmlNode节点
        //      strAttrName    属性名称
        // return:
        public static bool HasAttr(XmlNode node,
            string strAttrName)
        {
            Debug.Assert(node != null, "GetAttr()调用错误，node参数值不能为null。");
            Debug.Assert(strAttrName != null && strAttrName != "", "GetAttr()调用错误，strAttrName参数值不能为null或空字符串。");

            // 2012/4/25 NodeType == Document的节点，其Attributes成员为null
            if (node.Attributes == null)
                return false;

            if (node.Attributes[strAttrName] == null)
                return false;
            return true;
        }

        // 得到XmlNode节点的指定属性的值
        // TODO: 找属性使用的SelectSingleNode()函数，是否会浪费时间，可做测试与直接从node对应的属性集合中找属性用的时间比较。
        // parameters:
        //      node        XmlNode节点
        //      strAttrName    属性名称
        // return:
        //      返回属性值
        //      注：如何未找到指定的属性节点，返回""
        public static string GetAttr(XmlNode node,
            string strAttrName)
        {
            Debug.Assert(node != null, "GetAttr()调用错误，node参数值不能为null。");
            Debug.Assert(strAttrName != null && strAttrName != "", "GetAttr()调用错误，strAttrName参数值不能为null或空字符串。");

            // 2012/4/25 NodeType == Document的节点，其Attributes成员为null
            if (node.Attributes == null)
                return "";

            // 2012/2/16 优化
            XmlAttribute attr = node.Attributes[strAttrName];
            if (attr == null)
                return "";
            return attr.Value;
        }

        // 得到XmlNode节点指定名称空间的属性的值
        // parameters:
        //      strNameSpaceUrl 属性的名字空间的url
        //      node            XmlNode节点
        //      strAttrName        属性名称
        // return:
        //      指定属性的值
        //      注：如果未找到指定的属性节点，返回"";
        // ???找属性使用的SelectSingleNode()函数，是否会浪费时间，可做测试与直接从node对应的属性集合中找属性用的时间比较。
        public static string GetAttr(string strAttrNameSpaceUri,
            XmlNode node,
            string strAttrName)
        {
            Debug.Assert(node != null, "GetAttr()调用错误，node参数值不能为null。");
            Debug.Assert(strAttrName != null && strAttrName != "", "GetAttr()调用错误，strAttrName参数值不能为null或空字符串。");
            Debug.Assert(strAttrNameSpaceUri != null && strAttrNameSpaceUri != "",
                "GetAttr()调用错误，strNameSpaceUri参数值不能为null或空字符串。");

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("abc", strAttrNameSpaceUri);
            XmlNode nodeAttr = node.SelectSingleNode("@abc:" + strAttrName, nsmgr);

            if (nodeAttr == null)
                return "";
            else
                return nodeAttr.Value;
        }


        // 得到XmlNode节点的指定属性的值
        // parameters:
        //      node        XmlNode节点
        //      strAttrName    属性名称
        // return:
        //      返回属性值
        //      注：如何未找到指定的属性节点，返回null
        // 编写者：任延华
        // ???找属性使用的SelectSingleNode()函数，是否会浪费时间，可做测试与直接从node对应的属性集合中找属性用的时间比较。
        public static string GetAttrDiff(XmlNode node,
            string strAttrName)
        {
            Debug.Assert(node != null, "GetAttrDiff()调用错误，node参数值不能为null。");
            Debug.Assert(strAttrName != null && strAttrName != "", "GetAttrDiff()调用错误，strAttrName参数值不能为null或空字符串。");


            /*
            XmlNode nodeAttr = node.SelectSingleNode("@" + strAttrName);

            if (nodeAttr == null)
                return null;
            else
                return nodeAttr.Value;
             * */
            // 2012/4/25 NodeType == Document的节点，其Attributes成员为null
            if (node.Attributes == null)
                return null;

            // 2012/2/16 优化
            XmlAttribute attr = node.Attributes[strAttrName];
            if (attr == null)
                return null;
            return attr.Value;
        }

        // 编写者：谢涛
        public static string GetAttrOrDefault(XmlNode node,
            string strAttrName,
            string strDefault)
        {
            if (node == null)
                return strDefault;
            /*
			XmlNode nodeAttr = node.SelectSingleNode("@" + attrName);

			if (nodeAttr == null)
				return strDefault;
			else
				return nodeAttr.Value;
             * */

            Debug.Assert(node.Attributes != null, "");
            /*
            // 2012/4/25 NodeType == Document的节点，其Attributes成员为null
            if (node.Attributes == null)
                return strDefault;
             * */

            // 2012/2/16 优化
            XmlAttribute attr = node.Attributes[strAttrName];
            if (attr == null)
                return strDefault;
            return attr.Value;

        }


        // 设置XmlNode节点指定属性的值
        // 注: 2013/2/22 今后最好用 XmlElement node.SetAttribute(strAttrName, strAttrValue) 来替代本函数
        // parameters:
        //      node            XmlNode节点
        //      strAttrName     属性名称
        //      strAttrValue    属性值,可以为""或null,如果==null,表示删除这个属性
        public static void SetAttr(XmlNode node,
            string strAttrName,
            string strAttrValue)
        {
            Debug.Assert(node != null, "SetAttr()调用错误，node参数值不能为null。");
            Debug.Assert(strAttrName != null && strAttrName != "", "SetAttr()调用错误，strAttrName参数值不能为null或空字符串。");

            // 2012/4/25 NodeType == Document的节点，其Attributes成员为null
            Debug.Assert(node.Attributes != null, "");

            XmlAttributeCollection listAttr = node.Attributes;
            XmlAttribute attrFound = listAttr[strAttrName];

            if (attrFound == null)
            {
                if (strAttrValue == null)
                    return;	// 本来就不存在

                XmlElement element = (XmlElement)node;
                element.SetAttribute(strAttrName, strAttrValue);
            }
            else
            {
                if (strAttrValue == null)
                    node.Attributes.Remove(attrFound);
                else
                    attrFound.Value = strAttrValue;
            }
        }

        // 设置XmlNode元素节点的属性值，名字空间版本
        // parameters:
        //      node                XmlNode节点
        //      strAttrName         属性名称
        //      strAttrNameSpaceURI 属性名字空间的URI
        //      strAttrValue        属性值,如果==null,则删除这个属性
        public static void SetAttr(XmlNode node,
            string strAttrName,
            string strAttrNameSpaceURI,
            string strAttrValue)
        {
            Debug.Assert(node != null, "SetAttr()调用错误，node参数值不能为null。");
            Debug.Assert(strAttrName != null && strAttrName != "", "SetAttr()调用错误，strAttrName参数值不能为null或空字符串。");
            Debug.Assert(strAttrNameSpaceURI != null && strAttrNameSpaceURI != "", "SetAttr()调用错误，strAttrNameSpaceURI参数值不能为null或空字符串。");

            // 2012/4/25 NodeType == Document的节点，其Attributes成员为null
            Debug.Assert(node.Attributes != null, "");

            XmlAttributeCollection listAttr = node.Attributes;
            XmlAttribute attrFound = listAttr[strAttrName, strAttrNameSpaceURI];

            if (attrFound == null)
            {
                if (strAttrValue == null)
                    return;	// 本来就不存在

                XmlElement element = (XmlElement)node;
                element.SetAttribute(strAttrName, strAttrNameSpaceURI, strAttrValue);
            }
            else
            {
                if (strAttrValue == null)
                    node.Attributes.Remove(attrFound);
                else
                    attrFound.Value = strAttrValue;
            }
        }

        // 设置XmlNode元素节点的属性值，前缀和名字空间版本
        // parameters:
        //      node                XmlNode节点
        //      strAttrName         属性名称
        //      strPrefix   前缀
        //      strAttrNameSpaceURI 属性名字空间的URI
        //      strAttrValue        属性值,如果==null,则删除这个属性
        public static void SetAttr(XmlNode node,
            string strName,
            string strPrefix,
            string strNamespaceURI,
            string strValue)
        {
            Debug.Assert(node != null, "SetAttr()调用错误，node参数值不能为null。");
            Debug.Assert(String.IsNullOrEmpty(strName) == false, "SetAttr()调用错误，strName参数值不能为null或空字符串。");
            Debug.Assert(String.IsNullOrEmpty(strNamespaceURI) == false, "SetAttr()调用错误，strNamespaceURI参数值不能为null或空字符串。");
            Debug.Assert(String.IsNullOrEmpty(strPrefix) == false, "SetAttr()调用错误，strPrefix参数值不能为null或空字符串。");

            // 2012/4/25 NodeType == Document的节点，其Attributes成员为null
            Debug.Assert(node.Attributes != null, "");

            XmlAttribute attrFound = node.Attributes[strName, strNamespaceURI];

            if (attrFound == null)
            {
                if (strValue == null)
                    return;	// 本来就不存在

                XmlElement element = (XmlElement)node;
                XmlAttribute attr = node.OwnerDocument.CreateAttribute(strPrefix, strName, strNamespaceURI);
                attr.Value = strValue;
                element.SetAttributeNode(attr);
            }
            else
            {
                if (strValue == null)
                    node.Attributes.Remove(attrFound);
                else
                    attrFound.Value = strValue;
            }
        }

        // 得到childNodes集合中，所有的CDATA节点
        // parameters:
        //      childNodes: 儿子节点集合，里面有各种类型的节点
        // return:
        //      返回所有CDATA节点组成的数组,如果一个CDATA节点都没有，返回一个空集合
        // 编写者：任延华
        public static ArrayList GetCdataNodes(XmlNodeList childNodes)
        {
            Debug.Assert(childNodes != null, "GetCdataNodes()调用错误，childNodes参数值不能为null。");

            ArrayList aCDATA = new ArrayList();
            foreach (XmlNode item in childNodes)
            {
                if (item.NodeType == XmlNodeType.CDATA)
                    aCDATA.Add(item);
            }
            return aCDATA;
        }


        // 通过strXpath路径逐级创建node，如果strXpath对应的节点已存在，则直接返回
        // paramter:
        //		nodeRoot	根节点
        //		strXpath	简单的xpath，最后一层可以是属性名称(即@属性名)
        // return:
        //      返回strXpath对应的节点
        // 编写者：任延华
        public static XmlNode CreateNodeByPath(XmlNode nodeRoot,
            string strXpath)
        {
            Debug.Assert(nodeRoot != null, "CreateNodeByPath()调用错误，nodeRoot参数值不能为null。");

            XmlNode nodeFound = nodeRoot.SelectSingleNode(strXpath);
            if (nodeFound != null)
                return nodeFound;

            string[] aNodeName = strXpath.Split(new Char[] { '/' });
            return DomUtil.CreateNode(nodeRoot, aNodeName);
        }

        // 根据名称数组逐级创建节点
        // parameters:
        //      nodeRoot    根节点
        //      aNodeName   节点名称数组
        // return:
        //      返回新创建的XmlNode节点
        // 编写者：任延华
        public static XmlNode CreateNode(XmlNode nodeRoot,
            string[] aNodeName)
        {
            XmlDocument dom = nodeRoot.OwnerDocument;
            if (dom == null)
            {
                if (nodeRoot is XmlDocument)
                    dom = (XmlDocument)nodeRoot;
                else
                    throw (new Exception("CreateNode()发生异常，nodeRoot的OwnerDocument属性值为null，且nodeRoot不是XmlDocument类型。"));
            }

            if (aNodeName.Length == 0)
                return null;

            int i = 0;
            if (aNodeName[0] == "")
                i = 1;

            XmlNode nodeCurrent = nodeRoot;
            XmlNode temp = null;
            for (; i < aNodeName.Length; i++)
            {
                string strOneName = aNodeName[i];
                if (strOneName == "")
                    throw new Exception("通过CreateNode()创建元素时，第'" + Convert.ToInt32(i) + "'级的名称为空。");

                temp = nodeCurrent.SelectSingleNode(strOneName);
                if (temp == null)
                {
                    Char firstChar = strOneName[0];
                    if (firstChar == '@' && i == aNodeName.Length - 1)
                    {
                        string strAttrName = strOneName.Substring(1);
                        if (strAttrName == "")
                            throw new Exception("通过CreateNode()创建元素时，第'" + Convert.ToInt32(i) + "'级的属性名称为空。");
                        DomUtil.SetAttr(nodeCurrent, strAttrName, "");
                        temp = nodeCurrent.SelectSingleNode("@" + strAttrName);
                        if (temp == null)
                            throw new Exception("已经创建了'" + strAttrName + "'属性，不可能找不到。");
                    }
                    else
                    {
                        temp = dom.CreateElement(aNodeName[i]);
                        nodeCurrent.AppendChild(temp);
                    }
                }
                nodeCurrent = temp;
            }

            return nodeCurrent;
        }

        // TODO: 逐渐废止这个函数
        // 得到node节点的第一个文本节点的值,相当于GetNodeFirstText()
        // parameter:
        //		node    XmlNode节点
        // result:
        //		node的第一个文本节点的字符串，不去空白
        //      注：如果node下级不存在文本节点，返回"";
        // 编写者：任延华
        public static string GetNodeText(XmlNode node)
        {
            Debug.Assert(node != null, "GetNodeText()调用出错，node参数值不能为null。");

            XmlNode nodeText = node.SelectSingleNode("text()");
            if (nodeText == null)
                return "";
            else
                return nodeText.Value;
        }

        // TODO: 逐渐废止这个函数
        // 得到node节点的第一个文本节点的值
        // parameter:
        //		node    XmlNode节点
        // result:
        //		node的第一个文本节点的字符串，左右去空白
        //      注：如果node下级不存在文本节点，返回null;
        // 编写者：任延华
        public static string GetNodeTextDiff(XmlNode node)
        {
            Debug.Assert(node != null, "GetNodeTextDiff()调用出错，node参数值不能为null。");

            XmlNode nodeText = node.SelectSingleNode("text()");
            if (nodeText == null)
                return null;
            else
                return nodeText.Value;
        }


        // 得到node节点的第一个文本节点的值
        // parameter:
        //		node    XmlNode节点
        // result:
        //		node的第一个文本节点的字符串，左右去空白
        //      注：如果node下级不存在文本节点，返回"";
        // 编写者：任延华
        public static string GetNodeFirstText(XmlNode node)
        {
            Debug.Assert(node != null, "GetNodeFirstText()调用出错，node参数值不能为null。");

            XmlNode nodeText = node.SelectSingleNode("text()");
            if (nodeText == null)
                return "";
            else
                return nodeText.Value.Trim();
        }

#if NO
        // TODO: 逐渐废止这个函数
        // 得到当前节点所有的文本节点值
		// parameter:
		//      node    XmlNode节点
		// result:
		//		node的所有文本节点组合起来的字符串，中间不加任何符号，去每个文字节点内容的左右空白
        //      注：如果node下级不存在文本节点，返回"";
		// 编写者：任延华
		public static string  GetNodeAllText(XmlNode node)
		{
            Debug.Assert(node != null, "GetNodeAllText()调用出错，node参数值不能为null。");

			XmlNodeList nodeTextList = node.SelectNodes("text()");
			string strResult = "";
			foreach(XmlNode oneNode in nodeTextList)
			{
				strResult += oneNode.Value.Trim ();   //把左右空白都去掉
			}
			return strResult;
		}
#endif

        // 设node节点的第一个文本节点的内容
        // parameters:
        //      node    XmlNode节点
        //      strNewText  新的文字内容
        // return:
        //      void
        // 编写者：任延华
        public static void SetNodeText(XmlNode node,
            string newText)
        {
            Debug.Assert(node != null, "SetNodeText()调用错误，node参数值不能为null。");

            XmlNode nodeText = node.SelectSingleNode("text()");
            if (nodeText == null)
                node.AppendChild(node.OwnerDocument.CreateTextNode(newText));
            else
                nodeText.Value = newText;
        }

        // 对指定的节点设第一个文本节点的值
        // 如果第一个文本节点存在,则直接给text赋值，
        // 如果第一个文本节点不存在，则调CreateNode()逐级创建节点，然后赋值
        // parameters:
        //      nodeRoot    根节点
        //      strXpath    节点路径
        //      strNewText  新文本值
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 编写者：任延华
        public static int SetNodeValue(XmlNode nodeRoot,
            string strXpath,
            string strNewText,
            out string strError)
        {
            strError = "";

            Debug.Assert(nodeRoot != null, "SetNodeValue()调用错误，nodeRoot参数值不能为null。");
            Debug.Assert(strXpath != null && strXpath != "", "SetNodeValue()调用错误，strXpath参数值不能为null或空字符串。");


            XmlNode nodeFound = nodeRoot.SelectSingleNode(strXpath);
            if (nodeFound == null)
            {
                string[] aNodeName = strXpath.Split(new Char[] { '/' });
                try
                {
                    nodeFound = DomUtil.CreateNode(nodeRoot, aNodeName);
                }
                catch (Exception ex)
                {
                    strError = "CreateNode()出错，原因：" + ex.Message;
                    return -1;
                }
            }

            if (nodeFound == null)
            {
                strError = "SetNodeValue()，此时nodeFound不可能为null了。";
                return -1;
            }

            DomUtil.SetNodeText(nodeFound, strNewText);
            return 0;
        }

        // TODO: 这个函数的功能令人费解，逐步废止?
        // 编写者: 谢涛
        public static int SetNodeValue(XmlNode nodeRoot,
            string strXpath,
            XmlNode newNode)
        {
            if (nodeRoot == null)
                return -1;

            XmlNode nodeFound = nodeRoot.SelectSingleNode(strXpath);

            if (nodeFound == null)
            {
                string[] aNodeName = strXpath.Split(new Char[] { '/' });
                nodeFound = CreateNode(nodeRoot, aNodeName);
            }

            if (nodeFound == null)
                return -1;


            //XmlNode nodeTemp = nodeFound.OwnerDocument.CreateElement("test");
            //nodeTemp = newNode.CloneNode(true);

            nodeFound.InnerXml = newNode.OuterXml;

            //nodeFound.AppendChild(newNode.CloneNode(true));


            return 0;
        }


        // 2006/11/29
        public static string GetElementInnerXml(XmlNode nodeRoot,
    string strXpath)
        {
            XmlNode node = nodeRoot.SelectSingleNode(strXpath);
            if (node == null)
                return "";

            return node.InnerXml;
        }

        // 写入一个元素文本
        // return:
        //      返回该元素的XmlNode
        public static XmlNode SetElementInnerXml(XmlNode nodeRoot,
            string strXpath,
            string strInnerXml)
        {
            if (nodeRoot == null)
            {
                throw (new Exception("nodeRoot参数不能为null"));
            }

            XmlNode nodeFound = nodeRoot.SelectSingleNode(strXpath);

            if (nodeFound == null)
            {
                string[] aNodeName = strXpath.Split(new Char[] { '/' });
                nodeFound = CreateNode(nodeRoot, aNodeName);
            }

            if (nodeFound == null)
            {
                throw (new Exception("SetElementInnerXml() CreateNode error"));
            }

            nodeFound.InnerXml = strInnerXml;
            return nodeFound;
        }

        // 2006/11/29
        public static string GetElementOuterXml(XmlNode nodeRoot,
    string strXpath)
        {
            XmlNode node = nodeRoot.SelectSingleNode(strXpath);
            if (node == null)
                return "";

            return node.OuterXml;
        }

        public static bool IsEmptyElement(XmlNode nodeRoot,
    string strXpath)
        {
            XmlNode node = nodeRoot.SelectSingleNode(strXpath);
            if (node == null)
                return true;

            if (node.Attributes.Count == 0 && node.ChildNodes.Count == 0)
                return true;

            return false;
        }

        // 写入一个元素文本
        // return:
        //      返回该元素的XmlNode
        public static XmlNode SetElementOuterXml(XmlNode nodeRoot,
            string strXpath,
            string strOuterXml)
        {
            if (nodeRoot == null)
            {
                throw (new Exception("nodeRoot参数不能为null"));
            }

            XmlNode nodeFound = nodeRoot.SelectSingleNode(strXpath);
            if (nodeFound == null)
            {
                string[] aNodeName = strXpath.Split(new Char[] { '/' });
                nodeFound = CreateNode(nodeRoot, aNodeName);
            }

            if (nodeFound == null)
            {
                throw (new Exception("SetElementOuterXml() CreateNode error"));
            }

            XmlDocumentFragment fragment = nodeFound.OwnerDocument.CreateDocumentFragment();
            fragment.InnerXml = strOuterXml;

            nodeFound.ParentNode.InsertAfter(fragment, nodeFound);

            nodeFound.ParentNode.RemoveChild(nodeFound);

            nodeFound = nodeRoot.SelectSingleNode(strXpath);
            return nodeFound;
        }

        // 2009/10/31
        // 写入一个元素的OuterXml
        // return:
        //      返回变动后该元素的XmlNode
        public static XmlNode SetElementOuterXml(XmlNode node,
            string strOuterXml)
        {
            if (node == null)
            {
                throw (new Exception("node参数不能为null"));
            }

            XmlDocumentFragment fragment = node.OwnerDocument.CreateDocumentFragment();
            fragment.InnerXml = strOuterXml;

            node.ParentNode.InsertAfter(fragment, node);

            XmlNode new_node = node.NextSibling;    // 2012/12/12 新增加

            node.ParentNode.RemoveChild(node);

            return new_node;
        }

        // 插入新对象到儿子们的最前面
        public static XmlNode InsertFirstChild(XmlNode parent, XmlNode newChild)
        {
            XmlNode refChild = null;
            if (parent.ChildNodes.Count > 0)
                refChild = parent.ChildNodes[0];

            return parent.InsertBefore(newChild, refChild);
        }

        // 2012/9/30
        public static string GetElementInnerText(XmlNode nodeRoot,
    string strXpath)
        {
            XmlNode node = nodeRoot.SelectSingleNode(strXpath);
            if (node == null)
                return null;

            return node.InnerText.Trim();
        }

        // 2012/9/30
        // 获得一个元素的一个属性值
        public static string GetElementAttr(XmlNode nodeRoot,
            string strXpath,
            string strAttrName)
        {
            XmlNode node = nodeRoot.SelectSingleNode(strXpath);
            if (node == null)
                return null;
            XmlAttribute attr = node.Attributes[strAttrName];
            if (attr == null)
                return null;
            return attr.Value.Trim();
        }

        // 编写者: 谢涛
        public static string GetElementText(XmlNode nodeRoot,
            string strXpath)
        {
            XmlNode node = nodeRoot.SelectSingleNode(strXpath);
            if (node == null)
                return "";

            XmlNode nodeText;
            nodeText = node.SelectSingleNode("text()");

            if (nodeText == null)
                return "";
            else
                return nodeText.Value;
        }

        // 新版本 2006/10/24
        // 获得一个元素的下级文本
        // 一并返回元素节点对象
        public static string GetElementText(XmlNode nodeRoot,
            string strXpath,
            out XmlNode node)
        {
            node = nodeRoot.SelectSingleNode(strXpath);
            if (node == null)
                return "";

            return node.InnerText;
        }

        // 编写者: 谢涛
        public static string GetElementText(XmlNode nodeRoot,
            string strXpath,
            XmlNamespaceManager mngr)
        {
            XmlNode node = nodeRoot.SelectSingleNode(strXpath, mngr);
            if (node == null)
                return "";

            XmlNode nodeText;
            nodeText = node.SelectSingleNode("text()");

            if (nodeText == null)
                return "";
            else
                return nodeText.Value;
        }

        // 删除一个元素 2006/10/26
        // return:
        //      返回被删除掉的XmlNode
        public static XmlNode DeleteElement(XmlNode nodeRoot,
            string strXpath)
        {
            if (nodeRoot == null)
            {
                throw (new Exception("nodeRoot参数不能为null"));
            }

            XmlNode nodeFound = nodeRoot.SelectSingleNode(strXpath);

            if (nodeFound == null)
                return null;    // 既然不存在，正好也不必删除了

            return nodeFound.ParentNode.RemoveChild(nodeFound);
        }

        // 删除若干个元素 2011/1/11
        // return:
        //      返回被删除掉的XmlNode数组
        public static List<XmlNode> DeleteElements(XmlNode nodeRoot,
            string strXpath)
        {
            if (nodeRoot == null)
            {
                throw (new Exception("nodeRoot参数不能为null"));
            }


            XmlNodeList nodes = nodeRoot.SelectNodes(strXpath);

            if (nodes.Count == 0)
                return null;    // 既然不存在，正好也不必删除了

            List<XmlNode> deleted_nodes = new List<XmlNode>();
            foreach (XmlNode node in nodes)
            {
                if (node.ParentNode == null)
                    continue;
                deleted_nodes.Add(node.ParentNode.RemoveChild(node));
            }

            return deleted_nodes;
        }

        /*
        // 移除一个元素
        // 2007/6/19
        public static XmlNode RemoveElement(XmlNode nodeRoot,
            string strXPath)
        {
            if (nodeRoot == null)
            {
                throw (new Exception("nodeRoot参数不能为null"));
            }

            XmlNode nodeFound = nodeRoot.SelectSingleNode(strXPath);

            if (nodeFound == null)
            {
                // 正好不存在，也不必删除了
                return null;
            }

            nodeFound.ParentNode.RemoveChild(nodeFound);

            return nodeFound;
        }*/

        // 替换全部控制字符
        // parameters:
        //      chReplace   要替换成的字符。如果为 0 ，表示删除控制字符
        static string ReplaceControlChars(string strText,
            char chReplace)
        {
            if (String.IsNullOrEmpty(strText) == true)
                return strText;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];
                if (ch >= 0x1 && ch <= 0x1f)
                {
                    if (chReplace != 0)
                        sb.Append(chReplace);
                }
                else
                    sb.Append(ch);
            }

            return sb.ToString();
        }

        // 替换控制字符，但不替换 \0d \0a
        // parameters:
        //      chReplace   要替换成的字符。如果为 0 ，表示删除控制字符
        static string ReplaceControlCharsButCrLf(string strText,
    char chReplace)
        {
            if (String.IsNullOrEmpty(strText) == true)
                return strText;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];
                if (ch >= 0x1 && ch <= 0x1f && ch != 0x0d && ch != 0x0a)
                {
                    if (chReplace != 0)
                        sb.Append(chReplace);
                }
                else
                    sb.Append(ch);
            }

            return sb.ToString();
        }

        // 2010/21/16
        // 写入一个元素文本
        // 不去替换ControlChars
        // return:
        //      返回该元素的XmlNode
        public static XmlNode SetElementTextPure(XmlNode nodeRoot,
            string strXpath,
            string strText)
        {
            if (nodeRoot == null)
            {
                throw (new Exception("nodeRoot参数不能为null"));
            }

            XmlNode nodeFound = nodeRoot.SelectSingleNode(strXpath);

            /*
            // 2007/6/19
            if (nodeFound == null && strText == null)
            {
                // 正好不存在，也不必删除了
                return null;
            }*/


            if (nodeFound == null)
            {
                string[] aNodeName = strXpath.Split(new Char[] { '/' });
                nodeFound = CreateNode(nodeRoot, aNodeName);
            }

            if (nodeFound == null)
            {
                throw (new Exception("SetElementText() CreateNode error"));
            }

            if (String.IsNullOrEmpty(strText) == true)
                nodeFound.InnerText = strText;
            else
                nodeFound.InnerText = strText;

            return nodeFound;
        }

        // 写入一个元素文本
        // 文本内容中可以包含回车换行符号，但其他控制字符在写入的时候会被过滤为星号
        // return:
        //      返回该元素的XmlNode
        public static XmlNode SetElementText(XmlNode nodeRoot,
            string strXpath,
            string strText)
        {
            if (nodeRoot == null)
            {
                throw (new Exception("nodeRoot参数不能为null"));
            }

            XmlNode nodeFound = nodeRoot.SelectSingleNode(strXpath);

            /*
            // 2007/6/19
            if (nodeFound == null && strText == null)
            {
                // 正好不存在，也不必删除了
                return null;
            }*/


            if (nodeFound == null)
            {
                string[] aNodeName = strXpath.Split(new Char[] { '/' });
                nodeFound = CreateNode(nodeRoot, aNodeName);
            }

            if (nodeFound == null)
            {
                throw (new Exception("SetElementText() CreateNode error"));
            }

            /*
            if (strText == null)
            {
                // 2007/6/19
                nodeFound.ParentNode.RemoveChild(nodeFound);
            }
            else
             * */

            if (String.IsNullOrEmpty(strText) == true)
                nodeFound.InnerText = strText;
            else
                nodeFound.InnerText = ReplaceControlCharsButCrLf(strText, '*'); // 2013/3/12 ReplaceControlCharsButCrLf()   // 2008/12/19 ReplaceControlChars()

            return nodeFound;
        }

        // 得到node节点相对于nodeRoot节点的xpath路径
        // parameters:
        //      nodeRoot    根节点
        //      node        指定的节点
        //      strXpath    out参数，返回node相对于nodeRoot的xpath路径
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错,当node不属于nodeRoot下级时
        //      0   成功
        // 编写者: 任延华
        public static int Node2Path(XmlNode nodeRoot,
            XmlNode node,
            out string strXpath,
            out string strError)
        {
            strXpath = "";
            strError = "";

            Debug.Assert(nodeRoot != null, "Node2Path()调用错误，nodeRoot参数值不能为null。");
            Debug.Assert(node != null, "Node2Path()调用错误，node参数值不能为null。");

            //当node为属性节点时，加了属性xpath字符串
            string strAttr = "";
            if (node.NodeType == XmlNodeType.Attribute)
            {
                strAttr = "/@" + node.Name;
                XmlAttribute AttrNode = (XmlAttribute)node;
                node = AttrNode.OwnerElement;
            }

            bool bBelongRoot = false;

            while (node != null)
            {
                if (node == nodeRoot)
                {
                    bBelongRoot = true;
                    break;
                }

                XmlNode nodeMyself = node;

                node = node.ParentNode;
                if (node == null)
                    break;

                XmlNode nodeTemp = node.FirstChild;
                int nIndex = 1;
                while (nodeTemp != null)
                {
                    if (nodeTemp == nodeMyself) //Equals(nodeTemp,nodeMyself))
                    {
                        if (strXpath != "")
                            strXpath = "/" + strXpath;

                        strXpath = nodeMyself.Name + "[" + System.Convert.ToString(nIndex) + "]" + strXpath;
                        break;
                    }
                    if (nodeTemp.Name == nodeMyself.Name)
                        nIndex += 1;

                    nodeTemp = nodeTemp.NextSibling;
                }
            }

            if (bBelongRoot == false)
            {
                strError = "Node2Path()调用错误，node不属于nodeRoot的下级";
                return -1;
            }

            strXpath = strXpath + strAttr;
            return 0;
        }



    } // DomUtil类结束


}
