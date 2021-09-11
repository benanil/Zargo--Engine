#pragma warning disable CA1416 // Validate platform compatibility
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;

namespace ZargoEngine.Helper
{
    public static class AdminRelauncher
    {
        public static void RelaunchIfNotAdmin()
        {
            if (!OperatingSystem.IsWindows()) return;

            if (!RunningAsAdmin())
            {
                Console.WriteLine("Running as admin required!");
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = Assembly.GetEntryAssembly().Location;
                proc.Verb = "runas";
                try{
                    Process.Start(proc);
                    Environment.Exit(0);
                }
                catch (Exception ex){
                    Console.WriteLine("This program must be run as an administrator! \n\n" + ex.ToString());
                    Environment.Exit(0);
                }
            }
            else{
                Debug.Log("application running in admin mode");
            }
        }

        private static bool RunningAsAdmin()
        {
            bool isAdmin;
            try{
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException){
                isAdmin = false;
            }
            catch (Exception ex){
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}