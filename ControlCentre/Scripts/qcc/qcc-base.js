
"use strict";

window.qcc = window.qcc || {};

window.qcc.configuration = window.qcc.configuration || {};

window.qcc.redirectLocation = "/QCC/Configuration";

window.qcc.logEnabled = true;

window.qcc.log = function (msg) {
    window.qcc.logEnabled && console.log(msg);
};

window.qcc.serialize = function (cfg) {
    return ko.toJSON(cfg);
};

window.qcc.withLocalStorage = function (f) {
    if (typeof (Storage) === "undefined")
        alert('Browser local storage is required to use this web app');
    else return f();
};

window.qcc.computeConfigurationHash = function (cfg) {
    // TOD: Handle situation where order of members changes, but not actually their names
    return ['members', 'port', 'responseLimit', 'transportType']
        .map(function (p) { return ko.isObservable(cfg[p]) ? cfg[p]() : cfg[p]; }).join(',');
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
        result.observableForm = ko.mapping.fromJS(result);
        result.hash = window.qcc.computeConfigurationHash(result);
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
    qcc.log('Querying neighbours with timeout set to ' + config.responseLimit);
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
            onSuccess(data.machines);
        }
    })
        .fail(function (xHR, status, error) {
            if (failed) failed(xHR, status, error);
        })
        .always(function () {
            if (always) always();
        });
};

window.qcc.networkProbe = function (options) {
    $.ajax({
        url: options.url,
        type: 'POST',
        data: JSON.stringify({
            Port: options.config.port,
            Timeout: options.internalTimeout || options.config.responseLimit,
            TransportType: (ko.isObservable(options.config.transportType) ? options.config.transportType() : options.config.transportType),
            Scope: options.scope || 'workgroup',
            Machines: options.machines
        }),
        contentType: 'application/json',
        timeout: options.ajaxTimeout,
        success: function (data, textStatus, jqXHR) {
                options.success(data.machines);
            }
        })
        .fail(function () {
            if (!options.silent)
                alert('Probe attempt failed');
        })
        .always(function () {
            options.always && options.always();
        });
};

window.qcc.scanNetworkLite = function (config, scope, success, always, timeout) {
    window.qcc.networkProbe({
        url: '/Neighbourhood/ApparentNeighbours',
        config: config,
        scope: scope,
        success: success,
        always: always,
        ajaxTimeout: timeout
    });
};

window.qcc.pingNetworkLite = function (mcs, config, scope, success, always, silent, timeout) {
    window.qcc.networkProbe({
        url: '/Neighbourhood/ContactableNeighbours',
        config: config,
        scope: scope,
        success: success,
        always: always,
        // Config?
        ajaxTimeout: 6000,
        internalTimeout: 5000,
        silent: true,
        machines: mcs
    });
};

window.qcc.broadcastConfiguration = function (config, targets, nexus) {
    qcc.log('Updating neighbour configuraton for ' + targets.join(",") + ' with ' + nexus.join(","));
    $.ajax({
        url: '/Neighbourhood/ConfigurationOffer',
        type: 'POST',
        data: JSON.stringify({
            StatedNexus: nexus,
            ConfigurationTargets: targets,
            Port: config.port,
            Timeout: config.responseLimit,
            TransportType: (ko.isObservable(config.transportType) ? config.transportType() : config.transportType)
        }),
        contentType: 'application/json',
        success: function (data, textStatus, jqXHR) {
        }
    });
};

window.qcc.scanner = function(config, members, success, always) {
    window.qcc.pingNetworkLite(
        members,
        config,
        null,
        success,
        always || $.noop,
        true);
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