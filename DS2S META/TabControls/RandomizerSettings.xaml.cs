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

namespace DS2S_META
{
    public partial class RandomizerSettings : METAControl
    {
        static readonly string SettingsFilePath = $"{GetTxtResourceClass.ExeDir}/Resources/RandomizerSettings.xml";


        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                serializer.Serialize(writer, objectToWrite);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T)serializer.Deserialize(reader);
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

            public static Dictionary<MapArea, string> MapAreaComboItems { get; set; } = new()
            {
                {MapArea.ThingsBetwixt , "Things Betwixt"},
                {MapArea.Majula, "Majula"},
                {MapArea.ForestOfFallenGiants, "Forest of Fallen Giants"},
                {MapArea.HeidesTowerOfFlame, "Heide's Tower of Flame"},
                {MapArea.CathedralOfBlue, "Cathedral of Blue"},
                {MapArea.NoMansWharf, "No-man's Wharf"},
                {MapArea.TheLostBastille, "The Lost Bastille"},
                {MapArea.BelfryLuna, "Belfry Luna"},
                {MapArea.SinnersRise, "Sinner's Rise"},
                {MapArea.HuntsmansCopse, "Huntsman's Copse"},
                {MapArea.UndeadPurgatory, "Undead Purgatory"},
                {MapArea.HarvestValley, "Harvest Valley"},
                {MapArea.EarthenPeak, "Earthen Peak"},
                {MapArea.IronKeep, "Iron Keep"},
                {MapArea.BelfrySol, "Belfry Sol"},
                {MapArea.ShadedWoods, "Shaded Woods"},
                {MapArea.DoorsOfPharros, "Doors of Pharros"},
                {MapArea.BrightstoneCoveTseldora, "Brightstone Cove Tseldora"},
                // {MapArea.LordsPrivateChamber, "LordsPrivateChamber"},
                {MapArea.ThePit, "The Pit"},
                {MapArea.GraveOfSaints, "Grave of Saints"},
                {MapArea.TheGutter, "The Gutter"},
                {MapArea.BlackGulch, "Black Gulch"},
                {MapArea.ShrineOfWinter, "Shrine of Winter"},
                {MapArea.DrangleicCastle, "Drangleic Castle"},
                {MapArea.KingsPassage, "King's Passage"},
                {MapArea.ShrineOfAmana, "Shrine of Amana"},
                {MapArea.UndeadCrypt, "Undead Crypt"},
                {MapArea.ThroneOfWant, "Throne of Want"},
                {MapArea.AldiasKeep, "Aldia's Keep"},
                {MapArea.DragonAerie, "Dragon Aerie"},
                {MapArea.DragonShrine, "Dragon Shrine"},
                // {MapArea.DarkChasmOfOld, "DarkChasmOfOld"},
                {MapArea.MemoryOfJeigh, "Memory of Jeigh"},
                {MapArea.MemoryOfOrro, "Memory of Orro"},
                {MapArea.MemoryOfVammar, "Memory of Vammar"},
                {MapArea.DragonMemories, "Dragon Memories"},
                // {MapArea.MemoryOfTheKing, "MemoryOfTheKing"},
                {MapArea.ShulvaSanctumCity, "Shulva Sanctum City"},
                {MapArea.DragonsSanctum, "Dragon's Sanctum"},
                {MapArea.DragonsRest, "Dragon's Rest"},
                {MapArea.CaveOfTheDead, "Cave of the Dead"},
                {MapArea.BrumeTower, "Brume Tower"},
                {MapArea.IronPassage, "Iron Passage"},
                {MapArea.MemoryOfTheOldIronKing, "Memory of the Old Iron King"},
                {MapArea.FrozenEleumLoyce, "Frozen Eleum Loyce"},
                {MapArea.GrandCathedral, "Grand Cathedral"},
                {MapArea.TheOldChaos, "The Old Chaos"},
                {MapArea.FrigidOutskirts, "Frigid Outskirts" }
            };

            public event PropertyChangedEventHandler? PropertyChanged;

            public static KeyValuePair<ItemRestrictionType, string> GetTypeComboItem(ItemRestrictionType defaultType)
            {
                return TypeComboItems.Single(e => e.Key == defaultType);
            }
            public static KeyValuePair<MapArea, string> GetAreaComboItem(MapArea area)
            {
                return MapAreaComboItems.Single(e => e.Key == area);
            }

            public void UpdateRawEnumValues()
            {
                RawTypeValue = Type.Key;
                RawAreaValue = Area.Key;
            }

            public void UpdateEnumValuesFromRaw()
            {
                Type = GetTypeComboItem(RawTypeValue);
                Area = GetAreaComboItem(RawAreaValue);
            }

            // Those don't need to fire PropertyChangedEvents, since those cannot be changed in UI
            public List<int> ItemIDs { get; set; }
            public string Name { get; set; }

            // Used for hiding the section for area selection and min/max distance
            private Visibility _areaSelectionVisible = Visibility.Collapsed;
            public Visibility AreaSelectionVisible
            {
                get => _areaSelectionVisible; set
                {
                    _areaSelectionVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreaSelectionVisible)));
                }
            }

            
            // ComboBox selection for item restriction type
            private KeyValuePair<ItemRestrictionType, string> type = GetTypeComboItem(ItemRestrictionType.Anywhere);
            [XmlIgnore]
            public KeyValuePair<ItemRestrictionType, string> Type
            {
                get => type; set
                {
                    type = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
                }
            }
            public ItemRestrictionType RawTypeValue; // Bypass for unserializable KeyValuePair class

            // ComboBox selection for map area
            private KeyValuePair<MapArea, string> area = GetAreaComboItem(MapArea.Majula);
            [XmlIgnore]
            public KeyValuePair<MapArea, string> Area
            {
                get => area; set
                {
                    area = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Area)));
                }
            }
            public MapArea RawAreaValue; // Bypass for unserializable KeyValuePair class


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
                Type = GetTypeComboItem(defaultType);
                Area = GetAreaComboItem(area);
                AreaDistanceLowerBound = minDist;
                AreaDistanceUpperBound = maxDist;
                AreaSelectionVisible = defaultType == ItemRestrictionType.AreaDistance ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        public static ObservableCollection<ItemPlacementRestrictionSettings> ItemRestrictions { get; set; }


        public RandomizerSettings()
        {
            InitializeComponent();
            try
            {
                ItemRestrictions = ReadFromXmlFile<ObservableCollection<ItemPlacementRestrictionSettings>>(SettingsFilePath);
                // KeyValuePairs cannot be (de)serialized, so the area enum value is stored in an individual serializable variable
                foreach (var restriction in ItemRestrictions) restriction.UpdateEnumValuesFromRaw();
            }
            catch (FileNotFoundException)
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

            foreach (var restriction in ItemRestrictions) restriction.PropertyChanged += SaveRandomizerSettings;
        }

        private void RestrictionTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var box = sender as ComboBox;
            var context = box.DataContext as ItemPlacementRestrictionSettings;
            context.AreaSelectionVisible = context.Type.Key == ItemRestrictionType.AreaDistance ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveRandomizerSettings(object? sender, PropertyChangedEventArgs e)
        {
            foreach (var restriction in ItemRestrictions) restriction.UpdateRawEnumValues();
            WriteToXmlFile(SettingsFilePath, ItemRestrictions);
        }
    }
}