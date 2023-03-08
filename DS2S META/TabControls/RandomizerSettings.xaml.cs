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

namespace DS2S_META
{
    /// <summary>
    /// Randomizer Code & Front End for RandomizerSettings.xaml
    /// </summary>
    public partial class RandomizerSettings : METAControl
    {
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

            private Visibility _areaSelectionVisible = Visibility.Collapsed;
            public Visibility AreaSelectionVisible
            {
                get => _areaSelectionVisible; set
                {
                    _areaSelectionVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreaSelectionVisible)));
                }
            }

            public List<int> ItemIDs { get; set; }

            public KeyValuePair<ItemRestrictionType, string> Type { get; set; } = GetTypeComboItem(ItemRestrictionType.Anywhere);

            public string Name { get; set; }

            public KeyValuePair<MapArea, string> Area { get; set; } = GetAreaComboItem(MapArea.Majula);
            public int AreaDistanceLowerBound { get; set; } = 0;
            public int AreaDistanceUpperBound { get; set; } = 0;

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

        public static ObservableCollection<ItemPlacementRestrictionSettings> ItemRestrictions { get; set; } = new()
        {
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

        // FrontEnd:
        public RandomizerSettings()
        {
            InitializeComponent();
        }

        private void RestrictionTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var box = sender as ComboBox;
            var context = box.DataContext as ItemPlacementRestrictionSettings;
            context.AreaSelectionVisible = context.Type.Key == ItemRestrictionType.AreaDistance ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}