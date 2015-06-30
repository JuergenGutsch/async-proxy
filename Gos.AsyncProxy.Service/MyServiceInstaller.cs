using System.ComponentModel;
using System.ServiceProcess;
using GOS.AsyncProxy.Service.Properties;

namespace GOS.AsyncProxy.Service
{
    [RunInstaller(true)]
    public sealed class MyServiceInstaller : ServiceInstaller
    {
        public MyServiceInstaller()
        {
            Description = Settings.Default.Description;
            DisplayName = Settings.Default.DisplayName;
            ServiceName = Settings.Default.ServiceName;
            StartType = ServiceStartMode.Automatic;
        }
    }
}