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
using System.CodeDom;
using System.Reflection.Metadata;

namespace DS2S_META.Utils
{
    public static class Updater
    {
        public static async void InitiateUpdate(Uri uri, string newver)
        {
            // Prog bar?
            var progbar = new METAUpdating() { Width = 350, Height = 100 };
            progbar.Show();

            // File System setup
            if (!GetDirectories(out var currdir, out var parentdir))
                return;

            string batchScriptName;
            string updaterlog = $"{parentdir}\\updaterlog.log";
            if (File.Exists(updaterlog))
                File.Delete(updaterlog); // start from scratch
            using (StreamWriter logwriter = File.AppendText(updaterlog))
            {
                logwriter.WriteLine("> Log Creation");

                // Download new release binary (.7z)
                string urlpath = GetDownloadLink(uri.ToString(), newver);
                Uri dlurl = new(urlpath);
                logwriter.WriteLine($"File to download: \"{urlpath}\"");
                string dlfname_ext = Path.GetFileName(dlurl.LocalPath);
                string dlOutfile = $"{parentdir}\\{dlfname_ext}";
                logwriter.WriteLine($"Downloading to: \"{dlOutfile}\"");
                logwriter.WriteLine($"Download starting...");
                await HttpHelper.AsyncDownloadFile(dlurl, dlOutfile);
                logwriter.WriteLine("Download complete!");
                if (!File.Exists(dlOutfile))
                {
                    logwriter.WriteLine("Downloaded file found check: fail");
                    logwriter.WriteLine($"Download file not found at: {dlOutfile}. Exiting update.");
                    return;
                }
                logwriter.WriteLine("Downloaded file found check: success");

                // Unzip file contents to dir
                string dirname_url = Path.GetFileNameWithoutExtension(dlOutfile);
                string dirname = FixDirectoryName(dirname_url);
                string newdir_install = $"{parentdir}\\{dirname}";
                logwriter.WriteLine($"Checking install directory at: \"{newdir_install}\"");
                if (IsDuplicateDir(newdir_install))
                {
                    logwriter.WriteLine("Extraction directory check: fail");
                    logwriter.WriteLine($"Cannot extract update to directory: \"{newdir_install}\" as it is not empty. Exiting update.");
                    return; // Do not force overwrite unexpected places
                }
                logwriter.WriteLine("Extraction directory check: success");
                logwriter.WriteLine($"Extracting \"{dlOutfile}\" to \"{newdir_install}\"");
                var watch = new Stopwatch();
                watch.Start();
                int maxtimeout = 10000; // 10s
                bool extraction_timeout = Extract7zFile(dlOutfile, parentdir, maxtimeout); // auto-unzips into newdircheck
                watch.Stop();
                bool extraction_files_found = File.Exists($"{newdir_install}\\DS2S META.exe");
                if (extraction_timeout)
                    logwriter.WriteLine($"Extraction process timeout, likely missing executable. To investigate. Exiting update.");
                else
                    // didn't timeout
                    if (extraction_files_found)
                        logwriter.WriteLine($"Extraction successful in {watch.ElapsedMilliseconds}ms"); // files copied & fast
                    else
                        logwriter.WriteLine($"Extraction process ended prematurely. Exiting update."); // medium case to investigate
                if (extraction_timeout || !extraction_files_found)
                    return;
                logwriter.WriteLine("Removing downloaded zip file...");
                File.Delete(dlOutfile); // remove the .7z binary
                if (File.Exists(dlOutfile))
                {
                    logwriter.WriteLine($"Issue removing the download zip file at: \"{dlOutfile}\"");
                }
                logwriter.WriteLine($"Downloaded Zip file: \"{dlOutfile}\" successfully removed");


                // Prepare directory names for batch script:
                string newdir_reform_name = $"DS2S META"; // cannot be path!!
                string newdir_reform_dir = $"{parentdir}\\{newdir_reform_name}";
                string proctitle = "DS2S META";
                string newexepath = $"{newdir_reform_dir}\\{proctitle}.exe";

                // Save config stuff (to be loaded by new version)
                logwriter.WriteLine("Saving user settings .config file");
                Settings Settings = Properties.Settings.Default;
                Settings.IsUpgrading = true;
                logwriter.WriteLine("Meta flag set for isUpdating.");
                Settings.Save();
                logwriter.WriteLine("Settings saved successfully.");
                string srcsettings = $"{currdir}\\DS2S META.config";
                string destsettings_tmp = $"{parentdir}\\_tmpsave.config"; // destination dir doesn't exist yet
                logwriter.WriteLine($"Copying saved settings file to temporary location: \"{destsettings_tmp}\"");
                File.Copy(srcsettings, destsettings_tmp);
                if (!File.Exists(destsettings_tmp))
                {
                    logwriter.WriteLine($"Issue copying settings file to \"{destsettings_tmp}\". Exiting update.");
                    return;
                }
                logwriter.WriteLine($"Settings file copied successfully to \"{destsettings_tmp}\"");

                // Batch file cretion (be careful with this stuff)!
                int currprocid = Environment.ProcessId;
                batchScriptName = $"{parentdir}\\tmpMetaUpdater.bat";
                logwriter.WriteLine($"Checking for batch file from corrupted update");

                // write to this in the batch file whilst normal log is locked by this thread
                string updaterlog2 = $"{parentdir}\\updaterlog2.log"; 

                // Safety check:
                if (File.Exists(batchScriptName))
                {
                    logwriter.WriteLine($"Old batch file found at \"{batchScriptName}\"");
                    File.Delete(batchScriptName);
                    if (File.Exists(batchScriptName))
                    {
                        logwriter.WriteLine($"Error removing old batch file at \"{batchScriptName}\". Exiting update.");
                            return;
                    }
                    logwriter.WriteLine($"Old batch file successfully removed. Continuing update.");
                }
                logwriter.WriteLine("No previous batch files require removal.");

                logwriter.WriteLine("Creating batch file to takeover execution after closing this thread.");
                using (StreamWriter writer = File.AppendText(batchScriptName))
                {
                    // Wait for current process to end
                    writer.WriteLine($"SET logfile=\"{updaterlog}\""); // setup variable
                    writer.WriteLine($"SET logfiletwo=\"{updaterlog2}\""); // setup variable
                    writer.WriteLine("ECHO Batch file execution started successfully. >> %logfiletwo%"); // >> = append
                    writer.WriteLine("ECHO Waiting for previous DS2S META version process to end >> %logfiletwo%");
                    writer.WriteLine("cd ..");
                    writer.WriteLine(":Loop");
                    writer.WriteLine($"Tasklist /fi \"PID eq {currprocid}\" | find \":\"");
                    writer.WriteLine("if Errorlevel 1 (");
                    writer.WriteLine("  Timeout /T 1 /Nobreak");
                    writer.WriteLine("  Goto Loop");
                    writer.WriteLine(")");

                    // Now the file is definitely unlocked:
                    writer.WriteLine($"ECHO Transferring messages from logupdater2.log >> %logfile%");
                    writer.WriteLine($"ECHO Batch file execution started successfully. >> %logfile%"); // >> = append
                    writer.WriteLine($"ECHO Waiting for previous DS2S META version process to end >> %logfile%");

                    // Continue as normal:
                    writer.WriteLine("ECHO Old DS2S META process ended successfully >> %logfile%");
                    writer.WriteLine("ECHO Removing logupdater2.log >> %logfile%");
                    writer.WriteLine($"del {updaterlog2}");
                    writer.WriteLine($"ECHO logupdater2.log removed succesfully >> %logfile%");
                    writer.WriteLine($"ECHO Removing old running folder >> %logfile%");
                    writer.WriteLine($"rmdir /s /Q \"{currdir}\"");    // silently remove dir & subfolders
                    writer.WriteLine($"ECHO Copying directory: {newdir_install} to: {newdir_reform_dir} >> %logfile%");
                    writer.WriteLine($"ren \"{newdir_install}\" \"{newdir_reform_name}\"");     // Rename new folder to DS2S META
                    writer.WriteLine($"ECHO Copying temp settings file to {newdir_reform_dir}\\DS2S META.config >> %logfile%");
                    writer.WriteLine($"copy \"{destsettings_tmp}\" \"{newdir_reform_dir}\\DS2S META.config");
                    writer.WriteLine($"ECHO Removing temp settings file >> %logfile%");
                    writer.WriteLine($"del {destsettings_tmp}");
                    writer.WriteLine("ECHO Fixing working directory for new DS2S META thread >> %logfile%");
                    writer.WriteLine($"cd {newdir_reform_dir}");

                    // Run the new executable
                    writer.WriteLine($"ECHO Starting updated meta.exe process >> %logfile%");
                    writer.WriteLine($"start \"{proctitle}\" \"{newexepath}\"");
                    writer.WriteLine($"ECHO Ending this batch script execution and deleting myself >> %logfile%");
                }
                if (!File.Exists(batchScriptName))
                {
                    logwriter.WriteLine($"Error creating batch script file at: \"{batchScriptName}\". Exiting update.");
                }
                logwriter.WriteLine($"Batch script created successfully at: \"{batchScriptName}\"");
                

                // Run the above batch file in new thread
                Process? batchproc = RunBatchFile(batchScriptName);
                if (batchproc == null)
                {
                    logwriter.WriteLine("Batch file is a null process. Exiting update.");
                    return;
                }
                int batchprocid = batchproc.Id;

                // Ensure the batch file started:
                watch = new Stopwatch();
                watch.Start();
                int customtimeout_ms = 1000; // really shouldn't take this long to boot a process
                bool batchprocfound = false;
                while (watch.ElapsedMilliseconds < customtimeout_ms)
                {
                    Process[] processlist = Process.GetProcesses();
                    batchprocfound = processlist.Any(pr => pr.Id == batchprocid);

                    if (batchprocfound)
                        break;
                }
                
                if (batchprocfound)
                    logwriter.WriteLine("Batch script started successfully");
                else
                {
                    logwriter.WriteLine($"Batch script process failed to start within {customtimeout_ms}ms, probably something more fundamentally wrong. Exiting update.");
                    return;
                }
                logwriter.WriteLine("Ending old meta execution and passing over to batch script");
            }

            // wait until batch file process has started before killing this one.
            Application.Current.Shutdown(); // End current process (triggers .bat takeover)
        }

        // Utility
        private static string GetDownloadLink(string repo, string newver)
        {
            string temp = repo.Replace("tag", "download");
            return $"{temp}/DS2S.META.{newver}.7z";
        }
        private static Process? RunBatchFile(string batfile)
        {
            // Run the above batch file in new thread
            ProcessStartInfo pro = new()
            {
                FileName = "cmd.exe",
                Arguments = $"/C {batfile} & Del {batfile}", // run and remove self
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            return Process.Start(pro);
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
        public static bool Extract7zFile(string sourceArchive, string destination, int maxtimeout)
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
                bool? exited = x?.WaitForExit(maxtimeout);
                bool didtimeout = !(exited ?? false);
                return didtimeout;
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
        private static bool GetDirectories(out string currdir, out string parentdir)
        {
            string currexepath = Assembly.GetExecutingAssembly().Location;
            var currdir_null = new FileInfo(currexepath).Directory?.FullName;
            if (currdir_null == null) throw new NullReferenceException("Seems to be unable to find folder of current .exe");
            currdir = currdir_null;
            var dirpar = Directory.GetParent(currdir);
            if (dirpar == null) throw new NullReferenceException("Stop installing things on root :) ");
            parentdir = dirpar.ToString();
            return true;
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
