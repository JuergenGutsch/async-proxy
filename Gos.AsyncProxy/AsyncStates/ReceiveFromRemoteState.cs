using System.IO;
using System.Net.Sockets;

namespace GOS.AsyncProxy.AsyncStates
{
    internal class ReceiveFromRemoteState
    {
        public byte[] Buffer { get; set; }
        public Stream ClientStream { get; set; }
        public Stream RemoteStream { get; set; }
        public TcpClient Client { get; set; }
        public TcpClient RemoteClient { get; set; }
        public RequestSession Session { get; set; }

        public NetworkStream RemoteStreamBase { get; set; }
    }
}