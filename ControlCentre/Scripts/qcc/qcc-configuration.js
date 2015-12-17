$(document).ready(function () {
    
    // See if we have some in local storage

    var config = window.qcc.deserialize();

    if (!config) {
        alert('Cannot ascertain any configuration at all');
    }
    else {
        var obsForm = config.observableForm;
        obsForm.isInRange = function (val, min, max) {
            return window.qcc.isPositiveNumeric(val) && parseInt(val) >= min && parseInt(val) <= max;
        };
        obsForm.membersInvalid = ko.computed(function () {
            return !obsForm.members() || obsForm.members().length == 0 || $.trim(obsForm.members()) == '';
        });
        obsForm.responseLimitInvalid = ko.computed(function () {
            return !obsForm.isInRange(obsForm.responseLimit(), 1000, 30000);
        });
        obsForm.portInvalid = ko.computed(function () {
            return !obsForm.isInRange(obsForm.port(), 1025, 65535);
        });
        obsForm.isValid = ko.computed(function () {
            return !obsForm.membersInvalid() && !obsForm.responseLimitInvalid() && !obsForm.portInvalid();
        });
        ko.applyBindings(obsForm, $('#cfgBindingSection')[0]);
        $('#save').click(function () {
            var cfg = {};
            for (var prop in obsForm) {
                var cur = obsForm[prop];
                cfg[prop] = ko.isObservable(cur) ? cur() : '';
            }
            window.qcc.save(cfg);
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

        $('#cfgBindingSection').show('slidein');
    }

});