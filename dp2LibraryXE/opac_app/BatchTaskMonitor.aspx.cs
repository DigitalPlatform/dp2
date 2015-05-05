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
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Json;

using DigitalPlatform;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.CirculationClient;

public partial class BatchTaskMonitor : MyWebPage
{
    //OpacApplication app = null;
    //SessionInfo sessioninfo = null;

    Decoder ResultTextDecoder = Encoding.UTF8.GetDecoder();

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

        this.TitleBarControl1.CurrentColumn = TitleColumn.Management;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // Debug.WriteLine("Page_Load()");

        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        string strError = "";

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

        LoginState loginstate = Global.GetLoginState(this.Page);
        if (loginstate != LoginState.Librarian)
        {
            strError = "只有工作人员身份才能使用本模块";
            goto ERROR1;
        }

        string strAction = this.Request["action"];

        string strName = this.Request["name"];
        if (string.IsNullOrEmpty(strName) == true)
            strName = "CacheBuilder";

        this.Label_taskName.Text = strName;

        ResultInfo result_info = new ResultInfo();

        if (strAction == "getinfo")
        {
            string strResultOffset = this.Request["result_offset"];
            string strMaxResultBytes = this.Request["max_result_bytes"];

            if (string.IsNullOrEmpty(strResultOffset) == true)
            {
                result_info.ErrorString = "未指定 result_offset 参数";
                goto END1;
            }
            long lResultOffset = 0;
            Int64.TryParse(strResultOffset, out lResultOffset);
            int nMaxResultBytes = 4096;
            Int32.TryParse(strMaxResultBytes, out nMaxResultBytes);

            BatchTaskInfo param = new BatchTaskInfo();
            BatchTaskInfo info = null;

            param.ResultOffset = lResultOffset;
            param.MaxResultBytes = nMaxResultBytes;

            Debug.WriteLine("GetBatchTaskInfo()");

            int nRet = app.GetBatchTaskInfo(strName,
                param,
                out info,
                out strError);
            if (nRet == -1)
            {
                result_info.ErrorString = strError;
                goto END1;
            }

            result_info.Name = info.Name;
            result_info.MaxResultBytes = info.MaxResultBytes;
            result_info.ResultText = GetResultText(info.ResultText);
            result_info.ProgressText = info.ProgressText;
            result_info.ResultOffset = info.ResultOffset;
            result_info.ResultTotalLength = info.ResultTotalLength;
            result_info.ResultVersion = info.ResultVersion;

        END1:

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ResultInfo));

            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, result_info);
            string strResult = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();

            this.Response.Write(strResult);
            this.Response.End();
            return;
        }

        return;
    ERROR1:
        this.Response.Write(strError);
        this.Response.End();
    }

    // int m_nCount = 0;

    string GetResultText(byte[] baResult)
    {
        if (baResult == null)
            return "";
        if (baResult.Length == 0)
            return "";

        // Decoder ResultTextDecoder = Encoding.UTF8.GetDecoder;
        char[] chars = new char[baResult.Length];

        int nCharCount = this.ResultTextDecoder.GetChars(
            baResult,
                0,
                baResult.Length,
                chars,
                0);
        Debug.Assert(nCharCount <= baResult.Length, "");

        return new string(chars, 0, nCharCount).Replace("\r", "<br/>");
    }

    public class ResultInfo
    {
        // 名字
        public string Name = "";

        // 状态
        public string State = "";

        // 当前进度
        public string ProgressText = "";

        // 输出结果
        public int MaxResultBytes = 0;
        public string ResultText = null;
        public long ResultOffset = 0;   // 本次获得到ResultText达的末尾点
        public long ResultTotalLength = 0;  // 整个结果文件的长度

        public long ResultVersion = 0;  // 信息文件版本

        public string ErrorString = "";
    }
}