using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.LibraryService
{
    public static class WcfBindings
    {

        public static System.ServiceModel.Channels.Binding CreateHttpsBinding1()
        {
            var elements = new BindingElementCollection();

            var session = new ReliableSessionBindingElement()
            {
                Ordered = true,
                InactivityTimeout = TimeSpan.FromMinutes(20)
            };
            var encoding = new TextMessageEncodingBindingElement(
                    MessageVersion.Soap12WSAddressing10,
                    System.Text.Encoding.UTF8);
            {
                // 设置 ReaderQuotas
                encoding.ReaderQuotas.MaxArrayLength = 1024 * 1024;
                encoding.ReaderQuotas.MaxStringContentLength = 1024 * 1024;
                //encoding.ReaderQuotas.MaxDepth = 64;
                //encoding.ReaderQuotas.MaxNameTableCharCount = 1024 * 1024;
                //encoding.ReaderQuotas.MaxBytesPerRead = 4096;
            }

            var transport = new HttpsTransportBindingElement()
            {
                MaxReceivedMessageSize = 1024 * 1024
            };
            elements.Add(session);
            elements.Add(encoding);
            elements.Add(transport);
            var binding = new CustomBinding(elements);
            binding.Name = "CustomWsHttpsReliableBinding";
            SetTimeout(binding);
            return binding;
        }

        public static void SetTimeout(System.ServiceModel.Channels.Binding binding)
        {
            binding.SendTimeout = new TimeSpan(0, 20, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 20, 0);    // 决定Session存活
            binding.CloseTimeout = new TimeSpan(0, 20, 0);
            binding.OpenTimeout = new TimeSpan(0, 20, 0);
        }
    }
}
