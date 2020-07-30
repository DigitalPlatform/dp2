using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.SIP2;
using DigitalPlatform.SIP2.Request;
using DigitalPlatform.SIP2.Response;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    public class SipChannel
    {
        private TcpClient _client = null;
        private NetworkStream _networkStream = null;


        #region 一些配置参数
        // 字符集
        public Encoding Encoding { get; set; }
        // 命令结束符
        // public char MessageTerminator { get; set; }

        // SIP Server Url 与 Port
        public string SIPServerUrl { get; set; }
        public int SIPServerPort { get; set; }

        #endregion

        public SipChannel()
        {
            this.SetDefaultParameter();
        }

        public SipChannel(TcpClient client)
        {
            this._client = client;
            this._networkStream = client.GetStream();
            this.SetDefaultParameter();
        }

        public SipChannel(Encoding encoding)
        {
            this.Encoding = encoding;
        }

        // 设置缺省参数;
        public void SetDefaultParameter()
        {
            this.Encoding = Encoding.UTF8;
            // this.MessageTerminator = (char)13;
        }

        // 2020/7/27
        public bool Connected
        {
            get
            {
                if (this._client == null)
                    return false;

                return this._client.Connected;
            }
        }

        // result.Value
        //      -1  失败
        //      0   成功
        public async Task<NormalResult> ConnectionAsync(string serverUrl,
            int port)
        {
            this.SIPServerUrl = serverUrl;
            this.SIPServerPort = port;

            // 先进行关闭
            this.Close();
            try
            {
                // 这段代码当ip地址对应的服务器没有对应域名时，会抛异常。例如腾讯云的几台服务器
                // IPAddress ipAddress = IPAddress.Parse(this.SIPServerUrl);
                // string hostName = Dns.GetHostEntry(ipAddress).HostName;
                // TcpClient client = new TcpClient(hostName, this.SIPServerPort); 

                TcpClient client = new TcpClient();
                await client.ConnectAsync(this.SIPServerUrl, // IPAddress.Parse(this.SIPServerUrl),
                    this.SIPServerPort);
                this._client = client;
                this._networkStream = client.GetStream();
                return new NormalResult();
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "连接服务器失败:" + ex.Message,
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }


        public bool Connection(string serverUrl, int port, out string error)
        {
            error = "";

            this.SIPServerUrl = serverUrl;
            this.SIPServerPort = port;

            // 先进行关闭
            this.Close();

            try
            {
                // 这段代码当ip地址对应的服务器没有对应域名时，会抛异常。例如腾讯云的几台服务器
                // IPAddress ipAddress = IPAddress.Parse(this.SIPServerUrl);
                // string hostName = Dns.GetHostEntry(ipAddress).HostName;
                // TcpClient client = new TcpClient(hostName, this.SIPServerPort); 

                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse(this.SIPServerUrl), this.SIPServerPort);
                this._client = client;
                this._networkStream = client.GetStream();
            }
            catch (Exception ex)
            {
                error = "连接服务器失败:" + ex.Message;
                return false;
            }

            return true;
        }

        // 关闭通道
        public void Close()
        {
            if (_networkStream != null)
            {
                //您必须先关闭 NetworkStream 您何时通过发送和接收数据。 关闭 TcpClient 不会释放 NetworkStream。
                _networkStream.Close();
                _networkStream = null;
                WriteInfoLog("关闭NetworkStream完成");
            }

            if (_client != null)
            {
                /*
                TCPClient.Close 释放此 TcpClient 实例本身，并请求关闭基础 TCP 连接（被封装的Socket）。
                TCPClient.Client.Close 关闭 被封装的Socket 连接并释放所有关联此Socket的资源。
                TCPClient.Client.Shutdown 禁用被封装的 Socket 上的发送和接收。
                 */

                //this._client.Client.Shutdown(SocketShutdown.Both);
                //this._client.Client.Close(); //
                //LogManager.Logger.Info("关闭TcpClient封装的Socket连接完成");

                this._client.Close();
                WriteInfoLog("关闭TcpClient实例连接完成");

                this._client = null;
            }
        }

        // 发送消息
        public async Task<NormalResult> SendMessageAsync(string sendMsg)
        {
            if (this._client == null)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "_client对象不能为null。"
                };
            }

            if (this._networkStream == null)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "_networkStream对象不能为null。"
                };
            }

            try
            {
                if (this._networkStream.DataAvailable == true)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "异常：发送前发现流中有未读的数据!"
                    };
                }

                // 2018/7/28
                {
                    char tail_char = sendMsg[sendMsg.Length - 1];
                    if (tail_char != '\r' && tail_char != '\n')
                        sendMsg += '\r';
                }

                byte[] baPackage = this.Encoding.GetBytes(sendMsg);

                await this._networkStream.WriteAsync(baPackage, 0, baPackage.Length);
                await this._networkStream.FlushAsync();//刷新当前数据流中的数据
                return new NormalResult();
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }


        // 发送消息
        public int SendMessage(string sendMsg,
            out string error)
        {
            error = "";

            if (this._client == null)
            {
                error = "_client对象不能为null。";
                return -1;
            }

            if (this._networkStream == null)
            {
                error = "_networkStream对象不能为null。";
                return -1;
            }

            try
            {
                if (this._networkStream.DataAvailable == true)
                {
                    error = "异常：发送前发现流中有未读的数据!";
                    return -1;
                }

                // 2018/7/28
                {
                    char tail_char = sendMsg[sendMsg.Length - 1];
                    if (tail_char != '\r' && tail_char != '\n')
                        sendMsg += '\r';
                }

                byte[] baPackage = this.Encoding.GetBytes(sendMsg);

                this._networkStream.Write(baPackage, 0, baPackage.Length);
                this._networkStream.Flush();//刷新当前数据流中的数据
                return 0;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return -1;
            }
        }

        static Tuple<int, byte> FindTerminator(byte[] buffer, int start, int length)
        {
            for (int i = start; i < start + length; i++)
            {
                byte b = buffer[i];
                if (b == '\r' || b == '\n')
                    return new Tuple<int, byte>(i + 1, b);
            }

            return new Tuple<int, byte>(0, (byte)0);
        }

        public class RecvMessageResult : NormalResult
        {
            public string RecvMsg { get; set; }
        }

        // 接收消息
        public async Task<RecvMessageResult> RecvMessageAsync()
        {
            string error = "";
            string recvMsg = "";

            if (this._client == null)
            {
                return new RecvMessageResult
                {
                    Value = -1,
                    ErrorInfo = "_client对象不能为null。"
                };
            }

            if (this._networkStream == null)
            {
                return new RecvMessageResult
                {
                    Value = -1,
                    ErrorInfo = "_networkStream对象不能为null。"
                };
            }

            int offset = 0; //偏移量
            int nRet = 0;

            int nPackageLength = SIPConst.COMM_BUFF_LEN; //1024
            byte[] baPackage = new byte[nPackageLength];

            while (offset < nPackageLength)
            {
                if (this._client == null)
                {
                    return new RecvMessageResult
                    {
                        Value = -1,
                        ErrorInfo = "通讯中断"
                    };
                }

                try
                {
                    nRet = await this._networkStream.ReadAsync(baPackage,
                        offset,
                        baPackage.Length - offset);
                }
                catch (SocketException ex)
                {
                    // ??这个什么错误码
                    if (ex.ErrorCode == 10035)
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }

                    error = ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }
                catch (System.IO.IOException ex1)
                {
                    error = ex1.Message;
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    error = ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }

                if (nRet == 0) //返回值为0
                {
                    error = "Closed by remote peer";
                    goto ERROR1;
                }

                // 得到包的长度
                if (nRet >= 1 || offset >= 1)
                {
#if NO
                    //没有找到结束符，继续读
                    int nIndex = Array.IndexOf(baPackage, (byte)this.MessageTerminator);
                    if (nIndex != -1)
                    {
                        nPackageLength = nIndex;
                        break;
                    }
#endif

                    Tuple<int, byte> ret = FindTerminator(baPackage, offset, nRet);
                    if (ret.Item1 != 0)
                    {
                        nPackageLength = ret.Item1;
                        break;
                    }
#if NO
                    //流中没有数据了
                    if (this._networkStream.DataAvailable == false)
                    {
                        nPackageLength = offset + nRet;
                        break;
                    }
#endif
                }

                offset += nRet;
                if (offset >= baPackage.Length)
                {
                    // 扩大缓冲区
                    byte[] temp = new byte[baPackage.Length + SIPConst.COMM_BUFF_LEN];//1024
                    Array.Copy(baPackage, 0, temp, 0, offset);
                    baPackage = temp;
                    nPackageLength = baPackage.Length;
                }
            }

            // 最后规整缓冲区尺寸，如果必要的话
            if (baPackage.Length > nPackageLength)
            {
                byte[] temp = new byte[nPackageLength];
                Array.Copy(baPackage, 0, temp, 0, nPackageLength);
                baPackage = temp;
            }

            recvMsg = this.Encoding.GetString(baPackage);
            return new RecvMessageResult
            {
                Value = 0,
                RecvMsg = recvMsg
            };

        ERROR1:
            WriteErrorLog(error);
            baPackage = null;
            return new RecvMessageResult
            {
                Value = -1,
                ErrorInfo = error
            };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="recvMsg"></param>
        /// <param name="error"></param>
        /// <returns>
        /// <para> 0 正确 </para>
        /// <para> -1 错误 </para>
        /// <para> -2 空消息 </para>
        /// </returns>
        // 接收消息
        public int RecvMessage(out string recvMsg,
            out string error)
        {
            error = "";
            recvMsg = "";

            if (this._client == null)
            {
                error = "_client对象不能为null。";
                return -1;
            }

            if (this._networkStream == null)
            {
                error = "_networkStream对象不能为null。";
                return -1;
            }

            int offset = 0; //偏移量
            int nRet = 0;

            int nPackageLength = SIPConst.COMM_BUFF_LEN; //1024
            byte[] baPackage = new byte[nPackageLength];

            while (offset < nPackageLength)
            {
                if (this._client == null)
                {
                    error = "通讯中断";
                    goto ERROR1;
                }

                try
                {
                    nRet = this._networkStream.Read(baPackage,
                        offset,
                        baPackage.Length - offset);
                }
                catch (SocketException ex)
                {
                    // ??这个什么错误码
                    if (ex.ErrorCode == 10035)
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }

                    error = ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }
                catch (System.IO.IOException ex1)
                {
                    error = ex1.Message;
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    error = ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }

                if (nRet == 0) //返回值为0
                {
                    error = "Closed by remote peer";
                    goto ERROR1;
                }

                // 得到包的长度
                if (nRet >= 1 || offset >= 1)
                {
#if NO
                    //没有找到结束符，继续读
                    int nIndex = Array.IndexOf(baPackage, (byte)this.MessageTerminator);
                    if (nIndex != -1)
                    {
                        nPackageLength = nIndex;
                        break;
                    }
#endif

                    Tuple<int, byte> ret = FindTerminator(baPackage, offset, nRet);
                    if (ret.Item1 != 0)
                    {
                        nPackageLength = ret.Item1;
                        break;
                    }
#if NO
                    //流中没有数据了
                    if (this._networkStream.DataAvailable == false)
                    {
                        nPackageLength = offset + nRet;
                        break;
                    }
#endif
                }

                offset += nRet;
                if (offset >= baPackage.Length)
                {
                    // 扩大缓冲区
                    byte[] temp = new byte[baPackage.Length + SIPConst.COMM_BUFF_LEN];//1024
                    Array.Copy(baPackage, 0, temp, 0, offset);
                    baPackage = temp;
                    nPackageLength = baPackage.Length;
                }
            }

            // 最后规整缓冲区尺寸，如果必要的话
            if (baPackage.Length > nPackageLength)
            {
                byte[] temp = new byte[nPackageLength];
                Array.Copy(baPackage, 0, temp, 0, nPackageLength);
                baPackage = temp;
            }

            recvMsg = this.Encoding.GetString(baPackage);
            return 0;

        ERROR1:
            WriteErrorLog(error);
            baPackage = null;
            return -1;
        }

        public class SendAndRecvResult : RecvMessageResult
        {
            public BaseMessage Response { get; set; }

            public SendAndRecvResult()
            {

            }

            public SendAndRecvResult(NormalResult result)
            {
                Value = result.Value;
                ErrorInfo = result.ErrorInfo;
                ErrorCode = result.ErrorCode;
            }
        }

        // 发送消息，接收消息
        public async Task<SendAndRecvResult> SendAndRecvAsync(string requestText)
        {
            string error = "";
            int nRet = 0;

            // 校验消息
            BaseMessage request = null;
            nRet = SIPUtility.ParseMessage(requestText, out request, out error);
            if (nRet == -1)
            {
                return new SendAndRecvResult
                {
                    Value = -1,
                    ErrorInfo = "校验发送消息异常:" + error
                };
            }

            // 发送消息
            var send_result = await SendMessageAsync(requestText);
            if (send_result.Value == -1)
            {
                return new SendAndRecvResult(send_result);
            }

            // 接收消息
            var recv_result = await RecvMessageAsync();
            if (recv_result.Value == -1)
            {
                return new SendAndRecvResult(recv_result);
            }

            //解析返回的消息
            nRet = SIPUtility.ParseMessage(recv_result.RecvMsg, out BaseMessage response, out error);
            if (nRet == -1)
            {
                try
                {
                    dynamic p = response;
                    if (string.IsNullOrEmpty(p.AF_ScreenMessage_o) == false)
                        return new SendAndRecvResult
                        {
                            RecvMsg = recv_result.RecvMsg,
                            Response = response,
                            Value = -1,
                            ErrorInfo = p.AF_ScreenMessage_o,
                            ErrorCode = "sipError",
                        };
                }
                catch
                {

                }

                return new SendAndRecvResult
                {
                    RecvMsg = recv_result.RecvMsg,
                    Response = response,
                    Value = -1,
                    ErrorInfo = "解析返回的消息异常:" + error + "\r\n" + recv_result.RecvMsg,
                    ErrorCode = "parseError"
                };
            }

            return new SendAndRecvResult
            {
                Value = 0,
                RecvMsg = recv_result.RecvMsg,
                Response = response
            };
        }


        public static void WriteErrorLog(string text)
        {
            WpfClientInfo.WriteErrorLog(text);
        }

        public static void WriteInfoLog(string text)
        {
            WpfClientInfo.WriteInfoLog(text);
        }

        #region API

        string TransactionDate
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMdd    HHmmss");
            }
        }

#if NO
        public async Task<NormalResult> GetItemInfo(string oi,
            string barcode)
        {
            ItemInformation_17 request = new ItemInformation_17()
            {
                TransactionDate_18 = this.TransactionDate,

                AO_InstitutionId_r = oi,
                AC_TerminalPassword_o = "",
            };

            request.AB_ItemIdentifier_r = barcode;
            string cmdText = request.ToText();

            // this.Print("send:" + cmdText);
            BaseMessage response = null;
            var result = await SendAndRecvAsync(cmdText);
            if (result.Value == -1)
            {
                return result;
            }

            var response18 = response as ItemInformationResponse_18;
            if (response18 == null)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "返回的不是18消息"
                };
            }


            result.Response.;


            this.Print("recv:" + responseText);


        }
#endif

        public class LoginResult : NormalResult
        {
            public LoginResponse_94 Result { get; set; }
        }

        /*
        /// 1 登录成功
        /// 0 登录失败
        /// -1 出错
         * */
        public async Task<LoginResult> LoginAsync(string username,
            string password)
        {
            Login_93 request = new Login_93()
            {
                CN_LoginUserId_r = username,
                CO_LoginPassword_r = password,
            };
            request.SetDefaulValue();

            // 发送和接收消息
            string requestText = request.ToText();

            var result = await SendAndRecvAsync(requestText);
            if (result.Value == -1)
            {
                return new LoginResult
                {
                    Value = -1,
                    ErrorInfo = result.ErrorInfo,
                    ErrorCode = result.ErrorCode
                };
            }

            var response94 = result.Response as LoginResponse_94;
            if (response94 == null)
            {
                return new LoginResult
                {
                    Value = -1,
                    ErrorInfo = "返回的不是 94 消息"
                };
            }

            if (response94.Ok_1 == "0")
            {
                return new LoginResult
                {
                    Value = 0,
                    ErrorInfo = "登录失败",
                    Result = response94
                };
            }

            return new LoginResult
            {
                Value = 1,
                ErrorInfo = "登录成功",
                Result = response94
            };
        }

        public class ScStatusResult : NormalResult
        {
            public ACSStatus_98 Result { get; set; }
        }

        // -1出错，0不在线，1正常
        public async Task<ScStatusResult> ScStatusAsync()
        {
            SCStatus_99 request = new SCStatus_99()
            {
                StatusCode_1 = "0",
                MaxPrintWidth_3 = "030",
                ProtocolVersion_4 = "2.00",
            };

            // 发送和接收消息
            string requestText = request.ToText();

            var result = await SendAndRecvAsync(requestText);
            if (result.Value == -1)
            {
                return new ScStatusResult
                {
                    Value = -1,
                    ErrorInfo = result.ErrorInfo,
                    ErrorCode = result.ErrorCode
                };
            }

            var response98 = result.Response as ACSStatus_98;
            if (response98 == null)
            {
                return new ScStatusResult
                {
                    Value = -1,
                    ErrorInfo = "返回的不是 98 消息"
                };
            }

            if (response98.OnlineStatus_1 != "Y")
            {
                return new ScStatusResult
                {
                    Value = 0,
                    ErrorInfo = "ACS 当前不在线。" + response98.AF_ScreenMessage_o + " " + response98.BX_SupportedMessages_r,
                    Result = response98
                };
            }

            return new ScStatusResult
            {
                Value = 1,
                ErrorInfo = response98.AF_ScreenMessage_o,
                Result = response98
            };
        }

        public class GetItemInfoResult : NormalResult
        {
            public ItemInformationResponse_18 Result { get; set; }
        }

        public async Task<GetItemInfoResult> GetItemInfoAsync(
            string oi,
            string itemBarcode)
        {
            ItemInformation_17 request = new ItemInformation_17()
            {
                TransactionDate_18 = SIPUtility.NowDateTime,
                AO_InstitutionId_r = oi,
                AB_ItemIdentifier_r = itemBarcode,
            };
            request.SetDefaulValue();//设置其它默认值

            // 发送和接收消息
            string requestText = request.ToText();

            var result = await SendAndRecvAsync(requestText);
            if (result.Value == -1)
            {
                return new GetItemInfoResult
                {
                    Value = -1,
                    ErrorInfo = result.ErrorInfo,
                    ErrorCode = result.ErrorCode
                };
            }

            var response18 = result.Response as ItemInformationResponse_18;
            if (response18 == null)
            {
                return new GetItemInfoResult
                {
                    Value = -1,
                    ErrorInfo = "返回的不是18消息"
                };
            }

            return new GetItemInfoResult
            {
                Value = 0,
                Result = response18
            };
        }

        public class GetPatronInfoResult : NormalResult
        {
            public PatronInformationResponse_64 Result { get; set; }
        }

        public async Task<GetPatronInfoResult> GetPatronInfoAsync(string patronBarcode)
        {
            PatronInformation_63 request = new PatronInformation_63()
            {
                TransactionDate_18 = SIPUtility.NowDateTime,
                AO_InstitutionId_r = SIPConst.AO_Value,
                AA_PatronIdentifier_r = patronBarcode,
            };
            request.SetDefaulValue();//设置其它默认值

            // 发送和接收消息
            string requestText = request.ToText();

            var result = await SendAndRecvAsync(requestText);
            if (result.Value == -1)
            {
                return new GetPatronInfoResult
                {
                    Value = -1,
                    ErrorInfo = result.ErrorInfo,
                    ErrorCode = result.ErrorCode
                };
            }

            var response64 = result.Response as PatronInformationResponse_64;
            if (response64 == null)
            {
                return new GetPatronInfoResult
                {
                    Value = -1,
                    ErrorInfo = "返回的不是64消息"
                };
            }

            return new GetPatronInfoResult
            {
                Value = 0,
                Result = response64
            };
        }

        public class CheckoutResult : NormalResult
        {
            public CheckoutResponse_12 Result { get; set; }
        }

        // 借书
        // return.Value
        //      -1  出错
        //      0   请求失败
        //      1   请求成功
        public async Task<CheckoutResult> CheckoutAsync(string patronBarcode,
            string itemBarcode)
        {
            Checkout_11 request = new Checkout_11()
            {
                TransactionDate_18 = SIPUtility.NowDateTime,
                AA_PatronIdentifier_r = patronBarcode,
                AB_ItemIdentifier_r = itemBarcode,
                AO_InstitutionId_r = SIPConst.AO_Value,
            };
            request.SetDefaulValue();//设置其它默认值

            // 发送和接收消息
            string requestText = request.ToText();

            var result = await SendAndRecvAsync(requestText);
            if (result.Value == -1)
            {
                return new CheckoutResult
                {
                    Value = -1,
                    ErrorInfo = result.ErrorInfo,
                    ErrorCode = result.ErrorCode
                };
            }

            var response12 = result.Response as CheckoutResponse_12;
            if (response12 == null)
            {
                return new CheckoutResult
                {
                    Value = -1,
                    ErrorInfo = "返回的不是12消息"
                };
            }

            return new CheckoutResult
            {
                Value = response12.Ok_1 == "0" ? 0 : 1,
                ErrorInfo = response12.AF_ScreenMessage_o,
                Result = response12
            };
        }

        public class CheckinResult : NormalResult
        {
            public CheckinResponse_10 Result { get; set; }
        }

        // 还书
        // return.Value
        //      -1  出错
        //      0   请求失败
        //      1   请求成功
        public async Task<CheckinResult> CheckinAsync(string itemBarcode)
        {
            Checkin_09 request = new Checkin_09()
            {
                TransactionDate_18 = SIPUtility.NowDateTime,
                ReturnDate_18 = SIPUtility.NowDateTime,
                AB_ItemIdentifier_r = itemBarcode,
                AO_InstitutionId_r = SIPConst.AO_Value,
            };
            request.SetDefaulValue();//设置其它默认值

            // 发送和接收消息
            string requestText = request.ToText();

            var result = await SendAndRecvAsync(requestText);
            if (result.Value == -1)
            {
                return new CheckinResult
                {
                    Value = -1,
                    ErrorInfo = result.ErrorInfo,
                    ErrorCode = result.ErrorCode
                };
            }

            var response10 = result.Response as CheckinResponse_10;
            if (response10 == null)
            {
                return new CheckinResult
                {
                    Value = -1,
                    ErrorInfo = "返回的不是10消息"
                };
            }

            return new CheckinResult
            {
                Value = response10.Ok_1 == "0" ? 0 : 1,
                ErrorInfo = response10.AF_ScreenMessage_o,
                Result = response10
            };
        }

        public class RenewResult : NormalResult
        {
            public RenewResponse_30 Result { get; set; }
        }

        // 续借
        // return.Value
        //      -1  出错
        //      0   请求失败
        //      1   请求成功
        public async Task<RenewResult> RenewAsync(string patronBarcode,
            string itemBarcode)
        {
            Renew_29 request = new Renew_29()
            {
                TransactionDate_18 = SIPUtility.NowDateTime,
                AO_InstitutionId_r = SIPConst.AO_Value,
                AA_PatronIdentifier_r = patronBarcode,
                AB_ItemIdentifier_o = itemBarcode,
            };
            request.SetDefaulValue();//设置其它默认值

            // 发送和接收消息
            string requestText = request.ToText();

            var result = await SendAndRecvAsync(requestText);
            if (result.Value == -1)
            {
                return new RenewResult
                {
                    Value = -1,
                    ErrorInfo = result.ErrorInfo,
                    ErrorCode = result.ErrorCode
                };
            }

            var response30 = result.Response as RenewResponse_30;
            if (response30 == null)
            {
                return new RenewResult
                {
                    Value = -1,
                    ErrorInfo = "返回的不是30消息"
                };
            }

            return new RenewResult
            {
                Value = response30.Ok_1 == "0" ? 0 : 1,
                ErrorInfo = response30.AF_ScreenMessage_o,
                Result = response30
            };
        }


        #endregion
    }
}
