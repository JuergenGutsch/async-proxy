using System;
using System.Diagnostics;
using System.Text;

namespace GOS.AsyncProxy
{
    class ProcessUtilities
    {
        public static string GetExecutableOutput(string sExecute, string sParams, out int iExitCode)
        {
            iExitCode = -999;
            var stringBuilder = new StringBuilder();
            var strArrays = new string[5];
            strArrays[0] = "Results from ";
            strArrays[1] = sExecute;
            strArrays[2] = " ";
            strArrays[3] = sParams;
            strArrays[4] = "\r\n\r\n";
            stringBuilder.Append(string.Concat(strArrays));
            try
            {
                var process = new Process
                    {
                        StartInfo =
                            {
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = false,
                                CreateNoWindow = true,
                                FileName = sExecute,
                                Arguments = sParams
                            }
                    };
                process.Start();
                while (true)
                {
                    var str = process.StandardOutput.ReadLine();
                    var str1 = str;
                    if (str == null)
                    {
                        break;
                    }
                    str1 = str1.TrimEnd(new char[0]);
                    if (str1 != string.Empty)
                    {
                        stringBuilder.Append(string.Concat(str1, "\r\n"));
                    }
                }
                iExitCode = process.ExitCode;
                process.Dispose();
            }
            catch (Exception exception1)
            {
                var exception = exception1;
                stringBuilder.Append(string.Concat("Exception thrown: ", exception.ToString(), "\r\n", exception.StackTrace));
            }
            stringBuilder.Append("-------------------------------------------\r\n");
            return stringBuilder.ToString();
        }
    }
}
