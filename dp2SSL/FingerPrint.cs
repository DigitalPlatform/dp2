using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using DigitalPlatform.Interfaces;

namespace dp2SSL
{
    public static class FingerPrint
    {

        public static FingerprintChannel StartFingerprintChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            FingerprintChannel result = new FingerprintChannel();

            var clientProv = new BinaryClientFormatterSinkProvider();
            var serverProv = new BinaryServerFormatterSinkProvider();
            serverProv.TypeFilterLevel =
              System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            result.Channel = new IpcClientChannel(Guid.NewGuid().ToString(), // 随机的名字，令多个 Channel 对象可以并存 
                    clientProv);

            ChannelServices.RegisterChannel(result.Channel, true);
            bool bDone = false;
            try
            {
                result.Object = (IFingerprint)Activator.GetObject(typeof(IFingerprint),
                    strUrl);
                if (result.Object == null)
                {
                    strError = "无法连接到指纹接口服务器 " + strUrl;
                    return null;
                }
                // result.Object.Scaned += new ScanedEventHandler(result.handler.LocallyHandleScaned);
                bDone = true;
                return result;
            }
            catch(Exception ex)
            {
                strError = "StartFingerprintChannel() 出现异常: " + ex.Message;
                return null;
            }
            finally
            {
                if (bDone == false)
                    EndFingerprintChannel(result);
            }
        }

        public static void EndFingerprintChannel(FingerprintChannel channel)
        {
            if (channel != null && channel.Channel != null)
            {
                ChannelServices.UnregisterChannel(channel.Channel);
                channel.Channel = null;
            }
        }

#if NO
        // return:
        //      -2  remoting服务器连接失败。驱动程序尚未启动
        //      -1  出错
        //      0   成功
        public static int AddItems(
            FingerprintChannel channel,
            List<FingerprintItem> items,
            out string strError)
        {
            strError = "";

            try
            {
                int nRet = channel.Object.AddItems(items,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            // [System.Runtime.Remoting.RemotingException] = {"连接到 IPC 端口失败: 系统找不到指定的文件。\r\n "}
            catch (System.Runtime.Remoting.RemotingException ex)
            {
                strError = "针对 " + Program.MainForm.FingerprintReaderUrl + " 的 AddItems() 操作失败: " + ex.Message;
                return -2;
            }
            catch (Exception ex)
            {
                strError = "针对 " + Program.MainForm.FingerprintReaderUrl + " 的 AddItems() 操作失败: " + ex.Message;
                return -1;
            }

            return 0;
        }
#endif

    }

    public class FingerprintChannel
    {
        public IpcClientChannel Channel { get; set; }
        public IFingerprint Object { get; set; }
        // 通道已经成功启动。意思是已经至少经过一个 API 调用并表现正常
        public bool Started { get; set; }
    }

#if NO
    public class MyConcreteHandler : MyDelegateObject
    {
        protected override void EventHandlerCallbackCore(object sender, ScanedEventArgs e)
        {
            MessageBox.Show(e.Text);
        }
    }
#endif
}
