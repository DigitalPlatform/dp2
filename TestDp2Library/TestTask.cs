using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Threading;

namespace TestDp2Library
{
    [TestClass]
    public class TestTask
    {
        static AsyncSemaphore _semaphoreResultset = new AsyncSemaphore(1);

        [TestMethod]
        public void test_task_01()
        {
            int COUNT = 100;
            CancellationTokenSource cts = new CancellationTokenSource();

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < COUNT; i++)
            {
                var task =
                    Task.Factory.StartNew(async (number) =>
                    {
                        try
                        {
                            using (var releaser = await _semaphoreResultset.EnterAsync(cts.Token))
                            {
                                Process((int)number);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"process exeption: {ex.Message}");
                        }
                    },
                    i,
                    cts.Token,
                    TaskCreationOptions.LongRunning,    // 独立线程运行
                    TaskScheduler.Default).Unwrap();
                // Thread.Sleep(10);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("All tasks completed.");
        }


        void Process(int i)
        {
            Console.WriteLine($"Task {i} started.");
            Thread.Sleep(1000);
            Console.WriteLine($" Task {i} end.");

        }
    }
}
