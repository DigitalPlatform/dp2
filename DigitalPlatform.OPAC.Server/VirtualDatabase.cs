using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using System.Diagnostics;

namespace DigitalPlatform.OPAC.Server
{

    /// <summary>
    /// 一个虚拟数据库
    /// </summary>
    public class VirtualDatabase
    {
        internal XmlNode nodeDatabase = null;

        // 2020/11/17
        // 是否显示在数据库名列表中
        // 缺省为 true。
        public bool Visible
        {
            get
            {
                if (nodeDatabase == null)
                    return true;

                // 获得布尔型的属性参数值
                // return:
                //      -1  出错。但是bValue中已经有了bDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                int nRet = DomUtil.GetBooleanParam(nodeDatabase,
                    "visible",
                    true,
                    out bool bValue,
                    out string strError);

                return bValue;
            }
        }

        // 是否不在<all>之列
        // 缺省为false。
        // 本参数用来控制不希望出现在<all>范围内的数据库，例如“用户”库
        public bool NotInAll
        {
            get
            {
                if (nodeDatabase == null)
                    return false;

                bool bValue = false;
                string strError = "";
                // 获得布尔型的属性参数值
                // return:
                //      -1  出错。但是bValue中已经有了bDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                int nRet = DomUtil.GetBooleanParam(nodeDatabase,
                    "notInAll",
                    false,
                    out bValue,
                    out strError);

                return bValue;
            }
        }

        public bool IsVirtual
        {
            get
            {
                if (nodeDatabase == null)
                    return false;

                if (nodeDatabase.Name == "database")
                    return false;

                return true;
            }
        }

        // 获得特定语言下的From名称列表
        public List<string> GetFroms(string strLang)
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = this.nodeDatabase.SelectNodes("from");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strName = DomUtil.GetCaption(strLang, nodes[i]);
                if (strName == null)
                {   // 如果下面根本没有定义<caption>元素，则采用<from>元素的name属性值
                    strName = DomUtil.GetAttr(nodes[i], "name");
                    if (String.IsNullOrEmpty(strName) == true)
                        continue;   // 实在没有，只好舍弃
                }
                results.Add(strName);
            }

            return results;
        }

        // 2012/10/25
        public List<DbFromInfo> GetFromInfos(string strLang)
        {
            List<DbFromInfo> results = new List<DbFromInfo>();
            XmlNodeList nodes = this.nodeDatabase.SelectNodes("from");
            foreach (XmlNode node in nodes)
            {
                string strCaption = DomUtil.GetCaption(strLang, node);
                if (strCaption == null)
                {   // 如果下面根本没有定义<caption>元素，则采用<from>元素的name属性值
                    strCaption = DomUtil.GetAttr(node, "name");
                    if (String.IsNullOrEmpty(strCaption) == true)
                        continue;   // 实在没有，只好舍弃
                }

                DbFromInfo info = new DbFromInfo();
                info.Caption = strCaption;
                info.Style = DomUtil.GetAttr(node, "style");
                results.Add(info);
            }

            return results;
        }

        // 获得特定语言下的数据库名
        public string GetName(string strLang)
        {
            // 从一个元素的下级<caption>元素中, 提取语言符合的文字值
            string strCaption = DomUtil.GetCaption(strLang,
                this.nodeDatabase);

            if (String.IsNullOrEmpty(strCaption) == true)
            {
                if (IsVirtual == false)
                    return DomUtil.GetAttr(this.nodeDatabase, "name");
            }

            return strCaption;
        }

        // 在未指定语言的情况下获得全部数据库名
        // 2009/6/17
        public List<string> GetAllNames()
        {
            List<string> results = new List<string>();

            XmlNodeList nodes = this.nodeDatabase.SelectNodes("caption");
            for (int i = 0; i < nodes.Count; i++)
            {
                results.Add(nodes[i].InnerText);
            }

            return results;
        }

#if NO
        // 初始化From的一些属性, 以便将来运行起来快速方便
        // 在<from>元素下要插入若干<database>元素, 这是该From可用的数据库的列表
        // 这些信息属于软件初始化的范畴, 避免人工去配置
        public int InitialFromProperty(
            //            ResInfoItem[] root_dir_results,
            Hashtable db_dir_results,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (nodeDatabase == null)
            {
                strError = "nodeDatabase尚未设置值";
                return -1;
            }

            if (this.IsVirtual != true)
            {
                strError = "该函数只适用于<virtualDatabase>元素的初始化";
                return -1;
            }

            XmlNodeList dbnodes = this.nodeDatabase.SelectNodes("database");

            // 列出所有<from>元素
            XmlNodeList fromnodes = this.nodeDatabase.SelectNodes("from");
            for (int i = 0; i < fromnodes.Count; i++)
            {
                string strFromName = DomUtil.GetAttr(fromnodes[i], "name");

                // 删除原有子节点中<caption>以外的元素<database>
                RemoveDatabaseChildren(fromnodes[i]);

                // 从可用的数据库列表中, 挑出含有其下有from风格符合的
                for (int j = 0; j < dbnodes.Count; j++)
                {
                    string strDbName = DomUtil.GetAttr(dbnodes[j], "name");
                    string strStyle = DomUtil.GetAttr(dbnodes[j], "style");

                    nRet = MatchFromStyle(strDbName,
                        strStyle,
                        db_dir_results);
                    if (nRet == 0)
                        continue;

                    // 在<from>元素下加入一个<database>元素
                    XmlNode newnode = fromnodes[i].OwnerDocument.CreateElement("database");
                    fromnodes[i].AppendChild(newnode);
                    DomUtil.SetAttr(newnode, "name", strDbName);

                }

            }

            return 0;
        }

        // 初始化数据库和From的一些属性, 以便将来运行起来快速方便
        // 在<database>元素下要插入若干<from>元素
        // 这些信息属于软件初始化的范畴, 避免人工去配置
        public int InitialAllProperty(
            ResInfoItem[] root_dir_results,
            Hashtable db_dir_results,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (nodeDatabase == null)
            {
                strError = "nodeDatabase尚未设置值";
                return -1;
            }

            if (this.IsVirtual != false)
            {
                strError = "该函数只适用于<database>元素的初始化";
                return -1;
            }

            string strDbName = DomUtil.GetAttr(nodeDatabase, "name");

            RemoveChildren(nodeDatabase);

            ResInfoItem dbitem = KernelDbInfo.GetDbItem(
                root_dir_results,
                strDbName);
            if (dbitem == null)
            {
                strError = "根目录下没有找到名字为 '" + strDbName + "' 的数据库目录事项";
                return -1;
            }

            // 在下级加入<caption>元素
            for (int i = 0; i < dbitem.Names.Length; i++)
            {
                string strText = dbitem.Names[i];
                nRet = strText.IndexOf(":");
                if (nRet == -1)
                {
                    strError = "names字符串 '" + strText + "' 格式不正确。";
                    return -1;
                }
                string strLang = strText.Substring(0, nRet);
                string strName = strText.Substring(nRet + 1);

                XmlNode newnode = nodeDatabase.OwnerDocument.CreateElement("caption");
                newnode = nodeDatabase.AppendChild(newnode);
                DomUtil.SetAttr(newnode, "lang", strLang);
                DomUtil.SetNodeText(newnode, strName);

            }

            // 
            ResInfoItem[] fromitems = (ResInfoItem[])db_dir_results[strDbName];
            if (fromitems == null)
            {
                strError = "db_dir_results中没有找到关于 '" + strDbName + "' 的下级目录事项";
                return -1;
            }

            for (int i = 0; i < fromitems.Length; i++)
            {
                ResInfoItem item = fromitems[i];
                if (item.Type != ResTree.RESTYPE_FROM)
                    continue;

                // 插入<from>元素
                XmlNode fromnode = nodeDatabase.OwnerDocument.CreateElement("from");
                fromnode = nodeDatabase.AppendChild(fromnode);
                DomUtil.SetAttr(fromnode, "name", item.Name);    // 当前工作语言下的名字

                if (item.Names == null)
                    continue;

                // 插入caption
                for (int j = 0; j < item.Names.Length; j++)
                {
                    string strText = item.Names[j];
                    nRet = strText.IndexOf(":");
                    if (nRet == -1)
                    {
                        strError = "names字符串 '" + strText + "' 格式不正确。";
                        return -1;
                    }

                    string strLang = strText.Substring(0, nRet);
                    string strName = strText.Substring(nRet + 1);

                    XmlNode newnode = fromnode.OwnerDocument.CreateElement("caption");
                    newnode = fromnode.AppendChild(newnode);
                    DomUtil.SetAttr(newnode, "lang", strLang);
                    DomUtil.SetNodeText(newnode, strName);

                }

            }

            return 0;
        }
#endif
        /*
        // 从目录事项中获得指定名字的事项
        static ResInfoItem GetDbItem(
            ResInfoItem [] root_dir_results,
            string strDbName)
        {
            for (int i = 0; i < root_dir_results.Length; i++)
            {
                ResInfoItem info = root_dir_results[i];

                if (info.Type != ResTree.RESTYPE_DB)
                    continue;

                if (info.Name == strDbName)
                    return info;

            }

            return null;
        }
         * */

        // 删除下级的全部元素
        static void RemoveChildren(XmlNode parent)
        {
            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                XmlNode node = parent.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                parent.RemoveChild(node);
                i--;
            }
        }


        static void RemoveDatabaseChildren(XmlNode parent)
        {
            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                XmlNode node = parent.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                if (node.Name == "database")
                {
                    parent.RemoveChild(node);
                    i--;
                }

            }
        }

#if NO
        static int MatchFromStyle(string strDbName,
            string strStyle,
            Hashtable db_dir_results)
        {
            ResInfoItem[] infos = (ResInfoItem[])db_dir_results[strDbName];
            if (infos == null)
                return 0;
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].Type != ResTree.RESTYPE_FROM)
                    continue;
                if (StringUtil.IsInList(strStyle, infos[i].TypeString) == true)
                    return 1;
            }

            return 0;
        }
#endif

        // 把逗号间隔的style列表字符串拆分，然后去掉 _ 开头的那些值
        public static List<string> MakeStyleList(string strText)
        {
            List<string> list = StringUtil.SplitList(strText);
            // 去掉前缀为 _ 的值
            for (int i = 0; i < list.Count; i++)
            {
                if (StringUtil.HasHead(list[i], "_") == true)
                {
                    list.RemoveAt(i);
                    i--;
                }
            }

            return list;
        }

        // 根据虚拟的From名获得真实的From名。这个From是工作语言下的名字
        // 如果匹配多个From名，将在字符串中以逗号分隔 
        // parameters:
        //      strVirtualFromName  虚拟库的from名列表。或者style列表
        public string GetRealFromName(
            string strRealDbName,
            string strVirtualFromName,
            bool bOutputDebugInfo,
            out string strDebugInfo)
        {
            List<string> styles = new List<string>();
            strDebugInfo = "";

            // 2012/11/20
            // 得到纯粹的style列表。不包含那些 _ 打头的style
            string strPureStyles = StringUtil.MakePathList(MakeStyleList(strVirtualFromName));

            if (bOutputDebugInfo == true)
            {
                strDebugInfo += " begin GetRealFromName()\r\nstrRealDbName='" + strRealDbName + "' strVirtualFromName='" + strVirtualFromName + "'\r\n";
                strDebugInfo += "this.nodeDatabase: " + this.nodeDatabase.OuterXml + "\r\n";
            }

            if (String.IsNullOrEmpty(strVirtualFromName) == true
                || strVirtualFromName == "<全部>"
                || strVirtualFromName.ToLower() == "<all>")
            {
                XmlNodeList nodes = this.nodeDatabase.SelectNodes("from");
                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "是 全部， nodes.Count=" + nodes.Count.ToString() + "\r\n";
                }
                for (int i = 0; i < nodes.Count; i++)
                {
                    string strStyle = DomUtil.GetAttr(nodes[i], "style");
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "加入styles '" + strStyle + "'\r\n";
                    }
                    styles.Add(strStyle);
                }
            }
            else
            {
                XmlNode node = null;
                XmlNodeList nodes = this.nodeDatabase.SelectNodes("from/caption");
                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "选定全部 from/caption 元素， nodes.Count=" + nodes.Count.ToString() + "\r\n";
                }
                for (int i = 0; i < nodes.Count; i++)
                {
                    node = nodes[i];
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "node.InnerText.Trim()='" + node.InnerText.Trim() + "'\r\n";
                    }
                    if (strVirtualFromName == node.InnerText.Trim())
                    {
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "匹配上了\r\n";
                        }
                        string strStyle = DomUtil.GetAttr(node.ParentNode, "style");
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "取匹配节点的ParentNode的style属性加入styles数组。ParentNode.OuterXml=" + node.ParentNode.OuterXml + "\r\n";
                        }
                        styles.Add(strStyle);
                        goto FOUND;
                    }
                }

                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "没有匹配的Caption字符串\r\n继续试探匹配style字符串";
                }

                nodes = this.nodeDatabase.SelectNodes("from");
                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "选定全部 from 元素， nodes.Count=" + nodes.Count.ToString() + "\r\n";
                }
                foreach (XmlNode temp in nodes)
                {
                    string strStyles = DomUtil.GetAttr(temp, "style");
                    if (StringUtil.IsInList(strPureStyles, strStyles) == true)
                    {
                        styles.Add(strPureStyles);
                        goto FOUND;
                    }
                }

                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "没有匹配的\r\nend GetRealFromName(), 返回null";
                }
                return null;    // not found
            }

        FOUND:

            List<string> results = new List<string>();

            {
                XmlNode root = this.nodeDatabase.ParentNode;
                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "this.nodeDatabase.ParentNode.OuterXml=" + this.nodeDatabase.ParentNode.OuterXml + "\r\n";
                }
                XmlNode nodeDatabase = root.SelectSingleNode("database[@name='" + strRealDbName + "']");
                if (nodeDatabase == null)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "找不到name为'" + strRealDbName + "'的<database>元素\r\n\r\nend GetRealFromName(), 返回null";
                    }
                    return null;
                }

                XmlNodeList from_nodes = nodeDatabase.SelectNodes("from");
                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "选定name为'" + strRealDbName + "'的<database>元素下级的全部<From>元素， nodes.Count=" + from_nodes.Count.ToString() + "\r\n";
                }

                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "遍历styles。\r\n";
                }
                for (int i = 0; i < styles.Count; i++)
                {
                    string strStyle = styles[i];
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "strStyle='" + strStyle + "'\r\n";
                    }

                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "遍历<from>元素。\r\n";
                    }
                    for (int j = 0; j < from_nodes.Count; j++)
                    {
                        XmlNode node = from_nodes[j];

                        string strStyles = DomUtil.GetAttr(node, "style");
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "<from>元素的style属性值为'" + strStyles + "'\r\n";
                        }
                        if (StringUtil.IsInList(strStyle, strStyles) == true)
                        {
                            if (bOutputDebugInfo == true)
                            {
                                strDebugInfo += "strStyle'" + strStyle + "' 包含于 刚取出的 '" + strStyles + "' 之中，因此将name属性值 '" + DomUtil.GetAttr(node, "name") + "' 加入到结果数组\r\n";
                            }
                            results.Add(DomUtil.GetAttr(node, "name"));
                        }
                        else
                        {
                            if (bOutputDebugInfo == true)
                            {
                                strDebugInfo += "strStyle'" + strStyle + "' 不包含于 刚取出的 '" + strStyles + "' 之中，因此被忽略\r\n";
                            }
                        }
                    }
                }
            }

            if (results.Count == 0)
            {
                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "没有发现任何匹配的style属性值\r\nend GetRealFromName(), 返回null";
                }
                return null;    // style没有发现匹配的
            }

            string[] list = new string[results.Count];
            results.CopyTo(list);
            string strResult = String.Join(",", list);
            if (bOutputDebugInfo == true)
            {
                strDebugInfo += "结果数量 " + results.Count + ", '" + strResult + "'\r\nend GetRealFromName()";
            }
            return strResult;
        }

        // 获得下属的所有真实数据库名
        public List<string> GetRealDbNames()
        {
            List<string> results = new List<string>();

            XmlNodeList nodes = this.nodeDatabase.SelectNodes("descendant-or-self::database");
            for (int i = 0; i < nodes.Count; i++)
            {
                results.Add(DomUtil.GetAttr(nodes[i], "name"));
            }

            StringUtil.RemoveDupNoSort(ref results);    // 2011/9/16
            return results;
        }

        // 从特定的数据库中, 匹配出满足特定风格列表的from列表
        // parameters:
        //      strFromStyle    from style的列表, 以逗号分割。
        //                      如果为空，表示全部途径(2007/9/13)
        // return:
        //      null    没有找到
        //      以逗号分割的from名列表
        public string BuildCaptionListByStyleList(
            string strFromStyles,
            string strLang)
        {
            if (String.IsNullOrEmpty(strFromStyles) == true
                || strFromStyles == "<全部>" || strFromStyles.ToLower() == "<all>")
            {
                return "<all>";
                // strFromStyles = "<all>";
            }

            XmlNodeList nodes = null;

            string strResult = "";

            // 拆分出单独的style字符串
            string[] styles = strFromStyles.Split(new char[] { ',' });

            for (int i = 0; i < styles.Length; i++)
            {
                string strStyle = styles[i].Trim();
                if (String.IsNullOrEmpty(strStyle) == true)
                    continue;

                // 忽略 _time/_freetime,_rfc1123time/_utime等表示检索特性的style
                if (StringUtil.HasHead(strStyle, "_") == true
                    && StringUtil.HasHead(strStyle, "__") == false)
                    continue;

                if (nodes == null)  // 滞后获取
                    nodes = this.nodeDatabase.SelectNodes("from");

                foreach (XmlNode node in nodes)
                {
                    string strStyles = DomUtil.GetAttr(node, "style");
                    if (StringUtil.IsInList(strStyle, strStyles) == true
    || strStyle == "<all>") // 注：后来发现内核本来就支持<all>的from后，这里就没有必要了，但是代码仍保留
                    {
                        // 从一个元素的下级<caption>元素中, 提取语言符合的文字值
                        string strValue = DomUtil.GetCaption(strLang,
                            node);
                        if (strValue == null)
                        {
                            // 只好用中立语言的名字
                            string strName = DomUtil.GetAttr(node, "name");
                            if (string.IsNullOrEmpty(strName) == false)
                                return strName;

                            throw new Exception("数据库 '" + this.GetName(strLang) + "' 中没有找到style为 " + strStyles + " 的From事项的任何Caption");
                        }

                        // 全部路径情况下，要不包含"__id"途径
                        if (strStyle == "<all>"
                            && strValue == "__id")
                            continue;

                        if (strResult != "")
                            strResult += ",";

                        strResult += strValue;
                    }
                }
            }

            return strResult;
        }

    }

    public class DbFromInfo
    {
        public string Caption = ""; // 字面标签
        public string Style = "";   // 角色
    }

#if NO
    public class Caption
    {
        public string Lang = "";
        public string Value = "";

        public Caption(string strLang,
            string strValue)
        {
            this.Lang = strLang;
            this.Value = strValue;
        }
    }
#endif

    /// <summary>
    /// 虚拟数据库的集合
    /// </summary>
    public class VirtualDatabaseCollection : List<VirtualDatabase>
    {
        // public string ServerUrl = "";
        public string Lang = "zh";

        // public ResInfoItem[] root_dir_results = null;  // 根目录信息
        // public Hashtable db_dir_results = null;    // 二级目录信息

        // 2009/6/17 changed
        public VirtualDatabase this[string strDbName]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    VirtualDatabase vdb = this[i];

                    // 无论是否虚拟库，都要看所有<caption>
                    {
                        XmlNodeList nodes = vdb.nodeDatabase.SelectNodes("caption");
                        for (int j = 0; j < nodes.Count; j++)
                        {
                            if (nodes[j].InnerText == strDbName)
                                return vdb;
                        }
                    }

                    // 如果不是虚拟库，多判断一次
                    if (vdb.IsVirtual == false)
                    {
                        if (vdb.GetName(null) == strDbName)
                            return vdb;
                    }

                }

                return null;
            }
        }

#if NO
        // 获得一个普通数据库的定义(包括数据库名captions，froms name captions)
        // 格式为
        /*
        <database>
            <caption lang="zh-cn">中文图书</caption>
            <caption lang="en">Chinese book</caption>
            <from style="title">
                <caption lang="zh-cn">题名</caption>
                <caption lang="en">Title</caption>
            </from>
            ...
            <from name="__id" />
        </database>         * */
        // return:
        //      -1  error
        //      0   not found such database
        //      1   found and succeed
        public int GetDatabaseDef(
            string strDbName,
            out string strDef,
            out string strError)
        {
            strError = "";
            strDef = "";

            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<database />");


            {
                ResInfoItem dbitem = KernelDbInfo.GetDbItem(
                    this.root_dir_results,
                    strDbName);
                if (dbitem == null)
                {
                    strError = "根目录下没有找到名字为 '" + strDbName + "' 的数据库目录事项";
                    return 0;
                }

                // 在根下加入<caption>元素
                for (int i = 0; i < dbitem.Names.Length; i++)
                {
                    string strText = dbitem.Names[i];
                    nRet = strText.IndexOf(":");
                    if (nRet == -1)
                    {
                        strError = "names字符串 '" + strText + "' 格式不正确。";
                        return -1;
                    }
                    string strLang = strText.Substring(0, nRet);
                    string strName = strText.Substring(nRet + 1);

                    XmlNode newnode = dom.CreateElement("caption");
                    dom.DocumentElement.AppendChild(newnode);
                    DomUtil.SetAttr(newnode, "lang", strLang);
                    DomUtil.SetNodeText(newnode, strName);
                }
            }

            // 
            ResInfoItem[] fromitems = (ResInfoItem[])this.db_dir_results[strDbName];
            if (fromitems == null)
            {
                strError = "db_dir_results中没有找到关于 '" + strDbName + "' 的下级目录事项";
                return 0;
            }

            for (int i = 0; i < fromitems.Length; i++)
            {
                ResInfoItem item = fromitems[i];
                if (item.Type != ResTree.RESTYPE_FROM)
                    continue;

                // 插入<from>元素
                XmlNode fromnode = dom.CreateElement("from");
                dom.DocumentElement.AppendChild(fromnode);
                DomUtil.SetAttr(fromnode, "style", item.TypeString);    // style

                if (item.Names == null)
                    continue;

                // 插入caption
                for (int j = 0; j < item.Names.Length; j++)
                {
                    string strText = item.Names[j];
                    nRet = strText.IndexOf(":");
                    if (nRet == -1)
                    {
                        strError = "names字符串 '" + strText + "' 格式不正确。";
                        return -1;
                    }

                    string strLang = strText.Substring(0, nRet);
                    string strName = strText.Substring(nRet + 1);

                    XmlNode newnode = dom.CreateElement("caption");
                    fromnode.AppendChild(newnode);
                    DomUtil.SetAttr(newnode, "lang", strLang);
                    DomUtil.SetNodeText(newnode, strName);
                }

            }

            strDef = dom.OuterXml;

            return 1;
        }

#endif

        /// <summary>
        /// 构造函数
        /// 根据XML配置文件, 从服务器获取目录信息, 初始化数据结构
        /// </summary>
        public int Initial(XmlNode root,
            out string strError)
        {
            strError = "";

            // Debug.Assert(false, "");

            // 列出所有虚拟库XML节点
            // XmlNodeList virtualnodes = root.SelectNodes("virtualDatabase");
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode node = root.ChildNodes[i];

                if (node.NodeType != XmlNodeType.Element)
                    continue;

                // 2017/12/12
                if (DomUtil.GetBooleanParam(node, "hide", false) == true)
                    continue;

                if (node.Name == "virtualDatabase")
                {
                    // 构造虚拟数据库对象
                    VirtualDatabase vdb = new VirtualDatabase();
                    vdb.nodeDatabase = node;

                    this.Add(vdb);
                    continue;
                }

                if (node.Name == "database")    // 普通库
                {
                    // 构造普通数据库对象
                    VirtualDatabase vdb = new VirtualDatabase();
                    vdb.nodeDatabase = node;

                    this.Add(vdb);
                    continue;
                }
            }

            return 0;
        }

        // 获得全部可用的From列表
        public int GetFroms(
            string strLang,
            out DbFromInfo[] infos,
            out string strError)
        {
            strError = "";
            infos = null;

            // 把所有库的from累加起来
            List<DbFromInfo> all = new List<DbFromInfo>();

            foreach (VirtualDatabase v in this)
            {
                List<DbFromInfo> current = v.GetFromInfos(strLang);
                all.AddRange(current);
            }

            // 根据 Caption 值去重。以前做法
            // RemoveDupByCaption(ref all);

            // 根据 Style 值去重。2020/6/17 做法
            RemoveDupByStyle(ref all);

            int nIndexOfID = -1;    // __id途径所在的下标
            for (int i = 0; i < all.Count; i++)
            {
                DbFromInfo from = all[i];

                if (from.Caption == "__id" || from.Style == "recid")
                    nIndexOfID = i;
            }

            // 如果曾经出现过 __id caption
            if (nIndexOfID != -1)
            {
                DbFromInfo temp = all[nIndexOfID];
                all.RemoveAt(nIndexOfID);
                all.Add(temp);
            }

            infos = new DbFromInfo[all.Count];
            all.CopyTo(infos);
            return 0;
        }

        // 根据caption去重(按照特定语种caption来去重)
        public static void RemoveDupByCaption(ref List<DbFromInfo> target)
        {
            for (int i = 0; i < target.Count; i++)
            {
                DbFromInfo from1 = target[i];

                string strCaption1 = from1.Caption;
                // 把caption(特定语种)为空的事项丢弃
                if (string.IsNullOrEmpty(strCaption1) == true)
                {
                    target.RemoveAt(i);
                    i--;
                    continue;
                }

                for (int j = i + 1; j < target.Count; j++)
                {
                    DbFromInfo from2 = target[j];
                    string strCaption2 = from2.Caption;

                    if (strCaption1 == strCaption2)
                    {
                        target.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        // 2020/6/17
        // 根据 Style 去重
        public static void RemoveDupByStyle(ref List<DbFromInfo> target)
        {
            for (int i = 0; i < target.Count; i++)
            {
                DbFromInfo from1 = target[i];

                string strStyle1 = from1.Style;
                // 把caption(特定语种)为空的事项丢弃
                if (string.IsNullOrEmpty(strStyle1) == true)
                {
                    target.RemoveAt(i);
                    i--;
                    continue;
                }

                for (int j = i + 1; j < target.Count; j++)
                {
                    DbFromInfo from2 = target[j];
                    string strStyle2 = from2.Style;

                    if (strStyle1 == strStyle2)
                    {
                        target.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

#if NO
        // 判断两个caption和集中是否有共同的值
        static bool IsSame(List<Caption> captions1, List<Caption> captions2)
        {
            foreach (Caption caption1 in captions1)
            {
                foreach (Caption caption2 in captions2)
                {
                    if (caption1.Value == caption2.Value)
                        return true;
                }
            }

            return false;
        }
#endif

#if NO
        // 列出目录信息
        // 列出2级。第二级在Hashtable中
        int GetDirInfo(RmsChannelCollection Channels,
            string strServerUrl,
            out ResInfoItem[] root_dir_results,
            out Hashtable db_dir_results,
            out string strError)
        {
            root_dir_results = null;
            db_dir_results = null;

            RmsChannel channel = Channels.GetChannel(this.ServerUrl);

            // 列出所有数据库
            root_dir_results = null;

            long lRet = channel.DoDir("",
                this.Lang,
                "alllang",
                out root_dir_results,
                out strError);
            if (lRet == -1)
                return -1;

            db_dir_results = new Hashtable();

            for (int i = 0; i < root_dir_results.Length; i++)
            {
                ResInfoItem info = root_dir_results[i];
                if (info.Type != ResTree.RESTYPE_DB)
                    continue;

                ResInfoItem[] db_dir_result = null;

                lRet = channel.DoDir(info.Name,
                       this.Lang,
                       "alllang",
                       out db_dir_result,
                       out strError);
                if (lRet == -1)
                    return -1;

                db_dir_results[info.Name] = db_dir_result;
            }


            return 0;
        }
#endif


    }
}

