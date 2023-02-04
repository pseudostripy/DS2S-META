using Bluegrams.Application;
using Octokit;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DS2S_META.Utils;
using System.Text.Json;
using System.Threading.Tasks;

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Eventually most of this should go into a MainWindowViewModel like in ERDebugTool

        // Fields/Properties
        
        private Properties.Settings Settings;
        public HotkeyManager HKM;
        DS2SHook Hook => ViewModel.Hook;
        bool FormLoaded
        {
            get => ViewModel.GameLoaded;
            set => ViewModel.GameLoaded = value;
        }
        public bool Reading
        {
            get => ViewModel.Reading;
            set => ViewModel.Reading = value;
        }
        Timer UpdateTimer = new();
        private bool ShowUpdateComplete { get; set; } = false;
        

        //public DmgCalcViewModel DmgCalcViewModel { get; set; }

        public MainWindow()
        {
            PortableSettingsProvider.SettingsFileName = "DS2S Meta.config";
            PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);
            Settings = Properties.Settings.Default;
            HKM = new(this);
            InitializeComponent();




            GetMetaVersion();
            LoadSettingsAfterUpgrade();
            ShowOnlineWarning();
            Hook.OnHooked += Hook_OnHooked;
            Hook.MW = this;

            // This is duplicated in the ViewModel until DS2ViewModel is fixed accordingly
            //DmgCalcViewModel = new DmgCalcViewModel();
            //ViewModels.Add(DmgCalcViewModel);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            
            EnableTabs(false);
            InitAllTabs();

            //VersionUpdateCheck("Nordgaren"); // Race condition bug :/
            VersionUpdateCheck("Pseudostripy"); // Randomizer updates
            //lblWindowName.Content = $"META {MVI.MetaVersionStr}";

            UpdateTimer.Interval = 16;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            UpdateTimer.Enabled = true;
        }

        //ObservableCollection<ViewModelBase> ViewModels = new();

        private void Hook_OnHooked(object? sender, PHEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                EnableTabs(Hook.Loaded);
            }));
        }

        public void GetMetaVersion()
        {
            #if DRYUPDATE
                GetVersionsDry();
            #else
                GetVersions()
                Assembly assembly = Assembly.GetExecutingAssembly();
                MVI.ExeVersion = assembly.GetName().Version;
            #endif
        }
        
        private void GetVersions()
        {
            // Gets the current assembly version for Meta and the 
            // most recent release version

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


            Regex re = new(@"DS2S\.META\.(?<ver>.*)\."); // find e.g. "a.b.c" until last ".7z/.zip"
            var M = re.Match(filepath);
            if (!M.Success)
                throw new Exception("Error reading the expected file name for update file");
            MVI.GitVersion = Version.Parse(M.Groups["ver"].ToString());
            MVI.DryUpdateFilePath = filepath;
        }
        
        private void GetUpdateVersion()
        {
            var latestRelease = GitLatestRelease("Pseudostripy").Result;
            if (latestRelease == null) 
                return; // default string

            MVI.GitVersion = Version.Parse(latestRelease.TagName.ToLower().Replace("v", ""));
        }

        private void LoadSettingsAfterUpgrade()
        {
            if (!Settings.IsUpgrading)
                return;

            try
            {
                // Load previous settings from .config
                Settings.Upgrade();
            }
            catch (ConfigurationErrorsException)
            {
                // Incompatible settings, keep current
            }

            Settings.IsUpgrading = false;
            Settings.Save();
            ShowUpdateComplete = true;

            string updaterlog = Updater.LogPath;
            if (!File.Exists(updaterlog))
            {
                MessageBox.Show("Cannot find log file to remove.");
                return;
            }
                
            using (StreamWriter logwriter = File.AppendText(updaterlog))
            {
                logwriter.WriteLine("Update complete!");
                logwriter.WriteLine("Removing log file");
            };

#if !DEBUG
                File.Delete(updaterlog);
#endif
        }
        
        private void ShowOnlineWarning()
        {
            if (!Settings.ShowWarning) return;
            var warning = new METAWarning("Online Warning", 350, 240);
            warning.ShowDialog();
        }
        private static void ShowMetaUpdateWindow(Uri link, string ackverstring)
        {
            var warning = new METAUpdate(link, ackverstring)
            {
                Title = "New Update Available",
                Width = 450,
                Height = 280
            };
            warning.ShowDialog();
        }
        private void ShowUpdateCompleteWindow()
        {
            if (!ShowUpdateComplete)
                return;

            ShowUpdateComplete = false;
            TitleGrid.RowDefinitions[3].MaxHeight = 100;
            cbxUpdateOK.Visibility = Visibility.Visible;
            Activate();
        }

        
        public class MetaVersionInfo
        {
            public Version? GitVersion { get; set; }
            public Version? ExeVersion { get; set; }
            public Release? LatestRelease { get; set; }
            public Uri? LatestReleaseURI { get; set; }
            public string MetaVersionStr => ExeVersion == null ? "Version Undefined" : ExeVersion.ToString();
            public string GitVersionStr => GitVersion == null ? string.Empty : GitVersion.ToString();
            public string? DryUpdateFilePath { get; set; }
        }



        private static async Task<Release?> GitLatestRelease(string repo_owner)
        {
            GitHubClient gitHubClient = new(new ProductHeaderValue("DS2S-META"));
            Release? release = default;
            try
            {
                release = await gitHubClient.Repository.Release.GetLatest(repo_owner, "DS2S-META");
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is ApiException || ex is ArgumentException)
            {
                // These are OK
            }
            return release;
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
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Hook.EnableSpeedFactors)
                Hook.EnableSpeedFactors = false;

            Hook.ClearSpeedhackInject();


            UpdateTimer.Stop();
            SaveAllTabs();

            if (RandomizerControl.IsRandomized && Properties.Settings.Default.ShowWarnRandoExit)
            {
                var randoexit = new RandoExitWarning()
                {
                    Title = "Game Randomized Warning",
                    Width = 375,
                    Height = 195,
                };
                randoexit.ShowDialog();
            }

            HKM.ClearHooks();
            Settings.Save();
        }

        


        private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => ShowUpdateCompleteWindow()));

            Dispatcher.Invoke(new Action(() =>
            {
                UpdateMainProperties();
                if (Hook.Hooked)
                {
                    if (Hook.Loaded && Hook.Setup)
                    {
                        if (!FormLoaded)
                        {
                            FormLoaded = true;
                            Reading = true;
                            ReloadAllTabs();
                            Reading = false;
                            EnableTabs(true);
                        }
                        else
                        {
                            Reading = true;
                            UpdateProperties();
                            //UpdateAllViewModels();
                            UpdateAllTabs();
                            Reading = false;
                        }
                    }
                    else if (FormLoaded)
                    {
                        Reading = true;
                        UpdateProperties();
                        Hook.UpdateName();
                        EnableTabs(false);
                        FormLoaded = false;
                        Reading = false;
                    }
                }
            }));
            
        }
        private void UpdateMainProperties()
        {
            Hook.UpdateMainProperties();
            ViewModel.UpdateMainProperties();
            //HKM.UpdateHotkeyRegistration(Hook.Focused);
            HKM.CheckFocusEvent(Hook.Focused);
        }

        private void InitAllTabs()
        {
            metaItems.InitTab();
            metatabDmgCalc.InitTab();
            metaPlayer.InitTab();
            metaSettings.InitTab(HKM);
            ViewModel.DmgCalcViewModel.InitViewModel(Hook);
        }
        private void UpdateProperties()
        {
            Hook.UpdateStatsProperties();
            Hook.UpdatePlayerProperties();
            Hook.UpdateInternalProperties();
            Hook.UpdateBonfireProperties();
            Hook.UpdateCovenantProperties();
        }
        //private void UpdateAllViewModels()
        //{
        //    foreach(var vm in ViewModels)
        //        vm.UpdateViewModel();
        //}

        private void EnableTabs(bool enable)
        {
            metaPlayer.EnableCtrls(enable);
            metaStats.EnableCtrls(enable);
            //metaBonfire.EnableCtrls(enable);
            metaInternal.EnableCtrls(enable);
            metaItems.EnableCtrls(enable);
            //metaCovenant.EnableCtrls(enable);
            metatabDmgCalc.EnableCtrls(enable);
        }
        private void ReloadAllTabs()
        {
            metaPlayer.ReloadCtrl();
            metaStats.ReloadCtrl();
            metaItems.ReloadCtrl();
            //metaBonfire.ReloadCtrl();
            metatabDmgCalc.ReloadCtrl();
        }
        private void UpdateAllTabs()
        {
            metaPlayer.UpdateCtrl();
            metaStats.UpdateCtrl();
            //metaBonfire.UpdateCtrl();
            metaItems.UpdateCtrl();
        }

        

        private void link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (MVI.LatestReleaseURI == null) return;
            ShowMetaUpdateWindow(MVI.LatestReleaseURI, MVI.GitVersionStr);
        }
        private void SaveAllTabs()
        {
            //HKM.SaveHotkeys();
            //HKM.UnregisterHotkeys();
        }
        private void EnableStatEditing_Checked(object sender, RoutedEventArgs e)
        {
            metaStats.EnableCtrls(Hook.Loaded);
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void cbxUpdateOK_Checked(object sender, RoutedEventArgs e)
        {
            cbxUpdateOK.IsChecked = false;
            cbxUpdateOK.Visibility = Visibility.Hidden;
            TitleGrid.RowDefinitions[3].MaxHeight = 1;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void MainWindowClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
