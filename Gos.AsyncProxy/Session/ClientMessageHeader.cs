using System;
using System.Collections.Generic;

namespace GOS.AsyncProxy
{
    internal class ClientMessageHeader
    {
        public string Method { get; set; }
        public Uri RemoteUri { get; set; }
        public string Protocol { get; set; }
        public Version ProtocolVersion { get; set; }
        public IDictionary<string, string> Headers { get; set; }
    }
}