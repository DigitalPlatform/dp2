using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.RFID;
using DigitalPlatform;
using System.Diagnostics;

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

        // 测试 96-bit IPC 编码
        [TestMethod]
        public void Test_encode_96bit_1()
        {
            string source = "ABC01";
            string correct = "054142433031";

            test_encode_96bit(source, correct);
        }

        static void test_encode_96bit(string source, string correct)
        {
            var result = GaoxiaoUtility.EncodeIpc96bit(source);
            var target = ByteArray.GetTimeStampByteArray(correct);
            Assert.IsTrue(result.SequenceEqual(target));
        }

        [TestMethod]
        public void Test_encode_96bit_2()
        {
            string source = "ABC012";
            string correct = "06414243303132";

            test_encode_96bit(source, correct);
        }

        // 大于等于 8 字符，算法比较复杂
        [TestMethod]
        public void Test_encode_96bit_3()
        {
            string source = "ABC0123*";
            string correct = "A8324911EC010000";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_4()
        {
            string source = "ABC01234=";
            string correct = "D933491148130000";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_5()
        {
            string source = "ABC012345~";
            string correct = "EA374911E4C00000";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_6()
        {
            string source = "ABC0123456a";
            string correct = "1B36491100890700";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_7()
        {
            string source = "ABC01234567z";
            string correct = "AC3749111C5A4B00";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_8()
        {
            string source = "ABC012345678*";
            string correct = "AD3249113885F102";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_9()
        {
            string source = "ABC0123456789?";
            string correct = "FE33491154346F1D";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_10()
        {
            string source = "ABC0123456789_";
            string correct = "FE35491154346F1D";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_11()
        {
            string source = "A00000@";
            string correct = "0741303030303040";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_12()
        {
            string source = "AA0000@";
            string correct = "0741413030303040";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_13()
        {
            string source = "AAA9000000000)";
            string correct = "9E12455102000000";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_14()
        {
            string source = "ZZZ80007000000";
            string correct = "0EA3AA2A82B92A00";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_15()
        {
            string source = "99999999999999";
            string correct = "9E932449FE276BEE";

            test_encode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_96bit_16()
        {
            string source = "29899999196916";
            string correct = "6E8324422E4166EE";

            test_encode_96bit(source, correct);
        }

        // 测试解码 96 bit
        [TestMethod]
        public void Test_decode_96bit_1()
        {
            string source = "054142433031";
            string correct = "ABC01";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_2()
        {
            string source = "06414243303132";
            string correct = "ABC012";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_3()
        {
            string source = "A8324911EC010000";
            string correct = "ABC0123*";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_4()
        {
            string source = "D933491148130000";
            string correct = "ABC01234=";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_5()
        {
            string source = "EA374911E4C00000";
            string correct = "ABC012345~";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_6()
        {
            string source = "1B36491100890700";
            string correct = "ABC0123456a";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_7()
        {
            string source = "AC3749111C5A4B00";
            string correct = "ABC01234567z";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_8()
        {
            string source = "AD3249113885F102";
            string correct = "ABC012345678*";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_9()
        {
            string source = "FE33491154346F1D";
            string correct = "ABC0123456789?";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_10()
        {
            string source = "FE35491154346F1D";
            string correct = "ABC0123456789_";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_11()
        {
            string source = "0741303030303040";
            string correct = "A00000@";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_12()
        {
            string source = "0741413030303040";
            string correct = "AA0000@";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_13()
        {
            string source = "9E12455102000000";
            string correct = "AAA9000000000)";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_14()
        {
            string source = "0EA3AA2A82B92A00";
            string correct = "ZZZ80007000000";

            test_decode_96bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_96bit_15()
        {
            string source = "9E932449FE276BEE";
            string correct = "99999999999999";

            test_decode_96bit(source, correct);
        }


        [TestMethod]
        public void Test_decode_96bit_16()
        {
            string source = "6E8324422E4166EE";
            string correct = "29899999196916";

            test_decode_96bit(source, correct);
        }

        static void test_decode_96bit(string source_hex, string correct)
        {
            var source = ByteArray.GetTimeStampByteArray(source_hex);
            var result = GaoxiaoUtility.DecodeIpc96bit(source);
            Assert.AreEqual(correct, result);
        }

        // 测试 128-bit IPC 编码
        [TestMethod]
        public void Test_encode_128bit_1()
        {
            string source = "A";
            string correct = "0141";

            test_encode_128bit(source, correct);
        }

        static void test_encode_128bit(string source, string correct)
        {
            var result = GaoxiaoUtility.EncodeIpc128bit(source);
            var target = ByteArray.GetTimeStampByteArray(correct);
            Assert.IsTrue(result.SequenceEqual(target));
        }

        [TestMethod]
        public void Test_encode_128bit_2()
        {
            string source = "ABC012";
            string correct = "06414243303132";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_3()
        {
            string source = "ABC0123*";
            string correct = "08414243303132332A";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_4()
        {
            string source = "ABC01234=";
            string correct = "0941424330313233343D";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_5()
        {
            string source = "ABC012345~";
            string correct = "0A4142433031323334357E";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_6()
        {
            string source = "ABC0123456a";
            string correct = "0B4142433031323334353661";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_7()
        {
            string source = "ABC01234567z";
            string correct = "AC77000046410C0201304911";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_8()
        {
            string source = "ABC012345678*";
            string correct = "AD821C0046410C0201304911";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_9()
        {
            string source = "ABC0123456789?";
            string correct = "FE93200746410C0201304911";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_10()
        {
            string source = "ABC0123456789_";
            string correct = "FE95200746410C0201304911";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_11()
        {
            string source = "A00000@";
            string correct = "0741303030303040";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_12()
        {
            string source = "AA0000@";
            string correct = "0741413030303040";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_13()
        {
            string source = "AAA9000000000)";
            string correct = "9E0200000000000040124511";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_14()
        {
            string source = "ZZZ80007000000";
            string correct = "0E0300000070000000A2AA2A";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_15()
        {
            string source = "99999999999999";
            string correct = "9E9324094992240949922409";

            test_encode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_encode_128bit_16()
        {
            string source = "ZZZZZZZZZZZZZZ";
            string correct = "AEA5AA2AAAAAAA2AAAAAAA2A";

            test_encode_128bit(source, correct);
        }


        // 测试 128-bit IPC 解码
        [TestMethod]
        public void Test_decode_128bit_1()
        {
            string source = "0141";
            string correct = "A";

            test_decode_128bit(source, correct);
        }

        static void test_decode_128bit(string source_hex, string correct)
        {
            var source = ByteArray.GetTimeStampByteArray(source_hex);
            var result = GaoxiaoUtility.DecodeIpc128bit(source);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void Test_decode_128bit_2()
        {
            string source = "06414243303132";
            string correct = "ABC012";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_3()
        {
            string source = "08414243303132332A";
            string correct = "ABC0123*";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_4()
        {
            string source = "0941424330313233343D";
            string correct = "ABC01234=";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_5()
        {
            string source = "0A4142433031323334357E";
            string correct = "ABC012345~";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_6()
        {
            string source = "0B4142433031323334353661";
            string correct = "ABC0123456a";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_7()
        {
            string source = "AC77000046410C0201304911";
            string correct = "ABC01234567z";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_8()
        {
            string source = "AD821C0046410C0201304911";
            string correct = "ABC012345678*";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_9()
        {
            string source = "FE93200746410C0201304911";
            string correct = "ABC0123456789?";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_10()
        {
            string source = "FE95200746410C0201304911";
            string correct = "ABC0123456789_";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_11()
        {
            string source = "0741303030303040";
            string correct = "A00000@";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_12()
        {
            string source = "0741413030303040";
            string correct = "AA0000@";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_13()
        {
            string source = "9E0200000000000040124511";
            string correct = "AAA9000000000)";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_14()
        {
            string source = "0E0300000070000000A2AA2A";
            string correct = "ZZZ80007000000";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_15()
        {
            string source = "9E9324094992240949922409";
            string correct = "99999999999999";

            test_decode_128bit(source, correct);
        }

        [TestMethod]
        public void Test_decode_128bit_16()
        {
            string source = "AEA5AA2AAAAAAA2AAAAAAA2A";
            string correct = "ZZZZZZZZZZZZZZ";

            test_decode_128bit(source, correct);
        }

        // 某小学 UHF 标签
        [TestMethod]
        public void Test_decode_epc_binary_1()
        {
            // string source_hex = "E200 0017 2217 0133 1260 9896";
            string source_hex = "0104 5300 1853 0440 0D0B 0000";

            var source = Element.FromHexString(source_hex);
            var result = GaoxiaoUtility.DecodeGaoxiaoEpc(source);
            Debug.WriteLine(result);


            string user_hex = "0C02D9941004000100012C00380000000000000000000000000000000000";

            var elements = GaoxiaoUtility.DecodeUserBank(Element.FromHexString(user_hex));
            Debug.WriteLine(elements);
        }
    }
}
