using System.IO;
using System.Net.Sockets;

namespace GOS.AsyncProxy.AsyncStates
{
    internal class RemoteConnectionState
    {
        public Stream ClientStream { get; set; }
        public TcpClient RemoteClient { get; set; }
        public MemoryStream MessageStream { get; set; }
        public TcpClient Client { get; set; }
        public RequestSession Session { get; set; }
        public string RemoteHost { get; set; }

        public bool IsSsl { get; set; }
    }
}