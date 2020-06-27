using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RfidDrivers.First;

namespace UnitTestRFID
{
    [TestClass]
    public class TestDriver
    {
        [TestMethod]
        public void Test_lock_1()
        {
            RfidDriver1 driver = new RfidDriver1();
            driver.TestInitialReader();

            var result = driver.TestCall("timeout:5000,sleep:3000");

            Assert.AreEqual(0, result.Value);
        }

        [TestMethod]
        public void Test_lock_2()
        {
            RfidDriver1 driver = new RfidDriver1();
            driver.TestInitialReader();

            List<string> exception_tasks = new List<string>();
            // 第一个线程
            var task1 = Task.Run(() =>
            {
                try
                {
                    var result = driver.TestCall("timeout:5000,sleep:6000");
                    Assert.AreEqual(0, result.Value);
                }
                catch
                {
                    exception_tasks.Add("task1");
                }
            });

            // 第二个线程
            var task2 = Task.Run(() =>
            {
                try
                {
                    var result = driver.TestCall("timeout:5000,sleep:6000");
                    Assert.AreEqual(0, result.Value);
                }
                catch
                {
                    exception_tasks.Add("task2");
                }
            });


            Task.WaitAll(new List<Task> { task1, task2 }.ToArray());

            Assert.AreEqual(1, exception_tasks.Count);
            // Assert.AreEqual("task2", exception_tasks[0]);


            // 第二步：验证此时锁系统是否还可用？
            {
                var result = driver.TestCall("timeout:5000,sleep:3000");
                Assert.AreEqual(0, result.Value);
            }

        }
    }
}
