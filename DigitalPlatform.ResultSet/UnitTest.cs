using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DuckDbResultSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DigitalPlatform.ResultSet
{
    [TestClass]
    public class UnitTest
    {
        // 测试两个结果集进行或运算
        [TestMethod]
        public void test_or_id_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DpResultSet())
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset.Attach(filename1);
                // 奇数
                for (int i = 0; i < COUNT / 2; i++)
                {
                    var record = new DpRecord(I((i * 2) + 1));
                    resultset.Add(record);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除

                sw.Stop();
                Console.WriteLine($"填入第一个结果集 耗时 {sw.Elapsed}");
            }

            using (var resultset = new DpResultSet())
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset.Attach(filename2);

                // 偶数
                for (int i = 0; i < COUNT / 2; i++)
                {
                    var record = new DpRecord(I((i * 2) + 2));
                    resultset.Add(record);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除

                sw.Stop();
                Console.WriteLine($"填入第二个结果集 耗时 {sw.Elapsed}");
            }

            using (var resultset1 = new DpResultSet())
            using (var resultset2 = new DpResultSet())
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.Attach(filename1);
                resultset2.Attach(filename2);

                sw.Stop();
                Console.WriteLine($"Attach 耗时 {sw.Elapsed}");
                sw.Restart();

                resultset1.EnsureCreateIndex();
                resultset2.EnsureCreateIndex();

                sw.Stop();
                Console.WriteLine($"CreateIndex 耗时 {sw.Elapsed}");
                sw.Restart();

                resultset1.Sort();
                resultset2.Sort();

                sw.Stop();
                Console.WriteLine($"Sort 耗时 {sw.Elapsed}");
                sw.Restart();

                DpResultSet middle = new DpResultSet(true, true);
                StringBuilder debugInfo = null;
                var ret = DpResultSetManager.Merge(LogicOper.OR,
                    resultset1,
                    resultset2,
                    "",
                    null,
                    middle,
                    null,
                    null,
                    null,
                    ref debugInfo,
                    out string strError);
                if (ret == -1)
                    throw new Exception(strError);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = middle.Count;
                Assert.AreEqual(COUNT, count);


                // resultset1.Sort();

                int i = 0;
                foreach (DpRecord record in middle)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }

        [TestMethod]
        public void test_or_id_speedup_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DpResultSet())
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset.Attach(filename1);
                // 奇数
                for (int i = 0; i < COUNT / 2; i++)
                {
                    var record = new DpRecord(I((i * 2) + 1));
                    resultset.Add(record);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除

                sw.Stop();
                Console.WriteLine($"填入第一个结果集 耗时 {sw.Elapsed}");
            }

            using (var resultset = new DpResultSet())
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset.Attach(filename2);

                // 偶数
                for (int i = 0; i < COUNT / 2; i++)
                {
                    var record = new DpRecord(I((i * 2) + 2));
                    resultset.Add(record);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除

                sw.Stop();
                Console.WriteLine($"填入第二个结果集 耗时 {sw.Elapsed}");
            }

            var duck_filename1 = GetTempFileName("duck001");
            var duck_filename2 = GetTempFileName("duck002");


            using (var resultset1 = new DpResultSet())
            using (var resultset2 = new DpResultSet())
            using (var duck1 = new DuckResultSet(ResultSetType.Id, duck_filename1))
            using (var duck2 = new DuckResultSet(ResultSetType.Id, duck_filename2))
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.Attach(filename1);
                resultset2.Attach(filename2);

                Assert.AreEqual(COUNT / 2, resultset1.Count);
                Assert.AreEqual(COUNT / 2, resultset2.Count);

                sw.Stop();
                Console.WriteLine($"Attach 耗时 {sw.Elapsed}");
                sw.Restart();

                // 提高遍历速度
                resultset1.EnsureCreateIndex();
                using (var context = duck1.GetAppenderContext())
                {
                    int j = 0;
                    foreach (DpRecord record in resultset1)
                    {
                        context.AppendIdRow(record.ID, j + 1);
                        j++;
                    }
                }

                Assert.AreEqual(COUNT / 2, duck1.Count);

                // 提高遍历速度
                resultset2.EnsureCreateIndex();
                using (var context = duck2.GetAppenderContext())
                {
                    int j = 0;
                    foreach (DpRecord record in resultset2)
                    {
                        context.AppendIdRow(record.ID, j + 1);
                        j++;
                    }
                }

                Assert.AreEqual(COUNT / 2, duck2.Count);

                sw.Stop();
                Console.WriteLine($"Import To DuckResultSet 耗时 {sw.Elapsed}");
                sw.Restart();

                duck1.OR(duck2);

                Assert.AreEqual(COUNT, duck1.Count);

                sw.Stop();
                Console.WriteLine($"Duck OR 耗时 {sw.Elapsed}");
                sw.Restart();

                DpResultSet middle = new DpResultSet(true, true);
                // 提高遍历速度
                middle.EnsureCreateIndex();
                foreach (var record in duck1)
                {
                    middle.Add(new DpRecord(record.ID));
                }

                sw.Stop();
                Console.WriteLine($"Export 进入 middle 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = middle.Count;
                Assert.AreEqual(COUNT, count);


                // resultset1.Sort();

                int i = 0;
                foreach (DpRecord record in middle)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }


        #region 辅助函数

        string GetTempDir()
        {
            var temp_dir = Path.Combine(Environment.CurrentDirectory, "temp");
            if (Directory.Exists(temp_dir) == false)
                Directory.CreateDirectory(temp_dir);
            return temp_dir;
        }

        string GetTempFileName(string number = "001",
            bool deleteFile = true)
        {
            var temp_dir = GetTempDir();
            string filename = "";
            if (string.IsNullOrEmpty(number) == true)
                filename = Path.Combine(temp_dir, Guid.NewGuid().ToString());
            else
                filename = Path.Combine(temp_dir, number);

            if (deleteFile && File.Exists(filename))
                File.Delete(filename);

            return filename;
        }

        static string I(long number)
        {
            return number.ToString().PadLeft(10, '0');
        }

        #endregion
    }
}
