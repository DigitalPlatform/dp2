

$(document).ready(function () {
    if (window.external == null) {
        alert("window 没有联接 external");
        return;
    }
    var external = window.external;

    // setEvents();
});

function toggleSel(o) {
    o.toggleClass('sel');
    //window.external.onSelectionChanged();
}

function setSel(o) {
    o.addClass('sel');
    //window.external.onSelectionChanged();
}

function clearAllSel() {
    var changed = false;
    $(".check.sel").each(function (index) {
        var tr = $(this);
        tr.removeClass('sel');
        changed = true;
    });
    //if (changed == true)
    //    window.external.onSelectionChanged();
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
    //if (changed == true)
    //    window.external.onSelectionChanged();
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
    //if (changed == true)
    //    window.external.onSelectionChanged();
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


