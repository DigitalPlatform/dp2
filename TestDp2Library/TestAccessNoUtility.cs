using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform;
using DigitalPlatform.Text;

namespace TestDp2Library
{
    /// <summary>
    /// 测试 DIgitalPlatform.AccessNoUtility 类
    /// 部分案例参考 https://www.docin.com/p-1110510607.html
    /// </summary>
    [TestClass]
    public class TestAccessNoUtility
    {
        // TP311 TP32
        [TestMethod]
        public void Test_anu_classline_01()
        {
            string list = @"
TP311
TP32";
            test_class_line_case(list);
        }

        // F-1 F0
        [TestMethod]
        public void Test_anu_classline_02()
        {
            string list = @"
F-1
F0";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_03()
        {
            string list = @"
B2
B3
E27
E512
TM92
TU201";
            test_class_line_case(list);
        }


        [TestMethod]
        public void Test_anu_classline_04()
        {
            string list = @"
B
B-43
B0
B3
E27
E512
TM92
TU201
X799
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_05()
        {
            string list = @"
B021
B022
B022.2
C532
C54
D035.37
D035.4
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_06()
        {
            string list = @"
B021
B022
B022.2
D035.37
D035.4
TM101
TM90
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_07()
        {
            string list = @"
TP312
TP312A
TP312C
TP312X
TP312AL
TP312BA
TP312CO
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_08()
        {
            string list = @"
F-43
F0
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_09()
        {
            string list = @"
B-49
B-53
B-61
B0
B0-0
B0-53
B1
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_10()
        {
            string list = @"
TP3
TP3-43
TP3-61
TP30
TP301
TP301.1
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_11()
        {
            string list = @"
Z88:C
Z88:F
Z89:B
Z89:H
Z89:K
Z89:T
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_12()
        {
            string list = @"
H319.4-43
H319.4:I234
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_13()
        {
            string list = @"
H319-43
H319:A312
H319:D312-42
H319:TP311
H319:X256
H319:X256-42
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_14()
        {
            string list = @"
H319:A112
H319:S312
H319:T311
H319:TB312
H319:TV312
H319:U312
H319:V311
H319:Z256
";
            test_class_line_case(list);
        }

        [TestMethod]
        public void Test_anu_classline_15()
        {
            string list = @"
B-49
B0
B0-0
B0-53
";
            test_class_line_case(list);
        }

        // 测试索取号中的其它行(所谓“其它行”就是第一行分类号以外的其它行)
        [TestMethod]
        public void Test_anu_restline_01()
        {
            string list = @"
J559B(1)2
J559T(1)
J559W(2)
J559AJ(1)
J559AJ(2)
";
            test_rest_line_case(list);
        }

        // 完整的索取号比较
        [TestMethod]
        public void Test_anu_01()
        {
            string list = @"
I247.5/A001
I247.5/A001A
I247.5/A002
I247.5/B010
I247.5/Z256
";
            test_case(list);
        }

        [TestMethod]
        public void Test_anu_02()
        {
            string list = @"
I247.5/2
I247.5/1234
";
            test_case(list);
        }

        [TestMethod]
        public void Test_anu_03()
        {
            string list = @"
J333/G251
J333(512)/G251
";
            test_case(list);
        }

        [TestMethod]
        public void Test_anu_04()
        {
            string list = @"
K835.617/E43A
K835.617/S931
K835.617/W365
K835.617=43/S894
";
            test_case(list);
        }

        [TestMethod]
        public void Test_anu_06()
        {
            string list = @"
K827/Z763A
K827/Z763B
K827/Z763Y
K827/Z763AA
";
            test_case(list);
        }

        [TestMethod]
        public void Test_anu_07()
        {
            string list = @"
O13-43/S623
O13-43/S623(2)
O13-43/S623-2(1)
O13-43/S623-2(2)
";
            test_case(list);
        }

        [TestMethod]
        public void Test_anu_08()
        {
            string list = @"
I247.58/J822-2(14)
I247.58/J822-2(15)
I247.58/J822-2(32)
";
            test_case(list);
        }

        [TestMethod]
        public void Test_anu_09()
        {
            string list = @"
H360.41-42/l338(1)1
H360.41-42/l338(1)2
H360.41-42/l338(2)2
";
            test_case(list);
        }

        // ~

        [TestMethod]
        public void Test_anu_10()
        {
            string list = @"
K20-42
K20-42/15
";
            test_case(list);
        }

        /*
         * 
R711 妇科学
R711-62 妇科学手册
R711(711) 加拿大妇科学
R711(711)=535 八十年代的加拿大妇科学
R711=6 二十一世纪妇科学展望
R711＜326＞ 国外妇科学
R711：R83 航海妇科学
R711+R173 妇科学和妇女卫生
R711.1 女性生殖器畸形
         * 
         * */

        [TestMethod]
        public void Test_anu_11()
        {
            string list = @"
R711
R711-62
R711(711)
R711(711)=535
R711=6
R711<326>
R711:R83
R711+R173
R711.1
";
            test_case(list);
        }

        // 字母部分字符数不等的案例
        [TestMethod]
        public void Test_anu_12()
        {
            string list = @"
T1
TP1
";
            test_case(list);
        }

        // 字母和数字会被分为两个 segment
        [TestMethod]
        public void Test_segment_01()
        {
            var segments = CompareSegment.ParseLine("T1");
            Assert.AreEqual(2, segments.Count);
            Assert.AreEqual("T", segments[0].Text);
            Assert.AreEqual("1", segments[1].Text);
        }

        // 字母和数字会被分为两个 segment
        [TestMethod]
        public void Test_segment_02()
        {
            var segments = CompareSegment.ParseLine("TP1");
            Assert.AreEqual(2, segments.Count);
            Assert.AreEqual("TP", segments[0].Text);
            Assert.AreEqual("1", segments[1].Text);
        }

        // 字母、横杠、数字会被分为三个 segment
        [TestMethod]
        public void Test_segment_03()
        {
            var segments = CompareSegment.ParseLine("F-1");
            Assert.AreEqual(3, segments.Count);
            Assert.AreEqual("F", segments[0].Text);
            Assert.AreEqual("1", segments[2].Text);
        }

        [TestMethod]
        public void Test_segment_04()
        {
            var segments = CompareSegment.ParseLine("TP12");
            Assert.AreEqual(2, segments.Count);
            Assert.AreEqual("TP", segments[0].Text);
            Assert.AreEqual("12", segments[1].Text);
        }

        [TestMethod]
        public void Test_matchRange_01()
        {
            string accessNo = "TP311";
            string range = "TP311~TP312";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(1, ret);
        }


        // 检测 range 错误
        [TestMethod]
        public void Test_matchRange_12()
        {
            string accessNo = "TP311";
            string range = "TP312~TP311";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(-1, ret);
        }

        // 检测 range 错误
        [TestMethod]
        public void Test_matchRange_13()
        {
            string accessNo = "TP311";
            string range = "TP311~TP311`";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(-1, ret);
        }

        // 检测 range 错误
        [TestMethod]
        public void Test_matchRange_14()
        {
            string accessNo = "TP311";
            string range = "`TP311~TP311`";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(-1, ret);
        }

        // 检测 range 错误
        [TestMethod]
        public void Test_matchRange_15()
        {
            string accessNo = "TP311";
            string range = "`TP311~TP311";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(-1, ret);
        }

        [TestMethod]
        public void Test_matchRange_21()
        {
            string accessNo = "TP311";
            string range = "TP311~TP311";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(1, ret);
        }


        [TestMethod]
        public void Test_matchRange_22()
        {
            string accessNo = "TP311";
            string range = "`TP311~TP312";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_matchRange_23()
        {
            string accessNo = "TP312";
            string range = "`TP311~TP312";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(1, ret);
        }

        [TestMethod]
        public void Test_matchRange_24()
        {
            string accessNo = "TP313";
            string range = "`TP311~TP312";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_matchRange_25()
        {
            string accessNo = "TP311";
            string range = "TP311~TP312`";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(1, ret);
        }

        [TestMethod]
        public void Test_matchRange_26()
        {
            string accessNo = "TP312";
            string range = "TP311~TP312`";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_matchRange_27()
        {
            string accessNo = "TP313";
            string range = "TP311~TP312`";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_matchRange_28()
        {
            string accessNo = "TP311";
            string range = "`TP311~TP312`";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_matchRange_29()
        {
            string accessNo = "TP311";
            string range = "`TP311~TP312`";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_matchRange_30()
        {
            string accessNo = "TP311/1";
            string range = "`TP311~TP312`";
            // return:
            //      -1  出错
            //      0   没有匹配上
            //      1   匹配上了
            var ret = AccessNoUtility.MatchRange(range,
                accessNo,
                out string strError);
            Assert.AreEqual(1, ret);
        }

        // 不交叉
        [TestMethod]
        public void Test_hasCross_01()
        {
            string range1 = "A1~A2";
            string range2 = "A3~A4";

            {
                var ret = AccessNoUtility.HasCross(range1, range2);
                Assert.AreEqual(false, ret);
            }

            {
                var ret = AccessNoUtility.HasCross(range2, range1);
                Assert.AreEqual(false, ret);
            }
        }

        // 交叉部分
        [TestMethod]
        public void Test_hasCross_02()
        {
            string range1 = "TP312~TP315";
            string range2 = "TP31~TP312";

            {
                var ret = AccessNoUtility.HasCross(range1, range2);
                Assert.AreEqual(true, ret);
            }

            {
                var ret = AccessNoUtility.HasCross(range2, range1);
                Assert.AreEqual(true, ret);
            }
        }

        // 一个完整包含另外一个
        [TestMethod]
        public void Test_hasCross_03()
        {
            string range1 = "A1~A5";
            string range2 = "A2~A4";

            {
                var ret = AccessNoUtility.HasCross(range1, range2);
                Assert.AreEqual(true, ret);
            }

            {
                var ret = AccessNoUtility.HasCross(range2, range1);
                Assert.AreEqual(true, ret);
            }
        }

        // (头尾不包含)不交叉
        [TestMethod]
        public void Test_hasCross_11()
        {
            string range1 = "A1~A2`";
            string range2 = "`A3~A4";

            {
                var ret = AccessNoUtility.HasCross(range1, range2);
                Assert.AreEqual(false, ret);
            }

            {
                var ret = AccessNoUtility.HasCross(range2, range1);
                Assert.AreEqual(false, ret);
            }
        }

        // (头尾不包含)不交叉
        [TestMethod]
        public void Test_hasCross_12()
        {
            string range1 = "A1~A2`";
            string range2 = "`A2~A4";

            {
                var ret = AccessNoUtility.HasCross(range1, range2);
                Assert.AreEqual(false, ret);
            }

            {
                var ret = AccessNoUtility.HasCross(range2, range1);
                Assert.AreEqual(false, ret);
            }
        }

        // (头尾不包含)交叉部分
        [TestMethod]
        public void Test_hasCross_13()
        {
            string range1 = "A1~A8`";
            string range2 = "`A7~A9";
            // 注：至少 A75 算是一个交叉点

            {
                var ret = AccessNoUtility.HasCross(range1, range2);
                Assert.AreEqual(true, ret);
            }

            {
                var ret = AccessNoUtility.HasCross(range2, range1);
                Assert.AreEqual(true, ret);
            }
        }

        // (头尾不包含)一个包含另外一个
        [TestMethod]
        public void Test_hasCross_14()
        {
            string range1 = "A1~A9`";
            string range2 = "`A7~A8";
            // 注：至少 A75 算是一个交叉点

            {
                var ret = AccessNoUtility.HasCross(range1, range2);
                Assert.AreEqual(true, ret);
            }

            {
                var ret = AccessNoUtility.HasCross(range2, range1);
                Assert.AreEqual(true, ret);
            }
        }

        // (头尾不包含)常见接续情况
        [TestMethod]
        public void Test_hasCross_15()
        {
            string range1 = "A1~B`";
            string range2 = "B~C";
            // 注：至少 A75 算是一个交叉点

            {
                var ret = AccessNoUtility.HasCross(range1, range2);
                Assert.AreEqual(false, ret);
            }

            {
                var ret = AccessNoUtility.HasCross(range2, range1);
                Assert.AreEqual(false, ret);
            }
        }

        #region 服务函数

        void test_class_line_case(string list)
        {
            var lines = new List<string>(list.Replace("\n", "").Split('\r'));
            StringUtil.RemoveBlank(ref lines);

            test_class_line_case(lines.ToArray());
        }

        void test_class_line_case(string[] list)
        {
            List<string> results = new List<string>();
            results.AddRange(list);

            results.Sort((a, b) =>
            {
                return AccessNoUtility.CompareAccessNoClassLine(a, b);
            });

            Assert.AreEqual(results.Count, list.Length);
            int i = 0;
            foreach (string s in results)
            {
                Assert.AreEqual(list[i], s);
                i++;
            }
        }

        void test_rest_line_case(string list)
        {
            var lines = new List<string>(list.Replace("\n", "").Split('\r'));
            StringUtil.RemoveBlank(ref lines);

            test_rest_line_case(lines.ToArray());
        }

        void test_rest_line_case(string[] list)
        {
            List<string> results = new List<string>();
            results.AddRange(list);

            results.Sort((a, b) =>
            {
                return AccessNoUtility.CompareAccessNoRestLine(a, b);
            });

            Assert.AreEqual(results.Count, list.Length);
            int i = 0;
            foreach (string s in results)
            {
                Assert.AreEqual(list[i], s);
                i++;
            }
        }

        void test_case(string list)
        {
            var lines = new List<string>(list.Replace("\n", "").Split('\r'));
            StringUtil.RemoveBlank(ref lines);

            test_case(lines.ToArray());
        }

        void test_case(string[] list)
        {
            List<string> results = new List<string>();
            results.AddRange(list);

            results.Sort((a, b) =>
            {
                return AccessNoUtility.CompareAccessNo(a, b);
            });

            Assert.AreEqual(results.Count, list.Length);
            int i = 0;
            foreach (string s in results)
            {
                Assert.AreEqual(list[i], s);
                i++;
            }
        }

        #endregion

#if REMOVED

        // L412      L412-2    L412x    L412(x2)  L412.2    L412.2-2   L412.2p   L412.2(p2)
        [TestMethod]
        public void Test_anu_line_01()
        {
            string s1 = "L412";
            string s2 = "L412-2";

            var ret = AccessNoUtility.CompareAccessNoRestLine(s1, s2);
            Assert.IsTrue(ret < 0);
        }

        [TestMethod]
        public void Test_anu_line_02()
        {
            string s1 = "L412-2";
            string s2 = "L412x";

            var ret = AccessNoUtility.CompareAccessNoRestLine(s1, s2);
            Assert.IsTrue(ret < 0);
        }


        [TestMethod]
        public void Test_anu_line_03()
        {
            string s1 = "L412x";
            string s2 = "L412(x2)";

            var ret = AccessNoUtility.CompareAccessNoRestLine(s1, s2);
            Assert.IsTrue(ret < 0);
        }
#endif

    }
}
