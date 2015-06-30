using System;
using GOS.AsyncProxy.AsyncStates;

namespace GOS.AsyncProxy.Components
{
    public class WriteToRemote
    {
        public static void Run(IAsyncResult asyncResult)
        {
            var state = asyncResult.AsyncState as WriteToRemoteStreamState;
            if (state != null)
            {
                state.RemoteStream.EndWrite(asyncResult);
                var readState = new ReceiveFromRemoteState
                    {
                        Session = state.Session,
                        Buffer = new byte[Globals.BufferSize],
                        RemoteClient = state.RemoteClient,
                        RemoteStream = state.RemoteStream,
                        RemoteStreamBase = state.RemoteStreamBase,
                        Client = state.Client,
                        ClientStream = state.ClientStream

                    };

                state.RemoteStream.BeginRead(readState.Buffer, 0, readState.Buffer.Length, ReadFromRemote.Run, readState);
            }
        }
    }
}
