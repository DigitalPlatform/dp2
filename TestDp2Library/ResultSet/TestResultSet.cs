using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.ResultSet;

namespace TestDp2Library.ResultSet
{
    /// <summary>
    /// 测试结果集有关功能
    /// </summary>
    [TestClass]
    public class TestResultSet
    {
        // 测试 MergeCount() 函数
        [TestMethod]
        public void Test_resultSet_mergeCount_01()
        {
            string condition = "重复1";
            var source = CreateSourceResultSet(condition);

            var target = new DpResultSet(() =>
            {
                return Path.GetTempFileName();
            });

            StringBuilder debugInfo = null;

            // 合并
            int nRet = DpResultSetManager.MergeCount(source,
target,
null,
null,
ref debugInfo,
out string strError);
            Assert.AreEqual(100, nRet);
            Assert.AreEqual(100, target.Count);
            CheckTargetResultSet(condition, target);
        }

        [TestMethod]
        public void Test_resultSet_mergeCount_02()
        {
            string condition = "重复2";
            var source = CreateSourceResultSet(condition);

            var target = new DpResultSet(() =>
            {
                return Path.GetTempFileName();
            });

            StringBuilder debugInfo = null;

            // 合并
            int nRet = DpResultSetManager.MergeCount(source,
target,
null,
null,
ref debugInfo,
out string strError);
            Assert.AreEqual(50, nRet);
            Assert.AreEqual(100, target.Count);
            CheckTargetResultSet(condition, target);
        }

        [TestMethod]
        public void Test_resultSet_mergeCount_03()
        {
            string condition = "重复3";
            var source = CreateSourceResultSet(condition);

            var target = new DpResultSet(() =>
            {
                return Path.GetTempFileName();
            });

            StringBuilder debugInfo = null;

            // 合并
            int nRet = DpResultSetManager.MergeCount(source,
target,
null,
null,
ref debugInfo,
out string strError);
            Assert.AreEqual(900, nRet);
            Assert.AreEqual(100, target.Count);
            CheckTargetResultSet(condition, target);
        }

        [TestMethod]
        public void Test_resultSet_mergeCount_04()
        {
            string condition = "重复4";
            var source = CreateSourceResultSet(condition);

            var target = new DpResultSet(() =>
            {
                return Path.GetTempFileName();
            });

            StringBuilder debugInfo = null;

            // 合并
            int nRet = DpResultSetManager.MergeCount(source,
target,
null,
null,
ref debugInfo,
out string strError);
            Assert.AreEqual(450, nRet);
            Assert.AreEqual(100, target.Count);
            CheckTargetResultSet(condition, target);
        }

        // 测试没有重复的情况
        [TestMethod]
        public void Test_resultSet_mergeCount_05()
        {
            string condition = "";
            var source = CreateSourceResultSet(condition);

            var target = new DpResultSet(() =>
            {
                return Path.GetTempFileName();
            });

            StringBuilder debugInfo = null;

            // 合并
            int nRet = DpResultSetManager.MergeCount(source,
target,
null,
null,
ref debugInfo,
out string strError);
            Assert.AreEqual(0, nRet);
            Assert.AreEqual(100, target.Count);
            CheckTargetResultSet(condition, target);
        }

        // 测试空结果集
        [TestMethod]
        public void Test_resultSet_mergeCount_06()
        {
            using (var source = new DpResultSet(() =>
            {
                return Path.GetTempFileName();
            }))
            using (var target = new DpResultSet(() =>
            {
                return Path.GetTempFileName();
            }))
            {
                source.Sort();

                StringBuilder debugInfo = null;

                // 合并
                int nRet = DpResultSetManager.MergeCount(source,
    target,
    null,
    null,
    ref debugInfo,
    out string strError);
                Assert.AreEqual(0, nRet);
                Assert.AreEqual(0, target.Count);
            }
        }

        // 测试只有一条记录的情况
        [TestMethod]
        public void Test_resultSet_mergeCount_07()
        {
            string condition = "一条记录";
            var source = CreateSourceResultSet(condition);

            var target = new DpResultSet(() =>
            {
                return Path.GetTempFileName();
            });

            StringBuilder debugInfo = null;

            // 合并
            int nRet = DpResultSetManager.MergeCount(source,
target,
null,
null,
ref debugInfo,
out string strError);
            Assert.AreEqual(0, nRet);
            Assert.AreEqual(1, target.Count);
            CheckTargetResultSet(condition, target);
        }


        static DpResultSet CreateSourceResultSet(string condition)
        {
            DpResultSet result = new DpResultSet(() =>
            {
                return Path.GetTempFileName();
            });

            if (condition == "一条记录")
            {
                DpRecord record = new DpRecord($"id1");
                record.Index = 1;
                record.BrowseText = $"browse1";
                result.Add(record);

                result.Sort();
                return result;
            }

            for (int i = 0; i < 100; i++)
            {
                DpRecord record = new DpRecord($"id{i.ToString().PadLeft(4, '0')}");
                record.Index = 1;
                record.BrowseText = $"browse{i}";
                result.Add(record);
            }

            // 全部 100 个都重复
            if (condition == "重复1")
            {
                for (int i = 0; i < 100; i++)
                {
                    DpRecord record = new DpRecord($"id{i.ToString().PadLeft(4, '0')}");
                    record.Index = 1;
                    record.BrowseText = $"browse{i}";
                    result.Add(record);
                }
            }

            // 只重复前 50 个
            if (condition == "重复2")
            {
                for (int i = 0; i < 50; i++)
                {
                    DpRecord record = new DpRecord($"id{i.ToString().PadLeft(4, '0')}");
                    record.Index = 1;
                    record.BrowseText = $"browse{i}";
                    result.Add(record);
                }
            }

            // 全部 100 个，每个都重复 10 次
            if (condition == "重复3")
            {
                for (int i = 0; i < 100; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        DpRecord record = new DpRecord($"id{i.ToString().PadLeft(4, '0')}");
                        record.Index = 1;
                        record.BrowseText = $"browse{i}";
                        result.Add(record);
                    }
                }
            }

            // 只重复前 50 个，每个重复 10 次
            if (condition == "重复4")
            {
                for (int i = 0; i < 50; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        DpRecord record = new DpRecord($"id{i.ToString().PadLeft(4, '0')}");
                        record.Index = 1;
                        record.BrowseText = $"browse{i}";
                        result.Add(record);
                    }
                }
            }

            result.Sort();
            return result;
        }

        static void CheckTargetResultSet(string condition,
            DpResultSet target)
        {
            if (condition == "")
            {
                for (int i = 0; i < 100; i++)
                {
                    DpRecord record = target[i];
                    Assert.AreEqual($"id{i.ToString().PadLeft(4, '0')}", record.ID);
                    Assert.AreEqual(1, record.Index);
                    Assert.AreEqual($"browse{i}", record.BrowseText);
                }

                return;
            }

            if (condition == "一条记录")
            {
                DpRecord record = target[0];
                Assert.AreEqual($"id1", record.ID);
                Assert.AreEqual(1, record.Index);
                Assert.AreEqual($"browse1", record.BrowseText);

                return;
            }

            if (condition == "重复1")
            {
                for (int i = 0; i < 100; i++)
                {
                    DpRecord record = target[i];
                    Assert.AreEqual($"id{i.ToString().PadLeft(4, '0')}", record.ID);
                    Assert.AreEqual(2, record.Index);
                    Assert.AreEqual($"browse{i}", record.BrowseText);
                }
            }

            if (condition == "重复2")
            {
                for (int i = 0; i < 50; i++)
                {
                    DpRecord record = target[i];
                    Assert.AreEqual($"id{i.ToString().PadLeft(4, '0')}", record.ID);
                    Assert.AreEqual(2, record.Index);
                    Assert.AreEqual($"browse{i}", record.BrowseText);
                }

                for (int i = 50; i < 100; i++)
                {
                    DpRecord record = target[i];
                    Assert.AreEqual($"id{i.ToString().PadLeft(4, '0')}", record.ID);
                    Assert.AreEqual(1, record.Index);
                    Assert.AreEqual($"browse{i}", record.BrowseText);
                }
            }

            if (condition == "重复3")
            {
                for (int i = 0; i < 100; i++)
                {
                    DpRecord record = target[i];
                    Assert.AreEqual($"id{i.ToString().PadLeft(4, '0')}", record.ID);
                    Assert.AreEqual(10, record.Index);
                    Assert.AreEqual($"browse{i}", record.BrowseText);
                }
            }

            if (condition == "重复4")
            {
                for (int i = 0; i < 50; i++)
                {
                    DpRecord record = target[i];
                    Assert.AreEqual($"id{i.ToString().PadLeft(4, '0')}", record.ID);
                    Assert.AreEqual(10, record.Index);
                    Assert.AreEqual($"browse{i}", record.BrowseText);
                }

                for (int i = 50; i < 100; i++)
                {
                    DpRecord record = target[i];
                    Assert.AreEqual($"id{i.ToString().PadLeft(4, '0')}", record.ID);
                    Assert.AreEqual(1, record.Index);
                    Assert.AreEqual($"browse{i}", record.BrowseText);
                }
            }
        }


    }
}
