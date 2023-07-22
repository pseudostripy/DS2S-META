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

namespace DS2S_META.ViewModels
{
    // Note: CheatsControl has CheatsViewModel data context set in MainWindow.xaml
    public class CheatsViewModel : ViewModelBase
    {
        public DS2SHook? Hook { get; set; }

        // Constructor
        public CheatsViewModel()
        {
        }

        // Properties
        public bool GameLoaded { get; set; }
        private bool _hookprev; // TODO consider event handler
        public const int MadWarriorChrID = 0x000CC1A0; // 836000d
        public bool HookValid => Hook != null && Hook.Hooked;

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
                if (!EnableMW) return "Not enabled"; // should be invisibile
                if (!IsSpawned) return "MISSING";
                return "SPAWNED";
            }
        }
        public Visibility lblSpawnVisibility => CheckedMW ? Visibility.Visible : Visibility.Hidden;
        private bool _enableMW = false;
        public bool EnableMW
        {
            get => _enableMW;
            set
            {
                _enableMW = value;
                OnPropertyChanged();
            }
        }
        private bool _checkedMW;
        public bool CheckedMW
        {
            get => _checkedMW;
            set
            {
                _checkedMW = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(lblSpawnVisibility));
                OnPropertyChanged(nameof(IsSpawned));
            }
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
            // Hook checks:
            if (HookValid != _hookprev)
                HookChangeEvent();
            _hookprev = HookValid; // update for next time
            
            if (!HookValid)
                return;

            // Confirmed OK for model reading:
            IsSpawned = Hook?.CheckLoadedEnemies(MadWarriorChrID) == true;
        }

        private void HookChangeEvent()
        {
            // Used to disable functionality on loss of Hook amongst other situations
            EnableMW = Hook?.IsFeatureCompatible(METAFEATURE.MADWARRIOR) == true;
        }


        public void InitViewModel(DS2SHook hook)
        {
            Hook = hook;
            _hookprev = hook != null && hook.Hooked;
            HookChangeEvent();
        }
    }
}
