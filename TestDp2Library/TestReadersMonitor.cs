using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using DigitalPlatform.LibraryServer;

namespace TestDp2Library
{
    /// <summary>
    /// 测试 ReaderMonitor 相关函数
    /// </summary>
    public class TestReadersMonitor
    {
        /*
Tue, 26 Oct 2021 16:13:50 +0800
5day
-5day,-2day,-1day
        * 
         * 
         * */
        [Theory]
        // *** 整数形态
        // 已经过了 10-31, 属于超期的情况了
        [InlineData("2021-11-1", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "-5day,-2day,-1day", 0, new int[] { 0, 1, 2 })]

        [InlineData("2021-10-31", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "-5day,-2day,-1day", 0, new int[] { 0, 1, 2 })]

        [InlineData("2021-10-30", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "-5day,-2day,-1day", 0, new int[] { 0, 1, 2 })]
        [InlineData("2021-10-29", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "-5day,-2day,-1day", 0, new int[] { 0, 1 })]
        
        [InlineData("2021-10-28", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "-5day,-2day,-1day", 0, new int[] { 0 })]
        [InlineData("2021-10-27", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "-5day,-2day,-1day", 0, new int[] { 0 })]
        
        [InlineData("2021-10-26", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "-5day,-2day,-1day", 0, new int[] { 0 })]

        // 不通知
        [InlineData("2021-10-25", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "-5day,-2day,-1day", 0, new int[] { })]

        // *** 百分号形态
        [InlineData("2021-11-1", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "50%", 0, new int[] { 0 })]
        [InlineData("2021-10-31", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "50%", 0, new int[] { 0 })]
        [InlineData("2021-10-30", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "50%", 0, new int[] { 0 })]

        [InlineData("2021-10-29", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "50%", 0, new int[] { 0 })]
        [InlineData("2021-10-28", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "50%", 0, new int[] { })]
        [InlineData("2021-10-27", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "50%", 0, new int[] { })]
        [InlineData("2021-10-26", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "50%", 0, new int[] { })]
        [InlineData("2021-10-25", "Tue, 26 Oct 2021 16:13:50 +0800",
            "5day", "50%", 0, new int[] { })]

        public void TestReadersMonitor_CheckNotifyPoint(
            string today,
            string borrowDate,
            string period,
            string notifyDef,
            int expected_ret,
            int[] expect_indices)
        {
            if (expect_indices != null)
            {
                for (int i = 0; i < expect_indices.Length; i++)
                {
                    // 索引值必须小于数组尺寸
                    Assert.True(expect_indices[i] >= 0 && expect_indices[i] < expect_indices.Length);
                }
            }

            DateTime today_time = DateTime.Parse(today);

            // 检查每个通知点，返回当前时间已经达到或者超过了通知点的那些检查点的下标
            // return:
            //      -1  数据格式错误
            //      0   成功
            int nRet = LibraryApplication.CheckNotifyPoint(
                null,
                null,   // Calendar calendar,
                today_time.ToUniversalTime(),
                borrowDate,
                period,
                notifyDef,
                out List<int> indices,
                out string strError);
            Assert.Equal(expected_ret, nRet);
            if (expect_indices != null)
            {
                Assert.Equal(expect_indices.Length, indices.Count);
                for (int i = 0; i < expect_indices.Length; i++)
                {
                    Assert.Equal(expect_indices[i], indices[i]);
                }
            }
        }
    }
}
