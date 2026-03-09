using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DuckDbResultSet
{
    [TestClass]
    public class UnitTest
    {
        // 追加记录，Id 类型 
        [TestMethod]
        public void test_append_id_01()
        {
            var filename = GetTempFileName();

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow("1", 1);
                    context.AppendIdRow("2", 2);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(2, count);

                var record = resultset.GetRecord(0);
                Assert.AreEqual("1", record.ID);
                Assert.AreEqual(null, record.Key);

                record = resultset.GetRecord(1);
                Assert.AreEqual("2", record.ID);
                Assert.AreEqual(null, record.Key);

                resultset.Detach();
            }

            /*
            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                resultset.Desc = true;
                resultset.Sort();

                var record = resultset.GetRecord(0);
                Assert.AreEqual("2", record.ID);
                Assert.AreEqual(null, record.Key);

                record = resultset.GetRecord(1);
                Assert.AreEqual("1", record.ID);
                Assert.AreEqual(null, record.Key);
            }
            */
        }

        // 分为两次追加
        [TestMethod]
        public void test_append_id_02()
        {
            var filename = GetTempFileName();

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow("1", 1);
                    context.AppendIdRow("2", 2);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow("3", 3);
                    context.AppendIdRow("4", 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(4, count);

                int i = 0;
                foreach (var record in resultset)
                {
                    Console.WriteLine($"{record.ID}");

                    Assert.AreEqual($"{i + 1}", record.ID);
                    Assert.AreEqual(null, record.Key);
                    i++;
                }
            }
        }


        // 分为两次追加，两次之间有重复
        [TestMethod]
        public void test_dedup_id_01()
        {
            var filename = GetTempFileName();

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow("1", 1);
                    context.AppendIdRow("2", 2);
                    context.AppendIdRow("3", 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow("3", 4);
                    context.AppendIdRow("4", 5);
                    context.AppendIdRow("5", 6);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(6, count);

                // 去重
                resultset.DeDup();
                Assert.AreEqual(5, resultset.Count);

                int i = 0;
                foreach (var record in resultset.GetRange(0, resultset.Count, output_row_id: false))
                {
                    Console.WriteLine($"{record.ID} {record.RowID}");

                    Assert.AreEqual($"{i + 1}", record.ID);
                    Assert.AreEqual(null, record.Key);
                    i++;
                }
            }
        }

        // 空集合
        [TestMethod]
        public void test_dedup_id_02()
        {
            var filename = GetTempFileName();

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                /*
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow("1", 1);
                    context.AppendIdRow("2", 2);
                    context.AppendIdRow("3", 3);
                }
                */
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(0, count);

                // 去重
                resultset.DeDup();
                Assert.AreEqual(0, resultset.Count);

                foreach (var record in resultset)
                {
                    Debug.Fail("不应该有任何记录");
                }
            }
        }


        [TestMethod]
        public void test_dedup_keyid_01()
        {
            var filename = GetTempFileName();

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow("1", "key1", 1);
                    context.AppendKeyIdRow("2", "key2", 2);
                    context.AppendKeyIdRow("3", "key3_1", 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow("3", "key3_2", 4);
                    context.AppendKeyIdRow("4", "key4", 5);
                    context.AppendKeyIdRow("5", "key5", 6);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(6, count);

                // 去重
                resultset.DeDup();
                Assert.AreEqual(5, resultset.Count);

                int i = 0;
                foreach (var record in resultset)
                {
                    Console.WriteLine($"{record.ID} {record.Key}");

                    Assert.AreEqual($"{i + 1}", record.ID);
                    if (record.ID == "3")
                        Assert.AreEqual("key3_1, key3_2", record.Key);
                    else
                        Assert.AreEqual($"key{i + 1}", record.Key);
                    i++;
                }
            }
        }

        // 空集合
        [TestMethod]
        public void test_dedup_keyid_02()
        {
            var filename = GetTempFileName();

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(0, count);

                // 去重
                resultset.DeDup();
                Assert.AreEqual(0, resultset.Count);

                foreach (var record in resultset)
                {
                    Assert.Fail("空集合不该有任何记录");
                }
            }
        }


        [TestMethod]
        public void test_dedup_keycount_01()
        {
            var filename = GetTempFileName();
            const int v = 2;

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow("key1", v, 1);
                    context.AppendKeyCountRow("key2", v, 2);
                    context.AppendKeyCountRow("key3", v, 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow("key3", v, 4);
                    context.AppendKeyCountRow("key4", v, 5);
                    context.AppendKeyCountRow("key5", v, 6);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(6, count);

                // 去重
                resultset.DeDup();
                Assert.AreEqual(5, resultset.Count);

                int i = 0;
                foreach (var record in resultset)
                {
                    Console.WriteLine($"{record.Key} {record.Count}");

                    Assert.AreEqual($"key{i + 1}", record.Key);
                    if (record.Key == "key3")
                        Assert.AreEqual(v * 2, record.Count);
                    else
                        Assert.AreEqual(v, record.Count);
                    i++;
                }
            }
        }

        // 空集合
        [TestMethod]
        public void test_dedup_keycount_02()
        {
            var filename = GetTempFileName();

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename))
            {
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(0, count);

                // 去重
                resultset.DeDup();
                Assert.AreEqual(0, resultset.Count);

                foreach (var record in resultset)
                {
                    Debug.Fail("不应该有任何记录");
                }
            }
        }


        // 追加记录，KeyId 类型
        [TestMethod]
        public void test_append_keyid_02()
        {
            var filename = GetTempFileName();

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow("1", "key1", 1);
                    context.AppendKeyIdRow("2", "key2", 2);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(2, count);

                var record = resultset.GetRecord(0);
                Assert.AreEqual("1", record.ID);
                Assert.AreEqual("key1", record.Key);

                record = resultset.GetRecord(1);
                Assert.AreEqual("2", record.ID);
                Assert.AreEqual("key2", record.Key);
            }
        }

        // 追加记录，KeyId 类型
        [TestMethod]
        public void test_append_keyid_03()
        {
            var filename = GetTempFileName();

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow("key1", 1000, 1);
                    context.AppendKeyCountRow("key2", 2000, 2);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(2, count);

                var record = resultset.GetRecord(0);
                Assert.AreEqual(1000, record.Count);
                Assert.AreEqual("key1", record.Key);

                record = resultset.GetRecord(1);
                Assert.AreEqual(2000, record.Count);
                Assert.AreEqual("key2", record.Key);
            }
        }


#if REMOVED
        // 使用 SQL Insert 语句追加
        [TestMethod]
        public void test_append_large_1()
        {
            const int COUNT = 1000000;

            var filename = GetTempFileName();

            Stopwatch sw = Stopwatch.StartNew();
            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow("1", "key1", 1);
                    context.AppendKeyIdRow("2", "key2", 2);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            sw.Stop();
            var create_length = sw.Elapsed;

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(COUNT, count);
            }
        }

#endif

        // 使用 Appender 追加
        [TestMethod]
        public void test_append_keyid_large_2()
        {
            const int COUNT = 1000000;

            var filename = GetTempFileName();

            Stopwatch sw = Stopwatch.StartNew();
            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (var i = 0; i < COUNT; i++)
                    {
                        context.AppendKeyIdRow($"@1/{(i + 1).ToString().PadLeft(10, '0')}", "key", i + 1);
                        /*
                        var row = context.Appender.CreateRow();
                        row.AppendValue().AppendValue((string)null).EndRow();
                        */
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            sw.Stop();
            var create_length = sw.Elapsed;
            Console.WriteLine($"创建 {COUNT} 条记录耗费时间 {sw.Elapsed}");

            sw.Restart();
            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(COUNT, count);

                int i = 0;
                foreach (var record in resultset)
                {
                    Assert.AreEqual($"@1/{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }

            sw.Stop();
            Console.WriteLine($"遍历 {COUNT} 条记录耗费时间 {sw.Elapsed}");
        }

        // 分片获取
        [TestMethod]
        public void test_fetch_id_large_1()
        {
            const int COUNT = 1000000;

            var filename = GetTempFileName();

            Stopwatch sw = Stopwatch.StartNew();
            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (var i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"@1/{(i + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            sw.Stop();
            var create_length = sw.Elapsed;
            Console.WriteLine($"创建 {COUNT} 条记录耗费时间 {sw.Elapsed}");

            sw.Restart();
            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(COUNT, count);

                // resultset.SetSorted();  // 这样会迫使 select 利用 where ... row_id，这样速度快。比 sorted == false 情况恐怕要快十倍

                int BATCH_SIZE = 10000;
                int start = 0;
                for (int i = 0; i < COUNT / BATCH_SIZE; i++)
                {
                    var j = start;
                    foreach (var record in resultset.GetRange(start, BATCH_SIZE))
                    {
                        Assert.AreEqual($"@1/{(j + 1).ToString().PadLeft(10, '0')}", record.ID);
                        j++;
                    }

                    start += BATCH_SIZE;
                }
            }

            sw.Stop();
            Console.WriteLine($"分页遍历 {COUNT} 条记录耗费时间 {sw.Elapsed}");
        }


        [TestMethod]
        public void test_sort_id_large_1()
        {
            const int COUNT = 1000000;

            var filename = GetTempFileName();

            Stopwatch sw = Stopwatch.StartNew();
            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (var i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"@1/{(i + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            sw.Stop();
            var create_length = sw.Elapsed;
            Console.WriteLine($"创建 {COUNT} 条记录耗费时间 {sw.Elapsed}");

            sw.Restart();
            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                var count = resultset.Count;
                Assert.AreEqual(COUNT, count);

                resultset.Sort();

                resultset.Detach();
            }

            sw.Stop();
            Console.WriteLine($"排序 {COUNT} 条记录耗费时间 {sw.Elapsed}");

            sw.Restart();
            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                resultset.SetSorted();  // 恢复排序指示状态

                var count = resultset.Count;
                Assert.AreEqual(COUNT, count);

                int BATCH_SIZE = 100;
                int start = 0;
                for (int i = 0; i < COUNT / BATCH_SIZE; i++)
                {
                    var j = start;
                    foreach (var record in resultset.GetRange(start, BATCH_SIZE))
                    {
                        Assert.AreEqual($"@1/{(j + 1).ToString().PadLeft(10, '0')}", record.ID);
                        j++;
                    }

                    start += BATCH_SIZE;
                }
            }

            sw.Stop();
            Console.WriteLine($"遍历 {COUNT} 条记录耗费时间 {sw.Elapsed}");
        }

        #region OR Id

        // 测试两个结果集进行或运算
        [TestMethod]
        public void test_or_id_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 2).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                /*
                {
                    Assert.AreEqual("1", resultset1[0].ID);
                    Assert.AreEqual("3", resultset1[1].ID);
                }

                {
                    Assert.AreEqual("2", resultset2[0].ID);
                    Assert.AreEqual("4", resultset2[1].ID);
                }
                */
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);


                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }

        // 专门指定结果集参数
        [TestMethod]
        public void test_or_id_1_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");
            var filename3 = GetTempFileName("003");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 2).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            using (var resultset3 = new DuckResultSet(ResultSetType.Id, filename3))
            {
                Assert.AreEqual(COUNT / 2, resultset1.Count);
                Assert.AreEqual(COUNT / 2, resultset2.Count);
                Assert.AreEqual(0, resultset3.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2, resultset3);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset3.Count;
                Assert.AreEqual(COUNT, count);

                Assert.AreEqual(COUNT / 2, resultset1.Count);
                Assert.AreEqual(COUNT / 2, resultset2.Count);

                int i = 0;
                foreach (var record in resultset3)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }


        // OR 的两侧结果集为空
        [TestMethod]
        public void test_or_id_2()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{0} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                foreach (var record in resultset1)
                {
                    Assert.Fail("空集合不该有任何记录");
                }
            }
        }

        // OR 的左侧结果集为空
        [TestMethod]
        public void test_or_id_3()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{(i + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }

        // OR 的右侧结果集为空
        [TestMethod]
        public void test_or_id_4()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{(i + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }



        // OR 的两端有部分重叠
        [TestMethod]
        public void test_or_id_5()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"1".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"2".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"4".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"5".PadLeft(10, '0')}", 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(3, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{6} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(5, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }

        // OR 的两端单个结果集内部有部分重叠
        [TestMethod]
        public void test_or_id_6()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"1".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"2".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"2".PadLeft(10, '0')}", 3);
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"4".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"5".PadLeft(10, '0')}", 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(5, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }

        #endregion

        #region OR KeyId

        // 测试两个结果集进行或运算，KeyId 风格
        [TestMethod]
        public void test_or_keyid_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = (i + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual(number, record.ID);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
            }
        }

        // OR 的两侧结果集为空
        [TestMethod]
        public void test_or_keyid_2()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{0} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                foreach (var record in resultset1)
                {
                    Assert.Fail("空结果集不该有任何记录");
                }
            }
        }


        // OR 的左侧结果集为空
        [TestMethod]
        public void test_or_keyid_3()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                    Assert.AreEqual(number, record.ID);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
            }
        }

        // OR 的右侧结果集为空
        [TestMethod]
        public void test_or_keyid_4()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual(number, record.ID);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
            }
        }

        // OR 的两端有部分重叠
        [TestMethod]
        public void test_or_keyid_5()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(1)}", $"key{I(1)}", 1);
                    context.AppendKeyIdRow($"{I(2)}", $"key{I(2)}", 2);
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 1);
                    context.AppendKeyIdRow($"{I(4)}", $"key{I(4)}", 2);
                    context.AppendKeyIdRow($"{I(5)}", $"key{I(5)}", 3);

                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(3, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{6} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(5, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    // key 3 特殊
                    if (i == 2)
                    {
                        Assert.AreEqual($"key{I(3)}, key{I(3)}", record.Key);
                    }
                    else
                    {
                        Assert.AreEqual($"key{I(i + 1)}", record.Key);
                    }
                    i++;
                }


            }
        }

        // OR 的两端单个结果集内部有部分重叠
        [TestMethod]
        public void test_or_keyid_6()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(1)}", $"key{I(1)}", 1);
                    context.AppendKeyIdRow($"{I(2)}", $"key{I(2)}", 2);
                    context.AppendKeyIdRow($"{I(2)}", $"key{I(2)}", 3);
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 1);
                    context.AppendKeyIdRow($"{I(4)}", $"key{I(4)}", 2);
                    context.AppendKeyIdRow($"{I(5)}", $"key{I(5)}", 3);

                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(5, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    // key 2 和 key 3 特殊
                    if (i == 1)
                    {
                        Assert.AreEqual($"key{I(2)}, key{I(2)}", record.Key);
                    }
                    else if (i == 2)
                    {
                        Assert.AreEqual($"key{I(3)}, key{I(3)}", record.Key);
                    }
                    else
                    {
                        Assert.AreEqual($"key{I(i + 1)}", record.Key);
                    }
                    Console.WriteLine(record.Key);
                    i++;
                }


            }
        }

        // 单个集合中 ID 重复的，运算后如何连接
        [TestMethod]
        public void test_or_keyid_7()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(1)}", $"key{I(1)}", 1);
                    context.AppendKeyIdRow($"{I(2)}", $"key_1_{I(2)}", 2);
                    context.AppendKeyIdRow($"{I(2)}", $"key_2_{I(2)}", 3);
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 1);
                    context.AppendKeyIdRow($"{I(4)}", $"key{I(4)}", 2);
                    context.AppendKeyIdRow($"{I(5)}", $"key{I(5)}", 3);

                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(5, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    // key 2 和 key 3 特殊
                    if (i == 1)
                    {
                        Assert.AreEqual($"key_1_{I(2)}, key_2_{I(2)}", record.Key);
                    }
                    else if (i == 2)
                    {
                        Assert.AreEqual($"key{I(3)}, key{I(3)}", record.Key);
                    }
                    else
                    {
                        Assert.AreEqual($"key{I(i + 1)}", record.Key);
                    }
                    Console.WriteLine(record.Key);
                    i++;
                }


            }
        }

        #endregion

        #region OR KeyCount

        // 测试两个结果集进行或运算，KeyCount 风格
        [TestMethod]
        public void test_or_keycount_1()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = (i + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual(v, record.Count);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
            }
        }

        // OR 的两侧结果集为空
        [TestMethod]
        public void test_or_keycount_2()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{0} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                foreach (var record in resultset1)
                {
                    Assert.Fail("空结果集不该有任何记录");
                }
            }
        }

        // OR 的左侧结果集为空
        [TestMethod]
        public void test_or_keycount_3()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                    Assert.AreEqual(v, record.Count);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
            }
        }

        // OR 的右侧结果集为空
        [TestMethod]
        public void test_or_keycount_4()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual(v, record.Count);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
            }
        }

        // OR 的两端有部分重叠
        [TestMethod]
        public void test_or_keycount_5()
        {
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(1)}", v, 1);
                    context.AppendKeyCountRow($"key{I(2)}", v, 2);
                    context.AppendKeyCountRow($"key{I(3)}", v, 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(3)}", v, 1);
                    context.AppendKeyCountRow($"key{I(4)}", v, 2);
                    context.AppendKeyCountRow($"key{I(5)}", v, 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(3, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{6} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(5, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = (i + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual($"key{number}", record.Key);

                    // key 3 特殊
                    if (i == 2)
                    {
                        Assert.AreEqual(v * 2, record.Count);
                    }
                    else
                    {
                        Assert.AreEqual(v, record.Count);
                    }
                    i++;
                }
            }
        }

        // OR 的两端单个结果集内部有部分重叠
        [TestMethod]
        public void test_or_keycount_6()
        {
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(1)}", v, 1);
                    context.AppendKeyCountRow($"key{I(2)}", v, 2);
                    context.AppendKeyCountRow($"key{I(2)}", v, 3);
                    context.AppendKeyCountRow($"key{I(3)}", v, 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(3)}", v, 1);
                    context.AppendKeyCountRow($"key{I(4)}", v, 2);
                    context.AppendKeyCountRow($"key{I(5)}", v, 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.OR(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(5, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = (i + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual($"key{number}", record.Key);
                    // key 2 和 key 3 特殊
                    if (i == 1)
                    {
                        Assert.AreEqual(v * 2, record.Count);
                    }
                    else if (i == 2)
                    {
                        Assert.AreEqual(v * 2, record.Count);
                    }
                    else
                    {
                        Assert.AreEqual(v, record.Count);
                    }
                    Console.WriteLine(record.Key + " " + record.Count);
                    i++;
                }


            }
        }

        #endregion

        // //

        #region AND Id

        // TODO: 增加两侧结果集全部或者分别为空的测试用例

        // 测试两个结果集进行与运算
        // 完全不重合
        [TestMethod]
        public void test_and_id_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 2).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(COUNT / 2, resultset1.Count);
                Assert.AreEqual(COUNT / 2, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }

        // 目标结果集专门指定参数
        [TestMethod]
        public void test_and_id_1_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");
            var filename3 = GetTempFileName("003");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 2).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            using (var resultset3 = new DuckResultSet(ResultSetType.Id, filename3))
            {
                Assert.AreEqual(COUNT / 2, resultset1.Count);
                Assert.AreEqual(COUNT / 2, resultset2.Count);
                Assert.AreEqual(0, resultset3.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2, resultset3);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset3.Count;
                Assert.AreEqual(0, count);

                Assert.AreEqual(COUNT / 2, resultset1.Count);
                Assert.AreEqual(COUNT / 2, resultset2.Count);

                int i = 0;
                foreach (var record in resultset3)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }

        // 完全重合
        [TestMethod]
        public void test_and_id_2()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{I(i + 1)}", record.ID);
                    i++;
                }
            }
        }


        // AND 的左侧结果集为空
        [TestMethod]
        public void test_and_id_3()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{(i + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }

        // AND 的右侧结果集为空
        [TestMethod]
        public void test_and_id_4()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{(i + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }

        // AND 的两端结果集均为空
        [TestMethod]
        public void test_and_id_4_1()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                // 让内容为空
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{0} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
            }
        }


        // AND 的两端有部分重叠
        [TestMethod]
        public void test_and_id_5()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"1".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"2".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"4".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"5".PadLeft(10, '0')}", 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(3, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{6} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(1, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{I(3)}", record.ID);
                    // Assert.AreEqual($"key{I(3)}, key{I(3)}", record.Key);
                    i++;
                }
            }
        }

        // AND 的两端单个结果集内部有部分重叠
        [TestMethod]
        public void test_and_id_6()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"1".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"2".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"2".PadLeft(10, '0')}", 3);
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"4".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"5".PadLeft(10, '0')}", 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(1, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{I(3)}", record.ID);
                    i++;
                }
            }
        }

        #endregion

        #region AND KeyId

        // 测试两个结果集进行与运算，KeyId 风格
        [TestMethod]
        public void test_and_keyid_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                // resultset1.Sort();
            }
        }

        // 完全重合
        [TestMethod]
        public void test_and_keyid_2()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendKeyIdRow($"{I(i + 1)}", $"key1{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendKeyIdRow($"{I(i + 1)}", $"key2{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    Console.WriteLine($"{record.Key} {record.ID}");

                    Assert.AreEqual($"{I(i + 1)}", record.ID);
                    if (record.Key == $"key2{I(i + 1)} & key1{I(i + 1)}"
                        || record.Key == $"key1{I(i + 1)} & key2{I(i + 1)}")
                    {

                    }
                    else
                        Assert.Fail($"{record.Key} 和预期值不同");
                    i++;
                }
            }
        }


        // AND 的左侧结果集为空
        [TestMethod]
        public void test_and_keyid_3()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                /*
                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                    Assert.AreEqual(number, record.ID);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
                */
            }
        }

        // AND 的右侧结果集为空
        [TestMethod]
        public void test_and_keyid_4()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                /*
                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual(number, record.ID);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
                */
            }
        }

        // AND 的两侧结果集为空
        [TestMethod]
        public void test_and_keyid_4_1()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{0} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);
            }
        }


        // AND 的两端有部分重叠
        [TestMethod]
        public void test_and_keyid_5()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(1)}", $"key{I(1)}", 1);
                    context.AppendKeyIdRow($"{I(2)}", $"key{I(2)}", 2);
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 1);
                    context.AppendKeyIdRow($"{I(4)}", $"key{I(4)}", 2);
                    context.AppendKeyIdRow($"{I(5)}", $"key{I(5)}", 3);

                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(3, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{6} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(1, count);

                foreach (var record in resultset1)
                {
                    Console.WriteLine($"{record.Key} {record.ID}");

                    Assert.AreEqual($"{(3).ToString().PadLeft(10, '0')}", record.ID);
                    Assert.AreEqual($"key{I(3)} & key{I(3)}", record.Key);
                }
            }
        }

        // AND 的两端单个结果集内部有部分重叠
        [TestMethod]
        public void test_and_keyid_6()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(1)}", $"key{I(1)}", 1);
                    context.AppendKeyIdRow($"{I(2)}", $"key{I(2)}", 2);
                    context.AppendKeyIdRow($"{I(2)}", $"key{I(2)}", 3);
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 1);
                    context.AppendKeyIdRow($"{I(4)}", $"key{I(4)}", 2);
                    context.AppendKeyIdRow($"{I(5)}", $"key{I(5)}", 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(1, count);

                foreach (var record in resultset1)
                {
                    Console.WriteLine($"{record.Key} {record.ID}");

                    Assert.AreEqual($"{(3).ToString().PadLeft(10, '0')}", record.ID);
                    Assert.AreEqual($"key{I(3)} & key{I(3)}", record.Key);
                }
            }
        }

        [TestMethod]
        public void test_and_keyid_7()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(1)}", $"key{I(1)}", 1);
                    context.AppendKeyIdRow($"{I(2)}", $"key_1_{I(2)}", 2);
                    context.AppendKeyIdRow($"{I(2)}", $"key_2_{I(2)}", 3);
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(2)}", $"key_3_{I(2)}", 1);
                    context.AppendKeyIdRow($"{I(4)}", $"key{I(4)}", 2);
                    context.AppendKeyIdRow($"{I(5)}", $"key{I(5)}", 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(1, count);
                if (count == 1)
                {
                    var record = resultset1[0];
                    // foreach (var record in resultset1)
                    {
                        Console.WriteLine($"{record.Key} {record.ID}");
                        Assert.AreEqual($"{I(2)}", record.ID);
                        var correct_list = new List<string>()
                        {
                            $"key_1_{I(2)}, key_2_{I(2)}",
                            $"key_3_{I(2)}",
                        };
                        var result_list = record.Key.Split('&').Select(s => s.Trim()).ToList();
                        result_list.Sort();
                        CollectionAssert.AreEquivalent(correct_list, result_list);
                        // Assert.AreEqual($"key_1_{I(2)}, key_2_{I(2)} & key_3_{I(2)}", record.Key);
                    }
                }
            }
        }


        #endregion

        #region AND KeyCount

        // 测试两个结果集进行与运算，KeyCount 风格
        [TestMethod]
        public void test_and_keycount_1()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = (i + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual(v, record.Count);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
            }
        }

        // 完全重合
        [TestMethod]
        public void test_and_keycount_2()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendKeyCountRow($"key{I(i)}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendKeyCountRow($"key{I(i)}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual(v * 2, record.Count);
                    Assert.AreEqual($"key{I(i)}", record.Key);
                    i++;
                }
            }
        }

        // AND 的左侧结果集为空
        [TestMethod]
        public void test_and_keycount_3()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);
            }
        }

        // AND 的右侧结果集为空
        [TestMethod]
        public void test_and_keycount_4()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

            }
        }

        // AND 的两侧结果集为空
        [TestMethod]
        public void test_and_keycount_4_1()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{0} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);
            }
        }


        // AND 的两端有部分重叠
        [TestMethod]
        public void test_and_keycount_5()
        {
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(1)}", v, 1);
                    context.AppendKeyCountRow($"key{I(2)}", v, 2);
                    context.AppendKeyCountRow($"key{I(3)}", v, 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(3)}", v, 1);
                    context.AppendKeyCountRow($"key{I(4)}", v, 2);
                    context.AppendKeyCountRow($"key{I(5)}", v, 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(3, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{6} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(1, count);

                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"key{I(3)}", record.Key);
                    Assert.AreEqual(v * 2, record.Count);
                }
            }
        }

        // AND 的两端单个结果集内部有部分重叠
        [TestMethod]
        public void test_and_keycount_6()
        {
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(1)}", v, 1);
                    context.AppendKeyCountRow($"key{I(2)}", v, 2);
                    context.AppendKeyCountRow($"key{I(2)}", v, 3);
                    context.AppendKeyCountRow($"key{I(3)}", v, 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(3)}", v, 1);
                    context.AppendKeyCountRow($"key{I(4)}", v, 2);
                    context.AppendKeyCountRow($"key{I(5)}", v, 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.AND(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(1, count);

                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"key{I(3)}", record.Key);
                    Assert.AreEqual(v * 2, record.Count);
                    Console.WriteLine(record.Key + " " + record.Count);
                }
            }
        }

        #endregion

        // // //
        #region SUB Id

        // 测试两个结果集进行减法运算
        // 完全不重合
        [TestMethod]
        public void test_sub_id_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 2).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT / 2, count);

                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{I(i * 2 + 1)}", record.ID);
                    i++;
                }
            }
        }

        // 指定目标结果集参数
        [TestMethod]
        public void test_sub_id_1_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");
            var filename3 = GetTempFileName("003");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 2).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            using (var resultset3 = new DuckResultSet(ResultSetType.Id, filename3))
            {
                Assert.AreEqual(COUNT / 2, resultset1.Count);
                Assert.AreEqual(COUNT / 2, resultset2.Count);
                Assert.AreEqual(0, resultset3.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2, resultset3);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset3.Count;
                Assert.AreEqual(COUNT / 2, count);

                Assert.AreEqual(COUNT / 2, resultset1.Count);
                Assert.AreEqual(COUNT / 2, resultset2.Count);

                int i = 0;
                foreach (var record in resultset3)
                {
                    Assert.AreEqual($"{I(i * 2 + 1)}", record.ID);
                    i++;
                }
            }
        }


        // 完全重合
        [TestMethod]
        public void test_sub_id_2()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                // resultset1.Sort();

            }
        }


        // AND 的左侧结果集为空
        [TestMethod]
        public void test_sub_id_3()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{(i + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                /*
                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                    i++;
                }
                */
            }
        }

        // SUB 的右侧结果集为空
        [TestMethod]
        public void test_sub_id_4()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{(i + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{I(i + 1)}", record.ID);
                    i++;
                }
            }
        }

        // SUB 的两侧结果集为空
        [TestMethod]
        public void test_sub_id_4_1()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{0} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                foreach (var record in resultset1)
                {
                    Assert.Fail("空结果集不该有任何记录");
                }
            }
        }


        // SUB 的两端有部分重叠
        [TestMethod]
        public void test_sub_id_5()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"1".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"2".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"4".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"5".PadLeft(10, '0')}", 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(3, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{6} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(2, count);

                {
                    var record = resultset1[0];
                    Assert.AreEqual($"{I(1)}", record.ID);
                }

                {
                    var record = resultset1[1];
                    Assert.AreEqual($"{I(2)}", record.ID);
                }
            }
        }

        // SUB 的两端单个结果集内部有部分重叠
        [TestMethod]
        public void test_sub_id_6()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"1".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"2".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"2".PadLeft(10, '0')}", 3);
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendIdRow($"{"3".PadLeft(10, '0')}", 1);
                    context.AppendIdRow($"{"4".PadLeft(10, '0')}", 2);
                    context.AppendIdRow($"{"5".PadLeft(10, '0')}", 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(2, count);

                {
                    var record = resultset1[0];
                    Assert.AreEqual($"{I(1)}", record.ID);
                }

                {
                    var record = resultset1[1];
                    Assert.AreEqual($"{I(2)}", record.ID);
                }
            }
        }

        #endregion

        #region SUB KeyId

        // 测试两个结果集进行与运算，KeyId 风格
        [TestMethod]
        public void test_sub_keyid_1()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT / 2, count);

                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"key{I(i * 2 + 1)}", record.Key);
                    Assert.AreEqual($"{I(i * 2 + 1)}", record.ID);
                    i++;
                }
            }
        }

        // 完全重合
        [TestMethod]
        public void test_sub_keyid_2()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendKeyIdRow($"{I(i + 1)}", $"key1{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendKeyIdRow($"{I(i + 1)}", $"key2{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                // resultset1.Sort();
                /*
                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual($"{I(i + 1)}", record.ID);
                    if (record.Key == $"key2{I(i + 1)}, key1{I(i + 1)}"
                        || record.Key == $"key1{I(i + 1)}, key2{I(i + 1)}")
                    {

                    }
                    else
                        Assert.Fail($"{record.Key} 和预期值不同");
                    i++;
                }
                */
            }
        }


        // SUB 的左侧结果集为空
        [TestMethod]
        public void test_sub_keyid_3()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

            }
        }

        // SUB 的右侧结果集为空
        [TestMethod]
        public void test_sub_keyid_4()
        {
            const int COUNT = 1000000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyIdRow($"{number}", $"key{number}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual(number, record.ID);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
            }
        }

        // SUB 的两侧结果集为空
        [TestMethod]
        public void test_sub_keyid_4_1()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{0} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                foreach (var record in resultset1)
                {
                    Assert.Fail("空结果集不该有任何记录");
                }
            }
        }


        // SUB 的两端有部分重叠
        [TestMethod]
        public void test_sub_keyid_5()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(1)}", $"key{I(1)}", 1);
                    context.AppendKeyIdRow($"{I(2)}", $"key{I(2)}", 2);
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 1);
                    context.AppendKeyIdRow($"{I(4)}", $"key{I(4)}", 2);
                    context.AppendKeyIdRow($"{I(5)}", $"key{I(5)}", 3);

                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(3, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{6} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(2, count);

                {
                    var record = resultset1[0];
                    Assert.AreEqual($"{I(1)}", record.ID);
                    Assert.AreEqual($"key{I(1)}", record.Key);
                }

                {
                    var record = resultset1[1];
                    Assert.AreEqual($"{I(2)}", record.ID);
                    Assert.AreEqual($"key{I(2)}", record.Key);
                }
            }
        }

        // SUB 的两端单个结果集内部有部分重叠
        [TestMethod]
        public void test_sub_keyid_6()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(1)}", $"key{I(1)}", 1);
                    context.AppendKeyIdRow($"{I(2)}", $"keyfirst{I(2)}", 2);
                    context.AppendKeyIdRow($"{I(2)}", $"keysecond{I(2)}", 3);
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyIdRow($"{I(3)}", $"key{I(3)}", 1);
                    context.AppendKeyIdRow($"{I(4)}", $"key{I(4)}", 2);
                    context.AppendKeyIdRow($"{I(5)}", $"key{I(5)}", 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyId, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyId, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(2, count);

                {
                    var record = resultset1[0];
                    Assert.AreEqual($"{I(1)}", record.ID);
                    Assert.AreEqual($"key{I(1)}", record.Key);
                }

                {
                    var record = resultset1[1];
                    Assert.AreEqual($"{I(2)}", record.ID);
                    Assert.AreEqual($"keyfirst{I(2)}, keysecond{I(2)}", record.Key);
                }
            }
        }

        #endregion

        #region SUB KeyCount

        // 测试两个结果集进行与运算，KeyCount 风格
        [TestMethod]
        public void test_sub_keycount_1()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT / 2, count);

                // resultset1.Sort();

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = (i * 2 + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual(v, record.Count);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
            }
        }

        // 完全重合
        [TestMethod]
        public void test_sub_keycount_2()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendKeyCountRow($"key{I(i)}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendKeyCountRow($"key{I(i)}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                // resultset1.Sort();

                /*
                int i = 0;
                foreach (var record in resultset1)
                {
                    Assert.AreEqual(v * 2, record.Count);
                    Assert.AreEqual($"key{I(i)}", record.Key);
                    i++;
                }
                */
            }
        }

        // SUB 的左侧结果集为空
        [TestMethod]
        public void test_sub_keycount_3()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 2).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(COUNT, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);
            }
        }

        // SUB 的右侧结果集为空
        [TestMethod]
        public void test_sub_keycount_4()
        {
            const int COUNT = 1000000;
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        string number = ((i * 2) + 1).ToString().PadLeft(10, '0');
                        context.AppendKeyCountRow($"key{number}", v, i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(COUNT, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(COUNT, count);

                int i = 0;
                foreach (var record in resultset1)
                {
                    string number = (i * 2 + 1).ToString().PadLeft(10, '0');
                    Assert.AreEqual(v, record.Count);
                    Assert.AreEqual($"key{number}", record.Key);
                    i++;
                }
            }
        }

        // SUB 的两侧结果集为空
        [TestMethod]
        public void test_sub_keycount_4_1()
        {
            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                // 让内容为空

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(0, resultset1.Count);
                Assert.AreEqual(0, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{0} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(0, count);

                foreach (var record in resultset1)
                {
                    Assert.Fail("空结果集不该有任何记录");
                }
            }
        }


        // SUB 的两端有部分重叠
        [TestMethod]
        public void test_sub_keycount_5()
        {
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(1)}", v, 1);
                    context.AppendKeyCountRow($"key{I(2)}", v, 2);
                    context.AppendKeyCountRow($"key{I(3)}", v, 3);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(3)}", v, 1);
                    context.AppendKeyCountRow($"key{I(4)}", v, 2);
                    context.AppendKeyCountRow($"key{I(5)}", v, 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(3, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{6} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(2, count);

                {
                    var record = resultset1[0];
                    Assert.AreEqual($"key{I(1)}", record.Key);
                    Assert.AreEqual(v, record.Count);
                }

                {
                    var record = resultset1[1];
                    Assert.AreEqual($"key{I(2)}", record.Key);
                    Assert.AreEqual(v, record.Count);
                }
            }
        }

        // SUB 的两端单个结果集内部有部分重叠
        [TestMethod]
        public void test_sub_keycount_6()
        {
            const int v = 2;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(1)}", v, 1);
                    context.AppendKeyCountRow($"key{I(2)}", v, 2);
                    context.AppendKeyCountRow($"key{I(2)}", v, 3);
                    context.AppendKeyCountRow($"key{I(3)}", v, 4);
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    context.AppendKeyCountRow($"key{I(3)}", v, 1);
                    context.AppendKeyCountRow($"key{I(4)}", v, 2);
                    context.AppendKeyCountRow($"key{I(5)}", v, 3);
                }
                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset1 = new DuckResultSet(ResultSetType.KeyCount, filename1))
            using (var resultset2 = new DuckResultSet(ResultSetType.KeyCount, filename2))
            {
                Assert.AreEqual(4, resultset1.Count);
                Assert.AreEqual(3, resultset2.Count);

                Stopwatch sw = Stopwatch.StartNew();

                resultset1.SUB(resultset2);

                sw.Stop();
                Console.WriteLine($"{7} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");

                var count = resultset1.Count;
                Assert.AreEqual(2, count);

                foreach (var record in resultset1)
                {
                    Console.WriteLine($"{record.Key} {record.Count}");
                }

                {
                    var record = resultset1[0];
                    Assert.AreEqual($"key{I(1)}", record.Key);
                    Assert.AreEqual(v, record.Count);
                }

                {
                    var record = resultset1[1];
                    Assert.AreEqual($"key{I(2)}", record.Key);
                    Assert.AreEqual(v * 2, record.Count);
                }
            }
        }

        #endregion

        #region Interrupt

        [TestMethod]
        public void test_interrupt_sort_id()
        {
            const int COUNT = 700 * 10000;

            var filename = GetTempFileName();

            Stopwatch sw = Stopwatch.StartNew();
            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (var i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"@1/{(i + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            sw.Stop();
            var create_length = sw.Elapsed;
            Console.WriteLine($"创建 {COUNT} 条记录耗费时间 {sw.Elapsed}");

            sw.Restart();
            try
            {
                using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
                {
                    var count = resultset.Count;
                    Assert.AreEqual(COUNT, count);

                    // 延时 1 秒后中断
                    resultset.Sort(false,
                        new CancellationTokenSource(1000 * 1).Token);

                    Assert.Fail("Sort() 未如预期抛出 OperationCanceledException 异常");

                    resultset.Detach();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"如预期捕获了 OperationCanceledException 异常");
            }
            finally
            {
                sw.Stop();
                Console.WriteLine($"排序 {COUNT} 条记录耗费时间 {sw.Elapsed}");
            }


            /*
            sw.Restart();
            using (var resultset = new DuckResultSet(ResultSetType.Id, filename))
            {
                resultset.SetSorted();  // 恢复排序指示状态

                var count = resultset.Count;
                Assert.AreEqual(COUNT, count);

                int BATCH_SIZE = 100;
                int start = 0;
                for (int i = 0; i < COUNT / BATCH_SIZE; i++)
                {
                    var j = start;
                    foreach (var record in resultset.GetRange(start, BATCH_SIZE))
                    {
                        Assert.AreEqual($"@1/{(j + 1).ToString().PadLeft(10, '0')}", record.ID);
                        j++;
                    }

                    start += BATCH_SIZE;
                }
            }

            sw.Stop();
            Console.WriteLine($"遍历 {COUNT} 条记录耗费时间 {sw.Elapsed}");
            */
        }

        [TestMethod]
        public void test_interrupt_and()
        {
            const int COUNT = 500 * 10000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
                using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
                {
                    Assert.AreEqual(COUNT, resultset1.Count);
                    Assert.AreEqual(COUNT, resultset2.Count);


                    resultset1.AND(resultset2,
                        null,
                        "records",
                        " & ",
                        ", ",
                        new CancellationTokenSource(1).Token);

                    Assert.Fail("AND() 未如预期抛出 OperationCanceledException 异常");

                    sw.Stop();
                    Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");
                    sw = null;

                    var count = resultset1.Count;
                    Assert.AreEqual(COUNT, count);

                    // resultset1.Sort();

                    int i = 0;
                    foreach (var record in resultset1)
                    {
                        Assert.AreEqual($"{I(i + 1)}", record.ID);
                        i++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"如预期捕获了 OperationCanceledException 异常");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Stop();
                    Console.WriteLine($"{COUNT} 条 AND 过程耗费时间: {sw.Elapsed.ToString()}");
                }
            }
        }

        [TestMethod]
        public void test_interrupt_or()
        {
            const int COUNT = 500 * 10000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 奇数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 1).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT / 2; i++)
                    {
                        context.AppendIdRow($"{((i * 2) + 2).ToString().PadLeft(10, '0')}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
                using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
                {
                    resultset1.OR(resultset2,
                        null,
                        "records",
                        ", ",
                        new CancellationTokenSource(10 * 1).Token);

                    Assert.Fail("OR() 未如预期抛出 OperationCanceledException 异常");

                    sw.Stop();
                    Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");
                    sw = null;

                    var count = resultset1.Count;
                    Assert.AreEqual(COUNT, count);

                    int i = 0;
                    foreach (var record in resultset1)
                    {
                        Assert.AreEqual($"{(i + 1).ToString().PadLeft(10, '0')}", record.ID);
                        i++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"如预期捕获了 OperationCanceledException 异常");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Stop();
                    Console.WriteLine($"{COUNT} 条 OR 过程耗费时间: {sw.Elapsed.ToString()}");
                }
            }

        }

        [TestMethod]
        public void test_interrupt_sub()
        {
            const int COUNT = 500 * 10000;

            var filename1 = GetTempFileName("001");
            var filename2 = GetTempFileName("002");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename2))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    // 偶数
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
                using (var resultset2 = new DuckResultSet(ResultSetType.Id, filename2))
                {
                    Assert.AreEqual(COUNT, resultset1.Count);
                    Assert.AreEqual(COUNT, resultset2.Count);

                    resultset1.SUB(resultset2,
                        null,
                        "records",
                        ", ",
                        new CancellationTokenSource(10 * 1).Token);

                    Assert.Fail("SUB() 未如预期抛出 OperationCanceledException 异常");


                    sw.Stop();
                    Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");
                    sw = null;

                    var count = resultset1.Count;
                    Assert.AreEqual(0, count);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"如预期捕获了 OperationCanceledException 异常");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Stop();
                    Console.WriteLine($"{COUNT} 条 SUB 过程耗费时间: {sw.Elapsed.ToString()}");
                }
            }
        }

        [TestMethod]
        public void test_interrupt_dedup()
        {
            const int COUNT = 500 * 10000;

            var filename1 = GetTempFileName("001");

            using (var resultset = new DuckResultSet(ResultSetType.Id, filename1))
            {
                using (var context = resultset.GetAppenderContext())
                {
                    for (int i = 0; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{I(i + 1)}", i + 1);
                    }

                    for (int i = COUNT / 2; i < COUNT; i++)
                    {
                        context.AppendIdRow($"{I(i + 1)}", i + 1);
                    }
                }

                resultset.Detach(); // 保护物理文件不被 resultset.Close() 自动删除
            }


            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (var resultset1 = new DuckResultSet(ResultSetType.Id, filename1))
                {
                    Assert.AreEqual(COUNT + COUNT / 2, resultset1.Count);

                    resultset1.DeDup(", ",
                        new CancellationTokenSource(1).Token);

                    Assert.Fail("DeDup() 未如预期抛出 OperationCanceledException 异常");

                    sw.Stop();
                    Console.WriteLine($"{COUNT} 条 DeDup() 过程耗费时间: {sw.Elapsed.ToString()}");
                    sw = null;

                    var count = resultset1.Count;
                    Assert.AreEqual(COUNT, count);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"如预期捕获了 OperationCanceledException 异常");
            }
            finally
            {
                if (sw != null)
                {
                    sw.Stop();
                    Console.WriteLine($"{COUNT} 条 DeDup() 过程耗费时间: {sw.Elapsed.ToString()}");
                }
            }
        }

        #endregion

        #region EscapeString()

        [TestMethod]
        public void test_escapeString_1()
        {
            {
                string result = DuckResultSet.EscapeString("a");
                Assert.AreEqual("'a'", result);
            }
            {
                string result = DuckResultSet.EscapeString("ab");
                Assert.AreEqual("'ab'", result);
            }
            {
                string result = DuckResultSet.EscapeString("\r\n");
                Assert.AreEqual("chr(13) || chr(10)", result);
            }
            {
                string result = DuckResultSet.EscapeString("a\rb\nc");
                Assert.AreEqual("'a' || chr(13) || 'b' || chr(10) || 'c'", result);
            }

            {
                string result = DuckResultSet.EscapeString("a\r\n");
                Assert.AreEqual("'a' || chr(13) || chr(10)", result);
            }

            {
                string result = DuckResultSet.EscapeString("\rb\n");
                Assert.AreEqual("chr(13) || 'b' || chr(10)", result);
            }

            {
                string result = DuckResultSet.EscapeString("\r\nc");
                Assert.AreEqual("chr(13) || chr(10) || 'c'", result);
            }
        }

        #endregion

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

        static string S(string number)
        {
            return number.PadLeft(10, '0');
        }

        static string I(long number)
        {
            return number.ToString().PadLeft(10, '0');
        }
    }
}
