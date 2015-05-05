using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;
using System.Diagnostics;
using System.Windows.Forms;

using DigitalPlatform.GcatClient.gcat_new_ws;

namespace DigitalPlatform.GcatClient
{
    public class GcatNew
    {
        public static GcatServiceClient CreateChannel(string strUrl)
        {
            EndpointAddress address = new EndpointAddress(strUrl);
            GcatServiceClient client = new GcatServiceClient(CreateBasicHttpBinding0(), address);
            return client;
        }

        // 包装后的版本
        public static int SplitHanzi(
    DigitalPlatform.Stop stop,
    string strUrl,
    string strID,
    string strText,
    out string[] tokens,
    out string strError)
        {
            GcatServiceClient client = CreateChannel(strUrl);
            try
            {
                return SplitHanzi(stop,
                    client,
                    strID,
                    strText,
                    out tokens,
                    out strError);
            }
            finally
            {
                client.Close();
            }
        }

        // 内部调用
        // return:
        //      -2  strID验证失败
        //      -1  出错
        //      0   成功
        public static int SplitHanzi(
            DigitalPlatform.Stop stop,
            GcatServiceClient client,
            string strID,
            string strText,
            out string [] tokens,
            out string strError)
        {
            strError = "";
            tokens = null;

            try
            {
                IAsyncResult soapresult = client.BeginSplitHanzi(
                    strID,
                    strText,
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

                return client.EndSplitHanzi(
                    out tokens,
                    out strError,
                    soapresult);
            }
            catch (Exception ex)
            {
                strError = ConvertWebError(ex, client.Endpoint.Address.Uri.ToString());
                return -1;
            }
        }

        // 包装后的版本
        public static int GetPinyin(
    DigitalPlatform.Stop stop,
    string strUrl,
    string strID,
    string strText,
    out string strPinyinXml,
    out string strError)
        {
            GcatServiceClient client = CreateChannel(strUrl);
            try
            {
                return GetPinyin(stop,
                    client,
                    strID,
                    strText,
                    out strPinyinXml,
                    out strError);
            }
            finally
            {
                client.Close();
            }

        }

        // 内部调用
         // return:
        //      -2  strID验证失败
        //      -1  出错
        //      0   成功
        public static int GetPinyin(
            DigitalPlatform.Stop stop,
            GcatServiceClient client,
            string strID,
            string strText,
            out string strPinyinXml,
            out string strError)
        {
            strError = "";
            strPinyinXml = "";

            try
            {
                IAsyncResult soapresult = client.BeginGetPinyin(
                    strID,
                    strText,
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

                return client.EndGetPinyin(
                    out strPinyinXml,
                    out strError,
                    soapresult);
            }
            catch (Exception ex)
            {
                strError = ConvertWebError(ex, client.Endpoint.Address.Uri.ToString());
                return -1;
            }
        }

        // 包装后的版本
        public static int SetPinyin(
DigitalPlatform.Stop stop,
string strUrl,
string strID,
string strPinyinXml,
out string strError)
        {
            GcatServiceClient client = CreateChannel(strUrl);
            try
            {
                return SetPinyin(stop,
                    client,
                    strID,
                    strPinyinXml,
                    out strError);
            }
            finally
            {
                client.Close();
            }
        }

         // return:
        //      -2  strID验证失败
        //      -1  出错
        //      0   成功
        public static int SetPinyin(
    DigitalPlatform.Stop stop,
            GcatServiceClient client,
    string strID,
    string strPinyinXml,
    out string strError)
        {
            strError = "";

            try
            {
                IAsyncResult soapresult = client.BeginSetPinyin(
                    strID,
                    strPinyinXml,
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

                return client.EndSetPinyin(
                    out strError,
                    soapresult);
            }
            catch (Exception ex)
            {
                strError = ConvertWebError(ex, client.Endpoint.Address.Uri.ToString());
                return -1;
            }
        }

        // 内部调用
        public static int GetNumber(
            DigitalPlatform.Stop stop,
            string strUrl,
            string strID,
            string strAuthor,
            bool bSelectPinyin,
            bool bSelectEntry,
            bool bOutputDebugInfo,
            ref Question[] questions,
            out string strNumber,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strNumber = "";
            strDebugInfo = "";

            EndpointAddress address = new EndpointAddress(strUrl);
            GcatServiceClient client = new GcatServiceClient(CreateBasicHttpBinding0(), address);

            try
            {
                IAsyncResult soapresult = client.BeginGetNumber(
                    strID,
                    strAuthor,
                    bSelectPinyin,
                    bSelectEntry,
                    bOutputDebugInfo,
                    ref questions,
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

                return client.EndGetNumber(
                    ref questions,
                    out strNumber,
                    out strDebugInfo,
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

        // 外部调用
        // 特殊版本，具有缓存问题和答案的功能
        // return:
        //      -2  strID验证失败
        //      -1  error
        //      0   canceled
        //      1   succeed
        public static int GetNumber(
            ref Hashtable question_table,
            Stop stop,
            System.Windows.Forms.IWin32Window parent,
            string strUrl,
            string strID,
            string strAuthor,
            bool bSelectPinyin,
            bool bSelectEntry,
            bool bOutputDebugInfo,
            out string strNumber,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";
            strNumber = "";

            int nRet = 0;

            Question[] questions = (Question[])question_table[strAuthor];
            if (questions == null)
                questions = new Question[0];

            for (; ; )
            {
                // 这个函数具有catch 通讯中 exeption的能力
                // return:
                //		-3	需要回答问题
                //      -2  strID验证失败
                //      -1  出错
                //      0   成功
                nRet = GetNumber(
                    stop,
                    strUrl,
                    strID,
                    strAuthor,
                    bSelectPinyin,
                    bSelectEntry,
                    bOutputDebugInfo,
                    ref questions,
                    out strNumber,
                    out strDebugInfo,
                    out strError);
                if (nRet != -3)
                    break;

                Debug.Assert(nRet == -3, "");

                string strTitle = strError;

                string strQuestion = questions[questions.Length - 1].Text;

                QuestionDlg dlg = new QuestionDlg();
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.label_messageTitle.Text = strTitle;
                dlg.textBox_question.Text = strQuestion.Replace("\n", "\r\n");
                dlg.ShowDialog(parent);

                if (dlg.DialogResult != DialogResult.OK)
                {
                    strError = "放弃";
                    return 0;
                }

                questions[questions.Length - 1].Answer = dlg.textBox_result.Text;

                question_table[strAuthor] = questions;  // 保存
            }

            if (nRet == -1)
                return -1;
            if (nRet == -2)
                return -2;  // strID验证失败

            return 1;
        }

        // 外部调用
        // return:
        //      -2  strID验证失败
        //      -1  error
        //      0   canceled
        //      1   succeed
        public static int GetNumber(
            Stop stop,
            System.Windows.Forms.IWin32Window parent,
            string strUrl,
            string strID,
            string strAuthor,
            bool bSelectPinyin,
            bool bSelectEntry,
            bool bOutputDebugInfo,
            out string strNumber,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";
            strNumber = "";

            int nRet = 0;


            Question [] questions = new Question[0];

            for (; ; )
            {
                // 这个函数具有catch 通讯中 exeption的能力
                 // return:
                //		-3	需要回答问题
                //      -2  strID验证失败
                //      -1  出错
                //      0   成功
                nRet = GetNumber(
                    stop,
                    strUrl,
                    strID,
                    strAuthor,
                    bSelectPinyin,
                    bSelectEntry,
                    bOutputDebugInfo,
                    ref questions,
                    out strNumber,
                    out strDebugInfo,
                    out strError);
                if (nRet != -3)
                    break;

                Debug.Assert(nRet == -3, "");

                string strTitle = strError;

                string strQuestion = questions[questions.Length - 1].Text;

                QuestionDlg dlg = new QuestionDlg();
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.label_messageTitle.Text = strTitle;
                dlg.textBox_question.Text = strQuestion.Replace("\n", "\r\n");
                dlg.ShowDialog(parent);

                if (dlg.DialogResult != DialogResult.OK)
                {
                    strError = "放弃";
                    return 0;
                }

                questions[questions.Length - 1].Answer = dlg.textBox_result.Text;
            }

            if (nRet == -1)
                return -1;
            if (nRet == -2)
                return -2;  // strID验证失败

            return 1;
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
