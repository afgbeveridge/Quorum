$(document).ready(function () {

    // See if we have some in local storage

    var config = qcc.deserializeWithCheck(), maxTrackedRequests = 100, scanInterval = 10000;

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
        self.CurrentState = ko.observable('-');
        self.InEligibleForElection = ko.observable(null);
        // TODO: Arrange for controller server side to handle this
        self.address = 'http://' + n + ':' + port + '/';
        self.detected = ko.observable(false);
        function mutator(stateName) {
            $.ajax({
                url: qcc.makeUrl('/Neighbourhood/Render' + stateName),
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
        queries: ko.observable(0),
        transport: config.transportType,
        discoPeriod: ko.observable(parseInt(config.responseLimit) * 2),
        timer: ko.observable(),
        baseTimeout: config.responseLimit,
        stopOrStart: function () {
            if (vm.timer())
                stop(vm.timer);
            else
                start(vm.gatedDiscoPeriod());
        },
        querying: ko.observable(false),
        errorHistoryLimit: 5,
        communicationsErrors: ko.observableArray([]),
        showHideCommsErrors: function () {
            $('#generalErrors').toggle();
        },
        scanTimer: ko.observable()
    };

    vm.gatedDiscoPeriod = ko.computed({
        read: function () {
            return vm.discoPeriod();
        },
        write: function (value) {
            var valid = qcc.isPositiveNumeric(value) && parseInt(value) >= parseInt(config.responseLimit);
            if (valid)
                vm.discoPeriod(value);
            $('#invalidPollPeriod')[valid ? 'hide' : 'show']();
        },
        owner: this
    })


    vm.viable = ko.computed(function () {
        var active = 0;
        for (var i = 0; i < vm.members.length ; i++) {
            active = active + (vm.members[i].alive() === 'Yes' ? 1 : 0);
        }
        return active > 1;
    });

    vm.splitBrain = ko.computed(function () {
        return vm.members.filter(function (m) { return m.IsMaster() === true; }).length > 1;
    });

    vm.gatedDiscoPeriod.extend({ rateLimit: { timeout: 175, method: "notifyWhenChangesStop" } });

    function modifyAjaxTimeout() {
        $.ajaxSetup({
            type: 'POST',
            timeout: vm.gatedDiscoPeriod()
        });
    };

    ko.applyBindings(vm, $('#monitorSection')[0]);

    function formattedNow() {
        return moment(Date.now()).format("MMM Do YYYY, HH:mm:ss:SS");
    };

    function setTimer(period) {
        vm.timer(setInterval(function () {
            vm.querying(true);
            function invalidate(target) {
                target.IsMaster('Unknown');
                target.Strength('Unknown');
                target.CurrentState('-');
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
                target.lastContact(formattedNow());
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
            var mcs = vm.members.filter(function (m) { return m.detected(); }).map(function (m) { return m.name; });

            function getObservable(name) {
                var nm = name.toLowerCase();
                return qcc.findWithIndex(vm.members, function (v) { return v.name == nm; }).element;
            };

            qcc.queryMachines(mcs, config,
                function (machines) {
                    var needConfig = [];
                    var allMembers = vm.members.map(function (m) { return m.name; }).sort();
                    var curSet = allMembers.join("");
                    machines.forEach(function (m) {
                        var target = getObservable(m.Name);
                        if (!m.IsValid)
                            invalidate(target);
                        else {
                            validate(target, m);
                            if (!m.SupposedNeighbours || m.SupposedNeighbours.length == 0 || (m.SupposedNeighbours || []).sort().join("") != curSet)
                                needConfig.push(m.Name);
                        }
                    });
                    // Send all machines as possible quorum members in case one comes on line between a query cycle
                    (needConfig.length > 0) && qcc.broadcastConfiguration(config, needConfig, allMembers); //mcs);
                },
            function (xhr, status, error) {
                mcs.forEach(function (m) {
                    invalidate(getObservable(m));
                });
                // Record error in vm.communicationsErrors; limit according to vm.errorHistoryLimit
                if (vm.communicationsErrors().length >= vm.errorHistoryLimit) {
                    var id = vm.communicationsErrors()[0].id;
                    vm.communicationsErrors.remove(function (e) { return e.id == id; });
                }
                vm.communicationsErrors.push({ id: Date.now(), status: status, error: error, formattedDate: formattedNow() });
            },
            function () {
                vm.queries(vm.queries() + mcs.length);
                vm.querying(false);
            });
        }, period));
    };

    function stop(t) {
        if (t())
            clearInterval(t());
        t(null);
    };

    function start(val) {
        setTimer(val);
        modifyAjaxTimeout();
    };

    vm.gatedDiscoPeriod.subscribe(function (val) {
        stop(vm.timer);
        start(val);
    });

    $(window).unload(function () {
        qcc.log("Stopping timers");
        stop(vm.timer);
        stop(vm.scanTimer);
    });

    var scanNodes = vm.members.map(function (m) { return m.name; });

    function onScanComplete(machines) {
        var found = machines.map(function (m) { return m.Name.toLowerCase(); });
        vm.members.forEach(function (m) {
            m.detected(found.indexOf(m.name) >= 0);
        });
    };

    qcc.scanner(config, scanNodes, onScanComplete, function () {
        $('#bootWait').toggle();
        $('#monitorSection').show('slidein');
        start(vm.gatedDiscoPeriod());
    });

    vm.scanTimer(setInterval(qcc.scanner, scanInterval, config, scanNodes, onScanComplete));

});