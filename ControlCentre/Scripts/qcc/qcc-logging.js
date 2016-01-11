$(document).ready(function () {

    var cfg = qcc.deserializeWithCheck(), transport = cfg.transportType.toLowerCase(), secureTransport = transport.indexOf("s") > 0;

    if (transport.indexOf("h") < 0) {
        $('#bootWait').hide();
        $('#wrongTransport').show();
    }
    // Everything seems OK, proceed
    else {

        var maxSocketConnectionWait = 5000, socketConnectionTimeout = 100, token;

        $.getJSON(qcc.makeUrl('/Neighbourhood/GenerateCustomHeaders'), null, function (data) {
            token = data.result;
        });

        var socketViewModel = function (name, port) {
            var self = this;
            self.socket = null;
            self.isConnected = ko.observable(false);
            self.connectionFailed = ko.observable(false);
            self.connecting = ko.observable(false);
            self.waited = 0;
            self.close = function () {
                if (self.socket) {
                    qcc.log('Closing web socket for ' + name);
                    self.socket.close(1000, 'User request');
                    self.socket = null;
                    self.isConnected(false);
                }
            };
            self.noteError = function () {
                self.connectionFailed(true);
                self.socket = null;
                self.connecting(false);
                self.isConnected(false);
                setTimeout(function () {
                    // Reset UI to try and avoid confusion
                    self.connectionFailed(false);
                }, 3000);
            };
            self.waitForSocketHandshake = function () {
                setTimeout(function () {
                    if (self.socket.readyState == 2 || self.socket.readyState == 3) {
                        self.noteError();
                    }
                    else if (self.socket.readyState == 0) {
                        self.waited += socketConnectionTimeout;
                        if (self.waited > maxSocketConnectionWait) {
                            self.noteError();
                        }
                        else
                            self.waitForSocketHandshake();
                    }
                    else {
                        qcc.log('Opened web socket for ' + name);
                        self.socket.onmessage = function (event) {
                            var vm = ko.mapping.fromJSON(event.data);
                            if (!vm.Ping) {
                                vm.Node = name;
                                loggingViewModel.entries.unshift(vm);
                                if (loggingViewModel.entries().length > loggingViewModel.maxEntries())
                                    loggingViewModel.entries.pop();
                            }
                        };
                        self.socket.send(token);
                        self.connecting(false);
                        self.isConnected(true);
                    }
                }, socketConnectionTimeout);
            };

            self.open = function () {
                if (!self.socket) {
                    qcc.log('Opening web socket for ' + name);
                    self.connectionFailed(false);
                    self.waited = 0;
                    self.connecting(true);
                    self.socket = new WebSocket((secureTransport ? 'wss' : 'ws') + '://' + name + ':' + port);
                    self.waitForSocketHandshake();
                }
            };
        };

        var selectableNodeViewModel = function (name, port) {
            var self = this;
            self.name = name;
            self.available = ko.observable(true);
            self.socketModel = new socketViewModel(name, port);
            self.click = function () {
                if (self.available())
                    self.socketModel.isConnected() ? self.socketModel.close() : self.socketModel.open();
            };
            self.connected = ko.computed(function () {
                return self.socketModel.isConnected();
            });
            self.applicableImage = ko.computed(function () {
                var root = '/Content/Images/';
                return !self.available() ? root + 'not_contacted_server.png' :
                    (self.socketModel.connectionFailed() ? root + 'not_listening_server.png' :
                    (self.socketModel.connecting() ? root + 'ajax-loader.gif' :
                    (self.connected() ? root + 'listening_server.png' : root + 'server.png')));
            });
            self.nodeOpacity = ko.computed(function () {
                return self.connected() ? 'nodeConnected' : (self.available() ? 'nodeConnectable' : 'nodeUnconnectable');
            });
        };

        var loggingViewModel = {
            entries: ko.observableArray([]),
            maxEntries: ko.observable(50),
            config: cfg,
            socketViewModels: ko.observableArray([]),
            scanTimer: null,
            insecure: !secureTransport
        };

        loggingViewModel.nodes = ko.observableArray(loggingViewModel.config.members.map(function (m) { return new selectableNodeViewModel(m, cfg.port); }));

        function onScanComplete(machines) {
            var found = machines.map(function (m) { return m.Name; });
            loggingViewModel.nodes().forEach(function (m) {
                m.available(found.indexOf(m.name) >= 0);
            });
        };

        qcc.scanner(loggingViewModel.config, loggingViewModel.config.members, onScanComplete, function () {
            $('#bootWait').toggle();
            $('#bindingSection').show('slidein');
        });

        loggingViewModel.scanTimer = setInterval(qcc.scanner, 10000, loggingViewModel.config, loggingViewModel.config.members, onScanComplete);

        $(window).unload(function () {
            qcc.log("Stopping scan timer");
            clearInterval(loggingViewModel.scanTimer);
        });

        ko.applyBindings(loggingViewModel, $('#bindingSection')[0]);
    }
});