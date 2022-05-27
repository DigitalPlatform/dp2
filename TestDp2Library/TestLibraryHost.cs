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
    public class TestLibraryHost
    {
        [TestMethod]
        public void Test_getChar_01()
        {
            var c = LibraryHost.GetChar("", 0);
            Assert.AreEqual('n', c);
        }

        [TestMethod]
        public void Test_getChar_02()
        {
            var c = LibraryHost.GetChar("", 1);
            Assert.AreEqual('n', c);
        }

        [TestMethod]
        public void Test_getChar_03()
        {
            var c = LibraryHost.GetChar(null, 0);
            Assert.AreEqual('n', c);
        }

        [TestMethod]
        public void Test_getChar_04()
        {
            var c = LibraryHost.GetChar(null, 1);
            Assert.AreEqual('n', c);
        }

        [TestMethod]
        public void Test_getChar_11()
        {
            var c = LibraryHost.GetChar("y", 0);
            Assert.AreEqual('y', c);
        }

        [TestMethod]
        public void Test_getChar_12()
        {
            var c = LibraryHost.GetChar("n", 0);
            Assert.AreEqual('n', c);
        }

        [TestMethod]
        public void Test_getChar_13()
        {
            var c = LibraryHost.GetChar("ny", 1);
            Assert.AreEqual('y', c);
        }

        [TestMethod]
        public void Test_getChar_14()
        {
            var c = LibraryHost.GetChar("ny", 0);
            Assert.AreEqual('n', c);
        }
    }
}
