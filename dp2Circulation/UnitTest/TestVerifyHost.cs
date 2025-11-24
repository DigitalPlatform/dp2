using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    [TestClass]
    public class TestVerifyHost
    {
        // 测试 field:xxx
        [TestMethod]
        public void verifyField_field_r_01()
        {
            var wor = @"012345678901234567890123
001-------
2001 $aAAA";
            var field_name = "200";
            var condition = "field:r1";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void verifyField_field_r_02()
        {
            var wor = @"012345678901234567890123
001-------
2001 $aAAA
2001 $bBBB";
            var field_name = "200";
            var condition = "field:r1";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0].Contains($"多于"));
        }

        [TestMethod]
        public void verifyField_field_r_03()
        {
            var wor = @"012345678901234567890123
001-------
300  $aAAAAA";
            var field_name = "200";
            var condition = "field:r1";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0].Contains($"缺乏必备的"));
        }

        [TestMethod]
        public void verifyField_field_r_04()
        {
            var wor = @"012345678901234567890123
001-------
2001 $aAAA";
            var field_name = "200";
            var condition = "field:0";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0].Contains($"不允许出现"));
        }

        [TestMethod]
        public void verifyField_field_r_05()
        {
            var wor = @"012345678901234567890123
001-------
2001 $aAAA";
            var field_name = "200";
            var condition = "field:r0";
            try
            {
                var errors = runVerifyField(wor,
                    field_name,
                    condition);
                Assert.Fail("未如预期抛出异常");
            }
            catch(ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void verifyField_field_r_06()
        {
            var wor = @"012345678901234567890123
001-------
2001 $aAAA";
            var field_name = "200";
            var condition = "field:0r";
            try
            {
                var errors = runVerifyField(wor,
                    field_name,
                    condition);
                Assert.Fail("未如预期抛出异常");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void verifyField_indicator_01()
        {
            var wor = @"012345678901234567890123
001-------
2001 $aAAA";
            var field_name = "200";
            var condition = "indicator:1_";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void verifyField_indicator_02()
        {
            var wor = @"012345678901234567890123
001-------
200  $aAAA";
            var field_name = "200";
            var condition = "indicator:1_";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0].Contains($"但现在是"));
        }

        // 多项列举
        [TestMethod]
        public void verifyField_indicator_03()
        {
            var wor = @"012345678901234567890123
001-------
2001 $aAAA";
            var field_name = "200";
            var condition = "indicator:1_|2_";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void verifyField_indicator_04()
        {
            var wor = @"012345678901234567890123
001-------
200  $aAAA";
            var field_name = "200";
            var condition = "indicator:1_|2_";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0].Contains($"但现在是"));
        }

        [TestMethod]
        public void verifyField_subfield_01()
        {
            var wor = @"012345678901234567890123
001-------
200  $aAAA";
            var field_name = "200";
            var condition = "subfield:ar1";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void verifyField_subfield_02()
        {
            var wor = @"012345678901234567890123
001-------
200  $bBBB";
            var field_name = "200";
            var condition = "subfield:ar1|b";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0].Contains($"缺乏必备"));
        }

        // r 0 矛盾
        [TestMethod]
        public void verifyField_subfield_03()
        {
            var wor = @"012345678901234567890123
001-------
200  $bBBB";
            var field_name = "200";
            var condition = "subfield:ar0|b";
            try
            {
                var errors = runVerifyField(wor,
                    field_name,
                    condition);
                Assert.Fail("未如预期抛出异常");
                Assert.AreEqual(1, errors.Count);
                Assert.IsTrue(errors[0].Contains($"缺乏必备"));
            }
            catch(ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // r 0 矛盾
        [TestMethod]
        public void verifyField_subfield_04()
        {
            var wor = @"012345678901234567890123
001-------
200  $bBBB";
            var field_name = "200";
            var condition = "subfield:a0r|b";
            try
            {
                var errors = runVerifyField(wor,
                    field_name,
                    condition);
                Assert.Fail("未如预期抛出异常");
                Assert.AreEqual(1, errors.Count);
                Assert.IsTrue(errors[0].Contains($"缺乏必备"));
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void verifyField_subfield_05()
        {
            var wor = @"012345678901234567890123
001-------
200  $aAAA";
            var field_name = "200";
            var condition = "subfield:ar";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void verifyField_subfield_06()
        {
            var wor = @"012345678901234567890123
001-------
200  $aAAA$aAAAA";
            var field_name = "200";
            var condition = "subfield:ar|b";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        // 如果不想列出 |b|c，可以用 |*
        [TestMethod]
        public void verifyField_subfield_07()
        {
            var wor = @"012345678901234567890123
001-------
200  $bBBB$cCCC";
            var field_name = "200";
            var condition = "subfield:ar|*";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0].Contains("缺乏必备"));
        }

        // 不得不列出 |b|c
        [TestMethod]
        public void verifyField_subfield_08()
        {
            var wor = @"012345678901234567890123
001-------
200  $bBBB$cCCC";
            var field_name = "200";
            var condition = "subfield:ar|b|c";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0].Contains("缺乏必备"));
        }

        // 星号用法
        [TestMethod]
        public void verifyField_subfield_11()
        {
            var wor = @"012345678901234567890123
001-------
200  $bBBB$cCCC";
            var field_name = "200";
            var condition = "subfield:*1";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0].Contains("多于"));
        }

        [TestMethod]
        public void verifyField_subfield_12()
        {
            var wor = @"012345678901234567890123
001-------
200  $bBBB$cCCC";
            var field_name = "200";
            var condition = "subfield:*n";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void verifyField_subfield_13()
        {
            var wor = @"012345678901234567890123
001-------
200  $bBBB$cCCC";
            var field_name = "200";
            var condition = "subfield:a0|*n";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void verifyField_subfield_14()
        {
            var wor = @"012345678901234567890123
001-------
200  $bBBB$cCCC";
            var field_name = "200";
            var condition = "subfield:b1|*1";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        // $9
        [TestMethod]
        public void verifyField_subfield_21()
        {
            var wor = @"012345678901234567890123
001-------
2001 $a中国$9zhong guo";
            var field_name = "200";
            var condition = "subfield:a|9r";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void verifyField_subfield_22()
        {
            var wor = @"012345678901234567890123
001-------
2001 $a中国";
            var field_name = "200";
            var condition = "subfield:a|9r";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0].Contains("缺乏"));
        }

        // 如果 $a 内容为纯英文，那么无论如何没法创建对应的 $9 拼音子字段。所以不能苛求此时必有 $9
        [TestMethod]
        public void verifyField_subfield_23()
        {
            var wor = @"012345678901234567890123
001-------
2001 $aEnglish";
            var field_name = "200";
            var condition = "subfield:a|9r";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void verifyField_91()
        {
            var wor = @"012345678901234567890123
001-------
2001 $aAAA";
            var field_name = "200";
            var condition = "field:r1,indicatior:1_,subfield:ar";
            var errors = runVerifyField(wor,
                field_name,
                condition);
            Assert.AreEqual(0, errors.Count);
        }

        List<string> runVerifyField(string worksheet,
            string field_name,
            string condition)
        {
            var record = MarcRecord.FromWorksheet(worksheet.Replace("$", "ǂ"));
            // 校验字段的必备性，指示符值，字段中子字段的必备性
            // 注: 单字符参数的含义如下:
            // r 必备
            // o 可选
            // n 可重复
            // 1 不可重复
            // parameters:
            //      condition   校验要求。
            //                  field:xxxx 字段要求
            //                  subfield:axxxx|bxxxx|cxxxx 子字段要求
            //                  indicator:xx|xx|xx 指示符值要求。注意空格用下划线替代
            var errors = VerifyBase.VerifyField(record, field_name, condition);
            Console.WriteLine(StringUtil.MakePathList(errors, "\r\n"));
            return errors;
        }
    }
}
