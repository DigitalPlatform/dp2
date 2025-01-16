using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2SSL.SIP2
{
    public class SIPUtility
    {


        //统一用一个专门的LogManager
        //// public static ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //public static ILog Logger = log4net.LogManager.GetLogger("dp2SIPLogging");
        //public static void WriteLog(string message)
        //{
        //    Logger.Info(message);
        //}

        /// <summary>
        /// 将消息字符串 解析 成对应的消息对象
        /// </summary>
        /// <param name="cmdText">消息字符串</param>
        /// <param name="message">解析后的消息对象</param>
        /// <param name="error">氏族</param>
        /// <returns>
        /// true 成功
        /// false 出错
        /// </returns>
        public static int ParseMessage(string cmdText, out BaseMessage message, out string error)
        {
            message = new BaseMessage();
            error = "";

            if (cmdText.Length < 2)
            {
                error = "命令长度不够2位";
                return -1;
            }

            string cmdIdentifiers = cmdText.Substring(0, 2);
            //text = text.Substring(2);
            switch (cmdIdentifiers)
            {
                case "93":
                    {
                        message = new Login_93();
                        break;
                    }
                case "94":
                    {
                        message = new LoginResponse_94();
                        break;
                    }
                case "99":
                    {
                        message = new SCStatus_99();
                        break;
                    }
                case "98":
                    {
                        message = new ACSStatus_98();
                        break;
                    }
                case "11":
                    {
                        message = new Checkout_11();
                        break;
                    }
                case "12":
                    {
                        message = new CheckoutResponse_12();
                        break;
                    }
                case "09":
                    {
                        message = new Checkin_09();
                        break;
                    }
                case "10":
                    {
                        message = new CheckinResponse_10();
                        break;
                    }
                case "63":
                    {
                        message = new PatronInformation_63();
                        break;
                    }
                case "64":
                    {
                        message = new PatronInformationResponse_64();
                        break;
                    }
                case "35":
                    {
                        message = new EndPatronSession_35();
                        break;
                    }
                case "36":
                    {
                        message = new EndSessionResponse_36();
                        break;
                    }
                case "17":
                    {
                        message = new ItemInformation_17();
                        break;
                    }
                case "18":
                    {
                        message = new ItemInformationResponse_18();
                        break;
                    }
                case "29":
                    {
                        message = new Renew_29();
                        break;
                    }
                case "30":
                    {
                        message = new RenewResponse_30();
                        break;
                    }
                case "65":
                    {
                        message = new RenewAll_65();
                        break;
                    }
                case "66":
                    {
                        message = new RenewAllResponse_66();
                        break;
                    }
                case "37":
                    {
                        message = new FeePaid_37();
                        break;
                    }
                case "38":
                    {
                        message = new FeePaidResponse_38();
                        break;
                    }
                case "97":
                    {
                        message = new RequestACSResend_97();
                        break;
                    }
                case "96":
                    {
                        message = new RequestSCResend_96();
                        break;
                    }
                case "23":
                    {
                        message = new PatronStatusRequest_23();
                        break;
                    }
                case "24":
                    {
                        message = new PatronStatusResponse_24();
                        break;
                    }
                case "25":
                    {
                        message = new PatronEnable_25();
                        break;
                    }
                case "26":
                    {
                        message = new PatronEnableResponse_26();
                        break;
                    }
                case "01":
                    {
                        message = new BlockPatron_01();
                        break;
                    }
                case "15":
                    {
                        message = new Hold_15();
                        break;
                    }
                case "16":
                    {
                        message = new HoldResponse_16();
                        break;
                    }
                case "19":
                    {
                        message = new ItemStatusUpdate_19();
                        break;
                    }
                case "20":
                    {
                        message = new ItemStatusUpdateResponse_20();
                        break;
                    }
                case "41":
                    {
                        message = new ChannelInformation_41();
                        break;
                    }
                case "42":
                    {
                        message = new ChannelInformationResponse_42();
                        break;
                    }
                default:
                    error = "不支持的命令'" + cmdIdentifiers + "'";
                    return -1;
            }

            return message.Parse(cmdText, out error);
        }



        #region 通用函数

        /// <summary>
        /// 当前时间
        /// </summary>
        public static string NowDateTime
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMdd    HHmmss");

            }
        }



        #endregion

    }

}
