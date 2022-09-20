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

        
        // FrontEnd:
        public RandomizerControl()
        {
            InitializeComponent();
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
        }
        private async void btnRandomize_Click(object sender, RoutedEventArgs e)
        {
            // Want to try to Hook in DS2 to change the wooden chest above cardinal tower to metal chest items:
            if (!Hook.Hooked)
            {
                MsgMissingDS2();
                return;
            }

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
            int seed = Convert.ToInt32(txtSeed.Text);

            // Inform user of progress..
            btnRandomize.IsEnabled = false;
            lblWorking.Visibility = Visibility.Visible;

            var tasks = new List<Task>();
            if (IsRandomized)
                await Task.Run( () => RM.Unrandomize());
            else
                await Task.Run(() => RM.Randomize(seed));
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
            int seed = RM.GetRandom();
            txtSeed.Text = ZeroPadString(seed); // 10 is max value of digits of int32 in decimal
        }
    }
}