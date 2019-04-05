using System;
using System.Collections.Generic;
using System.Text;

using DigitalPlatform.OldZ3950;

namespace dp2Catalog
{
    /// <summary>
    /// 各种检索窗的公共抽象接口
    /// 便于外部通过一致的接口获取数据
    /// </summary>
    public interface ISearchForm
    {
        string CurrentProtocol
        {
            get;
        }

        string CurrentResultsetPath
        {
            get;
        }

        // 获得一条MARC/XML记录
        // parameters:
        //      index   注意，可能在调用后发现为需要跳过的分割条位置
        // return:
        //      -1  error
        //      0   suceed
        //      1   为诊断记录
        //      2   分割条，需要跳过这条记录
        int GetOneRecord(
            string strStyle,
            int nTest,  // 暂时使用
            string strPathParam, // int index,
            string strParameters,   // bool bHilightBrowseLine,
            out string strSavePath,
            out string strMARC,
            out string strXmlFragment,
            out string strOutStyle,
            out byte[] baTimestamp,
            out long lVersion,
            out DigitalPlatform.OldZ3950.Record record,
            out Encoding currrentEncoding,
            out LoginInfo logininfo, 
            out string strError);

        // 刷新一条MARC记录
        // parameters:
        //      strAction   refresh / delete
        // return:
        //      -2  不支持
        //      -1  error
        //      0   相关窗口已经销毁，没有必要刷新
        //      1   已经刷新
        //      2   在结果集中没有找到要刷新的记录
        int RefreshOneRecord(
            string strPathParam,
            string strAction,
            out string strError);

        // 对象、窗口是否还有效?
        bool IsValid();

        // TODO: 回调函数，里面可以出现对话框询问
        // 同步一条 MARC/XML 记录
        // 如果 Lversion 比检索窗中的记录新，则用 strMARC 内容更新检索窗内的记录
        // 如果 lVersion 比检索窗中的记录旧(也就是说 Lverion 的值偏小)，那么从 strMARC 中取出记录更新到记录窗
        // parameters:
        //      lVersion    [in]记录窗的 Version [out] 检索窗的记录 Version
        // return:
        //      -1  出错
        //      0   没有必要更新
        //      1   已经更新到 检索窗
        //      2   需要从 strMARC 中取出内容更新到记录窗
        int SyncOneRecord(string strPath,
            ref long lVersion,
            ref string strSyntax,
            ref string strMARC,
            out string strError);
    }

    public class LoginInfo
    {
        public string UserName = "";
        public string Password = "";
    }
}
