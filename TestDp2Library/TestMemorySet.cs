using DigitalPlatform.LibraryServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDp2Library
{
    [TestClass]
    public class TestMemorySet
    {
        [TestMethod]
        public void Test_memorySet_sort_01()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "col1", "col2" }
            };
            var indices = new int[] { 1 };
            int ret = MemorySet.CompareItems(item1,
    item1,
    indices);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_02()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "col1", "col2" }
            };
            var indices = new int[] { 2 };
            int ret = MemorySet.CompareItems(item1,
    item1,
    indices);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_03()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "col1", "col2" }
            };
            var indices = new int[] { 1, 2 };
            int ret = MemorySet.CompareItems(item1,
    item1,
    indices);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_04()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "col1", "col2" }
            };
            var indices = new int[] { 1, 2, 3 };    // 3 越过 Cols 边界
            int ret = MemorySet.CompareItems(item1,
    item1,
    indices);
            Assert.AreEqual(0, ret);
        }

        //
        [TestMethod]
        public void Test_memorySet_sort_05()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "col1", "col2" }
            };
            var indices = new int[] { -1 };
            int ret = MemorySet.CompareItems(item1,
    item1,
    indices);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_06()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "col1", "col2" }
            };
            var indices = new int[] { -2 };
            int ret = MemorySet.CompareItems(item1,
    item1,
    indices);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_07()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "col1", "col2" }
            };
            var indices = new int[] { -1, -2 };
            int ret = MemorySet.CompareItems(item1,
    item1,
    indices);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_08()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "col1", "col2" }
            };
            var indices = new int[] { -1, -2, -3 };    // 3 越过 Cols 边界
            int ret = MemorySet.CompareItems(item1,
    item1,
    indices);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_10()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "col1", "col2" }
            };
            var item2 = new SortItem
            {
                Path = "中文图书/2",
                Cols = new string[] { "col1", "col2" }
            };
            var indices = new int[] { 1, 2, 3 };    // 3 越过 Cols 边界
            int ret = MemorySet.CompareItems(item1,
    item2,
    indices);
            Assert.AreEqual(-1, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_11()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "col1", "col2" }
            };
            var item2 = new SortItem
            {
                Path = "中文图书/2",
                Cols = new string[] { "col1", "col2" }
            };
            var indices = new int[] { -1, -2, -3 };    // 3 越过 Cols 边界
            int ret = MemorySet.CompareItems(item1,
    item2,
    indices);
            Assert.AreEqual(1, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_12()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "text1", "text2" }
            };
            var item2 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "text2", "text3" }
            };
            var indices = new int[] { 1 };
            int ret = MemorySet.CompareItems(item1,
    item2,
    indices);
            Assert.AreEqual(0, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_13()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "text1", "text2" }
            };
            var item2 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "text2", "text2" }
            };
            var indices = new int[] { 1, 2 };
            int ret = MemorySet.CompareItems(item1,
    item2,
    indices);
            Assert.AreEqual(-1, ret);
        }

        [TestMethod]
        public void Test_memorySet_sort_14()
        {
            var item1 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "text1", "text2" }
            };
            var item2 = new SortItem
            {
                Path = "中文图书/1",
                Cols = new string[] { "text1", "text3" }
            };
            var indices = new int[] { 1, 2, 3 };
            int ret = MemorySet.CompareItems(item1,
    item2,
    indices);
            Assert.AreEqual(-1, ret);
        }

    }
}
