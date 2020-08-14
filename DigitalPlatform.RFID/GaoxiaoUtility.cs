using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    /// <summary>
    /// 国内高校联盟标签实用函数
    /// 参考: http://www.cityu.edu.hk/lib/rfid_consortium/document.html
    /// </summary>
    public static class GaoxiaoUtility
    {
        // 解码
        public static GaoxiaoEpcInfo DecodeGaoxiaoEpc(byte[] data)
        {
            GaoxiaoEpcInfo result = new GaoxiaoEpcInfo();

            if (data.Length < 1)
                throw new ArgumentException("data 的字节数不应小于 1");
            byte first = data[0];
            // 安全位
            result.Lending = (first & 0x80) != 0;

            // 预留位
            result.Reserve = (first >> 4) & 0x03;

            // 分拣信息
            result.Picking = first & 0x0f;

            if (data.Length < 2)
                throw new ArgumentException("data 的字节数不应小于 2");
            byte second = data[1];

            // 编码方式
            result.EncodingType = (second >> 6) & 0x03;

            // 数据模型版本
            // 值 0 代表第一版
            result.Version = (second & 0x3f) + 1;

            byte third = data[2];
            byte fourth = data[3];

            // bit 高 --> bit 低
            // 31 30 29 28 27 26 24 16 , 15 14 12 11 6 5 4 3
            return result;
        }

        #region Content Parameters 演算

        static int[] offset_table = new int[] {
            3,4,5,6,11,12,14,15,16,24,26,27,28,29,30,31,
        };

        // 根据 offset 得到 OID
        static int GetOID(int offset)
        {
            if (offset < 0 || offset >= offset_table.Length)
                throw new ArgumentException($"offset 值 {offset} 超越合法范围(0-{offset_table.Length - 1})");
            return offset_table[offset];
        }

        // 解码两 byte 的 Content Parameter，返回 OID 列表
        public static int[] DecodeContentParameter(byte[] two_bytes)
        {
            List<int> results = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                bool on = ((two_bytes[1] >> i) & 0x01) != 0;
                if (on)
                    results.Add((byte)GetOID(i));
            }

            for (int i = 0; i < 8; i++)
            {
                bool on = ((two_bytes[0] >> i) & 0x01) != 0;
                if (on)
                    results.Add((byte)GetOID(i + 8));
            }
            return results.ToArray();
        }

        // 根据 OID 获得 bit 偏移量
        static int GetOffset(int oid)
        {
            for (int i = 0; i < offset_table.Length; i++)
            {
                if (oid == offset_table[i])
                    return i;
            }

            return -1;
        }

        // 把 OID 列表编码为 Content Parameter 的两 byte
        public static byte[] EncodeContentParameter(int[] oid_list)
        {
            UInt16 value = 0;
            foreach (var oid in oid_list)
            {
                var offset = GetOffset(oid);
                if (offset == -1)
                    throw new Exception($"OID {oid} 不允许出现在 Content Parameters 中");

                value |= (UInt16)(0x00000001 << offset);
            }

            return Compact.ReverseBytes(BitConverter.GetBytes(value));
        }

        #endregion
    }

    // 高校联盟 EPC 信息结构
    public class GaoxiaoEpcInfo
    {
        // *** EPC 的第 1 字节

        // 安全位：第 7bit（最高 bit）为安全位，有 0 和 1 两个取值，0 代表未出借状态（馆内存放），1 代表出借状态。
        public bool Lending { get; set; }

        // 预留位: 第 5-6bit 为预留位，例如可用于扩展分拣信息(初始化值为 00)。
        public int Reserve { get; set; }

        // 分拣信息: 第 0-4bit 为分拣信息，可用于标识馆藏分拣时所属的分拣箱号，目前共定义了 32 种不同的取值。
        public int Picking { get; set; }

        // *** EPC 的第二字节

        // 编码方式
        /*
 6-7bit（最高 2bit）为编码方式，其中：
 00 编码方式 1，EPC 容量为 96bit (12Byte)
 01 编码方式 2，EPC 容量为 128bit (16Byte)
 10 编码方式 3，EPC 容量为 144bit 或以上 (≥18Byte)
 11 编码方式 4，保留（暂无定义）
        * */
        public int EncodingType { get; set; }

        // 数据模型版本
        //  0-5bit（低 6bit）为版本说明，可用于标识 64 种不同的版本信息(0-63)。
        public int Version { get; set; }

        // *** EPC 的第 3-4 字节

        // 内容索引
        public byte[] ContentParameters { get; set; }

        // *** EPC 的第 5-12/16/18 字节

        // 馆藏标识符
        public string PII { get; set; }

    }
}
