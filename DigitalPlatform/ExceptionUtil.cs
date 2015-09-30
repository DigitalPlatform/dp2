using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;


namespace DigitalPlatform
{
    public class ExceptionUtil
    {
        // 2015/9/30
        // 如果 Exceptoin 为 NullException 类型，则返回详细调用堆栈；否则只返回 e.Message 信息
        public static string GetAutoText(Exception e)
        {
            if (e is NullReferenceException)
                return GetDebugText(e);
            return e.Message;
        }

        // 返回详细调用堆栈
        public static string GetDebugText(Exception e)
        {
            StringBuilder message = new StringBuilder();
               
                /*
                new StringBuilder("\r\n\r\nUnhandledException:\r\n\r\nappId=");

            string appId = (string)AppDomain.CurrentDomain.GetData(".appId");
            if (appId != null)
            {
                message.Append(appId);
            }*/

            Exception currentException = null;
            for (currentException = e; currentException != null; currentException = currentException.InnerException)
            {
                message.AppendFormat("Type: {0}\r\nMessage: {1}\r\nStack:\r\n{2}\r\n\r\n",
                                     currentException.GetType().FullName,
                                     currentException.Message,
                                     currentException.StackTrace);
            }

            return message.ToString();
        }


        /*
        public static string GetDetailDebugText(Exception e)
        {
            StringBuilder message = new StringBuilder();

            Exception currentException = null;
            for (currentException = e; currentException != null; currentException = currentException.InnerException)
            {
                message.AppendFormat("Type: {0}\r\nMessage: {1}\r\nStack:\r\n{2}\r\nData:{3}\r\n\r\n",
                                     currentException.GetType().FullName,
                                     currentException.Message,
                                     currentException.StackTrace,
                                     currentException.Data);
            }

            return message.ToString();
        }*/

        public static string GetStackTraceText(StackTrace st)
        {
            string strText = "";
            StackFrame[] frames = st.GetFrames();

            for (int i = 0; i < frames.Length; i++)
            {
                StackFrame frame = frames[i];
                strText += frame.ToString() + "\r\n";
            }

            return strText;
        }

        public static string GetExceptionMessage(Exception ex)
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
    }
}
