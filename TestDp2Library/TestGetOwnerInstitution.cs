using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;

namespace TestDp2Library
{
    [TestClass]
    public class TestGetOwnerInstitution
    {
        // 分馆名字部分匹配上了
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_01()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='海淀分馆/' isil='test' />
            <item map='西城/' alternative='xc' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "海淀分馆/",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("test", isil);
            Assert.AreEqual("", alternative);
        }

        // 分馆名字部分匹配上了，但是房间名字部分没有匹配上
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_02()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='海淀分馆/' isil='test' />
            <item map='西城/' alternative='xc' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "海淀分馆/阅览室",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("test", isil);
            Assert.AreEqual("", alternative);
        }

        [TestMethod]
        public void TestMethod_GetOwnerInstitution_03()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='海淀分馆/' isil='test' />
            <item map='西城/' alternative='xc' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "西城/",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("", isil);
            Assert.AreEqual("xc", alternative);
        }

        // 分馆名字部分匹配上了
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_04()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='海淀分馆/' isil='test' />
            <item map='西城/' alternative='xc' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "西城/阅览室",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("", isil);
            Assert.AreEqual("xc", alternative);
        }

        // '/阅览室' 匹配上了
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_05()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='/' isil='test' />
            <item map='/阅览室' alternative='xc' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "阅览室",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("", isil);
            Assert.AreEqual("xc", alternative);
        }

        // map '/' 匹配上了；map '/阅览室' 没有匹配上
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_06()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='/' isil='test' />
            <item map='/阅览室' alternative='xc' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "流通书库",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("test", isil);
            Assert.AreEqual("", alternative);
        }

        // 用总馆形态去匹配分馆形态的 location (海淀分馆/)，应不匹配
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_07()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='/' isil='test' />
            <item map='/阅览室' alternative='xc' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "海淀分馆/",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(false, bRet);
            Assert.AreEqual("", isil);
            Assert.AreEqual("", alternative);
        }

        [TestMethod]
        public void TestMethod_GetOwnerInstitution_08()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='星洲小学/' isil='CN-0000001-XZ' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "星洲小学/阅览室",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("CN-0000001-XZ", isil);
            Assert.AreEqual("", alternative);
        }

        // 测试 type 属性为 "" 情况
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_09()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item type='' map='海淀分馆/' isil='test' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "海淀分馆/",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(false, bRet);
            Assert.AreEqual("", isil);
            Assert.AreEqual("", alternative);
        }

        // 测试 type 属性缺省 情况
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_10()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='海淀分馆/' isil='test' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "海淀分馆/",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("test", isil);
            Assert.AreEqual("", alternative);
        }


        [TestMethod]
        public void TestMethod_wilcard_GetOwnerInstitution_01()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='星洲小学/第一*' isil='CN-0000001-XZ1' />
		    <item map='星洲小学/第二*' isil='CN-0000001-XZ2' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "星洲小学/第一",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("CN-0000001-XZ1", isil);
            Assert.AreEqual("", alternative);
        }

        [TestMethod]
        public void TestMethod_wilcard_GetOwnerInstitution_02()
        {
            string xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='星洲小学/第一*' isil='CN-0000001-XZ1' />
		    <item map='星洲小学/第二*' isil='CN-0000001-XZ2' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "星洲小学/第二分部",
    "entity",
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("CN-0000001-XZ2", isil);
            Assert.AreEqual("", alternative);
        }


        [TestMethod]
        public void TestMethod_wilcard_GetOwnerInstitution_11()
        {
            string cfg_xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='星洲小学/西部*' isil='CN-0000001-XZX' />
		    <item map='星洲小学/东部*' isil='CN-0000001-XZD' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(cfg_xml);

            string patron_xml =
    @"<root>
        <readerType>普通读者</readerType>
        <department>西部三年级一班</department>
    </root>";
            XmlDocument patron_dom = new XmlDocument();
            patron_dom.LoadXml(patron_xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "星洲小学",
    patron_dom,
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("CN-0000001-XZX", isil);
            Assert.AreEqual("", alternative);
        }

        [TestMethod]
        public void TestMethod_wilcard_GetOwnerInstitution_12()
        {
            string cfg_xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='星洲小学/西部*' isil='CN-0000001-XZX' />
		    <item map='星洲小学/东部*' isil='CN-0000001-XZD' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(cfg_xml);

            string patron_xml =
    @"<root>
        <readerType>普通读者</readerType>
        <department>东部四年级一班</department>
    </root>";
            XmlDocument patron_dom = new XmlDocument();
            patron_dom.LoadXml(patron_xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "星洲小学",
    patron_dom,
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("CN-0000001-XZD", isil);
            Assert.AreEqual("", alternative);
        }

        // 两个 department 都没有匹配上
        [TestMethod]
        public void TestMethod_wilcard_GetOwnerInstitution_13()
        {
            string cfg_xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='星洲小学/西部*' isil='CN-0000001-XZX' />
		    <item map='星洲小学/东部*' isil='CN-0000001-XZD' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(cfg_xml);

            string patron_xml =
    @"<root>
        <readerType>普通读者</readerType>
        <department>五年级一班</department>
    </root>";
            XmlDocument patron_dom = new XmlDocument();
            patron_dom.LoadXml(patron_xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "星洲小学",
    patron_dom,
    out string isil,
    out string alternative);
            Assert.AreEqual(false, bRet);
            Assert.AreEqual("", isil);
            Assert.AreEqual("", alternative);
        }

        [TestMethod]
        public void TestMethod_wilcard_GetOwnerInstitution_14()
        {
            string cfg_xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='星洲小学/西部*' isil='CN-0000001-XZX' />
		    <item map='星洲小学/readerType:普通*' isil='CN-0000001-XZP' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(cfg_xml);

            string patron_xml =
    @"<root>
        <readerType>普通读者</readerType>
        <department>东部四年级一班</department>
    </root>";
            XmlDocument patron_dom = new XmlDocument();
            patron_dom.LoadXml(patron_xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "星洲小学",
    patron_dom,
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("CN-0000001-XZP", isil);
            Assert.AreEqual("", alternative);
        }

        // 匹配读者
        // 用 libraryCode + "/" 匹配上
        [TestMethod]
        public void TestMethod_patron_GetOwnerInstitution_01()
        {
            string cfg_xml =
    @"<rfid>
	    <ownerInstitution>
		    <item map='星洲小学/' isil='CN-0000001-XZ' />
		    <item map='东方小学/' isil='CN-0000001-DF' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(cfg_xml);

            string patron_xml =
    @"<root>
        <readerType>普通读者</readerType>
        <department>四年级一班</department>
    </root>";
            XmlDocument patron_dom = new XmlDocument();
            patron_dom.LoadXml(patron_xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "星洲小学",
    patron_dom,
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("CN-0000001-XZ", isil);
            Assert.AreEqual("", alternative);
        }

        [TestMethod]
        public void TestMethod_patron_GetOwnerInstitution_02()
        {
            string cfg_xml =
    @"<rfid>
	    <ownerInstitution>
		    <item type='patron' map='海淀分馆/特殊$' isil='CN-0000001-HDT' /><!-- 此项用末尾的 $ 压制了默认的 * -->
		    <item type='entity' map='海淀分馆/' isil='CN-0000001-HD' /><!-- 此项排除读者匹配 -->
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(cfg_xml);

            string patron_xml =
    @"<root>
        <readerType>普通读者</readerType>
        <department>特殊四年级一班</department>
    </root>";
            XmlDocument patron_dom = new XmlDocument();
            patron_dom.LoadXml(patron_xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "海淀分馆",
    patron_dom,
    out string isil,
    out string alternative);
            Assert.AreEqual(false, bRet);
            Assert.AreEqual("", isil);
            Assert.AreEqual("", alternative);
        }

        // map 长度较长的 “皇家警察”取胜
        [TestMethod]
        public void TestMethod_patron_GetOwnerInstitution_10()
        {
            string cfg_xml =
    @"<rfid>
	    <ownerInstitution>
		    <item type='patron' map='海淀分馆/readerType:本科生' isil='CN-0000001-DZ' />
		    <item type='patron' map='海淀分馆/皇家警察' isil='CN-0000001-AB' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(cfg_xml);

            string patron_xml =
    @"<root>
        <readerType>本科生</readerType>
        <department>皇家警察</department>
    </root>";
            XmlDocument patron_dom = new XmlDocument();
            patron_dom.LoadXml(patron_xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "海淀分馆",
    patron_dom,
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("CN-0000001-AB", isil);
            Assert.AreEqual("", alternative);
        }

        // map 长度都是 (5+)4 字符。靠前的 item 元素取胜
        // 注意 readerType: 这一部分不参与计算长度
        [TestMethod]
        public void TestMethod_patron_GetOwnerInstitution_11()
        {
            string cfg_xml =
    @"<rfid>
	    <ownerInstitution>
		    <item type='patron' map='海淀分馆/readerType:本科生呢' isil='CN-0000001-DZ' />
		    <item type='patron' map='海淀分馆/皇家警察' isil='CN-0000001-AB' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(cfg_xml);

            string patron_xml =
    @"<root>
        <readerType>本科生呢</readerType>
        <department>皇家警察</department>
    </root>";
            XmlDocument patron_dom = new XmlDocument();
            patron_dom.LoadXml(patron_xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "海淀分馆",
    patron_dom,
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("CN-0000001-DZ", isil);
            Assert.AreEqual("", alternative);
        }

        // map 长度都是 (5+)4 字符。靠前的 item 元素取胜
        // 注意 readerType: 这一部分不参与计算长度
        [TestMethod]
        public void TestMethod_patron_GetOwnerInstitution_12()
        {
            string cfg_xml =
    @"<rfid>
	    <ownerInstitution>
		    <item type='patron' map='海淀分馆/皇家警察' isil='CN-0000001-AB' />
		    <item type='patron' map='海淀分馆/readerType:本科生呢' isil='CN-0000001-DZ' />
        </ownerInstitution>
    </rfid>";
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.LoadXml(cfg_xml);

            string patron_xml =
    @"<root>
        <readerType>本科生呢</readerType>
        <department>皇家警察</department>
    </root>";
            XmlDocument patron_dom = new XmlDocument();
            patron_dom.LoadXml(patron_xml);

            bool bRet = LibraryServerUtil.GetOwnerInstitution(
    cfg_dom.DocumentElement,
    "海淀分馆",
    patron_dom,
    out string isil,
    out string alternative);
            Assert.AreEqual(true, bRet);
            Assert.AreEqual("CN-0000001-AB", isil);
            Assert.AreEqual("", alternative);
        }

    }
}
