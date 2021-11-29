using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace dp2SSL
{
    /// <summary>
    /// 统计网络流量用量
    /// </summary>
    public class NetworkUsage
    {
        StatisData _start;
        StatisData _end;

        public NetworkUsage()
        {
            _start = (new StatisData()).GetData();
        }

        // 重新开始统计
        public void Reset()
        {
            _start = (new StatisData()).GetData();
        }

        // 获得从对象创建或者上次 reset 到现在的统计数据
        // parameters:
        //      reset   是否要同时 reset _start
        public StatisData GetData(bool reset = false)
        {
            _end = (new StatisData()).GetData();
            var result = new StatisData
            {
                BytesReceived = _end.BytesReceived - _start.BytesReceived,
                BytesSent = _end.BytesSent - _start.BytesSent,
                StartTime = _start.StartTime,
                EndTime = _end.StartTime,
            };

            if (reset)
                _start = _end;

            return result;
        }
    }

    public class StatisData
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public long BytesReceived { get; set; }

        public long BytesSent { get; set; }


        public StatisData()
        {

        }

        public StatisData GetData()
        {
            StartTime = DateTime.Now;

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adaptor in interfaces)
            {
                var statis = adaptor.GetIPv4Statistics();
                this.BytesReceived += statis.BytesReceived;
                this.BytesSent += statis.BytesSent;
            }

            return this;
        }
    }
}
