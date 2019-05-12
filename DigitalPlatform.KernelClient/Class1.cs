using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.KernelClient.KernelServiceReference;

namespace DigitalPlatform.KernelClient
{
    public class Class1
    {
        public static async void Test(string url)
        {
            using (KernelServiceClient client = new KernelServiceClient(
                "NetNamedPipeBinding_KernelService", url))
            {
                await client.GetPropertyAsync(new GetPropertyRequest());
            }
        }

        public KernelServiceClient GetClient(string url)
        {
            return new KernelServiceClient(
                "NetNamedPipeBinding_KernelService",
                url);
        }
    }
}
