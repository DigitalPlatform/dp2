using DigitalPlatform.rms.Client.rmsws_localhost;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.rms.Client
{
    /// <summary>
    /// 枚举一层目录对象
    /// </summary>
    public class DirLoader : IEnumerable
    {
        public RmsChannel Channel { get; set; }
        public string Path { get; set; }
        public DigitalPlatform.Stop Stop { get; set; }
        public string Lang { get; set; }
        public string Style { get; set; }

        // 每批获取最多多少个记录
        public long BatchSize { get; set; }

        public DirLoader(RmsChannel channel,
            Stop stop,
            string path)
        {
            this.Channel = channel;
            this.Stop = stop;
            this.Path = path;
        }

        public IEnumerator GetEnumerator()
        {
            string strError = "";
            int nStart = 0;
            int nCount = -1;

            for (; ; )
            {
                ResInfoItem[] results = null;

                long lRet = this.Channel.DoDir(this.Path,
        nStart,
        nCount,
        string.IsNullOrEmpty(this.Lang) ? "zh" : this.Lang,
        this.Style,
        out results,
        out strError);
                if (lRet == -1)
                {
                    // 2017/6/7
                    if (this.Channel.OriginErrorCode == ErrorCodeValue.NotFound)
                        yield break;

                    throw new Exception(strError);
                }
                if (results == null)
                {
                    throw new Exception("results == null");
                }

                foreach(ResInfoItem item in results)
                {
                    yield return item;
                }
                
                nStart += results.Length;
                if (nStart >= lRet)
                    yield break;
            }
        }

    }
}
