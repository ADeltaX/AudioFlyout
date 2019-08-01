using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioFlyoutLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var file = AppDomain.CurrentDomain.BaseDirectory + "AudioFlyoutUA.exe";
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.FileName = file;
                Process.Start(psi);
            }
            catch (Exception)
            {
            }
        }
    }
}
