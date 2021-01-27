using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Collections;
using System.Threading;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace ShelfLockDriver.First
{
    [TestClass]
    public class WjlLockDriver : IShelfLockDriver, IDisposable
    {
        LockStateMemory _lockMemory = new LockStateMemory();

        private object _syncRoot = new object();

        private SerialPort _sp = new SerialPort();

        public string Name { get; set; }

        public WjlLockDriver()
        {
            // _sp.DataReceived += _sp_DataReceived;
            _sp.ErrorReceived += _sp_ErrorReceived;
        }

        private void _sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            // 
            int i = 0;
            i++;
        }

        public LockProperty LockProperty { get; set; }

        // '*' 解释成的锁编号数组
        List<string> _default_numbers = null;

        // 当前连接的板子数量
        // int _cardAmount = 0;

        // 初始化时需要提供端口号等参数
        // parameters:
        //      style   附加的子参数 
        public NormalResult InitializeDriver(LockProperty property,
            string style)
        {
            var result = Open(property.SerialPort,
                "9600", // baudRate,
                "8",    // dataBits,
                "One",  // stopBits,
                "None", // parity,
                "None"  // handshake
                );
            if (result.Value == -1)
                return result;

            this.LockProperty = property;
            /*
            var query_result = QueryCard();
            if (query_result.Value == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"查询锁控板数量时出错: {query_result.ErrorInfo}"
                };
            _cardAmount = query_result.Value;
            */

            if (this.LockProperty.LockAmountPerBoard == 0)
                this.LockProperty.LockAmountPerBoard = 8;

            _default_numbers = new List<string>();
            for (int i = 0; i < this.LockProperty.LockAmountPerBoard; i++)
            {
                _default_numbers.Add($"{i + 1}");
            }

            Name = property.SerialPort.ToUpper();

            // 清除记忆的状态。这样所有门锁都被认为是不确定状态。因为初始化的时候有可能部分门锁是打开状态
            _lockMemory.Clear();
            RetryLimit = 1; // 遇到出错后重试一次
            return new NormalResult();
        }

        NormalResult Open(string portName,
    String baudRate,
    string dataBits,
    string stopBits,
    string parity,
    string handshake)
        {
            if (_sp.IsOpen)
                _sp.Close();

            _sp.PortName = portName;
            _sp.BaudRate = Convert.ToInt32(baudRate);
            _sp.DataBits = Convert.ToInt16(dataBits);

            /**
             *  If the Handshake property is set to None the DTR and RTS pins 
             *  are then freed up for the common use of Power, the PC on which
             *  this is being typed gives +10.99 volts on the DTR pin & +10.99
             *  volts again on the RTS pin if set to true. If set to false 
             *  it gives -9.95 volts on the DTR, -9.94 volts on the RTS. 
             *  These values are between +3 to +25 and -3 to -25 volts this 
             *  give a dead zone to allow for noise immunity.
             *  http://www.codeproject.com/Articles/678025/Serial-Comms-in-Csharp-for-Beginners
             */
            if (handshake == "None")
            {
                //Never delete this property
                _sp.RtsEnable = true;
                _sp.DtrEnable = true;
            }

            // SerialPortEventArgs args = new SerialPortEventArgs();
            try
            {
                _sp.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
                _sp.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                _sp.Handshake = (Handshake)Enum.Parse(typeof(Handshake), handshake);
                _sp.ReadTimeout = 2000;
                _sp.WriteTimeout = 2000; /*Write time out*/
                _sp.Open();
                // args.isOpend = true;
            }
            catch (System.Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"串口打开失败: {ex.Message}"
                };
            }

            return new NormalResult();
        }


        public NormalResult ReleaseDriver()
        {
            try
            {
                if (_sp.IsOpen)
                    _sp.Close();
                return new NormalResult();
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        public NormalResult QueryCard()
        {
            // 构造请求消息
            var message = BuildQueryCardMessage();

            /*
            // 发送消息
            _sp.Write(message, 0, message.Length);

            byte[] buffer = new byte[5];

            var read_result = Read(buffer, 5, TimeSpan.FromSeconds(2));
            */
            var read_result = WriteAndRead(message, 5, false);
            if (read_result.Value == -1)
                return read_result;

            var parse_result = ParseOpenLockResultMessage(read_result.Result);
            if (parse_result.Value == -1)
                return parse_result;

            return new NormalResult { Value = parse_result.Value };
        }

        // 防止开门动作和探测门状态之间冲突
        private object _syncAPI = new object();

        public NormalResult OpenShelfLock(string lockNameList, string style)
        {
            lock (_syncAPI)
            {
                var open_and_close = StringUtil.IsInList("open+close", style);

                string[] list = lockNameList.Split(new char[] { ',' });
                int count = 0;
                foreach (var one in list)
                {
                    var path = LockPath.Parse(one, null, _default_numbers);

                    if (path.LockName == "*" || this.Name == path.LockName)
                    {

                    }
                    else
                        continue;

                    foreach (var card in path.CardNameList)
                    {
                        foreach (var number in path.NumberList)
                        {
                            int addr = 1;
                            int index = 1;
                            if (string.IsNullOrEmpty(card) == false)
                            {
                                if (Int32.TryParse(card, out addr) == false)
                                    return new NormalResult
                                    {
                                        Value = -1,
                                        ErrorInfo = $"锁控板编号 '{card}' 不合法"
                                    };
                            }
                            if (string.IsNullOrEmpty(number) == false)
                            {
                                if (Int32.TryParse(number, out index) == false)
                                    return new NormalResult
                                    {
                                        Value = -1,
                                        ErrorInfo = $"锁编号 '{number}' 不合法"
                                    };
                            }

                            var current_path = $"{this.Name}.{addr}.{index}";

                            if (open_and_close)
                            {
                                // 为了模拟开门后立即关门，这里加入开过门的痕迹，但并不真正开门
                                _lockMemory.MemoryOpen(current_path);
                                count++;
                                continue;
                            }

                            // 构造请求消息
                            var message = BuildOpenLockMessage((byte)addr, (byte)index);

                            /*
                            // 发送消息
                            _sp.Write(message, 0, message.Length);

                            byte[] buffer = new byte[5];

                            var read_result = Read(buffer, 5, TimeSpan.FromSeconds(2));
                            */
                            var read_result = WriteAndRead(message, 5, false);
                            if (read_result.Value == -1)
                                return read_result;

                            var parse_result = ParseOpenLockResultMessage(read_result.Result);
                            if (parse_result.Value == -1)
                                return parse_result;

                            // 加入一个表示发生过开门的状态，让后继获得状态的 API 至少能返回一次打开状态
                            _lockMemory.MemoryOpen(current_path);

                            _lockMemory.Set(current_path, "open");
                            count++;
                        }
                    }
                }

                // 2021/1/27
                // 延时一秒钟，以确保后面获得状态时门锁已经打开
                // Thread.Sleep(1000);

                return new NormalResult { Value = count };
            }
        }

        // static int _testCount = 1;

        // parameters:
        //      style   如果包含 "compact"，表示只在 result.StateBytes 中返回密集形态的锁状态值。
        //              否则就是在 States 中返回一个一个锁状态
        // result.Value:
        //      -1  出错
        //      其他  成功
        // result.StateBytes
        //      返回密集形态的锁状态值
        public GetLockStateResult GetShelfLockState(string lockNameList,
            string style)
        {
            lock (_syncAPI)
            {
                List<LockState> states = new List<LockState>();
                // LockPath 集合
                List<LockPath> paths = new List<LockPath>();
                // 板子名集合
                List<string> cards = new List<string>();

                bool compact = StringUtil.IsInList("compact", style);
                List<byte> result_bytes = new List<byte>();

                string[] list = lockNameList.Split(new char[] { ',' });
                foreach (var one in list)
                {
                    var path = LockPath.Parse(one, null, _default_numbers);

                    if (path.LockName == "*" || this.Name == path.LockName)
                    {

                    }
                    else
                        continue;   // 跳过不匹配本驱动名字的片段

                    paths.Add(path);

                    // 搜集板子编号
                    cards.AddRange(path.CardNameList);
                }

                StringUtil.RemoveDup(ref cards, false);

                // path string --> LockState
                Hashtable table = new Hashtable();

                // 按照每个板子来获取状态
                foreach (var card in cards)
                {
                    int addr = 1;
                    if (string.IsNullOrEmpty(card) == false)
                    {
                        if (Int32.TryParse(card, out addr) == false)
                            return new GetLockStateResult
                            {
                                Value = -1,
                                ErrorInfo = $"锁控板编号 '{card}' 不合法"
                            };
                    }

                    string board_path = $"{this.Name}.{addr}.";

                    // 如果门全部关闭，则不需要具体探测状态
                    // 即：只有打开状态的门才有必要探测状态，看看它们是否被手动关闭了
                    if (_lockMemory.GetOpenedCount() == 0   // 只有当无存疑的状态事项时，才进行优化
                        && _lockMemory.GetBoardState(board_path) == "all_close")
                    {
                        result_bytes.AddRange(GetAllCloseBytes());
                        continue;
                    }

                    /*
                    if (addr <= 0 || addr > _cardAmount)
                    {
                        return new GetLockStateResult
                        {
                            Value = -1,
                            ErrorInfo = $"锁控板编号 '{addr}' 超过当前可用值范围 1-{_cardAmount}"
                        };
                    }*/

                    // 构造请求消息
                    var message = BuildGetLockStateMessage((byte)addr);

                    var read_result = WriteAndRead(message, 8, true);
                    if (read_result.Value == -1)
                        return new GetLockStateResult
                        {
                            Value = -1,
                            ErrorInfo = read_result.ErrorInfo,
                            ErrorCode = read_result.ErrorCode,
                        };

                    var parse_result = ParseGetLockStateResultMessage(read_result.Result, false);
                    if (parse_result.Value == -1)
                        return new GetLockStateResult
                        {
                            Value = -1,
                            ErrorInfo = parse_result.ErrorInfo,
                            ErrorCode = "parseResultError"
                        };

                    if (compact)
                    {
                        result_bytes.AddRange(parse_result.States);
                        continue;
                    }

                    // TODO: 根据实际情况只返回部分 index 的
                    int byte_offset = 0;
                    foreach (var b in parse_result.States)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            // 优化为 close 状态的对象不加入 states 集合
                            if (((b >> i) & 0x01) == 0)
                            {
                                LockState state = new LockState();
                                state.State = "open";
                                int index = (byte_offset * 8) + i + 1;
                                state.Lock = this.Name;
                                state.Path = $"{this.Name}.{(card)}.{index}";
                                state.Board = addr;
                                state.Index = index;
                                // state.State = ((b >> i) & 0x01) != 0 ? "close" : "open";
                                // states.Add(state);
                                table[state.Path] = state;
                            }
                        }

                        byte_offset++;
                    }
                }

                if (compact)
                    return new GetLockStateResult { StateBytes = result_bytes };

                // 分配状态
                foreach (var path in paths)
                {
                    foreach (string card in path.CardNameList)
                    {
                        var numberList = path.NumberList;
                        if (numberList[0] == "*")
                            numberList = _default_numbers;
                        foreach (string number in numberList)
                        {
                            string path_string = $"{this.Name}.{card}.{number}";

                            LockState state = table[path_string] as LockState;
                            if (state == null)
                            {
                                state = new LockState();
                                state.State = "close";
                                state.Lock = this.Name;
                                state.Path = path_string;
                                state.Board = Convert.ToInt32(card);
                                state.Index = Convert.ToInt32(number);
                            }
                            else
                            {
                                /*
                                // testing !!!
                                if (state.State == "open"
                                    && _testCount > 0)
                                {
                                    state.State = "close";
                                    _testCount--;
                                }
                                */
                            }

                            states.Add(state);

                            // 记忆开闭状态
                            _lockMemory.Set(path_string, state.State);

                            // 观察这个锁是否曾经打开过而没有来得及获取至少一次状态？
                            var opened = _lockMemory.IsOpened(path_string,
                                false,
                                out DateTime memo_time);
                            if (opened)
                            {
                                // 正常情况：发出指令后立即就探测到门是开的状态
                                if (state.State == "open")
                                    _lockMemory.ClearOpen(path_string);

                                // 异常情况：发出指令后，探测到门还是关闭状态。
                                if (state.State == "close")
                                {
                                    // 需要等待一段时间，补一个 "open,close" 状态
                                    if (DateTime.Now - memo_time > _waitPeriod)
                                    {
                                        _lockMemory.ClearOpen(path_string);
                                        state.State = "open,close";
                                    }
                                }

                            }
                            else
                                _lockMemory.ClearOpen(path_string);
                        }
                    }
                }

                return new GetLockStateResult { States = states };

                // 构造表示全 Close 的一组状态 bytes
                List<byte> GetAllCloseBytes()
                {
                    return new List<byte> { 0xff, 0xff, 0xff, 0xff };
                }
            }
        }

        // 判断"open,close"情形的延迟时间长度
        static TimeSpan _waitPeriod = TimeSpan.FromSeconds(6);

        class ReadResult : NormalResult
        {
            public byte[] Result { get; set; }
        }

        // 通过原始的 bytes 得到 LockState 集合
        public List<LockState> BuildStates(List<string> cards,
            List<byte> bytes)
        {
            Debug.Assert(cards.Count == bytes.Count / 4);
            if (cards.Count != (bytes.Count / 4))
                throw new ArgumentException($"cards({cards.Count}) 与 bytes({bytes.Count}) 数量关系不正确");

            List<LockState> results = new List<LockState>();
            foreach (var card in cards)
            {
                var current_bytes = bytes.GetRange(0, 4);
                bytes.RemoveRange(0, 4);
                int byte_offset = 0;
                foreach (var b in current_bytes)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        string lock_state = (((b >> i) & 0x01) == 0) ? "open" : "close";

                        int board = Convert.ToInt32(card);
                        int index = (byte_offset * 8) + i + 1;

                        var state = new LockState
                        {
                            Path = $"{this.Name}.{board}.{index}",
                            Lock = this.Name,
                            Board = board,
                            Index = index,
                            State = lock_state,
                        };
                        results.Add(state);
                    }

                    byte_offset++;
                }
            }

            return results;
        }

        // 最多允许重试的次数。0 表示不重试
        public int RetryLimit { get; set; }

        ReadResult WriteAndRead(byte[] message,
            int readLength,
            bool delayRecv)
        {
            NormalResult read_result = null;
            for (int i = 0; i < RetryLimit + 1; i++)
            {
                lock (_syncRoot)
                {
                    bool suceed = false;
                    try
                    {
                        // 发送消息
                        _sp.Write(message, 0, message.Length);

                        // 厂家建议这里延迟 100 毫秒
                        if (delayRecv)
                            Thread.Sleep(100);

                        byte[] buffer = new byte[readLength];

                        read_result = Read(buffer, readLength/*, TimeSpan.FromSeconds(2)*/);
                        if (read_result.Value != -1)
                        {
                            suceed = true;
                            return new ReadResult { Result = buffer };
                        }

                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        read_result = new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"WriteAndRead() 出现异常: {ex.Message}",
                            ErrorCode = ex.GetType().ToString()
                        };
                    }
                    finally
                    {
                        if (suceed == false)
                        {
                            _sp.DiscardOutBuffer();
                            _sp.DiscardInBuffer();
                        }
                    }
                }
            }

            return new ReadResult
            {
                Value = -1,
                ErrorInfo = read_result.ErrorInfo,
                ErrorCode = read_result.ErrorCode
            };
        }

        NormalResult Read(byte[] buffer,
    int length)
        {
            try
            {
                int index = 0;
                DateTime start_time = DateTime.Now;
                while (true)
                {
                    {
                        var count = _sp.Read(buffer, index, length);
                        index += count;
                        length -= count;
                        if (length <= 0)
                            return new NormalResult();
                    }

                    Thread.Sleep(0);
                }
            }
            catch (TimeoutException ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "",
                    ErrorCode = "readTimeout"
                };
            }
        }


#if REMOVED
        NormalResult Read(byte[] buffer,
            int length,
            TimeSpan timeout)
        {
            int index = 0;
            DateTime start_time = DateTime.Now;
            while (true)
            {
                if (_sp.BytesToRead > 0)
                {
                    var count = _sp.Read(buffer, index, length);
                    index += count;
                    length -= count;
                    if (length <= 0)
                        return new NormalResult();
                }

                if (DateTime.Now - start_time > timeout)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "",
                        ErrorCode = "readTimeout"
                    };
                Thread.Sleep(0);
            }
        }
#endif

        /*
板地址查询：
查询的时候一次只能有一块板接到主机。

发送值:
BYTE[0] 0X80命令头（固定）。
BYTE[1] 0X01（固定）。
BYTE[2] 0X00（固定）。
BYTE[3] 0X99（固定）。
BYTE[4] 校验位：等于 BYTE[0]异或 BYTE[1]异或异或 BYTE[2]异或 BYTE[3]

返回值:
BYTE[0] 0X80 命令头（固定）。
BYTE[1] 0X01（固定）。
BYTE[2] 0X01 到 0X40，板地址，最多 64 个板。
BYTE[3] 0X99 包尾 2（固定）。
BYTE[4] 校验位：等于 BYTE[0]异或 BYTE[1]异或 BYTE[2]异或 BYTE[3]。
例 1：
发送：
BYTE[0] BYTE[1] BYTE[2] BYTE[3] BYTE[4]
0X80 0X01 0X00 0X99 0X18
如板地址为 1 时：
BYTE[0] BYTE[1] BYTE[2] BYTE[3] BYTE[4]
0X80 0X01 0X01 0X99 0X19
        * 
         * */
        // 构造查询板子数量指令
        static byte[] BuildQueryCardMessage()
        {
            var result = new List<byte>() { 0x80, 0x01, 0x00, 0x99 };
            result.Add(ComputeCheck(result));
            return result.ToArray();
        }

        // 从返回的消息中获得板子数量结果
        // result.Value
        //      -1  出错
        //     其他   板子数量 (1-64)
        static NormalResult ParseQueryCardResultMessage(byte[] message)
        {
            if (message == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "返回消息为 null",
                    ErrorCode = "comError"
                };
            if (message.Length != 5)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "返回消息长度不是 5 字节",
                    ErrorCode = "comError"
                };
            // 计算校验位
            var bytes = new List<byte>(message);
            bytes.RemoveAt(message.Length - 1);
            var check = ComputeCheck(bytes);
            if (check != message[message.Length - 1])
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "返回消息校验失败",
                    ErrorCode = "comError"
                };
            return new NormalResult { Value = message[2] };
        }

        /*
开柜命令如下：
BYTE[0] 0X8A 命令头（固定）。
BYTE[1] 0X01 到 0X40，板地址，最多 64 个板。
BYTE[2] 0X01 到 0X08，柜号，从 1 到 8。
BYTE[3] 0X11 开柜命令（固定）。
BYTE[4] 校验位：等于 BYTE[0]异或 BYTE[1]异或 BYTE[2]异或 BYTE[3]。
返回值：
BYTE[0] 0X80 命令头（固定）。
BYTE[1] 0X01 到 0X40，板地址，最多 64 个板。
BYTE[2] 0X01 到 0X08，柜号，从 1 到 8。
BYTE[3] 0X00 或者 0X11，看锁的状态如果锁开的时候开关是常开就是 0X11 为开，如果常闭开关就为 0X00。
BYTE[4] 校验位：等于 BYTE[0]异或 BYTE[1]异或 BYTE[2]异或 BYTE[3]。
由于电子锁需要 1 秒的时间才能开，返回值会在发完开锁命令后 1 秒才
会返回值。
如开第一组柜的一号柜：
发送
BYTE[0] BYTE[1] BYTE[2] BYTE[3] BYTE[4]
0X8A 0X01 0X01 0X11 0X9B
返回
BYTE[0] BYTE[1] BYTE[2] BYTE[3] BYTE[4]
0X80 0X01 0X01 0X11 0X91 (锁为开)
0X80 0X01 0X01 0X00 0X80 (锁为关)
* 
 * */
        // 构造开锁指令
        static byte[] BuildOpenLockMessage(byte cardNumber,
            byte lockNumber)
        {
            var result = new List<byte>() { 0x8A };
            // , 0x01, 0x01, 0x11, 0x91};
            result.Add(cardNumber);
            result.Add(lockNumber);
            result.Add(0x11);
            result.Add(ComputeCheck(result));
            return result.ToArray();
        }

        // 从返回的消息中获得开门结果
        // result.Value
        //      -1  出错
        //      0   门是关闭状态
        //      1   门是打开状态
        static NormalResult ParseOpenLockResultMessage(byte[] message)
        {
            if (message == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "返回消息为 null",
                    ErrorCode = "comError"
                };
            if (message.Length != 5)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "返回消息长度不是 5 字节",
                    ErrorCode = "comError"
                };
            // 计算校验位
            var bytes = new List<byte>(message);
            bytes.RemoveAt(message.Length - 1);
            var check = ComputeCheck(bytes);
            if (check != message[message.Length - 1])
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "返回消息校验失败",
                    ErrorCode = "comError"
                };
            return new NormalResult { Value = (message[3] != 0 ? 1 : 0) };
        }

        /*
读柜门开关命令：
读单个柜信息 BYTE[2]为 0X01 到 0X40，读一组柜信息 BYTE[2]为 0X00。
BYTE[0] 0X80 命令头（固定）。
BYTE[1] 0X01 到 0X40，板地址，最多 64 个板。
BYTE[2] 0X00 到 0X08，（0X00 是查所有柜门状态，0X01-0X08 是查单个柜门状态）。
BYTE[3] 0X33（固定）。
BYTE[4] 校验位：等于 BYTE[0]异或 BYTE[1]异或 BYTE[2]异或 BYTE[3]。

单个柜返回值：
BYTE[0] 0X80 命令头（固定）。
BYTE[1] 0X01 到 0X40，板地址，最多 64 个板。
BYTE[2] 0X01 到 0X08，柜号，从 1 到 8。
BYTE[3] 0X00 为此柜打开，0X11 为此柜锁住（看锁的状态如果锁开的时候是开关是长开就是 0X11 为开，
如果长闭开关就为 0X00）。
BYTE[4] 校验位：等于 BYTE[0]异或 BYTE[1]异或 BYTE[2]异或 BYTE[3]。

组柜返回值：
BYTE[0] 0X80 命令头（固定）。
BYTE[1] 0X01 到 0X40，板地址，最多 64 个板。
BYTE[2] 25-32 路柜信息，8 个字节由低到高为 25-32 柜的状态，锁开时开关为长开:1 为关 0 为开。
BYTE[3] 17-24 路柜信息，8 个字节由低到高为 17-24 柜的状态，锁开时开关为长开:1 为关 0 为开。
BYTE[4] 9-16 路柜信息，8 个字节由低到高为 9-16 柜的状态，锁开时开关为长开:1 为关 0 为开。
BYTE[5] 1-8 路柜信息，8 个字节由低到高为 1-8 柜的状态，锁开时开关为长开:1 为关 0 为开。
BYTE[6] 0X33（固定）。
BYTE[7] 校验位：等于 BYTE[0]异或 BYTE[1]异或 BYTE[2]异或 BYTE[3] 异或 BYTE[4]异或 BYTE[5] 异或 BYTE[6]。

读第一组柜第一柜状态：
发送：
BYTE[0] BYTE[1] BYTE[2] BYTE[3] BYTE[4]
0X80 0X01 0X01 0X33 0XB3
返回：
BYTE[0] BYTE[1] BYTE[2] BYTE[3] BYTE[4]
0X80 0X01 0X01 0X11 0X91 (锁为开)
0X80 0X01 0X01 0X00 0X80 (锁为关)

读第一组柜状态：
发送：
BYTE[0] BYTE[1] BYTE[2] BYTE[3] BYTE[4]
0X80 0X01 0X00 0X33 0XB2
返回：
BYTE[0] BYTE[1] BYTE[2] BYTE[3] BYTE[4] BYTE[5] BYTE[6] BYTE[7]
0X80 0X01 0XFF 0XFF 0XFF 0XFF 0X33 0XB2
        * 
         * */
        // 构造获取状态指令
        // parameters:
        //      cardNumber   卡编号(1-64)
        //      lockNumber  锁编号。0 表示希望获得卡内全部锁的状态；1-8 表示只希望获得一个锁的状态
        static byte[] BuildGetLockStateMessage(byte cardNumber,
            byte lockNumber = 0)
        {
            var result = new List<byte>() { 0x80 };
            // , 0x01, 0x01, 0x11, 0x91};
            result.Add(cardNumber);
            result.Add(lockNumber);
            result.Add(0x33);
            result.Add(ComputeCheck(result));
            return result.ToArray();
        }

        public class LockStateResult : NormalResult
        {
            public List<byte> States { get; set; }
        }

        // 从返回的消息中获得状态结果
        // result.Value
        //      -1  出错
        //      0   门是关闭状态
        //      1   门是打开状态
        static LockStateResult ParseGetLockStateResultMessage(byte[] message,
            bool single)
        {
            if (message == null)
                return new LockStateResult
                {
                    Value = -1,
                    ErrorInfo = "返回消息为 null",
                    ErrorCode = "comError"
                };
            if (single)
            {
                if (message.Length != 5)
                    return new LockStateResult
                    {
                        Value = -1,
                        ErrorInfo = "返回消息长度不是 5 字节",
                        ErrorCode = "comError"
                    };
            }
            else
            {
                if (message.Length != 8)
                    return new LockStateResult
                    {
                        Value = -1,
                        ErrorInfo = "返回消息长度不是 8 字节",
                        ErrorCode = "comError"
                    };
            }

            // 计算校验位
            var bytes = new List<byte>(message);
            bytes.RemoveAt(message.Length - 1);
            var check = ComputeCheck(bytes);
            if (check != message[message.Length - 1])
                return new LockStateResult
                {
                    Value = -1,
                    ErrorInfo = "返回消息校验失败",
                    ErrorCode = "comError"
                };

            if (single)
                return new LockStateResult { Value = (message[3] != 0 ? 1 : 0) };

            {
                List<byte> states = new List<byte>();
                for (int i = 0; i < 4; i++)
                {
                    var state = message[2 + i];
                    states.Insert(0, state);
                }
                return new LockStateResult
                {
                    Value = 0,
                    States = states
                };
            }
        }


        // 计算校验位
        static byte ComputeCheck(List<byte> bytes)
        {
            byte result = bytes[0];
            int i = 0;
            foreach (var b in bytes)
            {
                if (i == 0)
                    result = b;
                else
                    result = (byte)(b ^ result);
                i++;
            }

            return result;
        }

        [TestMethod]
        public void Test_ComputeCheck_1()
        {
            var bytes = new List<byte> { 0x8a, 0x01, 0x01, 0x11 };
            var result = ComputeCheck(bytes);
            Assert.AreEqual(0x9b, result);
        }

        [TestMethod]
        public void Test_ComputeCheck_2()
        {
            var bytes = new List<byte> { 0x80, 0x01, 0x01, 0x11 };
            var result = ComputeCheck(bytes);
            Assert.AreEqual(0x91, result);
        }

        [TestMethod]
        public void Test_ComputeCheck_3()
        {
            var bytes = new List<byte> { 0x80, 0x01, 0x01, 0x00 };
            var result = ComputeCheck(bytes);
            Assert.AreEqual(0x80, result);
        }

        public void Dispose()
        {
            if (_sp.IsOpen)
                _sp.Close();

            ((IDisposable)_sp).Dispose();
        }
    }
}
