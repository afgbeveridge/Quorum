$(document).ready(function () {

    var machine = function (mc) {
        var self = this;
        self.ipAddress = ko.observable(mc.IpAddressV4);
        self.name = ko.observable((mc.Name || '').toLowerCase());
        self.available = ko.observable('Unknown');
    };

    var assocLookup = [];

    var vm = {
        machines: ko.observableArray([]),
        appConfigText: ko.observable()
    };

    $('#startProbe').click(function () {
        $('#bindableSection').hide();
        $('#execQuery').hide();
        vm.machines.removeAll();
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
                $('#execQuery').show();
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
            changeStatus('Waiting...'); 
            // TODO: assocLookup[nm].available('Yes') -- make all known mcs Unknown in terms of availability
            $.ajax({
                url: '/Discovery/QueryMachines',
                type: 'POST',
                data: JSON.stringify({ machines: mcs }),
                contentType: 'application/json',
                success: function (data, textStatus, jqXHR) {
                    var accessible = [];
                    changeStatus('No');
                    data.machines.forEach(function (m) {
                        var nm = m.Name.toLowerCase();
                        assocLookup[nm].available('Yes');
                        accessible.push(nm);
                    });
                    vm.appConfigText(accessible.join(','));
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