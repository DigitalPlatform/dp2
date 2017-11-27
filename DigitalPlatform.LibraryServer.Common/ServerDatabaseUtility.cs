using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DigitalPlatform.LibraryServer.Common
{
    /// <summary>
    /// 用户验证数据库创建或者删除后 library.xml 和相关结构是否正确的函数库
    /// </summary>
    public static class ServerDatabaseUtility
    {
        // 验证 library.xml 在数据库删除操作以后，相关元素信息是否正确
        // parameters:
        //      cfg_dom 装入了 library.xml 的 XmlDocument 对象
        //      strDbName   被删除的数据库名
        // return:
        //      -1  验证过程出现错误(也就是说验证过程没有来的及完成)
        //      0   验证发现不正确
        //      1   验证发现正确
        public static int VerifyDatabaseDelete(XmlDocument cfg_dom,
            string strDbType,
            string strDbName,
            out string strError)
        {
            strError = "";

            if (cfg_dom == null)
            {
                strError = "cfg_dom == null";
                return -1;
            }

            if (string.IsNullOrEmpty(strDbType) == true)
            {
                strError = "strDbType 参数值不应为空";
                return -1;
            }
            if (string.IsNullOrEmpty(strDbName) == true)
            {
                strError = "strDbName 参数值不应为空";
                return -1;
            }

            // 实用库
            if (IsUtilDbType(strDbType))
            {
                if (IsUtilDbName(cfg_dom, strDbName) == true)
                {
                    strError = "删除完成后，实用库名 '" + strDbName + "' 在 library.xml 的 utilDb 元素内没有清理干净";
                    return 0;
                }
                return 1;
            }

            string strDbTypeCaption = GetTypeCaption(strDbType);

            // 单个数据库
            if (IsSingleDbType(strDbType))
            {
                string dbname = GetSingleDbName(cfg_dom, strDbType);
                if (string.IsNullOrEmpty(dbname) == false)
                {
                    strError = "删除完成后，"+strDbTypeCaption+"库名 '" + strDbName + "' 在 library.xml 的 相应元素内没有清理干净";
                    return 0;
                }
            }

            // 书目库
            if (strDbType == "biblio")
            {
                if (IsBiblioDbName(cfg_dom,  strDbName) == false)
                {
                    strError = "删除完成后，" + strDbTypeCaption + "库名 '" + strDbName + "' 在 library.xml 的 itemdbgroup/database 元素内没有清理干净";
                    return 0;
                }
            }

            // 单个书目库下属库
            if (IsBiblioSubType(strDbType) == true)
            {
                if (IsBiblioSubDbName(cfg_dom, strDbType, strDbName) == false)
                {
                    strError = "删除完成后，" + strDbTypeCaption + "库名 '" + strDbName + "' 在 library.xml 的 itemdbgroup/database 元素内没有清理干净";
                    return 0;
                }
            }
            return 1;
        }

        public static string GetTypeCaption(string strDbType)
        {
            switch (strDbType)
            {
                case "arrived":
                    return "预约到书";
                case "amerce":
                    return "违约金";
                case "message":
                    return "消息";
                case "invoice":
                    return "发票";
                case "pinyin":
                    return "拼音";
                case "gcat":
                    return "著者号码";
                case "word":
                    return "词";
                case "biblio":
                    return "书目";
                case "entity":
                    return "实体";
                case "order":
                    return "订购";
                case "issue":
                    return "期";
                case "comment":
                    return "评注";
                case "util":
                    return "实用";
                default:
                    throw new ArgumentException("未知的 strDbType 值 '" + strDbType + "'", "strDbType");
            }
        }

        #region 书目库

        // 是否为书目库名?
        // 注： 未考虑书目库名字的其他语种情况
        public static bool IsBiblioDbName(XmlDocument LibraryCfgDom, 
            string strBiblioDbName)
        {
            XmlNode node = LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");
            if (node != null)
                return true;
            return false;
        }

        // 是否为书目库的下属数据库名?
        public static bool IsBiblioSubDbName(XmlDocument LibraryCfgDom,
            string strDbType,
            string strSubDbName)
        {
            string strAttrName = GetBiblioChildDbAttributeName(strDbType);
            if (string.IsNullOrEmpty(strAttrName))
            {
                string strError = "数据库类型 '" + strDbType + "' 无法找到对应的 itemdbgroup/database 元素内的对应属性名";
                throw new Exception(strError);
            }
            XmlNode node = LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@" + strAttrName + "='" + strSubDbName + "']");
            if (node != null)
                return true;
            return false;
        }

        // 是否为书目库的下属数据库类型之一?
        public static bool IsBiblioSubType(string strType)
        {
            if (strType == "entity"
                    || strType == "order"
                    || strType == "issue"
                    || strType == "comment")
                return true;
            return false;
        }

        // 根据数据库类型，获得在 itemdbgroup/database 元素中的相关属性名
        // 注意，属性名不完全和类型名有规律对应关系
        public static string GetBiblioChildDbAttributeName(string strDbType)
        {
            switch (strDbType)
            {
                case "entity":
                    return "name";
                case "order":
                    return "orderDbName";
                case "issue":
                    return "issueDbName";
                case "comment":
                    return "commentDbName";
                case "biblio":
                    return "biblioDbName";
                default:
                    return null;
            }
        }

        #endregion


        #region 实用库

        // 是否为实用库的具体类型之一?
        public static bool IsUtilDbType(string strType)
        {
            if (strType == "publisher"
                    || strType == "zhongcihao"
                    || strType == "dictionary"
                    || strType == "inventory")
                return true;
            return false;
        }

        // 是否为实用库名
        // 实用库包括 publisher / zhongcihao / dictionary / inventory 类型
        public static bool IsUtilDbName(XmlDocument LibraryCfgDom,
            string strUtilDbName)
        {
            XmlNode node = LibraryCfgDom.DocumentElement.SelectSingleNode("utilDb/database[@name='" + strUtilDbName + "']");
            if (node == null)
                return false;

            return true;
        }

        // 是否为特定类型的实用库名
        public static bool IsUtilDbName(XmlDocument LibraryCfgDom,
            string strUtilDbName, 
            string strType)
        {
            XmlNode node = LibraryCfgDom.DocumentElement.SelectSingleNode("utilDb/database[@name='" + strUtilDbName + "' and @type='" + strType + "']");
            if (node == null)
                return false;

            return true;
        }

        // 根据数据库名字，获得一个实用库的类型
        public static string GetUtilDbType(XmlDocument LibraryCfgDom,
            string strUtilDbName)
        {
            XmlNode node = LibraryCfgDom.DocumentElement.SelectSingleNode("utilDb/database[@name='" + strUtilDbName + "']/@type");
            if (node == null)
                return null;

            return node.Value;
        }

        // 确保在 library.xml 中创建一个 utilDb/database 元素
        public static XmlElement EnsureUtilDatabaseElement(XmlDocument cfg_dom,
            string strDbName,
            string strType)
        {
            XmlElement nodeDatabase = cfg_dom.DocumentElement.SelectSingleNode("utilDb/database[@name='" + strDbName + "']") as XmlElement;
            if (nodeDatabase != null)
            {
                nodeDatabase.SetAttribute("type", strType);
                return nodeDatabase;
            }

            XmlElement container = EnsureContainerElement(cfg_dom, "utilDb");

            nodeDatabase = cfg_dom.CreateElement("database");
            container.AppendChild(nodeDatabase);
            nodeDatabase.SetAttribute("name", strDbName);
            nodeDatabase.SetAttribute("type", strType);
            return nodeDatabase;
        }

        // 确保在根元素创建一个容器元素
        public static XmlElement EnsureContainerElement(XmlDocument cfg_dom,
            string strElementName)
        {
            XmlElement container = cfg_dom.DocumentElement.SelectSingleNode(strElementName) as XmlElement;
            if (container == null)
            {
                container = cfg_dom.CreateElement(strElementName);
                cfg_dom.DocumentElement.AppendChild(container);
            }

            return container;
        }

        #endregion

        #region 单个数据库

        /*
        // 是否为单个数据库类型?
        //  单个数据库是指那些 DbName 保存在内存变量中的数据库类型。(一般不要从 cfg_dom 中直接存取)
        static bool IsSingleDbType(string strType)
        {
            switch (strType)
            {
                case "arrived":
                case "amerce":
                case "message":
                case "invoice":
                case "pinyin":
                case "gcat":
                case "word":
                    return true;
                default:
                    return false;
            }
        }         * */

        public static bool IsSingleDbType(string strType)
        {
            return strType == "arrived"
                    || strType == "amerce"
                    || strType == "message"
                    || strType == "invoice"
                    || strType == "pinyin"
                    || strType == "gcat"
                    || strType == "word";
        }

        // 从 library.xml 中得到各种单个数据库名
        public static string GetSingleDbName(XmlDocument cfg_dom,
            string strType)
        {
            string xpath = "";
            switch (strType)
            {
                case "arrived":
                    xpath = "arrived/@dbname";
                    break;
                case "amerce":
                    xpath = "amerce/@dbname";
                    break;
                case "message":
                    xpath = "message/@dbname";
                    break;
                case "invoice":
                    xpath = "invoice/@dbname";
                    break;
                case "pinyin":
                    xpath = "pinyin/@dbname";
                    break;
                case "gcat":
                    xpath = "gcat/@dbname";
                    break;
                case "word":
                    xpath = "word/@dbname";
                    break;
                default:
                    throw new ArgumentException("未知的 strType 值 '" + strType + "'", "strType");
            }

            XmlNode attr = cfg_dom.DocumentElement.SelectSingleNode(xpath);
            return attr.Value;
        }

        #endregion
    }
}
