using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.Marc;



namespace TestDp2Library
{
    [TestClass]
    public class TestFieldNameList
    {
        #region contains

        // 测试 Contains
        [TestMethod]
        public void Test_fieldNameList_contains_00()
        {
            string range = "***-***";
            string seg = "###";
            var fieldNameList = new FieldNameList();
            var ret = fieldNameList.Build(range,
                out string strError);
            Assert.AreEqual(0, ret);
            var contains = fieldNameList.Contains(seg);
            Assert.IsTrue(contains);
        }

        [TestMethod]
        public void Test_fieldNameList_contains_01()
        {
            string range = "***";
            string seg = "###";
            var fieldNameList = new FieldNameList();
            var ret = fieldNameList.Build(range,
                out string strError);
            Assert.AreEqual(0, ret);
            var contains = fieldNameList.Contains(seg);
            Assert.IsTrue(contains);
        }

        [TestMethod]
        public void Test_fieldNameList_contains_02()
        {
            string range = "001-999";
            string seg = "###";
            var fieldNameList = new FieldNameList();
            var ret = fieldNameList.Build(range,
                out string strError);
            Assert.AreEqual(0, ret);
            var contains = fieldNameList.Contains(seg);
            Assert.IsFalse(contains);
        }

        [TestMethod]
        public void Test_fieldNameList_contains_03()
        {
            string range = "001-999";
            string seg = "200";
            var fieldNameList = new FieldNameList();
            var ret = fieldNameList.Build(range,
                out string strError);
            Assert.AreEqual(0, ret);
            var contains = fieldNameList.Contains(seg);
            Assert.IsTrue(contains);
        }

        #endregion

        #region item sub item

        [TestMethod]
        public void Test_fieldNameList_sub_00()
        {
            string range1 = "001-999";
            string range2 = "001-999";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void Test_fieldNameList_sub_01()
        {
            string range1 = "***";
            string range2 = "***";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void Test_fieldNameList_sub_02()
        {
            string range1 = "***";
            string range2 = "###";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("000-999", results[0].ToString());
        }

        [TestMethod]
        public void Test_fieldNameList_sub_03()
        {
            string range1 = "***";
            string range2 = "000-999";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("###", results[0].ToString());
        }

        // [start1 --                  -- end1]
        //            [start2 -- end2]
        [TestMethod]
        public void Test_fieldNameList_sub_04()
        {
            string range1 = "001-004";
            string range2 = "002-003";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("001,004", results.ToString());
        }

        // 变体：左边齐平
        [TestMethod]
        public void Test_fieldNameList_sub_04_1()
        {
            string range1 = "001-004";
            string range2 = "001-003";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("004", results.ToString());
        }

        // 变体：右边齐平
        [TestMethod]
        public void Test_fieldNameList_sub_04_2()
        {
            string range1 = "001-004";
            string range2 = "002-004";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("001", results.ToString());
        }

        // 变体：两端齐平
        [TestMethod]
        public void Test_fieldNameList_sub_04_3()
        {
            string range1 = "001-004";
            string range2 = "001-004";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        // [start1 --         -- end1]
        //            [start2 --        -- end2]
        [TestMethod]
        public void Test_fieldNameList_sub_05()
        {
            string range1 = "001-003";
            string range2 = "002-005";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("001", results.ToString());
        }

        // 变体：左端齐平
        [TestMethod]
        public void Test_fieldNameList_sub_05_1()
        {
            string range1 = "001-003";
            string range2 = "001-005";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        // 变体：右端齐平
        [TestMethod]
        public void Test_fieldNameList_sub_05_2()
        {
            string range1 = "001-005";
            string range2 = "002-005";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("001", results.ToString());
        }

        //         [start1 -- end1]
        // [start2                   -- end2]
        [TestMethod]
        public void Test_fieldNameList_sub_06()
        {
            string range1 = "003-004";
            string range2 = "001-006";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        // 变体：左端齐平
        [TestMethod]
        public void Test_fieldNameList_sub_06_1()
        {
            string range1 = "001-004";
            string range2 = "001-006";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        // 变体：右端齐平
        [TestMethod]
        public void Test_fieldNameList_sub_06_2()
        {
            string range1 = "003-006";
            string range2 = "001-006";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        // all 减去 all
        [TestMethod]
        public void Test_fieldNameList_sub_07()
        {
            string range1 = "***";
            string range2 = "***";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        // 没有相交
        [TestMethod]
        public void Test_fieldNameList_sub_08()
        {
            string range1 = "001-002";
            string range2 = "003-004";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("001-002", results.ToString());
        }

        // 没有相交
        [TestMethod]
        public void Test_fieldNameList_sub_09()
        {
            string range1 = "003-004";
            string range2 = "001-002";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("003-004", results.ToString());
        }

        // 任意减去 all
        [TestMethod]
        public void Test_fieldNameList_sub_10()
        {
            string range1 = "003-004";
            string range2 = "***";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        // ### 减去 ###
        [TestMethod]
        public void Test_fieldNameList_sub_11()
        {
            string range1 = "###";
            string range2 = "###";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        // ### 减去其他
        [TestMethod]
        public void Test_fieldNameList_sub_12()
        {
            string range1 = "###";
            string range2 = "001-200";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("###", results.ToString());
        }

        // 其他减去 ###
        [TestMethod]
        public void Test_fieldNameList_sub_13()
        {
            string range1 = "001-200";
            string range2 = "###";

            var results = _testItemSub(range1, range2);

            Assert.AreEqual("001-200", results.ToString());
        }

        #endregion

        #region list sub item

        [TestMethod]
        public void Test_fieldNameList_listsub_01()
        {
            string range1 = "001-200,300-400";
            string range2 = "200-300";

            var results = _testListSub(range1, range2);

            Assert.AreEqual("001-199,301-400", results.ToString());
        }

        [TestMethod]
        public void Test_fieldNameList_listsub_02()
        {
            string range1 = "001-200,300-400";
            string range2 = "200-999";

            var results = _testListSub(range1, range2);

            Assert.AreEqual("001-199", results.ToString());
        }

        [TestMethod]
        public void Test_fieldNameList_listsub_03()
        {
            string range1 = "001-200,300-400";
            string range2 = "000-999";

            var results = _testListSub(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        [TestMethod]
        public void Test_fieldNameList_listsub_04()
        {
            string range1 = "001-200,300-400";
            string range2 = "000-301";

            var results = _testListSub(range1, range2);

            Assert.AreEqual("302-400", results.ToString());
        }

        [TestMethod]
        public void Test_fieldNameList_listsub_05()
        {
            string range1 = "001-200,300-400";
            string range2 = "***";

            var results = _testListSub(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        [TestMethod]
        public void Test_fieldNameList_listsub_06()
        {
            string range1 = "***";
            string range2 = "001-200";

            var results = _testListSub(range1, range2);

            Assert.AreEqual("###,000,201-999", results.ToString());
        }

        #endregion

        #region list sub list

        [TestMethod]
        public void Test_fieldNameList_listsublist_01()
        {
            string range1 = "***";
            string range2 = "001-200,300-400";

            var results = _testListSubList(range1, range2);

            Assert.AreEqual("###,000,201-299,401-999", results.ToString());
        }

        [TestMethod]
        public void Test_fieldNameList_listsublist_02()
        {
            string range1 = "001-009,200-300";
            string range2 = "001-200,300-400";

            var results = _testListSubList(range1, range2);

            Assert.AreEqual("201-299", results.ToString());
        }

        [TestMethod]
        public void Test_fieldNameList_listsublist_03()
        {
            string range1 = "001-009,200-300";
            string range2 = "***";

            var results = _testListSubList(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        [TestMethod]
        public void Test_fieldNameList_listsublist_04()
        {
            string range1 = "###,001-009,200-300";
            string range2 = "***";

            var results = _testListSubList(range1, range2);

            Assert.AreEqual("", results.ToString());
        }

        #endregion

        FieldNameList _testItemSub(string range1, string range2)
        {
            var item1 = FieldNameItem.Build(range1, out string strError);
            if (item1 == null)
                throw new Exception(strError);
            Assert.IsTrue(item1 != null);

            var item2 = FieldNameItem.Build(range2, out strError);
            if (item2 == null)
                throw new Exception(strError);
            Assert.IsTrue(item2 != null);

            return FieldNameItem.Sub(item1, item2);
        }

        FieldNameList _testListSub(string list_range, string item_range)
        {
            var list = new FieldNameList();
            var ret = list.Build(list_range, out string strError);
            if (ret == -1)
                throw new Exception(strError);

            var item2 = FieldNameItem.Build(item_range, out strError);
            if (item2 == null)
                throw new Exception(strError);

            Assert.IsTrue(item2 != null);

            return FieldNameItem.Sub(list, item2);
        }

        FieldNameList _testListSubList(string range1, string range2)
        {
            var list1 = new FieldNameList();
            var ret = list1.Build(range1, out string strError);
            if (ret == -1)
                throw new Exception(strError);

            var list2 = new FieldNameList();
            ret = list2.Build(range2, out strError);
            if (ret == -1)
                throw new Exception(strError);

            return FieldNameItem.Sub(list1, list2);
        }
    }
}
