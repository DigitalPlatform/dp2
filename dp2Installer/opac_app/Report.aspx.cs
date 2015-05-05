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
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;
using System.Diagnostics;
using System.Text;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.dp2.Statis;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;


public partial class Report : MyWebPage
{
    protected void Page_Init(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;


        this.TitleBarControl1.CurrentColumn = TitleColumn.Statis;

        this.SideBarControl1.LayoutStyle = SideBarLayoutStyle.Horizontal;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string strSideBarFile = Server.MapPath("./statis_sidebar.xml");
        if (File.Exists(strSideBarFile) == true)
            this.SideBarControl1.CfgFile = strSideBarFile;
        else
            this.SideBarControl1.Visible = false;

        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

#if NO
        string strError = "";
        int nRet = 0;
#endif
        string strAction = this.Request["action"];

        if (strAction == "keepalive")
        {
            this.Response.Write("OK");
            this.Response.End();
            return;
        }

        // 是否登录?
        if (sessioninfo.UserID == "")
        {
            if (/*this.IsPostBack == true && */strAction == "gettreedata")
            {
#if NO
                this.Response.Write(GetErrorString("请重新登录"));
                this.Response.End();
                return;
#endif
                this.Response.StatusCode = 403;
                this.Response.StatusDescription = "请重新登录";
                this.Response.End();
                return;
            }

            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx?loginstyle=librarian", true);
            return;
        }

        LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

        // 权限是否具备?
        if (loginstate == LoginState.Librarian
    && StringUtil.IsInList("viewreport", sessioninfo.RightsOrigin) == true)
        {
        }
        else
        {
            this.Response.Write("<html><body><p>当前用户不具备 viewreport 权限，无法查看报表</p>"
                + "<p><a href='./searchbiblio.aspx'>返回主页</a></p>"
                + "<p><a href='./login.aspx?loginstyle=librarian&redirect=./report.aspx'>重新登录</a></p>"
                + "</body></html>");
            this.Response.End();
            return;
        }

        string strID = this.Request["id"];
        if (string.IsNullOrEmpty(strID) == false)
        {
            DisplayTreeStruct(strID);
            return;
        }


        string strUrl = this.Request["file"];
        string strNode = this.Request["node"];

        if (string.IsNullOrEmpty(strUrl) == false)
        {
            string strFormat = this.Request["format"];

            GetReportFile(strUrl, strFormat);
            return;
        }

        if (strAction == "gettreedata")
        {
            string strStart = this.Request["node"];    // start

            GetTreeData(strStart);  // 从指定位置开始获得
            return;
        }
    }

    static string BuildJSONString(string value)
    {
        return "\"" + value.Replace(@"\", "/").Replace("\"", "\\\"") + "\"";
    }

    static string GetErrorString(string strText)
    {
        return "[ { \"label\" : " + BuildJSONString(strText) + " } ]";
        // return "[ { \"label\" : \"test\" } ]";

#if NO
        return @"[
    {
        'label': '" + strText.Replace("\"", "").Replace("'", "") + @"',
        'children': [
            {
                'label': 'child1'
            },
            {
                'label': 'child2'
            }
        ]
    },
    {
        'label': 'node2',
        'children': [
            {
                'label': 'child3'
            }
        ]
    }
]".Replace('\'', '"');
#endif
    }

    static string GetDisplayTimeString(string strTime)
    {
        if (IsTimeDirName(strTime) == false)
            return strTime;

        if (strTime.Length == 8)
            return strTime.Substring(0, 4) + "." + strTime.Substring(4, 2) + "." + strTime.Substring(6, 2);
        if (strTime.Length == 6)
            return strTime.Substring(0, 4) + "." + strTime.Substring(4, 2);

        return strTime;
    }

    // 把一个路径头部 ./ 的部分切掉
    static string CutLinkHead(string strLink)
    {
        strLink = strLink.Replace("\\", "/");
        if (StringUtil.HasHead(strLink, "./") == true)
            strLink = strLink.Substring(2);

        return strLink;
    }

    /*
var data = [
    {
        label: 'node1',
        children: [
            { label: 'child1' },
            { label: 'child2' }
        ]
    },
    {
        label: 'node2',
        children: [
            { label: 'child3' }
        ]
    }
];
     * * */
    static void BuildDataString(XmlElement parent,
        string strBaseDir,
        StringBuilder text)
    {
        if (parent.Name == "dir" || parent.Name == "report")
        {
            string strLink = parent.GetAttribute("link");

            string strName = parent.GetAttribute("name");
            if (parent.Name == "dir")
            {
                // strName = GetDisplayTimeString(strName);
            }
            else
                strName = "□ " + strName;
#if NO
            if (Path.GetExtension(strLink) == ".xlsx" == true)
                strName += " (Excel)";
#endif

            text.Append(" { \"label\" : " + BuildJSONString(strName) + " ");


            if (string.IsNullOrEmpty(strLink) == false
                && string.IsNullOrEmpty(strBaseDir) == false)
            {
                strLink = Path.Combine(strBaseDir, CutLinkHead(strLink));
            }

            // .rml 修改为 .html
            if (string.Compare(Path.GetExtension(strLink),".rml", true) == 0)
                strLink = Path.Combine(Path.GetDirectoryName(strLink), Path.GetFileNameWithoutExtension(strLink) + ".html").Replace("\\", "/");

            if (parent.Name == "report")
                text.Append(" , \"url\" : " + BuildJSONString(strLink) + " ");
            if (parent.Name == "dir" && string.IsNullOrEmpty(strLink) == false)
            {
                // TODO: 直接给出能便于找到 131 的 index.xml 的线索路径
                // link=".\201401\table_131"
                // text.Append(" , \"start\" : " + BuildJSONString(strLink));
                text.Append(" , \"id\" : " + BuildJSONString(strLink));
                text.Append(" , \"load_on_demand\" : true ");
            }
        }

        List<XmlElement> children = new List<XmlElement>();

        XmlNodeList nodes = parent.SelectNodes("report");
        foreach (XmlElement node in nodes)
        {
            children.Add(node);
        }

        {
            List<XmlElement> temp = new List<XmlElement>();
            nodes = parent.SelectNodes("dir");
            if (nodes.Count > 0)
            {
                foreach (XmlElement node in nodes)
                {
                    temp.Add(node);
                }
                temp.Reverse();
                children.AddRange(temp);
            }
        }

        if (children.Count > 0)
        {
            if (parent.Name == "dir" || parent.Name == "report")
                text.Append(", \"children\" :");

            if (parent.Name == "dir" || parent.Name == "report")
                text.Append("[ ");
            int i = 0;
            foreach (XmlElement child in children)
            {
                if (i > 0)
                    text.Append(", ");
                BuildDataString(child, strBaseDir, text);
                i++;
            }
            if (parent.Name == "dir" || parent.Name == "report")
                text.Append(" ]");
        }

        if (parent.Name == "dir" || parent.Name == "report")
            text.Append(" } ");

    }

    public static bool GetWindowsMimeType(string ext, out string mime)
    {
        mime = "application/octet-stream";
        Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);

        if (regKey != null)
        {
            object val = regKey.GetValue("Content Type");
            if (val != null)
            {
                string strval = val.ToString();
                if (!(string.IsNullOrEmpty(strval) || string.IsNullOrWhiteSpace(strval)))
                {
                    mime = strval;
                    return true;
                }
            }
        }
        return false;
    }

    // 获得显示用的馆代码形态
    static string GetDisplayLibraryCode(string strLibraryCode)
    {
        if (string.IsNullOrEmpty(strLibraryCode) == true)
            return "[全局]";
        return strLibraryCode;
    }

    // 探测文件是否存在
    // patameters:
    //      strRequestFileName  请求的文件名
    //      strOriginFileName   返回 原始文件名
    //      strRequestExt       返回 请求的文件扩展名 例如 .html
    // return:
    //      -1  出错
    //      1   成功
    int DetectFileName(string strRequestFileName,
        out string strOriginFileName,
        out string strRequestExt,
        out string strError)
    {
        strOriginFileName = "";
        strRequestExt = "";

        strError = "";

        strRequestExt = Path.GetExtension(strRequestFileName);

        if (File.Exists(strRequestFileName) == true)
        {
            strOriginFileName = strRequestFileName;
            return 1;
        }

        strOriginFileName = Path.Combine(Path.GetDirectoryName(strRequestFileName),
            Path.GetFileNameWithoutExtension(strRequestFileName) + ".rml");
        if (File.Exists(strOriginFileName) == false)
        {
            strError = "源文件 '" + strOriginFileName + "' 不存在 ...";
            return -1;
        }

        return 1;
    }

    void GetReportFile(string strUrl, string strFormat)
    {
        string strError = "";

        if (string.IsNullOrEmpty(app.ReportDir) == true)
        {
            strError = "dp2OPAC 尚未配置报表目录";
            goto ERROR1;
        }

        {
            // 检查路径的第一级
            string strFirstLevel = GetFirstLevel(strUrl);

            // 观察当前用户是否具有管辖这个分馆的权限
            if (
                string.IsNullOrEmpty(strFirstLevel) == false &&
                sessioninfo.Channel != null &&
                (
                sessioninfo.GlobalUser == true
                || StringUtil.IsInList(strFirstLevel, sessioninfo.Channel.LibraryCodeList) == true)
                )
            {
            }
            else
            {
                strError = "当前用户不具备查看分馆 '" + strFirstLevel + "' 的报表的权限";
                goto ERROR1;
            }
        }

#if NO
        string strLibraryCode = this.TitleBarControl1.SelectedLibraryCode;
        strLibraryCode = GetDisplayLibraryCode(strLibraryCode);

        string strReportDir = Path.Combine(app.ReportDir, strLibraryCode);
#endif
        string strReportDir = app.ReportDir;

        string strFileName = Path.Combine(strReportDir, CutLinkHead(strUrl));

        if (strFormat == "excel"
            || strFormat == "xslx"
            || strFormat == ".xslx")
        {
            strFileName = Path.Combine(Path.GetDirectoryName(strFileName),
                Path.GetFileNameWithoutExtension(strFileName) + ".xlsx");
        }

        string strOriginFileName = "";
        string strRequestExt = "";

        // 探测文件是否存在
        // patameters:
        //      strRequestFileName  请求的文件名
        //      strOriginFileName   返回 原始文件名
        //      strRequestExt       返回 请求的文件扩展名 例如 .html
        // return:
        //      -1  出错
        //      1   成功
        int nRet = DetectFileName(strFileName,
        out strOriginFileName,
        out strRequestExt,
        out strError);
        if (nRet == -1)
        {
            this.Response.Write(GetErrorString(strError));
            this.Response.End();
            return;
        }

        // 不让浏览器缓存页面
        this.Response.AddHeader("Pragma", "no-cache");
        this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        this.Response.AddHeader("Expires", "0");

        string strMime = "";
        // string strExt = Path.GetExtension(strFileName);
        if (string.Compare(strRequestExt, ".rml", true) == 0)
            strMime = "text/xml";
        else
            GetWindowsMimeType(strRequestExt, out strMime);

        if (string.IsNullOrEmpty(strMime) == true)
            this.Response.ContentType = "text/html";
        else
            this.Response.ContentType = strMime;

        this.Response.AddHeader("Content-Disposition", "inline;filename=" + HttpUtility.UrlEncode(Path.GetFileName(strFileName)));

        string strTempFileName = "";

        if (strFileName != strOriginFileName)
        {
            string strTempDir = Path.Combine(app.DataDir, "temp");
            PathUtil.CreateDirIfNeed(strTempDir);
            strTempFileName = Path.Combine(strTempDir, "~temp_" + Guid.NewGuid().ToString());
            string strCssTemplate = @"BODY {
	FONT-FAMILY: Microsoft YaHei, Verdana, 宋体;
	FONT-SIZE: 10pt;
}

DIV.tabletitle
{
	font-size: 14pt;
	font-weight: bold;
	text-align: center;
	margin: 16pt;

	color: #444444;
}

DIV.titlecomment
{
	text-align: center;
	color: #777777;
}

DIV.createtime
{
	text-align: center;
	color: #777777;

	padding: 4pt;
}

TABLE.table
{
    	font-size: 10pt;
    	/*width: 100%;*/
	margin: auto;
	border-color: #efefef;
	border-width: 16px; 
	border-style: solid;
	border-collapse:collapse;
	background-color: #eeeeee;
}

TABLE.table TR
{

	border-color: #999999;
	border-width: 0px; 
	border-bottom-width: 1px;
	border-style: solid;
} 

TABLE.table THEAD TH 
{
	font-weight: bold;
	white-space:nowrap;
}

TABLE.table TD, TABLE.table TH 
{
	padding: 4pt;
	padding-left: 10pt;
	padding-right: 10pt;
	text-align: left;

}

%columns%

DIV.createtime
{
	text-align: center;
	font-size: 0.8em;
}";

            if (string.Compare(strRequestExt, ".html", true) == 0)
            {
                nRet = DigitalPlatform.dp2.Statis.Report.RmlToHtml(strOriginFileName,
    strTempFileName,
    strCssTemplate,
    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else if (string.Compare(strRequestExt, ".xlsx", true) == 0)
            {
                nRet = DigitalPlatform.dp2.Statis.Report.RmlToExcel(strOriginFileName,
    strTempFileName,
    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            strFileName = strTempFileName;
        }


        Stream stream = File.Open(strFileName,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.ReadWrite);
        try
        {
            this.Response.AddHeader("Content-Length", stream.Length.ToString());

            FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

            stream.Seek(0, SeekOrigin.Begin);

            StreamUtil.DumpStream(stream, this.Response.OutputStream,
                flushdelegate);
        }
        finally
        {
            stream.Close();
        }

        if (string.IsNullOrEmpty(strTempFileName) == false)
        {
            File.Delete(strTempFileName);
        }

        this.Response.OutputStream.Flush();
        this.Response.End();
        return;
    ERROR1:
        this.Response.Write(GetErrorString(strError));
        this.Response.End();
    }

    bool MyFlushOutput()
    {
        Response.Flush();
        return Response.IsClientConnected;
    }

    // 获得路径的第一级
    static string GetFirstLevel(string strPath)
    {
        if (string.IsNullOrEmpty(strPath) == true)
            return "";
        strPath = strPath.Replace("\\", "/");
        string[] parts = strPath.Split(new char[] {'/'});
        if (parts.Length >= 1)
            return parts[0];

        return "";
    }

    #region 验证打包下载功能

    // 显示树结构
    // report.aspx?id=...
    void DisplayTreeStruct(string strStart)
    {
        string strError = "";
        int nRet = 0;

        if (string.IsNullOrEmpty(app.ReportDir) == true)
        {
            this.Response.Write(GetErrorString("dp2OPAC 尚未配置报表目录"));
            this.Response.End();
            return;
        }

        string strLevel = "";   // 层级特征 library 表示分馆层级，需要进行权限筛选
        string strReportDir = app.ReportDir;
        string strBaseDir = ""; // 整个 index.xml 基于的路径
        if (string.IsNullOrEmpty(strStart) == false)
        {
            // 检查路径的第一级
            string strFirstLevel = GetFirstLevel(strStart);

            // 观察当前用户是否具有管辖这个分馆的权限
            if (
                string.IsNullOrEmpty(strFirstLevel) == false &&
                sessioninfo.Channel != null &&
                (
                sessioninfo.GlobalUser == true
                || StringUtil.IsInList(strFirstLevel, sessioninfo.Channel.LibraryCodeList) == true)
                )
            {
            }
            else
            {
                strError = "当前用户不具备查看分馆 '" + strFirstLevel + "' 的报表的权限";
                goto ERROR1;
            }

            strBaseDir = strStart;

            strReportDir = Path.Combine(strReportDir, CutLinkHead(strStart));
        }
        else
        {
            strLevel = "library";
        }

        string strXmlFileName = Path.Combine(strReportDir, "index.xml");


        List<string> paths = new List<string>();
        if (File.Exists(strXmlFileName) == true)
        {

            // string strXmlFileName
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFileName);
            }
            catch (Exception ex)
            {
                strError = "文件 '" + strXmlFileName + "' 装入 DOM 时出错： " + ex.Message;
                goto ERROR1;
            }

            GetPaths(dom.DocumentElement,
                strBaseDir,
                ref paths);
        }

        {
            // 没有 index.xml，直接看目录结构
            GetSubDirPaths(
                strBaseDir,
                strReportDir,
                strLevel,
                ref paths);
        }

        foreach(string line in paths)
        {
            this.Response.Write("<p>" + line + "</p>");
        }

        this.Response.End();
        return;
    ERROR1:
        this.Response.Write(GetErrorString(strError));
        this.Response.End();
    }

    void GetSubDirPaths(
    string strBaseDir,
    string strDir,
    string strLevel,
    ref List<string> paths)
    {
        string strSelectedLibraryCode = this.TitleBarControl1.SelectedLibraryCode;

        // text.Append(" [ ");
        DirectoryInfo di = new DirectoryInfo(strDir);

        DirectoryInfo[] sub_dirs = di.GetDirectories();
        int i = 0;
        foreach (DirectoryInfo sub in sub_dirs)
        {
            if (strLevel == "library")
            {
                // 按照页面右上角选定的馆代码进行筛选
                if (string.IsNullOrEmpty(strSelectedLibraryCode) == false && sub.Name != strSelectedLibraryCode)
                    continue;
                // 观察当前用户是否具有管辖这个分馆的权限
                if (sessioninfo.Channel != null &&
                    (
                    sessioninfo.GlobalUser == true
                    || StringUtil.IsInList(sub.Name, sessioninfo.Channel.LibraryCodeList) == true)
                    )
                {
                }
                else
                    continue;
            }
            else
            {
                // 第一层，也就是分馆名层以外的其他层
                if (IsTimeDirName(sub.Name) == true)
                {
                }
                else
                    continue;
            }

            string strLine = "";

            string strLink = sub.Name;

            string strName = sub.Name;

            if (strLevel != "library")
                strName = "♫ " + GetDisplayTimeString(strName);

            strLine = "name: " + strName + " ";

            if (string.IsNullOrEmpty(strBaseDir) == false)
            {
                strLink = Path.Combine(strBaseDir, CutLinkHead(strLink));
            }

            // text.Append(" , \"start\" : " + BuildJSONString(strLink));

            strLine += " dir-id: " + strLink;
            paths.Add(strLine);
        }
    }

    static void GetPaths(XmlElement parent,
        string strBaseDir,
        ref List<string> paths)
    {
        if (parent.Name == "dir" || parent.Name == "report")
        {
            string strLine = "";

            string strLink = parent.GetAttribute("link");

            string strName = parent.GetAttribute("name");
            if (parent.Name == "dir")
            {
                // strName = GetDisplayTimeString(strName);
            }
            else
                strName = "□ " + strName;

            strLine = "name: " + strName + " ";

            if (string.IsNullOrEmpty(strLink) == false
                && string.IsNullOrEmpty(strBaseDir) == false)
            {
                strLink = Path.Combine(strBaseDir, CutLinkHead(strLink));
            }

#if NO
            // .rml 修改为 .html
            if (string.Compare(Path.GetExtension(strLink), ".rml", true) == 0)
                strLink = Path.Combine(Path.GetDirectoryName(strLink), Path.GetFileNameWithoutExtension(strLink) + ".html").Replace("\\", "/");
#endif

            if (parent.Name == "report")
                strLine += " url: " + strLink + " ";
            if (parent.Name == "dir" && string.IsNullOrEmpty(strLink) == false)
            {
                // TODO: 直接给出能便于找到 131 的 index.xml 的线索路径
                // link=".\201401\table_131"
                // text.Append(" , \"start\" : " + BuildJSONString(strLink));
                strLine += " dir-id: " + strLink;
            }

            paths.Add(strLine);
        }

        List<XmlElement> children = new List<XmlElement>();

        XmlNodeList nodes = parent.SelectNodes("report");
        foreach (XmlElement node in nodes)
        {
            children.Add(node);
        }

        {
            List<XmlElement> temp = new List<XmlElement>();
            nodes = parent.SelectNodes("dir");
            if (nodes.Count > 0)
            {
                foreach (XmlElement node in nodes)
                {
                    temp.Add(node);
                }
                temp.Reverse();
                children.AddRange(temp);
            }
        }

        if (children.Count > 0)
        {
            foreach (XmlElement child in children)
            {
                List<string> temp = new List<string>();
                GetPaths(child, strBaseDir, ref temp);

                paths.AddRange(temp);
            }

        }
    }

    #endregion

    // 获得树内容
    // ajax请求获得栏目内容
    // report.aspx?action=gettreedata
    void GetTreeData(string strStart)
    {
        string strError = "";
        int nRet = 0;

        this.Response.ContentEncoding = Encoding.UTF8;
        this.Response.ContentType = "application/json; charset=utf-8";

        if (string.IsNullOrEmpty(app.ReportDir) == true)
        {
            this.Response.Write(GetErrorString("dp2OPAC 尚未配置报表目录"));
            this.Response.End();
            return;
        }

#if NO
        string strLibraryCode = this.TitleBarControl1.SelectedLibraryCode;

        // 观察当前用户是否具有管辖这个分馆的权限
        if (sessioninfo.Channel != null &&
            (
            sessioninfo.GlobalUser == true
            || StringUtil.IsInList(strLibraryCode, sessioninfo.Channel.LibraryCodeList) == true)
            )
        {
        }
        else
        {
            strError = "当前用户不具备查看分馆 '"+strLibraryCode+"' 的报表的权限";
            goto ERROR1;
        }

        strLibraryCode = GetDisplayLibraryCode(strLibraryCode);
        string strReportDir = Path.Combine(app.ReportDir, strLibraryCode);
#endif
        string strLevel = "";   // 层级特征 library 表示分馆层级，需要进行权限筛选

        string strReportDir = app.ReportDir;

        string strBaseDir = ""; // 整个 index.xml 基于的路径
        if (string.IsNullOrEmpty(strStart) == false)
        {
            // 检查路径的第一级
            string strFirstLevel = GetFirstLevel(strStart);

            // 观察当前用户是否具有管辖这个分馆的权限
            if (
                string.IsNullOrEmpty(strFirstLevel) == false &&
                sessioninfo.Channel != null &&
                (
                sessioninfo.GlobalUser == true
                || StringUtil.IsInList(strFirstLevel, sessioninfo.Channel.LibraryCodeList) == true)
                )
            {
            }
            else
            {
                strError = "当前用户不具备查看分馆 '" + strFirstLevel + "' 的报表的权限";
                goto ERROR1;
            }

            strBaseDir = strStart;

#if NO
            strStart = strStart.Replace("\\", "/");
            if (StringUtil.HasHead(strStart, "./") == true)
                strStart = strStart.Substring(2);
#endif

            strReportDir = Path.Combine(strReportDir, CutLinkHead(strStart));

        }
        else
        {
            strLevel = "library";
        }

        string strXmlFileName = Path.Combine(strReportDir, "index.xml");

        StringBuilder text = new StringBuilder(4096);

        if (File.Exists(strXmlFileName) == true)
        {

            // string strXmlFileName
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFileName);
            }
            catch (Exception ex)
            {
                strError = "文件 '" + strXmlFileName + "' 装入 DOM 时出错： " + ex.Message;
                goto ERROR1;
            }


            BuildDataString(dom.DocumentElement,
                strBaseDir,
                text);
        }

            StringBuilder text1 = new StringBuilder(4096);
        {
            // 没有 index.xml，直接看目录结构
            ListSubDir(
                strBaseDir,
                strReportDir,
                strLevel,
                text1);
        }

        if (text.Length > 0 && text1.Length > 0)
            text.Append(" , ");

        text.Append(text1);

        text.Insert(0, "[ ");
        text.Append(" ]");

        this.Response.Write(text.ToString());
        this.Response.End();
        return;
    ERROR1:
        this.Response.Write(GetErrorString(strError));
        this.Response.End();
    }

    static bool IsTimeDirName(string strName)
    {
        if (string.IsNullOrEmpty(strName) == true)
            return false;

        if ((strName.Length == 4 || strName.Length == 6 || strName.Length == 8)
            && StringUtil.IsPureNumber(strName) == true)
            return true;

        if (strName.Length == 17)
        {
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strName,
                "-",
                out strLeft,
                out strRight);

            if (strLeft.Length == 8
                && strRight.Length == 8
                && StringUtil.IsPureNumber(strLeft) == true
                && StringUtil.IsPureNumber(strRight) == true)
                return true;
        }

        return false;
    }

    void ListSubDir(
        string strBaseDir,
        string strDir,
        string strLevel,
        StringBuilder text)
    {
        string strSelectedLibraryCode = this.TitleBarControl1.SelectedLibraryCode;

        // text.Append(" [ ");
        DirectoryInfo di = new DirectoryInfo(strDir);

        DirectoryInfo[] sub_dirs = di.GetDirectories();
        int i = 0;
        foreach (DirectoryInfo sub in sub_dirs)
        {
            if (strLevel == "library")
            {
                // 按照页面右上角选定的馆代码进行筛选
                if (string.IsNullOrEmpty(strSelectedLibraryCode) == false && sub.Name != strSelectedLibraryCode)
                    continue;
                // 观察当前用户是否具有管辖这个分馆的权限
                if (sessioninfo.Channel != null &&
                    (
                    sessioninfo.GlobalUser == true
                    || StringUtil.IsInList(sub.Name, sessioninfo.Channel.LibraryCodeList) == true)
                    )
                {
                }
                else 
                    continue;
            }
            else
            {
                // 第一层，也就是分馆名层以外的其他层
                if (IsTimeDirName(sub.Name) == true)
                {
                }
                else
                    continue;
            }



            if (i > 0)
                text.Append(", ");

            string strLink = sub.Name;

            string strName = sub.Name;

            if (strLevel != "library")
                strName = "♫ " + GetDisplayTimeString(strName);

            text.Append(" { \"label\" : " + BuildJSONString(strName) + " ");

            if (string.IsNullOrEmpty(strBaseDir) == false)
            {
                strLink = Path.Combine(strBaseDir, CutLinkHead(strLink));
            }

            // text.Append(" , \"start\" : " + BuildJSONString(strLink));

            text.Append(" , \"id\" : " + BuildJSONString(strLink));
            text.Append(" , \"load_on_demand\" : true ");


            text.Append(" } ");
            i++;
        }

        // text.Append(" ] ");
    }

}