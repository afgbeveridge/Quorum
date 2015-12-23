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
                    var content = machines.map(function(m) { return m.Name.toLowerCase(); }).join(',');
                    obsForm.members(content);
                },
                function () {
                    $('#scanDiv').show();
                    $('#scanWorking').hide();
                }
            );
        });

        $(window).on('beforeunload', function () {
            var hash = window.qcc.computeConfigurationHash(obsForm);
            if (hash != config.hash) return 'You have unsaved changes.';
        });

        $('#cfgBindingSection').show('slidein');

    }

});