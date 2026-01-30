function resultAlert(result, redirect = null) {
    let swalType = "warning";
    if (typeof result?.isSuccess === "boolean") {
        swalType = result.isSuccess ? "success" : "error";
    }
    const text = result?.message || (result?.messages || ["Bir hata oluştu..."]).join("<br>");
    Swal.fire({
        icon: swalType,
        html: text,
        //timer: 2000,
        timerProgressBar: true,
        showConfirmButton: true,
        confirmButtonText: 'OK',
        didDestroy: () => {
            if (redirect) {
                if (redirect === true) {
                    return window.location.reload();
                }
                window.location.href = redirect;
            }
        }
    });
}

function infoAlert(message, redirect = null) {
    Swal.fire({
        icon: 'info',
        html: message,
        //timer: 2000,
        timerProgressBar: true,
        showConfirmButton: true,
        confirmButtonText: 'OK',
        didDestroy: () => {
            if (redirect) {
                if (redirect === true) {
                    return window.location.reload();
                }
                window.location.href = redirect;
            }
        }
    });
}

function confirmAlert(onSuccess, onError = null, text = "") {
    Swal.fire({
        title: 'Emin misiniz?',
        text: text,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Evet',
        cancelButtonText: 'Hayır',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            onSuccess();
        } else if (result.dismiss === Swal.DismissReason.cancel) {
            onError && onError();
        }
    });
}

function confirmLinkAlert(url) {
    confirmAlert(() => {
        window.location.href = url;
    });
}

$.ajaxSetup({crossDomain: true});

$(document).ready(function () {
    $('th[data-sort]').on('click', function () {
        const url = new URL(window.location.href);
        const sort = $(this).data('sort');
        url.searchParams.set('sort', sort);
        window.location.href = url.href;
    });
});