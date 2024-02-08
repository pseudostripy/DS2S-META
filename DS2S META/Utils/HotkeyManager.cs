using System;
using System.Collections.Generic;
using System.Linq;
using mrousavy;
using System.Windows.Input;
using PropertyHook;
using System.Diagnostics;
using DesktopWPFAppLowLevelKeyboardHook;
using System.Configuration;
using System.Windows;
using DS2S_META.Utils;
using Octokit;
using System.Windows.Controls;

namespace DS2S_META
{
    /// <summary>
    /// Handle logic & data related to Hotkeys
    /// </summary>
    public class HotkeyManager
    {
        internal HKMODE Mode = HKMODE.DISABLED;
        internal MainWindow MW;
        internal List<Key> KeysInUse = new();
        internal Dictionary<Key, Action> KeyActions = new();

        internal List<LLHotKey> LLHotkeys = new();
        internal List<RegHotKey> RegHotKeys = new();
        internal Dictionary<TextBox, HotkeyBoxControl> Tb2Hkbc = new();

        public enum HKMODE
        {
            DISABLED,
            HKREG,
            HKLL, // Low-Level
        }
        public bool RegMode => Mode == HKMODE.HKREG;
        public bool LLMode => Mode == HKMODE.HKLL;

        public TabControl SettingsTab { get; set; }

        public bool HKReg_On = false;
        public bool HKLL_On = false;

        public const Key CLEARKEY = Key.Escape;

        internal HotkeyManager(MainWindow mw)
        {
            MW = mw;

            // Declare the event listener
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += Listener_OnKeyPressed;
        }

        // Assign actions to hotkeys:
        internal Action GetHotkeyMethod(string hkname)
        {
            return hkname switch
            {
                "Store Position" => () => MW.metaPlayer.StorePosition(),
                "Restore Position" => () => MW.metaPlayer.RestorePosition(),
                "Toggle Gravity" => () => MW.metaPlayer.ToggleGravity(),
                "Toggle Collision" => () => MW.metaPlayer.ToggleCollision(),
                "Move Up" => () => MW.metaPlayer.DeltaHeight(+5),
                "Move Down" => () => MW.metaPlayer.DeltaHeight(-5),
                "Toggle Speedup" => () => MW.metaPlayer.ToggleSpeed(),
                "Warp" => () => MW.metaPlayer.Warp(),
                "Create Item" => () => MW.metaItems.CreateItem(),
                "Fast Quit" => () => MW.metaPlayer.FastQuit(),
                "Give 17k" => () => MW.metaCheats.Hook.Give17kReward(),
                "Toggle AI" => () => MW.metaPlayer.ToggleAI(),
                "Toggle No Death" => () => MW.metaPlayer.ToggleNoDeath(),
                "Toggle OHKO" => () => MW.metaPlayer.ToggleRapierOhko(),
                "Give 3/1" => () => MW.metaCheats.Hook.Give3Chunk1Slab(),
                _ => throw new NotImplementedException("Unknown hotkey request method")
            };
        }
        
        private Action<HotKey> GetHotkeyAction1Method(string hkname)
        {
            var voidaction = GetHotkeyMethod(hkname);
            return (hotkey) => voidaction();
        }

        internal void LinkHotkeyControl(HotkeyBoxControl hkbc)
        {
            hkbc.tbxHotkey.KeyUp += HotkeyTextBox_KeyUp;

            // Repack data:
            int settingskeyint = (int)Properties.Settings.Default[hkbc.SettingsName];
            Key k = KeyInterop.KeyFromVirtualKey(settingskeyint);
            var ahk = GetHotkeyAction1Method(hkbc.HotkeyName);
            var RHK = new RegHotKey(hkbc.HotkeyName, hkbc.SettingsName, k, ahk, this);
            var LHK = new LLHotKey(hkbc.HotkeyName, hkbc.SettingsName, k, GetHotkeyMethod(hkbc.HotkeyName), this);
            
            // Add to lists of hotkeys
            RegHotKeys.Add(RHK);
            LLHotkeys.Add(LHK);
            Tb2Hkbc.Add(hkbc.tbxHotkey, hkbc);

            // Show text of current hotkey:
            UpdateText(hkbc, k);
        }

        private void HotkeyTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Update both hotkey sets on keyup event
            var tb = (TextBox)sender;
            
            // refresh?
            var temp = MW.tabControls.SelectedIndex;
            MW.tabControls.SelectedIndex = 0;
            MW.tabControls.SelectedIndex = temp;


            var hkbc = Tb2Hkbc[tb];
            if (hkbc.SettingsName == null)
                throw new Exception("Unable to get name of sender textbox");

            string settname = hkbc.SettingsName;
            UpdateText(hkbc, e.Key);
            e.Handled = true;

            // Clear duplicates
            RegHotKey regHotKey = RegHotKeys.Where(rhk => rhk.SettingsName == settname).First();
            LLHotKey llHotKey = LLHotkeys.Where(rhk => rhk.SettingsName == settname).First();
            ClearMatchingKeyBinds(regHotKey, llHotKey, e);
            
            // Update new keys
            regHotKey.RegisterNewKey(e.Key);
            llHotKey.K = e.Key;
            SaveKeySetting(settname, e.Key);
        }
        public static void UpdateText(HotkeyBoxControl hkbc, Key key)
        {
            hkbc.tbxHotkey.Text = key == Key.Escape ? "Unbound" : $"{key}";  // Update textbox with new key description
        }
        public static void SaveKeySetting(string settingsname, Key k)
        {
            Properties.Settings.Default[settingsname] = KeyInterop.VirtualKeyFromKey(k);
        }

        // Code for "normal" Register Hotkey implementation
        internal void ClearMatchingKeyBinds(RegHotKey rhk, LLHotKey lhk, KeyEventArgs e)
        {
            ClearMatchingKeyBindsReg(rhk, e);
            ClearMatchingKeyBindsLL(lhk, e);
        }
        internal void ClearMatchingKeyBindsReg(RegHotKey rhk, KeyEventArgs e)
        {
            if (e.Key == CLEARKEY)
                return;
            
            // Clear any other hotkeys with same key 
            var existingkeys = RegHotKeys.Where(hk => hk != rhk)
                                         .Where(hk => hk.K == e.Key).ToList();
            foreach (var existkey in existingkeys)
            {
                existkey.UnregisterHotkey();
                existkey.K = CLEARKEY; // this is so that on re-focus we don't run into a duplicate registration
                var existhkbc = GetHkbcFromSetting(existkey.SettingsName);
                UpdateText(existhkbc, CLEARKEY);
            }
                
        }
        internal void ClearMatchingKeyBindsLL(LLHotKey lhk, KeyEventArgs e)
        {
            if (e.Key == CLEARKEY)
                return;

            // Clear any other hotkeys with same key 
            var existingkeys = LLHotkeys.Where(hk => hk != lhk)
                                        .Where(hk => hk.K == e.Key).ToList();
            foreach (var existkey in existingkeys)
            {
                existkey.K = CLEARKEY;
                var existhkbc = GetHkbcFromSetting(existkey.SettingsName);
                UpdateText(existhkbc, CLEARKEY);
            }
                
        }
        private HotkeyBoxControl GetHkbcFromSetting(string settingName)
        {
            var hkbc_out = Tb2Hkbc.Values.Where(hkbc => hkbc.SettingsName== settingName).FirstOrDefault();
            if (hkbc_out == null)
                throw new Exception("Cannot find associated hkbc for duplicate hotkey reset");
            return hkbc_out;
        }


        // REG MODE interfaces
        public void SetHotkeyRegHook()
        {
            // Only set if not already hooked
            if (HKReg_On)
                return;

            RegisterHotkeys();
            HKReg_On = true;
            Mode = HKMODE.HKREG;
        }
        public void RemoveHotkeyRegHook()
        {
            // Only remove if actually hooked
            if (!HKReg_On)
                return;

            UnregisterHotkeys();
            HKReg_On = false;
        }
        private void RegisterHotkeys()
        {
            foreach (var rhk in RegHotKeys)
                rhk.RegisterHotkey();
            HKReg_On = true;
        }
        private void UnregisterHotkeys()
        {
            foreach (var rhk in RegHotKeys)
                rhk.UnregisterHotkey();
            HKReg_On = false;
        }

        


        // LowLevelHook_Code
        private LowLevelKeyboardListener _listener;
        void Listener_OnKeyPressed(object? sender, KeyPressedArgs e)
        {
            // Only hook the keys set explicitly in Meta
            if (!KeysInUse.Contains(e.KeyPressed))
                return;

            var action = KeyActions[e.KeyPressed];
            action(); // do it

            Debug.Print($"Mode: {Mode}");
            Debug.Print($"REG hook: {HKReg_On}");
            Debug.Print($"LL hook: {HKLL_On}");
        }

        // LL MODE interfaces
        public void SetHotkeyLLHook()
        {
            // Only applies if we're already set
            if (HKLL_On)
                return;

            HookKeyboard();
            RefreshKeyList();
            HKLL_On = true;
            Mode = HKMODE.HKLL;
        }
        public void RemoveHotkeyLLHook()
        {
            // Only remove if hooked
            if (!HKLL_On)
                return;

            UnhookKeyboard();
            RefreshKeyList();
            HKLL_On = false;
        }
        private void HookKeyboard()
        {
            _listener.HookKeyboard();
        }
        private void UnhookKeyboard()
        {
            _listener.UnHookKeyboard();
        }

        // Only keyboard hook when DS2 is actively focussed
        public bool KbHookRegistered => _listener.IsHooked;
        public void CheckFocusEvent(bool ds2focussed)
        {
            if (RegMode)
            {
                if (ds2focussed && !HKReg_On)
                    SetHotkeyRegHook();
                else if (!ds2focussed && HKReg_On)
                    RemoveHotkeyRegHook();
                return;
            }

            if (LLMode)
            {
                if (ds2focussed && !HKLL_On)
                    SetHotkeyLLHook();
                else if (!ds2focussed && KbHookRegistered)
                    RemoveHotkeyLLHook();
            }
        }

        
        internal void RefreshKeyList()
        {
            // Called after messing around with registering/unregistering hotkeys

            // latency improvement
            var activekeys = LLHotkeys.Where(lhk => lhk.K != CLEARKEY).ToList();
            KeysInUse = activekeys.Select(lhk => lhk.K).ToList();

            // meta macros on hotkey
            KeyActions = activekeys.ToDictionary(lhk => lhk.K, lhk => lhk.Act);
        }

        public void ClearHooks()
        {
            RemoveHotkeyRegHook();
            RemoveHotkeyLLHook();
        }
        public void ClearMode()
        {
            // Only when both checkboxes are disabled
            Mode = HKMODE.DISABLED;
        }
    }
}
