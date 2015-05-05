<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>

<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>登录</title>
    <cc1:linkcontrol ID="Linkcontrol1" runat="server" href="head.css" />
    <cc1:linkcontrol ID="Linkcontrol2" runat="server" href="login.css" />
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
        <cc1:LoginControl ID="LoginControl1" runat="server" 
            onlogin="LoginControl1_Login" OnAnonymouseLogin="LoginControl1_AnonymouseLogin"/>
                <br />
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    </form>
</body>
</html>
