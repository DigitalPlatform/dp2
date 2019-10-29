using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Core;

namespace dp2SSL
{
    public class SpeakList : SafeList<SpeakItem>
    {
        public delegate void Delegate_speak(string text);

        public void Speak(string type,
            int count,
            Delegate_speak func)
        {
            SpeakItem item = new SpeakItem
            {
                Format = type,
                Count = count,
                CreateTime = DateTime.Now
            };
            lock (_syncRoot)
            {
                this.Add(item);
            }

            Task.Run(() =>
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                var results = Merge();
                foreach (var result in results)
                {
                    string text = string.Format(type, result.Count);
                    func(text);
                }
            });
        }

        private readonly Object _syncRoot = new Object();

        // 把指定时刻以前创建的事项合并和输出
        List<SpeakItem> Merge()
        {
            // TODO: 是否加锁？
            List<SpeakItem> list = new List<SpeakItem>();
            lock (_syncRoot)
            {
                foreach (var item in this)
                {
                    list.Add(item);
                }

                foreach (var item in list)
                {
                    this.Remove(item);
                }
            }

            // 合并
            List<SpeakItem> merged = new List<SpeakItem>();
            SpeakItem current = null;
            foreach (var item in list)
            {
                if (current == null)
                {
                    current = item;
                    continue;
                }

                if (current.Format == item.Format)
                {
                    current.Count += item.Count;
                    continue;
                }

                // 推入结果队列
                merged.Add(current);
                current = item;
            }

            if (current != null)
                merged.Add(current);

            return merged;
        }
    }

    public class SpeakItem
    {
        public int Count { get; set; }
        public string Format { get; set; }    // Inc/Dec
        public DateTime CreateTime { get; set; }
    }
}
