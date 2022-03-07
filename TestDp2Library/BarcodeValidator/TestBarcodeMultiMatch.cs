using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer.Common;

namespace TestDp2Library
{
    // 测试 BarcodeValidator 的多项匹配
    [TestClass]
    public class TestBarcodeMultiMatch
    {
        [TestMethod]
        public void Test01()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆,海淀分馆/阅览室1' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            {
                var result = validator.Validate("海淀分馆/阅览室1", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            // 这个应该匹配不上
            {
                var result = validator.Validate("海淀分馆/阅览室", "0000001");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual(null, result.Type);
            }

            // https://stackoverflow.com/questions/933613/how-do-i-use-assert-to-verify-that-an-exception-has-been-thrown
            // location 中使用模式会抛出异常
            {
                try
                {
                    var result = validator.Validate("海淀分馆/阅览室*", "0000001");
                    Assert.Fail("这里本应抛出异常 ArgumentException");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is ArgumentException);
                }
            }


            // location 中使用模式会抛出异常
            {
                try
                {
                    var result = validator.Validate("海淀分馆/阅*", "0000001");
                    Assert.Fail("这里本应抛出异常 ArgumentException");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is ArgumentException);
                }
            }

        }

        [TestMethod]
        public void Test02()
        {
            string xml = @"
        <collection>
           <validator location='北院*,北院书库1,北院阅览室1' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("北院", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            {
                var result = validator.Validate("北院书库1", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            // 这个应该匹配不上
            {
                var result = validator.Validate("南院书库", "0000001");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual(null, result.Type);
            }

            // 前方一致匹配
            {
                var result = validator.Validate("北院书库", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            // 前方一致匹配
            {
                var result = validator.Validate("北院阅览室", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }
        }

        [TestMethod]
        public void Test03()
        {
            string xml = @"
        <collection>
           <validator location='北院???,北院书库1' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            // 字符数不够，匹配不上
            {
                var result = validator.Validate("北院", "0000001");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual(null, result.Type);
            }

            // 直接匹配
            {
                var result = validator.Validate("北院书库1", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            // 这个应该匹配不上
            {
                var result = validator.Validate("南院书库", "0000001");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual(null, result.Type);
            }

            // 前方一致匹配
            {
                var result = validator.Validate("北院其他库", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            // 前方一致匹配
            {
                var result = validator.Validate("北院阅览室", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }
        }


        [TestMethod]
        public void Test04()
        {
            string xml = @"
<barcodeValidation>
    <validator location=''>
        <patron>
            <CMIS />
            <range value='000001-009999' />
            <range value='P000001-P999999' />
        </patron>
        <entity>
            <range value='00000001-00999999' />
            <range value='20000001-20999999' />
            <range value='Z00001-Z99999' />
        </entity>
    </validator>
    <validator location='北院*'>
        <patron>
            <CMIS />
            <range value='N0000-N9999' />
            <range value='DZ0000-DZ9999' />
        </patron>
        <entity>
            <range value='BY200000000-BY999999999' />
            <range value='200000000-999999999' transform='result= &quot;BY&quot; + barcode ;' />
        </entity>
    </validator>
    <validator location='中院*'>
        <patron>
            <CMIS />
            <range value='N0000-N9999' />
            <range value='DZ0000-DZ9999' />
        </patron>
        <entity>
            <range value='ZY200000000-ZY999999999' />
            <range value='200000000-999999999' transform='result= &quot;ZY&quot; + barcode ;' />
        </entity>
    </validator>
    <validator location='南院*'>
        <patron>
            <CMIS />
            <range value='N0000-N9999' />
            <range value='DZ0000-DZ9999' />
        </patron>
        <entity>
            <range value='NY200000000-NY999999999' />
            <range value='200000000-999999999' transform='result= &quot;NY&quot; + barcode ;' />
        </entity>
    </validator>
</barcodeValidation>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Transform("北院", "N9999");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("patron", result.Type);
                Assert.AreEqual(false, result.Transformed);
                Assert.AreEqual(null, result.TransformedBarcode);
            }

            /*
            // 直接匹配
            {
                var result = validator.Validate("北院书库1", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            // 这个应该匹配不上
            {
                var result = validator.Validate("南院书库", "0000001");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual(null, result.Type);
            }

            // 前方一致匹配
            {
                var result = validator.Validate("北院其他库", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            // 前方一致匹配
            {
                var result = validator.Validate("北院阅览室", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }
            */
        }

        // 测试否定事项
        [TestMethod]
        public void Test10()
        {
            string xml = @"
        <collection>
           <validator location='*,!*/*' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            {
                var result = validator.Validate("阅览室", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            // 这个应该匹配不上
            {
                var result = validator.Validate("海淀分馆/阅览室1", "0000001");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual(null, result.Type);
            }

            // 这个应该匹配不上
            {
                var result = validator.Validate("海淀分馆/阅览室", "0000001");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual(null, result.Type);
            }
        }

        // 不使用否定模式，则字符串中间有 / 也会匹配上
        [TestMethod]
        public void Test11()
        {
            string xml = @"
        <collection>
           <validator location='*' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            {
                var result = validator.Validate("阅览室", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            {
                var result = validator.Validate("海淀分馆/阅览室1", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            {
                var result = validator.Validate("海淀分馆/阅览室", "0000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

        }

    }
}
