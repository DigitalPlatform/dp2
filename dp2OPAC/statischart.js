
$(document).ready(function () {
    window.setTimeout(SetImageSize, 100);
    $("#TreeView1").treeview();


    window.setTimeout(function () {
        ScrollIntoView($("UL#TreeView1"), $(".selected"));
    }, 0);

    window.setTimeout(function () {
        ScrollIntoView($(".statisentry"), $(".checked"));
    }, 0);
});

function SetImageSize() {
    var oldwidth = $("#HiddenField_imageWidth").val();

    if (oldwidth == "") {
        //var right = $("#Chart1").parent()[0];
        //$("#HiddenField_imageWidth").val(right.clientWidth - 16);
        var width = $("#Chart1").parent().width();
        var height = $("UL#TreeView1").parent().height() + $(".statisentry").parent().height() - $("#DropDownList_chartType").height();
        $("#HiddenField_imageWidth").val(width + "," + height);

        if (oldwidth == "") {
            // LangPostBack();
            $(".treebutton.hidden").trigger("click");
        }
    }
}

function SetImageSize0() {
    var right = $("#Chart1").parent()[0];
    var chart = document.getElementById("Chart1");
    var width = right.clientWidth - 16;
    chart.style.height = Math.round(width * 0.46) + "px";
    // chart.style.height = "460px";
    chart.style.width = width + "px";
}

function SetImageSize1() {

    var header_height = 0;
    if ($('TABLE.title:visible').length > 0)
        header_height = $('TABLE.title:visible').height();
    var columnbar = $('TABLE.columnbar')[0];

    var left = $("td.left")[0];

    var message = document.getElementById("Chart1");
    // message.style.height = (document.documentElement.clientHeight - header_height - columnbar.clientHeight - 30) + "px";
    message.style.width = (document.documentElement.clientWidth - left.clientWidth - 40 - 16) + "px";
}