using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.RFID;

namespace UnitTestRFID
{
    // 测试高校联盟 UHF 标签编码解码功能
    [TestClass]
    public class TestGaoxiao
    {
        // 测试解析 Content Parameter
        [TestMethod]
        public void Test_decode_contentParameter_1()
        {
            byte[] bytes = new byte[] { 0x00, 0xa1 };
            int[] results = GaoxiaoUtility.DecodeContentParameter(bytes);

            Assert.AreEqual(3, results.Length);
            // 3 12 15
            Assert.AreEqual(3, results[0]);
            Assert.AreEqual(12, results[1]);
            Assert.AreEqual(15, results[2]);
        }

        [TestMethod]
        public void Test_encode_contentParameter_1()
        {
            int[] oid_list = new int[] { 3, 12, 15 };
            byte[] correct = new byte[] { 0x00, 0xa1 };

            byte[] results = GaoxiaoUtility.EncodeContentParameter(oid_list);

            Assert.IsTrue(results.SequenceEqual(correct));
        }
    }
}
