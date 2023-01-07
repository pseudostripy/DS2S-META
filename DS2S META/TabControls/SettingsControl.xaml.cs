using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using DS2S_META.Randomizer;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using mrousavy;

namespace DS2S_META
{
    /// <summary>
    /// Code for Cheats Front End
    /// </summary>
    public partial class SettingsControl : METAControl
    {
        private Properties.Settings? Settings; // application level settings
        internal HotkeyManager? HKM;

        // FrontEnd:
        public SettingsControl()
        {
            InitializeComponent();
        }

        internal void InitTab(HotkeyManager hkm)
        {
            HKM = hkm;
            Settings = Properties.Settings.Default;
            cbxEnableHotkeys.IsChecked = Settings.EnableHotkeys;
            cbxFullscreenHotkeys.IsChecked = Settings.HandleHotkeys;

            // Init Hotkeys:
            HKM.LinkHotkeyControl(hkeyStorePosition);
            HKM.LinkHotkeyControl(hkeyRestorePosition);
            HKM.LinkHotkeyControl(hkeyGravity);
            HKM.LinkHotkeyControl(hkeyCollision);
            HKM.LinkHotkeyControl(hkeyUp);
            HKM.LinkHotkeyControl(hkeyDown);
            HKM.LinkHotkeyControl(hkeySpeed);
            HKM.LinkHotkeyControl(hkeyWarp);
            HKM.LinkHotkeyControl(hkeyCreateItem);
            HKM.LinkHotkeyControl(hkeyFastQuit);
            HKM.LinkHotkeyControl(hkey17k);

            // Enable hotkeys
            HKM.RefreshKeyList();

            // Reg hook takes priority in mutex
            if (Settings.EnableHotkeys)
            {
                cbxFullscreenHotkeys.IsChecked = false;
                HKM.SetHotkeyRegHook();
                return;
            }

            if (cbxFullscreenHotkeys.IsChecked == true)
                HKM.SetHotkeyLLHook();

        }

        private void cbxEnableHotkeys_Checked(object sender, RoutedEventArgs e)
        {
            if (cbxFullscreenHotkeys == null)
                return;
            cbxFullscreenHotkeys.IsChecked = false;
            HKM?.RemoveHotkeyLLHook();
            HKM?.SetHotkeyRegHook();
        }
        private void cbxEnableHotkeys_Unchecked(object sender, RoutedEventArgs e)
        {
            HKM?.RemoveHotkeyRegHook();
            CheckFullDisable();
        }

        private void cbxFullscreenHotkeys_Checked(object sender, RoutedEventArgs e)
        {
            cbxEnableHotkeys.IsChecked = false;
            HKM?.RemoveHotkeyRegHook();
            HKM?.SetHotkeyLLHook();
        }
        private void cbxFullscreenHotkeys_Unchecked(object sender, RoutedEventArgs e)
        {
            HKM?.RemoveHotkeyLLHook();
            CheckFullDisable();
        }

        private void CheckFullDisable()
        {
            if (cbxEnableHotkeys.IsChecked == false && cbxFullscreenHotkeys.IsChecked == false)
                HKM?.ClearMode();
        }

        
    }
}