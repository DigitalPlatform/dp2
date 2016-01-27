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
using System.Drawing.Imaging;
using System.Drawing;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.Text;

// using DigitalPlatform.CirculationClient;

public partial class GetPhoto : MyWebPage
{
    //OpacApplication app = null;
    //SessionInfo sessioninfo = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        string strError = "";
        int nRet = 0;

        string strBarcode = Request.QueryString["barcode"];
#if NO
        if (strBarcode.IndexOf("@") != -1)
        {
            if (sessioninfo.UserID == strBarcode
                && string.IsNullOrEmpty(sessioninfo.PhotoUrl) == false)
            {
                this.Response.Redirect(this.sessioninfo.PhotoUrl, true);
                return;
            }
        }
#endif

        string strAction = Request.QueryString["action"];

        // 获得一般 QR 图像
        // getphoto.aspx?action=qri&barcode=????????&width=???&height=??? 其中 width 和 height 参数可以缺省
        // 获得读者证号 QR 图像
        // getphoto.aspx?action=pqri&barcode=????????&width=???&height=??? 其中 barcode 参数是读者证条码号。要求当前账户具有 getpatrontempid 的权限，而且读者身份只能获取自己的 tempid
        if (strAction == "qri"
            || strAction == "pqri")
        {
            string strCharset = Request.QueryString["charset"];
            string strWidth = Request.QueryString["width"];
            string strHeight = Request.QueryString["height"];
            string strDisableECI = Request.QueryString["disableECI"];

            bool bDisableECI = false;
            if (string.IsNullOrEmpty(strDisableECI) == false &&
                (strDisableECI.ToLower() == "true"
                || strDisableECI.ToLower() == "yes"
                || strDisableECI.ToLower() == "on"))
                bDisableECI = true;

            int nWidth = 0;

            if (string.IsNullOrEmpty(strWidth) == false)
            {
                if (Int32.TryParse(strWidth, out nWidth) == false)
                {
                    strError = "width 参数 '" + strWidth + "' 格式不合法";
                    goto ERROR1;
                }
            }
            int nHeight = 0;
            if (string.IsNullOrEmpty(strHeight) == false)
            {
                if (Int32.TryParse(strHeight, out nHeight) == false)
                {
                    strError = "height 参数 '" + strHeight + "' 格式不合法";
                    goto ERROR1;
                }
            }

            if (strAction == "qri")
            {
                nRet = app.OutputQrImage(
                    this.Page,
                    strBarcode,
                    strCharset,
                    nWidth,
                    nHeight,
                    bDisableECI,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else if (strAction == "pqri")
            {
                // 读者证号二维码
                string strCode = "";
                // 获得读者证号二维码字符串
                nRet = app.GetPatronTempId(
                    // sessioninfo,
                    strBarcode,
                    out strCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;    // 把出错信息作为图像返回
                nRet = app.OutputQrImage(
                    this.Page,
                    strCode,
                    strCharset,
                    nWidth,
                    nHeight,
                    bDisableECI,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            this.Response.End();
            return;
        }


        string strDisplayName = Request.QueryString["displayName"];
        string strEncyptBarcode = Request.QueryString["encrypt_barcode"];

        // 2012/5/22
        // 较新的用法 userid=xxxx 或者 userid=encrypt:xxxx
        string strUserID = Request.QueryString["userid"];
        if (string.IsNullOrEmpty(strUserID) == false)
        {
            if (StringUtil.HasHead(strUserID, "encrypt:") == true)
            {
                strEncyptBarcode = strUserID.Substring("encrypt:".Length);
            }
            else
            {
                strBarcode = strUserID;
            }
        }

        string strPhotoPath = "";
        // 根据读者证条码号找到头像资源路径
        // return:
        //      -1  出错
        //      0   没有找到。包括读者记录不存在，或者读者记录里面没有头像对象
        //      1   找到
        nRet = app.GetReaderPhotoPath(
            // sessioninfo,
            strBarcode,
            strEncyptBarcode,
            strDisplayName,
            out strPhotoPath,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        if (nRet == 0)
        {
            this.Response.Redirect(MyWebPage.GetStylePath(app, "nonephoto.png"), true);
            return;
        }

        // TODO: HEAD / if-modify-since等精细处理

        // FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

        nRet = app.DownloadObject(
            this,
            null,
            strPhotoPath,
            false,
            "",
            out strError);
        if (nRet == -1)
            goto ERROR1;

        Response.End();
        return;
#if NO
    ERROR1:
        Response.Write(strError);
        Response.End();
        return;
#endif
    ERROR1:
        {
            // 文字图片
            using (MemoryStream image = WebUtil.TextImage(
                ImageFormat.Gif,
                strError,
                Color.Black,
                Color.Yellow,
                10,
                300))
            {
                Page.Response.ContentType = "image/gif";
                this.Response.AddHeader("Content-Length", image.Length.ToString());

                this.Response.AddHeader("Pragma", "no-cache");
                this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
                this.Response.AddHeader("Expires", "0");

                FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

                image.Seek(0, SeekOrigin.Begin);
                StreamUtil.DumpStream(image, Response.OutputStream, flushdelegate);
            }
            Response.Flush();
            Response.End();
        }
    }

    bool MyFlushOutput()
    {
        Response.Flush();
        return Response.IsClientConnected;
    }
}