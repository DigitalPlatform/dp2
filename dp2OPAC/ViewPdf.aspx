<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewPdf.aspx.cs" Inherits="ViewPdf" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>预览</title>
    <cc1:LinkControl ID="LinkControl2" runat="server" href="viewpdf.css" />
    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css" rel="stylesheet"
        type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
    <script>
        $(function () {
            $(".resizable").resizable();
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="main">
            <div class="toolbar">
                <asp:HiddenField ID="PageNo" runat="server" />
                <asp:HiddenField ID="PageCount" runat="server" />
                <asp:HiddenField ID="Uri" runat="server" />
                <asp:HiddenField ID="TitleParam" runat="server" />

                <asp:Button ID="FirstPage" runat="server" Text="|<" OnClick="FirstPage_Click" />

                <asp:Button ID="PrevPage" runat="server" Text="<" OnClick="PrevPage_Click" />
                <asp:Label ID="LabelPageNo" runat="server"></asp:Label>
                <asp:Button ID="NextPage" runat="server" Text=">" OnClick="NextPage_Click" />

                <asp:Button ID="TailPage" runat="server" Text=">|" OnClick="TailPage_Click" />
            </div>

            <div id="imageframe">
                <asp:Image ID="Image1" runat="server"></asp:Image>
            </div>
        </div>
    </form>
</body>
</html>
