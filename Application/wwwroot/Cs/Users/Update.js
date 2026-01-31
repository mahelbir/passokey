$('form').submit(function (event) {
    event.preventDefault();
    _loading.open();
    $.post({
        url: "/api/users/update",
        dataType: "json",
        contentType: "application/json",
        data: getFormJSON(this)
    }).done(res => {
        resultAlert(res);
    }).fail(xhr => {
        resultAlert(xhr.responseJSON);
    }).always(() => {
        _loading.close();
    });
});