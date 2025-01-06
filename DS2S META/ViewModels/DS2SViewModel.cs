using DS2S_META.Properties;
using DS2S_META.Utils;
using DS2S_META.Utils.DS2Hook;
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
    public class DS2SViewModel : ObservableObject
    {
        // Binding Variables (in order of appearance):
        public string MetaStatus
        {
            get
            {
                return MVI.UpdateStatus switch
                {
                    UPDATE_STATUS.UPTODATE => "Meta up to date",
                    UPDATE_STATUS.OUTOFDATE => "New Meta update available!",
                    UPDATE_STATUS.INDEVELOPMENT => "Meta in-development version",
                    UPDATE_STATUS.UNCHECKABLE => "Unsure if up-to-date",
                    UPDATE_STATUS.UNKNOWN_VER => "Unknown Meta version",
                    _ => throw new Exception("Impossible case")
                };
            }
        }
        public Visibility VisMetaStatus => MVI.UpdateStatus != UPDATE_STATUS.OUTOFDATE ? Visibility.Visible : Visibility.Hidden;
        public Visibility VisNewVersionHyperlink => MVI.UpdateStatus == UPDATE_STATUS.OUTOFDATE ? Visibility.Visible : Visibility.Hidden;

        public string LblContentInGame => InGame.ToString();
        public Brush FGColInGame => InGame ? Brushes.GreenYellow : Brushes.IndianRed;

        public string LblContentOnline => Hook.DS2P?.CGS.Online.ToString() ?? "Unhooked";
        public Visibility VisOnline => InGame ? Visibility.Visible : Visibility.Hidden;
        public Brush FGColOnline
        {
            get
            {
                if (!Hook.Hooked)
                    return Brushes.Black;

                if (Hook.DS2P?.CGS.Online == true)
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }

        public string LblContentProcessID => Hook.Process?.Id.ToString() ?? "Not Hooked";
        public Brush FGColProcessID => LblContentProcessID != "Not Hooked" ? Brushes.GreenYellow : Brushes.IndianRed;

        public string DS2VerInfoString => Hook?.VerMan.VerInfoString ?? "Not hooked";
        public Brush FGColDS2Version
        {
            get
            {
                if (!Hook.Hooked)
                    return Brushes.Black;

                if (Hook?.VerMan.IsValidVer == true)
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }
        

        // ViewModel helpers
        ObservableCollection<ViewModelBase> ViewModels = new();
        public DmgCalcViewModel DmgCalcViewModel { get; set; }
        public RandoSettingsViewModel RandoSettingsViewModel { get; set; }
        public CheatsViewModel CheatsViewModel { get; set; }
        public PlayerViewModel PlayerViewModel { get; set; }
        public StatsViewModel StatsViewModel { get; set; }
        public SettingsViewModel SettingsViewModel { get; set; }
        public InternalViewModel InternalViewModel { get; set; }

        // Wrapper exposures:
        private Settings Settings = Settings.Default;
        public DS2SHook Hook { get; private set; }
        
        public bool InGame => Hook.InGame;
        public bool DS2Loading => Hook.DS2P.CGS.IsLoading;
        
        public static MetaVersionInfo MVI = new();

        private static string DebugStr
        {
            get
            {
                var dbgstring = string.Empty;
#if DEBUG
                dbgstring = "DEBUG";
#endif
                return dbgstring;
            }
        }
        public static string WindowName => $"META {MVI.MetaVersionStr} {DebugStr}";
 

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
            Hook.OnHooked += RivaOnHookedEventHandler;
            Hook.OnUnhooked += AllTabsOnUnHooked;
            Hook.OnUnhooked += RivaOnUnhookedEventHandler;
            Hook.OnGameStateHandler += OnGameStateChange;

            // Setup ViewModels
            PlayerViewModel = new PlayerViewModel();
            DmgCalcViewModel = new DmgCalcViewModel();
            CheatsViewModel = new CheatsViewModel();
            RandoSettingsViewModel = new RandoSettingsViewModel();
            StatsViewModel = new StatsViewModel();
            SettingsViewModel = new SettingsViewModel(this);
            InternalViewModel = new InternalViewModel();
            ViewModels.Add(DmgCalcViewModel);
            ViewModels.Add(CheatsViewModel);
            ViewModels.Add(PlayerViewModel);
            ViewModels.Add(RandoSettingsViewModel);
            ViewModels.Add(StatsViewModel);
            ViewModels.Add(SettingsViewModel);
            ViewModels.Add(InternalViewModel);


            Hook.Start();
            ShowOnlineWarning();
            Versioning();
        }

        private void AllTabsOnHooked(object? sender, PHEventArgs e)
        {
            MetaFeature.Initialize(Hook);
            foreach (var vm in ViewModels)
                vm.OnHooked();
        }
        private void AllTabsOnUnHooked(object? sender, PHEventArgs e)
        {
            foreach (var vm in ViewModels) 
                vm.OnUnHooked();
        }
        private void RivaOnHookedEventHandler(object? sender, PHEventArgs e)
        {
            RivaHook.OnHooked();
        }
        private void RivaOnUnhookedEventHandler(object? sender, PHEventArgs e)
        {
            RivaHook.OnUnhooked();
        }
        public void CleanupAll()
        {
            foreach (var vm in ViewModels)
                vm.CleanupVM();
        }


        

        public void UpdateMainProperties()
        {
            OnPropertyChanged(nameof(InGame));

            OnPropertyChanged(nameof(LblContentInGame));
            OnPropertyChanged(nameof(FGColInGame));
            OnPropertyChanged(nameof(LblContentProcessID));
            OnPropertyChanged(nameof(FGColProcessID));
            OnPropertyChanged(nameof(LblContentOnline));
            OnPropertyChanged(nameof(FGColOnline));
            OnPropertyChanged(nameof(DS2VerInfoString));
            OnPropertyChanged(nameof(FGColDS2Version));
            OnPropertyChanged(nameof(VisOnline));
            OnPropertyChanged(nameof(DS2Loading)); // not used yet


            foreach (var vm in ViewModels)
                vm.UpdateViewModel();
            //DmgCalcViewModel.UpdateViewModel();
            //CheatsViewModel.UpdateViewModel();
            //PlayerViewModel.UpdateViewModel();

        }
        public void DoSlowUpdates()
        {
            foreach (var vm in ViewModels)
                vm.DoSlowUpdates();
        }



        public void InitViewModels()
        {
            foreach (var vm in ViewModels)
                vm.InitViewModel(Hook);
        }

        private void OnGameStateChange(object? sender, GameStateEventArgs e)
        {
            // Update some things immediately:
            OnPropertyChanged(nameof(InGame));
            OnPropertyChanged(nameof(LblContentInGame));
            OnPropertyChanged(nameof(FGColInGame));
            OnPropertyChanged(nameof(VisOnline));


            // Update tab properties
            if (e.GameState == (int)GAMESTATE.MAINMENU)
                AllTabsOnMainMenu();
            if (e.GameState == (int)GAMESTATE.LOADEDINGAME) // add more here
                AllTabsOnInGame();
        }
        
        private void AllTabsOnInGame()
        {
            PlayerViewModel.OnInGame();
            CheatsViewModel?.OnInGame();
            StatsViewModel?.OnInGame();
            InternalViewModel?.OnInGame();
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
            OnPropertyChanged(nameof(MetaStatus));
            OnPropertyChanged(nameof(VisMetaStatus));
            OnPropertyChanged(nameof(VisNewVersionHyperlink));
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
            OnPropertyChanged(nameof(MetaStatus)); // required because async

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
