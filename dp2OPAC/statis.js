
$(function () {
    var activetab = $('#HiddenField_activetab').val();

    $('#tabs').tabs({ select: function (event, ui) {
        if (ui.index == 0)
            $('#HiddenField_activetab').val("day");
        else {
            $('#HiddenField_activetab').val("range");
        }

    }
    });

    if (activetab == "range")
        $('#tabs').tabs("select", 1);

    $("tr.content td.day").hover(
    function (event) {
        // var text = $(this).text();
        var title = $(this).parent().children(".name").text();
        var index = $(this).parent().children().index(this);
        var c_td = $(this).parent().parent().children().last().children().eq(index);
        var day = c_td.children().eq(0).text() + c_td.children().eq(1).text() + "/" + $.trim(c_td.children().eq(2).text()) + "/" + $.trim(c_td.children().eq(3).text());

        var tips = $("#tips");

        tips.empty();
        tips.append("<div>" + title + "</div><div>" + day + "</div>");
        tips.show();
        var win = $(window);

        tips.css({ top: event.clientY + win.scrollTop() + 10, left: event.clientX + win.scrollLeft() + 10 });
    },

    function (event) {
        $("#tips").hide();
    }

    );
});


$(document).ready(function () {
    window.setTimeout(SetTableSize, 100);
});

function SetTableSize() {
    var right = $("div.statisframe").parent().parent().parent()[0];
    var chart = $("div.statisframe")[0];
    chart.style.width = right.clientWidth + "px";
}