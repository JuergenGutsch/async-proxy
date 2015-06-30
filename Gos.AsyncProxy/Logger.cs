using System;
using System.Diagnostics;
using System.IO;

namespace GOS.AsyncProxy
{
    public class Logger
    {
        private static readonly object _lock = new object();

        static Logger()
        {
            if (!EventLog.SourceExists("GOS"))
            {
                EventLog.CreateEventSource("GOS", "Proxy");
            }
        }

        public static void Log(string message, params object[] args)
        {
            var messageToLog = String.Format(message, args);
            lock (_lock)
            {
                var filename = string.Format("{0}.txt", DateTime.Now.ToString("yyyy-MM-dd"));

                using (var fs = new FileStream(filename, FileMode.Append))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(messageToLog);
                        Console.WriteLine(messageToLog);
                    }
                }
            }
        }

        public static void Log(Exception ex)
        {
            Log("ERROR: {0}\n{1}", ex.Message, ex.ToString());
        }
    }
}