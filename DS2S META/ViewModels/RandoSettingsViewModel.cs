using DS2S_META.Properties;
using DS2S_META.Randomizer;
using DS2S_META.Utils;
using DS2S_META.ViewModels.Commands;
using Octokit;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace DS2S_META.ViewModels
{
    using IPRSList = ObservableCollection<ItemRestriction>;

    public class RandoSettingsViewModel : ViewModelBase
    {
        public static List<string> PresetsComboItems { get; set; } = new() { "Default", "Custom", "Pain" };
        public static readonly string SettingsFilePath = $"{GetTxtResourceClass.ExeDir}/Resources/RandomizerSettings.xml";
        public static Dictionary<MapArea, string> MapAreaComboItems { get; set; } = new();

        internal IPRSList DefaultRestrictions { get; set; } = DefineDefaultRestrictions();
        internal IPRSList PainRestrictions { get; set; } = DefinePainRestrictions();
        
        public event EventHandler UserPresetChange;
        private bool AutoChange = false;

        public void HandleUserPresetChange(object? sender, EventArgs e)
        {
            // This event will only load some Preset settings
            // if the user changed via the combobox. Changes via
            // direct setting editing will exit early.
            if (AutoChange)
            {
                AutoChange = false;
                return;
            }

            // Legit update:
            LoadPresetSettings(GetDefaultPreset(Preset));
        }

        // Constructor
        public RandoSettingsViewModel()
        {
            try
            {
                LoadRandomizerSettings();
            }
            catch (Exception)
            {
                // Use defaults when not available
                ItemRestrictions = DefineDefaultRestrictions();
                SaveRandomizerSettings();
            }
            SetupSaveCallbacks();
            UserPresetChange += HandleUserPresetChange; // subscribe event handler
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
        }

        private IPRSList _itemRestrictions = new();
        public IPRSList ItemRestrictions
        { 
            get => _itemRestrictions;
            set
            {
                _itemRestrictions = value;
                OnPropertyChanged();
            }
        }

        private string _preset = "Default";
        public string Preset
        {
            get => _preset;
            set
            {
                var origval = _preset;
                
                // no change
                if (value == _preset)
                    return;

                // update early for shenanigans:
                _preset = value; 

                // Doesn't ever cause issues
                if (value == "Custom")
                {
                    OnPropertyChanged();
                    return;
                }

                var newpreset = GetDefaultPreset(_preset);
                var currRestrSet = ItemRestrictions;
                
                // Check for cancellation (shenanigans):
                if (!AreFunctionallySame(newpreset, currRestrSet) && 
                    !CheckOverwriteSttgOK())
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            // Do this against the underlying value so 
                            //  that we don't invoke the cancellation question again.
                            _preset = origval;
                            OnPropertyChanged(nameof(Preset));
                        }),
                        DispatcherPriority.ContextIdle,
                        null
                        );
                    return;
                }
                OnPropertyChanged(nameof(Preset));
                FirePresetChangeEvent();
            }
        }

        // Events:
        private void FirePresetChangeEvent()
        {
            UserPresetChange?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
        private void RestrictionPropertiesChanged(object? sender, PropertyChangedEventArgs e)
        {
            SaveRandomizerSettings();
            var checkpreset = GetPresetType();

            // This update will always pass because we've changed ItemRestrictions
            // to directly match the checkpreset type (by definition). So the Preset
            // setter will notice this and not complain because we're not "updating"
            // anything new.
            AutoChange = true; // avoids reloading xml for no reason
            Preset = checkpreset;

            // in case of no event trigger (custom => custom does this)
            AutoChange = false; 
            OnPropertyChanged(nameof(ItemRestrictions));
        }
        private void SetupSaveCallbacks()
        {
            // Save settings after each change
            foreach (var restriction in ItemRestrictions)
                restriction.PropertyChanged += RestrictionPropertiesChanged;
        }

        private string GetPresetType()
        {
            if (AreFunctionallySame(ItemRestrictions, DefaultRestrictions))
                return "Default";
            
            if (AreFunctionallySame(ItemRestrictions, PainRestrictions))
                return "Pain";

            return "Custom";
        }
        private static IPRSList GetDefaultPreset(string pstype)
        {
            return pstype switch
            {
                "Default" => DefineDefaultRestrictions(),// get clone;
                "Pain" => DefinePainRestrictions(),// get clone
                _ => throw new Exception("Should be handled in event handler directly"),
            };
        }


        // I/O
        internal void LoadPresetSettings(IPRSList newpreset)
        {
            ItemRestrictions = newpreset;

            // Overwrite and save:
            SaveRandomizerSettings();
            SetupSaveCallbacks(); // cleared in above re-load
        }
        internal void SaveRandomizerSettings()
        {
            TextWriter? writer = null;
            try
            {
                writer = new StreamWriter(SettingsFilePath, false);
                var serializable = ItemRestrictions;
                new XmlSerializer(serializable.GetType()).Serialize(writer, serializable);
            }
            finally
            {
                writer?.Close();
            }
        }
        internal void LoadRandomizerSettings()
        {
            TextReader? reader = null;
            try
            {
                reader = new StreamReader(SettingsFilePath);
                var deserialized = new XmlSerializer(typeof(IPRSList)).Deserialize(reader) as IPRSList;
                if (deserialized != null)
                    ItemRestrictions = deserialized;
            }
            finally
            {
                reader?.Close();
            }
            Preset = GetPresetType();
        }


        internal static bool CheckOverwriteSttgOK()
        {
            // user has clicked "never show again"
            if (!Settings.Default.ShowWarnSttgChg)
                    return true;

            var warning = new NewRandoSettingsWarn();
            warning.ShowDialog();
            return warning.IsOk;
        }

        public static IPRSList DefineDefaultRestrictions()
        {
            IPRSList iprs = new() {
                new ItemRestriction("Estus Flask", ITEMID.ESTUS),
                new ItemRestriction("Ring of Binding", ITEMID.RINGOFBINDING),
                new ItemRestriction("Silvercat Ring", ITEMID.CATRING),
                new ItemRestriction("Aged Feather", ITEMID.AGEDFEATHER),
                new ItemRestriction("Dull Ember", ITEMID.DULLEMBER),
                new ItemRestriction("Rotunda Lockstone", ITEMID.ROTUNDALOCKSTONE),
                new ItemRestriction("King's Ring", ITEMID.KINGSRING),
                new ItemRestriction("Ashen Mist Heart", ITEMID.ASHENMIST),
                new ItemRestriction("Key to the Embedded", ITEMID.KEYTOEMBEDDED),

                new ItemRestriction("Any Blacksmith Key", ITEMGROUP.BlacksmithKey),
                new ItemRestriction("Any Pyromancy Flame", ITEMGROUP.Pyro),
                new ItemRestriction("Any Staff", ITEMGROUP.Staff),
                new ItemRestriction("Any Chime", ITEMGROUP.Chime),
            };
            return iprs;
        }
        public static IPRSList DefinePainRestrictions()
        {
            int painmin = 10;
            IPRSList iprs = new() {
                new ItemRestriction("Estus Flask", ITEMID.ESTUS, RestrType.Distance, painmin),
                new ItemRestriction("Ring of Binding", ITEMID.RINGOFBINDING),
                new ItemRestriction("Silvercat Ring", ITEMID.CATRING, RestrType.Distance, painmin),
                new ItemRestriction("Aged Feather", ITEMID.AGEDFEATHER, RestrType.Distance, painmin),
                new ItemRestriction("Dull Ember", ITEMID.DULLEMBER, RestrType.Distance, 18),
                new ItemRestriction("Rotunda Lockstone", ITEMID.ROTUNDALOCKSTONE, RestrType.Distance, painmin),
                new ItemRestriction("King's Ring", ITEMID.KINGSRING, RestrType.Distance, painmin),
                new ItemRestriction("Ashen Mist Heart", ITEMID.ASHENMIST, RestrType.Distance, painmin),
                new ItemRestriction("Key to the Embedded", ITEMID.KEYTOEMBEDDED, RestrType.Distance, 18),

                new ItemRestriction("Any Blacksmith Key", ITEMGROUP.BlacksmithKey),
                new ItemRestriction("Any Pyromancy Flame", ITEMGROUP.Pyro, RestrType.Distance, painmin),
                new ItemRestriction("Any Staff", ITEMGROUP.Staff),
                new ItemRestriction("Any Chime", ITEMGROUP.Chime),
            };
            return iprs;
        }

        private static bool AreFunctionallySame(IPRSList list1, IPRSList list2)
        {
            if (list1.Count != list2.Count) 
                return false;

            // check all elements. One day can override a proper equality comparer
            for (int i = 0; i < list1.Count; i++)
            {
                var ir1 = list1[i];
                var ir2 = list2[i];
                if (ir1.ItemID != ir2.ItemID || ir1.DistMax != ir2.DistMax ||
                    ir1.DistMin != ir2.DistMin || ir1.RestrType != ir2.RestrType)
                {
                    return false;
                }
            }
            return true;
        }


    }
}
