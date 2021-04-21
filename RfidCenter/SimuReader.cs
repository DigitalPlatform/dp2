using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

using Serilog;

namespace RfidCenter
{
    /// <summary>
    /// 用于模拟 RFID 读卡器的类，以方便没有硬件的情况下进行开发调试
    /// </summary>
    public class SimuReader
    {
        #region locks

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

        #endregion
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
        // "RD5100(2) 680530",
        "RD2104 680701"
        };

        static string GetProductID(string name)
        {
            name = GetPureName(name);
            foreach (var s in product_id_table)
            {
                var parts = StringUtil.ParseTwoPart(s, " ");
                if (name == parts[0])
                    return parts[1];
            }

            return null;
        }

        // 取名字前的纯净部分。以非字母和数字的字符作为分割点
        static string GetPureName(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            StringBuilder result = new StringBuilder();
            foreach(char ch in text)
            {
                if (char.IsWhiteSpace(ch) ||
                    (char.IsDigit(ch) == false && char.IsLetter(ch) == false)
                    )
                    return result.ToString();
                result.Append(ch);
            }

            return result.ToString();
        }

        // 创建若干读卡器
        // parameters:
        //      names   读卡器名字列表
        public void Create(List<string> names)
        {
            _readers.Clear();
            _tags.Clear();
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
                reader.Result = new OpenReaderResult();
                _readers.Add(reader);
            }
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

        List<Reader> GetReadersByName(string reader_name)
        {
            List<Reader> results = new List<Reader>();
            foreach (Reader reader in _readers)
            {
                /*
                if (reader.ReaderHandle == UIntPtr.Zero)
                    continue;
                */
                // if (reader_name == "*" || reader_name == reader.Name)
                if (Reader.MatchReaderName(reader_name, reader.Name, out string antenna_list))
                    results.Add(reader);
            }

            return results;
        }

        // 对一个读卡器进行盘点
        // parameters:
        //      reader_name     一个读卡器名字。注意，不应包含多个读卡器名字
        //      antenna_list    内容为形态 "1|2|3|4"。如果为空，相当于 1(注: ListTags() API 当 readername_list 参数为 "*" 时，就会被当作天线列表为空)。
        //      style   可由下列值组成
        //              only_new    每次只列出最新发现的那些标签(否则全部列出)
        // exception:
        //      可能会抛出 System.AccessViolationException 异常
        public InventoryResult Inventory(string reader_name,
            string antenna_list,
            string style)
        {
            // Debug.Assert(false);

            NormalResult result = GetReader(reader_name,
out Reader reader);
            if (result.Value == -1)
                return new InventoryResult(result);

            // TODO: reader.AntannaCount 里面有天线数量，此外还需要知道天线编号的开始号，这样就可以枚举所有天线了

            // TODO: 这里要按照一个读卡器粒度来锁定就好了。因为带有天线的读卡器 inventory 操作速度较慢
            LockReader(reader);
            try
            {
                /*
                byte ai_type = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
                if (StringUtil.IsInList("only_new", style))
                    ai_type = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE;
                */

                // 2019/9/24
                // 天线列表
                // 1|2|3|4 这样的形态
                // string antenna_list = StringUtil.GetParameterByPrefix(style, "antenna", ":");
                byte[] antennas = GetAntennaList(antenna_list, reader);
                var results = Inventory(reader, antennas);
                return new InventoryResult { Results = results };
            }
            catch (Exception ex)
            {
                WriteErrorLog($"Inventory() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
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
            List<Reader> readers = GetReadersByName(reader_name);
            if (readers.Count == 0)
                return new FindTagResult { Value = -1, ErrorInfo = $"没有找到名为 {reader_name} 的读卡器" };

            // 锁定所有读卡器?
            Lock();
            try
            {
                FindTagResult temp_result = null;

                foreach (Reader reader in readers)
                {
                    if (StringUtil.IsInList(reader.Protocols, protocols) == false)
                        continue;

                    byte[] antennas = GetAntennaList(antenna_list, reader);

                    var results = GetTags(reader, antennas);
                    foreach (TagData tag in results)
                    {
                        // 解析出 PII
                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        LogicChip chip = LogicChip.From(tag.TagInfo.Bytes,
                            (int)tag.TagInfo.BlockSize);
                        string current_pii = chip.FindElement(ElementOID.PII)?.Text;
                        if (pii == current_pii)
                            return new FindTagResult
                            {
                                Value = 1,
                                ReaderName = reader.Name,
                                AntennaID = tag.InventoryInfo.AntennaID,    // 2020/12/14
                                UID = tag.TagInfo.UID
                            };
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


        static byte[] GetAntennaList(string list, Reader reader)
        {
            if (string.IsNullOrEmpty(list) == true)
            {
                // 2020/10/15
                // 列出全部天线编号
                return reader.GetAntennaList();
            }

            string[] numbers = list.Split(new char[] { '|', ',' });
            List<byte> bytes = new List<byte>();
            foreach (string number in numbers)
            {
                bytes.Add(Convert.ToByte(number));
            }

            return bytes.ToArray();
        }

        // parameters:
        //      one_reader_name 不能用通配符
        //      tag_type    如果 uid 为空，则 tag_type 应为 RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID
        // result.Value
        //      -1
        //      0
        public GetTagInfoResult GetTagInfo(// byte[] uid, UInt32 tag_type
            string one_reader_name,
            InventoryInfo info,
            string style = "")
        {
            NormalResult result = GetReader(one_reader_name,
    out Reader reader);
            if (result.Value == -1)
                return new GetTagInfoResult(result);

            bool quick = StringUtil.IsInList("quick", style);

            // 锁定一个读卡器
            LockReader(reader);
            try
            {
#if DEBUG
                if (info != null)
                {
                    Debug.Assert(info.UID.Length >= 8 || info.UID.Length == 0);
                }
#endif
                // 2019/9/27
                // 选择天线
                int antenna_id = -1;    // -1 表示尚未使用
                if (info != null && reader.AntennaCount > 1)
                {
                    antenna_id = (int)info.AntennaID;
                    /*
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
                    */
                }

                // 2019/11/20
                if (info != null && info.UID == "00000000")
                    return new GetTagInfoResult();

                var tagInfo = FindTagInfo(info?.UID, reader.Name);

                if (tagInfo == null)
                    return new GetTagInfoResult { Value = -1, ErrorInfo = "connectTag Error" };

#if NO
                UIntPtr hTag = _connectTag(
                    reader.ReaderHandle,
                    info?.UID,
                    info == null ? RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID : info.TagType);
                if (hTag == UIntPtr.Zero)
                    return new GetTagInfoResult { Value = -1, ErrorInfo = "connectTag Error" };
#endif

                try
                {

#if NO
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

                    NormalResult eas_result = new NormalResult();
                    if (quick == false)
                    {
                        eas_result = CheckEAS(reader.ReaderHandle, hTag);
                        if (eas_result.Value == -1)
                            return new GetTagInfoResult { Value = -1, ErrorInfo = eas_result.ErrorInfo, ErrorCode = eas_result.ErrorCode };
                    }

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

#endif
                    // 2020/11/24
                    // 模拟时间耗费。假定一个标签耗费 500 毫秒
                    Thread.Sleep(500);

                    return new GetTagInfoResult { TagInfo = tagInfo };
                }
                finally
                {
#if NO
                    _disconnectTag(reader.ReaderHandle, ref hTag);
                    if (quick == false)
                    {
                        // 2019/11/18 尝试关闭射频
                        RFIDLIB.rfidlib_reader.RDR_CloseRFTransmitter(reader.ReaderHandle);
                    }
#endif
                }
            }
            finally
            {
                UnlockReader(reader);
            }
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
                    /*
                    // 选择天线
                    if (reader.AntennaCount > 1)
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
                    */

                    var tag = FindTag(uid, reader.Name);
                    if (tag == null)
                        continue;

                    /*
                    UInt32 tag_type = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID;
                    UIntPtr hTag = _connectTag(reader.ReaderHandle, uid, tag_type);
                    if (hTag == UIntPtr.Zero)
                        continue;
                    */
                    try
                    {
                        // 写入 AFI
                        tag.TagInfo.AFI = enable ? (byte)0x07 : (byte)0xc2;

                        // 设置 EAS 状态
                        tag.TagInfo.EAS = enable;
                        return new NormalResult();
                    }
                    finally
                    {
                        // _disconnectTag(reader.ReaderHandle, ref hTag);
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
                // TODO: 选择天线
                // 2019/9/27
                // 选择天线
                if (reader.AntennaCount > 1)
                {
                    /*
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
                    */
                }

                var tag = FindTag(old_tag_info.UID, reader.Name);
                if (tag == null)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "connectTag Error"
                    };

                var tagInfo = tag.TagInfo;
                /*
                UInt32 tag_type = RFIDLIB.rfidlib_def.RFID_ISO15693_PICC_ICODE_SLI_ID;
                UIntPtr hTag = _connectTag(reader.ReaderHandle, old_tag_info.UID, tag_type);
                if (hTag == UIntPtr.Zero)
                    return new NormalResult { Value = -1, ErrorInfo = "connectTag Error" };
                */

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
                                NormalResult result0 = TagData.WriteBlocks(
                                    tagInfo,
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
                                string error_code = TagData.LockBlocks(
                                    tagInfo,
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
                        tagInfo.DSFID = new_tag_info.DSFID;
                        /*
                        NormalResult result0 = WriteDSFID(reader.ReaderHandle, hTag, new_tag_info.DSFID);
                        if (result0.Value == -1)
                            return result0;
                        */
                    }

                    // 写入 AFI
                    if (old_tag_info.AFI != new_tag_info.AFI)
                    {
                        tagInfo.AFI = new_tag_info.AFI;
                        /*
                        NormalResult result0 = WriteAFI(reader.ReaderHandle, hTag, new_tag_info.AFI);
                        if (result0.Value == -1)
                            return result0;
                        */
                    }

                    // 设置 EAS 状态
                    if (old_tag_info.EAS != new_tag_info.EAS)
                    {
                        tagInfo.EAS = new_tag_info.EAS;
                        /*
                        NormalResult result0 = EnableEAS(reader.ReaderHandle, hTag, new_tag_info.EAS);
                        if (result0.Value == -1)
                            return result0;
                        */
                    }

                    tag.RefreshInventoryInfo();
                    return new NormalResult();
                }
                finally
                {
                    // _disconnectTag(reader.ReaderHandle, ref hTag);
                }
            }
            finally
            {
                UnlockReader(reader);
            }
        }

        public NormalResult SimuTagInfo(string action,
    List<TagInfo> tags,
    string style)
        {
            // 设置标签。如果已经有相同 UID 的标签，则覆盖；如果没有，则新增加
            if (action == "setTag")
            {
                string protocol = StringUtil.GetParameterByPrefix(style, "protocol");
                if (string.IsNullOrEmpty(protocol))
                    protocol = InventoryInfo.ISO15693;

                foreach (var tag in tags)
                {
                    var data = this.Set(tag, protocol);
                }
            }

            // 移走标签
            if (action == "removeTag")
            {
                // 清除全部标签
                if (tags == null)
                {
                    _tags.Clear();
                }
                else
                {
                    foreach (var tag in tags)
                    {
                        var found = FindTag(tag.UID, null); // 无所谓在哪个读卡器上
                        if (found != null)
                        {
                            _tags.Remove(found);
                        }
                    }
                }
            }

            return new NormalResult();
        }

        #region

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


        #endregion


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

        /*
        // Reader --> TagCollection
        Hashtable _readerTable = new Hashtable();
        */

        // 所有模拟读卡器上的标签
        TagCollection _tags = new TagCollection();

        // 列出一个读卡器上的全部标签。也就是盘点(inventory)
        List<InventoryInfo> Inventory(Reader reader, byte[] antennas)
        {
            var results = new List<InventoryInfo>();
            foreach (var tag in _tags.GetList())
            {
                Debug.Assert(string.IsNullOrEmpty(tag.ReaderName) == false);
                if (tag.ReaderName == reader.Name)
                {
                    int index = Array.IndexOf<byte>(antennas, (byte)tag.InventoryInfo.AntennaID);
                    if (index == -1)
                    {
                        // Debug.Assert(false);
                        continue;
                    }
                    results.Add(tag.InventoryInfo);
                }
            }

            // 2020/11/24
            // 模拟时间耗费。假定 100 毫秒一个标签
            if (results.Count > 0)
            {
                Thread.Sleep(100 * results.Count);
            }
            return results;
        }


        // 列出一个读卡器上的全部标签。也就是盘点(inventory)
        List<TagData> GetTags(Reader reader, byte[] antennas)
        {
            var results = new List<TagData>();
            foreach (var tag in _tags.GetList())
            {
                if (tag.ReaderName == reader.Name)
                {
                    int index = Array.IndexOf(antennas, tag.InventoryInfo.AntennaID);
                    if (index == -1)
                        continue;
                    results.Add(tag);
                }
            }

            return results;
        }

        // parameters:
        //      readerName  读卡器名。如果为 null 表示匹配所有读卡器
        TagData FindTag(string uid, string readerName)
        {
            foreach (var tag in _tags.GetList())
            {
                if (tag.InventoryInfo.UID == uid
                    && (readerName == null || tag.ReaderName == readerName))
                {
                    Debug.Assert(tag.InventoryInfo.UID == tag.TagInfo.UID);
                    return tag;
                }
            }

            return null;
        }

        // 获得一个标签的 TagInfo
        TagInfo FindTagInfo(string uid, string readerName)
        {
            return FindTag(uid, readerName)?.TagInfo;
        }

        public TagData Set(TagInfo source,
            string protocol = InventoryInfo.ISO15693)
        {
            var tag = this.Build(source, protocol);
            var exist = FindTag(source.UID, null);
            if (exist == null)
                _tags.Add(tag);
            else
            {
                _tags.Remove(exist);
                _tags.Add(tag);
            }

            return tag;
        }

        // 根据前端发来的 tagInfo 构造一个 TagData 对象
        public TagData Build(TagInfo source,
            string protocol = InventoryInfo.ISO15693)
        {
            // Debug.Assert(false);    // testing

            if (this._readers.Count == 0)
                throw new Exception("当前没有任何(模拟的)读卡器，因此无法添加标签信息");

            var tag = TagData.Build(source, protocol);

            InventoryInfo inventory = tag.InventoryInfo;
            TagInfo tagInfo = tag.TagInfo;

            // 自动生成一些成员

            // inventory
            inventory.TagType = 0;  // ???

            inventory.AipID = 0;    // ???

            // tagInfo

            Reader reader = null;

            if (string.IsNullOrEmpty(tagInfo.ReaderName))
            {
                // 自动取协议合适的第一个读卡器？
                reader = _readers.Find((r) =>
                {
                    return StringUtil.IsInList(protocol, r.Protocols) == true;
                });
                if (reader == null)
                    throw new Exception($"没有找到满足 '{protocol}' 协议的读卡器");
            }
            else
            {
                var result = GetReader(tagInfo.ReaderName, out reader);
                if (result.Value == -1)
                    throw new Exception($"读卡器 '{tagInfo.ReaderName}' 没有找到");

                Debug.Assert(reader != null);
            }

            // tagInfo.ReaderName;  
            if (string.IsNullOrEmpty(tagInfo.ReaderName))
            {
                Debug.Assert(string.IsNullOrEmpty(reader.Name) == false);

                tagInfo.ReaderName = reader.Name;
            }

            // tagInfo.UID; // 自动发生
            // 8 bytes?
            if (string.IsNullOrEmpty(tagInfo.UID))
                tagInfo.UID = ByteArray.GetHexTimeStampString(Guid.NewGuid().ToByteArray());

            // public byte DSFID { get; set; }
            // public byte AFI { get; set; }
            // public byte IcRef { get; set; }

            // 每个块内包含的字节数
            tagInfo.BlockSize = 4; // 设置为默认值
                                   // 块最大总数
            tagInfo.MaxBlockCount = 28;  // 设置为默认值

            // public bool EAS { get; set; }

            // tagInfo.AntennaID;   // 检查是否符合范围。否则设置为第一个天线
            if (tagInfo.AntennaID < reader.AntennaStart
                || tagInfo.AntennaID >= reader.AntennaStart + reader.AntennaCount)
                tagInfo.AntennaID = (uint)reader.AntennaStart;

            // 锁定状态字符串。表达每个块的锁定状态
            // 例如 "ll....lll"。'l' 表示锁定，'.' 表示没有锁定。缺省为 '.'。空字符串表示全部块都没有被锁定
            // LockStatus { get; set; }

            // 芯片全部内容字节
            // Bytes { get; set; }

            tag.RefreshInventoryInfo();
            return tag;
        }


        /*
        // 获得一个标签的 TagInfo
        TagInfo FindTagInfo(string uid, string readerName)
        {
            foreach (var tag in _tags)
            {
                if (tag.InventoryInfo.UID == uid && tag.ReaderName == readerName)
                {
                    Debug.Assert(tag.InventoryInfo.UID == tag.TagInfo.UID);
                    return tag.TagInfo;
                }
            }

            return null;
        }
        */
    }

    // 一个标签的数据
    public class TagData
    {
        public string ReaderName { get; set; }
        public InventoryInfo InventoryInfo { get; set; }
        public TagInfo TagInfo { get; set; }

        // 根据前端发来的 tagInfo 构造一个 TagData 对象
        public static TagData Build(TagInfo tagInfo,
            string protocol = InventoryInfo.ISO15693)
        {
            TagData result = new TagData();
            InventoryInfo inventory = new InventoryInfo();
            result.InventoryInfo = inventory;
            result.TagInfo = tagInfo;

            if (protocol != InventoryInfo.ISO15693
                && protocol != InventoryInfo.ISO14443A)
                throw new ArgumentException($"标签协议名必须为 {InventoryInfo.ISO15693} {InventoryInfo.ISO14443A} 之一", nameof(protocol));

            // inventory
            inventory.Protocol = protocol;
            inventory.UID = tagInfo.UID;
            inventory.TagType = 0;  // ???

            inventory.AipID = 0;    // ???
            inventory.AntennaID = tagInfo.AntennaID;
            inventory.DsfID = tagInfo.DSFID;

            // tagInfo
            tagInfo.Tag = null;

            // tagInfo.ReaderName;  // 自动取协议合适的第一个读卡器？

            // tagInfo.UID; // 自动发生
            // public byte DSFID { get; set; }
            // public byte AFI { get; set; }
            // public byte IcRef { get; set; }

            // 每个块内包含的字节数
            // tagInfo.BlockSize = 0; // 设置为默认值
            // 块最大总数
            // tagInfo.MaxBlockCount = 0;  // 设置为默认值

            // public bool EAS { get; set; }

            // tagInfo.AntennaID;   // 检查是否符合范围。-1 表示设置为第一个天线

            // 锁定状态字符串。表达每个块的锁定状态
            // 例如 "ll....lll"。'l' 表示锁定，'.' 表示没有锁定。缺省为 '.'。空字符串表示全部块都没有被锁定
            // LockStatus { get; set; }

            // 芯片全部内容字节
            // Bytes { get; set; }

            return result;
        }

        // 根据 TagInfo 刷新 InventoryInfo 内的同名成员，和 this.ReaderName
        public void RefreshInventoryInfo()
        {
            // ? this.InventoryInfo.Protocol = TagInfo.Protocol;
            this.InventoryInfo.UID = TagInfo.UID;
            this.InventoryInfo.AntennaID = TagInfo.AntennaID;
            this.InventoryInfo.DsfID = TagInfo.DSFID;

            this.ReaderName = TagInfo.ReaderName;
        }

        // return:
        //      null 或者 ""  表示成功
        //      其他  错误码
        public static string LockBlocks(
            TagInfo tagInfo,
            UInt32 blkAddr,
            UInt32 numOfBlks)
        {
            List<char> map = new List<char>(tagInfo.LockStatus);
            int start = (int)blkAddr;
            for (int i = 0; i < numOfBlks; i++)
            {
                SetBlockStatus(ref map, start++, 'l');
            }
            tagInfo.LockStatus = map.ToString();
            return "";
        }

        static char NormalMapChar = '.';

        static void SetBlockStatus(ref List<char> map, int index, char ch)
        {
            while (map.Count < index + 1)
            {
                map.Add(NormalMapChar);
            }
            map[index] = ch;
        }

        public static char GetBlockStatus(string map, int index)
        {
            if (index >= map.Length)
                return '.';
            return map[index];
        }


        public static NormalResult WriteBlocks(TagInfo tagInfo,
            UInt32 blkAddr,
            UInt32 numOfBlks,
            byte[] data)
        {
            int start = (int)blkAddr;
            int length = (int)(tagInfo.BlockSize * numOfBlks);

            if (length > data.Length)
                return new NormalResult { Value = -1, ErrorCode = $"data 长度不够 {length}" };

            Debug.Assert(data.Length >= length);

            // 前
            List<byte> results = new List<byte>();
            for (int i = 0; i < start; i++)
            {
                results.Add(i >= tagInfo.Bytes.Length ? (byte)0 : tagInfo.Bytes[i]);
            }

            // 中
            for (int i = start; i < start + length; i++)
            {
                results.Add(i >= data.Length ? (byte)0 : data[i]);
            }

            // 后
            for (int i = start + length; i < tagInfo.Bytes.Length; i++)
            {
                results.Add(i >= tagInfo.Bytes.Length ? (byte)0 : tagInfo.Bytes[i]);
            }

            tagInfo.Bytes = results.ToArray();
            return new NormalResult();
        }
    }

    // 标签信息集合
    public class TagCollection // : List<TagData>
    {
        List<TagData> _tags = new List<TagData>();
        object _syncRoot = new object();

        public void Clear()
        {
            lock (_syncRoot)
            {
                _tags.Clear();
            }
        }

        public void Remove(TagData tag)
        {
            lock (_syncRoot)
            {
                _tags.Remove(tag);
            }
        }

        public void Add(TagData tag)
        {
            lock (_syncRoot)
            {
                _tags.Add(tag);
            }
        }

        public List<TagData> GetList()
        {
            lock (_syncRoot)
            {
                return new List<TagData>(_tags);
            }
        }
    }

}
