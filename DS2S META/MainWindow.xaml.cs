﻿using Bluegrams.Application;
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
using DS2S_META.Utils.DS2Hook;

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
        readonly Timer UpdateTimer = new();
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
            ViewModel.CleanupAll();
            UpdateTimer.Stop();

            if (RandomizerControl.IsRandomized)
                metaRando.RM.Unrandomize();

            Hook.SetupCleanupMan.Cleanup();
            HKM.ClearHooks();
            Settings.Save();
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
        private void Update4Hz()
        {
            if (!Hook.Hooked || !ParamMan.IsLoaded) 
                return;
            RivaHook.Refresh();
            ViewModel.DoSlowUpdates();
        }

        private void UpdateMainProperties()
        {
            Hook.UpdateMainProperties();
            ViewModel.UpdateMainProperties();
            HKM.CheckFocusEvent(Hook.Focused);
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
