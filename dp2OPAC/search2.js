
$(document).ready(function () {
    window.setTimeout("GetResultsetFrame()", 1);

    window.setTimeout("GetFilterInfo()", 1000);
});

function GetResultsetFrame() {

    var control = $('#ViewResultsetControl1');
    if (control.length == 0)
        return;

    var resultsetname = $("#ViewResultsetControl1_resultsetname").val();

    var xhr = $.ajax({
        url: 'search2.aspx?action=getresultsetframe&resultset=' + encodeURIComponent(resultsetname),
        cache: false,
        statusCode: {
            500: function () {
                control.append('\r\nerror 500');
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
                control.append('\r\getresultsetframe exception :' + e.description);
                return;
            }

            if (o.ErrorString.length > 0) {
                control.append("error: " + o.ErrorString + "\r\n");
                return;
            }

            control.append(o.Html);

            $(".bibliorecord").each(function (index) {

            });
        }
    });



}

function LoadOneBiblio(div) {


}

function GetFilterInfo() {

    var filter = $("#filter");
    var resultsetname = $("#resultsetname").val();
    if (resultsetname == "")
        return;

    var xhr = $.ajax({
        url: 'filter.aspx?action=getfilterinfo&resultset=' + encodeURIComponent(resultsetname),
        cache: false,
        statusCode: {
            500: function () {
                $('#filter').append('\r\nerror 500');
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
                $('#filter').append('\r\getfilterinfo exception :' + e.description);
                return;
            }

            if (o.ErrorString.length > 0) {
                if (o.ErrorString == "#pending") {
                    $("#progressbar").progressbar({ value: o.ProgressValue });
                    window.setTimeout("GetFilterInfo()", 1000);
                    return;
                }

                $('#filter').append("error: " + o.ErrorString + "\r\n");
                return;
            }

            $("#progressbar").hide();
            for (var i = 0; i < o.Items.length; i++) {
                filter.append("<div>" + o.Items[i].Name + "(" + o.Items[i].Count + ")" + "</div>");
            }
        }
    });

}