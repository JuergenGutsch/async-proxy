using System;

namespace GOS.AsyncProxy
{
    internal class RequestSession
    {
        public RequestSession()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public int WriteLoop { get; set; }
        public int WriteBytes { get; set; }
        public int WriteTotalBytes { get; set; }
        public string TargetUrl { get; set; }
        public string Protocoll { get; set; }
    }
}