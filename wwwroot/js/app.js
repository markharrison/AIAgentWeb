function randomString(length) {
    var chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXTZabcdefghiklmnopqrstuvwxyz".split("");

    if (!length) {
        length = Math.floor(Math.random() * chars.length);
    }

    var str = "";
    for (var i = 0; i < length; i++) {
        str += chars[Math.floor(Math.random() * chars.length)];
    }
    return str;
}

function doToast(vTitle, vText) {
    var vHtml = "";
    var vId = "idToast" + randomString(8);
    var vDate = new Date();
    var vTime = vDate.toLocaleTimeString();

    if ($('#idToaster').length === 0) {
        $('body').append('<div id="idToaster" style="background: transparent; position: fixed; bottom: 45px; right: 5px; z-index: 2000; width: 300px;"></div>');
    }

    vHtml = vHtml + "<div id='" + vId + "' class='toast fade hide'><div class='toast-header'>";
    vHtml = vHtml + "<strong class='mr-auto'>" + vTitle + "</strong><small class='text-muted'>&nbsp;" + vTime + "</small>";
    vHtml = vHtml + "<button type='button' class='btn-close ms-auto mb-1' data-bs-dismiss='toast' ></button>";
    vHtml = vHtml + "</div><div class='toast-body'>" + vText + "</div></div>";

    $("#idToaster").append(vHtml);
    $("#" + vId).on("hidden.bs.toast", function (event) {
        $("#" + event.currentTarget.id).remove();
    });
    $("#" + vId).toast({ delay: 10000 }).toast("show");
}

function setCookie(strCookieName, strCookieValue) {
    var myDate = new Date();
    myDate.setMonth(myDate.getMonth() + 12);
    document.cookie = strCookieName + "=" + strCookieValue + ";expires=" + myDate;
}

function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length === 2) return parts.pop().split(";").shift();
}


$(document).ready(function () {



});
