using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Xml;
using System.Globalization;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
// using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

public partial class Message : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = TitleColumn.Message;

        //this.TitleBarControl1.GetInboxUnreadCount -= new GetInboxUnreadCountEventHandler(HeadBarControl1_GetInboxUnreadCount);
        //this.TitleBarControl1.GetInboxUnreadCount += new GetInboxUnreadCountEventHandler(HeadBarControl1_GetInboxUnreadCount);
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        // 是否登录?
        if (sessioninfo.UserID == "")
        {
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
            return;
        }

        string strSenderName = "";
        if (sessioninfo.ReaderInfo != null
            && sessioninfo.IsReader == true)
        {
            string strDisplayName = "";
            string strBarcode = "";

            strDisplayName = sessioninfo.ReaderInfo.DisplayName;
            strBarcode = sessioninfo.ReaderInfo.Barcode;

            strSenderName = BoxesInfo.BuildOneAddress(strDisplayName, strBarcode);
        }
        else
            strSenderName = sessioninfo.UserID;

        this.MessageControl1.UserID = strSenderName;

        string strMessageID = this.Request["id"];
        string strMessageIDs = this.Request["ids"];

        if (this.IsPostBack == false)
        {
            // ids参数不为空
            if (String.IsNullOrEmpty(strMessageIDs) == false)
            {
                string[] ids = strMessageIDs.Split(new char[] { ',' });
                List<string> idlist = new List<string>();
                for (int i = 0; i < ids.Length; i++)
                {
                    idlist.Add(ids[i]);
                }

                this.MessageControl1.MessageData = null;
                this.MessageControl1.RecordID = null;
                this.MessageControl1.TimeStamp = null;
                this.MessageControl1.RecordIDs = idlist;
                this.MessageControl1.RecordIDsIndex = 0;
                return;
            }

            // id参数不为空
            if (String.IsNullOrEmpty(strMessageID) == false)
            {
                LibraryChannel channel = sessioninfo.GetChannel(true);
                try
                {
                    string strError = "";
                    MessageData[] messages = null;
                    string[] ids = new string[1];
                    ids[0] = strMessageID;
                    // 根据消息记录id获得消息详细内容
                    // 本函数还将检查消息是否属于strUserID指明的用户
                    // parameters:
                    //      strUserID   如果==null，表示不检查消息属于何用户
                    long nRet = // sessioninfo.Channel.
                        channel.GetMessage(
                        ids,
                        MessageLevel.Full,
                        out messages,
                        out strError);
                    if (nRet == -1)
                    {
                        this.Response.Write(strError);
                        this.Response.End();
                    }
                    if (messages == null || messages.Length < 1)
                    {
                        strError = "messages error";
                        this.Response.Write(strError);
                        this.Response.End();
                    }
                    this.MessageControl1.MessageData = messages[0];
                }
                finally
                {
                    sessioninfo.ReturnChannel(channel);
                }
            }
            else
            {
                string strRecipient = this.Request.QueryString["recipient"];
                if (String.IsNullOrEmpty(strRecipient) == false)
                    this.MessageControl1.Recipient = strRecipient;

                // 新创建的消息
                this.MessageControl1.MessageData = null;
                this.MessageControl1.RecordID = null;
                this.MessageControl1.TimeStamp = null;
            }
        }
    }
}