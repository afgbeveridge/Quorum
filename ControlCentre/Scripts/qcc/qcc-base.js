
window.qcc = window.qcc || {};

window.qcc.configuration = window.qcc.configuration || {};

window.qcc.serialize = function (members, port, responseLimit) {
    return [members, port, responseLimit].join("|");
};

window.qcc.withLocalStorage = function (f) {
    if (typeof (Storage) === "undefined")
        alert('This browser does not support local storage');
    else return f();
};

window.qcc.deserialize = function () {
    return window.qcc.withLocalStorage(function () {
        var stored = localStorage.getItem('qcc'), result;
        if (!stored)
            result = {
                members: null,
                port: 9999,
                responseLimit: 2000
            };
        else {
            var split = stored.split('|');
            result = {
                members: split[0],
                port: split[1],
                responseLimit: split[2]
            };
            result.membersArray = result.members.split(',');
        }
        return result;
    });
};

window.qcc.save = function (cfg) {
    window.qcc.withLocalStorage(function () {
        localStorage.setItem('qcc', window.qcc.serialize(cfg.members, cfg.port, cfg.responseLimit));
    });
};