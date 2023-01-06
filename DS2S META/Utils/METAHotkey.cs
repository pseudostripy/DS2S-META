using mrousavy;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DS2S_META
{
    public class METAHotkey
    {
        public string SettingsName;
        private TextBox HotkeyTextBox;
        private readonly HotkeyManager HKM;
        public Action HotkeyAction;
        private Brush DefaultColor;

        public Key Key { get; set; }
        public HotKey? HotKey;

        public METAHotkey(HotkeyBoxControl hkbc, Action setAction, HotkeyManager hkm)
        {
            if (hkbc.SettingsName == null)
                throw new Exception("No settings name supplied for hotkey box control");
            
            SettingsName = hkbc.SettingsName;
            HotkeyTextBox = hkbc.tbxHotkey;
            DefaultColor = HotkeyTextBox.Background;
            HotkeyAction = setAction;
            HKM = hkm;
            
            Key = KeyInterop.KeyFromVirtualKey((int)Properties.Settings.Default[SettingsName]);

            UpdateText();

            HotkeyTextBox.MouseEnter += HotkeyTextBox_MouseEnter;
            HotkeyTextBox.MouseLeave += HotkeyTextBox_MouseLeave;
            HotkeyTextBox.KeyUp += HotkeyTextBox_KeyUp;
            HKM.RefreshKeyList();
        }


        //public void RegisterHotkey()
        //{
        //    UnregisterHotkey();

        //    if (Key != Key.Escape)
        //        HotKey = new HotKey(ModifierKeys.None, Key, Window, HotkeyAction);
        //}

        //public void UnregisterHotkey()
        //{
        //    if (HotKey == null)
        //        return;

        //    HotKey.Dispose();
        //    HotKey = null;
        //}

        private void HotkeyTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            HKM.ClearMatchingKeyBinds(e.Key);   // Clear things it's overwriting
            Key = e.Key;                        // Set new keybind
            e.Handled = true;                   // Don't send on to other apps in this case
            UpdateText();
            HKM.RefreshKeyList();
        }

        public void UpdateText()
        {
            HotkeyTextBox.Text = Key == Key.Escape ? "Unbound" : $"{Key}";  // Update textbox with new key description
        }
        private void HotkeyTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            HotkeyTextBox.Background = DefaultColor;
        }
        private void HotkeyTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            HotkeyTextBox.Background = Brushes.LightGreen;
        }
        public void Save()
        {
            Properties.Settings.Default[SettingsName] = KeyInterop.VirtualKeyFromKey(Key);
        }
    }
}
