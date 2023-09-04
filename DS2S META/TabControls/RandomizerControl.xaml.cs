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
using DS2S_META.ViewModels;
using static DS2S_META.State;

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
        internal RandomizerManager RM = new();
        public static bool IsRandomized = false;
        private int Seed => Convert.ToInt32(txtSeed.Text);
        private RandoSettingsViewModel VM;

        // FrontEnd:
        public RandomizerControl()
        {
            InitializeComponent();
            txtSeed.Text = Settings.Default.LastRandomizerSeed;
        }

        public override void InitTab()
        {
            VM = (RandoSettingsViewModel)DataContext;
        }

        private void FixSeedVisibility()
        {
            //Handles the "Seed..." label on the text box
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
            RM.IsRandomSeed = true;
            rando_core_process(RANDOPROCTYPE.Rerand);
        }
        private void btnRandomize_Click(object sender, RoutedEventArgs e)
        {
            if (txtSeed.Text.Length == 0)
            {
                RM.IsRandomSeed = true;
                PopulateNewSeed();
            } else
                RM.IsRandomSeed = false;

            if (IsRandomized)
                rando_core_process(RANDOPROCTYPE.Unrand);
            else
                rando_core_process(RANDOPROCTYPE.Rand);
        }
        private enum RANDOPROCTYPE { Rand, Unrand, Rerand }
        private async void rando_core_process(RANDOPROCTYPE rpt)
        {
            RandomizerSetup();
            CreateItemRestrictions();

            // Inform user of progress..
            btnRandomize.IsEnabled = false;
            lblWorking.Visibility = Visibility.Visible;

            int seed = Seed;
            //RM.SetSeed(seed);

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
            // to reconsider structure
            RM.UIRestrictions = new();
            var vm = (RandoSettingsViewModel)DataContext;
            RM.UIRestrictions = vm.ItemRestrictions.Select(ir => ir).ToList();
        }
        

        private bool RandomizerSetup()
        {
            if (!EnsureHooked())
                return false;

            if (VM?.Hook == null) return false;

            if (!RM.IsInitialized)
                RM.Initalize(VM.Hook);

            // Get updated settings:
            var vm = (RandoSettingsViewModel)DataContext;
            RM.IsRaceMode = vm.RaceMode;

            // Warn user about the incoming warp
            // hook.Loaded = true when in game (false on load screens)
            if (VM.Hook.InGame && Settings.Default.ShowWarnRandowarp)
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

        private static string ZeroPadString(int seed)
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
            if (!RandomizerManager.GenCRCSeed(out int seed))
            {
                MessageBox.Show("Issue generating seed automatically. Please report");
                return;
            }

            // 10 is max value of digits of int32 in decimal
            txtSeed.Text = ZeroPadString(seed); 
        }
        private bool EnsureHooked()
        {
            if (VM?.Hook != null && VM.Hook.Hooked)
                return true;

            // Window warning to user:
            MsgMissingDS2();
            return false;
        }
    }
}