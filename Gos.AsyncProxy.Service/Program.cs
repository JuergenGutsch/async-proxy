using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;
using GOS.AsyncProxy;
using GOS.AsyncProxy.Service.Properties;

namespace GOS.AsyncProxy.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            var install = false;
            var uninstall = false;
            var rethrow = false;

            try
            {
                foreach (var arg in args)
                {
                    switch (arg)
                    {
                        case "-i":
                        case "-install":
                            install = true;
                            break;
                        case "-u":
                        case "-uninstall":
                            uninstall = true;
                            break;
                        default:
                            Console.Error.WriteLine("Argument not expected: " + arg);
                            break;
                    }
                }

                if (uninstall)
                {
                    Install(true, args);
                }
                if (install)
                {
                    Install(false, args);
                }

                if (!(install || uninstall))
                {
                    rethrow = true; // so that windows sees error...
                    var servicesToRun = new ServiceBase[]
                        {
                            new CallProxy()
                        };
                    ServiceBase.Run(servicesToRun);
                    rethrow = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                if (rethrow)
                {
                    throw;
                }
            }
        }

        static void Install(bool undo, string[] args)
        {
            try
            {
                Logger.Log(undo ? "Uninstalling ..." : "Installing ... ");
                using (var inst = new AssemblyInstaller(typeof(Program).Assembly, args))
                {
                    var state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        if (undo)
                        {
                            inst.Uninstall(state);
                        }
                        else
                        {
                            inst.Install(state);
                            inst.Commit(state);

                            StartService();
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Logger.Log(ex);
                            inst.Rollback(state);
                        }
                        catch { }
                        throw;
                    }
                    inst.Dispose();
                }
                Logger.Log("... finished");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private static void StartService()
        {
            var procStartInfo = new ProcessStartInfo("NET", "START " + Settings.Default.ServiceName)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            var proc = new Process { StartInfo = procStartInfo };
            if (proc.Start())
            {
                Logger.Log("Service started");
                var result = proc.StandardOutput.ReadToEnd();
                Logger.Log(result);
            }
        }
    }
}
