using System.ComponentModel;
using System.ServiceProcess;

namespace GOS.AsyncProxy.Service
{
    [RunInstaller(true)]
    public sealed class MyServiceProcessInstaller : ServiceProcessInstaller
    {
        public MyServiceProcessInstaller()
        {
            Account = ServiceAccount.LocalSystem;
        }
    }
}