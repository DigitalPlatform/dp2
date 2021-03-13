using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dp2Circulation
{
    [TestClass]
    public class UnitTestRfidExtension
    {

        [TestMethod]
        public void Test_GetSetInformation_1()
        {
            {
                string result = MainForm.GetSetInformation("第一册");
                Assert.AreEqual(null, result);
            }

            {
                string result = MainForm.GetSetInformation("(第一册)");
                Assert.AreEqual(null, result);
            }

            {
                string result = MainForm.GetSetInformation(" 1(第一册)2 ");
                Assert.AreEqual(null, result);
            }

            {
                string result = MainForm.GetSetInformation("(1,2)");
                Assert.AreEqual("12", result);
            }

            {
                string result = MainForm.GetSetInformation(" (1,2)");
                Assert.AreEqual("12", result);
            }

            {
                string result = MainForm.GetSetInformation("(1,2) ");
                Assert.AreEqual("12", result);
            }

            {
                string result = MainForm.GetSetInformation("(11,2)");
                Assert.AreEqual("1102", result);
            }

            {
                string result = MainForm.GetSetInformation("(255,2)");
                Assert.AreEqual("255002", result);
            }

            {
                string result = MainForm.GetSetInformation("(1,99)");
                Assert.AreEqual("0199", result);
            }

            {
                string result = MainForm.GetSetInformation("(1,255)");
                Assert.AreEqual("001255", result);
            }

            {
                string result = MainForm.GetSetInformation("(1, 255)");
                Assert.AreEqual("001255", result);
            }

            {
                string result = MainForm.GetSetInformation("(1, 255 )");
                Assert.AreEqual("001255", result);
            }

            {
                string result = MainForm.GetSetInformation("(100, 20 )");
                Assert.AreEqual("100020", result);
            }
        }
    }

}
