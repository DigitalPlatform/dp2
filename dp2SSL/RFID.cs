using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using DigitalPlatform;
using DigitalPlatform.RFID;

namespace dp2SSL
{
#if REMOVED
    public static class RFID
    {
        #region RFID 有关功能

        public static RfidChannel StartRfidChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            RfidChannel result = new RfidChannel();

            result.Channel = new IpcClientChannel(Guid.NewGuid().ToString(), // 随机的名字，令多个 Channel 对象可以并存 
                    new BinaryClientFormatterSinkProvider());

            ChannelServices.RegisterChannel(result.Channel, true);
            bool bDone = false;
            try
            {
                result.Object = (IRfid)Activator.GetObject(typeof(IRfid),
                    strUrl);
                if (result.Object == null)
                {
                    strError = "无法连接到服务器 " + strUrl;
                    return null;
                }
                bDone = true;
                return result;
            }
            catch (Exception ex)
            {
                strError = "StartRfidChannel() 出现异常: " + ex.Message;
                return null;
            }
            finally
            {
                if (bDone == false)
                    EndRfidChannel(result);
            }
        }

        public static void EndRfidChannel(RfidChannel channel)
        {
            if (channel != null && channel.Channel != null)
            {
                ChannelServices.UnregisterChannel(channel.Channel);
                channel.Channel = null;
            }
        }

#if NO
        static App App
        {
            get
            {
                return ((App)Application.Current);
            }
        }
#endif

        // return:
        //      -2  remoting服务器连接失败。驱动程序尚未启动
        //      -1  出错
        //      0   成功
        public static NormalResult SetEAS(
            RfidChannel channel,
            string reader_name,
            string tag_name,
            bool enable,
            out string strError)
        {
            strError = "";

            try
            {
                return channel.Object.SetEAS(reader_name,
                    tag_name,
                    enable);
            }
            // [System.Runtime.Remoting.RemotingException] = {"连接到 IPC 端口失败: 系统找不到指定的文件。\r\n "}
            catch (System.Runtime.Remoting.RemotingException ex)
            {
                strError = "针对 " + App.RfidUrl + " 的 SetEAS() 操作失败: " + ex.Message;
                return new NormalResult { Value = -2, ErrorInfo = strError };
            }
            catch (Exception ex)
            {
                strError = "针对 " + App.RfidUrl + " 的 SetEAS() 操作失败: " + ex.Message;
                return new NormalResult { Value = -1, ErrorInfo = strError };
            }
        }

#endregion

    }

    public class RfidChannel
    {
        public IpcClientChannel Channel { get; set; }
        public IRfid Object { get; set; }
        // 通道已经成功启动。意思是已经至少经过一个 API 调用并表现正常
        public bool Started { get; set; }
    }

#endif
}
