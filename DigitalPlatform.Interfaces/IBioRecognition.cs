using System;
using System.Collections.Generic;
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
        // 获得一个指纹特征字符串
        // return:
        //      -1  error
        //      0   放弃输入
        //      1   成功输入
        GetFeatureStringResult GetFeatureString(
            string strExcludeBarcodes,
            string strStyle);

        // 取消正在进行的 GetFingerprintString() 操作
        NormalResult CancelGetFeatureString();

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
        public string FeatureString { get; set; }
        public string Version { get; set; }
    }
}
