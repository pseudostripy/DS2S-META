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
        
    }
}