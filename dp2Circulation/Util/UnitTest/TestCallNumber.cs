using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DigitalPlatform.GUI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dp2Circulation.UnitTest
{
    [TestClass]
    public class TestCallNumber
    {
        // 测试寻找空号
        // 列表为空
        [TestMethod]
        public void getBlankNumber_01()
        {
            var lines = new List<Line>()
            {
            };
            string ignore_state = null;
            string hit_number = "";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>();

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 列表只有一项，并且不是空号
        [TestMethod]
        public void getBlankNumber_02()
        {
            var lines = new List<Line>()
            {
                new Line( "中文图书实体/1", "", "I247.5/10"),
            };
            string ignore_state = null;
            string hit_number = "";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>();

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 列表只有一项，是空号
        [TestMethod]
        public void getBlankNumber_03()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/10"),
            };
            string ignore_state = null;
            string hit_number = "10";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>();

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 列表只有一项，是空号。右侧有括号部分
        [TestMethod]
        public void getBlankNumber_04()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/10(空号)"),
            };
            string ignore_state = null;
            string hit_number = "10";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>();

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 列表只有一项，是空号。范围形态，右侧有括号部分
        [TestMethod]
        public void getBlankNumber_05()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/2-10(空号)"),
            };
            string ignore_state = null;
            string hit_number = "2";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>();

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        [TestMethod]
        public void getBlankNumber_06()
        {
            var lines = new List<Line>()
            {
                new Line( "中文图书实体/1", "", "I247.5/11"),
                new Line( "", "", "I247.5/2-10(空号)"),
            };
            string ignore_state = null;
            string hit_number = "2";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>();

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        [TestMethod]
        public void getBlankNumber_07()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/2-10(空号)"),   // 虽然实际上第一个不会出现空号，但也测试一下
                new Line( "中文图书实体/1", "", "I247.5/1"),
            };
            string ignore_state = null;
            string hit_number = "2";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>();

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 空号被占用
        [TestMethod]
        public void getBlankNumber_11()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/2(空号)"),
            };
            string ignore_state = null;
            string hit_number = "";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>() {
            "2"
            };

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 占用的和列表中无关
        [TestMethod]
        public void getBlankNumber_12()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/2(空号)"),
            };
            string ignore_state = null;
            string hit_number = "2";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>() {
            "3"
            };

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 空号被占用。有一些非空号项
        [TestMethod]
        public void getBlankNumber_13()
        {
            var lines = new List<Line>()
            {
                new Line( "中文图书实体/1", "", "I247.5/3"),
                new Line( "", "", "I247.5/2(空号)"),
            };
            string ignore_state = null;
            string hit_number = "";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>() {
            "2"
            };

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 空号被占用。有一些非空号项
        [TestMethod]
        public void getBlankNumber_14()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/2(空号)"),
                new Line( "中文图书实体/1", "", "I247.5/1"),
            };
            List<string> usedEmptyNumbers = new List<string>() {
            "2"
            };
            string ignore_state = null;
            string hit_number = "";
            var items = Line.BuildList(lines.ToArray());

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 号段被占用第一个号
        [TestMethod]
        public void getBlankNumber_15()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/2-10(空号)"),
            };
            List<string> usedEmptyNumbers = new List<string>() {
            "2"
            };

            string ignore_state = null;
            string hit_number = "3";
            var items = Line.BuildList(lines.ToArray());

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 号段被占用第二个号
        [TestMethod]
        public void getBlankNumber_16()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/2-10(空号)"),
            };
            List<string> usedEmptyNumbers = new List<string>() {
            "3"
            };

            string ignore_state = null;
            string hit_number = "2";
            var items = Line.BuildList(lines.ToArray());

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 号段被占用第一、第二个号
        [TestMethod]
        public void getBlankNumber_17()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/2-10(空号)"),
            };
            List<string> usedEmptyNumbers = new List<string>() {
                "3",
                "2",    // 不要求有序
            };

            string ignore_state = null;
            string hit_number = "4";
            var items = Line.BuildList(lines.ToArray());

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 号段被占完
        [TestMethod]
        public void getBlankNumber_18()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/2-10(空号)"),
            };
            List<string> usedEmptyNumbers = new List<string>() {
                "3",
                "2",    // 不要求有序
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
            };

            string ignore_state = null;
            string hit_number = "";
            var items = Line.BuildList(lines.ToArray());

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 号段被占完，转向下一个
        [TestMethod]
        public void getBlankNumber_19()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/2-10(空号)"),
                new Line( "", "", "I247.5/11(空号)"),
            };
            List<string> usedEmptyNumbers = new List<string>() {
                "3",
                "2",    // 不要求有序
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
            };

            string ignore_state = null;
            string hit_number = "11";
            var items = Line.BuildList(lines.ToArray());

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 号段被占完，转向下一个
        [TestMethod]
        public void getBlankNumber_20()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/11(空号)"),
                new Line( "", "", "I247.5/2-10(空号)"),
            };
            List<string> usedEmptyNumbers = new List<string>() {
                "3",
                "2",    // 不要求有序
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
            };

            string ignore_state = null;
            string hit_number = "11";
            var items = Line.BuildList(lines.ToArray());

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        [TestMethod]
        public void getBlankNumber_21()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/11-50(空号)"),
                new Line( "", "", "I247.5/2-10(空号)"),
            };
            List<string> usedEmptyNumbers = new List<string>() {
                "3",
                "2",    // 不要求有序
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
            };

            string ignore_state = null;
            string hit_number = "11";
            var items = Line.BuildList(lines.ToArray());

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 忽略状态
        [TestMethod]
        public void getBlankNumber_22()
        {
            var lines = new List<Line>()
            {
                new Line( "", "test1", "I247.5/11-50(空号)"),
                new Line( "", "注销,test", "I247.5/2-10(空号)"),
            };
            List<string> usedEmptyNumbers = new List<string>()
            {

            };

            string ignore_state = "注销";
            string hit_number = "11";
            var items = Line.BuildList(lines.ToArray());

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }


        [TestMethod]
        public void getBlankNumber_31()
        {
            var lines = new List<Line>()
            {
                new Line( "中文图书实体/1", "", "I247.5/10"),
                new Line( "中文图书实体/2", "", "I247.5/9"),
                new Line( "中文图书实体/3", "", "I247.5/8"),
                new Line( "", "", "I247.5/7"),
                new Line( "中文图书实体/5", "", "I247.5/6"),
            };
            string ignore_state = null;
            string hit_number = "7";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>();

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }

        // 范围中前后颠倒
        [TestMethod]
        public void getBlankNumber_41()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/10-2(空号)"),
            };
            string ignore_state = null;
            string hit_number = "2";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>();

            try
            {
                var ret = CallNumberForm.GetBlankNumber(items,
                    usedEmptyNumbers,
                    ignore_state);
                Assert.Fail("没有如预期抛出异常");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void getBlankNumber_42()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "I247.5/-2(空号)"),
            };
            string ignore_state = null;
            string hit_number = "2";
            var items = Line.BuildList(lines.ToArray());
            List<string> usedEmptyNumbers = new List<string>();

            try
            {
                var ret = CallNumberForm.GetBlankNumber(items,
                    usedEmptyNumbers,
                    ignore_state);
                Assert.Fail("没有如预期抛出异常");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // 没有斜杠
        [TestMethod]
        public void getBlankNumber_43()
        {
            var lines = new List<Line>()
            {
                new Line( "", "", "1-2(空号)"),
            };
            List<string> usedEmptyNumbers = new List<string>();
            string ignore_state = null;
            string hit_number = "";
            var items = Line.BuildList(lines.ToArray());

            var ret = CallNumberForm.GetBlankNumber(items,
                usedEmptyNumbers,
                ignore_state);
            Assert.AreEqual(hit_number, ret);
        }


        class Line
        {
            public string RecPath;
            public string State;
            public string AccessNo;

            public Line(string recPath, string state, string accessNo)
            {
                RecPath = recPath;
                State = state;
                AccessNo = accessNo;
            }

            public static List<ListViewItem> BuildList(Line[] lines)
            {
                var items = new List<ListViewItem>();
                foreach (Line line in lines)
                {
                    var item = new ListViewItem();
                    ListViewUtil.ChangeItemText(item, CallNumberForm.COLUMN_ITEMRECPATH, line.RecPath);
                    ListViewUtil.ChangeItemText(item, CallNumberForm.COLUMN_STATE, line.State);
                    ListViewUtil.ChangeItemText(item, CallNumberForm.COLUMN_CALLNUMBER, line.AccessNo);
                    items.Add(item);
                }

                return items;
            }
        }
    }
}
