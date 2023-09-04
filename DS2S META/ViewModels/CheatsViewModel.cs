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
    public class CheatsViewModel : ViewModelBase
    {
        // Constants
        public const int MadWarriorChrID = 0x000CC1A0; // 836000d

        // Constructor
        public CheatsViewModel()
        {
        }

        // MVVM Properties
        public Brush MWSpawnColor => IsSpawned ? Brushes.Green : Brushes.Red;
        private bool _isSpawned;
        public bool IsSpawned
        {
            get => _isSpawned;
            set
            {
                _isSpawned = value;
                OnPropertyChanged(nameof(LblSpawnedTxt));
                OnPropertyChanged(nameof(MWSpawnColor));
            }
        }    
        public string LblSpawnedTxt
        {
            get
            {
                if (!ChkMadWarrior) return "Not enabled"; // should be invisible
                if (!IsSpawned) return "MISSING";
                return "SPAWNED";
            }
        }
        public Visibility lblSpawnVisibility => ChkMadWarrior ? Visibility.Visible : Visibility.Hidden;
        private bool _chkMadWarrior;
        public bool ChkMadWarrior // isChecked
        {
            get => _chkMadWarrior;
            set
            {
                _chkMadWarrior = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(lblSpawnVisibility));
                OnPropertyChanged(nameof(IsSpawned));
            }
        }
        public bool EnGive17kReward => Hook.InGameAndFeature(METAFEATURE.GIVE17KREWARD);
        public bool EnGive3Chunk1Slab => Hook.InGameAndFeature(METAFEATURE.GIVE3CHUNK1SLAB);
        public bool EnMadWarrior => Hook.InGameAndFeature(METAFEATURE.MADWARRIOR);
        public bool EnRubbishChallenge => Hook.CheckFeature(METAFEATURE.RUBBISHCHALLENGE);

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
            // Check version features:
            if (EnMadWarrior && ChkMadWarrior) 
                IsSpawned = Hook?.CheckLoadedEnemies(CHRID.MADWARRIOR) == true; // Confirmed OK for model reading:
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
            EnableElements(); // refresh UI elements
        }
        private void EnableElements()
        {
            OnPropertyChanged(nameof(EnGive17kReward));
            OnPropertyChanged(nameof(EnGive3Chunk1Slab));
            OnPropertyChanged(nameof(EnMadWarrior));
            OnPropertyChanged(nameof(EnRubbishChallenge));
        }
    }
}
