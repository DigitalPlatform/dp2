
$(document).ready(function () {
    $("input:submit:not(.classical),input:file:not(.classical),button:not(.classical)").button();
    // SetSize();

    window.setTimeout("ResetAnchor()", 10);

    window.setTimeout("DisplayError()", 1000);
    window.setTimeout("GetSummary()", 10);

    $('.resizable').resizable();

    window.setTimeout("GetUntouched()", 1000);
});

function ScrollIntoView(container, elem) {
    if (container.length == 0 || elem.length == 0)
        return;

    var docViewTop = container.scrollTop() + container.offset().top;
    var docViewBottom = docViewTop + container.height();

    var elemTop = elem.offset().top;
    var elemBottom = elemTop + elem.height();

    var delta = 0;
    if (elemBottom > docViewBottom)
        delta = docViewBottom - elemBottom;
    else if (elemTop < docViewTop)
        delta = docViewTop - elemTop;

    if (delta != 0) {
    /*
        container.animate({
            scrollTop: container.scrollTop() - delta
        }, 'slow');
        */
        container.scrollTop(container.scrollTop() - delta);
    }

}

function PopTooltips(targets, id) {
    var 
        target = false,
        tooltip = false,
        title = false;

    targets.each(function (index) {
        target = $(this);
        tip = target.attr('title');
        tooltip = $('<div id="'+id+'"></div>');

        if (!tip || tip == '')
            return false;

        target.removeAttr('title');
        tooltip.css('opacity', 0)
               .html(tip)
               .appendTo('body');

        var init_tooltip = function () {
            if ($(window).width() < tooltip.outerWidth() * 1.5)
                tooltip.css('max-width', $(window).width() / 2);
            else
                tooltip.css('max-width', 340);

            var pos_left = target.offset().left + (target.outerWidth() / 2) - (tooltip.outerWidth() / 2),
                pos_top = target.offset().top - tooltip.outerHeight() - 20;

            if (pos_left < 0) {
                pos_left = target.offset().left + target.outerWidth() / 2 - 20;
                tooltip.addClass('left');
            }
            else
                tooltip.removeClass('left');

            if (pos_left + tooltip.outerWidth() > $(window).width()) {
                pos_left = target.offset().left - tooltip.outerWidth() + target.outerWidth() / 2 + 20;
                tooltip.addClass('right');
            }
            else
                tooltip.removeClass('right');

            if (pos_top < 0) {
                pos_top = target.offset().top + target.outerHeight();
                tooltip.addClass('top');
            }
            else
                tooltip.removeClass('top');

            tooltip.css({ left: pos_left, top: pos_top })
                   .animate({ top: '+=10', opacity: 1 }, 50);
        };

        init_tooltip();
        $(window).resize(init_tooltip);

    });
}

function GetUntouched() {
    if ($(".messagecolumn A").length == 0)
        return;
    var xhr = $.ajax({
        url: 'mymessage.aspx?action=getuntouched',
        cache: false,
        statusCode: {
            500: function () {
                // $('#message').append('\r\nerror 500');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
        },
        success: function (data) {
            var o = null;
            try {
                o = eval('(' + data + ')');
            }
            catch (e) {
                // $('#message').append('\r\ngetinfo exception :' + e.description);
                return;
            }

            if (o.ErrorString.length > 0) {
                // $('#message').append("error: " + o.ErrorString + "\r\n");
                return;
            }

            // alert(o.Count);
            if (o.Count == "" || o.Count == "0")
                return;

            $(".messagecolumn A").attr("title", "有 " + o.Count + " 个新消息");
            PopTooltips($(".messagecolumn A"), "tooltip");
        }
    });

}

function SetSize() {
    var outer = document.getElementById("outerframe");

    if (outer != null) {
        outer.style.width = (screen.width - 34) + "px";
        window.setTimeout("ResetAnchor()", 10);
    }
}

function TogglePageAll(button) {
    if ($(button).text() == '+')
        $(button).text('-');
    else
        $(button).text('+');
    $(button).parent().next().toggleClass("hide");
}

function GetSummary() {
    var o = $(".pending:first");
    if (o.length == 0) {
        // $(window).scroll();
        return;
    }

    var barcode = o.text();
    if (barcode == "") {
        o.removeClass("pending");
        window.setTimeout("GetSummary()", 1);
        return;
    }

    var lang = $('select#langlist option[SELECTED]').attr('value');

    $.ajax({
        url: 'getsummary.aspx?barcode=' + encodeURIComponent(barcode) + "&lang=" + lang,
        type: 'POST',
        cache: true,
        statusCode: {
            500: function () {
                o.text('\r\nerror 500');
                o.removeClass("pending");
            }
        },

        success: function (data) {
            o.html(data);
            o.removeClass("pending");
            window.setTimeout("GetSummary()", 1);
        }
    });
}

function ResetAnchor() {
    var anchor = window.location.hash;
    if (anchor != "") {
        window.location.hash = "";
        window.location.hash = anchor;
    }
}
function DisplayError() {
    if ($(".errorinfo-frame").length != 0) {
        $(".errorinfo-frame").dialog({
            modal: true,
            buttons: {
                Ok: function () {
                    $(this).dialog("close");
                }
            }
        });
        // $(".errorinfo-frame").dialog();
    }
}

function LangPostBack() {
    var theForm1 = document.forms['form1'];
    if (!theForm1) {
        theForm = document.form1;
    }

    theForm1.submit();
}

function CoverImageError(source) {
    source.src = "./style/blankcover.gif";
    source.onerror = "";
    return true;
}

// workaround for IE bug
function myConfirm(text) {
    if (confirm(text) == false)
        return cancelClick();
    return true;
}

// yet another IE hack
function cancelClick() {
    if (window.event) window.event.cancelBubble = true;
    return false;
}

// ColumnControl.cs
function HilightColumnCmdline(object) {
    $(object).prevAll().find("TD.no").effect("highlight", {}, 2000, null);
    $(object).effect("highlight", {}, 2000, null);

}

function onColumnCheckboxClick(object) {
    $(object).parent().parent().toggleClass("selected");
}

// CommentControl.cs
function HilightCommentCmdline(object) {
    $(object).parent().parent().effect("highlight", {}, 2000, null);
    // $(object).effect("bounce", {}, 400, null);
}

