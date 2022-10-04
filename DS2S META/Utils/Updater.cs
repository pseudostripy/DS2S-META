using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.IO.Enumeration;
using System.Windows;
using System.Text.RegularExpressions;
using DS2S_META.Properties;

namespace DS2S_META.Utils
{
    public static class Updater
    {
        public static async void InitiateUpdate(Uri uri, string newver)
        {
            // Prog bar?
            var progbar = new METAUpdating() { Width = 350, Height = 100 };
            progbar.Show();

            // Get Download link from latest release:
            string urlpath = GetDownloadLink(uri.ToString(), newver);

            // Setup variables:
            int currprocid = Environment.ProcessId;
            string currexepath = Assembly.GetExecutingAssembly().Location;
            string? currdir = new FileInfo(currexepath).Directory?.FullName;
            if (currdir == null) throw new NullReferenceException("Seems to be unable to find folder of current .exe");
            var dirpar = Directory.GetParent(currdir);
            if (dirpar == null) throw new NullReferenceException("Stop installing things on root :) ");
            string parentdir = dirpar.ToString();
            string batchScriptName = $"{parentdir}\\tmpMetaUpdater.bat";
            
            
            // Download new release binary (.7z)
            Uri dlurl = new Uri(urlpath);
            string dlfname_ext = Path.GetFileName(dlurl.LocalPath);
            string dlOutfile = $"{parentdir}\\{dlfname_ext}";
            await HttpHelper.AsyncDownloadFile(dlurl, dlOutfile);

            // Unzip file contents to dir
            string dirname_url = Path.GetFileNameWithoutExtension(dlOutfile);
            string dirname = FixDirectoryName(dirname_url);
            string newdir_install = $"{parentdir}\\{dirname}";
            if (IsDuplicateDir(newdir_install)) return; // Do not force overwrite unexpected places
            Extract7zFile(dlOutfile, parentdir);        // auto-unzips into newdircheck
            File.Delete(dlOutfile);                     // remove the .7z binary

            
            // Prepare directory names for batch script:
            string newdir_reform_name = $"DS2S META"; // cannot be path!!
            string newdir_reform_dir = $"{parentdir}\\{newdir_reform_name}";
            string proctitle = "DS2S META";
            string newexepath = $"{newdir_reform_dir}\\{proctitle}.exe";

            // Save config stuff (to be loaded by new version)
            Settings Settings = Properties.Settings.Default;
            Settings.IsUpgrading = true;
            Settings.Save();
            string srcsettings = $"{currdir}\\DS2S META.config";
            string destsettings_tmp = $"{parentdir}\\_tmpsave.config"; // destination dir doesn't exist yet
            File.Copy(srcsettings, destsettings_tmp);

            // Safety check in case of previous error during update
            if (File.Exists(batchScriptName))
                File.Delete(batchScriptName);

            // Careful with this kinda stuff!
            using (StreamWriter writer = File.AppendText(batchScriptName))
            {
                // Wait for current process to end
                writer.WriteLine("cd ..");
                writer.WriteLine(":Loop");
                writer.WriteLine($"Tasklist /fi \"PID eq {currprocid}\" | find \":\"");
                writer.WriteLine("if Errorlevel 1 (");
                writer.WriteLine("  Timeout /T 1 /Nobreak");
                writer.WriteLine("  Goto Loop");
                writer.WriteLine(")");

                // Filesystem updates
                writer.WriteLine($"rmdir /s /Q \"{currdir}\"");    // silently remove dir & subfolders
                writer.WriteLine($"ren \"{newdir_install}\" \"{newdir_reform_name}\"");     // Rename new folder to DS2S META
                writer.WriteLine($"copy \"{destsettings_tmp}\" \"{newdir_reform_dir}\\DS2S META.config");
                writer.WriteLine($"del {destsettings_tmp}");

                // Run the new executable
                writer.WriteLine($"start \"{proctitle}\" \"{newexepath}\"");                
            }

            // Run the above batch file in new thread
            RunBatchFile(batchScriptName);
            Application.Current.Shutdown(); // End current process (triggers .bat takeover)
        }

        // Utility
        private static string GetDownloadLink(string repo, string newver)
        {
            string temp = repo.Replace("tag", "download");
            return $"{temp}/DS2S.META.{newver}.7z";
        }
        private static void RunBatchFile(string batfile)
        {
            // Run the above batch file in new thread
            ProcessStartInfo pro = new()
            {
                FileName = "cmd.exe",
                Arguments = $"/C {batfile} & Del {batfile}", // run and remove self
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(pro);
        }
        private static bool IsDuplicateDir(string? dirpath)
        {
            if (!Directory.Exists(dirpath))
                return false;
            
            string messageBoxText = "Cannot update since a folder with the updated name already exists in the (parent) directory";
            string caption = "Meta Updater";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            return true;
        }
        public static void Extract7zFile(string sourceArchive, string destination)
        {
            // I know this is duplicated directory finding, but its cleaner to leave this method atomic
            string currexepath = Assembly.GetExecutingAssembly().Location;
            string? currdir = new FileInfo(currexepath).Directory?.FullName;
            string zPath = $"{currdir}\\Resources\\DLLs\\7z.exe";
            try
            {
                ProcessStartInfo pro = new()
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = zPath,
                    Arguments = string.Format($"x \"{sourceArchive}\" -o\"{destination}\"")
                };
                Process? x = Process.Start(pro);
                x?.WaitForExit();
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message);
            }
        }
        private static string FixDirectoryName(string urldir)
        {
            // Removing the "." characters that github adds for spaces
            string pattern = @"\.\d";
            Regex re = new(pattern);
            Match match = re.Match(urldir);
            int index = match.Index;
            return "DS2S META " + urldir.Substring(index + 1);
        }
    }

    public static class HttpHelper
    {
        static readonly HttpClient client = new HttpClient();
        public static async Task AsyncDownloadFile(Uri uri, string opath)
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                //string responseBody = await response.Content.ReadAsStringAsync();
                var responseBody = await response.Content.ReadAsByteArrayAsync();

                File.WriteAllBytes(opath, responseBody);
                //Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
    }
}
