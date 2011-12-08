using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Server {
    public class Transport : ITransportPublisher {

        private static readonly Action<string> HandleError = Console.WriteLine;
        private static readonly object ConnectionSubscriptionsLock = new object();

        private readonly int _serverPort;
        private readonly IList<Connection> _connectedSockets = new List<Connection>();
        private readonly ISubscriptionHandler _aggregationEngine;

        private Socket _serverSocket;
        private bool _isShutdown;
        
        public bool IsRunning {
            get { return !_isShutdown; }
        }

        public Transport(int port, ISubscriptionHandler aggregationEngine) {
            _serverPort = port;
            _isShutdown = true;
            _aggregationEngine = aggregationEngine;
        }

        public void Start() {
            if (SetupServerSocket())
                AsynchronousAcceptConnections();
        }

        public void Publish(IPayload payload) {
            Publish(GetFullEventName(payload.EventName, payload.Name), payload.ToJson());
        }

        public static string GetFullEventName(string eventName, string name) {
            return string.Format("{0}({1})", eventName, name);
        }

        private void Publish(string eventName, string payload) {
            var buffer = Encoding.UTF8.GetBytes(payload);
            foreach (var socketConnection in _connectedSockets.ToArray()) {
                lock (ConnectionSubscriptionsLock)
                    if (!socketConnection.Subscriptions.ContainsKey(eventName) || !socketConnection.Subscriptions[eventName])
                        continue;
                try {
                    socketConnection.Socket.Send(buffer, buffer.Length, SocketFlags.None);
                } catch (SocketException socketException) {
                    // WSAECONNRESET, the other side closed impolitely
                    if (socketException.ErrorCode == 10054) {
                        CloseSocket(socketConnection);
                    }
                }
            }
        }

        private bool SetupServerSocket() {
            var returnValue = false;
            try {
                var localMachineInfo = Dns.GetHostEntry("localhost");
                var myEndpoint = new IPEndPoint(localMachineInfo.AddressList[1], _serverPort);

                _serverSocket = new Socket(myEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(myEndpoint);
                _serverSocket.Listen((int)SocketOptionName.MaxConnections);

                returnValue = true;
            } catch (SocketException socketException) {
                HandleError(socketException.ToString());
            } catch (Exception exception) {
                HandleError(exception.ToString());
            }
            return returnValue;
        }

        private void AsynchronousAcceptConnections() {
            try {
                _isShutdown = false;
                _serverSocket.BeginAccept(new AsyncCallback(AsyncAcceptCallback), null);
                var riskAggregator = _aggregationEngine;
                if (riskAggregator != null) {
                    riskAggregator.Start(this);
                }
            } catch (SocketException socketException) {
                HandleError(socketException.ToString());
            } catch (Exception exception) {
                HandleError(exception.ToString());
            }
        }

        private void AsyncAcceptCallback(IAsyncResult result) {
            try {
                var connection = new Connection {
                    Socket = _serverSocket.EndAccept(result), 
                    Buffer = new byte[255]
                };
                _connectedSockets.Add(connection);
                connection.Socket.BeginReceive(connection.Buffer, 0, 255, SocketFlags.None, new AsyncCallback(ReceiveCallback), connection);
                _serverSocket.BeginAccept(new AsyncCallback(AsyncAcceptCallback), null);
            } catch (ObjectDisposedException) {
                // socket was closed.
                _serverSocket.Close();
                _isShutdown = true;
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            var connection = (Connection)result.AsyncState;
            try {
                var numRead = connection.Socket.EndReceive(result);
                if (0 != numRead) {
                    ProcessMessage(connection);
                    connection.Buffer = new byte[255];
                    connection.Socket.BeginReceive(connection.Buffer, 0, 255, SocketFlags.None, new AsyncCallback(ReceiveCallback), connection);
                } else {
                    CloseSocket(connection);
                }
            } catch (SocketException socketException) {
                //WSAECONNRESET, the other side closed impolitely
                if (socketException.ErrorCode == 10054) {
                    CloseSocket(connection);
                }
            } catch (ObjectDisposedException) {
                // The socket was closed out from under me
                CloseSocket(connection);
            }
        }

        private void ProcessMessage(Connection connection) {
            var size = 0;
            while (size < connection.Buffer.Length) {
                if (connection.Buffer[size] == 0x0) break;
                size++;
            }
            var newBuffer = new byte[size];
            Array.Copy(connection.Buffer, newBuffer, size);
            var message = Encoding.UTF8.GetString(newBuffer);
            dynamic jsonObject = JsonConvert.DeserializeObject(message);
            var args = ((IEnumerable<object>) jsonObject.args).Select<object, object>(obj => obj.ToString()).Cast<string>();
            var ev = args.FirstOrDefault();
            var location = args.Skip(1).FirstOrDefault();
            var incomingMessage = new IncomingMessage {Type = jsonObject.message.Value as string, Name = location, EventName = ev};
            ProcessIncomingMessage(connection, incomingMessage);
        }

        private void ProcessIncomingMessage(Connection connection, IncomingMessage incomingMessage) {
            switch (incomingMessage.Type) {
                case "subscribe":
                    ProcessEventSubscribe(connection, incomingMessage);
                    break;
                case "unsubscribe":
                    ProcessEventUnsubscribe(connection, incomingMessage);
                    break;
            }
        }

        private void ProcessEventSubscribe(Connection connection, IncomingMessage incomingMessage) {
            connection.Subscriptions[GetFullEventName(incomingMessage.EventName, incomingMessage.Name)] = true;
            _aggregationEngine.OnSubscribe(incomingMessage.Name);
        }

        private void ProcessEventUnsubscribe(Connection connection, IncomingMessage incomingMessage) {
            connection.Subscriptions[GetFullEventName(incomingMessage.EventName, incomingMessage.Name)] = false;
            _aggregationEngine.OnUnsubscribe(incomingMessage.Name);
        }

        private void CloseSocket(Connection connection) {
            _connectedSockets.Remove(connection);
        }
    }
}
