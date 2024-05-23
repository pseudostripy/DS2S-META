using DS2S_META.Utils.DS2Hook;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    /// <summary>
    /// "HookGroupObject" handles updating a group's worth of properties
    /// </summary>
    public abstract class HGO : INotifyPropertyChanged
    {
        public abstract void UpdateProperties();
        protected Dictionary<string, string> DataNameTranslationDict;
        protected virtual Dictionary<string, string> DefineTranslationDictionary() => new();
        protected string TranslateRENameToPHLeafPropName(string reDataName)
        {
            DataNameTranslationDict.TryGetValue(reDataName, out var propname);
            return propname ?? $"PH{reDataName}";
        }
        protected DS2SHook Hook {  get; set; } // probably useful to keep a copy of this

        public HGO(DS2SHook hook)
        {
            Hook = hook;
            DataNameTranslationDict = DefineTranslationDictionary();
        }

        protected void SetProperty(string propName, object? value)
        {
            var pinfo = GetType().GetProperty(propName) ?? throw new Exception($"Property {propName} cannot be found in class {GetType()}");
            pinfo.SetValue(this, value);
        }

        // OnPropertyChanged stuff
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }

        // Utility
        public static PHPointer? ValOrNull(Dictionary<string, PHPointer> PHPDict, string key)
        {
            PHPDict.TryGetValue(key, out var value);
            return value;
        }
    }
}
