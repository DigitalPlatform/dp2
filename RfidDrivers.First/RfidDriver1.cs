using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using Serilog;
using RFIDLIB;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

// 锁定全部读卡器靠一个全局锁来实现。锁定一个读卡器靠 RecordLock 来实现。锁定一个读卡器之前，先尝试用 read 方式获得全局锁

namespace RfidDrivers.First
{
    public class RfidDriver1 : IRfidDriver
    {
        string _state = "closed";

        // 状态。
        //      "initializing" 表示正在进行初始化; "closed" 表示已经不能使用
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public bool Pause
        {
            get
            {
                if (string.IsNullOrEmpty(this.State) == false)
                    return true;
                return false;
            }
        }

        // 当前正在进行的 API 调用数
        long _apiCount = 0;

        public void IncApiCount()
        {
            Interlocked.Increment(ref _apiCount);
        }

        public void DecApiCount()
        {
            Interlocked.Decrement(ref _apiCount);
        }

        void WaitApiSilence()
        {
            while (true)
            {
                var v = Interlocked.Read(ref _apiCount);
                if (v <= 0)
                    return;
                Thread.Sleep(10);
            }
        }

        // 读卡器锁
        public RecordLockCollection reader_locks = new RecordLockCollection();

        // 全局锁
        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        void Lock()
        {
            _lock.EnterWriteLock();
            // _lock.TryEnterWriteLock(1000);  // 2019/8/29
        }

        void Unlock()
        {
            _lock.ExitWriteLock();
        }

        // TODO: 测试时候可以缩小为 1~5 秒，便于暴露出超时异常导致的问题
        // static int _lockTimeout = 5000; // 1000 * 120;   // 2 分钟

        void LockReader(Reader reader, int timeout = 5000)
        {
            _lock.EnterReadLock();
            try
            {
                // TODO: 把超时极限时间变长。确保书柜的全部门 inventory 和 getTagInfo 足够
                reader_locks.LockForWrite(reader.GetHashCode().ToString(), timeout);
            }
            catch
            {
                // 不要忘记 _lock.ExitReaderLock
                _lock.ExitReadLock();
                throw;
            }
        }

        void UnlockReader(Reader reader)
        {
            try
            {
                reader_locks.UnlockForWrite(reader.GetHashCode().ToString());
            }
            finally
            {
                // 无论如何都会调用这一句
                _lock.ExitReadLock();
            }
        }

        List<Reader> _readers = new List<Reader>();
        public List<Reader> Readers
        {
            get
            {
                return new List<Reader>(_readers);
            }
        }

        List<ShelfLock> _shelfLocks = new List<ShelfLock>();
        public List<ShelfLock> ShelfLocks
        {
            get
            {
                return new List<ShelfLock>(_shelfLocks);
            }
        }

        ShelfLamp _shelfLamp = null;

        public ShelfLamp ShelfLamp
        {
            get
            {
                return _shelfLamp;
            }
        }

        // parameters:
        //      style   风格列表。xxx,xxx,xxx 形态
        //              其中，lock:COM1|COM2 指定锁控 COM 口
        public InitializeDriverResult InitializeDriver(
            string cfgFileName,
            string style,
            List<HintInfo> hint_table)
        {
            this.State = "initializing";

            // 等待所有 API 调用安静下来
            WaitApiSilence();
            // TODO: 锁定所有读卡器
            Lock();
            try
            {
                GetDriversInfo();

                NormalResult result = OpenAllReaders(cfgFileName,
                    hint_table,
                    out List<HintInfo> output_hint_table);
                if (result.Value == -1)
                    return new InitializeDriverResult(result);

                string lock_param = StringUtil.GetParameterByPrefix(style, "lock");
                if (lock_param != null)
                {
                    var lock_result = OpenAllLocks(lock_param);
                    if (lock_result.Value == -1)
                        return new InitializeDriverResult
                        {
                            Value = -1,
                            ErrorInfo = lock_result.ErrorInfo
                        };
                }

                // 2019/11/12
                string lamp_param = StringUtil.GetParameterByPrefix(style, "lamp");
                if (lamp_param != null)
                {
                    var lamp_result = InitialLamp(lamp_param);
                    if (lamp_result.Value == -1)
                        return new InitializeDriverResult
                        {
                            Value = -1,
                            ErrorInfo = lamp_result.ErrorInfo
                        };
                }

                return new InitializeDriverResult
                {
                    Readers = _readers,
                    HintTable = output_hint_table
                };
            }
            finally
            {
                Unlock();
                this.State = "";
            }
        }

        public NormalResult ReleaseDriver()
        {
            this.State = "initializing";
            // 等待所有 API 调用安静下来
            WaitApiSilence();
            // 锁定所有读卡器
            Lock();
            try
            {
                FreeLamp();
                CloseAllLocks();

                return CloseAllReaders();
            }
            finally
            {
                Unlock();
                this.State = "closed";
            }
        }

        // 打开所有读卡器
        NormalResult OpenAllReaders(
            string cfgFileName,
            List<HintInfo> hint_table,
            out List<HintInfo> output_hint_table)
        {
            output_hint_table = null;
            _readers = new List<Reader>();

            Hashtable name_table = new Hashtable();
            List<Reader> readers = OpenUsbReaders(name_table, out NormalResult error);
            if (error != null)
                return error;
            readers.AddRange(OpenComReaders(name_table, hint_table, out output_hint_table, out error));

            // 2020/9/15
            if (error != null)
                return error;

            _readers = readers;

            // 2019/8/24 添加
            // 使用了暗示信息，但始终没有找到任何一个读卡器，这时候要尝试一次不使用暗示信息
            if (readers.Count == 0 && hint_table != null)
            {
                _readers = OpenComReaders(name_table, null, out output_hint_table, out error);
                // 2020/9/15
                if (error != null)
                    return error;
            }

            // 2019/10/23
            if (string.IsNullOrEmpty(cfgFileName) == false)
                _readers.AddRange(OpenTcpReaders(name_table, cfgFileName, out error));

            return new NormalResult();
        }

        static List<string> adjust_seq(string[] rates,
            List<HintInfo> hint_table,
            string com_name)
        {
            if (hint_table == null)
                return new List<string>(rates);

            var hint_item = hint_table.Find((o) => { return o.COM == com_name; });
            if (hint_item == null)
                return new List<string>(rates);

            if (hint_item.BaudRate == "!")  // 这个 COM 口不是读卡器，需要跳过
                return new List<string>();

            List<string> results = new List<string>();
            results.Add(hint_item.BaudRate);
            foreach (string rate in rates)
            {
                if (rate == hint_item.BaudRate)
                    continue;
                results.Add(rate);
            }

            return results;
        }

        // 打开所有 COM 口读卡器
        // parameters:
        //      name_table  用于查重的名字表
        //      hint_table  暗示信息表。如果为 null，表示不提供暗示信息
        //                  注：暗示信息表可以加快 COM 口读卡器打开的速度
        static List<Reader> OpenComReaders(Hashtable name_table,
            List<HintInfo> hint_table,
            out List<HintInfo> output_hint_table,
            out NormalResult error)
        {
            error = null;

            output_hint_table = new List<HintInfo>();
            // 枚举所有的 COM 口 reader
            List<Reader> readers = EnumComReader("M201");

            List<Reader> results = new List<Reader>();

            // name --> count
            // Hashtable table = new Hashtable();

            string[] rates = new string[] {
                "38400",    // 最常见的放在最前面
                "19200",
                "9600",
                "57600",
                "115200",
                "230400",
            };

            // 打开所有的 reader
            foreach (Reader reader in readers)
            {
                WriteInfoLog($"优化前的 rates {StringUtil.MakePathList(rates)}");

                List<string> rate_list = adjust_seq(rates,
    hint_table,
    reader.SerialNumber);
                /*
                // testing
                List<string> rate_list = new List<string>(rates);
                */
                WriteInfoLog($"针对 {reader.SerialNumber} 遍历尝试波特率(优化后的) '{StringUtil.MakePathList(rate_list)}'");

                foreach (string baudRate in rate_list)
                {
                    var fill_result = FillReaderInfo(reader, baudRate);
                    if (fill_result.Value == -1)
                    {
                        if (fill_result.ErrorCode == "driverNameNotFound")
                            error = new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"探测读卡器型号过程出错: {fill_result.ErrorInfo}"
                            };
                        continue;
                    }

                    StringBuilder debugInfo = new StringBuilder();
                    OpenReaderResult result = OpenReader(reader.DriverName,
                        reader.Type,
                        reader.SerialNumber,
                        baudRate,
                        debugInfo);
                    if (result.Value == -1)
                    {
                        WriteInfoLog($"以波特率 {baudRate} 成功打开 COM 口读卡器 {reader.SerialNumber} 返回失败={result.ToString()}。调试信息={debugInfo.ToString()}");
                        continue;
                    }
                    WriteInfoLog($"*** 以波特率 {baudRate} 成功打开 COM 口读卡器 {reader.SerialNumber} 返回成功={result.ToString()}。调试信息={debugInfo.ToString()}");

                    output_hint_table.Add(new HintInfo
                    {
                        COM = reader.SerialNumber,
                        BaudRate = baudRate
                    });

                    reader.Result = result;
                    reader.ReaderHandle = result.ReaderHandle;

                    // 构造 Name
                    // 重复的 ProductName 后面要加上序号
                    {
                        int count = 0;
                        if (name_table.ContainsKey(reader.ProductName) == true)
                            count = (int)name_table[reader.ProductName];

                        if (count == 0)
                            reader.Name = reader.ProductName;
                        else
                            reader.Name = $"{reader.ProductName}({count + 1})";

                        Debug.Assert(string.IsNullOrEmpty(reader.Name) == false, "");

                        name_table[reader.ProductName] = ++count;
                    }

                    results.Add(reader);
                    break;  // 一个波特率只要成功了一个，就不再尝试其他波特率
                }
            }

            // 找出不是读卡器的 COM 口
            foreach (Reader reader in readers)
            {
                var com_name = reader.SerialNumber;
                var found = results.Find((o) => { return o.SerialNumber == com_name; });
                if (found == null)
                {
                    output_hint_table.Add(new HintInfo
                    {
                        COM = reader.SerialNumber,
                        BaudRate = "!"  // 表示这个 COM 口不是读卡器
                    });
                }
            }

            //_readers = readers;
            //return new NormalResult();
            return results;
        }

        // 打开所有 USB 读卡器
        // parameters:
        //      name_table  用于查重的名字表
        static List<Reader> OpenUsbReaders(Hashtable name_table,
            out NormalResult error)
        {
            error = null;

            // 枚举所有的 USB  reader
            List<Reader> readers = EnumUsbReader("M201"); // "RL8000"

            List<Reader> removed = new List<Reader>();

            // name --> count
            // Hashtable table = new Hashtable();

            // 打开所有的 reader
            foreach (Reader reader in readers)
            {
                var fill_result = FillReaderInfo(reader, "");
                if (fill_result.Value == -1)
                {
#if NO
                    if (reader.Type == "COM")
                    {
                        removed.Add(reader);
                        continue;
                    }
                    return fill_result;
#endif
                    // TODO: 是否报错?
                    error = fill_result;
                    return new List<Reader>();
                }

                StringBuilder debugInfo = new StringBuilder();
                OpenReaderResult result = OpenReader(reader.DriverName,
                    reader.Type,
                    reader.SerialNumber,
                    "",
                    debugInfo);
                WriteInfoLog($"打开 USB 读卡器 {reader.SerialNumber} 返回={result.ToString()}。调试信息={debugInfo.ToString()}");
                reader.Result = result;
                reader.ReaderHandle = result.ReaderHandle;

                // 构造 Name
                // 重复的 ProductName 后面要加上序号
                {
                    int count = 0;
                    if (name_table.ContainsKey(reader.ProductName) == true)
                        count = (int)name_table[reader.ProductName];

                    if (count == 0)
                        reader.Name = reader.ProductName;
                    else
                        reader.Name = $"{reader.ProductName}({count + 1})";

                    name_table[reader.ProductName] = ++count;
                }
            }

            // 去掉填充信息阶段报错的那些 reader
            foreach (Reader reader in removed)
            {
                readers.Remove(reader);
            }

            //_readers = readers;
            //return new NormalResult();
            return readers;
        }

        static List<Reader> EnumTcpReader(string cfgFileName)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(cfgFileName);
            }
            catch (FileNotFoundException)
            {
                return new List<Reader>();
            }

            List<Reader> results = new List<Reader>();

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("reader");
            foreach (XmlElement reader in nodes)
            {

                string type = reader.GetAttribute("type");
                if (type != "NET")
                    continue;
                Reader new_reader = new Reader();
                new_reader.DriverName = reader.GetAttribute("driverName");
                new_reader.Type = "NET";
                new_reader.SerialNumber = reader.GetAttribute("ip") + ":" + reader.GetAttribute("port");
                results.Add(new_reader);

                // 如果在 reader 元素里面使用 name 属性，则会优先使用这个名字(而不是软件自动给出名字)
                if (reader.HasAttribute("name"))
                    new_reader.PreferName = reader.GetAttribute("name");
            }

            return results;
        }

        // 打开所有 TCP 读卡器
        // parameters:
        //      cfgFileName  XML 配置文件名
        static List<Reader> OpenTcpReaders(
            Hashtable name_table,
            string cfgFileName,
            out NormalResult error)
        {
            error = null;

            // 枚举所有的 TCP reader
            List<Reader> readers = EnumTcpReader(cfgFileName);

            List<Reader> removed = new List<Reader>();

            // name --> count
            // Hashtable table = new Hashtable();

            // 打开所有的 reader
            foreach (Reader reader in readers)
            {
                var fill_result = FillReaderInfo(reader, "");
                if (fill_result.Value == -1)
                {
#if NO
                    if (reader.Type == "COM")
                    {
                        removed.Add(reader);
                        continue;
                    }
                    return fill_result;
#endif
                    // TODO: 是否报错?
                    error = fill_result;
                    return new List<Reader>();
                }

                StringBuilder debugInfo = new StringBuilder();
                OpenReaderResult result = OpenReader(reader.DriverName,
                    reader.Type,
                    reader.SerialNumber,
                    "",
                    debugInfo);
                WriteInfoLog($"打开 TCP 读卡器 {reader.SerialNumber} 返回={result.ToString()}。调试信息={debugInfo.ToString()}");
                reader.Result = result;
                reader.ReaderHandle = result.ReaderHandle;

                // 构造 Name
                if (string.IsNullOrEmpty(reader.PreferName) == false)
                {
                    // 2020/9/12
                    // *** readers.xml 中主动给出了名字
                    reader.Name = reader.PreferName;
                    if (name_table.ContainsKey(reader.Name) == true)
                    {
                        // 发生冲突，给一个随机后缀
                        reader.Name = reader.PreferName + "_" + Guid.NewGuid().ToString();
                    }
                    name_table[reader.Name] = 0;
                }
                else
                {
                    // *** 软件自动给出名字
                    // 重复的 ProductName 后面要加上序号
                    int count = 0;
                    if (name_table.ContainsKey(reader.ProductName) == true)
                        count = (int)name_table[reader.ProductName];

                    if (count == 0)
                        reader.Name = reader.ProductName;
                    else
                        reader.Name = $"{reader.ProductName}({count + 1})";

                    name_table[reader.ProductName] = ++count;
                }
            }

            // 去掉填充信息阶段报错的那些 reader
            foreach (Reader reader in removed)
            {
                readers.Remove(reader);
            }

            //_readers = readers;
            //return new NormalResult();
            return readers;
        }

#if NO
        // 刷新读卡器打开状态
        public NormalResult RefreshAllReaders()
        {
            Lock();
            try
            {
#if NO
                GetDriversInfo();

                // 枚举当前所有的 reader
                List<Reader> current_readers = EnumUsbReader("RL8000");

                // 增加新的
                foreach (Reader reader in current_readers)
                {
                    if (_findReader(_readers, reader.Name) == null)
                    {
                        _readers.Add(reader);
                        // 打开 reader
                        OpenReaderResult result = OpenReader(reader.SerialNumber);
                        reader.Result = result;
                        reader.ReaderHandle = result.ReaderHandle;
                    }
                }

                // 和 _readers 对比。删除 _readers 中多余的
                for (int i = 0; i < _readers.Count; i++)
                {
                    Reader reader = _readers[i];
                    if (_findReader(current_readers, reader.Name) == null)
                    {
                        CloseReader(reader.ReaderHandle);
                        _readers.RemoveAt(i);
                        i--;
                    }
                }

#endif

                CloseAllReaders();

                GetDriversInfo();

                NormalResult result = OpenAllReaders(null, out List<HintInfo> output);
                if (result.Value == -1)
                    return result;
                return new NormalResult();
            }
            finally
            {
                Unlock();
            }
        }
#endif


#if NO
        static Reader _findReader(List<Reader> _readers, string serialNumber)
        {
            foreach (Reader reader in _readers)
            {
                if (reader.SerialNumber == serialNumber)
                    return reader;

            }
            return null;
        }
#endif

        NormalResult CloseAllReaders()
        {
            // 关闭所有的 reader
            foreach (Reader reader in _readers)
            {
                //if (reader.Result == null && reader.Result.Value != -1)
                //    CloseReader(reader.Result.ReaderHandle);
                CloseReader(reader.ReaderHandle);
            }

            return new NormalResult();
        }

#if NOT_USE
        // result.Value:
        //      -1
        //      0
        NormalResult GetReaderHandle(string reader_name,
            out UIntPtr handle,
            out string protocols)
        {
            protocols = "";
            handle = UIntPtr.Zero;
            // Lock();
            try
            {
                handle = GetReaderHandle(reader_name, out protocols);
                if (handle == UIntPtr.Zero)
                    return new NormalResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };
                return new NormalResult();
            }
            catch (Exception ex)
            {
                return new NormalResult { Value = -1, ErrorInfo = $"GetReaderHandle() 异常: {ExceptionUtil.GetDebugText(ex)}" };
            }
            finally
            {
                // Unlock();
            }
        }
#endif

        List<Reader> GetReadersByName(string reader_name)
        {
            List<Reader> results = new List<Reader>();
            foreach (Reader reader in _readers)
            {
                if (reader.ReaderHandle == UIntPtr.Zero)
                    continue;
                // if (reader_name == "*" || reader_name == reader.Name)
                if (Reader.MatchReaderName(reader_name, reader.Name, out string antenna_list))
                    results.Add(reader);
            }

            return results;
        }


        List<object> GetAllReaderHandle(string reader_name)
        {
            List<object> results = new List<object>();
            foreach (Reader reader in _readers)
            {
#if NO
                if (reader.Result.ReaderHandle == null)
                    continue;
                if (reader_name == "*" || reader_name == reader.Name)
                    results.Add(reader.Result.ReaderHandle);
#endif

                if (reader.ReaderHandle == UIntPtr.Zero)
                    continue;
                // if (reader_name == "*" || reader_name == reader.Name)
                if (Reader.MatchReaderName(reader_name, reader.Name, out string antenna_list))
                    results.Add(reader.ReaderHandle);
            }

            return results;
        }

#if NOUSE
        // 根据 reader 名字找到 reader_handle
        UIntPtr GetReaderHandle(string reader_name_param, out string protocols)
        {
            string reader_name = Reader.GetNamePart(reader_name_param);
            protocols = "";
            foreach (Reader reader in _readers)
            {
                if (reader.Name == reader_name)
                {
#if NO
                    if (reader.Result == null
                        || reader.Result?.ReaderHandle == null)
                        throw new Exception($"名为 {reader_name} 的读卡器尚未打开");
                    return reader.Result?.ReaderHandle;
#endif
                    protocols = reader.Protocols;
                    return reader.ReaderHandle;
                }
            }

            return UIntPtr.Zero;
        }

#endif

        public List<CReaderDriverInf> readerDriverInfoList = new List<CReaderDriverInf>();


        private void GetDriversInfo()
        {
            /* 
             *  Call required, when application load ,this API just only need to load once
             *  Load all reader driver dll from drivers directory, like "rfidlib_ANRD201.dll"  
             */
            string path = "\\x86\\Drivers";
            if (IntPtr.Size == 8)
                path = "\\x64\\Drivers";
            int ret = RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers(
                path
                );

            /*
             * Not call required,it can be Omitted in your own appliation
             * enum and show loaded reader driver 
             */
            readerDriverInfoList.Clear();
            UInt32 nCount;
            nCount = RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverCount();
            uint i;
            for (i = 0; i < nCount; i++)
            {
                UInt32 nSize;
                CReaderDriverInf driver = new CReaderDriverInf();
                StringBuilder strCatalog = new StringBuilder();
                strCatalog.Append('\0', 64);

                nSize = (UInt32)strCatalog.Capacity;
                RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_CATALOG, strCatalog, ref nSize);
                driver.m_catalog = strCatalog.ToString();
                if (driver.m_catalog == RFIDLIB.rfidlib_def.RDRDVR_TYPE_READER) // Only reader we need
                {
                    StringBuilder strName = new StringBuilder();
                    strName.Append('\0', 64);
                    nSize = (UInt32)strName.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_NAME, strName, ref nSize);
                    driver.m_name = strName.ToString();

                    StringBuilder strProductType = new StringBuilder();
                    strProductType.Append('\0', 64);
                    nSize = (UInt32)strProductType.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_ID, strProductType, ref nSize);
                    driver.m_productType = strProductType.ToString();

                    StringBuilder strCommSupported = new StringBuilder();
                    strCommSupported.Append('\0', 64);
                    nSize = (UInt32)strCommSupported.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_COMMTYPESUPPORTED, strCommSupported, ref nSize);
                    driver.m_commTypeSupported = (UInt32)int.Parse(strCommSupported.ToString());

                    readerDriverInfoList.Add(driver);
                }
            }
        }

        // 枚举所有 USB 读卡器
        // parameters:
        //      driver_name 例如 "M201" "RL8000"
        private static List<Reader> EnumUsbReader(string driver_name)
        {
            List<Reader> readers = new List<Reader>();
            //CReaderDriverInf driver = (CReaderDriverInf)readerDriverInfoList[comboBox6.SelectedIndex];

            //if ((driver.m_commTypeSupported & RFIDLIB.rfidlib_def.COMMTYPE_USB_EN) > 0)
            {
                UInt32 nCount = RFIDLIB.rfidlib_reader.HID_Enum(driver_name);
                int iret;
                int i;
                for (i = 0; i < nCount; i++)
                {
                    StringBuilder sernum = new StringBuilder();
                    sernum.Append('\0', 64);
                    UInt32 nSize1;
                    nSize1 = (UInt32)sernum.Capacity;
                    iret = RFIDLIB.rfidlib_reader.HID_GetEnumItem(
                        (UInt32)i,
                        RFIDLIB.rfidlib_def.HID_ENUM_INF_TYPE_SERIALNUM,
                        sernum,
                        ref nSize1);
                    if (iret != 0)
                        continue;

                    string driver_path = "";
                    {
                        StringBuilder path = new StringBuilder();
                        path.Append('\0', 64);
                        UInt32 nSize2;
                        nSize2 = (UInt32)path.Capacity;
                        iret = RFIDLIB.rfidlib_reader.HID_GetEnumItem(
                            (UInt32)i,
                            RFIDLIB.rfidlib_def.HID_ENUM_INF_TYPE_DRIVERPATH,
                            path,
                            ref nSize2);
                        if (iret == 0)
                        {
                            driver_path = path.ToString();
                        }
                        else
                            continue;
                    }

                    Reader reader = new Reader
                    {
                        Type = "USB",
                        SerialNumber = sernum.ToString(),
                        // Name = sernum.ToString(),
                        DriverPath = driver_path
                    };
                    readers.Add(reader);
                }
            }

            return readers;
        }

        public static void WriteDebugLog(string text)
        {
            Log.Logger.Debug(text);
        }

        public static void WriteErrorLog(string text)
        {
            Log.Logger.Error(text);
        }

        public static void WriteInfoLog(string text)
        {
            Log.Logger.Information(text);
        }

        // 枚举所有 COM 读卡器
        private static List<Reader> EnumComReader(string driver_name)
        {
            List<Reader> readers = new List<Reader>();
            //CReaderDriverInf driver = (CReaderDriverInf)readerDriverInfoList[comboBox6.SelectedIndex];

            //if ((driver.m_commTypeSupported & RFIDLIB.rfidlib_def.COMMTYPE_USB_EN) > 0)
            {
                UInt32 nCOMCnt = RFIDLIB.rfidlib_reader.COMPort_Enum();

                WriteDebugLog($"COMPort_Enum() return [{nCOMCnt}]");

                for (uint i = 0; i < nCOMCnt; i++)
                {
                    StringBuilder comName = new StringBuilder();
                    comName.Append('\0', 64);
                    RFIDLIB.rfidlib_reader.COMPort_GetEnumItem(i, comName, (UInt32)comName.Capacity);
                    // comName);

                    WriteDebugLog($"COMPort_Enum() {i}: comName=[{comName.ToString()}]");

                    Reader reader = new Reader
                    {
                        Type = "COM",
                        SerialNumber = comName.ToString(),
                    };
                    readers.Add(reader);
                }
            }

            return readers;
        }


        #region XML

        static string product_xml = @"
<all_device>

  <!--RL8600-->
  <device product='RL8600'>
    <basic>
      <id>118001</id>
      <driver>118000</driver>
      <type>reader</type>
      <description>RL8000</description>
      <picture>RL8600.jpg</picture>
      <range>short</range>
      <min_antenna_id>0</min_antenna_id>
      <antena_count>1</antena_count>
      <communication com='true' usb='true' tcp_ip='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true' ISO14443A='true' ISO14443B='true' ISO18000P3M3='true' ST_ISO14443B='true' Sony_Felica='true' NFC_Forum_Type1='true'/>
    </protocol>
    <upgrade Enable='true' MCU='NuMicro' EnableTransparent='false'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
        <load_key>true</load_key>
        <reset_sys>true</reset_sys>
      </command>
      <multiple_tags/>
      <single_tag>
        <Transceive>
          <ISO15693_Transceive Multiple_Antenna='false'/>
          <ISO14443Ap4_Transceive/>
        </Transceive>
      </single_tag>
      <nfc_operation/>
    </function>
  </device>

  <!--RPAN(HF)-->
  <device product='R-PAN ISO15693'>
    <basic>
      <id>200001</id>
      <driver>200001</driver>
      <type>r_pan</type>
      <communication com='true' usb='true' tcp_ip='true' bluetooth='true'/>
      <description>r_pan</description>
      <picture>RPAN.jpg</picture>
      <range>short</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <upgrade Enable='true' EnableTransparent='false' Driver='118000' WaitTime='10000'></upgrade>
    <function>
      <configuration>
        <save_block>false</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_sys_time>true</set_sys_time>
        <erase_flash>true</erase_flash>
      </command>
      <multiple_tags/>
      <single_tag/>
      <buffer_mode/>
    </function>

  </device>

  <!--RD503-->
  <device product='RD503'>
    <basic>
      <id>000007</id>
      <driver>000007</driver>
      <type>reader</type>
      <communication com='true' tcp_ip='true'/>
      <picture>rd503.jpg</picture>
      <range>long</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer On-board</name>
          <name>Relay#1</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
      <noise_detect/>
    </function>

  </device>

  <!--G302-->
  <device product='G302'>
    <basic>
      <id>685422</id>
      <driver>685422</driver>
      <type>mt_gate</type>
      <communication com='true' tcp_ip='true'/>
      <picture>g302.jpg</picture>
      <range>long</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
    </basic>

    <protocol>
      <HF ISO15693='true'/>
    </protocol>

    <function>
      <configuration>
        <save_block>false</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer On-board</name>
          <name>Output#1</name>
          <name>Output#2</name>
          <name>Output#3</name>
          <name>Output#4</name>
        </set_output>
      </command>
      <channel_mode/>
      <noise_detect/>
    </function>

  </device>

  <!--LSG406-->
  <device product='LSG406'>
    <basic>
      <id>474026</id>
      <driver>474026</driver>
      <type>lsg_gate</type>
      <communication tcp_ip='true'/>
      <picture>lsg406.jpg</picture>
      <range>long</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Relay#1</name>
        </set_output>
        <flow_counter>true</flow_counter>
        <reset_counter>true</reset_counter>
        <reverse_direction>true</reverse_direction>
        <get_sys_time>true</get_sys_time>
        <set_sys_time>true</set_sys_time>
      </command>
      <flow_detect/>
      <noise_detect/>
    </function>
  </device>

  <!--LSG606-->
  <device product='LSG606'>
    <basic>
      <id>120001</id>
      <driver>120001</driver>
      <type>lsg_gate</type>
      <communication tcp_ip='true'/>
      <picture>lsg406.jpg</picture>
      <range>long</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
    </basic>
    <protocol>
      <UHF ISO18000P6C='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Relay#1</name>
        </set_output>
        <flow_counter>true</flow_counter>
        <reset_counter>true</reset_counter>
        <reverse_direction>true</reverse_direction>
        <get_sys_time>true</get_sys_time>
        <set_sys_time>true</set_sys_time>
      </command>
      <flow_detect/>
    </function>
  </device>


  <!--M103R-->
  <device product='M103R'>
    <basic>
      <id>690103</id>
      <driver>690103</driver>
      <type>reader</type>
      <picture>m103r.jpg</picture>
      <range>short</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
      <communication com='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>

    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
    </function>

  </device>

  <!--M201-->
  <device product='M201'>
    <basic>
      <id>690201</id>
      <driver>690201</driver>
      <type>reader</type>
      <picture>m201.jpg</picture>
      <range>middle</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
      <communication com='true' usb='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
      <noise_detect/>
    </function>
  </device>

  <!--M60-->
  <device product='M60'>
    <basic>
      <id>690600</id>
      <driver>690600</driver>
      <type>reader</type>
      <picture>M60.jpg</picture>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
      <antena_count>1</antena_count>
      <communication com='true' usb='true'/>
    </basic>
    <protocol>
      <UHF ISO18000P6C='true'/>
    </protocol>
    <upgrade Enable='true' MCU='STM32' EnableTransparent='false'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
      </command>
      <multiple_tags/>
      <single_tag/>
    </function>
  </device>

  <!--MR113-->
  <device product='MR113R'>
    <basic>
      <id>051103</id>
      <driver>051103</driver>
      <type>reader</type>
      <description>MR113R</description>
      <picture>mr113r.jpg</picture>
      <range>short</range>
      <min_antenna_id>0</min_antenna_id>
      <antena_count>1</antena_count>
      <communication com='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true' ISO14443A='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
    </function>

  </device>


  <!--RD100-->
  <device product='RD100'>
    <basic>
      <id>680100</id>
      <driver>680100</driver>
      <type>reader</type>
      <description>RD100</description>
      <picture>rd100.jpg</picture>
      <range>short</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
      <communication usb='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
    </function>

  </device>

  <!--RD120M-->
  <device product='RD120M'>
    <basic>
      <id>000010</id>
      <driver>000010</driver>
      <type>reader</type>
      <description>RD120M</description>
      <picture>rd120m.jpg</picture>
      <range>short</range>
      <min_antenna_id>0</min_antenna_id>
      <antena_count>1</antena_count>
      <communication usb='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true' ISO14443A='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
      <nfc_operation/>
    </function>

  </device>

  <!--RD131-->
  <device product='RD131'>
    <basic>
      <id>680131</id>
      <driver>680131</driver>
      <type>reader</type>
      <description>RD131</description>
      <picture>rd131.jpg</picture>
      <range>short</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
      <communication com='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
        <RF_Close>true</RF_Close>
        <led_display>true</led_display>
      </command>
      <multiple_tags/>
      <single_tag/>
    </function>

  </device>

  <!--RD201-->
  <device product='RD201'>
    <basic>
      <id>680201</id>
      <driver>680201</driver>
      <type>reader</type>
      <picture>rd201.jpg</picture>
      <range>middle</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
      <communication com='true' usb='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer</name>
          <name>Relay#1</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <single_tag/>
      <multiple_tags/>
      <noise_detect/>
    </function>

  </device>

  <!--RD242-->
  <device product='RD242'>
    <basic>
      <id>680242</id>
      <driver>680242</driver>
      <type>reader</type>
      <picture>rd242.jpg</picture>
      <range>middle</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>4</antena_count>
      <communication com='true' usb='true' tcp_ip='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <upgrade Enable='true' MCU='STM32' EnableTransparent='false'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer</name>
          <name>Relay#1</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
      <noise_detect/>
    </function>

  </device>


  <!--RD5112-->
  <device product='RD5112'>
    <basic>
      <id>000005</id>
      <driver>000005</driver>
      <type>reader</type>
      <picture>rd5112.jpg</picture>
      <range>long</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>12</antena_count>
      <communication com='true' tcp_ip='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer On-Board</name>
          <name>Relay#1</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
      <noise_detect/>
    </function>

  </device>


  <!--RD5100-->
  <device product='RD5100'>
    <basic>
      <id>680530</id>
      <driver>680530</driver>
      <type>reader</type>
      <picture>rd5100.jpg</picture>
      <cfg_name>RD5100</cfg_name>
      <range>long</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>30</antena_count>
      <cfg_antenna_count>30</cfg_antenna_count>
      <communication com='true' tcp_ip='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer On-Board</name>
          <name>Relay#1</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag>
        <Transceive>
          <ISO15693_Transceive Multiple_Antenna='false'/>
        </Transceive>
      </single_tag>
      <device_diagnosis>
        <antennas_check>true</antennas_check>
        <temperature_check>true</temperature_check>
        <error_check>true</error_check>
        <pa_current>true</pa_current>
        <noise_check>true</noise_check>
      </device_diagnosis>
    </function>
  </device>


  <!--RD122-->
  <device product='RD122'>
    <basic>
      <id>000004</id>
      <driver>000004</driver>
      <type>reader</type>
      <picture>rd122.jpg</picture>
      <range>short</range>
      <min_antenna_id>0</min_antenna_id>
      <antena_count>0</antena_count>
      <communication usb='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer On-Board</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
    </function>

  </device>


  <!--SSR100-->
  <device product='SSR100'>
    <basic>
      <id>000003</id>
      <driver>000003</driver>
      <type>reader</type>
      <picture>ssr100.jpg</picture>
      <range>long</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>12</antena_count>
      <communication com='true' tcp_ip='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer On-Board</name>
          <name>Relay#1 On-Board</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <noise_detect/>
      <multiple_tags/>
      <single_tag/>
    </function>
  </device>


  <!--RD543-->
  <device product='RD543'>
    <basic>
      <id>000006</id>
      <driver>000006</driver>
      <type>reader</type>
      <picture>rd543.jpg</picture>
      <range>long</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>4</antena_count>
      <communication com='true' tcp_ip='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer On-Board</name>
          <name>Relay#1</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
      <noise_detect/>
    </function>
  </device>


  <!--MF102U-->
  <device product='MF102U'>
    <basic>
      <id>011002</id>
      <driver>011002</driver>
      <type>reader</type>
      <description>MF102U</description>
      <picture>mf102u.jpg</picture>
      <range>short</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
      <communication usb='true'/>
    </basic>
    <protocol>
      <HF ISO14443A='true'/>
    </protocol>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
        <load_key>true</load_key>
      </command>
      <multiple_tags/>
      <single_tag/>
    </function>
  </device>

  <!--RL1500-->
  <device product='RL1500'>
    <basic>
      <id>111501</id>
      <driver>118000</driver>
      <type>reader</type>
      <description>RL1500</description>
      <picture>RL1500.jpg</picture>
      <range>short</range>
      <min_antenna_id>0</min_antenna_id>
      <antena_count>1</antena_count>
      <communication com='true' usb='true'/>
    </basic>
    <protocol>
      <HF ISO14443A='true'/>
    </protocol>
    <upgrade Enable='true' MCU='NuMicro' EnableTransparent='false'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
        <load_key>true</load_key>
      </command>
      <multiple_tags/>
      <single_tag/>
      <nfc_operation/>
    </function>

  </device>


  <!--RL1700-->
  <device product='RL1700'>
    <basic>
      <id>111701</id>
      <driver>118000</driver>
      <type>reader</type>
      <description>RL1700</description>
      <picture>RL1700.jpg</picture>
      <range>short</range>
      <min_antenna_id>0</min_antenna_id>
      <antena_count>1</antena_count>
      <communication com='true' usb='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <upgrade Enable='true' MCU='NuMicro' EnableTransparent='false'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Buzzer</name>
        </set_output>
        <RF_Open>true</RF_Open>
        <RF_Close>true</RF_Close>
      </command>
      <multiple_tags/>
      <single_tag/>
      <nfc_operation/>
    </function>

  </device>

  <!--R-PAN ILT-->
  <device product='R-PAN ILT'>
    <basic>
      <id>200003</id>
      <driver>200001</driver>
      <type>r_pan</type>
      <communication com='true' usb='true' tcp_ip='true' bluetooth='true'/>
      <description>r_pan</description>
      <picture>RPAN.jpg</picture>
      <range>short</range>
      <min_antenna_id>0</min_antenna_id>
      <antena_count>1</antena_count>
    </basic>
    <protocol>
      <HF ISO18000P3M3='true'/>
    </protocol>
    <upgrade Enable='true' EnableTransparent='false'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_sys_time>true</set_sys_time>
        <erase_flash>true</erase_flash>
      </command>
      <buffer_mode/>
    </function>

  </device>


  <!--  R-PAN UHF  -->
  <device product='R-PAN UHF'>
    <basic>
      <id>200002</id>
      <driver>200002</driver>
      <type>r_pan</type>
      <communication com='true' usb='true' tcp_ip='true' bluetooth='true'/>
      <description>R-PAN UHF</description>
      <picture>RPAN.jpg</picture>
      <range>short</range>
      <min_antenna_id>0</min_antenna_id>
      <antena_count>1</antena_count>
    </basic>

    <protocol>
      <UHF ISO18000P6C='true'/>
    </protocol>
    <upgrade Enable='true' EnableTransparent='false' Driver='118000' WaitTime='10000'></upgrade>
    <function>
      <configuration>
        <save_block>false</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_sys_time>true</set_sys_time>
        <erase_flash>true</erase_flash>
      </command>
      <multiple_tags/>
      <single_tag/>
      <buffer_mode/>
    </function>

  </device>

  <!--UM200-->
  <device product='UM200'>
    <basic>
      <id>691200</id>
      <driver>691200</driver>
      <type>reader</type>
      <communication com='true' usb='true'/>
      <description>UM200</description>
      <picture>um200.jpg</picture>
      <range>long</range>
      <min_antenna_id>0</min_antenna_id>
      <antena_count>0</antena_count>
    </basic>

    <protocol>
      <UHF ISO18000P6C='true'/>
    </protocol>
    <upgrade Enable='true' MCU='STM32'  EnableTransparent='false'></upgrade>
    
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <RF_Operation>true</RF_Operation>
      </command>
      <multiple_tags/>
      <single_tag/>
      <device_diagnosis>
        <antennas_check>true</antennas_check>
        <temperature_check>true</temperature_check>
      </device_diagnosis>
    </function>

  </device>

  <!--URD2004-->
  <device product='URD2004'>
    <basic>
      <id>690601</id>
      <driver>690600</driver>
      <type>reader</type>
      <communication com='true' usb='true'/>
      <description>URD2004</description>
      <range>long</range>
      <min_antenna_id>0</min_antenna_id>
      <antena_count>4</antena_count>
    </basic>

    <protocol>
      <UHF ISO18000P6C='true'/>
    </protocol>
    <upgrade Enable='true' MCU='STM32'  EnableTransparent='true'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output>
          <name>Port#1</name>
        </set_output>
      </command>
      <multiple_tags/>
      <single_tag/>
    </function>
  </device>

  <!--RD5200-->
  <device product='RD5200'>
    <basic>
      <id>690050</id>
      <driver>690050</driver>
      <type>reader</type>
      <picture>RD5200.jpg</picture>
      <noise>true</noise>
      <range>long</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>0</antena_count>
      <buffer_mode>false</buffer_mode>
      <save_block>true</save_block>
      <communication usb ='true' com='true' tcp_ip='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <upgrade Enable='true' MCU='STM32'  EnableTransparent='false'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output auto_detect='true'/>
        <input_status>true</input_status>
        <RF_Operation>true</RF_Operation>
        <check_mux>true</check_mux>
        <reset_sys>true</reset_sys>
      </command>
      <multiple_tags/>
      <single_tag>
        <Transceive>
          <ISO15693_Transceive Multiple_Antenna='true'/>
        </Transceive>
      </single_tag>
      <device_diagnosis>
        <antennas_check>true</antennas_check>
        <temperature_check>true</temperature_check>
        <error_check>true</error_check>
        <pa_current>true</pa_current>
        <noise_check>true</noise_check>
      </device_diagnosis>
    </function>
  </device>

  <!--RD2100 ??? -->
  <device product='RD2100'>
    <basic>
      <id>680701</id>
      <driver>680600</driver>
      <type>reader</type>
      <picture>RD2100.jpg</picture>
      <noise>true</noise>
      <range>middle</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>4</antena_count>
      <buffer_mode>false</buffer_mode>
      <save_block>true</save_block>
      <cfg_antenna auto_check='true' antenna_cnt='4'/>
      <communication usb ='true' com='true' tcp_ip='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <upgrade Enable='true' MCU='STM32'  EnableTransparent='false'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output enable='true'>
          <port id='1' name='RD2100_o1'/>
        </set_output>
        <input_status enable='true'>
          <port id='1' name='RD2100_i1'/>
        </input_status>
        <RF_Operation>true</RF_Operation>
        <check_mux>false</check_mux>
        <reset_sys>true</reset_sys>
      </command>
      <multiple_tags/>
      <single_tag>
        <Transceive>
          <ISO15693_Transceive Multiple_Antenna='true'/>
        </Transceive>
      </single_tag>
      <device_diagnosis>
        <temperature_check RF_Power='true' PA_Current='true'>true</temperature_check>
        <error_check>
          <DiagnosisFlg>
            <Content Bit='0' Des='RD2100_b0'/>
            <Content Bit='1' Des='RD2100_b1'/>
            <Content Bit='2' Des='RD2100_b2'/>
            <Content Bit='3' Des='RD2100_b3'/>
            <Content Bit='4' Des='RD2100_b4'/>
          </DiagnosisFlg>
        </error_check>
        <noise_check Get_Nosiebase='true'>true</noise_check>
      </device_diagnosis>
    </function>
  </device>


  <!--RD2100-->
  <device product='RD2100'>
    <basic>
      <id>680700</id>
      <driver>680600</driver>
      <type>reader</type>
      <picture>RD2100.jpg</picture>
      <noise>true</noise>
      <range>middle</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>4</antena_count>
      <buffer_mode>false</buffer_mode>
      <save_block>true</save_block>
      <cfg_antenna auto_check='true' antenna_cnt='4'/>
      <communication usb ='true' com='true' tcp_ip='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <upgrade Enable='true' MCU='STM32'  EnableTransparent='false'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output enable='true'>
          <port id='1' name='RD2100_o1'/>
        </set_output>
        <input_status enable='true'>
          <port id='1' name='RD2100_i1'/>
        </input_status>
        <RF_Operation>true</RF_Operation>
        <check_mux>false</check_mux>
        <reset_sys>true</reset_sys>
      </command>
      <multiple_tags/>
      <single_tag>
        <Transceive>
          <ISO15693_Transceive Multiple_Antenna='true'/>
        </Transceive>
      </single_tag>
      <device_diagnosis>
        <temperature_check RF_Power='true' PA_Current='true'>true</temperature_check>
        <error_check>
          <DiagnosisFlg>
            <Content Bit='0' Des='RD2100_b0'/>
            <Content Bit='1' Des='RD2100_b1'/>
            <Content Bit='2' Des='RD2100_b2'/>
            <Content Bit='3' Des='RD2100_b3'/>
            <Content Bit='4' Des='RD2100_b4'/>
          </DiagnosisFlg>
        </error_check>
        <noise_check Get_Nosiebase='true'>true</noise_check>
      </device_diagnosis>
    </function>
  </device>

  <!--M22-->
  <device product='M22'>
    <basic>
      <id>690022</id>
      <driver>690050</driver>
      <type>reader</type>
      <picture>M22.jpg</picture>
      <cfg_name>M22</cfg_name>
      <noise>true</noise>
      <range>middle</range>
      <min_antenna_id>1</min_antenna_id>
      <antena_count>1</antena_count>
      <buffer_mode>false</buffer_mode>
      <save_block>true</save_block>
      <communication usb ='true' com='true' tcp_ip='true'/>
    </basic>
    <protocol>
      <HF ISO15693='true'/>
    </protocol>
    <upgrade Enable='true' MCU='STM32'  EnableTransparent='false'></upgrade>
    <function>
      <configuration>
        <save_block>true</save_block>
      </configuration>
      <command>
        <information>true</information>
        <set_output auto_detect='true'/>
        <input_status>true</input_status>
        <RF_Operation>true</RF_Operation>
        <reset_sys>true</reset_sys>
      </command>
      <multiple_tags/>
      <single_tag>
        <Transceive>
          <ISO15693_Transceive Multiple_Antenna='true'/>
        </Transceive>
      </single_tag>
      <device_diagnosis>
        <temperature_check>true</temperature_check>
        <error_check check_flg='0x05FF'>true</error_check>
        <noise_check>true</noise_check>
      </device_diagnosis>
    </function>
  </device>

</all_device>";


        #endregion

        static XmlDocument _product_dom = null;

        static bool GetDriverName(string product_id,
            out string driver_name,
            out string product_name,
            out string protocols,
            out int antenna_count)
        {
            driver_name = "";
            product_name = "";
            protocols = "";
            antenna_count = 0;

            if (_product_dom == null)
            {
                _product_dom = new XmlDocument();
                _product_dom.LoadXml(product_xml);
            }

            // return _product_dom.DocumentElement.SelectSingleNode($"device[basic/id[text()='{product_id}']]/@product")?.Value;

            {
                XmlNode node = _product_dom.DocumentElement.SelectSingleNode($"device/basic/id[text()='{product_id}']/../driver/text()");
                if (node == null)
                    return false;
                // driver id
                driver_name = node.Value;
            }

            product_name = _product_dom.DocumentElement.SelectSingleNode($"device[basic/id[text()='{product_id}']]/@product")?.Value;

            {
                XmlElement hf = _product_dom.DocumentElement.SelectSingleNode($"device/basic/id[text()='{product_id}']/../../protocol/HF") as XmlElement;
                if (hf != null)
                {
                    List<string> list = new List<string>();
                    foreach (XmlAttribute attr in hf.Attributes)
                    {
                        if (attr.Value != "true")
                            continue;
                        list.Add(attr.Name);
                    }

                    protocols = StringUtil.MakePathList(list);
                }
            }

            // 2019/9/27
            {
                XmlElement count = _product_dom.DocumentElement.SelectSingleNode($"device/basic/id[text()='{product_id}']/../antena_count") as XmlElement;
                if (count != null)
                    Int32.TryParse(count.InnerText.Trim(), out antenna_count);
            }

            return true;
#if NO
            XmlNode node = _product_dom.DocumentElement.SelectSingleNode($"device/basic/id[text()='{product_id}']/../driver/text()");
            if (node == null)
                return null;

            string driver_id = node.Value;

            return _product_dom.DocumentElement.SelectSingleNode($"device[basic/id[text()='{driver_id}']]/@product")?.Value;
#endif
        }

        // 填充驱动类型和设备型号
        // parameters:
        //      baudRate    波特率。仅对 COM 口读卡器有效。空表示默认 38400
        static NormalResult FillReaderInfo(Reader reader, string baudRate)
        {
            StringBuilder debugInfo = new StringBuilder();
            var result = OpenReader(reader.DriverName,  // "",
                reader.Type,
                reader.SerialNumber,
                baudRate,
                debugInfo);
            WriteDebugLog($"FillReaderInfo() 中 OpenReader() return [{result.ToString()}] debugInfo={debugInfo?.ToString()}");

            try
            {
                int iret;
                /*
                 * Try to get  serial number and type from device
                 */
                StringBuilder devInfor = new StringBuilder();
                devInfor.Append('\0', 128);
                UInt32 nSize;
                nSize = (UInt32)devInfor.Capacity;
                iret = RFIDLIB.rfidlib_reader.RDR_GetReaderInfor(result.ReaderHandle, 0, devInfor, ref nSize);
                WriteDebugLog($"RDR_GetReaderInfor() return [{iret}]");
                if (iret != 0)
                {
                    WriteDebugLog("FillReaderInfo() 返回调主");
                    return new NormalResult { Value = -1, ErrorInfo = $"GetReaderInfo() error, iret=[{iret}], debugInfo={debugInfo.ToString()}" };
                }
                string dev_info = devInfor.ToString();
                string[] parts = dev_info.Split(new char[] { ';' });
                if (parts.Length < 3)
                {
                    WriteDebugLog("FillReaderInfo() 返回调主。(1)");
                    return new NormalResult { Value = -1, ErrorInfo = $"所得到的结果字符串 '{dev_info}' 格式不正确。应该为分号间隔的三段形态" };
                }

                string product_id = parts[1];

                bool bRet = GetDriverName(product_id,
                    out string driver_name,
                    out string product_name,
                    out string protocols,
                    out int antenna_count);
                if (bRet == false)
                {
                    WriteDebugLog($"FillReaderInfo() 返回调主。GetDriverName({product_id}) return false");
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"product_id {product_id} 在读卡器元数据中没有找到对应的 driver name",
                        ErrorCode = "driverNameNotFound"
                    };
                }

                reader.DriverName = driver_name;
                reader.ProductName = product_name;
                reader.Protocols = protocols;
                reader.AntannaCount = antenna_count;
                WriteDebugLog($"FillReaderInfo() 成功得到 Reader 信息。{reader.ToString()}");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                WriteErrorLog($"FillReaderInfo() exception[{ExceptionUtil.GetExceptionText(ex)}]");
                string error = $"FillReaderInfo() 出现异常: {ex.Message}";
                return new NormalResult { Value = -1, ErrorInfo = error };
            }
            finally
            {
                CloseReader(result.ReaderHandle);
            }
        }

        // parameters:
        //      comm_type   COM/USB/NET/BLUETOOTH 之一
        static string BuildConnectionString(string readerDriverName,
            string comm_type,
            string serial_number,
            string baudRate)
        {
            if (string.IsNullOrEmpty(readerDriverName))
            {
                readerDriverName = "M201";  // "RL8000";
                                            // readerDriverName = readerDriverInfoList[0].m_name;
            }

            if (string.IsNullOrEmpty(comm_type))
                comm_type = "USB";
#if NO
            string result = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
              RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + comm_type + ";" +
              "AddrMode=0";// ;SerNum=
#endif
            if (comm_type == "USB")
                return $"RDType={readerDriverName};CommType={comm_type};AddrMode=1;SerNum={serial_number}";
            else if (comm_type == "COM")
            {
                if (string.IsNullOrEmpty(baudRate))
                    baudRate = "38400";
                // TODO: BaudRate=38400;Frame=8E1;BusAddr=255 应该可以配置
                return $"RDType={readerDriverName};CommType={comm_type};COMName={serial_number};BaudRate={baudRate};Frame=8E1;BusAddr=255";// Frame=8E1 或者 8N1 8O1
            }
            else if (comm_type == "NET")
            {
                var parts = StringUtil.ParseTwoPart(serial_number, ":");
                string ip = parts[0];
                string port = parts[1];
                return $"RDType={readerDriverName};CommType={comm_type};RemoteIP={ip};RemotePort={port};LocalIP=";
            }
            else
                throw new ArgumentException($"未知的 comm_type [{comm_type}]");

#if NO
            int commTypeIdx = comboBox10.SelectedIndex;
            string connstr = "";
            // Build serial communication connection string
            if (commTypeIdx == 0)
            {
                connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + comm_type + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMNAME + "=" + comboBox1.Text + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMBARUD + "=" + comboBox14.Text + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMFRAME + "=" + comboBox15.Text + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_BUSADDR + "=" + "255";
            }
            // Build USBHID communication connection string
            else if (commTypeIdx == 1)
            {
                connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + comm_type + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDADDRMODE + "=" + usbOpenType.ToString() + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDSERNUM + "=" + comboBox9.Text;
            }
            // Build network communication connection string
            else if (commTypeIdx == 2)
            {
                string ipAddr;
                UInt16 port;
                ipAddr = textBox5.Text;
                port = (UInt16)int.Parse(textBox6.Text);
                connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_NET + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_REMOTEIP + "=" + ipAddr + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_REMOTEPORT + "=" + port.ToString() + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_LOCALIP + "=" + "";
            }
            // Build blueTooth communication connection string
            else if (commTypeIdx == 3)
            {
                if (txbBluetoothSN.Text == "")
                {
                    MessageBox.Show("The address of the bluetooth can not be null!");
                    return;
                }
                connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                         RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_BLUETOOTH + ";" +
                         RFIDLIB.rfidlib_def.CONNSTR_NAME_BLUETOOTH_SN + "=" + txbBluetoothSN.Text;
            }
#endif
        }

        static OpenReaderResult OpenReader(string driver_name,
            string type,
            string serial_number,
            string baudRate,
            StringBuilder debugInfo)
        {
            UIntPtr hreader = UIntPtr.Zero;
            string connection_string = BuildConnectionString(driver_name,
                type,
                serial_number,
                baudRate);
            if (debugInfo != null)
                debugInfo.Append($"driver_name=[{driver_name}],type=[{type}],serial_number=[{serial_number}],connect_string=[{connection_string}]");

            var iret = RFIDLIB.rfidlib_reader.RDR_Open(
                connection_string,
                ref hreader);
            if (iret != 0)
                return new OpenReaderResult
                {
                    Value = -1,
                    ErrorInfo = $"OpenReader error, return: {iret}",
                    ErrorCode = GetErrorCode(iret, hreader)
                };

            return new OpenReaderResult { ReaderHandle = hreader };
        }

        static NormalResult CloseReader(object reader_handle)
        {
            //Lock();
            try
            {
                RFIDLIB.rfidlib_reader.RDR_CloseRFTransmitter((UIntPtr)reader_handle);
                var iret = RFIDLIB.rfidlib_reader.RDR_Close((UIntPtr)reader_handle);
                if (iret != 0)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"CloseReader error, return: {iret}",
                        ErrorCode = GetErrorCode(iret, (UIntPtr)reader_handle)
                    };

                // 成功
                // hreader = (UIntPtr)0;
                return new NormalResult();
            }
            finally
            {
                //Unlock();
            }
        }


#if NO
        NormalResult LoadFactoryDefault(object reader_handle)
        {
            Lock();
            try
            {
                var iret = RFIDLIB.rfidlib_reader.RDR_LoadFactoryDefault((UIntPtr)reader_handle);
                if (iret != 0)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"LoadFactoryDefault error, return: {iret}",
                        ErrorCode = GetErrorCode(iret, (UIntPtr)reader_handle)
                    };

                // 成功
                return new NormalResult();
            }
            finally
            {
                Unlock();
            }
        }
#endif
        byte[] GetAntennaList(string list)
        {
            if (string.IsNullOrEmpty(list) == true)
                return new byte[] { 1 };    // 默认是一号天线
            string[] numbers = list.Split(new char[] { '|', ',' });
            List<byte> bytes = new List<byte>();
            foreach (string number in numbers)
            {
                bytes.Add(Convert.ToByte(number));
            }

            return bytes.ToArray();
        }

        NormalResult GetReader(string reader_name, out Reader reader)
        {
            reader = null;
            //2019/9/29
            if (reader_name == "*")
                return new NormalResult { Value = -1, ErrorInfo = "GetReader() 不应该用通配符的读卡器名" };
            var readers = GetReadersByName(reader_name);
            if (readers.Count == 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"没有找到名为 '{reader_name}' 的读卡器"
                };
            reader = readers[0];
            return new NormalResult();
        }

        // 兼容以前的用法
        public InventoryResult Inventory(string reader_name,
    string style)
        {
            return Inventory(reader_name,
    "",
    style);
        }

        // 对一个读卡器进行盘点
        // parameters:
        //      reader_name     一个读卡器名字。注意，不应包含多个读卡器名字
        //      antenna_list    内容为形态 "1|2|3|4"。如果为空，相当于 1。
        //      style   可由下列值组成
        //              only_new    每次只列出最新发现的那些标签(否则全部列出)
        // exception:
        //      可能会抛出 System.AccessViolationException 异常
        public InventoryResult Inventory(string reader_name,
            string antenna_list,
            string style)
        {
            NormalResult result = GetReader(reader_name,
out Reader reader);
            if (result.Value == -1)
                return new InventoryResult(result);

            // TODO: 这里要按照一个读卡器粒度来锁定就好了。因为带有天线的读卡器 inventory 操作速度较慢
            LockReader(reader);
            try
            {
                /*
                NormalResult result = GetReaderHandle(reader_name,
                    out UIntPtr hreader,
                    out string protocols);
                if (result.Value == -1)
                    return new InventoryResult(result);
                    */


                byte ai_type = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
                if (StringUtil.IsInList("only_new", style))
                    ai_type = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE;

                // 2019/9/24
                // 天线列表
                // 1|2|3|4 这样的形态
                // string antenna_list = StringUtil.GetParameterByPrefix(style, "antenna", ":");
                byte[] antennas = GetAntennaList(antenna_list);

                UInt32 nTagCount = 0;
                int ret = tag_inventory(
                    reader.ReaderHandle,
                    reader.Protocols,
                    ai_type,
                    (byte)antennas.Length,    // 1,
                    antennas,   // new Byte[] { 1 },
                    ref nTagCount,
                    out List<InventoryInfo> results);
                if (ret != 0)
                {
                    string error_code = GetErrorCode(ret, reader.ReaderHandle);
                    return new InventoryResult
                    {
                        Value = -1,
                        ErrorInfo = $"Inventory() error, errorCode={error_code}, ret={ret}, readerName={reader.Name}",
                        ErrorCode = error_code
                    };
                }

                Debug.Assert(nTagCount == results.Count);
                return new InventoryResult { Results = results };
            }
            catch (Exception ex)
            {
                return new InventoryResult
                {
                    Value = -1,
                    ErrorInfo = $"Inventory()出现异常:{ex.Message}",
                    ErrorCode = "exception"
                };
            }
            finally
            {
                UnlockReader(reader);
            }
        }

        // 是否为全 0
        static bool IsZero(byte[] uid)
        {
            foreach (byte b in uid)
            {
                if (b != 0)
                    return false;
            }

            return true;
        }

        // UIntPtr.Zero
        UIntPtr _connectTag(
            UIntPtr hreader,
            string UID,
            UInt32 tag_type = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID)
        {
            int iret;

            byte[] uid = null;
            if (string.IsNullOrEmpty(UID) == false)
                uid = Element.FromHexString(UID);
#if NO
            idx = comboBox2.SelectedIndex;
            if (idx == -1)
            {
                MessageBox.Show("please select address mode");
                return;
            }
            if (idx == 1 || idx == 2) // Addressed and select need uid 
            {
                if (comboBox3.Text == "")
                {
                    MessageBox.Show("please input a uid");
                    return;
                }
            }
#endif

#if NO
            //set tag type default is NXP icode sli 
            UInt32 tagType = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID;
            if (comboBox3.SelectedIndex != -1)
            {
                // if we get the tag type from inventory ,then input the identified tag type 
                tagType = (comboBox3.SelectedItem as tagInfo).m_tagType;
            }
#endif

            // set address mode 
            Byte addrMode = 1;  // (Byte)idx;
            if (uid == null || IsZero(uid))
            {
                addrMode = 0;   // none address mode
                tag_type = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID;
            }

            UIntPtr hTag = UIntPtr.Zero;
            // do connection
            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_Connect(
                hreader,
                tag_type,   // tagType, 
                addrMode,
                uid,
                ref hTag);
            if (iret == 0)
            {
                /* 
                * if select none address mode after inventory need to reset the tag first,because the tag is stay quiet now  
                * if the tag is in ready state ,do not need to call reset
                */
                if (addrMode == 0)
                {
                    iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_Reset(hreader, hTag);
                    if (iret != 0)
                    {
                        // MessageBox.Show("reset tag fail");
                        RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
                        return UIntPtr.Zero;
                    }
                }

                return hTag;
            }
            else
            {
                return UIntPtr.Zero;    // fail
            }
        }

        bool _disconnectTag(
            UIntPtr hreader,
            ref UIntPtr hTag)
        {
            int iret;

            // do disconnection
            iret = RFIDLIB.rfidlib_reader.RDR_TagDisconnect(hreader, hTag);
            if (iret == 0)
            {
                hTag = (UIntPtr)0;
                return true;
            }
            else
            {
                return false;
            }
        }

        // parameters:
        //      read_lock_status    是否要一并读取 lock 状态信息？
        ReadBlocksResult ReadBlocks(
            UIntPtr hreader,
            UIntPtr hTag,
            UInt32 blockAddr,
            UInt32 blockToRead,
            UInt32 block_size,
            bool read_lock_status)
        {
            int iret;
            UInt32 blocksRead = 0;
            UInt32 nSize;
            // Byte[] BlockBuffer = new Byte[Math.Max(40, blockToRead)];  // 40
            Byte[] BlockBuffer = new Byte[blockToRead * (block_size + (read_lock_status ? 1 : 0))];  // 40

            nSize = (UInt32)BlockBuffer.Length; // (UInt32)BlockBuffer.GetLength(0);
            UInt32 bytesRead = 0;
            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ReadMultiBlocks(
                hreader,
                hTag,
                read_lock_status ? (byte)1 : (byte)0,
                blockAddr,
                blockToRead,
                ref blocksRead,
                BlockBuffer,
                nSize,
                ref bytesRead);
            if (iret != 0)
            {
                return new ReadBlocksResult
                {
                    Value = -1,
                    ErrorInfo = "read blocks error",
                    ErrorCode = GetErrorCode(iret, hreader)
                };
            }

            if (read_lock_status == false)
            {
                // 检查读出字节数的合理性
                if ((bytesRead % block_size) != 0)
                    return new ReadBlocksResult
                    {
                        Value = -1,
                        ErrorInfo = $"实际读出的字节数 {bytesRead} 不是块尺寸 {block_size} 的整倍数",
                        ErrorCode = "bytesCountError"
                    };
                // 实际读出的块个数
                uint count = bytesRead / block_size;
                // 核验 blocksRead
                if (blocksRead != count)
                    return new ReadBlocksResult
                    {
                        Value = -1,
                        ErrorInfo = $"实际读出的块数 {blocksRead} 和实际读出的字节数 {bytesRead} 不符合",
                        ErrorCode = "blocksCountError"
                    };

                ReadBlocksResult result = new ReadBlocksResult
                {
                    Bytes = new byte[bytesRead],
                    LockStatus = null
                };
                Array.Copy(BlockBuffer, result.Bytes, bytesRead);
                return result;
            }
            else
            {
                // 检查读出字节数的合理性
                if ((bytesRead % (block_size + 1)) != 0)
                {
#if NO
                    // 2020/9/16 
                    // 调整过大的 bytesRead 值
                    uint adjusted_size = blockToRead * (block_size + 1);
                    if (bytesRead > adjusted_size)
                    {
                        WriteInfoLog($"读出的字节数 {bytesRead} 比预料的 {adjusted_size} ({blockToRead}个 block)要大。但做了调整，继续运行");
                        bytesRead = adjusted_size;
                    }
                    else
#endif
                    return new ReadBlocksResult
                    {
                        Value = -1,
                        ErrorInfo = $"实际读出的字节数 {bytesRead} 不是单元尺寸 {block_size + 1} 的整倍数(注意每个块后面跟随了一个 byte 的锁定信息。块尺寸为 {block_size})",
                        ErrorCode = "bytesCountError"
                    };
                }
                // 实际读出的块个数
                uint count = bytesRead / (block_size + 1);
                // 核验 blocksRead
                if (blocksRead != count)
                    return new ReadBlocksResult
                    {
                        Value = -1,
                        ErrorInfo = $"实际读出的块数 {blocksRead} 和实际读出的字节数 {bytesRead} 不符合(每个单元含有一个 byte 的锁定信息情形)",
                        ErrorCode = "blocksCountError"
                    };

                // BlockBuffer 中分离出 lock status byte
                List<byte> buffer = new List<byte>(BlockBuffer);
                StringBuilder status = new StringBuilder();
                for (int i = 0; i < blocksRead; i++)
                {
                    byte b = buffer[i * (int)block_size];
                    status.Append(b == 0 ? '.' : 'l');
                    buffer.RemoveAt(i * (int)block_size);
                }

                // 实际数据长度
                int data_length = (int)(count * block_size);
                // 截断最后多余的字节
                if (buffer.Count > data_length)
                {
                    buffer.RemoveRange(data_length, buffer.Count - data_length);
                    Debug.Assert(buffer.Count == data_length);
                }
                ReadBlocksResult result = new ReadBlocksResult
                {
                    Bytes = buffer.ToArray(),
                    LockStatus = status.ToString()
                };
                return result;
            }
        }

        // TODO: 中间某个读卡器出错，还要继续往后用其他读卡器探索读取？
        // TODO: 根据 PII 寻找标签。如果找到两个或者以上，并且它们 UID 不同，会报错
        // 注：PII 相同，UID 也相同，属于正常情况，这是因为多个读卡器都读到了同一个标签的缘故
        // parameters:
        //      reader_name 可以用通配符
        // return:
        //      result.Value    -1 出错
        //      result.Value    0   没有找到指定的标签
        //      result.Value    1   找到了。result.UID 和 result.ReaderName 里面有返回值
        // exception:
        //      可能会抛出 System.AccessViolationException 异常
        public FindTagResult FindTagByPII(
            string reader_name,
            string protocols,   // 2019/8/28
            string antenna_list,    // 2019/9/24
            string pii)
        {
#if NO
            List<object> handles = GetAllReaderHandle(reader_name);
            if (handles.Count == 0)
                return new FindTagResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };
#endif
            List<Reader> readers = GetReadersByName(reader_name);
            if (readers.Count == 0)
                return new FindTagResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };

            byte[] antennas = GetAntennaList(antenna_list);

            // 锁定所有读卡器?
            Lock();
            try
            {
                FindTagResult temp_result = null;

                foreach (Reader reader in readers)
                {
                    if (StringUtil.IsInList(reader.Protocols, protocols) == false)
                        continue;

                    // UIntPtr hreader = reader.ReaderHandle;
                    // 枚举所有标签
                    byte ai_type = RFIDLIB.rfidlib_def.AI_TYPE_NEW;

                    UInt32 nTagCount = 0;
                    int ret = tag_inventory(
                        reader.ReaderHandle,
                        protocols,  // reader.Protocols,
                        ai_type,
                        (byte)antennas.Length,    // 1,
                        antennas,   // new Byte[] { 1 },
                        ref nTagCount,
                        out List<InventoryInfo> results);
                    if (ret != 0)
                    {
                        temp_result = new FindTagResult
                        {
                            Value = -1,
                            ErrorInfo = "tag_inventory error",
                            ErrorCode = GetErrorCode(ret, reader.ReaderHandle)
                        };
                        continue;
                    }

                    Debug.Assert(nTagCount == results.Count);

                    foreach (InventoryInfo info in results)
                    {
                        // 选择天线
                        if (reader.AntannaCount > 1)
                        {
                            var hr = rfidlib_reader.RDR_SetAcessAntenna(reader.ReaderHandle,
                                (byte)info.AntennaID);
                            if (hr != 0)
                            {
                                return new FindTagResult
                                {
                                    Value = -1,
                                    ErrorInfo = $"1 RDR_SetAcessAntenna() error. hr:{hr},reader_name:{reader.Name},antenna_id:{info.AntennaID}",
                                    ErrorCode = GetErrorCode(hr, reader.ReaderHandle)
                                };
                            }
                        }

                        UIntPtr hTag = _connectTag(
    reader.ReaderHandle,
    info?.UID,
    info.TagType);
                        if (hTag == UIntPtr.Zero)
                        {
                            temp_result = new FindTagResult
                            {
                                Value = -1,
                                ErrorInfo = "connectTag Error"
                            };
                            continue;
                        }
                        try
                        {
                            int iret;
                            Byte[] uid = new Byte[8];
                            if (info != null && string.IsNullOrEmpty(info.UID) == false)
                                uid = Element.FromHexString(info.UID);

                            Byte dsfid, afi, icref;
                            UInt32 blkSize, blkNum;
                            dsfid = afi = icref = 0;
                            blkSize = blkNum = 0;
                            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_GetSystemInfo(
                                reader.ReaderHandle,
                                hTag,
                                uid,
                                ref dsfid,
                                ref afi,
                                ref blkSize,
                                ref blkNum,
                                ref icref);
                            if (iret != 0)
                            {
                                temp_result = new FindTagResult
                                {
                                    Value = -1,
                                    ErrorInfo = "ISO15693_GetSystemInfo() error 1",
                                    ErrorCode = GetErrorCode(iret, reader.ReaderHandle)
                                };
                                continue;
                            }

                            ReadBlocksResult result0 = ReadBlocks(
                                reader.ReaderHandle,
                        hTag,
                        0,
                        blkNum,
                        blkSize,
                        true);
                            if (result0.Value == -1)
                            {
                                temp_result = new FindTagResult
                                {
                                    Value = -1,
                                    ErrorInfo = result0.ErrorInfo,
                                    ErrorCode = result0.ErrorCode
                                };
                                continue;
                            }

                            // 解析出 PII
                            // Exception:
                            //      可能会抛出异常 ArgumentException TagDataException
                            LogicChip chip = LogicChip.From(result0.Bytes,
                                (int)blkSize);
                            string current_pii = chip.FindElement(ElementOID.PII)?.Text;
                            if (pii == current_pii)
                                return new FindTagResult
                                {
                                    Value = 1,
                                    ReaderName = reader.Name,   // 2019/8/28
                                    UID = info.UID
                                };
                        }
                        finally
                        {
                            _disconnectTag(reader.ReaderHandle, ref hTag);
                        }
                    }
                }

                // 如果中间曾出现过报错
                if (temp_result != null)
                    return temp_result;

                return new FindTagResult
                {
                    Value = 0,
                    ErrorInfo = $"没有找到 PII 为 {pii} 的标签",
                    ErrorCode = "tagNotFound"
                };
            }
            finally
            {
                Unlock();
            }
        }

        public NormalResult LoadFactoryDefault(string reader_name)
        {
            List<object> handles = GetAllReaderHandle(reader_name);
            if (handles.Count == 0)
                return new NormalResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };

            // 锁定很多读卡器
            Lock();
            try
            {
                foreach (UIntPtr reader_handle in handles)
                {
                    var iret = RFIDLIB.rfidlib_reader.RDR_LoadFactoryDefault(reader_handle);
                    if (iret != 0)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"LoadFactoryDefault error, return: {iret}",
                            ErrorCode = GetErrorCode(iret, reader_handle)
                        };
                }

                return new NormalResult();
            }
            finally
            {
                Unlock();
            }
        }

        // parameters:
        //      command 形态为 beep:-,mode:host,autoCloseRF:-
        public NormalResult SetConfig(string reader_name,
            string command)
        {
            List<Reader> readers = GetReadersByName(reader_name);
            if (readers.Count == 0)
                return new NormalResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };

            // 锁定所有读卡器
            Lock();
            try
            {
                Hashtable table = StringUtil.ParseParameters(command, ',', ':');

                foreach (Reader reader in readers)
                {
                    // UIntPtr reader_handle = reader.ReaderHandle;

                    foreach (string key in table.Keys)
                    {
                        string value = (string)table[key];

                        var result = ModifyConfig(reader,
    key,
    value);
                        if (result.Value == -1)
                            return result;
                    }
                }

                return new NormalResult();
            }
            finally
            {
                Unlock();
            }
        }

        NormalResult ModifyConfig(Reader reader,
            string key,
            string value)
        {
            uint cfg_no = 0;
            int index = 0;  // byte 位置
            int bit = 0;    // bit 位置。从低 bit 计算
            if (reader.ProductName == "RL1700"
                || reader.ProductName == "RL8600")
            {
                if (key == "beep")
                {
                    if (value != "+" && value != "-")
                        return new NormalResult { Value = -1, ErrorInfo = $"key '{key}' 的 value 部分 '{value}' 不合法。应为 + - 之一" };
                    cfg_no = 3;
                    index = 0;
                    bit = 1;
                }
                else if (key == "mode")
                {
                    if (value != "scan" && value != "host" && value != "buffer")
                        return new NormalResult { Value = -1, ErrorInfo = $"key '{key}' 的 value 部分 '{value}' 不合法。应为 host scan buffer 之一" };
                    cfg_no = 1;
                    index = 3;  // SM byte
                    bit = -1;   // 表示不用 bit，而使用整个 byte
                }
                else
                    return new NormalResult { Value = 0, ErrorInfo = $"读卡器型号 '{reader.ProductName}' 暂不支持 key '{key}'", ErrorCode = "notSupportKey" };
            }
            else if (reader.ProductName == "R-PAN ISO15693")
            {
                if (key == "mode")
                {
                    if (value != "scan" && value != "host")
                        return new NormalResult { Value = -1, ErrorInfo = $"key '{key}' 的 value 部分 '{value}' 不合法。应为 host scan 之一" };
                    cfg_no = 6;
                    index = 4;  // WM byte
                    bit = -1;   // 表示不用 bit，而使用整个 byte
                }
                else
                    return new NormalResult { Value = 0, ErrorInfo = $"读卡器型号 '{reader.ProductName}' 暂不支持 key '{key}'", ErrorCode = "notSupportKey" };
            }
            else if (reader.ProductName == "M201")
            {
                if (key == "autoCloseRF")
                {
                    if (value != "+" && value != "-")
                        return new NormalResult { Value = -1, ErrorInfo = $"key '{key}' 的 value 部分 '{value}' 不合法。应为 + - 之一" };
                    cfg_no = 3;
                    index = 0;
                    bit = 2;
                }
                else
                    return new NormalResult { Value = 0, ErrorInfo = $"读卡器型号 '{reader.ProductName}' 暂不支持 key '{key}'", ErrorCode = "notSupportKey" };
            }
            else
                return new NormalResult { Value = 0, ErrorInfo = $"暂不支持读卡器型号 '{reader.ProductName}'", ErrorCode = "notSupportReader" };

            byte[] buffer = new byte[16];
            var iret = RFIDLIB.rfidlib_reader.RDR_ConfigBlockRead(
                reader.ReaderHandle,
                cfg_no,
                buffer,
                16);
            if (iret != 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"ModifyConfig() read error, return: {iret}",
                    ErrorCode = GetErrorCode(iret, reader.ReaderHandle)
                };

            bool changed = false;
            // 修改
            if (key == "beep" || key == "autoCloseRF")
            {
                Debug.Assert(bit >= 0 && bit <= 7);

                byte old_value = buffer[index];

                if (value == "-")
                    buffer[index] = (byte)(buffer[index] & (0xff - (0x01 << bit)));
                else
                    buffer[index] = (byte)(buffer[index] | (0x01 << bit));

                if (old_value != buffer[index])
                    changed = true;
            }
            else if (key == "mode")
            {
                byte old_value = buffer[index];

                if (value == "host")    // 被动模式，接收主机命令才工作
                    buffer[index] = 0x00;
                else if (value == "scan")   // 主动模式，设备启动后自动开启扫描标签，扫描到标签主动发送数据
                    buffer[index] = 0x01;
                else if (value == "buffer") // 缓冲模式，设备启动后自动开启扫描标签，扫描到标签后缓冲，主机通过命令获取缓冲记录
                    buffer[index] = 0x02;

                if (old_value != buffer[index])
                    changed = true;
            }

            if (changed == false)
                return new NormalResult { Value = 0, ErrorInfo = "没有发生修改" };  // Value == 0 表示没有发生实际修改

            iret = RFIDLIB.rfidlib_reader.RDR_ConfigBlockWrite(
                reader.ReaderHandle,
                cfg_no,
                buffer,
                16,
                0xffff);
            if (iret != 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"ModifyConfig() write error, return: {iret}",
                    ErrorCode = GetErrorCode(iret, reader.ReaderHandle)
                };

            iret = RFIDLIB.rfidlib_reader.RDR_ConfigBlockSave(
    reader.ReaderHandle,
    cfg_no);
            if (iret != 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"ModifyConfig() save error, return: {iret}",
                    ErrorCode = GetErrorCode(iret, reader.ReaderHandle)
                };

            return new NormalResult { Value = 1 };
        }

        public ReadConfigResult ReadConfig(string reader_name, uint cfg_no)
        {
            List<Reader> readers = GetReadersByName(reader_name);
            if (readers.Count == 0)
                return new ReadConfigResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };

            // 锁定所有读卡器
            Lock();
            try
            {
                foreach (Reader reader in readers)
                {
                    var result = ReadConfig(reader, cfg_no);
                    if (result.Value == -1)
                        return result;
                    return result;  // 只返回第一个读卡器的信息
                }

                return new ReadConfigResult();
            }
            finally
            {
                Unlock();
            }
        }

        ReadConfigResult ReadConfig(Reader reader,
    uint cfg_no)
        {
            byte[] buffer = new byte[16];
            var iret = RFIDLIB.rfidlib_reader.RDR_ConfigBlockRead(
                reader.ReaderHandle,
                cfg_no,
                buffer,
                16);
            if (iret != 0)
                return new ReadConfigResult
                {
                    Value = -1,
                    ErrorInfo = $"ReadConfig() error, return: {iret}",
                    ErrorCode = GetErrorCode(iret, reader.ReaderHandle)
                };

            return new ReadConfigResult { Value = 1, Bytes = buffer, CfgNo = cfg_no };
        }

        // 设置 EAS 和 AFI
        // parameters:
        //      reader_name 读卡器名字。可以为 "*"，表示所有读卡器，此时会自动在多个读卡器上寻找 uid 符合的标签并进行修改
        //      style   处理风格。如果包含 "detect"，表示修改之前会先读出，如果没有必要修改则不会执行修改
        // return result.Value
        //      -1  出错
        //      0   成功
        public NormalResult SetEAS(
    string reader_name,
    string uid,
    uint antenna_id,
    bool enable,
    string style)
        {
            /*
            List<object> handles = GetAllReaderHandle(reader_name);
            if (handles.Count == 0)
                return new NormalResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };
                */
            var readers = GetReadersByName(reader_name);
            if (readers.Count == 0)
                return new NormalResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };

            // 锁定所有读卡器
            Lock();
            try
            {
                List<NormalResult> error_results = new List<NormalResult>();

                // foreach (UIntPtr hreader in handles)
                foreach (var reader in readers)
                {
                    // 选择天线
                    if (reader.AntannaCount > 1)
                    {
                        Debug.WriteLine($"antenna_id={antenna_id}");
                        var hr = rfidlib_reader.RDR_SetAcessAntenna(reader.ReaderHandle,
                            (byte)antenna_id);
                        if (hr != 0)
                        {
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"2 RDR_SetAcessAntenna() error. hr:{hr},reader_name:{reader.Name},antenna_id:{antenna_id}",
                                ErrorCode = GetErrorCode(hr, reader.ReaderHandle)
                            };
                        }
                    }

                    UInt32 tag_type = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID;
                    UIntPtr hTag = _connectTag(reader.ReaderHandle, uid, tag_type);
                    if (hTag == UIntPtr.Zero)
                        continue;
                    try
                    {
                        // 写入 AFI
                        {
                            NormalResult result0 = WriteAFI(reader.ReaderHandle,
                                hTag,
                                enable ? (byte)0x07 : (byte)0xc2);
                            if (result0.Value == -1)
                            {
                                error_results.Add(result0);
                                continue;
                            }
                        }

                        // 设置 EAS 状态
                        {
                            NormalResult result0 = EnableEAS(reader.ReaderHandle, hTag, enable);
                            if (result0.Value == -1)
                            {
                                error_results.Add(result0);
                                continue;
                            }
                        }

                        return new NormalResult();
                    }
                    finally
                    {
                        _disconnectTag(reader.ReaderHandle, ref hTag);
                    }
                }

                // 循环中曾经出现过报错
                if (error_results.Count > 0)
                    return error_results[0];

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"没有找到 UID 为 {uid} 的标签",
                    ErrorCode = "tagNotFound"
                };
            }
            finally
            {
                Unlock();
            }
        }

        // 给 byte [] 后面补足内容
        static bool EnsureBytes(TagInfo new_tag_info)
        {
            // 要确保 Bytes 包含全部 byte，避免以前标签的内容在保存后出现残留
            uint max_count = new_tag_info.BlockSize * new_tag_info.MaxBlockCount;

            // 2020/6/22
            if (new_tag_info.Bytes != null && new_tag_info.Bytes.Length > max_count)
            {
                throw new ArgumentException($"Bytes 中包含的字节数 {new_tag_info.Bytes.Length} 超过了 {new_tag_info.BlockSize}(BlockSize) 和 {new_tag_info.MaxBlockCount}(MaxBlockCount) 的乘积 {max_count}");
            }

            if (new_tag_info.Bytes != null && new_tag_info.Bytes.Length < max_count)
            {
                List<byte> bytes = new List<byte>(new_tag_info.Bytes);
                while (bytes.Count < max_count)
                {
                    bytes.Add(0);
                }

                new_tag_info.Bytes = bytes.ToArray();
                return true;
            }

            return false;
        }

        // parameters:
        //      one_reader_name 不能用通配符
        //      style   randomizeEasAfiPassword
        public NormalResult WriteTagInfo(// byte[] uid, UInt32 tag_type
            string one_reader_name,
            TagInfo old_tag_info,
            TagInfo new_tag_info //,
                                 // string style
            )
        {
            StringBuilder debugInfo = new StringBuilder();
            debugInfo.AppendLine($"WriteTagInfo() one_reader_name={one_reader_name}");
            debugInfo.AppendLine($"old_tag_info={old_tag_info.ToString()}");
            debugInfo.AppendLine($"new_tag_info={new_tag_info.ToString()}");
            WriteDebugLog(debugInfo.ToString());

            // 要确保 new_tag_info.Bytes 包含全部 byte，避免以前标签的内容在保存后出现残留
            EnsureBytes(new_tag_info);
            EnsureBytes(old_tag_info);

            NormalResult result = GetReader(one_reader_name,
    out Reader reader);
            if (result.Value == -1)
                return result;

            // 锁定一个读卡器
            LockReader(reader);
            try
            {
                /*
                NormalResult result = GetReaderHandle(reader_name, out UIntPtr hreader, out string protocols);
                if (result.Value == -1)
                    return result;
                    */


                // TODO: 选择天线
                // 2019/9/27
                // 选择天线
                if (reader.AntannaCount > 1)
                {
                    var hr = rfidlib_reader.RDR_SetAcessAntenna(reader.ReaderHandle,
                        (byte)old_tag_info.AntennaID);
                    if (hr != 0)
                    {
                        return new GetTagInfoResult
                        {
                            Value = -1,
                            ErrorInfo = $"3 RDR_SetAcessAntenna() error. hr:{hr},reader_name:{reader.Name},antenna_id:{old_tag_info.AntennaID}",
                            ErrorCode = GetErrorCode(hr, reader.ReaderHandle)
                        };
                    }
                }

                UInt32 tag_type = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID;
                UIntPtr hTag = _connectTag(reader.ReaderHandle, old_tag_info.UID, tag_type);
                if (hTag == UIntPtr.Zero)
                    return new NormalResult { Value = -1, ErrorInfo = "connectTag Error" };
                try
                {
                    // TODO: 如果是新标签，第一次执行修改密码命令

                    // *** 分段写入内容 bytes
                    if (new_tag_info.Bytes != null)
                    {
                        // 写入时候自动跳过锁定的块
                        List<BlockRange> new_ranges = BlockRange.GetBlockRanges(
                            (int)old_tag_info.BlockSize,
                            new_tag_info.Bytes,
                            old_tag_info.LockStatus,
                            'l');

                        // 检查要跳过的块，要对比新旧 bytes 是否完全一致。
                        // 不一致则说明数据修改过程有问题
                        {
                            List<BlockRange> compare_ranges = BlockRange.GetBlockRanges(
            (int)old_tag_info.BlockSize,
            old_tag_info.Bytes,
            old_tag_info.LockStatus,
            'l');
                            NormalResult result0 = CompareLockedBytes(
        compare_ranges,
        new_ranges);
                            if (result0.Value == -1)
                                return result0;
                        }

                        int current_block_count = 0;
                        foreach (BlockRange range in new_ranges)
                        {
                            if (range.Locked == false)
                            {
                                NormalResult result0 = WriteBlocks(
                                    reader.ReaderHandle,
                            hTag,
                            (uint)current_block_count,
                            (uint)range.BlockCount,
                            range.Bytes);
                                if (result0.Value == -1)
                                    return new NormalResult { Value = -1, ErrorInfo = result0.ErrorInfo, ErrorCode = result0.ErrorCode };
                            }

                            current_block_count += range.BlockCount;
                        }
                    }

                    // *** 兑现锁定 'w' 状态的块
                    if (new_tag_info.Bytes != null)
                    {
                        List<BlockRange> ranges = BlockRange.GetBlockRanges(
                            (int)old_tag_info.BlockSize,
                            new_tag_info.Bytes, // TODO: 研究一下此参数其实应该允许为 null
                            new_tag_info.LockStatus,
                            'w');

                        // 检查，原来的 'l' 状态的块，不应后来被当作 'w' 再次锁定
                        string error_info = CheckNewlyLockStatus(old_tag_info.LockStatus,
        new_tag_info.LockStatus);
                        if (string.IsNullOrEmpty(error_info) == false)
                            return new NormalResult { Value = -1, ErrorInfo = error_info, ErrorCode = "checkTwoLockStatusError" };

                        int current_block_count = 0;
                        foreach (BlockRange range in ranges)
                        {
                            if (range.Locked == true)
                            {
                                string error_code = LockBlocks(
                                    reader.ReaderHandle,
                                    hTag,
                                    (uint)current_block_count,
                                    (uint)range.BlockCount);
                                if (string.IsNullOrEmpty(error_code) == false)
                                    return new NormalResult { Value = -1, ErrorInfo = "LockBlocks error", ErrorCode = error_code };
                            }

                            current_block_count += range.BlockCount;
                        }
                    }

                    // 写入 DSFID
                    if (old_tag_info.DSFID != new_tag_info.DSFID)
                    {
                        NormalResult result0 = WriteDSFID(reader.ReaderHandle, hTag, new_tag_info.DSFID);
                        if (result0.Value == -1)
                            return result0;
                    }

                    // 写入 AFI
                    if (old_tag_info.AFI != new_tag_info.AFI)
                    {
                        NormalResult result0 = WriteAFI(reader.ReaderHandle, hTag, new_tag_info.AFI);
                        if (result0.Value == -1)
                            return result0;
                    }

                    // 设置 EAS 状态
                    if (old_tag_info.EAS != new_tag_info.EAS)
                    {
                        NormalResult result0 = EnableEAS(reader.ReaderHandle, hTag, new_tag_info.EAS);
                        if (result0.Value == -1)
                            return result0;
                    }

                    return new NormalResult();
                }
                finally
                {
                    _disconnectTag(reader.ReaderHandle, ref hTag);
                }
            }
            finally
            {
                UnlockReader(reader);
            }
        }

        // return:
        //      null 或者 "" 表示没有发现错误
        //      其他  返回错误描述文字
        static string CheckNewlyLockStatus(string existing_lock_status,
            string newly_lock_status)
        {
            int length = Math.Max(existing_lock_status.Length, newly_lock_status.Length);
            for (int i = 0; i < length; i++)
            {
                bool old_locked = BlockRange.GetLocked(existing_lock_status, i, 'l');
                bool new_locked = BlockRange.GetLocked(newly_lock_status, i, 'l');
                if (old_locked != new_locked)
                    return $"偏移{i} 位置 old_locked({old_locked}) 和 new_locked({new_locked}) 不一致";
                bool will_lock = BlockRange.GetLocked(newly_lock_status, i, 'w');
                if (old_locked == true && will_lock == true)
                    return $"偏移{i} 位置 old_locked({old_locked}) 和 will_lock({will_lock}) 不应同时为 true";
            }

            return null;
        }

        // 比较两套 range 中的锁定状态 bytes 是否一致
        static NormalResult CompareLockedBytes(
            List<BlockRange> ranges1,
            List<BlockRange> ranges2)
        {
            List<BlockRange> result1 = new List<BlockRange>();
            foreach (BlockRange range in ranges1)
            {
                if (range.Locked)
                    result1.Add(range);
            }

            List<BlockRange> result2 = new List<BlockRange>();
            foreach (BlockRange range in ranges2)
            {
                if (range.Locked)
                    result2.Add(range);
            }

            if (result1.Count != result2.Count)
            {
                return new NormalResult { Value = -1, ErrorInfo = $"两边的锁定区间数目不一致({result1.Count}和{result2.Count})" };
            }

            for (int i = 0; i < result1.Count; i++)
            {
                if (result1[i].Bytes.SequenceEqual(result2[i].Bytes) == false)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"新旧两套锁定范围 bytes 内容不一致。index={i}, {Element.GetHexString(result1[i].Bytes)}和{Element.GetHexString(result2[i].Bytes)}"
                    };
            }

            return new NormalResult();
        }

        // 写入 DSFID 位
        NormalResult WriteDSFID(
            UIntPtr hreader,
            UIntPtr hTag,
            byte dsfid)
        {
            int iret;
            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_WriteDSFID(hreader, hTag, dsfid);
            if (iret != 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "WriteDSFID error",
                    ErrorCode = GetErrorCode(iret, hreader)
                };
            return new NormalResult();
        }

        NormalResult WriteAFI(
            UIntPtr hreader,
            UIntPtr hTag,
            byte afi)
        {
            int iret;
            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_WriteAFI(
                hreader,
                hTag,
                afi);
            if (iret != 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "WriteAFI error",
                    ErrorCode = GetErrorCode(iret, hreader)
                };
            return new NormalResult();
        }

        class ReadAfiResult : NormalResult
        {
            public byte AFI { get; set; }
        }

        ReadAfiResult ReadAFI(
            UIntPtr hreader,
            UIntPtr hTag,
            string uid_string)
        {
            int iret;
            Byte[] uid = new Byte[8];
            uid = Element.FromHexString(uid_string);

            Byte dsfid, afi, icref;
            UInt32 blkSize, blkNum;
            dsfid = afi = icref = 0;
            blkSize = blkNum = 0;
            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_GetSystemInfo(
                hreader,
                hTag,
                uid,
                ref dsfid,
                ref afi,
                ref blkSize,
                ref blkNum,
                ref icref);
            if (iret != 0)
                return new ReadAfiResult
                {
                    Value = -1,
                    ErrorInfo = $"ISO15693_GetSystemInfo() error 3. iret:{iret},uid:{Element.GetHexString(uid)}",
                    ErrorCode = GetErrorCode(iret, hreader)
                };

            return new ReadAfiResult { AFI = afi };
        }

        // TODO: 最好让函数可以适应标签不支持 EAS 的情况
        // 检查 EAS 状态
        // return:
        //      result.Value 为 1 表示 On；为 0 表示 Off
        //      result.Value 为 -1 表示出错
        NormalResult CheckEAS(UIntPtr hreader,
            UIntPtr hTag)
        {
            int iret;
            Byte EASStatus = 0;
            iret = RFIDLIB.rfidlib_aip_iso15693.NXPICODESLI_EASCheck(hreader, hTag, ref EASStatus);
            if (iret != 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "CheckEAS error",
                    ErrorCode = GetErrorCode(iret, hreader)
                };

            return new NormalResult { Value = (EASStatus == 0 ? 0 : 1) };
        }

        NormalResult EnableEAS(UIntPtr hreader,
            UIntPtr hTag,
            bool bEnable)
        {
            int iret;
            if (bEnable)
                iret = RFIDLIB.rfidlib_aip_iso15693.NXPICODESLI_EableEAS(
                    hreader,
                    hTag);
            else
                iret = RFIDLIB.rfidlib_aip_iso15693.NXPICODESLI_DisableEAS(
                    hreader,
                    hTag);
            if (iret != 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = (bEnable ? "Enable" : "Disable") + "EAS error",
                    ErrorCode = GetErrorCode(iret, hreader)
                };
            return new NormalResult();
        }

        // parameters:
        //      numOfBlks   块数。等于 data.Length / 块大小
        NormalResult WriteBlocks(
            UIntPtr hreader,
            UIntPtr hTag,
            UInt32 blkAddr,
            UInt32 numOfBlks,
            byte[] data)
        {
            int iret;

            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_WriteMultipleBlocks(
                hreader,
                hTag,
                blkAddr,
                numOfBlks,
                data,
                (uint)data.Length);
            if (iret != 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "Write blocks error",
                    ErrorCode = GetErrorCode(iret, hreader)
                };
            return new NormalResult();
        }

        public NormalResult TestInitialReader()
        {
            _readers.Clear();
            _readers.Add(new Reader { Name = "test" });
            return new NormalResult();
        }

        public NormalResult TestCall(string style)
        {
            /*
            NormalResult result = GetReader(one_reader_name,
    out Reader reader);
            if (result.Value == -1)
                return new GetTagInfoResult(result);
                */
            Reader reader = _readers[0];

            string sleepString = StringUtil.GetParameterByPrefix(style, "sleep", ":");
            Int32.TryParse(sleepString, out int sleepValue);

            string timeoutString = StringUtil.GetParameterByPrefix(style, "timeout", ":");
            Int32.TryParse(timeoutString, out int timeoutValue);

            // 锁定一个读卡器
            LockReader(reader, timeoutValue);
            try
            {
                Thread.Sleep(sleepValue);

                return new NormalResult();
            }
            finally
            {
                UnlockReader(reader);
            }
        }

        // parameters:
        //      one_reader_name 不能用通配符
        //      tag_type    如果 uid 为空，则 tag_type 应为 RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID
        // result.Value
        //      -1
        //      0
        public GetTagInfoResult GetTagInfo(// byte[] uid, UInt32 tag_type
            string one_reader_name,
            InventoryInfo info)
        {
            NormalResult result = GetReader(one_reader_name,
    out Reader reader);
            if (result.Value == -1)
                return new GetTagInfoResult(result);

            // 锁定一个读卡器
            LockReader(reader);
            try
            {
                /*
                NormalResult result = GetReaderHandle(reader_name, out UIntPtr hreader, out string protocols);
                if (result.Value == -1)
                    return new GetTagInfoResult(result);
                    */


#if DEBUG
                if (info != null)
                {
                    Debug.Assert(info.UID.Length >= 8 || info.UID.Length == 0);
                }
#endif
                // 2019/9/27
                // 选择天线
                int antenna_id = -1;    // -1 表示尚未使用
                if (info != null && reader.AntannaCount > 1)
                {
                    antenna_id = (int)info.AntennaID;
                    var hr = rfidlib_reader.RDR_SetAcessAntenna(reader.ReaderHandle,
                        (byte)info.AntennaID);
                    if (hr != 0)
                    {
                        return new GetTagInfoResult
                        {
                            Value = -1,
                            ErrorInfo = $"4 RDR_SetAcessAntenna() error. hr:{hr},reader_name:{reader.Name},antenna_id:{info.AntennaID}",
                            ErrorCode = GetErrorCode(hr, reader.ReaderHandle)
                        };
                    }
                }

                // 2019/11/20
                if (info != null && info.UID == "00000000")
                    return new GetTagInfoResult();

                UIntPtr hTag = _connectTag(
                    reader.ReaderHandle,
                    info?.UID,
                    info == null ? RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID : info.TagType);
                if (hTag == UIntPtr.Zero)
                    return new GetTagInfoResult { Value = -1, ErrorInfo = "connectTag Error" };
                try
                {
                    int iret;
                    Byte[] uid = new Byte[8];
                    if (info != null && string.IsNullOrEmpty(info.UID) == false)
                    {
                        uid = Element.FromHexString(info.UID);
                        //Debug.Assert(info.UID.Length >= 8);
                        //Array.Copy(info.UID, uid, uid.Length);
                    }

                    Byte dsfid, afi, icref;
                    UInt32 blkSize, blkNum;
                    dsfid = afi = icref = 0;
                    blkSize = blkNum = 0;
                    iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_GetSystemInfo(
                        reader.ReaderHandle,
                        hTag,
                        uid,
                        ref dsfid,
                        ref afi,
                        ref blkSize,
                        ref blkNum,
                        ref icref);
                    if (iret != 0)
                        return new GetTagInfoResult
                        {
                            Value = -1,
                            ErrorInfo = $"ISO15693_GetSystemInfo() error 2. iret:{iret},reader_name:{one_reader_name},uid:{Element.GetHexString(uid)},antenna_id:{antenna_id}",
                            ErrorCode = GetErrorCode(iret, reader.ReaderHandle)
                        };

#if NO
                    byte[] block_status = GetLockStatus(
                        hTag,
                        0,
                        blkNum,
                        out string error_code);
                    if (block_status == null)
                        return new GetTagInfoResult { Value = -1, ErrorInfo = "GetLockStatus error", ErrorCode = error_code };
#endif

                    ReadBlocksResult result0 = ReadBlocks(
                        reader.ReaderHandle,
                        hTag,
                        0,
                        blkNum,
                        blkSize,
                        true);
                    if (result0.Value == -1)
                        return new GetTagInfoResult
                        {
                            Value = -1,
                            ErrorInfo = $"{result0.ErrorInfo},antenna_id:{antenna_id}",
                            ErrorCode = result0.ErrorCode
                        };

                    NormalResult eas_result = CheckEAS(reader.ReaderHandle, hTag);
                    if (eas_result.Value == -1)
                        return new GetTagInfoResult { Value = -1, ErrorInfo = eas_result.ErrorInfo, ErrorCode = eas_result.ErrorCode };

                    GetTagInfoResult result1 = new GetTagInfoResult
                    {
                        TagInfo = new TagInfo
                        {
                            // 这里返回真正 GetTagInfo 成功的那个 ReaderName。而 Inventory 可能返回类似 reader1,reader2 这样的字符串
                            ReaderName = one_reader_name,   // 2019/2/27

                            UID = Element.GetHexString(uid),
                            AFI = afi,
                            DSFID = dsfid,
                            BlockSize = blkSize,
                            MaxBlockCount = blkNum,
                            IcRef = icref,
                            LockStatus = result0.LockStatus,    // TagInfo.GetLockString(block_status),
                            Bytes = result0.Bytes,
                            EAS = eas_result.Value == 1,
                            // AntennaID = info == null ? 0 : info.AntennaID
                            // 2019/11/20
                            AntennaID = (uint)(antenna_id == -1 ? 0 : antenna_id),
                        }
                    };
                    return result1;
                }
                finally
                {
                    _disconnectTag(reader.ReaderHandle, ref hTag);
                    // 2019/11/18 尝试关闭射频
                    RFIDLIB.rfidlib_reader.RDR_CloseRFTransmitter(reader.ReaderHandle);
                }
            }
            finally
            {
                UnlockReader(reader);
            }
        }

        public NormalResult ManageReader(string reader_name, string command)
        {
            var readers = GetReadersByName(reader_name);
            if (readers.Count == 0)
                return new NormalResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };

            if (command == "CloseRFTransmitter")
            {
                foreach (var reader in readers)
                {
                    RFIDLIB.rfidlib_reader.RDR_CloseRFTransmitter(reader.ReaderHandle);
                }
                return new NormalResult();
            }

            return new NormalResult
            {
                Value = -1,
                ErrorInfo = $"未知的 command '{command}'"
            };
        }

        // 获得指定范围块的锁定状态
        // return:
        //      null    出错。错误码在 error_code 中返回
        //      返回锁定状态。每个 byte 表示一个 block 的锁定状态。0x00 表示没有锁定，0x01 表示已经锁定
        byte[] GetLockStatus(
            UIntPtr hreader,
            UIntPtr hTag,
            UInt32 blockAddr,
            UInt32 blockToRead,
            out string error_code)
        {
            error_code = "";
            int iret;

#if NO
            idx = comboBox4.SelectedIndex;
            if (idx < 0)
            {
                MessageBox.Show("please select block address");
                return;
            }
#endif
            Byte[] buffer = new Byte[blockToRead];
            UInt32 nSize = (UInt32)buffer.Length;   // (UInt32)buffer.GetLength(0);
            UInt32 bytesRead = 0;
            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_GetBlockSecStatus(
                hreader,
                hTag,
                blockAddr,
                blockToRead,
                buffer,
                nSize,
                ref bytesRead);
            if (iret == 0)
                return buffer;

            error_code = iret.ToString();
            return null;    // fail
        }

        // return:
        //      null 或者 ""  表示成功
        //      其他  错误码
        string LockBlocks(
            UIntPtr hreader,
            UIntPtr hTag,
            UInt32 blkAddr,
            UInt32 numOfBlks)
        {
            int iret;

            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_LockMultipleBlocks(
                hreader,
                hTag,
                blkAddr,
                numOfBlks);
            if (iret == 0)
                return "";
            else
                return iret.ToString();
        }

        // 盘点
        // TODO: 对于 M201 RL8600 RL1700 等只有一个天线的读卡器，要处理好不指定天线编号的和指定天线编号的两类盘点情况。目前无论如何都能返回盘点结果的做法是不好的，会对前端造成额外的过滤负担
        // parameters:
        //      AIType  RFIDLIB.rfidlib_def.AI_TYPE_NEW / RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE
        //      AntinnaSel  从 1 开始？
        // exception:
        //      可能会抛出 System.AccessViolationException 异常
        public int tag_inventory(
            UIntPtr hreader,
            string protocols,
            Byte AIType,
            Byte AntennaSelCount,
            Byte[] AntennaSel,
            ref UInt32 nTagCount,
            out List<InventoryInfo> results)
        {
            try
            {
                results = new List<InventoryInfo>();

                Byte enableAFI = 0;
                int iret;
                UIntPtr InvenParamSpecList = RFIDLIB.rfidlib_reader.RDR_CreateInvenParamSpecList();
                if (InvenParamSpecList.ToUInt64() != 0)
                {
                    RFIDLIB.rfidlib_aip_iso15693.ISO15693_CreateInvenParam(
                        InvenParamSpecList,
                        0,
                        enableAFI,
                        0x00,   // AFI, 打算要匹配的 AFI byte 值
                        0);

                    if (StringUtil.IsInList("ISO14443A", protocols))
                        RFIDLIB.rfidlib_aip_iso14443A.ISO14443A_CreateInvenParam(InvenParamSpecList, 0);
                }
                nTagCount = 0;
            LABEL_TAG_INVENTORY:
                // 可能会抛出 System.AccessViolationException 异常
                iret = RFIDLIB.rfidlib_reader.RDR_TagInventory(hreader,
                    AIType,
                    AntennaSelCount,
                    AntennaSel,
                    InvenParamSpecList);
                RFIDLIB.rfidlib_reader.RDR_CloseRFTransmitter(hreader);
                if (iret == 0 || iret == -21)
                {
                    nTagCount += RFIDLIB.rfidlib_reader.RDR_GetTagDataReportCount(hreader);
                    UIntPtr TagDataReport;
                    TagDataReport = (UIntPtr)0;
                    TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_FIRST); //first
                    while (TagDataReport.ToUInt64() > 0)
                    {
                        UInt32 aip_id = 0;
                        UInt32 tag_id = 0;
                        UInt32 ant_id = 0;
                        Byte dsfid = 0;
                        // Byte uidlen = 0;
                        Byte[] uid = new Byte[8];  // 16

                        /* Parse iso15693 tag report */
                        {
                            iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ParseTagDataReport(TagDataReport,
                                ref aip_id,
                                ref tag_id,
                                ref ant_id,
                                ref dsfid,
                                uid);
                            if (iret == 0)
                            {
                                // uidlen = 8;
                                // object[] pList = { aip_id, tag_id, ant_id, uid, (int)uidlen };
                                //// Invoke(tagReportHandler, pList);
                                //tagReportHandler(hreader, aip_id, tag_id, ant_id, uid ,8);
                                InventoryInfo result = new InventoryInfo
                                {
                                    Protocol = InventoryInfo.ISO15693,
                                    AipID = aip_id,
                                    TagType = tag_id,
                                    AntennaID = ant_id,
                                    DsfID = dsfid,
                                    UID = Element.GetHexString(uid),
                                };
                                // Array.Copy(uid, result.UID, result.UID.Length);
                                results.Add(result);
                            }
                        }

                        /* Parse Iso14443A tag report */
                        if (StringUtil.IsInList("ISO14443A", protocols))
                        {
                            uid = new Byte[8];

                            Byte uidlen = 0;

                            iret = RFIDLIB.rfidlib_aip_iso14443A.ISO14443A_ParseTagDataReport(TagDataReport,
                                ref aip_id,
                                ref tag_id,
                                ref ant_id,
                                uid,
                                ref uidlen);
                            if (iret == 0)
                            {
                                // object[] pList = { aip_id, tag_id, ant_id, uid, (int)uidlen };
                                // Invoke(tagReportHandler, pList);
                                //tagReportHandler(hreader, aip_id, tag_id, ant_id, uid, uidlen);

                                {
                                    Debug.Assert(uidlen >= 4);
                                    byte[] temp = new byte[uidlen];
                                    Array.Copy(uid, temp, uidlen);
                                    uid = temp;
                                }

                                InventoryInfo result = new InventoryInfo
                                {
                                    Protocol = InventoryInfo.ISO14443A,
                                    AipID = aip_id,
                                    TagType = tag_id,
                                    AntennaID = ant_id,
                                    DsfID = dsfid,
                                    UID = Element.GetHexString(uid),
                                };
                                results.Add(result);
                            }
                        }

                        /* Get Next report from buffer */
                        TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //next
                    }
                    if (iret == -21) // stop trigger occur,need to inventory left tags
                    {
                        AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE;//use only-new-tag inventory 
                        goto LABEL_TAG_INVENTORY;
                    }
                    iret = 0;
                }
                if (InvenParamSpecList.ToUInt64() != 0)
                    RFIDLIB.rfidlib_reader.DNODE_Destroy(InvenParamSpecList);

                RFIDLIB.rfidlib_reader.RDR_ResetCommuImmeTimeout(hreader);
                return iret;
            }
            finally
            {
                // RFIDLIB.rfidlib_reader.RDR_CloseRFTransmitter(hreader);
            }
        }

        /*
    附录3. RFIDLIB API错误代码表
    错误代码	  描述
    0	无错误，表示API调用成功。
    -1	未知错误
    -2	与读卡器硬件的通信失败
    -3	API的传入参数有误
    -4	API的传入参数的值不支持，如参数值只能是0-5，如果传入6那么会返回该错误。
    -5	超时，发送到读卡器的命令，在设定时间内等不到数据返回。
    -6	API申请内存失败
    -7	功能未开启
    -8	保留
    -9	保留
    -10	保留
    -11	保留
    -12	读卡器返回的数据包长度有误
    -13	保留
    -14	保留
    -15	保留
    -16	保留
    -17	读卡器返回操作失败标识数据包，可用API
    RDR_GetReaderLastReturnError 获取该失败的错误代码。
    -18	保留
    -19	保留
    -20	保留
    -21	Inventory的停止触发器发生，举个例子：假设设定1秒为Inventory
    的最大读卡时间，如果在1秒钟内还没读完所有的标签，读卡器会终止Inventory，那么API会返回该错误告诉应用程序，可能还有标签没读完。
    -22	标签操作命令不支持
    -23	传入RDR_SetConfig或RDR_GetConfig的配置项不支持。
    -24	保留
    -25	TCP socket错误，API返回该错误表明TCP连接已断开。
    -26	应用层传入的缓冲区太小。
    -27	与读卡器返回的数据有误。
    0	No error
    -1	Unknown error
    -2	IO error
    -3	Parameter error
    -4	Parameter value error
    -5	Reader respond timeout
    -6	Memory allocation fail
    -7	Reserved
    -8	Reserved
    -9	Reserved
    -10	Reserved
    -11	Reserved
    -12	Invalid message size from reader
    -13	Reserved
    -14	Reserved
    -15	Reserved
    -16	Reserved
    -17	Error from reader, 
    can use “RDR_GetReaderLastReturnError” to get reader error code .
    -18	Reserved
    -19	Reserved
    -20	Reserved
    -21	Timeout stop trigger occur .
    -22	Invalid tag command
    -23	Invalid Configuration block No
    -24	Reserved
    -25	TCP socket error
    -26	Size of input buffer too small.
    -27	Reserved

         * */
        static string GetErrorCode(int value, UIntPtr hr)
        {
            switch (value)
            {
                case 0:
                    return "noError";
                case -1:
                    return "unknownError";
                case -2:
                    return "ioError";
                case -3:
                    return "parameterError";
                case -4:
                    return "parameterValueError";
                case -5:
                    return "readerRespondTimeout";
                case -6:
                    return "memoryAllocationFail";
                case -7:
                    return "functionNotOpen";
                case -12:
                    return "messageSizeError";
                case -17:
                    if (hr != UIntPtr.Zero)
                    {
                        int code = RFIDLIB.rfidlib_reader.RDR_GetReaderLastReturnError(hr);
                        return $"errorFromReader={code}";
                    }
                    else
                        return "errorFromReader";
                case -21:
                    return "timeoutStopTrigger";
                case -22:
                    return "invalidTagCommand";
                case -23:
                    return "invalidConfigBlockNo";
                case -25:
                    return "tcpSocketError";
                case -26:
                    return "bufferTooSmall";
                case -27:
                    return "dataError";
            }

            return value.ToString();
        }

        static byte GetPasswordID(string type)
        {
            type = type.ToLower();
            if (type == "read")
                return 0x01;
            if (type == "write")
                return 0x02;
            if (type == "private")
                return 0x04;
            if (type == "destroy")
                return 0x08;
            if (type == "eas/afi")
                return 0x10;

            throw new ArgumentException($"未知的 type 值 {type}");
        }

        // parameters:
        //      reader_name 读卡器名字。可以为 "*"，表示所有读卡器，此时会自动在多个读卡器上寻找 uid 符合的标签并进行修改
        //      type    为 read write private destroy eas/afi 之一
        // return result.Value
        //      -1  出错
        //      0   成功
        public NormalResult ChangePassword(
    string reader_name,
    string uid,
    string type,
    uint old_password,
    uint new_password)
        {
            byte pwdType = GetPasswordID(type);

            List<object> handles = GetAllReaderHandle(reader_name);
            if (handles.Count == 0)
                return new NormalResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };

            // 锁定所有读卡器
            Lock();
            try
            {
                List<NormalResult> error_results = new List<NormalResult>();

                foreach (UIntPtr hreader in handles)
                {
                    UInt32 tag_type = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID;
                    UIntPtr hTag = _connectTag(hreader, uid, tag_type);
                    if (hTag == UIntPtr.Zero)
                        continue;
                    try
                    {
                        int iret = RFIDLIB.rfidlib_aip_iso15693.NXPICODESLI_GetRandomAndSetPassword(
                            hreader, hTag, pwdType, old_password);
                        if (iret != 0)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = "Authenticate error",
                                ErrorCode = GetErrorCode(iret, hreader)
                            };

                        iret = RFIDLIB.rfidlib_aip_iso15693.NXPICODESLI_WritePassword(
                            hreader, hTag, pwdType, new_password);
                        if (iret != 0)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = "WritePassword error",
                                ErrorCode = GetErrorCode(iret, hreader)
                            };

                        return new NormalResult();
                    }
                    finally
                    {
                        _disconnectTag(hreader, ref hTag);
                    }
                }

                // 循环中曾经出现过报错
                if (error_results.Count > 0)
                    return error_results[0];

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"没有找到 UID 为 {uid} 的标签",
                    ErrorCode = "tagNotFound"
                };
            }
            finally
            {
                Unlock();
            }
        }


        #region ShelfLock

        void CloseAllLocks()
        {
            foreach (var shelfLock in this._shelfLocks)
            {
                DisconnectLock(shelfLock.LockHandle);
                shelfLock.LockHandle = UIntPtr.Zero;
            }
        }

        // parameters:
        //      lock_param  COM口列表。形如 COM1|COM2。若为空，表示自动探测所用的 COM 口
        NormalResult OpenAllLocks(string lock_param)
        {
            List<string> used_ports = new List<string>();
            // 找出已经被读卡器占用的 COM 口
            foreach (var reader in _readers)
            {
                if (reader.Type == "COM")
                    used_ports.Add(reader.SerialNumber);
            }

            // 枚举 COM 口，尝试打开
            List<string> ports = new List<string>();

            if (string.IsNullOrEmpty(lock_param))
            {
                UInt32 nCOMCnt = RFIDLIB.rfidlib_reader.COMPort_Enum();

                WriteDebugLog($"OpenAllLock() COMPort_Enum() return [{nCOMCnt}]");

                for (uint i = 0; i < nCOMCnt; i++)
                {
                    StringBuilder comName = new StringBuilder();
                    comName.Append('\0', 64);
                    RFIDLIB.rfidlib_reader.COMPort_GetEnumItem(i, comName, (UInt32)comName.Capacity);

                    WriteDebugLog($"OpenAllLock() COMPort_Enum() {i}: comName=[{comName.ToString()}]");

                    if (used_ports.IndexOf(comName.ToString()) != -1)
                    {
                        WriteDebugLog($"OpenAllLock() comName=[{comName.ToString()}] used by readers, skiped");
                        continue;
                    }

                    ports.Add(comName.ToString());
                }
            }
            else
            {
                ports = StringUtil.SplitList(lock_param, '|');
            }

            List<ShelfLock> locks = new List<ShelfLock>();
            foreach (string port in ports)
            {
                // 尝试打开
                var result = ConnectLock(port);
                if (result.Value != -1)
                {
                    locks.Add(result.ShelfLock);
                }
                else
                {
                    WriteDebugLog($"OpenAllLock() ConnectLock() comName=[{port}] failed, errorinfo={result.ErrorInfo}, errorcode={result.ErrorCode}");
                }
            }

            this._shelfLocks = locks;

            return new NormalResult();
        }

        // parameters:
        //      port  COM口名称。形如 COM1
        NormalResult InitialLamp(string port)
        {
            // 尝试打开
            var result = ConnectLamp(port);
            if (result.Value == -1)
            {
                WriteDebugLog($"ConnectLamp() comName=[{port}] failed, errorinfo={result.ErrorInfo}, errorcode={result.ErrorCode}");
                return result;
            }
            this._shelfLamp = result.ShelfLamp;
            return new NormalResult();
        }

        void FreeLamp()
        {
            if (this._shelfLamp != null)
            {
                DisconnectLamp(this._shelfLamp.LampHandle);
                this._shelfLamp = null;
            }
        }

        class ConnectLampResult : NormalResult
        {
            public ShelfLamp ShelfLamp { get; set; }
        }

        void DisconnectLamp(UIntPtr hLamp)
        {
            if (hLamp == UIntPtr.Zero)
                return;

            RFIDLIB.miniLib_Lock.Mini_Disconnect(hLamp);
        }

        ConnectLampResult ConnectLamp(string port)
        {
            UIntPtr hLamp = UIntPtr.Zero; //Electronic lock handle

            int ret = RFIDLIB.miniLib_Lock.Mini_Connect(port, 9600, "8N1", ref hLamp);
            if (ret != 0)
                return new ConnectLampResult
                {
                    Value = -1,
                    ErrorInfo = "Connect Lamp fail",
                    ErrorCode = ret.ToString()
                };

            return new ConnectLampResult
            {
                Value = 0,
                ShelfLamp = new ShelfLamp
                {
                    Name = port,
                    SerialNumber = port,
                    LampHandle = hLamp
                }
            };
        }

        class ConnectLockResult : NormalResult
        {
            public ShelfLock ShelfLock { get; set; }
        }

        void DisconnectLock(UIntPtr hElectronicLock)
        {
            if (hElectronicLock == UIntPtr.Zero)
                return;

            RFIDLIB.miniLib_Lock.Mini_Disconnect(hElectronicLock);
        }

        ConnectLockResult ConnectLock(string port)
        {
            UIntPtr hElectronicLock = UIntPtr.Zero; //Electronic lock handle

            int ret = RFIDLIB.miniLib_Lock.Mini_Connect(port, 9600, "8N1", ref hElectronicLock);
            if (ret != 0)
                return new ConnectLockResult
                {
                    Value = -1,
                    ErrorInfo = "Connect Lock fail",
                    ErrorCode = ret.ToString()
                };

            return new ConnectLockResult
            {
                Value = 0,
                ShelfLock = new ShelfLock
                {
                    Name = port,
                    LockHandle = hElectronicLock
                }
            };
        }

        #endregion

        List<ShelfLock> GetLocksByName(string lock_name)
        {
            List<ShelfLock> results = new List<ShelfLock>();
            foreach (ShelfLock current_lock in _shelfLocks)
            {
                if (current_lock.LockHandle == UIntPtr.Zero)
                    continue;
                if (Reader.MatchReaderName(lock_name, current_lock.Name, out string antenna_list))
                    results.Add(current_lock);
            }

            return results;
        }

        // 解析锁名称字符串以后得到的细部结构
        public class LockPath
        {
            public string LockName { get; set; }
            public List<string> CardNameList { get; set; }
            public List<string> NumberList { get; set; }

            public static LockPath Parse(string text)
            {
                LockPath result = new LockPath();

                result.LockName = "*";
                result.CardNameList = new List<string> { "1" };
                result.NumberList = new List<string> { "1" };

                string[] parts = text.Split(new char[] { '.' });

                if (parts.Length > 0)
                    result.LockName = parts[0];
                if (parts.Length > 1 && string.IsNullOrEmpty(parts[1]) == false)
                    result.CardNameList = StringUtil.SplitList(parts[1], '|');
                if (parts.Length > 2 && string.IsNullOrEmpty(parts[2]) == false)
                    result.NumberList = StringUtil.SplitList(parts[2], '|');

                return result;
            }
        }

        /*
        public static void ParseLockName(string text, 
            out string lockName,
    out string card,
    out string number)
        {
            lockName = "*";
            card = "1";
            number = "1";
            string[] parts = text.Split(new char[] { '.' });

            if (parts.Length > 0)
                lockName = parts[0];
            if (parts.Length > 1)
                card = parts[1];
            if (parts.Length > 2)
                number = parts[2];
        }
        */

        // 探测锁状态
        // parameters:
        // parameters:
        //      lockNameParam   为 "锁控板名字.卡编号.锁编号"。
        //                      其中卡编号部分可以是 "1" 也可以是 "1|2" 这样的形态
        //                      其中锁编号部分可以是 "1" 也可以是 "1|2|3|4" 这样的形态
        //                      如果缺乏卡编号和锁编号部分，缺乏的部分默认为 "1"
        public GetLockStateResult GetShelfLockState(string lockNameParam)
        {
            var path = LockPath.Parse(lockNameParam);

            /*
            ParseLockName(lockNameParam,
                out string lockName,
out string card,
out string number);
*/

            List<ShelfLock> locks = GetLocksByName(path.LockName);
            if (locks.Count == 0)
                return new GetLockStateResult
                {
                    Value = -1,
                    ErrorInfo = $"当前不存在名为 '{path.LockName}' 的门锁对象",
                    ErrorCode = "lockNotFound"
                };

            List<LockState> states = new List<LockState>();

            foreach (var current_lock in locks)
            {
                foreach (var card in path.CardNameList)
                {
                    foreach (var number in path.NumberList)
                    {
                        int addr = 1;
                        int index = 1;
                        if (string.IsNullOrEmpty(card) == false)
                            Int32.TryParse(card, out addr);
                        if (string.IsNullOrEmpty(number) == false)
                            Int32.TryParse(number, out index);

                        Byte sta = 0x00;
                        int iret = RFIDLIB.miniLib_Lock.Mini_GetDoorStatus(current_lock.LockHandle,
                            (Byte)addr, // 1,
                            (Byte)index,
                            ref sta);
                        if (iret != 0)
                            return new GetLockStateResult
                            {
                                Value = -1,
                                ErrorInfo = $"getDoorStatus error (lock name='{current_lock.Name}' index={index})"
                            };

                        states.Add(new LockState
                        {
                            // Path
                            Path = $"{current_lock.Name}.{addr}.{index}",
                            // 锁名字
                            Lock = current_lock.Name,
                            Board = addr,
                            Index = index,
                            State = (sta == 0x00 ? "open" : "close")
                        });
                    }
                }
            }

            return new GetLockStateResult
            {
                Value = 0,
                States = states
            };
        }


#if REMOVED
        // 探测锁状态
        // parameters:
        //      lockName    锁名字。如果为 * 表示所有的锁
        public GetLockStateResult GetShelfLockState(string lockNameParam)
        {
            ParseLockName(lockNameParam,
                out string lockName,
out string card,
out string number);

            int addr = 1;
            int index = 1;
            if (string.IsNullOrEmpty(card) == false)
                Int32.TryParse(card, out addr);
            if (string.IsNullOrEmpty(number) == false)
                Int32.TryParse(number, out index);

            List<ShelfLock> locks = GetLocksByName(lockName);
            if (locks.Count == 0)
                return new GetLockStateResult
                {
                    Value = -1,
                    ErrorInfo = $"当前不存在名为 '{lockName}' 的门锁对象",
                    ErrorCode = "lockNotFound"
                };

            List<LockState> states = new List<LockState>();

            foreach (var current_lock in locks)
            {
                Byte sta = 0x00;
                int iret = RFIDLIB.miniLib_Lock.Mini_GetDoorStatus(current_lock.LockHandle,
                    (Byte)addr, // 1,
                    (Byte)index,
                    ref sta);
                if (iret != 0)
                    return new GetLockStateResult
                    {
                        Value = -1,
                        ErrorInfo = $"getDoorStatus error (lock name='{current_lock.Name}' index={index})"
                    };

                states.Add(new LockState
                {
                    // Path
                    Path = $"{current_lock.Name}.{addr}.{index}",
                    // 锁名字
                    Lock = current_lock.Name,
                    Board = addr,
                    Index = index,
                    State = (sta == 0x00 ? "open" : "close")
                });
            }

            return new GetLockStateResult
            {
                Value = 0,
                States = states
            };
        }

#endif

        /*
        // 还原锁名字
        static string RestoreLockName(string lockName, string number)
        {
            if (string.IsNullOrEmpty(number))
                return lockName;
            return lockName + "." + number;
        }
        */

        // 开/关紫外灯
        // parameters:
        //      lampName    暂未使用
        //      action      turnOn/turnOff
        public NormalResult TurnSterilamp(string lampName, string action)
        {
            if (this._shelfLamp == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "当前没有紫外灯",
                    ErrorCode = "notFound"
                };

            if (action == "turnOn")
            {
                // 注意：SDK 函数本身就写反了
                int iret = RFIDLIB.miniLib_Lock.Mini_CloseSterilamp(this._shelfLamp.LampHandle);
                if (iret == 0)
                    return new NormalResult();
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"开灯失败 iret={iret}",
                        ErrorCode = iret.ToString()
                    };
            }
            else if (action == "turnOff")
            {
                // 注意：SDK 函数本身就写反了
                int iret = RFIDLIB.miniLib_Lock.Mini_OpenSterilamp(this._shelfLamp.LampHandle);
                if (iret == 0)
                    return new NormalResult();
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"关灯失败 iret={iret}",
                        ErrorCode = iret.ToString()
                    };
            }
            else
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"未知的 action={action}",
                    ErrorCode = "unknownAction"
                };
        }

        // 开/关书柜灯
        // parameters:
        //      lampName    暂未使用
        //      action      turnOn/turnOff
        public NormalResult TurnShelfLamp(string lampName, string action)
        {
            if (this._shelfLamp == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "当前没有书柜灯",
                    ErrorCode = "notFound"
                };

            if (action == "turnOn")
            {
                int iret = RFIDLIB.miniLib_Lock.Mini_OpenLight(this._shelfLamp.LampHandle);
                if (iret == 0)
                    return new NormalResult();
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"开灯失败 iret={iret}",
                        ErrorCode = iret.ToString()
                    };
            }
            else if (action == "turnOff")
            {
                int iret = RFIDLIB.miniLib_Lock.Mini_CloseLight(this._shelfLamp.LampHandle);
                if (iret == 0)
                    return new NormalResult();
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"关灯失败 iret={iret}",
                        ErrorCode = iret.ToString()
                    };
            }
            else
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"未知的 action={action}",
                    ErrorCode = "unknownAction"
                };
        }

        // 开门
        // parameters:
        //      lockNameParam   为 "锁控板名字.卡编号.锁编号"。
        //                      其中卡编号部分可以是 "1" 也可以是 "1|2" 这样的形态
        //                      其中锁编号部分可以是 "1" 也可以是 "1|2|3|4" 这样的形态
        //                      如果缺乏卡编号和锁编号部分，缺乏的部分默认为 "1"
        public NormalResult OpenShelfLock(string lockNameParam)
        {
            var path = LockPath.Parse(lockNameParam);

            /*
            ParseLockName(lockNameParam,
                out string lockName,
out string card,
out string number);
*/

            List<ShelfLock> locks = GetLocksByName(path.LockName);
            if (locks.Count == 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"当前不存在名为 '{path.LockName}' 的门锁对象",
                    ErrorCode = "lockNotFound"
                };

            List<LockState> states = new List<LockState>();

            int count = 0;
            foreach (var current_lock in locks)
            {
                foreach (var card in path.CardNameList)
                {
                    foreach (var number in path.NumberList)
                    {
                        int addr = 1;
                        int index = 1;
                        if (string.IsNullOrEmpty(card) == false)
                            Int32.TryParse(card, out addr);
                        if (string.IsNullOrEmpty(number) == false)
                            Int32.TryParse(number, out index);

                        int iret = RFIDLIB.miniLib_Lock.Mini_OpenDoor(current_lock.LockHandle,
                            (Byte)addr,   // 1,
                            (Byte)index);
                        if (iret != 0)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"openDoor error (lock name='{current_lock.Name}' index={index})"
                            };
                        count++;
                    }
                }
            }

            return new NormalResult
            {
                Value = count
            };
        }


#if REMOVED
        // 开门
        public NormalResult OpenShelfLock(string lockNameParam)
        {
            ParseLockName(lockNameParam,
                out string lockName,
out string card,
out string number);

            int addr = 1;
            int index = 1;
            if (string.IsNullOrEmpty(card) == false)
                Int32.TryParse(card, out addr);
            if (string.IsNullOrEmpty(number) == false)
                Int32.TryParse(number, out index);

            List<ShelfLock> locks = GetLocksByName(lockName);
            if (locks.Count == 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"当前不存在名为 '{lockName}' 的门锁对象",
                    ErrorCode = "lockNotFound"
                };

            List<LockState> states = new List<LockState>();

            int count = 0;
            foreach (var current_lock in locks)
            {
                int iret = RFIDLIB.miniLib_Lock.Mini_OpenDoor(current_lock.LockHandle,
                    (Byte)addr,   // 1,
                    (Byte)index);
                if (iret != 0)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"openDoor error (lock name='{current_lock.Name}' index={index})"
                    };
                count++;
            }

            return new NormalResult
            {
                Value = count
            };
        }

#endif
    }

    public class CReaderDriverInf
    {
        public string m_catalog;
        public string m_name;
        public string m_productType;
        public UInt32 m_commTypeSupported;
    }

    public class ReadConfigResult : NormalResult
    {
        public uint CfgNo { get; set; }
        public byte[] Bytes { get; set; }
    }

    /*
    /// <summary>
    /// Driver1 函数库全局参数
    /// </summary>
    public static class Driver1Manager
    {
        public static ILog Log { get; set; }
    }
    */

    public class ReaderException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="error"></param>
        /// <param name="strText"></param>
        public ReaderException(string strText)
            : base(strText)
        {
        }
    }
}
