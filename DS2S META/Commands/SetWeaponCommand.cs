using DS2S_META.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Primitives;

namespace DS2S_META.ViewModels.Commands
{
    public class SetWeaponCommand : ICommand
    {
        private DmgCalcViewModel _dcvm;
        public DmgCalcViewModel DCVM
        {
            get => _dcvm;
            private set => _dcvm = value;
        }
        public SetWeaponCommand(DmgCalcViewModel dcvm)
        {
            _dcvm = dcvm;
        }
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        // Do the actual stuff?
        public void Execute(object? parameter)
        {
            //var ds2sitem = parameter as DS2SItem;
            //if (ds2sitem == null) return;

            ////var inf = cmbInfusion.SelectedItem as DS2SInfusion;
            ////if (item == null) return;
            //var inf = DS2SInfusion.Infusions[0]; // todo

            ////var upgr = nudUpgrade.Value;
            ////if (upgr == null) return;
            //var upgr = 0; // todo

            DCVM.Wep = ParamMan.GetWeaponFromID(DCVM.SelectedItem?.itemID);
        }
    }
}
