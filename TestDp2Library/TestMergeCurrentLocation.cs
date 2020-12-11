using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Xml;

namespace TestDp2Library
{
    /// <summary>
    /// 测试 LibraryApplication::MergeCurrentLocation()
    /// </summary>
    [TestClass]
    public class TestMergeCurrentLocation
    {
        // 没有包含 *
        [TestMethod]
        public void Test_MergeCurrentLocation_1()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>保存本库:0101</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>阅览室:0202</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(0, nRet);   // 没有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual("阅览室:0202", newValue);
        }

        // *:xxx
        [TestMethod]
        public void Test_MergeCurrentLocation_2()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>保存本库:0101</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>*:0202</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(1, nRet);   // 有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual("保存本库:0202", newValue);
        }

        // xxx:*
        [TestMethod]
        public void Test_MergeCurrentLocation_3()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>保存本库:0101</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>阅览室:*</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(1, nRet);   // 有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual("阅览室:0101", newValue);
        }

        // *:*
        [TestMethod]
        public void Test_MergeCurrentLocation_4()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>保存本库:0101</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>*:*</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(1, nRet);   // 有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual("保存本库:0101", newValue);
        }

        // 旧内容缺乏冒号
        // *:xxx
        [TestMethod]
        public void Test_MergeCurrentLocation_5()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>保存本库</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>*:0101</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(1, nRet);   // 有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual("保存本库:0101", newValue);
        }

        // 旧内容缺乏冒号
        // xxx:*
        [TestMethod]
        public void Test_MergeCurrentLocation_6()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>保存本库</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>阅览室:*</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(1, nRet);   // 有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual("阅览室", newValue);
        }

        // 旧内容缺乏冒号
        // *:*
        [TestMethod]
        public void Test_MergeCurrentLocation_7()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>保存本库</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>*:*</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(1, nRet);   // 有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual("保存本库", newValue);
        }

        // 旧内容只有右侧
        // *:xxx
        [TestMethod]
        public void Test_MergeCurrentLocation_8()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>:0101</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>*:0202</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(1, nRet);   // 有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual(":0202", newValue);
        }

        // 旧内容只有右侧
        // xxx:*
        [TestMethod]
        public void Test_MergeCurrentLocation_9()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>:0101</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>阅览室:*</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(1, nRet);   // 有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual("阅览室:0101", newValue);
        }

        // 旧内容只有右侧
        // *:*
        [TestMethod]
        public void Test_MergeCurrentLocation_10()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>:0101</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>*:*</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(1, nRet);   // 有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual(":0101", newValue);
        }

        // 新内容只有左侧
        [TestMethod]
        public void Test_MergeCurrentLocation_11()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>阅览室:0101</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>保存本库</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(0, nRet);   // 没有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual("保存本库", newValue);
        }

        // 新内容只有右侧
        [TestMethod]
        public void Test_MergeCurrentLocation_12()
        {
            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(@"<root>
<currentLocation>阅览室:0101</currentLocation>
</root>");

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(@"<root>
<currentLocation>:0202</currentLocation>
</root>");

            int nRet = LibraryApplication.MergeCurrentLocation(domExist,
    domNew,
    out string strError);
            Assert.AreEqual(0, nRet);   // 没有实质性修改
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");
            Assert.AreEqual(":0202", newValue);
        }

    }
}
