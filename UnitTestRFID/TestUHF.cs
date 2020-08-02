using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.RFID;
using DigitalPlatform;

namespace UnitTestRFID
{

    [TestClass]
    public class TestUHF
    {
        [TestMethod]
        public void Test_encode_longNumericString()
        {
            // 以 long numberic string 方式编码一个数字字符串
            var bytes = UhfUtility.EncodeLongNumericString("12312345678");

            byte[] correct = Element.FromHexString(
@"FB 21 02 DD DF 7C 4E");

            // 
            Assert.AreEqual(0, ByteArray.Compare(correct, bytes));
        }


        [TestMethod]
        public void Test_decode_longNumericString()
        {
            byte[] source = Element.FromHexString(
@"FB 21 02 DD DF 7C 4E");
            string result = UhfUtility.DecodeLongNumericString(source, 0, out int used);

            // 
            Assert.AreEqual("12312345678", result);
            Assert.AreEqual(source.Length, used);
        }

        // 测试编码 UII
        [TestMethod]
        public void Test_encode_uii_1()
        {
            TestEncodeUII("CH-", "141c");

            TestEncodeUII("000", "c04f");

            TestEncodeUII("134", "c70b");

            TestEncodeUII("-1.", "adb5");

            TestEncodeUII("123", "c6e2");

            TestEncodeUII("456", "da1d");

            TestEncodeUII("78.", "ed4d");

            TestEncodeUII("31", "d319");
        }

        static void TestEncodeUII(string uii, string hex)
        {
            var bytes = UhfUtility.EncodeUII(uii);

            byte[] correct = Element.FromHexString(hex);

            // 
            Assert.AreEqual(0, ByteArray.Compare(correct, bytes));
        }

        // 测试编码 UII
        // ISO/TS 28560-4:2014(E) page 49
        [TestMethod]
        public void Test_encode_uii_2()
        {
            var bytes = UhfUtility.EncodeUII("CH-000134-1.12345678.31");

            byte[] correct = Element.FromHexString(
@"141c c04f c70b adb5 c6e2 da1d ed4d d319");

            // 
            Assert.AreEqual(0, ByteArray.Compare(correct, bytes));
        }

        // 测试解码 UII
        [TestMethod]
        public void Test_decode_uii_1()
        {
            TestDecodeUii("141c", "CH-");
            TestDecodeUii("c04f", "000");
            TestDecodeUii("c70b", "134");
            TestDecodeUii("adb5", "-1.");
            TestDecodeUii("c6e2", "123");
            TestDecodeUii("da1d", "456");
            TestDecodeUii("ed4d", "78.");
            TestDecodeUii("d319", "31");
        }

        static void TestDecodeUii(string hex, string text)
        {
            byte[] source = Element.FromHexString(hex);

            var result = UhfUtility.DecodeUII(source, 0, source.Length);

            // 
            Assert.AreEqual(text, result);
        }

        // 测试解码 UII
        // ISO/TS 28560-4:2014(E) page 49
        [TestMethod]
        public void Test_decode_uii_2()
        {
            byte[] source = Element.FromHexString(
@"141c c04f c70b adb5 c6e2 da1d ed4d d319");

            var result = UhfUtility.DecodeUII(source, 0, source.Length);

            // 
            Assert.AreEqual("CH-000134-1.12345678.31", result);
        }
    }
}
