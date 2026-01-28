function getFormData(selector) {
    return $(selector).serializeArray();
}

function getForm(selector) {
    const array = getFormData(selector);
    const obj = {};
    $.each(array, function () {
        obj[this.name] = this.value ?? "";
    });
    return obj;
}

function getFormJSON(selector) {
    return JSON.stringify(getForm(selector));
}

function getOptionText(selector) {
    const elem = typeof selector === "string" ? $(selector) : selector;
    return elem.find("option:selected").text();
}

function getOptionData(selector, dataKey) {
    const elem = typeof selector === "string" ? $(selector) : selector;
    return elem.find("option:selected").data(dataKey);
}

function newUrl(url) {
    return new URL(url, window.location.origin);
}

function redirect(url) {
    window.location.href = url;
}

function getQueryParam(name, url = window.location.href) {
    const params = new URL(url).searchParams;
    return params.get(name);
}

function generateUUID() {
    var d = new Date().getTime();
    var d2 = ((typeof performance !== 'undefined') && performance.now && (performance.now() * 1000)) || 0;
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16;
        if (d > 0) {
            r = (d + r) % 16 | 0;
            d = Math.floor(d / 16);
        } else {
            r = (d2 + r) % 16 | 0;
            d2 = Math.floor(d2 / 16);
        }
        return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}

function setCookie(name, value, expireDate = null) {
    let expires = "";
    if (expireDate) {
        expires = "; expires=" + expireDate.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

function deleteCookie(name) {
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
}

function debounce(func, wait = 300) {
    let timeout;
    return function (...args) {
        const context = this;
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(context, args), wait);
    };
}