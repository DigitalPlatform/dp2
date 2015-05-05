using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;
using System.Runtime.Serialization;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.Message
{
    /// <summary>
    /// 消息收发管理中心
    /// 这个类代表了一个消息应用。通过加锁等方式对底层数据库操作实行严密安全的管理
    /// </summary>
    public class MessageCenter
    {
        public VerifyAccountEventHandler VerifyAccount;

        public string ServerUrl = "";   // 服务器URL
        public string MessageDbName = "";   // 消息数据库名
        public List<Box> Boxes = null;

        // 信箱类型名
        public const string INBOX = "收件箱";
        public const string TEMP = "草稿";
        public const string OUTBOX = "已发送";
        public const string RECYCLEBIN = "废件箱";

        public MessageCenter()
        {
            InitialStandardBoxes();
        }

        public MessageCenter(string strServerUrl,
            string strMessageDbName)
        {
            this.ServerUrl = strServerUrl;
            this.MessageDbName = strMessageDbName;

            InitialStandardBoxes();
        }

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.Message.res.MessageCenter.cs",
                typeof(MessageCenter).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {

                string s = GetRm().GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception ex)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }

        // 初始化标准的几个信箱
        public void InitialStandardBoxes()
        {
            this.Boxes = new List<Box>();

            Box box = null;

            // 收件箱 inbox
            box = new Box();
            box.Name = this.GetString("收件箱");
            box.Type = INBOX;
            this.Boxes.Add(box);

            // 草稿 temp
            box = new Box();
            box.Name = this.GetString("草稿");
            box.Type = TEMP;
            this.Boxes.Add(box);

            // 已发送 outbox
            box = new Box();
            box.Name = this.GetString("已发送");
            box.Type = OUTBOX;
            this.Boxes.Add(box);

            // 废件箱 recyclebin
            box = new Box();
            box.Name = this.GetString("废件箱");
            box.Type = RECYCLEBIN;
            this.Boxes.Add(box);
        }

        // 将信箱名字转换为boxtype值
        // 2009/7/6 new add
        public string GetBoxType(string strName)
        {
            for (int i = 0; i < this.Boxes.Count; i++)
            {
                Box box = this.Boxes[i];

                if (strName == box.Name)
                    return box.Type;
            }

            return null;    // not found
        }

        public static bool IsInBox(string strBoxType)
        {
            if (strBoxType == INBOX/*"收件箱"*/)
                return true;
            return false;
        }

        public static bool IsTemp(string strBoxType)
        {
            if (strBoxType == TEMP/*"草稿"*/)
                return true;
            return false;
        }

        public static bool IsOutbox(string strBoxType)
        {
            if (strBoxType == OUTBOX/*"已发送"*/)
                return true;
            return false;
        }

        public static bool IsRecycleBin(string strBoxType)
        {
            if (strBoxType == RECYCLEBIN/*"废件箱"*/)
                return true;
            return false;
        }

        // 构造检索式
        // parameters:
        //      strStyle    空字符串, 表示构造用于检索一个信箱内所有消息的检索式;
        //                  "untouched", 表示构造用于检索一个信箱内未读消息的检索式;
        //                  "touched", 表示构造用于检索一个信箱内已读消息的检索式;
        public int MakeSearchQuery(
            string strUserID,
            string strBox,
            string strStyle,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "";

            if (String.IsNullOrEmpty(strUserID) == true)
            {
                // text-level: 内部错误
                strError = "strUserID参数不能为空";
                return -1;
            }

            if (String.IsNullOrEmpty(strBox) == true)
            {
                // text-level: 内部错误
                strError = "strBox参数不能为空";
                return -1;
            }

            // 需要注意检查一下box名字是否合法

            if (String.IsNullOrEmpty(strStyle) == true)
            {
                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(this.MessageDbName)       // 2007/9/14 new add
                    + ":用户名信箱'><item><order>DESC</order><word>"
        + StringUtil.GetXmlStringSimple(strUserID + "|" + strBox + "|")
        + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item>"
        + "<lang>zh</lang></target>";
            }

            else if (String.Compare(strStyle, "untouched") == 0)
            {
                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(this.MessageDbName)       // 2007/9/14 new add
                    + ":用户名信箱'><item><order>DESC</order><word>"
        + StringUtil.GetXmlStringSimple(strUserID + "|" + strBox + "|0")
        + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item>"
        + "<lang>zh</lang></target>";
            }

            else if (String.Compare(strStyle, "touched") == 0)
            {
                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(this.MessageDbName)       // 2007/9/14 new add
                    + ":用户名信箱'><item><order>DESC</order><word>"
        + StringUtil.GetXmlStringSimple(strUserID + "|" + strBox + "|1")
        + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item>"
        + "<lang>zh</lang></target>";
            }
            else
            {
                // text-level: 内部错误
                strError = "未知的strStyle类型 '" + strStyle + "'";
                return -1;
            }

            return 0;
        }

        // TODO: 需要增加一个功能，把显示名转化为条码号
        // 校验收件人是否存在
        // parameters:
        // return:
        //      -1  error
        //      0   not exist
        //      1   exist
        public int DoVerifyRecipient(
            RmsChannelCollection channels,
            string strRecipient,
            out string strError)
        {
            strError = "";

            if (this.VerifyAccount == null)
            {
                // text-level: 内部错误
                strError = "尚未挂接VerifyRecipient事件，无法校验收件人的存在与否。";
                return -1;
            }

            VerifyAccountEventArgs e = new VerifyAccountEventArgs();
            e.Name = strRecipient;
            e.Channels = channels;
            this.VerifyAccount(this, e);
            if (e.Error == true)
            {
                strError = e.ErrorInfo;
                return -1;
            }
            if (e.Exist == false)
            {
                if (String.IsNullOrEmpty(e.ErrorInfo) == true)
                {
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("收件人s不存在"),   // "收件人 '{0}' 不存在。"
                        strRecipient);
                        // "收件人 '" + strRecipient + "' 不存在。";
                    return 0;
                }

                strError = e.ErrorInfo;
                return 0;
            }

            return 1;
        }

        const string EncryptKey = "dp2circulationpassword";

        // 加密明文
        public static string EncryptPassword(string PlainText)
        {
            return Cryptography.Encrypt(PlainText, EncryptKey);
        }

        // 解密加密过的文字
        public static string DecryptPassword(string EncryptText)
        {
            return Cryptography.Decrypt(EncryptText, EncryptKey);
        }

        public static string BuildOneAddress(string strDisplayName, string strBarcode)
        {
            string strAddress = "";
            if (String.IsNullOrEmpty(strDisplayName) == false)
            {
                if (strDisplayName.IndexOf("[") == -1)
                    strAddress = "[" + strDisplayName + "]";
                else
                    strAddress = strDisplayName;

                string strEncryptBarcode = MessageCenter.EncryptPassword(strBarcode);

                if (String.IsNullOrEmpty(strEncryptBarcode) == false)
                    strAddress += "=encrypt_barcode:" + strEncryptBarcode;
            }
            else
                strAddress = strBarcode;

            return strAddress;
        }

        int ParseAddress(string strAddress,
            out List<string> ids,
            out List<string> origins,
            out string strError)
        {
            ids = new List<string>();
            origins = new List<string>();
            strError = "";
            int nRet = 0;

            string[] parts = strAddress.Split(new char [] {';',','});
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;

                string strLeft = "";
                string strRight = "";
                nRet = strPart.IndexOf("=");
                if (nRet == -1)
                    strLeft = strPart;
                else
                {
                    strLeft = strPart.Substring(0, nRet).Trim();
                    strRight = strPart.Substring(nRet + 1).Trim();
                }

                if (String.IsNullOrEmpty(strRight) == false)
                    strLeft = strRight;

                // encrypt_barcode
                nRet = strLeft.IndexOf(":");
                if (nRet != -1)
                {
                    string strName = "";
                    string strValue = "";

                    strName = strLeft.Substring(0, nRet).Trim();
                    strValue = strRight.Substring(nRet + 1);

                    if (strName == "encrypt_barcode")
                        strLeft = DecryptPassword(strValue);
                    else
                    {
                        strError = "无法识别的部件名称 '"+strName+"'";
                        return -1;
                    }

                }
                else if (strLeft.IndexOf("[") != -1)
                {
                    // 单独一个 显示名
                    // TODO: 需要翻译为条码号?

                }

                ids.Add(strLeft);
                origins.Add(strPart);
            }

            return 0;
        }

        // 发送消息
        // parameters:
        //      bVerifyRecipient    是否验证收件人地址
        // return:
        //      -1  出错
        //      0   成功
        public int SendMessage(
            RmsChannelCollection Channels,
            string strRecipient,
            string strSender,
            string strSubject,
            string strMime,
            string strBody,
            bool bVerifyRecipient,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strRecipient) == true)
            {
                // text-level: 用户提示
                strError = this.GetString("收件人不能为空");    // "收件人不能为空"
                return -1;
            }

            List<string> userids = null;
            List<string> origins = null;
            nRet = ParseAddress(strRecipient,
            out userids,
            out origins,
            out strError);
            if (nRet == -1)
            {
                strError = "收件人地址格式不正确: " + strError;
                return -1;
            }

            for (int i = 0; i < userids.Count; i++)
            {
                string strUserID = userids[i];
                string strOrigin = origins[i];

                if (bVerifyRecipient == true)
                {
                    // 校验收件人是否存在
                    // parameters:
                    // return:
                    //      -1  error
                    //      0   not exist
                    //      1   exist
                    nRet = DoVerifyRecipient(
                        Channels,
                        strUserID,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        return -1;
                }

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");

                DomUtil.SetElementText(dom.DocumentElement,
                    "sender", strSender);
                DomUtil.SetElementText(dom.DocumentElement,
                    "recipient", strOrigin);    // fullname
                DomUtil.SetElementText(dom.DocumentElement,
                    "subject", strSubject);
                DomUtil.SetElementText(dom.DocumentElement,
                    "date", DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.UtcNow.ToLocalTime()));
                DomUtil.SetElementText(dom.DocumentElement,
                    "size", Convert.ToString(strBody.Length));
                DomUtil.SetElementText(dom.DocumentElement,
                    "touched", "0");
                DomUtil.SetElementText(dom.DocumentElement,
                    "username", strUserID);
                DomUtil.SetElementText(dom.DocumentElement,
                    "box", MessageCenter.INBOX);
                DomUtil.SetElementTextPure(dom.DocumentElement,
                    "content", strBody);
                DomUtil.SetElementText(dom.DocumentElement,
                    "mime", strMime);

                RmsChannel channel = Channels.GetChannel(this.ServerUrl);

                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

                // 写入收件箱
                long lRet = channel.DoSaveTextRes(this.MessageDbName + "/?",
                    dom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    // text-level: 内部错误
                    strError = "写入收件箱时出错: " + strError;
                    return -1;
                }

                DomUtil.SetElementText(dom.DocumentElement,
                    "username", strSender);
                DomUtil.SetElementText(dom.DocumentElement,
                    "box", MessageCenter.OUTBOX);


                // 写入已发送信箱
                lRet = channel.DoSaveTextRes(this.MessageDbName + "/?",
        dom.OuterXml,
        false,
        "content,ignorechecktimestamp",
        null,
        out output_timestamp,
        out strOutputPath,
        out strError);
                if (lRet == -1)
                {
                    // text-level: 内部错误
                    strError = "写入已发送信箱时出错: " + strError;
                    return -1;
                }
            }

            return 0;
        }

        // 发送草稿箱中的一条消息
        // parameters:
        //      bVerifyRecipient    是否验证收件人地址
        public int SendMessage(
            string strMessageID,
            string strRecipient,
            bool bVerifyRecipient,
            out string strError)
        {
            strError = "";

            return 0;
        }

        // 保存消息到"草稿"箱
        // parameters:
        //      strOldRecordID  原来在草稿箱中的记录id。如果有此id，用覆盖方式写入，否则用追加方式写入
        public int SaveMessage(
            RmsChannelCollection Channels,
            string strRecipient,
            string strSender,
            string strSubject,
            string strMime,
            string strBody,
            string strOldRecordID,
            byte [] baOldTimeStamp,
            out byte[] baOutputTimeStamp,
            out string strOutputID,
            out string strError)
        {
            strError = "";
            baOutputTimeStamp = null;
            strOutputID = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            DomUtil.SetElementText(dom.DocumentElement,
                "sender", strSender);
            DomUtil.SetElementText(dom.DocumentElement,
                "recipient", strRecipient);
            DomUtil.SetElementText(dom.DocumentElement,
                "subject", strSubject);
            DomUtil.SetElementText(dom.DocumentElement,
                "date", DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.UtcNow.ToLocalTime()));
            DomUtil.SetElementText(dom.DocumentElement,
                "size", Convert.ToString(strBody.Length));
            DomUtil.SetElementText(dom.DocumentElement,
                "touched", "0");
            DomUtil.SetElementText(dom.DocumentElement,
                "username", strSender);
            DomUtil.SetElementText(dom.DocumentElement,
                "box", MessageCenter.TEMP);
            DomUtil.SetElementTextPure(dom.DocumentElement,
                "content", strBody);
            DomUtil.SetElementText(dom.DocumentElement,
    "mime", strMime);

            RmsChannel channel = Channels.GetChannel(this.ServerUrl);

            // byte[] timestamp = null;
            // byte[] output_timestamp = null;
            string strOutputPath = "";

            string strPath = "";

            if (String.IsNullOrEmpty(strOldRecordID) == true)
            {
                strPath = this.MessageDbName + "/?";
            }
            else
            {
                strPath = this.MessageDbName + "/" + strOldRecordID;
            }

            // 写回册记录
            long lRet = channel.DoSaveTextRes(strPath,
                dom.OuterXml,
                false,
                "content,ignorechecktimestamp",
                baOldTimeStamp,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                return -1;
            }

            strOutputID = ResPath.GetRecordId(strOutputPath);

            return 0;
        }

        // 观察收件箱状态
        public int CheckInbox(string strUserName,
            out int nMessageCount,
            out string strError)
        {
            nMessageCount = 0;
            strError = "";

            return 0;
        }

        // 一次性获得许多消息
        // parameters:
        //      message_ids 消息ID的数组。如果字符串中包含'/'，则是路径，否则就是id
        public int GetMessage(
    RmsChannelCollection Channels,
    string strUserID,
    string[] message_ids,
    MessageLevel messagelevel,
    out List<MessageData> messages,
    out string strError)
        {
            strError = "";
            messages = new List<MessageData>();

            RmsChannel channel = Channels.GetChannel(this.ServerUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            for (int i = 0; i < message_ids.Length; i++)
            {
                string strID = message_ids[i];

                string strPath = strID;
                if (strID.IndexOf("/") == -1)
                    strPath = this.MessageDbName + "/" + strID;
                MessageData message = null;

                int nRet = 0;

                nRet = GetMessageByPath(
                    channel,
                    strPath,
                    messagelevel,
                    out message,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (strUserID != null && message.strUserName != strUserID)
                {
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("此条消息不属于用户s, 不允许察看"),  // "此条消息不属于用户 '{0}', 不允许察看。"
                        strUserID);
                    // "此条消息不属于用户 '" +strUserID+ "', 不允许察看。";
                    return -1;
                }

                messages.Add(message);
            }

            return 1;
        }

        // 根据消息记录id获得消息详细内容
        // 本函数还将检查消息是否属于strUserID指明的用户
        // parameters:
        //      strUserID   如果==null，表示不检查消息属于何用户
        public int GetMessage(
            RmsChannelCollection Channels,
            string strUserID,
            string strMessageID,
            MessageLevel messagelevel,
            out MessageData message,
            out string strError)
        {
            strError = "";
            message = null;

            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.ServerUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string strPath = this.MessageDbName + "/" + strMessageID;

            nRet = GetMessageByPath(
                channel,
                strPath,
                messagelevel,
                out message,
                out strError);
            if (nRet == -1)
                return -1;

            if (strUserID != null && message.strUserName != strUserID)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("此条消息不属于用户s, 不允许察看"),  // "此条消息不属于用户 '{0}', 不允许察看。"
                    strUserID);
                    // "此条消息不属于用户 '" +strUserID+ "', 不允许察看。";
                return -1;
            }

            return 1;
        }

        // 根据消息记录路径获得消息
        // 不检查消息是否属于特定用户
        int GetMessageByPath(
            RmsChannel channel,
            string strPath,
            MessageLevel messagelevel,
            out MessageData data,
            out string strError)
        {
            data = new MessageData();

            string strMetaData = "";
            byte[] timestamp = null;
            string strXml = "";
            string strOutputPath = "";

            long lRet = channel.GetRes(strPath,
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                // text-level: 内部错误
                strError = "获得消息记录 '" + strPath + "' 时出错: " + strError;
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                // text-level: 内部错误
                strError = "装载XML记录进入DOM时出错: " + ex.Message;
                return -1;
            }


            data.strSender = DomUtil.GetElementText(dom.DocumentElement,
                "sender");
            data.strRecipient = DomUtil.GetElementText(dom.DocumentElement,
                "recipient");
            data.strSubject = DomUtil.GetElementText(dom.DocumentElement,
                "subject");
            data.strCreateTime = DomUtil.GetElementText(dom.DocumentElement,
                "date");
            data.strMime = DomUtil.GetElementText(dom.DocumentElement,
                "mime");

            data.strSize = DomUtil.GetElementText(dom.DocumentElement,
                "size");

            string strTouched = DomUtil.GetElementText(dom.DocumentElement,
                "touched");
            if (strTouched == "1")
                data.Touched = true;
            else
                data.Touched = false;

            data.strRecordID = ResPath.GetRecordId(strOutputPath);

            if (messagelevel == MessageLevel.Full)
            {
                data.strBody = DomUtil.GetElementText(dom.DocumentElement,
                    "content");
            }

            data.strUserName = DomUtil.GetElementText(dom.DocumentElement,
                "username");

            // 恒定为中文名称
            data.strBoxType = DomUtil.GetElementText(dom.DocumentElement,
                "box");

            data.TimeStamp = timestamp;

            // 修改touched元素值
            if (messagelevel == MessageLevel.Full
                && data.Touched == false)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "touched", "1");

                byte[] output_timestamp = null;
                //string strOutputPath = "";

                lRet = channel.DoSaveTextRes(strPath,
                    dom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    // text-level: 内部错误
                    strError = "写回记录 '"+strPath+"' 时出错: " + strError;
                    return -1;
                }
                data.Touched = true;
                data.TimeStamp = output_timestamp;

            }

            return 1;
        }


        // 获得未读消息数
        public int GetUntouchedMessageCount(
            RmsChannelCollection Channels,
            string strUserID,
            string strBoxType,
            out string strError)
        {
            RmsChannel channel = Channels.GetChannel(this.ServerUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string strQueryXml = "";

            // 构造检索式
            int nRet = MakeSearchQuery(
                strUserID,
                strBoxType,
                "untouched",   // 信箱内全部邮件
                out strQueryXml,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "构造检索式出错: " + strError;
                return -1;
            }


            long lRet = channel.DoSearch(strQueryXml,
                "null",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
            {
                // text-level: 内部错误
                strError = "检索失败: " + strError;
                return -1;
            }

            return (int)lRet;
        }


        // 检索获得消息, 或者从结果集中获得消息
        // parameters:
        //      strStyle    search / untouched / touched
        //                  有search表示进行检索和获取，没有search就表示不检索而获取先前检索的结果集。
        //                  untoched和touched应当和search联用。否则只能获取先前的结果数
        public int GetMessage(
            RmsChannelCollection Channels,
            string strResultsetName,
            string strStyle,
            string strUserID,
            string strBoxType,
            MessageLevel messagelevel,
            int nStart,
            int nCount,
            out int nTotalCount,
            out List<MessageData> messages,
            out string strError)
        {
            nTotalCount = 0;
            messages = new List<MessageData>();
            strError = "";

            if (String.IsNullOrEmpty(this.MessageDbName) == true)
            {
                strError = "消息库尚未定义";
                return -1;
            }

            int nRet = 0;
            long lRet = 0;

            if (String.IsNullOrEmpty(strBoxType) == true)
            {
                strBoxType = MessageCenter.INBOX;
            }

            RmsChannel channel = Channels.GetChannel(this.ServerUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }


            if (String.IsNullOrEmpty(strResultsetName) == true)
                strResultsetName = "messages_of_" + strBoxType;

            bool bSearch = true;
            if (StringUtil.IsInList("search", strStyle) == true)
                bSearch = true;
            else
                bSearch = false;

            string strQueryStyle = "";
            if (StringUtil.IsInList("touched", strStyle) == true)
                strQueryStyle = "touched";
            else if (StringUtil.IsInList("untouched", strStyle) == true)
                strQueryStyle = "untouched";

            // 检索
            if (bSearch == true)
            {

                string strQueryXml = "";

                // 构造检索式
                nRet = MakeSearchQuery(
                    strUserID,
                    strBoxType,
                    strQueryStyle,
                    out strQueryXml,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "构造检索式出错: " + strError;
                    return -1;
                }


                lRet = channel.DoSearch(strQueryXml,
                    strResultsetName,
                    "", // strOuputStyle
                    out strError);
                if (lRet == -1)
                {
                    // text-level: 内部错误
                    strError = "检索失败: " + strError;
                    return -1;
                }

                // not found
                if (lRet == 0)
                {
                    // text-level: 用户提示
                    strError = this.GetString("没有任何消息");  // "没有任何消息"
                    return 0;
                }

                nTotalCount = (int)lRet;
            }


            if (nCount == 0)
                return nTotalCount;   // 如果不需要获得结果集

            Debug.Assert(nStart >= 0, "");


            // 获得结果集中指定范围的记录路径
            ArrayList aLine = null;
            lRet = channel.DoGetSearchFullResult(
                strResultsetName,
                nStart,
                nCount,
                "zh",
                null,
                out aLine,
                out strError);
            if (lRet == -1)
            {
                // 虽然返回-1,但是aLine中仍然有内容了
                if (aLine == null)
                {
                    // text-level: 内部错误
                    strError = "获取浏览格式失败: " + strError;
                    return -1;
                }
            }

            // 返回数据
            for (int i = 0; i < aLine.Count; i++)
            {
                string[] cols = null;

                cols = (string[])aLine[i];

                string strPath = cols[0];

                MessageData data = null;

                // TODO: level == none 只返回路径
                nRet = GetMessageByPath(
                    channel,
                    strPath,
                    messagelevel,
                    out data,
                    out strError);
                if (nRet == -1)
                    return -1;

                messages.Add(data);

            }

            return aLine.Count;
        }

        // 删除一个box中的全部消息
        public int DeleteMessage(
            RmsChannelCollection Channels,
            string strUserID,
            bool bMoveToRecycleBin,
            string strBoxType,
            out string strError)
        {
            strError = "";

            int nStart = 0;
            int nCount = -1;
                int nTotalCount = 0;
            for (; ; )
            {
                List<MessageData> messages = null;

                int nRet = GetMessage(
                    Channels,
                    "message_deleteall",
                    nStart == 0 ? "search" : "",
                    strUserID,
                    strBoxType,
                    MessageLevel.ID,
                    nStart,
                    nCount,
                out nTotalCount,
                out messages,
                out strError);
                if (nRet == -1)
                    return -1;

                if (nCount == -1)
                    nCount = nTotalCount - nStart;

                for (int j = 0; j < messages.Count; j++)
                {
                    MessageData data = messages[j];

                    nRet = DeleteMessage(
                        bMoveToRecycleBin,
                        Channels,
                        data.strRecordID,
                        data.TimeStamp,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }


                nStart += messages.Count;
                nCount -= messages.Count;
                
                if (nStart >= nTotalCount)
                    break;
                if (nCount <= 0)
                    break;
            }


            return nTotalCount;
        }

        // 删除消息
        public int DeleteMessage(
            bool bMoveToRecycleBin,
            RmsChannelCollection Channels,
            List<string> ids,
            List<byte[]> timestamps,
            out string strError)
        {
            strError = "";

            string strError1 = "";

//            Channel channel = Channels.GetChannel(this.ServerUrl);

            for (int i = 0; i<ids.Count; i++)
            {
                string strID = ids[i];

                // string strPath = this.MessageDbName + "/" + strID;

                byte [] timestamp = null;
                // byte[] output_timestamp = null;

                if (timestamps != null)
                    timestamp = timestamps[i];

                int nRet = DeleteMessage(
                    bMoveToRecycleBin,
                    Channels,
                    strID,
                    timestamp,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError1 += "删除记录 '" +strID+ "' 时发生错误: "+ strError + ";";
                }

            }

            if (strError1 != "")
            {
                strError = strError1;
                return -1;
            }

            return 0;
        }

        // 删除一条消息
        public int DeleteMessage(
            bool bMoveToRecycleBin,
            RmsChannelCollection Channels,
            string strID,
            byte [] timestamp,
            out string strError)
        {
            RmsChannel channel = Channels.GetChannel(this.ServerUrl);

            string strPath = this.MessageDbName + "/" + strID;

            long lRet = 0;
            byte[] output_timestamp = null;


            // 要移动到废件箱
            if (bMoveToRecycleBin == true)
            {
                string strXml = "";
                string strMetaData = "";
                string strOutputPath = "";

                // 读出记录
                lRet = channel.GetRes(strPath,
                    out strXml,
                    out strMetaData,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    // text-level: 内部错误
                    strError = "读出记录 '" + strPath + "' 时出错: " + strError;
                    return -1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    // text-level: 内部错误
                    strError = "装载XML记录进入DOM时出错: " + ex.Message;
                    return -1;
                }

                // 修改box参数
                DomUtil.SetElementText(dom.DocumentElement,
                    "box", MessageCenter.RECYCLEBIN);

                timestamp = output_timestamp;
                // 写回
                lRet = channel.DoSaveTextRes(strPath,
    dom.OuterXml,
    false,
    "content,ignorechecktimestamp",
    timestamp,
    out output_timestamp,
    out strOutputPath,
    out strError);
                if (lRet == -1)
                {
                    // text-level: 内部错误
                    strError = "写回记录 '" + strPath + "' 时出错: " + strError;
                    return -1;
                }

                return 0;
            }

            bool bNullTimeStamp = false;
            if (timestamp == null)
                bNullTimeStamp = true;


        REDO:
            lRet = channel.DoDeleteRes(strPath,
                timestamp,
                out output_timestamp,
                out strError);
            if (lRet == -1)
            {
                if (bNullTimeStamp == true
                    && channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    timestamp = output_timestamp;
                    goto REDO;
                }

                // text-level: 内部错误
                strError = "删除记录 '" + strPath + "' 时发生错误: " + strError;
                return -1;
            }

            return 0;
        }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public enum MessageLevel
    {
        [EnumMember]
        ID = 0,    // 只返回ID
        [EnumMember]
        Summary = 1,    // 摘要级，不返回body
        [EnumMember]
        Full = 2,   // 全部级，返回全部信息
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class MessageData
    {
        [DataMember]
        public string strUserName = ""; // 消息所从属的用户ID
        [DataMember]
        public string strBoxType = "";

        [DataMember]
        public string strSender = "";   // 发送者
        [DataMember]
        public string strRecipient = "";    // 接收者
        [DataMember]
        public string strSubject = "";  // 主题
        [DataMember]
        public string strMime = ""; // 媒体类型
        [DataMember]
        public string strBody = "";
        [DataMember]
        public string strCreateTime = "";   // 邮件创建(收到)时间
        [DataMember]
        public string strSize = "";     // 尺寸
        [DataMember]
        public bool Touched = false;    // 是否阅读过
        [DataMember]
        public string strRecordID = ""; // 记录ID。用于唯一定位一条消息

        [DataMember]
        public byte[] TimeStamp = null;

        public MessageData()
        {
        }

        public MessageData(MessageData origin)
        {
            this.strUserName = origin.strUserName;
            this.strBoxType = origin.strBoxType;

            this.strSender = origin.strSender;
            this.strRecipient = origin.strRecipient;
            this.strSubject = origin.strSubject;
            this.strMime = origin.strMime;
            this.strBody = origin.strBody;
            this.strCreateTime = origin.strCreateTime;
            this.strSize = origin.strSize;
            this.Touched = origin.Touched;

            this.strRecordID = origin.strRecordID;
            this.TimeStamp = origin.TimeStamp;
        }
    }

    public class Box
    {
        public string Name = "";
        public string Type = "";    // 类型

        /*
        INBOX = "收件箱";
        TEMP = "草稿";
        OUTBOX = "已发送";
        RECYCLEBIN = "废件箱";
         * */

    }

    // 校验一个帐户名是否存在
    public delegate void VerifyAccountEventHandler(object sender,
    VerifyAccountEventArgs e);

    public class VerifyAccountEventArgs : EventArgs
    {
        public RmsChannelCollection Channels = null;
        public string Name = "";    // [in]
        public bool Exist = false;  // [out]
        public bool Error = false;  // [out] 只有当Exist == false的时候，Error才能 == true
        public string ErrorInfo = "";   // [out] 当Exist==false，或者Error == true的时候，都应当返回出错信息。
    }
}
