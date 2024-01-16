using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform;

namespace UnitTestGeneral
{
    // 测试 NewStop 相关类
    [TestClass]
    public class UnitTestNewStop
    {
        // 最简单的情况：一个线程
        [TestMethod]
        public void Test_newStop_01()
        {
            LoopingHost host = new LoopingHost();
            host.StopManager = new StopManager { OwnerControl = new System.Windows.Forms.TextBox() };
            host.StopManager.CreateGroup("test");
            host.GroupName = "test";

            var looping = host.BeginLoop(
    (s, e) => { },
    "text",
    "");
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(10);
                }
            }
            finally
            {
                looping.Dispose();
            }

        }

        // 两个线程
        [TestMethod]
        public void Test_newStop_02()
        {
            LoopingHost host = new LoopingHost();
            host.StopManager = new StopManager { OwnerControl = new System.Windows.Forms.TextBox() };
            host.StopManager.CreateGroup("test");
            host.GroupName = "test";

            var loopings = new List<Looping>();

            var task1 = Task.Run(() =>
            {
                var looping = host.BeginLoop(
(s, e) => { },
"text1",
"");
                loopings.Add(looping);
                try
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        Thread.Sleep(10);
                    }
                }
                finally
                {
                    looping.Dispose();
                }

            });

            var task2 = Task.Run(() =>
            {
                var looping = host.BeginLoop(
(s, e) => { },
"text2",
"");
                loopings.Add(looping);
                try
                {
                    for (int i = 0; i < 500; i++)
                    {
                        Thread.Sleep(10);
                    }
                }
                finally
                {
                    looping.Dispose();
                }

            });

            bool finish = false;
            // 反复 Activate()
            var task3 = Task.Run(() =>
            {
                while(loopings.Count < 2)
                {
                    Thread.Sleep(0);
                }
                foreach (var looping in loopings)
                {
                    if (finish == true)
                        break;
                    host.StopManager.Activate(looping.Progress);
                    Thread.Sleep(0);
                }
            });

            Task.WaitAll(task1, task2);
            finish = true;
        }

    }
}
