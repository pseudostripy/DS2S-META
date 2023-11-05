using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.ParamRows
{
    public class ArmorRow : Param.Row
    {
        // Behind-fields:
        private int indArmorID { get; set; }
        private int indSlotCategory { get; set; }
        private int indReinforceID { get; set; }
        
        public int _ArmorID;
        public byte _SlotCategory;
        public int _ReinforceID;
        
        // Properties:
        public int ArmorID
        {
            get => _ArmorID;
            set
            {
                _ArmorID = value;
                WriteAtField(indArmorID, BitConverter.GetBytes(value));
            }
        }
        public byte SlotCategory
        {
            get => _SlotCategory;
            set
            {
                _SlotCategory = value;
                WriteAtField(indSlotCategory, BitConverter.GetBytes(value));
            }
        }
        public int ReinforceID
        {
            get => _ReinforceID;
            set
            {
                _ReinforceID = value;
                WriteAtField(indArmorID, BitConverter.GetBytes(value));
            }
        }
        

        // Linked param:
        internal ArmorReinforceRow? ArmorReinforceRow => ParamMan.GetLink<ArmorReinforceRow>(ParamMan.ArmorReinforceParam, ReinforceID);
        
        // Constructor:
        public ArmorRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            SetupIndices();

            // Initialise Values:
            ArmorID = (int)ReadAtFieldNum(indArmorID);
            SlotCategory = (byte)ReadAtFieldNum(indSlotCategory);
            ReinforceID = (int)ReadAtFieldNum(indReinforceID);
        }
        private void SetupIndices()
        {
            indArmorID = GetFieldIndex("Armor ID");
            indSlotCategory = GetFieldIndex("Slot Category");
            indReinforceID = GetFieldIndex("Armor Reinforce ID");
        }

    }
}
