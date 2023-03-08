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
        // Fields:
        private Color PURPLE = Color.FromArgb(0xFF, 0xB1, 0x59, 0xCC);
        private Color LIGHTPURPLE = Color.FromArgb(0xFF, 0xCE, 0x73, 0xF1); // #FFCE73F1
        private Color LIGHTRED = Color.FromArgb(0xFF, 0xDA, 0x4D, 0x4D);
        private Color LIGHTGREEN = Color.FromArgb(0xFF, 0x87, 0xCC, 0x59);
        internal RandomizerManager RM = new RandomizerManager();
        public static bool IsRandomized = false;
        private int Seed => Convert.ToInt32(txtSeed.Text);

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
            RM.OneFromItemSetRestrictions.Clear();
            foreach (var restriction in RandomizerSettings.ItemRestrictions)
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
    }
}