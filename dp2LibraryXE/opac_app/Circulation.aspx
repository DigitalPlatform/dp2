<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Circulation.aspx.cs" Inherits="WebApplication1.Circulation"
    MaintainScrollPositionOnPostBack="true" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>流通业务</title>
    <cc1:LinkControl runat="server" href="head.css" />
    <cc1:LinkControl runat="server" href="circulation.css" />

    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css"
        rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="frame">
            <cc1:TitleBarControl ID="TitleBarControl1" runat="server" />
            <table id="circulation">
                <tr>
                    <td class="patronInfo">
                        <div >patronInfo</div>
                    </td>
                    <td class="taskInfo">
                        <div>
                            <div id="task"></div>
                            
                            <div id="inputPanel">
                                <input id="barcode"/>
                                <button id="enter">输入</button>
                            </div>
                        </div>
                    </td>
                    <td class="operHistory">
                        <div>operHistory</div>
                    </td>
                </tr>
            </table>
            <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
        </div>
    </form>
</body>
</html>
