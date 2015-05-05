
var CurResultOffs = 0;
var CurResultVersion = "";
var ScrollToEnd = true;
var xhr = null;
var Doing = 0;
var loop_interval = 2000;
var RunningStyle = -1;  // -1 不在运行中 0 一次性运行 1 循环运行


$(document).ready(function () {
    ClearMessage();
    window.setTimeout(SetMessageSize, 10);
    window.setTimeout(function() { RefreshLoop("");}, 10);
    enableAction();
    $('#action_frame').draggable();
    $("input:submit,input:file,button").button();

    g_displayOperation = $("#CheckBox_displayOperation").attr("checked");

    $(window).blur(function () {
        loop_interval = 60 * 1000;  // 一分钟
    });
    $(window).focus(function () {
        loop_interval = 2000;   // 2秒
    });

});

var g_displayOperation = true;

function rewind() {
    // ClearMessage();
    CurResultOffs = 0; // rewind
    CurResultVersion = 0;
}

function displayOperationChanged(obj) {
    if (obj.checked == true)
        g_displayOperation = true;
    else
        g_displayOperation = false;

    $('#message').append('开始刷新... 然后这一行应该很快消失');

    rewind();

    // 立即刷新
    window.setTimeout(function() { RefreshOnce("clear");}, 100);

    // 请求服务器更新存储
    window.setTimeout(RequestOptionChanged, 2000);
}

function RequestOptionChanged() {

    // notify server
    var boolvalue = "";
    if (g_displayOperation == true)
        boolvalue = "true";
    else
        boolvalue = "false";
    var options = "displayOperation=" + boolvalue;

    $.ajax({
        url: 'chat.aspx?action=optionchanged',
        type: 'POST',
        data: 'options=' + encodeURIComponent(options),
        cache: false,
        statusCode: {
            500: function () {
                $('#message').append('\r\nerror 500');
            }
        },

        success: function (data) {
            $('#message').append('update option finish');
        }
    });
}

function SetMessageSize() {

    var header_height = 0;
    if ($('TABLE.title:visible').length > 0)
        header_height = $('TABLE.title:visible').height();
    var columnbar = $('TABLE.columnbar')[0];

    var message = document.getElementById("message_resize");
    message.style.height = (document.documentElement.clientHeight - header_height - columnbar.clientHeight - 30) + "px";

    // $("#action_frame").css("top", "-" + ($("#action_frame").height() + 14) + "px");
    $("#action_frame").css("top", document.documentElement.clientHeight - $("#action_frame").height() - 30 + "px");
    $("#action_frame").css("left", document.documentElement.clientWidth - $("#action_frame").width() - 50 + "px");

}

function PrepareHover() {

    if ($('#HiddenField_isManager').val() == 'no')
        return;

    $(".line").hover(
                function () {
                    // not allow delete operation item
                    if ($(this).hasClass("operation") == true)
                        return;
                    if ($(this).hasClass("title") == true)
                        return;

                    if ($(this).find("button:last").length == 0) {
                        $(this).prepend($("<button class='delete_button'>删除</button>"));
                        $(this).children("button").button();
                    }
                    $('.delete_button').click(function () {
                        var refid = $(this).parent().find(".refid").text();
                        DoDelete(refid, $(this).parent());

                    });
                },
                function () {
                    $(this).find("button:last").remove();
                }
            );
}

function SetImage() {

    var roomName = $('#DropDownList_roomName').val();

    $('.image').each(function (index) {
        var filename = $(this).text();
        if (filename != "") {
            $(this).empty();
            $(this).append($("<img class='attachment' src='chat.aspx?action=getimage&room=" + encodeURIComponent(roomName) + "&filename=" + filename + "'/>"));
        }
    });
}

function SetPhoto() {
    $('.userid').each(function (index) {
        if ($(this).children("div").length > 0)
            return;

        var userid = $(this).text();
        var displayName = $(this).attr("displayName");
        var photoUrl = $(this).attr("photo");
        if (userid != "") {
            $(this).empty();
            var fragment = "";

            // 可以考虑废止这一段
            /*
            if (typeof (photoUrl) == 'undefined' || photoUrl == "") {
                if (typeof (displayName) == 'undefined' || displayName == "")
                    photoUrl = "getphoto.aspx?barcode=" + encodeURIComponent(userid);
                else {
                    var encrypt = ParseEncrypt(userid);
                    if (encrypt == "")
                        photoUrl = "getphoto.aspx?barcode=" + encodeURIComponent(userid) + "&displayName=" + encodeURIComponent(displayName);
                    else
                        photoUrl = "getphoto.aspx?encrypt_barcode=" + encodeURIComponent(encrypt) + "&displayName=" + encodeURIComponent(displayName);
                }
            }
            */

            if (typeof (photoUrl) == 'undefined' || photoUrl == "")
                fragment = "";
            else
                fragment = "<img class='photo' src='" + photoUrl + "'/>";

            if (typeof (displayName) == 'undefined' || displayName == "")
                fragment += "<div class='username'>" + userid + "</div>";
            else
                fragment += "<div class='username'>[" + displayName + "]</div>";
            $(this).append($(fragment));
        }
    });
}

function ParseEncrypt(text) {
    if (text.length <= "encrypt:".length)
        return "";
    var head = text.substring(0, "encrypt:".length);
    if (head == "encrypt:")
        return text.substring("encrypt:".length);
    return "";
}

function RemoveItem(refid) {
    $('DIV.line DIV.refid').each(function (index) {
        var cur_refid = $(this).text();
        if (cur_refid == refid) {
            $(this).parent().remove();
        }
    });
}

function DoNotify() {
    var ismanager = $('#HiddenField_isManager').val() == 'yes';

    $('.notify.itemdeleted').each(function (index) {
        var refid = $(this).children("div.refid").text();
        RemoveItem(refid);

        if (g_displayOperation == true) {
            $(this).removeClass("notify");
            $(this).addClass("line operation");

            if (ismanager == false) {
                // hide deleted record string
                $(this).children("div.deletedrecord").remove();
            }

            $(this).append($("<div class='text'>删除条目 " + refid + "<div/>"));
        }
        else
            $(this).remove();
    });
}

function OnScroll(o) {
    if (o.scrollTop + o.clientHeight >= o.scrollHeight)
        ScrollToEnd = true;
    else
        ScrollToEnd = false;

    /* $('#progress_text').text("scrollTop="+ o.scrollTop + ", scrollHeight=" + o.scrollHeight + ",clientHeight=" + o.clientHeight); */
}

function ToEnd() {
    if (ScrollToEnd == true) {
        var div = $("#message")[0];
        div.scrollTop = div.scrollHeight;
    }
}

function DoSend(style) {

    var text1 = $('#text').val();
    var date = $('#TextBox_currentDate').val();

    if (text1 == "") {
        alert("尚未输入要发送的文字");
        return;
    }

    var roomName = $('#DropDownList_roomName').val();
    var userName = $('#TextBox_userName').val();

    var upfile = $("#fileToUpload").val();

    // alert(upfile);

    if (upfile != 'undefined' && upfile != "") {
        $("#loading1").show();
        $('#action_frame #text, action_frame #button').attr("disabled", "disabled");

        $.ajaxFileUpload({
            url: 'chat.aspx?action=sendimage',
            secureuri: false,
            fileElementId: 'fileToUpload',
            dataType: 'json',
            beforeSend: function () {
                // $("#loading1").show();
            },
            complete: function () {
                $("#loading1").hide();
                $('#action_frame #text, action_frame #button').attr("disabled", "");
                window.setTimeout(function () { $('#text').focus(); }, 100);
            },
            success: function (data, status) {
                var o = eval(data);
                if (typeof (o.error) != 'undefined') {
                    if (o.error != '') {
                        alert("1: " + o.error);
                    } else {
                        alert("2: " + o.msg);
                    }
                    return;
                }
                // var o = eval('(' + data + ')');
                window.setTimeout(function () { DoSend(style); }, 1);
            },
            error: function (data, status, e) {
                // alert(e);
                var o = eval(data);
                alert("3: data=" + data + " error=" + o.error + " status=" + status + " e=" + e);
            }
        });
        return;
    }


    $.ajax({
        url: 'chat.aspx?action=send',
        type: 'POST',
        data: 'room=' + encodeURIComponent(roomName) + '&date=' + date + '&name=' + encodeURIComponent(userName) + '&style=' + style + '&text=' + encodeURIComponent(text1),
        cache: false,
        statusCode: {
            500: function () {
                $('#message').append('\r\nerror 500');
            }
        },

        success: function (data) {
            var o = null;

            try {
                o = eval('(' + data + ')');
            }
            catch (e) {
                // possible login page
                $('#message').append('\r\nsend exception :' + e.description);
                return;
            }

            if (o.ErrorString.length > 0) {
                alert("4: " + o.ErrorString);
                return;
            }

            $('#text').val('');
            ScrollToEnd = true;
            window.setTimeout(function () { RefreshOnce(""); }, 1);
        }
    });
}


function DoDelete(refid, div) {
    if (typeof (refid) == 'undefined' || refid == "")
        return;

    div.effect("highlight", {}, 3000);

    var username = div.find(".userid .username").text();
    var text = div.children(".text").text();
    if (text.length > 100)
        text = text.substring(0, 100) + "...";
    if (myConfirm("确实要删除这条消息?\n\n" + username + ":\n" + text) == false)
        return;

    var roomName = $('#DropDownList_roomName').val();
    var date = $('#TextBox_currentDate').val();

    div.effect("explode", {}, 500, callback);
    function callback() {
        div.hide();
    }

    $.ajax({
        url: 'chat.aspx?action=delete',
        type: 'POST',
        data: 'room=' + encodeURIComponent(roomName) + '&date=' + date + '&refid=' + encodeURIComponent(refid),
        cache: false,
        statusCode: {
            500: function () {
                $('#message').append('\r\nerror 500');
            }
        },

        success: function (data) {

            var o = eval('(' + data + ')');

            if (o.ErrorString.length > 0) {
                alert("5: " + o.ErrorString);
                return;
            }

            // if version changed
            if (CurResultVersion != o.NewFileVersion) {
                CurResultVersion = o.NewFileVersion;

                ScrollToEnd = false;
                div.remove();
            }

            if (CurResultOffs > 0)
                CurResultOffs--;

            div.children('.delete_button').click(function () {
                alert("已经删除了");
            });



        }
    });
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}
function htmlDecode(value) {
    return $('<div/>').html(value).text();
}

function ClearMessage() {
    var roomName = $('#DropDownList_roomName').val();
    var userName = $('#TextBox_userName').val();
    var dateToday = $('#HiddenField_today').val();
    var datePanel = $('#TextBox_currentDate').val();
    var editors = $('#HiddenField_editors').val();

    var dateString = "";
    
    if (datePanel.length == 8)
        dateString = datePanel.substring(0, 4) + "." + datePanel.substring(4, 6) + "." + datePanel.substring(6, 8);

    $('#message').text("");
    if (dateToday != datePanel)
        $('#message').append("<div class='line title'><div class='text'>" + htmlEncode(roomName + " » 往日 " + dateString) + "<br/>编辑: " + editors + "</div></div>");
    else
        $('#message').append("<div class='line title'><div class='text'>" + htmlEncode(roomName + " » 今日 " + dateString) + "<br/>编辑: " + editors + "</div></div>");
}

function RefreshLoop(style) {
    LoadOne(1, style);
}

function RefreshOnce(style) {
    LoadOne(0, style);
}

function LoadOne(interval, style) {

    if (Doing > 0) {
        if (interval > 0) {
            // 正在运行的是循环方式，则本请求可以放弃
            if (RunningStyle == 1)
                return;
            $('#message').append('\r\n 冲突，重试 Loop ');
            window.setTimeout(function () { RefreshLoop(style); }, 4000);
        }
        else {
            $('#message').append('\r\n 冲突，重试 Once... ');
            window.setTimeout(function () { RefreshOnce(style); }, 4000);
        }
        return;
    }

    if (xhr != null)
        xhr.abort();

    /* $('#progress_text').text("CurResultOffs=[" + CurResultOffs + "]"); */


    var roomName = $('#DropDownList_roomName').val();
    var date = $('#TextBox_currentDate').val();
    var dateToday = $('#HiddenField_today').val();

    // $('#progress_text').text("Doing=[" + Doing + "]");

    Doing++;
    RunningStyle = interval;
    var xhr = $.ajax({
        url: 'chat.aspx?action=getinfo&room=' + encodeURIComponent(roomName) + '&date=' + date + '&start=' + CurResultOffs + '&max_lines=-1',
        cache: false,
        statusCode: {
            500: function () {
                $('#message').append('\r\nerror 500');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            Doing--;
            RunningStyle = -1;
        },
        context: interval,
        success: function (data) {

            var o = null;

            try {
                o = eval('(' + data + ')');
            }
            catch (e) {
                // possible login page
                $('#message').append('\r\ngetinfo exception :' + e.description);
                Doing--;
                RunningStyle = -1;
                return;
            }

            if (o.ErrorString.length > 0) {
                $('#message').append("error: " + o.ErrorString + "\r\n");
                $('#progress_text').text("CurResultOffs=[" + CurResultOffs + "]");
                Doing--;
                RunningStyle = -1;
                return;
            }

            /*
            $('#progress_text').text(o.ProgressText);
            */

            Circle();


            if (o.ResultText.length == 0) {
                // $('#message').append('.');
            }
            else {
                // clear old content
                if (CurResultOffs == 0 || style == "clear")
                    ClearMessage();

                $('#message').append(o.ResultText);
                PrepareHover();
                SetImage();
                SetPhoto();
                DoNotify();
                ToEnd();
            }

            if (CurResultOffs != 0 && CurResultVersion != o.ResultVersion) {
                rewind();

                Doing--;
                RunningStyle = -1;


                /* $('#message').append("***新内容 version=" + o.ResultVersion + " ***\r\n"); */
                if (interval != 0)
                    window.setTimeout(function () { RefreshLoop("clear"); }, interval);
                return;
            }

            CurResultVersion = o.ResultVersion;

            if (o.NextStart < CurResultOffs) {
                rewind();

                /* $('#message').append("***新内容***\r\n"); */
            }
            else
                CurResultOffs = o.NextStart;

            if (interval != 0) {
                if (CurResultOffs < o.TotalLines)
                    window.setTimeout(function () { RefreshLoop(""); }, 1);
                else {
                    if (date != dateToday)
                        window.setTimeout(function () { RefreshLoop(""); }, 600 * 1000); // 十分钟 不是当天的内容，更新的可能性很小
                    else
                        window.setTimeout(function () { RefreshLoop(""); }, loop_interval);  // interval
                }
            }

            Doing--;
            RunningStyle = -1;

        }
    });
}

var icons = new Array("|", "/", "-", "\\");
var icon_index = 0;

function Circle() {
    $('#progress_circle').text(icons[icon_index++ % 4]);
}

function OpenCreateDialog() {
    $('#createdialogform').dialog({ modal: true });
    $('#createdialogform').parent().appendTo($('form:first'));
}

function enableAction() {
    // var now = new Date();
    // var dateToday = now.format('yyyyMMdd'); /

    var dateToday = $('#HiddenField_today').val();
    var datePanel = $('#TextBox_currentDate').val();

    if (dateToday != datePanel) {
        $('#action_frame *').attr("disabled", "disabled");  // frame不要disabled，里面的控件才需要disabled
    }
}

/*
Date.prototype.format = function (format) {
    var o = {

        "M+": this.getMonth() + 1, //月
        "d+": this.getDate(), //日
        "h+": this.getHours(), //小时
        "m+": this.getMinutes(), //分
        "s+": this.getSeconds(), //秒
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度
        "S": this.getMilliseconds() //毫秒
    }

    if (/(y+)/.test(format))
        format = format.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(format))
            format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
    return format;
}
*/