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

namespace DS2S_META.Utils
{
    public static class Updater
    {
        public static async void InitiateUpdate()
        {
            // Setup variables:
            int currprocid = Environment.ProcessId;
            string currexepath = Assembly.GetExecutingAssembly().Location;
            string? currdir = new FileInfo(currexepath).Directory?.FullName;
            if (currdir == null) throw new NullReferenceException("Seems to be unable to find folder of current .exe");
            //string parentdir = $"{currdir}\\..";
            string testdir = "C:\\Users\\adaml\\Documents\\Coding\\TESTING";
            string testdeldir = $"{testdir}\\TESTDIR";
            string batchScriptName = $"{testdir}\\tmpMetaUpdater.bat";
            //string newprocdirtest = $"{parentdir}\\DS2S META 0.5.1.3"; // soon correct
            
            // Download new release binary (.7z)
            string urlpath = @"https://github.com/pseudostripy/DS2S-META/releases/download/0.6.0.0/DS2S.META.0.6.0.0.7z";
            Uri dlurl = new Uri(urlpath);
            string dlfname_ext = Path.GetFileName(dlurl.LocalPath);
            string dlOutfile = $"{testdir}\\{dlfname_ext}";
            await HttpHelper.AsyncDownloadFile(dlurl, dlOutfile);

            // Unzip file contents to dir
            string dirname_url = Path.GetFileNameWithoutExtension(dlOutfile);
            string dirname = FixDirectoryName(dirname_url);
            string newdir_install = $"{testdir}\\{dirname}";
            if (IsDuplicateDir(newdir_install)) return; // Do not force overwrite unexpected places
            Extract7zFile(dlOutfile, testdir);  // auto-unzips into newdircheck
            File.Delete(dlOutfile);             // remove the .7z binary

            bool check = Directory.Exists(newdir_install);

            // Prepare directory names for batch script:
            string newdir_reform_name = $"DS2S META"; // cannot be path!!
            string proctitle = "DS2S META";
            string newexepath = $"{testdir}\\{newdir_reform_name}\\{proctitle}.exe";

            // Save config stuff:
            // TODO

            // Safety check in case of previous error during update
            if (File.Exists(batchScriptName))
                File.Delete(batchScriptName);

            // Careful with this kinda stuff!
            using (StreamWriter writer = File.AppendText(batchScriptName))
            {
                writer.WriteLine("cd ..");
                writer.WriteLine(":Loop");
                writer.WriteLine($"Tasklist /fi \"PID eq {currprocid}\" | find \":\"");
                writer.WriteLine("if Errorlevel 1 (");
                writer.WriteLine("  Timeout /T 1 /Nobreak");
                writer.WriteLine("  Goto Loop");
                writer.WriteLine(")");
                writer.WriteLine($"rmdir /s /Q \"{currdir}\"");    // silently remove dir & subfolders
                //
                writer.WriteLine($"ren \"{newdir_install}\" \"{newdir_reform_name}\"");     // Rename new folder to DS2S META
                writer.WriteLine($"start \"{proctitle}\" \"{newexepath}\"");                // Run the new executable
            }

            // Run the above batch file in new thread
            RunBatchFile(batchScriptName);
            Application.Current.Shutdown(); // End current process (triggers .bat takeover)
        }

        // Utility
        private static void RunBatchFile(string batfile)
        {
            // Run the above batch file in new thread
            Process.Start(
                new ProcessStartInfo()
                {
                    Arguments = $"/C {batfile} & Del {batfile}", // run and remove self
                    //WindowStyle = ProcessWindowStyle.Hidden,
                    WindowStyle = ProcessWindowStyle.Normal,
                    //CreateNoWindow = true,
                    CreateNoWindow = false,
                    FileName = "cmd.exe"
                }
            );
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
