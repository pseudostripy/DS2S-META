using DS2S_META.Utils;
using DS2S_META.ViewModels.Commands;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DS2S_META.Randomizer;
using DS2S_META.Utils.Offsets;

namespace DS2S_META.ViewModels
{
    // Note: CheatsControl has CheatsViewModel data context set in MainWindow.xaml
    public class SettingsViewModel : ViewModelBase
    {
        // Constructor
        public SettingsViewModel()
        {
        }

        // Properties
        public string RivaXPixel
        {
            get => $"{Properties.Settings.Default.RivaXPixels}";
            set
            {
                string s = value.ToString();
                bool isInt = int.TryParse(s, out int xpx);
                if (!isInt) return; // do nothing
                Properties.Settings.Default.RivaXPixels = xpx;
                RefreshRivaOverlay();
            }
        }
        public string RivaYPixel
        {
            get => $"{Properties.Settings.Default.RivaYPixels}";
            set
            {
                string s = value.ToString();
                bool isInt = int.TryParse(s, out int ypx);
                if (!isInt) return; // do nothing
                Properties.Settings.Default.RivaYPixels = ypx;
                RefreshRivaOverlay();
            }
        }
        public string RivaTextSize
        {
            get => $"{Properties.Settings.Default.RivaTextSize}";
            set
            {
                string s = value.ToString();
                bool isInt = int.TryParse(s, out int sz);
                if (!isInt) return; // do nothing
                Properties.Settings.Default.RivaTextSize = sz;
                RefreshRivaOverlay();
            }
        }

        private void RefreshRivaOverlay()
        {
            if (Hook?.Hooked == true)
                RivaHook.Refresh();
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
        }

        internal void OnHooked()
        {
        }
        internal void OnUnHooked()
        {
        }
        private void EnableElements()
        {
        }
    }
}
