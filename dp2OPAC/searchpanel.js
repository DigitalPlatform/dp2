

$(document).ready(function () {
    LoadLists();
});

function LoadLists() {

    $("select.dbname").each(function (index) {
        getlist(this);
    });
}


function getlist(obj) {
    var xhr = $.ajax({
        url: 'searchbiblio.aspx?action=getdblist',
        cache: false,
        statusCode: {
            500: function () {
                $(obj).append('\r\nerror 500');
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
                // possible login page
                $(obj).append('\r\getdblist exception :' + e.description);
                return;
            }

            if (o.ErrorString.length > 0) {
                $(obj).append("error: " + o.ErrorString + "\r\n");
                return;
            }

            if (o.ResultText.length == 0) {
                // $('#message').append('.');
            }
            else {
                $(obj).append($(o.ResultText));
            }
        }
    });
}