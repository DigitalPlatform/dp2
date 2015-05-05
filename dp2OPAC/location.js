$(function () {

    SetLayout();

    var image_width = $('#HiddenField_imageWidth').val();
    var image_height = $('#HiddenField_imageHeight').val();

    $('.map').craftmap({
        image: {
            width: image_width,
            height: image_height
        }
    });

});

function SetLayout() {

    var header_height = 0;
    if ($('TABLE.title:visible').length > 0)
        header_height = $('TABLE.title:visible').height();
    var columnbar = $('TABLE.columnbar')[0];

    var layout = document.getElementById("mapFrame");
    layout.style.height = (document.documentElement.clientHeight - header_height - columnbar.clientHeight - 30) + "px";
    layout.style.width = (document.documentElement.clientWidth - 30) + "px";
}