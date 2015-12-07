using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections;
using System.Diagnostics;
using System.Data;
using System.Threading;

namespace DigitalPlatform.rms
{
    public class DelayTableCollection : IDisposable
    {
        Hashtable name_table = new Hashtable();

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        int m_nLockTimeout = 5 * 1000;

        public List<DelayTable> GetTables(string strDatabaseName)
        {
            if (this.m_lock.TryEnterReadLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTableCollection 加读锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                List<DelayTable> results = new List<DelayTable>();
                foreach (string key in this.name_table.Keys)
                {
                    DelayTable table = (DelayTable)name_table[key];
                    if (table.DatabaseName == strDatabaseName)
                        results.Add(table);
                }

                return results;
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        public int Count
        {
            get
            {
                return this.name_table.Count;
            }
        }

        public DelayTable GetTable(string strDatabaseName, string strTableName)
        {
            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTableCollection 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                string strName = strDatabaseName + "." + strTableName;
                DelayTable table = (DelayTable)name_table[strName];
                if (table == null)
                {
                    table = new DelayTable();
                    table.DatabaseName = strDatabaseName;
                    table.TableName = strTableName;
                    name_table[strName] = table;
                }

                return table;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public void Remove(DelayTable table)
        {
            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTableCollection 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                foreach (string key in this.name_table.Keys)
                {
                    DelayTable current = (DelayTable)name_table[key];
                    if (current == table)
                    {
                        this.name_table.Remove(key);
                        return;
                    }
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTableCollection 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                this.name_table.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public void Free()
        {
            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTableCollection 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                foreach (string key in this.name_table.Keys)
                {
                    DelayTable table = (DelayTable)this.name_table[key];
                    if (table != null)
                        table.Free();
                }
                this.name_table.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            this.Free();
        }

        public delegate string delegate_getfilename(string strDatabaseName, string strTableName);

        // return:
        //      -1  出错
        //      0   成功
        public int Write(
            string strDatabaseName,
            KeyCollection keys,
            delegate_getfilename getfilename,
            out string strError)
        {
            strError = "";

            // 确保 keys 里面的事项是排序过的。如果没有排序，本函数也能工作，只是效率略低
            DelayTable table = null;
            KeyCollection part_keys = new KeyCollection();

            foreach (KeyItem item in keys)
            {
                if (table == null)
                {
                    table = GetTable(strDatabaseName, item.SqlTableName);
                    if (string.IsNullOrEmpty(table.FileName) == true)
                    {
                        string strFilename = getfilename(strDatabaseName, item.SqlTableName);
                        int nRet = table.Create(strFilename, out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
                else
                {
                    if (table.TableName != item.SqlTableName)
                    {
                        if (part_keys.Count > 0)
                        {
                            table.Write(part_keys);
                            part_keys.Clear();
                        }

                        table = GetTable(strDatabaseName, item.SqlTableName);
                        if (string.IsNullOrEmpty(table.FileName) == true)
                        {
                            string strFilename = getfilename(strDatabaseName, item.SqlTableName);
                            int nRet = table.Create(strFilename, out strError);
                            if (nRet == -1)
                                return -1;
                        }
                    }
                }

                part_keys.Add(item);
            }

            if (part_keys.Count > 0)
            {
                Debug.Assert(table != null, "");
                table.Write(part_keys);
                part_keys.Clear();
            }

            return 0;
        }
    }

    /// <summary>
    /// 存储用于延迟写入 SQL 表的行信息
    /// </summary>
    public class DelayTable : IDataReader, IDisposable
    {
        public string DatabaseName = "";
        public string TableName = "";

        public FileStream Stream = null;
        public string FileName = "";

        XmlTextWriter _writer = null;
        XmlTextReader _reader = null;

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        int m_nLockTimeout = 5 * 1000;

        public int Create(string strOutputFileName,
            out string strError)
        {
            strError = "";

            Close();

            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTable 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {


                try
                {
                    Stream = File.Create(strOutputFileName);
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                _writer = new XmlTextWriter(Stream, Encoding.UTF8);
                //writer.Formatting = Formatting.Indented;
                //writer.Indentation = 4;

                _writer.WriteStartDocument();
                _writer.WriteStartElement("collection");


                this.FileName = strOutputFileName;
                return 0;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 文件大小
        public long Size
        {
            get
            {
                if (this.Stream == null)
                    return 0;
                return this.Stream.Length;
            }
        }

        public int OpenForRead(string strInputFileName,
            out string strError)
        {
            strError = "";

            Close();

            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTable 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                try
                {
                    Stream = File.Open(
            strInputFileName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);

                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                _reader = new XmlTextReader(Stream);

#if NO
            bool bRet = false;

            // 移动到根元素
            while (true)
            {
                bRet = reader.Read();
                if (bRet == false)
                {
                    strError = "没有根元素";
                    return -1;
                }
                if (reader.NodeType == XmlNodeType.Element)
                    break;
            }

            // 移动到其下级第一个element
            while (true)
            {
                bRet = reader.Read();
                if (bRet == false)
                {
                    strError = "没有第一个记录元素";
                    this.eof = true;
                    return 0;
                }
                if (reader.NodeType == XmlNodeType.Element)
                    break;
            }
#endif

                this.FileName = strInputFileName;
                return 0;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public void LockForRead()
        {
            if (this.m_lock.TryEnterReadLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTable 加读锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
        }

        public void UnlockForRead()
        {
            this.m_lock.ExitReadLock();
        }

        // 读入一个记录
        // return:
        //      false   文件结尾
        //      true    正常
        public bool Read(Hashtable table)
        {
#if NO
            if (this.m_lock.TryEnterReadLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTableCollection 加读锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
#endif
                if (_reader.NodeType != XmlNodeType.Element && _reader.Name != "item")
                {
                    if (_reader.ReadToFollowing("item") == false)
                        return false;
                }

                while (_reader.Read())
                {
                    while (_reader.NodeType == XmlNodeType.Element)
                    {
                        if (_reader.Name == "item")
                            return true;
                        // 字段名
                        table[_reader.Name] = _reader.ReadElementContentAsString();
                    }
                }

                return false;
#if NO
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
#endif
        }

        public void Write(KeyCollection keys)
        {
            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTable 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                foreach (KeyItem item in keys)
                {
                    _writer.WriteStartElement("item");
                    _writer.WriteElementString("keystring", item.Key);
                    // writer.WriteElementString("key1", item.KeyNoProcess);
                    _writer.WriteElementString("keystringnum", item.Num);
                    _writer.WriteElementString("fromstring", item.FromValue);
                    _writer.WriteElementString("idstring", item.RecordID);
                    _writer.WriteEndElement();
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public void Close()
        {
            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTable 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                if (_writer != null)
                {
                    _writer.WriteEndElement();
                    _writer.WriteEndDocument();
                    _writer.Close();
                    _writer = null;
                }

                if (_reader != null)
                {
                    _reader.Close();
                    _reader = null;
                }

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

        public void Free()
        {
            // 自动删除临时文件

            Close();

            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 DelayTable 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
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
        public void Dispose()
        {
            Free();
        }

        #region 实现 IDataReader

        // 摘要:
        //     获取一个值，该值指示当前行的嵌套深度。
        //
        // 返回结果:
        //     嵌套级别。
        public int Depth
        {
            get
            {
                return 1;
            }
        }

        //
        // 摘要:
        //     获取一个值，该值指示数据读取器是否已关闭。
        //
        // 返回结果:
        //     如果数据读取器已关闭，则为 true；否则为 false。
        public bool IsClosed
        {
            get
            {
                if (this._reader == null)
                    return false;
                return true;
            }
        }
        //
        // 摘要:
        //     通过执行 SQL 语句获取更改、插入或删除的行数。
        //
        // 返回结果:
        //     已更改、插入或删除的行数；如果没有任何行受到影响或语句失败，则为 0；-1 表示 SELECT 语句。
        public int RecordsAffected
        {
            get
            {
                return 0;
            }
        }

#if NO
        // 摘要:
        //     关闭 System.Data.IDataReader 对象。
        void Close()
        {


        }
#endif

        DataTable m_schemeTable = null;

        //
        // 摘要:
        //     返回一个 System.Data.DataTable，它描述 System.Data.IDataReader 的列元数据。
        //
        // 返回结果:
        //     一个描述列元数据的 System.Data.DataTable。
        //
        // 异常:
        //   System.InvalidOperationException:
        //     System.Data.IDataReader 是关闭的。
        public DataTable GetSchemaTable()
        {
            if (m_schemeTable == null)
            {
                this.m_schemeTable = new DataTable();
                foreach (string s in column_names)
                {
                    Type type = typeof(string);
                    if (s == "keystringnum")
                        type = typeof(long);

                    m_schemeTable.Columns.Add(new DataColumn(s, type)); 
                }
            }
            return m_schemeTable;
        }

        //
        // 摘要:
        //     当读取批处理 SQL 语句的结果时，使数据读取器前进到下一个结果。
        //
        // 返回结果:
        //     如果存在多个行，则为 true；否则为 false。
        public bool NextResult()
        {
            throw new Exception("not implememt");
            return false;
        }

        Hashtable m_currentData = null;

        //
        // 摘要:
        //     使 System.Data.IDataReader 前进到下一条记录。
        //
        // 返回结果:
        //     如果存在多个行，则为 true；否则为 false。
        public bool Read()
        {
            if (m_currentData == null)
                m_currentData = new Hashtable();
            m_currentData.Clear();
            return Read(m_currentData);
        }

        #endregion

        #region 实现 IDataRecord

        static string[] column_names = { 
                                           "keystring",
                                           "fromstring",
                                           "idstring",
                                           "keystringnum",
                                       };

        // 摘要:
        //     获取当前行中的列数。
        //
        // 返回结果:
        //     如果未放在有效的记录集中，则为 0；如果放在了有效的记录集中，则为当前记录的列数。默认值为 -1。
        public int FieldCount
        { 
            get
            {
                // (keystring,fromstring,idstring,keystringnum)
                return column_names.Length;
            }
        }

        // 摘要:
        //     获取位于指定索引处的列。
        //
        // 参数:
        //   i:
        //     要获取的列的从零开始的索引。
        //
        // 返回结果:
        //     作为 System.Object 位于指定索引处的列。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public object this[int i]
        {
            get
            {
                return m_currentData[column_names[i]];
            }
        }
        //
        // 摘要:
        //     获取具有指定名称的列。
        //
        // 参数:
        //   name:
        //     要查找的列的名称。
        //
        // 返回结果:
        //     名称指定为 System.Object 的列。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     未找到具有指定名称的列。
        public object this[string name]
        {
            get
            {
                return this.m_currentData[name];
            }
        }

        // 摘要:
        //     获取指定列的布尔值形式的值。
        //
        // 参数:
        //   i:
        //     从零开始的列序号。
        //
        // 返回结果:
        //     列的值。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public bool GetBoolean(int i)
        {
            return false;
        }
        //
        // 摘要:
        //     获取指定列的 8 位无符号整数值。
        //
        // 参数:
        //   i:
        //     从零开始的列序号。
        //
        // 返回结果:
        //     指定列的 8 位无符号整数值。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public byte GetByte(int i)
        {
            return 0;
        }

        //
        // 摘要:
        //     从指定的列偏移量将字节流作为数组从给定的缓冲区偏移量开始读入缓冲区。
        //
        // 参数:
        //   i:
        //     从零开始的列序号。
        //
        //   fieldOffset:
        //     字段中的索引，从该索引位置开始读取操作。
        //
        //   buffer:
        //     要将字节流读入的缓冲区。
        //
        //   bufferoffset:
        //     开始读取操作的 buffer 索引。
        //
        //   length:
        //     要读取的字节数。
        //
        // 返回结果:
        //     读取的实际字节数。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            string s = GetString(i);
            byte[] baContent = Encoding.UTF8.GetBytes(s);

            int start = (int)fieldOffset;
            int nCount = 0;
            for (int index = bufferoffset; index < bufferoffset + length; index++, start++, nCount++)
            {
                if (start >= s.Length)
                    break;
                buffer[index] = baContent[start];
            }

            return nCount;
        }
        //
        // 摘要:
        //     获取指定列的字符值。
        //
        // 参数:
        //   i:
        //     从零开始的列序号。
        //
        // 返回结果:
        //     指定列的字符值。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public char GetChar(int i)
        {
            string s = GetString(i);
            if (string.IsNullOrEmpty(s) == false)
                return s[0];

            return (char)0;
        }

        //
        // 摘要:
        //     从指定的列偏移量将字符流作为数组从给定的缓冲区偏移量开始读入缓冲区。
        //
        // 参数:
        //   i:
        //     从零开始的列序号。
        //
        //   fieldoffset:
        //     行中的索引，从该索引位置开始读取操作。
        //
        //   buffer:
        //     要将字节流读入的缓冲区。
        //
        //   bufferoffset:
        //     开始读取操作的 buffer 索引。
        //
        //   length:
        //     要读取的字节数。
        //
        // 返回结果:
        //     读取的实际字符数。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            string s = GetString(i);
            int start = (int)fieldoffset;
            int nCount = 0;
            for (int index = bufferoffset; index < bufferoffset + length; index++,start++,nCount ++)
            {
                if (start >= s.Length)
                    break;
                buffer[index] = s[start];
            }

            return nCount;
        }

        //
        // 摘要:
        //     返回指定的列序号的 System.Data.IDataReader。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     一个 System.Data.IDataReader。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public IDataReader GetData(int i)
        {
            return null;
        }

        //
        // 摘要:
        //     获取指定字段的数据类型信息。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     指定字段的数据类型信息。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public string GetDataTypeName(int i)
        {
            return GetFieldType(i).ToString();
        }
        //
        // 摘要:
        //     获取指定字段的日期和时间数据值。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     指定字段的日期和时间数据值。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public DateTime GetDateTime(int i)
        {
            string strColumnName = column_names[i];
            if (strColumnName == "keystringnum")
            {
                string s = (string)m_currentData[column_names[i]];
                long v = 0;
                Int64.TryParse(s, out v);
                return new DateTime(v);
            }

            return new DateTime(0);
        }
        //
        // 摘要:
        //     获取指定字段的固定位置的数值。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     指定字段的固定位置的数值。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public  decimal GetDecimal(int i)
        {
            string strColumnName = column_names[i];
            if (strColumnName == "keystringnum")
            {
                string s = (string)m_currentData[column_names[i]];
                long v = 0;
                Int64.TryParse(s, out v);
                return v;
            }

            return 0;
        }
        //
        // 摘要:
        //     获取指定字段的双精度浮点数。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     指定字段的双精度浮点数。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public double GetDouble(int i)
        {
            string strColumnName = column_names[i];
            if (strColumnName == "keystringnum")
            {
                string s = (string)m_currentData[column_names[i]];
                long v = 0;
                Int64.TryParse(s, out v);
                return v;
            }

            return 0;
        }
        //
        // 摘要:
        //     获取与从 System.Data.IDataRecord.GetValue(System.Int32) 返回的 System.Object 类型对应的
        //     System.Type 信息。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     与从 System.Data.IDataRecord.GetValue(System.Int32) 返回的 System.Object 类型对应的
        //     System.Type 信息。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public Type GetFieldType(int i)
        {
            string strColumnName = column_names[i];
            if (strColumnName == "keystringnum")
            {
                return typeof(long);
            }

            return typeof(string);
        }


        //
        // 摘要:
        //     获取指定字段的单精度浮点数。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     指定字段的单精度浮点数。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public float GetFloat(int i)
        {
            string strColumnName = column_names[i];
            if (strColumnName == "keystringnum")
            {
                string s = (string)m_currentData[column_names[i]];
                long v = 0;
                Int64.TryParse(s, out v);
                return v;
            }

            return 0;
        }
        //
        // 摘要:
        //     返回指定字段的 GUID 值。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     指定字段的 GUID 值。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public Guid GetGuid(int i)
        {
            return Guid.Empty;
        }

        //
        // 摘要:
        //     获取指定字段的 16 位有符号整数值。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     指定字段的 16 位有符号整数值。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public short GetInt16(int i)
        {
            string strColumnName = column_names[i];
            if (strColumnName == "keystringnum")
            {
                string s = (string)m_currentData[column_names[i]];
                Int16 v = 0;
                Int16.TryParse(s, out v);
                return v;
            }

            return 0;
        }

        //
        // 摘要:
        //     获取指定字段的 32 位有符号整数值。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     指定字段的 32 位有符号整数值。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public int GetInt32(int i)
        {
            string strColumnName = column_names[i];
            if (strColumnName == "keystringnum")
            {
                string s = (string)m_currentData[column_names[i]];
                Int32 v = 0;
                Int32.TryParse(s, out v);
                return v;
            }

            return 0;
        }
        //
        // 摘要:
        //     获取指定字段的 64 位有符号整数值。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     指定字段的 64 位有符号整数值。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public long GetInt64(int i)
        {
            string strColumnName = column_names[i];
            if (strColumnName == "keystringnum")
            {
                string s = (string)m_currentData[column_names[i]];
                long v = 0;
                Int64.TryParse(s, out v);
                return v;
            }

            return 0;
        }

        //
        // 摘要:
        //     获取要查找的字段的名称。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     字段名称或空字符串 ("")（如果没有返回值）。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public string GetName(int i)
        {
            return column_names[i];
        }

        //
        // 摘要:
        //     返回命名字段的索引。
        //
        // 参数:
        //   name:
        //     要查找的字段的名称。
        //
        // 返回结果:
        //     命名字段的索引。
        public int GetOrdinal(string name)
        {
            int i = 0;
            foreach (string s in column_names)
            {
                if (s == name)
                    return i;

                i++;
            }

            return -1;
        }

        //
        // 摘要:
        //     获取指定字段的字符串值。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     指定字段的字符串值。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public string GetString(int i)
        {
            return (string)m_currentData[column_names[i]];
        }

        //
        // 摘要:
        //     返回指定字段的值。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     返回时将包含字段值的 System.Object。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public object GetValue(int i)
        {
            return m_currentData[column_names[i]];
        }

        //
        // 摘要:
        //     使用当前记录的列值来填充对象数组。
        //
        // 参数:
        //   values:
        //     要将属性字段复制到的 System.Object 的数组。
        //
        // 返回结果:
        //     数组中 System.Object 的实例的数目。
        public int GetValues(object[] values)
        {
            values = new object[column_names.Length];
            for (int i = 0; i < column_names.Length; i++)
            {
                values[i] = m_currentData[column_names[i]];
            }

            return values.Length;
        }

        //
        // 摘要:
        //     返回是否将指定字段设置为空。
        //
        // 参数:
        //   i:
        //     要查找的字段的索引。
        //
        // 返回结果:
        //     如果指定的字段设置为 Null，则为 true；否则为 false。
        //
        // 异常:
        //   System.IndexOutOfRangeException:
        //     传递的索引位于 0 至 System.Data.IDataRecord.FieldCount 的范围之外。
        public bool IsDBNull(int i)
        {
            object o = m_currentData[column_names[i]];
            if (o == null)
                return true;
            return false;
        }


        #endregion
    }
}
