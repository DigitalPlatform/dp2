using DigitalPlatform.LibraryServer;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestDp2Library
{
    public class TestRemoveConflictName
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("name", "name")]
        [InlineData("?name,name", "name")]
        [InlineData("?name,,name", ",name")]
        [InlineData("?name,1,name", "1,name")]
        [InlineData("1,?name,2,name", "1,2,name")]
        [InlineData("1,?name,2,name,3", "1,2,name,3")]
        [InlineData("?name", "?name")]
        [InlineData("?name,1", "?name,1")]
        [InlineData("1,?name,2", "1,?name,2")]
        public void TestRemoveConflictName_01(string input, string result)
        {
            var input_list = StringUtil.SplitList(input);
            var result_list = LibraryApplication.RemoveConflictName(input_list);
            Assert.Equal(result, StringUtil.MakePathList(result_list));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("name", "name")]
        [InlineData("name,?name", "?name")]
        [InlineData("name,,?name", ",?name")]
        [InlineData("name,1,?name", "1,?name")]
        [InlineData("1,name,2,?name", "1,2,?name")]
        [InlineData("1,name,2,?name,3", "1,2,?name,3")]
        [InlineData("name,1", "name,1")]
        [InlineData("1,name,2", "1,name,2")]
        public void TestRemoveConflictName_02(string input, string result)
        {
            var input_list = StringUtil.SplitList(input);
            var result_list = LibraryApplication.RemoveConflictName(input_list);
            Assert.Equal(result, StringUtil.MakePathList(result_list));
        }
    }
}
