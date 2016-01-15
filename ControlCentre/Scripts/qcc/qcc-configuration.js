$(document).ready(function () {

    // See if we have some in local storage

    var config = qcc.deserialize();

    if (!config) {
        alert('Cannot ascertain any configuration at all');
    }
    else {
        toastr.options = {
            "positionClass": "toast-top-center",
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "2500"
        };
        var obsForm = config.observableForm;
        // Now fill in some limits and augment the observable form
        obsForm.limits = {
            minResponseLimit: 999,
            maxResponseLimit: 30000,
            minPort: 1025,
            maxPort: 65535,
            standardInsecurePort: 9999,
            standardSecurePort: 8999
        };

        function standardPortForProtocol(protocol) {
            return protocol.indexOf('s') > 0 ? obsForm.limits.standardSecurePort : obsForm.limits.standardInsecurePort;
        };

        obsForm.isInRange = function (val, min, max) {
            return qcc.isPositiveNumeric(val) && parseInt(val) >= min && parseInt(val) <= max;
        };
        obsForm.membersInvalid = ko.computed(function () {
            return !obsForm.members() || obsForm.members().length == 0 || 
                    $.trim(obsForm.members()) == '' || !obsForm.members().split(',').every(function(n) { return n; });
        });
        obsForm.responseLimitInvalid = ko.computed(function () {
            return !obsForm.isInRange(obsForm.responseLimit(), obsForm.limits.minResponseLimit, obsForm.limits.maxResponseLimit);
        });
        obsForm.portInvalid = ko.computed(function () {
            return !obsForm.isInRange(obsForm.port(), obsForm.limits.minPort, obsForm.limits.maxPort);
        });
        obsForm.transportType.subscribe(function (val) {
            return obsForm.port(standardPortForProtocol(val));
        });
        obsForm.isValid = ko.computed(function () {
            return !obsForm.membersInvalid() && !obsForm.responseLimitInvalid() && !obsForm.portInvalid();
        });
        obsForm.members.subscribe(function () {
            $('#analysisResults').hide();
        });
        obsForm.targets = ko.observableArray();
        obsForm.disjointProtocols = ko.observable(false);
        obsForm.analysisDetectedProtocol = ko.observable();
        obsForm.configurationAnalysisProtocolDisagreement = ko.computed(function () {
            return obsForm.analysisDetectedProtocol() && obsForm.analysisDetectedProtocol() !== obsForm.transportType();
        });
        obsForm.phase1QueriesOutstanding = ko.observable(0);
        obsForm.phase2QueriesOutstanding = ko.observable(0);
        obsForm.analysisInProgress = ko.computed(function () {
            return obsForm.phase1QueriesOutstanding() > 0 || obsForm.phase2QueriesOutstanding() > 0;
        });

        obsForm.phase1QueriesOutstanding.subscribe(function (val) {
            var machinesContacted = obsForm.targets().filter(function (m) { return m.contacted(); });
            if (val == 0) {
                if (obsForm.disjointProtocols() || machinesContacted.length == 0) {
                    machinesContacted.forEach(function (m) {
                        m.phase2Active(false);
                        m.phase2Ok('n');
                    });
                }
                else {
                    function getVmContainer(name) {
                        return qcc.findWithIndex(machinesContacted, function (p) { return p.name == name; }).element;
                    };
                    qcc.log('Commence phase #2 of analysis');
                    obsForm.phase2QueriesOutstanding(machinesContacted.length);
                    var protocolInUse = machinesContacted[0].protocol().toLowerCase();
                    $.ajax({
                        url: qcc.makeUrl('/Neighbourhood/RequestQuorumSelfValidation'),
                        type: 'POST',
                        data: JSON.stringify({
                            Timeout: obsForm.responseLimit() * 5,
                            Machines: machinesContacted.map(function (m) { return m.name; }),
                            PossibleNeighbours: obsForm.members().split(','),
                            TransportType: protocolInUse,
                            Port: standardPortForProtocol(protocolInUse)
                        }),
                        contentType: 'application/json',
                        timeout: obsForm.responseLimit() * 5,
                    })
                    .done(function (data, textStatus, jqXHR) {
                        var mems = data.result.QueriedMembers;
                        mems.forEach(function (m) {
                            var cur = getVmContainer(m.Name), uncontactable = m.Results.filter(function (n) { return !n.Contacted; }).map(function (n) { return n.Name; }).join(",");
                            cur.phase2Active(false);
                            cur.phase2Ok(uncontactable == '' ? 'y' : 'n');
                            cur.uncontactedMachines(uncontactable);
                        });
                    })
                    .fail(function () {
                        machinesContacted.forEach(function (m) { m.phase2Active(false); })
                    })
                    .always(function () {
                        obsForm.phase2QueriesOutstanding(obsForm.phase2QueriesOutstanding() - 1);
                    });
                }
            }
        });
        obsForm.targets.subscribe(function () {
            var cur = obsForm.targets().filter(function(m) { return m.contacted(); });
            obsForm.disjointProtocols(cur.length < 2 ? false : !cur.reduce(function (a, b) { return a.protocol() === b.protocol() ? a : false; }));
            obsForm.analysisDetectedProtocol(cur.length > 0 && !obsForm.disjointProtocols() ? cur[0].protocol().toLowerCase() : null);
        });

        ko.applyBindings(obsForm, $('#cfgBindingSection')[0]);

        function deriveConfiguration() {
            var cfg = {};
            obsForm.members(obsForm.members().toLowerCase());
            for (var prop in obsForm) {
                if (config.hasOwnProperty(prop)) {
                    var cur = obsForm[prop];
                    cfg[prop] = ko.isObservable(cur) ? cur() : '';
                }
            };
            return cfg;
        };

        $('#save').click(function () {
            if (obsForm.configurationAnalysisProtocolDisagreement())
                alert("Your configuration protocol disagrees with the analysis results");
            var cfg = deriveConfiguration();
            qcc.save(cfg);
            config.hash = qcc.computeConfigurationHash(obsForm);
            toastr["info"]("Configuration saved...", "QCC");
        });

        $('#cancel').click(function () { location.assign('/'); });

        $('#scanLite').click(function () {
            $('#scanDiv').hide();
            $('#scanWorking').show();
            qcc.scanNetworkLite(
                config,
                null,
                function (machines) {
                    var content = machines.map(function (m) { return m.Name.toLowerCase(); }).join(',');
                    obsForm.members(content);
                },
                function () {
                    $('#scanDiv').show();
                    $('#scanWorking').hide();
                }
            );
        });

        var analysisVm = function (name) {
            var self = this;
            self.name = name;
            self.contacted = ko.observable();
            self.contactedText = ko.computed(function () {
                return self.contacted() ? 'Yes' : 'No';
            });
            self.protocol = ko.observable();
            self.phase1Active = ko.observable(true);
            self.phase2Active = ko.observable(false);
            self.phase2Ok = ko.observable('?');
            self.uncontactedMachines = ko.observable('');
            self.inactive = ko.computed(function () {
                return !self.phase2Active() && !self.phase1Active();
            });
            self.phase2Image = ko.computed(function () {
                return self.phase2Ok() == 'y' ? '/Content/Images/tick.png' : (self.phase2Ok() == 'n' || !self.contacted() ? '/Content/Images/cross.png' : '/Content/Images/ajax-loader.gif');
            });
        };

        $('#analyze').click(function () {
            $('#importExport').hide();
            $('#analysisResults').show();
            if (!obsForm.analysisInProgress()) {
                qcc.log('Commence phase #1 of analysis');
                obsForm.targets.removeAll();
                var mems = obsForm.members().split(',');
                obsForm.phase1QueriesOutstanding(mems.length);
                mems.forEach(function (t) {
                    var cur = new analysisVm(t);
                    obsForm.targets.push(cur);
                    $.ajax({
                        url: qcc.makeUrl('/Neighbourhood/Analyze'),
                        type: 'POST',
                        data: JSON.stringify({
                            Timeout: obsForm.responseLimit(),
                            Name: t
                        }),
                        contentType: 'application/json',
                        timeout: obsForm.responseLimit() * 5,
                    })
                    .done(function (data, textStatus, jqXHR) {
                        var protocol = data.result.Protocol;
                        cur.phase1Active(false);
                        cur.contacted(protocol ? true : false);
                        cur.protocol(protocol || '-');
                        // Let all machines be dispatched to phase #2; this is for a reason
                        // protocol && obsForm.machinesContacted.push({ protocol: protocol, vm: cur });
                    })
                    .fail(function () {
                        cur.phase1Active(false);
                        cur.contacted(false);
                        cur.protocol('-');
                    })
                    .always(function () {
                        obsForm.targets.valueHasMutated();
                        obsForm.phase1QueriesOutstanding(obsForm.phase1QueriesOutstanding() - 1);
                    });
                });
            }
        });

        $('#hideAnalysis').click(function () {
            $('#analysisResults').hide();
        });

        function handleImportExportAction(toHide, toShow, doc) {
            $('#analysisResults').hide();
            $(toHide).hide();
            $('#importExport').show();
            $(toShow).show();
            $('#configurationDocument').val(doc);
        };

        $('#import').click(function () {
            handleImportExportAction('.exportMode', '.importMode', '');
        });

        $('#export').click(function () {
            var json = qcc.serialize(deriveConfiguration());
            handleImportExportAction('.importMode', '.exportMode', json);
        });

        $('#hideImportExportSection').click(function () {
            $('#importExport').hide();
        });

        $('#importConfiguration').click(function () {
            try {
                var cfg = JSON.parse($('#configurationDocument').val());
                ko.mapping.fromJS(cfg, null, obsForm);
            }
            catch (e) {
                toastr["error"]("Specified configuration invalid...", "QCC");
            }
        });

        $(window).on('beforeunload', function () {
            var hash = qcc.computeConfigurationHash(obsForm);
            if (hash != config.hash) return 'You have unsaved changes.';
            if (obsForm.obsForm.configurationAnalysisProtocolDisagreement()) return "Configuration protocol may be incorrect";
        });

        $('#cfgBindingSection').show('slidein');

    }

});