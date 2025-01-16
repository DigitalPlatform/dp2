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
    }
}
