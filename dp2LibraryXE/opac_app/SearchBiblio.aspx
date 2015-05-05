<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SearchBiblio.aspx.cs" Inherits="SearchBiblio"
 MaintainScrollPositionOnPostBack="true" validateRequest="false" %>


<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>
<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>检索</title>
    <cc1:linkcontrol runat="server" href="head.css" />
    <cc1:linkcontrol runat="server" href="browse.css" />
    <cc1:linkcontrol runat="server" href="biblio.css" />
    <cc1:LinkControl runat="server" href="marc.css" />
    <cc1:linkcontrol runat="server" href="items.css" />
    <cc1:linkcontrol runat="server" href="query.css" />
    <cc1:linkcontrol runat="server" href="comments.css" />
    <cc1:linkcontrol runat="server" href="review.css" />
    <cc1:LinkControl runat="server" href="searchbiblioaspx.css" />

    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css"
        rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>

    <link type="text/css" href="./jslider/css/jquery.slider.min.css" rel="stylesheet" />
    <script type="text/javascript" src="./jslider/js/jquery.slider.min.js"></script>

</head>
<body>
	<script type="text/javascript" language="javascript" src="opac.js"></script>
    <script type="text/javascript" language="javascript" src="searchbiblio.js"></script>

    <form id="form1" runat="server">
    <div id="frame">
        <cc1:TitleBarControl ID="TitleBarControl1" runat="server" 
            onrefreshing="TitleBarControl1_Refreshing" />
        <cc1:SideBarControl ID="SideBarControl1" runat="server" />
        <cc1:SearchFilterControl ID="filter" runat="server" 
            ontreeitemclick="filter_TreeItemClick"/>
        <cc1:BrowseSearchResultControl ID="BrowseSearchResultControl1" runat="server" class="normal"/>
        <cc1:BiblioSearchControl ID="BiblioSearchControl1" runat="server" 
            onsearch="BiblioSearchControl1_Search" />
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    </form>
</body>
</html>

