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
    public partial class MetaInfoWindow : Window
    {
        public MetaInfoWindow(string infostr)
        {
            InitializeComponent();
            tbError.Text = infostr;
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void btnOkay_Click(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }

        public static void ShowMetaInfo(string infostr)
        {
            Application.Current.Dispatcher.Invoke(() => ShowMetaInfoWindow(infostr));
        }
        private static void ShowMetaInfoWindow(string infostr)
        {
            var miWindow = new MetaInfoWindow(infostr);
            miWindow.ShowDialog();
        }
    }
}
