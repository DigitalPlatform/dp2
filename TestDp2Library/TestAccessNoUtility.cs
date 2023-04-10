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
