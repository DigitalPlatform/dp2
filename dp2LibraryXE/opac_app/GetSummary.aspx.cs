#define CHANNEL_POOL

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

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

public partial class GetSummary : System.Web.UI.Page // MyWebPage
{
    OpacApplication app = null;
    SessionInfo sessioninfo = null;

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

        string strBarcode = this.Request["barcode"];
        if (string.IsNullOrEmpty(strBarcode) == true)
        {
            strError = "需要指定barcode参数";
            goto ERROR1;
        }

        string strLang = this.Request["lang"];
        if (string.IsNullOrEmpty(strLang) == false)
        {
            //this.UICulture = strLang;
            //this.Culture = strLang;
            try
            {
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.CreateSpecificCulture(strLang);
                Thread.CurrentThread.CurrentUICulture = new
                    CultureInfo(strLang);
            }
            catch
            {
            }

            //this.Session["lang"] = Thread.CurrentThread.CurrentUICulture.Name;
        }

#if NO
        // test
        this.Response.Write("test");
        this.Response.End();
        return;
#endif

        if (StringUtil.HasHead(strBarcode, "biblio_html:") == true)
        {
            string strBiblioRecPath = strBarcode.Substring("biblio_html:".Length);

            // 获得书目记录XML
            string[] formats = new string[1];
            formats[0] = "xml";
            byte[] timestamp = null;

            string[] results = null;

            LibraryChannel channel = null;
#if CHANNEL_POOL
            channel = sessioninfo.GetChannel(true, sessioninfo.Parameters);
#else
            channel = sessioninfo.GetChannel(false);
#endif
            long lRet = channel.GetBiblioInfos(
                null,
                strBiblioRecPath,
                "",
                formats,
                out results,
                out timestamp,
                out strError);
#if CHANNEL_POOL
            sessioninfo.ReturnChannel(channel);
#endif
            if (lRet == -1)
            {
                strError = "获得种记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                goto ERROR1;
            }
            if (results == null || results.Length < 1)
            {
                strError = "results error ";
                goto ERROR1;
            }
            string strBiblioXml = results[0];

            // 创建HTML
            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);

            // 需要从内核映射过来文件
            string strLocalPath = "";
            nRet = app.MapKernelScriptFile(
                null,   // sessioninfo,
                strBiblioDbName,
                "./cfgs/opac_biblio.fltx",  // OPAC查询固定认这个角色的配置文件，作为公共查询书目格式创建的脚本。而流通前端，创建书目格式的时候，找的是loan_biblio.fltx配置文件
                out strLocalPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 将种记录数据从XML格式转换为HTML格式
            string strResult = "";

            KeyValueCollection result_params = null;
            string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
            nRet = app.ConvertBiblioXmlToHtml(
                    strFilterFileName,
                    strBiblioXml,
                    strBiblioRecPath,
                    out strResult,
                    out result_params,
                    out strError);
            if (nRet == -1)
                goto ERROR1;
            this.Response.Write(strResult);
            this.Response.End();
            return;
        }
        else if (StringUtil.HasHead(strBarcode, "formated:") == true)
        {
            strBarcode = strBarcode.Substring("formated:".Length);
            string strArrivedItemBarcode = "";

            string[] barcodes = strBarcode.Split(new char[] { ',' });
            string strTemp = "";
            foreach (string barcode in barcodes)
            {
                if (string.IsNullOrEmpty(barcode) == true)
                    continue;
                if (string.IsNullOrEmpty(strTemp) == false)
                    strTemp += ",";

                if (barcode[0] == '!')
                {
                    strArrivedItemBarcode = barcode.Substring(1);
                    strTemp += strArrivedItemBarcode;
                }
                else
                    strTemp += barcode;
            }

            strBarcode = strTemp;

            string strStyle = "html";  // this.Request["style"];
            string strOtherParams = ""; // this.Request["otherparams"];

            LibraryChannel channel = null;
#if CHANNEL_POOL
            channel = sessioninfo.GetChannel(true);

#else
            channel = sessioninfo.GetChannel(false);
#endif

            // 获得一系列册的摘要字符串
            // 
            // paramters:
            //      strStyle    风格。逗号间隔的列表。如果包含html text表示格式。forcelogin
            //      strOtherParams  <a>命令中其余的参数。例如" target='_blank' "可以用来打开新窗口
            string strResult = ReservationInfoControl.GetBarcodesSummary(
        app,
        channel,
        strBarcode,
        strArrivedItemBarcode,
        strStyle,
        strOtherParams);

#if CHANNEL_POOL
            sessioninfo.ReturnChannel(channel);
#endif
            this.Response.Write(strResult);
            this.Response.End();
            return;
        }
        else
        {
            // 获得摘要
            string strSummary = "";
            string strBiblioRecPath = "";

            LibraryChannel channel = null;
#if CHANNEL_POOL
            channel = sessioninfo.GetChannel(true);

#else
            channel = sessioninfo.GetChannel(false);
#endif

            long lRet = channel.GetBiblioSummary(
                null,
                strBarcode,
                null,
                null,
                out strBiblioRecPath,
                out strSummary,
                out strError);
#if CHANNEL_POOL
            sessioninfo.ReturnChannel(channel);
#endif
            if (lRet == -1 || lRet == 0)
                strSummary = strError;

            this.Response.Write(strSummary);
            this.Response.End();
        }
        return;
    ERROR1:
        this.Response.Write(strError);
        this.Response.End();
    }

    protected void Page_Unload(object sender, EventArgs e)
    {
        if (sessioninfo != null && sessioninfo.Channel != null)
            sessioninfo.Channel.Close();
    }
}