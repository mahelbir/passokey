let exampleUrl;
{
    const url = new URL(window.location.href);
    const baseUrl = url.protocol + '//' + url.host;
    exampleUrl = baseUrl + '/example/callback';
}

function renderRedirectUriList(containerId, list) {
    const container = $('#' + containerId);
    container.empty();

    list.forEach((uri, index) => {
        container.append(createUriRow(uri, index));
    });

    container.append(createUriRow('', list.length));
}

function createUriRow(value, index) {
    const row = $('<div class="input-group mb-1"></div>');
    const input = $(`<input type="url" class="form-control form-control-sm" placeholder="${exampleUrl}"/>`)
        .val(value);

    if (value) {
        const removeBtn = $('<div class="input-group-append"></div>')
            .append($('<button class="btn btn-outline-danger btn-sm" type="button"><i class="fas fa-times"></i></button>')
                .on('click', function () {
                    const container = row.parent();
                    const filledInputs = container.find('input').filter(function () {
                        return $(this).val().trim() !== '';
                    });
                    if (filledInputs.length <= 1) {
                        return resultAlert({message: "At least one redirect URI is required."});
                    }
                    row.remove();
                }));
        row.append(input).append(removeBtn);
    } else {
        input.on('input', function () {
            if ($(this).val().trim()) {
                const container = row.parent();
                row.remove();
                container.append(createUriRow($(this).val().trim(), container.children().length));
                container.append(createUriRow('', container.children().length));
            }
        });
        row.append(input);
    }

    return row;
}

function collectRedirectUriList(containerId) {
    const uris = [];
    $('#' + containerId).find('input').each(function () {
        const val = $(this).val().trim();
        if (val) uris.push(val);
    });
    return uris.length < 1 ? null : uris;
}