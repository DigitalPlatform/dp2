/* last-modified: 2014-9-19 */

$(document).ready(function () {
    if (window.external == null) {
        alert("window没有联接external");
        return;
    }
    var external = window.external;
    external.IsInLoop = true;
    GetSummary();
    GetImage();
});


function GetImageCallBack(o, result) {
    o.src = result;
}


function GetImage() {
    try {
        if (window.external == null) {
            // alert("window没有联接external");
            return;
        }

        $("IMG.pending").each(function (index) {
            var path = this.name;

            // window.external.AsyncGetObjectFilePath(path, "", "GetImageCallBack");
            window.external.AsyncGetObjectFilePath(path, "", "GetImageCallBack", this);

        });
    }
    catch (e) {

    }
}

function GetPatronSummaryCallBack(o, result) {
    o.innerHTML = result;
    $(o).removeClass("pending");
}


function GetSummaryCallBack(o, result) {
    o.innerHTML = "<div class='wide'></div>" + result;
    $(o).removeClass("pending");
    if (result.indexOf("<img") != -1)
        window.setTimeout("GetImage()", 100);
}



function GetSummary() {

    if (window.external == null) {
        alert("window没有联接external");
        return;
    }

    try {
        var external = window.external;

        if (external.IsInLoop == false)
            return;

        var oo = $("TD.pending:first,DIV.pending:first");   // TD
        if (oo.length == 0) {
            return;
        }

        o = oo[0];

        var path = o.innerText;

        o.innerHTML = "<div class='wide'></div>" + "<img src='./servermapped/images/ajax-loader.gif'></img>";

        var prefix = "";
        var nRet = path.indexOf(":");
        if (nRet != -1) {
            prefix = path.substring(0, nRet);
            path = path.substr(nRet + 1);
        }

        try {
            if (prefix == "P")
                external.AsyncGetPatronSummary(path, "GetPatronSummaryCallBack", o);
            else
                external.AsyncGetSummary(prefix == "" ? path : prefix + ":" + path, false, "GetSummaryCallBack", o);
        }
        catch (e) {
            o.innerHTML = path + "||######" + e;
        }

        oo.removeClass("pending");
        window.setTimeout("GetSummary()", 100);
    }
    catch (e) {

    }

}
