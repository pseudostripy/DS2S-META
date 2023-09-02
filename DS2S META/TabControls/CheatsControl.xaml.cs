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
using DS2S_META.ViewModels;

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
            // don't do this
            var vm = (CheatsViewModel)DataContext;
            vm.Hook?.Give17kReward();
        }
        private void Button_Click_31(object sender, RoutedEventArgs e)
        {
            // don't do this
            var vm = (CheatsViewModel)DataContext;
            vm.Hook?.Give3Chunk1Slab();
        }
    }
}