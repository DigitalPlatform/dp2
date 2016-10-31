// #define DUMP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.Drawing;
using DigitalPlatform.OPAC.Server;

public partial class css : MyWebPage
{
    protected void Page_Init(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
#if NO
        string strFileName = this.Request["name"];
        string strLibraryCode = this.Request["librarycode"];
        string strStyle = this.Request["style"];

        // string strAppDir = Server.MapPath("~/style/0");
        string strAppDir = Path.Combine(app.DataDir, "style");
        string strFilePath = Path.Combine(strAppDir, strFileName);

#endif
        string strError = "";
        int nRet = 0;

        string strLibraryCode = "";
        if (this.RouteData.Values.ContainsKey("librarycode"))
            strLibraryCode = this.RouteData.Values["librarycode"].ToString();

        string strStyle = "";
        if (this.RouteData.Values.ContainsKey("style"))
            strStyle = this.RouteData.Values["style"].ToString();

        string strFileName = "";
        if (this.RouteData.Values.ContainsKey("filename"))
            strFileName = this.RouteData.Values["filename"].ToString();
#if NO
        if (string.IsNullOrEmpty(strStyle) == true)
            strStyle = "0";
#endif

        string strAppDir = Path.Combine(app.DataDir, "style");
        if (string.IsNullOrEmpty(strLibraryCode) == true
            && string.IsNullOrEmpty(strStyle) == false)
            strAppDir = Path.Combine(strAppDir, strStyle);
        else
            strAppDir = Path.Combine(strAppDir, strStyle);    // "style/" + strLibraryCode + "/" + strStyle
        string strFilePath = Path.Combine(strAppDir, strFileName);

        LibraryInfo info = app.GetLibraryInfo(strLibraryCode);

#if NO
            {
                this.Response.ContentType = "text/html; charset=utf-8";
                this.Response.Write("test");
                this.Response.End();
                return;
            }
#endif

        string strExt = Path.GetExtension(strFileName);
        if (string.Compare(strExt, ".css", true) == 0)
        {
            if (info == null
                && string.IsNullOrEmpty(strLibraryCode) == false)
                goto DUMP_FILE;

            string strContent = "";

            // 构造 CSS 文件内容
            // 如果 webui.xml 中没有影射关系，则最后找物理文件
            // return:
            //      -1  出错
            //      0   在 webui.xml 中没有找到映射关系
            //      1   找到了映射关系，并获得了温家内容在 strContent 中
            //      2   .css 文件已经存在。 strContent 中返回这个 .css 文件的物理路径
            nRet = app.BuildCssContent(strLibraryCode,
                strStyle,
                strFileName,
                out strContent,
                out strFilePath,
                out strError);
            if (nRet == 0)
                goto DUMP_FILE;
            if (nRet == 2)
            {
                goto DUMP_FILE;
            }
            if (nRet == -1)
            {
                if (this.Response.IsClientConnected == false)
                    return;
                this.Response.ContentType = "text/html; charset=utf-8";
                this.Response.StatusCode = 500; // 500;
                this.Response.StatusDescription = strError;
                this.Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p></body></html>");
                // this.Response.Flush();
                this.Response.End();
                return;
            }

            this.Response.ContentType = "text/css; charset=utf-8";
            FileInfo fi = new FileInfo(strFilePath);
            DateTime lastmodified = fi.LastWriteTimeUtc;

            this.Response.AddHeader("Last-Modified", DateTimeUtil.Rfc1123DateTimeString(lastmodified)); // .ToUniversalTime()

            this.Response.Write(strContent);
            this.Response.End();
            return;
        }
        else
        {
            // 不是 .css 其他类型文件
#if DUMP
                app.WriteErrorLog("1 strFileName ["+strFileName+"] strLibraryCode ["+strLibraryCode+"]");
#endif

            if (strFileName == "title_logo.gif"
                && string.IsNullOrEmpty(strLibraryCode) == false)
            {
                // 1) 看看 logoText 属性
                string strLogoText = "";
                if (info != null)
                {
                    strLogoText = info.LogoText;
                    if (string.IsNullOrEmpty(strLogoText) == false)
                    {
                        OutputLogo(strFilePath,
                             strLogoText);
                        return;
                    }
                }

#if DUMP
                    app.WriteErrorLog("2 info.StyleName [" + info.StyleName + "]");
#endif
                // 2) 看看 style/haidian 目录中的 title_logo.gif 文件是否已经存在
                if (string.IsNullOrEmpty(info.StyleName) == false)
                {
                    string strPhysicalPath = Path.Combine(app.DataDir, "style/" + info.StyleName + "/title_logo.gif");
                    if (File.Exists(strPhysicalPath) == true)
                    {
                        strFilePath = strPhysicalPath;
                        goto DUMP_FILE;
                    }
                }

#if DUMP
                    app.WriteErrorLog("3 strLogoText [" + strLogoText + "]");
#endif

                // 3) 把 code 属性当作文字显示
                if (string.IsNullOrEmpty(strLogoText) == true)
                {
                    strLogoText = info.LibraryCode;
                    if (string.IsNullOrEmpty(strLogoText) == false)
                    {
                        OutputLogo(strFilePath,
                             strLogoText);
                        return;
                    }
                }
            }

        }

    DUMP_FILE:

        string strMime = "";
        PathUtil.GetWindowsMimeType(strExt, out strMime);

        nRet = DumpFile(
    strFilePath,
    strMime,
    out strError);
        if (nRet == 0)
        {
            if (this.Response.IsClientConnected == false)
                return;
            // TODO: 要根据文件扩展名得到媒体类型

            this.Response.ContentType = "text/html";
            this.Response.StatusCode = 404; // 404
            this.Response.StatusDescription = strError;
            this.Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p></body></html>");
            // this.Response.Flush();
            this.Response.End();
            return;
        }
        if (nRet == -1)
        {
            if (this.Response.IsClientConnected == false)
                return;
            this.Response.ContentType = "text/html";
            this.Response.StatusCode = 500; // 500
            this.Response.StatusDescription = strError;
            this.Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p></body></html>");
            // this.Response.Flush();
            this.Response.End();
            return;
        }

        // this.Response.Flush();
        this.Response.End();


#if NO
        string strAppDir = "";
        
        if (string.IsNullOrEmpty(strLibraryCode) == true)
            strAppDir = Server.MapPath("~/style/" + strStyle);
        else
            strAppDir = Server.MapPath("~/style/" + strLibraryCode + "/" + strStyle);

        string strFilePath = Path.Combine(strAppDir, strFileName);

        string strError = "";
        int nRet = DumpFile(
    strFilePath,
    "text/css; charset=utf-8",
    out strError);
        if (nRet == 0)
        {
            if (this.Response.IsClientConnected == false)
                return; 
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
            if (this.Response.IsClientConnected == false)
                return;
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
#endif

    }

    void OutputLogo(string strFileName,
        string strText)
    {
        FileInfo fi = new FileInfo(strFileName);
        DateTime lastmodified = fi.LastWriteTimeUtc;

        this.Response.AddHeader("Last-Modified", DateTimeUtil.Rfc1123DateTimeString(lastmodified)); // .ToUniversalTime()

        TextInfo info = new TextInfo();
        info.FontFace = "Microsoft YaHei";
        info.FontSize = 10;
        info.colorText = Color.Gray;

        // 文字图片
        using (MemoryStream image = ArtText.PaintText(
            strFileName,
            strText,
            info,
            "center",
            "100%",
            "100%",
            "65%",
            ImageFormat.Jpeg))
        {
            this.Response.ContentType = "image/jpeg";
            this.Response.AddHeader("Content-Length", image.Length.ToString());

            //this.Response.AddHeader("Pragma", "no-cache");
            //this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
            //this.Response.AddHeader("Expires", "0");

            FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);
            image.Seek(0, SeekOrigin.Begin);
            StreamUtil.DumpStream(image, Response.OutputStream, flushdelegate);
        }
        // Response.Flush();
        Response.End();
    }

    // return:
    //      -1  出错
    //      0   成功
    //      1   暂时不能访问
    int DumpFile(string strFilename,
        string strContentType,
        out string strError)
    {
        strError = "";

#if NO
        // 不让浏览器缓存页面
        this.Response.AddHeader("Pragma", "no-cache");
        this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        this.Response.AddHeader("Expires", "0");
#endif
        this.Response.ContentType = strContentType;

        FileInfo fi = new FileInfo(strFilename);
        DateTime lastmodified = fi.LastWriteTimeUtc;

        this.Response.AddHeader("Last-Modified", DateTimeUtil.Rfc1123DateTimeString(lastmodified)); // .ToUniversalTime()

        try
        {
            using (Stream stream = File.Open(strFilename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))   // 2015/1/12
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

#if NO
    bool MyFlushOutput()
    {
        Response.Flush();
        return Response.IsClientConnected;
    }
#endif
}