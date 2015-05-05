/* last-modified: 2011-9-8 */

$(document).ready(function () { GetAllSummary(); });

function GetAllSummary() {
    if (window.external == null) {
        alert("window没有联接external");
        return;
    }

    try {
        var external = window.external;

        external.IsInLoop = true;

        $("TD.pending").each(function (index) {
            if (external.IsInLoop == false)
                return;

            var o = this;

            var path = o.innerText;

            o.innerHTML = "<img src='./servermapped/images/ajax-loader.gif'></img>";

            var prefix = "";
            var nRet = path.indexOf(":");
            if (nRet != -1) {
                prefix = path.substring(0, nRet);
                path = path.substr(nRet + 1);
            }

            try {
                if (prefix == "P")
                    o.innerHTML = external.GetPatronSummary(path);
                else
                    o.innerHTML = /*path + "||" + */"<div class='wide'><div>" + external.GetSummary(path, false);
            }
            catch (e) {
                o.innerHTML = path + "||######" + e;
            }

        });

        external.IsInLoop = false;
    }
    catch (e) {

    }

}
