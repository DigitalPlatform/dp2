using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using DigitalPlatform.IO;

namespace DigitalPlatform.dp2.Statis
{
    [Serializable()]
    public class ObjectItem
    {
        public string Key = "";
        public byte[] Data = null;

        public ObjectItem(string strKey, 
            byte [] data)
        {
            this.Key = strKey;
            this.Data = data;
        }
    }
    // 行对象
    public class ObjectLineItem : Item
    {
        int m_nLength = 0;
        ObjectItem m_line = null;

        byte[] m_buffer = null;

        string m_strLineKey = "";	// 专门从m_line成员中提出，便于排序
        /*
        long m_nKeyBytes = 0;
         * */

        public ObjectItem ObjectItem
        {
            get
            {
                return m_line;
            }
            set
            {
                m_line = value;

                this.m_strLineKey = m_line.Key;
                byte[] baKey = Encoding.UTF8.GetBytes(this.m_strLineKey);
                int nKeyBytes = baKey.Length;

                // 初始化二进制内容
                using (MemoryStream s = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(s, m_line);

                    this.Length = (int)s.Length + 4 + nKeyBytes;	// 算上了length所占bytes

                    m_buffer = new byte[(int)s.Length];
                    s.Seek(0, SeekOrigin.Begin);
                    s.Read(m_buffer, 0, m_buffer.Length);
                }
            }
        }

        public override int Length
        {
            get
            {
                return m_nLength;
            }
            set
            {
                m_nLength = value;
            }
        }

        public override void ReadData(Stream stream)
        {
            if (this.Length == 0)
                throw new Exception("length尚未初始化");

            // 读入m_lKeyBytes
            byte[] bytesbuffer = new byte[4];
            stream.Read(bytesbuffer, 0, 4);
            int nKeyBytes = BitConverter.ToInt32(bytesbuffer, 0);

            // 读入m_strLineKey
            byte[] keybuffer = new byte[nKeyBytes];
            stream.Read(keybuffer, 0, keybuffer.Length);

            this.m_strLineKey = Encoding.UTF8.GetString(keybuffer);

            // 读入Length个bytes的内容
            byte[] buffer = new byte[this.Length - 4 - keybuffer.Length];
            stream.Read(buffer, 0, buffer.Length);

            // 还原内存对象
            using (MemoryStream s = new MemoryStream(buffer))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                m_line = (ObjectItem)formatter.Deserialize(s);
            }
        }

        public override void ReadCompareData(Stream stream)
        {
            if (this.Length == 0)
                throw new Exception("length尚未初始化");

            // 读入m_nKeyBytes
            byte[] bytesbuffer = new byte[4];
            stream.Read(bytesbuffer, 0, bytesbuffer.Length);
            int nKeyBytes = BitConverter.ToInt32(bytesbuffer, 0);

            // 读入m_strLineKey
            byte[] keybuffer = new byte[nKeyBytes];
            stream.Read(keybuffer, 0, keybuffer.Length);
            this.m_strLineKey = Encoding.UTF8.GetString(keybuffer);

            m_line = null;	// 表示line对象不可用
        }

        public override void WriteData(Stream stream)
        {
            if (m_line == null)
            {
                throw (new Exception("m_line尚未初始化"));
            }

            if (m_buffer == null)
            {
                throw (new Exception("m_buffer尚未初始化"));
            }

            Debug.Assert(this.m_strLineKey == m_line.Key, "");
            byte[] keybytes = Encoding.UTF8.GetBytes(this.m_strLineKey);
            int nKeyBytes = keybytes.Length;

            // 单独写入
            byte[] buffer = BitConverter.GetBytes(nKeyBytes);
            stream.Write(buffer, 0, buffer.Length);

            // key本身
            stream.Write(keybytes, 0, keybytes.Length);


            // 写入Length个bytes的内容
            stream.Write(m_buffer, 0, this.Length - 4 - nKeyBytes);
        }

        // 实现IComparable接口的CompareTo()方法,
        // 根据ID比较两个对象的大小，以便排序，
        // 按右对齐方式比较
        // obj: An object to compare with this instance
        // 返回值 A 32-bit signed integer that indicates the relative order of the comparands. The return value has these meanings:
        // Less than zero: This instance is less than obj.
        // Zero: This instance is equal to obj.
        // Greater than zero: This instance is greater than obj.
        // 异常: ArgumentException,obj is not the same type as this instance.
        public override int CompareTo(object obj)
        {
            ObjectLineItem item = (ObjectLineItem)obj;

            return String.Compare(this.m_strLineKey, item.m_strLineKey);
        }
    }

    public class ObjectFile : ItemFileBase
    {
        public string TempDir = "";

        public ObjectFile()
        {
        }

        public override string GetTempFileName()
        {
            if (String.IsNullOrEmpty(this.TempDir) == true)
                return Path.GetTempFileName();

            PathUtil.TryCreateDir(this.TempDir);

            for (int i = 0; ; i++)
            {
                string strFilename = PathUtil.MergePath(this.TempDir, "~tempfile_" + i.ToString() + ".tmp");
                if (File.Exists(strFilename) == false)
                {
                    // 占据这个文件
                    using (FileStream f = File.Create(strFilename))
                    {
                    }
                    return strFilename;
                }
            }
        }

        public override Item NewItem()
        {
            return new ObjectLineItem();
        }

        // 获得排序后的非重复事项数
        public long GetNoDupCount()
        {
            long lResult = 0;

            this.Sort();

            long lCount = this.Count;
            string strPrevText = "";
            for (long i = 0; i < lCount; i++)
            {
                ObjectLineItem line_item = (ObjectLineItem)this[i];
                string strCurText = line_item.ObjectItem.Key;
                if (strPrevText != strCurText)
                {
                    lResult++;
                    strPrevText = strCurText;
                }
            }

            return lResult;
        }

        // 二分法
        // 根据给出的Key得到Value
        // return:
        //      -1  not found
        public int Search(string strKeyParam,
            out byte[] data)
        {
            data = null;

            int k;	// 区间左
            int m;	// 区间右
            int j = -1;	// 区间中
            string strKey;
            int nComp;

            k = 0;
            m = (int)this.Count - 1;
            while (k <= m)
            {
                j = (k + m) / 2;
                // 取得j位置的值

                ObjectLineItem item = (ObjectLineItem)this[j];

                strKey = item.ObjectItem.Key;

                nComp = String.Compare(strKey, strKeyParam);
                if (nComp == 0)
                {
                    data = item.ObjectItem.Data;
                    break;
                }

                if (nComp > 0)
                {	// strKeyParam较小
                    m = j - 1;
                }
                else
                {
                    k = j + 1;
                }

            }

            if (k > m)
                return -1;	// not found

            return j;
        }

    }
}
