using mrousavy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DS2S_META.Utils
{
    public class LLHotKey
    {
        public string Name;
        public string SettingsName;
        public Key K;
        public Action Act;
        private readonly HotkeyManager HKM;

        public LLHotKey(string name, string? settingsname, Key k, Action ahk, HotkeyManager hkm)
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
        }
    }
}
