using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.IO
{
    /// <summary>
    /// UDP 广播通知
    /// </summary>
    public class UdpNotifier
    {
        UdpClient udpClient = new UdpClient();
        int PORT = 9876;

        public delegate void Delegate_notify(string message);

        public void StartListening(Delegate_notify func)
        {
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

            var from = new IPEndPoint(0, 0);
            Task.Run(() =>
            {
                while (true)
                {
                    var recvBuffer = udpClient.Receive(ref from);
                    func?.Invoke(Encoding.UTF8.GetString(recvBuffer));
                }
            });
        }

        public void StopListening()
        {
            udpClient.Dispose();
        }

        public void Notify(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, "255.255.255.255", PORT);
        }
    }
}
