using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DS2S_META.Utils;
using DS2S_META.Utils.ParamRows;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// This class provides easier access to the shop substructure fields
    /// of ShopLineupParam
    /// </summary>
    internal class ChrRow : Param.Row
    {
        // Behind-fields
        private int _itemlotid;
        
        // Properties
        internal int ItemLotID 
        { 
            get => _itemlotid;
            set
            {
                _itemlotid = value;
                WriteAtField(0, BitConverter.GetBytes(value));
            }
        }

        // Linked Params:
        internal ItemDropRow? ItemLot => ParamMan.GetLink<ItemDropRow>(ParamMan.ItemLotChrParam, ItemLotID);

        // Constructors:
        public ChrRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            // Initial field setting:
            ItemLotID = (int)ReadAtFieldNum(90);
        }

        // Methods:
    }
}
