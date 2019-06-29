using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Interfaces
{
    /// <summary>
    /// 生物识别接口
    /// </summary>
    public interface IBioRecognition
    {
#if NO
        int Open(
            ref string strVersion,
            out string strError);

        int Close();
#endif

        // 添加高速缓存事项
        // 如果items == null 或者 items.Count == 0，表示要清除当前的全部缓存内容
        // 如果一个item对象的FingerprintString为空，表示要删除这个缓存事项
        NormalResult AddItems(List<BioFeatureItem> items);

        // 2.0 增加的函数
        // 获得一个生物识别特征字符串
        // parameters:
        //      imageData   [in] 要提取特征的源图像数据。如果为 null，表示使用 xxxCenter 自动拍摄的图象
        // return:
        //      -1  error
        //      0   放弃输入
        //      1   成功输入
        GetFeatureStringResult GetFeatureString(
            byte[] imageData,
            string strExcludeBarcodes,
            string strStyle);

        // 取消正在进行的 GetFingerprintString() 操作
        NormalResult CancelGetFeatureString();

        RecognitionFaceResult RecognitionFace(string strStyle);

        // 2019/6/27
        NormalResult CancelRecognitionFace();

        // 2019/6/27
        GetImageResult GetImage(string strStyle);

        // 验证读者指纹. 1:1比对
        // parameters:
        //      item    读者信息。ReaderBarcode成员提供了读者证条码号，FingerprintString提供了指纹特征码
        //              如果 FingerprintString 不为空，则用它和当前采集的指纹进行比对；
        //              否则用 ReaderBarcode，对高速缓存中的指纹进行比对
        // return:
        //      -1  出错
        //      0   不匹配
        //      1   匹配
        NormalResult VerifyFeature(BioFeatureItem item);

        // 设置参数
        // bool SetParameter(string strName, object value);

        NormalResult EnableSendKey(bool enable);

        NormalResult GetState(string style);

        NormalResult ActivateWindow();

        // 事件通知
        NormalResult Notify(string event_name);
    }

    [Serializable()]
    public class BioFeatureItem
    {
        // 生物特征类型。fingerprint/face
        public string Type { get; set; }
        // 生物特征字符串
        public string FeatureString { get; set; }
        // 读者证条码号
        public string PatronID { get; set; }
    }

    [Serializable()]
    public class GetFeatureStringResult : NormalResult
    {
        // [out] 返回特征字符串。通常是一个 base64 的字符串，包装了 byte [] 内容 
        public string FeatureString { get; set; }
        // [out] 返回特征算法版本
        public string Version { get; set; }

        // [out] 返回读者照片
        public byte [] ImageData { get; set; }
    }

    [Serializable()]
    public class RecognitionFaceResult : NormalResult
    {
        // [out] 证条码号
        public string Patron { get; set; }
        // [out] 分数。0-100
        public int Score { get; set; }

        public string DebugInfo { get; set; }
    }

    [Serializable()]
    public class GetImageResult : NormalResult
    {
        // [out] 返回图象
        public byte[] ImageData { get; set; }
    }
}
