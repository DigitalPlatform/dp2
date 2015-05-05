var treetop = 0;
var treeleft = 0;
var min_height = 0;

$(document).ready(function () {
    /*
    $.getJSON(
    'report.aspx?action=gettreedata',
    function (data) {
    $('#tree1').tree({
    data: data
    });
    }
    );
    */
    var $tree = $('#tree1');

    if ($tree.length > 0) {
        treetop = $tree.offset().top;
        treeleft = $tree.offset().left;
    }

    var menuAPI = $tree.jqTreeContextMenu($('#myMenu'), {
        "downloadExcel": function (node) {
            // alert('Edit node: ' + node.name);
            if (node.url != null) {
                $('#report_view').contents().find('html').html("<h1 style='text-align: center;'>下载 '" + node.url + "' 对应的 Excel 文件</h1>");
                $("#report_view").attr("src", "report.aspx?file=" + encodeURIComponent(node.url) + "&format=excel");
            }
            else {
                // 打包下载
                alert("请选择一个文件节点进行下载");
            }
        },
        "delete": function (node) {
            alert('Delete node: ' + node.name);
        },
        "add": function (node) {
            alert('Add node: ' + node.name);
        }
    });

    $tree.bind(
    'tree.select',
    function (event) {
        if (event.node) {

            var $tree = $('#tree1');

            // node was selected
            var node = event.node;

            /*
            // fill dir node
            if (node.start != null) {
            $tree.tree('loadDataFromUrl',
            'report.aspx?action=gettreedata&start=' + encodeURIComponent(node.start),
            node);
            }
            */

            // open node
            $tree.tree('openNode', node);

            if (node.url != null) {

                menuAPI.enable(['downloadExcel'])

                $('#report_view').contents().find('html').html("<h1 style='text-align: center;'>" + node.url + "</h1>");

                $("#report_view").attr("src", "report.aspx?file=" + encodeURIComponent(node.url));
            }
            else {
                menuAPI.disable(['downloadExcel'])
                // $("#report_view").attr('src', 'about:blank');
                $('#report_view').contents().find('html').html("<h1 style='text-align: center;'>" + node.name + "</h1><h3 style='text-align: center;'>请选择一个下级节点</h3>");

                $('iframe')[0].style.height = min_height + 'px';
                $(window).scrollTop(0);
            }
            // alert(node.url);
        }
        else {
            // event.node is null
            // a node was deselected
            // e.previous_node contains the deselected node
        }
    });

    /*
    $tree.bind(
    'tree.contextmenu',
    function (event) {
    var node = event.node;
    alert(node.name);
    });
    */

    $tree.tree({
        onLoadFailed: function (response) {
            // alert(response);
            if (response.status == 403) {
                window.location.replace("./report.aspx");
            }
        },
        autoOpen: 2,
        usecontextmenu: true
    });


    window.setTimeout(SetViewSize, 100);
    window.setTimeout(function () { KeepAlive(); }, 1000 * 60);

    $('iframe').load(function () {
        //var $frame = $(this.contentWindow.document.body);
        //var delta = $frame.outerWidth() - $frame.innerWidth();
        this.style.height = Math.max((this.contentWindow.document.body.offsetHeight + 60), min_height) + 'px';
        $(window).scrollTop(0);
    });
});



function ensureVisible() {
    // $('#tree1').scrollTop($('.jqtree-selected').top);
    var $tree = $('#tree1');

    var node = $tree.tree('getSelectedNode');
    if (node != false)
        $tree.tree('scrollToNode', node);

    /*
    $element = $('.jqtree-selected .jqtree-title');
    $element.css('background-color', 'red');
    ScrollIntoView($tree, $element);
    */
}



$(window).load(function () {
    $(window).scroll(afterScroll);
    $(window).resize(afterResize);
});

function afterResize() {
    // $("#report_view").outerWidth($("#report_view").parent().innerWidth() - $("#tree1").outerWidth(true) - 2);

    afterScroll();
}

var save_height = 0;
var mode = "fix";

function afterScroll() {
    var scrollTop = $(window).scrollTop();
    var searchtop = $('table.footer').offset().top;

    var $tree = $('#tree1');
    if (scrollTop > treetop - 30) {

        if (mode == "fix") {
            if ($tree.css('position') != 'fixed')
                $tree.css('position', 'fixed');
            if ($tree.css('top') != 0)
                $tree.css('top', 0);
            // var x = ($tree.innerWidth() - $tree.width()) / 2 + 3;
            if ($tree.css('left') != treeleft)
                $tree.css('left', treeleft);    // for IE 8 / IE 7
            mode = "float";
        }
        var maxheight = Math.max(0, searchtop - scrollTop - 20);
        var new_height = Math.min($(window).height() - 40, maxheight);
        if (save_height != new_height) {
            $tree.css('height', new_height);
            save_height = new_height;
            // window.setTimeout(ensureVisible, 100);
            ensureVisible();
            //alert('height change 1');
        }
    }
    else {
        $tree.css('position', 'relative');
        $tree.css('top', 0);
        $tree.css('left', 0);    // for IE 8 / IE 7
        var new_height = $(window).height() - treetop - 20;
        if (save_height != new_height) {
            $tree.css('height', new_height);
            save_height = new_height;
            // window.setTimeout(ensureVisible, 100);
            ensureVisible();
            //alert('height change 2');
        }
        mode = "fix";
    }

}


function KeepAlive() {
    var xhr = $.ajax({
        url: 'report.aspx?action=keepalive',
        cache: false,
        statusCode: {
            500: function () {
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
        },
        success: function (data) {
        }
    });
}

function SetViewSize() {

    var header_height = 0;
    if ($('TABLE.title:visible').length > 0)
        header_height = $('TABLE.title:visible').height();

    var height = $(window).height() - header_height - $('TABLE.columnbar').height() - 30 * 2;
    $("#report_view").height(height);

    $("#tree1").height(height);

    min_height = height;
}




