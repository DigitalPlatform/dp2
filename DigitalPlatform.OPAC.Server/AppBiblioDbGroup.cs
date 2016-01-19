using System;
using System.Collections.Generic;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 本部分是和<biblioDbGroup>相关的代码
    /// </summary>
    public partial class OpacApplication
    {
        public List<ItemDbCfg> ItemDbs = null;

        // 返回所有馆代码。不包含为空的馆代码
        public List<string> GetAllLibraryCodes()
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//readerDbGroup/database");
            foreach (XmlNode node in nodes)
            {
                string strCode = DomUtil.GetAttr(node, "libraryCode");
                if (string.IsNullOrEmpty(strCode) == true)
                    continue;
                results.Add(strCode);
            }

            StringUtil.RemoveDupNoSort(ref results);

            return results;
        }

        // 读入<biblioDbGroup>相关配置
        // return:
        //      <biblioDbGroup>元素下<database>元素的个数。如果==0，表示配置不正常
        public int LoadBiblioDbGroupParam(
            out string strError)
        {
            strError = "";

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

                XmlDocument dom = this.OpacCfgDom;

                this.ItemDbs = new List<ItemDbCfg>();

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//biblioDbGroup/database");

                if (nodes.Count == 0)
                    return 0;

                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

                    ItemDbCfg item = new ItemDbCfg();

                    item.DbName = DomUtil.GetAttr(node, "itemDbName");

                    item.BiblioDbName = DomUtil.GetAttr(node, "biblioDbName");

                    item.BiblioDbSyntax = DomUtil.GetAttr(node, "syntax");

                    item.IssueDbName = DomUtil.GetAttr(node, "issueDbName");

                    item.OrderDbName = DomUtil.GetAttr(node, "orderDbName");

                    item.CommentDbName = DomUtil.GetAttr(node, "commentDbName");

                    item.UnionCatalogStyle = DomUtil.GetAttr(node, "unionCatalogStyle");

                    // 2008/6/4
                    bool bValue = true;
                    int nRet = DomUtil.GetBooleanParam(node,
                        "inCirculation",
                        true,
                        out bValue,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "元素<//biblioDbGroup/database>属性inCirculation读入时发生错误: " + strError;
                        return -1;
                    }

                    item.InCirculation = bValue;

                    item.Role = DomUtil.GetAttr(node, "role");

                    this.ItemDbs.Add(item);
                }

                return nodes.Count;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        // 获得(书目库相关角色)数据库的类型，顺便返回所从属的书目库名
        public string GetDbType(string strDbName,
            out string strBiblioDbName)
        {
            strBiblioDbName = "";

            if (String.IsNullOrEmpty(strDbName) == true)
                return null;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];
                strBiblioDbName = cfg.BiblioDbName;

                if (strDbName == cfg.DbName)
                    return "item";
                if (strDbName == cfg.BiblioDbName)
                    return "biblio";
                if (strDbName == cfg.IssueDbName)
                    return "issue";
                if (strDbName == cfg.OrderDbName)
                    return "order";
                if (strDbName == cfg.CommentDbName)
                    return "comment";
            }

            strBiblioDbName = "";
            return null;
        }

        // 获得数据库的类型
        public string GetDbType(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return null;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strDbName == this.ItemDbs[i].DbName)
                    return "item";
                if (strDbName == this.ItemDbs[i].BiblioDbName)
                    return "biblio";
                if (strDbName == this.ItemDbs[i].IssueDbName)
                    return "issue";
                if (strDbName == this.ItemDbs[i].OrderDbName)
                    return "order";
                if (strDbName == this.ItemDbs[i].CommentDbName)
                    return "comment";
            }

            return null;
        }

        // 是否在配置的实体库名之列?
        public bool IsItemDbName(string strItemDbName)
        {
            // 2008/10/16
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return false;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strItemDbName == this.ItemDbs[i].DbName)
                    return true;
            }

            // 2012/7/6
            // 可能是其他语言的数据库名
            XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/itemDbName/caption");
            foreach (XmlNode node in nodes)
            {
                if (strItemDbName == node.InnerText.Trim())
                    return true;
            }

            return false;
        }

        // 是否在配置的实体库名之列?
        // 另一版本，返回是否参与流通
        public bool IsItemDbName(string strItemDbName,
            out bool IsInCirculation)
        {
            IsInCirculation = false;

            // 2008/10/16
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return false;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strItemDbName == this.ItemDbs[i].DbName)
                {
                    IsInCirculation = this.ItemDbs[i].InCirculation;
                    return true;
                }
            }

            return false;
        }

        // TODO: 多语言改造
        // 是否在配置的书目库名之列?
        public ItemDbCfg GetBiblioDbCfg(string strBiblioDbName)
        {
            // 2008/10/16
            if (String.IsNullOrEmpty(strBiblioDbName) == true)
                return null;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];
                if (strBiblioDbName == this.ItemDbs[i].BiblioDbName)
                    return cfg;
            }

            return null;
        }

        // TODO：多语言改造
        // 是否具有orderWork角色
        public bool IsOrderWorkBiblioDb(string strBiblioDbName)
        {
            // 2008/10/16
            if (String.IsNullOrEmpty(strBiblioDbName) == true)
                return false;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];
                if (strBiblioDbName == this.ItemDbs[i].BiblioDbName)
                    return StringUtil.IsInList("orderWork", cfg.Role);
            }
            return false;
        }


        // 是否在配置的期库名之列?
        public bool IsIssueDbName(string strIssueDbName)
        {
            // 2008/10/16
            if (String.IsNullOrEmpty(strIssueDbName) == true)
                return false;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strIssueDbName == this.ItemDbs[i].IssueDbName)
                    return true;
            }

            // 2012/7/6
            // 可能是其他语言的数据库名
            XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/issueDbName/caption");
            foreach (XmlNode node in nodes)
            {
                if (strIssueDbName == node.InnerText.Trim())
                    return true;
            }

            return false;
        }

        // 是否在配置的订购库名之列?
        public bool IsOrderDbName(string strOrderDbName)
        {
            // 2008/10/16
            if (String.IsNullOrEmpty(strOrderDbName) == true)
                return false;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strOrderDbName == this.ItemDbs[i].OrderDbName)
                    return true;
            }

            // 2012/7/6
            // 可能是其他语言的数据库名
            XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/orderDbName/caption");
            foreach (XmlNode node in nodes)
            {
                if (strOrderDbName == node.InnerText.Trim())
                    return true;
            }

            return false;
        }

        // 是否在配置的评注库名之列?
        // 2008/12/8
        public bool IsCommentDbName(string strCommentDbName)
        {
            if (String.IsNullOrEmpty(strCommentDbName) == true)
                return false;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strCommentDbName == this.ItemDbs[i].CommentDbName)
                    return true;
            }

            // 2012/7/6
            // 可能是其他语言的数据库名
            XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/commentDbName/caption");
            foreach (XmlNode node in nodes)
            {
                if (strCommentDbName == node.InnerText.Trim())
                    return true;
            }

            return false;
        }

#if NO
        // 需要进一步简化，重写
        // 2012/7/2
        // (通过其他语言的书目库名)获得配置文件中所使用的那个书目库名
        public string GetCfgBiblioDbName(string strBiblioDbName)
        {
            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@biblioDbName='" + strBiblioDbName + "']");

            if (node != null)
                return strBiblioDbName;

            // 然后关注别名
            if (this.vdbs == null)
                return null;

            // 如果没有找到，则找<caption>
            VirtualDatabase vdb = this.vdbs[strBiblioDbName];
            if (vdb == null)
                return null;

            List<string> captions = vdb.GetAllNames();
            foreach (string caption in captions)
            {
                node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@biblioDbName='" + caption + "']");
                if (node != null)
                    return caption;
            }

            return null;
        }
#endif

        // 2012/7/9
        // 包装后的版本
        // 获得配置中使用的中立语言书目库名
        public string GetCfgBiblioDbName(string strBiblioDbName)
        {
            string strLang = "";
            return GetCfgBiblioDbName(strBiblioDbName, out strLang);
        }

        // 2012/7/6
        // 获得配置中使用的中立语言书目库名
        // (通过其他语言的书目库名)获得配置文件中所使用的那个书目库名
        // parameters:
        //      strLang 返回入口参数strBiblioDbName所对应的语言代码
        public string GetCfgBiblioDbName(string strBiblioDbName,
            out string strLang)
        {
            strLang = "";

            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@biblioDbName='" + strBiblioDbName + "']");

            if (node != null)
                return strBiblioDbName;

            // 可能是其他语言的数据库名
            XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/biblioDbName/caption");
            foreach (XmlNode current_node in nodes)
            {
                if (strBiblioDbName == current_node.InnerText.Trim())
                {
                    strLang = DomUtil.GetAttr(current_node, "lang");
                    node = current_node.ParentNode.ParentNode;
                    return DomUtil.GetAttr(node, "biblioDbName");
                }
            }

            return null;    // not found
        }


        // 判断一个数据库名是不是合法的书目库名
        public bool IsBiblioDbName(string strBiblioDbName)
        {
            if (GetCfgBiblioDbName(strBiblioDbName) == null)
                return false;

            return true;
        }

        // 从一个纯路径(不含url部分)中截取库名部分
        public static string GetDbName(string strLongPath)
        {
            int nRet = strLongPath.IndexOf("/");
            if (nRet == -1)
                return strLongPath;
            else
                return strLongPath.Substring(0, nRet);
        }

        // 从一个纯路径(不含url部分)中截取记录id部分
        public static string GetRecordId(string strLongPath)
        {
            int nRet = strLongPath.IndexOf("/");
            if (nRet == -1)
                return strLongPath;
            else
                return strLongPath.Substring(nRet + 1).Trim();
        }


        public string GetLangBiblioRecPath(
            string strLang,
            string strBiblioRecPath)
        {
            string strDbName = GetDbName(strBiblioRecPath);
            string strRecID = GetRecordId(strBiblioRecPath);

            string strDbName1 = GetBiblioDbName(
                strLang,
                strDbName);
            if (string.IsNullOrEmpty(strDbName1) == true)
                return strBiblioRecPath;
            return strDbName1 + "/" + strRecID;
        }

        public string GetBiblioDbName(
            string strLang,
            string strBiblioDbName)
        {
            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@biblioDbName='" + strBiblioDbName + "']/biblioDbName");
            if (node == null)
            {
                // 可能是其他语言的数据库名
                XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/biblioDbName/caption");
                foreach (XmlNode current_node in nodes)
                {
                    if (strBiblioDbName == current_node.InnerText.Trim())
                    {
                        node = current_node.ParentNode;
                        strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
                    }
                }

                if (node == null)
                    return null;    // not found
            }

            return DomUtil.GetCaption(strLang, node);
        }

        public string GetLangItemRecPath(
            string strType,
    string strLang,
    string strItemRecPath)
        {
            string strDbName = GetDbName(strItemRecPath);
            string strRecID = GetRecordId(strItemRecPath);

            string strDbName1 = GetItemDbName(
                strType,
                strLang,
                strDbName);
            if (string.IsNullOrEmpty(strDbName1) == true)
                return strItemRecPath;
            return strDbName1 + "/" + strRecID;
        }

        // 判断两个下属库路径是否等同
        public bool IsSameItemRecPath(
            string strType,
            string strRecPath1,
            string strRecPath2)
        {
            string strDbName1 = GetDbName(strRecPath1);
            string strRecID1 = GetRecordId(strRecPath1);

            string strDbName2 = GetDbName(strRecPath2);
            string strRecID2 = GetRecordId(strRecPath2);

            if (strRecID1 != strRecID2)
                return false;

            string strLangDbName1 = GetItemDbName(
    strType,
    "zh",
    strDbName1);
            string strLangDbName2 = GetItemDbName(
strType,
"zh",
strDbName2);

            if (strLangDbName1 == strLangDbName2)
                return true;

            return false;
        }

        // 获得特定语言的下属数据库名
        public string GetItemDbName(
            string strType,
            string strLang,
            string strItemDbName)
        {
            string strAttrName = "";
            if (strType == "item")
                strAttrName = "itemDbName";
            else if (strType == "issue")
                strAttrName = "issueDbName";
            else if (strType == "order")
                strAttrName = "orderDbName";
            else if (strType == "comment")
                strAttrName = "commentDbName";

            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@" + strAttrName + "='" + strItemDbName + "']/" + strAttrName);
            if (node == null)
            {
                // 可能是其他语言的数据库名
                XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/" + strAttrName + "/caption");
                foreach (XmlNode current_node in nodes)
                {
                    if (strItemDbName == current_node.InnerText.Trim())
                    {
                        node = current_node.ParentNode;
                        strItemDbName = DomUtil.GetAttr(node, strAttrName);
                    }
                }

                if (node == null)
                    return null;    // not found
            }

            return DomUtil.GetCaption(strLang, node);
        }
#if NO
        // 判断一个数据库名是不是合法的书目库名
        public bool IsBiblioDbName(string strBiblioDbName)
        {
            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@biblioDbName='" + strBiblioDbName + "']");

            if (node == null)
                return false;

            return true;
        }
#endif

        // 根据实体库名, 找到对应的书目库名
        // 注意，返回1的时候，strBiblioDbName也有可能为空
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetBiblioDbNameByItemDbName(string strItemDbName,
            out string strBiblioDbName,
            out string strError)
        {
            strError = "";
            strBiblioDbName = "";

            // 2007/5/25 new changed
            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@itemDbName='" + strItemDbName + "']");

            if (node == null)
            {
                // 2012/7/6
                // 可能是其他语言的数据库名
                XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/itemDbName/caption");
                foreach (XmlNode current_node in nodes)
                {
                    if (strItemDbName == current_node.InnerText.Trim())
                    {
                        node = current_node.ParentNode.ParentNode;  // 需要<database>元素
                        goto FOUND;
                    }
                }

                strError = "没有找到名为 '" + strItemDbName + "' 的实体库";
                return 0;
            }

        FOUND:
            strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
            return 1;
        }

        // 根据评注库名, 找到对应的书目库名
        // 注意，返回1的时候，strBiblioDbName也有可能为空
        // 2009/10/18
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetBiblioDbNameByCommentDbName(string strCommentDbName,
            out string strBiblioDbName,
            out string strError)
        {
            strError = "";
            strBiblioDbName = "";

            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@commentDbName='" + strCommentDbName + "']");

            if (node == null)
            {
                // 2012/7/6
                // 可能是其他语言的数据库名
                XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/commentDbName/caption");
                foreach (XmlNode current_node in nodes)
                {
                    if (strCommentDbName == current_node.InnerText.Trim())
                    {
                        node = current_node.ParentNode.ParentNode;  // 需要<database>元素
                        goto FOUND;
                    }
                }

                strError = "没有找到名为 '" + strCommentDbName + "' 的评注库";
                return 0;
            }

            FOUND:
            strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
            return 1;
        }

        // 根据订购库名, 找到对应的书目库名
        // 注意，返回1的时候，strBiblioDbName也有可能为空
        // 2008/8/28
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetBiblioDbNameByOrderDbName(string strOrderDbName,
            out string strBiblioDbName,
            out string strError)
        {
            strError = "";
            strBiblioDbName = "";

            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@orderDbName='" + strOrderDbName + "']");

            if (node == null)
            {
                // 2012/7/6
                // 可能是其他语言的数据库名
                XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/orderDbName/caption");
                foreach (XmlNode current_node in nodes)
                {
                    if (strOrderDbName == current_node.InnerText.Trim())
                    {
                        node = current_node.ParentNode.ParentNode;  // 需要<database>元素
                        goto FOUND;
                    }
                }

                strError = "没有找到名为 '" + strOrderDbName + "' 的订购库";
                return 0;
            }

            FOUND:
            strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
            return 1;
        }

        // 根据期库名, 找到对应的书目库名
        // 注意，返回1的时候，strBiblioDbName也有可能为空
        // 2009/2/2
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetBiblioDbNameByIssueDbName(string strIssueDbName,
            out string strBiblioDbName,
            out string strError)
        {
            strError = "";
            strBiblioDbName = "";

            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@issueDbName='" + strIssueDbName + "']");

            if (node == null)
            {
                // 2012/7/6
                // 可能是其他语言的数据库名
                XmlNodeList nodes = this.OpacCfgDom.DocumentElement.SelectNodes("//biblioDbGroup/database/issueDbName/caption");
                foreach (XmlNode current_node in nodes)
                {
                    if (strIssueDbName == current_node.InnerText.Trim())
                    {
                        node = current_node.ParentNode.ParentNode;  // 需要<database>元素
                        goto FOUND;
                    }
                }

                strError = "没有找到名为 '" + strIssueDbName + "' 的期库";
                return 0;
            }

            FOUND:
            strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
            return 1;
        }

        // 获得荐购存储库名列表
        // 所谓荐购存储库，就是用来存储读者推荐的新书目记录的目标库
        public List<string> GetOrderRecommendStoreDbNames()
        {
            List<string> results = new List<string>();
            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];
                if (StringUtil.IsInList("orderRecommendStore", cfg.Role) == true)
                    results.Add(cfg.BiblioDbName);
            }
            return results;
        }

        // 根据书目库名, 找到对应的实体库名
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetItemDbName(string strBiblioDbName,
            out string strItemDbName,
            out string strError)
        {
            strError = "";
            strItemDbName = "";

            // 2012/7/6
            string strCfgBiblioDbName = GetCfgBiblioDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strCfgBiblioDbName) == true)
                return 0;

            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@biblioDbName='" + strCfgBiblioDbName + "']");

            if (node == null)
                return 0;

            strItemDbName = DomUtil.GetAttr(node, "name");
            return 1;
        }

        // 根据书目库名, 找到对应的期库名
        // return:
        //      -1  出错
        //      0   没有找到(书目库)
        //      1   找到
        public int GetIssueDbName(string strBiblioDbName,
            out string strIssueDbName,
            out string strError)
        {
            strError = "";
            strIssueDbName = "";

            // 2012/7/6
            string strCfgBiblioDbName = GetCfgBiblioDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strCfgBiblioDbName) == true)
                return 0; 
            
            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@biblioDbName='" + strCfgBiblioDbName + "']");

            if (node == null)
                    return 0;

            strIssueDbName = DomUtil.GetAttr(node, "issueDbName");
            return 1;   // 注意有时虽然找到了书目库，但是issueDbName属性缺省或者为空
        }

        // 根据书目库名, 找到对应的订购库名
        // return:
        //      -1  出错
        //      0   没有找到(书目库)
        //      1   找到
        public int GetOrderDbName(string strBiblioDbName,
            out string strOrderDbName,
            out string strError)
        {
            strError = "";
            strOrderDbName = "";

            // 2012/7/6
            string strCfgBiblioDbName = GetCfgBiblioDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strCfgBiblioDbName) == true)
                return 0;

            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@biblioDbName='" + strCfgBiblioDbName + "']");

            if (node == null)
                    return 0;

            strOrderDbName = DomUtil.GetAttr(node, "orderDbName");
            return 1;   // 注意有时虽然找到了书目库，但是orderDbName属性缺省或者为空
        }

        // 根据书目库名, 找到对应的评注库名
        // 注：本函数尽力返回和strBiblioDbName同种语言的评注库名
        // return:
        //      -1  出错
        //      0   没有找到(书目库)
        //      1   找到
        public int GetCommentDbName(string strBiblioDbName,
            out string strCommentDbName,
            out string strError)
        {
            strError = "";
            strCommentDbName = "";

            string strLang = "";

            // 2012/7/6
            string strCfgBiblioDbName = GetCfgBiblioDbName(strBiblioDbName,out strLang);
            if (String.IsNullOrEmpty(strCfgBiblioDbName) == true)
                return 0;

            XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//biblioDbGroup/database[@biblioDbName='" + strCfgBiblioDbName + "']");

            if (node == null)
                return 0;

            if (string.IsNullOrEmpty(strLang) == true)
                strCommentDbName = DomUtil.GetAttr(node, "commentDbName");
            else
            {
                // 2012/7/9
                XmlNode nodeCaption = node.SelectSingleNode("commentDbName/caption[@lang='"+strLang+"']");
                if (nodeCaption == null)
                    strCommentDbName = DomUtil.GetAttr(node, "commentDbName");
                else
                    strCommentDbName = nodeCaption.InnerText.Trim();
            }

            return 1;   // 注意有时虽然找到了书目库，但是commentDbName属性缺省或者为空
        }
    }

    public class ItemDbCfg
    {
        public string DbName = "";  // 实体库名
        public string BiblioDbName = "";    // 书目库名
        public string BiblioDbSyntax = "";  // 书目库MARC语法

        public string IssueDbName = ""; // 期库
        public string OrderDbName = ""; // 订购库 2007/11/27
        public string CommentDbName = "";   // 评注库 2008/12/8

        public string UnionCatalogStyle = "";   // 联合编目特性 905  // 2007/12/15

        public bool InCirculation = true;   // 2008/6/4

        public string Role = "";    // 角色 biblioSource/orderWork // 2009/10/23
    }
}
