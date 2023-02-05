using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    internal class DS2SOffsetsV103 : DS2SOffsets
    {
        public DS2SOffsetsV103()
        {
            // V1.03
            PlayerStatsOffsets = new int[] { 0x20, 0x28, 0x110, 0x70, 0xA0, 0x170 };
            
            if (Func == null)
                return;

            Func.DisplayItem = "48 8B 89 D8 00 00 00 48 85 C9 0F 85 20 5E 00 00";
            Func.ApplySpEffectAoB = "48 89 6C 24 f8 48 8d 64 24 f8 48 8D 2d 33 A7 0A 00";
        }
    }
}
