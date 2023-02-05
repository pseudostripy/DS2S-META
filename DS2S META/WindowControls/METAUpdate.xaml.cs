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
using DS2S_META.Utils;
using PropertyHook;

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for METAWarning.xaml
    /// </summary>
    public partial class METAUpdate : Window
    {
        private readonly MetaVersionInfo MVI ;
        public METAUpdate(MetaVersionInfo mvi)
        {
            InitializeComponent();
            MVI = mvi;
            if (MVI.LatestReleaseURI == null)
                return;

            // Create hyperlink object dynamically
            Run runtext = new(MVI.LatestReleaseURI.ToString());
            var hyperobj = new Hyperlink(runtext)
            {
                NavigateUri = MVI.LatestReleaseURI
            };
            hyperobj.RequestNavigate += link_RequestNavigate;

            // Update UI
            lblNewVersion.Content = hyperobj;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cbxStopUpdateNotification.IsChecked == true)
                Properties.Settings.Default.AcknowledgeUpdateVersion = MVI.GitVersionStr;
        }

        private void link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = e.Uri.ToString(), UseShellExecute = true });
        }

        private void btnUpdater_Click(object sender, RoutedEventArgs e)
        {
            DoUpdate();
        }
        private void DoUpdate()
        {
            _ = Updater.WrapperInitiateUpdate(MVI);
            Close();
        }
    }
}
