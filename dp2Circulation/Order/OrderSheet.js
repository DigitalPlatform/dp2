

$(document).ready(function () {
    if (window.external == null) {
        alert("window 没有联接 external");
        return;
    }
    var external = window.external;

    // setEvents();
});

function toggleSel(o) {
    if (o.hasClass('check')) {
        o.toggleClass('sel');
        window.external.onSelectionChanged();
    }
}

function setSel(o) {
    if (o.hasClass('check')) {
        o.addClass('sel');
        window.external.onSelectionChanged();
    }
}

function clearAllSel() {
    var changed = false;
    $(".check.sel").each(function (index) {
        var tr = $(this);
        tr.removeClass('sel');
        changed = true;
    });
    if (changed == true)
        window.external.onSelectionChanged();
}

function clearAllChanged() {
    $(".changed").each(function (index) {
        $(this).removeClass('changed');
    });

    $(".new").each(function (index) {
        $(this).removeClass('new');
    });
}

function displayError(o, error) {
    if (error != null)
        o.addClass('error');
    else
        o.removeClass('error');

    setErrorText(o, error);
}

function setErrorText(o, error) {
    o.children('.error-text').detach();
    if (error != null)
        o.append("<div class='error-text'>" + error + "</div>");
}

function getSelection() {
    var result = "";
    $(".sel").each(function (index) {
        var tr = $(this);
        var biblio_recpath = tr.attr('biblio-recpath');
        var orders_refid = tr.attr('orders-refid');
        // result += "b=" + biblio_recpath + ",o=" + orders_refid + ";";
        result += orders_refid + "|";
    });
    return result;
}

function selectAllBiblio(clearBefore) {
    var changed = false;
    $(".check:enabled").each(function (index) {
        var tr = $(this);

        if (tr.hasClass('order')) {
            if (clearBefore) {
                if (clearSelIf(tr) == true)
                    changed = true;
            }
        }
        else {
            if (setSelIf(tr) == true)
                changed = true;
        }
    });
    if (changed == true)
        window.external.onSelectionChanged();
}

function selectAllOrder(clearBefore) {
    var changed = false;
    $(".check:enabled").each(function (index) {
        var tr = $(this);
        if (tr.hasClass('order')) {
            if (setSelIf(tr) == true)
                changed = true;
        }
        else {
            if (clearBefore) {
                if (clearSelIf(tr) == true)
                    changed = true;
            }
        }
    });
    if (changed == true)
        window.external.onSelectionChanged();
}

function setSelIf(o) {
    if (o.hasClass('sel') == false) {
        o.addClass('sel');
        return true;
    }
    return false;
}

function clearSelIf(o) {
    if (o.hasClass('sel') == true) {
        o.removeClass('sel');
        return true;
    }
    return false;
}

function onClicked(o) {
    var tr = $(o);
    if (window.event.ctrlKey) {
        toggleSel(tr);
    }
    else {
        clearAllSel();
        setSel(tr);
    }
}

//阻止事件冒泡函数
function stopBubble(e) {
    if (e && e.stopPropagation)
        e.stopPropagation()
    else
        window.event.cancelBubble = true
}

// 只是修改选中状态，不引发 window.external.onSelectionChanged();
function selectOrders(refid_list, scroll) {
    var changed = false;

    /*
    // 先清除所有书目行的选择
    $(".biblio.check.sel").each(function (index) {
        var tr = $(this);
        tr.removeClass('sel');
        changed = true;
    });
    */

    //alert("list:" + refid_list);

    var refids = refid_list.split('|');

    $(".item.check").each(function (index) {
        var tr = $(this);
        var current_refids = tr.attr('orders-refid').split('|');
        //alert("current_refids:" + current_refids);

        if (inarray(refids, current_refids) == false) {
            if (tr.hasClass('sel') == true) {
                tr.removeClass('sel');
                changed = true;
            }
        }
        else {
            if (tr.hasClass('sel') == false) {
                tr.addClass('sel');
                changed = true;
                if (scroll == true) {
                    window.setTimeout(function () {
                        ScrollIntoView(tr);
                    }, 100);
                }
            }
        }
        changed = true;
    });
    //if (changed == true)
    //    window.external.onSelectionChanged();
}

function inarray(array1, array2) {
    for (var i = 0; i < array1.length; i++) {
        var s = array1[i];
        if (jQuery.inArray(s, array2) != -1)
            return true;
    }

    return false;
}