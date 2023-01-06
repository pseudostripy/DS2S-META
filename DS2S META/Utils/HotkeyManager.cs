using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using DS2S_META.Utils;
using System.CodeDom;
using System.Threading;
using System.Transactions;
using System.Windows.Controls;
using Octokit;
using System.Security.Cryptography;
using mrousavy;
using System.Windows.Input;
using PropertyHook;
using System.Diagnostics;
using DesktopWPFAppLowLevelKeyboardHook;

namespace DS2S_META
{
    /// <summary>
    /// Handle logic & data related to Hotkeys
    /// </summary>
    public class HotkeyManager
    {
        internal MainWindow MW;
        internal List<METAHotkey> Hotkeys = new();
        internal List<METAHotkey> UsedHotkeys = new();
        internal List<METAHotkey> ActiveHotkeys = new(); // excludes those bound to Esc
        internal List<Key> KeysInUse = new();
        internal Dictionary<Key, Action> KeyActions = new();
        public bool HotKeysRegistered;
        public const Key CLEARKEY = Key.Escape;

        internal HotkeyManager(MainWindow mw)
        {
            MW = mw;

            // Declare the event listener
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += Listener_OnKeyPressed;
        }

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
                "Give 17k" => () => MW.metaCheats.Give17kReward(),
                _ => throw new NotImplementedException("Unknown hotkey request method")
            };
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
        }
        public void HookKeyboard()
        {
            _listener.HookKeyboard();
        }
        public void UnhookKeyboard()
        {
            _listener.UnHookKeyboard();
        }

        // Only keyboard hook when DS2 is actively focussed
        public bool KbHookRegistered => _listener.IsHooked;
        public void CheckFocusEvent(bool ds2focussed)
        {
            // Previously called CheckFocussed
            if (ds2focussed && !KbHookRegistered)
                HookKeyboard();

            if (!ds2focussed && KbHookRegistered)
                UnhookKeyboard();
        }
        internal void LinkHotkeyControl(HotkeyBoxControl hkbc)
        {
            var mhk = new METAHotkey(hkbc, GetHotkeyMethod(hkbc.HotkeyName), this);
            UsedHotkeys.Add(mhk);
        }
        internal void ClearMatchingKeyBinds(Key keytomatch)
        {
            var existingkeys = UsedHotkeys.Where(hk => hk.Key == keytomatch).ToList();
            if (existingkeys.Count == 0) return;
            existingkeys.ForEach(hk => hk.Key = CLEARKEY);
            existingkeys.ForEach(hk => hk.UpdateText());
        }
        internal void RefreshKeyList()
        {
            // Called after messing around with registering/unregistering hotkeys

            // latency improvement
            ActiveHotkeys = UsedHotkeys.Where(_hk => _hk.Key != CLEARKEY).ToList();
            KeysInUse = ActiveHotkeys.Select(mhk => mhk.Key).ToList();

            // meta macros on hotkey
            KeyActions = ActiveHotkeys.ToDictionary(mhk => mhk.Key, mhk => mhk.HotkeyAction);     
        }
        public void SaveHotkeys()
        {
            foreach (METAHotkey hotkey in UsedHotkeys)
                hotkey.Save();
        }
    }
}
