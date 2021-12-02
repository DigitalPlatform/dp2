using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Text;

namespace dp2SSL
{
    /// <summary>
    /// 统计网络流量用量
    /// </summary>
    public class NetworkUsage
    {
        List<AdaptorData> datas = new List<AdaptorData>();

        public NetworkUsage()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adaptor in interfaces)
            {
                this.datas.Add(new AdaptorData(adaptor));
            }
        }

        public void Reset()
        {
            foreach (var data in datas)
            {
                data.Reset();
            }
        }

        public List<NameStatisData> GetData(bool reset = false)
        {
            /*
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adaptor in interfaces)
            {

            }
            */
            List<NameStatisData> results = new List<NameStatisData>();
            foreach (var data in datas)
            {
                results.Add(new NameStatisData
                {
                    Name = data.AdaptorName,
                    Data = data.GetData(reset)
                });
            }

            return results;
        }

        public static string ToString(List<NameStatisData> datas)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (var data in datas)
            {
                if (text.Length == 0)
                {
                    string time_range = $"({data.Data.StartTime.ToString()} - {data.Data.EndTime.ToString()})";
                    text.AppendLine(time_range);
                }

                if (data.Data.IsEmpty())
                    continue;
                text.AppendLine($"{i+1}) {data.Name} \t接收字节数:{StringUtil.GetLengthText(data.Data.BytesReceived)} \t发送字节数:{StringUtil.GetLengthText(data.Data.BytesSent)}");
                i++;
            }

            return text.ToString();
        }
    }

    // 一个网络适配器的统计数据
    public class AdaptorData
    {
        NetworkInterface _adaptor;

        StatisData _start;
        StatisData _end;

        public AdaptorData(NetworkInterface adaptor)
        {
            _adaptor = adaptor;
            _start = (new StatisData()).GetData(adaptor);
        }

        // 重新开始统计
        public void Reset()
        {
            Debug.Assert(_adaptor != null);
            _start = (new StatisData()).GetData(this._adaptor);
        }

        // 获得从对象创建或者上次 reset 到现在的统计数据
        // parameters:
        //      reset   是否要同时 reset _start
        public StatisData GetData(bool reset = false)
        {
            Debug.Assert(_adaptor != null);

            _end = (new StatisData()).GetData(_adaptor);
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

        public string AdaptorName
        {
            get
            {
                if (_adaptor == null)
                    return null;
                return _adaptor.Name;
            }
        }
    }

    public class NameStatisData
    {
        public string Name { get; set; }

        public StatisData Data { get; set; }
    }

    // 统计数据基本单元
    public class StatisData
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public long BytesReceived { get; set; }

        public long BytesSent { get; set; }


        public bool IsEmpty()
        {
            return BytesReceived == 0 && BytesSent == 0;
        }

        public StatisData()
        {

        }

        public StatisData GetData(NetworkInterface adaptor)
        {
            StartTime = DateTime.Now;

            //var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            //foreach (var adaptor in interfaces)
            {
                var statis = adaptor.GetIPv4Statistics();
                this.BytesReceived += statis.BytesReceived;
                this.BytesSent += statis.BytesSent;
            }

            return this;
        }
    }
}
