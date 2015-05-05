<%@ Page Language="C#" AutoEventWireup="true" Inherits="Book" 
MaintainScrollPositionOnPostBack="true" validateRequest="false" Codebehind="Book.aspx.cs" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>

<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>图书详细信息</title>
    <cc1:LinkControl runat="server" href="head.css" />
    <cc1:LinkControl runat="server" href="biblio.css" />
    <cc1:LinkControl runat="server" href="marc.css" />
    <cc1:LinkControl runat="server" href="items.css" />
    <cc1:LinkControl runat="server" href="comments.css" />
    <cc1:LinkControl runat="server" href="review.css" />
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
        <cc1:BiblioControl ID="BiblioControl1" runat="server" />
        <cc1:ItemsControl ID="ItemsControl1" runat="server" />
        <cc1:CommentsControl ID="CommentsControl1" runat="server" />
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    </form>
</body>
</html>
