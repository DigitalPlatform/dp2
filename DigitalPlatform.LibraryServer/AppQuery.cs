using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和XML检索式加工相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 检查检索式内有没有超越规定读者检索的数据库
        // return:
        //      -1  error
        //      0   没有超越要求
        //      1   超越了要求
        public int CheckReaderOnlyXmlQuery(string strSourceQueryXml,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

#if NO
            if (this.vdbs == null)
            {
                this.ActivateManagerThreadForLoad();
                strError = "app.vdbs == null。故障原因请检查dp2Library日志";
                return -1;
            }

            Debug.Assert(this.vdbs != null, "");
#endif

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strSourceQueryXml);
            }
            catch (Exception ex)
            {
                strError = "XML检索式装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            // 遍历所有<target>元素
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//target");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strList = DomUtil.GetAttr(node, "list");

                if (String.IsNullOrEmpty(strList) == true)
                    continue;

                DbCollection dbs = new DbCollection();

                dbs.Build(strList);

                for (int j = 0; j < dbs.Count; j++)
                {
                    Db db = dbs[j];

                    // 允许预约到书库 2015/6/14
                    if (db.DbName == this.ArrivedDbName)
                        continue;

                    string strBiblioDbName = "";
                    string strDbType = this.GetDbType(db.DbName,
                        out strBiblioDbName);
                    if (String.IsNullOrEmpty(strDbType) == true)
                    {
                        strError = "数据库 '" + db.DbName + "' 超出了读者可检索的数据库范围";
                        return 1;
                    }
                }
            }

            return 0;
        }

        // 检查检索式内有没有超越当前用户管辖的读者库范围的读者库
        // 注: 本函数并不检查检索式中出现的读者库以外的其它类型的数据库
        // return:
        //      -1  error
        //      0   没有超越要求
        //      1   超越了要求
        public int CheckReaderDbXmlQuery(string strSourceQueryXml,
            string strLibraryCodeList,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

#if NO
            if (this.vdbs == null)
            {
                this.ActivateManagerThreadForLoad();
                strError = "app.vdbs == null。故障原因请检查dp2Library日志";
                return -1;
            }

            Debug.Assert(this.vdbs != null, "");
#endif

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strSourceQueryXml);
            }
            catch (Exception ex)
            {
                strError = "XML检索式装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            // 遍历所有<target>元素
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//target");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strList = DomUtil.GetAttr(node, "list");

                if (String.IsNullOrEmpty(strList) == true)
                    continue;

                DbCollection dbs = new DbCollection();

                dbs.Build(strList);

                for (int j = 0; j < dbs.Count; j++)
                {
                    Db db = dbs[j];

                    // 需要限制检索读者库为当前管辖的范围
                    {
                        string strLibraryCode = "";
                        bool bReaderDbInCirculation = true;
                        if (this.IsReaderDbName(db.DbName,
                            out bReaderDbInCirculation,
                            out strLibraryCode) == true)
                        {
                            // 检查当前操作者是否管辖这个读者库
                            // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                            if (this.IsCurrentChangeableReaderPath(db.DbName + "/?",
                                strLibraryCodeList) == false)
                            {
                                strError = $"读者库 '{db.DbName}' 不在{GetCurrentUserName(null)}管辖范围内";
                                return 1;
                            }
                        }
                    }
                }
            }

            return 0;
        }

        // 将包含虚拟库要求的XML检索式变换为内核能够理解的实在库XML检索式
        // return:
        //      -1  error
        //      0   没有发生变化
        //      1   发生了变化
        public int KernelizeXmlQuery(string strSourceQueryXml,
            out string strTargetQueryXml,
            out string strError)
        {
            strTargetQueryXml = "";
            strError = "";
            int nRet = 0;

            // 2024/5/10
            this.CheckVdbsThrow();

            Debug.Assert(this.vdbs != null, "");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strSourceQueryXml);
            }
            catch (Exception ex)
            {
                strError = "XML检索式装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            bool bChanged = false;

            {
                // 遍历所有<target>元素
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//target");
                foreach (XmlElement node in nodes)
                {
                    string strList = DomUtil.GetAttr(node, "list");

                    if (String.IsNullOrEmpty(strList) == true)
                        continue;

                    // 变换list参数值，将其中的虚拟库（连带途径）变换为物理库和途径
                    // parameters:
                    // return:
                    //      -1  error
                    //      0   没有发生变化
                    //      1   发生了变化
                    nRet = ConvertList(strList,
                        out string strOutputList,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                        continue;

                    bChanged = true;
                    DomUtil.SetAttr(node, "list", strOutputList);
                }
            }

#if NO
            {
                // 遍历所有 item 元素
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//item");
                foreach (XmlElement node in nodes)
                {
                    // 找到最近一个祖先的 target@list
                    string strList = GetTargetList(node);
                    if (strList == null)
                        continue;
                    List<string> parts = StringUtil.ParseTwoPart(strList, ":");

                    string strDbName = parts[0];
                    string strFrom = parts[1];

                    // 只取第一个 from
                    nRet = strFrom.IndexOf(",");
                    if (nRet != -1)
                        strFrom = strFrom.Substring(0, nRet).Trim();

                    if (string.IsNullOrEmpty(strDbName) || string.IsNullOrEmpty(strFrom))
                        continue;

                    Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                    strError = EnsureKdbs(false);
                    if (strError != null)
                    {
                        strError = "EnsureKdbs() error";
                        return -1;
                    }

                    string strFromStyle = this.kdbs.GetFromStyles(strDbName, strFrom, "zh");

                    string strQueryWord = node.SelectSingleNode("word/text()")?.Value;
                    string strMatchStyle = node.SelectSingleNode("match/text()")?.Value;
                    string strDataType = node.SelectSingleNode("dataType/text()")?.Value;
                    string strRelation = node.SelectSingleNode("relation/text()")?.Value;

                    if (string.IsNullOrEmpty(strRelation))
                        strRelation = "=";
                    if (string.IsNullOrEmpty(strDataType))
                        strDataType = "string";
                    if (strQueryWord == null)
                        strQueryWord = "";

                    if (strFrom == "__id")
                    {
                        // 如果为范围式
                        if (String.IsNullOrEmpty(strQueryWord) == false // 2013/3/25
                            && strQueryWord.IndexOfAny(new char[] { '-', '~' }) != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                            // 2012/3/29
                            strMatchStyle = "exact";
                        }
                        else if (String.IsNullOrEmpty(strQueryWord) == false)
                        {
                            strDataType = "number";
                            // 2012/3/29
                            strMatchStyle = "exact";
                        }
                        bChanged = true;
                    }
                    // 2014/8/28
                    else if (StringUtil.IsInList("_time", strFromStyle) == true)
                    {
                        // 如果为范围式
                        if (strQueryWord.IndexOf("~") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                        }
                        else
                        {
                            strDataType = "number";

                            // 如果检索词为空，并且匹配方式为前方一致、中间一致、后方一致，那么认为这是意图要命中全部记录
                            // 注意：如果检索词为空，并且匹配方式为精确一致，则需要认为是获取空值，也就是不存在对应检索点的记录
                            if (strMatchStyle != "exact" && string.IsNullOrEmpty(strQueryWord) == true)
                            {
                                strMatchStyle = "exact";
                                strRelation = "range";
                                strQueryWord = "~";
                            }
                        }

                        // 最后统一修改为exact。不能在一开始修改，因为strMatchStyle值还有帮助判断的作用
                        strMatchStyle = "exact";
                        bChanged = true;
                    }

                    DomUtil.SetElementText(node, "match", strMatchStyle);
                    DomUtil.SetElementText(node, "word", strQueryWord);
                    DomUtil.SetElementText(node, "dataType", strDataType);
                    DomUtil.SetElementText(node, "relation", strRelation);

                }
            }
#endif
            // 对 XML 检索式进行必要的变换。处理 range 和 time 检索细节
            nRet = FilterXmlQuery(dom,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
                bChanged = true;

            if (bChanged == false)
            {
                strTargetQueryXml = strSourceQueryXml;
                return 0;
            }

            strTargetQueryXml = dom.OuterXml;
            return 1;
        }

        // 对 XML 检索式进行必要的变换。处理 range 和 time 检索细节
        public int FilterXmlQuery(XmlDocument dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            bool bChanged = false;

            // 遍历所有 item 元素
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//item");
            foreach (XmlElement item in nodes)
            {
                // 找到最近一个祖先的 target@list
                string strList = GetTargetList(item);
                if (strList == null)
                    continue;

                // 2023/2/4
                // list 中用分号分隔为一个一个段落，每个段落表示一个数据库的检索途径
                string[] aDatabase = strList.Split(';');

                // 2023/2/4
                // 只取第一个分段
                string first_segment = aDatabase[0];
                List<string> parts = StringUtil.ParseTwoPart(first_segment, ":");

                string strDbName = parts[0];
                string strFrom = parts[1];

                // 只取第一个 from
                nRet = strFrom.IndexOf(",");
                if (nRet != -1)
                    strFrom = strFrom.Substring(0, nRet).Trim();

                if (string.IsNullOrEmpty(strDbName) || string.IsNullOrEmpty(strFrom))
                    continue;

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                strError = EnsureKdbs(false);
                if (strError != null)
                {
                    strError = "EnsureKdbs() error";
                    return -1;
                }

                string strFromStyle = this.kdbs.GetFromStyles(strDbName, strFrom, "zh");

                string strQueryWord = item.SelectSingleNode("word/text()")?.Value;
                string strMatchStyle = item.SelectSingleNode("match/text()")?.Value;
                string strDataType = item.SelectSingleNode("dataType/text()")?.Value;
                string strRelation = item.SelectSingleNode("relation/text()")?.Value;

                if (string.IsNullOrEmpty(strRelation))
                    strRelation = "=";
                if (string.IsNullOrEmpty(strDataType))
                    strDataType = "string";
                if (strQueryWord == null)
                    strQueryWord = "";

                if (strFrom == "__id")
                {
                    // 如果为范围式
                    if (String.IsNullOrEmpty(strQueryWord) == false // 2013/3/25
                        && strQueryWord.IndexOfAny(new char[] { '-', '~' }) != -1)
                    {
                        strRelation = "range";
                        strDataType = "number";
                        // 2012/3/29
                        strMatchStyle = "exact";
                    }
                    else if (String.IsNullOrEmpty(strQueryWord) == false)
                    {
                        strDataType = "number";
                        // 2012/3/29
                        strMatchStyle = "exact";
                    }
                    bChanged = true;
                }
                // 2014/8/28
                else if (StringUtil.IsInList("_time", strFromStyle) == true)
                {
                    // 如果为范围式
                    if (strQueryWord.IndexOf("~") != -1)
                    {
                        strRelation = "range";
                        strDataType = "number";
                    }
                    else
                    {
                        strDataType = "number";

                        // 如果检索词为空，并且匹配方式为前方一致、中间一致、后方一致，那么认为这是意图要命中全部记录
                        // 注意：如果检索词为空，并且匹配方式为精确一致，则需要认为是获取空值，也就是不存在对应检索点的记录
                        if (strMatchStyle != "exact" && string.IsNullOrEmpty(strQueryWord) == true)
                        {
                            strMatchStyle = "exact";
                            strRelation = "range";
                            strQueryWord = "~";
                        }
                    }

                    // 最后统一修改为exact。不能在一开始修改，因为strMatchStyle值还有帮助判断的作用
                    strMatchStyle = "exact";
                    bChanged = true;
                }

                DomUtil.SetElementText(item, "match", strMatchStyle);
                DomUtil.SetElementText(item, "word", strQueryWord);
                DomUtil.SetElementText(item, "dataType", strDataType);
                DomUtil.SetElementText(item, "relation", strRelation);
            }

            if (bChanged == true)
                return 1;

            return 0;
        }

        static string GetTargetList(XmlElement current)
        {
            while (current != null)
            {
                if (current.Name == "target")
                    return current.GetAttribute("list");
                current = current.ParentNode as XmlElement;
            }

            return null;
        }

        // 变换list参数值，将其中的虚拟库（连带途径）变换为物理库和途径
        // parameters:
        // return:
        //      -1  error
        //      0   没有发生变化
        //      1   发生了变化
        int ConvertList(string strSourceList,
            out string strTargetList,
            out string strError)
        {
            strTargetList = "";
            strError = "";

            DbCollection dbs = new DbCollection();

            dbs.Build(strSourceList);

            bool bChanged = false;

            DbCollection target_dbs = new DbCollection();

            for (int i = 0; i < dbs.Count; i++)
            {
                Db db = dbs[i];

                // 2024/5/10
                this.CheckVdbsThrow();

                Debug.Assert(this.vdbs != null, "");

                VirtualDatabase vdb = this.vdbs[db.DbName];

                if (vdb == null)  // 不是虚拟库
                {
                    target_dbs.Add(db);
                    continue;
                }

                if (vdb.IsVirtual == false)  // 不是虚拟库
                {
                    target_dbs.Add(db);
                    continue;
                }

                bChanged = true;

                // 一个Db对象可能演化为多个Db对象
                List<Db> multi_dbs = new List<Db>();

                // 获得下属的所有真实数据库名
                List<string> real_dbnames = vdb.GetRealDbNames();
                for (int j = 0; j < real_dbnames.Count; j++)
                {
                    Db target_db = new Db();
                    target_db.DbName = real_dbnames[j];

                    List<string> real_froms = new List<string>();
                    for (int k = 0; k < db.Froms.Count; k++)
                    {
                        // 虚拟的路径名
                        string strVirtualFromName = db.Froms[k];

                        // 2024/5/10
                        this.CheckVdbsThrow();

                        // 实在的路径名
                        string strRealFroms = vdb.GetRealFromName(
                            this.vdbs.db_dir_results,
                            target_db.DbName,
                            strVirtualFromName);

                        if (String.IsNullOrEmpty(strRealFroms) == true)
                            continue;

                        string[] froms = strRealFroms.Split(new char[] { ',' });

                        for (int l = 0; l < froms.Length; l++)
                        {
                            real_froms.Add(froms[l]);
                        }
                    }

                    if (real_froms.Count == 0)
                        continue;

                    target_db.Froms = real_froms;
                    multi_dbs.Add(target_db);
                }

                if (multi_dbs.Count == 0)
                    continue;

                target_dbs.AddRange(multi_dbs);
            }

            if (bChanged == false)
            {
                strTargetList = strSourceList;
                return 0;
            }

            strTargetList = target_dbs.GetString();
            return 1;
        }

        // 检查一个 XML 检索式里面的数据库名是否超出当前账户的权限
        // return:
        //      -1  检查过程出错
        //      0   没有超出权限的情况
        //      1   有超出权限的情况，报错信息在 strError 中
        //      2   出现了不被允许的空检索词，报错信息在 strError 中
        public int CheckSearchRights(
            SessionInfo sessioninfo,
            string strQueryXml,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strQueryXml);
            }
            catch (Exception ex)
            {
                strError = "XML 检索式装入 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            // 权限超出的数据库名列表
            List<string> outof_list = new List<string>();

            // 遍历所有 item 元素
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//item");
            foreach (XmlElement item in nodes)
            {
                // 找到最近一个祖先的 target@list
                string strList = GetTargetList(item);
                if (strList == null)
                    continue;   // 注: 也可以选择返回 -1 报错，说检索式结构不正确

                var dbnames = GetDbNames(strList);
                foreach (var dbname in dbnames)
                {
                    var error = HasSearchRight(sessioninfo, dbname);
                    if (error != null)
                        outof_list.Add(dbname + ":" + error);
                }

                if (string.IsNullOrEmpty(this.DenyEmptyQueryWord) == false)
                {
                    var word = item.SelectSingleNode("word")?.InnerText;
                    if (string.IsNullOrEmpty(word))
                    {
                        if (AllowEmptyQueryWord(sessioninfo) == false)
                        {

                            strError = $"当前访问者身份不允许使用空检索词";
                            return 2;
                        }
                    }
                }
            }

            if (outof_list.Count > 0)
            {
                strError = $"{SessionInfo.GetCurrentUserName(sessioninfo)}对下列数据库不具备检索权限:\r\n{StringUtil.MakePathList(outof_list, "\r\n")} ";
                return 1;
            }

            return 0;
        }

        public bool AllowEmptyQueryWord(SessionInfo sessioninfo)
        {
            if ((sessioninfo.UserID == "public" && StringUtil.IsInList("public", this._denyEmptyQueryWord))
|| (sessioninfo.UserType == "reader" && StringUtil.IsInList("reader", this._denyEmptyQueryWord))
|| (sessioninfo.UserType != "reader" && StringUtil.IsInList("worker", this._denyEmptyQueryWord))
)
            {
                return false;
            }

            return true;
        }

        // 判断一个数据库名是否允许当前用户检索
        string HasSearchRight(SessionInfo sessioninfo,
            string strDbName)
        {
            string rights = sessioninfo.RightsOrigin;
            var db_type = GetAllDbType(strDbName);

            string right = "";
            if (db_type == "biblio")
            {
                right = "searchbiblio";
            }
            else if (db_type == "reader")
            {
                var ret = (StringUtil.IsInList("searchreader", rights));
                if (ret == false)
                    return $"不具备权限 searchreader";
                {
                    // 检查当前操作者是否管辖这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strDbName + "/?",
                        sessioninfo.ExpandLibraryCodeList) == false)
                        return $"读者库 '{strDbName}' 不在{GetCurrentUserName(sessioninfo)}管辖范围内";
                }
                return null;
            }
            else if (db_type == "order")
            {
                right = "searchorder";
            }
            else if (db_type == "issue")
            {
                right = "searchissue";
            }
            else if (db_type == "item")
            {
                right = "searchitem";
            }
            else if (db_type == "comment")
            {
                right = "searchcomment";
            }
            else if (db_type == "authority")
            {
                right = "searchauthority";
            }
            else if (db_type == "amerce")
            {
                // 2024/2/12
                if (AmerceDbKeysContainsReaderRefIdKey() == false)
                    return $"违约金库 '{this.AmerceDbName}' 的检索点定义太旧。请先刷新其 keys 配置文件(并重建检索点)，再重试操作";
                right = "searchamerce";
            }
            else if (db_type == "inventory")
            {
                right = "searchinventory";
            }
            else if (db_type == "arrived")
            {
                right = "searcharrived";
            }
            else if (db_type == "publisher")
            {
                right = "searchpublisher";
            }
            else if (db_type == "zhongcihao")
            {
                right = "searchzhongcihao";
            }
            else if (db_type == "dictionary")
            {
                right = "searchdictionary";
            }
            else
                return $"无法识别数据库 '{strDbName}' 的类型";

            // 如果 right 中包含 getreaderinfo，则需要单独 if 一次
            if (StringUtil.IsInList("getreaderinfo", right))
            {
                // 注意这里要获得原始的 getreaderinfo:，因为并不在意 file 元素的权限
                var level = StringUtil.GetParameterByPrefix(rights, "getreaderinfo");
                if (level != null)
                    return null;
            }

            if (StringUtil.IsInList(right, rights) == false)
                return $"不具备权限 {right}";

            return null;
        }

        // 从 targe@list 中剖析出所有库名
        static List<string> GetDbNames(string strList)
        {
            List<string> results = new List<string>();
            // 以 ; 号分成多个库
            string[] aDatabase = strList.Split(';');
            foreach (string strOneDatabase in aDatabase)
            {
                if (string.IsNullOrEmpty(strOneDatabase))
                    continue;

                var parts = StringUtil.ParseTwoPart(strOneDatabase, ":");

                string strDbName = parts[0];
                if (string.IsNullOrEmpty(strDbName) == false)
                    results.Add(strDbName);
            }

            return results;
        }
    }

    class DbCollection : List<Db>
    {
        public int Build(string strList)
        {
            string[] segments = strList.Split(new char[] { ';' });
            for (int i = 0; i < segments.Length; i++)
            {
                string strSegment = segments[i].Trim();

                if (String.IsNullOrEmpty(strSegment) == true)
                    continue;

                // 解析出数据库名
                string strDbName = "";
                int nRet = strSegment.IndexOf(':');
                if (nRet == -1)
                {
                    strDbName = strSegment;
                    strSegment = "";
                }
                else
                {
                    strDbName = strSegment.Substring(0, nRet).Trim();
                    strSegment = strSegment.Substring(nRet + 1).Trim();
                }

                Db db = new Db(strDbName, strSegment);
                this.Add(db);
            }

            return 0;
        }

        public string GetString()
        {
            string strResult = "";
            for (int i = 0; i < this.Count; i++)
            {
                Db db = this[i];

                if (i != 0)
                    strResult += ";";
                strResult += db.GetString();
            }

            return strResult;
        }
    }

    class Db
    {
        public string DbName = "";
        public List<string> Froms = new List<string>();

        public Db()
        {
        }

        public Db(string strDbName,
            string strFromList)
        {
            this.DbName = strDbName;

            if (String.IsNullOrEmpty(strFromList) == true)
                return;

            string[] froms = strFromList.Split(new char[] { ',' });
            for (int i = 0; i < froms.Length; i++)
            {
                string strFrom = froms[i].Trim();

                if (String.IsNullOrEmpty(strFrom) == true)
                    continue;

                this.Froms.Add(strFrom);
            }
        }

        public string GetString()
        {
            if (this.Froms.Count == 0)
                return this.DbName;

            string strFroms = "";
            for (int i = 0; i < this.Froms.Count; i++)
            {
                if (i != 0)
                    strFroms += ",";
                strFroms += this.Froms[i];
            }

            return this.DbName + ":" + strFroms;
        }
    }
}
