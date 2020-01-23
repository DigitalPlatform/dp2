﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

// using DigitalPlatform.Marc;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using static DigitalPlatform.Script.ScriptUtil;

namespace DigitalPlatform.Marc
{
    public static class MarcTable
    {
        static readonly string CRLF = "\n";

        #region UNIMARC

        /* // https://en.wikipedia.org/wiki/International_Standard_Bibliographic_Description
0: Content form and media type area
1: Title and statement of responsibility area, consisting of 
1.1 Title proper
1.2 Parallel title
1.3 Other title information
1.4 Statement of responsibility
2: Edition area
3: Material or type of resource specific area (e.g., the scale of a map or the numbering of a periodical)
4: Publication, production, distribution, etc., area
5: Material description area (e.g., number of pages in a book or number of CDs issued as a unit)
6: Series area
7: Notes area
8: Resource identifier and terms of availability area (e.g., ISBN, ISSN)
         * */
        // parameters:
        //     strStyle    创建结果的风格。如果为 "" 表示创建所有大项和题名拼音、数字资源。
        public static int ScriptUnimarc(
            string strRecPath,
            string strMARC,
            string strStyle,
            XmlElement maps_container,
            out List<NameValueLine> results,
            out string strError)
        {
            strError = "";
            results = new List<NameValueLine>();

            if (strStyle == null)
                strStyle = "";

            MarcRecord record = new MarcRecord(strMARC);

            if (record.ChildNodes.count == 0)
                return 0;

            if (StringUtil.IsInList("*", strStyle) // strStyle == "*" 
                || string.IsNullOrEmpty(strStyle))
                strStyle += ",areas,coverimageurl,titlepinyin,object,summary,subjects,classes";

            /*
* content_form_area
* title_area
* edition_area
* material_specific_area
* publication_area
* material_description_area
* series_area
* notes_area
* resource_identifier_area
* * */

            if (StringUtil.IsInList("coverimageurl", strStyle))
            {
                string imageUrl = ScriptUtil.GetCoverImageUrl(strMARC, "LargeImage");
                if (string.IsNullOrEmpty(imageUrl) == false)
                    results.Add(new NameValueLine("_coverImage", imageUrl, "coverimageurl"));
            }

            // 题名拼音
            // 200 字段
            if (StringUtil.IsInList("titlepinyin", strStyle))
            {
                MarcNodeList subfields = record.select("field[@name='200']/subfield[@name='9']");
                if (subfields.count == 0)
                    subfields = record.select("field[@name='200']/subfield[@name='A']");    // 兼容 CALIS 习惯
                if (subfields.count > 0)
                {
                    List<string> texts = new List<string>();
                    subfields.Contents.ForEach((o) => { texts.Add(o); });
                    if (texts.Count > 0)
                        results.Add(new NameValueLine("题名与责任说明拼音", StringUtil.MakePathList(texts, " ; "), "titlepinyin"));
                }
            }

            // 1: Title and statement of responsibility area
            // 200 字段
            if (StringUtil.IsInList("areas,title_area", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='200']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("题名与责任者", BuildUnimarcFields(fields), "title_area"));
            }

            // 题名
            if (StringUtil.IsInList("title", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='200']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("题名", BuildUnimarcFields(fields, "acdehi"), "title"));
            }

            // 作者
            if (StringUtil.IsInList("author", strStyle))
            {
#if NO
                MarcNodeList fields = record.select("field[@name='200']");
                if (fields.count > 0)
                {
                    results.Add(new NameValueLine("责任者",
                        BuildUnimarcFields(fields, "fg").Trim().Trim(new char[] { '/', ';' }).Trim(),
                        "author"));
                }
#endif

                MarcNodeList subfields = record.select("field[@name='200']/subfield[@name='f' or @name='g']");
                if (subfields.count > 0)
                {
                    List<string> texts = new List<string>();
                    subfields.Contents.ForEach((o) =>
                    {
                        if (string.IsNullOrEmpty(o))
                            return;
                        if (texts.Count > 0 && o[0] != '=')
                            texts.Add(" ; " + o);
                        else
                        {
                            if (o[0] == '=')
                            {
                                // 确保 等号+空
                                o = RemovePrefixChar(o, "=");
                                o = "= " + o;
                            }
                            texts.Add(o);
                        }
                    });
                    if (texts.Count > 0)
                        results.Add(new NameValueLine("责任者", StringUtil.MakePathList(texts, ""), "author"));
                }
            }

            // 责任者检索点
            if (StringUtil.IsInList("author_accesspoint", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='701' or @name='702' or @name='711' or @name='712' or @name='700']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("责任者检索点",
                        BuildUnimarcFields(fields),
                        "author_accesspoint"));
            }

            // 2: Edition area
            // 205 字段
            if (StringUtil.IsInList("areas,edition_area", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='205']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("版本项", BuildUnimarcFields(fields), "edition_area"));
            }

            // 资料特殊细节
            // 3: Material or type of resource specific area(e.g., the scale of a map or the numbering of a periodical)
            // 206 207 208 字段
            if (StringUtil.IsInList("areas,material_specific_area", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='206' or @name='207' or @name='208']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("资料特殊细节项", BuildUnimarcFields(fields), "material_specific_area"));
            }

            // 出版发行项
            // 4: Publication, production, distribution, etc., area
            // 210 字段
            if (StringUtil.IsInList("areas,publication_area", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='210']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("出版发行项", BuildUnimarcFields(fields), "publication_area"));
            }

            // 出版者
            if (StringUtil.IsInList("publisher", strStyle))
            {
#if NO
                MarcNodeList fields = record.select("field[@name='210']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("出版者",
                        BuildUnimarcFields(fields, "c").Trim().Trim(new char[] { ':' }),
                        "publisher"));
#endif
                MarcNodeList subfields = record.select("field[@name='210']/subfield[@name='c']");
                if (subfields.count > 0)
                {
                    List<string> texts = new List<string>();
                    subfields.Contents.ForEach((o) =>
                    {
                        if (string.IsNullOrEmpty(o))
                            return;
                        if (texts.Count > 0)
                            texts.Add(" : " + o);
                        else

                            texts.Add(o);
                    });
                    if (texts.Count > 0)
                        results.Add(new NameValueLine("出版者", StringUtil.MakePathList(texts, ""), "publisher"));
                }

            }

            // 出版时间
            if (StringUtil.IsInList("publishtime", strStyle))
            {
#if NO
                MarcNodeList fields = record.select("field[@name='210']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("出版时间",
                        BuildUnimarcFields(fields, "d").Trim().Trim(new char[] { ',' }),
                        "publishtime"));
#endif
                MarcNodeList subfields = record.select("field[@name='210']/subfield[@name='d']");
                if (subfields.count > 0)
                {
                    List<string> texts = new List<string>();
                    subfields.Contents.ForEach((o) =>
                    {
                        if (string.IsNullOrEmpty(o))
                            return;
                        if (texts.Count > 0)
                            texts.Add(", " + o);
                        else

                            texts.Add(o);
                    });
                    if (texts.Count > 0)
                        results.Add(new NameValueLine("出版时间", StringUtil.MakePathList(texts, ""), "publishtime"));
                }
            }

            // 载体形态项
            // 215 字段
            // 5: Material description area(e.g., number of pages in a book or number of CDs issued as a unit)
            if (StringUtil.IsInList("areas,material_description_area", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='215']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("载体形态项", BuildUnimarcFields(fields), "material_description_area"));
            }

            // 页数
            if (StringUtil.IsInList("pages", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='215']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("页数",
                        BuildUnimarcFields(fields, "a").Trim().Trim(new char[] { ',' }),
                        "pages"));
            }

            // 丛编项
            // 6: Series area
            // 225 字段
            if (StringUtil.IsInList("areas,series_area", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='225']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("丛编项", BuildUnimarcFields(fields), "series_area"));
            }

            // 附注
            // 7: Notes area
            // 300 304 312 314 320 324 326 327 字段
            if (StringUtil.IsInList("areas,notes_area", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='300' or @name='304' or @name='312' or @name='314'  or @name='320'  or @name='324'  or @name='326'  or @name='327']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("附注项", BuildUnimarcFields(fields), "notes_area,notes"));
            }

            // 获得方式
            // 8: resource_identifier_area
            // 8: Resource identifier and terms of availability area(e.g., ISBN, ISSN)
            if (StringUtil.IsInList("areas,resource_identifier_area", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='010' or @name='011' or @name='091']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("获得方式项", BuildUnimarcFields(fields), "resource_identifier_area"));
            }

            // ISBN
            if (StringUtil.IsInList("isbn", strStyle))
            {
                StringBuilder text = new StringBuilder();
                record.select("field[@name='010']/subfield[@name='a' or @name='z']")
                    .List.ForEach((o) =>
                    {
                        if (text.Length > 0)
                            text.Append(CRLF);
                        text.Append(o.Content);
                        if (o.Name == "z")
                            text.Append("(错误的)");
                    });
                if (text.Length > 0)
                    results.Add(new NameValueLine("ISBN", text.ToString(), "isbn"));
            }

            // ISSN
            if (StringUtil.IsInList("issn", strStyle))
            {
                StringBuilder text = new StringBuilder();
                record.select("field[@name='011']/subfield[@name='a' or @name='z']")
                    .List.ForEach((o) =>
                    {
                        if (text.Length > 0)
                            text.Append(CRLF);
                        text.Append(o.Content);
                        if (o.Name == "z")
                            text.Append("(错误的)");
                    });
                if (text.Length > 0)
                    results.Add(new NameValueLine("ISSN", text.ToString(), "issn"));
            }

            // 价格
            if (StringUtil.IsInList("price", strStyle))
            {
                StringBuilder text = new StringBuilder();
                record.select("field[@name='010' or @name='011' or @name='091']/subfield[@name='d']")
                    .List.ForEach((o) =>
                    {
                        if (text.Length > 0)
                            text.Append(CRLF);
                        text.Append(o.Content);
                    });
                if (text.Length > 0)
                    results.Add(new NameValueLine("价格", text.ToString(), "price"));
            }

            // 提要文摘
            // 330 字段
            if (StringUtil.IsInList("summary", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='330']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("提要文摘", BuildUnimarcFields(fields), "summary"));
            }

            // 主题分析项
            // 600 601 606 610 字段
            if (StringUtil.IsInList("subjects", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='600' or @name='601' or @name='606' or @name='610']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("主题分析", BuildUnimarcFields(fields), "subjects"));
            }

            // 分类号
            // 69x 字段
            if (StringUtil.IsInList("classes", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='690' or @name='692' or @name='694']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("分类号", BuildUnimarcFields(fields), "classes"));
            }

            // 中图法分类号
            if (StringUtil.IsInList("clc_class", strStyle))
            {
                StringBuilder text = new StringBuilder();
                record.select("field[@name='690']/subfield[@name='a']")
                    .List.ForEach((o) =>
                    {
                        if (text.Length > 0)
                            text.Append(CRLF);
                        text.Append(o.Content);
                    });
                if (text.Length > 0)
                    results.Add(new NameValueLine("中图法分类号", text.ToString(), "clc_class"));
            }

            // 科图法分类号
            if (StringUtil.IsInList("ktf_class", strStyle))
            {
                StringBuilder text = new StringBuilder();
                record.select("field[@name='692']/subfield[@name='a']")
                    .List.ForEach((o) =>
                    {
                        if (text.Length > 0)
                            text.Append(CRLF);
                        text.Append(o.Content);
                    });
                if (text.Length > 0)
                    results.Add(new NameValueLine("科图法分类号", text.ToString(), "ktf_class"));
            }

            // 人大法分类号
            if (StringUtil.IsInList("rdf_class", strStyle))
            {
                StringBuilder text = new StringBuilder();
                record.select("field[@name='694']/subfield[@name='a']")
                    .List.ForEach((o) =>
                    {
                        if (text.Length > 0)
                            text.Append(CRLF);
                        text.Append(o.Content);
                    });
                if (text.Length > 0)
                    results.Add(new NameValueLine("人大法分类号", text.ToString(), "rdf_class"));
            }

            // 相关题名
            if (StringUtil.IsInList("other_titles", strStyle))
            {
                MarcNodeList fields = record.select("field[@name='500' or @name='501' or @name='503'  or @name='512' or @name='513' or @name='514' or @name='515' or @name='516' or @name='520' or @name='530' or @name='531' or @name='532' or @name='540' or @name='541']");
                if (fields.count > 0)
                    results.Add(new NameValueLine("相关题名", BuildUnimarcFields(fields), "other_titles"));
            }

            // 数字资源
            if (StringUtil.IsInList("object", strStyle))
            {

                string objectTable = ScriptUtil.BuildObjectXmlTable(strMARC,
                    StringUtil.IsInList("object_template", strStyle) ? BuildObjectHtmlTableStyle.Template | BuildObjectHtmlTableStyle.TemplateMultiHit : BuildObjectHtmlTableStyle.None,
                    "unimarc",
                    strRecPath,
                    maps_container);
                //if (string.IsNullOrEmpty(objectTable) == false)
                //    results.Add(new NameValueLine("数字资源", objectTable, "object"));

                // 2018/11/5
                if (string.IsNullOrEmpty(objectTable) == false)
                {
                    var line = new NameValueLine("数字资源", "", "object");
                    line.Xml = objectTable;
                    results.Add(line);
                }
            }

            return 0;
        }

        // 组合构造若干个普通字段内容。针对 UNIMARC 格式。需要为子字段加入前缀符号
        // parameters:
        //      strSubfieldNameList 筛选的子字段名列表。如果为 null，表示不筛选
        static string BuildUnimarcFields(MarcNodeList fields,
            string strSubfieldNameList = null)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (MarcField field in fields)
            {
                if (field.Name == "206")
                    Process206def(field);

                MarcNodeList nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    StringBuilder temp = new StringBuilder();
                    foreach (MarcSubfield subfield in nodes)
                    {
                        if (subfield.Name == "6")
                            continue;

                        if (strSubfieldNameList != null)
                        {
                            if (strSubfieldNameList.IndexOf(subfield.Name) == -1)
                                continue;
                        }

                        PrePostfix prefix = GetUnimarcPrePostfix(subfield);
                        if (prefix != null)
                        {
                            string current = prefix.Prefix
                                + RemovePrefixChar(subfield.Content, prefix.Prefix)
                                + prefix.Postfix;
                            // 上次的末尾和这次的头部，只允许有一个空格。多余的空格要舍掉
                            if (HasTailBlank(temp) == true
                                && string.IsNullOrEmpty(current) == false && current[0] == ' ')
                                current = current.Substring(1);
                            temp.Append(current);
                        }
                    }

                    if (temp.Length > 0)
                    {
                        if (i > 0)
                            text.Append(CRLF);
                        if (field.Name == "225")
                            text.Append((Left_Parentheses + temp.ToString().Trim() + Right_Parentheses).Trim());
                        else
                            text.Append(temp.ToString().Trim());
                        i++;
                    }
                }
            }

            return text.ToString().Trim();
        }

        static void Process206def(MarcField field)
        {
            bool bRange = false;
            // 把 $d$e$f 聚集在一起
            foreach (MarcSubfield subfield in field.ChildNodes)
            {
                if (subfield.Name == "d" || subfield.Name == "e" || subfield.Name == "f")
                {
                    if (bRange == false)
                    {
                        subfield.Content = " (" + subfield.Content;
                        bRange = true;  // 表示进入了连续 $d$e$f 范围
                    }
                    else
                    {
                        string strPrefix = " ";
                        if (subfield.Name == "f")
                            strPrefix = " ; ";
                        subfield.Content = strPrefix + subfield.Content;
                    }
                }
                else
                {
                    if (bRange)
                    {
                        subfield.Content += ") ";
                        bRange = false;
                    }
                }
            }

            if (bRange)
                field.ChildNodes[field.ChildNodes.count - 1].Content += ")";
        }

        const string Left_Parentheses = " (";
        const string Right_Parentheses = ") ";


        static bool HasTailBlank(StringBuilder text)
        {
            if (text.Length == 0)
                return false;
            if (text[text.Length - 1] == ' ')
                return true;
            return false;
        }

        static bool HasLeadBlank(StringBuilder text)
        {
            if (text.Length == 0)
                return false;
            if (text[0] == ' ')
                return true;
            return false;
        }

        static bool HasLeadChar(string strText, char ch)
        {
            if (string.IsNullOrEmpty(strText))
                return false;
            if (strText[0] == ch)
                return true;
            return false;
        }

        // 去掉可能的前缀字符
        static string RemovePrefixChar(string strContent, string strPrefix)
        {
            if (string.IsNullOrEmpty(strPrefix))
                return strContent;
            strPrefix = strPrefix.Trim();
            if (string.IsNullOrEmpty(strPrefix))
                return strContent;
            string strSave = strContent.TrimStart();    // 去掉左边可能的空格字符，并保留这个结果
            string strResult = strSave.TrimStart(new char[] { strPrefix[0] });
            if (strSave == strResult)
                return strContent;  // 没有匹配的前缀字符，返回原始字符串
            return strResult.TrimStart();   // 返回去除了前缀连同空格的字符串
        }

        public class PrePostfix
        {
            public string Prefix { get; set; }
            public string Postfix { get; set; }

            public PrePostfix(string prefix)
            {
                this.Prefix = prefix;
            }

            public PrePostfix(string prefix, string postfix)
            {
                this.Prefix = prefix;
                this.Postfix = postfix;
            }
        }

        // 从对照表中检索出前缀字符串
        static string GetPrefix(string[] relations,
            string subfield_name)
        {
            foreach (string relation in relations)
            {
                string left = "";
                string right = "";
                int nRet = relation.IndexOf("|");
                if (nRet == -1)
                    left = relation;
                else
                {
                    left = relation.Substring(0, nRet);
                    right = relation.Substring(nRet + 1);
                }

                if (left == subfield_name)
                    return right;
            }

            return "";  // not found
        }

        // 获得一个 UNIMARC 子字段的(ISBD)前缀字符串
        static PrePostfix GetUnimarcPrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            if (field.Name == "200")
                return GetUnimarc_200_PrePostfix(subfield);
            if (field.Name == "205")
                return GetUnimarc_205_PrePostfix(subfield);
            if (field.Name == "206")
                return GetUnimarc_206_PrePostfix(subfield);
            if (field.Name == "207" || field.Name == "208")
                return GetUnimarc_207_208_PrePostfix(subfield);
            if (field.Name == "210")
                return GetUnimarc_210_PrePostfix(subfield);
            if (field.Name == "215")
                return GetUnimarc_215_PrePostfix(subfield);
            if (field.Name == "225")
                return GetUnimarc_225_PrePostfix(subfield);
            if (field.Name == "600" || field.Name == "601"
                || field.Name == "606" || field.Name == "610")
                return GetUnimarc_6xx_PrePostfix(subfield);
            if (field.Name == "690" || field.Name == "692"
    || field.Name == "694")
                return GetUnimarc_69x_PrePostfix(subfield);
            if (field.Name == "300" || field.Name == "304"
|| field.Name == "312" || field.Name == "314" || field.Name == "320" || field.Name == "324"
|| field.Name == "326" || field.Name == "327" || field.Name == "330")
                return GetUnimarc_3xx_PrePostfix(subfield);
            if (StringUtil.IsInList(field.Name, "010,011,013,015,016,091"))
                return GetUnimarc_0xx_PrePostfix(subfield);
            if (StringUtil.IsInList(field.Name, "500,501,503,512,513,514,515,516,520,530,531,532,540,541"))
                return GetUnimarc_5xx_PrePostfix(subfield);

            return new PrePostfix("?");
        }

        static string[] unimarc_200_relations = new string[] {
                "c|. ",
                "d| = ",
                "e| : ",
                "f| / ",
                "g| ; ",
                "h|. ",
            };

        static PrePostfix GetUnimarc_200_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            if (field.Name == "200")
            {
                switch (subfield.Name)
                {
                    case "a":
                        if (subfield.PrevSibling == null)
                            return new PrePostfix("");
                        if (subfield.PrevSibling.Name == "f" || subfield.PrevSibling.Name == "g")
                            return new PrePostfix(". ");
                        return new PrePostfix(" ; ");
                    case "b":
                        return new PrePostfix(" [", "] ");
                    case "c":
                    case "d":
                    case "e":
                    case "h":
                        return new PrePostfix(GetPrefix(unimarc_200_relations, subfield.Name));
                    case "i":
                        if (subfield.PrevSibling != null
                            && (subfield.PrevSibling.Name == "h" || subfield.PrevSibling.Name == "H"))
                            return new PrePostfix(", ");
                        return new PrePostfix(". ");
                    case "f":
                        if (subfield.PrevSibling != null && subfield.PrevSibling.Name == "f")
                        {
                            if (subfield.Content.TrimStart().StartsWith("="))
                            {
                                // 去除子字段内容中可能出现的 = 前缀
                                subfield.Content = subfield.Content.TrimStart(new char[] { '=' });
                                return new PrePostfix(" = ");
                            }
                            return new PrePostfix(" ; ");
                        }
                        return new PrePostfix(" / ");
                    case "g":
                        if (subfield.PrevSibling != null && subfield.PrevSibling.Name == "g")
                        {
                            if (subfield.Content.TrimStart().StartsWith("="))
                            {
                                // 去除子字段内容中可能出现的 = 前缀
                                subfield.Content = subfield.Content.TrimStart(new char[] { '=' });
                                return new PrePostfix(" = ");
                            }
                            return new PrePostfix(" ; ");
                        }
                        return new PrePostfix(" ; ");

                }
            }

            return null;
        }

        static string[] unimarc_205_relations = new string[] {
            "b|, ",
                "d| = ",
                "f| / ",
                "g| ; ",
            };

        static PrePostfix GetUnimarc_205_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            if (field.Name == "205")
            {
                switch (subfield.Name)
                {
                    case "a":
                        if (subfield.PrevSibling == null)
                            return new PrePostfix("");
                        return new PrePostfix(" ; ");
                    case "b":
                    case "d":
                    case "f":
                    case "g":
                        return new PrePostfix(GetPrefix(unimarc_205_relations, subfield.Name));
                }
            }

            return null;
        }

        static PrePostfix GetUnimarc_206_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            if (field.Name == "206")
            {
                switch (subfield.Name)
                {
                    case "a":
                        if (subfield.PrevSibling == null)
                            return new PrePostfix("");
                        return new PrePostfix(" ; ");
                    case "b":
                        if (subfield.PrevSibling == null)
                            return new PrePostfix("");
                        return new PrePostfix(". ");
                    case "c":
                        return new PrePostfix(" ; ");

                    // 注: $d$e$f 已经预处理过了，包含了标点符号在内容中
                    case "d":
                    case "e":
                    case "f":
                        return new PrePostfix("");
                }
            }

            return null;
        }


        static PrePostfix GetUnimarc_207_208_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            if (field.Name == "207" || field.Name == "208")
            {
                switch (subfield.Name)
                {
                    case "a":
                        if (subfield.PrevSibling == null)
                            return new PrePostfix("");
                        return new PrePostfix(" ; ");
                    case "d":
                        return new PrePostfix(" = ");
                }
            }

            return null;
        }

        static string[] unimarc_210_relations = new string[] {
            "c| : ",
                "d|, ",
                "e|" + Left_Parentheses,
                "g| : ",
                "h|, "
            };

        static PrePostfix GetUnimarc_210_PrePostfix(MarcSubfield subfield)
        {
            string strPostfix = "";
            MarcField field = subfield.Parent as MarcField;
            if (field.Name == "210")
            {
                if (subfield.NextSibling == null
                    && subfield.Parent.select("subfield[@name='e']").count > 0)
                    strPostfix = Right_Parentheses;

                switch (subfield.Name)
                {
                    case "a":
                        if (subfield.PrevSibling == null)
                            return new PrePostfix("");
                        return new PrePostfix(" ; ");
                    case "c":
                    case "d":
                    case "e":
                    case "g":
                    case "h":
                        return new PrePostfix(GetPrefix(unimarc_210_relations, subfield.Name), strPostfix);
                }
            }

            return null;
        }

        static string[] unimarc_215_relations = new string[] {
            "c| : ",
                "d| ; ",
                "e| + ",
            };

        static PrePostfix GetUnimarc_215_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            if (field.Name == "215")
            {
                switch (subfield.Name)
                {
                    case "a":
                        if (subfield.PrevSibling == null)
                            return new PrePostfix("");
                        return new PrePostfix(" ; ");
                    case "c":
                    case "d":
                    case "e":
                        return new PrePostfix(GetPrefix(unimarc_215_relations, subfield.Name));
                }
            }

            return null;
        }

        static string[] unimarc_225_relations = new string[] {
                "d| = ",
                "e| : ",
                "f| / ",
                "h|. ",
                "v| ; ",
                "x|, ",
            };

        static PrePostfix GetUnimarc_225_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            if (field.Name == "225")
            {
                switch (subfield.Name)
                {
                    case "a":
                        if (subfield.PrevSibling == null)
                            return new PrePostfix("");
                        return new PrePostfix(" ; ");
                    case "d":
                    case "e":
                    case "f":
                    case "h":
                    case "v":
                    case "x":
                        return new PrePostfix(GetPrefix(unimarc_225_relations, subfield.Name));
                    case "i":
                        if (subfield.PrevSibling != null
                            && (subfield.PrevSibling.Name == "h" || subfield.PrevSibling.Name == "H"))
                            return new PrePostfix(", ");
                        return new PrePostfix(". ");
                }
            }

            return null;
        }

        static PrePostfix GetUnimarc_6xx_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            if (field.Name == "600" || field.Name == "601"
                || field.Name == "606" || field.Name == "610")
            {
                switch (subfield.Name)
                {
                    case "a":
                        if (subfield.PrevSibling == null)
                            return new PrePostfix("");
                        return new PrePostfix(" ; ");
                    case "b":
                    case "f":
                        return new PrePostfix("--");
                    case "j":
                    case "x":
                    case "y":
                    case "z":
                        return new PrePostfix("-");
                }
            }

            return null;
        }

        static PrePostfix GetUnimarc_69x_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            string strClassification = "";
            if (field.Name == "690")
                strClassification = "中图法分类号";
            if (field.Name == "692")
                strClassification = "科图法分类号";
            if (field.Name == "694")
                strClassification = "人大法分类号";

            if (string.IsNullOrEmpty(strClassification) == false)
            {
                switch (subfield.Name)
                {
                    case "a":
                        return new PrePostfix(strClassification + ": ");
                }
            }

            return null;
        }

        static PrePostfix GetUnimarc_3xx_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            string strClassification = "";
            if (field.Name == "300")
                strClassification = "一般性附注";
            if (field.Name == "304")
                strClassification = "题名责任说明附注";
            if (field.Name == "312")
                strClassification = "相关题名附注";
            if (field.Name == "314")
                strClassification = "知识责任附注";
            if (field.Name == "320")
                strClassification = "书目索引附注";
            if (field.Name == "324")
                strClassification = "复制品的原作附注";

            if (field.Name == "326")
                strClassification = "连续出版物出版频率附注";
            if (field.Name == "327")
                strClassification = "内容附注(子目)"; // TODO 要添加序号
            if (field.Name == "330")
                strClassification = "提要文摘";

            if (string.IsNullOrEmpty(strClassification) == false)
            {
                switch (subfield.Name)
                {
                    case "a":
                        return new PrePostfix(strClassification + ": ");
                    case "b":
                        return new PrePostfix(" (", ")");
                }
            }

            return null;
        }

        static PrePostfix GetUnimarc_0xx_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            string strTypeName = "";
            if (field.Name == "010")
                strTypeName = "ISBN";
            if (field.Name == "011")
                strTypeName = "ISSN";
            if (field.Name == "013")
                strTypeName = "ISMN";
            if (field.Name == "015")
                strTypeName = "ISRN";
            if (field.Name == "016")
                strTypeName = "ISRC";
            if (field.Name == "091")
                strTypeName = "统一书刊号";

            if (string.IsNullOrEmpty(strTypeName) == false)
            {
                switch (subfield.Name)
                {
                    case "a":
                        return new PrePostfix(strTypeName + " ");
                    case "b":
                        return new PrePostfix(Left_Parentheses, Right_Parentheses);
                    case "d":
                        return new PrePostfix(" : ");
                    case "y":
                    case "Y":
                        return new PrePostfix("(失效的)" + strTypeName + ":");
                    case "z":
                    case "Z":
                        return new PrePostfix("(错误的)" + strTypeName + ":");
                }
            }

            return null;
        }

        static PrePostfix GetUnimarc_5xx_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            string strTypeName = "";
            if (field.Name == "500")
                strTypeName = "统一题名";
            if (field.Name == "501")
                strTypeName = "作品集统一题名";
            if (field.Name == "503")
                strTypeName = "统一惯用标目";
            if (field.Name == "512")
                strTypeName = "封面题名";
            if (field.Name == "513")
                strTypeName = "附加题名页题名";
            if (field.Name == "514")
                strTypeName = "卷端题名";
            if (field.Name == "515")
                strTypeName = "逐页题名";
            if (field.Name == "516")
                strTypeName = "书脊题名";
            if (field.Name == "520")
                strTypeName = "前题名";
            if (field.Name == "530")
                strTypeName = "识别题名";
            if (field.Name == "531")
                strTypeName = "缩略题名";
            if (field.Name == "532")
                strTypeName = "完整题名";
            if (field.Name == "540")
                strTypeName = "编目员补充的附加题名";
            if (field.Name == "541")
                strTypeName = "编目员补充的翻译题名";

            if (string.IsNullOrEmpty(strTypeName) == false)
            {
                switch (subfield.Name)
                {
                    case "a":
                        return new PrePostfix(strTypeName + ": ");
                }
            }

            return null;
        }

        static string[] unimarc_7xx_relations = new string[] {
            "b|, ",
            "d|: ",
            "e|: ",
                "f|: ",
            };


        static PrePostfix GetUnimarc_7xx_PrePostfix(MarcSubfield subfield)
        {
            MarcField field = subfield.Parent as MarcField;
            string strTypeName = "";
            if (field.Name == "700")
                strTypeName = "个人名称(主要责任)";
            if (field.Name == "701")
                strTypeName = "个人名称(等同责任)";
            if (field.Name == "702")
                strTypeName = "个人名称(次要责任)";
            if (field.Name == "710")
                strTypeName = "团体名称(主要责任)";
            if (field.Name == "711")
                strTypeName = "团体名称(等同责任)";
            if (field.Name == "712")
                strTypeName = "团体名称(次要责任)";
            if (field.Name == "720")
                strTypeName = "家族名称(主要责任)";
            if (field.Name == "721")
                strTypeName = "家族名称(等同责任)";
            if (field.Name == "722")
                strTypeName = "家族名称(次要责任)";

            if (field.Name == "716")
                strTypeName = "商标";
            if (field.Name == "730")
                strTypeName = "名称(责任实体)";

            if (field.Name == "740")
                strTypeName = "法律和宗教文本统一惯用标目(主要责任)";
            if (field.Name == "741")
                strTypeName = "法律和宗教文本统一惯用标目(等同责任)";
            if (field.Name == "742")
                strTypeName = "法律和宗教文本统一惯用标目(次要责任)";

            if (string.IsNullOrEmpty(strTypeName) == false)
            {
                switch (subfield.Name)
                {
                    case "a":
                        return new PrePostfix(strTypeName + ": ");
                    case "b":
                    case "c":
                    case "d":
                    case "e":
                    case "f":
                    case "g":
                    case "h":
                    case "i":
                    case "l":
                    case "n":
                    case "p":
                    case "t":
                        return new PrePostfix(GetPrefix(unimarc_7xx_relations, subfield.Name));
                }
            }

            return null;
        }


        #endregion

        public static int ScriptMarc21(
            string strRecPath,
            string strMARC,
            string strStyle,
            XmlElement maps_container,
            out List<NameValueLine> results,
            out string strError)
        {
            strError = "";
            results = new List<NameValueLine>();

            MarcRecord record = new MarcRecord(strMARC);

            if (record.ChildNodes.count == 0)
                return 0;

            string strImageUrl = ScriptUtil.GetCoverImageUrl(strMARC, "LargeImage");    // LargeImage
            if (string.IsNullOrEmpty(strImageUrl) == false)
                results.Add(new NameValueLine("_coverImage", strImageUrl,
                    "coverimageurl" // 2019/7/19 添加
                    ));

            // LC control no.
            MarcNodeList nodes = record.select("field[@name='010']/subfield[@name='a']");
            if (nodes.count > 0)
            {
                results.Add(new NameValueLine("LC control no.", nodes[0].Content.Trim()));
            }

            // Type of material
            results.Add(new NameValueLine("Type of material", GetMaterialType(record)));

            // Personal name
            MarcNodeList fields = record.select("field[@name='100']");
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    results.Add(new NameValueLine("Personal name", ConcatSubfields(nodes), "author"));
                }
            }

            // Corporate name
            fields = record.select("field[@name='110']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Corporate name", BuildFields(fields)));
            }

            // Uniform title
            fields = record.select("field[@name='240']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Uniform title", BuildFields(fields)));
            }

            // Main title
            fields = record.select("field[@name='245']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Main title", BuildFields(fields), "title_area")); // 原 title
            }

            // TODO: 选择除了著者以外的子字段，构成题名字符串
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Title", BuildFields(fields, "abhnp"), "title"));
            }
#if NO
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    results.Add(new OneLine("Main title", ConcatSubfields(nodes)));
                }
            }
#endif

            // Portion of title
            fields = record.select("field[@name='246' and @indicator2='0']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Portion of title", BuildFields(fields)));
            }

            // Spine title
            fields = record.select("field[@name='246' and @indicator2='8']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Spine title", BuildFields(fields)));
            }

            // Edition
            fields = record.select("field[@name='250']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Edition", BuildFields(fields), "edition_area"));
            }

            // Published/Created
            fields = record.select("field[@name='260']");
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    results.Add(new NameValueLine("Published / Created", ConcatSubfields(nodes), "publication_area"));  // 原"publisher"。附加的空格便于在 HTML 中自然折行
                }
            }

            // Related names
            fields = record.select("field[@name='700' or @name='710' or @name='711']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Related names", BuildFields(fields), "author"));
            }

            // Related titles
            fields = record.select("field[@name='730' or @name='740']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Related titles", BuildFields(fields)));
            }

            // Description
            fields = record.select("field[@name='300' or @name='362']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Description", BuildFields(fields)));
            }

            // ISBN
            fields = record.select("field[@name='020']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("ISBN", BuildFields(fields), "isbn"));
            }

            // Current frequency
            fields = record.select("field[@name='310']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Current frequency", BuildFields(fields)));
            }

            // Former title
            fields = record.select("field[@name='247']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Former title", BuildFields(fields)));
            }

            // Former frequency
            fields = record.select("field[@name='321']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Former frequency", BuildFields(fields)));
            }

            // Continues
            fields = record.select("field[@name='780']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Continues", BuildFields(fields)));
            }

            // ISSN
            MarcNodeList subfields = record.select("field[@name='022']/subfield[@name='a']");
            if (subfields.count > 0)
            {
                results.Add(new NameValueLine("ISSN", ConcatSubfields(subfields), "issn"));
            }

            // Linking ISSN
            subfields = record.select("field[@name='022']/subfield[@name='l']");
            if (subfields.count > 0)
            {
                results.Add(new NameValueLine("Linking ISSN", ConcatSubfields(subfields)));
            }

            // Invalid LCCN
            subfields = record.select("field[@name='010']/subfield[@name='z']");
            if (subfields.count > 0)
            {
                results.Add(new NameValueLine("Invalid LCCN", ConcatSubfields(subfields)));
            }

            // Contents
            fields = record.select("field[@name='505' and @indicator1='0']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Contents", BuildFields(fields)));
            }

            // Partial contents
            fields = record.select("field[@name='505' and @indicator1='2']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Partial contents", BuildFields(fields)));
            }

            // Computer file info
            fields = record.select("field[@name='538']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Computer file info", BuildFields(fields)));
            }

            // Notes
            fields = record.select("field[@name='500' or @name='501' or @name='504' or @name='561' or @name='583' or @name='588' or @name='590']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Notes", BuildFields(fields)));
            }

            // References
            fields = record.select("field[@name='510']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("References", BuildFields(fields)));
            }

            // Additional formats
            fields = record.select("field[@name='530' or @name='533' or @name='776']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Additional formats", BuildFields(fields)));
            }

            // Subjects
            fields = record.select("field[@name='600' or @name='610' or @name='630' or @name='650' or @name='651']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Subjects", BuildSubjects(fields)));
            }

            // Form/Genre
            fields = record.select("field[@name='655']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Form/Genre", BuildSubjects(fields)));
            }

            // Series
            fields = record.select("field[@name='440' or @name='490' or @name='830']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Series", BuildFields(fields), "series_area"));
            }


            // LC classification
            fields = record.select("field[@name='050']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("LC classification", BuildFields(fields)));
            }
#if NO
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    results.Add(new OneLine("LC classification", ConcatSubfields(nodes)));
                }
            }
#endif

            // NLM class no.
            fields = record.select("field[@name='060']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("NLM class no.", BuildFields(fields)));
            }


            // Dewey class no.
            // 不要 $2
            fields = record.select("field[@name='082']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Dewey class no.", BuildFields(fields, "a")));
            }

            // NAL class no.
            fields = record.select("field[@name='070']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("NAL class no.", BuildFields(fields)));
            }

            // National bib no.
            fields = record.select("field[@name='015']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("National bib no.", BuildFields(fields, "a")));
            }

            // National bib agency no.
            fields = record.select("field[@name='016']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("National bib agency no.", BuildFields(fields, "a")));
            }

            // LC copy
            fields = record.select("field[@name='051']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("LC copy", BuildFields(fields)));
            }

            // Other system no.
            fields = record.select("field[@name='035'][subfield[@name='a']]");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Other system no.", BuildFields(fields, "a")));
            }
#if NO
            fields = record.select("field[@name='035']");
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield[@name='a']");
                if (nodes.count > 0)
                {
                    results.Add(new OneLine("Other system no.", ConcatSubfields(nodes)));
                }
            }
#endif

            // Reproduction no./Source
            fields = record.select("field[@name='037']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Reproduction no./Source", BuildFields(fields)));
            }

            // Geographic area code
            fields = record.select("field[@name='043']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Geographic area code", BuildFields(fields)));
            }

            // Quality code
            fields = record.select("field[@name='042']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Quality code", BuildFields(fields)));
            }

            /*
            // Links
            fields = record.select("field[@name='856'or @name='859']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Links", BuildLinks(fields)));
            }
             * */

            // Content type
            fields = record.select("field[@name='336']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Content type", BuildFields(fields, "a")));
            }

            // Media type
            fields = record.select("field[@name='337']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Media type", BuildFields(fields, "a")));
            }

            // Carrier type
            fields = record.select("field[@name='338']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Carrier type", BuildFields(fields, "a")));
            }

            fields = record.select("field[@name='856' or @name='859']");
            if (fields.count > 0)
            {
                string strXml = ScriptUtil.BuildObjectXmlTable(strMARC,
                    StringUtil.IsInList("object_template", strStyle) ? BuildObjectHtmlTableStyle.Template | BuildObjectHtmlTableStyle.TemplateMultiHit : BuildObjectHtmlTableStyle.None,
                    "usmarc",
                    strRecPath,
                    maps_container);
                if (string.IsNullOrEmpty(strXml) == false)
                {
                    var line = new NameValueLine("Digital Resource", "", "object");
                    line.Xml = strXml;
                    results.Add(line);
                }
            }

            return 0;
        }

        static string GetMaterialType(MarcRecord record)
        {
            if ("at".IndexOf(record.Header[6]) != -1
                && "acdm".IndexOf(record.Header[7]) != -1)
                return "Book";  // Books

            if (record.Header[6] == "m")
                return "Computer Files";

            if ("df".IndexOf(record.Header[6]) != -1)
                return "Map";  // Maps

            if ("cdij".IndexOf(record.Header[6]) != -1)
                return "Music";  // Music

            if ("a".IndexOf(record.Header[6]) != -1
    && "bis".IndexOf(record.Header[7]) != -1)
                return "Periodical or Newspaper";  // Continuing Resources

            if ("gkor".IndexOf(record.Header[6]) != -1)
                return "Visual Material";  // Visual Materials

            if (record.Header[6] == "p")
                return "Mixed Material";    // Mixed Materials

            return "";
        }

        // 直接串联每个子字段的内容
        static string ConcatSubfields(MarcNodeList nodes)
        {
            StringBuilder text = new StringBuilder();
            foreach (MarcNode node in nodes)
            {
                if (node.Name == "6")
                    continue;
                text.Append(node.Content + " ");
            }

            return text.ToString().Trim();
        }

        // 组合构造若干个普通字段内容
        // parameters:
        //      strSubfieldNameList 筛选的子字段名列表。如果为 null，表示不筛选
        static string BuildFields(MarcNodeList fields,
            string strSubfieldNameList = null)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (MarcNode field in fields)
            {
                MarcNodeList nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    StringBuilder temp = new StringBuilder();
                    foreach (MarcNode subfield in nodes)
                    {
                        if (subfield.Name == "6")
                            continue;

                        if (strSubfieldNameList != null)
                        {
                            if (strSubfieldNameList.IndexOf(subfield.Name) == -1)
                                continue;
                        }
                        temp.Append(subfield.Content + " ");
                    }

                    if (temp.Length > 0)
                    {
                        if (i > 0)
                            text.Append(CRLF);
                        text.Append(temp.ToString().Trim());
                        i++;
                    }
                }
            }

            return text.ToString().Trim();
        }

        // 组合构造若干个主题字段内容
        static string BuildSubjects(MarcNodeList fields)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (MarcNode field in fields)
            {
                MarcNodeList nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    if (i > 0)
                        text.Append(CRLF);

                    bool bPrevContent = false;  // 前一个子字段是除了 x y z 以外的子字段
                    StringBuilder temp = new StringBuilder();
                    foreach (MarcNode subfield in nodes)
                    {
                        if (subfield.Name == "6")
                            continue;
                        if (subfield.Name == "2")
                            continue;   // 不使用 $2

                        if (subfield.Name == "x"
                            || subfield.Name == "y"
                            || subfield.Name == "z"
                            || subfield.Name == "v")
                        {
                            temp.Append("--");
                            temp.Append(subfield.Content);
                            bPrevContent = false;
                        }
                        else
                        {
                            if (bPrevContent == true)
                                temp.Append(" ");
                            temp.Append(subfield.Content);
                            bPrevContent = true;
                        }
                    }

                    text.Append(temp.ToString().Trim());
                    i++;
                }
            }

            return text.ToString().Trim();
        }

        // 组合构造若干个856字段内容
        static string BuildLinks(MarcNodeList fields)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (MarcNode field in fields)
            {
                MarcNodeList nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    string u = "";
                    MarcNodeList single = nodes.select("subfield[@name='u']");
                    if (single.count > 0)
                    {
                        u = single[0].Content;
                    }

                    string z = "";
                    single = nodes.select("subfield[@name='z']");
                    if (single.count > 0)
                    {
                        z = single[0].Content;
                    }

                    string t3 = "";
                    single = nodes.select("subfield[@name='3']");
                    if (single.count > 0)
                    {
                        t3 = single[0].Content;
                    }

                    if (i > 0)
                        text.Append(CRLF);

                    StringBuilder temp = new StringBuilder();

                    if (string.IsNullOrEmpty(t3) == false)
                        temp.Append(t3 + ": <|");

                    temp.Append("url:" + u);
                    temp.Append(" text:" + u);
                    if (string.IsNullOrEmpty(z) == false)
                        temp.Append("|>  " + z);

                    text.Append(temp.ToString().Trim());
                    i++;
                }
            }

            return text.ToString().Trim();
        }

    }

    public class NameValueLine
    {
        public string Name = "";
        public string Value = "";
        public string Type = "";
        public string Xml = "";

        public NameValueLine()
        {
        }

        public NameValueLine(string strName, string strValue)
        {
            Name = strName;
            Value = strValue;
        }

        public NameValueLine(string strName, string strValue, string strType)
        {
            Name = strName;
            Value = strValue;
            Type = strType;
        }

        public static List<NameValueLine> FromTableXml(string strXml)
        {
            List<NameValueLine> results = new List<NameValueLine>();
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            XmlNodeList lines = dom.DocumentElement.SelectNodes("line");
            foreach (XmlElement line in lines)
            {
                string name = line.GetAttribute("name");
                string value = line.GetAttribute("value");
                string type = line.GetAttribute("type");
                string xml = line.InnerXml;
                NameValueLine result = new NameValueLine
                {
                    Name = name,
                    Value = value,
                    Type = type,
                    Xml = xml
                };
                results.Add(result);
            }

            return results;
        }

        // 创建 Table Xml
        // parameters:
        //      style   "slim" 表示希望返回一种简单格式，line 元素中去掉了 name 元素
        public static string BuildTableXml(List<NameValueLine> lines, string style)
        {
            bool slim = StringUtil.IsInList("slim", style);
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            foreach (NameValueLine line in lines)
            {
                XmlElement new_line = dom.CreateElement("line");
                dom.DocumentElement.AppendChild(new_line);
                if (slim == false)
                    new_line.SetAttribute("name", line.Name);

                if (string.IsNullOrEmpty(line.Value) == false)
                    new_line.SetAttribute("value", line.Value);

                if (string.IsNullOrEmpty(line.Type) == false)
                    new_line.SetAttribute("type", line.Type);

                if (string.IsNullOrEmpty(line.Xml) == false)
                    new_line.InnerXml = line.Xml;
            }

            return dom.OuterXml;
        }

    }

}
