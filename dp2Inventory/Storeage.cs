﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FreeSql.DataAnnotations;

using DigitalPlatform.CirculationClient;

namespace dp2Inventory
{
    public static class Storeage
    {
        static IFreeSql _fsql = null;

        public static void Initialize()
        {
            Debug.Assert(string.IsNullOrEmpty(ClientInfo.UserDir) == false);

            string filePath = Path.Combine(ClientInfo.UserDir, "history.db");
            var connectionString = $"Data Source={filePath};";
            _fsql = new FreeSql.FreeSqlBuilder()
.UseConnectionString(FreeSql.DataType.Sqlite, connectionString)
.UseAutoSyncStructure(true) //自动同步实体结构到数据库
.Build(); //请务必定义成 Singleton 单例模式

        }

        public static List<HistoryItem> LoadItems()
        {
            return _fsql.Select<HistoryItem>()
  // .Where(a => a.Id == 10)
  .ToList();
        }

        public static void SaveItems(List<HistoryItem> items)
        {
            // 先删除以前的全部数据
            _fsql.Delete<HistoryItem>().Where("1=1").ExecuteAffrows();

            // 然后保存数据
            var t2 = _fsql.Insert(items).ExecuteAffrows();
        }

        public static void Finish()
        {
            _fsql?.Dispose();
        }
    }

    public class HistoryItem
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string UID { get; set; }
        public string PII { get; set; }
        public string Title { get; set; }
        public string CurrentLocation { get; set; }
        public string Location { get; set; }
        public string State { get; set; }
        public string TOU { get; set; }
        public string OI { get; set; }

        public string WriteTime { get; set; }

        public string Action { get; set; }

        public string BatchNo { get; set; }
    }
}
