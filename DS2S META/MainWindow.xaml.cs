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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Hook.EnableSpeedFactors)
                Hook.EnableSpeedFactors = false;

            Hook.ClearSpeedhackInject();


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

        
        private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
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
            ViewModel.RandoSettingsViewModel.InitViewModel(Hook);
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
            metaInternal.EnableCtrls(enable);
            metaItems.EnableCtrls(enable);
            metatabDmgCalc.EnableCtrls(enable);
        }
        private void ReloadAllTabs()
        {
            metaPlayer.ReloadCtrl();
            metaStats.ReloadCtrl();
            metaItems.ReloadCtrl();
            metatabDmgCalc.ReloadCtrl();
        }
        private void UpdateAllTabs()
        {
            metaPlayer.UpdateCtrl();
            metaStats.UpdateCtrl();
            metaItems.UpdateCtrl();
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
