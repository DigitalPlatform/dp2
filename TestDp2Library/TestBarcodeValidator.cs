using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer.Common;

namespace TestDp2Library
{
    [TestClass]
    public class TestBarcodeValidator
    {

        // 匹配 entity
        [TestMethod]
        public void Test01()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
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
                Assert.AreEqual(result.OK, true);
                Assert.AreEqual(result.Type, "entity");
                /*
                Assert.AreEqual(result.TransformedBarcode, null);
                Assert.AreEqual(result.Transformed, false);
                */
            }
        }

        // 匹配 patron
        [TestMethod]
        public void Test02()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
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
                var result = validator.Validate("海淀分馆", "P000001");
                Assert.AreEqual(result.OK, true);
                Assert.AreEqual(result.Type, "patron");
                /*
                Assert.AreEqual(result.TransformedBarcode, null);
                Assert.AreEqual(result.Transformed, false);
                */
            }
        }


        // range 无法匹配
        [TestMethod]
        public void Test03()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' transform='...' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆", "P0000001");
                Assert.AreEqual(result.OK, false);
                Assert.AreEqual(result.ErrorCode, "notMatch");
                /*
                Assert.AreEqual(result.TransformedBarcode, null);
                Assert.AreEqual(result.Transformed, false);
                */
            }
        }

        // location 无法匹配
        [TestMethod]
        public void Test04()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' transform='...' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆1", "P0000001");
                Assert.AreEqual(result.OK, false);
                Assert.AreEqual(result.ErrorCode, "locationDefNotFound");
                /*
                Assert.AreEqual(result.TransformedBarcode, null);
                Assert.AreEqual(result.Transformed, false);
                */
            }
        }

        // 匹配 entity，同时变换条码。变换脚本元素 tranform 在 validator 元素下
        [TestMethod]
        public void Test10()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
               <transform>
                    barcode+'tail'
               </transform>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆", "0000001");
                Assert.AreEqual(result.OK, true);
                Assert.AreEqual(result.Type, "entity");
                /*
                Assert.AreEqual(result.TransformedBarcode, "0000001tail");
                Assert.AreEqual(result.Transformed, true);
                */
            }
        }

        // 匹配 entity，同时变换条码。变换脚本元素 tranform 在 validator 元素下
        [TestMethod]
        public void Test10_transform()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
               <transform>
                    barcode+'tail'
               </transform>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Transform("海淀分馆", "0000001");
                Assert.AreEqual(result.OK, true);
                Assert.AreEqual(result.Type, null);
                Assert.AreEqual(result.TransformedBarcode, "0000001tail");
                Assert.AreEqual(result.Transformed, true);
            }
        }

        // 匹配 entity，同时变换条码。变换脚本在 range 元素的 transform 属性中
        [TestMethod]
        public void Test11()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999' transform='barcode+&quot;tail&quot;'></range>
               </entity>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆", "0000001");
                Assert.AreEqual(result.OK, false);
                Assert.AreEqual(result.Type, null);
#if NO
                Assert.AreEqual(result.TransformedBarcode, "0000001tail");
                Assert.AreEqual(result.Transformed, true);
#endif
            }
        }

        // 匹配 entity，同时变换条码。变换脚本在 range 元素的 transform 属性中
        [TestMethod]
        public void Test11_transform()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999' transform='barcode+&quot;tail&quot;'></range>
               </entity>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Transform("海淀分馆", "0000001");
                Assert.AreEqual(result.OK, true);
                Assert.AreEqual(result.Type, "entity");
                Assert.AreEqual(result.TransformedBarcode, "0000001tail");
                Assert.AreEqual(result.Transformed, true);
            }
        }

        // 匹配 entity，同时变换条码。
        // 变换脚本元素 tranform 在 validator 元素下，和在 range 元素 transform 属性里面都有，按照规则实际变换时使用 range 元素里面的脚本
        [TestMethod]
        public void Test12()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999' transform='barcode+&quot;tail1&quot;'></range>
               </entity>
               <transform>
                    barcode+'tail2'
               </transform>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆", "0000001");
                Assert.AreEqual(result.OK, false);
                Assert.AreEqual(result.Type, null);
#if NO
                Assert.AreEqual(result.TransformedBarcode, "0000001tail1");
                Assert.AreEqual(result.Transformed, true);
#endif
            }
        }

        [TestMethod]
        public void Test12_transform()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999' transform='barcode+&quot;tail1&quot;'></range>
               </entity>
               <transform>
                    barcode+'tail2'
               </transform>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Transform("海淀分馆", "0000001");
                Assert.AreEqual(result.OK, true);
                Assert.AreEqual(result.Type, "entity");
                Assert.AreEqual(result.TransformedBarcode, "0000001tail1");
                Assert.AreEqual(result.Transformed, true);
            }
        }

        // 匹配 entity，同时变换条码。
        // 变换脚本有语法错误(元素 tranform 在 validator 元素下)
        [TestMethod]
        public void Test13()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
               <transform>
                    barcode----'tail'
               </transform>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆", "0000001");
                Assert.AreEqual(result.OK, true);
                // Assert.AreEqual(result.ErrorCode, "scriptError");
                Assert.AreEqual(result.ErrorCode, null);

                Assert.AreEqual(result.Type, "entity");
#if NO
                Assert.AreEqual(result.TransformedBarcode, null);
                Assert.AreEqual(result.Transformed, false);
#endif
            }
        }

        [TestMethod]
        public void Test13_transform()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
               <transform>
                    barcode----'tail'
               </transform>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Transform("海淀分馆", "0000001");
                Assert.AreEqual(result.OK, false);
                Assert.AreEqual(result.ErrorCode, "scriptError");

                Assert.AreEqual(result.Type, null);
                Assert.AreEqual(result.TransformedBarcode, null);
                Assert.AreEqual(result.Transformed, false);
            }
        }

        // 进行变换，但没有匹配上的例子
        [TestMethod]
        public void Test14_transform()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
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
                var result = validator.Transform("海淀分馆", "T0000001");
                Assert.AreEqual(result.OK, true);
                Assert.AreEqual(result.ErrorCode, "notMatch");

                Assert.AreEqual(result.Type, null);
                Assert.AreEqual(result.TransformedBarcode, null);
                Assert.AreEqual(result.Transformed, false);
            }
        }


        // 没有变换脚本
        [TestMethod]
        public void TestNeedValidation00()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
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
                var result = validator.NeedValidate("海淀分馆");
                Assert.AreEqual(result, false);
            }
        }

        // 没有变换脚本，并且 location 也不匹配
        [TestMethod]
        public void TestNeedValidation01()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
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
                var result = validator.NeedValidate("西城分馆");
                Assert.AreEqual(result, false);
            }
        }

        // 有变换脚本，但 location 没有匹配
        // 变换脚本元素 tranform 在 validator 元素下
        [TestMethod]
        public void TestNeedValidation02()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
               <transform>
                    barcode+'tail'
               </transform>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.NeedValidate("西城分馆");
                Assert.AreEqual(result, false);
            }
        }


        // 变换脚本元素 tranform 在 validator 元素下
        [TestMethod]
        public void TestNeedValidation03()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
               <transform>
                    barcode+'tail'
               </transform>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.NeedValidate("海淀分馆");
                Assert.AreEqual(result, true);
            }
        }

        // 变换脚本元素 tranform 在 range 元素下
        [TestMethod]
        public void TestNeedValidation04()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999' transform='barcode+&quot;tail&quot;'></range>
               </entity>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.NeedValidate("海淀分馆");
                Assert.AreEqual(result, true);
            }
        }

        // 变换脚本元素 tranform 在 validator 和 range 元素下都有
        [TestMethod]
        public void TestNeedValidation05()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999' transform='barcode+&quot;tail&quot;'></range>
               </entity>
               <transform>
                    barcode+'tail'
               </transform>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.NeedValidate("海淀分馆");
                Assert.AreEqual(result, true);
            }
        }

    }
}
