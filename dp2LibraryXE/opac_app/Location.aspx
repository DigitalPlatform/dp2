<%@ Page Language="C#" AutoEventWireup="true" Inherits="Location" 
MaintainScrollPositionOnPostBack="true" validateRequest="false" Codebehind="Location.aspx.cs" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>
<!doctype html>
<html>
<head id="Head1" runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>检索</title>
    <cc1:linkcontrol ID="Linkcontrol1" runat="server" href="head.css" />
    <cc1:LinkControl ID="LinkControl9" runat="server" href="location.css" />

    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css"
        rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
</head>
<body>
    <script type="text/javascript" language="javascript" src="./craftmap/js/craftmap.js"></script>
	<script type="text/javascript" language="javascript" src="opac.js"></script>
	<script type="text/javascript" language="javascript" src="location.js"></script>

    <form id="form1" runat="server">
    <div id="frame">
        <cc1:TitleBarControl ID="TitleBarControl1" runat="server" />
        <div class="map" id="mapFrame">
            <asp:Image ID="Image1" runat="server" class="imgMap"/>
            <asp:Literal ID="Literal1" runat="server"></asp:Literal>
        </div>
        <asp:HiddenField ID="HiddenField_imageWidth" runat="server" />
        <asp:HiddenField ID="HiddenField_imageHeight" runat="server" />
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    </form>
</body>
</html>