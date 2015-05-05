using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace DigitalPlatform.rms
{
    /// <summary>
    /// 2013/2/19
    /// 用于存储一批记录索引号的简单文件存储结构
    /// 每10个byte作为一个单元，密集排列
    /// 可以通过写入10个0 byte的方法表示删除的位置
    /// </summary>
    public class RecordIDStorage : IDisposable
    {
        public Stream Stream = null;
        public string FileName = "";
        public const int ID_LENGHTH = 10;

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        int m_nLockTimeout = 5 * 1000;

        public int Open(string strFileName,
            out string strError)
        {
            strError = "";

            Close();

            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 RecordIDStorage 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                try
                {
                    Stream = File.Open(
            strFileName,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.ReadWrite);

                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    return -1;
                }

                this.FileName = strFileName;
                return 0;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 清除文件中现有的全部内容
        public void Truncate()
        {
            if (this.Stream == null)
                return;
            this.Stream.SetLength(0);
        }

        public void Close()
        {
            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 RecordIDStorage 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                if (this.Stream != null)
                {
                    this.Stream.Close();
                    this.Stream = null;
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

            // 删除
        public void Delete()
        {
            Close();

            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 RecordIDStorage 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                if (string.IsNullOrEmpty(this.FileName) == false)
                {
                    try
                    {
                        File.Delete(this.FileName);
                    }
                    catch
                    {
                    }
                    this.FileName = null;
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 自动删除临时文件
        // 如果不想被删除，则需要用 Detach() 解除文件和对象之间的关系
        public void Dispose()
        {
            Delete();
        }

        // 解除文件和对象的关系，返回文件名
        // 如果不利用本汉书解除文件和对象的关系，一般情况下当对象销毁的时候，文件也会一并删除
        public string Detach()
        {
            string strFileName = this.FileName;

            Close();

            this.FileName = null;   // 解除关系
            return strFileName;
        }

        // 顺次读出下一个号码
        public bool Read(out string strID)
        {
            return Read(-1, out strID);
        }
#if NO
        // 顺次读出下一个号码
        public bool Read(out string strID)
        {
            strID = "";
            if (this.Stream == null)
                return false;

            byte[] data = new byte[ID_LENGHTH];
            int nRet = this.Stream.Read(data, 0, ID_LENGHTH);
            if (nRet < ID_LENGHTH)
                return false;

            strID = Encoding.ASCII.GetString(data);
            return true;
        }
#endif

        // 顺次写入一个号码
        public void Write(string strID)
        {
            Write(-1, strID);
        }
#if NO
        // 顺次写入一个号码
        public void Write(string strID)
        {
            if (this.Stream == null)
                throw new Exception("当前 Stream 尚未打开");

            if (strID == null)
            {
                // 将号码填充为删除值
                byte[] data = new byte[ID_LENGHTH];
                for (int i = 0; i < ID_LENGHTH; i++)
                {
                    data[i] = 0;
                }

                this.Stream.Write(data, 0, ID_LENGHTH);
                return;
            }

            if (string.IsNullOrEmpty(strID) == true
                || strID.Length != ID_LENGHTH)
                throw new ArgumentException("strID 值不合法", "strID");

            byte[] data = Encoding.ASCII.GetBytes(strID);
            if (data.Length != ID_LENGHTH)
                throw new ArgumentException("strID 值转换为 bytes 后应是 "+ID_LENGHTH+" bytes (而现在是 "+data.Length+" bytes)", "strID");

            Debug.Assert(data.Length == ID_LENGHTH, "");
            this.Stream.Write(data, 0, ID_LENGHTH);
        }
#endif

        // 指定位置读出一个号码
        // parameters:
        //      index   号码位置。如果是 -1 表示在当前位置写入
        //      strID   [out]读出的号码。如果为 null，表示把这个位置的号码已经被清空
        // return:
        //      false   文件已经结束，本次没有读入信息
        //      true   读入成功
        public bool Read(long index,
            out string strID)
        {
            if (this.m_lock.TryEnterReadLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 RecordIDStorage 加读锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                strID = "";
                if (this.Stream == null)
                    return false;

                if (index != -1)
                {
                    long offs = index * (long)ID_LENGHTH;
                    if (offs > this.Stream.Length - ID_LENGHTH)
                        throw new ArgumentException("index值 " + index + " 超过许可范围", "index");

                    if (this.Stream.Position != offs)
                        this.Stream.Seek(offs, SeekOrigin.Begin);
                }

                byte[] data = new byte[ID_LENGHTH];
                int nRet = this.Stream.Read(data, 0, ID_LENGHTH);
                if (nRet < ID_LENGHTH)
                    return false;

                if (data[0] == 0)
                    strID = null;
                else
                    strID = Encoding.ASCII.GetString(data);
                return true;
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        // 在文件尾部追加写入
        public void Append(string strID)
        {
            Write(-2, strID);
        }

        // 指定位置写入一个号码
        // parameters:
        //      index   号码位置。如果是 -1 表示在当前位置写入；-2表示在文件末尾追加写入
        //              -1和-2的差别是，当一直在文件末尾写入的时候，用-1本来是可以的，但当有其他移动文件指针读或者写的操作干扰的时候，就不能确保文件指针在文件末尾了，所以-2可以确保在文件尾部写入
        //      strID   要写入的号码。如果为 null，表示把这个位置的号码清空
        public void Write(int index, string strID)
        {
            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 RecordIDStorage 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                if (this.Stream == null)
                    throw new Exception("当前 Stream 尚未打开");

                if (string.IsNullOrEmpty(strID) == true
                    || strID.Length != ID_LENGHTH)
                    throw new ArgumentException("strID 值不合法", "strID");

                if (index == -2)
                    this.Stream.Seek(0, SeekOrigin.End);
                else if (index != -1)
                {
                    long offs = index * (long)ID_LENGHTH;
                    if (offs > this.Stream.Length)
                        throw new ArgumentException("index值 " + index + " 超过文件尾部", "index");

                    if (this.Stream.Position != offs)
                        this.Stream.Seek(offs, SeekOrigin.Begin);
                }

                if (strID == null)
                {
                    // 将号码填充为删除值
                    byte[] data = new byte[ID_LENGHTH];
                    for (int i = 0; i < ID_LENGHTH; i++)
                    {
                        data[i] = 0;
                    }

                    this.Stream.Write(data, 0, ID_LENGHTH);
                    return;
                }

                {
                    byte[] data = Encoding.ASCII.GetBytes(strID);
                    if (data.Length != ID_LENGHTH)
                        throw new ArgumentException("strID 值转换为 bytes 后应是 " + ID_LENGHTH + " bytes (而现在是 " + data.Length + " bytes)", "strID");

                    Debug.Assert(data.Length == ID_LENGHTH, "");
                    this.Stream.Write(data, 0, ID_LENGHTH);
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 号码的个数
        public long Count
        {
            get
            {
                if (this.m_lock.TryEnterReadLock(this.m_nLockTimeout) == false)
                    throw new ApplicationException("为 RecordIDStorage 加读锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
                try
                {
                    if (this.Stream == null)
                        return 0;
                    return this.Stream.Length / ID_LENGHTH;
                }
                finally
                {
                    this.m_lock.ExitReadLock();
                }
            }
        }

        // 从文件开始位置定位到第几个 ID 的位置
        // parameters:
        //      index   号码位置。如果是 -1 表示定位到当前位置；-2表示定位到文件末尾
        //              0   表示文件开头
        public void Seek(int index)
        {
            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 RecordIDStorage 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                if (this.Stream == null)
                    throw new Exception("当前 Stream 尚未打开");

                if (index == -2)
                    this.Stream.Seek(0, SeekOrigin.End);
                else if (index != -1)
                {
                    long offs = index * (long)ID_LENGHTH;
                    if (offs > this.Stream.Length)
                        throw new ArgumentException("index值 " + index + " 超过文件尾部", "index");

                    if (this.Stream.Position != offs)
                        this.Stream.Seek(offs, SeekOrigin.Begin);
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

    }
}
