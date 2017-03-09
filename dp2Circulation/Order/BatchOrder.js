

$(document).ready(function () {
    if (window.external == null) {
        alert("window 没有联接 external");
        return;
    }
    var external = window.external;

    // setEvents();
});

/*
function setEvents() {
    $(".list.event").change(function () {
        var td = $(this).parent();
        var tr = $(this).parent().parent();
        var refid = tr.attr('ref-id');
        var biblio_recpath = tr.attr('biblio-recpath');
        var col_name = $(this).attr('col-name');
        var selected = $(this).val();
        // alert("Handler for .change() called. col_name=" + col_name +",value="+selected + ",refid=" +refid + ",biblio_recpath=" + bilbio_recpath);
        window.external.onOrderChanged(biblio_recpath,
            refid,
            col_name,
            selected);
        $(this).addClass('changed');
        tr.addClass('changed');

        if (col_name == 'copy') {
            var result = window.external.verifyDistribute(biblio_recpath, refid);

            var dis_td = tr.children('.dis-text');
            displayError(dis_td, result);
        }
        //$(this).removeClass('event');
    });
    $(".list.event").removeClass('event');

    $(".check.event").click(function () {
        var tr = $(this);
        if (window.event.ctrlKey) {
            toggleSel(tr);
        }
        else {
            clearAllSel();
            setSel(tr);
        }
        //$(this).removeClass('event');
    });
    $(".check.event").removeClass('event');
}
*/

function toggleSel(o) {
    o.toggleClass('sel');
    window.external.onSelectionChanged();
}

function setSel(o) {
    o.addClass('sel');
    window.external.onSelectionChanged();
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

function getFirstSelectedBiblioRecPath() {
    var tr = $(".sel").first();
    if (tr.hasClass('order')) {
        return tr.attr('biblio-recpath');
    }
    else {
        return tr.attr('biblio-recpath');
    }
}

function newOrder(xml) {
    $(".sel").each(function (index) {
        var tr = $(this);
        if (tr.hasClass('order')) {
            var biblio_recpath = tr.attr('biblio-recpath');
            tr.after(window.external.newOrder(biblio_recpath, xml));

            window.setTimeout(function () {
                // tr.next().scrollIntoView();
                ScrollIntoView(tr.next());
            }, 10);
        }
        else {
            var biblio_recpath = tr.attr('biblio-recpath');
            var next_tr = tr.next();
            if (next_tr.hasClass('title') == false) {
                tr.after(window.external.getOrderTitleLine(biblio_recpath));
                next_tr = tr.next();
            }
            next_tr.after(window.external.newOrder(biblio_recpath, xml));
            window.setTimeout(function () {
                // next_tr.next().scrollIntoView();
                ScrollIntoView(next_tr.next());
            }, 10);
        }
    });

    // setEvents();
}

function deleteOrder() {
    var count = 0;
    $(".sel").each(function (index) {
        var tr = $(this);
        if (tr.hasClass('order')) {
            count++;
        }
    });
    if (count == 0)
        return;
    if (confirm('确实要标记删除选定的 ' + count + ' 个订购记录?') == false)
        return;
    var deleted = false;
    $(".sel").each(function (index) {
        var tr = $(this);
        if (tr.hasClass('order')) {
            var biblio_recpath = tr.attr('biblio-recpath');
            var refid = tr.attr('ref-id');
            tr.after(window.external.deleteOrder(biblio_recpath, refid));

            tr.detach();

            deleted = true;
        }
        else {
            // 删除下属全部订购记录
        }

    });
    if (deleted)
        window.external.onSelectionChanged();
}

function changeOrder(xml) {

    var changed = false;
    $(".sel").each(function (index) {
        var tr = $(this);
        if (tr.hasClass('order')) {
            var biblio_recpath = tr.attr('biblio-recpath');
            var refid = tr.attr('ref-id');
            tr.after(window.external.changeOrder(biblio_recpath, refid, xml));

            var new_tr = tr.next();
            tr.detach();
            setSel(new_tr);

            changed = true;
        }
        else {
        }

    });

}


function onDisButtonClick(o) {
    var td = $(o).parent();
    var tr = td.parent();

    var biblio_recpath = tr.attr('biblio-recpath');
    var refid = tr.attr('ref-id');
    var result = window.external.editDistribute(biblio_recpath, refid);
    if (result == null)
        return;

    var text_td = td.prev();
    // alert(text_td.html());
    text_td.html(result.replace(';', ';<br/>'));
    text_td.addClass('changed');

    displayError(text_td, null);
}

function onRangeButtonClick(o) {
    var td = $(o).parent();
    var tr = td.parent();

    var biblio_recpath = tr.attr('biblio-recpath');
    var refid = tr.attr('ref-id');
    var result = window.external.editRange(biblio_recpath, refid);
    if (result == null)
        return;

    var text_td = td.prev();
    // alert(text_td.html());
    text_td.html(result);
    text_td.addClass('changed');

    displayError(text_td, null);
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

function getSelectionCount() {
    var count_order = 0;
    var count_biblio = 0;
    $(".sel").each(function (index) {
        var tr = $(this);
        if (tr.hasClass('order'))
            count_order++;
        else
            count_biblio++;

    });
    return { order: count_order, biblio: count_biblio };
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

function selectAllBiblioHasOrder(clearBefore) {
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
            var biblio_recpath = tr.attr('biblio-recpath');
            var order_count = window.external.getOrderCount(biblio_recpath);
            if (order_count > 0) {
                if (setSelIf(tr) == true)
                    changed = true;
            }
            else {
                if (clearBefore) {
                    if (clearSelIf(tr) == true)
                        changed = true;
                }
            }
        }
    });
    if (changed == true)
        window.external.onSelectionChanged();
}

function selectAllBiblioNoOrder(clearBefore) {
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
            var biblio_recpath = tr.attr('biblio-recpath');
            var order_count = window.external.getOrderCount(biblio_recpath);
            if (order_count == 0) {
                if (setSelIf(tr) == true)
                    changed = true;
            }
            else {
                if (clearBefore) {
                    if (clearSelIf(tr) == true)
                        changed = true;
                }
            }
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

function onChanged(o) {
    var td = $(o).parent();
    var tr = $(o).parent().parent();
    var refid = tr.attr('ref-id');
    var biblio_recpath = tr.attr('biblio-recpath');
    var col_name = $(o).attr('col-name');
    var selected = $(o).val();
    // alert("Handler for .change() called. col_name=" + col_name +",value="+selected + ",refid=" +refid + ",biblio_recpath=" + bilbio_recpath);
    window.external.onOrderChanged(biblio_recpath,
        refid,
        col_name,
        selected);
    $(o).addClass('changed');
    tr.addClass('changed');

    if (col_name == 'copy') {
        var result = window.external.verifyDistribute(biblio_recpath, refid);

        var dis_td = tr.children('.dis-text');
        displayError(dis_td, result);
    }
}

function onClicked(o) {
    var tr = $(o);
    if (window.event.ctrlKey) {
        toggleSel(tr);
    }
    else {
        clearAllSel();
        setSel(tr);
        // GetPosition(tr[0]);
    }
}

function onDoubleClicked(o) {
    var biblio_recpath = o.attr('biblio-recpath');
    if (biblio_recpath != null)
        window.external.loadBiblio(biblio_recpath);
}

function loadBiblio() {
    var count = 0;
    $(".sel").each(function (index) {
        var tr = $(this);
        if (tr.hasClass('order')) {

        }
        else {
            if (count < 10) {
                var biblio_recpath = tr.attr('biblio-recpath');
                window.external.LoadBiblio(biblio_recpath);
                count++;
            }
        }
    });
}

function selectOrders(refid_list, scroll) {
    var changed = false;
    // 先清除所有书目行的选择
    $(".biblio.check.sel").each(function (index) {
        var tr = $(this);
        tr.removeClass('sel');
        changed = true;
    });

    var refids = refid_list.split('|');

    $(".order.check").each(function (index) {
        var tr = $(this);
        var refid = tr.attr('ref-id');
        if (jQuery.inArray(refid, refids) == -1) {
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
    if (changed == true)
        window.external.onSelectionChanged();
}

function getSelection() {
    var result = "";
    $(".order.sel").each(function (index) {
        var tr = $(this);
        var order_refid = tr.attr('ref-id');
        result += order_refid + "|";
    });
    return result;
}

function setErrorInfo(refid, text) {
    $(".order").each(function (index) {
        var tr = $(this);
        var order_refid = tr.attr('ref-id');
        if (order_refid == refid) {
            tr.before("<tr class='error-info'><td colspan='14' ><div class='error-text'>" + text + "</div></tr></tr>");
        }
    });
}

function clearAllErrorInfo() {
    $(".error-info").each(function (index) {
        var tr = $(this);
        // tr.html('...');
        tr.detach();
    });
}

function removeSelectedBiblio() {
    var biblios = new Array();
    var count = 0;
    var order_changed_count = 0;
    $(".sel").each(function (index) {
        var tr = $(this);
        if (!tr.hasClass('order')) {
            biblios[count++] = tr;

            var biblio_recpath = tr.attr('biblio-recpath');
            order_changed_count += window.external.getOrderChangedCount(biblio_recpath);
        }
    });

    if (count == 0)
        return;

    if (order_changed_count > 0) {
        if (confirm('确实要移除删除选定的 ' + count + ' 个书目记录? 注意这些书目记录下面有 ' + order_changed_count + ' 个订购记录在内存中修改过尚未保存，移除书目记录将丢失这些修改。') == false)
            return;
    }
    else {
        if (confirm('确实要移除删除选定的 ' + count + ' 个书目记录?') == false)
            return;
    }

    var deleted = false;
    if (biblios.length > 0) {
        for (var i = biblios.length - 1 ; i >= 0; i--) {
            var tr = biblios[i];
            var biblio_recpath = tr.attr('biblio-recpath');
            window.external.removeBiblio(biblio_recpath);
            // 一直移除所有订购信息到末尾
            //alert("before " + i);
            removeBiblio(tr);
            deleted = true;
        }
    }
    if (deleted) {
        //alert("before selectionChanged");
        window.external.onSelectionChanged();
    }
}

function removeBiblio(tr) {
    var trs = new Array();
    var i = 0;
    trs[i++] = tr;
    tr = tr.next();
    while (tr.length != 0) {
        if (tr == null || tr.length == 0)
            break;
        if (tr.hasClass('biblio'))
            break;
        trs[i++] = tr;
        tr = tr.next();
    }

    if (trs.length > 0) {
        for (var j = trs.length - 1 ; j >= 0; j--) {
            trs[j].detach();
        }
    }
}