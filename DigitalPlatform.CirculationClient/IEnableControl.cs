using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.CirculationClient
{
    public interface IEnableControl
    {
        void EnableControls(bool enable);
    }

    // 组合接口
    public interface IChannelLooping : ILoopingHost, IChannelHost, IEnableControl
    {
        // 三种动作: GetChannel() BeginLoop() 和 EnableControl()
        // parameters:
        //          style 可以有如下子参数:
        //              disableControl
        //              timeout:hh:mm:ss 确保超时参数在 hh:mm:ss 以长
        // https://learn.microsoft.com/en-us/dotnet/api/system.timespan.parse?view=net-6.0
        // [ws][-]{ d | [d.]hh:mm[:ss[.ff]] }[ws]
        Looping Looping(
            out LibraryChannel channel,
            string text = "",
            string style = null,
            StopEventHandler handler = null);

        // 两种动作: BeginLoop() 和 EnableControl()
        Looping Looping(string text,
            string style = null,
            StopEventHandler handler = null);

    }
}
