using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetLocalIPs
{
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (string ip in GetLocalIPs())
            {
                Console.WriteLine(ip);
            }

            Console.WriteLine("OK");
        }

        public static List<string> GetLocalIPs()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.Select(ip => ip.ToString()).ToList();
        }
    }
}
