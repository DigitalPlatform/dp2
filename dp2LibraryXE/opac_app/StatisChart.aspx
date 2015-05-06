<%@ Page Language="C#" AutoEventWireup="true" Inherits="StatisChart"
    meta:resourcekey="PageResource1" MaintainScrollPositionOnPostback="true" Codebehind="StatisChart.aspx.cs" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>
<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc2" %>
<!doctype html>
<html>
<head id="Head1" runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>统计图表</title>
    <link href="./jquerytreeview/jquery.treeview.css"
        rel="stylesheet" type="text/css" />
    <cc2:LinkControl ID="Linkcontrol1" runat="server" href="head.css" />
    <cc2:LinkControl ID="Linkcontrol2" runat="server" href="statischart.css" />
    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css" rel="stylesheet"
        type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
    <script type="text/javascript" src="./jquerytreeview/jquery.treeview.js"></script>
</head>
<body>
    <script type="text/javascript" language="javascript" src="opac.js"></script>
    <script type="text/javascript" language="javascript" src="statischart.js"></script>
    <form id="form1" runat="server">
    <div>
        <cc2:TitleBarControl ID="TitleBarControl1" runat="server" 
            meta:resourcekey="TitleBarControl1Resource1" 
            onrefreshing="TitleBarControl1_Refreshing" />
        <cc2:SideBarControl ID="SideBarControl1" runat="server" />
        <table class='total'>
            <tr>
                <td class='total-left' align='left'>
                    <div class='content_wrapper'>
                        <cc2:PanelControl ID="PanelControl1" runat="server"></cc2:PanelControl>
                        <cc2:treecontrol id="TreeView1" runat="server" 
                            ongetnodedata="TreeView1_GetNodeData" 
                            ontreeitemclick="TreeView1_TreeItemClick" />
                    </div>
                    <div class='content_wrapper'>
                        <cc2:PanelControl ID="PanelControl2" runat="server"></cc2:PanelControl>
                        <cc2:StatisEntryControl ID="StatisEntryControl1" runat="server"></cc2:StatisEntryControl>
                    </div>
                </td>
                <td class='total-middle'>
                </td>
                <td class='total-right' align='left'>
                    <div class='chart_wrapper'>
                        <asp:Label ID="Label_chartType" runat="server" Text="统计图类型" meta:resourcekey="Label_chartType"></asp:Label>
                        <asp:DropDownList ID="DropDownList_chartType" runat="server" AutoPostBack="True"
                            OnSelectedIndexChanged="DropDownList_chartType_SelectedIndexChanged">
                        </asp:DropDownList>
                        <asp:CheckBox ID="CheckBox_3D" runat="server" Text="3D" AutoPostBack="True" />
                        <br />
                        <asp:Chart ID="Chart1" runat="server" Width="500px" Height="230px">
                            <ChartAreas>
                                <asp:ChartArea Name="ChartArea1">
                                </asp:ChartArea>
                            </ChartAreas>
                        </asp:Chart>
                    </div>
                </td>
            </tr>
        </table>
        <asp:HiddenField ID="HiddenField_entryItems" runat="server" />
        <asp:HiddenField ID="HiddenField_imageWidth" runat="server" />
        <cc2:FooterBarControl ID="FooterBarControl1" runat="server" meta:resourcekey="FooterBarControl1Resource1" />
    </div>
    </form>
</body>
</html>
