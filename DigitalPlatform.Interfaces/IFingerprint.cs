using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;

using DigitalPlatform;

namespace DigitalPlatform.Interfaces
{
    /// <summary>
    /// 指纹阅读器接口
    /// </summary>
    public interface IFingerprint
    {
        event MessageArrivedEvent MessageArrived;

        int Open(out string strError);

        int Close();

        // 2.0 增加的函数
        int GetVersion(out string strVersion,
            out string strCfgInfo,
            out string strError);

        // 添加高速缓存事项
        // 如果items == null 或者 items.Count == 0，表示要清除当前的全部缓存内容
        // 如果一个item对象的FingerprintString为空，表示要删除这个缓存事项
        int AddItems(List<FingerprintItem> items,
            out string strError);

        // 获得一个指纹特征字符串
        // return:
        //      -1  error
        //      0   放弃输入
        //      1   成功输入
        int GetFingerprintString(out string strFingerprintString,
            out string strVersion,
            out string strError);

        // 2.0 增加的函数
        // 获得一个指纹特征字符串
        // return:
        //      -1  error
        //      0   放弃输入
        //      1   成功输入
        int GetFingerprintString(
            string strExcludeBarcodes,
            out string strFingerprintString,
            out string strVersion,
            out string strError);

        // 取消正在进行的 GetFingerprintString() 操作
        int CancelGetFingerprintString();

        // 验证读者指纹. 1:1比对
        // parameters:
        //      item    读者信息。ReaderBarcode成员提供了读者证条码号，FingerprintString提供了指纹特征码
        //              如果 FingerprintString 不为空，则用它和当前采集的指纹进行比对；
        //              否则用 ReaderBarcode，对高速缓存中的指纹进行比对
        // return:
        //      -1  出错
        //      0   不匹配
        //      1   匹配
        int VerifyFingerprint(FingerprintItem item,
            out string strError);
#if NO
        int ReadInput(out string strBarcode,
            out string strError);
#endif

        // 设置参数
        bool SetParameter(string strName, object value);

        GetMessageResult GetMessage(string style);

        // event MessageHandler DisplayMessage;

        NormalResult EnableSendKey(bool enable);

        NormalResult GetState(string style);

        NormalResult ActivateWindow();
    }

    [Serializable()]
    public class GetMessageResult : NormalResult
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()},Message={Message}";
        }
    }

    [Serializable()]
    public class FingerprintItem
    {
        // 指纹特征字符串
        public string FingerprintString = "";
        // 读者证条码号
        public string ReaderBarcode = "";
    }

    public delegate void MessageHandler(object sender,
        MessageEventArgs e);

    /// <summary>
    /// 空闲事件的参数
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        public string Message = "";
    }

#if NO
    [Serializable()]
    public delegate void ScanedEventHandler(object sender,
ScanedEventArgs e);

    /// <summary>
    /// 
    /// </summary>
    [Serializable()]
    public class ScanedEventArgs : EventArgs
    {
        public string Text { get; set; }
    }
#endif

#if NO
    public abstract class MyDelegateObject : MarshalByRefObject
    {
        public void EventHandlerCallback(object sender, ScanedEventArgs e)
        {
            EventHandlerCallbackCore(sender, e);
        }

        protected abstract void EventHandlerCallbackCore(object sender, ScanedEventArgs e);

        public override object InitializeLifetimeService() { return null; }
    }
#endif

    public delegate void MessageArrivedEvent(string Message);

    public class EventProxy : MarshalByRefObject
    {

        #region Event Declarations

        public event MessageArrivedEvent MessageArrived;

        #endregion

        #region Lifetime Services

        public override object InitializeLifetimeService()
        {
            return null;
            //Returning null holds the object alive
            //until it is explicitly destroyed
        }

        #endregion

        #region Local Handlers

        public void LocallyHandleMessageArrived(string Message)
        {
            if (MessageArrived != null)
                MessageArrived(Message);
        }

        #endregion

    }
}
