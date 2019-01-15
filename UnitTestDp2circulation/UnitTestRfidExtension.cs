using System;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using dp2Circulation;

namespace UnitTestDp2circulation
{
    [TestClass]
    public class UnitTestRfidExtension
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

            bool bRet = MainForm.GetOwnerInstitution(
    cfg_dom,
    "海淀分馆/",
    out string isil,
    out string alternative);
            Assert.AreEqual(isil, "test");
            Assert.AreEqual(alternative, "");
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

            bool bRet = MainForm.GetOwnerInstitution(
    cfg_dom,
    "海淀分馆/阅览室",
    out string isil,
    out string alternative);
            Assert.AreEqual(isil, "test");
            Assert.AreEqual(alternative, "");
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

            bool bRet = MainForm.GetOwnerInstitution(
    cfg_dom,
    "西城/",
    out string isil,
    out string alternative);
            Assert.AreEqual(isil, "");
            Assert.AreEqual(alternative, "xc");
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

            bool bRet = MainForm.GetOwnerInstitution(
    cfg_dom,
    "西城/阅览室",
    out string isil,
    out string alternative);
            Assert.AreEqual(isil, "");
            Assert.AreEqual(alternative, "xc");
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

            bool bRet = MainForm.GetOwnerInstitution(
    cfg_dom,
    "阅览室",
    out string isil,
    out string alternative);
            Assert.AreEqual(isil, "");
            Assert.AreEqual(alternative, "xc");
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

            bool bRet = MainForm.GetOwnerInstitution(
    cfg_dom,
    "流通书库",
    out string isil,
    out string alternative);
            Assert.AreEqual(isil, "test");
            Assert.AreEqual(alternative, "");
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

            bool bRet = MainForm.GetOwnerInstitution(
    cfg_dom,
    "海淀分馆/",
    out string isil,
    out string alternative);
            Assert.AreEqual(bRet, false);
            Assert.AreEqual(isil, "");
            Assert.AreEqual(alternative, "");
        }

        [TestMethod]
        public void Test_GetSetInformation_1()
        {
            {
                string result = MainForm.GetSetInformation("第一册");
                Assert.AreEqual(result, null);
            }

            {
                string result = MainForm.GetSetInformation("(第一册)");
                Assert.AreEqual(result, null);
            }

            {
                string result = MainForm.GetSetInformation(" 1(第一册)2 ");
                Assert.AreEqual(result, null);
            }

            {
                string result = MainForm.GetSetInformation("(1,2)");
                Assert.AreEqual(result, "12");
            }

            {
                string result = MainForm.GetSetInformation(" (1,2)");
                Assert.AreEqual(result, "12");
            }

            {
                string result = MainForm.GetSetInformation("(1,2) ");
                Assert.AreEqual(result, "12");
            }

            {
                string result = MainForm.GetSetInformation("(11,2)");
                Assert.AreEqual(result, "1102");
            }

            {
                string result = MainForm.GetSetInformation("(255,2)");
                Assert.AreEqual(result, "255002");
            }

            {
                string result = MainForm.GetSetInformation("(1,99)");
                Assert.AreEqual(result, "0199");
            }

            {
                string result = MainForm.GetSetInformation("(1,255)");
                Assert.AreEqual(result, "001255");
            }

            {
                string result = MainForm.GetSetInformation("(1, 255)");
                Assert.AreEqual(result, "001255");
            }

            {
                string result = MainForm.GetSetInformation("(1, 255 )");
                Assert.AreEqual(result, "001255");
            }
        }
    }
}
