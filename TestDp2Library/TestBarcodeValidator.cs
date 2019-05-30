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
        public void Test1()
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
                var result = validator.Validate("海淀分馆", "0000001");
                Assert.AreEqual(result.OK, true);
                Assert.AreEqual(result.Type, "entity");
                Assert.AreEqual(result.Transformed, "0000001");
            }
        }

        // 匹配 patron
        [TestMethod]
        public void Test2()
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
                var result = validator.Validate("海淀分馆", "P000001");
                Assert.AreEqual(result.OK, true);
                Assert.AreEqual(result.Type, "patron");
                Assert.AreEqual(result.Transformed, "P000001");
            }
        }


        // range 无法匹配
        [TestMethod]
        public void Test3()
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
            }
        }

        // location 无法匹配
        [TestMethod]
        public void Test4()
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
            }
        }

    }
}
