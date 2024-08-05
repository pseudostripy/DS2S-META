using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class ItemUsageRow : Param.Row
    {
        // Behind fields
        private const int DROPPABLE = 2; // 0-based
        private int indUsageBitfield { get; set; }
        private byte _UsageBitfield;

        // Properties
        public byte UsageBitfield
        {
            get => _UsageBitfield;
            set
            {
                _UsageBitfield = value;
                WriteAtField(indUsageBitfield, BitConverter.GetBytes((short)value));
            }
        }

        // Getter fields:
        public bool IsDroppable => GetBit(UsageBitfield, DROPPABLE);

        // Constructor:
        public ItemUsageRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            SetupIndices();

            // Initialise Values:
            UsageBitfield = (byte)ReadAtFieldNum(indUsageBitfield);
        }
        private void SetupIndices()
        {
            indUsageBitfield = 3; // 3rd field into def (unnamed)
        }



        private static bool GetBit(byte bitfield, int bitindex)
        {
            return ((bitfield >> bitindex) & 1) == 1;
        }
    }
}
