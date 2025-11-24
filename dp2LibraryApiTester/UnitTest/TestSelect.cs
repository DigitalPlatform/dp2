using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using dp2LibraryApiTester.TestCase;

namespace dp2LibraryApiTester.UnitTest
{
    [TestClass]
    public class TestSelect
    {
        #region 测试 SplitByBraces()

        [TestMethod]
        public void test_splitBraces_01()
        {
            string path = "//{http:/xxx:name}[@name='aaa']";
            string[] correct = new string[] {
            "//",
            "{http:/xxx:name}",
            "[@name='aaa']"
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        [TestMethod]
        public void test_splitBraces_02()
        {
            string path = "//{http:/xxx:name}";
            string[] correct = new string[] {
            "//",
            "{http:/xxx:name}",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        [TestMethod]
        public void test_splitBraces_03()
        {
            string path = "{http:/xxx:name}[@name='aaa']";
            string[] correct = new string[] {
            "{http:/xxx:name}",
            "[@name='aaa']"
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        [TestMethod]
        public void test_splitBraces_04()
        {
            string path = "{http:/xxx:name}";
            string[] correct = new string[] {
            "{http:/xxx:name}",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        [TestMethod]
        public void test_splitBraces_05()
        {
            string path = "{http:/xxx:name";
            string[] correct = new string[] {
            "{http:/xxx:name",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        [TestMethod]
        public void test_splitBraces_06()
        {
            string path = "http:/xxx:name}";
            string[] correct = new string[] {
            "http:/xxx:name}",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));

            try
            {
                Utility.SplitByBraces(path, true);
                Assert.Fail("这里应该抛出异常才对");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void test_splitBraces_11()
        {
            string path = "//{{http:/xxx:name}}[@name='aaa']";
            string[] correct = new string[] {
            "//",
            "{{http:/xxx:name}}",
            "[@name='aaa']"
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        [TestMethod]
        public void test_splitBraces_12()
        {
            string path = "//{{11}22{33}}[@name='aaa']";
            string[] correct = new string[] {
            "//",
            "{{11}22{33}}",
            "[@name='aaa']"
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        [TestMethod]
        public void test_splitBraces_21()
        {
            string path = "";
            string[] correct = new string[] {
            "",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        [TestMethod]
        public void test_splitBraces_22()
        {
            string path = "1";
            string[] correct = new string[] {
            "1",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        [TestMethod]
        public void test_splitBraces_23()
        {
            string path = "{";
            string[] correct = new string[] {
            "{",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));

            try
            {
                Utility.SplitByBraces(path, true);
                Assert.Fail("这里应该抛出异常才对");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void test_splitBraces_24()
        {
            string path = "}";
            string[] correct = new string[] {
            "}",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));

            try
            {
                Utility.SplitByBraces(path, true);
                Assert.Fail("这里应该抛出异常才对");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void test_splitBraces_25()
        {
            string path = "{{";
            string[] correct = new string[] {
            "{{",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));

            try
            {
                Utility.SplitByBraces(path, true);
                Assert.Fail("这里应该抛出异常才对");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void test_splitBraces_26()
        {
            string path = "}}";
            string[] correct = new string[] {
            "}}",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));

            try
            {
                Utility.SplitByBraces(path, true);
                Assert.Fail("这里应该抛出异常才对");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void test_splitBraces_27()
        {
            string path = "}}{{";
            string[] correct = new string[] {
            "}}{{",
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        [TestMethod]
        public void test_splitBraces_28()
        {
            string path = "}}{{1234{5678";
            string[] correct = new string[] {
            "}}{{1234",
            "{5678"
            };
            var result = Utility.SplitByBraces(path, false);
            Assert.IsTrue(result.SequenceEqual(correct));

            try
            {
                Utility.SplitByBraces(path, true);
                Assert.Fail("这里应该抛出异常才对");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        #region 测试 ReplaceNamespaceToPrefix()

        [TestMethod]
        public void test_prefix_01()
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            var path = "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']";
            var result = Utility.ReplaceNamespaceToPrefix(
            nsmgr,
            path);
            Console.WriteLine($"'{path}'\r\n替换结果为\r\n'{result}'");
            AssertReplaceResult(nsmgr,
    result,
    path); 
        }

        [TestMethod]
        public void test_prefix_02()
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            var path = "//{http:://dp2003.com/UNIMARC}:datafield[@tag='997']";
            var result = Utility.ReplaceNamespaceToPrefix(
            nsmgr,
            path);
            Console.WriteLine($"'{path}'\r\n替换结果为\r\n'{result}'");
            AssertReplaceResult(nsmgr,
    result,
    path);
        }


        [TestMethod]
        public void test_prefix_03()
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            var path = "//operations";
            var result = Utility.ReplaceNamespaceToPrefix(
            nsmgr,
            path);
            Console.WriteLine($"'{path}'\r\n替换结果为\r\n'{result}'");
            AssertReplaceResult(nsmgr,
    result,
    path);
        }

        [TestMethod]
        public void test_prefix_04()
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            var path = "operations";
            var result = Utility.ReplaceNamespaceToPrefix(
            nsmgr,
            path);
            Console.WriteLine($"'{path}'\r\n替换结果为\r\n'{result}'");
            AssertReplaceResult(nsmgr,
    result,
    path);
        }

        [TestMethod]
        public void test_prefix_05()
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            var path = "*";
            var result = Utility.ReplaceNamespaceToPrefix(
            nsmgr,
            path);
            Console.WriteLine($"'{path}'\r\n替换结果为\r\n'{result}'");
            AssertReplaceResult(nsmgr,
    result,
    path);
        }

        [TestMethod]
        public void test_prefix_06()
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            var path = "*[@name='001']";
            var result = Utility.ReplaceNamespaceToPrefix(
            nsmgr,
            path);
            Console.WriteLine($"'{path}'\r\n替换结果为\r\n'{result}'");
            AssertReplaceResult(nsmgr,
    result,
    path);
        }

        [TestMethod]
        public void test_prefix_07()
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            var path = "*[@name='001'] | controlfield";
            var result = Utility.ReplaceNamespaceToPrefix(
            nsmgr,
            path);
            Console.WriteLine($"'{path}'\r\n替换结果为\r\n'{result}'");
            AssertReplaceResult(nsmgr,
    result,
    path);
        }

        [TestMethod]
        public void test_prefix_08()
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            var path = "{http://111}:field[@name='001'] | {http://222}:controlfield";

            var result = Utility.ReplaceNamespaceToPrefix(
            nsmgr,
            path);
            Console.WriteLine($"'{path}'\r\n替换结果为\r\n'{result}'");
            AssertReplaceResult(nsmgr,
    result,
    path); 
        }

        static void AssertReplaceResult(XmlNamespaceManager nsmgr,
            string result,
            string path)
        {
            StringBuilder text = new StringBuilder();
            var segments = Utility.SplitByBraces(path, true);
            foreach (var segment in segments)
            {
                if (segment.First() == '{' && segment.Last() == '}')
                {
                    var uri = segment.Substring(1, segment.Length - 2);
                    // 从名字空间管理器中找 uri 对应的 prefix
                    var prefix = nsmgr.LookupPrefix(uri);
                    // prefix 输出用于后继 XPATH 选择之用
                    text.Append(prefix);
                }
                else
                    text.Append(segment.ToString());
            }

            Assert.AreEqual(result, text.ToString());

            // 用 Select() 实际验证 XPATH 是否正确
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                var nodes = dom.DocumentElement.SelectNodes(result, nsmgr);
            }
        }

        static void AssertReplaceResult(XmlNamespaceManager nsmgr,
    string result,
    string correct_uri, // 期望识别出的 namespace uri 部分
    string correct_result_template) // 期望的转换结果(XPATH)模板。因为 prefix 部分未知，所以做成模板形态
        {
            if (string.IsNullOrEmpty(correct_uri))
            {
                Assert.AreEqual(correct_result_template, result);
                return;
            }

            var prefix = nsmgr.LookupPrefix(correct_uri);
            Console.WriteLine($"{correct_uri} 对应的前缀为 '{prefix}'");
            Assert.IsNotNull(prefix);
            var value = correct_result_template.Replace("{prefix}", prefix);
            Assert.AreEqual(value, result);
        }

        #endregion
    }
}
