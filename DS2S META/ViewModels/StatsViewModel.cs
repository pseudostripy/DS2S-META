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
using DS2S_META.Utils.Offsets.HookGroupObjects;

namespace DS2S_META.ViewModels
{
    // Note: CheatsControl has CheatsViewModel data context set in MainWindow.xaml
    public class StatsViewModel : ViewModelBase
    {

        private PlayerDataHGO? PD => Hook?.DS2P?.PlayerData; // shorthand
        public bool EnGiveSouls => MetaFeature.FtGiveSouls;

        public string CharacterName
        {
            get => "test";  //PD?.CharacterName ?? string.Empty;
            set
            {
                return;
                //if (PD?.CharacterName != null)
                //    PD.CharacterName = value;
                //OnPropertyChanged();
            }
        }


        // Constructor
        public StatsViewModel()
        {
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
            //EnableElements();

        }

        public override void OnHooked()
        {
            EnableElements();
        }
        public override void OnUnHooked()
        {
            EnableElements();
        }
        internal void OnInGame()
        {
            // called upon transition from load-screen or main-menu to in-game
            if (Hook == null) return;
            EnableElements(); // refresh UI element enables
        }
        internal void OnMainMenu()
        {
            EnableElements(); // disable stuff that requires InGame
        }

        private void EnableElements() 
        {
            OnPropertyChanged(nameof(EnGiveSouls));
        }

    }
}
