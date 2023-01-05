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

namespace DS2S_META
{
    /// <summary>
    /// Handle logic & data related to Hotkeys
    /// </summary>
    public class HotkeyManager
    {
        internal MainWindow MW;
        internal List<METAHotkey> Hotkeys = new();
        public bool HotKeysRegistered;

        internal HotkeyManager(MainWindow mw)
        {
            MW = mw;
        }

        internal Action<HotKey> GetHotkeyMethod(string hkname)
        {
            return hkname switch
            {
                "Store Position" => (hotkey) => MW.metaPlayer.StorePosition(),
                "Restore Position" => (hotkey) => MW.metaPlayer.RestorePosition(),
                "Toggle Gravity" => (hotkey) => MW.metaPlayer.ToggleGravity(),
                "Toggle Collision" => (hotkey) => MW.metaPlayer.ToggleCollision(),
                "Move Up" => (hotkey) => MW.metaPlayer.DeltaHeight(+5),
                "Move Down" => (hotkey) => MW.metaPlayer.DeltaHeight(-5),
                "Toggle Speedup" => (hotkey) => MW.metaPlayer.ToggleSpeed(),
                "Warp" => (hotkey) => MW.metaPlayer.Warp(),
                "Create Item" => (hotkey) => MW.metaItems.CreateItem(),
                "Fast Quit" => (hotkey) => MW.metaPlayer.FastQuit(),
                //{ "17k", (hotkey) => continue },
                _ => throw new NotImplementedException("Unknown hotkey request method")
            };
        }

        // Links the definitions in UI to their functions
        internal void LinkHotkeyControl(HotkeyBoxControl hkbc)
        {
            var mhk = new METAHotkey(hkbc, GetHotkeyMethod(hkbc.HotkeyName), MW, this);
            Hotkeys.Add(mhk);
        }

        // Utility:
        internal bool IsKeyUsed(METAHotkey mhk, Key currkey, out METAHotkey? existingkey)
        {
            existingkey = Hotkeys.Find(hk => hk.Key == currkey
                                             && hk.SettingsName != mhk.SettingsName
                                             && hk.Key != Key.Escape);
            return existingkey != null;
        }
        public void RegisterHotkeys()
        {
            foreach (var hotkey in Hotkeys)
                hotkey.RegisterHotkey();
            HotKeysRegistered = true;
        }
        public void UnregisterHotkeys()
        {
            foreach (var hotkey in Hotkeys)
                hotkey.UnregisterHotkey();
            HotKeysRegistered = false;
        }
        public void SaveHotkeys()
        {
            foreach (METAHotkey hotkey in Hotkeys)
                hotkey.Save();
        }
        
    }
}
