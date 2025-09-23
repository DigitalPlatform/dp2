using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;

namespace TestDp2Library
{
    [TestClass]
    public class TestDp2libraryAccount
    {
        [TestMethod]
        public void Test_account_01()
        {
            string list = "127.0.0.1";
            string ip = "127.0.0.1";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_account_02()
        {
            string list = "127.0.0.1";
            string ip = "127.0.0.2";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_account_03()
        {
            string list = "127.0.0.1";
            string ip = "127.0.0.1(Z3950)";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_account_04()
        {
            string list = "127.0.0.1(Z3950)";
            string ip = "127.0.0.1";
            try
            {
                var ret = Account.MatchIpAddressList(list, ip);
                Assert.Fail("这里应该抛出 ArgumentException 异常");
            }
            catch (ArgumentException ex)
            {
                return;
            }
        }

        [TestMethod]
        public void Test_account_05()
        {
            string list = "127.0.0.1";
            string ip = "127.0.0.1(Z3950); localhost";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_account_06()
        {
            string list = "127.0.0.1";
            string ip = "localhost; 127.0.0.1(Z3950)";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_account_07()
        {
            string list = "127.0.0.1|127.0.0.2";
            string ip = "localhost; 127.0.0.1(Z3950)";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_account_08()
        {
            string list = "127.0.0.1|127.0.0.2";
            string ip = "127.0.0.1(Z3950)";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_account_09()
        {
            string list = "127.0.0.1|127.0.0.2";
            string ip = "127.0.0.2(Z3950)";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_account_10()
        {
            string list = "127.0.0.1|127.0.0.2";
            string ip = "127.0.0.1";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_account_11()
        {
            string list = "127.0.0.1|127.0.0.2";
            string ip = "127.0.0.2";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_account_12()
        {
            string list = "127.0.0.1|127.0.0.2";
            string ip = "localhost";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_account_13()
        {
            string list = "localhost|127.0.0.2";
            string ip = "127.0.0.1";
            var ret = Account.MatchIpAddressList(list, ip);
            Assert.AreEqual(true, ret);
        }
    }
}
