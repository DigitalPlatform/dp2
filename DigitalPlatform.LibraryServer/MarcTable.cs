using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DigitalPlatform.Marc;
using DigitalPlatform.Script;

namespace DigitalPlatform.LibraryServer
{
    public static class MarcTable
    {
        static string CRLF = "\n";

        public static int ScriptUnimarc(
            string strRecPath,
            string strMARC,
            out List<NameValueLine> results,
            out string strError)
        {
            strError = "";
            results = new List<NameValueLine>();

            MarcRecord record = new MarcRecord(strMARC);

            if (record.ChildNodes.count == 0)
                return 0;

            // 010
            MarcNodeList fields = record.select("field[@name='010' or @name='011' or @name='091']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("获得方式", BuildFields(fields)));
            }

            return 0;
        }

        public static int ScriptMarc21(
            string strRecPath,
            string strMARC,
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
                results.Add(new NameValueLine("_coverImage", strImageUrl));

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
                    results.Add(new NameValueLine("Personal name", ConcatSubfields(nodes)));
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
                results.Add(new NameValueLine("Main title", BuildFields(fields), "title"));
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
                results.Add(new NameValueLine("Edition", BuildFields(fields)));
            }

            // Published/Created
            fields = record.select("field[@name='260']");
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    results.Add(new NameValueLine("Published / Created", ConcatSubfields(nodes)));  // 附加的空格便于在 HTML 中自然折行
                }
            }

            // Related names
            fields = record.select("field[@name='700' or @name='710' or @name='711']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Related names", BuildFields(fields)));
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
                results.Add(new NameValueLine("ISBN", BuildFields(fields)));
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
                results.Add(new NameValueLine("ISSN", ConcatSubfields(subfields)));
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
            fields = record.select("field[@name='500'  or @name='501' or @name='504' or @name='561' or @name='583' or @name='588' or @name='590']");
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
                results.Add(new NameValueLine("Series", BuildFields(fields)));
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
                string strXml = ScriptUtil.BuildObjectXmlTable(strMARC);
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
            StringBuilder text = new StringBuilder(4096);
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
            StringBuilder text = new StringBuilder(4096);
            int i = 0;
            foreach (MarcNode field in fields)
            {
                MarcNodeList nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    StringBuilder temp = new StringBuilder(4096);
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
            StringBuilder text = new StringBuilder(4096);
            int i = 0;
            foreach (MarcNode field in fields)
            {
                MarcNodeList nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    if (i > 0)
                        text.Append(CRLF);

                    bool bPrevContent = false;  // 前一个子字段是除了 x y z 以外的子字段
                    StringBuilder temp = new StringBuilder(4096);
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
            StringBuilder text = new StringBuilder(4096);
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

                    StringBuilder temp = new StringBuilder(4096);

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
    }

}
