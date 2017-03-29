<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GetNumber1.aspx.cs" Inherits="gcatv2.GetNumber" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>通用汉语著者号码表</title>
    <link href="./styles/getnumber.css" rel="stylesheet" type="text/css" />

    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css"
        rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
</head>
<body>
    <asp:PlaceHolder ID="PlaceHolder_script" runat="server"></asp:PlaceHolder>
    <form id="form1" runat="server">

        <div class='main'>
            <asp:HiddenField ID="HiddenField_questions" runat="server" />
            <asp:HiddenField ID="HiddenField_id" runat="server" />
            <asp:HiddenField ID="HiddenField_memoID" runat="server" />
            <div class='titlebar'>通用汉语著者号码表 -- GCAT V2</div>
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
                版权所有 (C) 2006 - 2012 数字平台(北京)软件有限责任公司
            </p>
            <p class='visitcounter'>
                您是自2006年3月以来 第
            <img alt='counter' src="http://dp2003.com/dp2counter/counter.aspx?id=gcat&backcolor=eeeeee" />
                位访客
            </p>
            <br />
        </div>
        <br />
        <asp:PlaceHolder ID="PlaceHolder_questionDialog" runat="server" Visible="false">
            <asp:Panel ID='questiondialogform' runat="server">
                <div class='questiontext'>
                    <asp:Label ID="Label_question" runat="server" Text="Label"></asp:Label>
                </div>
                <br />
                <br />
                <asp:TextBox ID="TextBox_answer" runat="server"></asp:TextBox>
                <asp:Button ID="Button_continue" runat="server" Text="继续" OnClick="Button_continue_Click"
                    OnClientClick="" />
            </asp:Panel>
        </asp:PlaceHolder>
        <asp:PlaceHolder ID="PlaceHolder_loginDialog" runat="server" Visible="false">
            <asp:Panel ID='logindialogform' runat="server">
                <table>
                    <tr>
                        <td class='name'>
                            <asp:Label ID="Label_id" runat="server" Text="Label">ID:</asp:Label>
                        </td>
                        <td class='value'>
                            <asp:TextBox ID="TextBox_id" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:CheckBox ID="CheckBox_memoID" runat="server" Text="记住ID" Checked="True"></asp:CheckBox>
                        </td>
                    </tr>
                    <tr>
                        <td colspan='2'>
                            <hr />
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Button ID="Button_login" runat="server" Text="继续" OnClick="Button_login_Click"
                                OnClientClick="" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
        </asp:PlaceHolder>
    </form>
</body>
</html>
