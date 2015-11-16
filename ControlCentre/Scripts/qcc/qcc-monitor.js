$(document).ready(function () {

    // See if we have some in local storage

    var config = window.qcc.deserialize(), defaultDiscoveryPeriod = 2000, maxTrackedRequests = 500;

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
            self.IsMaster = ko.observable('Unknown');
            self.UpTime = ko.observable(0);
            self.Strength = ko.observable('Unknown');
            self.InEligibleForElection = ko.observable(null);
            self.address = 'http://' + n + ':' + port + '/';
            self.kill = function () {
                $.post(self.address, '{"TypeHint": "AbdicationState" }', function (d) {
                })
                .fail(function (xr, text, err) {
                    alert('Failed to make ' + self.address + ' ineligible for election');
                });
            };
            self.elect = function () {
                $.post(self.address, '{"TypeHint": "PretenderState" }', function (d) {
                })
                .fail(function (xr, text, err) {
                    alert('Failed to make ' + self.address + ' eligible for election');
                });
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
        };


        var vm = {
            members: config.membersArray.map(function (m) { return new member(m, config.port, config.responseLimit); }),
            discoPeriod: ko.observable(defaultDiscoveryPeriod),
            queries: ko.observable(0)
        };

        vm.viable = ko.computed(function () {
            var active = 0;
            for (var i = 0; i < vm.members.length ; i++) {
                active = active + (vm.members[i].alive() === 'Yes' ? 1 : 0);
            }
            return active > 1;
        });

        vm.discoPeriod.extend({ rateLimit: { timeout: 100, method: "notifyWhenChangesStop" } });

        $.ajaxSetup({
            type: 'POST',
            timeout: config.responseLimit
        });

        ko.applyBindings(vm, $('#monitorSection')[0]);

        var contactAddresses = vm.members.map(function (m) { return { address: m.address, vm: m }; });

        var timer;

        function setTimer(period) {
            timer = setInterval(function () {
                vm.queries(vm.queries() + contactAddresses.length);
                contactAddresses.forEach(function (bundle) {
                    bundle.start = Date.now();
                    $.post(bundle.address, '{"TypeHint": "QueryRequest" }', function (d) {
                        bundle.vm.HandledEvents.removeAll();
                        bundle.vm.lastContact(moment(Date.now()).format("MMM Do YYYY, HH:mm:ss:SS"));
                        ko.mapping.fromJSON(d, null, bundle.vm);
                        // TODO: This is awkward. Interfaces, concrete types, JSON.Net etc etc
                        bundle.vm.HandledEvents(JSON.parse(d).HandledEvents.$values);
                        bundle.vm.alive('Yes');
                        bundle.vm.successfulRequests(bundle.vm.successfulRequests() + 1);
                        var elapsed = Date.now() - bundle.start;
                        if (bundle.vm.minimumResponseTime() == 0) {
                            bundle.vm.minimumResponseTime(elapsed);
                            bundle.vm.maximumResponseTime(elapsed);
                        }
                        else if (elapsed < bundle.vm.minimumResponseTime())
                            bundle.vm.minimumResponseTime(elapsed);
                        else if (elapsed > bundle.vm.maximumResponseTime())
                            bundle.vm.maximumResponseTime(elapsed);
                        bundle.vm.totalResponseTimes(bundle.vm.totalResponseTimes() + elapsed);
                    })
                    .fail(function (xr, text, err) {
                        bundle.vm.IsMaster('Unknown');
                        bundle.vm.Strength('Unknown');
                        bundle.vm.UpTime(0);
                        bundle.vm.alive('No');
                        bundle.vm.showingHardwareDetails(false);
                        bundle.vm.failedRequests(bundle.vm.failedRequests() + 1);
                        bundle.vm.InEligibleForElection(null);
                    })
                });
            }, period);
        };

        vm.discoPeriod.subscribe(function (val) {
            if (timer)
                clearInterval(timer);
            setTimer(val);
        });

        setTimer(defaultDiscoveryPeriod);
    };

});