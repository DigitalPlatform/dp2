using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Script;

namespace DigitalPlatform.LibraryServer
{
    // 和分片缓存有关的代码
    public partial class LibraryApplication
    {
        // 2023/1/16
        // Session 存储记忆
        public ChunkMemory MemoryTable { get; set; }

        // 记忆 WriteRes() API 中途的 chunk
        // parameters:
        //      timestamp 前端发来的时间戳
        //      output_timestamp 服务器返回给前端的时间戳。前端会在下一次接续请求的时候发来这个时间戳
        // return:
        //      -2  时间戳不匹配
        //      -1  出错
        //      0   未完成
        //      1   完成
        public int MemoryChunk(string path,
            string range,
            long lTotalLength,
            byte[] chunk,
            byte[] timestamp,
            out byte[] data,
            out byte[] output_timestamp,
            out string strError)
        {
            output_timestamp = null;

            if (this.MemoryTable == null)
            {
                this.MemoryTable = new ChunkMemory();
                this.MemoryTable.Initialize(this.TempDir);
            }

            // 记忆
            // return:
            //      -2  时间戳不匹配
            //      -1  出错
            //      0   未完成
            //      1   完成
            int nRet = this.MemoryTable.Memory(path,
                range,
                lTotalLength,
                chunk,
                timestamp,
                out data,
                out output_timestamp,
                out strError);
            return nRet;
        }

        /*
        public bool ChangeFirstTimestamp(string path,
            byte[] timestamp)
        {
            return this.MemoryTable.ChangeFirstTimestamp(path, timestamp);
        }
        */

        #region GetRes() API 的记录缓存

        const int MAX_RESCACHE_COUNT = 4096;

        // 用于 GetRes() API 的记录缓存
        ObjectCache<GetResItem> _getResCache = new ObjectCache<GetResItem>();

        public void ResCache_SetRecord(string key,
            string recpath,
            string xml,
            string metadata,
            byte[] timestamp)
        {
            if (_getResCache.Count > MAX_RESCACHE_COUNT)
                _getResCache.Clear();

            var item = new GetResItem
            {
                Key = key,
                RecPath = recpath,
                Xml = xml,
                Metadata = metadata,
                Timestamp = timestamp,
                CreateTime = DateTime.Now
            };
            _getResCache.SetObject(key, item);
        }

        // parameters:
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到了
        public int ResCache_GetRecord(
            string key,
            out string recpath,
            out string xml,
            out string metadata,
            out byte[] timestamp)
        {
            recpath = null;
            xml = "";
            metadata = null;
            timestamp = null;

            var item = _getResCache.GetObject(key, null);
            if (item == null)
                return 0;   // 没有找到

            recpath = item.RecPath;
            xml = item.Xml;
            metadata = item.Metadata;
            timestamp = item.Timestamp;
            return 1;
        }

        public void ResCache_RemoveRecord(string key)
        {
            _getResCache.SetObject(key, null);
        }

        // 清除较旧的事项
        public void ResCache_CleanIdle(TimeSpan length)
        {
            DateTime now = DateTime.Now;
            List<GetResItem> delete_items = new List<GetResItem>();
            _getResCache.ProcessAll((item) =>
            {
                if (now - item.CreateTime > length)
                    delete_items.Add(item);
                return true;
            });

            foreach(var item in delete_items)
            {
                _getResCache.SetObject(item.Key, null);
            }
        }

        class GetResItem
        {
            // 缓存事项 Key。一般由记录路径和数据风格两部分拼接而成
            public string Key { get; set; }

            // 记录路径
            public string RecPath { get; set; }
            public string Xml { get; set; }
            public string Metadata { get; set; }
            public byte[] Timestamp { get; set; }

            public DateTime CreateTime { get; set; }
        }

        #endregion
    }
}
