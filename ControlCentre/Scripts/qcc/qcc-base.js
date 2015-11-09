
window.qcc = window.qcc || {};

window.qcc.configuration = window.qcc.configuration || {};

window.qcc.serialize = function (members, port, responseLimit) {
    return [members, port, responseLimit].join("|");
};

window.qcc.deserialize = function () {
    if (typeof (Storage) === "undefined")
        alert('This browser does not support local storage');
    else {
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
    }
};

window.qcc.save = function (cfg) {
    if (typeof (Storage) === "undefined")
        alert('This browser does not support local storage');
    else {
        localStorage.setItem('qcc', window.qcc.serialize(cfg.members, cfg.port, cfg.responseLimit));
    }
};