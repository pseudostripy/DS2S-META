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
        public bool EnWarp => Hook?.InGame == true;

        private bool _chkNoDeath = false;
        public bool ChkNoDeath {
            get => _chkNoDeath;
            set {
                _chkNoDeath = value;
                if (Hook != null)
                    if (value)
                        Hook.SetNoDeath();
                    else
                        Hook.SetYesDeath();
            }
        }
        private bool _chkDisableAi = false;
        public bool ChkDisableAi { 
            get => _chkDisableAi;
            set 
            {
                _chkDisableAi = value;
                // update game AI when we toggle checkbox but note that it can update itself too and desync from this flag (i.e. after load)
                if (Hook != null)
                    Hook.DisableAI = (byte)(value ? 1 : 0); 
                OnPropertyChanged(nameof(ChkDisableAi));
            }
        }

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
            if (Hook == null)
                return;
            EnableElements(); // refresh UI element enables

            // things that need to be reset on load:
            if (ChkNoDeath)
                Hook.SetNoDeath();
            if (ChkDisableAi)
                Hook.DisableAI = 1;
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
