using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DS2S_META
{
    public partial class MainWindow : Window
    {
        public List<METAHotkey> Hotkeys = new List<METAHotkey>();

        private void InitHotkeys()
        {
            cbxEnableHotkeys.IsChecked = Settings.EnableHotkeys;
            cbxHandleHotkeys.IsChecked = Settings.HandleHotkeys;

            Hotkeys.Add(new METAHotkey("StorePosition", hkeyStorePosition.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                metaPlayer.StorePosition();
            }, this));

            Hotkeys.Add(new METAHotkey("RestorePosition", hkeyRestorePosition.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                metaPlayer.RestorePosition();
            }, this));

            Hotkeys.Add(new METAHotkey("ToggleGravity", hkeyGravity.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                metaPlayer.cbxGravity.IsChecked = !metaPlayer.cbxGravity.IsChecked == true;
            }, this));

            Hotkeys.Add(new METAHotkey("ToggleCollision", hkeyCollision.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                metaPlayer.cbxCollision.IsChecked = !metaPlayer.cbxCollision.IsChecked == true;
            }, this));

            Hotkeys.Add(new METAHotkey("Up", hkeyUp.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                Hook.StableZ += 5;
            }, this));

            Hotkeys.Add(new METAHotkey("Down", hkeyDown.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                Hook.StableZ -= 5;
            }, this));

            Hotkeys.Add(new METAHotkey("ModifySpeed", hkeySpeed.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                if (metaPlayer.cbxSpeed.IsEnabled)
                    metaPlayer.cbxSpeed.IsChecked = !metaPlayer.cbxSpeed.IsChecked == true;
            }, this));

            Hotkeys.Add(new METAHotkey("ToggleSpeedFactors", hkeySpeedFactor.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                metaInternal.cbxSpeeds.IsChecked = !metaInternal.cbxSpeeds.IsChecked == true;
            }, this));

            Hotkeys.Add(new METAHotkey("Warp", hkeyWarp.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                if (!Hook.Multiplayer)
                    metaPlayer.Warp();
            }, this));

            Hotkeys.Add(new METAHotkey("CreateItem", hkeyCreateItem.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                metaItems.CreateItem();
            }, this));

            Hotkeys.Add(new METAHotkey("FastQuit", hkeyFastQuit.tbxHotkey, tabHotkeys, (hotkey) =>
            {
                Hook.FastQuit = 6;
            }, this));
        }

        private void SaveHotkeys()
        {
            Settings.EnableHotkeys = cbxEnableHotkeys.IsChecked == true;
            Settings.HandleHotkeys = cbxHandleHotkeys.IsChecked == true;
            foreach (METAHotkey hotkey in Hotkeys)
                hotkey.Save();
        }

        private bool HotkeysSet = false;
        private void CheckFocused()
        {
            if (Hook.Focused && !HotkeysSet)
                RegisterHotkeys();

            if (!Hook.Focused && HotkeysSet)
                UnregisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            foreach (var hotkey in Hotkeys)
            {
                hotkey.RegisterHotkey();
            }
            HotkeysSet = true;
        }

        private void UnregisterHotkeys()
        {
            foreach (var hotkey in Hotkeys)
            {
                var key = hotkey.Key;
                hotkey.UnregisterHotkey();
            }
            HotkeysSet = false;
        }

    }
}
