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

        // Constructor
        public DmgCalcViewModel()
        {
            _weaponList = new ObservableCollection<DS2SItem>(DS2SItemCategory.AllWeapons);
            WeaponCollectionView = CollectionViewSource.GetDefaultView(_weaponList);
            //WeaponCollectionView.Filter += FilterWeapons;
        }
        
        public WeaponRow? Wep { get; set; }

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
            lMod = Wep?.WTypeRow?.lMod ?? 0;
            rMod = Wep?.WTypeRow?.rMod ?? 0;
        }

        public DS2SHook? Hook { get; set; }
        public void InitViewModel(DS2SHook hook)
        {
            Hook = hook;
            SetWeapon = new SetWeaponCommand(this);
        }

        // Commands:
        public ICommand? SetWeapon { get; set; }

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

        private DS2SItem? _selectedItem;
        public DS2SItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetField(ref _selectedItem, value);
            }
        }

    }
}
