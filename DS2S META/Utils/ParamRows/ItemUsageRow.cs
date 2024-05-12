using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class ItemUsageRow : Param.Row
    {
        // 0-based bitfield indexers
        private const int DROPPABLE = 2;
        private const int LADDERUSAGE = 0;

        // Behind fields
        private int IndUsageBitfield { get; set; }
        private int IndLadderUsageBitfield { get; set; }
        private byte _usageBitfield;
        private byte _ladderUsageBitfield;

        // Properties
        public byte UsageBitfield
        {
            get => _usageBitfield;
            set
            {
                _usageBitfield = value;
                WriteAtField(IndUsageBitfield, value.AsByteArray());
            }
        }
        public byte LadderUsageBitfield
        {
            get => _ladderUsageBitfield;
            set
            {
                _ladderUsageBitfield = value;
                WriteAtField(IndLadderUsageBitfield, value.AsByteArray());
            }
        }

        // Getter fields:
        public bool IsDroppable => GetBit(UsageBitfield, DROPPABLE);
        public bool IsLadderUsable => GetBit(LadderUsageBitfield, LADDERUSAGE);

        // Constructor:
        public ItemUsageRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            SetupIndices();

            // Initialise Values:
            UsageBitfield = (byte)ReadAtFieldNum(IndUsageBitfield);
            LadderUsageBitfield = (byte)ReadAtFieldNum(IndLadderUsageBitfield);
        }
        private void SetupIndices()
        {
            // 0-based
            IndUsageBitfield = 3; // 3rd field into def (unnamed)
            IndLadderUsageBitfield = 2;
        }



        private static bool GetBit(byte bitfield, int bitindex)
        {
            return ((bitfield >> bitindex) & 1) == 1;
        }
    }
}
