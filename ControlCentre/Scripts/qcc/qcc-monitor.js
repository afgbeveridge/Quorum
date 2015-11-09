$(document).ready(function () {

    // See if we have some in local storage

    var config = window.qcc.deserialize(), defaultDiscoveryPeriod = 10000;

    if (!config || !config.membersArray || config.membersArray.length == 0) {
        alert('Cannot ascertain any configuration at all');
    }
    else {
        var member = function (n, port, limit) {
            var self = this;
            self.name = n;
            self.port = port;
            self.limit = limit;
            self.lastContact = ko.observable('-');
            self.alive = ko.observable('Unknown');
            self.isMaster = ko.observable('Unknown');
            self.upTime = ko.observable(0);
        };

        var vm = { 
            members: config.membersArray.map(function (m) { return new member(m, config.port, config.responseLimit); }),
            responseLimit: ko.observable(defaultDiscoveryPeriod)
        };

        vm.responseLimit.extend({ rateLimit: { timeout: 100, method: "notifyWhenChangesStop" } });

        ko.applyBindings(vm, $('#monitorSection')[0]);

        var contactAddresses = config.membersArray.map(function (m, idx) { return { address: 'http://' + m + ':' + config.port + '/', vm: vm.members[idx] } });

        var timer;

        function setTimer(period) {
            timer = setInterval(function () {
                contactAddresses.forEach(function (bundle) {
                    $.post(bundle.address, '{"TypeHint": "QueryRequest" }', function (d) {
                        bundle.vm.lastContact(Date.now());
                        var data = JSON.parse(d);
                        bundle.vm.isMaster(data.IsMaster);
                        bundle.vm.upTime(data.UpTime);
                        bundle.vm.alive('Yes');
                    })
                    .fail(function (xr, text, err) {
                        bundle.vm.isMaster('Unknown');
                        bundle.vm.upTime('0');
                        bundle.vm.alive('No');
                    })
                });
            }, period);
        };

        vm.responseLimit.subscribe(function (val) {
            if (timer) 
                clearInterval(timer);
            setTimer(val);
        });

        setTimer(defaultDiscoveryPeriod);

    }

});