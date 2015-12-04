$(document).ready(function () {
    
    // See if we have some in local storage

    var config = window.qcc.deserialize();

    if (!config) {
        alert('Cannot ascertain any configuration at all');
    }
    else {
        for (var prop in config) {
            var curVal = config[prop];
            if (ko.isObservable(curVal))
                ko.applyBindings(config, $('#' + prop)[0]);
            else
                $('#' + prop).val(config[prop]);
        }
        $('#save').click(function () {
            var cfg = {};
            for (var prop in config) {
                var cur = config[prop];
                cfg[prop] = ko.isObservable(cur) ? cur() : $('#' + prop).val();
            }
            window.qcc.save(cfg); 
        });
        $('#cancel').click(function () { location.assign('/'); });
    }

});