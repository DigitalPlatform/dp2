using DigitalPlatform.Marc;
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
                results.Add("error", new MarcColumn("error", "尚未支持 USMARC 的 marc filter"));
                return results;
            }

            return results;
        }

        public static Dictionary<string, MarcColumn> BuildUnimarc(string strMarc,
            List<string> column_list_param = null)
        {
            Dictionary<string, MarcColumn> results = new Dictionary<string, MarcColumn>();
            List<string> column_list = new List<string>();
            if (column_list_param != null)
                column_list.AddRange(column_list_param);

            MarcRecord record = new MarcRecord(strMarc);
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

        static List<MarcField> GetMarcFields(MarcRecord record, string name)
        {
            List<MarcField> results = new List<MarcField>();
            foreach(MarcField field in record.ChildNodes)
            {
                if (field.Name == name)
                    results.Add(field);
            }
            return results;
        }

        // parameters:
        //      subfieldList    字符串数组。每个单元格式如下: "a; " 第一字符表示子字段名，后面若干字符表示要插入的前置符号。
        static string BuildContent(MarcRecord record,
            string fieldName,
            string[] subfieldList,
            bool trimStart)
        {
            List<char> chars = new List<char>() { ' ' };    // 用于 TrimStart 的字符
            Hashtable prefix_table = new Hashtable();  // name -> prefix 
            foreach (string s in subfieldList)
            {
                string name = s.Substring(0, 1);
                string prefix = s.Substring(1);
                prefix_table[name] = prefix;

                string strChar = prefix.Trim();
                if (string.IsNullOrEmpty(strChar) == false)
                    chars.Add(strChar[0]);
            }

            List<MarcField> fields = GetMarcFields(record, fieldName);

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
