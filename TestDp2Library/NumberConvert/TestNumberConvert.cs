using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform;

namespace TestDp2Library
{
    [TestClass]
    public class TestNumberConvert
    {
        [TestMethod]
        public void Test_numberConvert_01()
        {
            string chinese = "一万零一";
            long result = 10001;
            var ret = NumberConvert.ParseCnToInt(chinese);
            Assert.AreEqual(result, ret);
        }

        [TestMethod]
        public void Test_numberConvert_02()
        {
            string chinese = "一万零一百";
            long result = 10100;
            var ret = NumberConvert.ParseCnToInt(chinese);
            Assert.AreEqual(result, ret);
        }

        [TestMethod]
        public void Test_numberConvert_03()
        {
            string chinese = "一万零一百元";
            long result = 10100;
            var ret = NumberConvert.ParseCnToInt(chinese);
            Assert.AreEqual(result, ret);
        }

        [TestMethod]
        public void Test_numberConvert_04()
        {
            string chinese = "共一万零一百";
            long result = 10100;
            var ret = NumberConvert.ParseCnToInt(chinese);
            Assert.AreEqual(result, ret);
        }
    }
}
