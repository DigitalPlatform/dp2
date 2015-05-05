using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.Interfaces
{
    /// <summary>
    /// 卡中心接口
    /// </summary>
    public interface ICardCenter
    {
        // 获得若干读者记录
        // parameters:
        //      strPosition 第一次调用前，需要将此参数的值清为空
        //      records 读者XML记录字符串数组。注：读者记录中的某些字段卡中心可能缺乏对应字段，那么需要在XML记录中填入 <元素名 dprms:missing />，这样不至于造成同步时图书馆读者库中的这些字段被清除。至于读者借阅信息等字段，则不必操心
        // return:
        //      -1  出错
        //      0   正常获得一批记录，但是尚未获得全部
        //      1   正常获得最后一批记录
        int GetPatronRecords(ref string strPosition,
            out string[] records,
            out string strError);

        // 获得一条读者记录
        // parameters:
        //      strID   读者记录标识符号。用什么字段作为标识，Client和Server需要另行约定
        //      strRecord   读者XML记录
        //                  注：读者记录中的某些字段卡中心可能缺乏对应字段，那么需要在XML记录中填入 <元素名 dprms:missing />，这样不至于造成同步时图书馆读者库中的这些字段被清除。至于读者借阅信息等字段，则不必操心
        // return:
        //      -1  出错(调用出错等特殊情况)
        //      0   读者记录不存在(读者记录正常性不存在)
        //      1   成功返回读者记录
        int GetPatronRecord(string strID,
            out string strRecord,
            out string strError);

        // 从卡中心扣款
        // parameters:
        //      strRecord   读者XML记录。里面包含足够的表示字段即可
        //      strPriceString  金额字符串。一般为类似“CNY12.00”这样的形式
        //      strRest    扣款后的余额
        // return:
        //      -2  密码不正确
        //      -1  出错(调用出错等特殊原因)
        //      0   扣款不成功(因为余额不足等普通原因)。注意strError中应当返回不成功的原因
        //      1   扣款成功
        int Deduct(string strRecord,
            string strPriceString,
            string strPassword, // new
            out string strRest, // new
            out string strError);

    }
}
