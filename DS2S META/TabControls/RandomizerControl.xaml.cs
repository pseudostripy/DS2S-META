using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for CovenantControl.xaml
    /// </summary>
    public partial class RandomizerControl : METAControl
    {
        private Dictionary<int, ItemLot> VanillaLots;
        internal bool isRandomized = false;

        public RandomizerControl()
        {
            InitializeComponent();
        }



        //Handles the "Seeed..." label on the text box
        private void FixSeedVisibility()
        {
            if (txtSeed.Text == "")
                lblSeed.Visibility = Visibility.Visible;
            else
                lblSeed.Visibility = Visibility.Hidden;

        }

        private void txtSeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            FixSeedVisibility();
        }

        
        private void randomize()
        {
            // Need to get a list of the vanilla item lots C#.8 pleeeease :(
            if (VanillaLots==null)
                VanillaLots = Hook.GetVanillaLots();
            Dictionary<int, ItemLot> ShuffledLots = new Dictionary<int, ItemLot>();

            // Make into flat list of stuff:
            var flatlist = VanillaLots.SelectMany(kvp => kvp.Value.Lot).ToList();

            
            flatlist.Shuffle();

            // Make into new lots:
            foreach (var kvp in VanillaLots)
            {
                ItemLot IL = new ItemLot();
                for (int row = 0; row < kvp.Value.NumDrops; row++)
                {
                    IL.AddDrop(flatlist[0]);
                    flatlist.RemoveAt(0); // pop
                }
                ShuffledLots[kvp.Key] = IL; // add to new dictionary
            }


            Hook.WriteAllLots(ShuffledLots);
            isRandomized = true;
        }
        private void unrandomize()
        {
            Hook.WriteAllLots(VanillaLots);
            isRandomized = false;
        }

        
        private void btnRandomize_Click(object sender, RoutedEventArgs e)
        {
            // Want to try to Hook in DS2 to change the wooden chest above cardinal tower to metal chest items:
            if (!Hook.Hooked)
                return;

            if (isRandomized)
                unrandomize();
            else
                randomize();

            // Force an area reload. TODO add warning:
            Hook.WarpLast();

            // Update UI button:
            btnRandomize.Content = isRandomized ? "Unrandomize!" : "Randomize!";
        }
    }
}
