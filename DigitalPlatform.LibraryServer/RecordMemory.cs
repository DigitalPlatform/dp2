using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.Core;
using DigitalPlatform.IO;
using MongoDB.Driver.Core.Misc;

namespace DigitalPlatform.LibraryServer
{
    // 用于存储记录传输中的分片的结构，合成完整的记录
    public class ChunkMemory
    {
        // 记录路径 --> ChunkItem
        Hashtable _table = new Hashtable();

        string _tempDir = null;

        public void Initialize(string temp_dir)
        {
            _tempDir = temp_dir;
        }

        const int MAX_CHUNKITEM_COUNT = 4096;

        // TODO: 应对多文件攻击。一个是定期清理过时的 Item；一个是发现超过极限数量的时候全部清空集合
        // 记忆一个 chunk
        // 注: 本函数要求前端严格按照 offset 从小到大顺序请求写入分片。主要是算法在收到第一个分片的时候自动清除了临时文件的全部内容，另外时间戳环环相扣的逻辑也是按照这个顺序来确定的
        // parameters:
        //      timestamp 前端发来的时间戳
        //      style   需要记忆的风格
        //      output_timestamp 服务器返回给前端的时间戳。前端会在下一次接续请求的时候发来这个时间戳
        // return:
        //      -2  时间戳不匹配
        //      -1  出错
        //      0   未完成
        //      1   完成
        public int Memory(string path,
            string range,
            long lTotalLength,
            byte[] chunk,
            byte[] timestamp,
            string style,
            out byte[] data,
            out byte[] output_timestamp,
            out string output_style,
            out string strError)
        {
            data = null;
            output_timestamp = null;
            output_style = null;
            strError = "";

            // 防止事项数越过极限
            if (this._table.Count > MAX_CHUNKITEM_COUNT)
            {
                this.Clear();
                strError = "MemoryTable 内的事项数超过极限值，已自动清理这些事项。请重试操作";
                return -1;
            }

            try
            {
                bool first_chunk = false; // 是否为第一个 chunk
                var item = GetItem(path);
                // string file_path = GetFilePath(path);
                // 确保目录已经创建
                PathUtil.CreateDirIfNeed(Path.GetDirectoryName(item.FilePath));
                using (var stream = new FileStream(item.FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    // 可能会抛出异常
                    var rangeList = new RangeList(range);
                    int offset = 0;
                    long tail = 0;  // 写入到达的尾部位置
                    foreach (var one_range in rangeList)
                    {
                        if (one_range.lStart == 0)
                        {
                            first_chunk = true;
                            item.FirstTimestamp = timestamp;
                            item.Style = style;
                            stream.SetLength(0);
                        }

                        stream.Seek(one_range.lStart, SeekOrigin.Begin);
                        stream.Write(chunk, offset, (int)one_range.lLength);
                        offset += (int)one_range.lLength;

                        tail = one_range.lStart + one_range.lLength;
                    }

                    // range 字符串为空的时候，当作第一个分片情形
                    if (string.IsNullOrEmpty(range))
                    {
                        first_chunk = true;
                        item.FirstTimestamp = timestamp;
                        item.Style = style;
                        stream.SetLength(0);
                    }

                    // 如果不是第一个 chunk，则需要检查和以前时间戳是否匹配
                    if (first_chunk == false)
                    {
                        if (ByteArray.Compare(timestamp, item.LastTimestamp) != 0)
                        {
                            output_timestamp = item.LastTimestamp;
                            strError = $"前端请求的时间戳({ByteArray.GetHexTimeStampString(timestamp)})和服务器端的时间戳({ByteArray.GetHexTimeStampString(item.LastTimestamp)})不匹配";
                            return -2;
                        }
                    }

                    // 最后一次
                    if (tail >= lTotalLength)
                    {
                        if (stream.Length != lTotalLength)
                        {
                            strError = $"ChunkMemory::Memory()临时文件的总字节数({stream.Length})和期望的字节数({lTotalLength})不一致";
                            goto ERROR; // 返回时要自动删除临时文件
                        }
                        data = new byte[stream.Length];
                        stream.Seek(0, SeekOrigin.Begin);
                        var getted = stream.Read(data, 0, data.Length);
                        if (getted < data.Length)
                        {
                            strError = $"ChunkMemory::Memory()从临时文件实际读取的字节数({getted})不足期望的字节数({data.Length})";
                            goto ERROR; // 返回时要自动删除临时文件
                        }
                        output_timestamp = item.FirstTimestamp; // 返回前端第一次发来的时间戳
                        output_style = item.Style;
                        goto FINISH;
                    }
                }

                item.LastTimestamp = NewTimestamp();
                output_timestamp = item.LastTimestamp;
                return 0;
            }
            catch (Exception ex)
            {
                strError = $"ChunkMemory::Memory()出现异常: {ex.Message}";
                return -1;
            }

        FINISH:
            DeleteItemByRecPath(path);
            return 1;
        ERROR:
            DeleteItemByRecPath(path);
            return -1;
        }

        static byte[] NewTimestamp()
        {
            return Guid.NewGuid().ToByteArray();
        }

        /*
        string GetFilePath(string rec_path)
        {
            lock (_table.SyncRoot)
            {
                var data = _table[rec_path] as ChunkItem;
                if (data == null)
                {
                    data = new ChunkItem { FilePath = GetTempFilePath()}
                    _table[rec_path] = data;
                }

                return data.FilePath;
            }
        }
        */

        ChunkItem GetItem(string rec_path)
        {
            lock (_table.SyncRoot)
            {
                var data = _table[rec_path] as ChunkItem;
                if (data == null)
                {
                    data = new ChunkItem
                    {
                        FilePath = GetTempFilePath(),
                        CreateTime = DateTime.Now
                    };
                    _table[rec_path] = data;
                }

                return data;
            }
        }

        /*
        public bool ChangeFirstTimestamp(string rec_path,
            byte[] timestamp)
        {
            var item = GetItem(rec_path);
            if (item == null)
                return false;
            item.FirstTimestamp = timestamp;
            return true;
        }
        */

        void DeleteItemByRecPath(string rec_path)
        {
            lock (_table.SyncRoot)
            {
                var data = _table[rec_path] as ChunkItem;
                if (data == null)
                    return;

                if (string.IsNullOrEmpty(data.FilePath) == false
                    && File.Exists(data.FilePath))
                    File.Delete(data.FilePath);

                _table.Remove(rec_path);
            }
        }

        // 清除全部事项
        public void Clear()
        {
            lock (_table.SyncRoot)
            {
                foreach (string key in _table.Keys)
                {
                    ChunkItem item = _table[key] as ChunkItem;
                    if (item == null)
                        continue;
                    item.DeleteTempFile();
                }

                _table.Clear();
            }
        }

        // 清理比较旧的事项
        public void CleanIdle(TimeSpan delta)
        {
            lock (_table.SyncRoot)
            {
                DateTime now = DateTime.Now;
                List<string> delete_keys = new List<string>();
                foreach (string key in _table.Keys)
                {
                    ChunkItem item = _table[key] as ChunkItem;
                    if (item == null)
                        continue;
                    if (now - item.CreateTime > delta)
                    {
                        item.DeleteTempFile();
                        delete_keys.Add(key);
                    }
                }

                foreach (string key in delete_keys)
                {
                    _table.Remove(key);
                }
            }
        }

        string GetTempFilePath()
        {
            return Path.Combine(this._tempDir, Guid.NewGuid().ToString());
        }

        // 根据前端的片段请求，返回 byte []
        public static int ReturnFragment(string xml,
            long fragment_start,
            int fragment_length,
            string style,
            out byte[] content,
            out long total_length,
            out string strError)
        {
            strError = "";
            content = null;
            total_length = 0;

            if (xml == null)
                throw new ArgumentException($"参数 {nameof(xml)} 值不允许为 null", nameof(xml));

            var data = Encoding.UTF8.GetBytes(xml);

            if (fragment_start > data.Length)
            {
                strError = $"起点({fragment_start})越过数据长度范围({data.Length})";
                return -1;
            }

            if (fragment_length == -1)
            {
                fragment_length = Math.Min(data.Length - (int)fragment_start, 100 * 1024);
                if (fragment_length < 0)
                {
                    strError = $"起点({fragment_start})超过最大长度({data.Length})";
                    return -1;
                }
            }
            else if (fragment_start + fragment_length > data.Length)
            {
                /*
                strError = $"起点({fragment_start})加上片段长度({fragment_length})超过数据总长度({data.Length})";
                return -1;
                */
                // 自动调节
                fragment_length = data.Length - (int)fragment_start;
                if (fragment_length < 0)
                {
                    strError = $"起点({fragment_start})越过数据长度范围({data.Length})";
                    return -1;
                }
            }

            total_length = data.Length;
            content = new byte[fragment_length];
            Array.Copy(data, fragment_start, content, 0, fragment_length);
            return 0;
        }

        // 判断一个范围字符串，是否为第一个分片
        public static bool IsFirstChunk(string range)
        {
            var rangeList = new RangeList(range);
            if (rangeList.Count == 0)
                return true;    // 当 range 为空字符串时，作用为写入一条空白记录
            if (rangeList.Count > 0 && rangeList[0].lStart == 0)
                return true;
            return false;
        }

        // 2024/10/22
        // 判断一个范围字符串，是否为最后一个分片
        public static bool IsTailChunk(string range, long total_length)
        {
            var rangeList = new RangeList(range);
            if (rangeList.Count == 0)
            {
                if (total_length == 0)
                    return true;
                return false; // 当 range 为空字符串时，作用为写入一条空白记录
            }
            var tail = rangeList[rangeList.Count - 1];
            if (tail.lStart + tail.lLength >= total_length)
                return true;
            return false;
        }
    }

    class ChunkItem
    {
        // 临时文件全路径
        public string FilePath { get; set; }

        // 第一个 chunk 请求时的时间戳。最后会用这个时间戳真正请求 dp2kernel
        public byte[] FirstTimestamp { get; set; }

        // 最新时间戳，这是 dp2library 和前端约定的，不断变化的一个时间戳，用于锁定循环时数据版本
        public byte[] LastTimestamp { get; set; }

        public DateTime CreateTime { get; set; }

        // 记忆的风格
        public string Style { get; set; }

        public void DeleteTempFile()
        {
            if (string.IsNullOrEmpty(this.FilePath) == false
    && File.Exists(this.FilePath))
            {
                File.Delete(this.FilePath);
                this.FilePath = null;
            }
        }
    }
}
