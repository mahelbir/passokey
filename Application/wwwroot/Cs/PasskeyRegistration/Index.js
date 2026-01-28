const {startRegistration} = SimpleWebAuthnBrowser;

$('form').submit(function (event) {
    event.preventDefault();
    _loading.open();

    $.post({
        url: `/auth/registration/start/${clientId}`,
        dataType: "json",
        contentType: "application/json",
        data: getFormJSON(this)
    }).done(async (startResponse) => {

        try {
            const credential = await startRegistration({optionsJSON: startResponse.data.options});
            _loading.open();
            $.post({
                url: `/auth/registration/finish/${clientId}`,
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify({credential: credential})
            }).done((finishResponse) => {
                finishResponse.isSuccess && resultAlert(finishResponse, `/auth/login/${clientId}`);
            }).fail(xhr => {
                resultAlert(xhr.responseJSON, true);
            }).always(() => {
                _loading.close();
            });

        } catch (error) {
            resultAlert({message: "Error creating credentials"}, true);
        }

    }).fail(xhr => {
        resultAlert(xhr.responseJSON, true);
    }).always(() => {
        _loading.close();
    });
});