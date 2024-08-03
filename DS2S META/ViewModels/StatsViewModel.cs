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
using DS2S_META.Utils.DS2Hook;
using DS2S_META.Commands;
using System.Windows.Navigation;

namespace DS2S_META.ViewModels
{
    // Note: CheatsControl has CheatsViewModel data context set in MainWindow.xaml
    public class StatsViewModel : ViewModelBase
    {

        private PlayerDataHGO? PD => Hook?.DS2P?.PlayerData; // shorthand
        
        public bool EnGiveSouls => MetaFeature.FtGiveSouls;
        public bool EnResetSoulMemory => MetaFeature.FtGiveSouls;
        public bool EnMaxLevels => MetaFeature.FtMaxLevels;
        public bool EnClassLevelReset => MetaFeature.FtResetToClassLevels;
        
        public ICommand MaxLevelsCommand { get; set; }
        public ICommand ResetLevelsCommand { get; set; }
        public ICommand ResetSoulMemoryCommand { get; set; }
        public ICommand GiveSoulsCommand { get; set; }

        // Binding properties:
        private string _characterName = string.Empty;
        public string CharacterName
        {
            get => PD?.CharacterName ?? string.Empty;
            set
            {
                if (PD?.CharacterName != null)
                    PD.CharacterName = value;
                _characterName = value;
                OnPropertyChanged();
            }
        }
        public static List<DS2SClass> ClassList => DS2Resource.Classes;
        public DS2SClass? SelectedClass
        {
            get
            {
                var hookClass = PD?.Class;
                if (hookClass == null) return null;
                return DS2Resource.GetClassById((PLAYERCLASS)hookClass);
            }
            set
            {
                if (PD?.Class == null) return;
                PD.Class = value?.ID;
                OnPropertyChanged();
            }
        }
        public int SoulLevel
        {
            get => PD?.SoulLevel ?? 0;
            set { return; }
        }
        public int SinnerLevel
        {
            get => PD?.SinnerLevel ?? 0;
            set 
            {
                if (PD?.SinnerLevel != null)
                    PD.SinnerLevel = value;
            }
        }
        public int SinnerPoints
        {
            get => PD?.SinnerPoints ?? 0;
            set
            {
                if (PD?.SinnerPoints != null)
                    PD.SinnerPoints = value;
            }
        }
        public int HollowLevel
        {
            get => PD?.HollowLevel ?? 0;
            set
            {
                if (PD?.HollowLevel != null)
                    PD.HollowLevel = value;
            }
        }
        private int _giveSoulsVal = 0;
        public int GiveSoulsVal
        {
            get => _giveSoulsVal;
            set
            {
                _giveSoulsVal |= value;
                OnPropertyChanged();
            }
        }
        public int Souls
        {
            get => PD?.Souls ?? 0;
            set { return; }
        }
        public int SoulMemory
        {
            get => PD?.SoulMemory ?? 0;
            set { return; }
        }

        // Constructor
        public StatsViewModel()
        {
            MaxLevelsCommand = new RelayCommand(MaxLevelsExecute, MaxLevelsCanExec);
            ResetLevelsCommand = new RelayCommand(ResetLevelsExecute, ResetLevelsCanExec);
            ResetSoulMemoryCommand = new RelayCommand(ResetSoulMemoryExecute, ResetSoulMemoryCanExec);
            GiveSoulsCommand = new RelayCommand(GiveSoulsExecute, GiveSoulsCanExec);
        }
        private void MaxLevelsExecute(object? parameter) => Hook?.SetMaxLevels();
        private void ResetLevelsExecute(object? parameter) => ResetToClassLevels();
        private void ResetSoulMemoryExecute(object? parameter) => ResetToClassLevels();
        private void GiveSoulsExecute(object? parameter) => Hook?.AddSouls(GiveSoulsVal);
        
        private bool MaxLevelsCanExec(object? parameter) => MetaFeature.FtMaxLevels;
        private bool ResetLevelsCanExec(object? parameter) => MetaFeature.FtResetToClassLevels;
        private bool ResetSoulMemoryCanExec(object? parameter) => MetaFeature.FtResetSoulMemory;
        private bool GiveSoulsCanExec(object? parameter) => MetaFeature.FtGiveSouls;

        public void ResetToClassLevels()
        {
            var hookClass = SelectedClass;
            if (hookClass == null) return;

            foreach (ATTR attr in Enum.GetValues(typeof(ATTR)))
                Hook?.DS2P.PlayerData.SetAttributeLevel(attr, hookClass.ClassMinLevels[attr]);
        }

        public List<AttrLvlDataVM> LvlAttrList { get; set; } = SetupAttrControlList();
        private static List<AttrLvlDataVM> SetupAttrControlList()
        {
            List<AttrLvlDataVM> attrControls = new();
            foreach (var attr in Enum.GetValues<ATTR>())
                attrControls.Add(new AttrLvlDataVM(attr));
            return attrControls;
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
            OnPropertyChanged(nameof(Souls));
            OnPropertyChanged(nameof(SoulMemory));
            OnPropertyChanged(nameof(HollowLevel));
            OnPropertyChanged(nameof(SinnerLevel));
            OnPropertyChanged(nameof(SinnerPoints));
            CheckNameChange();
            OnPropertyChanged(nameof(SelectedClass));
            UpdateLevels();
            OnPropertyChanged(nameof(SoulLevel));
        }
        private void UpdateLevels()
        {
            // Refresh the whole list
            foreach (var lvlctrl in LvlAttrList)
                lvlctrl.UpdateViewModel();
        }
        private void CheckNameChange()
        {
            var charName = PD?.CharacterName;
            if (charName == null) return; // hook not setup yet
            if (charName == _characterName) return; // no change 
            CharacterName = charName;
        }
        private void UpdateLevelMinimums()
        {
            // Refresh the whole list
            foreach (var lvlctrl in LvlAttrList)
                lvlctrl.UpdateNewClassData();
        }

        public override void OnHooked()
        {
            EnableElements();

            // can probably do this in the view somehow, but im bad at frontend...
            // this is complicated enough already
            foreach (var lvlctrl in LvlAttrList)
                lvlctrl.Hook = Hook;
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
            OnPropertyChanged(nameof(EnResetSoulMemory));
        }

    }
}
