using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Xml;
using DigitalPlatform.Core;

namespace dp2SIPServer
{
    public class Session
    {
        TcpClient client = null;


        public LibraryChannelPool _channelPool = new LibraryChannelPool();

        string _username = "";
        string _password = "";

        string DateTimeNow
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMdd    HHmmss");
            }
        }


        public void Dispose()
        {
            if (this.client != null)
            {
                try
                {
                    this.client.Close();
                }
                catch
                {
                }
                this.client = null;
            }

            foreach (LibraryChannel channel in _channelList)
            {
                if (channel != null)
                    channel.Abort();
            }
        }

        internal Session(TcpClient client)
        {
            this.client = client;
        }



        List<LibraryChannel> _channelList = new List<LibraryChannel>();

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetChannel()
        {
            LibraryChannel channel = this._channelPool.GetChannel(Properties.Settings.Default.LibraryServerUrl,
                Properties.Settings.Default.Username);
            channel.Idle += channel_Idle;
            _channelList.Add(channel);
            // TODO: 检查数组是否溢出
            return channel;
        }

        void channel_Idle(object sender, IdleEventArgs e)
        {
            Application.DoEvents();
        }

        public void ReturnChannel(LibraryChannel channel)
        {
            channel.Idle -= channel_Idle;

            this._channelPool.ReturnChannel(channel);
            _channelList.Remove(channel);
        }


        public void WriteToLog(string strText)
        {
            string strOperLogPath = Application.StartupPath + "\\operlog";

            DirectoryInfo dirInfo = new DirectoryInfo(strOperLogPath);
            if (!dirInfo.Exists)
                dirInfo = Directory.CreateDirectory(strOperLogPath);

            if (String.IsNullOrEmpty(strText) == true)
                return;

            strText = DateTime.Now.ToString() + ":" + strText;

            try
            {
                string strFilename = dirInfo.FullName + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                StreamWriter sw = new StreamWriter(strFilename,
                    true,	// append
                    Encoding.UTF8);

                sw.WriteLine(strText);
                sw.Close();
            }
            catch (Exception ex)
            {
                // WriteHtml("写入日志文件发生错误：" + ex.Message + "\r\n");
            }
        }

        // 接收请求包
        public int RecvTcpPackage(out string strPackage,
            out string strError)
        {
            strError = "";

            Debug.Assert(client != null, "client为空");

            strPackage = "";

            List<byte> list = new List<byte>();
            while (true)
            {
                try
                {
                    int nRet = client.GetStream().ReadByte();
                    if (nRet == -1)
                    {
                        break;
                    }
                    else if (nRet == 0x0a)
                    {
                        break;
                    }
                    else
                    {
                        list.Add((byte)nRet);
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == 10035)
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }
                    strError = "recv出错: " + ex.Message;
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    strError = "recv出错: " + ex.Message;
                    goto ERROR1;
                }
            }


            if (list.Count < 1)
                goto ERROR1;

            byte[] baPackage = new byte[list.Count - 1];
            list.CopyTo(0, baPackage, 0, list.Count - 1);
            strPackage = Encoding.GetEncoding(936).GetString(baPackage);

            return 0;

        ERROR1:
            this.CloseSocket();
            strPackage = "";
            return -1;
        }

        // 发出响应包
        // return:
        //      -1  出错
        //      0   正确发出
        //      1   发出前，发现流中有未读入的数据
        public int SendTcpPackage(string strPackage,
            out string strError)
        {
            strError = "";

            if (client == null)
            {
                strError = "client尚未初始化";
                return -1;
            }

            byte[] baPackage = Encoding.GetEncoding(936).GetBytes(strPackage);
            try
            {
                NetworkStream stream = client.GetStream();
                if (stream.DataAvailable == true)
                {
                    strError = "发送前发现流中有未读的数据";
                    return 1;
                }

                stream.Write(baPackage, 0, baPackage.Length);
                // stream.Flush();
                return 0;
            }
            catch (Exception ex)
            {
                strError = "send出错: " + ex.Message;
                this.CloseSocket();
                return -1;
            }
        }


        public void CloseSocket()
        {
            if (client != null)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Close();
                }
                catch { }
                try
                {
                    client.Close();
                }
                catch { }

                client = null;
            }
        }

        /// <summary>
        /// Session处理轮回
        /// </summary>
        public void Processing()
        {
            string strPackage = "";
            string strError = "";

            try
            {
                while (true)
                {
                    int nRet = RecvTcpPackage(out strPackage, out strError);
                    if (nRet == -1)
                    {
                        this.WriteToLog(strError);
                        return;
                    }

                    this.WriteToLog("Recv:" + strPackage);

                    if (strPackage.Length < 2)
                    {
                        this.WriteToLog("命令错误，命令长度不够2位");
                        return;
                    }

                    string strMessageIdentifiers = strPackage.Substring(0, 2);

                    string strReaderBarcode = "";
                    string strItemBarcode = "";
                    string strPassword = "";
                    string[] parts = strPackage.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string part = parts[i];
                        if (part.Length < 2)
                            continue;

                        switch (part.Substring(0, 2))
                        {
                            case "AA":
                                strReaderBarcode = part.Substring(2);
                                break;
                            case "AB":
                                strItemBarcode = part.Substring(2);
                                break;
                            case "AD":
                                strPassword = part.Substring(2);
                                break;
                            default:
                                break;
                        }
                    }

                    // 登录到dp2系统
                    nRet = DoLogin(Properties.Settings.Default.Username,
                        Properties.Settings.Default.Password,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        this.WriteToLog(strError);
                        return;
                    }

                    string strBackMsg = "";
                    switch (strMessageIdentifiers)
                    {
                        case "09":
                            {
                                nRet = Return(strItemBarcode, out strBackMsg, out strError);
                                if (nRet == -1)
                                    this.WriteToLog(strError);
                                break;
                            }
                        case "11":
                            {
                                nRet = Borrow(false,
                                   strReaderBarcode,
                                   strItemBarcode,
                                   out strBackMsg,
                                   out strError);
                                if (nRet == -1)
                                    this.WriteToLog(strError);
                                break;
                            }
                        case "17":
                            {
                                nRet = GetItemInfo(strItemBarcode, out strBackMsg, out strError);
                                if (nRet == 0)
                                    this.WriteToLog(strError);
                                break;
                            }
                        case "29":
                            {
                                nRet = Borrow(true,
                                    "",  //读者条码号为空，续借
                                    strItemBarcode,
                                    out strBackMsg,
                                    out strError);
                                if (nRet == 0)
                                    this.WriteToLog(strError);
                                break;
                            }
                        case "85":
                            {
                                nRet = GetReaderInfo(strReaderBarcode,
                                    strPassword,
                                    "readerInfo",
                                    out strBackMsg,
                                    out strError);
                                if (nRet == 0)
                                    this.WriteToLog(strError);
                                break;
                            }
                        case "63":
                            {
                                nRet = GetReaderInfo(strReaderBarcode,
                                    strPassword,
                                    "borrowInfo",
                                    out strBackMsg,
                                    out strError);
                                if (nRet == 0)
                                    this.WriteToLog(strError);
                                break;
                            }
                        case "81":
                            {
                                nRet = SetReaderInfo(strPackage, out strBackMsg, out strError);
                                if (nRet == 0)
                                {
                                    if (String.IsNullOrEmpty(strError) == false)
                                        this.WriteToLog(strError);
                                }
                                break;
                            }
                        case "91":
                            {
                                nRet = CheckDupReaderInfo(strPackage, out strBackMsg, out strError);
                                if (nRet == 0)
                                {
                                    if (String.IsNullOrEmpty(strError) == false)
                                        this.WriteToLog(strError);
                                }
                                break;
                            }
                        case "99":
                            {
                                strBackMsg = "98YYYNYN01000320150329    1629482.00AOST|AMST|AN|AF连接ACS_Server成功!|AG|BX|AY1AZDA74";
                                break;
                            }
                        default:
                            strBackMsg = "无法识别的命令'" + strMessageIdentifiers + "'\r\n";
                            break;
                    }

                    nRet = SendTcpPackage(strBackMsg, out strError);
                    if (nRet == -1)
                    {
                        this.WriteToLog(strError);
                        return;
                    }

                    this.WriteToLog("Send:" + strBackMsg);

                }
            }
            finally
            {
                this.CloseSocket();
            }
        }

        // 进行登录
        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        int DoLogin(string strUserName,
            string strPassword,
            out string strError)
        {
            strError = "";

            LibraryChannel channel = this.GetChannel();

            // return:
            //      -1  error
            //      0   登录未成功
            //      1   登录成功
            long lRet = channel.Login(strUserName,
                strPassword,
                "type=worker,client=dp2SIPServer|0.01",
                out strError);
            if (lRet == -1)
            {
                this.ReturnChannel(channel);
                return -1;
            }

            this.ReturnChannel(channel);

            return (int)lRet;
        }

        /// <summary>
        /// 借
        /// </summary>
        /// <param name="bRenew"></param>
        /// <param name="strReaderBarcode"></param>
        /// <param name="strItemBarcode"></param>
        /// <param name="strBackMsg"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        int Borrow(bool bRenew,
            string strReaderBarcode,
            string strItemBarcode,
            out string strBackMsg,
            out string strError)
        {
            strBackMsg = "";
            strError = "";

            int nRet = 0;

            int nFlag = 0;

            string strTitle = "";

            string strPrice = "";

            string strLastReturningDate = ""; // 续借前应还日期

            string[] aDupPath = null;
            string[] item_records = null;
            string[] reader_records = null;
            string[] biblio_records = null;
            string strOutputReaderBarcode = "";
            BorrowInfo borrow_info = null;

            LibraryChannel channel = this.GetChannel();

            long lRet = channel.Borrow(
                null,   // stop,
                bRenew,  // 续借为 true
                 strReaderBarcode,    //读者证条码号
                 strItemBarcode,     // 册条码号
                null, //strConfirmItemRecPath,
                false,
                null,   // this.OneReaderItemBarcodes,
                "biblio,item",//  "reader,item,biblio", // strStyle,
                "xml:noborrowhistory",  // strItemReturnFormats,
                out item_records,
                "summary",    // strReaderFormatList
                out reader_records,
                "xml",         //strBiblioReturnFormats,
                out biblio_records,
                out aDupPath,
                out strOutputReaderBarcode,
                out borrow_info,
                out strError);
            if (lRet == -1)
            {
                nRet = -1;
                nFlag = 0;
            }
            else
            {
                nFlag = 1;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(item_records[0]);

                    strPrice = DomUtil.GetElementText(dom.DocumentElement, "price");

                    strLastReturningDate = DomUtil.GetElementText(dom.DocumentElement, "lastReturningDate");
                    strLastReturningDate = DateTimeUtil.Rfc1123DateTimeStringToLocal(strLastReturningDate, "yyyy-MM-dd");
                }
                catch (Exception ex)
                {
                    nRet = -1;
                    strError = "册信息解析错误" + ex.Message;
                }


                if (nRet == 0)
                {
                    string strMarcSyntax = "";
                    MarcRecord record = MarcXml2MarcRecord(biblio_records[0],
                        out strMarcSyntax,
                        out strError);
                    if (record != null)
                    {
                        if (strMarcSyntax == "unimarc")
                        {
                            strTitle = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                        }
                        else if (strMarcSyntax == "usmarc")
                        {
                            strTitle = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                        }
                    }
                    else
                    {
                        nRet = -1;
                        strError = "书目信息解析错误:" + strError;
                    }
                }
            }

            StringBuilder sb = new StringBuilder(1024);
            if (bRenew == false)
                sb.Append("12").Append(nFlag.ToString()).Append("NNY20141027    181545AO");
            else
                sb.Append("30").Append(nFlag.ToString()).Append("YNN20141028    082239AO001");

            sb.Append("|AA").Append(strOutputReaderBarcode).Append("|AB").Append(strItemBarcode);
            if (nFlag == 0)
            {
                if (bRenew == false)
                {
                    sb.Append("|AF 图书【").Append(strItemBarcode).Append("】借阅失败！");
                    sb.Append("|AG读者【").Append(strOutputReaderBarcode).Append("】借书 ").Append("【").Append(strItemBarcode).Append("】失败！").Append("\r\n");
                }
                else
                {
                    sb.Append("|AF 图书【").Append(strItemBarcode).Append("】续借失败！");
                    sb.Append("|AG读者【").Append(strOutputReaderBarcode).Append("】续借图书 ").Append("【").Append(strItemBarcode).Append("】失败！").Append("\r\n");
                }
            }
            else // if (nFlag == 1)
            {
                string strLatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(borrow_info.LatestReturnTime, "yyyy-MM-dd");
                sb.Append("|AJ").Append(strTitle).Append("|AH").Append(strLatestReturnTime);

                if (bRenew == false)
                {
                    sb.Append("|CHATN").Append("PR").Append(strPrice);
                    sb.Append("|AF 图书【").Append(strItemBarcode).Append("】借阅成功！应还日期：").Append(strLatestReturnTime);
                    sb.Append("|AG读者【").Append(strOutputReaderBarcode).Append("】借书 ").Append(strTitle);
                    sb.Append("【").Append(strItemBarcode).Append("】成功，应还日期：").Append(strLatestReturnTime).Append("\r\n");
                }
                else
                {
                    sb.Append("|CM").Append(strLastReturningDate);

                    sb.Append("|AF 图书【").Append(strItemBarcode).Append("】续借处理成功，应还日期：").Append(strLatestReturnTime);
                    sb.Append("|AG读者【").Append(strOutputReaderBarcode).Append("】续借处理成功，应还日期：").Append(strLatestReturnTime).Append("\r\n");
                }
            }
            strBackMsg = sb.ToString();

            this.ReturnChannel(channel);

            return nRet;
        }


        /// <summary>
        /// 还书
        /// </summary>
        /// <param name="strItemBarcode"></param>
        /// <param name="strBackMsg"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        int Return(
            // string strReaderBarcode, 
            string strItemBarcode,
            out string strBackMsg,
            out string strError)
        {
            strBackMsg = "";
            strError = "";

            int nRet = 0;

            int nFlag = 0;


            string strTitle = "";

            string strLocation = "";
            string strBookType = "";
            string strPrice = "";

            string strOverduePrice = "";
            string strReturnDate = "";


            string[] item_records = null;
            string[] reader_records = null;
            string[] biblio_records = null;
            string[] aDupPath = null;
            string strOutputReaderBarcode = "";
            ReturnInfo return_info = null;

            LibraryChannel channel = this.GetChannel();
            long lRet = channel.Return(null,
                "return",
                "",    //strReaderBarcode,
                strItemBarcode,
                "", // strConfirmItemRecPath
                false,
                "item,biblio",
                "xml",
                out item_records,
                "xml",
                out reader_records,
                "xml",
                out biblio_records,
                out aDupPath,
                out strOutputReaderBarcode,
                out return_info,
                out strError);
            if (lRet == -1)
            {
                nRet = -1;
                nFlag = 0;
            }
            else
            {
                nFlag = 1;

                XmlDocument item_dom = new XmlDocument();
                string strItemXml = item_records[0];
                try
                {
                    item_dom.LoadXml(strItemXml);


                    strLocation = DomUtil.GetElementText(item_dom.DocumentElement, "location");
                    strPrice = DomUtil.GetElementText(item_dom.DocumentElement, "price");
                    strBookType = DomUtil.GetElementText(item_dom.DocumentElement, "bookType");

                    strReturnDate = DomUtil.GetAttr(item_dom.DocumentElement, "borrowHistory/borrower", "returnDate");
                    if (String.IsNullOrEmpty(strReturnDate) == false)
                        strReturnDate = DateTimeUtil.Rfc1123DateTimeStringToLocal(strReturnDate, "yyyy-MM-dd");
                    else
                        strReturnDate = DateTime.Now.ToString("yyyy-MM-dd");
                }
                catch (Exception ex)
                {
                    nRet = -1;
                    strError = "册信息解析错误:" + ex.Message;
                }

                if (nRet == 0 && lRet == 1)
                {
                    XmlDocument overdue_dom = new XmlDocument();
                    try
                    {
                        overdue_dom.LoadXml(return_info.OverdueString);

                        strOverduePrice = DomUtil.GetAttr(overdue_dom.DocumentElement, "price");
                    }
                    catch (Exception ex)
                    {
                        nRet = -1;
                        strError = "超期信息解析错误:" + ex.Message;
                    }
                }


                if (nRet == 0)
                {
                    string strMarcSyntax = "";
                    MarcRecord record = MarcXml2MarcRecord(biblio_records[0], out strMarcSyntax, out strError);
                    if (record != null)
                    {
                        if (strMarcSyntax == "unimarc")
                        {
                            strTitle = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                        }
                        else if (strMarcSyntax == "usmarc")
                        {
                            strTitle = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                        }
                    }
                    else
                    {
                        nRet = -1;
                        strError = "书目信息解析错误:" + strError;
                    }
                }
            }

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("10").Append(nFlag.ToString()).Append("YNN").Append(this.DateTimeNow).Append("AO");
            sb.Append("|AB").Append(strItemBarcode);

            if (nFlag == 1)
            {
                sb.Append("|AQ").Append(strLocation);
                sb.Append("|AJ").Append(strTitle).Append("|AA").Append(strOutputReaderBarcode);
                sb.Append("|CK").Append(strBookType);
                sb.Append("|CF").Append(strOverduePrice); // 超期还书欠费金额
                sb.Append("|CH").Append(strTitle).Append("ATN").Append("PR").Append(strPrice);
                sb.Append("|CLsort bin A1");

                sb.Append("|AF图书【").Append(strItemBarcode).Append("】还回处理成功！").Append(strReturnDate);
                sb.Append("|AG图书").Append(strTitle).Append("[").Append(strItemBarcode).Append("]已于").Append(strReturnDate).Append("归还！");
            }
            else // nFlag == 0
            {
                sb.Append("|CLsort bin A1");
                sb.Append("|AF图书【").Append(strItemBarcode).Append("】还回处理错误！");
                sb.Append("|AG图书【").Append(strItemBarcode).Append("】还回处理错误！");
            }
            sb.Append("\r\n");

            strBackMsg = sb.ToString();

            this.ReturnChannel(channel);

            return nRet;
        }

        /// <summary>
        /// 获得读者记录
        /// </summary>
        /// <param name="strBarcode"></param>
        /// <param name="strBackMsg"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        int GetReaderInfo(string strBarcode,
            string strPassword,
            string strStyle,
            out string strBackMsg,
            out string strError)
        {
            strBackMsg = "";
            strError = "";

            int nRet = 0;

            long lRet = 0;

            LibraryChannel channel = this.GetChannel();

            StringBuilder sb = new StringBuilder(1024);

            if (strStyle == "" || strStyle == "readerInfo")
            {
                sb.Append("86").Append(this.DateTimeNow).Append("AO");
            }
            else  // strStyle == "borrowInfo"
            {
                sb.Append("64              001").Append(this.DateTimeNow).Append("000600000006000000000008AO001");
            }
            sb.Append("|AA").Append(strBarcode);

            // strStyle == "readerInfo" && 
            if (String.IsNullOrEmpty(strPassword) == false)
            {
                lRet = channel.VerifyReaderPassword(null,
                    strBarcode,
                    strPassword,
                    out strError);
                if (lRet == -1)
                {
                    sb.Append("|BLN").Append("|CQN");
                    sb.Append("|AF").Append("校验密码过程中发生错误……");
                    sb.Append("|AG").Append("校验密码过程中发生错误……").Append("\r\n");
                    strBackMsg = sb.ToString();
                    return nRet;
                }
                else if (lRet == 0)
                {
                    sb.Append("|BLN").Append("|CQN");
                    sb.Append("|AF").Append("卡号或密码不正确。");
                    sb.Append("|AG").Append("卡号或密码不正确。").Append("\r\n");
                    strBackMsg = sb.ToString();
                    return nRet;
                }
            }


            XmlDocument dom = new XmlDocument();
            string[] results = null;
            lRet = channel.GetReaderInfo(
                null,// stop,
                strBarcode, //读者卡号,
                "advancexml",   // this.RenderFormat, // "html",
                out results,
                out strError);
            switch (lRet)
            {
                case -1:
                    nRet = 0;
                    break;
                case 0:
                    nRet = 0;
                    strError = "读者不存在";
                    break;
                case 1:
                    {
                        nRet = 1;
                        string strReaderXml = results[0];
                        try
                        {
                            dom.LoadXml(strReaderXml);
                        }
                        catch (Exception ex)
                        {
                            nRet = 0;
                            strError = "读者信息解析错误:" + ex.Message;
                        }
                        break;
                    }
                default: // lRet > 1
                    nRet = 0;
                    strError = "找到多条读者记录";
                    break;
            }

            if (strStyle == "" || strStyle == "readerInfo")
            {
                sb.Append("|OK").Append(nRet.ToString());
            }

            if (nRet == 0)
            {
                sb.Append("|BLN").Append("|AF查询读者信息失败:").Append(strError).Append("|AG查询读者信息失败!\r\n");
            }
            else // nRet == 1
            {
                sb.Append("|AE").Append(DomUtil.GetElementText(dom.DocumentElement, "name"));
                sb.Append("|XT").Append(DomUtil.GetElementText(dom.DocumentElement, "readerType"));

                string strExpireDate = DomUtil.GetElementText(dom.DocumentElement, "expireDate");
                strExpireDate = DateTimeUtil.Rfc1123DateTimeStringToLocal(strExpireDate, "yyyy-MM-dd");
                sb.Append("|XD").Append(strExpireDate);
                if (strStyle == "" || strStyle == "readerInfo")
                {
                    sb.Append("|BP").Append(DomUtil.GetElementText(dom.DocumentElement, "tel"));
                    sb.Append("|BD").Append(DomUtil.GetElementText(dom.DocumentElement, "address"));
                    sb.Append("|XO").Append(DomUtil.GetElementText(dom.DocumentElement, "idCardNumber"));

                    string strCreateDate = DomUtil.GetElementText(dom.DocumentElement, "createDate");
                    strCreateDate = DateTimeUtil.Rfc1123DateTimeStringToLocal(strCreateDate, "yyyy-MM-dd");
                    sb.Append("|XB").Append(strCreateDate);

                    string strDateOfBirth = DomUtil.GetElementText(dom.DocumentElement, "dateOfBirth");
                    strDateOfBirth = DateTimeUtil.Rfc1123DateTimeStringToLocal(strDateOfBirth, "yyyy-MM-dd");
                    sb.Append("|XH").Append(strDateOfBirth);
                    // sb.Append("|XN"); // 民族
                    sb.Append("|XF").Append(DomUtil.GetElementText(dom.DocumentElement, "comment"));
                    string strGender = DomUtil.GetElementText(dom.DocumentElement, "gender").Trim();
                    if (strGender == "男")
                        strGender = "1";
                    else if (strGender == "女")
                        strGender = "0";
                    else
                        strGender = "1";
                    sb.Append("|XM").Append(strGender).Append("|AF查询读者信息成功!|AG查询读者信息成功!\r\n");
                }
                else if (strStyle == "borrowInfo")
                {
                    string strOverduesPrice = "";
                    XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue/@price");
                    foreach (XmlNode node in nodes)
                    {
                        string strPrice = node.Value;
                        if (String.IsNullOrEmpty(strPrice))
                            continue;

                        if (String.IsNullOrEmpty(strOverduesPrice) == false)
                            strOverduesPrice += "+";

                        strOverduesPrice += strPrice;
                    }
                    nRet = PriceUtil.SumPrices(strOverduesPrice, out strOverduesPrice, out strError);
                    if (nRet == 0)
                        sb.Append("|CF").Append(strOverduesPrice).Append("|JF").Append(strOverduesPrice);
                    else
                        sb.Append("|CF").Append("CNY0.0").Append("|JF").Append("CNY0.0");

                    // 验证读者：Y表示读者存在，N表示读者不存在
                    sb.Append("|BLY");

                    // CQ验证密码：Y表示读者密码正确，N表示读者密码错误
                    sb.Append("|CQY");

                    // 租金 JE 预付款
                    sb.Append("|JE");

                    // 保证金保证系数
                    sb.Append("|XR");

                    // 所借图书总额
                    string strItemsPrices = "";
                    string barcodes = "";
                    XmlNodeList borrows = dom.DocumentElement.SelectNodes("borrows/borrow");
                    foreach (XmlNode borrow in borrows)
                    {
                        string strItemBarcode = DomUtil.GetAttr(borrow, "barcode");
                        if (String.IsNullOrEmpty(strItemBarcode) == false)
                            barcodes += "|AS" + strItemBarcode;

                        string strPrice = DomUtil.GetAttr(borrow, "price");
                        if (String.IsNullOrEmpty(strPrice) == false)
                        {
                            if (String.IsNullOrEmpty(strItemsPrices) == false)
                                strItemsPrices += "+";

                            strItemsPrices += strPrice;
                        }
                    }

                    sb.Append(barcodes);

                    nRet = PriceUtil.SumPrices(strItemsPrices, out strItemsPrices, out strError);
                    if (nRet == 0)
                        sb.Append("|XC").Append(strItemsPrices);
                    else
                        sb.Append("|XC0.0");

                    // 押金
                    sb.Append("|BV").Append(DomUtil.GetElementText(dom.DocumentElement, "foregift"));

                    // 可借总册数
                    string strCount = DomUtil.GetElementAttr(dom.DocumentElement, "info/item[@name='可借总册数']", "value");
                    sb.Append("|BZ").Append(strCount);

                    string strBorrowsCount = DomUtil.GetElementAttr(dom.DocumentElement, "info/item[@name='当前还可借']", "value");
                    string strMsg = "";
                    if (strBorrowsCount != "0")
                        strMsg += "您在本馆最多可借【" + strCount + "】册，还可以再借【" + strBorrowsCount + "】册。";
                    else
                        strMsg += "您在本馆借书数已达最多可借数【" + strCount + "】，不能继续借了!";
                    sb.Append("|AF").Append(strMsg).Append("|AG").Append(strMsg).Append("\r\n");
                }
            }

            strBackMsg = sb.ToString();

            this.ReturnChannel(channel);
            return nRet;
        }

        /// <summary>
        /// 获得图书信息
        /// </summary>
        /// <param name="strBarcode"></param>
        /// <param name="strBackMsg"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        int GetItemInfo(string strBarcode,
            out string strBackMsg,
            out string strError)
        {
            strBackMsg = "";
            strError = "";

            int nRet = 0;

            string strBookState = "";
            string strReaderBarcode = "";
            string strTitle = "";
            string strAuthor = "";
            string strISBN = "";
            string strIsArrived = "";
            string strBookType = "";
            string strPrice = "";
            string strAccessNo = "";
            // string strDocType = ""; // 文献类型
            string strLocation = "";
            string strReturningDate = "";


            string strItemXml = "";
            string strBiblio = "";
            LibraryChannel channel = this.GetChannel();
            long lRet = channel.GetItemInfo(null,
                strBarcode,
                "xml",
                out strItemXml,
                "xml",
                out strBiblio,
                out strError);
            switch (lRet)
            {
                case -1:
                    nRet = 0;
                    strBookState = "05";
                    strError = "获得'" + strBarcode + "'发生错误:" + strError;
                    break;
                case 0:
                    nRet = 0;
                    strBookState = "00";
                    strError = strBarcode + " 册记录不存在";
                    break;
                case 1:
                    nRet = 1;
                    break;
                default: // lRet > 1
                    nRet = 0;
                    strBookState = "05";
                    strError = strBarcode + " 找到多条册记录，条码重复";
                    break;
            }

            XmlDocument dom = new XmlDocument();
            if (nRet == 1)
            {
                try
                {
                    dom.LoadXml(strItemXml);

                    string strItemState = DomUtil.GetElementText(dom.DocumentElement, "state");
                    if (String.IsNullOrEmpty(strItemState))
                    {
                        strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "borrower");
                        if (String.IsNullOrEmpty(strReaderBarcode))
                            strBookState = "02";
                        else
                            strBookState = "03";
                    }
                    else
                    {
                        if (StringUtil.IsInList("丢失", strItemState))
                            strBookState = "04";

                        if (StringUtil.IsInList("#预约", strItemState))
                            strIsArrived = "1";
                        else
                            strIsArrived = "0";
                    }

                    strPrice = DomUtil.GetElementText(dom.DocumentElement, "price");
                    strBookType = DomUtil.GetElementText(dom.DocumentElement, "bookType");
                    strAccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");
                    strLocation = DomUtil.GetElementText(dom.DocumentElement, "location");
                    strReturningDate = DomUtil.GetElementText(dom.DocumentElement, "returningDate");
                    strReturningDate = DateTimeUtil.Rfc1123DateTimeStringToLocal(strReturningDate, "yyyy-MM-dd");


                    string strMarcSyntax = "";
                    MarcRecord record = MarcXml2MarcRecord(strBiblio, out strMarcSyntax, out strError);
                    if (record != null)
                    {
                        if (strMarcSyntax == "unimarc")
                        {
                            strISBN = record.select("field[@name='010']/subfield[@name='a']").FirstContent;
                            strTitle = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                            strAuthor = record.select("field[@name='200']/subfield[@name='f']").FirstContent;
                        }
                        else if (strMarcSyntax == "usmarc")
                        {
                            strISBN = record.select("field[@name='020']/subfield[@name='a']").FirstContent;
                            strTitle = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                            strAuthor = record.select("field[@name='245']/subfield[@name='c']").FirstContent;
                        }
                    }
                    else
                    {
                        nRet = 0;
                        strError = "书目信息解析错误:" + strError;
                    }
                }
                catch (Exception ex)
                {
                    nRet = 0;
                    strError = strBarcode + ":读者记录解析错误:" + ex.Message;
                }
            }

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("18").Append(strBookState).Append("0001").Append(this.DateTimeNow).Append("CF00000");

            if (String.IsNullOrEmpty(strReaderBarcode) == false)
                sb.Append("|AA").Append(strReaderBarcode);

            sb.Append("|AB").Append(strBarcode);
            if (nRet == 0)
            {
                sb.Append("|AF").Append("获得图书信息发生错误！").Append("|AG").Append("获得图书信息发生错误！\r\n");
            }
            else
            {
                sb.Append("|AJ").Append(strTitle).Append("|AW").Append(strAuthor).Append("|AK").Append(strISBN);
                sb.Append("|RE").Append(strIsArrived).Append("|CK").Append(strBookType).Append("|CH").Append(strPrice);
                sb.Append("|AH").Append(strReturningDate);
                sb.Append("|KC").Append(strAccessNo).Append("|AQ").Append(strLocation).Append("|AF").Append("|AG\r\n");
            }

            strBackMsg = sb.ToString();
            this.ReturnChannel(channel);
            return nRet;
        }

        int SetReaderInfo(string strSIP2Package,
            out string strBackMsg,
            out string strError)
        {
            strBackMsg = "";
            strError = "";

            int nRet = 0;

            string strBarcode = "";
            string strOperation = "";
            string[] parts = strSIP2Package.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Length < 2)
                    continue;

                string strValue = part.Substring(2);
                string str = part.Substring(0, 2);

                if (str == "AA")
                    strBarcode = strValue;
                else if (str == "XK")
                    strOperation = strValue;
                else
                    continue;

                if (String.IsNullOrEmpty(strBarcode) == false
                    && String.IsNullOrEmpty(strOperation) == false)
                    break;
            }

            if (String.IsNullOrEmpty(strOperation))
            {
                nRet = 0;
                strBackMsg = "82" + this.DateTimeNow + "AO|AA" + strBarcode +
                    "|XK" + strOperation +
                    "|OK0|AF修改读者记录发生错误，命令不对。|AG修改读者记录发生错误，命令不对。";
            }
            else if (strOperation == "01"
                || strOperation == "11"
                || strOperation == "02")
            {
                nRet = DoSetReaderInfo(strSIP2Package, out strBackMsg, out strError);
            }
            else if (strOperation == "14")
            {
                nRet = ChangePassword(strSIP2Package, out strBackMsg, out strError);
            }

            return nRet;
        }

        int DoSetReaderInfo(string strSIP2Package,
            out string strBackMsg,
            out string strError)
        {
            strBackMsg = "";
            strError = "";

            long lRetValue = 0;

            string strMsg = ""; // 返回给SIP2的错误信息

            string strAction = "";
            string strReaderBarcode = "";
            string strIDCardNumber = ""; // 身份证号

            bool bForegift = false; // 是否创建押金
            string strForegiftValue = ""; // 押金金额

            string strPassword = "";

            string strOperation = "";

            LibraryChannel channel = this.GetChannel();

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("82").Append(this.DateTimeNow).Append("AO");

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            #region 处理SIP2通讯包，构造读者dom
            string[] parts = strSIP2Package.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Length < 2)
                    continue;

                string strValue = part.Substring(2);
                switch (part.Substring(0, 2))
                {
                    case "AA":
                        {
                            if (String.IsNullOrEmpty(strValue))
                            {
                                strMsg = "办证时读者证号不能为空";
                                goto ERROR1;
                            }
                            strReaderBarcode = strValue;
                            DomUtil.SetElementText(dom.DocumentElement, "barcode", strValue);
                            break;
                        }
                    case "XO":
                        {
                            if (String.IsNullOrEmpty(strValue))
                            {
                                strMsg = "办证时身份证号不能为空";
                                goto ERROR1;
                            }
                            strIDCardNumber = strValue;
                            DomUtil.SetElementText(dom.DocumentElement, "idCardNumber", strValue);
                            break;
                        }
                    case "AD":
                        {
                            strPassword = strValue;
                            break;
                        }
                    case "XF":
                        DomUtil.SetElementText(dom.DocumentElement, "comment", strValue);
                        break;
                    case "XT":
                        DomUtil.SetElementText(dom.DocumentElement, "readerType", strValue);
                        break;
                    case "BV":
                        {
                            strForegiftValue = strValue;
                            break;
                        }
                    case "AM": // 开户馆
                        // DomUtil.SetElementText(dom.DocumentElement, "readerType", strValue);
                        break;
                    case "BD":
                        DomUtil.SetElementText(dom.DocumentElement, "address", strValue);
                        break;
                    case "XM":
                        {
                            if (strValue == "0")
                                strValue = "女";
                            else
                                strValue = "男";
                            DomUtil.SetElementText(dom.DocumentElement, "gender", strValue);
                            break;
                        }
                    case "MP":
                        DomUtil.SetElementText(dom.DocumentElement, "tel", strValue);
                        break;
                    case "XH":
                        {
                            try
                            {
                                DateTime dt = DateTimeUtil.Long8ToDateTime(strValue);
                                strValue = DateTimeUtil.Rfc1123DateTimeStringEx(dt);
                                DomUtil.SetElementText(dom.DocumentElement, "dateOfBirth", strValue);
                            }
                            catch { }
                            break;
                        }
                    case "AE":
                        DomUtil.SetElementText(dom.DocumentElement, "name", strValue);
                        break;
                    case "XN": // 民族
                        DomUtil.SetElementText(dom.DocumentElement, "station", strValue);
                        break;
                    case "XK": // 操作类型，01 办证操作 11办证但不处理押金
                        if (strValue == "01")
                        {
                            strAction = "new";
                            bForegift = true;
                            // DomUtil.SetElementText(dom.DocumentElement, "state", "暂停");
                        }
                        else if (strValue == "11")
                        {
                            strAction = "new";
                            bForegift = false;
                        }
                        else if (strValue == "02")
                        {
                            strAction = "change";
                        }
                        strOperation = strValue;
                        break;
                    default:
                        break;
                }
            } // end of for
            #endregion

            string strOldXml = "";

            string[] results = null;



            #region 根据卡号检索读者记录是否存在
            long lRet = channel.SearchReader(null,
                "<all>",
                strReaderBarcode,
                -1,
                "Barcode",
                "exact",
                "en",
                "default",
                "keycount",
                out strError);
            if (lRet == -1)
            {
                strMsg = "办证失败！按证号查找读者记录发生错误。";
                goto ERROR1;
            }
            else if (lRet >= 1)
            {
                strMsg = "办证失败！卡号为【" + strReaderBarcode + "】的读者已存在。";
                goto ERROR1;
            }
            #endregion

            #region 根据身份证号获得读者记录
            byte[] baTimestamp = null;
            string strRecPath = "";
            lRet = channel.GetReaderInfo(null,
                strIDCardNumber,
                "xml",
                out results,
                out strRecPath,
                out baTimestamp,
                out strError);
            switch (lRet)
            {
                case -1:
                    strMsg = "办证失败！按身份证号查找读者记录发生错误。";
                    goto ERROR1;
                case 0:
                    strRecPath = "读者/?";
                    break;
                case 1:
                    {
                        if (strAction == "change")
                        {
                            strOldXml = results[0];
                        }
                        else // strAction == "new"
                        {
                            XmlDocument result_dom = new XmlDocument();
                            result_dom.LoadXml(results[0]);
                            string strBarcode = DomUtil.GetElementText(result_dom.DocumentElement, "barcode");
                            strMsg = "办证失败！您已经有一张卡号为【" + strBarcode + "】的读者证，不能再办证。如读者证丢失，需补证请到柜台办理。";
                            goto ERROR1;
                        }
                        break;
                    }
                default: // lRet > 1
                    strMsg = "办证失败！身份证号为【" + strIDCardNumber + "】的读者已存在多条记录。";
                    goto ERROR1;
            }
            #endregion


            string strExistingXml = "";
            string strSavedXml = "";
            string strSavedRecPath = "";
            byte[] baNewTimestamp = null;
            ErrorCodeValue kernel_errorcode = ErrorCodeValue.NoError;
            lRet = channel.SetReaderInfo(null,
                strAction,
                strRecPath,
                dom.DocumentElement.OuterXml, // strNewXml
                strOldXml,
                baTimestamp,
                out strExistingXml,
                out strSavedXml,
                out strSavedRecPath,
                out baNewTimestamp,
                out kernel_errorcode,
                out strError);
            if (lRet == -1)
            {
                strMsg = strAction == "new" ? "办证失败！创建读者记录发生错误。" : "修改读者信息发生错误。";
                goto ERROR1;
            }

            lRetValue = lRet;

            if (bForegift == true
                && String.IsNullOrEmpty(strForegiftValue) == false)
            {
                // 创建交费请求
                string strReaderXml = "";
                string strOverdueID = "";
                lRet = channel.Foregift(null,
                   "foregift",
                   strReaderBarcode,
                    out strReaderXml,
                    out strOverdueID,
                   out strError);
                if (lRet == -1)
                {
                    lRet = DeleteReader(strSavedRecPath, baNewTimestamp, out strError);
                    if (lRet == -1)
                        strError = "办证过程中交费发生错误（回滚失败）：" + strError;
                    else
                        strError = "办证过程中交费发生错误（回滚成功）";


                    strMsg = "办证交费过程中创建交费请求失败，办证失败，请重新操作。";
                    goto ERROR1;
                }

                int nRet = DoAmerce(strReaderBarcode,
                    strForegiftValue,
                    out strMsg,
                    out strError);
                if (nRet == 0)
                    goto ERROR1;
            }

            if (String.IsNullOrEmpty(strPassword) == false
                && strAction == "new")
            {
                lRet = channel.ChangeReaderPassword(null,
                    strReaderBarcode,
                    "", // strOldReaderPassword
                    strPassword,
                    out strError);
                if (lRet != 1)
                {
                    strMsg = "设置密码不成功，可用[生日]登录后再修改密码。";
                }
            }
            sb.Append("|AA").Append(strReaderBarcode).Append("|XD").Append("|OK1");
            if (lRetValue == 0)
            {
                strMsg = strAction == "new" ? "办理新证成功！" + strMsg : "读者信息修改成功！";
            }
            else // if (lRetValue == 1)
            {
                strMsg = strAction == "new" ? "办理新证成功！但部分内容被拒绝。" + strMsg : "读者信息修改成功！但部分内容被拒绝。";
            }
            sb.Append("|XK").Append(strOperation);
            sb.Append("|AF").Append(strMsg).Append("|AG").Append(strMsg).Append("\r\n");
            strBackMsg = sb.ToString();
            this.ReturnChannel(channel);
            return 1;

        ERROR1:
            sb.Append("|XK").Append(strOperation).Append("|OK0").Append("|AF").Append(strMsg).Append("|AG").Append(strMsg).Append("\r\n");
            strBackMsg = sb.ToString();
            this.ReturnChannel(channel);
            return 0;
        }


        int CheckDupReaderInfo(string strSIP2Package,
             out string strBackMsg,
             out string strError)
        {
            strBackMsg = "";
            strError = "";

            LibraryChannel channel = this.GetChannel();

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("9220141021 100511AO");

            string strBarcode = "";
            string strIDCardNumber = "";
            string strOperation = "";
            string[] parts = strSIP2Package.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Length < 2)
                    continue;

                string strValue = part.Substring(2);
                string str = part.Substring(0, 2);

                if (str == "AA")
                    strBarcode = strValue;
                if (str == "XO")
                    strIDCardNumber = strValue;
                else if (str == "XK")
                    strOperation = strValue;
                else
                    continue;
            }

            sb.Append("|AA").Append(strBarcode);
            sb.Append("|XO").Append(strIDCardNumber);

            if ((strOperation == "0" && String.IsNullOrEmpty(strBarcode))
                || (strOperation == "1" && String.IsNullOrEmpty(strIDCardNumber)))
            {
                goto ERROR1;
            }

            #region 根据借书证号或身份证号获得读者记录
            string[] results = null;
            long lRet = channel.GetReaderInfo(null,
                strOperation == "0" ? strBarcode : strIDCardNumber,
                "xml",
                out results,
                out strError);
            switch (lRet)
            {
                case -1:
                    goto ERROR1;
                case 0:
                    sb.Append("|AC0");
                    break;
                case 1:
                    sb.Append("|AC1");
                    break;
                default: // lRet > 1
                    sb.Append("|AC1");
                    break;
            }
            #endregion

            strBackMsg = sb.ToString();
            this.ReturnChannel(channel);
            return 1;

        ERROR1:
            sb.Append("|XK").Append(strOperation).Append("|OK0");
            strBackMsg = sb.ToString();
            this.ReturnChannel(channel);
            return 0;
        }


        #region 交押金
        int DoAmerce(string strReaderBarcode,
            string strForegiftValue,
            out string strMsg,
            out string strError)
        {
            strMsg = "";
            strError = "";

            int nRet = 0;

            LibraryChannel channel = this.GetChannel();

            byte[] baTimestamp = null;
            string strRecPath = "";
            string[] results = null;
            long lRet = channel.GetReaderInfo(null,
                strReaderBarcode,
                "xml",
                out results,
                out strRecPath,
                out baTimestamp,
                out strError);
            if (lRet == -1)
            {
                strMsg = "办证交押金时获得读者记录发生错误，办证失败，请重新操作。";
            }
            else if (lRet == 0)
            {
                strMsg = "办证交押金时发现卡号读者竟不存在，办证失败，请重新操作。";
            }
            else if (lRet == 1)
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(results[0]);

                string strId = DomUtil.GetAttr(dom.DocumentElement, "overdues/overdue[@reason='押金。']", "id");
                if (String.IsNullOrEmpty(strId))
                {
                    strMsg = "办证交押金时发现费信息竟不存在，办证失败，请到柜台办理。";
                    goto UNDO;
                }

                string strValue = DomUtil.GetAttr(dom.DocumentElement, "overdues/overdue[@reason='押金。']", "price");
                strValue = PriceUtil.OldGetPurePrice(strValue);
                float value = float.Parse(strValue);

                float foregiftValue = float.Parse(strForegiftValue);
                if (value != foregiftValue)
                {
                    strMsg = "您放入的金额是：" + strForegiftValue + "，而您需要交的押金金额为：" + value.ToString();
                    goto UNDO;
                }


                AmerceItem item = new AmerceItem();
                item.ID = strId;
                AmerceItem[] amerce_items = { item };

                AmerceItem[] failed_items = null;
                string strReaderXml = "";
                lRet = channel.Amerce(null,
                    "amerce",
                    strReaderBarcode,
                    amerce_items,
                    out failed_items,
                    out strReaderXml,
                    out strError);
                if (lRet == -1 && lRet == 1)
                {
                    strMsg = "办证时收押金失败，办证失败，请到柜台办理。";
                    goto UNDO;
                }

                nRet = 1;
            }
            else // lRet > 1
            {
                strMsg = "办证交押金时发现卡号为【" + strReaderBarcode + "】读者存在多条，办证失败，请到柜台办理。";
            }

            this.ReturnChannel(channel);
            return nRet;
        UNDO:
            lRet = DeleteReader(strRecPath, baTimestamp, out strError);
            if (lRet == -1)
                strError = "办证过程中交费发生错误（回滚失败）：" + strError;
            else
                strError = "办证过程中交费发生错误（回滚成功）";
            this.ReturnChannel(channel);
            return nRet;
        }
        #endregion

        long DeleteReader(string strRecPath,
            byte[] baTimestamp,
            out string strError)
        {
            LibraryChannel channel = this.GetChannel();

            string strExistingXml = "";
            string strSavedXml = "";
            string strSavedRecPath = "";
            byte[] baNewTimestamp = null;
            ErrorCodeValue kernel_errorcode = ErrorCodeValue.NoError;
            long lRet = channel.SetReaderInfo(null,
                "forcedelete",
                strRecPath,
                "", // strNewXml, 
                "", // strOldXml, 
                baTimestamp,
                out strExistingXml,
                out strSavedXml,
                out strSavedRecPath,
                out baNewTimestamp,
                out kernel_errorcode,
                out strError);

            this.ReturnChannel(channel);

            return lRet;
        }

        int ChangePassword(string strSIP2Package,
            out string strBackMsg,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            string strBarcode = "";
            string strOldPassword = "";
            string strNewPassword = "";
            string strOperation = "";

            string[] parts = strSIP2Package.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Length < 2)
                    continue;

                string strValue = part.Substring(2);
                switch (part.Substring(0, 2))
                {
                    case "AA":
                        strBarcode = strValue;
                        break;
                    case "AD":
                        strOldPassword = strValue;
                        break;
                    case "KD":
                        strNewPassword = strValue;
                        break;
                    case "XK":
                        strOperation = strValue;
                        break;
                }
            }
            StringBuilder sb = new StringBuilder(1024);
            sb.Append("82").Append(this.DateTimeNow).Append("AO");
            sb.Append("|AA").Append(strBarcode);
            sb.Append("|XK").Append(strOperation);

            LibraryChannel channel = this.GetChannel();
            string strMsg = "";
            long lRet = channel.ChangeReaderPassword(null,
                strBarcode,
                strOldPassword,
                strNewPassword,
                out strError);
            if (lRet == -1)
            {
                nRet = 0;
                strMsg = "修改密码过程中发生错误，请稍后再试。";
            }
            else if (lRet == 0)
            {
                nRet = 0;
                strMsg = "旧密码输入错误，请重新输入。";
            }
            else
            {
                nRet = 1;
                strMsg = "读者修改密码成功！";
            }
            sb.Append("|OK").Append(nRet.ToString());
            sb.Append("|AF").Append(strMsg).Append("|AG").Append(strMsg).Append("\r\n");
            strBackMsg = sb.ToString();

            this.ReturnChannel(channel);
            return nRet;
        }

        static MarcRecord MarcXml2MarcRecord(string strMarcXml,
            out string strOutMarcSyntax,
            out string strError)
        {
            MarcRecord record = null;

            strError = "";
            strOutMarcSyntax = "";

            string strMARC = "";
            int nRet = MarcUtil.Xml2Marc(strMarcXml,
                false,
                "",
                out strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == 0)
                record = new MarcRecord(strMARC);
            else
                strError = "MarcXml转换错误:" + strError;

            return record;
        }
    }
}
