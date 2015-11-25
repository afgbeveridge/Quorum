$(document).ready(function () {

    var config = window.qcc.deserialize();

    if (!config || !config.membersArray || config.membersArray.length == 0) {
        alert('Cannot ascertain any configuration at all');
        config = { port: 9999, responseLimit: 10000 };
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
        $.getJSON('/Discovery/Neighbourhood', function (res) {
            assocLookup = [];
            res.machines.forEach(function (m) {
                var v = new machine(m);
                vm.machines.push(v);
                assocLookup[v.name()] = v;
            })
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
            $.ajax({
                url: '/Discovery/QueryMachines',
                type: 'POST',
                data: JSON.stringify({ machines: mcs, port: config.port, timeout: config.responseLimit }),
                contentType: 'application/json',
                success: function (data, textStatus, jqXHR) {
                    changeStatus('No');
                    data.machines.forEach(function (m) {
                        var nm = m.Name.toLowerCase();
                        assocLookup[nm].available('Yes');
                        vm.responders.push(nm);
                    });
                    var activeMachineList = vm.responders().join(',');
                    vm.appConfigText('&lt;add key="quorum.environment" value="' + activeMachineList + '"/&gt;');
                    vm.ccConfigText(activeMachineList);
                }
            })
            .always(function () {
                $('#executeQuery').removeAttr('disabled');
                $('#probeStart').show();
                $('#startProbe').show();
                $('#queryWorking').hide();
            });
        }
    });

    ko.applyBindings(vm, $('#bindableSection')[0]);

});