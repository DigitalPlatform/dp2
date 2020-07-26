using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer.Common;

namespace TestDp2Library
{
    // BarcodeValidator 的一般测试
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
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
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
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("patron", result.Type);
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
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual("notMatch", result.ErrorCode);
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
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual("locationDefNotFound", result.ErrorCode);
            }
        }

        // 默认的模式匹配
        [TestMethod]
        public void Test_pattern_01()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                    <!-- 读者证条码规则 -->
                   <range value='00000001-99999999' />
               </patron>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆", "0ABCEFGA");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual(null, result.Type);
            }
        }

        // 指定的模式匹配
        [TestMethod]
        public void Test_pattern_02()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' >
               <patron>
                    <!-- 读者证条码规则 -->
                   <range value='00000001-99999999' pattern='[0][0-Z][0-Z][0-Z][0-Z][0-Z][0-Z][0-Z]'/>
               </patron>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("海淀分馆", "0ABCEFGA");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("patron", result.Type);
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
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
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
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
                Assert.AreEqual("0000001tail", result.TransformedBarcode);
                Assert.AreEqual(true, result.Transformed);
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
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual(null, result.Type);
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
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
                Assert.AreEqual("0000001tail", result.TransformedBarcode);
                Assert.AreEqual(true, result.Transformed);
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
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual(null, result.Type);
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
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
                Assert.AreEqual("0000001tail1", result.TransformedBarcode);
                Assert.AreEqual(true, result.Transformed);
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
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual(null, result.ErrorCode);

                Assert.AreEqual("entity", result.Type);
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
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual("scriptError", result.ErrorCode);

                Assert.AreEqual(null, result.Type);
                Assert.AreEqual(null, result.TransformedBarcode);
                Assert.AreEqual(false, result.Transformed);
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
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("notMatch", result.ErrorCode);

                Assert.AreEqual(null, result.Type);
                Assert.AreEqual(null, result.TransformedBarcode);
                Assert.AreEqual(false, result.Transformed);
            }
        }

        [TestMethod]
        public void Test15_transform()
        {
            string xml = @"
<barcodeValidation>
    <validator location='第三中学'>
        <patron>
            <CMIS />
            <range value='T000001-T999999' />
            <range value='000001-999999' transform='result=&quot;T&quot;+barcode ;' />
        </patron>
        <entity>
            <range value='SZ001-SZ999' />
            <range value='Z001-Z999' transform='result= &quot;S&quot; + barcode ;' />
        </entity>
        <transform>
	if (barcode.length == 4)
	result = 'Z' + barcode;
                 else if (barcode.length == 5)
                  result = barcode;
else
	message = '待变换的输入条码号 \''+barcode+'\' 长度不对';
        </transform>
    </validator>
</barcodeValidation>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Transform("第三中学", "000001");
                Assert.AreEqual(true, result.OK);

                Assert.AreEqual("patron", result.Type);
                Assert.AreEqual("T000001", result.TransformedBarcode);
                Assert.AreEqual(true, result.Transformed);
            }
        }

        [TestMethod]
        public void Test16_transform()
        {
            string xml = @"
<barcodeValidation>
    <validator location='第三中学'>
        <patron>
            <CMIS />
            <range value='T000001-T999999' />
            <range value='000001-999999' transform='result=&quot;T&quot;+barcode ;' />
        </patron>
        <entity>
            <range value='SZ001-SZ999' />
            <range value='Z001-Z999' transform='result= &quot;S&quot; + barcode ;' />
        </entity>
        <transform>
	if (barcode.length == 4)
	result = 'Z' + barcode;
                 else if (barcode.length == 5)
                  result = barcode;
else
	message = '待变换的输入条码号 \''+barcode+'\' 长度不对';
        </transform>
    </validator>
</barcodeValidation>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Transform("第三中学", "T000001");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual("scriptError", result.ErrorCode);

                Assert.AreEqual(null, result.Type);
                Assert.AreEqual(null, result.TransformedBarcode);
                Assert.AreEqual(false, result.Transformed);
            }
        }

        [TestMethod]
        public void Test17_transform()
        {
            string xml = @"
<barcodeValidation>
    <validator location='第三中学'>
        <patron>
            <CMIS />
            <range value='T000001-T999999' />
            <range value='000001-999999' transform='result=&quot;T&quot;+barcode ;' />
        </patron>
        <entity>
            <range value='SZ001-SZ999' />
            <range value='Z001-Z999' transform='result= &quot;S&quot; + barcode ;' />
        </entity>
        <transform>
	if (barcode.length == 4)
	result = 'Z' + barcode;
                 else if (barcode.length == 5)
                  result = barcode;
else
	message = '待变换的输入条码号 \''+barcode+'\' 长度不对';
        </transform>
    </validator>
</barcodeValidation>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Transform("第三中学", "A001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual(null, result.ErrorCode);

                Assert.AreEqual(null, result.Type);
                Assert.AreEqual("ZA001", result.TransformedBarcode);
                Assert.AreEqual(true, result.Transformed);
            }
        }

        [TestMethod]
        public void Test18_transform()
        {
            string xml = @"
<barcodeValidation>
    <validator location='第三中学'>
        <patron>
            <CMIS />
            <range value='T000001-T999999' />
            <range value='000001-999999' transform='result=&quot;T&quot;+barcode ;' />
        </patron>
        <entity>
            <range value='SZ001-SZ999' />
            <range value='Z001-Z999' transform='result= &quot;S&quot; + barcode ;' />
        </entity>
        <transform>
	        if (barcode.length == 4)
	            result = 'Z' + barcode;
            else if (barcode.length == 5)
                result = barcode;
            else
	            message = '待变换的输入条码号 \''+barcode+'\' 长度不对';
        </transform>
    </validator>
</barcodeValidation>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Transform("第三中学", "A0001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual(null, result.ErrorCode);

                Assert.AreEqual(null, result.Type);
                Assert.AreEqual("A0001", result.TransformedBarcode);
                Assert.AreEqual(true, result.Transformed);
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
                var result = validator.NeedTransform("海淀分馆");
                Assert.AreEqual(false, result);
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
                var result = validator.NeedTransform("西城分馆");
                Assert.AreEqual(false, result );
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
                var result = validator.NeedTransform("西城分馆");
                Assert.AreEqual(false, result);
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
                var result = validator.NeedTransform("海淀分馆");
                Assert.AreEqual(true, result );
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
                var result = validator.NeedTransform("海淀分馆");
                Assert.AreEqual(true, result );
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
                var result = validator.NeedTransform("海淀分馆");
                Assert.AreEqual(true, result );
            }
        }

        // 虽然元素 tranform 在 validator 和 range 元素下都有，但被 validator/@suppress 压制住了
        [TestMethod]
        public void TestNeedValidation06()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' suppress=''>
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
                var result = validator.NeedTransform("海淀分馆");
                Assert.AreEqual(false, result);
            }
        }


        // 压制校验
        [TestMethod]
        public void Test_suppress_1()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆' suppress='海淀分馆 不打算提供校验'>
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
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual("suppressed", result.ErrorCode);
                Assert.AreEqual("海淀分馆 不打算提供校验", result.ErrorInfo);

                Assert.AreEqual(null, result.Type);
            }
        }

        // 压制校验
        [TestMethod]
        public void Test_suppress_2()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆'>
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
           </validator>
           <validator location='西城分馆' suppress='西城分馆 不打算提供校验'>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("西城分馆", "0000001");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual("suppressed", result.ErrorCode);
                Assert.AreEqual("西城分馆 不打算提供校验", result.ErrorInfo);

                Assert.AreEqual(null, result.Type);
            }
        }

        // 压制校验
        [TestMethod]
        public void Test_suppress_3()
        {
            string xml = @"
        <collection>
           <validator location='海淀分馆'>
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
           </validator>
           <validator location='西城分馆' suppress=''>
           </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("西城分馆", "0000001");
                Assert.AreEqual(false, result.OK);
                Assert.AreEqual("suppressed", result.ErrorCode);
                Assert.AreEqual("馆藏地 '西城分馆' 不打算定义验证规则", result.ErrorInfo);

                Assert.AreEqual(null, result.Type);
            }
        }


        // 中营瑞丽实际案例
        [TestMethod]
        public void Test_sample_01()
        {
            string xml = @"
        <collection>
    <validator location='南开区中营瑞丽小学,南开区中营瑞丽小学/*'>
        <patron>
            <CMIS />
            <range value='ZYRLP00001-ZYRLP99999' />
        </patron>
        <entity>
            <range value='ZYRL000001-ZYRL999999' />
        </entity>
    </validator>
        </collection>";

            BarcodeValidator validator = new BarcodeValidator(xml);

            {
                var result = validator.Validate("南开区中营瑞丽小学", "ZYRL000010");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            {
                var result = validator.Validate("南开区中营瑞丽小学", "ZYRL000001");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }

            {
                var result = validator.Validate("南开区中营瑞丽小学", "ZYRL000009");
                Assert.AreEqual(true, result.OK);
                Assert.AreEqual("entity", result.Type);
            }
        }
    }
}
