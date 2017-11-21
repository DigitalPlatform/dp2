using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace DigitalPlatform.LibraryServer.Common
{
    /// <summary>
    /// 后台批处理任务 ServerReplication 的参数序列化/反序列化机制
    /// </summary>
    public class ServerReplicationParam
    {
        public string RecoverLevel { get; set; }
        public bool ClearFirst { get; set; }
        public bool ContinueWhenError { get; set; }

        public static ServerReplicationParam FromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new ServerReplicationParam();

            ServerReplicationParam result = JsonConvert.DeserializeObject<ServerReplicationParam>(value);
            if (result == null)
                result = new ServerReplicationParam();

            return result;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// 起始位置参数。
    /// 注：须严格限定，用于描述开始位置的参数才定义在这里。这样便于整体修改这部分的默认值
    /// </summary>
    public class ServerReplicationStart
    {
        public long Index { get; set; }    // 开始位置
        public string Date {get;set;}  // 开始文件名

        public static ServerReplicationStart FromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new ServerReplicationStart();

            ServerReplicationStart result = JsonConvert.DeserializeObject<ServerReplicationStart>(value);
            if (result == null)
                result = new ServerReplicationStart();
            else
            {
#if NO
                if (string.IsNullOrEmpty(result.Date) == false
                    && result.Date.Length > 8)
                    result.Date = result.Date.Substring(0, 8);
#endif
            }
            return result;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string GetSummary()
        {
            string strResult = "";
            strResult += "日期: " + this.Date + "\r\n";
            strResult += "偏移: " + this.Index.ToString();
            return strResult;
        }
    }
}
