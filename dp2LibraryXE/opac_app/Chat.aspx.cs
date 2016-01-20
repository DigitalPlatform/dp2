using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Xml;
using System.Globalization;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Json;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.Drawing;

public partial class Chat : MyWebPage
{
    //OpacApplication app = null;
    //SessionInfo sessioninfo = null;

#if NO
    protected override void InitializeCulture()
    {
        WebUtil.InitLang(this);
        base.InitializeCulture();
    }
#endif

    protected void Page_Init(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        this.TitleBarControl1.CurrentColumn = TitleColumn.BookReview;
        this.TitleBarControl1.Dp2Sso = "first"; // SSO指定的登录优先起作用
    }

    void PrepareSsoLogin()
    {
        HttpCookie cookie = this.Request.Cookies["dp2-sso"];
        if (cookie != null)
        {
            Hashtable table = StringUtil.ParseParameters(cookie.Value, ',', '=', "url");
            string strDomain = (string)table["domain"];

            if (strDomain == "dp2opac")
                return; // 如果原始域来自dp2opac，就没有必要处理了

            string strUserName = (string)table["username"];
            string strLoginState = (string)table["loginstate"]; // in/out
            string strPhotoUrl = (string)table["photourl"];
            string strRights = (string)table["rights"];

            // 经过验证后，转换为SessionInfo状态
            if (strLoginState == "in")
            {
                if (sessioninfo.UserID != strUserName + "@" + strDomain)
                {
                    sessioninfo.UserID = strUserName + "@" + strDomain;
                    sessioninfo.PhotoUrl = strPhotoUrl;
                    sessioninfo.ReaderInfo = null;
                    sessioninfo.SsoRights = strRights;
                }
            }
            else
            {
                // 及时反映登出状态
                if (sessioninfo.UserID.IndexOf("@") != -1)
                    sessioninfo.UserID = "";
            }
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        // SSO登录
        PrepareSsoLogin();


        string strError = "";
        int nRet = 0;

#if NO
        // 是否登录?
        if (sessioninfo.UserID == "")
        {
            if (this.Page.Request["forcelogin"] == "on")
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                Response.Redirect("login.aspx", true);
                return;
            }
            if (this.Page.Request["forcelogin"] == "userid")
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                Response.Redirect("login.aspx?loginstyle=librarian", true);
                return;
            }
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
            return;
        }
#endif

        /*
        Decoder decoder = (Decoder)this.Session["decoder"];
        if (decoder == null)
        {
            decoder = Encoding.UTF8.GetDecoder();
            this.Session["decoder"] = decoder;
        }
         * */


        LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

        bool bIsManager = false;

        if (loginstate == LoginState.Librarian
    && StringUtil.IsInList("managechatroom", sessioninfo.RightsOrigin) == true)
            bIsManager = true;

        string strAction = this.Request["action"];

        if (strAction == "getimage")
        {
            DoGetImage();
            return;
        }

        if (strAction == "sendimage")
        {
            DoSendImage();
            return;
        }

        if (strAction == "send")
        {
            DoSendText(loginstate);
            return;
        }

        if (strAction == "delete")
        {
            DoDeleteItem(-1, bIsManager);
            return;
        }

        if (strAction == "getinfo")
        {
            DoGetInfo(bIsManager);
            return;
        }

        if (strAction == "optionchanged")
        {
            DoOptionChanged();
            return;
        }

        // 以下为出现页面
        if (this.IsPostBack == false)
        {
            List<string> name_list = app.ChatRooms.GetRoomNames(
                MergeRights(sessioninfo.RightsOrigin, sessioninfo.SsoRights));
            foreach (string name in name_list)
            {
                this.DropDownList_roomName.Items.Add(name);
            }

            if (loginstate == LoginState.Public
                || loginstate == LoginState.NotLogin)
                this.TextBox_userName.Enabled = true;
            else
            {
                this.TextBox_userName.Enabled = false;

                this.TextBox_userName.Text = sessioninfo.UserID;
                if (loginstate == LoginState.Reader
                    && sessioninfo.ReaderInfo != null
                    && string.IsNullOrEmpty(sessioninfo.ReaderInfo.DisplayName) == false)
                    this.TextBox_userName.Text = "[" + sessioninfo.ReaderInfo.DisplayName + "]";
            }

            // 通过URL直接选定栏目
            string strRoom = this.Request["room"];
            if (string.IsNullOrEmpty(strRoom) == false)
            {
                DecodeUri(ref strRoom);

                string strTemp = "";
                // return:
                //      -2  权限不够
                //      -1  出错
                //      0   栏目不存在
                //      1   栏目找到
                nRet = app.GetRoomName(
                    MergeRights(sessioninfo.RightsOrigin, sessioninfo.SsoRights),
                    strRoom,
                    out strTemp);
                if (nRet == 1)
                {
                    strRoom = strTemp;
                    this.DropDownList_roomName.Text = strRoom;
                }
                else if (nRet == -2)
                {
                    /*
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                Response.Redirect("login.aspx", true);
                     * */
                    Response.Redirect(this.TitleBarControl1.GetLoginUrl(), true);
                    return;
                }
                else
                {
                    strError = "栏目 '" + strRoom + "' 不存在";
                    goto ERROR1;
                }
            }

            // 首次设置displayOperation(缺省为false) 根据Cookie
            HttpCookie cookie = this.Request.Cookies["dp2opac-chat"];
            if (cookie != null)
            {
                Hashtable table = StringUtil.ParseParameters(cookie.Value, ',', '=', "url");

                this.CheckBox_displayOperation.Checked = StringUtil.GetBooleanValue((string)table["displayOperation"], false);
            }
            else
            {
                this.CheckBox_displayOperation.Checked = false;
            }

            if (string.IsNullOrEmpty(sessioninfo.UserID) == true)
                this.Label_userNameComment.Text = "(您现在是访客身份)";
        }

        this.TextBox_currentDate.Text = DateTimeUtil.DateTimeToString8(this.Calendar1.SelectedDate);
        if (this.TextBox_currentDate.Text == "00010101")
        {
            this.TextBox_currentDate.Text = DateTimeUtil.DateTimeToString8(DateTime.Now);
            this.Calendar1.SelectedDate = DateTime.Now;
        }

        if (bIsManager == true)
        {
            // 创建和删除栏目的界面要素
            this.PlaceHolder_createDialog.Visible = true;
            this.PlaceHolder_managemenPanel.Visible = true;
        }
        else
        {
            this.PlaceHolder_createDialog.Visible = false;
            this.PlaceHolder_managemenPanel.Visible = false;
        }

        {
            // -1 0 1
            int nIsEditor = app.IsEditor(this.DropDownList_roomName.Text,
                 sessioninfo.UserID,
                 out strError);

            if (nIsEditor == 1 || bIsManager == true)
            {
                // 指示javascript代码，显示每个消息的删除按钮
                this.HiddenField_isManager.Value = "yes";
            }
            else
            {
                this.HiddenField_isManager.Value = "no";
            }
        }

        this.HiddenField_today.Value = DateTimeUtil.DateTimeToString8(DateTime.Now);

        ChatRoom room = app.GetChatRoom(
            MergeRights(sessioninfo.RightsOrigin, sessioninfo.SsoRights),
            this.DropDownList_roomName.Text);
        this.HiddenField_editors.Value = room != null ? StringUtil.MakePathList(room.EditorList) : "";
        return;
    ERROR1:
        this.Response.Write(strError);
        this.Response.End();
    }

    static string MergeRights(string r1, string r2)
    {
        if (string.IsNullOrEmpty(r1) == true
            && string.IsNullOrEmpty(r2) == true)
            return "";
        if (string.IsNullOrEmpty(r1) == true)
            return r2;
        if (string.IsNullOrEmpty(r2) == true)
            return r1;
        return r1 + "," + r2;
    }

    void DoOptionChanged()
    {
        string strOptions = this.Request.Form["options"];

        HttpCookie cookie = this.Response.Cookies["dp2opac-chat"];
        if (cookie == null)
        {
            cookie = new HttpCookie("dp2opac-chat", strOptions);
            cookie.Expires = DateTime.Now + new TimeSpan(365, 0, 0, 0, 0);  // 一年以后失效
            this.Response.Cookies.Add(cookie);
        }
        else
        {
            Hashtable table = StringUtil.ParseParameters(strOptions, ',', '=', "url");

            Hashtable old_table = StringUtil.ParseParameters(cookie.Value, ',', '=', "url");

            Hashtable new_table = StringUtil.MergeParametersTable(old_table, table);

            string strValue = StringUtil.BuildParameterString(new_table, ',', '=', "url");

            // 和现有的内容合并
            cookie.Value = strValue;
            cookie.Expires = DateTime.Now + new TimeSpan(365, 0, 0, 0, 0);  // 一年以后失效
        }

        this.Response.End();
    }

    static void DecodeUri(ref string strText)
    {
        if (string.IsNullOrEmpty(strText) == true)
            return ;

        if (strText.IndexOf("%u") != -1)
            strText = HttpUtility.UrlDecode(strText);
    }

    // ajax请求获得栏目内容
    // chat.aspx?action=getinfo&room=xxx&date=xxx&start=xxx&maxlines=xxx
    void DoGetInfo(bool bIsManager)
    {
        string strError = "";
        int nRet = 0;

        GetResultInfo result_info = new GetResultInfo();

        string strRoom = this.Request["room"];
        if (string.IsNullOrEmpty(strRoom) == true)
            strRoom = "default";
        else
        {
            DecodeUri(ref strRoom);
        }

        string strStart = this.Request["start"];
        string strMaxLines = this.Request["max_lines"];
        string strDate = this.Request["date"];

        if (string.IsNullOrEmpty(strStart) == true)
        {
            result_info.ErrorString = "未指定 result_offset 参数";
            goto END_GETINFO;
        }
        long lStart = 0;
        Int64.TryParse(strStart, out lStart);
        int nMaxLines = 4096;
        Int32.TryParse(strMaxLines, out nMaxLines);

        ChatInfo info = null;

        Debug.WriteLine("GetBatchTaskInfo()");

        bool bDisplayAllIP = false;
        if (bIsManager == true)
            bDisplayAllIP = true;

        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        nRet = app.GetChatInfo(
            MergeRights(sessioninfo.RightsOrigin, sessioninfo.SsoRights),
            strRoom,
            strDate,
            lStart,
            nMaxLines,
            bDisplayAllIP,
            out info,
            out strError);
        if (nRet == -1 || nRet == 0)
        {
            result_info.ErrorString = strError;
            goto END_GETINFO;
        }

        result_info.Name = info.Name;
        // result_info.MaxResultBytes = info.MaxResultBytes;
        result_info.ResultText = info.ResultText;
        result_info.NextStart = info.NextStart;
        result_info.TotalLines = info.TotalLines;
        result_info.ResultVersion = info.ResultVersion;

    END_GETINFO:
        /*
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChatResultInfo));

        MemoryStream ms = new MemoryStream();
        ser.WriteObject(ms, result_info);
        string strResult = Encoding.UTF8.GetString(ms.ToArray());
        ms.Close();
         * */

        this.Response.Write(GetResultString(result_info));
        this.Response.End();
    }

    // TODO: 要禁止删除一个操作信息事项
    // ajax请求删除一个条目
    // chat.aspx?action=delete&refid=xxx&date=xxx
    // 版主和managechatroom的帐户可以进行删除
    void DoDeleteItem(int nIsEditor,
        bool bIsManager)
    {
        string strError = "";
        int nRet = 0;

        DeleteResultInfo result_info = new DeleteResultInfo();

        string strRoom = this.Request.Form["room"];

        if (nIsEditor == -1)
        {
            // -1 0 1
            nIsEditor = app.IsEditor(strRoom,
                 sessioninfo.UserID,
                 out strError);
        }

        if (bIsManager == false && nIsEditor != 1)
        {
            result_info.ErrorString = "当前帐户不具备 managechatroom 权限，也不是编辑身份，无法删除消息";
            result_info.ResultValue = -1;
            goto END_DELETE;
        }

        string strRefID = this.Request.Form["refid"];
        string strDate = this.Request["date"];

        long lNewVersion = 0;
        nRet = app.DeleteChatItem(
            MergeRights(sessioninfo.RightsOrigin, sessioninfo.SsoRights),
            strRoom,
            strDate,
            strRefID,
            false,  // bChangeVersion
            true,   // bNotify
            sessioninfo.UserID,
            out lNewVersion,
            out strError);

        result_info.NewFileVersion = lNewVersion;
        result_info.ErrorString = strError;
        result_info.ResultValue = nRet;

    END_DELETE:
        this.Response.Write(GetResultString(result_info));
        this.Response.End();
    }

    // ajax请求发送文字内容，创建一个条目
    // chat.aspx?action=send&data=xxx&room=xxx&text=xxx&style=xxx&name=xxx
    void DoSendText(LoginState loginstate)
    {
        string strError = "";
        int nRet = 0;

        SendResultInfo result_info = new SendResultInfo();

        string strDate = this.Request["date"];
        if (strDate != DateTimeUtil.DateTimeToString8(DateTime.Now))
        {
            result_info.ErrorString = "不允许发送到非今天的文件中";
            result_info.ResultValue = -1;
            goto END_SEND;
        }

        string strRoom = this.Request.Form["room"];
        string strText = this.Request.Form["text"];
        string strStyle = this.Request.Form["style"];
        string strUserName = this.Request.Form["name"];

        string strUserID = sessioninfo.UserID;
        string strDisplayName = "";
        string strIP = "";
        if (loginstate == LoginState.NotLogin
            || loginstate == LoginState.Public)
        {
            strUserID = "(访客)" + strUserName;
            strIP = this.Request.UserHostAddress.ToString();
        }
        else if (loginstate == LoginState.Reader)
        {
            if (sessioninfo.ReaderInfo != null
                && string.IsNullOrEmpty(sessioninfo.ReaderInfo.DisplayName) == false)
                strDisplayName = sessioninfo.ReaderInfo.DisplayName;
        }

        nRet = app.CreateChatItem(
            MergeRights(sessioninfo.RightsOrigin, sessioninfo.SsoRights),
            strRoom,
            strUserID,
            strDisplayName,
            strIP,
            strText,
            strStyle,
            sessioninfo.PostedFileInfo != null ? sessioninfo.PostedFileInfo.FileName : "",
            // null,   // this.FileUpload1.PostedFile,
            sessioninfo.PhotoUrl,
            out strError);
        result_info.ErrorString = strError;
        result_info.ResultValue = nRet;

        DeleteTempFile();

    END_SEND:
        this.Response.Write(GetResultString(result_info));
        this.Response.End();
    }

    // ajax请求发送给服务器一个图像文件，要求暂存到Session中，供后面的sendtext请求使用
    // chat.aspx?action=sendimage
    void DoSendImage()
    {
        string strError = "";
        int nRet = 0;

        if (this.Request.Files.Count == 0)
        {
            strError = "请求中没有包含图像文件";
            goto ERROR1;
        }

        try
        {

            DeleteTempFile();

            HttpPostedFile file = this.Request.Files[0];

            string strContentType = file.ContentType;
            string strImage = StringUtil.GetFirstPartPath(ref strContentType);
            if (strImage != "image")
            {
                strError = "只允许上传图像文件";
                goto ERROR1;
            }

            sessioninfo.PostedFileInfo = new PostedFileInfo();
            sessioninfo.PostedFileInfo.FileName = PathUtil.MergePath(sessioninfo.GetTempDir(), "postedfile" + Path.GetExtension(file.FileName));
            //file.SaveAs(sessioninfo.PostedFileInfo.FileName + ".tmp");

            //bool bWrited = false;
            using (Stream target = File.Create(sessioninfo.PostedFileInfo.FileName))
            {
                // 缩小尺寸
                nRet = GraphicsUtil.ShrinkPic(file.InputStream,
                        file.ContentType,
                        app.ChatRooms.PicMaxWidth,
                        app.ChatRooms.PicMaxHeight,
                        true,
                        target,
                        out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)  // 没有必要缩放
                {
                    if (file.InputStream.CanSeek == true)
                        file.InputStream.Seek(0, SeekOrigin.Begin); // 2012/5/20
                    StreamUtil.DumpStream(file.InputStream, target);
                    // bWrited = false;
                }
            }

            /*
            if (bWrited == false)
                File.Copy(sessioninfo.PostedFileInfo.FileName + ".tmp",
                    sessioninfo.PostedFileInfo.FileName,
                    true);
             * */

        }
        catch (Exception ex)
        {
            strError = "ShrinkPic error -" + ex.Message;
            goto ERROR1;
        }

        this.Response.Write("{\"result\":\"OK\"}");
        this.Response.End();
        return;
    ERROR1:
        // app.WriteErrorLog("DoSendImage() error :" + strError);

        this.Response.Charset = "utf-8";
        // this.Response.StatusCode = 500;
        // this.Response.StatusDescription = strError;
        SendImageResultInfo result_info = new SendImageResultInfo();
        result_info.error = strError;
        this.Response.Write(GetResultString(result_info));
        this.Response.End();
    }

    // 浏览器通过URL获取图像文件
    // chat.aspx?action=getimage&room=xxx&filename=xxx
    // filename为纯文件名
    void DoGetImage()
    {
        string strError = "";
        int nRet = 0;

        string strFileName = this.Request["filename"];
        string strRoomName = this.Request["room"];

        DecodeUri(ref strRoomName);

        string strFilePath = app.GetPhysicalImageFilename(
            MergeRights(sessioninfo.RightsOrigin, sessioninfo.SsoRights),
            strRoomName,
            strFileName);
        // string strContentType = API.MimeTypeFrom(ReadFirst256Bytes(strFilePath), "");
        string strContentType = PathUtil.MimeTypeFrom(strFilePath);
        nRet = DumpFile(strFilePath,
            strContentType,
            out strError);
        if (nRet == 0)
        {
            this.Response.ContentType = "text/html";
            this.Response.StatusCode = 404;
            this.Response.StatusDescription = strError;
            this.Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p></body></html>");
            this.Response.Flush();
            this.Response.End();
            return;
        }
        if (nRet == -1)
        {
            this.Response.ContentType = "text/html";
            this.Response.StatusCode = 500;
            this.Response.StatusDescription = strError;
            this.Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p></body></html>");
            this.Response.Flush();
            this.Response.End();
            return;
        }

        this.Response.Flush();
        this.Response.End();
    }

#if NO
    // 读取文件前256bytes
    byte[] ReadFirst256Bytes(string strFileName)
    {
        FileStream fileSource = File.Open(
            strFileName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);

        byte[] result = new byte[Math.Min(256, fileSource.Length)];
        fileSource.Read(result, 0, result.Length);

        fileSource.Close();

        return result;
    }
#endif

    // return:
    //      -1  出错
    //      0   成功
    //      1   暂时不能访问
    int DumpFile(string strFilename,
        string strContentType,
        out string strError)
    {
        strError = "";

        // 不让浏览器缓存页面
        this.Response.AddHeader("Pragma", "no-cache");
        this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        this.Response.AddHeader("Expires", "0");

        this.Response.ContentType = strContentType;

        try
        {
            using (Stream stream = File.Open(strFilename,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.ReadWrite))
            {
                this.Response.AddHeader("Content-Length", stream.Length.ToString());

                FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

                stream.Seek(0, SeekOrigin.Begin);

                StreamUtil.DumpStream(stream, this.Response.OutputStream,
                    flushdelegate);
            }
        }
        catch (FileNotFoundException)
        {
            strError = "文件 '" + strFilename + "' 不存在";
            return 0;
        }
        catch (DirectoryNotFoundException)
        {
            strError = "文件 '" + strFilename + "' 路径中某一级目录不存在";
            return 0;
        }
        catch (Exception ex)
        {
            strError = ExceptionUtil.GetAutoText(ex);
            return -1;
        }

        return 1;
    }

    bool MyFlushOutput()
    {
        Response.Flush();
        return Response.IsClientConnected;
    }

    void DeleteTempFile()
    {
        if (sessioninfo.PostedFileInfo != null
    && string.IsNullOrEmpty(sessioninfo.PostedFileInfo.FileName) == false)
        {
            try
            {
                File.Delete(sessioninfo.PostedFileInfo.FileName);
                sessioninfo.PostedFileInfo.FileName = "";
            }
            catch
            {
            }
        }
    }

#if NO
    static string GetResultText(Decoder decoder, byte[] baResult)
    {
        if (baResult == null)
            return "";
        if (baResult.Length == 0)
            return "";

        // Decoder ResultTextDecoder = Encoding.UTF8.GetDecoder;
        char[] chars = new char[baResult.Length];

        int nCharCount = decoder.GetChars(
            baResult,
                0,
                baResult.Length,
                chars,
                0);
        Debug.Assert(nCharCount <= baResult.Length, "");

        return new string(chars, 0, nCharCount).Replace("\r", "<br/>");
    }
#endif

    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        this.TextBox_currentDate.Text = DateTimeUtil.DateTimeToString8(this.Calendar1.SelectedDate);
    }
    protected void Calendar1_DayRender(object sender, DayRenderEventArgs e)
    {

        //      -1  error
        //      0   file not found
        //      1   succeed
        string strDate = DateTimeUtil.DateTimeToString8(e.Day.Date);

        string strRoom = this.DropDownList_roomName.Text;
        if (string.IsNullOrEmpty(strRoom) == true)
            strRoom = "default";

        ChatInfo info = null;
        string strError = "";
        int nRet = app.GetChatInfo(
            MergeRights(sessioninfo.RightsOrigin, sessioninfo.SsoRights),
            strRoom,
            strDate,
            0,
            0,
            true,
            out info,
            out strError);
        if (nRet == -1 || nRet == 0)
        {
            e.Day.IsSelectable = false;
            e.Cell.ForeColor = System.Drawing.Color.Gray;
        }
        else
        {
            e.Day.IsSelectable = true;
        }

    }

    protected void Button_create_Click(object sender, EventArgs e)
    {
        string strError = "";

        if (StringUtil.IsInList("managechatroom", sessioninfo.RightsOrigin) == false)
        {
            strError = "当前帐户不具备 managechatroom 权限，无法创建新栏目";
            goto ERROR1;
        }


        int nRet = app.CreateChatRoom(TextBox_roomName.Text,
            out strError);
        if (nRet == -1)
        {
            goto ERROR1;
        }

        // 刷新栏目名字列表
        this.DropDownList_roomName.Items.Clear();
        List<string> name_list = app.ChatRooms.GetRoomNames(MergeRights(sessioninfo.RightsOrigin, sessioninfo.SsoRights));
        foreach (string name in name_list)
        {
            this.DropDownList_roomName.Items.Add(name);
        }

        this.DropDownList_roomName.Text = TextBox_roomName.Text;
        return;
    ERROR1:
        this.Response.Write(strError);
        this.Response.End();
    }
    protected void Button_deleteChatRoom_Click(object sender, EventArgs e)
    {
        string strError = "";

        if (StringUtil.IsInList("managechatroom", sessioninfo.RightsOrigin) == false)
        {
            strError = "当前帐户不具备 managechatroom 权限，无法删除栏目";
            goto ERROR1;
        }


        int nRet = app.DeleteChatRoom(this.DropDownList_roomName.Text,
            out strError);
        if (nRet == -1)
        {
            goto ERROR1;
        }

        // 刷新栏目名字列表
        this.DropDownList_roomName.Items.Clear();
        List<string> name_list = app.ChatRooms.GetRoomNames(MergeRights(sessioninfo.RightsOrigin, sessioninfo.SsoRights));
        foreach (string name in name_list)
        {
            this.DropDownList_roomName.Items.Add(name);
        }
        return;
    ERROR1:
        this.Response.Write(strError);
        this.Response.End();

    }
}

public class GetResultInfo
{
    // 名字
    public string Name = "";

    // 输出结果
    // public int MaxResultBytes = 0;
    public string ResultText = null;
    public long NextStart = 0;   // 本次获得到ResultText达的末尾点
    public long TotalLines = 0;  // 整个结果文件的长度

    public long ResultVersion = 0;  // 信息文件版本

    public string ErrorString = "";
}

public class DeleteResultInfo
{
    public long NewFileVersion = 0;
    public long ResultValue = 0;
    public string ErrorString = "";
}

public class SendResultInfo
{
    public long ResultValue = 0;
    public string ErrorString = "";
}

public class SendImageResultInfo
{
    public string error = "";
}