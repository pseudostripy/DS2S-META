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

namespace DS2S_META
{
    /// <summary>
    /// Randomizer Code & Front End for RandomizerControl.xaml
    /// </summary>
    public partial class RandomizerControl : METAControl
    {
        // Fields:
        private Color PURPLE = Color.FromArgb(0xFF, 0xB1, 0x59, 0xCC);
        private Color LIGHTRED = Color.FromArgb(0xFF, 0xDA, 0x4D, 0x4D);
        private Color LIGHTGREEN = Color.FromArgb(0xFF, 0x87, 0xCC, 0x59);
        internal RandomizerManager RM = new RandomizerManager();
        private bool IsRandomized => RM.IsRandomized;

        
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
        private void btnRandomize_Click(object sender, RoutedEventArgs e)
        {
            // Want to try to Hook in DS2 to change the wooden chest above cardinal tower to metal chest items:
            if (!Hook.Hooked)
            {
                MsgMissingDS2();
                return;
            }

            if (!RM.IsInitialized)
                RM.Initalize(Hook);

            // Sort out seeding
            if (txtSeed.Text == "")
                PopulateNewSeed();
            int seed = Convert.ToInt32(txtSeed.Text);
            
            if (IsRandomized)
                RM.Unrandomize();
            else
                RM.Randomize(seed);

            // Update UI:
            btnRandomize.Content = IsRandomized ? "Unrandomize!" : "Randomize!";
            Color bkg = IsRandomized ? PURPLE : LIGHTGREEN;
            lblGameRandomization.Background = new SolidColorBrush(bkg);
            string gamestate = IsRandomized ? "Randomized" : "Normal";
            lblGameRandomization.Content = $"Game is {gamestate}!";
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
            txtSeed.Text = seed.ToString().PadLeft(10, '0'); // 10 is max value of digits of int32 in decimal
        }
    }
}