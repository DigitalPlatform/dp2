

function insertHtml(content) {
    /*alert("into insertHtml()");*/
    $(".item").first().before(content);
    /*
    $(".item").each(function (index) {
        var item = $(this);
        item.before(content);
        return;
    });
    */
}