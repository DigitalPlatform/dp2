using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Text;
using static DigitalPlatform.Text.dp2StringUtil;
using DigitalPlatform.Xml;
using System.Xml;

namespace TestDp2Library
{
    /// <summary>
    /// 测试 Unescape() 函数。该函数目前用于 dp2ssl 的 TinyServer.cs 中
    /// </summary>
    [TestClass]
    public class TestString
    {
        [TestMethod]
        public void test_unescape_1()
        {
            var source = "\\r";
            var correct = "\r";

            var result = Unescape(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_unescape_2()
        {
            var source = "\\n";
            var correct = "\n";

            var result = Unescape(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_unescape_3()
        {
            var source = "\\t";
            var correct = "\t";

            var result = Unescape(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_unescape_4()
        {
            var source = "\\\\";
            var correct = "\\";

            var result = Unescape(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_unescape_5()
        {
            var source = "\\*";
            var correct = "*";

            var result = Unescape(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_unescape_6()
        {
            var source = "\\+";
            var correct = "+";

            var result = Unescape(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_unescape_7()
        {
            var source = "\\w";
            var correct = " ";

            var result = Unescape(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_unescape_8()
        {
            var source = "这是空格\\w这是回车\\r\\n";
            var correct = "这是空格 这是回车\r\n";

            var result = Unescape(source);
            Assert.AreEqual(correct, result);
        }

        static string Unescape(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return System.Text.RegularExpressions.Regex.Unescape(text.Replace("\\w", " "));
        }

        [TestMethod]
        public void test_compareVersion_01()
        {
            try
            {
                int ret = StringUtil.CompareVersion("99", "0.02");
                Assert.Fail("应该抛出异常才对");
            }
            catch (Exception ex)
            {
            }
        }

        [TestMethod]
        public void test_compareVersion_02()
        {

            int ret = StringUtil.CompareVersion("99.0", "0.02");
            Assert.IsTrue(ret > 0);
        }

        [TestMethod]
        public void test_ParseBandwidth_01()
        {
            var source = "1024";
            long correct = 1024;

            var result = LibraryServerUtil.ParseBandwidth(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_ParseBandwidth_02()
        {
            var source = "5K";
            long correct = 5 * 1024;

            var result = LibraryServerUtil.ParseBandwidth(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_ParseBandwidth_03()
        {
            var source = "12M";
            long correct = 12 * 1024 * 1024;

            var result = LibraryServerUtil.ParseBandwidth(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_ParseBandwidth_04()
        {
            var source = "100G";
            long correct = (long)100 * 1024 * 1024 * 1024;

            var result = LibraryServerUtil.ParseBandwidth(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_ParseBandwidth_05()
        {
            var source = "100G1";

            try
            {
                var result = LibraryServerUtil.ParseBandwidth(source);
                Assert.Fail("不应该走到这里");
            }
            catch
            {
            }
        }

        // 2024/6/28
        [TestMethod]
        public void test_romanToNumber_01()
        {
            // MCMXCIV --> 1994
            var ret = RomanToNumber.RomanToInt("MCMXCIV");
            Assert.AreEqual(1994, ret);
        }

        // 2024/6/28
        [TestMethod]
        public void test_romanToNumber_02()
        {
            int ret = RomanToNumber.RomanToInt("I");
            Assert.AreEqual(1, ret);

            ret = RomanToNumber.RomanToInt("II");
            Assert.AreEqual(2, ret);

            ret = RomanToNumber.RomanToInt("III");
            Assert.AreEqual(3, ret);

            ret = RomanToNumber.RomanToInt("IV");
            Assert.AreEqual(4, ret);

            ret = RomanToNumber.RomanToInt("V");
            Assert.AreEqual(5, ret);

            ret = RomanToNumber.RomanToInt("VI");
            Assert.AreEqual(6, ret);

            ret = RomanToNumber.RomanToInt("VII");
            Assert.AreEqual(7, ret);

            ret = RomanToNumber.RomanToInt("VIII");
            Assert.AreEqual(8, ret);

            ret = RomanToNumber.RomanToInt("VIIII");
            Assert.AreEqual(9, ret);

            ret = RomanToNumber.RomanToInt("IX");
            Assert.AreEqual(9, ret);

            ret = RomanToNumber.RomanToInt("X");
            Assert.AreEqual(10, ret);

            ret = RomanToNumber.RomanToInt("XI");
            Assert.AreEqual(11, ret);

            ret = RomanToNumber.RomanToInt("XII");
            Assert.AreEqual(12, ret);
        }

        [TestMethod]
        public void test_romanToNumber_10()
        {
            {
                string source = "MCMXCIV, 其余部分";
                string target = "1994, 其余部分";

                var ret = RomanToNumber.ReplaceRomanDigitToNumber(source);
                Assert.AreEqual(target, ret);
            }

            {
                string source = "前导MCMXCIV, 其余部分";
                string target = "前导1994, 其余部分";

                var ret = RomanToNumber.ReplaceRomanDigitToNumber(source);
                Assert.AreEqual(target, ret);
            }

            {
                string source = "前导MCMXCIV";
                string target = "前导1994";

                var ret = RomanToNumber.ReplaceRomanDigitToNumber(source);
                Assert.AreEqual(target, ret);
            }

            {
                string source = "MCMXCIV";
                string target = "1994";

                var ret = RomanToNumber.ReplaceRomanDigitToNumber(source);
                Assert.AreEqual(target, ret);
            }

            {
                string source = "前导mcmxciv, 其余部分";
                string target = "前导1994, 其余部分";

                var ret = RomanToNumber.ReplaceRomanDigitToNumber(source);
                Assert.AreEqual(target, ret);
            }

            {
                string source = "前导viII, 其余部分";
                string target = "前导6II, 其余部分";

                var ret = RomanToNumber.ReplaceRomanDigitToNumber(source);
                Assert.AreEqual(target, ret);
            }


            {
                string source = "没有包含罗马数字";
                string target = "没有包含罗马数字";

                var ret = RomanToNumber.ReplaceRomanDigitToNumber(source);
                Assert.AreEqual(target, ret);
            }
        }

        [TestMethod]
        public void test_macroString_01()
        {
            string source = "{macro}";
            string correct = "macro_value";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_02()
        {
            string source = "1{macro}";
            string correct = "1macro_value";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_03()
        {
            string source = "{macro}2";
            string correct = "macro_value2";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_04()
        {
            string source = "1{macro}2";
            string correct = "1macro_value2";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_05()
        {
            string source = "12{macro}34";
            string correct = "12macro_value34";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_06()
        {
            string source = "{}";
            string correct = "_value";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_07()
        {
            string source = "1{}";
            string correct = "1_value";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_08()
        {
            string source = "{}2";
            string correct = "_value2";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_09()
        {
            string source = "12{}34";
            string correct = "12_value34";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_10()
        {
            string source = "{}{}";
            string correct = "_value_value";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_11()
        {
            string source = "{macro1}{macro2}";
            string correct = "macro1_valuemacro2_value";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_12()
        {
            string source = "{macro1}A{macro2}";
            string correct = "macro1_valueAmacro2_value";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_13()
        {
            string source = "A{macro1}B{macro2}C";
            string correct = "Amacro1_valueBmacro2_valueC";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_14()
        {
            string source = "{macro1}{macro2}B";
            string correct = "macro1_valuemacro2_valueB";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_15()
        {
            string source = "A{macro1}{macro2}";
            string correct = "Amacro1_valuemacro2_value";
            var result = dp2StringUtil.MacroString(source,
    "{",
    "}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_21()
        {
            string source = "A{{macro1}}B{{macro2}}C";
            string correct = "Amacro1_valueBmacro2_valueC";
            var result = dp2StringUtil.MacroString(source,
    "{{",
    "}}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        // 测试畸形的情况
        [TestMethod]
        public void test_macroString_31()
        {
            string source = "{{%year%}";
            string correct = "{{%year%}";
            var result = dp2StringUtil.MacroString(source,
    "{{",
    "}}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_32()
        {
            string source = "{%year%}}";
            string correct = "{%year%}}";
            var result = dp2StringUtil.MacroString(source,
    "{{",
    "}}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_macroString_33()
        {
            string source = "{%year%}";
            string correct = "{%year%}";
            var result = dp2StringUtil.MacroString(source,
    "{{",
    "}}",
    (macro) =>
    {
        return $"{macro}_value";
    });
            Assert.AreEqual(correct, result);
        }

        // 测试 DomUtil.SetElementOuterXml() 函数
        // 原先 DOM 中没有名字为 "dprms:file" 的元素，新设置进去
        [TestMethod]
        public void test_setElementOuterXml_01()
        {
            string origin = "<root><a>123</a></root>";
            string name = "dprms:file";
            string outerxml = "<dprms:file id=\"0\" usage=\"photo\" xmlns:dprms=\"http://dp2003.com/dprms\" />";
            string correct = "<root><a>123</a><dprms:file id=\"0\" usage=\"photo\" xmlns:dprms=\"http://dp2003.com/dprms\" /></root>";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(origin);

            DomUtil.SetElementOuterXml(dom.DocumentElement,
                "http://dp2003.com/dprms",
                name,
                outerxml);
            Assert.AreEqual(correct, dom.DocumentElement.OuterXml);
        }

        // 原先 DOM 中就有名字为 "dprms:file" 的元素，覆盖式设置进去
        [TestMethod]
        public void test_setElementOuterXml_02()
        {
            string origin = "<root><a>123</a><dprms:file id=\"0\" usage=\"photo\" xmlns:dprms=\"http://dp2003.com/dprms\" /></root>";
            string name = "dprms:file";
            string outerxml = "<dprms:file id=\"1\" usage=\"photo\" xmlns:dprms=\"http://dp2003.com/dprms\" />";
            string correct = "<root><a>123</a><dprms:file id=\"1\" usage=\"photo\" xmlns:dprms=\"http://dp2003.com/dprms\" /></root>";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(origin);

            DomUtil.SetElementOuterXml(dom.DocumentElement,
                "http://dp2003.com/dprms",
                name,
                outerxml);
            Assert.AreEqual(correct, dom.DocumentElement.OuterXml);
        }

        // 原先 DOM 中就有名字为 "dprms:file" 的元素，覆盖式设置进去另外一个元素(名字没有前缀)的 OuterXml 内容
        [TestMethod]
        public void test_setElementOuterXml_03()
        {
            string origin = "<root><a>123</a><dprms:file id=\"0\" usage=\"photo\" xmlns:dprms=\"http://dp2003.com/dprms\" /></root>";
            string name = "dprms:file";
            string outerxml = "<file id=\"1\" usage=\"photo\" />";
            string correct = "<root><a>123</a><file id=\"1\" usage=\"photo\" /></root>";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(origin);

            DomUtil.SetElementOuterXml(dom.DocumentElement,
                "http://dp2003.com/dprms",
                name,
                outerxml);
            Assert.AreEqual(correct, dom.DocumentElement.OuterXml);
        }

        // 原先 DOM 中就有名字为 "dprms:file" 的元素，覆盖式设置进去另外一个元素(名字有前缀)的 OuterXml 内容
        [TestMethod]
        public void test_setElementOuterXml_04()
        {
            string origin = "<root><a>123</a><dprms:file id=\"0\" usage=\"photo\" xmlns:dprms=\"http://dp2003.com/dprms\" /></root>";
            string name = "dprms:file";
            string outerxml = "<test:file id=\"1\" usage=\"photo\" xmlns:test=\"http://dp2003.com/test\" />";
            string correct = "<root><a>123</a><test:file id=\"1\" usage=\"photo\" xmlns:test=\"http://dp2003.com/test\" /></root>";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(origin);

            DomUtil.SetElementOuterXml(dom.DocumentElement,
                "http://dp2003.com/dprms",
                name,
                outerxml);
            Assert.AreEqual(correct, dom.DocumentElement.OuterXml);
        }

        // SetElementOuterXml() 用到了 CreateNode() 函数
        [TestMethod]
        public void test_createNode_01()
        {
            string origin = "<root><a>123</a></root>";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(origin);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", "http://dp2003.com/dprms");

            var file_element = DomUtil.CreateNode(dom.DocumentElement,
                new string[] { "dprms:file" },
                nsmgr) as XmlElement;
            Assert.IsTrue(file_element != null);
            Assert.AreEqual("dprms:file", file_element.Name);
        }
    }
}
