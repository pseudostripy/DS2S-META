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
using System.Diagnostics;

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for METAWarning.xaml
    /// </summary>
    public partial class METAUpdate : Window
    {
        private Uri Link;
        public METAUpdate(Uri link)
        {
            InitializeComponent();
            Link = link;

            //Run linkrun = new Run(link.ToString());
            //var hyper = new Hyperlink(linkrun);
            //hyper.NavigateUri = link;
            ////hyper.Name = "link";
            //lblNewVersion.Content = hyper;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.ShowUpdateMessage = !cbxShowUpdateNotification.IsChecked.Value;
        }

        private void link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
    }
}
