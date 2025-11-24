using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DigitalPlatform.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DigitalPlatform.LibraryServer
{
    [TestClass]
    public class TestReaderAccess
    {
        // 没有存取定义
        [TestMethod]
        public void AccessReaderRange_01()
        {
            string access = "";
            string xml = @"<root>
<barcode>R0000001</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";

            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(2, ret);
        }

        // 存取定义中没有匹配 reader 操作的部分
        [TestMethod]
        public void AccessReaderRange_02()
        {
            string access = "*:getbiblioinfo";
            string xml = @"<root>
<barcode>R0000001</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";

            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(2, ret);
        }

        // 读者记录中 name 元素内容不匹配
        [TestMethod]
        public void AccessReaderRange_03()
        {
            string access = "读者库:reader=name(姓名111)";
            string xml = @"<root>
<barcode>R0000001</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(1, ret);
        }

        // 读者记录中 name 元素内容匹配
        [TestMethod]
        public void AccessReaderRange_04()
        {
            string access = "读者库:reader=name(姓名)";
            string xml = @"<root>
<barcode>R0000001</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(0, ret);
        }

        // 正则表达式
        [TestMethod]
        public void AccessReaderRange_05()
        {
            string access = "读者库:reader=barcode(@^\\d{7}$)";
            string xml = @"<root>
<barcode>0000001</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(0, ret);
        }

        // 正则表达式
        [TestMethod]
        public void AccessReaderRange_06()
        {
            string access = "读者库:reader=barcode(@^\\d{7}$)";
            string xml = @"<root>
<barcode>00000010</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(1, ret);
        }

        // 通配符
        [TestMethod]
        public void AccessReaderRange_07()
        {
            string access = "读者库:reader=name(姓*)";
            string xml = @"<root>
<barcode>00000010</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            int correct_ret = 0;
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(correct_ret, ret);
        }

        // 通配符
        [TestMethod]
        public void AccessReaderRange_08()
        {
            string access = "读者库:reader=name(姓?)";
            string xml = @"<root>
<barcode>00000010</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            int correct_ret = 0;

            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(correct_ret, ret);
        }

        // 读者记录中多个元素判断
        [TestMethod]
        public void AccessReaderRange_09()
        {
            string access = "读者库:reader=barcode(1*),name=(姓名)";
            string xml = @"<root>
<barcode>00000010</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            int correct_ret = 1;    // 不匹配

            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(correct_ret, ret);
        }

        // 读者记录中多个元素判断
        [TestMethod]
        public void AccessReaderRange_10()
        {
            string access = "读者库:reader=barcode(0*),name=(姓?)";
            string xml = @"<root>
<barcode>00000010</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            int correct_ret = 0;    // 匹配

            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(correct_ret, ret);
        }

        // 圆括号部分为空
        [TestMethod]
        public void AccessReaderRange_11()
        {
            string access = "读者库:reader=name()";
            string xml = @"<root>
<barcode>R0000001</barcode>
<name>姓名</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            int correct_ret = 1;
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(correct_ret, ret);
        }

        // 圆括号部分为空
        [TestMethod]
        public void AccessReaderRange_12()
        {
            string access = "读者库:reader=name()";
            string xml = @"<root>
<barcode>R0000001</barcode>
<name></name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            int correct_ret = 0;
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(correct_ret, ret);
        }

        // 圆括号部分为星号
        [TestMethod]
        public void AccessReaderRange_13()
        {
            string access = "读者库:reader=name(*)";
            string xml = @"<root>
<barcode>R0000001</barcode>
<name></name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            int correct_ret = 0;
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(correct_ret, ret);
        }


        // 转义字符
        [TestMethod]
        public void AccessReaderRange_14()
        {
            // string escaped = StringUtil.EscapeString("()", "()");
            string escaped = "%28%29";
            string access = $"读者库:reader=name({escaped})";
            string xml = @"<root>
<barcode>R0000001</barcode>
<name>()</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            int correct_ret = 0;
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(correct_ret, ret);
        }

        // 转义字符
        // 按理说 % 本身要用 %25 表示才行
        [TestMethod]
        public void AccessReaderRange_15()
        {
            string escaped = StringUtil.EscapeString("%", "%");
            string access = $"读者库:reader=name({escaped})";
            string xml = @"<root>
<barcode>R0000001</barcode>
<name>%</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            int correct_ret = 0;
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(correct_ret, ret);
        }

        // 健壮性。直接用 % 也是可以的
        [TestMethod]
        public void AccessReaderRange_17()
        {
            string escaped = StringUtil.EscapeString("%", "%");
            string access = $"读者库:reader=name(%)";
            string xml = @"<root>
<barcode>R0000001</barcode>
<name>%</name>
<department>数学系</department>
</root>";
            string reader_dbname = "读者库";
            int correct_ret = 0;
            // return:
            //      -1  出错
            //      0   允许继续访问
            //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
            //      2   没有定义相关的存取定义参数
            var ret = LibraryApplication.AccessReaderRange(
            access,
            DOM(xml),
            reader_dbname,
            out string error);
            Assert.AreEqual(correct_ret, ret);
        }

        static XmlDocument DOM(string xml)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            return dom;
        }

        [TestMethod]
        public void splitString_01()
        {
            string text = "(1)";
            string correct = "(1)";
            var list = StringUtil.SplitString(text,
            ",",
            new string[] { "()" });
            Assert.AreEqual(correct, StringUtil.MakePathList(list, "|"));
        }

        // 简单用法
        [TestMethod]
        public void splitString_02()
        {
            string text = "(1),(2)";
            string correct = "(1)|(2)";
            var list = StringUtil.SplitString(text,
            ",",
            new string[] { "()" });
            Assert.AreEqual(correct, StringUtil.MakePathList(list, "|"));
        }

        // 圆括号保护了逗号
        [TestMethod]
        public void splitString_03()
        {
            string text = "(1,2)";
            string correct = "(1,2)";
            var list = StringUtil.SplitString(text,
            ",",
            new string[] { "()" });
            Assert.AreEqual(correct, StringUtil.MakePathList(list, "|"));
        }

        // 右括号多出一个，似乎对效果没有影响
        [TestMethod]
        public void splitString_04()
        {
            string text = "(1)),2";
            string correct = "(1))|2";
            var list = StringUtil.SplitString(text,
            ",",
            new string[] { "()" });
            Assert.AreEqual(correct, StringUtil.MakePathList(list, "|"));
        }

        // 缺乏最后的右括号。但对效果没有影响
        [TestMethod]
        public void splitString_05()
        {
            string text = "((1),2";
            string correct = "((1),2";
            var list = StringUtil.SplitString(text,
            ",",
            new string[] { "()" });
            Assert.AreEqual(correct, StringUtil.MakePathList(list, "|"));
        }

        // 圆括号不正确配对，切割正确性存疑
        [TestMethod]
        public void splitString_06()
        {
            string text = "(1))(,2";
            string correct = "(1))(,2";
            var list = StringUtil.SplitString(text,
            ",",
            new string[] { "()" });
            Assert.AreEqual(correct, StringUtil.MakePathList(list, "|"));
        }

        [TestMethod]
        public void splitString_07()
        {
            string text = "00(1)22";
            string correct = "00(1)22";
            var list = StringUtil.SplitString(text,
            ",",
            new string[] { "()" });
            Assert.AreEqual(correct, StringUtil.MakePathList(list, "|"));
        }

        [TestMethod]
        public void splitString_08()
        {
            string text = "00(1)22,3";
            string correct = "00(1)22|3";
            var list = StringUtil.SplitString(text,
            ",",
            new string[] { "()" });
            Assert.AreEqual(correct, StringUtil.MakePathList(list, "|"));
        }

    }
}
