<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ResetPassword.aspx.cs" 
Inherits="ResetPassword" MaintainScrollPositionOnPostBack="true" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>
<!doctype html>
<html>
<head id="Head1" runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>找回密码</title>
    <cc1:LinkControl ID="Linkcontrol1" runat="server" href="head.css" />
    <cc1:LinkControl ID="Linkcontrol2" runat="server" href="resetpassword.css" />
    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css"
        rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
</head>
<body>
    <script type="text/javascript" language="javascript" src="opac.js"></script>
    <form id="form1" runat="server">
    <div id="frame">
        <cc1:TitleBarControl ID="TitleBarControl1" runat="server" />
        <br />
        <!-- 圆角开始 -->
        <div class='content_wrapper'>
            <table class='roundbar' cellpadding='0' cellspacing='0'>
                <tr class='titlebar'>
                    <td class='left'>
                    </td>
                    <td class='middle'>
                        <asp:Label ID="Label5" runat="server" Text="找回密码" meta:resourcekey="Label5Resource1"></asp:Label>
                    </td>
                    <td class='right'>
                    </td>
                </tr>
            </table>
            <table class='resetpassword'>
                <tr>
                    <td class='name'>
                    </td>
                    <td class='value'>
                        <asp:Label ID="Label_intro" runat="server" Text="找回密码的功能是为指定的帐户分配一个临时密码，用短信方式发送到帐户以前登记的手机号码。然后您可用这个临时密码来登录进入 dp2OPAC 界面，自行修改密码了。请注意临时密码一个小时以后就会失效，需要尽快修改为正式密码" meta:resourcekey="Label1Resource1"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td class='name'>
                        <asp:Label ID="Label1" runat="server" Text="读者证条码号" meta:resourcekey="Label1Resource1"></asp:Label>
                    </td>
                    <td class='value'>
                        <asp:TextBox ID="TextBox_readerBarcode" runat="server" meta:resourcekey="TextBox_readerBarcodeResource1"></asp:TextBox><br />
                    </td>
                </tr>
                <tr>
                    <td class='name'>
                        <asp:Label ID="Label2" runat="server" Text="姓名" meta:resourcekey="Label2Resource1"></asp:Label>
                    </td>
                    <td class='value'>
                        <asp:TextBox ID="TextBox_name" runat="server" meta:resourcekey="TextBox_nameResource1"></asp:TextBox><br />
                    </td>
                </tr>
                <tr>
                    <td class='name'>
                        <asp:Label ID="Label3" runat="server" Text="手机号码" meta:resourcekey="Label3Resource1"></asp:Label>
                    </td>
                    <td class='value'>
                        <asp:TextBox ID="TextBox_tel" runat="server" meta:resourcekey="TextBox_telResource1"></asp:TextBox><br />
                    </td>
                </tr>
                <tr>
                    <td class='name'>
                    </td>
                    <td class='value'>
                        <asp:Button ID="Button_getTempPassword" runat="server" OnClick="Button_GetTempPassword_Click"
                            Text="获得临时密码" meta:resourcekey="Button_getTempPasswordResource1" />
                    </td>
                </tr>
                <tr>
                    <td class='value' colspan='2'>
                        <asp:Label ID="Label_message" runat="server" meta:resourcekey="Label_messageResource1"></asp:Label>
                    </td>
                </tr>
            </table>
            <!-- 圆角结束 -->
        </div>
        <br />
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    </form>
</body>
</html>

