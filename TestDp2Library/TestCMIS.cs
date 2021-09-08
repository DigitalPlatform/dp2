using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using DigitalPlatform.Text;

namespace TestDp2Library
{
    public class TestCMIS
    {
        // 2021/9/8
        /*
310115200907151923 18位数字
12010420090414001X 17位数字+字母
G320324200811266522 字母+18位数字
G12011120080909010X 字母+17位数字+字母
L4211232008080100B5 字母+16位数字+字母+1位数字
3415211202410271 16位数字         * 
         * */
        [Theory]
        [InlineData("310115200907151923", true)]
        [InlineData("12010420090414001X", true)]
        [InlineData("G320324200811266522", true)]
        [InlineData("G12011120080909010X", true)]
        [InlineData("L4211232008080100B5", true)]
        [InlineData("3415211202410271", true)]
        public void test_isValidCMIS_1(string input, bool correct)
        {
            var ret = StringUtil.IsValidCMIS(input);
            Assert.Equal(correct, ret);
        }
    }
}
