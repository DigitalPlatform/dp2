using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;
using System.Diagnostics;

namespace TestDp2Library
{
    [TestClass]
    public class TestPatronBinding
    {
        [TestMethod]
        public void Test_AddBindingString_01()
        {
            string strEmail = "";
            string strNewEmail = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
    false);
            Assert.AreEqual("weixinid:5678", strNewEmail);
        }

        [TestMethod]
        public void Test_AddBindingString_02()
        {
            string strEmail = "";
            string strNewEmail = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
    true);
            Assert.AreEqual("weixinid:5678", strNewEmail);
        }

        [TestMethod]
        public void Test_AddBindingString_03()
        {
            string strEmail = "email:xietao@dp2003.com";
            string strNewEmail = LibraryApplication.AddBindingString(strEmail,
    "weixinid:1234",
    true);
            Assert.AreEqual("email:xietao@dp2003.com,weixinid:1234", strNewEmail);
            // Debug.WriteLine(strNewEmail);
        }

        [TestMethod]
        public void Test_AddBindingString_04()
        {
            string strEmail = "email:xietao@dp2003.com,weixinid:1234";
            string strNewEmail = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
    true);
            Assert.AreEqual("email:xietao@dp2003.com,weixinid:1234,weixinid:5678", strNewEmail);
        }

        [TestMethod]
        public void Test_AddBindingString_05()
        {
            string strEmail = "email:xietao@dp2003.com,weixinid:1234";
            string strNewEmail = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
    false);
            Assert.AreEqual("email:xietao@dp2003.com,weixinid:5678", strNewEmail);
        }

        [TestMethod]
        public void Test_AddBindingString_06()
        {
            string strEmail = "weixinid:1234,email:xietao@dp2003.com";
            string strNewEmail = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
    true);
            Assert.AreEqual("weixinid:1234,email:xietao@dp2003.com,weixinid:5678", strNewEmail);
        }

        [TestMethod]
        public void Test_AddBindingString_07()
        {
            string strEmail = "weixinid:1234,email:xietao@dp2003.com";
            string strNewEmail = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
    false);
            Assert.AreEqual("email:xietao@dp2003.com,weixinid:5678", strNewEmail);
        }

        // --------------------------

        [TestMethod]
        public void Test_AddBindingString_11()
        {
            string strEmail = "";
            int nRet = LibraryApplication.AddBindingString(strEmail,
"weixinid:5678",
"single",
out string strResult,
out string strError);
            Assert.AreEqual(1, nRet);
            Assert.AreEqual("weixinid:5678", strResult);
        }

        [TestMethod]
        public void Test_AddBindingString_12()
        {
            string strEmail = "";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
"multiple",
out string strResult,
out string strError);
            Assert.AreEqual(1, nRet);
            Assert.AreEqual("weixinid:5678", strResult);
        }

        [TestMethod]
        public void Test_AddBindingString_13()
        {
            string strEmail = "email:xietao@dp2003.com";
            int nRet = LibraryApplication.AddBindingString(strEmail,
                "weixinid:1234",
                "multiple",
                out string strResult,
                out string strError);
            Assert.AreEqual(1, nRet);
            Assert.AreEqual("email:xietao@dp2003.com,weixinid:1234", strResult);
        }

        [TestMethod]
        public void Test_AddBindingString_14()
        {
            string strEmail = "email:xietao@dp2003.com,weixinid:1234";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
                "multiple",
                out string strResult,
                out string strError);
            Assert.AreEqual(1, nRet);
            Assert.AreEqual("email:xietao@dp2003.com,weixinid:1234,weixinid:5678", strResult);
        }

        [TestMethod]
        public void Test_AddBindingString_15()
        {
            string strEmail = "email:xietao@dp2003.com,weixinid:1234";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
                "single",
                out string strResult,
                out string strError);
            Assert.AreEqual(1, nRet);
            Assert.AreEqual("email:xietao@dp2003.com,weixinid:5678", strResult);
        }

        [TestMethod]
        public void Test_AddBindingString_16()
        {
            string strEmail = "weixinid:1234,email:xietao@dp2003.com";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
                "multiple",
                out string strResult,
                out string strError);
            Assert.AreEqual(1, nRet);
            Assert.AreEqual("weixinid:1234,email:xietao@dp2003.com,weixinid:5678", strResult);
        }

        [TestMethod]
        public void Test_AddBindingString_17()
        {
            string strEmail = "weixinid:1234,email:xietao@dp2003.com";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
                "single",
                out string strResult,
                out string strError);
            Assert.AreEqual(1, nRet);
            Assert.AreEqual("email:xietao@dp2003.com,weixinid:5678", strResult);
        }

        // ------

        [TestMethod]
        public void Test_AddBindingString_18()
        {
            string strEmail = "email:xietao@dp2003.com,weixinid:1234";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
                "singlestrict",
                out string strResult,
                out string strError);
            Assert.AreEqual(-1, nRet);
        }

        [TestMethod]
        public void Test_AddBindingString_19()
        {
            string strEmail = "email:xietao@dp2003.com,weixinid:1234";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
                "singlestrict",
                out string strResult,
                out string strError);
            Assert.AreEqual(-1, nRet);
        }

        [TestMethod]
        public void Test_AddBindingString_20()
        {
            string strEmail = "weixinid:1234,email:xietao@dp2003.com";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
                "singlestrict",
                out string strResult,
                out string strError);
            Assert.AreEqual(-1, nRet);
        }

        [TestMethod]
        public void Test_AddBindingString_21()
        {
            string strEmail = "weixinid:1234,email:xietao@dp2003.com";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
                "singlestrict",
                out string strResult,
                out string strError);
            Assert.AreEqual(-1, nRet);
        }

        [TestMethod]
        public void Test_AddBindingString_22()
        {
            string strEmail = "email:xietao@dp2003.com";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
                "singlestrict",
                out string strResult,
                out string strError);
            Assert.AreEqual(1, nRet);
            Assert.AreEqual("email:xietao@dp2003.com,weixinid:5678", strResult);
        }

        [TestMethod]
        public void Test_AddBindingString_23()
        {
            string strEmail = "";
            int nRet = LibraryApplication.AddBindingString(strEmail,
    "weixinid:5678",
                "singlestrict",
                out string strResult,
                out string strError);
            Assert.AreEqual(1, nRet);
            Assert.AreEqual("weixinid:5678", strResult);
        }

    }
}
