$(document).ready(function () {
    
    // See if we have some in local storage

    var config = window.qcc.deserialize();

    if (!config) {
        alert('Cannot ascertain any configuration at all');
    }
    else {
        for (var prop in config) { 
            $('#' + prop).val(config[prop]);
        }
        $('#save').click(function () {
            var cfg = {};
            for (var prop in config) {
                cfg[prop] = $('#' + prop).val();
            }
            window.qcc.save(cfg);
        });
        $('#cancel').click(function () { location.assign('/'); });
    }

});