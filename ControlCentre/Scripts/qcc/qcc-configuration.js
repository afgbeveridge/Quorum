$(document).ready(function () {

    // See if we have some in local storage

    var config = window.qcc.deserialize();

    if (!config) {
        alert('Cannot ascertain any configuration at all');
    }
    else {
        var obsForm = config.observableForm;
        // Now fill in some limits
        obsForm.limits = {
            minResponseLimit: 999,
            maxResponseLimit: 30000,
            minPort: 1025,
            maxPort: 65535,
            standardInsecurePort: 9999,
            standardSecurePort: 8999
        };
        obsForm.isInRange = function (val, min, max) {
            return window.qcc.isPositiveNumeric(val) && parseInt(val) >= min && parseInt(val) <= max;
        };
        obsForm.membersInvalid = ko.computed(function () {
            return !obsForm.members() || obsForm.members().length == 0 || $.trim(obsForm.members()) == '';
        });
        obsForm.responseLimitInvalid = ko.computed(function () {
            return !obsForm.isInRange(obsForm.responseLimit(), obsForm.limits.minResponseLimit, obsForm.limits.maxResponseLimit);
        });
        obsForm.portInvalid = ko.computed(function () {
            return !obsForm.isInRange(obsForm.port(), obsForm.limits.minPort, obsForm.limits.maxPort);
        });
        obsForm.transportType.subscribe(function (val) {
            return obsForm.port(val.indexOf('s') > 0 ? obsForm.limits.standardSecurePort : obsForm.limits.standardInsecurePort);
        });
        obsForm.isValid = ko.computed(function () {
            return !obsForm.membersInvalid() && !obsForm.responseLimitInvalid() && !obsForm.portInvalid();
        });
        obsForm.members.subscribe(function () {
            $('#analysisResults').hide();
        });
        obsForm.targets = ko.observableArray();
        ko.applyBindings(obsForm, $('#cfgBindingSection')[0]);
        $('#save').click(function () {
            var cfg = {};
            for (var prop in obsForm) {
                if (config.hasOwnProperty(prop)) {
                    var cur = obsForm[prop];
                    cfg[prop] = ko.isObservable(cur) ? cur() : '';
                }
            }
            window.qcc.save(cfg);
            config.hash = window.qcc.computeConfigurationHash(obsForm);
            $('#savedMessage').show().fadeOut(2000);
        });

        $('#cancel').click(function () { location.assign('/'); });

        $('#scanLite').click(function () {
            $('#scanDiv').hide();
            $('#scanWorking').show();
            window.qcc.scanNetworkLite(
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

        $('#analyze').click(function () {
            $('#analysisResults').show();
            obsForm.targets.removeAll();
            obsForm.members().split(',').forEach(function (t) {
                var cur = { name: t, contacted: ko.observable(), protocol: ko.observable(), querying: ko.observable(true) };
                obsForm.targets.push(cur);
                $.ajax({
                    url: '/Neighbourhood/Analyze',
                    type: 'POST',
                    data: JSON.stringify({
                        Timeout: obsForm.responseLimit(),
                        Name: t
                    }),
                    contentType: 'application/json',
                    timeout: obsForm.responseLimit() * 5,
                    success: function (data, textStatus, jqXHR) {
                        cur.querying(false);
                        cur.contacted(data.result.Protocol ? 'Yes' : 'No');
                        cur.protocol(data.result.Protocol || '-');
                    }
                })
            .fail(function () {
                cur.querying(false);
                cur.contacted('No');
                cur.protocol('-');
            });
            });

        });

        $(window).on('beforeunload', function () {
            var hash = window.qcc.computeConfigurationHash(obsForm);
            if (hash != config.hash) return 'You have unsaved changes.';
        });

        $('#cfgBindingSection').show('slidein');

    }

});