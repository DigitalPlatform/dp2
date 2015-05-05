/* last-modified: 2011-9-8 */

$(document).ready(function () { Tasks(); });

function Tasks() {
    MoveWarningText();
    AdjustSize();
    // GetCardPhoto();
    $('div.warning').effect('pulsate', { times: 3 }, 500);
    var state = $('TR.content.state TD.value');
    if (state.text() != "") {
        $('TR.content.state').parent().parent().effect('shake', { times: 10 }, 500);
    }
}

function MoveWarningText() {
    var frame = $('#warningframe');
    if (frame.Length != 0) {
        frame.appendTo($('#insertpoint'));
    }
}

function AdjustSize() {
    try {
        var max_height = 0;
        $(".readerinfo").each(function (index) {
            // alert(this.clientHeight);
            var height = parseInt(this.clientHeight);
            if (height > max_height)
                max_height = height;
        });
        $(".readerinfo").each(function (index) {
            this.style.height = max_height + "px";
        });
    }
    catch (e) {
    }
}

function GetCardPhoto() {
    try {
        if (window.external == null) {
            // alert("window没有联接external");
            return;
        }

        var photo = document.getElementById('cardphoto');
        if (photo != null) {
            var barcode = photo.name;
            var localpath = window.external.GetObjectFilePath(barcode, "cardphoto");
            if (localpath != null)
                photo.src = localpath;
            else
                photo.src = "";
        }
    }
    catch (e) {

    }
}

var hover_id;
var hover_string = "";

function OnHover(s) {
    window.clearTimeout(hover_id);
    hover_string = s;
    hover_id = window.setTimeout(DoHover, 10);
}

function DoHover() {
    try {
        window.external.HoverItemProperty(hover_string);
    }
    catch (e) {
    }
}

