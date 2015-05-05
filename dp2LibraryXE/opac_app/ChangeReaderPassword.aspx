<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ChangeReaderPassword.aspx.cs"
    Inherits="ChangeReaderPassword" meta:resourcekey="PageResource1" MaintainScrollPositionOnPostBack="true"%>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>
<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>修改密码</title>
    <cc1:LinkControl ID="Linkcontrol1" runat="server" href="head.css" />
    <cc1:LinkControl ID="Linkcontrol2" runat="server" href="changepassword.css" />
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
        <cc1:SideBarControl ID="SideBarControl1" runat="server" />
        <br />
        <!-- 圆角开始 -->
        <div class='content_wrapper'>
            <table class='roundbar' cellpadding='0' cellspacing='0'>
                <tr class='titlebar'>
                    <td class='left'>
                    </td>
                    <td class='middle'>
                        <asp:Label ID="Label5" runat="server" Text="修改密码" meta:resourcekey="Label5Resource1"></asp:Label>
                    </td>
                    <td class='right'>
                    </td>
                </tr>
            </table>
            <table class='changepassword'>
                <tr>
                    <td class='name'>
                        <asp:Label ID="Label1" runat="server" Text="读者证条码号" meta:resourcekey="Label1Resource1"></asp:Label>
                    </td>
                    <td class='value'>
                        <asp:TextBox ID="TextBox_readerBarcode" runat="server" ReadOnly="True" meta:resourcekey="TextBox_readerBarcodeResource1"></asp:TextBox><br />
                    </td>
                </tr>
                <tr>
                    <td class='name'>
                        <asp:Label ID="Label2" runat="server" Text="旧密码" meta:resourcekey="Label2Resource1"></asp:Label>
                    </td>
                    <td class='value'>
                        <asp:TextBox ID="TextBox_oldPassword" runat="server" TextMode="Password" meta:resourcekey="TextBox_oldPasswordResource1"></asp:TextBox><br />
                    </td>
                </tr>
                <tr>
                    <td class='name'>
                        <asp:Label ID="Label3" runat="server" Text="新密码" meta:resourcekey="Label3Resource1"></asp:Label>
                    </td>
                    <td class='value'>
                        <asp:TextBox ID="TextBox_newPassword" runat="server" TextMode="Password" meta:resourcekey="TextBox_newPasswordResource1"></asp:TextBox><br />
                    </td>
                </tr>
                <tr>
                    <td class='name'>
                        <asp:Label ID="Label4" runat="server" Text="再输入一次新密码" meta:resourcekey="Label4Resource1"></asp:Label>
                    </td>
                    <td class='value'>
                        <asp:TextBox ID="TextBox_confirmNewPassword" runat="server" TextMode="Password" meta:resourcekey="TextBox_confirmNewPasswordResource1"></asp:TextBox><br />
                    </td>
                </tr>
                <tr>
                    <td class='name'>
                    </td>
                    <td class='value'>
                        <asp:Button ID="Button_changePassword" runat="server" OnClick="Button_changePassword_Click"
                            Text="修改密码" meta:resourcekey="Button_changePasswordResource1" />
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
