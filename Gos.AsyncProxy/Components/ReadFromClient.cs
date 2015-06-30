using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using GOS.AsyncProxy.AsyncStates;

namespace GOS.AsyncProxy.Components
{
    public class ReadFromClient
    {
        public static void Run(IAsyncResult asyncResult)
        {
            var state = asyncResult.AsyncState as ClientConnectionState;

            if (state != null)
            {
                int read;
                try
                {
                    read = state.ClientStream.EndRead(asyncResult);
                }
                catch (Exception ex)
                {
                    WriteLog(state.Session, 0, "ERR", ex.Message);
                    state.ClientStreamBase.Close();
                    state.ClientStream.Close();
                    return;
                }
                state.MessageStream.Write(state.Buffer, 0, read);

                if (state.ClientStreamBase.DataAvailable)
                {
                    state.ClientStream.BeginRead(state.Buffer, 0, state.Buffer.Length, ReadFromClient.Run, state);
                }
                else
                {
                    if (read == 0)
                    {
                        state.ClientStream.Close();
                        return;
                    }

                    var messageHeader = ParseClientMessage(state.MessageStream);
                    if (messageHeader == null)
                    {
                        state.ClientStream.Close();
                        state.ClientStreamBase.Close();
                        return;
                    }

                    state.ClientMessageHeader = messageHeader;
                    state.Session.TargetUrl = state.ClientMessageHeader.RemoteUri.ToString();
                    state.Session.Protocoll = state.ClientMessageHeader.Protocol;

                    // TODO: filter the data from the client
                    var connectState = new ConnectToRemoteState
                    {
                        Session = state.Session,
                        RemotePort = state.ClientMessageHeader.RemoteUri.Port,
                        RemoteHost = state.ClientMessageHeader.RemoteUri.Host,
                        RequestMethod = state.ClientMessageHeader.Method,
                        Client = state.Client,
                        ClientStream = state.ClientStream,
                        MessageStream = state.MessageStream,
                        IsSsl = state.IsSsl
                    };

                    StartRemoteConnection(connectState);
                }
            }
        }

        private static void StartRemoteConnection(ConnectToRemoteState state)
        {
            if (state.RequestMethod.Equals("CONNECT", StringComparison.InvariantCultureIgnoreCase))
            {
                // handle SSL response for CONNECT
                HandleSslConnect(state);
                return;
            }

            var remoteip = GetRemoteIp(state.RemoteHost); // TODO: handle SocketError.HostNotFound
            if (remoteip == null)
            {
                WriteLog(state.Session, 0, "ERR");
                return;
            }

            var remoteClient = new TcpClient();

            var remoteConnectionState = new RemoteConnectionState
            {
                Session = state.Session,
                ClientStream = state.ClientStream,
                Client = state.Client,
                MessageStream = state.MessageStream,
                RemoteClient = remoteClient,
                RemoteHost = state.RemoteHost,
                IsSsl = state.IsSsl
            };

            remoteClient.BeginConnect(remoteip, state.RemotePort, ConnectToRemote.Run, remoteConnectionState);
        }

        private static void HandleSslConnect(ConnectToRemoteState state)
        {
            var connectStreamWriter = new StreamWriter(state.ClientStream);
            connectStreamWriter.WriteLine("HTTP/1.0 200 Connection established");
            connectStreamWriter.WriteLine("Timestamp: {0}", DateTime.Now);
            connectStreamWriter.WriteLine("Proxy-agent: GOS Proxy Service");
            connectStreamWriter.WriteLine();
            connectStreamWriter.Flush();

            var sslStream = new SslStream(state.ClientStream, false);
            try
            {
                var certProvider = new CertificateProvider();

                bool created;
                var certificate = certProvider.LoadOrCreateCertificate(state.RemoteHost, out created);
                sslStream.AuthenticateAsServer(certificate, false, SslProtocols.Tls | SslProtocols.Ssl3 | SslProtocols.Ssl2, true);
            }
            catch (Exception ex)
            {
                WriteLog(state.Session, 0, "ERR", ex.Message);
                sslStream.Close();
                state.ClientStream.Close();
                connectStreamWriter.Close();
                return;
            }

            var nstate = new ClientConnectionState
            {
                Session = state.Session,
                Client = state.Client,
                ClientStream = sslStream,
                ClientStreamBase = (NetworkStream)state.ClientStream,
                Buffer = new byte[Globals.BufferSize],
                MessageStream = new MemoryStream(),
                IsSsl = true,
            };

            try
            {
                sslStream.BeginRead(nstate.Buffer, 0, nstate.Buffer.Length, ReadFromClient.Run, nstate);
            }
            catch (Exception ex)
            {
                WriteLog(state.Session, 0, "ERR", ex.Message);
                sslStream.Close();
                state.ClientStream.Close();
            }
        }


        private static IPAddress GetRemoteIp(string host)
        {
            try
            {
                var entry = Dns.GetHostEntry(host);
                var ips = entry.AddressList;
                return ips.FirstOrDefault();
            }
            catch (SocketException ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        private static ClientMessageHeader ParseClientMessage(Stream clientStream)
        {
            var streamReader = new StreamReader(clientStream);
            {
                clientStream.Position = 0;

                var httpCmd = String.Empty;

                for (var i = 0; i < 10; i++)
                {
                    httpCmd = streamReader.ReadLine();

                    if (String.IsNullOrEmpty(httpCmd))
                    {
                        continue;
                    }
                    if (httpCmd.Count(c => c.Equals(' ')) >= 2)
                    {
                        break;
                    }
                }

                if (String.IsNullOrEmpty(httpCmd))
                {
                    return null;
                }

                var splitBuffer = httpCmd.Split(new[] { ' ' }, 3);

                var method = splitBuffer[0];
                var remoteUri = splitBuffer[1];
                if (method.Equals("CONNECT", StringComparison.InvariantCultureIgnoreCase))
                {
                    remoteUri = "https://" + remoteUri;
                }
                var protocollVersion = splitBuffer[2];

                splitBuffer = protocollVersion.Split(new[] { '/' }, 2);
                var protocol = splitBuffer[0];
                var version = splitBuffer[1];

                var headers = new Dictionary<string, string>();
                while (true)
                {
                    var headerline = streamReader.ReadLine();
                    if (String.IsNullOrEmpty(headerline))
                    {
                        break;
                    }

                    var lineBuffer = headerline.Split(new[] { ':' }, 2);
                    var key = lineBuffer[0].Trim();
                    var value = lineBuffer[1].Trim();
                    headers.TryAdd(key, value);
                }

                if (remoteUri.StartsWith("/"))
                {
                    remoteUri = string.Format("https://{0}{1}", headers["Host"], remoteUri);
                }

                var message = new ClientMessageHeader
                {
                    Method = method,
                    RemoteUri = new Uri(remoteUri),
                    Protocol = protocol,
                    ProtocolVersion = Version.Parse(version),
                    Headers = headers
                };

                clientStream.Position = 0;

                return message;
            }
        }

        private static void WriteLog(RequestSession session, int read, string state, string message = "")
        {
            var url = session.TargetUrl;
            if (url.Length > 80)
            {
                url = url.Substring(0, 80);
            }
            session.WriteLoop++;
            session.WriteBytes = read;
            session.WriteTotalBytes += read;
            Logger.Log("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", session.Id, state, session.WriteLoop,
                session.WriteBytes, session.WriteTotalBytes, url, message);
        }
    }
}
