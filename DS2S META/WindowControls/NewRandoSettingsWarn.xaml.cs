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
    /// Check with user whether OK to procedd
    /// </summary>
    public partial class NewRandoSettingsWarn: Window
    {
        public bool IsOk = false;
        
        public NewRandoSettingsWarn()
        {
            InitializeComponent();
            Title = "New Settings Detected";
            Width = 370;
            Height = 190;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsOk)
                Properties.Settings.Default.ShowWarnSttgChg = cbxNoShowWarning.IsChecked == false;
        }

        private void btnOkay_Click(object sender, RoutedEventArgs e)
        {
            IsOk = true;
            Window.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }
    }
}
