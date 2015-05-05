<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyBookshelf.aspx.cs" Inherits="MyBookshelf"
MaintainScrollPositionOnPostBack="true" validateRequest="false" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>

<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>我的书架</title>
    <cc1:linkcontrol ID="Linkcontrol1" runat="server" href="head.css" />
    <cc1:linkcontrol ID="Linkcontrol2" runat="server" href="browse.css" />
    <cc1:linkcontrol ID="Linkcontrol3" runat="server" href="biblio.css" />
    <cc1:LinkControl ID="LinkControl4" runat="server" href="marc.css" />
    <cc1:linkcontrol ID="Linkcontrol5" runat="server" href="items.css" />
    <cc1:linkcontrol ID="Linkcontrol6" runat="server" href="comments.css" />
    <cc1:linkcontrol ID="Linkcontrol7" runat="server" href="review.css" />
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
        <cc1:BrowseSearchResultControl ID="BrowseSearchResultControl1" runat="server" />
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    </form>
</body>
</html>
