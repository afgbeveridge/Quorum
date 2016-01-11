
"use strict";

window.qcc = window.qcc || {};

qcc.configuration = qcc.configuration || {};

qcc.redirectLocation = "/QCC/Configuration";
qcc.urlBase = '';

qcc.logEnabled = true;

qcc.log = function (msg) {
    qcc.logEnabled && console.log(msg);
};

qcc.makeUrl = function (url) {
    return qcc.urlBase + url;
};

qcc.serialize = function (cfg) {
    return ko.toJSON(cfg);
};

qcc.withLocalStorage = function (f) {
    if (typeof (Storage) === "undefined")
        alert('Browser local storage is required to use this web app');
    else return f();
};

qcc.computeConfigurationHash = function (cfg) {
    // TOD: Handle situation where order of members changes, but not actually their names
    return ['members', 'port', 'responseLimit', 'transportType']
        .map(function (p) { return ko.isObservable(cfg[p]) ? cfg[p]() : cfg[p]; }).join(',');
};

qcc.deserialize = function () {
    return qcc.withLocalStorage(function () {
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
        result.hash = qcc.computeConfigurationHash(result);
        result.members = result.members.split(',');
        return result;
    });
};

qcc.deserializeWithCheck = function () {
    var config = qcc.deserialize();
    if (!config || !config.members || config.members.length == 0 || !config.members.every(function (n) { return n && n.length > 0; })) {
        alert('Please set up your configuration');
        window.location.assign(qcc.redirectLocation);
    }
    return config;
};

qcc.save = function (cfg) {
    qcc.withLocalStorage(function () {
        localStorage.setItem('qcc', qcc.serialize(cfg));
    });
};

qcc.queryMachines = function (mcs, config, onSuccess, failed, always) {
    qcc.log('Querying neighbours with timeout set to ' + config.responseLimit);
    $.ajax({
        url: qcc.makeUrl('/Neighbourhood/QueryMachines'),
        type: 'POST',
        data: JSON.stringify({
            Machines: mcs,
            Port: config.port,
            Timeout: config.responseLimit,
            TransportType: (ko.isObservable(config.transportType) ? config.transportType() : config.transportType)
        }),
        contentType: 'application/json',
    })
        .done(function (data, textStatus, jqXHR) {
            onSuccess(data.machines);
        })

        .fail(function (xHR, status, error) {
            if (failed) failed(xHR, status, error);
        })
        .always(function () {
            if (always) always();
        });
};

qcc.networkProbe = function (options) {
    $.ajax({
        url: qcc.makeUrl(options.url),
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
    }).
        done(function (data, textStatus, jqXHR) {
            options.success(data.machines);
        })
        .fail(function () {
            if (!options.silent)
                alert('Probe attempt failed');
        })
        .always(function () {
            options.always && options.always();
        });
};

qcc.scanNetworkLite = function (config, scope, success, always, timeout) {
    qcc.networkProbe({
        url: qcc.makeUrl('/Neighbourhood/ApparentNeighbours'),
        config: config,
        scope: scope,
        success: success,
        always: always,
        ajaxTimeout: timeout
    });
};

qcc.pingNetworkLite = function (mcs, config, scope, success, always, silent, timeout) {
    qcc.networkProbe({
        url: qcc.makeUrl('/Neighbourhood/ContactableNeighbours'),
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

qcc.broadcastConfiguration = function (config, targets, nexus) {
    qcc.log('Updating neighbour configuraton for ' + targets.join(",") + ' with ' + nexus.join(","));
    $.ajax({
        url: qcc.makeUrl('/Neighbourhood/ConfigurationOffer'),
        type: 'POST',
        data: JSON.stringify({
            StatedNexus: nexus,
            ConfigurationTargets: targets,
            Port: config.port,
            Timeout: config.responseLimit,
            TransportType: (ko.isObservable(config.transportType) ? config.transportType() : config.transportType)
        }),
        contentType: 'application/json',
    })
        .done(function (data, textStatus, jqXHR) {
        });
};

qcc.scanner = function (config, members, success, always) {
    qcc.pingNetworkLite(
        members,
        config,
        null,
        success,
        always || $.noop,
        true);
};

// Util
qcc.findWithIndex = function (arr, fn) {
    var result;
    for (var i = 0; i < arr.length && !result; i++) {
        result = fn(arr[i], i) ? { element: arr[i], index: i } : null;
    }
    return result;
};

qcc.isPositiveNumeric = function (val) {
    return /^\d+$/.test(val);
};