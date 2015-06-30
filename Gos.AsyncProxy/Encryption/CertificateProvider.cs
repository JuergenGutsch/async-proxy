using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Linq;

namespace GOS.AsyncProxy
{
    public class CertificateProvider
    {
        private X509Certificate2 _certRoot;
        private readonly string _makeCertLocation = "MakeCert.exe";
        private readonly string _makeCertParamsEE = "-pe -ss my -n \"CN={0}{1}\" -sky exchange -in \"{2}\" -is root -eku 1.3.6.1.5.5.7.3.1 -ir localmachine -sr localmachine -cy end -a sha1 -m 120 -b {3}{4}";
        private readonly string _makeCertParamsRoot = "-r -ss root -n \"CN={0}{1}\" -sky signature -eku 1.3.6.1.5.5.7.3.1 -sr localmachine -h 1 -cy authority -a sha1 -m 120 -b {3}{4}";
        private readonly string _certRootCN = "GOS";
        private readonly string _certSubjectO = ", O=gutsch-online e.K., OU=IT";

        private readonly Dictionary<string, X509Certificate2> _certServerCache;

        public CertificateProvider()
        {
            _certServerCache = new Dictionary<string, X509Certificate2>();
        }

        internal X509Certificate2 LoadOrCreateCertificate(string hostname, out bool attemptedCreation)
        {
            attemptedCreation = false;
            var x509Certificate2 = LoadCertificateFromWindowsStore(hostname);
            if (x509Certificate2 == null)
            {
                attemptedCreation = true;
                x509Certificate2 = CreateCertificate(hostname, false);
                if (x509Certificate2 == null)
                {
                    Logger.Log("Error Creating Certificate for" + hostname);
                }
            }
            return x509Certificate2;
        }

        private X509Certificate2 LoadCertificateFromWindowsStore(string sHostname, bool isRoot = false)
        {
            var storename = isRoot ? StoreName.Root : StoreName.My;
            var store = new X509Store(storename, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var str = string.Format("CN={0}{1}", sHostname, _certSubjectO);

            var enumerator = store.Certificates.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (current != null && !string.Equals(current.Subject, str, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                store.Close();
                return current;
            }
            store.Close();
            return null;
        }

        private X509Certificate2 GetRootCertificate()
        {
            if (_certRoot == null)
            {
                var x509Certificate2 = LoadCertificateFromWindowsStore(_certRootCN, true);
                _certRoot = x509Certificate2;
            }
            return _certRoot;
        }

        private X509Certificate2 CreateCertificate(string sHostname, bool isRoot)
        {
            var num = 0;
            var chrArray = new[] { ' ', '/' };
            if (sHostname.IndexOfAny(chrArray) == -1)
            {
                if (!isRoot && GetRootCertificate() == null)
                {
                    if (GetRootCertificate() == null && !CreateRootCertificate())
                    {
                        Logger.Log("Root Certificate doesn't exist!");
                        return null;
                    }
                }


                if (File.Exists(_makeCertLocation))
                {
                    X509Certificate2 x509Certificate22 = null;
                    string str;
                    if (!isRoot)
                    {
                        var stringPref = new object[5];
                        stringPref[0] = sHostname;
                        stringPref[1] = _certSubjectO;
                        stringPref[2] = _certRootCN;
                        stringPref[3] = DateTime.Now.AddDays(-7).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                        str = string.Format(_makeCertParamsEE, stringPref);
                    }
                    else
                    {
                        var objArray = new object[5];
                        objArray[0] = sHostname;
                        objArray[1] = _certSubjectO;
                        objArray[2] = _certRootCN;
                        objArray[3] = DateTime.Now.AddDays(-7).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                        str = string.Format(_makeCertParamsRoot, objArray);
                    }

                    string executableOutput = String.Empty;
                    try
                    {
                        X509Certificate2 x509Certificate2;
                        if (!_certServerCache.TryGetValue(sHostname, out x509Certificate2))
                        {
                            x509Certificate2 = LoadCertificateFromWindowsStore(sHostname);
                        }

                        if (x509Certificate2 == null)
                        {
                            executableOutput = ProcessUtilities.GetExecutableOutput(_makeCertLocation, str, out num);

                            if (num == 0)
                            {
                                int num2 = 6;
                                do
                                {
                                    x509Certificate22 = LoadCertificateFromWindowsStore(sHostname);
                                    Thread.Sleep(50 * (6 - num2));

                                    num2--;
                                }
                                while (x509Certificate22 == null && num2 >= 0);
                            }
                            if (x509Certificate22 != null)
                            {
                                if (!isRoot)
                                {
                                    _certServerCache[sHostname] = x509Certificate22;
                                }
                                else
                                {
                                    _certRoot = x509Certificate22;
                                }
                            }
                        }
                        else
                        {
                            return x509Certificate2;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }

                    if (x509Certificate22 == null)
                    {
                        string str2 = string.Format("Creation of the interception certificate failed.\n\nmakecert.exe returned {0}.\n\n{1}", num, executableOutput);
                        var objArray3 = new object[3];
                        objArray3[0] = _makeCertLocation;
                        objArray3[1] = str;
                        objArray3[2] = str2;
                        Logger.Log("GOS.CertMaker> [{0} {1}] Returned Error: {2} ", objArray3);

                    }
                    return x509Certificate22;
                }

                Logger.Log(string.Concat("Cannot locate:\n\t\"", _makeCertLocation, "\"\n\nPlease move makecert.exe to the GOS installation directory."), "MakeCert.exe not found");
                throw new FileNotFoundException(string.Concat("Cannot locate: ", _makeCertLocation, ". Please move makecert.exe to the GOS installation directory."));
            }
            return null;
        }

        private bool CreateRootCertificate()
        {
            return null != CreateCertificate(_certRootCN, true);
        }
    }
}
