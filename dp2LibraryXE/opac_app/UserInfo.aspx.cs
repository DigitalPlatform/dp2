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
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.CirculationClient;

public partial class UserInfo : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = DigitalPlatform.OPAC.Web.TitleColumn.Browse;

        this.TitleBarControl1.LibraryCodeChanged += new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);
        this.TitleBarControl1.LibraryCodeChanged -= new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);

        this.BrowseSearchResultControl1.DefaultFormatName = "详细";
        this.BrowseSearchResultControl1.Visible = true;
    }

    void TitleBarControl1_LibraryCodeChanged(object sender, LibraryCodeChangedEventArgs e)
    {
        this.BrowseSearchResultControl1.ResetAllItemsControlPager();
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
            sessioninfo.UserID = "public";
            sessioninfo.IsReader = false;
        }

        string strError = "";
        int nRet = 0;

        SessionInfo temp_sessioninfo = new SessionInfo(app);
        temp_sessioninfo.UserID = app.ManagerUserName;
        temp_sessioninfo.Password = app.ManagerPassword;
        temp_sessioninfo.IsReader = false;
        try
        {

            bool bHintDisplayName = false;  // []暗示为显示名
            string strDisplayName = this.Request["displayName"];
            string strBarcode = this.Request["barcode"];
            string strEncyptBarcode = Request.QueryString["encrypt_barcode"];

            string strText = "";

            // 如果为加密的条码形态
            if (String.IsNullOrEmpty(strEncyptBarcode) == false)
            {
                strBarcode = OpacApplication.DecryptPassword(strEncyptBarcode);
                if (strBarcode == null)
                {
                    strError = "encrypt_barcode参数值格式错误";
                    goto ERROR1;
                }
                bHintDisplayName = true;
                goto SEARCH_COMMENT;
            }

            {
                if (String.IsNullOrEmpty(strDisplayName) == false)
                {
                    if (strDisplayName.IndexOfAny(new char[] { '[', ']' }) != -1)
                        bHintDisplayName = true;
                    strDisplayName = strDisplayName.Replace("[", "").Trim();
                    strDisplayName = strDisplayName.Replace("]", "").Trim();
                }


                nRet = 0;
                string strReaderXml = "";
                string strOutputReaderPath = "";

                if (String.IsNullOrEmpty(strDisplayName) == false)
                {
                    byte[] timestamp = null;

                    /*
                    // 通过读者显示名获得读者记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = app.GetReaderRecXmlByDsiplayName(
                    sessioninfo.Channels,
                    strDisplayName,
                    out strReaderXml,
                    out strOutputReaderPath,
                    out timestamp,
                    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0 && bHintDisplayName == true)
                    {
                        strBarcode = "";
                        goto SEARCH_COMMENT;
                    }
                    */
                    string[] results = null;
                    long lRet = temp_sessioninfo.Channel.GetReaderInfo(
                        null,
                        "@displayName:" + strDisplayName,
                        "xml",
                        out results,
                        out strOutputReaderPath,
                        out timestamp,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                    if (lRet == 0 && bHintDisplayName == true)
                    {
                        strBarcode = "";
                        goto SEARCH_COMMENT;
                    }
                    strReaderXml = results[0];

                CONTINUE1:
                    if (nRet == 0)
                        strBarcode = strDisplayName;
                }

            SEARCH_BARCODE:


                if (nRet == 0 && String.IsNullOrEmpty(strBarcode) == false)
                {
                    strReaderXml = "";
                    byte[] timestamp = null;

                    /*
                    // 试探当做读者证条码号检索
                    // 通过读者证条码号获得读者记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = app.GetReaderRecXml(
                sessioninfo.Channels,
                strBarcode,
                out strReaderXml,
                out strOutputReaderPath,
                out timestamp,
                out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        goto SEARCH_COMMENT;
                    }
                     * */
                    string[] results = null;
                    long lRet = temp_sessioninfo.Channel.GetReaderInfo(
                        null,
                        strBarcode,
                        "xml",
                        out results,
                        out strOutputReaderPath,
                        out timestamp,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                    if (lRet == 0)
                    {
                        goto SEARCH_COMMENT;
                    }
                    strReaderXml = results[0];

                }

                if (nRet == 0)
                {
                    /*
                    strError = "读者显示名或者证条码号 '" + strDisplayName + "' 不存在";
                    goto ERROR1;
                     * */
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        strBarcode = strDisplayName;
                    goto SEARCH_COMMENT;
                }

                XmlDocument readerdom = null;
                nRet = OpacApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录 '" + strOutputReaderPath + "' 进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                strDisplayName = DomUtil.GetElementText(readerdom.DocumentElement,
                    "displayName");

                strBarcode = DomUtil.GetElementInnerXml(readerdom.DocumentElement,
                    "barcode");
            }



        SEARCH_COMMENT:

            strText = strDisplayName;
            if (String.IsNullOrEmpty(strText) == true)
                strText = strBarcode;

            this.Label_name.Text = strText;

            string strRecipient = "";
            /*
            if (String.IsNullOrEmpty(strDisplayName) == false)
            {
                if (strDisplayName.IndexOf("[") == -1)
                    strRecipient = "[" + strDisplayName + "]";
                else
                    strRecipient = strDisplayName;
                if (String.IsNullOrEmpty(strEncyptBarcode) == false)
                    strRecipient += " encrypt_barcode:" + strEncyptBarcode;
            }
            else
                strRecipient = strBarcode;
             * */
            strRecipient = BoxesInfo.BuildOneAddress(strDisplayName, strBarcode);


            string strSendMessageUrl = "./message.aspx?recipient=" + HttpUtility.UrlEncode(strRecipient);
            this.Button_sendMessage.OnClientClick = "window.open('" + strSendMessageUrl + "','_blank'); return cancelClick();";

            LoginState loginstate = Global.GetLoginState(this.Page);
            if (loginstate == LoginState.NotLogin || loginstate == LoginState.Public)
                this.Button_sendMessage.Enabled = false;

            this.BrowseSearchResultControl1.Title = strText + " 所发表的书评";

            if (String.IsNullOrEmpty(strEncyptBarcode) == false)
                this.Image_photo.ImageUrl = "./getphoto.aspx?encrypt_barcode=" + HttpUtility.UrlEncode(strEncyptBarcode) + "&displayName=" + HttpUtility.UrlEncode(strDisplayName);
            else
                this.Image_photo.ImageUrl = "./getphoto.aspx?barcode=" + HttpUtility.UrlEncode(strBarcode);

            this.Image_photo.Width = 128;
            this.Image_photo.Height = 128;

            if (this.IsPostBack == false)
            {
                string strXml = "";
                if (String.IsNullOrEmpty(strDisplayName) == false
                    && String.IsNullOrEmpty(strBarcode) == false)
                {
                    // 创建评注记录XML检索式
                    // 用作者和作者显示名共同限定检索
                    nRet = ItemSearchControl.BuildCommentQueryXml(
                        app,
                        strDisplayName,
                        strBarcode,
                        "<全部>",
                        15000,   // app.SearchMaxResultCount
                        true,
                    out strXml,
                    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else if (String.IsNullOrEmpty(strBarcode) == false)
                {
                    // 创建XML检索式
                    nRet = ItemSearchControl.BuildQueryXml(
                        this.app,
                        "comment",
                        strBarcode,
                        "<全部>",
                        "作者",
                        "exact",
                        15000,   // app.SearchMaxResultCount
                        true,
                    out strXml,
                    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else if (String.IsNullOrEmpty(strDisplayName) == false)
                {
                    // 创建XML检索式
                    nRet = ItemSearchControl.BuildQueryXml(
                        this.app,
                        "comment",
                        strDisplayName,
                        "<全部>",
                        "作者显示名",
                        "exact",
                        15000,   // app.SearchMaxResultCount
                        true,
                    out strXml,
                    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    strError = "strBarcode和strDisplayName均为空，无法进行检索";
                    goto ERROR1;
                }

                string strResultSetName = "opac_userinfo";

                sessioninfo.Channel.Idle += new IdleEventHandler(channel_Idle);
                try
                {
                    long lRet = sessioninfo.Channel.Search(
                        null,
                        strXml,
                        strResultSetName,
                        "", // strOutputStyle
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    // not found
                    if (lRet == 0)
                    {
                        this.BrowseSearchResultControl1.Title = strText + " 没有发表过任何书评";
                    }
                    else
                    {
                        this.BrowseSearchResultControl1.ResultSetName = strResultSetName;
                        this.BrowseSearchResultControl1.ResultCount = (int)lRet;
                        this.BrowseSearchResultControl1.StartIndex = 0;
                    }
                    return;
                }
                finally
                {
                    sessioninfo.Channel.Idle -= new IdleEventHandler(channel_Idle);
                }
            }
            return;
        ERROR1:
            Response.Write(HttpUtility.HtmlEncode(strError));
            Response.End();
        }
        finally
        {
            temp_sessioninfo.CloseSession();
        }
    }


    void channel_Idle(object sender, IdleEventArgs e)
    {
        bool bConnected = this.Response.IsClientConnected;

        if (bConnected == false)
        {
            LibraryChannel channel = (LibraryChannel)sender;
            channel.Abort();
        }

        e.bDoEvents = false;
    }

    protected void Button_sendMessage_Click(object sender, EventArgs e)
    {

    }

}