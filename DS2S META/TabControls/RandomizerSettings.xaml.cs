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

namespace DS2S_META
{
    // Abbreviations for more readable save/load methods for settings
    using IPRSList = ObservableCollection<ItemPlacementRestrictionSettings>;
    using SerializableCAD = List<KeyValueStruct<MapArea, List<KeyValueStruct<MapArea, int>>>>;

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
            TextWriter writer = null;
            try
            {
                writer = new StreamWriter(SettingsFilePath, false);
                var serializable = new ValueTuple<IPRSList, SerializableCAD>(ItemRestrictions, DictionaryToKeyValueStructList(AreaDistanceCalculator.ConnectedAreaDistances));
                new XmlSerializer(serializable.GetType()).Serialize(writer, serializable);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        internal static void LoadRandomizerSettings()
        {
            TextReader reader = null;
            try
            {
                reader = new StreamReader(SettingsFilePath);
                var deserialized = (ValueTuple<IPRSList, SerializableCAD>)new XmlSerializer(typeof(ValueTuple<IPRSList, SerializableCAD>)).Deserialize(reader);
                ItemRestrictions = deserialized.Item1;
                AreaDistanceCalculator.ConnectedAreaDistances = KeyValueStructListToDictionary(deserialized.Item2);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }


        public class ItemPlacementRestrictionSettings : INotifyPropertyChanged
        {
            public static Dictionary<ItemRestrictionType, string> TypeComboItems { get; set; } = new() {
                { ItemRestrictionType.Anywhere, "Anywhere" },
                { ItemRestrictionType.Vanilla , "Vanilla" },
                { ItemRestrictionType.AreaDistance , "Distance from area" }
            };

            public event PropertyChangedEventHandler? PropertyChanged;

            public static KeyValuePair<ItemRestrictionType, string> GetTypeComboItem(ItemRestrictionType defaultType)
            {
                return TypeComboItems.Single(e => e.Key == defaultType);
            }

            public void UpdateVisibility()
            {
                AreaSelectionVisible = Type == ItemRestrictionType.AreaDistance ? Visibility.Visible : Visibility.Collapsed;
            }


            // Those don't need to fire PropertyChangedEvents, since those cannot be changed in UI
            public List<int> ItemIDs { get; set; }
            public string Name { get; set; }


            // Used for hiding the section for area selection and min/max distance
            private Visibility _areaSelectionVisible = Visibility.Collapsed;
            [XmlIgnore]
            public Visibility AreaSelectionVisible
            {
                get => _areaSelectionVisible; set
                {
                    _areaSelectionVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreaSelectionVisible)));
                }
            }


            // ComboBox selection for item restriction type
            private ItemRestrictionType type = ItemRestrictionType.Anywhere;
            public ItemRestrictionType Type
            {
                get => type; set
                {
                    type = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
                }
            }

            // ComboBox selection for map area
            private MapArea area = MapArea.Majula;
            public MapArea Area
            {
                get => area; set
                {
                    area = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Area)));
                }
            }


            // Min distance field value
            private int areaDistanceLowerBound = 0;
            public int AreaDistanceLowerBound
            {
                get => areaDistanceLowerBound; set
                {
                    areaDistanceLowerBound = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreaSelectionVisible)));
                }
            }

            // Max distance field value
            private int areaDistanceUpperBound = 0;
            public int AreaDistanceUpperBound
            {
                get => areaDistanceUpperBound; set
                {
                    areaDistanceUpperBound = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreaSelectionVisible)));
                }
            }


            // Necessary for deserialization
            public ItemPlacementRestrictionSettings()
            {
                ItemIDs = new();
                Name = string.Empty;
            }

            public ItemPlacementRestrictionSettings(string name, List<int> itemIds, ItemRestrictionType defaultType = ItemRestrictionType.Anywhere, MapArea area = MapArea.Majula, int minDist = 0, int maxDist = 0)
            {
                ItemIDs = itemIds;
                Name = name;
                Type = defaultType;
                Area = area;
                AreaDistanceLowerBound = minDist;
                AreaDistanceUpperBound = maxDist;
                UpdateVisibility();
            }
        }


        private static List<MapArea> AreasExcludedFromComboBox = new() { MapArea.LordsPrivateChamber, MapArea.MemoryOfTheKing, MapArea.DarkChasmOfOld };
        public static Dictionary<MapArea, string> MapAreaComboItems { get; set; } = MapAreas.toString.Where(area => !AreasExcludedFromComboBox.Contains(area.Key)).ToDictionary(a => a.Key, a => a.Value);
        public static Dictionary<MapArea, string> AreasWithConnectedAreasComboItems { get; set; } = new();

        public static ObservableCollection<ItemPlacementRestrictionSettings> ItemRestrictions { get; set; } = new();
        public static ObservableCollection<KeyValueStruct<KeyValueStruct<MapArea, string>, int>> ConnectedAreaDistanceListItems { get; set; } = new();


        public RandomizerSettings()
        {
            InitializeComponent();
            try
            {
                LoadRandomizerSettings();
            }
            catch (Exception) // If the settings file is broken, FileNotFoundException won't help - just recreate the settings
            {
                ItemRestrictions = new() {
                    new ItemPlacementRestrictionSettings("Estus Flask", new(){60155000}, ItemRestrictionType.Anywhere),
                    new ItemPlacementRestrictionSettings("Ring of Binding", new(){ 40410000 }, ItemRestrictionType.Anywhere),
                    new ItemPlacementRestrictionSettings("Silvercat Ring", new(){ 40420000 }, ItemRestrictionType.Anywhere),
                    new ItemPlacementRestrictionSettings("Aged Feather", new(){ 60355000 }, ItemRestrictionType.Anywhere),

                    new ItemPlacementRestrictionSettings("Any Pyromancy Flame", new(){ 05400000, 05410000 }, ItemRestrictionType.Anywhere),

                    new ItemPlacementRestrictionSettings("Dull Ember", new(){ 50990000 }, ItemRestrictionType.Anywhere),
                    new ItemPlacementRestrictionSettings("Lenigrast's Key", new(){ 50870000 }, ItemRestrictionType.Anywhere),
                    new ItemPlacementRestrictionSettings("Any Blacksmith Key", new(){ 50850000, 50870000, 50990000 }, ItemRestrictionType.Anywhere),

                    new ItemPlacementRestrictionSettings("Rotunda Lockstone", new(){ 50890000 }, ItemRestrictionType.Anywhere),
                    new ItemPlacementRestrictionSettings("King's Ring", new(){ 40510000 }, ItemRestrictionType.Anywhere),
                    new ItemPlacementRestrictionSettings("Ashen Mist Heart", new(){ 50910000 }, ItemRestrictionType.Anywhere),
                };
            }

            UpdateAreasWithConnectedAreasComboItems();

            foreach (var restriction in ItemRestrictions)
            {
                restriction.PropertyChanged += RestrictionPropertiesChanged;
                restriction.UpdateVisibility();
            }
        }

        private void RestrictionTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((sender as ComboBox)?.DataContext as ItemPlacementRestrictionSettings)?.UpdateVisibility();
        }

        private void RestrictionPropertiesChanged(object? sender, PropertyChangedEventArgs e)
        {
            SaveRandomizerSettings();
        }

        public static void UpdateAreasWithConnectedAreasComboItems()
        {
            AreasWithConnectedAreasComboItems = MapAreas.toString.Where(area => AreaDistanceCalculator.ConnectedAreaDistances.ContainsKey(area.Key) && AreaDistanceCalculator.ConnectedAreaDistances[area.Key].Any()).ToDictionary(a => a.Key, a => a.Value);
        }

        private void UpdateConnectedAreaDistanceListItems(MapArea area)
        {
            ConnectedAreaDistanceListItems.Clear();

            // Shouldn't ever occur, if combobox entries are properly updated based on the dictionary entries
            if (!AreaDistanceCalculator.ConnectedAreaDistances.ContainsKey(area) || !AreaDistanceCalculator.ConnectedAreaDistances[area].Any())
            {
                listViewConnectedAreasDistances.Visibility = Visibility.Collapsed;
                return;
            }

            listViewConnectedAreasDistances.Visibility = Visibility.Visible;

            foreach (var a in AreaDistanceCalculator.ConnectedAreaDistances[area])
                ConnectedAreaDistanceListItems.Add(new KeyValueStruct<KeyValueStruct<MapArea, string>, int>(new KeyValueStruct<MapArea, string>(a.Key, MapAreas.toString[a.Key]), a.Value));
        }

        private void ConnectedAreaDistanceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewConnectedAreasDistances == null)
                return;

            var selectedArea = ((KeyValuePair<MapArea, string>)((ComboBox)sender).SelectedValue).Key;
            UpdateConnectedAreaDistanceListItems(selectedArea);
        }

        private void ConnectedAreaDistanceBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;

            MapArea area = ((KeyValuePair<MapArea, string>)connectedAreaDistanceComboBox.SelectedValue).Key;
            MapArea targetArea = ((KeyValueStruct<KeyValueStruct<MapArea, string>, int>)(box.DataContext)).Key.Key;
            try
            {
                AreaDistanceCalculator.ConnectedAreaDistances[area][targetArea] = int.Parse(box.Text);
            }
            catch (Exception) { }

            SaveRandomizerSettings();
        }
    }
}