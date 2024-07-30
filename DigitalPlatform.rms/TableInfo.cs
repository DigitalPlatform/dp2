using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.rms
{
    public class TableInfo : IComparable
    {
        private XmlNode m_node = null;	// XmlNode节点
        Hashtable m_captionTable = new Hashtable();
        public XmlNode Node
        {
            get
            {
                return this.m_node;
            }
            set
            {
                this.m_node = value;
                m_captionTable.Clear();
            }
        }

        public string SqlTableName = "";	// Sql表名
        public string ID = "";			// 表ID
        public string TypeString = "";        // 表类型，风格
        public string ExtTypeString = "";       // _time 等检索特性

        // 2024/7/29
        public List<XmlElement> nodesConvertQueryString = null;
        public List<XmlElement> nodesConvertQueryNumber = null;

        public List<XmlElement> nodesConvertKeyString = null;
        public List<XmlElement> nodesConvertKeyNumber = null;

        /*
        public XmlNode nodeConvertQueryString = null;	// 处理检索词的字符串形态的配置节点 
        public XmlNode nodeConvertQueryNumber = null;	// 处理检索词的数字形态的配置节点

        public XmlNode nodeConvertKeyString = null;		// 处理检索点的字符串形态配置节点
        public XmlNode nodeConvertKeyNumber = null;		// 处理检索点的数字形态的配置节点
        */

        public bool Dup = false;

        public int OriginPosition = -1;  //未初始化
        public bool m_bQuery = false;

        // parameters:
        //		node	<table>节点
        //      strKeysTableNamePrefix  表名前缀字符串，如果 == null，表示使用"keys_"。如果不要前缀，应该是""
        public int Initial(XmlNode node,
            string strKeysTableNamePrefix,
            out string strError)
        {
            Debug.Assert(node != null, "Initial()调用错误，node参数值不能为null。");
            strError = "";

            this.Node = node;

            string strPartSqlTableName = DomUtil.GetAttr(this.m_node, "name").Trim();
            if (strPartSqlTableName == "")
            {
                strError = "未定义 'name' 属性。";
                return -1;
            }

            if (string.Compare(strPartSqlTableName, "records", true) == 0)
            {
                strError = "'name' 属性中的表名不能为 'records'。因为这是一个系统的保留字。";
                return -1;
            }

            if (strKeysTableNamePrefix == null)
                strKeysTableNamePrefix = "keys_";

            this.SqlTableName = strKeysTableNamePrefix + strPartSqlTableName;

            if (node != null)
            {
                this.ID = DomUtil.GetAttr(node, "id");
                this.TypeString = DomUtil.GetAttr(node, "type");
            }

            if (this.ID == "")
            {
                strError = "未定义'id'属性。";
                return -1;
            }

            XmlNode nodeConvert = node.SelectSingleNode("convert");
            if (nodeConvert != null)
            {
                string strStopwordTable = DomUtil.GetAttrDiff(nodeConvert, "stopwordTable");
                if (strStopwordTable != null)
                {
                    strError = "keys配置文件是旧版本，而目前<convert>元素已经不支持'stopwordTable'属性。请修改配置文件";
                    return -1;
                }
            }

            XmlNode nodeConvertQuery = node.SelectSingleNode("convertquery");
            if (nodeConvertQuery != null)
            {
                string strStopwordTable = DomUtil.GetAttrDiff(nodeConvertQuery, "stopwordTable");
                if (strStopwordTable != null)
                {
                    strError = "keys配置文件是旧版本，而目前<convertquery>元素已经不支持'stopwordTable'属性。请修改配置文件";
                    return -1;
                }
            }

            this.nodesConvertKeyString = BuildList(node.SelectSingleNode("convert/string") as XmlElement);
            this.nodesConvertKeyNumber = BuildList(node.SelectSingleNode("convert/number") as XmlElement);

            this.nodesConvertQueryString = BuildList(node.SelectSingleNode("convertquery/string") as XmlElement);
            this.nodesConvertQueryNumber = BuildList(node.SelectSingleNode("convertquery/number") as XmlElement);

            /*
            this.nodeConvertKeyString = node.SelectSingleNode("convert/string");
            this.nodeConvertKeyNumber = node.SelectSingleNode("convert/number");

            this.nodeConvertQueryString = node.SelectSingleNode("convertquery/string");
            this.nodeConvertQueryNumber = node.SelectSingleNode("convertquery/number");
            */

            SetExtTypeString();
            return 0;
        }

        static List<XmlElement> BuildList(XmlElement node)
        {
            if (node == null)
                return null;
            return new List<XmlElement> { node };
        }

        // 2012/5/16
        void SetExtTypeString()
        {
            XmlNode nodeConvertKeyNumber = null;
            if (nodesConvertKeyNumber != null
                && nodesConvertKeyNumber.Count > 0)
                nodeConvertKeyNumber = nodesConvertKeyNumber[0];

            XmlNode nodeConvertQueryNumber = null;
            if (nodesConvertQueryNumber != null
                && nodesConvertQueryNumber.Count > 0)
                nodeConvertQueryNumber = nodesConvertQueryNumber[0];

            if (nodeConvertKeyNumber != null
                && nodeConvertQueryNumber != null)
            {
                string strExtStyle = "";
                string strStyleKey = DomUtil.GetAttr(nodeConvertKeyNumber, "style");
                string strStyleQuery = DomUtil.GetAttr(nodeConvertKeyNumber, "style");
                if (StringUtil.IsInList("freetime", strStyleQuery) == true)
                {
                    StringUtil.SetInList(ref strExtStyle, "_time", true);
                    StringUtil.SetInList(ref strExtStyle, "_freetime", true);
                }

                if (StringUtil.IsInList("rfc1123time", strStyleQuery) == true)
                {
                    StringUtil.SetInList(ref strExtStyle, "_time", true);
                    StringUtil.SetInList(ref strExtStyle, "_rfc1123time", true);
                }

                if (StringUtil.IsInList("utime", strStyleQuery) == true)
                {
                    StringUtil.SetInList(ref strExtStyle, "_time", true);
                    StringUtil.SetInList(ref strExtStyle, "_utime", true);
                }

                this.ExtTypeString = strExtStyle;
            }
        }


        // 得到以逗号分隔的所有语言版本信息
        public string GetAllCaption()
        {
            string strCaptions = "";
            XmlNodeList nodeList = this.m_node.SelectNodes("caption");
            foreach (XmlNode node in nodeList)
            {
                if (strCaptions != "")
                    strCaptions += ",";
                strCaptions += node.InnerText.Trim();    // 2012/2/16
            }

            if (strCaptions != "")
                strCaptions += ",";
            strCaptions += "@" + this.ID;

            return strCaptions;
        }

        // 获得所有语言代码的标签
        // 每个字符串, 左边是语言代码, 间隔一个冒号, 右边是文字内容
        public List<string> GetAllLangCaption()
        {
            List<string> results = new List<string>();

            XmlNode node = this.m_node;

            XmlNodeList nodes = node.SelectNodes("caption");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strLang = DomUtil.GetAttr(nodes[i], "lang");
                string strText = nodes[i].InnerText;

                results.Add(strLang + ":" + strText);
            }

            return results;
        }

        // 有缓存的版本
        public string GetCaption(string strLang)
        {
            string strResult = (string)this.m_captionTable[strLang == null ? "<null>" : strLang];
            if (strResult != null)
                return strResult;

            strResult = GetCaptionInternal(strLang);
            this.m_captionTable[strLang == null ? "<null>" : strLang] = strResult;
            return strResult;
        }

        // 取一个节点指定的某种语言代码的标签信息
        public string GetCaptionInternal(string strLang)
        {
            XmlNode node = this.m_node;

            XmlNode nodeCaption = null;
            string strCaption = "";
            string strXPath = "";

            if (strLang == null)
                goto END1;

            strLang = strLang.Trim();
            if (strLang == "")
                goto END1;

            // 1.先精确找
            strXPath = "caption[@lang='" + strLang + "']";
            nodeCaption = node.SelectSingleNode(strXPath);
            if (nodeCaption != null)
                strCaption = nodeCaption.InnerText.Trim();   // 2012/2/16
            if (string.IsNullOrEmpty(strCaption) == false)
                return strCaption;


            // 2.将语言版本截成两字符精确找
            if (strLang.Length >= 2)
            {
                string strShortLang = strLang.Substring(0, 2);
                strXPath = "caption[@lang='" + strShortLang + "']";
                nodeCaption = node.SelectSingleNode(strXPath);
                if (nodeCaption != null)
                    strCaption = nodeCaption.InnerText.Trim(); // 2012/2/16
                if (string.IsNullOrEmpty(strCaption) == false)
                    return strCaption;

                // 3.找前两个字符相同的排在第一版本
                strXPath = "caption[(substring(@lang,1,2)='" + strShortLang + "')]";
                nodeCaption = node.SelectSingleNode(strXPath);
                if (nodeCaption != null)
                    strCaption = nodeCaption.InnerText.Trim(); // 2012/2/16
                if (string.IsNullOrEmpty(strCaption) == false)
                    return strCaption;

            }

        END1:
            // 4.找排在第一位的caption
            strXPath = "caption";
            nodeCaption = node.SelectSingleNode(strXPath);
            if (nodeCaption != null)
                strCaption = nodeCaption.InnerText.Trim(); // 2012/2/16
            if (string.IsNullOrEmpty(strCaption) == false)
                return strCaption;


            // 5.最后返回@id
            string strID = "";
            if (node != null)
                strID = DomUtil.GetAttr(node, "id");
            if (strID == "")
                throw new Exception("检索点表的id不可能为null。");

            return "@" + strID;

        }


        public int CompareTo(object myObject)
        {
            TableInfo tableInfo = (TableInfo)myObject;

            int nRet = 0;

            int nThisID = 0;
            bool bError = false;
            try
            {
                nThisID = Convert.ToInt32(this.ID);
            }
            catch
            {
                bError = true;
            }

            int nObjectID = 0;
            try
            {
                nObjectID = Convert.ToInt32(tableInfo.ID);
            }
            catch
            {
                bError = true;
            }

            if (bError == false)
            {
                nRet = nThisID - nObjectID;
            }
            else
            {
                nRet = String.Compare(this.ID, tableInfo.ID);
            }

            if (nRet != 0)
                return nRet;

            if (this.m_bQuery != tableInfo.m_bQuery)
            {
                if (this.m_bQuery == true)
                    return -1;
                else
                    return 1;
            }

            // ??? 序号永远是不等的
            return this.OriginPosition - tableInfo.OriginPosition;
        }
    }
}
