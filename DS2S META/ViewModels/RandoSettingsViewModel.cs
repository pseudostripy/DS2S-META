using DS2S_META.Randomizer;
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

namespace DS2S_META.ViewModels
{
    using IPRSList = ObservableCollection<ItemRestriction>;

    public class RandoSettingsViewModel : ViewModelBase
    {
        public static List<string> PresetsComboItems { get; set; } = new() { "Default", "Custom", "Pain" };

        //private ObservableCollection<DS2SItem> _weaponList;
        //public ICollectionView? WeaponCollectionView { get; }

        //private ObservableCollection<DS2SInfusion> _infusionList;
        //public ICollectionView? InfusionCollectionView { get; set; }

        private string _teststring = "hello";
        public string TestString 
        { 
            get => _teststring;
            set 
            {
                _teststring = value;
                OnPropertyChanged();
            }
        
        }

        // Constructor
        public RandoSettingsViewModel()
        {
            //var wepCat = DS2SItemCategory.AllWeapons;
            //wepCat.Sort();
            //_weaponList = new ObservableCollection<DS2SItem>(wepCat); // alphabetical
            //_infusionList = new ObservableCollection<DS2SInfusion>(new List<DS2SInfusion>());

            //WeaponCollectionView = CollectionViewSource.GetDefaultView(_weaponList);
            //InfusionCollectionView = CollectionViewSource.GetDefaultView(_infusionList);

            //WeaponCollectionView.Filter += FilterWeapons;
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
            //if (!ParamMan.IsLoaded)
            //    return;

            //// Update the ones we care about:
            //lMod = WepSel?.WTypeRow?.lMod ?? 0;
            //rMod = WepSel?.WTypeRow?.rMod ?? 0;

            //// Calc scaling:
            //pScale = (int)Math.Floor(CalcScaling());
            //pBase = (int)Math.Floor(WepSel?.ReinforceRow?.GetPhysDmg(UpgradeVal) ?? 0);
            //OnPropertyChanged(nameof(pAR));

            //CounterDmg = WepSel?.CounterDmg ?? 0;
        }

        public string _preset = "Default";
        public string Preset
        {
            get => _preset;
            set
            {
                _preset = value;
                LoadPresetSettings();
                OnPropertyChanged(nameof(Preset));
            }
        }

        internal void LoadPresetSettings()
        {
            var temp = DefaultRestrictions();
            var ircheck = RandomizerSettings.ItemRestrictions;
            var test = AreFunctionallySame(temp, ircheck);
            var debug = 1;
        }

        public static IPRSList DefaultRestrictions()
        {
            IPRSList iprs = new() {
                new ItemRestriction("Estus Flask", ITEMID.ESTUS),
                new ItemRestriction("Ring of Binding", ITEMID.RINGOFBINDING),
                new ItemRestriction("Silvercat Ring", ITEMID.CATRING),
                new ItemRestriction("Aged Feather", ITEMID.AGEDFEATHER),
                new ItemRestriction("Dull Ember", ITEMID.DULLEMBER),
                new ItemRestriction("Rotunda Lockstone", ITEMID.ROTUNDALOCKSTONE),
                new ItemRestriction("King's Ring", ITEMID.KINGSRING),
                new ItemRestriction("Ashen Mist Heart", ITEMID.ASHENMIST),
                new ItemRestriction("Key to the Embedded", ITEMID.KEYTOEMBEDDED),

                new ItemRestriction("Any Blacksmith Key", ITEMGROUP.BlacksmithKey),
                new ItemRestriction("Any Pyromancy Flame", ITEMGROUP.Pyro),
                new ItemRestriction("Any Staff", ITEMGROUP.Staff),
                new ItemRestriction("Any Chime", ITEMGROUP.Chime),
            };
            return iprs;
        }
        private static bool AreFunctionallySame(IPRSList list1, IPRSList list2)
        {
            if (list1.Count != list2.Count) 
                return false;

            // check all elements. One day can override a proper equality comparer
            for (int i = 0; i < list1.Count; i++)
            {
                var ir1 = list1[i];
                var ir2 = list2[i];
                if (ir1.ItemID != ir2.ItemID ||
                    ir1.DistMax != ir2.DistMax ||
                    ir1.DistMin != ir2.DistMin ||
                    ir1.RestrType != ir2.RestrType)
                {
                    return false;
                }
            }
            return true;
        }


    }
}
