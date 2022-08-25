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

            // Test getting some parameter values with the Hook:
            var metalchestPID = 10105070;
            var testoffset = Hook.GetItemLotOtherOffset(metalchestPID);

            txtOutput.Text = $"Offset = {testoffset}";
        }
    }
}
