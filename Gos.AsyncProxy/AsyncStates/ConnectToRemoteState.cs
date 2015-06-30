using System.IO;
using System.Net.Sockets;

namespace GOS.AsyncProxy.AsyncStates
{
    internal class ConnectToRemoteState
    {
        public int RemotePort { get; set; }
        public string RemoteHost { get; set; }
        public Stream ClientStream { get; set; }
        public MemoryStream MessageStream { get; set; }
        public string RequestMethod { get; set; }
        public TcpClient Client { get; set; }
        public RequestSession Session { get; set; }

        public bool IsSsl { get; set; }
    }
}