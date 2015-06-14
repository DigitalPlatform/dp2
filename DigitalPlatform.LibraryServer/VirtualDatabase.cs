using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{

    /// <summary>
    /// 一个虚拟数据库
    /// </summary>
    public class VirtualDatabase
    {
        internal XmlNode nodeDatabase = null;

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
                        // 获得整数型的属性参数值
        // return:
        //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
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



        // 获得特定语言下的数据库名
        public string GetName(string strLang)
        {

            /*

            XmlNode node = this.nodeDatabase.SelectSingleNode("caption[@lang='" + strLang + "']");
            if (node == null)
            {
                string strLangLeft = "";
                string strLangRight = "";

                SplitLang(strLang,
                   out strLangLeft,
                   out strLangRight);

                // 所有<caption>元素
                XmlNodeList nodes = this.nodeDatabase.SelectNodes("caption");

                for (int i = 0; i < nodes.Count; i++)
                {
                    string strThisLang = DomUtil.GetAttr(nodes[i], "lang");

                    if (strThisLang == strLangLeft)
                        return nodes[i].InnerText;
                }

                node = this.nodeDatabase.SelectSingleNode("caption");
                if (node != null)
                    return node.InnerText;
                return null;    // not found
            }

            return node.InnerText;
             */

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
            for(int i=0;i<nodes.Count;i++)
            {
                results.Add(nodes[i].InnerText);
            }

            return results;
        }

        /*
        // 列出可用的From Style集合
        public List<string> GetStyles()
        {
            List<string> results = new List<string>();

            XmlNodeList nodes = nodeDatabase.SelectNodes("from");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strStyle = DomUtil.GetAttr(nodes[i], "style");
                results.Add(strStyle);
            }

            return results;
        }
         */

        // 初始化From的一些属性, 以便将来运行起来快速方便
        // 在<from>元素下要插入若干<database>元素, 这是该From可用的数据库的列表
        // 这些信息属于软件初始化的范畴, 避免人工去配置
        // return:
        //      -1  出错
        //      0   对DOM没有修改
        //      1   对DOM发生了修改
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

            bool bChanged = false;
            XmlNodeList dbnodes = this.nodeDatabase.SelectNodes("database");

            // 列出所有<from>元素
            XmlNodeList fromnodes = this.nodeDatabase.SelectNodes("from");
            for (int i = 0; i < fromnodes.Count; i++)
            {
                string strFromName = DomUtil.GetAttr(fromnodes[i], "name");
                string strFromStyle = DomUtil.GetAttr(fromnodes[i], "style");

                // 删除原有子节点中<caption>以外的元素<database>
                RemoveDatabaseChildren(fromnodes[i]);

                // 从可用的数据库列表中, 挑出含有其下有from风格符合的
                for (int j = 0; j < dbnodes.Count; j++)
                {
                    string strDbName = DomUtil.GetAttr(dbnodes[j], "name");
                    // BUG: string strStyle = DomUtil.GetAttr(dbnodes[j], "style");

                    nRet = MatchFromStyle(strDbName,
                        strFromStyle,   //strStyle,
                        db_dir_results);
                    if (nRet == 0)
                        continue;

                    // 在<from>元素下加入一个<database>元素
                    XmlNode newnode = fromnodes[i].OwnerDocument.CreateElement("database");
                    fromnodes[i].AppendChild(newnode);
                    DomUtil.SetAttr(newnode, "name", strDbName);
                    bChanged = true;
                }

            }

            if (bChanged == true)
                return 1;

            return 0;
        }

        // 初始化数据库和From的一些属性, 以便将来运行起来快速方便
        // 在<database>元素下要插入若干<from>元素
        // 这些信息属于软件初始化的范畴, 避免人工去配置
        // return:
        //      -1  出错
        //      0   对DOM没有修改
        //      1   对DOM发生了修改
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

            bool bChanged = false;

            string strDbName = DomUtil.GetAttr(nodeDatabase, "name");

            RemoveChildren(nodeDatabase);
            bChanged = true;

            ResInfoItem dbitem = KernelDbInfo.GetDbItem(
                root_dir_results,
                strDbName);
            if (dbitem == null)
            {
                strError = "数据库内核根目录下没有找到名字为 '" +strDbName+ "' 的数据库事项。如果该数据库已经被删除，请修改dp2Library的library.xml文件中<virtualDatabases>元素下的有关内容";
                return -1;
            }
                
            // 在下级加入<caption>元素
            for (int i = 0; i < dbitem.Names.Length; i++)
            {
                string strText = dbitem.Names[i];
                nRet = strText.IndexOf(":");
                if (nRet == -1)
                {
                    strError = "names字符串 '" +strText+ "' 格式不正确。";
                    return -1;
                }
                string strLang = strText.Substring(0, nRet);
                string strName = strText.Substring(nRet + 1);

                XmlNode newnode = nodeDatabase.OwnerDocument.CreateElement("caption");
                newnode = nodeDatabase.AppendChild(newnode);
                DomUtil.SetAttr(newnode, "lang", strLang);
                DomUtil.SetNodeText(newnode, strName);
                bChanged = true;
            }

            // 
            ResInfoItem [] fromitems = (ResInfoItem[])db_dir_results[strDbName];
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

                // 以前忘记了
                DomUtil.SetAttr(fromnode, "style", item.TypeString);    // 2011/1/21
                bChanged = true;

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
                    bChanged = true;
                }

            }

            if (bChanged == true)
                return 1;
            return 0;
        }

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

        static int MatchFromStyle(string strDbName,
            string strFromStyle,
            Hashtable db_dir_results)
        {
            ResInfoItem[] infos = (ResInfoItem[])db_dir_results[strDbName];
            if (infos == null)
                return 0;
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].Type != ResTree.RESTYPE_FROM)
                    continue;
                if (StringUtil.IsInList(strFromStyle, infos[i].TypeString) == true)
                    return 1;
            }

            return 0;
        }

        // 根据虚拟的From名获得真实的From名。这个From是工作语言下的名字
        // 如果匹配多个From名，将在字符串中以逗号分隔 2007/7/8改造
        public string GetRealFromName(
            Hashtable db_dir_results,
            string strRealDbName,
            string strVirtualFromName)
        {
            List<string> styles = new List<string>();

            if (String.IsNullOrEmpty(strVirtualFromName) == true
                || strVirtualFromName == "<全部>"
                || strVirtualFromName.ToLower() == "<all>")
            {
                XmlNodeList nodes = this.nodeDatabase.SelectNodes("from");

                for (int i = 0; i < nodes.Count; i++)
                {
                    string strStyle = DomUtil.GetAttr(nodes[i], "style");

                    styles.Add(strStyle);
                }

            }
            else
            {

                XmlNode node = null;
                XmlNodeList nodes = this.nodeDatabase.SelectNodes("from/caption");
                for (int i = 0; i < nodes.Count; i++)
                {
                    node = nodes[i];
                    if (strVirtualFromName == node.InnerText.Trim())
                        goto FOUND;
                }
                return null;    // not found

                FOUND:

                string strStyle = DomUtil.GetAttr(node.ParentNode, "style");

                styles.Add(strStyle);
            }

            List<string> results = new List<string>();

            for (int i = 0; i < styles.Count; i++)
            {
                string strStyle = styles[i];

                // 从物理库的From事项中，找到style符合的
                ResInfoItem[] froms = (ResInfoItem[])db_dir_results[strRealDbName];

                if (froms == null)
                {
                    // return null;    // from目录事项居然没有找到
                    continue;
                }

                for (int j = 0; j < froms.Length; j++)
                {
                    ResInfoItem item = froms[j];
                    string strStyles = item.TypeString;
                    /*
                    if (StringUtil.IsInList(strStyle, strStyles) == true)
                    {
                        return item.Name;
                    }
                     * */
                    if (StringUtil.IsInList(strStyle, strStyles) == true)
                    {
                        results.Add(item.Name);
                    }

                }
            }

            if (results.Count == 0)
                return null;    // style没有发现匹配的

            string[] list = new string[results.Count];
            results.CopyTo(list);

            return String.Join(",", list);
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

            return results;
        }

    }


    /// <summary>
    /// 虚拟数据库的集合
    /// </summary>
    public class VirtualDatabaseCollection : List<VirtualDatabase>
    {
        public string ServerUrl = "";
        public string Lang = "zh";

        public ResInfoItem[] root_dir_results = null;  // 根目录信息
        public Hashtable db_dir_results = null;    // 二级目录信息

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


#if NOOOOOOOOOOOOOOOOOOO
        public VirtualDatabase this[string strDbName]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    VirtualDatabase vdb = this[i];

                    if (vdb.IsVirtual == true)
                    {
                        XmlNodeList nodes = vdb.nodeDatabase.SelectNodes("caption");
                        for (int j = 0; j < nodes.Count; j++)
                        {
                            if (nodes[j].InnerText == strDbName)
                                return vdb;
                        }
                    }
                    else
                    {
                        /*
                        if (vdb.GetName(null) == strDbName)
                            return vdb;
                         * */
                        // TODO: 大小写敏感问题？
                        List<string> all_names = vdb.GetAllNames();
                        if (all_names.IndexOf(strDbName) != -1)
                            return vdb;
                    }

                }

                return null;
            }
        }
#endif

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


        // 构造函数
        // 根据XML配置文件, 从服务器获取目录信息, 初始化数据结构
        // parameters:
        //      biblio_dbs_root <itemdbgroup>元素
        public int Initial(XmlNode root,
            RmsChannelCollection Channels,
            string strServerUrl,
            XmlNode biblio_dbs_root,
            out string strError)
        {
            strError = "";

            this.ServerUrl = strServerUrl;

            this.root_dir_results = null;
            this.db_dir_results = null;

        // 列出目录信息
        // 列出2级。第二级在Hashtable中
            int nRet = GetDirInfo(Channels,
                strServerUrl,
                out root_dir_results,
                out db_dir_results,
                out strError);
            if (nRet == -1)
                return -1;

            // 列出所有虚拟库XML节点
            XmlNodeList virtualnodes = root.SelectNodes("virtualDatabase");
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode node = root.ChildNodes[i];

                if (node.NodeType != XmlNodeType.Element)
                    continue;

                if (node.Name == "virtualDatabase")
                {

                    // 构造虚拟数据库对象
                    VirtualDatabase vdb = new VirtualDatabase();
                    vdb.nodeDatabase = node;

                    // 初始化From的一些属性, 以便将来运行起来快速方便
                    // 在<from>元素下要插入若干<database>元素, 这是该From可用的数据库的列表
                    // 这些信息属于软件初始化的范畴, 避免人工去配置
                    nRet = vdb.InitialFromProperty(
                        db_dir_results,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    this.Add(vdb);
                    continue;
                }

                if (node.Name == "database")    // 普通库
                {

                    // 构造普通数据库对象
                    VirtualDatabase vdb = new VirtualDatabase();
                    vdb.nodeDatabase = node;

                    // 要检查数据库名在<itemdbgroup>里面是否存在
                    string strDbName = DomUtil.GetAttr(node, "name");
                    if (biblio_dbs_root != null)
                    {
                        XmlNode nodeBiblio = biblio_dbs_root.SelectSingleNode("database[@biblioDbName='"+strDbName+"']");
                        if (nodeBiblio == null)
                        {
                            strError = "书目库 '"+strDbName+"' 在<itemdbgroup>内不存在定义，但却在<virtualDatabases>内存在。这表明<virtualDatabases>定义陈旧了，需要利用管理功能加以整理修改，或者直接在library.xml中修改";
                            return -1;
                        }
                    }

                    nRet = vdb.InitialAllProperty(
                        root_dir_results,
                        db_dir_results,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    this.Add(vdb);
                    continue;
                }
            }

            return 0;
        }

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

        /*
        // 发现数据库的原始定义
        int FindDatabaseOriginDef(string strDatabaseName,
            ResInfoItem[] dir_results,
            out string strError)
        {
            strError = "";


            return 1;
        }
         */

    }
}
