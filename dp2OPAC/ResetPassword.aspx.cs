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

public partial class ResetPassword : MyWebPage  // System.Web.UI.Page
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

        this.TitleBarControl1.CurrentColumn = DigitalPlatform.OPAC.Web.TitleColumn.PersonalInfo;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;
    }

    // 正规化参数值
    static string GetParamValue(string strText)
    {
        return strText.Replace("=", "").Replace(",", "");
    }

    protected void Button_GetTempPassword_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(this.TextBox_name.Text) == true)
        {
            this.Label_message.Text = "请输入姓名。";
            return;
        }

        if (string.IsNullOrEmpty(this.TextBox_readerBarcode.Text) == true)
        {
            this.Label_message.Text = "请输入读者证条码号。";
            return;
        }

        if (string.IsNullOrEmpty(this.TextBox_tel.Text) == true)
        {
            this.Label_message.Text = "请输入手机号码。";
            return;
        }

        if (this.TextBox_tel.Text.Length != 11)
        {
            this.Label_message.Text = "手机号码格式不正确。应该为 11 位数字";
            return;
        }

        string strParameters = "name=" + GetParamValue(this.TextBox_name.Text)
            + ",tel=" + GetParamValue(this.TextBox_tel.Text)
            + ",barcode=" + GetParamValue(this.TextBox_readerBarcode.Text);

        string strError = "";
        long lRet = sessioninfo.Channel.ResetPassword(
            null,
            strParameters,
            "",
            out strError);
        if (lRet != 1)
            goto ERROR1;

        if (string.IsNullOrEmpty(strError) == true)
            this.Label_message.Text = "临时密码已通过短信方式发送到手机 " + this.TextBox_tel.Text + "。请按照手机短信提示进行操作";
        else
            this.Label_message.Text = strError;
        return;
    ERROR1:
        this.Label_message.Text = strError;
    }
}