using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.Script;

namespace TestDp2Library
{
    [TestClass]

    public class TestAssemblyCache
    {
        [TestMethod]
        public void Test_AssemblyCache_01()
        {
            ObjectCache<SampleClass> objectCache = new ObjectCache<SampleClass>();

            var result = objectCache.GetObject("", null);
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void Test_AssemblyCache_02()
        {
            ObjectCache<SampleClass> objectCache = new ObjectCache<SampleClass>();

            var obj = new SampleClass { Text = "text"};
            objectCache.SetObject("test", obj);
            var result = objectCache.GetObject("test", null);
            Assert.AreEqual(obj, result);
            Assert.AreEqual("text", obj.Text);
        }

        [TestMethod]
        public void Test_AssemblyCache_03()
        {
            ObjectCache<SampleClass> objectCache = new ObjectCache<SampleClass>();

            var result = objectCache.GetObject("test", () => {
                return new SampleClass { Text = "text" };
            });
            Assert.AreEqual("text", result.Text);
        }

        class SampleClass
        {
            public string Text { get; set; }
        }
    }
}
