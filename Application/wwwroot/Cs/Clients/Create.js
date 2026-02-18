renderRedirectUriList('redirectUriList', redirectUriList);

$('form').submit(function (event) {
    event.preventDefault();
    _loading.open();
    const data = getForm(this);
    data.redirectUriList = collectRedirectUriList('redirectUriList');
    $.post({
        url: "/api/clients/create",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(data)
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