using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.OPAC.Server
{
    // 负责解析过滤器字符串，并生成相应的查询XML
    /* 过滤器项
     * 语法：*A-B+C
     * 其中，*表示AND，-表示SUB，+表示OR
     * A、B、C分别是结果集名称
     */
    public class QueryFilterItem
    {
        public string Operator { get; set; }
        public string ResultsetName { get; set; }

        public static List<QueryFilterItem> BuildList(string text)
        {
            var results = new List<QueryFilterItem>();
            QueryFilterItem current = null;
            foreach (char ch in text)
            {
                if (ch == '*' || ch == '-' || ch == '+')
                {
                    string strOperator = "AND";
                    if (ch == '*')
                        strOperator = "AND";
                    else if (ch == '-')
                        strOperator = "SUB";
                    else if (ch == '+')
                        strOperator = "OR";

                    current = new QueryFilterItem { Operator = strOperator };
                    results.Add(current);
                }
                else
                {
                    if (current == null)
                    {
                        current = new QueryFilterItem { Operator = "AND" };
                        results.Add(current);
                    }
                    current.ResultsetName += ch;
                }
            }

            return results;
        }

        public static string BuildQueryXml(string strQueryXml,
            List<QueryFilterItem> items)
        {
            StringBuilder text = new StringBuilder();
            text.Append("<group>");
            text.Append(strQueryXml);
            foreach (var item in items)
            {
                text.Append($"<operator value='{item.Operator}'/>");
                text.Append($"<item resultset='#{item.ResultsetName}' />");
            }
            text.Append("</group>");
            // strQueryXml = $"<group>{strQueryXml}<operator value='{strOperator}'/>{strLocationQueryXml}</group>";    // !!!
            return text.ToString();
        }
    }

}
