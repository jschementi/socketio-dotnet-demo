define([
    'underscore',
    'jquery',
    'socketio',
    'text!templates/demo.html',
    'text!templates/event.html',
    'text!templates/subscription.html'
], function (
    _,
    $,
    io,
    demo_template,
    event_template,
    subscription_template
) {

    var events = {};

    function renderEvent (evt) {
        if (events[evt.name]) {
            events[evt.name].updateValue(evt.value);
            return;
        }

        events[evt.name] = evt;
        evt.id = _.uniqueId('events');

        $('#events').append(_.template(event_template)(evt));

        var valueEl = $('#' + evt.id + ' .value');

        function updateValue (value) {
            valueEl.html(value);
        }

        evt.updateValue = _.throttle(updateValue, 1000 / 24);
        
        renderEvent(evt);
    };

    function removeEvent (name) {
        var evt = events[name];
        if (evt) {
            $('#' + evt.id).remove();
            delete events[name];
        }
    };
    
    function renderSubscriptionSelectors () {
        var possibleLocations = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K"];
        for (var i = 0, len = possibleLocations.length; i < len; i++) {
            var sub = possibleLocations[i];
            var item = _.template(subscription_template)({ sub: sub });
            $('#subscriptions').append(item);
        }
    };

    return {
        run: function (elem) {
            $(elem).append(_.template(demo_template)());

            var socket = io.connect();

            socket.on('changeValue', function (data) {
                return renderEvent(data);
            });
            
            renderSubscriptionSelectors();
            
            $('#subscriptions input[type=checkbox]').change(function () {
                var ev = $(this).val();
                if ($(this).prop('checked')) {
                    socket.emit('message', {
                        message: 'subscribe',
                        args: ['changeValue', ev]
                    });
                } else {
                    socket.emit('message', {
                        message: 'unsubscribe',
                        args: ['changeValue', ev]
                    });
                    removeEvent($(this).attr('name'));
                }
            });
        }
    };
});
