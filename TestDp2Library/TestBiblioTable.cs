using System;
using DigitalPlatform.Text;
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
    <line name='题名与责任者' value='AAAA / FFFF. AAAA / FFFF' type='title_area' />
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


        //以下是兔老师提供

        [TestMethod]
        public void TestMarcTable_200_01()
        {
            // MARC 工作单格式$a$b$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa经贸合作研究ǂb专著ǂf何军明著";

            // table 的 XML 格式
            // 注意，] 后面携带一个空格，/ 之前又有一个空格，两个空格要舍掉一个才是正确的，这个测试比较重要
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='经贸合作研究 [专著] / 何军明著' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_02()
        {
            // MARC 工作单格式$a$f$g
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa经贸合作研究ǂf何军明主编ǂg张晓涛编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='经贸合作研究 / 何军明主编 ; 张晓涛编' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_02_1()
        {
            // MARC 工作单格式$a$f$f  错误的著录情况
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa经贸合作研究ǂf何军明主编ǂf张晓涛编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='经贸合作研究 / 何军明主编 ; 张晓涛编' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_03()
        {
            // MARC 工作单格式$a$e$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa经贸合作研究ǂAjing Mao He Zuo Yan Jiuǂe潜力与对策ǂf何军明著";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='经贸合作研究 : 潜力与对策 / 何军明著' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_04()
        {
            // MARC 工作单格式$a$d$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa解读中原地区三大国家战略规划与“一带一路”战略ǂdInterpretation of three strategic of planning of central plains region and the belt & roadǂf张改素, 丁志伟主编ǂzeng";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='解读中原地区三大国家战略规划与“一带一路”战略 = Interpretation of three strategic of planning of central plains region and the belt &amp; road / 张改素, 丁志伟主编' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_05()
        {
            // MARC 工作单格式$a$e$d$e$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa“一带一路”与中亚的繁荣稳定ǂA“yi Dai Yi Lu ”yu Zhong Ya De Fan Rong Wen Dingǂe“一带一路”与中亚国际论坛论文集ǂd= 《Один пояс, один путь》и стабильность, процветание в ЦАǂeмаждународный форум《одни пояс, один путь》и ЦА сборник статйǂf主编张恒龙ǂzrus";

            // table 的 XML 格式
            // $d 内容前面的 “= ” 要去掉，避免和另外添加的 = 重复
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='“一带一路”与中亚的繁荣稳定 : “一带一路”与中亚国际论坛论文集 = 《Один пояс, один путь》и стабильность, процветание в ЦА : маждународный форум《одни пояс, один путь》и ЦА сборник статй / 主编张恒龙' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_06()
        {
            // MARC 工作单格式$a$h$i$b$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa海峡西岸经济区发展报告ǂAHai Xia Xi An Jing Ji Qu Fa Zhan Bao Gaoǂh2017ǂi基于“一带一路”和自贸区背景ǂb专著ǂf洪永淼主编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='海峡西岸经济区发展报告. 2017, 基于“一带一路”和自贸区背景 [专著] / 洪永淼主编' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_07()
        {
            // MARC 工作单格式$a$f$h$i$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa海峡西岸经济区发展报告ǂf丁志为主编ǂh2017ǂi基于“一带一路”和自贸区背景ǂf洪永淼编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='海峡西岸经济区发展报告 / 丁志为主编. 2017, 基于“一带一路”和自贸区背景 / 洪永淼编' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_08()
        {
            // MARC 工作单格式$a$i$d$i$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa中国与“一带一路”沿线国家经贸合作国别报告ǂAZhong Guo Yu “yi Dai Yi Lu ”yan Xian Guo Jia Jing Mao He Zuo Guo Bie Bao Gaoǂi东亚、中亚与西亚篇ǂdCountry reports on the economic and trade cooperation between China and countries along the belt and roadǂiEast Asia, central Asia and West Asiaǂf张晓涛著ǂzeng";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='中国与“一带一路”沿线国家经贸合作国别报告. 东亚、中亚与西亚篇 = Country reports on the economic and trade cooperation between China and countries along the belt and road. East Asia, central Asia and West Asia / 张晓涛著' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_09()
        {
            // MARC 工作单格式$a$f$i$b$e$d$f$i$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa“一带一路”国家国情ǂf总主编刘作奎ǂi俄罗斯ǂb专著ǂe政治、经济、文化ǂd= One belt and one road national conditionǂfLiu ZuokuiǂiRussiaǂePolitics, economy, cultureǂf主编李晶ǂzeng";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='“一带一路”国家国情 / 总主编刘作奎. 俄罗斯 [专著] : 政治、经济、文化 = One belt and one road national condition / Liu Zuokui. Russia : Politics, economy, culture / 主编李晶' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_10()
        {
            // MARC 工作单格式$a$b$a$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa西岸经济ǂb专著ǂe形势ǂa东岸经济ǂe趋势ǂf丁志为主编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='西岸经济 [专著] : 形势 ; 东岸经济 : 趋势 / 丁志为主编' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_11()
        {
            // MARC 工作单格式$a$f$c$f$g
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa花间集ǂf(后蜀) 赵崇祚辑ǂc花间集补ǂf(明) 温博辑ǂg陈晨校点";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='花间集 / (后蜀) 赵崇祚辑. 花间集补 / (明) 温博辑 ; 陈晨校点' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_200_11_1()
        {
            // MARC 工作单格式$a$f$a$f$g 错误的著录情况
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa花间集ǂf(后蜀) 赵崇祚辑ǂa花间集补ǂf(明) 温博辑ǂg陈晨校点";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任者' value='花间集 / (后蜀) 赵崇祚辑. 花间集补 / (明) 温博辑 ; 陈晨校点' type='title_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_titlepinyin_1()
        {
            // MARC 工作单格式，拼音A
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa“一带一路”沿线国家国情研究ǂA“yi Dai Yi Lu ”yan Xian Guo Jia Guo Qing Yan Jiuǂi东南亚六国国情研究";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任说明拼音' value='“yi Dai Yi Lu ”yan Xian Guo Jia Guo Qing Yan Jiu' type='titlepinyin' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "titlepinyin",
                strTableXml);
        }


        [TestMethod]
        public void TestMarcTable_titlepinyin_2()
        {
            // MARC 工作单格式，拼音9
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa“一带一路”沿线国家国情研究ǂ9“yi Dai Yi Lu ”yan Xian Guo Jia Guo Qing Yan Jiuǂi东南亚六国国情研究";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名与责任说明拼音' value='“yi Dai Yi Lu ”yan Xian Guo Jia Guo Qing Yan Jiu' type='titlepinyin' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "titlepinyin",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_title_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa“一带一路”沿线国家国情研究ǂ9“yi Dai Yi Lu ”yan Xian Guo Jia Guo Qing Yan Jiuǂi东南亚六国国情研究";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名' value='“一带一路”沿线国家国情研究. 东南亚六国国情研究' type='title' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_title_2()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa海峡西岸经济区发展报告ǂAHai Xia Xi An Jing Ji Qu Fa Zhan Bao Gaoǂh2017ǂi基于“一带一路”和自贸区背景ǂb专著ǂf洪永淼主编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名' value='海峡西岸经济区发展报告. 2017, 基于“一带一路”和自贸区背景' type='title' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title",
                strTableXml);
        }


        [TestMethod]
        public void TestMarcTable_title_3()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa海峡西岸经济区发展报告ǂAHai Xia Xi An Jing Ji Qu Fa Zhan Bao Gaoǂe基于“一带一路”和自贸区背景ǂb专著ǂf洪永淼主编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名' value='海峡西岸经济区发展报告 : 基于“一带一路”和自贸区背景' type='title' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_title_4()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa花间集ǂf(后蜀) 赵崇祚辑ǂc花间集补ǂf(明) 温博辑ǂg陈晨校点";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名' value='花间集. 花间集补' type='title' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_title_5()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa西岸经济ǂb专著ǂe形势ǂa东岸经济ǂe趋势ǂf丁志为主编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名' value='西岸经济 : 形势 ; 东岸经济 : 趋势' type='title' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_title_6()
        {
            // MARC 工作单格式$a$f$i$b$e$d$f$i$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa“一带一路”国家国情ǂf总主编刘作奎ǂi俄罗斯ǂb专著ǂe政治、经济、文化ǂd= One belt and one road national conditionǂfLiu ZuokuiǂiRussiaǂePolitics, economy, cultureǂf主编李晶ǂzeng";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='题名' value='“一带一路”国家国情. 俄罗斯 : 政治、经济、文化 = One belt and one road national condition. Russia : Politics, economy, culture' type='title' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "title",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_author_1()
        {
            // MARC 工作单格式$a$f$i$b$e$d$f$i$f
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa“一带一路”国家国情ǂf总主编刘作奎ǂi俄罗斯ǂb专著ǂe政治、经济、文化ǂd= One belt and one road national conditionǂfLiu ZuokuiǂiRussiaǂePolitics, economy, cultureǂf主编李晶ǂzeng";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='责任者' value='总主编刘作奎 ; Liu Zuokui ; 主编李晶' type='author' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "author",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_author_2()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2001 ǂa经贸合作研究ǂf何军明主编ǂg张晓涛编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='责任者' value='何军明主编 ; 张晓涛编' type='author' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "author",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_205_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
205  ǂa新2版ǂb增订本";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='版本项' value='新2版, 增订本' type='edition_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "edition_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_205_2()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
205  ǂa修订本ǂf王菊丽补订";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='版本项' value='修订本 / 王菊丽补订' type='edition_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "edition_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_specific_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
206  ǂa三千万分之一";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='三千万分之一' type='material_specific_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "material_specific_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_206_ex1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2060 ǂbScale 1:6 336 000ǂdW 170º-W 50º/N 80º-N 40º";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='Scale 1:6 336 000 (W 170º-W 50º/N 80º-N 40º)' type='material_specific_area' />
</root>";
            TestUtility.VerifyTableXml(strWorksheet,
    "material_specific_area",
    strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_206_ex2()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2060 ǂbScale 1:250 000ǂbVertical scale 1:125 000ǂcUniversal Transverse Mercator proj.ǂdW 124º- W 122º/N 58º-N57º";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='Scale 1:250 000. Vertical scale 1:125 000 ; Universal Transverse Mercator proj. (W 124º- W 122º/N 58º-N57º)' type='material_specific_area' />
</root>";
            TestUtility.VerifyTableXml(strWorksheet,
    "material_specific_area",
    strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_206_ex3()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2060 ǂeRA 16 hr. 30 min. to 19 hr. 30min./Decl. -16º to -49ºǂfeq. 1950, epoch 1948";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='(RA 16 hr. 30 min. to 19 hr. 30min./Decl. -16º to -49º ; eq. 1950, epoch 1948)' type='material_specific_area' />
</root>";
            TestUtility.VerifyTableXml(strWorksheet,
    "material_specific_area",
    strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_206_ex5()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2060 ǂbScale [ca. 1:500.000]ǂbVertical scale [ca. 1:100.000]";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='Scale [ca. 1:500.000]. Vertical scale [ca. 1:100.000]' type='material_specific_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "material_specific_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_206_ex6()
        {
            // MARC 工作单格式
            string strWorksheet =
"01106nam2 2200253   45__\r\n"
 + "2060 ǂbScale 1:25.000ǂcGauss-Kruger projectionǂdW 8º 42' 37\" W 8º 42' 34\" W 8º 31' 03\" W 8º 31' 01\" / N 41º 55' 01\" N 41º 54' 58\" N 41º 49' 37\" N 41º 49' 34\"";

            // table 的 XML 格式
            // $c 前置分号 $d 内容括在圆括号里面
            string strTableXml =
@"<root>"
+ "    <line name='资料特殊细节项' value='" +
StringUtil.GetXmlStringSimple("Scale 1:25.000 ; Gauss-Kruger projection (W 8º 42\' 37\" W 8º 42\' 34\" W 8º 31\' 03\" W 8º 31\' 01\" / N 41º 55\' 01\" N 41º 54\' 58\" N 41º 49\' 37\" N 41º 49\' 34\")")
+ "' type='material_specific_area' />"
+ "</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "material_specific_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_206_ex7()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2060 ǂbScale not givenǂeRA 16 hr. 30 min. to 19 hr. 30 min. / Decl. -16° to -49°ǂfeq. 1950, epoch 1948";

            // table 的 XML 格式
            // $e $f 之间用空格相接。然后把两者一起扩在圆括号里
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='Scale not given (RA 16 hr. 30 min. to 19 hr. 30 min. / Decl. -16° to -49° ; eq. 1950, epoch 1948)' type='material_specific_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "material_specific_area",
                strTableXml);
        }


        [TestMethod]
        public void TestMarcTable_specific_2()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
207  ǂaVol. 1, no. 1(1980, 2)-";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='Vol. 1, no. 1(1980, 2)-' type='material_specific_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "material_specific_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_specific_3()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
207  ǂaVol. 17, no, 13(1928)-vol. 26, no. 1(1937, 6)ǂa复刊：vol. 1, no. 1(1946. 8)-";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='Vol. 17, no, 13(1928)-vol. 26, no. 1(1937, 6) ; 复刊：vol. 1, no. 1(1946. 8)-' type='material_specific_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "material_specific_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_specific_4()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
208  ǂa管弦乐总谱ǂdFull score";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='管弦乐总谱 = Full score' type='material_specific_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "material_specific_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_specific_5()
        {
            // MARC 工作单格式，unimarc中定义，cnmarc中只使用$a
            string strWorksheet =
@"01106nam2 2200253   45__
206 0#ǂbScale 1:250 000ǂbVertical scale 1:125 000ǂcUniversal Transverse Mercator proj.ǂdW 124º- W 122º/N 58º-N57º";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='Scale 1:250 000. Vertical scale 1:125 000 ; Universal Transverse Mercator proj. (W 124º- W 122º/N 58º-N57º)' type='material_specific_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "material_specific_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_specific_6()
        {
            // MARC 工作单格式，unimarc中定义，cnmarc中只使用$a
            string strWorksheet =
@"01106nam2 2200253   45__
206 0#ǂeRA 16 hr. 30 min. to 19 hr. 30min./Decl. -16º to -49ºǂfeq. 1950, epoch 1948";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='资料特殊细节项' value='(RA 16 hr. 30 min. to 19 hr. 30min./Decl. -16º to -49º ; eq. 1950, epoch 1948)' type='material_specific_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "material_specific_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_210_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
210  ǂa北京ǂa上海ǂc中国发展出版社ǂd2017";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='出版发行项' value='北京 ; 上海 : 中国发展出版社, 2017' type='publication_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "publication_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_210_2()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
210  ǂa北京ǂc中国发展出版社ǂc北京出版社ǂd2017";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='出版发行项' value='北京 : 中国发展出版社 : 北京出版社, 2017' type='publication_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "publication_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_210_3()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
210  ǂa北京ǂc中国发展出版社ǂa上海ǂc上海出版社ǂd2017";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='出版发行项' value='北京 : 中国发展出版社 ; 上海 : 上海出版社, 2017' type='publication_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "publication_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_210_4()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
210  ǂa北京ǂc中国音像出版社ǂd2001ǂe北京ǂg北京电影制片厂ǂh2002";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='出版发行项' value='北京 : 中国音像出版社, 2001 (北京 : 北京电影制片厂, 2002)' type='publication_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "publication_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_pub_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
210  ǂa北京ǂc中国发展出版社ǂc北京出版社ǂd2017";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='出版者' value='中国发展出版社 : 北京出版社' type='publisher' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "publisher",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_pubtime_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
210  ǂa北京ǂc中国发展出版社ǂc北京出版社ǂd2017";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='出版时间' value='2017' type='publishtime' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "publishtime",
                strTableXml);
        }


        [TestMethod]
        public void TestMarcTable_215_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
215  ǂa320页ǂc彩图, 地图ǂd24cmǂe光盘1片";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='载体形态项' value='320页 : 彩图, 地图 ; 24cm + 光盘1片' type='material_description_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "material_description_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_pages_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
215  ǂa320页ǂc彩图, 地图ǂd24cmǂe光盘1片";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='页数' value='320页' type='pages' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "pages",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_225_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2252 ǂa智库丛书ǂi中国-中东欧国家智库系列ǂf洪永淼主编";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='丛编项' value='(智库丛书. 中国-中东欧国家智库系列 / 洪永淼主编)' type='series_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "series_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_225_2()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2252 ǂa智库丛书ǂh第一辑ǂi中国-中东欧国家智库系列";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='丛编项' value='(智库丛书. 第一辑, 中国-中东欧国家智库系列)' type='series_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "series_area",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_225_3()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
2252 ǂa智库丛书ǂe中国-中东欧国家ǂv01";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='丛编项' value='(智库丛书 : 中国-中东欧国家 ; 01)' type='series_area' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "series_area",
                strTableXml);
        }


        [TestMethod]
        public void TestMarcTable_subjects_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
6060 ǂa中外关系ǂAzhong wai guan xiǂx文化交流ǂx研究ǂy欧洲";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='主题分析' value='中外关系-文化交流-研究-欧洲' type='subjects' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "subjects",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_690_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
690  ǂaG125ǂv5";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='中图法分类号' value='G125' type='clc_class' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "clc_class",
                strTableXml);
        }

        [TestMethod]
        public void TestMarcTable_classes_1()
        {
            // MARC 工作单格式
            string strWorksheet =
@"01106nam2 2200253   45__
690  ǂaG125ǂv5";

            // table 的 XML 格式
            string strTableXml =
@"<root>
    <line name='分类号' value='中图法分类号: G125' type='classes' />
</root>";

            TestUtility.VerifyTableXml(strWorksheet,
                "classes",
                strTableXml);
        }
    }
}
