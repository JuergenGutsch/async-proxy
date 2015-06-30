using System;
using System.Threading;
using GOS.AsyncProxy.AsyncStates;

namespace GOS.AsyncProxy.Components
{
    public class ReadFromRemote
    {
        public static void Run(IAsyncResult asyncResult)
        {
            Thread.Sleep(30);
            var state = asyncResult.AsyncState as ReceiveFromRemoteState;
            if (state == null)
            {
                return;
            }

            var read = 0;
            try
            {
                read = state.RemoteStream.EndRead(asyncResult);
                state.ClientStream.Write(state.Buffer, 0, read);
                WriteLog(state.Session, read, "OK");
            }
            catch (Exception ex)
            {
                WriteLog(state.Session, read, "ERR", ex.Message);
                CloseStreams(state);
                return;
            }

            if (state.RemoteStreamBase.CanRead && state.RemoteStreamBase.DataAvailable)
            {
                try
                {
                    state.RemoteStream.BeginRead(state.Buffer, 0, state.Buffer.Length, ReadFromRemote.Run, state);
                }
                catch (Exception ex)
                {
                    WriteLog(state.Session, read, "ERR", ex.Message);
                }
            }
            else
            {
                var i = 0;
                while (i < 5)
                {
                    Thread.Sleep(250);
                    if (state.RemoteStreamBase.CanRead && state.RemoteStreamBase.DataAvailable)
                    {
                        try
                        {
                            state.RemoteStream.BeginRead(state.Buffer, 0, state.Buffer.Length, ReadFromRemote.Run, state);
                        }
                        catch (Exception ex)
                        {
                            WriteLog(state.Session, read, "ERR", ex.Message);
                            return;
                        }
                    }
                    i++;
                }
                CloseStreams(state);
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


        private static void CloseStreams(ReceiveFromRemoteState state)
        {
            state.RemoteClient.Close();
            state.RemoteStream.Close();
            state.Client.Close();
            state.ClientStream.Close();
        }
    }
}
