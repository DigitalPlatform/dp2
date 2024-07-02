using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Text;

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

    }
}
