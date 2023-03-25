using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using DS2S_META.Randomizer;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Octokit;
using System.ComponentModel;
using DS2S_META.Properties;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Drawing;
using static DS2S_META.RandomizerSettings;
using System.Runtime.CompilerServices;

namespace DS2S_META
{
    // Abbreviations for more readable save/load methods for settings
    using IPRSList = ObservableCollection<ItemRestriction>;
    //using SerializableCAD = List<KeyValueStruct<MapArea, List<KeyValueStruct<MapArea, int>>>>;

    public partial class RandomizerSettings : METAControl
    {
        static readonly string SettingsFilePath = $"{GetTxtResourceClass.ExeDir}/Resources/RandomizerSettings.xml";

        public struct KeyValueStruct<K, V>
        {
            public K Key { get; set; }
            public V Value { get; set; }

            public KeyValueStruct(K key, V value) : this()
            {
                Key = key;
                Value = value;
            }
        }

        public static List<KeyValueStruct<K, List<KeyValueStruct<L, V>>>> DictionaryToKeyValueStructList<K, L, V>(Dictionary<K, Dictionary<L, V>> dict)
            where K : notnull
            where L : notnull
            where V : notnull
        {
            var list = new List<KeyValueStruct<K, List<KeyValueStruct<L, V>>>>();
            foreach (var kv in dict)
            {
                list.Add(new KeyValueStruct<K, List<KeyValueStruct<L, V>>>(kv.Key, DictionaryToKeyValueStructList(kv.Value)));
            }
            return list;
        }

        public static List<KeyValueStruct<K, V>> DictionaryToKeyValueStructList<K, V>(Dictionary<K, V> dict)
            where K : notnull
            where V : notnull
        {
            var list = new List<KeyValueStruct<K, V>>();
            foreach (var kv in dict)
            {
                list.Add(new KeyValueStruct<K, V>(kv.Key, kv.Value));
            }
            return list;
        }

        public static Dictionary<K, Dictionary<L, V>> KeyValueStructListToDictionary<K, L, V>(List<KeyValueStruct<K, List<KeyValueStruct<L, V>>>> list)
            where K : notnull
            where L : notnull
            where V : notnull
        {
            var dict = new Dictionary<K, Dictionary<L, V>>();
            foreach (var kv in list)
            {
                dict[kv.Key] = KeyValueStructListToDictionary(kv.Value);
            }
            return dict;
        }

        public static Dictionary<K, V> KeyValueStructListToDictionary<K, V>(List<KeyValueStruct<K, V>> list)
            where K : notnull
            where V : notnull
        {
            var dict = new Dictionary<K, V>();
            foreach (var kv in list)
            {
                dict[kv.Key] = kv.Value;
            }
            return dict;
        }

        internal static void SaveRandomizerSettings()
        {
            //TextWriter writer = null;
            //try
            //{
            //    writer = new StreamWriter(SettingsFilePath, false);
            //    var serializable = new ValueTuple<IPRSList, SerializableCAD>(ItemRestrictions, DictionaryToKeyValueStructList(AreaDistanceCalculator.ConnectedAreaDistances));
            //    new XmlSerializer(serializable.GetType()).Serialize(writer, serializable);
            //}
            //finally
            //{
            //    if (writer != null)s
            //        writer.Close();
            //}
        }

        internal static void LoadRandomizerSettings()
        {
            //TextReader reader = null;
            //try
            //{
            //    reader = new StreamReader(SettingsFilePath);
            //    var deserialized = (ValueTuple<IPRSList, SerializableCAD>)new XmlSerializer(typeof(ValueTuple<IPRSList, SerializableCAD>)).Deserialize(reader);
            //    ItemRestrictions = deserialized.Item1;
            //    AreaDistanceCalculator.ConnectedAreaDistances = KeyValueStructListToDictionary(deserialized.Item2);
            //}
            //finally
            //{
            //    reader?.Close();
            //}
        }


        public class ItemRestriction : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            // Fields/Properties:
            public string Name { get; set; }
            public List<int> ItemIDs { get; set; }
            public ITEMGROUP GroupType { get; set; }

            private int _distMin = 1;
            public int DistMin
            {
                get => _distMin;
                set
                {
                    _distMin = value;
                    OnPropertyChanged(nameof(DistMin));
                }
            }
            private int _distMax = 20;
            public int DistMax
            {
                get => _distMax;
                set
                {
                    _distMax = value;
                    OnPropertyChanged(nameof(DistMax));
                }
            }
            

            public static Dictionary<RestrType, string> TypeComboItems { get; set; } = new() 
            {
                [RestrType.Anywhere]  = "Anywhere",
                [RestrType.Vanilla]   = "Vanilla",
                [RestrType.Distance]  = "Distance"
            };


            // ComboBox selection for item restriction type
            private RestrType _restrType = RestrType.Anywhere;
            public RestrType RestrType
            {
                get => _restrType;
                set
                {
                    _restrType = value;
                    OnPropertyChanged(nameof(RestrType));
                    OnPropertyChanged(nameof(VisDistSettings));
                }
            }

            //public static KeyValuePair<ItemRestrictionType, string> GetTypeComboItem(ItemRestrictionType defaultType)
            //{
            //    return TypeComboItems.Single(e => e.Key == defaultType);
            //}

            //public void UpdateVisibility()
            //{
            //    AreaSelectionVisible = Type == ItemRestrictionType.AreaDistance ? Visibility.Visible : Visibility.Collapsed;
            //}


            


            // Used for hiding the section for area selection and min/max distance
            //private Visibility _visDistSettings = Visibility.Collapsed;
            public Visibility VisDistSettings => RestrType == RestrType.Distance ? Visibility.Visible : Visibility.Collapsed;
            //[XmlIgnore]
            //public Visibility VisDistSettings
            //{
            //    get => _visDistSettings; 
            //    set
            //    {
            //        _visDistSettings = value;
            //        OnPropertyChanged(nameof(VisDistSettings));
            //    }
            //}


            // Min distance field value
            //private int areaDistanceLowerBound = 0;
            //public int AreaDistanceLowerBound
            //{
            //    get => areaDistanceLowerBound; 
            //    set
            //    {
            //        areaDistanceLowerBound = value;
            //        OnPropertyChanged(nameof(AreaSelectionVisible)));
            //    }
            //}

            //// Max distance field value
            //private int areaDistanceUpperBound = 0;
            //public int AreaDistanceUpperBound
            //{
            //    get => areaDistanceUpperBound; set
            //    {
            //        areaDistanceUpperBound = value;
            //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreaSelectionVisible)));
            //    }
            //}


            //// Necessary for deserialization
            //public ItemPlacementRestrictionSettings()
            //{
            //    ItemIDs = new();
            //    Name = string.Empty;
            //}

            // Constructors:
            public ItemRestriction(string name, ITEMGROUP grp, RestrType restrType = RestrType.Anywhere)
            {
                Name = name;
                ItemIDs = new();
                GroupType = grp;    
                RestrType = restrType;
            }
            public ItemRestriction(string name, ITEMID itemID, RestrType restrType = RestrType.Anywhere)
            {
                Name = name;
                ItemIDs = new List<int>() { (int)itemID };
                GroupType = ITEMGROUP.Specified;
                RestrType = restrType;
            }

        }

        public static Dictionary<MapArea, string> MapAreaComboItems { get; set; } = new();
        public static IPRSList ItemRestrictions { get; set; } = new();
        public static ObservableCollection<KeyValueStruct<KeyValueStruct<MapArea, string>, int>> ConnectedAreaDistanceListItems { get; set; } = new();


        public RandomizerSettings()
        {
            InitializeComponent();
            ItemRestrictions = DefaultRestrictions();
            //try
            //{
            //    LoadRandomizerSettings();
            //}
            //catch (Exception) // If the settings file is broken, FileNotFoundException won't help - just recreate the settings...?
            //{
            //    ItemRestrictions = DefaultRestrictions();
            //    };
            //}
        }

            //UpdateMapAreaComboItems();

            //foreach (var restriction in ItemRestrictions)
            //{
            //    restriction.PropertyChanged += RestrictionPropertiesChanged;
            //    restriction.UpdateVisibility();
            //}


            //private void RestrictionTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
            //{
            //    ((sender as ComboBox)?.DataContext as ItemPlacementRestrictionSettings)?.UpdateVisibility();
            //}

        public static IPRSList DefaultRestrictions()
        {
            return new() {
                new ItemRestriction("Estus Flask", ITEMID.ESTUS),
                new ItemRestriction("Ring of Binding", ITEMID.RINGOFBINDING),
                new ItemRestriction("Silvercat Ring", ITEMID.CATRING),
                new ItemRestriction("Aged Feather", ITEMID.AGEDFEATHER),
                new ItemRestriction("Dull Ember", ITEMID.DULLEMBER),
                new ItemRestriction("Rotunda Lockstone", ITEMID.ROTUNDALOCKSTONE),
                new ItemRestriction("King's Ring", ITEMID.KINGSRING),
                new ItemRestriction("Ashen Mist Heart", ITEMID.ASHENMIST),

                new ItemRestriction("Any Blacksmith Key", ITEMGROUP.BlacksmithKey),
                new ItemRestriction("Any Pyromancy Flame", ITEMGROUP.Pyro),
                new ItemRestriction("Any Staff", ITEMGROUP.Staff),
                new ItemRestriction("Any Chime", ITEMGROUP.Chime),
            };
        }

        private void RestrictionPropertiesChanged(object? sender, PropertyChangedEventArgs e)
        {
            SaveRandomizerSettings();
        }

        private void RandoSettings_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}