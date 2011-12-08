using System.Collections.Generic;
using System.Net.Sockets;

namespace Server {
    public class Connection {
        public Socket Socket;
        public byte[] Buffer;
        public readonly IDictionary<string, bool> Subscriptions = new Dictionary<string, bool>();
    }
}
