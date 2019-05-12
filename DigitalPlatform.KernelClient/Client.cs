using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.KernelClient.KernelServiceReference;

namespace DigitalPlatform.KernelClient
{
    public class Client : KernelServiceClient
    {
        public Client(string bindingName, string url) : base(bindingName, url)
        {

        }

        public static Client GetClient(string url)
        {
            return new Client(
                "NetNamedPipeBinding_KernelService",
                url);
        }
    }
}
