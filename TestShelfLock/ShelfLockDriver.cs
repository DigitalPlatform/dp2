using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace TestShelfLock
{
    [TestClass]
    public class ShelfLockDriver : IShelfLockDriver
    {
        private object _syncRoot = new object();

        private SerialPort _sp = new SerialPort();

        public string Name { get; set; }

        public ShelfLockDriver()
        {
            // _sp.DataReceived += _sp_DataReceived;
            _sp.ErrorReceived += _sp_ErrorReceived;
        }

        private void _sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            // 
        }

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
            Name = property.SerialPort.ToUpper();
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
            if (_sp.IsOpen)
                _sp.Close();
            return new NormalResult();
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
            var read_result = WriteAndRead(message, 5);
            if (read_result.Value == -1)
                return read_result;

            var parse_result = ParseOpenLockResultMessage(read_result.Result);
            if (parse_result.Value == -1)
                return parse_result;

            return new NormalResult { Value = parse_result.Value };
        }

        public NormalResult OpenShelfLock(string lockNameList, string style)
        {
            string[] list = lockNameList.Split(new char[] { ',' });
            int count = 0;
            foreach (var one in list)
            {
                var path = LockPath.Parse(one);

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

                        var current_path = $"{path.LockName}.{addr}.{index}";

                        // 构造请求消息
                        var message = BuildOpenLockMessage((byte)addr, (byte)index);

                        /*
                        // 发送消息
                        _sp.Write(message, 0, message.Length);

                        byte[] buffer = new byte[5];

                        var read_result = Read(buffer, 5, TimeSpan.FromSeconds(2));
                        */
                        var read_result = WriteAndRead(message, 5);
                        if (read_result.Value == -1)
                            return read_result;

                        var parse_result = ParseOpenLockResultMessage(read_result.Result);
                        if (parse_result.Value == -1)
                            return parse_result;

                        count++;
                    }
                }
            }

            return new NormalResult { Value = count };
        }

        public GetLockStateResult GetShelfLockState(string lockNameList)
        {

            List<LockState> states = new List<LockState>();
            // LockPath 集合
            List<LockPath> paths = new List<LockPath>();
            // 板子名集合
            List<string> cards = new List<string>();

            string[] list = lockNameList.Split(new char[] { ',' });
            foreach (var one in list)
            {
                var path = LockPath.Parse(one);

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

                var read_result = WriteAndRead(message, 8);
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

                // TODO: 根据实际情况只返回部分 index 的
                int byte_offset = 0;
                foreach (var b in parse_result.States)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        int index = (byte_offset * 8) + i + 1;
                        LockState state = new LockState();
                        state.Lock = this.Name;
                        state.Path = $"{this.Name}.{(card)}.{index}";
                        state.Board = addr;
                        state.Index = index;
                        state.State = ((b >> i) & 0x01) != 0 ? "close" : "open";
                        states.Add(state);
                    }

                    byte_offset++;
                }
            }

            // 分配状态

            return new GetLockStateResult { States = states };
        }

        class ReadResult : NormalResult
        {
            public byte[] Result { get; set; }
        }

        ReadResult WriteAndRead(byte[] message,
            int readLength)
        {
            lock (_syncRoot)
            {
                // 发送消息
                _sp.Write(message, 0, message.Length);

                byte[] buffer = new byte[readLength];

                var read_result = Read(buffer, readLength/*, TimeSpan.FromSeconds(2)*/);
                if (read_result.Value == -1)
                    return new ReadResult
                    {
                        Value = -1,
                        ErrorInfo = read_result.ErrorInfo,
                        ErrorCode = read_result.ErrorCode
                    };
                return new ReadResult { Result = buffer };
            }
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

}
