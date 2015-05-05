<%@ Page Language="C#" AutoEventWireup="true" CodeFile="BatchTaskMonitor.aspx.cs"
    Inherits="BatchTaskMonitor" %>

<%@ Register Assembly="DigitalPlatform.OPAC.Web" Namespace="DigitalPlatform.OPAC.Web"
    TagPrefix="cc1" %>

<!doctype html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>批处理任务监控</title>
    <cc1:LinkControl ID="LinkControl1" runat="server" href="head.css" />
    <cc1:LinkControl ID="LinkControl2" runat="server" href="batchtaskmonitor.css" />



    <link href="./jquery-ui-1.8.7.custom/css/jquery-ui-1.8.7.custom.css" rel="stylesheet"
        type="text/css" />
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="./jquery-ui-1.8.7.custom/js/jquery-ui-1.8.7.custom.min.js"></script>
</head>
<body>
    <script type="text/javascript" language="javascript" src="opac.js"></script>
    <script type="text/javascript" language="javascript">

        var CurResultOffs = 0;
        var CurResultVersion = 0;

        $(document).ready(function () { window.setTimeout("Load()", 10); });

        function Load() {

            $.ajax({
                url: 'batchtaskmonitor.aspx?action=getinfo&name=CacheBuilder&result_offset=' + CurResultOffs + '&max_result_bytes=4096',
                cache: false,
                statusCode: {
                    500: function () {
                        $('#message').append('\r\nerror 500');
                    }
                },

                success: function (data) {
                    var o = eval('(' + data + ')');

                    if (o.ErrorString.length > 0) {
                        $('#message').append("error: " + o.ErrorString + "\r\n");
                        window.setTimeout("Load()", 1000);
                        return;
                    }

                    $('#progress_text').text(o.ProgressText);
                    Circle();

                    if (o.ResultText.length == 0) {
                        // $('#message').append('.');
                    }
                    else
                        $('#message').append(o.ResultText);

                    if (CurResultOffs == 0)
                        CurResultVersion = o.ResultVersion;
                    else if (CurResultVersion != o.ResultVersion) {
                        CurResultOffs = 0; // rewind
                        $('#message').append("***新内容 version=" + o.ResultVersion + " ***\r\n");
                        window.setTimeout("Load()", 1000);
                        return;
                    }

                    if (o.ResultTotalLength < CurResultOffs) {
                        CurResultOffs = 0; // rewind
                        $('#message').append("***新内容***\r\n");
                    }
                    else
                        CurResultOffs = o.ResultOffset;

                    window.setTimeout("Load()", 1000);
                }
            });


        }

        var icons = new Array("|", "/", "-", "\\");
        var icon_index = 0;

        function Circle() {
            $('#progress_circle').text(icons[icon_index++ % 4]);
        }
    
    </script>
    <form id="form1" runat="server">
    <div id="frame">
        <cc1:TitleBarControl ID="TitleBarControl1" runat="server" />
        <div id='first_line'>
            <asp:Label ID="Label_taskName" runat="server" Text=""></asp:Label>
        </div>
        <div id='message'>
        </div>
        <div id="progress">
            <div id='progress_circle'>
            </div>
            <div id='progress_text'>
            </div>
        </div>
        <cc1:FooterBarControl ID="FooterBarControl1" runat="server" />
    </div>
    </form>
</body>
</html>
