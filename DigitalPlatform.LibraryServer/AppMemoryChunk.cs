using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
