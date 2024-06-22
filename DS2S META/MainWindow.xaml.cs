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
using DS2S_META.ViewModels;
using System.Linq;

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
        Timer UpdateTimer = new();
        private int ElapsedCtr = 0;

        //public DmgCalcViewModel DmgCalcViewModel { get; set; }

        public MainWindow()
        {
            PortableSettingsProvider.SettingsFileName = "DS2S Meta.config";
            PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);
            Settings = Properties.Settings.Default;
            HKM = new(this);
            InitializeComponent();

            //LoadSettingsAfterUpgrade();
            //ShowOnlineWarning();
            //Hook.OnHooked += Hook_OnHooked;
            Hook.MW = this;



        // This is duplicated in the ViewModel until DS2ViewModel is fixed accordingly
        //DmgCalcViewModel = new DmgCalcViewModel();
        //ViewModels.Add(DmgCalcViewModel);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //EnableTabsOnHooked(false);
            InitAllTabs();

            UpdateTimer.Interval = 16;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            UpdateTimer.Elapsed += UpdateTimeElapsed_4HzUpdates;
            UpdateTimer.Enabled = true;
        }

        //ObservableCollection<ViewModelBase> ViewModels = new();
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HandleRivaAndSpeedhackUnhooking();

            //Dispatcher.Invoke(new Action(() => { RivaHook.RefreshEnd(); }));
            if (Hook.EnableSpeedFactors)//gz
                Hook.EnableSpeedFactors = false;

            
            ViewModel.CleanupAll();

            UpdateTimer.Stop();
            SaveAllTabs();

            if (RandomizerControl.IsRandomized)
            {
                // Just fix the problem /shrug
                metaRando.RM.Unrandomize();

                if (Properties.Settings.Default.ShowWarnRandoExit)
                {
                    //var randoexit = new RandoExitWarning()
                    //{
                    //    Title = "Game Randomized Warning",
                    //    Width = 375,
                    //    Height = 195,
                    //};
                    //randoexit.ShowDialog();
                }

            }

            Hook.Cleanup();
            HKM.ClearHooks();
            Settings.Save();
        }
        private void HandleRivaAndSpeedhackUnhooking()
        {
            // Vanilla:
            if (!Hook.Is64Bit)
            {
                RivaHook.OnUnhooked();
                Hook.ClearSpeedhackInject();
                return;
            }

            // More complicated for SOTFS:
            if (!Hook.SpeedhackEverEnabled)
            {
                RivaHook.OnUnhooked();
                return;
            }

            if (!Properties.Settings.Default.RestartRivaOnClose)
            {
                RivaUnhookSlow();
                return;
            }

            // Try reopen RIVA programatically
            string rivaExePath = Properties.Settings.Default.RivaExePath;
            bool canFindRiva = File.Exists(rivaExePath);

            
            List<string> rtssProcNames = new() { "RTSS", "RTSSHooksLoader64" };
            var RTSSprocs = Process.GetProcesses().Where(proc => rtssProcNames.Contains(proc.ProcessName)).ToList();
                
            if (RTSSprocs.Count == 0)
            {
                // RTSS not open (nothing to do)
                Hook.ClearSpeedhackInject();
                return;
            }
            if (!canFindRiva)
            {
                // Riva not found
                string msg = @$"Cannot find RIVA at expected location:
{rivaExePath}
If you want to enable this feature, please paste the full RIVA.exe path
in to the Settings tab textbox next time you open Meta.
                                
Reverting to unhooking RIVA the slow way";
                MetaInfoWindow.ShowMetaInfo(msg);
                RivaUnhookSlow();
                return;
            }

            // Kill RTSS and request to reopen it
            Hook.ClearSpeedhackInject();
            foreach (var proc in RTSSprocs)
                proc.Kill();
            ExecuteAsAdmin(rivaExePath);
        }
        private void RivaUnhookSlow()
        {
            // Unload and wait for RIVA to refresh itself ~2mins
            Dispatcher.Invoke(new Action(() => { RivaHook.RefreshEnd(); }));
            Dispatcher.Invoke(new Action(() =>
            {
                Hook.ClearSpeedhackInject();
                RivaHook.OnUnhooked();
            }));
        }
        private void UpdateTimeElapsed_4HzUpdates(object? sender, EventArgs e)
        {
            // Fix race condition if this was queued just before closing
            if (!UpdateTimer.Enabled)
                return;

            // Increment counter
            ElapsedCtr++;
            if (ElapsedCtr % 16 != 0) return; // 16*16ms ~ 4Hz
            Dispatcher.Invoke(new Action(() => { Update4Hz(); }));
        }
        public void ExecuteAsAdmin(string fileName)
        {
            Process proc = new();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action( () =>
            {
                // Fix race condition if this was queued just before closing
                if (!UpdateTimer.Enabled)
                    return;

                UpdateMainProperties();
                if (!Hook.Hooked || !ParamMan.IsLoaded)
                    return;

                // Hook will be initialized by now
                UpdateProperties();     
            }));
            
        }
        private void UpdateMainProperties()
        {
            Hook.UpdateMainProperties();
            ViewModel.UpdateMainProperties();
            HKM.CheckFocusEvent(Hook.Focused);
        }
        private void Update4Hz()
        {
            if (!Hook.Hooked || !ParamMan.IsLoaded) 
                return;
            RivaHook.Refresh();
            ViewModel.DoSlowUpdates();
        }

        private void InitAllTabs()
        {
            metaItems.InitTab();
            metatabDmgCalc.InitTab();
            metaPlayer.InitTab();
            metaSettings.InitTab(HKM);
            metaRando.InitTab();
            metaStats.InitTab();

            // todo for each
            ViewModel.InitViewModels();
            //ViewModel.DmgCalcViewModel.InitViewModel(Hook);
            //ViewModel.CheatsViewModel.InitViewModel(Hook);
            //ViewModel.RandoSettingsViewModel.InitViewModel(Hook);
            //ViewModel.PlayerViewModel.InitViewModel(Hook);
        }
        private void UpdateProperties()
        {
            Hook.UpdateGameState();
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
        
        //private void ReloadAllTabs()
        //{
        //    metaPlayer.ReloadCtrl();
        //    metaStats.ReloadCtrl();
        //    metaItems.ReloadCtrl();
        //    metatabDmgCalc.ReloadCtrl();
        //}
        //private void UpdateAllTabs()
        //{
        //    metaPlayer.UpdateCtrl();
        //    metaStats.UpdateCtrl();
        //    metaItems.UpdateCtrl();
        //}

        
        private void SaveAllTabs()
        {
            //HKM.SaveHotkeys();
            //HKM.UnregisterHotkeys();
        }
        private void EnableStatEditing_Checked(object sender, RoutedEventArgs e)
        {
            metaStats.EnableCtrls(Hook.InGame);
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void cbxUpdateOK_Checked(object sender, RoutedEventArgs e)
        {
            cbxUpdateOK.IsChecked = false;
            ViewModel.HideCbxUpdate();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void MainWindowClose_Click(object sender, RoutedEventArgs e)
        {
            UpdateTimer.Stop();
            Close();
        }

        private void Link_NewUpdate_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowMetaUpdateWindow();
        }
    }
}
