renderRedirectUriList('redirectUriList', redirectUriList);

$('form').submit(function (event) {
    event.preventDefault();
    _loading.open();
    const data = getForm(this);
    data.redirectUriList = collectRedirectUriList('redirectUriList');
    $.post({
        url: "/api/clients/update",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(data)
    }).done(res => {
        resultAlert(res);
    }).fail(xhr => {
        resultAlert(xhr.responseJSON);
    }).always(() => {
        _loading.close();
    });
});

$('#changeSecretKey').click(function () {
    confirmAlert(() => {
        _loading.open();
        const id = $('input[name=id]').val();
        $.post({
            url: "/api/clients/change-secret-key/" + id,
            dataType: "json",
            contentType: "application/json"
        }).done(res => {
            if (res?.data?.secretKey) {
                $('#secretKey').val(res.data.secretKey);
            }
            resultAlert(res);
        }).fail(xhr => {
            resultAlert(xhr.responseJSON);
        }).always(() => {
            _loading.close();
        });
    })
});