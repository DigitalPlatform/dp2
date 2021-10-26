using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using DigitalPlatform.Core;

namespace TestDp2Library
{
    // 注: dp-library repo 中也有如下单元测试代码
    public class TestCompactData
    {
        [Theory]
        [InlineData(null, "00:00:00:(Args is null)")]
        [InlineData(new object[] { }, "00:00:00:")]
        [InlineData(new object[] { null }, "00:00:00:(null)")]
        [InlineData(new object[] { null, null }, "00:00:00:(null),(null)")]
        [InlineData(new object[] { "test" }, "00:00:00:test")]
        [InlineData(new object[] { "test1", "test2" }, "00:00:00:test1,test2")]
        [InlineData(new object[] { "test1", "test2", null }, "00:00:00:test1,test2,(null)")]
        public void TestCompactData_getString(object[] args, string expect_result)
        {
            CompactData data = new CompactData();
            var start = new DateTime(2021, 8, 20);
            data.Ticks = 0;
            data.Args = args;
            var result = data.GetString(start);
            Assert.Equal(expect_result, result);
        }

    }
}
