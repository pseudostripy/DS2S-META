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

namespace DS2S_META.ViewModels
{
    // Note: CheatsControl has CheatsViewModel data context set in MainWindow.xaml
    public class StatsViewModel : ViewModelBase
    {

        private PlayerDataHGO? PD => Hook?.DS2P?.PlayerData; // shorthand
        
        public bool EnGiveSouls => MetaFeature.FtGiveSouls;

        public ICommand MaxLevelsCommand { get; set; }



        public string CharacterName
        {
            get => PD?.CharacterName ?? string.Empty;
            set
            {
                if (PD?.CharacterName != null)
                    PD.CharacterName = value;
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

        

        

        // Constructor
        public StatsViewModel()
        {
            MaxLevelsCommand = new RelayCommand(MaxLevelsExecute, MaxLevelsCanExec);
        }
        private void MaxLevelsExecute(object? parameter) => Hook?.SetMaxLevels();
        private bool MaxLevelsCanExec(object? parameter) => Hook?.InGame == true && MetaFeature.FtNewTestCharacter;


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
            OnPropertyChanged(nameof(CharacterName));
            OnPropertyChanged(nameof(SelectedClass));
            UpdateLevels();
        }
        private void UpdateLevels()
        {
            // Refresh the whole list
            foreach (var lvlctrl in LvlAttrList)
                lvlctrl.UpdateViewModel();
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
        }

    }
}
