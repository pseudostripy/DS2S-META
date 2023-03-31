using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DS2S_META.Randomizer;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DS2S_META.Properties;
using System.Xml.Serialization;
using static DS2S_META.RandomizerSettings;
using System.Runtime.CompilerServices;
using DS2S_META.ViewModels;

namespace DS2S_META
{   
    using IPRSList = ObservableCollection<ItemRestriction>;
    
    public partial class RandomizerSettings : METAControl
    {
        // Fields/Properties:
        public static readonly string SettingsFilePath = $"{GetTxtResourceClass.ExeDir}/Resources/RandomizerSettings.xml";
        public static Dictionary<MapArea, string> MapAreaComboItems { get; set; } = new();
        public static IPRSList ItemRestrictions { get; set; } = new();
        
        // Constructor:
        public RandomizerSettings()
        {
            InitializeComponent();
            try
            {
                LoadRandomizerSettings();
            }
            catch (Exception) 
            {
                // Use defaults when not available
                ItemRestrictions = DefaultRestrictions();
                SetItemGroupOptions();
                SaveRandomizerSettings();
            }
            SetupSaveCallbacks();
        }

        // Methods:
        private void SetupSaveCallbacks()
        {
            // Save settings after each change
            foreach (var restriction in ItemRestrictions)
                restriction.PropertyChanged += RestrictionPropertiesChanged;
        }
        internal static void SaveRandomizerSettings()
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
        internal static void LoadRandomizerSettings()
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
        }

        public static IPRSList DefaultRestrictions()
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
        private void SetItemGroupOptions()
        {
            foreach (var irest in ItemRestrictions)
            {
                switch (irest.GroupType)
                {
                    case ITEMGROUP.Specified:
                        break;

                    // TODO Make more robust with Param field types
                    case ITEMGROUP.Pyro:
                        irest.ItemIDs = new() { 05400000, 05410000 };
                        break;

                    case ITEMGROUP.Staff:
                        irest.ItemIDs = new() { 1280000, 3800000, 3810000, 3820000, 3830000, 3850000, 3860000, 3870000,
                                            3880000, 3890000, 3900000, 3910000, 3930000, 3940000, 4150000, 5370000,
                                            5540000, 11150000 };
                        break;

                    case ITEMGROUP.BlacksmithKey:
                        irest.ItemIDs = new List<ITEMID>() { ITEMID.LENIGRASTKEY, ITEMID.DULLEMBER, ITEMID.FANGKEY }.Cast<int>().ToList();
                        break;

                    case ITEMGROUP.Chime:
                        irest.ItemIDs = new() { 2470000, 4010000, 4020000, 4030000, 4040000, 4050000, 4060000, 4080000,
                                            4090000, 4100000, 4110000, 4120000, 4150000, 11150000 };
                        break;
                    default:
                        throw new Exception("which one is it?");
                }
            }
        }

        // Events:
        private void RestrictionPropertiesChanged(object? sender, PropertyChangedEventArgs e)
        {
            SaveRandomizerSettings();
        }
    }
}