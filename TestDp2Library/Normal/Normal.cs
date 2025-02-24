using DigitalPlatform.LibraryServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDp2Library.Normal
{
    [TestClass]
    public class TestDp2libraryNormal
    {
        [TestMethod]
        public void Test_libraryCodeList_01()
        {
            {
                string list1 = "";
                string list2 = "";
                string correct = "";
                string result = LibraryApplication.MergeLibraryCodeLists(list1, list2);
                Assert.AreEqual(correct, result);
            }

            {
                string list1 = "";
                string list2 = "海淀分馆";
                string correct = ",海淀分馆";
                string result = LibraryApplication.MergeLibraryCodeLists(list1, list2);
                Assert.AreEqual(correct, result);
            }

            {
                string list1 = "";
                string list2 = ",海淀分馆";
                string correct = ",海淀分馆";
                string result = LibraryApplication.MergeLibraryCodeLists(list1, list2);
                Assert.AreEqual(correct, result);
            }

            {
                string list1 = "";
                string list2 = "海淀分馆,";
                string correct = ",海淀分馆";
                string result = LibraryApplication.MergeLibraryCodeLists(list1, list2);
                Assert.AreEqual(correct, result);
            }

            {
                string list1 = "海淀分馆";
                string list2 = "海淀分馆,西城分馆";
                string correct = "海淀分馆,西城分馆";
                string result = LibraryApplication.MergeLibraryCodeLists(list1, list2);
                Assert.AreEqual(correct, result);
            }

            {
                string list1 = "西城分馆";
                string list2 = "海淀分馆,西城分馆";
                string correct = "西城分馆,海淀分馆";
                string result = LibraryApplication.MergeLibraryCodeLists(list1, list2);
                Assert.AreEqual(correct, result);
            }

            {
                string list1 = null;
                string list2 = "海淀分馆,西城分馆";
                string correct = "海淀分馆,西城分馆";
                string result = LibraryApplication.MergeLibraryCodeLists(list1, list2);
                Assert.AreEqual(correct, result);
            }

            {
                string list1 = "海淀分馆,西城分馆";
                string list2 = null;
                string correct = "海淀分馆,西城分馆";
                string result = LibraryApplication.MergeLibraryCodeLists(list1, list2);
                Assert.AreEqual(correct, result);
            }

            {
                string list1 = null;
                string list2 = null;
                string correct = null;
                string result = LibraryApplication.MergeLibraryCodeLists(list1, list2);
                Assert.AreEqual(correct, result);
            }
        }
    }

}
