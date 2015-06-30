using System;
using System.IO;
using System.Net.Security;
using GOS.AsyncProxy.AsyncStates;

namespace GOS.AsyncProxy.Components
{
    public class ConnectToRemote
    {
        public static void Run(IAsyncResult asyncResult)
        {
            var state = asyncResult.AsyncState as RemoteConnectionState;
            if (state != null)
            {
                try
                {
                    state.RemoteClient.EndConnect(asyncResult);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return;
                }

                state.RemoteClient.ReceiveBufferSize = Globals.BufferSize;
                state.RemoteClient.SendBufferSize = Globals.BufferSize;

                var remoteStreamBase = state.RemoteClient.GetStream();
                Stream remoteStream = remoteStreamBase;

                if (state.IsSsl)
                {
                    var sslStream = new SslStream(remoteStream, false);
                    sslStream.AuthenticateAsClient(state.RemoteHost);

                    remoteStream = sslStream;
                }
                var buffer = state.MessageStream.ToArray();

                var writeState = new WriteToRemoteStreamState
                    {
                        Session = state.Session,
                        RemoteClient = state.RemoteClient,
                        RemoteStream = remoteStream,
                        RemoteStreamBase = remoteStreamBase,
                        Client = state.Client,
                        ClientStream = state.ClientStream
                    };

                remoteStream.BeginWrite(buffer, 0, buffer.Length, WriteToRemote.Run, writeState);
            }
        }

    }
}
