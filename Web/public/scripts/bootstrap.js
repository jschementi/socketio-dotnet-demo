;(function() {

    function amd (script) {
        return "/3rdparty/" + script + "-amd";
    };

    function nonamd (script) {
        return "order!/3rdparty/" + script + ".js";
    };

    require.config({
        paths: {
            jquery: amd('jquery'),
            underscore: amd('underscore'),
            socketio: amd('socketio'),
            templates: '/templates'
        }
    });

    require([nonamd('jquery'), nonamd('underscore')], function () {
        require(['demo'], function (Demo) {
            Demo.run($('body').first());
        });
    });

}).call(this);
