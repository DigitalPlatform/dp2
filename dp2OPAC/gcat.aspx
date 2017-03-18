<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="gcat.aspx.cs" Inherits="WebApplication1.gcat" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />

    <title>通用汉语著者号码表取号</title>
    <cc1:LinkControl runat="server" href="gcat.css" />
    <link href="./style/gcat.css" rel="stylesheet" type="text/css" />

</head>
<body>

    <form id="form1" method="post" runat="server">

        <div class='main'>
            <div class='titlebar'>通用汉语著者号码表 -- GCAT V3</div>


            <p>
                <asp:Label ID="Label_errorInfo" runat="server" Width="100%" ForeColor="Red"></asp:Label>
            </p>
            <div id='authorline'>
                ① 著者:<br />
                <asp:TextBox class="ui-state-default" ID="TextBox_author" runat="server"></asp:TextBox>
            </div>
            <p class="optionsline">
                <asp:CheckBox ID="CheckBox_selectEntry" runat="server" Text="选择条目" Checked="True"></asp:CheckBox>
                &nbsp;
            <asp:CheckBox ID="CheckBox_selectPinyin" runat="server" Text="选择多音字" Checked="True"></asp:CheckBox>
                &nbsp;
            <asp:CheckBox ID="CheckBox_outputDebugInfo" runat="server" Text="输出调试信息" Checked="True"></asp:CheckBox>
            </p>
            <p>
                <asp:Button class="ui-state-default1 ui-corner-all1" ID="Button_get" runat="server" Text="② 获取" OnClick="Button_get_Click"></asp:Button>
            </p>


            <asp:PlaceHolder ID="question_frame" runat="server">

                <asp:HiddenField ID="hidden_questions" runat="server" />
                <div class='questiontext'>
                    <asp:Label ID="Label_questionText" runat="server" Width="100%"></asp:Label>
                </div>
                <p>
                    输入答案:<br />
                    <asp:TextBox ID="TextBox_answer" runat="server" Width="100px"></asp:TextBox>
                    <asp:Button ID="Button_answer" runat="server" Width="96px" Text="继续" OnClick="Button_get_Click"></asp:Button>
                </p>
                <p>
                </p>

            </asp:PlaceHolder>

            <p>
                ③ 著者号:<br />
                <asp:TextBox class="ui-state-default" ID="TextBox_number" runat="server"
                    ReadOnly="True"></asp:TextBox>
            </p>
            <asp:Panel ID="Panel_debuginfo" class='debuginfo' runat="server" Visible="false">
                <asp:Label ID="Label_debugInfo" runat="server"></asp:Label>
            </asp:Panel>

            <br />
            <hr />
            <p class='copyright'>
                数字平台(北京)软件有限责任公司 <a href="http://github.com/digitalplatform/dp2">开源的图书馆业务软件 dp2</a>
            </p>
            <p class='visitcounter'>
                您是自2006年3月以来 第
            <img alt='counter' src="http://dp2003.com/dp2counter/counter.aspx?id=gcat&backcolor=eeeeee" />
                位访客
            </p>
            <br />
        </div>
    </form>
</body>
</html>
