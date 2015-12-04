﻿$(document).ready(function () {

    var config = window.qcc.deserializeWithCheck();

    if (!config || !config.members || config.members.length == 0) {
        alert('Set up your configuration...will guess for now');
    }

    var machine = function (mc) {
        var self = this;
        self.ipAddress = ko.observable(mc.IpAddressV4);
        self.name = ko.observable((mc.Name || '').toLowerCase());
        self.available = ko.observable('Unknown');
    };

    var assocLookup = [];

    var vm = {
        machines: ko.observableArray([]),
        appConfigText: ko.observable(),
        ccConfigText: ko.observable(),
        responders: ko.observableArray([])
    };

    $('#startProbe').click(function () {
        $('#bindableSection').hide();
        vm.machines.removeAll();
        vm.responders.removeAll();
        vm.appConfigText('');
        $('#probeStart').hide();
        $('#probeWorking').show();
        $.ajax({
            url: '/Neighbourhood/ApparentNeighbours',
            type: 'POST',
            data: JSON.stringify({
                Port: config.port,
                Timeout: config.responseLimit,
                TransportType: (ko.isObservable(config.transportType) ? config.transportType() : config.transportType)
            }),
            contentType: 'application/json',
            success: function (data, textStatus, jqXHR) {
                assocLookup = [];
                data.machines.forEach(function (m) {
                    var v = new machine(m);
                    vm.machines.push(v);
                    assocLookup[v.name()] = v;
                });
            }
        })
        .fail(function () {
            alert('Probe attempt failed');
        })
        .always(function () {
            $('#probeWorking').hide();
            $('#bindableSection').show();
            $('#probeStart').show();
            $('#startProbe').text('Re-probe...');
        });
    });

    function changeStatus(status) {
        for (var k in assocLookup)
            assocLookup[k].available(status);
    };

    $('#executeQuery').click(function () {
        if (vm.machines().length > 0) {
            $('#executeQuery').attr('disabled', 'true');
            $('#probeStart').hide();
            $('#queryWorking').show();
            $('#startProbe').hide();
            var mcs = vm.machines().map(function (m) { return m.name(); });
            vm.responders.removeAll();
            changeStatus('Waiting...');
            window.qcc.queryMachines(mcs, config, function (machines) {
                changeStatus('No');
                machines.forEach(function (m) {
                    if (m.IsValid) {
                        var nm = m.Name.toLowerCase();
                        assocLookup[nm].available('Yes');
                        vm.responders.push(nm);
                    }
                });
                var activeMachineList = vm.responders().join(',');
                vm.appConfigText('&lt;add key="quorum.environment" value="' + activeMachineList + '"/&gt;');
                vm.ccConfigText(activeMachineList);
            },
            null,
            function () {
                $('#executeQuery').removeAttr('disabled');
                $('#probeStart').show();
                $('#startProbe').show();
                $('#queryWorking').hide();
            });
        }
    });

    ko.applyBindings(vm, $('#bindableSection')[0]);

});