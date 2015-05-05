using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.Interfaces
{
    /// <summary>
    /// 指纹阅读器接口
    /// </summary>
    public interface IFingerprint
    {
        int Open(out string strError);

        int Close();

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


        // event MessageHandler DisplayMessage;
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
}
