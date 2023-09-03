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

namespace DS2S_META.ViewModels
{
    // Note: CheatsControl has CheatsViewModel data context set in MainWindow.xaml
    public class PlayerViewModel : ViewModelBase
    {
        // Binding Variables:
        public bool EnNoDeath => Hook.InGameAndFeature(METAFEATURE.NODEATH);
        public bool EnRapierOHKO => Hook.InGameAndFeature(METAFEATURE.OHKO_RAPIER);
        public bool EnFistOHKO => Hook.InGameAndFeature(METAFEATURE.OHKO_FIST);
        public bool EnSpeedhack => Hook?.Hooked == true;
        public bool EnGravity => Hook?.InGame == true;
        public bool EnCollision => Hook?.InGame == true;
        public bool EnDisableAi => Hook.InGameAndFeature(METAFEATURE.DISABLEAI);
        public bool EnStorePosition => Hook?.InGame == true;
        public bool EnRestorePosition => Hook?.InGame == true;

        

        // Constructor
        public PlayerViewModel()
        {
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
            //EnableElements();
        }

        internal void OnHooked()
        {
            EnableElements();
        }
        internal void OnUnHooked()
        {
            EnableElements();
        }
        internal void OnInGame()
        {
            // called upon transition from load-screen or main-menu to in-game
            EnableElements();
        }
        internal void OnMainMenu()
        {
            EnableElements(); // disable stuff that requires InGame
        }

        private void EnableElements() 
        {
            OnPropertyChanged(nameof(EnNoDeath));
            OnPropertyChanged(nameof(EnRapierOHKO));
            OnPropertyChanged(nameof(EnFistOHKO));
            OnPropertyChanged(nameof(EnSpeedhack));
            OnPropertyChanged(nameof(EnGravity));
            OnPropertyChanged(nameof(EnCollision));
            OnPropertyChanged(nameof(EnDisableAi));
            OnPropertyChanged(nameof(EnStorePosition));
            OnPropertyChanged(nameof(EnRestorePosition));
        }

    }
}
