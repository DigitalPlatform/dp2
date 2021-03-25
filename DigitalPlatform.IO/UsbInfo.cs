// #define SERIAL

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// RS232 设备可参考：
// https://stackoverflow.com/questions/3293889/how-to-auto-detect-arduino-com-port
namespace DigitalPlatform.IO
{
    // 查看 USB 设备，感知 USB 插拔变化的实用类
    // https://stackoverflow.com/questions/3331043/get-list-of-connected-usb-devices
    public static class UsbInfo
    {
        public delegate void delegate_changed(int add_count, int remove_count);

        static ManagementObjectSearcher _searcherUsb = null;

        // 启动观察线程
        public static void StartWatch(delegate_changed callback_param,
            CancellationToken token_param)
        {
            // TODO: 用独立 Thread 实现。把 Thread 的 Stack Size 变大
#if SERIAL
            Infos.AddRange(GetSerialDevices());
#endif

#if NO
            int stackSize = 1024 * 1024 * 64;
            Thread th = new Thread(() =>
            {
                try
                {
                    _searcherUsb = new ManagementObjectSearcher(@"Select * From Win32_USBHub");

                    List<USBDeviceInfo> Infos = new List<USBDeviceInfo>();
                    Infos = GetUSBDevices();

                    while (token.IsCancellationRequested == false)
                    {
                        Task.Delay(TimeSpan.FromSeconds(2), token).Wait(token);
                        if (token.IsCancellationRequested)
                            break;
                        var result = GetUSBDevices();
                        if (token.IsCancellationRequested)
                            break;

#if SERIAL
                        result.AddRange(GetSerialDevices());
#endif
                        // 和 Infos 比较
                        Compare(Infos,
        result,
        out int add_count,
        out int remove_count);
                        Infos = result;
                        if (add_count != 0 || remove_count != 0)
                            callback?.Invoke(add_count, remove_count);
                    }
                }
                catch
                {

                }
                finally
                {
                    _searcherUsb?.Dispose();
                }
            },
                stackSize);

            th.Start();
            th.Join();
#endif

            delegate_changed callback = callback_param;
            CancellationToken token = token_param;
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    _searcherUsb = new ManagementObjectSearcher(@"Select * From Win32_USBHub");

                    List<USBDeviceInfo> Infos = new List<USBDeviceInfo>();
                    Infos = GetUSBDevices();

                    while (token.IsCancellationRequested == false)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), token);
                        if (token.IsCancellationRequested)
                            break;

                        // 2021/3/25
                        // 用消息触发回调
                        lock (_syncRoot_messages)
                        {
                            if (_messages.Count > 0)
                            {
                                foreach (var message in _messages)
                                {
                                    callback?.Invoke(message.add_count, message.remove_count);
                                }
                                _messages.Clear();
                            }
                        }

                        var result = GetUSBDevices();
                        if (token.IsCancellationRequested)
                            break;

#if SERIAL
                        result.AddRange(GetSerialDevices());
#endif
                        // 和 Infos 比较
                        Compare(Infos,
        result,
        out int add_count,
        out int remove_count);
                        Infos = result;
                        if (add_count != 0 || remove_count != 0)
                            callback?.Invoke(add_count, remove_count);
                    }
                }
                catch
                {

                }
                finally
                {
                    _searcherUsb?.Dispose();
                }

            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        static object _syncRoot_messages = new object();
        static List<Message> _messages = new List<Message>();

        class Message
        {
            public int add_count { get; set; }
            public int remove_count { get; set; }
        }

        // 加入一个消息。消息可促使 UsbInfo 触发一次回调
        public static void AddMessage(int add_count, int remove_count)
        {
            lock (_syncRoot_messages)
            {
                _messages.Add(new Message
                {
                    add_count = add_count,
                    remove_count = remove_count
                });
            }
        }

        // 比较两个集合的变化
        static void Compare(List<USBDeviceInfo> infos1,
            List<USBDeviceInfo> infos2,
            out int add_count,
            out int remove_count)
        {
            add_count = 0;
            remove_count = 0;
            List<USBDeviceInfo> cross = new List<USBDeviceInfo>();  // 交叉部分
            foreach (var info1 in infos1)
            {
                var id1 = info1.DeviceID;

                var found = infos2.Find((o) => { return o.DeviceID == id1; });
                if (found == null)
                    remove_count++;
                else
                    cross.Add(found);
            }

            foreach (var info2 in infos2)
            {
                var id2 = info2.DeviceID;
                var found = cross.Find((o) => { return o.DeviceID == id2; });
                if (found == null)
                    add_count++;
            }
        }

        /*
操作类型 crashReport -- 异常报告 
主题 rfidcenter 
媒体类型 text 
内容 发生未捕获的异常: 
Type: System.Runtime.InteropServices.COMException
Message: 消息筛选器取消了调用。 (异常来自 HRESULT:0x80010002 (RPC_E_CALL_CANCELED))
Stack:
在 System.Management.ThreadDispatch.Start()
在 System.Management.ManagementScope.Initialize()
在 System.Management.ManagementObjectSearcher.Initialize()
在 System.Management.ManagementObjectSearcher.Get()
在 DigitalPlatform.IO.UsbInfo.GetUSBDevices()
在 DigitalPlatform.IO.UsbInfo.StartWatch(delegate_changed callback, CancellationToken token)
在 RfidCenter.MainForm..ctor()
在 RfidCenter.Program.Main()


rfidcenter 版本: RfidCenter, Version=1.2.7110.20005, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.1.7601 Service Pack 1
本机 MAC 地址: 94DE80D1DF42 
操作时间 2019/6/26 16:47:50 (Wed, 26 Jun 2019 16:47:50 +0800) 
前端地址 xxxx 经由 http://dp2003.com/dp2library 
         * */
        public static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();
            // return devices; // testing
            try
            {
                /*
                using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                {
                */
                var searcher = _searcherUsb;

                using (ManagementObjectCollection collection = searcher.Get())
                {
                    if (collection == null)
                        return devices;

                    foreach (var device in collection)
                    {
                        devices.Add(new USBDeviceInfo(
                        (string)device.GetPropertyValue("DeviceID"),
                        (string)device.GetPropertyValue("PNPDeviceID"),
                        (string)device.GetPropertyValue("Description")
                        ));
                    }
                }
                /*
            }
                */
                return devices;
            }
            catch (Exception)
            {
                // TODO: 可考虑写入错误日志
                return devices;
            }
        }

        public static List<USBDeviceInfo> GetSerialDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            try
            {
                using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_SerialPort"))
                {
                    using (ManagementObjectCollection collection = searcher.Get())
                    {
                        foreach (var device in collection)
                        {
                            devices.Add(new USBDeviceInfo(
                            (string)device.GetPropertyValue("DeviceID"),
                            (string)device.GetPropertyValue("PNPDeviceID"),
                            (string)device.GetPropertyValue("Description")
                            ));
                        }
                    }
                }
                return devices;
            }
            catch (Exception)
            {
                // TODO: 可考虑写入错误日志
                return devices;
            }
        }

        public static string ToString(List<USBDeviceInfo> infos)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (var info in infos)
            {
                text.Append($"{++i}) {info.ToString()}\r\n");
            }

            return text.ToString();
        }
    }

    public class USBDeviceInfo
    {
        public USBDeviceInfo(string deviceID,
            string pnpDeviceID,
            string description)
        {
            this.DeviceID = deviceID;
            this.PnpDeviceID = pnpDeviceID;
            this.Description = description;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }

        public override string ToString()
        {
            return $"DeviceID={this.DeviceID}, PnpDeviceID={this.PnpDeviceID}, Discription={this.Description}";
        }

        public static string ToString(List<USBDeviceInfo> infos)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (var info in infos)
            {
                text.Append($"{i + 1}) {info.ToString()}\r\n");
                i++;
            }
            return text.ToString();
        }
    }
}

