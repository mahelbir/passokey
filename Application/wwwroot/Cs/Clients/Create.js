$('form').submit(function (event) {
    event.preventDefault();
    _loading.open();
    $.post({
        url: "/api/clients/create",
        dataType: "json",
        contentType: "application/json",
        data: getFormJSON(this)
    }).done(res => {
        let url;
        if (res?.data?.id) {
            url = `/clients/update/${res.data.id}`;
        }
        resultAlert(res, url);
    }).fail(xhr => {
        resultAlert(xhr.responseJSON);
    }).always(() => {
        _loading.close();
    });
});