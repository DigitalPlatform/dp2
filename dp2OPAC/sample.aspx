<%@ Page Language="C#" AutoEventWireup="true" Inherits="sample" Codebehind="sample.aspx.cs" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label ID="Label1" runat="server" Text="Label">检索词</asp:Label>
        <asp:TextBox ID="TextBox_word" runat="server"></asp:TextBox>
        <br/>

        <asp:Label ID="Label2" runat="server" Text="Label"></asp:Label>
        <asp:DropDownList ID="DropDownList_dbname" runat="server">
            <asp:ListItem>中文图书</asp:ListItem>
        </asp:DropDownList>
        <br/>

        <asp:Label ID="Label3" runat="server" Text="Label"></asp:Label>
        <asp:DropDownList ID="DropDownList_from" runat="server">
            <asp:ListItem Value="title">题名</asp:ListItem>
        </asp:DropDownList>
        <br/>

        <asp:Label ID="Label4" runat="server" Text="Label"></asp:Label>
        <asp:DropDownList ID="DropDownList_matchStyle" runat="server">
            <asp:ListItem Value="left">前方一致</asp:ListItem>
        </asp:DropDownList>
        <br/>
        

        <asp:Button ID="Button_search" runat="server" Text="检索" 
            onclick="Button_search_Click" />
        <br/>

        <cc1:BrowseSearchResultControl ID="BrowseSearchResultControl1" runat="server" />

    </div>
    </form>
</body>
</html>
