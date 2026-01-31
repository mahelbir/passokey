const {startAuthentication} = SimpleWebAuthnBrowser;

function login() {
    _loading.open();

    $.post({
        url: `/api/auth/login/start`,
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify({clientId})
    }).done(async (startResponse) => {

        try {
            const credential = await startAuthentication({optionsJSON: startResponse.data.options});
            _loading.open();
            const queryParams = new URLSearchParams(window.location.search);
            $.post({
                url: `/api/auth/login/finish`,
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify({
                    clientId: clientId,
                    credential: credential,
                    state: queryParams.get("state") || null,
                    redirectUri: queryParams.get("redirectUri") || null
                })
            }).done((finishResponse) => {
                finishResponse.isSuccess && resultAlert(finishResponse, finishResponse.data.redirect);
            }).fail(xhr => {
                resultAlert(xhr.responseJSON);
            }).always(() => {
                _loading.close();
            });

        } catch (error) {
            resultAlert({message: "An error occurred while logging in"});
        }

    }).fail(xhr => {
        resultAlert(xhr.responseJSON);
    }).always(() => {
        _loading.close();
    });
}

$('.btn-passkey').on('click', login);
window.addEventListener('load', () => {
    window.document.hasFocus() && login();
});