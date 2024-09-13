using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalPlatform.Marc;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DigitalPlatform.MarcEditor
{
    [TestClass]
    public class UnitTestMarcEditor
    {
        [TestMethod]
        public void Test_GetCurrent_01()
        {
            // return:
            //      0   不在子字段上
            //      1   在子字段上
            var ret = MyEdit.GetCurrentSubfieldCaretInfo(
    "200  $aAAA$bBBB".Replace('$', Record.SUBFLD),
    0,
    out string strSubfieldName,
    out string strSufieldContent,
    out int nStart,
    out int nContentStart,
    out int nContentLength,
    out bool forbidden);
            Assert.AreEqual(0, ret);
            Assert.AreEqual("", strSubfieldName);
            Assert.AreEqual("", strSufieldContent);
            Assert.AreEqual(0, nStart);
            Assert.AreEqual(0, nContentStart);
            Assert.AreEqual(0, nContentLength);
        }

        [TestMethod]
        public void Test_GetCurrent_02()
        {
            // return:
            //      0   不在子字段上
            //      1   在子字段上
            var ret = MyEdit.GetCurrentSubfieldCaretInfo(
    "200  $aAAA$bBBB".Replace('$', Record.SUBFLD),
    1,
    out string strSubfieldName,
    out string strSufieldContent,
    out int nStart,
    out int nContentStart,
    out int nContentLength,
    out bool forbidden);
            Assert.AreEqual(0, ret);
            Assert.AreEqual("", strSubfieldName);
            Assert.AreEqual("", strSufieldContent);
            Assert.AreEqual(0, nStart);
            Assert.AreEqual(0, nContentStart);
            Assert.AreEqual(0, nContentLength);
        }

        [TestMethod]
        public void Test_GetCurrent_12()
        {
            // return:
            //      0   不在子字段上
            //      1   在子字段上
            var ret = MyEdit.GetCurrentSubfieldCaretInfo(
    "200  $aAAA$bBBB".Replace('$', Record.SUBFLD),
    5,
    out string strSubfieldName,
    out string strSufieldContent,
    out int nStart,
    out int nContentStart,
    out int nContentLength,
    out bool forbidden);
            Assert.AreEqual(1, ret);
            Assert.AreEqual("a", strSubfieldName);
            Assert.AreEqual("AAA", strSufieldContent);
            Assert.AreEqual(5, nStart);
            Assert.AreEqual(7, nContentStart);
            Assert.AreEqual(3, nContentLength);
        }

        [TestMethod]
        public void Test_GetCurrent_13()
        {
            // return:
            //      0   不在子字段上
            //      1   在子字段上
            var ret = MyEdit.GetCurrentSubfieldCaretInfo(
    "200  $aAAA$bBBB".Replace('$', Record.SUBFLD),
    5+5,
    out string strSubfieldName,
    out string strSufieldContent,
    out int nStart,
    out int nContentStart,
    out int nContentLength,
    out bool forbidden);
            Assert.AreEqual(1, ret);
            Assert.AreEqual("b", strSubfieldName);
            Assert.AreEqual("BBB", strSufieldContent);
            Assert.AreEqual(10, nStart);
            Assert.AreEqual(12, nContentStart);
            Assert.AreEqual(3, nContentLength);
        }
    }
}
