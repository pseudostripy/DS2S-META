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
        public static string ExeDir => GetTxtResourceClass.ExeDir ?? string.Empty;
        public static string ExeParentDir => Directory.GetParent(ExeDir)?.FullName ?? string.Empty;
        public static string LogPath => Path.Combine(ExeParentDir, "logupdater.txt");
        public static readonly string FinalDirName = $"DS2S META"; // after update

        private static readonly Settings Settings = Settings.Default;

        public static async Task<bool> WrapperInitiateUpdate(Uri uri, string newver)
        {
            // Prog bar?
            var progbar = new METAUpdating() { Width = 350, Height = 100 };
            progbar.Show();

            var success = await InitiateUpdate(uri, newver);
            if (success)
                return true;

            // Failure:
            // Throw warning to user that update failed
            MessageBox.Show($"Update failed, please check {LogPath}", "Update error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            progbar.Close();
            return success;
        }

        public static async Task<bool> InitiateUpdate(Uri uri, string newver)
        {
            // Start new log:
            File.Delete(LogPath);

            string batchScriptName;
            
            using (StreamWriter logwriter = File.AppendText(LogPath))
            {
                logwriter.WriteLine("> Log Creation");

                string localCopyPath;

                if (uri.IsFile)
                {
                    // file scheme (update from local zip file)
                    localCopyPath = Path.Combine(ExeParentDir,Path.GetFileName(uri.LocalPath));
                    logwriter.WriteLine($"Copying source file {uri.LocalPath} to dest: {localCopyPath}");
                    bool tempfile_already_exists = File.Exists(localCopyPath);
                    if (tempfile_already_exists)
                    {
                        logwriter.WriteLine("Error, destination file already exists");
                        return false;
                    }
                    File.Copy(uri.LocalPath, localCopyPath, true);
                    bool copy_incomplete = !File.Exists(localCopyPath);
                    if (copy_incomplete)
                    {
                        logwriter.WriteLine("Error in creating local file copy from source");
                        return false; 
                    }
                }
                else 
                {
                    // http scheme (download release binary from github)
                    string urlpath = GetDownloadLink(uri.ToString(), newver);
                    Uri dlurl = new(urlpath);
                    logwriter.WriteLine($"File to download: \"{urlpath}\"");
                    string dlfname_ext = Path.GetFileName(dlurl.LocalPath);
                    localCopyPath = $"{ExeParentDir}\\{dlfname_ext}";
                    logwriter.WriteLine($"Downloading to: \"{localCopyPath}\"");
                    logwriter.WriteLine($"Download starting...");
                    await HttpHelper.AsyncDownloadFile(dlurl, localCopyPath);

                    bool dl_file_not_found = !File.Exists(localCopyPath);
                    if (dl_file_not_found)
                    {
                        logwriter.WriteLine("Downloaded file found check: fail");
                        logwriter.WriteLine($"Download file not found at: {localCopyPath}. Exiting update.");
                        return false;
                    }
                    logwriter.WriteLine("Download complete!");
                }


                // Unzip file contents to dir
                //string dirname_url = Path.GetFileNameWithoutExtension(localCopyPath);
                string newdir_install = Path.Combine(ExeParentDir,"__temp_copy_ds2s_meta__");
                if (Directory.Exists(newdir_install))
                    Directory.Delete(newdir_install, true);

                // Just delete this folder, no way anyone has anything useful in there!
                bool install_folder_uncleanable = Directory.Exists(newdir_install);
                if (install_folder_uncleanable)
                {
                    logwriter.WriteLine("Cannot write to install folder, likely access violation from open file");
                    return false;
                }
                Directory.CreateDirectory(newdir_install); // remake it fresh

                
                //logwriter.WriteLine($"Checking install directory at: \"{newdir_install}\"");
                //if (IsDuplicateDir(newdir_install))
                //{
                //    logwriter.WriteLine("Extraction directory check: fail");
                //    logwriter.WriteLine($"Cannot extract update to directory: \"{newdir_install}\" as it is not empty. Exiting update.");
                //    // Do not force overwrite unexpected places
                //    return false;
                //}
                //logwriter.WriteLine("Extraction directory check: success");
                logwriter.WriteLine($"Extracting \"{localCopyPath}\" to \"{newdir_install}\"");
                var watch = new Stopwatch();
                watch.Start();
                int maxtimeout = 10000; // 10s
                bool extraction_timeout = Extract7zFile(localCopyPath, newdir_install, maxtimeout); // auto-unzips into newdircheck
                watch.Stop();

                if (extraction_timeout)
                {
                    logwriter.WriteLine($"Extraction process timeout, likely missing executable. To investigate. Exiting update.");
                    return false;
                }

                
                // Extraction filesystem cleanup:
                var dirs = Directory.GetDirectories(newdir_install);
                if (dirs.Length != 1)
                {
                    logwriter.WriteLine("Issue with the extraction, unexpected number of folders created");
                    return false;
                }
                var extractedDirName = dirs[0];

                bool extraction_files_missing = !File.Exists($"{extractedDirName}\\DS2S META.exe");
                if (extraction_files_missing)
                {
                    logwriter.WriteLine($"Extraction process ended prematurely. Exiting update.");
                    return false;
                }

                var temp_dir2 = Path.Combine(ExeParentDir, "__temp_extract_ds2_meta__"); // needs a new folder for move
                Directory.Move(extractedDirName, temp_dir2);

                bool dirmove_err = !Directory.Exists(temp_dir2);
                if (dirmove_err)
                {
                    logwriter.WriteLine("Error in moving inner extracted folder into own folder in parent dir");
                    return false;
                }
                Directory.Delete(newdir_install, true);

                bool temp1_removal_err = Directory.Exists(newdir_install);
                if (temp1_removal_err)
                {
                    logwriter.WriteLine("Error removing first temporary install folder");
                    return false;
                }
                logwriter.WriteLine($"Extraction successful in {watch.ElapsedMilliseconds}ms");
                
                logwriter.WriteLine("Removing local copy of zip file...");
                File.Delete(localCopyPath); // remove the .7z binary
                bool local_copy_not_deleted = File.Exists(localCopyPath);

                if (local_copy_not_deleted)
                {
                    logwriter.WriteLine($"Issue removing the local copy (or downloaded copy) zip file at: \"{localCopyPath}\"");
                    return false;
                }
                logwriter.WriteLine($"Zip file: \"{localCopyPath}\" successfully removed");


                // Prepare directory names for batch script:
                string newdir_reform_dir = $"{ExeParentDir}\\{FinalDirName}";
                string proctitle = "DS2S META";
                string newexepath = $"{newdir_reform_dir}\\{proctitle}.exe";

                // Save config stuff (to be loaded by new version)
                logwriter.WriteLine("Saving user settings .config file");
                Settings.IsUpgrading = true;
                logwriter.WriteLine("Meta flag set for isUpdating.");
                Settings.Save();
                logwriter.WriteLine("Settings saved successfully.");
                string srcsettings = $"{ExeDir}\\DS2S META.config";
                string destsettings_tmp = $"{ExeParentDir}\\_tmpsave.config"; // destination dir doesn't exist yet
                logwriter.WriteLine($"Copying saved settings file to temporary location: \"{destsettings_tmp}\"");
                File.Copy(srcsettings, destsettings_tmp);
                if (!File.Exists(destsettings_tmp))
                {
                    logwriter.WriteLine($"Issue copying settings file to \"{destsettings_tmp}\". Exiting update.");
                    return false;
                }
                logwriter.WriteLine($"Settings file copied successfully to \"{destsettings_tmp}\"");

                // Batch file cretion (be careful with this stuff)!
                int currprocid = Environment.ProcessId;
                batchScriptName = $"{ExeParentDir}\\tmpMetaUpdater.bat";
                logwriter.WriteLine($"Checking for batch file from corrupted update");

                // write to this in the batch file whilst normal log is locked by this thread
                string updaterlog2 = $"{ExeParentDir}\\updaterlog2.log"; 

                // Safety check:
                if (File.Exists(batchScriptName))
                {
                    logwriter.WriteLine($"Old batch file found at \"{batchScriptName}\"");
                    File.Delete(batchScriptName);
                    if (File.Exists(batchScriptName))
                    {
                        logwriter.WriteLine($"Error removing old batch file at \"{batchScriptName}\". Exiting update.");
                            return false;
                    }
                    logwriter.WriteLine($"Old batch file successfully removed. Continuing update.");
                }
                logwriter.WriteLine("No previous batch files require removal.");

                logwriter.WriteLine("Creating batch file to takeover execution after closing this thread.");
                using (StreamWriter writer = File.AppendText(batchScriptName))
                {
                    // Wait for current process to end
                    writer.WriteLine($"SET logfile=\"{LogPath}\""); // setup variable
                    writer.WriteLine($"SET logfiletwo=\"{updaterlog2}\""); // setup variable
                    writer.WriteLine("ECHO Batch file execution started successfully. >> %logfiletwo%"); // >> = append
                    writer.WriteLine("ECHO Waiting for previous DS2S META version process to end >> %logfiletwo%");
                    writer.WriteLine("cd ..");
                    writer.WriteLine($"ECHO Current working directory changed to %cd% >> %logfiletwo%");
                    writer.WriteLine($"ECHO Searching for process pid = {currprocid} >> %logfiletwo%");
                    writer.WriteLine(":Loop");
                    writer.WriteLine("ECHO Current errorlevel = %errorlevel% >> %logfiletwo%");
                    writer.WriteLine($"ECHO tasklist result...>> %logfiletwo%");
                    writer.WriteLine($"tasklist /fi \"PID eq {currprocid}\" >> %logfiletwo%");
                    writer.WriteLine($"Tasklist /fi \"PID eq {currprocid}\" | find \":\"");
                    writer.WriteLine("if Errorlevel 1 (");
                    writer.WriteLine($"ECHO Process PID {currprocid} is still active, waiting 1s >> %logfiletwo%");
                    writer.WriteLine("  Timeout /T 1 /Nobreak");
                    writer.WriteLine("  Goto Loop");
                    writer.WriteLine(")");
                    writer.WriteLine("ECHO Broke out of wait loop, META process should be ended >> %logfiletwo%");

                    // Now the file is definitely unlocked:
                    writer.WriteLine($"ECHO Transferring messages from logupdater2.log >> %logfile%");
                    writer.WriteLine($"type \"{updaterlog2}\" >> \"{LogPath}\"");
                    
                    // Continue as normal:
                    writer.WriteLine("ECHO Old DS2S META process ended successfully >> %logfile%");
                    writer.WriteLine("ECHO Removing logupdater2.log >> %logfile%");
#if !DEBUG
                    writer.WriteLine($"del \"{updaterlog2}\"");
#endif
                    writer.WriteLine($"ECHO logupdater2.log removed succesfully >> %logfile%");
                    writer.WriteLine($"ECHO Removing old running folder >> %logfile%");
                    writer.WriteLine($"rmdir /s /Q \"{ExeDir}\"");    // silently remove dir & subfolders
                    writer.WriteLine($"ECHO Renaming directory: {temp_dir2} to: {newdir_reform_dir} >> %logfile%");
                    writer.WriteLine($"ren \"{temp_dir2}\" \"{FinalDirName}\"");     // Rename new folder to DS2S META
                    writer.WriteLine($"ECHO Copying temp settings file to {newdir_reform_dir}\\DS2S META.config >> %logfile%");
                    writer.WriteLine($"copy \"{destsettings_tmp}\" \"{newdir_reform_dir}\\DS2S META.config");
                    writer.WriteLine($"ECHO Removing temp settings file >> %logfile%");
                    writer.WriteLine($"del \"{destsettings_tmp}\"");
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
                    return false;
                }
                logwriter.WriteLine($"Batch script created successfully at: \"{batchScriptName}\"");
                

                // Run the above batch file in new thread
                Process? batchproc = RunBatchFile(batchScriptName);
                if (batchproc == null)
                {
                    logwriter.WriteLine("Batch file is a null process. Exiting update.");
                    return false;
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


                if (!batchprocfound)
                {
                    logwriter.WriteLine($"Batch script process failed to start within {customtimeout_ms}ms, " +
                                        $"probably something more fundamentally wrong. Exiting update.");
                    return false;
                }


                logwriter.WriteLine($"Batch script started successfully with PID {batchprocid}");
                logwriter.WriteLine("Ending old meta execution and passing over to batch script");
            }

            // wait until batch file process has started before killing this one.
            Application.Current.Shutdown(); // End current process (triggers .bat takeover)
            return true; // I guess unreachable
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
                Arguments = $"/C \"\"{batfile}\" & Del \"{batfile}\"\"", // run and remove self
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            return Process.Start(pro);
        }
        //private static bool IsDuplicateDir(string? dirpath)
        //{
        //    if (!Directory.Exists(dirpath))
        //        return false;
            
        //    string messageBoxText = "Cannot update since a folder with the updated name already exists in the (parent) directory";
        //    string caption = "Meta Updater";
        //    MessageBoxButton button = MessageBoxButton.OK;
        //    MessageBoxImage icon = MessageBoxImage.Warning;
        //    MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        //    return true;
        //}
        public static bool Extract7zFile(string sourceArchive, string destination, int maxtimeout)
        {
            // I know this is duplicated directory finding, but its cleaner to leave this method atomic
            string currexepath = Assembly.GetExecutingAssembly().Location;
            string? currdir = new FileInfo(currexepath).Directory?.FullName;
            string zPath = $"{currdir}\\Resources\\Tools\\7zip\\7z.exe";
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
        //private static string FixDirectoryName(string urldir)
        //{
        //    // Removing the "." characters that github adds for spaces
        //    string pattern = @"\.\d";
        //    Regex re = new(pattern);
        //    Match match = re.Match(urldir);
        //    int index = match.Index;
        //    return "DS2S META " + urldir.Substring(index + 1);
        //}
        //private static bool GetDirectories(out string currdir, out string parentdir)
        //{
        //    string currexepath = Assembly.GetExecutingAssembly().Location;
        //    var currdir_null = new FileInfo(currexepath).Directory?.FullName;
        //    if (currdir_null == null) throw new NullReferenceException("Seems to be unable to find folder of current .exe");
        //    currdir = currdir_null;
        //    var dirpar = Directory.GetParent(currdir);
        //    if (dirpar == null) throw new NullReferenceException("Stop installing things on root :) ");
        //    parentdir = dirpar.ToString();
        //    return true;
        //}
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
