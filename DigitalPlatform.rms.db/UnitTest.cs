using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DigitalPlatform.rms
{
    [TestClass]
    public class TestSessionInfo
    {
        [TestMethod]
        public void testSplitSegments_01()
        {
            string text = "";
            string delimeters = "\u0001\u0002";
            var results = SessionInfo.SplitSegments(text,
                delimeters);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("", results[0]);
        }

        [TestMethod]
        public void testSplitSegments_02()
        {
            string text = "abc";
            string delimeters = "\u0001\u0002";
            var results = SessionInfo.SplitSegments(text,
                delimeters);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("abc", results[0]);
        }

        [TestMethod]
        public void testSplitSegments_03()
        {
            string text = "\u0001abc";
            string delimeters = "\u0001\u0002";
            var results = SessionInfo.SplitSegments(text,
                delimeters);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("\u0001abc", results[0]);
        }

        [TestMethod]
        public void testSplitSegments_04()
        {
            string text = "\u0002abc";
            string delimeters = "\u0001\u0002";
            var results = SessionInfo.SplitSegments(text,
                delimeters);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("\u0002abc", results[0]);
        }

        [TestMethod]
        public void testSplitSegments_05()
        {
            string text = "\u0003abc";
            string delimeters = "\u0001\u0002";
            var results = SessionInfo.SplitSegments(text,
                delimeters);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("\u0003abc", results[0]);
        }

        [TestMethod]
        public void testSplitSegments_06()
        {
            string text = "\u0001\u0003abc";
            string delimeters = "\u0001\u0002";
            var results = SessionInfo.SplitSegments(text,
                delimeters);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("\u0001\u0003abc", results[0]);
        }

        [TestMethod]
        public void testSplitSegments_07()
        {
            string text = "\u0003\u0001abc";
            string delimeters = "\u0001\u0002";
            var results = SessionInfo.SplitSegments(text,
                delimeters);
            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("\u0003", results[0]);
            Assert.AreEqual("\u0001abc", results[1]);
        }

        [TestMethod]
        public void testSplitSegments_08()
        {
            string text = "\u0001\u0001";
            string delimeters = "\u0001\u0002";
            var results = SessionInfo.SplitSegments(text,
                delimeters);
            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("\u0001", results[0]);
            Assert.AreEqual("\u0001", results[1]);
        }

        [TestMethod]
        public void testSplitSegments_09()
        {
            string text = "\u0001\u0002";
            string delimeters = "\u0001\u0002";
            var results = SessionInfo.SplitSegments(text,
                delimeters);
            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("\u0001", results[0]);
            Assert.AreEqual("\u0002", results[1]);
        }

        [TestMethod]
        public void testSplitSegments_10()
        {
            string text = "\u0001abc\u0002ABC";
            string delimeters = "\u0001\u0002";
            var results = SessionInfo.SplitSegments(text,
                delimeters);
            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("\u0001abc", results[0]);
            Assert.AreEqual("\u0002ABC", results[1]);
        }


    }
}
