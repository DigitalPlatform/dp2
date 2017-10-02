using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DigitalPlatform.LibraryClient.localhost;

// 2017/10/1

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// 文件事项枚举类
    /// 包装利用 ListFile() API
    /// </summary>
    public class FileItemLoader : IEnumerable
    {
        public LibraryChannel Channel { get; set; }
        public DigitalPlatform.Stop Stop { get; set; }

        public string Category { get; set; }
        public string FileName { get; set; }
        //public string Style { get; set; }
        //public string Lang { get; set; }

        // 每批获取最多多少个记录
        public long BatchSize { get; set; }

        public FileItemLoader(LibraryChannel channel,
            DigitalPlatform.Stop stop,
            string category,
            string filename)
        {
            this.Channel = channel;
            this.Stop = stop;
            this.Category = category;
            this.FileName = filename;
            //this.Style = style;
            //this.Lang = lang;
        }

        public IEnumerator GetEnumerator()
        {
            string strError = "";

            long nStart = 0;
            long nPerCount = -1;
            long nCount = 0;

            if (this.BatchSize != 0)
                nPerCount = this.BatchSize;

            while (true)
            {
                FileItemInfo[] items = null;
                long lRet = this.Channel.ListFile(
                    this.Stop,
                    "list",
                    this.Category,
                    this.FileName,
                    nStart,
                    nPerCount,
                    out items,
                    out strError);
                if (lRet == -1)
                {
                    if (this.Channel.ErrorCode != ErrorCode.NoError)
                        throw new ChannelException(Channel.ErrorCode, strError);

                    goto ERROR1;
                }

                if (items == null || items.Length == 0)
                    yield break;

                if (items != null)
                {
                    foreach (FileItemInfo item in items)
                    {
                        yield return item;
                    }

                    nStart += items.Length;
                    nCount += items.Length;
                }
                if (nCount >= lRet)
                    yield break;
            }
        ERROR1:
            throw new Exception(strError);
        }

    }
}
