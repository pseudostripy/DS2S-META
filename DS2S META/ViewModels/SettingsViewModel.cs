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
        // This impacts a wide variety of things that might need events triggered
        private readonly DS2SViewModel VMParent;

        // Constructor
        public SettingsViewModel(DS2SViewModel parent)
        {
            VMParent = parent;
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
                //RefreshRivaOverlay(); // deprecated
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
                //RefreshRivaOverlay(); // deprecated
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
                //RefreshRivaOverlay(); // deprecated
            }
        }

        // Extra state here because impacts (overrides) PlayerViewModel
        //private bool _chkAlwaysRestOnWarp = Properties.Settings.Default.AlwaysRestAfterWarp;
        public bool ChkAlwaysRestOnWarp
        {
            get => Properties.Settings.Default.AlwaysRestAfterWarp;
            set
            {
                // Update myself
                Properties.Settings.Default.AlwaysRestAfterWarp = value;
                OnPropertyChanged();
                
                // Reset other things for somewhat consistency (and call their onPropertyEvents)
                VMParent.PlayerViewModel.ChkWarpRest = value;
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
