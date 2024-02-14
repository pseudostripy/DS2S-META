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
        private bool _chkBIKP1 = false;
        public bool ChkBIKP1
        {
            get => _chkBIKP1;
            set
            {
                bool forceLoad = true;
                var isModEnabled = Hook?.BIKP1Skip(value, forceLoad);  // request mod enablement toggle
                if (isModEnabled == null) return;           // not hooked
                _chkBIKP1 = (bool)isModEnabled;             // success
                OnPropertyChanged();                        // notify
            }
        }
        private bool _chkDealNoDmg = false;
        public bool ChkDealNoDmg
        {
            get => _chkDealNoDmg;
            set
            {
                var bworked = Hook?.GeneralizedDmgMod(false, value, _chkTakeNoDmg);
                if (bworked != true) 
                    return; // likely bug or inject failure. exit gracefully
                
                // Success, update properties and FE
                _chkDealNoDmg = value;
                _chkOHKO = false; // overruled
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChkOHKO));
            }
        }
        private bool _chkOHKO = false;
        public bool ChkOHKO
        {
            get => _chkOHKO;
            set
            {
                var bworked = Hook?.GeneralizedDmgMod(value, false, _chkTakeNoDmg);
                if (bworked != true)
                    return; // likely bug or inject failure. exit gracefully

                // Success, update properties and FE
                _chkOHKO = value;
                _chkDealNoDmg = false; // overruled
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChkDealNoDmg));
            }
        }
        private bool _chkTakeNoDmg = false;
        public bool ChkTakeNoDmg
        {
            get => _chkTakeNoDmg;
            set
            {
                var bworked = Hook?.GeneralizedDmgMod(_chkOHKO, _chkDealNoDmg, value);
                if (bworked != true)
                    return; // likely bug or inject failure. exit gracefully

                // Done. Update FE
                _chkTakeNoDmg = value;
                OnPropertyChanged();
            }
        }
        

        public bool EnGive17kReward => MetaFeature.IsActive(METAFEATURE.GIVE17KREWARD);
        public bool EnGive3Chunk1Slab => MetaFeature.IsActive(METAFEATURE.GIVE3CHUNK1SLAB);
        public bool EnMadWarrior => MetaFeature.IsActive(METAFEATURE.MADWARRIOR);
        public bool EnRubbishChallenge => MetaFeature.IsActive(METAFEATURE.RUBBISHCHALLENGE);
        public bool EnBIKP1Skip => MetaFeature.IsActive(METAFEATURE.BIKP1SKIP);
        public bool EnDmgMod => MetaFeature.IsActive(METAFEATURE.DMGMOD);

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

            
            // things that need to be reset on load:
            if (ChkBIKP1)
                Hook?.BIKP1Skip(true, false); // no inf load
        }
        public override void CleanupVM()
        {
            Hook?.UninstallDmgMod();
            Hook?.UninstallBIKP1Skip();
        }
        private void EnableElements()
        {
            OnPropertyChanged(nameof(EnGive17kReward));
            OnPropertyChanged(nameof(EnGive3Chunk1Slab));
            OnPropertyChanged(nameof(EnMadWarrior));
            OnPropertyChanged(nameof(EnRubbishChallenge));
            OnPropertyChanged(nameof(EnBIKP1Skip));
            OnPropertyChanged(nameof(EnDmgMod));
        }
    }
}
