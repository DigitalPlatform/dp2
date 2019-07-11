using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.IO
{
    // 查看 USB 设备，感知 USB 插拔变化的实用类
    // https://stackoverflow.com/questions/3331043/get-list-of-connected-usb-devices
    public class UsbInfo
    {
        public delegate void delegate_changed(int add_count, int remove_count);

        // 启动观察线程
        public static void StartWatch(delegate_changed callback,
            CancellationToken token)
        {
            List<USBDeviceInfo> Infos = new List<USBDeviceInfo>();

            Infos = GetUSBDevices();

            Task.Run(() =>
            {
                while (token.IsCancellationRequested == false)
                {
                    Task.Delay(TimeSpan.FromSeconds(2)).Wait(token);
                    var result = GetUSBDevices();

                    // 和 Infos 比较
                    Compare(Infos,
    result,
    out int add_count,
    out int remove_count);
                    Infos = result;
                    if (add_count != 0 || remove_count != 0)
                        callback?.Invoke(add_count, remove_count);
                }
            });
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

            try
            {
                using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
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
            catch(Exception)
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
    }
}

