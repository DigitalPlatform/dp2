using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    public interface IRfidDriver
    {
        InitializeDriverResult InitializeDriver(string style, List<HintInfo> hint_table);

        NormalResult ReleaseDriver();

        // OpenReaderResult OpenReader(string serial_number);

        // NormalResult CloseReader(object reader_handle);

        InventoryResult Inventory(string reader_name, string style);

        GetTagInfoResult GetTagInfo(string reader_name, InventoryInfo info);

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

        GetLockStateResult GetShelfLockState(string lockName,
    int index);

        NormalResult OpenShelfLock(string lockName,
    int index);
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

        public override string ToString()
        {
            return $"uid={UID},dsfid={Element.GetHexString(DSFID)},afi={Element.GetHexString(AFI)},icref={Element.GetHexString(IcRef)},blkSize={BlockSize},blkNum={MaxBlockCount},lock={LockStatus},AntennaID={AntennaID},Bytes={Element.GetHexString(Bytes)}";
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
                UID = this.UID,
                DSFID = this.DSFID,
                AFI = this.AFI,
                IcRef = this.IcRef,
                BlockSize = this.BlockSize,
                MaxBlockCount = this.MaxBlockCount,
                LockStatus = this.LockStatus,
                AntennaID = this.AntennaID,
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
        public int AntannaCount { get; set; }   // 天线数量

        public UIntPtr ReaderHandle { get; set; }
        [NonSerialized]
        public OpenReaderResult Result = null;

        public override string ToString()
        {
            return $"Name={Name},SerialNumber={SerialNumber},DriverPath={DriverPath},Result={Result?.ToString()}";
        }

        // 匹配读卡器名字
        public static bool MatchReaderName(string list, string one)
        {
            if (list == "*" || list == one)
                return true;
            if (list.IndexOf(",") == -1)
                return false;
            string[] names = list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (Array.IndexOf(names, one) != -1)
                return true;
            return false;
        }
    }

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

    [Serializable()]
    public class LockState
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public string State { get; set; }

        public override string ToString()
        {
            return $"Name={Name},Index={Index},State={State}";
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
                    text.Append($"{i + 1}) {state.ToString()}");
                    i++;
                }
            }
            return text.ToString();
        }
    }
}
