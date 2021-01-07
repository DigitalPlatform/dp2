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
        public delegate void delegate_changed(string addr, string action);

        // 启动观察线程
        public static void StartWatch(
            List<string> addr_list,
            delegate_changed callback,
            CancellationToken token)
        {

            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    // addr --> bool
                    Hashtable table = new Hashtable();

                    while (token.IsCancellationRequested == false)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), token);
                        if (token.IsCancellationRequested)
                            break;

                        var cli = new BluetoothClient();
                        var peers = cli.DiscoverDevices();
                        List<string> all = new List<string>(addr_list);
                        foreach (var peer in peers)
                        {
                            if (token.IsCancellationRequested)
                                break;

                            string addr = peer.DeviceAddress.ToString();
                            int index = addr_list.IndexOf(addr);
                            if (index != -1)
                            {
                                bool old = false;
                                if (table.ContainsKey(addr))
                                    old = (bool)table[addr];
                                if (peer.Connected != old)
                                {
                                    table[addr] = peer.Connected;
                                    callback?.Invoke(addr, peer.Connected ? "connect" : "disconnect");
                                }
                            }

                            all.Remove(addr);
                        }

                        // 此时 all 里面剩下的就是没有探测到的。当作 disconnected 看待
                        foreach(string addr in all)
                        {
                            if (token.IsCancellationRequested)
                                break;

                            bool old = false;
                            if (table.ContainsKey(addr))
                                old = (bool)table[addr];
                            if (old != false)
                            {
                                table[addr] = false;
                                callback?.Invoke(addr, "disconnect");
                            }
                        }
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
}
