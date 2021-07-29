using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using DigitalPlatform.LibraryServer;

namespace TestDp2Library
{
    // 测试 合并两个权限字符串的函数 MergeRights()
    public class TestMergeRights
    {
        [Theory]
        [InlineData("borrow", "", "borrow")]
        [InlineData("borrow,return", "", "borrow,return")]
        [InlineData("borrow", "return", "borrow,return")]
        [InlineData("", "return", "return")]
        [InlineData("", "borrow,return", "borrow,return")]
        [InlineData("borrow", "getreaderinfo:1", "borrow,getreaderinfo:1")]
        [InlineData("borrow,getreaderinfo", "getreaderinfo:1", "borrow,getreaderinfo:1")]
        [InlineData("borrow,getreaderinfo:1", "getreaderinfo:1", "borrow,getreaderinfo:1")]
        [InlineData("borrow,getreaderinfo:1", "getreaderinfo", "borrow,getreaderinfo")]
        [InlineData("borrow,getreaderinfo", "getreaderinfo", "borrow,getreaderinfo")]
        public void TestMergeRights_01(string rights1, string rights2, string result)
        {
            var temp = LibraryApplication.MergeRights(rights1, rights2);
            Assert.Equal(result, temp);
        }

        // 两个源均为 null 或者 ""
        [Theory]
        [InlineData("", "", "")]
        [InlineData(null, "", "")]
        [InlineData("", null, "")]
        [InlineData(null, null, "")]
        public void TestMergeRights_02(string rights1, string rights2, string result)
        {
            var temp = LibraryApplication.MergeRights(rights1, rights2);
            Assert.Equal(result, temp);
        }

        // 两个源中包含一个 null 或者 ""
        [Theory]
        [InlineData("", "getreaderinfo", "getreaderinfo")]
        [InlineData(null, "getreaderinfo", "getreaderinfo")]
        [InlineData("getreaderinfo", null, "getreaderinfo")]
        [InlineData("getreaderinfo", "", "getreaderinfo")]
        public void TestMergeRights_03(string rights1, string rights2, string result)
        {
            var temp = LibraryApplication.MergeRights(rights1, rights2);
            Assert.Equal(result, temp);
        }
    }
}
