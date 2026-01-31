$('form').submit(function (event) {
    event.preventDefault();
    _loading.open();
    $.post({
        url: "/api/user-client-permissions/create",
        dataType: "json",
        contentType: "application/json",
        data: getFormJSON(this)
    }).done(res => {
        let url = null;
        if (res?.isSuccess) {
            url = "/user-client-permissions/" + $('input[name=clientId]').val();
        }
        resultAlert(res, url);
    }).fail(xhr => {
        resultAlert(xhr.responseJSON);
    }).always(() => {
        _loading.close();
    });
});

$("#back").click(function () {
    history.back();
});