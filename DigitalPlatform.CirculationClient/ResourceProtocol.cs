using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;

using AsyncPluggableProtocol;

using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.CirculationClient
{
    public class ResourceProtocol : IProtocol
    {
        public string Name
        {
            get
            {
                return "dpres";
            }
        }

        IChannelManager _manager = null;
        ProgressChanged _progressChanged = null;

        public ResourceProtocol(IChannelManager manager, 
            ProgressChanged progressFunc)
        {
            this._manager = manager;
            this._progressChanged = progressFunc;
        }

        // private static string DefaultNamespace = typeof(Program).Namespace;
        // private static string DefaultNamespace = "DigitalPlatform";

        public Task<Stream> GetStreamAsync(string url)
        {
#if NO
            var resource = DefaultNamespace + "." + url.Substring(Name.Length + 1);
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            return TaskEx.FromResult(stream);
#endif
            string path = url.Substring(Name.Length + 1);
            var stream = new dp2ResStream(_manager, path, _progressChanged);
            return Task.FromResult((Stream)stream);
        }
    }

    public delegate void ProgressChanged(string path, long current, long length);

    public delegate void StreamProgressChangedEventHandler(object sender,
StreamProgressChangedEventArgs e);

    public class StreamProgressChangedEventArgs : EventArgs
    {
        public string Path { get; set; }
        public long Current { get; set; }
        public long Length { get; set; }
    }
}
