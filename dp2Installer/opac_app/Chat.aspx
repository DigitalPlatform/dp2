<%@ Page Language="C#" AutoEventWireup="true" Inherits="Chat"
    MaintainScrollPositionOnPostback="true" ValidateRequest="false" Codebehind="Chat.aspx.cs" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>
<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>贴吧</title>
    <cc1:LinkControl ID="LinkControl1" runat="server" href="head.css" />
    <cc1:LinkControl ID="LinkControl2" runat="server" href="chatroom.css" />
    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css" rel="stylesheet"
        type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
    <script type="text/javascript" src="./ajaxfileupload.js"></script>
    <script>
        $(function () {
            $(".resizable").resizable();
        });
    </script>
</head>
<body>
    <script type="text/javascript" language="javascript" src="opac.js"></script>
    <script type="text/javascript" language="javascript" src="chatroom.js"></script>
    <form id="form1" runat="server">
    <div id="frame">
        <cc1:TitleBarControl ID="TitleBarControl1" runat="server" />
        <table id='columns'>
            <tr>
                <td id='room_name_column'>
                    <div id='room_name_area'>
                        <div class='input_line'>
                            <div class='label1'>
                                栏目:</div>
                            <asp:DropDownList ID="DropDownList_roomName" runat="server" AutoPostBack="True">
                            </asp:DropDownList>
                        </div>
                        <hr />
                        <div class='input_line'>
                            <div class='label1'>
                                日期:</div>
                            <asp:TextBox ID="TextBox_currentDate" runat="server" ReadOnly="True"></asp:TextBox>
                        </div>
                        <asp:Calendar ID="Calendar1" runat="server" OnDayRender="Calendar1_DayRender" OnSelectionChanged="Calendar1_SelectionChanged">
                        </asp:Calendar>
                        <asp:CheckBox ID="CheckBox_displayOperation" runat="server" Text="显示操作事项" onclick="displayOperationChanged(this);"/>
                        <asp:PlaceHolder ID="PlaceHolder_managemenPanel" runat="server">
                            <br />
                            <br />
                            <div class='input_line'>
                                <button id='Button_beginCreate' type="button" onclick="OpenCreateDialog();">
                                    创建新栏目</button>
                                &nbsp;
                                <asp:Button ID="Button_deleteChatRoom" runat="server" Text="删除栏目" OnClick="Button_deleteChatRoom_Click"
                                    OnClientClick="return myConfirm('确实要删除当前栏目? (一旦删除，不可恢复)');" />
                            </div>
                        </asp:PlaceHolder>
                    </div>
                </td>
                <td id='splitter_column' onclick="$('TD#room_name_column').toggle();$('TABLE.title').toggle();SetMessageSize();">
                    <div id='splitter_area'>
                    </div>
                </td>
                <td id='message_column'>
                    <div id='message_area'>
                        <div id='message_frame'>
                            <div id='message_resize' class='resizable'>
                                <div id='message' onscroll="OnScroll(this);">
                                </div>
                            </div>
                            <div id="progress">
                                <div id='progress_circle'>
                                </div>
                                <div id='progress_text'>
                                    &nbsp;
                                </div>
                                <div class="clear">
                                </div>
                            </div>
                            <div id='action_frame'>
                                <div id='send_text_area'>
                                    <asp:TextBox ID="text" runat="server" Rows="6" TextMode="MultiLine" onkeydown="if(event.which || event.keyCode){if ((event.which == 13 || event.keyCode == 13) && event.ctrlKey) {DoSend('');}} else {return true};"></asp:TextBox>
                                </div>
                                <div id='send_button_area'>
                                    <button id='send_button' type="button" onclick="DoSend('');">
                                        发送(Ctrl+Enter)</button>
                                    <br />
                                    <button id='send_sticker_button' type="button" onclick="DoSend('sticker');">
                                        发送为贴纸</button>
                                </div>
                                <div id='send_option_area'>
                                    <span class='label2'>上传图像文件: </span>
                                    <asp:FileUpload ID="fileToUpload" runat="server" />
                                    <img id="loading1" src="./style/ajax-loader.gif" style="display: none;"  />
                                    <br />
                                    <span class='label2'>网名或昵称: </span>
                                    <asp:TextBox ID="TextBox_userName" runat="server"></asp:TextBox>
                                    <asp:Label ID="Label_userNameComment" runat="server" Text=""></asp:Label>

                                </div>
                            </div>
                        </div>
                    </div>
                </td>
            </tr>
        </table>
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    <asp:HiddenField ID="HiddenField_isManager" runat="server" />
    <asp:HiddenField ID="HiddenField_today" runat="server" />
    <asp:HiddenField ID="HiddenField_editors" runat="server" />

    <asp:PlaceHolder ID="PlaceHolder_createDialog" runat="server" Visible="true">
        <asp:Panel ID='createdialogform' runat="server" title="创建新的栏目" style="display: none;">
            <asp:Label ID="Label_roomName" runat="server" Text="Label">栏目名: </asp:Label>
            <asp:TextBox ID="TextBox_roomName" runat="server"></asp:TextBox>
            <br />
            <br />
            <asp:Button ID="Button_create" runat="server" Text="创建" OnClick="Button_create_Click"
                OnClientClick="" />
        </asp:Panel>
    </asp:PlaceHolder>
    </form>
</body>
</html>
