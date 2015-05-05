using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;
using System.Diagnostics;
using System.Windows.Forms;

using DigitalPlatform.UnionCatalogClient.UnionCatalogServiceReference;

namespace DigitalPlatform.UnionCatalogClient
{
    public class UnionCatalog
    {
        // parameters:
        //      strAction   动作。为"new" "change" "delete" "onlydeletebiblio"之一。"delete"在删除书目记录的同时，会自动删除下属的实体记录。不过要求实体均未被借出才能删除。
        // return:
        //      -2  登录不成功
        //      -1  出错
        //      0   成功
        public static int UpdateRecord(
            DigitalPlatform.Stop stop,
            string strUrl,
            string strAuthString,
            string strAction,
            string strRecPath,
            string strFormat,
            string strRecord,
            string strTimestamp,
            out string strOutputRecPath,
            out string strOutputTimestamp,
            out string strError)
        {
            strError = "";
            strOutputRecPath = "";
            strOutputTimestamp = "";

            EndpointAddress address = new EndpointAddress(strUrl);
            UnionCatalogServiceClient client = new UnionCatalogServiceClient(CreateBasicHttpBinding0(), address);

            try
            {
                IAsyncResult soapresult = client.BeginUpdateRecord(
                    strAuthString,
                    strAction,
                    strRecPath,
                    strFormat,
                    strRecord,
                    strTimestamp,
                    null,
                    null);
                for (; ; )
                {
                    bool bRet = DoIdle(stop); // 出让控制权，避免CPU资源耗费过度
                    if (bRet == true)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    if (soapresult.IsCompleted)
                        break;
                }

                return client.EndUpdateRecord(
                    out strOutputRecPath,
                    out strOutputTimestamp,
                    out strError,
                    soapresult);
            }
            catch (Exception ex)
            {
                strError = ConvertWebError(ex, strUrl);
                return -1;
            }
        }

        static bool DoIdle(Stop stop)
        {
            System.Threading.Thread.Sleep(1);	// 避免CPU资源过度耗费

            Application.DoEvents();	// 出让界面控制权
            if (stop != null && stop.State != 0)
                return true;

            System.Threading.Thread.Sleep(1);	// 避免CPU资源过度耗费
            return false;
        }

        static string ConvertWebError(Exception ex0,
            string strUrl)
        {
            if (ex0 is EndpointNotFoundException)
            {
                EndpointNotFoundException ex = (EndpointNotFoundException)ex0;
                return "服务器 " + strUrl + " 没有响应";
            }

            return GetExceptionMessage(ex0);
        }

        static string GetExceptionMessage(Exception ex)
        {
            string strResult = ex.GetType().ToString() + ":" + ex.Message;
            while (ex != null)
            {
                if (ex.InnerException != null)
                    strResult += "\r\n" + ex.InnerException.GetType().ToString() + ": " + ex.InnerException.Message;

                ex = ex.InnerException;
            }

            return strResult;
        }

        public static System.ServiceModel.Channels.Binding CreateBasicHttpBinding0()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.None;
            binding.MaxReceivedMessageSize = 1024 * 1024;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            binding.SendTimeout = new TimeSpan(0, 2, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 2, 0);

            return binding;
        }
    }
}
