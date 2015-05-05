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
using DigitalPlatform.CirculationClient;

public partial class MyMessage : MyWebPage
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
        int i = 0;
        i++;

        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        this.MessageListControl1.UserID = sessioninfo.UserID;

        this.TitleBarControl1.CurrentColumn = TitleColumn.Message;
        //this.TitleBarControl1.GetInboxUnreadCount -= new GetInboxUnreadCountEventHandler(HeadBarControl1_GetInboxUnreadCount);
        //this.TitleBarControl1.GetInboxUnreadCount += new GetInboxUnreadCountEventHandler(HeadBarControl1_GetInboxUnreadCount);
    }

#if NO
    void HeadBarControl1_GetInboxUnreadCount(object sender,
        GetInboxUnreadCountEventArgs e)
    {
        if (sessioninfo == null)
            return;
        int nUntouched = sessioninfo.Channel.GetUntouchedMessageCount(
            BoxesInfo.INBOX);
        /*
        if (nUntouched == -1)
        {
            return -1;
        }*/
        e.UnreadCount = nUntouched;
    }
#endif

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        if (this.IsPostBack == false)
        {
            string strAction = this.Request["action"];
            if (strAction == "getuntouched")
            {
                DoGetUntouched();
                return;
            }
        }

        // 是否登录?
        if (sessioninfo.UserID == "")
        {
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
            return;
        }

        string strError = "";

        if (sessioninfo.UserID == "public")
        {
            strError = "以访客身份登录时不能使用消息功能。";
            goto ERROR1;
        }

        // 注意：box参数中为boxtype值，也就是一套固定的中文的信箱名，不能用其他语言
        string strBox = this.Page.Request["box"];

        // 装入一个信箱的信息
        int nRet = this.MessageListControl1.LoadBox(
            sessioninfo.UserID,
            strBox,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        return;
    ERROR1:
        Response.Write(strError);
        Response.End();
    }

    // ajax请求获得当前未读的信件数
    // mymessage.aspx?action=getuntouched
    void DoGetUntouched()
    {
        // string strError = "";
        // int nRet = 0;

        GetUntouched result_info = new GetUntouched();
        if (sessioninfo == null)
        {
            result_info.ErrorString = "sessioninfo == null";
            goto END_GETINFO;
        }
        if (sessioninfo.UserID == "" || sessioninfo.UserID == "public")
        {
            goto END_GETINFO;
        }

        // return:
        //      -1  出错
        //      >=0 未读过的消息条数
        int nUntouched = sessioninfo.Channel.GetUntouchedMessageCount(BoxesInfo.INBOX);
        if (nUntouched == -1)
        {
            // 2014/3/12
            if (sessioninfo.Channel.ErrorCode == DigitalPlatform.CirculationClient.localhost.ErrorCode.NotLogin)
            {
                Session.Abandon();  // 迫使重新登录
                goto END_GETINFO;
            }
        }

        // 检查这个值是否有变化，如果有变化，需要把SessionInfo中的读者记录缓存清除，迫使后面的操作重新获取最新鲜的读者记录
        // 这样做的目的是，假如读者接到了通知信件，那可能是读者记录发生了改变(例如预约到书等)，这里及时清除缓存，能确保读者读到的信件和预约状态等显示保持同步，防止出现迷惑读者的信息新旧状态不同的情况
        // 当然，页面上的Refresh命令也能起到同样的作用
        {
            object o = this.Session["untouched_count"];
            if (o != null)
            {
                if ((int)o != nUntouched)
                    sessioninfo.Clear();
            }
            this.Session["untouched_count"] = nUntouched;
        }

        result_info.Count = nUntouched.ToString();
    END_GETINFO:
        this.Response.Write(GetResultString(result_info));
        this.Response.End();
    }

}

public class GetUntouched
{
    public string Count = "";
    public string ErrorString = "";
}