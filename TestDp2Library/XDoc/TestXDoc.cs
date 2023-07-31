using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Text;

namespace TestDp2Library.XDoc
{
    [TestClass]
    public class TestXDoc
    {
        [TestMethod]
        public void Test_xdoc_01()
        {
            string xml = "<root />";
            var doc = DigitalPlatform.Xml.XDoc.Parse(xml);

            Assert.IsNotNull(doc);
            Assert.AreEqual("root", doc.Root.Name);
            Assert.AreEqual("<root />", doc.Root.OuterXml);

        }

        [TestMethod]
        public void Test_xdoc_02()
        {
            string xml = "<root><barcode>0000001</barcode></root>";
            var doc = DigitalPlatform.Xml.XDoc.Parse(xml);

            Assert.IsNotNull(doc);
            Assert.AreEqual("root", doc.Root.Name);
            Assert.AreEqual(1, doc.Root.Elements("barcode").Count);
            Assert.AreEqual("0000001", doc.Root.Elements("barcode").FirstInnerText);
        }

        [TestMethod]
        public void Test_xdoc_03()
        {
            string xml = "<root attr1='1' attr2='2'><barcode>0000001</barcode></root>";
            var doc = DigitalPlatform.Xml.XDoc.Parse(xml);

            Assert.IsNotNull(doc);
            Assert.AreEqual("root", doc.Root.Name);
            Assert.AreEqual(2, doc.Root.Attributes.Count);
            Assert.AreEqual("1", doc.Root.Attribute("attr1").Value);
            Assert.AreEqual("2", doc.Root.Attribute("attr2").Value);
            Assert.AreEqual("1", doc.Root.GetAttribute("attr1"));
            Assert.AreEqual("2", doc.Root.GetAttribute("attr2"));
        }
    }
}
