<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Report.aspx.cs" Inherits="Report" 
MaintainScrollPositionOnPostBack="true" validateRequest="false" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>

<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>报表</title>
    <link href="./jqtree/jqtree.css" rel="stylesheet" type="text/css" />
    <link href="./jqtree/bootstrap.min.css" rel="stylesheet" type="text/css" />

    <cc1:LinkControl ID="Linkcontrol1" runat="server" href="head.css" />
    <cc1:LinkControl ID="Linkcontrol2" runat="server" href="report.css" />

    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css"
        rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
    <script type="text/javascript" language="javascript" src="./jqtree/tree.jquery.js"></script>
    <script type="text/javascript" language="javascript" src="./jqtree/jqtreecontextmenu.js"></script>
        <script type="text/javascript" language="javascript" src="opac.js"></script>
    <script type="text/javascript" language="javascript" src="report.js"></script>

</head>
<body>

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
                        <div id="fixedwidth"></div>
                        <div id='tree1' class='content_wrapper' data-url='./report.aspx?action=gettreedata'>
                        </div>
                    </td>
                    <td class='middle'>
                    </td>
                    <td class='right' align='left'>
                        <iframe id='report_view' frameborder='0'>
                        </iframe>
                    </td>
                </tr>
            </table>
            <ul id="myMenu" class="dropdown-menu" role="menu" aria-labelledby="dLabel">
                <li class="downloadExcel"><a href="#downloadExcel"><i class="icon-edit"></i>下载 Excel 格式</a></li>
            </ul>
            <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
        </div>

    </form>
</body>
</html>
