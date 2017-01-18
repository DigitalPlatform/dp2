using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.LibraryClient
{
    public interface IChannelManager
    {
        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        LibraryChannel GetChannel(string strServerUrl = ".",
            string strUserName = ".",
            GetChannelStyle style = GetChannelStyle.GUI);
        void ReturnChannel(LibraryChannel channel);

        void DoStop(object sender, StopEventArgs e);
    }

    [Flags]
    public enum GetChannelStyle
    {
        None = 0x0, // 
        GUI = 0x01, // Idle 里面做 Application.DoEvents()
    }

}
