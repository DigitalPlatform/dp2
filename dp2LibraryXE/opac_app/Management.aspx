<%@ Page Language="C#" AutoEventWireup="true" Inherits="Management"
MaintainScrollPositionOnPostBack="true" validateRequest="false" Codebehind="Management.aspx.cs" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>

<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>管理</title>
    <cc1:linkcontrol ID="Linkcontrol1" runat="server" href="head.css" />
    <cc1:LinkControl ID="LinkControl9" runat="server" href="management.css" />
    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css" rel="stylesheet"
        type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
</head>
<body>
    <script type="text/javascript" language="javascript" src="opac.js"></script>
    <form id="form1" runat="server">
    <div id="frame">
        <cc1:TitleBarControl ID="TitleBarControl1" runat="server" />
        <asp:Button ID="Button_refreshCfg" runat="server" Text="刷新配置" OnClick="Button_refreshCfg_Click" />
        <br />
        <a href="./batchtaskmonitor.aspx">批处理任务监控</a>
        <br />
        <a href="./management.aspx?action=geterrorlog">观看今日的应用日志</a>
        <br />
        <a href="./management.aspx?action=geteventlog">观看Windows Event Log 日志</a>
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    </form>
</body>
</html>
