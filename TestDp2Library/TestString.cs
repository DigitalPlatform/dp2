using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestDp2Library
{
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
    }


}
