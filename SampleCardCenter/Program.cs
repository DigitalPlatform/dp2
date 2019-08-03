using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

namespace SampleCardCenter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                StartServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动 .NET Remoting Server 时发生错误: {ex.Message}");
                return;
            }

            Console.WriteLine("启动成功。按照任意键退出");
            Console.ReadKey();
        }

        static IChannel _serverChannel = null;

        static void StartServer()
        {
            _serverChannel = new IpcServerChannel("CardCenterChannel");
            RemotingConfiguration.ApplicationName = "CardCenterServer";

            //Register the server channel.
            ChannelServices.RegisterChannel(_serverChannel, false);

            //Register this service type.
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(CardCenterServer),
                "CardCenterServer",
                WellKnownObjectMode.Singleton);
        }
    }
}
