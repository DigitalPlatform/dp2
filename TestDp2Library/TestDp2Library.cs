using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Marc;

namespace TestDp2Library
{
    [TestClass]
    public class LibraryApplicationUnitTest
    {
        [TestMethod]
        public void TestMarcTable_1()
        {
            MarcRecord record = new MarcRecord();
            // 旧记录有三个 856 字段，第一个的权限禁止当前用户访问
            record.add(new MarcField("001A1234567"));
            record.add(new MarcField('$', "2001 $atitle value$fauthor vaue"));

            List<NameValueLine> expect_results = new List<NameValueLine>() {
                new NameValueLine{ Name = "题名", Type = "title", Value = "title value" },
            };

            Test(record.Text,
                "title",
                expect_results);
        }

        public void Test(string strMARC,
            string strStyle,
            List<NameValueLine> expect_results)
        {

            int nRet = MarcTable.ScriptUnimarc(
        "中文图书/1",
        strMARC,
        strStyle,
        out List<NameValueLine> results,
        out string strError);
            if (nRet == -1)
                throw new Exception(strError);

            if (results.Count != expect_results.Count)
                throw new Exception("和期望的 results.Count 不符合。实际=" + results.Count + "  期望=" + expect_results.Count);

            CompareResults(results, expect_results);
        }

        static void CompareResults(List<NameValueLine> results,
            List<NameValueLine> expect_results)
        {
            if (results.Count != expect_results.Count)
                throw new Exception("和期望的 results.Count 不符合。实际=" + results.Count + "  期望=" + expect_results.Count);

            for (int i = 0; i < results.Count; i++)
            {
                NameValueLine line = results[i];
                NameValueLine expect_line = expect_results[i];
                if (ToString(line) != ToString(expect_line))
                    throw new Exception("两个集合不一致(集合元素 " + (i + 1) + "位置)。结果:\r\n" + ToString(line) + "\r\n期望的结果:\r\n" + ToString(expect_line));
            }
        }

        static string ToString(List<NameValueLine> results)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (NameValueLine line in results)
            {
                text.Append((i + 1).ToString() + ")\r\n" + line.ToString());
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
