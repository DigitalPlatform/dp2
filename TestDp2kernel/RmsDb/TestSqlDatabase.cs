using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.rms;

namespace TestDp2kernel.RmsDb
{
    [TestClass]
    public class TestSqlDatabase
    {
        [TestMethod]
        public void Test_truncateRange_01()
        {
            var result = SqlDatabase.TruncateRange("0", 0);
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void Test_truncateRange_02()
        {
            var result = SqlDatabase.TruncateRange("0-1", 0);
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void Test_truncateRange_03()
        {
            var result = SqlDatabase.TruncateRange("0", 1);
            Assert.AreEqual("0", result);
        }

        [TestMethod]
        public void Test_truncateRange_04()
        {
            var result = SqlDatabase.TruncateRange("0-1", 1);
            Assert.AreEqual("0", result);
        }

        [TestMethod]
        public void Test_truncateRange_05()
        {
            var result = SqlDatabase.TruncateRange("1-2", 1);
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void Test_truncateRange_06()
        {
            var result = SqlDatabase.TruncateRange("1-2", 2);
            Assert.AreEqual("1", result);
        }

        [TestMethod]
        public void Test_truncateRange_07()
        {
            var result = SqlDatabase.TruncateRange("1", 2);
            Assert.AreEqual("1", result);
        }

        [TestMethod]
        public void Test_truncateRange_10()
        {
            var result = SqlDatabase.TruncateRange("0-1,8-9", 10);
            Assert.AreEqual("0-1,8-9", result);
        }

        [TestMethod]
        public void Test_truncateRange_11()
        {
            var result = SqlDatabase.TruncateRange("0-1,8-9", 9);
            Assert.AreEqual("0-1,8", result);
        }

        [TestMethod]
        public void Test_truncateRange_12()
        {
            var result = SqlDatabase.TruncateRange("0-1,8-9", 8);
            Assert.AreEqual("0-1", result);
        }

        [TestMethod]
        public void Test_reachEnd_01()
        {
            // 判断一个 range 是否触达指定的长度
            // return:
            //      0   没有触达
            //      1   末尾刚好触达
            //      2   末尾越过 length
            var ret = SqlDatabase.ReachEnd("0-1", 3);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_reachEnd_02()
        {
            // 判断一个 range 是否触达指定的长度
            // return:
            //      0   没有触达
            //      1   末尾刚好触达
            //      2   末尾越过 length
            var ret = SqlDatabase.ReachEnd("0-1", 2);
            Assert.AreEqual(1, ret);
        }

        [TestMethod]
        public void Test_reachEnd_03()
        {
            // 判断一个 range 是否触达指定的长度
            // return:
            //      0   没有触达
            //      1   末尾刚好触达
            //      2   末尾越过 length
            var ret = SqlDatabase.ReachEnd("0-2", 2);
            Assert.AreEqual(2, ret);
        }

        [TestMethod]
        public void Test_reachEnd_04()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                // 判断一个 range 是否触达指定的长度
                // return:
                //      0   没有触达
                //      1   末尾刚好触达
                //      2   末尾越过 length
                var ret = SqlDatabase.ReachEnd("", 1);
            });
        }

        [TestMethod]
        public void Test_reachEnd_05()
        {
            // 判断一个 range 是否触达指定的长度
            // return:
            //      0   没有触达
            //      1   末尾刚好触达
            //      2   末尾越过 length
            var ret = SqlDatabase.ReachEnd("1-2", 4);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_reachEnd_06()
        {
            // 判断一个 range 是否触达指定的长度
            // return:
            //      0   没有触达
            //      1   末尾刚好触达
            //      2   末尾越过 length
            var ret = SqlDatabase.ReachEnd("2-3", 4);
            Assert.AreEqual(1, ret);
        }

        [TestMethod]
        public void Test_reachEnd_07()
        {
            // 判断一个 range 是否触达指定的长度
            // return:
            //      0   没有触达
            //      1   末尾刚好触达
            //      2   末尾越过 length
            var ret = SqlDatabase.ReachEnd("3-4", 4);
            Assert.AreEqual(2, ret);
        }

        [TestMethod]
        public void Test_reachEnd_08()
        {
            // 判断一个 range 是否触达指定的长度
            // return:
            //      0   没有触达
            //      1   末尾刚好触达
            //      2   末尾越过 length
            var ret = SqlDatabase.ReachEnd("0-1,7-8", 10);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_reachEnd_09()
        {
            // 判断一个 range 是否触达指定的长度
            // return:
            //      0   没有触达
            //      1   末尾刚好触达
            //      2   末尾越过 length
            var ret = SqlDatabase.ReachEnd("0-1,8-9", 10);
            Assert.AreEqual(1, ret);
        }

        [TestMethod]
        public void Test_reachEnd_10()
        {
            // 判断一个 range 是否触达指定的长度
            // return:
            //      0   没有触达
            //      1   末尾刚好触达
            //      2   末尾越过 length
            var ret = SqlDatabase.ReachEnd("0-1,9-10", 10);
            Assert.AreEqual(2, ret);
        }

        [TestMethod]
        public void Test_reachEnd_11()
        {
            // 判断一个 range 是否触达指定的长度
            // return:
            //      0   没有触达
            //      1   末尾刚好触达
            //      2   末尾越过 length
            var ret = SqlDatabase.ReachEnd("0-1,100", 10);
            Assert.AreEqual(2, ret);
        }
    }
}
