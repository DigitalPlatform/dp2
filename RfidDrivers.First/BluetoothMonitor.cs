using InTheHand.Net.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RfidDrivers.First
{
    // https://github.com/inthehand/32feet/wiki/Discovery
    /// <summary>
    /// 蓝牙设备连接状态探测
    /// </summary>
    public static class BluetoothMonitor
    {

        public delegate void delegate_changed(int number, List<BluetoothInfo> infos);

        // 启动观察线程
        // parameters:
        //      addr_list   要关注的蓝牙设备地址列表。若为 null，表示关注全部蓝牙设备
        public static void StartWatch(
            List<string> addr_list,
            delegate_changed callback,
            CancellationToken token)
        {

            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    int number = 0;

                    // 记忆遇到过的全部设备
                    // addr --> BluetoothInfo
                    Hashtable device_table = new Hashtable();

                    // addr --> int (0 表示 disconnect, 1 表示 connect)
                    Hashtable table = new Hashtable();

                    while (token.IsCancellationRequested == false)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), token);
                        if (token.IsCancellationRequested)
                            break;

                        var infos = new List<BluetoothInfo>();

                        var cli = new BluetoothClient();
                        var peers = cli.DiscoverDevices();
                        List<string> all = new List<string>();
                        if (addr_list != null)
                            all.AddRange(addr_list);

                        List<string> found_addr_list = new List<string>();
                        foreach (var peer in peers)
                        {
                            if (token.IsCancellationRequested)
                                break;

                            string addr = peer.DeviceAddress.ToString();

                            if (device_table.ContainsKey(addr) == false)
                                device_table[addr] = new BluetoothInfo
                                {
                                    Addr = addr,
                                    Name = peer.DeviceName,
                                };

                            found_addr_list.Add(addr);

                            int index = 0;
                            if (addr_list != null)
                                index = addr_list.IndexOf(addr);
                            if (index != -1)
                            {
                                int old = -1;
                                if (table.ContainsKey(addr))
                                    old = (int)table[addr];
                                int new_state = peer.Connected ? 1 : 0;
                                if (new_state != old
                                || number == 0)
                                {
                                    table[addr] = new_state;
                                    infos.Add(new BluetoothInfo
                                    {
                                        Addr = addr,
                                        Name = peer.DeviceName,
                                        State = peer.Connected ? "connect" : "disconnect",
                                        OldState = GetStateString(old),
                                    });
                                }
                            }

                            all.Remove(addr);
                        }

                        string GetStateString(int value)
                        {
                            if (value == -1)
                                return "";
                            if (value == 1)
                                return "connect";
                            return "disconnect";
                        }

                        // 把没有匹配的 key 移除
                        List<string> remove_list = new List<string>();
                        foreach (string addr in table.Keys)
                        {
                            if (found_addr_list.IndexOf(addr) == -1)
                                remove_list.Add(addr);
                        }

                        foreach (string addr in remove_list)
                        {
                            int old = -1;
                            if (table.ContainsKey(addr))
                                old = (int)table[addr];

                            table.Remove(addr);
                            infos.Add(new BluetoothInfo
                            {
                                Addr = addr,
                                Name = GetName(addr),
                                State = "remove",
                                OldState = GetStateString(old),
                            });
                        }

                        string GetName(string addr)
                        {
                            BluetoothInfo info = (BluetoothInfo)device_table[addr];
                            if (info != null)
                                return info.Name;
                            return "?";
                        }

                        // 此时 all 里面剩下的就是没有探测到的。当作 disconnected 看待
                        foreach (string addr in all)
                        {
                            if (token.IsCancellationRequested)
                                break;

                            int old = -1;
                            if (table.ContainsKey(addr))
                                old = (int)table[addr];
                            if (old != 0
                            || number == 0)
                            {
                                table[addr] = 0;
                                infos.Add(new BluetoothInfo
                                {
                                    Addr = addr,
                                    Name = GetName(addr),
                                    State = "disconnect",
                                    OldState = GetStateString(old),
                                });
                            }
                        }

                        if (infos.Count > 0)
                            callback?.Invoke(number, infos);

                        number++;
                    }
                }
                catch
                {

                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

    }

    public class BluetoothInfo
    {
        public string Addr { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string OldState { get; set; }

        public override string ToString()
        {
            return $"Name={Name},Addr={Addr},State={State},OldState={OldState}";
        }

        public static string ToString(List<BluetoothInfo> infos)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (var info in infos)
            {
                text.AppendLine($"{(++i)}) {info.ToString()}");
            }

            return text.ToString();
        }
    }

}
