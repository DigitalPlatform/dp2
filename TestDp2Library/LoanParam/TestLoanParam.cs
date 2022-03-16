using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.LibraryServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestDp2Library.LoanParam
{
    [TestClass]
    public class TestLoanParam
    {
        // 匹配 location
        [TestMethod]
        public void Test_getBorrowCount_01()
        {
            string @xml = @"<root>
<borrows>
    <borrow barcode='0000001' type='普通图书' location='海淀分馆/阅览室'/>
    <borrow barcode='0000002' type='普通图书' location='海淀分馆/保存本库'/>
    <borrow barcode='0000003' type='普通图书' location='西城分馆/阅览室'/>
    <borrow barcode='0000004' type='普通图书' location='西城分馆/保存本库'/>
</borrows>
</root>";
            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(xml);

            var ret = LibraryApplication.GetBorrowedCount(readerdom,
    "海淀分馆");
            Assert.AreEqual(2, ret);
        }

        // 匹配不上 location
        [TestMethod]
        public void Test_getBorrowCount_02()
        {
            string @xml = @"<root>
<borrows>
    <borrow barcode='0000001' type='普通图书' location='海淀分馆/阅览室'/>
    <borrow barcode='0000002' type='普通图书' location='海淀分馆/保存本库'/>
    <borrow barcode='0000003' type='普通图书' location='西城分馆/阅览室'/>
    <borrow barcode='0000004' type='普通图书' location='西城分馆/保存本库'/>
</borrows>
</root>";
            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(xml);

            var ret = LibraryApplication.GetBorrowedCount(readerdom,
    "");
            Assert.AreEqual(0, ret);
        }

        // 匹配 location 和 bookType
        [TestMethod]
        public void Test_getBorrowCount_10()
        {
            string @xml = @"<root>
<borrows>
    <borrow barcode='0000001' type='普通图书' location='海淀分馆/阅览室'/>
    <borrow barcode='0000002' type='普通图书' location='海淀分馆/保存本库'/>
    <borrow barcode='0000003' type='普通图书' location='西城分馆/阅览室'/>
    <borrow barcode='0000004' type='普通图书' location='西城分馆/保存本库'/>
</borrows>
</root>";
            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(xml);

            var ret = LibraryApplication.GetBorrowedCount(readerdom,
    "海淀分馆",
    "普通图书");
            Assert.AreEqual(2, ret);
        }

        // 匹配不上 location
        [TestMethod]
        public void Test_getBorrowCount_11()
        {
            string @xml = @"<root>
<borrows>
    <borrow barcode='0000001' type='普通图书' location='海淀分馆/阅览室'/>
    <borrow barcode='0000002' type='普通图书' location='海淀分馆/保存本库'/>
    <borrow barcode='0000003' type='普通图书' location='西城分馆/阅览室'/>
    <borrow barcode='0000004' type='普通图书' location='西城分馆/保存本库'/>
</borrows>
</root>";
            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(xml);

            var ret = LibraryApplication.GetBorrowedCount(readerdom,
    "",
    "普通图书");
            Assert.AreEqual(0, ret);
        }


        [TestMethod]
        public void Test_matchReaderType_01()
        {
            string pattern = "普通读者";
            string value = "普通读者";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_02()
        {
            string pattern = "普通读者";
            string value = "本科生";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_03()
        {
            string pattern = "普通*";
            string value = "普通读者";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_04()
        {
            string pattern = "普通*";
            string value = "本科生";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_05()
        {
            string pattern = "海淀分馆/*";
            string value = "海淀分馆/普通读者";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_06()
        {
            string pattern = "*";
            string value = "海淀分馆/普通读者";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_07()
        {
            string pattern = "*/*";
            string value = "海淀分馆/普通读者";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_08()
        {
            string pattern = "*/*";
            string value = "海淀分馆";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_09()
        {
            string pattern = "*/阅览室";
            string value = "海淀分馆/阅览室";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_10()
        {
            string pattern = "????/阅览室";
            string value = "海淀分馆/阅览室";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_11()
        {
            string pattern = "???/阅览室";
            string value = "海淀分馆/阅览室";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_12()
        {
            string pattern = "*/??室";
            string value = "海淀分馆/阅览室";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_13()
        {
            string pattern = "*/*室";
            string value = "海淀分馆/阅览室";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_matchReaderType_14()
        {
            string pattern = "*/阅?室";
            string value = "海淀分馆/阅览室";

            bool ret = DigitalPlatform.LibraryServer.LoanParam.MatchReaderType(pattern, value);
            Assert.AreEqual(true, ret);
        }
    }


}
