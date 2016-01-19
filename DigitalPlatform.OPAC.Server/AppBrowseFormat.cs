using System;
using System.Collections.Generic;
using System.Xml;

using DigitalPlatform.Xml;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 本部分是和浏览格式相关的代码
    /// </summary>
    public partial class OpacApplication
    {
        // 在未指定语言的情况下获得全部<caption>名
        public static List<string> GetAllNames(XmlNode parent)
        {
            List<string> results = new List<string>();

            XmlNodeList nodes = parent.SelectNodes("caption");
            for (int i = 0; i < nodes.Count; i++)
            {
                results.Add(nodes[i].InnerText);
            }

            return results;
        }

        static string m_strKernelBrowseFomatsXml =
    "<formats> "
    + "<format name='brief' type='kernel'>"
    + "    <caption lang='zh-cn'>简略</caption>"      // 浏览
    + "    <caption lang='en'>Brief</caption>"
    + "</format>"
    + "<format name='MARC' type='kernel'>"
    + "    <caption lang='zh-cn'>MARC</caption>"
    + "    <caption lang='en'>MARC</caption>"
    + "</format>"
    + "</formats>";

        // 2011/1/2
        // 是否为内置格式名
        // paramters:
        //      strNeutralName  语言中立的名字。例如 browse / MARC。大小写不敏感
        public static bool IsKernelFormatName(string strName,
            string strNeutralName)
        {
            if (strName.ToLower() == strNeutralName.ToLower())
                return true;

            // 先从内置的格式里面找
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(m_strKernelBrowseFomatsXml);

            XmlNodeList format_nodes = dom.DocumentElement.SelectNodes("format");
            for (int j = 0; j < format_nodes.Count; j++)
            {
                XmlNode node = format_nodes[j];

                List<string> captions = GetAllNames(node);
                if (captions.IndexOf(strName) == -1)
                    continue;

                if (DomUtil.GetAttr(node, "name").ToLower() == strNeutralName.ToLower())
                    return true;
            }

            return false;
        }

        // 2011/1/2
        // 获得特定语言的格式名
        // 包括内置的格式
        public string GetBrowseFormatName(string strName,
            string strLang)
        {
            // 先从内置的格式里面找
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(m_strKernelBrowseFomatsXml);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("format");
            string strFormat = GetBrowseFormatName(
                nodes,
                strName,
                strLang);
            if (String.IsNullOrEmpty(strFormat) == false)
                return strFormat;

            XmlNode root = this.OpacCfgDom.DocumentElement.SelectSingleNode("browseformats");
            if (root == null)
            {
                // string strError = "<browseformats>元素尚未配置...";
                // TODO: 抛出异常?
                return null;
            }

            // 然后从用户定义的格式里面找
            nodes = root.SelectNodes("database/format");
            return GetBrowseFormatName(
                nodes,
                strName,
                strLang);
        }

        // 2011/1/2
        static string GetBrowseFormatName(
            XmlNodeList format_nodes,
            string strName,
            string strLang)
        {

            for (int j = 0; j < format_nodes.Count; j++)
            {
                XmlNode node = format_nodes[j];

                List<string> captions = GetAllNames(node);
                if (captions.IndexOf(strName) == -1)
                    continue;

                string strFormatName = DomUtil.GetCaption(strLang, node);
                if (String.IsNullOrEmpty(strFormatName) == false)
                    return strFormatName;
            }

            return null;    // not found
        }
        
        // 获得一些数据库的全部浏览格式配置信息
        // parameters:
        //      dbnames 要列出哪些数据库的浏览格式？如果==null, 则表示列出全部可能的格式名
        // return:
        //      -1  出错
        //      >=0 formatname个数
        public int GetBrowseFormatNames(
            string strLang,
            List<string> dbnames,
            out List<string> formatnames,
            out string strError)
        {
            strError = "";
            formatnames = new List<string>();

            XmlNode root = this.OpacCfgDom.DocumentElement.SelectSingleNode("browseformats");
            if (root == null)
            {
                strError = "<browseformats>元素尚未配置...";
                return -1;
            }

            XmlNodeList dbnodes = root.SelectNodes("database");
            for (int i = 0; i < dbnodes.Count; i++)
            {
                XmlNode nodeDatabase = dbnodes[i];

                string strDbName = DomUtil.GetAttr(nodeDatabase, "name");

                // dbnames如果==null, 则表示列出全部可能的格式名
                if (dbnames != null)
                {
                    if (dbnames.IndexOf(strDbName) == -1)
                        continue;
                }

                XmlNodeList nodes = nodeDatabase.SelectNodes("format");
                for (int j = 0; j < nodes.Count; j++)
                {
                    XmlNode node = nodes[j];

                    string strFormatName = DomUtil.GetCaption(strLang, node);
                    if (String.IsNullOrEmpty(strFormatName) == true)
                        strFormatName = DomUtil.GetAttr(node, "name");

                    /*
                    if (String.IsNullOrEmpty(strFormatName) == true)
                    {
                        strError = "格式配置片断 '" + node.OuterXml + "' 格式不正确...";
                        return -1;
                    }*/

                    if (formatnames.IndexOf(strFormatName) == -1)
                        formatnames.Add(strFormatName);
                }

            }

            bool bMarcVisible = true;
            XmlNode nodeMarcControl = this.WebUiDom.DocumentElement.SelectSingleNode("marcControl");
            if (nodeMarcControl != null)
            {
                bMarcVisible = DomUtil.GetBooleanParam(nodeMarcControl,
                    "visible",
                    true);
            }

            // 2011/1/2
            // 从内置的格式里面找
            // TODO: 对一些根本不是MARC格式的数据库，排除"MARC"格式名
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(m_strKernelBrowseFomatsXml);

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("format");
                for (int j = 0; j < nodes.Count; j++)
                {
                    XmlNode node = nodes[j];

                    string strFormatName = DomUtil.GetCaption(strLang, node);
                    if (String.IsNullOrEmpty(strFormatName) == true)
                        strFormatName = DomUtil.GetAttr(node, "name");

                    // 2012/11/29
                    if (bMarcVisible == false && strFormatName == "MARC")
                        continue;

                    if (formatnames.IndexOf(strFormatName) == -1)
                        formatnames.Add(strFormatName);
                }
            }

            return formatnames.Count;
        }

        // 获得一个数据库的全部浏览格式配置信息
        // return:
        //      -1  出错
        //      0   没有配置。具体原因在strError中
        //      >=1 format个数
        public int GetBrowseFormats(string strDbName,
            out List<BrowseFormat> formats,
            out string strError)
        {
            strError = "";
            formats = null;

            XmlNode root = this.OpacCfgDom.DocumentElement.SelectSingleNode("browseformats");
            if (root == null)
            {
                strError = "<browseformats>元素尚未配置...";
                return -1;
            }

            XmlNode node = root.SelectSingleNode("database[@name='" + strDbName + "']");
            if (node == null)
            {
                strError = "针对数据库 '" + strDbName + "' 没有在<browseformats>下配置<database>参数";
                return 0;
            }

            formats = new List<BrowseFormat>();

            XmlNodeList nodes = node.SelectNodes("format");
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                BrowseFormat format = new BrowseFormat();
                format.Name = DomUtil.GetAttr(node, "name");
                format.Type = DomUtil.GetAttr(node, "type");
                format.Style = DomUtil.GetAttr(node, "style");
                format.ScriptFileName = DomUtil.GetAttr(node, "scriptfile");
                formats.Add(format);
            }

            if (nodes.Count == 0)
            {
                strError = "数据库 '" + strDbName + "' 在<browseformats>下的<database>元素下，一个<format>元素也未配置。";
            }

            return nodes.Count;
        }

        // 获得一个数据库的一个浏览格式配置信息
        // parameters:
        //      strDbName   "zh"语言的数据库名。也就是<browseformats>下<database>元素的name属性内的数据库名。
        //      strFormatName   界面上选定的格式名。注意，不一定是正好属于this.Lang语言的
        // return:
        //      0   没有配置
        //      1   成功
        public int GetBrowseFormat(string strDbName,
            string strFormatName,
            out BrowseFormat format,
            out string strError)
        {
            strError = "";
            format = null;

            // 先从全部<format>元素下面的全部<caption>中找
            XmlNode nodeDatabase = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                "browseformats/database[@name='" + strDbName + "']");

            if (nodeDatabase == null)
            {
                strError = "数据库名 '" + strDbName + "' 在<browseformats>元素下没有找到匹配的<database>元素";
                return -1;
            }

            XmlNode nodeFormat = null;

            XmlNodeList nodes = nodeDatabase.SelectNodes("format");
            for (int j = 0; j < nodes.Count; j++)
            {
                XmlNode node = nodes[j];

                List<string> captions = GetAllNames(node);
                if (captions.IndexOf(strFormatName) != -1)
                {
                    nodeFormat = node;
                    break;
                }
            }

            // 再从<format>元素的name属性中找
            if (nodeFormat == null)
            {
                nodeFormat = nodeDatabase.SelectSingleNode(
                    "format[@name='" + strFormatName + "']");
                if (nodeFormat == null)
                {
                    return 0;
                }
            }

            format = new BrowseFormat();
            format.Name = DomUtil.GetAttr(nodeFormat, "name");
            format.Type = DomUtil.GetAttr(nodeFormat, "type");
            format.Style = DomUtil.GetAttr(nodeFormat, "style");
            format.ScriptFileName = DomUtil.GetAttr(nodeFormat, "scriptfile");

            return 1;
        }



    }

    public class BrowseFormat
    {
        public string Name = "";
        public string ScriptFileName = "";
        public string Type = "";
        public string Style = "";   // original -- 表示完全自行控制每行内容 %checkbox%宏可以创建序号checkbox
                                    // item,comment -- 表示包括册记录和评注控件部分
        // 2013/1/21


        // 将脚本文件名正规化
        // 因为在定义脚本文件的时候, 有一个当前库名环境,
        // 如果定义为 ./cfgs/filename 表示在当前库下的cfgs目录下,
        // 而如果定义为 /cfgs/filename 则表示在同服务器的根下
        public static string CanonicalizeScriptFileName(string strDbName,
            string strScriptFileNameParam)
        {
            int nRet = 0;
            nRet = strScriptFileNameParam.IndexOf("./");
            if (nRet != -1)
            {
                // 认为是当前库下
                return strDbName + strScriptFileNameParam.Substring(1);
            }

            nRet = strScriptFileNameParam.IndexOf("/");
            if (nRet != -1)
            {
                // 认为从根开始
                return strScriptFileNameParam.Substring(1);
            }

            return strScriptFileNameParam;  // 保持原样
        }
    }

}
