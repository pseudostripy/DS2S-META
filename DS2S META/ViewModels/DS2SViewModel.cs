using DS2S_META.Properties;
using DS2S_META.Utils;
using Octokit;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static DS2S_META.MainWindow;

namespace DS2S_META.ViewModels
{
    internal class DS2SViewModel : ObservableObject
    {
        private Settings Settings;
        public DS2SHook Hook { get; private set; }
        public bool GameLoaded { get; set; }
        public bool Reading
        {
            get => DS2SHook.Reading;
            set => DS2SHook.Reading = value;
        }

        private MetaVersionInfo MVI = new();

        public string WindowName => $"META {MVI.MetaVersionStr}";

        public static bool DesignMode
        {
            get { return DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()); }
        }

        public DS2SViewModel()
        {
            if (DesignMode)
                return; // maaaaybe fixes designer unhandled crash :thinking:?
                

            Hook = new DS2SHook(5000, 5000);
            Hook.OnHooked += Hook_OnHooked;
            Hook.OnUnhooked += Hook_OnUnhooked;
            Hook.Start();

            DmgCalcViewModel = new DmgCalcViewModel();
            ViewModels.Add(DmgCalcViewModel);

            GetVersions();
            VersionUpdate();
        }
        
        public Brush ForegroundID
        {
            get
            {
                if (Hook.ID != "Not Hooked")
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }
        public string ContentLoaded
        {
            get
            {
                if (Hook.Loaded)
                    return "Yes";
                return "No";
            }
        }
        public Brush ForegroundLoaded
        {
            get
            {
                if (Hook.Loaded)
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }
        public string ContentOnline
        {
            get
            {
                if (!Hook.Hooked)
                    return string.Empty;

                if (Hook.Online)
                    return "Yes";
                return "No";
            }
        }
        public Brush ForegroundOnline
        {
            get
            {
                if (!Hook.Hooked)
                    return Brushes.Black;

                if (Hook.Online)
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }
        public Brush ForegroundVersion
        {
            get
            {
                if (!Hook.Hooked)
                    return Brushes.Black;

                if (Hook.IsValidVer)
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }
        public string CheckVer 
        {
            get 
            {
                return MVI.UpdateStatus switch
                {
                    UPDATE_STATUS.UPTODATE => "App up to date",
                    UPDATE_STATUS.OUTOFDATE => "New update available!",
                    UPDATE_STATUS.INDEVELOPMENT => "In-development version",
                    UPDATE_STATUS.UNCHECKABLE => "Unsure if up-to-date",
                    UPDATE_STATUS.UNKNOWN_VER => "Unknown Meta version",
                    _ => throw new Exception("Impossible case")
                };
            }
        }


        ObservableCollection<ViewModelBase> ViewModels = new();
        public DmgCalcViewModel DmgCalcViewModel { get; set; }

        public void UpdateMainProperties()
        {
            OnPropertyChanged(nameof(ForegroundID));
            OnPropertyChanged(nameof(ContentLoaded));
            OnPropertyChanged(nameof(ForegroundLoaded));
            OnPropertyChanged(nameof(ContentOnline));
            OnPropertyChanged(nameof(ForegroundOnline));
            OnPropertyChanged(nameof(ForegroundVersion));
            OnPropertyChanged(nameof(GameLoaded));

            DmgCalcViewModel.UpdateViewModel();
        }

        private void Hook_OnHooked(object? sender, PHEventArgs e)
        {
        }
        private void Hook_OnUnhooked(object? sender, PHEventArgs e)
        {

        }




        private void GetVersions()
        {
            // Gets the current assembly version for Meta and the 
            // most recent release version
            #if DRYUPDATE
                GetVersionsDry();
            #else
                GetVersionsStandard();
            #endif
        }
        private void GetVersionsDry()
        {
            // NOTE: !Can only get here in DryUpdate build configurations!
            Updater.EnsureDryUpdateSettings();
            string ini_file = Updater.DryUpdateSettingsPath;

            var updateini = JsonSerializer.Deserialize<UpdateIni>(ini_file);
            if (updateini == null) throw new Exception($"Error deserializing {Updater.DryUpdateSettingsPath}");

            MVI.ExeVersion = Version.Parse(updateini.ThisVer);
            MVI.LatestReleaseURI = new Uri(updateini.UpdatePath);


            //Regex re = new(@"DS2S\.META\.(?<ver>.*)\."); // find e.g. "a.b.c" until last ".7z/.zip"
            //var M = re.Match(filepath);
            //if (!M.Success)
            //    throw new Exception("Error reading the expected file name for update file");
            //MVI.GitVersion = Version.Parse(M.Groups["ver"].ToString());
            //MVI.DryUpdateFilePath = filepath;
        }
        private void GetVersionsStandard()
        {
            MVI.ExeVersion = Updater.GetExeVersion();

            var latestRel = Updater.GitLatestRelease("Pseudostripy").Result;
            if (latestRel == null) return;

            MVI.GitVersion = Version.Parse(latestRel.TagName.ToLower().Replace("v", ""));
            MVI.LatestReleaseURI = new Uri(latestRel.HtmlUrl);
        }

        private void VersionUpdate()
        {
            
            
            if (MVI.GitVersion > MVI.ExeVersion) //Compare latest version to current version
            {
                lblNewVersion.Visibility = Visibility.Visible;
                labelCheckVersion.Visibility = Visibility.Hidden;

                // Only show msg again when newer version released
                if (Settings.Default.AcknowledgeUpdateVersion != MVI.GitVersionStr)
                    ShowMetaUpdateWindow(linkNewVersionAvail.NavigateUri, MVI.GitVersionStr);
            }
        }
        

        

        private void ShowOnlineWarning()
        {
            if (!Settings.ShowWarning) return;
            var warning = new METAWarning("Online Warning", 350, 240);
            warning.ShowDialog();
        }
        public static void ShowMetaUpdateWindow(Uri link, string ackverstring)
        {
            var warning = new METAUpdate(link, ackverstring)
            {
                Title = "New Update Available",
                Width = 450,
                Height = 280
            };
            warning.ShowDialog();
        }
        public void ShowUpdateCompleteWindow()
        {
            if (!ShowUpdateComplete)
                return;

            ShowUpdateComplete = false;
            TitleGrid.RowDefinitions[3].MaxHeight = 100;
            cbxUpdateOK.Visibility = Visibility.Visible;
            Activate();
        }


        



        



        private async void VersionUpdateCheck(string repo_owner)
        {
            try
            {
#if !DRYUPDATE
                    // Get Repo Version:
                    GitHubClient gitHubClient = new(new ProductHeaderValue("DS2S-META"));
                    Release release = await gitHubClient.Repository.Release.GetLatest(repo_owner, "DS2S-META");
                    MVI.GitVersion = Version.Parse(release.TagName.ToLower().Replace("v", ""));
#endif
                if (MVI.GitVersion > MVI.ExeVersion) //Compare latest version to current version
                {
#if DRYUPDATE
                    if (DryUpdateMissingIni)
                    {
                        MVI.GitVersion = MVI.ExeVersion;
                        linkNewVersionAvail.NavigateUri = new Uri("");
                    }
                    else
                    {
                        if (MVI.DryUpdateFilePath == null) throw new Exception("Null file path");
                        linkNewVersionAvail.NavigateUri = new Uri(MVI.DryUpdateFilePath);
                    }
#else
                        // Store info:
                        MVI.LatestReleaseURI = new Uri(release.HtmlUrl);
                        MVI.LatestRelease = release;
                        link.NavigateUri = new Uri(release.HtmlUrl);
#endif

                    lblNewVersion.Visibility = Visibility.Visible;
                    labelCheckVersion.Visibility = Visibility.Hidden;

                    // Only show msg again when newer version released
                    if (Properties.Settings.Default.AcknowledgeUpdateVersion != MVI.GitVersionStr)
                        ShowMetaUpdateWindow(linkNewVersionAvail.NavigateUri, MVI.GitVersionStr);
                }
                else if (MVI.GitVersion == MVI.ExeVersion)
                    labelCheckVersion.Content = "App up to date";
                else
                    labelCheckVersion.Content = "In-development version.";
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is ApiException || ex is ArgumentException)
            {
                labelCheckVersion.Content = "Current app version unknown";
            }
            catch (Exception ex)
            {
                labelCheckVersion.Content = "Something is very broke, contact DS2 META repo owner";
                MessageBox.Show(ex.Message);
            }
        }

    }
}
