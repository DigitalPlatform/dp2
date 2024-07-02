using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalPlatform.LibraryServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestDp2Library
{
    [TestClass]
    public class TestCalendar
    {
        // 2024/6/27
        [TestMethod]
        public void Test_calendar_01()
        {
            var calendar = new Calendar("test", "20240629-20240630");

            var timeStart = new DateTime(2024, 6, 27);

            // 获得预约保留末端时间
            // 中间要排除所有非工作日
            // return:
            //      -1  出错
            //      0   成功
            int ret = LibraryApplication.GetOverTime(
                calendar,
                timeStart,
                2,
                "day",
                out DateTime timeEnd,
                out string strError);
            Assert.AreEqual(0, ret);

            Assert.AreEqual(new DateTime(2024, 7, 1, 4, 0, 0), timeEnd);
        }

        // 2024/6/27
        [TestMethod]
        public void Test_calendar_02()
        {
            // var calendar = new Calendar("test", "20240629-20240630");

            var timeStart = new DateTime(2024, 6, 27);

            // 获得预约保留末端时间
            // 中间要排除所有非工作日
            // return:
            //      -1  出错
            //      0   成功
            int ret = LibraryApplication.GetOverTime(
                null,   // calendar,
                timeStart,
                2,
                "day",
                out DateTime timeEnd,
                out string strError);
            Assert.AreEqual(0, ret);

            Assert.AreEqual(new DateTime(2024, 6, 29, 4, 0, 0), timeEnd);
        }


    }
}
