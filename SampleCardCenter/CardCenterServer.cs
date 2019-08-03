using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Interfaces;
using DigitalPlatform.Text;

namespace SampleCardCenter
{
    /// <summary>
    /// .NET Remoting Server。扮演接口的服务器角色。
    /// </summary>
    public class CardCenterServer : MarshalByRefObject, ICardCenter
    {
        /*
        // parameters:
        //      strRecord   [in] 读者XML记录，或读者证条码号。
        //                  如果是读者 XML 记录，第一字符是 '<'，函数内代码可以据此进行判断。读者 XML 记录里面包含足够的标识字段即可，不要求包含所有字段
        //      strPriceString  [out] 金额字符串。一般为类似“CNY12.00”这样的形式
        //      strRest    [out] 扣款后的余额
         * */
        /// <summary>
        /// 从卡中心扣款
        /// </summary>
        /// <param name="strRecord">账户标识，一般为读者 barcode </param>
        /// <param name="strPriceString">扣款金额字符串,单位：元。一般为类似“CNY12.00”这样的形式</param>
        /// <param name="strPassword">账户密码</param>
        /// <param name="strRest">扣款后的余额，单位：元，格式为 货币符号+金额数字部分，形如“CNY100.00”表示人民币 100元</param>
        /// <param name="strError">错误信息</param>
        /// <returns>
        /// <para>-2  密码不正确</para>
        /// <para>-1  出错(调用出错等特殊原因)</para>
        /// <para>0   扣款不成功(因为余额不足等普通原因)。注意 strError 中应当返回不成功的原因</para>
        /// <para>1   扣款成功</para>
        /// </returns>
        public int Deduct(string strRecord,
            string strPriceString,
            string strPassword,
            out string strRest,
            out string strError)
        {
            strRest = "";
            strError = "";

            if (strRecord.Trim().Substring(0, 1) == "<")
            {
                strError = "参数'strRecord'值不正确，不是正确的卡号";
                return -1;
            }

            // 第一个流程 扣款金额字符串 为空，需返回 账户余额
            if (String.IsNullOrEmpty(strPriceString) == true)
            {
                strRest = "";
                return 1;
            }

            // 处理带前缀的价格字符串
            string strPrefix = ""; // 金额字符串前缀
            string strValue = ""; // 金额字符串数字部分
            string strPostfix = ""; // 金额字符串后缀
            int nRet = PriceUtil.ParsePriceUnit(strPriceString,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return -1;

            // TODO: 这里应该加入真实的扣款代码。
            // 一般是要访问卡中心的 SQL 数据库，找到该读者的记录，对金额字段进行扣款修改，然后返回扣款后账户余额
            // 可用的参数：经过前面的准备，这里 strPrefix 内容是货币代码；strValue 内容是要扣款的数字金额值。strRecord 内容是读者证条码号；strPassword 是读者(在前端扣款界面)输入的密码

            // 扣款成功，返回扣款后的余额。注意这只是一行简单的示范代码
            strRest = strPrefix + "0.00";
            return 1;
        }

        // (dp2 系统目前暂未用到此函数)
        // 获得一条读者记录
        // parameters:
        //      strID   读者记录标识符号。用什么字段作为标识，Client和Server需要另行约定
        //      strRecord   读者XML记录
        //                  注：读者记录中的某些字段卡中心可能缺乏对应字段，那么需要在XML记录中填入 <元素名 dprms:missing />，这样不至于造成同步时图书馆读者库中的这些字段被清除。至于读者借阅信息等字段，则不必操心
        // return:
        //      -1  出错(调用出错等特殊情况)
        //      0   读者记录不存在(读者记录正常性不存在)
        //      1   成功返回读者记录
        public int GetPatronRecord(string strID,
            out string strRecord,
            out string strError)
        {
            throw new NotImplementedException();
        }


        // 获得若干条读者记录
        // parameters:
        //      strPosition [in,out]第一次调用前，需要将此参数的值清为空。然后函数会在其中返回值，调用者不要破坏其内容。
        //      records [out] 读者XML记录字符串数组。注：读者记录中的某些字段卡中心可能缺乏对应字段，那么需要在XML记录中填入 <元素名 dprms:missing />，这样不至于造成同步时图书馆读者库中的这些字段被清除。至于读者借阅信息等字段，则不必操心
        // return:
        //      -1  出错
        //      0   正常获得一批记录，但是尚未获得全部
        //      1   正常获得最后一批记录
        public int GetPatronRecords(ref string strPosition,
            out string[] records,
            out string strError)
        {
            strError = "";
            records = null;

            List<string> results = new List<string>();

            // 首次调用本函数
            if (string.IsNullOrEmpty(strPosition))
            {
                results.Add("<root><barcode>R0000001</barcode><name>姓名1</name></root>");
                results.Add("<root><barcode>R0000002</barcode><name>姓名2</name></root>");
                records = results.ToArray();
                strPosition = "2";
                return 0;
            }

            // 第二次调用本函数
            if (strPosition == "2")
            {
                results.Add("<root><barcode>R0000003</barcode><name>姓名3</name></root>");
                results.Add("<root><barcode>R0000004</barcode><name>姓名4</name></root>");
                records = results.ToArray();
                strPosition = "4";
                return 0;
            }

            // 第三次调用本函数
            if (strPosition == "4")
            {
                results.Add("<root><barcode>R0000005</barcode><name>姓名5</name></root>");
                results.Add("<root><barcode>R0000006</barcode><name>姓名6</name></root>");
                records = results.ToArray();
                strPosition = "finish";
                return 1;   // 表示这是最后一批记录
            }

            strError = "strPosition 参数值不符合要求";
            return -1;
        }
    }

}
