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
    public class TestCrc16
    {

        // https://www.gs1.org/sites/default/files/docs/epc/Gen2_Protocol_Standard.pdf
        // F.3 Example CRC-16 calculations
        // Table F.2: EPC memory contents for an example Tag
        [TestMethod]
        public void Test_crc16_01()
        {
            string content = "0000";
            string crc = "E2F0";

            var result = BuildCrc16(content);
            Assert.AreEqual(crc, result);
        }

        [TestMethod]
        public void Test_crc16_02()
        {
            string content = "08001111";
            string crc = "CCAE";

            var result = BuildCrc16(content);
            Assert.AreEqual(crc, result);
        }

        [TestMethod]
        public void Test_crc16_03()
        {
            string content = "100011112222";
            string crc = "968F";

            var result = BuildCrc16(content);
            Assert.AreEqual(crc, result);
        }

        [TestMethod]
        public void Test_crc16_04()
        {
            string content = "1800111122223333";
            string crc = "78F6";

            var result = BuildCrc16(content);
            Assert.AreEqual(crc, result);
        }

        [TestMethod]
        public void Test_crc16_05()
        {
            string content = "20001111222233334444";
            string crc = "C241";

            var result = BuildCrc16(content);
            Assert.AreEqual(crc, result);
        }

        [TestMethod]
        public void Test_crc16_06()
        {
            string content = "280011112222333344445555";
            string crc = "2A91";

            var result = BuildCrc16(content);
            Assert.AreEqual(crc, result);
        }

        [TestMethod]
        public void Test_crc16_07()
        {
            string content = "3000111122223333444455556666";
            string crc = "1835";

            var result = BuildCrc16(content);
            Assert.AreEqual(crc, result);
        }

        [TestMethod]
        public void Test_crc16_11()
        {
            string content = "3000170328087903000084560000";
            string crc = "DAD5";

            var result = BuildCrc16(content);
            Assert.AreEqual(crc, result);
        }

        static string BuildCrc16(string content_hex)
        {
            var content_bytes = ByteArray.GetTimeStampByteArray(content_hex);

            // 检查一下 bytes 数应该是偶数
            if (content_bytes.Length % 2 != 0)
                throw new ArgumentException($"content_hex 参数中的 bytes 数不是偶数");
            
            var crc_bytes = CRC16.crc16x25(content_bytes);
            return ByteArray.GetHexTimeStampString(crc_bytes).ToUpper();
        }
    }
}
