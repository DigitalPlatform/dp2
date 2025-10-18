using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DigitalPlatform.rms
{
    /// <summary>
    /// 用于生成 MARC 记录浏览格式的类
    /// </summary>
    public static class MarcBrowse
    {
#if NO
        // parameters:
        //      column_list 需要创建的列名数组。如果为 null，表示创建全部列
        public static Dictionary<string, MarcColumn> Build(string strXml,
            List<string> column_list = null)
        {
            Dictionary<string, MarcColumn> results = new Dictionary<string, MarcColumn>();

            string strOutMarcSyntax = "";
            string strMarc = "";
            string strError = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            int nRet = MarcUtil.Xml2Marc(strXml,
                true,
                "",
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
            {
                results.Add("error", new MarcColumn("error", strError));
                return results;
            }

            if (strOutMarcSyntax == "unimarc")
            {
                return BuildUnimarc(strMarc, column_list);
            }

            return results;
        }
#endif
        public static Dictionary<string, MarcColumn> Build(XmlDocument dom,
    List<string> column_list = null)
        {
            Dictionary<string, MarcColumn> results = new Dictionary<string, MarcColumn>();

            string strOutMarcSyntax = "";
            string strMarc = "";
            string strError = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            int nRet = MarcUtil.Xml2Marc(dom,
                true,
                "",
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
            {
                results.Add("error", new MarcColumn("error", strError));
                return results;
            }

            if (strOutMarcSyntax == "unimarc"
                || string.IsNullOrEmpty(strOutMarcSyntax))
            {
                return BuildUnimarc(strMarc, column_list);
            }
            else if (strOutMarcSyntax == "usmarc")
            {
                // results.Add("error", new MarcColumn("error", "尚未支持 USMARC 的 marc filter"));
                // return results;
                return BuildUsmarc(strMarc, column_list);
            }

            return results;
        }

        /* 以下是一个 USMARC 西文书目库的 browse 配置文件范例
<?xml version="1.0" encoding="utf-8"?>
<root>
    <nstable>
        <item prefix="marc" url="http://www.loc.gov/MARC21/slim" />
    </nstable>
    <col title="书名">
        <title>
            <caption lang="zh-CN">书名</caption>
            <caption lang="en">Title</caption>
        </title>
        <use>title</use>
    </col>
    <col title="作者">
        <title>
            <caption lang="zh-CN">作者</caption>
            <caption lang="en">Author</caption>
        </title>
        <use>author</use>
    </col>
    <col title="出版者">
        <title>
            <caption lang="zh-CN">出版者</caption>
            <caption lang="en">Publisher</caption>
        </title>
        <use>publisher</use>
    </col>
    <col title="出版时间">
        <title>
            <caption lang="zh-CN">出版时间</caption>
            <caption lang="en">Publish time</caption>
        </title>
        <use>publishtime</use>
    </col>
    <col title="中图法分类号">
        <title>
            <caption lang="zh-CN">中图法分类号</caption>
            <caption lang="en">CLC classification</caption>
        </title>
        <use>clc</use>
    </col>
    <col title="主题词">
        <title>
            <caption lang="zh-CN">主题词</caption>
            <caption lang="en">Subject</caption>
        </title>
        <use>subject</use>
    </col>
    <col title="关键词" convert="join(; )">
        <title>
            <caption lang="zh-CN">关键词</caption>
            <caption lang="en">Keyword</caption>
        </title>
        <xpath nstable="">//marc:record/marc:datafield[@tag='610']/marc:subfield[@code='a']</xpath>
    </col>
    <col title="ISBN">
        <title>
            <caption lang="zh-CN">ISBN</caption>
            <caption lang="en">ISBN</caption>
        </title>
        <use>isbn</use>
    </col>
</root>
         * */

        // USMARC 可用的列名: title, author, publisher, publishtime, clc, ktf, rdf, lcc, nlm, ddc, nal, subject, isbn
        public static Dictionary<string, MarcColumn> BuildUsmarc(string strMarc,
    List<string> column_list_param = null)
        {
            Dictionary<string, MarcColumn> results = new Dictionary<string, MarcColumn>();
            List<string> column_list = new List<string>();
            if (column_list_param != null)
                column_list.AddRange(column_list_param);

            MarcRecord record = new MarcRecord(strMarc);

            // title
            if (column_list_param == null
                || column_list.Remove("title") == true)
            {
                string value = BuildContent(record,
                    "245",
                    new string[] { "a ", "b " },
                    true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("title", new MarcColumn("title", value));
            }

            // author
            if (column_list_param == null
                || column_list.Remove("author") == true)
            {
                string value = BuildContent(record,
    "245",
    new string[] { "c " },
    true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("author", new MarcColumn("author", value));
            }

            // publisher
            if (column_list_param == null
                || column_list.Remove("publisher") == true)
            {
                string value = BuildContent(record,
"264,260",
new string[] { "b " },
true);

                if (string.IsNullOrEmpty(value) == false)
                    results.Add("publisher", new MarcColumn("publisher", value));
            }

            // publishtime
            if (column_list_param == null
                || column_list.Remove("publishtime") == true)
            {
                string value = BuildContent(record,
"264,260",
new string[] { "c " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("publishtime", new MarcColumn("publishtime", value));
            }

            // clc
            if (column_list_param == null
                || column_list.Remove("clc") == true)
            {
                string value = BuildContent(record,
"093",
new string[] { "a " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("clc", new MarcColumn("clc", value));
            }

            // ktf
            if (column_list_param == null
    || column_list.Remove("ktf") == true)
            {
                string value = BuildContent(record,
"094",
new string[] { "a " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("ktf", new MarcColumn("ktf", value));
            }

            // rdf
            if (column_list_param == null
|| column_list.Remove("rdf") == true)
            {
                string value = BuildContent(record,
"095",
new string[] { "a " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("rdf", new MarcColumn("rdf", value));
            }

            // lcc
            if (column_list_param == null
|| column_list.Remove("lcc") == true)
            {
                string value = BuildContent(record,
"050",
new string[] { "a " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("lcc", new MarcColumn("lcc", value));
            }

            // nlm
            // NLM class no.
            if (column_list_param == null
|| column_list.Remove("nlm") == true)
            {
                string value = BuildContent(record,
"060",
new string[] { "a " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("nlm", new MarcColumn("nlm", value));
            }

            // ddc
            // Dewey class no.
            if (column_list_param == null
|| column_list.Remove("ddc") == true)
            {
                string value = BuildContent(record,
"082",
new string[] { "a " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("ddc", new MarcColumn("ddc", value));
            }

            // nal
            // NAL class no.
            if (column_list_param == null
|| column_list.Remove("nal") == true)
            {
                string value = BuildContent(record,
"070",
new string[] { "a " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("nal", new MarcColumn("nal", value));
            }

            // subject
            // Subjects
            // ("field[@names='600' or @names='610' or @names='630'
            // or @names='650' or @names='651']");
            if (column_list_param == null
                || column_list.Remove("subject") == true)
            {
                string value = BuildContent(record,
"600,610,630,650,651",
new string[] { "a; ", "x--", "y--", "z--" },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("subject", new MarcColumn("subject", value));
            }

            /*
            // ?
            if (column_list_param == null
                || column_list.Remove("keyword") == true)
            {
                string value = BuildContent(record,
"610",
new string[] { "a; " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("keyword", new MarcColumn("keyword", value));
            }
            */

            if (column_list_param == null
                || column_list.Remove("isbn") == true)
            {
                string value = BuildContent(record,
"020",
new string[] { "a " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("isbn", new MarcColumn("isbn", value));
            }

            return results;
        }

        /* 以下是一个 UNIMARC 中文书目库的 browse 配置文件范例
<?xml version="1.0" encoding="utf-8"?>
<root>
    <nstable>
        <item prefix="marc" url="http://dp2003.com/UNIMARC" />
    </nstable>
    <col title="书名">
        <title>
            <caption lang="zh-CN">书名</caption>
            <caption lang="en">Title</caption>
        </title>
        <use>title</use>
    </col>
    <col title="作者">
        <title>
            <caption lang="zh-CN">作者</caption>
            <caption lang="en">Author</caption>
        </title>
        <use>author</use>
    </col>
    <col title="出版者">
        <title>
            <caption lang="zh-CN">出版者</caption>
            <caption lang="en">Publisher</caption>
        </title>
        <use>publisher</use>
    </col>
    <col title="出版时间">
        <title>
            <caption lang="zh-CN">出版时间</caption>
            <caption lang="en">Publish time</caption>
        </title>
        <use>publishtime</use>
    </col>
    <col title="中图法分类号">
        <title>
            <caption lang="zh-CN">中图法分类号</caption>
            <caption lang="en">CLC classification</caption>
        </title>
        <use>clc</use>
    </col>
    <col title="主题词">
        <title>
            <caption lang="zh-CN">主题词</caption>
            <caption lang="en">Subject</caption>
        </title>
        <use>subject</use>
    </col>
    <col title="关键词" convert="join(; )">
        <title>
            <caption lang="zh-CN">关键词</caption>
            <caption lang="en">Keyword</caption>
        </title>
        <xpath nstable="">//marc:record/marc:datafield[@tag='610']/marc:subfield[@code='a']</xpath>
    </col>
    <col title="ISBN">
        <title>
            <caption lang="zh-CN">ISBN</caption>
            <caption lang="en">ISBN</caption>
        </title>
        <use>isbn</use>
    </col>
</root>
        * */
        // UNIMARC 可用的列名: title, author, publisher, publishtime, clc, ktf, rdf, subject, keyword, isbn
        public static Dictionary<string, MarcColumn> BuildUnimarc(string strMarc,
            List<string> column_list_param = null)
        {
            Dictionary<string, MarcColumn> results = new Dictionary<string, MarcColumn>();
            List<string> column_list = new List<string>();
            if (column_list_param != null)
                column_list.AddRange(column_list_param);

            MarcRecord record = new MarcRecord(strMarc);

            // title
            if (column_list_param == null
                || column_list.Remove("title") == true)
            {
                string value = BuildContent(record,
                    "200",
                    new string[] {"a; ", "e: ", "h. ", "i. "},
                    true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("title", new MarcColumn("title", value));
            }

            // author
            if (column_list_param == null
                || column_list.Remove("author") == true)
            {
                string value = BuildContent(record,
    "200",
    new string[] { "f; ", "g; "},
    true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("author", new MarcColumn("author", value));
            }

            // publisher
            if (column_list_param == null
                || column_list.Remove("publisher") == true)
            {
                string value = BuildContent(record,
"210",
new string[] { "c; " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("publisher", new MarcColumn("publisher", value));
            }

            // publishtime
            if (column_list_param == null
                || column_list.Remove("publishtime") == true)
            {
                string value = BuildContent(record,
"210",
new string[] { "d; " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("publishtime", new MarcColumn("publishtime", value));
            }

            // clc
            if (column_list_param == null
                || column_list.Remove("clc") == true)
            {
                string value = BuildContent(record,
"690",
new string[] { "a; " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("clc", new MarcColumn("clc", value));
            }

            // ktf
            if (column_list_param == null
|| column_list.Remove("ktf") == true)
            {
                string value = BuildContent(record,
"692",
new string[] { "a; " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("ktf", new MarcColumn("ktf", value));
            }

            // rdf
            if (column_list_param == null
|| column_list.Remove("rdf") == true)
            {
                string value = BuildContent(record,
"694",
new string[] { "a; " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("rdf", new MarcColumn("rdf", value));
            }

            // subject
            if (column_list_param == null
                || column_list.Remove("subject") == true)
            {
                string value = BuildContent(record,
"606",
new string[] { "a; ", "x--", "y--", "z--" },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("subject", new MarcColumn("subject", value));
            }

            // keyword
            if (column_list_param == null
                || column_list.Remove("keyword") == true)
            {
                string value = BuildContent(record,
"610",
new string[] { "a; " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("keyword", new MarcColumn("keyword", value));
            }

            // isbn
            if (column_list_param == null
                || column_list.Remove("isbn") == true)
            {
                string value = BuildContent(record,
"010",
new string[] { "a; " },
true);
                if (string.IsNullOrEmpty(value) == false)
                    results.Add("isbn", new MarcColumn("isbn", value));
            }

            return results;
        }

        // parameters:
        //      names   字段名列表。用逗号间隔
        static List<MarcField> GetMarcFields(MarcRecord record, string names)
        {
            List<MarcField> results = new List<MarcField>();
            foreach(MarcField field in record.ChildNodes)
            {
                if (StringUtil.IsInList(field.Name,names))
                    results.Add(field);
            }
            return results;
        }

        // parameters:
        //      fieldNames      字段名列表。逗号间隔
        //      subfieldList    字符串数组。每个单元格式如下: "a; " 第一字符表示子字段名，后面若干字符表示要插入的前置符号。
        static string BuildContent(MarcRecord record,
            string fieldNames,
            string[] subfieldList,
            bool trimStart)
        {
            List<char> chars = new List<char>() { ' ' };    // 用于 TrimStart 的字符
            Hashtable prefix_table = new Hashtable();  // names -> prefix 
            foreach (string s in subfieldList)
            {
                string name = s.Substring(0, 1);
                string prefix = s.Substring(1);
                prefix_table[name] = prefix;

                string strChar = prefix.Trim();
                if (string.IsNullOrEmpty(strChar) == false)
                    chars.Add(strChar[0]);
            }

            List<MarcField> fields = GetMarcFields(record, fieldNames);

            StringBuilder text = new StringBuilder();
            foreach (MarcField field in fields)
            {
                foreach (MarcSubfield subfield in field.ChildNodes)
                {
                    bool bExist = prefix_table.ContainsKey(subfield.Name);
                    if (bExist)
                    {
                        string prefix = (string)prefix_table[subfield.Name];
                        if (string.IsNullOrEmpty(prefix) == false)
                            text.Append(prefix + subfield.Content);
                        else
                            text.Append(" " + subfield.Content);
                    }
                }
            }
            if (trimStart)
                return text.ToString().TrimStart(chars.ToArray());
            return text.ToString();
        }

#if NO
        // parameters:
        //      subfieldList    字符串数组。每个单元格式如下: "a; " 第一字符表示子字段名，后面若干字符表示要插入的前置符号。
        static string BuildContent(MarcRecord record,
            string fieldName,
            string[] subfieldList,
            bool trimStart)
        {
            List<char> chars = new List<char>() {' '};    // 用于 TrimStart 的字符
            Hashtable prefix_table = new Hashtable();  // name -> prefix 
            StringBuilder xpath = new StringBuilder();
            xpath.Append("field[@name='200']/subfield[");
            int i = 0;
            foreach (string s in subfieldList)
            {
                string name = s.Substring(0, 1);
                string prefix = s.Substring(1);
                prefix_table[name] = prefix;

                string strChar = prefix.Trim();
                if (string.IsNullOrEmpty(strChar) == false)
                    chars.Add(strChar[0]);

                if (i > 0)
                    xpath.Append(" or ");
                xpath.Append("@name='" + name + "'");
                i++;
            }
            xpath.Append("]");

            MarcNodeList subfields = record.select(xpath.ToString());
            StringBuilder text = new StringBuilder();
            foreach (MarcSubfield subfield in subfields)
            {
                string prefix = (string)prefix_table[subfield.Name];
                if (string.IsNullOrEmpty(prefix) == false)
                    text.Append(prefix + subfield.Content);
                else
                    text.Append(" " + subfield.Content);
            }
            if (trimStart)
                return text.ToString().TrimStart(chars.ToArray());
            return text.ToString();
        }

#endif
    }

    public class MarcColumn
    {
        public string ColumnName { get; set; }
        public string Value { get; set; }

        public MarcColumn(string name, string value)
        {
            this.ColumnName = name;
            this.Value = value;
        }
    }
}
