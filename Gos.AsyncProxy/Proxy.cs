using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using GOS.AsyncProxy.Components;

namespace GOS.AsyncProxy
{
public class Proxy
{
    private TcpListener _tcpListener;

    public IPAddress ListenToIp { get; set; }

    public int ListenOnPort { get; set; }
        
    public void Start()
    {
        Logger.Log("start starting proxy");
        Stop();
        ServicePointManager.ServerCertificateValidationCallback += 
            (sender, certificate, chain, policyErrors) => true;

        Logger.Log("starting tcp listener");
        _tcpListener = new TcpListener(ListenToIp, ListenOnPort);
        _tcpListener.Start();

        Logger.Log("register listener callback");
        _tcpListener.BeginAcceptTcpClient(AcceptTcpClient.Run, _tcpListener);

        Logger.Log("end starting proxy");
    }
        
    public void Stop()
    {
        if (_tcpListener != null)
        {
            _tcpListener.Stop();
        }
        _tcpListener = null;
    }
}
}