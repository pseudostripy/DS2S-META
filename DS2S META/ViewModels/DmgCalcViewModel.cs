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
using System.Windows.Data;
using System.Windows.Input;

namespace DS2S_META.ViewModels
{
    public class DmgCalcViewModel : ViewModelBase
    {
        private ObservableCollection<DS2SItem> _weaponList;
        public ICollectionView? WeaponCollectionView { get; }

        private ObservableCollection<DS2SInfusion> _infusionList;
        public ICollectionView? InfusionCollectionView { get; set; }

        // Constructor
        public DmgCalcViewModel()
        {
            _weaponList = new ObservableCollection<DS2SItem>(DS2SItemCategory.AllWeapons);
            _infusionList = new ObservableCollection<DS2SInfusion>(new List<DS2SInfusion>());

            WeaponCollectionView = CollectionViewSource.GetDefaultView(_weaponList);
            InfusionCollectionView = CollectionViewSource.GetDefaultView(_infusionList);

            WeaponCollectionView.Filter += FilterWeapons;
        }
        
        public WeaponRow? WepStore { get; set; }
        public WeaponRow? WepSel { get; set; }

        public int UpgrSel { get; set; }
        public int UpgrStore { get; set; }
        public DS2SInfusion InfusionStore { get; set; }

        private int _nudUpgrMax = 5;
        public int NudUpgrMax {
            get => _nudUpgrMax;
            set
            {
                _nudUpgrMax = value;
                OnPropertyChanged(nameof(NudUpgrMax));
            }
        }

        private void UpdateWepStats()
        {
            WepSel = ParamMan.GetWeaponFromID(SelectedItem?.itemID);
            NudUpgrMax = WepSel?.MaxUpgrade ?? 0;

            var inflist = WepSel?.GetInfusionList();
            if (inflist == null) return;
            _infusionList = new ObservableCollection<DS2SInfusion>(inflist);
            InfusionCollectionView = CollectionViewSource.GetDefaultView(_infusionList);
            OnPropertyChanged(nameof(InfusionCollectionView));
        }

        private bool FilterWeapons(object obj)
        {
            return true; // todo?
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
            if (!ParamMan.IsLoaded)
                return;

            // Update the ones we care about:
            lMod = WepStore?.WTypeRow?.lMod ?? 0;
            rMod = WepStore?.WTypeRow?.rMod ?? 0;
        }

        public DS2SHook? Hook { get; set; }
        public void InitViewModel(DS2SHook hook)
        {
            Hook = hook;
            SetWeapon = new SetWeaponCommand(this);
        }

        // Commands:
        public ICommand? SetWeapon { get; private set; }

        // Properties:
        private float _lMod = 0;
        public float lMod
        {
            get => _lMod;
            set
            {
                if (SetField(ref _lMod, value))
                    OnPropertyChanged(nameof(lMod));
            }
        }
        private float _rMod = 0;
        public float rMod
        {
            get => _rMod;
            set
            {
                if (SetField(ref _rMod, value))
                    OnPropertyChanged(nameof(rMod));
            }
        }

        public float hMod { 
            get
            {
                if (LeftHandSelected)
                    return lMod;
                return rMod;
            } 
        }

        private bool _leftHandSelected = false;
        public bool LeftHandSelected 
        {
            get => _leftHandSelected;
            set
            {
                _leftHandSelected = value;
                OnPropertyChanged(nameof(LeftHandSelected));
            }
        }

        // UI properties:
        private DS2SItem? _selectedItem;
        public DS2SItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetField(ref _selectedItem, value);
                OnPropertyChanged();
                UpdateWepStats();
            }
        }

        public DS2SInfusion _selectedInfusion;
        public DS2SInfusion SelectedInfusion
        {
            get => _selectedInfusion;
            set => _selectedInfusion = value;
        }

        private int _upgradeval = 0;
        public int UpgradeVal {
            get => _upgradeval;
            set
            {
                _upgradeval = value;
                OnPropertyChanged();
            }
        }

    }
}
