<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewPdf.aspx.cs" Inherits="ViewPdf" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:HiddenField ID="PageNo" runat="server" />
            <asp:HiddenField ID="PageCount" runat="server" />

            <asp:Button ID="FirstPage" runat="server" Text="|<" OnClick="FirstPage_Click" />

            <asp:Button ID="PrevPage" runat="server" Text="<" OnClick="PrevPage_Click" />
            <asp:Label ID="LabelPageNo" runat="server"></asp:Label>
            <asp:Button ID="NextPage" runat="server" Text=">" OnClick="NextPage_Click" />

            <asp:Button ID="TailPage" runat="server" Text=">|" OnClick="TailPage_Click" />

        </div>

        <div>
            <asp:Image ID="Image1" runat="server"></asp:Image>
        </div>
    </form>
</body>
</html>
