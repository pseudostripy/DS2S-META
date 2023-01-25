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
    internal class TestVM : ObservableObject
    {
        
        public DS2SHook? Hook { get; private set; }
        public bool GameLoaded { get; set; }
        public bool Reading
        {
            get => DS2SHook.Reading;
            set => DS2SHook.Reading = value;
        }
        ObservableCollection<ViewModelBase> ViewModels = new();
        public DmgCalcViewModel DmgCalcViewModel { get; set; }

        public TestVM()
        {
            Hook = new DS2SHook(5000, 5000);
            Hook.OnHooked += Hook_OnHooked;
            Hook.OnUnhooked += Hook_OnUnhooked;

            DmgCalcViewModel = new DmgCalcViewModel();
            ViewModels.Add(DmgCalcViewModel);
        }

        private void Hook_OnHooked(object? sender, PHEventArgs e)
        {
        }

        private void Hook_OnUnhooked(object? sender, PHEventArgs e)
        {
        }

    }
}
