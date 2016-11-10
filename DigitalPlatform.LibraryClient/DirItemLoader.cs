using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// Dir() 枚举器
    /// </summary>
    public class DirItemLoader : IEnumerable
    {
        public LibraryChannel Channel { get; set; }
        public DigitalPlatform.Stop Stop { get; set; }

        public string Lang { get; set; }
        public string Path { get; set; }
        public string Style { get; set; }

        // 每批获取最多多少个记录
        public long BatchSize { get; set; }

        public DirItemLoader(LibraryChannel channel,
            DigitalPlatform.Stop stop,
            string path,
            string style,
            string lang = "zh")
        {
            this.Channel = channel;
            this.Stop = stop;
            this.Path = path;
            this.Style = style;
            this.Lang = lang;
        }

        public IEnumerator GetEnumerator()
        {
            string strError = "";

            long nStart = 0;
            long nPerCount = -1;
            long nCount = 0;
            ErrorCodeValue kernel_errorcode = ErrorCodeValue.NoError;

            while (true)
            {
                ResInfoItem[] items = null;
                long lRet = this.Channel.Dir(this.Path,
                    nStart,
                    nPerCount,
                    this.Lang,
                    this.Style,
                    out items,
                    out kernel_errorcode,
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
                    foreach (ResInfoItem item in items)
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
