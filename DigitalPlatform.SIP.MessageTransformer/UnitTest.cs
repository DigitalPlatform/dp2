using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DigitalPlatform.SIP
{

    [TestClass]
    public class TestMessageTransformer
    {
        [TestMethod]
        public void Test_ignore_01()
        {
            string rule = @"
msg:93:beg:4
fld:CN:ign
";
            string message = "9300CNcirculation|COtest";
            string correct = "9300COtest";
            MessageTransformer t = MessageTransformer.Instance();
            t.Initial(rule);
            t.Process(message, out string result);

            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void Test_ignore_02()
        {
            string rule = @"
msg:93:beg:2
fld:CN:ign
";
            string message = "9300CNcirculation|COtest";
            string correct = "9300CNcirculation|COtest";
            MessageTransformer t = MessageTransformer.Instance();
            t.Initial(rule);
            t.Process(message, out string result);

            Assert.AreEqual(correct, result);
        }
    }
}
