using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和构造 dp2kernel 检索式相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 构造检索实体库的 XML 检索式
        // return:
        //      -1  出错
        //      0   没有发现任何实体库定义
        //      1   成功
        public int BuildSearchItemQuery(
    string strItemDbName,
    string strQueryWord,
    int nPerMax,
    string strFrom,
    string strMatchStyle,
    string strLang,
    string strSearchStyle,
            out string strQueryXml,
            out string strError)
        {
            strError = "";
            strQueryXml = "";

            List<string> dbnames = new List<string>();

            if (String.IsNullOrEmpty(strItemDbName) == true
                || strItemDbName == "<全部>"
                || strItemDbName.ToLower() == "<all>")
            {
                foreach (ItemDbCfg cfg in this.ItemDbs)
                {
                    string strDbName = cfg.DbName;
                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;
                    dbnames.Add(strDbName);
                }

                if (dbnames.Count == 0)
                {
                    strError = "没有发现任何实体库";
                    return 0;
                }

            }
            else if (strItemDbName == "<全部期刊>"
                || strItemDbName.ToLower() == "<all series>")
            {
                foreach (ItemDbCfg cfg in this.ItemDbs)
                {
                    string strCurrentItemDbName = cfg.DbName;
                    string strCurrentIssueDbName = cfg.IssueDbName;

                    if (String.IsNullOrEmpty(strCurrentItemDbName) == true)
                        continue;

                    if (String.IsNullOrEmpty(strCurrentIssueDbName) == true)
                        continue;

                    dbnames.Add(strCurrentItemDbName);
                }

                if (dbnames.Count == 0)
                {
                    strError = "没有发现任何期刊实体库";
                    return 0;
                }
            }
            else if (strItemDbName == "<全部图书>"
                || strItemDbName.ToLower() == "<all book>")
            {
                foreach (ItemDbCfg cfg in this.ItemDbs)
                {
                    string strCurrentItemDbName = cfg.DbName;
                    string strCurrentIssueDbName = cfg.IssueDbName;

                    if (String.IsNullOrEmpty(strCurrentItemDbName) == true)
                        continue;

                    // 大书目库中必须不包含期库，才说明它是图书用途
                    if (String.IsNullOrEmpty(strCurrentIssueDbName) == false)
                        continue;

                    dbnames.Add(strCurrentItemDbName);
                }

                if (dbnames.Count == 0)
                {
                    strError = "没有发现任何图书实体库";
                    return 0;
                }
            }
            else
            {
                string[] splitted = strItemDbName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string strDbName in splitted)
                {
                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;

                    if (this.IsItemDbName(strDbName) == false)
                    {
                        strError = "库名 '" + strDbName + "' 不是合法的实体库名";
                        return -1;
                    }

                    dbnames.Add(strDbName);
                }
            }

            bool bDesc = StringUtil.IsInList("desc", strSearchStyle);

            // 构造检索式
            for (int i = 0; i < dbnames.Count; i++)
            {
                string strDbName = dbnames[i];

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

#if NO
                strError = EnsureKdbs(false);
                if (strError != null)
                    return -1;
#endif

                string strRelation = "=";
                string strDataType = "string";

#if NO
                string strFromStyle = this.kdbs.GetFromStyles(strDbName, strFrom, strLang);
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
                }
#endif

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)    // 2007/9/14 
                    + "'><item>"
                    + (bDesc == true ? "<order>DESC</order>" : "")
                + "<word>"
                    + StringUtil.GetXmlStringSimple(strQueryWord)
                    + "</word><match>" + strMatchStyle + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType><maxCount>" + nPerMax.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";

                if (i > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
            }

            if (dbnames.Count > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

            // 对 XML 检索式进行必要的变换。处理 range 和 time 检索细节
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strQueryXml);
            int nRet = FilterXmlQuery(dom, out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
                strQueryXml = dom.DocumentElement.OuterXml;

            return 1;
        }


        // 构造检索书目库的 XML 检索式
        // return:
        //      -2  没有找到指定风格的检索途径
        //      -1  出错
        //      0   没有发现任何书目库定义
        //      1   成功
        public int BuildSearchBiblioQuery(
    string strBiblioDbNames,
    string strQueryWord,
    int nPerMax,
    string strFromStyle,
    string strMatchStyle,
    string strLang,
    string strSearchStyle,
    out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "";

            List<string> dbnames = new List<string>();

            if (String.IsNullOrEmpty(strBiblioDbNames) == true
                || strBiblioDbNames == "<全部>"
                || strBiblioDbNames.ToLower() == "<all>")
            {
                foreach (ItemDbCfg cfg in this.ItemDbs)
                {
                    string strDbName = cfg.BiblioDbName;

                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;

                    dbnames.Add(strDbName);
                }

                if (dbnames.Count == 0)
                {
                    strError = "没有发现任何书目库";
                    return 0;
                }
            }
            else if (strBiblioDbNames == "<全部图书>"
                || strBiblioDbNames.ToLower() == "<all book>")
            {
                foreach (ItemDbCfg cfg in this.ItemDbs)
                {
                    if (String.IsNullOrEmpty(cfg.IssueDbName) == false)
                        continue;
                    string strDbName = cfg.BiblioDbName;
                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;

                    dbnames.Add(strDbName);
                }

                if (dbnames.Count == 0)
                {
                    strError = "没有发现任何图书类型的书目库";
                    return 0;
                }
            }
            else if (strBiblioDbNames == "<全部期刊>"
                || strBiblioDbNames.ToLower() == "<all series>")
            {
                foreach (ItemDbCfg cfg in this.ItemDbs)
                {
                    if (String.IsNullOrEmpty(cfg.IssueDbName) == true)
                        continue;
                    string strDbName = cfg.BiblioDbName;
                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;

                    dbnames.Add(strDbName);
                }

                if (dbnames.Count == 0)
                {
                    strError = "没有发现任何期刊类型的书目库";
                    return 0;
                }
            }
            else
            {
                string[] splitted = strBiblioDbNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string strDbName in splitted)
                {
                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;

                    if (this.IsBiblioDbName(strDbName) == false)
                    {
                        strError = "库名 '" + strDbName + "' 不是合法的书目库名";
                        return -1;
                    }

                    dbnames.Add(strDbName);
                }
            }

            bool bDesc = StringUtil.IsInList("desc", strSearchStyle);

            // 构造检索式
            string strFromList = "";
            string strUsedFromCaptions = "";
            for (int i = 0; i < dbnames.Count; i++)
            {
                string strDbName = dbnames[i];

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                strError = EnsureKdbs(false);
                if (strError != null)
                    return -1;

                string strFromCaptions = this.kdbs.BuildCaptionListByStyleList(strDbName, strFromStyle, strLang);

                if (String.IsNullOrEmpty(strFromCaptions) == true)
                {
                    continue;
                }

                strUsedFromCaptions = strFromCaptions;

                if (String.IsNullOrEmpty(strFromList) == false)
                    strFromList += ";";
                strFromList += strDbName + ":" + strFromCaptions;
            }

            if (String.IsNullOrEmpty(strFromList) == true)
            {
#if NO
                strError = "在数据库 '" + StringUtil.MakePathList(dbnames) + "' 中没有找到匹配风格 '" + strFromStyle + "' 的From Caption";
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.FromNotFound;
                return result;
#endif
                strError = "在数据库 '" + StringUtil.MakePathList(dbnames) + "' 中没有找到匹配风格 '" + strFromStyle + "' 的From Caption";
                return -2;
            }

            string strRelation = "=";
            string strDataType = "string";

            if (strUsedFromCaptions == "__id")
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
                    // 2008/3/9 
                    strDataType = "number";
                    // 2012/3/29
                    strMatchStyle = "exact";
                }
            }
            /*
        else if (strUsedFromCaptions == "操作时间"
            || strUsedFromCaptions == "出版时间")                     * */
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

                    // 2012/3/29
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
            }

            strQueryXml = "";
            strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strFromList)
                + "'><option warning='0'/><item>"
                + (bDesc == true ? "<order>DESC</order>" : "")
                + "<word>"
                + StringUtil.GetXmlStringSimple(strQueryWord)
                + "</word><match>" + strMatchStyle + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType><maxCount>" + nPerMax.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";

            return 1;
        }

    }

}
