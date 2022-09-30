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
        private string CurrentMetaVersion;
        public METAUpdate(Uri link, string curver)
        {
            InitializeComponent();
            Link = link;
            CurrentMetaVersion = curver;

            // Create hyperlink object dynamically
            Run runtext = new Run($"{Link}");
            var hyperobj = new Hyperlink(runtext);
            hyperobj.NavigateUri = Link;
            hyperobj.RequestNavigate += link_RequestNavigate;

            // Update UI
            lblNewVersion.Content = hyperobj;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!cbxStopUpdateNotification.IsChecked == true)
                Properties.Settings.Default.AcknowledgeUpdateVersion = CurrentMetaVersion;
        }

        private void link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = e.Uri.ToString(), UseShellExecute = true });
        }
    }
}
