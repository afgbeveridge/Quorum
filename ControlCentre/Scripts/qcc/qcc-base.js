
window.qcc = window.qcc || {};

window.qcc.configuration = window.qcc.configuration || {};

window.qcc.redirectLocation = "/QCC/Configuration";

window.qcc.serialize = function (cfg) {
    return ko.toJSON(cfg);
};

window.qcc.withLocalStorage = function (f) {
    if (typeof (Storage) === "undefined")
        alert('Browser local storage is required to use this web app');
    else return f();
};

window.qcc.deserialize = function () {
    return window.qcc.withLocalStorage(function () {
        var stored = localStorage.getItem('qcc'), result;
        if (!stored)
            result = {
                members: null,
                port: 9999,
                responseLimit: 6000,
                transportType: ko.observable('http')
            };
        else {
            result = JSON.parse(stored);
            result.transportType = ko.observable(result.transportType);
            result.members = result.members.split(',');
        }
        return result;
    });
};

window.qcc.deserializeWithCheck = function () {
    var config = qcc.deserialize();
    if (!config || !config.members || config.members.length == 0 || !config.members.every(function (n) { return n && n.length > 0; })) {
        alert('Please set up your configuration');
        window.location.assign(qcc.redirectLocation);
    }
    return config;
};

window.qcc.save = function (cfg) {
    window.qcc.withLocalStorage(function () {
        localStorage.setItem('qcc', window.qcc.serialize(cfg));
    });
};

window.qcc.ensureTransportTypeSet = function (cfg, cb) {
    $.getJSON("/QCC/SetTransportType?type=" + (ko.isObservable(cfg.transportType) ? cfg.transportType() : cfg.transportType), null, function () {
        if (cb) cb();
    })
    .fail(function () { alert("Could not change transport type"); });
};

window.qcc.queryMachines = function (mcs, config, onSuccess, failed, always) {
    $.ajax({
        url: '/Neighbourhood/QueryMachines',
        type: 'POST',
        data: JSON.stringify({ Machines: mcs, Port: config.port, Timeout: config.responseLimit }),
        contentType: 'application/json',
        success: function (data, textStatus, jqXHR) {
            console.log('Proceed...');
            onSuccess(data.machines);
        }
    })
        .fail(function () {
            if (failed) failed();
        })
        .always(function () {
            if (always) always();
        });
};

window.qcc.findWithIndex = function (arr, fn) {
    var result;
    for (var i = 0; i < arr.length && !result; i++) {
        result = fn(arr[i], i) ? { element: arr[i], index: i } : null;
    }
    return result;
};