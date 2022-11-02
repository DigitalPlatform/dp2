using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.LibraryClient
{
    public class ChannelList
    {
        private static readonly Object _syncRoot_channelList = new Object(); // 2017/5/18
        List<LibraryChannel> _channelList = new List<LibraryChannel>();

        public void AddChannel(LibraryChannel channel)
        {
            lock (_syncRoot_channelList)
            {
                _channelList.Add(channel);
            }
        }

        public void RemoveChannel(LibraryChannel channel)
        {
            lock (_syncRoot_channelList)
            {
                _channelList.Remove(channel);
            }
        }

        public void AbortAll()
        {
            lock (_syncRoot_channelList)
            {
                foreach (LibraryChannel channel in _channelList)
                {
                    if (channel != null)
                        channel.Abort();
                }
            }
        }
    }

    public interface IChannelHost
    {
        // parameters:
        //      strStyle    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        LibraryChannel GetChannel(string strServerUrl = ".",
            string strUserName = ".",
            GetChannelStyle style = GetChannelStyle.GUI,
            string strClientIP = "");

        void ReturnChannel(LibraryChannel channel);

        void DoStop(object sender, StopEventArgs e);
    }
}
