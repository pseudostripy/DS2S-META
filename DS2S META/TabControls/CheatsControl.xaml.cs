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
using Octokit;

namespace DS2S_META
{
    /// <summary>
    /// Code for Cheats Front End
    /// </summary>
    public partial class CheatsControl : METAControl
    {
        internal Rubbishizer RubMan = new();

        // FrontEnd:
        public CheatsControl()
        {
            InitializeComponent();
        }

        // Rubbish Challenge
        private void cbxRubbishMode_Checked(object sender, RoutedEventArgs e)
        {
            Rubbishize();
        }
        private void cbxRubbishMode_Unchecked(object sender, RoutedEventArgs e)
        {
            Unrubbishize();
        }
        private void Rubbishize()
        {
            RubMan.Rubbishize();
        }
        private void Unrubbishize()
        {
            RubMan.Unrubbishize();
        }

        // 17k
        private void Button_Click_17k(object sender, RoutedEventArgs e)
        {
            Give17kReward();
        }
        private void Button_Click_31(object sender, RoutedEventArgs e)
        {
            Give3Chunk1Slab();
        }
        public void Give17kReward()
        {
            // Add Soul of Pursuer x1 Ring of Blades x1 / 
            var itemids = new int[2] { 0x03D09000, 0x0264CB00 };
            var amounts = new short[2] { 1, 1 };
            Hook.GiveItems(itemids, amounts);
            Hook.AddSouls(17001);
        }
        public void Give3Chunk1Slab()
        {
            // For the lizard in dlc2
            var items = new ITEMID[2] { ITEMID.TITANITECHUNK, ITEMID.TITANITESLAB };
            var itemids = items.Cast<int>().ToArray();
            var amounts = new short[2] { 3, 1 };
            Hook.GiveItems(itemids, amounts);
        }
    }
}