using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace RfidCenter
{
    /// <summary>
    /// 用于模拟 RFID 读卡器的类，以方便没有硬件的情况下进行开发调试
    /// </summary>
    public class SimuReader
    {
        public List<Reader> Readers
        {
            get
            {
                return new List<Reader>(_readers);
            }
        }

        List<Reader> _readers = new List<Reader>();

        static string[] product_id_table = new string[] {
        "M201 690201",
        "RL8600 118001",
        "RD5100 680530",
        "RD5100(2) 680530"
        };

        static string GetProductID(string name)
        {
            foreach (var s in product_id_table)
            {
                var parts = StringUtil.ParseTwoPart(s, " ");
                if (name == parts[0])
                    return parts[1];
            }

            return null;
        }

        // 创建若干读卡器
        // parameters:
        //      names   读卡器名字列表
        public void Create(List<string> names)
        {
            _readers.Clear();
            foreach (string name in names)
            {
                var reader = new Reader();
                reader.Name = name;
                reader.Type = "USB";    // 类型 USB/COM

                var product_id = GetProductID(name);
                if (product_id == null)
                    throw new Exception($"(模拟读卡器) 名字 '{name}' 没有找到对应的 product id");
                
                bool bRet = RfidDrivers.First.RfidDriver1.GetDriverName(product_id,
    out string driver_name,
    out string product_name,
    out string protocols,
    out int antenna_count,
    out int min_antenna_id);
                if (bRet == false)
                {
                    string error = $"product_id {product_id} 在读卡器元数据中没有找到对应的 driver name";
                    throw new Exception(error);
                }

                reader.DriverName = driver_name;
                reader.ProductName = product_name;
                reader.Protocols = protocols;
                reader.AntennaCount = antenna_count;
                reader.AntennaStart = min_antenna_id;

                _readers.Add(reader);
            }
        }
    }


}
