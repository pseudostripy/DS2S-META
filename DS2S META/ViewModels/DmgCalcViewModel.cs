using DS2S_META.Utils;
using DS2S_META.Utils.Offsets.HookGroupObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using DS2S_META.DataClassHelpers.Commands;

namespace DS2S_META.ViewModels
{
    public class DmgCalcViewModel : ViewModelBase
    {
        private ObservableCollection<DS2SItem> _weaponList;
        public ICollectionView? WeaponCollectionView { get; }

        private ObservableCollection<DS2SInfusion> _infusionList;
        public ICollectionView? InfusionCollectionView { get; set; }

        private ScalingBonusHGO? ScalingBonusHGO => Hook?.DS2P?.ScalingBonusHGO;

        // Constructor
        public DmgCalcViewModel()
        {
            var wepCat = DS2Resource.Weapons;
            wepCat.Sort();
            _weaponList = new ObservableCollection<DS2SItem>(wepCat); // alphabetical
            _infusionList = new ObservableCollection<DS2SInfusion>(new List<DS2SInfusion>());

            WeaponCollectionView = CollectionViewSource.GetDefaultView(_weaponList);
            InfusionCollectionView = CollectionViewSource.GetDefaultView(_infusionList);

            WeaponCollectionView.Filter += FilterWeapons;
            SetWeapon = new SetWeaponCommand(this);
        }
        
        public WeaponRow? WepSel { get; set; }

        public string hModString
        {
            get
            {
                string strhand = LeftHandSelected ? "lMod" : "rMod";
                return $"hMod ({strhand})";
            }
        }
        public string SetWepLabel
        {
            get
            {
                if (SelectedItem == null) 
                    return "UNSELECTED WEAPON";
                return $"{SelectedInfusion} {SelectedItem?.Name} +{UpgradeVal}";
            }
        }
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
            WepSel = ParamMan.GetWeaponFromID(SelectedItem?.ItemId);
            NudUpgrMax = WepSel?.MaxUpgrade ?? 0;

            var inflist = WepSel?.GetInfusionList();
            if (inflist == null) return;
            _infusionList = new ObservableCollection<DS2SInfusion>(inflist);
            InfusionCollectionView = CollectionViewSource.GetDefaultView(_infusionList);
            OnPropertyChanged(nameof(InfusionCollectionView));
            OnPropertyChanged(nameof(SetWepLabel));
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
            LMod = WepSel?.WTypeRow?.LMod ?? 0;
            RMod = WepSel?.WTypeRow?.RMod ?? 0;

            // Calc scaling:
            pScale = (int)Math.Floor( CalcScaling() );
            pBase = (int)Math.Floor( WepSel?.ReinforceRow?.GetPhysDmg(UpgradeVal) ?? 0 );
            OnPropertyChanged(nameof(pAR));

            CounterDmg = WepSel?.CounterDamage ?? 0;
        }

        public float CalcScaling()
        {
            // Always do str/dex:
            var strbonus = ScalingBonusHGO?.GetBonus(BNSTYPE.STR);
            var strsf = WepSel?.ReinforceRow?.WeaponStatsAffectRow?.ReadScalingValue(WeaponStatsAffectRow.SCTYPE.STR, UpgradeVal); // scale factor

            var dexbonus = ScalingBonusHGO?.GetBonus(BNSTYPE.DEX);
            var dexsf = WepSel?.ReinforceRow?.WeaponStatsAffectRow?.ReadScalingValue(WeaponStatsAffectRow.SCTYPE.DEX, UpgradeVal);

            var scaling = strsf * strbonus + dexsf * dexbonus;
            if (scaling == null) return 0;
            return (float)scaling;
        }

        // Commands:
        public ICommand? SetWeapon { get; private set; }

        // Properties:
        private float _lMod = 0;
        public float LMod
        {
            get => _lMod;
            set
            {
                if (SetField(ref _lMod, value))
                    OnPropertyChanged(nameof(hMod));
            }
        }
        private float _rMod = 0;
        public float RMod
        {
            get => _rMod;
            set
            {
                if (SetField(ref _rMod, value))
                    OnPropertyChanged(nameof(hMod));
            }
        }
        private int _pBase = 0;
        public int pBase
        {
            get => _pBase;
            set => SetField(ref _pBase, value);
        }
        private int _pScale = 0;
        public int pScale
        {
            get => _pScale;
            set => SetField(ref _pScale, value);
        }
        public short _counterDmg = 0;
        public short CounterDmg
        {
            get => _counterDmg;
            set => SetField(ref _counterDmg, value);
        }

        public int pAR => pBase + pScale;

        public float hMod { 
            get
            {
                if (LeftHandSelected)
                    return LMod;
                return RMod;
            } 
        }

        private bool _leftHandSelected = false;
        public bool LeftHandSelected 
        {
            get => _leftHandSelected;
            set
            {
                _leftHandSelected = value;
                OnPropertyChanged(nameof(hModString));
                OnPropertyChanged(nameof(hMod));
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
            set
            {
                _selectedInfusion = value;
                OnPropertyChanged(nameof(SetWepLabel));
            }
        }

        private int _upgradeval = 0;
        public int UpgradeVal {
            get => _upgradeval;
            set
            {
                _upgradeval = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SetWepLabel));
            }
        }

    }
}
