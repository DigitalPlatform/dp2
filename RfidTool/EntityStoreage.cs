using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using FreeSql.DataAnnotations;
using Newtonsoft.Json;

namespace RfidTool
{
    public static class EntityStoreage
    {
        static IFreeSql _fsql = null;

        public static void Initialize()
        {
            Debug.Assert(string.IsNullOrEmpty(ClientInfo.UserDir) == false);

            string filePath = Path.Combine(ClientInfo.UserDir, "entity.db");
            var connectionString = $"Data Source={filePath};";
            _fsql = new FreeSql.FreeSqlBuilder()
.UseConnectionString(FreeSql.DataType.Sqlite, connectionString)
.UseAutoSyncStructure(true) //自动同步实体结构到数据库
.Build(); //请务必定义成 Singleton 单例模式

        }

        public static List<EntityItem> FindByBarcode(string barcode)
        {
            return _fsql.Select<EntityItem>()
                .Where(o => o.UII == barcode || o.UII.EndsWith("." + barcode))
                .ToList();
        }

        public static EntityItem FindByUII(string uii)
        {
            return _fsql.Select<EntityItem>()
                .Where(o => o.UII == uii)
                .First();
        }

        public delegate void delegate_showText(string text);

        class OfflineItem
        {
            public string UII { get; set; }
            public string RecPath { get; set; }
            public string Xml { get; set; }
            public byte[] Timestamp { get; set; }

            public string Title { get; set; }
        }

        // 导入脱机实体信息
        public static async Task<NormalResult> ImportOfflineEntityAsync(
string filename,
delegate_showText func_showProgress,
CancellationToken token)
        {
            try
            {
                int count = 0;
                using (var s = new StreamReader(filename, Encoding.UTF8))
                using (var reader = new JsonTextReader(s))
                using (var repo = _fsql.GetRepository<EntityItem>())
                {
                    while (token.IsCancellationRequested == false)
                    {
                        // https://www.newtonsoft.com/json/help/html/ReadMultipleContentWithJsonReader.htm
                        if (!reader.Read())
                            break;

                        if (reader.TokenType == JsonToken.StartArray
                            || reader.TokenType == JsonToken.EndArray
                            || reader.TokenType == JsonToken.Comment)
                            continue;

                        JsonSerializer serializer = new JsonSerializer();
                        OfflineItem o = serializer.Deserialize<OfflineItem>(reader);

                        func_showProgress?.Invoke($"正在导入 {o.UII} {o.Title} ...");

                        EntityItem item = new EntityItem();
                        item.UII = o.UII;
                        item.RecPath = o.RecPath;
                        item.Xml = o.Xml;
                        item.Timestamp = o.Timestamp;
                        item.Title = o.Title;
                        await repo.InsertOrUpdateAsync(item, token);

                        count++;
                    }
                }

                return new NormalResult { Value = count };
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"ImportOfflineEntityAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"ImportOfflineEntityAsync() 出现异常: {ex.Message}"
                };
            }
        }

        /*
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
        */

        // 删除全部记录
        public static void DeleteAll()
        {
            _fsql.Delete<EntityItem>().Where("1=1").ExecuteAffrows();
        }

        public static long GetCount()
        {
            return _fsql.Select<EntityItem>().Count();
        }

        public static void Finish()
        {
            _fsql?.Dispose();
        }
    }

    public class EntityItem
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        // 
        public string UII { get; set; }
        public string RecPath { get; set; }
        public string Xml { get; set; }
        public byte[] Timestamp { get; set; }

        public string Title { get; set; }
    }
}
