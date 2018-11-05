using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Marc;

namespace TestDp2Library
{
    /// <summary>
    /// 辅助测试的一些工具函数
    /// </summary>
    public class TestUtility
    {
#if NO
        [TestMethod]
        public void TestMarcTable_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
20010ǂa浙江1979～1988年经济发展报告ǂf浙江十年(1979～1988)经济发展的系统分析课题组编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='责任者' value='浙江十年(1979～1988)经济发展的系统分析课题组编' type='author' />
</root>";

            VerifyTableXml(strWorksheet,
                "author",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_2()
        {
            MarcRecord record = new MarcRecord();
            // 
            record.add(new MarcField("001A1234567"));
            record.add(new MarcField('$', "2001 $atitle value$fauthor value"));

            List<NameValueLine> expect_results = new List<NameValueLine>() {
                new NameValueLine{ Name = "题名与责任者", Type = "title_area", Value = "title value / author value" },
            };

            Test(record.Text,
                "title_area",
                expect_results);
        }
#endif

        public static void VerifyTableXml(string strWorksheet,
            string strStyle,
            string strTableXml)
        {
            MarcRecord record = MarcRecord.FromWorksheet(strWorksheet);
            List<NameValueLine> expect_results = NameValueLine.FromTableXml(strTableXml);

            int nRet = MarcTable.ScriptUnimarc(
        "中文图书/1",
        record.Text,
        strStyle,
        null,
        out List<NameValueLine> results,
        out string strError);
            if (nRet == -1)
                throw new Exception(strError);

            CompareResults(results, expect_results);
        }

        static void CompareResults(List<NameValueLine> results,
            List<NameValueLine> expect_results)
        {
            Debug.WriteLine("\r\n结果:\r\n" + ToString(results) + "\r\n期望的结果:\r\n" + ToString(expect_results));

            if (results.Count != expect_results.Count)
                throw new Exception("和期望的 results.Count 不符合。\r\n实际=" + results.Count + "  期望=" + expect_results.Count);

            for (int i = 0; i < results.Count; i++)
            {
                NameValueLine line = results[i];
                NameValueLine expect_line = expect_results[i];
                if (ToString(line) != ToString(expect_line))
                    throw new Exception("两个集合不一致(集合元素 " + (i + 1) + "位置)。\r\n结果:\r\n" + ToString(line) + "\r\n期望的结果:\r\n" + ToString(expect_line));
            }

        }

        static string ToString(List<NameValueLine> results)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (NameValueLine line in results)
            {
                text.Append((i + 1).ToString() + ")\r\n" + ToString(line));
                i++;
            }

            return text.ToString();
        }

        static string ToString(NameValueLine line)
        {
            StringBuilder text = new StringBuilder();
            text.Append("Name=" + line.Name + "\r\n");
            text.Append("Type=" + line.Type + "\r\n");
            text.Append("Value=" + line.Value + "\r\n");
            text.Append("Xml=" + line.Xml + "\r\n");

            return text.ToString();
        }
    }
}
