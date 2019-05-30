using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.IO
{
    public class UsbInfo
    {

        public delegate void delegate_changed(int add_count, int remove_count);

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

        public static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("DeviceID"),
                (string)device.GetPropertyValue("PNPDeviceID"),
                (string)device.GetPropertyValue("Description")
                ));
            }

            collection.Dispose();
            return devices;
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

