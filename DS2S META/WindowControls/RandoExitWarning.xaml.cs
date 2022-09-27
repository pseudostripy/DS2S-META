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
    /// Interaction logic for RandoWarpWarning.xaml
    /// </summary>
    public partial class RandoExitWarning : Window
    {
        public bool IsOk = false;
        
        public RandoExitWarning()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.ShowWarnRandoExit = cbxNoShowRandoExitWarning.IsChecked == false;
        }
    }
}
