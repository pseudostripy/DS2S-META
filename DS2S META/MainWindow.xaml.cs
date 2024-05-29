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
            //RivaHook.OnUnhooked(); // deprecated
            if (Hook.EnableSpeedFactors)//gz
                Hook.EnableSpeedFactors = false;

            Hook.ClearSpeedhackInject();
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

            HKM.ClearHooks();
            Settings.Save();
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
                //EnableTabs(Hook.InGame);

                //if (Hook.InGame)
                //    UpdateAllTabs();                
            }));
            
        }
        private void UpdateMainProperties()
        {
            Hook.UpdateMainProperties();
            ViewModel.UpdateMainProperties();
            //HKM.UpdateHotkeyRegistration(Hook.Focused);
            HKM.CheckFocusEvent(Hook.Focused);
        }
        private void Update4Hz()
        {
            if (!Hook.Hooked || !ParamMan.IsLoaded) 
                return;
            //RivaHook.Refresh(); // deprecated. wait for whenever I do JD render insert
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
            Close();
        }

        private void link_NewUpdate_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowMetaUpdateWindow();
        }
    }
}
