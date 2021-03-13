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
        public void TestMethod_GetOwnerInstitution_1()
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
    out string isil,
    out string alternative);
            Assert.AreEqual("test", isil);
            Assert.AreEqual("", alternative);
        }

        // 分馆名字部分匹配上了，但是房间名字部分没有匹配上
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_2()
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
    out string isil,
    out string alternative);
            Assert.AreEqual("test", isil);
            Assert.AreEqual("", alternative);
        }

        [TestMethod]
        public void TestMethod_GetOwnerInstitution_3()
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
    out string isil,
    out string alternative);
            Assert.AreEqual("", isil);
            Assert.AreEqual("xc", alternative);
        }

        // 分馆名字部分匹配上了
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_4()
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
    out string isil,
    out string alternative);
            Assert.AreEqual("", isil);
            Assert.AreEqual("xc", alternative);
        }

        // '/阅览室' 匹配上了
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_5()
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
    out string isil,
    out string alternative);
            Assert.AreEqual("", isil);
            Assert.AreEqual("xc", alternative);
        }

        // map '/' 匹配上了；map '/阅览室' 没有匹配上
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_6()
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
    out string isil,
    out string alternative);
            Assert.AreEqual("test", isil);
            Assert.AreEqual("", alternative);
        }

        // 用总馆形态去匹配分馆形态的 location (海淀分馆/)，应不匹配
        [TestMethod]
        public void TestMethod_GetOwnerInstitution_7()
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
    out string isil,
    out string alternative);
            Assert.AreEqual(bRet, false);
            Assert.AreEqual("", isil);
            Assert.AreEqual("", alternative);
        }

        [TestMethod]
        public void TestMethod_GetOwnerInstitution_8()
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
    out string isil,
    out string alternative);
            Assert.AreEqual("CN-0000001-XZ", isil);
            Assert.AreEqual("", alternative);
        }

        [TestMethod]
        public void TestMethod_wilcard_GetOwnerInstitution_1()
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
    out string isil,
    out string alternative);
            Assert.AreEqual("CN-0000001-XZ1", isil);
            Assert.AreEqual("", alternative);
        }

        [TestMethod]
        public void TestMethod_wilcard_GetOwnerInstitution_2()
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
    out string isil,
    out string alternative);
            Assert.AreEqual("CN-0000001-XZ2", isil);
            Assert.AreEqual("", alternative);
        }

    }
}
