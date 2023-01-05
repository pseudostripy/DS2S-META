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
        private readonly Window Window;
        private readonly HotkeyManager HKM;
        private Action<HotKey> HotkeyAction;
        private Brush DefaultColor;

        public Key Key;
        public HotKey? HotKey;

        public METAHotkey(HotkeyBoxControl hkbc, Action<HotKey> setAction, Window window, HotkeyManager hkm)
        {
            if (hkbc.SettingsName == null)
                throw new Exception("No settings name supplied for hotkey box control");
            
            SettingsName = hkbc.SettingsName;
            HotkeyTextBox = hkbc.tbxHotkey;
            DefaultColor = HotkeyTextBox.Background;
            HotkeyAction = setAction;
            Window = window;
            HKM = hkm;
            
            Key = KeyInterop.KeyFromVirtualKey((int)Properties.Settings.Default[SettingsName]);

            if (Key == Key.Escape)
                HotkeyTextBox.Text = "Unbound";
            else
                HotkeyTextBox.Text = Key.ToString();

            HotkeyTextBox.MouseEnter += HotkeyTextBox_MouseEnter;
            HotkeyTextBox.MouseLeave += HotkeyTextBox_MouseLeave;
            HotkeyTextBox.KeyUp += HotkeyTextBox_KeyUp;
        }

        // old
        //public METAHotkey(string settingsName, TextBox setTextBox, TabItem setTabPage, Action<HotKey> setAction, Window window)
        //{
        //    SettingsName = settingsName;
        //    HotkeyTextBox = setTextBox;
        //    DefaultColor = HotkeyTextBox.Background;
        //    //HotkeyTabPage = setTabPage;
        //    HotkeyAction = setAction;
        //    Window = window;

        //    Key = KeyInterop.KeyFromVirtualKey((int)Properties.Settings.Default[SettingsName]);

        //    if (Key == Key.Escape)
        //        HotkeyTextBox.Text = "Unbound";
        //    else
        //        HotkeyTextBox.Text = Key.ToString();

        //    HotkeyTextBox.MouseEnter += HotkeyTextBox_MouseEnter;
        //    HotkeyTextBox.MouseLeave += HotkeyTextBox_MouseLeave;
        //    HotkeyTextBox.KeyUp += HotkeyTextBox_KeyUp;
        //}


        public void RegisterHotkey()
        {
            UnregisterHotkey();

            if (Key != Key.Escape)
                HotKey = new HotKey(ModifierKeys.None, Key, Window, HotkeyAction);
        }

        public void UnregisterHotkey()
        {
            if (HotKey == null)
                return;

            HotKey.Dispose();
            HotKey = null;
        }

        private void HotkeyTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Key = e.Key;
            if (Key == Key.Escape)
                HotkeyTextBox.Text = "Unbound";
            else
                HotkeyTextBox.Text = Key.ToString();
            e.Handled = true;

            UnregisterHotkey();

            // Clear existing key by sending it Esc
            if (HKM.IsKeyUsed(this, Key, out var existingKey))
            {
                KeyEventArgs args = new(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.Escape);
                args.RoutedEvent = e.RoutedEvent;
                existingKey?.HotkeyTextBox_KeyUp(existingKey.HotkeyTextBox, args);
            }

            RegisterHotkey();
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
