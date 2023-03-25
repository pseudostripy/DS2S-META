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


            // Those don't need to fire PropertyChangedEvents, since those cannot be changed in UI
            public List<int> ItemIDs { get; set; }
            public ITEMGROUP GroupType { get; set; }
            public string Name { get; set; }


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
            // Group of items:
            public ItemRestriction(string name, ITEMGROUP grp, RestrType restrType = Randomizer.RestrType.Anywhere)
            {
                Name = name;
                GroupType = grp;    
                RestrType = restrType;
                //UpdateVisibility();
            }

            // Specified item:
            public ItemRestriction(string name, ITEMID itemID, RestrType restrType = Randomizer.RestrType.Anywhere)
            {
                Name = name;
                ItemIDs = new List<int>((int)itemID);
                GroupType = ITEMGROUP.Specified;
                RestrType = restrType;
                //UpdateVisibility();
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

            //        //new ItemRestriction("Any Pyromancy Flame", new(){ 05400000, 05410000 }, RestrType.Anywhere),
            //        //new ItemRestriction("Any Staff", new(){ 1280000, 3800000, 3810000, 3820000, 3830000, 3850000, 3860000, 3870000,
            //        //    3880000, 3890000, 3900000, 3910000, 3930000, 3940000, 4150000, 5370000, 5540000, 11150000 }, RestrType.Anywhere),
            //        //new ItemRestriction("Any Chime", new(){ 2470000, 4010000, 4020000, 4030000, 4040000, 4050000, 4060000, 4080000,
            //        //    4090000, 4100000, 4110000, 4120000, 4150000, 11150000 }, RestrType.Anywhere),
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

        //public static void UpdateMapAreaComboItems()
        //{
        //    MapAreaComboItems = MapAreas.toString.Where(area => AreaDistanceCalculator.HasConnectedAreas(area.Key)).ToDictionary(a => a.Key, a => a.Value);
        //}

        //private void UpdateConnectedAreaDistanceListItems(MapArea area)
        //{
        //    ConnectedAreaDistanceListItems.Clear();

        //    // Shouldn't ever occur, if combobox entries are properly updated based on the dictionary entries
        //    if (!AreaDistanceCalculator.HasConnectedAreas(area))
        //    {
        //        listViewConnectedAreasDistances.Visibility = Visibility.Collapsed;
        //        return;
        //    }

        //    listViewConnectedAreasDistances.Visibility = Visibility.Visible;

        //    foreach (var a in AreaDistanceCalculator.ConnectedAreaDistances[area])
        //        if (AreaDistanceCalculator.HasConnectedAreas(a.Key)) // Hides NPC/covenant pseudoareas
        //            ConnectedAreaDistanceListItems.Add(new KeyValueStruct<KeyValueStruct<MapArea, string>, int>(new KeyValueStruct<MapArea, string>(a.Key, MapAreas.toString[a.Key]), a.Value));
        //}

        //private void ConnectedAreaDistanceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (listViewConnectedAreasDistances == null)
        //        return;

        //    var selectedArea = ((KeyValuePair<MapArea, string>)((ComboBox)sender).SelectedValue).Key;
        //    UpdateConnectedAreaDistanceListItems(selectedArea);
        //}

        //private void ConnectedAreaDistanceBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var box = (TextBox)sender;

        //    MapArea area = ((KeyValuePair<MapArea, string>)connectedAreaDistanceComboBox.SelectedValue).Key;
        //    MapArea targetArea = ((KeyValueStruct<KeyValueStruct<MapArea, string>, int>)(box.DataContext)).Key.Key;
        //    try
        //    {
        //        AreaDistanceCalculator.ConnectedAreaDistances[area][targetArea] = int.Parse(box.Text);
        //    }
        //    catch (Exception) { }

        //    SaveRandomizerSettings();
        //}
    }
}