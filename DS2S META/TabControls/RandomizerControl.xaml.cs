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
    /// Randomizer Code & Front End for RandomizerControl.xaml
    /// </summary>
    public partial class RandomizerControl : METAControl
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

        // Fields:
        private Color PURPLE = Color.FromArgb(0xFF, 0xB1, 0x59, 0xCC);
        private Color LIGHTPURPLE = Color.FromArgb(0xFF, 0xCE, 0x73, 0xF1); // #FFCE73F1
        private Color LIGHTRED = Color.FromArgb(0xFF, 0xDA, 0x4D, 0x4D);
        private Color LIGHTGREEN = Color.FromArgb(0xFF, 0x87, 0xCC, 0x59);
        internal RandomizerManager RM = new RandomizerManager();
        public static bool IsRandomized = false;
        private int Seed => Convert.ToInt32(txtSeed.Text);

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
        public RandomizerControl()
        {
            InitializeComponent();
            txtSeed.Text = Settings.Default.LastRandomizerSeed;
        }
        private void FixSeedVisibility()
        {
            //Handles the "Seeed..." label on the text box
            if (txtSeed.Text == "")
                lblSeed.Visibility = Visibility.Visible;
            else
                lblSeed.Visibility = Visibility.Hidden;

        }
        private void txtSeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            FixSeedVisibility();
            Settings.Default.LastRandomizerSeed = txtSeed.Text;
        }
        private void btnRerandomize_Click(object sender, RoutedEventArgs e)
        {
            PopulateNewSeed();
            rando_core_process(RANDOPROCTYPE.Rerand);
        }
        private void btnRandomize_Click(object sender, RoutedEventArgs e)
        {
            if (IsRandomized)
                rando_core_process(RANDOPROCTYPE.Unrand);
            else
                rando_core_process(RANDOPROCTYPE.Rand);
        }
        private enum RANDOPROCTYPE { Rand, Unrand, Rerand }
        private async void rando_core_process(RANDOPROCTYPE rpt)
        {
            randomizerSetup();
            CreateItemRestrictions();

            // Inform user of progress..
            btnRandomize.IsEnabled = false;
            lblWorking.Visibility = Visibility.Visible;

            int seed = Seed;
            var tasks = new List<Task>();
            switch (rpt)
            {
                case RANDOPROCTYPE.Rand:
                    await Task.Run(() => RM.Randomize(seed));
                    break;
                case RANDOPROCTYPE.Unrand:
                    await Task.Run(() => RM.Unrandomize());
                    break;
                case RANDOPROCTYPE.Rerand:
                    if (IsRandomized)
                        await Task.Run(() => RM.Unrandomize());
                    await Task.Run(() => RM.Randomize(seed));
                    break;
            }
            IsRandomized = RM.IsRandomized;


            // Update UI:
            btnRandomize.Content = IsRandomized ? "Unrandomize!" : "Randomize!";
            Color bkg = IsRandomized ? LIGHTPURPLE : LIGHTGREEN;
            lblGameRandomization.Background = new SolidColorBrush(bkg);

            if (IsRandomized)
                txtGameState.Text = $"Game is Randomized!{Environment.NewLine} Seed: {ZeroPadString(RM.CurrSeed)}";
            else
                txtGameState.Text = $"Game is Normal!";


            // Restore after completion:
            lblWorking.Visibility = Visibility.Hidden;
            btnRandomize.IsEnabled = true;
        }

        private void CreateItemRestrictions()
        {
            RM.Restrictions.Clear();
            foreach (var restriction in ItemRestrictions)
            {
                switch (restriction.Type.Key)
                {
                    case ItemRestrictionType.Anywhere:
                        // No reason to create a dummy filter
                        //RM.ItemSetRestrictions.Add(restriction.ItemIDs, new NoPlacementRestriction());
                        break;
                    case ItemRestrictionType.Vanilla:
                        RM.OneFromItemSetRestrictions.Add(restriction.ItemIDs, new VanillaPlacementRestriction());
                        break;
                    case ItemRestrictionType.AreaDistance:
                        RM.OneFromItemSetRestrictions.Add(restriction.ItemIDs, new AreaDistancePlacementRestriction(restriction.Area.Key, restriction.AreaDistanceLowerBound, restriction.AreaDistanceUpperBound));
                        break;
                }
            }
        }

        private bool randomizerSetup()
        {
            if (!ensureHooked())
                return false;

            if (!RM.IsInitialized)
                RM.Initalize(Hook);

            // Warn user about the incoming warp
            if (Properties.Settings.Default.ShowWarnRandowarp)
            {
                var randowarning = new RandoWarpWarning()
                {
                    Title = "Randomizer Warp Warning",
                    Width = 375,
                    Height = 175,
                };
                randowarning.ShowDialog();
            }

            // Sort out seeding
            if (txtSeed.Text == "")
                PopulateNewSeed();

            return true;
        }

        private string ZeroPadString(int seed)
        {
            return seed.ToString().PadLeft(10, '0');
        }
        private void MsgMissingDS2()
        {
            MessageBox.Show("Please open Dark Souls 2 first.");
            return;
        }
        private void imgGenerate_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PopulateNewSeed();
        }
        private void PopulateNewSeed()
        {
            int seed = RandomizerManager.GetRandom();
            txtSeed.Text = ZeroPadString(seed); // 10 is max value of digits of int32 in decimal
        }
        private bool ensureHooked()
        {
            // Want to try to Hook in DS2 to change the wooden chest above cardinal tower to metal chest items:
            if (Hook.Hooked)
                return true;

            // Window warning to user:
            MsgMissingDS2();
            return false;
        }

        private void RestrictionTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var box = sender as ComboBox;
            var context = box.DataContext as ItemPlacementRestrictionSettings;
            context.AreaSelectionVisible = context.Type.Key == ItemRestrictionType.AreaDistance ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}