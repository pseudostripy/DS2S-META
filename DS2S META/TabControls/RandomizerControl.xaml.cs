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

        
        private void cbSlabIt_Checked(object sender, RoutedEventArgs e)
        {
            // Want to try to Hook in DS2 to change the wooden chest above cardinal tower to metal chest items:
            if (!Hook.Hooked)
            {
                cbSlabIt.IsChecked = false;
                return;
            }

            // Some IDs to play with:
            var metalchestPID = 10105070;
            var lightningurnID = 60560000;
            var dungpieID = 60595000;
            var sunlightmedalID = 62120000;

            // make a fake itemlot for testing:
            ItemLot testlot = new ItemLot();
            testlot.AddDrop(lightningurnID, 18);
            testlot.AddDrop(dungpieID, 40);
            testlot.AddDrop(sunlightmedalID);

            // Rewrite the parameters in the itemlot table
            //Hook.WriteItemLotTable(metalchestPID, testlot);
            
            randomize();

            txtOutput.Text = $"Success?";
        }

        private void randomize()
        {
            // Need to get a list of the vanilla item lots
            Dictionary<int,ItemLot> VanillaLots = Hook.GetVanillaLots();
            Dictionary<int, ItemLot> ShuffledLots = new Dictionary<int, ItemLot>();

            // Make into flat list of stuff:
            var flatlist = VanillaLots.SelectMany(kvp => kvp.Value.Lot).ToList();

            
            flatlist.Shuffle();

            // Make into new lots:
            foreach (var kvp in VanillaLots)
            {
                int Ndrops = kvp.Value.Lot.Count();

                ItemLot IL = new ItemLot();
                for (int row = 0; row < Ndrops; row++)
                {
                    IL.AddDrop(flatlist[0]);
                    flatlist.RemoveAt(0); // pop
                }
                ShuffledLots[kvp.Key] = IL; // add to new dictionary
            }

            
            // Implement randomness:
            foreach(var kvp in ShuffledLots)
                Hook.WriteItemLotTable(kvp.Key, kvp.Value);
            
            int debug = 1;
        }

        private void cbSlabIt_Unchecked(object sender, RoutedEventArgs e)
        {
            txtOutput.Text = "Output";
        }

        

    }
}
