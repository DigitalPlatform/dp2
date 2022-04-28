using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Text;

namespace DigitalPlatform.RFID
{
    public interface IRfidDriver
    {
        InitializeDriverResult InitializeDriver(string cfgFileName,
            string style,
            List<HintInfo> hint_table);

        NormalResult ReleaseDriver();

        // OpenReaderResult OpenReader(string serial_number);

        // NormalResult CloseReader(object reader_handle);

        InventoryResult Inventory(string reader_name, string style);

        InventoryResult Inventory(string reader_name, string antenna_list, string style);

        // 2020/10/10 增加 style 参数
        GetTagInfoResult GetTagInfo(string reader_name,
            InventoryInfo info,
            string style);

        NormalResult WriteTagInfo(
            string reader_name,
            TagInfo old_tag_info,
            // UInt32 tag_type,
            TagInfo new_tag_info);

        NormalResult ChangePassword(string reader_name,
string uid,
string type,
uint old_password,
uint new_password);

        //void ConnectTag();

//void DisconnectTag();

#if OLD_SHELFLOCK

        GetLockStateResult GetShelfLockState(string lockName);

        // 2020/11/23 增加 style 参数
        NormalResult OpenShelfLock(string lockName, string style);
#endif

        NormalResult TurnShelfLamp(string lampName, string action);

        // 2020/4/8
        NormalResult TurnSterilamp(string lampName, string action);

    }

    // 一段连续的 block
    public class BlockRange
    {
        // 原始数据
        public byte[] Bytes { get; set; }
        // 块数
        public int BlockCount { get; set; }
        // 是否被锁定
        public bool Locked { get; set; }

        // 把原始 bytes 按照是否锁定的状态切割为若干段
        // parameters:
        //      ch  要判断的状态字符。为 'l' 或 'w'。'l' 表示已经锁定的；'w' 表示即将锁定的
        public static List<BlockRange> GetBlockRanges(
            int block_size,
            byte[] data,
            string lock_string,
            char ch)
        {
            // testing
            // throw new ArgumentException($"test test");

            if ((data.Length % block_size) != 0)
                throw new ArgumentException($"data 的 Length({data.Length}) 必须是 block_size({block_size}) 的整倍数");

            List<byte> bytes = new List<byte>();
            bool prev_locked = false;

            List<BlockRange> ranges = new List<BlockRange>();
            for (int i = 0; i < data.Length / block_size; i++)
            {
                bool current_locked = GetLocked(lock_string, i, ch);
                if (prev_locked != current_locked
                    && bytes.Count > 0)
                {
                    BlockRange range = new BlockRange
                    {
                        Bytes = bytes.ToArray(),
                        BlockCount = bytes.Count / block_size,
                        Locked = prev_locked,
                    };
                    ranges.Add(range);
                    bytes.Clear();
                }

                for (int j = 0; j < block_size; j++)
                    bytes.Add(data[(i * block_size) + j]);

                prev_locked = current_locked;
            }

            // 最后一次
            if (bytes.Count > 0)
            {
                BlockRange range = new BlockRange
                {
                    Bytes = bytes.ToArray(),
                    BlockCount = bytes.Count / block_size,
                    Locked = prev_locked,
                };
                ranges.Add(range);
                bytes.Clear();
            }

            return ranges;
        }

        public static bool GetLocked(string lock_string, int index, char ch)
        {
            if (index >= lock_string.Length)
                return false;
            return lock_string[index] == ch;
        }
    }

    // [Serializable()]
    public class InventoryResult : NormalResult
    {
        public List<InventoryInfo> Results { get; set; }

        public InventoryResult()
        {

        }

        public InventoryResult(NormalResult result) : base(result)
        {

        }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append(base.ToString() + "\r\n");
            if (Results != null)
            {
                int i = 1;
                foreach (InventoryInfo info in Results)
                {
                    text.Append($"{i++}) {info.ToString()}\r\n");
                }
            }
            return text.ToString();
        }
    }

    // [Serializable]
    public class InventoryInfo
    {
        // 协议类型
        public const string ISO15693 = "ISO15693";
        public const string ISO14443A = "ISO14443A";
        public const string ISO18000P6C = "ISO18000P6C";

        public string Protocol { get; set; }
        public string UID { get; set; }
        public UInt32 TagType { get; set; }

        public UInt32 AipID { get; set; }
        public UInt32 AntennaID { get; set; }
        public Byte DsfID { get; set; }

        public override string ToString()
        {
            return $"UID={(UID)}";
        }
    }

    [Serializable]
    public class TagInfo
    {
        // 2019/2/27
        public string ReaderName { get; set; }

        public string UID { get; set; }
        public byte DSFID { get; set; }
        public byte AFI { get; set; }
        public byte IcRef { get; set; }
        // 每个块内包含的字节数
        public UInt32 BlockSize { get; set; }
        // 块最大总数
        public UInt32 MaxBlockCount { get; set; }

        public bool EAS { get; set; }

        public uint AntennaID { get; set; }

        // 锁定状态字符串。表达每个块的锁定状态
        // 例如 "ll....lll"。'l' 表示锁定，'.' 表示没有锁定。缺省为 '.'。空字符串表示全部块都没有被锁定
        public string LockStatus { get; set; }

        // 芯片全部内容字节
        public byte[] Bytes { get; set; }

        // 2020/10/1
        // 附加的数据
        public object Tag { get; set; }

        // 2020/12/13
        // 芯片所采用的协议。如果为空表示 ISO15693
        public string Protocol { get; set; }

        public override string ToString()
        {
            return $"UID={UID},DSFID={Element.GetHexString(DSFID)},AFI={Element.GetHexString(AFI)},EAS={EAS},IcRef={Element.GetHexString(IcRef)},BlockSize={BlockSize},MaxBlockCount={MaxBlockCount},LockStatus={LockStatus},AntennaID={AntennaID},Protocol={Protocol},ReaderName={ReaderName},Bytes={Element.GetHexString(Bytes)}";
        }

        // 获得锁定状态字符串
        // 从 byte [] 转换为 string 形态
        public static string GetLockString(byte[] status)
        {
            if (status == null)
                return "";
            StringBuilder text = new StringBuilder();
            foreach (byte b in status)
            {
                text.Append(b == 0 ? "." : "l");
            }
            return text.ToString();
        }

        // 克隆出一个新的 TagInfo 对象
        public TagInfo Clone()
        {
            TagInfo result = new TagInfo
            {
                ReaderName = this.ReaderName,   // 2021/1/16
                UID = this.UID,
                DSFID = this.DSFID,
                AFI = this.AFI,
                IcRef = this.IcRef,
                BlockSize = this.BlockSize,
                MaxBlockCount = this.MaxBlockCount,
                EAS = this.EAS, // 2021/1/16
                AntennaID = this.AntennaID,
                LockStatus = this.LockStatus,
                Tag = this.Tag, // 2021/1/16
                Protocol = this.Protocol,
                Bytes = Clone(this.Bytes)
            };

            return result;
        }

        static byte[] Clone(byte[] source)
        {
            if (source == null)
                return null;
            byte[] result = new byte[source.Length];
            Array.Copy(source, result, source.Length);
            return result;
        }

        public void SetEas(bool enable)
        {
            if (enable == true)
            {
                this.AFI = 0x07;
                this.EAS = true;
            }
            else
            {
                this.AFI = 0xc2;
                this.EAS = false;
            }
        }
    }

    [Serializable()]
    public class GetTagInfoResult : NormalResult
    {
        public TagInfo TagInfo { get; set; }

        public GetTagInfoResult()
        {

        }

        public GetTagInfoResult(NormalResult result) : base(result)
        {

        }

        public override string ToString()
        {
            if (Value == 0)
                return this.TagInfo.ToString();
            return base.ToString();
        }
    }

    public class ReadBlocksResult : NormalResult
    {
        // 内容
        public byte[] Bytes { get; set; }
        // 块锁定状态字符串。字符 'l' 表示锁定，'.' 表示没有锁定
        public string LockStatus { get; set; }
    }

    // [Serializable]
    public class OpenReaderResult : NormalResult
    {
        public UIntPtr ReaderHandle { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()},ReaderHandle={ReaderHandle}";
        }
    }

    public class FindTagResult : NormalResult
    {
        // 找到标签所在的读卡器名字
        public string ReaderName { get; set; }
        public string UID { get; set; }

        // 2020/12/14
        public uint AntennaID { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()},ReaderName={ReaderName},UID={UID}";
        }
    }

    public class InitializeDriverResult : NormalResult
    {
        // [out]
        public List<Reader> Readers { get; set; }

        // [out]
        public List<HintInfo> HintTable { get; set; }

        public InitializeDriverResult(NormalResult result) : base(result)
        {

        }

        public InitializeDriverResult()
        {

        }
    }

    // COM 口暗示信息事项
    public class HintInfo
    {
        public string COM { get; set; }
        public string BaudRate { get; set; }
    }

    // 解析读卡器名称字符串以后得到的细部结构
    public class ReaderPath
    {
        // 读卡器名字
        public string ReaderName { get; set; }
        // 天线编号列表
        public List<string> AntennaList { get; set; }

        // 解析一段读卡器名字。例如 "readerName:1|2|3|4"
        public static ReaderPath Parse(string text)
        {
            ReaderPath result = new ReaderPath();

            var parts = StringUtil.ParseTwoPart(text, ":");
            result.ReaderName = parts[0];
            string list = parts[1];
            if (string.IsNullOrEmpty(list))
                result.AntennaList = new List<string> { "1" };
            else
            {
                string[] antenna_list = list.Split(new char[] { '|' });
                result.AntennaList = new List<string>(antenna_list);
            }

            return result;
        }
    }


    // [Serializable()]
    public class Reader
    {
        public string Name { get; set; }
        public string Type { get; set; }    // 类型 USB/COM
        public string DriverName { get; set; }  // RL8000 M201 等等
        public string ProductName { get; set; } // 产品型号
        public string Protocols { get; set; }   // 支持的协议。ISO15693,ISO14443A 等
        public string SerialNumber { get; set; }    // 序列号(USB)，或者 COM 端口号
        public string DriverPath { get; set; }
        public int AntennaCount { get; set; }   // 天线数量
        public int AntennaStart { get; set; }   // 天线编号开始号

        public UIntPtr ReaderHandle { get; set; }
        [NonSerialized]
        public OpenReaderResult Result = null;

        public string PreferName { get; set; }  // 推荐使用的名字 2020/9/12

        public string DriverVersion { get; set; }   // 厂家 DLL 驱动版本
        public string DeviceSN { get; set; }    // 厂家设备序列号。每台设备一个唯一的号

        public override string ToString()
        {
            return $"Name={Name},SerialNumber={SerialNumber},DriverPath={DriverPath},Result={Result?.ToString()},DriverName={DriverName}, ProductName={ProductName}, Protocols={Protocols}, AntennaCount={AntennaCount}, AntennaStart={AntennaStart}, DriverVersion={DriverVersion}, DeviceSN={DeviceSN}";
        }

        /*
        // 获得 readername:1|2|3|4 里面冒号左边的读卡器名部分
        public static string GetNamePart(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            var parts = StringUtil.ParseTwoPart(text, ":");
            return parts[0];
        }
        */

        // 匹配读卡器名字
        // parameters:
        //      list    要匹配的读卡器名字列表。形态为 "name1,name2" 或者 "name1:1|2|3|4,name2"
        //              list 有还可能为 "*" 或 "*:1|2|3|4"
        //      one     要匹配的单个读卡器名字。形态为 "name1"。注意它不能包含 *
        public static bool MatchReaderName(string list,
            string one,
            out string antenna_list)
        {
            antenna_list = "";

            if (one.IndexOf(":") != -1)
                throw new Exception($"参数 one 内容 ({one}) 不应该包含冒号");

            if (list == "*")
                return true;

            string[] names = list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //if (Array.IndexOf(names, one) != -1)
            //    return true;
            foreach (string name in names)
            {
                var parts = StringUtil.ParseTwoPart(name, ":");

                if (parts[0] == one || parts[0] == "*")
                {
                    antenna_list = parts[1];
                    return true;
                }
            }
            return false;
        }

        // 获得全部天线编号列表
        public byte[] GetAntennaList()
        {
            if (AntennaStart == -1)
                throw new Exception($"读卡器 '{Name}' 的 AntennaStart 成员尚未初始化");
            if (AntennaCount <= 0)
                throw new Exception($"读卡器 '{Name}' 的 AntennaCount 值({AntennaCount})不合法");

            List<byte> results = new List<byte>();
            for (int i = 0; i < AntennaCount; i++)
            {
                results.Add((byte)(i + AntennaStart));
            }

            return results.ToArray();
        }
    }

    // 书柜灯
    public class ShelfLamp
    {
        public string Name { get; set; }
        public string SerialNumber { get; set; }    // 序列号(USB)，或者 COM 端口号
        public UIntPtr LampHandle { get; set; }
    }

    // 书柜门锁
    public class ShelfLock
    {
        public string Name { get; set; }
        public string Type { get; set; }    // 类型 USB/COM
        public string DriverName { get; set; }  // 驱动名称
        public string ProductName { get; set; } // 产品型号
        public string Protocols { get; set; }   // 支持的协议
        public string SerialNumber { get; set; }    // 序列号(USB)，或者 COM 端口号
        public string DriverPath { get; set; }
        public UIntPtr LockHandle { get; set; }
    }

#if OLD_SHELFLOCK

    [Serializable()]
    public class LockState
    {
        public string Path { get; set; }
        public string Lock { get; set; }
        public int Board { get; set; }
        public int Index { get; set; }
        public string State { get; set; }

        public override string ToString()
        {
            return $"Path={Path},Lock={Lock},Board={Board},Index={Index},State={State}";
        }
    }

    [Serializable()]
    public class GetLockStateResult : NormalResult
    {
        public List<LockState> States { get; set; }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append(base.ToString() + "\r\n");
            if (this.States == null)
                text.Append("States=null\r\n");
            else
            {
                int i = 0;
                text.Append($"States.Count={States.Count}\r\n");
                foreach (var state in this.States)
                {
                    text.Append($"{i + 1}) {state.ToString()}\r\n");
                    i++;
                }
            }
            return text.ToString();
        }
    }

#endif
}
