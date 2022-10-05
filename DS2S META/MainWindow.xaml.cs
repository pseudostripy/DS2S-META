using Bluegrams.Application;
using Octokit;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Fields/Properties
        private string MetaVersion = "Version_Undefined";
        private Properties.Settings Settings;
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
        Timer UpdateTimer = new Timer();
        private bool ShowUpdateComplete { get; set; } = false;

        public MainWindow()
        {
            PortableSettingsProvider.SettingsFileName = "DS2S Meta.config";
            PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);
            Settings = Properties.Settings.Default;
            InitializeComponent();
            GetVersion();
            LoadSettingsAfterUpgrade();
            ShowOnlineWarning();

            Hook.OnHooked += Hook_OnHooked;
        }

        private void Hook_OnHooked(object? sender, PHEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                EnableTabs(Hook.Loaded);
            }));
        }

        public void GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var assemblyver = assembly.GetName().Version;
            MetaVersion = assemblyver == null ? "version undefined" : assemblyver.ToString();
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

            string updaterlog = "..\\updaterlog.log";
            if (!File.Exists(updaterlog))
                throw new Exception("Cannot find expected log after update");
            using (StreamWriter logwriter = File.AppendText(updaterlog))
            {
                logwriter.WriteLine("Update complete!");
                logwriter.WriteLine("Removing log file");
            };
            File.Delete(updaterlog);
        }
        private void ShowOnlineWarning()
        {
            if (Settings.ShowWarning)
            {
                var warning = new METAWarning()
                {
                    Title = "Online Warning",
                    Width = 350,
                    Height = 240
                };
                warning.ShowDialog();
            }
        }
        private void ShowMetaUpdateWindow(Uri link, string ackverstring)
        {
            var warning = new METAUpdate(link, ackverstring)
            {
                Title = "New Update Available",
                Width = 450,
                Height = 215
            };
            warning.ShowDialog();
        }
        private void ShowUpdateCompleteWindow()
        {
            if (!ShowUpdateComplete)
                return;

            ShowUpdateComplete = false;
            cbxUpdateOK.Visibility = Visibility.Visible;
            this.Activate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var assemblyver = assembly.GetName().Version;
            MetaVersion = assemblyver == null ? "version undefined" : assemblyver.ToString();
            
            lblWindowName.Content = $"DS2 Scholar META {MetaVersion}";
            EnableTabs(false);
            InitAllTabs();

            VersionUpdateCheck("Nordgaren");
            VersionUpdateCheck("Pseudostripy"); // Randomizer updates

            UpdateTimer.Interval = 16;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            UpdateTimer.Enabled = true;
        }

        private async void VersionUpdateCheck(string repo_owner)
        {
            try
            {
                GitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue("DS2S-META"));
                Release release = await gitHubClient.Repository.Release.GetLatest(repo_owner, "DS2S-META");
                Version gitVersion = Version.Parse(release.TagName.ToLower().Replace("v", ""));
                Version exeVersion = Version.Parse(MetaVersion);
                if (gitVersion > exeVersion) //Compare latest version to current version
                {
                    link.NavigateUri = new Uri(release.HtmlUrl);
                    lblNewVersion.Visibility = Visibility.Visible;
                    labelCheckVersion.Visibility = Visibility.Hidden;

                    // Only show msg again when newer version released
                    if (Properties.Settings.Default.AcknowledgeUpdateVersion != gitVersion.ToString())
                        ShowMetaUpdateWindow(link.NavigateUri, gitVersion.ToString());
                }
                else if (gitVersion == exeVersion)
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

            if (Hook.SpeedhackDllPtr != IntPtr.Zero)
            {
                Hook.DisableSpeedhack();
                Hook.Free(Hook.SpeedhackDllPtr);
            }
            
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
            CheckFocused();
        }

        private void InitAllTabs()
        {
            metaItems.InitTab();
            metatabDmgCalc.InitTab();
            metaPlayer.InitTab();
            InitHotkeys();
        }
        private void UpdateProperties()
        {
            Hook.UpdateStatsProperties();
            Hook.UpdatePlayerProperties();
            Hook.UpdateInternalProperties();
            Hook.UpdateBonfireProperties();
            Hook.UpdateCovenantProperties();
        }
        private void EnableTabs(bool enable)
        {
            metaPlayer.EnableCtrls(enable);
            metaStats.EnableCtrls(enable);
            metaBonfire.EnableCtrls(enable);
            metaInternal.EnableCtrls(enable);
            metaItems.EnableCtrls(enable);
            metaCovenant.EnableCtrls(enable);
        }
        private void ReloadAllTabs()
        {
            metaPlayer.ReloadCtrl();
            metaStats.ReloadCtrl();
            metaItems.ReloadCtrl();
            metaBonfire.ReloadCtrl();
        }
        private void UpdateAllTabs()
        {
            metaPlayer.UpdateCtrl();
            metaStats.UpdateCtrl();
            metaBonfire.UpdateCtrl();
            metaItems.UpdateCtrl();
        }

        private void link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = e.Uri.ToString(), UseShellExecute = true });
        }

        private void SaveAllTabs()
        {
            SaveHotkeys();
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

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MainWindowClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SpawnUndroppable_Checked(object sender, RoutedEventArgs e)
        {
            metaItems.UpdateCreateEnabled();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void cbxUpdateOK_Checked(object sender, RoutedEventArgs e)
        {
            cbxUpdateOK.IsChecked = false;
            cbxUpdateOK.Visibility = Visibility.Hidden;
        }
    }
}
