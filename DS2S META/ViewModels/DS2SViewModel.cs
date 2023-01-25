using PropertyHook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DS2S_META.ViewModels
{
    internal class DS2SViewModel : ObservableObject
    {
        
        public DS2SHook Hook { get; private set; }
        public bool GameLoaded { get; set; }
        public bool Reading
        {
            get => DS2SHook.Reading;
            set => DS2SHook.Reading = value;
        }


        public static bool DesignMode
        {
            get { return DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()); }
        }

        public DS2SViewModel()
        {
            if (DesignMode)
                return; // maaaaybe fixes designer unhandled crash :thinking:?
                

            Hook = new DS2SHook(5000, 5000);
            Hook.OnHooked += Hook_OnHooked;
            Hook.OnUnhooked += Hook_OnUnhooked;
            Hook.Start();

            DmgCalcViewModel = new DmgCalcViewModel();
            ViewModels.Add(DmgCalcViewModel);
        }
        
        public Brush ForegroundID
        {
            get
            {
                if (Hook.ID != "Not Hooked")
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }
        public string ContentLoaded
        {
            get
            {
                if (Hook.Loaded)
                    return "Yes";
                return "No";
            }
        }
        public Brush ForegroundLoaded
        {
            get
            {
                if (Hook.Loaded)
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }
        public string ContentOnline
        {
            get
            {
                if (!Hook.Hooked)
                    return string.Empty;

                if (Hook.Online)
                    return "Yes";
                return "No";
            }
        }
        public Brush ForegroundOnline
        {
            get
            {
                if (!Hook.Hooked)
                    return Brushes.Black;

                if (Hook.Online)
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }
        public Brush ForegroundVersion
        {
            get
            {
                if (!Hook.Hooked)
                    return Brushes.Black;

                if (Hook.IsValidVer)
                    return Brushes.GreenYellow;
                return Brushes.IndianRed;
            }
        }

        ObservableCollection<ViewModelBase> ViewModels = new();
        public DmgCalcViewModel DmgCalcViewModel { get; set; }

        public void UpdateMainProperties()
        {
            OnPropertyChanged(nameof(ForegroundID));
            OnPropertyChanged(nameof(ContentLoaded));
            OnPropertyChanged(nameof(ForegroundLoaded));
            OnPropertyChanged(nameof(ContentOnline));
            OnPropertyChanged(nameof(ForegroundOnline));
            OnPropertyChanged(nameof(ForegroundVersion));
            OnPropertyChanged(nameof(GameLoaded));

            DmgCalcViewModel.UpdateViewModel();
        }

        private void Hook_OnHooked(object? sender, PHEventArgs e)
        {
        }

        private void Hook_OnUnhooked(object? sender, PHEventArgs e)
        {

        }
    }
}
