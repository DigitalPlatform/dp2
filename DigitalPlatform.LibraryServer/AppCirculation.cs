using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Net.Mail;
using System.Web;

using System.Resources;
using System.Globalization;
using System.Runtime.Serialization;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
using DigitalPlatform.Range;
using DigitalPlatform.Drawing;  // ShrinkPic()

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是流通业务相关的代码
    /// </summary>
    public partial class LibraryApplication
    {

        // 为统计指标"出纳/读者数"而暂存的(流通操作)最后一位读者的证条码。可能不太准确
        // string m_strLastReaderBarcode = "";

        // 读者记录中，借阅历史中最大保存个数
        public int MaxPatronHistoryItems = 10;  // 100;

        // 册记录中，借阅历史中最大保存个数
        public int MaxItemHistoryItems = 10;    // 100;


        public bool VerifyBarcode = false;  // 创建和修改读者记录、册记录的时候是否验证条码号

        public bool AcceptBlankItemBarcode = true;
        public bool AcceptBlankReaderBarcode = true;

        public bool VerifyBookType = false;  // 创建和修改册记录的时候是否验证图书类型
        public bool VerifyReaderType = false;  // 创建和修改读者记录的时候是否验证读者类型
        public bool BorrowCheckOverdue = true;  // 借书的时候是否检查未还超期册

        public string CirculationNotifyTypes = "";  // 流通操作时发出实时通知的类型
#if NO
        // 保存资源
        // return:
        //		-1	error
        //		0	发现上载的文件其实为空，不必保存了
        //		1	已经保存
        public static int SaveUploadFile(
            System.Web.UI.Page page,
            RmsChannel channel,
            string strXmlRecPath,
            string strFileID,
            string strResTimeStamp,
            HttpPostedFile postedFile,
            int nLogoLimitW,
            int nLogoLimitH,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(postedFile.FileName) == true
                && postedFile.ContentLength == 0)
            {
                return 0;	// 没有必要保存
            }

            WebPageStop stop = new WebPageStop(page);

            string strResPath = strXmlRecPath + "/object/" + strFileID;

            string strLocalFileName = Path.GetTempFileName();
            try
            {
                using (Stream t = File.Create(strLocalFileName))
                {
                    // 缩小尺寸
                    int nRet = GraphicsUtil.ShrinkPic(postedFile.InputStream,
                            postedFile.ContentType,
                            nLogoLimitW,
                            nLogoLimitH,
                            true,
                            t,
                            out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)  // 没有必要缩放
                    {
                        postedFile.InputStream.Seek(0, SeekOrigin.Begin); // 2012/5/20
                        StreamUtil.DumpStream(postedFile.InputStream, t);
                    }
                }

            // t.Close();


                // 检测文件尺寸
                FileInfo fi = new FileInfo(strLocalFileName);

                if (fi.Exists == false)
                {
                    strError = "文件 '" + strLocalFileName + "' 不存在...";
                    return -1;
                }

                string[] ranges = null;

                if (fi.Length == 0)
                { // 空文件
                    ranges = new string[1];
                    ranges[0] = "";
                }
                else
                {
                    string strRange = "";
                    strRange = "0-" + Convert.ToString(fi.Length - 1);

                    // 按照100K作为一个chunk
                    ranges = RangeList.ChunkRange(strRange,
                        100 * 1024);
                }

                byte[] timestamp = ByteArray.GetTimeStampByteArray(strResTimeStamp);
                byte[] output_timestamp = null;

                // 2007/12/13
                string strLastModifyTime = DateTime.UtcNow.ToString("u");

                string strLocalPath = postedFile.FileName;

                // page.Response.Write("<br/>正在保存" + strLocalPath);

            REDOWHOLESAVE:
                string strWarning = "";

                for (int j = 0; j < ranges.Length; j++)
                {
                REDOSINGLESAVE:

                    // Application.DoEvents();	// 出让界面控制权

                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strWaiting = "";
                    if (j == ranges.Length - 1)
                        strWaiting = " 请耐心等待...";

                    string strPercent = "";
                    RangeList rl = new RangeList(ranges[j]);
                    if (rl.Count >= 1)
                    {
                        double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
                        strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                    }

                    if (stop != null)
                        stop.SetMessage("正在上载 " + ranges[j] + "/"
                            + Convert.ToString(fi.Length)
                            + " " + strPercent + " " + strLocalFileName + strWarning + strWaiting);

                    // page.Response.Write(".");	// 防止前端因等待过久而超时

                    long lRet = channel.DoSaveResObject(strResPath,
                        strLocalFileName,
                        strLocalPath,
                        postedFile.ContentType,
                        strLastModifyTime,
                        ranges[j],
                        j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
                        timestamp,
                        out output_timestamp,
                        out strError);

                    timestamp = output_timestamp;

                    // DomUtil.SetAttr(node, "__timestamp",	ByteArray.GetHexTimeStampString(timestamp));

                    strWarning = "";

                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {

                            timestamp = new byte[output_timestamp.Length];
                            Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
                            strWarning = " (时间戳不匹配, 自动重试)";
                            if (ranges.Length == 1 || j == 0)
                                goto REDOSINGLESAVE;
                            goto REDOWHOLESAVE;
                        }

                        goto ERROR1;
                    }


                }


                return 1;	// 已经保存
            ERROR1:
                return -1;
            }
            finally
            {
                // 不要忘记删除临时文件
                File.Delete(strLocalFileName);
            }
        }

#endif

        // 根据读者证条码号找到头像资源路径
        // parameters:
        //      strReaderBarcode    读者证条码号
        //      strEncryptBarcode   如果strEncryptBarcode有内容，则用它，而不用strReaderBarcode
        //      strDisplayName  供验证的显示名。可以为null，表示不验证
        // return:
        //      -1  出错
        //      0   没有找到。包括读者记录不存在，或者读者记录里面没有头像对象
        //      1   找到
        public int GetReaderPhotoPath(
            SessionInfo sessioninfo,
            string strReaderBarcode,
            string strEncyptBarcode,
            string strDisplayName,
            out string strPhotoPath,
            out string strError)
        {
            strError = "";
            strPhotoPath = "";

            if (String.IsNullOrEmpty(strEncyptBarcode) == false)
            {
                string strTemp = LibraryApplication.DecryptPassword(strEncyptBarcode);
                if (strTemp == null)
                {
                    strError = "strEncyptBarcode中包含的文字格式不正确";
                    return -1;
                }
                strReaderBarcode = strTemp;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 读入读者记录
            string strReaderXml = "";
            byte[] reader_timestamp = null;
            string strOutputReaderRecPath = "";
            int nRet = this.GetReaderRecXml(
                // sessioninfo.Channels,
                channel,
                strReaderBarcode,
                out strReaderXml,
                out strOutputReaderRecPath,
                out reader_timestamp,
                out strError);
            if (nRet == 0)
            {
                strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                return 0;
            }
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "读入读者记录时发生错误: " + strError;
                return -1;
            }

            if (nRet > 1)
            {
                // text-level: 内部错误
                strError = "读入读者记录时，发现读者证条码号 '" + strReaderBarcode + "' 命中 " + nRet.ToString() + " 条，这是一个严重错误，请系统管理员尽快处理。";
                return -1;
            }

            XmlDocument readerdom = null;
            nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                return -1;
            }

            // 验证显示名
            if (String.IsNullOrEmpty(strDisplayName) == false)
            {
                string strDisplayNameValue = DomUtil.GetElementText(readerdom.DocumentElement,
                        "displayName");
                if (strDisplayName.Trim() != strDisplayNameValue.Trim())
                {
                    strError = "虽然读者记录找到了，但是显示名已经不匹配";
                    return 0;
                }
            }

            // 看看是不是已经有图像对象

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            // 全部<dprms:file>元素
            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("//dprms:file[@usage='photo']", nsmgr);

            if (nodes.Count == 0)
            {
                strError = "读者记录中没有头像对象";
                return 0;
            }

            strPhotoPath = strOutputReaderRecPath + "/object/" + DomUtil.GetAttr(nodes[0], "id");

            return 1;
        }

        // 获得读者库的馆代码
        // return:
        //      -1  出错
        //      0   成功
        public int GetLibraryCode(
            string strReaderRecPath,
            out string strLibraryCode,
            out string strError)
        {
            strLibraryCode = "";
            strError = "";

            string strReaderDbName = ResPath.GetDbName(strReaderRecPath);
            bool bReaderDbInCirculation = true;
            if (this.IsReaderDbName(strReaderDbName,
                out bReaderDbInCirculation,
                out strLibraryCode) == false)
            {
                // text-level: 内部错误
                strError = "读者记录路径 '" + strReaderRecPath + "' 中的数据库名 '" + strReaderDbName + "' 不在定义的读者库之列。";
                return -1;
            }
            return 0;
        }

        static string GetBorrowActionName(string strAction)
        {
            if (strAction == "borrow")
            {
                return "借阅";
            }
            else if (strAction == "renew")
            {
                return "续借";
            }
            else return strAction;
        }

        static string GetLibLocCode(string strLibraryUid)
        {
            if (string.IsNullOrEmpty(strLibraryUid) == true)
                return "";
            string strResult = "";
            if (strLibraryUid.Length > 0)
                strResult = strLibraryUid.Substring(0, 1);
            if (strLibraryUid.Length > 1)
                strResult += strLibraryUid.Substring(strLibraryUid.Length - 1, 1);

            return strResult;
        }

        // 根据读者证条码号构造二维码字符串
        public static string BuildQrCode(string strReaderBarcode,
            string strLibraryUid)
        {
            DateTime now = DateTime.UtcNow;
            // 时效字符串 20130101
            string strDateString = DateTimeUtil.DateTimeToString8(now);
            string strSalt = strDateString + "|" + strReaderBarcode + "|" + GetLibLocCode(strLibraryUid);
            string strHash = BuildPqrHash(strSalt);
            return "PQR:" + strReaderBarcode + "@" + strHash;
        }

        static string BuildPqrHash(string strText)
        {
            return Cryptography.GetSHA1(strText).ToUpper().Replace("+", "").Replace("/", "").Replace("=", "");
        }

        public int DecodeQrCode(string strCode,
            out string strReaderBarcode,
            out string strError)
        {
            strError = "";
            strReaderBarcode = "";


            if (string.IsNullOrEmpty(strCode) == true
                || strCode.Length < "PQR:".Length
                || StringUtil.HasHead(strCode, "PQR:") == false)
            {
                strError = "不是读者证号二维码";
                return 0;
            }

            strCode = strCode.Substring("PQR:".Length);

            string strHashcode = "";

            int nRet = strCode.IndexOf("@");
            if (nRet != -1)
            {
                strReaderBarcode = strCode.Substring(0, nRet);
                strHashcode = strCode.Substring(nRet + 1);
            }
            else
            {
                strError = "PQR 号码格式错误: 缺乏字符 '@'";
                return -1;
            }

            string strLibraryUid = this.UID;
            string strSalt = DateTimeUtil.DateTimeToString8(DateTime.Now) + "|" + strReaderBarcode + "|" + GetLibLocCode(strLibraryUid);
            string strVerify = BuildPqrHash(strSalt);

            if (strVerify != strHashcode)
            {
                strError = "PQR 号码格式错误: 校验失败";
                return -1;
            }

            return 1;
        }

#if NO
        // 根据读者证条码号构造二维码字符串
        public static string BuildQrCode(string strReaderBarcode,
            string strLibraryUid)
        {
            DateTime now = DateTime.UtcNow;
            // 时效字符串 开始点:长度 ， 都是 ticks 数字
            string strDateString = now.Ticks.ToString() + ":" + new TimeSpan(24, 0, 0).Ticks.ToString();
            return "PQR:" + Cryptography.Encrypt(strDateString + "|" + strReaderBarcode + "@" + strLibraryUid, LibraryApplication.qrkey);
        }

        public int DecodeQrCode(string strCode,
            out string strReaderBarcode,
            out string strError)
        {
            string strLibraryUid = "";
            int nRet = DecodeQrCode(strCode,
                out strReaderBarcode,
                out strLibraryUid,
                out strError);
            if (nRet != 1)
                return nRet;
            if (strLibraryUid != this.UID)
            {
                strError = "不是本馆的读者证号二维码";
                return -1;
            }

            return 1;
        }


        // 把二维码字符串转换为读者证条码号
        // parameters:
        //      strReaderBcode  [out]读者证条码号
        // return:
        //      -1      出错
        //      0       所给出的字符串不是读者证号二维码
        //      1       成功      
        public static int DecodeQrCode(string strCode,
            out string strReaderBarcode,
            out string strLibraryUid,
            out string strError)
        {
            strError = "";
            strReaderBarcode = "";
            strLibraryUid = "";

            if (string .IsNullOrEmpty(strCode) == true
                || strCode.Length < "PQR:".Length
                || StringUtil.HasHead(strCode, "PQR:") == false)
            {
                strError = "不是读者证号二维码";
                return 0;
            }

            strCode = strCode.Substring("PQR:".Length);

            // 解密
            try
            {
                string strPlainText = Cryptography.Decrypt(strCode, LibraryApplication.qrkey);

                // 时效部分
                int nRet = strPlainText.IndexOf("|");
                if (nRet == -1)
                {
                    strError = "号码格式错误";
                    return -1;
                }
                string strTimeString = strPlainText.Substring(0, nRet);
                string strTemp = strPlainText.Substring(nRet + 1);

                // 检查时效性
                nRet = strTimeString.IndexOf(":");
                if (nRet == -1)
                {
                    strError = "号码格式错误";
                    return -1;
                }
                string strStart = strTimeString.Substring(0, nRet);
                string strLength = strTimeString.Substring(nRet + 1);
                long lStart = 0;
                if (long.TryParse(strStart, out lStart) == false)
                {
                    // 第一个数字部分格式错误
                    strError = "号码格式错误";
                    return -1;
                }
                long lLength = 0;
                if (long.TryParse(strLength, out lLength) == false)
                {
                    // 第二个数字部分格式错误
                    strError = "号码格式错误";
                    return -1;
                }
                DateTime start = new DateTime(lStart);
                TimeSpan delta = new TimeSpan(lLength);
                DateTime now = DateTime.UtcNow;
                if (now < start || now >= start + delta)
                {
                    strError = "号码已经失效";
                    return -1;
                }

                nRet = strTemp.IndexOf("@");
                if (nRet != -1)
                {
                    strReaderBarcode = strTemp.Substring(0, nRet);
                    strLibraryUid = strTemp.Substring(nRet + 1);
                }
                else
                    strReaderBarcode = strTemp;
                return 1;
            }
            catch(Exception ex)
            {
                strError = "号码格式错误";
                return -1;
            }
        }

#endif

        // 检查评估模式
        // return:
        //      -1  检查过程出错
        //      0   可以通过
        //      1   不允许通过
        public static int CheckTestModePath(string strPath,
            out string strError)
        {
            strError = "";

            string strRecID = ResPath.GetRecordId(strPath);
            if (string.IsNullOrEmpty(strRecID) == true
                || strRecID == "?")
                return 0;
            // if (StringUtil.IsPureNumber(strID) == false)


            long id = 0;
            if (long.TryParse(strRecID, out id) == false)
            {
                strError = "检查评估模式记录路径的过程出错，路经 '" + strPath + "' 中的记录ID '" + strRecID + "' 不是数字";
                return -1;
            }

            if (id >= 1 && id <= 1000)
                return 0;
            strError = "评估模式只能使用ID 在 1-1000 范围内的记录 (当前记录路径为 '" + strPath + "')";
            return 1;
        }

        // API: 借书
        // text-level: 用户提示 OPAC续借功能要调用此函数
        // parameters:
        //      strReaderBarcode    读者证条码号。续借的时候可以为空
        //      strItemBarcode  册条码号
        //      strConfirmItemRecPath  册记录路径。在册条码号重复的情况下，才需要使用这个参数，平时为null即可
        //      saBorrowedItemBarcode   同一读者先前已经借阅成功的册条码号集合。用于在返回的读者html中显示出特定的颜色而已。
        //      strStyle    操作风格。"item"表示将返回册记录；"reader"表示将返回读者记录
        //      strItemFormat   规定strItemRecord参数所返回的数据格式
        //      strItemRecord   返回册记录
        //      strReaderFormat 规定strReaderRecord参数所返回的数据格式
        //      strReaderRecord 返回读者记录
        //      aDupPath    如果发生条码号重复，这里返回了相关册记录的路径
        //      return_time 本次借阅要求的最后还书时间。GMT时间。
        // 权限：无论工作人员还是读者，首先应具备borrow或renew权限。
        //      对于读者，还需要他进行的借阅(续借)操作是针对自己的，即strReaderBarcode必须和账户信息中的证条码号一致。
        //      也就是说，读者不允许替他人借阅(续借)图书，这样规定是为了防止读者捣乱。
        public LibraryServerResult Borrow(
            SessionInfo sessioninfo,
            bool bRenew,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            bool bForce,
            string[] saBorrowedItemBarcode,
            string strStyle,
            string strItemFormatList,
            out string[] item_records,
            string strReaderFormatList,
            out string[] reader_records,

            string strBiblioFormatList, // 2008/5/9
            out string[] biblio_records, // 2008/5/9

            out string[] aDupPath,
            out string strOutputReaderBarcodeParam, // 2011/9/25
            out BorrowInfo borrow_info   // 2007/12/6
            )
        {
            item_records = null;
            reader_records = null;
            biblio_records = null;
            aDupPath = null;
            strOutputReaderBarcodeParam = "";
            borrow_info = new BorrowInfo();

            List<string> time_lines = new List<string>();
            DateTime start_time = DateTime.Now;
            string strOperLogUID = "";

            LibraryServerResult result = new LibraryServerResult();

            string strAction = "borrow";
            if (bRenew == true)
                strAction = "renew";

            string strActionName = GetBorrowActionName(strAction);

            // 个人书斋名
            string strPersonalLibrary = "";
            if (sessioninfo.UserType == "reader"
                && sessioninfo.Account != null)
                strPersonalLibrary = sessioninfo.Account.PersonalLibrary;

            // 权限判断
            if (bRenew == false)
            {
                // 权限字符串
                if (StringUtil.IsInList("borrow", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "借阅操作被拒绝。不具备borrow权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    // return result;
                }

                // 对读者身份的附加判断
                // 注：具有个人书斋的，还可以继续向后执行
                if (sessioninfo.UserType == "reader"
                    && sessioninfo.Account != null && strReaderBarcode != sessioninfo.Account.Barcode
                    && string.IsNullOrEmpty(strPersonalLibrary) == true)
                {
                    result.Value = -1;
                    result.ErrorInfo = "借阅操作被拒绝。作为读者不能代他人进行借阅操作。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else
            {
                // 权限字符串
                if (StringUtil.IsInList("renew", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    // text-level: 用户提示
                    result.ErrorInfo = this.GetString("续借操作被拒绝。不具备renew权限。"); // "续借操作被拒绝。不具备renew权限。"
                    result.ErrorCode = ErrorCode.AccessDenied;
                    // return result;
                }

                // 对读者身份的附加判断
                // 注：具有个人书斋的，还可以继续向后执行
                if (sessioninfo.UserType == "reader"
                    && sessioninfo.Account != null && strReaderBarcode != sessioninfo.Account.Barcode
                    && string.IsNullOrEmpty(strPersonalLibrary) == true)
                {
                    result.Value = -1;
                    // text-level: 用户提示
                    result.ErrorInfo = this.GetString("续借操作被拒绝。作为读者不能代他人进行续借操作。");  // "续借操作被拒绝。作为读者不能代他人进行续借操作。"
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            // 如果没有普通的权限，需要预检查存取权限
            LibraryServerResult result_save = null;
            if (result.Value == -1 && String.IsNullOrEmpty(sessioninfo.Access) == false)
            {
                string strAccessActionList = GetDbOperRights(sessioninfo.Access,
                        "", // 此时还不知道实体库名，先取得当前帐户关于任意一个实体库的存取定义
                        "circulation");
                if (string.IsNullOrEmpty(strAccessActionList) == true)
                    return result;

                // 通过了这样一番检查后，后面依然要检查存取权限。
                // 如果后面检查中，精确针对某个实体库的存取权限存在，则依存取权限；如果不存在，则依普通权限
                result_save = result.Clone();
            }
            else if (result.Value == -1)
                return result;  // 延迟报错 2014/9/16

            result = new LibraryServerResult();

            string strError = "";

            if (bForce == true)
            {
                strError = "bForce参数不能为true";
                goto ERROR1;
            }

            int nRet = 0;
            long lRet = 0;
            string strIdcardNumber = "";    // 身份证号
            string strQrCode = "";  //

            if (string.IsNullOrEmpty(strReaderBarcode) == false)
            {
                string strOutputCode = "";
                // 把二维码字符串转换为读者证条码号
                // parameters:
                //      strReaderBcode  [out]读者证条码号
                // return:
                //      -1      出错
                //      0       所给出的字符串不是读者证号二维码
                //      1       成功      
                nRet = DecodeQrCode(strReaderBarcode,
                    out strOutputCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    strQrCode = strReaderBarcode;
                    strReaderBarcode = strOutputCode;
                }
            }


            int nRedoCount = 0; // 因为时间戳冲突, 重试的次数
            string strLockReaderBarcode = strReaderBarcode; // 加锁专用字符串，不怕后面被修改了

            REDO_BORROW:

            bool bReaderLocked = false;

            string strOutputReaderXml = "";
            string strOutputItemXml = "";
            string strBiblioRecID = "";
            string strOutputItemRecPath = "";
            string strOutputReaderRecPath = "";
            string strLibraryCode = "";

            // 加读者记录锁
            // this.ReaderLocks.LockForWrite(strLockReaderBarcode);
            if (String.IsNullOrEmpty(strReaderBarcode) == false)
            {
                // 加读者记录锁
                strLockReaderBarcode = strReaderBarcode;
#if DEBUG_LOCK_READER
                this.WriteErrorLog("Borrow 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif
                this.ReaderLocks.LockForWrite(strReaderBarcode);
                bReaderLocked = true;
                strOutputReaderBarcodeParam = strReaderBarcode;
            }

            try // 读者记录锁定范围开始
            {
                // 读取读者记录
                XmlDocument readerdom = null;
                byte[] reader_timestamp = null;
                string strOldReaderXml = "";

                if (string.IsNullOrEmpty(strReaderBarcode) == false)
                {
                    bool bReaderDbInCirculation = true;
                    LibraryServerResult result1 = GetReaderRecord(
                sessioninfo,
                strActionName,
                time_lines,
                true,
                ref strReaderBarcode,
                ref strIdcardNumber,
                ref strLibraryCode,
                out bReaderDbInCirculation,
                out readerdom,
                out strOutputReaderRecPath,
                out reader_timestamp);
                    if (result1.Value == 0)
                    {
                    }
                    else
                    {
                        return result1;
                    }

                    if (bReaderDbInCirculation == false)
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("借书操作被拒绝。读者证条码号s所在的读者记录s因其数据库s属于未参与流通的读者库"),  // "借书操作被拒绝。读者证条码号 '{0}' 所在的读者记录 '{1}' 因其数据库 '{2}' 属于未参与流通的读者库"
                            strReaderBarcode,
                            strOutputReaderRecPath,
                            StringUtil.GetDbName(strOutputReaderRecPath));

                        // "借书操作被拒绝。读者证条码号 '" + strReaderBarcode + "' 所在的读者记录 '" +strOutputReaderRecPath + "' 因其数据库 '" +strReaderDbName+ "' 属于未参与流通的读者库";
                        goto ERROR1;
                    }

                    // 记忆修改前的读者记录
                    strOldReaderXml = readerdom.OuterXml;

                    if (String.IsNullOrEmpty(strIdcardNumber) == false
                        || string.IsNullOrEmpty(strReaderBarcode) == true /* 2013/5/23 */)
                    {
                        // 获得读者证条码号
                        strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                            "barcode");
                    }
                    strOutputReaderBarcodeParam = DomUtil.GetElementText(readerdom.DocumentElement,
                            "barcode");

                    string strReaderDbName = ResPath.GetDbName(strOutputReaderRecPath);

                    // 检查当前用户管辖的读者范围
                    // return:
                    //      -1  出错
                    //      0   允许继续访问
                    //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
                    nRet = CheckReaderRange(sessioninfo,
                        readerdom,
                        strReaderDbName,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        // strError = "当前用户 '" + sessioninfo.UserID + "' 的存取权限或好友关系禁止操作读者(证条码号为 " + strReaderBarcode + ")。具体原因：" + strError;
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                byte[] item_timestamp = null;
                List<string> aPath = null;
                string strItemXml = "";
                // string strOutputItemRecPath = "";

                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    // text-level: 内部错误
                    strError = "get channel error";
                    goto ERROR1;
                }

                // 加册记录锁
                this.EntityLocks.LockForWrite(strItemBarcode);

                try // 册记录锁定范围开始
                {
                    // 读入册记录
                    DateTime start_time_read_item = DateTime.Now;

                    // 如果已经有确定的册记录路径
                    if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                    {
                        // 检查路径中的库名，是不是实体库名
                        // return:
                        //      -1  error
                        //      0   不是实体库名
                        //      1   是实体库名
                        nRet = this.CheckItemRecPath(strConfirmItemRecPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            strError = strConfirmItemRecPath + strError;
                            goto ERROR1;
                        }

                        string strMetaData = "";

                        lRet = channel.GetRes(strConfirmItemRecPath,
                            out strItemXml,
                            out strMetaData,
                            out item_timestamp,
                            out strOutputItemRecPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // text-level: 内部错误
                            strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得册记录失败: " + strError;
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        // 从册条码号获得册记录

                        // 获得册记录
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        nRet = this.GetItemRecXml(
                            // sessioninfo.Channels,
                            channel,
                            strItemBarcode,
                            "first",    // 在若干实体库中顺次检索，命中一个以上则返回，不再继续检索更多
                            out strItemXml,
                            100,
                            out aPath,
                            out item_timestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            result.Value = -1;
                            // text-level: 用户提示
                            result.ErrorInfo = string.Format(this.GetString("册条码号s不存在"),   // "册条码号 '{0}' 不存在"
                                strItemBarcode);

                            // "册条码号 '" + strItemBarcode + "' 不存在";
                            result.ErrorCode = ErrorCode.ItemBarcodeNotFound;
                            return result;
                        }
                        if (nRet == -1)
                        {
                            // text-level: 内部错误
                            strError = "读入册记录时发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (aPath.Count > 1)
                        {
                            if (this.Statis != null)
                                this.Statis.IncreaseEntryValue(
                                    strLibraryCode,
                                    "出纳",
                                    "借书遇册条码号重复次数",
                                    1);

                            result.Value = -1;
                            // text-level: 用户提示
                            result.ErrorInfo = string.Format(this.GetString("册条码号为s的册记录有s条，无法进行借阅操作"),  // "册条码号为 '{0}' 的册记录有 "{1}" 条，无法进行借阅操作。请在附加册记录路径后重新提交借阅操作。"
                                strItemBarcode,
                                aPath.Count.ToString());
                            this.WriteErrorLog(result.ErrorInfo);   // 2012/12/30

                            // "册条码号为 '" + strItemBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，无法进行借阅操作。请在附加册记录路径后重新提交借阅操作。";
                            result.ErrorCode = ErrorCode.ItemBarcodeDup;

                            aDupPath = new string[aPath.Count];
                            aPath.CopyTo(aDupPath);
                            return result;
                        }
                        else
                        {
                            Debug.Assert(nRet == 1, "");
                            Debug.Assert(aPath.Count == 1, "");

                            if (nRet == 1)
                            {
                                strOutputItemRecPath = aPath[0];
                            }
                        }
                    }

                    string strItemDbName = "";

                    // 看看册记录所从属的数据库，是否在参与流通的实体库之列
                    // 2008/6/4
                    if (String.IsNullOrEmpty(strOutputItemRecPath) == false)
                    {
                        strItemDbName = ResPath.GetDbName(strOutputItemRecPath);
                        bool bItemDbInCirculation = true;
                        if (this.IsItemDbName(strItemDbName, out bItemDbInCirculation) == false)
                        {
                            // text-level: 内部错误
                            strError = "册记录路径 '" + strOutputItemRecPath + "' 中的数据库名 '" + strItemDbName + "' 居然不在定义的实体库之列。";
                            goto ERROR1;
                        }

                        if (bItemDbInCirculation == false)
                        {
                            // text-level: 内部错误
                            strError = "借书操作被拒绝。册条码号 '" + strItemBarcode + "' 所在的册记录 '" + strOutputItemRecPath + "' 因其数据库 '" + strItemDbName + "' 属于未参与流通的实体库";
                            goto ERROR1;
                        }
                    }

                    // 检查存取权限
                    string strAccessParameters = "";

                    {

                        // 检查存取权限 circulation
                        if (String.IsNullOrEmpty(sessioninfo.Access) == false)
                        {
                            string strAccessActionList = "";
                            strAccessActionList = GetDbOperRights(sessioninfo.Access,
                                strItemDbName,
                                "circulation");
#if NO
                            if (String.IsNullOrEmpty(strAccessActionList) == true && result_save != null)
                            {
                                // TODO: 也可以直接返回 result_save
                                strError = "当前用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strItemDbName + "' 执行 出纳 操作的存取权限";
                                result.Value = -1;
                                result.ErrorInfo = strError;
                                result.ErrorCode = ErrorCode.AccessDenied;
                                return result;
                            }
#endif
                            if (strAccessActionList == null)
                            {
                                strAccessActionList = GetDbOperRights(sessioninfo.Access,
            "", // 此时还不知道实体库名，先取得当前帐户关于任意一个实体库的存取定义
            "circulation");
                                if (strAccessActionList == null)
                                {
                                    // 对所有实体库都没有定义任何存取权限，这时候要退而使用普通权限
                                    strAccessActionList = sessioninfo.Rights;

                                    // 注：其实此时 result_save == null 即表明普通权限检查已经通过了的
                                }
                                else
                                {
                                    // 对其他实体库定义了存取权限，但对 strItemDbName 没有定义
                                    strError = "当前用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strItemDbName + "' 执行 出纳 操作的存取权限";
                                    result.Value = -1;
                                    result.ErrorInfo = strError;
                                    result.ErrorCode = ErrorCode.AccessDenied;
                                    return result;
                                }
                            }

                            if (strAccessActionList == "*")
                            {
                                // 通配
                            }
                            else
                            {
                                if (IsInAccessList(strAction, strAccessActionList, out strAccessParameters) == false)
                                {
                                    strError = "当前用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strItemDbName + "' 执行 出纳  " + strActionName + " 操作的存取权限";
                                    result.Value = -1;
                                    result.ErrorInfo = strError;
                                    result.ErrorCode = ErrorCode.AccessDenied;
                                    return result;
                                }
                            }
                        }
                    }

                    XmlDocument itemdom = null;
                    nRet = LibraryApplication.LoadToDom(strItemXml,
                        out itemdom,
                        out strError);
                    if (nRet == -1)
                    {
                        // text-level: 内部错误
                        strError = "装载册记录进入XML DOM时发生错误: " + strError;
                        goto ERROR1;
                    }

                    WriteTimeUsed(
                        time_lines,
                        start_time_read_item,
                        "Borrow() 中读取册记录 耗时 ");

                    DateTime start_time_process = DateTime.Now;

                    // 检查评估模式下书目记录路径
                    if (this.TestMode == true || sessioninfo.TestMode == true)
                    {
                        string strBiblioDbName = "";
                        // 根据实体库名, 找到对应的书目库名
                        // return:
                        //      -1  出错
                        //      0   没有找到
                        //      1   找到
                        nRet = this.GetBiblioDbNameByItemDbName(strItemDbName,
                            out strBiblioDbName,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "根据实体库名 '" + strItemDbName + "' 获得书目库名时出错: " + strError;
                            goto ERROR1;
                        }

                        string strParentID = DomUtil.GetElementText(itemdom.DocumentElement,
    "parent");
                        // 检查评估模式
                        // return:
                        //      -1  检查过程出错
                        //      0   可以通过
                        //      1   不允许通过
                        nRet = CheckTestModePath(strBiblioDbName + "/" + strParentID,
                            out strError);
                        if (nRet != 0)
                        {
                            strError = strActionName + "操作被拒绝: " + strError;
                            goto ERROR1;
                        }
                    }

                    // ***
                    // 延迟获得读者证条码号
                    if (string.IsNullOrEmpty(strReaderBarcode) == true)
                    {
                        if (bRenew == false)
                        {
                            strError = "必须提供 strReaderBarcode 参数值才能进行 " + strActionName + " 操作";
                            goto ERROR1;
                        }

                        string strOutputReaderBarcode = ""; // 返回的借阅者证条码号
                        // 在册记录中获得借阅者证条码号
                        // return:
                        //      -1  出错
                        //      0   该册为未借出状态
                        //      1   成功
                        nRet = GetBorrowerBarcode(itemdom,
                            out strOutputReaderBarcode,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = strError + " (册记录路径为 '" + strOutputItemRecPath + "')";
                            goto ERROR1;
                        }

                        if (nRet == 0 || string.IsNullOrEmpty(strOutputReaderBarcode) == true)
                        {
                            strError = "册 '" + strItemBarcode + "' 当前未曾被任何读者借阅，所以无法进行" + strActionName + "操作";
                            goto ERROR1;
                        }
#if NO
                    // 如果提供了读者证条码号，则需要核实
                    if (String.IsNullOrEmpty(strReaderBarcode) == false)
                    {
                        if (strOutputReaderBarcode != strReaderBarcode)
                        {
                            // 暂时不报错，滞后验证
                            bDelayVerifyReaderBarcode = true;
                            strIdcardNumber = strReaderBarcode;
                        }
                    }
#endif

                        if (String.IsNullOrEmpty(strReaderBarcode) == true)
                            strReaderBarcode = strOutputReaderBarcode;

                        // *** 如果读者记录在前面没有锁定, 在这里锁定
                        if (bReaderLocked == false)
                        {
                            Debug.Assert(string.IsNullOrEmpty(strReaderBarcode) == false, "");
                            // 2015/9/2
                            nRedoCount++;
                            if (nRedoCount > 10)
                            {
                                strError = "Borrow() 为锁定读者，试图重试，但发现已经超过 10 次, 只好放弃...";
                                this.WriteErrorLog(strError);
                                goto ERROR1;
                            }
                            goto REDO_BORROW;
                        }
                    }

                    // ***


                    // 校验读者证条码号参数是否和XML记录中完全一致
#if NO
                    string strTempReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                        "barcode");

                    if (string.IsNullOrEmpty(strReaderBarcode) == false
                        && strReaderBarcode != strTempReaderBarcode)
                    {
                        // text-level: 内部错误
                        strError = "借阅操作被拒绝。因读者证条码号参数 '" + strReaderBarcode + "' 和读者记录中<barcode>元素内的读者证条码号值 '" + strTempReaderBarcode + "' 不一致。";
                        goto ERROR1;
                    }
#endif
                    {
                        // return:
                        //      false   不匹配
                        //      true    匹配
                        bool bRet = CheckBarcode(readerdom,
                strReaderBarcode,
                "读者",
                out strError);
                        if (bRet == false)
                        {
                            strError = "借阅操作被拒绝。因" + strError + "。";
                            goto ERROR1;
                        }
                    }

                    // 2007/1/2
                    // 校验册条码号参数是否和XML记录中完全一致

#if NO
                    string strRefID = "";
                    string strHead = "@refID:";
                    // string strFrom = "册条码";
                    if (StringUtil.HasHead(strItemBarcode, strHead, true) == true)
                    {
                        // strFrom = "参考ID";
                        strRefID = strItemBarcode.Substring(strHead.Length);

                        string strTempRefID = DomUtil.GetElementText(itemdom.DocumentElement,
    "refID");
                        if (strRefID != strTempRefID)
                        {
                            // text-level: 内部错误
                            strError = "借阅操作被拒绝。因册参考ID参数 '" + strRefID + "' 和册记录中<refID>元素内的册条码号值 '" + strTempRefID + "' 不一致。";
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        string strTempItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
    "barcode");
                        if (strItemBarcode != strTempItemBarcode)
                        {
                            // text-level: 内部错误
                            strError = "借阅操作被拒绝。因册条码号参数 '" + strItemBarcode + "' 和册记录中<barcode>元素内的册条码号值 '" + strTempItemBarcode + "' 不一致。";
                            goto ERROR1;
                        }
                    }
#endif
                    {
                        // return:
                        //      false   不匹配
                        //      true    匹配
                        bool bRet = CheckBarcode(itemdom,
                strItemBarcode,
                "册",
                out strError);
                        if (bRet == false)
                        {
                            // text-level: 内部错误
                            strError = "借阅操作被拒绝。因" + strError + "。";
                            goto ERROR1;
                        }
                    }

                    string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                        "readerType");

                    Calendar calendar = null;
                    // return:
                    //      -1  出错
                    //      0   没有找到日历
                    //      1   找到日历
                    nRet = GetReaderCalendar(strReaderType,
                        strLibraryCode,
                        out calendar,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        goto ERROR1;

                    /*
                    string strBookType = DomUtil.GetElementText(itemdom.DocumentElement,
                        "bookType");
                     */

                    bool bReaderDomChanged = false;

                    // 刷新以停代金事项
                    if (StringUtil.IsInList("pauseBorrowing", this.OverdueStyle) == true)
                    {
                        //
                        // 处理以停代金功能
                        // return:
                        //      -1  error
                        //      0   readerdom没有修改
                        //      1   readerdom发生了修改
                        nRet = ProcessPauseBorrowing(
                            strLibraryCode,
                            ref readerdom,
                            strOutputReaderRecPath,
                            sessioninfo.UserID,
                            "refresh",
                            sessioninfo.ClientAddress,  // 前端触发
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 内部错误
                            strError = "在刷新以停代金的过程中发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (nRet == 1)
                            bReaderDomChanged = true;
                    }

                    byte[] output_timestamp = null;
                    string strOutputPath = "";

                    // 检查借阅权限
                    // return:
                    //      -1  配置参数错误
                    //      0   权限不够，借阅操作应当被拒绝
                    //      1   权限够
                    nRet = CheckBorrowRights(
                        sessioninfo.Account,
                        calendar,
                        bRenew,
                        strLibraryCode, // 读者记录所在读者库的馆代码
                        strAccessParameters,
                        ref  readerdom,
                        ref  itemdom,
                        out  strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        // 如果有必要保存回读者记录(因前面刷新了以停代金数据)
                        if (bReaderDomChanged == true)
                        {
                            string strError_1 = "";
                            /*
                            byte[] output_timestamp = null;
                            string strOutputPath = "";
                             * */

                            // 写回读者记录
                            lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                                readerdom.OuterXml,
                                false,
                                "content",  // ,ignorechecktimestamp
                                reader_timestamp,
                                out output_timestamp,
                                out strOutputPath,
                                out strError_1);
                            if (lRet == -1)
                            {
                                // text-level: 内部错误
                                strError = strError + "。然而在写入读者记录过程中，发生错误: " + strError_1;
                                goto ERROR1;
                            }
                        }

                        if (nRet == 0)
                        {
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.AccessDenied;  // 权限不够
                            return result;
                        }

                        goto ERROR1;
                    }

                    XmlDocument domOperLog = new XmlDocument();
                    domOperLog.LoadXml("<root />");
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "libraryCode",
                        strLibraryCode);    // 读者所在的馆代码
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "operation",
                        "borrow");
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "action",
                        bRenew == true ? "renew" : "borrow");
                    // 原来在这里

                    // 借阅API的从属函数
                    // 检查预约相关信息
                    // return:
                    //      -1  error
                    //      0   正常
                    //      1   发现该册被保留)， 不能借阅
                    //      2   发现该册预约， 不能续借
                    //      3   发现该册被保留， 不能借阅。而且本函数修改了册记录(<location>元素发生了变化)，需要本函数返回后，把册记录保存。
                    nRet = DoBorrowReservationCheck(
                        sessioninfo,
                        bRenew,
                        ref readerdom,
                        ref itemdom,
                        bForce,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1 || nRet == 2)
                    {
                        // 被预约保留, 不能借阅
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        if (nRet == 1)
                            result.ErrorCode = ErrorCode.BorrowReservationDenied;
                        if (nRet == 2)
                            result.ErrorCode = ErrorCode.RenewReservationDenied;
                        return result;
                    }

                    if (nRet == 3)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.BorrowReservationDenied;

                        /*
                        byte[] output_timestamp = null;
                        string strOutputPath = "";
                         * */

                        // 写回册记录
                        lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                            itemdom.OuterXml,
                            false,
                            "content",  // ,ignorechecktimestamp
                            item_timestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            // text-level: 内部错误
                            strError = "借阅操作中遇在架预约图书需要写回册记录 " + strOutputItemRecPath + " 时出错: " + strError;
                            this.WriteErrorLog(strError);
                            strError += "。借阅操作被拒绝。";
                            goto ERROR1;
                        }

                        return result;
                    }

                    // 移动到这里

                    // 在读者记录和册记录中添加借阅信息
                    // string strNewReaderXml = "";
                    nRet = DoBorrowReaderAndItemXml(
                        bRenew,
                        strLibraryCode,
                        ref readerdom,
                        ref itemdom,
                        bForce,
                        sessioninfo.UserID,
                        strOutputItemRecPath,
                        strOutputReaderRecPath,
                        ref domOperLog,
                        out borrow_info,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    WriteTimeUsed(
                        time_lines,
                        start_time_process,
                        "Borrow() 中进行各种数据处理 耗时 ");

                    DateTime start_time_write_reader = DateTime.Now;
                    // 原来输出xml或xml的语句在此

                    // 写回读者、册记录

                    // 写回读者记录
                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        readerdom.OuterXml,
                        false,
                        "content",  // ,ignorechecktimestamp
                        reader_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        // 2015/9/2
                        this.WriteErrorLog("Borrow() 写入读者记录 '" + strOutputReaderRecPath + "' 时出错: " + strError);

                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            nRedoCount++;
                            if (nRedoCount > 10)
                            {
                                // text-level: 内部错误
                                strError = "Borrow() 写回读者记录的时候,遇到时间戳冲突,并因此重试10次未能成功，只好放弃...";
                                this.WriteErrorLog(strError);
                                goto ERROR1;
                            }
                            goto REDO_BORROW;
                        }
                        goto ERROR1;
                    }

                    WriteTimeUsed(
                        time_lines,
                        start_time_write_reader,
                        "Borrow() 中写回读者记录 耗时 ");

                    DateTime start_time_write_item = DateTime.Now;

                    // 及时更新时间戳
                    reader_timestamp = output_timestamp;

                    // 写回册记录
                    lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                        itemdom.OuterXml,
                        false,
                        "content",  // ,ignorechecktimestamp
                        item_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        // 2015/9/2
                        this.WriteErrorLog("Borrow() 写入册记录 '" + strOutputItemRecPath + "' 时出错: " + strError);

                        // 要Undo刚才对读者记录的写入
                        string strError1 = "";
                        lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                            strOldReaderXml,
                            false,
                            "content",  // ,ignorechecktimestamp
                            reader_timestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError1);
                        if (lRet == -1) // 初次Undo失败
                        {
                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                // 读者记录Undo的时候, 发现时间戳冲突了
                                // 这时需要读出现存记录, 试图删除新增加的<borrows><borrow>元素
                                // return:
                                //      -1  error
                                //      0   没有必要Undo
                                //      1   Undo成功
                                nRet = UndoBorrowReaderRecord(
                                    channel,
                                    strOutputReaderRecPath,
                                    strReaderBarcode,
                                    strItemBarcode,
                                    out strError);
                                if (nRet == -1)
                                {
                                    // text-level: 内部错误
                                    strError = "Borrow() Undo读者记录 '" + strOutputReaderRecPath + "' (读者证条码号为'" + strReaderBarcode + "') 借阅册条码号 '" + strItemBarcode + "' 的修改时，发生错误，无法Undo: " + strError;
                                    this.WriteErrorLog(strError);
                                    goto ERROR1;
                                }

                                // 成功
                                // 2015/9/2 增加下列防止死循环的语句
                                nRedoCount++;
                                if (nRedoCount > 10)
                                {
                                    strError = "Borrow() Undo 读者记录(1)成功，试图重试 Borrow 时，发现先前重试已经超过 10 次，只好不重试了，做出错返回...";
                                    this.WriteErrorLog(strError);
                                    goto ERROR1;
                                }
                                goto REDO_BORROW;
                            }

                            // 以下为 不是时间戳冲突的其他错误情形
                            // text-level: 内部错误
                            strError = "Borrow() Undo读者记录 '" + strOutputReaderRecPath + "' (读者证条码号为'" + strReaderBarcode + "') 借阅册条码号 '" + strItemBarcode + "' 的修改时，发生错误，无法Undo: " + strError;
                            // strError = strError + ", 并且Undo写回旧读者记录也失败: " + strError1;
                            this.WriteErrorLog(strError);
                            goto ERROR1;
                        } // end of 初次Undo失败

                        // 以下为Undo成功的情形
                        // 2015/9/2 增加下列防止死循环的语句
                        nRedoCount++;
                        if (nRedoCount > 10)
                        {
                            strError = "Borrow() Undo 读者记录(2)成功，试图重试 Borrow 时，发现先前重试已经超过 10 次，只好不重试了，做出错返回...";
                            this.WriteErrorLog(strError);
                            goto ERROR1;
                        }
                        goto REDO_BORROW;

                    } // end of 写回册记录失败

                    WriteTimeUsed(
                        time_lines,
                        start_time_write_item,
                        "Borrow() 中写回册记录 耗时 ");

                    DateTime start_time_write_operlog = DateTime.Now;

                    // 写入日志
                    if (string.IsNullOrEmpty(strConfirmItemRecPath) == false)
                    {
                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "confirmItemRecPath", strConfirmItemRecPath);
                    }

                    if (string.IsNullOrEmpty(strIdcardNumber) == false)
                    {
                        // 表明是使用身份证号来完成借阅操作的
                        DomUtil.SetElementText(domOperLog.DocumentElement,
        "idcardNumber", strIdcardNumber);
                    }

                    // 记载读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerRecord", readerdom.OuterXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);

                    // 记载册记录
                    node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "itemRecord", itemdom.OuterXml);
                    DomUtil.SetAttr(node, "recPath", strOutputItemRecPath);

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        start_time,
                        out strOperLogUID,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "Borrow() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }

                    WriteTimeUsed(
                        time_lines,
                        start_time_write_operlog,
                        "Borrow() 中写操作日志 耗时 ");

                    DateTime start_time_write_statis = DateTime.Now;

                    // 写入统计指标
#if NO
                    if (this.m_strLastReaderBarcode != strReaderBarcode)
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "出纳",
                            "读者数",
                            1);
                        this.m_strLastReaderBarcode = strReaderBarcode;

                    }
#endif
                    if (this.Garden != null)
                        this.Garden.Activate(strReaderBarcode,
                            strLibraryCode);

                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(
                        strLibraryCode,
                        "出纳",
                        "借册",
                        1);

                    WriteTimeUsed(
                        time_lines,
                        start_time_write_statis,
                        "Borrow() 中写统计指标 耗时 ");

                    strOutputItemXml = itemdom.OuterXml;

                    // strOutputReaderXml 将用于构造读者记录返回格式
                    DomUtil.DeleteElement(readerdom.DocumentElement, "password");
                    strOutputReaderXml = readerdom.OuterXml;

                    strBiblioRecID = DomUtil.GetElementText(itemdom.DocumentElement, "parent"); //
                } // 册记录锁定范围结束
                finally
                {
                    // 解册记录锁
                    this.EntityLocks.UnlockForWrite(strItemBarcode);    // strItemBarcode 在整个函数中不允许被修改
                }

            } // 读者记录锁定范围结束
            finally
            {
                // this.ReaderLocks.UnlockForWrite(strLockReaderBarcode);
                if (bReaderLocked == true)
                {
                    this.ReaderLocks.UnlockForWrite(strLockReaderBarcode);
#if DEBUG_LOCK_READER
                    this.WriteErrorLog("Borrow 结束为读者加写锁 '" + strLockReaderBarcode + "'");
#endif

                }
            }

            // 输出数据
            // 把输出数据部分放在读者锁以外范围，是为了尽量减少锁定的时间，提高并发运行效率

            DateTime output_start_time = DateTime.Now;

            if (String.IsNullOrEmpty(strOutputReaderXml) == false
    && StringUtil.IsInList("reader", strStyle) == true)
            {
                DateTime start_time_1 = DateTime.Now;
                nRet = BuildReaderResults(
            sessioninfo,
            null,
            strOutputReaderXml,
            strReaderFormatList,
            strLibraryCode,  // calendar/advancexml/html 时需要
            null,    // recpaths 时需要
            strOutputReaderRecPath,   // recpaths 时需要
            null,    // timestamp 时需要
            OperType.Borrow,
            saBorrowedItemBarcode,
            strItemBarcode,
            ref reader_records,
            out strError);
                if (nRet == -1)
                {
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                        strError);
                    // "虽然出现了下列错误，但是借阅操作已经成功: " + strError;
                    goto ERROR1;
                }

                WriteTimeUsed(
    time_lines,
    start_time_1,
    "Borrow() 中返回读者记录(" + strReaderFormatList + ") 耗时 ");
            }

#if NO
            if (String.IsNullOrEmpty(strOutputReaderXml) == false
                && StringUtil.IsInList("reader", strStyle) == true)
            {
                DateTime start_time_1 = DateTime.Now;

                string[] reader_formats = strReaderFormatList.Split(new char[] { ',' });
                reader_records = new string[reader_formats.Length];

                for (int i = 0; i < reader_formats.Length; i++)
                {
                    string strReaderFormat = reader_formats[i];
                    // 将读者记录数据从XML格式转换为HTML格式
                    // if (String.Compare(strReaderFormat, "html", true) == 0)
                    if (IsResultType(strReaderFormat, "html") == true)
                    {
                        string strReaderRecord = "";
                        nRet = this.ConvertReaderXmlToHtml(
                            sessioninfo,
                            this.CfgDir + "\\readerxml2html.cs",
                            this.CfgDir + "\\readerxml2html.cs.ref",
                            strLibraryCode,
                            strOutputReaderXml,
                            strOutputReaderRecPath, // 2009/10/18
                            OperType.Borrow,
                            saBorrowedItemBarcode,
                            strItemBarcode,
                            strReaderFormat,
                            out strReaderRecord,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 用户提示
                            strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                strError);
                            // "虽然出现了下列错误，但是借阅操作已经成功: " + strError;
                            goto ERROR1;
                        }
                        reader_records[i] = strReaderRecord;

                    }
                    // 将读者记录数据从XML格式转换为text格式
                    // else if (String.Compare(strReaderFormat, "text", true) == 0)
                    else if (IsResultType(strReaderFormat, "text") == true)
                    {
                        string strReaderRecord = "";
                        nRet = this.ConvertReaderXmlToHtml(
                            sessioninfo,
                            this.CfgDir + "\\readerxml2text.cs",
                            this.CfgDir + "\\readerxml2text.cs.ref",
                            strLibraryCode,
                            strOutputReaderXml,
                            strOutputReaderRecPath, // 2009/10/18
                            OperType.Borrow,
                            saBorrowedItemBarcode,
                            strItemBarcode,
                            strReaderFormat,
                            out strReaderRecord,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 用户提示
                            strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                strError);
                            goto ERROR1;
                        }
                        reader_records[i] = strReaderRecord;
                    }
                    // else if (String.Compare(strReaderFormat, "xml", true) == 0)
                    else if (IsResultType(strReaderFormat, "xml") == true)
                    {
                        // reader_records[i] = strOutputReaderXml;
                        string strResultXml = "";
                        nRet = GetItemXml(strOutputReaderXml,
            strReaderFormat,
            out strResultXml,
            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 用户提示
                            strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                strError);
                            goto ERROR1;
                        }
                        reader_records[i] = strResultXml;
                    }
                    else if (IsResultType(strReaderFormat, "summary") == true)
                    {
                        // 2013/12/15
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strOutputReaderXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "读者 XML 装入 DOM 出错: " + ex.Message;
                            // text-level: 用户提示
                            strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                strError);
                            goto ERROR1;
                        }
                        reader_records[i] = DomUtil.GetElementText(dom.DocumentElement, "name");
                    }
                    else
                    {
                        strError = "strReaderFormatList参数出现了不支持的数据格式类型 '" + strReaderFormat + "'";
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                            strError);
                        goto ERROR1;
                    }
                } // end of for

                WriteTimeUsed(
                    time_lines,
                    start_time_1,
                    "Borrow() 中返回读者记录(" + strReaderFormatList + ") 耗时 ");
            }

#endif

            if (String.IsNullOrEmpty(strOutputItemXml) == false
                && StringUtil.IsInList("item", strStyle) == true)
            {
                DateTime start_time_1 = DateTime.Now;

                string[] item_formats = strItemFormatList.Split(new char[] { ',' });
                item_records = new string[item_formats.Length];

                for (int i = 0; i < item_formats.Length; i++)
                {
                    string strItemFormat = item_formats[i];

                    // 将册记录数据从XML格式转换为HTML格式
                    //if (String.Compare(strItemFormat, "html", true) == 0)
                    if (IsResultType(strItemFormat, "html") == true)
                    {
                        string strItemRecord = "";
                        nRet = this.ConvertItemXmlToHtml(
                            this.CfgDir + "\\itemxml2html.cs",
                            this.CfgDir + "\\itemxml2html.cs.ref",
                            strOutputItemXml,
                            strOutputItemRecPath,   // 2009/10/18
                            out strItemRecord,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 用户提示
                            strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                strError);
                            goto ERROR1;
                        }
                        item_records[i] = strItemRecord;
                    }
                    // 将册记录数据从XML格式转换为text格式
                    // else if (String.Compare(strItemFormat, "text", true) == 0)
                    else if (IsResultType(strItemFormat, "text") == true)
                    {
                        string strItemRecord = "";
                        nRet = this.ConvertItemXmlToHtml(
                            this.CfgDir + "\\itemxml2text.cs",
                            this.CfgDir + "\\itemxml2text.cs.ref",
                            strOutputItemXml,
                            strOutputItemRecPath,   // 2009/10/18
                            out strItemRecord,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 用户提示
                            strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                strError);
                            goto ERROR1;
                        }
                        item_records[i] = strItemRecord;
                    }
                    // else if (String.Compare(strItemFormat, "xml", true) == 0)
                    else if (IsResultType(strItemFormat, "xml") == true)
                    {
                        string strResultXml = "";
                        nRet = GetItemXml(strOutputItemXml,
            strItemFormat,
            out strResultXml,
            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 用户提示
                            strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                strError);
                            goto ERROR1;
                        }
                        item_records[i] = strResultXml;
                    }
                    else
                    {
                        strError = "strItemFormatList参数出现了不支持的数据格式类型 '" + strItemFormat + "'";
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                            strError);
                        goto ERROR1;
                    }
                } // end of for

                WriteTimeUsed(
    time_lines,
    start_time_1,
    "Borrow() 中返回册记录(" + strItemFormatList + ") 耗时 ");
            }

            // 2008/5/9
            if (StringUtil.IsInList("biblio", strStyle) == true)
            {
                DateTime start_time_1 = DateTime.Now;

                if (String.IsNullOrEmpty(strBiblioRecID) == true)
                {
                    strError = "册记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录ID";
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                        strError);
                    goto ERROR1;
                }

                string strItemDbName = ResPath.GetDbName(strOutputItemRecPath);

                string strBiblioDbName = "";
                // 根据实体库名, 找到对应的书目库名
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = this.GetBiblioDbNameByItemDbName(strItemDbName,
                    out strBiblioDbName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "实体库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                        strError);
                    goto ERROR1;
                }

                string strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;

                string[] biblio_formats = strBiblioFormatList.Split(new char[] { ',' });
                biblio_records = new string[biblio_formats.Length];

                string strBiblioXml = "";
                // 至少有html xml text之一，才获取strBiblioXml
                if (StringUtil.IsInList("html", strBiblioFormatList) == true
                    || StringUtil.IsInList("xml", strBiblioFormatList) == true
                    || StringUtil.IsInList("text", strBiblioFormatList) == true)
                {
                    RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                            strError);
                        goto ERROR1;
                    }

                    string strMetaData = "";
                    byte[] timestamp = null;
                    string strTempOutputPath = "";
                    lRet = channel.GetRes(strBiblioRecPath,
                        out strBiblioXml,
                        out strMetaData,
                        out timestamp,
                        out strTempOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获得种记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                            strError);
                        goto ERROR1;
                    }
                }

                for (int i = 0; i < biblio_formats.Length; i++)
                {
                    string strBiblioFormat = biblio_formats[i];

                    // 需要从内核映射过来文件
                    string strLocalPath = "";
                    string strBiblio = "";

                    // 将书目记录数据从XML格式转换为HTML格式
                    if (String.Compare(strBiblioFormat, "html", true) == 0)
                    {
                        // TODO: 可以cache
                        nRet = this.MapKernelScriptFile(
                            sessioninfo,
                            strBiblioDbName,
                            "./cfgs/loan_biblio.fltx",
                            out strLocalPath,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 用户提示
                            strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                strError);
                            goto ERROR1;
                        }

                        // 将种记录数据从XML格式转换为HTML格式
                        string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                        if (string.IsNullOrEmpty(strBiblioXml) == false)
                        {
                            nRet = this.ConvertBiblioXmlToHtml(
                                strFilterFileName,
                                strBiblioXml,
                                    null,
                                strBiblioRecPath,
                                out strBiblio,
                                out strError);
                            if (nRet == -1)
                            {
                                // text-level: 用户提示
                                strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                    strError);
                                goto ERROR1;
                            }
                        }
                        else
                            strBiblio = "";

                        biblio_records[i] = strBiblio;
                    }
                    // 将册记录数据从XML格式转换为text格式
                    else if (String.Compare(strBiblioFormat, "text", true) == 0)
                    {
                        // TODO: 可以cache
                        nRet = this.MapKernelScriptFile(
                            sessioninfo,
                            strBiblioDbName,
                            "./cfgs/loan_biblio_text.fltx",
                            out strLocalPath,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 用户提示
                            strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                strError);
                            goto ERROR1;
                        }
                        // 将种记录数据从XML格式转换为TEXT格式
                        string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                        if (string.IsNullOrEmpty(strBiblioXml) == false)
                        {
                            nRet = this.ConvertBiblioXmlToHtml(
                                strFilterFileName,
                                strBiblioXml,
                                    null,
                                strBiblioRecPath,
                                out strBiblio,
                                out strError);
                            if (nRet == -1)
                            {
                                // text-level: 用户提示
                                strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                                    strError);
                                goto ERROR1;
                            }

                        }
                        else
                            strBiblio = "";

                        biblio_records[i] = strBiblio;
                    }
                    else if (String.Compare(strBiblioFormat, "xml", true) == 0)
                    {
                        biblio_records[i] = strBiblioXml;
                    }
                    else if (String.Compare(strBiblioFormat, "recpath", true) == 0)
                    {
                        biblio_records[i] = strBiblioRecPath;
                    }
                    else if (string.IsNullOrEmpty(strBiblioFormat) == true)
                    {
                        biblio_records[i] = "";
                    }
                    else
                    {
                        strError = "strBiblioFormatList参数出现了不支持的数据格式类型 '" + strBiblioFormat + "'";
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("虽然出现了下列错误，但是借阅操作已经成功s"),   // "虽然出现了下列错误，但是借阅操作已经成功: {0}";
                            strError);
                        goto ERROR1;
                    }
                } // end of for

                WriteTimeUsed(
time_lines,
start_time_1,
"Borrow() 中返回书目记录(" + strBiblioFormatList + ") 耗时 ");

            }

            WriteTimeUsed(
    time_lines,
    start_time,
    "Borrow() 耗时 ");
            // 如果整个时间超过一秒，则需要计入操作日志
            if (DateTime.Now - start_time > new TimeSpan(0, 0, 1))
            {
                WriteLongTimeOperLog(
                    sessioninfo,
                    strAction,
                    start_time,
                    "整个操作耗时超过 1 秒。详情:" + StringUtil.MakePathList(time_lines, ";"),
                    strOperLogUID,
                    out strError);
            }

            // 如果创建输出数据的时间超过一秒，则需要计入操作日志
            if (DateTime.Now - output_start_time > new TimeSpan(0, 0, 1))
            {
                WriteLongTimeOperLog(
                    sessioninfo,
                    strAction,
                    output_start_time,
                    "output 阶段耗时超过 1 秒",
                    strOperLogUID,
                    out strError);
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 写入长时操作日志
        int WriteLongTimeOperLog(
            SessionInfo sessioninfo,
            string strAction,
            DateTime start_time,
            string strComment,
            string strLinkUID,
            out string strError)
        {
            strError = "";

            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "operation",
                "memo");
            DomUtil.SetElementText(domOperLog.DocumentElement, "action", strAction);
            DomUtil.SetElementText(domOperLog.DocumentElement,
    "linkUID",
    strLinkUID);
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "comment",
                strComment);
            string strOutputUID = "";
            int nRet = this.OperLog.WriteOperLog(domOperLog,
    sessioninfo.ClientAddress,
    start_time,
    out strOutputUID,
    out strError);
            if (nRet == -1)
                return -1;
            return 0;
        }

        LibraryServerResult GetReaderRecord(
            SessionInfo sessioninfo,
            string strActionName,
            List<string> time_lines,
            bool bVerifyReaderRecPath,
            ref string strReaderBarcode,    // 2015/1/4 加上 ref
            ref string strIdcardNumber,
            ref string strLibraryCode,
            out bool bReaderDbInCirculation,
            out XmlDocument readerdom,
            out string strOutputReaderRecPath,
            out byte[] reader_timestamp)
        {
            string strError = "";
            int nRet = 0;
            bReaderDbInCirculation = true;

            LibraryServerResult result = new LibraryServerResult();

            strOutputReaderRecPath = "";
            readerdom = null;
            reader_timestamp = null;

            DateTime start_time_read_reader = DateTime.Now;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 读入读者记录
            string strReaderXml = "";
            nRet = this.TryGetReaderRecXml(
                // sessioninfo.Channels,
                channel,
                strReaderBarcode,
                sessioninfo.LibraryCodeList,    // TODO: 对个人书斋情况要测试一下
                out strReaderXml,
                out strOutputReaderRecPath,
                out reader_timestamp,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "读入读者记录时发生错误: " + strError;
                goto ERROR1;
            }

            if (nRet == 0)
            {
                // 如果是身份证号，则试探检索“身份证号”途径
                if (StringUtil.IsIdcardNumber(strReaderBarcode) == true)
                {
                    strIdcardNumber = strReaderBarcode;
                    strReaderBarcode = ""; // 迫使函数返回后，重新获得 reader barcode

                    // 通过特定检索途径获得读者记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetReaderRecXmlByFrom(
                        // sessioninfo.Channels,
                        channel,
                        strIdcardNumber,
                        "身份证号",
                        out strReaderXml,
                        out strOutputReaderRecPath,
                        out reader_timestamp,
                        out strError);
                    if (nRet == -1)
                    {
                        // text-level: 内部错误
                        strError = "用身份证号 '" + strIdcardNumber + "' 读入读者记录时发生错误: " + strError;
                        goto ERROR1;
                    }
                    if (nRet == 0)
                    {
                        result.Value = -1;
                        // text-level: 用户提示
                        result.ErrorInfo = string.Format(this.GetString("身份证号s不存在"),   // "身份证号 '{0}' 不存在"
                            strIdcardNumber);
                        result.ErrorCode = ErrorCode.IdcardNumberNotFound;
                        return result;
                    }
                    if (nRet > 1)
                    {
                        // text-level: 用户提示
                        result.Value = -1;
                        result.ErrorInfo = "用身份证号 '" + strIdcardNumber + "' 检索读者记录命中 " + nRet.ToString() + " 条，因此无法用身份证号来进行借还操作。请改用证条码号来进行借还操作。";
                        result.ErrorCode = ErrorCode.IdcardNumberDup;
                        return result;
                    }
                    Debug.Assert(nRet == 1, "");
                    goto SKIP0;
                }
                else
                {
                    // 2013/5/24
                    // 如果需要，从读者证号等辅助途径进行检索
                    foreach (string strFrom in this.PatronAdditionalFroms)
                    {
                        nRet = this.GetReaderRecXmlByFrom(
                            // sessioninfo.Channels,
                            channel,
                            null,
                            strReaderBarcode,
                            strFrom,
                            out strReaderXml,
                            out strOutputReaderRecPath,
                            out reader_timestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: 内部错误
                            strError = "用" + strFrom + " '" + strReaderBarcode + "' 读入读者记录时发生错误: " + strError;
                            goto ERROR1;
                        }
                        if (nRet == 0)
                            continue;
                        if (nRet > 1)
                        {
                            // text-level: 用户提示
                            result.Value = -1;
                            result.ErrorInfo = "用" + strFrom + " '" + strReaderBarcode + "' 检索读者记录命中 " + nRet.ToString() + " 条，因此无法用" + strFrom + "来进行借还操作。请改用证条码号来进行借还操作。";
                            result.ErrorCode = ErrorCode.IdcardNumberDup;
                            return result;
                        }

                        strReaderBarcode = "";

#if NO
                            result.ErrorInfo = strError;
                            result.Value = nRet;
#endif
                        goto SKIP0;
                    }
                }

                result.Value = -1;
                // text-level: 用户提示
                result.ErrorInfo = string.Format(this.GetString("读者证条码号s不存在"),   // "读者证条码号 '{0}' 不存在"
                    strReaderBarcode);
                // "读者证条码号 '" + strReaderBarcode + "' 不存在";
                result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                return result;
            }

            // 2008/6/17
            if (nRet > 1)
            {
                // text-level: 内部错误
                strError = "读入读者记录时，发现读者证条码号 '" + strReaderBarcode + "' 命中 " + nRet.ToString() + " 条，这是一个严重错误，请系统管理员尽快处理。";
                goto ERROR1;
            }

        SKIP0:

            // 看看读者记录所从属的数据库，是否在参与流通的读者库之列
            // 2008/6/4
            if (bVerifyReaderRecPath == true
                && String.IsNullOrEmpty(strOutputReaderRecPath) == false)
            {
                if (this.TestMode == true || sessioninfo.TestMode == true)
                {
                    // 检查评估模式
                    // return:
                    //      -1  检查过程出错
                    //      0   可以通过
                    //      1   不允许通过
                    nRet = CheckTestModePath(strOutputReaderRecPath,
                        out strError);
                    if (nRet != 0)
                    {
                        strError = strActionName + "操作被拒绝: " + strError;
                        goto ERROR1;
                    }
                }

                string strReaderDbName = ResPath.GetDbName(strOutputReaderRecPath);
                // bool bReaderDbInCirculation = true;
                if (this.IsReaderDbName(strReaderDbName,
                    out bReaderDbInCirculation,
                    out strLibraryCode) == false)
                {
                    // text-level: 内部错误
                    strError = "读者记录路径 '" + strOutputReaderRecPath + "' 中的数据库名 '" + strReaderDbName + "' 居然不在定义的读者库之列。";
                    goto ERROR1;
                }

#if NO
                if (bReaderDbInCirculation == false)
                {
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("借书操作被拒绝。读者证条码号s所在的读者记录s因其数据库s属于未参与流通的读者库"),  // "借书操作被拒绝。读者证条码号 '{0}' 所在的读者记录 '{1}' 因其数据库 '{2}' 属于未参与流通的读者库"
                        strReaderBarcode,
                        strOutputReaderRecPath,
                        strReaderDbName);

                    // "借书操作被拒绝。读者证条码号 '" + strReaderBarcode + "' 所在的读者记录 '" +strOutputReaderRecPath + "' 因其数据库 '" +strReaderDbName+ "' 属于未参与流通的读者库";
                    goto ERROR1;
                }
#endif

                // 检查当前操作者是否管辖这个读者库
                // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
        sessioninfo.LibraryCodeList) == false)
                {
                    strError = "读者记录路径 '" + strOutputReaderRecPath + "' 的读者库不在当前用户管辖范围内";
                    goto ERROR1;
                }
            }

            nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "装载读者记录进入 XML DOM 时发生错误: " + strError;
                goto ERROR1;
            }

            WriteTimeUsed(
                time_lines,
                start_time_read_reader,
                strActionName + " 中读取读者记录 耗时 ");
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 读者记录中 允许用于读者范围过滤的元素名列表
        static string[] reader_content_fields = new string[] {
                "barcode",
                "state",
                "readerType",
                "createDate",
                "expireDate",
                "name",
                "namePinyin",
                "gender",
                "birthday",
                "dateOfBirth",
                "idCardNumber",
                "department",
                "post",
                "address",
                "tel",
                "email",
                "comment",
                "cardNumber",
                "displayName",  // 显示名
                "nation",
                "rights",
                "personalLibrary",
                "friends",
            };

        // 检查当前用户管辖的读者范围
        // parameters:
        //      
        // return:
        //      -1  出错
        //      0   允许继续访问
        //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
        static int CheckReaderRange(
            SessionInfo sessioninfo,
            XmlDocument reader_dom,
            string strReaderDbName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strAccessString = sessioninfo.Access;
            string strReaderBarcode = DomUtil.GetElementText(reader_dom.DocumentElement, "barcode");

            if (String.IsNullOrEmpty(strAccessString) == false)
            {
                // return:
                //      -1  出错
                //      0   允许继续访问
                //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
                //      2   没有定义相关的存取定义参数
                nRet = AccessReaderRange(
                    strAccessString,
                    reader_dom,
                    strReaderDbName,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                {
                    strError = "当前用户 '" + sessioninfo.UserID + "' 的存取权限禁止操作读者(证条码号为 " + strReaderBarcode + ")。具体原因：" + strError;
                    return 1;
                }
                if (nRet == 0)
                    return 0;
            }

            if (sessioninfo.UserType == "reader")
            {
                // 看看 借书者 是否就是操作者自己?
                if (sessioninfo.UserID == strReaderBarcode)
                    return 0;

                // 没有匹配的 reader 范畴，那么就看 reader_dom 中的 fiends 元素
                string strFields = DomUtil.GetElementText(reader_dom.DocumentElement, "friends");
                if (string.IsNullOrEmpty(strFields) == false)
                {
                    // 判断当前用户是否为 reader_dom 读者的 friends
                    if (StringUtil.IsInList(sessioninfo.Account.Barcode, strFields) == true)
                        return 0;
                    strError = "'" + sessioninfo.Account.Barcode + "' 不在 '" + strReaderBarcode + "' 的好友列表中";
                    strError = "借阅者 (证条码号为 " + strReaderBarcode + ") 的好友关系禁止当前用户 '" + sessioninfo.UserID + "' 进行操作)。具体原因：" + strError;
                    return 1;
                }
                else
                {
                    // 没有定义任何好友关系
                    strError = "'" + strReaderBarcode + "' 尚未定义好友列表";
                    strError = "借阅者 (证条码号为 " + strReaderBarcode + ") 的好友关系禁止当前用户 '" + sessioninfo.UserID + "' 进行操作)。具体原因：" + strError;
                    return 1;
                }
            }
            else
                return 0;
        }

        // 检查存取权限中的 location
        // parameters:
        //      
        // return:
        //      -1  出错
        //      0   允许继续访问
        //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
        //      2   没有定义相关的存取定义参数
        static int AccessLocationRange(
            string strAccessString,
            string strItemLocation,
            string strReaderDbName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strAccessString) == true)
                return 2;

            string strAccessActionList = "";
            strAccessActionList = GetDbOperRights(strAccessString,
                strReaderDbName,
                "location");
            if (strAccessActionList == "*")
            {
                // 通配
                return 0;
            }

            if (strAccessActionList == null)
                return 2;

            string strRoom = "";
            string strCode = "";

            // 解析
            ParseCalendarName(strItemLocation,
            out strCode,
            out strRoom);

            if (StringUtil.IsInList(strRoom, strAccessActionList) == true)
                return 0;

            strError = "当前用户只能操作馆藏地为 '" + strAccessActionList + "' 之一的册，不能操作(分馆 '" + strCode + "' 内)馆藏地为 '" + strRoom + "' 的册";
            return 1;
        }

        // 检查存取权限中的 reader
        // parameters:
        //      
        // return:
        //      -1  出错
        //      0   允许继续访问
        //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
        //      2   没有定义相关的存取定义参数
        static int AccessReaderRange(
            string strAccessString,
            XmlDocument reader_dom,
            string strReaderDbName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strAccessString) == true)
                return 2;

            string strAccessActionList = "";
            strAccessActionList = GetDbOperRights(strAccessString,
                strReaderDbName,
                "reader");
            if (strAccessActionList == "*")
            {
                // 通配
                return 0;
            }

            if (strAccessActionList == null)
                return 2;

            foreach (string name in reader_content_fields)
            {
                string strAccessParameters = "";
                if (IsInAccessList(name, strAccessActionList, out strAccessParameters) == false)
                    continue;

                // 匹配一个读者字段
                // parameters:
                //      strName     字段名
                //      strMatchCase  字段内容匹配模式 @引导的是正则表达式，否则是普通星号模糊匹配方式
                // return:
                //      -1  出错
                //      0   没有匹配上
                //      1   匹配上了
                nRet = MatchReaderField(reader_dom,
                    name,
                    strAccessParameters,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    return 1;
            }

            return 0;
        }

        // 匹配一个读者字段
        // parameters:
        //      strName     字段名
        //      strMatchCase  字段内容匹配模式 @引导的是正则表达式，否则是普通星号模糊匹配方式
        // return:
        //      -1  出错
        //      0   没有匹配上。strError 中有说明原因的文字
        //      1   匹配上了
        static int MatchReaderField(XmlDocument reader_dom,
            string strName,
            string strMatchCase,
            out string strError)
        {
            strError = "";

            string strValue = DomUtil.GetElementText(reader_dom.DocumentElement, strName);

            string strPattern = "";

            // Regular expression
            if (strMatchCase.Length >= 1
                && strMatchCase[0] == '@')
            {
                strPattern = strMatchCase.Substring(1);
            }
            else
                strPattern = WildcardToRegex(strMatchCase);

            if (StringUtil.RegexCompare(strPattern,
                    RegexOptions.None,
                    strValue) == true)
                return 1;

            strError = "字段 " + strName + " 内容 '" + strValue + "' 无法匹配 '" + strMatchCase + "'";
            return 0;
        }

        static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".")
            + "$";
        }

        int GetItemXml(string strItemXml,
            string strFormat,
            out string strResultXml,
            out string strError)
        {
            strError = "";
            strResultXml = "";
            // int nRet = 0;

            if (string.IsNullOrEmpty(strItemXml) == true)
                return 0;

            string strSubType = "";
            string strType = "";
            StringUtil.ParseTwoPart(strFormat,
                ":",
                out strType,
                out strSubType);

            if (string.IsNullOrEmpty(strSubType) == true)
            {
                strResultXml = strItemXml;
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            if (dom.DocumentElement == null)
            {
                strResultXml = strItemXml;
                return 0;
            }

            // 去掉 <borrowHistory> 的下级元素
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrowHistory/*");
            foreach (XmlNode node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            strResultXml = dom.OuterXml;
            return 0;
        }

        // 在调试文件中写入耗费时间信息
        void WriteTimeUsed(
            List<string> lines,
            DateTime start_time,
            string strPrefix)
        {
#if NO
            if (this.DebugMode == false)
                return;
            TimeSpan delta = DateTime.Now - start_time;
            string strTiming = strPrefix + " " + delta.ToString();
            this.WriteDebugInfo(strTiming);
#endif
            TimeSpan delta = DateTime.Now - start_time;
            lines.Add(strPrefix + " " + delta.TotalSeconds.ToString("F3"));
        }

        #region Borrow()下级函数

        // 借阅API的从属函数
        // 检查借阅权限
        // text-level: 用户提示 OPAC的续借要调用Borrow()函数，进而调用本函数
        // parameters:
        //      strLibraryCode  读者记录所在读者库的馆代码
        //      strAccessParameters 许可操作的馆藏地点列表。如果为 空 或者 "*"，表示全部许可
        // return:
        //      -1  配置参数错误
        //      0   权限不够，借阅操作应当被拒绝
        //      1   权限够
        int CheckBorrowRights(
            Account account,
            Calendar calendar,
            bool bRenew,
            string strLibraryCode,
            string strAccessParameters,
            ref XmlDocument readerdom,
            ref XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            LibraryApplication app = this;

            if (StringUtil.IsInList("pauseBorrowing", this.OverdueStyle) == true)
            {
                /* 这一段已经移动到本函数外面去做了，因为涉及到对readerdom的修改问题
                //
                // 处理以停代金功能
                // return:
                //      -1  error
                //      0   readerdom没有修改
                //      1   readerdom发生了修改
                nRet = ProcessPauseBorrowing(ref readerdom,
                    "refresh",
                    out strError);
                if (nRet == -1)
                {
                    strError = "在刷新以停代金的过程中发生错误: " + strError;
                    return -1;
                }
                 * */

                // 是否存在以停代金事项？
                string strMessage = "";
                nRet = HasPauseBorrowing(
                    calendar,
                    strLibraryCode,
                    readerdom,
                    out strMessage,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "在计算以停代金的过程中发生错误: " + strError;
                    return -1;
                }
                if (nRet == 1)
                {
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("借阅操作被拒绝，因该读者s"),   // "借阅操作被拒绝，因该读者{0}"
                        strMessage);

                    // "借阅操作被拒绝，因该读者" + strMessage;
                    return 0;
                }
            }

            string strOperName = this.GetString("借阅");

            if (bRenew == true)
                strOperName = this.GetString("续借");

            string strRefID = DomUtil.GetElementText(itemdom.DocumentElement,
"refID");
            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");
            string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");

            string strItemBarcodeParam = strItemBarcode;
            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
#if NO
                // text-level: 内部错误
                strError = "册记录中册条码号不能为空";
                return -1;
#endif
                // 如果册条码号为空，则使用 参考ID
                if (String.IsNullOrEmpty(strRefID) == true)
                {
                    // text-level: 内部错误
                    strError = "册记录中册条码号和参考ID不应同时为空";
                    return -1;
                }
                strItemBarcodeParam = "@refID:" + strRefID;
            }

            // 馆藏地点
            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement, "location");

            // 去掉#reservation部分
            // StringUtil.RemoveFromInList("#reservation", true, ref strLocation);
            strLocation = StringUtil.GetPureLocationString(strLocation);

            // 检查册所属的馆藏地点是否合读者所在的馆藏地点吻合
            string strRoom = "";
            string strCode = "";
            {

                // 解析
                ParseCalendarName(strLocation,
            out strCode,
            out strRoom);
                if (strCode != strLibraryCode)
                {
                    strError = "借阅操作被拒绝。因册记录的馆藏地 '" + strLocation + "' 不属于读者所在馆代码 '" + strLibraryCode + "' ";
                    return 0;
                }
            }

            // 检查馆藏地列表
            if (string.IsNullOrEmpty(strAccessParameters) == false && strAccessParameters != "*")
            {
                bool bFound = false;
                List<string> locations = StringUtil.SplitList(strAccessParameters);
                foreach (string s in locations)
                {
                    string c = "";
                    string r = "";
                    ParseCalendarName(s,
                        out c,
                        out r);
                    if (/*string.IsNullOrEmpty(c) == false && */ c != "*")
                    {
                        if (c != strCode)
                            continue;
                    }

                    if (/*string.IsNullOrEmpty(r) == false && */ r != "*")
                    {
                        if (r != strRoom)
                            continue;
                    }

                    bFound = true;
                    break;
                }

                if (bFound == false)
                {
                    strError = "借阅操作被拒绝。因册记录的馆藏地 '" + strLocation + "' 不在当前用户存取定义规定的馆藏地许可范围 '" + strAccessParameters + "' 之内";
                    return 0;
                }
            }

            // 2006/12/29
            // 检查册是否能够被借出
            bool bResultValue = false;
            string strMessageText = "";

            // 执行脚本函数ItemCanBorrow
            // parameters:
            // return:
            //      -2  not found script
            //      -1  出错
            //      0   成功
            nRet = app.DoItemCanBorrowScriptFunction(
                bRenew,
                account,
                itemdom,
                out bResultValue,
                out strMessageText,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "执行CanBorrow()脚本函数时出错: " + strError;
                return -1;
            }
            if (nRet == -2)
            {
                // 如果没有配置脚本函数，就根据馆藏地点察看地点允许配置来决定是否允许借阅
                List<string> locations = app.GetLocationTypes(strLibraryCode, true);
                if (locations.IndexOf(strRoom) == -1)
                {
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("册s的馆藏地点为s，按规定此册不允许外借"),  // "册 {0} 的馆藏地点为 {1}，按规定(<locationTypes>配置)此册不允许外借。"
                        strItemBarcodeParam,
                        strLocation);

                    // "册 " + strItemBarcode + " 的馆藏地点为 " + strLocation + "，按规定(<locationTypes>配置)此册不允许外借。";
                    return 0;
                }
            }
            else
            {
                // 根据脚本返回结果
                if (bResultValue == false)
                {
                    strError = string.Format(this.GetString("不允许s。因为册s的状态为s"),   // "不允许 {0}。因为册 {1} 的状态为 {2}"
                        strOperName,
                        strItemBarcodeParam,
                        strMessageText);
                    /*
                    strError = "不允许" 
                        + (bRenew == true ? "续借" : "外借")
                        + "。因为册 " + strItemBarcode + " 的状态为 "+strMessageText;
                     * */
                    return 0;
                }
            }

            // 
            // 个人书斋的检查
            string strPersonalLibrary = "";
            if (account != null)
                strPersonalLibrary = account.PersonalLibrary;

            if (string.IsNullOrEmpty(strPersonalLibrary) == false)
            {
                if (strPersonalLibrary != "*" && StringUtil.IsInList(strRoom, strPersonalLibrary) == false)
                {
                    strError = "当前用户 '" + account.Barcode + "' 只能操作馆代码 '" + strLibraryCode + "' 中地点为 '" + strPersonalLibrary + "' 的图书，不能操作地点为 '" + strRoom + "' 的图书";
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("s操作被拒绝，原因s"),  // "{0} 操作被拒绝，原因: {1}"
                        strOperName,
                        strError);
                    return 0;
                }
            }

            if (bRenew == false)
            {
                // 检查读者记录中是否已经有了对应册的<borrow>
                XmlNode node = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcodeParam + "']");
                if (node != null)
                {
                    if (string.IsNullOrEmpty(strItemBarcode) == false)
                        node = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']");
                    if (node != null)
                    {
                        // text-level: 用户提示

                        // string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");
                        strError = "借阅操作被拒绝。读者 '" + strReaderBarcode + "' 早先已经借阅了册 '" + strItemBarcodeParam + "' 。(读者记录中已存在对应的<borrow>元素)";
                        // strError = "操作前在读者记录中发现居然已存在表明读者借阅了册'"+strItemBarcode+"'的字段信息 " + node.OuterXml;
                        return -1;
                    }
                }
            }

            // 检查借阅证是否超期，是否有挂失等状态
            // return:
            //      -1  检测过程发生了错误。应当作不能借阅来处理
            //      0   可以借阅
            //      1   证已经过了失效期，不能借阅
            //      2   证有不让借阅的状态
            nRet = CheckReaderExpireAndState(readerdom,
                out strError);
            if (nRet != 0)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("s操作被拒绝，原因s"),  // "{0} 操作被拒绝，原因: {1}"
                    strOperName,
                    strError);
                // strOperName + "操作被拒绝，原因: " + strError;
                return -1;
            }

            // 检查是否已经有记载了的<overdue>字段
            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
            if (nodes.Count > 0)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("该读者当前有s个已还违约记录尚未处理"), // "该读者当前有 {0} 个已还违约记录尚未处理，因此{1}操作被拒绝。请读者尽快办理违约后相关手续（如交纳违约金），然后方可进行{2}。"
                    nodes.Count.ToString(),
                    strOperName,
                    strOperName);
                // "该读者当前有 " + Convert.ToString(nodes.Count) + " 个已还违约记录尚未处理，因此" + strOperName + "操作被拒绝。请读者尽快办理违约后相关手续（如交纳违约金），然后方可进行" + strOperName + "。";
                return 0;
            }

            if (this.BorrowCheckOverdue == true)
            {
                // 检查当前是否有潜在的超期册
                // return:
                //      -1  error
                //      0   没有超期册
                //      1   有超期册
                nRet = CheckOverdue(
                    calendar,
                    readerdom,
                    false,  // bForce,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                {
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("因为超期，操作s被拒绝"),   //  + "{0}，因此{1}操作被拒绝。请读者尽快将这些已超期册履行还书手续。"
                        strError,
                        strOperName);
                    // strError + "，因此" + strOperName + "操作被拒绝。请读者尽快将这些已超期册履行还书手续。";
                    return 0;
                }
            }

            // 2008/4/14
            string strBookState = DomUtil.GetElementText(itemdom.DocumentElement, "state");
            if (String.IsNullOrEmpty(strBookState) == false)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("s操作被拒绝，因为册状态为s"),  // "{0}操作被拒绝，原因: 册 '{1}' 的状态为 '{2}'。"
                    strOperName,
                    strItemBarcodeParam,
                    strBookState);
                // strOperName + "操作被拒绝，原因: 册 '" + strItemBarcode + "' 的状态为 '"+ strBookState + "'。";
                return 0;
            }

            // 2010/3/19
            XmlNode nodeParentItem = itemdom.DocumentElement.SelectSingleNode("binding/bindingParent");
            if (nodeParentItem != null)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("s操作被拒绝，因为合订成员册不能单独外借"),  // "{0}操作被拒绝，原因: 合订成员册 {1} 不能单独外借。"
                    strOperName,
                    strItemBarcodeParam);
                return 0;
            }

            // 从想要借阅的册信息中，找到图书类型
            string strBookType = DomUtil.GetElementText(itemdom.DocumentElement, "bookType");

            // 从读者信息中, 找到读者类型
            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement, "readerType");

            // 首次借阅情况，需要判断册数限制条件
            // 而续借情况，因为先前的借阅已经判断过相关权限了，因此不必判断了
            if (bRenew == false)
            {
                // 从读者信息中，找出该读者以前已经借阅过的同类图书的册数
                nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow[@type='" + strBookType + "']");

                int nThisTypeCount = nodes.Count;

                // 得到该类图书的册数限制配置
                MatchResult matchresult;
                string strParamValue = "";
                // return:
                //      reader和book类型均匹配 算4分
                //      只有reader类型匹配，算3分
                //      只有book类型匹配，算2分
                //      reader和book类型都不匹配，算1分
                nRet = app.GetLoanParam(
                    //null,
                    strLibraryCode,
                    strReaderType,
                    strBookType,
                    "可借册数",
                    out strParamValue,
                    out matchresult,
                    out strError);
                if (nRet == -1 || nRet < 4)
                {
                    // text-level: 用户提示
                    strError = "馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 图书类型 '" + strBookType + "' 尚未定义 可借册数 参数, 因此拒绝" + strOperName + "操作";
                    return -1;
                }

                // 看看是此类否超过册数限制
                int nThisTypeMax = 0;
                try
                {
                    nThisTypeMax = Convert.ToInt32(strParamValue);
                }
                catch
                {
                    strError = "馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 图书类型 '" + strBookType + "' 的 可借册数 参数值 '" + strParamValue + "' 格式有问题, 因此拒绝" + strOperName + "操作";
                    return -1;
                }

                if (nThisTypeCount + 1 > nThisTypeMax)
                {
                    strError = "读者 '" + strReaderBarcode + "' 所借 '" + strBookType + "' 类图书数量将超过 馆代码 '" + strLibraryCode + "' 中 该读者类型 '" + strReaderType + "' 对该图书类型 '" + strBookType + "' 的最多 可借册数 值 '" + strParamValue + "'，因此本次" + strOperName + "操作被拒绝";
                    return 0;
                }

                // 得到该读者类型针对所有类型图书的总册数限制配置
                // return:
                //      reader和book类型均匹配 算4分
                //      只有reader类型匹配，算3分
                //      只有book类型匹配，算2分
                //      reader和book类型都不匹配，算1分
                nRet = app.GetLoanParam(
                    //null,
                    strLibraryCode,
                    strReaderType,
                    "",
                    "可借总册数",
                    out strParamValue,
                    out matchresult,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 用户提示
                    strError = "在获取馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 的 可借总册数 参数过程中出错: " + strError + "。因此拒绝" + strOperName + "操作";
                    return -1;
                }
                if (nRet < 3)
                {
                    // text-level: 用户提示
                    strError = "馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 尚未定义 可借总册数 参数, 因此拒绝" + strOperName + "操作";
                    return -1;
                }

                // 然后看看总册数是否已经超过限制
                int nMax = 0;
                try
                {
                    nMax = Convert.ToInt32(strParamValue);
                }
                catch
                {
                    // text-level: 用户提示
                    strError = "馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 的 可借总册数 参数值 '" + strParamValue + "' 格式有问题, 因此拒绝" + strOperName + "操作";
                    return -1;
                }

                // 从读者信息中，找出该读者已经借阅过的册数
                nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

                int nCount = nodes.Count;

                if (nCount + 1 > nMax)
                {
                    // text-level: 用户提示
                    strError = "读者 '" + strReaderBarcode + "' 所借册数将超过 馆代码 '" + strLibraryCode + "' 中 类型 '" + strReaderType + "' 可借总册数 值'" + strParamValue + "'，因此本次" + strOperName + "操作被拒绝";
                    return 0;
                }
            }

            if (bRenew == false)
            {
                // 检查所借图书的总价格是否超过押金余额
                // return:
                //      -1  error
                //      0   没有超过
                //      1   超过
                nRet = CheckTotalPrice(readerdom,
                    itemdom,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                    return 0;
            }

            return 1;
        }

        // 获得<foregift borrowStyle="????"/>的值(????部分)
        public string GetForegiftBorrowStyle()
        {
            if (this.LibraryCfgDom == null)
                return "";

            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("foregift");
            if (node == null)
                return "";

            return DomUtil.GetAttr(node, "borrowStyle");
        }

        // 检查所借图书的总价格是否超过押金余额
        // return:
        //      -1  error
        //      0   没有超过
        //      1   超过
        int CheckTotalPrice(XmlDocument readerdom,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strCfgStyle = GetForegiftBorrowStyle();
            if (StringUtil.IsInList("checkSum", strCfgStyle) == false)
                return 0;   // 没有启用检查册价格总额是否超过押金余额的功能

            // 获得读者押金余额
            string strForegift = DomUtil.GetElementText(readerdom.DocumentElement,
                "foregift");
            if (String.IsNullOrEmpty(strForegift) == true)
            {
                strError = "读者没有押金余额，不能借书。交纳押金后，才能借书。";
                return -1;
            }

            List<string> foregift_results = null;
            nRet = PriceUtil.SumPrices(strForegift,
                out foregift_results,
                out strError);
            if (nRet == -1)
            {
                strError = "汇总读者押金余额字符串 '" + strForegift + "' 的过程发生错误: " + strError;
                return -1;
            }

            if (foregift_results.Count == 0)
            {
                strError = "读者押金余额字符串 '" + strForegift + "' 经汇总后发现为空，不能借书。交纳押金后才能借书。";
                return -1;
            }

            if (foregift_results.Count > 1)
            {
                strError = "读者押金余额字符串 '" + strForegift + "' 经汇总后发现有多种货币(共" + foregift_results.Count.ToString() + "种)，无法参与册价格比较，因此不能借书。";
                return -1;
            }

            // 汇总已经在借的册的价格总数
            List<string> prices = new List<string>();
            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strPrice = DomUtil.GetAttr(node, "price");

                if (strPrice != null)
                    strPrice = strPrice.Trim();

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                prices.Add(strPrice);
            }

            // 再加上即将要借的一册
            string strThisPrice = DomUtil.GetElementText(itemdom.DocumentElement,
                "price");
            if (String.IsNullOrEmpty(strThisPrice) == false)
                prices.Add(strThisPrice);

            if (prices.Count == 0)
                return 0;   // 都没有价格字符串，也就无法进行计算了

            List<string> results = null;

            nRet = PriceUtil.TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return -1;

            if (results.Count == 0)
            {
                strError = "TotalPrice()出错。册价格汇总后居然为空。";
                return -1;
            }


            if (results.Count > 1)
            {
                strError = "该读者借阅的图书中，其价格的币种为 " + results.Count.ToString() + " 个，无法简单计算出总价";
                return -1;
            }

            // 比较两个价格字符串
            // return:
            //      -3  币种不同，无法直接比较 strError中有说明
            //      -2  error strError中有说明
            //      -1  strPrice1小于strPrice2
            //      0   等于
            //      1   strPrice1大于strPrice2
            nRet = PriceUtil.Compare(foregift_results[0],
                results[0],
                out strError);
            if (nRet == -2)
            {
                strError = "将所借册价格和押金余额进行比较的时候发生错误，借阅操作被拒绝。详情：" + strError;
                return -1;
            }

            if (nRet == -3)
            {
                strError = "所借册价格和押金余额的币种不同，无法进行价格比较，因此借阅操作被拒绝。详情：" + strError;
                return -1;
            }

            if (nRet == -1)
            {
                strError = "本读者已经借阅的图书和当前拟借图书的册价格共为 " + results[0] + "，超过读者押金余额 " + foregift_results[0] + "，借阅操作被拒绝。";
                return 1;
            }

            return 0;
        }


        // Borrow()下级函数
        // 撤销已经写入读者记录的借阅信息
        // 如果记录已经不存在？是否需要用读者证条码号再查出新位置的读者记录来？
        // 因为没有借阅信息的读者记录，确实可能被另外的用户移动位置。
        // parameters:
        //      strReaderRecPath    读者记录路径
        //      strReaderBarcode    读者证条码号。若需要检查记录，看看里面条码号是否已经变化了，就使用这个参数。如果不想检查，就用null
        //      strItemBarcode  已经借的册条码号
        // return:
        //      -1  error
        //      0   没有必要Undo
        //      1   Undo成功
        int UndoBorrowReaderRecord(
            RmsChannel channel,
            string strReaderRecPath,
            string strReaderBarcode,
            string strItemBarcode,
            out string strError)
        {
            strError = "";
            long lRet = 0;
            int nRet = 0;

            string strMetaData = "";
            byte[] reader_timestamp = null;
            string strOutputPath = "";

            string strReaderXml = "";

            int nRedoCount = 0;

        REDO:

            lRet = channel.GetRes(strReaderRecPath,
    out strReaderXml,
    out strMetaData,
    out reader_timestamp,
    out strOutputPath,
    out strError);
            if (lRet == -1)
            {
                strError = "读出原记录 '" + strReaderRecPath + "' 时出错";
                return -1;
            }

            XmlDocument readerdom = null;
            nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载库中读者记录 '" + strReaderRecPath + "' 进入XML DOM时发生错误: " + strError;
                return -1;
            }

            // 检查读者证条码号字段 是否发生变化
            if (String.IsNullOrEmpty(strReaderBarcode) == false)
            {
                string strReaderBarcodeContent = DomUtil.GetElementText(readerdom.DocumentElement,
                    "barcode");
                if (strReaderBarcode != strReaderBarcodeContent)
                {
                    strError = "发现从数据库中读出的读者记录 '" + strReaderRecPath + "' ，其<barcode>字段内容 '" + strReaderBarcodeContent + "' 和要Undo的读者记录证条码号 '" + strReaderBarcode + "' 已不同。";
                    return -1;
                }
            }

            // 去除dom中表示借阅的节点
            XmlNode node = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']");
            if (node == null)
                return 0;   // 已经没有必要Undo了

            node.ParentNode.RemoveChild(node);

            byte[] output_timestamp = null;
            // string strOutputPath = "";

            // 写回读者记录
            lRet = channel.DoSaveTextRes(strReaderRecPath,
                readerdom.OuterXml,
                false,
                "content",  // ,ignorechecktimestamp
                reader_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    nRedoCount++;
                    if (nRedoCount > 10)
                    {
                        strError = "写回读者记录的时候发生时间戳冲突，并且已经重试10次，仍发生错误，只好停止重试";
                        return -1;
                    }
                    goto REDO;
                }

                strError = "写回读者记录的时候发生错误" + strError;
                return -1;
            }

            return 1;   // Undo已经成功
        }

        // 从若干重复条码号的册记录中，选出其中符合当前读者证条码号的
        // parameters:
        //      bOnlyGetFirstItemXml    如果为true，表明在aItemXml中只装入匹配上的第一个记录的XML。这是为了防止内存崩溃。
        //                              如果为false，表明全部匹配记录都进入aItemXml
        // return:
        //      -1  出错
        //      其他    选出的数量
        static int FindItem(
            RmsChannel channel,
            string strReaderBarcode,
            List<string> aPath,
            bool bOnlyGetFirstItemXml,
            out List<string> aFoundPath,
            out List<string> aItemXml,
            out List<byte[]> aTimestamp,
            out string strError)
        {
            aFoundPath = new List<string>();
            aTimestamp = new List<byte[]>();
            aItemXml = new List<string>();
            strError = "";

            for (int i = 0; i < aPath.Count; i++)
            {
                string strXml = "";
                string strMetaData = "";
                string strOutputPath = "";
                byte[] timestamp = null;

                string strPath = aPath[i];

                long lRet = channel.GetRes(strPath,
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 装入DOM
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "记录 '" + strPath + "' XML装入DOM出错: " + ex.Message;
                    goto ERROR1;
                }

                // 检查<borrower>
                string strBorrower = DomUtil.GetElementText(dom.DocumentElement,
                    "borrower");
                if (String.IsNullOrEmpty(strBorrower) == true)
                    continue;

                if (
                    (String.IsNullOrEmpty(strReaderBarcode) == false
                    && strBorrower == strReaderBarcode)
                    // 或者没有提供读者证条码号来鉴别，那就提取出有人借过的所有册
                    || (String.IsNullOrEmpty(strReaderBarcode) == true
                    && String.IsNullOrEmpty(strBorrower) == false)
                    )
                {
                    aFoundPath.Add(strPath);
                    if (bOnlyGetFirstItemXml == true && aItemXml.Count >= 1)
                    {
                        // 被优化掉了
                    }
                    else
                    {
                        aItemXml.Add(strXml);
                    }
                    aTimestamp.Add(timestamp);
                }
            }

            return (aFoundPath.Count);
        ERROR1:
            return -1;
        }


        #endregion

        // 2009/10/27
        // 获得读者姓名
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetReaderName(
            SessionInfo sessioninfo,
            string strReaderBarcode,
            out string strReaderName,
            out string strError)
        {
            strError = "";
            strReaderName = "";
            int nRet = 0;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 读入读者记录
            string strReaderXml = "";
            byte[] reader_timestamp = null;
            string strOutputReaderRecPath = "";

            nRet = this.GetReaderRecXml(
                // sessioninfo.Channels,
                channel,
                strReaderBarcode,
                out strReaderXml,
                out strOutputReaderRecPath,
                out reader_timestamp,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            if (nRet > 1)
            {
            }

            XmlDocument readerdom = null;
            nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                return -1;
            }

            strReaderName = DomUtil.GetElementText(readerdom.DocumentElement, "name");

            return 1;
        }

        static string GetReturnActionName(string strAction)
        {
            if (strAction == "return")
                return "还书";
            else if (strAction == "lost")
                return "丢失声明";
            else if (strAction == "inventory")
                return "盘点";
            else if (strAction == "read")
                return "读过";
            else
                return strAction;
        }

        static bool IsBiblioRecPath(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return false;
            return strText.ToLower().StartsWith("@bibliorecpath:");
        }

        // API: 还书
        // 权限：  工作人员需要return权限，如果是丢失处理需要lost权限；所有读者均不具备还书操作权限。盘点需要 inventory 权限
        // parameters:
        //      strAction   return/lost/inventory/read
        //      strReaderBarcodeParam   读者证条码号。当 strAction 为 "inventory" 时，这里是批次号
        // return:
        //      Result.Value    -1  出错 0 操作成功 1 操作成功，但有值得操作人员留意的情况：如有超期情况；发现条码号重复；需要放入预约架
        public LibraryServerResult Return(
            SessionInfo sessioninfo,
            string strAction,
            string strReaderBarcodeParam,
            string strItemBarcodeParam,
            string strConfirmItemRecPath,
            bool bForce,
            string strStyle,

            string strItemFormatList,   // 2008/5/9
            out string[] item_records,  // 2008/5/9

            string strReaderFormatList,
            out string[] reader_records,

            string strBiblioFormatList, // 2008/5/9
            out string[] biblio_records,    // 2008/5/9

            out string[] aDupPath,
            out string strOutputReaderBarcodeParam,
            out ReturnInfo return_info)
        {
            item_records = null;
            reader_records = null;
            biblio_records = null;
            aDupPath = null;
            strOutputReaderBarcodeParam = "";
            return_info = new ReturnInfo();

            string strError = "";

            List<string> time_lines = new List<string>();
            DateTime start_time = DateTime.Now;
            string strOperLogUID = "";

            LibraryServerResult result = new LibraryServerResult();

            string strActionName = GetReturnActionName(strAction);

            // 个人书斋名
            string strPersonalLibrary = "";
            if (sessioninfo.UserType == "reader"
                && sessioninfo.Account != null)
                strPersonalLibrary = sessioninfo.Account.PersonalLibrary;

            // 权限判断
            if (strAction == "return")
            {
                // 权限字符串
                if (StringUtil.IsInList("return", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = strActionName + "操作被拒绝。不具备 return 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    // return result;
                }
            }
            else if (strAction == "lost")
            {
                // 权限字符串
                if (StringUtil.IsInList("lost", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = strActionName + " 操作被拒绝。不具备 lost 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    // return result;
                }
            }
            else if (strAction == "inventory")
            {
                // 权限字符串
                if (StringUtil.IsInList("inventory", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = strActionName + " 操作被拒绝。不具备 inventory 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    // return result;
                }
            }
            else if (strAction == "read")
            {
                // 权限字符串
                if (StringUtil.IsInList("read", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = strActionName + " 操作被拒绝。不具备 read 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    // return result;
                }
            }
            else
            {
                strError = "无法识别的 strAction 参数值 '" + strAction + "'。";
                goto ERROR1;
            }

            // 对读者身份的附加判断
            // 注：具有个人书斋的，还可以继续向后执行
            if (sessioninfo.UserType == "reader"
                && string.IsNullOrEmpty(strPersonalLibrary) == true)
            {
                result.Value = -1;
                result.ErrorInfo = strActionName + "操作被拒绝。作为读者不能进行此类操作。";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            // 如果没有普通的权限，需要预检查存取权限
            LibraryServerResult result_save = null;
            if (result.Value == -1 && String.IsNullOrEmpty(sessioninfo.Access) == false)
            {
                string strAccessActionList = GetDbOperRights(sessioninfo.Access,
                        "", // 此时还不知道实体库名，先取得当前帐户关于任意一个实体库的存取定义
                        "circulation");
                if (string.IsNullOrEmpty(strAccessActionList) == true)
                    return result;

                // 通过了这样一番检查后，后面依然要检查存取权限。
                // 如果后面检查中，精确针对某个实体库的存取权限存在，则依存取权限；如果不存在，则依普通权限
                result_save = result.Clone();
            }
            else if (result.Value == -1)
                return result;  // 延迟报错 2014/9/16

            result = new LibraryServerResult();

            string strReservationReaderBarcode = "";

            string strReaderBarcode = strReaderBarcodeParam;

            if (strAction == "read" && string.IsNullOrEmpty(strReaderBarcode))
            {
                strError = "读过功能 strReaderBarcode 参数值不应为空";
                goto ERROR1;
            }

            string strBatchNo = "";
            if (strAction == "inventory")
            {
                strBatchNo = strReaderBarcodeParam; // 为避免判断发生混乱，后面统一用 strBatchNo 存储批次号
                strReaderBarcodeParam = "";
                strReaderBarcode = "";
            }

            long lRet = 0;
            int nRet = 0;
            string strIdcardNumber = "";
            string strQrCode = "";  //
            bool bDelayVerifyReaderBarcode = false; // 是否延迟验证
            string strLockReaderBarcode = "";

            if (bForce == true)
            {
                strError = "bForce 参数不能为 true";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(strReaderBarcode) == false)
            {
                string strOutputCode = "";
                // 把二维码字符串转换为读者证条码号
                // parameters:
                //      strReaderBcode  [out]读者证条码号
                // return:
                //      -1      出错
                //      0       所给出的字符串不是读者证号二维码
                //      1       成功      
                nRet = this.DecodeQrCode(strReaderBarcode,
                    out strOutputCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    strQrCode = strReaderBarcode;
                    strReaderBarcode = strOutputCode;
                }
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            int nRedoCount = 0;

        REDO_RETURN:

            bool bReaderLocked = false;
            bool bEntityLocked = false;

            if (String.IsNullOrEmpty(strReaderBarcodeParam) == false)
            {
                // 加读者记录锁
                strLockReaderBarcode = strReaderBarcodeParam;
#if DEBUG_LOCK_READER
                this.WriteErrorLog("Return 开始为读者加写锁 1 '" + strReaderBarcodeParam + "'");
#endif
                this.ReaderLocks.LockForWrite(strReaderBarcodeParam);
                bReaderLocked = true;
                strOutputReaderBarcodeParam = strReaderBarcode;
            }

            string strOutputReaderXml = "";
            string strOutputItemXml = "";
            string strBiblioRecID = "";
            string strOutputItemRecPath = "";
            string strOutputReaderRecPath = "";
            string strLibraryCode = "";
            string strInventoryWarning = "";    // 盘点时的警告信息。先存储在其中，等读者记录完全获得后再报错

            try // 读者记录锁定范围(可能)开始
            {
                // 2016/1/27
                // 读取读者记录
                XmlDocument readerdom = null;
                byte[] reader_timestamp = null;
                string strOldReaderXml = "";
                bool bReaderDbInCirculation = true;

                if (string.IsNullOrEmpty(strReaderBarcode) == false)
                {
                    LibraryServerResult result1 = GetReaderRecord(
                sessioninfo,
                strActionName,
                time_lines,
                strAction != "inventory",
                ref strReaderBarcode,
                ref strIdcardNumber,
                ref strLibraryCode,
                out bReaderDbInCirculation,
                out readerdom,
                out strOutputReaderRecPath,
                out reader_timestamp);
                    if (result1.Value == 0)
                    {
                    }
                    else
                    {
                        return result1;
                    }

                    // 记忆修改前的读者记录
                    strOldReaderXml = readerdom.OuterXml;

                    if (String.IsNullOrEmpty(strIdcardNumber) == false
                        || string.IsNullOrEmpty(strReaderBarcode) == true /* 2013/5/23 */)
                    {
                        // 获得读者证条码号
                        strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                            "barcode");
                    }
                    strOutputReaderBarcodeParam = DomUtil.GetElementText(readerdom.DocumentElement,
                            "barcode");

                    string strReaderDbName = ResPath.GetDbName(strOutputReaderRecPath);

                    // 检查当前用户管辖的读者范围
                    // return:
                    //      -1  出错
                    //      0   允许继续访问
                    //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
                    nRet = CheckReaderRange(sessioninfo,
                        readerdom,
                        strReaderDbName,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        // strError = "当前用户 '" + sessioninfo.UserID + "' 的存取权限或好友关系禁止操作读者(证条码号为 " + strReaderBarcode + ")。具体原因：" + strError;
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                //

                List<string> aPath = null;

                string strItemXml = "";
                byte[] item_timestamp = null;

                // *** 获得册记录 ***
                bool bItemBarcodeDup = false;   // 是否发生册条码号重复情况
                string strDupBarcodeList = "";  // 用于最后返回ErrorInfo的重复册条码号列表

                // 册记录可能加锁
                // 如果读者记录此时已经加锁, 就为册记录加锁
                if (bReaderLocked == true)
                {
                    this.EntityLocks.LockForWrite(strItemBarcodeParam);
                    bEntityLocked = true;
                }

                try // 册记录锁定范围开始
                {
                    WriteTimeUsed(
                        time_lines,
                        start_time,
                        "Return() 中前期检查和锁定 耗时 ");

                    DateTime start_time_read_item = DateTime.Now;

                    // 如果已经有确定的册记录路径
                    if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                    {
                        // 检查路径中的库名，是不是实体库名
                        // return:
                        //      -1  error
                        //      0   不是实体库名
                        //      1   是实体库名
                        nRet = this.CheckItemRecPath(strConfirmItemRecPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            strError = strConfirmItemRecPath + strError;
                            goto ERROR1;
                        }

                        string strMetaData = "";

                        lRet = channel.GetRes(strConfirmItemRecPath,
                            out strItemXml,
                            out strMetaData,
                            out item_timestamp,
                            out strOutputItemRecPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得册记录失败: " + strError;
                            goto ERROR1;
                        }
                    }
                    else if (IsBiblioRecPath(strItemBarcodeParam) == false)
                    {
                        // 从册条码号获得册记录

                        // 获得册记录
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        nRet = this.GetItemRecXml(
                            // sessioninfo.Channels,
                            channel,
                            strItemBarcodeParam,
                            "first",    // 在若干实体库中顺次检索，命中一个以上则返回，不再继续检索更多
                            out strItemXml,
                            100,
                            out aPath,
                            out item_timestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "册条码号 '" + strItemBarcodeParam + "' 不存在";
                            result.ErrorCode = ErrorCode.ItemBarcodeNotFound;
                            return result;
                        }
                        if (nRet == -1)
                        {
                            strError = "读入册记录时发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (aPath.Count > 1)
                        {
                            if (this.Statis != null)
                                this.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "出纳",
                                "还书遇册条码号重复次数",
                                1);

                            bItemBarcodeDup = true; // 此时已经需要设置状态。虽然后面可以进一步识别出真正的册记录

                            // 构造strDupBarcodeList
                            /*
                            string[] pathlist = new string[aPath.Count];
                            aPath.CopyTo(pathlist);
                            strDupBarcodeList = String.Join(",", pathlist);
                             * */
                            strDupBarcodeList = StringUtil.MakePathList(aPath);

                            List<string> aFoundPath = null;
                            List<byte[]> aTimestamp = null;
                            List<string> aItemXml = null;

                            if (String.IsNullOrEmpty(strReaderBarcodeParam) == true)
                            {
                                if (this.Statis != null)
                                    this.Statis.IncreaseEntryValue(
                                    strLibraryCode,
                                    "出纳",
                                    "还书遇册条码号重复并无读者证条码号辅助判断次数",
                                    1);

                                // 如果没有给出读者证条码号参数
                                result.Value = -1;
                                result.ErrorInfo = "册条码号为 '" + strItemBarcodeParam + "' 册记录有 " + aPath.Count.ToString() + " 条，无法进行还书操作。请在附加册记录路径后重新提交还书操作。";
                                result.ErrorCode = ErrorCode.ItemBarcodeDup;

                                aDupPath = new string[aPath.Count];
                                aPath.CopyTo(aDupPath);
                                return result;
                            }

                            // 从若干重复条码号的册记录中，选出其中符合当前读者证条码号的
                            // return:
                            //      -1  出错
                            //      其他    选出的数量
                            nRet = FindItem(
                                channel,
                                strReaderBarcode,
                                aPath,
                                true,   // 优化
                                out aFoundPath,
                                out aItemXml,
                                out aTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "选择重复条码号的册记录时发生错误: " + strError;
                                goto ERROR1;
                            }

                            if (nRet == 0)
                            {
                                result.Value = -1;
                                result.ErrorInfo = "册条码号 '" + strItemBarcodeParam + "' 检索出的 " + aPath.Count + " 条记录中，没有任何一条其<borrower>元素表明了被读者 '" + strReaderBarcode + "' 借阅。";
                                result.ErrorCode = ErrorCode.ItemBarcodeNotFound;
                                return result;
                            }

                            if (nRet > 1)
                            {
                                if (this.Statis != null)
                                    this.Statis.IncreaseEntryValue(
                                    strLibraryCode,
                                    "出纳",
                                    "借书遇册条码号重复并读者证条码号也无法去重次数",
                                    1);

                                result.Value = -1;
                                result.ErrorInfo = "册条码号为 '" + strItemBarcodeParam + "' 并且<borrower>元素表明为读者 '" + strReaderBarcode + "' 借阅的册记录有 " + aFoundPath.Count.ToString() + " 条，无法进行还书操作。请在附加册记录路径后重新提交还书操作。";
                                result.ErrorCode = ErrorCode.ItemBarcodeDup;
                                this.WriteErrorLog(result.ErrorInfo);   // 2012/12/30

                                aDupPath = new string[aFoundPath.Count];
                                aFoundPath.CopyTo(aDupPath);
                                return result;
                            }

                            Debug.Assert(nRet == 1, "");

                            if (this.Statis != null)
                                this.Statis.IncreaseEntryValue(strLibraryCode,
                                "出纳",
                                "借书遇册条码号重复但根据读者证条码号成功去重次数",
                                1);

                            this.WriteErrorLog("借书遇册条码号 '" + strItemBarcodeParam + "' 重复但根据读者证条码号 '" + strReaderBarcode + "' 成功去重");   // 2012/12/30

                            strOutputItemRecPath = aFoundPath[0];
                            item_timestamp = aTimestamp[0];
                            strItemXml = aItemXml[0];
                        }
                        else
                        {
                            Debug.Assert(nRet == 1, "");
                            Debug.Assert(aPath.Count == 1, "");
                            if (nRet == 1)
                            {
                                strOutputItemRecPath = aPath[0];
                                // strItemXml已经有册记录了
                            }
                        }

                        // 函数返回后有用
                        aDupPath = new string[1];
                        aDupPath[0] = strOutputItemRecPath;
                    }

                    // 看看册记录所从属的数据库，是否在参与流通的实体库之列
                    // 2008/6/4
                    string strItemDbName = "";
                    bool bItemDbInCirculation = true;
                    if (strAction != "inventory")
                    {
                        if (String.IsNullOrEmpty(strOutputItemRecPath) == false)
                        {
                            strItemDbName = ResPath.GetDbName(strOutputItemRecPath);
                            if (this.IsItemDbName(strItemDbName, out bItemDbInCirculation) == false)
                            {
                                strError = "册记录路径 '" + strOutputItemRecPath + "' 中的数据库名 '" + strItemDbName + "' 居然不在定义的实体库之列。";
                                goto ERROR1;
                            }
                        }
                    }

                    // 检查存取权限
                    string strAccessParameters = "";

                    {

                        // 检查存取权限
                        if (String.IsNullOrEmpty(sessioninfo.Access) == false)
                        {
                            string strAccessActionList = "";
                            strAccessActionList = GetDbOperRights(sessioninfo.Access,
                                strItemDbName,
                                "circulation");
#if NO
                            if (String.IsNullOrEmpty(strAccessActionList) == true && result_save != null)
                            {
                                // TODO: 也可以直接返回 result_save
                                strError = "当前用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strItemDbName + "' 执行 出纳 操作的存取权限";
                                result.Value = -1;
                                result.ErrorInfo = strError;
                                result.ErrorCode = ErrorCode.AccessDenied;
                                return result;
                            }
#endif
                            if (strAccessActionList == null)
                            {
                                strAccessActionList = GetDbOperRights(sessioninfo.Access,
            "", // 此时还不知道实体库名，先取得当前帐户关于任意一个实体库的存取定义
            "circulation");
                                if (strAccessActionList == null)
                                {
                                    // 对所有实体库都没有定义任何存取权限，这时候要退而使用普通权限
                                    strAccessActionList = sessioninfo.Rights;

                                    // 注：其实此时 result_save == null 即表明普通权限检查已经通过了的
                                }
                                else
                                {
                                    // 对其他实体库定义了存取权限，但对 strItemDbName 没有定义
                                    strError = "当前用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strItemDbName + "' 执行 出纳 操作的存取权限";
                                    result.Value = -1;
                                    result.ErrorInfo = strError;
                                    result.ErrorCode = ErrorCode.AccessDenied;
                                    return result;
                                }
                            }

                            if (strAccessActionList == "*")
                            {
                                // 通配
                            }
                            else
                            {
                                if (IsInAccessList(strAction, strAccessActionList, out strAccessParameters) == false)
                                {
                                    strError = "当前用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strItemDbName + "' 执行 出纳  " + strActionName + " 操作的存取权限";
                                    result.Value = -1;
                                    result.ErrorInfo = strError;
                                    result.ErrorCode = ErrorCode.AccessDenied;
                                    return result;
                                }
                            }
                        }
                    }

                    XmlDocument itemdom = null;
                    if (string.IsNullOrEmpty(strItemXml) == false)
                    {
                        nRet = LibraryApplication.LoadToDom(strItemXml,
                            out itemdom,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "装载册记录进入 XML DOM 时发生错误: " + strError;
                            goto ERROR1;
                        }
                    }

                    WriteTimeUsed(
                        time_lines,
                        start_time_read_item,
                        "Return() 中读取册记录 耗时 ");

                    DateTime start_time_lock = DateTime.Now;

                    // 检查评估模式下书目记录路径
                    if (this.TestMode == true || sessioninfo.TestMode == true)
                    {
                        string strBiblioDbName = "";
                        // 根据实体库名, 找到对应的书目库名
                        // return:
                        //      -1  出错
                        //      0   没有找到
                        //      1   找到
                        nRet = this.GetBiblioDbNameByItemDbName(strItemDbName,
                            out strBiblioDbName,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "根据实体库名 '" + strItemDbName + "' 获得书目库名时出错: " + strError;
                            goto ERROR1;
                        }

                        string strParentID = DomUtil.GetElementText(itemdom.DocumentElement,
    "parent");
                        // 检查评估模式
                        // return:
                        //      -1  检查过程出错
                        //      0   可以通过
                        //      1   不允许通过
                        nRet = CheckTestModePath(strBiblioDbName + "/" + strParentID,
                            out strError);
                        if (nRet != 0)
                        {
                            strError = strActionName + "操作被拒绝: " + strError;
                            goto ERROR1;
                        }
                    }

                    string strOutputReaderBarcode = ""; // 返回的借阅者证条码号
                    if (strAction != "read")
                    {
                        // 在册记录中获得借阅者证条码号
                        // return:
                        //      -1  出错
                        //      0   该册为未借出状态
                        //      1   成功
                        nRet = GetBorrowerBarcode(itemdom,
                            out strOutputReaderBarcode,
                            out strError);
                        if (strAction == "inventory")
                        {
                            if (nRet == -1)
                            {
                                strError = strError + " (册记录路径为 '" + strOutputItemRecPath + "')";
                                goto ERROR1;
                            }
                            if (string.IsNullOrEmpty(strOutputReaderBarcode) == false)
                            {
                                // 该册处于被借阅状态，需要警告前端，建议立即进行还书操作
                                strInventoryWarning = "册 " + strItemBarcodeParam + " 当前处于被借阅状态。如确属在架已还图书，建议立即为之补办还书手续。" + " (册记录路径为 '" + strOutputItemRecPath + "')";
                            }
                        }
                        else
                        {
                            if (nRet == -1 || nRet == 0)
                            {
                                strError = strError + " (册记录路径为 '" + strOutputItemRecPath + "')";
                                goto ERROR1;
                            }
                        }

                        // 如果提供了读者证条码号，则需要核实
                        if (String.IsNullOrEmpty(strReaderBarcodeParam) == false)
                        {
                            if (strOutputReaderBarcode != strReaderBarcodeParam)
                            {
#if NO
                            if (StringUtil.IsIdcardNumber(strReaderBarcodeParam) == true)
                            {
                                // 暂时不报错，滞后验证
                                bDelayVerifyReaderBarcode = true;
                                strIdcardNumber = strReaderBarcodeParam;
                            }
                            else
                            {
                                strError = "册记录表明，册 " + strItemBarcode + " 实际被读者 " + strOutputReaderBarcode + " 所借阅，而不是您当前输入的读者(证条码号) " + strReaderBarcodeParam + "。还书操作被放弃。";
                                goto ERROR1;
                            }
#endif
                                // 暂时不报错，滞后验证
                                bDelayVerifyReaderBarcode = true;
                                strIdcardNumber = strReaderBarcodeParam;
                            }
                        }

                        if (String.IsNullOrEmpty(strReaderBarcode) == true)
                            strReaderBarcode = strOutputReaderBarcode;
                    }

                    // *** 如果读者记录在前面没有锁定, 在这里锁定
                    if (bReaderLocked == false && string.IsNullOrEmpty(strReaderBarcode) == false)
                    {
                        // 加读者记录锁
                        strLockReaderBarcode = strReaderBarcode;
#if DEBUG_LOCK_READER
                        this.WriteErrorLog("Return 开始为读者加写锁 2 '" + strLockReaderBarcode + "'");
#endif
                        this.ReaderLocks.LockForWrite(strLockReaderBarcode);
                        bReaderLocked = true;
                        strOutputReaderBarcodeParam = strReaderBarcode;
                    }

                    // *** 如果册记录在前面没有锁定，则在这里锁定
                    if (bEntityLocked == false)
                    {
                        this.EntityLocks.LockForWrite(strItemBarcodeParam);
                        bEntityLocked = true;

                        // 因为前面对于册记录一直没有加锁，所以这里锁定后要
                        // 检查时间戳，确保记录内容没有（实质性）改变
                        byte[] temp_timestamp = null;
                        string strTempOutputPath = "";
                        string strTempItemXml = "";
                        string strMetaData = "";

                        lRet = channel.GetRes(
                            strOutputItemRecPath,
                            out strTempItemXml,
                            out strMetaData,
                            out temp_timestamp,
                            out strTempOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "册条码号(滞后)加锁后重新提取册记录 '" + strOutputItemRecPath + "' 时发生错误: " + strError;
                            goto ERROR1;
                        }

                        // 如果时间戳发生过改变
                        if (ByteArray.Compare(item_timestamp, temp_timestamp) != 0)
                        {
                            // 装载新记录进入DOM
                            XmlDocument temp_itemdom = null;
                            nRet = LibraryApplication.LoadToDom(strTempItemXml,
                                out temp_itemdom,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "装载册记录strTempItemXml 路径'" + strOutputItemRecPath + "' 进入XML DOM时发生错误: " + strError;
                                goto ERROR1;
                            }

                            // 检查新旧册记录有无要害性改变？
                            if (IsItemRecordSignificantChanged(itemdom,
                                temp_itemdom) == true)
                            {
                                // 则只好重做
                                nRedoCount++;
                                if (nRedoCount > 10)
                                {
                                    strError = "Return() 册条码号(滞后)加锁后重新提取册记录的时候,遇到时间戳冲突,并因此重试超过 10 次未能成功, 只好放弃...";
                                    this.WriteErrorLog(strError);
                                    goto ERROR1;
                                }
                                /*
                                // 如果重做超过5次，则索性修改读者证条码号参数，让它具有（经检索提取的）确定的值，这样就不会滞后加锁了
                                if (nRedoCount > 5)
                                    strReaderBarcodeParam = strReaderBarcode;
                                 * */
#if DEBUG_LOCK_READER
                                this.WriteErrorLog("Return goto REDO_RETURN 1 nRedoCount=" + nRedoCount + "");
#endif
                                goto REDO_RETURN;
                            }

                            // 如果没有要害性改变，就刷新相关参数，然后继续向后进行
                            itemdom = temp_itemdom;
                            item_timestamp = temp_timestamp;
                            strItemXml = strTempItemXml;
                        }

                        // 如果时间戳没有发生过改变，则不必刷新任何参数
                    }

                    WriteTimeUsed(
                        time_lines,
                        start_time_lock,
                        "Return() 中补充锁定 耗时 ");

                    // 读入读者记录
                    DateTime start_time_read_reader = DateTime.Now;

                    if (readerdom == null
                        && string.IsNullOrEmpty(strReaderBarcode) == false)
                    {
                        LibraryServerResult result1 = GetReaderRecord(
                    sessioninfo,
                    strActionName,
                    time_lines,
                    strAction != "inventory",
                    ref strReaderBarcode,
                    ref strIdcardNumber,
                    ref strLibraryCode,
                    out bReaderDbInCirculation,
                    out readerdom,
                    out strOutputReaderRecPath,
                    out reader_timestamp);
                        if (result1.Value == 0)
                        {
                        }
                        else
                        {
                            return result1;
                        }

                        // 记忆修改前的读者记录
                        strOldReaderXml = readerdom.OuterXml;

                        if (String.IsNullOrEmpty(strIdcardNumber) == false
                            || string.IsNullOrEmpty(strReaderBarcode) == true /* 2013/5/23 */)
                        {
                            // 获得读者证条码号
                            strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                                "barcode");
                        }
                        strOutputReaderBarcodeParam = DomUtil.GetElementText(readerdom.DocumentElement,
                                "barcode");

                        string strReaderDbName = ResPath.GetDbName(strOutputReaderRecPath);

                        // 检查当前用户管辖的读者范围
                        // return:
                        //      -1  出错
                        //      0   允许继续访问
                        //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
                        nRet = CheckReaderRange(sessioninfo,
                            readerdom,
                            strReaderDbName,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1)
                        {
                            // strError = "当前用户 '" + sessioninfo.UserID + "' 的存取权限或好友关系禁止操作读者(证条码号为 " + strReaderBarcode + ")。具体原因：" + strError;
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }

#if NO
                    string strReaderXml = "";
                    byte[] reader_timestamp = null;
                    if (string.IsNullOrEmpty(strReaderBarcode) == false)
                    {
                        nRet = this.TryGetReaderRecXml(
                            // sessioninfo.Channels,
                            channel,
                            strReaderBarcode,
                            sessioninfo.LibraryCodeList,    // TODO: 测试个人书斋情况
                            out strReaderXml,
                            out strOutputReaderRecPath,
                            out reader_timestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            // 如果是身份证号，则试探检索“身份证号”途径
                            if (StringUtil.IsIdcardNumber(strReaderBarcode) == true)
                            {
                                strIdcardNumber = strReaderBarcode;
                                strReaderBarcode = "";

                                // 通过特定检索途径获得读者记录
                                // return:
                                //      -1  error
                                //      0   not found
                                //      1   命中1条
                                //      >1  命中多于1条
                                nRet = this.GetReaderRecXmlByFrom(
                                    // sessioninfo.Channels,
                                    channel,
                                    strIdcardNumber,
                                    "身份证号",
                                    out strReaderXml,
                                    out strOutputReaderRecPath,
                                    out reader_timestamp,
                                    out strError);
                                if (nRet == -1)
                                {
                                    // text-level: 内部错误
                                    strError = "用身份证号 '" + strIdcardNumber + "' 读入读者记录时发生错误: " + strError;
                                    goto ERROR1;
                                }
                                if (nRet == 0)
                                {
                                    result.Value = -1;
                                    // text-level: 用户提示
                                    result.ErrorInfo = string.Format(this.GetString("身份证号s不存在"),   // "身份证号 '{0}' 不存在"
                                        strIdcardNumber);
                                    result.ErrorCode = ErrorCode.IdcardNumberNotFound;
                                    return result;
                                }
                                if (nRet > 1)
                                {
                                    // text-level: 用户提示
                                    result.Value = -1;
                                    result.ErrorInfo = "用身份证号 '" + strIdcardNumber + "' 检索读者记录命中 " + nRet.ToString() + " 条，因此无法用身份证号来进行借还操作。请改用证条码号来进行借还操作。";
                                    result.ErrorCode = ErrorCode.IdcardNumberDup;
                                    return result;
                                }
                                Debug.Assert(nRet == 1, "");
                                goto SKIP0;
                            }
                            else
                            {
                                // 2013/5/24
                                // 如果需要，从读者证号等辅助途径进行检索
                                foreach (string strFrom in this.PatronAdditionalFroms)
                                {
                                    nRet = this.GetReaderRecXmlByFrom(
                                        // sessioninfo.Channels,
                                        channel,
                                        null,
                                        strReaderBarcode,
                                        strFrom,
                                    out strReaderXml,
                                    out strOutputReaderRecPath,
                                    out reader_timestamp,
                                        out strError);
                                    if (nRet == -1)
                                    {
                                        // text-level: 内部错误
                                        strError = "用" + strFrom + " '" + strReaderBarcode + "' 读入读者记录时发生错误: " + strError;
                                        goto ERROR1;
                                    }
                                    if (nRet == 0)
                                        continue;
                                    if (nRet > 1)
                                    {
                                        // text-level: 用户提示
                                        result.Value = -1;
                                        result.ErrorInfo = "用" + strFrom + " '" + strReaderBarcode + "' 检索读者记录命中 " + nRet.ToString() + " 条，因此无法用" + strFrom + "来进行借还操作。请改用证条码号来进行借还操作。";
                                        result.ErrorCode = ErrorCode.IdcardNumberDup;
                                        return result;
                                    }

                                    Debug.Assert(nRet == 1, "");

                                    strIdcardNumber = "";
                                    strReaderBarcode = "";

                                    goto SKIP0;
                                }
                            }

                            result.Value = -1;
                            result.ErrorInfo = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                            result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                            return result;
                        }
                        if (nRet == -1)
                        {
                            strError = "读入读者记录时发生错误: " + strError;
                            goto ERROR1;
                        }

                        // 2008/6/17
                        if (nRet > 1)
                        {
                            strError = "读入读者记录时，发现读者证条码号 '" + strReaderBarcode + "' 命中 " + nRet.ToString() + " 条，这是一个严重错误，请系统管理员尽快处理。";
                            goto ERROR1;
                        }
                    }
#endif
                SKIP0:

                    if (strAction == "inventory")
                    {
                        nRet = DoInventory(
                            sessioninfo,
                            strAccessParameters,
                            itemdom,
                            strOutputItemRecPath,
                            strBatchNo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        strOutputItemXml = itemdom.OuterXml;

                        strOutputReaderXml = strOldReaderXml;   // strReaderXml;
                        nRet = RemovePassword(ref strOutputReaderXml, out strError);
                        if (nRet == -1)
                        {
                            strError = "从读者记录中去除 password 阶段出错: " + strError;
                            goto ERROR1;
                        }

                        strBiblioRecID = DomUtil.GetElementText(itemdom.DocumentElement, "parent"); //

                        SetReturnInfo(ref return_info, itemdom);
                        goto END3;
                    }

#if NO

                    // 看看读者记录所从属的数据库，是否在参与流通的读者库之列
                    // 2008/6/4
                    bool bReaderDbInCirculation = true;
                    string strReaderDbName = "";
                    if (strAction != "inventory"
                        && String.IsNullOrEmpty(strOutputReaderRecPath) == false)
                    {
                        if (this.TestMode == true || sessioninfo.TestMode == true)
                        {
                            // 检查评估模式
                            // return:
                            //      -1  检查过程出错
                            //      0   可以通过
                            //      1   不允许通过
                            nRet = CheckTestModePath(strOutputReaderRecPath,
                                out strError);
                            if (nRet != 0)
                            {
                                strError = strActionName + "操作被拒绝: " + strError;
                                goto ERROR1;
                            }
                        }

                        strReaderDbName = ResPath.GetDbName(strOutputReaderRecPath);
                        if (this.IsReaderDbName(strReaderDbName,
                            out bReaderDbInCirculation,
                            out strLibraryCode) == false)
                        {
                            strError = "读者记录路径 '" + strOutputReaderRecPath + "' 中的数据库名 '" + strReaderDbName + "' 居然不在定义的读者库之列。";
                            goto ERROR1;
                        }
                    }

                    // TODO: 即便不是参与流通的数据库，也让还书?

                    // 检查当前操作者是否管辖这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (strAction != "inventory"
                        && this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
            sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "读者记录路径 '" + strOutputReaderRecPath + "' 的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }

                    XmlDocument readerdom = null;
                    nRet = LibraryApplication.LoadToDom(strReaderXml,
                        out readerdom,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                        goto ERROR1;
                    }
                    WriteTimeUsed(
                        time_lines,
                        start_time_read_reader,
                        "Return() 中读取读者记录 耗时 ");


                    // string strReaderDbName = ResPath.GetDbName(strOutputReaderRecPath);

                    // 观察读者记录是否在操作范围内
                    // return:
                    //      -1  出错
                    //      0   允许继续访问
                    //      1   权限限制，不允许继续访问。strError 中有说明原因的文字
                    nRet = CheckReaderRange(sessioninfo,
                        readerdom,
                        strReaderDbName,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        // strError = "当前用户 '" + sessioninfo.UserID + "' 的存取权限禁止操作读者(证条码号为 " + strReaderBarcode + ")。具体原因：" + strError;
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
#endif

                    DateTime start_time_process = DateTime.Now;

                    string strReaderName = readerdom == null ? "" :
                        DomUtil.GetElementText(readerdom.DocumentElement, "name");

                    if (bDelayVerifyReaderBarcode == true)
                    {
                        // 顺便验证一下身份证号
                        if (string.IsNullOrEmpty(strIdcardNumber) == false)
                        {
                            Debug.Assert(string.IsNullOrEmpty(strIdcardNumber) == false, "");

                            string strTempIdcardNumber = DomUtil.GetElementText(readerdom.DocumentElement, "idCardNumber");
                            if (strIdcardNumber != strTempIdcardNumber)
                            {
                                strError = "册记录表明，册 " + strItemBarcodeParam + " 实际被读者(证条码号) " + strOutputReaderBarcode + " 所借阅，此读者的身份证号为 " + strTempIdcardNumber + "，不是您当前输入的(验证用)身份证号 " + strIdcardNumber + "。还书操作被放弃。";
                                goto ERROR1;
                            }
                        }
                        // 重新获取读者证条码号
                        strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");
                        strOutputReaderBarcodeParam = strReaderBarcode; // 为了返回值

                        {
                            if (strOutputReaderBarcode != strReaderBarcode)
                            {
                                strError = "册记录表明，册 " + strItemBarcodeParam + " 实际被读者 " + strOutputReaderBarcode + " 所借阅，而不是您当前指定的读者(证条码号) " + strReaderBarcodeParam + "。还书操作被放弃。";
                                goto ERROR1;
                            }
                        }
                    }

                    XmlDocument domOperLog = new XmlDocument();
                    domOperLog.LoadXml("<root />");
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "libraryCode",
                        strLibraryCode);    // 读者所在的馆代码
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "return");
                    DomUtil.SetElementText(domOperLog.DocumentElement, "action", strAction);

                    // 从读者信息中, 找到读者类型
                    string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                        "readerType");

                    // 证状态 2009/1/29
                    string strReaderState = DomUtil.GetElementText(readerdom.DocumentElement,
                        "state");

                    string strOperTime = this.Clock.GetClock();
                    string strWarning = "";

                    // 处理册记录
                    string strOverdueString = "";
                    string strLostComment = "";

                    if (strAction != "read")
                    {
                        // 获得相关日历
                        Calendar calendar = null;
                        // return:
                        //      -1  出错
                        //      0   没有找到日历
                        //      1   找到日历
                        nRet = GetReaderCalendar(strReaderType,
                            strLibraryCode,
                            out calendar,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                            goto ERROR1;

                        // return:
                        //      -1  出错
                        //      0   正常
                        //      1   超期还书或者丢失处理的情况
                        nRet = DoReturnItemXml(
                            strAction,
                            sessioninfo,    // sessioninfo.Account,
                            calendar,
                            strReaderType,
                            strLibraryCode,
                            strAccessParameters,
                            readerdom,  // 为了调用GetLost()脚本函数
                            ref itemdom,
                            bForce,
                            bItemBarcodeDup,  // 若条码号足以定位，则不记载实体记录路径
                            strOutputItemRecPath,
                            sessioninfo.UserID, // 还书操作者
                            strOperTime,
                            out strOverdueString,
                            out strLostComment,
                            out return_info,
                            out strWarning,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (string.IsNullOrEmpty(strWarning) == false)
                        {
                            if (String.IsNullOrEmpty(result.ErrorInfo) == false)
                                result.ErrorInfo += "\r\n";
                            result.ErrorInfo += strWarning;
                            result.Value = 1;
                        }
                    }
                    else
                        nRet = 0;

                    string strItemBarcode = "";
                    if (itemdom != null)
                    {
                        strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");

                        // 创建日志记录
                        DomUtil.SetElementText(domOperLog.DocumentElement, "itemBarcode",
                            string.IsNullOrEmpty(strItemBarcode) == false ? strItemBarcode : strItemBarcodeParam);
                        /* 后面会写入<overdues>
                        if (nRet == 1)
                        {
                            // 如果有超期和或丢失处理信息
                            DomUtil.SetElementText(domOperLog.DocumentElement, "overdueString",
                            strOverdueString);
                        }
                         * */
                    }

                    bool bOverdue = false;
                    string strOverdueInfo = "";

                    if (nRet == 1)
                    {
                        bOverdue = true;
                        strOverdueInfo = strError;
                    }

                    // 处理读者记录
                    // string strNewReaderXml = "";
                    string strDeletedBorrowFrag = "";
                    if (strAction != "read")
                    {
                        nRet = DoReturnReaderXml(
                            strLibraryCode,
                            ref readerdom,
                            strItemBarcodeParam,
                            strItemBarcode,
                            strOverdueString.StartsWith("!") ? "" : strOverdueString,
                            sessioninfo.UserID, // 还书操作者
                            strOperTime,
                            sessioninfo.ClientAddress,  // 前端触发
                            out strDeletedBorrowFrag,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    // 创建日志记录
                    Debug.Assert(string.IsNullOrEmpty(strReaderBarcode) == false, "");
                    DomUtil.SetElementText(domOperLog.DocumentElement, "readerBarcode",
                        strReaderBarcode);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        sessioninfo.UserID);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTime);

                    if (strAction == "read")
                    {
                        string strBiblioRecPath = "";
                        string strVolume = "";
                        if (IsBiblioRecPath(strItemBarcodeParam) == false)
                        {
                            strBiblioRecID = DomUtil.GetElementText(itemdom.DocumentElement, "parent"); //

                            string strBiblioDbName = "";
                            // 根据实体库名, 找到对应的书目库名
                            // return:
                            //      -1  出错
                            //      0   没有找到
                            //      1   找到
                            nRet = this.GetBiblioDbNameByItemDbName(strItemDbName,
                                out strBiblioDbName,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            if (string.IsNullOrEmpty(strBiblioDbName) == false)
                            {
                                strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
                                DomUtil.SetElementText(domOperLog.DocumentElement, "biblioRecPath",
                                    strBiblioRecPath);
                            }

                            strVolume = DomUtil.GetElementText(itemdom.DocumentElement, "volume");
                            if (string.IsNullOrEmpty(strVolume) == false)
                                DomUtil.SetElementText(domOperLog.DocumentElement, "no", strVolume);
                        }
                        else
                            strBiblioRecPath = strItemBarcodeParam.Substring("@biblioRecPath:".Length);

                        // 探测 mongodb 库中是否已经存在这样的事项
                        IEnumerable<ChargingOperItem> collection = this.ChargingOperDatabase.Exists(
                            strReaderBarcode,
                            "", // string.IsNullOrEmpty(strItemBarcode) == false ? strItemBarcode : strItemBarcodeParam,
                            strBiblioRecPath,
                            strVolume,
                            new DateTime(0),    // DateTime.Now - new TimeSpan(0, 5, 0),
                            new DateTime(0),
                            "read");
                        if (collection != null)
                        {
                            DateTime existingOperTime = new DateTime(0);

                            foreach (ChargingOperItem item in collection)
                            {
                                existingOperTime = item.OperTime;
                                break;
                            }

                            if (existingOperTime != new DateTime(0))
                            {
                                strError = "读者 '" + strReaderBarcode + "' 早先 (" + existingOperTime.ToString("G") + ") 已经读过 [" + GetReadCaption(strBiblioRecPath, strVolume) + "] 了，本次操作被拒绝";
                                goto ERROR1;
                            }
                        }

                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "biblioRecPath", strBiblioRecPath);
                        goto WRITE_OPERLOG;
                    }

                    WriteTimeUsed(
                        time_lines,
                        start_time_process,
                        "Return() 中进行各种数据处理 耗时 ");

                    // 原来创建输出xml或html格式的代码在此

                    DateTime start_time_reservation_check = DateTime.Now;

                    if (StringUtil.IsInList("simulate_reservation_arrive", strStyle))
                    {
                        // 模拟预约情况
                        nRet = SimulateReservation(
                            ref readerdom,
                            ref itemdom,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    // 察看本册预约情况, 并进行初步处理
                    // 如果为丢失处理，需要通知等待者，书已经丢失了，不用再等待
                    // return:
                    //      -1  error
                    //      0   没有修改
                    //      1   进行过修改
                    nRet = DoItemReturnReservationCheck(
                        (strAction == "lost") ? true : false,
                        ref itemdom,
                        out strReservationReaderBarcode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 1 && return_info != null)
                    {
                        // <location>元素中可能增加了 #reservation 部分
                        return_info.Location = DomUtil.GetElementText(itemdom.DocumentElement,
                            "location");
                    }

                    WriteTimeUsed(
                        time_lines,
                        start_time_reservation_check,
                        "Return() 中进行预约检查 耗时 ");

                    // 写回读者、册记录
                    // byte[] timestamp = null;
                    byte[] output_timestamp = null;
                    string strOutputPath = "";

                    /*
                    Channel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        goto ERROR1;
                    }
                     * */
                    DateTime start_time_write_reader = DateTime.Now;

                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        readerdom.OuterXml,
                        false,
                        "content",  // ,ignorechecktimestamp
                        reader_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        // 2015/9/2
                        this.WriteErrorLog("Return() 写入读者记录 '" + strOutputReaderRecPath + "' 时出错: " + strError);

                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            nRedoCount++;
                            if (nRedoCount > 10)
                            {
                                strError = "Return() 写回读者记录的时候,遇到时间戳冲突,并因此重试超过 10 次未能成功, 只好放弃重试...";
                                this.WriteErrorLog(strError);
                                goto ERROR1;
                            }
#if DEBUG_LOCK_READER
                            this.WriteErrorLog("Return goto REDO_RETURN 2 nRedoCount=" + nRedoCount + "");
#endif
                            goto REDO_RETURN;
                        }

                        goto ERROR1;
                    }

                    reader_timestamp = output_timestamp;

                    WriteTimeUsed(
                        time_lines,
                        start_time_write_reader,
                        "Return() 中写回读者记录 耗时 ");

                    DateTime start_time_write_item = DateTime.Now;

                    lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                        itemdom.OuterXml,
                        false,
                        "content,ignorechecktimestamp",
                        item_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        // 2015/9/2
                        this.WriteErrorLog("Return() 写入册记录 '" + strOutputItemRecPath + "' 时出错: " + strError);

                        // 要Undo刚才对读者记录的写入
                        string strError1 = "";
                        lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                            strOldReaderXml,    // strReaderXml,
                            false,
                            "content,ignorechecktimestamp",
                            reader_timestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError1);
                        if (lRet == -1)
                        {
                            // 2015/9/2
                            this.WriteErrorLog("Return() 写入读者记录 '" + strOutputReaderRecPath + "' 时出错: " + strError);

                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                // 读者记录Undo的时候, 发现时间戳冲突了
                                // 这时需要读出现存记录, 试图增加回刚删除的<borrows><borrow>元素
                                // return:
                                //      -1  error
                                //      0   没有必要Undo
                                //      1   Undo成功
                                nRet = UndoReturnReaderRecord(
                                    channel,
                                    strOutputReaderRecPath,
                                    strReaderBarcode,
                                    strItemBarcodeParam,
                                    strDeletedBorrowFrag,
                                    strOverdueString.StartsWith("!") ? "" : strOverdueString,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "Return() Undo读者记录 '" + strOutputReaderRecPath + "' (读者证条码号为 '" + strReaderBarcode + "' 读者姓名为 '" + strReaderName + "') 还书册条码号 '" + strItemBarcodeParam + "' 的修改时，发生错误，无法Undo: " + strError;
                                    this.WriteErrorLog(strError);
                                    goto ERROR1;
                                }

                                // 成功
                                // 2015/9/2 增加下列防止死循环的语句
                                nRedoCount++;
                                if (nRedoCount > 10)
                                {
                                    strError = "Return() Undo 读者记录(1)成功，试图重试 Return 时，发现先前重试已经超过 10 次，只好不重试了，做出错返回...";
                                    this.WriteErrorLog(strError);
                                    goto ERROR1;
                                }
#if DEBUG_LOCK_READER
                                this.WriteErrorLog("Return goto REDO_RETURN 3 nRedoCount=" + nRedoCount + "");
#endif
                                goto REDO_RETURN;
                            }


                            // 以下为 不是时间戳冲突的其他错误情形
                            strError = "Return() Undo读者记录 '" + strOutputReaderRecPath + "' (读者证条码号为 '" + strReaderBarcode + "' 读者姓名为 '" + strReaderName + "') 还书册条码号 '" + strItemBarcodeParam + "' 的修改时，发生错误，无法Undo: " + strError;
                            // strError = strError + ", 并且Undo写回旧读者记录也失败: " + strError1;
                            this.WriteErrorLog(strError);
                            goto ERROR1;
                        }

                        // 以下为Undo成功的情形
                        // 2015/9/2 增加下列防止死循环的语句
                        nRedoCount++;
                        if (nRedoCount > 10)
                        {
                            strError = "Return() Undo 读者记录(2)成功，试图重试 Return 时，发现先前重试已经超过 10 次，只好不重试了，做出错返回...";
                            this.WriteErrorLog(strError);
                            goto ERROR1;
                        }
#if DEBUG_LOCK_READER
                        this.WriteErrorLog("Return goto REDO_RETURN 4 nRedoCount=" + nRedoCount + "");
#endif
                        goto REDO_RETURN;
                    }

                    WriteTimeUsed(
                        time_lines,
                        start_time_write_item,
                        "Return() 中写回册记录 耗时 ");

                WRITE_OPERLOG:
                    DateTime start_time_write_operlog = DateTime.Now;

                    // 写入日志

                    // overdue信息
                    if (String.IsNullOrEmpty(strOverdueString) == false)
                    {
                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "overdues",
                            strOverdueString.StartsWith("!") ? strOverdueString.Substring(1) : strOverdueString);
                    }

                    // 确认册路径
                    if (string.IsNullOrEmpty(strConfirmItemRecPath) == false)
                    {
                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "confirmItemRecPath", strConfirmItemRecPath);
                    }

                    if (string.IsNullOrEmpty(strIdcardNumber) == false)
                    {
                        // 表明是使用身份证号来完成还书操作的
                        DomUtil.SetElementText(domOperLog.DocumentElement,
        "idcardNumber", strIdcardNumber);
                    }

                    // 写入读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerRecord", readerdom.OuterXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);

                    // 写入册记录
                    if (itemdom != null)
                    {
                        node = DomUtil.SetElementText(domOperLog.DocumentElement,
                            "itemRecord", itemdom.OuterXml);
                        DomUtil.SetAttr(node, "recPath", strOutputItemRecPath);
                    }

                    if (strLostComment != "")
                    {
                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "lostComment",
                            strLostComment);
                    }

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        start_time,
                        out strOperLogUID,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "Return() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }

                    WriteTimeUsed(
                        time_lines,
                        start_time_write_operlog,
                        "Return() 中写操作日志 耗时 ");

                    DateTime start_time_write_statis = DateTime.Now;

                    // 写入统计指标
#if NO
                    if (this.m_strLastReaderBarcode != strReaderBarcode)
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(strLibraryCode,
                            "出纳",
                            "读者数",
                            1);
                        this.m_strLastReaderBarcode = strReaderBarcode;
                    }
#endif
                    if (this.Garden != null)
                        this.Garden.Activate(strReaderBarcode,
                            strLibraryCode);

                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(strLibraryCode,
                        "出纳",
                        strAction == "read" ? "读过册" : "还册",
                        1);

                    if (strAction == "lost")
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(strLibraryCode,
                            "出纳",
                            "声明丢失",
                            1);
                    }
                    WriteTimeUsed(
                        time_lines,
                        start_time_write_statis,
                        "Return() 中写统计指标 耗时 ");

                    result.ErrorInfo = strActionName + "操作成功。" + result.ErrorInfo;  // 2013/11/13

                    if (bReaderDbInCirculation == false)
                    {
                        if (String.IsNullOrEmpty(result.ErrorInfo) == false)
                            result.ErrorInfo += "\r\n";
                        result.ErrorInfo += "读者证条码号 '" + strReaderBarcode + "' 所在的读者记录 '" + strOutputReaderRecPath + "' 其数据库 '" + StringUtil.GetDbName(strOutputReaderRecPath) + "' 属于未参与流通的读者库。";
                        result.Value = 1;
                    }

                    if (bItemDbInCirculation == false)
                    {
                        if (String.IsNullOrEmpty(result.ErrorInfo) == false)
                            result.ErrorInfo += "\r\n";
                        result.ErrorInfo += "册条码号 '" + strItemBarcodeParam + "' 所在的册记录 '" + strOutputItemRecPath + "' 其数据库 '" + StringUtil.GetDbName(strOutputReaderRecPath) + "' 属于未参与流通的实体库。";
                        result.Value = 1;
                    }

                    if (bOverdue == true)
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(strLibraryCode,
                            "出纳",
                            "还超期册",
                            1);

                        if (String.IsNullOrEmpty(result.ErrorInfo) == false)
                            result.ErrorInfo += "\r\n";

                        result.ErrorInfo += strOverdueInfo;
                        result.ErrorCode = ErrorCode.Overdue;
                        result.Value = 1;
                    }

                    if (bItemBarcodeDup == true)
                    {
                        if (String.IsNullOrEmpty(result.ErrorInfo) == false)
                            result.ErrorInfo += "\r\n";
                        result.ErrorInfo += "***警告***: " + strActionName + "操作过程中发现下列册记录它们的册条码号发生了重复: " + strDupBarcodeList + "。请通知系统管理员纠正此数据错误。";
                        result.Value = 1;
                    }

                    if (String.IsNullOrEmpty(strReservationReaderBarcode) == false // 2009/10/19 changed  //bFoundReservation == true
                        && strAction != "lost")
                    {
                        // 为了提示信息中出现读者姓名，这里特以获取读者姓名
                        string strReservationReaderName = "";

                        if (strReaderBarcode == strReservationReaderBarcode)
                            strReservationReaderName = strReaderName;
                        else
                        {
                            DateTime start_time_getname = DateTime.Now;

                            // 获得读者姓名
                            // return:
                            //      -1  error
                            //      0   not found
                            //      1   found
                            nRet = GetReaderName(
                                sessioninfo,
                                strReservationReaderBarcode,
                                out strReservationReaderName,
                                out strError);

                            WriteTimeUsed(
                                time_lines,
                                start_time_getname,
                                "Return() 中获得预约者的姓名 耗时 ");
                        }

                        if (String.IsNullOrEmpty(result.ErrorInfo) == false)
                            result.ErrorInfo += "\r\n";
                        result.ErrorInfo += "因本册图书已被读者 " + strReservationReaderBarcode + " "
                            + strReservationReaderName + " 预约，请放入预约保留架。";    // 2009/10/10 changed
                        result.Value = 1;
                    }

                    // 读者证状态不为空情况下的提示
                    // 2008/1/29
                    if (String.IsNullOrEmpty(strReaderState) == false)
                    {
                        if (String.IsNullOrEmpty(result.ErrorInfo) == false)
                            result.ErrorInfo += "\r\n";
                        result.ErrorInfo += "***警告***: 当前读者证状态为: " + strReaderState + "。请注意进行后续处理。";
                        result.Value = 1;
                    }

                    if (itemdom != null)
                        strOutputItemXml = itemdom.OuterXml;

                    // strOutputReaderXml 将用于构造读者记录返回格式
                    DomUtil.DeleteElement(readerdom.DocumentElement, "password");
                    strOutputReaderXml = readerdom.OuterXml;

                    if (itemdom != null)
                    {
                        strBiblioRecID = DomUtil.GetElementText(itemdom.DocumentElement, "parent"); //
                    }
                } // 册记录锁定范围结束
                finally
                {
                    // 册记录解锁
                    if (bEntityLocked == true)
                        this.EntityLocks.UnlockForWrite(strItemBarcodeParam);
                }

            } // 读者记录锁定范围结束
            finally
            {
                if (bReaderLocked == true)
                {
                    this.ReaderLocks.UnlockForWrite(strLockReaderBarcode);
#if DEBUG_LOCK_READER
                    this.WriteErrorLog("Return 结束为读者加写锁 '" + strLockReaderBarcode + "'");
#endif
                }
            }

            // TODO: 将来可以改进为，丢失时发现有人预约，也通知，不过通知的内容是要读者不再等待了。
            if (String.IsNullOrEmpty(strReservationReaderBarcode) == false
                && strAction != "lost")
            {
                DateTime start_time_1 = DateTime.Now;

                List<string> DeletedNotifyRecPaths = null;  // 被删除的通知记录。不用。
                // 通知预约到书的操作
                // 出于对读者库加锁方面的便利考虑, 单独做了此函数
                // return:
                //      -1  error
                //      0   没有找到<request>元素
                nRet = DoReservationNotify(
                    // sessioninfo.Channels,
                    channel,
                    strReservationReaderBarcode,
                    true,   // 需要函数内加锁
                    strItemBarcodeParam,
                    false,  // 不在大架
                    false,  // 不需要再修改当前册记录，因为前面已经修改过了
                    out DeletedNotifyRecPaths,
                    out strError);
                if (nRet == -1)
                {
                    strError = "还书操作已经成功, 但是预约到书通知功能失败, 原因: " + strError;
                    goto ERROR1;
                }

                WriteTimeUsed(
time_lines,
start_time_1,
"Return() 中预约到书通知 耗时 ");

                /* 前面已经通知过了
                result.Value = 1;
                result.ErrorCode = ErrorCode.ReturnReservation;
                if (result.ErrorInfo != "")
                    result.ErrorInfo += "\r\n";

                result.ErrorInfo += "还书操作成功。因此册图书被读者 " + strReservationReaderBarcode + " 预约，请放入预约保留架。";
                 * */

                // 最好超期和保留两种状态码可以并存?
            }

        END3:
            // 输出数据
            // 把输出数据部分放在读者锁以外范围，是为了尽量减少锁定的时间，提高并发运行效率
            DateTime output_start_time = DateTime.Now;

            if (String.IsNullOrEmpty(strOutputReaderXml) == false
    && StringUtil.IsInList("reader", strStyle) == true)
            {
                DateTime start_time_1 = DateTime.Now;

                nRet = BuildReaderResults(
sessioninfo,
null,
strOutputReaderXml,
strReaderFormatList,
strLibraryCode,  // calendar/advancexml/html 时需要
null,    // recpaths 时需要
strOutputReaderRecPath,   // recpaths 时需要
null,    // timestamp 时需要
OperType.Return,
                            null,
                            strItemBarcodeParam,
ref reader_records,
out strError);
                if (nRet == -1)
                {
                    strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                    goto ERROR1;
                }

                WriteTimeUsed(
time_lines,
start_time_1,
"Return() 中返回读者记录(" + strReaderFormatList + ") 耗时 ");
            }

#if NO
            if (String.IsNullOrEmpty(strOutputReaderXml) == false
                && StringUtil.IsInList("reader", strStyle) == true)
            {
                DateTime start_time_1 = DateTime.Now;

                string[] reader_formats = strReaderFormatList.Split(new char[] { ',' });
                reader_records = new string[reader_formats.Length];

                for (int i = 0; i < reader_formats.Length; i++)
                {
                    string strReaderFormat = reader_formats[i];

                    // 将读者记录数据从XML格式转换为HTML格式
                    // if (String.Compare(strReaderFormat, "html", true) == 0)
                    if (IsResultType(strReaderFormat, "html") == true)
                    {
                        string strReaderRecord = "";
                        nRet = this.ConvertReaderXmlToHtml(
                            sessioninfo,
                            this.CfgDir + "\\readerxml2html.cs",
                            this.CfgDir + "\\readerxml2html.cs.ref",
                            strLibraryCode,
                            strOutputReaderXml,
                            strOutputReaderRecPath, // 2009/10/18
                            OperType.Return,
                            null,
                            strItemBarcodeParam,
                            strReaderFormat,
                            out strReaderRecord,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                            goto ERROR1;
                        }
                        reader_records[i] = strReaderRecord;
                    }
                    // 将读者记录数据从XML格式转换为text格式
                    // else if (String.Compare(strReaderFormat, "text", true) == 0)
                    else if (IsResultType(strReaderFormat, "text") == true)
                    {
                        string strReaderRecord = "";
                        nRet = this.ConvertReaderXmlToHtml(
                            sessioninfo,
                            this.CfgDir + "\\readerxml2text.cs",
                            this.CfgDir + "\\readerxml2text.cs.ref",
                            strLibraryCode,
                            strOutputReaderXml,
                            strOutputReaderRecPath, // 2009/10/18
                            OperType.Return,
                            null,
                            strItemBarcodeParam,
                            strReaderFormat,
                            out strReaderRecord,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                            goto ERROR1;
                        }
                        reader_records[i] = strReaderRecord;
                    }
                    // else if (String.Compare(strReaderFormat, "xml", true) == 0)
                    else if (IsResultType(strReaderFormat, "xml") == true)
                    {
                        // reader_records[i] = strOutputReaderXml;
                        string strResultXml = "";
                        nRet = GetItemXml(strOutputReaderXml,
            strReaderFormat,
            out strResultXml,
            out strError);
                        if (nRet == -1)
                        {
                            strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                            goto ERROR1;
                        }
                        reader_records[i] = strResultXml;
                    }
                    else if (IsResultType(strReaderFormat, "summary") == true)
                    {
                        // 2013/12/15
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strOutputReaderXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "读者 XML 装入 DOM 出错: " + ex.Message;
                            strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                            goto ERROR1;
                        }
                        reader_records[i] = DomUtil.GetElementText(dom.DocumentElement, "name");
                    }
                    else
                    {
                        strError = "strReaderFormatList参数出现了不支持的数据格式类型 '" + strReaderFormat + "'";
                        strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                        goto ERROR1;
                    }
                } // end of for

                WriteTimeUsed(
    time_lines,
    start_time_1,
    "Return() 中返回读者记录(" + strReaderFormatList + ") 耗时 ");

            } // end if
#endif

            // 2008/5/9
            if (String.IsNullOrEmpty(strOutputItemXml) == false
                && StringUtil.IsInList("item", strStyle) == true)
            {
                DateTime start_time_1 = DateTime.Now;

                string[] item_formats = strItemFormatList.Split(new char[] { ',' });
                item_records = new string[item_formats.Length];

                for (int i = 0; i < item_formats.Length; i++)
                {
                    string strItemFormat = item_formats[i];

                    // 将册记录数据从XML格式转换为HTML格式
                    // if (String.Compare(strItemFormat, "html", true) == 0)
                    if (IsResultType(strItemFormat, "html") == true)
                    {
                        string strItemRecord = "";
                        nRet = this.ConvertItemXmlToHtml(
                            this.CfgDir + "\\itemxml2html.cs",
                            this.CfgDir + "\\itemxml2html.cs.ref",
                            strOutputItemXml,
                            strOutputItemRecPath,   // 2009/10/18
                            out strItemRecord,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                            goto ERROR1;
                        }
                        item_records[i] = strItemRecord;
                    }
                    // 将册记录数据从XML格式转换为text格式
                    // else if (String.Compare(strItemFormat, "text", true) == 0)
                    else if (IsResultType(strItemFormat, "text") == true)
                    {
                        string strItemRecord = "";
                        nRet = this.ConvertItemXmlToHtml(
                            this.CfgDir + "\\itemxml2text.cs",
                            this.CfgDir + "\\itemxml2text.cs.ref",
                            strOutputItemXml,
                            strOutputItemRecPath,   // 2009/10/18
                            out strItemRecord,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                            goto ERROR1;
                        }
                        item_records[i] = strItemRecord;
                    }
                    // else if (String.Compare(strItemFormat, "xml", true) == 0)
                    else if (IsResultType(strItemFormat, "xml") == true)
                    {
                        // item_records[i] = strOutputItemXml;
                        string strResultXml = "";
                        nRet = GetItemXml(strOutputItemXml,
            strItemFormat,
            out strResultXml,
            out strError);
                        if (nRet == -1)
                        {
                            strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                            goto ERROR1;
                        }
                        item_records[i] = strResultXml;
                    }
                    else
                    {
                        strError = "strItemFormatList参数出现了不支持的数据格式类型 '" + strItemFormat + "'";
                        strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                        goto ERROR1;
                    }
                } // end of for

                WriteTimeUsed(
time_lines,
start_time_1,
"Return() 中返回册记录(" + strItemFormatList + ") 耗时 ");

            }

            // 2008/5/9
            if (StringUtil.IsInList("biblio", strStyle) == true)
            {
                DateTime start_time_1 = DateTime.Now;
                string strBiblioRecPath = "";
                if (IsBiblioRecPath(strItemBarcodeParam) == true)
                    strBiblioRecPath = strItemBarcodeParam.Substring("@biblioRecPath:".Length);

                if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                {
                    if (String.IsNullOrEmpty(strBiblioRecID) == true)
                    {
                        strError = "册记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录ID";
                        strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                        goto ERROR1;
                    }

                    string strItemDbName = ResPath.GetDbName(strOutputItemRecPath);

                    string strBiblioDbName = "";
                    // 根据实体库名, 找到对应的书目库名
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = this.GetBiblioDbNameByItemDbName(strItemDbName,
                        out strBiblioDbName,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "实体库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                        strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                        goto ERROR1;
                    }

                    strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
                }

                string[] biblio_formats = strBiblioFormatList.Split(new char[] { ',' });
                biblio_records = new string[biblio_formats.Length];

                string strBiblioXml = "";
                // 至少有html xml text之一，才获取strBiblioXml
                if (StringUtil.IsInList("html", strBiblioFormatList) == true
                    || StringUtil.IsInList("xml", strBiblioFormatList) == true
                    || StringUtil.IsInList("text", strBiblioFormatList) == true)
                {
#if NO
                    RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                        goto ERROR1;
                    }
#endif

                    string strMetaData = "";
                    byte[] timestamp = null;
                    string strTempOutputPath = "";
                    lRet = channel.GetRes(strBiblioRecPath,
                        out strBiblioXml,
                        out strMetaData,
                        out timestamp,
                        out strTempOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获得种记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                        strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                        goto ERROR1;
                    }
                }

                for (int i = 0; i < biblio_formats.Length; i++)
                {
                    string strBiblioFormat = biblio_formats[i];

                    // 需要从内核映射过来文件
                    string strLocalPath = "";
                    string strBiblio = "";

                    // 将书目记录数据从XML格式转换为HTML格式
                    if (String.Compare(strBiblioFormat, "html", true) == 0)
                    {
                        // TODO: 可以cache
                        nRet = this.MapKernelScriptFile(
                            sessioninfo,
                            StringUtil.GetDbName(strBiblioRecPath),
                            "./cfgs/loan_biblio.fltx",
                            out strLocalPath,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                            goto ERROR1;
                        }

                        // 将种记录数据从XML格式转换为HTML格式
                        string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";

                        if (string.IsNullOrEmpty(strBiblioXml) == false)
                        {
                            nRet = this.ConvertBiblioXmlToHtml(
                                strFilterFileName,
                                strBiblioXml,
                                    null,
                                strBiblioRecPath,
                                out strBiblio,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                                goto ERROR1;
                            }

                        }
                        else
                            strBiblio = "";

                        biblio_records[i] = strBiblio;
                    }
                    // 将册记录数据从XML格式转换为text格式
                    else if (String.Compare(strBiblioFormat, "text", true) == 0)
                    {
                        // TODO: 可以cache
                        nRet = this.MapKernelScriptFile(
                            sessioninfo,
                            StringUtil.GetDbName(strBiblioRecPath),
                            "./cfgs/loan_biblio_text.fltx",
                            out strLocalPath,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                            goto ERROR1;
                        }
                        // 将种记录数据从XML格式转换为TEXT格式
                        string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                        if (string.IsNullOrEmpty(strBiblioXml) == false)
                        {
                            nRet = this.ConvertBiblioXmlToHtml(
                                strFilterFileName,
                                strBiblioXml,
                                    null,
                                strBiblioRecPath,
                                out strBiblio,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                                goto ERROR1;
                            }

                        }
                        else
                            strBiblio = "";

                        biblio_records[i] = strBiblio;
                    }
                    else if (String.Compare(strBiblioFormat, "xml", true) == 0)
                    {
                        biblio_records[i] = strBiblioXml;
                    }
                    else if (String.Compare(strBiblioFormat, "recpath", true) == 0)
                    {
                        biblio_records[i] = strBiblioRecPath;
                    }
                    else if (string.IsNullOrEmpty(strBiblioFormat) == true)
                    {
                        biblio_records[i] = "";
                    }
                    else
                    {
                        strError = "strBiblioFormatList参数出现了不支持的数据格式类型 '" + strBiblioFormat + "'";
                        strError = "虽然出现了下列错误，但是还书操作已经成功: " + strError;
                        goto ERROR1;
                    }
                } // end of for

                WriteTimeUsed(
time_lines,
start_time_1,
"Return() 中返回书目记录(" + strBiblioFormatList + ") 耗时 ");
            }

            this.WriteTimeUsed(
                time_lines,
                start_time,
                "Return() 耗时 ");
            // 如果整个时间超过一秒，则需要计入操作日志
            if (DateTime.Now - start_time > new TimeSpan(0, 0, 1))
            {
                WriteLongTimeOperLog(
                    sessioninfo,
                    strAction,
                    start_time,
                    "整个操作耗时超过 1 秒。详情:" + StringUtil.MakePathList(time_lines, ";"),
                    strOperLogUID,
                    out strError);
            }

            if (string.IsNullOrEmpty(strInventoryWarning) == false)
            {
                result.ErrorInfo = strInventoryWarning;
                result.ErrorCode = ErrorCode.Borrowing;
                result.Value = 1;
            }

            // 如果创建输出数据的时间超过一秒，则需要计入操作日志
            if (DateTime.Now - output_start_time > new TimeSpan(0, 0, 1))
            {
                WriteLongTimeOperLog(
                    sessioninfo,
                    strAction,
                    output_start_time,
                    "output 阶段耗时超过 1 秒",
                    strOperLogUID,
                    out strError);
            }

            // result.Value值在前面可能被设置成1
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 从读者记录中删除 password 元素
        static int RemovePassword(ref string strReaderXml,
            out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(strReaderXml))
                return 0;

            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "读者记录 XML 装入 DOM 时出错:" + ex.Message;
                return -1;
            }

            DomUtil.DeleteElement(readerdom.DocumentElement, "password");
            strReaderXml = readerdom.DocumentElement.OuterXml;
            return 0;
        }

        // 构造用于提示“读过”卷册的文字
        static string GetReadCaption(string strBiblioRecPath, string strVolume)
        {
            if (string.IsNullOrEmpty(strVolume) == true)
                return "书目记录 '" + strBiblioRecPath + "'";
            return "书目记录 '" + strBiblioRecPath + "' 卷 '" + strVolume + "'";
        }

        // 执行盘点记载
        int DoInventory(
            SessionInfo sessioninfo,
            string strAccessParameters,
            XmlDocument itemdom,
            string strItemRecPath,
            string strBatchNo,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strInventoryDbName = GetInventoryDbName();

            if (string.IsNullOrEmpty(strInventoryDbName) == true)
            {
                strError = "当前尚未配置盘点库，无法进行盘点操作";
                return -1;
            }

            // 馆藏地点
            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement, "location");
            // 去掉#reservation部分
            strLocation = StringUtil.GetPureLocationString(strLocation);

            // 获得册记录的馆代码，检查是否在当前用户管辖范围内

            string strLibraryCode = "";
            // 检查一个册记录的馆藏地点是否符合馆代码列表要求
            // parameters:
            //      strLibraryCodeList  当前用户管辖的馆代码列表
            //      strLibraryCode  [out]册记录中的馆代码
            // return:
            //      -1  检查过程出错
            //      0   符合要求
            //      1   不符合要求
            nRet = CheckItemLibraryCode(itemdom,
                sessioninfo.LibraryCodeList,
                out strLibraryCode,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
            {
                strError = "盘点操作被拒绝: " + strError;
                return -1;
            }

#if NO
            string strCode = "";
            string strRoom = "";
            {
                // 解析
                ParseCalendarName(strLocation,
            out strCode,
            out strRoom);
                if (StringUtil.IsInList(strCode, sessioninfo.LibraryCodeList) == false)
                {
                    strError = "册记录的馆藏地 '" + strLocation + "' 不属于当前用户所在馆代码 '" + sessioninfo.LibraryCodeList + "' 管辖，不允许进行盘点操作。";
                    return -1;
                }
            }
#endif

            // 检查存取定义馆藏地列表
            if (string.IsNullOrEmpty(strAccessParameters) == false && strAccessParameters != "*")
            {
                string strCode = "";
                string strRoom = "";
                // 解析
                ParseCalendarName(strLocation,
            out strCode,
            out strRoom);

                bool bFound = false;
                List<string> locations = StringUtil.SplitList(strAccessParameters);
                foreach (string s in locations)
                {
                    string c = "";
                    string r = "";
                    ParseCalendarName(s,
                        out c,
                        out r);
                    if (/*string.IsNullOrEmpty(c) == false && */ c != "*")
                    {
                        if (c != strLibraryCode)
                            continue;
                    }

                    if (/*string.IsNullOrEmpty(r) == false && */ r != "*")
                    {
                        if (r != strRoom)
                            continue;
                    }

                    bFound = true;
                    break;
                }

                if (bFound == false)
                {
                    strError = "盘点操作被拒绝。因册记录的馆藏地 '" + strLocation + "' 不在当前用户存取定义规定的盘点操作的馆藏地许可范围 '" + strAccessParameters + "' 之内";
                    return -1;
                }
            }

            string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");

            string strItemRefID = DomUtil.GetElementText(itemdom.DocumentElement, "refID");

            // 在盘点库中查重
            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            List<string> aPath = null;
            // 根据馆代码、批次号和册条码号对盘点库进行查重
            // 本函数只负责查重, 并不获得记录体
            // return:
            //      -1  error
            //      其他    命中记录条数(不超过nMax规定的极限)
            nRet = SearchInventoryRecDup(
                channel,
                strLibraryCode,
                strBatchNo,
                strItemBarcode,
                strItemRefID,
                2,
                out aPath,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet >= 1)
            {
                if (string.IsNullOrEmpty(strItemBarcode) == false)
                    strError = "馆代码 '" + strLibraryCode + "' 批次号 '" + strBatchNo + "' 册条码号 '" + strItemBarcode + "' 的盘点记录已经存在，无法重复创建 ...";
                else
                    strError = "馆代码 '" + strLibraryCode + "' 批次号 '" + strBatchNo + "' 参考ID '" + strItemRefID + "' 的盘点记录已经存在，无法重复创建 ...";
                return -1;
            }

            // 在盘点库中创建一条新记录
            XmlDocument inventory_dom = new XmlDocument();
            inventory_dom.LoadXml("<root />");

            DomUtil.SetElementText(inventory_dom.DocumentElement, "itemBarcode", strItemBarcode);
            DomUtil.SetElementText(inventory_dom.DocumentElement, "itemRefID", strItemRefID);   // 册记录的参考 ID
            DomUtil.SetElementText(inventory_dom.DocumentElement, "itemRecPath", strItemRecPath);

            DomUtil.SetElementText(inventory_dom.DocumentElement, "batchNo", strBatchNo);
            DomUtil.SetElementText(inventory_dom.DocumentElement, "location", strLocation);
            DomUtil.SetElementText(inventory_dom.DocumentElement, "libraryCode", strLibraryCode);
            DomUtil.SetElementText(inventory_dom.DocumentElement, "refID", Guid.NewGuid().ToString());  // 盘点记录的参考 ID
            string strOperTime = this.Clock.GetClock();
            DomUtil.SetElementText(inventory_dom.DocumentElement, "operator",
                sessioninfo.UserID);   // 操作者
            DomUtil.SetElementText(inventory_dom.DocumentElement, "operTime",
                strOperTime);   // 操作时间

            byte[] output_timestamp = null;
            string strOutputPath = "";

            long lRet = channel.DoSaveTextRes(strInventoryDbName + "/?",
    inventory_dom.OuterXml,
    false,   // include preamble?
    "content",
    null,   // info.OldTimestamp,
    out output_timestamp,
    out strOutputPath,
    out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

#if NO
        // 执行“读过”记载
        int DoRead(
            SessionInfo sessioninfo,
            string strAccessParameters,
            XmlDocument itemdom,
            string strItemRecPath,
            string strBatchNo,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strInventoryDbName = GetInventoryDbName();

            if (string.IsNullOrEmpty(strInventoryDbName) == true)
            {
                strError = "当前尚未配置盘点库，无法进行盘点操作";
                return -1;
            }

            // 馆藏地点
            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement, "location");
            // 去掉#reservation部分
            strLocation = StringUtil.GetPureLocationString(strLocation);

            // 获得册记录的馆代码，检查是否在当前用户管辖范围内

            string strLibraryCode = "";
            // 检查一个册记录的馆藏地点是否符合馆代码列表要求
            // parameters:
            //      strLibraryCodeList  当前用户管辖的馆代码列表
            //      strLibraryCode  [out]册记录中的馆代码
            // return:
            //      -1  检查过程出错
            //      0   符合要求
            //      1   不符合要求
            nRet = CheckItemLibraryCode(itemdom,
                sessioninfo.LibraryCodeList,
                out strLibraryCode,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
            {
                strError = "盘点操作被拒绝: " + strError;
                return -1;
            }

#if NO
            string strCode = "";
            string strRoom = "";
            {
                // 解析
                ParseCalendarName(strLocation,
            out strCode,
            out strRoom);
                if (StringUtil.IsInList(strCode, sessioninfo.LibraryCodeList) == false)
                {
                    strError = "册记录的馆藏地 '" + strLocation + "' 不属于当前用户所在馆代码 '" + sessioninfo.LibraryCodeList + "' 管辖，不允许进行盘点操作。";
                    return -1;
                }
            }
#endif

            // 检查存取定义馆藏地列表
            if (string.IsNullOrEmpty(strAccessParameters) == false && strAccessParameters != "*")
            {
                string strCode = "";
                string strRoom = "";
                // 解析
                ParseCalendarName(strLocation,
            out strCode,
            out strRoom);

                bool bFound = false;
                List<string> locations = StringUtil.SplitList(strAccessParameters);
                foreach (string s in locations)
                {
                    string c = "";
                    string r = "";
                    ParseCalendarName(s,
                        out c,
                        out r);
                    if (/*string.IsNullOrEmpty(c) == false && */ c != "*")
                    {
                        if (c != strLibraryCode)
                            continue;
                    }

                    if (/*string.IsNullOrEmpty(r) == false && */ r != "*")
                    {
                        if (r != strRoom)
                            continue;
                    }

                    bFound = true;
                    break;
                }

                if (bFound == false)
                {
                    strError = "盘点操作被拒绝。因册记录的馆藏地 '" + strLocation + "' 不在当前用户存取定义规定的盘点操作的馆藏地许可范围 '" + strAccessParameters + "' 之内";
                    return -1;
                }
            }

            string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");

            string strItemRefID = DomUtil.GetElementText(itemdom.DocumentElement, "refID");

            // 在盘点库中查重
            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            List<string> aPath = null;
            // 根据馆代码、批次号和册条码号对盘点库进行查重
            // 本函数只负责查重, 并不获得记录体
            // return:
            //      -1  error
            //      其他    命中记录条数(不超过nMax规定的极限)
            nRet = SearchInventoryRecDup(
                channel,
                strLibraryCode,
                strBatchNo,
                strItemBarcode,
                strItemRefID,
                2,
                out aPath,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet >= 1)
            {
                if (string.IsNullOrEmpty(strItemBarcode) == false)
                    strError = "馆代码 '" + strLibraryCode + "' 批次号 '" + strBatchNo + "' 册条码号 '" + strItemBarcode + "' 的盘点记录已经存在，无法重复创建 ...";
                else
                    strError = "馆代码 '" + strLibraryCode + "' 批次号 '" + strBatchNo + "' 参考ID '" + strItemRefID + "' 的盘点记录已经存在，无法重复创建 ...";
                return -1;
            }

            // 在盘点库中创建一条新记录
            XmlDocument inventory_dom = new XmlDocument();
            inventory_dom.LoadXml("<root />");

            DomUtil.SetElementText(inventory_dom.DocumentElement, "itemBarcode", strItemBarcode);
            DomUtil.SetElementText(inventory_dom.DocumentElement, "itemRefID", strItemRefID);   // 册记录的参考 ID
            DomUtil.SetElementText(inventory_dom.DocumentElement, "itemRecPath", strItemRecPath);

            DomUtil.SetElementText(inventory_dom.DocumentElement, "batchNo", strBatchNo);
            DomUtil.SetElementText(inventory_dom.DocumentElement, "location", strLocation);
            DomUtil.SetElementText(inventory_dom.DocumentElement, "libraryCode", strLibraryCode);
            DomUtil.SetElementText(inventory_dom.DocumentElement, "refID", Guid.NewGuid().ToString());  // 盘点记录的参考 ID
            string strOperTime = this.Clock.GetClock();
            DomUtil.SetElementText(inventory_dom.DocumentElement, "operator",
                sessioninfo.UserID);   // 操作者
            DomUtil.SetElementText(inventory_dom.DocumentElement, "operTime",
                strOperTime);   // 操作时间

            byte[] output_timestamp = null;
            string strOutputPath = "";

            long lRet = channel.DoSaveTextRes(strInventoryDbName + "/?",
    inventory_dom.OuterXml,
    false,   // include preamble?
    "content",
    null,   // info.OldTimestamp,
    out output_timestamp,
    out strOutputPath,
    out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

#endif

        #region Return()下级函数

        // 看看新旧册记录是否有实质性改变
        // 所谓实质性改变，就是<barcode>和<borrower>两个字段的内容发生了变化
        static bool IsItemRecordSignificantChanged(XmlDocument domOld,
            XmlDocument domNew)
        {
            string strOldBarcode = DomUtil.GetElementText(domOld.DocumentElement,
                "barcode");
            string strOldBorrower = DomUtil.GetElementText(domOld.DocumentElement,
                "borrower");

            string strNewBarcode = DomUtil.GetElementText(domNew.DocumentElement,
    "barcode");
            string strNewBorrower = DomUtil.GetElementText(domNew.DocumentElement,
                "borrower");

            if (strOldBarcode != strNewBarcode)
                return true;

            if (strOldBorrower != strNewBorrower)
                return true;

            return false;
        }

        // 撤销对读者记录的借阅信息删除操作(撤销还书)
        // parameters:
        //      strReaderRecPath    读者记录路径
        //      strReaderBarcode    读者证条码号。若需要检查记录，看看里面条码号是否已经变化了，就使用这个参数。如果不想检查，就用null
        //      strItemBarcode  已经借的册条码号
        //      strDeleteBorrowFrag 被删除掉的<borrow>元素片断
        //      strAddedOverdueFrag 已经加入的<overdue>元素片断
        // return:
        //      -1  error
        //      0   没有必要Undo
        //      1   Undo成功
        int UndoReturnReaderRecord(
            RmsChannel channel,
            string strReaderRecPath,
            string strReaderBarcode,
            string strItemBarcode,
            string strDeleteBorrowFrag,
            string strAddedOverdueFrag,
            out string strError)
        {
            strError = "";
            long lRet = 0;
            int nRet = 0;

            string strMetaData = "";
            byte[] reader_timestamp = null;
            string strOutputPath = "";

            string strReaderXml = "";

            int nRedoCount = 0;

        REDO:

            lRet = channel.GetRes(strReaderRecPath,
    out strReaderXml,
    out strMetaData,
    out reader_timestamp,
    out strOutputPath,
    out strError);
            if (lRet == -1)
            {
                strError = "读出原记录 '" + strReaderRecPath + "' 时出错";
                return -1;
            }

            XmlDocument readerdom = null;
            nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载库中读者记录 '" + strReaderRecPath + "' 进入XML DOM时发生错误: " + strError;
                return -1;
            }

            // 检查读者证条码号字段 是否发生变化
            if (String.IsNullOrEmpty(strReaderBarcode) == false)
            {
                string strReaderBarcodeContent = DomUtil.GetElementText(readerdom.DocumentElement,
                    "barcode");
                if (strReaderBarcode != strReaderBarcodeContent)
                {
                    strError = "发现从数据库中读出的读者记录 '" + strReaderRecPath + "' ，其<barcode>字段内容 '" + strReaderBarcodeContent + "' 和要Undo的读者记录证条码号 '" + strReaderBarcode + "' 已不同。";
                    return -1;
                }
            }

            // 观察dom中表示借阅的节点
            XmlNode node = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']");
            if (node != null)
                return 0;   // 已经没有必要Undo了


            // 检查<borrows>元素是否存在
            XmlNode root = readerdom.DocumentElement.SelectSingleNode("borrows");
            if (root == null)
            {
                root = readerdom.CreateElement("borrows");
                root = readerdom.DocumentElement.AppendChild(root);
            }

            // 加回<borrow>元素
            XmlDocumentFragment fragment = readerdom.CreateDocumentFragment();
            fragment.InnerXml = strAddedOverdueFrag;

            root.AppendChild(fragment);


            // 删除已经加入的<overdue>元素
            {
                XmlDocument tempdom = new XmlDocument();
                tempdom.LoadXml(strAddedOverdueFrag);
                // 获得其id属性
                string strID = DomUtil.GetAttr(tempdom.DocumentElement,
                    "id");

                if (String.IsNullOrEmpty(strID) == false)
                {
                    XmlNode nodeOverdue = readerdom.DocumentElement.SelectSingleNode(
                        "overdues/overdue[@id='" + strID + "']");
                    if (nodeOverdue != null)
                        nodeOverdue.ParentNode.RemoveChild(nodeOverdue);
                }

            }

            // TODO: 删除已经加入到<borrowHistory>中的<borrow>元素？

            byte[] output_timestamp = null;
            // string strOutputPath = "";

            // 写回读者记录
            lRet = channel.DoSaveTextRes(strReaderRecPath,
                readerdom.OuterXml,
                false,
                "content",  // ,ignorechecktimestamp
                reader_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    nRedoCount++;
                    if (nRedoCount > 10)
                    {
                        strError = "写回读者记录的时候发生时间戳冲突，并且已经重试10次，仍发生错误，只好停止重试";
                        return -1;
                    }
                    goto REDO;
                }

                strError = "写回读者记录的时候发生错误" + strError;
                return -1;
            }

            return 1;   // Undo已经成功
        }

        #endregion

        // 包装版本,为了兼容脚本使用
        // return:
        //      -1  出错
        //      0   没有找到日历
        //      1   找到日历
        public int GetReaderCalendar(string strReaderType,
    out Calendar calendar,
    out string strError)
        {
            return GetReaderCalendar(strReaderType,
                "",
                out calendar,
                out strError);
        }

        // 获得日历全名
        // 特殊地，"./基本日历"，指当前馆代码的基本日历，假如当前馆代码为“海淀分馆”，则应该规范为“海淀分馆/基本日历”
        public static string GetCalendarFullName(string strName,
            string strLibraryCodeParam)
        {
            string strLibraryCode = "";
            string strPureName = "";

            // 解析日历名
            ParseCalendarName(strName,
        out strLibraryCode,
        out strPureName);

            if (strLibraryCode == ".")
            {
                if (string.IsNullOrEmpty(strLibraryCode) == true)
                    return strPureName;

                return strLibraryCodeParam + "/" + strPureName;
            }

            return strName;
        }

        // 获得和一个特定读者类型相关联的日历
        // parameters:
        //      strReaderType   读者类型。可以为空，表示匹配任何读者类型都可以
        // return:
        //      -1  出错
        //      0   没有找到日历
        //      1   找到日历
        public int GetReaderCalendar(string strReaderType,
            string strLibraryCode,
            out Calendar calendar,
            out string strError)
        {
            strError = "";
            calendar = null;

            // 获得 '工作日历名' 配置参数
            string strCalendarName = "";
            MatchResult matchresult;
            // return:
            //      reader和book类型均匹配 算4分
            //      只有reader类型匹配，算3分
            //      只有book类型匹配，算2分
            //      reader和book类型都不匹配，算1分
            int nRet = this.GetLoanParam(
                //null,
                strLibraryCode,
                strReaderType,
                "",
                "工作日历名",
                out strCalendarName,
                out matchresult,
                out strError);
            if (nRet == -1)
            {
                strError = "获得 馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 的 工作日历名 参数时发生错误: " + strError;
                return -1;
            }

            if (string.IsNullOrEmpty(strReaderType) == true)
            {
                // 只根据馆代码找一个日历，对分数要求不高
                if (nRet < 1 || string.IsNullOrEmpty(strCalendarName) == true)
                {
                    strError = "馆代码 '" + strLibraryCode + "' 中 任意读者类型 的 工作日历名 参数无法获得";
                    return 0;
                }
            }
            else
            {
                if (nRet < 3)
                {
                    strError = "馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 的 工作日历名 参数无法获得: " + strError;
                    return 0;
                }
            }

            // 特殊地，"./基本日历"，指当前馆代码的基本日历，假如当前馆代码为“海淀分馆”，则应该用“海淀分馆/基本日历”去寻找
            strCalendarName = GetCalendarFullName(strCalendarName, strLibraryCode);

            string strXPath = "";

            strXPath = "calendars/calendar[@name='" + strCalendarName + "']";
            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);

            if (nodes.Count == 0)
            {
                strError = "名为 '" + strCalendarName + "' 的日历配置不存在";
                return 0;
            }

            string strName = DomUtil.GetAttr(nodes[0], "name");
            string strData = nodes[0].InnerText;

            try
            {
                calendar = new Calendar(strName, strData);
            }
            catch (Exception ex)
            {
                strError = "日历 '" + strCalendarName + "' 的数据构造 Calendar 对象时出错: " + ex.Message;
                return -1;
            }

            return 1;
        }

        // (为管理目的)获得日历
        // 分馆用户也能看到全部日历
        // parameters:
        //      strAction   get list getcount
        public int GetCalendar(string strAction,
            string strLibraryCodeList,
            string strName,
            int nStart,
            int nCount,
            out List<CalenderInfo> contents,
            out string strError)
        {
            contents = new List<CalenderInfo>();
            strError = "";

            string strXPath = "";

#if NO
            if (strAction == "list" || strAction == "getcount")
                strXPath = "calendars/calendar";    // 列出所有
            else if (strAction == "get")
            {
                if (string.IsNullOrEmpty(strName) == false)
                    strXPath = "calendars/calendar[@name='" + strName + "']";
                else
                    strXPath = "calendars/calendar";    // 列出所有
            }
            else
            {
                strError = "不能识别的strAction参数 '" + strAction + "'";
                return -1;
            }
#endif
            // 2014/3/2
            if (string.IsNullOrEmpty(strName) == false)
                strXPath = "calendars/calendar[@name='" + strName + "']";
            else
                strXPath = "calendars/calendar";    // 列出所有

            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);

            // 仅仅需要得到数量
            if (strAction == "getcount")
                return nodes.Count;

            if (nCount == -1)
                nCount = nodes.Count - nStart;

            for (int i = nStart; i < Math.Min(nodes.Count, nStart + nCount); i++)
            {
                XmlNode node = nodes[i];

                string strCurName = DomUtil.GetAttr(node, "name");
                string strComment = DomUtil.GetAttr(node, "comment");
                string strRange = DomUtil.GetAttr(node, "range");

                CalenderInfo info = new CalenderInfo();
                info.Name = strCurName;
                info.Range = strRange;
                info.Comment = strComment;

                if (strAction == "list")
                {
                    // 不返回内容
                    contents.Add(info);
                    continue;
                }


                info.Content = node.InnerText;
                contents.Add(info);
            }

            return nodes.Count; // 返回总数
        }

        // 解析日历名
        public static void ParseCalendarName(string strName,
            out string strLibraryCode,
            out string strPureName)
        {
            strLibraryCode = "";
            strPureName = "";
            int nRet = strName.IndexOf("/");
            if (nRet == -1)
            {
                strPureName = strName;
                return;
            }
            strLibraryCode = strName.Substring(0, nRet).Trim();
            strPureName = strName.Substring(nRet + 1).Trim();
        }


        // 修改日历
        // 分馆用户只能修改自己管辖的分馆的日历
        // parameters:
        //      strAction   change new delete overwirte(2008/8/23)
        public int SetCalendar(string strAction,
            string strLibraryCodeList,
            CalenderInfo info,
            out string strError)
        {
            strError = "";

            {
                string strLibraryCode = "";
                string strPureName = "";

                // 解析日历名
                ParseCalendarName(info.Name,
            out strLibraryCode,
            out strPureName);

                // 检查日历名中馆代码。必须使用单个馆代码
                if (strLibraryCode.IndexOf(",") != -1)
                {
                    strError = "日历名中馆代码部分不允许含有逗号";
                    return -1;
                }
                // 检查日历名中馆代码。不能使用.
                if (strLibraryCode.IndexOf(".") != -1)
                {
                    strError = "日历名中馆代码部分不允许使用符号 '.' ";
                    return -1;
                }

                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {

                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                    {
                        strError = "当前用户管辖的馆代码为 '" + strLibraryCodeList + "'，不包含日历名中的馆代码 '" + strLibraryCode + "'，修改操作被拒绝";
                        return -1;
                    }
                }
            }

            string strXPath = "";

            strXPath = "calendars/calendar[@name='" + info.Name + "']";

            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);

            XmlNode node = null;

            // 2008/8/23
            if (strAction == "overwrite")
            {
                if (String.IsNullOrEmpty(info.Name) == true)
                {
                    strError = "日历名不能为空";
                    return -1;
                }

                if (nodes.Count == 0)
                {
                    XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("calendars");
                    if (root == null)
                    {
                        root = this.LibraryCfgDom.CreateElement("calendars");
                        this.LibraryCfgDom.DocumentElement.AppendChild(root);
                    }

                    node = this.LibraryCfgDom.CreateElement("calendar");
                    root.AppendChild(node);
                }
                else if (nodes.Count > 1)
                {
                    // 增强健壮性
                    for (int i = 1; i < nodes.Count; i++)
                    {
                        nodes[i].ParentNode.RemoveChild(nodes[i]);
                    }
                    node = nodes[0];

                }
                else
                {
                    Debug.Assert(nodes.Count == 1, "");
                    node = nodes[0];
                }

                DomUtil.SetAttr(node, "name", info.Name);   // 2008/10/8 增加。原来缺少本行，为一个bug
                DomUtil.SetAttr(node, "range", info.Range);
                DomUtil.SetAttr(node, "comment", info.Comment);
                node.InnerText = info.Content;
                this.Changed = true;
                return 0;
            }


            if (strAction == "change")
            {
                if (nodes.Count == 0)
                {
                    strError = "日历名 '" + info.Name + "' 不存在";
                    return -1;
                }
                if (nodes.Count > 1)
                {
                    strError = "日历名 '" + info.Name + "' 存在  " + nodes.Count.ToString() + " 个。修改操作被拒绝。";
                    return -1;
                }
                node = nodes[0];
                DomUtil.SetAttr(node, "range", info.Range);
                DomUtil.SetAttr(node, "comment", info.Comment);
                node.InnerText = info.Content;
                this.Changed = true;
                return 0;
            }

            if (strAction == "new")
            {
                if (String.IsNullOrEmpty(info.Name) == true)
                {
                    strError = "日历名不能为空";
                    return -1;
                }

                if (nodes.Count > 0)
                {
                    strError = "日历名 '" + info.Name + "' 已经存在";
                    return -1;
                }

                XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("calendars");
                if (root == null)
                {
                    root = this.LibraryCfgDom.CreateElement("calendars");
                    this.LibraryCfgDom.DocumentElement.AppendChild(root);
                }

                node = this.LibraryCfgDom.CreateElement("calendar");
                root.AppendChild(node);

                DomUtil.SetAttr(node, "name", info.Name);
                DomUtil.SetAttr(node, "range", info.Range);
                DomUtil.SetAttr(node, "comment", info.Comment);
                node.InnerText = info.Content;
                this.Changed = true;
                return 0;
            }

            if (strAction == "delete")
            {
                if (nodes.Count == 0)
                {
                    strError = "日历名 '" + info.Name + "' 不存在";
                    return -1;
                }

                for (int i = 0; i < nodes.Count; i++)
                {
                    node = nodes[i];
                    node.ParentNode.RemoveChild(node);
                }
                this.Changed = true;
                return 0;
            }

            strError = "无法识别的strAction参数值 '" + strAction + "' ";
            return -1;
        }

#if DEBUG_LOAN_PARAM
        public int GetLoanParam(
            XmlDocument cfg_dom,
            string strReaderType,
            string strBookType,
            string strParamName,
            out string strParamValue,
            out MatchResult matchresult,
            out string strError)
        {
            string strDebug = "";
            return GetLoanParam(
                cfg_dom,
                strReaderType,
                strBookType,
                strParamName,
                out strParamValue,
                out matchresult,
                out strDebug,
                out strError);
        }
#endif

        // 包装后的版本
        // 获得流通参数
        // parameters:
        //      strLibraryCode  图书馆代码, 如果为空,表示使用<library>元素以外的片段
        // return:
        //      reader和book类型均匹配 算4分
        //      只有reader类型匹配，算3分
        //      只有book类型匹配，算2分
        //      reader和book类型都不匹配，算1分
        public int GetLoanParam(
            string strLibraryCode,
            string strReaderType,
            string strBookType,
            string strParamName,
            out string strParamValue,
            out MatchResult matchresult,
#if DEBUG_LOAN_PARAM
            out string strDebug,
#endif
 out string strError)
        {
            strParamValue = "";
            strError = "";
            matchresult = MatchResult.None;

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("rightsTable");
            if (root == null)
            {
                strError = "library.xml 配置文件中尚未配置 <rightsTable> 元素";
                return -1;
            }

            return LoanParam.GetLoanParam(
                   root,    // this.LibraryCfgDom,
                   strLibraryCode,
                   strReaderType,
                   strBookType,
                   strParamName,
                    out strParamValue,
                    out matchresult,
#if DEBUG_LOAN_PARAM
                    out strDebug,
#endif
 out strError);
        }


        /*
        public List<string> GetReaderTypes()
        {
            List<string> result = new List<string>();
            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//readerTypes/item");   // 0.02以前为readertypes

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                result.Add(node.InnerText);
            }

            return result;
        }

        public List<string> GetBookTypes()
        {
            List<string> result = new List<string>();
            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//bookTypes/item");   // 0.02以前为booktypes

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                result.Add(node.InnerText);
            }

            return result;
        }
         * */

        public int SetValueTablesXml(
string strLibraryCodeList,
string strFragment,
out string strError)
        {
            return SetLibraryFragmentXml(
                "valueTables",
                strLibraryCodeList,
                strFragment,
                out strError);
        }

        // 将前端发来的权限XML代码更新到library.xml中
        public int SetRightsTableXml(
string strLibraryCodeList,
string strFragment,
out string strError)
        {
            return SetLibraryFragmentXml(
                "rightsTable",
                strLibraryCodeList,
                strFragment,
                out strError);
        }

        // 将前端发来的片断XML代码更新到library.xml中
        public int SetLibraryFragmentXml(
            string strRootElementName,
string strLibraryCodeList,
string strFragment,
out string strError)
        {
            strError = "";

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode(strRootElementName);   // 0.02前为rightstable
            if (root == null)
            {
                root = this.LibraryCfgDom.CreateElement(strRootElementName);
                this.LibraryCfgDom.DocumentElement.AppendChild(root);
            }

            XmlDocument source_dom = new XmlDocument();
            source_dom.LoadXml("<" + strRootElementName + " />");

            XmlDocumentFragment fragment = source_dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strFragment;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            source_dom.DocumentElement.AppendChild(fragment);

            // 检查所有<library>元素的code属性值
            // parameters:
            // return:
            //      -1  检查的过程出错
            //      0   没有错误
            //      1   检查后发现错误
            int nRet = CheckLibraryCodeAttr(source_dom.DocumentElement,
                strLibraryCodeList,
                out strError);
            if (nRet != 0)
                return -1;

            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
            {
                try
                {
                    root.InnerXml = strFragment;
                }
                catch (Exception ex)
                {
                    strError = "设置<" + strRootElementName + ">元素的InnerXml时发生错误: " + ex.Message;
                    return -1;
                }

                return 0;
            }
            else
            {
                // 检查是否有不属于任何<library>元素的元素
                XmlNodeList nodes = source_dom.DocumentElement.SelectNodes("descendant::*[count(ancestor-or-self::library) = 0]");
                if (nodes.Count > 0)
                {
                    strError = "当前用户的分馆用户身份不允许拟保存的<" + strRootElementName + ">代码中出现非<library>元素下级的事项元素";
                    return -1;
                }
            }

            List<string> librarycodes = StringUtil.FromListString(strLibraryCodeList);

            // 对当前用户能管辖的每个馆代码进行处理 -- 删除每个library元素
            foreach (string strLibraryCode in librarycodes)
            {
                XmlNode target = root.SelectSingleNode("descendant::library[@code='" + strLibraryCode + "']");
                if (target != null)
                {
                    target.ParentNode.RemoveChild(target);
                }
            }

            // 对当前用户能管辖的每个馆代码进行处理 -- 创建前端发来的<library>元素
            foreach (string strLibraryCode in librarycodes)
            {
                XmlNode source = source_dom.DocumentElement.SelectSingleNode("descendant::library[@code='" + strLibraryCode + "']");
                if (source == null)
                    continue;   // 源没有这个元素

                Debug.Assert(source != null, "");


                XmlNode target = root.OwnerDocument.CreateElement("library");
                root.AppendChild(target);
                DomUtil.SetAttr(target, "code", strLibraryCode);

                target.InnerXml = source.InnerXml;
            }

            return 0;
        }

        public int GetValueTablesXml(
string strLibraryCodeList,
out string strValue,
out string strError)
        {
            return GetiLibraryFragmentXml(
                "valueTables",
                strLibraryCodeList,
                out strValue,
                out strError);
        }

        public int GetRightsTableXml(
string strLibraryCodeList,
out string strValue,
out string strError)
        {
            return GetiLibraryFragmentXml(
                "rightsTable",
                strLibraryCodeList,
                out strValue,
                out strError);
        }

        public int GetiLibraryFragmentXml(
            string strRootElementName,
            string strLibraryCodeList,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode(strRootElementName);   // 0.02前为rightstable
            if (root == null)
                return 0;
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
            {
                strValue = root.InnerXml;
                return 0;
            }

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml("<" + strRootElementName + " />");

            List<string> librarycodes = StringUtil.FromListString(strLibraryCodeList);
            foreach (string strLibraryCode in librarycodes)
            {
                XmlNode source = root.SelectSingleNode("descendant::library[@code='" + strLibraryCode + "']");
                if (source == null)
                    continue;

                XmlNode target = domNew.CreateElement("library");
                domNew.DocumentElement.AppendChild(target);
                DomUtil.SetAttr(target, "code", strLibraryCode);

                target.InnerXml = source.InnerXml;
            }

            strValue = domNew.DocumentElement.InnerXml;
            return 0;
        }

        // 检查所有<library>元素的code属性值
        // parameters:
        // return:
        //      -1  检查的过程出错
        //      0   没有错误
        //      1   检查后发现错误
        public static int CheckLibraryCodeAttr(XmlNode root,
            string strLibraryCodeList,
            out string strError)
        {
            strError = "";

            List<string> all_librarycodes = new List<string>();
            XmlNodeList nodes = root.SelectNodes("descendant::library");
            foreach (XmlNode node in nodes)
            {
                string strCode = DomUtil.GetAttr(node, "code");
                if (string.IsNullOrEmpty(strCode) == true)
                    continue;
                if (strCode.IndexOf(" ") != -1)
                {
                    strError = "<library>元素的code属性值 '" + strCode + "' 中不应包含空格字符";
                    return 1;
                }
                if (strCode.IndexOf(",") != -1)
                {
                    strError = "<library>元素的code属性值 '" + strCode + "' 中不应包含逗号字符";
                    return 1;
                }
                if (strCode.IndexOf("*") != -1)
                {
                    strError = "<library>元素的code属性值 '" + strCode + "' 中不应包含星号字符";
                    return 1;
                }

                all_librarycodes.Add(strCode);
            }

            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
            {
                List<string> range = StringUtil.FromListString(strLibraryCodeList);
                // 观察all_librarycodes集合是否超过strLibraryCodeList范围
                foreach (string strCode in all_librarycodes)
                {
                    if (range.IndexOf(strCode) == -1)
                    {
                        strError = "<library>元素的code属性值 '" + strCode + "' 超出范围 '" + strLibraryCodeList + "'，这是不允许的";
                        return 1;
                    }
                }
            }

            return 0;
        }

        // 取连个集合的交叉部分
        static List<string> AND(List<string> list1, List<string> list2)
        {
            List<string> result = new List<string>();
            foreach (string s in list1)
            {
                if (list2.IndexOf(s) != -1)
                    result.Add(s);
            }

            return result;
        }

        // 将新旧两组<location>元素按照name属性进行碰撞，得出三个集合
        // parameters:
        //      create_nodes    [out]打算新增的节点 (来自 new_nodes)
        //      delete_nodes    [out]打算删除的节点 (来自 old_nodes)
        //      remain_nodes    [out]新旧之间共同的节点 (来自 old_nodes)
        static int GetThreeLocationCollections(XmlNode new_root,
            XmlNode old_root,
            out List<XmlNode> create_nodes,
            out List<XmlNode> delete_nodes,
            out List<XmlNode> remain_nodes,
            out string strError)
        {
            strError = "";

            create_nodes = new List<XmlNode>();
            delete_nodes = new List<XmlNode>();
            remain_nodes = new List<XmlNode>();

            XmlNodeList new_nodes = new_root.SelectNodes("*");
            XmlNodeList old_nodes = old_root.SelectNodes("*");

            if (new_nodes.Count == 0)
            {
                foreach (XmlNode node in old_nodes)
                {
                    delete_nodes.Add(node);
                }
                return 0;
            }

            if (old_nodes.Count == 0)
            {
                foreach (XmlNode node in new_nodes)
                {
                    create_nodes.Add(node);
                }
                return 0;
            }

            List<string> old_names = new List<string>();
            List<string> new_names = new List<string>();


            foreach (XmlNode node in old_nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                /*
                if (string.IsNullOrEmpty(strName) == true)
                {
                    strError = "馆藏地点名不能为空。'" + node.OuterXml + "'";
                    return -1;
                }
                 * */
                if (old_names.IndexOf(strName) != -1)
                {
                    strError = "馆藏地点名 '" + strName + "' 不应重复使用";
                    return -1;
                }
                old_names.Add(strName);
            }

            foreach (XmlNode node in new_nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                /*
                if (string.IsNullOrEmpty(strName) == true)
                {
                    strError = "馆藏地点名不能为空。'" + node.OuterXml + "'";
                    return -1;
                }
                 * */
                if (new_names.IndexOf(strName) != -1)
                {
                    strError = "馆藏地点名 '" + strName + "' 不应重复使用";
                    return -1;
                }
                new_names.Add(strName);
            }

            // 公共部分
            List<string> common_names = AND(old_names, new_names);

            foreach (string strName in common_names)
            {
                XmlNode node = old_root.SelectSingleNode("location[@name='" + strName + "']");
                if (node == null)
                {
                    strError = "很奇怪 old_root 下没有找到 name 属性为 '" + strName + "' 的<location>元素";
                    return -1;
                }
                remain_nodes.Add(node);
            }

            // 要创建的部分
            foreach (XmlNode node in new_nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                if (common_names.IndexOf(strName) == -1)
                {
                    create_nodes.Add(node);
                }
            }

            // 要删除的部分
            foreach (XmlNode node in old_nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                if (common_names.IndexOf(strName) == -1)
                {
                    delete_nodes.Add(node);
                }
            }
            return 0;
        }

        // 将新旧两组<group>元素按照name属性进行碰撞，得出三个集合
        // parameters:
        //      create_nodes    [out]打算新增的节点 (来自 new_nodes)
        //      delete_nodes    [out]打算删除的节点 (来自 old_nodes)
        //      remain_nodes    [out]新旧之间共同的节点 (来自 old_nodes)
        static int GetThreeGroupCollections(XmlNode new_root,
            XmlNode old_root,
            out List<XmlNode> create_nodes,
            out List<XmlNode> delete_nodes,
            out List<XmlNode> remain_nodes,
            out string strError)
        {
            strError = "";

            create_nodes = new List<XmlNode>();
            delete_nodes = new List<XmlNode>();
            remain_nodes = new List<XmlNode>();

            XmlNodeList new_nodes = new_root.SelectNodes("*");
            XmlNodeList old_nodes = old_root.SelectNodes("*");

            if (new_nodes.Count == 0)
            {
                foreach (XmlNode node in old_nodes)
                {
                    delete_nodes.Add(node);
                }
                return 0;
            }

            if (old_nodes.Count == 0)
            {
                foreach (XmlNode node in new_nodes)
                {
                    create_nodes.Add(node);
                }
                return 0;
            }

            List<string> old_names = new List<string>();
            List<string> new_names = new List<string>();


            foreach (XmlNode node in old_nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                if (string.IsNullOrEmpty(strName) == true)
                {
                    strError = "排架体系名不能为空。'" + node.OuterXml + "'";
                    return -1;
                }
                if (old_names.IndexOf(strName) != -1)
                {
                    strError = "排架体系名 '" + strName + "' 不应重复使用";
                    return -1;
                }
                old_names.Add(strName);
            }

            foreach (XmlNode node in new_nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                if (string.IsNullOrEmpty(strName) == true)
                {
                    strError = "排架体系名不能为空。'" + node.OuterXml + "'";
                    return -1;
                }
                if (new_names.IndexOf(strName) != -1)
                {
                    strError = "排架体系名 '" + strName + "' 不应重复使用";
                    return -1;
                }
                new_names.Add(strName);
            }

            // 公共部分
            List<string> common_names = AND(old_names, new_names);

            foreach (string strName in common_names)
            {
                XmlNode node = old_root.SelectSingleNode("group[@name='" + strName + "']");
                if (node == null)
                {
                    strError = "很奇怪 old_root 下没有找到 name 属性为 '" + strName + "' 的<group>元素";
                    return -1;
                }
                remain_nodes.Add(node);
            }

            // 要创建的部分
            foreach (XmlNode node in new_nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                if (common_names.IndexOf(strName) == -1)
                {
                    create_nodes.Add(node);
                }
            }

            // 要删除的部分
            foreach (XmlNode node in old_nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                if (common_names.IndexOf(strName) == -1)
                {
                    delete_nodes.Add(node);
                }
            }
            return 0;
        }

        // 观察两个XmlNode的属性是否完全一致
        static bool AttrEqual(XmlNode node1, XmlNode node2)
        {
            if (node1.Attributes.Count != node2.Attributes.Count)
                return false;

            List<String> attrs1 = new List<string>();
            List<string> attrs2 = new List<string>();

            foreach (XmlAttribute attr in node1.Attributes)
            {
                attrs1.Add(attr.Name + "=" + attr.Value);
            }

            foreach (XmlAttribute attr in node2.Attributes)
            {
                attrs2.Add(attr.Name + "=" + attr.Value);
            }

            Debug.Assert(attrs1.Count == attrs2.Count, "");

            attrs1.Sort();
            attrs2.Sort();

            for (int i = 0; i < attrs1.Count; i++)
            {
                if (attrs1[i] != attrs2[i])
                    return false;
            }

            return true;
        }

        // 修改 <callNumber> 元素定义。本函数专用于分馆用户。全局用户可以直接修改这个元素的 InnerXml 即可
        public int SetCallNumberXml(
string strLibraryCodeList,
string strFragment,
out string strError)
        {
            strError = "";

            XmlDocument source_dom = new XmlDocument();
            source_dom.LoadXml("<root />");

            XmlDocumentFragment fragment = source_dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strFragment;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            source_dom.DocumentElement.AppendChild(fragment);
            XmlNode source_root = source_dom.DocumentElement;

            XmlNode exist_root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("callNumber");
            if (exist_root == null)
            {
                exist_root = this.LibraryCfgDom.CreateElement("callNumber");
                this.LibraryCfgDom.DocumentElement.AppendChild(exist_root);
                this.Changed = true;
            }

            // 分别处理三类动作：
            // 1) 增加了新的<group>元素
            // 2) 删除了原有的<group>元素
            // 3) 修改了原有的<group>元素

            // 将新旧两组<group>元素按照name属性进行碰撞，得出三个集合

            List<XmlNode> create_group_nodes = null;
            List<XmlNode> delete_group_nodes = null;
            List<XmlNode> remain_group_nodes = null;

            // parameters:
            //      create_nodes    [out]打算新增的节点 (来自 new_nodes)
            //      delete_nodes    [out]打算删除的节点 (来自 old_nodes)
            //      remain_nodes    [out]新旧之间共同的节点 (来自 old_nodes)
            int nRet = GetThreeGroupCollections(source_root,
                exist_root,
                out create_group_nodes,
                out delete_group_nodes,
                out remain_group_nodes,
                out strError);

            // 观察打算新创建的<group>元素，是否下属的<location>元素都是当前用户管辖范围内的馆藏地点名称
            foreach (XmlNode group_node in create_group_nodes)
            {
                XmlNodeList location_nodes = group_node.SelectNodes("location");
                foreach (XmlNode location in location_nodes)
                {
                    string strLocationName = DomUtil.GetAttr(location, "name");

                    string strLibraryCode = "";
                    string strPureName = "";

                    // 解析
                    ParseCalendarName(strLocationName,
                out strLibraryCode,
                out strPureName);

                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                    {
                        strError = "要新增的name属性值为 '" + DomUtil.GetAttr(group_node, "name") + "' 的<group>元素因其下属的<location>元素name属性中的馆藏地点 '" + strLocationName + "' 不在当前用户管辖范围 '" + strLibraryCodeList + "' 内，修改<callNumber>定义操作被拒绝";
                        return -1;
                    }
                }
            }

            // 观察打算删除的<group>元素，是否下属的<location>元素都是当前用户管辖范围内的馆藏地点名称
            foreach (XmlNode group_node in delete_group_nodes)
            {
                XmlNodeList location_nodes = group_node.SelectNodes("location");
                foreach (XmlNode location in location_nodes)
                {
                    string strLocationName = DomUtil.GetAttr(location, "name");

                    string strLibraryCode = "";
                    string strPureName = "";

                    // 解析
                    ParseCalendarName(strLocationName,
                out strLibraryCode,
                out strPureName);

                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                    {
                        strError = "要删除的name属性值为 '" + DomUtil.GetAttr(group_node, "name") + "' 的<group>元素因其下属的<location>元素name属性中的馆藏地点 '" + strLocationName + "' 不在当前用户管辖范围 '" + strLibraryCodeList + "' 内，修改<callNumber>定义操作被拒绝";
                        return -1;
                    }
                }
            }

            // 观察打算修改的每个<group>元素，其中下属增加的<location>元素和删除的<location>元素都必须是在当前用户的管辖范围内
            // 另外如果要修改<group>元素本身的除了name以外的任何一个属性，都要求下属的<location>全部在当前用户的管辖范围内才行
            foreach (XmlNode group_node in remain_group_nodes)
            {
                // 注意 node 来自 old_nodes 集合
                string strGroupName = DomUtil.GetAttr(group_node, "name");

                XmlNode new_group = source_root.SelectSingleNode("group[@name='" + strGroupName + "']");
                if (new_group == null)
                {
                    strError = "name属性值为 '" + strGroupName + "' 的<group>元素在新提交的<callNumber> XML片断中居然没有找到";
                    return -1;
                }

                XmlNode old_group = exist_root.SelectSingleNode("group[@name='" + strGroupName + "']");
                if (old_group == null)
                {
                    strError = "name属性值为 '" + strGroupName + "' 的<group>元素在原有的<callNumber> XML片断中居然没有找到";
                    return -1;
                }

                List<XmlNode> create_location_nodes = null;
                List<XmlNode> delete_location_nodes = null;
                List<XmlNode> remain_location_nodes = null;

                // 将新旧两组<location>元素按照name属性进行碰撞，得出三个集合
                // parameters:
                //      create_nodes    [out]打算新增的节点 (来自 new_nodes)
                //      delete_nodes    [out]打算删除的节点 (来自 old_nodes)
                //      remain_nodes    [out]新旧之间共同的节点 (来自 old_nodes)
                nRet = GetThreeLocationCollections(new_group,
                    old_group,
                    out create_location_nodes,
                    out delete_location_nodes,
                    out remain_location_nodes,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 观察打算新创建的<location>元素，是否都是当前用户管辖范围内的馆藏地点名称
                foreach (XmlNode location in create_location_nodes)
                {
                    string strLocationName = DomUtil.GetAttr(location, "name");

                    string strLibraryCode = "";
                    string strPureName = "";

                    // 解析
                    ParseCalendarName(strLocationName,
                out strLibraryCode,
                out strPureName);

                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                    {
                        strError = "name属性值为 '" + strGroupName + "' 的<group>元素下，拟新增的<location>元素name属性值中的馆藏地点 '" + strLocationName + "' 不在当前用户管辖范围 '" + strLibraryCodeList + "' 内，修改<callNumber>定义操作被拒绝";
                        return -1;
                    }
                }


                // 观察打算删除的<location>元素，是否都是当前用户管辖范围内的馆藏地点名称
                foreach (XmlNode location in delete_location_nodes)
                {
                    string strLocationName = DomUtil.GetAttr(location, "name");

                    string strLibraryCode = "";
                    string strPureName = "";

                    // 解析
                    ParseCalendarName(strLocationName,
                out strLibraryCode,
                out strPureName);

                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                    {
                        strError = "name属性值为 '" + strGroupName + "' 的<group>元素下，拟删除的原有<location>元素name属性值中的馆藏地点 '" + strLocationName + "' 不在当前用户管辖范围 '" + strLibraryCodeList + "' 内，修改<callNumber>定义操作被拒绝";
                        return -1;
                    }
                }

                // 观察<group>元素本身的属性修改情况
                if (AttrEqual(old_group, new_group) == false)
                {
                    // new_root下所有<location>元素，必须都在当前用户的管辖范围内
                    XmlNodeList locations = new_group.SelectNodes("location");
                    foreach (XmlNode location in locations)
                    {
                        string strLocationName = DomUtil.GetAttr(location, "name");

                        string strLibraryCode = "";
                        string strPureName = "";

                        // 解析
                        ParseCalendarName(strLocationName,
                    out strLibraryCode,
                    out strPureName);

                        if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                        {
                            strError = "name属性值为 '" + strGroupName + "' 的<group>元素，若其属性发生了修改，这要求其下的所有<location>元素应在当前用户的管辖范围内。但发现这个<group>元素其下的<location>元素name属性值中的馆藏地点 '" + strLocationName + "' 不在当前用户管辖范围 '" + strLibraryCodeList + "' 内，修改<callNumber>定义操作被拒绝";
                            return -1;
                        }

                    }
                }
            }

            // 若没有问题了，兑现修改
            exist_root.InnerXml = source_root.InnerXml;
            this.Changed = true;
            return 0;
        }

        public int SetLocationTypesXml(
    string strLibraryCodeList,
    string strFragment,
    out string strError)
        {
            strError = "";

            XmlDocument source_dom = new XmlDocument();
            source_dom.LoadXml("<root />");

            XmlDocumentFragment fragment = source_dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strFragment;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            source_dom.DocumentElement.AppendChild(fragment);

            // 检查所有<library>元素的code属性值
            // parameters:
            // return:
            //      -1  检查的过程出错
            //      0   没有错误
            //      1   检查后发现错误
            int nRet = CheckLibraryCodeAttr(source_dom.DocumentElement,
                strLibraryCodeList,
                out strError);
            if (nRet != 0)
                return -1;

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("locationTypes");
            if (root == null)
            {
                root = this.LibraryCfgDom.CreateElement("locationTypes");
                this.LibraryCfgDom.DocumentElement.AppendChild(root);
                this.Changed = true;
            }
            // 把当前用户能管辖的全部已有片断删除，然后一个一个插入
            // 注意，list为空或者"*"，管辖全部内容
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
            {
                // root.RemoveAll();

                root.InnerXml = source_dom.DocumentElement.InnerXml;
                this.Changed = true;
                return 0;
            }
            else
            {
                // 检查是否有不属于任何<library>元素的元素
                XmlNodeList nodes = source_dom.DocumentElement.SelectNodes("descendant::*[count(ancestor-or-self::library) = 0]");
                if (nodes.Count > 0)
                {
                    strError = "当前用户的分馆用户身份不允许拟保存的<locationTypes>代码中出现非<library>元素下级的事项元素";
                    return -1;
                }

                List<string> librarycodes = StringUtil.FromListString(strLibraryCodeList);
                foreach (string strLibraryCode in librarycodes)
                {
                    XmlNode node = root.SelectSingleNode("library[@code='" + strLibraryCode + "']");
                    if (node != null)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }

            // 第一级<item>插入
            {

            }

            // 一个一个<library>元素地插入
            {
                List<string> librarycodes = StringUtil.FromListString(strLibraryCodeList);
                foreach (string strLibraryCode in librarycodes)
                {
                    XmlNodeList nodes = source_dom.DocumentElement.SelectNodes("library[@code='" + strLibraryCode + "']");
                    foreach (XmlNode node in nodes)
                    {
                        XmlNode new_node = this.LibraryCfgDom.CreateElement("library");
                        root.AppendChild(new_node);
                        DomUtil.SetAttr(new_node, "code", strLibraryCode);
                        new_node.InnerXml = node.InnerXml;
                    }
                }
                this.Changed = true;
            }

            return 0;
        }

        // 按照馆代码列表，返回<locationTypes>内的适当片断
        public int GetLocationTypesXml(
            string strLibraryCodeList,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";
#if NO
            XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("locationTypes"); // 0.02前为locationtypes
            if (root == null)
            {
                nRet = 0;
                goto END1;
            }

            strValue = root.InnerXml;
#endif
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
            {
                XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("locationTypes"); // 0.02前为locationtypes
                if (root == null)
                    return 0;
                strValue = root.InnerXml;

                return 0;
            }

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml("<locationTypes />");

            List<string> librarycodes = StringUtil.FromListString(strLibraryCodeList);
            foreach (string strLibraryCode in librarycodes)
            {
                string strXPath = "//locationTypes/library[@code='" + strLibraryCode + "']";
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);

                foreach (XmlNode node in nodes)
                {
                    XmlDocumentFragment fragment = domNew.CreateDocumentFragment();
                    fragment.InnerXml = node.OuterXml;

                    domNew.DocumentElement.AppendChild(fragment);
                }
            }

#if NO
            // 兼容以前的习惯。把第一级的<item>元素也返回
            if (string.IsNullOrEmpty(strLibraryCodeList) == true
                || strLibraryCodeList == "*")
            {
                string strXPath = "//locationTypes/item";
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);
                foreach (XmlNode node in nodes)
                {
                    XmlDocumentFragment fragment = domNew.CreateDocumentFragment();
                    fragment.InnerXml = node.OuterXml;

                    domNew.DocumentElement.AppendChild(fragment);
                }
            }
#endif

            strValue = domNew.DocumentElement.InnerXml;
            return 0;
        }

        // 获得不允许外借的馆藏地点类型列表
        // parameters:
        //      strLibraryCode  一个图书馆代码
        //`return:
        //      纯粹的馆藏地点名字符串数组。所谓纯粹，就是“馆代码/地点名”中的地点名部分
        public List<string> GetCantBorrowLocationTypes(string strLibraryCode)
        {
            List<string> result = new List<string>();
            string strXPath = "//locationTypes/library[@code='" + strLibraryCode + "']/item";

            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);
            foreach (XmlElement item in nodes)
            {
                if (DomUtil.IsBooleanTrue(item.GetAttribute("canborrow"), false) == false)
                    result.Add(item.InnerText.Trim());
            }

            // 兼容原来的习惯。找到那些不属于<library>元素后代的<item>元素
            if (string.IsNullOrEmpty(strLibraryCode) == true)
            {
                strXPath = "//locationTypes/item[count(ancestor::library) = 0]";
                nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);
                foreach (XmlElement item in nodes)
                {
                    if (DomUtil.IsBooleanTrue(item.GetAttribute("canborrow"), false) == false)
                        result.Add(item.InnerText.Trim());
                }
            }

            return result;
        }

        // 2015/6/14 改造，采用先获得集合然后筛选的方法
        // 获得馆藏地点类型列表
        // parameters:
        //      strLibraryCode  一个图书馆代码
        //      bOnlyCanBorrow  是仅仅列出canborrow属性为'yes'的<item>事项
        // return:
        //      纯粹的馆藏地点名字符串数组。所谓纯粹，就是“馆代码/地点名”中的地点名部分
        public List<string> GetLocationTypes(string strLibraryCode,
            bool bOnlyCanBorrow)
        {
            List<string> result = new List<string>();
            string strXPath = "";
#if NO
            if (bOnlyCanBorrow == true)
            {
                // 这里有个问题，canborrow 属性的值不一定是 yes no，可能是其他
                strXPath = "//locationTypes/library[@code='" + strLibraryCode + "']/item[@canborrow='yes']";
            }
            else
#endif
            strXPath = "//locationTypes/library[@code='" + strLibraryCode + "']/item";

            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);
            foreach (XmlElement item in nodes)
            {
                if (bOnlyCanBorrow == false || DomUtil.IsBooleanTrue(item.GetAttribute("canborrow"), false) == true)
                    result.Add(item.InnerText.Trim());
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                result.Add(node.InnerText);
            }

            // 兼容原来的习惯。找到那些不属于<library>元素后代的<item>元素
            if (string.IsNullOrEmpty(strLibraryCode) == true)
            {
                strXPath = "";
#if NO
                if (bOnlyCanBorrow == true)
                    strXPath = "//locationTypes/item[@canborrow='yes'][count(ancestor::library) = 0]";
                else
#endif
                strXPath = "//locationTypes/item[count(ancestor::library) = 0]";
                nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);
                foreach (XmlElement item in nodes)
                {
                    if (bOnlyCanBorrow == false || DomUtil.IsBooleanTrue(item.GetAttribute("canborrow"), false) == true)
                        result.Add(item.InnerText.Trim());
                }
            }

            return result;
        }

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.LibraryServer.res.LibraryApplication",
                typeof(LibraryApplication).Module.Assembly);

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
            catch (Exception /*ex*/)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }

        // 检查超期情况。
        // return:
        //      -1  数据格式错误
        //      0   没有发现超期    strError也有未来到期的提示信息
        //      1   发现超期   strError中有提示信息
        public int CheckPeriod(
            Calendar calendar,
            string strBorrowDate,
            string strPeriod,
            out string strError)
        {
            long lOver = 0;
            string strPeriodUnit = "";

            return CheckPeriod(
                calendar,
                strBorrowDate,
                strPeriod,
                out lOver,
                out strPeriodUnit,
                out strError);
        }


        // 检查超期情况。但不生成记载信息。本模块用于借书前的例行检查（而不是用于还书）。
        // return:
        //      -1  数据格式错误
        //      0   没有发现超期
        //      1   发现超期   strError中有提示信息
        //      2   已经在宽限期内，很容易超期 2009/3/13
        public int CheckPeriod(
            Calendar calendar,
            string strBorrowDate,
            string strPeriod,
            out long lOver,
            out string strPeriodUnit,
            out string strError)
        {
            DateTime borrowdate;
            lOver = 0;
            strPeriodUnit = "";

            LibraryApplication app = this;

            try
            {
                borrowdate = DateTimeUtil.FromRfc1123DateTimeString(strBorrowDate);
            }
            catch
            {
                // text-level: 内部错误
                strError = string.Format(this.GetString("借阅日期值s格式错误"), // "借阅日期值 '{0}' 格式错误"
                    strBorrowDate);

                // "借阅日期值 '" + strBorrowDate + "' 格式错误";
                return -1;
            }

            // 解析期限值
            // string strPeriodUnit = "";
            long lPeriodValue = 0;

            int nRet = ParsePeriodUnit(strPeriod,
                out lPeriodValue,
                out strPeriodUnit,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = string.Format(this.GetString("借阅期限值s格式错误s"),    // "借阅期限 值 '{0}' 格式错误: {1}" 
                    strPeriod,
                    strError);
                // "借阅期限 值 '" + strPeriod + "' 格式错误: " + strError;
                return -1;
            }

            DateTime timeEnd = DateTime.MinValue;   // 还书最后期限
            DateTime nextWorkingDay = DateTime.MinValue;   // 如果还书最后期限正好在一个非工作日上，那么这是其下一个工作日

            // 测算还书日期
            // parameters:
            //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
            // return:
            //      -1  出错
            //      0   成功。timeEnd在工作日范围内。
            //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
            nRet = GetReturnDay(
                calendar,
                borrowdate,
                lPeriodValue,
                strPeriodUnit,
                out timeEnd,
                out nextWorkingDay,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "测算还书时间过程发生错误: " + strError;
                return -1;
            }
            bool bEndInNonWorkingDay = false;
            if (nRet == 1)
            {
                // now在非工作日
                bEndInNonWorkingDay = true;
            }

            DateTime now_rounded = app.Clock.UtcNow;  //  今天

            // 正规化时间
            nRet = RoundTime(strPeriodUnit,
                ref now_rounded,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta = now_rounded - timeEnd;

            long lDelta = 0;
            long lDelta1 = 0;   // 校正（考虑工作日）后的差额

            nRet = ParseTimeSpan(
                delta,
                strPeriodUnit,
                out lDelta,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta1 = new TimeSpan(0);
            if (bEndInNonWorkingDay == true)
            {
                delta1 = now_rounded - nextWorkingDay;

                nRet = ParseTimeSpan(
    delta1,
    strPeriodUnit,
    out lDelta1,
    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                delta1 = delta;
                lDelta1 = lDelta;
            }


            strError = "";

            if (lDelta1 > 0)
            {
                if (bEndInNonWorkingDay == true)
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("已超过借阅期限多少天"), // "已超过借阅期限 ({0}) {1} {2}。",
                        timeEnd.ToLongDateString(),
                        Convert.ToString(lDelta1),
                        GetDisplayTimeUnitLang(strPeriodUnit));
                    // 反正已经超期，最后一天是不是在非工作日就没有必要提醒了

                    // "已超过借阅期限 (" + timeEnd.ToLongDateString() + ") " + Convert.ToString(lDelta1) + GetDisplayTimeUnit(strPeriodUnit) + "。";
                    lOver = lDelta1;    // 2009/8/5
                    return 1;
                }
                else
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("已超过借阅期限多少天"), // "已超过借阅期限 ({0}) {1} {2}。",
                        timeEnd.ToLongDateString(),
                        Convert.ToString(lDelta),
                        GetDisplayTimeUnitLang(strPeriodUnit));

                    // "已超过借阅期限 (" + timeEnd.ToLongDateString() + ") " + Convert.ToString(lDelta) + GetDisplayTimeUnit(strPeriodUnit) + "。";
                    lOver = lDelta;    // 2009/8/5
                    return 1;
                }
            }

            if (lDelta == 0 || lDelta1 == 0)
            {
                if (strPeriodUnit == "day")
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("今天就是还书的最后期限"), // "今天就是还书的最后期限 ({0})。"
                        timeEnd.ToLongDateString());
                    // "今天就是还书的最后期限 (" + timeEnd.ToLongDateString() + ")。";
                }
                else if (strPeriodUnit == "hour")
                {
                    // text-level: 用户提示
                    strError += this.GetString("当前这个小时就是还书的最后期限");
                    // "当前这个小时就是还书的最后期限。";
                }
                else
                {
                    // text-level: 用户提示
                    strError += this.GetString("现在就是还书的最后期限");
                    // "现在就是还书的最后期限。";
                }

                if (bEndInNonWorkingDay && lDelta1 < 0)
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("但根据"),
                        calendar.Name,
                        now_rounded.ToLongDateString(),
                        nextWorkingDay.ToLongDateString());
                    // "但根据 {0} 显示，今天({1})是非工作日，您可以在最近第一个工作日({2})去图书馆还书。"

                    // "但根据 '" + calendar.Name + "' 显示，今天(" + now_rounded.ToLongDateString() + ")是非工作日，您可以在最近第一个工作日(" + nextWorkingDay.ToLongDateString() + ")去图书馆还书。";
                }

                lOver = 0;    // 2009/8/5
            }
            else
            {
                Debug.Assert(lDelta1 < 0, "");

                bool bOverdue = false;
                // 理论上已经超过最后期限，但是还在宽限期以内
                if (lDelta > 0)
                {
                    Debug.Assert(bEndInNonWorkingDay == true, "");

                    // text-level: 用户提示
                    strError += string.Format(this.GetString("本已超过借阅期限"),
                        timeEnd.ToLongDateString(),
                        Convert.ToString(lDelta),
                        GetDisplayTimeUnitLang(strPeriodUnit));

                    // "本已超过借阅期限 ({0}) {1}{2}。但";

                    // "本已超过借阅期限 (" + timeEnd.ToLongDateString() + ") " + Convert.ToString(lDelta) + GetDisplayTimeUnit(strPeriodUnit) + "。但";
                    bOverdue = true;

                    lOver = lDelta;    // 2009/8/5
                }
                else
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("距最后期限还有"),
                        timeEnd.ToLongDateString(),
                        Convert.ToString(-lDelta),  // lDelta1 BUG!!!
                        GetDisplayTimeUnitLang(strPeriodUnit));
                    // "距最后期限 ({0}) 还有 {1}{2}。";

                    // "距最后期限 (" + timeEnd.ToLongDateString() + ") 还有 " + Convert.ToString(-lDelta1) + GetDisplayTimeUnit(strPeriodUnit) + "。";

                    lOver = lDelta1;    // 2009/8/5
                }

                if (bEndInNonWorkingDay == true)
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("根据得知"),
                        calendar.Name,
                        timeEnd.ToLongDateString(),
                        nextWorkingDay.ToLongDateString());
                    // "根据 '{0}' 得知，还书截止日 ({1}) 恰逢图书馆非工作日，您可以选择最迟在截止日后的第一个工作日 ({2}) 去图书馆还书。";

                    // "根据 '" + calendar.Name + "' 得知，还书截止日 (" + timeEnd.ToLongDateString() + ") 恰逢图书馆非工作日，您可以选择最迟在截止日后的第一个工作日 (" + nextWorkingDay.ToLongDateString() + ") 去图书馆还书。";
                }

                if (bOverdue == true)
                {
                    strError += "";
                    return 2;
                }
            }

            return 0;
        }

        // 获得还书日期
        // return:
        //      -1  数据格式错误
        //      0   没有发现超期
        //      1   发现超期   strError中有提示信息
        //      2   已经在宽限期内，很容易超期 
        public int GetReturningTime(
            Calendar calendar,
            string strBorrowDate,
            string strPeriod,
            out DateTime timeReturning,
            out DateTime timeNextWorkingDay,
            out long lOver,
            out string strPeriodUnit,
            out string strError)
        {
            DateTime borrowdate;
            lOver = 0;
            strPeriodUnit = "";

            timeReturning = DateTime.MinValue;   // 还书最后期限
            timeNextWorkingDay = DateTime.MinValue;   // 如果还书最后期限正好在一个非工作日上，那么这是其下一个工作日


            LibraryApplication app = this;

            try
            {
                // 借阅开始日，GMT时间
                borrowdate = DateTimeUtil.FromRfc1123DateTimeString(strBorrowDate);
            }
            catch
            {
                // text-level: 内部错误
                strError = string.Format(this.GetString("借阅日期值s格式错误"), // "借阅日期值 '{0}' 格式错误"
                    strBorrowDate);

                // "借阅日期值 '" + strBorrowDate + "' 格式错误";
                return -1;
            }

            // 解析期限值
            // string strPeriodUnit = "";
            long lPeriodValue = 0;

            int nRet = ParsePeriodUnit(strPeriod,
                out lPeriodValue,
                out strPeriodUnit,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = string.Format(this.GetString("借阅期限值s格式错误s"),    // "借阅期限 值 '{0}' 格式错误: {1}" 
                    strPeriod,
                    strError);
                // "借阅期限 值 '" + strPeriod + "' 格式错误: " + strError;
                return -1;
            }


            // 测算还书日期
            // parameters:
            //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
            // return:
            //      -1  出错
            //      0   成功。timeEnd在工作日范围内。
            //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
            nRet = GetReturnDay(
                calendar,
                borrowdate,
                lPeriodValue,
                strPeriodUnit,
                out timeReturning,
                out timeNextWorkingDay,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "测算还书时间过程发生错误: " + strError;
                return -1;
            }
            bool bEndInNonWorkingDay = false;
            if (nRet == 1)
            {
                // now在非工作日
                bEndInNonWorkingDay = true;
            }

            DateTime now_rounded = app.Clock.UtcNow;  //  今天

            // 正规化时间
            nRet = RoundTime(strPeriodUnit,
                ref now_rounded,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta = now_rounded - timeReturning;

            long lDelta = 0;
            long lDelta1 = 0;   // 校正（考虑工作日）后的差额

            nRet = ParseTimeSpan(
                delta,
                strPeriodUnit,
                out lDelta,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta1 = new TimeSpan(0);
            if (bEndInNonWorkingDay == true)
            {
                delta1 = now_rounded - timeNextWorkingDay;

                nRet = ParseTimeSpan(
    delta1,
    strPeriodUnit,
    out lDelta1,
    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                delta1 = delta;
                lDelta1 = lDelta;
            }


            strError = "";

            if (lDelta1 > 0)
            {
                if (bEndInNonWorkingDay == true)
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("已超过借阅期限多少天"), // "已超过借阅期限 ({0}) {1} {2}。",
                        timeReturning.ToLongDateString(),
                        Convert.ToString(lDelta1),
                        GetDisplayTimeUnitLang(strPeriodUnit));
                    // 反正已经超期，最后一天是不是在非工作日就没有必要提醒了

                    // "已超过借阅期限 (" + timeEnd.ToLongDateString() + ") " + Convert.ToString(lDelta1) + GetDisplayTimeUnit(strPeriodUnit) + "。";
                    lOver = lDelta1;    // 2009/8/5
                    return 1;
                }
                else
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("已超过借阅期限多少天"), // "已超过借阅期限 ({0}) {1} {2}。",
                        timeReturning.ToLongDateString(),
                        Convert.ToString(lDelta),
                        GetDisplayTimeUnitLang(strPeriodUnit));

                    // "已超过借阅期限 (" + timeEnd.ToLongDateString() + ") " + Convert.ToString(lDelta) + GetDisplayTimeUnit(strPeriodUnit) + "。";
                    lOver = lDelta;    // 2009/8/5
                    return 1;
                }
            }

            if (lDelta == 0 || lDelta1 == 0)
            {
                if (strPeriodUnit == "day")
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("今天就是还书的最后期限"), // "今天就是还书的最后期限 ({0})。"
                        timeReturning.ToLongDateString());
                    // "今天就是还书的最后期限 (" + timeEnd.ToLongDateString() + ")。";
                }
                else if (strPeriodUnit == "hour")
                {
                    // text-level: 用户提示
                    strError += this.GetString("当前这个小时就是还书的最后期限");
                    // "当前这个小时就是还书的最后期限。";
                }
                else
                {
                    // text-level: 用户提示
                    strError += this.GetString("现在就是还书的最后期限");
                    // "现在就是还书的最后期限。";
                }

                if (bEndInNonWorkingDay && lDelta1 < 0)
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("但根据"),
                        calendar.Name,
                        now_rounded.ToLongDateString(),
                        timeNextWorkingDay.ToLongDateString());
                    // "但根据 {0} 显示，今天({1})是非工作日，您可以在最近第一个工作日({2})去图书馆还书。"

                    // "但根据 '" + calendar.Name + "' 显示，今天(" + now_rounded.ToLongDateString() + ")是非工作日，您可以在最近第一个工作日(" + nextWorkingDay.ToLongDateString() + ")去图书馆还书。";
                }

                lOver = 0;    // 2009/8/5
            }
            else
            {
                Debug.Assert(lDelta1 < 0, "");

                bool bOverdue = false;
                // 理论上已经超过最后期限，但是还在宽限期以内
                if (lDelta > 0)
                {
                    Debug.Assert(bEndInNonWorkingDay == true, "");

                    // text-level: 用户提示
                    strError += string.Format(this.GetString("本已超过借阅期限"),
                        timeReturning.ToLongDateString(),
                        Convert.ToString(lDelta),
                        GetDisplayTimeUnitLang(strPeriodUnit));

                    // "本已超过借阅期限 ({0}) {1}{2}。但";

                    // "本已超过借阅期限 (" + timeEnd.ToLongDateString() + ") " + Convert.ToString(lDelta) + GetDisplayTimeUnit(strPeriodUnit) + "。但";
                    bOverdue = true;

                    lOver = lDelta;    // 2009/8/5
                }
                else
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("距最后期限还有"),
                        timeReturning.ToLongDateString(),
                        Convert.ToString(-lDelta1),
                        GetDisplayTimeUnitLang(strPeriodUnit));
                    // "距最后期限 ({0}) 还有 {1}{2}。";

                    // "距最后期限 (" + timeEnd.ToLongDateString() + ") 还有 " + Convert.ToString(-lDelta1) + GetDisplayTimeUnit(strPeriodUnit) + "。";

                    lOver = lDelta1;    // 2009/8/5
                }

                if (bEndInNonWorkingDay == true)
                {
                    // text-level: 用户提示
                    strError += string.Format(this.GetString("根据得知"),
                        calendar.Name,
                        timeReturning.ToLongDateString(),
                        timeNextWorkingDay.ToLongDateString());
                    // "根据 '{0}' 得知，还书截止日 ({1}) 恰逢图书馆非工作日，您可以选择最迟在截止日后的第一个工作日 ({2}) 去图书馆还书。";

                    // "根据 '" + calendar.Name + "' 得知，还书截止日 (" + timeEnd.ToLongDateString() + ") 恰逢图书馆非工作日，您可以选择最迟在截止日后的第一个工作日 (" + nextWorkingDay.ToLongDateString() + ") 去图书馆还书。";
                }

                if (bOverdue == true)
                {
                    strError += "";
                    return 2;
                }
            }

            return 0;
        }

        // 检查每个通知点，返回当前时间已经达到或者超过了通知点的那些检查点的下标
        // return:
        //      -1  数据格式错误
        //      0   成功
        public int CheckNotifyPoint(
            Calendar calendar,
            string strBorrowDate,
            string strPeriod,
            string strNotifyDef,
            out List<int> indices,
            out string strError)
        {
            strError = "";

            indices = new List<int>();

            // long lOver = 0;
            string strPeriodUnit = "";

            DateTime borrowdate;

            LibraryApplication app = this;

            try
            {
                // 注意返回的是GMT时间
                borrowdate = DateTimeUtil.FromRfc1123DateTimeString(strBorrowDate);
            }
            catch
            {
                // text-level: 内部错误
                strError = string.Format(this.GetString("借阅日期值s格式错误"), // "借阅日期值 '{0}' 格式错误"
                    strBorrowDate);

                // "借阅日期值 '" + strBorrowDate + "' 格式错误";
                return -1;
            }

            // 解析期限值
            // string strPeriodUnit = "";
            long lPeriodValue = 0;

            int nRet = ParsePeriodUnit(strPeriod,
                out lPeriodValue,
                out strPeriodUnit,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = string.Format(this.GetString("借阅期限值s格式错误s"),    // "借阅期限 值 '{0}' 格式错误: {1}" 
                    strPeriod,
                    strError);
                // "借阅期限 值 '" + strPeriod + "' 格式错误: " + strError;
                return -1;
            }

            DateTime timeEnd = DateTime.MinValue;   // 还书最后期限
            DateTime nextWorkingDay = DateTime.MinValue;   // 如果还书最后期限正好在一个非工作日上，那么这是其下一个工作日

            // 测算还书日期
            // parameters:
            //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
            // return:
            //      -1  出错
            //      0   成功。timeEnd在工作日范围内。
            //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
            nRet = GetReturnDay(
                calendar,
                borrowdate,
                lPeriodValue,
                strPeriodUnit,
                out timeEnd,
                out nextWorkingDay,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "测算还书时间过程发生错误: " + strError;
                return -1;
            }

#if NO
            bool bEndInNonWorkingDay = false;
            if (nRet == 1)
            {
                // now在非工作日
                bEndInNonWorkingDay = true;
            }
#endif
            DateTime now = this.Clock.UtcNow;

            // 正规化时间
            nRet = RoundTime(strPeriodUnit,
                ref borrowdate,
                out strError);
            if (nRet == -1)
                return -1;
            nRet = RoundTime(strPeriodUnit,
    ref timeEnd,
    out strError);
            if (nRet == -1)
                return -1;
            nRet = RoundTime(strPeriodUnit,
    ref now,
    out strError);
            if (nRet == -1)
                return -1;

            string[] points = strNotifyDef.Split(new char[] { ',' });
            int index = 0;
            foreach (string strOnePoint in points)
            {
                // 观察当天是不是大于等于检查点时刻
                // parameters:
                //      strCheckPoint   单个的检查点定义。-1day,1hour,-19%,10%
                // return:
                //      -1  出错
                //      0   不满足
                //      1   满足
                nRet = GetCheckPoint(borrowdate,
                    timeEnd,
                    now,
                    strOnePoint,
                    out strError);
                if (nRet == -1)
                {
                    strError = "提醒通知定义字符串 '" + strNotifyDef + "' 中 '" + strOnePoint + "' 部分格式错误: " + strError;
                    return -1;
                }

                if (nRet == 1)
                    indices.Add(index);

                index++;
            }

            return 0;
        }

        // 观察当天是不是大于等于检查点时刻
        // parameters:
        //      start   借阅时间(GMT时间)。调用前应当已经根据基本单位进行了正规化
        //      end     应还时间(GMT时间)。调用前应当已经根据基本单位进行了正规化
        //      now     当前时间(GMT时间)。调用前应当已经根据基本单位进行了正规化
        //      strCheckPoint   单个的检查点定义。-1day,1hour,-19%,10%
        // return:
        //      -1  出错
        //      0   不满足
        //      1   满足
        static int GetCheckPoint(DateTime start,
            DateTime end,
            DateTime now,
            string strCheckPoint,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strCheckPoint) == true)
            {
                strError = "strCheckPoint 值不能为空";
                return -1;
            }

            bool bReverse = false;  // 是否从末尾开始找 ?
            DateTime point;
            string strValue = strCheckPoint.Trim();

            if (strValue[0] == '-')
            {
                strValue = strValue.Substring(1).Trim();
                bReverse = true;

                if (string.IsNullOrEmpty(strValue) == true)
                {
                    strError = "负号右边不能为空";
                    return -1;
                }
            }

            // 是否为百分号形式?
            if (strValue[strValue.Length - 1] == '%')
            {
                strValue = strValue.Substring(0, strValue.Length - 1).Trim();
                if (string.IsNullOrEmpty(strValue) == true)
                {
                    strError = "百分号左边不能为空";
                    return -1;
                }

                // 数值
                float v = 0;
                if (float.TryParse(strValue, out v) == false)
                {
                    strError = "检查点定义 '" + strCheckPoint + "' 格式错误，其中 '" + strValue + "' 部分应该为数值形态";
                    return -1;
                }

                // 计算出时间点
                if (bReverse == true)
                    point = end - new TimeSpan((long)((end - start).Ticks * (v / 100)));
                else
                    point = start + new TimeSpan((long)((end - start).Ticks * (v / 100)));

                // point 是否需要正规化？ 困难是此时不具备时间参数参数

                if (point <= start || point >= end)
                    return 0;

                if (now >= point)
                    return 1;

                return 0;
            }

            // 解析期限值
            string strPeriodUnit = "";
            long lPeriodValue = 0;

            int nRet = ParsePeriodUnit(strValue,
                out lPeriodValue,
                out strPeriodUnit,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta;

            if (strPeriodUnit == "day")
                delta = new TimeSpan((int)lPeriodValue, 0, 0, 0);
            else if (strPeriodUnit == "hour")
                delta = new TimeSpan((int)lPeriodValue, 0, 0);
            else
            {
                strError = "未知的时间单位 '" + strPeriodUnit + "'";
                return -1;
            }

            // 计算出时间点
            if (bReverse == true)
                point = end - delta;
            else
                point = start + delta;

            if (point <= start || point >= end)
                return 0;

            if (now >= point)
                return 1;

            return 0;
        }

        // 把时间单位变换为可读的形态
        // 以前的版本
        public static string GetDisplayTimeUnit(string strUnit)
        {
            if (strUnit == "day")
                return "天";
            if (strUnit == "hour")
                return "小时";

            return strUnit; // 无法翻译的
        }


        // 把时间单位变换为可读的形态
        // 新版本，能够自动适应当前语言
        public string GetDisplayTimeUnitLang(string strUnit)
        {
            if (strUnit == "day")
                return this.GetString("天");
            if (strUnit == "hour")
                return this.GetString("小时");

            return strUnit; // 无法翻译的
        }

        // 把整个字符串中的时间单位变换为可读的形态
        // 语言相关的最新版本
        public string GetDisplayTimePeriodStringEx(string strText)
        {
            strText = strText.Replace("day", this.GetString("天"));

            return strText.Replace("hour", this.GetString("小时"));
        }


        // 把整个字符串中的时间单位变换为可读的形态
        // 为了兼容某些旧的脚本而保留的版本，建议今后不要用了，而改用GetDisplayTimePeriodStringEx()
        public static string GetDisplayTimePeriodString(string strText)
        {
            strText = strText.Replace("day", "天");

            return strText.Replace("hour", "小时");
        }

        // 根据strPeriod中的时间单位(day/hour)，返回本地日期或者时间字符串
        // parameters:
        //      strPeriod   原始格式的时间长度字符串。也就是说，时间单位不和语言相关，是"day"或"hour"
        public static string LocalDateOrTime(string strTimeString,
            string strPeriod)
        {
            string strError = "";
            long lValue = 0;
            string strUnit = "";
            int nRet = LibraryApplication.ParsePeriodUnit(strPeriod,
                        out lValue,
                        out strUnit,
                        out strError);
            if (nRet == -1)
                strUnit = "day";
            if (strUnit == "day")
                return DateTimeUtil.LocalDate(strTimeString);

            return DateTimeUtil.LocalTime(strTimeString);
        }

        // 根据strPeriod中的时间单位(day/hour)，返回本地日期或者时间字符串
        // parameters:
        //      strPeriod   原始格式的时间长度字符串。也就是说，时间单位不和语言相关，是"day"或"hour"
        public static string LocalDateOrTime(DateTime time,
            string strPeriod)
        {
            string strError = "";
            long lValue = 0;
            string strUnit = "";
            int nRet = LibraryApplication.ParsePeriodUnit(strPeriod,
                        out lValue,
                        out strUnit,
                        out strError);
            if (nRet == -1)
                strUnit = "day";
            if (strUnit == "day")
                return time.ToString("d");  // 精确到日

            return time.ToString("g");  // 精确到分钟。G精确到秒
            // http://www.java2s.com/Tutorial/CSharp/0260__Date-Time/UsetheToStringmethodtoconvertaDateTimetoastringdDfFgGmrstTuUy.htm
        }

        // 分析价格参数
        // 2006/10/11
        public static int ParsePriceUnit(string strString,
            out string strPrefix,
            out double fValue,
            out string strPostfix,
            out string strError)
        {
            strPrefix = "";
            fValue = 0.0F;
            strPostfix = "";
            strError = "";

            strString = strString.Trim();

            if (String.IsNullOrEmpty(strString) == true)
            {
                strError = "价格字符串为空";
                return -1;
            }

            string strValue = "";

            bool bInPrefix = true;

            for (int i = 0; i < strString.Length; i++)
            {
                if ((strString[i] >= '0' && strString[i] <= '9')
                    || strString[i] == '.')
                {
                    bInPrefix = false;
                    strValue += strString[i];
                }
                else
                {
                    if (bInPrefix == true)
                        strPrefix += strString[i];
                    else
                    {
                        strPostfix = strString.Substring(i).Trim();
                        break;
                    }
                }
            }

            // 将strValue转换为数字
            try
            {
                fValue = Convert.ToDouble(strValue);
            }
            catch (Exception)
            {
                strError = "价格参数数字部分'" + strValue + "'格式不合法";
                return -1;
            }

            /*
            if (String.IsNullOrEmpty(strUnit) == true)
                strUnit = "CNY";   // 缺省单位为 人民币元

            strUnit = strUnit.ToUpper();    // 统一转换为大写
             * */

            return 0;
        }

        // 分析期限参数
        public static int ParsePeriodUnit(string strPeriod,
            out long lValue,
            out string strUnit,
            out string strError)
        {
            lValue = 0;
            strUnit = "";
            strError = "";

            strPeriod = strPeriod.Trim();

            if (String.IsNullOrEmpty(strPeriod) == true)
            {
                strError = "期限字符串为空";
                return -1;
            }

            string strValue = "";


            for (int i = 0; i < strPeriod.Length; i++)
            {
                if (strPeriod[i] >= '0' && strPeriod[i] <= '9')
                {
                    strValue += strPeriod[i];
                }
                else
                {
                    strUnit = strPeriod.Substring(i).Trim();
                    break;
                }
            }

            // 将strValue转换为数字
            try
            {
                lValue = Convert.ToInt64(strValue);
            }
            catch (Exception)
            {
                strError = "期限参数数字部分'" + strValue + "'格式不合法";
                return -1;
            }

            if (String.IsNullOrEmpty(strUnit) == true)
                strUnit = "day";   // 缺省单位为"天"

            strUnit = strUnit.ToLower();    // 统一转换为小写

            return 0;
        }


        // 按照时间单位,把时间值零头去除,正规化,便于后面计算差额
        /// <summary>
        /// 按照时间基本单位，去掉零头，便于互相计算(整单位的)差额。
        /// 算法是先转换为本地时间，去掉零头，再转换回 GMT 时间
        /// </summary>
        /// <param name="strUnit">时间单位。day/hour之一。如果为空，相当于 day</param>
        /// <param name="time">要处理的时间。为 GMT 时间</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int RoundTime(string strUnit,
            ref DateTime time,
            out string strError)
        {
            strError = "";

            time = time.ToLocalTime();
            if (strUnit == "day" || string.IsNullOrEmpty(strUnit) == true)
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    12, 0, 0, 0);
            }
            else if (strUnit == "hour")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    time.Hour, 0, 0, 0);
            }
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }
            time = time.ToUniversalTime();
            return 0;
        }

        public static int ParseTimeSpan(
            TimeSpan delta,
            string strUnit,
            out long lValue,
            out string strError)
        {
            lValue = 0;
            strError = "";

            if (strUnit == "day")
                lValue = (long)delta.TotalDays;
            else if (strUnit == "hour")
                lValue = (long)delta.TotalHours;
            else
            {
                strError = "不能识别的时间单位 '" + strUnit + "'";
                return -1;
            }

            return 0;
        }

        // 构造TimeSpan
        public static int BuildTimeSpan(
            long lPeriod,
            string strUnit,
            out TimeSpan delta,
            out string strError)
        {
            strError = "";

            if (strUnit == "day")
                delta = new TimeSpan((int)lPeriod, 0, 0, 0);
            else if (strUnit == "hour")
                delta = new TimeSpan((int)lPeriod, 0, 0);
            else
            {
                delta = new TimeSpan(0);
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }

            return 0;
        }

        // 获得预约保留末端时间
        // 中间要排除所有非工作日
        // parameters:
        //      calendar    日历对象。可以为 null
        public static int GetOverTime(
            Calendar calendar,
            DateTime timeStart,
            long lPeriod,
            string strUnit,
            out DateTime timeEnd,
            out string strError)
        {
            strError = "";
            timeEnd = DateTime.MinValue;

            // 正规化时间
            int nRet = RoundTime(strUnit,
                ref timeStart,
                out strError);
            if (nRet == -1)
                return -1;

            if (calendar == null)
            {
                TimeSpan delta;

                if (strUnit == "day")
                    delta = new TimeSpan((int)lPeriod, 0, 0, 0);
                else if (strUnit == "hour")
                    delta = new TimeSpan((int)lPeriod, 0, 0);
                else
                {
                    strError = "未知的时间单位 '" + strUnit + "'";
                    return -1;
                }

                timeEnd = timeStart + delta;

                // 正规化时间
                nRet = RoundTime(strUnit,
                    ref timeEnd,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                TimeSpan delta;

                if (strUnit == "day")
                    delta = new TimeSpan((int)lPeriod, 0, 0, 0);
                else if (strUnit == "hour")
                    delta = new TimeSpan((int)lPeriod, 0, 0);
                else
                {
                    strError = "未知的时间单位 '" + strUnit + "'";
                    return -1;
                }

                timeEnd = calendar.GetEndTime(timeStart,
                    delta);

                // 正规化时间
                nRet = RoundTime(strUnit,
                    ref timeEnd,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        // 测算还书日期
        // parameters:
        //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
        //      timeStart   借阅开始时间。GMT时间
        //      timeEnd     返回应还回的最后时间。GMT时间
        // return:
        //      -1  出错
        //      0   成功。timeEnd在工作日范围内。
        //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
        public static int GetReturnDay(
            Calendar calendar,
            DateTime timeStart,
            long lPeriod,
            string strUnit,
            out DateTime timeEnd,
            out DateTime nextWorkingDay,
            out string strError)
        {
            strError = "";
            timeEnd = DateTime.MinValue;
            nextWorkingDay = DateTime.MinValue;

            // 正规化时间
            int nRet = RoundTime(strUnit,
                ref timeStart,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta;

            if (strUnit == "day")
                delta = new TimeSpan((int)lPeriod, 0, 0, 0);
            else if (strUnit == "hour")
                delta = new TimeSpan((int)lPeriod, 0, 0);
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }

            timeEnd = timeStart + delta;

            // 正规化时间
            nRet = RoundTime(strUnit,
                ref timeEnd,
                out strError);
            if (nRet == -1)
                return -1;

            bool bInNonWorkingDay = false;

            // 看看末端是否正好在非工作日
            if (calendar != null)
            {
                bInNonWorkingDay = calendar.IsInNonWorkingDay(timeEnd,
                    out nextWorkingDay);
                nRet = RoundTime(strUnit,
    ref nextWorkingDay,
    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (bInNonWorkingDay == true)
            {
                Debug.Assert(nextWorkingDay != DateTime.MinValue, "");
                return 1;
            }

            return 0;

        }

        // 计算时间之间的距离
        // parameters:
        //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
        // return:
        //      -1  出错
        //      0   成功。timeEnd在工作日范围内。
        //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
        public static int GetTimeDistance(
            Calendar calendar,
            string strUnit,
            DateTime timeStart,
            DateTime timeEnd,
            out long lValue,
            out DateTime nextWorkingDay,
            out string strError)
        {
            lValue = 0;
            strError = "";
            nextWorkingDay = DateTime.MinValue;


            int nRet = RoundTime(strUnit,
                ref timeStart,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = RoundTime(strUnit,
                ref timeEnd,
                out strError);
            if (nRet == -1)
                return -1;

            bool bInNonWorkingDay = false;

            // 看看末端是否正好在非工作日
            if (calendar != null)
            {
                bInNonWorkingDay = calendar.IsInNonWorkingDay(timeEnd,
                    out nextWorkingDay);
                nRet = RoundTime(strUnit,
    ref nextWorkingDay,
    out strError);
                if (nRet == -1)
                    return -1;
            }

            TimeSpan delta;

            delta = timeEnd - timeStart;

            if (strUnit == "day")
            {
                lValue = (long)delta.TotalDays;
            }
            else if (strUnit == "hour")
            {
                lValue = (long)delta.TotalHours;
            }
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }

            if (bInNonWorkingDay == true)
            {
                Debug.Assert(nextWorkingDay != DateTime.MinValue, "");
                return 1;
            }

            return 0;
        }



        // 检测一个条码号是否在列表中
        static bool IsInBarcodeList(string strBarcode,
            string strBarcodeList)
        {
            string[] barcodes = strBarcodeList.Split(new char[] { ',' });
            for (int i = 0; i < barcodes.Length; i++)
            {
                string strPerBarcode = barcodes[i].Trim();
                if (String.IsNullOrEmpty(strPerBarcode) == true)
                    continue;

                if (strPerBarcode == strBarcode)
                    return true;
            }

            return false;
        }

        // Undo一个已交费记录
        int UndoOneAmerce(SessionInfo sessioninfo,
            string strReaderBarcode,
            string strAmercedItemId,
            out string strReaderXml,
            out string strError)
        {
            strError = "";
            strReaderXml = "";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            long lRet = 0;
            int nRet = 0;

            string strFrom = "ID";
            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(this.AmerceDbName + ":" + strFrom)       // 2007/9/14
                + "'><item><word>"
                + strAmercedItemId + "</word><match>" + "exact" + "</match><relation>=</relation><dataType>string</dataType><maxCount>100</maxCount></item><lang>" + "zh" + "</lang></target>";

            lRet = channel.DoSearch(strQueryXml,
                "amerced",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
            {
                strError = "检索ID为 '" + strAmercedItemId + "' 的已付违约金记录出错: " + strError;
                return -1;
            }

            if (lRet == 0)
            {
                strError = "没有找到ID为 '" + strAmercedItemId + "' 的已付违约金记录";
                return -1;
            }

            List<string> aPath = null;
            lRet = channel.DoGetSearchResult("amerced",
                100,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
            {
                strError = "检索ID为 '" + strAmercedItemId + "' 的已付违约金记录，获取浏览格式阶段出错: " + strError;
                return -1;
            }

            if (lRet == 0)
            {
                strError = "检索ID为 '" + strAmercedItemId + "' 的已付违约金记录，已检索命中，但是获取浏览格式没有找到";
                return -1;
            }

            if (aPath.Count == 0)
            {
                strError = "检索ID为 '" + strAmercedItemId + "' 的已付违约金记录，已检索命中，但是获取浏览格式没有找到";
                return -1;
            }

            if (aPath.Count > 1)
            {
                strError = "ID为 '" + strAmercedItemId + "' 的已付违约金记录检索出多条。请系统管理员及时更正此错误。";
                return -1;
            }

            string strAmercedRecPath = aPath[0];

            string strMetaData = "";
            byte[] amerced_timestamp = null;
            string strOutputPath = "";
            string strAmercedXml = "";

            lRet = channel.GetRes(strAmercedRecPath,
                out strAmercedXml,
                out strMetaData,
                out amerced_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "获取已付违约金记录 '" + strAmercedRecPath + "' 时出错: " + strError;
                return -1;
            }

            string strOverdueString = "";
            string strOutputReaderBarcode = "";

            // 将违约金记录格式转换为读者记录中的<overdue>元素格式
            // return:
            //      -1  error
            //      0   strAmercedXml中<state>元素的值为*非*"settlemented"
            //      1   strAmercedXml中<state>元素的值为"settlemented"
            nRet = ConvertAmerceRecordToOverdueString(strAmercedXml,
                out strOutputReaderBarcode,
                out strOverdueString,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
            {
                strError = "ID为 " + strAmercedItemId + " (路径为 '" + strOutputPath + "' ) 的违约金库记录其状态为 已结算(settlemented)，不能撤回交费操作";
                return -1;
            }

            // 如果strReaderBarcode参数值非空，则要检查一下检索出来的已付违约金记录是否真的属于这个读者
            if (String.IsNullOrEmpty(strReaderBarcode) == false
                && strReaderBarcode != strOutputReaderBarcode)
            {
                strError = "ID为 '" + strAmercedItemId + "' 的已付违约金记录，并不是属于所指定的读者 '" + strReaderBarcode + "'，而是属于另一读者 '" + strOutputReaderBarcode + "'";
                return -1;
            }

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("UndoOneAmerce 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForWrite(strReaderBarcode);

            try
            {
                // 读入读者记录
                strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;

                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                string strLibraryCode = "";
                // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                if (String.IsNullOrEmpty(strOutputReaderRecPath) == false)
                {
                    // 检查当前操作者是否管辖这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
            sessioninfo.LibraryCodeList,
            out strLibraryCode) == false)
                    {
                        strError = "读者记录路径 '" + strOutputReaderRecPath + "' 从属的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 准备日志DOM
                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "libraryCode",
                    strLibraryCode);    // 读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement, "operation",
                    "amerce");

                bool bReaderDomChanged = false;

                // 修改读者记录
                // 增添超期信息
                if (String.IsNullOrEmpty(strOverdueString) != true)
                {
                    XmlDocumentFragment fragment = readerdom.CreateDocumentFragment();
                    fragment.InnerXml = strOverdueString;

                    // 看看根下面是否有overdues元素
                    XmlNode root = readerdom.DocumentElement.SelectSingleNode("overdues");
                    if (root == null)
                    {
                        root = readerdom.CreateElement("overdues");
                        readerdom.DocumentElement.AppendChild(root);
                    }

                    // 2008/11/11
                    // undo交押金
                    XmlNode node_added = root.AppendChild(fragment);
                    bReaderDomChanged = true;

                    Debug.Assert(node_added != null, "");
                    string strReason = DomUtil.GetAttr(node_added, "reason");
                    if (strReason == "押金。")
                    {
                        string strPrice = "";

                        strPrice = DomUtil.GetAttr(node_added, "newPrice");
                        if (String.IsNullOrEmpty(strPrice) == true)
                            strPrice = DomUtil.GetAttr(node_added, "price");
                        else
                        {
                            Debug.Assert(strPrice.IndexOf('%') == -1, "从newPrice属性中取出来的价格字符串，岂能包含%符号");
                        }

                        if (String.IsNullOrEmpty(strPrice) == false)
                        {
                            // 需要从<foregift>元素中减去这个价格
                            string strContent = DomUtil.GetElementText(readerdom.DocumentElement,
                                "foregift");

                            string strNegativePrice = "";
                            // 将形如"-123.4+10.55-20.3"的价格字符串反转正负号
                            // parameters:
                            //      bSum    是否要顺便汇总? true表示要汇总
                            nRet = PriceUtil.NegativePrices(strPrice,
                                false,
                                out strNegativePrice,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "反转价格字符串 '" + strPrice + "时发生错误: " + strError;
                                goto ERROR1;
                            }

                            strContent = PriceUtil.JoinPriceString(strContent, strNegativePrice);

                            DomUtil.SetElementText(readerdom.DocumentElement,
                                "foregift",
                                strContent);
                            bReaderDomChanged = true;
                        }
                    }
                }

                if (bReaderDomChanged == true)
                {
                    byte[] output_timestamp = null;

                    strReaderXml = readerdom.OuterXml;
                    // 野蛮写入
                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        strReaderXml,
                        false,
                        "content,ignorechecktimestamp", // ?????
                        reader_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    int nRedoDeleteCount = 0;
                REDO_DELETE:
                    // 删除已付违约金记录
                    lRet = channel.DoDeleteRes(strAmercedRecPath,
                        amerced_timestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                            && nRedoDeleteCount < 10)
                        {
                            nRedoDeleteCount++;
                            amerced_timestamp = output_timestamp;
                            goto REDO_DELETE;
                        }
                        strError = "删除已付违约金记录 '" + strAmercedRecPath + "' 失败: " + strError;
                        this.WriteErrorLog(strError);
                        goto ERROR1;
                    }

                    // 具体动作
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "action", "undo");

                    // id list
                    /*
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "idList", strAmercedItemId);
                     * */
                    AmerceItem[] amerce_items = new AmerceItem[1];
                    amerce_items[0] = new AmerceItem();
                    amerce_items[0].ID = strAmercedItemId;
                    WriteAmerceItemList(domOperLog,
                        amerce_items);


                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerBarcode", strReaderBarcode);

                    /*
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "amerceItemID", strAmercedItemId);
                     */

                    // 删除掉的违约金记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "amerceRecord", strAmercedXml);
                    DomUtil.SetAttr(node, "recPath", strAmercedRecPath);

                    // 最新的读者记录
                    node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerRecord", strReaderXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);


                    string strOperTime = this.Clock.GetClock();
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        sessioninfo.UserID);   // 操作者
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTime);   // 操作时间

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "Amerce() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }

                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(strLibraryCode,
                        "违约金",
                        "取消次",
                        1);

                    {
                        string strPrice = "";
                        // 取出违约金记录中的金额数字
                        nRet = GetAmerceRecordPrice(strAmercedXml,
                            out strPrice,
                            out strError);
                        if (nRet != -1)
                        {
                            string strPrefix = "";
                            string strPostfix = "";
                            double fValue = 0.0;
                            // 分析价格参数
                            nRet = ParsePriceUnit(strPrice,
                                out strPrefix,
                                out fValue,
                                out strPostfix,
                                out strError);
                            if (nRet != -1)
                            {
                                if (this.Statis != null)
                                    this.Statis.IncreaseEntryValue(
                                    strLibraryCode,
                                    "违约金",
                                    "取消元",
                                    fValue);
                            }
                        }
                    }
                }
            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("UndoOneAmerce 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif

            }

            return 0;
        ERROR1:
            return -1;
        }

        // UNDO违约金交纳
        // return:
        //      -1  error
        //      0   succeed
        //      1   部分成功。strError中有报错信息，failed_item中有那些没有被处理的item的列表
        int UndoAmerces(
            SessionInfo sessioninfo,
            string strReaderBarcode,
            AmerceItem[] amerce_items,
            out AmerceItem[] failed_items,
            out string strReaderXml,
            out string strError)
        {
            strError = "";
            strReaderXml = "";
            failed_items = null;
            int nErrorCount = 0;

            List<string> OverdueStrings = new List<string>();
            List<string> AmercedRecPaths = new List<string>();

            // string[] ids = strAmercedItemIdList.Split(new char[] { ',' });
            List<AmerceItem> failed_list = new List<AmerceItem>();
            for (int i = 0; i < amerce_items.Length; i++)
            {
                AmerceItem item = amerce_items[i];

                /*
                string strID = ids[i].Trim();
                 * */
                if (String.IsNullOrEmpty(item.ID) == true)
                    continue;

                string strTempError = "";

                int nRet = UndoOneAmerce(sessioninfo,
                    strReaderBarcode,
                    item.ID,
                    out strReaderXml,
                    out strTempError);
                if (nRet == -1)
                {
                    if (String.IsNullOrEmpty(strError) == false)
                        strError += ";\r\n";
                    strError += strTempError;
                    nErrorCount++;
                    // return -1;
                    failed_list.Add(item);
                }
            }

            // 每个ID都发生了错误
            if (nErrorCount >= amerce_items.Length)
                return -1;

            // 部分发生错误
            if (nErrorCount > 0)
            {
                failed_items = new AmerceItem[failed_list.Count];
                failed_list.CopyTo(failed_items);

                strError = "操作部分成功。(共提交了 " + amerce_items.Length + " 个事项，发生错误的有 " + nErrorCount + " 个) \r\n" + strError;
                return 1;
            }

            return 0;
        }

        // 交违约金/撤销交违约金
        // parameters:
        //      strReaderBarcode    如果功能是"undo"，可以将此参数设置为null。如果此参数不为null，则软件要进行核对，如果不是这个读者的已付违约金记录，则要报错
        //      strAmerceItemIdList id列表, 以逗号分割
        // 权限：需要有amerce/amercemodifyprice/amerceundo/amercemodifycomment等权限
        // 日志：
        //      要产生日志
        // return:
        //      result.Value    0 成功；1 部分成功(result.ErrorInfo中有信息)
        public LibraryServerResult Amerce(
            SessionInfo sessioninfo,
            string strFunction,
            string strReaderBarcode,
            AmerceItem[] amerce_items,
            out AmerceItem[] failed_items,
            out string strReaderXml)
        {
            strReaderXml = "";
            failed_items = null;

            LibraryServerResult result = new LibraryServerResult();

            if (String.Compare(strFunction, "amerce", true) == 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("amerce", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "交违约金操作被拒绝。不具备amerce权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (String.Compare(strFunction, "modifyprice", true) == 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("amercemodifyprice", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改违约金额的操作被拒绝。不具备amercemodifyprice权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (String.Compare(strFunction, "modifycomment", true) == 0)
            {
                /*
                // 权限字符串
                if (StringUtil.IsInList("amercemodifycomment", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改违约金之注释的操作被拒绝。不具备amercemodifycomment权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
                 * */
            }

            if (String.Compare(strFunction, "undo", true) == 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("amerceundo", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "撤销交违约金操作被拒绝。不具备amerceundo权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (String.Compare(strFunction, "rollback", true) == 0)
            {
                // 权限字符串
                if (StringUtil.IsInList("amerce", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "撤回交违约金事务的操作被拒绝。不具备amerce权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            if (strFunction != "rollback")
            {
                // 看看amerce_items中是否有价格变更或注释变更的情况
                bool bHasNewPrice = false;
                bool bHasOverwriteComment = false;    // NewComment具有、并且为覆盖。也就是说包括NewPrice和NewComment同时具有的情况
                for (int i = 0; i < amerce_items.Length; i++)
                {
                    AmerceItem item = amerce_items[i];

                    // NewPrice域中有值
                    if (String.IsNullOrEmpty(item.NewPrice) == false)
                    {
                        bHasNewPrice = true;
                    }

                    // NewComment域中有值
                    if (String.IsNullOrEmpty(item.NewComment) == false)
                    {
                        string strNewComment = item.NewComment;

                        bool bAppend = true;
                        if (string.IsNullOrEmpty(strNewComment) == false
                            && strNewComment[0] == '<')
                        {
                            bAppend = false;
                            strNewComment = strNewComment.Substring(1);
                        }
                        else if (string.IsNullOrEmpty(strNewComment) == false
                            && strNewComment[0] == '>')
                        {
                            bAppend = true;
                            strNewComment = strNewComment.Substring(1);
                        }

                        if (bAppend == false)
                            bHasOverwriteComment = true;
                    }
                }

                // 如果要变更价格，则需要额外的amercemodifyprice权限。
                // amercemodifyprice在功能amerce和modifyprice中都可能用到，关键是看是否提交了有新价格的参数
                if (bHasNewPrice == true)
                {
                    if (StringUtil.IsInList("amercemodifyprice", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "含有价格变更要求的交违约金操作被拒绝。不具备amercemodifyprice权限。(仅仅具备amerce权限还不够的)";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                if (bHasOverwriteComment == true)
                {
                    // 如果有了amerce权限，则暗含有了amerceappendcomment的权限

                    if (StringUtil.IsInList("amercemodifycomment", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "含有违约金注释(覆盖型)变更要求的操作被拒绝。不具备amercemodifycomment权限。(仅仅具备amerce权限还不够的)";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
            }

            int nRet = 0;
            string strError = "";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            if (String.Compare(strFunction, "amerce", true) != 0
                && String.Compare(strFunction, "undo", true) != 0
                && String.Compare(strFunction, "modifyprice", true) != 0
                && String.Compare(strFunction, "modifycomment", true) != 0
                && String.Compare(strFunction, "rollback", true) != 0)
            {
                result.Value = -1;
                result.ErrorInfo = "未知的strFunction参数值 '" + strFunction + "'";
                result.ErrorCode = ErrorCode.InvalidParameter;
                return result;
            }

            // 如果是undo, 需要先检索出指定id的违约金库记录，然后从记录中得到<readerBarcode>，和参数核对
            if (String.Compare(strFunction, "undo", true) == 0)
            {
                // UNDO违约金交纳
                // return:
                //      -1  error
                //      0   succeed
                //      1   部分成功。strError中有报错信息
                nRet = UndoAmerces(
                    sessioninfo,
                    strReaderBarcode,
                    amerce_items,
                    out failed_items,
                    out strReaderXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2009/10/10 changed
                result.Value = nRet;
                if (nRet == 1)
                    result.ErrorInfo = strError;
                return result;
            }

            // 回滚
            // 2009/7/14
            if (String.Compare(strFunction, "rollback", true) == 0)
            {
                if (amerce_items != null)
                {
                    strError = "调用rollback功能时amerce_item参数必须为空";
                    goto ERROR1;
                }

                if (sessioninfo.AmerceIds == null
                    || sessioninfo.AmerceIds.Count == 0)
                {
                    strError = "当前没有可以rollback的违约金事项";
                    goto ERROR1;
                }

                // strReaderBarcode参数值一般为空即可。如果有值，则要求和SessionInfo对象中储存的最近一次的Amerce操作读者证条码号一致
                if (String.IsNullOrEmpty(strReaderBarcode) == false)
                {
                    if (sessioninfo.AmerceReaderBarcode != strReaderBarcode)
                    {
                        strError = "调用rollback功能时strReaderBarcode参数和最近一次Amerce操作的读者证条码号不一致";
                        goto ERROR1;
                    }
                }

                amerce_items = new AmerceItem[sessioninfo.AmerceIds.Count];

                for (int i = 0; i < sessioninfo.AmerceIds.Count; i++)
                {
                    AmerceItem item = new AmerceItem();
                    item.ID = sessioninfo.AmerceIds[i];

                    amerce_items[i] = item;
                }

                // UNDO违约金交纳
                // return:
                //      -1  error
                //      0   succeed
                //      1   部分成功。strError中有报错信息
                nRet = UndoAmerces(
                    sessioninfo,
                    sessioninfo.AmerceReaderBarcode,
                    amerce_items,
                    out failed_items,
                    out strReaderXml,
                    out strError);
                if (nRet == -1 || nRet == 1)
                    goto ERROR1;

                // 清空ids
                sessioninfo.AmerceIds = new List<string>();
                sessioninfo.AmerceReaderBarcode = "";

                result.Value = 0;
                return result;
            }

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("Amerce 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForWrite(strReaderBarcode);

            try
            {
                // 读入读者记录
                strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;
                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    result.Value = -1;
                    result.ErrorInfo = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                    return result;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                // 所操作的读者库德馆代码
                string strLibraryCode = "";

                // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                if (String.IsNullOrEmpty(strOutputReaderRecPath) == false)
                {
                    // 获得读者库的馆代码
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = GetLibraryCode(
            strOutputReaderRecPath,
            out strLibraryCode,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 检查当前操作者是否管辖这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
            sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "读者记录路径 '" + strOutputReaderRecPath + "' 从属的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 准备日志DOM
                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "operation",
                    "amerce");

                // 具体动作
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "action", strFunction.ToLower());

                // 读者证条码号
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "readerBarcode", strReaderBarcode);

                //
                List<string> AmerceRecordXmls = null;
                List<string> CreatedNewPaths = null;

                List<string> Ids = null;


                string strOperTimeString = this.Clock.GetClock();   // RFC1123格式


                bool bReaderDomChanged = false; // 读者dom是否发生了变化，需要回存

                {
                    // 在日志中保留旧的读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "oldReaderRecord", strReaderXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);
                }

                if (String.Compare(strFunction, "modifyprice", true) == 0)
                {
                    /*
                    // 在日志中保留旧的读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "oldReaderRecord", strReaderXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);
                     * */

                    nRet = ModifyPrice(ref readerdom,
                        amerce_items,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet != 0)
                    {
                        bReaderDomChanged = true;
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "违约金",
                            "修改次",
                            nRet);
                    }
                    else
                    {
                        // 如果一个事项也没有发生修改，则需要返回错误信息，以引起前端的警觉
                        strError = "警告：没有任何事项的价格(和注释)被修改。";
                        goto ERROR1;
                    }

                    goto SAVERECORD;
                }

                if (String.Compare(strFunction, "modifycomment", true) == 0)
                {
                    /*
                    // 在日志中保留旧的读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "oldReaderRecord", strReaderXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);
                     * */

                    nRet = ModifyComment(
                        ref readerdom,
                        amerce_items,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet != 0)
                    {
                        bReaderDomChanged = true;
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "违约金之注释",
                            "修改次",
                            nRet);
                    }
                    else
                    {
                        // 如果一个事项也没有发生修改，则需要返回错误信息，以引起前端的警觉
                        strError = "警告：没有任何事项的注释被修改。";
                        goto ERROR1;
                    }

                    goto SAVERECORD;
                }

                List<string> NotFoundIds = null;
                Ids = null;

                // 交违约金：在读者记录中去除所选的<overdue>元素，并且构造一批新记录准备加入违约金库
                // return:
                //      -1  error
                //      0   读者dom没有变化
                //      1   读者dom发生了变化
                nRet = DoAmerceReaderXml(
                    strLibraryCode,
                    ref readerdom,
                    amerce_items,
                    sessioninfo.UserID,
                    strOperTimeString,
                    out AmerceRecordXmls,
                    out NotFoundIds,
                    out Ids,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                    bReaderDomChanged = true;


                // 在违约金数据库中创建若干新的违约金记录
                // parameters:
                //      AmerceRecordXmls    需要写入的新记录的数组
                //      CreatedNewPaths 已经创建的新记录的路径数组。可以用于Undo(删除刚刚创建的新记录)
                nRet = CreateAmerceRecords(
                    // sessioninfo.Channels,
                    channel,
                    AmerceRecordXmls,
                    out CreatedNewPaths,
                    out strError);
                if (nRet == -1)
                {
                    // undo已经写入的部分记录
                    if (CreatedNewPaths != null
                        && CreatedNewPaths.Count != 0)
                    {
                        string strNewError = "";
                        nRet = DeleteAmerceRecords(
                            sessioninfo.Channels,
                            CreatedNewPaths,
                            out strNewError);
                        if (nRet == -1)
                        {
                            string strList = "";
                            for (int i = 0; i < CreatedNewPaths.Count; i++)
                            {
                                if (strList != "")
                                    strList += ",";
                                strList += CreatedNewPaths[i];
                            }
                            strError = "在创建新的违约金记录的过程中发生错误: " + strError + "。在Undo新创建的违约金记录的过程中，又发生错误: " + strNewError + ", 请系统管理员手工删除新创建的罚款记录: " + strList;
                            goto ERROR1;
                        }
                    }

                    goto ERROR1;
                }

            SAVERECORD:

                // 为写回读者、册记录做准备
                // byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

#if NO
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
#endif
                long lRet = 0;

                if (bReaderDomChanged == true)
                {
                    strReaderXml = readerdom.OuterXml;

                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        strReaderXml,
                        false,
                        "content,ignorechecktimestamp",
                        reader_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;


                    // id list
                    /*
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "idList", strAmerceItemIdList);
                     * */
                    WriteAmerceItemList(domOperLog, amerce_items);


                    /*
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerBarcode", strReaderBarcode);
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "itemBarcodeList", strItemBarcodeList);
                     */

                    // 仅当功能为amerce时，才把被修改的实体记录写入日志。
                    if (String.Compare(strFunction, "amerce", true) == 0)
                    {

                        Debug.Assert(AmerceRecordXmls.Count == CreatedNewPaths.Count, "");

                        // 写入多个重复的<amerceRecord>元素
                        for (int i = 0; i < AmerceRecordXmls.Count; i++)
                        {
                            XmlNode nodeAmerceRecord = domOperLog.CreateElement("amerceRecord");
                            domOperLog.DocumentElement.AppendChild(nodeAmerceRecord);
                            nodeAmerceRecord.InnerText = AmerceRecordXmls[i];

                            DomUtil.SetAttr(nodeAmerceRecord, "recPath", CreatedNewPaths[i]);
                            /*
                            DomUtil.SetElementText(domOperLog.DocumentElement,
                                "record", AmerceRecordXmls[i]);
                             **/

                            if (this.Statis != null)
                                this.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "违约金",
                                "给付次",
                                1);

                            {
                                string strPrice = "";
                                // 取出违约金记录中的金额数字
                                nRet = GetAmerceRecordPrice(AmerceRecordXmls[i],
                                    out strPrice,
                                    out strError);
                                if (nRet != -1)
                                {
                                    string strPrefix = "";
                                    string strPostfix = "";
                                    double fValue = 0.0;
                                    // 分析价格参数
                                    nRet = ParsePriceUnit(strPrice,
                                        out strPrefix,
                                        out fValue,
                                        out strPostfix,
                                        out strError);
                                    if (nRet != -1)
                                    {
                                        if (this.Statis != null)
                                            this.Statis.IncreaseEntryValue(
                                            strLibraryCode,
                                            "违约金",
                                            "给付元",
                                            fValue);
                                    }
                                    else
                                    {
                                        // 2012/11/15
                                        this.WriteErrorLog("累计 违约金 给付元 [" + strPrice + "] 时出错: " + strError);
                                    }
                                }
                            }
                        } // end of for
                    }

                    // 最新的读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "readerRecord", strReaderXml);
                    DomUtil.SetAttr(node, "recPath", strOutputReaderRecPath);

                    string strOperTime = this.Clock.GetClock();
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        sessioninfo.UserID);   // 操作者
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTime);   // 操作时间

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "Amerce() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }
                }

                // 记忆下最近一次Amerce操作的ID和读者证条码号
                if (strFunction != "rollback"
                    && Ids != null
                    && Ids.Count != 0)
                {
                    sessioninfo.AmerceReaderBarcode = strReaderBarcode;
                    sessioninfo.AmerceIds = Ids;
                }
            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("Amerce 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 根据AmerceItem数组，修改readerdom中的<amerce>元素中的价格price属性。
        // 为功能"modifyprice"服务。
        int ModifyPrice(ref XmlDocument readerdom,
            AmerceItem[] amerce_items,
            out string strError)
        {
            strError = "";
            int nChangedCount = 0;

            for (int i = 0; i < amerce_items.Length; i++)
            {
                AmerceItem item = amerce_items[i];

                // 遇到NewPrice域值为空的，直接跳过。
                // 这说明，不接受修改价格为完全空的字符串。
                if (String.IsNullOrEmpty(item.NewPrice) == true)
                {
                    if (String.IsNullOrEmpty(item.NewComment) == false)
                    {
                        strError = "不能用modifyprice子功能来单独修改注释(而不修改价格)，请改用appendcomment和modifycomment子功能";
                        return -1;
                    }

                    continue;
                }

                // 通过id值在读者记录中找到对应的<overdue>元素
                XmlNode nodeOverdue = readerdom.DocumentElement.SelectSingleNode("overdues/overdue[@id='" + item.ID + "']");
                if (nodeOverdue == null)
                {
                    strError = "ID为 '" + item.ID + "' 的<overdues/overdue>元素没有找到...";
                    return -1;
                }

                string strOldPrice = DomUtil.GetAttr(nodeOverdue, "price");

                if (strOldPrice != item.NewPrice)
                {
                    // 修改price属性
                    DomUtil.SetAttr(nodeOverdue, "price", item.NewPrice);
                    nChangedCount++;

                    // 增补注释
                    string strNewComment = item.NewComment;
                    string strExistComment = DomUtil.GetAttr(nodeOverdue, "comment");

                    // 处理追加标志
                    bool bAppend = true;
                    if (string.IsNullOrEmpty(strNewComment) == false
                        && strNewComment[0] == '<')
                    {
                        bAppend = false;
                        strNewComment = strNewComment.Substring(1);
                    }
                    else if (string.IsNullOrEmpty(strNewComment) == false
                        && strNewComment[0] == '>')
                    {
                        bAppend = true;
                        strNewComment = strNewComment.Substring(1);
                    }

                    if (String.IsNullOrEmpty(strNewComment) == false
                        && bAppend == true)
                    {
                        string strText = "";
                        if (String.IsNullOrEmpty(strExistComment) == false)
                            strText += strExistComment;
                        if (String.IsNullOrEmpty(strNewComment) == false)
                        {
                            if (String.IsNullOrEmpty(strText) == false)
                                strText += "；";
                            strText += strNewComment;
                        }

                        DomUtil.SetAttr(nodeOverdue, "comment", strText);
                    }
                    else if (bAppend == false)
                    {
                        DomUtil.SetAttr(nodeOverdue, "comment", strNewComment);
                    }
                }
            }

            return nChangedCount;
        }

        // 2008/6/19
        // 根据AmerceItem数组，修改readerdom中的<amerce>元素中的comment属性。
        // 为功能"modifycomment"服务。
        int ModifyComment(
            ref XmlDocument readerdom,
            AmerceItem[] amerce_items,
            out string strError)
        {
            strError = "";
            int nChangedCount = 0;

            for (int i = 0; i < amerce_items.Length; i++)
            {
                AmerceItem item = amerce_items[i];

                // 不能同时修改价格。
                if (String.IsNullOrEmpty(item.NewPrice) == false)
                {
                    strError = "不能用modifycomment子功能来修改价格，请改用modifyprice子功能";
                    return -1;
                }

                /*
                // 遇到NewComment域值为空、并且为追加的，直接跳过
                if (String.IsNullOrEmpty(item.NewComment) == true
                    && strFunction == "appendcomment")
                {
                    continue;
                }*/

                // 通过id值在读者记录中找到对应的<overdue>元素
                XmlNode nodeOverdue = readerdom.DocumentElement.SelectSingleNode("overdues/overdue[@id='" + item.ID + "']");
                if (nodeOverdue == null)
                {
                    strError = "ID为 '" + item.ID + "' 的<overdues/overdue>元素没有找到...";
                    return -1;
                }


                {
                    string strExistComment = DomUtil.GetAttr(nodeOverdue, "comment");

                    // 增补或修改注释
                    string strNewComment = item.NewComment;

                    // 处理追加标志
                    bool bAppend = true;
                    if (string.IsNullOrEmpty(strNewComment) == false
                        && strNewComment[0] == '<')
                    {
                        bAppend = false;
                        strNewComment = strNewComment.Substring(1);
                    }
                    else if (string.IsNullOrEmpty(strNewComment) == false
                        && strNewComment[0] == '>')
                    {
                        bAppend = true;
                        strNewComment = strNewComment.Substring(1);
                    }

                    if (String.IsNullOrEmpty(strNewComment) == false
                        && bAppend == true)
                    {
                        string strText = "";
                        if (String.IsNullOrEmpty(strExistComment) == false)
                            strText += strExistComment;
                        if (String.IsNullOrEmpty(strNewComment) == false)
                        {
                            if (String.IsNullOrEmpty(strText) == false)
                                strText += "；";
                            strText += strNewComment;
                        }

                        DomUtil.SetAttr(nodeOverdue, "comment", strText);
                        nChangedCount++;
                    }
                    else if (bAppend == false)
                    {
                        DomUtil.SetAttr(nodeOverdue, "comment", strNewComment);
                        nChangedCount++;    // BUG!!! 2011/12/1前少了这句话
                    }
                }
            }

            return nChangedCount;
        }

        // 从日志DOM中读出违约金事项信息
        public static AmerceItem[] ReadAmerceItemList(XmlDocument domOperLog)
        {
            XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("amerceItems/amerceItem");
            AmerceItem[] results = new AmerceItem[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strID = DomUtil.GetAttr(node, "id");
                string strNewPrice = DomUtil.GetAttr(node, "newPrice");
                string strComment = DomUtil.GetAttr(node, "newComment");

                results[i] = new AmerceItem();
                results[i].ID = strID;
                results[i].NewPrice = strNewPrice;
                results[i].NewComment = strComment;    // 2007/4/17
            }

            return results;
        }

        // 在日志DOM中写入违约金事项信息
        static void WriteAmerceItemList(XmlDocument domOperLog,
            AmerceItem[] amerce_items)
        {
            XmlNode root = domOperLog.CreateElement("amerceItems");
            domOperLog.DocumentElement.AppendChild(root);

            for (int i = 0; i < amerce_items.Length; i++)
            {
                AmerceItem item = amerce_items[i];

                XmlNode node = domOperLog.CreateElement("amerceItem");
                root.AppendChild(node);

                DomUtil.SetAttr(node, "id", item.ID);

                if (String.IsNullOrEmpty(item.NewPrice) == false)
                    DomUtil.SetAttr(node, "newPrice", item.NewPrice);

                // 2007/4/17
                if (String.IsNullOrEmpty(item.NewComment) == false)
                    DomUtil.SetAttr(node, "newComment", item.NewComment);

            }

            /*

            // id list
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "idList", strAmerceItemIdList);
            */
        }

        // 在册记录中获得借阅者证条码号
        // return:
        //      -1  出错
        //      0   该册为未借出状态
        //      1   成功
        static int GetBorrowerBarcode(XmlDocument dom,
            out string strOutputReaderBarcode,
            out string strError)
        {
            strOutputReaderBarcode = "";
            strError = "";

            strOutputReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "borrower");

            if (String.IsNullOrEmpty(strOutputReaderBarcode) == true)
            {
                strError = "该册为未借出状态";   // "册记录中<borrower>元素值表明该册并未曾被任何读者所借阅过";
                return 0;
            }

            return 1;
        }


        // 删除刚刚创建的新违约金记录
        int DeleteAmerceRecords(
            RmsChannelCollection channels,
            List<string> CreatedNewPaths,
            out string strError)
        {
            strError = "";

            RmsChannel channel = channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            for (int i = 0; i < CreatedNewPaths.Count; i++)
            {
                string strPath = CreatedNewPaths[i];

                byte[] timestamp = null;
                byte[] output_timestamp = null;
                int nRedoCount = 0;
            REDO:

                long lRet = channel.DoDeleteRes(strPath,
                    timestamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                        && nRedoCount < 5) // 重试次数小于5次
                    {
                        timestamp = output_timestamp;
                        nRedoCount++;
                        goto REDO;
                    }

                    return -1;
                }

            }


            return 0;
        }

        // 在违约金数据库中创建若干新的违约金记录
        // parameters:
        //      AmerceRecordXmls    需要写入的新记录的数组
        //      CreatedNewPaths 已经创建的新记录的路径数组。可以用于Undo(删除刚刚创建的新记录)
        int CreateAmerceRecords(
            // RmsChannelCollection channels,
            RmsChannel channel,
            List<string> AmerceRecordXmls,
            out List<string> CreatedNewPaths,
            out string strError)
        {
            strError = "";
            CreatedNewPaths = new List<string>();
            long lRet = 0;

            if (string.IsNullOrEmpty(this.AmerceDbName) == true)
            {
                strError = "尚未配置违约金库名";
                return -1;
            }

#if NO
            RmsChannel channel = channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            for (int i = 0; i < AmerceRecordXmls.Count; i++)
            {
                string strXml = AmerceRecordXmls[i];

                string strPath = this.AmerceDbName + "/?";

                string strOutputPath = "";
                byte[] timestamp = null;
                byte[] output_timestamp = null;

                // 写新记录
                lRet = channel.DoSaveTextRes(
                    strPath,
                    strXml,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    return -1;

                CreatedNewPaths.Add(strOutputPath);
            }

            return 0;
        }

        // 取出违约金记录中的金额数字
        static int GetAmerceRecordPrice(string strAmercedXml,
            out string strPrice,
            out string strError)
        {
            strPrice = "";
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strAmercedXml);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                return -1;
            }

            strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            return 0;
        }

        // 将违约金记录格式转换为读者记录中的<overdue>元素格式
        // return:
        //      -1  error
        //      0   strAmercedXml中<state>元素的值为*非*"settlemented"
        //      1   strAmercedXml中<state>元素的值为"settlemented"
        public static int ConvertAmerceRecordToOverdueString(string strAmercedXml,
            out string strReaderBarcode,
            out string strOverdueString,
            out string strError)
        {
            strReaderBarcode = "";
            strOverdueString = "";
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strAmercedXml);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                return -1;
            }

            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "itemBarcode");
            string strItemRecPath = DomUtil.GetElementText(dom.DocumentElement,
                "itemRecPath");

            strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "readerBarcode");
            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");

            string strID = DomUtil.GetElementText(dom.DocumentElement,
                "id");
            string strReason = DomUtil.GetElementText(dom.DocumentElement,
                "reason");

            // 2007/12/17
            string strOverduePeriod = DomUtil.GetElementText(dom.DocumentElement,
                "overduePeriod");

            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            string strOriginPrice = DomUtil.GetElementText(dom.DocumentElement,
                "originPrice");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");

            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement,
                "borrowDate");
            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
                "borrowPeriod");
            string strBorrowOperator = DomUtil.GetElementText(dom.DocumentElement,
                "borrowOperator");  // 2006/3/27

            string strReturnDate = DomUtil.GetElementText(dom.DocumentElement,
                "returnDate");
            string strReturnOperator = DomUtil.GetElementText(dom.DocumentElement,
                "returnOperator");

            // 2008/6/23
            string strPauseStart = DomUtil.GetElementText(dom.DocumentElement,
                "pauseStart");

            // 写入DOM
            XmlDocument domOutput = new XmlDocument();
            domOutput.LoadXml("<overdue />");
            XmlNode nodeOverdue = domOutput.DocumentElement;

            DomUtil.SetAttr(nodeOverdue, "barcode", strItemBarcode);
            if (String.IsNullOrEmpty(strItemRecPath) == false)
                DomUtil.SetAttr(nodeOverdue, "recPath", strItemRecPath);

            DomUtil.SetAttr(nodeOverdue, "reason", strReason);

            // 2007/12/17
            if (String.IsNullOrEmpty(strOverduePeriod) == false)
                DomUtil.SetAttr(nodeOverdue, "overduePeriod", strOverduePeriod);

            if (String.IsNullOrEmpty(strOriginPrice) == false)
            {
                DomUtil.SetAttr(nodeOverdue, "price", strOriginPrice);
                DomUtil.SetAttr(nodeOverdue, "newPrice", strPrice);
            }
            else
                DomUtil.SetAttr(nodeOverdue, "price", strPrice);

            // 撤回的时候不丢失注释。因为已经无法分辨哪次追加的注释，所以原样保留。
            // 2007/4/19
            if (String.IsNullOrEmpty(strComment) == false)
                DomUtil.SetAttr(nodeOverdue, "comment", strComment);

            // TODO: 这里值得研究一下。如果AmerceItem.Comment能覆盖数据中的comment信息，
            // 那么撤回的时候就不要丢失注释。

            DomUtil.SetAttr(nodeOverdue, "borrowDate", strBorrowDate);
            DomUtil.SetAttr(nodeOverdue, "borrowPeriod", strBorrowPeriod);
            DomUtil.SetAttr(nodeOverdue, "returnDate", strReturnDate);
            DomUtil.SetAttr(nodeOverdue, "borrowOperator", strBorrowOperator);
            DomUtil.SetAttr(nodeOverdue, "operator", strReturnOperator);
            DomUtil.SetAttr(nodeOverdue, "id", strID);

            // 2008/6/23
            if (String.IsNullOrEmpty(strPauseStart) == false)
                DomUtil.SetAttr(nodeOverdue, "pauseStart", strPauseStart);

            strOverdueString = nodeOverdue.OuterXml;

            if (strState == "settlemented")
                return 1;

            return 0;
        }

        // 将读者记录中的<overdue>元素和属性转换为违约金库的记录格式
        // parameters:
        //      strLibraryCode  读者记录从属的馆代码
        //      strState    一般为"amerced"，表示尚未结算
        //      strNewPrice 例外的价格。如果为空，则表示沿用原来的价格。
        //      strComment  前端给出的注释。
        static int ConvertOverdueStringToAmerceRecord(XmlNode nodeOverdue,
            string strLibraryCode,
            string strReaderBarcode,
            string strState,
            string strNewPrice,
            string strNewComment,
            string strOperator,
            string strOperTime,
            string strForegiftPrice,    // 来自读者记录<foregift>元素内的价格字符串
            out string strFinalPrice,   // 最终使用的价格字符串
            out string strAmerceRecord,
            out string strError)
        {
            strAmerceRecord = "";
            strError = "";
            strFinalPrice = "";
            int nRet = 0;


            string strItemBarcode = DomUtil.GetAttr(nodeOverdue, "barcode");
            string strItemRecPath = DomUtil.GetAttr(nodeOverdue, "recPath");
            string strReason = DomUtil.GetAttr(nodeOverdue, "reason");

            // 2007/12/17
            string strOverduePeriod = DomUtil.GetAttr(nodeOverdue, "overduePeriod");

            string strPrice = "";
            string strOriginPrice = "";

            if (String.IsNullOrEmpty(strNewPrice) == true)
                strPrice = DomUtil.GetAttr(nodeOverdue, "price");
            else
            {
                strPrice = strNewPrice;
                strOriginPrice = DomUtil.GetAttr(nodeOverdue, "price");
            }

            // 2008/11/15
            // 看看价格字符串是否为宏?
            if (strPrice == "%return_foregift_price%")
            {
                // 记忆下取宏的变化
                if (String.IsNullOrEmpty(strOriginPrice) == true)
                    strOriginPrice = strPrice;

                // 将形如"-123.4+10.55-20.3"的价格字符串反转正负号
                // parameters:
                //      bSum    是否要顺便汇总? true表示要汇总
                nRet = PriceUtil.NegativePrices(strForegiftPrice,
                    true,
                    out strPrice,
                    out strError);
                if (nRet == -1)
                {
                    strError = "反转(来自读者记录中的<foregift>元素的)价格字符串 '" + strForegiftPrice + "' 时出错: " + strError;
                    return -1;
                }

                // 如果经过反转后的价格字符串为空，则需要特别替换为“0”，以免后面环节被当作没有值的空字符串。负号是有意义的，表示退款(而不是交款)哟
                if (String.IsNullOrEmpty(strPrice) == true)
                    strPrice = "-0";

            }

            if (strPrice.IndexOf('%') != -1)
            {
                strError = "价格字符串 '" + strPrice + "' 格式错误：除了使用宏%return_foregift_price%以外，价格字符串中不允许出现%符号";
                return -1;
            }

            strFinalPrice = strPrice;

            string strBorrowDate = DomUtil.GetAttr(nodeOverdue, "borrowDate");
            string strBorrowPeriod = DomUtil.GetAttr(nodeOverdue, "borrowPeriod");
            string strReturnDate = DomUtil.GetAttr(nodeOverdue, "returnDate");
            string strBorrowOperator = DomUtil.GetAttr(nodeOverdue, "borrowOperator");
            string strReturnOperator = DomUtil.GetAttr(nodeOverdue, "operator");
            string strID = DomUtil.GetAttr(nodeOverdue, "id");
            string strExistComment = DomUtil.GetAttr(nodeOverdue, "comment");

            // 2008/6/23
            string strPauseStart = DomUtil.GetAttr(nodeOverdue, "pauseStart");

            // 写入DOM
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            DomUtil.SetElementText(dom.DocumentElement,
                "itemBarcode", strItemBarcode);

            if (String.IsNullOrEmpty(strItemRecPath) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement,
    "itemRecPath", strItemRecPath);
            }

            DomUtil.SetElementText(dom.DocumentElement,
                "readerBarcode", strReaderBarcode);

            // 2012/9/15
            DomUtil.SetElementText(dom.DocumentElement,
    "libraryCode", strLibraryCode);

            DomUtil.SetElementText(dom.DocumentElement,
                "state", strState);
            DomUtil.SetElementText(dom.DocumentElement,
                "id", strID);
            DomUtil.SetElementText(dom.DocumentElement,
                "reason", strReason);

            // 2007/12/17
            if (String.IsNullOrEmpty(strOverduePeriod) == false)
                DomUtil.SetElementText(dom.DocumentElement,
                    "overduePeriod", strOverduePeriod);

            DomUtil.SetElementText(dom.DocumentElement,
                "price", strPrice);
            if (String.IsNullOrEmpty(strOriginPrice) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "originPrice", strOriginPrice);
            }

            // 2008/6/25
            {
                bool bAppend = true;
                if (string.IsNullOrEmpty(strNewComment) == false
                    && strNewComment[0] == '<')
                {
                    bAppend = false;
                    strNewComment = strNewComment.Substring(1);
                }
                else if (string.IsNullOrEmpty(strNewComment) == false
                    && strNewComment[0] == '>')
                {
                    bAppend = true;
                    strNewComment = strNewComment.Substring(1);
                }

                if (bAppend == true)
                {
                    string strText = "";
                    if (String.IsNullOrEmpty(strExistComment) == false)
                        strText += strExistComment;
                    if (String.IsNullOrEmpty(strNewComment) == false)
                    {
                        if (String.IsNullOrEmpty(strText) == false)
                            strText += "；";
                        strText += strNewComment;
                    }

                    DomUtil.SetElementText(dom.DocumentElement,
                        "comment",
                        strText);
                }
                else
                {
                    Debug.Assert(bAppend == false, "");

                    DomUtil.SetElementText(dom.DocumentElement,
                        "comment",
                        strNewComment);
                }
            }

            /*
            if (String.IsNullOrEmpty(strNewComment) == false
                || String.IsNullOrEmpty(strExistComment) == false)
            {
                string strText = "";
                if (String.IsNullOrEmpty(strExistComment) == false)
                    strText += strExistComment;
                if (String.IsNullOrEmpty(strNewComment) == false)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += "；";
                    strText += strNewComment;
                }

                // 2008/6/25 从SetElementInnerXml()修改而来
                DomUtil.SetElementText(dom.DocumentElement,
                    "comment",
                    strText);
            }
             * */

            DomUtil.SetElementText(dom.DocumentElement,
                "borrowDate", strBorrowDate);
            DomUtil.SetElementText(dom.DocumentElement,
                "borrowPeriod", strBorrowPeriod);
            DomUtil.SetElementText(dom.DocumentElement,
                "borrowOperator", strBorrowOperator);   // 2006/3/27

            DomUtil.SetElementText(dom.DocumentElement,
                "returnDate", strReturnDate);
            DomUtil.SetElementText(dom.DocumentElement,
                "returnOperator", strReturnOperator);

            DomUtil.SetElementText(dom.DocumentElement,
                "operator", strOperator);   // 罚金操作者
            DomUtil.SetElementText(dom.DocumentElement,
                "operTime", strOperTime);

            // 2008/6/23
            if (String.IsNullOrEmpty(strPauseStart) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "pauseStart", strPauseStart);
            }


            strAmerceRecord = dom.OuterXml;

            return 0;
        }

        // 交违约金：在读者记录中去除所选的<overdue>元素，并且构造一批新记录准备加入违约金库
        // parameters:
        //      strLibraryCode  读者记录从属的馆代码
        // return:
        //      -1  error
        //      0   读者dom没有变化
        //      1   读者dom发生了变化
        static int DoAmerceReaderXml(
            string strLibraryCode,
            ref XmlDocument readerdom,
            AmerceItem[] amerce_items,
            string strOperator,
            string strOperTimeString,
            out List<string> AmerceRecordXmls,
            out List<string> NotFoundIds,
            out List<string> Ids,
            out string strError)
        {
            strError = "";
            AmerceRecordXmls = new List<string>();
            NotFoundIds = new List<string>();
            Ids = new List<string>();
            int nRet = 0;

            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");
            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                strError = "读者记录中竟然没有<barcode>元素值";
                return -1;
            }

            bool bChanged = false;  // 读者dom是否发生了改变

            // string strNotFoundIds = "";

            for (int i = 0; i < amerce_items.Length; i++)
            {
                AmerceItem item = amerce_items[i];

                // string strID = ids[i].Trim();
                if (String.IsNullOrEmpty(item.ID) == true)
                    continue;

                XmlNode node = readerdom.DocumentElement.SelectSingleNode("overdues/overdue[@id='" + item.ID + "']");
                if (node == null)
                {
                    NotFoundIds.Add(item.ID);

                    /*
                    if (strNotFoundIds != "")
                        strNotFoundIds += ",";
                    strNotFoundIds += item.ID;
                     * */
                    continue;
                }

                string strForegiftPrice = DomUtil.GetElementText(readerdom.DocumentElement,
                    "foregift");

                string strFinalPrice = "";  // 最终使用的价格字符串。这是从item.NewPrice和node节点的price属性中选择出来，并且经过去除宏操作的一个最后价格字符串
                string strAmerceRecord = "";
                // 将读者记录中的<overdue>元素和属性转换为违约金库的记录格式
                nRet = ConvertOverdueStringToAmerceRecord(node,
                    strLibraryCode,
                    strReaderBarcode,
                    "amerced",
                    item.NewPrice,
                    item.NewComment,
                    strOperator,
                    strOperTimeString,
                    strForegiftPrice,
                    out strFinalPrice,
                    out strAmerceRecord,
                    out strError);
                if (nRet == -1)
                    return -1;

                AmerceRecordXmls.Add(strAmerceRecord);

                Ids.Add(item.ID);

                // 如果是押金，需要增/减<foregift>元素内的价格值。交费为增，退费为减。不过正负号已经含在价格字符串中，可以都理解为交费
                string strReason = "";
                strReason = DomUtil.GetAttr(node, "reason");

                // 2008/11/11
                if (strReason == "押金。")
                {
                    string strNewPrice = "";

                    /*
                    string strOldPrice = DomUtil.GetElementText(readerdom.DocumentElement,
                        "foregift");

                    if (strOldPrice.IndexOf('%') != -1)
                    {
                        strError = "来自读者记录<foregift>元素的价格字符串 '" + strOldPrice + "' 格式错误：价格字符串中不允许出现%符号";
                        return -1;
                    }

                    string strPrice = "";

                    if (String.IsNullOrEmpty(item.NewPrice) == true)
                        strPrice = DomUtil.GetAttr(node, "price");
                    else
                        strPrice = item.NewPrice;

                    // 看看价格字符串是否为宏?
                    if (strPrice == "%return_foregift_price%")
                    {
                        // 将形如"-123.4+10.55-20.3"的价格字符串反转正负号
                        // parameters:
                        //      bSum    是否要顺便汇总? true表示要汇总
                        nRet = PriceUtil.NegativePrices(strOldPrice,
                            true,
                            out strPrice,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "反转(来自读者记录中的<foregift>元素的)价格字符串 '" + strOldPrice + "' 时出错: " + strError;
                            return -1;
                        }
                    }

                    if (strPrice.IndexOf('%') != -1)
                    {
                        strError = "价格字符串 '" + strPrice + "' 格式错误：除了使用宏%return_foregift_price%以外，价格字符串中不允许出现%符号";
                        return -1;
                    }

                    if (String.IsNullOrEmpty(strOldPrice) == false)
                    {
                        strNewPrice = PriceUtil.JoinPriceString(strOldPrice, strPrice);
                    }
                    else
                    {
                        strNewPrice = strPrice;
                    }
                     * */
                    if (String.IsNullOrEmpty(strForegiftPrice) == false)
                    {
                        strNewPrice = PriceUtil.JoinPriceString(strForegiftPrice, strFinalPrice);
                    }
                    else
                    {
                        strNewPrice = strFinalPrice;
                    }


                    DomUtil.SetElementText(readerdom.DocumentElement,
                        "foregift",
                        strNewPrice);

                    // 是否顺便写入最近一次的交费时间?
                    bChanged = true;
                }

                // 在读者记录中删除这个节点
                node.ParentNode.RemoveChild(node);
                bChanged = true;
            }

            /*
            if (strNotFoundIds != "")
            {
                strError = "下列id没有相匹配的<overdue>元素" + strNotFoundIds;
                return -1;
            }*/
            if (NotFoundIds.Count > 0)
            {
                strError = "下列id没有相匹配的<overdue>元素: " + StringUtil.MakePathList(NotFoundIds);
                return -1;
            }


            if (bChanged == true)
                return 1;
            return 0;
        }
        /*
        // 是否存在以停代金事项？
        static bool InPauseBorrowing(XmlDocument readerdom,
            out string strMessage)
        {
            strMessage = "";

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
            if (nodes.Count == 0)
                return false;

            XmlNode node = null;
            int nTotalCount = 0;

            string strPauseStart = "";

            // 计算以停代金事项总数目
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                if (String.IsNullOrEmpty(strPauseStart) == false)
                    nTotalCount++;
            }

            // 找到第一个已启动事项
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                if (String.IsNullOrEmpty(strPauseStart) == false)
                    goto FOUND;
            }

            if (nTotalCount > 0)
            {
                strMessage = "有未启动的 " + nTotalCount.ToString() + " 项以停代金事项";
                return true;
            }


            return false;   // 没有找到已启动的事项
        FOUND:
            string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
            strMessage = "有一项于 " + DateTimeUtil.LocalDate(strPauseStart) + " 开始的，为期 " + strOverduePeriod + " 的以停代金过程";

            if (nTotalCount > 1)
                strMessage += "(此外还有未启动的 "+(nTotalCount-1).ToString()+" 项)";

            return true;
        }
         * */

        // 为了兼容以前的版本。除了在校本中使用外，尽量不要使用了
        // 计算以停代金的停借周期值
        public int ComputePausePeriodValue(string strReaderType,
            long lValue,
            out long lResultValue,
            out string strPauseCfgString,
            out string strError)
        {
            return ComputePausePeriodValue(strReaderType,
                "",
                lValue,
                out lResultValue,
                out strPauseCfgString,
                out strError);
        }

        // 计算以停代金的停借周期值
        public int ComputePausePeriodValue(string strReaderType,
            string strLibraryCode,
            long lValue,
            out long lResultValue,
            out string strPauseCfgString,
            out string strError)
        {
            strError = "";
            strPauseCfgString = "1.0";
            lResultValue = lValue;

            // 获得 '以停代金因子' 配置参数
            MatchResult matchresult;
            // return:
            //      reader和book类型均匹配 算4分
            //      只有reader类型匹配，算3分
            //      只有book类型匹配，算2分
            //      reader和book类型都不匹配，算1分
            int nRet = this.GetLoanParam(
                //null,
                strLibraryCode,
                strReaderType,
                "",
                "以停代金因子",
                out strPauseCfgString,
                out matchresult,
                out strError);
            if (nRet == -1)
            {
                strError = "获得 馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 的 以停代金因子 参数时发生错误: " + strError;
                return -1;
            }

            if (nRet < 3 || string.IsNullOrEmpty(strPauseCfgString) == true)
            {
                // 没有找到匹配读者类型的定义，则按照 1.0 计算
                strPauseCfgString = "1.0";
                return 0;
            }

            double ratio = 1.0;

            try
            {
                ratio = Convert.ToDouble(strPauseCfgString);
            }
            catch
            {
                strError = "以停代金因子 配置字符串 '" + strPauseCfgString + "' 格式错误。应该为一个小数。";
                return -1;
            }

            lResultValue = (long)((double)lValue * ratio);
            return 1;
        }

        // 包装版本，为了兼容以前脚本。一次代码中不要使用这个函数
        public int HasPauseBorrowing(
    Calendar calendar,
    XmlDocument readerdom,
    out string strMessage,
    out string strError)
        {
            return HasPauseBorrowing(
                calendar,
                "",
                readerdom,
                out strMessage,
                out strError);
        }

        // 是否存在以停代金事项？
        // text-level: 用户提示
        // return:
        //      -1  error
        //      0   不存在
        //      1   存在
        public int HasPauseBorrowing(
            Calendar calendar,
            string strLibraryCode,
            XmlDocument readerdom,
            out string strMessage,
            out string strError)
        {
            strError = "";
            strMessage = "";

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
            if (nodes.Count == 0)
                return 0;

            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                "readerType");

            int nRet = 0;
            XmlNode node = null;
            int nTotalCount = 0;

            string strFirstPauseStart = "";


            // 找到第一个已启动事项
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                string strPauseStart = "";
                strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                if (String.IsNullOrEmpty(strPauseStart) == false)
                {
                    // 2008/1/16 修正：
                    // 如果有pauseStart属性，但是没有overduePeriod属性，属于格式错误，
                    // 需要接着向后寻找格式正确的第一项
                    string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
                    if (String.IsNullOrEmpty(strOverduePeriod) == true)
                    {
                        strPauseStart = "";
                        continue;
                    }

                    strFirstPauseStart = strPauseStart;
                    break;
                }
            }

            long lTotalOverduePeriod = 0;
            string strTotalUnit = "";

            // 遍历以停代金事项，计算时程总长度和最后结束日期
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
                if (String.IsNullOrEmpty(strOverduePeriod) == true)
                    continue;

                string strUnit = "";
                long lOverduePeriod = 0;

                // 分析期限参数
                nRet = ParsePeriodUnit(strOverduePeriod,
                    out lOverduePeriod,
                    out strUnit,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (strTotalUnit == "")
                    strTotalUnit = strUnit;
                else
                {
                    if (strTotalUnit != strUnit)
                    {
                        // 出现了时间单位的不一致
                        if (strTotalUnit == "day" && strUnit == "hour")
                            lOverduePeriod = lOverduePeriod / 24;
                        else if (strTotalUnit == "hour" && strUnit == "day")
                            lOverduePeriod = lOverduePeriod * 24;
                        else
                        {
                            // text-level: 内部错误
                            strError = "时间单位 '" + strUnit + "' 和前面曾用过的时间单位 '" + strTotalUnit + "' 不一致，无法进行加法运算";
                            return -1;
                        }

                    }
                }

                long lResultValue = 0;
                string strPauseCfgString = "";
                // 计算以停代金的停借周期值
                nRet = ComputePausePeriodValue(strReaderType,
                    strLibraryCode,
                    lOverduePeriod,
                    out lResultValue,
                    out strPauseCfgString,
                    out strError);
                if (nRet == -1)
                    return -1;


                lTotalOverduePeriod += lResultValue;    //  lOverduePeriod;

                nTotalCount++;
            }

            // 2008/1/16 changed strPauseStart -->strFirstPauseStart
            if (String.IsNullOrEmpty(strFirstPauseStart) == true)
            {
                if (nTotalCount > 0)
                {
                    // text-level: 用户提示
                    strMessage = string.Format(this.GetString("有s项未启动的以停代金事项"), // "有 {0} 项未启动的以停代金事项"
                        nTotalCount.ToString());
                    // "有 " + nTotalCount.ToString() + " 项未启动的以停代金事项";
                    return 1;
                }

                return 0;
            }

            DateTime pause_start;
            try
            {
                pause_start = DateTimeUtil.FromRfc1123DateTimeString(strFirstPauseStart);
            }
            catch
            {
                // text-level: 内部错误
                strError = "停借开始日期 '" + strFirstPauseStart + "' 格式错误";
                return -1;
            }

            DateTime timeEnd;   // 以停代金整个的结束日期
            DateTime nextWorkingDay;

            // 测算还书日期
            // parameters:
            //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
            // return:
            //      -1  出错
            //      0   成功。timeEnd在工作日范围内。
            //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
            nRet = GetReturnDay(
                calendar,
                pause_start,
                lTotalOverduePeriod,
                strTotalUnit,
                out timeEnd,
                out nextWorkingDay,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "测算以停代金结束日期过程发生错误: " + strError;
                return -1;
            }

            bool bEndInNonWorkingDay = false;
            if (nRet == 1)
            {
                // end在非工作日
                bEndInNonWorkingDay = true;
            }

            DateTime now_rounded = this.Clock.UtcNow;  //  今天

            // 正规化时间
            nRet = RoundTime(strTotalUnit,
                ref now_rounded,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta = now_rounded - timeEnd;

            long lDelta = 0;
            nRet = ParseTimeSpan(
                delta,
                strTotalUnit,
                out lDelta,
                out strError);
            if (nRet == -1)
                return -1;

            if (strTotalUnit == "hour")
            {
                // text-level: 用户提示
                strMessage = string.Format(this.GetString("共有s项以停代金事项，从s开始，总计应暂停借阅s, 于s结束"),
                    // "共有 {0} 项以停代金事项，从 {1} 开始，总计应暂停借阅 {2}, 于 {3} 结束。"
                    nTotalCount.ToString(),
                    pause_start.ToString("s"),
                    lTotalOverduePeriod.ToString() + GetDisplayTimeUnitLang(strTotalUnit),
                    timeEnd.ToString("s"));
                // "共有 " + nTotalCount.ToString() + " 项以停代金事项，从 " + pause_start.ToString("s") + " 开始，总计应暂停借阅 " + lTotalOverduePeriod.ToString() + GetDisplayTimeUnitLang(strTotalUnit) + ", 于 " + timeEnd.ToString("s") + " 结束。";
            }
            else
            {
                // text-level: 用户提示

                strMessage = string.Format(this.GetString("共有s项以停代金事项，从s开始，总计应暂停借阅s, 于s结束"),
                    // "共有 {0} 项以停代金事项，从 {1} 开始，总计应暂停借阅 {2}, 于 {3} 结束。"
                    nTotalCount.ToString(),
                    pause_start.ToString("d"),  // "yyyy-MM-dd"
                    lTotalOverduePeriod.ToString() + GetDisplayTimeUnitLang(strTotalUnit),
                    timeEnd.ToString("d")); // "yyyy-MM-dd"
                // "共有 " + nTotalCount.ToString() + " 项以停代金事项，从 " + pause_start.ToString("yyyy-MM-dd") + " 开始，总计应暂停借阅 " + lTotalOverduePeriod.ToString() + GetDisplayTimeUnitLang(strTotalUnit) + ", 于 " + timeEnd.ToString("yyyy-MM-dd") + " 结束。";
            }

            if (lDelta > 0)
            {
                // text-level: 用户提示
                strMessage += this.GetString("到当前时刻，上述整个以停代金周期已经结束。"); // "到当前时刻，上述整个以停代金周期已经结束。"
            }


            return 1;
        }

        // 处理以停代金功能
        // TODO: 如果本函数被日志恢复程序调用，则其内部采用UtcNow作为当前时间就是不正确的。应当是日志中记载的借阅当时时间
        // TODO: 写入日志的同时，也需要写入<overdues>元素内一个说明性的位置，便于随时查对
        // parameters:
        //      strReaderRecPath    当strAction为"refresh"时，需要给这个参数内容。以便写入日志。
        // return:
        //      -1  error
        //      0   readerdom没有修改
        //      1   readerdom发生了修改
        public int ProcessPauseBorrowing(
            string strLibraryCode,
            ref XmlDocument readerdom,
            string strReaderRecPath,
            string strUserID,
            string strAction,
            string strClientAddress,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 启动
            if (strAction == "start")
            {
                XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
                if (nodes.Count == 0)
                    return 0;

                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    string strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                    if (String.IsNullOrEmpty(strPauseStart) == false)
                        return 0;   // 已经有启动了的事项，不必再启动
                }

                // 2008/1/16 changed
                // 寻找第一个具有overduePeriod属性值的事项，设置为启动
                bool bFound = false;
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
                    if (String.IsNullOrEmpty(strOverduePeriod) == false)
                    {
                        // 把第一个具有overduePeriod属性值的事项设置为启动
                        DomUtil.SetAttr(node, "pauseStart", this.Clock.GetClock());
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                    return 0;   // 没有找到具有overduePeriod属性值的事项

                // 写入统计指标
                // 启动事项数，而不是读者个数
                if (this.Statis != null)
                    this.Statis.IncreaseEntryValue(
                    strLibraryCode,
                    "出纳",
                    "以停代金事项启动",
                    1);

                // TODO: 创建事件日志，记录启动事项的动作
                return 1;
            }

            // 刷新
            if (strAction == "refresh")
            {
                if (String.IsNullOrEmpty(strReaderRecPath) == true)
                {
                    strError = "refresh时必须提供strReaderRecPath参数值，否则无法创建日志记录";
                    return -1;
                }

                int nExpiredCount = 0;

                string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                    "barcode");
                string strOldReaderXml = readerdom.OuterXml;

                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "operation",
                    "amerce");
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "action",
                    "expire");
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "readerBarcode",
                    strReaderBarcode);

                XmlNode node_expiredOverdues = domOperLog.CreateElement("expiredOverdues");
                domOperLog.DocumentElement.AppendChild(node_expiredOverdues);

                string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
    "readerType");

                bool bChanged = false;

                for (; ; )
                {
                    XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
                    if (nodes.Count == 0)
                        break;

                    // 找到第一个已启动事项
                    XmlNode node = null;
                    string strPauseStart = "";
                    XmlNode node_firstOverdueItem = null;   // 第一项符合超期条件(但不一定启动了的)的<overdue>元素
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        node = nodes[i];
                        string strTempOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
                        if (String.IsNullOrEmpty(strTempOverduePeriod) == true)
                            continue;   // 忽略那些没有overduePeriod的元素

                        if (node_firstOverdueItem == null)
                            node_firstOverdueItem = node;
                        strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                        if (String.IsNullOrEmpty(strPauseStart) == false)
                            goto FOUND;
                    }

                    // 没有找到已启动的事项，则需要把第一个符合条件的事项启动
                    if (node_firstOverdueItem != null)
                    {
                        DomUtil.SetAttr(node_firstOverdueItem,
                            "pauseStart",
                            this.Clock.GetClock());
                        bChanged = true;
                        continue;   // 重新执行刷新操作似乎没有必要，因为没有刚开始就立即结束的？
                    }
                    break;
                FOUND:
                    string strUnit = "";
                    long lOverduePeriod = 0;

                    string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");

                    // 分析期限参数
                    nRet = ParsePeriodUnit(strOverduePeriod,
                        out lOverduePeriod,
                        out strUnit,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    long lResultValue = 0;
                    string strPauseCfgString = "";

                    // 计算以停代金的停借周期值
                    nRet = ComputePausePeriodValue(strReaderType,
                        strLibraryCode,
                        lOverduePeriod,
                        out lResultValue,
                        out strPauseCfgString,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    lOverduePeriod = lResultValue;

                    DateTime timeStart = DateTimeUtil.FromRfc1123DateTimeString(strPauseStart);

                    nRet = RoundTime(strUnit,
                        ref timeStart,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    DateTime timeNow = this.Clock.UtcNow;
                    nRet = RoundTime(strUnit,
                        ref timeNow,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    DateTime nextWorkingDay = new DateTime(0);
                    long lDistance = 0;
                    // 计算时间之间的距离
                    // parameters:
                    //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
                    // return:
                    //      -1  出错
                    //      0   成功。timeEnd在工作日范围内。
                    //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
                    nRet = GetTimeDistance(
                        null,   // Calendar calendar,
                        strUnit,
                        timeStart,
                        timeNow,
                        out lDistance,
                        out nextWorkingDay,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    long lDelta = lDistance - lOverduePeriod;

                    if (lDelta < 0)
                        break;  // 已经起作用的事项尚未到期

                    // 消除已经惩罚到期的<overdue>元素
                    DomUtil.SetAttr(node, "pauseStart", "");
                    Debug.Assert(node.ParentNode != null);
                    if (node.ParentNode != null)
                    {
                        // 推入事件日志
                        XmlDocumentFragment fragment = domOperLog.CreateDocumentFragment();
                        fragment.InnerXml = node.OuterXml;
                        node_expiredOverdues.AppendChild(fragment);

                        nExpiredCount++;

                        // 将到期的<overdue>元素从读者记录中删除
                        node.ParentNode.RemoveChild(node);
                        bChanged = true;

                        // 写入统计指标
                        // 到期事项数，而不是读者个数
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "出纳",
                            "以停代金事项到期",
                            1);
                    }

                    // TODO: 创建事件日志，记录到期消除事项的动作

                    // 启动下一个具有overduePeriod属性的<overdue>元素
                    nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        node = nodes[i];
                        strPauseStart = DomUtil.GetAttr(node, "pauseStart");
                        if (String.IsNullOrEmpty(strPauseStart) == true)
                            goto FOUND_1;
                    }

                    break;// 没有找到下一个可启动的事项了
                FOUND_1:

                    TimeSpan delta;

                    // 构造TimeSpan
                    nRet = BuildTimeSpan(
                        lDelta,
                        strUnit,
                        out delta,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    DateTime timeLastEnd = timeNow - delta;

                    // 把第一个事项设置为启动
                    // 启动的日期是上一个事项到期的日子，而不是今日
                    DomUtil.SetAttr(nodes[0],
                        "pauseStart",
                        DateTimeUtil.Rfc1123DateTimeStringEx(timeLastEnd.ToLocalTime()));
                    bChanged = true;

                    // 写入统计指标
                    // 启动事项数，而不是读者个数
                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(
                        strLibraryCode,
                        "出纳",
                        "以停代金事项启动",
                        1);

                    // TODO: 创建事件日志，记录启动事项的动作

                    // 需要重新刷新，因为刚启动的事项可能马上就到期
                } // end of for

                if (nExpiredCount > 0)
                {
                    string strOperTime = this.Clock.GetClock();

                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        strUserID);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTime);

                    // 2012/5/7
                    // 修改前的读者记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
    "oldReaderRecord", strOldReaderXml);   // 2014/3/8 以前 oldReeaderRecord
                    DomUtil.SetAttr(node, "recPath", strReaderRecPath);

                    // 日志中包含修改后的读者记录
                    node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerRecord", readerdom.OuterXml);
                    DomUtil.SetAttr(node, "recPath", strReaderRecPath);

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        strClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "Refresh Pause Borrowing 操作 写入日志时发生错误: " + strError;
                        return -1;
                    }
                }

                return bChanged == true ? 1 : 0;
            } // end of if 

            return 0;
        }

        // 还书：在读者记录中去除借书信息，所去除的借书信息进入历史字段
        // 加入超期检查警告
        // parameters:
        //      strItemBarcodeParam return() API 中的 strItemBarcodeParam，可能包含 @refID: 前缀部分
        //      strItemBarcode  册记录中的 <barcode> 元素内容
        //      strDeletedBorrowFrag 返回从读者记录中删除的<borrow>元素xml片断字符串(OuterXml)
        int DoReturnReaderXml(
            string strLibraryCode,
            ref XmlDocument readerdom,
            string strItemBarcodeParam,
            string strItemBarcode,
            string strOverdueString,
            string strReturnOperator,
            string strOperTime,
            string strClientAddress,
            out string strDeletedBorrowFrag,
            out string strError)
        {
            strError = "";
            strDeletedBorrowFrag = "";
            int nRet = 0;

            // 此时 strItemBarcodeParam 中可能有 refID: 前缀部分

            if (String.IsNullOrEmpty(strItemBarcodeParam) == true)
            {
                strError = "册条码号不能为空";
                return -1;
            }

            XmlNode nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcodeParam + "']");
            if (nodeBorrow == null)
            {
                // 再尝试一次直接用 册条码号
                if (string.IsNullOrEmpty(strItemBarcode) == false)
                    nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']");
                if (nodeBorrow == null)
                {
                    strError = "在读者记录表明该读者并未曾借阅过册 '" + strItemBarcodeParam + "'。";
                    return -1;
                }
            }

            // 删除前记载下来
            strDeletedBorrowFrag = nodeBorrow.OuterXml;

            // 删除借阅册信息
            nodeBorrow.ParentNode.RemoveChild(nodeBorrow);

            // 增添超期信息
            if (String.IsNullOrEmpty(strOverdueString) != true)
            {
                XmlDocumentFragment fragment = readerdom.CreateDocumentFragment();
                fragment.InnerXml = strOverdueString;

                // 看看根下面是否有overdues元素
                XmlNode root = readerdom.DocumentElement.SelectSingleNode("overdues");
                if (root == null)
                {
                    root = readerdom.CreateElement("overdues");
                    readerdom.DocumentElement.AppendChild(root);
                }

                // root.AppendChild(fragment);
                // 插入到最前面
                DomUtil.InsertFirstChild(root, fragment);


                if (StringUtil.IsInList("pauseBorrowing", this.OverdueStyle) == true)
                {
                    //
                    // 处理以停代金功能
                    // return:
                    //      -1  error
                    //      0   readerdom没有修改
                    //      1   readerdom发生了修改
                    nRet = ProcessPauseBorrowing(
                        strLibraryCode,
                        ref readerdom,
                        "", // 因为action为start，可以省略
                        strReturnOperator,
                        "start",
                        strClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "在启动以停代金的过程中发生错误: " + strError;
                        return -1;
                    }
                }

            }

            // 加入到借阅历史字段中
            {
                // 看看根下面是否有borrowHistory元素
                XmlNode root = readerdom.DocumentElement.SelectSingleNode("borrowHistory");
                if (root == null)
                {
                    root = readerdom.CreateElement("borrowHistory");
                    readerdom.DocumentElement.AppendChild(root);
                }

                if (this.MaxPatronHistoryItems > 0)
                {
                    XmlDocumentFragment fragment = readerdom.CreateDocumentFragment();
                    fragment.InnerXml = strDeletedBorrowFrag;

                    // 插入到最前面
                    XmlNode temp = DomUtil.InsertFirstChild(root, fragment);
                    if (temp != null)
                    {
                        // 加入还书时间
                        DomUtil.SetAttr(temp, "returnDate", strOperTime);

                        string strBorrowOperator = DomUtil.GetAttr(temp, "operator");
                        // 把原来的operator属性值复制到borrowOperator属性中
                        DomUtil.SetAttr(temp, "borrowOperator", strBorrowOperator);
                        // operator此时需要表示还书操作者了
                        DomUtil.SetAttr(temp, "operator", strReturnOperator);

                    }
                }

                // 如果超过100个，则删除多余的
                while (root.ChildNodes.Count > this.MaxPatronHistoryItems)
                    root.RemoveChild(root.ChildNodes[root.ChildNodes.Count - 1]);

                // 增量借阅量属性值
                string strBorrowCount = DomUtil.GetAttr(root, "count");
                if (String.IsNullOrEmpty(strBorrowCount) == true)
                    strBorrowCount = "1";
                else
                {
                    long lCount = 1;
                    try
                    {
                        lCount = Convert.ToInt64(strBorrowCount);
                    }
                    catch { }
                    lCount++;
                    strBorrowCount = lCount.ToString();
                }
                DomUtil.SetAttr(root, "count", strBorrowCount);
            }

            return 0;
        }

        // 获得一个代表当前超期事项的唯一性字符串
        public string GetOverdueID()
        {
            // 获得一个自从应用启动以来的增量序号
            long lNumber = Interlocked.Increment(ref m_lSeed);

            // 获得代表当前时间的ticks
            long lTicks = DateTime.Now.Ticks;

            return lTicks.ToString() + "-" + lNumber.ToString();
        }

        // 计算出超期违约金的价格字符串
        // 算法是
        // 1) 将字符串 RMB0.5YUAN/day 拆分为几个部分。prefix=RMB single_price=0.5 postfix=YUAN unit=day
        // 2) 按照超期的时间，乘以 singgle_price，然后加上 prefix 和 postfic 部分，就可以得到结果字符串。不过，还需要考虑一下时间单位换算。目前只支持在 day 和 hour 之间换算
        // 注：所举例的 RMB0.5YUAN/day，金额部分 RMB0.5YUAN 中既包含了前缀，也包含了后缀，是个不错的例子。但实际应用中，一般只有前缀部分，例如 "CNY0.5"
        // parameters:
        //      strPriceCfgString   违约金配置字符串。形态为 'CNY0.5/day'
        //      lDistance   超期时间数
        //      strPeriodUnit   超期时间单位
        //      strOverduePrice 返回所计算出的违约金价格字符串
        // return:
        //      -1  error
        //      0   succeed
        int ComputeOverduePrice(
            string strPriceCfgString,
            long lDistance,
            string strPeriodUnit,
            out string strOverduePrice,
            out string strError)
        {
            strOverduePrice = "";
            strError = "";
            int nRet = 0;

            // 解析strPriceCfgString参数
            string strPriceBase = "";
            string strPerUnit = "day";

            // '/'左边是价格，右边是时间单位。例如 '0.5yuan/day'
            nRet = strPriceCfgString.IndexOf("/");
            if (nRet == -1)
            {
                strPriceBase = strPriceCfgString;
                strPerUnit = "day";
            }
            else
            {
                strPriceBase = strPriceCfgString.Substring(0, nRet).Trim();
                strPerUnit = strPriceCfgString.Substring(nRet + 1).Trim();
            }

            double fSinglePrice = 0.0F;
            string strPrefix = "";
            string strPostfix = "";

            nRet = ParsePriceUnit(strPriceBase,
                out strPrefix,
                out fSinglePrice,
                out strPostfix,
                out strError);
            if (nRet == -1)
            {
                strError = "解析金额字符串 '" + strPriceBase + "' 时发生错误: " + strError;
                return -1;
            }

            // 如果超期时间数目的单位 和 配置违约金额的时间单位 正好符合
            if (strPeriodUnit.ToLower() == strPerUnit.ToLower())
            {
                strOverduePrice = strPrefix + ((double)(fSinglePrice * lDistance)).ToString() + strPostfix;
                return 0;
            }

            if (strPeriodUnit.ToLower() == "day"
                && strPerUnit.ToLower() == "hour")
            {

                strOverduePrice = strPrefix + ((double)(fSinglePrice * lDistance * 24)).ToString() + strPostfix;
                return 0;
            }

            if (strPeriodUnit.ToLower() == "hour"
                && strPerUnit.ToLower() == "day")
            {

                strOverduePrice = strPrefix + ((double)((fSinglePrice * lDistance) / 24)).ToString() + strPostfix;
                return 0;
            }

            strError = "配置的 超期时间单位 '" + strPeriodUnit + "' 和 违约金额时间单位 '" + strPerUnit + "' 之间无法进行换算。";
            return -1;
        }

        // 计算出丢失图书的违约金的价格字符串
        // parameters:
        //      strPriceCfgString   违约金倍率。形态为一个小数，例如 '10.5'
        //      strItemPrice   册原价格
        //      strLostPrice 返回所计算出的违约金价格字符串
        // return:
        //      -1  error
        //      0   succeed
        //      1   因为缺原始价格，从而只好创建了带问号的算式
        int ComputeLostPrice(
            string strPriceCfgString,
            string strItemPrice,
            out string strLostPrice,
            out string strError)
        {
            strLostPrice = "";
            strError = "";
            int nRet = 0;

            double ratio = 1.0;

            try
            {
                ratio = Convert.ToDouble(strPriceCfgString);
            }
            catch
            {
                strError = "违约金因子配置字符串 '" + strPriceCfgString + "' 格式错误。应该为一个小数。";
                return -1;
            }

            // 如果原始价格为空
            if (String.IsNullOrEmpty(strItemPrice) == true)
            {
                strLostPrice = "?*" + strPriceCfgString;
                return 1;
            }

            /*
            // 处理价格字符串中可能存在的乘号、除号
            List<string> temp_prices = new List<string>();
            temp_prices.Add(strItemPrice);

            string strOutputPrice = "";
            // TODO: 似乎可用SumPrices()
            nRet = PriceUtil.TotalPrice(temp_prices,
                out strOutputPrice,
                out strError);
            if (nRet == -1)
            {
                strError = "解析价格字符串 '" + strItemPrice + "' 时发生错误1: " + strError;
                return -1;
            }

            strItemPrice = strOutputPrice;
             * */
            // 正规化价格字符串
            // 处理价格字符串中可能存在的乘号、除号
            nRet = CanonicalizeItemPrice(ref strItemPrice,
                out strError);
            if (nRet == -1)
                return -1;

            double fItemPrice = 0.0F;
            string strPrefix = "";
            string strPostfix = "";

            // 最好能够同时处理前缀和后缀
            nRet = ParsePriceUnit(strItemPrice,
                out strPrefix,
                out fItemPrice,
                out strPostfix,
                out strError);
            if (nRet == -1)
            {
                strError = "解析价格字符串 '" + strItemPrice + "' 时发生错误2: " + strError;
                return -1;
            }

            strLostPrice = strPrefix + (fItemPrice * ratio).ToString() + strPostfix;
            return 0;
        }

        // 正规化价格字符串
        // 处理价格字符串中可能存在的乘号、除号
        public static int CanonicalizeItemPrice(ref string strPrice,
            out string strError)
        {
            strError = "";

            List<string> temp_prices = new List<string>();
            temp_prices.Add(strPrice);

            string strOutputPrice = "";
            // TODO: 似乎可用SumPrices()
            int nRet = PriceUtil.TotalPrice(temp_prices,
                out strOutputPrice,
                out strError);
            if (nRet == -1)
            {
                strError = "正规化价格字符串 '" + strPrice + "' 时发生错误1: " + strError;
                return -1;
            }

            strPrice = strOutputPrice;
            return 0;
        }

        // 在册记录中删除借书信息
        // parameters:
        //      strOverdueString    表示超期信息的字符串。borrowOperator属性表示借阅操作者；operator属性表示还书操作者
        // return:
        //      -1  出错
        //      0   正常
        //      1   超期还书或者丢失处理的情况
        int DoReturnItemXml(
            string strAction,
            SessionInfo sessioninfo,
            Calendar calendar,
            string strReaderType,
            string strLibraryCode,
            string strAccessParameters,
            XmlDocument readerdom,
            ref XmlDocument itemdom,
            bool bForce,
            bool bItemBarcodeDup,
            string strItemRecPath,
            string strReturnOperator,
            string strOperTime,
            out string strOverdueString,
            out string strLostComment,
            out ReturnInfo return_info,
            out string strWarning,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            strOverdueString = "";
            strLostComment = "";
            strWarning = "";

            Debug.Assert(String.IsNullOrEmpty(strItemRecPath) == false, "");

            string strActionName = GetReturnActionName(strAction);

            return_info = new ReturnInfo();

            LibraryApplication app = this;

            string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");
            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
#if NO
                // text-level: 内部错误
                strError = "册记录中册条码号不能为空";
                return -1;
#endif
                // 如果册条码号为空，则记载 参考ID
                string strRefID = DomUtil.GetElementText(itemdom.DocumentElement,
    "refID");
                if (String.IsNullOrEmpty(strRefID) == true)
                {
                    // text-level: 内部错误
                    strError = "册记录中册条码号和参考ID不应同时为空";
                    return -1;
                }
                strItemBarcode = "@refID:" + strRefID;
            }

            // 馆藏地点
            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement, "location");
            // 去掉#reservation部分
            strLocation = StringUtil.GetPureLocationString(strLocation);

            // 既然一个册记录已经被允许借了，那就无条件要允许还，不管册的馆藏地点是否属于这个读者所在的馆藏地点。如果发现不一致，需要警告
            // 检查册所属的馆藏地点是否合读者所在的馆藏地点吻合
            string strCode = "";
            string strRoom = "";
            {

                // 解析
                ParseCalendarName(strLocation,
            out strCode,
            out strRoom);
                if (strCode != strLibraryCode)
                {
                    strWarning += "册记录的馆藏地 '" + strLocation + "' 不属于读者所在馆代码 '" + strLibraryCode + "'，请注意后续处理。";
                }
            }

            // 检查存取定义馆藏地列表
            if (string.IsNullOrEmpty(strAccessParameters) == false && strAccessParameters != "*")
            {
                bool bFound = false;
                List<string> locations = StringUtil.SplitList(strAccessParameters);
                foreach (string s in locations)
                {
                    string c = "";
                    string r = "";
                    ParseCalendarName(s,
                        out c,
                        out r);
                    if (/*string.IsNullOrEmpty(c) == false && */ c != "*")
                    {
                        if (c != strCode)
                            continue;
                    }

                    if (/*string.IsNullOrEmpty(r) == false && */ r != "*")
                    {
                        if (r != strRoom)
                            continue;
                    }

                    bFound = true;
                    break;
                }

                if (bFound == false)
                {
                    strError = strActionName + "操作被拒绝。因册记录的馆藏地 '" + strLocation + "' 不在当前用户存取定义规定的 " + strActionName + " 操作的馆藏地许可范围 '" + strAccessParameters + "' 之内";
                    return -1;
                }
            }
            ///
            // 检查册是否能够被还回
            bool bResultValue = false;
            string strMessageText = "";

            // 执行脚本函数ItemCanReturn
            // parameters:
            // return:
            //      -2  not found script
            //      -1  出错
            //      0   成功
            nRet = app.DoItemCanReturnScriptFunction(
                sessioninfo.Account,
                itemdom,
                out bResultValue,
                out strMessageText,
                out strError);
            if (nRet == -1)
            {
                strError = "执行CanReturn()脚本函数时出错: " + strError;
                return -1;
            }
            if (nRet == -2)
            {
            }
            else
            {
                // 根据脚本返回结果
                if (bResultValue == false)
                {
                    strError = "还书失败。因为册 " + strItemBarcode + " 的状态为 " + strMessageText;
                    return -1;
                }
            }

            // 
            // 个人书斋的检查
            string strPersonalLibrary = "";
            if (sessioninfo.UserType == "reader"
                && sessioninfo.Account != null)
                strPersonalLibrary = sessioninfo.Account.PersonalLibrary;

            if (string.IsNullOrEmpty(strPersonalLibrary) == false)
            {
                if (strPersonalLibrary != "*" && StringUtil.IsInList(strRoom, strPersonalLibrary) == false)
                {
                    strError = "还书失败。当前用户 '" + sessioninfo.Account.Barcode + "' 只能操作馆代码 '" + strLibraryCode + "' 中地点为 '" + strPersonalLibrary + "' 的图书，不能操作地点为 '" + strRoom + "' 的图书";
                    return -1;
                }
            }

            bool bOverdue = false;

            string strOverdueMessage = "";

            // 图书类型
            string strBookType = DomUtil.GetElementText(itemdom.DocumentElement, "bookType");

            string strBorrowDate = DomUtil.GetElementText(itemdom.DocumentElement, "borrowDate");
            string strPeriod = DomUtil.GetElementText(itemdom.DocumentElement, "borrowPeriod");


            // 这是借阅时的操作者
            string strBorrowOperator = DomUtil.GetElementText(itemdom.DocumentElement, "operator");

            // 册状态
            string strState = DomUtil.GetElementText(itemdom.DocumentElement,
                "state");
            string strComment = DomUtil.GetElementText(itemdom.DocumentElement,
                "comment");
            string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                "borrower");

            // 册价格
            string strItemPrice = DomUtil.GetElementText(itemdom.DocumentElement, "price");

            if (strAction == "lost"
                && String.IsNullOrEmpty(strItemPrice) == true
                && bForce == false)
            {
                strError = "册价格(<price>元素)为空，无法计算丢失图书违约金数。请先为该册登入价格信息，再重新进行丢失声明处理。";
                return -1;
            }

            DateTime borrowdate = new DateTime((long)0);

            try
            {
                borrowdate = DateTimeUtil.FromRfc1123DateTimeString(strBorrowDate);
            }
            catch
            {
                if (bForce == true)
                    goto DOCHANGE;
                strError = "借阅日期字符串 '" + strBorrowDate + "' 格式错误";
                return -1;
            }

            // 看看是否超期
            string strPeriodUnit = "";
            long lPeriodValue = 0;

            nRet = LibraryApplication.ParsePeriodUnit(strPeriod,
                out lPeriodValue,
                out strPeriodUnit,
                out strError);
            if (nRet == -1)
            {
                if (bForce == true)
                    goto DOCHANGE;
                strError = "册记录中借阅期限值 '" + strPeriod + "' 格式错误: " + strError;
                return -1;
            }

            DateTime timeEnd = DateTime.MinValue;
            DateTime nextWorkingDay = DateTime.MinValue;

            // 测算还书日期
            // parameters:
            //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
            // return:
            //      -1  出错
            //      0   成功。timeEnd在工作日范围内。
            //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
            nRet = LibraryApplication.GetReturnDay(
                calendar,
                borrowdate,
                lPeriodValue,
                strPeriodUnit,
                out timeEnd,
                out nextWorkingDay,
                out strError);
            if (nRet == -1)
            {
                if (bForce == true)
                    goto DOCHANGE;
                strError = "测算还书日期过程发生错误: " + strError;
                return -1;
            }

            return_info.LatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringEx(timeEnd.ToLocalTime());

            bool bEndInNonWorkingDay = false;
            if (nRet == 1)
            {
                // 结束在非工作日
                bEndInNonWorkingDay = true;
            }

            DateTime now = app.Clock.UtcNow;  //  今天  当下

            // 正规化时间
            DateTime now_rounded = now;
            nRet = RoundTime(strPeriodUnit,
                ref now_rounded,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta = now_rounded - timeEnd;

            long lOver = 0;
            long lDelta = 0;
            long lDelta1 = 0;   // 校正（考虑工作日）后的差额

            nRet = ParseTimeSpan(
                delta,
                strPeriodUnit,
                out lDelta,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta1 = new TimeSpan(0);
            if (bEndInNonWorkingDay == true)
            {
                delta1 = now_rounded - nextWorkingDay;

                nRet = ParseTimeSpan(
    delta1,
    strPeriodUnit,
    out lDelta1,
    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                delta1 = delta;
                lDelta1 = lDelta;
            }

            if (lDelta1 > 0)
            {
                // 获得 '超期违约金因子' 配置参数
                bool bComputePrice = true;
                string strPriceCfgString = "";
                {
                    MatchResult matchresult;
                    // return:
                    //      reader和book类型均匹配 算4分
                    //      只有reader类型匹配，算3分
                    //      只有book类型匹配，算2分
                    //      reader和book类型都不匹配，算1分
                    nRet = app.GetLoanParam(
                        //null,
                        strLibraryCode,
                        strReaderType,
                        strBookType,
                        "超期违约金因子",
                        out strPriceCfgString,
                        out matchresult,
                        out strError);
                }

                if (nRet == -1)
                {
                    if (bForce == true)
                        bComputePrice = false;  // goto CONTINUE_OVERDUESTRING;
                    else
                    {
                        strError = "还书失败。获得 馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 超期违约金因子 参数时发生错误: " + strError;
                        return -1;
                    }
                }
                if (nRet < 4) // nRet == 0
                {
                    if (bForce == true)
                        bComputePrice = false;  // goto CONTINUE_OVERDUESTRING;
                    else
                    {
                        strError = "还书失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 超期违约金因子 参数无法获得: " + strError;
                        return -1;
                    }
                }

                // 注: '超期违约金因子' 参数值如果为空，表明不对超期做违约金处理(方法是在 strOverdueString 第一字符添加一个 '!' )。但需要记入日志
                {
                    // long lOver = 0;
                    // 如果<amerce overdueStyle="...">中包含了includeNoneworkingDay，表示超期天数按照包含了末尾非工作日的算法。否则，就是不包含末尾非工作日，从第一个工作日开始后面的超过天数。
                    if (StringUtil.IsInList("includeNoneworkingDay", this.OverdueStyle) == true)
                        lOver = lDelta;
                    else
                        lOver = lDelta1;

                    string strOverduePrice = "";
                    if (bComputePrice == true && string.IsNullOrEmpty(strPriceCfgString) == false)
                    {
                        nRet = ComputeOverduePrice(
                            strPriceCfgString,
                            lDelta1,    // 按照调整后的差额计算
                            strPeriodUnit,
                            out strOverduePrice,
                            out strError);
                        if (nRet == -1)
                        {
                            if (bForce == true)
                            {
                                // goto CONTINUE_OVERDUESTRING;
                            }
                            else
                            {
                                strError = "还书失败。计算超期违约金价格时出错: " + strError;
                                return -1;
                            }
                        }
                    }

                    // CONTINUE_OVERDUESTRING:
                    strOverdueMessage += "还书时已超过借阅期限 " + Convert.ToString(lOver) + GetDisplayTimeUnitLang(strPeriodUnit) + "。请履行超期手续。";

                    // 最好用XmlTextWriter或者DOM来构造strOverdueString
                    XmlDocument tempdom = new XmlDocument();
                    tempdom.LoadXml("<overdue />");
                    DomUtil.SetAttr(tempdom.DocumentElement, "barcode", strItemBarcode);

                    if (bItemBarcodeDup == true)
                    {
                        // 若条码号足以定位，则不记载实体记录路径
                        DomUtil.SetAttr(tempdom.DocumentElement, "recPath", strItemRecPath);
                    }

                    string strReason = "超期。超 " + (lOver).ToString() + GetDisplayTimeUnitLang(strPeriodUnit) + "; 违约金因子: " + strPriceCfgString;
                    DomUtil.SetAttr(tempdom.DocumentElement, "reason", strReason);

                    // 超期时间长度 2007/12/17
                    DomUtil.SetAttr(tempdom.DocumentElement, "overduePeriod", (lOver).ToString() + strPeriodUnit);

                    DomUtil.SetAttr(tempdom.DocumentElement, "price", strOverduePrice);
                    DomUtil.SetAttr(tempdom.DocumentElement, "borrowDate", strBorrowDate);
                    DomUtil.SetAttr(tempdom.DocumentElement, "borrowPeriod", strPeriod);
                    DomUtil.SetAttr(tempdom.DocumentElement, "returnDate", DateTimeUtil.Rfc1123DateTimeStringEx(now.ToLocalTime()));
                    DomUtil.SetAttr(tempdom.DocumentElement, "borrowOperator", strBorrowOperator);
                    DomUtil.SetAttr(tempdom.DocumentElement, "operator", strReturnOperator);
                    // id属性是唯一的, 为交违约金C/S界面创造了有利条件
                    string strOverdueID = GetOverdueID();
                    DomUtil.SetAttr(tempdom.DocumentElement, "id", strOverdueID);

                    strOverdueString = tempdom.DocumentElement.OuterXml;

                    /*
                    strOverdueString = "<overdue barcode='" + strItemBarcode
                        + "' over='" + Convert.ToString(lOver) + strPeriodUnit
                        + "' borrowDate='" + strBorrowDate
                        + "' borrowPeriod='" + strPeriod 
                        + "' returnDate='" + DateTimeUtil.Rfc1123DateTimeString(now) 
                        + "' operator='" + strOperator
                        + "' id='" + GetOverdueID() + "'/>";
                     */

                    if (string.IsNullOrEmpty(strPriceCfgString) == false)
                        bOverdue = true;
                    else
                        strOverdueString = "!" + strOverdueString;  // 表示后面不写入读者记录，但需记入日志
                }
            }

            if (strAction == "lost")
            {
                string strLostPrice = "?";
                string strReason = "丢失。";

                string strBiblioRecID = DomUtil.GetElementText(itemdom.DocumentElement, "parent");  //
                string strItemDbName = ResPath.GetDbName(strItemRecPath);
                string strBiblioDbName = "";
                // 根据实体库名, 找到对应的书目库名
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = this.GetBiblioDbNameByItemDbName(strItemDbName,
                    out strBiblioDbName,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "实体库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                    return -1;
                }
                string strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;


                int nResultValue = 0;
                string strTempReason = "";
                // 执行脚本函数GetLost
                // 根据当前读者记录、实体记录、书目记录，计算出丢失后的赔偿金额
                // parameters:
                // return:
                //      -2  not found script
                //      -1  出错
                //      0   成功
                nRet = this.DoGetLostScriptFunction(
                    sessioninfo,
            readerdom,
            itemdom,
            strBiblioRecPath,
            out nResultValue,
            out strLostPrice,
            out strTempReason,
            out strError);
                if (nRet == -1)
                {
                    strError = "调用脚本函数GetLost()时出错: " + strError;
                    return -1;
                }

                if (nRet == 0)
                {
                    if (nResultValue == -1)
                    {
                        strError = "(脚本函数)计算丢失赔偿金额时报错: " + strError;
                        return -1;
                    }

                    strReason += strTempReason;
                }
                // 没有发现脚本函数，则从配置表中找到参数并计算
                else if (nRet == -2)
                {
                    // 获得 '丢失违约金因子' 配置参数
                    string strPriceCfgString = "";
                    MatchResult matchresult;
                    // return:
                    //      reader和book类型均匹配 算4分
                    //      只有reader类型匹配，算3分
                    //      只有book类型匹配，算2分
                    //      reader和book类型都不匹配，算1分
                    nRet = app.GetLoanParam(
                        //null,
                        strLibraryCode,
                        strReaderType,
                        strBookType,
                        "丢失违约金因子",
                        out strPriceCfgString,
                        out matchresult,
                        out strError);
                    if (nRet == -1)
                    {
                        if (bForce == true)
                            goto CONTINUE_LOSTING;
                        strError = "丢失处理失败。获得 馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 丢失违约金因子 参数时发生错误: " + strError;
                        return -1;
                    }
                    if (nRet < 4)  // nRet == 0
                    {
                        if (bForce == true)
                            goto CONTINUE_LOSTING;

                        strError = "还书失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 丢失违约金因子 参数无法获得: " + strError;
                        return -1;
                    }

                    nRet = ComputeLostPrice(
                        strPriceCfgString,
                        strItemPrice,
                        out strLostPrice,
                        out strError);
                    if (nRet == -1)
                    {
                        if (bForce == true)
                            goto CONTINUE_LOSTING;

                        strError = "丢失处理失败。计算丢失违约金价格时出错: " + strError;
                        return -1;
                    }

                    strReason = "丢失。原价格: " + strItemPrice + "; 违约金因子:" + strPriceCfgString;
                }
                else
                {
                    Debug.Assert(false, "");
                }

            CONTINUE_LOSTING:

                strOverdueMessage += "有丢失违约金 " + strLostPrice + "。请履行付违约金手续。";

                // 构造strOverdueString
                XmlDocument tempdom = new XmlDocument();
                tempdom.LoadXml("<overdue />");
                DomUtil.SetAttr(tempdom.DocumentElement, "barcode", strItemBarcode);
                DomUtil.SetAttr(tempdom.DocumentElement, "reason", strReason);
                DomUtil.SetAttr(tempdom.DocumentElement, "price", strLostPrice);
                DomUtil.SetAttr(tempdom.DocumentElement, "borrowDate", strBorrowDate);
                DomUtil.SetAttr(tempdom.DocumentElement, "borrowPeriod", strPeriod);
                DomUtil.SetAttr(tempdom.DocumentElement, "returnDate", DateTimeUtil.Rfc1123DateTimeStringEx(now.ToLocalTime()));
                DomUtil.SetAttr(tempdom.DocumentElement, "borrowOperator", strBorrowOperator);
                DomUtil.SetAttr(tempdom.DocumentElement, "operator", strReturnOperator);
                // id属性是唯一的, 为交违约金C/S界面创造了有利条件
                string strOverdueID = GetOverdueID();
                DomUtil.SetAttr(tempdom.DocumentElement, "id", strOverdueID);

                strOverdueString += tempdom.DocumentElement.OuterXml;

                strLostComment = "本册于 " + DateTimeUtil.Rfc1123DateTimeStringEx(now.ToLocalTime()) + " 由读者 " + strBorrower + " 声明丢失。违约金记录id为 " + strOverdueID + "。最后一次借阅的情况如下: 借阅日期: " + strBorrowDate + "; 借阅期限: " + strPeriod + "。";
            }


        DOCHANGE:

            XmlNode nodeOldBorrower = null;

            // 加入到借阅历史字段中
            {
                // 看看根下面是否有borrowHistory元素
                XmlNode root = itemdom.DocumentElement.SelectSingleNode("borrowHistory");
                if (root == null)
                {
                    root = itemdom.CreateElement("borrowHistory");
                    itemdom.DocumentElement.AppendChild(root);
                }


                if (this.MaxItemHistoryItems > 0)
                {
                    nodeOldBorrower = itemdom.CreateElement("borrower");
                    // 插入到最前面
                    XmlNode temp = DomUtil.InsertFirstChild(root, nodeOldBorrower);
                    if (temp != null)
                    {
                        // 加入还书时间
                        DomUtil.SetAttr(temp, "returnDate", strOperTime);
                    }
                }

                // 如果超过100个，则删除多余的
                while (root.ChildNodes.Count > this.MaxItemHistoryItems)
                    root.RemoveChild(root.ChildNodes[root.ChildNodes.Count - 1]);

                // 增量借阅量属性值
                string strBorrowCount = DomUtil.GetAttr(root, "count");
                if (String.IsNullOrEmpty(strBorrowCount) == true)
                    strBorrowCount = "1";
                else
                {
                    long lCount = 1;
                    try
                    {
                        lCount = Convert.ToInt64(strBorrowCount);
                    }
                    catch { }
                    lCount++;
                    strBorrowCount = lCount.ToString();
                }
                DomUtil.SetAttr(root, "count", strBorrowCount);
            }

            if (nodeOldBorrower != null)
                DomUtil.SetAttr(nodeOldBorrower,
                    "barcode",
                    DomUtil.GetElementText(itemdom.DocumentElement, "borrower"));
            // DomUtil.SetElementText(itemdom.DocumentElement, "borrower", "");
            DomUtil.DeleteElement(itemdom.DocumentElement,
    "borrower");

            // 2009/9/18
            //DomUtil.SetElementText(itemdom.DocumentElement,
            //    "borrowerReaderType", "");
            DomUtil.DeleteElement(itemdom.DocumentElement,
    "borrowerReaderType");
            // 2012/9/8
            //DomUtil.SetElementText(itemdom.DocumentElement,
            //    "borrowerRecPath", "");
            DomUtil.DeleteElement(itemdom.DocumentElement,
"borrowerRecPath");

            if (nodeOldBorrower != null)
                DomUtil.SetAttr(nodeOldBorrower,
               "borrowDate",
               DomUtil.GetElementText(itemdom.DocumentElement, "borrowDate"));
            //DomUtil.SetElementText(itemdom.DocumentElement,
            //    "borrowDate", "");
            DomUtil.DeleteElement(itemdom.DocumentElement,
"borrowDate");

            if (nodeOldBorrower != null)
                DomUtil.SetAttr(nodeOldBorrower,
               "borrowPeriod",
               DomUtil.GetElementText(itemdom.DocumentElement, "borrowPeriod"));
            //DomUtil.SetElementText(itemdom.DocumentElement,
            //    "borrowPeriod", "");
            DomUtil.DeleteElement(itemdom.DocumentElement,
"borrowPeriod");

            // 2014/11/14
            if (nodeOldBorrower != null)
            {
                string strValue = DomUtil.GetElementText(itemdom.DocumentElement, "returningDate");
                if (string.IsNullOrEmpty(strValue) == false)
                    DomUtil.SetAttr(nodeOldBorrower,
                        "returningDate",
                        strValue);
            }
            DomUtil.DeleteElement(itemdom.DocumentElement,
                "returningDate");

            // 2014/11/14
            DomUtil.DeleteElement(itemdom.DocumentElement,
                "lastReturningDate");

            // string strBorrowOperator = DomUtil.GetElementText(itemdom.DocumentElement, "operator");
            //DomUtil.SetElementText(itemdom.DocumentElement,
            //    "operator", "");    // 清除
            DomUtil.DeleteElement(itemdom.DocumentElement,
"operator");

            // item中原operator元素值表示借阅操作者，此时应转入历史中的borrowOperator元素中
            if (nodeOldBorrower != null)
                DomUtil.SetAttr(nodeOldBorrower,
                "borrowOperator",
                strBorrowOperator);

            // 借阅历史中operator属性值表示还书操作者
            if (nodeOldBorrower != null)
                DomUtil.SetAttr(nodeOldBorrower,
                "operator",
                strReturnOperator);

            // 2011/6/28
            return_info.BorrowOperator = strBorrowOperator;
            return_info.ReturnOperator = strReturnOperator;

            string strNo = DomUtil.GetElementText(itemdom.DocumentElement, "no");
            if (nodeOldBorrower != null)
            {
                if (string.IsNullOrEmpty(strNo) == false
                && strNo != "0")    // 2013/12/23
                    DomUtil.SetAttr(nodeOldBorrower,
                        "no",
                        strNo);
            }
            DomUtil.DeleteElement(itemdom.DocumentElement,
                "no");

            string strRenewComment = DomUtil.GetElementText(itemdom.DocumentElement, "renewComment");
            if (nodeOldBorrower != null)
            {
                if (string.IsNullOrEmpty(strRenewComment) == false) // 2013/12/23
                    DomUtil.SetAttr(nodeOldBorrower,
                        "renewComment",
                        strRenewComment);
            }
            DomUtil.DeleteElement(itemdom.DocumentElement,
                "renewComment");

            if (nodeOldBorrower != null)
            {
                if (strAction == "lost"
                && strLostComment != "")
                {
                    DomUtil.SetAttr(nodeOldBorrower,
        "state",
        strState);
                    DomUtil.SetAttr(nodeOldBorrower,
                        "comment",
                        strComment);

                    /*
                    if (String.IsNullOrEmpty(strState) == false)
                        strState += ",";
                    strState += "丢失";
                     * */

                    StringUtil.SetInList(ref strState,
                "丢失",
                true);

                    DomUtil.SetElementText(itemdom.DocumentElement,
                        "state", strState);
                    if (strLostComment != "")
                    {
                        if (String.IsNullOrEmpty(strComment) == false)
                            strComment += "\r\n";
                        strComment += strLostComment;
                        DomUtil.SetElementText(itemdom.DocumentElement,
                            "comment", strComment);
                    }
                }
            }

            //  统计指标
            {
                TimeSpan delta_0 = this.Clock.UtcNow - borrowdate;
                if (delta_0.TotalDays < 1)
                {
                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(
                        strLibraryCode,
                        "出纳",
                        "当日内立即还册",
                        1);
                }
            }

            // return_info
            return_info.BorrowTime = strBorrowDate;
            return_info.Period = strPeriod;
            return_info.OverdueString = strOverdueString;
            // string strNo = DomUtil.GetElementText(itemdom.DocumentElement, "no");
            return_info.BorrowCount = 0;

            // 2012/3/28
            if (string.IsNullOrEmpty(strNo) == false)
                Int64.TryParse(strNo, out return_info.BorrowCount);
#if NO
            try
            {
                return_info.BorrowCount = Convert.ToInt32(strNo);
            }
            catch
            {
                return_info.BorrowCount = 0;
            }
#endif

            return_info.BookType = strBookType;
            /*
            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement,
                "location");
             * */
            return_info.Location = strLocation;

            if (bOverdue == true
                || strAction == "lost")
            {
                strError = strOverdueMessage;
                return 1;
            }

            return 0;
        }

        // 盘点操作中，设置 ReturnInfo 信息
        static void SetReturnInfo(ref ReturnInfo return_info, XmlDocument item_dom)
        {
#if NO
            return_info.LatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringEx(timeEnd.ToLocalTime());
            return_info.BorrowOperator = strBorrowOperator;
            return_info.ReturnOperator = strReturnOperator;
            return_info.BorrowTime = strBorrowDate;
            return_info.Period = strPeriod;
            return_info.OverdueString = strOverdueString;
            return_info.BorrowCount = 0;
#endif
            return_info.BookType = DomUtil.GetElementText(item_dom.DocumentElement, "bookType");
            // return_info.Location = StringUtil.GetPureLocation(DomUtil.GetElementText(item_dom.DocumentElement, "location"));
            return_info.Location = DomUtil.GetElementText(item_dom.DocumentElement, "location");    // 可能会携带 #reservatoin, 部分
        }

        // 模拟预约情况
        int SimulateReservation(
            ref XmlDocument readerdom,
            ref XmlDocument itemdom,
            out string strError)
        {
            strError = "";

            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");
            if (string.IsNullOrEmpty(strReaderBarcode))
            {
                strReaderBarcode = "@refID:" + DomUtil.GetElementText(readerdom.DocumentElement,
                    "refID");
            }

            string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                "barcode");
            if (string.IsNullOrEmpty(strItemBarcode))
            {
                strItemBarcode = "@refID:" + DomUtil.GetElementText(itemdom.DocumentElement,
                    "refID");
            }

            {
                XmlElement reservations = itemdom.DocumentElement.SelectSingleNode("reservations") as XmlElement;
                if (reservations == null)
                {
                    reservations = itemdom.CreateElement("reservations");
                    itemdom.DocumentElement.AppendChild(reservations);
                }

                XmlElement request = itemdom.CreateElement("request");
                reservations.AppendChild(request);

                request.SetAttribute("reader", strReaderBarcode);
            }

            {
                XmlElement reservations = readerdom.DocumentElement.SelectSingleNode("reservations") as XmlElement;
                if (reservations == null)
                {
                    reservations = readerdom.CreateElement("reservations");
                    readerdom.DocumentElement.AppendChild(reservations);
                }

                XmlElement request = readerdom.CreateElement("request");
                reservations.AppendChild(request);

                request.SetAttribute("items", strItemBarcode);
            }

            return 0;
        }

        // 还书后对册记录中的预约信息进行检查和处理
        // 算法是：找到第一个没有超期(expireDate)并且state不是arrived的<request>元素，
        // 返回这个元素的reader属性（这就是下一个预约者），并且把这个找到的<request>
        // 元素的state属性打上arrived标记。
        // 如果为丢失处理，本函数的调用者需要通知等待者：书已经丢失了，不用再等待
        // parameters:
        //      bMaskLocationReservation    不要给<location>打上#reservation标记
        // return:
        //      -1  error
        //      0   没有修改
        //      1   对册记录进行过修改。进行过修改，不一定返回strReservationReaderBarcode。修改可能是顺便删除了过期的<request>元素
        internal int DoItemReturnReservationCheck(
            bool bDontMaskLocationReservation,
            ref XmlDocument itemdom,
            out string strReservationReaderBarcode,
            out string strError)
        {
            strReservationReaderBarcode = "";
            strError = "";
            bool bChanged = false;

            // 找到所有<reservations/request>元素
            XmlNodeList nodes = itemdom.DocumentElement.SelectNodes("reservations/request");
            if (nodes.Count == 0)
                return 0;   // 没有找到<request>元素, 也就是说明没有被预约

            XmlNode node = null;
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                string strExpireDate = DomUtil.GetAttr(node, "expireDate");
                // 看看请求是否过期
                if (String.IsNullOrEmpty(strExpireDate) == false)
                {
                    DateTime expiredate = DateTimeUtil.FromRfc1123DateTimeString(strExpireDate);
                    DateTime now = this.Clock.UtcNow;   // 2007/12/17 changed //  DateTime.UtcNow;
                    if (expiredate > now)
                    {
                        // TODO: 过期的预约请求，是否自动删除?
                        node.ParentNode.RemoveChild(node);
                        bChanged = true;
                        continue;
                    }
                }

                // 看看状态是不是arrived
                string strState = DomUtil.GetAttr(node, "state");
                if (strState == "arrived")
                {
                    // 删除以前残留的，状态为arrived的<request>元素
                    node.ParentNode.RemoveChild(node);
                    bChanged = true;
                    continue;
                }

                goto FOUND;
            }

            if (bChanged == false)
                return 0;   // not changed
            return 1;   // 虽然没有找到，但是册记录发生了修改
        FOUND:

            Debug.Assert(node != null, "");
            strReservationReaderBarcode = DomUtil.GetAttr(node, "reader");

            if (String.IsNullOrEmpty(strReservationReaderBarcode) == true)
            {
                strError = "<request>元素中reader属性值为空";
                return -1;
            }

            /*
            // 删除<request>元素
            node.ParentNode.RemoveChild(node);
             * */
            // 将<request>元素的state属性值修改为arrived
            DomUtil.SetAttr(node, "state", "arrived");
            // 到达时间
            DomUtil.SetAttr(node, "arrivedDate", this.Clock.GetClock());

            bChanged = true;

            if (bDontMaskLocationReservation == false)
            {
                // 修改<location>元素,加入一个#reservation列举值
                string strText = DomUtil.GetElementText(itemdom.DocumentElement, "location");
                if (strText == null)
                    strText = "";

                if (StringUtil.IsInList("#reservation", strText) == false)
                {
                    if (strText != "")
                        strText += ",";
                    strText += "#reservation";
                }

                DomUtil.SetElementText(itemdom.DocumentElement, "location", strText);
                bChanged = true;
            }

            return 1;
        }

        // 获得smtp服务器配置信息
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetSmtpServerCfg(
            out string strAddress,
            out string strManagerEmail,
            out string strError)
        {
            strAddress = "";
            strManagerEmail = "";
            strError = "";
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode(
                "smtpServer");
            if (node == null)
            {
                strError = "在library.xml中没有找到<smtpServer>元素";
                return 0;
            }
            strAddress = DomUtil.GetAttr(node, "address");

            if (String.IsNullOrEmpty(strAddress) == true)
            {
                strError = "<smtpServer>未配置address参数值。";
                return -1;
            }

            strManagerEmail = DomUtil.GetAttr(node, "managerEmail");

            if (String.IsNullOrEmpty(strManagerEmail) == true)
            {
                strError = "<smtpServer>未配置managerEmail参数值。";
                return -1;
            }

            return 1;
        }

#if NOOOOOOOOOOOOOO
        // 发送通知email
        // return:
        //      -1  error
        //      0   not found smtp server cfg
        //      1   succeed
        public int SendEmail(string strUserEmail,
            string strSubject,
            string strBody,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strManagerEmail = "";

            string strSmtpServerAddress = "";
                    // 获得smtp服务器配置信息
        // return:
        //      -1  error
        //      0   not found
        //      1   found
            nRet = GetSmtpServerCfg(
                out strSmtpServerAddress,
                out strManagerEmail,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;   // not found cfg

            if (String.IsNullOrEmpty(strSmtpServerAddress) == true)
                strSmtpServerAddress = "127.0.0.1";
            

            MailMessage Message = new MailMessage();
            Message.To = strUserEmail;	// To
            Message.From = strManagerEmail; // From
            Message.Subject = strSubject;
            Message.Body = strBody;	// Body

            try
            {
                SmtpMail.SmtpServer = strSmtpServerAddress;
                SmtpMail.Send(Message);
            }
            catch (Exception ex/*System.Web.HttpException ehttp*/)
            {
                strError = GetInnerMessage(ex);
                return -1;
            }


            return 1;
        }
#endif

        // 发送通知email
        // return:
        //      -1  error
        //      0   not found smtp server cfg
        //      1   succeed
        public int SendEmail(string strUserEmail,
            string strSubject,
            string strBody,
            string strMime,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strManagerEmail = "";

            string strSmtpServerAddress = "";
            // 获得smtp服务器配置信息
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetSmtpServerCfg(
                out strSmtpServerAddress,
                out strManagerEmail,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;   // not found cfg

            if (String.IsNullOrEmpty(strSmtpServerAddress) == true)
                strSmtpServerAddress = "127.0.0.1";

            try
            {
                // System.Net.Mail 新名字空间
                MailMessage message = new MailMessage(
                    strManagerEmail,
                    strUserEmail,
                    strSubject,
                    strBody);
                if (strMime == "html")
                    message.IsBodyHtml = true;

                SmtpClient client = new SmtpClient(strSmtpServerAddress);
                // Credentials are necessary if the server requires the client 
                // to authenticate before it will send e-mail on the client's behalf.
                client.UseDefaultCredentials = true;
                client.Send(message);
            }
            catch (Exception ex/*System.Web.HttpException ehttp*/)
            {
                strError = ExceptionUtil.GetDebugText(ex);  // GetInnerMessage(ex);
                return -1;
            }

            return 1;
        }

#if NO
        public static string GetInnerMessage(Exception ex)
        {
            string strResult = "";
            for (; ; )
            {
                strResult += "|" + ex.Message;
                ex = ex.InnerException;
                if (ex == null)
                    return strResult;
            }
        }
#endif

        // 获得邮件模板
        int GetMailTemplate(
            string strType, // dpmail email 等
            string strTemplateName,
            out string strText,
            out string strError)
        {
            strError = "";
            strText = "";

            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("mailTemplates/template[@name='" + strTemplateName + "']");
            if (nodes.Count == 0)
                return 0;   // not found

            foreach (XmlNode node in nodes)
            {
                string strCurrentType = DomUtil.GetAttr(node, "type");
                if (strType == strCurrentType)
                {
                    strText = node.InnerText;
                    return 1;
                }
            }

            // 如果没有找到，就采用第一个
            // 这是为了兼容以前没有type属性的<template>用法
            strText = nodes[0].InnerText;
            return 1;
        }

        // 根据模板和值表，替换出最终的文字
        int GetMailText(string strTemplate,
            Hashtable valueTable,
            out string strText,
            out string strError)
        {
            strError = "";

            strText = strTemplate;
            foreach (string strKey in valueTable.Keys)
            {
                string strValue = (string)valueTable[strKey];

                strText = strText.Replace(strKey, strValue);
            }

            return 0;
        }

        // 借阅API的从属函数
        // 检查预约相关信息
        // text-level: 用户提示
        // return:
        //      -1  error
        //      0   正常
        //      1   发现该册被保留， 不能借阅
        //      2   发现该册预约， 不能续借
        //      3   发现该册被保留， 不能借阅。而且本函数修改了册记录(<location>元素发生了变化)，需要本函数返回后，把册记录保存。
        int DoBorrowReservationCheck(
            SessionInfo sessioninfo,
            bool bRenew,
            ref XmlDocument readerdom,
            ref XmlDocument itemdom,
            bool bForce,
            out string strError)
        {
            strError = "";

            int nRet = 0;
            long lRet = 0;

            // 获得例行参数
            string strRefID = DomUtil.GetElementText(itemdom.DocumentElement,
"refID");
            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");

            string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                "barcode");
            string strItemBarcodeParam = strItemBarcode;

            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
#if NO
                // text-level: 内部错误
                strError = "册记录中册条码号不能为空";
                return -1;
#endif
                // 如果册条码号为空，则使用 参考ID
                if (String.IsNullOrEmpty(strRefID) == true)
                {
                    // text-level: 内部错误
                    strError = "册记录中册条码号和参考ID不应同时为空";
                    return -1;
                }
                strItemBarcodeParam = "@refID:" + strRefID;
            }

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                // text-level: 内部错误
                strError = "读者记录中读者证条码号不能为空";
                return -1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            XmlNodeList nodesReservationRequest = itemdom.DocumentElement.SelectNodes("reservations/request");

            // 续借处理
            if (bRenew == true)
            {
                if (nodesReservationRequest.Count > 0)
                {
                    string strList = "";
                    for (int i = 0; i < nodesReservationRequest.Count; i++)
                    {
                        XmlNode node = nodesReservationRequest[i];

                        string strReader = DomUtil.GetAttr(node, "reader");

                        if (strList != "")
                            strList += ",";
                        strList += strReader;
                    }

                    // 如果预约操作者为普通读者，则strList不便显示
                    if (sessioninfo.UserType == "reader")
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("续借操作被拒绝。因册s当前已被s位读者预约"),    // "续借操作被拒绝。因 册 {0} 当前已被 {1} 位读者预约。为方便他人，请尽早还回此书，谢谢。"
                            strItemBarcodeParam,
                            nodesReservationRequest.Count.ToString());
                        // "续借操作被拒绝。因 册 " + strItemBarcode + " 当前已被 " + nodesReservationRequest.Count.ToString() + " 位读者预约。为方便他人，请尽早还回此书，谢谢。";
                    }
                    else
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("续借操作被拒绝。因册s当前已被下列读者预约s"),  // "续借操作被拒绝。因 册 {0} 当前已被下列读者预约: {1}。为方便他人，请尽早还回此书，谢谢。"
                            strItemBarcodeParam,
                            strList);
                        // "续借操作被拒绝。因 册 " + strItemBarcode + " 当前已被下列读者预约: " + strList + "。为方便他人，请尽早还回此书，谢谢。";
                    }
                    // 当前持有此书的读者无法续借，则只好在到期前归还图书馆。
                    return 2;
                }
            }

            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement,
                "location");

            // 用册记录<location>中是否有#reservation字样来判断似乎并不充分。
            // 应当也看看<reservations/request>是否存在。
            if (nodesReservationRequest.Count == 0
                && StringUtil.IsInList("#reservation", strLocation) == false)// 看看这册是否属于在预约保留架上的
                return 0;

            int nRedoLoadCount = 0;

        REDO_LOAD_QUEUE_REC:

            // 进一步检索预约到书库, 看看是否属于已经通知来取书的册, 或者是等待上普通架的预约超期未取册
            string strNotifyXml = "";
            string strOutputPath = "";
            byte[] timestamp = null;
            // 获得预约到书队列记录
            // return:
            //      -1  error
            //      0   not found
            //      1   命中1条
            //      >1  命中多于1条
            nRet = GetArrivedQueueRecXml(
                // sessioninfo.Channels,
                channel,
                strItemBarcodeParam,    // strItemBarcode
                out strNotifyXml,
                out timestamp,
                out strOutputPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
            {
                // 虽然册location中有#reservation，但是通知队列中并没有这个记录
                // 记住改变册记录的location
                goto CHANGEITEMLOCATION;
            }

            XmlDocument notifydom = new XmlDocument();
            try
            {
                notifydom.LoadXml(strNotifyXml);
            }
            catch (Exception ex)
            {
                // text-level: 内部错误
                strError = "装载预约到书通知记录XML到DOM时出错: " + ex.Message;
                return -1;
            }

            // RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);

            string strState = DomUtil.GetElementText(notifydom.DocumentElement,
                "state");
            if (StringUtil.IsInList("outof", strState) == true)
            {
                // 通知记录存在, 但是已经超期, 正好当前读者借走这册, 可以删除这个通知记录了。注意修改册记录的location，去掉#reservation


            }
            else
            {
                // 检查是不是正好通知的本读者取书
                string strNotifyReaderBarcode = DomUtil.GetElementText(notifydom.DocumentElement,
                    "readerBarcode");

                // 正好是本读者来取书了
                if (strNotifyReaderBarcode == strReaderBarcode)
                {
                    // 删除读者记录中的reservation通知行


                    // 在册记录中，删除相关的<reservations/request>元素 2007/1/17
                    XmlNodeList nodes = itemdom.DocumentElement.SelectNodes("reservations/request[@reader='" + strReaderBarcode + "']");
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        XmlNode node = nodes[i];
                        node.ParentNode.RemoveChild(node);
                    }
                }
                else // 不是预约者来借书
                {

                    // 如果是在架预约的通知情形
                    if (StringUtil.IsInList("#reservation", strLocation) == false)
                    {
                        // 需要修改册记录的<location>为包含#reservation标志，并且把队列记录的<itemBarcode>的onShelf设置为false。
                        if (strLocation != "")
                            strLocation += ",";
                        strLocation += "#reservation";
                        DomUtil.SetElementText(itemdom.DocumentElement, "location", strLocation);

                        // 修改预约通知记录
                        //XmlNode nodeItemBarcode = notifydom.DocumentElement.SelectSingleNode("itemBarcode");
                        //if (nodeItemBarcode != null)
                        {
                            // DomUtil.SetAttr(nodeItemBarcode, "onShelf", "false");
                            DomUtil.SetElementText(notifydom.DocumentElement, "onShelf", "false");  // 2015/5/7

                            byte[] output_timestamp = null;
                            string strTempOutputPath = "";

                            lRet = channel.DoSaveTextRes(strOutputPath,
                                notifydom.OuterXml,
                                false,
                                "content",  // ,ignorechecktimestamp",
                                timestamp,
                                out output_timestamp,
                                out strTempOutputPath,
                                out strError);
                            if (lRet == -1)
                            {
                                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                                    && nRedoLoadCount < 10)
                                {
                                    nRedoLoadCount++;
                                    goto REDO_LOAD_QUEUE_REC;
                                }

                                // text-level: 内部错误
                                strError = "写回队列记录 '" + strOutputPath + "' 时发生错误 : " + strError;
                                this.WriteErrorLog("借阅操作中遇在架预约图书需要写回队列记录 " + strOutputPath + " 时出错: " + strError);
                                return -1;
                            }
                        }

                        // text-level: 用户提示
                        strError = string.Format(this.GetString("借阅操作被拒绝。因为册s为读者s所在架预约，已处于保留和通知取书状态"),
                            // "借阅操作被拒绝。因为 册 {0} 为读者 {1} 所(在架)预约，已处于保留和通知取书状态。\r\n图书馆员请注意：虽然本次借阅操作被拒绝，但此册地点信息已被软件自动修改为在预约保留架(而不是在原来的普通架)，请收下此书后放入预约保留架(的特定位置，例如“曾在架”部分)。"
                            strItemBarcodeParam,
                            strNotifyReaderBarcode);
                        // "借阅操作被拒绝。因为 册 " + strItemBarcode + " 为读者 " + strNotifyReaderBarcode + " 所(在架)预约，已处于保留和通知取书状态。\r\n图书馆员请注意：虽然本次借阅操作被拒绝，但此册地点信息已被软件自动修改为在预约保留架(而不是在原来的普通架)，请收下此书后放入预约保留架(的特定位置，例如“曾在架”部分)。";
                        return 3;
                    }

                    // text-level: 用户提示
                    strError = string.Format(this.GetString("借阅操作被拒绝。因为册s为读者s所预约，已处于保留和通知取书状态"),
                        // "借阅操作被拒绝。因为 册 {0} 为读者 {1} 所预约，已处于保留和通知取书状态。"
                        strItemBarcodeParam,
                        strNotifyReaderBarcode);
                    // "借阅操作被拒绝。因为 册 " + strItemBarcode + " 为读者 " + strNotifyReaderBarcode + " 所预约，已处于保留和通知取书状态。";
                    return 1;
                }
            }

            // 删除通知队列记录
            {
                byte[] output_timestamp = null;
                int nRedoCount = 0;
            REDO_DELETE:
                lRet = channel.DoDeleteRes(strOutputPath,
                    timestamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                        && nRedoCount < 10)
                    {
                        nRedoCount++;
                        timestamp = output_timestamp;
                        goto REDO_DELETE;
                    }
                    // 写入错误日志?
                    this.WriteErrorLog("在借阅操作中，删除预约到书库记录 '" + strOutputPath + "' 出错: " + strError);
                }
            }

        CHANGEITEMLOCATION:

            // StringUtil.RemoveFromInList("#reservation", true, ref strLocation);
            strLocation = StringUtil.GetPureLocationString(strLocation);

            DomUtil.SetElementText(itemdom.DocumentElement,
                "location", strLocation);

            // 无论如何，均试探性清除读者记录中可能的预约信息

            // 在读者记录中加入或删除预约信息
            // parameters:
            //      strFunction "new"新增预约信息；"delete"删除预约信息; "merge"合并; "split"拆散
            // return:
            //      -1  error
            //      0   unchanged
            //      1   changed
            nRet = DoReservationReaderXml(
                "delete",
                strItemBarcodeParam,    // strItemBarcode
                sessioninfo.Account.UserID,
                ref readerdom,
                out strError);
            if (nRet == -1)
            {
                // 写入错误日志?
                this.WriteErrorLog("借阅操作中, 在读者记录中删除潜在的预约信息时(调用DoReservationReaderXml() function=delete itembarcode=" + strItemBarcodeParam + ")出错: " + strError);
            }

            return 0;
        }

        // 借阅API的从属函数
        // 在读者记录和册记录中加入借书信息
        // text-level: 用户提示
        // parameters:
        //      domOperLog 构造日志记录DOM
        //      this_return_time    本次借阅的应还最后时间。GMT时间。
        // return:
        //      -1  error
        //      0   正常
        //      // 1   发现先前借阅的图书目前有超期情况
        int DoBorrowReaderAndItemXml(
            bool bRenew,
            string strLibraryCode,
            ref XmlDocument readerdom,
            ref XmlDocument itemdom,
            bool bForce,
            string strOperator,
            string strItemRecPath,
            string strReaderRecPath,
            ref XmlDocument domOperLog,
            out BorrowInfo borrow_info,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            borrow_info = new BorrowInfo();

            DateTime this_return_time = new DateTime(0);

            LibraryApplication app = this;

            // 获得例行参数
            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");

            string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                "barcode");

            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
#if NO
                // text-level: 内部错误
                strError = "册记录中册条码号不能为空";
                return -1;
#endif
                // 如果册条码号为空，则记载 参考ID
                string strRefID = DomUtil.GetElementText(itemdom.DocumentElement,
    "refID");
                if (String.IsNullOrEmpty(strRefID) == true)
                {
                    // text-level: 内部错误
                    strError = "册记录中册条码号和参考ID不应同时为空";
                    return -1;
                }
                strItemBarcode = "@refID:" + strRefID;
            }

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                // text-level: 内部错误
                strError = "读者记录中读者证条码号不能为空";
                return -1;
            }

            // 从想要借阅的册信息中，找到图书类型
            string strBookType = DomUtil.GetElementText(itemdom.DocumentElement, "bookType");

            // 从读者信息中, 找到读者类型
            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement, "readerType");


            // 修改读者记录
            int nNo = 0;

            XmlNode nodeBorrow = null;

            if (bRenew == true)
            {
                // 看看是否已经有先前已经借阅的册
                nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']");
                if (nodeBorrow == null)
                {
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("该读者未曾借阅过册s，因此无法续借"),
                        // "该读者未曾借阅过册 '{0}'，因此无法续借。"
                        strItemBarcode);
                    // "该读者未曾借阅过册 '" + strItemBarcode + "'，因此无法续借。";
                    return -1;
                }

                // 获得上次的序号
                string strNo = DomUtil.GetAttr(nodeBorrow, "no");
                if (String.IsNullOrEmpty(strNo) == true)
                    nNo = 0;
                else
                {
                    try
                    {
                        nNo = Convert.ToInt32(strNo);
                    }
                    catch
                    {
                        if (bForce == false)
                        {
                            // text-level: 内部错误
                            strError = "读者记录中 XML 片断 " + nodeBorrow.OuterXml + "其中 no 属性值'" + strNo + "' 格式错误";
                            return -1;
                        }
                        nNo = 0;
                    }
                }

            }
            else // bRenew == false
            {
                // 检查<borrows>元素是否存在
                XmlNode root = readerdom.DocumentElement.SelectSingleNode("borrows");
                if (root == null)
                {
                    root = readerdom.CreateElement("borrows");
                    root = readerdom.DocumentElement.AppendChild(root);
                }

                // 加入借阅册信息
                nodeBorrow = readerdom.CreateElement("borrow");
                nodeBorrow = DomUtil.InsertFirstChild(root, nodeBorrow); // 2006/12/24 changed，2015/1/12 增加等号左边的部分 
                // nodeBorrow = root.AppendChild(nodeBorrow);
            }

            //
            string strThisBorrowPeriod = "10day";   // 本次借阅的期限
            string strLastBorrowPeriod = "";    // 上次借阅的期限

            // barcode
            DomUtil.SetAttr(nodeBorrow, "barcode", strItemBarcode);

            // 记载册记录路径
            if (String.IsNullOrEmpty(strItemRecPath) == false)
            {
                DomUtil.SetAttr(nodeBorrow, "recPath", strItemRecPath); // 2006/12/24
                string strParentID = DomUtil.GetElementText(itemdom.DocumentElement, "parent");

                string strBiblioRecPath = "";
                // 通过册记录路径和parentid得知从属的种记录路径
                // parameters:
                // return:
                //      -1  error
                //      1   找到
                nRet = GetBiblioRecPathByItemRecPath(
                    strItemRecPath,
                    strParentID,
                    out strBiblioRecPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据册记录路径 '" + strItemRecPath + "' 和 parent_id '" + strParentID + "' 获得书目库路径时出错: " + strError;
                    return -1;
                }
                DomUtil.SetAttr(nodeBorrow, "biblioRecPath", strBiblioRecPath); // 2015/10/2
            }

            // 加入借期字段
            // 读者记录中的借期字段，目的是为了查询方便，但注意没有法律效力。
            // 真正对超期判断起作用的，是册记录中的借期字段。
            string strBorrowPeriodList = "";
            MatchResult matchresult;
            // return:
            //      reader和book类型均匹配 算4分
            //      只有reader类型匹配，算3分
            //      只有book类型匹配，算2分
            //      reader和book类型都不匹配，算1分
            nRet = app.GetLoanParam(
                //null,
                strLibraryCode,
                strReaderType,
                strBookType,
                "借期",
                out strBorrowPeriodList,
                out matchresult,
                out strError);
            if (nRet == -1)
            {
                if (bForce == true)
                    goto DOCHANGE;
                // text-level: 内部错误
                strError = "借阅失败。获得 馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数时发生错误: " + strError;
                return -1;
            }
            if (nRet < 4)  // nRet == 0
            {
                if (bForce == true)
                    goto DOCHANGE;

                // text-level: 内部错误
                strError = "借阅失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数无法获得: " + strError;
                return -1;
            }

            // 按照逗号分列值，需要根据序号取出某个参数

            string[] aPeriod = strBorrowPeriodList.Split(new char[] { ',' });

            if (aPeriod.Length == 0)
            {
                if (bForce == true)
                    goto DOCHANGE;

                // text-level: 内部错误
                strError = "借阅失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数 '" + strBorrowPeriodList + "'格式错误";
                return -1;
            }

            if (bRenew == true)
            {
                nNo++;
                if (nNo >= aPeriod.Length)
                {
                    if (aPeriod.Length == 1)
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("续借失败。读者类型s针对图书类型s的借期参数值s规定，不能续借"),
                            // "续借失败。读者类型 '{0}' 针对图书类型 '{1}' 的 借期 参数值 '{2}' 规定，不能续借。(所定义的一个期限，是指第一次借阅的期限)"
                            strReaderType,
                            strBookType,
                            strBorrowPeriodList);

                        // "续借失败。读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数值 '" + strBorrowPeriodList + "' 规定，不能续借。(所定义的一个期限，是指第一次借阅的期限)";
                    }
                    else
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("续借失败。读者类型s针对图书类型s的借期参数值s规定，只能续借s次"),
                            // "续借失败。读者类型 '{0}' 针对图书类型 '{1}' 的 借期 参数值 '{2}' 规定，只能续借 {3} 次。"
                            strReaderType,
                            strBookType,
                            strBorrowPeriodList,
                            Convert.ToString(aPeriod.Length - 1));
                        // "续借失败。读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数值 '" + strBorrowPeriodList + "' 规定，只能续借 " + Convert.ToString(aPeriod.Length - 1) + " 次。";
                    }
                    return -1;
                }
                strThisBorrowPeriod = aPeriod[nNo].Trim();
                strLastBorrowPeriod = aPeriod[nNo - 1].Trim();

                if (String.IsNullOrEmpty(strThisBorrowPeriod) == true)
                {
                    if (bForce == true)
                        goto DOCHANGE;

                    // text-level: 内部错误
                    strError = "借阅失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数 '" + strBorrowPeriodList + "' 格式错误：第 " + Convert.ToString(nNo) + "个部分为空。";
                    return -1;
                }
            }
            else
            {
                strThisBorrowPeriod = aPeriod[0].Trim();

                if (String.IsNullOrEmpty(strThisBorrowPeriod) == true)
                {
                    if (bForce == true)
                        goto DOCHANGE;

                    // text-level: 内部错误
                    strError = "借阅失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数 '" + strBorrowPeriodList + "' 格式错误：第一部分为空。";
                    return -1;
                }
            }

            // 检查strBorrowPeriod是否合法
            {
                long lPeriodValue = 0;
                string strPeriodUnit = "";
                nRet = LibraryApplication.ParsePeriodUnit(
                    strThisBorrowPeriod,
                    out lPeriodValue,
                    out strPeriodUnit,
                    out strError);
                if (nRet == -1)
                {
                    if (bForce == true)
                        goto DOCHANGE;

                    // text-level: 内部错误
                    strError = "借阅失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数 '" + strBorrowPeriodList + "' 格式错误：'" +
                         strThisBorrowPeriod + "' 格式错误: " + strError;
                    return -1;
                }
            }

        DOCHANGE:

            // 测算本次 借阅/续借 的应还书时间
            DateTime now = app.Clock.UtcNow;  //  今天，当下。GMT时间

            {
                long lPeriodValue = 0;
                string strPeriodUnit = "";
                nRet = LibraryApplication.ParsePeriodUnit(
                    strThisBorrowPeriod,
                    out lPeriodValue,
                    out strPeriodUnit,
                    out strError);
                if (nRet == -1)
                    goto SKIP_CHECK_RENEW_PERIOD;

                DateTime nextWorkingDay;

                // parameters:
                //      calendar    工作日历。如果为null，表示函数不进行非工作日判断。
                // return:
                //      -1  出错
                //      0   成功。timeEnd在工作日范围内。
                //      1   成功。timeEnd正好在非工作日。nextWorkingDay已经返回了下一个工作日的时间
                nRet = GetReturnDay(
                    null,
                    now,
                    lPeriodValue,
                    strPeriodUnit,
                    out this_return_time,
                    out nextWorkingDay,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "测算这次还书时间过程发生错误: " + strError;
                    return -1;
                }

                // 正规化时间
                nRet = RoundTime(strPeriodUnit,
                    ref this_return_time,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "测算这次还书时间过程发生错误: " + strError;
                    return -1;
                }
            }

            // 如果是续借，检查不续借的应还最后日期和续借后的应还最后日期哪个靠后。
            // 如果不续借的日期还靠后，则不许读者续借。
            if (bRenew == true)
            {
                // 上次借阅日
                string strLastBorrowDate = DomUtil.GetAttr(nodeBorrow, "borrowDate");
                if (String.IsNullOrEmpty(strLastBorrowDate) == true)
                    goto SKIP_CHECK_RENEW_PERIOD;

                DateTime last_borrowdate;
                try
                {
                    last_borrowdate = DateTimeUtil.FromRfc1123DateTimeString(strLastBorrowDate);
                }
                catch
                {
                    goto SKIP_CHECK_RENEW_PERIOD;
                }

                long lLastPeriodValue = 0;
                string strLastPeriodUnit = "";
                nRet = ParsePeriodUnit(strLastBorrowPeriod,
                    out lLastPeriodValue,
                    out strLastPeriodUnit,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "借阅期限 值 '" + strLastBorrowPeriod + "' 格式错误: " + strError;
                    goto SKIP_CHECK_RENEW_PERIOD;
                }

                DateTime nextWorkingDay;

                DateTime last_return_time;
                // 测算上次借书的还书日期
                nRet = GetReturnDay(
                    null,
                    last_borrowdate,
                    lLastPeriodValue,
                    strLastPeriodUnit,
                    out last_return_time,
                    out nextWorkingDay,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "测算上次还书时间过程发生错误: " + strError;
                    goto SKIP_CHECK_RENEW_PERIOD;
                }

                // 正规化时间
                nRet = RoundTime(strLastPeriodUnit,
                    ref last_return_time,
                    out strError);
                if (nRet == -1)
                    goto SKIP_CHECK_RENEW_PERIOD;

                TimeSpan delta = last_return_time - this_return_time;

                if (delta.Ticks > 0)
                {
                    strError = string.Format(this.GetString("本次续借操作被拒绝。假如这次续借实行后，应还日期将反而提前了"),
                        // "本次续借操作被拒绝。假如这次续借实行后，应还日期将为 {0} (注：续借借期从当日开始计算。从今日开始借期为 {1})；而如果您不续借，应还日期本来为 {2} (注：从 {3} 开始借期为 {4} )。即，续借后的应还日期反而提前了。"
                        GetLocalTimeString(strLastPeriodUnit, this_return_time),
                        GetDisplayTimePeriodStringEx(strThisBorrowPeriod),
                        GetLocalTimeString(strLastPeriodUnit, last_return_time),
                        GetLocalTimeString(strLastPeriodUnit, last_borrowdate),
                        GetDisplayTimePeriodStringEx(strLastBorrowPeriod));
                    /*
                    // 2008/5/8 changed
                    strError = "本次续借操作被拒绝。假如这次续借实行后，应还日期将为 "
                        + GetLocalTimeString(strLastPeriodUnit, this_return_time)
                        + " (注：续借借期从当日开始计算。从今日开始借期为 "
                        + GetDisplayTimePeriodString(strThisBorrowPeriod)
                        + " )；而如果您不续借，应还日期本来为 " 
                        + GetLocalTimeString(strLastPeriodUnit, last_return_time) // this_return_time.ToString() BUG!!!
                        + " (注：从 "
                        + GetLocalTimeString(strLastPeriodUnit, last_borrowdate)
                        + " 开始借期为 "
                        + GetDisplayTimePeriodString(strLastBorrowPeriod)
                        + " )。即，续借后的应还日期反而提前了。";
                     * */
                    return -1;
                }
            }

        SKIP_CHECK_RENEW_PERIOD:

            string strRenewComment = "";

            string strBorrowDate = app.Clock.GetClock();

            string strLastReturningDate = "";   // 上次的应还时间

            if (bRenew == true)
            {
                strLastReturningDate = DomUtil.GetAttr(nodeBorrow, "returningDate");

                // 修正序号
                nNo = Math.Max(nNo, 1);

                // 保存前一次借阅的信息
                strRenewComment = DomUtil.GetAttr(nodeBorrow, "renewComment");

                if (strRenewComment != "")
                    strRenewComment += "; ";

                strRenewComment += "no=" + Convert.ToString(nNo - 1) + ", ";
                strRenewComment += "borrowDate=" + DomUtil.GetAttr(nodeBorrow, "borrowDate") + ", ";
                strRenewComment += "borrowPeriod=" + DomUtil.GetAttr(nodeBorrow, "borrowPeriod") + ", ";
                strRenewComment += "returnDate=" + strBorrowDate + ", ";
                strRenewComment += "operator=" + DomUtil.GetAttr(nodeBorrow, "operator");
            }

            // borrowDate
            DomUtil.SetAttr(nodeBorrow, "borrowDate",
                strBorrowDate);

            if (nNo > 0)    // 2013/12/23
                DomUtil.SetAttr(nodeBorrow, "no", Convert.ToString(nNo));

            DomUtil.SetAttr(nodeBorrow, "borrowPeriod", strThisBorrowPeriod);

            // 2014/11/14
            // returningDate
            string strReturningDate = DateTimeUtil.Rfc1123DateTimeStringEx(this_return_time.ToLocalTime());
            DomUtil.SetAttr(nodeBorrow, "returningDate",
                strReturningDate);

            // 2014/11/14
            // lastReturningDate

            if (nNo > 0)
                DomUtil.SetAttr(nodeBorrow, "lastReturningDate",
                    strLastReturningDate);

            if (string.IsNullOrEmpty(strRenewComment) == false)    // 2013/12/23
                DomUtil.SetAttr(nodeBorrow, "renewComment", strRenewComment);

            DomUtil.SetAttr(nodeBorrow, "operator", strOperator);

            // 2007/11/5
            DomUtil.SetAttr(nodeBorrow, "type", strBookType);   // 在读者记录<borrows/borrow>元素中写入type属性，内容为图书册类型，便于后续借书的时候判断某一种册类型是否超过读者权限规定值。这种方式可以节省时间，不必从多个册记录中去获得册类型字段

            // 2006/11/12
            string strBookPrice = DomUtil.GetElementText(itemdom.DocumentElement, "price");
            DomUtil.SetAttr(nodeBorrow, "price", strBookPrice);   // 在读者记录<borrows/borrow>元素中写入price属性，内容为图书册价格类型，便于后续借书的时候判断已经借的和即将借的总价格是否超过读者的押金余额。这种方式可以节省时间，不必从多个册记录中去获得册价格字段

            // 修改册记录
            string strOldReaderBarcode = "";

            strOldReaderBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                "borrower");

            if (bRenew == false)
            {
                if (bForce == false
                    && String.IsNullOrEmpty(strOldReaderBarcode) == false)
                {
                    // 2007/1/2
                    if (strOldReaderBarcode == strReaderBarcode)
                    {
                        // text-level: 用户提示
                        strError = "借阅操作被拒绝。因册 '" + strItemBarcode + "' 在本次操作前已经被当前读者 '" + strReaderBarcode + "' 借阅了。";
                        return -1;
                    }

                    // text-level: 用户提示
                    strError = "借阅操作被拒绝。因册 '" + strItemBarcode + "' 在本次操作前已经处于被读者 '" + strOldReaderBarcode + "' 借阅(持有)状态(尚未归还)。\r\n如果属于拿错情况，请工作人员立即扣留此书，设法交还给持有人；\r\n如果确系(经持有人同意)其他读者要转借此册，需先履行还书手续；\r\n如果持有人要续借此册，请履行续借手续。";
                    return -1;
                }
            }

            DomUtil.SetElementText(itemdom.DocumentElement,
                "borrower", strReaderBarcode);

            // 2008/9/18
            DomUtil.SetElementText(itemdom.DocumentElement,
                "borrowerReaderType", strReaderType);

            // 2012/9/8
            DomUtil.SetElementText(itemdom.DocumentElement,
                "borrowerRecPath", strReaderRecPath);

            DomUtil.SetElementText(itemdom.DocumentElement,
                "borrowDate",
                strBorrowDate);

            if (nNo > 0)    // 2013/12/23
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "no",
                    Convert.ToString(nNo));

            DomUtil.SetElementText(itemdom.DocumentElement,
                "borrowPeriod",
                strThisBorrowPeriod);   // strBorrowPeriod现在已经是个别参数，不是逗号分隔的列举值了

            DomUtil.SetElementText(itemdom.DocumentElement,
                "returningDate",
                strReturningDate);

            if (nNo > 0)
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "lastReturningDate",
                    strLastReturningDate);

            if (string.IsNullOrEmpty(strRenewComment) == false) // 2013/12/23
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "renewComment",
                    strRenewComment);

            DomUtil.SetElementText(itemdom.DocumentElement,
                "operator",
                strOperator);
            // 注：虽然是借书操作本来不操作 borrowHistory 元素，但有这样一种情况，以前的极限数量较大，后来极限数量改小了，由于目前并没有专门批处理册记录的模块，所以希望借书时候顺便把元素数量删减，这样后面创建日志记录的时候，在日志记录里面记载的也就是尺寸减小后的册记录内容了
            // 删除超过极限数量的 BorrowHistory/borrower 元素
            // return:
            //      -1  出错
            //      0   册记录没有改变
            //      1   册记录发生改变
            nRet = RemoveItemHistoryItems(itemdom,
                out strError);
            if (nRet == -1)
                return -1;

            // 创建日志记录
            DomUtil.SetElementText(domOperLog.DocumentElement, "readerBarcode",
                strReaderBarcode);     // 读者证条码号
            DomUtil.SetElementText(domOperLog.DocumentElement, "itemBarcode",
                strItemBarcode);    // 册条码号
            DomUtil.SetElementText(domOperLog.DocumentElement, "borrowDate",
                strBorrowDate);     // 借阅日期
            DomUtil.SetElementText(domOperLog.DocumentElement, "borrowPeriod",
                strThisBorrowPeriod);   // 借阅期限
            DomUtil.SetElementText(domOperLog.DocumentElement, "returningDate",
                strReturningDate);     // 应还日期

            // 2015/1/12
            DomUtil.SetElementText(domOperLog.DocumentElement, "type",
    strBookType);    // 图书类型
            DomUtil.SetElementText(domOperLog.DocumentElement, "price",
strBookPrice);    // 图书价格

            // TODO: 0 需要忽略写入
            DomUtil.SetElementText(domOperLog.DocumentElement, "no",
                Convert.ToString(nNo)); // 续借次数

            if (nNo > 0)
                DomUtil.SetElementText(domOperLog.DocumentElement, "lastReturningDate",
    strLastReturningDate);     // 上次应还日期

            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                strOperator);   // 操作者
            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                strBorrowDate);   // 操作时间

            // 返回借阅成功的信息

            // 返回满足RFC1123的时间值字符串 GMT时间
            borrow_info.LatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringEx(this_return_time.ToLocalTime());
            borrow_info.Period = strThisBorrowPeriod;
            borrow_info.BorrowCount = nNo;

            // 2011/6/26
            borrow_info.BorrowOperator = strOperator;
            /*
            borrow_info.BookType = strBookType;
            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement,
                "location");
            borrow_info.Location = strLocation;
             * */

            return 0;
        }

        // 2015/10/9
        // 删除超过极限数量的 BorrowHistory/borrower 元素
        // return:
        //      -1  出错
        //      0   册记录没有改变
        //      1   册记录发生改变
        int RemoveItemHistoryItems(XmlDocument itemdom,
            out string strError)
        {
            strError = "";

            XmlNodeList nodes = itemdom.DocumentElement.SelectNodes("borrowHistory/borrower");
            if (nodes.Count > this.MaxItemHistoryItems)
            {
                for (int i = this.MaxItemHistoryItems; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    node.ParentNode.RemoveChild(node);
                }
                return 1;
            }

            return 0;
        }

        // 将时间值的本地时间，按照单位转换为适当的显示字符串
        // 2008/5/7
        static string GetLocalTimeString(string strUnit,
            DateTime time)
        {
            if (strUnit == "day")
                return time.ToLocalTime().ToString("d");   // "yyyy-MM-dd"
            if (strUnit == "hour")
                return time.ToLocalTime().ToString("G");

            return time.ToLocalTime().ToString("G");
        }

        // 借阅API的从属函数
        // 检查当前是否有潜在的超期册
        // text-level: 用户提示
        // return:
        //      -1  error
        //      0   没有超期册
        //      1   有超期册
        int CheckOverdue(
            Calendar calendar,
            XmlDocument readerdom,
            bool bForce,
            out string strError)
        {
            strError = "";
            int nOverCount = 0;
            int nRet = 0;

            LibraryApplication app = this;


            string strOverdueItemBarcodeList = "";

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");
            XmlNode node = null;
            if (nodes.Count > 0)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    node = nodes[i];
                    string strBarcode = DomUtil.GetAttr(node, "barcode");
                    string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                    string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                    string strOperator = DomUtil.GetAttr(node, "operator");

                    // return:
                    //      -1  数据格式错误
                    //      0   没有发现超期
                    //      1   发现超期   strError中有提示信息
                    //      2   已经在宽限期内，很容易超期 2009/3/13
                    nRet = app.CheckPeriod(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out strError);
                    if (nRet == -1)
                    {
                        if (bForce == true)
                            continue;
                        // text-level: 内部错误
                        strError = "读者记录中 有关册 '" + strBarcode + "' 的借阅期限信息检查出现错误：" + strError;
                    }

                    if (nRet == 1)
                    {
                        if (strOverdueItemBarcodeList != "")
                            strOverdueItemBarcodeList += ",";
                        strOverdueItemBarcodeList += strBarcode;
                        nOverCount++;
                    }
                }

                // 发现未归还的册中出现了超期情况
                if (nOverCount > 0)
                {
                    // strError = "该读者当前有 " + Convert.ToString(nOverCount) + " 个未还超期册: " + strOverdueItemBarcodeList + " ，因此借阅操作被拒绝。请读者尽快将这些已超期册履行还书手续。";

                    // text-level: 用户提示
                    strError = string.Format(this.GetString("该读者当前有s个未还超期册"),   // "该读者当前有 {0} 个未还超期册: {1}"
                        Convert.ToString(nOverCount),
                        strOverdueItemBarcodeList);

                    // "该读者当前有 " + Convert.ToString(nOverCount) + " 个未还超期册: " + strOverdueItemBarcodeList + ""; // ，因此借阅(或续借)操作被拒绝。请读者尽快将这些已超期册履行还书手续。
                    return 1;
                }
            }

            return 0;
        }

        // 检查借阅证是否超期，是否有挂失等状态
        // 2006/8/23 但是 尚未测试
        // text-level: 用户提示 OPAC预约功能要调用此函数
        // return:
        //      -1  检测过程发生了错误。应当作不能借阅来处理
        //      0   可以借阅
        //      1   证已经过了失效期，不能借阅
        //      2   证有不让借阅的状态
        public int CheckReaderExpireAndState(XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            string strExpireDate = DomUtil.GetElementText(readerdom.DocumentElement, "expireDate");
            if (String.IsNullOrEmpty(strExpireDate) == false)
            {
                DateTime expireDate;
                try
                {
                    expireDate = DateTimeUtil.FromRfc1123DateTimeString(strExpireDate);
                }
                catch
                {
                    // text-level: 内部错误
                    strError = string.Format(this.GetString("借阅证失效期值s格式错误"), // "借阅证失效期<expireDate>值 '{0}' 格式错误"
                        strExpireDate);

                    // "借阅证失效期<expireDate>值 '" + strExpireDate + "' 格式错误";
                    return -1;
                }

                DateTime now = this.Clock.UtcNow;

                if (expireDate <= now)
                {
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("今天s已经超过借阅证失效期s"),  // "今天({0})已经超过借阅证失效期({1})。"
                        now.ToLocalTime().ToLongDateString(),
                        expireDate.ToLocalTime().ToLongDateString());
                    // "今天(" + now.ToLocalTime().ToLongDateString() + ")已经超过借阅证失效期(" + expireDate.ToLocalTime().ToLongDateString() + ")。";
                    return 1;
                }

            }

            string strState = DomUtil.GetElementText(readerdom.DocumentElement, "state");
            if (String.IsNullOrEmpty(strState) == false)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("借阅证的状态为s"), // "借阅证的状态为 '{0}'。"
                    strState);
                // "借阅证的状态为 '" + strState + "'。";
                return 2;
            }

            return 0;
        }


        // 清除读者和册记录中的已到预约事项，并提取下一个预约读者证条码号
        // 本函数还负责清除册记录中以前残留的state=arrived的<request>元素
        // parameters:
        //      strItemBarcode  册条码号。支持 "@refID:" 前缀用法
        //      bMaskLocationReservation    不要给册记录<location>打上#reservation标记
        //      strReservationReaderBarcode 返回下一个预约读者的证条码号
        public int ClearArrivedInfo(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strReaderBarcode,
            string strItemBarcode,
            bool bDontMaskLocationReservation,
            out string strReservationReaderBarcode,
            out string strError)
        {
            strError = "";

            byte[] timestamp = null;
            byte[] output_timestamp = null;
            string strOutputPath = "";
            long lRet = 0;
            int nRet = 0;
            strReservationReaderBarcode = "";

#if NO
            RmsChannel channel = null;
            channel = channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            bool bDontLock = false;

            // 加读者记录锁
            try
            {
#if DEBUG_LOCK_READER
                this.WriteErrorLog("ClearArrivedInfo 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif
                this.ReaderLocks.LockForWrite(strReaderBarcode);
            }
            catch (System.Threading.LockRecursionException)
            {
                // 2012/5/31
                // 有可能本函数被DigitalPlatform.LibraryServer.LibraryApplication.Reservation()调用时，已经对读者记录加了锁
                bDontLock = true;
#if DEBUG_LOCK_READER
                this.WriteErrorLog("ClearArrivedInfo 开始为读者加写锁 '" + strReaderBarcode + "' 时遇到抛出 LockRecursionException 异常");
#endif

            }

            try
            {

                // 读入读者记录
                string strReaderXml = "";
                string strOutputReaderRecPath = "";
                nRet = this.GetReaderRecXml(
                    // channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    goto DOITEM;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    return -1;
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    return -1;
                }

                // 从当前读者记录中删除有关字段
                XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("reservations/request");
                XmlNode readerRequestNode = null;
                string strItems = "";
                bool bFound = false;
                for (int i = 0; i < nodes.Count; i++)
                {
                    readerRequestNode = nodes[i];
                    strItems = DomUtil.GetAttr(readerRequestNode, "items");
                    if (IsInBarcodeList(strItemBarcode, strItems) == true)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == true)
                {
                    Debug.Assert(readerRequestNode != null, "");

                    // 是清除，还是修改状态标记并保留一段？
                    // 现在是清除。如果能同时写入日志最好，以便将来查询
                    readerRequestNode.ParentNode.RemoveChild(readerRequestNode);
                }

                // 写回读者记录
                if (bFound == true)
                {
                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        readerdom.OuterXml,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "写回读者记录 '" + strOutputReaderRecPath + "' 时发生错误 : " + strError;
                        return -1;
                    }
                }

            DOITEM:
                // 顺便获得下一个预约读者证条码号
                string strItemXml = "";
                string strOutputItemRecPath = "";
                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetItemRecXml(
                    channel,
                    strItemBarcode,
                    out strItemXml,
                    out strOutputItemRecPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "册条码号 '" + strItemBarcode + "' 不存在";
                    return 0;
                }
                if (nRet == -1)
                {
                    strError = "读入册记录时发生错误: " + strError;
                    return -1;
                }

                XmlDocument itemdom = null;
                nRet = LibraryApplication.LoadToDom(strItemXml,
                    out itemdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载册记录进入XML DOM时发生错误: " + strError;
                    return -1;
                }
                // 察看本册预约情况, 如果有，提取出第一个预约读者的证条码号
                // 该函数还负责清除以前残留的state=arrived的<request>元素
                // return:
                //      -1  error
                //      0   没有修改
                //      1   进行过修改
                nRet = DoItemReturnReservationCheck(
                    bDontMaskLocationReservation,
                    ref itemdom,
                    out strReservationReaderBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
#if NO
                    channel = channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        return -1;
                    }
#endif

                    lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                        itemdom.OuterXml,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "写回册记录 '" + strOutputItemRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }

                }

            }
            finally
            {
                if (bDontLock == false)
                {
                    this.ReaderLocks.UnlockForWrite(strReaderBarcode);
#if DEBUG_LOCK_READER
                    this.WriteErrorLog("ClearArrivedInfo 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif
                }
            }

            return 1;
        }

        // 转移借阅信息
        // 将源读者记录中的<borrows>和<overdues>转移到目标读者记录中
        // result.Value:
        //      -1  error
        //      0   没有必要转移。即源读者记录中没有需要转移的借阅信息
        //      1   已经成功转移
        public LibraryServerResult DevolveReaderInfo(
            SessionInfo sessioninfo,
            string strSourceReaderBarcode,
            string strTargetReaderBarcode)
        {
            string strError = "";
            int nRet = 0;
            long lRet = 0;
            bool bChanged = false;  // 是否发生过实质性改动

            LibraryServerResult result = new LibraryServerResult();

            // 权限字符串
            if (StringUtil.IsInList("devolvereaderinfo", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "转移借阅信息操作被拒绝。不具备devolvereaderinfo权限。";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            // 检查源和目标条码号不能相同
            if (strSourceReaderBarcode == strTargetReaderBarcode)
            {
                strError = "源和目标读者记录证条码号不能相同";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strSourceReaderBarcode) == true)
            {
                strError = "源读者记录证条码号不能为空";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strTargetReaderBarcode) == true)
            {
                strError = "目标读者记录证条码号不能为空";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 对源和目标两个证条码号加锁，读入两个读者记录
            // 加锁有先后有技巧：先加条码号较小的锁定。否则容易造成死锁
            string strBarcode1 = "";
            string strBarcode2 = "";
            if (String.Compare(strSourceReaderBarcode, strTargetReaderBarcode) < 0)
            {
                strBarcode1 = strSourceReaderBarcode;
                strBarcode2 = strTargetReaderBarcode;
            }
            else
            {
                strBarcode1 = strTargetReaderBarcode;
                strBarcode2 = strSourceReaderBarcode;
            }

            try
            {
                // 加读者记录锁1
#if DEBUG_LOCK_READER
                this.WriteErrorLog("DevolveReaderInfo 开始为读者加写锁1 '" + strBarcode1 + "'");
#endif
                this.ReaderLocks.LockForWrite(strBarcode1);
                try // 读者记录锁定1范围开始
                {
                    // 加读者记录锁2
#if DEBUG_LOCK_READER
                    this.WriteErrorLog("DevolveReaderInfo 开始为读者加写锁2 '" + strBarcode2 + "'");
#endif
                    this.ReaderLocks.LockForWrite(strBarcode2);
                    try // 读者记录锁定2范围开始
                    {

                        // 读入源读者记录
                        string strSourceReaderXml = "";
                        string strSourceOutputReaderRecPath = "";
                        byte[] source_reader_timestamp = null;
                        nRet = this.GetReaderRecXml(
                            // sessioninfo.Channels,
                            channel,
                            strSourceReaderBarcode,
                            out strSourceReaderXml,
                            out strSourceOutputReaderRecPath,
                            out source_reader_timestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "源读者证条码号 '" + strSourceReaderBarcode + "' 不存在";
                            result.ErrorCode = ErrorCode.SourceReaderBarcodeNotFound;
                            return result;
                        }
                        if (nRet == -1)
                        {
                            strError = "读入源读者记录时发生错误: " + strError;
                            goto ERROR1;
                        }

                        // 2008/6/17
                        if (nRet > 1)
                        {
                            strError = "读入源读者记录时，发现读者证条码号 " + strSourceReaderBarcode + " 命中 " + nRet.ToString() + " 条，这是一个严重错误，请系统管理员尽快处理。";
                            goto ERROR1;
                        }

                        string strSourceLibraryCode = "";

                        // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                        if (String.IsNullOrEmpty(strSourceOutputReaderRecPath) == false)
                        {
                            // 检查当前操作者是否管辖这个读者库
                            // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                            if (this.IsCurrentChangeableReaderPath(strSourceOutputReaderRecPath,
                    sessioninfo.LibraryCodeList,
                    out strSourceLibraryCode) == false)
                            {
                                strError = "源读者记录路径 '" + strSourceOutputReaderRecPath + "' 从属的读者库不在当前用户管辖范围内";
                                goto ERROR1;
                            }
                        }

                        XmlDocument source_readerdom = null;
                        nRet = LibraryApplication.LoadToDom(strSourceReaderXml,
                            out source_readerdom,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "装载源读者记录进入XML DOM时发生错误: " + strError;
                            goto ERROR1;
                        }

                        // 读入目标读者记录
                        string strTargetReaderXml = "";
                        string strTargetOutputReaderRecPath = "";
                        byte[] target_reader_timestamp = null;
                        nRet = this.GetReaderRecXml(
                            // sessioninfo.Channels,
                            channel,
                            strTargetReaderBarcode,
                            out strTargetReaderXml,
                            out strTargetOutputReaderRecPath,
                            out target_reader_timestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "目标读者证条码号 '" + strTargetReaderBarcode + "' 不存在";
                            result.ErrorCode = ErrorCode.TargetReaderBarcodeNotFound;
                            return result;
                        }
                        if (nRet == -1)
                        {
                            strError = "读入目标读者记录时发生错误: " + strError;
                            goto ERROR1;
                        }

                        // 2008/6/17
                        if (nRet > 1)
                        {
                            strError = "读入目标读者记录时，发现读者证条码号 " + strTargetReaderBarcode + " 命中 " + nRet.ToString() + " 条，这是一个严重错误，请系统管理员尽快处理。";
                            goto ERROR1;
                        }

                        string strTargetLibraryCode = "";

                        // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                        if (String.IsNullOrEmpty(strTargetOutputReaderRecPath) == false)
                        {
                            // 检查当前操作者是否管辖这个读者库
                            // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                            if (this.IsCurrentChangeableReaderPath(strTargetOutputReaderRecPath,
                    sessioninfo.LibraryCodeList,
                    out strTargetLibraryCode) == false)
                            {
                                strError = "源读者记录路径 '" + strTargetOutputReaderRecPath + "' 从属的读者库不在当前用户管辖范围内";
                                goto ERROR1;
                            }
                        }

                        XmlDocument target_readerdom = null;
                        nRet = LibraryApplication.LoadToDom(strTargetReaderXml,
                            out target_readerdom,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "装载目标读者记录进入XML DOM时发生错误: " + strError;
                            goto ERROR1;
                        }

                        XmlDocument domOperLog = new XmlDocument();
                        domOperLog.LoadXml("<root />");
                        DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strSourceLibraryCode + "," + strTargetLibraryCode);    // 读者所在的馆代码
                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "operation", "devolveReaderInfo");

                        // 从逻辑日志的角度，应当说，只要有源读者证条码号和
                        // 目标读者证条码号，就足以复原
                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "sourceReaderBarcode",
                            strSourceReaderBarcode);
                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "targetReaderBarcode",
                            strTargetReaderBarcode);

                        string strOperTimeString = this.Clock.GetClock();   // RFC1123格式

                        DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                sessioninfo.UserID);
                        DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                            strOperTimeString);

                        // 准备附件可能用到的临时文件名
                        string strAttachmentFileName = this.GetTempFileName("attach");  //  Path.GetTempFileName();
                        Stream attachment = null;

                        try // 在此范围内，需注意最后删除临时文件
                        {
                            // 移动借阅信息 -- <borrows>元素内容
                            // return:
                            //      -1  error
                            //      0   not found brrowinfo
                            //      1   found and moved
                            nRet = DevolveBorrowInfo(
                                // sessioninfo.Channels,
                                channel,
                                strSourceReaderBarcode,
                                strTargetReaderBarcode,
                                strOperTimeString,
                                ref source_readerdom,
                                ref target_readerdom,
                                ref domOperLog,
                                strAttachmentFileName,
                                out attachment,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            if (nRet == 1)
                                bChanged = true;

                            // 移动超期违约金信息 -- <overdues>元素内容
                            // return:
                            //      -1  error
                            //      0   not found overdueinfo
                            //      1   found and moved
                            nRet = DevolveOverdueInfo(
                                strSourceReaderBarcode,
                                strTargetReaderBarcode,
                                strOperTimeString,
                                ref source_readerdom,
                                ref target_readerdom,
                                ref domOperLog,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            if (nRet == 1)
                                bChanged = true;

                            // 没有实质性改变
                            if (bChanged == false)
                            {
                                result.Value = 0;
                                return result;
                            }

#if NO
                            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                            if (channel == null)
                            {
                                strError = "get channel error";
                                goto ERROR1;
                            }
#endif

                            // 保存两条读者记录
                            // 写回读者记录
                            byte[] output_timestamp = null;
                            string strOutputPath = "";

                            int nRedoCount = 0;

                            // 应当先保存target读者记录。因为如果此后中断，还原的可能性要大一些

                            // REDO_WRITE_TARGET:
                            lRet = channel.DoSaveTextRes(strTargetOutputReaderRecPath,
                                target_readerdom.OuterXml,
                                false,
                                "content,ignorechecktimestamp",
                                target_reader_timestamp,
                                out output_timestamp,
                                out strOutputPath,
                                out strError);
                            if (lRet == -1)
                            {
                                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                                {
                                    nRedoCount++;
                                    if (nRedoCount > 10)
                                    {
                                        strError = "写回目标读者记录的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                                        goto ERROR1;
                                    }
                                    target_reader_timestamp = output_timestamp;
                                    goto REDO_WRITE_SOURCE;
                                }
                                goto ERROR1;
                            }

                        REDO_WRITE_SOURCE:
                            nRedoCount = 0;
                            lRet = channel.DoSaveTextRes(strSourceOutputReaderRecPath,
                                source_readerdom.OuterXml,
                                false,
                                "content,ignorechecktimestamp",
                                source_reader_timestamp,
                                out output_timestamp,
                                out strOutputPath,
                                out strError);
                            if (lRet == -1)
                            {
                                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                                {
                                    nRedoCount++;
                                    if (nRedoCount > 10)
                                    {
                                        strError = "写回源读者记录的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                                        goto ERROR1;
                                    }
                                    source_reader_timestamp = output_timestamp;
                                    goto REDO_WRITE_SOURCE;
                                }
                                goto ERROR1;
                            }

                            // 记载最终写入的源读者记录到日志
                            XmlNode nodeRecord = DomUtil.SetElementText(domOperLog.DocumentElement,
                                "sourceReaderRecord",
                                source_readerdom.OuterXml);
                            DomUtil.SetAttr(nodeRecord,
                                "recPath",
                                strSourceOutputReaderRecPath);

                            // 记载最终写入的目标读者记录
                            nodeRecord = DomUtil.SetElementText(domOperLog.DocumentElement,
                                "targetReaderRecord",
                                target_readerdom.OuterXml);
                            DomUtil.SetAttr(nodeRecord,
                                "recPath",
                                strTargetOutputReaderRecPath);

                            if (attachment != null)
                            {
                                // 将文件指针复位到头部
                                attachment.Seek(0, SeekOrigin.Begin);
                            }

                            nRet = this.OperLog.WriteOperLog(domOperLog,
                                sessioninfo.ClientAddress,
                                attachment,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "DevolveReaderInfo() API 写入日志时发生错误: " + strError;
                                goto ERROR1;
                            }
                        }
                        finally //  end of 在此范围内，需注意最后删除临时文件
                        {
                            if (attachment != null)
                            {
                                attachment.Close();
                                attachment = null;
                            }
                            File.Delete(strAttachmentFileName);
                        }

                    }// 读者记录锁定2范围结束
                    finally
                    {
                        this.ReaderLocks.UnlockForWrite(strBarcode2);
#if DEBUG_LOCK_READER
                        this.WriteErrorLog("DevolveReaderInfo 结束为读者加写锁2 '" + strBarcode2 + "'");
#endif
                    }
                }// 读者记录锁定1范围结束
                finally
                {
                    this.ReaderLocks.UnlockForWrite(strBarcode1);
#if DEBUG_LOCK_READER
                    this.WriteErrorLog("DevolveReaderInfo 结束为读者加写锁1 '" + strBarcode1 + "'");
#endif
                }

            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 移动借阅信息 -- <borrows>元素内容
        // 此函数也被日志恢复模块所使用，只是恢复时domOperLog为null
        // parameters:
        //      domOperLog      操作日志DOM对象。如果调用时为null，表示根本不创建日志（包括日志附件）
        //      strAttachmentFileName   日志附件文件名。如果有必要创建日志附件，则创建出的文件用这个名字。
        //      attachment              [out]如果创建了日志附件，返回打开的流。
        // return:
        //      -1  error
        //      0   not found brrowinfo
        //      1   found and moved
        int DevolveBorrowInfo(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strSourceReaderBarcode,
            string strTargetReaderBarcode,
            string strOperTimeString,
            ref XmlDocument source_dom,
            ref XmlDocument target_dom,
            ref XmlDocument domOperLog,
            string strAttachmentFileName,
            out Stream attachment,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            attachment = null;

            XmlNode nodeSourceBorrows = source_dom.DocumentElement.SelectSingleNode("borrows");
            if (nodeSourceBorrows == null)
                return 0;

            XmlNodeList nodesSourceBorrow = nodeSourceBorrows.SelectNodes("borrow");
            if (nodesSourceBorrow.Count == 0)
                return 0;

            XmlNode nodeTargetBorrows = target_dom.DocumentElement.SelectSingleNode("borrows");
            if (nodeTargetBorrows == null)
            {
                nodeTargetBorrows = target_dom.CreateElement("borrows");
                target_dom.DocumentElement.AppendChild(nodeTargetBorrows);
            }

            int nAttachmentIndex = 0;
            if (domOperLog != null)
            {
                if (nodesSourceBorrow.Count > 10)
                {
                    // 涉及的实体记录太多，无法直接写入日志记录

                    // 创建附件流
                    attachment = File.Create(strAttachmentFileName);
                }
                else
                {
                    attachment = null;  // 不使用附件
                }
            }

            for (int i = 0; i < nodesSourceBorrow.Count; i++)
            {
                XmlNode source = nodesSourceBorrow[i];

                // 加<borrow>元素
                XmlDocumentFragment fragment = target_dom.CreateDocumentFragment();
                fragment.InnerXml = source.OuterXml;

                XmlNode target = nodeTargetBorrows.AppendChild(fragment);
                // 增加一个注释元素
                DomUtil.SetAttr(target, "devolveComment", "从读者 " + strSourceReaderBarcode + " 转移而来，操作时间 " + strOperTimeString);

                string strEntityBarcode = DomUtil.GetAttr(source, "barcode");

                if (String.IsNullOrEmpty(strEntityBarcode) == true)
                    continue;

                // 同步修改册记录中的借者证条码号
                // return:
                //      -1  error
                //      0   entitybarcode not found
                //      1   found and changed
                nRet = ChangeEntityBorrower(
                    // Channels,
                    channel,
                    strEntityBarcode,
                    strSourceReaderBarcode,
                    strTargetReaderBarcode,
                    strOperTimeString,
                    ref domOperLog,
                    attachment,
                    ref nAttachmentIndex,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    // 暂时不理会
                }
            }

            // 源记录中根下创建一个注释元素
            // 并将原<borrows>元素中的内容移入，用于日后备查
            XmlNode nodeComment = source_dom.CreateElement("devolvedBorrows");
            source_dom.DocumentElement.AppendChild(nodeComment);
            nodeComment.InnerXml = nodeSourceBorrows.InnerXml;

            DomUtil.SetAttr(nodeComment, "comment", "已于 " + strOperTimeString + " 将下列借阅信息转移到读者 " + strTargetReaderBarcode + " 名下");

            // 创建日志记录信息要素
            if (domOperLog != null)
            {
                // 日志记录中的<borrows>元素内存放了来自源读者记录打算移动到目标读者记录的那些<borrow>元素
                DomUtil.SetElementInnerXml(domOperLog.DocumentElement,
                    "borrows",
                    nodeSourceBorrows.InnerXml);
            }

            // 删除源记录中的<borrows/borrow>元素
            nodeSourceBorrows.InnerXml = "";
            return 1;
        }

        // 移动超期违约金信息 -- <overdues>元素内容
        // 此函数也被日志恢复模块所使用，只是恢复时domOperLog为null
        // return:
        //      -1  error
        //      0   not found overdueinfo
        //      1   found and moved
        int DevolveOverdueInfo(
            string strSourceReaderBarcode,
            string strTargetReaderBarcode,
            string strOperTimeString,
            ref XmlDocument source_dom,
            ref XmlDocument target_dom,
            ref XmlDocument domOperLog,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            // 移动超期违约金信息
            XmlNode nodeSourceOverdues = source_dom.DocumentElement.SelectSingleNode("overdues");
            if (nodeSourceOverdues == null)
                return 0;

            XmlNodeList nodesSourceOverdue = nodeSourceOverdues.SelectNodes("overdue");
            if (nodesSourceOverdue.Count == 0)
                return 0;

            XmlNode nodeTargetOverdues = target_dom.DocumentElement.SelectSingleNode("overdues");
            if (nodeTargetOverdues == null)
            {
                nodeTargetOverdues = target_dom.CreateElement("overdues");
                target_dom.DocumentElement.AppendChild(nodeTargetOverdues);
            }

            for (int i = 0; i < nodesSourceOverdue.Count; i++)
            {
                XmlNode source = nodesSourceOverdue[i];

                // 加<overdue>元素
                XmlDocumentFragment fragment = target_dom.CreateDocumentFragment();
                fragment.InnerXml = source.OuterXml;

                XmlNode target = nodeTargetOverdues.AppendChild(fragment);

                // 增加一个注释元素
                DomUtil.SetAttr(target, "devolveComment", "从读者 " + strSourceReaderBarcode + " 转移而来，操作时间 " + strOperTimeString);
            }

            // 源记录中根下创建一个注释元素
            // 并将原<overdues>元素中的内容移入，用于日后备查
            XmlNode nodeComment = source_dom.CreateElement("devolvedOverdues");
            source_dom.DocumentElement.AppendChild(nodeComment);
            nodeComment.InnerXml = nodeSourceOverdues.InnerXml;

            DomUtil.SetAttr(nodeComment, "comment", "已于 "
                + strOperTimeString
                + " 将下列超期信息转移到读者 " + strTargetReaderBarcode + " 名下");

            // 创建日志记录信息要素
            if (domOperLog != null)
            {
                // 日志记录中的<overdues>元素内存放了来自源读者记录打算移动到目标读者记录的那些<overdue>元素
                DomUtil.SetElementInnerXml(domOperLog.DocumentElement,
                    "overdues",
                    nodeSourceOverdues.InnerXml);
            }


            // 删除源记录中的<overdues/overdue>元素
            nodeSourceOverdues.InnerXml = "";
            return 1;
        }

        // 修改册记录中的借者证条码号
        // parameters:
        //      domOperLog  日志记录DOM对象。如果==null，表示根本不创建日志（包括日志DOM）
        //      attachment    如果!=null表示要把实体记录保存到日志的attachment中。如果==null，表示直接把实体记录保存到日志记录(DOM)中
        //      nAttachmentIndex    日志附件记录index。第一次调用的时候，此值应为0，然后如果本函数增添了日志附件记录，会自动增量这个值
        // return:
        //      -1  error
        //      0   entitybarcode not found
        //      1   found and changed
        int ChangeEntityBorrower(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strEntityBarcode,
            string strOldReaderBarcode,
            string strNewReaderBarcode,
            string strOperTimeString,
            ref XmlDocument domOperLog,
            Stream attachment,
            ref int nAttachmentIndex,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            // 加册记录锁
            this.EntityLocks.LockForWrite(strEntityBarcode);

            try // 册记录锁定范围开始
            {
                // 从册条码号获得册记录
                byte[] item_timestamp = null;
                List<string> aPath = null;
                string strItemXml = "";
                string strOutputItemRecPath = "";

                int nRedoCount = 0;
            REDO:

                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetItemRecXml(
                    // Channels,
                    channel,
                    strEntityBarcode,
                    out strItemXml,
                    100,
                    out aPath,
                    out item_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "册条码号 '" + strEntityBarcode + "' 不存在";
                    return 0;
                }
                if (nRet == -1)
                {
                    strError = "读入册记录 '" + strEntityBarcode + "' 时发生错误: " + strError;
                    goto ERROR1;
                }

                // RmsChannel channel = null;

                if (aPath.Count > 1)
                {
                    /*
                    strError = "册条码号为 '" + strEntityBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，无法进行修改";
                    return -1;
                     * */

                    // bItemBarcodeDup = true; // 此时已经需要设置状态。虽然后面可以进一步识别出真正的册记录

                    // 构造strDupBarcodeList
                    /*
                    string[] pathlist = new string[aPath.Count];
                    aPath.CopyTo(pathlist);
                    string strDupBarcodeList = String.Join(",", pathlist);
                     * */
                    string strDupBarcodeList = StringUtil.MakePathList(aPath);

                    List<string> aFoundPath = null;
                    List<byte[]> aTimestamp = null;
                    List<string> aItemXml = null;

                    if (String.IsNullOrEmpty(strOldReaderBarcode) == true)
                    {
                        strError = "strOldReaderBarcode参数值不能为空";
                        goto ERROR1;
                    }

#if NO
                    channel = Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        goto ERROR1;
                    }
#endif

                    // 从若干重复条码号的册记录中，选出其中符合当前读者证条码号的
                    // return:
                    //      -1  出错
                    //      其他    选出的数量
                    nRet = FindItem(
                        channel,
                        strOldReaderBarcode,
                        aPath,
                        true,   // 优化
                        out aFoundPath,
                        out aItemXml,
                        out aTimestamp,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "选择重复条码号的册记录时发生错误: " + strError;
                        goto ERROR1;
                    }

                    if (nRet == 0)
                    {
                        strError = "册条码号 '" + strEntityBarcode + "' 检索出的 " + aPath.Count + " 条记录中，没有任何一条其<borrower>元素表明了被读者 '" + strOldReaderBarcode + "' 借阅。";
                        goto ERROR1;
                    }

                    if (nRet > 1)
                    {
                        strError = "册条码号为 '" + strEntityBarcode + "' 并且<borrower>元素表明为读者 '" + strOldReaderBarcode + "' 借阅的册记录有 " + aFoundPath.Count.ToString() + " 条，无法进行移动操作。";
                        goto ERROR1;
                    }

                    Debug.Assert(nRet == 1, "");

                    strOutputItemRecPath = aFoundPath[0];
                    item_timestamp = aTimestamp[0];
                    strItemXml = aItemXml[0];
                }
                else
                {
                    Debug.Assert(nRet == 1, "");
                    Debug.Assert(aPath.Count == 1, "");

                    if (nRet == 1)
                    {
                        strOutputItemRecPath = aPath[0];
                    }
                }

                XmlDocument itemdom = null;
                nRet = LibraryApplication.LoadToDom(strItemXml,
                    out itemdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载册记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                    "borrower");

                if (String.IsNullOrEmpty(strBorrower) == true)
                {
                    strError = "实体记录中没有借者信息(<borrower>元素内容)";
                    goto ERROR1;
                }

                // 核对旧读者证条码号
                if (strBorrower != strOldReaderBarcode)
                {
                    strError = "实体记录中，已有借者证条码号 '" + strBorrower + "' 和期望的改前证条码号 '" + strOldReaderBarcode + "' 不一致...";
                    goto ERROR1;
                }

                // 修改为新读者证条码号
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "borrower",
                    strNewReaderBarcode);

                // 加上一个注释
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "devolveComment",
                    "本册原为读者 " + strOldReaderBarcode + " 所借阅，后于 "
                    + strOperTimeString + " 被转移到读者 " + strNewReaderBarcode + " 名下");

#if NO
                if (channel == null)
                {
                    channel = Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        goto ERROR1;
                    }
                }
#endif

                // 保存实体记录
                byte[] output_timestamp = null;
                string strOutputPath = "";

                lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                    itemdom.OuterXml,
                    false,
                    "content",  // ,ignorechecktimestamp
                    item_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        if (nRedoCount > 10)
                            goto ERROR1;
                        nRedoCount++;
                        item_timestamp = output_timestamp;
                        goto REDO;
                    }
                }

                // 将保存了的记录写入日志
                if (domOperLog != null)
                {
                    XmlNode nodeLogRecord = domOperLog.CreateElement("changedEntityRecord");
                    domOperLog.DocumentElement.AppendChild(nodeLogRecord);
                    DomUtil.SetAttr(nodeLogRecord, "recPath", strOutputPath);

                    if (attachment == null)
                    {
                        // 实体记录完全保存到日志记录中
                        nodeLogRecord.InnerText = itemdom.OuterXml;
                    }
                    else
                    {
                        // 实体记录保存到附件中，只在日志记录中留下序号

                        // 保存附件序号
                        DomUtil.SetAttr(nodeLogRecord, "attachmentIndex", nAttachmentIndex.ToString());

                        byte[] content = Encoding.UTF8.GetBytes(itemdom.OuterXml);
                        byte[] length = BitConverter.GetBytes((long)content.LongLength);
                        attachment.Write(length, 0, length.Length);
                        attachment.Write(content, 0, content.Length);

                        nAttachmentIndex++;
                    }
                }
            }
            finally
            {
                this.EntityLocks.UnlockForWrite(strEntityBarcode);
            }
            return 1;
        ERROR1:
            return -1;
        }

        // 检查一个读者记录的借还信息是否异常。
        // parameters:
        //      nStart      从第几个借阅的册事项开始处理
        //      nCount      共处理几个借阅的册事项
        //      nProcessedBorrowItems   [out]本次处理了多少个借阅册事项
        //      nTotalBorrowItems   [out]当前读者一共包含有多少个借阅册事项
        // result.Value
        //      -1  错误。
        //      0   检查无错。
        //      1   检查发现有错。
        public LibraryServerResult CheckReaderBorrowInfo(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strReaderBarcode,
            int nStart,
            int nCount,
            out int nProcessedBorrowItems,
            out int nTotalBorrowItems)
        {
            string strError = "";
            int nRet = 0;
            nTotalBorrowItems = 0;
            nProcessedBorrowItems = 0;

            string strCheckError = "";

            LibraryServerResult result = new LibraryServerResult();
            int nErrorCount = 0;

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("CheckReaderBorrowInfo 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForWrite(strReaderBarcode);

            try // 读者记录锁定范围开始
            {
                // 读入读者记录
                string strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;
                nRet = this.GetReaderRecXml(
                    // Channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    result.Value = -1;
                    result.ErrorInfo = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                    return result;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                XmlNodeList nodesBorrow = readerdom.DocumentElement.SelectNodes("borrows/borrow");

                nTotalBorrowItems = nodesBorrow.Count;

                if (nTotalBorrowItems == 0)
                {
                    result.Value = 0;
                    result.ErrorInfo = "读者记录中没有借还信息。";
                    return result;
                }

                if (nStart >= nTotalBorrowItems)
                {
                    strError = "nStart参数值" + nStart.ToString() + "大于当前读者记录中的借阅册个数" + nTotalBorrowItems.ToString();
                    goto ERROR1;
                }

                nProcessedBorrowItems = 0;
                for (int i = nStart; i < nTotalBorrowItems; i++)
                {
                    if (nCount != -1 && nProcessedBorrowItems >= nCount)
                        break;

                    // 一个API最多做10条
                    if (nProcessedBorrowItems >= 10)
                        break;

                    XmlNode node = nodesBorrow[i];

                    string strItemBarcode = DomUtil.GetAttr(node, "barcode");

                    nProcessedBorrowItems++;

                    string strOutputReaderBarcode_0 = "";

                    string[] aDupPath = null;
                    // 检查一个实体记录的借还信息是否异常。
                    LibraryServerResult result_1 = CheckItemBorrowInfo(
                        // Channels,
                        channel,
                        strReaderBarcode,
                        readerdom,
                        strOutputReaderRecPath,
                        strItemBarcode,
                        null,
                        out strOutputReaderBarcode_0,
                        out aDupPath);
                    if (result_1.Value == -1 || result_1.Value == 1)
                    {
                        if (result_1.ErrorCode == ErrorCode.ItemBarcodeDup)
                        {
                            List<string> linkedPath = new List<string>();

                            for (int j = 0; j < aDupPath.Length; j++)
                            {
                                string[] aDupPathTemp = null;
                                string strOutputReaderBarcode = "";
                                LibraryServerResult result_2 = CheckItemBorrowInfo(
                                    // Channels,
                                    channel,
                                    strReaderBarcode,
                                    readerdom,
                                    strOutputReaderRecPath,
                                    strItemBarcode,
                                    aDupPath[j],
                                    out strOutputReaderBarcode,
                                    out aDupPathTemp);
                                if (result_2.Value == -1)
                                {
                                    strError = result_2.ErrorInfo;
                                    goto ERROR1;
                                }



                                if (strOutputReaderBarcode == strReaderBarcode)
                                {
                                    linkedPath.Add(aDupPath[j]);

                                    if (result_2.Value == 1)
                                    {
                                        strCheckError += "检查读者记录中借阅册条码号 " + strItemBarcode + " 关联的册记录(记录路径 " + aDupPath[j] + ") 时发现错误: " + result_1.ErrorInfo + "。";
                                        nErrorCount++;
                                    }
                                }

                            } // end of for

                            if (linkedPath.Count == 0)
                            {
                                strCheckError += "读者记录中借阅册条码号 " + strItemBarcode + " 关联了 " + aDupPath.Length + " 条册记录，可是这些册记录中没有任何一条中有关联回读者证条码号的借阅信息。";
                                nErrorCount++;
                            }

                            if (linkedPath.Count > 1)
                            {
                                strCheckError += "读者记录中借阅册条码号 " + strItemBarcode + " 关联了 " + aDupPath.Length + " 条册记录，这些册记录中有 " + linkedPath.Count.ToString() + "条中有关联回读者证条码号的借阅信息。";
                                nErrorCount++;
                            }

                            continue;
                        }

                        if (result_1.ErrorCode == ErrorCode.ReaderBarcodeNotFound)
                        {
                            strCheckError += "读者记录中借阅册条码号 " + strItemBarcode + " 关联的册记录中，其<borrower>字段关联回的读者证条码号是 " + strOutputReaderBarcode_0 + "，而不是出发的读者证条码号 " + strReaderBarcode + "。并且证条码号为 " + strOutputReaderBarcode_0 + " 的读者记录不存在。";
                            nErrorCount++;
                            continue;
                        }

                        if (result_1.Value == -1)
                        {
                            strCheckError += "检查读者记录中借阅册条码号 " + strItemBarcode + " 关联的册记录时发生错误: " + result_1.ErrorInfo + "。";
                            nErrorCount++;
                        }
                        if (result_1.Value == 1)
                        {
                            strCheckError += "检查读者记录中借阅册条码号 " + strItemBarcode + " 关联的册记录时发现错误: " + result_1.ErrorInfo + "。";
                            nErrorCount++;
                        }
                        continue;
                    } // end of return -1


                    if (strOutputReaderBarcode_0 != strReaderBarcode)
                    {
                        strCheckError += "读者记录中借阅册条码号 " + strItemBarcode + " 关联的册记录中，其<borrower>字段关联回的读者证条码号是 " + strOutputReaderBarcode_0 + "，而不是出发的读者证条码号 " + strReaderBarcode + "。";
                        nErrorCount++;
                    }
                }
            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("CheckReaderBorrowInfo 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif
            }

            if (String.IsNullOrEmpty(strCheckError) == false)
            {
                result.Value = 1;
                result.ErrorInfo = strCheckError;
            }
            else
                result.Value = 0;

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 检查一个实体记录的借还信息是否异常。
        // parameters:
        //      strLockedReaderBarcode  外层已经加锁过的条码号。本函数根据这个信息，可以避免重复加锁。
        //      exist_readerdom 已经装载入DOM的读者记录。其读者证条码号是strLockedReaderBarcode。如果提供了这个值，本函数会优化性能。
        // result.Value
        //      -1  出错。
        //      0   实体记录中没有借阅信息，或者检查发现无错。
        //      1   检查发现有错。
        public LibraryServerResult CheckItemBorrowInfo(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strLockedReaderBarcode,
            XmlDocument exist_readerdom,
            string strExistReaderRecPath,
            string strItemBarcode,
            string strConfirmItemRecPath,
            out string strOutputReaderBarcode,
            out string[] aDupPath)
        {
            string strError = "";
            aDupPath = null;
            strOutputReaderBarcode = "";
            long lRet = 0;
            int nRet = 0;

            string strCheckError = "";

            LibraryServerResult result = new LibraryServerResult();

            if (exist_readerdom != null)
            {
                if (String.IsNullOrEmpty(strExistReaderRecPath) == true)
                {
                    strError = "如果exist_readerdom参数不为空，则strExistReaderRecPath也不应为空。";
                    goto ERROR1;
                }
            }

            string strOutputItemRecPath = "";
            byte[] item_timestamp = null;

#if NO
            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }
#endif

            string strItemXml = "";

            // 如果已经有确定的册记录路径
            if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
            {
                // 检查路径中的库名，是不是实体库名
                // return:
                //      -1  error
                //      0   不是实体库名
                //      1   是实体库名
                nRet = this.CheckItemRecPath(strConfirmItemRecPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = strConfirmItemRecPath + strError;
                    goto ERROR1;
                }

                string strMetaData = "";

                lRet = channel.GetRes(strConfirmItemRecPath,
                    out strItemXml,
                    out strMetaData,
                    out item_timestamp,
                    out strOutputItemRecPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得册记录失败: " + strError;
                    goto ERROR1;
                }
            }
            else
            {
                // 从册条码号获得册记录
                List<string> aPath = null;

                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetItemRecXml(
                    // Channels,
                    channel,
                    strItemBarcode,
                    out strItemXml,
                    100,
                    out aPath,
                    out item_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    result.Value = -1;
                    result.ErrorInfo = "册条码号 '" + strItemBarcode + "' 不存在";
                    result.ErrorCode = ErrorCode.ItemBarcodeNotFound;
                    return result;
                }
                if (nRet == -1)
                {
                    strError = "读入册记录时发生错误: " + strError;
                    goto ERROR1;
                }

                if (aPath.Count > 1)
                {
                    /*
                    // 构造strDupBarcodeList
                    string[] pathlist = new string[aPath.Count];
                    aPath.CopyTo(pathlist);
                    strDupBarcodeList = String.Join(",", pathlist);
                     * */

                    result.Value = -1;
                    result.ErrorInfo = "册条码号为 '" + strItemBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，无法进行修复操作。请在附加册记录路径后重新提交修复操作。";
                    result.ErrorCode = ErrorCode.ItemBarcodeDup;

                    aDupPath = new string[aPath.Count];
                    aPath.CopyTo(aDupPath);
                    return result;
                }
                else
                {
                    Debug.Assert(nRet == 1, "");
                    Debug.Assert(aPath.Count == 1, "");
                    if (nRet == 1)
                    {
                        strOutputItemRecPath = aPath[0];
                        // strItemXml已经有册记录了
                    }
                }

                // 函数返回后有用
                aDupPath = new string[1];
                aDupPath[0] = strOutputItemRecPath;
            }

            XmlDocument itemdom = null;
            nRet = LibraryApplication.LoadToDom(strItemXml,
                out itemdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载册记录进入XML DOM时发生错误: " + strError;
                goto ERROR1;
            }

            strOutputReaderBarcode = ""; // 借阅者证条码号

            strOutputReaderBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
    "borrower");
            if (String.IsNullOrEmpty(strOutputReaderBarcode) == true)
            {
                strError = "册记录中<borrower>元素值表明该册当前并未被任何读者借阅";
                result.Value = 0;   // 2008/1/25 comment 此时无法断定是否为错误。还需要strOutputReaderBarcode返回后进行比较才能确定
                result.ErrorInfo = strError;
                return result;
            }

            // 读出读者记录，看看是否有borrows/borrow元素表明有这个册条码号
            // 加读者记录锁
            if (strLockedReaderBarcode != strOutputReaderBarcode)
            {
#if DEBUG_LOCK_READER
                this.WriteErrorLog("CheckItemBorrowInfo 开始为读者加写锁 '" + strOutputReaderBarcode + "'");
#endif
                this.ReaderLocks.LockForWrite(strOutputReaderBarcode);
            }

            try // 读者记录锁定范围开始
            {
                // 读入读者记录
                string strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;

                XmlDocument readerdom = null;

                if (exist_readerdom == null)
                {
                    nRet = this.GetReaderRecXml(
                        // Channels,
                        channel,
                        strOutputReaderBarcode,
                        out strReaderXml,
                        out strOutputReaderRecPath,
                        out reader_timestamp,
                        out strError);
                    if (nRet == 0)
                    {
                        result.Value = 1;
                        result.ErrorInfo = "读者证条码号 '" + strOutputReaderBarcode + "' 不存在";
                        result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                        return result;
                    }
                    if (nRet == -1)
                    {
                        strError = "读入读者记录时发生错误: " + strError;
                        goto ERROR1;
                    }

                    nRet = LibraryApplication.LoadToDom(strReaderXml,
                        out readerdom,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                        goto ERROR1;
                    }
                }
                else
                {
                    readerdom = exist_readerdom;
                    strOutputReaderRecPath = strExistReaderRecPath;
                }

                XmlNodeList nodesBorrow = readerdom.DocumentElement.SelectNodes("borrows/borrow[@barcode='" + strItemBarcode + "']");
                if (nodesBorrow.Count == 0)
                {
                    strCheckError += "虽然册记录 " + strOutputItemRecPath + " 中表明了被读者 '" + strOutputReaderBarcode + "' 借阅，但是读者记录 " + strOutputReaderRecPath + " 中并没有关于册条码号 '" + strItemBarcode + "' 的借阅记录。";
                    goto END1;
                }
                if (nodesBorrow.Count > 1)
                {
                    strCheckError = "读者记录有 " + strOutputReaderRecPath + " 中关于册条码号 '" + strItemBarcode + "' 的 " + nodesBorrow.Count.ToString() + " 条含糊借阅记录。";
                    goto END1;
                }

                Debug.Assert(nodesBorrow.Count == 1, "");
            }
            finally
            {
                if (strLockedReaderBarcode != strOutputReaderBarcode)
                {
                    this.ReaderLocks.UnlockForWrite(strOutputReaderBarcode);
#if DEBUG_LOCK_READER
                    this.WriteErrorLog("CheckItemBorrowInfo 结束为读者加写锁 '" + strOutputReaderBarcode + "'");
#endif
                }
            }

        END1:
            if (String.IsNullOrEmpty(strCheckError) == false)
            {
                result.Value = 1;
                result.ErrorInfo = strCheckError;
            }
            else
                result.Value = 0;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 修复读者记录一侧的借阅信息链条错误
        // 具体来说，就是读者这边有借阅信息，但是指向的实体不存在，或者虽然存在但是
        // 其中没有指回的链。
        public LibraryServerResult RepairReaderSideError(
            SessionInfo sessioninfo,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            out string[] aDupPath)
        {
            string strError = "";
            aDupPath = null;
            int nRet = 0;
            long lRet = 0;

            LibraryServerResult result = new LibraryServerResult();

            int nRedoCount = 0; // 因为时间戳冲突, 重试的次数

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                strError = "读者证条码号不能为空。";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "册条码号不能为空。";
                goto ERROR1;
            }
        REDO_REPAIR:

            /*
                    string strOutputReaderXml = "";
                    string strOutputItemXml = "";
             * */
            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("RepairReaderSideError 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForWrite(strReaderBarcode);

            try // 读者记录锁定范围开始
            {
                // 读入读者记录
                string strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;
                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                string strLibraryCode = "";
                // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
                    sessioninfo.LibraryCodeList,
                    out strLibraryCode) == false)
                {
                    strError = "读者记录路径 '" + strOutputReaderRecPath + "' 的读者库不在当前用户管辖范围内";
                    goto ERROR1;
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 校验读者证条码号参数是否和XML记录中完全一致
#if NO
                string strTempBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                    "barcode");
                if (strReaderBarcode != strTempBarcode)
                {
                    strError = "修复操作被拒绝。因读者证条码号参数 '" + strReaderBarcode + "' 和读者记录中<barcode>元素内的读者证条码号值 '" + strTempBarcode + "' 不一致。";
                    goto ERROR1;
                }
#endif
                {
                    // return:
                    //      false   不匹配
                    //      true    匹配
                    bool bRet = CheckBarcode(readerdom,
            strReaderBarcode,
            "读者",
            out strError);
                    if (bRet == false)
                    {
                        strError = "修复操作被拒绝。因" + strError + "。";
                        goto ERROR1;
                    }
                }

                XmlNode nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']");
                if (nodeBorrow == null)
                {
                    strError = "修复操作被拒绝。读者记录 " + strReaderBarcode + " 中并不存在有关册 " + strItemBarcode + " 的借阅信息。";
                    goto ERROR1;
                }

                byte[] item_timestamp = null;
                List<string> aPath = null;
                string strItemXml = "";
                string strOutputItemRecPath = "";

#if NO
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
#endif

                // 加册记录锁
                this.EntityLocks.LockForWrite(strItemBarcode);

                try // 册记录锁定范围开始
                {
                    // 读入册记录

                    // 如果已经有确定的册记录路径
                    if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                    {
                        // 检查路径中的库名，是不是实体库名
                        // return:
                        //      -1  error
                        //      0   不是实体库名
                        //      1   是实体库名
                        nRet = this.CheckItemRecPath(strConfirmItemRecPath, out strError);
                        if (nRet != 1)
                            goto ERROR1;

                        string strMetaData = "";

                        lRet = channel.GetRes(strConfirmItemRecPath,
                            out strItemXml,
                            out strMetaData,
                            out item_timestamp,
                            out strOutputItemRecPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得册记录失败: " + strError;
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        // 从册条码号获得册记录

                        // 获得册记录
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        nRet = this.GetItemRecXml(
                            // sessioninfo.Channels,
                            channel,
                            strItemBarcode,
                            out strItemXml,
                            100,
                            out aPath,
                            out item_timestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            /*
                            result.Value = -1;
                            result.ErrorInfo = "册条码号 '" + strItemBarcode + "' 不存在";
                            result.ErrorCode = ErrorCode.ItemBarcodeNotFound;
                            return result;
                             * */
                            // 册条码号不存在也是需要修复的情况之一。
                            // bItemRecordNotFound = true;
                            goto DELETE_CHAIN;
                        }
                        if (nRet == -1)
                        {
                            strError = "读入册记录时发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (aPath.Count > 1)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "册条码号为 '" + strItemBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，无法进行修复操作。请在附加册记录路径后重新提交修复操作。";
                            result.ErrorCode = ErrorCode.ItemBarcodeDup;

                            aDupPath = new string[aPath.Count];
                            aPath.CopyTo(aDupPath);
                            return result;
                        }
                        else
                        {
                            Debug.Assert(nRet == 1, "");
                            Debug.Assert(aPath.Count == 1, "");

                            if (nRet == 1)
                            {
                                strOutputItemRecPath = aPath[0];
                            }
                        }
                    }

                    XmlDocument itemdom = null;
                    nRet = LibraryApplication.LoadToDom(strItemXml,
                        out itemdom,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "装载册记录进入XML DOM时发生错误: " + strError;
                        goto ERROR1;
                    }

#if NO
                    // TODO: 要实现 strItemBarcode 为 @refID:xxxxx 的情况。因为现在允许册记录没有册条码号了
                    // 校验册条码号参数是否和XML记录中完全一致
                    string strTempItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                        "barcode");
                    if (strItemBarcode != strTempItemBarcode)
                    {
                        strError = "修复操作被拒绝。因册条码号参数 '" + strItemBarcode + "' 和册记录中<barcode>元素内的册条码号值 '" + strTempItemBarcode + "' 不一致。";
                        goto ERROR1;
                    }
#endif
                    {
                        // return:
                        //      false   不匹配
                        //      true    匹配
                        bool bRet = CheckBarcode(itemdom,
                strItemBarcode,
                "册",
                out strError);
                        if (bRet == false)
                        {
                            strError = "修复操作被拒绝。因" + strError + "。";
                            goto ERROR1;
                        }
                    }

                    // 看看册记录中是否有指回读者记录的链
                    string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                        "borrower");
                    if (strBorrower == strReaderBarcode)
                    {
                        strError = "修复操作被拒绝。您所请求要修复的链，本是一条完整正确的链。可直接进行普通还书操作。";
                        goto CORRECT;
                    }

                DELETE_CHAIN:

                    // 移除读者记录侧的链
                    nodeBorrow.ParentNode.RemoveChild(nodeBorrow);

                    byte[] output_timestamp = null;
                    string strOutputPath = "";

                    // 写回读者记录
                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        readerdom.OuterXml,
                        false,
                        "content",  // ,ignorechecktimestamp
                        reader_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            nRedoCount++;
                            if (nRedoCount > 10)
                            {
                                strError = "写回读者记录的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                                goto ERROR1;
                            }
                            goto REDO_REPAIR;
                        }
                        goto ERROR1;
                    }

                    // 及时更新时间戳
                    reader_timestamp = output_timestamp;

                    /*
<root>
  <operation>repairBorrowInfo</operation> 
  <action>...</action> 具体动作 有 repairreaderside repairitemside
  <readerBarcode>...</readerBarcode>
  <itemBarcode>...</itemBarcode>
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
</root>
                     * * */

                    XmlDocument domOperLog = new XmlDocument();
                    domOperLog.LoadXml("<root />");
                    DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "repairBorrowInfo");
                    DomUtil.SetElementText(domOperLog.DocumentElement, "action", "repairreaderside");
                    DomUtil.SetElementText(domOperLog.DocumentElement, "readerBarcode", strReaderBarcode);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "itemBarcode", strItemBarcode);
                    if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                        DomUtil.SetElementText(domOperLog.DocumentElement, "confirmItemRecPath", strConfirmItemRecPath);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
    sessioninfo.UserID);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        this.Clock.GetClock());

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "RepairReaderSideError() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }

                    // 写入统计指标
                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(
                        strLibraryCode,
                        "修复借阅信息",
                        "读者侧次数",
                        1);

                } // 册记录锁定范围结束
                finally
                {
                    // 解册记录锁
                    this.EntityLocks.UnlockForWrite(strItemBarcode);
                }

            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("RepairReaderSideError 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        CORRECT:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.NoError;   // 表示链条本来就没有错误
            return result;
        }

        // 修复册记录一侧的借阅信息链条错误
        // 具体来说，就是册这边有借阅信息，但是指向的读者记录不存在，或者虽然存在但是
        // 其中没有指回的链。
        public LibraryServerResult RepairItemSideError(
            SessionInfo sessioninfo,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            out string[] aDupPath)
        {
            string strError = "";
            aDupPath = null;
            int nRet = 0;
            long lRet = 0;

            LibraryServerResult result = new LibraryServerResult();

            int nRedoCount = 0; // 因为时间戳冲突, 重试的次数

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                strError = "读者证条码号不能为空。";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "册条码号不能为空。";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }
        REDO_REPAIR:

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("RepairItemSideError 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif

            this.ReaderLocks.LockForWrite(strReaderBarcode);

            try // 读者记录锁定范围开始
            {
                // 读入读者记录
                string strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                if (nRet == 0)
                {
                    // 对应的读者记录不存在。就无法检查读者所在的馆代码了
                }
                else if (nRet > 1)
                {
                    strError = "证条码号为 '" + strReaderBarcode + "' 的读者记录存在 " + nRet + " 条。请先修复此问题，再重试修复册记录的链问题";
                    goto ERROR1;
                }
                else
                {
                    Debug.Assert(string.IsNullOrEmpty(strOutputReaderRecPath) == false, "");

                    string strLibraryCode = "";

                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
                        sessioninfo.LibraryCodeList,
                        out strLibraryCode) == false)
                    {
                        strError = "读者记录路径 '" + strOutputReaderRecPath + "' 的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }
                }

                XmlDocument readerdom = null;
                if (nRet == 0)
                {
                    /*
                    strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    goto ERROR1;
                     * */
                }
                else
                {
                    nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                    if (nRet == -1)
                    {
                        strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                        goto ERROR1;
                    }

                    // 校验读者证条码号参数是否和XML记录中完全一致
#if NO
                    string strTempBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                        "barcode");
                    if (strReaderBarcode != strTempBarcode)
                    {
                        strError = "修复操作被拒绝。因读者证条码号参数 '" + strReaderBarcode + "' 和读者记录中<barcode>元素内的读者证条码号值 '" + strTempBarcode + "' 不一致。";
                        goto ERROR1;
                    }
#endif
                    // return:
                    //      false   不匹配
                    //      true    匹配
                    bool bRet = CheckBarcode(readerdom,
            strReaderBarcode,
            "读者",
            out strError);
                    if (bRet == false)
                    {
                        strError = "修复操作被拒绝。因" + strError + "。";
                        goto ERROR1;
                    }
                }

                byte[] item_timestamp = null;
                List<string> aPath = null;
                string strItemXml = "";
                string strOutputItemRecPath = "";

#if NO
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
#endif

                // 加册记录锁
                this.EntityLocks.LockForWrite(strItemBarcode);

                try // 册记录锁定范围开始
                {
                    // 读入册记录

                    // 如果已经有确定的册记录路径
                    if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                    {
                        // 检查路径中的库名，是不是实体库名
                        // return:
                        //      -1  error
                        //      0   不是实体库名
                        //      1   是实体库名
                        nRet = this.CheckItemRecPath(strConfirmItemRecPath, out strError);
                        if (nRet != 1)
                            goto ERROR1;

                        string strMetaData = "";

                        lRet = channel.GetRes(strConfirmItemRecPath,
                            out strItemXml,
                            out strMetaData,
                            out item_timestamp,
                            out strOutputItemRecPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "根据strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得册记录失败: " + strError;
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        // 从册条码号获得册记录

                        // 获得册记录
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        nRet = this.GetItemRecXml(
                            // sessioninfo.Channels,
                            channel,
                            strItemBarcode,
                            out strItemXml,
                            100,
                            out aPath,
                            out item_timestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "册条码号 '" + strItemBarcode + "' 不存在";
                            result.ErrorCode = ErrorCode.ItemBarcodeNotFound;
                            return result;
                        }
                        if (nRet == -1)
                        {
                            strError = "读入册记录时发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (aPath.Count > 1)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "册条码号为 '" + strItemBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，无法进行修复操作。请在附加册记录路径后重新提交修复操作。";
                            result.ErrorCode = ErrorCode.ItemBarcodeDup;

                            aDupPath = new string[aPath.Count];
                            aPath.CopyTo(aDupPath);
                            return result;
                        }
                        else
                        {
                            Debug.Assert(nRet == 1, "");
                            Debug.Assert(aPath.Count == 1, "");

                            if (nRet == 1)
                            {
                                strOutputItemRecPath = aPath[0];
                            }
                        }
                    }

                    XmlDocument itemdom = null;
                    nRet = LibraryApplication.LoadToDom(strItemXml,
                        out itemdom,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "装载册记录进入XML DOM时发生错误: " + strError;
                        goto ERROR1;
                    }

                    string strLibraryCode = "";
                    // 检查一个册记录的馆藏地点是否符合馆代码列表要求
                    // return:
                    //      -1  检查过程出错
                    //      0   符合要求
                    //      1   不符合要求
                    nRet = CheckItemLibraryCode(itemdom,
                        sessioninfo,
                        out strLibraryCode,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "检查分馆代码时出错: " + strError;
                        goto ERROR1;
                    }
                    if (nRet == 1)
                    {
                        strError = "册记录 '" + strOutputItemRecPath + "' 不在当前用户管辖范围内";
                        goto ERROR1;
                    }

#if NO
                    // TODO: 要考虑 @refID:xxxx 形态。可以编写一个检查函数
                    // 校验册条码号参数是否和XML记录中完全一致
                    string strTempItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                        "barcode");
                    if (strItemBarcode != strTempItemBarcode)
                    {
                        strError = "修复操作被拒绝。因册条码号参数 '" + strItemBarcode + "' 和册记录中<barcode>元素内的册条码号值 '" + strTempItemBarcode + "' 不一致。";
                        goto ERROR1;
                    }
#endif
                    // return:
                    //      false   不匹配
                    //      true    匹配
                    bool bRet = CheckBarcode(itemdom,
            strItemBarcode,
            "册",
            out strError);
                    if (bRet == false)
                    {
                        strError = "修复操作被拒绝。因" + strError + "。";
                        goto ERROR1;
                    }

                    // 看看册记录中是否有指向读者记录的链
                    string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                        "borrower");
                    if (String.IsNullOrEmpty(strBorrower) == true)
                    {
                        strError = "修复操作被拒绝。您所请求要修复的册记录中，本来就没有借阅信息，因此谈不上修复。";
                        goto CORRECT;
                    }

                    if (strBorrower != strReaderBarcode)
                    {
                        strError = "修复操作被拒绝。您所请求要修复的册记录中，并没有指明借阅者是读者 " + strReaderBarcode + "。";
                        goto ERROR1;
                    }

                    // 看看读者记录中是否有指回链条。
                    if (readerdom != null)
                    {
                        XmlNode nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcode + "']");
                        if (nodeBorrow != null)
                        {
                            strError = "修复操作被拒绝。您所请求要修复的链，本是一条完整正确的链。可直接进行普通还书操作。";
                            goto ERROR1;
                        }
                    }

                    // DELETE_CHAIN:

                    // 移除册记录侧的链
                    DomUtil.DeleteElement(itemdom.DocumentElement,
                        "borrower");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
                        "borrowDate");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
                        "borrowPeriod");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
                        "borrowerReaderType");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
                        "borrowerRecPath");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
                        "returningDate");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
                        "operator");
                    DomUtil.RemoveEmptyElements(itemdom.DocumentElement);

                    byte[] output_timestamp = null;
                    string strOutputPath = "";

                    // 写回册记录
                    lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                        itemdom.OuterXml,
                        false,
                        "content",  // ,ignorechecktimestamp
                        item_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            nRedoCount++;
                            if (nRedoCount > 10)
                            {
                                strError = "写回册记录的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                                goto ERROR1;
                            }
                            goto REDO_REPAIR;
                        }
                        goto ERROR1;
                    } // end of 写回册记录失败

                    XmlDocument domOperLog = new XmlDocument();
                    domOperLog.LoadXml("<root />");
                    DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 册所在的馆代码
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "repairBorrowInfo");
                    DomUtil.SetElementText(domOperLog.DocumentElement, "action", "repairitemside");
                    DomUtil.SetElementText(domOperLog.DocumentElement, "readerBarcode", strReaderBarcode);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "itemBarcode", strItemBarcode);
                    if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                        DomUtil.SetElementText(domOperLog.DocumentElement, "confirmItemRecPath", strConfirmItemRecPath);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        sessioninfo.UserID);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        this.Clock.GetClock());

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "RepairItemSideError() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }

                    // 写入统计指标
                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(
                        strLibraryCode,
                        "修复借阅信息",
                        "实体侧次数",
                        1);

                } // 册记录锁定范围结束
                finally
                {
                    // 解册记录锁
                    this.EntityLocks.UnlockForWrite(strItemBarcode);
                }
            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("RepairItemSideError 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        CORRECT:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.NoError;   // 表示链条本来就没有错误
            return result;
        }

        // 检查条码号参数和记录中的字段是否匹配
        // parameters:
        //      strRecordTypeCaption    记录类型。 册/读者
        // return:
        //      false   不匹配
        //      true    匹配
        static bool CheckBarcode(XmlDocument itemdom,
            string strItemBarcode,
            string strRecordTypeCaption,
            out string strError)
        {
            strError = "";

            string strRefID = "";
            string strHead = "@refID:";
            if (StringUtil.HasHead(strItemBarcode, strHead, true) == true)
            {
                // strFrom = "参考ID";
                strRefID = strItemBarcode.Substring(strHead.Length);

                string strTempRefID = DomUtil.GetElementText(itemdom.DocumentElement,
"refID");
                if (strRefID != strTempRefID)
                {
                    strError = "参考ID参数 '" + strRefID + "' 和" + strRecordTypeCaption + "记录中<refID>元素(参考ID)值 '" + strTempRefID + "' 不一致";
                    return false;
                }
            }
            else
            {
                string strTempItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
"barcode");
                if (strItemBarcode != strTempItemBarcode)
                {
                    strError = strRecordTypeCaption + "条码号参数 '" + strItemBarcode + "' 和" + strRecordTypeCaption + "记录中<barcode>元素内的条码号值 '" + strTempItemBarcode + "' 不一致";
                    return false;
                }
            }

            return true;
        }

        // 入馆登记
        // result.Value -1 出错 其他 特定门(strGateName)的本次的累计量
        public LibraryServerResult PassGate(
            SessionInfo sessioninfo,
            string strReaderBarcode,
            string strGateName,
            string strResultTypeList,
            out string[] results)
        {
            string strError = "";
            int nRet = 0;
            results = null;

            int nResultValue = 0;

            LibraryServerResult result = new LibraryServerResult();

            if (string.IsNullOrEmpty(strReaderBarcode) == false)
            {
                string strOutputCode = "";
                // 把二维码字符串转换为读者证条码号
                // parameters:
                //      strReaderBcode  [out]读者证条码号
                // return:
                //      -1      出错
                //      0       所给出的字符串不是读者证号二维码
                //      1       成功      
                nRet = DecodeQrCode(strReaderBarcode,
                    out strOutputCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    // strQrCode = strBarcode;
                    strReaderBarcode = strOutputCode;
                }
            }

            if (sessioninfo.UserType == "reader")
            {
                // TODO: 如果使用身份证号，似乎这里会遇到阻碍
                if (strReaderBarcode != sessioninfo.Account.Barcode)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得读者信息被拒绝。作为读者只能对自己进行入馆登记操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            string strReaderXml = "";
            string strOutputReaderRecPath = "";
            string strLibraryCode = "";

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("PassGate 开始为读者加读锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForRead(strReaderBarcode);
            try // 读者记录锁定范围开始
            {

                // 读入读者记录
                byte[] reader_timestamp = null;
                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    result.Value = -1;
                    result.ErrorInfo = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                    return result;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                nRet = this.GetLibraryCode(strOutputReaderRecPath,
                    out strLibraryCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (string.IsNullOrEmpty(strLibraryCode) == false)
                    strGateName = strLibraryCode + ":" + strGateName;

                // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                if (String.IsNullOrEmpty(strOutputReaderRecPath) == false)
                {
                    // 检查当前操作者是否管辖这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
            sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "读者记录路径 '" + strOutputReaderRecPath + "' 从属的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }
                }

                /*
                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }
                 * */

                // 增量总量
                if (this.Statis != null)
                    this.Statis.IncreaseEntryValue(
                    strLibraryCode,
                    "入馆人次",
                    "所有门之总量",
                    1);

                // 增量特定门的累计量
                if (this.Statis != null)
                    nResultValue = this.Statis.IncreaseEntryValue(
                    strLibraryCode,
                    "入馆人次",
                    String.IsNullOrEmpty(strGateName) == true ? "(blank)" : strGateName,
                    (int)1);

                if (this.PassgateWriteToOperLog == true)
                {
                    XmlDocument domOperLog = new XmlDocument();
                    domOperLog.LoadXml("<root />");
#if NO
                    DomUtil.SetElementText(domOperLog.DocumentElement,
        "libraryCode",
        strLibraryCode);    // 读者所在的馆代码
#endif
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "operation",
                        "passgate");
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerBarcode",
                        strReaderBarcode);
                    DomUtil.SetElementText(domOperLog.DocumentElement,
        "libraryCode",
        strLibraryCode);
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "gateName",
                        strGateName);

                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        sessioninfo.UserID);

                    string strOperTime = this.Clock.GetClock();

                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTime);

                    nRet = this.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "PassGate() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }
                }
            } // 读者记录锁定范围结束
            finally
            {
                this.ReaderLocks.UnlockForRead(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("PassGate 结束为读者加读锁 '" + strReaderBarcode + "'");
#endif
            }

            if (String.IsNullOrEmpty(strResultTypeList) == true)
            {
                results = null; // 不返回任何结果
                goto END1;
            }

            string[] result_types = strResultTypeList.Split(new char[] { ',' });
            results = new string[result_types.Length];

            for (int i = 0; i < result_types.Length; i++)
            {
                string strResultType = result_types[i];

                if (String.Compare(strResultType, "xml", true) == 0)
                {
                    results[i] = strReaderXml;
                }
                // else if (String.Compare(strResultType, "html", true) == 0)
                else if (IsResultType(strResultType, "html") == true)
                {
                    string strReaderRecord = "";
                    // 将读者记录数据从XML格式转换为HTML格式
                    nRet = this.ConvertReaderXmlToHtml(
                        sessioninfo,
                        this.CfgDir + "\\readerxml2html.cs",
                        this.CfgDir + "\\readerxml2html.cs.ref",
                        strLibraryCode,
                        strReaderXml,
                        strOutputReaderRecPath, // 2009/10/18
                        OperType.None,
                        null,
                        "",
                        strResultType,
                        out strReaderRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "ConvertReaderXmlToHtml()出错(脚本程序为" + this.CfgDir + "\\readerxml2html.cs" + "): " + strError;
                        goto ERROR1;
                    }
                    results[i] = strReaderRecord;
                }
                // else if (String.Compare(strResultType, "text", true) == 0)
                else if (IsResultType(strResultType, "text") == true)
                {
                    string strReaderRecord = "";
                    // 将读者记录数据从XML格式转换为text格式
                    nRet = this.ConvertReaderXmlToHtml(
                        sessioninfo,
                        this.CfgDir + "\\readerxml2text.cs",
                        this.CfgDir + "\\readerxml2text.cs.ref",
                        strLibraryCode,
                        strReaderXml,
                        strOutputReaderRecPath, // 2009/10/18
                        OperType.None,
                        null,
                        "",
                        strResultType,
                        out strReaderRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "ConvertReaderXmlToHtml()出错(脚本程序为" + this.CfgDir + "\\readerxml2html.cs" + "): " + strError;
                        goto ERROR1;
                    }
                    results[i] = strReaderRecord;
                }
                else
                {
                    strError = "未知的结果类型 '" + strResultType + "'";
                    goto ERROR1;
                }
            }

        END1:
            result.Value = nResultValue;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        // 创建押金交费请求
        // parameters:
        //      strOutputReaderXml 返回修改后的读者记录
        //      strOutputID 返回本次创建的交费请求的 ID
        // result.Value -1 出错 其他 本次创建的交费请求条数
        public LibraryServerResult Foregift(
            SessionInfo sessioninfo,
            string strAction,
            string strReaderBarcode,
            out string strOutputReaderXml,
            out string strOutputID)
        {
            strOutputReaderXml = "";
            strOutputID = "";

            string strError = "";
            int nRet = 0;

            LibraryServerResult result = new LibraryServerResult();

            string strReaderXml = "";

            strAction = strAction.ToLower();

            if (strAction == "foregift")
            {
                // 权限判断
                if (StringUtil.IsInList("foregift", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "创建押金交费请求的操作被拒绝。不具备foregift权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else if (strAction == "return")
            {
                // 权限判断
                if (StringUtil.IsInList("returnforegift", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "创建退还押金(交费)请求的操作被拒绝。不具备returnforegift权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else
            {
                strError = "未知的strAction值 '" + strAction + "'";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }


            int nRedoCount = 0; // 因为时间戳冲突, 重试的次数
        REDO_FOREGIFT:


            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("Foregift 开始为读者加读锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForRead(strReaderBarcode);

            try // 读者记录锁定范围开始
            {

                // 读入读者记录
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;
                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    result.Value = -1;
                    result.ErrorInfo = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                    return result;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                string strLibraryCode = "";
                // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                if (String.IsNullOrEmpty(strOutputReaderRecPath) == false)
                {
                    // 检查当前操作者是否管辖这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
            sessioninfo.LibraryCodeList,
            out strLibraryCode) == false)
                    {
                        strError = "读者记录路径 '" + strOutputReaderRecPath + "' 从属的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 检查当前是不是已经有了押金交费请求
                XmlNodeList nodeOverdues = readerdom.DocumentElement.SelectNodes("overdues/overdue");
                for (int i = 0; i < nodeOverdues.Count; i++)
                {
                    XmlNode node = nodeOverdues[i];

                    string strWord = "押金。";

                    string strReason = DomUtil.GetAttr(node, "reason");
                    if (strReason.Length < strWord.Length)
                        continue;
                    string strPart = strReason.Substring(0, strWord.Length);
                    if (strPart == strWord)
                    {
                        strError = "读者 '" + strReaderBarcode + "' 已经存在押金交费请求。需要先将此押金请求交费完成后，才能创建新的押金交费请求。";
                        goto ERROR1;
                    }
                }

                string strOperTime = this.Clock.GetClock();

                string strOverdueString = "";
                // 根据Foregift() API要求，修改readerdom
                nRet = DoForegift(strAction,
                    readerdom,
                    ref strOutputID,   // 为空表示函数内自动创建id
                    sessioninfo.UserID,
                    strOperTime,
                    out strOverdueString,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
                // ***
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
#endif

                byte[] output_timestamp = null;
                string strOutputPath = "";

                strOutputReaderXml = readerdom.OuterXml;

                // 写回读者记录
                long lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                    strOutputReaderXml,
                    false,
                    "content",  // ,ignorechecktimestamp
                    reader_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        nRedoCount++;
                        if (nRedoCount > 10)
                        {
                            strError = "写回读者记录的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                            goto ERROR1;
                        }
                        goto REDO_FOREGIFT;
                    }
                    goto ERROR1;
                }

                // 增量总量
                if (this.Statis != null)
                    this.Statis.IncreaseEntryValue(
                    strLibraryCode,
                    "押金",
                    "创建交费请求次",
                    1);
                // TODO: 增加价格量?

                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "operation",
                    "foregift");
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "action",
                    strAction);
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "readerBarcode",
                    strReaderBarcode);

                // 新增的细节字符串 一个或者多个<overdue> OuterXml内容
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "overdues", strOverdueString/*nodeOverdue.OuterXml*/);

                // 新的读者记录
                XmlNode nodeReaderRecord = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "readerRecord", readerdom.OuterXml);
                DomUtil.SetAttr(nodeReaderRecord, "recPath", strOutputReaderRecPath);

                DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                    sessioninfo.UserID);

                // string strOperTime = this.Clock.GetClock();

                DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                    strOperTime);

                nRet = this.OperLog.WriteOperLog(domOperLog,
                    sessioninfo.ClientAddress,
                    out strError);
                if (nRet == -1)
                {
                    strError = "PassGate() API 写入日志时发生错误: " + strError;
                    goto ERROR1;
                }


            } // 读者记录锁定范围结束
            finally
            {
                this.ReaderLocks.UnlockForRead(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("Foregift 结束为读者加读锁 '" + strReaderBarcode + "'");
#endif
            }

            // END1:
            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        // 根据Foregift() API要求，修改readerdom
        // parameters:
        //      strAction   为foregift和return之一
        //      strID   违约金记录ID。如果此参数为null，表示函数会自动产生一个id。否则就用参数值
        int DoForegift(
            string strAction,
            XmlDocument readerdom,
            ref string strID,
            string strOperator,
            string strOperTime,
            out string strOverdueString,
            out string strError)
        {
            strOverdueString = "";
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strID) == true)
                strID = GetOverdueID();

            // 获得相关参数
            XmlNode nodeForegift = readerdom.DocumentElement.SelectSingleNode("foregift");
            if (nodeForegift == null)
            {
                nodeForegift = readerdom.CreateElement("foregift");
                readerdom.DocumentElement.AppendChild(nodeForegift);
            }

            string strExistPrice = nodeForegift.InnerText;

            string strCurrentDate = strOperTime;
            DateTime current_date = DateTimeUtil.FromRfc1123DateTimeString(strCurrentDate);

            if (strAction == "foregift")
            {

            }
            else if (strAction == "return")
            {
                // 要所有的overdues/overdue元素消失，borrows/borrow元素消失，才能进行return操作。这样做是为了避免退回押金后，又有超期还书等情况需要扣除押金
                XmlNodeList overdue_nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
                XmlNodeList borrow_nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

                string strMessage = "";
                if (overdue_nodes.Count > 0)
                {
                    strMessage += " " + overdue_nodes.Count.ToString() + " 个未交费事项";
                }

                if (borrow_nodes.Count > 0)
                {
                    if (String.IsNullOrEmpty(strMessage) == false)
                        strMessage += "和";

                    strMessage += " " + borrow_nodes.Count.ToString() + " 个已借未还书事项";
                }

                if (overdue_nodes.Count + borrow_nodes.Count > 0)
                {
                    strError = "本读者当前有" + strMessage + "，因此不能创建退还押金的请求。请先归还全部图书和交纳所有欠费。";
                    return -1;
                }
            }
            else
            {
                strError = "未知的strAction值 '" + strAction + "'";
                goto ERROR1;
            }

            int nResultValue = 0;
            string strForegiftPrice = "";
            // 执行脚本函数GetForegift
            // 根据已有价格，计算出需要新交的价格
            // parameters:
            // return:
            //      -2  not found script
            //      -1  出错
            //      0   成功
            nRet = this.DoGetForegiftScriptFunction(
                strAction,
                readerdom,
                strExistPrice,
                out nResultValue,
                out strForegiftPrice,
                out strError);
            if (nRet == -1 || nRet == -2)
                goto ERROR1;

            if (nResultValue == -1)
            {
                // strError?
                goto ERROR1;
            }

            // *** 修改读者记录

            // action "foregift" 和 "return" 并不立即修改当前读者记录<foregit>里面的价钱，而是要等到交费动作的时候才兑现

            // 看看根下面是否有overdues元素
            XmlNode root = readerdom.DocumentElement.SelectSingleNode("overdues");
            if (root == null)
            {
                root = readerdom.CreateElement("overdues");
                readerdom.DocumentElement.AppendChild(root);
            }

            // 添加一个overdue元素
            XmlNode nodeOverdue = readerdom.CreateElement("overdue");
            root.AppendChild(nodeOverdue);

            DomUtil.SetAttr(nodeOverdue, "reason", "押金。");
            DomUtil.SetAttr(nodeOverdue, "price", strForegiftPrice);    // 注：strForegiftPrice的值可能为"%return_foregift_price%"，表示当前剩余的押金额的负数
            DomUtil.SetAttr(nodeOverdue, "borrowDate", strCurrentDate);   // borrowDate中放起始日期参数
            DomUtil.SetAttr(nodeOverdue, "borrowPeriod", "");
            DomUtil.SetAttr(nodeOverdue, "returnDate", "");
            DomUtil.SetAttr(nodeOverdue, "borrowOperator", strOperator);  // 创建交费请求的人

            // id属性是唯一的, 为交违约金C/S界面创造了有利条件
            DomUtil.SetAttr(nodeOverdue, "id", strID);

            if (strAction == "return")
                DomUtil.SetAttr(nodeOverdue, "comment", "退还押金");


            strOverdueString = nodeOverdue.OuterXml;
            return 0;
        ERROR1:
            return -1;
        }

        // 创建租金交费请求
        // parameters:
        //      strOutputReaderXml 返回修改后的读者记录
        //      strOutputID 返回本次创建的交费请求的 ID
        // result.Value -1 出错 其他 本次创建的交费请求条数
        public LibraryServerResult Hire(
            SessionInfo sessioninfo,
            string strAction,
            string strReaderBarcode,
            out string strOutputReaderXml,
            out string strOutputID)
        {
            strOutputReaderXml = "";
            strOutputID = "";

            string strError = "";
            int nRet = 0;

            LibraryServerResult result = new LibraryServerResult();

            string strReaderXml = "";

            strAction = strAction.ToLower();

            if (strAction == "hire")
            {
                // 权限判断
                if (StringUtil.IsInList("hire", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "创建租金交费请求的操作被拒绝。不具备hire权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else if (strAction == "hirelate")
            {
                // 权限判断
                if (StringUtil.IsInList("hirelate", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "(延迟)创建租金交费请求的操作被拒绝。不具备hirelate权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            int nRedoCount = 0; // 因为时间戳冲突, 重试的次数
        REDO_HIRE:


            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("Hire 开始为读者加读锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForRead(strReaderBarcode);

            try // 读者记录锁定范围开始
            {

                // 读入读者记录
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;
                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    result.Value = -1;
                    result.ErrorInfo = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                    return result;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                string strLibraryCode = "";
                // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                if (String.IsNullOrEmpty(strOutputReaderRecPath) == false)
                {
                    // 检查当前操作者是否管辖这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
            sessioninfo.LibraryCodeList,
            out strLibraryCode) == false)
                    {
                        strError = "读者记录路径 '" + strOutputReaderRecPath + "' 从属的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 检查当前是不是已经有了租金交费请求
                XmlNodeList nodeOverdues = readerdom.DocumentElement.SelectNodes("overdues/overdue");
                for (int i = 0; i < nodeOverdues.Count; i++)
                {
                    XmlNode node = nodeOverdues[i];

                    string strWord = "租金。";

                    string strReason = DomUtil.GetAttr(node, "reason");
                    if (strReason.Length < strWord.Length)
                        continue;
                    string strPart = strReason.Substring(0, strWord.Length);
                    if (strPart == strWord)
                    {
                        strError = "读者 '" + strReaderBarcode + "' 已经存在租金交费请求。需要先将此租金请求交费完成后，才能创建新的租金交费请求。";
                        goto ERROR1;
                    }
                }

                string strOperTime = this.Clock.GetClock();

                string strOverdueString = "";
                // 根据Hire() API要求，修改readerdom
                nRet = DoHire(strAction,
                    readerdom,
                    ref strOutputID,   // 为空表示函数内自动创建id
                    sessioninfo.UserID,
                    strOperTime,
                    out strOverdueString,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
                // ***
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
#endif

                byte[] output_timestamp = null;
                string strOutputPath = "";

                strOutputReaderXml = readerdom.OuterXml;

                // 写回读者记录
                long lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                    strOutputReaderXml,
                    false,
                    "content",  // ,ignorechecktimestamp
                    reader_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        nRedoCount++;
                        if (nRedoCount > 10)
                        {
                            strError = "写回读者记录的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                            goto ERROR1;
                        }
                        goto REDO_HIRE;
                    }
                    goto ERROR1;
                }

                // 增量总量
                if (this.Statis != null)
                    this.Statis.IncreaseEntryValue(
                    strLibraryCode,
                    "租金",
                    "创建交费请求次",
                    1);
                // TODO: 增加价格量?

                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "operation",
                    "hire");
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "action",
                    strAction);
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "readerBarcode",
                    strReaderBarcode);

                // 新增的细节字符串 一个或者多个<overdue> OuterXml内容
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "overdues", strOverdueString/*nodeOverdue.OuterXml*/);

                // 新的读者记录
                XmlNode nodeReaderRecord = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "readerRecord", readerdom.OuterXml);
                DomUtil.SetAttr(nodeReaderRecord, "recPath", strOutputReaderRecPath);

                DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                    sessioninfo.UserID);

                // string strOperTime = this.Clock.GetClock();

                DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                    strOperTime);

                nRet = this.OperLog.WriteOperLog(domOperLog,
                    sessioninfo.ClientAddress,
                    out strError);
                if (nRet == -1)
                {
                    strError = "PassGate() API 写入日志时发生错误: " + strError;
                    goto ERROR1;
                }


            } // 读者记录锁定范围结束
            finally
            {
                this.ReaderLocks.UnlockForRead(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("Hire 结束为读者加读锁 '" + strReaderBarcode + "'");
#endif
            }

            // END1:
            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        // 根据Hire() API要求，修改readerdom
        // parameters:
        //      strID   违约金记录ID。如果此参数为null，表示函数会自动产生一个id。否则就用参数值
        int DoHire(
            string strAction,
            XmlDocument readerdom,
            ref string strID,
            string strOperator,
            string strOperTime,
            out string strOverdueString,
            out string strError)
        {
            strOverdueString = "";
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strID) == true)
                strID = GetOverdueID();

            // 获得相关参数
            XmlNode nodeHire = readerdom.DocumentElement.SelectSingleNode("hire");
            if (nodeHire == null)
            {
                // 2013/6/16
                nodeHire = readerdom.CreateElement("hire");
                readerdom.DocumentElement.AppendChild(nodeHire);
                //strError = "读者记录中没有租金参数定义 (<hire>元素)，因此无法创建租金交费请求";
                //goto ERROR1;
            }

            string strHirePeriod = DomUtil.GetAttr(nodeHire, "period");
            string strStartDate = "";

            string strCurrentDate = strOperTime;   //this.Clock.GetClock();
            DateTime current_date = DateTimeUtil.FromRfc1123DateTimeString(strCurrentDate);

            if (strAction == "hire")
            {
                strStartDate = DomUtil.GetAttr(nodeHire, "expireDate");

                // 如果记录中的末次租金失效期为空，则试取办证日期和当前时间的靠后者
                if (String.IsNullOrEmpty(strStartDate) == true)
                {
                    string strCreateDate = DomUtil.GetElementText(readerdom.DocumentElement,
                        "createDate");

                    // 如果根本没有办证时间
                    if (String.IsNullOrEmpty(strCreateDate) == true)
                    {
                        strStartDate = strCurrentDate;
                    }
                    else
                    {
                        // 如果有办证时间
                        DateTime createdate = new DateTime(0);
                        try
                        {
                            createdate = DateTimeUtil.FromRfc1123DateTimeString(strCreateDate);
                        }
                        catch
                        {
                            strError = "办证日期 <createDate> '" + strCreateDate + "' 格式错误";
                            goto ERROR1;
                        }

                        if (createdate > current_date)
                            strStartDate = strCreateDate;   // 采用办证时间
                        else
                            strStartDate = strCurrentDate;  // 采用当前时间
                    }
                }
            }
            else if (strAction == "hirelate")   // hire和hirelate有什么区别?
            {
                strStartDate = strCurrentDate;

                // 已经存在的末次租金失效期，参考
                string strExistStartDate = DomUtil.GetAttr(nodeHire, "expireDate");

                if (String.IsNullOrEmpty(strExistStartDate) == true)
                    goto SKIP_HIRE_LATE;

                DateTime exist_expiredate = new DateTime(0);
                try
                {
                    exist_expiredate = DateTimeUtil.FromRfc1123DateTimeString(strExistStartDate);
                }
                catch
                {
                    goto SKIP_HIRE_LATE;
                }

                DateTime temp_startdate = DateTimeUtil.FromRfc1123DateTimeString(strStartDate);

                // 如果当前日期比已经存在的末次租金失效期还靠前，就取靠后的一个，避免读者吃亏
                if (exist_expiredate > temp_startdate)
                    strStartDate = strExistStartDate;
            }

        SKIP_HIRE_LATE:

            int nResultValue = 0;
            string strHireExpireDate = "";
            string strHirePrice = "";
            // 执行脚本函数GetHire
            // 根据当前时间、周期，计算出失效期和价格
            // parameters:
            // return:
            //      -2  not found script
            //      -1  出错
            //      0   成功
            nRet = this.DoGetHireScriptFunction(
                readerdom,
                strStartDate,
                strHirePeriod,
                out nResultValue,
                out strHireExpireDate,
                out strHirePrice,
                out strError);
            if (nRet == -1 || nRet == -2)
                goto ERROR1;

            if (nResultValue == -1)
            {
                // strError?
                goto ERROR1;
            }

            // *** 修改读者记录

            // 修改租金失效期
            DomUtil.SetAttr(nodeHire, "expireDate", strHireExpireDate);

            // 推动证失效期
            string strReaderExpireDate = DomUtil.GetElementText(readerdom.DocumentElement,
                "expireDate");
            if (String.IsNullOrEmpty(strReaderExpireDate) == true)
                strReaderExpireDate = strHireExpireDate;

            // 
            DateTime reader_expiredate = new DateTime(0);
            try
            {
                reader_expiredate = DateTimeUtil.FromRfc1123DateTimeString(strReaderExpireDate);
            }
            catch
            {
                strError = "证失效期 '" + strReaderExpireDate + "' 不合法";
                goto ERROR1;
            }

            // 
            DateTime hire_expiredate = new DateTime(0);
            try
            {
                hire_expiredate = DateTimeUtil.FromRfc1123DateTimeString(strHireExpireDate);
            }
            catch
            {
                strError = "租金失效期 '" + strHireExpireDate + "' 不合法";
                goto ERROR1;
            }

            // 如果租金失效期大于证失效期，或者读者记录中现有失效期为空
            if (hire_expiredate > reader_expiredate
                || String.IsNullOrEmpty(DomUtil.GetElementText(readerdom.DocumentElement, "expireDate")) == true
                )
            {
                DomUtil.SetElementText(readerdom.DocumentElement,
                    "expireDate",
                    DateTimeUtil.Rfc1123DateTimeStringEx(hire_expiredate.ToLocalTime()));
            }

            // 看看根下面是否有overdues元素
            XmlNode root = readerdom.DocumentElement.SelectSingleNode("overdues");
            if (root == null)
            {
                root = readerdom.CreateElement("overdues");
                readerdom.DocumentElement.AppendChild(root);
            }

            // 添加一个overdue元素
            XmlNode nodeOverdue = readerdom.CreateElement("overdue");
            root.AppendChild(nodeOverdue);

            // DomUtil.SetAttr(nodeOverdue, "barcode", "");    // 册条码号为空
            // DomUtil.SetAttr(nodeOverdue, "recPath", strItemRecPath);


            DomUtil.SetAttr(nodeOverdue, "reason", "租金。于 " + strStartDate + " 交纳 " + strHirePeriod + " 的租金，失效期为 " + strHireExpireDate);
            DomUtil.SetAttr(nodeOverdue, "price", strHirePrice);
            DomUtil.SetAttr(nodeOverdue, "borrowDate", strStartDate);   // borrowDate中放起始日期参数
            DomUtil.SetAttr(nodeOverdue, "borrowPeriod", strHirePeriod);    // borrowperiod中放租金周期参数
            DomUtil.SetAttr(nodeOverdue, "returnDate", strHireExpireDate);  // returnDate中放失效期参数
            DomUtil.SetAttr(nodeOverdue, "borrowOperator", strOperator);  // 创建交费请求的人
            // DomUtil.SetAttr(nodeOverdue, "operator", strOperator);
            // id属性是唯一的, 为交违约金C/S界面创造了有利条件
            DomUtil.SetAttr(nodeOverdue, "id", strID);

            strOverdueString = nodeOverdue.OuterXml;
            return 0;
        ERROR1:
            return -1;
        }

        // 结算
        public LibraryServerResult Settlement(
            SessionInfo sessioninfo,
            string strAction,
            string[] ids)
        {
            string strError = "";
            int nRet = 0;

            LibraryServerResult result = new LibraryServerResult();

            strAction = strAction.ToLower();

            if (strAction == "settlement")
            {
                // 权限判断
                if (StringUtil.IsInList("settlement", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "结算操作被拒绝。不具备settlement权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else if (strAction == "undosettlement")
            {
                // 权限判断
                if (StringUtil.IsInList("undosettlement", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "撤销结算的操作被拒绝。不具备undosettlement权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else if (strAction == "delete")
            {
                // 权限判断
                if (StringUtil.IsInList("deletesettlement", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "删除结算记录的操作被拒绝。不具备deletesettlement权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else
            {
                strError = "无法识别的strAction参数值 '" + strAction + "'";
                goto ERROR1;
            }

            string strOperTime = this.Clock.GetClock();
            string strOperator = sessioninfo.UserID;

            //
            string strText = "";
            string strCount = "";

            strCount = "<maxCount>100</maxCount>";

            for (int i = 0; i < ids.Length; i++)
            {
                string strID = ids[i];

                if (i != 0)
                {
                    strText += "<operator value='OR' />";
                }

                strText += "<item><word>"
                    + StringUtil.GetXmlStringSimple(strID)
                    + "</word>"
                    + strCount
                    + "<match>exact</match><relation>=</relation><dataType>string</dataType>"
                    + "</item>";
            }

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(this.AmerceDbName + ":" + "ID")       // 2007/9/14
                + "'>" + strText
                + "<lang>zh</lang></target>";

            string strIds = String.Join(",", ids);

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "amerced",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
            {
                strError = "检索ID为 '" + strIds + "' 的违约金记录出错: " + strError;
                goto ERROR1;
            }

            if (lRet == 0)
            {
                strError = "没有找到id为 '" + strIds + "' 的违约金记录";
                goto ERROR1;
            }

            long lHitCount = lRet;

            long lStart = 0;
            long lPerCount = Math.Min(50, lHitCount);
            List<string> aPath = null;

            // 获得结果集，对逐个记录进行处理
            for (; ; )
            {
                lRet = channel.DoGetSearchResult(
                    "amerced",   // strResultSetName
                    lStart,
                    lPerCount,
                    "zh",
                    null,   // stop
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "未命中";
                    break;  // ??
                }

                // TODO: 要判断 aPath.Count == 0 跳出循环。否则容易进入死循环

                // 处理浏览结果
                for (int i = 0; i < aPath.Count; i++)
                {
                    string strPath = aPath[i];

                    string strCurrentError = "";

                    // 结算一个交费记录
                    nRet = SettlementOneRecord(
                        sessioninfo.LibraryCodeList,
                        true,   // 要创建日志
                        channel,
                        strAction,
                        strPath,
                        strOperTime,
                        strOperator,
                        sessioninfo.ClientAddress,
                        out strCurrentError);
                    // 遇到一般出错应当继续处理
                    if (nRet == -1)
                    {
                        strError += strAction + "违约金记录 '" + strPath + "' 时发生错误: " + strCurrentError + "\r\n";
                    }
                    // 但是遇到日志空间满这样的错误就不能继续处理了
                    if (nRet == -2)
                    {
                        strError = strCurrentError;
                        goto ERROR1;
                    }
                }

                lStart += aPath.Count;
                if (lStart >= lHitCount || lPerCount <= 0)
                    break;
            }

            if (strError != "")
                goto ERROR1;

            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;

        }

        // 结算一个交费记录
        // parameters:
        //      strLibraryCodeList  当前操作者管辖的图书馆代码
        //      bCreateOperLog  是否创建日志
        //      strOperTime 结算的操作时间
        //      strOperator 结算的操作者
        // return:
        //      -2  致命出错，不宜再继续循环调用本函数
        //      -1  一般出错，可以继续循环调用本函数
        //      0   正常
        int SettlementOneRecord(
            string strLibraryCodeList,
            bool bCreateOperLog,
            RmsChannel channel,
            string strAction,
            string strAmercedRecPath,
            string strOperTime,
            string strOperator,
            string strClientAddress,
            out string strError)
        {
            strError = "";

            string strMetaData = "";
            byte[] amerced_timestamp = null;
            string strOutputPath = "";
            string strAmercedXml = "";

            // 准备日志DOM
            XmlDocument domOperLog = null;

            if (bCreateOperLog == true)
            {

            }

            int nRedoCount = 0;
        REDO:

            long lRet = channel.GetRes(strAmercedRecPath,
                out strAmercedXml,
                out strMetaData,
                out amerced_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "获取违约金记录 '" + strAmercedRecPath + "' 时出错: " + strError;
                return -1;
            }

            XmlDocument amerced_dom = null;
            int nRet = LibraryApplication.LoadToDom(strAmercedXml,
                out amerced_dom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载违约金记录进入XML DOM时发生错误: " + strError;
                return -1;
            }

            string strLibraryCode = DomUtil.GetElementText(amerced_dom.DocumentElement, "libraryCode");
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
            {
                if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                {
                    strError = "当前用户未能管辖违约金记录 '" + strAmercedRecPath + "' 所在的馆代码 '" + strLibraryCode + "'";
                    return -1;
                }
            }

            if (bCreateOperLog == true)
            {
                domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");

                // 2012/10/2
                // 相关读者所在的馆代码
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "libraryCode", strLibraryCode);

                DomUtil.SetElementText(domOperLog.DocumentElement, "operation",
                    "settlement");
                DomUtil.SetElementText(domOperLog.DocumentElement, "action",
                    strAction);


                // 在日志中记忆 id
                string strID = DomUtil.GetElementText(amerced_dom.DocumentElement,
                    "id");
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "id", strID);
            }

            string strOldState = DomUtil.GetElementText(amerced_dom.DocumentElement,
                "state");

            if (strAction == "settlement")
            {
                if (strOldState != "amerced")
                {
                    strError = "结算操作前，记录状态必须为amerced。(但发现为'" + strOldState + "')";
                    return -1;
                }
                if (strOldState == "settlemented")
                {
                    strError = "结算操作前，记录状态已经为settlemented";
                    return -1;
                }
            }
            else if (strAction == "undosettlement")
            {
                if (strOldState != "settlemented")
                {
                    strError = "撤销结算操作前，记录状态必须为settlemented。(但发现为'" + strOldState + "')";
                    return -1;
                }
                if (strOldState == "amerced")
                {
                    strError = "撤销结算操作前，记录状态已经为settlemented";
                    return -1;
                }
            }
            else if (strAction == "delete")
            {
                if (strOldState != "settlemented")
                {
                    strError = "删除结算操作前，记录状态必须为settlemented。(但发现为'" + strOldState + "')";
                    return -1;
                }
            }
            else
            {
                strError = "无法识别的strAction参数值 '" + strAction + "'";
                return -1;
            }

            byte[] output_timestamp = null;

            if (bCreateOperLog == true)
            {
                // oldAmerceRecord
                XmlNode nodeOldAmerceRecord = DomUtil.SetElementText(domOperLog.DocumentElement,
    "oldAmerceRecord", strAmercedXml);
                DomUtil.SetAttr(nodeOldAmerceRecord, "recPath", strAmercedRecPath);
            }

            if (strAction == "delete")
            {
                // 删除已结算违约金记录
                lRet = channel.DoDeleteRes(strAmercedRecPath,
                    amerced_timestamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                        && nRedoCount < 10)
                    {
                        nRedoCount++;
                        amerced_timestamp = output_timestamp;
                        goto REDO;
                    }
                    strError = "删除已结算违约金记录 '" + strAmercedRecPath + "' 失败: " + strError;
                    this.WriteErrorLog(strError);
                    return -1;
                }

                goto END1;  // 写日志
            }

            // 修改状态
            if (strAction == "settlement")
            {
                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "state", "settlemented");


                // 清除两个信息
                DomUtil.DeleteElement(amerced_dom.DocumentElement,
                    "undoSettlementOperTime");
                DomUtil.DeleteElement(amerced_dom.DocumentElement,
                    "undoSettlementOperator");


                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "settlementOperTime", strOperTime);
                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "settlementOperator", strOperator);
            }
            else
            {
                Debug.Assert(strAction == "undosettlement", "");

                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "state", "amerced");


                // 清除两个信息
                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "settlementOperTime", "");
                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "settlementOperator", "");


                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "undoSettlementOperTime", strOperTime);
                DomUtil.SetElementText(amerced_dom.DocumentElement,
                    "undoSettlementOperator", strOperator);

            }

            if (bCreateOperLog == true)
            {
                DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                    strOperator);   // 操作者
                DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                    strOperTime);   // 操作时间
            }


            // 保存回数据库
            lRet = channel.DoSaveTextRes(strAmercedRecPath,
                amerced_dom.OuterXml,
                false,
                "content", // ?????,ignorechecktimestamp
                amerced_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "写回违约金记录 '" + strAmercedRecPath + "' 时出错: " + strError;
                return -1;
            }

            if (bCreateOperLog == true)
            {
                // amerceRecord
                XmlNode nodeAmerceRecord = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "amerceRecord", amerced_dom.OuterXml);
                DomUtil.SetAttr(nodeAmerceRecord, "recPath", strAmercedRecPath);
            }


        END1:
            if (bCreateOperLog == true)
            {
                if (this.Statis != null)
                {
                    if (strAction == "settlement")
                        this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "费用结算", "结算记录数", 1);
                    else if (strAction == "undosettlement")
                        this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "费用结算", "撤销结算记录数", 1);
                    else if (strAction == "delete")
                        this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "费用结算", "删除结算记录数", 1);
                }


                nRet = this.OperLog.WriteOperLog(domOperLog,
                    strClientAddress,
                    out strError);
                if (nRet == -1)
                {
                    strError = "settlement() API 写入日志时发生错误: " + strError;
                    return -2;
                }
            }

            return 0;
        }

#if NO
        static Hashtable ParseMedaDataXml(string strXml,
    out string strError)
        {
            strError = "";
            Hashtable result = new Hashtable();

            if (strXml == "")
                return result;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return null;
            }

            XmlAttributeCollection attrs = dom.DocumentElement.Attributes;
            for (int i = 0; i < attrs.Count; i++)
            {
                string strName = attrs[i].Name;
                string strValue = attrs[i].Value;

                result.Add(strName, strValue);
            }


            return result;
        }
#endif

        // 下载对象资源
        // return:
        //      -1  出错
        //      0   304返回
        //      1   200返回
        public int DownloadObject(System.Web.UI.Page Page,
            FlushOutput flushOutputMethod,
    RmsChannelCollection channels,
    string strPath,
    bool bSaveAs,
    out string strError)
        {
            strError = "";

            WebPageStop stop = new WebPageStop(Page);

            RmsChannel channel = channels.GetChannel(this.WsUrl);

            if (channel == null)
            {
                strError = "GetChannel() Error...";
                return -1;
            }

            // strPath = boards.GetCanonicalUri(strPath);

            // 获得资源。写入文件的版本。特别适用于获得资源，也可用于获得主记录体。
            // parameters:
            //		fileTarget	文件。注意在调用函数前适当设置文件指针位置。函数只会在当前位置开始向后写，写入前不会主动改变文件指针。
            //		strStyleParam	一般设置为"content,data,metadata,timestamp,outputpath";
            //		input_timestamp	若!=null，则本函数会把第一个返回的timestamp和本参数内容比较，如果不相等，则报错
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            string strMetaData = "";
            string strOutputPath;
            byte[] baOutputTimeStamp = null;

            // 获得媒体类型
            long lRet = channel.GetRes(
                strPath,
                null,	// Response.OutputStream,
                stop,
                "metadata",
                null,	// byte [] input_timestamp,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "GetRes() (for metadata) Error : " + strError;
                return -1;
            }

            if (Page.Response.IsClientConnected == false)
                return -1;

            // 取metadata中的mime类型信息
            Hashtable values = StringUtil.ParseMedaDataXml(strMetaData,
                out strError);

            if (values == null)
            {
                strError = "ParseMedaDataXml() Error :" + strError;
                return -1;
            }

            string strLastModifyTime = (string)values["lastmodifytime"];
            if (String.IsNullOrEmpty(strLastModifyTime) == false)
            {
                DateTime lastmodified = DateTime.Parse(strLastModifyTime);
                string strIfHeader = Page.Request.Headers["If-Modified-Since"];

                if (String.IsNullOrEmpty(strIfHeader) == false)
                {
                    DateTime isModifiedSince = DateTimeUtil.FromRfc1123DateTimeString(strIfHeader).ToLocalTime();

                    if (isModifiedSince != lastmodified)
                    {
                        // 修改过
                    }
                    else
                    {
                        // 没有修改过
                        Page.Response.StatusCode = 304;
                        Page.Response.SuppressContent = true;
                        return 0;
                    }

                }

                Page.Response.AddHeader("Last-Modified", DateTimeUtil.Rfc1123DateTimeString(lastmodified.ToUniversalTime()));
                /*
                                Page.Response.Cache.SetLastModified(lastmodified);
                                Page.Response.Cache.SetCacheability(HttpCacheability.Public);
                 * */
            }

            string strMime = (string)values["mimetype"];
            string strClientPath = (string)values["localpath"];
            if (strClientPath != "")
                strClientPath = PathUtil.PureName(strClientPath);

            // TODO: 如果是非image/????类型，都要加入content-disposition
            // 是否出现另存为对话框
            if (bSaveAs == true)
            {
                string strEncodedFileName = HttpUtility.UrlEncode(strClientPath, Encoding.UTF8);
                Page.Response.AddHeader("content-disposition", "attachment; filename=" + strEncodedFileName);
            }

            /*
            Page.Response.AddHeader("Accept-Ranges", "bytes");
            Page.Response.AddHeader("Last-Modified", "Wed, 21 Nov 2007 07:10:54 GMT");
             * */

            // 用 text/plain IE XML 搜索google
            // http://support.microsoft.com/kb/329661
            // http://support.microsoft.com/kb/239750/EN-US/
            /*
To use this fix, you must add the following registry value to the key listed below: 
Key: HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings

Value name: IsTextPlainHonored
Value type: DWORD
Value data: HEX 0x1 
             * */

            /*

            Page.Response.CacheControl = "no-cache";    // 如果不用此句，text/plain会被当作xml文件打开
            Page.Response.AddHeader("Pragma", "no-cache");
            Page.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
//            Page.Response.AddHeader("Cache-Control", "public");
            Page.Response.AddHeader("Expires", "0");
            Page.Response.AddHeader("Content-Transfer-Encoding", "binary");
             * */


            // 设置媒体类型
            if (strMime == "text/plain")
                strMime = "text";
            Page.Response.ContentType = strMime;

            string strSize = (string)values["size"];
            if (String.IsNullOrEmpty(strSize) == false)
            {
                Page.Response.AddHeader("Content-Length", strSize);
            }

            if (Page.Response.IsClientConnected == false)
                return -1;

            // 传输数据
            lRet = channel.GetRes(
                strPath,
                Page.Response.OutputStream,
                flushOutputMethod,
                stop,
                "content,data",
                null,	// byte [] input_timestamp,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "GetRes() (for res) Error : " + strError;
                return -1;
            }

            return 1;
        }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class AmerceItem
    {
        [DataMember]
        public string ID = "";  // 识别id
        [DataMember]
        public string NewPrice = "";    // 变更的价格
        [DataMember]
        public string NewComment = ""; // 注释
    }

    public class WebPageStop : Stop
    {
        System.Web.UI.Page Page = null;

        public WebPageStop(System.Web.UI.Page page)
        {
            this.Page = page;
        }

        public override int State
        {
            get
            {
                if (this.Page == null)
                    return -1;

                if (this.Page.Response.IsClientConnected == false)
                    return 2;

                return 0;
            }
        }

    }


    // 借书成功后的信息
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class BorrowInfo
    {
        // 应还日期/时间
        [DataMember]
        public string LatestReturnTime = "";    // RFC1123格式，GMT时间

        // 借书期限。例如“20day”
        [DataMember]
        public string Period = "";

        // 当前为续借的第几次？0表示初次借阅
        [DataMember]
        public long BorrowCount = 0;

        // 借书操作者
        [DataMember]
        public string BorrowOperator = "";

        /*
        // 2008/5/9
        // 所借的册的图书类型
        public string BookType = "";

        // 2008/5/9
        // 所借的册的馆藏地点
        public string Location = "";
         * */
    }

    // 还书成功后的信息
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class ReturnInfo
    {
        // 借阅日期/时间
        [DataMember]
        public string BorrowTime = "";    // RFC1123格式，GMT时间

        // 应还日期/时间
        [DataMember]
        public string LatestReturnTime = "";    // RFC1123格式，GMT时间

        // 原借书期限。例如“20day”
        [DataMember]
        public string Period = "";

        // 当前为续借的第几次？0表示初次借阅
        [DataMember]
        public long BorrowCount = 0;

        // 违约金描述字符串。XML格式
        [DataMember]
        public string OverdueString = "";

        // 借书操作者
        [DataMember]
        public string BorrowOperator = "";

        // 还书操作者
        [DataMember]
        public string ReturnOperator = "";

        // 2008/5/9
        /// <summary>
        /// 所还的册的图书类型
        /// </summary>
        [DataMember]
        public string BookType = "";

        // 2008/5/9
        /// <summary>
        /// 所还的册的馆藏地点
        /// </summary>
        [DataMember]
        public string Location = "";
    }

}
