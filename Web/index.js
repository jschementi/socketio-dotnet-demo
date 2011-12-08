var fs = new (require('node-static').Server)('./public'),
    app = require('http').createServer(function (req, res) {
        req.addListener('end', function () {
            fs.serve(req, res);
        });
    }),
    io = require('socket.io').listen(app),
    _ = require('underscore'),
    net = require('net'),
    utils = require('./lib/utils'),
    prod = false

if (process.env.NODE_ENV == 'production') {
    prod = true;

    io.enable('browser client minification');
    io.enable('browser client etag');
    io.enable('browser client gzip');
    io.set('log level', 1);
}

app.listen(9876);
console.log((prod ? "Production" : "Development") + " HTTP server listening on http://0.0.0.0:9876");

io.sockets.on('connection', function (client) {
    console.log('socketio> client connected');

    var dotNetServer = net.connect(30000, function () {
        console.log('node>    .NET server connected');

        client.on('message', function (message) {
            dotNetServer.write(JSON.stringify(message));
        });

        dotNetServer.on('data', function (buf) {
            utils.bufferToObjects(buf).forEach(function (obj) {
                client.emit(obj.event, obj.args);
            });
        });
    });
});

