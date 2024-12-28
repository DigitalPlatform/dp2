using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DigitalPlatform;

using DigitalPlatform.RFID;
using DigitalPlatform.Script;

namespace RfidCenter
{
    public class RfidDriverHub
    {
        List<IRfidDriver> _rfidDrivers = new List<IRfidDriver>();

        public IEnumerable<IRfidDriver> Drivers
        {
            get
            {
                return _rfidDrivers;
            }
        }

        public void ReleaseDriver()
        {
            foreach (var driver in _rfidDrivers)
            {
                driver.ReleaseDriver();
            }

            _rfidDrivers.Clear();
        }

        public NormalResult LoadDriver()
        {
            ReleaseDriver();

            var bin_dir = Environment.CurrentDirectory;
            var fis = (new DirectoryInfo(bin_dir)).GetFiles("RfidDrivers.*.dll");

            string first = "RfidDrivers.First.dll";
            // 排序，把 first 放在最前面
            Array.Sort(fis, (a, b) => {
                var ret = string.Compare(a.Name, b.Name);
                if (ret == 0)
                    return 0;
                // 不等的情况下，检查其中一个是不是 first
                if (string.Compare(a.Name, first, true) == 0)
                    return -1;
                if (string.Compare(b.Name, first, true) == 0)
                    return 1;
                return ret;
            });

            foreach (var fi in fis)
            {
                // 只要 RfidDrivers.xxx.dll。排除掉 RfidDrivers.xxx.xxx.dll 这样的
                if (fi.Name.ToCharArray().Count(c => c == '.') != 2)
                    continue;

                var assembly = Assembly.LoadFile(fi.FullName);
                if (assembly == null)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"RFID 标准驱动 DLL '{fi.FullName}' 加载失败"
                    };
                }

                Type type = ScriptManager.GetDerivedClassType(
assembly,
"DigitalPlatform.RFID.IRfidDriver");
                if (type == null)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"RFID 标准驱动 DLL '{fi.FullName}' 中没有找到从 DigitalPlatform.RFID.IRfidDriver 派生的类。加载失败"
                    };
                }

                var obj = (IRfidDriver)type.InvokeMember(null,
BindingFlags.DeclaredOnly |
BindingFlags.Public | BindingFlags.NonPublic |
BindingFlags.Instance | BindingFlags.CreateInstance,
null,
null,
null);
                if (obj == null)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"创建类 {type.Name} 的实例失败"
                    };
                }

                _rfidDrivers.Add(obj);
            }

            // _rfidDrivers.RemoveAt(0);   // debug

            return new NormalResult();
        }

        public InitializeDriverResult InitializeDriver(string cfgFileName,
            string style,
            List<HintInfo> hint_table)
        {
            InitializeDriverResult first_result = null;
            List<Reader> readers = new List<Reader>();
            foreach (var driver in _rfidDrivers)
            {
                var result = driver.InitializeDriver(cfgFileName, style, hint_table);
                if (result.Value == -1)
                    return result;
                if (first_result == null)
                    first_result = result;
                readers.AddRange(result.Readers);
            }

            if (first_result == null)
                return new InitializeDriverResult
                {
                    Value = -1,
                    ErrorInfo = "目前尚未挂接任何 RFID 驱动"
                };

            return new InitializeDriverResult
            {
                Value = first_result.Value,
                ErrorCode = first_result.ErrorCode,
                ErrorInfo = first_result.ErrorInfo,
                Readers = readers,
                HintTable = first_result.HintTable
            };
        }

        public bool Pause
        {
            get
            {
                return _rfidDrivers.Where(driver => driver.Pause).Any();
            }
        }

        public void IncApiCount()
        {
            foreach (var driver in _rfidDrivers)
            {
                driver.IncApiCount();
            }
        }

        public void DecApiCount()
        {
            foreach (var driver in _rfidDrivers)
            {
                driver.DecApiCount();
            }
        }

        public NormalResult TurnSterilamp(string lampName, string action)
        {
            foreach (var driver in _rfidDrivers)
            {
                return driver.TurnSterilamp(lampName, action);
            }

            return new NormalResult
            {
                Value = -1,
                ErrorInfo = "因尚未装载任何 RFID 驱动，TurnSterilamp() 失败"
            };
        }

        public NormalResult TurnShelfLamp(string lampName, string action)
        {
            foreach (var driver in _rfidDrivers)
            {
                return driver.TurnShelfLamp(lampName, action);
            }

            return new NormalResult
            {
                Value = -1,
                ErrorInfo = "因尚未装载任何 RFID 驱动，TurnShelfLamp() 失败"
            };
        }
    }
}
