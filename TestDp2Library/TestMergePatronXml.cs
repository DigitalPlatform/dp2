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
    [TestClass]
    public class TestMergePatronXml
    {
        [TestMethod]
        public void Test_MergeTwoReaderXml_01()
        {
            string[] element_names = new string[] {
            "name"
            };
            string[] important_fields = new string[] {
            "name"
            };

            string old_xml = @"<root>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            AreEqual(domNew, domMerged);
        }

        #region 实用函数

        static string Convert(string xml)
        {
            if (xml == null)
                return "";
            return xml.Replace("\r\n", "").Replace("'","\"");
        }

        static void AreEqual(XmlDocument domNew, XmlDocument domMerged)
        {
            DomUtil.DeleteElement(domMerged.DocumentElement, "refID");
            Assert.AreEqual(domNew.OuterXml, domMerged.OuterXml);
        }

        static string GetOuterXml(XmlDocument domMerged)
        {
            DomUtil.DeleteElement(domMerged.DocumentElement, "refID");
            return domMerged.OuterXml;
        }

        #endregion

        // 无 important，成功
        [TestMethod]
        public void Test_MergeTwoReaderXml_02()
        {
            string[] element_names = new string[] {
            "name"
            };
            string[] important_fields = null;

            string old_xml = @"<root>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
<namePinyin>pinyin</namePinyin>
</root>";

            string result_xml = @"<root>
<name>新姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            Assert.AreEqual( Convert(result_xml), GetOuterXml(domMerged));
        }


        // 有 important，失败
        [TestMethod]
        public void Test_MergeTwoReaderXml_03()
        {
            string[] element_names = new string[] {
            "name"
            };
            string[] important_fields = new string[] {
            "namePinyin"
            };

            string old_xml = @"<root>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
<namePinyin>pinyin</namePinyin>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(-1, ret);
            Assert.AreEqual(true, strError.StartsWith("下列"));
            // AreEqual(domNew, domMerged);
        }

        //

        // 无 important，成功
        [TestMethod]
        public void Test_MergeTwoReaderXml_04()
        {
            string[] element_names = new string[] {
            "name"
            };
            string[] important_fields = null;

            string old_xml = @"<root>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";


            string result_xml = @"<root>
<name>新姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,  // 注意检查被阻止修改的元素名
    out string strError);
            Assert.AreEqual(0, ret);
            Assert.AreEqual(Convert(result_xml), GetOuterXml(domMerged));
        }


        // 有 important，失败
        [TestMethod]
        public void Test_MergeTwoReaderXml_05()
        {
            string[] element_names = new string[] {
            "name"
            };
            string[] important_fields = new string[] {
            "http://dp2003.com/dprms:file"
            };

            string old_xml = @"<root>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(-1, ret);
            Assert.AreEqual(true, strError.StartsWith("下列"));
            // AreEqual(domNew, domMerged);
        }


        // dprms:file 元素修改成功
        [TestMethod]
        public void Test_MergeTwoReaderXml_06()
        {
            string[] element_names = new string[] {
            "name",
            "http://dp2003.com/dprms:file"
            };
            string[] important_fields = new string[] {
            "http://dp2003.com/dprms:file"
            };

            string old_xml = @"<root>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            AreEqual(domNew, domMerged);
        }

        // dprms:file 元素没有变化
        // 因为没有变化，所以即便 dprms:file 被列入了重要元素，也没有发生报错
        [TestMethod]
        public void Test_MergeTwoReaderXml_07()
        {
            string[] element_names = new string[] {
            "name",
            // "http://dp2003.com/dprms:file"
            };
            string[] important_fields = new string[] {
            "http://dp2003.com/dprms:file"
            };

            string old_xml = @"<root>
<name>姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            AreEqual(domNew, domMerged);
        }

        // dprms:file 元素请求希望发生了变化，但权限不允许变化
        // 因 dprms:file 被列入了重要元素，所以会报错
        [TestMethod]
        public void Test_MergeTwoReaderXml_08()
        {
            string[] element_names = new string[] {
            "name",
            };
            string[] important_fields = new string[] {
            "http://dp2003.com/dprms:file"
            };

            string old_xml = @"<root>
<name>姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
<dprms:file id='1' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(-1, ret);
            Assert.AreEqual(true, strError.StartsWith("下列"));
        }

        // 删除一个元素
        [TestMethod]
        public void Test_MergeTwoReaderXml_09()
        {
            string[] element_names = new string[] {
            "name",
            "department",
            };
            string[] important_fields = new string[] {
            "name"
            };

            string old_xml = @"<root>
<name>姓名</name>
<department>单位</department>
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            AreEqual(domNew, domMerged);
        }

        // 删除一个元素
        // 因为 department 元素不在许可修改的范围，所以删除没有兑现。但没有报错
        [TestMethod]
        public void Test_MergeTwoReaderXml_10()
        {
            string[] element_names = new string[] {
            "name",
            // "department",
            };
            string[] important_fields = new string[] {
            "name"
            };

            string old_xml = @"<root>
<name>姓名</name>
<department>单位</department>
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
</root>";

            string result_xml = @"<root>
<name>新姓名</name>
<department>单位</department>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            Assert.AreEqual(Convert(result_xml), GetOuterXml(domMerged));
        }

        // 删除一个元素
        // 因为 department 元素不在许可修改的范围，所以删除没有兑现。
        // 因为 department 元素属于重要元素，所以最后报错了
        [TestMethod]
        public void Test_MergeTwoReaderXml_11()
        {
            string[] element_names = new string[] {
            "name",
            };
            string[] important_fields = new string[] {
            "name",
            "department",
            };

            string old_xml = @"<root>
<name>姓名</name>
<department>单位</department>
</root>";
            string new_xml = @"<root>
<name>新姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    element_names,
    "change",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(-1, ret);
            Assert.AreEqual(true, strError.StartsWith("下列"));

        }


        // changestate 重要元素期待修改，而拒绝修改，报错
        [TestMethod]
        public void Test_MergeTwoReaderXml_changeState_01()
        {
            string[] important_fields = new string[] {
            "name"
            };

            string old_xml = @"<root>
<state>状态1</state>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<state>状态2</state>
<name>新姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    null,
    "changestate",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(-1, ret);
            Assert.AreEqual(true, strError.StartsWith("下列"));
        }

        // changestate 重要元素期待修改，然而重要元素新旧之间没有变化，成功
        [TestMethod]
        public void Test_MergeTwoReaderXml_changeState_02()
        {
            string[] important_fields = new string[] {
            "name"
            };

            string old_xml = @"<root>
<state>状态1</state>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<state>状态2</state>
<name>姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    null,
    "changestate",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            AreEqual(domNew, domMerged);
        }

        // changestate 不重要的元素没有被兑现修改
        [TestMethod]
        public void Test_MergeTwoReaderXml_changeState_03()
        {
            string[] important_fields = new string[] {
            "state"
            };

            string old_xml = @"<root>
<state>状态1</state>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<state>状态2</state>
<name>新姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";

            string result_xml = @"<root>
<state>状态2</state>
<name>姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    null,
    "changestate",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            Assert.AreEqual(Convert(result_xml), GetOuterXml(domMerged));
        }

        // changestate 不重要的元素没有被兑现修改
        [TestMethod]
        public void Test_MergeTwoReaderXml_changeState_04()
        {
            string[] important_fields = new string[] {
            "state"
            };

            string old_xml = @"<root>
<state>状态1</state>
<name>姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";
            string new_xml = @"<root>
<state>状态2</state>
<name>新姓名</name>
</root>";

            string result_xml = @"<root>
<state>状态2</state>
<name>姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    null,
    "changestate",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            Assert.AreEqual(Convert(result_xml), GetOuterXml(domMerged));
        }

        //

        // changeforegift 重要元素期待修改，而拒绝修改，报错
        [TestMethod]
        public void Test_MergeTwoReaderXml_changeForegift_01()
        {
            string[] important_fields = new string[] {
            "name"
            };

            string old_xml = @"<root>
<foregift attr='value1'>string1</foregift>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<foregift attr='value2'>string2</foregift>
<name>新姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    null,
    "changeforegift",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(-1, ret);
            Assert.AreEqual(true, strError.StartsWith("下列"));
        }

        // changeforegift 重要元素期待修改，然而重要元素新旧之间没有变化，成功
        [TestMethod]
        public void Test_MergeTwoReaderXml_changeForegift_02()
        {
            string[] important_fields = new string[] {
            "name"
            };

            string old_xml = @"<root>
<foregift attr='value1'>string1</foregift>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<foregift attr='value2'>string2</foregift>
<name>姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    null,
    "changeforegift",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            AreEqual(domNew, domMerged);
        }

        // changeforegift 不重要的元素没有被兑现修改
        [TestMethod]
        public void Test_MergeTwoReaderXml_changeForegift_03()
        {
            string[] important_fields = new string[] {
            "foregift"
            };

            string old_xml = @"<root>
<foregift attr='value1'>string1</foregift>
<name>姓名</name>
</root>";
            string new_xml = @"<root>
<foregift attr='value2'>string2</foregift>
<name>新姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";

            string result_xml = @"<root>
<foregift attr='value2'>string2</foregift>
<name>姓名</name>
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    null,
    "changeforegift",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            Assert.AreEqual(Convert(result_xml), GetOuterXml(domMerged));
        }

        // changeforegift 不重要的元素没有被兑现修改
        [TestMethod]
        public void Test_MergeTwoReaderXml_changeForegift_04()
        {
            string[] important_fields = new string[] {
            "foregift"
            };

            string old_xml = @"<root>
<foregift attr='value1'>string1</foregift>
<name>姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";
            string new_xml = @"<root>
<foregift attr='value2'>string2</foregift>
<name>新姓名</name>
</root>";

            string result_xml = @"<root>
<foregift attr='value2'>string2</foregift>
<name>姓名</name>
<dprms:file id='0' usage='face' xmlns:dprms='http://dp2003.com/dprms' />
</root>";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(old_xml);
            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(new_xml);

            var ret = LibraryApplication.MergeTwoReaderXml(
    null,
    "changeforegift",
    domExist,
    domNew,
    important_fields,
    null,
    out XmlDocument domMerged,
    out _,
    out string strError);
            Assert.AreEqual(0, ret);
            Assert.AreEqual(Convert(result_xml), GetOuterXml(domMerged));
        }

    }
}
