<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Browse.aspx.cs" Inherits="Browse2" 
 MaintainScrollPositionOnPostBack="true" validateRequest="false" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>

<!doctype html>
<html>
<head id="Head1" runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>浏览</title>
    <link href="./jquerytreeview/jquery.treeview.css"
        rel="stylesheet" type="text/css" />
    <cc1:LinkControl ID="Linkcontrol1" runat="server" href="head.css" />
    <cc1:LinkControl ID="Linkcontrol2" runat="server" href="browse.css" />
    <cc1:LinkControl ID="Linkcontrol3" runat="server" href="biblio.css" />
    <cc1:LinkControl ID="Linkcontrol4" runat="server" href="marc.css" />
    <cc1:LinkControl ID="Linkcontrol5" runat="server" href="items.css" />
    <cc1:LinkControl ID="Linkcontrol6" runat="server" href="comments.css" />
    <cc1:LinkControl ID="Linkcontrol8" runat="server" href="review.css" />
    <cc1:LinkControl ID="Linkcontrol7" runat="server" href="browseaspx.css" />

    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css"
        rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
    <script type="text/javascript" src="./jquerytreeview/jquery.treeview.js"></script>
</head>
<body>

    <script type="text/javascript" language="javascript" src="opac.js"></script>
    <script language="javascript" type="text/javascript">
        var treetop = 0;
        $(window).load(function () {
            $(window).scroll(function () {
                if ($(window).scrollTop() > treetop - 30 ) {
                    $('#tree').css('position', 'fixed');
                    $('#tree').css('top', 0);
                    $('#TreeView1').css('height', $(window).height() - 80);
                } else {
                    $('#tree').css('position', 'relative');
                    $('#tree').css('top', 0);
                    $('#TreeView1').css('height', $(window).height() - treetop - 50);
                }
            });
        });
        $(document).ready(function () {
            $("#TreeView1").treeview();
            window.setTimeout(function () {
                ScrollIntoView($("UL#TreeView1"), $(".selected"));
            }, 1000);
            treetop = $('#TreeView1').offset().top;
            $('#TreeView1').css('height', $(window).height() - treetop - 50);
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
            <table class='total'>
                <tr>
                    <td class='left' align='left'>
                        <div class='commands'>
                            <asp:Button ID="ButtonRefreshAllCache" runat="server" Text="刷新全部缓存" meta:resourcekey="ButtonRefreshAllCache"
                                OnClick="ButtonRefreshAllCache_Click"></asp:Button>
                            <asp:Button ID="ButtonAppendAllCache" runat="server" meta:resourcekey="ButtonAppendAllCache"
                                Text="增补全部缓存" OnClick="ButtonAppendAllCache_Click" />
                        </div>
                        <div id="fixedwidth"></div>
                        <div id='tree' class='content_wrapper'>
                            <cc1:PanelControl ID="PanelControl1" runat="server"></cc1:PanelControl>


                            <cc1:TreeControl ID="TreeView1" runat="server" 
                                ongetnodedata="TreeView1_GetNodeData" />
                        </div>
                    </td>
                    <td class='middle'>
                    </td>
                    <td class='right' align='left'>
                        <div class='commands'>
                            <asp:Button ID="ButtonRefreshCache" runat="server" Text="刷新缓存" meta:resourcekey="ButtonRefreshCache"
                                OnClick="ButtonRefreshCache_Click" />
                            <asp:Literal ID="ErrorInfo" runat="server" Visible="False"></asp:Literal>
                        </div>
                        <div class='description'>
                            <asp:Literal ID="Description1" runat="server"></asp:Literal>
                        </div>
                        <asp:HiddenField ID="BrowseDataFileName" runat="server" />
                        <div class='right'>
                            <cc1:BrowseSearchResultControl ID="BrowseSearchResultControl1" runat="server"></cc1:BrowseSearchResultControl>
                        </div>
                    </td>
                </tr>
            </table>
            <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
        </div>
    </form>
</body>
</html>

