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
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using static DS2S_META.MainWindow;

namespace DS2S_META.ViewModels
{
    internal class DS2SViewModel : ObservableObject
    {
        // Wrapper exposures:
        private Settings Settings = Settings.Default;
        public DS2SHook Hook { get; private set; }
        //public bool Reading
        //{
        //    get => DS2SHook.Reading;
        //    set => DS2SHook.Reading = value;
        //}

        public bool GameLoaded => Hook.InGame;
        public bool DS2Loading => Hook.IsLoading;

        private MetaVersionInfo MVI = new();

        public string WindowName => $"META {MVI.MetaVersionStr}";

        public static bool DesignMode
        {
            get { return DesignerProperties.GetIsInDesignMode(new DependencyObject()); }
        }

        public DS2SViewModel()
        {
            if (DesignMode)
                return; // maaaaybe fixes designer unhandled crash :thinking:?
                

            Hook = new DS2SHook(5000, 5000);
            Hook.OnHooked += AllTabsOnHooked;
            Hook.OnUnhooked += AllTabsOnUnhooked;
            Hook.OnGameStateHandler += OnGameStateChange;

            // Setup ViewModels
            PlayerViewModel = new PlayerViewModel();
            DmgCalcViewModel = new DmgCalcViewModel();
            CheatsViewModel = new CheatsViewModel();
            RandoSettingsViewModel = new RandoSettingsViewModel();
            ViewModels.Add(DmgCalcViewModel);
            ViewModels.Add(CheatsViewModel);
            ViewModels.Add(PlayerViewModel);
            ViewModels.Add(RandoSettingsViewModel);


            Hook.Start();
            ShowOnlineWarning();
            Versioning();
        }

        private void AllTabsOnHooked(object? sender, PHEventArgs e)
        {
            PlayerViewModel.OnHooked();
            CheatsViewModel.OnHooked();
        }
        private void AllTabsOnUnhooked(object? sender, PHEventArgs e)
        {
            PlayerViewModel.OnUnHooked();
            CheatsViewModel.OnUnHooked();
        }


        // Binding Variables:
        public string ContentLoaded
        {
            get
            {
                if (Hook.InGame)
                    return "Yes";
                return "No";
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
        public Brush ForegroundID
        {
            get
            {
                if (Hook.ID != "Not Hooked")
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }
        public Brush ForegroundLoaded
        {
            get
            {
                if (Hook.InGame)
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
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
        
        public Visibility CheckVerVis => MVI.UpdateStatus != UPDATE_STATUS.OUTOFDATE ? Visibility.Visible : Visibility.Hidden;
        public Visibility NewVerVis => MVI.UpdateStatus == UPDATE_STATUS.OUTOFDATE ? Visibility.Visible : Visibility.Hidden;

        // ViewModel helpers
        ObservableCollection<ViewModelBase> ViewModels = new();
        public DmgCalcViewModel DmgCalcViewModel { get; set; }
        public RandoSettingsViewModel RandoSettingsViewModel { get; set; }
        public CheatsViewModel CheatsViewModel { get; set; }
        public PlayerViewModel PlayerViewModel { get; set; }

        public void UpdateMainProperties()
        {
            OnPropertyChanged(nameof(ForegroundID));
            OnPropertyChanged(nameof(ContentLoaded));
            OnPropertyChanged(nameof(ForegroundLoaded));
            OnPropertyChanged(nameof(ContentOnline));
            OnPropertyChanged(nameof(ForegroundOnline));
            OnPropertyChanged(nameof(ForegroundVersion));
            OnPropertyChanged(nameof(GameLoaded));
            OnPropertyChanged(nameof(DS2Loading)); // not used yet

            DmgCalcViewModel.UpdateViewModel();
            CheatsViewModel.UpdateViewModel();
            PlayerViewModel.UpdateViewModel();

        }

        public void InitViewModels()
        {
            foreach (var vm in ViewModels)
                vm.InitViewModel(Hook);
        }

        private void OnGameStateChange(object? sender, GameStateEventArgs e)
        {
            if (e.GameState == Hook.MAINMENU)
                AllTabsOnMainMenu();
            if (e.GameState == Hook.LOADEDINGAME) // add more here
                AllTabsOnInGame();
        }
        
        private void AllTabsOnInGame()
        {
            PlayerViewModel.OnInGame();
            CheatsViewModel?.OnInGame();
            //PlayerViewModel.EnableCtrls(enable);
            //metaStats.EnableCtrls(enable);
            //metaInternal.EnableCtrls(enable);
            //metaItems.EnableCtrls(enable);
            //metatabDmgCalc.EnableCtrls(enable);
            // ViewModel.RandoSettingsViewModel.enablectrls // todo
        }

        private void AllTabsOnMainMenu()
        {
            PlayerViewModel.OnMainMenu();
            
            //metaPlayer.EnableOnHooked(enable);
            //metaStats.EnableCtrls(enable);
            //metaInternal.EnableCtrls(enable);
            //metaItems.EnableCtrls(enable);
            //metatabDmgCalc.EnableCtrls(enable);
        }

        

        private async void Versioning()
        {
            await GetVersions();
            VersionUpdate();
            MVI.UpdateStatus = MVI.SyncUpdateStatus();
            OnPropertyChanged(nameof(CheckVer));
            OnPropertyChanged(nameof(CheckVerVis));
            OnPropertyChanged(nameof(NewVerVis));
        }

        private async Task GetVersions()
        {
            // Gets the current assembly version for Meta and the 
            // most recent release version
            #if DRYUPDATE
                GetVersionsDry();
            #else
                await GetVersionsStandard();
            #endif
        }
        private void GetVersionsDry()
        {
            // NOTE: !Can only get here in DryUpdate build configurations!
            Updater.EnsureDryUpdateSettings();
            
            var jsonstring = File.ReadAllText(Updater.DryUpdateSettingsPath);
            var updateini = JsonSerializer.Deserialize<UpdateIni>(jsonstring);
            if (updateini == null) throw new Exception($"Error deserializing {Updater.DryUpdateSettingsPath}");

            MVI.ExeVersion = Version.Parse(updateini.ThisVer);
            MVI.LatestReleaseURI = new Uri(updateini.UpdatePath);
            MVI.GitVersion = ParseUpdateFilename(MVI);            
        }
        private async Task GetVersionsStandard()
        {
            MVI.ExeVersion = Updater.GetExeVersion();

            var latestRel = await Updater.GitLatestRelease("Pseudostripy");
            if (latestRel == null) return; 

            MVI.GitVersion = Version.Parse(latestRel.TagName.ToLower().Replace("v", ""));
            MVI.LatestReleaseURI = new Uri(latestRel.HtmlUrl);
            OnPropertyChanged(nameof(CheckVer)); // required because async

            MVI.UpdateStatus = MVI.SyncUpdateStatus();
        }
        private static Version? ParseUpdateFilename(MetaVersionInfo MVI)
        {
            if (MVI.LatestReleaseURI == null) return default;

            // e.g. 0.7.0.1 from "C:/fol/DS2S_META_v0.7.0.1.zip"
            Regex re = new(@"DS2S.?META.?v?(?<ver>\d.*)(.zip|.7z)", RegexOptions.IgnoreCase); 

            var M = re.Match(MVI.LatestReleaseURI.ToString());
            if (M.Success)
                return Version.Parse(M.Groups["ver"].ToString());
            
            MessageBox.Show("Error parsing update file name");
            return default;
        }

        private void VersionUpdate()
        {
            if (Settings.IsUpgrading)
            {
                Updater.LoadSettingsAfterUpgrade();
                ShowCbxUpdate();
            }

            if (MVI.UpdateStatus != UPDATE_STATUS.OUTOFDATE)
                return;
                

            // Only show msg again when newer version released
            if (!MVI.IsAcknowledged)
                ShowMetaUpdateWindow();
        }

        

        

        private void ShowOnlineWarning()
        {
            if (!Settings.ShowWarning) return;
            var warning = new METAWarning("Online Warning", 350, 240);
            warning.ShowDialog();
        }
        public void ShowMetaUpdateWindow()
        {
            var warning = new METAUpdate(MVI)
            {
                Title = "New Update Available",
                Width = 450,
                Height = 280
            };
            warning.ShowDialog();
        }

        private bool ShowCbxUpdateBool { get; set; } = false;
        public int Row3MaxH => ShowCbxUpdateBool ? 100 : 1;
        public Visibility Row3Visibility => ShowCbxUpdateBool ? Visibility.Visible : Visibility.Hidden;
        
        public void ShowCbxUpdate()
        {
            ShowCbxUpdateBool = true;
            OnPropertyChanged(nameof(Row3MaxH));
            OnPropertyChanged(nameof(Row3Visibility));
        }
        public void HideCbxUpdate()
        {
            ShowCbxUpdateBool = false;
            OnPropertyChanged(nameof(Row3MaxH));
            OnPropertyChanged(nameof(Row3Visibility));
        }

    }
}
