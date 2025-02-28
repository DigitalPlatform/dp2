using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace dp2SSL.SIP2
{
    [TestClass]
    public class UnitTestSip2
    {
        [TestMethod]
        public void Test_default_01()
        {
            var message = new Checkout_11();

            Assert.AreEqual(4, message.FixedLengthFields.Count);
            Assert.AreEqual(8, message.VariableLengthFields.Count);
        }

        // 测试没有定义的情况下的功能
        [TestMethod]
        public void Test_emptyRule_01()
        {
            var message = new Checkout_11();
            message.ClearMessageRule();
            message.FixedLengthFields.Clear();
            message.VariableLengthFields.Clear();

            message.AA_PatronIdentifier_r = "0001";

            Assert.AreEqual(0, message.FixedLengthFields.Count);
            Assert.AreEqual(1, message.VariableLengthFields.Count);

            Assert.AreEqual("0001", message.AA_PatronIdentifier_r);
        }

        [TestMethod]
        public void Test_emptyRule_02()
        {
            var message = new Checkout_11();
            message.ClearMessageRule();
            message.FixedLengthFields.Clear();
            message.VariableLengthFields.Clear();

            message.SetFixedFieldValue("##", "1234");
            message.AA_PatronIdentifier_r = "0001";

            Assert.AreEqual(1, message.FixedLengthFields.Count);
            Assert.AreEqual(1, message.VariableLengthFields.Count);

            Assert.AreEqual("1234", message.GetFixedFieldValue("##"));
            Assert.AreEqual("0001", message.AA_PatronIdentifier_r);
        }

        [TestMethod]
        public void Test_verify_01()
        {
            // 注: 这个消息的定长字段部分过短了，造成解析出来的第一个变长字段有问题
            string message = "98YYYYYN18018020250226    095853AOCN-0000001-ZG|AF|AG|AY1AZF38D";

            var m = new BaseMessage();
            // 解析字符串命令为对象
            // parameters:
            //      style   解析风格。
            //              目前可以包含 verify:xxx 子参数。
            //              verify:xxx 可以为"verify" 或者 "verify:fix|var|requir"。fix|var|requir 分别表示要校验固定长字段、变长字段、字段必备性
            var ret = m.Parse(message,
            "verify",
            out string error);
            Assert.IsTrue(string.IsNullOrEmpty(error) == false);
        }

        // 


        [TestMethod]
        public void Test_verify_02()
        {
            string message = "98YYYYYN18018020250226    0958532.00AOCN-0000001-ZG|AF|AG";

            var m = new BaseMessage();
            // 解析字符串命令为对象
            // parameters:
            //      style   解析风格。
            //              目前可以包含 verify:xxx 子参数。
            //              verify:xxx 可以为"verify" 或者 "verify:fix|var|requir"。fix|var|requir 分别表示要校验固定长字段、变长字段、字段必备性
            var ret = m.Parse(message,
            "verify",
            out string error);
            Assert.AreEqual("", error);
        }

        [TestMethod]
        public void Test_checksum_01()
        {
            {
                string message = "990 402.00";
                string correct = "990 402.00AY1AZFCB4";
                var result = BaseMessage.SetChecksum(Encoding.UTF8,
                message,
                '1',
                message.Length);
                Assert.AreEqual(correct, result);
            }

            // <CR>
            {
                string message = "9900401.00";
                string correct = "9900401.00AY1AZFCA5";
                var result = BaseMessage.SetChecksum(Encoding.UTF8,
                message,
                '1',
                message.Length);
                Assert.AreEqual(correct, result);
            }
        }

        [TestMethod]
        public void Test_checksum_02()
        {
            string message = "2300119960212    100239AOid_21|104000000105|AC|AD|AY2AZF400";
            string correct = "2300119960212    100239AOid_21|104000000105|AC|AD|AY2AZF400";
            var result = BaseMessage.SetChecksum(Encoding.UTF8,
            message);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void Test_checksum_03()
        {
            string message = "6300119980723    091905Y         AOInstitutionID|AAPatronID|BP00001|BQ00005|AY1AZEA83";
            string correct = "6300119980723    091905Y         AOInstitutionID|AAPatronID|BP00001|BQ00005|AY1AZEA83";
            var result = BaseMessage.SetChecksum(Encoding.UTF8,
            message);
            Assert.AreEqual(correct, result);
        }

        // 
        [TestMethod]
        public void Test_checksum_04()
        {
            string message = "01N19960213    162352AO|ALCARD BLOCK TEST|AA104000000705|AC|AY2AZF02F";
            string correct = "01N19960213    162352AO|ALCARD BLOCK TEST|AA104000000705|AC|AY2AZF02F";
            var result = BaseMessage.SetChecksum(Encoding.UTF8,
            message);
            Assert.AreEqual(correct, result);
        }

        // 2519980723    094240AOCertification Institute ID|AAPatronID|AY4AZEBF1
        // 1719980723    100000AOCertification Institute ID|ABItemBook|AY1AZEBEB
        // 11YN19960212   10051419960212   100514AO|AA104000000105|AB000000000005792|AC|AY3AZEDB7
        // 09N19980821    085721                  APCertification Terminal Location|AOCertification Institute ID|ABCheckInBook|ACTPWord|BIN|AY2AZD6A5
        // 3719980723    0932110401USDBV111.11|AOCertification Institute ID|AAPatronID|BKTransactionID|AY2AZE1EF<
        //  3519980723    094014AOCertification Institute ID|AAPatronID|AY3AZEBF2
        // 941AY3AZFDFA
        // 98YYYNYN01000319960214    1447001.00AOID_21|AMCentral Library|ANtty30|AFScreen Message|AGPrint Message|AY1AZDA74
        //  24              00119960212    100239AO|AA104000000105|AEJohn Doe|AFScreen Message|AGCheck Print message|AY2AZDFC4
        // 64              00119980723    104009000100000002000100020000AOInstitutionID for PatronID|AAPatronID|AEPatron Name|BZ0002|CA0003|CB0010|BLY|ASItemID1 for PatronID|AUChargeItem1|AUChargeItem2|BDHome Address|BEE Mail Address|BFHome Phone for PatronID|AFScreenMessage 0 for PatronID, Language 1|AFScreen Message 1 for PatronID, Language 1|AFScreen Message 2 for PatronID, Language 1|AGPrint Line 0 for PatronID, Language 1|AGPrint Line 1 for PatronID, Language 1|AGPrint Line 2 for PatronID, language 1|AY4AZ608F
        //  26              00119980723    111413AOInstitutionID for PatronID|AAPatronID|AEPatron Name|BLY|AFScreenMessage 0 for PatronID, Language 1|AFScreen Message 1 for PatronID, Language 1|AGPrint Line 0 for PatronID, Language 1|AY7AZ8EA6
        // 1808000119980723    115615CF00000|ABItemBook|AJTitle For Item Book|CK003|AQPermanent Location for ItemBook, Language 1|APCurrent Location ItemBook|CHFree-form text with new item property|AY0AZC05B
        //  38Y19980723    111035AOInstitutionID for PatronID|AAPatronID|AFScreenMessage 0 for PatronID, Language 1|AGPrint Line 0 for PatronID, Language 1|AY6AZ9716
        // 36Y19980723    110658AOInstitutionID for PatronID|AAPatronID|AFScreenMessage 0 for PatronID, Language 1|AFScreen Message 1 for PatronID, Language 1|AFScreen Message 2 for PatronID, Language 1|AGPrint Line 0 for PatronID, Language 1|AGPrint Line 1 for PatronID, Language 1|AGPrint Line 2 for PatronID, language 1|AY5AZ970F
        // 
    }
}
