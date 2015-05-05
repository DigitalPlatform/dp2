<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Statis.aspx.cs" Inherits="Statis"
    Culture="auto" meta:resourcekey="PageResource1" UICulture="auto" MaintainScrollPositionOnPostback="true" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc2" %>
<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>统计信息</title>
    <cc2:LinkControl ID="Linkcontrol1" runat="server" href="head.css" />
    <cc2:LinkControl ID="Linkcontrol2" runat="server" href="statis.css" />
    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css" rel="stylesheet"
        type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
</head>
<body>
    <script type="text/javascript" language="javascript" src="opac.js"></script>
    <script type="text/javascript" language="javascript" src="statis.js"></script>
    <form id="form1" runat="server">
    <div id="frame">
        <cc2:TitleBarControl ID="TitleBarControl1" runat="server" meta:resourcekey="TitleBarControl1Resource1" />
        <cc2:SideBarControl ID="SideBarControl1" runat="server" />
        <table id="tframe">
            <tr>
                <td valign='top' align='left'>
                    <div id="tabs">
                        <ul>
                            <li>
                                <asp:HyperLink ID="HyperLink1" href="#tabs-1" meta:resourcekey="HyperLink1Resource1"
                                    runat="server">单日</asp:HyperLink></li>
                            <li>
                                <asp:HyperLink ID="HyperLink2" href="#tabs-2" meta:resourcekey="HyperLink2Resource1"
                                    runat="server">日期范围</asp:HyperLink></li>
                        </ul>
                        <div id="tabs-2">
                            <asp:TextBox ID="TextBox_dateRange" runat="server" CssClass="date_range" onkeydown="if (event.which || event.keyCode){if ((event.which == 13) || (event.keyCode == 13)) {document.getElementById('Button_beginStatis').click();return false;}} else {return true}; "
                                meta:resourcekey="TextBox_dateRangeResource1"></asp:TextBox>
                            <asp:Button ID="Button_beginStatis" runat="server" Text="开始统计" CssClass="begin_statis"
                                OnClick="Button_beginStatis_Click" meta:resourcekey="Button_beginStatisResource1" />
                            <br />
                            <div class='date_range_comment'>
                                <asp:Literal ID="Literal1" runat="server" Text="注: 日期范围形态如下<br />" meta:resourcekey="Literal1Resource1">
                                </asp:Literal>
                                2008<br />
                                2008-2009<br />
                                200801<br />
                                200801-200804<br />
                                20081231<br />
                                20080101-20081231
                            </div>
                        </div>
                        <div id="tabs-1">
                            <asp:Calendar ID="Calendar1" runat="server" BackColor="White" BorderColor="#999999"
                                CellPadding="4" DayNameFormat="Shortest" Font-Names="Verdana" Font-Size="8pt"
                                ForeColor="Black" Height="180px" OnDayRender="Calendar1_DayRender" OnSelectionChanged="Calendar1_SelectionChanged"
                                Width="200px" meta:resourcekey="Calendar1Resource1">
                                <SelectedDayStyle BackColor="#666666" Font-Bold="True" ForeColor="White" />
                                <TodayDayStyle BackColor="#CCCCCC" ForeColor="Black" />
                                <SelectorStyle BackColor="#CCCCCC" />
                                <WeekendDayStyle BackColor="#FFFFCC" />
                                <OtherMonthDayStyle ForeColor="Gray" />
                                <NextPrevStyle VerticalAlign="Bottom" />
                                <DayHeaderStyle BackColor="#CCCCCC" Font-Bold="True" Font-Size="7pt" />
                                <TitleStyle BackColor="#999999" BorderColor="Black" Font-Bold="True" />
                            </asp:Calendar>
                        </div>
                    </div>
                </td>
                <td valign='top' width='auto' align='center'>
                    <div class='content_wrapper'>
                        <cc2:PanelControl ID="PanelControl2" runat="server"></cc2:PanelControl>
                        <div class='body_wrapper'>
                            <cc2:StatisViewControl ID="StatisViewControl1" runat="server" meta:resourcekey="StatisViewControl1Resource1" />
                        </div>
                    </div>
                </td>
            </tr>
        </table>
    <div id='tips'>test</div>
        <cc2:FooterBarControl ID="FooterBarControl1" runat="server" meta:resourcekey="FooterBarControl1Resource1" />
    </div>
    <asp:HiddenField ID="HiddenField_activetab" runat="server" />
    </form>
</body>
</html>
