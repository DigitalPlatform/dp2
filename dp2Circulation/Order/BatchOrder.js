

$(document).ready(function () {
    if (window.external == null) {
        alert("window 没有联接 external");
        return;
    }
    var external = window.external;
    // external.IsInLoop = true;
    setEvents();
});

function setEvents() {
    $(".list.event").change(function () {
        $(this).addClass('changed');
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
        $(this).removeClass('event');
    });

    $(".check.event").click(function () {
        var tr = $(this);
        if (window.event.ctrlKey) {
            toggleSel(tr);
        }
        else {
            clearAllSel();
            setSel(tr);
        }
        $(this).removeClass('event');
    });
}

function toggleSel(o)
{
    o.toggleClass('sel');
}

function setSel(o)
{
    o.addClass('sel');
}

function clearAllSel()
{
    $(".check.sel").each(function (index) {
        var tr = $(this);
        tr.removeClass('sel');
    });
}

function newOrder()
{
    $(".sel").each(function (index) {
        var tr = $(this);
        if (tr.hasClass('order')) {
            var biblio_recpath = tr.attr('biblio-recpath');
            tr.after(window.external.newOrder(biblio_recpath));
        }
        else {
            var biblio_recpath = tr.attr('biblio-recpath');
            tr = tr.next();
            tr.after(window.external.newOrder(biblio_recpath));
        }
    });

    setEvents();
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
    if (confirm('确实要删除选定的 ' + count + ' 个订购记录?') == false)
        return;
    $(".sel").each(function (index) {
        var tr = $(this);
        if (tr.hasClass('order')) {
            var biblio_recpath = tr.attr('biblio-recpath');
            var refid = tr.attr('ref-id');
            tr.after(window.external.deleteOrder(biblio_recpath, refid));

            tr.detach();
        }
        else {
            // 删除下属全部订购记录
        }

    });


}

