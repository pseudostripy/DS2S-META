using DS2S_META.Randomizer;
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
        private int indReinforce { get; set; }
        private int indGenRegist { get; set; }
        public int _itemlotID;
        private int _genRegistID;
        
        // Properties:
        public int ItemLotID
        {
            get => _itemlotID;
            set
            {
                _itemlotID = value;
                WriteAt(indReinforce, BitConverter.GetBytes(value));
            }
        }
        public int GeneratorRegistID
        {
            get => _genRegistID;
            set
            {
                _genRegistID = value;
                WriteAt(indGenRegist, BitConverter.GetBytes(value));
            }
        }
        
        // Linked param:
        internal ItemLotRow? ItemLot => ParamMan.GetLink<ItemLotRow>(ParamMan.ItemLotChrParam, ItemLotID);
        internal GeneratorRegistRow? GeneratorRegist => ParamMan.GetGenRegistLink<GeneratorRegistRow>(GeneratorRegistID, Param.Name);

        // Constructor:
        public GeneratorParamRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            SetupIndices();
            
            // Initialise Values:
            ItemLotID = (int)ReadAt(indReinforce);
            GeneratorRegistID = (int)ReadAt(indGenRegist);
        }
        private void SetupIndices()
        {
            indGenRegist = GetFieldIndex("GeneratorRegistParam");
            indReinforce = GetFieldIndex("ItemLotID1");
        }
    }
}
