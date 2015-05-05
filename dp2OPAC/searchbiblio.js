var treetop = 0;

$(window).load(function () {
    $(window).scroll(afterScroll);
    $(window).resize(afterResize);
});

function afterResize() {
    $("#BrowseSearchResultControl1").outerWidth($("#BrowseSearchResultControl1").parent().innerWidth() - $("#filter").outerWidth(true) - 2);

    afterScroll();
}

function afterScroll() {
    var scrollTop = $(window).scrollTop();
    var searchtop = $('#BiblioSearchControl1').offset().top;

    var filter = $('#filter');
    if (scrollTop > treetop - 30) {
        $('#filter').css('position', 'fixed');
        $('#filter').css('top', 0);
        var x = (filter.innerWidth() - filter.width()) / 2 + 3;
        $('#filter').css('left', x);    // for IE 8 / IE 7
        var maxheight = Math.max(0, searchtop - scrollTop - 40);
        $('#filter').css('height', Math.min($(window).height() - 40, maxheight));
    }
    else {
        $('#filter').css('position', 'relative');
        $('#filter').css('top', 0);
        $('#filter').css('left', 0);    // for IE 8 / IE 7
        $('#filter').css('height', $(window).height() - treetop - 50);
    }
}

$(document).ready(function () {
    $("#BrowseSearchResultControl1").outerWidth($("#BrowseSearchResultControl1").parent().innerWidth() - $("#filter").outerWidth(true) - 2);

    window.setTimeout("GetFilterInfo()", 100);

    if ($('#filter').length > 0)
        treetop = $('#filter').offset().top;

    $('#filter').css('height', $(window).height() - treetop - 50);
});

function onslide(value) {
    var v = parseInt(value);
    value = Math.floor(v / 10) * 10;
    // value = value - 1;
    var resultset = $(this.inputNode).data("resultset");
    // alert(value + "," + count);
    $("#filter_selected-data").val(resultset + "," + value);

    /*
    window.setTimeout(function () {
        $(this.inputNode).prepend("<img src='./style/ajax-loader.gif'></img>");
    },
    100);
    */


    $("#filter_button").trigger("click");
}


function GetFilterInfo() {

    var filter = $("#filter");
    var resultsetname = $("#filter_resultsetname").val();

    if (resultsetname == "" || resultsetname === undefined) {
        filter.hide();
        $("#BrowseSearchResultControl1").outerWidth($("#BrowseSearchResultControl1").parent().innerWidth());
        return;
    }

    var selected = $("#filter_selected-data").val();
    var lang = $('select#langlist option[SELECTED]').attr('value');

    // filter.show();

    var xhr = $.ajax({
        url: 'filter.aspx?action=getfilterinfo&resultset=' + encodeURIComponent(resultsetname) + "&selected=" + encodeURIComponent(selected) + "&lang=" + encodeURIComponent(lang),
        cache: false,
        statusCode: {
            500: function () {
                filter.append('\r\nerror 500');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            filter.append('\r\nerror :' + textStatus);
        },
        success: function (data) {
            var o = null;
            try {
                o = eval('(' + data + ')');
            }
            catch (e) {
                filter.append('\r\getfilterinfo exception :' + e.description);
                return;
            }

            if (o.ErrorString.length > 0) {
                if (o.ErrorString == "#pending") {
                    $("#filter_progressbar").progressbar({ value: o.ProgressValue });
                    window.setTimeout("GetFilterInfo()", 1000);
                    return;
                }

                filter.append("error: " + o.ErrorString + "\r\n");
                return;
            }

            $("#filter_progressbar").hide();

            if (o.Comment != "")
                filter.append("<div class='comment'>" + o.Comment + "</div>");

            for (var i = 0; i < o.Items.length; i++) {
                var item = o.Items[i];

                var selectedclass = "";
                if (item.Selected == true)
                    selectedclass = " selected";

                filter.append("<div class='l1" + selectedclass + "'><a href='" + item.Url + "'>" + item.Name + "<span class='count'>" + item.Count + "</span>" + "</a></div>");
                var children = item.Children;
                if (children != null) {
                    for (var j = 0; j < children.length; j++) {
                        var subitem = children[j];

                        selectedclass = "";
                        if (subitem.Selected == true)
                            selectedclass = " selected";

                        var index = "";
                        if (subitem.Index != "")
                            index = "<span class='index'>" + subitem.Index + "</span>"

                        if (subitem.Type == "nav")
                            filter.append("<div class='l2" + selectedclass + "'><a class='nav' href='#' data-url='" + subitem.Url + "' >" + index + subitem.Name + "</a></div>");
                        else
                            filter.append("<div class='l2" + selectedclass + "'><a href='" + subitem.Url + "'>" + index + subitem.Name + "<span class='count'>" + subitem.Count + "</span>" + "</a></div>");
                    }

                    var a = item.Type.split(",");   // subresultsetname,count,start
                    var subresultset = "";
                    var start = "0";
                    var count = "0";
                    if (a.length >= 1)
                        subresultset = a[0];
                    if (a.length >= 2)
                        count = a[1];
                    if (a.length >= 3)
                        start = a[2];
                    if (count > o.PageSize)
                        filter.append("<div class='l2 sliderframe'><input class='slider' type='slider' value='" + (parseInt(start) + 1) + "' data-count='" + count + "' data-resultset='" + subresultset + "'/></div>");

                }
            }

            $(".slider").each(function (index) {
                var count = $(this).data("count");
                /*
                var lines = parseInt(count);
                while(lines > 10)
                    lines = Math.floor(lines / 10);
                var a = new Array();
                for (var i = 0; i < lines + 1; i++) {
                    a[i] = '|';
                }
                */
                $(this).slider({
                    from: 1,
                    to: count,
                    limit: true,
                    step: 1,
                    round: 1,
                    calculate: function (value) {
                        var v = parseInt(value);
                        return Math.floor(v / 10) * 10 + 1;
                    },
                    // scale: a,
                    callback: onslide,
                    dimension: '&nbsp;',
                    skin: "round"
                });
            });

            $("#filter .nav").click(function () {
                $("#filter_selected-data").val($(this).data("url"));
                $("#filter_button").trigger("click");
            });

            window.setTimeout(function () {
                ScrollIntoView($("#filter"), $(".selected"));
            }, 10);

        }
    });

}

function SetFilterParam(obj) {
    // alert($(obj).data("url"));
    alert($(obj).attr("data-url"));
    $("#filter_selected-data").val($(obj).data("url"));
}