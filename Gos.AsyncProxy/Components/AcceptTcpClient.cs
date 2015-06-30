using System;
using System.IO;
using System.Net.Sockets;
using GOS.AsyncProxy.AsyncStates;

namespace GOS.AsyncProxy.Components
{
    public class AcceptTcpClient
    {
        public static void Run(IAsyncResult result)
        {
            Logger.Log("start AcceptTcpClient.Run");
            if (result.IsCompleted == false || !(result.AsyncState is TcpListener))
            {
                return;
            }

            Logger.Log("load state");
            var tcpListener = result.AsyncState as TcpListener;

            try
            {
                Logger.Log("tcpListener.EndAcceptTcpClient");
                var tcpClient = tcpListener.EndAcceptTcpClient(result);

                tcpClient.ReceiveBufferSize = Globals.BufferSize;
                tcpClient.SendBufferSize = Globals.BufferSize;

                Logger.Log("tcpClient.GetStream");
                var clientStream = tcpClient.GetStream();

                Logger.Log("new ClientConnectionState");
                var state = new ClientConnectionState
                {
                    Session = new RequestSession(),
                    Client = tcpClient,
                    ClientStream = clientStream,
                    ClientStreamBase = clientStream,
                    Buffer = new byte[Globals.BufferSize],
                    MessageStream = new MemoryStream(),
                    IsSsl = false
                };
                Logger.Log("clientStream.BeginRead");
                clientStream.BeginRead(state.Buffer, 0, state.Buffer.Length, ReadFromClient.Run, state);
            }
            catch (Exception ex)
            {
                Logger.Log("Error while attempting to complete async connection.");
                Logger.Log(ex);
            }

            // auf den nächsten Client warten und reagieren
            Logger.Log("tcpListener.BeginAcceptTcpClient");
            tcpListener.BeginAcceptTcpClient(AcceptTcpClient.Run, tcpListener);
            Logger.Log("end AcceptTcpClient.Run");
        }
    }
}
