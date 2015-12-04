$(document).ready(function () {

    // See if we have some in local storage

    var config = window.qcc.deserializeWithCheck(), maxTrackedRequests = 500;

    var member = function (n, port, limit) {
        var self = this;
        self.name = n;
        self.port = port;
        self.limit = limit;
        self.lastContact = ko.observable('-');
        self.alive = ko.observable('Unknown');
        self.IsMaster = ko.observable('Unknown');
        self.UpTime = ko.observable(0);
        self.Strength = ko.observable('Unknown');
        self.InEligibleForElection = ko.observable(null);
        // TODO: Arrange for controller server side to handle this
        self.address = 'http://' + n + ':' + port + '/';
        function mutator(stateName) {
            $.ajax({
                url: '/Neighbourhood/Render' + stateName,
                type: 'POST',
                data: JSON.stringify({
                    Name: self.name,
                    Port: config.port,
                    Timeout: config.responseLimit,
                    TransportType: (ko.isObservable(config.transportType) ? config.transportType() : config.transportType)
                }),
                contentType: 'application/json',
                timeout: config.responseLimit * 5,
                success: function (data, textStatus, jqXHR) {
                }
            })
            .fail(function () {
                alert('Failed to change state of ' + self.address);
            })
        };
        self.kill = function () {
            mutator('InEligible');
        };
        self.elect = function () {
            mutator('Eligible');
        };
        self.showingHardwareDetails = ko.observable(false);
        self.showingEventHistory = ko.observable(false);
        self.toggleDetail = function () { self.showingHardwareDetails(!self.showingHardwareDetails()); }
        self.toggleEventHistory = function () { self.showingEventHistory(!self.showingEventHistory()); }
        self.Hardware = {
            PhysicalMemory: ko.observable(),
            CPUManufacturer: ko.observable(),
            CPUSpeed: ko.observable(),
            OS: ko.observable()
        };
        // Stats
        self.failedRequests = ko.observable(0);
        self.successfulRequests = ko.observable(0);
        self.minimumResponseTime = ko.observable(0);
        self.maximumResponseTime = ko.observable(0);
        self.totalResponseTimes = ko.observable(0);
        self.averageResponseTime = ko.computed(function () {
            return self.successfulRequests() == 0 ? 0 : self.totalResponseTimes() / self.successfulRequests();
        });
        self.HandledEvents = ko.observableArray([]);
        self.stateIcon = ko.computed(function () {
            var root = '/Content/Images/';
            return self.alive() == 'Yes' ? root + 'traffic-lights-' + (self.InEligibleForElection() || !self.IsMaster() ? 'yellow' : 'green') + '-icon.png' : root + 'traffic-lights-red-icon.png';
        });
        self.WorkUnitsExecuted = ko.observable(0);
        self.PendingEvents = ko.observableArray([]);
        self.formattedTime = function () {
            return Number(self.UpTime().toFixed(4)).toLocaleString();
        }
    };


    var vm = {
        members: config.members.map(function (m) { return new member(m, config.port, config.responseLimit); }),
        discoPeriod: ko.observable(parseInt(config.responseLimit) * 2),
        queries: ko.observable(0),
        transport: config.transportType,
        timer: ko.observable(),
        stopOrStart: function () {
            console.log('Starting or stopping monitor timer');
            if (vm.timer())
                stop();
            else
                start(vm.discoPeriod());
        }
    };

    vm.viable = ko.computed(function () {
        var active = 0;
        for (var i = 0; i < vm.members.length ; i++) {
            active = active + (vm.members[i].alive() === 'Yes' ? 1 : 0);
        }
        return active > 1;
    });

    vm.discoPeriod.extend({ rateLimit: { timeout: 100, method: "notifyWhenChangesStop" } });

    function modifyAjaxTimeout() {
        $.ajaxSetup({
            type: 'POST',
            timeout: vm.discoPeriod() - 500
        });
    };

    ko.applyBindings(vm, $('#monitorSection')[0]);

    function setTimer(period) {
        vm.timer(setInterval(function () {
            vm.queries(vm.queries() + vm.members.length);
            function invalidate(target) {
                target.IsMaster('Unknown');
                target.Strength('Unknown');
                target.UpTime(0);
                target.alive('No');
                target.showingHardwareDetails(false);
                target.showingEventHistory(false);
                target.failedRequests(target.failedRequests() + 1);
                target.InEligibleForElection(null);
            };
            function validate(target, data) {
                target.HandledEvents.removeAll();
                target.PendingEvents.removeAll();
                target.lastContact(moment(Date.now()).format("MMM Do YYYY, HH:mm:ss:SS"));
                ko.mapping.fromJS(data, null, target);
                target.HandledEvents(data.HandledEvents);
                target.PendingEvents(data.PendingEvents.map(function (e) {
                    return { name: e.Name, id: e.Id, prettyDate: e.CreatedOn, age: e.AgeInSeconds };
                }));
                target.alive('Yes');
                target.successfulRequests(target.successfulRequests() + 1);
                // TODO: Neighbour returns response time
                var elapsed = data.LastRequestElapsedTime;
                if (target.minimumResponseTime() == 0) {
                    target.minimumResponseTime(elapsed);
                    target.maximumResponseTime(elapsed);
                }
                else if (elapsed < target.minimumResponseTime())
                    target.minimumResponseTime(elapsed);
                else if (elapsed > target.maximumResponseTime())
                    target.maximumResponseTime(elapsed);
                target.totalResponseTimes(target.totalResponseTimes() + elapsed);
            };
            var mcs = vm.members.map(function (m) { return m.name; });

            function getObservable(name) {
                var nm = name.toLowerCase();
                return qcc.findWithIndex(vm.members, function (v) { return v.name == nm; }).element;
            };

            window.qcc.queryMachines(mcs, config,
                function (machines) {
                    machines.forEach(function (m) {
                        var target = getObservable(m.Name);
                        m.IsValid ? validate(target, m) : invalidate(target);
                    });
                },
            function () {
                mcs.forEach(function (m) {
                    invalidate(getObservable(m));
                });
            });
        }, period));
    };

    function stop() {
        if (vm.timer())
            clearInterval(vm.timer());
        vm.timer(null);
    };

    function start(val) {
        setTimer(val);
        modifyAjaxTimeout();
    };

    vm.discoPeriod.subscribe(function (val) {
        stop();
        start(val);
    });

    start(vm.discoPeriod());

    $(window).unload(function () {
        console.log('Clean up');
        if (vm.timer())
            clearInterval(vm.timer());
    });

    $('#monitorSection').show();

});