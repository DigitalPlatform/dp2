<%@ Page Language="C#" AutoEventWireup="true" Inherits="UserInfo"
 MaintainScrollPositionOnPostBack="true" validateRequest="false" Codebehind="UserInfo.aspx.cs" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>
<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>用户信息</title>
    <cc1:LinkControl ID="Linkcontrol1" runat="server" href="head.css" />
    <cc1:LinkControl ID="Linkcontrol2" runat="server" href="browse.css" />
    <cc1:LinkControl ID="Linkcontrol3" runat="server" href="biblio.css" />
    <cc1:LinkControl ID="Linkcontrol4" runat="server" href="marc.css" />
    <cc1:LinkControl ID="Linkcontrol5" runat="server" href="items.css" />
    <cc1:LinkControl ID="Linkcontrol6" runat="server" href="comments.css" />
    <cc1:LinkControl ID="Linkcontrol8" runat="server" href="review.css" />
    <cc1:LinkControl ID="Linkcontrol7" runat="server" href="userinfoaspx.css" />
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
        <table class='total'>
            <tr>
                <td class='left' align='left'>
                    <div class='photo_back'>
                        <asp:Image ID="Image_photo" runat="server" />
                        <div class='user_text'>
                            <asp:Label ID="Label_name" runat="server" Text="Label"></asp:Label>
                        </div>
                        <div>
                            <asp:Button ID="Button_sendMessage" runat="server" Text="发送消息" OnClick="Button_sendMessage_Click" />
                        </div>
                    </div>
                </td>
                <td class='middle'>
                </td>
                <td class='right' align='left'>
                    <cc1:BrowseSearchResultControl ID="BrowseSearchResultControl1" runat="server" />
                </td>
            </tr>
        </table>
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    </form>
</body>
</html>
