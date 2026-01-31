const {startRegistration} = SimpleWebAuthnBrowser;

$('form').submit(function (event) {
    event.preventDefault();
    _loading.open();

    $.post({
        url: `/api/auth/registration/start`,
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify({
            clientId: clientId,
            username: $('input[name=username]').val() || null
        })
    }).done(async (startResponse) => {

        try {
            const credential = await startRegistration({optionsJSON: startResponse.data.options});
            _loading.open();
            $.post({
                url: `/api/auth/registration/finish`,
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify({
                    clientId: clientId,
                    credential: credential
                })
            }).done((finishResponse) => {
                if (finishResponse.isSuccess) {
                    const loginUrl = new URL(window.location.href);
                    loginUrl.pathname = `/auth/login/${clientId}`;
                    loginUrl.search = window.location.search;
                    resultAlert(finishResponse, loginUrl.toString());
                }
            }).fail(xhr => {
                resultAlert(xhr.responseJSON, true);
            }).always(() => {
                _loading.close();
            });

        } catch (error) {
            resultAlert({message: "Error creating credentials"});
        }

    }).fail(xhr => {
        resultAlert(xhr.responseJSON);
    }).always(() => {
        _loading.close();
    });
});