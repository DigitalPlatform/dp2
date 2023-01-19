using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Core;
using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer
{
    // 用于存储记录传输中的分片的结构，合成完整的记录
    public class ChunkMemory
    {
        // 记录路径 --> 临时文件全路径
        Hashtable _table = new Hashtable();

        string _tempDir = null;

        public void Initialize(string temp_dir)
        {
            _tempDir = temp_dir;
        }


        // 记忆
        // return:
        //      -1  出错
        //      0   未完成
        //      1   完成
        public int Memory(string path,
            string range,
            long lTotalLength,
            byte[] chunk,
            out byte[] data,
            out string strError)
        {
            data = null;
            strError = "";

            try
            {
                string file_path = GetFilePath(path);
                // 确保目录已经创建
                PathUtil.CreateDirIfNeed(Path.GetDirectoryName(file_path));
                using (var stream = new FileStream(file_path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    // 可能会抛出异常
                    var rangeList = new RangeList(range);
                    int offset = 0;
                    foreach (var one_range in rangeList)
                    {
                        stream.Seek(one_range.lStart, SeekOrigin.Begin);
                        stream.Write(chunk, offset, (int)one_range.lLength);
                        offset += (int)one_range.lLength;
                    }

                    // 最后一次
                    if (offset == (int)lTotalLength)
                    {
                        data = new byte[stream.Length];
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Read(data, 0, data.Length);
                        goto FINISH;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = $"ChunkMemory::Memory()出现异常: {ex.Message}";
                return -1;
            }

        FINISH:
            DeleteTempFile(path);
            return 1;
        }

        string GetFilePath(string rec_path)
        {
            string file_path = _table[rec_path] as string;
            if (file_path == null)
            {
                file_path = GetTempFilePath();
                _table[rec_path] = file_path;
            }

            return file_path;
        }

        void DeleteTempFile(string rec_path)
        {
            string file_path = _table[rec_path] as string;
            if (file_path == null)
                return;

            if (File.Exists(file_path))
                File.Delete(file_path);
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

            var data = Encoding.UTF8.GetBytes(xml);
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
                strError = $"起点({fragment_start})加上片段长度({fragment_length})超过数据总长度({data.Length})";
                return -1;
            }

            total_length = data.Length;
            content = new byte[fragment_length];
            Array.Copy(data, fragment_start, content, 0, fragment_length);
            return 0;
        }
    }
}
