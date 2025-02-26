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
    }
}
