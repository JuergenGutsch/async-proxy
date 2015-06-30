using System;
using System.Diagnostics;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using GOS.AsyncProxy;
using GOS.AsyncProxy.Service.Properties;

namespace GOS.AsyncProxy.Service
{
    public partial class CallProxy : ServiceBase
    {
        private Thread _thread;

        public CallProxy()
        {
            InitializeComponent(); 
            if (!EventLog.SourceExists("GOS"))
            {
                EventLog.CreateEventSource("GOS", "Proxy");
            }
            ServiceEventLog.Source = "GOS";
            ServiceEventLog.Log = "Proxy";
        }

        protected override void OnStart(string[] args)
        {
            ServiceEventLog.WriteEntry("Starting GOS Proxy Proxy service", EventLogEntryType.Information);
            try
            {
                _thread = new Thread(Execute);
                _thread.Start();
            }
            catch (Exception ex)
            {
                ServiceEventLog.WriteEntry("GOS Proxy Proxy service started\n" + ex, EventLogEntryType.Error);
                Logger.Log(ex);
            }

            ServiceEventLog.WriteEntry("GOS Proxy Proxy service started", EventLogEntryType.Information);
        }

        private void Execute()
        {
            var ip = Settings.Default.ListenIP;
            var port = Settings.Default.ListenPort;

            var proxy = new Proxy
                {
                    ListenToIp = IPAddress.Parse(ip),
                    ListenOnPort = port
                };
            try
            {
                proxy.Start();
            }
            catch (Exception ex)
            {
                proxy.Stop();
                ServiceEventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
                Logger.Log(ex);
            }
        }

        protected override void OnStop()
        {
            if (_thread != null)
            {
                _thread.Abort();
                _thread.Join();
                ServiceEventLog.WriteEntry("GOS Proxy Proxy service stopped", EventLogEntryType.Information);
            }
        }
    }
}
