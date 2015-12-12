
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
        if (stored)
            result = JSON.parse(stored);
        else {
            result = {
                members: '',
                port: 9999,
                responseLimit: 3000,
                transportType: 'http'
            };
        }
        // TODO: Make an observableForm property mapped from JS to etc using ko mapping
        result.observableForm = ko.mapping.fromJS(result);
        result.members = result.members.split(',');
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

window.qcc.queryMachines = function (mcs, config, onSuccess, failed, always) {
    $.ajax({
        url: '/Neighbourhood/QueryMachines',
        type: 'POST',
        data: JSON.stringify({
            Machines: mcs,
            Port: config.port,
            Timeout: config.responseLimit,
            TransportType: (ko.isObservable(config.transportType) ? config.transportType() : config.transportType)
        }),
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

window.qcc.scanNetworkLite = function (config, scope, success, always) {
    $.ajax({
        url: '/Neighbourhood/ApparentNeighbours',
        type: 'POST',
        data: JSON.stringify({
            Port: config.port,
            Timeout: config.responseLimit,
            TransportType: (ko.isObservable(config.transportType) ? config.transportType() : config.transportType),
            Scope: scope || 'workgroup'
        }),
        contentType: 'application/json',
        success: function (data, textStatus, jqXHR) {
                success(data.machines);
            }
        })
        .fail(function () {
            alert('Probe attempt failed');
        })
        .always(function () {
            always && always();
        });
};

// Util
window.qcc.findWithIndex = function (arr, fn) {
    var result;
    for (var i = 0; i < arr.length && !result; i++) {
        result = fn(arr[i], i) ? { element: arr[i], index: i } : null;
    }
    return result;
};

window.qcc.isPositiveNumeric = function (val) {
    return /^\d+$/.test(val);
};