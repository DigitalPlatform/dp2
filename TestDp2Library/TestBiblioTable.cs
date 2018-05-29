using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestDp2Library
{
    /// <summary>
    /// 测试验证书目 table 格式的创建过程
    /// </summary>
    [TestClass]
    public class TestBiblioTable
    {
        [TestMethod]
        public void TestMarcTable_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
20010ǂa浙江1979～1988年经济发展报告ǂf浙江十年(1979～1988)经济发展的系统分析课题组编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='责任者' value='浙江十年(1979～1988)经济发展的系统分析课题组编' type='author' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "author",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_f_repeat_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
20010ǂaAAAAǂfFFFFǂaAAAAǂfFFFF";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='AAAA / FFFF ; AAAA / FFFF' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        // 测试著录错误情况
        [TestMethod]
        public void TestMarcTable_f_repeat_2()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
20010ǂaAAAAǂfFFFFǂfFFFF";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='AAAA / FFFF ; FFFF' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_f_repeat_3()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
20010ǂaAAAAǂfFFFFǂf=FFFF";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='AAAA / FFFF = FFFF' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }
    }
}
