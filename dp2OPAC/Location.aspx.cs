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
using DigitalPlatform.IO;
using DigitalPlatform.Xml;

using DigitalPlatform.CirculationClient;

public partial class Location : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = TitleColumn.Search;

    }



    protected void Page_Load(object sender, EventArgs e)
    {
        string strError = "";
        if (this.IsPostBack == false)
        {
            // location.aspx?action=getimage&name=xxx
            string strAction = this.Request["action"];
            if (strAction == "getimage")
            {
                string strName = this.Request["name"];
                string strFileName = PathUtil.MergePath(app.DataDir, "location/" + strName);
                DumpImageFile(strFileName,
        out strError);
                this.Response.End();
                return;
            }

            // location.aspx?location=流通库&accessNo=I247.5/1234
            string strLocation = this.Request["location"];
            string strAccessNo = this.Request["accessNo"];

            string strImageFileName = "";
            string strDataCoords = "";
            int nImageWidth = 0;
            int nImageHeight = 0;
            string strDescription = "";

            string strLocationDir = PathUtil.MergePath(app.DataDir, "location");
            if (Directory.Exists(strLocationDir) == false)
            {
                strError = "location 目录尚未配置，无法使用书架定位功能";
                goto ERROR1;
            }

            // parameters:
            //      strImageFileName    返回图像文件名。为纯文件名，不带路径部分
            //      strDataCoords       返回书架顶点坐标
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = ItemsControl.MatchImageFile(
                strLocationDir,
                strLocation,
                strAccessNo,
                out strImageFileName,
                out strDataCoords,
                out nImageWidth,
                out nImageHeight,
                out strDescription,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "馆藏地点 '" + strLocation + "' 分类号 '" + strAccessNo + "' 没有匹配的图像文件";
                goto ERROR1;
            }

            this.Image1.ImageUrl = "./location.aspx?action=getimage&name=" + HttpUtility.UrlEncode(strImageFileName);

            //this.Literal1.Text = "<div class='marker' id='ID' data-coords='-5,-23'>text</div>";
            this.Literal1.Text = "<div class='marker' id='ID' data-coords='" + strDataCoords + "'>" + HttpUtility.HtmlEncode(strDescription) + "</div>";
            this.HiddenField_imageWidth.Value = nImageWidth.ToString();
            this.HiddenField_imageHeight.Value = nImageHeight.ToString();
        }

        return;
    ERROR1:
        this.Response.Write(strError);
        this.Response.End();
    }

    bool MyFlushOutput()
    {
        Response.Flush();
        return Response.IsClientConnected;
    }

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

    // return:
    //      -1  出错
    //      0   成功
    int DumpImageFile(
        string strImageFile,
        out string strError)
    {
        strError = "";

        FileInfo fi = new FileInfo(strImageFile);

        Page page = this;

        DateTime lastmodified = fi.LastWriteTimeUtc;
        string strIfHeader = page.Request.Headers["If-Modified-Since"];

        if (String.IsNullOrEmpty(strIfHeader) == false)
        {
            DateTime isModifiedSince = DateTimeUtil.FromRfc1123DateTimeString(strIfHeader); // .ToLocalTime();

            if (DateTimeUtil.CompareHeaderTime(isModifiedSince, lastmodified) != 0)
            {
                // 修改过
            }
            else
            {
                // 没有修改过
                page.Response.StatusCode = 304;
                page.Response.SuppressContent = true;
                return 0;
            }

        }

        page.Response.AddHeader("Last-Modified", DateTimeUtil.Rfc1123DateTimeString(lastmodified)); // .ToUniversalTime()

        string strContentType = API.MimeTypeFrom(ReadFirst256Bytes(strImageFile),
"");

        page.Response.ContentType = strContentType;

        try
        {

            Stream stream = File.Open(strImageFile,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.ReadWrite);
            try
            {
                page.Response.AddHeader("Content-Length", stream.Length.ToString());

                FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

                stream.Seek(0, SeekOrigin.Begin);

                StreamUtil.DumpStream(stream, page.Response.OutputStream,
                    flushdelegate);
            }
            finally
            {
                stream.Close();
            }
        }
        catch (Exception ex)
        {
            page.Response.ContentType = "text/plain";
            page.Response.StatusCode = 503;
            page.Response.StatusDescription = ex.Message;
            return -1;
        }

        return 0;
    }
}