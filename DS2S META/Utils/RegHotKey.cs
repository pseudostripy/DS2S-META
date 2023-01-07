using mrousavy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DS2S_META.Utils
{
    public class RegHotKey
    {
        public string Name;
        public string SettingsName;
        public Key K;
        public HotKey? HK;
        public Action<HotKey> Act;
        private readonly HotkeyManager HKM;

        public RegHotKey(string name, string? settingsname, Key k, Action<HotKey> ahk, HotkeyManager hkm)
        {
            Name = name;

            if (settingsname == null)
                throw new Exception("Settings name is required");
            SettingsName = settingsname;
            K = k;
            HKM = hkm;
            Act = ahk;
        }

        public void RegisterNewKey(Key k)
        {
            K = k;
            RegisterHotkey();
        }

        public void RegisterHotkey()
        {
            UnregisterHotkey();

            if (K != HotkeyManager.CLEARKEY)
                HK = new HotKey(ModifierKeys.None, K, HKM.MW, Act);
        }
        public void UnregisterHotkey()
        {
            if (HK == null)
                return;

            HK.Dispose();
            HK = null;
        }
    }
}
