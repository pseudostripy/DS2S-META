using DS2S_META.Randomizer;
using DS2S_META.Utils.ParamRows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    /// <summary>
    /// Data Class for storing Weapons
    /// </summary>
    public class GeneratorParamRow : Param.Row
    {
        // Behind-fields:
        private int IndItemLotId { get; set; }
        private int IndGenRegist { get; set; }
        private int _itemlotID;
        private int _genRegistID;
        
        // Properties:
        public int ItemLotID
        {
            get => _itemlotID;
            set
            {
                _itemlotID = value;
                WriteAt(IndItemLotId, BitConverter.GetBytes(value));
            }
        }
        public int GeneratorRegistID
        {
            get => _genRegistID;
            set
            {
                _genRegistID = value;
                WriteAt(IndGenRegist, BitConverter.GetBytes(value));
            }
        }
        
        // Linked param:
        internal ItemLotRow? DirectItemLot => ParamMan.GetLink<ItemLotRow>(ParamMan.ItemLotChrParam, ItemLotID);
        internal GeneratorRegistRow? GeneratorRegist => ParamMan.GetGenRegistLink<GeneratorRegistRow>(GeneratorRegistID, Param.Name);

        // The GeneratorParamRow can either supply an ItemLotId directly using the
        // ItemLot field, or indirectly via providing a GenRegistParamId which in
        // turn defines an Enemy who has an ItemLotId. We're interested in both.
        public ItemLotBaseRow? IndirectItemLot => GeneratorRegist?.Enemy?.ItemLot;
        public bool IsDirectItemLot => ItemLotID == 0; // seems to be the main indicator
        public ItemLotBaseRow? ItemLot => IsDirectItemLot ? DirectItemLot : IndirectItemLot;


        // Constructor:
        public GeneratorParamRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            SetupIndices();
            
            // Initialise Values:
            ItemLotID = (int)ReadAt(IndItemLotId);
            GeneratorRegistID = (int)ReadAt(IndGenRegist);
        }
        private void SetupIndices()
        {
            IndGenRegist = GetFieldIndex("GeneratorRegistParam");
            IndItemLotId = GetFieldIndex("ItemLotID1");
        }
    }
}
