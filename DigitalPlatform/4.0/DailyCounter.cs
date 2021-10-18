using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace DigitalPlatform
{
    public class DailyCounter
    {
        public string Date { get; set; }
        public long Value { get; set; }

        static string DateTimeToString8(DateTime time)
        {
            return time.ToString("yyyyMMdd");
        }

        // 增量每日使用次数计数器
        // return:
        //      false   在限制范围内
        //      true    已经超出范围
        public static bool IncDailyCounter(
            string filename,
            // string counter_name,
            long limit_value)
        {
            try
            {
                string today = DateTimeToString8(DateTime.Now);
                // string filename = Path.Combine(Program.MainForm.UserDir, $"daily_counter_{counter_name}.txt");
                if (File.Exists(filename))
                {
                    var attr = File.GetAttributes(filename);
                    if (attr != FileAttributes.Normal)
                    {
                        File.SetAttributes(filename, FileAttributes.Normal);
                    }
                }
                /*
                FileInfo fi = new FileInfo(filename);
                if (fi.Exists)
                    fi.Attributes &= ~FileAttributes.Hidden;
                */
                try
                {
                    string value = null;
                    try
                    {
                        value = File.ReadAllText(filename);
                    }
                    catch (FileNotFoundException)
                    {
                        value = "";
                    }

                    DailyCounter counter = JsonConvert.DeserializeObject<DailyCounter>(value);
                    if (counter == null || counter.Date != today)
                        counter = new DailyCounter { Date = DateTimeToString8(DateTime.Now) };

                    counter.Value++;

                    value = JsonConvert.SerializeObject(counter);
                    File.WriteAllText(filename, value);
                    if (counter.Value > limit_value)
                        return true;
                    return false;
                }
                finally
                {
                    // fi.Attributes |= FileAttributes.Hidden;
                    File.SetAttributes(filename, FileAttributes.Hidden);
                }
            }
            catch
            {
                return true;
            }
        }
    }
}
