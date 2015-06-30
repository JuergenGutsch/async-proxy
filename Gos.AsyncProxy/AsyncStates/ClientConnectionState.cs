using System.IO;
using System.Net.Sockets;

namespace GOS.AsyncProxy.AsyncStates
{
    internal class ClientConnectionState
    {
        public byte[] Buffer { get; set; }

        public Stream ClientStream { get; set; }
        public MemoryStream MessageStream { get; set; }
        public ClientMessageHeader ClientMessageHeader { get; set; }

        public TcpClient Client { get; set; }

        public RequestSession Session { get; set; }

        public NetworkStream ClientStreamBase { get; set; }

        public bool IsSsl { get; set; }
    }
}