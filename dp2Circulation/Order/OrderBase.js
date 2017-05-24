

function ScrollIntoView(elem) {
    var obj = elem[0];

    var obj_left = 0;
    var obj_top = 0;

    while (obj != document.body) {
        obj_left += obj.offsetLeft;
        obj_top += obj.offsetTop;
        obj = obj.offsetParent;
    }

    var obj_height = elem.height();

    var window_offset = window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop;
    // var window_offset = document.documentElement.scrollTop; // $(window).scrollTop();
    var window_height = $(window).height();

    if (obj_top < window_offset) {
        $(window).scrollTop(obj_top);
        return;
    }

    if (obj_top + obj_height > window_offset + window_height) {
        // alert("obj_top=" + obj_top + " obj_height=" + obj_height + "  window_offset=" + window_offset + " window_height=" + window_height);
        $(window).scrollTop(obj_top + obj_height - window_height);
        return;
    }
}

function getWindowOffset()
{
    return window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop;
}

function setWindowOffset(offset)
{
    $(window).scrollTop(offset);
}

function GetPosition(obj) {
    var left = 0;
    var top = 0;

    while (obj != document.body) {
        left += obj.offsetLeft;
        top += obj.offsetTop;
        obj = obj.offsetParent;
    }

    var v1 = document.documentElement.scrollTop;
    var v2 = document.body.scrollTop;
    var v3 = $(window).scrollTop();
    // var top = ($(window).scrollTop() || $("body").scrollTop());
    var h = document.body.offsetHeight;
    var h1 = $(window).height();

    alert("Left Is : " + left + "\r\n" + "Top   Is : " + top + " v1=" + v1 + " v2=" + v2 + " v3=" + v3 + " h1=" + h1);

    $(window).scrollTop(top);
}