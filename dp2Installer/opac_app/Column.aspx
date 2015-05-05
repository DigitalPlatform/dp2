<%@ Page Language="C#" AutoEventWireup="true" Inherits="Column"
MaintainScrollPositionOnPostback="true" ValidateRequest="false" Codebehind="Column.aspx.cs" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>
<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>最新评注</title>
    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css"
        rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
    <cc1:LinkControl ID="LinkControl1" runat="server" href="head.css" />
    <cc1:linkcontrol ID="Linkcontrol3" runat="server" href="biblio.css" />
    <cc1:LinkControl ID="LinkControl2" runat="server" href="column.css" />
    <cc1:linkcontrol ID="Linkcontrol4" runat="server" href="review.css" />
</head>
<body>
    <script type="text/javascript" language="javascript" src="opac.js"></script>
    <script type="text/javascript" language="javascript">
        $(document).ready(function () {
            PopTooltips($(".guestbooktips"), "tooltip_chat");
        });
    </script>
    <form id="form1" runat="server">
    <div id="frame">
        <cc1:TitleBarControl ID="TitleBarControl1" runat="server" />
        <table class='cmdbar'>
            <tr>
                <td align='left'>
                    <cc1:SideBarControl ID="SideBarControl1" runat="server" />
                </td>
            </tr>
        </table>
        <cc1:ColumnControl ID="ColumnControl1" runat="server" />
        <br />
        <asp:Button ID="Button_createColumnStorage" runat="server" Text="创建栏目缓存" OnClick="Button_createColumnStorage_Click" />
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    </form>
</body>
</html>
